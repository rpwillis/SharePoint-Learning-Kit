/* Copyright (c) Microsoft Corporation. All rights reserved. */

//GradingList.cs
//
//Implementation of Grade the assignment section, ServerControl and associated Classes for GradingList control 
//
using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.SharePointLearningKit;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Frameset;
using Resources.Properties;
using System.Text;
using System.Globalization;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  Custom Control to Render Grading Items which is used to 
    ///  enter grades and comments for each learner assignment, 
    ///  and advance the workflow state for a learner assignment.
    ///  usage: &lt;{0}:GradingList runat="server" ID="gradingList"&gt;
    ///         &lt;/{0}:GradingList&gt;;
    /// </summary>

    [ToolboxData("<{0}:GradingList runat=server></{0}:GradingList>")]
    public class GradingList : WebControl, INamingContainer
    {
        SlkCulture culture;

        #region Private Fields
        /// <summary>
        ///  Collection of Grading Items.
        /// </summary>
        private GradingItemCollection m_items;
        /// <summary>
        /// Holds the Postback Grading Item Collection.
        /// </summary>
        private Dictionary<string, GradingItem> m_postBackGradingItems;
        DropBoxManager dropBox;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor used to initialize the Grading Item Collection  
        /// </summary>
        public GradingList()
        {
            //Initialize the GradingItem Collection
            m_items = new GradingItemCollection();
        }
        #endregion Constructor

        #region Public and Private Properties

        /// <summary>Whether to use grades or not.</summary>
        bool UseGrades { get; set; }

        /// <summary>Whether to hide points or not.</summary>
        bool HidePoints { get; set; }

        /// <summary>Whether there is a learner report url to add.</summary>
        bool HasLearnerReport { get; set; }

        /// <summary>The url to the learner report.</summary>
        string LearnerReportUrl { get; set; }

        /// <summary>The AssignmentProperties to use.</summary>
        public AssignmentProperties AssignmentProperties { get; set; }

        /// <summary>
        /// FinalPoints Field TextBox Control ID
        /// </summary>
        private string FinalScoreId { get; set; }

        /// <summary>Comments Field TextBox Control ID</summary>
        private string CommentsId { get; set; }

        /// <summary>Grade Field TextBox Control ID</summary>
        private string GradeId { get; set; }

        /// <summary>Action Field CheckBox Control ID</summary>
        private string ActionId { get; set; }

        /// <summary>Grading Score Control ID</summary>
        private string GradingScoreId { get; set; }

        /// <summary>Grading Image Control ID</summary>
        private string GradingImageId { get; set; }

        /// <summary>Grading Row ID</summary>
        private string GradingRowId { get; set; }

        /// <summary>Collection of Grading Items to be listed</summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        internal GradingItemCollection Items
        {
            get { return m_items; }
        }

        #endregion

        #region Private and Protected Methods

        #region OnInit
        /// <summary>
        /// Override OnInit to Register ControlState
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
            culture = new SlkCulture();
        }
        #endregion

        #region LoadControlState
        /// <summary>
        /// Loads Control State from previously saved State
        /// </summary>
        /// <param name="savedState">savedState</param>
        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                Pair p = (Pair)savedState;
                base.LoadViewState(p.First);
                Items.LoadViewState(p.Second);
            }
            else
            {
                base.LoadViewState(null);
            }
            //base.LoadControlState(savedState);
        }

        #endregion

        #region Add
        /// <summary>
        /// Adds the LearnerAssignmentProperties to the GradingItemCollection             
        /// </summary>
        /// <param name="gradingProperties">Grading Properties</param> 
        public void Add(LearnerAssignmentProperties gradingProperties)
        {
            GradingItem item = new GradingItem();

            item.LearnerAssignmentId = gradingProperties.LearnerAssignmentId.GetKey();
            item.GradedScore = gradingProperties.GradedPoints;
            item.FinalScore = gradingProperties.FinalPoints;
            item.Grade = gradingProperties.Grade;
            item.InstructorComments = gradingProperties.InstructorComments;
            item.LearnerComments = gradingProperties.LearnerComments;
            item.LearnerName = gradingProperties.LearnerName;
            item.Status = gradingProperties.Status.GetValueOrDefault();
            item.SuccessStatus = gradingProperties.SuccessStatus;
            item.LearnerAssignmentGuidId = gradingProperties.LearnerAssignmentGuidId;
            item.LearnerId = gradingProperties.LearnerId.GetKey();
            this.m_items.Add(item);
        }
        #endregion

        #region OnPreRender
        /// <summary>
        ///  Over rides OnPreRender to Render APP 
        /// </summary> 
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Register Client Script
            RegisterGradingClientScriptBlock();
        }
        #endregion+

        /// <summary>See <see cref="Control.OnLoad"/>.</summary>
        protected override void OnLoad(EventArgs e)
        {
            SetUpIds();
        }

        #region Render
        /// <summary>
        /// Renders the Grading List Collection
        /// </summary>
        /// <param name="writer">htmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Items != null && Items.Count > 0)
            {

                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 0, writer))
                        {
                            // render the "<table>" element and its contents 
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarystandardbody");
                            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                            {
                                // render the header row 
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-viewheadertr");
                                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                                {
                                    // render the Learner column headers
                                    RenderColumnHeader(culture.Resources.GradingLearnerHeaderText, writer);
                                    // render the Status column headers
                                    RenderColumnHeader(culture.Resources.GradingStatusHeaderText, writer);

                                    if (AssignmentProperties.IsNonELearning)
                                    {
                                        // render the File Submission column headers
                                        RenderColumnHeader(culture.Resources.GradingFileSubmissionHeaderText, writer);
                                    }
                                    else
                                    {
                                        RenderColumnHeader(culture.Resources.GradingGradedScoreHeaderText, writer);
                                    }

                                    if (HidePoints == false)
                                    {
                                        // render the Final Score column headers
                                        RenderColumnHeader(culture.Resources.GradingFinalScoreHeaderText, writer);
                                    }

                                    if (UseGrades)
                                    {
                                        RenderColumnHeader(culture.Resources.GradingGradeHeaderText, writer);
                                    }

                                    // render the Comments column headers
                                    RenderColumnHeader(culture.Resources.GradingCommentsHeaderText, writer);
                                    // render the Action column headers
                                    RenderColumnHeader(culture.Resources.GradingActionHeaderText, writer);
                                }
                                foreach (GradingItem item in Items.Values)
                                {
                                    RenderGradingItem(item, writer);
                                }
                            }
                        }
                    }
                }

            }

        }
        #endregion

        /// <summary>Initalizes the grading list based on the SLK settings.</summary>
        public void Initialize(SlkSettings settings)
        {
            Clear();
            UseGrades = settings.UseGrades;
            HidePoints = AssignmentProperties.HidePoints;

            LearnerReportUrl = settings.LearnerReportUrl;
            HasLearnerReport = string.IsNullOrEmpty(LearnerReportUrl) == false;

            if (AssignmentProperties.IsNonELearning)
            {
                dropBox = new DropBoxManager(AssignmentProperties);
            }

            foreach (LearnerAssignmentProperties item in AssignmentProperties.Results)
            {
                Add(item);
            }
        }

        #region RenderColumnHeader
        /// <summary>
        /// Renders a column header, i.e. the label at the top of a column in the Grading list.
        /// </summary>
        /// 
        /// <param name="columnName">The <c>Name</c> of the column being rendered.</param>
        /// 
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private static void RenderColumnHeader(string columnName, HtmlTextWriter htmlTextWriter)
        {
            // render the "<th>" element for this column header
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "border-left: none; padding-left: 3px;");
            using (new HtmlBlock(HtmlTextWriterTag.Th, 1, htmlTextWriter))
            {
                // render the "<div>" element containing the column header
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                            "position: relative; left: 0px; top: 0px; width: 100%;");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
                {
                    // render the "<table>" element containing the column header
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-unselectedtitle");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%;");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    using (new HtmlBlock(HtmlTextWriterTag.Table, 0, htmlTextWriter))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, htmlTextWriter))
                        {
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                            {
                                // write the column title
                                htmlTextWriter.WriteEncodedText(columnName);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region RenderGradedLearner
        /// <summary>
        /// Render the Graded Learner 
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderGradedLearner(GradingItem item, HtmlTextWriter htmlTextWriter)
        {

            if (item.Status == LearnerAssignmentState.NotStarted)
            {
                // Just label no link
                Label lblLearnerItem = new Label();
                lblLearnerItem.ID = "lblLearner" + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                lblLearnerItem.Text = item.LearnerName;
                lblLearnerItem.ToolTip = culture.Resources.GradingStatusNotStartedToolTip;
                lblLearnerItem.RenderControl(htmlTextWriter);
            }
            else
            {
                // the learner’s name is hyperlinked to open the learner assignment in the frameset. 

                HyperLink lnkLearnerItem = new HyperLink();
                lnkLearnerItem.ID = "lnkLearner" + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                lnkLearnerItem.Text = item.LearnerName;

                if (AssignmentProperties.IsELearning)
                {
                    string extraQueryString = null;
                    Frameset.AssignmentView view = Frameset.AssignmentView.InstructorReview;

                    //Final Score is disabled for Class Server content when the frameset 
                    //is open to the grading view of the associated learner assignment.
                    switch (item.Status)
                    {
                        case LearnerAssignmentState.Active:
                           lnkLearnerItem.ToolTip = culture.Resources.GradingStatusInProgressToolTip;
                           view = Frameset.AssignmentView.InstructorReview;
                           break;

                        case LearnerAssignmentState.Completed:
                            lnkLearnerItem.ToolTip = culture.Resources.GradingStatusSubmittedToolTip;
                            view = Frameset.AssignmentView.Grading;
                            extraQueryString = string.Format(CultureInfo.InvariantCulture, "&{0}={1}", FramesetQueryParameter.Src, "Grading");
                            break;

                        case LearnerAssignmentState.Final:
                            lnkLearnerItem.ToolTip = culture.Resources.GradingStatusFinalToolTip;
                            view = Frameset.AssignmentView.Grading;
                            extraQueryString = string.Format(CultureInfo.InvariantCulture, "&{0}={1}", FramesetQueryParameter.Src, "Grading");
                            break;
                    }

                    lnkLearnerItem.NavigateUrl = String.Format(CultureInfo.InvariantCulture,
                                "javascript:Slk_OpenLearnerAssignment(\"{0}?{1}={2}&{3}={4}{5}\",{6},{7});",
                                Constants.FrameSetPage,
                                FramesetQueryParameter.SlkView,
                                view,
                                FramesetQueryParameter.LearnerAssignmentId,
                                item.LearnerAssignmentGuidId,
                                extraQueryString,
                                item.LearnerAssignmentId,
                                AssignmentProperties.IsClassServerContent ? "true" : "false");
                }
                else
                {
                    AssignmentFile[] files = dropBox.LastSubmittedFiles(item.LearnerId);
                    if (files != null && files.Length > 0)
                    {
                        SetUpSubmittedFileHyperLink(lnkLearnerItem, files, item);
                    }
                }

                lnkLearnerItem.RenderControl(htmlTextWriter);
            }
        }
        #endregion

        #region RenderFileSubmissionState
        /// <summary>
        /// Render the file submission state 
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderFileSubmissionState(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            WebControl control;

            if (item.Status == LearnerAssignmentState.Completed || item.Status == LearnerAssignmentState.Final)
            {
                AssignmentFile[] files = dropBox.LastSubmittedFiles(item.LearnerId);

                if (files == null || files.Length == 0)
                {
                    control = new Label();
                    ((Label)control).Text = culture.Resources.GradingFileSubmissionSubmittedNoFiles;
                }
                else
                {
                    control = new HyperLink();
                    ((HyperLink)control).Text = culture.Resources.GradingFileSubmissionSubmitted;
                    SetUpSubmittedFileHyperLink((HyperLink)control, files, item);
                }
            }
            else
            {
                control = new Label();
                ((Label)control).Text = culture.Resources.GradingFileSubmissionNotSubmitted;
            }

            control.ID = "lnkFileSubmissionState" + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
            control.RenderControl(htmlTextWriter);
        }

        void SetUpSubmittedFileHyperLink(HyperLink link, AssignmentFile[] files, GradingItem item)
        {
            SlkAppBasePage slkAppBasePage = new SlkAppBasePage();
            if (files.Length > 1)
            {
                string script = string.Format("openSubmittedFiles('{0}');return false;", item.LearnerAssignmentGuidId);
                link.Attributes.Add("onclick", script);
                link.NavigateUrl = "#";
            }
            else
            {
                DropBoxEditMode editMode = item.Status == LearnerAssignmentState.Completed ? DropBoxEditMode.Edit : DropBoxEditMode.View;
                DropBoxEditDetails editDetails = dropBox.GenerateDropBoxEditDetails(files[0], slkAppBasePage.SPWeb, editMode, Page.Request.RawUrl);
                link.NavigateUrl = editDetails.Url;
                if (string.IsNullOrEmpty(editDetails.OnClick) == false)
                {
                    link.Attributes.Add("onclick", editDetails.OnClick + "return false;");
                }
            }
        }
        #endregion

        #region RenderGradedScore
        /// <summary>
        /// Render the Graded Score which Shows the computed points, 
        /// rounded to the nearest numeric, followed by an diamond icon 
        /// if the learner assignment success status is known. 
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderGradedScore(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                //Render the computed points, rounded to the nearest integer,
                //followed by an icon which tell the success status.

                using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                {
                    //Controls to Graded Score
                    Image imgGraded = new Image();
                    imgGraded.ID = GradingImageId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                    imgGraded.Width = new Unit(11, UnitType.Pixel);
                    imgGraded.Height = new Unit(11, UnitType.Pixel);

                    //Render Graded Score
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-right: 6px;");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                    {
                        WebControl gradedScore = new Label();
                        bool isHyperLink = false;
                        if (item.GradedScore != null && HasLearnerReport)
                        {
                            gradedScore = new HyperLink();
                            isHyperLink = true;
                        }

                        gradedScore.ID = GradingScoreId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                        //Add the ToolTipText to get the localized Text while 
                        //Changing the Tool Tip from Client Side 
                        gradedScore.Attributes.Add("ToolTipText", culture.Resources.GradingScoreToolTip);
                        if (item.GradedScore != null)
                        {
                            string text = culture.Format(culture.Resources.GradingGradedScore, item.GradedScore.Value);
                            if (isHyperLink)
                            {
                                HyperLink link = (HyperLink)gradedScore;
                                link.Text = text;
                                link.NavigateUrl = LearnerReportUrl + item.LearnerAssignmentGuidId;
                            }
                            else
                            {
                                ((Label)gradedScore).Text = text;
                            }

                            //Tool Tip for Graded Score 
                            //Similar to <Computed points with full precision> Points.
                            gradedScore.ToolTip = culture.Format(culture.Resources.GradingGradedScoreToolTip, item.GradedScore.Value, culture.Resources.GradingScoreToolTip);
                        }
                        else
                        {
                            ((Label)gradedScore).Text = Constants.NonBreakingSpace;
                        }

                        gradedScore.RenderControl(htmlTextWriter);
                    }

                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "1%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                                "padding-right: 6px; padding-top:2px");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                    {
                        //Image Tag icon for the learner assignment success status              
                        switch (item.SuccessStatus)
                        {
                            case SuccessStatus.Unknown:
                                imgGraded.ImageUrl = Constants.BlankGifUrl;
                                break;
                            case SuccessStatus.Failed:
                                imgGraded.ImageUrl = Constants.ImagePath + Constants.GradingUnSatisfiedIcon;
                                imgGraded.ToolTip = culture.Resources.GradingSuccessStatusUnSatisfiedToolTip;
                                break;
                            case SuccessStatus.Passed:

                                imgGraded.ImageUrl = Constants.ImagePath + Constants.GradingSatisfiedIcon;
                                imgGraded.ToolTip = culture.Resources.GradingSuccessStatusSatisfiedToolTip;
                                break;
                        }
                        //Render the Image Control
                        imgGraded.RenderControl(htmlTextWriter);
                    }
                }
            }
        }
        #endregion

        TextBox CreateInputBox(string id, GradingItem item)
        {
            TextBox inputBox = new TextBox();
            inputBox.CssClass = "ms-input";
            inputBox.Width = new Unit(50, UnitType.Pixel);
            inputBox.ID = id;
            string onFocusHandler = String.Format(CultureInfo.InvariantCulture, "Slk_GradingHighlightGradingRow({0});", item.LearnerAssignmentId);
            inputBox.Attributes.Add("onfocus", onFocusHandler);
            return inputBox;
        }

        #region RenderFinalScore
        /// <summary>Render the Final Score defaults to the computed points value, and always shows full precision.</summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderFinalScore(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            if (item.IsComplete)
            {
                //Renders  the final points value, and always shows full precision.
                string uniqueId = FinalScoreId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                TextBox txtFinalScore = CreateInputBox(uniqueId, item);

                //Add Client Event Handlers to the Control
                txtFinalScore.Attributes.Add("onblur", "Slk_GradingValidateFinalScore(this);");
                txtFinalScore.Attributes.Add("onclick", "Slk_GradingFinalScoreDisabled(this);");
                if (item.FinalScore != null)
                {
                    txtFinalScore.Text = item.FinalScore.Value.ToString(CultureInfo.CurrentCulture);
                }

                txtFinalScore.ToolTip = culture.Resources.GradingFinalScoreSubmittedToolTip;
                txtFinalScore.RenderControl(htmlTextWriter);
            }

        }
        #endregion

        #region RenderActionCheckBox
        /// <summary>
        /// Each row of the grading Grid has an action checkbox that can be used 
        /// to advance the learner assignment to the next state in the workflow. 
        /// The label and tooltip of the checkbox reflect the state that 
        /// the learner assignment would advance to.
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderActionCheckBox(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            string uniqueId = ActionId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
            string actionToolTip = String.Empty;
            switch (item.Status)
            {
                //If LearnerAssignmentState is NotStarted or Active, checkbox label is "Collect", 
                //Tooltip is "Prevents the learner from  continuing to work on the assignment, 
                //and allows you to grade it".
                case LearnerAssignmentState.NotStarted:
                    item.ActionText = culture.Resources.GradingActionTextNotSubmitted;
                    actionToolTip = culture.Resources.GradingActionNotSubmittedToolTip;
                    break;
                case LearnerAssignmentState.Active:
                    item.ActionText = culture.Resources.GradingActionTextNotSubmitted;
                    actionToolTip = culture.Resources.GradingActionNotSubmittedToolTip;
                    break;
                //If LearnerAssignmentState is Completed, checkbox label is "Return", 
                //Tooltip is "Returns the assignment to the learner for  review"
                case LearnerAssignmentState.Completed:
                    item.ActionText = culture.Resources.GradingActionTextSubmitted;
                    actionToolTip = culture.Resources.GradingActionSubmittedToolTip;
                    break;
                //If LearnerAssignmentState is Final, checkbox label is "Reactivate",
                //  Tooltip is "Allows the learner to re-do the assignment."
                case LearnerAssignmentState.Final:
                    item.ActionText = culture.Resources.GradingActionTextFinal;
                    actionToolTip = culture.Resources.GradingActionFinalToolTip;
                    break;
                default:
                    break;
            }

            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "SlkGrading");
                    using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
                    {
                        //Render Checkbox
                        CheckBox checkBox = new CheckBox();
                        checkBox.ID = uniqueId;
                        checkBox.ToolTip = actionToolTip;
                        checkBox.Checked = item.ActionState;
                        string onFocusHandler
                            = String.Format(CultureInfo.InvariantCulture,
                                            "Slk_GradingHighlightGradingRow({0});",
                                            item.LearnerAssignmentId);
                        checkBox.Attributes.Add("onfocus", onFocusHandler);
                        checkBox.Text = item.ActionText;
                        checkBox.RenderControl(htmlTextWriter);
                    }

                }
            }
        }
        #endregion

        #region RenderGradingItem
        /// <summary>
        /// Render the Grading Item Row.  
        /// </summary>       
        /// <param name="item">GradingItem</param>
        /// <param name="htmlTextWriter">htmlTextWriter</param>
        private void RenderGradingItem(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            string focusScript = String.Format(CultureInfo.InvariantCulture, "Slk_GradingHighlightGradingRow({0});", item.LearnerAssignmentId);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Onclick, focusScript);

            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Id, GradingRowId + item.LearnerAssignmentId);

            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
            {
                //Render Graded Learner 
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 5px; padding-top:5pt");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    RenderGradedLearner(item, htmlTextWriter);
                }
                //Renders the status of the learner assignment
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 5px; padding-top:5pt");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    htmlTextWriter.Write(SlkUtilities.GetLearnerAssignmentState(item.Status));
                }

                if (AssignmentProperties.IsNonELearning)
                {
                    //Render file submission state 
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 5px; padding-top:5pt");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                    {
                        RenderFileSubmissionState(item, htmlTextWriter);
                    }
                }
                else
                {
                    //Render the Graded Score 
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                        "width: 1%; padding-left: 5px; padding-top:5pt");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                    {
                        RenderGradedScore(item, htmlTextWriter);
                    }
                }

                if (HidePoints == false)
                {
                    //Render Final Score   
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 50px; padding-left: 5px; padding-top:3px");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                    {
                        RenderFinalScore(item, htmlTextWriter);
                    }
                }

                if (UseGrades)
                {
                    //Render Grade  
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 50px; padding-left: 5px; padding-top:3px");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                    {
                        if (item.IsComplete)
                        {
                            TextBox gradeBox = CreateInputBox(GradeId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture), item);
                            gradeBox.MaxLength = 20;
                            gradeBox.Text = item.Grade;
                            gradeBox.RenderControl(htmlTextWriter);
                        }

                    }
                }

                //Render Comments  
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 200px; padding-left: 5px; padding-top:3px");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    TextBox txtInstructorComments = new TextBox();
                    txtInstructorComments.CssClass = "ms-long";
                    txtInstructorComments.ID = CommentsId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                    txtInstructorComments.TextMode = TextBoxMode.MultiLine;
                    /* box model width doesn't include border or padding so setting textarea width to 100% makes it bigger than the cell it is in
                     * to resolve
                     *  -webkit-box-sizing: border-box;  <=iOS4, <= Android  2.3 
                     *  -moz-box-sizing: border-box;  FF1+ 
                     *  box-sizing: border-box;  Chrome, IE8, Opera, Safari 5.1
                     *
                     *  Really needs breaking out into own css class, as do all the slk styles
                     */
                    string style = "width: 100%; height:40px; overflow:visible;-webkit-box-sizing: border-box;-moz-box-sizing: border-box;box-sizing: border-box;";
                    txtInstructorComments.Style.Value = style;
                    txtInstructorComments.Text = item.InstructorComments;
                    txtInstructorComments.Attributes.Add("onfocus", focusScript);
                    txtInstructorComments.RenderControl(htmlTextWriter);

                }

                //Render Action checkboxes
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 3px;");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {

                    RenderActionCheckBox(item, htmlTextWriter);
                }
            }

            // Render learner comments if any
            if (string.IsNullOrEmpty(item.LearnerComments) == false)
            {
                htmlTextWriter.Write("<tr class=\"ms-vb\"><td>&nbsp;</td><td>");
                htmlTextWriter.Write(HttpUtility.HtmlEncode(culture.Resources.GradingLearnerComments));
                htmlTextWriter.Write("</td><td colspan=\"5\">");
                string encodedString = HttpUtility.HtmlEncode(item.LearnerComments);
                encodedString = System.Text.RegularExpressions.Regex.Replace(encodedString, @"\r\n?|\n", "<br />");
                htmlTextWriter.Write(encodedString);
                htmlTextWriter.Write("</td></tr>");
            }
        }
        #endregion

        #region SetGradingItems
        /// <summary>
        ///  Set the Postback Collection to Grading Items
        /// </summary>
        /// <param name="controlId">Id</param>
        /// <param name="controlValue">Value</param>       
        private void SetGradingItems(string controlId, string controlValue)
        {
            //Find  the Control and update the corresponding Grading Item   
            bool isGradingItemChanged = false;
            string key = controlId.Substring(FinalScoreId.Length); // All control ids are same length
            long keyValue = long.Parse(key, CultureInfo.InvariantCulture);
            GradingItem item = Items[keyValue];

            //set the Final Score Value
            if (controlId.StartsWith(FinalScoreId, StringComparison.Ordinal))
            {
                float? finalScore = null;

                if (!String.IsNullOrEmpty(controlValue))
                {
                    try
                    {
                        finalScore = float.Parse(controlValue, CultureInfo.CurrentCulture.NumberFormat);
                    }
                    catch (FormatException ex)
                    {
                        throw new SafeToDisplayException(ex.Message, ex);
                    }
                }

                if (item.FinalScore != finalScore)
                {
                    isGradingItemChanged = true;
                    item.FinalScore = finalScore;
                }

            }  //set the Grade Value
            else if (controlId.StartsWith(GradeId, StringComparison.Ordinal))
            {
                if (item.Grade != controlValue)
                {
                    isGradingItemChanged = true;
                    item.Grade = controlValue;
                }

            } //set the Comments Value
            else if (controlId.StartsWith(CommentsId, StringComparison.Ordinal))
            {
                if (item.InstructorComments != controlValue)
                {
                    isGradingItemChanged = true;
                    item.InstructorComments = controlValue;
                }

            } //set the Action Value
            else if (controlId.StartsWith(ActionId, StringComparison.Ordinal))
            {
                bool actionState = (controlValue == "on") ?  true : false;

                if (item.ActionState != actionState)
                {
                    isGradingItemChanged = true;
                    item.ActionState = actionState;
                }
            }

            //If the Postback Collection has changes add the modfied Item 
            //to the Postback Grading Item Collection to return and also Check if 
            //Item already exists in Postback Grading Item Collection.

            if (isGradingItemChanged && !(m_postBackGradingItems.ContainsKey(key)))
            {
                m_postBackGradingItems.Add(key, item);
                Items.LearnerItemsChanged.Add(item.LearnerAssignmentId);
            }
        }
        #endregion

        #region DeterminePostBackGradingItems
        /// <summary>
        /// Determines PostBack Collection of GradingItems and 
        /// returns the Grading Item Collection as Dictionary
        /// </summary>
        /// <returns>Dictionary Collection of Grading Item</returns>
        internal Dictionary<string, GradingItem> DeterminePostBackGradingItems()
        {
            m_postBackGradingItems = new Dictionary<string, GradingItem>();

            NameValueCollection postbackCollection = this.Page.Request.Form;

            foreach (string key in postbackCollection.Keys)
            {
                if (key.Contains(this.ClientID))
                {
                    string controlID = key;
                    string controlValue = postbackCollection[key];
                    SetGradingItems(controlID, controlValue);
                }
            }

            // Add any items in LearnerItemsChanged which are not included already
            if (Items.LearnerItemsChanged != null && Items.LearnerItemsChanged.Count > 0)
            {
                foreach (long key in Items.LearnerItemsChanged)
                {
                    string itemKey = key.ToString(CultureInfo.InvariantCulture);

                    if ((m_postBackGradingItems.ContainsKey(itemKey)) == false)
                    {
                        GradingItem item = Items[key];
                        m_postBackGradingItems.Add(itemKey, item);
                    }
                }
            }

            return m_postBackGradingItems;
        }

        #endregion

        #region SaveControlState
        /// <summary>
        /// Save the ControlState and returns the saved object
        /// </summary>
        /// <returns>savedObject</returns>
        protected override object SaveControlState()
        {
            object baseState = base.SaveControlState();
            //Update the Post back Items.
            DeterminePostBackGradingItems();
            object itemState = Items.SaveViewState();
            if ((baseState == null) && (itemState == null))
                return null;
            return new Pair(baseState, itemState);
        }
        #endregion

        #region Clear
        /// <summary>
        /// Removes all the GradingItem in the Grading Item Collection 
        /// </summary>      
        void Clear()
        {
            //Clear the Grading Item Collection
            this.Items.Clear();
            //Clears the Grading Item Changed Collection
            this.Items.LearnerItemsChanged.Clear();

        }
        #endregion

        void SetUpIds()
        {
            // Important - ensure that all the control id lengths are the same for optimization later
            FinalScoreId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "score_");
            CommentsId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "commnt");
            GradeId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "grade_");
            ActionId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "action");
            GradingScoreId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "gscore");
            GradingImageId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "graded");
            GradingRowId = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}", ClientID, ClientIDSeparator, "rowId_");
        }

        #region RegisterGradingClientScriptBlock
        /// <summary>
        /// Method that creates and defines the Grading Client Script  
        /// and registers script blocks with the page.
        /// </summary>    
        private void RegisterGradingClientScriptBlock()
        {
            // Define the name and type of the client scripts on the page.
            String csTitle = "GradingClientScript";

            Type cstype = this.GetType();

            // Get a ClientScriptManager reference from the Page class.
            ClientScriptManager cs = this.Page.ClientScript;

            // Check to see if the client script is already registered.
            if (!cs.IsClientScriptBlockRegistered(cstype, csTitle))
            {
                //Build the Script 
                StringBuilder csGradingClientScript = new StringBuilder(1000);

                csGradingClientScript.AppendLine("<!-- Place Holder Grading Client Script -->");

                csGradingClientScript.AppendLine("slk_selectedGradingRowId = null;");

                csGradingClientScript.AppendLine("var arrActualGradedPoints = new Array();");
                csGradingClientScript.AppendLine("var slk_arrOpenedFrameSets = new Array();");


                foreach (GradingItem item in this.Items.Values)
                {
                    csGradingClientScript.Append(@"
                     arrActualGradedPoints[");
                    csGradingClientScript.Append(item.LearnerAssignmentId);
                    csGradingClientScript.Append(@"] = ");
                    if (item.GradedScore == null)
                    {
                        csGradingClientScript.Append("null");
                    }
                    else
                    {
                        csGradingClientScript.Append(item.GradedScore);
                    }
                    csGradingClientScript.Append(@";");
                }
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strFinalScorePrefix = \"{0}\";",
                                           FinalScoreId
                                           ));
                csGradingClientScript.AppendLine(
                            String.Format(CultureInfo.InvariantCulture,
                                          "var slk_strDecimalChar = \"{0}\";",
                                          decimalSeparator
                                         ));

                //Construct frameset  Handler JavaScript . 
                //This Method is invoked from Frameset. 
                //SlkUpdateLearnerAssignment(learnerAssignmentId,/* Id */ 
                //assignmentStatus /* as localized string */,
                //passFail /* as "passed" or "failed" or unknown */ , 
                //finalPoints, /*finalScore */
                //computedPoints /*Graded Score */ )
                //assignmentStatus and finalPoints values are not used in Grading. 
                csGradingClientScript.AppendLine(@"
                function SlkUpdateLearnerAssignment(learnerAssignmentId, assignmentStatus,
                                                    successStatus, finalPoints, computedPoints)
                {
                    
                    if((learnerAssignmentId == undefined) &&
                       (assignmentStatus == undefined)&&
                       (successStatus == undefined)&&
                       (finalPoints == undefined)&&
                       (computedPoints == undefined) 
                      )
                    {                        
                        return;
                    }
                "
                );
                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strGradedScorePrefix = \"{0}\";",
                                           GradingScoreId
                                          ));
                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strGradedImagePrefix = \"{0}\";",
                                           GradingImageId
                                          ));
                csGradingClientScript.AppendLine(
                            String.Format(CultureInfo.InvariantCulture,
                                          "var slk_strGradingUnSatisfiedSrc = \"{0}\";",
                                          Constants.ImagePath + Constants.GradingUnSatisfiedIcon
                                         ));
                csGradingClientScript.AppendLine(
                         String.Format(CultureInfo.InvariantCulture,
                                       "var slk_strGradingSatisfiedSrc = \"{0}\";",
                                       Constants.ImagePath + Constants.GradingSatisfiedIcon
                                      ));
                csGradingClientScript.AppendLine(
                        String.Format(CultureInfo.InvariantCulture,
                                      "var slk_strGradingUnKnownSrc = \"{0}\";",
                                      Constants.BlankGifUrl));

                csGradingClientScript.AppendLine(@"                    
                    var finalScoreId = slk_strFinalScorePrefix + learnerAssignmentId ;
                    if(document.getElementById(finalScoreId) == null && 
                        document.getElementById(finalScoreId) == undefined)
                    {
                        return;
                    }
                    document.getElementById(finalScoreId).disabled = false; 
                    if(Trim(learnerAssignmentId) == """")
                    {
                        return;
                    }
                    slk_arrOpenedFrameSets[learnerAssignmentId] = null;
                    var gradedPoints = Slk_ParseDouble(computedPoints);    
                    
                    var finalScore = Slk_GradingGetFinalScore(gradedPoints , learnerAssignmentId);
                    document.getElementById(finalScoreId).value 
                                        = finalScore == null ? """" : Slk_FormatDouble(finalScore);    
                    
                    var gradedScoreId = slk_strGradedScorePrefix + learnerAssignmentId ;
                                       
                    var gradedScoreText = gradedPoints == null ? """" : 
                                          Slk_FormatDouble(
                                                     Math.round(gradedPoints*Math.pow(10,2))/Math.pow(10,2));
                    document.getElementById(gradedScoreId).innerText = gradedScoreText == """"? gradedScoreText :
                                (gradedScoreText+""0"").match(/.*\.\d{2}/) || gradedScoreText + "".00"";
                 
                    document.getElementById(gradedScoreId).title 
                                        = gradedPoints == null ?"""": Slk_FormatDouble(gradedPoints) + "" "" + 
                                          document.getElementById(gradedScoreId).ToolTipText;

                    var gradedImageId = slk_strGradedImagePrefix + learnerAssignmentId ;
                 ");
                csGradingClientScript.Append(@"
                    if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Passed.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingSatisfiedSrc;
                    }
                    else if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Failed.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingUnSatisfiedSrc;
                    }
                    else if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Unknown.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingUnKnownSrc;
                    }   
                }                      
		        ");

                csGradingClientScript.AppendLine(@" 
                function Slk_GradingGetFinalScore(modifiedGradedPoints, learnerAssignmentId)
                {
                    var finalPointsElementId = slk_strFinalScorePrefix + learnerAssignmentId ;
                    var finalPoints = Slk_ParseDouble(document.getElementById(finalPointsElementId).value);
                    var actualGradedPoints = arrActualGradedPoints[learnerAssignmentId];
                                   
                    switch((((actualGradedPoints != null)<< 2) |
                            ((finalPoints != null) << 1)|
                            ((modifiedGradedPoints != null)))+ 1)
                    {
                        case 1:
                          break    
                        case 2:                          
                          finalPoints = modifiedGradedPoints;
                          break
                        case 3:
                          break    
                        case 4:
                             finalPoints = finalPoints + modifiedGradedPoints;
                          break
                        case 5:                         
                          break    
                        case 6:
                            var isOverride = confirm(""" +
                culture.Format(culture.Resources.GradingConfirmOverrideFinalPoints, "\"+ modifiedGradedPoints +\"") + @""");");
                csGradingClientScript.AppendLine(@"
                            if(isOverride)
                            {
                                finalPoints = modifiedGradedPoints;
                            }                             
                          break
                        case 7:
                          finalPoints = modifiedGradedPoints;
                          break    
                        case 8:                         
                          var deltaScore =  finalPoints - actualGradedPoints;
                          finalPoints = modifiedGradedPoints + deltaScore  ;
                          break
                        default:
                            finalPoints = null;
                    }
                    arrActualGradedPoints[learnerAssignmentId] = modifiedGradedPoints;
                    
                    return finalPoints;          
                 }");


                //Method for Highlight/UnHighlight the Selected Graded Row. 
                csGradingClientScript.AppendLine(@"             
                 function Slk_GradingHighlightGradingRow(rowId)
                 {
                  "
                );
                csGradingClientScript.Append(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var gradingRowPrefix = \"{0}\";",
                                           GradingRowId
                                          ));
                csGradingClientScript.AppendLine(@"
                    var selectedGradingRowId = gradingRowPrefix + rowId;
                    obj = document.getElementById(selectedGradingRowId);
                    if(obj != null)
                    {
                      if (slk_selectedGradingRowId != null)
                      {
	                    document.getElementById(slk_selectedGradingRowId).className = """";
                      }
                      slk_selectedGradingRowId = obj.id;

                      if (slk_selectedGradingRowId != null)
                      {
	                    obj.className = ""ms-alternating"";
                      }
                    }
                }
                function Slk_GradingFinalScoreDisabled(obj)
                {  
                    if(obj != null && obj.disabled)
                    {
                        alert(obj.title);
                    }                       
                }    

                function Trim(s) 
                {
                   var m = s.match(/^\s*(\S+(\s+\S+)*)\s*$/);
                   return (m == null) ? """" : m[1];
                }

                function Slk_OpenLearnerAssignment(navigateUrl, 
                                                      learnerAssignmentId, 
                                                      isClassServerContent)
                {                   

                    var finalScoreElementId = slk_strFinalScorePrefix + learnerAssignmentId ;

                     if(document.getElementById(finalScoreElementId) != null && 
                        document.getElementById(finalScoreElementId) != undefined)
                    {
                        document.getElementById(finalScoreElementId).disabled = isClassServerContent; 
                    } 

                    if((slk_arrOpenedFrameSets[learnerAssignmentId] != undefined) &&
                       (slk_arrOpenedFrameSets[learnerAssignmentId] != null)&&
                       (!slk_arrOpenedFrameSets[learnerAssignmentId].closed) 
                      )
                    {
                        slk_arrOpenedFrameSets[learnerAssignmentId].focus();
                    } 
                    else
                    {
                        var openWindow = window.open(navigateUrl , ""_blank"");
                        slk_arrOpenedFrameSets[learnerAssignmentId] = openWindow;
                    }
                }       

                function Slk_GradingValidateFinalScore(obj)   
                {
                   if(obj != null)
                    { 
                        var finalScore = obj.value; 
                        if(Trim(finalScore) != """")
                        {                       
                            if(Slk_ParseDouble(finalScore) == null)
                            {");
                csGradingClientScript.Append(@"
                                alert(""");
                csGradingClientScript.Append(culture.Resources.GradingFinalScoreNaNError);
                csGradingClientScript.Append(@""");                         
                                obj.select();
                            }
                        }
                    }
                }         
                function Slk_ParseDouble(str)
                {                    
                    var re = new RegExp(""^\\s*([-\\+])?(\\d*)\\"" + slk_strDecimalChar + ""?(\\d*)\\s*$""); 
                    var m = str.match(re); 
                    if (m == null) 
                        return null; 
                    if (m[2].length == 0 && m[3].length == 0) 
                        return null; 
                    var strCleanInput = (m[1] != null ? m[1] : """") + (m[2].length > 0 ? m[2] : ""0"") +
                        (m[3].length > 0 ? ""."" + m[3] : """"); 
                    var dbl = parseFloat(strCleanInput); 
                    return (isNaN(dbl) ? null : dbl); 
                }
                
                function Slk_FormatDouble(str)
                { 
                  return String(str).replace(/\./g, slk_strDecimalChar); 
                }
                 "
            );

                SlkAppBasePage slkAppBasePage = new SlkAppBasePage();
                string submittedUrl = string.Format("{0}{1}SubmittedFiles.aspx?LearnerAssignmentId=", slkAppBasePage.SPWeb.Url, Constants.SlkUrlPath);
                string submittedJavascript = @"
