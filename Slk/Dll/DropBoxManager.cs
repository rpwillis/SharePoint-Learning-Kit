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
        AssignmentProperties assignmentProperties;

#region constructors
        /// <summary>Initializes a new instance of <see cref="DropBoxManager"/>.</summary>
        /// <param name="assignmentProperties">The assignment properties.</param>
        public DropBoxManager(AssignmentProperties assignmentProperties)
        {
            this.assignmentProperties = assignmentProperties;
        }
#endregion constructors

#region properties
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
        /// <summary>Sets the correct permissions when the item is collected.</summary>
        /// <param name="user">The user to update the folder for.</param>
        public void ApplyCollectAssignmentPermissions(SPUser user)
        {
            // If the assignment is auto return, the learner will still be able to view the drop box assignment files
            // otherwise, learner permissions will be removed from the drop box library & learner's subfolder in the Drop Box document library
            SPRoleType learnerPermissions = SPRoleType.Reader;
            if (assignmentProperties.AutoReturn == false)
            {
                learnerPermissions = SPRoleType.None;
            }

            ApplyAssignmentPermission(user, learnerPermissions, SPRoleType.Contributor, true);
        }

        /// <summary>Sets the correct permissions when the item is returned to the learner.</summary>
        /// <param name="user">The user to update the folder for.</param>
        public void ApplyReturnAssignmentPermission(SPUser user)
        {
            ApplyAssignmentPermission(user, SPRoleType.Reader, SPRoleType.Reader, false);
        }

        /// <summary>Sets the correct permissions when the item is reactivated.</summary>
        /// <param name="user">The user to update the folder for.</param>
        public void ApplyReactivateAssignmentPermission(SPUser user)
        {
            ApplyAssignmentPermission(user, SPRoleType.Contributor, SPRoleType.Reader, true);
        }

        /// <summary>Copies the original file to the student's drop box.</summary>
        /// <returns>The url of the file.</returns>
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
                                        DropBox dropBox = new DropBox(destinationWeb);
                                        AssignmentFolder assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);
                                        AssignmentFolder learnerSubFolder = null;

                                        if (assignmentFolder == null)
                                        {
                                            assignmentFolder = dropBox.CreateAssignmentFolder(assignmentProperties);
                                        }
                                        else
                                        {
                                            learnerSubFolder = assignmentFolder.FindLearnerFolder(learner);
                                        }

                                        if (learnerSubFolder == null)
                                        {
                                            learnerSubFolder = assignmentFolder.CreateLearnerAssignmentFolder(CurrentUser);
                                        }

                                        assignmentFolder.ApplyPermission(learner, SPRoleType.Reader);
                                        learnerSubFolder.ApplyPermission(learner, SPRoleType.Contributor);

                                        using (Stream stream = file.OpenBinaryStream())
                                        {
                                            url = learnerSubFolder.SaveFile(file.Name, stream, learner);
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

        /// <summary>Uploads files to the learner's drop box.</summary>
        /// <param name="files">The files to upload.</param>
        public void UploadFiles(AssignmentUpload[] files)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    spSite.CatchAccessDeniedException = false;
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        DropBox dropBox = new DropBox(spWeb);
                        AssignmentFolder assignmentFolder = dropBox.GetOrCreateAssignmentFolder(assignmentProperties);
                        assignmentFolder.ApplyPermission(CurrentUser, SPRoleType.Reader);

                        AssignmentFolder learnerSubFolder = assignmentFolder.FindLearnerFolder(CurrentUser);

                        if (learnerSubFolder == null)
                        {
                            learnerSubFolder = assignmentFolder.CreateLearnerAssignmentFolder(CurrentUser);
                        }
                        else
                        {
                            learnerSubFolder.ResetIsLatestFiles();
                        }

                        CheckExtensions(spSite, files);

                        bool currentAllowUnsafeUpdates = spWeb.AllowUnsafeUpdates;
                        spWeb.AllowUnsafeUpdates = true;
                        try
                        {

                            foreach (AssignmentUpload upload in files)
                            {
                                learnerSubFolder.SaveFile(upload.Name, upload.Stream, CurrentUser);
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
            using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
            {
                using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                {
                    DropBox dropBox = new DropBox(web);
                    return dropBox.AllFiles(assignmentProperties.Id.GetKey());
                }
            }
            
        }

        /// <summary>Function to update the DropBox document library after the current assignment being edited</summary>
        /// <param name="oldAssignmentProperties">The old assignment properties</param>
        public void UpdateAssignment(AssignmentProperties oldAssignmentProperties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        DropBox dropBox = new DropBox(spWeb);

                        string oldAssignmentFolderName = GenerateFolderName(oldAssignmentProperties);
                        string newAssignmentFolderName = FolderName;

                        // If assignment title has been changed, create a new assignment folder and move old assignment folder contents to it
                        if (string.Compare(oldAssignmentFolderName,  newAssignmentFolderName, true, CultureInfo.CurrentUICulture) != 0)
                        {
                            dropBox.ChangeFolderName(oldAssignmentFolderName, newAssignmentFolderName);
                        }

                        // Get new assignment folder, or the old one if the title has not been changed
                        // in both cases, the value of the current assignment folder name will be stored in newAssignmentFolderName
                        AssignmentFolder assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);

                        if (assignmentFolder != null)
                        {
                            spWeb.AllowUnsafeUpdates = true;
                            assignmentFolder.RemoveAllPermissions();

                            // Grant assignment instructors Read permission on the assignment folder
                            ApplyInstructorsReadAccessPermissions(assignmentFolder);

                            // Delete subfolders of the learners who have been removed from the assignment
                            DeleteRemovedLearnerFolders(assignmentFolder, oldAssignmentProperties);

                            foreach (SlkUser learner in assignmentProperties.Learners)
                            {
                                // Grant assignment learners Read permission on the assignment folder
                                assignmentFolder.ApplyPermission(learner.SPUser, SPRoleType.Reader);

                                AssignmentFolder learnerSubFolder = assignmentFolder.FindLearnerFolder(learner.SPUser);

                                if (learnerSubFolder != null)
                                {
                                    learnerSubFolder.RemoveAllPermissions();

                                    // Grant assignment instructors permission on the learner subfolder based on the learner assignment status
                                    ApplyInstructorsReadAccessPermissions(learnerSubFolder);

                                    // Grant learner permission on his/her learner subfolder based on the learner assignment status
                                    learnerSubFolder.ApplyPermission(learner.SPUser, SPRoleType.Reader);
                                }
                                else
                                // In case the assignment is assigned to new learner
                                {
                                    // Create a new subfolder for this learner
                                    assignmentFolder.CreateLearnerAssignmentFolder(learner.SPUser);
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                }
            });
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

        /// <summary>Returns the last submitted files for the given learner.</summary>
        public AssignmentFile[] LastSubmittedFiles(SPUser user)
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
                        DropBox dropBox = new DropBox(spWeb);
                        toReturn = dropBox.LastSubmittedFiles(user, assignmentProperties.Id.GetKey());
                    }
                }
            });
            return toReturn;
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
                        DropBox dropBox = new DropBox(spWeb);

                        //Get the folder if it exists 
                        AssignmentFolder assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);

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

        /// <summary>Creates an assignment folder.</summary>
        public void CreateAssignmentFolder()
        {
            Debug("Starting DropBoxManager.CreateAssignmentFolder");
            Microsoft.SharePoint.Utilities.SPUtility.ValidateFormDigest();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        Debug("DropBoxManager.CreateAssignmentFolder: Have opened web {0}", spWeb.Url);
                        DropBox dropBox = new DropBox(spWeb);

                        //Get the folder if it exists 
                        if (dropBox.GetAssignmentFolder(assignmentProperties) != null)
                        {
                            Debug("DropBoxManager.CreateAssignmentFolder: assignment folder  already exists");
                            throw new SafeToDisplayException(AppResources.AssFolderAlreadyExists);
                        }

                        Debug("DropBoxManager.CreateAssignmentFolder: assignment folder does not exist");

                        AssignmentFolder assignmentFolder = dropBox.CreateAssignmentFolder(assignmentProperties);
                        Debug("DropBoxManager.CreateAssignmentFolder: have created assignment folder");
                        ApplyInstructorsReadAccessPermissions(assignmentFolder);
                        Debug("DropBoxManager.CreateAssignmentFolder: have applied instructor permission");

                        //Create a Subfolder for each learner
                        foreach (SlkUser learner in assignmentProperties.Learners)
                        {
                            SPUser spLearner = learner.SPUser;
                            Debug("DropBoxManager.CreateAssignmentFolder: create folder for {0}", spLearner.Name);
                            assignmentFolder.ApplyPermission(spLearner, SPRoleType.Reader);
                            Debug("DropBoxManager.CreateAssignmentFolder: have have set permission on assignment folder");
                            assignmentFolder.CreateLearnerAssignmentFolder(spLearner);
                            Debug("DropBoxManager.CreateAssignmentFolder: have created folder for {0}", spLearner.Name);
                        }
                    }
                }
            });
        }

        /// <summary>Creates an assignment folder.</summary>
        /// <returns>The created folder or null if it already exists.</returns>
        public AssignmentFolder CreateSelfAssignmentFolder()
        {
            AssignmentFolder assignmentFolder = null;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        DropBox dropBox = new DropBox(spWeb);

                        //Get the folder if it exists 
                        assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);

                        //Create the assignment folder if it does not exist
                        if (assignmentFolder == null)
                        {
                            assignmentFolder = dropBox.CreateAssignmentFolder(assignmentProperties);
                            assignmentFolder.ApplyPermission(CurrentUser, SPRoleType.Reader);
                            assignmentFolder.CreateLearnerAssignmentFolder(CurrentUser);
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

        /// <summary>Sets the correct permissions when the item is submitted.</summary>
        /// <param name="web">The web the assignment is for.</param>
        public void ApplySubmittedPermissions(SPWeb web)
        {
            DropBox dropBox = new DropBox(web);
            AssignmentFolder assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);
            AssignmentFolder learnerSubFolder = null;

            if (assignmentFolder == null)
            {
                assignmentFolder = dropBox.CreateAssignmentFolder(assignmentProperties);
            }
            else
            {
                learnerSubFolder = assignmentFolder.FindLearnerFolder(CurrentUser);
                ApplySubmittedPermissions(assignmentFolder, learnerSubFolder);
            }
        }
