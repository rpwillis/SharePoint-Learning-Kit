/* Copyright (c) 2009. All rights reserved. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.ObjectModel;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>
    /// Contains code-behind for SubmittedFiles.aspx
    /// </summary>
    public class CommentedFiles : SlkAppBasePage
    {
        #region Control Declarations

        /// <summary>The title of the page.</summary>
        protected Literal pageTitle;
        /// <summary>The title of the page.</summary>
        protected Literal pageTitleInTitlePage;
        /// <summary>The description of the page.</summary>
        protected Literal pageDescription;

        /// <summary>The error banner.</summary>
        protected ErrorBanner errorBanner;

        /// <summary>The contents panel.</summary>
        protected Panel contentPanel;

        /// <summary>Page header label</summary>
        protected Label headerMessage;
        /// <summary>The file upload control.</summary>
        protected FileUpload commentedFileUpload;
        /// <summary>The upload button.</summary>
        protected Button UploadButton;
        /// <summary>The status label.</summary>
        protected Label UploadStatusLabel;
        /// <summary>The error label.</summary>
        protected Label UploadErrorStatusLabel;
        
        #endregion


        #region Private Variables
        /// <summary>Holds assignmentID value. </summary>
        private long? m_assignmentId;
        /// <summary>Holds Assignment Properties. </summary>
        private AssignmentProperties m_assignmentProperties;

        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the AssignmentId Query String value.
        /// </summary>
        private long? AssignmentId
        {
            get
            {
                if (m_assignmentId == null)
                {
                    m_assignmentId = QueryString.ParseLong(QueryStringKeys.AssignmentId, null);
                }
                return m_assignmentId;
            }
        }
        /// <summary>
        /// Gets the AssignmentItemIdentifier value.
        /// </summary>
        private AssignmentItemIdentifier AssignmentItemIdentifier
        {
            get
            {
                AssignmentItemIdentifier assignmentItemId = null;
                if (AssignmentId != null)
                {
                    assignmentItemId = new AssignmentItemIdentifier(AssignmentId.Value);
                }
                return assignmentItemId;
            }
        }
        /// <summary>
        /// Gets the AssignmentProperties.
        /// </summary>
        private AssignmentProperties AssignmentProperties
        {
            get
            {
                return m_assignmentProperties;
            }
        }
        #endregion

        /// <summary>
        /// Checks is uploaded file is zipped
        /// </summary>
        /// <param name="fileName">the name of the uploade file</param>
        /// <returns>a boolean indicating the check result</returns>
        bool IsFileZipped(string fileName)
        {
            return fileName.ToLower().EndsWith(".zip");
        }

        bool isStudent(ZipEntry entry, Dictionary<string, List<SPFile>> originalFiles)
        {
            return originalFiles.ContainsKey(entry.Name);
        }

        /// <summary>The event handler for clicking on the upload button.</summary>
        protected void UploadButton_Click(object sender, EventArgs e)
        {
            string tempFolderPath = Path.GetTempPath();


            if (commentedFileUpload.HasFile)
            {
                if (IsFileZipped(commentedFileUpload.FileName))
                {
                    /*
                    AssignmentProperties assignmentProperties = SlkStore.GetAssignmentProperties(AssignmentItemIdentifier, SlkRole.Instructor);
                    DropBoxManager dropBox = new DropBoxManager(assignmentProperties);
                    public Dictionary<string, List<SPFile>> originalFiles = dropBox.AllFiles()

                    // Ensure that the temporary folder is unique
                    string temporaryFolder = Path.Combine(tempFolderPath, Guid.NewGuid().ToString());
                    string temporaryPath = Path.Combine(temporaryFolder, commentedFileUpload.FileName);
                    try
                    {
                        using (ZipFile zip = new ZipFile(commentedFileUpload.FileContent))
                        {
                            bool atStudentFolders = (zip.Entries.Count > 1);
                            foreach (ZipEntry entry in zip)
                            {
                                if (entry.IsDirectory)
                                {
                                    if (IsStudent(entry, originalFiles))
                                }
                            }
                        }
                        commentedFileUpload.SaveAs(temporaryFolder);

                        //Extract the uploaded file in the windows temp directory
                        ExtractUploadedFile(temporaryPath);

                        //Upload the extracted student assignment files to the corresponding folders
                        //on the dropbox document list.
                        List<AssignmentUploadTracker> assignmentUploadTrackers = UploadCommentedFiles(temporaryPath);

                        if (assignmentUploadTrackers !=null && assignmentUploadTrackers.Count != 0)
                        {
                            //Displays the upload commented files status
                            DisplayUploadStatus(assignmentUploadTrackers);
                        }
                        else
                        {
                            //The zip file does not contain any commented files
                            UploadErrorStatusLabel.Text = string.Empty;
                            UploadStatusLabel.Text = string.Empty;
                            UploadErrorStatusLabel.Text = AppResources.CommentedFilesExtractionFailed;
                        }
                    }
                    finally
                    {
                        if (Directory.Exists(temporaryFolder))
                        {
                            Directory.Delete(temporaryFolder, true);
                        }
                    }
                    */
                }
                else
                {
                    //Invaild file extention
                    UploadErrorStatusLabel.Text = string.Empty;
                    UploadStatusLabel.Text = string.Empty;
                    UploadErrorStatusLabel.Text = AppResources.CommentedFilesInvalidExtenstion;
                }
            }
            else
            {
                //No file was uploaded
                UploadErrorStatusLabel.Text = string.Empty;
                UploadStatusLabel.Text = string.Empty;
                UploadErrorStatusLabel.Text = AppResources.CommentedFilesNoFileAttached;
            }
        }

       
        /// <summary>
        /// This function is used to extract the uploaded compressed file
        /// which conatin the commented files
        /// </summary>
        /// <param name="zippedFilePath">The path of the zipped file</param>
        /// <param name="outputPath">Out put directory path</param>
        private void ExtractUploadedFile(string zippedFilePath, string outputPath)
        {
            try
            {
                FastZip fileFastZip = new FastZip();
                fileFastZip.ExtractZip(zippedFilePath, outputPath, "");
            }
            catch (Exception ex)
            {
                this.contentPanel.Visible = false;
                this.errorBanner.Clear();
                this.errorBanner.AddException(ex);
                
            }
        }

        /// <summary>
        /// This function uploads the instructor commented files 
        /// to the corresponding student folder on the dropbox list.
        /// </summary>
        /// <param name="zippedFileName">The name of the zipped file</param>
        /// <param name="tempFolderPath">Temp directory path</param>
        private List<AssignmentUploadTracker> UploadCommentedFiles(string zippedFileName, string tempFolderPath)
        {
            //Keeps track of the uploading progress;
            List<AssignmentUploadTracker> uploadTrackers = new List<AssignmentUploadTracker>();
            AssignmentUploadTracker currentUploadTracker;

            try
            {
                AssignmentProperties assignmentProperties = SlkStore.GetAssignmentProperties(AssignmentItemIdentifier, SlkRole.Instructor);

                using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
                {
                    using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        SPList dropBoxList = web.Lists[AppResources.DropBoxDocLibName];

                        //Gets the name of the assignment folder.
                        string assignmentFolderName = zippedFileName.Split('.')[0];

                        /* Searching for the assignment folder using the naming format: "AssignmentTitle AssignmentCreationDate" 
                             * (This is the naming format defined in AssignmentProperties.aspx.cs page) */
                        SPQuery query = new SPQuery();
                        query.Folder = dropBoxList.RootFolder;
                        query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + assignmentFolderName + "</Value></Eq></Where>";
                        SPListItemCollection assignmentFolders = dropBoxList.GetItems(query);

                        try
                        {
                            if (assignmentFolders.Count == 0)
                            {
                                // The assignment folder does not exits on the dropbox list.
                                throw new Exception(assignmentFolderName + " " + AppResources.CommentedFilesNoAssignmentFolderException);
                            }
                        }
                        catch (Exception assignmetNotFoundEx)
                        {
                            this.contentPanel.Visible = false;
                            this.errorBanner.Clear();
                            errorBanner.AddError(ErrorType.Error, assignmetNotFoundEx.Message);
                            return null;
                        }

                        //Gets the assignment SP folder on the dropbox list.
                        SPFolder assignmentFolder = assignmentFolders[0].Folder;

                        string[] commentedFilesPaths;
                        string extractedFolderPath = tempFolderPath + "\\" + assignmentFolderName;
                        DirectoryInfo extractedAssignmentfolder = new DirectoryInfo(extractedFolderPath);
                        Stream assignmentFileStream;
                        byte[] assignmentContents;
                        string studentFolderName = string.Empty;
                        string[] studentFolders;
                        string assignmentFileName = string.Empty;
                        int uploadFilescounter = 0;
                        int ignoredFilescounter = 0;
                        int missedFilescounter = 0;
                        SPFileCollection orgFiles;
                        bool fileFound = false;
                        bool commentedFileFound = false;

                        if (extractedAssignmentfolder.Exists)
                        {
                            //Gets an array of the students assignment folders paths.
                            studentFolders = Directory.GetDirectories(extractedFolderPath);


                            //loop on each student folder to upload the instuctor commented files.
                            foreach (string studentFolderPath in studentFolders)
                            {
                                //keeps track of the current upload status
                                currentUploadTracker = new AssignmentUploadTracker();

                                //Gets the student assignment folder
                                studentFolderName = studentFolderPath.Split('\\')[studentFolderPath.Split('\\').Length - 1];

                                //Tracking the upload operation: save the student folder name
                                currentUploadTracker.StudentFolderName = studentFolderName;

                                //Gets the student assignment SP folder of the current assignment
                                query = new SPQuery();
                                query.Folder = assignmentFolder;
                                query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + studentFolderName + "</Value></Eq></Where>";
                                SPListItemCollection assignmentSubFolders = dropBoxList.GetItems(query);

                                if (assignmentSubFolders.Count == 0)
                                {
                                    //Tracking the upload operation: The student folder does not exists
                                    currentUploadTracker.IsCompleted = false;

                                    //throw new Exception(AppResources.SubmittedFilesNoAssignmentSubFolderException);
                                }
                                else
                                {
                                    SPFolder assignmentSubFolder = assignmentSubFolders[0].Folder;

                                    //Gets the commented files of the current student assignment
                                    commentedFilesPaths = Directory.GetFiles(studentFolderPath);

                                    //Gets the assignment original files
                                    orgFiles = assignmentSubFolder.Files;

                                    //Reset tracking counters
                                    uploadFilescounter = 0;
                                    ignoredFilescounter = 0;
                                    missedFilescounter = 0;

                                    //loop on each file to upload it to the student assignment SP folder
                                    foreach (string assignmentFilePath in commentedFilesPaths)
                                    {
                                        fileFound = false;

                                        foreach (SPFile assignmentFile in orgFiles)
                                        {
                                            //Gets ths assignment file name
                                            assignmentFileName = assignmentFilePath.Split('\\')[assignmentFilePath.Split('\\').Length - 1];

                                            //checks that file exists on ths SP assignment folder
                                            if (assignmentFile.Name == assignmentFileName)
                                            {
                                                fileFound = true;
                                            }

                                        }
                                        //The file exists
                                        if (fileFound)
                                        {
                                            uploadFilescounter++;

                                            assignmentFileStream = File.OpenRead(assignmentFilePath);
                                            assignmentContents = new byte[assignmentFileStream.Length];
                                            assignmentFileStream.Read(assignmentContents, 0, (int)assignmentFileStream.Length);
                                            assignmentFileStream.Close();

                                            web.AllowUnsafeUpdates = true;

                                            //Uploads the current file.
                                            assignmentSubFolder.Files.Add(assignmentFileName, assignmentContents, true);
                                        }
                                        else
                                        {
                                            ignoredFilescounter++;
                                        }

                                    }
                                    //loop on each file on the student assignment SP folder
                                    // to check if there files missed
                                    foreach (SPFile assignmentFile in orgFiles)
                                    {
                                        if ((bool)assignmentFile.Item["IsLatest"])
                                        {
                                            commentedFileFound = false;

                                            foreach (string assignmentFilePath in commentedFilesPaths)
                                            {
                                                //Gets ths assignment file name
                                                assignmentFileName = assignmentFilePath.Split('\\')[assignmentFilePath.Split('\\').Length - 1];

                                                //checks that file exists on ths SP assignment folder
                                                if (assignmentFile.Name == assignmentFileName)
                                                {
                                                    commentedFileFound = true;
                                                }

                                            }
                                            //The file does not exists
                                            if (!commentedFileFound) missedFilescounter++;
                                        }

                                    }
                                    //Tracking the upload operation: The number of files missed
                                    currentUploadTracker.MissedFilesCount = missedFilescounter;

                                    //Tracking the upload operation: The number of files uploaded
                                    currentUploadTracker.UploadedFilesCount = uploadFilescounter;

                                    //Tracking the upload operation: The number of files ignored.
                                    currentUploadTracker.IgnoredFilesCount = ignoredFilescounter;

                                    //Tracking the upload operation: The upload operation is completed.
                                    currentUploadTracker.IsCompleted = true;
                                }

                                uploadTrackers.Add(currentUploadTracker);
                            }

                            bool studentFolderFound = false;
                            AssignmentUploadTracker currentTracker;

                            //checks if a sttudent folder was not uploaded by the teacher.
                            if (assignmentFolder.SubFolders.Count != studentFolders.Length)
                            {
                                //loops on each student folder under the assignment folder
                                foreach (SPFolder studentFolder in assignmentFolder.SubFolders)
                                {
                                    studentFolderFound = false;

                                    //loops on each student folder upload by the teacher
                                    foreach (string studentFolderPath in studentFolders)
                                    {
                                        //Gets the student assignment folder name
                                        studentFolderName = studentFolderPath.Split('\\')[studentFolderPath.Split('\\').Length - 1];
                                        if (studentFolder.Name == studentFolderName) studentFolderFound = true;

                                    }
                                    //student folder was not uploaded
                                    if (!studentFolderFound)
                                    {
                                        //tracking the student.
                                        currentTracker = new AssignmentUploadTracker();
                                        currentTracker.StudentFolderName = studentFolder.Name;
                                        currentTracker.IsCompleted = false;
                                        currentTracker.UploadedFilesCount = -1;

                                        uploadTrackers.Add(currentTracker);
                                    }
                                }
                            }
                            web.Update();
                            web.Dispose();

                            //Delete the compressed file and extracted folder
                            //from the windows temp directory
                            File.Delete(tempFolderPath + "\\" + zippedFileName);
                            Directory.Delete(tempFolderPath + "\\" + assignmentFolderName, true);
                        }

                    }
                }
            }


            catch (Exception ex)
            {
                this.contentPanel.Visible = false;
                this.errorBanner.Clear();
                this.errorBanner.AddException(ex);
               
            }

            return uploadTrackers;
        }

        #region OnPreRender

        /// <summary>
        ///  Over rides OnPreRender.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            string exceptionMessage = string.Empty;

            try
            {
                this.SetResourceText();

                AssignmentProperties assignmentProperties = SlkStore.GetAssignmentProperties(AssignmentItemIdentifier, SlkRole.Instructor);

                using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
                {
                    using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                    {
                        if (!(SlkStore.IsInstructor(web)))
                        {
                            exceptionMessage = AppResources.CommentedFilesNoAccessException;

                            throw new SafeToDisplayException(exceptionMessage);
                        }
                    }
                }

                //Checks if all learners have completed the assignment.
                if (!IsPostBack)
                {
                    bool isAssignmentsCompleted = true;
                    ReadOnlyCollection<GradingProperties> learnersGradingCollection = SlkStore.GetGradingProperties(AssignmentItemIdentifier, out m_assignmentProperties);
                    foreach (GradingProperties learnerGrading in learnersGradingCollection)
                    {
                        if (learnerGrading.Status != LearnerAssignmentState.Completed)
                        {
                            isAssignmentsCompleted = false;
                        }
                    }
                    if (!isAssignmentsCompleted)
                    {
                        exceptionMessage = AppResources.CommentedFilesAssignmnetsNotCompleted;

                        throw new SafeToDisplayException(exceptionMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                this.contentPanel.Visible = false;
                this.errorBanner.Clear();
                this.errorBanner.AddError(ErrorType.Error, ex.Message);

            }

        }

       #endregion

        private void DisplayUploadStatus(List<AssignmentUploadTracker> uploadTrackers)
        {
            try
            {
                UploadStatusLabel.Text = string.Empty;
                UploadErrorStatusLabel.Text = string.Empty;

                foreach (AssignmentUploadTracker uploadTracker in uploadTrackers)
                {
                    if (!uploadTracker.IsCompleted)
                    {
                        if (uploadTracker.UploadedFilesCount == -1)
                        {
                            UploadErrorStatusLabel.Text +=
                                AppResources.CommentedFilesNoFilesUploaded + " " + uploadTracker.StudentFolderName + "<br>";
                        }
                        else
                        {
                            UploadErrorStatusLabel.Text +=
                                AppResources.CommentedFilesUploadFailed + uploadTracker.StudentFolderName + "<br>";
                        }
                    }
                    else
                    {
                        //UploadStatusLabel.Text +=
                        //    uploadTracker.UploadedFilesCount + " " + AppResources.CommentedFilesUploadMessage + " "
                        //    + uploadTracker.StudentFolderName; 

                        ////check if files were ignored
                        //if (uploadTracker.IgnoredFilesCount != 0)
                        //{
                        //    UploadStatusLabel.Text +=
                        //        ", " + uploadTracker.IgnoredFilesCount + " " + AppResources.CommentedFilesIgnoredMessage;

                        //}
                        UploadStatusLabel.Text += uploadTracker.StudentFolderName + ": "
                            + uploadTracker.UploadedFilesCount + " "
                            + AppResources.CommentedFilesUploadMessage + ", "

                            + uploadTracker.MissedFilesCount + " "
                            + AppResources.CommentedFilesMissedMessage + " "
                            + AppResources.CommentedFilesStatusMessage + " "

                            + uploadTracker.IgnoredFilesCount + " "
                            + AppResources.CommentedFilesIgnoredMessage;

                        UploadStatusLabel.Text += "<br>";
                    }
                }
            }
            catch (Exception displayEx)
            {
                this.contentPanel.Visible = false;
                this.errorBanner.Clear();
                this.errorBanner.AddException(displayEx);
            }
        }

        #region SetResourceText
        /// <summary>
        ///  Set the Control Text from Resource File.
        /// </summary>
        private void SetResourceText()
        {
            AppResources.Culture = LocalizationManager.GetCurrentCulture();
            this.pageTitle.Text = AppResources.CommentedFilesPageTitle;
            this.pageTitleInTitlePage.Text = AppResources.CommentedFilesTitleinTitlePage;
            this.pageDescription.Text = AppResources.CommentedFilesPageDescription;
        }
        
       #endregion
            

    }
    
    class AssignmentUploadTracker
    {
        string _studentFolderName;

        public string StudentFolderName
        {
            get { return _studentFolderName; }
            set { _studentFolderName = value; }
        }
        int _ignoredFilesCount;

        public int IgnoredFilesCount
        {
            get { return _ignoredFilesCount; }
            set { _ignoredFilesCount = value; }
        }

        int _uploadedFilesCount;

        public int UploadedFilesCount
        {
            get { return _uploadedFilesCount; }
            set { _uploadedFilesCount = value; }
        }
        int _missedFilesCount;

        public int MissedFilesCount
        {
            get { return _missedFilesCount; }
            set { _missedFilesCount = value; }
        }

        bool _IsCompleted;

        public bool IsCompleted
        {
            get { return _IsCompleted; }
            set { _IsCompleted = value; }
        }
    }
}
