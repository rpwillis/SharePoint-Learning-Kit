using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties;
using System.Web.UI;
using System.Web.UI.WebControls;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptCustomPropertiesEditor : clsBaseLayoutWP
    {

        #region Private Properties

        private clsProperties m_CustomProperties = null;
        private clsPropertyValues m_CustomPropertiesValues = null;
        private clsUILayout m_CustomPropertyLayout = null;

        #endregion


        #region Overrides

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
            clsProperty Fld = null;
            clsPropertyValue pValue = null;

            if (fieldName != "")
            {
                Fld = (clsProperty)m_CustomProperties.Find("Name", fieldName);
                pValue = GetPropertyValue(Fld, m_CustomPropertiesValues);
            }

            switch (typeName.ToLower())
            {
                case "fieldcontrol": //simple field or popup if has allowedvalues

                    if (Fld == null)
                    {
                        break; //because this type of control require the fieldname and the clsProperty (Fld).
                    }
                    if (Fld.AllowedValues != "")
                    {
                        DropDownList DDlist = null;
                        if (PlaneFieldControls[Fld.Name] != null)
                        {
                            DDlist = (DropDownList)PlaneFieldControls[Fld.Name];
                        }
                        else
                        {
                            DDlist = new DropDownList();
                        }
                        //DDlist.AutoPostBack = true; this properties don't alter other properties, for this reason not need do a postback.
                        DDlist.AutoPostBack = false;
                        DDlist.CssClass = "MIP-Small";
                        DDlist.ID = Fld.Name;
                        //DDlist.ToolTip = Fld.FieldDefinition.HelpText;
                        //DDlist.Enabled = Fld.IsEditable; this property all time accept edition.
                        string[] AlloweValuesList = Fld.AllowedValues.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        //if (!Fld.IsEditable) //all custom properites are writeables in this moment.
                        //{
                        //    DDlist.BackColor = System.Drawing.Color.LightGray;
                        //}
                        if (!Fld.Required)
                        {
                            DDlist.Items.Add("");
                        }
                        foreach (string value in AlloweValuesList)
                        {
                            DDlist.Items.Add(value);
                        }

                        if (pValue.PropertyValue != null && pValue.PropertyValue.Trim() != "")
                        {
                            if (DDlist.Items.Count == 0)
                            {
                                //DDlist.Items.Add(Fld.Value.ToString());
                                foreach (string value in AlloweValuesList)
                                {
                                    DDlist.Items.Add(value);
                                }
                            }
                            DDlist.SelectedValue = pValue.PropertyValue;
                        }
                        else
                        {
                            if (DDlist.Items.Count > 0)
                            {
                                if (DDlist.Items.FindByValue(Fld.DefaultValue) != null)
                                {
                                    DDlist.SelectedValue = Fld.DefaultValue;
                                }
                                else
                                {
                                    DDlist.SelectedValue = DDlist.Items[0].Value;
                                }
                            }
                        }

                        DDlist.Width = new Unit(100, UnitType.Percentage);
                        //lbl.AssociatedControlID = DDlist.ID;
                        Container.Controls.Add(DDlist);
                        PlaneFieldControls[Fld.Name] = DDlist;
                    }
                    else
                    {
                        TextBox txtField = null;
                        if (PlaneFieldControls[Fld.Name] != null)
                        {
                            txtField = (TextBox)PlaneFieldControls[Fld.Name];
                        }
                        else
                        {
                            txtField = new TextBox();
                        }
                        txtField.CssClass = "MIP-Small";
                        txtField.BorderStyle = BorderStyle.Solid;
                        txtField.BorderWidth = new Unit(1, UnitType.Pixel);
                        txtField.BorderColor = System.Drawing.Color.Black;
                        txtField.ID = fieldName;
                        if (((BLBusinessBase)Datasource)[fieldName] != null)
                        {
                            txtField.Text = (((BLBusinessBase)Datasource)[fieldName]).ToString();
                        }
                        else
                        {
                            txtField.Text = Fld.DefaultValue;
                        }
                        txtField.Width = new Unit(99.99, UnitType.Percentage);
                        //lbl.AssociatedControlID = txtField.ID;
                        //if (!Fld.IsEditable)
                        //{
                        //    txtField.BackColor = System.Drawing.Color.LightGray;
                        //}
                        Container.Controls.Add(txtField);
                        PlaneFieldControls[Fld.Name] = txtField;
                    }
                    if (Fld.Required && !IsInEditMode)
                    {
                        RequiredFieldValidator FldValidator = new RequiredFieldValidator();
                        FldValidator.CssClass = "MIP-SmallError";
                        FldValidator.Text = "";//Resources.Shared.strRequired;
                        FldValidator.ErrorMessage = "Field Requiered";//Resources.Shared.strRequired;
                        FldValidator.SetFocusOnError = true;
                        FldValidator.ControlToValidate = Fld.Name;
                        Container.Controls.Add(FldValidator);
                    }
                    break;
                case "htmlfieldcontrol": //Comment
                    if (Fld == null)
                    {
                        break; //because this type of control requires the fieldname and the clsProperty (Fld).
                    }
                    CreateHtmlEditorControl(fieldName, Datasource, Container);
                    break;
                default:
                    SetError(string.Format(Resources.GenericWebParts.strErrorControlTypeNotSupported, typeName));
                    break;


            }

        }

        protected override void EnsureDatasourceObject()
        {
            if (m_CustomProperties == null || m_CustomPropertyLayout == null || m_CustomPropertiesValues == null)
            {
                m_CustomProperties = clsProperties.GetCollection(Datasource.GetType());
                m_CustomPropertyLayout = clsUILayout.PropertyLayout(Datasource.GetType(), "");
                m_CustomPropertiesValues = clsPropertyValues.GetCollection(Datasource.GetType(), (string)(m_Datasource as BLBusinessBase)["GUID"]);
            }
        }

        public override void SetItemFields()
        {
            //first we need Init Business Object, this is done in this moment by the set of m_Datasource
            EnsureDatasourceObject();
            //the we can iterate by the custom properties collection
            //Ensure that the controls was created to we can get the PlaneFieldControls collection
            EnsureChildControls();
            if (m_CustomProperties != null)
            {
                foreach (clsProperty property in m_CustomProperties)
                {
                    Control Ctrl = (Control)PlaneFieldControls[property.Name];
                    if (Ctrl != null)
                    {
                        clsPropertyValue pValue = GetPropertyValue(property, m_CustomPropertiesValues);

                        switch (Ctrl.GetType().FullName)
                        {
                            case "System.Web.UI.WebControls.TextBox":
                                TextBox fieldTextBox = (TextBox)Ctrl;
                                if (fieldTextBox.Text != "")
                                {
                                    if (fieldTextBox.Text != "")
                                    {
                                        try
                                        {
                                            pValue.PropertyValue = Convert.ChangeType(fieldTextBox.Text, Type.GetType(property.Type)).ToString();
                                        }
                                        catch (InvalidCastException ex)
                                        {
                                            //if this is the case we need show a message in the screen with a invalid value error for this field.
                                            //fieldTextBox.
                                        }
                                    }
                                }
                                break;
                            case "System.Web.UI.WebControls.DropDownList":
                                DropDownList fieldDDList = (DropDownList)Ctrl;
                                try
                                {
                                    pValue.PropertyValue = Convert.ChangeType(fieldDDList.SelectedValue, Type.GetType(property.Type)).ToString();
                                }
                                catch (InvalidCastException ex)
                                {
                                    //if this is the case we need show a message in the screen with a invalid value error for this field.
                                    //fieldTextBox.
                                }
                                break;
                            case "Infragistics.WebUI.WebHtmlEditor.WebHtmlEditor":
                                System.Web.UI.WebControls.TextBox fieldEditor = (System.Web.UI.WebControls.TextBox)Ctrl;
                                try
                                {
                                    pValue.PropertyValue = (string)fieldEditor.Text;
                                }
                                catch (InvalidCastException ex)
                                {
                                    //if this is the case we need show a message in the screen with a invalid value error for this field.
                                    //fieldTextBox.
                                }
                                break;
                        }
                    }
                }
            }
        }

        protected override string GetXMLLayout()
        {
            EnsureDatasourceObject();
            String Layout = m_CustomPropertyLayout.LayoutXML;
            if (Layout == "")
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
            return Layout;
        }

        public override bool HasDatasource()
        {
            if (m_CustomProperties == null || m_CustomPropertyLayout == null || m_CustomPropertiesValues == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool IsNew()
        {
            return false;
        }

        protected override void CrateUnlayoutProperies(object Datasource, Control nContainer, bool IsPostback)
        {
            //Create a XML Node that agroup all this properties
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

            foreach (clsProperty Prop in m_CustomProperties)
            {
                //check if the property was not all ready defined.
                if (PlaneFieldControls[Prop.Name] == null)
                {
                    System.Xml.XmlNode CategoryGroupNode = (System.Xml.XmlNode)SubGroupNodes[Prop.Category];
                    System.Xml.XmlNode CategoryGroupColumn = null;
                    if (CategoryGroupNode == null)
                    {
                        CategoryGroupNode = doc.CreateNode(System.Xml.XmlNodeType.Element, "GROUP", null);
                        Attr = doc.CreateAttribute("Label");
                        Attr.Value = Prop.Category;
                        CategoryGroupNode.Attributes.Append(Attr);
                        SubGroupNodes[Prop.Category] = CategoryGroupNode;

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
                    Attr.Value = "FieldControl"; //defult type for any property
                    ControlNode.Attributes.Append(Attr);

                    Attr = doc.CreateAttribute("FieldName");
                    Attr.Value = Prop.Name;
                    ControlNode.Attributes.Append(Attr);

                    Attr = doc.CreateAttribute("Label");
                    Attr.Value = Prop.Name + ":"; //default display name for the field
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


        #endregion

        public BLBusinessBase Datasource
        {
            get
            {
                m_Datasource = ViewState["CPEDatasource"];
                return (BLBusinessBase)m_Datasource;
            }
            set
            {
                ViewState["CPEDatasource"] = value;
                m_CustomProperties = null;
                m_CustomPropertyLayout = null;
                m_CustomPropertiesValues = null;
                m_Datasource = value;

            }
        }

        private clsPropertyValue GetPropertyValue(clsProperty property, clsPropertyValues Datasource)
        {
            clsPropertyValue pValue = (clsPropertyValue)Datasource.Find("MasterGUID", property.GUID);
            if (pValue == null)
            {
                pValue = (clsPropertyValue)Datasource.NewBusinessObject();
                pValue.MasterObject = property;
                pValue.ObjectGUID = (string)((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)m_Datasource)["GUID"];
                // pValue.MarkAsChild();
                m_CustomPropertiesValues.Add(pValue);
            }
            return pValue;
        }



       

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
            if ((CommandName.ToLower() == "insert" || CommandName.ToLower() == "update") && Datasource != null)
            {
                if (m_CustomPropertiesValues.IsValid)
                {
                    m_CustomPropertiesValues = m_CustomPropertiesValues.Save();
                }
            }
        }

       
    }
}
