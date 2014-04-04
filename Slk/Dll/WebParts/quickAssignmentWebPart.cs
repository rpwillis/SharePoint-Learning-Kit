using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>The type of usage.</summary>
    public enum QuickAssignmentMode
    {
        /// <summary>Only show the title and move on to choosing the site on a separate page.</summary>
        TitleOnly,
        /// <summary>ONly show the title, but assign to this site.</summary>
        TitleOnlyForThisSite,
        /// <summary>Choose the site in the web part.</summary>
        ChooseSite
    }

    /// <summary>The quick assignment web part.</summary>
    public class QuickAssignmentWebPart : WebPart
    {
        TextBox titleBox;
        Button submit;
        DropDownList sites;
        UserWebList webList;
        bool show;
        bool showEvaluated;
        bool errorOccurred;
        SlkCulture culture = new SlkCulture();

#region properties
        ///<summary>The license for the webpart.</summary>
        ///<value>The license.</value>
        [WebBrowsable(),
         AlwpWebDisplayName("QuickAssignmentTypeDisplayName"),
         AlwpWebDescription("QuickAssignmentTypeDescription"),
         SlkCategory(),
         Personalizable(PersonalizationScope.Shared)
         ]
        public QuickAssignmentMode Mode { get; set; }

        ///<summary>The license for the webpart.</summary>
        ///<value>The license.</value>
        [WebBrowsable(),
         AlwpWebDisplayName("QuickAssignmentAlwaysShowDisplayName"),
         AlwpWebDescription("QuickAssignmentAlwaysShowDescription"),
         SlkCategory(),
         Personalizable(PersonalizationScope.Shared)
         ]
        public bool AlwaysShow { get; set; }
#endregion properties

#region constuctors
        /// <summary>Initializes a new instance of <see cref="QuickAssignmentWebPart"/>.</summary>
        public QuickAssignmentWebPart()
        {
            Mode = QuickAssignmentMode.TitleOnly;
        }
#endregion constuctors

#region protected methods
        /// <summary>Creates the child controls.</summary>
        protected override void CreateChildControls()
        {
            try
            {
                if (Show())
                {
                    titleBox = new TextBox();
                    titleBox.MaxLength = 100;
                    titleBox.Rows = 2;
                    titleBox.Width = Unit.Percentage(100); 
                    Controls.Add(titleBox);

                    if (Mode != QuickAssignmentMode.TitleOnly && Mode != QuickAssignmentMode.TitleOnlyForThisSite)
                    {
                        CreateSitesDropDown();
                    }

                    submit = new Button();
                    submit.Text = culture.Resources.QuickAssignmentAssignText;
                    submit.Click += AssignClick;
                    submit.CssClass="ms-ButtonHeightWidth";
                    Controls.Add(submit);
                }
            }
            catch (SafeToDisplayException e)
            {
                errorOccurred = true;
                Literal literal = new Literal();
                literal.Text = string.Format(CultureInfo.InvariantCulture, "<p class=\"ms-formvalidation\">{0}</p>", e.Message);
                Controls.Add(literal);
            }

            base.CreateChildControls();
        }

        /// <summary>Renders the web part.</summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (errorOccurred)
            {
                base.RenderContents(writer);
            }
            else
            {
                if (Show())
                {
                    writer.Write("<table class='ms-formtable' width='100%' cellspacing='0' cellpadding='0' border='0' style='margin-top: 8px;'>");
                    RenderFormLine(writer, culture.Resources.QuickAssignmentLabelTitle, titleBox);
                    if (sites != null)
                    {
                        RenderFormLine(writer, culture.Resources.QuickAssignmentLabelSite, sites);
                    }
                    writer.Write("</table>");
                    submit.RenderControl(writer);
                }
                else if (WebPartManager.DisplayMode == WebPartManager.BrowseDisplayMode)
                {
                    writer.Write("<style type='text/css'>#MSOZoneCell_WebPart{0} {1}</style>", ClientID, "{display:none;}");
                }
            }
        }

        void RenderFormLine(HtmlTextWriter writer, string label, WebControl control)
        {
            writer.Write("<tr><td class='ms-formlabel' nowrap='true' valign='top'><h3 class='ms-standardheader'><nobr>");
            writer.Write(label);
            writer.Write("</nobr></h3></td>");
            writer.Write("<td class='ms-formbody' valign='top'>");
            control.RenderControl(writer);
            writer.Write("</td></tr>");
        }
#endregion protected methods

#region click event
        /// <summary>The action on clicking assign.</summary>
        public void AssignClick(object sender, EventArgs e)
        {
            try
            {
                string webUrl = FindSelectedWeb();
                string urlFormat = "{0}{1}{2}.aspx?Location={3}&Title={4}";
                string encodedTitle = HttpUtility.UrlEncode(titleBox.Text);
                string page = "assignmentproperties";

                if (Mode == QuickAssignmentMode.TitleOnly)
                {
                    page = "actions";
                }

                string url = String.Format(CultureInfo.InvariantCulture, urlFormat, webUrl, Constants.SlkUrlPath, page, Package.NoPackageLocation, encodedTitle);

                Page.Response.Redirect(url, true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
        }
#endregion click event

#region private methods
        string FindSelectedWeb()
        {
            if (sites != null && sites.SelectedIndex > -1)
            {
                Guid webId = new Guid(sites.SelectedItem.Value);
                foreach (WebListItem item in webList.Items)
                {
                    if (item.SPWebGuid == webId)
                    {

                        try
                        {
                            using (SPSite site = new SPSite(item.SPSiteGuid, SPContext.Current.Site.Zone))
                            {
                                using (SPWeb web = site.OpenWeb(item.SPWebGuid))
                                {
                                    return SanitizeUrl(web.ServerRelativeUrl);
                                }
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // the user doesn't have permission to access this site, so ignore it
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            // the site doesn't exist
                        }
                    }
                }

            }

            return SanitizeUrl(SPContext.Current.Web.ServerRelativeUrl);
        }

        private string SanitizeUrl(string serverRelativeUrl)
        {
            if (serverRelativeUrl == "/")
            {
                return string.Empty;
            }
            else
            {
                return serverRelativeUrl;
            }
        }

        void CreateSitesDropDown()
        {
            SPWeb web = SPContext.Current.Web;
            SlkStore store = SlkStore.GetStore(web);
            webList = new UserWebList(store, web);

            sites = new DropDownList();
            foreach (WebListItem item in webList.Items)
            {
                sites.Items.Add(new ListItem(item.Title, item.SPWebGuid.ToString()));
            }

            Controls.Add(sites);
        }

        bool Show()
        {
            if (showEvaluated == false)
            {
                if (AlwaysShow)
                {
                    show = true;
                }
                else
                {
                    SPWeb web = SPContext.Current.Web;
                    SlkStore store = SlkStore.GetStore(web);
                    show =  store.IsInstructor(web);
                }

                showEvaluated = true;
            }

            return show;
        }
#endregion private methods
    }
}
