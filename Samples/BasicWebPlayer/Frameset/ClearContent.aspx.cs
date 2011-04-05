/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Resources;

namespace Microsoft.LearningComponents.Frameset
{
    public partial class Frameset_ClearContent : BwpFramesetPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string PleaseWaitHtml
        {
            get
            {
                return ResHelper.GetMessage(FramesetResources.CON_PleaseWait);
            }
        }

        public string UnexpectedErrorTitleHtml
        {
            get
            {
                return HttpUtility.HtmlEncode(ResHelper.GetMessage(FramesetResources.FRM_UnknownExceptionTitle));
            }
        }

        public string UnexpectedErrorMsgHtml
        {
            get
            {
                return HttpUtility.HtmlEncode(ResHelper.GetMessage(FramesetResources.FRM_UnexpectedErrorNoException));
            }
        }
    }
}