/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
 
namespace SiteBuilder
{


    /// <summary>
    /// Implements interactions with SharePoint V3.
    /// </summary>
    public class SharePointV3
    {


        /// <summary>
        /// Creates SPWeb object for the url passed
        /// </summary>
        /// <param name="webSiteURL">url to open</param>
        /// <returns>SPWeb under current user account</returns>
        public SPWeb OpenWeb(string webSiteURL)
        {
            string siteURL = CleanURL(webSiteURL);
            SPSite site = new SPSite(siteURL);
            return site.OpenWeb();
        }

        /// <summary>
        /// Detects if there is a SharePoint v3 site available at the URL provided
        /// </summary>
        /// <param name="SiteURL">URL to test</param>
        /// <param name="ErrorMessage">Exception message if the test was unsuccessful.</param>
        /// <returns>true if SharePoint v3 is found.</returns>
        public bool TestSharePointV3Site(string webSiteURL, out string errorMessage)
        {
            bool returnValue = false;
            errorMessage  = String.Empty;
            string siteURL = CleanURL(webSiteURL);
            //this will only open if the site is SharePoint services v3
            try
            {
                SPSite site = new SPSite(siteURL);
                SPWeb web = site.OpenWeb();
                //the previous two lines succeed 
                //if the site itself does not exist but its parent site does exist
                //so I check actual url property of the web                 
                if (web.Url.ToLower() == siteURL.ToLower())
                {
                    returnValue = true;
                }
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
            }
            return returnValue;

        }

        /// <summary>
        /// checks if the document library exists on the web site
        /// </summary>
        /// <param name="webSiteURL">url for the website</param>
        /// <param name="documentLibraryName">name of the document library</param>
        /// <param name="errorMessage">error message, if there was an exception</param>
        /// <returns></returns>
        public bool TestSharePointDocLibrary(string webSiteURL, string documentLibraryName, out string errorMessage)
        {
            bool returnValue = false;
            errorMessage = String.Empty;
            string siteURL = CleanURL(webSiteURL);
            try
            {
                SPWeb documentLibWeb = this.OpenWeb(webSiteURL);
                SPDocumentLibrary docLibrary = this.GetDocumentLibrary(documentLibraryName, documentLibWeb);
                returnValue = true;
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
            }
            return returnValue;            
        }

        /// <summary>
        /// Cleans URL provided of trailing slash and removes file name
        /// if URL contains it.
        /// </summary>
        /// <param name="URL">URL to be cleaned</param>        
        /// <returns>cleaned URL string</returns>
        private string CleanURL(string URL)
        {
            string siteURL = URL;
            if (siteURL.Contains("."))
            {
                //removing the file name
                siteURL = siteURL.Substring(0, siteURL.LastIndexOf("/"));
            }
            //trimming last slash in the url
            if (siteURL.EndsWith("/"))
            {
                siteURL = siteURL.Substring(0, siteURL.Length - 1);
            }
            return siteURL;
        }

        /// <summary>
        /// Creates a valid subsite Url from passes site Url and sub site name
        /// Cleans siteUrl from any trailing slashes or file names included in the Url
        /// </summary>
        /// <param name="siteUrl">site Url</param>
        /// <param name="subSiteName">Sub site name</param>
        /// <returns>subsite Url</returns>
        public string BuildSubSiteUrl(string siteUrl, string subSiteName)
        {
            return CleanURL(siteUrl + "/" + subSiteName);
        }

