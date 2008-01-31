/* Copyright (c) Microsoft Corporation. All rights reserved. */

// ActionsPage.aspx.cs
//
// Code-behind for Actions.aspx.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.SharePointLearningKit.WebControls;
using System.Threading;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{

	/// <summary>
	/// Code-behind for Actions.aspx.
	/// </summary>
	public class ActionsPage : SlkAppBasePage
	{
		#region Control Declarations
		// page controls
		protected Label ResourceFileName;
		protected HyperLink DocLibLink;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
		protected Label lblTitle;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
		protected Label lblDescription;
		//protected HyperLink AssignHyperLink;
		protected DropDownList organizationList;
		protected ErrorBanner errorBanner;
		protected Literal pageTitle;
		//protected Literal pageTitleinTitlePage;
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
		protected TableGridRow organizationRow;
		protected TableGridRow organizationRowBottomLine;
		protected RequiredFieldValidator newSiteRequired;
		#endregion

		#region Private Variables
		// property values
		private Guid? newSite;
		private Guid? m_listId;
		private int? m_itemId;
		private SPFile m_spFile;
		private SPListItem m_spListItem;
		private SPList m_spList;
		private string m_location;
		private int? m_versionId;
		private bool? m_nonELearning;
		#endregion

		#region Private Properties

		/// <summary>
		/// Retrieves from the query string the GUID of the document library containing the
		/// e-learning package or non-e-learning document on which an action will be performed.
		/// </summary>
		private Guid ListId
		{
			get
			{
				if (!m_listId.HasValue)
				{
					Guid id;
					QueryString.Parse(QueryStringKeys.ListId, out id);
					m_listId = id;
				}
				return m_listId.Value;
			}
		}

		/// <summary>
		/// Retrieves from the query string the SharePoint integer identifier of the file
		/// on which an action will be performed.
		/// </summary>
		private int ItemId
		{
			get
			{
				if (!m_itemId.HasValue)
				{
					int id;
					QueryString.Parse(QueryStringKeys.ItemId, out id);
					m_itemId = id;
				}
				return m_itemId.Value;
			}
		}

		/// <summary>
		/// Retrieves the location string of the file that the Actions page is acting upon.
		/// </summary>
		private string Location
		{
			get
			{
				if (m_location == null)
				{
					LoadSlkObjects();
					if (m_location == null)
						throw new InternalErrorException("SLKActions1001");
				}
				return m_location;
			}
		}

		/// <summary>
		/// Retrieves the SharePoint version identifier of the file that the Actions page is acting upon.
		/// For example, the first version is typically 512, corresponding to version "1.0".
		/// </summary>
		private int VersionId
		{
			get
			{
				if (!m_versionId.HasValue)
				{
					LoadObjects();
					if (!m_versionId.HasValue)
						throw new InternalErrorException("SLKActions1002");
				}
				return m_versionId.Value;
			}
		}

		/// <summary>
		/// Returns <c>true</c> if the file that the Actions page is acting upon is a non-e-learning document.
		/// Returns <c>false</c> if it's an e-learning package.
		/// </summary>
		private bool NonELearning
		{
			get
			{
				if (!m_nonELearning.HasValue)
				{
					LoadSlkObjects();
					if (!m_nonELearning.HasValue)
						throw new InternalErrorException("SLKActions1003");
				}
				return m_nonELearning.Value;
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
						throw new InternalErrorException("SLKActions1004");
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
				if (m_spListItem == null)
				{
					LoadObjects();
					if (m_spListItem == null)
						throw new InternalErrorException("SLKActions1005");
				}
				return m_spListItem;
			}
		}

		/// <summary>
		/// Returns the SPList that contains the file that the Actions page is acting upon.
		/// </summary>
		private SPList SPList
		{
			get
			{
				if (m_spList == null)
				{
					LoadObjects();
					if (m_spList == null)
						throw new InternalErrorException("SLKActions1006");
				}
				return m_spList;
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
		}

		void showWarnings_Click(object sender, EventArgs e)
		{
			ShowWarnings = !ShowWarnings;
		}

		protected override void Render(HtmlTextWriter writer)
		{
			// This is needed to allow the showWarnings postback
			this.ClientScript.RegisterForEventValidation("showWarnings");

			base.Render(writer);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override void OnPreRender(EventArgs e)
		{
			try
			{
				SetResourceText();

				ResourceFileName.Text = Server.HtmlEncode(SPFile.Name);
				DocLibLink.NavigateUrl = SPList.DefaultViewUrl;
				DocLibLink.Text = Server.HtmlEncode(SPList.Title);

				// Make sure that the package isn't checked out.
				//Using LoginName instead of Sid as Sid may be empty while using FBA
				if (SPFile.CheckOutStatus != SPFile.SPCheckOutStatus.None &&
					SPFile.CheckedOutBy.LoginName.Equals(SPWeb.CurrentUser.LoginName))
				{
					// If it's checked out by the current user, show an error.
					throw new SafeToDisplayException(AppResources.ActionsCheckedOutError);
				}

				// no minor versions or limited version number warnings
				if (!SPList.EnableVersioning || SPList.MajorVersionLimit != 0
					|| SPList.MajorWithMinorVersionsLimit != 0)
				{
					errorBanner.AddError(ErrorType.Warning,
						string.Format(CultureInfo.CurrentCulture, AppResources.ActionsVersioningOff, Server.HtmlEncode(SPList.Title)));
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

				// validate the package -- if it's already registered in LearningStore then
				// retrieve the cached warnings, otherwise validate the package now, but don't
				// actually register it (since we want to avoid unnecessary registrations of the
				// package in the database in the case where an author edits and previews a
				// package many times before deciding to assign it); note that non-e-learning
				// files are not validated
				LearningStoreXml warnings;
				if (NonELearning)
					warnings = null;
				else
					warnings = SlkStore.ValidatePackage(Location);

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
							title = Path.GetFileNameWithoutExtension(SPFile.Name);
						description = "";

						// hide the "Organizations" row
						organizationRow.Visible = false;
						organizationRowBottomLine.Visible = false;
					}
					else
					{
						// e-learning content...

						// set <packageInformation> to refer to information about the package
						PackageInformation packageInformation = SlkStore.GetPackageInformation(Location);
						m_spFile = packageInformation.SPFile;
						title = packageInformation.Title;
						description = packageInformation.Description;

						// populate the drop-down list of organizations; hide the entire row containing that
						// drop-down if there's only one organization
						ReadOnlyCollection<OrganizationNodeReader> organizations = packageInformation.ManifestReader.Organizations;
						foreach (OrganizationNodeReader nodeReader in organizations)
						{
							string id = nodeReader.Id;
							organizationList.Items.Add(new ListItem(Server.HtmlEncode(GetDefaultTitle(nodeReader.Title, id)), id));
							if (packageInformation.ManifestReader.DefaultOrganization.Id == id)
								organizationList.Items.FindByValue(id).Selected = true;
						}
						if (organizations.Count == 1)
						{
							organizationRow.Visible = false;
							organizationRowBottomLine.Visible = false;
						}
					}

					// copy <title> to the UI
					lblTitle.Text = Server.HtmlEncode(title);
					lblDescription.Text = SlkUtilities.GetCrlfHtmlEncodedText(description);
				}

				// if the package has warnings, tell the user
				if (warnings != null)
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
						using (XmlReader xmlReader = warnings.CreateReader())
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
				errorBanner.AddException(ex);
			}

			catch (SafeToDisplayException ex)
			{
				DocLibLink.Text = Server.HtmlEncode(SPList.Title);
				ResourceFileName.Text = Server.HtmlEncode(SPListItem.Name);
				errorBanner.AddException(ex);
			}

			catch (Exception ex)
			{
				errorBanner.AddException(ex);
			}
		}

		private List<WebListItem> GetSiteList()
		{
			// code to iterate through user's list of SPWebs
			bool addCurrentToList = true;
			int mruItems = SlkStore.Settings.UserWebListMruSize;
			int itemMax;
			int listCount;
			ReadOnlyCollection<SlkUserWebListItem> userWebList = SlkStore.GetUserWebList();

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
				SPSite site = null;
				SPWeb web = null;
				bool previousValue = SPSecurity.CatchAccessDeniedException;
				SPSecurity.CatchAccessDeniedException = false;
				try
				{
					SlkUserWebListItem item = userWebList[i];
					site = new SPSite(item.SPSiteGuid);
					web = site.OpenWeb(item.SPWebGuid);
					if (SPWeb.ID.Equals(item.SPWebGuid))
						webList.Add(new WebListItem(item, web.Url, string.Format(CultureInfo.CurrentCulture, "{0} {1}", web.Title, AppResources.ActionslblMRUCurrentSite)));
					else
						webList.Add(new WebListItem(item, web.Url, web.Title));
				}
				catch (UnauthorizedAccessException)
				{
					// the user doesn't have permission to access this site, so ignore it
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
					if(web != null)
					    web.Dispose();
					if(site != null)
					    site.Dispose();
				}
			}
			if (addCurrentToList)
			{
				webList.Add(new WebListItem(SPWeb.Site.ID, SPWeb.ID, DateTime.Now, SPWeb.Url,
					string.Format(CultureInfo.CurrentCulture, "{0} {1}", SPWeb.Title, AppResources.ActionslblMRUCurrentSite)));
			}
			webList.Sort();

			return webList;
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
					li.Attributes.Add("title", string.Format(CultureInfo.CurrentCulture, AppResources.ActionsMRUToolTip, Server.HtmlEncode(webListItem.Title)));
					hl = new HyperLink();
					string url = SlkUtilities.UrlCombine(webListItem.Url, "_layouts/SharePointLearningKit/AssignmentProperties.aspx");
					url = String.Format(CultureInfo.InvariantCulture, "{0}?Location={1}", url, Location);

					if (!NonELearning)
						url = String.Format(CultureInfo.InvariantCulture, "{0}&OrgIndex={1}", url, OrganizationIndex);
					hl.NavigateUrl = url;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
		protected void lnkMRUShowAll_Click(object sender, EventArgs e)
		{
			// user clicked on link to show or hide complete web site list
			ShowAllSites = !ShowAllSites;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lnk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected void lnkAssignSelf_Click(object sender, EventArgs e)
		{
			try
			{
				LearningStoreXml packageWarnings;
				AssignmentProperties properties = SlkStore.GetNewAssignmentDefaultProperties(SPWeb, Location, OrganizationIndex, SlkRole.Learner, out packageWarnings);
				AssignmentItemIdentifier assignmentId = SlkStore.CreateAssignment(SPWeb, Location, OrganizationIndex, SlkRole.Learner, properties);
				Guid learnerAssignmentGuidId = SlkStore.GetCurrentUserLearnerAssignment(assignmentId);

				string url = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, "_layouts/SharePointLearningKit/Lobby.aspx");
				url = String.Format(CultureInfo.InvariantCulture, "{0}?LearnerAssignmentGuidId={1}", url, learnerAssignmentGuidId.ToString());

				Response.Redirect(url, true);
			}
			catch (ThreadAbortException)
			{
				// Calling Response.Redirect throws a ThreadAbortException which will
				// flag an error in the next step if we don't do this.
				throw;
			}
			catch (Exception ex)
			{
				contentPanel.Visible = false;
				errorBanner.AddException(ex);
			}
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
							errorBanner.AddHtmlErrorText(ErrorType.Warning, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsNotEnabled, Server.HtmlEncode(destinationUrl)));
							DisplayAddSite();
							return;
						}

						// check if the user is an instructor on that site
						if (!destinationSlkStore.IsInstructor(destinationWeb))
						{
							errorBanner.AddHtmlErrorText(ErrorType.Warning, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsNotInstructor, Server.HtmlEncode(destinationUrl)));
							DisplayAddSite();
							return;
						}

						// check if the site is already in the list
						ReadOnlyCollection<SlkUserWebListItem> userWebList = SlkStore.GetUserWebList();
						foreach (SlkUserWebListItem webListItem in userWebList)
						{
							if (destinationWeb.ID.Equals(webListItem.SPWebGuid))
							{
								errorBanner.AddHtmlErrorText(ErrorType.Info, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsAlreadyInList, Server.HtmlEncode(destinationWeb.Title)));
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
				errorBanner.AddHtmlErrorText(ErrorType.Warning, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
			}
			catch (UnauthorizedAccessException)
			{
				// the user doesn't have permission to access this site, so show an error message
				errorBanner.AddHtmlErrorText(ErrorType.Warning, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
			}
			catch (FileNotFoundException)
			{
				errorBanner.AddHtmlErrorText(ErrorType.Warning, string.Format(CultureInfo.CurrentCulture, AppResources.ActionsInvalidSite, Server.HtmlEncode(txtNewSite.Text)));
			}
			finally
			{
				SPSecurity.CatchAccessDeniedException = previousValue;
			}
			if (showAddSite)
				DisplayAddSite();
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
		/// Initializes <c>VersionId</c>, <c>SPFile</c>, <c>SPListItem</c>, and <c>SPList</c>.
		/// </summary>
		private void LoadObjects()
		{
			try
			{
				m_spList = SPWeb.Lists[ListId];
				m_spListItem = m_spList.GetItemById(ItemId);
				// reject folders
				if (m_spListItem.FileSystemObjectType.Equals(SPFileSystemObjectType.Folder))
				{
					throw new SafeToDisplayException(AppResources.ActionsItemIsFolder);
				}
				// reject anything but a file
				if (!m_spListItem.FileSystemObjectType.Equals(SPFileSystemObjectType.File))
				{
					throw new SafeToDisplayException(AppResources.ActionsItemNotFound);
				}
				m_spFile = m_spListItem.File;
			}
			catch (SPException)
			{
				// The list isn't found
				throw new SafeToDisplayException(AppResources.ActionsItemNotFound);
			}
			catch (ArgumentException)
			{
				// The file isn't found
				throw new SafeToDisplayException(AppResources.ActionsItemNotFound);
			}

			m_versionId = m_spFile.UIVersion;
		}

		/// <summary>
		/// Initializes <c>Location</c> and <c>NonELearning</c>.
		/// </summary>
		private void LoadSlkObjects()
		{
			// set <location> to the SharePointPackageStore location string for the package specified
			// by <spWeb> and <spFile>; note that this package may not exist yet in the PackageStore,
			// but if it does than <location> will be its location string
			SharePointFileLocation spFileLocation = new SharePointFileLocation(SPWeb, SPFile.UniqueId, VersionId);
			m_location = spFileLocation.ToString();

			// set <nonELearning> to true if this file isn't an e-learning package type
			using (SharePointPackageReader spReader = new SharePointPackageReader(SlkStore.SharePointCacheSettings, spFileLocation, false))
			{
				m_nonELearning = PackageValidator.Validate(spReader).HasErrors;
			}
		}

		/// <summary>
		/// Copies localizable strings from string resources to UI.
		/// </summary>
		private void SetResourceText()
		{
			pageTitle.Text = AppResources.ActionsPageTitle;
			pageDescription.Text = AppResources.ActionsPageDescription;
			lblOrganization.Text = AppResources.ActionslblOrganization;
			lblWhatHeader.Text = AppResources.ActionslblWhatHeader;
			lblSelfAssignHeader.Text = AppResources.ActionslblSelfAssignHeader;
			lblSelfAssignAssign.Text = AppResources.ActionslblSelfAssignAssign;
			lnkAssignSelf.Text = AppResources.ActionslnkAssignSelf;
			lblAssignSelf.Text = AppResources.ActionslblAssignSelf;
			lblChoose.Text = AppResources.ActionslblChoose;
			lnkMRUAddSite.Text = AppResources.ActionslnkMRUAddSite;
			lblMRUAddress.Text = AppResources.ActionslblMRUAddress;
			lnkMRUTestLink.Text = AppResources.ActionslnkMRUTestLink;
			addButton.Text = AppResources.ActionsbtnAdd;
			newSiteRequired.ErrorMessage = AppResources.ActionsSiteRequired;
		}


		private class WebListItem : SlkUserWebListItem, IComparable
		{
			private string m_url;
			private string m_title;

			internal string Url
			{
				[DebuggerStepThrough]
				get
				{
					return m_url;
				}
			}

			/// <summary>
			/// The plain text (not HTML) to display for this page.
			/// </summary>
			internal string Title
			{
				[DebuggerStepThrough]
				get
				{
					return m_title;
				}
			}

			internal WebListItem(Guid spSiteGuid, Guid spWebGuid, DateTime lastAccessTime, string url, string title)
				: base(spSiteGuid, spWebGuid, lastAccessTime)
			{
				m_url = url;
				m_title = title;
			}

			internal WebListItem(SlkUserWebListItem item, string url, string title)
				: base(item.SPSiteGuid, item.SPWebGuid, item.LastAccessTime)
			{
				m_url = url;
				m_title = title;
			}

			#region IComparable Members

			int IComparable.CompareTo(object obj)
			{
				return StringComparer.CurrentCultureIgnoreCase.Compare(Title, ((WebListItem)obj).Title);
			}

			#endregion
		}
	}
}

