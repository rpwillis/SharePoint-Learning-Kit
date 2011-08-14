/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.LearningComponents;
using System.Globalization;
using Microsoft.LearningComponents.Frameset;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Resources;
using System.Text;
using Resources.Properties;
using System.Diagnostics.CodeAnalysis;
using System.Threading;


namespace Microsoft.SharePointLearningKit.Frameset
{
    /// <summary>
    /// Represents a page within the SLK frameset. This is the base class of all frameset pages. This class is not shared
    /// between SLK and BWP framesets.
    /// </summary>
    public class FramesetPage : SlkAppBasePage
    {
        private FramesetPageHelper m_helper;
        private Guid m_learnerAssignmentGuidId;
        private LearnerAssignmentProperties learnerAssignmentProperties;
        
        /////////////////////////////////////////////////////////////////////////////////////
        private SlkStore m_observerRoleLearnerStore;

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

        /////////////////////////////////////////////////////////////////////////////////////


        /// <summary>Initializes a new instance of <see cref="FramesetPage"/>.</summary>
        public FramesetPage() : base()
        {
        }

        private FramesetPageHelper FramesetHelper
        {
            get
            {
                if (m_helper == null)
                {
                    m_helper = new FramesetPageHelper(Request);
                }
                return m_helper;
            }
        }

        /// <summary>Process a request for a view. If not allowed, register an error and return false.</summary>
        public bool ProcessViewRequest(SessionView view, LearningSession session)
        {
            LearnerAssignmentProperties la = GetLearnerAssignment();

            LearnerAssignmentState state = la.Status == null ? LearnerAssignmentState.NotStarted : la.Status.Value;
            return ProcessViewRequest(state, view);
        }

        /// <summary>Process a view request to determine if it's valid. The AssignmentView must be set before calling this method.</summary>
        protected bool ProcessViewRequest(LearnerAssignmentState? learnerStatus, SessionView sessionView)
        {
            if (learnerStatus == null)
            {
                return ProcessViewRequest(LearnerAssignmentState.NotStarted, sessionView);
            }
            else
            {
                return ProcessViewRequest(learnerStatus.Value, sessionView);
            }
        }

        /// <summary>Process a view request to determine if it's valid. The AssignmentView must be set before calling this method.</summary>
        protected bool ProcessViewRequest(LearnerAssignmentState learnerStatus, SessionView sessionView)
        {
            switch (AssignmentView)
            {
                case AssignmentView.Execute:
                    {
                        // Verify that session view matches what you expect
                        if (sessionView != SessionView.Execute)
                        {
                            throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
                        }

                        // Can only access active assignments in Execute view
                        if (learnerStatus != LearnerAssignmentState.Active)
                        {
                            RegisterError(SlkFrameset.FRM_AssignmentNotAvailableTitle,
                                SlkFrameset.FRM_AssignmentTurnedInMsgHtml, false);

                            return false;
                        }
                        break;
                    }
                case AssignmentView.Grading:
                    {
                        // Verify that session view matches what you expect
                        if (sessionView != SessionView.Review)
                        {
                            throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
                        }

                        // Grading is not available if the assignment has not been submitted.
                        if ((learnerStatus == LearnerAssignmentState.Active)
                            || (learnerStatus == LearnerAssignmentState.NotStarted))
                        {
                            RegisterError(SlkFrameset.FRM_AssignmentNotGradableTitle,
                             SlkFrameset.FRM_AssignmentCantBeGradedMsgHtml, false);
                            return false;
                        }
                        break;
                    }
                case AssignmentView.InstructorReview:
                    {
                        // Verify that session view matches what you expect
                        if (sessionView != SessionView.Review)
                        {
                            throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
                        }

                        // Only available if student has started the assignment
                        if (learnerStatus == LearnerAssignmentState.NotStarted)
                        {
                            RegisterError(SlkFrameset.FRM_ReviewNotAvailableTitle,
                             SlkFrameset.FRM_ReviewNotAvailableMsgHtml, false);
                            return false;
                        }

                        break;
                    }
                case AssignmentView.StudentReview:
                    {
                        // Verify that session view matches what you expect
                        if (sessionView != SessionView.Review)
                        {
                            throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
                        }

                        // If the user is an observer and the assignment state is equal to 'Completed' or 'NotStarted',
                        // then register error message
                        if (SlkStore.IsObserver(SPWeb) && ((learnerStatus == LearnerAssignmentState.Completed) || (learnerStatus == LearnerAssignmentState.NotStarted)))
                        {
                            RegisterError(SlkFrameset.FRM_ObserverReviewNotAvailableTitle, SlkFrameset.FRM_ObserverReviewNotAvailableMsgHtml, false);
                            return false;
                        }
                        // If requesting student review, the assignment state must be final
                        if ( !SlkStore.IsObserver(SPWeb) && learnerStatus != LearnerAssignmentState.Final)
                        {
                            RegisterError(SlkFrameset.FRM_ReviewNotAvailableTitle,
                                SlkFrameset.FRM_LearnerReviewNotAvailableMsgHtml, false);
                            return false;
                        }

                        break;
                    }
                default:
                    break;
            }
            return true;
        }

