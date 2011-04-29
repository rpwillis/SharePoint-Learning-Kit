/* Copyright (c) Microsoft Corporation. All rights reserved. */

//CustomCheckBoxList.cs
//
//Implementation of ServerControl Class CustomCheckBoxList
//


using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Text;
using System.IO;
using Microsoft.SharePointLearningKit;
using System.Web.UI.HtmlControls;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Specialized;

namespace Microsoft.SharePointLearningKit.WebControls
{    
   /// <summary>
   ///  Slk custom CheckBoxList control renders  Checkbox control and text in separate &lt;td&gt;'s
   ///  as a tabular formatted checkbox list.
   ///  usage: &lt;{0}:CustomCheckBoxList runat="server" ID="customCheckBoxList" 
   ///              DataTextField="CheckBoxText" DataValueField="CheckBoxValue"  
   ///              DataStateField="CheckBoxState" DataToolTipField="CheckBoxToolTipText"&gt;
   ///         &lt;/{0}:CustomCheckBoxList&gt;;
   /// </summary>

    [ToolboxData("<{0}:CustomCheckBoxList runat=server></{0}:CustomCheckBoxList>")]
   public class  CustomCheckBoxList : DataBoundControl, INamingContainer, IPostBackDataHandler,IValidator 
   {

        #region Private Fields
        /// <summary>
        /// CheckBox Control to Repeat
        /// </summary>
        private CheckBox m_controlToRepeat;
        /// <summary>
        /// Label Control To Associate to checkbox
        /// </summary>
        private Label m_controlToAssociate;
        /// <summary>
        /// Holds Header Text
        /// </summary>
        private string m_headerText;
        /// <summary>
        /// Holds Item Collection 
        /// </summary>
        private CheckBoxItemCollection m_items;

        /// <summary>
        /// CheckBox Control to Repeat
        /// </summary>
        private CheckBox ControlToRepeat
        {
            get
            {
                if (m_controlToRepeat == null)
                {
                    this.m_controlToRepeat = new CheckBox();
                    this.m_controlToRepeat.EnableViewState = false;
                    this.Controls.Add(this.m_controlToRepeat);
                }
                return this.m_controlToRepeat;
            }
        }        
        /// <summary>
        /// Label Control To Associate to checkbox
        /// </summary>
        private Label ControlToAssociate
        {
            get
            {
                if (m_controlToAssociate == null)
                {
                    this.m_controlToAssociate = new Label();
                    this.m_controlToAssociate.EnableViewState = false;
                    this.m_controlToAssociate.AssociatedControlID = this.ControlToRepeat.UniqueID;
                    this.Controls.Add(this.m_controlToAssociate);
                }
                return this.m_controlToAssociate;
            }
        }
        /// <summary>
        /// Holds IsValid 
        /// </summary>
        private bool m_valid = true;
        /// <summary>
        /// Holds ErrorMessage
        /// </summary>
        private string m_errorMessage;
        /// <summary>
        /// Holds IsRequired 
        /// </summary>
        private bool m_isRequired;

       #endregion

