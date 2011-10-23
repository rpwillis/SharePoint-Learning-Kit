/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using Resources;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Microsoft.LearningComponents.Frameset
{

    /// <summary>
    /// Delegate to return the activity id to display as the current activity.
    /// </summary>
    /// <param name="showError">If true, error information is registered.</param>
    /// <param name="rootActivityId"></param>
    /// <returns></returns>
    public delegate bool TryGetActivityInfo(bool showError, out long rootActivityId);

    /// <summary>
    /// Return the resource path, which is a relative path within the package to requested resource file to render.
    /// </summary>
    public delegate string GetResourcePath();

    /// <summary>
    /// Delegate to register an error condition. Only the first registered error will be displayed to the user.
    /// </summary>
    /// <param name="shortDescriptionHtml">A short description (sort of a title) of the problem. Html format.</param>
    /// <param name="messageHtml">A longer description of the problem. Html format.</param>
    /// <param name="asInfo">If true, the error is displayed as information</param>
    public delegate void RegisterError(string shortDescriptionHtml, string messageHtml, bool asInfo);

    /// <summary>
    /// Delegate to allow the application to verify posted data before it is saved to the session. If the method
    /// returns 'false' the data will not be saved. The application will populate the fileCollection with posted
    /// files that it considers to be valid.
    /// </summary>
    public delegate bool ProcessPostedData(LearningSession session, HttpRequest request, Dictionary<string, HttpPostedFile> fileCollection);

    /// <summary>
    /// Delegate to allow the application to take action on a session after all the posted data has been processed.
    /// </summary>
    public delegate void ProcessPostedDataComplete(LearningSession session);

    /// <summary>
    /// Delegate to allow the application to update the render context. The application does not 
    /// need to set RelativePath or Response. To set script in the rendered content, add information to the onLoadScript.
    /// </summary>
    public delegate void UpdateRenderContext(RenderContext renderContext, StringBuilder onLoadScript, LearningSession session);

    /// <summary>
    /// Code that is used to assist in rendering the Content.aspx page of the frameset.
    /// This code is shared in Basic Web Player and SLK framesets.
    /// </summary>
    public class ContentHelper : PostableFrameHelper
    {
        private UpdateRenderContext m_updateRenderContext;
        private UpdateRenderContext UpdateRenderContext
        {
            get { return m_updateRenderContext; }
            set { m_updateRenderContext = value; }
        }

        private bool m_isPostedPage;    // if true, the page was posted

        /// <summary>
        /// Create ContentHelper with request and response information.
        /// </summary>
        /// <param name="embeddedUIPath">The path to UI such as images and themes which is part of the product (as opposed 
        /// to part of the package being rendered).</param>
        /// <param name="request">The request for the current page.</param>
        /// <param name="response">The response to send to the client.</param>
        public ContentHelper(HttpRequest request, HttpResponse response, Uri embeddedUIPath) 
                    : base(request, response, embeddedUIPath)
        {
        }

        /// <summary>
        /// Process the page load event. This method will end the response.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]   // delegate parameters that are called as methods are cased as methods
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // it's not worth changing this now
        public void ProcessPageLoad(PackageStore packageStore,
                                    bool firstRendering,    // frameset is being initialized
                                    bool isPostBack,    // page was posted
                                    TryGetViewInfo TryGetViewInfo,
                                    TryGetAttemptInfo TryGetAttemptInfo,
                                    TryGetActivityInfo TryGetActivityInfo,
                                    GetResourcePath GetResourcePath,
                                    AppendContentFrameDetails appendContentFrameDetails,
                                    UpdateRenderContext updateRenderContext,
                                    ProcessPostedData ProcessPostedData,
                                    ProcessViewRequest ProcessViewRequest, 
                                    ProcessPostedDataComplete ProcessPostedDataComplete,
                                    RegisterError registerError,
                                    GetErrorInfo getErrorInfo,
                                    GetFramesetMsg getFramesetMsg)
        {
            RegisterError = registerError;
            UpdateRenderContext = updateRenderContext;
            GetErrorInfo = getErrorInfo;
            AppendContentFrameDetails = appendContentFrameDetails;
            GetFramesetMsg = getFramesetMsg;

            m_isPostedPage = isPostBack;
                    
            // If this is the first time the page is being rendered, just show 'please wait' and return.
            if (firstRendering)
            {
                // Initial page rendering, show "please wait"
                WritePleaseWaitToResponse(Response);
                Response.End();
                return;
            }

            SessionView view;
            
            // Get View information. It determines what else to look for.
            if (!TryGetViewInfo(true, out view))
            {
                return;
            }

            // There is something to display, so process the request...

            AttemptItemIdentifier attemptId;
            if (!TryGetAttemptInfo(true, out attemptId))
                return;

            StoredLearningSession slsSession = new StoredLearningSession(view, attemptId, packageStore);

            if (!ProcessViewRequest(view, slsSession))
                return; 

            Session = slsSession;

            if (slsSession.View == SessionView.Execute)
            {
                // Check if the session can be executed...

                if (slsSession.AttemptStatus != AttemptStatus.Active)
                {
                    RegisterError(GetFramesetMsg(FramesetStringId.CannotDisplayContentTitle),
                                        GetFramesetMsg(FramesetStringId.SessionIsNotActiveMsg), false);
                    return;
                }

                if (!slsSession.CurrentActivityIsActive)
                {
                    RegisterError(GetFramesetMsg(FramesetStringId.SelectActivityTitleHtml),
                                  GetFramesetMsg(FramesetStringId.SelectActivityMessageHtml), true);
                    return;
                }
            }
            else if (slsSession.View == SessionView.Review)
            {
                // Get information about which activity to review, then make that the current activity.

                long activityId = -1;   // make compiler happy
                if (!TryGetActivityInfo(true, out activityId))
                    return;

                // Move to the requested activity. Under normal conditions, this should always succeed, since the frameset should be 
                // giving this page a valid activity id.
                MoveToActivity(slsSession, activityId);
            }

            else if (slsSession.View == SessionView.RandomAccess)
            {
                // Get information about which activity to edit, then make that the current activity.

                long activityId = -1;   // make compiler happy
                if (!TryGetActivityInfo(true, out activityId))
                    return;

                // Move to the requested activity. Under normal conditions, this should always succeed, since the frameset should be 
                // giving this page a valid activity id.
                MoveToActivity(slsSession, activityId);
            }

            if (isPostBack /* && !SessionEnded */)
            {
                //Process information from posted content
                if (!this.SessionIsReadOnly)
                {
                    HttpFileCollection files = Request.Files;
                    int numFiles = files.Count;
                    Dictionary<string, HttpPostedFile> newFileCollection = new Dictionary<string, HttpPostedFile>(numFiles);

                    // Allow the application to process the form data
                    if (!ProcessPostedData(slsSession, Request, newFileCollection))
                        return;

                    // Allow MLC to process the form data
                    Session.ProcessFormData(Request.Form, newFileCollection);

                    // Allow application to do final processing after all posted data is processed.
                    ProcessPostedDataComplete(Session);

                    // If there was an error in processing form data, end the processing. This allows, for instance, to 
                    // save the data, display an error and not have commands (such as 'move to next activity') executed.
                    if (HasError)
                    {
                        Session.CommitChanges();
                        return;
                    }
                }

                // Issue with Review view: where to get activity id? From URL or posted data?

                // Find out what the commands are and do them.
                ICollection<CommandInfo> commands = GetCommands();
                foreach (CommandInfo cmdInfo in commands)
                {
                    switch (cmdInfo.Command)
                    {
                        case Commands.DoNext:
                            {
                                // When leaving a current activity, we must allow navigation requests the SCO has made to be 
                                // executed. If that results in changing the current activity, then do not also ask for another 
                                // move.
                                if (!ProcessNavigationRequests(Session))
                                {
                                    if (Session.IsMoveToNextValid())
                                    {
                                        MoveToNextActivity(Session);
                                        ActivityHasChanged = true;
                                    }
                                }
                                else
                                {
                                    ActivityHasChanged = true;
                                }

                                if (!ActivityHasChanged)
                                {
                                    // Moving to the next activity is not valid. 

                                    WriteError(ResHelper.Format(GetFramesetMsg(FramesetStringId.MoveToNextFailedHtml), ThemeFolderPath), true);
                                }
                                
                            }
                            break;
                        case Commands.DoPrevious:
                            {
                                if (!ProcessNavigationRequests(Session))
                                {
                                    if (Session.IsMoveToPreviousValid())
                                    {
                                       MoveToPreviousActivity(Session);
                                       ActivityHasChanged = true;
                                    }
                                }
                                else
                                {
                                    ActivityHasChanged = true;
                                }


                                if (!ActivityHasChanged)
                                {
                                    // Moving to the previous activity is not valid.
                                    WriteError(ResHelper.Format(GetFramesetMsg(FramesetStringId.MoveToPreviousFailedHtml), ThemeFolderPath), true);
                                }

                            }
                            break;
                        case Commands.DoChoice:
                        case Commands.DoTocChoice:
                            {
                                // These commands are used to navigate to activities, and to navigate to the final 'submit' page.
                                // In SCORM content, these commands do different things. In LRM content (which is what we are 
                                // in, since this is the posted page), they have identical effects. 
                                
                                // First check whether this is a request for the submit page or an activity.

                                string cmdData = cmdInfo.CommandData;
                                if (String.CompareOrdinal(cmdData, SubmitId) == 0)
                                {
                                    // Requesting submit page. Do not change the current activity, but mark it as changed so that 
                                    // it appears to the user that it has changed.
                                    ActivityHasChanged = true;
                                    string title = GetFramesetMsg(FramesetStringId.SubmitPageTitleHtml);
                                    string message;
                                    string saveBtn = GetFramesetMsg(FramesetStringId.SubmitPageSaveButtonHtml);
                                    if (Session.HasCurrentActivity)
                                        message = GetFramesetMsg(FramesetStringId.SubmitPageMessageHtml);
                                    else
                                        message = GetFramesetMsg(FramesetStringId.SubmitPageMessageNoCurrentActivityHtml);
                                    WriteSubmitPage(title, message, saveBtn);
                                }
                                else
                                {
                                    long activityId;
                                    if (long.TryParse(cmdData, out activityId))
                                    {
                                        // If the requested activity is the current activity, then do not do the navigation.
                                        // We skip it because moving to the activity basically exits the current attempt and creates
                                        // a new one. That new one also increments the attempt count. If we don't do the move, we 
                                        // pretend it was done. This will force the content frame to be reloaded with the current 
                                        // activity.
                                        if (IsCurrentActiveActivity(activityId))
                                            ActivityHasChanged = true;
                                        else
                                        {
                                            if (!ProcessNavigationRequests(Session))
                                            {
                                                if (Session.IsMoveToActivityValid(activityId))
                                                {
                                                    MoveToActivity(Session, activityId);
                                                    ActivityHasChanged = true;
                                                }
                                            }
                                            else
                                            {
                                                ActivityHasChanged = true;
                                            }
                                        }
                                    }
                                }

                                if (!ActivityHasChanged)
                                {
                                    // Moving to the selected activity is not valid. 

                                    WriteError(ResHelper.Format(GetFramesetMsg(FramesetStringId.MoveToActivityFailedHtml), ThemeFolderPath), true);
                                }
                            }
                            break;
                        case Commands.DoSave:
                            {
                                // Do nothing. The information will be saved since the page was posted.
                            }
                            break;
                        case Commands.DoSubmit:
                            {
                                if (Session.View == SessionView.Execute)
                                {
                                    ProcessNavigationRequests(Session);
                                    Session.Exit();
                                }
                                
                                ActivityHasChanged = true;
                            }
                            break;
                    }
                }
            }

            // If an error has been registered (and it's not the submit 
            // page rendering), don't attempt to render the content.
            if (HasError && !SubmitPageDisplayed)
            {
                if (!SessionIsReadOnly)
                {
                    Session.CommitChanges();
                }
                return;
            }

            // There was no error, so go ahead and render the content from the package.
            // If the current activity has changed in the processing of this page, then render the current activity's resource.
            // Otherwise, ask the application which resource to read.
            if (ActivityHasChanged)
            {
               // If the activity has changed, we render the content page without rendering the content. The page will then 
               // reload the content frame with the new activity.
                if (!SessionIsReadOnly)
                {
                    Session.CommitChanges();
                }
            }
            else
            {
                // Render the requested file in the current activity.
                RenderPackageContent(GetResourcePath());

                // If there was no error, end the response. Otherwise, we wait and render the error on the page.
                if (!HasError)
                {
                    Response.End();
                }
            }
            
                           
        }

        /// <summary>Write initialization code for frameset manager. </summary>
        /// <remarks>
        /// This method is called in three possible cases:
        /// 1. An error condition occurred
        /// 2. The submit page is being displayed. Note that in this case, since the submit page is registered as displaying an 
        /// error condition, HasError will be true.
        /// 3. The current activity has changed and we display this page mainly so that the 'please wait' information can be 
        /// displayed and the client can issue a GET request to load the new activity. 
        /// </remarks>
        public void WriteFrameMgrInit()
        {
            // Write frame to post. When displaying an error (which is the case, since we are here) the hidden frame is posted next
            Response.Write("frameMgr.SetPostFrame('frameHidden');\r\n");
            Response.Write("frameMgr.SetPostableForm(window.top.frames[MAIN_FRAME].document.getElementById(HIDDEN_FRAME).contentWindow.document.forms[0]);\r\n");

            if (HasError || SubmitPageDisplayed)
            {
                // Set the content frame URL to be null. This means the content frame will not be re-loaded by the frameMgr.
                Response.Write("frameMgr.SetContentFrameUrl(null); \r\n");
            }

            // If there is no session, we can't do anything else
            if (Session == null)
                return;
            
            if ((ActivityHasChanged) && (!SubmitPageDisplayed))
            {
                // Reload the content frame with the new activity.
                Response.Write(String.Format(CultureInfo.CurrentCulture, "frameMgr.SetContentFrameUrl(\"{0}\"); \r\n", GetContentFrameUrl()));

                // The new activity may be scorm content, so reinitialize the rte, if needed
                Response.Write(ResHelper.Format("frameMgr.InitNewActivity( {0} );\r\n", (CurrentActivityRequiresRte ? "true" : "false")));
            }            

            Response.Write(ResHelper.Format("frameMgr.SetAttemptId('{0}');\r\n", FramesetUtil.GetStringInvariant(Session.AttemptId.GetKey())));
            
            // Write view to display. 
            Response.Write(String.Format(CultureInfo.CurrentCulture, "frameMgr.SetView('{0}');\r\n", FramesetUtil.GetString(Session.View)));          

            // Write the current activity Id. Write -1 if there isn't one.
            string activityId;
            if (SubmitPageDisplayed)
            {
                activityId = SubmitId;
            }
            else
            {
                activityId = (Session.HasCurrentActivity ? FramesetUtil.GetStringInvariant(Session.CurrentActivityId) : "-1");
            }
            Response.Write(String.Format(CultureInfo.InvariantCulture, "frameMgr.SetActivityId({0});\r\n", 
                                            JScriptString.QuoteString(activityId, true)));

            // Write nav visibility, in case it's changed since the hidden frame was rendered
            if (SubmitPageDisplayed)
            {
                // If the submit page is being displayed, don't show UI elements
                Response.Write(String.Format(CultureInfo.CurrentCulture, "frameMgr.SetNavVisibility( {0}, {1}, {2}, {3}, {4});",
                ("false"),  // showNext
                ("false"),  // showPrevious
                ("false"),  // showAbandon
                ("false"),  // showExit
                (Session.ShowSave ? "true" : "false")));
                
                // If the submit page is now being displayed, make sure the frameset isn't waiting for another commit
                Response.Write("frameMgr.WaitForContentCompleted(0);\r\n");
            }
            else
            {
                Response.Write(String.Format(CultureInfo.CurrentCulture, "frameMgr.SetNavVisibility( {0}, {1}, {2}, {3}, {4});\r\n",
               (Session.ShowNext ? "true" : "false"),
               (Session.ShowPrevious ? "true" : "false"),
               (Session.ShowAbandon ? "true" : "false"),
               (Session.ShowExit ? "true" : "false"),
               (Session.ShowSave ? "true" : "false")));
            }  

            // Register that the frame loading is complete. This is required so as to notify the frameset of activity id, and 
            // other UI status.
            Response.Write(String.Format(CultureInfo.InvariantCulture, "frameMgr.RegisterFrameLoad({0});\r\n ",
                JScriptString.QuoteString("frameContent", false)));

            if (m_isPostedPage)
            {
                // Set PostIsComplete. THIS MUST BE THE LAST VALUE SET! 
                Response.Write("frameMgr.PostIsComplete();");
            }                 
        }

        /// <summary>
        /// Helper function to return true if the <paramref name="resPath"/> is the path to the primary 
        /// file of the resource associated with the current activity. There must be a current activity 
        /// in Session before calling this method.
        /// </summary>
        private bool IsPrimaryResource(string resPath)
        {
            // If the path is empty, this is a request for the primary resource.
            if (String.IsNullOrEmpty(resPath))
                return true;
            
            // If the path matches the resource xml:base attribute, then it's really an empty relative path, 
            // in which case this is a request for the primary resource.
            Uri resXmlBase = Session.CurrentActivityResourceXmlBase;
            Uri resourcePath = new Uri(resPath, UriKind.RelativeOrAbsolute);

            if ((resXmlBase != null) 
                && (Uri.Compare(resXmlBase, resourcePath, UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0))
                return true;
                
            // Alternately, if the path matches the entry point of the current activity, it
            // return true.
            if (Uri.Compare(Session.CurrentActivityEntryPoint, resourcePath, UriComponents.Path, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper function to return true if <paramref name="resPath"/> is the path to a primary 
        /// resource of an LRM activity *other than* the current activity. 
        /// </summary>
        /// <returns>True if this is a request for another activity's primary resource. If this is false, then
        /// pageId should be ignored.</returns>
        private bool IsPrimaryResourceOfLrmActivity(string resPath, out long pageId)
        {
            // Initialize out parameter
            pageId = -1;

            if (Session.PackageFormat != PackageFormat.Lrm)
                return false;

            Uri isRelative;
            try
            {
                isRelative = new Uri(resPath, UriKind.Relative);
                // If we got here, it's a relative uri               
            }
            catch (UriFormatException)
            {
                // It's an absolute uri, which by definition cannot be a primary resource url
                return false;
            }
            if (isRelative == null) // use the local variable to make fxcop happy
                return false;

            string simplePath = GetSimplePath(resPath);
      
            // simplePath may have several segments. Split on / and if there are more than 2 segments,
            // then it's not going to match the pattern
            string[] simplePathSegments = simplePath.Split(new char[] { '/' });
            if (simplePathSegments.Length != 2)
                return false;

            // Check if the second path segment matches "page.htm". 
            if (!CaseAndCultureInsensitiveCompare(simplePathSegments[1], "page.htm"))
                return false;

            // Check if the first path matches Pnnnn where nnnn is a number <= 1999999999
            if (!CaseAndCultureInsensitiveCompare(simplePathSegments[0].Substring(0, 1), "P"))
                return false;

            MatchCollection matches = Regex.Matches(simplePathSegments[0], @"^[P,p](\d{1,10})$");
            if ((matches.Count != 1) || (matches[0].Groups.Count != 2))
                return false;

            string match = matches[0].Groups[1].Value;
            pageId = long.Parse(match, NumberFormatInfo.InvariantInfo);

            return true;
        }

        /// <summary>
        /// Gets the simple path that represents a request with all relative references (./ or ../) removed, 
        /// slashes are converted to forward slashes, etc.  The result may not be a path in the package, or 
        /// even the actual request for the file, however, it is a path that can be used to determine if this 
        /// is a request for a primary resource in an LRM package.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetSimplePath(string path)
        {
            // replace backward slash with forward slash
            string retVal = path.Replace('\\', '/');

            // replace /./ with /
            // For example, P1111/./Page.htm becomes P1111/Page.htm
            retVal = retVal.Replace("/./", "/");

            // If there are no path separators, then we are done
            if (retVal.IndexOf('/') == -1)
                return retVal;

            // Now remove all ../ references. We are trying to see if the result 
            // is a path that ends with the pattern Pnnnn/Page.htm.

            // Separate the current string into segments of the path
            string[] retValSegments = retVal.Split(new char[] { '/' });

            // Keep a list of segments in the path that will be returned
            List<string> pathSegments = new List<string>(retValSegments.Length);

            foreach (string segment in retValSegments)
            {
                if (String.CompareOrdinal(segment, "..") == 0)
                {
                    // This is a relative reference, so remove the previous entry to 
                    // pathSegments, if it exists. If it doesn't exist, this is the first 
                    // entry in the path, in which case we do nothing
                    if (pathSegments.Count > 0)
                    {
                        pathSegments.RemoveRange(pathSegments.Count - 1, 1);
                    }
                }
                else
                {
                    // Add it to the list of valid segments
                    pathSegments.Add(segment);
                }
            }

            // Build the string from the segments that are left
            StringBuilder buffer = new StringBuilder(retVal.Length);
            bool isFirst = true;
            foreach (string segment in pathSegments)
            {
                if (!isFirst)
                {
                    buffer.AppendFormat("/{0}", segment);
                }
                else
                {
                    buffer.Append(segment);
                    isFirst = false;
                }
            }
            retVal = buffer.ToString();
            
            return retVal;
        }

        /// <summary>
        /// Returns true if the strings are equal when compared in any possible way.
        /// </summary>
        private static bool CaseAndCultureInsensitiveCompare(string str1, string str2)
        {
            if (String.CompareOrdinal(str1, str2) == 0)
                return true;

            // Case-insensitive, current culture
            if (String.Compare(str1, str2, StringComparison.CurrentCultureIgnoreCase) == 0)
                return true;

            // Case-sensitive, current culture
            if (String.Compare(str1, str2, StringComparison.CurrentCulture) == 0)
                return true;

            // Case-insensitive, ordinal culture
            if (String.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // catching all exceptions in order to return http response code
        private void RenderPackageContent(string resPath)
        {
            // If this is the primary resource of the current activity, then process it for rendering, including (in the case of 
            // LRM content) injecting script to notify client script of page load.
            if (IsPrimaryResource(resPath))
            {
                // If the resPath is empty or it matches the xmlBase value of the resource of the current activity, 
                // that means it's asking for the default resource associated with the current 
                // activity. Check if there is one. If there isn't, then show a message that indicates the content can't be shown 
                // because there's no resource. 
                if (Session.CurrentActivityResourceType == ResourceType.None)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundTitle),
                                       ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundMsg), false);
                    return;
                }

                // If the entry point for the current activity is an absolute Uri, then we should not be here (the 
                // hidden frame should have loaded the absolute Uri into the content frame).
                if (Session.CurrentActivityEntryPoint.IsAbsoluteUri)
                {
                    RegisterError(ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundTitle),
                                       ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundMsg), false);
                    return;
                }

                bool fileNotFound = false;  // Check if we got an exception indicating the file could not be found
                try
                {
                    string filePath = Session.CurrentActivityEntryPoint.OriginalString.Replace("%20", " ");
                    RenderContext context = new RenderContext(filePath, Response);

                    StringBuilder scriptBlock = new StringBuilder(1000);

                    // Initialize the hidden control and script for the page
                    InitHiddenControlInfo(context, scriptBlock);

                    // Allow application to modify the context. 
                    UpdateRenderContext(context, scriptBlock, Session);

                    FinishOnLoadScript(scriptBlock);

                    // Save script to context, if neeed
                    if (scriptBlock.Length > 0)
                        context.Script = scriptBlock.ToString();

                    // Clear any previous page information and render the page
                    Session.Render(context);

                    // When the primary resource is rendered, the session may be changed, so save any changes.
                    if (!SessionIsReadOnly)
                    {
                        Session.CommitChanges();
                    }
                }
                catch (FileNotFoundException)
                {
                    fileNotFound = true;
                }
                catch (DirectoryNotFoundException)
                {
                    fileNotFound = true;
                }
                if (fileNotFound)
                {
                    // The entry point defined in the package was a relative Uri but does not map to a file in the package.
                    RegisterError(ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundTitle),
                                       ResHelper.GetMessage(FramesetResources.CON_ResourceNotFoundMsg), false);
                    return;
                }
            }
            else
            {
                long pageId;
                if (IsPrimaryResourceOfLrmActivity(resPath, out pageId))
                {
                    // If this is LRM content and is a request for the primary resource of another activity, then 
                    // move to that activity and render a 'please wait'
                    // message that tells the frameset about the new activity.

                    ProcessNavigationRequests(Session);
                    
                    string activityId = "ITEM" + FramesetUtil.GetStringInvariant(pageId);
                    Session.MoveToActivity(activityId);
                
                    if (!SessionIsReadOnly)
                    {
                        Session.CommitChanges();
                    }

                    Response.Clear();
                    Response.Redirect(GetRedirectLocation());
                }
                else
                {
                    // There was a relative path provided. Write it to the output stream. If this method fails because the path was not 
                    // in the package or could not be accessed for some reason, return an error code.
                    try
                    {
                        RenderContext context = new RenderContext(resPath.Replace("%20", " "), Response);
                        UpdateRenderContext(context, null, Session);
                        Session.Render(context);

                        // Session is not changed in non-Execute views, so no need to call CommitChanges().
                    }
                    catch (ThreadAbortException)
                    {
                        // Do nothing
                    }
                    catch (FileNotFoundException)
                    {
                        Response.StatusCode = 404;
                        Response.StatusDescription = "Not Found";
                    }
                    catch (HttpException)
                    {
                        // Something wrong with the http connection, so in this case do not set the response
                        // headers.
                    }
                    catch
                    {
                        // This could fail for any number of reasons: invalid content that 
                        // refers to non-existant file, or a problem somewhere in the server 
                        // code. Return generic error code.
                        Response.StatusCode = 500;
                        Response.StatusDescription = "Internal Server Error";
                    }
                }
            }
        }

        /// <summary>
        /// Gets the string to use to redirect the current response to ChangeActivity page.
        /// </summary>
        /// <returns></returns>
        private string GetRedirectLocation()
        {
            string url = ResHelper.Format("./ChangeActivity.aspx?{0}={1}&{2}={3}&{4}={5}",
                            FramesetQueryParameter.View, FramesetUtil.GetString(Session.View),
                            FramesetQueryParameter.AttemptId, FramesetUtil.GetStringInvariant(Session.AttemptId.GetKey()),
                            FramesetQueryParameter.ActivityId, FramesetUtil.GetStringInvariant(Session.CurrentActivityId));
            UrlString urlStr = new UrlString(url);
            return urlStr.ToAscii();
        }

        /// <summary>
        /// Initialize information for the hidden controls and script. This sets up the information to create hidden fields in the form
        /// and to update the framesetMgr on page load.
        /// </summary>
        private void InitHiddenControlInfo(RenderContext context, StringBuilder onLoadScript)
        {
            // Should only do this if this is LRM content. Other content does not allow writing to the page.
            if (Session.CurrentActivityResourceType != ResourceType.Lrm)
                return;

            IDictionary<string, string> controls = context.FormHiddenControls;

            // Write the script to define frameMgr in script.
            WriteFindFrameMgr(onLoadScript);

            // If the session is attempt-based, then write attempt information
            if (Session != null)
            {
                controls.Add(HiddenFieldNames.AttemptId, FramesetUtil.GetStringInvariant(Session.AttemptId.GetKey()));
                onLoadScript.AppendFormat("frameMgr.SetAttemptId(document.getElementById({0}).value);\r\n", 
                    JScriptString.QuoteString(HiddenFieldNames.AttemptId, false));
            }

            // If the session has ended (that is, is suspended, completed or abandoned), then we're
            // done. Just return.
            if (SessionIsEnded)
            {
                return;
            }

            // Write view to display. 

            controls.Add(HiddenFieldNames.View, FramesetUtil.GetString(Session.View));
            onLoadScript.AppendFormat("frameMgr.SetView(document.getElementById({0}).value);\r\n",
                    JScriptString.QuoteString(HiddenFieldNames.View, false));

            // Write frame to post.
            controls.Add(HiddenFieldNames.PostFrame, "frameContent");
            onLoadScript.AppendFormat("frameMgr.SetPostFrame(document.getElementById({0}).value);\r\n",
                    JScriptString.QuoteString(HiddenFieldNames.PostFrame, false));

            // Set contentFrameUrl to be null. This prevents the content frame from being re-loaded.
            onLoadScript.Append("frameMgr.SetContentFrameUrl(null);\r\n");

            // If a new activity has been identified, then instruct frameMgr to reinitialize the RTE. 
            // BE CAREFUL to do this before setting any other data related to the rte! 
            if (ActivityHasChanged)
            {
                string initNewActivity = "false";
                if (Session.HasCurrentActivity)
                {
                    initNewActivity = (CurrentActivityRequiresRte ? "true" : "false");
                }
                onLoadScript.AppendFormat("frameMgr.InitNewActivity( {0} );\r\n", initNewActivity);
            }

            // Write the current activity Id. Write -1 if there isn't one.
            controls.Add(HiddenFieldNames.ActivityId, (Session.HasCurrentActivity ? FramesetUtil.GetStringInvariant(Session.CurrentActivityId) : "-1"));
            onLoadScript.AppendFormat("frameMgr.SetActivityId(document.getElementById({0}).value);\r\n",
                    JScriptString.QuoteString(HiddenFieldNames.ActivityId, false));
           
            // Write the navigation control state. Format of the control state is a series of T (to show) or F (to hide)
            // values, separated by semi-colons. The order of controls is: 
            // showNext, showPrevious, showAbandon, showExit, showSave
            StringBuilder sb = new StringBuilder(10);
            sb.Append((Session.ShowNext) ? "T" : "F");
            sb.Append(";");
            sb.Append((Session.ShowPrevious) ? "T" : "F");
            sb.Append(";");
            sb.Append((Session.ShowAbandon) ? "T" : "F");
            sb.Append(";");
            sb.Append((Session.ShowExit) ? "T" : "F");
            sb.Append(";");
            sb.Append((Session.ShowSave) ? "T" : "F");
            sb.Append(";");
            onLoadScript.AppendFormat("frameMgr.SetNavVisibility( {0}, {1}, {2}, {3}, {4});\r\n",
                (Session.ShowNext ? "true" : "false"),
                (Session.ShowPrevious ? "true" : "false"),
                (Session.ShowAbandon ? "true" : "false"),
                (Session.ShowExit ? "true" : "false"),
                (Session.ShowSave ? "true" : "false"));
            controls.Add(HiddenFieldNames.ShowUI, sb.ToString());

            context.Script = onLoadScript.ToString();
        }

        // Register with framemanager that loading is complete. This needs to be the final script in the buffer.
        private void FinishOnLoadScript(StringBuilder scriptBlock)
        {
            // Don't do the script block if the content is not LRM.
            if (Session.CurrentActivityResourceType != ResourceType.Lrm)
                return;

            // Add the form to post. Need to do it here because we have just allowed the application to set the form
            // name.
            scriptBlock.Append("frameMgr.SetPostableForm(document.forms[0]);\r\n");

            // Register that the frame loading is complete. This is required so as to notify the frameset of activity id, and 
            // other UI status.
            scriptBlock.AppendFormat("frameMgr.RegisterFrameLoad({0});\r\n ",
                JScriptString.QuoteString("frameContent", false));

            if (m_isPostedPage)
            {
                // Set PostIsComplete. THIS MUST BE THE LAST VALUE SET! 
                scriptBlock.AppendLine("frameMgr.PostIsComplete();");
            }         
        }

        /// <summary>
        /// Write the initialization code to allow script in content to access the 
        /// frameset manager. This initialization code will be run in the onload handler in LRM content.
        /// This method should not be called if the current activity ResourceType is not LRM.
        /// </summary>
        private static void WriteFindFrameMgr(StringBuilder onLoadScript)
        {
            int tabLevel = 0;
            onLoadScript.Append("function FindFrmMgr(win) {\r\n");
            WriteTab(++tabLevel, onLoadScript);
            onLoadScript.Append("var frmDepthCount = 0;\r\n");
            WriteTab(tabLevel, onLoadScript);
            onLoadScript.Append("while ((win.g_frameMgr == null) && (win.parent != null) && (win.parent != win))\r\n");
            WriteTab(tabLevel, onLoadScript);
            onLoadScript.Append("{\r\n");
            WriteTab(++tabLevel, onLoadScript);
            onLoadScript.Append("frmDepthCount++;    \r\n");
            WriteTab(tabLevel, onLoadScript);
            onLoadScript.Append("if (frmDepthCount > 20) \r\n");
            WriteTab(tabLevel, onLoadScript);
            onLoadScript.Append("{\r\n");
            WriteTab(++tabLevel, onLoadScript);
            onLoadScript.Append("return null;\r\n");
            WriteTab(--tabLevel, onLoadScript);
            onLoadScript.Append("}\r\n");
            WriteTab(tabLevel, onLoadScript);
            onLoadScript.Append("win = win.parent;\r\n");
            WriteTab(--tabLevel, onLoadScript);
            onLoadScript.Append("}\r\n");
            WriteTab(--tabLevel, onLoadScript);
            onLoadScript.Append("return win.g_frameMgr;\r\n");
            onLoadScript.Append("}\r\n\r\n");
            onLoadScript.Append("// Recurse up the frame hierarchy to find the FramesetManager object.\r\n");
            onLoadScript.Append("function API_GetFramesetManager()\r\n");
            onLoadScript.Append("{\r\n");
            WriteTab(1, onLoadScript);
            onLoadScript.Append("return FindFrmMgr(window);\r\n");
            onLoadScript.Append("}\r\n\r\n");
            onLoadScript.Append("var frameMgr = API_GetFramesetManager();\r\n");
        }

        private static void WriteTab(int level, StringBuilder buffer)
        {
            for (int i=0; i<level; i++)
                buffer.Append("\t");
        }
        /// <summary>
        /// Write the 'please wait' page to the response. This clears anything else in the response.
        /// </summary>
        private static void WritePleaseWaitToResponse(HttpResponse response)
        {
            response.Clear();
            response.ContentType = "text/html";

            response.Write("<html><head>");
            response.Write("<LINK rel=\"stylesheet\" type=\"text/css\" href=\"./Theme/Styles.css\"/></head>");
            response.Write("<body class=\"ErrorBody\">");

            response.Write("<table border=\"0\" width=\"100%\" id=\"table1\" style=\"border-collapse: collapse\">");
            response.Write("<tr><td class=\"ErrorTitle\">&nbsp;</td></tr>");
            response.Write("<tr><td align=\"center\">");
            response.Write(ResHelper.GetMessage(FramesetResources.CON_PleaseWait));
            response.Write("</td></tr>");
            response.Write("<tr><td class=\"ErrorMessage\">&nbsp;</td></tr>");
            response.Write("</table>");

            response.Write("</body>");
            response.Write("</html>");
        }
    }

}