        /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnInit"/>.</summary>
        protected override void OnInit(EventArgs e)
        {
            SlkFrameset.Culture = Thread.CurrentThread.CurrentCulture;
            base.OnInit(e);
        }

        /// <summary>
        /// Register error information to be written to the response object.
        /// Only the first error that is registered is displayed.
        /// </summary>
        /// <param name="shortDescription">A short description (sort of a title) of the problem. Html format.</param>
        /// <param name="message">A longer description of the problem. Html format.</param>
        /// <param name="asInfo">If true, message is show with Info icon. Otherwise, it's an error.</param>
        protected virtual void RegisterError(string shortDescription, string message, bool asInfo)
        {
            FramesetHelper.RegisterError(shortDescription, message, asInfo);
        }

        /// <summary>
        /// Clears the current error state.
        /// </summary>
        protected virtual void ClearError()
        {
            FramesetHelper.ClearError();
        }

        /// <summary>
        /// Returns true, and the value of the required parameter. If the parameter is not found or has no value,
        /// this method will display the error page.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter. The value should be ignored if false is returned.</param>
        /// <returns>True if the parameter existed.</returns>
        public bool TryGetRequiredParameter(string name, out string value)
        {
            return FramesetHelper.TryGetRequiredParameter(name, out value);
        }

        /// <summary>The learner assignment id.</summary>
        protected Guid LearnerAssignmentGuidId
        {
            get 
            {
                if (m_learnerAssignmentGuidId.Equals(Guid.Empty) == true)
                {
                    TryProcessLearnerAssignmentIdParameter(false, out m_learnerAssignmentGuidId);
                }
                return m_learnerAssignmentGuidId; 
            }
            set { m_learnerAssignmentGuidId = value; }
        }

        /// <summary>
        /// Provides a cached version of learner assignment properties.
        /// </summary>
        /// <returns></returns>
        protected LearnerAssignmentProperties GetLearnerAssignment()
        {
            return GetLearnerAssignment(false);
        }

        /// <summary>
        /// Provides an option to override and update the cached data about the 
        /// current learner assignment.
        /// </summary>
        /// <param name="forceUpdate">If false, the value is cached.</param>
        /// <returns></returns>
        protected LearnerAssignmentProperties GetLearnerAssignment(bool forceUpdate)
        {
            // If the current value has not been set, or if we have to update it, get 
            // the information from the base class.
            if (learnerAssignmentProperties == null || forceUpdate)
            {
                SlkRole slkRole;
                if (SlkStore.IsObserver(SPWeb) && AssignmentView == AssignmentView.StudentReview)
                {
                    // If the user is an observer and the AssignmentView is 'StudentReview', then
                    // set the slkrole to 'Observer'; Otherwise get the role using GetSlkRole method
                    slkRole = SlkRole.Observer;
                }
                else
                {
                    slkRole = GetSlkRole(AssignmentView);
                }

                AssignmentProperties assignment = SlkStore.LoadAssignmentPropertiesForLearner(LearnerAssignmentGuidId, slkRole);
                learnerAssignmentProperties = assignment.Results[0];
            }

            return learnerAssignmentProperties;
        }

