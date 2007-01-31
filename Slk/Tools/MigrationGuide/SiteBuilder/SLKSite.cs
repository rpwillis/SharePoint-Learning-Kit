/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using Microsoft.SharePointLearningKit;
using Microsoft.SharePoint;
using MigrationHelper;
using System.Collections;
using Microsoft.LearningComponents.SharePoint;
using System.Xml.XPath;



namespace SiteBuilder
{
    /// <summary>
    /// Performs creating of the SLK sites and groups and users and assignments transfer
    /// </summary>
    public class SLKSite
    {
        /// <summary>
        /// accesses the document library at the path provided, 
        /// loops through the learning packages
        /// located the library and reads the package Id from package manifest
        /// </summary>
        /// <param name="siteURL">web site url</param>
        /// <param name="documentLibraryName">document library name</param>
        /// <returns></returns>
        public Hashtable GetAllLearningResources(string siteURL, string documentLibraryName)
        {
            Hashtable resources = new Hashtable();
            SharePointV3 sharepoint = new SharePointV3();
            SPWeb documentLibWeb = sharepoint.OpenWeb(siteURL);
            SPDocumentLibrary docLibrary = sharepoint.GetDocumentLibrary(documentLibraryName, documentLibWeb);
            Microsoft.SharePointLearningKit.SlkStore store = SlkStore.GetStore(documentLibWeb);
            foreach (SPListItem item in docLibrary.Items)
            {
                //if this list item is a file
                if (item.File != null)
                {
                    SharePointFileLocation fileLocation = new SharePointFileLocation(documentLibWeb, item.UniqueId, item.File.UIVersion);
                    string location = fileLocation.ToString();
                    try
                    {
                        PackageInformation info = store.GetPackageInformation(location);
                        XPathNavigator manifestNavigator = info.ManifestReader.CreateNavigator();
                        Guid packageIdentifier = ManifestParser.ParseIndexXml(manifestNavigator);
                        if (packageIdentifier == Guid.Empty)
                        {
                            packageIdentifier = ManifestParser.ParseImsManifestXml(manifestNavigator);
                        }
                        if (packageIdentifier != Guid.Empty)
                        {
                            resources.Add(packageIdentifier.ToString(), location);
                        }
                    }
                    catch 
                    {
                        //not a valid learning package, do nothing
                    }
                }
            }
            return resources;
        }


        /// <summary>
        /// Parses XML file, processes Class Server information class by class.
        /// Works from a non-UI thread using BackgroundWorker. Handles all exceptions.
        /// Reports progress.
        /// Reads data for one class at a time and calls ProcessClass to create class, 
        /// its groups and users
        /// </summary> 
        /// <param name="XMLFilePath">Full path of the XML file to be parsed</param>
        /// <param name="LogFilePath">File name of the log file, it will be created in working directory</param>
        /// <param name="worker">BackgroundWorker object for passing status</param>
        /// <param name="e">DoWorkEventArgs (not used)</param>
        /// <returns>true if the file was successfully parsed. Any errors encountered while creating sites will be just written to the log file and will not affect this result</returns>

        public bool CreateSitesFromXML(string xmlFilePath, string logFilePath, 
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool status = false;
            try
            {
                LogFile log = new LogFile(logFilePath);
                ConfigXMLFileReader configReader = new ConfigXMLFileReader(xmlFilePath);
                CS4Class nextClass = configReader.GetNextClass();
                while (nextClass != null)
                {
                    worker.ReportProgress(0, TextResources.ProcessingClass + ": " + nextClass.ClassWeb);
                    string logText = String.Empty;
                    ProcessClass(nextClass, ref logText);
                    log.WriteToLogFile(logText);
                    nextClass = configReader.GetNextClass();
                }
                configReader.Dispose();
                log.FinishLogging();
                status = true;
            }
            catch (System.Exception ex)
            {
                worker.ReportProgress(0, TextResources.AnError + ex.Message);
            }

            return status;
        }

