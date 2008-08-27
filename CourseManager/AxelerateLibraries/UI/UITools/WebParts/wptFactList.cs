using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Axelerate.BusinessLayerFrameWork.BLCore;

using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Web.UI.Design.WebControls;
using System.Web.UI.Design;

using System.Xml;
using System.Xml.Serialization;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    [ToolboxData("<{0}:wptFactList runat=server />")]
    public class wptFactList : clsBaseLayoutWP
    {

#region Constructors
        public wptFactList():base()
        {
        }

        public wptFactList(object Datasource)
            : this()
        {
            m_Datasource = Datasource;
            ClassName = Datasource.GetType().AssemblyQualifiedName;
            
        }
#endregion

        #region Enums
        public enum FactContentType
        {
            FieldControl = 0,
            HTMLFieldControl = 1,
            ProgressBarControl = 2,
            ImageControl = 5
        }
        #endregion

        #region Members

        #endregion

        #region Method Overrides
        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
            string typeName = "";
            string fieldName = "";
            if (CtrlNode.Attributes["FieldName"] != null)
            {
                fieldName = CtrlNode.Attributes["FieldName"].Value;
            }
            if (CtrlNode.Attributes["Type"] != null)
            {
                typeName = CtrlNode.Attributes["Type"].Value;
            }
            EnsureDatasourceObject();
            if (Datasource != null)
            {
                if (!IsNew())
                {
                    BLBusinessBase BLDatasource = (BLBusinessBase)Datasource;
                    Container.Controls.Add(renderPropertyControl(BLDatasource, ClassName, BLDatasource.DataKey.ToString(), typeName.ToLower(), fieldName, false, false, ""));
                }
            }
        }

        protected override void CreateButtons(object Datasource, Control nCotainer)
        {

        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }

        protected override void EnsureDatasourceObject()
        {
            if (m_Datasource == null)
            {
                m_Datasource = GetBusinessClassInstance();//ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "TryGetObjectByGUID", new object[] { base.getKeyFieldValue(), null });// { base.ObjectGUID, null});
            }
        }

        public override void SetItemFields()
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
                    return "<FORM><LAYOUT></LAYOUT></FORM>";
                }
            }
            else return Layout.LayoutXML;
        }


        [Category("Fact List Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LayoutName")]
        [DefaultValue("")]
        [WebDisplayName("Fact List Layout:")]
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

        public override bool HasDatasource()
        {
            return (m_Datasource != null);
        }

        public override bool IsNew()
        {
            return ((BLBusinessBase)m_Datasource).IsNew;
        }

        #endregion

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        #endregion


       

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        
    }
}
