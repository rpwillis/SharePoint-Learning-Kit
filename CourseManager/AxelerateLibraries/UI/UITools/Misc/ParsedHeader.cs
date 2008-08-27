using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerUITools.WebParts;
using System.Xml;
using System.Collections;
using Axelerate.BusinessLayerUITools.BaseClasses;
using System.Web.UI.WebControls;

namespace Axelerate.BusinessLayerUITools
{
    class ParsedHeader : ParsedColumn
    {
        int columnNumber;

        public ParsedHeader()
        {
        }

        public ParsedHeader(wptGrid Parent, XmlNode Node, int col)
        {
            gNode = Node;
            m_Parent = Parent;
            columnNumber = col;
        }

        public ParsedHeader(wptGrid Parent, XmlNode Node)
        {
            gNode = Node;
            m_Parent = Parent;
        }

        public ParsedHeader(wptGrid Parent, XmlNode Node, Hashtable has)
        {
            gNode = Node;
            hash = has;
        }

        internal override void PropertyType(System.Web.UI.WebControls.GridViewRow container, Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase bussinesObj, System.Web.UI.WebControls.Panel literal)
        {
            if (gNode.Attributes["HeaderPropertyType"] != null)
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
                if (gNode.Attributes["HeaderPropertyType"] != null)
                {
                    switch (gNode.Attributes["HeaderPropertyType"].Value.ToUpper())
                    {
                        case "TEXT":
                            Label text = new Label();
                            text.Text = gNode.Attributes["Header"].Value;
                            literal.Controls.Add(text);
                            break;
                        case "HTML":
                            literal.Controls.Add(parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                                clsCtrlWebPartBase.FactContentType.HTML, gNode.Attributes["Header"].Value, link, targetURL));
                            break;
                        case "PROGRESSBAR":
                            literal.Controls.Add(parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                                clsCtrlWebPartBase.FactContentType.ProgressBar, gNode.Attributes["Header"].Value, link, targetURL));
                            break;
                        case "IMAGE":

                            literal.Controls.Add(parser.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                                clsCtrlWebPartBase.FactContentType.Image, gNode.Attributes["Header"].Value, link, targetURL));
                            break;
                        case "ACTIONS":
                            if (gNode.HasChildNodes)
                            {
                                foreach (XmlNode childnode in gNode.ChildNodes)
                                {
                                    if (childnode.Name.ToLower() == "header")
                                    {
                                        foreach (XmlNode node in childnode)
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
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