function openSubmittedFiles(id) {
    var options = SP.UI.$create_DialogOptions();
    options.url = '" + submittedUrl + @"' + id;
    options.allowMaximize = true;
    options.showClose = true;
    options.autoSize = true;
    SP.UI.ModalDialog.showModalDialog(options);
} ";
                csGradingClientScript.AppendLine(submittedJavascript);

                csGradingClientScript.AppendLine("<!-- Grading Client Script Ends Here -->");

                //Register Learner/Learner Group onclick events as ClientScriptBlock
                cs.RegisterClientScriptBlock(cstype, csTitle, csGradingClientScript.ToString(), true);
            }

        }
        #endregion
        #endregion
    }

    /// <summary>
    /// A collection of GradingItem objects in a GradingListcontrol. 
    /// This class cannot be inherited. 
    /// </summary>
    internal sealed class GradingItemCollection : Dictionary<long, GradingItem>, IStateManager
    {
        /// <summary>
        /// Holds Changed Learners Items
        /// </summary> 
        private List<long> m_learnerItemsChanged = new List<long>();

        private bool m_isTracked;

        #region Private and Public Methods
        /// <summary>
        /// Returns Items Changed in Postback
        /// </summary>
        internal List<long> LearnerItemsChanged
        {
            get { return m_learnerItemsChanged; }
        }

        #endregion

        /// <summary>Adds a <see cref="GradingItem"/> to the collection.</summary>
        /// <param name="item">The <see cref="GradingItem"/> to add.</param>
        public void Add(GradingItem item)
        {
            Add(item.LearnerAssignmentId, item);
        }

        #region IStateManager Members
        /// <summary>
        /// TrackingViewState
        /// </summary>
        public bool IsTrackingViewState
        {
            get { return m_isTracked; }
        }

        #region LoadViewState
        /// <summary>
        /// LoadsViewState
        /// </summary>
        /// <param name="state">savedState</param>
        public void LoadViewState(object state)
        {
            if (state != null)
            {
                Triplet obj = (Triplet)state;
                Clear();
                Triplet objState1 = (Triplet)obj.First;
                Triplet objState2 = (Triplet)obj.Second;
                Triplet objState3 = (Triplet)obj.Third;
                Triplet objState4 = (Triplet)objState3.First;

                long[] assignmentId = (long[])objState1.First;
                string[] learnerName = (string[])objState1.Second;
                long[] learnerId = (long[])objState3.Second;
                LearnerItemsChanged.AddRange((long[])objState1.Third);

                LearnerAssignmentState[] status = (LearnerAssignmentState[])objState2.First;
                SuccessStatus[] successStatus = (SuccessStatus[])objState2.Second;
                string[] gradedScore = (string[])objState2.Third;
                string[] finalScore = (string[])objState4.First;
                string[] instructorComments = (string[])objState4.Second;
                string[] grades = (string[])objState3.Third;
                bool[] actionState = (bool[])objState4.Third;

                for (int i = 0; i < assignmentId.Length; i++)
                {
                    GradingItem item = new GradingItem();
                    item.LearnerAssignmentId = assignmentId[i];
                    item.LearnerName = learnerName[i];
                    item.LearnerId = learnerId[i];
                    item.Status = status[i];
                    item.SuccessStatus = successStatus[i];
                    if (!String.IsNullOrEmpty(gradedScore[i]))
                        item.GradedScore = float.Parse(gradedScore[i],
                                                       CultureInfo.CurrentCulture.NumberFormat);
                    if (!String.IsNullOrEmpty(finalScore[i]))
                        item.FinalScore = float.Parse(finalScore[i],
                                                      CultureInfo.CurrentCulture.NumberFormat);
                    item.Grade = grades[i];
                    item.InstructorComments = instructorComments[i];
                    item.ActionState = actionState[i];
                    Add(item);
                }
            }
        }
        #endregion

        #region SaveViewState
        /// <summary>
        /// SaveViewState
        /// </summary>
        /// <returns>savedObject</returns>
        public object SaveViewState()
        {
            if (Count > 0)
            {
                int numOfItems = Count;

                long[] assignmentId = new long[numOfItems];
                long[] learnerId = new long[numOfItems];
                string[] learnerName = new string[numOfItems];
                LearnerAssignmentState[] learnerStatus = new LearnerAssignmentState[numOfItems];
                SuccessStatus[] successStatus = new SuccessStatus[numOfItems];
                string[] gradedScore = new string[numOfItems];
                string[] finalScore = new string[numOfItems];
                string[] instructorComments = new string[numOfItems];
                string[] grades = new string[numOfItems];
                bool[] actionState = new bool[numOfItems];

                int i = 0;
                foreach (GradingItem item in Values)
                {
                    assignmentId[i] = item.LearnerAssignmentId;
                    learnerName[i] = item.LearnerName;
                    learnerId[i] = item.LearnerId;
                    learnerStatus[i] = item.Status;
                    successStatus[i] = item.SuccessStatus;
                    gradedScore[i] = item.GradedScore == null ?  String.Empty : item.GradedScore.Value.ToString(CultureInfo.CurrentCulture);
                    finalScore[i] = item.FinalScore == null ?  String.Empty : item.FinalScore.Value.ToString(CultureInfo.CurrentCulture);
                    grades[i] = item.Grade;
                    instructorComments[i] = item.InstructorComments;
                    actionState[i] = item.ActionState;
                    i++;
                }

                Triplet objState4 = new Triplet(finalScore, instructorComments, actionState);
                Triplet objState1 = new Triplet(assignmentId, learnerName, LearnerItemsChanged.ToArray());
                Triplet objState2 = new Triplet(learnerStatus, successStatus, gradedScore);
                Triplet objState3 = new Triplet(objState4, learnerId, grades);

                return new Triplet(objState1, objState2, objState3);

            }

            return null;

        }
        #endregion

        #region TrackViewState
        /// <summary>
        /// TrackViewState
        /// </summary>
        public void TrackViewState()
        {
            m_isTracked = true;
        }
        #endregion

        #endregion
    }
}
