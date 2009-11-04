using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

using System.IO;
using System.Reflection;
using Microsoft.Win32;

using System.Configuration;
using System.Data;
using System.Web;

using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using System.Collections.ObjectModel;
using Microsoft.SharePoint;

using System.Diagnostics;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
//using Microsoft.SharePointLearningKit;
using System.Threading;
//using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics.CodeAnalysis;
//using Resources.Properties;
using InstallerHelper;

using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection.Emit;

using Microsoft.SharePoint.Library;

namespace SolutionsAndFeaturesTest
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 3)
            {
                Program prog = new Program();
                try
                {
                    SPSite site = new SPSite(args[2].ToString());
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    return 1;
                }

                string featureId = "00057005-c978-11da-ba52-00042350e42e"; //"00057005-c978-11da-ba52-00042350e42f";
                bool res = true;
                res = prog.CheckSLKVersion(args[0].ToString(), featureId); //Check the actual version of SLK
                if (res)
                {
                    res = prog.CheckDNFVersion(args[1].ToString()); //Check the actual version of .NET Framework
                    if (!res)
                    {
                        return 1;
                    }
                }
                else if (!res)
                {
                    return 1;
                }

                string solutionName = @"course_manager_configure.wsp";
                string[] paths = { args[2] };
                prog.AddSolution(@"Files\", solutionName, paths);
                /*prog.CopyJS();
                prog.DeploySolution(@"Solution\", solutionName, paths);
                prog.ActivateFeature(@"Solution\", solutionName, paths, featureId);
                solutionName = @"coursemanagerwebparts.cab";
                prog.DeploySolution(@"WebPart\", solutionName, paths);*/
                return 0;
            }
            else
            {
                System.Console.WriteLine(InstallerResources.strErrorParameters);
                return 1;
            }

                /*string solutionName = @"course_manager_configure.wsp";
                string[] paths = { args[1] };
                string featureId = "00057005-c978-11da-ba52-00042350e42f";
                prog.CopyJS();
                prog.DeploySolution(@"Solution\", solutionName, paths);
                prog.ActivateFeature(@"Solution\", solutionName, paths, featureId);
                solutionName = @"coursemanagerwebparts.cab";
                prog.DeploySolution(@"WebPart\", solutionName, paths);
                return 0;*/

        }

        private void CopyJS()
        {
            try
            {
                System.Console.WriteLine(InstallerResources.strCopyingDocumentSelector);
                File.Copy("SPDocumentSelectorIFrame.js",
                    "c:\\program files\\common files\\microsoft shared\\web server extensions\\12\\TEMPLATE\\LAYOUTS\\1033\\SPDocumentSelectorIFrame.js");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message+"\r\n");
            }
        }

        public static string GetFeatureVersionById(string featureId, string scope)
        {
            Guid id = new Guid(featureId);
            SPFeatureDefinition fd;

            if (!String.IsNullOrEmpty(scope) && scope.ToLower() == "site")
            {
                foreach (SPFeature f in SPContext.Current.Site.Features)
                {
                    fd = f.Definition;
                    if (fd.Id.Equals(id))
                        return fd.Version.ToString();
                }
            }
            else
            {
                foreach (SPFeature f in SPContext.Current.Web.Features)
                {
                    fd = f.Definition;
                    if (fd.Id.Equals(id))
                        return fd.Version.ToString();
                }
            }

            return null;
        } 

        /// <summary>
        /// Returns TRUE if the SLK version is compatible
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private Boolean CheckSLKVersion(string version, string featureId)
        {
            System.Console.WriteLine(InstallerResources.strCheckingSLKVersion);

            //System.Console.WriteLine("Checking SLk Version " + version + " ... \r\n");
            try
            {
                //Ver1
                /*if (SPFarm.Local.Solutions["sharepointlearningkit.wsp"] != null)
                {
                    SPSolution definition = SPFarm.Local.Solutions["sharepointlearningkit.wsp"];
                    System.Console.WriteLine("\r\nSolution ID: "+definition.Id + "\r\nSolution Version: " +
                        definition.Version + "\r\nSolution Name: " + definition.Name);
                    if (definition.Version == long.Parse(version))
                    {
                        return true;
                    }
                    else
                    {
                        System.Console.WriteLine("ERROR: SLK Version " + version + " is not installed");
                        return false;
                    }
                }
                else if (SPFarm.Local.Solutions["sharepointlearningkit.wsp"] == null)
                    System.Console.WriteLine("ERROR: SLK Version " + version + " is not installed");
                return false;*/

                //Ver2
                System.Reflection.Assembly asm = System.Reflection.Assembly.Load(new AssemblyName(
                    "Microsoft.SharePointLearningKit"));
                if (asm.GlobalAssemblyCache)
                {
                    asm = System.Reflection.Assembly.Load(
                    "Microsoft.SharePointLearningKit, Version=" + version +
                    ", Culture=neutral, PublicKeyToken=24e5ae139825747e");

                    //Get methods from the SLK Version 1.3.0.2
                    Type paramType = Type.GetType("Microsoft.LearningComponents.Storage.LearningStoreItemIdentifier, Microsoft.LearningComponents.Storage, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e");
                    Type[] Parameters = new Type[] { paramType };
                    Type MyType = Type.GetType(
                        "Microsoft.SharePointLearningKit.SlkStore, Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e",true,true);
                    System.Reflection.MethodInfo methodInfo = MyType.GetMethod("GetLearnerAssignmentGuidId",
                        Parameters);
                    System.Reflection.MethodInfo methodInfo2 = MyType.GetMethod("GetLearnerAssignmentProperties",
                        new Type[] { typeof(System.Guid),
                            Type.GetType("Microsoft.SharePointLearningKit.SlkRole, Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e") });
                    System.Reflection.MethodInfo methodInfo3 = MyType.GetMethod("SetFinalPoints",
                        new Type[] { typeof(System.Guid),
                            typeof(float) });

                    if ( (methodInfo == null) || (methodInfo2 == null) || (methodInfo3 == null) )
                    {
                        System.Console.WriteLine(InstallerResources.strErrorSLKNotInstaller);
                        return false;
                    }
                }
                else
                {
                    System.Console.WriteLine(InstallerResources.strErrorSLKNotInstaller);
                    return false;
                }
                System.Console.WriteLine(InstallerResources.strSLKInstalled);
                return true;

                //Ver3
                /*foreach(SPFeatureDefinition FDef in SPFarm.Local.FeatureDefinitions)
                {
                    System.Console.WriteLine("\r\nFeature Name: " + FDef.Name + "\r\nFeature ID: " + FDef.Id +
                        "\r\nFeature Version: " + FDef.Version);
                    if (FDef.Id.Equals(new Guid(featureId)))
                    {
                        if (FDef.Version.ToString().Equals(version))
                        {
                            System.Console.WriteLine("SLK Version "+version+" found");
                            //return true;
                        }
                    }
                }
                return false;*/

                //Ver3
                /*Guid id = new Guid(featureId);
                SPFeatureDefinition fd;
                System.Console.WriteLine("Checking SPContext.Current.Site.Features");
                foreach (SPFeature f in SPContext.Current.Site.Features)
                {
                    System.Console.WriteLine("SPContext.Current.Site.Features checked");
                    fd = f.Definition;
                    System.Console.WriteLine(fd.Version.ToString());
                    if (fd.Id.Equals(id))
                    {
                        if (fd.Version.ToString().Equals(version))
                        {
                        }
                            //return true;
                    }
                }
                return false;*/

            }
            catch (FileLoadException)
            {
                Console.WriteLine(InstallerResources.strErrorSLKVersionNotFound);
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine(InstallerResources.strErrorSLKVersionNotFound);
                return false;
            }
        }

        /// <summary>
        /// Returns TRUE if the .Net Framework version is compatible
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private Boolean CheckDNFVersion(string version)
        {
            System.Console.WriteLine(InstallerResources.strCheckingNetVersion + version + InstallerResources.strGoingNewLine);
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.Load(
                    "Microsoft.Build.Framework, Version=" + version +
                    ", Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                System.Console.WriteLine(InstallerResources.strNETVersion + version + InstallerResources.strIsInstalled);
                if(asm.GlobalAssemblyCache)
                    return true;
                return false;

            }
            catch (FileLoadException)
            {
                Console.WriteLine(InstallerResources.strErrorNetFrameworkVersionNotFound);
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine(InstallerResources.strErrorNetFrameworkVersionNotFound);
                return false;
            }
        }

        private bool ValidateCourseManager(string name)
        {
            foreach (SPSolution definition in SPFarm.Local.Solutions)
            {
                if (definition.Name.Equals(name))
                    return true;
            }
            return false;
        }

        #region Add/DeploySolution
        private void AddSolution(string path, string name, string[] url)
        {
            if (SPFarm.Local.Solutions[name] == null)
            {
                SPSolution solution = SPFarm.Local.Solutions.Add(path + name);
            }
        }

        private void DeploySolution(string path, string name, string[] url)
        {
            System.Console.WriteLine(InstallerResources.strDeploying + name + InstallerResources.strGoingNewLine);
            SPSolution solution = new SPSolution();
            //Add the solution
            if (!ValidateCourseManager(name))
            {
                solution = SPFarm.Local.Solutions.Add(path + name);
            }
            else
            {
                solution = SPFarm.Local.Solutions[name];
            }

            Collection<SPWebApplication> deployedWebapps = solution.DeployedWebApplications;
            Collection<SPWebApplication> webapps = new Collection<SPWebApplication>(); //New webapplications were the solution is going to be deployed
            foreach (string singleUrl in url)
            {
                SPWebApplication webapp = SPWebApplication.Lookup(new Uri(singleUrl));
                if (!deployedWebapps.Contains(webapp))
                {
                    webapps.Add(webapp);
                }
                else
                {
                    System.Console.WriteLine(InstallerResources.strSolutionAlreadyDeployed + singleUrl + "\r\n");
                }
            }

            if (webapps.Count > 0)
            {
                //Deploy the solution
                solution.Deploy(DateTime.Now, true, webapps, true);
                while (solution.Deployed == false)
                {
                }
            }
        }

        private void ActivateFeature(string path, string name, string[] url, string id)
        {
            System.Console.WriteLine(InstallerResources.strActivating + name + InstallerResources.strFeature);
            foreach (string singleUrl in url)
            {
                SPWeb web = new SPSite(singleUrl).OpenWeb();
                SPFeatureCollection featureCollect = web.Features;
                featureCollect.Add(new Guid(id), true);
            }
        }
        #endregion

    }


}
