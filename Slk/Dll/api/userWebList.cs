using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Represent the user's web list of sites assigned to.</summary>
    public class UserWebList
    {
        List<WebListItem> webList = new List<WebListItem>();

#region constructors
        /// <summary>Initializes a new instance of <see cref="UserWebList"/>.</summary>
        /// <param name="store">The ISlkStore to retrieve the web list from.</param>
        /// <param name="currentSite">The current site.</param>
        public UserWebList(ISlkStore store, SPWeb currentSite)
        {
            LimitedSize = store.Settings.UserWebListMruSize;
            bool addCurrentToList = true;
            ReadOnlyCollection<SlkUserWebListItem> userWebList = store.FetchUserWebList();

            bool previousValue = SPSecurity.CatchAccessDeniedException;
            SPSecurity.CatchAccessDeniedException = false;

            SlkCulture culture = new SlkCulture(currentSite);

            try
            {
                foreach (SlkUserWebListItem item in userWebList)
                {
                    try
                    {
                        using (SPSite site = new SPSite(item.SPSiteGuid, SPContext.Current.Site.Zone))
                        {
                            using (SPWeb web = site.OpenWeb(item.SPWebGuid))
                            {
                                WebListItem listItem;
                                if (item.SPWebGuid == currentSite.ID)
                                {
                                    addCurrentToList = false;
                                    string title = culture.Format("{0} {1}", web.Title, AppResources.ActionslblMRUCurrentSite);
                                    listItem = new WebListItem(item, web.Url, title);
                                }
                                else
                                {
                                    listItem = new WebListItem(item, web.Url, web.Title);
                                }

                                webList.Add(listItem);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // the user doesn't have permission to access this site, so ignore it
                        continue;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // the site doesn't exist
                        continue;
                    }

                    if (addCurrentToList)
                    {
                        string title = culture.Format("{0} {1}", currentSite.Title, AppResources.ActionslblMRUCurrentSite);
                        webList.Add(new WebListItem(currentSite.Site.ID, currentSite.ID, DateTime.Now, currentSite.Url, title));
                    }
                }
            }
            finally
            {
                SPSecurity.CatchAccessDeniedException = previousValue;
            }

            if (addCurrentToList)
            {
                string title = culture.Format("{0} {1}", currentSite.Title, AppResources.ActionslblMRUCurrentSite);
                webList.Add(new WebListItem(currentSite.Site.ID, currentSite.ID, DateTime.Now, currentSite.Url, title));
            }
        }
#endregion constructors

#region properties
        /// <summary>The size of the limited list.</summary>
        public int LimitedSize { get; private set; }

        /// <summary>The list of items.</summary>
        public IEnumerable<WebListItem> Items 
        {
            get { return webList ;}
        }
#endregion properties
    }
}
