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
    public abstract class clsBaseGrid : clsBaseEditor
    {
        protected override void ParseLayout(XmlNode LayoutNode, Control nContainer, object Datasource, bool IsPostback)
        {
            if (LayoutNode.Name.ToUpper() == "COLLECTIONLAYOUT")
            {
                if (LayoutNode.ChildNodes.Count > 0)
                {
                    System.Xml.XmlNode rootBandNode = LayoutNode.ChildNodes[0];

                    if (rootBandNode.Name.ToUpper() == "BAND")
                    {
                        CreateGrid(rootBandNode, nContainer, Datasource, IsPostback);
                    }
                }
            }
        }
        
        protected abstract void CreateGrid(System.Xml.XmlNode gNode, Control nContainer, object Datasource, bool IsPostback);
    }
}