        /// <summary>
        /// Creates a subsite for a SharePoint v3 site
        /// </summary>
        /// <param name="ParentSiteURL">SharePoint v3 site URL</param>        
        /// <param name="SiteURLName">Subsite name</param>        
        /// <param name="SiteTitle">Subsite title</param>        
        /// <param name="SiteDescription">Subsite description</param>
        /// <param name="SiteLCID">Sibsite LCID</param>
        /// <param name="OverwriteIfExists">if true, the existing subsite with the same name will be deleted and created again.</param>
        /// <param name="OutNewSiteURL">the method will return the full URL of the subsite</param>
        /// <param name="ResultText">the method will return text log of the operations performed.</param>
        /// <returns>true if the subsite is successfully created.</returns>
        public bool CreateSite(string parentSiteURL, string siteURLName, string siteTitle, 
            string siteDescription, uint siteLCID, bool overwriteIfExists, 
            ref string outNewSiteURL, ref string resultText)
        {
            bool result = false;
            //string testResult = "";
            string subSiteFullName = CleanURL(parentSiteURL) + "/" + siteURLName;
            outNewSiteURL = subSiteFullName;
            resultText = String.Format(TextResources.AttemptingToCreateSite, subSiteFullName) + Environment.NewLine;
            //wrapping in try catch as this will be executed in a different thread
            try
            {                
                SPSite parentSite = new SPSite(parentSiteURL);
                SPWeb siteWeb = parentSite.OpenWeb();
                SPWebCollection subSites = siteWeb.Webs;
                string currentTemplate = siteWeb.WebTemplate;
                string siteTestStatus;
                if (TestSharePointV3Site(subSiteFullName, out siteTestStatus))
                {
                    //site already exists
                    resultText += TextResources.SiteExists + System.Environment.NewLine;
                    if (overwriteIfExists)
                    {
                        //delete site
                        SPUserCollection siteUsers = siteWeb.SiteUsers;
                        //emptying site users collection
                        int numUsers = siteUsers.Count;
                        for (int userIndex = numUsers; userIndex == 1; userIndex--)
                        {
                            if (!(parentSite.Owner.Sid == siteWeb.SiteUsers[userIndex].Sid))
                            {
                                siteWeb.SiteUsers.Remove(userIndex);
                            }
                        }
                        subSites.Delete(siteURLName);
                    }
                    else
                    {
                        //dont need to overwrite, exiting
                        result = true;
                        return result;
                    }
                }

                subSites.Add(siteURLName, siteTitle, siteDescription, siteLCID, currentTemplate, true, false);
                string siteTestResult;
                if (TestSharePointV3Site(subSiteFullName, out siteTestResult))
                {
                    result = true;
                    resultText += TextResources.SiteCreated + System.Environment.NewLine;
                }
            }
            catch(System.Exception ex)
            {
                resultText += ex.Message + System.Environment.NewLine;
            }
            return result;
        }

        /// <summary>
        /// Assigns users to a SharePoint v3 site according to permissions specified in the User object for every user
        /// </summary>
        /// <param name="SiteURL">SharePoint v3 site URL</param>
        /// <param name="Users">Users to be added to the site</param>
        /// <param name="Log">The method will return log of the operations performed.</param>
        public void AssignUsersToSite(string siteURL, CS4UserCollection users, ref string log)
        {
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                try
                {
                    AddSiteUser(users.Item(userIndex));
                    log += String.Format(TextResources.UserAddedToSite, users.Item(userIndex).UserLoginWithDomain, siteURL ) + Environment.NewLine;
                }
                catch (System.Exception ex)
                {
                    log += String.Format(TextResources.CantAddUserToSite, users.Item(userIndex).UserLoginWithDomain , siteURL ) + ex.Message + Environment.NewLine;
                }
            }            
        }

        /// <summary>
        /// Returns SPDocumentLibrary object by library name
        /// </summary>
        /// <param name="documentLibraryName">document library name</param>
        /// <param name="documentLibWeb">web site to query for the library</param>
        /// <returns>SPDocumentLibrary</returns>
        public SPDocumentLibrary GetDocumentLibrary(string documentLibraryName, SPWeb documentLibWeb)
        {
            SPListCollection lists = documentLibWeb.Lists;
            SPDocumentLibrary docLibrary = (SPDocumentLibrary)lists[documentLibraryName];
            return docLibrary;
        }


