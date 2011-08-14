/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using Resources;

namespace Microsoft.LearningComponents.Frameset
{

    public partial class Frameset_NavOpen : PageHelper
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        #region called from aspx
        public static string NextTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_NextTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        public static string PreviousTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_PrevTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        public static string SaveTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_SaveTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }

        public static string MinimizeTitleHtml
        {
            get
            {
                PlainTextString titleTxt = new PlainTextString(ResHelper.GetMessage(FramesetResources.NAV_MinimizeTitle));
                return new HtmlString(titleTxt).ToString();
            }
        }
        #endregion
    }
}