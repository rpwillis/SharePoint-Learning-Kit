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
        /// <summary>Instructor message label</summary>
        protected Label instructorMessage;
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
        private LearnerAssignmentProperties m_learnerAssignmentProperties;

        /// <summary>
        /// Holds AssignmentProperties value.
        /// </summary>
        private AssignmentProperties m_assignmentProperties;

        private SlkStore m_observerRoleLearnerStore;

        #endregion

        #region Properties

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
                if (this.m_learnerAssignmentProperties == null)
                {
                    if (SlkStore.IsLearner(SPWeb))
                    {
                        this.m_learnerAssignmentProperties = SlkStore.GetLearnerAssignmentProperties(this.LearnerAssignmentGuid, SlkRole.Learner);
                    }
                    else if (SlkStore.IsObserver(SPWeb))
                    {
                        this.m_learnerAssignmentProperties = SlkStore.GetLearnerAssignmentProperties(this.LearnerAssignmentGuid, SlkRole.Observer);
                    }
                    else if (SlkStore.IsInstructor(SPWeb))
                    {
                        this.m_learnerAssignmentProperties = SlkStore.GetLearnerAssignmentProperties(this.LearnerAssignmentGuid, SlkRole.Instructor);
                    }
                    else
                    {
                        this.m_learnerAssignmentProperties = SlkStore.GetLearnerAssignmentProperties(this.LearnerAssignmentGuid, SlkRole.None);
                    }
                }

                return this.m_learnerAssignmentProperties;
            }

            set
            {
                this.m_learnerAssignmentProperties = value;
            }
        }

        /// <summary>Gets or sets the general properties of the learner assignment being displayed by this page. </summary>
        private AssignmentProperties AssignmentProperties
        {
            get
            {
                if (this.m_assignmentProperties == null)
                {
                    if (SlkStore.IsLearner(SPWeb))
                    {
                        this.m_assignmentProperties = SlkStore.GetAssignmentProperties(
                                                               LearnerAssignmentProperties.AssignmentId,
                                                               SlkRole.Learner);
                    }
                    else if (SlkStore.IsObserver(SPWeb))
                    {
                       this.m_assignmentProperties = SlkStore.GetAssignmentProperties(
                                                              LearnerAssignmentProperties.AssignmentId,
                                                              SlkRole.Learner);
                    }
                    else if (SlkStore.IsInstructor(SPWeb))
                    {
                        this.m_assignmentProperties = SlkStore.GetAssignmentProperties(
                                                               LearnerAssignmentProperties.AssignmentId,
                                                               SlkRole.Instructor);
                    }
                    else
                    {
                        this.m_assignmentProperties = SlkStore.GetAssignmentProperties(
                                                               LearnerAssignmentProperties.AssignmentId,
                                                               SlkRole.None);
                    }
                }

                SlkMemberships memberships = new SlkMemberships();
                memberships.FindAllSlkMembers(SPWeb, SlkStore, false);

                return this.m_assignmentProperties;
            }

            set
            {
                this.m_assignmentProperties = value;
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
            if (!Page.IsPostBack)
            {
                try
                {
                    this.SetResourceText();

                    string learnerAssignmentStatus = LearnerAssignmentProperties.Status.ToString();

                    if ((SlkStore.IsInstructor(SPWeb) &&
                        (learnerAssignmentStatus.Equals(LearnerAssignmentState.Completed.ToString()) ||
                        learnerAssignmentStatus.Equals(LearnerAssignmentState.Final.ToString()))) ||
                        ((SlkStore.IsLearner(SPWeb) || SlkStore.IsObserver(SPWeb)) &&
                        learnerAssignmentStatus.Equals(LearnerAssignmentState.Final.ToString())))
                    {
                        using (SPSite site = new SPSite(LearnerAssignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
                        {
                            using (SPWeb web = site.OpenWeb(LearnerAssignmentProperties.SPWebGuid))
                            {
                                this.BuildPageContent(web);
                            }
                        }
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
                    this.errorBanner.AddException(ex);
                }
            }
        }

       #endregion

        #region BuildPageContent
        /// <summary>
        /// Builds the content displayed in the page
        /// </summary>
        /// <param name="web">The SPWeb of the assignment</param>
        protected void BuildPageContent(SPWeb web)
        {
            DropBoxManager dropBox = new DropBoxManager(AssignmentProperties);
            AssignmentFile[] files = dropBox.LastSubmittedFiles(LearnerAssignmentProperties.LearnerId.GetKey());

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

            /*
            if (SlkStore.IsInstructor(SPWeb))
            {
                this.instructorMessage.Style.Add("display", string.Empty);

                StringBuilder instructorLinkURL = new StringBuilder();
                instructorLinkURL.AppendFormat("{0}{1}{2}", web.Url, "/", assignmentSubFolder.Url);
                this.instructorLink.NavigateUrl = instructorLinkURL.ToString();
                this.instructorLink.Target = "_blank";
                this.instructorLink.Style.Add("display", string.Empty);
            }
            */
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

            StringBuilder labelText = new StringBuilder();
            labelText.AppendFormat(
                           "{0}{1}{2}", 
                           LearnerAssignmentProperties.Title, 
                           " :", 
                           LearnerAssignmentProperties.LearnerName, 
                           AppResources.SubmittedFilesHeader);

            this.headerMessage.Text = labelText.ToString();

            labelText = new StringBuilder();
            labelText.AppendFormat(
                           "{0}{1}{2}", 
                           AppResources.SubmittedFilesInstructorMssage1,
                           LearnerAssignmentProperties.LearnerName,
                           AppResources.SubmittedFilesInstructorMessage2);

            this.instructorMessage.Text = labelText.ToString();

            this.instructorLink.Text = AppResources.SubmittedFilesInstructorMessage3;
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

            linkName.Attributes.Add("onclick", DropBoxManager.EditJavascript(file.Url, SPWeb) + "return false;");
            linkName.NavigateUrl = file.Url;
        }

       #endregion
    }
}
