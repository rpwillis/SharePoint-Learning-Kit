/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// ProvisionFromExcel.cs
//
// This is SharePoint Learning Kit sample code that compiles into a console application.  You can
// compile this application using Visual Studio 2005, or you can compile and run this application
// without Visual Studio installed by double-clicking CompileAndRun.bat.
//
// This sample code is located in Samples\SLK\ProvisionFromExcel within SLK-SDK-n.n.nnn-ENU.zip.
//
// This application executes commands for provisioning SharePoint and SLK from a Microsoft Excel
// XML Spreadsheet, such as the provided sample ProvisionData.xls.
//
// The spreadsheet file name can be specified on the command line.  If it's not, the default is
// ProvisionData.xls in the same directory as AddToProvisionDatas.exe.  There must be a worksheet
// (i.e. a tab in Excel) named "Provisioning Data".
//

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Win32;
using Microsoft.LearningComponents;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePointLearningKit;

class Program
{
    /// <summary>
    /// Directory entries that correspond to objects that are children of the root computer object.
    /// </summary>
    static DirectoryEntries s_computerChildren;

    /// <summary>
    /// The directory entry that corresponds to the "Guests" local machine group.
    /// </summary>
    static DirectoryEntry s_guests;

    static void Main(string[] args)
    {
        Stack<IDisposable> disposer = new Stack<IDisposable>();
        int rowNumber = 0;
        try
        {
            // load the XML spreadsheet into memory
            string path;
            if (args.Length == 1)
                path = args[0];
            else
            {
                path = Path.Combine(Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location), "ProvisionData.xls");
            }
            List<List<string>> rows = LoadXmlSpreadsheet(path, "Provisioning Data");

            // initialize <s_computerChildren> and <s_guests>, used by CreateUser
            DirectoryEntry computer = new DirectoryEntry(
                String.Format("WinNT://{0},computer", Environment.MachineName));
            disposer.Push(computer);
            s_computerChildren = computer.Children;
            s_guests = s_computerChildren.Find("Guests", "group");
            disposer.Push(s_guests);

            // process each row of the spreadsheet
            foreach (List<string> row in rows)
            {
                // keep track of the row number for feedback purposes
                rowNumber++;

                // skip blank rows and rows starting with ";" (comments)
                if ((row.Count == 0) || row[0].StartsWith(";"))
                    continue;

                // to simplify debugging debugging, command "PauseAndQuit" pauses and then quits
                if (row[0] == "PauseAndQuit")
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    break;
                }

                // process the row
                Console.WriteLine("-- Row {0} of {1} --", rowNumber, rows.Count);
                switch (row[0])
                {
                case "AddManagedPath":
                    ExpectCells(row, 3);
                    AddManagedPath(row[1], row[2]);
                    break;
                case "CreateSiteCollection":
                    ExpectCells(row, 3);
                    CreateSiteCollection(row[1], row[2]);
                    break;
                case "ConfigureSiteCollectionForSlk":
                    ExpectCells(row, 4);
                    ConfigureSiteCollectionForSlk(row[1], row[2], row[3]);
                    break;
                case "CreateWebSite":
                    ExpectCells(row, 4);
                    CreateWebSite(row[1], row[2], row[3]);
                    break;
                case "ActivateFeatureOnWebSite":
                    ExpectCells(row, 3);
                    ActivateFeatureOnWebSite(row[1], row[2]);
                    break;
                case "CreateUser":
                    ExpectCells(row, 4);
                    CreateUser(row[1], row[2], row[3]);
                    break;
                case "AddUserToSiteCollection":
                    ExpectCells(row, 4);
                    AddUserToSiteCollection(row[1], row[2], row[3]);
                    GrantUserAccess(row[1], "Read", row[3]);
                    break;
                case "GrantUserAccess":
                    ExpectCells(row, 4);
                    GrantUserAccess(row[1], row[2], row[3]);
                    break;
                case "AddToUserWebList":
                    ExpectCells(row, 4);
                    AddToUserWebList(row[1], row[2], row[3]);
                    break;
                default:
                    throw new Exception(String.Format("Unknown command: {0}", row[0]));
                }
            }

            Console.WriteLine("Done.");
        }
#if !DEBUG // catch exceptions in Visual Studio during development
        catch (Exception ex)
        {
            if (rowNumber > 0)
                Console.WriteLine("Error: Row {0}: {1}", rowNumber, ex.Message);
            else
                Console.WriteLine("Error: {0}", ex.Message);
        }
