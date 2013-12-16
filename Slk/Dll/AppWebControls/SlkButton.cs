/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Microsoft.SharePointLearningKit;
using System.Web.UI.HtmlControls;

namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  SlkButton : Slk Custom Control
    ///  Control used to add an image and link pair to a ButtonToolbar.  For example,
    ///  the "Begin Assignment" button in the Lobby page.
    ///  usage: &lt;{0}:SlkButton runat="server" ID="slkButton" &gt;&lt;/{0}:SlkButton&gt;
    /// </summary>
    [DesignerAttribute(typeof(ControlDesigner)),
    ToolboxData("<{0}:SlkButton runat=\"server\"></{0}:SlkButton>")]
    public class SlkButton : WebControl, IPostBackEventHandler
    {

        private string m_imageUrl = string.Empty;
        private string m_clientClick = string.Empty;
        private string m_text = string.Empty;
        private string m_navigateUrl = string.Empty;
        private string m_target = string.Empty;

        /// <summary>
        /// The URL of the icon-like image that optionally appears in the button.
        /// String.Empty if none.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string ImageUrl
        {
            get { return m_imageUrl; }
            set { m_imageUrl = value; }
        }

        /// <summary>
        /// The plain text (not HTML) of the label of the button.
        /// </summary>
        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }
        
        /// <summary>
        /// The optional script code that gets executed in the browser when the user clicks the button.
        /// </summary>
        public string OnClientClick
        {
            get { return m_clientClick; }
            set { m_clientClick = value; }
        }

        /// <summary>
        /// The URL that is navigated to when the user clicks the button.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string NavigateUrl
        {
            get { return m_navigateUrl; }
            set { m_navigateUrl = value; }
        }

        /// <summary>
        /// If specified, this is the name of the frame which <c>NavigateUrl</c> will navigate in.
        /// If not specified, the default is the current window.
        /// </summary>
        public string Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        /// <summary>The click event handler.</summary>
        public event EventHandler Click;

        /// <summary>
        /// Server-side on-click handler.
        /// </summary>
        /// 
        /// <param name="e">Event arguments.</param>
        ///
        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        /// <summary>See <see cref="Page.Render"/>.</summary>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 0, writer))
            {
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                {
                    using (GetTd(writer))
                    {
                        using (GetAnchor(writer))
                        {
                            using (GetImage(writer))
                            { }
                        }
                    }

                    using (GetTd(writer))
                    {
                        using (GetAnchor(writer))
                        {
                            writer.Write(SlkUtilities.GetHtmlEncodedText(Text));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a new <c>HtmlBlock</c> which generates a "&lt;td&gt;" element for use within
        /// the toolbar.
        /// </summary>
        /// 
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private static HtmlBlock GetTd(HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, Constants.ToolbarClass);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "1");
            HtmlBlock block = new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter);
            return block;
        }

        /// <summary>
        /// Returns a new <c>HtmlBlock</c> which generates a "&lt;a&gt;" element for use within
        /// the toolbar.  Clicking on the anchor executes the toolbar behavior for that button.
        /// </summary>
        /// 
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private HtmlBlock GetAnchor(HtmlTextWriter htmlTextWriter)
        {
            // If NavigateUrl and Click are both set, NavigateUrl will have precedence.
            string href = "javascript:";
            if (AccessKey.Length != 0)
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
			if (Enabled)
			{
				if (Click != null)
                                {
                                    href += Page.ClientScript.GetPostBackEventReference(this, "") + ";";
                                }

				if (OnClientClick != null && OnClientClick.Length != 0)
                                {
                                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClientClick);
                                }

				if (Target.Length != 0)
                                {
                                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Target, Target);
                                }

				if (NavigateUrl != null && NavigateUrl.Length != 0)
                                {
                                    href = NavigateUrl;
                                }

				htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Href, href);
			}
			else
			{
				htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
			}
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
			htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
            HtmlBlock block = new HtmlBlock(HtmlTextWriterTag.A, 0, htmlTextWriter);
            return block;
        }

        /// <summary>
        /// Returns a new <c>HtmlBlock</c> which generates a "&lt;img&gt;" element for use within
        /// the toolbar.  This is the icon-like image.
        /// </summary>
        /// 
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private HtmlBlock GetImage(HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Src, ImageUrl);
            if (ToolTip.Length != 0)
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Alt, ToolTip);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "border-width:0px;");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "16");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "16");
            HtmlBlock block = new HtmlBlock(HtmlTextWriterTag.Img, 0, htmlTextWriter);
            return block;
        }

        #region IPostBackEventHandler Members

        /// <summary>The post back event handler.</summary>
        public void RaisePostBackEvent(string eventArgument)
        {
            OnClick(new EventArgs());
        }

        #endregion
    }
}