#endregion public methods

#region private methods
        SPList DropBoxLibrary (SPWeb web)
        {
            DropBox dropBox = new DropBox(web);
            return dropBox.DropBoxList;
        }

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

        void DeleteRemovedLearnerFolders(AssignmentFolder assignmentFolder, AssignmentProperties oldAssignmentProperties)
        {
            foreach (SlkUser oldLearner in oldAssignmentProperties.Learners)
            {
                if (!assignmentProperties.Learners.Contains(oldLearner.UserId))
                {
                    // Get learner subfolder, and delete it if exists
                    AssignmentFolder learnerSubFolder = assignmentFolder.FindLearnerFolder(oldLearner.SPUser);
                    if (learnerSubFolder != null)
                    {
                        learnerSubFolder.Delete();
                    }
                }
            }
        }

        void ApplyInstructorsReadAccessPermissions(AssignmentFolder folder)
        {
            foreach (SlkUser instructor in assignmentProperties.Instructors)
            {
                folder.ApplyPermission(instructor.SPUser, SPRoleType.Reader);
            }
        }

        void ApplyAssignmentPermission(SPUser user, SPRoleType learnerPermissions, SPRoleType instructorPermissions, bool removeObserverPermissions)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite spSite = new SPSite(assignmentProperties.SPSiteGuid))
                {
                    using (SPWeb spWeb = spSite.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        DropBox dropBox = new DropBox(spWeb);
                        AssignmentFolder assignmentFolder = dropBox.GetAssignmentFolder(assignmentProperties);

            Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("ApplyAssignmentPermission {0}", assignmentFolder == null);
                        if (assignmentFolder != null)
                        {
                            // Get the learner sub folder
                            AssignmentFolder learnerSubFolder = assignmentFolder.FindLearnerFolder(user);
            Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("ApplyAssignmentPermission learnerSubFolder {0}", learnerSubFolder == null);

                            //For Course Manager assignments, if the folder is not created yet
                            if (learnerSubFolder == null)
                            {
                                learnerSubFolder = assignmentFolder.CreateLearnerAssignmentFolder(user);
                            }
                            
                            // Apply learner permissions
                            assignmentFolder.RemovePermissions(user);
                            if (learnerPermissions == SPRoleType.None)
                            {
                                learnerSubFolder.RemovePermissions(user);
                            }
                            else
                            {
            Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("ApplyAssignmentPermission apply permission {0}", learnerPermissions);
                                assignmentFolder.ApplyPermission(user, SPRoleType.Reader);
                                learnerSubFolder.ApplyPermission(user, learnerPermissions);
                            }

                            // Apply instructor permissions
            Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("ApplyAssignmentPermission iterate instructors");
                            foreach (SlkUser instructor in assignmentProperties.Instructors)
                            {
            Microsoft.SharePointLearningKit.WebControls.SlkError.Debug("ApplyAssignmentPermission apply instructor permission {0} {1}", instructorPermissions, instructor.SPUser.Name);
                                learnerSubFolder.RemovePermissions(instructor.SPUser);
                                learnerSubFolder.ApplyPermission(instructor.SPUser, instructorPermissions);
                            }

                            if (removeObserverPermissions)
                            {
                                learnerSubFolder.RemoveObserverPermission();
                            }
                        }
                    }
                }
            });
        }

        void ApplySubmittedPermissions(AssignmentFolder assignmentFolder, AssignmentFolder learnerSubFolder)
        {
            // IF the assignment is auto return, the learner will still be able to view the drop box assignment files
            // otherwise, learner permissions will be removed from the learner's subfolder in the Drop Box document library
            if (assignmentProperties.AutoReturn == false)
            {
                assignmentFolder.RemovePermissions(CurrentUser);
                learnerSubFolder.RemovePermissions(CurrentUser);
            }

            // Grant instructors contribute permission on learner subfolder
            foreach (SlkUser instructor in assignmentProperties.Instructors)
            {
                if (instructor.SPUser == null)
                {
                    throw new SafeToDisplayException(string.Format(CultureInfo.CurrentUICulture, AppResources.DropBoxManagerUploadFilesNoInstructor, instructor.Name));
                }
                learnerSubFolder.RemovePermissions(instructor.SPUser);
                learnerSubFolder.ApplyPermission(instructor.SPUser, SPRoleType.Contributor);
            }

        }

