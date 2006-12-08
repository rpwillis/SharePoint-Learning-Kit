/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.
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
using System.Collections.Generic;
using Resources;
using System.Web.Caching;
using System.Web.Hosting;
using System.Text;

// Views supported in this page: Execute. 
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
//
namespace Microsoft.LearningComponents.Frameset
{
    public partial class Frameset_Content : BwpFramesetPage
    {
        ContentHelper m_contentHelper;

        // The path to the requested content. This may be part of the url (if we are running without  
        // an http module) or a URL parameter (with http module). We run in both modes because 
        // VS.NET does not parse URLs of the form: /.../Content.aspx/0/1/foo.gif correctly without 
        // the assistance of the module.
        string m_contentPath;

        // pfParts[0] = "", pfParts[1] = view, pfParts[2] = extra view data, everything else is path or extra data for attempt views
        string[] m_contentPathParts;

        SessionView m_view;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Get the path to the content. This may be part of the url (if we are running without  
                // an http module) or a URL parameter (with http module). We run in both modes because 
                // VS.NET does not parse URLs of the form: /.../Content.aspx/0/1/foo.gif correctly without 
                // the assistance of the module.
                m_contentPath = GetContentPath();
                bool isPosted = false;
                if (String.CompareOrdinal(Request.HttpMethod, "POST") == 0)
                    isPosted = true;

                m_contentHelper = new ContentHelper(Request, Response, FramesetPath);
                m_contentHelper.ProcessPageLoad(PStore,
                                                    String.IsNullOrEmpty(m_contentPath),
                                                    isPosted,
                                                    GetViewInfo,
                                                    GetAttemptInfo,
                                                    GetActivityInfo,
                                                    GetResourcePath,
                                                    AppendContentFrameDetails,
                                                    UpdateRenderContext,
                                                    ProcessPostedData,
                                                    ProcessViewRequest,
                                                    ProcessPostedDataComplete,
                                                    RegisterError,
                                                    GetErrorInfo,
                                                    GetMessage);
            }
            catch (ThreadAbortException)
            {
                // response ended. Do nothing.
                return;
            }
            catch (Exception e2)
            {
                ClearError();
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle),
                   ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionMsg, HttpUtility.HtmlEncode(e2.Message.Replace("\r\n", " "))), false);
                
                // Clear any existing response information so that the error gets rendered correctly.
                Response.Clear();   
            }
        }

        public bool GetViewInfo(bool showErrorPage, out SessionView view)
        {
            // make compiler happy
            view = SessionView.Execute;

            // Get the parts of the content path (parameter PF) value
            // pfParts[0] = "", pfParts[1] = view, pfParts[2] = extra view data, everything else is path or extra data for attempt views
            m_contentPathParts = m_contentPath.Split(new char[] { '/' });

            // First section is view -- it must exist in all cases.
            string strView = m_contentPathParts[1];
            if (String.IsNullOrEmpty(strView))
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredTitle, FramesetQueryParameter.View),
                    ResHelper.GetMessage(FramesetResources.FRM_ParameterRequiredMsg, FramesetQueryParameter.View), false);
                }
                return false;
            }

            try
            {
                view = (SessionView)Enum.Parse(typeof(SessionView), strView, true);
                if (!Enum.IsDefined(typeof(SessionView), view))
                {
                    if (showErrorPage)
                    {
                        RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, strView), false);
                    }
                    return false;
                }
            }
            catch (ArgumentException)
            {
                if (showErrorPage)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterTitle, FramesetQueryParameter.View),
                        ResHelper.GetMessage(FramesetResources.FRM_InvalidParameterMsg, FramesetQueryParameter.View, strView), false);
                }
                return false;
            }
            m_view = view;
            return true;
        }

        public bool GetAttemptInfo(bool showErrorPage, out AttemptItemIdentifier attemptId)
        {
            attemptId = null;    

            long attemptKey;
            bool isValid = false;

            // For views based on an attempt, AttemptId is required
            // It must be a positive long value. 
            if (m_contentPathParts.Length >= 3)
            {
                if (long.TryParse(m_contentPathParts[2], out attemptKey))
                {
                    if (attemptKey > 0)
                    {
                        attemptId = new AttemptItemIdentifier(attemptKey);
                        isValid = true;
                    }
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

        public bool GetActivityInfo(bool showErrorPage, out long activityId)
        {
            // This will only be called in Review view, in which case, m_contentPathParts is...

            // m_contentPathParts[1] = view, m_contentPathParts[2] = attemptId, m_contentPathParts[3] = activityId to display, m_contentPathParts[4] and beyond is resource path

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

        public string GetResourcePath()
        {
            if (m_view == SessionView.Review)
            {
                return m_contentPath.Substring(m_contentPathParts[1].Length + m_contentPathParts[2].Length + m_contentPathParts[3].Length + 4);
            }
            else if (m_view == SessionView.Execute)
            {
                // Get the relative path to the resource. This may not exist, and that's ok. If it exists, its it the portion
                // of param that follows the view and attempt id. The 3 below is the forward slashes.
                return m_contentPath.Substring(m_contentPathParts[1].Length + m_contentPathParts[2].Length + 3);
            }
            return string.Empty;
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
            int beginContext = rawUrl.IndexOf("Frameset/Content.aspx/", StringComparison.InvariantCultureIgnoreCase);
            if (beginContext > 0)
            {
                int endContent = beginContext + "Frameset/Content.aspx".Length;
                // It's a request with information following the page name.
                string filePath = rawUrl.Substring(0, endContent);
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

        /// <summary>
        /// Allow the application to modify the RenderContext before content is rendered.
        /// </summary>
        /// <param name="context">The context to render. Note: If app needs to change context.Script,
        /// it should do so by adding to the scriptBlock buffer. </param>
        /// <param name="scriptBlock">The script that will be written in the onLoad handler of LRM content. 
        /// If this value is null, then the app 
        /// cannot modify the onload script. (This happens when the resource being rendered is not 
        /// the primary resource of an activity that is in LRM format.)</param>
        /// <param name="session">The session in which content is being rendered.</param>
        public void UpdateRenderContext(RenderContext context, StringBuilder scriptBlock, LearningSession session)
        {
            // Set values other than Response and RelativePath
            context.EmbeddedUIResourcePath = new Uri(Request.Url, "Images/");

            // Set values that map to file extensions
            SetMappings(context.MimeTypeMapping, context.IisCompatibilityModeExtensions);
            
            if (scriptBlock == null)    // This is not the primary resource, so nothing else matters
                return;

            // Basic Web Player never shows correct answers or reviewer info
            context.ShowCorrectAnswers = false;
            context.ShowReviewerInformation = false;
        }

        /// <summary>
        /// Allow application to process posted data. 
        /// </summary>
        public bool ProcessPostedData(LearningSession session, HttpRequest request, Dictionary<string, HttpPostedFile> fileCollection)
        {
            // Verify that posted files map to files that actually exist. 
            HttpFileCollection files = request.Files;
            StringBuilder messageBuffer = new StringBuilder();
            bool firstError = true;
            foreach (string fileId in files)
            {
                HttpPostedFile postedFile = files[fileId];
                string filename = postedFile.FileName;

                // If contentLength == 0 and fileName == emptyString, then this is probably a posting after
                // the initial file posting. (For instance, to remove the file.) This is a valid file and is added to the 
                // collection.
                // If the contentLength == 0 and fileName != emptyString, then user is trying to attach a file
                // that has no contents. This is not allowed.
                if ((String.IsNullOrEmpty(filename) && (postedFile.ContentLength == 0))
                        || (!String.IsNullOrEmpty(filename) && (postedFile.ContentLength > 0)))
                {
                    fileCollection.Add(fileId, postedFile);
                }
                else
                {
                    // This is not a valid file.
                    if (firstError)
                    {
                        messageBuffer.Append(FramesetResources.CON_AttachedFileDoesNotExistHtml);
                        messageBuffer.Append("\r\n<br><br><ul>\r\n");
                        firstError = false;
                    }
                    messageBuffer.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(filename));
                }
            }

            if (!firstError)
            {
                messageBuffer.Append("</ul><br>");
                messageBuffer.Append(FramesetResources.CON_FileAttachmentErrorEndHtml);
                
                // Add information for the 'Continue' link
                JScriptString js = new JScriptString(ResHelper.FormatInvariant("API_GetFramesetManager().DoChoice(\"{0}\");",
                                    FramesetUtil.GetStringInvariant(session.CurrentActivityId)));
                messageBuffer.AppendFormat(CultureInfo.CurrentCulture, "<br><br><a href='{0}' >{1}</a>",
                                js.ToJavascriptProtocol(), HttpUtility.HtmlEncode(FramesetResources.HID_ReloadCurrentContent));

                RegisterError(FramesetResources.CON_FileAttachmentErrorTitleHtml, messageBuffer.ToString(), false);
            }

            return true;
        }

        /// <summary>
        /// Delegate to allow BWP to take action after posted data is processed.
        /// </summary>
        /// <param name="session"></param>
        public void ProcessPostedDataComplete(LearningSession session)
        {
            // Do nothing
        }

        /// <summary>
        /// Set the mime mapping values into the <paramref name="mimeMapping"/> collection.
        /// The name is the extension (eg, "txt"), the value is the mime type.
        /// </summary>
        private void SetMappings(IDictionary<string, string> mimeMapping, ICollection<string> iisCompatibilityModeExtensions)
        {
            XPathNavigator contentNav = GetMimeMappingXml();

            XPathNodeIterator nodeIter = contentNav.Select("/contentTypes/contentType");
            Dictionary<string, string> results = new Dictionary<string, string>(nodeIter.Count);

            while (nodeIter.MoveNext())
            {
                XPathNavigator current = nodeIter.Current;
                string extension = current.GetAttribute("extension", String.Empty);
                mimeMapping.Add(extension, current.GetAttribute("mime", String.Empty));

                if (current.GetAttribute("iiscompatmode", String.Empty).CompareTo("true") == 0)
                    iisCompatibilityModeExtensions.Add(extension);
            }
        }

        private const string MIME_MAPPING = "MimeMapping";

        /// <summary>
        /// Retrieve the mime mapping xml from the cache (if it's there) or the file system (if it's not).
        /// </summary>
        private XPathNavigator GetMimeMappingXml()
        {
            if (Cache[MIME_MAPPING] != null)
            {
                return (XPathNavigator)Cache[MIME_MAPPING];
            }

            // It's not in the cache, so add it.
            string mimeMappingPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\ContentTypeMapping.xml");
            XmlDocument mappings = new XmlDocument();
            mappings.Load(mimeMappingPath);
            XPathNavigator contentNav = mappings.CreateNavigator();
            Cache.Insert(MIME_MAPPING, contentNav, new CacheDependency(mimeMappingPath));
            return contentNav;
        }
        
        #region called from aspx
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
                    // Find the part of the first instance of /Frameset/Content.aspx and then get a substring that is the 
                    // current url, up to the end of the /Frameset/.
                    string framesetPath  = currentExecutionPath.Substring(0, currentExecutionPath.IndexOf("/Frameset/Content.aspx", StringComparison.OrdinalIgnoreCase) + 10);
                    m_framesetPath = new Uri(framesetPath, UriKind.Absolute);
                }
                return m_framesetPath;
            }
        }

        public string ErrorTitleHtml
        {
            get
            {
                bool hasError, errorAsInfo;
                string errorTitle, errorMsg;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out errorAsInfo);

                return errorTitle;
            }
        }

        public string ErrorMessageHtml
        {
            get
            {
                bool hasError, errorAsInfo;
                string errorTitle, errorMsg;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out errorAsInfo);

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
