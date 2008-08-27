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
using System.Drawing;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation;



namespace BusinessLayerUITools
{
    /// <summary>
    /// This class provide a custom validation for businessobjects of the MIPLogicalLayer.
    /// Display the validating errors in a table.
    /// </summary>
    [ProvideProperty("Validation", typeof(Control))]
    [ParseChildren(true, "BOValidatorProperies")]
    [PersistChildren(false)]
    [ToolboxBitmap(typeof(ValidationSummary))]
    [DefaultProperty("BOValidatorProperies")]
    public class clsBusinessObjectErrorValidator : ValidationSummary, IExtenderProvider
    {
        private clsBOValidatorProperties m_BOValidatorProperies;

        public clsBusinessObjectErrorValidator()
        {
            //  this.Load += new EventHandler(clsBusinessObjectErrorValidator_Load);
            m_BOValidatorProperies = new clsBOValidatorProperties(this);
        }

        public new bool DesignMode = (HttpContext.Current == null);

        /// <summary>
        /// Reference to Container control used in databind
        /// </summary>
        private Control m_Container;

        /// <summary>
        /// 'invisible property' exposing validators
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public clsBOValidatorProperties BOValidatorProperies
        {
            get
            {
                return m_BOValidatorProperies;
            }
        }

        /// <summary>
        /// this method is used to ensure that designer is notified
        /// every time there is a change in the sub-ordinate validators
        /// </summary>
        internal void NotifyDesigner()
        {
            if (this.DesignMode)
            {
                IDesignerHost Host = this.Site.Container as IDesignerHost;
                ControlDesigner Designer = Host.GetDesigner(this) as ControlDesigner;
                PropertyDescriptor Descriptor = null;
                try
                {
                    Descriptor = TypeDescriptor.GetProperties(this)["BOValidatorProperies"];
                }
                catch
                {
                    return;
                }

                ComponentChangedEventArgs ccea = new ComponentChangedEventArgs(
                            this,
                            Descriptor,
                            null,
                            this.BOValidatorProperies);
                Designer.OnComponentChanged(this, ccea);
            }
        }

        /// <summary>
        /// helper method used to find or generate
        /// validators if no such found for a
        /// text box requesting validation
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        private clsBOValidatorProperty FindValidation(string ControlID)
        {
            foreach (clsBOValidatorProperty validation in m_BOValidatorProperies)
            {
                if (validation.Target == ControlID)
                {
                    return validation;
                }
            }

            clsBOValidatorProperty newValidation = new clsBOValidatorProperty();
            newValidation.Target = ControlID;
            newValidation.HostingControl = this;
            m_BOValidatorProperies.Add(newValidation);
            NotifyDesigner();
            return newValidation;
        }

        /// <summary>
        /// property extender methods
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        public clsBOValidatorProperty GetValidation(Control Control)
        {
            return FindValidation(Control.ID);
        }

        /// <summary>
        /// property extender methods
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        public clsBOValidatorProperty GetValidation(string ControlID)
        {
            return FindValidation(ControlID);
        }

        public bool IsValid()
        {
            bool result = true;
            foreach (clsBOValidatorProperty P in BOValidatorProperies)
            {
                result &= P.IsValid;
            }
            return result;
        }


        /// <summary>
        /// Determines the padding inside of the error display box.
        /// </summary>
        [Description("The Cellpadding for the wrapper table that bounds the Error Display."), DefaultValue("10")]
        public string CellPadding
        {
            get
            {
                return m_CellPadding;
            }
            set
            {
                m_CellPadding = value;
            }
        }
        private string m_CellPadding = "10";

        /// <summary>
        /// Determines the padding inside of the error display box.
        /// </summary>
        [Description("The Border for the wrapper table that bounds the Error Display."), DefaultValue("0")]
        public string Border
        {
            get
            {
                return m_Border;
            }
            set
            {
                m_Border = value;
            }
        }
        private string m_Border = "0";

        /// <summary>
        /// The CSS Class used for the table and column to display the error items.
        /// </summary>
        [Description("CSS Class used for the container table"), DefaultValue("")]
        public string CssClass
        {
            get
            {
                return m_CssClass;
            }
            set
            {
                m_CssClass = value;
            }
        }
        private string m_CssClass = "";

