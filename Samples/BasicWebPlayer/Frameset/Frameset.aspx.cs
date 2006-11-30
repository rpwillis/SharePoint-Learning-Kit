/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using System.Globalization;
using System.Text;

using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Resources;
using System.IO;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// This is the top-level frameset for display views of a package. 
    /// The URL to this page differs based on the view requested.
    /// Query parameters are:
    /// View: The integer that corresponds to the SessionView value for the session.
    /// AttemptId: The attempt to be displayed. This is required if the view is based on an attempt. It must already
    ///     exist in LearningStore. It must correspond to an attempt in a state that is appropriate for the view. For instance,
    ///     if View=0 (Execute), the attempt must be active.
    /// </summary>
    public partial class Frameset_Frameset : BwpFramesetPage
    {
        private FramesetHelper m_framesetHelper; 

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                m_framesetHelper = new FramesetHelper();
                m_framesetHelper.ProcessPageLoad(PStore, LoggingOptions.LogAll, ProcessViewParameter, ProcessAttemptIdParameter, ProcessViewRequest);
            }
            catch (FileNotFoundException)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = "Not Found";
            }
            catch (Exception)
            {
                // Doesn't matter why.
                Response.StatusCode = 500;
                Response.StatusDescription = "Internal Server Error";
                RegisterError(FramesetResources.FRM_NotAvailableTitleHtml, FramesetResources.FRM_NotAvailableHtml, false);
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

        #region Called From Aspx    // the following methods are called from in-place aspx code

        /// <summary>
        /// Gets the URL to the page loaded into the MainFrames frame.
        /// </summary>
        public string MainFramesUrl
        {
            get
            {
                StringBuilder frames = new StringBuilder(4096);
                int view = Convert.ToInt32(m_framesetHelper.View);
                frames.Append(String.Format(CultureInfo.CurrentUICulture, "MainFrames.aspx?{0}={1}&",
                                                FramesetQueryParameter.View, view.ToString()));
                frames.Append(String.Format(CultureInfo.CurrentUICulture, "{0}={1}",
                                                FramesetQueryParameter.AttemptId, m_framesetHelper.AttemptId.GetKey()));
                return new UrlString(frames.ToString()).ToAscii();
            }
        }

        /// <summary>
        /// Gets the title for the frameset. The one that goes into the title bar of the browser window.
        /// </summary>
        public string PageTitleHtml
        {
            get
            {
                PlainTextString text = new PlainTextString(ResHelper.GetMessage(FramesetResources.FRM_Title));
                HtmlString html = new HtmlString(text);
                return html.ToString();
            }
        }

        /// <summary>
        /// Gets the version of SCORM used in the current attempt.
        /// </summary>
        public string ScormVersionHtml
        {
            get { return m_framesetHelper.ScormVersionHtml; }
        }

        /// <summary>
        /// Returns "true" if the Rte is required on the first activity. "false" otherwise. (No quotes in the string.)
        /// </summary>
        public string RteRequired
        {
            get { return m_framesetHelper.RteRequired; }
        }

        #endregion  // called from aspx
    }
}