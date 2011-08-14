/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

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
using Microsoft.LearningComponents.Storage;

namespace Microsoft.LearningComponents.Frameset
{
    // Query parameters on this page:
    // AttemptId = 
    // View = 
    // ActivityId =     // the new activity id
    public partial class Frameset_ChangeActivity : BwpFramesetPage
    {
        private ChangeActivityHelper m_helper;

        bool m_pageLoadSuccessful = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                m_helper = new ChangeActivityHelper(Request, Response);
                m_helper.ProcessPageLoad(ProcessViewParameter,
                                    ProcessAttemptIdParameter,
                                    TryGetActivityId,  
                                    RegisterError,
                                    GetErrorInfo,
                                    GetMessage);
                m_pageLoadSuccessful = (!HasError);
            }
            catch (Exception e2)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionMsg, HttpUtility.HtmlEncode(e2.Message)), false);
                m_pageLoadSuccessful = false;

                // Clear any existing response information so that the error gets rendered correctly.
                Response.Clear();   
            }
        }

        //public bool TryGetSessionView(bool showErrorPage, out SessionView view)
        //{
        //    string viewParam;

        //    // Default value to make compiler happy
        //    view = SessionView.Execute;

        //    if (!TryGetRequiredParameter(FramesetQueryParameter.View, out viewParam))
        //        return false;

        //    try
        //    {
        //        // Get the view enum value
        //        view = (SessionView)Enum.Parse(typeof(SessionView), viewParam, true);
        //        if ((view < SessionView.Execute) || (view > SessionView.Review))
        //        {
        //            if (showErrorPage)
        //            {
        //                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
        //                                ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
        //            }
        //            return false;
        //        }
        //    }
        //    catch (ArgumentException)
        //    {
        //        if (showErrorPage)
        //        {
        //            RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
        //                ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, viewParam), false);
        //        }
        //        return false;
        //    }
        //    return true;
        //}

        public bool TryGetActivityId(bool showErrorPage, out long activityId)
        {
            string activityIdParam = null;
            bool isValid = true;

            activityId = -1;

            if (!GetRequiredParameter(FramesetQueryParameter.ActivityId, out activityIdParam))
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

        //public override bool TryGetAttemptId(bool showErrorPage, out AttemptItemIdentifier attemptId)
        //{
        //    string attemptIdParam = null;
        //    bool isValid = true;

        //    // make compiler happy
        //    attemptId = null;

        //    if (!TryGetRequiredParameter(FramesetQueryParameter.AttemptId, out attemptIdParam))
        //        return false;

        //    // Try converting it to a long value. It must be positive.
        //    try
        //    {
        //        long attemptIdKey = long.Parse(attemptIdParam, NumberFormatInfo.InvariantInfo);

        //        if (attemptIdKey <= 0)
        //            isValid = false;
        //        else
        //            attemptId = new AttemptItemIdentifier(attemptIdKey);
        //    }
        //    catch (FormatException)
        //    {
        //        isValid = false;
        //    }

        //    if (!isValid && showErrorPage)
        //    {
        //        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.AttemptId),
        //                ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.AttemptId, attemptIdParam), false);
        //    }

        //    return isValid;
        //}

        #region called from aspx

        public string ErrorTitleHtml {
            get { return m_helper.ErrorTitleHtml; }
        }

        public string ErrorMsgHtml { 
            get { return m_helper.ErrorMessageHtml; } 
        }

        public static string PleaseWaitHtml {
            get { 
                return ResHelper.GetMessage(FramesetResources.CON_PleaseWait); }
        }

        public void WriteFrameMgrInit()
        {
            // If the page did not load successfully, then don't write anything
            if (!m_pageLoadSuccessful)
                return;

            m_helper.WriteFrameMgrInit();
        }
        #endregion

    }
}