        /// <summary>
        /// The CSS Class used for the row to display the title.
        /// </summary>
        [Description("CSS Class used for the table cell that contain the Title"), DefaultValue(""), Category("Title Appareance")]
        public string CssTitleClass
        {
            get
            {
                return m_CssTitleClass;
            }
            set
            {
                m_CssTitleClass = value;
            }
        }
        private string m_CssTitleClass = "";


        /// <summary>
        /// The CSS Class used for the text in the title if any.
        /// </summary>
        [Description("CSS Class used for the Title text line"), DefaultValue(""), Category("Title Appareance")]
        public string TitleTextCssClass
        {
            get
            {
                return m_TitleTextCssClass;
            }
            set
            {
                m_TitleTextCssClass = value;
            }
        }
        private string m_TitleTextCssClass = "";

        /// <summary>
        /// The CSS Class used for the text in the title if any.
        /// </summary>
        [Description("CSS Class used for the Error text lines"), DefaultValue(""), Category("Error Line Appareance")]
        public string ErrorLineTextCssClass
        {
            get
            {
                return m_ErrorLineTextCssClass;
            }
            set
            {
                m_ErrorLineTextCssClass = value;
            }
        }
        private string m_ErrorLineTextCssClass = "";

        /// <summary>
        /// Represent the prefix of the ID for the labels that show the Error message per property.
        /// Example: the property name of a bussines object can has a label with id "errMessageName" where
        /// "errMessage" is the prefix and "Name" is the name of the property; if this property has a error in the broken rules
        /// they show this label and set the text to the broken rule description.
        /// </summary>
        [Description("The prefix of the ID for the labels that show the error message per property."), DefaultValue("errMessage"), Category("Error Line Appareance")]
        public string MessagePerPropertyPrefix
        {
            get
            {
                return m_MessagePerPropertyPrefix;
            }
            set
            {
                m_MessagePerPropertyPrefix = value;
            }
        }
        private string m_MessagePerPropertyPrefix = "errMessage";

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //Hide the label controls used to show individual error messages by property.
            HideErrorMessagesControl(m_Container.Controls, this.m_MessagePerPropertyPrefix);
            if (!IsValid())
            {
                foreach (clsBOValidatorProperty po in this.BOValidatorProperies)
                {
                    if (!po.IsValid)
                    {
                        foreach (ValidationError br in po.Errors)
                        {
                            Control ctl = LocateInnerControl(m_Container.Controls, m_MessagePerPropertyPrefix + br.ErrorLocation);
                            if (ctl != null)
                            {
                                if (typeof(Label).IsAssignableFrom(ctl.GetType()))
                                {
                                    Label lb = (Label)ctl;
                                    if (lb.Text == "")
                                    {
                                        lb.Text = br.ErrorText;
                                    }
                                    else
                                    {
                                        lb.ToolTip = br.ErrorText;
                                    }
                                    lb.Visible = true;
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Render the error list to the page
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {

            //if any of the clsBOValidatorProperty is invalid we need to render the error list
            if (!IsValid() && this.ShowSummary)
            {

                //Add the css class and additional attributes for the wrap table
                if (CssClass != "")
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Border, this.Border.ToString());
                if (!this.Width.IsEmpty)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, this.Width.ToString());
                }
                if (!this.Height.IsEmpty)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, this.Height.ToString());
                }
                //begin render the table
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                //fisrt row has the error list of the SummaryValidator that this control inherits
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                base.Render(writer);
                writer.RenderEndTag();  // <td>
                writer.RenderEndTag();  // <tr>
                //Now for each clsBOValidatorProperty that are not valid render 2 rows 1 with the title and other with the list of errors 
                foreach (clsBOValidatorProperty po in this.BOValidatorProperies)
                {
                    if (!po.IsValid)
                    {
                        //if has a title we add a row with it
                        if (po.ErrorMessageTitle != "")
                        {
                            //Add row
                            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                            //Add attributes to the title row
                            if (CssTitleClass != "")
                            {
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssTitleClass);
                            }
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            //add attributes to the text in the span
                            if (TitleTextCssClass != "")
                            {
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.TitleTextCssClass);
                            }
                            writer.RenderBeginTag(HtmlTextWriterTag.Span);
                            writer.Write(po.ErrorMessageTitle);
                            writer.RenderEndTag();  // <Span>
                            writer.RenderEndTag();  // <td>
                            writer.RenderEndTag();  // <tr>
                        }
                        //add a row for the Error Messages
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        //For each error, I add a respective message
                        for (int i = 0; i < po.Errors.Count; i++)
                        {
                            //Add style for each error message
                            if (ErrorLineTextCssClass != "")
                            {
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.ErrorLineTextCssClass);
                            }
                            else
                            {

                                if (this.ForeColor.IsNamedColor)
                                {
                                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, this.ForeColor.Name);
                                }
                                else
                                {
                                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, this.ForeColor.ToArgb().ToString("X"));
                                }
                            }
                            if (this.DisplayMode == ValidationSummaryDisplayMode.BulletList)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Li);
                            }
                            else if (this.DisplayMode == ValidationSummaryDisplayMode.List)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Ul);
                            }
                            else
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.P);
                            }

                            writer.Write("     " + po.Errors[i].ErrorText);
                            writer.RenderEndTag();  // <Span>
                        }
                        writer.RenderEndTag();  // <td>
                        writer.RenderEndTag();  // <tr>

                    }
                }
                writer.RenderEndTag();  // <table>
            }
        }

        #region IExtenderProvider Members
        /// <summary>
        /// Determines whether a control can be extended. Basically
        /// we allow ANYTHING to be extended so all controls except
        /// the databinder itself are extendable.
        /// 
        /// Optionally the control can be set up to not act as 
        /// an extender in which case the IsExtender property 
        /// can be set to false
        /// </summary>
        /// <param name="extendee"></param>
        /// <returns></returns>
        public bool CanExtend(object extendee)
        {
            // *** Don't extend ourself <g>
            if (extendee is clsBusinessObjectErrorValidator)
                return false;

            if (extendee is System.Web.UI.WebControls.ObjectDataSource)
                return true;

            return false;
        }


        #endregion

        /// <summary>
        /// Binds the controls that are attached to this DataBinder.
        /// </summary>
        /// <returns>true if there no errors. False otherwise.</returns>
        public new bool DataBind()
        {
            return this.DataBind(this.Page);
        }


        /// <summary>
        /// Binds data of the specified controls into the specified bindingsource
        /// </summary>
        /// <param name="Container">The top level container that is bound</param>
        public bool DataBind(Control Container)
        {
            this.m_Container = Container;

            foreach (clsBOValidatorProperty po in this.BOValidatorProperies)
            {
                if (po.Validate)
                {
                    Control ctl = LocateInnerControl(Container.Controls, po.Target);
                    po.BindTo(ctl);
                }
            }
            return true;
        }

        protected void HideErrorMessagesControl(ControlCollection cnt, string PrefixID)
        {
            foreach (Control ctl in cnt)
            {
                if (ctl.ID != null && ctl.ID != "" && ctl.ID.StartsWith(PrefixID))
                {
                    ctl.Visible = false;
                }
                else
                {
                    if (ctl.HasControls())
                    {
                        HideErrorMessagesControl(ctl.Controls, PrefixID);
                    }
                }
            }
        }
        protected Control LocateInnerControl(ControlCollection cnt, string ID)
        {
            foreach (Control ctl in cnt)
            {
                if (ctl.ID == ID)
                {
                    return ctl;
                }
                else
                {
                    if (ctl.HasControls())
                    {
                        Control ctlResult = LocateInnerControl(ctl.Controls, ID);
                        if (ctlResult != null)
                        {
                            return ctlResult;
                        }
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Control designer used so we get a grey button display instead of the 
    /// default label display for the control.
    /// </summary>
    internal class clsBusinessObjectErrorValidatorDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            Control Ctl = this.CreateViewControl();

            return base.CreatePlaceHolderDesignTimeHtml("Control Extender");
        }
    }

    //inputparameters[0] es el objeto de negocios
    //Inserting del ObjectDatasource.
}
