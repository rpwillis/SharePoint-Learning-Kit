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


        private Guid m_learnerAssignmentGuidId = Guid.Empty;
        private LearnerAssignmentProperties learnerAssignmentProperties;
        private AssignmentProperties assignmentProperties;

        #endregion

        #region Private Properties

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
        private LearnerAssignmentProperties LearnerAssignmentProperties
        {
            get
            {
                if (learnerAssignmentProperties == null)
                {
                    LoadAssignmentProperties();
                }
                return learnerAssignmentProperties;
            }
        }

        // <summary>
        // Gets current Assignment Properties 
        // </summary>
        private AssignmentProperties AssignmentProperties
        {
            get
            {
                if (assignmentProperties == null)
                {
                    LoadAssignmentProperties();
                }
                return assignmentProperties;
            }
        }

        #endregion

        /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnInit"/>.</summary>
        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                pageTitle.Text = PageCulture.Resources.FilesUploadPageTitle;
                pageTitleInTitlePage.Text = PageCulture.Resources.FilesUploadPageTitleInTitle;
                documentUpload.Text = PageCulture.Resources.FilesUploadDocumentUpload;
                documentUploadDescription.Text = PageCulture.Resources.FilesUploadDocumentUploadDescription;
                name.Text = PageCulture.Resources.FilesUploadName;

                contentPanel = new Panel();
                contentPanel.Visible = false;
                errorBanner.Clear();

                if (LearnerAssignmentProperties.Status == LearnerAssignmentState.Completed || LearnerAssignmentProperties.Status == LearnerAssignmentState.Final)
                {
                    contentPanel.Visible = false;
                    errorBanner.AddError(ErrorType.Error, PageCulture.Resources.FilesUploadAssIsInaccessible);
                }
                else
                {
                    DisplayLastAssignmentAttempt();
                }

                lblMessage.Text = PageCulture.Resources.FilesUploadAssInaccessible;
            }
        }

        /// <summary>The OK button event handler.</summary>
        protected void btnOK_Click(object sender, EventArgs e)
        {
            SlkMemberships memberships = new SlkMemberships(null, null, null);
            memberships.FindAllSlkMembers(SPWeb, SlkStore, true);

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
                    LearnerAssignmentProperties.UploadFilesAndSubmit(uploadedFiles.ToArray());

                    //Redirect to the SLk ALWP Page
                    HttpContext.Current.Response.Write(
                    "<script>var x = '" + PageCulture.Resources.FilesUploadPageSuccessMsg + "';" + 
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
                errorBanner.AddError(ErrorType.Info, PageCulture.Resources.FilesUploadOrCancel);
                errorBanner.Visible = true;
            }
        }

        /// <summary>The cancel button event handler.</summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Go back to the homepage
            Response.Redirect(SPWeb.ServerRelativeUrl, true);
        }

#region private methods
        private void DisplayLastAssignmentAttempt()
        {
            DropBoxManager dropBoxManager = new DropBoxManager(AssignmentProperties);
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

        void LoadAssignmentProperties()
        {
            assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.Learner);
            learnerAssignmentProperties = assignmentProperties.Results[0];
        }
#endregion private methods
    }
}





