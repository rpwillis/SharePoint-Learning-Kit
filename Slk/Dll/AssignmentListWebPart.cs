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
    public class AssignmentListWebPart : Microsoft.SharePoint.WebPartPages.WebPart, ICellConsumer
    {
        #region WebPartCommunication

        private int _cellConnectedCount=0;
        public event CellConsumerInitEventHandler CellConsumerInit;

        public override void EnsureInterfaces()
        {
            //Registers an interface for the Web Part.
            RegisterInterface("Observer_WebPart_Listener",   //InterfaceName
               InterfaceTypes.ICellConsumer,               //InterfaceType
               Microsoft.SharePoint.WebPartPages.WebPart.UnlimitedConnections,               //MaxConnections
               ConnectionRunAt.Server,            //RunAtOptions
               this,                              //InterfaceObject
               "CellConsumerInterface_WPQ_",      //InterfaceClientReference
               AppResources.ObserverRoleCommunicationInterfaceTitle,                       //MenuLabel
               "Learner Information Receiving Interface");               //Description
        }
        public override ConnectionRunAt CanRunAt()
        {
            //This Web Part can run only on the server
            return ConnectionRunAt.Server;
        }
    
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
        public override void PartCommunicationInit()
        {
            //Initialize the learner id
            m_observerRoleLearnerLogin = "";

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

        public void CellProviderInit(object sender, CellProviderInitEventArgs cellProviderInitArgs)
        {
            //Callback on the Cell Provider's initialization
            m_observerRoleLearnerLogin = "";

            InitializeLearnerKey();
        }
        public void CellReady(object sender, CellReadyEventArgs cellReadyArgs)
        {
            m_observerRoleLearnerLogin = "";

            InitializeLearnerKey();
            // On CellReady, validate and set the learner's login id
            if (cellReadyArgs.Cell != null)
            {
                m_observerRoleLearnerLogin = cellReadyArgs.Cell.ToString();
            }
        }
        #endregion

 

        #region Private Variables
        /// <summary>
        /// Holds List Scope - "Only Show Assignments for this Site" web part property
        /// </summary>
        private bool m_listScope;
        /// <summary>
        /// Holds Display Summary - "Show Summary" web part property
        /// </summary>
        private bool m_displaySummary;
        /// <summary>
        /// Holds Summary Width
        /// </summary>
        private Unit m_summaryWidth;
        /// <summary>
        /// Holds query Set Override
        /// </summary>
        private string m_querySetOverride;
        /// <summary>
        /// Stores the default Query Name - comes from the SLK Settings file
        /// </summary>
        private string m_defaultQueryName;
        /// <summary>
        /// Stores Current SPWeb
        /// </summary>
        private SPWeb m_spWeb;
        /// <summary>
        /// Stores the SlkStore
        /// </summary>
        private SlkStore m_slkStore;
        ///<summary>
        /// Stores the input learner login in the case of the observer mode
        ///</summary>
        private string m_observerRoleLearnerLogin;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AssignmentListWebPart()
        {
            // Set the culture for the resources
            AppResources.Culture = Thread.CurrentThread.CurrentCulture;

            // Initialize private variables.

            m_displaySummary = true;
            m_listScope = true;

            //Sets the WepPart Properties.
            this.Title = AppResources.AlwpWepPartTitle;

            this.Description = AppResources.AlwpWepPartDescription;

            this.ToolTip = String.Format(CultureInfo.CurrentCulture, 
                                         AppResources.AlwpWepPartToolTipFormat, 
                                         this.Title, this.Description);

        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Sets the assignment list to display assignments from this site or 
        /// all sites using the same SLK database.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpListScopeDisplayName"),
         AlwpWebDescription("AlwpListScopeDescription")]
        public bool ListScope
        {
            get { return m_listScope; }
            set { m_listScope = value; }
        }

        /// <summary>
        /// Shows or hides the query titles and summary counts with the assignment list.
        /// </summary>
        [WebBrowsable(),
         AlwpWebDisplayName("AlwpDisplaySummaryDisplayName"),
         AlwpWebDescription("AlwpDisplaySummaryDescription")]
        public bool DisplaySummary
        {
            get { return m_displaySummary; }
            set { m_displaySummary = value; }
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
                if (m_summaryWidth == null ||
                    m_summaryWidth.IsEmpty)
                {
                    m_summaryWidth =
                        new Unit(Constants.SummaryFrameWidth, UnitType.Pixel);
                }
                return m_summaryWidth;
            }
            set 
            {
                if (value.IsEmpty || value.Value > 0)
                {
                    m_summaryWidth = value;
                }
                else
                {
                    //Throw an ArgumentOutOfRangeException for negative values 
                    throw new ArgumentOutOfRangeException(AppResources.AlwpSummaryWidthDisplayName);
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
            get { return m_querySetOverride; }
            set { m_querySetOverride = SlkUtilities.GetHtmlEncodedText(value); }
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
                if (m_spWeb == null)
                    m_spWeb = SlkUtilities.GetCurrentSPWeb();
                return m_spWeb;
            }
        }

        /// <summary>
        /// Gets the current <c>SlkStore</c>.
        /// </summary>
        public SlkStore SlkStore
        {
            get
            {
                if (m_slkStore == null)
                    m_slkStore = SlkStore.GetStore(SPWeb);
                return m_slkStore;
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
                throw new UserNotFoundException(AppResources.SlkExUserNotFound);

            if (!String.IsNullOrEmpty(SlkUtilities.Trim(QuerySetOverride)))
            {
                // set <querySetDef> to the QuerySetDefinition named <querySetName>

                QuerySetDefinition querySetDef
                        = SlkStore.Settings.FindQuerySetDefinition(QuerySetOverride, true);
                if (querySetDef == null)
                {
                    throw new SafeToDisplayException
                                    (AppResources.AlwpQuerySetNotFound, QuerySetOverride);
                }
                else
                {
                    m_defaultQueryName = querySetDef.DefaultQueryName;
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
            edPart.Title = AppResources.AlwpWepPartTitle;
            editorArray.Add(edPart);

            return new EditorPartCollection(editorArray);
        }

        #endregion

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
                SlkError slkError;
                ErrorBanner.WriteException(e, out slkError);
                ErrorBanner.RenderErrorItems(writer, slkError);
            }
            catch (Exception ex)
            {
                SlkError slkError;
                SlkError.WriteException(ex, out slkError);
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
            // set <showSummary> and <showList> to indicate whether the summary and/or list cell should
            // be made visible 

            int cols; // number of columns of outer table
       
            SetAlwpProperties(out cols);

            //Validates the Alwp Properties 
            ValidateAlwpProperties();

            string observerRoleLearnerKey = GetLearnerKey(m_observerRoleLearnerLogin);

            //Adjust the Height to Fit width of zone

            string wpHeightStyle = "height: ";

            if (this.Height ==  null || this.Height.Length == 0)
            {
                wpHeightStyle += Constants.WebPartHeight + Constants.SemiColon;
            }
            else
            {
                wpHeightStyle += this.Height.ToString(CultureInfo.InvariantCulture) + Constants.SemiColon;
            }

            // create a unique name for the query results iframe based on
            // the web part's GUID. test this by having two ALWP web parts on the same page 
            //and ensuring that selecting a query in one doesn't affect the other and vice versa
            string FrameId = "Frame" + Guid.NewGuid().ToString().Replace("-", "");

            // write a comment to help locate this Web Part when viewing HTML source
            htmlTextWriter.Write("<!-- Begin ALWP -->");
            htmlTextWriter.WriteLine();

            // render the "<table>" element and its contents
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, wpHeightStyle);
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                // render the main row
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");

                using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                {
                    StringBuilder url = new StringBuilder(1000);

                    //Append the SPWebScope to QueryString

                    if (ListScope)
                    {
                        url.AppendFormat("&" + QueryStringKeys.SPWebScope + "={0}", 
                                         SPWeb.ID.ToString());
                    }
                    url.AppendFormat("&" + QueryStringKeys.FrameId + "={0}", FrameId);

                    // begin the summary views outer cell
                    if (DisplaySummary)
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width :" + 
                                                    SummaryWidth.ToString(CultureInfo.InvariantCulture));
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            //Append the QuerySet to QueryString
                            url.AppendFormat("&" + QueryStringKeys.QuerySet + "={0}", QuerySetOverride);

                            string urlQueryString = url.ToString();

                            if (urlQueryString.StartsWith("&", StringComparison.Ordinal))
                                urlQueryString = Constants.QuestionMark + urlQueryString.Substring(1);

                            // Get the ServerRelativeUrl for QueryResultPage 
                            string urlString = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl,
                                                    Constants.SlkUrlPath,
                                                    Constants.QuerySetPage);

                            //Append the QueryString Values
                            urlString += urlQueryString;


                            // write the summary cell 
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width :" +
                                                        SummaryWidth.ToString(CultureInfo.InvariantCulture));
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                            htmlTextWriter.AddAttribute("frameborder", "0");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, urlString);
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Name,
                                                        FrameId 
                                                        + "_AlwpQuerySummary");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Iframe, 1, htmlTextWriter);
                        }
                    }

                    // write the query results list cell 

                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                        htmlTextWriter.AddAttribute("frameborder", "0");
						if (!DisplaySummary)
						{
							//Append the QuerySet to QueryString
							url.AppendFormat("&" + QueryStringKeys.Query + "={0}", m_defaultQueryName);

							string urlQueryString = url.ToString();

                            if (urlQueryString.StartsWith("&", StringComparison.Ordinal))
								urlQueryString = Constants.QuestionMark + urlQueryString.Substring(1);

							// Get the ServerRelativeUrl for QueryResultPage 
							string urlString = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl,
													Constants.SlkUrlPath,
													Constants.QueryResultPage);

							//Append the QueryString Values
							urlString += urlQueryString;

							htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, urlString);
						}
						else
						{
							string urlString = SlkUtilities.UrlCombine(SPWeb.Url, Constants.BlankGifUrl);
							htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, urlString);
						}
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Name, FrameId);
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Iframe, 1, htmlTextWriter);
                    }
                }

                // write a row containing a separator line
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                {
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-partline");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, 
                                                cols.ToString(CultureInfo.InvariantCulture));
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        HtmlBlock.WriteBlankGif("1", "1", htmlTextWriter);
                }

                // write a row that adds extra vertical space
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                {
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, 
                                                cols.ToString(CultureInfo.InvariantCulture));
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        HtmlBlock.WriteBlankGif("1", "5", htmlTextWriter);
                }
            }

            // write a comment to help locate this Web Part when viewing HTML source
            htmlTextWriter.Write("<!-- End ALWP -->");
            htmlTextWriter.WriteLine();
        }

        #endregion

        #region SetAlwpProperties
        /// <summary>
        ///  Setting AssignmentListWebPart Inputs
        /// </summary>        
        /// <param name="cols">No of Columns</param>
        private void SetAlwpProperties(out int cols)
        {
            //Enable/disable Summary List depends on the WebPart Property

            //isShowSummary = DisplaySummary;
            //isShowList = true;
            cols = DisplaySummary ? 2 : 1;           

            //Set the QuerySetOverride Property
            SetQuerySetOverride();
        }
        #endregion        

        #region SetQuerySetOverride
        /// <summary>
        /// Check the Current User Role and Set the
        /// Default QuerySet accrodingly    
        /// </summary>
        private void SetQuerySetOverride()
        {
            //Override the QuerySet depends on the weppart property selection and role.

            if (String.IsNullOrEmpty(SlkUtilities.Trim(QuerySetOverride)))
            {

                //Defaluted to Default LearnerQuerySet For all, but instructor and observer.
                QuerySetOverride = Constants.DefaultLearnerQuerySet;               
               
                //// check for the role and assign the query set
                if (SlkStore.IsInstructor(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultInstructorQuerySet;
                }
                else if (SlkStore.IsLearner(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultLearnerQuerySet;
                }
                else if (SlkStore.IsObserver(SPWeb))
                {
                    QuerySetOverride = Constants.DefaultObserverQuerySet;
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
                try
                {
                    SPUser inputSPUser = SPWeb.AllUsers[learnerLogin];
                    string observerRoleLearnerKey = String.IsNullOrEmpty(inputSPUser.Sid) ? inputSPUser.LoginName : inputSPUser.Sid;
                    try
                    {
                        // Set the obtained LearnerKey as a session variable available across other pages
                        Page.Session["LearnerKey"] = observerRoleLearnerKey;
                    }
                    catch (HttpException)
                    {
                        throw new SafeToDisplayException(AppResources.SessionNotConfigured);
                    }
                    return observerRoleLearnerKey;
                }
                catch (SPException spe)
                {
                    throw new UserNotFoundException(spe.Message);
                }
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

    /// <summary>
    /// Error Banner For ALWP. 
    /// </summary>
    public static class ErrorBanner
    {
        #region RenderImage
        /// <summary>
        /// Render Error Message Image Tag
        /// </summary>
        /// <param name="errorType">ErrorType decides Image Src Attribute</param> 
        private static Image RenderImage(ErrorType errorType)
        {
            //Controls to Render Error
            Image imgErrorType = new Image();

            switch (errorType)
            {
                case ErrorType.Error:
                    {
                        //Error Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                        imgErrorType.ToolTip = AppResources.SlkErrorTypeErrorToolTip;
                        break;
                    }
                case ErrorType.Info:
                    {
                        //Info Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.InfoIcon;
                        imgErrorType.ToolTip = AppResources.SlkErrorTypeInfoToolTip;
                        break;
                    }
                case ErrorType.Warning:
                    {
                        //ErrorType Image Tag
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.WarningIcon;
                        imgErrorType.ToolTip = AppResources.SlkErrorTypeWarningToolTip;
                        break;
                    }

                default:
                    {
                        //Error Image Tag                       
                        imgErrorType.ImageUrl = Constants.ImagePath + Constants.ErrorIcon;
                        imgErrorType.ToolTip = AppResources.SlkErrorTypeErrorToolTip;
                        break;
                    }
            }

            return imgErrorType;
        }
        #endregion

        #region RenderErrorItems
        /// <summary>
        /// Render Error Message Literal Controls 
        /// Error Type and Error Text
        /// </summary>
        /// <param name="htmlTextWriter">HtmlTextWriter to Add the Items</param> 
        /// <param name="slkError">Error Items</param> 
        internal static void RenderErrorItems(HtmlTextWriter htmlTextWriter, SlkError slkError)
        {
            //Controls to Render Error

            Literal lcErrorText = new Literal();

            lcErrorText.ID = "lcErrorText";

            lcErrorText.Text = slkError.ErrorText;
           
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
            {
                // render the "<table>" element and its contents

                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarycustombody");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                                         "padding:3px 2px 4px 4px;");

                //SLK Release 1.4 – ITWorx
                //Date: 19 March, 2009
                //Code changes to fix “Firefox rendering issue”, Work Items # 3121 & 15909 on SLK Issue Tracker at CodePlex
                //For more details about the issue, check http://www.codeplex.com/SLK/WorkItem/View.aspx?WorkItemId=3121 
                //and http://www.codeplex.com/SLK/WorkItem/View.aspx?WorkItemId=15909

                htmlTextWriter.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0px");

                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                    {
                        if (!(slkError.ErrorType == ErrorType.Info))
                        {
                            //Add Attributes for the <TD> tag
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 22px;");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                            {
                                Image imgError = RenderImage(slkError.ErrorType);
                                imgError.RenderControl(htmlTextWriter);
                            }
                        }

                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            lcErrorText.RenderControl(htmlTextWriter);
                        }

                    }
                }
            }
        }

        #endregion

        #region WriteException
        /// <summary>
        /// Checks for deadlock and writes the SqlExeception to the event Log and outs the SlkError Object. 
        /// </summary>    
        /// <param name="sqlEx">SqlException</param>       
        /// <param name="slkError">SlkError Object.</param>
        internal static void WriteException(SqlException sqlEx, out SlkError slkError)
        {
            //Set the Standard Error text 
            string errorText = AppResources.SlkGenericError;
            
            //check whether deadlock occured
            if (sqlEx.Number == 1205)
            {
                errorText = AppResources.SlkExAlwpSqlDeadLockError;
            }

            //Slk Error with Generic or dead lock error message.

            errorText = Constants.Space + SlkUtilities.GetHtmlEncodedText(errorText);

            //Add the Error to Error Collection.
            slkError = new SlkError(ErrorType.Error, errorText);

            //log the exception in EventLog. 
            SlkError.WriteToEventLog(sqlEx);
        }
        #endregion
    }
    /// <summary>
    /// Defines the string value to use as a ToolTip for a property of a ALWP. 
    /// This allows the descriptions in ALWP's tool pane to be localized string resources.
    /// </summary>
    internal sealed class AlwpWebDescriptionAttribute : WebDescriptionAttribute
    {
        private bool m_loadedResource;

        public AlwpWebDescriptionAttribute(string resourceId)
            : base(resourceId)
        {
        }
        /// <summary>
        /// Gets the Description to display as a ToolTip
        /// </summary>
        public override string Description
        {
            get
            {
                if (!m_loadedResource)
                {
                    DescriptionValue = AppResources.ResourceManager.GetString(base.Description); 
                    m_loadedResource = true;
                }
                return base.Description;
            }
        }
    }
    /// <summary>
    /// Defines the friendly name for a property of a ALWP.
    /// This allows the labels in ALWP's tool pane to be localized string resources.
    /// </summary>
    internal sealed class AlwpWebDisplayNameAttribute : WebDisplayNameAttribute
    {
        private bool m_loadedResource;

        public AlwpWebDisplayNameAttribute(string resourceId)
            : base(resourceId)
        {
        }
        /// <summary>
        /// Gets the name of a property to display 
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (!m_loadedResource)
                {
                    DisplayNameValue = AppResources.ResourceManager.GetString(base.DisplayName);
                    m_loadedResource = true;
                }
                return base.DisplayName;
            }
        }
    }
}
