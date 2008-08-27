using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerUITools.WebParts;
using System.Xml;
using System.Collections;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.BaseClasses
{
    public class clsBaseEditor : clsBaseLayoutWP
    {
        #region Private Properties

        internal const string strNotReadable = "-";
        internal Hashtable ConnectionsToStablish = new Hashtable();
        internal Hashtable m_CalendarExtenders = new Hashtable();

        #endregion

        #region Public properties

        /// <summary>
        /// Indicate if most add the client validator in CreateInnerControl methods.
        /// </summary>
        protected bool AddClientValidators
        {
            get
            {
                return (ViewState["AddClientValidators"] == null ? true : (bool)ViewState["AddClientValidators"]);
            }
            set
            {
                ViewState["AddClientValidators"] = value;
            }
        }

        #endregion

        internal virtual void setFieldControl(ref string fieldName, ref string BOControlLayout)
        {
            string[] PropertyNameAndLayoutName = fieldName.Split(new char[] { '|' });
            fieldName = PropertyNameAndLayoutName[0];
            if (PropertyNameAndLayoutName.Length > 1)
            {
                BOControlLayout = PropertyNameAndLayoutName[1];
            }
        }

        internal virtual bool checkPermitions(string fieldName, string typeName, BLBusinessBase BLDatasource, PropertyInfo propInfo, Control Container)
        {
            if (!propInfo.CanWrite &&
                    (!(typeof(BLBusinessBase).IsInstanceOfType(BLDatasource[propInfo.Name])) &&
                    !(typeof(IBLListBase).IsInstanceOfType(BLDatasource[propInfo.Name]))))
            {  //If the property is ReadOnly And is not a BusinessObject or a Collection
                Control propertyControl = renderPropertyControl(BLDatasource, ClassName, BLDatasource["GUID"].ToString(), typeName, fieldName, false, false, "");
                Container.Controls.Add(propertyControl);
                PlaneFieldControls[fieldName] = propertyControl;
                return false;
            }
            else
                return true;
        }

        internal virtual void CreateImageControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, PropertyInfo propInfo, Control Container)
        {
            Image img = new Image();
            SetAppareance(img, CtrlNode);
            if (IsReadable)
            {
                if (typeof(BLBusinessBase).IsAssignableFrom(BLDatasource.GetType()))
                {
                    img = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)BLDatasource, "{" + fieldName + "}", false);
                }
            }
            Container.Controls.Add(img);
        }

        internal virtual void CreateProgressBarControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, PropertyInfo propInfo, Control Container)
        {
            Image imgPB = new Image();
            if (IsReadable)
            {
                if (typeof(BLBusinessBase).IsAssignableFrom(BLDatasource.GetType()))
                {
                    imgPB = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)BLDatasource, "{" + fieldName + "}", true);
                }
            }
            Container.Controls.Add(imgPB);
        }

        internal virtual void CreateLabel(string fieldName, string typeName, XmlNode CtrlNode, Control Container)
        {
            Label lbl = new Label();
            lbl.Text = fieldName;
            SetAppareance(lbl, CtrlNode);
            Container.Controls.Add(lbl);
        }

        internal virtual void CreateHtmlFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, PropertyInfo propInfo, Control Container)
        {
            TextBox tbxHTML = null;
            if (PlaneFieldControls[fieldName] == null)
            {
                tbxHTML = new TextBox();
                tbxHTML.ID = fieldName;
                if (CtrlNode.Attributes["ID"] != null)
                {
                    tbxHTML.ID = CtrlNode.Attributes["ID"].Value;
                }
            }
            else
            {
                tbxHTML = (TextBox)PlaneFieldControls[fieldName];
            }
            SetAppareance(tbxHTML, CtrlNode);
            Object objHTMLProperty = propInfo.GetValue(BLDatasource, null);
            if (!IsReadable)
            {
                tbxHTML.TextMode = TextBoxMode.Password;
                tbxHTML.Text = "          ";
            }
            else
            {
                tbxHTML.TextMode = TextBoxMode.MultiLine;
                tbxHTML.Text = clsSharedMethods.SharedMethods.getPropertyValueToString(objHTMLProperty, "");
            }
            //
            tbxHTML.Enabled = !IsEditable;
            Container.Controls.Add(tbxHTML);
            PlaneFieldControls[fieldName] = tbxHTML;
        }

        internal virtual bool CreateStringFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            if (Attribute.IsDefined(propInfo, typeof(StringListAllowedValueAttribute), false))
            {
                StringListAllowedValueAttribute allowedStringValues =
                    (StringListAllowedValueAttribute)Attribute.GetCustomAttribute(propInfo, typeof(StringListAllowedValueAttribute), true);
                string resourcefile = "";//allowedStringValues.ResourcesFile;
                string[] allowedStrings = allowedStringValues.Values;
                if (allowedStrings.Length > 0)
                {
                    if (resourcefile != "")
                    {
                        System.Resources.ResourceManager _resourceManager;
                        Assembly resourceAssembly = BLDatasource.GetType().Assembly;
                        _resourceManager = new System.Resources.ResourceManager(
                             resourcefile, resourceAssembly);
                        _resourceManager.IgnoreCase = true;

                        for (int i = 0; i < allowedStrings.Length; i++)
                        {
                            string valueName = allowedStrings[i];
                            if (_resourceManager.GetString(valueName) != null)
                            {
                                allowedItems.Add(new ListItem(_resourceManager.GetString(valueName), valueName));
                            }
                            else
                            {
                                allowedItems.Add(new ListItem(valueName, valueName));
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < allowedStrings.Length; i++)
                        {
                            allowedItems.Add(new ListItem(allowedStrings[i]));
                        }
                    }
                }
                return true;
            }
            else if (Attribute.IsDefined(propInfo, typeof(BCAllowedValuesAttribute), false))
            {
                try
                {
                    BCAllowedValuesAttribute allowedCollectionValues =
                        (BCAllowedValuesAttribute)Attribute.GetCustomAttribute(propInfo, typeof(BCAllowedValuesAttribute), true);
                    BLCriteria criteria = new BLCriteria(propInfo.PropertyType);

                    IBLListBase objects = allowedCollectionValues.get_AllowedObjects(allowedCollectionValues.GetFactoryMethodParameterArray(m_Datasource, criteria));
                    foreach (BLBusinessBase allowedValue in objects)
                    {
                        allowedItems.Add(new ListItem(allowedValue.ToString(), allowedValue[allowedCollectionValues.TargetPropertyName].ToString()));
                    }

                }
                catch (SecurityException)
                {
                }
                return true;
            }
            else
            {
                TextBox tbxProperty = null;
                tbxProperty = new TextBox();
                tbxProperty.ID = fieldName;
                if (CtrlNode.Attributes["ID"] != null)
                {
                    tbxProperty.ID = CtrlNode.Attributes["ID"].Value;
                }
                if (PlaneFieldControls[tbxProperty.ID] != null)
                {
                    tbxProperty = (TextBox)PlaneFieldControls[tbxProperty.ID];
                }
                SetAppareance(tbxProperty, CtrlNode);
                tbxProperty.Width = new Unit("100%");
                Object objProperty = propInfo.GetValue(BLDatasource, null);
                if (IsReadable)
                {
                    tbxProperty.TextMode = TextBoxMode.SingleLine;
                }
                else
                {
                    tbxProperty.TextMode = TextBoxMode.Password;
                }
                tbxProperty.Text = clsSharedMethods.SharedMethods.getPropertyValueToString(objProperty, "");
                tbxProperty.ReadOnly = !IsEditable;
                if (Attribute.IsDefined(propInfo, typeof(StringLengthValidationAttribute), false))
                {
                    StringLengthValidationAttribute maxLength = (StringLengthValidationAttribute)Attribute.GetCustomAttribute(propInfo, typeof(StringLengthValidationAttribute), true);
                    tbxProperty.MaxLength = maxLength.MaxLength;
                }
                if (Attribute.IsDefined(propInfo, typeof(StringRequiredValidationAttribute), false) && !IsInEditMode && AddClientValidators)
                {
                    /*RequiredFieldValidator requiredField = new RequiredFieldValidator();
                    requiredField.ControlToValidate = tbxProperty.ID;
                    requiredField.Display = ValidatorDisplay.Dynamic;
                    requiredField.Text = "Required Field";
                    Container.Controls.Add(tbxProperty);
                    Container.Controls.Add(requiredField);*/
                    Container.Controls.Add(tbxProperty);
                }
                else
                {
                    Container.Controls.Add(tbxProperty);
                }

                PlaneFieldControls[tbxProperty.ID] = tbxProperty;
                return false;
            }
        }

        internal virtual bool CreateCustomControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            bool CanRead = clsSharedMethods.SharedMethods.CheckAccess(m_Datasource, "READ", propInfo.Name);
            bool CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(m_Datasource, "UPDATE", propInfo.Name);
            foreach (XmlNode CustomEditorXML in CtrlNode.ChildNodes)
            {
                if (CustomEditorXML.Name.ToUpper() == "CUSTOMVALUEEDITOR")
                {
                    ICustomValueEditor editor = null;
                    string ID = fieldName;
                    if (CtrlNode.Attributes["ID"] != null)
                    {
                        ID = CtrlNode.Attributes["ID"].Value;
                    }
                    if (PlaneFieldControls[ID] == null)
                    {
                        editor = (ICustomValueEditor)System.Activator.CreateInstance(System.Type.GetType(CustomEditorXML.Attributes["ClassName"].Value));
                        editor.DataValueField = propInfo.Name;
                        ((System.Web.UI.WebControls.WebControl)editor).ID = ID;
                    }
                    else
                    {
                        editor = (ICustomValueEditor)PlaneFieldControls[ID];
                    }
                    editor.DataSource = BLDatasource;
                    editor.DataValueFormat = "";
                    if (CustomEditorXML.Attributes["Format"] != null)
                    {
                        editor.DataValueFormat = CustomEditorXML.Attributes["Format"].Value;
                    }
                    if (CanRead)
                    {
                        editor.Value = propInfo.GetValue(BLDatasource, null);
                    }
                    if (CanUpdate)
                    {
                        editor.IsReadOnly = true;
                    }
                    else
                    {
                        editor.IsReadOnly = false;
                    }
                    if (CtrlNode.Attributes["Style"] != null && CustomEditorXML.Attributes["Style"] != null)
                    {
                        CtrlNode.Attributes["Style"].Value += CustomEditorXML.Attributes["Style"].Value;
                    }
                    SetAppareance((System.Web.UI.WebControls.WebControl)editor, CtrlNode);
                    Container.Controls.Add((System.Web.UI.WebControls.WebControl)editor);
                    PlaneFieldControls[ID] = editor;
                }

            }
            return true;
        }

        internal virtual void CreateNumericFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            TextBox tbxNumberProperty = null;
            string CtrlID = fieldName;
            if (CtrlNode.Attributes["ID"] != null)
            {
                CtrlID = CtrlNode.Attributes["ID"].Value;
            }
            if (PlaneFieldControls[CtrlID] == null)
            {
                tbxNumberProperty = new TextBox();
                tbxNumberProperty.ID = CtrlID;

            }
            else
            {
                tbxNumberProperty = (TextBox)PlaneFieldControls[CtrlID];
            }
            SetAppareance(tbxNumberProperty, CtrlNode);
            Object objNumberProperty = propInfo.GetValue(BLDatasource, null);
            if (IsReadable)
            {
                tbxNumberProperty.TextMode = TextBoxMode.SingleLine;
            }
            else
            {
                tbxNumberProperty.TextMode = TextBoxMode.Password;
            }
            tbxNumberProperty.Text = clsSharedMethods.SharedMethods.getPropertyValueToString(objNumberProperty, "");
            tbxNumberProperty.MaxLength = 5;
            Container.Controls.Add(tbxNumberProperty);
            PlaneFieldControls[CtrlID] = tbxNumberProperty;

            if (!IsInEditMode && AddClientValidators)
            {
                RegularExpressionValidator validator = new RegularExpressionValidator();
                validator.ControlToValidate = tbxNumberProperty.ID;
                validator.Display = ValidatorDisplay.Dynamic;
                validator.Text = Resources.LocalizationUIToolsResource.numValueRequired; //TODO: move to resource file
                if (!propInfo.PropertyType.Equals(typeof(Double)) || !propInfo.PropertyType.Equals(typeof(double)) || !propInfo.PropertyType.Equals(typeof(float)) || !propInfo.PropertyType.Equals(typeof(decimal)) || !propInfo.PropertyType.Equals(typeof(Decimal)))
                {
                    validator.ValidationExpression = @"[-+]?\d+";
                }
                else
                {
                    validator.ValidationExpression = @"[-+]?\d*\.?\d+";
                }
                validator.SetFocusOnError = true;
                Container.Controls.Add(validator);
            }
        }

        internal virtual void CreateDateTimeControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            TextBox tbxDateProperty = new TextBox();
            tbxDateProperty.ID = fieldName;
            if (CtrlNode.Attributes["ID"] != null)
            {
                tbxDateProperty.ID = CtrlNode.Attributes["ID"].Value;
            }
            if (PlaneFieldControls[tbxDateProperty.ID] != null)
            {
                tbxDateProperty = (TextBox)PlaneFieldControls[tbxDateProperty.ID];
            }
            Object objDateProperty = propInfo.GetValue(BLDatasource, null);
            string objDateString = "";
            if (((DateTime)objDateProperty) == null || ((DateTime)objDateProperty).CompareTo(DateTime.MinValue) < 0)
            {
                objDateProperty = DateTime.Now;
                objDateString = ((DateTime)objDateProperty).ToShortDateString();
            }
            else
            {
                objDateString = clsSharedMethods.SharedMethods.getPropertyValueToString(objDateProperty, "");
            }
            if (IsReadable)
            {
                tbxDateProperty.TextMode = TextBoxMode.SingleLine;
                tbxDateProperty.Text = objDateString;
            }
            else
            {
                tbxDateProperty.TextMode = TextBoxMode.Password;
                tbxDateProperty.Text = "-";
            }
            SetAppareance(tbxDateProperty, CtrlNode);
            tbxDateProperty.ReadOnly = true;
            Container.Controls.Add(tbxDateProperty);
            PlaneFieldControls[tbxDateProperty.ID] = tbxDateProperty;

            if (IsEditable)
            {
                AjaxControlToolkit.CalendarExtender Calendar = new AjaxControlToolkit.CalendarExtender();
                Calendar.TargetControlID = tbxDateProperty.ID;
                if (CtrlNode.Attributes["ContentFormat"] != null)
                {
                    Calendar.Format = CtrlNode.Attributes["ContentFormat"].Value;
                }
                //m_CalendarExtenders.Add(tbxDateProperty.ID, Calendar);
                Calendar.SelectedDate = Convert.ToDateTime(objDateProperty);
                Container.Controls.Add(Calendar);
            }
            //RegularExpressionValidator validator = new RegularExpressionValidator();
            //validator.ControlToValidate = tbxNumberProperty.ID;
            //validator.Display = ValidatorDisplay.Dynamic;
            //validator.Text = "Require a date time value"; //TODO: move to resource file
            //validator.ValidationExpression = @"[-+]?\d*\.?\d+";

            //validator.SetFocusOnError = true;
            //Container.Controls.Add(validator);
        }

        internal virtual bool CreateEnumFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            string[] constantEnumStrings = Enum.GetNames(propInfo.PropertyType);
            Array allowedEnumValues = Enum.GetValues(propInfo.PropertyType);
            IsEditable = clsSharedMethods.SharedMethods.CheckAccess(BLDatasource, "UPDATE", propInfo.Name);
            if (Attribute.IsDefined(propInfo, typeof(EnumAllowedValuesAttribute), false))
            {
                try
                {
                    EnumAllowedValuesAttribute allowedEnumConstantValues =
                        (EnumAllowedValuesAttribute)Attribute.GetCustomAttribute(propInfo, typeof(EnumAllowedValuesAttribute), true);
                    string resourcefile = allowedEnumConstantValues.ResourcesFile;
                    if (constantEnumStrings.Length > 0)
                    {
                        if (resourcefile != "")
                        {
                            System.Resources.ResourceManager _resourceManager;
                            Assembly resourceAssembly = BLDatasource.GetType().Assembly;
                            _resourceManager = new System.Resources.ResourceManager(
                                 resourcefile, resourceAssembly);
                            _resourceManager.IgnoreCase = true;

                            for (int iterator = 0; iterator < constantEnumStrings.Length; iterator++)
                            {
                                allowedItems.Add(new ListItem(_resourceManager.GetString(constantEnumStrings[iterator]), System.Convert.ToString(allowedEnumValues.GetValue(iterator))));
                            }
                        }
                        else
                        {
                            for (int iterator = 0; iterator < constantEnumStrings.Length; iterator++)
                            {
                                allowedItems.Add(new ListItem(constantEnumStrings[iterator], System.Convert.ToString(allowedEnumValues.GetValue(iterator))));
                            }
                        }
                    }
                }
                catch (SecurityException)
                {

                }
            }
            else
            {
                for (int iterator = 0; iterator < constantEnumStrings.Length; iterator++)
                {
                    allowedItems.Add(new ListItem(constantEnumStrings[iterator], System.Convert.ToString(allowedEnumValues.GetValue(iterator))));
                }
            }
            return true;
        }

        internal virtual void CreateBoolFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            if (IsReadable)
            {
                CheckBox cbxBoolProperty = null;
                string CtrlID = propInfo.Name;
                if (CtrlNode.Attributes["ID"] != null)
                {
                    CtrlID = CtrlNode.Attributes["ID"].Value;
                }
                if (PlaneFieldControls[CtrlID] == null)
                {
                    cbxBoolProperty = new CheckBox();
                    cbxBoolProperty.ID = CtrlID;
                }
                else
                {
                    cbxBoolProperty = (CheckBox)PlaneFieldControls[CtrlID];
                }
                SetAppareance(cbxBoolProperty, CtrlNode);
                if (CtrlNode.Attributes["Text"] != null)
                {
                    cbxBoolProperty.Text = CtrlNode.Attributes["Text"].Value;
                }
                cbxBoolProperty.Checked = (bool)propInfo.GetValue(BLDatasource, null);
                Container.Controls.Add(cbxBoolProperty);
                PlaneFieldControls[CtrlID] = cbxBoolProperty;
            }
            else
            {
                Literal boolPropertyWithoutPermissions = new Literal();
                boolPropertyWithoutPermissions.Text = "-";
                Container.Controls.Add(boolPropertyWithoutPermissions);
            }
        }

        internal virtual void CreateCollectionFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, PropertyInfo propInfo, Control Container)
        {
            Literal BOliteral = new Literal();
            if (Attribute.IsDefined(propInfo, typeof(BCAllowedValuesAttribute), false))
            {
                BOliteral.Text = Resources.ErrorMessages.errAllowedValuesAttibute;
            }
            else
            {
                if (IsReadable)
                {
                    BOliteral.Text = BLDatasource.ToString();
                }
                else
                {
                    BOliteral.Text = strNotReadable;
                }
            }
            Container.Controls.Add(BOliteral);
        }

        internal virtual void FillAllowedValues(string fieldName, XmlNode CtrlNode, string typeName, object BLDatasource, bool IsReadable, bool IsEditable, IList allowedItems, bool AsRadioButton, PropertyInfo propInfo, Control Container)
        {
            if ((CtrlNode.Attributes["SelectionType"] != null) && (CtrlNode.Attributes["SelectionType"].Value.ToUpper() == "LISTBOX"))
            {
                WebControls.ListBox list = null;
                string listID = fieldName;
                if (CtrlNode.Attributes["ID"] != null)
                {
                    listID = CtrlNode.Attributes["ID"].Value;
                }
                if (PlaneFieldControls[listID] == null)
                {
                    list = new Axelerate.BusinessLayerUITools.WebControls.ListBox();
                    list.ID = listID;

                }
                else
                {
                    list = (WebControls.ListBox)PlaneFieldControls[listID];
                }
                SetAppareance(list, CtrlNode);

                if (CtrlNode.Attributes["OnMouseOverCssClass"] != null)
                {
                    list.OnMouseOverCssClass = CtrlNode.Attributes["OnMouseOverCssClass"].Value;
                }
                if (CtrlNode.Attributes["SelectedItemCssClass"] != null)
                {
                    list.SelectedItemCssClass = CtrlNode.Attributes["SelectedItemCssClass"].Value;
                }
                if (CtrlNode.Attributes["InnerControlCssClass"] != null)
                {
                    list.InnerControlCssClass = CtrlNode.Attributes["InnerControlCssClass"].Value;
                }
                if (CtrlNode.Attributes["Columns"] != null)
                {
                    list.Columns = Convert.ToInt32(CtrlNode.Attributes["Columns"].Value);
                }
                if (CtrlNode.Attributes["DataIconField"] != null)
                {
                    list.DataIconField = CtrlNode.Attributes["DataIconField"].Value;
                }
                if (CtrlNode.Attributes["DataTextField"] != null)
                {
                    list.DataTextField = CtrlNode.Attributes["DataTextField"].Value;
                }
                if (CtrlNode.Attributes["DataValueField"] != null)
                {
                    list.DataValueField = CtrlNode.Attributes["DataValueField"].Value;
                }
                list.DataSource = allowedItems;            //////////////////////////
                list.Enabled = IsEditable;

                list.Width = new Unit(100, UnitType.Percentage);
                Container.Controls.Add(list);
                PlaneFieldControls[listID] = list;
            }
            else
            {
                bool Dropdown = AsRadioButton;
                if (!Dropdown) ///TODO: Change this to a special attribute
                {
                    DropDownList DDlist = null;
                    string listID = fieldName;
                    if (CtrlNode.Attributes["ID"] != null)
                    {
                        listID = CtrlNode.Attributes["ID"].Value;
                    }
                    if (PlaneFieldControls[listID] == null)
                    {
                        DDlist = new DropDownList();
                        DDlist.ID = listID;
                    }
                    else
                    {
                        DDlist = (DropDownList)PlaneFieldControls[listID];
                    }
                    DDlist.AutoPostBack = false;
                    DDlist.CssClass = "MIP-Small";
                    DDlist.DataSource = allowedItems;
                    if (IsReadable)
                    {
                        string propertyValue = propInfo.GetValue(BLDatasource, null).ToString();
                        //if the dropdown list has the value, select it else, add the value and select it.
                        bool FindItem = false;
                        foreach (ListItem itm in allowedItems)
                        {
                            if (itm.Value == propertyValue)
                            {
                                FindItem = true;
                                break;
                            }
                        }
                        if (!FindItem)
                        {
                            ListItem selectedItem = new ListItem(propertyValue + "*", propertyValue);
                            allowedItems.Add(selectedItem);
                        }
                        DDlist.SelectedValue = propertyValue;
                    }

                    DDlist.Enabled = IsEditable;

                    DDlist.Width = new Unit(100, UnitType.Percentage);
                    Container.Controls.Add(DDlist);
                    PlaneFieldControls[listID] = DDlist;
                }
                else
                {
                    RadioButtonList rbOptionList = null;
                    string listID = fieldName;
                    if (CtrlNode.Attributes["ID"] != null)
                    {
                        listID = CtrlNode.Attributes["ID"].Value;
                    }
                    if (PlaneFieldControls[listID] == null)
                    {
                        rbOptionList = new RadioButtonList();
                        rbOptionList.ID = listID;
                        if (CtrlNode.Attributes["ID"] != null)
                        {
                            rbOptionList.ID = CtrlNode.Attributes["ID"].Value;
                        }
                    }
                    else
                    {
                        rbOptionList = (RadioButtonList)PlaneFieldControls[listID];
                    }

                    rbOptionList.AutoPostBack = false;
                    rbOptionList.DataTextField = "Text";
                    rbOptionList.DataValueField = "Value";
                    rbOptionList.DataSource = allowedItems;
                    if (IsReadable)
                    {
                        string propertyValue = propInfo.GetValue(BLDatasource, null).ToString();
                        //if the radio button list has the value, select it else, add the value and select it.
                        bool FindItem = false;
                        foreach (ListItem itm in allowedItems)
                        {
                            if (itm.Value == propertyValue)
                            {
                                FindItem = true;
                                break;
                            }
                        }
                        if (!FindItem)
                        {
                            ListItem selectedItem = new ListItem(propertyValue + "*", propertyValue);
                            allowedItems.Add(selectedItem);
                        }
                        rbOptionList.SelectedValue = propertyValue;
                    }
                    rbOptionList.DataSource = allowedItems;
                    rbOptionList.DataBind();

                    rbOptionList.Enabled = IsEditable;

                    rbOptionList.Width = new Unit(100, UnitType.Percentage);
                    Container.Controls.Add(rbOptionList);
                    PlaneFieldControls[listID] = rbOptionList;
                }
            }
        }

        internal virtual void CreateFieldControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, bool AsRadioButton, PropertyInfo propInfo, Control Container)
        {
            bool HasAllowedValues = false;
            IList allowedItems = new ArrayList();
            if (propInfo.PropertyType == typeof(string))
            {
                HasAllowedValues = CreateStringFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            else if (propInfo.PropertyType == typeof(int) || propInfo.PropertyType == typeof(float) || propInfo.PropertyType == typeof(double))
            {
                CreateNumericFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            else if (propInfo.PropertyType == typeof(Enum))
            {
                CreateEnumFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            else if (propInfo.PropertyType == typeof(DateTime))
            {
                CreateDateTimeControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            else if (propInfo.PropertyType == typeof(bool))
            {
                CreateBoolFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            else if (propInfo.PropertyType.IsEnum)
            {
                #region Enum Control Creation
                /*                if (Attribute.IsDefined(propInfo, typeof(EnumAllowedValuesAttribute), false))
                {
                    HasAllowedValues = true;
                    try
                    {
                        EnumAllowedValuesAttribute allowedEnumValues =
                            (EnumAllowedValuesAttribute)Attribute.GetCustomAttribute(propInfo, typeof(EnumAllowedValuesAttribute), true);
                        string resourcefile = allowedEnumValues.ResourcesFile;
                        Array allowedValues = Enum.GetValues(propInfo.PropertyType);
                        if (allowedValues.Length > 0)
                        {
                            if (resourcefile != "")
                            {
                                System.Resources.ResourceManager _resourceManager;
                                Assembly resourceAssembly = Datasource.GetType().Assembly;
                                _resourceManager = new System.Resources.ResourceManager(
                                     resourcefile, resourceAssembly);
                                _resourceManager.IgnoreCase = true;

                                for (int i = 0; i < allowedValues.Length; i++)
                                {
                                    string valueName = allowedValues.GetValue(i).ToString();
                                    allowedItems.Add(new ListItem(_resourceManager.GetString(valueName), valueName));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < allowedValues.Length; i++)
                                {
                                    allowedItems.Add(new ListItem(allowedValues.GetValue(i).ToString()));
                                }
                            }
                        }
                    }
                    catch (SecurityException se)
                    {

                    }
                }*/
                #endregion
            }
            else if (typeof(BLBusinessBase).IsInstanceOfType(propInfo.GetValue(BLDatasource, null)))
            { //If the property is a fieldcontrol and it's type is a BLBusinessBase, it loads all the allowed values available for that kind of object.
                CreateCollectionFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, allowedItems, propInfo, Container);
            }
            if (HasAllowedValues) //If the property has allowed values, then create a dropdown list and add the controls.
            {
                FillAllowedValues(fieldName, CtrlNode, typeName, BLDatasource, IsReadable, IsEditable, allowedItems, AsRadioButton, propInfo, Container);
            }
        }

        internal virtual void CreateBoEditorControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, bool AsRadioButton, PropertyInfo propInfo, string BOControlLayout, Control Container)
        {
            if (typeof(BLBusinessBase).IsInstanceOfType(propInfo.GetValue(BLDatasource, null)))  //Check if the property is an instance of a BLBusinessBase
            {
                if (IsReadable)
                {
                    wptBOEditor editor = null;
                    if (ChildEditorControls[fieldName] == null)
                    {
                        editor = new wptBOEditor((BLBusinessBase)propInfo.GetValue(BLDatasource, null), this);
                    }
                    else
                    {
                        editor = (wptBOEditor)ChildEditorControls[fieldName];
                    }
                    editor.FactoryParameters = fieldName;
                    editor.LayoutName = BOControlLayout;
                    editor.ClassName = propInfo.PropertyType.AssemblyQualifiedName;
                    Container.Controls.Add(editor);
                    ChildEditorControls[fieldName] = editor;
                }
                else
                {
                    Literal editorNotReadable = new Literal();
                    editorNotReadable.Text = strNotReadable;
                    Container.Controls.Add(editorNotReadable);
                }
            }
        }

        internal virtual void CreateGridControl(string fieldName, string typeName, XmlNode CtrlNode, object BLDatasource, bool IsReadable, bool IsEditable, bool AsRadioButton, PropertyInfo propInfo, string BOControlLayout, Control Container)
        {
            string factoryParams = "";
            string factoryMethod = "";
            string className = "";
            string layoutName = "";
            string ConsumerType = "";
            string ReturnedProperty = "";
            string Id = "";
            string HeaderCss = "";
            string BodyCss = "";
            string WPTitle = "";
            bool ShowHeader = false;
            bool RemovableItems = false;
            string mStyle = "";

            if (CtrlNode.Attributes["ID"] != null)
            {
                Id = CtrlNode.Attributes["ID"].Value;
            }
            if (Id == "")
            {
                Id = fieldName;
            }

            if (CtrlNode.Attributes["ReturnedProperty"] != null)
            {
                ReturnedProperty = CtrlNode.Attributes["ReturnedProperty"].Value;
            }
            if (CtrlNode.Attributes["RemovableItems"] != null)
            {
                RemovableItems = System.Convert.ToBoolean(CtrlNode.Attributes["RemovableItems"].Value);
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
            if (CtrlNode.Attributes["ShowHeader"] != null)
            {
                ShowHeader = System.Convert.ToBoolean(CtrlNode.Attributes["ShowHeader"].Value);
            }
            if (CtrlNode.Attributes["HeaderCss"] != null)
            {
                HeaderCss = CtrlNode.Attributes["HeaderCss"].Value;
            }
            if (CtrlNode.Attributes["BodyCss"] != null)
            {
                BodyCss = CtrlNode.Attributes["BodyCss"].Value;
            }
            if (CtrlNode.Attributes["Style"] != null)
            {
                mStyle = CtrlNode.Attributes["Style"].Value;
            }
            if (CtrlNode.Attributes["Title"] != null)
            {
                WPTitle = CtrlNode.Attributes["Title"].Value;
            }
            if (CtrlNode.Attributes["ComponentBehavior"] != null)
            {
                if (CtrlNode.Attributes["ComponentBehavior"].Value.ToUpper() == "SELECTOR")
                {
                    wptGrid GridSelector;

                    if (ChildEditorControls[Id] == null)
                    {
                        GridSelector = new wptGrid();
                        ChildEditorControls.Add(Id, GridSelector);
                    }
                    else
                    {
                        GridSelector = (wptGrid)ChildEditorControls[Id];
                    }

                    GridSelector.ID = Id;
                    GridSelector.ClassName = className;
                    GridSelector.FactoryMethod = factoryMethod;
                    GridSelector.FactoryParameters = factoryParams;
                    GridSelector.LayoutName = layoutName;
                    GridSelector.ConsumerType = ConsumerType;
                    GridSelector.ComponentBehaviorType = CtrlNode.Attributes["ComponentBehavior"].Value;
                    GridSelector.ReturnedProperty = ReturnedProperty;
                    Container.Controls.Add(GridSelector);
                    GridSelector.ShowHeader = ShowHeader;
                    GridSelector.HeaderCssClass = HeaderCss;
                    GridSelector.Title = WPTitle;
                    GridSelector.BodyCssClass = BodyCss;
                    GridSelector.RemovableItems = RemovableItems;
                    GridSelector.Style.Value = mStyle;
                    //                  ChildEditorControls.Add(fieldName, GridSelector);

                    PropertyInfo properties = m_Datasource.GetType().GetProperty(fieldName);
                    IBLListBase collection = (IBLListBase)properties.GetValue(m_Datasource, null);
                    if (collection != null)
                    {
                        foreach (BLBusinessBase selectedItem in collection)
                        {
                            GridSelector.SelectItem(selectedItem.DataKey.ToString());
                        }
                    }
                }
            }
            //       object source = (IBLListBase)m_Datasource;

            else if (className == "" || layoutName == "")
            {
                if (typeof(IBLListBase).IsAssignableFrom(propInfo.PropertyType))
                {
                    if (IsReadable)
                    {
                        wptGrid grid;
                        if (ChildEditorControls[Id] == null)
                        {
                            grid = new wptGrid((IBLListBase)propInfo.GetValue(BLDatasource, null));
                            ChildEditorControls.Add(Id, grid);
                        }
                        else
                        {
                            grid = (wptGrid)ChildEditorControls[Id];
                        }

                        grid.ID = Id;
                        grid.FactoryParameters = "GUID,null";
                        grid.FactoryMethod = "TryGetObjectByGUID";
                        grid.LayoutName = BOControlLayout;
                        grid.ClassName = propInfo.PropertyType.AssemblyQualifiedName;
                        grid.DisplayLayouts = true;
                        grid.ConsumerType = ConsumerType;
                        grid.Style.Value = mStyle;
                        if (CtrlNode.Attributes["SelectorProvider"] != null)
                        {
                            ConnectionsToStablish[fieldName] = CtrlNode.Attributes["SelectorProvider"].Value;
                            //                                          grid.RegisterProvider = CtrlNode.Attributes["SelectorProvider"].Value;
                        }
                        grid.ShowHeader = ShowHeader;
                        grid.HeaderCssClass = HeaderCss;
                        grid.Title = WPTitle;
                        grid.BodyCssClass = BodyCss;
                        Container.Controls.Add(grid);
                        //                      ChildEditorControls.Add(fieldName, grid);
                    }
                    else
                    {
                        Literal gridNotReadable = new Literal();
                        gridNotReadable.Text = strNotReadable;
                        Container.Controls.Add(gridNotReadable);
                    }
                }
            }
            else //If Consumer
            {
                wptGrid GridSelector;
                if (ChildEditorControls[Id] == null)
                {
                    GridSelector = new wptGrid();
                    ChildEditorControls.Add(Id, GridSelector);
                }
                else
                {
                    GridSelector = (wptGrid)ChildEditorControls[Id];
                }
                GridSelector.ID = Id;

                GridSelector.ClassName = className;
                //GridSelector.FactoryMethod = factoryMethod;
                //GridSelector.FactoryParameters = factoryParams;
                GridSelector.LayoutName = layoutName;
                GridSelector.ConsumerType = ConsumerType;
                GridSelector.ComponentBehaviorType = "Consumer";
                GridSelector.Style.Value = mStyle;
                if (CtrlNode.Attributes["SelectorProvider"] != null)
                {
                    ConnectionsToStablish[CtrlNode.Attributes["SelectorProvider"].Value] = Id;
                    //                                          grid.RegisterProvider = CtrlNode.Attributes["SelectorProvider"].Value;
                }
                GridSelector.ShowHeader = ShowHeader;
                GridSelector.HeaderCssClass = HeaderCss;
                GridSelector.Title = WPTitle;
                GridSelector.BodyCssClass = BodyCss;
                GridSelector.ReturnedProperty = ReturnedProperty;
                Container.Controls.Add(GridSelector);
                //               ChildEditorControls.Add(fieldName, GridSelector);
            }
        }

        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        protected override void EnsureDatasourceObject()
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        public override void SetItemFields()
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
                                default:
                                    if (typeof(ICustomValueEditor).IsAssignableFrom(Ctrl.GetType()))
                                    {
                                        ICustomValueEditor CustomEditor = (ICustomValueEditor)Ctrl;
                                        if (CanRead || CustomEditor.Value != null)
                                        {
                                            try
                                            {
                                                Object propertyValue = Convert.ChangeType(CustomEditor.Value, property.PropertyType);
                                                property.SetValue(m_Datasource, propertyValue, null);
                                            }
                                            catch (InvalidCastException ex)
                                            {
                                                //if this is the case we need show a message in the screen with a invalid value error for this field.
                                                //fieldTextBox.
                                            }
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
        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        protected override string GetXMLLayout()
        {
            Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout Layout;
            Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), LayoutName);
            if (Layout.LayoutXML == "")
            {
                Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), "Default");
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
                    }
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
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            foreach (string ID in m_CalendarExtenders.Keys)
            {
                AjaxControlToolkit.CalendarExtender Calendar = (AjaxControlToolkit.CalendarExtender)m_CalendarExtenders[ID];
                Calendar.TargetControlID = ((TextBox)PlaneFieldControls[ID]).ClientID;
            }
        }
    }
}
