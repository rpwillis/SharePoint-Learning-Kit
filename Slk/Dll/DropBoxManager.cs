using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.IO;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.LearningComponents.Storage;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Class for creating the folders and subfolders under the DropBox document library in both modes Create and Edit.
    /// </summary>
    public class DropBoxManager
    {
        const string dropBoxName = "DropBox";
        const string columnAssignmentKey = "Assignment";
        const string columnAssignmentDate = "AssignmentDate";
        const string columnAssignmentName = "AssignmentName";
        const string columnAssignmentId = "AssignmentId";
        const string columnIsLatest = "IsLatest";
        const string columnLearnerId = "LearnerId";
        const string columnLearner = "Learner";
        const string propertyKey = "SLKDropBox";
        AssignmentProperties assignmentProperties;

#region constructors
        public DropBoxManager(AssignmentProperties assignmentProperties)
        {
            this.assignmentProperties = assignmentProperties;
        }
#endregion constructors

#region properties
        /// <summary>The name of the Drop Box library.</summary>
        static string LibraryName
        {
            get { return dropBoxName  ;}
        }

        /// <summary>The name of the assignment folder in the drop box library.</summary>
        public string FolderName
        {
            get
            {
                return GenerateFolderName(assignmentProperties);
            }
        }

        SPUser CurrentUser
        {
            get { return SPContext.Current.Web.CurrentUser ;}
        }
#endregion properties

#region public methods
        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        public void ApplyCollectAssignmentPermissions(long learnerId)
        {
            // If the assignment is auto return, the learner will still be able to view the drop box assignment files
            // otherwise, learner permissions will be removed from the drop box library & learner's subfolder in the Drop Box document library
            SPRoleType learnerPermissions = SPRoleType.Reader;
            if (assignmentProperties.AutoReturn == false)
            {
                learnerPermissions = SPRoleType.None;
            }

            ApplyAssignmentPermission(learnerId, learnerPermissions, SPRoleType.Contributor, true);
        }

        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        /// <param name="item">The current learning item</param>
        public void ApplyReturnAssignmentPermission(long learnerId)
        {
            ApplyAssignmentPermission(learnerId, SPRoleType.Reader, SPRoleType.Reader, false);
        }

        /// <summary>
        /// Updates permissions of Drop Box assignment folder corresponding to item
        /// </summary>
        /// <param name="item">The current learning item</param>
        public void ApplyReactivateAssignmentPermission(long learnerId)
        {
            ApplyAssignmentPermission(learnerId, SPRoleType.Reader, SPRoleType.Contributor, true);
        }

        public string CopyFileToDropBox()
        {
            string url = null;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SharePointFileLocation fileLocation;
                if (!SharePointFileLocation.TryParse(assignmentProperties.Location, out fileLocation))
                {
                    throw new SafeToDisplayException(SlkFrameset.FRM_DocumentNotFound);
                }

                using (SPSite sourceSite = new SPSite(fileLocation.SiteId,SPContext.Current.Site.Zone))
                {
                    using (SPWeb sourceWeb = sourceSite.OpenWeb(fileLocation.WebId))
                    {
                        SPFile file = sourceWeb.GetFile(fileLocation.FileId);

                        if (MustCopyFileToDropBox(file.Name))
                        {
                            using (SPSite destinationSite = new SPSite(assignmentProperties.SPSiteGuid))
                            {
                                destinationSite.CatchAccessDeniedException = false;
                                using (SPWeb destinationWeb = destinationSite.OpenWeb(assignmentProperties.SPWebGuid))
                                {
                                    bool originalAllow = destinationWeb.AllowUnsafeUpdates;
                                    destinationWeb.AllowUnsafeUpdates = true;
                                    try
                                    {
                                        SPUser learner = CurrentUser;
                                        SPList dropBox = DropBoxLibrary(destinationWeb);
                                        SPListItem assignmentFolder = GetAssignmentFolder(dropBox);
                                        SPListItem learnerSubFolder = null;

                                        if (assignmentFolder == null)
                                        {
                                            assignmentFolder = CreateAssignmentFolder(dropBox);
                                        }
                                        else
                                        {
                                            learnerSubFolder = FindLearnerFolder(assignmentFolder, learner);
                                        }

                                        if (learnerSubFolder == null)
                                        {
                                            learnerSubFolder = CreateLearnerAssignmentFolder(assignmentFolder, CurrentUser);
                                        }

                                        ApplyPermissionToFolder(assignmentFolder, learner, SPRoleType.Reader);
                                        ApplyPermissionToFolder(learnerSubFolder, learner, SPRoleType.Contributor);

                                        using (Stream stream = file.OpenBinaryStream())
                                        {
                                            url = SaveFile(file.Name, stream, learnerSubFolder.Folder, learner);
                                        }
                                    }
                                    finally
                                    {
                                        destinationWeb.AllowUnsafeUpdates = originalAllow;
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return url;
        }

        public void UploadFiles(LearnerAssignmentProperties learnerAssignmentProperties, AssignmentUpload[] files)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    spSite.CatchAccessDeniedException = false;
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPUser learner = CurrentUser;
                        SPList dropBox = DropBoxLibrary(spWeb);
                        SPListItem assignmentFolder = GetAssignmentFolder(dropBox);
                        SPListItem learnerSubFolder = null;

                        if (assignmentFolder == null)
                        {
                            assignmentFolder = CreateAssignmentFolder(dropBox);
                        }
                        else
                        {
                            learnerSubFolder = FindLearnerFolder(assignmentFolder, learner);

                            if (learnerSubFolder != null)
                            {
                                ResetIsLatestFiles(learnerSubFolder);
                            }
                        }

                        ApplyPermissionToFolder(assignmentFolder, CurrentUser, SPRoleType.Reader);

                        if (learnerSubFolder == null)
                        {
                            learnerSubFolder = CreateLearnerAssignmentFolder(assignmentFolder, CurrentUser);
                        }

                        CheckExtensions(spSite, files);

                        bool currentAllowUnsafeUpdates = spWeb.AllowUnsafeUpdates;
                        spWeb.AllowUnsafeUpdates = true;
                        try
                        {

                            foreach (AssignmentUpload upload in files)
                            {
                                SaveFile(upload.Name, upload.Stream, learnerSubFolder.Folder, learner);
                            }
                        }
                        finally
                        {
                            spWeb.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
                        }
                        ApplySubmittedPermissions(spWeb);
                    }
                }
            });

            try
            {
                using (SPSite contextSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    contextSite.CatchAccessDeniedException = false;
                    using (SPWeb contextWeb = contextSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SlkStore.GetStore(contextWeb).ChangeLearnerAssignmentState(learnerAssignmentProperties.LearnerAssignmentGuidId, LearnerAssignmentState.Completed);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // state transition isn't supported
            }
        }

        /// <summary>Applys permissions when the document is submitted.</summary>
        public void ApplySubmittedPermissions()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate{
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        ApplySubmittedPermissions(spWeb);
                    }
                }
                    });
        }

        /// <summary>Returns all files for an assignment grouped by learner.</summary>
        public Dictionary<string, List<SPFile>> AllFiles()
        {
            Dictionary<string, List<SPFile>> files = new Dictionary<string, List<SPFile>>();
            using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
            {
                using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                {
                    SPList dropBox = DropBoxLibrary(web);

                    string queryXml = @"<Where>
                                        <And>
                                            <Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq>
                                            <Eq><FieldRef Name='{2}'/><Value Type='Boolean'>1</Value></Eq>
                                        </And>
                                     </Where>";
                    queryXml = string.Format(CultureInfo.InvariantCulture, queryXml, columnAssignmentId, assignmentProperties.Id.GetKey(), columnIsLatest);
                    SPQuery query = new SPQuery();
                    query.ViewAttributes = "Scope=\"Recursive\"";
                    query.Query = queryXml;
                    SPListItemCollection items = dropBox.GetItems(query);

                    SPFieldUser learnerField = (SPFieldUser)dropBox.Fields[columnLearner];

                    foreach (SPListItem item in items)
                    {
                        SPFile file = item.File;
                        SPFieldUserValue learnerValue = (SPFieldUserValue) learnerField.GetFieldValue(item[columnLearner].ToString());
                        SPUser learner = learnerValue.User;

                        List<SPFile> learnerFiles;
                        string learnerAccount = learner.LoginName.Replace("\\", "-");
                        if (files.TryGetValue(learnerAccount, out learnerFiles) == false)
                        {
                            learnerFiles = new List<SPFile>();
                            files.Add(learnerAccount, learnerFiles);
                        }

                        learnerFiles.Add(item.File);
                    }

                    return files;
                }
            }
            
        }

        /// <summary>
        /// Function to update the DropBox document library after the current assignment being edited
        /// </summary>
        /// <param name="newAssignmentProperties">The new assignment properties</param>
        public void UpdateAssignment(AssignmentProperties newAssignmentProperties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList dropBox = DropBoxLibrary(spWeb);
                        string newAssignmentFolderName = GenerateFolderName(newAssignmentProperties);

                        // If assignment title has been changed, create a new assignment folder and move old assignment folder contents to it
                        if (string.Compare(FolderName,  newAssignmentFolderName, true, CultureInfo.CurrentUICulture) != 0)
                        {
                            ChangeFolderName(dropBox, newAssignmentFolderName);
                        }

                        // Get new assignment folder, or the old one if the title has not been changed
                        // in both cases, the value of the current assignment folder name will be stored in newAssignmentFolderName
                        SPListItem assignmentFolder = GetFolder(dropBox, newAssignmentFolderName);

                        if (assignmentFolder != null)
                        {
                            spWeb.AllowUnsafeUpdates = true;

                            //remove all permissions that has been granted before on the assignment folder
                            foreach (SPRoleAssignment role in assignmentFolder.RoleAssignments)
                            {
                                role.RoleDefinitionBindings.RemoveAll();
                            }
                            assignmentFolder.Update();

                            // Grant assignment instructors Read permission on the assignment folder
                            ApplyInstructorsReadAccessPermissions(assignmentFolder);

                            // Delete subfolders of the learners who have been removed from the assignment
                            DeleteRemovedLearnerFolders(assignmentFolder, newAssignmentProperties);

                            foreach (SlkUser learner in newAssignmentProperties.Learners)
                            {
                                // Grant assignment learners Read permission on the assignment folder
                                ApplyPermissionToFolder(assignmentFolder, learner.SPUser, SPRoleType.Reader);

                                SPListItem learnerSubFolder = FindLearnerFolder(assignmentFolder, learner.SPUser);

                                if (learnerSubFolder != null)
                                {
                                    //remove all permissions that has been granted before on the learner subfolder
                                    foreach (SPRoleAssignment role in learnerSubFolder.RoleAssignments)
                                    {
                                        role.RoleDefinitionBindings.RemoveAll();
                                    }
                                    learnerSubFolder.Update();

                                    // Grant assignment instructors permission on the learner subfolder based on the learner assignment status
                                    ApplyInstructorsReadAccessPermissions(learnerSubFolder);

                                    // Grant learner permission on his/her learner subfolder based on the learner assignment status
                                    ApplyPermissionToFolder(learnerSubFolder, learner.SPUser, SPRoleType.Reader);
                                }
                                else
                                // In case the assignment is assigned to new learner
                                {
                                    // Create a new subfolder for this learner
                                    CreateLearnerAssignmentFolder(assignmentFolder, learner.SPUser);
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
        public SPListItem CreateSubFolder(SPFolder folder, string newFolderName)
        {
            SPWeb web = folder.Item.Web;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                SPFolder learnerSubFolder = folder.SubFolders.Add(newFolderName);
                learnerSubFolder.Update();
                BreakRoleInheritance(learnerSubFolder.Item);
                return learnerSubFolder.Item;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        /// <summary>Returns the last submitted files for the given learner.</summary>
        public AssignmentFile[] LastSubmittedFiles(long learnerId)
        {
            foreach (SlkUser learner in assignmentProperties.Learners)
            {
                if (learner.UserId.GetKey() == learnerId)
                {
                    return LastSubmittedFiles(learner.SPUser);
                }
            }
            return new AssignmentFile[0];
        }

        /// <summary>Returns the last submitted files for the current user.</summary>
        public AssignmentFile[] LastSubmittedFiles()
        {
            return LastSubmittedFiles(CurrentUser);
        }

        /// <summary>Deletes the assignment folder.</summary>
        public void DeleteAssignmentFolder()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = DropBoxLibrary(spWeb);

                        //Get the folder if it exists 
                        SPListItem assignmentFolder = GetAssignmentFolder(assDropBox);

                        if (assignmentFolder != null)
                        {
                            bool currentAllowUnsafeUpdates = spWeb.AllowUnsafeUpdates;
                            spWeb.AllowUnsafeUpdates = true;
                            try
                            {
                                assignmentFolder.Delete();
                            }
                            catch
                            {
                                spWeb.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
                            }
                        }
                    }
                }
            });
        }

        SPListItem CreateLearnerAssignmentFolder(SPListItem assignmentFolder, SPUser user)
        {
            SPWeb web = assignmentFolder.ParentList.ParentWeb;
            bool originalAllow = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;

            try
            {
                SPListItem assSubFolder = CreateSubFolder(assignmentFolder.Folder, LearnerFolderName(user));
                ApplyPermissionToFolder(assSubFolder, user, SPRoleType.Reader);
                return assSubFolder;
            }
            finally
            {
                web.AllowUnsafeUpdates = originalAllow;
            }

        }

        /// <summary>Creates an assignment folder.</summary>
        public void CreateAssignmentFolder()
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = DropBoxLibrary(spWeb);

                        //Give Instructor read access permission over the Document Library
                        ApplyInstructorReadAccessOnDocLib(assDropBox, spWeb);

                        //Get the folder if it exists 
                        if (GetAssignmentFolder(assDropBox) != null)
                        {
                            throw new SafeToDisplayException(AppResources.AssFolderAlreadyExists);
                        }

                        SPListItem assignmentFolder = CreateAssignmentFolder(assDropBox);
                        ApplyInstructorsReadAccessPermissions(assignmentFolder);

                        //Create a Subfolder for each learner
                        foreach (SlkUser learner in assignmentProperties.Learners)
                        {
                            SPUser spLearner = learner.SPUser;
                            ApplyPermissionToFolder(assignmentFolder, spLearner, SPRoleType.Reader);
                            CreateLearnerAssignmentFolder(assignmentFolder, spLearner);
                        }
                    }
                }
            });
        }

        /// <summary>Creates an assignment folder.</summary>
        /// <returns>The created folder</returns>
        public SPListItem CreateSelfAssignmentFolder()
        {
            SPListItem assignmentFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList assDropBox = DropBoxLibrary(spWeb);

                        //Give Instructor read access permission over the Document Library
                        ApplyInstructorReadAccessOnDocLib(assDropBox, spWeb);
                        
                        //Get the folder if it exists 
                        assignmentFolder = GetAssignmentFolder(assDropBox);

                        //Create the assignment folder if it does not exist
                        if (assignmentFolder == null)
                        {
                            assignmentFolder = CreateAssignmentFolder(assDropBox);
                            ApplyPermissionToFolder(assignmentFolder, CurrentUser, SPRoleType.Reader);
                            CreateLearnerAssignmentFolder(assignmentFolder, CurrentUser);
                        }
                        else
                        {
                            assignmentFolder = null;
                        }
                    }
                }
            });
            return assignmentFolder;
        }

