/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Globalization;
using System.Threading;
using System.Xml.XPath;
using System.Xml;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.IO;
using System.Web;
using System.Web.UI;
using Microsoft.LearningComponents.Frameset;
using Resources;
using System.Web.Hosting;
using System.Collections.Generic;
using System.Text;
using Resources.Properties;
using Microsoft.SharePoint;
using System.Diagnostics.CodeAnalysis;

// <summary>
// Views supported in this page: Student/InstructorReview, Execute, Grading
// 
// Displays the content frame. The URL to this page is of the form:
// For attempt-based sessions:
// Content.aspx?PF="post-file path information"
// 
// The post-file-path information is parsed as follows:
// For attempt-based sessions:
//     /view/attemptId/view-specific-information/path-to-resource
//     where view-specific-information is:
//         if (view = Execute)
//             view-specific-information is not provided
//     where path-to-resource is optional. If not provided, the default resource of the current activity will be used.
// 
// </summary>
namespace Microsoft.SharePointLearningKit.Frameset
{
    public class Content : FramesetPage
    {
        ContentHelper m_contentHelper;

        // The path to the requested content. This may be part of the url (if we are running without  
        // an http module) or a URL parameter (with http module). We run in both modes because 
        // VS.NET does not parse URLs of the form: /.../Content.aspx/0/1/foo.gif correctly without 
        // the assistance of the module.
        string m_contentPath;

        // pfParts[0] = "", pfParts[1] = assignment view, pfParts[2] = extra view data, everything else is path or extra data for attempt views
        string[] m_contentPathParts;

        SessionView m_sessionView;

        // The Session.TotalPoints value prior to processing any posted data.
        float? m_initialTotalPoints;