        /// <summary>
        /// Get the attempt id requested for this session. This includes reading the LearnerAssignmentId parameter.
        /// </summary>
        /// <returns>Returns true if the attemptId is found. This method does not verify that the attemptId 
        /// exists in LearningStore and does not create an attempt if the value is invalid.</returns>
        public virtual bool TryGetAttemptId(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            // If there is an attempt id parameter, get it and return it. Don't ever show an error that AttemptId 
            // was not provided. (The error will be that LearnerAssignmentId was not provided.)
            if (ProcessAttemptIdParameter(false, out attemptId))
                return true;

            // If there's a LearnerAssignmentId, then you can use this to get the AttemptId, however it 
            // causes a database call. Not good for routine page rendering (but ok the first time a frame 
            // is shown).
            Guid learnerAssignmentGuidId;
            if (TryProcessLearnerAssignmentIdParameter(showErrorPage, out learnerAssignmentGuidId))
            {
                ClearError();

                // There was a learnerAssignmentId and no AttemptId, so translate from learnerAssignmentId to attemptId
                LearnerAssignmentGuidId = learnerAssignmentGuidId;
                LearnerAssignmentProperties la = GetLearnerAssignment();  // this causes LearningStoreAccess
                attemptId = la.AttemptId;        
                return (attemptId != null);
            }
            return false;
        }

