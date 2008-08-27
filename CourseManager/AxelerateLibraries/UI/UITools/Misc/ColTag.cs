using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Axelerate.BusinessLayerUITools.Misc
{
    class ColTag : TableCell
    {
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (this.Visible)
            {
                writer.AddStyleAttribute("bgcolor", this.BackColor.Name);
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, this.ColumnSpan.ToString());
                writer.AddAttribute("width", this.Width.ToString());

                //               writer.AddStyleAttribute("display", "none");

                writer.AddStyleAttribute("valign", this.VerticalAlign.ToString());
                writer.AddAttribute("title", this.ToolTip);
                writer.AddAttribute("style", this.Style.Value);
                writer.AddAttribute("id", this.ClientID);
                writer.AddStyleAttribute("align", this.HorizontalAlign.ToString());
                // writer.WriteAttribute("height", this.Height.Value.ToString());
                writer.AddAttribute("class", this.CssClass);
                writer.AddStyleAttribute("border-style", this.BorderStyle.ToString());
                writer.AddStyleAttribute("border-color", this.BorderColor.Name);
                writer.AddStyleAttribute("border-width", this.BorderWidth.Value.ToString());
                if (!this.Wrap)
                {
                    writer.AddAttribute("wrap", "soft");
                }
                else
                {
                    writer.AddAttribute("wrap", "off");
                }
                writer.RenderBeginTag("COL");
                writer.RenderEndTag();
            }
            //           Attributes.Render(writer);
        }
    }
}