#endregion public methods

#region private methods
        void CheckExtensions(SPSite site, AssignmentUpload[] files)
        {
            Collection<string> blockedExtensions = site.WebApplication.BlockedFileExtensions;
            List<string> failures = new List<string>();

            foreach (AssignmentUpload upload in files)
            {
                if (CheckExtension(blockedExtensions, upload.Name) == false)
                {
                    failures.Add(upload.Name);
                }
            }

            if (failures.Count > 0)
            {
                string message = string.Format(CultureInfo.CurrentUICulture, AppResources.FilesUploadPageFailureMessage, string.Join(", ", failures.ToArray()));
                throw new SafeToDisplayException(message);
            }
        }

        bool CheckExtension(Collection<string> blockedExtensions, string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return true;
            }
            else
            {
                extension = extension.ToLower(CultureInfo.InvariantCulture);
                if (extension[0] == '.')
                {
                    extension = extension.Substring(1);
                }

                if (blockedExtensions != null && blockedExtensions.Contains(extension))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        void ChangeFolderName(SPList dropBox, string newAssignmentFolderName)
        {
            SPWeb web = dropBox.ParentWeb;
            //Get old assignment folder
            SPListItem oldAssignmentFolder = GetAssignmentFolder(dropBox);
            if (oldAssignmentFolder != null)
            {
                if (!web.GetFolder(oldAssignmentFolder.Url.Replace(FolderName, newAssignmentFolderName)).Exists)
                {
                    oldAssignmentFolder.Folder.MoveTo(oldAssignmentFolder.Url.Replace(FolderName, newAssignmentFolderName));
                }
                else
                {
                    throw new SafeToDisplayException(AppResources.AssignmentNameAlreadyUsed);
                }
            }
            else
            {
                throw new SafeToDisplayException(AppResources.AssFolderNotFound);
            }
        }

        void DeleteRemovedLearnerFolders(SPListItem assignmentFolder, AssignmentProperties newAssignmentProperties)
        {
            foreach (SlkUser oldLearner in assignmentProperties.Learners)
            {
                if (!newAssignmentProperties.Learners.Contains(oldLearner.UserId))
                {
                    // Get learner subfolder, and delete it if exists
                    SPListItem learnerSubFolder = FindLearnerFolder(assignmentFolder, oldLearner.SPUser);
                    if (learnerSubFolder != null)
                    {
                        learnerSubFolder.Delete();
                    }
                    assignmentFolder.Update();
                }
            }
        }

        /// <summary>
        /// Function searches for the assignment folder
        /// </summary>
        /// <param name="dropBoxList">The current document library which contains the specified folder</param>
        /// <returns>The folder object</returns>
        SPListItem GetAssignmentFolder(SPList dropBoxList)
        {
            return GetFolder(dropBoxList, FolderName);
        }

        /// <summary>
        /// Function searches for the specified subfolder
        /// </summary>
        /// <param name="assDropBox">The document library which contains the assignment folder</param>
        /// <param name="assignmentFolder">The assignment folder object which contains the subfolders</param>
        /// <param name="subFolderName"></param>
        /// <returns></returns>
        SPListItem GetSubFolder(SPListItem assignmentFolder, string subFolderName)
        {
            SPQuery subFolderQuery = new SPQuery();
            subFolderQuery.Folder = assignmentFolder.Folder;
            subFolderQuery.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + subFolderName + "</Value></Eq></Where>";
            SPListItemCollection allSubFolders = assignmentFolder.ParentList.GetItems(subFolderQuery);
            if (allSubFolders.Count != 0)
            {
                return allSubFolders[0];
            }
            else
            {
                return null;
            }
        }

        void ApplyInstructorReadAccessOnDocLib(SPList assDropBox, SPWeb spWeb)
        {
            SPUser user = CurrentUser;
            if (!assDropBox.DoesUserHavePermissions(user, SPBasePermissions.ViewListItems))
            {
                ApplyPermission(spWeb, assDropBox, user, SPRoleType.Reader, new UpdateList(assDropBox));
            }
        }

        void ApplyInstructorsReadAccessPermissions(SPListItem folder)
        {
            foreach (SlkUser instructor in assignmentProperties.Instructors)
            {
                ApplyPermissionToFolder(folder, instructor.SPUser, SPRoleType.Reader);
            }
        }

        void ApplyPermissionToFolder(SPListItem folder, SPUser user, SPRoleType roleType)
        {
            if (folder != null)
            {
                ApplyPermission(folder.Web, folder, user, roleType, new UpdateListItem(folder));
            }
        }

        void ApplyPermission(SPWeb web, ISecurableObject securable, SPUser user, SPRoleType roleType, UpdateItem updateItem)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }
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

        string SaveFile(string fileName, Stream fileStream, SPFolder learnerSubFolder, SPUser learner)
        {
            string fileUrl = learnerSubFolder.Url + '/' + fileName;

            SPFile file = learnerSubFolder.Files.Add(fileUrl, fileStream, true);
            file = learnerSubFolder.Files[fileUrl];
            file.Item[columnAssignmentDate] = assignmentProperties.StartDate;
            file.Item[columnAssignmentName] = assignmentProperties.Title.Trim();
            file.Item[columnAssignmentId] = assignmentProperties.Id.GetKey();
            file.Item[columnLearnerId] = learner.Sid;
            file.Item[columnLearner] = learner;
            file.Item[columnIsLatest] = true; 
            file.Item.Update();
            return file.ServerRelativeUrl;
        }

        void RemovePermissions(SPListItem folder, SPUser user)
        {
            if (folder != null && user != null)
            {
                SPWeb web = folder.Web;
                bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
                try
                {
                    web.AllowUnsafeUpdates = true;
                    folder.RoleAssignments.Remove(user);
                    folder.Update();
                }
                finally
                {
                    web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
                }
            }
        }

        void RemoveObserverPermission(SPListItem learnerSubFolderItem)
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
        
        void ApplyAssignmentPermission(long learnerId, SPRoleType learnerPermissions, SPRoleType instructorPermissions, bool removeObserverPermissions)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList dropBox = DropBoxLibrary(spWeb);
                        SPListItem assignmentFolder = GetAssignmentFolder(dropBox);

                        if (assignmentFolder != null)
                        {
                            foreach (SlkUser learner in assignmentProperties.Learners)
                            {
                                if (learner.UserId.GetKey() == learnerId)
                                {
                                    SPUser currentLearner = learner.SPUser;

                                    // Get the learner sub folder
                                    SPListItem learnerSubFolder = FindLearnerFolder(assignmentFolder, currentLearner);

                                    //For Course Manager assignments, if the folder is not created yet
                                    if (learnerSubFolder == null)
                                    {
                                        learnerSubFolder = CreateSubFolder(assignmentFolder.Folder, currentLearner.Name);
                                    }
                                    
                                    // Apply learner permissions
                                    RemovePermissions(assignmentFolder, currentLearner);
                                    if (learnerPermissions == SPRoleType.None)
                                    {
                                        RemovePermissions(learnerSubFolder, currentLearner);
                                    }
                                    else
                                    {
                                        ApplyPermissionToFolder(assignmentFolder, currentLearner, SPRoleType.Reader);
                                        ApplyPermissionToFolder(learnerSubFolder, currentLearner, learnerPermissions);
                                    }

                                    // Apply instructor permissions
                                    foreach (SlkUser instructor in assignmentProperties.Instructors)
                                    {
                                        RemovePermissions(learnerSubFolder, instructor.SPUser);
                                        ApplyPermissionToFolder(learnerSubFolder, instructor.SPUser, instructorPermissions);
                                    }

                                    if (removeObserverPermissions)
                                    {
                                        RemoveObserverPermission(learnerSubFolder);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            });
        }

        SPList DropBoxLibrary(SPWeb web)
        {
            object property = web.AllProperties[propertyKey];
            if (property != null)
            {
                try
                {
                    Guid id = new Guid(property.ToString());
                    return web.Lists.GetList(id, true);
                }
                catch (FormatException) {}
                catch (OverflowException) {}
                catch (SPException) {}
            }

            return CreateDropBoxLibrary(web);
        }

        SPListItem CreateAssignmentFolder(SPList dropBox)
        {
            SPWeb web = dropBox.ParentWeb;
            string url = dropBox.RootFolder.ServerRelativeUrl;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;

            try
            {
                web.AllowUnsafeUpdates = true;
                SPListItem assignmentFolder = dropBox.Items.Add(url, SPFileSystemObjectType.Folder, FolderName);
                assignmentFolder.Update();
                BreakRoleInheritance(assignmentFolder);
                dropBox.Update();
                CreateAssignmentView(dropBox);
                return assignmentFolder;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        void CreateAssignmentView(SPList dropBox)
        {
            StringCollection fields = new StringCollection();
            fields.Add(dropBox.Fields[SPBuiltInFieldId.DocIcon].InternalName);
            fields.Add(dropBox.Fields[SPBuiltInFieldId.LinkFilename].InternalName);
            fields.Add(columnLearner);
            fields.Add(dropBox.Fields[SPBuiltInFieldId.Modified].InternalName);
            fields.Add(columnIsLatest);

            string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /></GroupBy><Where><Eq><FieldRef Name=\"{1}\" /><Value Type=\"Text\">{2}</Value></Eq></Where>";
            query = string.Format(CultureInfo.InvariantCulture, query, columnLearner, columnAssignmentId, assignmentProperties.Id.GetKey());
            SPView view = dropBox.Views.Add(AssignmentViewName(), fields, query, 100, true, false, SPViewCollection.SPViewType.Html, false);
            view.Update();
            dropBox.Update();

            // Seem to need to re-get the view to set scope and group header.
            SPView view2 = dropBox.Views[view.ID];
            view2.Scope = SPViewScope.Recursive;
            RemoveFieldNameFromGroupHeader(view2);
            view2.Update();
            dropBox.Update();
        }

        SPListItem FindLearnerFolder(SPListItem assignmentFolder, SPUser learner)
        {
            return GetSubFolder(assignmentFolder, LearnerFolderName(learner));
        }

        void ResetIsLatestFiles(SPListItem learnerSubFolder)
        {
            SPWeb web = learnerSubFolder.ParentList.ParentWeb;
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                foreach (SPFile file in learnerSubFolder.Folder.Files)
                {
                    file.Item[columnIsLatest] = false;
                    file.Item.Update();
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        AssignmentFile[] LastSubmittedFiles(SPUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            AssignmentFile[] toReturn = null;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList dropBox = DropBoxLibrary(spWeb);

                        string queryXml = @"<Where>
                                            <And>
                                                <Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq>
                                                <Eq><FieldRef Name='{2}'/><Value Type='Text'>{3}</Value></Eq>
                                            </And>
                                         </Where>";
                        queryXml = string.Format(CultureInfo.InvariantCulture, queryXml, columnAssignmentId, assignmentProperties.Id.GetKey(), columnLearnerId, user.Sid);
                        SPQuery query = new SPQuery();
                        query.ViewAttributes = "Scope=\"Recursive\"";
                        query.Query = queryXml;
                        SPListItemCollection items = dropBox.GetItems(query);

                        List<AssignmentFile> files = new List<AssignmentFile>();

                        foreach (SPListItem item in items)
                        {
                            if (item[columnIsLatest] != null)
                            {
                                if ((bool)item[columnIsLatest])
                                {
                                    SPFile file = item.File;
                                    files.Add(new AssignmentFile(file.Name, file.ServerRelativeUrl));
                                }
                            }
                        }
                        toReturn = files.ToArray();
                    }
                }
            });
            return toReturn;
        }

        string AssignmentViewName()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", GetDateOnly(assignmentProperties.StartDate), assignmentProperties.Title.Trim());
        }

        void ApplySubmittedPermissions(SPListItem assignmentFolder, SPListItem learnerSubFolder)
        {
            // IF the assignment is auto return, the learner will still be able to view the drop box assignment files
            // otherwise, learner permissions will be removed from the learner's subfolder in the Drop Box document library
            if (assignmentProperties.AutoReturn == false)
            {
                RemovePermissions(assignmentFolder, CurrentUser);
                RemovePermissions(learnerSubFolder, CurrentUser);
            }

            // Grant instructors contribute permission on learner subfolder
            foreach (SlkUser instructor in assignmentProperties.Instructors)
            {
                if (instructor.SPUser == null)
                {
                    throw new SafeToDisplayException(string.Format(CultureInfo.CurrentUICulture, AppResources.DropBoxManagerUploadFilesNoInstructor, instructor.Name));
                }
                RemovePermissions(learnerSubFolder, instructor.SPUser);
                ApplyPermissionToFolder(learnerSubFolder, instructor.SPUser, SPRoleType.Contributor);
            }

        }

        public void ApplySubmittedPermissions(SPWeb web)
        {
            SPList dropBox = DropBoxLibrary(web);
            SPListItem assignmentFolder = GetAssignmentFolder(dropBox);
            SPListItem learnerSubFolder = null;

            if (assignmentFolder == null)
            {
                assignmentFolder = CreateAssignmentFolder(dropBox);
            }
            else
            {
                learnerSubFolder = FindLearnerFolder(assignmentFolder, CurrentUser);
                ApplySubmittedPermissions(assignmentFolder, learnerSubFolder);
            }
        }
