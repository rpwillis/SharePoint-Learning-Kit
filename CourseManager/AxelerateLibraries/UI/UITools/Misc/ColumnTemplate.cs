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
    //   [Serializable]
    public class ColumnTemplate
    {
        internal clsBaseLayoutWP m_Parent;
        internal XmlNode gNode;
        internal Hashtable hash;
        internal int index = -1;


        public Unit Width
        {
            get
            {
                if (gNode.Attributes["Width"] != null)
                {
                    return new Unit(gNode.Attributes["Width"].Value);
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool IsHidden
        {
            get
            {
                if (gNode.Attributes["IsHidden"] != null)
                {
                    return Convert.ToBoolean(gNode.Attributes["IsHidden"].Value);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsEditable
        {
            get
            {
                if (gNode.Attributes["EditBehavior"] != null)
                {
                    switch (gNode.Attributes["EditBehavior"].Value)
                    {
                        case "Editable":
                            return true;
                            break;
                        case "NoEditable":
                        default:
                            return false;
                            break;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public string ContentProperty
        {
            get
            {
                switch (gNode.Attributes["ColumnPropertyType"].Value.ToUpper())
                {
                    case "PROPERTY":
                        return gNode.Attributes["Content"].Value;
                    default:
                        return "";
                }
            }
        }

        public string PropertyFriendlyName
        {
            get
            {
                if (gNode.Attributes["Header"] != null)
                {
                    return gNode.Attributes["Header"].Value;
                }
                else
                {
                    return ContentProperty;
                }
            }
        }

        public ColumnTemplate(clsBaseLayoutWP Parent, XmlNode Node, int cIndex): this(Parent,Node, null, cIndex)
        {
        }

        public ColumnTemplate(clsBaseLayoutWP Parent, XmlNode Node, Hashtable htable, int cIndex)
        {
            gNode = Node;
            m_Parent = Parent;
            hash = htable;
            index = cIndex;
        }

        public void InstantiateColumn(WebControl container, BLBusinessBase bussinesObj, int DataItemIndex)
        {
            WebControl literal = (WebControl)container;
            HiddenField hiddenvalue = null;
            if (IsHidden)
            {
                container.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            if (gNode.Attributes["ColumnPropertyType"] != null)
            {
                bool link = false;
                string targetURL = "";
                if (gNode.Attributes["EditBehavior"] != null)
                {
                    switch (gNode.Attributes["EditBehavior"].Value)
                    {
                        case "Editable":
                            container.Attributes.Add("editable", "true");
                            hiddenvalue = new HiddenField();
                            hiddenvalue.ID = container.ID + "_value";
 //                           ((wptHyperGrid)m_Parent).ColumnCollection[index].ValueField = hiddenvalue;
                            ((wptHyperGrid)m_Parent).HiddenValues[container.Attributes["rdk"] + "_" + index.ToString() ] = hiddenvalue;
                            container.Controls.Add(hiddenvalue);
                            break;
                        case "NoEditable":
                        default:
                            container.Attributes.Add("editable", "false");
                            break;
                    }
                }
                if (gNode.Attributes["IsLink"] != null)
                {
                    link = System.Convert.ToBoolean(gNode.Attributes["IsLink"].Value);
                }
                if (gNode.Attributes["TargetURL"] != null)
                {
                    targetURL = gNode.Attributes["TargetURL"].Value;
                }
                switch (gNode.Attributes["ColumnPropertyType"].Value.ToUpper())
                {
                    case "PROPERTY":
                        String value ="";
                        if (gNode.Attributes["DisplayValue"] == null)
                        {
                            value = "{" + gNode.Attributes["Content"].Value + "}";
                        }
                        else
                        {
                            value = gNode.Attributes["DisplayValue"].Value;
                        }
                        Label propertycontrol = new Label();
                        propertycontrol.ID = container.ID + "_lbl";
                        String ParsedValue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, value, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString());
                        if (gNode.Attributes["MaxChars"] != null)
                        {
                            int maxchars = Convert.ToInt32(gNode.Attributes["MaxChars"].Value);
                            if (maxchars < ParsedValue.Length)
                            {
                                ParsedValue = ParsedValue.Substring(0, maxchars) + "...";
                            }
                        }
                        propertycontrol.Text = ParsedValue;
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            try
                            {
                                String TooltipParsedValue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString());
                                propertycontrol.ToolTip = TooltipParsedValue;
                                literal.ToolTip = TooltipParsedValue;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        literal.Controls.Add(propertycontrol);
                        if (hiddenvalue != null)
                        {

                            hiddenvalue.Value = Convert.ToString(DataBinder.Eval(bussinesObj, gNode.Attributes["Content"].Value)); //clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, "{" + gNode.Attributes["Content"].Value + "}", bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString()); ;
                        }
                        break;
                    case "HTML":
                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.HTML, gNode.Attributes["Content"].Value, link, targetURL));
                        
                        break;
                    case "PROGRESSBAR":
                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.ProgressBar, gNode.Attributes["Content"].Value, link, targetURL));
                        
                        break;
                    case "IMAGE":

                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.Image, gNode.Attributes["Content"].Value, link, targetURL));
                        if (gNode.Attributes["ToolTip"] != null)
                        {
                            String TooltipParsedValue = clsSharedMethods.SharedMethods.parseBOPropertiesString(bussinesObj, gNode.Attributes["ToolTip"].Value, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString());
                            literal.ToolTip = TooltipParsedValue;
                        }
                        break;
                    case "ACTIONS":
                        foreach (XmlNode node in gNode)
                        {
                            switch (node.Name.ToUpper())
                            {
                                case "TOOLBAR":
                                    m_Parent.CreateToolBar(node, literal, bussinesObj, false, null, DataItemIndex);
                                    break;
                                case "MENU":
                                    m_Parent.CreateMenu(node, literal, bussinesObj, false, null, DataItemIndex);
                                    break;
                            }
                        }
                        break;
                }
            }
            PropertyType(bussinesObj, literal);
        }

        public void InstantiateHeader(WebControl container, BLBusinessBase bussinesObj, int DataItemIndex)
        {
            WebControl literal = (WebControl)container;
            literal.Style.Add(HtmlTextWriterStyle.ZIndex, "1");
            if (IsHidden)
            {
                container.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
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
                switch (gNode.Attributes["HeaderPropertyType"].Value.ToUpper())
                {
                    case "PROPERTY":
                        String value = "{" + gNode.Attributes["Content"].Value + "}";
                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.HTML, value, link, targetURL));
                        break;
                    case "HTML":
                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.HTML, gNode.Attributes["Content"].Value, link, targetURL));
                        break;
                    case "PROGRESSBAR":
                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.ProgressBar, gNode.Attributes["Content"].Value, link, targetURL));
                        break;
                    case "IMAGE":

                        literal.Controls.Add(m_Parent.renderPropertyControl(bussinesObj, bussinesObj.GetType().AssemblyQualifiedName, bussinesObj["GUID"].ToString(),
                            clsCtrlWebPartBase.FactContentType.Image, gNode.Attributes["Content"].Value, link, targetURL));
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
                                                m_Parent.CreateToolBar(node, literal, bussinesObj, false, null, DataItemIndex);
                                                break;
                                            case "MENU":
                                                m_Parent.CreateMenu(node, literal, bussinesObj, false, null, DataItemIndex);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            PropertyType(bussinesObj, literal);
        }

        private void PropertyType(BLBusinessBase bussinesObj, WebControl literal)
        {
            if (gNode.Attributes["SelectRow"] != null)
            {
                if (Convert.ToBoolean(gNode.Attributes["SelectRow"].Value))
                {
                    CheckBox check = new CheckBox();
                    check.Checked = Convert.ToBoolean(hash[bussinesObj.UniqueString]);
                    literal.Controls.Add(check);
                }
            }
        }
    }
}
