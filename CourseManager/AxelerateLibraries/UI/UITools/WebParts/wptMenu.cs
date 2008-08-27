using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptMenu : clsBaseLayoutWP
    {
        public wptMenu(object datasource)
        {
            m_Datasource = datasource;
        }

        public wptMenu()
        {
        }

        

        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
        }

        protected override void EnsureDatasourceObject()
        {
            if (m_Datasource == null)
            {
                m_Datasource = GetBusinessClassInstance();
            }
        }

        protected override void CreateButtons(object Datasource, System.Web.UI.Control nCotainer)
        {
        }

        public override void SetItemFields()
        {
        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
        }

        protected override string GetXMLLayout()
        {
            Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout Layout;
            Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), LayoutName);
            if (Layout.LayoutXML == "")
            {
                clsLog.Trace(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName);
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName);
                    return "<FORM><COLLECTIONLAYOUT><BAND></BAND></COLLECTIONLAYOUT></FORM>";
                }
            }
            return Layout.LayoutXML;
        }

        public override bool HasDatasource()
        {
            return (m_Datasource != null);
        }

        public override bool IsNew()
        {
            return false;
        }

        [Category("Menu Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LayoutName")]
        [DefaultValue("")]
        [WebDisplayName("Grid Layout:")]
        [WebDescription("The name of the layout that will be displayed.")]
        public string LayoutName
        {
            get
            {
                if (ViewState["XMLLayout"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["XMLLayout"];
                }
            }
            set
            {

                ViewState["XMLLayout"] = value;
            }
        }

        
    }
}
