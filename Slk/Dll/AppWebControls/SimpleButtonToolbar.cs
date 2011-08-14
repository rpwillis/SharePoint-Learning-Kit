/* Copyright (c) Microsoft Corporation. All rights reserved. */

//SimpleButtonToolbar.cs
//
// Implementation of ServerControl Class SimpleButtonToolbar
//

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePointLearningKit;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Drawing;
using System.Web.UI.Design;
using System.ComponentModel.Design;


namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  SimpleButtonToolbar : Slk Custom Control  Control used to group and render
    ///  Controls as a formatted Toolbar .
    ///  usage: &lt;{0}:SimpleButtonToolbar runat="server" ID="simpleToolBar" &gt; 
    ///            &lt;asp:Button ID="btnOK" runat="server" Text="OK" /&gt;                        
    ///            &lt;asp:Button ID="btnCancel" runat="server" Text="Cancel"/&gt;
    ///         &lt;/{0}:SimpleButtonToolbar&gt;
    /// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Toolbar"), DesignerAttribute(typeof(ControlDesigner)),
     ToolboxData("<{0}:SimpleButtonToolbar runat=\"server\"></{0}:SimpleButtonToolbar>")]
    public class SimpleButtonToolbar : PlaceHolder
    {
        #region Private and Protected Methods

        #region Render
        /// <summary>
        ///  Render the Custom made ControlPanel
        /// </summary>       
        /// <param name="writer">HtmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            //Begin the Custom Rendering of ControlPanel

            //Control Panel  will  render the below Html
            //<table class="ms-formtoolbar" cellpadding="2" cellspacing="0" border="0" width="100%">
            //<tr>      
            //<td width="99%" class="ms-toolbar" nowrap><img src="/_layouts/images/blank.gif" width="1" height="18" alt=""></td>

            string className = "ms-formtoolbar";
            string cellPadding = "2";

            //Add Attributes for the <Table> tag
            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, cellPadding);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            using (new HtmlBlock(HtmlTextWriterTag.Table, 0, writer))
            {
                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                {
                    className = "ms-toolbar";
                    string width = "99%";
                    //Add Attributes for the <TD> tag
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, width);
                    writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");

                    using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
                    {
                        //Add Attributes for the <Img> tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, Constants.BlankGifUrl);
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "1");
                        writer.AddAttribute(HtmlTextWriterAttribute.Height, "18");
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 1, writer);
                    }

                    //Get All the Child Controls nested in Panel and Render them
                    foreach (Control control in Controls)
                    {
                        //To Check if it is LiteralControl Ignore Formating
                        if (typeof(LiteralControl).Equals(control.GetType()))
                        {
                            //Render the Literal Control
                            control.RenderControl(writer);
                        }
                        else
                        {
                            //To Render a TD Separater 
                            //<td class=ms-separator>&nbsp;</td>

                            className = "ms-separator";
                            //Add Attributes for the <TD> tag       
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                            writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
                            {
                                writer.Write("&nbsp;");
                            }

                            //Render the Child Controls
                            RenderChildControl(writer, control);

                        }
                    }
                }//</tr>
            }//</table>            

        }
        #endregion

        #region RenderChildControl
        /// <summary>
        ///  Begin the Rendering of the Custom Child Controls nested in ControlPanel
        /// </summary>          
        /// <param name="writer">HtmlTextWriter</param> 
        /// <param name="control">Control</param>               
        private static void RenderChildControl(HtmlTextWriter writer, Control control)
        {
            //The Child Control will be wrapped with below html

            //<td class="ms-toolbar" nowrap="true">
            //  <table cellpadding="0" cellspacing="0" width="100%">
            //    <tr>
            //      <td align="right" width="100%" nowrap>
            //        <Control> will be rendered Here
            //      </td>
            //    </tr>
            //  </table>
            //</td>           

            string className = "ms-toolbar";
            string width = "100%";
            //Add Attributes for the <TD> tag
            writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
            writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
            {
                //Add Attributes for the <Table> tag 
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, width);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, writer))
                    {
                        //Add Attributes for the <TD> tag 
                        writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, width);
                        writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");

                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, writer))
                        {
                            control.RenderControl(writer);
                        }
                    }
                }
            }


        }
        #endregion

        #endregion
    }
}
