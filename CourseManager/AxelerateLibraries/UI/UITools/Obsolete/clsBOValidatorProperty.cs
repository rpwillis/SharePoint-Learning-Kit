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
using Axelerate.BusinessLayerFrameWork.BLCore.Validation;

namespace BusinessLayerUITools
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ToolboxData("<{0}:clsBOValidatorProperty runat=server />")]
    [Serializable]
    public class clsBOValidatorProperty : Control
    {
        #region Member variable
        private bool m_IsValid = true;
        private List<ValidationError> m_Errors = new List<ValidationError>();
        private bool m_Validate = false;
        private string m_ErrorMessageTitle = string.Empty;
        private string m_Target = string.Empty;
        private clsBusinessObjectErrorValidator m_Control;

        #endregion

        #region Constructor
        public clsBOValidatorProperty()
        {
        }
        #endregion

        public new bool DesignMode = (HttpContext.Current == null);
        /// <summary>
        /// accessor to the hosting validator control
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal clsBusinessObjectErrorValidator HostingControl
        {
            get { return m_Control; }
            set
            {
                m_Control = value;
            }
        }

        public void BindTo(Control ctl)
        {
            if (ctl != null)
            {
                if (ctl is ObjectDataSource)
                {
                    (ctl as ObjectDataSource).Inserting += new ObjectDataSourceMethodEventHandler(this.odsBusinessObject_Inserting);
                    (ctl as ObjectDataSource).Updating += new ObjectDataSourceMethodEventHandler(this.odsBusinessObject_Updating);
                }
            }
        }

        internal bool IsValid
        {
            get { return m_IsValid; }
            set { m_IsValid = value; }
        }

        internal List<ValidationError> Errors
        {
            get { return m_Errors; }
        }

        /// <summary>
        /// designates text control for which this
        /// validation will be performed
        /// </summary>
        [NotifyParentProperty(true)]
        [Description("The ID of the control to that is bound."), DefaultValue("")]
        [TypeConverter(typeof(ControlIDConverter))]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Target
        {
            set
            {
                m_Target = value;
            }
            get
            {
                return m_Target;
            }
        }


        /// <summary>
        /// Say if the ObjectDatasource must be validated
        /// </summary>
        public bool Validate
        {
            get
            {
                return m_Validate;
            }
            set
            {
                m_Validate = value;
                NotifyDesigner();
            }
        }

        [Description("Optional Title of the table that will be displayed in case this BusinessObject is invalid")]
        public string ErrorMessageTitle
        {
            get
            {
                return m_ErrorMessageTitle;
            }
            set
            {
                m_ErrorMessageTitle = value;
                NotifyDesigner();
            }
        }

        internal void odsBusinessObject_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            BLBusinessBase BusinessObject = (e.InputParameters[0] as BLBusinessBase);

            if (BusinessObject.IsValid)
            {
                //e.Cancel = false;
                IsValid = true;
                Errors.Clear();

            }
            else
            {
                e.Cancel = true;
                IsValid = false;
                foreach (ValidationError BR in BusinessObject.ValidationErrors)
                {
                    Errors.Add(BR);
                }
            }

        }

        internal void odsBusinessObject_Updating(object sender, System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs e)
        {
            BLBusinessBase BusinessObject = (e.InputParameters[0] as BLBusinessBase);

            if (BusinessObject.IsValid)
            {
                e.Cancel = false;
                IsValid = true;
                Errors.Clear();

            }
            else
            {
                e.Cancel = true;
                IsValid = false;
                foreach (ValidationError BR in BusinessObject.ValidationErrors)
                {
                    Errors.Add(BR);
                }
            }
        }

        #region Methods
        /// <summary>
        /// notifies hosting control that a property
        /// has been changed
        /// </summary>
        private void NotifyDesigner()
        {
            if (m_Control != null)
            {
                m_Control.NotifyDesigner();
            }
        }

        public override string ToString()
        {
            return string.Format("validator for {0}[{1}]", Target, ErrorMessageTitle);
        }
        #endregion
    }
}
