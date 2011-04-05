/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

using System;
using Microsoft.LearningComponents;
using System.Globalization;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.LearningComponents.Frameset
{

    public partial class Frameset_MainFrames : BwpFramesetPage
    {
        AttemptItemIdentifier m_attemptId;
        SessionView m_view;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ProcessAttemptIdParameter(false, out m_attemptId))
                return;

            if (!ProcessViewParameter(false, out m_view))
                return;
        }

        #region called from aspx
        public string HiddenFrameUrl
        {
            get
            {
                string strUrl = String.Format(CultureInfo.CurrentCulture, "Hidden.aspx?{0}={1}&{2}={3}&{4}=1",
                    FramesetQueryParameter.View, Convert.ToInt32(m_view),
                    FramesetQueryParameter.AttemptId, m_attemptId.GetKey().ToString(),
                    FramesetQueryParameter.Init);
                UrlString hiddenUrl = new UrlString(strUrl);
                return hiddenUrl.ToAscii();
            }
        }

        public string TocFrameUrl
        {
            get
            {
                string strUrl = String.Format(CultureInfo.CurrentCulture, "Toc.aspx?View={0}&AttemptId={1}", m_view.ToString(), m_attemptId.GetKey().ToString());
                UrlString hiddenUrl = new UrlString(strUrl);
                return hiddenUrl.ToAscii();
            }
        }
        #endregion

    }
}