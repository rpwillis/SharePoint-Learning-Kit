using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using Microsoft.Build.BuildEngine;
using SharePointLearningKit.Services;

namespace Microsoft.Education.Services
{
    /// <summary>
    /// This application auto-configures IIS to add an Application Virtual Directory for the current directory.  This is used to enable
    /// Grava Services to be easily deployed by unzipping the Services.Zip into a directory and then executing this file once.
    /// When this has been run once, there is no need to run it again, future deployments can simply unzip and go.
    /// </summary>
    static class Program
    {
        private const string _IISHost = "IIS://localhost";
        private static string _exeDirectory;

        [STAThread]
        static void Main()
        {
            if (!DetectDotNetDependency())
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(Strings.DotNet35NotDetected);
                Console.ResetColor();
                return;
            }

            _exeDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            #pragma warning disable 0618 // The alternative API will not work for this scenario
            // Override the .NET Assembly loader to find the Content Service assembly in the bin directory so we don't have to duplicate it
            AppDomain.CurrentDomain.AppendPrivatePath(Path.Combine(_exeDirectory, @"bin\"));
            #pragma warning restore 0618

            try
            {
                CreateApplicationVDir(Strings.ApplicationName);
                AddContentDirectoryToSettings();
                UpdateClickOnceManifests();
                GenerateCreateLessonStub();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(Strings.IISApplicationCreated);
            Console.ResetColor();
        }

        /// <summary>
        /// Find the path to the SharePoint root by examining the comment fields of the web sites.
        /// </summary>
        static string FindSharepointRootPath()
        {
            DirectoryEntry webServer = new DirectoryEntry(_IISHost + "/W3SVC");
            foreach (DirectoryEntry de in webServer.Children)
            {
                if (de.SchemaClassName == "IIsWebServer")
                {
                    if (de.Properties["ServerComment"][0].ToString() == Strings._SharePointServerComment)
                    {
                        return String.Format(CultureInfo.CurrentCulture, _IISHost + "/W3SVC/{0}/Root", de.Name); // Substitute the Sharepoint web name into the rootPath
                    }
                }
            }

            throw new Exception(String.Format(CultureInfo.CurrentCulture, Strings.ChangedTheDefaultSharePointComment, Strings._SharePointServerComment));
        }

        /// <summary>
        /// Detect whether System.ServiceModel.Web from .NET 3.5 can be found -- this is a dependency of our services
        /// </summary>
        /// <returns>true if found</returns>
        static bool DetectDotNetDependency()
        {
            Console.WriteLine(Strings.CheckingForDotNet35);
            try
            {
                Assembly dependency = Assembly.Load("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create an Application VDir for this directory and set its properties to allow the execution of WCF Services
        /// </summary>
        static void CreateApplicationVDir(string applicationName)
        {
            DirectoryEntry site = new DirectoryEntry(FindSharepointRootPath()); // BUGBUG: Do we want to hardcode this installer to just SharePoint?
            string className = site.SchemaClassName.ToString();

            DirectoryEntries vdirs = site.Children;

            #region Delete old Application if it exists
            foreach (DirectoryEntry de in vdirs)
            {
                if (de.Name == applicationName)
                {
                    Console.WriteLine(Strings.DeletingExistingApplication, de.Name);
                    de.DeleteTree(); // Out with the old, in with the new!
                }
            }
            #endregion

            #region Add the new Application
            Console.WriteLine(Strings.AddingNewApplication, applicationName);

            DirectoryEntry vdir = vdirs.Add(applicationName, className);
            vdir.Properties["AppFriendlyName"][0] = applicationName;

            string currentPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            vdir.Properties["Path"][0] = currentPath;                               // Set this from the currently executing directory
            vdir.Properties["AccessScript"][0] = true;                              // Allow scripts
            vdir.Properties["AuthFlags"][0] = "1";                                  // Turn on Anonymous authentication
            vdir.Invoke("AppCreate3", new object[] { 0, "SLK Services", true });    // Make this an Application vdir running in process in a new AppPool named "SLK Services"

            vdir.CommitChanges();
            #endregion

            #region Fix up the ScriptMap setting that SharePoint removes which breaks WCF Services
            vdir.RefreshCache(); // Load the default properties that were inherited when the Application was committed
            foreach (PropertyValueCollection pvc in vdir.Properties)
            {
                if (pvc.PropertyName == "ScriptMaps") // Update the ScriptMap to add back in .svc (WCF Service) support
                {
                    List<object> values = new List<object>(pvc.Value as object[]);
                    bool found = false;

                    foreach (object value in values)
                    {
                        if ((value as string).StartsWith(".svc"))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine(Strings.AddingSvcMapping);
                        values.Add(Strings._SvcScriptMapping);
                        pvc.Value = values.ToArray();
                        vdir.CommitChanges();
                    }
                }
            }
            vdir.Close();
            #endregion

            #region Add the MimeType mapping for .grproj
            PropertyValueCollection mimeMapCollection = site.Properties["MimeMap"]; // Can I just do this on the vdir I wonder...?
            foreach (IISOle.IISMimeType mimeType in mimeMapCollection)
            {
                if (mimeType.Extension == ".grproj")
                {
                    mimeMapCollection.Remove(mimeType);
                    break;
                }
            }

            IISOle.MimeMapClass mimeMapping = new IISOle.MimeMapClass();
            mimeMapping.Extension = ".grproj";
            mimeMapping.MimeType = "x-ms-gravaproject";
            mimeMapCollection.Add(mimeMapping);
            site.CommitChanges();
            #endregion
        }

        /// <summary>
        /// Adds the content directory to the web.config appsettings
        /// </summary>
        static void AddContentDirectoryToSettings()
        {
            FileInfo fiApp = new FileInfo(Assembly.GetExecutingAssembly().Location);
            FileInfo fiConfig = new FileInfo(Path.Combine(fiApp.DirectoryName, "web.config"));
            Debug.Assert(fiConfig.Exists, "web.config does not exist in the local directory");

            Console.WriteLine(Strings.ModifyingWebConfig, fiApp.Directory.FullName);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fiConfig.FullName);

            XmlNode contentPathNode = xmlDocument.SelectSingleNode(Strings._ContentXPath);
            contentPathNode.InnerText = fiApp.Directory.FullName + Strings.ContentDirectory;
            
            xmlDocument.Save(fiConfig.FullName);
        }

        /// <summary>
        /// Returns the server name to be used in URLs
        /// </summary>
        static string GetServerName()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).HostName;
        }

        /// <summary>
        /// Build ClickOnce installer files for this server using the Player and Authoring binaries found in PlayerBin and AuthoringBin
        /// </summary>
        static void UpdateClickOnceManifests()
        {
            Engine engine = new Engine();
            engine.OnlyLogCriticalEvents = true;
            engine.RegisterLogger(new ConsoleLogger());

            if (Directory.Exists(_exeDirectory + @"\PlayerBin\")) // Only generate ClickOnce for Player if PlayerBin is found
            {
                Console.WriteLine(Strings.UpdatingPlayerClickOnceManifests, GetServerName());

                Project playerProject = new Project(engine);
                playerProject.Load(_exeDirectory + @"\PublishGravaPlayer.build");

                playerProject.SetProperty("SourceFolder", _exeDirectory + @"\PlayerBin\");
                playerProject.SetProperty("OutputFolder", _exeDirectory + @"\Player\");
                playerProject.SetProperty("ManifestCertificateKeyFile", _exeDirectory + @"\ClickOnce.pfx");
                playerProject.SetProperty("DeploymentUrl", "http://" + GetServerName() + "/Services/Player/Player.application");

                if (!engine.BuildProject(playerProject))
                    throw new Exception("Build of Player ClickOnce failed");
            }

            if (Directory.Exists(_exeDirectory + @"\AuthoringBin\")) // Only generate ClickOnce for Authoring if AuthoringBin is found
            {
                Console.WriteLine(Strings.UpdatingAuthoringClickOnceManifests, GetServerName());

                Project authoringProject = new Project(engine);
                authoringProject.Load(_exeDirectory + @"\PublishGravaAuthoring.build");

                authoringProject.SetProperty("SourceFolder", _exeDirectory + @"\AuthoringBin\");
                authoringProject.SetProperty("OutputFolder", _exeDirectory + @"\Authoring\");
                authoringProject.SetProperty("ManifestCertificateKeyFile", _exeDirectory + @"\ClickOnce.pfx");
                authoringProject.SetProperty("DeploymentUrl", "http://" + GetServerName() + "/Services/Authoring/Authoring.application");

                if (!engine.BuildProject(authoringProject))
                    throw new Exception("Build of Authoring ClickOnce failed");
            }
        }

        /// <summary>
        /// Generate CreateLesson.html file which is added to Document Libraries to enable the creation of Grava Lessons
        /// </summary>
        static void GenerateCreateLessonStub()
        {
            if (Directory.Exists(_exeDirectory + @"\AuthoringBin\")) // Only generate CreateLesson if Authoring is installed
            {
                Console.WriteLine(Strings.GeneratingCreateLessonStub, GetServerName());

                using (Stream htmlStream = new MemoryStream())
                {
                    string launchUrl = "/Services/Authoring/Authoring.application?" +
                        "DocumentLibrary=\" + documentLibraryUrl + \"" +
                        "&New=/Services/NewLesson.grproj";

                    string redirectHtml = ContentService.GenerateRedirectStub("Create new lesson", "Grava Authoring", launchUrl);

                    string servicesDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
                    using (FileStream fs = File.OpenWrite(Path.Combine(servicesDirectory, "CreateLesson.html")))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(redirectHtml);
                            sw.Flush();
                        }
                    }
                }
            }
        }
    }
}