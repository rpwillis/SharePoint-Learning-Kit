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
using Microsoft.LearningComponents.Storage;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{

    /// <summary>
    /// Changes the current activity and notifies the frameset.
    /// </summary>
    public class ChangeActivityHelper : PostableFrameHelper // technically, this is not postable, but has some of the same requirements
    {
        private SessionView m_view;
        private AttemptItemIdentifier m_attemptId;
        private long m_activityId;

        public ChangeActivityHelper(HttpRequest request, HttpResponse response) : base(request, response, null) { }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public void ProcessPageLoad(TryGetViewInfo TryGetViewInfo,
                                    TryGetAttemptInfo TryGetAttemptInfo,
                                    TryGetActivityInfo TryGetActivityInfo,
                                    RegisterError registerError,
                                    GetErrorInfo getErrorInfo,
                                    GetFramesetMsg getFramesetMessage)
        {
            RegisterError = registerError;
            GetErrorInfo = getErrorInfo;
            GetFramesetMsg = getFramesetMessage;

            // This page simply causes the frameset to request to move to a different activity. It does not verify that 
            // the user is authorized to do this. If the user is not authorized, the subsequent request will fail.

            if (!TryGetViewInfo(true, out m_view))
            {
                return;
            }

            if (!TryGetAttemptInfo(true, out m_attemptId))
            {
               return;
            }

            if (!TryGetActivityInfo(true, out m_activityId))
            {
                return;
            }
        }

        public void WriteFrameMgrInit()
        {
            Response.Write(ResHelper.Format("frameMgr.SetAttemptId({0});\r\n", FramesetUtil.GetString(m_attemptId)));
            Response.Write(ResHelper.Format("frameMgr.SetView({0});\r\n", FramesetUtil.GetString(m_view)));
            Response.Write("frameMgr.SetPostFrame(\"frameHidden\");\r\n");
            Response.Write("frameMgr.SetPostableForm(GetHiddenFrame().contentWindow.document.forms[0]);\r\n");

            // Tell frameMgr to move to new activity
            Response.Write(ResHelper.Format("frameMgr.DoChoice(\"{0}\", true);\r\n", FramesetUtil.GetStringInvariant(m_activityId)));
        }

        public string ErrorMessageHtml { get { return ErrorMessage; } }
        public string ErrorTitleHtml { get { return ErrorTitle; } }
    }
}