        /// <summary>
        /// Gets the attempt id required to render the page. Uses the FramesetQueryParameter.LearnerAssignmentId 
        /// to determine attempt information.
        /// </summary>
        /// <param name="showErrorPage">Whether to show the error page or not.</param>
        /// <param name="learnerAssignmentGuidId">The id of the learner assignment.</param>
        /// <returns></returns>
        protected bool TryProcessLearnerAssignmentIdParameter(bool showErrorPage, out Guid learnerAssignmentGuidId)
        {
            // Initialize out parameter
            learnerAssignmentGuidId = Guid.Empty;

            string learnerAssignmentParam;
            if (!TryGetRequiredParameter(FramesetQueryParameter.LearnerAssignmentId, out learnerAssignmentParam))
                return false;

            bool isValid = true;    // Assume return value is valid

            // Try converting it to a guid value.
            try
            {
                Guid learnerAssignmentKey = new Guid(learnerAssignmentParam);
                learnerAssignmentGuidId = learnerAssignmentKey;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            if (!isValid && showErrorPage)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.LearnerAssignmentId),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.LearnerAssignmentId, learnerAssignmentParam), false);
            }

            return isValid;
        }

        /// <summary>
        /// Helper function to process the parameters to get SessionView. This method assumes the AssignmentView
        /// parameter is required. If it does not 
        /// exist or is not a valid value and showErrorPage=true, the error page is shown and the method returns false. 
        /// If false is returned, the caller should ignore the value of <paramref name="view"/>.    
        /// </summary>
        /// <param name="showErrorPage">If true, an error will be registered to show to the user.</param>
        /// <param name="view">The view that is being requested.</param>
        public virtual bool TryGetSessionView(bool showErrorPage, out SessionView view)
        {
            view = SessionView.Execute; // must be initialized

            if (!TryProcessAssignmentViewParameter(showErrorPage, out m_assignmentView))
                return false;

            view = GetSessionView(m_assignmentView);

            return true;
        }

        /// <summary>
        /// Helper function to process the AssignmentView parameter. This method assumes the parameter is required. If it does not 
        /// exist or is not a valid value and showErrorPage=true, the error page is shown and the method returns false. 
        /// If false is returned, the caller should ignore the value of <paramref name="view"/>. 
        /// </summary>
        /// <param name="showErrorPage"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public bool TryProcessAssignmentViewParameter(bool showErrorPage, out AssignmentView view)
        {
            string viewParam;

            // Default value to make compiler happy
            view = AssignmentView.Execute;

            if (!TryGetRequiredParameter(FramesetQueryParameter.SlkView, out viewParam))
                return false;

            try
            {
                // Get the view enum value
                view = (AssignmentView)Enum.Parse(typeof(AssignmentView), viewParam, true);
                if ((view < AssignmentView.Execute) || (view > AssignmentView.Grading))
                {
                    if (showErrorPage)
                    {
                        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.SlkView),
                                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.SlkView, viewParam), false);
                    }
                    return false;
                }
            }
            catch (ArgumentException)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.SlkView),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.SlkView, viewParam), false);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the SessionView that corresponds to the AssignmentView.
        /// </summary>
        protected static SessionView GetSessionView(AssignmentView assignView)
        {
            switch (assignView)
            {
                case AssignmentView.Execute:
                    return SessionView.Execute;

                case AssignmentView.Grading:
                    //return SessionView.RandomAccess;
                    return SessionView.Review;

                case AssignmentView.InstructorReview:
                    return SessionView.Review;

                case AssignmentView.StudentReview:
                    return SessionView.Review;
            }
            // default value -- this would be an error
            return SessionView.Execute;
        }

        private AssignmentView m_assignmentView;

        /// <summary>The assignment view.</summary>
        protected AssignmentView AssignmentView
        {
            get {  return m_assignmentView;  }
            set { m_assignmentView = value; }
        }

        /// <summary>
        /// Returns the SlkRole tied to the requested assignment view.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected static SlkRole GetSlkRole(AssignmentView view)
        {
            switch (view)
            {
                case AssignmentView.Execute:
                    return SlkRole.Learner;
                case AssignmentView.Grading:
                    return SlkRole.Instructor;
                case AssignmentView.InstructorReview:
                    return SlkRole.Instructor;
                case AssignmentView.StudentReview:
                    return SlkRole.Learner;
            }
            // This would be bad
            return SlkRole.None;
        }

        /// <summary>Add details to the content.aspx URL to provide information about the learner assignment</summary>
        /// <param name="session"></param>
        /// <param name="sb"></param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameters are validated
        protected void AppendContentFrameDetails(LearningSession session, StringBuilder sb)
        {
            FramesetUtil.ValidateNonNullParameter("sb", sb);
            FramesetUtil.ValidateNonNullParameter("session", session);

            // The URL for attempt-based content frames is:
            // http://<...basicWebApp>/Content.aspx/<AssignmentView>/<LearnerAssignmentId>/otherdata/
            // the otherdata depends on the view
            sb.Append(String.Format(CultureInfo.InvariantCulture, "/{0}", Convert.ToInt32(AssignmentView, NumberFormatInfo.InvariantInfo)));

            StoredLearningSession slsSession = session as StoredLearningSession;

            if (slsSession == null)
            {
                // Not attempt-based view
            }
            else
            {
                sb.AppendFormat("/{0}", LearnerAssignmentGuidId.ToString());

                // In review & ra views, append the current activity id
                if ((slsSession.View == SessionView.Review) || (slsSession.View == SessionView.RandomAccess)) 
                {
                    sb.AppendFormat("/{0}", FramesetUtil.GetStringInvariant(slsSession.CurrentActivityId));
                }
            }
        }

        /// <summary>
        /// Helper function to process the AttemptId parameter. This method assumes the parameter is required. If it does not 
        /// exist or is not a valid value and showErrorPage=true, the error page is shown and the method returns false. 
        /// If false is returned, the caller should ignore the value of <paramref name="attemptId"/>.    
        /// </summary>
        /// <param name="showErrorPage">If true, an error should be written.</param>
        /// <param name="attemptId">The attempt id returned.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public bool ProcessAttemptIdParameter(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            return FramesetHelper.TryProcessAttemptIdParameter(showErrorPage, out attemptId);
        }

        /// <summary>Writes the slk javascript initialization.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        protected static void WriteSlkMgrInit(StringBuilder sb)
        {
            sb.AppendLine("function FindSlkFrmMgr(win) {");
            sb.AppendLine("var frmDepthCount = 0;");
            sb.AppendLine("while ((win.g_slkFrameMgr == null) && (win.parent != null) && (win.parent != win)) {");
            sb.AppendLine("\tfrmDepthCount++;");
            sb.AppendLine("\tif (frmDepthCount > 20)");
            sb.AppendLine("\t\t{ return null; }");
            sb.AppendLine("win = win.parent;");
            sb.AppendLine("}");
            sb.AppendLine("return win.g_slkFrameMgr;");
            sb.AppendLine("}");
            sb.AppendLine("function Slk_GetSlkManager(){");
            sb.AppendLine("return FindSlkFrmMgr(window); } ");
            sb.AppendLine("var slkMgr;");
        }

        /// <summary>Whether an error has occurred or not.</summary>
        protected bool HasError
        {
            get { return FramesetHelper.HasError; }
        }

        /// <summary>
        /// If true, show the error page instead of the page
        /// </summary>
        public bool ShowError
        {
            get { return FramesetHelper.ShowError; }
        }

        /// <summary>
        /// Return the short title of the error.
        /// </summary>
        public string ErrorTitle
        {
            get
            {
                return FramesetHelper.ErrorTitle;
            }
        }

        /// <summary>
        /// Return the long(er) error message.
        /// </summary>
        public string ErrorMsg
        {
            get
            {
                return FramesetHelper.ErrorMsg;
            }
        }

        /// <summary>Whether to show the error as information.</summary>
        public bool ErrorAsInfo
        {
            get { return FramesetHelper.ErrorAsInfo; }
        }

        /// <summary>
        /// Return all previously registered error information.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]  
        public void GetErrorInfo(out bool hasError, out string errorTitle, out string errorMsg, out bool asInfo)
        {
            hasError = HasError;
            errorTitle = ErrorTitle;
            errorMsg = ErrorMsg;
            asInfo = ErrorAsInfo;
        }

        /// <summary>Gets an appropriate message.</summary>
        /// <param name="stringId">The id of the message.</param>
        /// <returns>The message.</returns>
        public string GetMessage(FramesetStringId stringId)
        {
            switch (stringId)
            {
                case FramesetStringId.MoveToNextFailedHtml:
                    return SlkFrameset.HID_MoveNextFailedHtml;

                case FramesetStringId.MoveToPreviousFailedHtml:
                    return SlkFrameset.HID_MovePreviousFailedHtml;

                case FramesetStringId.MoveToActivityFailedHtml:
                    return SlkFrameset.HID_MoveToActivityFailedHtml;

                case FramesetStringId.SubmitPageTitleHtml:
                    if (AssignmentView == AssignmentView.Execute)
                        return SlkFrameset.HID_SubmitPageTitleHtml;
                    else
                        return SlkFrameset.HID_SubmitGradingPageTitleHtml;

                case FramesetStringId.SubmitPageMessageHtml:
                    if (AssignmentView == AssignmentView.Execute)
                        return SlkFrameset.HID_SubmitPageMessageHtml;
                    else
                        return SlkFrameset.HID_SubmitGradingPageMessageHtml;

                case FramesetStringId.SubmitPageMessageNoCurrentActivityHtml:
                    // This should not happen in grading view
                    return SlkFrameset.HID_SubmitPageMessageNoCurrentActivityHtml;
                    
                case FramesetStringId.SubmitPageSaveButtonHtml:
                    if (AssignmentView == AssignmentView.Execute)
                        return SlkFrameset.FRM_SubmitPageBtnHtml;
                    else
                        return SlkFrameset.FRM_SubmitGradingPageBtnHtml;

                case FramesetStringId.CannotDisplayContentTitle:
                    return SlkFrameset.FRM_CannotDisplayContentTitle;

                case FramesetStringId.SessionIsNotActiveMsg:
                    return SlkFrameset.FRM_SessionIsNotActiveMsg;

                case FramesetStringId.SelectActivityTitleHtml:
                    return SlkFrameset.HID_SelectActivityTitleHtml;

                case FramesetStringId.SelectActivityMessageHtml:
                    return SlkFrameset.HID_SelectActivityMessageHtml;

                case FramesetStringId.ActivityIsNotActiveMsg:
                    return FramesetResources.FRM_ActivityIsNotActiveMsg;

                default:
                    throw new InternalErrorException(SlkFrameset.FRM_ResourceNotFound);
            }
        }
    }

    /// <summary>
    /// Requested view, in terms of the assignment
    /// </summary>
    public enum AssignmentView
    {
        /// <summary>Execute the assignment.</summary>
        Execute = 0,
        /// <summary>Instuctor is reviewing.</summary>
        InstructorReview,
        /// <summary>Student is reviewing.</summary>
        StudentReview,
        /// <summary>Instructor is grading.</summary>
        Grading
    }
}
