using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.SharePointLearningKit.WebControls;
using Microsoft.SharePointLearningKit.Localization;
using System.Globalization;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    public class FilesUploadPage : SlkAppBasePage
    {
        #region Controls Declaration

        protected HtmlInputFile uploadFile1, uploadFile2, uploadFile3, uploadFile4, uploadFile5;
        protected Label lblMessage;
        protected Button btnOK, btnCancel;
        protected ErrorBanner errorBanner;
        protected Panel contentPanel = new Panel();
        protected Panel pnlOldFiles = new Panel();
        protected Literal pageTitle;
        protected Label pageTitleInTitlePage;
        protected Label documentUpload;
        protected Label documentUploadDescription;
        protected Label name;

        #endregion

        #region Private Variables

        private string m_sourceUrl;
        private string m_validFileExtensions;

        private Guid m_learnerAssignmentGuidId = Guid.Empty;
        private LearnerAssignmentProperties m_currentLearnerAssignmentProperties = null;
        private AssignmentProperties m_currentAssignmentProperties = null;
        private LearnerAssignmentState m_currentAssignmentStatus;

        private DropBoxManager m_dropBoxMgr = new DropBoxManager();
        private string m_dropBoxDocLibName;
        private SPList m_dropBoxDocLib = null;
        private string m_assignmentFolderName = null;
        private SPListItem m_assignmentFolder = null;

        #endregion

        #region Private Properties

        // <summary>
        // Gets the value of the "Source" query parameter, the URL of the source page.
        // </summary>
        private string SourceUrl
        {
            get
            {
                if (string.IsNullOrEmpty(m_sourceUrl))
                {
                    QueryString.Get("Source", out m_sourceUrl, false);
                }

                return m_sourceUrl;
            }
        }

        // <summary>
        // Gets the valid file extensions specified at the site collection configuration.
        // </summary>
        private string ValidFileExtensions
        {
            get
            {
                if (string.IsNullOrEmpty(m_validFileExtensions))
                {
                    SlkSPSiteMapping mapping = SlkSPSiteMapping.GetMapping(CurrentAssignmentProperties.SPSiteGuid);
                    m_validFileExtensions = mapping.DropBoxFilesExtensions;
                }

                return m_validFileExtensions;
            }
        }

        // <summary>
        // Gets the value of the "LearnerAssignmentId" query parameter.
        // </summary>
        private Guid LearnerAssignmentGuid
        {
            get
            {
                if (m_learnerAssignmentGuidId.Equals(Guid.Empty) == true)
                {
                    Guid id;
                    QueryString.Parse(QueryStringKeys.LearnerAssignmentId, out id);

                    m_learnerAssignmentGuidId = id;
                }

                return m_learnerAssignmentGuidId;
            }
        }

        // <summary>
        // Gets current Learner Assignment Properties 
        // </summary>
        private LearnerAssignmentProperties CurrentLearnerAssignmentProperties
        {
            get
            {
                if (m_currentLearnerAssignmentProperties == null)
                {
                    m_currentLearnerAssignmentProperties = SlkStore.GetLearnerAssignmentProperties(LearnerAssignmentGuid, SlkRole.Learner);
                }
                return m_currentLearnerAssignmentProperties;
            }
            set
            {
                m_currentLearnerAssignmentProperties = value;
            }
        }

        // <summary>
        // Gets current Assignment Properties 
        // </summary>
        private AssignmentProperties CurrentAssignmentProperties
        {
            get
            {
                if (m_currentAssignmentProperties == null)
                {
                    m_currentAssignmentProperties = SlkStore.GetAssignmentProperties(CurrentLearnerAssignmentProperties.AssignmentId, SlkRole.Learner);
                }
                return m_currentAssignmentProperties;
            }
        }

        // <summary>
        // Gets current Assignment Status 
        // </summary>
        private LearnerAssignmentState CurrentAssignmentStatus
        {
            get
            {
                m_currentAssignmentStatus = CurrentLearnerAssignmentProperties.Status;
                return m_currentAssignmentStatus;
            }
        }

        // <summary>
        // Gets the name of the drop box document library  
        // </summary>
        private String DropBoxDocLibName
        {
            get
            {
                if (String.IsNullOrEmpty(m_dropBoxDocLibName))
                {
                    m_dropBoxDocLibName = AppResources.DropBoxDocLibName;
                }
                return m_dropBoxDocLibName;
            }
        }

        // <summary>
        // Gets current Assignment Drop Box list
        // </summary>
        private SPList DropBoxDocLib
        {
            get
            {
                AppResources.Culture = LocalizationManager.GetCurrentCulture();
                if (m_dropBoxDocLib == null)
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        using (SPSite site = new SPSite(CurrentAssignmentProperties.SPSiteGuid))
                        {
                            using (SPWeb web = site.OpenWeb(CurrentAssignmentProperties.SPWebGuid))
                            {
                                m_dropBoxDocLib = web.Lists[DropBoxDocLibName];
                                if (m_dropBoxDocLib == null)
                                {
                                    errorBanner.AddError(ErrorType.Error, AppResources.DropBoxUnavailableError);
                                }
                            }
                        }
                    });
                }
                return m_dropBoxDocLib;
            }
        }

        // <summary>
        // Gets current Assignment folder name
        // </summary>
        private string AssignmentFolderName
        {
            get
            {
                if (m_assignmentFolderName == null)
                {
                    m_assignmentFolderName = CurrentAssignmentProperties.Title + " " + m_dropBoxMgr.GetDateOnly(CurrentAssignmentProperties.DateCreated);
                }
                return m_assignmentFolderName;
            }
        }

        // <summary>
        // Gets current Assignment folder
        // </summary>
        private SPListItem AssignmentFolder
        {
            get
            {
                if (m_assignmentFolder == null)
                {
                    m_assignmentFolder = m_dropBoxMgr.GetAssignmentFolder(DropBoxDocLib, AssignmentFolderName);
                    if (m_assignmentFolder == null)
                    {
                        //To handle assignments created by course manager
                        AssignmentItemIdentifier assignmentItemIdentifier =
                                                                SlkStore.GetLearnerAssignmentProperties(
                                                                LearnerAssignmentGuid,
                                                                SlkRole.Learner).AssignmentId;

                        AssignmentProperties assignmentProperties =
                                                        SlkStore.GetAssignmentPropertiesForCurrentLearner(
                                                        assignmentItemIdentifier);

                        try
                        {
                            DropBoxManager dropBoxMgr = new DropBoxManager();

                            string assignmentFolderName = (assignmentProperties.Title + " " + dropBoxMgr.GetDateOnly(assignmentProperties.DateCreated)).Trim();

                            m_assignmentFolder = dropBoxMgr.CreateAssignmentFolderForCourseManagerLearner(DropBoxDocLibName, assignmentProperties, assignmentFolderName);

                        }
                        catch (Exception ex)
                        {
                            AppResources.Culture = LocalizationManager.GetCurrentCulture();

                            if (ex.Message == AppResources.SLKFeatureNotActivated)
                            {
                                // Deletes the assignment
                                SlkStore.DeleteAssignment(assignmentItemIdentifier);
                                //Log the Exception 
                                errorBanner.Clear();
                                errorBanner.AddException(ex);
                            }
                            else
                            {
                                //Log the Exception 
                                errorBanner.Clear();
                                errorBanner.AddException(ex);
                            }
                        }
                        
                    }
                }
                return m_assignmentFolder;
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                AppResources.Culture = LocalizationManager.GetCurrentCulture();

                pageTitle.Text = AppResources.FilesUploadPageTitle;
                pageTitleInTitlePage.Text = AppResources.FilesUploadPageTitleInTitle;
                documentUpload.Text = AppResources.FilesUploadDocumentUpload;
                documentUploadDescription.Text = AppResources.FilesUploadDocumentUploadDescription;
                name.Text = AppResources.FilesUploadName;

                contentPanel = new Panel();
                contentPanel.Visible = false;
                errorBanner.Clear();

                if (CurrentAssignmentStatus == LearnerAssignmentState.Completed || CurrentAssignmentStatus == LearnerAssignmentState.Final)
                {
                    contentPanel.Visible = false;
                    errorBanner.AddError(ErrorType.Error, AppResources.FilesUploadAssIsInaccessible);
                }
                else
                {
                    DisplayLastAssignmentAttempt(AssignmentFolderName);
                }

                lblMessage.Text = AppResources.FilesUploadAssInaccessible;
            }
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {
            AppResources.Culture = LocalizationManager.GetCurrentCulture();

            int uploadedFilesCount = 0;
            HtmlInputFile[] allPageUploadFiles = new HtmlInputFile[] { uploadFile1, uploadFile2, uploadFile3, uploadFile4, uploadFile5 };
            SPFile fileUploaded = null;
            SPListItem subFolder = null;

            SPUser learner = SPContext.Current.Web.CurrentUser;

            // Get the learner subfolder
            subFolder = m_dropBoxMgr.GetSubFolder(AssignmentFolder, learner.Name);
            if (subFolder == null)
                subFolder = m_dropBoxMgr.GetSubFolder(AssignmentFolder, learner.LoginName);

            if (subFolder != null)
            {
                // Reset IsLatest property to false for all old submitted files in the learner subfolder for current assignment
                // this is used to keep track of files submitted at the learner last assignment attempt
                m_dropBoxMgr.ResestIsLatestFiles(subFolder);
            }
            else
            {
                AssignmentItemIdentifier assignmentItemIdentifier =
                                                                SlkStore.GetLearnerAssignmentProperties(
                                                                LearnerAssignmentGuid,
                                                                SlkRole.Learner).AssignmentId;

                AssignmentProperties assignmentProperties =
                                                SlkStore.GetAssignmentPropertiesForCurrentLearner(
                                                assignmentItemIdentifier);

                DropBoxManager dropBoxMgr = new DropBoxManager();

                string assignmentFolderName = (assignmentProperties.Title + " " + dropBoxMgr.GetDateOnly(assignmentProperties.DateCreated)).Trim();

                subFolder = dropBoxMgr.CreateAssignmentSubFolderForCourseManagerLearner(assignmentProperties, AssignmentFolder);
            }

            if (IsValidFilesExtensions(allPageUploadFiles))
            {
                for (int i = 0; i < 5; i++)
                {
                    HttpPostedFile postedFile = allPageUploadFiles[i].PostedFile;
                    if (postedFile != null && postedFile.FileName != "")
                    {
                        uploadedFilesCount++;
                        // Get the file name
                        string fileName = GetFileName(postedFile.FileName);

                        fileUploaded = m_dropBoxMgr.UploadFile(fileName, postedFile.InputStream, subFolder.Folder,
                            LearnerAssignmentGuid, CurrentLearnerAssignmentProperties);
                    }
                }

                if (uploadedFilesCount > 0)
                {
                    // Trigger Submit assignment for the instructor for grading
                    TriggerSubmitWorkFlow();

                    //Redirect to the SLk ALWP Page
                    HttpContext.Current.Response.Write(
                    "<script>var x = '" + AppResources.FilesUploadPageSuccessMsg + "';" + 
                    "var url = '" + SPWeb.Url+ "';" + 
                    "if (x != ''){alert(x);window.location=url;};</script>");

                   
                }
                else
                {
                    //contentPanel.Visible = false;
                    errorBanner.Clear();
                    errorBanner.AddError(ErrorType.Info, AppResources.FilesUploadOrCancel);
                    errorBanner.Visible = true;
                }
            }
            else
            {
                contentPanel.Visible = false;
                errorBanner.Clear();
                errorBanner.AddError(ErrorType.Error, string.Format(, AppResources.FilesUploadInvalidExtensions, ValidFileExtensions.Replace(';', ',')));
                errorBanner.Visible = true;
            }
        }

        private bool IsValidFilesExtensions(HtmlInputFile[] allPageUploadFiles)
        {
            // Validate files extensions
            for (int i = 0; i < 5; i++)
            {
                HttpPostedFile postedFile = allPageUploadFiles[i].PostedFile;
                if (postedFile != null && postedFile.FileName != "")
                {
                    // Get the file extension
                    string fileExtension = GetFileExtension(postedFile.FileName);

                    if (!IsValidFileExtension(fileExtension))
                        return false;
                }
            }
            return true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Go back to the homepage
            Response.Redirect(SPWeb.ServerRelativeUrl, true);
        }

        private void DisplayLastAssignmentAttempt(string assFolderName)
        {
            HyperLink lnkOldFile = null;

            SPListItem subFolder = null;

            if (AssignmentFolder != null)
            {
                // Get the learner subFolder
                subFolder = m_dropBoxMgr.GetSubFolder(AssignmentFolder, SPContext.Current.Web.CurrentUser.Name);
                if(subFolder == null)
                    subFolder = m_dropBoxMgr.GetSubFolder(AssignmentFolder, SPContext.Current.Web.CurrentUser.LoginName);

                if (subFolder != null && subFolder.Folder.Files.Count > 0)
                {
                    foreach (SPFile file in subFolder.Folder.Files)
                    {
                        if (file.Item["IsLatest"] != null)
                        {
                            if ((bool)file.Item["IsLatest"] == true)
                            {
                                lnkOldFile = new HyperLink();
                                lnkOldFile.Text = file.Name;
                                lnkOldFile.NavigateUrl = file.ServerRelativeUrl;
                                pnlOldFiles.Controls.Add(lnkOldFile);
                                pnlOldFiles.Controls.Add(new LiteralControl("<br/>"));
                            }
                        }
                    }
                }
            }
        }

        private string GetFileName(string fullName)
        {
            string[] fullNameParts = fullName.Split('\\');
            string fileNameWithExtension = fullNameParts[fullNameParts.Length - 1];
            return fileNameWithExtension;
        }

        private string GetFileExtension(string fullName)
        {
            string fileExtension = fullName.Remove(0, fullName.LastIndexOf('.') + 1);
            return fileExtension;
        }

        private bool IsValidFileExtension(string fileExtension)
        {
            bool valid = true;
            string[] allFilesExtensions = ValidFileExtensions.Split(';');
                 
            for (int x = 0; x < allFilesExtensions.Length; x++)
            {
                if (allFilesExtensions[x].ToLower() == fileExtension.ToLower())
                {
                    valid = true;
                    return valid;
                }
                else
                    valid = false;
            }

            return valid;
        }

        private void TriggerSubmitWorkFlow()
        {
            try
            {
                try
                {
                    SlkStore.ChangeLearnerAssignmentState(LearnerAssignmentGuid, LearnerAssignmentState.Completed);
                }
                catch (InvalidOperationException)
                {
                    // state transition isn't supported
                    // errorBanner.AddException(new SafeToDisplayException(AppResources.LobbyCannotChangeState));
                }
                // Clear the object so it will refresh from the database
                CurrentLearnerAssignmentProperties = null;
            }
            catch (Exception)
            {
                // TODO: handle exception
                // any exceptions here will be handled in PreRender
            }
        }
    }
}





