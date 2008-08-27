using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Axelerate.BusinessLayerUITools.Misc
{
    class ColGroup : TableRow
    {
        public int Span
        {
            get
            {
                if (ViewState["span"] == null)
                {
                    ViewState["span"] = 0;
                }
                return (int)ViewState["span"];
            }
            set
            {
                ViewState["span"] = value;
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            writer.AddStyleAttribute("bgcolor", this.BackColor.Name);
            writer.AddAttribute("span", this.Span.ToString());
            writer.AddAttribute("width", this.Width.Value.ToString());
            if (!this.Visible)
            {
                writer.AddStyleAttribute("display", "none");
            }
            writer.AddStyleAttribute("valign", this.VerticalAlign.ToString());
            writer.AddAttribute("title", this.ToolTip);
            writer.AddAttribute("style", this.Style.Value);
            writer.AddAttribute("id", this.ClientID);
            writer.AddStyleAttribute("align", this.HorizontalAlign.ToString());
            //writer.WriteAttribute("height", this.Height.Value.ToString());
            writer.AddAttribute("class", this.CssClass);
            writer.AddStyleAttribute("border-style", this.BorderStyle.ToString());
            writer.AddStyleAttribute("border-color", this.BorderColor.Name);
            Attributes.Render(writer);
            writer.RenderBeginTag("COLGROUP");
            foreach (ColTag cel in Cells)
            {
                cel.RenderControl(writer);
            }
            writer.RenderEndTag();
        }
        public override void RenderControl(HtmlTextWriter writer)
        {
            Render(writer);
        }
    }
}
