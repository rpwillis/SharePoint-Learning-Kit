/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Resources;
using System.Globalization;
using Microsoft.LearningComponents.Frameset;
using System.Diagnostics.CodeAnalysis;
using Resources.Properties;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit.Frameset
{
    // Query parameters on this page:
    // AttemptId = 
    // View = 
    // ActivityId =     // the new activity id
    public partial class ChangeActivity : FramesetPage
    {
        private ChangeActivityHelper m_helper;

        bool m_pageLoadSuccessful = true;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                SlkUtilities.RetryOnDeadlock(delegate()
                {
                    m_helper = new ChangeActivityHelper(Request, Response);
                    m_helper.ProcessPageLoad(TryGetSessionView,
                                        TryGetAttemptId,
                                        TryGetActivityId,
                                        RegisterError,
                                        GetErrorInfo,
                                        GetMessage);
                    m_pageLoadSuccessful = (!HasError);
                });
            }
            catch (Exception e2)
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                SlkFrameset.Culture = LocalizationManager.GetCurrentCulture();
                // Unexpected exceptions are not shown to user
                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, FramesetResources.FRM_UnknownExceptionMsg, e2.ToString());
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedExceptionMsg), false);
                m_pageLoadSuccessful = false;

                // Clear the response in case something has been written
                Response.Clear();
            }
        }

        public override bool TryGetSessionView(bool showErrorPage, out SessionView view)
        {
            string viewParam;

            // Default value to make compiler happy
            view = SessionView.Execute;

            if (!TryGetRequiredParameter(FramesetQueryParameter.View, out viewParam))
                return false;

            try
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();

                // Get the view enum value
                view = (SessionView)Enum.Parse(typeof(SessionView), viewParam, true);
                if ((view < SessionView.Execute) || (view > SessionView.Review))
                {
                    if (showErrorPage)
                    {
                        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
                    }
                    return false;
                }
            }
            catch (ArgumentException)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
                }
                return false;
            }
            return true;
        }

        public bool TryGetActivityId(bool showErrorPage, out long activityId)
        {
            string activityIdParam = null;
            bool isValid = true;

            activityId = -1;
            FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
            if (!TryGetRequiredParameter(FramesetQueryParameter.ActivityId, out activityIdParam))
                return false;


            // Try converting it to a long value. It must be positive.
            try
            {
                long activityIdKey = long.Parse(activityIdParam, NumberFormatInfo.InvariantInfo);

                if (activityIdKey <= 0)
                    isValid = false;
                else
                    activityId = activityIdKey;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            if (!isValid && showErrorPage)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.ActivityId),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.ActivityId, activityIdParam), false);
            }

            return isValid;
        }

        public override bool TryGetAttemptId(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            string attemptIdParam = null;
            bool isValid = true;

            // make compiler happy
            attemptId = null;
            FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
            if (!TryGetRequiredParameter(FramesetQueryParameter.AttemptId, out attemptIdParam))
                return false;

            // Try converting it to a long value. It must be positive.
            try
            {
                long attemptIdKey = long.Parse(attemptIdParam, NumberFormatInfo.InvariantInfo);

                if (attemptIdKey <= 0)
                    isValid = false;
                else
                    attemptId = new AttemptItemIdentifier(attemptIdKey);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            if (!isValid && showErrorPage)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.AttemptId),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.AttemptId, attemptIdParam), false);
            }

            return isValid;
        }

        #region called from aspx

        public string ErrorTitleHtml {
            get { return m_helper.ErrorTitleHtml; }
        }

        public string ErrorMsgHtml { 
            get { return m_helper.ErrorMessageHtml; } 
        }

        public string HasErrorHtml { get { return (HasError ? "true" : "false"); } }

        public static string PleaseWaitHtml {
            get
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture(); 
                return ResHelper.GetMessage(FramesetResources.CON_PleaseWait); 
            }
        }

        public void WriteFrameMgrInit()
        {
            if (!m_pageLoadSuccessful)
                return;

            m_helper.WriteFrameMgrInit();
        }

        // Gets url path to the SLK folder that contains our images, theme, etc.
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public Uri SlkEmbeddedUIPath
        {
            get
            {
                return new Uri(Request.Url, "/_layouts/SharePointLearningKit/Frameset/");
            }
        }
        #endregion

    }
}