        #region Constructor
        /// <summary>
        /// Defaults the Databound Fields to be bind 
        /// with repeated Control while databinding
        /// </summary>
        public CustomCheckBoxList()
        {
            DataTextField = "Text";
            DataValueField = "Value";
            DataStateField = "Selected";
            DataToolTipField = "ToolTip";
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Sets or gets the Header Text to be displayed 
        /// On Top of the Control List
        /// </summary>
        internal string HeaderText
        {
            get
            {
                object o = ViewState["HeaderText"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set
            {
                m_headerText = value;
                ViewState["HeaderText"] = m_headerText;
            }
        }
        /// <summary>
        /// Control's Unique ID
        /// </summary>
        public override string UniqueID
        {
            get
            {
                return base.UniqueID;
            }
        }
        /// <summary>
        /// DataTextField to bind with Text Field of ControlToRepeat
        /// </summary>
        internal string DataTextField
        {
            get
            {
                object o = ViewState["DataTextField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set { ViewState["DataTextField"] = value; }
        }
        /// <summary>
        /// DataValueField to bind with Value Field of ControlToRepeat
        /// </summary>
        internal string DataValueField
        {
            get
            {
                object o = ViewState["DataValueField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set { ViewState["DataValueField"] = value; }
        }

        /// <summary>
        /// DataStateField to bind with State(Checked) of ControlToRepeat
        /// </summary>
        internal string DataStateField
        {
            get
            {
                object o = ViewState["DataStateField"];
                if (o == null)
                    return false.ToString(CultureInfo.InvariantCulture);
                return (string)o;
            }
            set { ViewState["DataStateField"] = value; }
        }
        /// <summary>
        /// DataToolTipField to bind with ToolTip Field of ControlToRepeat
        /// </summary>
        internal string DataToolTipField
        {
            get
            {
                object o = ViewState["DataToolTipField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set { ViewState["DataToolTipField"] = value; }
        }        
       
        /// <summary>
        /// Collection of ControlItems to be repeated
        /// </summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        internal CheckBoxItemCollection Items
        {
            get
            {
                if (m_items == null)
                {
                    m_items = new CheckBoxItemCollection();
                    if (base.IsTrackingViewState)
                        m_items.TrackViewState();
                }
                return m_items;
            }


        }
        /// <summary>
        /// Selected Item Index Value
        /// </summary>
        internal int SelectedIndex
        {
            get
            {
                for (int num1 = 0; num1 < this.Items.Count; num1++)
                {
                    if (this.Items[num1].Selected)
                    {
                        return num1;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Gets/Sets a value indicating whether 
        /// the control has required to have atleast one checkbox selected 
        /// </summary>
        internal bool IsRequired
        {        
                     
            get
            {
                object o = ViewState["IsRequired"];
                if (o == null)
                    return false;
                return (bool)o;                
            }
            set
            {
                m_isRequired = value;
                ViewState["IsRequired"] = m_isRequired;
            }
        }
        #endregion

        #region Private and Protected Methods
        
        #region OnInit
        /// <summary>
        /// OnInit
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ToolTip = this.ID;
            if (this.Page != null)
            {
                //Registering control Requires for PostBack
                this.Page.RegisterRequiresPostBack(this);
            }
            Page.Validators.Add(this);
        }
        #endregion

        #region LoadViewState
        /// <summary>
        /// LoadsViewState
        /// </summary>
        /// <param name="savedState">savedState</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair p = (Pair)savedState;
                base.LoadViewState(p.First);
                Items.LoadViewState(p.Second);
            }
            else
                base.LoadViewState(null);
        }
        #endregion        
   
        #region OnPreRender
        /// <summary>
        /// OnPreRender Event Handler
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
            base.OnPreRender(e);
        }
        #endregion

        #region Render
        /// <summary>
        /// Control Render Method
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            //Renders the Validation Error Message on Top of the Control.

            if (!IsValid)
            {
                if (!String.IsNullOrEmpty(SlkUtilities.Trim(this.ErrorMessage)))
                {
                    Label lblErrorText = new Label();
                    lblErrorText.Text = SlkUtilities.GetHtmlEncodedText(this.ErrorMessage); 
                    lblErrorText.CssClass = "ms-formvalidation";

                    using (new HtmlBlock(HtmlTextWriterTag.Div, 0, writer))
                    {
                        lblErrorText.RenderControl(writer);
                    }
                }
            }
            //Render the CheckBox List
            RenderCustomCheckBox(writer);             
        }
        #endregion

        #region RenderCustomCheckbox
        /// <summary>
        /// Render the Custom made CheckboxControl parses the content of
        /// List Items and Render the Items
        /// </summary>       
        /// <param name="htmlTextWriter">HtmlTextWriter</param>
        protected void RenderCustomCheckBox(HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "UserGenericText");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                if (!String.IsNullOrEmpty(SlkUtilities.Trim(this.HeaderText)))
                {

                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                    {
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            LiteralControl ltrl = new LiteralControl(SlkUtilities.GetHtmlEncodedText(this.HeaderText));
                            ltrl.RenderControl(htmlTextWriter);
                        }
                    }
                }

                //for each item in  the collection of SlkCheckBoxItem objects render
                //Checkbox and Associated label Control.
                foreach (SlkCheckBoxItem item in this.Items)
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                    {
                        CheckBox checkBox = ControlToRepeat;
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "1%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign,"top");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            checkBox.ID = item.Value;
                            checkBox.ToolTip = item.ToolTip;
                            checkBox.Checked = item.Selected;
                            
                            if (this.HasAttributes)
                            {
                                foreach (string text1 in this.Attributes.Keys)
                                {
                                    checkBox.Attributes[text1] = this.Attributes[text1];
                                }
                            }

                            checkBox.RenderControl(htmlTextWriter);                        
        
                        }

                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                        htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "border-top: none; padding-top:2px");
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                        {
                            Label label = ControlToAssociate;
                            label.ToolTip = item.ToolTip;
                            label.Text = item.Text;
                            label.AssociatedControlID = item.Value;
                            label.RenderControl(htmlTextWriter);

                        }

                    }
                }                 
            }
        }
        #endregion

        #region PerformDataBinding
        /// <summary>
        /// Performs Data Binding operation for ControlToRepeat
        /// </summary>
        /// <param name="data">Datasource Collection to be binded</param>
        protected override void PerformDataBinding(IEnumerable data)
        {

            base.PerformDataBinding(data);

            string textField = DataTextField;
            string valueField = DataValueField;
            string stateField = DataStateField;
            string toolTipField = DataToolTipField;

            if (data!= null)
            {
                foreach (object o in data)
                {
                    SlkCheckBoxItem item
                             = new SlkCheckBoxItem();
                    item.Text = DataBinder.GetPropertyValue(o, textField, null);
                    item.Value = DataBinder.GetPropertyValue(o, valueField, null);
                    item.Selected  = bool.Parse(DataBinder.GetPropertyValue(o, stateField, null));
                    item.ToolTip = DataBinder.GetPropertyValue(o, toolTipField, null);

                    Items.Add(item);
                }
            }
        }
        #endregion

        #region SaveViewState
        /// <summary>
        /// SaveViewState
        /// </summary>
        /// <returns>savedObject</returns>
        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            object itemState = Items.SaveViewState();
            if ((baseState == null) && (itemState == null))
                return null;
            return new Pair(baseState, itemState);
        }
        #endregion       

        #region OnUnload
        /// <summary>
        /// OnUnload
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnUnload(EventArgs e)
        {
            if (Page != null)
            {
                Page.Validators.Remove(this);
            }
            base.OnUnload(e);
        }
        #endregion


        #endregion      
        
        #region IPostBackDataHandler Members
        
        #region LoadPostData
        /// <summary>
        /// LoadPostData
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns>bool</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            if (base.IsEnabled)
            {
                if (!String.IsNullOrEmpty(postDataKey))
                {
                    if (postDataKey.Contains(this.UniqueID))
                    {
                        if (postCollection != null)
                        {
                            foreach (string key in postCollection.Keys)
                            {
                                if (key.Contains(this.UniqueID))
                                {
                                    // the checkbox's UniqueID property is something like:
                                    //     ctl00_PlaceHolderMain_chkListLearners_20
                                    // this code parses out the "20"
                                    string text1 = key.Substring(this.UniqueID.Length + 1);

                                    int num1 = int.Parse(text1, CultureInfo.InvariantCulture);
                                    this.EnsureDataBound();

                                    SlkCheckBoxItem item
                                        = this.Items.FindByValue(num1.ToString(CultureInfo.InvariantCulture));

                                    bool flag = postCollection[key] != null;

                                    if (item.Selected != flag)
                                    {
                                        item.Selected = flag;
                                    }
                                }

                            }
                        }
                    }
                }            
            }
            return false;
        }
        #endregion

