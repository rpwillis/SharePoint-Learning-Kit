using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Collections;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Web.UI;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    public class ListBox : CompositeControl
    {
        private HiddenField _selectedIndex;
        private Table table = null;

        public ListBox()
        {
            _selectedIndex = new HiddenField();
            _selectedIndex.EnableViewState = true;
            _selectedIndex.ID = "_selectedIndex";
        }

        public ListBox(int columns)
        {
            Columns = columns;
            _selectedIndex = new HiddenField();
            _selectedIndex.EnableViewState = true;
            _selectedIndex.ID = "_selectedIndex";
        }

        #region Public Properties

        public IList DataSource
        {
            get
            {
                if (ViewState["DataSource"] == null)
                {
                    return null;
                }
                else
                {
                    return (IList)ViewState["DataSource"];
                }
            }
            set
            {
                ViewState["DataSource"] = value;
            }
        }

        public string SelectedItemCssClass
        {
            get
            {
                if (ViewState["SelectedItemCssClass"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["SelectedItemCssClass"].ToString();
                }
            }
            set
            {
                ViewState["SelectedItemCssClass"] = value;
            }
        }

        public string OnMouseOverCssClass
        {
            get
            {
                if (ViewState["OnMouseOverCssClass"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["OnMouseOverCssClass"].ToString();
                }
            }
            set
            {
                ViewState["OnMouseOverCssClass"] = value;
            }
        }

        public string InnerControlCssClass
        {
            get
            {
                if (ViewState["InnerControlCssClass"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["InnerControlCssClass"].ToString();
                }
            }
            set
            {
                ViewState["InnerControlCssClass"] = value;
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

        public object SelectedItem
        {
            get
            {
                if (SelectedIndex != -1)
                {
                    return DataSource[SelectedIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        public object SelectedValue
        {
            get
            {
                if (SelectedIndex != -1)
                {
                    return DataBinder.Eval(DataSource[SelectedIndex], DataValueField);
                }
                else
                {
                    return null;
                }
            }
        }

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

        public int SelectedIndex
        {
            get
            {
                if ((_selectedIndex == null) || (_selectedIndex.Value == ""))
                {
                    return -1;
                }
                else
                {
                    return Convert.ToInt32(_selectedIndex.Value);
                }
            }
            set
            {
                ViewState["SelectedIndex"] = value;
            }
        }

        public int Columns
        {
            get
            {
                if (ViewState["Columns"] == null)
                {
                    return 1;
                }
                else
                {
                    return Convert.ToInt32(ViewState["Columns"]);
                }
            }
            set
            {
                ViewState["Columns"] = value;
            }
        }
        #endregion

        #region Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();
            table = new Table();
            table.CssClass = base.CssClass;
            base.Controls.Add(table);
            base.Controls.Add(_selectedIndex);
            if (DataSource != null)
            {
                IList collection = (IList)DataSource;
                int ItemIndex = 0;
                int counter = 1;
                TableRow ActualRow = new TableRow();
                table.Rows.Add(ActualRow);
                TableCell ActualCell = new TableCell();
                ActualRow.Cells.Add(ActualCell);
                //setAttributes(ActualCell, ItemIndex);
                foreach (object obj in collection)
                {
                    Table objectTable = new Table();
                   // objectTable.CssClass = InnerControlCssClass;
                    ActualCell.Controls.Add(objectTable);
                    TableRow iconRow = new TableRow();
                    TableRow TextRow = new TableRow();
                    objectTable.Rows.Add(iconRow);
                    objectTable.Rows.Add(TextRow);
                    TableCell IconCell = new TableCell();
                    TableCell TextCell = new TableCell();
                    iconRow.Cells.Add(IconCell);
                    TextRow.Cells.Add(TextCell);
                    Image icon = null;
                    String text = null;
                    if (typeof(BLBusinessBase).IsAssignableFrom(obj.GetType()))
                    {
                        if (DataIconField != null)
                        {
                            icon = clsSharedMethods.SharedMethods.getImageProperty((BLBusinessBase)obj, "{" + DataIconField + "}", false);
                        }
                        else
                        {
                            icon = new Image();
                            icon.ImageUrl = DefaultImage;
                        }
                        text = clsSharedMethods.SharedMethods.parseBOPropertiesString((BLBusinessBase)obj, "{" + DataTextField + "}", obj.GetType().AssemblyQualifiedName, ((BLBusinessBase)obj).DataKey.ToString());
                    }
                    else
                    {
                        icon = new Image();
                        icon.ImageUrl = DefaultImage;
                        text = DataBinder.Eval(obj, DataTextField).ToString();
                    }
                    IconCell.Controls.Add(icon);
                    Label but = new Label();
                    but.Text = text;
                    TextCell.Controls.Add(but);
                    if (counter == Columns)
                    {
                        ActualRow = new TableRow();
                        table.Rows.Add(ActualRow);
                        counter = 1;
                    }
                    else
                    {
                        counter++;
                    }
                    ItemIndex++;
                    ActualCell = new TableCell();
                    //setAttributes(ActualCell, ItemIndex);
                    ActualRow.Cells.Add(ActualCell);
                    
                }
            }
            base.DataBind();
        }
        #endregion

        #region Private Methods
        private void setAttributes(TableCell TextCell, int index)
        {
            TextCell.Attributes.Add("onmouseover", "setColor(this, \"" + OnMouseOverCssClass + "\");");
            TextCell.Attributes.Add("onmouseout", "setColorOut(this, \"" + InnerControlCssClass + "\");");
            TextCell.Attributes.Add("onclick", "select(this, " + index + ", \"" + _selectedIndex.ClientID + "\", \"" + InnerControlCssClass +"\", \"" + SelectedItemCssClass + "\");");
        }

        protected override void OnPreRender(EventArgs e)
        {
            int index = 0;
            foreach (TableRow row in table.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    setAttributes(cell, index);
                    index++;
                }
            }
            //setJavaScript();
        }

        //This script has to to be in js and linked in the master page
 /*       private void setJavaScript()
        {
            System.Text.StringBuilder Script = new System.Text.StringBuilder();

            Script.AppendLine("<script language=\"javascript\">");
            Script.AppendLine("var selected = null;");

            Script.AppendLine("function setColorOut(elem)");
            Script.AppendLine("{");
            Script.AppendLine("if(elem == selected || elem == null)");
            Script.AppendLine("return;");
            Script.AppendLine("elem.className = \"" + InnerControlCssClass + "\";");
            Script.AppendLine("elem.style.cursor='default';");
            Script.AppendLine("}");

            Script.AppendLine("function setColor(elem)");
            Script.AppendLine("{");
            Script.AppendLine("if(elem == selected || elem == null)");
            Script.AppendLine("return;");
            Script.AppendLine("elem.className = \"" + SelectedItemCssClass + "\";");
            Script.AppendLine("elem.style.cursor='default';");
            Script.AppendLine("}");

            Script.AppendLine("function select(elem, num)");
            Script.AppendLine("{");
            Script.AppendLine("if(elem == selected || elem == null)");
            Script.AppendLine("return;");
            Script.AppendLine("else if(selected != null)");
            Script.AppendLine("{");
            Script.AppendLine("selected.className = \""+ InnerControlCssClass +"\";");
            Script.AppendLine("}");
            Script.AppendLine("setColor(elem);");
            Script.AppendLine("selected = elem;");
            Script.AppendLine("document.getElementById(\""+ _selectedIndex.ClientID +"\").value = num;");
            Script.AppendLine("}");

            Script.AppendLine("</script>");
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "Listbox", Script.ToString());
        }*/
        #endregion
    }
}
