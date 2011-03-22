using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
using System.Globalization;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>The upload files page.</summary>
    public class FilesUploadPage : SlkAppBasePage
    {
        #region Controls Declaration

        /// <summary>The upload files controls.</summary>
        protected HtmlInputFile uploadFile1, uploadFile2, uploadFile3, uploadFile4, uploadFile5;
        /// <summary>The message label.</summary>
        protected Label lblMessage;
        /// <summary>The page buttons.</summary>
        protected Button btnOK, btnCancel;
        /// <summary>The error banner.</summary>
        protected ErrorBanner errorBanner;
        /// <summary>The content panel.</summary>
        protected Panel contentPanel = new Panel();
        /// <summary>The old files panel.</summary>
        protected Panel pnlOldFiles = new Panel();
        /// <summary>The page title.</summary>
        protected Literal pageTitle;
        /// <summary>The page title.</summary>
        protected Label pageTitleInTitlePage;
        /// <summary>The label for the document upload.</summary>
        protected Label documentUpload;
        /// <summary>The description for the document upload.</summary>
        protected Label documentUploadDescription;
        /// <summary>The name label.</summary>
        protected Label name;

        #endregion

        #region Private Variables

        private string m_sourceUrl;

        private Guid m_learnerAssignmentGuidId = Guid.Empty;
        private LearnerAssignmentProperties m_currentLearnerAssignmentProperties = null;
        private AssignmentProperties m_currentAssignmentProperties = null;

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
                    m_sourceUrl = QueryString.ParseString("Source");
                }

                return m_sourceUrl;
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
                    m_learnerAssignmentGuidId = QueryString.ParseGuid(QueryStringKeys.LearnerAssignmentId);
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

        #endregion

        /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnInit"/>.</summary>
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                pageTitle.Text = AppResources.FilesUploadPageTitle;
                pageTitleInTitlePage.Text = AppResources.FilesUploadPageTitleInTitle;
                documentUpload.Text = AppResources.FilesUploadDocumentUpload;
                documentUploadDescription.Text = AppResources.FilesUploadDocumentUploadDescription;
                name.Text = AppResources.FilesUploadName;

                contentPanel = new Panel();
                contentPanel.Visible = false;
                errorBanner.Clear();

                if (CurrentLearnerAssignmentProperties.Status == LearnerAssignmentState.Completed || CurrentLearnerAssignmentProperties.Status == LearnerAssignmentState.Final)
                {
                    contentPanel.Visible = false;
                    errorBanner.AddError(ErrorType.Error, AppResources.FilesUploadAssIsInaccessible);
                }
                else
                {
                    DisplayLastAssignmentAttempt();
                }

                lblMessage.Text = AppResources.FilesUploadAssInaccessible;
            }
        }

        /// <summary>The OK button event handler.</summary>
        protected void btnOK_Click(object sender, EventArgs e)
        {
            SlkMemberships members = SlkStore.GetInstructorMemberships(SPWeb);
            CurrentAssignmentProperties.PopulateSPUsers(members);
            DropBoxManager dropBoxMgr = new DropBoxManager(CurrentAssignmentProperties);

            List<AssignmentUpload> uploadedFiles = new List<AssignmentUpload>();

            HtmlInputFile[] allPageUploadFiles = new HtmlInputFile[] { uploadFile1, uploadFile2, uploadFile3, uploadFile4, uploadFile5 };
            for (int i = 0; i < 5; i++)
            {
                HttpPostedFile postedFile = allPageUploadFiles[i].PostedFile;
                if (postedFile != null && postedFile.FileName != "")
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    uploadedFiles.Add(new AssignmentUpload(fileName, postedFile.InputStream));
                }
            }

            if (uploadedFiles.Count > 0)
            {
                try
                {
                    dropBoxMgr.UploadFiles(CurrentLearnerAssignmentProperties, uploadedFiles.ToArray());

                    //Redirect to the SLk ALWP Page
                    HttpContext.Current.Response.Write(
                    "<script>var x = '" + AppResources.FilesUploadPageSuccessMsg + "';" + 
                    "var url = '" + SPWeb.Url+ "';" + 
                    "if (x != ''){alert(x);window.location=url;};</script>");
                }
                catch (SafeToDisplayException exception)
                {
                    contentPanel.Visible = false;
                    errorBanner.Clear();
                    errorBanner.AddError(ErrorType.Error, exception.Message);
                    errorBanner.Visible = true;
                }
                catch (Exception exception)
                {
                    contentPanel.Visible = false;
                    errorBanner.Clear();
                    errorBanner.AddError(ErrorType.Error, exception.ToString());
                    errorBanner.Visible = true;
                }
            }
            else
            {
                errorBanner.Clear();
                errorBanner.AddError(ErrorType.Info, AppResources.FilesUploadOrCancel);
                errorBanner.Visible = true;
            }
        }

        /// <summary>The cancel button event handler.</summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Go back to the homepage
            Response.Redirect(SPWeb.ServerRelativeUrl, true);
        }

        private void DisplayLastAssignmentAttempt()
        {
            DropBoxManager dropBoxManager = new DropBoxManager(CurrentAssignmentProperties);
            AssignmentFile[] files = dropBoxManager.LastSubmittedFiles();

            foreach (AssignmentFile file in files)
            {
                HyperLink fileLink = new HyperLink();
                fileLink.Text = file.Name;
                fileLink.NavigateUrl = file.Url;
                pnlOldFiles.Controls.Add(fileLink);
                pnlOldFiles.Controls.Add(new LiteralControl("<br/>"));
            }
        }
    }
}





