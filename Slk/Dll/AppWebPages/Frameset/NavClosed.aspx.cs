/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.LearningComponents.Frameset;
using Resources;

namespace Microsoft.SharePointLearningKit.Frameset
{
    /// <summary>The navigation closed frame.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nav")]
    public class NavClosed : FramesetPage
    {
        #region called from aspx
        /// <summary>The html for the next title.</summary>
        public static string NextTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_NextTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }

        /// <summary>The html for the previous title.</summary>
        public static string PreviousTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_PrevTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        /// <summary>The html for the save title.</summary>
        public static string SaveTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_SaveTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }

        /// <summary>The html for the maximize title.</summary>
        public static string MaximizeTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_MaximizeTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        #endregion
    }
}
