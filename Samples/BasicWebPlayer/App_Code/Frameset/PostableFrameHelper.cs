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
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.Text;
using Resources;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// The base class for frameset helper classes that support the postable frames (content and hidden)
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")] // postable == a frame that can be posted
    public class PostableFrameHelper
    {
#pragma warning disable 1591
        private StoredLearningSession m_session;

        private bool m_activityHasChanged;
        private Uri m_embeddedUIPath;    // path to the folder containing subfolders 'Theme' and 'Images'
        protected Uri EmbeddedUIPath
        {
            get { return m_embeddedUIPath; }
        }

        private RegisterError m_registerError;
        protected RegisterError RegisterError
        { 
            get { return m_registerError; }
            set { m_registerError = value; }
        }

        private GetErrorInfo m_errorInfo;
        protected GetErrorInfo GetErrorInfo 
        { 
            get { return m_errorInfo; }
            set { m_errorInfo = value; }
        }

        private HttpRequest m_request;
        protected HttpRequest Request { get { return m_request; } }

        private HttpResponse m_response;
        protected HttpResponse Response { get { return m_response; } }

        private AppendContentFrameDetails m_contentFrameDetails;
        protected AppendContentFrameDetails AppendContentFrameDetails 
        { 
            get { return m_contentFrameDetails; }
            set { m_contentFrameDetails = value; }
        }


        private GetFramesetMsg m_getFramesetMsg;
        protected GetFramesetMsg GetFramesetMsg
        {
            get { return m_getFramesetMsg; }
            set { m_getFramesetMsg = value; }
        }

        protected const string SubmitId = "SUBMIT";

        public PostableFrameHelper(HttpRequest request, HttpResponse response, Uri embeddedUIPath)
        {
            m_request = request;
            m_response = response;
            m_embeddedUIPath = embeddedUIPath;
        }
#pragma warning restore 1591

        /// <summary>
        /// Gets and sets the current session
        /// </summary>
        public StoredLearningSession Session
        {
            get { return m_session; }
            set { m_session = value; }
        }

        /// <summary>
        /// Return true if the session data model cannot be written to
        /// </summary>
        protected bool SessionIsReadOnly { get { return (m_session.View == SessionView.Review); } }

        /// <summary>
        /// Return true if the current session is no longer active.
        /// </summary>
        protected bool SessionIsEnded
        {
            get
            {
                // If the session is not execute, the end state is set by the app.
                // In the case of Execute, look at attempt status to determine if it's finished.
                if (m_session.View != SessionView.Execute)
                    return m_sessionEnded;

                return (m_session.AttemptStatus != AttemptStatus.Active);
            }
            set
            {
                m_sessionEnded = value;
            }
        }
        private bool m_sessionEnded;    

        /// <summary>
        /// Returns true if the current activity in the session requires the client-side RTE.
        /// </summary>
        protected bool CurrentActivityRequiresRte
        {
            get
            {
                // If there is no current activity, the RTE is not required
                if (!m_session.HasCurrentActivity)
                    return false;

                // If this is Execute view and the current activity is not active, the RTE is not required.
                if ((m_session.View == SessionView.Execute) && (!m_session.CurrentActivityIsActive))
                    return false;

                return (m_session.CurrentActivityResourceType == ResourceType.Sco);
            }
        }

        /// <summary>
        /// Returns true if the <paramref name="activityId"/> is the current activity in the session
        /// and the attempt on the activity is active. 
        /// </summary>
        protected bool IsCurrentActiveActivity(long activityId)
        {
            // If there is no current activity, or the current activity is not active, then return false
            if ((!m_session.HasCurrentActivity)
                || (!m_session.CurrentActivityIsActive))
                return false;

            return (m_session.CurrentActivityId == activityId);
        }

        /// <summary>
        /// Returns true if the current activity has changed on this page load. This will cause the RteSite object to reinitialize the 
        /// data model on the client (if the client requires the rte).
        /// </summary>
        protected bool ActivityHasChanged
        {
            get { return m_activityHasChanged; }
            set { m_activityHasChanged = value; }
        }

        /// <summary>
        /// Moves the session to the next activity with a resource. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        internal protected static void MoveToNextActivity(LearningSession session)
        {
            session.MoveToNext();
            
            // If this is RA view, make sure the activity has a resource
            if (session.View == SessionView.RandomAccess)
            {
                while (session.CurrentActivityResourceType == ResourceType.None)
                {
                    session.MoveToNext();
                }
            }
        }

        /// <summary>
        /// ProcessesNavigationRequests and returns true if the current activity has changed. Unlike the 
        /// Session.ProcessNavigationRequests, this method may be called in any session view.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        protected static bool ProcessNavigationRequests(LearningSession session)
        {
            if (session.View != SessionView.Execute)
                return false;

            // View is execute, so call the method
            return session.ProcessNavigationRequests();
        }

        /// <summary>
        /// Moves the session to the previous activity with a resource. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        protected static void MoveToPreviousActivity(LearningSession session)
        {
            session.MoveToPrevious();

            // If this is RA view, make sure the activity has a resource
            if (session.View == SessionView.RandomAccess)
            {
                while (session.CurrentActivityResourceType == ResourceType.None)
                {
                    session.MoveToPrevious();
                }
            }
        }

        /// <summary>
        /// Moves to requested activity. In RA view, if that activity does not have a resource, it 
        /// moves to the first child that does.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        protected static void MoveToActivity(LearningSession session, long activityId)
        {
            FramesetUtil.ValidateNonNullParameter("session", session);
            session.MoveToActivity(activityId);

            // If this is random access view, make sure to move to an activity that has a resource.
            if ((session.View == SessionView.RandomAccess) 
                        && (session.CurrentActivityResourceType == ResourceType.None))
            {
                MoveToNextActivity(session);
            }
        }

        /// <summary>
        /// Uses the current session to determine the Url to the content frame. The current activity's 
        /// resource may be relative or absolute.
        /// </summary>
        /// <returns>The url, in Ascii format, for the content frame.</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings"),    // the url is needed in string form
        SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]    // this does too much work to be property
        protected string GetContentFrameUrl()
        {
            StringBuilder sb = new StringBuilder(4096);

            Uri entryPoint = Session.CurrentActivityEntryPoint;

            if (entryPoint.IsAbsoluteUri)
            {
                sb.Append(new UrlString(entryPoint).ToAscii());
            }
            else
            {
                // In this case, the resource file is in the package, so just load the content frame referencing the attempt
                // and view information, as appropriate.

                // The URL for attempt-based content frames in BWP is:
                // http://<...basicWebApp>/Content.aspx/<view>/<attemptId>/otherdata/
                // the otherdata depends on the view
                sb.Append("Content.aspx");
                AppendContentFrameDetails(Session, sb);
                sb.Append("/");

                sb.Append(Session.CurrentActivityEntryPoint.OriginalString);

                // If there were query parameters, append them
                string parameters = Session.CurrentActivityResourceParameters;
                if (!String.IsNullOrEmpty(parameters))
                {
                    // parameters is defined in SCORM as already being encoded
                    if (!parameters.StartsWith("?", StringComparison.Ordinal))
                        sb.Append("?");
                    sb.Append(parameters);
                }
            }

            return sb.ToString();
        }  

