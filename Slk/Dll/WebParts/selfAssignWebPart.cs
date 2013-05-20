using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.WebParts
{
    /// <summary>A web part to show documents which can be self-assigned.</summary>
    public class SelfAssignWebPart : WebPart
    {
        List<string> errors = new List<string>();
        SlkCulture culture = new SlkCulture();

#region constructors
#endregion constructors

#region properties
        ///<summary>The site to get the documents from.</summary>
        [WebBrowsable(),
         AlwpWebDisplayName("SelfAssignSiteUrlDisplayName"),
         AlwpWebDescription("SelfAssignSiteUrlDescription"),
         SlkCategory(),
         Personalizable(PersonalizationScope.Shared)]
        public string SiteUrl { get; set; }

        ///<summary>The list to get the documents from.</summary>
        [WebBrowsable(),
         AlwpWebDisplayName("SelfAssignListNameDisplayName"),
         AlwpWebDescription("SelfAssignListNameDescription"),
         SlkCategory(),
         Personalizable(PersonalizationScope.Shared)]
        public string ListName { get; set; }

        ///<summary>The view to get the documents from.</summary>
        [WebBrowsable(),
         AlwpWebDisplayName("SelfAssignViewNameDisplayName"),
         AlwpWebDescription("SelfAssignViewNameDescription"),
         SlkCategory(),
         Personalizable(PersonalizationScope.Shared)]
        public string ViewName { get; set; }

#endregion properties

#region public methods
#endregion public methods

#region protected methods
        /// <summary>Renders the web part.</summary>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (ValidateProperties())
            {
                SPListItemCollection items = FindDocuments();

                if (items != null && items.Count > 0)
                {
                    writer.Write("<ul class=\"slk-sa\">");

                    foreach (SPListItem item in items)
                    {
                        if (item.File != null)
                        {
                            string url = "{0}/_layouts/SharePointLearningKit/frameset/frameset.aspx?ListId={1}&ItemId={2}&SlkView=Execute&play=true";
                            string serverRelativeUrl = SPControl.GetContextWeb(Context).ServerRelativeUrl;
                            url = string.Format(CultureInfo.InvariantCulture, url, serverRelativeUrl, item.ParentList.ID.ToString("B"), item.ID);

                            string title = item.Title;
                            if (string.IsNullOrEmpty(title))
                            {
                                title = item.Name;
                            }

                            writer.Write("<li><a href=\"{0}\" target=\"_blank\">{1}</a></li>", url, HttpUtility.HtmlEncode(title));
                        }
                    }

                    writer.Write("</table>");
                }
                else
                {
                    writer.Write("<p class=\"ms-vb\">{0}</p>", HttpUtility.HtmlEncode(culture.Resources.SelfAssignPartNoItems));
                }
            }

            RenderErrors(writer);

        }
#endregion protected methods

#region private methods
        bool ValidateProperties()
        {
            if (string.IsNullOrEmpty(ListName))
            {
                errors.Add(culture.Resources.SelfAssignListNameRequired);
                return false;
            }

            return true;
        }

        void RenderErrors(HtmlTextWriter writer)
        {
            if (errors.Count > 0)
            {
                writer.Write("<p class=\"ms-formvalidation\">");

                foreach (string error in errors)
                {
                    writer.Write(HttpUtility.HtmlEncode(error));
                    writer.Write("<br />");
                }

                writer.Write("</p>");
            }
        }

        SPListItemCollection FindDocuments()
        {
            if (string.IsNullOrEmpty(SiteUrl))
            {
                return FindDocuments(SPContext.Current.Web);
            }
            else
            {
                using (SPSite site = new SPSite(SiteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        return FindDocuments(web);
                    }
                }
            }
        }

        SPListItemCollection FindDocuments(SPWeb web)
        {
            SPList list;
            try
            {
                list = web.Lists[ListName];
            }
            catch (SPException)
            {
                errors.Add(culture.Resources.SelfAssignInvalidList);
                return null;
            }

            SPView view;

            if (string.IsNullOrEmpty(ViewName))
            {
                view = list.DefaultView;
            }
            else
            {
                try
                {
                    view = list.Views[ViewName];
                }
                catch (ArgumentException)
                {
                    errors.Add(culture.Resources.SelfAssignInvalidView);
                    return null;
                }
                catch (SPException)
                {
                    errors.Add(culture.Resources.SelfAssignInvalidView);
                    return null;
                }
            }

            SPQuery query = new SPQuery();
            query.Query = view.Query;
            if (view.Scope == SPViewScope.Recursive || view.Scope == SPViewScope.RecursiveAll)
            {
                query.ViewAttributes = "Scope=\"Recursive\"";
            }

            // Cannot use list.GetItems(view) as that limits the columns and may not include the title column
            return list.GetItems(query);
        }
#endregion private methods

#region static members
#endregion static members
    }
}

