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
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit.Frameset
{
    /// <summary>The Clear Content Frame.</summary>
    public class ClearContent : FramesetPage
    {
        /// <summary>The Please Wait Html</summary>
        public static string PleaseWaitHtml
        {
            get  
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                return ResHelper.GetMessage(FramesetResources.CON_PleaseWait);   
            }
        }

        /// <summary>The unexpected error title Html</summary>
        public static  string UnexpectedErrorTitleHtml
        {
            get 
            {
                FramesetResources.Culture = LocalizationManager.GetCurrentCulture();
                return HttpUtility.HtmlEncode(FramesetResources.FRM_UnknownExceptionTitle); 
            }
        }

        /// <summary>The unexpected error message Html</summary>
        public static string UnexpectedErrorMsgHtml
        {
            get 
            {
                SlkFrameset.Culture = LocalizationManager.GetCurrentCulture();
                return HttpUtility.HtmlEncode(ResHelper.GetMessage(SlkFrameset.FRM_UnexpectedErrorNoException));  
            }
        }
    }
}
