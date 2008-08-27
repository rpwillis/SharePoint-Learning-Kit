using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties;
using System.Reflection;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Common;
using System.Xml.Serialization;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptBOEditor : clsBaseEditor
    {
        #region Members
        private object m_parenteditor;
        #endregion


        public wptBOEditor() : base()
        {
            
        }

        /// <summary>
        /// Constructor used to create internal editors (an editor inside another editor).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="KeyFieldValue"></param>
        public wptBOEditor(BLBusinessBase datasource, clsBaseEditor parenteditor) : base()
        {
            Datasource = datasource;
            ParentEditor = (wptBOEditor)parenteditor;
        }

        #region Method Overrides

        /// <summary>
        /// This function create the control specific based on the datasource, type, field and add it to the Container
        /// </summary>
        /// <param name="Datasource">The Business Object from which the data values will be taken.</param>
        /// <param name="typeName">Type of the control that will be rendered.</param>
        /// <param name="fieldName">Name of the property or field that will be used to create the control.</param>
        /// <param name="Container">The container that will contain the control.</param>
        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
            string typeName = "";
            string fieldName = "";
            string BOControlLayout = "";
            if (CtrlNode.Attributes["FieldName"] != null)
            {
                fieldName = CtrlNode.Attributes["FieldName"].Value;
            }
            if (CtrlNode.Attributes["Type"] != null)
            {
                typeName = CtrlNode.Attributes["Type"].Value;
            }
            setFieldControl(ref fieldName, ref BOControlLayout);
            Type DsType = Datasource.GetType();
            if (fieldName.Contains("."))
            {
                string newFieldName = fieldName.Substring(0, fieldName.IndexOf('.'));
                System.Reflection.PropertyInfo subPropInfo = null;
                //TODO: We can use FindMember to try obtimize this code.
                try {
                    subPropInfo = DsType.GetProperty(newFieldName);
                }
                catch {
                    subPropInfo = DsType.GetProperty(newFieldName, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly);                    
                }
                if (subPropInfo != null)
                {
                    if (clsSharedMethods.SharedMethods.CheckAccess(Datasource, "READ", newFieldName))
                    {
                        object subDatasource = DataBinder.Eval(Datasource, newFieldName);
                        CtrlNode.Attributes["FieldName"].Value = CtrlNode.Attributes["FieldName"].Value.Remove(0, newFieldName.Length + 1);
                        CreateInnerControl(subDatasource, CtrlNode, Container, AsRadioButton);
                    }
                    else
                    {
                        Literal CantReadError = new Literal();
                        CantReadError.Text = Resources.ErrorMessages.errPermissionsAccessProperty;
                        Container.Controls.Add(CantReadError);
                    }
                }
                else
                {
                    Literal CantReadError = new Literal();
                    CantReadError.Text = Resources.ErrorMessages.errObjectProp;
                    Container.Controls.Add(CantReadError);
                }
            }
            else
            {
                System.Reflection.PropertyInfo propInfo = DsType.GetProperty(fieldName);
                if (propInfo != null)
                {
                    object BLDatasource = Datasource;

                    if (checkPermitions(fieldName, typeName, ((BLBusinessBase)BLDatasource), propInfo, Container))
                    {
                        bool IsReadable = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "READ", fieldName);
                        bool IsEditable = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "UPDATE", fieldName); //Flag used to check if the property is Editable
                        switch (typeName.ToLower())
                        {
                            case "imagecontrol":
                                CreateImageControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, propInfo, Container);
                                break;
                            case "progressbarcontrol":
                                CreateProgressBarControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, propInfo, Container);
                                break;
                            case "htmlfieldcontrol":
                                CreateHtmlFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, propInfo, Container);
                                break;
                            case "fieldcontrol":
                                CreateFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, Container);
                                break;
                            case "boeditorcontrol":
                                CreateBoEditorControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
                                break;
                            case "gridcontrol":
                                CreateGridControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
                                break;
                            case "treecontrol":
                                CreateTreeView(CtrlNode, Container, BLDatasource, IsPostback);
                                break;
                            default:
                                SetError(string.Format(Resources.GenericWebParts.strErrorControlTypeNotSupported, typeName));
                                break;
                        }
                    }
                    else
                    {
                        SetError(Resources.ErrorMessages.errMissingPermissions);
                    }
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidFieldName + " " + fieldName + " in Layout " + LayoutName);
                }
            }
        }
                
        protected override void CrateUnlayoutProperies(object Datasource, Control nContainer, bool IsPostback)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml("<FORM><LAYOUT><GROUP></GROUP></LAYOUT></FORM>");
            //get the group node that is child of layout and is child of form.
            System.Xml.XmlNode UnlayoutProperiesGroup = doc.ChildNodes[0].ChildNodes[0].ChildNodes[0];

            System.Collections.Hashtable SubGroupNodes = new System.Collections.Hashtable();
            System.Xml.XmlAttribute Attr = null;

            System.Xml.XmlNode UnlayoutPropertiesColumn = doc.CreateNode(System.Xml.XmlNodeType.Element, "COLUMN", null);
            Attr = doc.CreateAttribute("PercentWidth");
            Attr.Value = "100";
            UnlayoutPropertiesColumn.Attributes.Append(Attr);

            UnlayoutProperiesGroup.AppendChild(UnlayoutPropertiesColumn);

            BLBusinessBase bussinessobj = (BLBusinessBase) Datasource;
            PropertyInfo[] properties = bussinessobj.GetType().GetProperties();
            BLFieldMapList FieldMapProperties = bussinessobj.DataLayer.FieldMapList;
            foreach (BLFieldMap field in FieldMapProperties.DataFetchFields)
            {
                if (PlaneFieldControls[field.NeutralFieldName] == null && ChildEditorControls[field.NeutralFieldName] == null)
                {
                    System.Xml.XmlNode CategoryGroupNode = (System.Xml.XmlNode)SubGroupNodes[bussinessobj.GetType().ToString()];
                    System.Xml.XmlNode CategoryGroupColumn = null;
                    if (CategoryGroupNode == null)
                    {
                        CategoryGroupNode = doc.CreateNode(System.Xml.XmlNodeType.Element, "GROUP", null);
                        Attr = doc.CreateAttribute("Label");
                        Attr.Value = bussinessobj.GetType().ToString();
                        CategoryGroupNode.Attributes.Append(Attr);
                        SubGroupNodes[bussinessobj.GetType().ToString()] = CategoryGroupNode;

                        CategoryGroupColumn = doc.CreateNode(System.Xml.XmlNodeType.Element, "COLUMN", null);
                        Attr = doc.CreateAttribute("PercentWidth");
                        Attr.Value = "100";
                        CategoryGroupColumn.Attributes.Append(Attr);

                        CategoryGroupNode.AppendChild(CategoryGroupColumn);
                    }
                    else
                    {
                        CategoryGroupColumn = CategoryGroupNode.ChildNodes[0];
                    }
                    System.Xml.XmlNode ControlNode = doc.CreateNode(System.Xml.XmlNodeType.Element, "CONTROL", null);


                    Attr = doc.CreateAttribute("Type");
                    if (field.Field.FieldType.IsAssignableFrom(typeof(BLBusinessBase)))
                    {
                        Attr.Value = "BOEditorControl";
                        ControlNode.Attributes.Append(Attr);
                    }
                    else if (field.Field.FieldType.IsAssignableFrom(typeof(IBLListBase)))
                    {
                        Attr.Value = "GridControl";
                    }
                    else
                    {
                        Attr.Value = "FieldControl"; //defult type for any property
                    }
                    ControlNode.Attributes.Append(Attr);

                    Attr = doc.CreateAttribute("FieldName");
                    Attr.Value = field.NeutralFieldName;
                    ControlNode.Attributes.Append(Attr);

                    Attr = doc.CreateAttribute("Label");
                    Attr.Value = field.NeutralFieldName + ":"; //default display name for the field
                    ControlNode.Attributes.Append(Attr);

                    Attr = doc.CreateAttribute("LabelPosition");
                    Attr.Value = "left"; //default label position
                    ControlNode.Attributes.Append(Attr);

                    CategoryGroupColumn.AppendChild(ControlNode);
                }
            }
            
            //now add all the subgroups to the main group.
            foreach (System.Xml.XmlNode SubGroupNode in SubGroupNodes.Values)
            {
                UnlayoutPropertiesColumn.AppendChild(SubGroupNode);
            }

            //Now Create the group in the layout of controls
            CreateGroup(UnlayoutProperiesGroup, Datasource, nContainer, IsPostback, null, -1);
        }

        /// <summary>
        /// Most create the object that this control edit and set it to m_Datasource, and
        /// create any aditional business object needed for the databinding.
        /// </summary>
        protected override void EnsureDatasourceObject()
        {
            if (m_Datasource == null)
            {
                m_Datasource = GetBusinessClassInstance(); //ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "TryGetObjectByGUID", new object[] { base.getKeyFieldValue(), null });// { base.ObjectGUID, null});
            }
            Datasource = (BLBusinessBase)m_Datasource;
        }

        /// <summary>
        /// Get the modified information from the controls (that come from the post) and fill the
        /// respective values of the datasource (fill the m_CustomPropertiesValues)
        /// </summary>
        public override void SetItemFields()
        {
            EnsureDatasourceObject();
            EnsureChildControls();
            PropertyInfo[] properties = m_Datasource.GetType().GetProperties();
            if (properties != null)
            {
                foreach (PropertyInfo property in properties)
                {
                    bool CanRead = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "READ", property.Name);
                    bool CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "UPDATE", property.Name);
                    Control Ctrl = (Control)PlaneFieldControls[property.Name];
                    if (Ctrl != null)
                    {
                        if (CanUpdate)
                        {
                            switch (Ctrl.GetType().FullName)
                            {
                                case "System.Web.UI.WebControls.TextBox":
                                    TextBox fieldTextBox = (TextBox)Ctrl;
                                    if (CanRead || fieldTextBox.Text != "")
                                    {
                                        try
                                        {
                                            Object propertyValue = Convert.ChangeType(fieldTextBox.Text, property.PropertyType);
                                            property.SetValue(m_Datasource, propertyValue, null);
                                        }
                                        catch (InvalidCastException ex)
                                        {
                                            //if this is the case we need show a message in the screen with a invalid value error for this field.
                                            //fieldTextBox.
                                        }
                                    }
                                    break;
                                case "System.Web.UI.WebControls.DropDownList":
                                case "System.Web.UI.WebControls.RadioButtonList":
                                case "System.Web.UI.WebControls.BulletedList":
                                case "System.Web.UI.WebControls.CheckBoxList":
                                case "System.Web.UI.WebControls.ListBox":

                                    ListControl fieldDDList = (ListControl)Ctrl;
                                    if (CanRead || fieldDDList.SelectedValue != "")
                                    {
                                        try
                                        {
                                            Object propertyValue = Convert.ChangeType(fieldDDList.SelectedValue, property.PropertyType);
                                            property.SetValue(m_Datasource, propertyValue, null);
                                        }
                                        catch (InvalidCastException ex)
                                        {
                                            //if this is the case we need show a message in the screen with a invalid value error for this field.
                                            //DDL.
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else //search for the control at ChildEditorControls
                    {
                        Ctrl = (Control)ChildEditorControls[property.Name];
                        if (Ctrl != null)
                        {
                            switch (Ctrl.GetType().FullName)
                            {
                                case "Axelerate.BusinessLayerUITools.wptBOEditor":
                                    ((wptBOEditor)Ctrl).SetItemFields();
                                    break;
                                case "Axelerate.BusinessLayerUITools.wptGrid":
                                    //((wptGrid)Ctrl).
                                    break;
                            }
                        }
                    }
                }
            }
        }

        

        [Category("Editor Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LayoutName")]
        [DefaultValue("")]
        [WebDisplayName("Editor Layout:")]
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

        public override bool IsNew()
        {
            return ((BLBusinessBase)m_Datasource).IsNew;
        }

       
        
        #endregion

        #region Private Methods
        #endregion

        #region Properties
        private BLBusinessBase Datasource
        {
            get
            {
                m_Datasource = ViewState["CPEDatasource"];
                return (BLBusinessBase)m_Datasource;
            }
            set
            {
                ViewState["CPEDatasource"] = value;
                /*m_CustomProperties = null;
                m_CustomPropertyLayout = null;
                m_CustomPropertiesValues = null;*/
                m_Datasource = value;

            }
        }
        private wptBOEditor ParentEditor
        {
            get
            {
                m_parenteditor = ViewState["BOEParentEditor"];
                return (wptBOEditor)m_parenteditor;
            }
            set
            {
                ViewState["BOEParentEditor"] = value;
                /*m_CustomProperties = null;
                m_CustomPropertyLayout = null;
                m_CustomPropertiesValues = null;*/
                m_parenteditor = value;
            }
        }
        #endregion

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
            EnsureDatasourceObject();
            if ((CommandName.ToLower() == "insert" || CommandName.ToLower() == "update") && m_Datasource != null)
            {
                SetItemFields();

                if (Datasource.IsValid)
                {
                    Datasource = ((BLBusinessBase)m_Datasource).Save();
                }
            }
            else if (CommandName.ToLower() == "cancel")
            {
            }
        }

        protected override void CreateButtons(object Datasource, Control nCotainer)
        {
            if (m_parenteditor == null)
            {
            //    base.CreateButtons(Datasource, nCotainer);
            }
        }

        protected void CreateTreeView(System.Xml.XmlNode parentNode, Control nContainer, object Datasource, bool IsPostback)
        {
            BLBusinessBase BLDatasource = (BLBusinessBase)Datasource;
            wptTreeView treeControl = null;
            if (ChildEditorControls[parentNode.Attributes["FieldName"].Value] == null)
            {
                treeControl = new wptTreeView();
                ChildEditorControls[parentNode.Attributes["FieldName"].Value] = treeControl;
            }
            else
            {
                treeControl = (wptTreeView)ChildEditorControls[parentNode.Attributes["FieldName"].Value];
            }
            treeControl.ComponentBehaviorType = "READONLY";
            treeControl.Datasource = BLDatasource;
            treeControl.ClassName = BLDatasource.GetType().AssemblyQualifiedName;
            treeControl.LayoutName = parentNode.Attributes["LayoutName"].Value;
            nContainer.Controls.Add(treeControl);
        }
    }
}
