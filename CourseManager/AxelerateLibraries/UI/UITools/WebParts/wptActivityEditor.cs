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
using System.Collections;
using Axelerate.BusinessLayerUITools.Interfaces;
using System.Xml;
using System.Xml.Serialization;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptActivityEditor : clsBaseEditor
    {
        #region Members
        private object m_parenteditor;
        #endregion

        private int cont = 0;

        public wptActivityEditor() : base()
        {
            
        }

        /// <summary>
        /// Constructor used to create internal editors (an editor inside another editor).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="KeyFieldValue"></param>
        public wptActivityEditor(BLBusinessActivity datasource, wptActivityEditor parenteditor)
            : base()
        {
            Datasource = datasource;
            ParentEditor = parenteditor;
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
            if (fieldName.CompareTo("") == 0)       //its a consumer
            {
                fieldName = "consumer" + cont++;
                switch (typeName.ToLower())
                {
                    case "gridcontrol":
                        CreateGridControl(fieldName, typeName, CtrlNode, null, true, true, AsRadioButton, null, BOControlLayout, Container);
                        break;
                }
            }
            else
            {
                setFieldControl(ref fieldName, ref BOControlLayout);
                Type DsType = m_Datasource.GetType();
                System.Reflection.PropertyInfo propInfo = DsType.GetProperty(fieldName);
                if (propInfo != null)
                {
                    object BLDatasource = Datasource;
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
                            case "activityeditorcontrol":
                                /*                           if (typeof(BLBusinessBase).IsInstanceOfType(BLDatasource[fieldName]))  //Check if the property is an instance of a BLBusinessBase
                                                           {
                                                               if (IsReadable)
                                                               {
                                                                   wptBOEditor editor = new wptBOEditor((BLBusinessBase)BLDatasource[fieldName], this);
                                                                   editor.FactoryParameters = fieldName;
                                                                   editor.LayoutName = BOControlLayout;
                                                                   editor.ClassName = propInfo.PropertyType.AssemblyQualifiedName;
                                                                   Container.Controls.Add(editor);
                                                                   ChildEditorControls.Add(fieldName, editor);
                                                               }
                                                               else
                                                               {
                                                                   Literal editorNotReadable = new Literal();
                                                                   editorNotReadable.Text = strNotReadable;
                                                                   Container.Controls.Add(editorNotReadable);
                                                               }
                                                           }*/
                                break;
                            case "gridselectorcontrol":
                                CreateGridControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
                                break;
                            case "multitabbedselectorcontrol":
                                CreateMultiTabbedControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
                                break;

                            default:
                                SetError(string.Format(Resources.GenericWebParts.strErrorControlTypeNotSupported, typeName));
                                break;
                        }
                    }
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidFieldName + " " + fieldName + " in Layout " + LayoutName);
                }
            }
        }

        internal void CreateMultiTabbedControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, bool AsRadioButton, PropertyInfo propInfo, string BOControlLayout, Control Container)
        {
            string factoryParams = "";
            string factoryMethod = "";
            string className = "";
            string layoutName = "";
            string ConsumerType = "";
            string ReturnedProperty = "";
            
            if (CtrlNode.Attributes["ReturnedProperty"] != null)
            {
                ReturnedProperty = CtrlNode.Attributes["ReturnedProperty"].Value;
            }
            if (CtrlNode.Attributes["FactoryMethod"] != null)
            {
                factoryMethod = CtrlNode.Attributes["FactoryMethod"].Value;
            }
            if (CtrlNode.Attributes["FactoryParameters"] != null)
            {
                factoryParams = CtrlNode.Attributes["FactoryParameters"].Value;
            }
            if (CtrlNode.Attributes["ClassName"] != null)
            {
                className = CtrlNode.Attributes["ClassName"].Value;
            }
            if (CtrlNode.Attributes["LayoutName"] != null)
            {
                layoutName = CtrlNode.Attributes["LayoutName"].Value;
            }
            if (CtrlNode.Attributes["ConsumerType"] != null)
            {
                ConsumerType = CtrlNode.Attributes["ConsumerType"].Value;
            }
            if (CtrlNode.Attributes["ComponentBehavior"] != null)
            {
                if (CtrlNode.Attributes["ComponentBehavior"].Value.ToUpper() == "SELECTOR")
                {
                    wptMultiTabbedSelector Selector;
                    if (ChildEditorControls[fieldName] == null)
                    {
                        Selector = new wptMultiTabbedSelector();
                        ChildEditorControls.Add(fieldName, Selector);
                    }
                    else
                    {
                        Selector = (wptMultiTabbedSelector)ChildEditorControls[fieldName];
                    }
                    Selector.ID = fieldName;
                    Selector.ClassName = className;
                    Selector.FactoryMethod = factoryMethod;
                    Selector.FactoryParameters = factoryParams;
                    Selector.LayoutName = layoutName;
                    Selector.ConsumerType = ConsumerType;
                    Selector.ComponentBehaviorType = "SELECTOR";
                    Selector.ReturnedProperty = ReturnedProperty;
                    Container.Controls.Add(Selector);
  //                  ChildEditorControls.Add(fieldName, GridSelector);

                    PropertyInfo properties = m_Datasource.GetType().GetProperty(fieldName);
                    IBLListBase collection = (IBLListBase)properties.GetValue(Datasource, null);
                    if (collection != null)
                    {
                        foreach (BLBusinessBase selectedItem in collection)
                        {
                            Selector.SelectItem(selectedItem.DataKey.ToString());
                        }
                    }
                }
                else if (CtrlNode.Attributes["ComponentBehavior"].Value.ToUpper() == "READONLY")
                {
                    wptMultiTabbedSelector Selector;
                    if (ChildEditorControls[fieldName] == null)
                    {
                        Selector = new wptMultiTabbedSelector();
                        ChildEditorControls.Add(fieldName, Selector);
                    }
                    else
                    {
                        Selector = (wptMultiTabbedSelector)ChildEditorControls[fieldName];
                    }
                    Selector.ID = fieldName;
                    Selector.ClassName = className;
                    Selector.FactoryMethod = factoryMethod;
                    Selector.FactoryParameters = factoryParams;
                    Selector.LayoutName = layoutName;
                    Selector.ComponentBehaviorType = "READONLY";
                    Selector.ConsumerType = ConsumerType;
                    if (CtrlNode.Attributes["SelectorProvider"] != null)
                    {
                        ConnectionsToStablish[CtrlNode.Attributes["SelectorProvider"].Value] = fieldName;
                        //                                          grid.RegisterProvider = CtrlNode.Attributes["SelectorProvider"].Value;
                    }

                    Selector.ReturnedProperty = ReturnedProperty;
                    Container.Controls.Add(Selector);
                    //               ChildEditorControls.Add(fieldName, GridSelector);
                }
            }
        }

        protected override void InitLayount(string DisplayForm, object Datasource, bool IsPostback)
        {
            base.InitLayount(DisplayForm, Datasource, IsPostback);
            foreach (String key in ConnectionsToStablish.Keys)
            {
                wptGrid consumer = (wptGrid)ChildEditorControls[ConnectionsToStablish[key]];
                ISelectorProvider provider = (ISelectorProvider)ChildEditorControls[key];
                consumer.RegisterProvider(provider);
            }
        }
                
        protected override void CrateUnlayoutProperies(object Datasource, Control nContainer, bool IsPostback)
        {

        }

        /// <summary>
        /// Most create the object that this control edit and set it to m_Datasource, and
        /// create any aditional business object needed for the databinding.
        /// </summary>
        protected override void EnsureDatasourceObject()
        {
            if (Datasource == null)
            {
                Datasource = (BLBusinessActivity) GetBusinessClassInstance(); //ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "TryGetObjectByGUID", new object[] { base.getKeyFieldValue(), null });// { base.ObjectGUID, null});
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
            return false;// ((BLBusinessActivity)m_Datasource).IsNew;
        }

       
        
        #endregion

        #region Private Methods
        #endregion

        #region Properties
        private BLBusinessActivity Datasource
        {
            get
            {
                //m_Datasource = ViewState["CPEDatasource"];
                return (BLBusinessActivity)m_Datasource;
            }
            set
            {
                //ViewState["CPEDatasource"] = value;
                /*m_CustomProperties = null;
                m_CustomPropertyLayout = null;
                m_CustomPropertiesValues = null;*/
                m_Datasource = value;

            }
        }
        private wptActivityEditor ParentEditor
        {
            get
            {
                m_parenteditor = ViewState["BOEParentEditor"];
                return (wptActivityEditor)m_parenteditor;
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
            
        }

        protected override void CreateButtons(object Datasource, Control nCotainer)
        {
            if (m_parenteditor == null)
            {
                base.CreateButtons(Datasource, nCotainer);
            }
        }

        
    }
}
