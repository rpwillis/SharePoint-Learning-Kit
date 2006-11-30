/* Copyright (c) Microsoft Corporation. All rights reserved. */

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
using Microsoft.LearningComponents.Frameset;
using Resources;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit.Frameset
{
    public class ClearContent : FramesetPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public static string PleaseWaitHtml
        {
            get  {  return ResHelper.GetMessage(FramesetResources.CON_PleaseWait);   }
        }

        public static  string UnexpectedErrorTitleHtml
        {
            get { return HttpUtility.HtmlEncode(FramesetResources.FRM_UnknownExceptionTitle); }
        }

        public static string UnexpectedErrorMsgHtml
        {
            get { return HttpUtility.HtmlEncode(ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedErrorNoException));  }
        }
    }
}