using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using Axelerate.BusinessLayerUITools.Common;
using System.Web.UI;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Collections;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    public class DropDownList : CompositeDataBoundControl
    {
        private HiddenField _selectedIndex;
        private Table table = null;
        private Panel options = null;

        public DropDownList()
        {
            table = new Table();
            options = new Panel();
            _selectedIndex = new HiddenField();
        }

        #region Public Properties
        public string DataValueField
        {
            get
            {
                if (ViewState["DataValueField"] == null)
                {
                    return null;
                }
                else
                {
                    return ViewState["DataValueField"].ToString();
                }
            }
            set
            {
                ViewState["DataValueField"] = value;
            }
        }

        public string DataIconField
        {
            get
            {
                if (ViewState["DataIconField"] == null)
                {
                    return null;
                }
                else
                {
                    return ViewState["DataIconField"].ToString();
                }
            }
            set
            {
                ViewState["DataIconField"] = value;
            }
        }

        public string DataTextField
        {
            get
            {
                if (ViewState["DataTextField"] == null)
                {
                    return null;
                }
                else
                {
                    return ViewState["DataTextField"].ToString();
                }
            }
            set
            {
                ViewState["DataTextField"] = value;
            }
        }

        public string DefaultImage
        {
            get
            {
                if (ViewState["DefaultImage"] == null)
                {
                    return null;
                }
                else
                {
                    return ViewState["DefaultImage"].ToString();
                }
            }
            set
            {
                ViewState["DefaultImage"] = value;
            }
        }

        #endregion

        protected override void CreateChildControls()
        {
            Controls.Clear();
            table = new Table();
            table.CssClass = base.CssClass;
            base.Controls.Add(table);
            base.Controls.Add(options);
            base.Controls.Add(_selectedIndex);
            TableRow row = new TableRow();
            TableCell selected = new TableCell();
            TableCell arrow = new TableCell();
            table.Rows.Add(row);
            row.Cells.Add(selected);
            row.Cells.Add(arrow);
            Panel panArrow = new Panel();
            arrow.Controls.Add(panArrow);
            Table optionsTable = new Table();
            options.Controls.Add(optionsTable);
            if (DataSource != null)
            {
                foreach (object token in (IList)DataSource)
                {
                    TableRow optionsRow = new TableRow();
                    TableCell optionIcon = new TableCell();
                    TableCell optionText = new TableCell();
                    optionsTable.Rows.Add(optionsRow);
                    optionsRow.Cells.Add(optionIcon);
                    optionsRow.Cells.Add(optionText);
                    Image icon = null;
                    String text = null;
                    if (typeof(BLBusinessBase).IsAssignableFrom(token.GetType()))
                    {
                        if (DataIconField != null)
                        {
                            icon = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)token, "{" + DataIconField + "}", false);
                        }
                        else
                        {
                            icon = new Image();
                            icon.ImageUrl = DefaultImage;
                        }
                        text = clsSharedMethods.SharedMethods.parseBOPropertiesString((BLBusinessBase)token, "{" + DataTextField + "}", token.GetType().AssemblyQualifiedName, ((BLBusinessBase)token).DataKey.ToString());
                    }
                    else
                    {
                        icon = new Image();
                        icon.ImageUrl = DefaultImage;
                        text = DataBinder.Eval(token, DataTextField).ToString();
                    }
                    optionIcon.Controls.Add(icon);
                    Label lblText = new Label();
                    lblText.Text = text;
                    optionText.Controls.Add(lblText);
                }
            }
        }

        protected override int CreateChildControls(System.Collections.IEnumerable dataSource, bool dataBinding)
        {
            throw new Exception(Resources.ErrorMessages.errMethodNotImplemented);
        }
    }
}
