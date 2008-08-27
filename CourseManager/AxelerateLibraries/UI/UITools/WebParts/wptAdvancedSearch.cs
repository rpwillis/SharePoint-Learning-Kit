using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Web.UI;
using System.Web.UI.WebControls;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using System.Reflection;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using System.Collections;
using Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties;
using System.Web.UI.WebControls.WebParts;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLayerUITools.BaseClasses;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptAdvancedSearch : clsBaseEditor, ISearchProvider
    {

        #region TriState Selector Info Class
        internal class TriStateTreeViewInfo
        {
            public readonly string SelectedItemsFieldName;
            public readonly string UnselectedItemsFieldName;
            public readonly wptTreeView SelectorTreeView;

            public TriStateTreeViewInfo(string nSelectedItemsFieldName, string nUnselectedItemsFieldName, wptTreeView nSelectorTreeView)
            {
                SelectedItemsFieldName = nSelectedItemsFieldName;
                UnselectedItemsFieldName = nUnselectedItemsFieldName;
                SelectorTreeView = nSelectorTreeView;
            }
        }
        #endregion

        private Hashtable m_FieldControls = new Hashtable();
        private Hashtable m_TreeViewControls = new Hashtable();
        #region Properties

        private Hashtable FieldControls
        {
            get
            {
                return m_FieldControls;
            }
            set
            {
                m_FieldControls = value;
            }
        }

        public bool NewSearch
        {
            get
            {
                if (ViewState["NewSearch"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["NewSearch"]);
                }
            }
            set
            {
                ViewState["NewSearch"] = value;
            }
        }

        public bool AllowEmptySearch
        {
            get
            {
                if (ViewState["AllowEmptySearch"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["AllowEmptySearch"]);
                }
            }
            set
            {
                ViewState["AllowEmptySearch"] = value;
            }
        }

        #endregion
        
        

        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
            string typeName = "";
            string fieldName = "";
            string BOControlLayout = "";
            string returnedProperty = "";
            string factoryParams = "";
            string factoryMethod = "";
            string className = "";
            string layoutName = "";
            string id = "";
            string wptBehavior = ComponentBehaviorType;
            if (CtrlNode.Attributes["FieldName"] != null)
            {
                fieldName = CtrlNode.Attributes["FieldName"].Value;
            }
            if (CtrlNode.Attributes["Type"] != null)
            {
                typeName = CtrlNode.Attributes["Type"].Value;
            }
            if (CtrlNode.Attributes["ReturnedProperty"] != null)
            {
                if (CtrlNode.Attributes["ReturnedProperty"].Value == "")
                {
                    returnedProperty = null;
                }
                else returnedProperty = CtrlNode.Attributes["ReturnedProperty"].Value;
            }
            else
            {
                returnedProperty = null;
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
            if (CtrlNode.Attributes["ComponentBehavior"] != null)
            {
                wptBehavior = CtrlNode.Attributes["ComponentBehavior"].Value;
            }
            if (CtrlNode.Attributes["ID"] != null)
            {
                id = CtrlNode.Attributes["ID"].Value;
            }
            setFieldControl(ref fieldName, ref BOControlLayout);
            bool IsCriteria = false;
            IBLListBase activate;
            Type DsType = Type.GetType(ClassName);
            object BLDatasource;
            if (!typeof(BLCriteria).IsAssignableFrom(DsType))
            {
                activate = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ClassName));
                DsType = activate.DataLayer.BusinessLogicType;
                BLDatasource = (BLBusinessBase)System.Activator.CreateInstance(DsType);
            }
            else
            {
                IsCriteria = true;
                BLDatasource = m_Datasource;
            }

            
            System.Reflection.PropertyInfo propInfo = DsType.GetProperty(fieldName);
            if (propInfo != null)
            {               
                bool IsReadable = clsSharedMethods.SharedMethods.CheckAccess(BLDatasource, "READ", fieldName);
                bool IsEditable = clsSharedMethods.SharedMethods.CheckAccess(BLDatasource, "UPDATE", fieldName); //Flag used to check if the property is Editable
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
                    case "gridselectorcontrol":
                        CreateGridControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
                        break;
                    case "multitabbedselectorcontrol":
                        CreateMultiTabbedControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, BOControlLayout, Container);
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
                SetError(Resources.ErrorMessages.errInvalidFieldName + " " + fieldName + " in Layout " + LayoutName);
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
                    IBLListBase collection = (IBLListBase)properties.GetValue(BLDatasource, null);
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

        protected override void EnsureDatasourceObject()
        {
            Type t = Type.GetType(ClassName);
            if (typeof(BLCriteria).IsAssignableFrom(t))
            {
                m_Datasource = GetBusinessClassInstance();
            }
            else
            {
                m_Datasource = new BLCriteria(Type.GetType(ClassName));
            }
        }

        public override void SetItemFields()
        {
            //I only need do this function if the classname is a Criteria type
            if (typeof(BLCriteria).IsAssignableFrom(Type.GetType(ClassName)))
            {
                EnsureDatasourceObject();
                EnsureChildControls();
                foreach (clsBaseLayoutWP contr in ChildEditorControls.Values)
                {
                    contr.SetItemFields();
                }
                PropertyInfo[] properties = m_Datasource.GetType().GetProperties();
                if (properties != null)
                {
                    foreach (PropertyInfo property in properties)
                    {
                        bool CanRead = clsSharedMethods.SharedMethods.CheckAccess(m_Datasource, "READ", property.Name);
                        bool CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(m_Datasource, "UPDATE", property.Name);
                        Control Ctrl = (Control)PlaneFieldControls[property.Name];
                        if (Ctrl != null)
                        {
                            if (CanUpdate)
                            {
                                switch (Ctrl.GetType().FullName)
                                {
                                    case "System.Web.UI.WebControls.CheckBox":
                                        CheckBox fieldCheckBox = (CheckBox)Ctrl;
                                        property.SetValue(m_Datasource, fieldCheckBox.Checked, null);
                                        break;

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
                                    case "Axelerate.BusinessLayerUITools.WebControls.ListBox":
                                        WebControls.ListBox fieldListBox = (WebControls.ListBox)Ctrl;
                                        if (CanRead)
                                        {
                                            try
                                            {
                                                Object propertyValue = fieldListBox.SelectedValue;
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
                                //switch (Ctrl.GetType().FullName)
                                //{
                                //    case "Axelerate.BusinessLayerUITools.WebParts.wptActivityEditor":
                                //        ((wptActivityEditor)Ctrl).SetItemFields();
                                //        break;
                                //    case "Axelerate.BusinessLayerUITools.WebParts.wptGrid":
                                //       // Datasource.GetType().GetProperty(fieldname
                                //        property.SetValue(m_Datasource, ((wptGrid)Ctrl).SelectedItems, null);
                                //        break;
                                //}
                                if (typeof(clsBaseLayoutWP).IsAssignableFrom(Ctrl.GetType()))
                                {
                                    ((clsBaseLayoutWP)Ctrl).SetItemFields();
                                    if (typeof(ISelectorProvider).IsAssignableFrom(Ctrl.GetType()) && ((clsBaseLayoutWP)Ctrl).ComponentBehaviorType.ToUpper() == "SELECTOR")
                                    {
                                        property.SetValue(m_Datasource, ((ISelectorProvider)Ctrl).SelectedItems, null);
                                    }
                                }

                            }
                        }
                    }
                }
                foreach (TriStateTreeViewInfo selector in this.m_TreeViewControls.Values)
                {
                   PropertyInfo property = null;
                   try
                   {
                       property = m_Datasource.GetType().GetProperty(selector.SelectedItemsFieldName);
                   }
                   catch (AmbiguousMatchException ex)
                   {
                       property = m_Datasource.GetType().GetProperty(selector.SelectedItemsFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance);
                   }
                   if (property != null)
                   {
                       property.SetValue(m_Datasource, selector.SelectorTreeView.CheckedItems, null);
                   }
                   if (selector.UnselectedItemsFieldName != "")
                   {
                       try
                       {
                           property = m_Datasource.GetType().GetProperty(selector.UnselectedItemsFieldName);
                       }
                       catch (AmbiguousMatchException ex)
                       {
                           property = m_Datasource.GetType().GetProperty(selector.UnselectedItemsFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance);
                       }
                       if (property != null)
                       {
                           property.SetValue(m_Datasource, selector.SelectorTreeView.UnCheckedItems, null);
                       }
                   }
 
                }
            }

        }

        [ConnectionProvider("Search Provider", "default")]
        public ISearchProvider GetSearchProvider()
        {
            return this;
        }

        public override void ExecuteCommand(object sender, CommandEventArgs e, bool IsContext)
        {
            if ((e.CommandName.ToUpper() == "SEARCH"))
            {
                NewSearch = true;
            }
            else
            {
                EnsureDatasourceObject();
                String[] enteredArgs = e.CommandArgument.ToString().Split('|');
                int indexRow = Convert.ToInt32(enteredArgs[0]);
                String args = enteredArgs[1];

                String[] arguments = new string[0];
                if (args.CompareTo("") != 0)
                {
                    arguments = args.Split(',');//e.CommandArgument.ToString().Split(',');
                    int size = arguments.Length;
                    for (int index = 0; index != size; index++)
                    {
                        arguments[index] = getArgument(arguments[index]);
                    }
                }
                System.Reflection.MethodInfo method = null;
                object source = null;
                if (indexRow == -1)
                {
                    method = m_Datasource.GetType().GetMethod(e.CommandName);
                    source = m_Datasource;
                }
                else
                {
                    IList datasource = (IList)m_Datasource;
                    method = datasource[indexRow].GetType().GetMethod(e.CommandName);
                    source = datasource[indexRow];
                }
                try
                {
                    if (method != null)
                    {
                        method.Invoke(source, arguments);
                    }
                }
                catch (Exception ex)
                {
                }
                //(ClassName, button.CommandName, button.CommandArgument)
                //m_Datasource 
            }
        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
        }

        public override bool HasDatasource()
        {
            return m_Datasource != null;
        }

        public override bool IsNew()
        {
            return false;
        }

        protected override void CreateButtons(object Datasource, Control nCotainer)
        {
        }

        protected void CreateTreeView(System.Xml.XmlNode CtrlNode, Control nContainer, object Datasource, bool IsPostback)
        {
            string factoryParams = "";
            string factoryMethod = "";
            string className = "";
            string layoutName = "";
            string SelectedItemsField = "";
            string UnselectedItemsField = "";
            string id = "";

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
            if (CtrlNode.Attributes["FieldName"] != null)
            {
                SelectedItemsField = CtrlNode.Attributes["FieldName"].Value;
            }
            if (CtrlNode.Attributes["UnselectedField"] != null)
            {
                UnselectedItemsField = CtrlNode.Attributes["UnselectedField"].Value;
            }
            id = SelectedItemsField;
            if (CtrlNode.Attributes["ID"] != null)
            {
                id = CtrlNode.Attributes["ID"].Value;
            }

            wptTreeView treeControl = null;

            BLBusinessBase BLDatasource = null; // (BLBusinessBase)Datasource;

            if (m_TreeViewControls[id] == null)
            {
                treeControl = new wptTreeView();
                treeControl.ID = id;
                m_TreeViewControls[id] = new TriStateTreeViewInfo(SelectedItemsField, UnselectedItemsField, treeControl);
            }
            else
            {
                treeControl = ((TriStateTreeViewInfo)m_TreeViewControls[id]).SelectorTreeView;
            }
            SetAppareance(treeControl, CtrlNode);

            if (className == "" || factoryMethod == "")
            {
                BLDatasource = (BLBusinessBase)DataBinder.Eval(Datasource, SelectedItemsField);
                
                treeControl.ComponentBehaviorType = "READONLY";
                treeControl.Datasource = BLDatasource;
                treeControl.ClassName = BLDatasource.GetType().AssemblyQualifiedName;
            }
            else
            {
                treeControl.ComponentBehaviorType = "READONLY";
                treeControl.ClassName = className;
                treeControl.FactoryMethod = factoryMethod;
                treeControl.FactoryParameters = factoryParams;
                
            }
            
            treeControl.LayoutName = layoutName;
            
            nContainer.Controls.Add(treeControl);
        }

        #region ISearchProvider Members

        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Resultstitle")]
        [DefaultValue("")]
        [WebDisplayName("Results title:")]
        [WebDescription("Enter the property that will be diplayed as the results' title.")]
        public string title
        {
            get
            {
                if (ViewState["Resultstitle"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Resultstitle"].ToString();
                }
            }
            set
            {
                ViewState["Resultstitle"] = value;
            }
        }

        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "description")]
        [DefaultValue("")]
        [WebDisplayName("Results description:")]
        [WebDescription("Enter the property that will be diplayed as the results' description.")]
        public string description
        {
            get
            {
                if (ViewState["Resultsdescription"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Resultsdescription"].ToString();
                }

            }
            set
            {
                ViewState["Resultsdescription"] = value;
            }
        }

        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Details")]
        [DefaultValue("")]
        [WebDisplayName("Results details:")]
        [WebDescription("Enter the property that will be diplayed as the results' details.")]
        public string details
        {
            get
            {
                if (ViewState["Details"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Details"].ToString();
                }
            }
            set
            {
                ViewState["Details"] = value;
            }
        }

        [Category("Results Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Redirect")]
        [DefaultValue("")]
        [WebDisplayName("Datailed results page URL:")]
        [WebDescription("Enter the page URL where the detailed results are going to be displayed.")]
        public string redirect
        {
            get
            {
                if (ViewState["Redirect"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["Redirect"].ToString();
                }
            }
            set
            {
                ViewState["Redirect"] = value;
            }
        }

        public string[] columns
        {
            get
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
            set
            {
                throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
            }
        }

        public string businessobj
        {
            get
            {
                return Type.GetType(ClassName).AssemblyQualifiedName;
            }
            set
            {

            }
        }

        public BLCriteria Criteria
        {
            get
            {
                if (NewSearch)
                {
                    NewSearch = false;
                }
                EnsureDatasourceObject();
                SetItemFields();

                Type t = Type.GetType(ClassName);
                if (typeof(BLCriteria).IsAssignableFrom(t))
                {
                    //BLCriteria need call a PreProcess method to create the filters based on his internal variables

                    return (BLCriteria)m_Datasource;
                }
                else
                {
                    IBLListBase activate = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ClassName));
                    String suffix = activate.DataLayer.DataLayerFieldSuffix;
                    BLCriteria nCriteria = (BLCriteria)m_Datasource;

                    foreach (Control control in PlaneFieldControls.Values)
                    {
                        if (typeof(TextBox).IsAssignableFrom(control.GetType()))
                        {
                            TextBox text = (TextBox)control;
                            if (AllowEmptySearch || (text.Text.CompareTo("") != 0))
                            {
                                String DBfield = text.ID + suffix;
                                String[] words = text.Text.Split(null);
                                foreach (String word in words)
                                {
                                    nCriteria.AddBinaryExpression(DBfield, text.ID, "like", "%" + word + "%", BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                                }
                            }
                        }
                        if (typeof(CheckBox).IsAssignableFrom(control.GetType()))
                        {
                            CheckBox check = (CheckBox)control;
                            String DBfield = check.ID + suffix;
                            nCriteria.AddBinaryExpression(DBfield, check.ID, "=", check.Checked, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                        }
                        if (typeof(ListControl).IsAssignableFrom(control.GetType()))
                        {
                            ListControl dropdown = (ListControl)control;
                            String DBfield = dropdown.ID + suffix;
                            nCriteria.AddBinaryExpression(DBfield, dropdown.ID, "=", dropdown.SelectedValue, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                        }
                    }
                    return nCriteria;
                }

            }
        }

        #endregion
    }
}