#pragma warning disable 1591
        protected bool HasError
        {
            get
            {
                bool hasError;
                string errorTitle;
                string errorMsg;
                bool asInfo;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);
                return hasError;
            }
        }

        protected string ErrorMessage
        {
            get
            {
                bool hasError;
                string errorTitle;
                string errorMsg;
                bool asInfo;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);
                return errorMsg;
            }
        }

        protected string ErrorTitle
        {
            get
            {
                bool hasError;
                string errorTitle;
                string errorMsg;
                bool asInfo;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);
                return errorTitle;
            }
        }

        protected bool ErrorAsInfo
        {
            get
            {
                bool hasError;
                string errorTitle;
                string errorMsg;
                bool asInfo;
                GetErrorInfo(out hasError, out errorTitle, out errorMsg, out asInfo);
                return asInfo;
            }
        }
#pragma warning restore 1591

        /// <summary>
        /// If there was an error condition, write the error message to the hidden control so that it can
        /// be displayed to the user. This method does not allow reloading the current content frame.
        /// </summary>
        /// <param name="message"></param>
        protected void WriteError(PlainTextString message)
        {
            WriteError(message, false);
        }

        /// <summary>
        /// If there was an error condition, write the error message to the hidden control so that it can
        /// be displayed to the user.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        /// <param name="allowReloadCurrentActivity">If true, a link will appear with the message to reload the 
        /// current activity.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        protected void WriteError(PlainTextString message, bool allowReloadCurrentActivity)
        {
            FramesetUtil.ValidateNonNullParameter("message", message);
            string msgToDisplay;

            // If the message will contain a link to reload the current activity, then add the link to have the 
            // framesetMgr load the activity. We have to do this (rather than a get request) since we need to 
            // re-initialize the RTE object at this point.
            if (allowReloadCurrentActivity)
            {
                JScriptString js = new JScriptString(ResHelper.FormatInvariant("API_GetFramesetManager().DoChoice(\"{0}\");", 
                                    FramesetUtil.GetStringInvariant(m_session.CurrentActivityId)));
                string origMessage = message.ToString();
                StringBuilder sb = new StringBuilder(origMessage.Length * 2);
                sb.Append(ResHelper.Format("{0}<br><br><a href='{1}' >{2}</a>",origMessage, js.ToJavascriptProtocol(), FramesetResources.HID_ReloadCurrentContent));

                msgToDisplay = sb.ToString();
            }
            else
            {
                msgToDisplay = message.ToString();
            }
            RegisterError(FramesetResources.HID_ServerErrorTitle, msgToDisplay, false);
        }

        /// <summary>
        /// If there is a request to show the 'submit' page, write the message to the hidden control so that it can
        /// be displayed to the user. If there is no current activity, the Cancel button is not shown.
        /// </summary>
        /// <param name="messageHtml">The message to display.</param>
        /// <param name="titleHtml">The title of the message to display.</param>
        /// <param name="saveButtonHtml">The button text to display on the submit button.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        protected void WriteSubmitPage(string titleHtml, string messageHtml, string saveButtonHtml)
        {
            FramesetUtil.ValidateNonNullParameter("messageHtml", messageHtml);
            string msgToDisplay;
            bool hasCurrentActivity = Session.HasCurrentActivity;

            // The message will contain a link to submit the attempt.
            StringBuilder sb = new StringBuilder(messageHtml.Length * 2 + 500);
            sb.AppendLine("<script>");
            
            // If there is a current activity to return to, allow the user to do that.
            if (hasCurrentActivity)
            {
                sb.AppendLine("function onCancel()");
                sb.AppendLine("{");
                sb.AppendLine(" var frameMgr = API_GetFramesetManager();");
                sb.AppendLine(" if (frameMgr.ReadyForNavigation())");
                sb.AppendLine(" {");
                sb.AppendLine(ResHelper.Format("     frameMgr.DoChoice(\"{0}\");", FramesetUtil.GetStringInvariant(m_session.CurrentActivityId)));
                sb.AppendLine(" }");
                sb.AppendLine(" event.cancelBubble = true;");
                sb.AppendLine(" event.returnValue = false;");
                sb.AppendLine("}");
            }

            sb.AppendLine("function onSubmit()");
            sb.AppendLine("{");
            sb.AppendLine(" var frameMgr = API_GetFramesetManager();");
            sb.AppendLine(" if (frameMgr.ReadyForNavigation())");
            sb.AppendLine(" {");
            sb.AppendLine("     frameMgr.DoSubmit();");
            sb.AppendLine(" }");
            sb.AppendLine(" event.cancelBubble = true;");
            sb.AppendLine(" event.returnValue = false;");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            sb.AppendLine(ResHelper.Format("{0}<br><br><input type='button' value='{1}' id='submitBtn' onClick='onSubmit()'/>", messageHtml, saveButtonHtml));

            if (hasCurrentActivity)
            {
                sb.AppendLine(ResHelper.Format("&nbsp;&nbsp;<input type='button' value='{0}' id='cancelBtn' onClick='onCancel()'/>", FramesetResources.POST_ContinueHtml));
            }

            msgToDisplay = sb.ToString();
            RegisterError(titleHtml, msgToDisplay, true);

            SubmitPageDisplayed = true;
        }

        private bool m_submitPageDisplayed;
        /// <summary>True if the submit page is displayed.</summary>
        protected bool SubmitPageDisplayed
        {
            get { return m_submitPageDisplayed; }
            private set { m_submitPageDisplayed = value; }
        }

        /// <summary>
        /// Gets the path to the theme folder for URI references. Returns the string that 
        /// can be used as the url to the theme folder. The strings ends with a trailing slash.
        /// </summary>
        protected string ThemeFolderPath
        {
            get
            {
                Uri themeFolder = new Uri(m_embeddedUIPath, new Uri("Theme", UriKind.Relative));
                return themeFolder.OriginalString;
            }
        }
        /// <summary>
        /// If the current activity is not active, then try moving to it. SequencingException is not
        /// thrown from this method, since if it didn't work there is nothing the caller can do to 
        /// fix the situation.
        /// </summary>
        protected void ActivateCurrentActivity()
        {
            try
            {
                if (m_session.HasCurrentActivity && (!m_session.CurrentActivityIsActive))
                {
                    m_session.MoveToActivity(m_session.CurrentActivityId);
                }
            }
            catch (SequencingException)
            {
            }
        }

        /// <summary>Gets the commands for the frame.</summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // this method does too much work to be a property
        protected ReadOnlyCollection<CommandInfo> GetCommands()
        {
            string postedCommand = Request.Form[HiddenFieldNames.Command];
            string postedCommandData = Request.Form[HiddenFieldNames.CommandData];
            
            // If there is no posted command, then return an empty collection. This may happen when the content (for instance LRM)
            // posts itself.
            if (String.IsNullOrEmpty(postedCommand))
            {
                return new ReadOnlyCollection<CommandInfo>(new List<CommandInfo>());
            }

            // Commands are semi-colon separated. 
            string[] commands = postedCommand.Split(new char[] { ';' });
            int numCommands = commands.Length;
            string[] commandData = null;
            
            List<CommandInfo> commandList = new List<CommandInfo>(numCommands);

            // Command data are separated by @C, and encoded
            if (!String.IsNullOrEmpty(postedCommandData))
            {
                postedCommandData = postedCommandData.Replace("@G", ">").Replace("@L", "<").Replace("@A", "@");
                commandData = postedCommandData.Split(new string[] { "@C" }, StringSplitOptions.None);
            }

            int i = 0;
            foreach(string cmd in commands)
            {
                if (String.IsNullOrEmpty(cmd))
                {
                    i++;
                    continue;
                }
               
                commandList.Add(new CommandInfo(cmd, commandData[i]));
                i++;
            }

            return new ReadOnlyCollection<CommandInfo>(commandList);
        }

        /// <summary>
        /// Returns the string to represent the state of the table of contents nodes. The format of the string
        ///  strActivityId,true;strActivityId,false;
        ///  where strActivityId is the numeric activity id
        ///      if (false) the node should be disabled
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // this does too much work to be a property
        protected string GetTocStates()
        {
            StringBuilder sb = new StringBuilder(1000);
            TableOfContentsElement root = Session.GetTableOfContents(true);
            
            GetTocNodeState(sb, root);

            return sb.ToString();
        }

        /// <summary>
        /// Recursive function to append toc state to the buffer.
        /// </summary>
        private void GetTocNodeState(StringBuilder buffer, TableOfContentsElement element)
        {
            if (element.IsVisible)
            {
                buffer.AppendFormat("{0},{1};", XmlConvert.ToString(element.ActivityId), element.IsValidChoiceNavigationDestination ? "true" : "false");
            }
            
            foreach (TableOfContentsElement child in element.Children)
            {
                GetTocNodeState(buffer, child);
            }
        }
    }

    /// <summary>
    /// Represents commands and associated data, posted to the server.
    /// </summary>
    public class CommandInfo
    {
        private string m_data;
        private string m_command;

        /// <summary>Initializes a new instance of <see cref="CommandInfo"/>.</summary>
        /// <param name="command">The command name.</param>
        public CommandInfo(string command)
        {
            m_command = command;
        }

        /// <summary>Initializes a new instance of <see cref="CommandInfo"/>.</summary>
        /// <param name="command">The command name.</param>
        /// <param name="data">The command data.</param>
        public CommandInfo(string command, string data)
        {
            m_command = command;
            m_data = data;
        }

        /// <summary>The name of the command.</summary>
        public string Command { get { return m_command; } }

        /// <summary>The data of the command.</summary>
        public string CommandData { get { return m_data; } }
    }
}
