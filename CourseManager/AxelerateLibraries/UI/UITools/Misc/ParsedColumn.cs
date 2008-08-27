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
using Axelerate.BusinessLayerUITools.Common;

namespace Axelerate.BusinessLayerUITools
{
    public class ParsedColumn : ITemplate
    {
        internal wptGrid m_Parent;
        internal XmlNode gNode;
        internal Hashtable hash;

        public ParsedColumn()
        {
        }

        public ParsedColumn(wptGrid Parent, XmlNode Node)
        {
            gNode = Node;
            m_Parent = Parent;
        }

        public ParsedColumn(wptGrid Parent, XmlNode Node, Hashtable has)
        {
            gNode = Node;
            hash = has;
        }

        public void InstantiateIn(Control container)
        {
            Panel literal = new Panel();
            literal.DataBinding += new EventHandler(this.OnDataBinding);
            container.Controls.Add(literal);
        }

        public void OnDataBinding(object sender, EventArgs e)
        {
            Panel literal = (Panel)sender;
            literal.Width = new Unit("100%");
            GridViewRow container = (GridViewRow)literal.NamingContainer;
            BLBusinessBase bussinesObj = ((BLBusinessBase)container.DataItem);

            if (gNode.Attributes["SelectRow"] != null)
            {
                if (Convert.ToBoolean(gNode.Attributes["SelectRow"].Value))
                {
                    CheckBox check = new CheckBox();
                    check.Checked = Convert.ToBoolean(hash[bussinesObj.UniqueString]);
                    literal.Controls.Add(check);
                }
            }

            PropertyType(container, bussinesObj, literal);
        }

        internal virtual void PropertyType(GridViewRow container, BLBusinessBase bussinesObj, Panel literal)
        {
            if (gNode.Attributes["ColumnPropertyType"] != null)
            {
                bool link = false;
                string targetURL = "";
                if (gNode.Attributes["IsLink"] != null)
                {
                    link = System.Convert.ToBoolean(gNode.Attributes["IsLink"].Value);
                }
                if (gNode.Attributes["TargetURL"] != null)
                {
                    targetURL = gNode.Attributes["TargetURL"].Value;
                }
                clsCtrlWebPartBase parser = new wptGrid();
                WebControl RenderedControl = null;
                string tooltipvalue = "";
                switch (gNode.Attributes["ColumnPropertyType"].Value.ToUpper())
                {
                    case "PROPERTY":
                        String value = "";
                        if (gNode.Attributes["ValueFormat"] != null)
                        {
                            value = "{" + gNode.Attributes["Content"].Value + ", '" + gNode.Attributes["ContentFormat"].Value + "'}";
                        }
                        else
                        {
                            value = "{" + gNode.Attributes["Content"].Value + "}";
                        }
                        int maxchars = -1;
                        if (gNode.Attributes["MaxChars"] != null)
                        {
                            maxchars = Convert.ToInt32(gNode.Attributes["MaxChars"].Value);
                        }
                        RenderedControl = (WebControl)parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.HTML, value, link, targetURL, maxchars);
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            tooltipvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, "GUID");
                            RenderedControl.ToolTip = tooltipvalue;
                            literal.ToolTip = tooltipvalue;
                        }
                        literal.Controls.Add(RenderedControl);
                        break;
                    case "HTML":
                        RenderedControl = (WebControl)parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.HTML, gNode.Attributes["Content"].Value, link, targetURL);
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            tooltipvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, "GUID");
                            RenderedControl.ToolTip = tooltipvalue;
                            literal.ToolTip = tooltipvalue;
                        }
                        literal.Controls.Add(RenderedControl);
                        break;
                    case "PROGRESSBAR":
                        RenderedControl = (WebControl)parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.ProgressBar, gNode.Attributes["Content"].Value, link, targetURL);
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            tooltipvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, "GUID");
                            RenderedControl.ToolTip = tooltipvalue;
                            literal.ToolTip = tooltipvalue;
                        }
                        literal.Controls.Add(RenderedControl);
                        break;
                    case "IMAGE":
                        RenderedControl = (WebControl)parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.Image, gNode.Attributes["Content"].Value, link, targetURL);
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            tooltipvalue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, "GUID");
                            RenderedControl.ToolTip = tooltipvalue;
                            literal.ToolTip = tooltipvalue;
                        }
                        literal.Controls.Add(RenderedControl);
                        break;
                    case "ACTIONS":
                        foreach (XmlNode node in gNode)
                        {
                            switch (node.Name.ToUpper())
                            {
                                case "TOOLBAR":
                                    m_Parent.CreateToolBar(node, literal, bussinesObj, false, null, container.DataItemIndex);
                                    break;
                                case "MENU":
                                    m_Parent.CreateMenu(node, literal, bussinesObj, false, null, container.DataItemIndex);
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}
