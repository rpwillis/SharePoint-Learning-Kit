/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.LearningComponents.Storage;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using Resources;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Code to use by SLK and BWP frameset TOC files.
    /// </summary>
    public class TocHelper
    {
        LearningSession m_session;
        
        private HttpResponse m_response;
        private HttpResponse Response { get { return m_response; } }
        
        // The text for the submit page link. This is only used in Execute & RandomAccess (SLK only) views.
        private string m_submitPageLinkText;

       /// <summary>Processes the page.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), // catch is to allow shared code between framesets
       SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly"),     // methods are cased as method names 
       SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]      // arguments are validated
        public void ProcessPageLoad(HttpResponse response, 
                                        PackageStore packageStore,
                                        TryGetViewInfo TryGetViewInfo,
                                        TryGetAttemptInfo TryGetAttemptInfo,
                                        ProcessViewRequest ProcessViewRequest,
                                        RegisterError RegisterError,
                                        string submitPageLinkText)
        {
            FramesetUtil.ValidateNonNullParameter("packageStore", packageStore);
            FramesetUtil.ValidateNonNullParameter("TryGetViewInfo", TryGetViewInfo);
            FramesetUtil.ValidateNonNullParameter("TryGetAttemptInfo", TryGetAttemptInfo);
            FramesetUtil.ValidateNonNullParameter("RegisterError", RegisterError);

            m_response = response;
            m_submitPageLinkText = submitPageLinkText;
                        
            AttemptItemIdentifier attemptId;
            SessionView view;

            if (!TryGetViewInfo(true, out view))
                return;

            // This is an attempt-based session
            if (!TryGetAttemptInfo(true, out attemptId))
            {
                return;
            }

            m_session = new StoredLearningSession(view, new AttemptItemIdentifier(attemptId), packageStore);

            // If user cannot access the session, then remove the reference to the session.
            if (!ProcessViewRequest(view, m_session))
                m_session = null;
        }

        /// <summary>
        /// Write TOC Html elements for the table of contents
        /// </summary>
        /// <returns></returns>
        public void TocElementsHtml(HttpRequest Request, string AssignmentGradingViewName)
        {
            // If there was an error on page load, do nothing here.
            if (m_session == null)
                return; 

            TableOfContentsElement toc = m_session.GetTableOfContents(true);    // get toc without sequencing information
            using (HtmlStringWriter sw = new HtmlStringWriter(Response.Output))
            {
                sw.Indent = 2;  // start at table level 2, since that's where we are

                WriteTocEntry(sw, toc);

                string SLKView =  Request.QueryString["SlkView"];
                
                // Write the TOC entry to submit the training, only in Execute view or RandomAccess (SLK only) views

                if ((m_session.View == SessionView.Execute)
                    || ((m_session.View == SessionView.Review) && (SLKView.ToLower() == AssignmentGradingViewName.ToLower())))
                    WriteSubmitPageEntry(sw);
            }
        }


        /// <summary>
        /// Write TOC Html elements for the table of contents
        /// </summary>
        /// <returns></returns>
        /// 
        public void TocElementsHtml()
        {
            // If there was an error on page load, do nothing here.
            if (m_session == null)
                return;

            TableOfContentsElement toc = m_session.GetTableOfContents(true);    // get toc without sequencing information
            using (HtmlStringWriter sw = new HtmlStringWriter(Response.Output))
            {
                sw.Indent = 2;  // start at table level 2, since that's where we are

                WriteTocEntry(sw, toc);

                // Write the TOC entry to submit the training, only in Execute view or RandomAccess (SLK only) views
                if ((m_session.View == SessionView.Execute)
                    || (m_session.View == SessionView.RandomAccess))
                    WriteSubmitPageEntry(sw);
            }
        }

        /// <summary>
        /// Return the version of the frameset files. This is used to compare to the version of the js file, to ensure the js
        /// file is not being cached from a previous version.
        /// </summary>
        public static string TocVersion
        {
            get
            {
                return Regex.Match(typeof(LearningStore).Assembly.FullName, @"\bVersion=(\d+\.\d+\.\d+\.\d+)").Groups[1].Value;
            }
        }

        /// <summary>
        /// Return the message to display if the js version doesn't match the aspx version.
        /// </summary>
        public static string InvalidVersionHtml
        {
            get
            {
                return ResHelper.GetMessage(FramesetResources.TOC_InvalidJsVersion);
            }
        }

        /// <summary>
        /// Recursive function to write an element in to the TOC, along with its children.
        /// </summary>
        /// <param name="sw">The string writer to write the toc entry to.</param>
        /// <param name="currentElement">The element that will be written to the string writer. The element 
        /// and all its children will be written.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is validated
        private void WriteTocEntry(HtmlStringWriter sw, TableOfContentsElement currentElement)
        {
            FramesetUtil.ValidateNonNullParameter("sw", sw);
            FramesetUtil.ValidateNonNullParameter("currentElement", currentElement);

            string activityId = FramesetUtil.GetStringInvariant(currentElement.ActivityId);
            bool elementHasChildren = currentElement.Children.Count > 0;
            HtmlString activityIdHtml = new PlainTextString(activityId).ToHtmlString();
            HtmlString titleHtml = new PlainTextString(currentElement.Title).ToHtmlString();

            // If the current element is visible or is an invisible leaf node, then render it. (If it's an 
            // invisible leaf node, the node will exist but not be visible.)
            if (RenderThisNode(currentElement))
            {
                sw.AddAttribute(HtmlTextWriterAttribute.Class, new PlainTextString("NodeParent"));
                sw.AddAttribute("activityId", activityIdHtml);
                sw.AddAttribute("isValidChoice", (currentElement.IsValidChoiceNavigationDestination ? "true" : "false"));
                sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("div{0}", activityIdHtml));
                sw.RenderBeginTag(HtmlTextWriterTag.Div);   // #Div1

                if (currentElement.IsVisible)
                {
                    sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("icon{0}", activityIdHtml));
                    if (currentElement.HasVisibleChildren)
                        sw.AddAttribute(HtmlTextWriterAttribute.Src, "Theme/MinusBtn.gif");
                    else
                        sw.AddAttribute(HtmlTextWriterAttribute.Src, "Theme/Leaf.gif");
                    sw.AddAttribute(HtmlTextWriterAttribute.Align, "absMiddle");
                    sw.RenderBeginTag(HtmlTextWriterTag.Img);
                    sw.RenderEndTag();

                    sw.AddAttribute(HtmlTextWriterAttribute.Src, "Theme/1px.gif");
                    sw.AddAttribute(HtmlTextWriterAttribute.Align, "absMiddle");
                    sw.RenderBeginTag(HtmlTextWriterTag.Img);
                    sw.RenderEndTag();
                    sw.WriteLine();

                    sw.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("a{0}", activityIdHtml));
                    if (!currentElement.IsValidChoiceNavigationDestination)
                    {
                        sw.AddAttribute(HtmlTextWriterAttribute.Disabled, "true");
                        sw.AddAttribute("class", "disable");
                    }

                    sw.AddAttribute(HtmlTextWriterAttribute.Style,
                                        ResHelper.FormatInvariant("FONT-WEIGHT: normal;visibility:{0}", (currentElement.IsVisible ? "visible" : "hidden")));
                    sw.AddAttribute(HtmlTextWriterAttribute.Title, titleHtml);
                    sw.RenderBeginTag(HtmlTextWriterTag.A);
                    sw.WriteHtml(titleHtml);
                    sw.RenderEndTag();
                }
            }

            // Write sub-elements (regardless of whether or not this node is rendered)
            if (elementHasChildren)
            {
                sw.WriteLine();
                bool clusterStarted = false;
                if (currentElement.IsVisible)
                {
                    sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("divCluster{0}", activityIdHtml));
                    sw.AddAttribute(HtmlTextWriterAttribute.Style, "MARGIN-TOP: 5px; DISPLAY: block; MARGIN-LEFT: 18px;");
                    sw.RenderBeginTag(HtmlTextWriterTag.Div);
                    clusterStarted = true;
                }

                foreach (TableOfContentsElement childElement in currentElement.Children)
                {
                    WriteTocEntry(sw, childElement);
                }

                if (clusterStarted)
                {
                    sw.RenderEndTag(); // end div
                    sw.WriteLine();
                }
            }

            if (RenderThisNode(currentElement))
            {
                sw.RenderEndTag();  // div (see #Div1, above)
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Returns true if this node should be rendered. This indicates nothing about whether or not children of the node should 
        /// be rendered.
        /// </summary>
        private static bool RenderThisNode(TableOfContentsElement element)
        {
            // If this element is visible, then return true
            if (element.IsVisible)
                return true;

            // If this element is a leaf node, the node will be rendered (but not visible)
            if (element.Children.Count == 0)
                return true;

            // In all other cases, don't render this element
            return false;
        }

        /// <summary>
        /// Write an element in to the TOC that allows submitting the attempt.
        /// </summary>
        /// <param name="sw">The string writer to write the toc entry to.</param>
        private void WriteSubmitPageEntry(HtmlStringWriter sw)
        {
            string activityId = "SUBMIT";
            HtmlString activityIdHtml = new PlainTextString(activityId).ToHtmlString();
            HtmlString titleHtml = new PlainTextString(m_submitPageLinkText).ToHtmlString();

            sw.AddAttribute(HtmlTextWriterAttribute.Class, new PlainTextString("NodeParent"));
            sw.AddAttribute("activityId", activityIdHtml);
            sw.AddAttribute("isValidChoice", "true");
            sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("div{0}", activityIdHtml));
            sw.RenderBeginTag(HtmlTextWriterTag.Div);   // #Div1
                      
            sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("icon{0}", activityIdHtml));
            sw.AddAttribute(HtmlTextWriterAttribute.Src, "Theme/Leaf.gif");
            sw.AddAttribute(HtmlTextWriterAttribute.Align, "absMiddle");
            sw.RenderBeginTag(HtmlTextWriterTag.Img);
            sw.RenderEndTag();

            sw.AddAttribute(HtmlTextWriterAttribute.Src, "Theme/1px.gif");
            sw.AddAttribute(HtmlTextWriterAttribute.Align, "absMiddle");
            sw.RenderBeginTag(HtmlTextWriterTag.Img);
            sw.RenderEndTag();
            sw.WriteLine();

            sw.AddAttribute(HtmlTextWriterAttribute.Href, "");
            sw.AddAttribute(HtmlTextWriterAttribute.Id, ResHelper.FormatInvariant("a{0}", activityIdHtml));
            sw.AddAttribute(HtmlTextWriterAttribute.Style, "FONT-WEIGHT: normal;visibility:visible");
            sw.AddAttribute(HtmlTextWriterAttribute.Title, titleHtml);
            sw.RenderBeginTag(HtmlTextWriterTag.A);
            sw.WriteHtml(titleHtml);
            sw.RenderEndTag();
        
            sw.RenderEndTag();  // div (see #Div1, above)
            sw.WriteLine();
        }
    }
}
