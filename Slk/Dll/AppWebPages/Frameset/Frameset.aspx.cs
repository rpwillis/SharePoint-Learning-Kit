/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Globalization;
using System.Text;

using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.Frameset;
using Resources;
using Resources.Properties;
using System.Transactions;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.SharePointLearningKit.Frameset
{
    /// <summary>
    /// This is the top-level frameset for display views of a package. 
    /// The format of the URL is:
    ///     Frameset.aspx/[optional filename]
    /// If the optional filename is provided and the learner assignment corresponds to non-elearning content,
    /// then the file associated with the learner assignment is displayed.
    /// 
    /// The URL to this page differs based on the view requested.
    /// Query parameters are:
    /// SlkView: The value of the SlkView enum that corresponds to the view to be displayed
    /// LearnerAssignmentId: The learner assignment to be displayed
    /// </summary>
    public class FramesetCode : FramesetPage
    {
        private FramesetHelper m_framesetHelper;
        // If true, the content being displayed is e-learning (i.e., scorm or lrm) content
        private bool m_isELearning;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // all exceptions caught and written to server event log
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                SlkUtilities.RetryOnDeadlock(delegate()
                {
                    Response.Clear();
                    ClearError();

                    m_framesetHelper = new FramesetHelper();
                    m_framesetHelper.ProcessPageLoad(SlkStore.PackageStore, TryGetSessionView, TryGetAttemptId, ProcessViewRequest);

                    // If this is not e-learning content, then we have to write the content directly to response.
                    if (!HasError && !m_isELearning)
                    {
                        SendNonElearningContent();
                        Response.End();
                    }
                });
            }
            catch (ThreadAbortException)
            {
                // response ended. Do nothing.
                return;
            }
            catch (FileNotFoundException)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = "Not Found";

                RegisterError(SlkFrameset.FRM_DocumentNotFoundTitleHtml, SlkFrameset.FRM_DocumentNotFound, false);
            }
            catch (UserNotFoundException)
            {
                Response.StatusCode = 500;
                Response.StatusDescription = "Internal Server Error";

                // This probably indicates the site allows anonymous access and the user is not signed in. 
                RegisterError(SlkFrameset.FRM_AssignmentNotAvailableTitle, SlkFrameset.FRM_SignInRequiredMsgHtml, false);
            }
            catch (HttpException ex)
            {
                // Do not set response codes

                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, ex.ToString());
                RegisterError(SlkFrameset.FRM_AssignmentNotAvailableTitle, SlkFrameset.FRM_AssignmentNotAvailableMsgHtml, false);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.StatusDescription = "Internal Server Error";

                SlkUtilities.LogEvent(System.Diagnostics.EventLogEntryType.Error, ex.ToString());
                RegisterError(SlkFrameset.FRM_AssignmentNotAvailableTitle, SlkFrameset.FRM_AssignmentNotAvailableMsgHtml, false);
            }
        }

        /// <summary>
        /// Gets the attempt id of the requested attempt. If the page is provided a learner assignment id and 
        /// the attempt associated with the assignment doesn't exist, then one is created. If the user does not have 
        /// access to the attempt, or the requested view of the attempt, or there is no 
        /// attempt (such as non-elearning content) then false is returned.
        /// </summary>
        /// <param name="showErrorPage"></param>
        /// <param name="attemptId"></param>
        /// <returns></returns>
        public override bool TryGetAttemptId(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            // Initialize out parameter
            attemptId = null;

            Guid learnerAssignmentGuidId;

            if (!TryProcessLearnerAssignmentIdParameter(showErrorPage, out learnerAssignmentGuidId))
                // In this case, if the parameter was not valid (eg, it's not a number), the error is already registered. 
                // So just return.
                return false;
                    
            LearnerAssignmentGuidId = learnerAssignmentGuidId;

            // Put this operation in a transaction because if the attempt has not been started, we'll start the attempt 
            // and update the assignment. Both should succeed or both should fail.
            LearnerAssignmentProperties la;
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                // Must force a read of assignment data so that it's read in the same transaction that it might be updated.
                la = GetLearnerAssignment(true);

                SessionView view;
                if (!TryGetSessionView(true, out view))
                    return false;

                if (IsELearningAssignment(la))
                {
                    // This is e-learning content
                    m_isELearning = true;

                    // Accessing LearnerAssignment (above) would have checked the database and retrieve any information available about 
                    // the assignment, including its attempt id, if it exists. If the LearnerAssignment information is valid 
                    // but there's no attempt, the AttemptId property will be null
                    if (la.AttemptId == null)
                    {
                        // Only create the attempt if this is a request for Execute view
                        if (view == SessionView.Execute)
                        {
                            if (!FileExistsInSharePoint(la.Location))
                            {
                                if (showErrorPage)
                                {
                                    RegisterError(SlkFrameset.FRM_PackageNotFoundTitle, SlkFrameset.FRM_PackageNotFound, false);
                                }
                                return false;
                            }

                            SlkStore.StartAttemptOnLearnerAssignment(learnerAssignmentGuidId);

                            // Force a reset of internal data regarding the current learner assignment
                            la = GetLearnerAssignment(true);
                        }
                        else
                        {
                            // This is an error condition, since in other cases, the attempt must already exist.
                            // Use private method to get the right error message.
                            if (!ProcessViewRequest(la, view))
                                return false;
                        }
                    }
                    else
                    {
                        // Attempt is already created. Verify the user has access to it and that the package exists. 

                        if (!ProcessViewRequest(la, view))
                            return false;

                        if (!FileExistsInSharePoint(la.Location))
                        {
                            if (showErrorPage)
                            {
                                RegisterError(SlkFrameset.FRM_PackageNotFoundTitle, SlkFrameset.FRM_PackageNotFound, false);
                            }
                            return false;
                        }
                    }

                    // Attempt exists, set the out parameter 
                    attemptId = la.AttemptId;
                }
                else
                {
                    // Is not e-learning assignment
                    m_isELearning = false;

                    // Verify that the learner can see the assignment.
                    if (view == SessionView.Execute)
                    {
                        if (!FileExistsInSharePoint(la.Location))
                        {
                            if (showErrorPage)
                            {
                                RegisterError(SlkFrameset.FRM_PackageNotFoundTitle, SlkFrameset.FRM_PackageNotFound, false);
                            }
                            return false;
                        }

                        // Mark the assignment as started
                        if (la.Status != LearnerAssignmentState.Active)
                        {
                            SlkStore.ChangeLearnerAssignmentState(la.LearnerAssignmentGuidId, LearnerAssignmentState.Active);
                        }
                    }
                    else
                    {
                        // Verify this is a view they have access to given the state of the assignment. No need to check 
                        // return value, as non-elearning content always returns false from this method.
                        if (!ProcessViewRequest(la, view))
                            return false;

                        if (!FileExistsInSharePoint(la.Location))
                        {
                            if (showErrorPage)
                            {
                                RegisterError(SlkFrameset.FRM_PackageNotFoundTitle, SlkFrameset.FRM_PackageNotFound, false);
                            }
                            return false;
                        }
                    }
                }
                scope.Complete();
            }
            
            return (attemptId != null);
        }

        /// Returns true if the correct version of the file exists in SharePoint.
        /// NOTE: This method checks file availability using elevated privileges. Be 
        /// cautious when using this information in messages displayed to the user.
        private static bool FileExistsInSharePoint(string location)
        {            
            SharePointFileLocation spFileLocation;
            bool fileExists = true;    // assume it exists
            if (SharePointFileLocation.TryParse(location, out spFileLocation))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    // If the site does not exist, this throws FileNotFound
                    using (SPSite spSite = new SPSite(spFileLocation.SiteId, SPContext.Current.Site.Zone))
                    {
                        // If the web does not exist, this throws FileNotFound
                        using (SPWeb spWeb = spSite.OpenWeb(spFileLocation.WebId))
                        {
                            SPFile spFile = spWeb.GetFile(spFileLocation.FileId);
                            if (!spFile.Exists)
                            {
                                fileExists = false;
                                return;
                            }
                            // The file exists. Now check if the right version exists.
                            DateTime lastModified;
                            if ((spFile.Versions.Count == 0) || spFile.UIVersion == spFileLocation.VersionId)
                            {
                                // The requested version is the currect one
                                if (spFile.UIVersion != spFileLocation.VersionId)
                                {
                                    fileExists = false;
                                    return;
                                }
                                // It exists: check its timestamp
                                lastModified = spFile.TimeLastModified;
                            }
                            else
                            {
                                // The specified version isn't the current one
                                SPFileVersion spFileVersion = spFile.Versions.GetVersionFromID(spFileLocation.VersionId);

                                if (spFileVersion == null)
                                {
                                    fileExists = false;
                                    return;
                                }

                                // There is no 'last modified' of a version, so use the time the version was created.
                                lastModified = spFileVersion.Created;
                            }

                            // If the timestamps are not the same, the file has been modified, so return false
                            if (lastModified.CompareTo(spFileLocation.Timestamp) != 0)
                            {
                                fileExists = false;
                                return;
                            }
                        }
                    }
                });
            }
            return fileExists;
        }

        /// <summary>
        /// Returns the path in the URL after the Frameset.aspx reference.
        /// </summary>
        /// <returns>The information about what content to display. It will return 
        /// null if there was no information. This indicates the frameset is initializing.</returns>
        private string GetFramesetPath()
        {
            string framesetPath = null;

            // See if there is information after the Frameset.aspx string in the raw URL
            string originalUrl = Request.Url.OriginalString;
            int beginContext = originalUrl.IndexOf("Frameset/Frameset.aspx/", StringComparison.OrdinalIgnoreCase);
            if (beginContext > 0)
            {
                int endContent = beginContext + "Frameset/Frameset.aspx".Length;
                // It's a request with information following the page name.
                framesetPath = originalUrl.Substring(endContent);
            }

            return framesetPath;
        }

        /// <summary>
        /// Based on the filename of the content to be displayed, this method gets the frameset url
        /// that should be redirected to in order to display that content.
        /// </summary>
        /// <returns>The url, in Ascii format, for the frameset window to redirect.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings"),   // the caller is going to write this to the response, so no point in creating another object
        SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")] // Elearning is ok
        protected string GetNonElearningFrameUrl(string fileName)
        {
            StringBuilder sb = new StringBuilder(4096);

            // The url to the frameset page is appended with the filename. This allows the browser to interpret the file 
            // type based on the URL, ending in, for instance, .doc.
            sb.Append(UrlCombine(SPWeb.Url, "_layouts/SharePointLearningKit/Frameset/Frameset.aspx", HttpUtility.UrlPathEncode(fileName)));

            // Append query parameters
            sb.AppendFormat(CultureInfo.CurrentCulture, "?{0}={1}&{2}={3}", 
                    FramesetQueryParameter.LearnerAssignmentGuidId, FramesetQueryParameter.GetValueAsParameter(LearnerAssignmentGuidId), 
                    FramesetQueryParameter.SlkView, FramesetQueryParameter.GetValueAsParameter(AssignmentView));

            return sb.ToString();
        }

        /// <summary>
        /// Given a SharePoint File, find the URL location of the File
        /// </summary>
        /// <returns>The url in Ascii format</returns>
        protected string GetNonElearningDocumentUrl(SharePointFileLocation spFileLocation)
        {
            string documentUrl = null;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                // If the site does not exist, this throws FileNotFound
                using (SPSite spSite = new SPSite(spFileLocation.SiteId,SPContext.Current.Site.Zone))
                {
                    // If the web does not exist, this throws FileNotFound
                    using (SPWeb spWeb = spSite.OpenWeb(spFileLocation.WebId))
                    {
                        SPFile spFile = spWeb.GetFile(spFileLocation.FileId);
                        documentUrl = UrlCombine(spSite.Url, HttpUtility.UrlPathEncode(spFile.ServerRelativeUrl));
                    }
                }
            });

            return documentUrl;
        }

        /// <summary>
        /// Combines url paths in a similar way to Path.Combine for file paths.
        /// Beginning and trailing slashes are not needed but will be accounted for
        /// if they are present.
        /// </summary>
        /// <param name="basePath">The start of the url. Beginning slashes will not be removed.</param>
        /// <param name="args">All other url segments to be added. Beginning and ending slashes will be
        /// removed and ending slashes will be added.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        private static string UrlCombine(string basePath, params string[] args)
        {
            if (basePath == null)
                throw new ArgumentNullException("basePath");
            if (args == null)
                throw new ArgumentNullException("args");

            if (basePath.EndsWith("/", StringComparison.Ordinal))
                basePath = basePath.Remove(basePath.Length - 1);

            StringBuilder sb = new StringBuilder(basePath);
            foreach (string path in args)
            {
                string tempPath = path;
                if (tempPath.EndsWith("/", StringComparison.Ordinal))
                {
                    tempPath = tempPath.Remove(tempPath.Length - 1);
                }
                if (tempPath.StartsWith("/", StringComparison.Ordinal))
                {
                    sb.AppendFormat("{0}", tempPath);
                }
                else
                {
                    sb.AppendFormat("/{0}", tempPath);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if the learner assignment references e-learning content.
        /// </summary>
        private static bool IsELearningAssignment(LearnerAssignmentProperties la)
        {
           return (la.RootActivityId != null);
        }

        /// <summary>
        /// Called by FramesetHelper. Delegate to return session view.
        /// </summary>
        public override bool TryGetSessionView(bool showErrorPage, out SessionView view)
        {
            view = SessionView.Execute; // must be initialized

            AssignmentView assignmentView;
            if (!TryProcessAssignmentViewParameter(showErrorPage, out assignmentView))
                return false;
            AssignmentView = assignmentView;

            return base.TryGetSessionView(showErrorPage, out view);
        }

        ///// <summary>
        ///// Process a request for a view. If not allowed, register an error and return false.
        ///// </summary>
        ///// <param name="view"></param>
        ///// <param name="session"></param>
        //public bool ProcessViewRequest(SessionView view, LearningSession session)
        //{
        //    LearnerAssignmentProperties la = GetLearnerAssignment();

        //    return ProcessViewRequest(la, view);
        //}

        ///// <summary>
        ///// Process a view request to determine if it's valid. The AssignmentView must be 
        ///// set before calling this method.
        ///// </summary>
        //private bool ProcessViewRequest(LearnerAssignmentProperties la, SessionView sessionView)
        //{

        //    switch (AssignmentView)
        //    {
        //        case AssignmentView.Execute:
        //            {
        //                // Verify that session view matches what you expect
        //                if (sessionView != SessionView.Execute)
        //                {
        //                    throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
        //                }

        //                // The assignment must have started
        //                if (!VerifyAssignmentStartDate(la, true))
        //                {
        //                    return false;
        //                }

        //                // Can only access active assignments in Execute view
        //                if (la.Status != LearnerAssignmentState.Active)
        //                {
        //                    RegisterError(SlkFrameset.FRM_AssignmentNotAvailableTitle,
        //                        SlkFrameset.FRM_AssignmentTurnedInMsgHtml, false);
                            
        //                    return false;
        //                }
        //                break;
        //            }
        //        case AssignmentView.Grading:
        //            {
        //                // Verify that session view matches what you expect
        //                if (sessionView != SessionView.RandomAccess)
        //                {
        //                    throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
        //                }

        //                // Grading is not available if the assignment has not been submitted.
        //                if ((la.Status == LearnerAssignmentState.Active)
        //                    || (la.Status == LearnerAssignmentState.NotStarted))
        //                {
        //                    RegisterError(SlkFrameset.FRM_AssignmentNotGradableTitle,
        //                     SlkFrameset.FRM_AssignmentCantBeGradedMsgHtml, false);
        //                    return false;
        //                }
        //                break;
        //            }
        //        case AssignmentView.InstructorReview:
        //            {
        //                // Verify that session view matches what you expect
        //                if (sessionView != SessionView.Review)
        //                {
        //                    throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
        //                }

        //                // Only available if student has started the assignment
        //                if (la.Status == LearnerAssignmentState.NotStarted)
        //                {
        //                    RegisterError(SlkFrameset.FRM_ReviewNotAvailableTitle,
        //                     SlkFrameset.FRM_ReviewNotAvailableMsgHtml, false);
        //                    return false;
        //                }

        //                break;
        //            }
        //        case AssignmentView.StudentReview:
        //            {
        //                // Verify that session view matches what you expect
        //                if (sessionView != SessionView.Review)
        //                {
        //                    throw new InvalidOperationException(SlkFrameset.FRM_UnexpectedViewRequestHtml);
        //                }

        //                // The assignment must have started
        //                if (!VerifyAssignmentStartDate(la, true))
        //                {
        //                    return false;
        //                }

        //                // If requesting student review, the assignment state must be final
        //                if (la.Status != LearnerAssignmentState.Final)  
        //                {
        //                    RegisterError(SlkFrameset.FRM_ReviewNotAvailableTitle,
        //                        SlkFrameset.FRM_LearnerReviewNotAvailableMsgHtml, false);
        //                    return false;
        //                }

        //                break;
        //            }
        //        default:
        //            break;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Send the non-elearning content to the response. Non-elearning content is any content other than 
        /// scorm or lrm content. It is an error to call this method when the current learner assignment is elearning content.
        /// In some cases, this method will end the response.
        /// </summary>
        private void SendNonElearningContent()
        {
            // Get the cached learner assignment properties
            LearnerAssignmentProperties la = GetLearnerAssignment();

            SharePointFileLocation spFileLocation;
            if (!SharePointFileLocation.TryParse(la.Location, out spFileLocation))
            {
                // location was not valid
                RegisterError(SlkFrameset.FRM_DocumentNotFoundTitleHtml, SlkFrameset.FRM_DocumentNotFound, false);
                return;
            }

            // Find the location of the document in the Sharepoint Document Library and go there
            string documentUrl = GetNonElearningDocumentUrl(spFileLocation);

            // Special case handling for HTML files:
            //   * Launch directly from the document library, not from the Cache
            //   * Add the LearnerAssignmentId to the Query portion of the URL so that we pass the learning context down to the content
            // This enables us to have more advanced behavior (CMI Tracking, scoring, completion status) for "Non-ELearning" documents.
            // It is a first step towards a more comprehensive strategy for adding educational workflow to all types of documents.
            if (documentUrl.EndsWith("html", StringComparison.OrdinalIgnoreCase) || documentUrl.EndsWith("htm", StringComparison.OrdinalIgnoreCase))
            {
                string redirectUrl = String.Format("{0}?{1}={2}", documentUrl, FramesetQueryParameter.LearnerAssignmentGuidId, LearnerAssignmentGuidId.ToString());
                Response.Clear();
                Response.Redirect(redirectUrl, true); // ends response
            }

            using (CachedSharePointFile cachedFile = new CachedSharePointFile(SlkStore.SharePointCacheSettings, spFileLocation, true))
            {
                // If the current request URL does not include the file name of the file, then this request is the first frameset rendering. 
                // That means this will redirect to a URL that does include the filename of the file. This redirection allows the browser to 
                // properly handle the content.
                string framesetPath = GetFramesetPath();
                if (String.IsNullOrEmpty(framesetPath))
                {
                    string redirectUrl = GetNonElearningFrameUrl(cachedFile.FileName);
                    Response.Clear();
                    Response.Redirect(redirectUrl, true);   // ends response
                }

                // This is the first actual access of the file. If it doesn't exist, the exception will be caught by the Page_load method.
                SetMimeType(cachedFile.FileName);

                // Clear the response and write the file.
                Response.Clear();

                // If this file is using IIS Compability mode, then we get the stream from the cached file and write it to the 
                // response, otherwise, use TransmitFile.
                if (UseCompatibilityMode(cachedFile.FileName))
                {
                    WriteIisCompatibilityModeToResponse(cachedFile.GetFileStream());
                }
                else
                {
                    cachedFile.TransmitFile(Response);
                }
            }
        }

        /// <summary>
        /// Returns true if the fileName indicates it must use IIS compatibility mode
        /// </summary>
        private bool UseCompatibilityMode(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            ICollection<string> compatExtensions = SlkStore.Settings.NonELearningIisCompatibilityModeExtensions;
            foreach (string compatExtension in compatExtensions)
            {
                if ((String.CompareOrdinal(compatExtension, fileExtension) == 0) ||
                    (String.CompareOrdinal(compatExtension, ".*") == 0)) // meaning 'all'
                    return true;
            }

            return false;
        }
        
        private const int BUFFER_SIZE = 100000;
        /// <summary>
        /// Write the stream to the response using IIS compatibility mode. The stream will be closed 
        /// after it is written.
        /// </summary>
        private void WriteIisCompatibilityModeToResponse(Stream packageStream)
        {
            // Create buffer big enough to handle many files in one chunk, but small enough to not thrash
            // the response.
            byte[] buffer = new Byte[BUFFER_SIZE];

            // Length of the file
            int length;

            // Total bytes to read
            long dataToRead;

            try
            {

                // Total bytes to read:
                dataToRead = packageStream.Length;

                Response.AppendHeader("Content-length", dataToRead.ToString(CultureInfo.InvariantCulture));

                // Read the bytes.
                while (dataToRead > 0)
                {
                    // Verify that the client is connected.
                    if (Response.IsClientConnected)
                    {
                        // Read the data in buffer.
                        length = packageStream.Read(buffer, 0, BUFFER_SIZE);

                        // Write the data to the current output stream.
                        Response.OutputStream.Write(buffer, 0, length);

                        // Flush the data to the HTML output.
                        Response.Flush();

                        buffer = new Byte[BUFFER_SIZE];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            finally
            {
                if (packageStream != null)
                {
                    //Close the file.
                    packageStream.Close();
                }
            }
        }


        /// <summary>
        /// Sets the mime type on the response based on the file name.
        /// </summary>
        private void SetMimeType(string fileName)
        {
            IDictionary<string, string> mappings = SlkStore.Settings.MimeTypeMappings;
            string mimeType;
            string fileExtension = Path.GetExtension(fileName);

            if (mappings.ContainsKey(fileExtension))
            {
                mimeType = mappings[fileExtension];
            }
            else
            {
                mimeType = "application/octet-stream";
            }

            Response.ContentType = mimeType;
        }

        #region Called From Aspx    // the following methods are called from in-place aspx code

        /// <summary>
        /// Gets the URL to the page loaded into the MainFrames frame.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings"),    
        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), // the caller will use the string
        SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")] // MainFrames is correct
        public string MainFramesUrl
        {
            get
            {
                StringBuilder frames = new StringBuilder(4096);
                int view = Convert.ToInt32(AssignmentView, NumberFormatInfo.InvariantInfo);                
                frames.Append(String.Format(CultureInfo.CurrentUICulture, "MainFrames.aspx?{0}={1}&",
                                                FramesetQueryParameter.SlkView, view.ToString(NumberFormatInfo.InvariantInfo)));
                
                GetLearnerAssignment();

                frames.Append(String.Format(CultureInfo.CurrentUICulture, "{0}={1}",
                                            FramesetQueryParameter.LearnerAssignmentGuidId, FramesetQueryParameter.GetValueAsParameter(LearnerAssignmentGuidId)));
            
                return new UrlString(frames.ToString()).ToAscii();
            }
        }

        /// <summary>
        /// Gets the title for the frameset. The one that goes into the title bar of the browser window.
        /// </summary>
        public static string PageTitleHtml
        {
            get
            {
                PlainTextString text = new PlainTextString(ResHelper.GetMessage(SlkFrameset.FRM_Title));
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

        public bool OpenedByGradingPage
        {
            get
            {
                string srcParam = Request.QueryString[FramesetQueryParameter.Src];
                if (!String.IsNullOrEmpty(srcParam)
                    && (String.Compare(srcParam, "Grading", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return true;
                }
                return false;
            }
        }

        public static string CannotUpdateGradingHtml
        {
            get { return SlkFrameset.FRM_GradingPageNotUpdated; }
        }
        #endregion  // called from aspx
    }
}