#endif
        finally
        {
            // dispose of objects used by this method
            while (disposer.Count > 0)
                disposer.Pop().Dispose();
        }
    }

    /// <summary>
    /// Loads into memory one worksheet of an Excel spreadsheet file that's saved in "XML
    /// Spreadsheet" format.
    /// </summary>
    ///
    /// <param name="fileName">The name of the file to load.</param>
    ///
    /// <param name="worksheetName">The name of the worksheet (tab in Excel) to load.</param>
    ///
    /// <returns>
    /// A list of rows, each containing one string per cell.
    /// </returns>
    ///
    static List<List<string>> LoadXmlSpreadsheet(string fileName, string worksheetName)
    {
        // load the XML spreadsheet into memory
        XPathNavigator rootNode;
        using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite))
        {
            rootNode = new XPathDocument(stream).CreateNavigator();
        }

        // create a namespace manager for accessing the XML spreadsheet
        string ssNamespace = "urn:schemas-microsoft-com:office:spreadsheet";
        XmlNamespaceManager nsm = new XmlNamespaceManager(rootNode.NameTable);
        nsm.AddNamespace("ss", ssNamespace);

        // set <worksheetNode> to the worksheet named <worksheetName>
        XPathNavigator worksheetNode = rootNode.SelectSingleNode(
            String.Format("ss:Workbook/ss:Worksheet[@ss:Name = '{0}']", worksheetName), nsm);
        if (worksheetNode == null)
            throw new Exception(String.Format("Worksheet not found: {0}", worksheetName));

        // loop once for each row in the worksheet; accumulate the collection of worksheet rows
        // in <rows>
        List<List<string>> rows = new List<List<string>>();
        foreach (XPathNavigator rowNode in worksheetNode.Select("ss:Table/ss:Row", nsm))
        {
            // if this row specifies an "Index" attribute, insert blank rows
            if (rowNode.MoveToAttribute("Index", ssNamespace))
            {
                while (rows.Count < rowNode.ValueAsInt - 1)
                    rows.Add(new List<string>());
                rowNode.MoveToParent();
            }

            // set <cells> to the cells of this row
            List<string> cells = new List<string>();
            foreach (XPathNavigator cellNode in rowNode.Select("ss:Cell", nsm))
            {
                if (cellNode.MoveToAttribute("Index", ssNamespace))
                {
                    while (cells.Count < cellNode.ValueAsInt - 1)
                        cells.Add(String.Empty);
                    cellNode.MoveToParent();
                }
                XPathNavigator dataNode = cellNode.SelectSingleNode("ss:Data", nsm);
                if (dataNode != null)
                    cells.Add(dataNode.Value.Trim());
            }

            // trim trailing blank cells
            while ((cells.Count > 0) && (cells[cells.Count - 1].Length == 0))
                cells.RemoveAt(cells.Count - 1);

            // set <cells> to be the cells of this row
            rows.Add(cells);
        }

        return rows;
    }

    /// <summary>
    /// Throws an exception if a given spreadsheet row doesn't contain a given number of cells.
    /// </summary>
    ///
    /// <param name="row">The row to examine.</param>
    ///
    /// <param name="count">The expected number of cells in the row.</param>
    ///
    static void ExpectCells(List<string> row, int count)
    {
        if (row.Count != count)
        {
            throw new Exception(String.Format("Expecting {0} cells, found {1} cells",
                count, row.Count));
        }
    }

    /// <summary>
    /// Creates a local machine account.  The account is not created if it already exists.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "SlkLearner123".  The name may start with
    ///     ".\", indicating that it's a local machine account.</param>
    /// 
    /// <param name="fullName">The full name, e.g. "SLK Sample Learner 123".  Not used if the
    ///     account already exists.</param>
    /// 
    /// <param name="password">The password for the new account.  Not used if the account already
    ///     exists.</param>
    ///
    static void CreateUser(string loginName, string fullName, string password)
    {
        // add the user as a local user of this computer; set <existed> to true if the user
        // already existed
        Console.WriteLine("Finding or creating user account \"{0}\"", loginName);
        if (loginName.StartsWith(@".\"))
            loginName = loginName.Substring(2);
        DirectoryEntry user;
        bool existed;
        try
        {
            user = s_computerChildren.Find(loginName, "user");
            existed = true;
            Console.WriteLine("...exists already");
        }
        catch (COMException)
        {
            user = s_computerChildren.Add(loginName, "user");
            existed = false;
        }

        using (user)
        {
            // if the user didn't exist, set up their account
            if (!existed)
            {
                // set properties of the user
                user.Invoke("SetPassword", new object[] { password });
                user.Invoke("Put", new object[] { "FullName", fullName });
                user.Invoke("Put", new object[] { "Description",
                    "* Created by SharePoint Learning Kit sample code *" });
                user.CommitChanges();

                // add the user to the Guests group
                try
                {
                    s_guests.Invoke("Add", new object[] { user.Path });
                }
                catch (TargetInvocationException)
                {
                    // probably the user is already a member of the group
                }
            }

#if false
            // add the user to SharePoint
            string domainName = String.Format(@"{0}\{1}", s_parentWeb.Site.HostName, loginName);
            s_parentWeb.SiteUsers.Add(domainName, String.Empty, fullName, String.Empty);
#endif
        }

        if (!existed)
            Console.WriteLine("...created");
    }

    /// <summary>
    /// Adds a "wildcard inclusion" managed path to a given SharePoint Web application.  This is
    /// the same functionality you can access using the "Define Managed Paths" page in SharePoint
    /// Central Administration.
    /// </summary>
    ///
    /// <param name="webApplicationUrl">The URL of the web application.</param>
    ///
    /// <param name="relativeUrl">The relative URL of the inclusion, e.g. "sites".</param>
    ///
    static void AddManagedPath(string webApplicationUrl, string relativeUrl)
    {
        Console.WriteLine("Finding or creating managed path \"{0}\" under \"{1}\"", relativeUrl,
            webApplicationUrl);
        SPWebApplication webApp = SPWebApplication.Lookup(new Uri(webApplicationUrl));
        if (webApp.Prefixes.Contains(relativeUrl))
            Console.WriteLine("...exists already");
        else
        {
            webApp.Prefixes.Add(relativeUrl, SPPrefixType.WildcardInclusion);
            Console.WriteLine("...created");
        }
    }

    /// <summary>
    /// Creates a SharePoint site collection, if it doesn't already exist.
    /// </summary>
    ///
    /// <param name="siteCollectionUrl">The URL of the site collection to create.</param>
    ///
    /// <param name="title">The title of the root Web site, of the new site collection, e.g.
    ///     "SLK Sample Web Site".  Not used if the site collection exists already.</param>
    ///
    static void CreateSiteCollection(string siteCollectionUrl, string title)
    {
        Console.WriteLine("Finding or creating site collection \"{0}\"", siteCollectionUrl);

        // set <loginName> to the "domain\login-name" of the current user
        string loginName;
        using (WindowsIdentity currentUser = WindowsIdentity.GetCurrent())
            loginName = currentUser.Name;

        // quit if the site collection already exists
        try
        {
            using (SPSite spSite = new SPSite(siteCollectionUrl))
            {
                // site collection exists -- quit
                Console.WriteLine("...exists already");
                return;
            }
        }
        catch (FileNotFoundException)
        {
            // site collection doesn't exist -- create it below
        }

        // create the site collection
        SPWebApplication webApp = SPWebApplication.Lookup(new Uri(siteCollectionUrl));
        using (SPSite spSite = webApp.Sites.Add(siteCollectionUrl, loginName, String.Empty))
        {
            using (SPWeb spWeb = spSite.RootWeb)
            {
                spWeb.Title = title;
                spWeb.Update();
            }
            Console.WriteLine("...created");
        }
    }

    /// <summary>
    /// Activates a feature on a given SharePoint Web site, if that feature is not yet active.
    /// </summary>
    ///
    /// <param name="webSiteUrl">The URL of the Web site.</param>
    ///
    /// <param name="featureName">The name of the feature.</param>
    ///
    static void ActivateFeatureOnWebSite(string webSiteUrl, string featureName)
    {
        Console.WriteLine("Activating feature \"{0}\" on Web site \"{1}\"", featureName,
            webSiteUrl);
        using (SPSite spSite = new SPSite(webSiteUrl))
        {
            SPFeatureDefinition featureDefinition =
                spSite.WebApplication.Farm.FeatureDefinitions[featureName];
            if (featureDefinition == null)
                throw new Exception(String.Format("Feature \"{0}\" not found", featureName));
            using (SPWeb spWeb = spSite.OpenWeb())
            {
                SPFeature feature = spWeb.Features[featureDefinition.Id];
                if (feature != null)
                    Console.WriteLine("...exists already");
                else
                {
                    spWeb.Features.Add(featureDefinition.Id);
                    Console.WriteLine("...created");
                }
            }
        }
    }
 
    /// <summary>
    /// Configures a SharePoint site collection for use with SLK.
    /// </summary>
    ///
    /// <param name="siteCollectionUrl">The URL of the site collection to configure.</param>
    ///
    static void ConfigureSiteCollectionForSlk(string siteCollectionUrl, string databaseServer,
        string databaseName)
    {
        Console.WriteLine("Configuring site collection \"{0}\" for use with SLK",
            siteCollectionUrl);

        // set <sharePointLocation> to the path to the installation location of SharePoint
        string sharePointLocation;
        using (RegistryKey registryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            @"Software\Microsoft\Shared Tools\Web Server Extensions\12.0"))
        {
            object value;
            if ((registryKey == null) || ((value = registryKey.GetValue("Location")) == null))
                throw new Exception("SharePoint is not installed");
            sharePointLocation = value.ToString();
        }

        // set <slkAdminLocation> to the path where SLK administration files are located
        string slkAdminLocation = Path.Combine(sharePointLocation,
            @"Template\Admin\SharePointLearningKit");
        if (!Directory.Exists(slkAdminLocation))
            throw new Exception("SharePoint Learning Kit is not installed");

        using (SPSite spSite = new SPSite(siteCollectionUrl))
        {
            // load default parameter values as needed
            string currentDatabaseServer, currentDatabaseName, instructorPermission,
                learnerPermission, observerPermission;
            bool createDatabase, createPermissions;
            bool mappingFound = SlkAdministration.LoadConfiguration(spSite.ID,
                out currentDatabaseServer, out currentDatabaseName, out createDatabase,
                out instructorPermission, out learnerPermission, out observerPermission, out createPermissions);
            if (databaseServer == "CurrentOrDefaultServer")
                databaseServer = currentDatabaseServer;
            else
            if (databaseServer == "localhost")
                databaseServer = spSite.HostName;
            if (databaseName == "CurrentOrDefaultDatabase")
                databaseName = currentDatabaseName;

            // load SlkSchema.sql, the SQL Server schema for SLK
            string schemaFileContents;
            if (createDatabase)
            {
                schemaFileContents = File.ReadAllText(
                    Path.Combine(slkAdminLocation, "SlkSchema.sql"));
                Console.WriteLine("...database doesn't exist -- creating it");
            }
            else
            {
                schemaFileContents = null; // don't create a database -- it already exists
                Console.WriteLine("...database exists already");
            }

            // load SlkSettings.xml, the default SLK Settings
            string settingsFileContents = File.ReadAllText(
                Path.Combine(slkAdminLocation, "SlkSettings.xml.dat"));

            // save the SLK configuration for this SPSite
            SlkAdministration.SaveConfiguration(spSite.ID, databaseServer, databaseName,
                schemaFileContents, instructorPermission, learnerPermission, observerPermission, createPermissions,
                null, settingsFileContents, spSite.WebApplication.ApplicationPool.Username,
                ImpersonationBehavior.UseOriginalIdentity);
        }
    }

    /// <summary>
    /// Creates a SharePoint Web site (SPWeb).  The Web site is not created if it already exists.
    /// </summary>
    /// 
    /// <param name="parentWebSiteUrl">The URL of the parent of the Web site to create.</param>
    ///
    /// <param name="relativeUrl">The relative URL of the SPWeb, e.g. "SlkSampleWeb123".</param>
    /// 
    /// <param name="title">The title of the new Web site, e.g. "SLK Sample Web Site".  Not used
    ///     if the site collection exists already.</param>
    ///
    static void CreateWebSite(string parentWebSiteUrl, string relativeUrl, string title)
    {
        Console.WriteLine("Finding or creating Web site \"{0}\" under \"{1}\"", relativeUrl,
            parentWebSiteUrl);
        using (SPSite spSite = new SPSite(parentWebSiteUrl))
        {
            using (SPWeb parentWeb = spSite.OpenWeb())
            {
                // if the Web site with name <relativeUrl> already exists, do nothing
                SPWeb spWeb;
                try
                {
                    using (spWeb = parentWeb.Webs[relativeUrl])
                    {
                        if (!spWeb.Exists)
                            throw new System.IO.FileNotFoundException();
                        Console.WriteLine("...exists already");
                        return;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    // Web site doesn't exist -- create it below
                }

                // create the Web site
                using (spWeb = parentWeb.Webs.Add(relativeUrl, title, String.Empty,
                    (uint) parentWeb.Locale.LCID, "STS", true, false))
                {
                    // add the new Web site to the "quick launch" navigation area of the parent
                    // Web site
                    SPNavigationNode node = new SPNavigationNode(spWeb.Title, relativeUrl);
                    if (parentWeb.Navigation.QuickLaunch != null)
                        parentWeb.Navigation.QuickLaunch.AddAsLast(node);
                }
                Console.WriteLine("...created");
            }
        }
    }

    /// <summary>
    /// Adds a user to a SharePoint site collection, if they don't already exist.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "MyDomain\SlkLearner123".  If the login name
    ///     starts with ".\", it's assumed to be a local machine account.</param>
    /// 
    /// <param name="fullName">The full name, e.g. "SLK Sample Learner 123".  Not used if the
    ///     user is already in the site collection.</param>
    /// 
    /// <param name="siteCollectionUrl">The URL of the site collection to add the user to.</param>
    ///
    static void AddUserToSiteCollection(string loginName, string fullName, string siteCollectionUrl)
    {
        Console.WriteLine("Adding user \"{0}\" to SharePoint site collection \"{1}\"", loginName,
            siteCollectionUrl);
        using (SPSite spSite = new SPSite(siteCollectionUrl))
        {
            if (loginName.StartsWith(@".\"))
                loginName = String.Format(@"{0}\{1}", spSite.HostName, loginName.Substring(2));
            using (SPWeb rootWeb = spSite.RootWeb)
                rootWeb.SiteUsers.Add(loginName, String.Empty, fullName, String.Empty);
        }
    }

    /// <summary>
    /// Creates one or more role assignments for a given user on a given SharePoint Web site.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "MyDomain\SlkLearner123".  If the login name
    ///     starts with ".\", it's assumed to be a local machine account.</param>
    ///
    /// <param name="permissions">A comma-separated list of permission names to use for the role
    ///     assignments, "Contribute,SLK Instructor".</param>
    ///
    /// <param name="webSiteUrl">The URL of the Web site.</param>
    ///
    static void GrantUserAccess(string loginName, string permissions, string webSiteUrl)
    {
        Console.WriteLine("Giving \"{0}\" permission(s) \"{1}\" on \"{2}\"", loginName,
            permissions, webSiteUrl);
        using (SPSite spSite = new SPSite(webSiteUrl))
        {
            using (SPWeb spWeb = spSite.OpenWeb())
            {
                if (loginName.StartsWith(@".\"))
                    loginName = String.Format(@"{0}\{1}", spSite.HostName, loginName.Substring(2));
                SPUser spUser = spWeb.SiteUsers[loginName];
                foreach (string substring in permissions.Split(','))
                {
                    string permission = substring.Trim();
                    SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser);
                    SPRoleDefinition roleDefinition;
                    try
                    {
                        roleDefinition = spWeb.RoleDefinitions[permission];
                    }
                    catch (SPException)
                    {
                        throw new Exception(String.Format(
                            "Cannot access permission \"{0}\"", permission));
                    }
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                    spWeb.RoleAssignments.Add(roleAssignment);
                }
            }
        }
    }

    /// <summary>
    /// Adds a SharePoint Web site to the SLK user web list of a given user.  A user web list is
    /// the list of Web sites shown in E-Learning Actions pages that are displayed within a given
    /// site collection.
    /// </summary>
    ///
    /// <param name="loginName">The login name, e.g. "MyDomain\SlkLearner123".  If the login name
    ///     starts with ".\", it's assumed to be a local machine account.</param>
    ///
    /// <param name="siteCollectionUrl">The URL of the site collection containing the user web
    ///     list to update.</param>
    ///
    /// <param name="webSiteUrl">The URL of the Web site to add to the user web list.</param>
    ///
    static void AddToUserWebList(string loginName, string siteCollectionUrl, string webSiteUrl)
    {
        Console.WriteLine("Adding \"{0}\" to the user web list of \"{1}\" in \"{2}\"", webSiteUrl,
            loginName, siteCollectionUrl);

        // "log in" to SharePoint as the user running this program
        using (SPSite currentUserSite = new SPSite(siteCollectionUrl))
        {
            if (loginName.StartsWith(@".\"))
            {
                loginName = String.Format(@"{0}\{1}", currentUserSite.HostName, loginName.Substring(2));
            }

            SPWeb rootWeb = currentUserSite.RootWeb;
            // set <spUser> to the user corresponding to <loginName>
            SPUser spUser = rootWeb.AllUsers[loginName];

            // "log in" to SharePoint as the user <spUser>, and set <slkStore> to refer to that
            // user and the site collection specified by <siteCollectionUrl>
            using (SPSite destinationSite = new SPSite(webSiteUrl, spUser.UserToken))
            {
                using (SPWeb destinationWeb = destinationSite.OpenWeb())
                {
                    SlkStore slkStore = SlkStore.GetStore(destinationWeb);
                    slkStore.AddToUserWebList(destinationWeb);
                }
            }
        }
    }
}