        #region RaisePostDataChangedEvent
        /// <summary>
        /// Implement this if the ControlToRepeat needs 
        /// PostBack Processing
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            //this.ControlToTest.RaisePostDataChangedEvent();
        }
        #endregion

        #endregion

        #region IValidator Members

        /// <summary>
        /// Holds the plain text (not HTML) of the error message to display at the top of
        /// the checkbox list.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                object o = ViewState["ErrorMessage"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set
            {
                m_errorMessage = value;
                ViewState["ErrorMessage"] = m_errorMessage;
            }
        }

        /// <summary>
        /// <c>true</c> if control validation succeeded on all child controls.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_valid;
            }
            set
            {
                m_valid = value;
            }
        }

        /// <summary>
        /// Performs validation of child controls; initializes <c>IsValid</c>.
        /// </summary>
        public void Validate()
        {
            this.IsValid = true;

            if (this.IsRequired)
            {
                if (this.SelectedIndex == -1)
                {
                    this.IsValid = false;
                }
            }

        }


        #endregion
    }
    /// <summary>
    /// Represents a data item in a data-bound CheckBox list control. 
    /// This class cannot be inherited.  
    /// </summary>
    internal sealed class SlkCheckBoxItem
    {

        #region Private Fields
        /// <summary>
        /// Private variables
        /// </summary>
        private string m_checkBoxText;
        private bool m_checkBoxState;
        private string m_checkBoxValue;
        private string m_checkBoxToolTip;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>

        public SlkCheckBoxItem()
        {

        }

        public SlkCheckBoxItem(string checkBoxText,
                           string checkBoxValue,
                           bool isChecked,
                           string checkBoxToolTipText)
        {
            // Initialize Object
            m_checkBoxText = checkBoxText;
            m_checkBoxState = isChecked;
            m_checkBoxValue = checkBoxValue;
            m_checkBoxToolTip = checkBoxToolTipText;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether the item is selected. 
        /// </summary> 
        public bool Selected
        {
            get { return m_checkBoxState; }
            set { m_checkBoxState = value; }

        }
        /// <summary>
        /// Gets or sets the value associated with the SlkCheckBoxItem
        /// </summary>
        public string Value
        {
            get { return m_checkBoxValue; }
            set { m_checkBoxValue = value; }
        }
        /// <summary>
        /// Gets or sets the text displayed in a CustomCheckBoxList 
        /// control for the item represented by the SlkCheckBoxItem
        /// </summary>
        public string Text
        {
            get { return m_checkBoxText; }
            set { m_checkBoxText = value; }
        }
        /// <summary>
        /// Gets or sets the tooptip text displayed in a CustomCheckBoxList 
        /// control for the item represented by the SlkCheckBoxItem
        /// </summary>
        public string ToolTip
        {
            get { return m_checkBoxToolTip; }
            set { m_checkBoxToolTip = value; }
        }

        #endregion
    }

    /// <summary>
    /// A Collection of SlkCheckBoxItem objects in a CheckBox control. 
    /// This class cannot be inherited. 
    /// </summary>
    internal sealed class CheckBoxItemCollection : List<SlkCheckBoxItem>, IStateManager
    {
        private bool m_isTracked;

        #region Private and Public Methods
        /// <summary>
        /// Searches the collection for a SlkCheckBoxItem with a Value
        /// property that contains the specified text. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>index</returns>
        private int FindByValueInternal(string value)
        {
            int num1 = 0;
            foreach (SlkCheckBoxItem item1 in this)
            {
                if (item1.Value.Equals(value))
                {
                    return num1;
                }
                num1++;
            }
            return -1;
        }
        /// <summary>
        /// Searches the collection for a SlkCheckBoxItem with a Value
        /// property that contains the specified value. 
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>SlkCheckBoxItem</returns>
        internal SlkCheckBoxItem FindByValue(string value)
        {
            int num1 = this.FindByValueInternal(value);
            if (num1 != -1)
            {
                return (SlkCheckBoxItem)this[num1];
            }
            return null;
        }
        #endregion
        
        #region IStateManager Members
        /// <summary>
        /// TrackingViewState
        /// </summary>
        public bool IsTrackingViewState
        {
            get { return m_isTracked; }
        }

        #region LoadViewState
        /// <summary>
        /// LoadsViewState
        /// </summary>
        /// <param name="state">savedState</param>
        public void LoadViewState(object state)
        {
            if (state != null)
            {
                Triplet obj = (Triplet)state;
                Clear();
                string[] rgText = (string[])obj.First;
                string[] rgValue = (string[])obj.Second;
                string[] rgToolTip = (string[])obj.Third;

                for (int i = 0; i < rgText.Length; i++)
                {
                    Add(new SlkCheckBoxItem(rgText[i], rgValue[i], false, rgToolTip[i]));
                }
            }

        }
        #endregion

        #region SaveViewState
        /// <summary>
        /// SaveViewState
        /// </summary>
        /// <returns>savedObject</returns>
        public object SaveViewState()
        {
            int numOfItems = Count;


            object[] rgText = new string[numOfItems];
            object[] rgValue = new string[numOfItems];
            object[] rgToolTip = new string[numOfItems];

            for (int i = 0; i < numOfItems; i++)
            {
                rgText[i] = this[i].Text;
                rgValue[i] = this[i].Value;
                rgToolTip[i] = this[i].ToolTip;
            }

            return new Triplet(rgText, rgValue, rgToolTip);
        }
        #endregion

        #region TrackViewState
        /// <summary>
        /// TrackViewState
        /// </summary>
        public void TrackViewState()
        {
            m_isTracked = true;
        }
        #endregion

        #endregion
    }
}
