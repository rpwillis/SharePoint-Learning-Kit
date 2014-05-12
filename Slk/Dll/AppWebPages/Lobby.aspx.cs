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
        protected TableGridRow RowGrade;
        protected TableGridRow RowScore;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStatusValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStartValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDueValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblCommentsValue;
        protected Label lblLearnerCommentsValue;
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
        protected Label lblLearnerComments;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblAutoReturn;
        protected ErrorBanner errorBanner;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrLearnerComments;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrComments;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrAutoReturn;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrSite;
        System.Web.UI.WebControls.BulletedList navigationList;
        protected Label ConfirmWhatNext;
        protected TextBox LearnerComments;

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
        protected Panel NextPanel;
#pragma warning restore 1591
        #endregion

        #region Private Variables
        bool startAssignment;
        bool initialViewForOfficeWebApps;
        string currentUrl;
        private Guid m_learnerAssignmentGuidId = Guid.Empty;
        private LearnerAssignmentProperties learnerAssignmentProperties;
        AssignmentFile assignmentFile;
        const string startQueryStringName = "slkStart";

        /// <summary>
        /// Holds AssignmentProperties value.
        /// </summary>
        private AssignmentProperties assignmentProperties;

        #endregion

        #region Private Properties

        string CurrentUrl
        {
            get
            {
                if (currentUrl == null)
                {
                    currentUrl = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, Constants.SlkUrlPath, "Lobby.aspx");
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
        #endregion

#region protected methods
        /// <summary>See <see cref="Control.CreateChildControls"/>.</summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            navigationList = new System.Web.UI.WebControls.BulletedList();
            navigationList.ID = "navigationList";
            NextPanel.Controls.Add(navigationList);
        }

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
                pageTitle.Text = PageCulture.Resources.LobbyBeginAssignmentText;
                pageTitleInTitlePage.Text = PageCulture.Resources.LobbyBeginAssignmentText;

                SetResourceText();

                LearnerAssignmentState learnerAssignmentStatus = LearnerAssignmentProperties.Status.Value;
                startAssignment = (Request.QueryString[startQueryStringName] == "true");

                if (AssignmentProperties.IsNonELearning)
                {
                    HandleNonELearningAssignment();
                }

                if (startAssignment && learnerAssignmentStatus == LearnerAssignmentState.NotStarted)
                {
                    LearnerAssignmentProperties.Start();
                    learnerAssignmentStatus = LearnerAssignmentProperties.Status.Value;
                }

                ClientScript.RegisterClientScriptBlock(this.GetType(), "lblStatusValue", "var lblStatusValue = \"" + lblStatusValue.ClientID + "\";", true);

                StringBuilder clientScript = new StringBuilder();
                string submittedJavascript = string.Format("slkSubmittedUrl = '{0}{1}SubmittedFiles.aspx?LearnerAssignmentId=';", SPWeb.Url, Constants.SlkUrlPath);
                clientScript.AppendLine(submittedJavascript);

                string sourceUrl = string.Format("slkSourceUrl = '&source={0}';", HttpUtility.UrlEncode(Page.Request.RawUrl));
                clientScript.AppendLine(sourceUrl);

                ClientScript.RegisterClientScriptBlock(this.GetType(), "openSubmittedFiles", clientScript.ToString(), true);

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

                SetUpDisplayValues(learnerAssignmentStatus);
                SetUpForAssignmentState(learnerAssignmentStatus);

                tgrAutoReturn.Visible = AssignmentProperties.AutoReturn;
                contentPanel.Visible = true;

            }
            catch (InvalidOperationException ex)
            {
                SlkStore.LogException(ex);
                errorBanner.AddException(SlkStore, new SafeToDisplayException(PageCulture.Resources.LobbyInvalidLearnerAssignmentId, LearnerAssignmentGuidId.ToString()));
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
                    LearnerAssignmentProperties.SaveLearnerComment(LearnerComments.Text);
                }
                catch (InvalidOperationException)
                {
                    // state transition isn't supported
                    errorBanner.AddException(SlkStore, new SafeToDisplayException(PageCulture.Resources.LobbyCannotChangeState));
                }

                // Clear the object so it will refresh from the database
                LearnerAssignmentProperties = null;
            }
            catch (LearningStoreConstraintViolationException exception)
            {
                // any exceptions here will be handled in PreRender
                errorBanner.AddException(SlkStore, new SafeToDisplayException(PageCulture.Resources.LobbySubmitException));
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
                errorBanner.AddException(SlkStore, new SafeToDisplayException(PageCulture.Resources.LobbyDeleteException));
                SlkStore.LogException(exception);
            }
        }
