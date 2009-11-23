using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents.Storage;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Class for creating the folders and subfolders under the DropBox document library in both modes Create and Edit.
    /// </summary>
    public class DropBoxManager
    {
        public DropBoxManager()
        {
        }

        public void CreateAssignmentFolderForCourseManagerInstructor(string docLibTitle, AssignmentProperties assignmentProperties, SlkMemberships slkMembers, string assignmentFolderName)
        {
            SPListItem newFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = null;

                        try
                        {
                            assDropBox = spWeb.Lists[docLibTitle];
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(AppResources.SLKFeatureNotActivated, ex.InnerException);
                        }

                        //Get the folder if it exists 
                        newFolder = GetAssignmentFolder(assDropBox, assignmentFolderName);

                        //Create the assignment folder if it does not exist
                        if (newFolder == null)
                        {
                            newFolder = CreateAssignmentFolder(docLibTitle, assignmentProperties,
                                                    slkMembers, assignmentFolderName);

                            if (newFolder != null)
                            {
                                //Create a Subfolder for each learner
                                CreateLearnersSubFolders(newFolder, assignmentProperties, slkMembers);
                            }
                        }
                        else
                        {
                            //Give Instructor read access permission over the Document Library
                            ApplyInstructorReadAccessOnDocLib(assDropBox, spWeb);

                            //give instructor permission over the assignmnet folder
                            spWeb.AllowUnsafeUpdates = true;

                            ApplyInstructorsReadAccessPermissions(newFolder, slkMembers, assignmentProperties);

                            foreach (SPFolder subFolder in newFolder.Folder.SubFolders)
                            {
                               if ((!subFolder.Item.DoesUserHavePermissions(SPContext.Current.Web.CurrentUser, SPBasePermissions.EditListItems)) &&
                                   (!subFolder.Item.DoesUserHavePermissions(SPContext.Current.Web.CurrentUser, SPBasePermissions.ViewListItems)))
                               {
                                    if (subFolder.Files.Count == 0)
                                    {
                                        ApplyInstructorsReadAccessPermissions(subFolder.Item, slkMembers, assignmentProperties);
                                    }
                                    else
                                    {
                                        foreach (SlkUser instructor in assignmentProperties.Instructors)
                                        {
                                            SPUser user = GetSPInstructor(slkMembers, instructor);
                                            ApplyPermissionToFolder(subFolder.Item, user, SPRoleType.Reader);
                                            ApplyPermissionToFolder(subFolder.Item, user, SPRoleType.Contributor);
                                        }
                                    }
                               }
                            }

                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
        }

        public SPListItem CreateAssignmentSubFolderForCourseManagerLearnerAssCollect(AssignmentProperties assignmentProperties, SPListItem newFolder, string subFolderName)
        {
            SPListItem assSubFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        spWeb.AllowUnsafeUpdates = true;

                        assSubFolder = CreateSubFolder(newFolder.Folder, subFolderName);

                        assSubFolder.BreakRoleInheritance(true);
                        spWeb.AllowUnsafeUpdates = true;

                        foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                        {
                            role.RoleDefinitionBindings.RemoveAll();
                        }

                        assSubFolder.Web.AllowUnsafeUpdates = true;
                        assSubFolder.Update();
                        assSubFolder.Web.AllowUnsafeUpdates = false;

                        ApplyPermissionToFolder(newFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);
                        ApplyPermissionToFolder(assSubFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);

                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });

            return assSubFolder;
        }

        public SPListItem CreateAssignmentSubFolderForCourseManagerLearner(AssignmentProperties assignmentProperties, SPListItem newFolder)
        {
            SPListItem assSubFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        // Check whether a folder with the same name exists or not
                        string subFolderName = SPContext.Current.Web.CurrentUser.Name;

                        if (string.IsNullOrEmpty(subFolderName))
                        {
                            subFolderName = SPContext.Current.Web.CurrentUser.LoginName;
                        }

                        spWeb.AllowUnsafeUpdates = true;

                        assSubFolder = CreateSubFolder(newFolder.Folder, subFolderName);

                        assSubFolder.BreakRoleInheritance(true);
                        spWeb.AllowUnsafeUpdates = true;

                        foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                        {
                            role.RoleDefinitionBindings.RemoveAll();
                        }

                        assSubFolder.Web.AllowUnsafeUpdates = true;
                        assSubFolder.Update();
                        assSubFolder.Web.AllowUnsafeUpdates = false;

                        ApplyPermissionToFolder(newFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);
                        ApplyPermissionToFolder(assSubFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);

                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });

            return assSubFolder;
        }

        public SPListItem CreateAssignmentFolderForCourseManagerLearner(string docLibTitle, AssignmentProperties assignmentProperties, string assignmentFolderName)
        {
            string url;
            SPListItem newFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = null;

                        try
                        {
                            assDropBox = spWeb.Lists[docLibTitle];
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(AppResources.SLKFeatureNotActivated, ex.InnerException);
                        }

                        //Get the folder if it exists 
                        newFolder = GetAssignmentFolder(assDropBox, assignmentFolderName);

                        //Create the assignment folder if it does not exist
                        if (newFolder == null)
                        {
                            url = assDropBox.RootFolder.ServerRelativeUrl;
                            spWeb.AllowUnsafeUpdates = true;
                            newFolder = assDropBox.Items.Add(url, SPFileSystemObjectType.Folder, assignmentFolderName);
                            newFolder.Update();
                            assDropBox.Update();
                            newFolder.BreakRoleInheritance(true);
                            spWeb.AllowUnsafeUpdates = true;

                            foreach (SPRoleAssignment role in newFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            newFolder.Update();

                            spWeb.AllowUnsafeUpdates = false;
                        }
                       
                        // Check whether a folder with the same name exists or not
                        string subFolderName = SPContext.Current.Web.CurrentUser.Name;

                        if(string.IsNullOrEmpty(subFolderName))
                        {
                            subFolderName = SPContext.Current.Web.CurrentUser.LoginName;
                        }

                        SPListItem assSubFolder = GetSubFolder(newFolder, subFolderName);

                        spWeb.AllowUnsafeUpdates = true;
                        if (assSubFolder == null)
                        {
                            assSubFolder = CreateSubFolder(newFolder.Folder, subFolderName);
                        }

                        assSubFolder.BreakRoleInheritance(true);
                        spWeb.AllowUnsafeUpdates = true;

                        foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                        {
                            role.RoleDefinitionBindings.RemoveAll();
                        }
                        assSubFolder.Update();

                        ApplyPermissionToFolder(newFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);
                        ApplyPermissionToFolder(assSubFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);

                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });

            return newFolder;
        }

        /// <summary>
        /// Creates an assignment folder.
        /// </summary>
        /// <param name="docLibName">The drop box document library title</param>
        /// <param name="assignmentProperties">The current assignment properties</param>
        /// <param name="slkMembers">All members on the site Instructors, Learners and LearnerGroups</param>
        /// <returns>The name of the created folder</returns>
        public SPListItem CreateAssignmentFolder(string docLibTitle, AssignmentProperties assignmentProperties, SlkMemberships slkMembers, string assignmentFolderName)
        {
            string url;
            SPListItem newFolder = null;
            
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = null;
                        try
                        {
                            assDropBox = spWeb.Lists[docLibTitle];
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(AppResources.SLKFeatureNotActivated, ex.InnerException);
                        }

                        //Give Instructor read access permission over the Document Library
                        ApplyInstructorReadAccessOnDocLib(assDropBox, spWeb);

                        //Get the folder if it exists 
                        newFolder = GetAssignmentFolder(assDropBox, assignmentFolderName);

                        //Create the assignment folder if it does not exist
                        if (newFolder == null)
                        {
                            url = assDropBox.RootFolder.ServerRelativeUrl;
                            spWeb.AllowUnsafeUpdates = true;
                            newFolder = assDropBox.Items.Add(url, SPFileSystemObjectType.Folder, assignmentFolderName);
                            newFolder.Update();
                            assDropBox.Update();
                            newFolder.BreakRoleInheritance(true);
                            spWeb.AllowUnsafeUpdates = true;

                            foreach (SPRoleAssignment role in newFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            newFolder.Update();

                            ApplyInstructorsReadAccessPermissions(newFolder, slkMembers, assignmentProperties);
                            spWeb.AllowUnsafeUpdates = false;
                        }
                        else
                        {
                            newFolder = null;
                        }                       
                    }
                }
            });
            return newFolder;
        }

        /// <summary>
        /// Creates an assignment folder.
        /// </summary>
        /// <param name="docLibName">The drop box document library title</param>
        /// <param name="assignmentProperties">The current assignment properties</param>
        /// <param name="slkMembers">All members on the site Instructors, Learners and LearnerGroups</param>
        /// <returns>The name of the created folder</returns>
        public SPListItem CreateSelfAssignmentFolder(string docLibTitle, AssignmentProperties assignmentProperties, string assignmentFolderName)
        {
            string url;
            SPListItem newFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = spWeb.Lists[docLibTitle];

                        //Give Instructor read access permission over the Document Library
                        ApplyInstructorReadAccessOnDocLib(assDropBox, spWeb);
                        
                        //Get the folder if it exists 
                        newFolder = GetAssignmentFolder(assDropBox, assignmentFolderName);

                        //Create the assignment folder if it does not exist
                        if (newFolder == null)
                        {
                            url = assDropBox.RootFolder.ServerRelativeUrl;
                            spWeb.AllowUnsafeUpdates = true;
                            newFolder = assDropBox.Items.Add(url, SPFileSystemObjectType.Folder, assignmentFolderName);
                            newFolder.Update();
                            assDropBox.Update();
                            newFolder.BreakRoleInheritance(true);
                            spWeb.AllowUnsafeUpdates = true;

                            foreach (SPRoleAssignment role in newFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            newFolder.Update();

                            ApplyPermissionToFolder(newFolder, SPContext.Current.Web.CurrentUser, SPRoleType.Reader);
                            spWeb.AllowUnsafeUpdates = false;
                        }
                        else
                        {
                            newFolder = null;
                        }
                    }
                }
            });
            return newFolder;
        }

        /// <summary>
        /// Create a subfolder for each learner in this assignment, the subfolder will be named after the learner name.
        /// </summary>
        /// <param name="docLibName">The dropbox document library instance title which exists in the Instance.xml file</param>
        /// <param name="rootFolderName">The name of the folder to create the learners sub folders under</param>
        /// <param name="assignmentProperties">The properties of the current assignment</param>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        public void CreateLearnersSubFolders(SPListItem assignmentFolder, AssignmentProperties assignmentProperties, SlkMemberships slkMembers)
        {
            // create a subfolder for each assignment learner
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = spWeb.Lists[assignmentFolder.ParentList.ID];
                        SPListItem tempAssignmentFolder = assDropBox.Folders[assignmentFolder.UniqueId];
                        SPListItem assSubFolder = null;
                        SPUser spLearner = null;

                        // Get the assignment Folder
                        if (assDropBox.GetItemById(tempAssignmentFolder.ID) != null)
                        {
                            foreach (SlkUser learner in assignmentProperties.Learners)
                            {
                                spLearner = GetSPLearner(slkMembers, learner);
                                // Check whether a folder with the same name exists or not
                                assSubFolder = GetSubFolder(tempAssignmentFolder, spLearner.Name);
                                spWeb.AllowUnsafeUpdates = true;
                                if (assSubFolder == null)
                                    assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spLearner.Name);
                                else
                                    assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spLearner.LoginName);

                                //assSubFolder.BreakRoleInheritance(true);
                                spWeb.AllowUnsafeUpdates = true;

                                foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                                {
                                    role.RoleDefinitionBindings.RemoveAll();
                                }
                                assSubFolder.Update();

                                ApplyPermissionToFolder(tempAssignmentFolder, spLearner, SPRoleType.Reader);
                                ApplyPermissionToFolder(assSubFolder, spLearner, SPRoleType.Reader);


                                spWeb.AllowUnsafeUpdates = true;
                                foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                                {
                                    if (role.Member.Name != spLearner.Name)
                                    {
                                        bool isInstructor = false;
                                        foreach (SlkUser slkInstructor in assignmentProperties.Instructors)
                                        {
                                            if (role.Member.Name == GetSPInstructor(slkMembers, slkInstructor).Name)
                                            {
                                                isInstructor = true;
                                            }
                                        }
                                        if (!isInstructor)
                                        {
                                            role.RoleDefinitionBindings.RemoveAll();
                                            role.Update();
                                        }
                                    }
                                }
                                assSubFolder.Update();

                                spWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Function to create a learner subfolder, the subfolder will be named after the learner name.
        /// </summary>
        /// <param name="docLibName">The dropbox document library instance title which exists in the Instance.xml file</param>
        /// <param name="rootFolderName">The name of the folder to create the learners sub folders under</param>
        /// <param name="assignmentProperties">The properties of the current assignment</param>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        public void CreateLearnersSubFolders(SPListItem assignmentFolder, SlkUser learner, SlkMemberships slkMembers)
        {
            // create a subfolder for each assignment learner
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentFolder.Web.Site.ID))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentFolder.Web.ID))
                    {
                        SPList assDropBox = spWeb.Lists[assignmentFolder.ParentList.ID];
                        SPListItem tempAssignmentFolder = assDropBox.Folders[assignmentFolder.UniqueId];
                        SPUser spLearner = null;

                        // Get the assignment Folder
                        if (assDropBox.GetItemById(tempAssignmentFolder.ID) != null)
                        {
                            spLearner = GetSPLearner(slkMembers, learner);
                            // Check whether a folder with the same name exists or not
                            SPListItem assSubFolder = GetSubFolder(tempAssignmentFolder, learner.Name);
                            spWeb.AllowUnsafeUpdates = true;
                            if (assSubFolder == null)
                                assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spLearner.Name);
                            else
                                assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spLearner.LoginName);

                            assSubFolder.BreakRoleInheritance(true);
                            spWeb.AllowUnsafeUpdates = true;

                            foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            assSubFolder.Update();

                            ApplyPermissionToFolder(tempAssignmentFolder, spLearner, SPRoleType.Reader);
                            ApplyPermissionToFolder(assSubFolder, spLearner, SPRoleType.Reader);
                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
        } 

        /// <summary>
        /// Create a subfolder for each learner in this assignment, the subfolder will be named after the learner name.
        /// </summary>
        /// <param name="docLibName">The dropbox document library instance title which exists in the Instance.xml file</param>
        /// <param name="rootFolderName">The name of the folder to create the learners sub folders under</param>
        /// <param name="assignmentProperties">The properties of the current assignment</param>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        public void CreateSelfAssignmentSubFolder(SPListItem assignmentFolder, AssignmentProperties assignmentProperties)
        {
            // create a subfolder for each assignment learner
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = spWeb.Lists[assignmentFolder.ParentList.ID];
                        SPListItem tempAssignmentFolder = assDropBox.Folders[assignmentFolder.UniqueId];

                        // Get the assignment Folder
                        if (assDropBox.GetItemById(tempAssignmentFolder.ID) != null)
                        {
                            SPUser spInstructor = SPContext.Current.Web.CurrentUser;
                            // Check whether a folder with the same name exists or not
                            SPListItem assSubFolder = GetSubFolder(tempAssignmentFolder, spInstructor.Name);
                            spWeb.AllowUnsafeUpdates = true;
                            if (assSubFolder == null)
                                assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spInstructor.Name);
                            else
                                assSubFolder = CreateSubFolder(tempAssignmentFolder.Folder, spInstructor.LoginName);

                            assSubFolder.BreakRoleInheritance(true);
                            spWeb.AllowUnsafeUpdates = true;

                            foreach (SPRoleAssignment role in assSubFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            assSubFolder.Update();

                            ApplyPermissionToFolder(tempAssignmentFolder, spInstructor, SPRoleType.Reader);
                            ApplyPermissionToFolder(assSubFolder, spInstructor, SPRoleType.Reader);

                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Trancate the time part and the '/' from the Created Date and convert it to string to be a valid folder name.
        /// </summary>
        /// <param name="fullDate">The date as it is returned from the CreatedDate property of the assignment</param>
        /// <returns> The date converted to string and without the time and the '/' character </returns>
        public string GetDateOnly(DateTime fullDate)
        {
            return fullDate.ToShortDateString().Replace("/","");
        }

        /// <summary>
        /// Function to update the DropBox document library after the current assignment being edited
        /// </summary>
        /// <param name="newAssProb">The new assignment properties</param>
        /// <param name="SlkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        /// <param name="docLibName">The document library name</param>
        public void UpdateAssignment(AssignmentProperties oldAssignmentProperties, AssignmentProperties newAssignmentProperties,
            SlkMemberships slkMembers, string dropBoxDocLibName)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(oldAssignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(oldAssignmentProperties.SPWebGuid))
                    {
                        SPListItem oldAssignmentFolder = null, newAssignmentFolder = null, learnerSubFolder = null;
                        SPListItem tempAssignmentFolder = null, templearnerSubfolder = null;
                        SPList dropBoxDocLib = spWeb.Lists[dropBoxDocLibName];
                        SPUser spLearner = null;
                        string oldAssignmentTitle = oldAssignmentProperties.Title.Trim();
                        string newAssignmentTitle = newAssignmentProperties.Title.Trim();
                        string oldAssignmentFolderName = (oldAssignmentTitle + " " + GetDateOnly(newAssignmentProperties.DateCreated)).Trim();
                        string newAssignmentFolderName = (newAssignmentTitle + " " + GetDateOnly(oldAssignmentProperties.DateCreated)).Trim();

                        // If assignment title has been changed, create a new assignment folder and move old assignment folder contents to it
                        if (oldAssignmentFolderName.ToLower() != newAssignmentFolderName.ToLower())
                        {
                            //Get old assignment folder
                            oldAssignmentFolder = GetAssignmentFolder(dropBoxDocLib, oldAssignmentFolderName);
                            if (oldAssignmentFolder != null)
                            {
                                if (!spWeb.GetFolder(oldAssignmentFolder.Url.Replace(oldAssignmentFolderName, newAssignmentFolderName)).Exists)
                                {
                                    spWeb.AllowUnsafeUpdates = true;

                                    SPFolder folderSrc = spWeb.GetFolder(oldAssignmentFolder.Url);
                                    folderSrc.MoveTo(oldAssignmentFolder.Url.Replace(oldAssignmentFolderName, newAssignmentFolderName));

                                    spWeb.AllowUnsafeUpdates = false;
                                }
                                else
                                {
                                    throw new Exception(AppResources.AssignmentNameAlreadyUsed);
                                }
                            }
                            else
                            {
                                throw new Exception(AppResources.AssFolderNotFound);
                            }
                        }

                        // Get new assignment folder, or the old one if the title has not been changed
                        // in both cases, the value of the current assignment folder name will be stored in newAssignmentFolderName
                        newAssignmentFolder = GetAssignmentFolder(dropBoxDocLib, newAssignmentFolderName);

                        if (newAssignmentFolder != null)
                        {
                            tempAssignmentFolder = spWeb.GetFolder(newAssignmentFolder.Folder.Url).Item;

                            spWeb.AllowUnsafeUpdates = true;

                            //remove all permissions that has been granted before on the assignment folder
                            foreach (SPRoleAssignment role in tempAssignmentFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            tempAssignmentFolder.Update();

                            // Grant assignment instructors Read permission on the assignment folder
                            ApplyInstructorsReadAccessPermissions(tempAssignmentFolder, slkMembers, newAssignmentProperties);

                            // Delete subfolders of the learners who have been removed from the assignment
                            foreach (SlkUser oldLearner in oldAssignmentProperties.Learners)
                            {
                                if (!newAssignmentProperties.Learners.Contains(oldLearner.UserId))
                                {
                                    spLearner = GetSPLearner(slkMembers, oldLearner);
                                    spWeb.AllowUnsafeUpdates = true;

                                    // Get learner subfolder, and delete it if exists
                                    learnerSubFolder = GetSubFolder(newAssignmentFolder, spLearner.Name);
                                    if (learnerSubFolder == null)
                                        learnerSubFolder = GetSubFolder(newAssignmentFolder, spLearner.LoginName);
                                    if (learnerSubFolder != null)
                                    {
                                        templearnerSubfolder = spWeb.GetFolder(learnerSubFolder.Folder.Url).Item;
                                        templearnerSubfolder.Delete();
                                    }
                                    tempAssignmentFolder.Update();
                                }
                            }

                            foreach (SlkUser learner in newAssignmentProperties.Learners)
                            {
                                spLearner = GetSPLearner(slkMembers, learner);

                                // Grant assignment learners Read permission on the assignment folder
                                ApplyPermissionToFolder(tempAssignmentFolder, spLearner, SPRoleType.Reader);

                                // Get learner subfolder
                                learnerSubFolder = GetSubFolder(tempAssignmentFolder, spLearner.Name);
                                if (learnerSubFolder == null)
                                    learnerSubFolder = GetSubFolder(tempAssignmentFolder, spLearner.LoginName);
                                if (learnerSubFolder != null)
                                {
                                    templearnerSubfolder = spWeb.GetFolder(learnerSubFolder.Folder.Url).Item;

                                    spWeb.AllowUnsafeUpdates = true;

                                    //remove all permissions that has been granted before on the learner subfolder
                                    foreach (SPRoleAssignment role in templearnerSubfolder.RoleAssignments)
                                    {
                                        role.RoleDefinitionBindings.RemoveAll();
                                    }
                                    templearnerSubfolder.Update();

                                    // TODO:Update instructors and learners permissions based on the learner assignment status
                                    
                                    // Grant assignment instructors permission on the learner subfolder based on the learner assignment status
                                    ApplyInstructorsReadAccessPermissions(templearnerSubfolder, slkMembers, newAssignmentProperties);

                                    // Grant learner permission on his/her learner subfolder based on the learner assignment status
                                    ApplyPermissionToFolder(templearnerSubfolder, spLearner, SPRoleType.Reader);
                                }
                                else
                                // In case the assignment is assigned to new learner
                                {
                                    // Create a new subfolder for this learner
                                    CreateLearnersSubFolders(tempAssignmentFolder, learner, slkMembers);
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
        }
        
        /// <summary>
        /// Creates sub folder under a specific assignment folder, it is being named after the specified learner's name.
        /// </summary>
        /// <param name="rootFolderName">The assignment folder name which is the root folder to create the subfolder under</param>
        /// <param name="subFolderName">The name of the subfolder</param>
        /// <param name="assWeb">The current assignment SPWeb</param>
        /// <param name="docLibName">The document library name</param>
        public SPListItem CreateSubFolder(SPFolder assFolder, string learnerName)
        {
            assFolder.Item.Web.AllowUnsafeUpdates = true;
            SPFolder learnerSubFolder = assFolder.SubFolders.Add(learnerName);
            learnerSubFolder.Item.BreakRoleInheritance(true);
            assFolder.Item.Web.AllowUnsafeUpdates = true;
            assFolder.Update();
            assFolder.Item.Web.AllowUnsafeUpdates = false;
            return learnerSubFolder.Item;
        }

        /// <summary>
        /// Function to get the SPUser of the SLKUser
        /// </summary>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        /// <param name="slkUser">The SLKUser object of the instructor</param>
        /// <returns>The SPUser object of the specified </returns>
        public SPUser GetSPInstructor(SlkMemberships slkMembers, SlkUser slkUser)
        {
            for (int i = 0; i < slkMembers.Instructors.Count; i++)
            {
                if (slkMembers.Instructors[i].UserId.GetKey() == slkUser.UserId.GetKey())
                {
                    return slkMembers.Instructors[i].SPUser;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get the SP object of the specified SLK learner object
        /// </summary>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        /// <param name="slkUser">The SLKUser  of the specified learner object</param>
        /// <returns>The SP object of the specified SLKUser learner</returns>
        public SPUser GetSPLearner(SlkMemberships slkMembers, SlkUser slkUser)
        {
            for (int i = 0; i < slkMembers.Learners.Count; i++)
            {
                if (slkMembers.Learners[i].UserId.GetKey() == slkUser.UserId.GetKey())
                {
                    return slkMembers.Learners[i].SPUser;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the SPUser object of the specified SlkUser object
        /// </summary>
        /// <param name="slkMembers">All SlkMembers in the site, including Learners, instructors and LearnerGroups</param>
        /// <param name="slkUser">The SlkUser object</param>
        /// <returns>The SPUser object of the specified SlkUser</returns>
        public SPUser GetSPUser(SlkMemberships slkMembers, SlkUser slkUser)
        {
            for (int i = 0; i < slkMembers.Instructors.Count; i++)
            {
                if (slkMembers.Instructors[i].UserId.GetKey() == slkUser.UserId.GetKey())
                {
                    return slkMembers.Instructors[i].SPUser;
                }
            }
            for (int i = 0; i < slkMembers.Learners.Count; i++)
            {
                if (slkMembers.Learners[i].UserId.GetKey() == slkUser.UserId.GetKey())
                {
                    return slkMembers.Learners[i].SPUser;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets SlkUser from SlkMemberships by UserItemIdentifier
        /// </summary>
        /// <param name="slkMembers">All SlkMembers in the web, including Learners, instructors and LearnerGroups</param>
        /// <param name="slkUserId">The UserId of the SlkUser object of the instructor</param>
        /// <returns>SlkUser</returns>
        public SlkUser GetSlkUserById(SlkMemberships slkMembers, UserItemIdentifier slkUserId)
        {
            for (int i = 0; i < slkMembers.Instructors.Count; i++)
            {
                if (slkMembers.Instructors[i].UserId.GetKey() == slkUserId.GetKey())
                {
                    return slkMembers.Instructors[i];
                }
            }

            for (int i = 0; i < slkMembers.Learners.Count; i++)
            {
                if (slkMembers.Learners[i].UserId.GetKey() == slkUserId.GetKey())
                {
                    return slkMembers.Learners[i];
                }
            }
            return null;
        }
        
        /// <summary>
        /// Function searches for the assignment folder
        /// </summary>
        /// <param name="assDropBox">The current document library which contains the specified folder</param>
        /// <param name="folderName">The specified foler name</param>
        /// <returns>The folder object</returns>
        public SPListItem GetAssignmentFolder(SPList assDropBox, string folderName)
        {
            SPListItem assFolder = null;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(assDropBox.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(assDropBox.ParentWeb.ID))
                    {
                        SPList dropBoxList = web.Lists[assDropBox.ID];
                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + folderName + "</Value></Eq></Where>";
                        if (dropBoxList.GetItems(query).Count != 0)
                            assFolder = dropBoxList.GetItems(query)[0];
                    }
                }
            });
            return assFolder;
        }
        
        /// <summary>
        /// Function searches for the specified subfolder
        /// </summary>
        /// <param name="assDropBox">The document library which contains the assignment folder</param>
        /// <param name="assFolder">The assignment folder object which contains the subfolders</param>
        /// <param name="subFolderName"></param>
        /// <returns></returns>
        public SPListItem GetSubFolder(SPListItem assFolder, string subFolderName)
        {
            SPFolder tempFolder = null;
            SPListItem assSubFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(assFolder.ParentList.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(assFolder.ParentList.ParentWeb.ID))
                    {
                        tempFolder = web.Lists[assFolder.ParentList.ID].Folders[assFolder.UniqueId].Folder;
                        SPQuery subFolderQuery = new SPQuery();
                        subFolderQuery.Folder = tempFolder;
                        subFolderQuery.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + subFolderName + "</Value></Eq></Where>";
                        SPListItemCollection allSubFolders = tempFolder.Item.ParentList.GetItems(subFolderQuery);
                        if (allSubFolders.Count != 0)
                            assSubFolder = allSubFolders[0];
                    }
                }
            });
            return assSubFolder;
        }

        private void ApplyInstructorReadAccessOnDocLib(SPList assDropBox, SPWeb spWeb)
        {
            SPUser user = SPContext.Current.Web.CurrentUser;
            if (!assDropBox.DoesUserHavePermissions(user, SPBasePermissions.ViewListItems))
            {
                ApplyPermission(spWeb, assDropBox, user, SPRoleType.Reader, new UpdateList(assDropBox));
            }
        }

        void ApplyInstructorsReadAccessPermissions(SPListItem folder, SlkMemberships slkMembers, AssignmentProperties assignmentProperties)
        {
            foreach (SlkUser instructor in assignmentProperties.Instructors)
            {
                ApplyPermissionToFolder(folder, GetSPInstructor(slkMembers, instructor), SPRoleType.Reader);
            }
        }

        void ApplyPermissionToFolder(SPListItem folder, SPUser user, SPRoleType roleType)
        {
            ApplyPermission(folder.Web, folder, user, roleType, new UpdateListItem(folder));
        }

        void ApplyPermission(SPWeb web, ISecurableObject securable, SPUser user, SPRoleType roleType, UpdateItem updateItem)
        {
            SPRoleDefinition roleDefinition = web.RoleDefinitions.GetByType(roleType);
            SPRoleAssignment roleAssignment = new SPRoleAssignment(user);

            bool currentUnsafeUpdates = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;
            try
            {
                roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                securable.RoleAssignments.Add(roleAssignment);
                updateItem.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = currentUnsafeUpdates;
            }
        }

        public string GetSPUserName(string fullName)
        {
           return fullName.Remove(0, fullName.IndexOf("\\") + 1);
        }

        public void ResestIsLatestFiles(SPListItem learnerSubFolder)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(learnerSubFolder.ParentList.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(learnerSubFolder.ParentList.ParentWeb.ID))
                    {
                        SPList list = web.Lists[learnerSubFolder.ParentList.ID];
                        SPFolder subFolder = list.Folders[learnerSubFolder.Folder.ParentFolder.UniqueId].Folder.SubFolders[learnerSubFolder.Url];

                        web.AllowUnsafeUpdates = true;

                        foreach (SPFile file in subFolder.Files)
                        {
                            file.Item["IsLatest"] = false;
                            file.Item.Update();
                        }
                    }
                }
            });
        }

        public SPFile UploadFile(string fileName, Stream fileStream, SPFolder learnerSubFolder,
            Guid learnerAssignmentGuid, LearnerAssignmentProperties learnerAssignmentProperties)
        {
            SPFile file = null;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(learnerSubFolder.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(learnerSubFolder.ParentWeb.ID))
                    {
                        SPFolder assignmentFolder = web.Lists[learnerSubFolder.ParentListId].Folders[learnerSubFolder.ParentFolder.UniqueId].Folder;
                        SPFolder subFolder = assignmentFolder.SubFolders[learnerSubFolder.Url];

                        string fileUrl = subFolder.Url + '/' + fileName;
                        web.AllowUnsafeUpdates = true;

                        subFolder.Files.Add(fileUrl, fileStream, true);
                        file = subFolder.Files[fileUrl];
                        file.Item.Update();
                        //Set the properties of the list item
                        SetFileProperties(file, learnerAssignmentGuid, learnerAssignmentProperties);

                        // IF the assignment is auto return, the learner will still be able to view the drop box assignment files
                        // otherwise, learner permissions will be removed from the learner's subfolder in the Drop Box document library
                        if (!learnerAssignmentProperties.AutoReturn)
                        {
                            RemoveLearnerPermission(assignmentFolder, SPContext.Current.Web.CurrentUser);
                            RemoveLearnerPermission(subFolder, SPContext.Current.Web.CurrentUser);
                        }

                        // Grant instructors contribute permission on learner subfolder
                        ApplyInstructorsGradingPermission(subFolder.Item);

                        web.AllowUnsafeUpdates = false;
                    }
                }
            });
            return file;
        }

        /// <summary>
        /// Returns true if roleAssignment member is granted "Read" and "SLK Instructor" permissions on roleAssignment parent
        /// </summary>
        /// <param name="subFolderItem">learner subfolder item</param>
        private void ApplyInstructorsGradingPermission(SPListItem subFolderItem)
        {
            bool isReader = false, isInstructor = false;
            SPFolder assignmentFolder = subFolderItem.Folder.ParentFolder;
            SPWeb web = assignmentFolder.ParentWeb;

            foreach (SPRoleAssignment assignmentFolderRoleAssignment in assignmentFolder.Item.RoleAssignments)
            {
                for (int i = 0; i < assignmentFolderRoleAssignment.RoleDefinitionBindings.Count; i++)
                {
                    if (assignmentFolderRoleAssignment.RoleDefinitionBindings[i].Type == SPRoleType.Reader)
                    {
                        isReader = true;

                        foreach (SPRoleAssignment webRoleAssignment in web.RoleAssignments)
                        {
                            for (int j = 0; j < webRoleAssignment.RoleDefinitionBindings.Count; j++)
                            {
                                if (webRoleAssignment.Member.Name == assignmentFolderRoleAssignment.Member.Name &&
                                    webRoleAssignment.RoleDefinitionBindings[j].Name == SlkStore.GetStore(web).Mapping.InstructorPermission)
                                {
                                    isInstructor = true;
                                    break;
                                }
                            }
                            if (isInstructor)
                                break;
                        }
                    }
                    if (isReader && isInstructor)
                    {
                        ApplyPermissionToFolder(subFolderItem, (SPUser)assignmentFolderRoleAssignment.Member, SPRoleType.Contributor);
                        isReader = false;
                        isInstructor = false;
                        break;
                    }
                }
            }
        }

        private void SetFileProperties(SPFile file, Guid learnerAssignmentGuid, LearnerAssignmentProperties currentLearnerAssignmentProperties)
        {
            file.Item["AssignmentGUID"] = learnerAssignmentGuid.ToString();
            file.Item["LearnerID"] = currentLearnerAssignmentProperties.LearnerName;
            //Set IsLatest property to true for new submitted files
            //this is used to keep track of files submitted at the learner last assignment attempt
            file.Item["IsLatest"] = true; 
            file.Item.Update();
        }

        private void RemoveLearnerPermission(SPFolder folder, SPUser learner)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(folder.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(folder.ParentWeb.ID))
                    {
                        SPList dropBoxLib = web.Lists[folder.ParentListId];

                        // If the folder parent is the root, then this is an assignment folder
                        if (folder.ParentFolder == folder.Item.ParentList.RootFolder)
                        {
                            SPListItem assignmentFolder = dropBoxLib.Folders[folder.ParentFolder.UniqueId];
                            SPListItem subFolder = dropBoxLib.Folders[assignmentFolder.UniqueId].Folder.SubFolders[folder.Url].Item;
                            web.AllowUnsafeUpdates = true;
                            subFolder.RoleAssignments.Remove(learner);
                            assignmentFolder.RoleAssignments.Remove(learner);
                            web.AllowUnsafeUpdates = false;
                        }
                        // else, then this is a learner subfolder
                        else
                        {
                            folder.Item.Web.AllowUnsafeUpdates = true;
                            folder.Item.RoleAssignments.Remove(learner);
                            folder.Update();
                            folder.Item.Web.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
        }

        private void RemoveInstructorPermission(SPFolder learnerSubFolder, SPUser instructor)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(learnerSubFolder.ParentWeb.Site.ID))
                {
                    using (SPWeb web = site.OpenWeb(learnerSubFolder.ParentWeb.ID))
                    {
                        SPList dropBoxLib = web.Lists[learnerSubFolder.ParentListId];
                        learnerSubFolder.Item.Web.AllowUnsafeUpdates = true;
                        learnerSubFolder.Item.RoleAssignments.Remove(instructor);
                        learnerSubFolder.Update();
                        learnerSubFolder.Item.Web.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        private void RemoveObserverPermission(SPListItem learnerSubFolderItem)
        {
            bool isReader = false, isObserver = false;
            SPWeb web = learnerSubFolderItem.Web;

            foreach (SPRoleAssignment learnerSubfolderRoleAssignment in learnerSubFolderItem.RoleAssignments)
            {
                for (int i = 0; i < learnerSubfolderRoleAssignment.RoleDefinitionBindings.Count; i++)
                {
                    if (learnerSubfolderRoleAssignment.RoleDefinitionBindings[i].Type == SPRoleType.Reader)
                    {
                        isReader = true;

                        foreach (SPRoleAssignment webRoleAssignment in web.RoleAssignments)
                        {
                            for (int j = 0; j < webRoleAssignment.RoleDefinitionBindings.Count; j++)
                            {
                                if (webRoleAssignment.Member.Name == learnerSubfolderRoleAssignment.Member.Name &&
                                    webRoleAssignment.RoleDefinitionBindings[j].Name == SlkStore.GetStore(web).Mapping.ObserverPermission)
                                {
                                    isObserver = true;
                                    break;
                                }
                            }
                            if (isObserver)
                                break;
                        }
                    }
                    if (isReader && isObserver)
                    {
                        learnerSubfolderRoleAssignment.RoleDefinitionBindings.RemoveAll();
                        web.AllowUnsafeUpdates = true;
                        learnerSubfolderRoleAssignment.Update();
                        web.AllowUnsafeUpdates = false;
                        isReader = false;
                        isObserver = false;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        /// <param name="item">The current learning item</param>
        public void ApplyCollectAssignmentPermissions(SPList dropBoxDocLib, AssignmentProperties currentAssignmentProperties, string learnerName, SlkMemberships slkMembers)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(currentAssignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(currentAssignmentProperties.SPWebGuid))
                    {
                        SPUser currentLearner = null;
                        SPListItem assignmentFolder = null, learnerSubFolder = null;

                        // Get the Drop Box
                        SPList dropBox = spWeb.Lists[dropBoxDocLib.ID];

                        // Get the assignment folder
                        string assignmentFolderName = (currentAssignmentProperties.Title + " " + GetDateOnly(currentAssignmentProperties.DateCreated)).Trim();
                        assignmentFolder = GetAssignmentFolder(dropBox, assignmentFolderName);

                        if (assignmentFolder != null)
                        {
                            foreach (SlkUser learner in currentAssignmentProperties.Learners)
                            {
                                if (learner.Name == learnerName)
                                {
                                    currentLearner = GetSPLearner(slkMembers, learner);

                                    // IF the assignment is auto return, the learner will still be able to view the drop box assignment files
                                    // otherwise, learner permissions will be removed from the assignment folder in the Drop Box document library
                                    if (!currentAssignmentProperties.AutoReturn)
                                    {
                                        // Remove any permissions previously granted to the learner on the assignment folder
                                        RemoveLearnerPermission(assignmentFolder.Folder, currentLearner);
                                    }

                                    // Get the learner sub folder
                                    learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.Name);
                                    if (learnerSubFolder == null)
                                    {
                                        learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.LoginName);
                                    }

                                    //For Course Manager assignments, if the folder is not created yet
                                    if (learnerSubFolder == null)
                                    {
                                        learnerSubFolder = CreateAssignmentSubFolderForCourseManagerLearnerAssCollect(
                                            currentAssignmentProperties, 
                                            assignmentFolder,
                                            currentLearner.Name);
                                    }
                                    
                                    // IF the assignment is auto return, the learner will still be able to view the drop box assignment files
                                    // otherwise, learner permissions will be removed from the learner's subfolder in the Drop Box document library
                                    if (!currentAssignmentProperties.AutoReturn)
                                    {
                                        if (learnerSubFolder != null)
                                            RemoveLearnerPermission(learnerSubFolder.Folder, currentLearner);
                                    }

                                    // Grant instructor contribute permission to be able to edit learner files
                                    foreach (SlkUser instructor in currentAssignmentProperties.Instructors)
                                    {
                                        ApplyPermissionToFolder(learnerSubFolder, GetSPInstructor(slkMembers, instructor), SPRoleType.Contributor);
                                    }
                                    RemoveObserverPermission(learnerSubFolder);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        /// <param name="item">The current learning item</param>
        public void ApplyReturnAssignmentPermission(SPList dropBoxDocLib, AssignmentProperties currentAssignmentProperties, string learnerName, SlkMemberships slkMembers)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(currentAssignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(currentAssignmentProperties.SPWebGuid))
                    {
                        SPUser currentLearner = null;
                        SPListItem assignmentFolder = null, learnerSubFolder = null;

                        // Get the Drop Box
                        SPList dropBox = spWeb.Lists[dropBoxDocLib.ID];

                        // Get the assignment folder
                        string assignmentFolderName = (currentAssignmentProperties.Title + " " + GetDateOnly(currentAssignmentProperties.DateCreated)).Trim();
                        assignmentFolder = GetAssignmentFolder(dropBox, assignmentFolderName);

                        if (assignmentFolder != null)
                        {
                            foreach (SlkUser learner in currentAssignmentProperties.Learners)
                            {
                                if (learner.Name == learnerName)
                                {
                                    // Get the learner SPUser 
                                    currentLearner = GetSPLearner(slkMembers, learner);

                                    // first, remove any permissions previously granted to the learner on the assignment folder
                                    RemoveLearnerPermission(assignmentFolder.Folder, currentLearner);
                                    // then, grant learner Read permission on the assignment folder 
                                    ApplyPermissionToFolder(assignmentFolder, currentLearner, SPRoleType.Reader);

                                    // Get the learner sub folder
                                    learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.Name);
                                    if (learnerSubFolder == null)
                                        learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.LoginName);

                                    if (learnerSubFolder != null)
                                    {
                                        RemoveLearnerPermission(learnerSubFolder.Folder, currentLearner);
                                        // Grant learner Read permission on the learner subfolder 
                                        ApplyPermissionToFolder(learnerSubFolder, currentLearner, SPRoleType.Reader);
                                        // Remove instructors permission from the learner subfolder
                                        foreach (SlkUser instructor in currentAssignmentProperties.Instructors)
                                        {
                                            SPUser user = GetSPInstructor(slkMembers, instructor);
                                            ApplyPermissionToFolder(learnerSubFolder, user, SPRoleType.Reader);
                                            RemoveInstructorPermission(learnerSubFolder.Folder, user);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        /// <param name="item">The current learning item</param>
        public void ApplyReactivateAssignmentPermission(SPList dropBoxDocLib, AssignmentProperties currentAssignmentProperties, string learnerName, SlkMemberships slkMembers)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(currentAssignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(currentAssignmentProperties.SPWebGuid))
                    {
                        SPUser currentLearner = null;
                        SPListItem assignmentFolder = null, learnerSubFolder = null;

                        // Get the Drop Box
                        SPList dropBox = spWeb.Lists[dropBoxDocLib.ID];

                        // Get the assignment folder
                        string assignmentFolderName = (currentAssignmentProperties.Title + " " + GetDateOnly(currentAssignmentProperties.DateCreated)).Trim();
                        assignmentFolder = GetAssignmentFolder(dropBox, assignmentFolderName);

                        if (assignmentFolder != null)
                        {
                            foreach (SlkUser learner in currentAssignmentProperties.Learners)
                            {
                                if (learner.Name == learnerName)
                                {
                                    // Get the learner SPUser 
                                    currentLearner = GetSPLearner(slkMembers, learner);

                                    // Grant learner Read permission on the assignment folder 
                                    ApplyPermissionToFolder(assignmentFolder, currentLearner, SPRoleType.Reader);

                                    // Get the learner sub folder
                                    learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.Name);
                                    if (learnerSubFolder == null)
                                        learnerSubFolder = GetSubFolder(assignmentFolder, currentLearner.LoginName);

                                    if (learnerSubFolder != null)
                                    {
                                        // Grant learner Read permission on the learner subfolder 
                                        ApplyPermissionToFolder(learnerSubFolder, currentLearner, SPRoleType.Reader);
                                        // Give Instructors Read Access Permission on the learner subfolder
                                        ApplyInstructorsGradingPermission(learnerSubFolder);

                                        RemoveObserverPermission(learnerSubFolder);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        abstract class UpdateItem
        {
            public abstract void Update();
        }

        class UpdateList : UpdateItem
        {
            SPList list;

            public UpdateList(SPList list)
            {
                this.list = list;
            }

            public override void Update()
            {
                list.Update();
            }
        }

        class UpdateListItem : UpdateItem
        {
            SPListItem listItem;

            public UpdateListItem(SPListItem listItem)
            {
                this.listItem = listItem;
            }

            public override void Update()
            {
                listItem.Update();
            }
        }
    }
}