        /// <summary>
        /// Does the actual work of creating an SLK site with groups and users.
        /// </summary>
        /// <param name="classData">Class to be created</param>
        /// <param name="processingLog">Method returns log of actions performed</param>
        private void ProcessClass(CS4Class classData, ref string processingLog)
        {
            string log = System.String.Empty;
            SharePointV3 SLKweb = new SharePointV3();
            
            CS4UserCollection classUsers = new CS4UserCollection();
            CS4UserCollection groupUsers = new CS4UserCollection();
            bool classAdded = false;
            string classURL = String.Empty;
            string classCreateResult = String.Empty;
            CS4User groupsOwner = null;
            
            log += System.Environment.NewLine + TextResources.ProcessingClass + classData.ClassName + System.Environment.NewLine;

            if (classData.Transfer)
            {
                classAdded = SLKweb.CreateSite(
                                    SiteBuilder.Default.SLKSchoolWeb,
                                    classData.ClassWeb,
                                    classData.ClassName,
                                    String.Empty,
                                    classData.ClassLCID,
                                    classData.Overwrite,
                                    ref classURL,
                                    ref classCreateResult);
                log += classCreateResult;
                if (classAdded)
                {
                    //adding site users
                    for (int classUserIndex = 0; classUserIndex < classData.Users.Count; classUserIndex++)
                    {
                        CS4User user = classData.Users.Item(classUserIndex);
                        if (user.Transfer)
                        {                            
                            user.UserRoles = this.DefineUserRoles(user.IsTeacher, classURL);
                            classUsers.Add(user);
                            //if the user is teacher, set it as group owner
                            //we only take first teacher to be groups owner as we only need one
                            if ((groupsOwner == null) && (user.IsTeacher))
                            {
                                groupsOwner = user;
                            }
                        }
                        else
                        {
                            log += string.Format(TextResources.UserNotForTransfer, user.UserLoginWithDomain) + System.Environment.NewLine;
                        }
                    }
                    string addUsersLog = String.Empty;
                    SLKweb.AssignUsersToSite(classURL, classUsers, ref addUsersLog);
                    log += addUsersLog;
                    //adding groups
                    for (int groupIndex = 0; groupIndex < classData.Groups.Count; groupIndex++)
                    {
                        log += string.Format(TextResources.ProcessingGroup,classData.Groups.Item(groupIndex).WebName) + System.Environment.NewLine;
                        if (classData.Groups.Item(groupIndex).Transfer)
                        {
                            //processing group users
                            groupUsers = new CS4UserCollection();
                            for (int groupUserIndex = 0; groupUserIndex < classData.Groups.Item(groupIndex).GroupUsers.Count; groupUserIndex++)
                            {
                                CS4User groupUser = classData.Groups.Item(groupIndex).GroupUsers.Item(groupUserIndex);
                                if (groupUser.Transfer)
                                {
                                    groupUser.UserRoles = this.DefineUserRoles(groupUser.IsTeacher, classURL);
                                    groupUsers.Add(groupUser);
                                }
                                else
                                {
                                    log += string.Format(TextResources.GroupUserNotForTransfer, groupUser.UserLoginWithDomain) + System.Environment.NewLine;
                                }
                            }
                            //adding group
                            //only if we have a group owner and at least one group user
                            if ((groupUsers.Count > 0) && (groupsOwner != null))
                            {
                                //taking first user as default user
                                bool groupAdded = false;
                                string groupCreateResult = string.Empty;
                                groupAdded = SLKweb.CreateUsersGroup(
                                            classURL,
                                            classData.Groups.Item(groupIndex).WebName,
                                            groupsOwner,
                                            classData.Groups.Item(groupIndex).GroupUsers.Item(0),
                                            classData.Groups.Item(groupIndex).Overwrite,
                                            ref groupCreateResult);
                                //adding group users
                                log += groupCreateResult;
                                if (groupAdded)
                                {

                                    string addGroupUsersLog = String.Empty;
                                    SLKweb.AssignUsersToGroup(classURL, classData.Groups.Item(groupIndex).WebName, groupUsers, ref addGroupUsersLog);
                                    log += addGroupUsersLog;
                                }
                            }
                            else
                            {
                                //not transferring this group
                                log += TextResources.GroupOwnerOrDefaultUserProblem + System.Environment.NewLine;
                            }
                        }
                        else
                        {
                            log += TextResources.GroupNotForTransfer + System.Environment.NewLine;
                        }

                    }

                }
            }
            else
            {
                log += TextResources.ClassNotForTransfer + System.Environment.NewLine;
            }
            log += String.Format(TextResources.FinishedProcessingClass, classData.ClassName) + System.Environment.NewLine;
            processingLog = log;               
        }


        /// <summary>
        /// Contains logic for determining which SLK roles should be assigned to the user
        /// depending on its Class Server role.
        /// </summary>
        /// <param name="userRole">Class Server user role</param>
        /// <param name="classWebUrl">Class in which the user holds this role</param>
        /// <returns>List of SLK roles</returns>
        private List<CS4User.UserPermission> DefineUserRoles(bool isTeacher, string classWebUrl)
        {
            List<CS4User.UserPermission> userRoles = new List<CS4User.UserPermission>();
            if (isTeacher)
            {
                //teacher gets the following permissions:
                //- read and contribute on school site
                userRoles.Add(new CS4User.UserPermission(SiteBuilder.Default.SLKSchoolWeb, SiteBuilder.Default.ContributeSharePointRole));
                userRoles.Add(new CS4User.UserPermission(SiteBuilder.Default.SLKSchoolWeb, SiteBuilder.Default.ReadSharePointRole));
                //- read on library site
                userRoles.Add(new CS4User.UserPermission(SiteBuilder.Default.SLKDocLibraryWeb, SiteBuilder.Default.ReadSharePointRole));
                //- contribute, SLK Instructor on class site
                userRoles.Add(new CS4User.UserPermission(classWebUrl, SiteBuilder.Default.ContributeSharePointRole));
                userRoles.Add(new CS4User.UserPermission(classWebUrl, SiteBuilder.Default.SLKInstructorSharePointRole));
            }
            else
            {
                //student gets the following permissions:
                //- read on school site
                userRoles.Add(new CS4User.UserPermission(SiteBuilder.Default.SLKSchoolWeb, SiteBuilder.Default.ReadSharePointRole));
                //- read, SLK Learner on class site
                userRoles.Add(new CS4User.UserPermission(classWebUrl, SiteBuilder.Default.ReadSharePointRole));
                userRoles.Add(new CS4User.UserPermission(classWebUrl, SiteBuilder.Default.SLKLearnerSharePointRole));
            }
            return userRoles;
        }

    }

 

}
