/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.LearningComponents.Frameset;
using Resources;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit.Frameset
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nav")]
    public class NavClosed : FramesetPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        #region called from aspx
        public static string NextTitleHtml
        {
            get
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_NextTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        public static string PreviousTitleHtml
        {
            get
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_PrevTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        public static string SaveTitleHtml
        {
            get
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_SaveTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }

        public static string MaximizeTitleHtml
        {
            get
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_MaximizeTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        #endregion
    }
}