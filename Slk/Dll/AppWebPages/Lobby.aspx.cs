/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Lobby.aspx.cs
//
// Code-behind for Lobby.aspx.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.LearningComponents.Frameset;
using Microsoft.SharePointLearningKit.Frameset;
using System.Threading;
using System.Configuration;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{

    /// <summary>
    /// Code-behind for Lobby.aspx.
    /// </summary>
    public class LobbyPage : SlkAppBasePage
    {
        #region Control Declarations
#pragma warning disable 1591
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblScoreValue;
        protected Label labelGradeValue;
        protected TableGridRow rowGrade;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStatusValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStartValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDueValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblCommentsValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblTitle;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDescription;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblSite;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblScore;
        protected Label labelGrade;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStatus;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStart;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblComments;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblAutoReturn;
        protected ErrorBanner errorBanner;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrComments;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrAutoReturn;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrSite;

        protected SlkButton slkButtonBegin;
        protected SlkButton slkButtonSubmit;
        protected SlkButton slkButtonDelete;
        protected SlkButton slkButtonReviewSubmitted;
        protected SlkButton slkButtonSubmitFiles;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk")]
        protected HyperLink lnkSite;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblSiteValue;
        protected Image infoImage;

        protected Literal pageTitle;
        protected Literal pageTitleInTitlePage;
        protected Literal pageDescription;

        protected Panel contentPanel;
#pragma warning restore 1591
        #endregion

        #region Private Variables
        string currentUrl;
        private Guid m_learnerAssignmentGuidId = Guid.Empty;
        private LearnerAssignmentProperties learnerAssignmentProperties;
        AssignmentFile assignmentFile;
        const string startQueryStringName = "start";

        /// <summary>
        /// Holds AssignmentProperties value.
        /// </summary>
        private AssignmentProperties assignmentProperties;
        /// <summary>
        /// The name of the drop box document library 
        /// </summary>
        private string m_dropBoxDocLibName;

        #endregion

        #region Private Properties

        string CurrentUrl
        {
            get
            {
                if (currentUrl == null)
                {
                    currentUrl = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, "_layouts/SharePointLearningKit/Lobby.aspx");
                    currentUrl = String.Format(CultureInfo.InvariantCulture, "{0}?{1}", currentUrl, Request.QueryString);
                }
                return currentUrl;
            }
        }

        /// <summary>
        /// Gets the value of the "LearnerAssignmentId" query parameter.
        /// </summary>
        private Guid LearnerAssignmentGuidId
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

        /// <summary>
        /// Gets the properties of the learner assignment being displayed by this page.
        /// </summary>
        private LearnerAssignmentProperties LearnerAssignmentProperties
        {
            get
            {
                if (learnerAssignmentProperties == null)
                {
                    LoadAssignmentObjects();
                }
                return learnerAssignmentProperties;
            }
            set
            {
                learnerAssignmentProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the general properties of the learner assignment being displayed by this page.
        /// </summary>
        private AssignmentProperties AssignmentProperties
        {
            get
            {
                if (this.assignmentProperties == null)
                {
                    LoadAssignmentObjects();
                }

                return this.assignmentProperties;
            }

            set
            {
                this.assignmentProperties = value;
            }
        }
        /// <summary>
        /// Gets the Drop Box document library name 
        /// </summary>
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

        #endregion

        #region OnPreRender
        /// <summary>
        ///  Overrides OnPreRender.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                // setting default title
                pageTitle.Text = AppResources.LobbyBeginAssignmentText;
                pageTitleInTitlePage.Text = AppResources.LobbyBeginAssignmentText;

                SetResourceText();

                LearnerAssignmentState learnerAssignmentStatus = LearnerAssignmentProperties.Status.Value;
                bool startAssignment = (Request.QueryString[startQueryStringName] == "true");
                bool initialViewForOfficeWebApps = false;

                if (AssignmentProperties.IsNonELearning && LearnerAssignmentProperties.Status == LearnerAssignmentState.NotStarted)
                {
                    if (AssignmentProperties.IsNoPackageAssignment)
                    {
                        startAssignment = true;
                    }
                    else
                    {
                        CopyDocumentToDropBox();

                        if (assignmentFile != null && assignmentFile.IsOffice2010File && SlkStore.Settings.DropBoxSettings.UseOfficeWebApps)
                        {
                            // If using office web apps need to change the status of the assignment as the begin button is just a url rather than a script 
                            // which opens the document in another application
                            startAssignment = true;
                            initialViewForOfficeWebApps = true;
                        }
                    }
                }
                else if (AssignmentProperties.IsNonELearning)
                {
                    FindDocumentUrl();
                }

                if (startAssignment && learnerAssignmentStatus == LearnerAssignmentState.NotStarted)
                {
                    LearnerAssignmentProperties.Start();
                    learnerAssignmentStatus = LearnerAssignmentProperties.Status.Value;
                }

                ClientScript.RegisterClientScriptBlock(this.GetType(), "lblStatusValue", "var lblStatusValue = \"" + lblStatusValue.ClientID + "\";", true);

                if (learnerAssignmentStatus == LearnerAssignmentState.Completed && AssignmentProperties.AutoReturn == true)
                {
                    // assignment was probably changed to be "auto-return" after this learner submitted it; we'll
                    // re-submit now to invoke the auto-return mechanism; note that we use
                    // LearnerAssignmentState.Completed instead of LearnerAssignmentState.Final because
                    // the latter would throw a security-related exception (learner's aren't allowed to move their
                    // learner assignments into Final state) -- using Completed works because
                    // SlkStore.ChangeLearnerAssignmentState performs auto-return even if the current state is
                    // LearnerAssignmentState.Completed
                    LearnerAssignmentProperties.Submit();
                    // Set the property to null so that it will refresh the next time it is referenced
                    LearnerAssignmentProperties = null;
                }

                lblTitle.Text = Server.HtmlEncode(AssignmentProperties.Title);
                lblDescription.Text = SlkUtilities.GetCrlfHtmlEncodedText(AssignmentProperties.Description);

                // for the assignment site, if the user doesn't have permission to view it
                // we'll catch the exception and hide the row
                bool previousValue = SPSecurity.CatchAccessDeniedException;
                SPSecurity.CatchAccessDeniedException = false;
                try
                {
                    using(SPSite assignmentSite = new SPSite(AssignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
                    {
                        using(SPWeb assignmentWeb = assignmentSite.OpenWeb(AssignmentProperties.SPWebGuid))
                        {

                            // If the assignment is in a different SPWeb redirect to it.
                            if (SPWeb.ID != assignmentWeb.ID)
                                Response.Redirect(SlkUtilities.UrlCombine(assignmentWeb.Url, Request.Path + "?" + Request.QueryString.ToString()));

                            lnkSite.Text = Server.HtmlEncode(assignmentWeb.Title);
                            lnkSite.NavigateUrl = assignmentWeb.ServerRelativeUrl;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    lblSiteValue.Text = AppResources.LobbyInvalidSite;
                    lblSiteValue.Visible = true;
                }
                catch (FileNotFoundException)
                {
                    lblSiteValue.Text = AppResources.LobbyInvalidSite;
                    lblSiteValue.Visible = true;
                }
                finally
                {
                    SPSecurity.CatchAccessDeniedException = previousValue;
                }

                if (LearnerAssignmentProperties.SuccessStatus == SuccessStatus.Passed
                    && LearnerAssignmentProperties.FinalPoints.HasValue
                    && LearnerAssignmentProperties.FinalPoints.Value == 0.0
                    )
                {
                    lblScoreValue.Text = string.Empty;
                }
                else
                {

                    if (learnerAssignmentStatus != LearnerAssignmentState.Final)
                    {
                        if (AssignmentProperties.PointsPossible.HasValue)
                        {
                            lblScoreValue.Text = PageCulture.Format(AppResources.LobbyPointsNoValuePointsPossible, AssignmentProperties.PointsPossible);
                        }
                        else
                        {
                            lblScoreValue.Text = PageCulture.Format(AppResources.LobbyPointsNoValue, LearnerAssignmentProperties.FinalPoints);
                        }
                    }
                    else
                    {
                        string finalPoints = AppResources.LobbyPointsNoValue;
                        if (LearnerAssignmentProperties.FinalPoints.HasValue)
                            finalPoints = LearnerAssignmentProperties.FinalPoints.Value.ToString(CultureInfo.CurrentCulture);

                        if (AssignmentProperties.PointsPossible.HasValue)
                            lblScoreValue.Text = PageCulture.Format(AppResources.LobbyPointsValuePointsPossible, finalPoints, AssignmentProperties.PointsPossible);
                        else
                            lblScoreValue.Text = PageCulture.Format(AppResources.LobbyPointsValue, finalPoints);
                    }
                }

                if (SlkStore.Settings.UseGrades)
                {
                    labelGradeValue.Text = LearnerAssignmentProperties.Grade;
                }
                else
                {
                    rowGrade.Visible = false;
                }

                lblStartValue.Text = PageCulture.Format(AppResources.LongDateShortTime, AssignmentProperties.StartDate);
                if (AssignmentProperties.DueDate.HasValue)
                    lblDueValue.Text = PageCulture.Format(AppResources.LongDateShortTime, AssignmentProperties.DueDate.Value);

                if (LearnerAssignmentProperties.InstructorComments.Length != 0)
                    lblCommentsValue.Text = SlkUtilities.GetCrlfHtmlEncodedText(LearnerAssignmentProperties.InstructorComments);
                else
                    tgrComments.Visible = false;

                lblStatusValue.Text = Server.HtmlEncode(SlkUtilities.GetLearnerAssignmentState(learnerAssignmentStatus));
                AssignmentView view = AssignmentView.Execute;
                switch (learnerAssignmentStatus)
                {
                    case LearnerAssignmentState.NotStarted:
                        SetUpForNotStarted();
                        break;

                    case LearnerAssignmentState.Active:
                        if (initialViewForOfficeWebApps)
                        {
                            SetUpForNotStarted();
                        }
                        else
                        {
                            SetUpForActive();
                        }
                        break;

                    case LearnerAssignmentState.Completed:
                        SetUpForCompleted();
                        break;

                    case LearnerAssignmentState.Final:
                        SetUpForFinal();
                        view = AssignmentView.StudentReview;
                        break;

                    default:
                        break;
                }

                SetUpSlkBeginButtonAction(view);

                // Set up delete button
                if (AssignmentProperties.CreatedById.Equals(LearnerAssignmentProperties.LearnerId) && !AssignmentProperties.HasInstructors)
                {
                    slkButtonDelete.Text = AppResources.LobbyDeleteAssignmentText;
                    slkButtonDelete.ToolTip = AppResources.LobbyDeleteToolTip;
                    slkButtonDelete.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');",
                        AppResources.LobbyDeleteMessage);
                    slkButtonDelete.ImageUrl = Constants.ImagePath + Constants.DeleteIcon;
                }
                else
                {
                    slkButtonDelete.Visible = false;
                }

                // Set up submit button
                if (AssignmentProperties.HasInstructors)
                {
                    slkButtonSubmit.Text = AppResources.LobbySubmitText;
                    slkButtonSubmit.ToolTip = AppResources.LobbySubmitToolTip;
                    slkButtonSubmit.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');", AppResources.LobbySubmitMessage);
                    slkButtonSubmit.ImageUrl = Constants.ImagePath + Constants.SubmitIcon;
                }
                else
                {
                    slkButtonSubmit.Text = AppResources.LobbyMarkasCompleteText;
                    slkButtonSubmit.ToolTip = AppResources.LobbyMarkasCompleteToolTip;
                    slkButtonSubmit.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');", AppResources.LobbyMarkasCompleteMessage);
                    slkButtonSubmit.ImageUrl = Constants.ImagePath + Constants.MarkCompleteIcon;
                }

                slkButtonBegin.AccessKey = AppResources.LobbyBeginAccessKey;
                slkButtonDelete.AccessKey = AppResources.LobbyDeleteAccessKey;
                slkButtonSubmit.AccessKey = AppResources.LobbySubmitAccessKey;

                tgrAutoReturn.Visible = AssignmentProperties.AutoReturn;

                contentPanel.Visible = true;

            }
            catch (InvalidOperationException ex)
            {
                SlkStore.LogException(ex);
                errorBanner.AddException(SlkStore, new SafeToDisplayException(AppResources.LobbyInvalidLearnerAssignmentId, LearnerAssignmentGuidId.ToString()));
                contentPanel.Visible = false;
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which needs to be rethrown
                throw;
            }
            catch (Exception exception)
            {
                errorBanner.AddException(SlkStore, exception);
                contentPanel.Visible = false;
            }
        }

        void SetUpSlkBeginButtonAction(AssignmentView view)
        {
            if (AssignmentProperties.IsNoPackageAssignment)
            {
                slkButtonBegin.Visible = false;
            }
            else
            {
                string openFrameset = String.Format(CultureInfo.InvariantCulture, "javascript:SlkOpenFramesetWindow('Frameset/Frameset.aspx?{0}={1}&{2}={3}');",
                                                    FramesetQueryParameter.SlkView, view, FramesetQueryParameter.LearnerAssignmentId, LearnerAssignmentGuidId);
                if (AssignmentProperties.IsNonELearning)
                {

                    if (assignmentFile == null)
                    {
                        slkButtonBegin.NavigateUrl = string.Format(CultureInfo.InvariantCulture, "{0}window.location='{1}&{2}=true';", openFrameset, CurrentUrl, startQueryStringName);
                    }
                    else
                    {
                        SetupFileAction(assignmentFile, slkButtonBegin, true);
                    }
                }
                else
                {
                    slkButtonBegin.OnClientClick = openFrameset;
                    slkButtonBegin.NavigateUrl = openFrameset;
                }
                slkButtonBegin.ImageUrl = Constants.ImagePath + Constants.NewDocumentIcon;
            }
        }

        void SetupFileAction(AssignmentFile file, SlkButton button, bool includeReload)
        {
            
            DropBoxManager dropBoxMgr = new DropBoxManager(AssignmentProperties);

            DropBoxEditMode editMode = DropBoxEditMode.Edit;
            switch (LearnerAssignmentProperties.Status)
            {
                case LearnerAssignmentState.Completed:
                case LearnerAssignmentState.Final:
                    editMode = DropBoxEditMode.View;
                    break;
            }

            DropBoxEditDetails editDetails = dropBoxMgr.GenerateDropBoxEditDetails(file, SPWeb, editMode, Page.Request.RawUrl);

            string script = editDetails.OnClick;
            if (string.IsNullOrEmpty(script))
            {
                if (includeReload)
                {
                    script = string.Format(CultureInfo.InvariantCulture, "window.location='{0}&{1}=true';", CurrentUrl, startQueryStringName);
                }
            }
            else
            {
                if (LearnerAssignmentProperties.Status == LearnerAssignmentState.NotStarted && includeReload)
                {
                    script = string.Format(CultureInfo.InvariantCulture, "{0}window.location='{1}&{2}=true';return false;", script, CurrentUrl, startQueryStringName);
                }
                else
                {
                    script = script + "return false;";
                }

            }

            button.OnClientClick = script;
            button.NavigateUrl = editDetails.Url;
        }

        void SetUpSubmitButtons(bool enableSubmitFiles)
        {
            //Check if non e-learning and enable the appropriate button accordingly
            if (AssignmentProperties.RootActivityId == null)
            {
                slkButtonSubmitFiles.Visible = true;
                slkButtonSubmitFiles.Text = AppResources.LobbySubmitFilesText;
                slkButtonSubmitFiles.ToolTip = AppResources.LobbySubmitFilesToolTip;
                slkButtonSubmitFiles.ImageUrl = Constants.ImagePath + Constants.NewDocumentIcon;
                slkButtonSubmitFiles.AccessKey = AppResources.LobbySubmitFilesAccessKey;
                slkButtonSubmitFiles.Enabled = enableSubmitFiles;
                if (enableSubmitFiles)
                {
                    string url = string.Format(CultureInfo.InvariantCulture, 
                            "{0}/_layouts/SharePointLearningKit/FilesUploadPage.aspx?LearnerAssignmentId={1}", SPWeb.Url, LearnerAssignmentGuidId.ToString());

                    slkButtonSubmitFiles.NavigateUrl = url;
                    slkButtonSubmitFiles.Target = "_self";
                }

                slkButtonSubmit.Visible = true;
            }
            else
            {
                slkButtonSubmitFiles.Visible = false;
            }

            slkButtonSubmit.Enabled = enableSubmitFiles;
        }

        void SetUpForFinal()
        {
            slkButtonBegin.Text = AppResources.LobbyReviewAssignmentText;
            slkButtonBegin.ToolTip = AppResources.LobbyReviewAssignmentToolTip;

            pageTitle.Text = AppResources.LobbyReviewAssignmentText;
            pageTitleInTitlePage.Text = AppResources.LobbyReviewAssignmentText;

            //Check if non-elearning content
            if (AssignmentProperties.RootActivityId == null)
            {
                slkButtonReviewSubmitted.Text = AppResources.LobbyReviewSubmittedText;
                slkButtonReviewSubmitted.ToolTip = AppResources.LobbyReviewSubmittedToolTip;
                slkButtonReviewSubmitted.ImageUrl = Constants.ImagePath + Constants.NewDocumentIcon;
                slkButtonReviewSubmitted.Visible = true;

                DropBoxManager manager = new DropBoxManager(AssignmentProperties);

                AssignmentFile[] assignmentFiles = manager.LastSubmittedFiles();

                if (assignmentFiles.Length != 1)
                {
                    string onClickUrl = string.Format("{0}/_layouts/SharePointLearningKit/SubmittedFiles.aspx?LearnerAssignmentId={1}", SPWeb.Url, LearnerAssignmentGuidId.ToString());
                    slkButtonReviewSubmitted.OnClientClick = String.Format(CultureInfo.InvariantCulture, "window.open('{0}','popupwindow','width=400,height=300,scrollbars,resizable'); ", onClickUrl);
                }
                else
                {
                    SetupFileAction(assignmentFiles[0], slkButtonReviewSubmitted, false);
                }

            }

            SetUpSubmitButtons(false);
            slkButtonSubmit.Visible = false;
        }
                        
        void SetUpForCompleted()
        {
            slkButtonBegin.Text = AppResources.LobbyReviewAssignmentText;
            slkButtonBegin.ToolTip = AppResources.LobbyReviewAssignmentToolTipCompleted;
            slkButtonBegin.Enabled = false;
            pageTitle.Text = AppResources.LobbyReviewAssignmentText;
            pageTitleInTitlePage.Text = AppResources.LobbyReviewAssignmentText;

            SetUpSubmitButtons(false);

            slkButtonSubmit.Visible = false;

            slkButtonReviewSubmitted.Visible = false;
        }

        void SetUpForNotStarted()
        {
            slkButtonBegin.Text = AppResources.LobbyBeginAssignmentText;
            slkButtonBegin.ToolTip = AppResources.LobbyBeginAssignmentToolTip;

            pageTitle.Text = AppResources.LobbyBeginAssignmentText;
            pageTitleInTitlePage.Text = AppResources.LobbyBeginAssignmentText;

            SetUpSubmitButtons(false);

            slkButtonReviewSubmitted.Visible = false;
        }

        void SetUpForActive()
        {
            slkButtonBegin.Text = AppResources.LobbyResumeAssignmentText;
            slkButtonBegin.ToolTip = AppResources.LobbyResumeAssignmentToolTip;
            pageTitle.Text = AppResources.LobbyResumeAssignmentText;
            pageTitleInTitlePage.Text = AppResources.LobbyResumeAssignmentText;

            SetUpSubmitButtons(true);

            slkButtonReviewSubmitted.Visible = false;
        }

        #endregion

        /// <summary>
        /// slkButtonSubmit Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void slkButtonSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    LearnerAssignmentProperties.Submit();
                }
                catch (InvalidOperationException)
                {
                    // state transition isn't supported
                    errorBanner.AddException(SlkStore, new SafeToDisplayException(AppResources.LobbyCannotChangeState));
                }
                // Clear the object so it will refresh from the database
                LearnerAssignmentProperties = null;
            }
            catch (LearningStoreConstraintViolationException exception)
            {
                // any exceptions here will be handled in PreRender
                errorBanner.AddException(SlkStore, new SafeToDisplayException(AppResources.LobbySubmitException));
                SlkStore.LogException(exception);
            }
        }

        /// <summary>
        /// slkButtonDelete Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void slkButtonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Delete corresponding assignment folder from the drop box if exists
                if (AssignmentProperties.PackageFormat == null)
                {
                    DropBoxManager dropBoxMgr = new DropBoxManager(AssignmentProperties);
                    dropBoxMgr.DeleteAssignmentFolder();
                }

                SlkStore.DeleteAssignment(AssignmentProperties.Id);
                Response.Redirect(SPWeb.ServerRelativeUrl, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception exception)
            {
                errorBanner.AddException(SlkStore, new SafeToDisplayException(AppResources.LobbyDeleteException));
                SlkStore.LogException(exception);
            }
        }