        LearnerAssignmentProperties m_learnerAssignment;    // cached version of learner assignment to display

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // all exceptions caught and written to event log rather than getting aspx error page
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {  
                bool isPosted = false;
                if (String.CompareOrdinal(Request.HttpMethod, "POST") == 0)
                    isPosted = true;

                // Get the path to the content. This may be part of the url (if we are running without  
                // an http module) or a URL parameter (with http module). We run in both modes because 
                // VS.NET does not parse URLs of the form: /.../Content.aspx/0/1/foo.gif correctly without 
                // the assistance of the module.
                m_contentPath = GetContentPath();
                
                SPSecurity.CatchAccessDeniedException = true;              

                SlkUtilities.RetryOnDeadlock(delegate()
                {
                    // Initialize data that may get set on a first try, but must be reset for retry
                    Response.Clear();
                    ClearError();
                    m_learnerAssignment = null;

                    m_contentHelper = new ContentHelper(Request, Response, SlkEmbeddedUIPath);
                    m_contentHelper.ProcessPageLoad(SlkStore.PackageStore,
                                                        String.IsNullOrEmpty(m_contentPath),
                                                        isPosted,
                                                        SlkStore.Settings.LoggingOptions,
                                                        TryGetViewInfo,
                                                        TryGetAttemptInfo,
                                                        TryGetActivityInfo,
                                                        GetResourcePath,
                                                        AppendContentFrameDetails,
                                                        UpdateRenderContext,
                                                        ProcessPostedData,
                                                        ProcessPostedDataComplete,
                                                        RegisterError,
                                                        GetErrorInfo,
                                                        GetMessage);
                });
            
            }
            catch (ThreadAbortException)
            {
                // response ended. Do nothing.
                return;
            }
            catch (UnauthorizedAccessException uae)
            {
                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, FramesetResources.FRM_UnknownExceptionMsg, uae.ToString());
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedExceptionMsg), false);

                // Clear the response in case something has been written
                Response.Clear();
            }
            catch (Exception e2)
            {
                // Unexpected exceptions are not shown to user
                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, FramesetResources.FRM_UnknownExceptionMsg, e2.ToString());
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedExceptionMsg), false);

                // Clear the response in case something has been written
                Response.Clear();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // need to catch it to provide correct SLK message
        public bool TryGetViewInfo(bool showErrorPage, out SessionView view)
        {
            // make compiler happy
            view = SessionView.Execute;
            AssignmentView assignmentView;

            // Get the parts of the content path (parameter PF) value
            // pfParts[0] = "", pfParts[1] = assignment view, pfParts[2] = extra view data, everything else is path or extra data for attempt views
            m_contentPathParts = m_contentPath.Split(new char[] { '/' });

            // First section is assignment view -- it must exist in all cases.
            string strAssignmentView = m_contentPathParts[1];
            if (String.IsNullOrEmpty(strAssignmentView))
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredTitle, FramesetQueryParameter.SlkView),
                    ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredMsg, FramesetQueryParameter.SlkView), false);
                }
                return false;
            }

            try
            {
                assignmentView = (AssignmentView)Enum.Parse(typeof(AssignmentView), strAssignmentView, true);
                if ((assignmentView < AssignmentView.Execute) || (assignmentView > AssignmentView.Grading))
                {
                    if (showErrorPage)
                    {
                        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.SlkView),
                                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.SlkView, strAssignmentView), false);
                    }
                    return false;
                }
            }
            catch
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.SlkView),
                                    ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.SlkView, strAssignmentView), false);
                }
                return false;
            }

            view = GetSessionView(assignmentView);
            m_sessionView = view;
            AssignmentView = assignmentView;

            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // catch general exception so that message can be SLK-specific
        public bool TryGetAttemptInfo(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            // Initialize out parameter
            attemptId = null;

            long learnerAssignmentKey;
            bool isValid = false;

            // For views based on an attempt, LearnerAssignmentId is required
            // It must be a positive long value. 
            if (m_contentPathParts.Length >= 3)
            {
                if (long.TryParse(m_contentPathParts[2], out learnerAssignmentKey))
                {
                    if (learnerAssignmentKey > 0)
                    {
                        LearnerAssignmentId = new LearnerAssignmentItemIdentifier(learnerAssignmentKey);
                    }
                }
            }

            try
            {
                m_learnerAssignment = GetLearnerAssignment();
                attemptId = m_learnerAssignment.AttemptId;
                isValid = true;
            }
            catch
            {
                // If we could not get the assignment, send the proper error codes
                Response.StatusCode = 404;
                Response.StatusDescription = "Not Found";
                return false;
            }

            if (!isValid)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.CON_ContentCannotBeDisplayedTitle),
                            ResHelper.GetMessage(FramesetResources.CON_ContentCannotBeDisplayedMsg), false);
                }
                return false;
            }
            return true;
        }

        public bool TryGetActivityInfo(bool showErrorPage, out long activityId)
        {
            // This will only be called in Review & RandomAccess views, in which case, m_contentPathParts is...

            // m_contentPathParts[1] = assignment view, m_contentPathParts[2] = learnerAssignmentId, m_contentPathParts[3] = activityId to display, m_contentPathParts[4] and beyond is resource path

            // activity id must be provided
            activityId = -1;
            bool isValid = false;
            if (m_contentPathParts.Length >= 4)
            {
                string strActivityId = m_contentPathParts[3];
                if (long.TryParse(strActivityId, out activityId))
                {
                    if (activityId > 0)
                        isValid = true;
                }
            }
            if (!isValid)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.CON_ContentCannotBeDisplayedTitle),
                            ResHelper.GetMessage(FramesetResources.CON_ContentCannotBeDisplayedMsg), false);
                }
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // It does too much work
        public string GetResourcePath()
        {
            if ((m_sessionView == SessionView.Review) || (m_sessionView == SessionView.RandomAccess))
            {
                return m_contentPath.Substring(m_contentPathParts[1].Length + m_contentPathParts[2].Length + m_contentPathParts[3].Length + 4);
            }
            else if (m_sessionView == SessionView.Execute)
            {
                // Get the relative path to the resource. This may not exist, and that's ok. If it exists, its it the portion
                // of param that follows the assign view and learner assignment id. The 3 below is the forward slashes.
                return m_contentPath.Substring(m_contentPathParts[1].Length + m_contentPathParts[2].Length + 3);
            }
            return string.Empty;
        }

        /// <summary>
        /// Delegate to allow the application to update the render context. The application does not 
        /// need to set RelativePath and OutputStream.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public void UpdateRenderContext(RenderContext context, StringBuilder scriptBlock, LearningSession session)
        {
            // Set values other than OutputStream and RelativePath
            context.EmbeddedUIResourcePath = new Uri(SlkEmbeddedUIPath, "Images/");

            SetMimeTypeMapping(context.MimeTypeMapping);

            SetIisCompatibilityModeExtensions(context.IisCompatibilityModeExtensions);

            // If this is not the primary resource, nothing else matters.
            if (scriptBlock == null)
                return;
            
            LearnerAssignmentProperties la = GetLearnerAssignment();
            switch (AssignmentView)
            {
                case AssignmentView.Execute:
                    {
                        context.ShowCorrectAnswers = false;
                        context.ShowReviewerInformation = false;
                    }
                    break;
                case AssignmentView.StudentReview:
                    {
                        context.ShowCorrectAnswers = la.ShowAnswersToLearners;
                        context.ShowReviewerInformation = false;
                    }
                    break;
                case AssignmentView.InstructorReview:
                    {
                        context.ShowCorrectAnswers = true;
                        context.ShowReviewerInformation = true;
                    }
                    break;
                case AssignmentView.Grading:
                    {
                        context.ShowCorrectAnswers = true;
                        context.ShowReviewerInformation = true;
                    }
                    break;
            }

            // Update hidden controls and script to include assignment information if there is script 
            // information to be written. Only write script in LRM content.
            if ((scriptBlock != null) && (session.CurrentActivityResourceType == ResourceType.Lrm))
            {
                WriteSlkMgrInit(scriptBlock);

                scriptBlock.AppendLine("slkMgr = Slk_GetSlkManager();");
                context.FormHiddenControls.Add(HiddenFieldNames.LearnerAssignmentId, FramesetUtil.GetStringInvariant(la.LearnerAssignmentId.GetKey()));
                scriptBlock.AppendFormat("slkMgr.LearnerAssignmentId = document.all[{0}].value;\r\n",
                    JScriptString.QuoteString(HiddenFieldNames.LearnerAssignmentId, false));

                context.FormHiddenControls.Add(HiddenFieldNames.LearnerAssignmentStatus, SlkUtilities.GetLearnerAssignmentState(la.Status));
                scriptBlock.AppendFormat("slkMgr.Status = document.all[{0}].value;\r\n",
                    JScriptString.QuoteString(HiddenFieldNames.LearnerAssignmentStatus, false));

                // Set the change in final points. This can only happen in grading.
                if (AssignmentView == AssignmentView.Grading)
                {
                    string finalPointsValue = "null";
                    float? finalPoints = la.FinalPoints;
                    if (finalPoints != null)
                    {
                        // FinalPoints is invariant culture
                        finalPointsValue = Convert.ToString(finalPoints.Value, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    scriptBlock.AppendFormat("slkMgr.FinalPoints = {0};\r\n",finalPointsValue);
                }
                
                // Send information about total points (ie, computed points on the client). 
                if (session != null) 
                {
                    if (session.TotalPoints != null)
                    {
                        // TotalPoints is passed in current culture, as a string
                        JScriptString totalPointsValue = JScriptString.QuoteString(Convert.ToString(session.TotalPoints, CultureInfo.CurrentCulture.NumberFormat), false);
                        scriptBlock.AppendFormat("slkMgr.ComputedPoints = {0};\r\n", totalPointsValue);
                    }
                    else
                    {
                        scriptBlock.Append("slkMgr.ComputedPoints = \"\";\r\n");
                    }
                    

                    if (session.SuccessStatus != SuccessStatus.Unknown)
                    {
                        scriptBlock.AppendFormat("slkMgr.PassFail = {0};\r\n",
                            JScriptString.QuoteString(((session.SuccessStatus == SuccessStatus.Passed) ? "passed" : "failed"), false));
                    }
                }
            }
        }

        /// <summary>
        /// Process the posted data before it is saved by the shared code. This allows checking the file attachments
        /// and making sure they meet the requirements of the SlkSettings.
        /// </summary>
        public bool ProcessPostedData(LearningSession session, HttpRequest request, Dictionary<string, HttpPostedFile> files)
        {
            // Save initial value of FinalPoints
            m_initialTotalPoints = session.TotalPoints;

            // Check the files for validity and fill in the output collection
            HttpFileCollection attachedFiles = Request.Files;
            int numFilesAttached = attachedFiles.Count;
            if (numFilesAttached > 0)
            {
                // Check if posted data meets requirements in SlkSettings. Additionally, check if there are files 
                // that refer to files that do not exist (i.e. have 0 length).

                int maxKb = SlkStore.Settings.MaxAttachmentKilobytes;
                ICollection<string> attachmentTypes = SlkStore.Settings.ApprovedAttachmentTypes;
                List<string> invalidFileAttachments = new List<string>(numFilesAttached);
                List<string> invalidFileSize = new List<string>(numFilesAttached);
                List<string> filesDontExist = new List<string>(numFilesAttached);

                // Keep track of whether there is at least
                bool hasInvalidFileAttachment = false;
                bool hasInvalidFileSize = false;
                bool fileExists = true;  

                // Go through posted files and test if they meet requirements
                foreach(string fileKey in attachedFiles)
                {
                    HttpPostedFile file = attachedFiles[fileKey];
                    bool fileIsValid = true;        // is this file a valid file attachment?

                    string filename = file.FileName;
                    // If the filename is empty, the file wasn't actually attached.
                    if (!String.IsNullOrEmpty(filename))
                    {
                        if (file.ContentLength == 0)
                        {
                            filesDontExist.Add(filename);
                            fileExists = false;
                            fileIsValid = false;
                        }
                        else if ((file.ContentLength / 1024) > maxKb)
                        {
                            invalidFileSize.Add(filename);
                            hasInvalidFileSize = true;
                            fileIsValid = false;
                        }

                        if (!attachmentTypes.Contains(Path.GetExtension(filename)))
                        {
                            invalidFileAttachments.Add(filename);
                            hasInvalidFileAttachment = true;
                            fileIsValid = false;
                        }
                    }
                    // else: The file was valid on a previous posting, so consider it valid here

                    if (fileIsValid)
                    {
                        // Add it to the returned list of valid files
                        files.Add(fileKey, file);
                    }
                }

                // if any of the posted files are invalid, then we need to write the message
                if (hasInvalidFileSize || hasInvalidFileAttachment || !fileExists)
                {
                    StringBuilder message = new StringBuilder(1000);
                    if (hasInvalidFileAttachment)
                    {
                        message.Append(ResHelper.GetMessage(SlkFrameset.CON_InvalidFileExtensionMsgHtml));
                        message.Append("<br><br><ul>");
                        foreach (string filename in invalidFileAttachments)
                        {
                            message.AppendFormat("<li>{0}</li>", SlkUtilities.GetHtmlEncodedText(filename));

                        }
                        message.Append("</ul>");
                    }

                    if (hasInvalidFileSize)
                    {
                        message.AppendFormat(ResHelper.GetMessage(SlkFrameset.CON_MaxFileSizeExceededMsgHtml, Convert.ToString(maxKb, CultureInfo.CurrentUICulture.NumberFormat)));
                        message.Append("<br><br><ul>");
                        foreach (string filename in invalidFileSize)
                        {
                            message.AppendFormat("<li>{0}</li>", SlkUtilities.GetHtmlEncodedText(filename));

                        }
                        message.Append("</ul>");
                    }

                    if (!fileExists)
                    {
                        message.AppendFormat(SlkFrameset.CON_FilesDontExistHtml);
                        message.Append("<br><br><ul>");
                        foreach (string filename in filesDontExist)
                        {
                            message.AppendFormat("<li>{0}</li>", SlkUtilities.GetHtmlEncodedText(filename));

                        }
                        message.Append("</ul>");
                    }

                    // If one of the cases that relates to SLK settings is true, then tell user that there are settings
                    // that affect the error
                    if (hasInvalidFileSize || hasInvalidFileAttachment)
                        message.AppendFormat(CultureInfo.CurrentCulture, "{0}<br><br>", ResHelper.GetMessage(SlkFrameset.CON_InvalidFileAttachmentMsgHtml));

                    message.Append(FramesetResources.CON_FileAttachmentErrorEndHtml);

                    // Add information for the 'Continue' link
                    JScriptString js = new JScriptString(ResHelper.FormatInvariant("API_GetFramesetManager().DoChoice(\"{0}\");",
                                        FramesetUtil.GetStringInvariant(session.CurrentActivityId)));
                    message.AppendFormat(CultureInfo.CurrentCulture, "<br><br><a href='{0}' >{1}</a>",
                                    js.ToJavascriptProtocol(), HttpUtility.HtmlEncode(FramesetResources.HID_ReloadCurrentContent));

                    RegisterError(SlkFrameset.CON_InvalidFileAttachmentTitleHtml, message.ToString(), false);
                }
            }
            return true;
        }

        /// <summary>
        /// Complete the processing of posted data.
        /// </summary>
        /// <param name="session"></param>
        public void ProcessPostedDataComplete(LearningSession session)
        {
            // If this is grading view, then update the FinalPoints in the assignment.
            if (AssignmentView == AssignmentView.Grading)
            {
                // Figure out the new FinalPoints value and update it if required.
                float? totalPoints = session.TotalPoints;

                if (m_initialTotalPoints == null)
                {
                    if (totalPoints != null)
                        SetFinalPoints(totalPoints);
                }
                else // m_initalPoints != null
                {
                    if (totalPoints == null)
                        SetFinalPoints(totalPoints);   // setting FinalPoints to null
                    else
                    {
                        // neither m_initialPoints nor totalPoints is null
                        float pointsDelta = totalPoints.Value - m_initialTotalPoints.Value;

                        // Update the FinalPoints value if it has changed
                        if (pointsDelta != 0)
                            IncrementFinalPoints(pointsDelta);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the final points and updates the in-memory LearnerAssignmentProperties.
        /// </summary>
        /// <param name="points"></param>
        private void SetFinalPoints(float? points)
        {
            SlkStore.SetFinalPoints(LearnerAssignmentId, points);

            // Force an update to the in-memory record of LearnerAssignment 
            GetLearnerAssignment(true);
        }

        /// <summary>
        /// Increments the final points in the assignment and updates the in-memory LearnerAssignmentProperties.
        /// </summary>
        /// <param name="delta"></param>
        private void IncrementFinalPoints(float delta)
        {
            SlkStore.IncrementFinalPoints(LearnerAssignmentId, delta);

            // Force an update to the in-memory record of LearnerAssignment 
            GetLearnerAssignment(true);
        }

        /// <summary>
        /// Set the mime mapping values into the <paramref name="mimeMapping"/> collection.
        /// </summary>
        public void SetMimeTypeMapping(IDictionary<string, string> mimeMapping)
        {
            IDictionary<string, string> slkMappings = SlkStore.Settings.MimeTypeMappings;

            foreach (KeyValuePair<string, string> kvPair in slkMappings)
            {
                mimeMapping.Add(kvPair.Key, kvPair.Value);
            } 
        }

        /// <summary>
        /// Set the IIS compatibility mode for the settings in the collection.
        /// </summary>
        public void SetIisCompatibilityModeExtensions(ICollection<string> fileExtensions)
        {
            ICollection<string> slkExtensions = SlkStore.Settings.ELearningIisCompatibilityModeExtensions;

            foreach (string ext in slkExtensions)
            {
                fileExtensions.Add(ext);
            }
        }

        /// <summary>
        /// Returns the path to the content (including view and attempt information) that is in the URL
        /// after the Content.aspx reference.
        /// </summary>
        /// <returns>The information about what content to display. It will return 
        /// null if there was no information. This indicates the frameset is initializing.</returns>
        private string GetContentPath()
        {
            string contentPath = null;

            // See if there is information after the Content.aspx string in the raw URL
            string rawUrl = Request.Url.AbsolutePath;
            int beginContext = rawUrl.IndexOf("Frameset/Content.aspx/", StringComparison.OrdinalIgnoreCase);
            if (beginContext > 0)
            {
                int endContent = beginContext + "Frameset/Content.aspx".Length;
                // It's a request with information following the page name.
                contentPath = rawUrl.Substring(endContent);
            }

            // There was no information in the URL after the Content.aspx reference, so check if there is a 
            // URL parameter. The URL parameter will only exist if there was an HttpModule to create it.
            if (String.IsNullOrEmpty(contentPath))
            {
                contentPath = Request.QueryString[FramesetQueryParameter.ContentFilePath];
            }

            return contentPath;
        }

        #region Called from aspx
        // Gets url path to the SLK folder that contains our images, theme, etc.
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public Uri SlkEmbeddedUIPath
        {
            get
            {
                return new Uri(Request.Url, "/_layouts/SharePointLearningKit/Frameset/");
            }
        }

        public string ErrorTitleHtml
        {
            get
            {
                bool hasError, asInfo;
                string errorTitle, errorMsg;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);

                return errorTitle;
            }
        }

        public string ErrorMessageHtml
        {
            get
            {
                bool hasError, asInfo;
                string errorTitle, errorMsg;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);

                return errorMsg;
            }
        }

        // Write initialization code for frameset manager. This method is called when an "error" message 
        // is displayed. Note that this includes the case where the 'submit' page is displayed.
        public void WriteFrameMgrInit()
        {
            m_contentHelper.WriteFrameMgrInit();
        }

        public string ErrorIcon
        {
            get
            {
                if (ErrorAsInfo)
                    return "Info.gif";

                return "Error.gif";
            }
        }
        #endregion
    }
}