#endregion private methods

#region public static methods
#endregion public static methods

#region static methods
        /// <summary>Generates the edit javascript script.</summary>
        /// <param name="fileUrl">The url of the file.</param>
        /// <param name="web">The web the file is in.</param>
        /// <returns>The script.</returns>
        public static string EditJavascript(string fileUrl, SPWeb web)
        {
            //string script = "return DispEx(this,event,'TRUE','FALSE','TRUE','','0','SharePoint.OpenDocuments','','','', '21','0','0','0x7fffffffffffffff');return false;";
            string script = "editDocumentWithProgID2('{0}', '', 'SharePoint.OpenDocuments','0','{1}','0');";
            return string.Format(CultureInfo.InvariantCulture, script, Microsoft.SharePoint.Utilities.SPHttpUtility.UrlPathEncode(fileUrl, false), web.Url);
        }

        static string GenerateFolderName(AssignmentProperties properties)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", GetDateOnly(properties.StartDate), properties.Title.Trim(), properties.Id.GetKey());
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

        /// <summary>Dumps debug messages.</summary>
        public static void Debug(string message, params object[] arguments)
        {
            /*
            try
            {
                using (System.Web.Hosting.HostingEnvironment.Impersonate())
                {
                    using (StreamWriter writer = new StreamWriter("c:\\temp\\dropBox.log", true))
                    {
                        message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + message;
                        writer.WriteLine(message, arguments);
                    }
                }
            }
            catch (Exception e)
            {
                Microsoft.SharePointLearningKit.WebControls.SlkError.WriteToEventLog(e);
            }
            */
        }

    }

#region AssignmentFile
    /// <summary>A file in an assignment.</summary>
    public struct AssignmentFile
    {
        string name;
        string url;

        /// <summary>The file's Url.</summary>
        public string Url
        {
            get { return url ;}
        }
        /// <summary>The file's Name.</summary>
        public string Name
        {
            get { return name ;}
        }
        /// <summary>Initializes a new instance of <see cref="AssignmentFile"/>.</summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="url">The file's url.</param>
        public AssignmentFile(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
#endregion AssignmentFile

#region AssignmentUpload
    /// <summary>An uploaded file for an assignment.</summary>
    public struct AssignmentUpload
    {
        string name;
        Stream stream;
        /// <summary>The file's contents.</summary>
        public Stream Stream
        {
            get { return stream ;}
        }
        /// <summary>The name of the file.</summary>
        public string Name
        {
            get { return name ;}
        }
        /// <summary>Initializes a new instance of <see cref="AssignmentUpload"/>.</summary>
        public AssignmentUpload(string name, Stream inputStream)
        {
            this.name = name;
            this.stream = inputStream;
        }
    }
#endregion AssignmentUpload
}