#region private methods
        void LoadAssignmentObjects()
        {
            assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuidId, SlkRole.Learner);
            learnerAssignmentProperties = assignmentProperties.Results[0];
        }

        private void SetResourceText()
        {
            pageDescription.Text = AppResources.LobbyPageDescription;

            lblSite.Text = AppResources.LobbylblSite;
            lblScore.Text = AppResources.LobbylblScore;
            labelGrade.Text = AppResources.LobbyLabelGrade;
            lblStatus.Text = AppResources.LobbylblStatus;
            lblStart.Text = AppResources.LobbylblStart;
            lblDue.Text = AppResources.LobbylblDue;
            lblComments.Text = AppResources.LobbylblComments;
            lblAutoReturn.Text = AppResources.LobbylblAutoReturn;
            infoImage.AlternateText = AppResources.SlkErrorTypeInfoToolTip;
            ClientScript.RegisterClientScriptBlock(this.GetType(), "SlkWindowAlreadyOpen",
            string.Format(CultureInfo.InvariantCulture, "var SlkWindowAlreadyOpen = \"{0}\";", AppResources.LobbyWindowAlreadyOpen), true);
        }

        /// <summary> Generates the url to view the submitted files. </summary>
        private string GenerateUrlForAssignmentReview()
        {
            DropBoxManager manager = new DropBoxManager(AssignmentProperties);
            AssignmentFile[] assignmentFiles = manager.LastSubmittedFiles();

            if (assignmentFiles.Length != 1)
            {
                return string.Format("{0}/_layouts/SharePointLearningKit/SubmittedFiles.aspx?LearnerAssignmentId={1}", SPWeb.Url, LearnerAssignmentGuidId.ToString());
            }
            else
            {
                return assignmentFiles[0].Url;
            }
        }

        void CopyDocumentToDropBox()
        {
           DropBoxManager manager = new DropBoxManager(AssignmentProperties);
           assignmentFile = manager.CopyFileToDropBox(); 
        }

        void FindDocumentUrl()
        {
           DropBoxManager manager = new DropBoxManager(AssignmentProperties);
           AssignmentFile[] files = manager.LastSubmittedFiles();
           if (files.Length > 0)
           {
               assignmentFile = files[0];
           }
        }
#endregion private methods
    }
}
