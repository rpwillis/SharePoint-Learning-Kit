/* Copyright (c) Microsoft Corporation. All rights reserved. */

// ActionsPage.aspx.cs
//
// Code-behind for Actions.aspx.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Frameset;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit.WebControls;
using Resources.Properties;
using System.Configuration;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{

    /// <summary>
    /// Code-behind for Actions.aspx.
    /// </summary>
    public class ActionsPage : SlkAppBasePage
    {
#region Control Declarations
#pragma warning disable 1591
        protected Label ResourceFileName;
        protected HyperLink DocLibLink;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblTitle;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDescription;
        protected DropDownList organizationList;
        protected ErrorBanner errorBanner;
        protected Literal pageTitle;
        protected Literal pageDescription;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblOrganization;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblWhatHeader;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblSelfAssignHeader;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblSelfAssignAssign;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk")]
        protected LinkButton lnkAssignSelf;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblAssignSelf;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblChoose;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk")]
        protected HyperLink lnkMRUTestLink;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk")]
        protected HyperLink lnkMRUAddSite;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblMRUAddress;
        protected PlaceHolder siteList;
        protected TextBox txtNewSite;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk")]
        protected LinkButton lnkMRUShowAll;
        protected Button addButton;
        protected Panel contentPanel;
        protected TableGridRow selfAssignRow;
        protected TableGridRow organizationRow;
        protected TableGridRow organizationRowBottomLine;
        protected RequiredFieldValidator newSiteRequired;
#pragma warning restore 1591
#endregion

        #region Private Variables
        // property values
        AssignmentObjectsFromQueryString objects;
        private Guid? newSite;
        private SPFile m_spFile;
        Package package;
        #endregion

        #region Private Properties
        bool NoFileAssignment
        {
            get { return Request.QueryString["Location"] == Package.NoPackageLocation.ToString() ;}
        }

        /// <summary>
        /// Returns <c>true</c> if the file that the Actions page is acting upon is a non-e-learning document.
        /// Returns <c>false</c> if it's an e-learning package.
        /// </summary>
        private bool NonELearning
        {
            get
            {
                if (package == null)
                {
                    LoadSlkObjects();
                    if (package == null)
                    {
                        throw new InternalErrorException("SLKActions1003");
                    }
                }

                return package.IsNonELearning;
            }
        }

        /// <summary>
        /// Returns the selected organization index, 0 if there is only one,
        /// or null if the content is non-elearning
        /// </summary>
        private int? OrganizationIndex
        {
            get
            {
                if (NonELearning)
                    return null;

                if (organizationList.Visible)
                    return organizationList.SelectedIndex;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Returns the SPFile object of the file that the Actions page is acting upon.
        /// </summary>
        private SPFile SPFile
        {
            get
            {
                if (m_spFile == null)
                {
                    LoadObjects();
                    if (m_spFile == null)
                    {
                        throw new InternalErrorException("SLKActions1004");
                    }
                }
                return m_spFile;
            }
        }

        /// <summary>
        /// Returns the SPListItem object of the file that the Actions page is acting upon.
        /// </summary>
        private SPListItem SPListItem
        {
            get
            {
                if (objects == null)
                {
                    LoadObjects();
                    if (objects.ListItem == null)
                        throw new InternalErrorException("SLKActions1005");
                }

                return objects.ListItem;
            }
        }

        /// <summary>
        /// Returns the SPList that contains the file that the Actions page is acting upon.
        /// </summary>
        private SPList SPList
        {
            get
            {
                if (objects == null)
                {
                    LoadObjects();
                    if (objects.List == null)
                        throw new InternalErrorException("SLKActions1006");
                }
                return objects.List;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the Actions page is currently supposed to be displaying package validation
        /// warnings, i.e. if the user has clicked the "(show details)" link.
        /// </summary>
        private bool ShowWarnings
        {
            get
            {
                if (ViewState["showWarnings"] != null)
                    return (bool)ViewState["showWarnings"];

                return false;
            }
            set
            {
                ViewState["showWarnings"] = value;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the Actions page is currently supposed to be displaying all Web sites in
        /// the user's web site MRU list, i.e. if the user has clicked "Show all sites".
        /// </summary>
        private bool ShowAllSites
        {
            get
            {
                if (ViewState["showAllSites"] != null)
                    return (bool)ViewState["showAllSites"];

                return false;
            }
            set
            {
                ViewState["showAllSites"] = value;
            }
        }
        #endregion

#region protected members
        /// <summary>See <see cref="Microsoft.SharePoint.WebControls.UnsecuredLayoutsPageBase.OnInit"/>.</summary>
        protected override void OnInit(EventArgs e)
        {
            // Adding in a fake control to handle our "(show details...)" link
            Button showWarnings = new Button();
            showWarnings.ID = "showWarnings";
            showWarnings.Visible = false;
            showWarnings.CausesValidation = false;
            showWarnings.Click += new EventHandler(showWarnings_Click);
            this.Controls.Add(showWarnings);

            base.OnInit(e);

            try
            {
                if (NoFileAssignment == false)
                {
                    LoadSlkObjects();
                }

                string action = QueryString.ParseStringOptional(QueryStringKeys.Action);

                if (string.IsNullOrEmpty(action) == false)
                {
                    switch (action.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "assignself":
                            AssignToSelf();
                            break;
                        case "assignsite":
                            AssignToSite();
                            break;
                        default:
                            //Ignore
                            break;
                    }
                }
            }
            catch (SafeToDisplayException exception)
            {
                errorBanner.Clear();
                errorBanner.AddException(SlkStore, exception);
            }
        }

        /// <summary>See <see cref="Page.Render"/>.</summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Not sure best place to put this.
            if (package != null)
            {
                package.Dispose();
            }

            // This is needed to allow the showWarnings postback
            this.ClientScript.RegisterForEventValidation("showWarnings");

            base.Render(writer);
        }

        /// <summary>The Pre-Render event.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                SetResourceText();

                if (NoFileAssignment)
                {
                    // non-file assignment
                    DocLibLink.Text = AppResources.ActionsDocLibLinkNoFile;
                    ResourceFileName.Text = Request.QueryString["Title"];
                    lblTitle.Text = ResourceFileName.Text;
                    selfAssignRow.Visible = false;
                    organizationRow.Visible = false;
                    organizationRowBottomLine.Visible = false;
                }
                else
                {
                    CheckAndDisplayFile();
                }

                List<WebListItem> webList = GetSiteList();

                PopulateSiteListControl(webList);

                // the "addSiteUrl" JScript local variable will contain the unique client-side ID of the "Add a site" text box
                ClientScript.RegisterClientScriptBlock(this.GetType(), "addSiteUrl", "var addSiteUrl = \"" + txtNewSite.ClientID + "\";", true);
                ClientScript.RegisterClientScriptBlock(this.GetType(), "addSiteUrlMessage", "var addSiteUrlMessage = \"" + AppResources.ActionsSiteRequired + "\";", true);

                contentPanel.Visible = true;
            }
            catch (UserNotFoundException ex)
            {
                errorBanner.Clear();
                errorBanner.AddException(SlkStore, ex);
            }

            catch (SafeToDisplayException ex)
            {
                if (NoFileAssignment == false)
                {
                    DocLibLink.Text = Server.HtmlEncode(SPList.Title);
                    ResourceFileName.Text = Server.HtmlEncode(SPListItem.Name);
                }
                errorBanner.AddException(SlkStore, ex);
            }

            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException
            }
            catch (Exception ex)
            {
                errorBanner.AddException(SlkStore, ex);
            }
        }

        /// <summary>Event when click on show all.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void lnkMRUShowAll_Click(object sender, EventArgs e)
        {
            // user clicked on link to show or hide complete web site list
            ShowAllSites = !ShowAllSites;
        }

        /// <summary>
        /// btnAdd Click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void addButton_Click(object sender, EventArgs e)
        {
            // user clicked "Add" button (after clicking "Add a site to this list")

            bool showAddSite = true;

            bool previousValue = SPSecurity.CatchAccessDeniedException;
            SPSecurity.CatchAccessDeniedException = false;
            try
            {
                string destinationUrl = txtNewSite.Text.Trim();
                Uri destinationUri = new Uri(destinationUrl);
                using (SPSite site = new SPSite(destinationUri.AbsoluteUri))
                {
                    using (SPWeb destinationWeb = site.OpenWeb())
                    {
                        // check if the site is a valid Slk site
                        SlkStore destinationSlkStore;
                        try
                        {
                            destinationSlkStore = SlkStore.GetStore(destinationWeb);
                        }
                        catch (SlkNotConfiguredException)
                        {
                            errorBanner.AddHtmlErrorText(ErrorType.Warning, PageCulture.Format(AppResources.ActionsNotEnabled, Server.HtmlEncode(destinationUrl)));
                            DisplayAddSite();
                            return;
                        }

                        // check if the user is an instructor on that site
                        if (!destinationSlkStore.IsInstructor(destinationWeb))
                        {
                            errorBanner.AddHtmlErrorText(ErrorType.Warning, PageCulture.Format(AppResources.ActionsNotInstructor, Server.HtmlEncode(destinationUrl)));
                            DisplayAddSite();
                            return;
                        }

                        // check if the site is already in the list
                        ReadOnlyCollection<SlkUserWebListItem> userWebList = SlkStore.FetchUserWebList();
                        foreach (SlkUserWebListItem webListItem in userWebList)
                        {
                            if (destinationWeb.ID.Equals(webListItem.SPWebGuid))
                            {
                                errorBanner.AddHtmlErrorText(ErrorType.Info, PageCulture.Format(AppResources.ActionsAlreadyInList, Server.HtmlEncode(destinationWeb.Title)));
                                break;
                            }
                        }

                        // add the web to the list
                        SlkStore.AddToUserWebList(destinationWeb); //local slkstore
                        ShowAllSites = false;
                        txtNewSite.Text = string.Empty;
                        newSite = destinationWeb.ID;
                        showAddSite = false;
                    }
                }
            }
            catch (UriFormatException)
            {
                // the url is an invalid format
                errorBanner.AddHtmlErrorText(ErrorType.Warning, PageCulture.Format(AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
            }
            catch (UnauthorizedAccessException)
            {
                // the user doesn't have permission to access this site, so show an error message
                errorBanner.AddHtmlErrorText(ErrorType.Warning, PageCulture.Format(AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
            }
            catch (FileNotFoundException)
            {
                errorBanner.AddHtmlErrorText(ErrorType.Warning, PageCulture.Format(AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
            }
            finally
            {
                SPSecurity.CatchAccessDeniedException = previousValue;
            }
            if (showAddSite)
                DisplayAddSite();
        }
#endregion protected members

#region event handlers
        /// <summary>The event handler for the assign self click.</summary>
        protected void lnkAssignSelf_Click(object sender, EventArgs arguments)
        {
            AssignToSelf();
        }
#endregion event handlers


#region private methods
        void AssignToSite()
        {
            try
            {
                string url = AssignmentSiteUrl(SPWeb.Url);
                Response.Redirect(url, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (SafeToDisplayException e)
            {
                errorBanner.Clear();
                errorBanner.AddError(ErrorType.Error, e.Message);
            }
            catch (Exception ex)
            {
                contentPanel.Visible = false;
                errorBanner.AddException(SlkStore, ex);
            }
        }

        void AssignToSelf()
        {
            try
            {
                Guid learnerAssignmentGuidId = AssignmentProperties.CreateSelfAssignment(SlkStore, SPWeb, package.Location, OrganizationIndex);
                string url = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, "_layouts/SharePointLearningKit/Lobby.aspx");
                url = String.Format(CultureInfo.InvariantCulture, 
                        "{0}?{1}={2}", url, FramesetQueryParameter.LearnerAssignmentId, learnerAssignmentGuidId.ToString());

                Response.Redirect(url, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (SafeToDisplayException e)
            {
                errorBanner.Clear();
                errorBanner.AddError(ErrorType.Error, e.Message);
            }
            catch (Exception ex)
            {
                contentPanel.Visible = false;
                errorBanner.AddException(SlkStore, ex);
            }
        }

        string AssignmentSiteUrl(string webUrl)
        {
            string urlFormat = "{0}/_layouts/SharePointLearningKit/AssignmentProperties.aspx?Location={1}{2}{3}";
            string orgIndex = null;
            string titleValue = null;
            SharePointFileLocation location = null;
            string title = null;

            if (NoFileAssignment)
            {
                title = Request.QueryString["title"];;
                location = Package.NoPackageLocation;
            }
            else
            {
                location = package.Location;
                if (NonELearning == false)
                {
                    orgIndex = String.Format(CultureInfo.InvariantCulture, "&OrgIndex={0}", OrganizationIndex);
                }
            }

            if (string.IsNullOrEmpty(title) == false)
            {
                titleValue = "&Title=" + title;
            }

            return String.Format(CultureInfo.InvariantCulture, urlFormat, webUrl, location.ToString(), orgIndex, titleValue);
        }

        private void PopulateSiteListControl(List<WebListItem> webList)
        {
            HyperLink hl = null;
            HtmlGenericControl li = null;
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            try
            {
                foreach (WebListItem webListItem in webList)
                {
                    li = new HtmlGenericControl("li");
                    li.Attributes.Add("title", PageCulture.Format(AppResources.ActionsMRUToolTip, Server.HtmlEncode(webListItem.Title)));
                    hl = new HyperLink();
                    hl.NavigateUrl = AssignmentSiteUrl(webListItem.Url);

                    hl.Text = Server.HtmlEncode(webListItem.Title);
                    li.Controls.Add(hl);
                    hl = null;
                    if (newSite.HasValue && newSite.Value.Equals(webListItem.SPWebGuid))
                    {
                        HtmlImage img = new HtmlImage();
                        img.Src = "Images/NewIcon.gif";
                        li.Controls.Add(img);
                    }
                    ul.Controls.Add(li);
                    li = null;
                }
                siteList.Controls.Add(ul);
                ul = null;
            }
            catch
            {
                if(hl != null)
                    hl.Dispose();
                if(li != null)
                    li.Dispose();
                if(ul != null)
                    ul.Dispose();
                throw;
            }
        }

        private static string GetDefaultTitle(string title, string id)
        {
            if (String.IsNullOrEmpty(title))
            {
                // Ids can be much longer than titles, so truncate the id to fit in the title column.
                int maxTitleLen = 255; // Setting to 255 since the BaseSchemaInternal class is internal
                if (id.Length > maxTitleLen)
                    return id.Substring(0, maxTitleLen);
                else
                    return id;
            }
            return title;
        }

        private void DisplayAddSite()
        {
            ClientScript.RegisterStartupScript(this.GetType(), "DisplayAddSite", "DisplayAddSiteUi();", true);
        }
        /// <summary>
        /// Initializes <c>SPFile</c>, <c>SPListItem</c>, and <c>SPList</c>.
        /// </summary>
        private void LoadObjects()
        {
            objects = new AssignmentObjectsFromQueryString();
            objects.LoadObjects(SPWeb);
            m_spFile = objects.File;
        }

        /// <summary>
        /// Initializes <c>Location</c> and <c>NonELearning</c>.
        /// </summary>
        private void LoadSlkObjects()
        {
            LoadObjects();
            package = new Package(SlkStore, SPFile, SPWeb);
        }

        /// <summary>
        /// Copies localizable strings from string resources to UI.
        /// </summary>
        private void SetResourceText()
        {
            pageTitle.Text = AppResources.ActionsPageTitle;
            lblOrganization.Text = AppResources.ActionslblOrganization;
            if (NoFileAssignment)
            {
                lblWhatHeader.Text = AppResources.ActionslblWhatHeaderNoFile;
                lblSelfAssignAssign.Text = AppResources.ActionslblSelfAssignAssignNoFile;
                pageDescription.Text = AppResources.ActionsPageDescriptionNoFile;
            }
            else
            {
                lblWhatHeader.Text = AppResources.ActionslblWhatHeader;
                lblSelfAssignAssign.Text = AppResources.ActionslblSelfAssignAssign;
                pageDescription.Text = AppResources.ActionsPageDescription;
            }
            lblSelfAssignHeader.Text = AppResources.ActionslblSelfAssignHeader;
            lnkAssignSelf.Text = AppResources.ActionslnkAssignSelf;
            lblAssignSelf.Text = AppResources.ActionslblAssignSelf;
            lblChoose.Text = AppResources.ActionslblChoose;
            lnkMRUAddSite.Text = AppResources.ActionslnkMRUAddSite;
            lblMRUAddress.Text = AppResources.ActionslblMRUAddress;
            lnkMRUTestLink.Text = AppResources.ActionslnkMRUTestLink;
            addButton.Text = AppResources.ActionsbtnAdd;
            newSiteRequired.ErrorMessage = AppResources.ActionsSiteRequired;
        }

        void showWarnings_Click(object sender, EventArgs e)
        {
            ShowWarnings = !ShowWarnings;
        }

        private List<WebListItem> GetSiteList()
        {
            // code to iterate through user's list of SPWebs
            bool addCurrentToList = true;
            int mruItems = SlkStore.Settings.UserWebListMruSize;
            int itemMax;
            int listCount;
            ReadOnlyCollection<SlkUserWebListItem> userWebList = SlkStore.FetchUserWebList();

            foreach (SlkUserWebListItem webListItem in userWebList)
            {
                if (SPWeb.ID.Equals(webListItem.SPWebGuid))
                {
                    addCurrentToList = false;
                    break;
                }
            }

            if (addCurrentToList)
                listCount = userWebList.Count + 1;
            else
                listCount = userWebList.Count;

            if (!ShowAllSites && listCount > mruItems)
            {
                itemMax = mruItems;
                lnkMRUShowAll.Visible = true;
                lnkMRUShowAll.Text = AppResources.ActionslnkMRUShowAll;
            }
            else
            {
                itemMax = listCount;
                lnkMRUShowAll.Visible = false;
            }

            List<WebListItem> webList = new List<WebListItem>(itemMax);

            if (addCurrentToList)
                itemMax--;

            for (int i = 0; i < itemMax && i < userWebList.Count; i++)
            {
                bool previousValue = SPSecurity.CatchAccessDeniedException;
                SPSecurity.CatchAccessDeniedException = false;
                try
                {
                    SlkUserWebListItem item = userWebList[i];
                    using (SPSite site = new SPSite(item.SPSiteGuid, SPContext.Current.Site.Zone))
                    {
                        using (SPWeb web = site.OpenWeb(item.SPWebGuid))
                        {
                            if (SPWeb.ID.Equals(item.SPWebGuid))
                            {
                                webList.Add(new WebListItem(item, web.Url, PageCulture.Format("{0} {1}", web.Title, AppResources.ActionslblMRUCurrentSite)));
                            }
                            else
                            {
                                webList.Add(new WebListItem(item, web.Url, web.Title));
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // the user doesn't have permission to access this site, so ignore it
                    continue;
                }
                catch (System.Data.SqlClient.SqlException)
                {
                    // site is in another web application and this web application's app pool user doesn't have access
                    continue;
                }
                catch (FileNotFoundException)
                {
                    // the site doesn't exist
                    continue;
                }
                finally
                {
                    SPSecurity.CatchAccessDeniedException = previousValue;
                }
            }

            if (addCurrentToList)
            {
                webList.Add(new WebListItem(SPWeb.Site.ID, SPWeb.ID, DateTime.Now, SPWeb.Url,
                    PageCulture.Format("{0} {1}", SPWeb.Title, AppResources.ActionslblMRUCurrentSite)));
            }
            webList.Sort();

            return webList;
        }

        void CheckAndDisplayFile()
        {
            ResourceFileName.Text = Server.HtmlEncode(SPFile.Name);
            DocLibLink.NavigateUrl = SPList.DefaultViewUrl;
            DocLibLink.Text = Server.HtmlEncode(SPList.Title);

            // Make sure that the package isn't checked out.
            //Using LoginName instead of Sid as Sid may be empty while using FBA
#if SP2010
            if (SPFile.CheckOutType != SPFile.SPCheckOutType.None && SPFile.CheckedOutBy.LoginName.Equals(SPWeb.CurrentUser.LoginName))
#else
            if (SPFile.CheckOutStatus != SPFile.SPCheckOutStatus.None && SPFile.CheckedOutBy.LoginName.Equals(SPWeb.CurrentUser.LoginName))
#endif
            {
                // If it's checked out by the current user, show an error.
                throw new SafeToDisplayException(AppResources.ActionsCheckedOutError);
            }

            // no minor versions or limited version number warnings
            if (!SPList.EnableVersioning || SPList.MajorVersionLimit != 0 || SPList.MajorWithMinorVersionsLimit != 0)
            {
                if (SlkStore.Settings.AutoVersionLibrariesIfUnversioned)
                {
                    SlkStore.VersionLibrary((SPDocumentLibrary)SPList);
                    Response.Redirect(Request.RawUrl, true);
                }
                else
                {
                    errorBanner.AddError(ErrorType.Warning, PageCulture.Format(AppResources.ActionsVersioningOff, Server.HtmlEncode(SPList.Title)));
                }
            }

            // If the current file isn't a published version, show a warning.
            // If the document library doesn't have minor versions, the file is NEVER SPFileLevel.Published
            if (SPList.EnableMinorVersions)
            {
                if (SPFile.Level == SPFileLevel.Draft)
                {
                    errorBanner.AddError(ErrorType.Warning, AppResources.ActionsDraftVersion);
                }
            }

            if (!IsPostBack)
            {
                // get information about the package: populate the "Organizations" row
                // (as applicable) in the UI, and set <title> and <description> to the text
                // of the title and description to display
                string title, description;
                if (NonELearning)
                {
                    // non-e-learning content...

                    // set <title> and <description>
                    title = SPFile.Title;
                    if (String.IsNullOrEmpty(title))
                    {
                        title = Path.GetFileNameWithoutExtension(SPFile.Name);
                    }
                    description = string.Empty;

                    // hide the "Organizations" row
                    organizationRow.Visible = false;
                    organizationRowBottomLine.Visible = false;
                }
                else
                {
                    // e-learning content...

                    // set <packageInformation> to refer to information about the package
                    title = package.Title;
                    description = package.Description;

                    // populate the drop-down list of organizations; hide the entire row containing that drop-down if there's only one organization
                    if (package.Organizations.Count <= 1)
                    {
                        organizationRow.Visible = false;
                        organizationRowBottomLine.Visible = false;
                    }
                    else
                    {
                        foreach (OrganizationNodeReader nodeReader in package.Organizations)
                        {
                            string id = nodeReader.Id;
                            organizationList.Items.Add(new ListItem(Server.HtmlEncode(GetDefaultTitle(nodeReader.Title, id)), id));
                        }

                        ListItem defaultOrganization = organizationList.Items.FindByValue(package.DefaultOrganizationId.ToString(CultureInfo.InvariantCulture));
                        if (defaultOrganization != null)
                        {
                            defaultOrganization.Selected = true;
                        }
                    }
                }

                // copy <title> to the UI
                lblTitle.Text = Server.HtmlEncode(title);
                lblDescription.Text = SlkUtilities.GetCrlfHtmlEncodedText(description);
            }

            // if the package has warnings, tell the user
            if (package.Warnings != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(AppResources.ActionsWarning);
                sb.AppendLine("<br />");
                sb.Append("<a href=\"javascript: __doPostBack('showWarnings','');\">");
                if (ShowWarnings)
                    sb.Append(AppResources.ActionsHideDetails);
                else
                    sb.Append(AppResources.ActionsShowDetails);
                sb.AppendLine("</a>");

                if (ShowWarnings)
                {
                    sb.AppendLine("<ul style=\"margin-top:0;margin-bottom:0;margin-left:24;\">");
                    using (XmlReader xmlReader = package.Warnings.CreateReader())
                    {
                        XPathNavigator root = new XPathDocument(xmlReader).CreateNavigator();
                        foreach (XPathNavigator error in root.Select("/Warnings/Warning"))
                        {
                            sb.Append("<li>");
                            sb.Append(Server.HtmlEncode(error.Value));
                            sb.AppendLine("</li>");
                        }
                    }
                    sb.Append("</ul>\n");
                }
                errorBanner.AddHtmlErrorText(ErrorType.Warning, sb.ToString());
            }
        }

#endregion private methods


    }
}

