/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.Frameset;
using Resources.Properties;
using Resources;
using System.Diagnostics.CodeAnalysis;


namespace Microsoft.SharePointLearningKit.Frameset
{
    [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased")]
    public class TOC : FramesetPage
    {
        TocHelper m_tocHelper;

        // suppress message as this is the top-level page displayed.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                SlkUtilities.RetryOnDeadlock(delegate()
                {
                    // Clear data that may need to be reset on retry
                    Response.Clear();
                    ClearError();

                    m_tocHelper = new TocHelper();
                    string submitText = "";
                    SessionView view;
                    if (TryGetSessionView(false, out view))
                    {
                        if (view == SessionView.Execute)
                            submitText = SlkFrameset.TOC_SubmitAssignment;
                        else
                            submitText = SlkFrameset.TOC_SubmitGrading;

                    }
                    m_tocHelper.ProcessPageLoad(Response, SlkStore.PackageStore, TryGetSessionView,
                                                TryGetAttemptId, ProcessViewRequest, RegisterError,
                                                submitText);
                });
            } 
            catch (Exception ex)
            {
                // Unexpected exceptions are not shown to user
                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, FramesetResources.FRM_UnknownExceptionMsg, ex.ToString());
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedExceptionMsg), false);
            }       

        }

        #region called from aspx
        /// <summary>
        /// Write the html for the table of contents to the response.
        /// </summary>
        /// <returns></returns>
        public void WriteToc()
        {
            SlkUtilities.RetryOnDeadlock(delegate()
            {
                // This accesses the database to get the list of nodes
                m_tocHelper.TocElementsHtml(Request, AssignmentView.Grading.ToString());
            });
        }

        /// <summary>
        /// Return the version of the frameset files. This is used to compare to the version of the js file, to ensure the js
        /// file is not being cached from a previous version.
        /// </summary>
        public static string TocVersion
        {
            get
            {
                return TocHelper.TocVersion;
            }
        }

        /// <summary>
        /// Return the message to display if the js version doesn't match the aspx version.
        /// </summary>
        public static string InvalidVersionHtml
        {
            get
            {
                return TocHelper.InvalidVersionHtml;
            }
        }

        #endregion  // called from aspx
    }
}
