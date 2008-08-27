using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Web.UI.WebControls;
using System.Data;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Collections;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.WebParts;
using AjaxControlToolkit;

namespace Axelerate.BusinessLayerUITools
{
    public class TabTemplate : ITemplate
    {
        private XmlNode gNode;
        private wptMultiTabbedSelector m_Parent;
        private Object _BindedCriteria;

        public TabTemplate()
        {
        }

        public TabTemplate(wptMultiTabbedSelector Parent, XmlNode Node, object crit)
        {
            m_Parent = Parent;
            gNode = Node;
            _BindedCriteria = crit;
        }

        public void InstantiateIn(Control container)
        {
            Panel literal = new Panel();
            literal.Width = new Unit("100%");
            Label text = new Label();
            if (gNode.Attributes["Label"] != null)
            {
                text.Text = gNode.Attributes["Label"].Value + " ";
                literal.Controls.Add(text);
            }
            if (gNode.Attributes["MouseOverImageUrl"] != null)
            {
                Axelerate.BusinessLayerUITools.WebControls.ImageButton button = new Axelerate.BusinessLayerUITools.WebControls.ImageButton();
                button = new Axelerate.BusinessLayerUITools.WebControls.ImageButton();
                button.MouseOutImageUrl = gNode.Attributes["MouseOutImageUrl"].Value;
                if (gNode.Attributes["ImageUrl"] != null)
                {
                    button.ImageUrl = gNode.Attributes["ImageUrl"].Value;
                }
                if (gNode.Attributes["MouseOverImageUrl"] != null)
                {
                    button.MouseOverImageUrl = gNode.Attributes["MouseOverImageUrl"].Value;
                }
                if (gNode.Attributes["PressedImageUrl"] != null)
                {
                    button.PressedImageUrl = gNode.Attributes["PressedImageUrl"].Value;
                }
                ButtonAction(button);
                literal.Controls.Add(button);
            }
            else if (gNode.Attributes["ImageUrl"] != null)
            {
                ImageButton button = new ImageButton();
                button.ImageUrl = gNode.Attributes["ImageUrl"].Value;
                ButtonAction(button);
                literal.Controls.Add(button);
            }
            else if (gNode.Attributes["Text"] != null)
            {
                Button button = new Button();
                button.Text = gNode.Attributes["Text"].Value;
                ButtonAction(button);
                literal.Controls.Add(button);
            }
            container.Controls.Add(literal);

        }

        private void ButtonAction(IButtonControl button)
        {
            if (gNode.Attributes["Command"] != null)
            {
                //this.GetType().GetMethod(gnode.Attributes["Command"].Value)
                button.CommandName = gNode.Attributes["Command"].Value;
            }
            if (gNode.Attributes["CommandArgument"] != null)
            {
                button.CommandArgument += gNode.Attributes["CommandArgument"].Value;
            }
            button.Click += new EventHandler(button_Click);
        }

        void button_Click(object sender, EventArgs e)
        {
            m_Parent.DeleteSearch(_BindedCriteria);
        }
    }
}
