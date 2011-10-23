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
using Microsoft.LearningComponents;
using System.Text;
using System.Globalization;
using Resources;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Delegate to retrieve information about the requested View. This method should assume View is required. 
    /// If it does not 
    /// exist or is not a valid value and showErrorPage=true, the error handling should be done within the delegate
    /// and false returned. 
    /// If false is returned, the caller will ignore the value of <paramref name="view"/>.    
    /// </summary>
    /// <param name="showErrorPage">If true, an error page is registered on error.</param>
    /// <param name="view">The view to evaluate.</param>
    public delegate bool TryGetViewInfo(bool showErrorPage, out SessionView view);

    /// <summary>
    /// Delegate to retrieve the AttemptId for the session. This method should assume the attemptId is required. If 
    /// it does not exist or is not numeric, the error handling should be done within the delegate. 
    /// This method does not check LearningStore to determine if returned attemptId is valid.
    /// </summary>
    /// <param name="showErrorPage">If true, the error page is shown if the value is not available.</param>
    /// <param name="attemptId">The attempt id.</param>
    /// <returns>If false, the value did not exist or was not valid. The application will not continue with 
    /// page processing.</returns>
    public delegate bool TryGetAttemptInfo(bool showErrorPage, out AttemptItemIdentifier attemptId);

    /// <summary>
    /// Delegate to have the application process a request for a view of a session.
    /// Return value of 'false' indicates the application does not allow access to the view.
    /// </summary>
    /// <param name="view">The requested view.</param>
    /// <param name="session">The requested session.</param>
    public delegate bool ProcessViewRequest(SessionView view, LearningSession session);

    /// <summary>
    /// Delegate to allow the application to provide app-specific strings for some error conditions.
    /// </summary>
    /// <param name="msgId">The application-neutral id for the string.</param>
    /// <returns>The string the application determines to display for the specific case.</returns>
    public delegate string GetFramesetMsg(FramesetStringId msgId);

    /// <summary>
    /// Helper that is used to assist in rendering the Frameset.aspx page of the frameset.
    /// This code is shared in Basic Web Player and SLK framesets.
    /// </summary>
    public class FramesetHelper
    {
        private LearningSession m_session;

        /// <summary>
        /// Process the PageLoad event on the Frameset.aspx page. 
        /// </summary>
        /// <param name="packageStore">The package store containing the packages for the sessions in the frameset.</param>
        /// <param name="TryGetViewInfo">Delegate to retrieve the requested view.</param>
        /// <param name="TryGetAttemptInfo">Delegate to retrieve the requested attempt.</param>
        /// <param name="ProcessViewRequest">Delegate to process a request for a specific view.</param>
        /// <remarks>
        /// This method will never throw an exception.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]   // parameters called as methods are cased as methods
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]   // parameters are validated
        public void ProcessPageLoad(PackageStore packageStore,
                                    TryGetViewInfo TryGetViewInfo,
                                    TryGetAttemptInfo TryGetAttemptInfo,
                                    ProcessViewRequest ProcessViewRequest)
        {
            // These should never be a problem, however fxcop complains about them.
            FramesetUtil.ValidateNonNullParameter("TryGetViewInfo", TryGetViewInfo);
            FramesetUtil.ValidateNonNullParameter("TryGetAttemptInfo", TryGetAttemptInfo);
            FramesetUtil.ValidateNonNullParameter("ProcessViewRequest", ProcessViewRequest);
            FramesetUtil.ValidateNonNullParameter("packageStore", packageStore);

            // Session information that may be required
            SessionView view;
            AttemptItemIdentifier attemptId; // not required for all views

            // Get View information. It determines what else to look for.
            if (!TryGetViewInfo(true, out view))
                return;

            // Based on View, request other information
            switch (view)
            {
                case SessionView.Execute:
                    {
                        // AttemptId is required
                        if (!TryGetAttemptInfo(true, out attemptId))
                            return;

                        // Create the session
                        m_session = new StoredLearningSession(view, attemptId, packageStore);
                        
                        StoredLearningSession slsSession = m_session as StoredLearningSession;

                        if (!ProcessViewRequest(SessionView.Execute, slsSession))
                            return;

                        // If the attempt id appeared valid (that is, it was numeric), but does not represent a valid 
                        // attempt, the call to access AttemptStatus on the session will trigger an InvalidOperationException
                        // containing a message for the user that the attempt id was not valid.
                        switch (slsSession.AttemptStatus)
                        {
                            case AttemptStatus.Abandoned:
                                {
                                    // Can't do execute view on abandoned sessions. The application should have handled this
                                    // in the ProcessViewRequest.
                                    return;
                                }
                            case AttemptStatus.Active:
                                {
                                    // Check if it's started. If not, try starting it and forcing selection of a current activity.
                                    if (!slsSession.HasCurrentActivity)
                                    {
                                        try
                                        {
                                            slsSession.Start(false);
                                            slsSession.CommitChanges();
                                        }
                                        catch (SequencingException)
                                        {
                                            // Intentionally ignored. This means it was either already started or could not 
                                            // select an activity. In either case, just let the content frame ask the user to 
                                            // deal with selecting an activity.
                                        }
                                    }
                                    else
                                    {
                                        // If the current activity is not active, then it's possible the frameset was removed from the 
                                        // user and the content suspended the current activity. In that case, we do this little trick
                                        // and try suspending all the activities and then resuming them. The resume will simply resume
                                        // all the activities between the current activity and the root. Other suspended activities
                                        // will not be affected.
                                        if (!slsSession.CurrentActivityIsActive)
                                        {
                                            slsSession.Suspend();
                                            slsSession.Resume();
                                            slsSession.CommitChanges();
                                        }
                                    }
                                }
                                break;
                            case AttemptStatus.Completed:
                                {
                                    // Can't do execute view on completed sessions. The application should have handled this in the 
                                    // ProcessViewRequest.
                                    return;
                                }
                            case AttemptStatus.Suspended:
                                {
                                    // Resume it
                                    slsSession.Resume();
                                    slsSession.CommitChanges();
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    break;
                case SessionView.RandomAccess:
                    {
                        // AttemptId is required
                        if (!TryGetAttemptInfo(true, out attemptId))
                            return;

                        StoredLearningSession slsSession = new StoredLearningSession(SessionView.RandomAccess, attemptId, packageStore);
                        
                        m_session = slsSession;

                        if (!ProcessViewRequest(SessionView.RandomAccess, slsSession ))
                            return;

                        // Move to the first activity with a resource.
                        PostableFrameHelper.MoveToNextActivity(m_session);
                    }
                    break;
                case SessionView.Review:
                    {
                        // AttemptId is required
                        if (!TryGetAttemptInfo(true, out attemptId))
                            return;

                        // Create the session
                        StoredLearningSession slsSession = new StoredLearningSession(view, attemptId, packageStore);
                        m_session = slsSession;

                        if (!ProcessViewRequest(SessionView.Review, m_session))
                            return;

                        // Access a property. If the user doesn't have permission, this will throw exception
                        if (m_session.HasCurrentActivity)
                        {
                            // This is good. The 'if' statement is here to make compiler happy.
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Return the view of the current session
        /// </summary>
        public SessionView View
        {
            get { return m_session.View; }
        }

        /// <summary>
        /// Gets the attempt id associated with this session. 
        /// </summary>
        public AttemptItemIdentifier AttemptId
        {
            get
            {
                return ((StoredLearningSession)m_session).AttemptId;
            }
        }

        /// <summary>
        /// Gets the scorm version format used in the current session.
        /// </summary>
        public string ScormVersionHtml
        {
            get
            {
                if (m_session == null)
                    return "&nbsp;";

                // If the package is LRM, then if it has SCOs, they are 1.2 format
                PackageFormat scoFormat = m_session.PackageFormat;
                if (scoFormat == PackageFormat.Lrm)
                    scoFormat = PackageFormat.V1p2;

                PlainTextString textVersion = new PlainTextString(scoFormat.ToString());
                return new HtmlString(textVersion).ToString();
            }
        }

        /// <summary>
        /// Returns "true" if the Rte is required on the first activity. "false" otherwise. (No quotes in the string.)
        /// </summary>
        public string RteRequired
        {
            get
            {
                if (m_session == null)
                    return "false";

                if (!m_session.HasCurrentActivity)
                    return "false";

                if (m_session.CurrentActivityResourceType == ResourceType.Sco)
                    return "true";

                return "false";
            }
        }
    }


#pragma warning disable 1591
    // Ids that uniquely identify strings to be displayed in the frameset. The specific frameset
    // (SLK or BWP) will map these ids to strings that are different between the versions of the frameset.
    public enum FramesetStringId
    {
        MoveToActivityFailedHtml,
        MoveToNextFailedHtml,
        MoveToPreviousFailedHtml,
        SubmitPageTitleHtml,
        SubmitPageMessageHtml,
        SubmitPageMessageNoCurrentActivityHtml,
        SubmitPageSaveButtonHtml,
        CannotDisplayContentTitle,
        SessionIsNotActiveMsg,
        ActivityIsNotActiveMsg,
        ParameterRequiredTitleHtml,
        ParameterRequiredMsgHtml,
        SelectActivityMessageHtml,
        SelectActivityTitleHtml
    }
#pragma warning restore 1591
}
