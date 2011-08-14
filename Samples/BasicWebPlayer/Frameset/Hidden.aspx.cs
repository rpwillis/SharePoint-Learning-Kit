/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Xml;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.DataModel;
using Microsoft.LearningComponents.Storage;
using Resources;
using System.Web;

// This file contains the BWP-specific hidden frame rendering code. Most of the actual work is done in the code shared
// with SLK.

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Parameters to this page:
    ///     AttemptId = current attempt id
    ///     View = requested view
    ///     I = (value is ignored). If present, the page is being display during initialization of the frameset.
    /// </summary>
    public partial class Frameset_Hidden : BwpFramesetPage
    {
        HiddenHelper m_hiddenHelper;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                m_hiddenHelper = new HiddenHelper(Request, Response, FramesetPath);
                m_hiddenHelper.ProcessPageLoad(PStore, GetSessionTitle, ProcessViewParameter, ProcessAttemptIdParameter,
                                                AppendContentFrameDetails, RegisterError, GetErrorInfo, ProcessSessionEnd, ProcessViewRequest,
                                                GetMessage, IsPostBack);
            }
            catch (ThreadAbortException)
            {
                // Do nothing -- thread is leaving.
            }
            catch (Exception ex)
            {
                ClearError();
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                                ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionMsg, HttpUtility.HtmlEncode(ex.Message.Replace("\r\n", " "))), false);
            }
        
        }

        /// <summary>
        /// Render all hidden controls on to the page. 
        /// </summary>
        public void WriteHiddenControls()
        {
            m_hiddenHelper.WriteHiddenControls();
        }

        /// <summary>
        /// Write the script to initialize the frameset manager.
        /// </summary>
        public void WriteFrameMgrInit()
        {
            m_hiddenHelper.WriteFrameMgrInit();
        }

        /// <summary>
        /// Gets the title to display in the session.
        /// </summary>
        public PlainTextString GetSessionTitle(LearningSession session)
        {
            return new PlainTextString(session.Title);
        }

        /// <summary>
        /// Allows the app to take action when the session is ending.
        /// </summary>
        public void ProcessSessionEnd(LearningSession session, ref string messageTitle, ref string message)
        {
            // Session ending results in message shown to the user. 
            if (session.View == SessionView.Execute)
            {
                StoredLearningSession slsSession = session as StoredLearningSession;
                if (slsSession != null)
                {
                    // The rollup and/or sequencing process may have changed the state of the attempt. If so, there are some cases
                    // that cannot continue so show an error message. 
                    switch (slsSession.AttemptStatus)
                    {
                        case AttemptStatus.Abandoned:
                            messageTitle = FramesetResources.HID_SessionAbandonedTitle;
                            message = FramesetResources.FRM_ExecuteViewAbandonedSessionMsg;
                            break;
                        case AttemptStatus.Completed:
                            messageTitle = FramesetResources.HID_SessionCompletedTitle;
                            message = FramesetResources.FRM_ExecuteViewCompletedSessionMsg;
                            break;
                        case AttemptStatus.Suspended:
                            messageTitle = FramesetResources.HID_SessionSuspendedTitle;
                            message = FramesetResources.FRM_ExecuteViewSuspendedSessionMsg;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the uri path to the frameset directory.
        /// </summary>
        private Uri m_framesetPath;
        public Uri FramesetPath
        {
            get{
                if (m_framesetPath == null)
                {
                    // Need to get the Frameset directory, so that include files work. 
                    // We have to do the following path magic in order to account for the fact that the 
                    // current URL is of arbitrary depth.
                    //Request.Url.OriginalString
                    string currentExecutionPath = Request.Url.OriginalString; //Request.CurrentExecutionFilePath;
                    // Find the part of the first instance of /Frameset/Hidden.aspx and then get a substring that is the 
                    // current url, up to the end of the /Frameset/.
                    string framesetPath = currentExecutionPath.Substring(0, currentExecutionPath.IndexOf("/Frameset/Hidden.aspx", StringComparison.OrdinalIgnoreCase) + 10);
                    m_framesetPath = new Uri(framesetPath, UriKind.Absolute);
                }
                return m_framesetPath;
            }
        }
    }
}