#endregion private methods

#region static methods
        static string LearnerFolderName(SPUser learner)
        {
            string folderName = learner.LoginName;

            return folderName.Replace("\\", "-");
        }

        static string GenerateFolderName(AssignmentProperties properties)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", GetDateOnly(properties.StartDate), properties.Title.Trim(), properties.Id.GetKey());
        }

        static SPListItem GetFolder(SPList dropBoxList, string folderName)
        {
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + folderName + "</Value></Eq></Where>";
            if (dropBoxList.GetItems(query).Count != 0)
            {
                return dropBoxList.GetItems(query)[0];
            }
            else
            {
                return null;
            }
        }
        
        static SPField AddField(SPList list, string name, SPFieldType type, bool addToDefaultView)
        {
            return AddField(list, name, type, addToDefaultView, true);
        }

        static SPField AddField(SPList list, string name, SPFieldType type, bool addToDefaultView, bool required)
        {
            list.Fields.Add(name, type, required);
            SPField field = list.Fields[name];
            if (addToDefaultView)
            {
                list.DefaultView.ViewFields.Add(field);
            }
            return field;
        }

        static SPList CreateDropBoxLibrary(SPWeb web)
        {
            return CreateDropBoxLibrary(web, 0);
        }

        static SPList CreateDropBoxLibrary(SPWeb web, int recursiveNumber)
        {
            bool currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            try
            {
                web.AllowUnsafeUpdates = true;
                Guid id;
                try
                {
                    string name = LibraryName;
                    if (recursiveNumber > 0)
                    {
                        name = name + recursiveNumber.ToString(CultureInfo.InvariantCulture);
                    }
                    id = web.Lists.Add(name, LibraryName, SPListTemplateType.DocumentLibrary);
                }
                catch (SPException)
                {
                    // Library already exists, add a number to make it unique
                    if (recursiveNumber < 10)
                    {
                        return CreateDropBoxLibrary(web, recursiveNumber++);
                    }
                    else
                    {
                        throw;
                    }
                }

                SPList list = web.Lists[id];

                SPView defaultView = list.DefaultView;

                SPField assignmentId = AddField(list, columnAssignmentId, SPFieldType.Text, false);
                assignmentId.Indexed = true;
                assignmentId.Update();
                AddField(list, columnAssignmentDate, SPFieldType.DateTime, true);
                AddField(list, columnAssignmentName, SPFieldType.Text, true);
                SPFieldCalculated assignmentKey = (SPFieldCalculated)AddField(list, columnAssignmentKey, SPFieldType.Calculated, true, false);
                string formula = "=TEXT([{0}], \"yyyy-mm-dd\")&\" \"&[{1}]";
                assignmentKey.Formula = string.Format(formula, columnAssignmentDate, columnAssignmentName);
                assignmentKey.Update();
                AddField(list, columnIsLatest, SPFieldType.Boolean, true);
                AddField(list, columnLearner, SPFieldType.User, true);
                AddField(list, columnLearnerId, SPFieldType.Text, false);

                // Set up versioning
                list.EnableVersioning = true;

                defaultView.Title = AppResources.DropBoxDefaultViewTitle;
                string query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"100\"><FieldRef Name=\"{0}\" /><FieldRef Name=\"{1}\" /></GroupBy>";
                defaultView.Query = string.Format(CultureInfo.InvariantCulture, query, columnAssignmentKey, columnLearner);

                defaultView.Update();
                list.Update();

                SPView view = list.Views[defaultView.ID];
                view.Scope = SPViewScope.Recursive;
                RemoveFieldNameFromGroupHeader(view);
                view.Update();
                list.Update();

                web.AllProperties[propertyKey] = list.ID.ToString("D", CultureInfo.InvariantCulture);
                web.Update();

                // Change name to internationalized name
                list.Title = AppResources.DropBoxTitle;
                ChangeColumnTitle(columnAssignmentKey, AppResources.DropBoxColumnAssignmentKey, list);
                ChangeColumnTitle(columnAssignmentName, AppResources.DropBoxColumnAssignmentName, list);
                ChangeColumnTitle(columnAssignmentDate, AppResources.DropBoxColumnAssignmentDate, list);
                ChangeColumnTitle(columnAssignmentId, AppResources.DropBoxColumnAssignmentId, list);
                ChangeColumnTitle(columnIsLatest, AppResources.DropBoxColumnIsLatest, list);
                ChangeColumnTitle(columnLearner, AppResources.DropBoxColumnLearner, list);
                ChangeColumnTitle(columnLearnerId, AppResources.DropBoxColumnLearnerId, list);
                list.Update();

                return list;
            }
            finally
            {
                web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
            }
        }

        static void RemoveFieldNameFromGroupHeader(SPView view)
        {
            string fieldNameHtml = "<GetVar Name=\"GroupByField\" HTMLEncode=\"TRUE\" /><HTML><![CDATA[</a> :&nbsp;]]></HTML>";
            if (view.GroupByHeader != null)
            {
                view.GroupByHeader = view.GroupByHeader.Replace(fieldNameHtml, string.Empty);
            }
        }

        static void ChangeColumnTitle(string columnKey, string newName, SPList list)
        {
            SPField field = list.Fields[columnKey];
            field.Title = newName;
            field.Update();
        }

        static void BreakRoleInheritance(SPListItem folder)
        {
            // There's a bug with BreakRoleInheritance which means that passing true reset AllowUnsafeUpdates to
            // false so the call fails. http://www.ofonesandzeros.com/2008/11/18/the-security-validation-for-this-page-is-invalid/
            folder.BreakRoleInheritance(true);

            folder.ParentList.ParentWeb.AllowUnsafeUpdates = true;

            SPRoleAssignmentCollection roleAssigns = folder.RoleAssignments;

            for (int i = roleAssigns.Count-1; i >= 0; i--)
            {
                roleAssigns.Remove(i);
            }
            folder.Update();
        }

        /// <summary>
        /// Truncate the time part and the '/' from the Created Date and convert it to string to be a valid folder name.
        /// </summary>
        /// <param name="fullDate">The date as it is returned from the CreatedDate property of the assignment</param>
        /// <returns> The date converted to string and without the time and the '/' character </returns>
        static string GetDateOnly(DateTime fullDate)
        {
            return fullDate.ToString("yyyy-MM-dd");
        }

        static bool MustCopyFileToDropBox(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            switch (extension)
            {
                case ".doc":
                    return true;
                case ".docx":
                    return true;
                case ".xls":
                    return true;
                case ".xlsx":
                    return true;
                case ".ppt":
                    return true;
                case ".pptx":
                    return true;
                default:
                    return false;
            }
        }
#endregion static methods

#region UpdateItem classes
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
#endregion UpdateItem classes

#region AssignmentFile
    public struct AssignmentFile
    {
        string name;
        string url;
        public string Url
        {
            get { return url ;}
        }
        public string Name
        {
            get { return name ;}
        }
        public AssignmentFile(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
#endregion AssignmentFile

#region AssignmentFile
    public struct AssignmentUpload
    {
        string name;
        Stream stream;
        public Stream Stream
        {
            get { return stream ;}
        }
        public string Name
        {
            get { return name ;}
        }
        public AssignmentUpload(string name, Stream inputStream)
        {
            this.name = name;
            this.stream = inputStream;
        }
    }
#endregion AssignmentFile
}
