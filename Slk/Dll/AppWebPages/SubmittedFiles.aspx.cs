/* Copyright (c) 2009. All rights reserved. */

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
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>
    /// Contains code-behind for SubmittedFiles.aspx
    /// </summary>
    public class SubmittedFiles : SlkAppBasePage
    {
        DropBoxManager dropBox;

        #region Control Declarations

        /// <summary>The title of the page.</summary>
        protected Literal pageTitle;
        /// <summary>The page's description.</summary>
        protected Literal pageDescription;
        /// <summary>The page's <see cref="ErrorBanner"/>..</summary>
        protected ErrorBanner errorBanner;
        /// <summary>The page's content area.</summary>
        protected Panel contentPanel;
        /// <summary>Page header label</summary>
        protected Label headerMessage;
        /// <summary>First assignment file hyperlink</summary>
        protected HyperLink file1;
        /// <summary>Second assignment file hyperlink</summary>
        protected HyperLink file2;
        /// <summary>Third assignment file hyperlink</summary>
        protected HyperLink file3;
        /// <summary>Fourth assignment file hyperlink</summary>
        protected HyperLink file4;
        /// <summary>Fifth assignment file hyperlink</summary>
        protected HyperLink file5;
        /// <summary>Instructor message hyperlink</summary>
        protected HyperLink instructorLink;

        #endregion

        #region Private Variables

        /// <summary>
        /// Holds LearnerAssignmentGuid value.
        /// </summary>
        private Guid m_learnerAssignmentGuidId = Guid.Empty;

        /// <summary>
        /// Holds LearnerAssignmentProperties value.
        /// </summary>
        private LearnerAssignmentProperties learnerAssignmentProperties;

        /// <summary>
        /// Holds AssignmentProperties value.
        /// </summary>
        private AssignmentProperties assignmentProperties;

        private SlkStore m_observerRoleLearnerStore;

        #endregion

        #region Properties

        /// <summary>See <see cref="SlkAppBasePage.OverrideMasterPage"/>.</summary>
        protected override bool OverrideMasterPage
        {
            get { return false ;}
        }

        /// <summary>
        /// Gets the value of the "LearnerAssignmentId" query parameter.
        /// </summary>
        private Guid LearnerAssignmentGuid
        {
            get
            {
                if (this.m_learnerAssignmentGuidId.Equals(Guid.Empty) == true)
                {
                    this.m_learnerAssignmentGuidId = QueryString.ParseGuid(QueryStringKeys.LearnerAssignmentId);
                }

                return this.m_learnerAssignmentGuidId;
            }
        }

        /// <summary>
        /// Overriding the SlkStore property to take the Observer role into account
        /// </summary>
        public override SlkStore SlkStore
        {
            get
            {
                if (String.IsNullOrEmpty(ObserverRoleLearnerKey) == false)
                {
                    if (m_observerRoleLearnerStore == null)
                    {
                        m_observerRoleLearnerStore = SlkStore.GetStore(SPWeb, ObserverRoleLearnerKey);
                    }
                    return m_observerRoleLearnerStore;
                }
                return base.SlkStore;
            }
        }

        /// <summary>
        /// Gets or sets the properties (specific to a certain learner) of the learner assignment being displayed by this page.
        /// </summary>
        private LearnerAssignmentProperties LearnerAssignmentProperties
        {
            get
            {
                if (this.learnerAssignmentProperties == null)
                {
                    LoadAssignmentProperties();
                }

                return this.learnerAssignmentProperties;
            }
        }

        /// <summary>Gets or sets the general properties of the learner assignment being displayed by this page. </summary>
        private AssignmentProperties AssignmentProperties
        {
            get
            {
                if (this.assignmentProperties == null)
                {
                    LoadAssignmentProperties();
                }

                return this.assignmentProperties;
            }
        }

        LearnerAssignmentState Status 
        {
            get
            {
                return LearnerAssignmentProperties.Status == null ? LearnerAssignmentState.NotStarted : LearnerAssignmentProperties.Status.Value;
            }
        }

        #endregion

        #region OnPreRender

        /// <summary>
        ///  Over rides OnPreRender.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!Page.IsPostBack)
            {
                try
                {
                    this.SetResourceText();


                    if (
                            (SlkStore.IsInstructor(SPWeb) && (Status == LearnerAssignmentState.Completed || Status == LearnerAssignmentState.Final))
                            || SlkStore.IsLearner(SPWeb) ||
                            (SlkStore.IsObserver(SPWeb) && Status == LearnerAssignmentState.Final)
                        )
                    {
                        BuildPageContent();
                    }
                    else
                    {
                        string exceptionMessage;

                        if (SlkStore.IsInstructor(SPWeb))
                        {
                            exceptionMessage = AppResources.SubmittedFilesTeacherNoAccessException;
                        }
                        else
                        {
                            exceptionMessage = AppResources.SubmittedFilesLearnerNoAccessException;
                        }

                        throw new SafeToDisplayException(exceptionMessage);
                    }
                }
                catch (Exception ex)
                {
                    this.contentPanel.Visible = false;
                    this.errorBanner.Clear();
                    this.errorBanner.AddException(SlkStore, ex);
                }
            }
        }

       #endregion

        #region BuildPageContent
        /// <summary>
        /// Builds the content displayed in the page
        /// </summary>
        protected void BuildPageContent()
        {
            dropBox = new DropBoxManager(AssignmentProperties);
            AssignmentFile[] files = dropBox.LastSubmittedFiles(LearnerAssignmentProperties.User.SPUser);


            int fileIndex = 0;
            foreach (AssignmentFile file in files)
            {
                if (fileIndex == 0)
                {
                    this.DisplayFileLink(this.file1, file);
                }
                else if (fileIndex == 1)
                {
                    this.DisplayFileLink(this.file2, file);
                }
                else if (fileIndex == 2)
                {
                    this.DisplayFileLink(this.file3, file);
                }
                else if (fileIndex == 3)
                {
                    this.DisplayFileLink(this.file4, file);
                }
                else
                {
                    this.DisplayFileLink(this.file5, file);
                }

                fileIndex++;
            }
        }

        #endregion

        #region SetResourceText
        /// <summary>
        ///  Set the Control Text from Resource File.
        /// </summary>
        private void SetResourceText()
        {
            this.pageTitle.Text = AppResources.SubmittedFilesPageTitle;
            this.pageDescription.Text = AppResources.SubmittedFilesPageDescription;
            this.headerMessage.Text = string.Format(AppResources.SubmittedFilesHeader, AssignmentProperties.Title, LearnerAssignmentProperties.LearnerName);
            this.instructorLink.Text = string.Format(AppResources.SubmittedFilesInstructorMessage, LearnerAssignmentProperties.LearnerName);
        }

       #endregion

        #region DisplayFileLink

        /// <summary>
        /// Display the assignment file link and URL
        /// </summary>
        /// <param name="linkName">The name of the file's hyperlink control</param>
        /// <param name="file">The assignment file</param>
        private void DisplayFileLink(HyperLink linkName, AssignmentFile file)
        {
            string assignmentFileName = file.Name;
            linkName.Text = file.Name;
            linkName.Target = "_blank";
            linkName.Style.Add("display", string.Empty);

            DropBoxEditMode editMode = Status == LearnerAssignmentState.Completed ? DropBoxEditMode.Edit : DropBoxEditMode.View;
            DropBoxEditDetails editDetails = dropBox.GenerateDropBoxEditDetails(file, SPWeb, editMode, Page.Request.RawUrl);
            linkName.NavigateUrl = editDetails.Url;
            if (string.IsNullOrEmpty(editDetails.OnClick) == false)
            {
                linkName.Attributes.Add("onclick", editDetails.OnClick + "return false;");
            }
        }

       #endregion

#region private methods
        void LoadAssignmentProperties()
        {
            if (SlkStore.IsInstructor(SPWeb))
            {
                this.assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.Instructor);
            }
            else if (SlkStore.IsLearner(SPWeb))
            {
                this.assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.Learner);
            }
            else if (SlkStore.IsObserver(SPWeb))
            {
               this.assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.Learner);
            }
            else
            {
                this.assignmentProperties = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuid, SlkRole.None);
            }

            learnerAssignmentProperties = assignmentProperties.Results[0];
        }
#endregion private methods
    }
}