        /// <summary>
        /// Adds a user to specified roles on the website
        /// </summary>
        /// <param name="SiteWeb">site to which the user will be assigned</param>
        /// <param name="SiteUser">user to be added. Make sure its Roles collection contains some roles otherwise the user will not be added.</param>
        private void AddSiteUser(CS4User siteUser)
        {
            string siteUrl = System.String.Empty;
            SPSite site = null; 
            SPWeb siteWeb = null;
            SPUser spUser = null;
            foreach (CS4User.UserPermission userPermission in siteUser.UserRoles)
            {
                if (siteUrl != userPermission.WebUrl)
                {
                    siteUrl = userPermission.WebUrl;                    
                    site = new SPSite(siteUrl);
                    siteWeb = site.OpenWeb();
                    try
                    {
                        spUser = siteWeb.SiteUsers[siteUser.UserLoginWithDomain];
                    }
                    catch
                    {
                        //user has not been added to this site yet
                        siteWeb.Users.Add(siteUser.UserLoginWithDomain, siteUser.Email, siteUser.UserName, "");
                        spUser = siteWeb.SiteUsers[siteUser.UserLoginWithDomain];
                    }

                }
                //siteWeb.Roles[userPermission.Role].AddUser(siteUser.UserLoginWithDomain, siteUser.Email, siteUser.UserName, "");
                SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser);
                SPRoleDefinition roleDefinition = siteWeb.RoleDefinitions[userPermission.Role];
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                siteWeb.RoleAssignments.Add(roleAssignment);

            }
        }

        /// <summary>
        /// Adds users to SharePoint group. If user does not belong to the site, it will be added.
        /// </summary>
        /// <param name="SiteURL">A SharePoint site where the group resides</param>
        /// <param name="GroupName">SharePoint group name</param>
        /// <param name="Users">List of users to be added</param>
        /// <param name="Log">Method returns a log of operations performed</param>
        public void AssignUsersToGroup(string siteURL, string groupName, CS4UserCollection users, ref string log)
        {
            SPSite site = null;
            SPWeb siteWeb = null;
            SPGroup group = null;
            try
            {
                site = new SPSite(siteURL);
                siteWeb = site.OpenWeb();
                group = siteWeb.SiteGroups[groupName];
            }
            catch (System.Exception ex)
            {
                log += String.Format(TextResources.CantAddUsersToGroup, groupName) + ex.Message + Environment.NewLine;
                return;
            }
            //ensure user has access to the site
            for (int userIndex = 0; userIndex < users.Count; userIndex++)
            {
                SPUser user = null;
                bool createUser = false;
                try
                {
                    user = siteWeb.SiteUsers[users.Item(userIndex).UserLoginWithDomain];
                }
                catch
                {
                    //user does not exist
                    createUser = true;
                }
                try
                {
                    if (createUser)
                    {
                        AddSiteUser(users.Item(userIndex));
                        user = siteWeb.SiteUsers[users.Item(userIndex).UserLoginWithDomain];
                    }
                    group.AddUser(user);
                    log += String.Format(TextResources.UserAddedToGroup, users.Item(userIndex).UserLoginWithDomain, groupName) + Environment.NewLine;
                }
                catch (System.Exception ex)
                {
                    log += String.Format(TextResources.CantAddUserToGroup, users.Item(userIndex).UserLoginWithDomain, groupName ) + ex.Message + Environment.NewLine;
                }

            }          

        }

        /// <summary>
        /// Adds SharePoint group to SharePoint v3 site
        /// </summary>
        /// <param name="SiteURL">SharePoint v3 site URL</param>
        /// <param name="GroupName">Group to be added</param>
        /// <param name="GroupOwner">Group owner. If the user does not belong to site it will be added.</param>
        /// <param name="DefaultUser">Default group user. If the user does not belong to site it will be added.</param>
        /// <param name="OverwriteIfExists">If set, the group will be removed and created again, if it exists.</param>
        /// <param name="ResultText">The method returns log of operations performed.</param>
        /// <returns>true if the group is successfully added.</returns>
        public bool CreateUsersGroup(string siteURL, string groupName, CS4User groupOwner, 
                CS4User defaultUser, bool overwriteIfExists, ref string resultText)
        {
            bool result = false;
            resultText = String.Format(TextResources.AttemptingToCreateGroup, groupName) + Environment.NewLine;
            SPSite site = null;
            SPWeb siteWeb = null;
            SPGroupCollection groups = null;
            try
            {
                site = new SPSite(siteURL);
                siteWeb = site.OpenWeb();
                groups = siteWeb.SiteGroups;
            }                        
            catch (System.Exception ex)
            {
                resultText += TextResources.CantCreateGroup + ex.Message + System.Environment.NewLine;
                return result;
            }
            SPUser user = null;
            bool createDefaultUser = false;
            try
            {
                user = siteWeb.SiteUsers[defaultUser.UserLoginWithDomain];
            }
            catch
            {
                //user does not exist
                createDefaultUser = true;
            }
            SPMember owner = null;
            bool createGroupOwner = false;
            try
            {
                owner = siteWeb.SiteUsers[groupOwner.UserLoginWithDomain];
            }
            catch
            {
                createGroupOwner = true;
            }
            try
            {
                if (createDefaultUser)
                {
                    AddSiteUser(defaultUser);
                    user = siteWeb.SiteUsers[defaultUser.UserLoginWithDomain];
                }
                if (createGroupOwner)
                {
                    AddSiteUser(groupOwner);
                    owner = siteWeb.SiteUsers[groupOwner.UserLoginWithDomain];
                }
            }
            catch (System.Exception ex)
            {
                resultText += TextResources.NoGroupOwnerOrDefaultUser + ex.Message + Environment.NewLine;
                return result;
            }
            SPGroup group = null;
            bool groupExists = false;
            try
            {
                group = groups[groupName];
                groupExists = true;
                resultText += TextResources.GroupExists + Environment.NewLine;
            }
            catch
            {
                //group does not exist               
            }
            try
            {
                if (groupExists)
                {                    
                    if (overwriteIfExists)
                    {
                        SPUserCollection groupUsers = group.Users;
                        //emptying group users collection
                        foreach (SPUser groupUser in groupUsers)
                        {
                            group.RemoveUser(groupUser);
                        }
                        groups.Remove(groupName);
                    }
                    else
                    {
                        result = true;
                        return result;
                    }
                }
                groups.Add(groupName, owner, user, String.Empty);
                resultText += TextResources.GroupCreated + System.Environment.NewLine;
                result = true;
            }
            catch (System.Exception ex)
            {
                resultText += ex.Message;
            }
            return result;
        }
    }
}
