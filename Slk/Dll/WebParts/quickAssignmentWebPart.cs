using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>The quick assignment web part.</summary>
    public class QuickAssignmentWebPart : WebPart
    {
        TextBox titleBox;
        Button submit;
        DropDownList sites;
        UserWebList webList;

#region protected methods
        /// <summary>Creates the child controls.</summary>
        protected override void CreateChildControls()
        {
            titleBox = new TextBox();
            titleBox.MaxLength = 100;
            titleBox.Rows = 2;
            Controls.Add(titleBox);

            SPWeb web = SPContext.Current.Web;
            SlkStore store = SlkStore.GetStore(web);
            webList = new UserWebList(store, web);

            sites = new DropDownList();
            foreach (WebListItem item in webList.Items)
            {
                sites.Items.Add(new ListItem(item.Title, item.SPWebGuid.ToString()));
            }

            Controls.Add(sites);

            submit = new Button();
            submit.Text = "Assign";
            submit.Click += AssignClick;
            Controls.Add(submit);

            base.CreateChildControls();
        }
#endregion protected methods

#region click event
        /// <summary>The action on clicking assign.</summary>
        public void AssignClick(object sender, EventArgs e)
        {
            try
            {
                string webUrl = FindSelectedWeb();
                string url = SlkUtilities.UrlCombine(webUrl, "_layouts/SharePointLearningKit/AssignmentProperties.aspx");
                string encodedTitle = HttpUtility.UrlEncode(titleBox.Text);
                url = String.Format(CultureInfo.InvariantCulture, "{0}?Location={1}&Title={2}", url, AssignmentProperties.noPackageLocation, encodedTitle);

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
            if (sites.SelectedIndex > -1)
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
                                    return web.ServerRelativeUrl;
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

            return SPContext.Current.Web.ServerRelativeUrl;
        }
#endregion private methods
    }
}
