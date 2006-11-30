/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Microsoft.SharePointLearningKit;

namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  ButtonToolbar : Slk Custom Control
    ///  Control used to group and render SlkButton Controls as a formatted Toolbar.
    ///  For example, in the Lobby page, this is the toolbar with "Begin Assignment",
    ///  "Submit", etc.
    ///  usage: &lt;{0}:ButtonToolbar runat="server" ID="toolBar" &gt;
    ///         &lt;{0}:SlkButton runat="server" ID="slkButton1" &gt;&lt;/{0}:SlkButton&gt;
    ///         &lt;{0}:SlkButton runat="server" ID="slkButton2" &gt;&lt;/{0}:SlkButton&gt;
    ///         &lt;/{0}:ButtonToolbar&gt;
    /// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Toolbar"), DesignerAttribute(typeof(ControlDesigner)),
    ToolboxData("<{0}:ButtonToolbar runat=\"server\"></{0}:ButtonToolbar>")]
    public class ButtonToolbar : PlaceHolder
    {
        protected override void Render(HtmlTextWriter writer)
        {
            string className = "ms-toolbar";
            string cellPadding = "2";
            string width = "100%";

            //Add Attributes for the <Table> tag
            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, cellPadding);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, width);
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            using (new HtmlBlock(HtmlTextWriterTag.Table, 0, writer))
            {
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                {
                    //Get All the Child Controls nested in Panel and Render them
                    bool skipSeparator = true; // no separator on first item
                    foreach (Control control in Controls)
                    {
                        //Check if this is a LiteralControl; if so, render as-is
                        if (control is LiteralControl)
                        {
                            //Render the Literal Control
                            control.RenderControl(writer);
                        }
                        else
                        {
                            if (control.Visible)
                            {
                                if (!skipSeparator)
                                {
                                    //To Render a TD Separater 
                                    //<td class=ms-separator>&nbsp;</td>

                                    className = "ms-separator";
                                    //Add Attributes for the <TD> tag
                                    writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
                                    {
                                        writer.Write("|");
                                    }
                                }
                                else
                                    skipSeparator = false;

                                //Render the Child Controls
                                WebControl webControl = control as WebControl;
                                if (webControl != null)// e.g. if it's an HtmlControl
                                {
                                    webControl.CssClass = "ms-toolbar";
                                }
                                RenderChildControl(writer, control);
                            }
                        }
                    }

                    className = "ms-toolbar";
                    width = "99%";
                    //Add Attributes for the <TD> tag
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, width);
                    writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "1");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
                    {
                        //Add Attributes for the <Img> tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, Constants.BlankGifUrl);
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "1");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "18");
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, writer);
                    }
                }//</tr>
            }//</table>
        }

        private static void RenderChildControl(HtmlTextWriter htmlTextWriter, Control control)
        {
            string className = "ms-toolbar";
            //Add Attributes for the <TD> tag
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, className);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "1");
            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
            {
                control.RenderControl(htmlTextWriter);
            }
        }
    }
}