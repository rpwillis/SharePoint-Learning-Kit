/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Xml;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.DataModel;
using Microsoft.LearningComponents.Frameset;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Resources.Properties;
using Resources;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePointLearningKit.WebControls;


// This file contains the BWP-specific hidden frame rendering code. Most of the actual work is done in the code shared
// with SLK.

namespace Microsoft.SharePointLearningKit.Frameset
{
    /// <summary>
    /// Parameters to this page:
    ///     LearnerAssignmentId = current learner assignment id
    ///     View = requested view
    ///     I = (value is ignored). If present, the page is being display during initialization of the frameset.
    /// </summary>
    public class Hidden : FramesetPage
    {
        private HiddenHelper m_hiddenHelper;
        private bool m_sessionEnded;    // if true, the session ended during this page rendering

        /// <summary>The page load event.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // exceptions caught, added to event log
        protected void Page_Load(object sender, EventArgs e)
        {
           try
           {
               SlkUtilities.RetryOnDeadlock(delegate()
               {
                   //Initialize data that may need to be reset on retry
                   Response.Clear();
                   ClearError();

                   m_sessionEnded = false;
                   m_hiddenHelper = new HiddenHelper(Request, Response, SlkEmbeddedUIPath);
                   m_hiddenHelper.ProcessPageLoad(SlkStore.PackageStore, GetSessionTitle, TryGetSessionView, 
                                       TryGetAttemptId, AppendContentFrameDetails, RegisterError,
                                       GetErrorInfo, ProcessSessionEnd, ProcessViewRequest, GetMessage, IsPostBack);


                   // Send assignment information to client. If the session has ended, then force a reload of the current 
                   // assignment properties. Otherwise, the cached value will have required info so no need to re-query database.
                   LearnerAssignmentProperties la = GetLearnerAssignment(SessionEnded);

                   // Add assignment information to the hidden controls
                   HiddenControlInfo hiddenCtrlInfo = new HiddenControlInfo();
                   hiddenCtrlInfo.Id = new PlainTextString(HiddenFieldNames.LearnerAssignmentId);
                   hiddenCtrlInfo.Value = new PlainTextString(FramesetUtil.GetStringInvariant(la.LearnerAssignmentId.GetKey()));
                   hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString(ResHelper.Format("slkMgr.LearnerAssignmentId = document.getElementById({0}).value;",
                      JScriptString.QuoteString(HiddenFieldNames.LearnerAssignmentId, false)));

                   m_hiddenHelper.HiddenControls.Add(hiddenCtrlInfo);

                   // Learner assignment status ('not started', 'in progress', etc)
                   hiddenCtrlInfo = new HiddenControlInfo();
                   hiddenCtrlInfo.Id = new PlainTextString(HiddenFieldNames.LearnerAssignmentStatus);
                   hiddenCtrlInfo.Value = new PlainTextString(SlkUtilities.GetLearnerAssignmentState(la.Status));
                   hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString(ResHelper.Format("slkMgr.Status = document.getElementById({0}).value;",
                      JScriptString.QuoteString(HiddenFieldNames.LearnerAssignmentStatus, false)));

                   m_hiddenHelper.HiddenControls.Add(hiddenCtrlInfo);

                   hiddenCtrlInfo = new HiddenControlInfo();
                   if (la.FinalPoints != null)
                   {
                       // finalPoints is passed in invariant culture, as a float
                       hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString(ResHelper.Format("slkMgr.FinalPoints = {0};",
                                       Convert.ToString(la.FinalPoints, CultureInfo.InvariantCulture.NumberFormat)));
                   }
                   else
                   {
                       hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString("slkMgr.FinalPoints = null;");
                   }
                   m_hiddenHelper.HiddenControls.Add(hiddenCtrlInfo);

                   // Send information about total points (ie, computed points on the client). This is called 'graded score' in 
                   // grading page.
                   LearningSession session = m_hiddenHelper.Session;
                   if (session != null)
                   {
                       hiddenCtrlInfo = new HiddenControlInfo();
                       if (session.TotalPoints != null)
                       {
                           // TotalPoints is passed in current culture, as a string
                           JScriptString totalPointsValue = JScriptString.QuoteString(Convert.ToString(session.TotalPoints, CultureInfo.CurrentCulture.NumberFormat), false);
                           hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString(ResHelper.Format("slkMgr.ComputedPoints = {0};", totalPointsValue));
                       }
                       else
                       {
                           hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString("slkMgr.ComputedPoints = \"\";");
                       }
                       m_hiddenHelper.HiddenControls.Add(hiddenCtrlInfo);

                       if (session.SuccessStatus != SuccessStatus.Unknown)
                       {
                           hiddenCtrlInfo = new HiddenControlInfo();
                           hiddenCtrlInfo.FrameManagerInitializationScript = new JScriptString(ResHelper.Format("slkMgr.PassFail = {0};\r\n",
                               JScriptString.QuoteString(((session.SuccessStatus == SuccessStatus.Passed) ? "passed" : "failed"), false)));

                           m_hiddenHelper.HiddenControls.Add(hiddenCtrlInfo);
                       }
                   }
               });
            }
            catch (Exception ex)
            {
                ClearError();

                // Unexpected exceptions are not shown to user
                SlkError.WriteToEventLog(FramesetResources.FRM_UnknownExceptionMsg, ex.ToString());
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedExceptionMsg), false);
            }        
        }

        
        /// <summary>
        /// Returns true if the current assignment view is InstructorReview.
        /// </summary>
        private bool IsInstructorReview
        {
            get { 
                return (AssignmentView == AssignmentView.InstructorReview);
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
        /// Gets the title to display for the session.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public PlainTextString GetSessionTitle(LearningSession session)
        {
            FramesetUtil.ValidateNonNullParameter("session", session);

            LearnerAssignmentProperties la = GetLearnerAssignment();
            switch (session.View)
            {
                case SessionView.Execute:
                    return new PlainTextString(la.Assignment.Title);
                
                case SessionView.RandomAccess:
                    return new PlainTextString(ResHelper.Format("{0}: {1}", la.LearnerName, la.Assignment.Title));

                case SessionView.Review:
                    {
                        if (IsInstructorReview)
                        {
                            return new PlainTextString(ResHelper.Format("{0}: {1}", la.LearnerName, la.Assignment.Title));
                        }
                        else
                        {
                            return new PlainTextString(la.Assignment.Title);
                        }
                    }
                default:
                    return new PlainTextString(la.Assignment.Title);

            }
        }

        /// <summary>
        /// Gets and sets whether the current session has ended.
        /// </summary>
        private bool SessionEnded { 
            get { return m_sessionEnded; } 
            set { m_sessionEnded = value; } 
        }

        /// <summary>
        /// Called from the common code to allow processing a session that had ended. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference"),
        SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        public void ProcessSessionEnd(LearningSession session, ref string messageTitle, ref string message)
        {
            FramesetUtil.ValidateNonNullParameter("session", session);

            // If we have already been here, then there is nothing more to do.
            if (SessionEnded)
                return;

            LearnerAssignmentProperties la = GetLearnerAssignment();
           
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
                            messageTitle = SlkFrameset.HID_SessionAbandonedTitle;
                            message = SlkFrameset.HID_ExecuteViewAbandonedSessionMsg;
                            SessionEnded = true;
                            break;
                        case AttemptStatus.Completed:
                            messageTitle = SlkFrameset.HID_SessionCompletedTitle;
                            message = SlkFrameset.HID_ExecuteViewCompletedSessionMsg;
                            SessionEnded = true;
                            break;
                        case AttemptStatus.Suspended:
                            messageTitle = SlkFrameset.HID_SessionSuspendedTitle;
                            message = SlkFrameset.HID_ExecuteViewSuspendedSessionMsg;
                            // Do not set SessionEnded -- the session being suspended does not warrant ending the learner assignment
                            break;
                    }
                }

                if (SessionEnded)
                {
                    // Call FinishLearnerAssignment since the attempt has already been completed.
                    SlkStore.FinishLearnerAssignment(LearnerAssignmentGuidId);
                }
            }
            else if (session.View == SessionView.RandomAccess)
            {
                messageTitle = SlkFrameset.HID_GradingFinishedTitle;
                message = SlkFrameset.HID_GradingFinishedMessage;
                StringBuilder sb = new StringBuilder(1000);
                sb.Append(message);
                sb.Append("<br><script>");

                // Write the assignment status to slkFrameMgr
                WriteSlkMgrInit(sb);

                sb.AppendLine("slkMgr = Slk_GetSlkManager();");
                sb.AppendFormat("slkMgr.LearnerAssignmentId = {0};\r\n",
                    JScriptString.QuoteString(FramesetUtil.GetStringInvariant(la.LearnerAssignmentId.GetKey()), false));

                sb.AppendFormat("slkMgr.Status = {0};\r\n",
                    JScriptString.QuoteString(SlkUtilities.GetLearnerAssignmentState(la.Status), false));

                if (AssignmentView == AssignmentView.Grading)
                {
                    string finalPointsValue = "null";
                    float? finalPoints = la.FinalPoints;
                    if (finalPoints != null)
                    {
                        finalPointsValue = Convert.ToString(finalPoints.Value, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    sb.AppendFormat("slkMgr.FinalPoints = {0};\r\n", finalPointsValue);
                }

                // Send information about total points (ie, computed points on the client). 
                if (session != null)
                {
                    if (session.TotalPoints != null)
                    {
                        sb.AppendFormat("slkMgr.ComputedPoints = {0};\r\n",
                            JScriptString.QuoteString(Convert.ToString(session.TotalPoints, CultureInfo.CurrentCulture.NumberFormat), false));
                    }
                    else
                    {
                        sb.AppendFormat("slkMgr.ComputedPoints = \"\";\r\n");
                    }

                    if (session.SuccessStatus != SuccessStatus.Unknown)
                    {
                        sb.AppendFormat("slkMgr.PassFail = {0};\r\n",
                            JScriptString.QuoteString(((session.SuccessStatus == SuccessStatus.Passed) ? "passed" : "failed"), false));
                    }
                }
            }
        }

        private Uri SlkEmbeddedUIPath
        {
            get
            {
                return new Uri(Request.Url, "/_layouts/SharePointLearningKit/Frameset/");
            }
        }
    }

    // Hidden field names that are unique to SLK.
#pragma warning disable 1591
    public partial class HiddenFieldNames
    {
        public const string LearnerAssignmentId = "hidLAId";
        public const string LearnerAssignmentStatus = "hidLAStatus";
        public const string LearnerAssignmentPassFail = "hidLAPassFail";
        public const string LearnerAssignmentFinalPoints = "hidLAFinalPoints";
        public const string LearnerAssignmentContentPoints = "hidContentPoints";
    }
#pragma warning restore 1591
}
