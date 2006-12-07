/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.IO;
using System.Web.UI;
using Resources;



namespace Microsoft.LearningComponents.Frameset
{
    public partial class Frameset_TOC : BwpFramesetPage
    {
        TocHelper m_tocHelper; 

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                m_tocHelper = new TocHelper();
                m_tocHelper.ProcessPageLoad(Response, PStore, ProcessViewParameter, 
                                    ProcessAttemptIdParameter, ProcessViewRequest, RegisterError, 
                                    FramesetResources.TOC_SubmitAttempt);
            }
            catch (Exception ex)
            {
                RegisterError(ResHelper.GetMessage(FramesetResources.FRM_UnexpectedErrorTitle),
                                ResHelper.GetMessage(FramesetResources.FRM_UnexpectedError, HttpUtility.HtmlEncode(ex.Message)), false);
            }
        }

        #region called from aspx
        /// <summary>
        /// Write the html for the table of contents to the response.
        /// </summary>
        /// <returns></returns>
        public void WriteToc()
        {
            m_tocHelper.TocElementsHtml();
        }

        /// <summary>
        /// Return the version of the frameset files. This is used to compare to the version of the js file, to ensure the js
        /// file is not being cached from a previous version.
        /// </summary>
        public static string TocVersion
        {
            get
            {
                return TocHelper.TocVersion;
            }
        }

        /// <summary>
        /// Return the message to display if the js version doesn't match the aspx version.
        /// </summary>
        public static string InvalidVersionHtml
        {
            get
            {
                return TocHelper.InvalidVersionHtml;
            }
        }

        #endregion  // called from aspx
    }
}