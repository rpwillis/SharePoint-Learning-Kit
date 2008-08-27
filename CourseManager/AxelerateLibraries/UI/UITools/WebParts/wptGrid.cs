using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using Axelerate.BusinessLayerUITools;

using Axelerate.BusinessLayerFrameWork.BLCore;
using System.Xml;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties;
using System.Drawing;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptGrid : clsBaseGrid, ISelectorProvider, ISearchConsumer
    {
        private DropDownList dropdown;
        private DropDownList bottomDropdown;
        private CheckBox upperCheckBox;
        private CheckBox bottomCheckBox;
        private Panel bottomlayoutPanel;
        private Panel layoutPanel;
        private GridView mainGrid = new GridView();
        private IBLListBase m_SelectedItems = null;
        private IBLListBase m_NotSelectedItems = null;
        private ISelectorProvider m_Provider;
        private ArrayList CommandsToExecuteOnProvider = new ArrayList();
        public BLCriteria InCriteria
        {
            get
            {
                return (BLCriteria)ViewState["InCriteria"];
            }
            set
            {
                ViewState["InCriteria"] = value;
            }
        }

        private string param
        {
            get
            {
                if (ViewState["param"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["param"].ToString();
                }
            }
            set
            {
                ViewState["param"] = value;
            }
        }

        private string column
        {
            get
            {
                if (ViewState["column"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["column"].ToString();
                }
            }
            set
            {
                ViewState["column"] = value;
            }
        }

        public bool RemovableItems
        {
            get
            {
                if (ViewState["RemovableItems"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["RemovableItems"]);
                }
            }
            set
            {
                ViewState["RemovableItems"] = value;
            }
        }

        public TableCellCollection GridHeader
        {
            get
            {
                if ((mainGrid != null) && (mainGrid.HeaderRow != null))
                {
                    return mainGrid.HeaderRow.Cells;
                }
                else
                {
                    return null;
                }
            }
        }

        private ArrayList SelectedDatakeys
        {
            get
            {
                if (ViewState["SelectedDatakeys"] == null)
                {
                    ViewState["SelectedDatakeys"] = new ArrayList();
                }
                return (ArrayList)ViewState["SelectedDatakeys"];
            }
            set
            {
                ViewState["SelectedDatakeys"] = value;
            }
        }

        private Hashtable hash
        {
            get
            {
                if (ViewState["Hash"] == null)
                {
                    ViewState["Hash"] = new Hashtable();
                }
                return (Hashtable)ViewState["Hash"];
            }
        }

        private String SelectType
        {
            get
            {
                if (ViewState["SelectType"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["SelectType"].ToString();
                }
            }
            set
            {
                ViewState["SelectType"] = value;
            }
        }

        public wptGrid(IBLListBase datasorce)
        {
            m_Datasource = datasorce;
        }

        public wptGrid(BLCriteria crit)
        {
            InCriteria = crit;
            m_Datasource = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { InCriteria });
        }

        public wptGrid()
        {
        }

        private bool enableCommands = false;
        internal override bool EnabledCommands
        {
            get
            {
                if (this.mainGrid != null)
                {
                    enableCommands = (mainGrid.SelectedIndex != -1);
                }
                if (SelectedItems != null)
                {
                    enableCommands = (SelectedItems.Count > 0);
                }
                return enableCommands;
            }
            set
            {
                enableCommands = value;
            }
        }

        protected void mainGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (SelectType.ToUpper() == "MULTIPLE")
            {
                foreach (GridViewRow row in mainGrid.Rows)
                {
                    Panel pan = (Panel)row.Cells[0].Controls[0];
                    IBLListBase source = (IBLListBase)m_Datasource;
                    String key = ((BLBusinessBase)source[row.DataItemIndex]).UniqueString;
                    hash[key] = ((CheckBox)pan.Controls[0]).Checked;
                }
            }
            ((GridView)sender).PageIndex = e.NewPageIndex;
            //((ObjectDataSource)m_Datasource).sl= m_Datasource;
            hash["PageIndex"] = e.NewPageIndex;
            ((GridView)sender).DataBind();
        }

        protected void mainGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            //string Sort = e.SortExpression + " " + ConvertSortDirectionToSql(e.SortDirection);
            //((GridView)sender).DataSource = m_Datasource;
            ((GridView)sender).DataBind();

        }



        /*
                private void createInfoGroup (XmlNode bandNode, Table table)
                {
                    TableRow row = new TableRow();
                    TableCell mainContainer = new TableCell();
                    table.Rows.Add(row);
                    row.Cells.Add(mainContainer);

                    if (bandNode.Attributes["CssClass"] != null)
                    {
                        mainContainer.CssClass = (bandNode.Attributes["CssClass"]).Value;
                    }
                    if (bandNode.Attributes["Style"] != null)
                    {
                        mainContainer.Style.Value = (bandNode.Attributes["Style"]).Value;
                    }
                    if (bandNode.Attributes["Height"] != null)
                    {
                        mainContainer.Height = new Unit((bandNode.Attributes["Height"]).Value);
                    }
                    if (bandNode.Attributes["Width"] != null)
                    {
                        mainContainer.Width = new Unit((bandNode.Attributes["Width"]).Value);
                    }
                    if (bandNode.Attributes["Halign"] != null)
                    {
                        switch (bandNode.Attributes["Halign"].Value.ToUpper())
                        {
                            case "CENTER":
                                mainContainer.HorizontalAlign = HorizontalAlign.Center;
                                break;
                            case "JUSTIFY":
                                mainContainer.HorizontalAlign = HorizontalAlign.Justify;
                                break;
                            case "LEFT":
                                mainContainer.HorizontalAlign = HorizontalAlign.Left;
                                break;
                            case "RIGHT":
                                mainContainer.HorizontalAlign = HorizontalAlign.Right;
                                break;
                        }
                    }
                    if (bandNode.Attributes["Valign"] != null)
                    {
                        switch (bandNode.Attributes["Valign"].Value.ToUpper())
                        {
                            case "MIDDLE":
                                mainContainer.VerticalAlign = VerticalAlign.Middle;
                                break;
                            case "BOTTOM":
                                mainContainer.VerticalAlign = VerticalAlign.Bottom;
                                break;
                            case "TOP":
                                mainContainer.VerticalAlign = VerticalAlign.Top;
                                break;
                        }
                    }
                    foreach (System.Xml.XmlNode gNode in bandNode)
                    {
                        switch (gNode.Name.ToUpper())
                        {
                            case "GROUP":
                                CreateCommands(gNode, mainContainer);
                                break;
                        }
                    }
                }*/

        protected override void CreateGrid(XmlNode bandNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostback)
        {
            if (dropdown == null)
            {
                dropdown = new DropDownList();
                bottomDropdown = new DropDownList();
                //dropdown.SelectedIndexChanged 
            }
            if (upperCheckBox == null)
            {
                upperCheckBox = new CheckBox();
                bottomCheckBox = new CheckBox();
            }
            if (hash.ContainsKey("state"))
            {
                upperCheckBox.Checked = (bool)hash["state"];
                bottomCheckBox.Checked = (bool)hash["state"];
            }
            bottomlayoutPanel = new Panel();
            layoutPanel = new Panel();
            Panel pan = new Panel();
            pan.Width = new Unit(100, UnitType.Percentage);
            pan.Height = new Unit(100, UnitType.Percentage);
            Panel upCheckPanel = new Panel();
            Panel bottomCheckPanel = new Panel();
            mainGrid = new GridView();
            mainGrid.ID = this.ID;
            mainGrid.Width = new Unit(100, UnitType.Percentage);
            if (hash.ContainsKey("SelectedItem"))
            {
                mainGrid.SelectedIndex = (int)hash["SelectedItem"];
            }

            mainGrid.AutoGenerateColumns = false;
            upperCheckBox.Visible = false;
            upperCheckBox.ID = "UpperCheck";
            bottomCheckBox.Visible = false;
            bottomCheckBox.ID = "BottomCheck";
            upCheckPanel.Controls.Add(upperCheckBox);
            bottomCheckBox.CheckedChanged += new EventHandler(bottomCheckBox_CheckedChanged);
            upperCheckBox.CheckedChanged += new EventHandler(upperCheckBox_CheckedChanged);

            bottomCheckPanel.Controls.Add(bottomCheckBox);

            if (bandNode.Attributes["AllowPaging"] != null)
            {
                mainGrid.AllowPaging = Convert.ToBoolean((bandNode.Attributes["AllowPaging"]).Value);
                mainGrid.PagerSettings.Mode = PagerButtons.Numeric;
                mainGrid.PagerSettings.Visible = true;
                if (bandNode.Attributes["PageSize"] != null)
                {
                    mainGrid.PageSize = Convert.ToInt32((bandNode.Attributes["PageSize"]).Value);
                }
                if (hash["PageIndex"] != null)
                {
                    mainGrid.PageIndex = (int)hash["PageIndex"];
                }

                mainGrid.PageIndexChanging += new GridViewPageEventHandler(mainGrid_PageIndexChanging);
            }
            mainGrid.Sorting += new GridViewSortEventHandler(mainGrid_Sorting);

            if (bandNode.Attributes["CssClass"] != null)
            {
                mainGrid.HeaderStyle.CssClass = (bandNode.Attributes["CssClass"]).Value;
            }
            if (bandNode.Attributes["Style"] != null)
            {
                mainGrid.Style.Value = (bandNode.Attributes["Style"]).Value;
                if (mainGrid.Style["height"] != null)
                {
                    pan.Height = new Unit(mainGrid.Style["height"]);
                }
                if (mainGrid.Style["width"] != null)
                {
                    pan.Width = new Unit(mainGrid.Style["width"]);
                }
            }
            if (bandNode.Attributes["HeaderVisible"] != null)
            {
                mainGrid.ShowHeader = Convert.ToBoolean((bandNode.Attributes["HeaderVisible"]).Value);
            }
            if (bandNode.Attributes["CellPadding"] != null)
            {
                mainGrid.CellPadding = Convert.ToInt32((bandNode.Attributes["CellPadding"]).Value);
            }
            if (bandNode.Attributes["CellSpacing"] != null)
            {
                mainGrid.CellSpacing = Convert.ToInt32((bandNode.Attributes["CellSpacing"]).Value);
            }
            if (bandNode.Attributes["RowHeight"] != null)
            {
                mainGrid.RowStyle.Height = new Unit((bandNode.Attributes["RowHeight"]).Value);
            }
            if (bandNode.Attributes["GridLines"] != null)
            {
                switch (bandNode.Attributes["GridLines"].Value.ToLower())
                {
                    case "none":
                        mainGrid.GridLines = GridLines.None;
                        break;
                    case "horizontal":
                        mainGrid.GridLines = GridLines.Horizontal;
                        break;
                    case "vertical":
                        mainGrid.GridLines = GridLines.Vertical;
                        break;
                    case "both":
                        mainGrid.GridLines = GridLines.Both;
                        break;
                    default:
                        break;
                }

            }
            CreateBand(bandNode, mainGrid, Datasource, IsPostback);
            mainGrid.PageIndexChanging += new GridViewPageEventHandler(mainGrid_PageIndexChanging);
            if (RemovableItems && (ReturnedProperty != null))
            {
                //m_Datasource = m_NotSelectedItems;
                mainGrid.DataSource = NotSelectedItems;
            }
            else
            {
                mainGrid.DataSource = m_Datasource;
            }


            if (bandNode.Attributes["RowCssClass"] != null)
            {
                mainGrid.RowStyle.CssClass = (bandNode.Attributes["RowCssClass"]).Value;
            }
            if ((bandNode.Attributes["SelectedRowCssClass"] != null) && (bandNode.Attributes["SelectedRowCssClass"].Value != ""))
            {
                mainGrid.SelectedRowStyle.CssClass = (bandNode.Attributes["SelectedRowCssClass"]).Value;
            }

            if (bandNode.Attributes["AlternateRowCssClass"] != null)
            {
                mainGrid.AlternatingRowStyle.CssClass = (bandNode.Attributes["AlternateRowCssClass"]).Value;
            }
            if (bandNode.Attributes["ScrollBars"] != null)
            {
                switch (bandNode.Attributes["ScrollBars"].Value.ToLower())
                {
                    case "auto":
                        pan.ScrollBars = ScrollBars.Auto;
                        break;
                    case "both":
                        pan.ScrollBars = ScrollBars.Both;
                        break;
                    case "horizontal":
                        pan.ScrollBars = ScrollBars.Horizontal;
                        break;
                    case "none":
                        pan.ScrollBars = ScrollBars.None;
                        break;
                    case "vertical":
                        pan.ScrollBars = ScrollBars.Vertical;
                        break;
                    default:
                        break;
                }
            }

            if (bandNode.Attributes["Height"] != null)
            {
                pan.Height = new Unit((bandNode.Attributes["Height"]).Value);
            }
            if (bandNode.Attributes["Width"] != null)
            {
                pan.Width = new Unit((bandNode.Attributes["Width"]).Value);
            }
            pan.Controls.Add(layoutPanel);
            pan.Controls.Add(upCheckPanel);
            pan.Controls.Add(mainGrid);
            pan.Controls.Add(bottomCheckPanel);
            pan.Controls.Add(bottomlayoutPanel);
            if (DisplayLayouts)
            {
                clsUILayouts layouts = getLayouts();
                dropdown.DataSource = layouts;
                bottomDropdown.DataSource = layouts;
                dropdown.DataTextField = "Name";
                bottomDropdown.DataTextField = "Name";
                dropdown.DataValueField = "Name";
                bottomDropdown.DataValueField = "Name";
                layoutPanel.Controls.Add(dropdown);
                bottomlayoutPanel.Controls.Add(bottomDropdown);


                if (DisplayButton)
                {
                    Button upperBut = new Button();
                    Button BottomBut = new Button();
                    upperBut.Click += new EventHandler(but_Click);
                    BottomBut.Click += new EventHandler(BottomBut_Click);
                    upperBut.Text = "Select";
                    BottomBut.Text = "Select";
                    layoutPanel.Controls.Add(upperBut);
                    bottomlayoutPanel.Controls.Add(BottomBut);
                }
                else
                {
                    dropdown.AutoPostBack = true;
                    bottomDropdown.AutoPostBack = true;
                    dropdown.SelectedIndexChanged += new EventHandler(dropdown_SelectedIndexChanged);
                    bottomDropdown.SelectedIndexChanged += new EventHandler(dropdown_SelectedIndexChanged);
                }
                dropdown.DataBind();
                bottomDropdown.DataBind();
                if (hash["Layout"] != null)
                {
                    dropdown.Items.FindByText(hash["Layout"].ToString()).Selected = true;
                    bottomDropdown.Items.FindByText(hash["Layout"].ToString()).Selected = true;
                }
                else
                {
                    dropdown.Items.FindByText(LayoutName).Selected = true;
                    bottomDropdown.Items.FindByText(LayoutName).Selected = true;
                }


            }
            nContainer.Controls.Add(pan);
            mainGrid.DataBind();
            /*     TableCell cell = new TableCell();
                 cell.Controls.Add((Control)new TextBox());
                 mainGrid.HeaderRow.Cells.Add(cell);*/
            if (bandNode.Attributes["RowStyle"] != null || bandNode.Attributes["AlternateRowStyle"] != null)
            {
                bool flag = true;
                foreach (GridViewRow row in mainGrid.Rows)
                {
                    if (flag)
                    {
                        if (bandNode.Attributes["RowStyle"] != null)
                        {
                            row.Style.Value = (bandNode.Attributes["RowStyle"]).Value;
                        }
                    }
                    else
                    {
                        if (bandNode.Attributes["AlternateRowStyle"] != null)
                        {
                            row.Style.Value = (bandNode.Attributes["AlternateRowStyle"]).Value;
                        }
                        else if (bandNode.Attributes["RowStyle"] != null)
                        {
                            row.Style.Value = (bandNode.Attributes["RowStyle"]).Value;
                        }
                    }
                    flag = !flag;
                }
            }

            if (bandNode.Attributes["SelectedRowStyle"] != null)
            {
                if (mainGrid.SelectedIndex >= 0 && mainGrid.Rows.Count > 0 && mainGrid.Rows.Count > mainGrid.SelectedIndex)
                {
                    mainGrid.Rows[mainGrid.SelectedIndex].Style.Value = (bandNode.Attributes["SelectedRowStyle"]).Value;
                }
            }

        }



        private clsUILayouts getLayouts()
        {
            return clsUILayouts.GetCollection(Type.GetType(ClassName));
        }

        private void CreateBand(XmlNode bandNode, GridView mainGrid, object Datasource, bool IsPostback)
        {
            if (bandNode.Attributes["SelectedType"] != null)
            {
                SelectType = bandNode.Attributes["SelectedType"].Value;
                switch (SelectType.ToUpper())
                {
                    case "NONE":
                        mainGrid.AutoGenerateSelectButton = false;
                        break;
                    case "SINGLE":
                        mainGrid.AutoGenerateSelectButton = true;
                        break;
                    case "MULTIPLE":
                        upperCheckBox.Visible = true;
                        upperCheckBox.AutoPostBack = true;

                        bottomCheckBox.Visible = true;
                        bottomCheckBox.AutoPostBack = true;
                        upperCheckBox.Text = "Select All";
                        bottomCheckBox.Text = "Select All";

                        TemplateField template = new TemplateField();
                        ParsedColumn pColumn = new ParsedColumn(this, bandNode, hash);
                        template.ItemTemplate = pColumn;
                        mainGrid.Columns.Add(template);

                        if (bandNode.Attributes["SelectRowCss"] != null)
                        {
                            template.ItemStyle.CssClass = bandNode.Attributes["SelectRowCss"].Value;
                        }
                        break;
                }
            }
            int columns = 0;
            foreach (System.Xml.XmlNode gNode in bandNode)
            {
                switch (gNode.Name.ToUpper())
                {
                    case "COLUMN":
                        CreateGridColumn(gNode, mainGrid, Datasource, IsPostback, columns);
                        columns++;
                        break;
                    case "CHILDBAND":
                        /*  UltraGridBand childBand = new UltraGridBand();
                          childBand.Key = "childBand" + mainGrid.Bands.Count.ToString();
                          parentBand.ChildBandColumn = "";
                          childBand.BaseTableName = "";
                          CreateBand(gNode, mainGrid, childBand, Datasource, IsPostback);*/
                        break;
                    case "CRITERIA":
                        CreateCriteria(gNode, Datasource, IsPostback);
                        break;
                }
            }
        }

        private void CreateCriteria(XmlNode Node, object Datasource, bool IsPostback)
        {
            BLCriteria crit = new BLCriteria(Datasource.GetType());

            foreach (XmlNode gNode in Node.ChildNodes)
            {
                switch (gNode.Name.ToUpper())
                {
                    case "BINARYEXPRESSION":
                        if (gNode.Attributes["BDColumnName"] != null && gNode.Attributes["Key"] != null && gNode.Attributes["Operator"] != null &&
                            gNode.Attributes["Value"] != null && gNode.Attributes["ConcatenationOperator"] != null)
                        {
                            string columnName = gNode.Attributes["BDColumnName"].ToString();
                            string objectName = gNode.Attributes["BDColumnName"].ToString();
                            objectName = objectName.Remove(objectName.Length - 4); //To remove the Columname postfix
                            string operatorString = gNode.Attributes["Operator"].ToString();
                            string value = gNode.Attributes["Value"].ToString();
                            BLCriteriaExpression.BLCriteriaOperator concatenationOp;
                            switch (gNode.Attributes["ConcatenationOperator"].ToString().ToUpper())
                            {
                                case "AND":
                                    concatenationOp = BLCriteriaExpression.BLCriteriaOperator.OperatorAnd;
                                    break;
                                case "OR":
                                    concatenationOp = BLCriteriaExpression.BLCriteriaOperator.OperatorOr;
                                    break;
                                default:
                                case "NONE":
                                    concatenationOp = BLCriteriaExpression.BLCriteriaOperator.OperatorNone;
                                    break;
                            }
                            crit.AddBinaryExpression(columnName, objectName, operatorString, value, concatenationOp);
                        }
                        break;
                    case "EXPRESSION":
                        break;
                    default:
                        break;
                }
            }

            /*
             *
             *<Criteria>
             *  <BinaryExpression BDColumnName="Nombre de la columna bd" Key="Filter name" Operator="Binary Operator " Value="Value to operate" ConcatenationOperator ="AND|OR|NONE" />
             *  <Expression ConcatenationOperator ="AND|OR|NONE" >
             *    <BinaryExpression BDColumnName="Nombre de la columna bd" Key="Filter name" Operator="Binary Operator " Value="Value to operate" ConcatenationOperator ="AND|OR|NONE" />
             *  </Expression>
             *  <OrderFields>
             *    <OrderBy ColumnName="Name of the column to sort the data" Assending="True|False" />
             *  </OrderFields>
             *</Criteria>
             */
        }

        private void CreateGridColumn(XmlNode gNode, GridView mainGrid, object Datasource, bool IsPostback, int columIndex)
        {
            DataControlField ugcol = (DataControlField)new TemplateField();
            if (gNode.Attributes["Content"] != null)
            {
                if (gNode.Attributes["ColumnPropertyType"] != null)
                {
                    TemplateField template = new TemplateField();
                    template.HeaderTemplate = new ParsedHeader(this, gNode, columIndex);
                    ParsedColumn pColumn = new ParsedColumn(this, gNode);
                    template.ItemTemplate = pColumn;
                    ugcol = (DataControlField)template;
                }
            }

            if (gNode.Attributes["Header"] != null)
            {
                ugcol.HeaderText = (gNode.Attributes["Header"]).Value;
            }
            if (gNode.Attributes["Width"] != null)
            {
                ugcol.ItemStyle.Width = new Unit((gNode.Attributes["Width"]).Value);
            }
            if (gNode.Attributes["Height"] != null)
            {
                ugcol.ItemStyle.Height = new Unit((gNode.Attributes["Height"]).Value);
            }
            if (gNode.Attributes["HorizontalAlign"] != null)
            {
                switch ((gNode.Attributes["HorizontalAlign"]).Value.ToUpper())
                {
                    case "CENTER":
                        ugcol.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        break;
                    case "LEFT":
                        ugcol.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                        break;
                    case "RIGHT":
                        ugcol.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                        break;
                    case "JUSTIFY":
                        ugcol.ItemStyle.HorizontalAlign = HorizontalAlign.Justify;
                        break;
                }
            }
            if (gNode.Attributes["VerticalAlign"] != null)
            {
                switch ((gNode.Attributes["VerticalAlign"]).Value.ToUpper())
                {
                    case "BOTTOM":
                        ugcol.ItemStyle.VerticalAlign = VerticalAlign.Bottom;
                        break;
                    case "MIDDLE":
                        ugcol.ItemStyle.VerticalAlign = VerticalAlign.Middle;
                        break;
                    case "TOP":
                        ugcol.ItemStyle.VerticalAlign = VerticalAlign.Top;
                        break;
                }
            }

            if (gNode.Attributes["Wrap"] != null)
            {
                ugcol.ItemStyle.Wrap = Convert.ToBoolean((gNode.Attributes["Wrap"]).Value);
            }
            if (gNode.Attributes["CssClass"] != null)
            {
                ugcol.ItemStyle.CssClass = (gNode.Attributes["CssClass"]).Value;
            }
            if (gNode.Attributes["HeaderCssClass"] != null)
            {
                ugcol.HeaderStyle.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
            }
            if (gNode.Attributes["SortExpression"] != null)
            {
                ugcol.SortExpression = (gNode.Attributes["SortExpression"]).Value;
            }
            if (gNode.Attributes["PercentWidth"] != null)
            {
                ugcol.ItemStyle.Width = new System.Web.UI.WebControls.Unit(gNode.Attributes["PercentWidth"].Value);
            }
            else if (gNode.Attributes["FixedWidth"] != null)
            {
                ugcol.ItemStyle.Width = new System.Web.UI.WebControls.Unit(gNode.Attributes["FixedWidth"].Value);
            }

            if (gNode.Attributes["IsHidden"] != null)
            {
                ugcol.Visible = !System.Convert.ToBoolean((gNode.Attributes["IsHidden"]).Value);
            }
            mainGrid.Columns.Add(ugcol);
        }

        void bottomCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            upperCheckBox.Checked = check.Checked;
            setChecks(check);

        }

        void upperCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            bottomCheckBox.Checked = check.Checked;
            setChecks(check);
        }

        private void setChecks(CheckBox check)
        {
            foreach (GridViewRow row in mainGrid.Rows)
            {
                Panel pan = (Panel)row.Cells[0].Controls[0];
                ((CheckBox)pan.Controls[0]).Checked = check.Checked;
                IBLListBase source = (IBLListBase)m_Datasource;
                String key = ((BLBusinessBase)source[row.DataItemIndex]).UniqueString;
                hash[key] = ((CheckBox)pan.Controls[0]).Checked;
            }
        }
        /*
                private BoundField CreatePropertyColumn(XmlNode gNode)
                {
                    BoundField cfield = new BoundField();
                    cfield.DataField = gNode.Attributes["Content"].Value;
                    if (gNode.Attributes["EditBehavior"] != null)
                    {
                        switch ((gNode.Attributes["EditBehavior"]).Value.ToUpper())
                        {
                            case "EDITABLEONFOCUS":
                                cfield.ReadOnly = true;
                                break;
                            case "NOEDITABLE":
                                cfield.ReadOnly = true;
                                break;
                            case "ALWAYSEDITABLE":
                                cfield.ReadOnly = false;
                                break;
                        }
                    }
                    return cfield;
                }*/

        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
        }

        protected void dropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList drop = (DropDownList)sender;
            dropdown.SelectedIndex = drop.SelectedIndex;
            bottomDropdown.SelectedIndex = drop.SelectedIndex;
            LayoutName = drop.SelectedValue;

        }

        protected void but_Click(object sender, EventArgs e)
        {
            bottomDropdown.SelectedIndex = dropdown.SelectedIndex;
            //           LayoutName = dropdown.SelectedValue;
            //           hash["Layout"] = LayoutName;
        }

        protected void BottomBut_Click(object sender, EventArgs e)
        {
            dropdown.SelectedIndex = bottomDropdown.SelectedIndex;
            //           LayoutName = bottomDropdown.SelectedValue;
            //           hash["Layout"] = LayoutName;
        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {

        }

        protected override void EnsureDatasourceObject()
        {

            if (m_Provider != null)
            {
                m_Datasource = m_Provider.SelectedItems;
            }
            else
            {
                if (m_Datasource == null)
                {
                    m_Datasource = GetBusinessClassInstance();//ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "GetCollection", new object[] { Criteria });// { base.ObjectGUID, null});                    
                }
            }
        }

        protected override BLCriteria GetCriteria()
        {
            BLCriteria Criteria = base.GetCriteria();
            System.Xml.XmlDocument XMLForm = new System.Xml.XmlDocument();
            string DisplayForm = LayoutTextXML;
            if (DisplayForm != "")
            {
                XMLForm.LoadXml(DisplayForm);
                if (XMLForm.ChildNodes.Count > 0)
                {
                    if (XMLForm.ChildNodes[0].Name.ToUpper() == "FORM")
                    {
                        System.Xml.XmlNode FormNode = XMLForm.ChildNodes[0];
                        if (FormNode.ChildNodes.Count > 0)
                        {
                            System.Xml.XmlNode LayoutNode = FormNode.ChildNodes[0];
                            if (LayoutNode.Name.ToUpper() == "COLLECTIONLAYOUT")
                            {
                                if (LayoutNode.ChildNodes.Count > 0)
                                {
                                    //System.Xml.XPath.XPathNavigator PathNavigator = rootBandNode.CreateNavigator();
                                    //PathNavigator.SelectChildren("CRITERIA", "");
                                    foreach (System.Xml.XmlNode Child in LayoutNode.ChildNodes)
                                    {
                                        if (Child.Name.ToUpper() == "CRITERIA")
                                        {
                                            Criteria.LoadFromXML(Child, new Hashtable());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            InCriteria = Criteria;
            return Criteria;
        }

        public override void SetItemFields()
        {
            if (upperCheckBox != null)
            {
                hash["state"] = upperCheckBox.Checked;
            }
            if (dropdown != null)
            {
                if (dropdown.SelectedValue.CompareTo("") != 0)
                {
                    //               if (LayoutName.CompareTo(dropdown.SelectedValue) == 0)
                    //               {
                    //LayoutName = dropdown.SelectedValue;
                    //                   DataBind();
                    //               }
                }
                hash["Layout"] = dropdown.SelectedValue;
                hash["SelectedItem"] = mainGrid.SelectedIndex;
            }

        }

        private Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout Layout = null;

        protected override string GetXMLLayout()
        {
            if (Layout == null && ClassName != "")
            {
                Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), LayoutName);
            }
            if ((Layout != null && ClassName != "") && (LayoutName != Layout.Name || Type.GetType(ClassName).FullName != Layout.ObjectType))
            {
                Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), LayoutName);
            }
            if ((Layout != null) && (Layout.LayoutXML == ""))
            {
                clsLog.Trace(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName);
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidLayoutName + " " + LayoutName);
                    return "<FORM><COLLECTIONLAYOUT><BAND></BAND></COLLECTIONLAYOUT></FORM>";
                }
            }
            else return Layout.LayoutXML;
        }



        public bool DisplayLayouts
        {
            get
            {
                if (ViewState["DisplayLayouts"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["DisplayLayouts"]);
                }
            }
            set
            {
                ViewState["DisplayLayouts"] = value;
            }
        }

        public bool DisplayButton
        {
            get
            {
                if (ViewState["DisplayButton"] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(ViewState["DisplayButton"]);
                }
            }
            set
            {
                ViewState["DisplayButton"] = value;
            }
        }

        protected override void CreateButtons(object Datasource, Control nCotainer)
        {
        }

        public override bool HasDatasource()
        {
            return (m_Datasource != null);
        }

        public override bool IsNew()
        {
            return false;
        }



        //[ConnectionProvider("Context Provider", "default")]
        //public clsBaseLayoutWP GetContextProvider()
        //{
        //    return this;
        //}

        [ConnectionProvider("Selector Provider", "GridSelector")]
        public ISelectorProvider GetSelectorProvider()
        {
            return this;
        }

        [ConnectionConsumer("Selector Consumer", "GridConsumer")]
        public void RegisterProvider(ISelectorProvider Provider)
        {
            m_Provider = Provider;
            foreach (CommandEventArgs args in CommandsToExecuteOnProvider)
            {
                ExecuteCommand(null, args, true);
            }
        }


        #region ISelectorProvider Members

        public IList SelectedItems
        {
            get
            {
                if (m_SelectedItems == null)
                {
                    InitializeItems();

                }
                return m_SelectedItems;
            }
        }

        public IList NotSelectedItems
        {
            get
            {
                if (m_NotSelectedItems == null)
                {
                    InitializeItems();
                }
                return m_NotSelectedItems;
            }
        }

        private void InitializeItems()
        {
            EnsureDatasourceObject();
            if (m_Datasource != null && ComponentBehaviorType.ToUpper() == "SELECTOR" || ComponentBehaviorType.ToUpper() == "CONSUMER")
            {
                m_NotSelectedItems = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ConsumerType)); //(IBLListBase)new ArrayList();
                m_SelectedItems = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ConsumerType));//(IBLListBase)new ArrayList();
                IBLListBase Datasource = (IBLListBase)m_Datasource;
                foreach (BLBusinessBase businessObj in Datasource)
                {
                    if (ReturnedProperty != null && ReturnedProperty != "")
                    {
                        object property = businessObj.GetType().GetProperty(ReturnedProperty).GetValue(businessObj, null);
                        if (typeof(IBLListBase).IsAssignableFrom(property.GetType()))
                        {
                            IBLListBase prop = (IBLListBase)property;
                            foreach (BLBusinessBase obj in prop)
                            {
                                // obj.MarkAsChild();
                                if (SelectedDatakeys.Contains(obj.DataKey.ToString()))
                                {
                                    m_SelectedItems.Add(obj);
                                }
                                else
                                {
                                    m_NotSelectedItems.Add(obj);
                                }
                            }
                        }
                        else
                        {
                            BLBusinessBase prop = (BLBusinessBase)property;
                            // prop.MarkAsChild();
                            if (SelectedDatakeys.Contains(prop.DataKey.ToString()))
                            {
                                m_SelectedItems.Add(prop);
                            }
                            else
                            {
                                m_NotSelectedItems.Add(prop);
                            }
                        }
                    }
                    else
                    {
                        // businessObj.MarkAsChild();
                        if (SelectedDatakeys.Contains(businessObj.DataKey.ToString()))
                        {
                            m_SelectedItems.Add(businessObj);
                        }
                        else
                        {
                            m_NotSelectedItems.Add(businessObj);
                        }
                    }
                }
            }
        }

        public string ReturnedProperty
        {
            get
            {
                if (ViewState["ReturnedProperty"] == null)
                {
                    return null;
                }
                else
                {
                    return (String)ViewState["ReturnedProperty"];
                }
            }
            set
            {
                ViewState["ReturnedProperty"] = value;
            }
        }

        public void SelectItem(string ObjectDataKey)
        {
            if (ComponentBehaviorType.ToUpper() == "SELECTOR" || ComponentBehaviorType.ToUpper() == "CONSUMER")
            {
                m_SelectedItems = null;
                m_NotSelectedItems = null;
                if (!(SelectedDatakeys.Contains(ObjectDataKey)))
                {
                    SelectedDatakeys.Add(ObjectDataKey);
                }
            }
        }

        public void DeselectItem(string ObjectDatakey)
        {
            if (ComponentBehaviorType.ToUpper() == "SELECTOR" || ComponentBehaviorType.ToUpper() == "CONSUMER")
            {
                m_SelectedItems = null;
                m_NotSelectedItems = null;
                SelectedDatakeys.Remove(ObjectDatakey);
            }
        }

        public string ConsumerType
        {
            get
            {
                if (ViewState["ConsumerType"] == null)
                {
                    return null;
                }
                else
                {
                    return (string)ViewState["ConsumerType"];
                }
            }
            set
            {
                ViewState["ConsumerType"] = value;
            }
        }
        #endregion

        public override void ExecuteCommand(object sender, CommandEventArgs e, bool IsContext)
        {
            EnsureDatasourceObject();
            String[] enteredArgs = e.CommandArgument.ToString().Split('|');
            int indexRow = Convert.ToInt32(enteredArgs[0]);
            String args = enteredArgs[1];
            IList datasource = (IList)m_Datasource;

            string[] commandNameAndTarget = e.CommandName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string CommandName = commandNameAndTarget[0];
            string TargetName = "";
            string RedirectPageTo = "";
            if (commandNameAndTarget.Length > 1)
            {
                TargetName = commandNameAndTarget[1].Trim();
            }
            if (commandNameAndTarget.Length > 2)
            {
                RedirectPageTo = commandNameAndTarget[2].Trim();
            }

            if (RemovableItems && ReturnedProperty == null)
            {
                datasource = NotSelectedItems;
            }

            if (IsContext && indexRow == -1)
            {
                if (mainGrid.SelectedIndex >= 0 && mainGrid.Rows.Count > 0)
                {
                    indexRow = mainGrid.Rows[mainGrid.SelectedIndex].DataItemIndex;
                }
            }
            else if (CommandName.ToUpper() == "SORT ASC")
            {
                if (InCriteria != null)
                {
                    InCriteria.AddOrderedField(args, true);
                    m_Datasource = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { InCriteria });
                }
            }
            else if (CommandName.ToUpper() == "SORT DESC")
            {
                if (InCriteria != null)
                {
                    InCriteria.AddOrderedField(args, false);
                    m_Datasource = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { InCriteria });
                }
            }
            if (CommandName.ToUpper() == "SELECTROW")
            {
                if (indexRow >= 0)
                {
                    for (int i = 0; i < mainGrid.Rows.Count; i++)
                    {
                        if (mainGrid.Rows[i].DataItemIndex == indexRow)
                        {
                            mainGrid.SelectedIndex = i;
                            hash["SelectedItem"] = i;
                            break;
                        }
                    }
                }
            }
            else if (CommandName.ToUpper() == "SELECT")
            {
                if (indexRow >= 0)
                {
                    BLBusinessBase businessObj = (BLBusinessBase)datasource[indexRow];
                    if (ReturnedProperty != null && ReturnedProperty != "")
                    {
                        object property = businessObj.GetType().GetProperty(ReturnedProperty).GetValue(businessObj, null);
                        if (typeof(IBLListBase).IsAssignableFrom(property.GetType()))
                        {
                            IBLListBase prop = (IBLListBase)property;
                            foreach (BLBusinessBase obj in prop)
                            {
                                SelectItem(obj.DataKey.ToString());
                            }
                        }
                        else
                        {
                            BLBusinessBase prop = (BLBusinessBase)property;
                            SelectItem(prop.DataKey.ToString());
                        }
                    }
                    else
                    {
                        SelectItem(businessObj.DataKey.ToString());
                    }
                }
            }
            if (CommandName.ToUpper() == "REMOVE")
            {
                if (datasource != null && datasource.Count > 0 && indexRow >= 0)
                {
                    DeselectItem(((BLBusinessBase)datasource[indexRow]).DataKey.ToString());
                }
            }
            else if (CommandName.ToUpper() == "REMOVEONPROVIDER")
            {
                if (m_Provider != null)
                {
                    datasource = m_Provider.SelectedItems;
                    if (datasource != null && datasource.Count > 0 && indexRow >= 0)
                    {
                        m_Provider.DeselectItem(((BLBusinessBase)datasource[indexRow]).DataKey.ToString());
                    }
                }
                else
                {
                    CommandsToExecuteOnProvider.Add(e);
                }
            }
            else if (CommandName.ToUpper() == "NAVIGATETO")
            {
                string targetURL = "";
                try
                {
                    indexRow = Convert.ToInt32(enteredArgs[0]);
                    targetURL = enteredArgs[1];
                }
                catch (Exception se)
                {
                    targetURL = enteredArgs[0];
                }
                if (IsContext && indexRow == -1)
                {
                    if (mainGrid.SelectedRow != null)
                    {
                        indexRow = mainGrid.SelectedRow.DataItemIndex;
                    }
                }
                if (m_Datasource != null && targetURL.Contains("{"))
                {
                    object source = null;
                    if (typeof(IBLListBase).IsAssignableFrom(m_Datasource.GetType()))
                    {
                        IBLListBase elementList = (IBLListBase)m_Datasource;
                        if (indexRow >= 0 && indexRow < elementList.Count)
                        {
                            source = elementList[indexRow];
                        }
                    }
                    else if (typeof(BLBusinessBase).IsAssignableFrom(m_Datasource.GetType()))
                    {
                        source = (BLBusinessBase)m_Datasource;
                    }
                    if (source != null)
                    {
                        BLBusinessBase BLDatasource = (BLBusinessBase)source;
                        targetURL = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.parseBOPropertiesString(BLDatasource, targetURL, BLDatasource.GetType().AssemblyQualifiedName, "GUID");
                    }
                }
                Page.Response.Redirect(targetURL);
            }
            else
            {
                if (IsContext)
                {
                    indexRow = mainGrid.SelectedRow.DataItemIndex;
                }
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
                    method = m_Datasource.GetType().GetMethod(CommandName);
                    source = m_Datasource;
                }
                else
                {
                    method = datasource[indexRow].GetType().GetMethod(CommandName);
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
            }
        }

        #region ISearchConsumer Members

        [ConnectionConsumer("Search Consumer", "SearchGridConsumer")]
        public void RegisterProvider(ISearchProvider Provider)
        {
            BLCriteria criteria = Provider.Criteria;
            if (criteria != null)
            {
                m_Datasource = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { criteria });
                this.DataBind();
            }
        }

        public Object Datasource
        {
            get
            {
                m_Datasource = ViewState["GridDatasource"];
                return m_Datasource;
            }
            set
            {
                ViewState["GridDatasource"] = value;
                m_Datasource = value;

            }
        }
        #endregion

       
    }
}
