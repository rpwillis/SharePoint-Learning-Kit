/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.LearningComponents;
using System.Globalization;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Microsoft.LearningComponents.Frameset;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.SharePointLearningKit.Frameset
{

    /// <summary>
    /// The MainFrames page. Contains dynamic references to HiddenFrame and TocFrame.
    /// </summary>
    /// <remarks>
    /// UrlParameters to this page:
    ///     View
    ///     LearnerAssignmentId
    ///     AttemptId
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")] // MainFrames is fine
    public class MainFrames : FramesetPage
    {
        AssignmentView m_view;
        AttemptItemIdentifier m_attemptId;

        protected void Page_Load(object sender, EventArgs e)
        {
            // On page load, just get the required parameters. Errors are not registered, as this frame doesn't show errors.

            // Compiler doesn't allow passing property as out parameter, so get the value then assign it to the property.
            LearnerAssignmentItemIdentifier learnerAssignmentId;
            if (!TryProcessLearnerAssignmentIdParameter(false, out learnerAssignmentId))
                return;
            LearnerAssignmentId = learnerAssignmentId;

            if (!TryProcessAssignmentViewParameter(false, out m_view))
                return;

            if (!ProcessAttemptIdParameter(false, out m_attemptId))
                return;
        }

        #region called from aspx
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string HiddenFrameUrl
        {
            get
            {
                string strUrl = String.Format(CultureInfo.CurrentCulture, "Hidden.aspx?{0}={1}&{2}={3}&{4}=1",
                    FramesetQueryParameter.SlkView, FramesetQueryParameter.GetValueAsParameter(m_view),
                    FramesetQueryParameter.LearnerAssignmentId, FramesetQueryParameter.GetValueAsParameter(LearnerAssignmentId),
                    FramesetQueryParameter.Init);
                UrlString hiddenUrl = new UrlString(strUrl);
                return hiddenUrl.ToAscii();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string TocFrameUrl
        {
            get
            {
                string strUrl = String.Format(CultureInfo.CurrentCulture,
                                "Toc.aspx?{0}={1}&{2}={3}",
                                FramesetQueryParameter.SlkView, FramesetQueryParameter.GetValueAsParameter(m_view),
                                FramesetQueryParameter.LearnerAssignmentId, FramesetQueryParameter.GetValueAsParameter(LearnerAssignmentId)) ;
                UrlString hiddenUrl = new UrlString(strUrl);
                return hiddenUrl.ToAscii();
            }
        }
        #endregion

    }
}