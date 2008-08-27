using System;
using System.Data;
using System.Configuration;
using System.ComponentModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using System.Collections;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerUITools.BaseClasses;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using System.Reflection;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.BaseClasses
{
    public  abstract class clsBaseTreeview: clsBaseLayoutWP 
    {
        protected override void ParseLayout(XmlNode LayoutNode, Control nContainer, object Datasource, bool IsPostback)
        {
            if (LayoutNode.Name.ToUpper() == "TREEVIEWLAYOUT")
            {
                if (LayoutNode.ChildNodes.Count > 0)
                {
                    foreach (System.Xml.XmlNode rootCollectionNode in LayoutNode.ChildNodes)
                    {
                        if (rootCollectionNode.Name.ToUpper() == "INODESTYLES")
                        {
                            foreach (System.Xml.XmlNode styleNode in rootCollectionNode.ChildNodes)
                            {
                                if (string.Compare(styleNode.Name, "INODESTYLE", true) == 0)
                                {
                                    if (styleNode.Attributes["ClassType"] != null)
                                    {
                                        TreeViewNodeStyles.Add(styleNode.Attributes["ClassType"].Value, styleNode);
                                    }
                                    else
                                    {
                                        if (!TreeViewNodeStyles.ContainsKey("General"))
                                        {
                                            TreeViewNodeStyles.Add("General", styleNode);
                                        }
                                    }
                                }
                            }
                        }
                        else if (rootCollectionNode.Name.ToUpper() == "NODECOLLECTION")
                        {
                            CreateTreeView(rootCollectionNode, nContainer, Datasource, IsPostback);
                        }
                    }
                }
            }
        }

        protected abstract void CreateTreeView(System.Xml.XmlNode parentNode, Control nContainer, object Datasource, bool IsPostback);
    }
}
