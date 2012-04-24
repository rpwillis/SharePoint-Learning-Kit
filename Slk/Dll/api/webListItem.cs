using System;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Represents a web site.</summary>
    public class WebListItem : SlkUserWebListItem, IComparable
    {
        /// <summary>The url of the site.</summary>
        public string Url { get; private set; }

        /// <summary>The plain text (not HTML) to display for this page. </summary>
        public string Title { get; private set; }

        internal WebListItem(Guid spSiteGuid, Guid spWebGuid, DateTime lastAccessTime, string url, string title)
            : base(spSiteGuid, spWebGuid, lastAccessTime)
        {
            Url = url;
            Title = title;
        }

        internal WebListItem(SlkUserWebListItem item, string url, string title)
            : base(item.SPSiteGuid, item.SPWebGuid, item.LastAccessTime)
        {
            Url = url;
            Title = title;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            return StringComparer.CurrentCultureIgnoreCase.Compare(Title, ((WebListItem)obj).Title);
        }

        #endregion
    }
}
