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
using Microsoft.LearningComponents.Storage;
using Resources;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Represents a page within the Basic Web Player frameset. This is the base class of all frameset pages. This class is not shared
    /// between SLK and BWP framesets.
    /// </summary>
    public class BwpFramesetPage : PageHelper
    {
        private FramesetPageHelper m_helper;

        public string GetMessage(FramesetStringId stringId)
        {
            switch (stringId)
            {
                case FramesetStringId.MoveToNextFailedHtml:
                    return FramesetResources.HID_MoveNextFailedHtml;
                case FramesetStringId.MoveToPreviousFailedHtml:
                    return FramesetResources.HID_MovePreviousFailedHtml;
                case FramesetStringId.MoveToActivityFailedHtml:
                    return FramesetResources.HID_MoveToActivityFailedHtml;
                case FramesetStringId.SubmitPageTitleHtml:
                    return FramesetResources.HID_SubmitPageTitleHtml;
                case FramesetStringId.SubmitPageMessageHtml:
                    return FramesetResources.HID_SubmitPageMessageHtml;
                case FramesetStringId.SubmitPageMessageNoCurrentActivityHtml:
                    return FramesetResources.HID_SubmitPageMessageNoCurrentActivityHtml;
                case FramesetStringId.SubmitPageSaveButtonHtml:
                    return FramesetResources.POST_SubmitHtml;
                case FramesetStringId.CannotDisplayContentTitle:
                    return FramesetResources.FRM_CannotDisplayContentTitle;
                case FramesetStringId.SessionIsNotActiveMsg:
                    return FramesetResources.FRM_SessionIsNotActiveMsg;
                case FramesetStringId.ActivityIsNotActiveMsg:
                    return FramesetResources.FRM_ActivityIsNotActiveMsg;
                case FramesetStringId.SelectActivityTitleHtml:
                    return FramesetResources.HID_SelectActivityTitleHtml;
                case FramesetStringId.SelectActivityMessageHtml:
                    return FramesetResources.HID_SelectActivityMsgHtml;
                default:
                    throw new InvalidOperationException(FramesetResources.FRM_ResourceNotFound);
            }
        }

        public BwpFramesetPage()
            : base()
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

        /// <summary>
        /// Delegate implementation to allow the frameset to take action on a session view request. This allows SLK and 
        /// BWP to have different behavior and messages about which requests are not valid.
        /// </summary>
        public bool ProcessViewRequest(SessionView view, LearningSession session)
        {
            switch (view)
            {
                case SessionView.Execute:
                    {
                        StoredLearningSession slsSession = session as StoredLearningSession;
                        if (slsSession != null)
                        {
                            if (slsSession.AttemptStatus == AttemptStatus.Completed)
                            {
                                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidAttemptStatusForViewTitle),
                                         ResHelper.GetMessage(FramesetResources.FRM_ExecuteViewCompletedSessionMsg), false);
                                return false;
                            }
                            else if (slsSession.AttemptStatus == AttemptStatus.Abandoned)
                            {
                                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidAttemptStatusForViewTitle),
                                    ResHelper.GetMessage(FramesetResources.FRM_ExecuteViewAbandonedSessionMsg), false);
                                return false;
                            }
                        }
                    }
                    break;

                case SessionView.Review:
                    // BWP does not provide review view
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_ViewNotSupportedTitle),
                                         ResHelper.GetMessage(FramesetResources.FRM_ReviewViewNotSupportedMsg), false);
                    break;

                case SessionView.RandomAccess:
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_ViewNotSupportedTitle),
                                        ResHelper.GetMessage(FramesetResources.FRM_RAViewNotSupportedMsg), false);
                    break;
            }
            return true;
        }

        /// <summary>
        /// Register error information to be written to the response object.
        /// Only the first error that is registered is displayed.
        /// </summary>
        /// <param name="shortDescription">A short description (sort of a title) of the problem. Html format.</param>
        /// <param name="message">A longer description of the problem. Html format.</param>
        /// <param name="asInfo">If true, display as information message.</param>
        protected virtual void RegisterError(string shortDescription, string message, bool asInfo)
        {
            FramesetHelper.RegisterError(shortDescription, message, asInfo);
        }

        /// <summary>
        /// Clear error information that is waiting to be written.
        /// </summary>
        protected void ClearError()
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
        public bool GetRequiredParameter(string name, out string value)
        {
            return FramesetHelper.TryGetRequiredParameter(name, out value);
        }

        /// <summary>
        /// Helper function to get the AttemptId query parameter. This method assumes the parameter is required. If 
        /// it does not exist or is not numeric, the error page is shown. This method does not check LearningStore.
        /// </summary>
        /// <param name="isRequired">If true, the error page is shown if the value is not provided.</param>
        /// <param name="attemptId">The attempt id.</param>
        /// <returns>If false, the value did not exist or was not valid. The application should not continue with 
        /// page processing.</returns>
        public bool ProcessAttemptIdParameter(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            string attemptParam = null;
            bool isValid = true;

            attemptId = null;

            if (!GetRequiredParameter(FramesetQueryParameter.AttemptId, out attemptParam))
                return false;

            // Try converting it to a long value. It must be positive.
            try
            {
                long attemptKey = long.Parse(attemptParam, NumberFormatInfo.InvariantInfo);

                if (attemptKey <= 0)
                    isValid = false;
                else
                    attemptId = new AttemptItemIdentifier(attemptKey);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            if (!isValid && showErrorPage)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.AttemptId),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.AttemptId, attemptParam), false);
            }

            return isValid;
        }

        /// <summary>
        /// Helper function to process the View parameter. This method assumes the parameter is required. If it does not 
        /// exist or is not a valid value and showErrorPage=true, the error page is shown and the method returns false. 
        /// If false is returned, the caller should ignore the value of <param name="view"/>.    
        /// </summary>
        public bool ProcessViewParameter(bool showErrorPage, out SessionView view)
        {
            return FramesetHelper.TryProcessViewParameter(showErrorPage, out view);
        }

        public static void AppendContentFrameDetails(LearningSession session, StringBuilder sb)
        {
            // The URL for attempt-based content frames is:
            // http://<...basicWebApp>/Content.aspx/<view>/<attemptId>/otherdata/
            // the otherdata depends on the view
            sb.Append(String.Format(CultureInfo.CurrentCulture, "/{0}", Convert.ToInt32(session.View)));

            StoredLearningSession slsSession = session as StoredLearningSession;
            sb.AppendFormat("/{0}", slsSession.AttemptId.GetKey().ToString());
        }

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

        public bool ErrorAsInfo
        {
            get
            {
                return FramesetHelper.ErrorAsInfo;
            }
        }

        /// <summary>
        /// Return all previously registered error information.
        /// </summary>
        public void GetErrorInfo(out bool hasError, out string errorTitle, out string errorMsg, out bool asInfo)
        {
            hasError = HasError;
            errorTitle = ErrorTitle;
            errorMsg = ErrorMsg;
            asInfo = ErrorAsInfo;
        }
    }
}