#endregion protected methods

#region private methods
        private void SetUpForAssignmentState(LearnerAssignmentState learnerAssignmentStatus)
        {
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

            SetUpButtons(view);
        }

        private void HandleNonELearningAssignment()
        {
            if (LearnerAssignmentProperties.Status == LearnerAssignmentState.NotStarted)
            {
                if (AssignmentProperties.IsNoPackageAssignment)
                {
                    startAssignment = true;
                }
                else
                {
                    try
                    {
                        CopyDocumentToDropBox();
                    }
                    catch (SPException)
                    {
                        // Sometimes fails first time
                        try
                        {
                            CopyDocumentToDropBox();
                        }
                        catch (SPException exception)
                        {
                            SlkStore.LogException(exception);
                        }
                    }

                    if (assignmentFile != null && assignmentFile.IsOwaCompatible && SlkStore.Settings.DropBoxSettings.UseOfficeWebApps)
                    {
                        // If using office web apps need to change the status of the assignment as the begin button is just a url rather than a script 
                        // which opens the document in another application
                        startAssignment = true;
                        initialViewForOfficeWebApps = true;
                    }
                }
            }
            else
            {
                if (AssignmentProperties.IsNoPackageAssignment == false)
                {
                    FindDocumentUrl();
                }
            }
        }

        private void SetUpDisplayValues(LearnerAssignmentState learnerAssignmentStatus)
        {
            lblTitle.Text = Server.HtmlEncode(AssignmentProperties.Title);
            lblDescription.Text = SlkUtilities.ClickifyLinks(SlkUtilities.GetCrlfHtmlEncodedText(AssignmentProperties.Description));

            SetUpAssignmentSiteLink();
            SetUpScoreAndGradeDisplayValues(learnerAssignmentStatus);

            SPTimeZone timeZone = SPWeb.RegionalSettings.TimeZone;
            lblStartValue.Text = FormatDateForDisplay(timeZone.UTCToLocalTime(AssignmentProperties.StartDate));
            if (AssignmentProperties.DueDate.HasValue)
            {
                lblDueValue.Text = FormatDateForDisplay(timeZone.UTCToLocalTime(AssignmentProperties.DueDate.Value));
            }

            if (LearnerAssignmentProperties.InstructorComments.Length != 0)
            {
                lblCommentsValue.Text = SlkUtilities.GetCrlfHtmlEncodedText(LearnerAssignmentProperties.InstructorComments);
            }
            else
            {
                tgrComments.Visible = false;
            }

            lblStatusValue.Text = Server.HtmlEncode(SlkUtilities.GetLearnerAssignmentState(learnerAssignmentStatus));
        }

        private void SetUpButtons(AssignmentView view)
        {
            SetUpSlkBeginButtonAction(view);

            // Set up delete button
            if (AssignmentProperties.CreatedById.Equals(LearnerAssignmentProperties.LearnerId) && !AssignmentProperties.HasInstructors)
            {
                slkButtonDelete.Text = PageCulture.Resources.LobbyDeleteAssignmentText;
                slkButtonDelete.ToolTip = PageCulture.Resources.LobbyDeleteToolTip;
                slkButtonDelete.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');",
                        PageCulture.Resources.LobbyDeleteMessage);
                slkButtonDelete.ImageUrl = Constants.ImagePath + Constants.DeleteIcon;
            }
            else
            {
                slkButtonDelete.Visible = false;
            }

            // Set up submit button
            if (AssignmentProperties.HasInstructors)
            {
                slkButtonSubmit.Text = PageCulture.Resources.LobbySubmitText;
                slkButtonSubmit.ToolTip = PageCulture.Resources.LobbySubmitToolTip;
                slkButtonSubmit.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');", PageCulture.Resources.LobbySubmitMessage);
                slkButtonSubmit.ImageUrl = Constants.ImagePath + Constants.SubmitIcon;
            }
            else
            {
                slkButtonSubmit.Text = PageCulture.Resources.LobbyMarkasCompleteText;
                slkButtonSubmit.ToolTip = PageCulture.Resources.LobbyMarkasCompleteToolTip;
                slkButtonSubmit.OnClientClick = PageCulture.Format("javascript: return confirm('{0}');", PageCulture.Resources.LobbyMarkasCompleteMessage);
                slkButtonSubmit.ImageUrl = Constants.ImagePath + Constants.MarkCompleteIcon;
            }

            slkButtonBegin.AccessKey = PageCulture.Resources.LobbyBeginAccessKey;
            slkButtonDelete.AccessKey = PageCulture.Resources.LobbyDeleteAccessKey;
            slkButtonSubmit.AccessKey = PageCulture.Resources.LobbySubmitAccessKey;
        }

        private void SetUpAssignmentSiteLink()
        {
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
                        {
                            Response.Redirect(SlkUtilities.UrlCombine(assignmentWeb.Url, Request.Path + "?" + Request.QueryString.ToString()));
                        }

                        lnkSite.Text = Server.HtmlEncode(assignmentWeb.Title);
                        lnkSite.NavigateUrl = assignmentWeb.ServerRelativeUrl;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                lblSiteValue.Text = PageCulture.Resources.LobbyInvalidSite;
                lblSiteValue.Visible = true;
            }
            catch (FileNotFoundException)
            {
                lblSiteValue.Text = PageCulture.Resources.LobbyInvalidSite;
                lblSiteValue.Visible = true;
            }
            finally
            {
                SPSecurity.CatchAccessDeniedException = previousValue;
            }

        }

        /// <summary>Set the score and grade display values.</summary>
        private void SetUpScoreAndGradeDisplayValues(LearnerAssignmentState learnerAssignmentStatus)
        {
            if (AssignmentProperties.HidePoints)
            {
                RowScore.Visible = false;
            }
            else
            {
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
                            lblScoreValue.Text = PageCulture.Format(PageCulture.Resources.LobbyPointsNoValuePointsPossible, AssignmentProperties.PointsPossible);
                        }
                        else
                        {
                            lblScoreValue.Text = PageCulture.Format(PageCulture.Resources.LobbyPointsNoValue, LearnerAssignmentProperties.FinalPoints);
                        }
                    }
                    else
                    {
                        string finalPoints = PageCulture.Resources.LobbyPointsNoValue;
                        if (LearnerAssignmentProperties.FinalPoints.HasValue)
                            finalPoints = LearnerAssignmentProperties.FinalPoints.Value.ToString(CultureInfo.CurrentCulture);

                        if (AssignmentProperties.PointsPossible.HasValue)
                            lblScoreValue.Text = PageCulture.Format(PageCulture.Resources.LobbyPointsValuePointsPossible, finalPoints, AssignmentProperties.PointsPossible);
                        else
                            lblScoreValue.Text = PageCulture.Format(PageCulture.Resources.LobbyPointsValue, finalPoints);
                    }
                }
            }

            if (SlkStore.Settings.UseGrades)
            {
                labelGradeValue.Text = LearnerAssignmentProperties.Grade;
            }
            else
            {
                RowGrade.Visible = false;
            }

        }

        private void SetUpSlkBeginButtonAction(AssignmentView view)
        {
            if (AssignmentProperties.IsNoPackageAssignment)
            {
                slkButtonBegin.Visible = false;
            }
            else
            {
                string openFrameset = String.Format(CultureInfo.InvariantCulture, "javascript:SlkOpenFramesetWindow('Frameset/Frameset.aspx?{0}={1}&{2}={3}');",
                                                    FramesetQueryParameter.SlkView, view, FramesetQueryParameter.LearnerAssignmentId, LearnerAssignmentGuidId);

                if (AssignmentProperties.IsNonELearning && LearnerAssignmentProperties.Status.Value != LearnerAssignmentState.Final)
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

        private void SetupFileAction(AssignmentFile file, SlkButton button, bool includeReload)
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

        private void SetUpSubmitButtons(bool enableSubmitFiles, bool enableSubmit)
        {
            //Check if non e-learning and enable the appropriate button accordingly
            if (AssignmentProperties.RootActivityId == null)
            {
                slkButtonSubmitFiles.Visible = true;
                slkButtonSubmitFiles.Text = PageCulture.Resources.LobbySubmitFilesText;
                slkButtonSubmitFiles.ToolTip = PageCulture.Resources.LobbySubmitFilesToolTip;
                slkButtonSubmitFiles.ImageUrl = Constants.ImagePath + Constants.NewDocumentIcon;
                slkButtonSubmitFiles.AccessKey = PageCulture.Resources.LobbySubmitFilesAccessKey;
                slkButtonSubmitFiles.Enabled = enableSubmitFiles;
                if (enableSubmitFiles)
                {
                    string url = string.Format(CultureInfo.InvariantCulture, 
                            "{0}{1}FilesUploadPage.aspx?fl=true&LearnerAssignmentId={2}&{3}={4}", 
                            SPWeb.Url, Constants.SlkUrlPath, LearnerAssignmentGuidId.ToString(), QueryStringKeys.Source, RawSourceUrl);

                    slkButtonSubmitFiles.NavigateUrl = url;
                    slkButtonSubmitFiles.Target = "_self";
                }

                slkButtonSubmit.Visible = true;
            }
            else
            {
                slkButtonSubmitFiles.Visible = false;
            }

            slkButtonSubmit.Enabled = enableSubmit;
        }

        private void SetUpForFinal()
        {
            slkButtonBegin.Text = PageCulture.Resources.LobbyReviewAssignmentText;
            slkButtonBegin.ToolTip = PageCulture.Resources.LobbyReviewAssignmentToolTip;

            pageTitle.Text = PageCulture.Resources.LobbyReviewAssignmentText;
            pageTitleInTitlePage.Text = PageCulture.Resources.LobbyReviewAssignmentText;

            //Check if non-elearning content
            if (AssignmentProperties.RootActivityId == null)
            {
                slkButtonReviewSubmitted.Text = PageCulture.Resources.LobbyReviewSubmittedText;
                slkButtonReviewSubmitted.ToolTip = PageCulture.Resources.LobbyReviewSubmittedToolTip;
                slkButtonReviewSubmitted.ImageUrl = Constants.ImagePath + Constants.NewDocumentIcon;
                slkButtonReviewSubmitted.Visible = true;

                DropBoxManager manager = new DropBoxManager(AssignmentProperties);

                AssignmentFile[] assignmentFiles = manager.LastSubmittedFiles(true);

                if (assignmentFiles.Length != 1)
                {
                    string script = string.Format("openSubmittedFiles('{0}');return false;", LearnerAssignmentGuidId);
                    slkButtonReviewSubmitted.OnClientClick = script;
                    slkButtonBegin.OnClientClick = script;
                }
                else
                {
                    SetupFileAction(assignmentFiles[0], slkButtonReviewSubmitted, false);
                    SetupFileAction(assignmentFiles[0], slkButtonBegin, false);
                }

            }

            SetUpSubmitButtons(false, false);
            slkButtonSubmit.Visible = false;
            ShowNavigationOptions();
            ShowLearnerComments();
        }

        private void ShowLearnerCommentsTextField()
        {
            tgrLearnerComments.Visible = true;
            LearnerComments.Visible = true;
            LearnerComments.Text = LearnerAssignmentProperties.LearnerComments;
            lblLearnerCommentsValue.Visible = false;
        }
                        
        private void ShowLearnerComments()
        {
            if (string.IsNullOrEmpty(LearnerAssignmentProperties.LearnerComments) == false)
            {
                tgrLearnerComments.Visible = true;
                lblLearnerCommentsValue.Visible = true;
                LearnerComments.Visible = false;
                lblLearnerCommentsValue.Text = SlkUtilities.GetCrlfHtmlEncodedText(LearnerAssignmentProperties.LearnerComments);
            }
            else
            {
                tgrLearnerComments.Visible = false;
                lblLearnerCommentsValue.Visible = false;
                LearnerComments.Visible = false;
            }
        }
                        
        private void SetUpForCompleted()
        {
            slkButtonBegin.Text = PageCulture.Resources.LobbyReviewAssignmentText;
            slkButtonBegin.ToolTip = PageCulture.Resources.LobbyReviewAssignmentToolTipCompleted;
            slkButtonBegin.Enabled = false;
            pageTitle.Text = PageCulture.Resources.LobbyReviewAssignmentText;
            pageTitleInTitlePage.Text = PageCulture.Resources.LobbyReviewAssignmentText;

            SetUpSubmitButtons(false, false);

            slkButtonSubmit.Visible = false;

            slkButtonReviewSubmitted.Visible = false;
            ShowNavigationOptions();
            ShowLearnerComments();
        }

        private void SetUpForNotStarted()
        {
            slkButtonBegin.Text = PageCulture.Resources.LobbyBeginAssignmentText;
            slkButtonBegin.ToolTip = PageCulture.Resources.LobbyBeginAssignmentToolTip;

            pageTitle.Text = PageCulture.Resources.LobbyBeginAssignmentText;
            pageTitleInTitlePage.Text = PageCulture.Resources.LobbyBeginAssignmentText;

            SetUpSubmitButtons(true, false);

            slkButtonReviewSubmitted.Visible = false;
        }

        private void ShowNavigationOptions()
        {
            NextPanel.Visible = true;
            navigationList.DisplayMode = BulletedListDisplayMode.HyperLink;
            navigationList.Items.Add(new ListItem( PageCulture.Resources.AppNavigateToSite + Constants.Space + SPWeb.Title, SPWeb.ServerRelativeUrl));

            if (string.IsNullOrEmpty(SourceUrl) == false)
            {
                navigationList.Items.Add(new ListItem( PageCulture.Resources.Back, SourceUrl));
            }
        }

        private void SetUpForActive()
        {
            slkButtonBegin.Text = PageCulture.Resources.LobbyResumeAssignmentText;
            slkButtonBegin.ToolTip = PageCulture.Resources.LobbyResumeAssignmentToolTip;
            pageTitle.Text = PageCulture.Resources.LobbyResumeAssignmentText;
            pageTitleInTitlePage.Text = PageCulture.Resources.LobbyResumeAssignmentText;

            SetUpSubmitButtons(true, true);

            slkButtonReviewSubmitted.Visible = false;
            ShowLearnerCommentsTextField();
        }

        private void LoadAssignmentObjects()
        {
            assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuidId, SlkRole.Learner);
            learnerAssignmentProperties = assignmentProperties.Results[0];
        }

        private void SetResourceText()
        {
            pageDescription.Text = PageCulture.Resources.LobbyPageDescription;

            lblSite.Text = PageCulture.Resources.LobbylblSite;
            lblScore.Text = PageCulture.Resources.LobbylblScore;
            labelGrade.Text = PageCulture.Resources.LobbyLabelGrade;
            lblStatus.Text = PageCulture.Resources.LobbylblStatus;
            lblStart.Text = PageCulture.Resources.LobbylblStart;
            lblDue.Text = PageCulture.Resources.LobbylblDue;
            lblComments.Text = PageCulture.Resources.LobbylblComments;
            lblLearnerComments.Text = PageCulture.Resources.LobbyLearnerComments;
            lblAutoReturn.Text = PageCulture.Resources.LobbylblAutoReturn;
            infoImage.AlternateText = PageCulture.Resources.SlkErrorTypeInfoToolTip;
            ClientScript.RegisterClientScriptBlock(this.GetType(), "SlkWindowAlreadyOpen",
                string.Format(CultureInfo.InvariantCulture, "var SlkWindowAlreadyOpen = \"{0}\";", PageCulture.Resources.LobbyWindowAlreadyOpen), true);
            ConfirmWhatNext.Text = PageCulture.Resources.CtrlLabelAPPWhatNextText;
        }

        /// <summary> Generates the url to view the submitted files. </summary>
        private string GenerateUrlForAssignmentReview()
        {
            DropBoxManager manager = new DropBoxManager(AssignmentProperties);
            AssignmentFile[] assignmentFiles = manager.LastSubmittedFiles(true);

            if (assignmentFiles.Length != 1)
            {
                return string.Format("{0}{1}SubmittedFiles.aspx?LearnerAssignmentId={2}", SPWeb.Url, Constants.SlkUrlPath, LearnerAssignmentGuidId.ToString());
            }
            else
            {
                return assignmentFiles[0].Url;
            }
        }

        private void CopyDocumentToDropBox()
        {
           DropBoxManager manager = new DropBoxManager(AssignmentProperties);
           assignmentFile = manager.CopyFileToDropBox(); 
        }

        private void FindDocumentUrl()
        {
           DropBoxManager manager = new DropBoxManager(AssignmentProperties);
           AssignmentFile[] files = manager.LastSubmittedFiles(true);
           if (files.Length == 0)
           {
               // Intial copy must have failed.
               CopyDocumentToDropBox();
           }
           else if (files.Length > 0)
           {
               assignmentFile = files[0];
           }
        }
#endregion private methods
    }
}
