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
using System.IO;
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
using Axelerate.BusinessLayerUITools.Misc;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using Axelerate.BusinessLayerUITools.WebControls;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation;

namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptHyperGrid : clsBaseGrid, ISelectorProvider, ISearchConsumer, IPostBackEventHandler
    {
        private System.Web.UI.WebControls.DropDownList dropdown;
        private System.Web.UI.WebControls.DropDownList bottomDropdown;
        private CheckBox upperCheckBox;
        private CheckBox bottomCheckBox;
        private Panel bottomlayoutPanel;
        private Panel layoutPanel;
        private Panel fixedScroll;
        private Table mainGrid = new Table();
        private IBLListBase m_SelectedItems = null;
        private IBLListBase m_NotSelectedItems = null;
        private ISelectorProvider m_Provider;
        private ArrayList CommandsToExecuteOnProvider = new ArrayList();
        private ArrayList Columns;
        private ArrayList Headers;
        private ArrayList FixedHeaders;
        private ArrayList FixedColums;
        private ArrayList Bands;
        private Hashtable rowCollection;
        internal List<HyperColumn> ColumnCollection;
        internal List<HyperColumn> RepeateableColumnCollection;
        private List<HyperRow> RowHeaderCollection;
        private ArrayList _RowImages;
        private ArrayList RepColumns;
        private Panel pan;
        private HiddenField TransferField;
        internal Hashtable HiddenValues;
        private HiddenField SelectedColumn;
        private HiddenField SelectedRow;
        private HyperRow HyperEditionRow;
        private List<wptHyperGrid> BandCollection;
        private Hashtable LockFieldCollection = new Hashtable();
        private Hashtable ImageLockCollection = new Hashtable();
        private ArrayList EditionRowValues;
        private ArrayList EditionRowLabels;
        private HybridDictionary _MethodErrorMessages = null;

        bool errorthrown = false;

        [Category("Miscellaneous")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Count Method")]
        [DefaultValue("")]
        [WebDisplayName("Count Method")]
        [WebDescription("Method used to get the number of elements returned by the factory method (uses the the fatory parameters as inputs).")]
        public virtual string CountMethod
        {
            get
            {
                if (ViewState["CountMethod"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["CountMethod"];
                }
            }
            set
            {
                ViewState["CountMethod"] = value;
            }
        }

        [Category("Miscellaneous")]
        [WebBrowsable(false)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LockImageURL")]
        [DefaultValue("")]
        [WebDisplayName("Lock Image URL")]
        [WebDescription("Image to use to show in the lock icon when the column is locked")]
        public String LockImageURL
        {
            get
            {
                if (ViewState["LockImageURL"] == null)
                {
                    if (Page != null)
                    {

                        ViewState["LockImageURL"] = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.LockClose.png");

                    }
                    else
                    {
                        return "";
                    }
                }
                return ViewState["LockImageURL"].ToString();
            }
            set
            {
                ViewState["LockImageURL"] = value;
            }
        }

        [Category("Miscellaneous")]
        [WebBrowsable(false)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "UnLockImageURL")]
        [DefaultValue("")]
        [WebDisplayName("Unlock Image URL")]
        [WebDescription("Image to use to show in the lock icon when the column is unlocked")]
        public String UnLockImageURL
        {
            get
            {
                if (ViewState["UnLockImageURL"] == null)
                {
                    if (Page != null)
                    {
                        ViewState["UnLockImageURL"] = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.LockOpen.png");
                    }
                    else
                    {
                        return "";
                    }
                }
                return ViewState["UnLockImageURL"].ToString();
            }
            set
            {
                ViewState["UnLockImageURL"] = value;
            }
        }

        /// <summary>
        /// If true show a lock/unlock icon on the editable columns
        /// </summary>
        [Category("Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LockOnColumns")]
        [DefaultValue("")]
        [WebDisplayName("Show Lock on Columns")]
        [WebDescription("If true show a lock/unlock icon on the editable columns")]
        public bool LockOnColumns
        {
            get
            {
                if (ViewState["LockOnColumns"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["LockOnColumns"];
                }
            }
            set
            {
                ViewState["LockOnColumns"] = value;
            }
        }

        /// <summary>
        /// Indicate the default lock state of the editable columns (the posible values are 'locked' and 'unlocked')
        /// </summary>
        [Category("Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "DefaultLock")]
        [DefaultValue("")]
        [WebDisplayName("Default Column Lock State")]
        [WebDescription("indicate the default lock state of the editable columns (the posible values are 'locked' and 'unlocked')")]
        public String DefaultLock                   //locked or unlocked
        {
            get
            {
                if (ViewState["DefaultLock"] == null)
                {
                    ViewState["DefaultLock"] = "unlocked";
                }
                return ViewState["DefaultLock"].ToString();
            }
            set
            {
                ViewState["DefaultLock"] = value;
            }
        }

        private int SelectedRowIndex
        {
            get
            {
                if (SelectedRow.Value != "")
                {
                    return Convert.ToInt32(SelectedRow.Value);
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                SelectedRow.Value = value.ToString();
            }
        }

        private int SelectedColumnIndex
        {
            get
            {
                if (SelectedColumn.Value != "")
                {
                    return Convert.ToInt32(SelectedColumn.Value);
                }
                else
                {
                    return -1;
                }
            }
        }
        private String SelectedCell
        {
            get
            {
                if (TransferField != null)
                {
                    return TransferField.Value;
                }
                else
                {
                    return "";
                }
            }
        }

        private HyperColumn SelectedHyperColumn
        {
            get
            {
                if (SelectedColumnIndex != -1)
                {
                    HyperColumn result = null;
                    foreach (HyperColumn col in ColumnCollection)
                    {
                        if (col.ColumnIndex == SelectedColumnIndex)
                        {
                            result = col;
                        }
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool SaveOnCellEditting
        {
            get
            {
                if (ViewState["SaveOnCellEditting"] == null)
                {
                    ViewState["SaveOnCellEditting"] = false;
                }
                return (bool)ViewState["SaveOnCellEditting"];
            }
            set
            {
                ViewState["SaveOnCellEditting"] = value;
            }
        }
        private ArrayList RowImages
        {
            get
            {
                if (_RowImages == null)
                {
                    _RowImages = new ArrayList();
                }
                return _RowImages;
            }
        }

        private bool EditionRow
        {
            get
            {
                if (ViewState["EditionRow"] == null)
                {
                    ViewState["EditionRow"] = false;
                }
                return (bool)ViewState["EditionRow"];
            }
            set
            {
                ViewState["EditionRow"] = value;
            }
        }

        private HybridDictionary MethodErrorMessages
        {
            get
            {
                if (_MethodErrorMessages == null)
                {
                    _MethodErrorMessages = new HybridDictionary();
                }
                return _MethodErrorMessages;
            }
        }

        public String ExpandImageUrl
        {
            get
            {
                if (ViewState["ExpandImageUrl"] == null)
                {
                    ViewState["ExpandImageUrl"] = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.ig_tblPlus.gif");
                }
                return (String)ViewState["ExpandImageUrl"];
            }
            set
            {
                ViewState["ExpandImageUrl"] = value;
            }
        }

        public String CollapseImageUrl
        {
            get
            {
                if (ViewState["CollapseImageUrl"] == null)
                {
                    ViewState["CollapseImageUrl"] = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.ig_tblMinus.gif");
                }
                return (String)ViewState["CollapseImageUrl"];
            }
            set
            {
                ViewState["CollapseImageUrl"] = value;
            }
        }

        public String HeaderText
        {
            get
            {
                if (ViewState["HeaderText"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["HeaderText"].ToString();
                }
            }
            set
            {
                ViewState["HeaderText"] = value;
            }
        }

        public String GridHeaderCssClass
        {
            get
            {
                if (ViewState["GridHeaderCssClass"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["GridHeaderCssClass"].ToString();
                }
            }
            set
            {
                ViewState["GridHeaderCssClass"] = value;
            }
        }

        public bool VisibleHeader
        {
            get
            {
                if (ViewState["VisibleHeader"] == null)
                {
                    ViewState["VisibleHeader"] = false;
                }
                return (bool)ViewState["VisibleHeader"];
            }
            set
            {
                ViewState["VisibleHeader"] = value;
            }
        }

        public bool AutoGenerateSelectButton
        {
            get
            {
                if (ViewState["AutoGenerateSelectButton"] == null)
                {
                    ViewState["AutoGenerateSelectButton"] = false;
                }
                return (bool)ViewState["AutoGenerateSelectButton"];
            }
            set
            {
                ViewState["AutoGenerateSelectButton"] = value;
            }
        }
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
                if ((mainGrid != null) && (mainGrid.Rows.Count != 0))
                {
                    return mainGrid.Rows[0].Cells;
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

        private wptHyperGrid m_Parent = null;

        public wptHyperGrid(wptHyperGrid Parent, IBLListBase datasorce)
            : this(datasorce)
        {
            m_Parent = Parent;
        }

        public wptHyperGrid(IBLListBase datasorce)
        {
            m_Datasource = datasorce;
        }

        public wptHyperGrid(BLCriteria crit)
        {
            InCriteria = crit;
            m_Datasource = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { InCriteria });
        }

        public wptHyperGrid()
        {
        }

        private bool enableCommands = false;
        internal override bool EnabledCommands
        {
            get
            {
                if (this.mainGrid != null)
                {
                    enableCommands = (this.SelectedRowIndex != -1);
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

        public WebControl EditorContainer
        {
            get
            {
                //if (m_Parent != null)
                //{
                //    return m_Parent.EditorContainer;
                //}
                //else
                //{
                return pan;
                //}
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

        protected virtual int GetCount()
        {
            object Instance = 0;
            object[] Params = GetParams();
            try
            {
                if (CountMethod.Trim() != "")
                {

                    Instance = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, CountMethod, Params);
                }
            }
            catch
            {
                Instance = 0;
            }
            return (int)Instance;
        }

        protected override void CreateGrid(XmlNode bandNode, Control nContainer, object Datasource, bool IsPostback)
        {
            Pager pager = null;
            if (bandNode.Attributes["AllowPaging"] != null)
            {
                if (Convert.ToBoolean(bandNode.Attributes["AllowPaging"].Value))
                {
                    BLCriteria Crit = GetCriteria();
                    pager = new Pager();
                    pager.ID = this.ID + "_Pager";
                    pager.PageCount = 1;
                    pager.Height = new Unit("10px");
                    pager.SelectedPageChange += new SelectedPageChangeEventHandler(pager_SelectedPageChange);
                    if (bandNode.Attributes["PageSize"] != null)
                    {
                        Crit.PageSize = Convert.ToInt32((bandNode.Attributes["PageSize"]).Value);
                        int rowcount = GetCount();
                        pager.PageCount = Convert.ToInt32(Math.Ceiling(((double)rowcount / (double)Crit.PageSize)));
                    }
                    if (hash["PageIndex"] != null)
                    {
                        Crit.PageNumber = (int)hash["PageIndex"];
                        pager.PageNumber = (int)hash["PageIndex"];
                    }
                }

            }
            if (dropdown == null)
            {
                dropdown = new System.Web.UI.WebControls.DropDownList();
                bottomDropdown = new System.Web.UI.WebControls.DropDownList();
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

            pan = new Panel();
            TextBox celleditor = new TextBox();

            pan.Controls.Add(celleditor);
            celleditor.ID = this.ID + "_input";
            celleditor.Attributes.Add("celltoedit", "");
            celleditor.Style.Add(HtmlTextWriterStyle.Display, "none");
            celleditor.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            if ((this.Page != null) && SaveOnCellEditting)
            {
                celleditor.Attributes.Add("serveronedit", "javascript:" + this.Page.ClientScript.GetPostBackEventReference(this, "oncellvaluechanged,[rdk],[cdk],[cellindex],[value]"));
            }
            else
            {
                celleditor.Attributes.Add("serveronedit", "");
            }
            bottomlayoutPanel = new Panel();

            Columns = new ArrayList();
            Headers = new ArrayList();
            FixedHeaders = new ArrayList();
            RepColumns = new ArrayList();
            fixedScroll = new Panel();
            rowCollection = new Hashtable();
            ColumnCollection = new List<HyperColumn>();
            RepeateableColumnCollection = new List<HyperColumn>();
            RowHeaderCollection = new List<HyperRow>();
            FixedColums = new ArrayList();
            Bands = new ArrayList();
            EditionRowLabels = new ArrayList();
            EditionRowValues = new ArrayList();

            layoutPanel = new Panel();
            HiddenValues = new Hashtable();
            BandCollection = new List<wptHyperGrid>();
            pan.Width = new Unit(100, UnitType.Percentage);
            pan.Height = new Unit(100, UnitType.Percentage);
            Panel upCheckPanel = new Panel();
            Panel bottomCheckPanel = new Panel();
            mainGrid = new Table();
            mainGrid.Style["table-layout"] = "fixed";

            if (!this.PlaneFieldControls.ContainsKey(this.ID + "_selectedcell"))
            {
                TransferField = new HiddenField();
                TransferField.ID = this.ID + "_selectedcell";
                //                PlaneFieldControls[this.ID + "_selectedcell"] = PlaneFieldControls;
                PlaneFieldControls[this.ID + "_selectedcell"] = TransferField;
            }
            else
            {
                TransferField = (HiddenField)this.PlaneFieldControls[this.ID + "_selectedcell"];
            }

            if (!this.PlaneFieldControls.ContainsKey(this.ID + "_selectedrow"))
            {
                SelectedRow = new HiddenField();
                SelectedRow.ID = this.ID + "_selectedrow";
            }
            else
            {
                SelectedRow = (HiddenField)this.PlaneFieldControls[this.ID + "_selectedrow"];
            }

            if (!this.PlaneFieldControls.ContainsKey(this.ID + "_selectedcolumn"))
            {
                SelectedColumn = new HiddenField();
                SelectedColumn.ID = this.ID + "_selectedcolumn";
            }
            else
            {
                SelectedColumn = (HiddenField)this.PlaneFieldControls[this.ID + "_selectedcolumn"];
            }

            pan.Controls.Add(TransferField);
            pan.Controls.Add(SelectedColumn);
            pan.Controls.Add(SelectedRow);
            mainGrid.Style.Add(HtmlTextWriterStyle.Position, "relative");
            mainGrid.CellPadding = 0;
            mainGrid.CellSpacing = 0;
            mainGrid.ID = this.ID;
            mainGrid.Width = new Unit(100, UnitType.Percentage);
            //if (hash.ContainsKey("SelectedItem"))
            //{
            //    this.SelectedIndex = (int)hash["SelectedItem"];
            //}

            upperCheckBox.Visible = false;
            upperCheckBox.ID = "UpperCheck";
            bottomCheckBox.Visible = false;
            bottomCheckBox.ID = "BottomCheck";
            upCheckPanel.Controls.Add(upperCheckBox);
            bottomCheckBox.CheckedChanged += new EventHandler(bottomCheckBox_CheckedChanged);
            upperCheckBox.CheckedChanged += new EventHandler(upperCheckBox_CheckedChanged);

            bottomCheckPanel.Controls.Add(bottomCheckBox);
            if (bandNode.Attributes["EditedRowCss"] != null)
            {
                mainGrid.Attributes.Add("editedrowcss", bandNode.Attributes["EditedRowCss"].Value);
            }
            if (bandNode.Attributes["EditionRow"] != null)
            {
                EditionRow = true;
            }
            if (bandNode.Attributes["CommandErrorMessages"] != null)
            {
                MethodErrorMessages.Clear();
                String[] commanderrors = bandNode.Attributes["CommandErrorMessages"].Value.Split(new char[] { '|' });
                for (int i = 0; i < commanderrors.Length; i++)
                {
                    String[] commandNameAndError = commanderrors[i].Split(new char[] { ':' }, 2);
                    if (commandNameAndError.Length == 2)
                    {
                        MethodErrorMessages.Add(commandNameAndError[0], commandNameAndError[1]);
                    }
                }
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
                    fixedScroll.Width = new Unit(mainGrid.Style["width"]);
                }
            }

            CreateBand(bandNode, Datasource, IsPostback);
            if (RemovableItems && (ReturnedProperty != null))
            {
                source = NotSelectedItems;
            }
            else
            {
                source = m_Datasource;
            }
            if ((bandNode.Attributes["SelectedRowCssClass"] != null) && (bandNode.Attributes["SelectedRowCssClass"].Value != ""))
            {
                mainGrid.Attributes["SelectedRowCssClass"] = bandNode.Attributes["SelectedRowCssClass"].Value;
                if (this.SelectedRowIndex != -1)
                {
                    mainGrid.Rows[SelectedRowIndex].CssClass = (bandNode.Attributes["SelectedRowCssClass"]).Value;
                }
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

                fixedScroll.Width = new Unit((bandNode.Attributes["Width"]).Value);
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
            if (FixedColums.Count != 0)
            {
                fixedScroll.Height = new Unit("16px");
                fixedScroll.ScrollBars = ScrollBars.Horizontal;
                pan.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");
                pan.Style.Add(HtmlTextWriterStyle.Position, "relative");
                nContainer.Controls.Add(fixedScroll);
            }
            GridBuilder(bandNode);

            if (pager != null)
            {
                pan.Controls.Add(pager);
            }

            if ((bandNode.Attributes["HeaderVisible"] != null) && (mainGrid.Rows.Count != 0))
            {
                mainGrid.Rows[1].Visible = Convert.ToBoolean((bandNode.Attributes["HeaderVisible"]).Value);
            }
            if ((bandNode.Attributes["CssClass"] != null) && (mainGrid.Rows.Count != 0))
            {
                mainGrid.Rows[1].CssClass = (bandNode.Attributes["CssClass"]).Value;
            }
            if ((bandNode.Attributes["HeaderStyle"] != null) && (mainGrid.Rows.Count != 0))
            {
                mainGrid.Rows[1].Style.Value = (bandNode.Attributes["HeaderStyle"]).Value;
            }
            /*     TableCell cell = new TableCell();
                 cell.Controls.Add((Control)new TextBox());
                 mainGrid.HeaderRow.Cells.Add(cell);*/


            if (bandNode.Attributes["SelectedRowStyle"] != null)
            {
                if (this.SelectedRowIndex >= 0 && mainGrid.Rows.Count > 0 && mainGrid.Rows.Count > this.SelectedRowIndex)
                {
                    mainGrid.Rows[this.SelectedRowIndex].Style.Value = (bandNode.Attributes["SelectedRowStyle"]).Value;
                }
            }
        }

        void pager_SelectedPageChange(object sender, CommandEventArgs e)
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
            ((Pager)sender).PageNumber = Convert.ToInt16(e.CommandArgument);
            //((ObjectDataSource)m_Datasource).sl= m_Datasource;
            hash["PageIndex"] = e.CommandArgument;
            BLCriteria Cri = GetCriteria();
            Cri.PageNumber = Convert.ToInt16(e.CommandArgument);
            ResetDatasource();
        }

        private void GridBuilder(XmlNode xmlgrid)
        {
            Table table = mainGrid;
            CreateCols(table, FixedColums, null, true, true, 0, 0, false);
            ColTag colt = null;
            Panel scrollWidth = null;
            double tableSize = 0;
            if (FixedColums.Count != 0)
            {
                foreach (ColumnTemplate cell in FixedColums)
                {
                    if (!cell.IsHidden)
                    {
                        tableSize += cell.Width.Value;
                    }
                }
                if ((Bands.Count != 0) || (EditionRow))
                {
                    tableSize += 50;
                }
                if (this.AutoGenerateSelectButton)
                {
                    tableSize += 50;
                }
                scrollWidth = new Panel();
                scrollWidth.Width = new Unit(tableSize);
                fixedScroll.Controls.Add(scrollWidth);
                table.Width = new Unit(tableSize);
                colt = new ColTag();
                colt.Style.Add(HtmlTextWriterStyle.Position, "relative");
                table.Rows[0].Cells.Add(colt);
            }
            Unit size = CreateHeader(table, FixedHeaders, Headers, xmlgrid);
            if (colt != null)
            {
                table.Width = new Unit(table.Width.Value + size.Value);
                colt.Width = size;
                scrollWidth.Width = table.Width;
            }
            CreateContent(table, FixedColums, Columns, size, xmlgrid);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Page != null && !this.DesignMode)
            {
                string scriptUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.WebParts.wptHyperGrid.js");
                ScriptManager.RegisterClientScriptInclude(Page, typeof(wptHyperGrid), "hypergrid", scriptUrl);
            }
            if (m_Datasource != null)
            {
                if (TransferField.Value != "")
                {
                    System.Text.StringBuilder SelectScript = new System.Text.StringBuilder();
                    SelectScript.AppendLine("<script language=\"javascript\" type=\"text/javascript\">");
                    if (SelectedRowIndex != -1)
                    {
                        SelectScript.AppendLine("setTimeout(\"axGrid_SelectRow(" + SelectedRowIndex + ",'" + mainGrid.ClientID + "')\", 0);");
                    }
                    else if (SelectedColumnIndex != -1)
                    {
                        SelectScript.AppendLine("setTimeout(\"axGrid_SelectColumn(" + SelectedColumnIndex + ",'" + mainGrid.ClientID + "')\", 0);");
                    }
                    SelectScript.AppendLine("</script>");
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "HyperGrid", SelectScript.ToString(), false);
                }

                base.OnPreRender(e);
                foreach (HyperRow hr in rowCollection.Values)
                {
                    hr.OnPrerender();
                }
                foreach (HyperRow rhr in RowHeaderCollection)
                {
                    rhr.OnPrerender();
                }

                String PanelIDs = "";
                foreach (HyperRow row in rowCollection.Values)
                {
                    PanelIDs += row.PanelClientID + ";";
                }
                foreach (HyperRow row in RowHeaderCollection)
                {
                    PanelIDs += row.PanelClientID + ";";
                }
                if (HyperEditionRow != null)
                {
                    PanelIDs += HyperEditionRow.PanelClientID + ";";
                }
                foreach (object lockimg in ImageLockCollection.Keys)
                {
                    System.Web.UI.WebControls.Image img = (System.Web.UI.WebControls.Image)ImageLockCollection[lockimg];
                    img.Attributes["onclick"] = "axGrid_LockedUnlockColumn(event, this,'" + mainGrid.ClientID + "','" + lockimg + "','" + LockImageURL + "','" + UnLockImageURL + "')";
                }

                mainGrid.Attributes["onmousedown"] = "axGrid_cellClick(event, '" + mainGrid.ClientID + "')";
                mainGrid.Attributes["onscroll"] = "axGrid_OnAutomaticScroll(event,'" + mainGrid.ClientID + "')";
                mainGrid.Attributes["onmove"] = "axGrid_OnAutomaticScroll(event,'" + mainGrid.ClientID + "')";
                pan.Attributes["onkeydown"] = "axGrid_OnKeyDown(event, '" + mainGrid.ClientID + "')";
                pan.Attributes["onscroll"] = "axGrid_SynchronizeScroll(event, this, '" + fixedScroll.ClientID + "')";
                fixedScroll.Attributes["onload"] = "SetScrollHeight(this, '" + pan.ClientID + "')";
                fixedScroll.Attributes.Add("onScroll", "OnGridScroll(this, '" + PanelIDs + "')");
            }

        }

        private void CreateContent(Table table, ArrayList fcolumns, ArrayList cols, Unit tableSize, XmlNode xmlgrid)
        {
            int index = 0;
            ICollection Dsource = (ICollection)source;
            int rowcounter = 0;
            bool flag = true;
            if (Dsource != null)
            {
                foreach (BLBusinessBase data in Dsource)
                {
                    TableRow row = new TableRow();
                    TableRow rowband = new TableRow();
                    row.ID = this.ID + "_row_" + rowcounter;
                    rowband.ID = this.ID + "_cbrow_" + rowcounter;
                    HyperRow hyperR;
                    if (Bands.Count != 0)
                    {
                        hyperR = new HyperRow(index, row, rowband);
                        TableHeaderCell header = new TableHeaderCell();
                        header.Text = "&nbsp";
                        header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        System.Web.UI.WebControls.Image ima = new System.Web.UI.WebControls.Image();
                        ima.ImageUrl = ExpandImageUrl;
                        ima.AlternateText = "Expand";
                        hyperR.expandImage = ima;
                        if (this.PlaneFieldControls.ContainsKey(this.ID + "_cbrowstatus_" + rowcounter))
                        {
                            hyperR.childbandStatus = (HiddenField)this.PlaneFieldControls[this.ID + "_cbrowstatus_" + rowcounter];
                        }
                        else
                        {
                            hyperR.childbandStatus = new HiddenField();
                            hyperR.childbandStatus.Value = "none";
                            hyperR.childbandStatus.ID = this.ID + "_cbrowstatus_" + rowcounter;
                            this.PlaneFieldControls[hyperR.childbandStatus.ID] = hyperR.childbandStatus;
                        }
                        header.Controls.Add(hyperR.childbandStatus);
                        header.Controls.Add(ima);
                        RowImages.Add(ima);
                        header.Width = new Unit("25px");
                        row.Cells.Add(header);
                        SetGridLines(xmlgrid, header);
                    }
                    else
                    {
                        hyperR = new HyperRow(index, row);
                    }
                    hyperR.DataItem = data;
                    hyperR.Parent = this;
                    rowCollection[data.DataKey.ToString()] = hyperR;
                    if (this.AutoGenerateSelectButton)
                    {
                        TableHeaderCell header = new TableHeaderCell();
                        header.ID = this.ID + "_headerrow_" + rowcounter;
                        header.Text = "&nbsp";
                        header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        header.Width = new Unit("50px");

                        Label lblSelect = new Label();
                        lblSelect.Text = Axelerate.BusinessLayerUITools.Resources.LocalizationUIToolsResource.strSelectButtonText;
                        lblSelect.ToolTip = Axelerate.BusinessLayerUITools.Resources.LocalizationUIToolsResource.strSelectButtonTooltip;
                        header.Controls.Add(lblSelect);

                        /*System.Web.UI.WebControls.Image btnSelect = new System.Web.UI.WebControls.Image();
                        if (Page != null)
                        {
                            btnSelect.ImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.select.gif");
                            btnSelect.ToolTip = Axelerate.BusinessLayerUITools.Resources.LocalizationUIToolsResource.strSelectButtonTooltip;
                        }
                        header.Controls.Add(btnSelect);*/

                        row.Cells.Add(header);
                        SetGridLines(xmlgrid, header);
                    }
                    int cellcounter = 0;
                    foreach (ColumnTemplate col in fcolumns)
                    {
                        TableCell cell = new TableCell();
                        SetAppareance(cell, col.gNode);
                        HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + cellcounter];
                        if ((hvalue != null) && (Convert.ToBoolean(hvalue.Value)) && (col.gNode.Attributes["LockedCssClass"] != null))
                        {
                            cell.CssClass = col.gNode.Attributes["LockedCssClass"].Value;
                        }
                        cell.Text = "&nbsp";
                        cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter;
                        cell.Attributes.Add("rdk", data.DataKey.ToString());
                        cell.Attributes.Add("cdk", "");
                        col.InstantiateColumn(cell, data, index);
                        row.Cells.Add(cell);
                        cellcounter++;
                        SetGridLines(xmlgrid, cell);
                    }
                    TableCell contentcell = new TableCell();
                    contentcell.HorizontalAlign = HorizontalAlign.Left;
                    contentcell.Style.Add("colspan", cols.Count.ToString());
                    Panel contentPanel = new Panel();
                    contentPanel.Width = new Unit("100%");
                    contentcell.Controls.Add(contentPanel);
                    Table contentTable = new Table();
                    contentTable.Style["table-layout"] = "fixed";
                    contentTable.CellPadding = 0;
                    contentTable.CellSpacing = 0;
                    contentPanel.Style.Add(HtmlTextWriterStyle.Overflow, "hidden");
                    contentPanel.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    contentcell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    contentTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    hyperR.scrollPanel = contentTable;
                    contentTable.Width = tableSize;
                    contentPanel.Controls.Add(contentTable);
                    CreateCols(contentTable, cols, RepColumns, false, true, rowcounter, cellcounter, EditionRow);
                    TableRow contentRow = new TableRow();
                    if (xmlgrid.Attributes["CellPadding"] != null)
                    {
                        contentTable.CellPadding = Convert.ToInt32((xmlgrid.Attributes["CellPadding"]).Value);
                        mainGrid.CellPadding = Convert.ToInt32((xmlgrid.Attributes["CellPadding"]).Value);
                    }
                    if (xmlgrid.Attributes["CellSpacing"] != null)
                    {
                        mainGrid.CellSpacing = Convert.ToInt32((xmlgrid.Attributes["CellSpacing"]).Value);
                        contentTable.CellSpacing = Convert.ToInt32((xmlgrid.Attributes["CellSpacing"]).Value);
                    }
                    if (xmlgrid.Attributes["RowHeight"] != null)
                    {
                        row.CssClass = (xmlgrid.Attributes["RowHeight"]).Value;
                        contentRow.Style.Add("Height", (xmlgrid.Attributes["RowHeight"]).Value);
                    }

                    if (flag)
                    {
                        if (xmlgrid.Attributes["RowCssClass"] != null)
                        {
                            row.CssClass = (xmlgrid.Attributes["RowCssClass"]).Value;
                            contentRow.CssClass = (xmlgrid.Attributes["RowCssClass"]).Value;
                        }
                        if (xmlgrid.Attributes["RowStyle"] != null)
                        {
                            row.CssClass = (xmlgrid.Attributes["RowStyle"]).Value;
                            contentRow.Style.Value = (xmlgrid.Attributes["RowStyle"]).Value;
                        }
                    }
                    else
                    {
                        if (xmlgrid.Attributes["AlternateRowCssClass"] != null)
                        {
                            row.CssClass = (xmlgrid.Attributes["AlternateRowCssClass"]).Value;
                            contentRow.CssClass = (xmlgrid.Attributes["AlternateRowCssClass"]).Value;
                        }
                        if (xmlgrid.Attributes["AlternateRowStyle"] != null)
                        {
                            row.Style.Value = (xmlgrid.Attributes["AlternateRowStyle"]).Value;
                            contentRow.Style.Value = (xmlgrid.Attributes["AlternateRowStyle"]).Value;
                        }
                    }
                    flag = !flag;
                    contentRow.ID = this.ID + "_nfrow_" + rowcounter;
                    contentTable.Rows.Add(contentRow);
                    foreach (ColumnTemplate col in cols)
                    {
                        TableCell cell = new TableCell();
                        SetAppareance(cell, col.gNode);
                        HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + cellcounter];
                        if ((hvalue != null) && (Convert.ToBoolean(hvalue.Value)) && (col.gNode.Attributes["LockedCssClass"] != null))
                        {
                            cell.CssClass = col.gNode.Attributes["LockedCssClass"].Value;
                        }
                        cell.Text = "&nbsp";
                        cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter;
                        cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cell.Attributes.Add("rdk", data.DataKey.ToString());
                        cell.Attributes.Add("cdk", "");
                        col.InstantiateColumn(cell, data, index);
                        contentRow.Cells.Add(cell);
                        cellcounter++;
                        SetGridLines(xmlgrid, cell);
                    }
                    CreateRepeatableColumn(contentRow, data, rowcounter, cellcounter, xmlgrid);
                    if (EditionRow)
                    {
                        TableHeaderCell header = new TableHeaderCell();
                        header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        header.Width = new Unit("25px");
                        contentRow.Cells.Add(header);
                        TableHeaderCell cancelheader = new TableHeaderCell();
                        cancelheader.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cancelheader.Width = new Unit("25px");
                        contentRow.Cells.Add(cancelheader);
                        SetGridLines(xmlgrid, header);
                        SetGridLines(xmlgrid, cancelheader);
                    }
                    row.Cells.Add(contentcell);
                    table.Rows.Add(row);
                    if (Bands.Count != 0)
                    {
                        rowband.Cells.Add(new TableCell());
                        rowband.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        TableCell cell = new TableCell();
                        cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cell.ColumnSpan = row.Cells.Count - 1;
                        rowband.Cells.Add(cell);
                        rowband.Style.Add("display", "none");
                        int zUp = Dsource.Count - rowcounter;
                        rowband.Style.Add(HtmlTextWriterStyle.ZIndex, zUp.ToString());
                        Panel container = new Panel();
                        container.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cell.Controls.Add(container);
                        if (FixedColums.Count != 0)
                        {
                            container.Width = new Unit(pan.Width.Value - 50);
                        }
                        else
                        {
                            container.Width = new Unit("100%");
                        }
                        CreateChildBand(container, data, index);
                        table.Rows.Add(rowband);
                    }
                    index++;
                    rowcounter++;
                }
            }
            if (EditionRow)
            {
                CreateEditionRow(table, FixedHeaders, Columns, rowcounter, tableSize, xmlgrid);
            }
        }

        private void SetGridLines(XmlNode xmlgrid, WebControl cell)
        {
            if (xmlgrid.Attributes["GridLines"] != null)
            {
                switch (xmlgrid.Attributes["GridLines"].Value.ToLower())
                {
                    case "none":
                        cell.BorderWidth = 0;
                        break;
                    case "horizontal":
                        cell.Style.Add("border-bottom-width", "1");
                        cell.Style.Add("border-top-width", "1");
                        break;
                    case "vertical":
                        cell.Style.Add("border-left-width", "1");
                        cell.Style.Add("border-right-width", "1");
                        break;
                    case "both":
                        cell.BorderWidth = 1;
                        break;
                    default:
                        break;
                }
            }
            if (xmlgrid.Attributes["BorderStyle"] != null)
            {
                cell.Style.Add("border-style", xmlgrid.Attributes["BorderStyle"].Value);
            }
            if (xmlgrid.Attributes["BorderColor"] != null)
            {
                cell.Style.Add("border-color", xmlgrid.Attributes["BorderColor"].Value);
            }
            if (xmlgrid.Attributes["BorderWidth"] != null)
            {
                cell.Style.Add("border-width", xmlgrid.Attributes["BorderWidth"].Value);
            }
        }

        private void CreateChildBand(Control rowband, BLBusinessBase data, int index)
        {
            foreach (XmlNode node in Bands)
            {
                if (node.Attributes["PropertyName"] != null)
                {
                    String PropertyName = node.Attributes["PropertyName"].Value;
                    PropertyInfo property = data.GetType().GetProperty(PropertyName);
                    try
                    {
                        if (property != null)
                        {
                            wptHyperGrid band = new wptHyperGrid(this, (IBLListBase)property.GetValue(data, null));
                            BandCollection.Add(band);
                            band.Width = new Unit("100%");
                            if (node.Attributes["ID"] != null)
                            {
                                band.ID = node.Attributes["ID"].Value;
                            }
                            else
                            {
                                band.ID = rowband.ID + "_childgrid_" + index.ToString();
                            }
                            if (node.Attributes["Width"] != null)
                            {
                                band.Width = new Unit(node.Attributes["Width"].Value);
                            }
                            if (node.Attributes["Height"] != null)
                            {
                                band.Height = new Unit(node.Attributes["Height"].Value);
                            }
                            band.ShowHeader = true;
                            band.ShowMinimizeRestoreButton = true;
                            if (node.Attributes["ShowHeader"] != null)
                            {
                                band.ShowHeader = Convert.ToBoolean(node.Attributes["ShowHeader"].Value);
                            }
                            if (node.Attributes["AutoGenerateSelectButton"] != null)
                            {
                                band.AutoGenerateSelectButton = Convert.ToBoolean(node.Attributes["AutoGenerateSelectButton"].Value);
                            }
                            if (node.Attributes["ShowMinimizeRestoreButton"] != null)
                            {
                                band.ShowMinimizeRestoreButton = Convert.ToBoolean(node.Attributes["ShowMinimizeRestoreButton"].Value);
                            }
                            if (node.Attributes["IsCollapsed"] != null)
                            {
                                band.IsCollapsed = Convert.ToBoolean(node.Attributes["IsCollapsed"].Value);
                            }
                            if (node.Attributes["Title"] != null)
                            {
                                band.Title = node.Attributes["Title"].Value;
                            }
                            if (node.Attributes["HeaderCssClass"] != null)
                            {
                                band.HeaderCssClass = node.Attributes["HeaderCssClass"].Value;
                            }
                            if (node.Attributes["DefaultLock"] != null)
                            {
                                band.DefaultLock = node.Attributes["DefaultLock"].Value;
                            }
                            if (node.Attributes["FactoryMethod"] != null)
                            {
                                band.FactoryMethod = node.Attributes["FactoryMethod"].Value;
                            }
                            if (node.Attributes["CountMethod"] != null)
                            {
                                band.CountMethod = node.Attributes["CountMethod"].Value;
                            }
                            if (node.Attributes["FactoryParameters"] != null)
                            {
                                band.FactoryParameters = node.Attributes["FactoryParameters"].Value;
                            }
                            if (node.Attributes["ClassName"] != null)
                            {
                                band.ClassName = node.Attributes["ClassName"].Value;
                            }
                            if (node.Attributes["CssClass"] != null)
                            {
                                band.CssClass = node.Attributes["CssClass"].Value;
                            }
                            if (node.Attributes["LayoutName"] != null)
                            {
                                band.LayoutName = node.Attributes["LayoutName"].Value;
                            }
                            if (node.Attributes["ComponentBehavior"] != null)
                            {
                                band.ComponentBehaviorType = node.Attributes["ComponentBehavior"].Value;
                            }
                            if (node.Attributes["HeaderCssClass"] != null)
                            {
                                band.HeaderCssClass = node.Attributes["HeaderCssClass"].Value;
                            }
                            rowband.Controls.Add(band);
                        }
                        else
                        {
                            SetError(string.Format(Resources.GenericWebParts.strErrorPropertyNotFound, data.GetType().ToString(), PropertyName));
                        }
                    }
                    catch (Exception ex)
                    {
                        SetError(ex.GetBaseException().Message);
                    }
                }
                else
                {
                    SetError(string.Format(Resources.GenericWebParts.strErrorPropertyNotFoundInLayout, node.Name));
                }
            }
        }
        /// <summary>
        /// Creates the table's header
        /// </summary>
        /// <param name="table">The table that will have the header</param>
        /// <param name="FixedHeads">The collection of fixed headers</param>
        /// <param name="heads">The collection of non-fixed headers</param>
        /// <returns>The non-fixed column total size</returns>
        private Unit CreateHeader(Table table, ArrayList FixedHeads, ArrayList heads, XmlNode xmlgrid)
        {
            TableHeaderRow row = new TableHeaderRow();
            row.HorizontalAlign = HorizontalAlign.Left;
            HyperRow hyperR = new HyperRow(-1, row);
            if (Bands.Count != 0)
            {
                TableHeaderCell header = new TableHeaderCell();
                header.Text = "&nbsp";
                header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                header.Width = new Unit("25px");
                row.Cells.Add(header);
                SetGridLines(xmlgrid, header);
            }
            RowHeaderCollection.Add(hyperR);
            if (this.AutoGenerateSelectButton)
            {
                TableHeaderCell header = new TableHeaderCell();
                header.Text = "&nbsp";
                header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                header.Width = new Unit("50px");
                row.Cells.Add(header);
                SetGridLines(xmlgrid, header);
            }

            bool registerColumns = (ColumnCollection.Count == 0);
            if (registerColumns)
            {
                foreach (ColumnTemplate col in FixedColums)
                {
                    HyperColumn column = new HyperColumn(col.index, false);
                    column.IsEditable = col.IsEditable;
                    column.Property = col.ContentProperty;
                    ColumnCollection.Add(column);
                }
                foreach (ColumnTemplate col in Columns)
                {
                    HyperColumn column = new HyperColumn(col.index, false);
                    column.IsEditable = col.IsEditable;
                    column.Property = col.ContentProperty;
                    column.PropertyFriendlyName = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, col.PropertyFriendlyName);
                    ColumnCollection.Add(column);
                }
            }

            int headerCounter = 0;
            foreach (ColumnTemplate head in FixedHeads)
            {
                XmlNode gNode = head.gNode;
                TableHeaderCell cell = new TableHeaderCell();

                if (head.IsEditable)
                {
                    XmlDocument doc = new XmlDocument();
                    string AdditionalStyles = "";
                    if (head.gNode.Attributes["EditorStyle"] != null)
                    {
                        AdditionalStyles = head.gNode.Attributes["EditorStyle"].Value.Replace("\"", "");
                    }
                    doc.LoadXml("<Control FieldName=\"" + head.ContentProperty + "\" Type=\"" + head.gNode.Attributes["EditorType"].Value + "\" ID=\"" + this.ID + "_input_" + headerCounter + "\" Style=\"display:none;position:absolute;left:0px;top:0px;" + AdditionalStyles + "\" />");
                    XmlNode EditorNode = doc.ChildNodes[0];
                    EditorNode.InnerXml = head.gNode.InnerXml;

                    HiddenField lockfield = new HiddenField();
                    if (LockFieldCollection[this.ID + "_lock_" + headerCounter] == null)
                    {
                        lockfield.ID = this.ID + "_lock_" + headerCounter;
                        lockfield.Value = (DefaultLock.ToLower() == "locked").ToString();
                        LockFieldCollection[lockfield.ID] = lockfield;
                    }
                    else
                    {
                        lockfield = (HiddenField)LockFieldCollection[this.ID + "_lock_" + headerCounter];
                    }

                    EditorContainer.Controls.Add(lockfield);

                    object data = CreateNewBusinessObject();
                    CreateEditorControl(data, EditorNode, EditorContainer, false, headerCounter);
                }

                Table headerTable = new Table();
                headerTable.Width = new Unit("100%");
                headerTable.Height = new Unit("100%");
                headerTable.BorderStyle = BorderStyle.None;
                headerTable.CellPadding = 0;
                headerTable.CellSpacing = 0;
                cell.Controls.Add(headerTable);
                TableHeaderRow headerRow = new TableHeaderRow();
                TableHeaderCell headerCell = new TableHeaderCell();
                TableHeaderCell headerlockCell = new TableHeaderCell();
                headerCell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                headerTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
                headerCell.Width = new Unit("100%");
                headerTable.Width = new Unit("100%");
                headerCell.Height = new Unit("100%");
                headerTable.Height = new Unit("100%");
                headerCell.Text = "&nbsp";
                if (head.IsHidden)
                {
                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                if ((LockOnColumns) && (head.IsEditable))
                {
                    System.Web.UI.WebControls.Image lockimg = new System.Web.UI.WebControls.Image();
                    HiddenField lockvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + headerCounter];
                    if (lockvalue != null && lockvalue.Value != "" && Convert.ToBoolean(lockvalue.Value))
                    {
                        lockimg.ImageUrl = LockImageURL;
                    }
                    else
                    {
                        lockimg.ImageUrl = UnLockImageURL;
                    }
                    ImageLockCollection[headerCounter] = lockimg;
                    headerlockCell.Controls.Add(lockimg);
                    headerRow.Cells.Add(headerlockCell);
                }

                headerRow.Cells.Add(headerCell);
                headerTable.Rows.Add(headerRow);

                SetAppareance(headerCell, head.gNode);
                SetAppareance(headerlockCell, head.gNode);

                cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                if (gNode.Attributes["HeaderCssClass"] != null)
                {
                    cell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                    headerCell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                }
                if (gNode.Attributes["HeaderStyle"] != null)
                {
                    cell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                    headerCell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                }
                if (xmlgrid.Attributes["CssClass"] != null)
                {
                    headerCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                    headerlockCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                }
                if (gNode.Attributes["Header"] != null)
                {
                    Label text = new Label();
                    text.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    text.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, gNode.Attributes["Header"].Value);
                    headerCell.Controls.Add(text);
                }
                headerCell.ID = this.ID + "_header_" + headerCounter;
                SetGridLines(xmlgrid, cell);
                head.InstantiateHeader(headerCell, null, -1);
                row.Cells.Add(cell);
                headerCounter++;
            }
            TableHeaderCell contentcell = new TableHeaderCell();
            contentcell.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentcell.Style.Add("colspan", heads.Count.ToString());
            Panel contentPanel = new Panel();
            contentPanel.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentPanel.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");
            contentPanel.Width = new Unit("100%");
            contentcell.Controls.Add(contentPanel);
            Table contentTable = new Table();
            contentTable.CssClass = xmlgrid.Attributes["CssClass"].Value;
            contentTable.Style["table-layout"] = "fixed";
            hyperR.scrollPanel = contentTable;
            contentTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentTable.CellPadding = 0;
            contentTable.CellSpacing = 0;
            double tablesize = 0;
            contentPanel.Controls.Add(contentTable);
            CreateCols(contentTable, heads, RepColumns, false, false, -1, -1, EditionRow);
            TableHeaderRow contentRow = new TableHeaderRow();
            contentTable.Rows.Add(contentRow);
            UnitType SizeType = UnitType.Pixel;
            foreach (ColumnTemplate head in heads)
            {
                XmlNode gNode = head.gNode;
                TableHeaderCell cell = new TableHeaderCell();

                if (head.IsEditable)
                {
                    XmlDocument doc = new XmlDocument();
                    string AdditionalStyles = "";
                    if (head.gNode.Attributes["EditorStyle"] != null)
                    {
                        AdditionalStyles = head.gNode.Attributes["EditorStyle"].Value.Replace("\"", "");
                    }
                    doc.LoadXml("<Control FieldName=\"" + head.ContentProperty + "\" Type=\"" + head.gNode.Attributes["EditorType"].Value + "\" ID=\"" + this.ID + "_input_" + headerCounter + "\" Style=\"display:none;position:absolute;left:0px;top:0px;" + AdditionalStyles + "\" />");
                    XmlNode EditorNode = doc.ChildNodes[0];

                    HiddenField lockfield = new HiddenField();
                    if (LockFieldCollection[this.ID + "_lock_" + headerCounter] == null)
                    {
                        lockfield.ID = this.ID + "_lock_" + headerCounter;
                        lockfield.Value = (DefaultLock.ToLower() == "locked").ToString();
                        LockFieldCollection[lockfield.ID] = lockfield;
                    }
                    else
                    {
                        lockfield = (HiddenField)LockFieldCollection[this.ID + "_lock_" + headerCounter];
                    }

                    EditorContainer.Controls.Add(lockfield);

                    EditorNode.InnerXml = head.gNode.InnerXml;
                    object data = CreateNewBusinessObject();
                    CreateEditorControl(data, EditorNode, EditorContainer, false, headerCounter);
                }

                Table headerTable = new Table();
                headerTable.BorderStyle = BorderStyle.None;
                headerTable.CellSpacing = 0;
                headerTable.CellPadding = 0;
                cell.Controls.Add(headerTable);
                TableHeaderRow headerRow = new TableHeaderRow();
                TableHeaderCell headerCell = new TableHeaderCell();
                TableHeaderCell headerlockCell = new TableHeaderCell();
                headerCell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                headerTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
                headerCell.Width = new Unit("100%");
                headerTable.Width = new Unit("100%");
                headerCell.Height = new Unit("100%");
                headerTable.Height = new Unit("100%");

                if ((LockOnColumns) && (head.IsEditable))
                {
                    headerRow.Cells.Add(headerlockCell);
                }
                headerCell.Text = "&nbsp";
                headerRow.Cells.Add(headerCell);
                headerTable.Rows.Add(headerRow);
                if ((LockOnColumns) && (head.IsEditable))
                {
                    System.Web.UI.WebControls.Image lockimg = new System.Web.UI.WebControls.Image();
                    HiddenField lockvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + headerCounter];
                    if (lockvalue != null && lockvalue.Value != "" && Convert.ToBoolean(lockvalue.Value))
                    {
                        lockimg.ImageUrl = LockImageURL;
                    }
                    else
                    {
                        lockimg.ImageUrl = UnLockImageURL;
                    }
                    ImageLockCollection[headerCounter] = lockimg;
                    headerlockCell.Controls.Add(lockimg);
                }

                //SetAppareance(cell, head.gNode);
                SetAppareance(headerCell, head.gNode);
                SetAppareance(headerlockCell, head.gNode);
                if (head.IsHidden)
                {
                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                if (xmlgrid.Attributes["CssClass"] != null)
                {
                    headerCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                    headerlockCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                }
                if (gNode.Attributes["HeaderCssClass"] != null)
                {
                    headerCell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                    headerlockCell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                }
                if (gNode.Attributes["HeaderStyle"] != null)
                {
                    headerCell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                    headerlockCell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                }
                if (!head.IsHidden)
                {
                    tablesize += head.Width.Value;
                    SizeType = head.Width.Type;
                }
                if (gNode.Attributes["Header"] != null)
                {
                    Label text = new Label();
                    text.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    text.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, gNode.Attributes["Header"].Value);
                    headerCell.Controls.Add(text);
                }

                headerCell.ID = this.ID + "_header_" + headerCounter;
                SetGridLines(xmlgrid, cell);
                head.InstantiateHeader(headerCell, null, -1);
                contentRow.Cells.Add(cell);
                headerCounter++;
            }
            tablesize += CreateRepeatableHeader(contentRow, xmlgrid, headerCounter);
            if (EditionRow)
            {
                if (SizeType != UnitType.Percentage)
                {
                    tablesize += 25;
                }
                TableHeaderCell header = new TableHeaderCell();
                header.Style.Add(HtmlTextWriterStyle.Position, "relative");
                header.Width = new Unit("25px");
                contentRow.Cells.Add(header);
                SetGridLines(xmlgrid, header);
                TableHeaderCell cancelheader = new TableHeaderCell();
                cancelheader.Style.Add(HtmlTextWriterStyle.Position, "relative");
                cancelheader.Width = new Unit("25px");
                contentRow.Cells.Add(cancelheader);
                SetGridLines(xmlgrid, cancelheader);
            }
            row.Cells.Add(contentcell);
            table.Rows.Add(row);
            Unit size = new Unit(tablesize, SizeType);
            contentTable.Width = size;
            return size;
        }

        private void CreateEditionRow(Table table, ArrayList fcolumns, ArrayList cols, int rowcounter, Unit tableSize, XmlNode xmlgrid)
        {
            TableRow row = new TableRow();
            bool MessageShow = false;
            if (Bands.Count != 0)
            {
                TableHeaderCell Auto = new TableHeaderCell();
                Auto.Style.Add(HtmlTextWriterStyle.Position, "relative");
                Auto.Width = new Unit("25px");
                row.Cells.Add(Auto);
                SetGridLines(xmlgrid, Auto);
            }

            if (this.AutoGenerateSelectButton)
            {
                TableHeaderCell Auto = new TableHeaderCell();
                Auto.Style.Add(HtmlTextWriterStyle.Position, "relative");
                Auto.Width = new Unit("50px");
                row.Cells.Add(Auto);
                SetGridLines(xmlgrid, Auto);
            }
            int cellcounter = 0;
            foreach (ColumnTemplate col in fcolumns)
            {
                TableCell cell = new TableCell();
                cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                cell.ID = this.ID + "_cell_addrow_" + cellcounter;
                if (col.IsHidden)
                {
                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                if (col.IsEditable)
                {
                    string LabelID = this.ID + "_cell_addrow_" + cellcounter + "_lbl";
                    string ValueID = this.ID + "_cell_addrow_" + cellcounter + "_value";
                    Label text = new Label();
                    HiddenField field = new HiddenField();
                    text.ID = LabelID;
                    if (!MessageShow && !col.IsHidden)
                    {
                        text.Text = xmlgrid.Attributes["EditionRow"].Value;
                        MessageShow = !MessageShow;
                    }
                    EditionRowLabels.Add(text);


                    field.ID = ValueID;
                    EditionRowValues.Add(field);

                    ColumnCollection[cellcounter].ValueField = field;
                    cell.Controls.Add(text);
                    cell.Attributes.Add("editable", "true");
                    cell.Controls.Add(field);
                }
                cell.Attributes.Add("rdk", "");
                cell.Attributes.Add("cdk", "");
                row.Cells.Add(cell);
                cellcounter++;
                SetGridLines(xmlgrid, cell);
            }
            TableCell contentcell = new TableCell();
            contentcell.HorizontalAlign = HorizontalAlign.Left;
            contentcell.Style.Add("colspan", cols.Count.ToString());
            Panel contentPanel = new Panel();
            contentPanel.Height = new Unit("100%");
            contentPanel.Width = new Unit("100%");
            contentcell.Controls.Add(contentPanel);
            Table contentTable = new Table();
            HyperEditionRow = new HyperRow((rowcounter + 1), row);
            HyperEditionRow.scrollPanel = contentTable;
            contentTable.Style["table-layout"] = "fixed";
            contentTable.Height = new Unit("100%");
            contentTable.CellPadding = 0;
            contentTable.CellSpacing = 0;
            contentPanel.Style.Add(HtmlTextWriterStyle.Overflow, "hidden");
            contentPanel.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentcell.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
            contentTable.Width = tableSize;
            contentPanel.Controls.Add(contentTable);
            CreateCols(contentTable, cols, RepColumns, false, true, -1, -1, true);
            TableRow contentRow = new TableRow();
            if (xmlgrid.Attributes["CellPadding"] != null)
            {
                contentTable.CellPadding = Convert.ToInt32((xmlgrid.Attributes["CellPadding"]).Value);
                mainGrid.CellPadding = Convert.ToInt32((xmlgrid.Attributes["CellPadding"]).Value);
            }
            if (xmlgrid.Attributes["CellSpacing"] != null)
            {
                mainGrid.CellSpacing = Convert.ToInt32((xmlgrid.Attributes["CellSpacing"]).Value);
                contentTable.CellSpacing = Convert.ToInt32((xmlgrid.Attributes["CellSpacing"]).Value);
            }
            if (xmlgrid.Attributes["EditionRowCssClass"] != null)
            {
                row.CssClass = (xmlgrid.Attributes["EditionRowCssClass"]).Value;
                contentRow.CssClass = (xmlgrid.Attributes["EditionRowCssClass"]).Value;
            }
            if (xmlgrid.Attributes["EditionRowStyle"] != null)
            {
                row.CssClass = (xmlgrid.Attributes["EditionRowStyle"]).Value;
                contentRow.Style.Value = (xmlgrid.Attributes["EditionRowStyle"]).Value;
            }
            if (xmlgrid.Attributes["EditionRowHeight"] != null)
            {
                row.CssClass = (xmlgrid.Attributes["EditionRowHeight"]).Value;
                contentRow.Style.Add("Height", (xmlgrid.Attributes["EditionRowHeight"]).Value);
            }
            contentRow.ID = this.ID + "_nfrow_" + rowcounter;
            contentTable.Rows.Add(contentRow);
            foreach (ColumnTemplate col in cols)
            {
                TableCell cell = new TableCell();
                cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter;
                cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                cell.Text = "&nbsp";
                if (col.IsHidden)
                {
                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                if (col.IsEditable)
                {

                    Label text = new Label();
                    HiddenField field = new HiddenField();
                    string labelID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_lbl";
                    string valueID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_value";

                    text.ID = labelID;
                    if (!MessageShow && !col.IsHidden)
                    {
                        text.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, xmlgrid.Attributes["EditionRow"].Value);
                        MessageShow = !MessageShow;
                    }
                    EditionRowLabels.Add(text);

                    field.ID = valueID;
                    EditionRowValues.Add(field);


                    ColumnCollection[cellcounter].ValueField = field;
                    cell.Controls.Add(text);
                    cell.Attributes.Add("editable", "true");
                    cell.Controls.Add(field);
                }
                cell.Attributes.Add("rdk", "");
                cell.Attributes.Add("cdk", "");
                SetAppareance(cell, col.gNode);
                contentRow.Cells.Add(cell);
                cellcounter++;
                SetGridLines(xmlgrid, cell);
            }
            if ((m_Datasource != null) && ((IBLListBase)m_Datasource).Count != 0)
            {
                object dsource = ((IBLListBase)m_Datasource)[0];
                foreach (XmlNode gNode in RepColumns)
                {
                    IBLListBase property = (IBLListBase)GetProperty(gNode, dsource);
                    bool registerColumns = (ColumnCollection.Count == 0);
                    foreach (BLBusinessBase data in property)
                    {
                        foreach (XmlNode ChildNode in gNode)
                        {
                            if (ChildNode.Name.ToUpper() == "COLUMN")
                            {
                                TableCell cell = new TableCell();
                                cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                                cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter;
                                ColumnTemplate col = new ColumnTemplate(this, ChildNode, cellcounter);
                                if (col.IsHidden)
                                {
                                    cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                                }
                                if (col.IsEditable)
                                {
                                    Label text = new Label();
                                    HiddenField field = new HiddenField();
                                    string labelID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_lbl";
                                    string valueID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_value";

                                    text.ID = labelID;
                                    if (!MessageShow && !col.IsHidden)
                                    {
                                        text.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, xmlgrid.Attributes["EditionRow"].Value);
                                        MessageShow = !MessageShow;
                                    }
                                    EditionRowLabels.Add(text);


                                    field.ID = valueID;
                                    EditionRowValues.Add(field);

                                    ColumnCollection[cellcounter].ValueField = field;

                                    cell.Controls.Add(text);
                                    cell.Attributes.Add("editable", "true");
                                    cell.Controls.Add(field);
                                }
                                cell.Attributes.Add("cdk", "");
                                cell.Attributes.Add("rdk", "");
                                contentRow.Controls.Add(cell);
                                cellcounter++;
                                SetGridLines(xmlgrid, cell);
                            }
                        }
                    }
                }
            }
            TableHeaderCell header = new TableHeaderCell();
            header.Style.Add(HtmlTextWriterStyle.Position, "relative");
            Axelerate.BusinessLayerUITools.WebControls.ImageButton btnSave = new Axelerate.BusinessLayerUITools.WebControls.ImageButton();
            btnSave.OnClientClick = "axGrid_CommitActiveCell();";
            if (Page != null)
            {
                btnSave.MouseOutImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.save.gif");
                btnSave.PressedImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.save_down.gif");
                btnSave.MouseOverImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.save_over.gif");
            }
            btnSave.UseImageButtonComposer = false;
            btnSave.Command += new CommandEventHandler(btnSave_Command);
            btnSave.CausesValidation = false;
            if (xmlgrid.Attributes["EditionRowAddTooltip"] != null)
            {
                btnSave.ToolTip = clsSharedMethods.SharedMethods.getStringFromResources(this.m_Datasource, xmlgrid.Attributes["EditionRowAddTooltip"].Value);
            }
            else
            {
                btnSave.ToolTip = Resources.LocalizationUIToolsResource.strAddButton;
            }
            header.Width = new Unit("25px");
            header.Controls.Add(btnSave);
            contentRow.Cells.Add(header);
            SetGridLines(xmlgrid, header);

            TableHeaderCell cancelheader = new TableHeaderCell();
            cancelheader.Style.Add(HtmlTextWriterStyle.Position, "relative");
            Axelerate.BusinessLayerUITools.WebControls.ImageButton btnCancel = new Axelerate.BusinessLayerUITools.WebControls.ImageButton();
            btnCancel.CausesValidation = false;
            if (Page != null)
            {
                btnCancel.MouseOutImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.cancel.png");
                btnCancel.PressedImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.cancel_down.png");
                btnCancel.MouseOverImageUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.cancel_over.png");
            }
            btnCancel.ToolTip = Resources.LocalizationUIToolsResource.strClearButton;
            btnCancel.UseImageButtonComposer = false;
            btnCancel.Command += new CommandEventHandler(btnCancel_Command);
            cancelheader.Width = new Unit("25px");
            cancelheader.Controls.Add(btnCancel);
            contentRow.Cells.Add(cancelheader);
            SetGridLines(xmlgrid, cancelheader);

            row.Cells.Add(contentcell);
            if (xmlgrid.Attributes["OffsetEditionRow"] != null)
            {
                TableRow divRow = new TableRow();
                TableCell divCell = new TableCell();
                divCell.Height = new Unit(xmlgrid.Attributes["OffsetEditionRow"].Value);
                divCell.Width = row.Width;
                divCell.ColumnSpan = row.Cells.Count;
                if (xmlgrid.Attributes["GridHeaderCssClass"] != null)
                {
                    divRow.CssClass = xmlgrid.Attributes["GridHeaderCssClass"].Value;
                }
                else
                {
                    divRow.CssClass = row.CssClass;
                }
                divRow.Cells.Add(divCell);
                table.Rows.Add(divRow);
            }

            table.Rows.Add(row);
        }

        void btnCancel_Command(object sender, CommandEventArgs e)
        {
            foreach (HiddenField field in EditionRowValues)
            {
                field.Value = "";
            }
        }

        internal override IButtonControl CreateCommand(XmlNode gnode, object Datasource, Control container, bool IsPostBack, CommandEventHandler Handler, int DatasourceIndex)
        {
            IButtonControl button = base.CreateCommand(gnode, Datasource, container, IsPostBack, Handler, DatasourceIndex);
            if ((gnode.Attributes["Command"].Value.ToLower() == "update") && (typeof(Axelerate.BusinessLayerUITools.WebControls.ImageButton).IsAssignableFrom(button.GetType())))
            {
                ((Axelerate.BusinessLayerUITools.WebControls.ImageButton)button).OnClientClick = "axGrid_CommitActiveCell()";
            }
            if ((gnode.Attributes["Command"].Value.ToLower() == "update") && (typeof(LinkButton).IsAssignableFrom(button.GetType())))
            {
                ((LinkButton)button).OnClientClick = "axGrid_CommitActiveCell()";
            }
            return button;
        }
        private BLBusinessBase CreateNewBusinessObject()
        {
            Type ObjType = ((IBLListBase)m_Datasource).DataLayer.BusinessLogicType;
            BLBusinessBase BL = null;
            MethodInfo mt = m_Datasource.GetType().GetMethod("NewBusinessObject");
            if (mt != null)
            {
                BL = (BLBusinessBase)mt.Invoke(m_Datasource, new object[] { });//(BLBusinessBase)System.Activator.CreateInstance(ObjType);
            }
            else
            {
                BL = (BLBusinessBase)System.Activator.CreateInstance(ObjType);
            }
            return BL;
        }

        private void btnSave_Command(object sender, CommandEventArgs e)
        {
            Type ObjType = ((IBLListBase)m_Datasource).DataLayer.BusinessLogicType;
            BLBusinessBase BL = CreateNewBusinessObject();

            bool CanUpdate = false;
            bool CanRead = false;
            try
            {
                foreach (HyperColumn col in ColumnCollection)
                {
                    if (col.IsNewRowEditable)
                    {
                        PropertyInfo property = ObjType.GetProperty(col.Property);
                        CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(BL, "UPDATE", col.Property);
                        if (!col.RepeatableColumn)
                        {
                            if (CanUpdate)
                            {
                                object Value = null;
                                try
                                {
                                    Value = Convert.ChangeType(col.ValueField.Value, property.PropertyType);
                                }
                                catch (Exception ex)
                                {
                                    if (col.ValueField.Value == "")
                                    {
                                        SetError(String.Format(Resources.ErrorMessages.errRequiredValue, col.PropertyFriendlyName));
                                    }
                                    else
                                    {
                                        SetError(String.Format(Resources.ErrorMessages.errTryingConvertValueOnAddrow, col.PropertyFriendlyName, col.ValueField.Value));
                                    }
                                    errorthrown = true;
                                    return;
                                }
                                try
                                {
                                    property.SetValue(BL, Value, null); //col.value
                                }
                                catch (Exception ex)
                                {
                                    SetError(ex.GetBaseException().Message);
                                    errorthrown = true;
                                }
                            }
                        }
                    }
                }
                MethodInfo savemethod = m_Datasource.GetType().GetMethod("Save");

                if (!errorthrown)
                {
                    ObjType.GetProperty("GUID").SetValue(BL, Guid.NewGuid().ToString(), null);

                    if (BL.IsValid)
                    {
                        //Save main object
                        ((IBLListBase)m_Datasource).Add(BL);

                        m_Datasource = savemethod.Invoke(m_Datasource, null);
                        ResetDatasource();
                    }
                    else
                    {
                        System.Text.StringBuilder errors = new System.Text.StringBuilder();
                        foreach (ValidationError bRule in BL.BrokenRulesCollection)
                        {
                            bool Found = false;
                            string ValueToReplace = "";
                            string FinalValue = "";
                            for (int count = 0; count < ColumnCollection.Count && !Found; count++)
                            {
                                HyperColumn col = ColumnCollection[count];
                                if (col.Property == bRule.ErrorLocation)
                                {
                                    Found = true;
                                    ValueToReplace = col.PropertyFriendlyName;
                                }
                            }
                            if (Found)
                            {
                                FinalValue = bRule.ErrorText.Replace(bRule.ErrorLocation, ValueToReplace);
                            }
                            else
                            {
                                FinalValue = bRule.ErrorText;
                            }
                            errors.AppendLine(FinalValue + "<br/>");
                        }
                        SetError(/*Resources.ErrorMessages.errSavingBOInvalidState + "<br/>" + */errors.ToString());
                        errorthrown = true;
                    }
                }
                foreach (HyperColumn col in ColumnCollection)
                {
                    if (col.IsNewRowEditable)
                    {
                        if (col.RepeatableColumn)
                        {
                            CanRead = clsSharedMethods.SharedMethods.CheckAccess(BL, "READ", col.Property);
                            if (CanRead)
                            {
                                PropertyInfo baseProperty = ObjType.GetProperty(col.Property);
                                string guidProperty = (col.ContentXml.Attributes["KeyProperty"] != null && col.ContentXml.Attributes["KeyProperty"].ToString() != "") ? col.ContentXml.Attributes["KeyProperty"].Value : "DetailDataKey";
                                BLBusinessBase SubObject = ((IBLListBase)baseProperty.GetValue(BL, null)).Find(guidProperty, col.RepeatableColumnKey);
                                if (SubObject != null)
                                {
                                    PropertyInfo property = SubObject.GetType().GetProperty(col.PropertyBound);
                                    CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(SubObject, "UPDATE", col.PropertyBound);
                                    if (CanUpdate)
                                    {
                                        object Value = null;
                                        Value = Convert.ChangeType(col.ValueField.Value, property.PropertyType);

                                        property.SetValue(SubObject, Value, null);
                                    }
                                }
                            }
                        }
                    }
                }
                if (!errorthrown)
                {
                    if (BL.IsValid)
                    {
                        m_Datasource = savemethod.Invoke(m_Datasource, null);
                        ResetDatasource();
                    }
                    else
                    {
                        System.Text.StringBuilder errors = new System.Text.StringBuilder();
                        foreach (ValidationError bRule in BL.BrokenRulesCollection)
                        {
                            bool Found = false;
                            string ValueToReplace = "";
                            string FinalValue = "";
                            for (int count = 0; count < ColumnCollection.Count && !Found; count++)
                            {
                                HyperColumn col = ColumnCollection[count];
                                if (col.Property == bRule.ErrorLocation)
                                {
                                    Found = true;
                                    ValueToReplace = col.PropertyFriendlyName;
                                }
                            }
                            if (Found)
                            {
                                FinalValue = bRule.ErrorText.Replace(bRule.ErrorLocation, ValueToReplace);
                            }
                            else
                            {
                                FinalValue = bRule.ErrorText;
                            }
                            errors.AppendLine(FinalValue + "<br/>");
                        }
                        SetError(/*Resources.ErrorMessages.errSavingBOInvalidState + "<br/>" + */errors.ToString());
                        errorthrown = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Text.StringBuilder errors = new System.Text.StringBuilder();
                foreach (ValidationError bRule in BL.BrokenRulesCollection)
                {
                    errors.AppendLine(bRule.ErrorText + "<br/>");
                }
                SetError(ex.GetBaseException().Message);
                errorthrown = true;
            }
            if (!errorthrown)
            {
                ClearError();
            }
        }

        private double CreateRepeatableHeader(Control contentRow, XmlNode xmlgrid, int headerCounter)
        {
            double columnsize = 0;
            int cellcounter = headerCounter;
            foreach (XmlNode gNode in RepColumns)
            {
                IBLListBase source = null;

                if (gNode.Attributes["DSClassName"] != null)
                {
                    string rcclassname = gNode.Attributes["DSClassName"].Value;
                    string factmethod = (gNode.Attributes["FactoryMethod"] == null) ? "" : gNode.Attributes["FactoryMethod"].Value;
                    string factparam = (gNode.Attributes["FactoryParameters"] == null) ? "" : gNode.Attributes["FactoryParameters"].Value;

                    source = (IBLListBase)GetBusinessClassInstance(rcclassname, factmethod, factparam);
                }
                else
                {
                    if (m_Datasource != null)
                    {
                        IBLListBase data = (IBLListBase)m_Datasource;
                        if (data.Count != 0)
                        {
                            source = (IBLListBase)GetProperty(gNode, data[0]);
                        }
                    }
                }
                int index = 0;
                if (source == null) return 0;
                foreach (BLBusinessBase data in source)
                {
                    TableHeaderCell cell = new TableHeaderCell();
                    cell.Text = "&nbsp";
                    cell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                    Table headerTable = new Table();
                    headerTable.Width = new Unit("100%");
                    headerTable.Height = new Unit("100%");
                    headerTable.BorderStyle = BorderStyle.None;
                    headerTable.CellSpacing = 0;
                    headerTable.CellPadding = 0;
                    cell.Controls.Add(headerTable);
                    TableHeaderRow headerRow = new TableHeaderRow();
                    TableHeaderCell headerCell = new TableHeaderCell();
                    TableHeaderCell headerlockCell = new TableHeaderCell();
                    headerCell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    headerTable.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    headerCell.Width = new Unit("100%");
                    headerTable.Width = new Unit("100%");
                    headerCell.Height = new Unit("100%");
                    headerTable.Height = new Unit("100%");
                    ColumnTemplate head = new ColumnTemplate(this, gNode, index);


                    headerRow.Cells.Add(headerlockCell);

                    headerRow.Cells.Add(headerCell);
                    headerTable.Rows.Add(headerRow);

                    cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                    if (xmlgrid.Attributes["CssClass"] != null)
                    {
                        //cell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                        headerCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                        headerlockCell.CssClass = (xmlgrid.Attributes["CssClass"]).Value;
                    }
                    if (gNode.Attributes["HeaderCssClass"] != null)
                    {
                        headerCell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                        headerlockCell.CssClass = (gNode.Attributes["HeaderCssClass"]).Value;
                    }
                    if (gNode.Attributes["HeaderStyle"] != null)
                    {
                        headerCell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                        headerlockCell.Style.Value = (gNode.Attributes["HeaderStyle"]).Value;
                    }
                    if (gNode.Attributes["Header"] != null)
                    {
                        Label text = new Label();
                        text.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        text.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, gNode.Attributes["Header"].Value);
                        headerCell.Controls.Add(text);
                    }


                    if (!head.IsHidden)
                    {
                        cell.Width = head.Width;
                        columnsize += head.Width.Value;
                    }
                    else
                    {
                        cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    SetAppareance(headerCell, head.gNode);
                    int repIndex = 0;
                    foreach (XmlNode Node in gNode.ChildNodes)          //It supports just 1 editale column for each repeatable column
                    {
                        HyperColumn column = new HyperColumn(cellcounter, true);
                        column.ContentXml = Node;
                        column.Property = gNode.Attributes["PropertyName"].Value;
                        string content = "";
                        if (Node.Attributes["ColumnPropertyType"].Value.ToUpper() == "PROPERTY")
                        {
                            content = Node.Attributes["Content"].Value;
                        }
                        column.PropertyBound = content;
                        bool editable = false;
                        if (Node.Attributes["EditBehavior"] != null)
                        {
                            if (Node.Attributes["EditBehavior"].Value.ToUpper() == "EDITABLE")
                            {
                                editable = true;
                            }
                        }
                        column.IsEditable = editable;
                        if (!column.IsEditable)
                        {
                            column.repeatIndex = repIndex;
                            repIndex++;
                        }
                        else
                        {
                            HiddenField lockfield = new HiddenField();
                            if (LockFieldCollection[this.ID + "_lock_" + cellcounter] == null)
                            {
                                lockfield.ID = this.ID + "_lock_" + cellcounter;
                                lockfield.Value = (DefaultLock.ToLower() == "locked").ToString();
                                LockFieldCollection[lockfield.ID] = lockfield;
                            }
                            else
                            {
                                lockfield = (HiddenField)LockFieldCollection[this.ID + "_lock_" + cellcounter];
                            }

                            EditorContainer.Controls.Add(lockfield);
                        }
                        if (typeof(IBLMNRelationBusinessBase).IsAssignableFrom(data.GetType()))
                        {
                            column.RepeatableColumnKey = ((IBLMNRelationBusinessBase)data).DetailDataKey;
                        }
                        else
                        {
                            if (typeof(BLBusinessBase).IsAssignableFrom(data.GetType()))
                            {
                                column.RepeatableColumnKey = ((BLBusinessBase)data)["GUID"];
                            }
                            else
                            {
                                this.SetError(Resources.ErrorMessages.errDataSupportIBLMNR);
                            }
                        }
                        RepeateableColumnCollection.Add(column);
                        ColumnCollection.Add(column);
                        if ((LockOnColumns) && (editable))
                        {
                            if (headerlockCell.Controls.Count == 0)
                            {
                                System.Web.UI.WebControls.Image lockimg = new System.Web.UI.WebControls.Image();
                                HiddenField lockvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + cellcounter];
                                if (lockvalue != null && lockvalue.Value != "" && Convert.ToBoolean(lockvalue.Value))
                                {
                                    lockimg.ImageUrl = LockImageURL;
                                }
                                else
                                {
                                    lockimg.ImageUrl = UnLockImageURL;
                                }
                                ImageLockCollection[headerCounter] = lockimg;
                                headerlockCell.Controls.Add(lockimg);
                            }
                        }
                    }

                    if (head.IsHidden)
                    {
                        cell.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                    //SetAppareance(cell, head.gNode);
                    SetAppareance(headerCell, head.gNode);
                    cellcounter++;
                    //cell.ID = this.ID + "_header_" + headerCounter;
                    headerCell.ID = this.ID + "_header_" + headerCounter;
                    SetGridLines(xmlgrid, cell);
                    //SetAppareance(cell, xmlgrid);
                    head.InstantiateHeader(headerCell, data, index);
                    contentRow.Controls.Add(cell);
                    ArrayList cols = FindNodes(gNode, "Column");
                    headerCounter++;
                    index++;
                }
            }
            return columnsize;
        }

        private void CreateRepeatableColumn(Control contentRow, object source, int rowcounter, int cellcounter, XmlNode xmlgrid)
        {
            foreach (XmlNode gNode in RepColumns)
            {
                int colnum = FindNodes(gNode, "column").Count;

                IBLListBase property = (IBLListBase)GetProperty(gNode, source);
                int index = 0;
                int columnCounter = 0;
                foreach (HyperColumn hcolumn in RepeateableColumnCollection)
                {
                    string guidProperty = (hcolumn.ContentXml.Attributes["KeyProperty"] != null && hcolumn.ContentXml.Attributes["KeyProperty"].ToString() != "") ? hcolumn.ContentXml.Attributes["KeyProperty"].Value : "DetailDataKey";

                    BLBusinessBase data = property.Find(guidProperty, hcolumn.RepeatableColumnKey);

                    XmlNode ChildNode = hcolumn.ContentXml;

                    if (ChildNode.Name.ToUpper() == "COLUMN")
                    {
                        TableCell cell = new TableCell();
                        SetAppareance(cell, hcolumn.ContentXml);
                        cell.Text = "&nbsp";
                        cell.Style.Add(HtmlTextWriterStyle.Position, "relative");
                        cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter;
                        ColumnTemplate col = new ColumnTemplate(this, ChildNode, cellcounter);

                        HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + cellcounter];
                        if ((hvalue != null) && (Convert.ToBoolean(hvalue.Value)) && (col.gNode.Attributes["LockedCssClass"] != null))
                        {
                            cell.CssClass = col.gNode.Attributes["LockedCssClass"].Value;
                        }
                        if (data == null)
                        {
                            Label propertycontrol = new Label();
                            propertycontrol.ID = cell.ID + "_lbl";
                            if (ChildNode.Attributes["NullContent"] != null)
                            {
                                propertycontrol.Text = clsSharedMethods.SharedMethods.getStringFromResources(m_Datasource, ChildNode.Attributes["NullContent"].Value);
                            }
                            if (!col.IsEditable)
                            {
                                cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_repeatable_" + hcolumn.repeatIndex;
                                propertycontrol.ID = cell.ID + "_repeatable_" + hcolumn.repeatIndex + "_lbl";
                            }
                            cell.Controls.Add(propertycontrol);
                            contentRow.Controls.Add(cell);
                        }
                        else
                        {
                            if (hcolumn.IsEditable)
                            {
                                string AdditionalStyles = "";
                                if (ChildNode.Attributes["EditorStyle"] != null)
                                {
                                    AdditionalStyles = ChildNode.Attributes["EditorStyle"].Value.Replace("\"", "");
                                }
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml("<Control FieldName=\"" + hcolumn.PropertyBound + "\" Type=\"" + ChildNode.Attributes["EditorType"].Value + "\" ID=\"" + this.ID + "_input_" + cellcounter + "\" Style=\"display:none;position:absolute;left:0px;top:0px;" + AdditionalStyles + "\" />");
                                XmlNode EditorNode = doc.ChildNodes[0];
                                EditorNode.InnerXml = ChildNode.InnerXml;
                                CreateEditorControl(data, EditorNode, EditorContainer, false, cellcounter);
                            }
                            cell.Attributes.Add("cdk", data.DataKey.ToString());
                            cell.Attributes.Add("rdk", ((BLBusinessBase)source).DataKey.ToString());
                            if (!col.IsEditable)
                            {
                                cell.ID = this.ID + "_cell_" + rowcounter + "_" + cellcounter + "_repeatable_" + hcolumn.repeatIndex;
                            }
                            col.InstantiateColumn(cell, data, index);
                            contentRow.Controls.Add(cell);
                        }
                        SetGridLines(xmlgrid, cell);
                    }
                    columnCounter++;
                    if (columnCounter == colnum)
                    {
                        columnCounter = 0;
                        cellcounter++;
                    }
                    index++;

                }
            }
        }


        private object GetProperty(XmlNode node, object data)
        {

            if (node.Attributes["PropertyName"] != null)
            {
                String PropertyName = node.Attributes["PropertyName"].Value;
                PropertyInfo property = data.GetType().GetProperty(PropertyName);
                try
                {
                    if (property != null)
                    {
                        return property.GetValue(data, null);
                    }
                    else
                    {
                        SetError(string.Format(Resources.GenericWebParts.strErrorPropertyNotFound, data.GetType().ToString(), PropertyName));
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                SetError(string.Format(Resources.GenericWebParts.strErrorPropertyNotFoundInLayout, node.Name));
                return null;
            }
        }

        private void CreateCols(Table table, ArrayList cols, ArrayList RepCols, bool isFixedColumn, bool isContent, int rownumber, int cellnumber, bool editionrow)
        {
            ColGroup group = new ColGroup();
            table.Rows.Add(group);
            if (isFixedColumn)
            {
                if (Bands.Count != 0)
                {
                    ColTag select = new ColTag();
                    select.Width = new Unit("25px");
                    group.Cells.Add(select);
                }
                if (this.AutoGenerateSelectButton)
                {
                    ColTag header = new ColTag();
                    header.Width = new Unit("50px");
                    group.Cells.Add(header);
                }
            }
            foreach (ColumnTemplate col in cols)
            {
                XmlNode gNode = col.gNode;
                ColTag tag = new ColTag();
                ApplyStyle(tag, gNode);
                group.Cells.Add(tag);
                if (rownumber != -1 && cellnumber != -1)
                {
                    tag.ID = this.ID + "_coltag_" + rownumber + "_" + cellnumber;
                    cellnumber++;
                }
            }
            if (RepCols != null)
            {
                foreach (XmlNode node in RepCols)
                {
                    IBLListBase source = null;
                    if (m_Datasource != null)
                    {
                        IBLListBase data = (IBLListBase)m_Datasource;
                        if (data.Count != 0)
                        {
                            source = (IBLListBase)GetProperty(node, data[0]);
                            int size = source.Count;
                            if (isContent)
                            {
                                ArrayList nodes = FindNodes(node, "Column");
                                for (int index = 0; index != size; index++)
                                {
                                    foreach (XmlNode contentNode in nodes)
                                    {
                                        ColTag tag = new ColTag();
                                        ApplyStyle(tag, contentNode);
                                        group.Cells.Add(tag);
                                        if (rownumber != -1 && cellnumber != -1)
                                        {
                                            tag.ID = this.ID + "_coltag_" + rownumber + "_" + cellnumber;
                                            cellnumber++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int index = 0; index != size; index++)
                                {
                                    ColTag tag = new ColTag();
                                    ApplyStyle(tag, node);
                                    group.Cells.Add(tag);
                                    tag.ID = this.ID + "_coltag_" + rownumber + "_" + cellnumber;
                                    cellnumber++;
                                }
                            }
                        }
                    }
                }
            }
            if (editionrow)
            {
                ColTag select = new ColTag();
                select.Width = new Unit("25px");
                group.Cells.Add(select);
                ColTag cancelselect = new ColTag();
                cancelselect.Width = new Unit("25px");
                group.Cells.Add(cancelselect);
            }
        }

        private ArrayList FindNodes(XmlNode node, String tagName)
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode childnode in node.ChildNodes)
            {
                if (childnode.Name.ToUpper() == tagName.ToUpper())
                {
                    list.Add(childnode);
                }
            }
            return list;
        }

        private void ApplyStyle(TableCell tag, XmlNode gNode)
        {
            if (gNode.Attributes["Width"] != null)
            {
                tag.Width = new Unit((gNode.Attributes["Width"]).Value);
            }
            if (gNode.Attributes["Height"] != null)
            {
                tag.Height = new Unit((gNode.Attributes["Height"]).Value);
            }
            if (gNode.Attributes["HorizontalAlign"] != null)
            {
                switch ((gNode.Attributes["HorizontalAlign"]).Value.ToUpper())
                {
                    case "CENTER":
                        tag.HorizontalAlign = HorizontalAlign.Center;
                        break;
                    case "LEFT":
                        tag.HorizontalAlign = HorizontalAlign.Left;
                        break;
                    case "RIGHT":
                        tag.HorizontalAlign = HorizontalAlign.Right;
                        break;
                    case "JUSTIFY":
                        tag.HorizontalAlign = HorizontalAlign.Justify;
                        break;
                }
            }
            if (gNode.Attributes["VerticalAlign"] != null)
            {
                switch ((gNode.Attributes["VerticalAlign"]).Value.ToUpper())
                {
                    case "BOTTOM":
                        tag.VerticalAlign = VerticalAlign.Bottom;
                        break;
                    case "MIDDLE":
                        tag.VerticalAlign = VerticalAlign.Middle;
                        break;
                    case "TOP":
                        tag.VerticalAlign = VerticalAlign.Top;
                        break;
                }
            }

            if (gNode.Attributes["Wrap"] != null)
            {
                tag.Wrap = Convert.ToBoolean((gNode.Attributes["Wrap"]).Value);
            }
            if (gNode.Attributes["CssClass"] != null)
            {
                tag.CssClass = (gNode.Attributes["CssClass"]).Value;
            }
            if (gNode.Attributes["SortExpression"] != null)
            {
            }
            if (gNode.Attributes["PercentWidth"] != null)
            {
                tag.Width = new Unit(gNode.Attributes["PercentWidth"].Value);
            }
            else if (gNode.Attributes["FixedWidth"] != null)
            {
                tag.Width = new Unit(gNode.Attributes["FixedWidth"].Value);
            }

            if (gNode.Attributes["IsHidden"] != null)
            {
                tag.Visible = !System.Convert.ToBoolean((gNode.Attributes["IsHidden"]).Value);
            }
        }

        private clsUILayouts getLayouts()
        {
            return clsUILayouts.GetCollection(Type.GetType(ClassName));
        }

        private void CreateBand(XmlNode bandNode, object Datasource, bool IsPostback)
        {
            if (bandNode.Attributes["SelectedType"] != null)
            {
                SelectType = bandNode.Attributes["SelectedType"].Value;
                switch (SelectType.ToUpper())
                {
                    case "NONE":
                        this.AutoGenerateSelectButton = false;
                        break;
                    case "SINGLE":
                        this.AutoGenerateSelectButton = true;              ////////////////////////////////////////
                        break;
                    case "MULTIPLE":
                        upperCheckBox.Visible = true;
                        upperCheckBox.AutoPostBack = true;

                        bottomCheckBox.Visible = true;
                        bottomCheckBox.AutoPostBack = true;
                        upperCheckBox.Text = "Select All";
                        bottomCheckBox.Text = "Select All";
                        this.Columns.Add(new ColumnTemplate(this, bandNode, hash, -1));

                        if (bandNode.Attributes["SelectRowCss"] != null)
                        {
                            //                            template.ItemStyle.CssClass = bandNode.Attributes["SelectRowCss"].Value;      /////////////////////////////
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
                        CreateGridColumn(gNode, Datasource, IsPostback, columns);
                        columns++;
                        break;
                    case "REPEATABLECOLUMNS":
                        RepColumns.Add(gNode);
                        break;
                    case "CHILDBAND":
                        Bands.Add(gNode);
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
        }

        private void CreateGridColumn(XmlNode gNode, object Datasource, bool IsPostback, int columIndex)
        {
            DataControlField ugcol = (DataControlField)new TemplateField();
            if (gNode.Attributes["Content"] != null)
            {
                if (gNode.Attributes["ColumnPropertyType"] != null)
                {
                    if (gNode.Attributes["FixedColumn"] != null)
                    {
                        if (Convert.ToBoolean(gNode.Attributes["FixedColumn"].Value))
                        {
                            this.FixedColums.Add(new ColumnTemplate(this, gNode, columIndex));
                            this.FixedHeaders.Add(new ColumnTemplate(this, gNode, columIndex));
                        }
                        else
                        {
                            this.Columns.Add(new ColumnTemplate(this, gNode, columIndex));
                            this.Headers.Add(new ColumnTemplate(this, gNode, columIndex));
                        }
                    }
                    else
                    {
                        this.Columns.Add(new ColumnTemplate(this, gNode, columIndex));
                        this.Headers.Add(new ColumnTemplate(this, gNode, columIndex));
                    }
                }
            }
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

        protected void dropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.DropDownList drop = (System.Web.UI.WebControls.DropDownList)sender;
            dropdown.SelectedIndex = drop.SelectedIndex;
            bottomDropdown.SelectedIndex = drop.SelectedIndex;
            LayoutName = drop.SelectedValue;

        }

        protected void but_Click(object sender, EventArgs e)
        {
            bottomDropdown.SelectedIndex = dropdown.SelectedIndex;
        }

        protected void BottomBut_Click(object sender, EventArgs e)
        {
            dropdown.SelectedIndex = bottomDropdown.SelectedIndex;
        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
            bool CanUpdate = false;
            EnsureDatasourceObject();
            if ((CommandName.ToLower() == "insert" || CommandName.ToLower() == "update") && m_Datasource != null)
            {
                try
                {
                    foreach (wptHyperGrid band in BandCollection)
                    {
                        band.SaveChanges("update", "");
                    }
                    foreach (HyperRow row in rowCollection.Values)
                    {
                        object Item = row.DataItem;
                        foreach (HyperColumn col in ColumnCollection)
                        {
                            if (col.IsEditable)
                            {
                                string rdk = ((BLBusinessBase)row.DataItem).DataKey.ToString();
                                HiddenField hdf = (HiddenField)HiddenValues[rdk + "_" + col.ColumnIndex.ToString()];
                                String[] propertychain = col.Property.Split(new char[] { '.' });
                                PropertyInfo property = null;
                                String propertyName = col.Property;
                                foreach (String prop in propertychain)
                                {
                                    propertyName = prop;
                                    if (property != null)
                                    {
                                        Item = property.GetValue(Item, null);
                                    }
                                    property = Item.GetType().GetProperty(prop);
                                    if (property == null)
                                    {
                                        SetError(Resources.ErrorMessages.errPropNotFound + prop + Resources.LocalizationUIToolsResource.strIn + row.DataItem.GetType());
                                        return;
                                    }
                                }
                                object Value = null;
                                if (hdf != null)
                                {
                                    if ((col.RepeatableColumn) && (col.RepeatableColumnKey != null))
                                    {
                                        IBLListBase subitems = (IBLListBase)(property.GetValue(Item, null));

                                        string guidProperty = (col.ContentXml.Attributes["KeyProperty"] != null && col.ContentXml.Attributes["KeyProperty"].ToString() != "") ? col.ContentXml.Attributes["KeyProperty"].Value : "DetailDataKey";

                                        BLBusinessBase subdataitem = subitems.Find(guidProperty, col.RepeatableColumnKey);
                                        if (subdataitem != null)
                                        {
                                            CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(subdataitem, "UPDATE", col.PropertyBound);
                                            property = subdataitem.GetType().GetProperty(col.PropertyBound);
                                            try
                                            {
                                                Value = Convert.ChangeType(hdf.Value, property.PropertyType);
                                            }
                                            catch
                                            {
                                                SetError(string.Format(Resources.ErrorMessages.errTryingConvertValueOnSaveChanges, property.Name, Value));
                                                errorthrown = true;
                                            }
                                            CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(Item, "UPDATE", propertyName);
                                            if (CanUpdate && !property.GetValue(subdataitem, null).Equals(Value))
                                            {
                                                property.SetValue(subdataitem, Value, null);
                                            }
                                        }
                                    }
                                    else if (!col.RepeatableColumn)
                                    {
                                        CanUpdate = clsSharedMethods.SharedMethods.CheckAccess(Item, "UPDATE", propertyName);
                                        if (CanUpdate)
                                        {
                                            try
                                            {
                                                Value = Convert.ChangeType(hdf.Value, property.PropertyType);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (hdf.Value == "")
                                                {
                                                    SetError(String.Format(Resources.ErrorMessages.errRequiredValue, col.PropertyFriendlyName));
                                                    errorthrown = true;
                                                }
                                                else
                                                {
                                                    SetError(String.Format(Resources.ErrorMessages.errTryingConvertValueOnSaveChanges, col.PropertyFriendlyName, hdf.Value));
                                                    errorthrown = true;
                                                }
                                            }
                                            if (!property.GetValue(Item, null).Equals(Value))
                                            {
                                                try
                                                {
                                                    property.SetValue(Item, Value, null);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SetError(ex.GetBaseException().Message);
                                                    errorthrown = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (typeof(BLBusinessBase).IsAssignableFrom(Item.GetType()))
                        {
                            BLBusinessBase BL = (BLBusinessBase)Item;
                            if (!BL.IsValid)
                            {
                                System.Text.StringBuilder errors = new System.Text.StringBuilder();
                                foreach (ValidationError bRule in BL.BrokenRulesCollection)
                                {
                                    bool Found = false;
                                    string ValueToReplace = "";
                                    string FinalValue = "";
                                    for (int count = 0; count < ColumnCollection.Count && !Found; count++)
                                    {
                                        HyperColumn col = ColumnCollection[count];
                                        if (col.Property == bRule.ErrorLocation)
                                        {
                                            Found = true;
                                            ValueToReplace = col.PropertyFriendlyName;
                                        }
                                    }
                                    if (Found)
                                    {
                                        FinalValue = bRule.ErrorText.Replace(bRule.ErrorLocation, ValueToReplace);
                                    }
                                    else
                                    {
                                        FinalValue = bRule.ErrorText;
                                    }
                                    errors.AppendLine(FinalValue + "<br/>");
                                }
                                SetError(/*Resources.ErrorMessages.errSavingBOInvalidState + "<br/>" + */errors.ToString());
                                errorthrown = true;
                            }
                        }
                    }
                    if (!errorthrown)
                    {
                        MethodInfo savemethod = m_Datasource.GetType().GetMethod("Save");
                        m_Datasource = savemethod.Invoke(m_Datasource, null);
                        if (m_Parent == null)
                        {
                            ResetDatasource();
                        }
                    }
                    else
                    {
                        ResetDatasource();
                    }

                }
                catch (Exception ex)
                {
                    SetError(ex.GetBaseException().Message);
                    errorthrown = true;
                }
            }

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
                    m_Datasource = GetBusinessClassInstance();
                }
            }
        }

        protected override BLCriteria GetCriteria()
        {
            if (InCriteria == null)
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
                                foreach (System.Xml.XmlNode LayoutNode in FormNode.ChildNodes)
                                {
                                    if (LayoutNode.Name.ToUpper() == "COLLECTIONLAYOUT")
                                    {
                                        if (LayoutNode.ChildNodes.Count > 0)
                                        {
                                            foreach (System.Xml.XmlNode Child in LayoutNode.ChildNodes)
                                            {
                                                if (Child.Name.ToUpper() == "CRITERIA")
                                                {
                                                    Criteria.LoadFromXML(Child, new Hashtable());
                                                    break;
                                                }
                                                else if (Child.Name.ToUpper() == "BAND")
                                                {
                                                    if (Child.Attributes["AllowPaging"] != null)
                                                    {
                                                        if (Convert.ToBoolean(Child.Attributes["AllowPaging"].Value))
                                                        {
                                                            if (Child.Attributes["PageSize"] != null)
                                                            {

                                                                Criteria.PageSize = Convert.ToInt32((Child.Attributes["PageSize"]).Value);
                                                            }
                                                            else
                                                            {
                                                                Criteria.PageSize = 20;
                                                            }
                                                            hash["PageIndex"] = 0;
                                                            Criteria.PageNumber = (int)hash["PageIndex"];
                                                        }
                                                        if (Criteria.OrderByFields.Count == 0)
                                                        {
                                                            if (Child.Attributes["DefaultSortBy"] != null)
                                                            {
                                                                Criteria.AddOrderedField(Child.Attributes["DefaultSortBy"].Value, true);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                InCriteria = Criteria;
            }
            return InCriteria;
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
                }
                hash["Layout"] = dropdown.SelectedValue;
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
            else
            {
                if (Layout != null)
                {
                    return Layout.LayoutXML;
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidLayoutName);
                    return "<FORM><COLLECTIONLAYOUT><BAND></BAND></COLLECTIONLAYOUT></FORM>";
                }
            }
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
                                //obj.MarkAsChild();
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
                            //prop.MarkAsChild();
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
                        //businessObj.MarkAsChild();
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
            try
            {
                HyperColumn sa = SelectedHyperColumn;
                EnsureDatasourceObject();
                String[] enteredArgs = e.CommandArgument.ToString().Split('|');
                int indexRow = Convert.ToInt32(enteredArgs[0]);
                int indexColumn = -1;
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
                if (TargetName == "")
                {
                    IsContext = false;
                }
                else
                {
                    IsContext = true;
                }
                if (IsContext && indexRow == -1)
                {
                    if (this.SelectedRowIndex >= 0 && mainGrid.Rows.Count > 0)
                    {
                        indexRow = this.SelectedRowIndex;
                    }
                    else
                    {
                        //check if a cell is selected and get the row index of the cell.
                        if (this.SelectedCell != "" && rowCollection.Count > 0)
                        {
                            string[] cellPosition = this.SelectedCell.Split(new char[] { ':' });
                            int.TryParse(cellPosition[0], out indexRow);
                            int.TryParse(cellPosition[1], out indexColumn);
                        }
                        else
                        {
                            indexColumn = this.SelectedColumnIndex;
                        }
                    }
                }
                if (CommandName.ToUpper() == "LOCKUNLOCKCOLUMN" && SelectedColumnIndex != -1)
                {
                    HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + SelectedColumnIndex];
                    hvalue.Value = (!Convert.ToBoolean(hvalue.Value)).ToString();

                }
                else if (CommandName.ToUpper() == "UNLOCKCOLUMN" && SelectedColumnIndex != -1)
                {
                    HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + SelectedColumnIndex];
                    hvalue.Value = false.ToString();

                }
                else if (CommandName.ToUpper() == "LOCKCOLUMN" && SelectedColumnIndex != -1)
                {
                    HiddenField hvalue = (HiddenField)LockFieldCollection[this.ID + "_lock_" + SelectedColumnIndex];
                    hvalue.Value = true.ToString();

                }
                else if (CommandName.ToUpper() == "DELETESELECTEDROW" && (SelectedRowIndex >= 0 && SelectedRowIndex < rowCollection.Count))
                {

                    //remove selected row
                    object obj = ((IBLListBase)m_Datasource)[indexRow];
                    MethodInfo DeleteMethod = obj.GetType().GetMethod("Delete");
                    MethodInfo SaveMethod = m_Datasource.GetType().GetMethod("Save");
                    try
                    {
                        DeleteMethod.Invoke(obj, null);
                        m_Datasource = SaveMethod.Invoke(m_Datasource, null);
                    }
                    catch (Exception ex)
                    {
                        SetError(ex.GetBaseException().Message);
                        errorthrown = true;
                    }

                    ResetDatasource();

                }
                else if (CommandName.ToUpper() == "EXCELEXPORT")
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(LayoutTextXML);

                    Type dataSourceType = ReflectionHelper.CreateBusinessType(ClassName);

                    if (datasource.GetType().IsAssignableFrom(dataSourceType))
                    {

                    }
                    else
                    {
                        throw new Exception("Incorrect Datasource Type");
                    }
                }
                else if ((CommandName.ToUpper() == "UPDATE") || (CommandName.ToUpper() == "INSERT"))
                {
                    SaveChanges(CommandName.ToString(), e.CommandArgument.ToString());
                }
                else if (CommandName.ToUpper() == "SORT ASC")
                {
                    if (InCriteria != null)
                    {
                        InCriteria.AddOrderedField(args, true);
                        m_Datasource = GetBusinessClassInstance();
                    }
                }
                else if (CommandName.ToUpper() == "SORT DESC")
                {
                    if (InCriteria != null)
                    {
                        InCriteria.AddOrderedField(args, false);
                        m_Datasource = GetBusinessClassInstance();
                    }
                }
                else if (CommandName.ToUpper() == "SELECTROW")
                {
                    if (indexRow >= 0)
                    {
                        SelectedRow.Value = indexRow.ToString();
                        //hash["SelectedItem"] = indexRow;
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
                else if (CommandName.ToUpper() == "REMOVE")
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
                        if (this.SelectedRowIndex != -1)
                        {
                            indexRow = this.SelectedRowIndex;
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
                    object selectedItem = null;
                    if (SelectedRowIndex > 0)
                    {
                        selectedItem = datasource[SelectedRowIndex];
                    }
                    if (IsContext)
                    {
                        if (TargetName.ToUpper() == "COLUMN")
                        {
                            if (SelectedColumnIndex != -1)
                            {
                                HyperColumn col = SelectedHyperColumn;
                                foreach (object data in datasource)
                                {
                                    if (col.RepeatableColumn)
                                    {
                                        PropertyInfo prop = data.GetType().GetProperty(col.Property);
                                        IBLListBase attribute = (IBLListBase)prop.GetValue(data, null);
                                        string guidProperty = (col.ContentXml.Attributes["KeyProperty"] != null && col.ContentXml.Attributes["KeyProperty"].ToString() != "") ? col.ContentXml.Attributes["KeyProperty"].Value : "DetailDataKey";
                                        BLBusinessBase repetitiveData = attribute.Find(guidProperty, col.RepeatableColumnKey);
                                        if (repetitiveData != null)
                                        {
                                            method = repetitiveData.GetType().GetMethod(CommandName);
                                            source = repetitiveData;
                                        }
                                    }
                                    else
                                    {
                                        method = data.GetType().GetMethod(CommandName);
                                        source = data;
                                    }
                                    try
                                    {
                                        if (method != null)
                                        {
                                            method.Invoke(source, arguments);
                                        }
                                        else
                                        {
                                            if (MethodErrorMessages[CommandName] != null)
                                            {
                                                SetError(MethodErrorMessages[CommandName].ToString());
                                                errorthrown = true;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SetError(ex.GetBaseException().Message);
                                        return;
                                    }
                                }
                                ResetDatasource();
                                return;
                            }
                            else
                            {
                                if (MethodErrorMessages[CommandName] != null)
                                {
                                    SetError(MethodErrorMessages[CommandName].ToString());
                                    errorthrown = true;
                                    return;
                                }
                            }
                        }
                        else if (indexRow == -1 && (TargetName == "" || TargetName.ToUpper() == "GRID"))
                        {
                            method = m_Datasource.GetType().GetMethod(CommandName);
                            source = m_Datasource;
                        }
                        else
                        {
                            if ((TargetName.ToUpper() == "ROW" || TargetName.ToUpper() == "CELL") && indexRow != -1)
                            {
                                if (indexColumn != -1 && TargetName.ToUpper() == "CELL" && SelectedHyperColumn.RepeatableColumn)
                                {
                                    object data = datasource[indexRow];
                                    PropertyInfo prop = data.GetType().GetProperty(SelectedHyperColumn.Property);
                                    IBLListBase attribute = (IBLListBase)prop.GetValue(data, null);
                                    BLBusinessBase repetitiveData = attribute.Find((BLDataKey)SelectedHyperColumn.RepeatableColumnKey);

                                    source = repetitiveData;
                                    method = source.GetType().GetMethod(CommandName);
                                }
                                else
                                {
                                    method = datasource[indexRow].GetType().GetMethod(CommandName);
                                    source = datasource[indexRow];
                                }
                            }
                        }
                    }
                    else
                    {
                        if (arguments != null && arguments.Length == 0)
                        {
                            method = datasource[indexRow].GetType().GetMethod(CommandName, new Type[] { });
                        }
                        else
                        {
                            method = datasource[indexRow].GetType().GetMethod(CommandName);
                        }
                        source = datasource[indexRow];
                    }
                    try
                    {
                        if (method != null)
                        {
                            method.Invoke(source, arguments);
                            ResetDatasource();
                        }
                        else
                        {
                            if (TargetName != "")
                            {
                                bool TargetFound = false;


                                for (int index = 0; index < BandCollection.Count && !TargetFound; index++)
                                {
                                    wptHyperGrid currentBand = BandCollection[index];
                                    if (currentBand.SelectedRowIndex != -1 && TargetName.ToUpper() == "ROW")
                                    {
                                        currentBand.ExecuteCommand(sender, e, true);
                                        TargetFound = true;
                                    }
                                    else if (currentBand.SelectedColumnIndex != -1 && TargetName.ToUpper() == "COLUMN")
                                    {
                                        currentBand.ExecuteCommand(sender, e, true);
                                        TargetFound = true;
                                    }
                                    else if (currentBand.TransferField.Value != "" && TargetName.ToUpper() == "CELL")
                                    {
                                        currentBand.ExecuteCommand(sender, e, true);
                                        TargetFound = true;
                                    }
                                }
                            }
                        }

                        ResetDatasource();
                        if (selectedItem != null)
                        {
                            SelectedRowIndex = datasource.IndexOf(selectedItem);
                        }

                    }
                    catch (Exception exEx)
                    {
                        SetError(exEx.GetBaseException().Message);
                        errorthrown = true;
                    }
                    if (!errorthrown)
                    {
                        ClearError();
                    }
                }
            }
            //General error message
            catch (Exception ex)
            {
                SetError(ex.GetBaseException().Message);
                errorthrown = true;
            }
            if (!errorthrown)
            {
                ClearError();
            }
        }


        public override void ResetDatasource()
        {
            if (m_Parent == null)
            {
                base.ResetDatasource();
            }
            else
            {
                m_Parent.ResetDatasource();
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

        public Object source
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


        #region IPostBackEventHandler Members

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        /// <summary>
        /// Called when a downlevel browser submits the form
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected void RaisePostBackEvent(string eventArg)
        {
            ProcessEvents(eventArg);
        }

        /// <summary>
        /// Called when the TreeView on the client-side submitted the form.
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected bool ProcessEvents(string eventArg)
        {
            if (eventArg == null || eventArg == String.Empty || eventArg == " ") // Don't know why, but the framework is giving a " " eventArg instead of null
                return false;
            bool hasEvents = false;
            String[] events = eventArg.Split(new Char[] { ';' });
            foreach (string strWholeEvent in events)
            {
                String[] parms = strWholeEvent.Split(new Char[] { ',' });
                if (parms[0].Length > 0)
                {
                    if (parms[0].Equals("oncellvaluechanged") && parms.GetLength(0) == 5)
                    {
                        hasEvents = true;
                        DoCellValueChanged(parms[1], parms[2], int.Parse(parms[3]), parms[4]);

                    }
                }
            }
            return hasEvents;
        }

        private void DoCellValueChanged(string rdk, string cdk, int cellindex, object value)
        {
            HyperColumn col = ColumnCollection[cellindex];
            BLBusinessBase rowDatasource = null;
            HyperRow Hrow = (HyperRow)rowCollection[rdk];
            if (Hrow != null)
            {
                rowDatasource = (BLBusinessBase)Hrow.DataItem;
            }
            if (!col.RepeatableColumn)
            {
                if (rowDatasource != null)
                {
                    rowDatasource[col.Property] = value;
                }
            }
            else
            {
            }

            if (m_Datasource != null)
            {
                MethodInfo savemethod = m_Datasource.GetType().GetMethod("Save");
                m_Datasource = savemethod.Invoke(m_Datasource, null);
            }
        }

        #endregion

        #region Editor
        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
        }
        protected void CreateEditorControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton, int ColumnNumber)
        {
            string typeName = "";
            string fieldName = "";
            fieldName = CtrlNode.Attributes["FieldName"].Value;
            if (CtrlNode.Attributes["Type"] != null)
            {
                typeName = CtrlNode.Attributes["Type"].Value;
            }
            string[] PropertyNameAndLayoutName = fieldName.Split(new char[] { '|' });
            fieldName = PropertyNameAndLayoutName[0];
            Type DsType = Datasource.GetType();
            if (fieldName.Contains("."))
            {
                string newFieldName = fieldName.Substring(0, fieldName.IndexOf('.'));
                System.Reflection.PropertyInfo subPropInfo = null;
                try
                {
                    subPropInfo = DsType.GetProperty(newFieldName);
                }
                catch
                {
                    subPropInfo = DsType.GetProperty(newFieldName, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly);
                }
                if (subPropInfo != null)
                {
                    if (clsSharedMethods.SharedMethods.CheckAccess(Datasource, "READ", newFieldName))
                    {
                        object subDatasource = DataBinder.Eval(Datasource, newFieldName);

                        CtrlNode.Attributes["FieldName"].Value = CtrlNode.Attributes["FieldName"].Value.Remove(0, newFieldName.Length + 1);
                        CreateEditorControl(subDatasource, CtrlNode, Container, AsRadioButton, ColumnNumber);
                    }
                    else
                    {
                        SetError(Resources.ErrorMessages.errPermissionsAccessProperty);
                        errorthrown = true;
                    }
                }
                else
                {
                    SetError(Resources.ErrorMessages.errObjectProp);
                    errorthrown = true;
                }
            }
            else
            {
                System.Reflection.PropertyInfo propInfo = DsType.GetProperty(fieldName);
                if (propInfo != null)
                {
                    object BLDatasource = Datasource;

                    if (checkPermitions(fieldName, typeName, ((BLBusinessBase)BLDatasource), propInfo, Container))
                    {
                        bool IsReadable = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "READ", fieldName);
                        bool IsEditable = clsSharedMethods.SharedMethods.CheckAccess(Datasource, "UPDATE", fieldName); //Flag used to check if the property is Editable

                        switch (typeName.ToLower())
                        {
                            case "multiline":
                                CreateHtmlFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, propInfo, Container);

                                break;
                            case "fieldcontrol":
                                CreateFieldControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, AsRadioButton, propInfo, Container);

                                break;
                            case "custom":
                                CreateCustomControl(fieldName, typeName, CtrlNode, BLDatasource, IsReadable, IsEditable, null, propInfo, Container);

                                break;
                            default:
                                SetError(string.Format(Resources.GenericWebParts.strErrorControlTypeNotSupported, typeName));
                                errorthrown = true;
                                break;
                        }
                    }
                    else
                    {
                        SetError(Resources.ErrorMessages.errMissingPermissions);
                        errorthrown = true;
                    }
                }
                else
                {
                    SetError(Resources.ErrorMessages.errInvalidFieldName + " " + fieldName + " in Layout " + LayoutName);
                    errorthrown = true;
                }
            }
        }
        protected override void SetError(string Message)
        {
            if (m_Parent != null)
            {
                m_Parent.SetError(Message);
                m_Parent.errorthrown = true;
            }
            else
            {
                base.SetError(Message);
            }
        }

        protected override void ClearError()
        {
            if (m_Parent != null)
            {
                m_Parent.ClearError();
            }
            else
            {
                base.ClearError();
            }
        }
        #endregion
    }
}
