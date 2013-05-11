/* Copyright (c) Microsoft Corporation. All rights reserved. */

//TableGrid.cs
//
//Implementation of TableGrid
//
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.SharePointLearningKit;
using System.Drawing;
using System.Web.UI.Design;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Globalization;

namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  Table grid Control used to display content in formatted fixed tds.  A &lt;TableGrid&gt; element must
    /// contain one or more &lt;TableGridRow&gt; elements, which in turn must contain one or more
    /// &lt;TableGridColumn&gt; elements.  &lt;TableGridColumn&gt; implements a single table cell; it can
    /// have a "ColumnType" attribute -- see <c>TableGridColumn</c>.
    /// </summary>
    /// <remarks>
    ///  usage: &lt;{0}:TableGrid ID="tgEmptyTop" runat="server" Width="100%" CellPadding="0" CellSpacing="0"&gt;
    ///                 &lt;{0}:TableGridRow&gt;
    ///                     &lt;{0}:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" 
    ///                             runat = "server"&gt;                                                 
    ///                     &lt;/{0}:TableGridColumn&gt;
    ///                 &lt;/{0}:TableGridRow&gt;
    ///         &lt;{0}:TableGrid&gt;
    /// </remarks>
   [DesignerAttribute(typeof(ControlDesigner)),
   ToolboxData("<{0}:TableGrid runat=\"server\"></{0}:TableGrid>")]
    public class TableGrid : Table
    {       
        /// <summary>Renders the control.</summary>
        protected override void Render(HtmlTextWriter writer)
        {                  
            base.Render(writer);
        }
    }

    /// <summary> TableGridRow Renders </summary>
    public class TableGridRow : TableRow
    {        
        /// <summary>Renders the control.</summary>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }
    }

    /// <summary>
    ///  TableGridColumn Renders &lt;td&gt;&lt;/td&gt; in applied format depends on
    ///  the selected FormType attribute.
    /// </summary>
    /// 
    /// <remarks>
    /// This probably should be called <c>TableGridCell</c> instead of <c>TableGridColumn</c>.
    /// </remarks>
    public class TableGridColumn : TableCell
    {
        #region Private Variables
        /// <summary>
        /// TableGridColumn Column Type
        /// </summary>
        private FormType m_columnType;
        #endregion

        #region Public Accessors
        /// <summary>
        /// Allowed FormType for the ColumnType.
        /// FormLabel : Renders &lt;td valign="top" class="ms-formlabel" width="190"&gt;&lt;/td&gt;
        /// FormBody  : Renders &lt;td valign="top" class="ms-formbody" width="400"&gt;&lt;/td&gt;
        /// FormLine  : Renders &lt;td valign="top" class="ms-formline" width="100%" colspan="2" style="width: 100%;"&gt;        ///                     &lt;/td&gt;
        /// FormBreak : Renders &lt;td valign="top" class="ms-formlabel" width="100%" colspan="2" style="height: 1px;         ///                         width: 100%;"&gt;
        ///                         &lt;img src="/_layouts/images/blank.gif" width="1" height="1" alt="" /&gt;
        ///                     &lt;/td&gt;
        /// SectionLine: Renders &lt;td height="2px" class="ms-sectionline"&gt;
        ///                          &lt;img src="/_layouts/images/blank.gif" width="1" height="1" alt="" /&gt;
        ///                      &lt;/td&gt;
        /// </summary>
        public enum FormType
        {
            /// <summary>A label.</summary>
            FormLabel = 0,
            /// <summary>A body.</summary>
            FormBody,
            /// <summary>A line.</summary>
            FormLine,
            /// <summary>A break.</summary>
            FormBreak,
            /// <summary>An error.</summary>
            FormError,
            /// <summary>A section line.</summary>
            SectionLine,
            /// <summary>A default.</summary>
            FormDefault
        }

        /// <summary>
        /// Decides the FormType to be rendered. 
        /// </summary>
        public FormType ColumnType
        {
            get { return m_columnType; }
            set { m_columnType = value; }
        }

        #endregion

        #region Private and Protected Methods

        #region Render
        /// <summary>
        ///  Render the Custom made TD
        /// </summary>       
        /// <param name="writer">HtmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {                       
            base.Render(writer);      
        }
        #endregion

        #region RenderContents
        /// <summary>
        /// Render TableGridColumn Contents
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param> 
        protected override void RenderContents(HtmlTextWriter writer)
        {
           
            //if FormType is Error render the Error Block 
            
            if (ColumnType == FormType.FormError)
            {
                //place holder Content
                StringBuilder renderedContent = new StringBuilder();
                //Text Writer to write the content to content Render
                using(TextWriter contentWriter = new StringWriter(renderedContent, CultureInfo.CurrentUICulture))
                {
                    using(HtmlTextWriter originalStream = new HtmlTextWriter(contentWriter))
                    {
                        base.RenderContents(originalStream);
                        string renderedHtmlText = renderedContent.ToString();
                        RenderErrorBlock(writer, renderedHtmlText);
                    }
                }
            }
            else
            {
                //if FormType is Break render the image tag with blank gif 
                if (ColumnType == FormType.FormBreak || 
                    ColumnType == FormType.SectionLine)
                {
                    //Add Attributes for the <Img> tag
                    writer.AddAttribute(HtmlTextWriterAttribute.Src,Constants.BlankGifUrl);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "1");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, 
                                                (ColumnType == FormType.SectionLine) ? "1" : "5");
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, "");
                    HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, writer);
                }

                base.RenderContents(writer);
            }
        }

       
        #endregion

        #region RenderErrorBlock
        /// <summary>
        /// Render Error Message with Error Text
        /// </summary>
        /// <param name="writer">HtmlTextWriter</param> 
        /// <param name="renderedContent">Error Block To Be Rendered</param>
        protected static void RenderErrorBlock(HtmlTextWriter writer, string renderedContent)
        {
            if (writer != null)
            {
                //Add Attributes for the <Table> tag 
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formbody");
                using (new HtmlBlock(HtmlTextWriterTag.Table, 0, writer))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                    {
                        //Add Attributes for the <TD> tag 
                        //AddHtmlAttributes(htmlTextWriter);                    
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "1%");
                        writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 0, writer))
                        {
                            //ErrorMessage Image Tag
                            //Add Attributes for the <Img> tag
                            writer.AddAttribute(HtmlTextWriterAttribute.Src, Constants.ImagePath + Constants.WarningIcon);
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "16px");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, writer);
                        }

                        writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 0, writer))
                        {
                            writer.Write(renderedContent);
                        }
                    }
                }
            }        
        }
        #endregion

        #region AddAttributesToRender
        /// <summary>
        ///  Add Attributes To the Custom made TD Tag
        /// </summary>       
        /// <param name="writer">HtmlTextWriter</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            //Add Attributes for the <TD> tag 
            AddHtmlAttributes(writer);
            
            base.AddAttributesToRender(writer);
        }
        #endregion

        #region AddHtmlAttributes
        /// <summary>
        ///  Set the Html Attributes to the main TD Tag
        /// </summary>          
        /// <param name="htmlTextWriter">HtmlTextWriter</param> 
        private void AddHtmlAttributes(HtmlTextWriter htmlTextWriter)
        {
            //set the TD Properties accroding to the formType
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            string width = "";

            switch (ColumnType)
            {
                case FormType.FormLabel:
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formlabel");
                        width = "190";
                        break;
                    }
                case FormType.FormBody :
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formbody");
                        width = "400";
                        break;
                    }
                case FormType.FormLine:
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-formline");
                        break;
                    }
                case FormType.FormBreak:
                    {
                        this.Height = Unit.Pixel(1);                       
                        break;
                    }
                case FormType.FormError:
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "border-top: none");                         
                        break;
                    }
                case FormType.SectionLine:
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-sectionline"); 
                        this.Height = Unit.Pixel(3); 
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
            if (!String.IsNullOrEmpty(width))
            {
                this.Width = Unit.Point(int.Parse(width, CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #endregion

    }
}
