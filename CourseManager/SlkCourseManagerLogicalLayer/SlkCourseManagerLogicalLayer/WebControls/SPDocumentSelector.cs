using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axelerate.BusinessLayerUITools.Interfaces;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Axelerate.SlkCourseManagerLogicalLayer.WebControls
{
    /// <summary>
    /// Document Selector Class
    /// </summary>
    public class SPDocumentSelector : WebControl, ICustomValueEditor
    {

        #region Business Object Data
        /// <summary>
        /// Datasource
        /// </summary>
        private object m_Datasource = null;
        //private Axelerate.BusinessLayerUITools.WebControls.ImageButton imgbtn = null;
        /// <summary>
        /// Popup button
        /// </summary>
        private Button btnPopup = null;
        /// <summary>
        /// Name
        /// </summary>
        private Label m_FName = new Label();
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public SPDocumentSelector() : base("div")
        {
            
        }

        /// <summary>
        /// Adds an Attribute to the component's rendering process.
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(System.Web.UI.HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            
            if (DataSource != null)
            {
                string fileName = Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileName);
                if (DataValueFormat != null && DataValueFormat != "")
                {
                    fileName = fileName.Substring(0, System.Convert.ToInt32(DataValueFormat));
                    fileName = fileName + "...";
                }
                writer.AddAttribute("value", ((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileURL);
                writer.AddAttribute("displayvalue", "<table border='0' cellpadding='0' cellspacing='0' width='100%'><tr><td style='width:auto' ><span title='" + Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileURL) + "'>" + fileName + "</span></td><td style='width:16px' ><input type='button' value='...' title='Select Document' disabled='disabled' /></td></tr></table>");
            }
            else
            {
                writer.AddAttribute("value", "");
                writer.AddAttribute("displayvalue", "");
            }
            writer.AddAttribute("dvcontainerid", m_FName.ClientID);
        }

        /// <summary>
        /// Override to the CreateChildControls method.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Table tb = new Table();
            tb.Width = new Unit(100, UnitType.Percentage);
            tb.CellPadding = 0;
            tb.CellSpacing = 0;
            tb.BorderStyle = BorderStyle.None;

            TableRow tr = new TableRow();
            

            TableCell cellFileName = new TableCell();
            cellFileName.Style["width"] = "auto";
            
            if (m_Datasource != null)
            {
                m_FName.ID = "_label";
                m_FName.Text = Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileName);
                if (DataValueFormat != null && DataValueFormat != "")
                {
                    m_FName.Text = m_FName.Text.Substring(0, System.Convert.ToInt32(DataValueFormat));
                    m_FName.Text = m_FName.Text + "...";
                }
                m_FName.ToolTip = Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileURL);
            }
            cellFileName.Controls.Add(m_FName);

            TableCell cellbutton = new TableCell();
            cellbutton.Style["width"] = "16px";

            btnPopup = new Button();
            btnPopup.Text = "...";
            btnPopup.ToolTip = "Select Document";
            cellbutton.Controls.Add(btnPopup);

            tr.Cells.Add(cellFileName);
            tr.Cells.Add(cellbutton);
            tb.Rows.Add(tr);

            Controls.Add(tb);

        }

        /// <summary>
        /// Override to the DataBind Method.
        /// </summary>
        public override void DataBind()
        {
            base.DataBind();
            if (m_Datasource != null && m_FName != null)
            {
                m_FName.Text = Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileName);
                if (DataValueFormat != null && DataValueFormat != "")
                {
                    m_FName.Text = m_FName.Text.Substring(0, System.Convert.ToInt32(DataValueFormat));
                    m_FName.Text = m_FName.Text + "...";
                }
                m_FName.ToolTip = Page.Server.UrlDecode(((SlkCourseManagerLogicalLayer.clsActivity)DataSource).FileURL);
            }
        }

        /// <summary>
        /// Override to the OnPreRender method.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (Page != null)
            {
                string scriptLocation = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.SlkCourseManagerLogicalLayer.WebControls.SPDocumentSelector.js");
                ScriptManager.RegisterClientScriptInclude(this, this.GetType(), "SPDocumentSelector", scriptLocation);
                string Initscript = @"<script type='text/javascript' language='javascript'>SPDocumentSelector_Init('" + this.ClientID + @"');</script>";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SPDocumentSelector_" + ClientID, Initscript, false);
            }
            //m_FName.ID = this.ClientID + "_label";
            //imgbtn.OnClientClick = "return SPDocumentSelector_Show('" + this.ClientID + "', 'SPPopupDivID')";
            btnPopup.OnClientClick = "return SPDocumentSelector_Show('" + this.ClientID + "', 'SPPopupDivID');";
        }

        #region ICustomValueEditor Members

        /// <summary>
        /// Gets or Sets the DataSource
        /// </summary>
        public object DataSource
        {
            get
            {
                return m_Datasource;
            }
            set
            {
                m_Datasource = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Data Value Field.
        /// </summary>
        public string DataValueField
        {
            get
            {

                return (ViewState["SPDocSelDataValueField"] == null) ? string.Empty : ViewState["SPDocSelDataValueField"].ToString();
            }
            set
            {
                ViewState["SPDocSelDataValueField"] = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Data Value Format
        /// </summary>
        public string DataValueFormat
        {
            get
            {
                return (ViewState["SPDocSelDataValueFormat"] == null) ? string.Empty : ViewState["SPDocSelDataValueFormat"].ToString();
            }
            set
            {
                ViewState["SPDocSelDataValueFormat"] = value;
            }
        }

        /// <summary>
        /// Gets or Sets if the Component is Read Only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                if (ViewState["SPDocIsReadOnly"] == null)
                {
                    return false;
                }
                else
                {
                    return System.Convert.ToBoolean(ViewState["SPDocIsReadOnly"]);
                }
            }
            set
            {
                ViewState["SPDocIsReadOnly"] = value;
            }
        }

        /// <summary>
        /// Gets or Sets the selected document value.
        /// </summary>
        public object Value
        {
            get
            {
                if (ViewState["SpDocValue"] == null)
                {
                    return null;
                }
                else
                {
                    return ViewState["SpDocValue"];
                }
            }
            set
            {
                ViewState["SpDocValue"] = value;
            }
        }

        #endregion
    }
}
