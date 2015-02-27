/* Copyright (c) Microsoft Corporation. All rights reserved. */

// AssignmentListWebPart.cs
//
// Implements the SLK Assignment List Web Part.
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Security;
using System.Text;
using System.Threading;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System.Xml.Serialization;
using Microsoft.SharePoint.WebPartPages.Communication;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit.ApplicationPages;
using Microsoft.SharePointLearningKit.WebControls;
using Microsoft.SharePointLearningKit;
using Resources.Properties;



namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>
    /// The primary access to assignments for learners and instructors in SLK. 
    /// Allows instructors and learners to view a list of their  
    /// SharePoint Learning Kit (SLK) assignments.
    /// </summary>
#pragma warning disable 618
    public class AssignmentListWebPart : Microsoft.SharePoint.WebPartPages.WebPart, ICellConsumer
    {
        #region WebPartCommunication

        bool forObserver;
        private int _cellConnectedCount=0;
        /// <summary>The event raised then the cell is consumed.</summary>
        public event CellConsumerInitEventHandler CellConsumerInit;

        /// <summary>See <see cref="Microsoft.SharePoint.WebPartPages.WebPart.GetInitEventArgs"/>.</summary>
        [Obsolete]
        public override InitEventArgs GetInitEventArgs(string interfaceName)
        {
            return null;
            /*
            // Check if this is my particular cell interface.
            if (interfaceName == "MyCellConsumerInterface" || interfaceName == "Observer_WebPart_Listener")
            {
                // Create the object that will return the initialization arguments.
                CellConsumerInitEventArgs cellConsumerInitArgs = new CellConsumerInitEventArgs();

                // Set the FieldName and FieldDisplay name values.
                cellConsumerInitArgs.FieldName = "AccountName";
                cellConsumerInitArgs.FieldDisplayName = "Account Name";

                // Return the CellConsumerInitEventArgs object.
                return(cellConsumerInitArgs);
            }
            else
            {
                return(null);
            }
            */
        }

        /// <summary>See <see cref="Microsoft.SharePoint.WebPartPages.WebPart.EnsureInterfaces"/>.</summary>
        [Obsolete]
        public override void EnsureInterfaces()
        {
            //Registers an interface for the Web Part.
            RegisterInterface("Observer_WebPart_Listener",   //InterfaceName
               InterfaceTypes.ICellConsumer,               //InterfaceType
               Microsoft.SharePoint.WebPartPages.WebPart.UnlimitedConnections,               //MaxConnections
               ConnectionRunAt.Server,            //RunAtOptions
               this,                              //InterfaceObject
               "CellConsumerInterface_WPQ_",      //InterfaceClientReference
               culture.Resources.ObserverRoleCommunicationInterfaceTitle,                       //MenuLabel
               "Learner Information Receiving Interface");               //Description
        }

        /// <summary>See <see cref="Microsoft.SharePoint.WebPartPages.WebPart.CanRunAt"/>.</summary>
        [Obsolete]
        public override ConnectionRunAt CanRunAt()
        {
            //This Web Part can run only on the server
            return ConnectionRunAt.Server;
        }
    
        /// <summary>See <see cref="Microsoft.SharePoint.WebPartPages.WebPart.PartCommunicationConnect"/>.</summary>
        [Obsolete]
        public override void PartCommunicationConnect(string interfaceName,
         Microsoft.SharePoint.WebPartPages.WebPart connectedPart,
         string connectedInterfaceName,
         ConnectionRunAt runAt)
      {
          //Check if the connect is for this particular cell interface
          if (interfaceName == "Observer_WebPart_Listener")
          {
              //Keep a count of the connections
              _cellConnectedCount++;
          }

      }

        /// <summary>See <see cref="Microsoft.SharePoint.WebPartPages.WebPart.PartCommunicationInit"/>.</summary>
        [Obsolete]
        public override void PartCommunicationInit()
        {
            //Initialize the learner id
            observerRoleLearnerLogin = "";

            InitializeLearnerKey();
            //If the connection wasn't actually formed then don't want to send Init event
            if (_cellConnectedCount > 0)
            {
                //If there is a listener, send init event
                if (CellConsumerInit != null)
                {
                    //Need to create the args for the CellConsumerInit event
                    CellConsumerInitEventArgs cellConsumerInitArgs = new CellConsumerInitEventArgs();

                    //Fire the CellConsumerInit event.
                    CellConsumerInit(this, cellConsumerInitArgs);
                }
            }
        }

        /// <summary>Initialises the cell provider.</summary>
        public void CellProviderInit(object sender, CellProviderInitEventArgs cellProviderInitArgs)
        {
            //Callback on the Cell Provider's initialization
            observerRoleLearnerLogin = "";

            InitializeLearnerKey();
        }

        /// <summary>Raised when the cell is ready.</summary>
        public void CellReady(object sender, CellReadyEventArgs cellReadyArgs)
        {
            observerRoleLearnerLogin = "";

            // On CellReady, validate and set the learner's login id
            InitializeLearnerKey();
            if (cellReadyArgs.Cell != null)
            {
                observerRoleLearnerLogin = cellReadyArgs.Cell.ToString();
            }
        }
        #endregion

        #region Private Variables
        SlkCulture culture;
        SlkCulture invariantCulture = new SlkCulture(CultureInfo.InvariantCulture);
        string frameId = "Frame" + Guid.NewGuid().ToString().Replace("-", "");
        /// <summary>
        /// Holds List Scope - "Only Show Assignments for this Site" web part property
        /// </summary>
        private bool listScope;
        /// <summary>
        /// Holds Display Summary - "Show Summary" web part property
        /// </summary>
        private bool displaySummary;
        /// <summary>
        /// Holds Summary Width
        /// </summary>
        private Unit summaryWidth;
        /// <summary>
        /// Holds query Set Override
        /// </summary>
        private string querySetOverride;
        /// <summary>
        /// Stores the default Query Name - comes from the SLK Settings file
        /// </summary>
        private string defaultQueryName;
        /// <summary>
        /// Stores Current SPWeb
        /// </summary>
        private SPWeb spWeb;
        /// <summary>
        /// Stores the SlkStore
        /// </summary>
        private SlkStore slkStore;
        ///<summary>
        /// Stores the input learner login in the case of the observer mode
        ///</summary>
        private string observerRoleLearnerLogin;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AssignmentListWebPart()
        {
            culture = new SlkCulture();
            displaySummary = true;
            listScope = true;
            ToolTip = culture.Format(culture.Resources.AlwpWepPartToolTipFormat, this.Title, this.Description);
        }
        #endregion

        #region Public Properties

        /// <summary>See <see cref="WebPart.Description"/>.</summary>
        public override string Description
        {
            get
            {
                // Localise the description if empty or it is the default value
                if (string.IsNullOrEmpty(base.Description) || base.Description == GetLocalizedString("AlwpWepPartDescription"))
                {
                    return culture.Resources.AlwpWepPartDescription;
                }
                else
                {
                    return base.Description;
                }
            }
            set { base.Description = value ;}
        }

        /// <summary>See <see cref="WebPart.Title"/>.</summary>
        public override string Title
        {
            get
            {
                // Localise the description if empty or it is the default value
                if (string.IsNullOrEmpty(base.Title) || base.Title == GetLocalizedString("AlwpWepPartTitle"))
                {
                    return culture.Resources.AlwpWepPartTitle;
                }
                else
                {
                    return base.Title;
                }
            }
            set { base.Title = value ;}
        }

        private string GetLocalizedString(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return string.Empty;
            }
            else
            {
                string resourceFile = "SLK";
                int lcid = culture.Culture.LCID;
                return Microsoft.SharePoint.Utilities.SPUtility.GetLocalizedString("$Resources:" + resourceName, resourceFile, (uint)lcid);
            }
        }

        /// <summary>
        /// Sets the assignment list to display assignments from this site or 
        /// all sites using the same SLK database.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpListScopeDisplayName"),
         AlwpWebDescription("AlwpListScopeDescription")]
        public bool ListScope
        {
            get { return listScope; }
            set { listScope = value; }
        }

        /// <summary>
        /// Shows or hides the query titles and summary counts with the assignment list.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpDisplaySummaryDisplayName"),
         AlwpWebDescription("AlwpDisplaySummaryDescription")]
        public bool DisplaySummary
        {
            get { return displaySummary; }
            set { displaySummary = value; }
        }

        /// <summary>
        /// Set the width of the summary frame when it is displayed.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpSummaryWidthDisplayName"),
         AlwpWebDescription("AlwpSummaryWidthDescription")]
        public Unit SummaryWidth
        {
            get
            {
                if (summaryWidth == null || summaryWidth.IsEmpty)
                {
                    summaryWidth = new Unit(Constants.SummaryFrameWidth, UnitType.Pixel);
                }
                return summaryWidth;
            }
            set 
            {
                if (value.IsEmpty || value.Value > 0)
                {
                    summaryWidth = value;
                }
                else
                {
                    //Throw an ArgumentOutOfRangeException for negative values 
                    throw new ArgumentOutOfRangeException(culture.Resources.AlwpSummaryWidthDisplayName);
                }
            }
        }

        /// <summary>
        /// Specify an override query set.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpQuerySetOverrideDisplayName"),
         AlwpWebDescription("AlwpQuerySetOverrideDescription")]
        public string QuerySetOverride
        {
            get { return querySetOverride; }
            set { querySetOverride = SlkUtilities.GetHtmlEncodedText(value); }
        }

        /// <summary>
        /// Gets a reference to the WebPart control to enable it to be edited by custom EditorPart controls.
        /// </summary>
        public override object WebBrowsableObject
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the current <c>SPWeb</c>.
        /// </summary>
        public SPWeb SPWeb
        {
            get
            {
                //Gets the Current SPWeb Object               
                if (spWeb == null)
                    spWeb = SlkUtilities.GetCurrentSPWeb();
                return spWeb;
            }
        }

        /// <summary>
        /// Gets the current <c>SlkStore</c>.
        /// </summary>
        public SlkStore SlkStore
        {
            get
            {
                if (slkStore == null)
                {
                    slkStore = SlkStore.GetStore(SPWeb);
                }

                return slkStore;
            }
        }

        #endregion

        #region Private and Protected Methods

        #region ValidateAlwpProperties
        /// <summary>
        /// Validates the required attributes to render AssignmentListWebPart 
        /// Throws SafeToDisplayException If not Valid.
        /// </summary>
        private void ValidateAlwpProperties()
        {

            //Check for Current User and QuerySet Values              

            if (SPWeb.CurrentUser == null)
            {
                throw new UserNotFoundException(culture.Resources.SlkExUserNotFound);
            }

            if (String.IsNullOrEmpty(SlkUtilities.Trim(QuerySetOverride)) == false)
            {
                // set <querySetDef> to the QuerySetDefinition named <querySetName>

                QuerySetDefinition querySetDef = SlkStore.Settings.FindQuerySetDefinition(QuerySetOverride, true);
                if (querySetDef == null)
                {
                    throw new SafeToDisplayException (culture.Resources.AlwpQuerySetNotFound, QuerySetOverride);
                }
                else
                {
                    defaultQueryName = querySetDef.DefaultQueryName;
                }
            }


        }
        #endregion

        #region CreateEditorParts
        /// <summary>
        /// Creating Editor Parts for Custom Properties, i.e. the part of the tool pane
        /// labeled "Assignment List"
        /// </summary>                          
        /// <returns>EditorPartCollection</returns>       
        public override EditorPartCollection CreateEditorParts()
        {
            ArrayList editorArray = new ArrayList();
            //create the Default PropertyGridEditorPart 
            PropertyGridEditorPart edPart = new PropertyGridEditorPart();

            edPart.ID = this.ID + "_editorPart1";
            //Assign the Category for Title and 
            edPart.Title = culture.Resources.AlwpWepPartTitle;
            editorArray.Add(edPart);

            return new EditorPartCollection(editorArray);
        }

        #endregion

        /// <summary>Finds the scope for the query.</summary>
        protected virtual string FindScope()
        {
            if (ListScope)
            {
                return SPWeb.ID.ToString();
            }
            else
            {
                return "all";
            }
        }

        #region RenderContents
        /// <summary>
        /// Render Assignment List WebPart Contents
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param>  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void RenderContents(HtmlTextWriter writer)
        {
            try
            {
                //Render Assignment List webpart
                RenderAssignmentList(writer);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Not much we can do about this.
            }
            catch (SafeToDisplayException e)
            {
                SlkError slkError = new SlkError(ErrorType.Error, Constants.Space + e.Message);
                ErrorBanner.RenderErrorItems(writer, slkError);
            }
            catch (UserNotFoundException e)
            {
                SlkError slkError = new SlkError(ErrorType.Info, Constants.Space + e.Message);
                ErrorBanner.RenderErrorItems(writer, slkError);
            }
            catch (SqlException e)
            {
                ISlkStore store = null;
                try
                {
                    store = SlkStore;
                }
                catch (Exception)
                {
                }

                SlkError slkError = SlkError.WriteException(store, e);
                ErrorBanner.RenderErrorItems(writer, slkError);
            }
            catch (Exception ex)
            {
                ISlkStore store = null;
                try
                {
                    store = SlkStore;
                }
                catch (Exception)
                {
                }

                SlkError slkError = SlkError.WriteException(store, ex);
                ErrorBanner.RenderErrorItems(writer, slkError);
            }
        }
        #endregion

        #region RenderAssignmentList
        /// <summary>
        /// Get AssignmentListWebPart Properties and Renders Webpart
        /// </summary>            
        /// <param name="htmlTextWriter">HtmlTextWriter</param>                      
        private void RenderAssignmentList(HtmlTextWriter htmlTextWriter)
        {
            int cols = DisplaySummary ? 2 : 1;           

            //Set the QuerySetOverride Property
            DefaultQuerySetIfRequired();

            //Validates the Alwp Properties 
            ValidateAlwpProperties();

            string observerRoleLearnerKey = GetLearnerKey(observerRoleLearnerLogin);

            //Adjust the Height to Fit width of zone

            string wpHeightStyle = "height: {0};";
            string height = string.IsNullOrEmpty(Height) ? Constants.WebPartHeight : Height.ToString(CultureInfo.InvariantCulture);
            wpHeightStyle = string.Format(CultureInfo.InvariantCulture, wpHeightStyle, height);

            // create a unique name for the query results iframe based on
            // the web part's GUID. test this by having two ALWP web parts on the same page 
            //and ensuring that selecting a query in one doesn't affect the other and vice versa

            // write a comment to help locate this Web Part when viewing HTML source
            htmlTextWriter.Write("<!-- Begin ALWP -->");
            htmlTextWriter.Write("<script type=\"text/javascript\">var scope{0} = '{1}';</script>", frameId, FindScope());
            htmlTextWriter.Write("<table cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\" style=\"");
            htmlTextWriter.Write(wpHeightStyle);
            htmlTextWriter.Write("\">");

            htmlTextWriter.Write("<tr valign=\"top\">");

            StringBuilder queryString = BaseQueryString();
            string sourceParameter = HttpUtility.UrlEncode(Page.Request.Url.ToString());

            string queryResultsUrl;
            if (DisplaySummary)
            {
                queryString.AppendFormat("&{0}={1}&{2}={3}", QueryStringKeys.QuerySet, QuerySetOverride, QueryStringKeys.Source, sourceParameter);
                queryResultsUrl = SlkUtilities.UrlCombine(SPWeb.Url, Constants.BlankGifUrl);

                WriteSummary(htmlTextWriter, queryString.ToString());
            }
            else
            {
                queryString.AppendFormat("&{0}={1}", QueryStringKeys.Query, defaultQueryName);
                queryResultsUrl = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, Constants.SlkUrlPath, Constants.QueryResultPage);
                queryResultsUrl += queryString.ToString();
            }

            if (forObserver)
            {
                queryResultsUrl = string.Format(CultureInfo.InvariantCulture, "{0}&{1}=true", queryResultsUrl, QueryStringKeys.ForObserver);
            }

            if (queryResultsUrl.Contains(Constants.BlankGifUrl) == false)
            {
                queryResultsUrl = string.Format("{0}&{1}={2}", queryResultsUrl, QueryStringKeys.Source, sourceParameter);
            }

            WriteQueryResults(htmlTextWriter, queryResultsUrl);

            htmlTextWriter.Write("</tr>");

            // write a row containing a separator line
            htmlTextWriter.Write("<tr><td class=\"ms-partline\" colspan=\"2\"><img src=\"");
            htmlTextWriter.Write(Constants.BlankGifUrl);
            htmlTextWriter.Write("\" width=\"1\" height=\"1\" alt=\"\" /></td></tr>");
            // write a row that adds extra vertical space
            htmlTextWriter.Write("<tr><td class=\"ms-partline\" colspan=\"2\"><img src=\"");
            htmlTextWriter.Write(Constants.BlankGifUrl);
            htmlTextWriter.Write("\" width=\"1\" height=\"1\" alt=\"\" /></td></tr>");

            htmlTextWriter.Write("</table>");

            DumpCultures(htmlTextWriter);
            // write a comment to help locate this Web Part when viewing HTML source
            htmlTextWriter.Write("<!-- End ALWP -->");
            htmlTextWriter.WriteLine();
        }

        #endregion

        internal static void DumpCultures(HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.Write(@"<!-- 
            CultureInfo.InvariantCulture {0}
            CultureInfo.CurrentUICulture {1}
            CultureInfo.CurrentCulture {2}
                    -->", CultureInfo.InvariantCulture, CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture);
        }

        StringBuilder BaseQueryString()
        {
            StringBuilder url = new StringBuilder(1000);

            url.AppendFormat("?{0}={1}", QueryStringKeys.FrameId, frameId);

            if (forObserver)
            {
                url.AppendFormat("&{0}={1}", QueryStringKeys.ForObserver, "true");
            }

            return url;
        }

        void WriteQueryResults(HtmlTextWriter htmlTextWriter, string url)
        {
            htmlTextWriter.Write("<td class=\"iframe-overflow\" style=\"height:100%\">");
            WriteFrameEx(htmlTextWriter, url, String.Empty, true);
            htmlTextWriter.Write("</td>");
        }

        void WriteSummary(HtmlTextWriter htmlTextWriter, string urlQueryString)
        {
            htmlTextWriter.Write("<td height=\"100%\" style=\"height:100%; width :");
            htmlTextWriter.Write(SummaryWidth.ToString(CultureInfo.InvariantCulture));
            htmlTextWriter.Write("\">");

            // Get the ServerRelativeUrl for QueryResultPage 
            string urlString = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, Constants.SlkUrlPath, Constants.QuerySetPage);

            //Append the QueryString Values
            urlString += urlQueryString;
            WriteFrame(htmlTextWriter, urlString.ToString(), "_AlwpQuerySummary", false);

            htmlTextWriter.Write("</td>");
        }

        void WriteFrameEx(HtmlTextWriter htmlTextWriter, string url, string nameQualifier, bool fullWidth)
        {
            bool isIpad = HttpContext.Current.Request.UserAgent.ToUpperInvariant().Contains("IPAD");
            htmlTextWriter.Write("<iframe ");
            if (fullWidth)
            {
                if (isIpad == false)
                {
                    htmlTextWriter.Write("width=\"100%\" ");
                }
            }
            else
            {
                htmlTextWriter.Write(" style=\"width :");
                htmlTextWriter.Write(SummaryWidth.ToString(CultureInfo.InvariantCulture));
                htmlTextWriter.Write("\" ");
            }

            if (isIpad == false)
            {
                htmlTextWriter.Write(" height=\"100%\" ");
            }

            htmlTextWriter.Write(" frameborder=\"0\" src=\"");
            htmlTextWriter.Write(url);
            htmlTextWriter.Write("\" name=\"");
            htmlTextWriter.Write(frameId);
            htmlTextWriter.Write(nameQualifier);
            htmlTextWriter.Write("\"></iframe>");
        }

        void WriteFrame(HtmlTextWriter htmlTextWriter, string url, string nameQualifier, bool fullWidth)
        {
            htmlTextWriter.Write("<iframe ");
            if (fullWidth)
            {
                htmlTextWriter.Write("width=\"100%\" ");
            }
            else
            {
                htmlTextWriter.Write(" style=\"width :");
                htmlTextWriter.Write(SummaryWidth.ToString(CultureInfo.InvariantCulture));
                htmlTextWriter.Write("\" ");
            }
            htmlTextWriter.Write(" height=\"100%\" frameborder=\"0\" src=\"");
            htmlTextWriter.Write(url);
            htmlTextWriter.Write("\" name=\"");
            htmlTextWriter.Write(frameId);
            htmlTextWriter.Write(nameQualifier);
            htmlTextWriter.Write("\"></iframe>");
        }

        #region DefaultQuerySetIfRequired
        /// <summary>
        /// Check the Current User Role and Set the
        /// Default QuerySet accrodingly    
        /// </summary>
        private void DefaultQuerySetIfRequired()
        {
            //Override the QuerySet depends on the weppart property selection and role.

            if (String.IsNullOrEmpty(SlkUtilities.Trim(QuerySetOverride)))
            {

                //Defaluted to Default LearnerQuerySet For all, but instructor and observer.
                QuerySetOverride = Constants.DefaultLearnerQuerySet;               
               
                //// check for the role and assign the query set
                if (SlkStore.IsObserver(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultObserverQuerySet;
                }
                else if (SlkStore.IsInstructor(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultInstructorQuerySet;
                }
                else if (SlkStore.IsLearner(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultLearnerQuerySet;
                }
            }
        }

        #endregion       

        #region GetLearnerKey
        /// <summary>
        /// Get the learner key corresponding to the input learner login
        /// If the login is not valid, an error is reported on the page
        /// </summary>
        private string GetLearnerKey(string learnerLogin)
        {
            // If logged-in user is an observer, return the corresponding key, return empty string otherwise
            if (String.IsNullOrEmpty(learnerLogin) == false && SlkStore.IsObserver(SPWeb) == true)
            {
                SPUser inputSPUser;
                bool allowUnsafeUpdates = SPWeb.AllowUnsafeUpdates;
                try
                {
                    SPWeb.AllowUnsafeUpdates = true;
                    inputSPUser = SPWeb.EnsureUser(learnerLogin);
                }
                catch (SPException)
                {
                    // Try again with claims based login name
                    try
                    {
                        inputSPUser = SPWeb.EnsureUser("i:0#.w|" + learnerLogin);
                    }
                    catch (SPException e)
                    {
                        throw new UserNotFoundException(e.Message);
                    }
                }
                finally
                {
                    SPWeb.AllowUnsafeUpdates = allowUnsafeUpdates;
                }

                SlkUser slkUser = new SlkUser(inputSPUser);
                string observerRoleLearnerKey = slkUser.Key;
                try
                {
                    // Set the obtained LearnerKey as a session variable available across other pages
                    Page.Session["LearnerKey"] = observerRoleLearnerKey;
                    forObserver = true;
                }
                catch (HttpException)
                {
                    throw new SafeToDisplayException(culture.Resources.SessionNotConfigured);
                }
                return observerRoleLearnerKey;
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion

        ///<summary>Initializes the LearnerKey session variable to a blank string.</summary>
        void InitializeLearnerKey()
        {
            try
            {
                Page.Session["LearnerKey"] = String.Empty;
            }
            catch (HttpException)
            {
                // Session state is not turned on
            }
        }

        #endregion

    }

}
