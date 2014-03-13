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
        private List<CheckBox> includes = new List<CheckBox>();

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

        /// <summary>See <see cref="Control.CreateChildControls"/>.</summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Page.Form.Enctype = "multipart/form-data";
            LoadLastAssignmentAttempt();
        }

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

                lblMessage.Text = PageCulture.Resources.FilesUploadAssInaccessible;
            }
        }

        /// <summary>The OK button event handler.</summary>
        protected void btnOK_Click(object sender, EventArgs e)
        {

            List<AssignmentUpload> uploadedFiles = FindUploadedFiles();
            if (uploadedFiles.Count > 0)
            {
                try
                {
                    List<int> filesToKeep = new List<int>();
                    foreach (CheckBox check in includes)
                    {
                        if (check.Checked)
                        {
                            if (check.ID.Length > 5)
                            {
                                int fileId = int.Parse(check.ID.Substring(5), CultureInfo.InvariantCulture);
                                filesToKeep.Add(fileId);
                            }
                        }
                    }

                    LearnerAssignmentProperties.UploadFilesAndSubmit(uploadedFiles.ToArray(), filesToKeep.ToArray());

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
        private void LoadLastAssignmentAttempt()
        {
            DropBoxManager dropBoxManager = new DropBoxManager(AssignmentProperties);
            AssignmentFile[] files = dropBoxManager.LastSubmittedFiles();

            if (files.Length > 0)
            {
                LiteralControl literal = new LiteralControl(string.Format("<table><tr><th>{0}</th><th>{1}</th></tr>", PageCulture.Resources.IncludeTitle, PageCulture.Resources.CtrlLabelUploadDocumentName));
                pnlOldFiles.Controls.Add(literal);

                foreach (AssignmentFile file in files)
                {
                    pnlOldFiles.Controls.Add(new LiteralControl("<tr><td>"));

                    CheckBox check = new CheckBox();
                    check.ID = "check" + file.Id.ToString(CultureInfo.InvariantCulture);
                    check.Checked = false;
                    includes.Add(check);
                    pnlOldFiles.Controls.Add(check);

                    pnlOldFiles.Controls.Add(new LiteralControl("</td><td>"));

                    HyperLink fileLink = new HyperLink();
                    fileLink.Text = file.Name;

                    DropBoxEditMode editMode = DropBoxEditMode.Edit;
                    DropBoxEditDetails editDetails = dropBoxManager.GenerateDropBoxEditDetails(file, SPWeb, editMode, Page.Request.RawUrl);
                    fileLink.NavigateUrl = editDetails.Url;
                    if (string.IsNullOrEmpty(editDetails.OnClick) == false)
                    {
                        fileLink.Attributes.Add("onclick", editDetails.OnClick + "return false;");
                    }

                    pnlOldFiles.Controls.Add(fileLink);

                    pnlOldFiles.Controls.Add(new LiteralControl("</td></tr>"));
                }

                pnlOldFiles.Controls.Add(new LiteralControl("</table>"));
            }

        }

        private void LoadAssignmentProperties()
        {
            assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.Learner);
            learnerAssignmentProperties = assignmentProperties.Results[0];
        }

        private List<AssignmentUpload> FindUploadedFiles()
        {
            List<AssignmentUpload> uploadedFiles = new List<AssignmentUpload>();

            foreach (string item in Request.Files)
            {
                HttpPostedFile file = Request.Files[item];
                if (file != null && file.ContentLength > 0)
                {
                    FileInfo info = new FileInfo(file.FileName);
                    uploadedFiles.Add(new AssignmentUpload(info.Name, file.InputStream));
                }
            }

            return uploadedFiles;
        }
#endregion private methods
    }
}





