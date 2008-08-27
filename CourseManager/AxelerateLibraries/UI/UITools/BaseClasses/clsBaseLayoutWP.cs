using System;
using System.Data;
using System.Configuration;
using System.ComponentModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using System.Collections;
using Axelerate.BusinessLayerUITools.Common;
using Axelerate.BusinessLayerUITools.BaseClasses;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using System.Reflection;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;

namespace Axelerate.BusinessLayerUITools
{
    public abstract class clsBaseLayoutWP : clsCtrlWebPartBase
    {
        #region Enum
        public enum BusinessObjectOperations
        {
            CREATE = 0,
            READ = 1,
            UPDATE = 2,
            DELETE = 3
        }
        #endregion

        #region Private Properties

        internal int cont = 0;

        /// <summary>
        /// this is the collection of controls that was created dynamic and must be binded to the datasource. 
        /// </summary>
        protected System.Collections.Hashtable PlaneFieldControls = new System.Collections.Hashtable();

        /// <summary>
        /// this is the collection of complex controls (another editor or grid) that was created dynamic and most be binded to the datasource. 
        /// </summary>
        protected System.Collections.Hashtable ChildEditorControls = new System.Collections.Hashtable();

        /// <summary>
        /// Collection of TabControls that was created dynamic based on the XML Layount information.
        /// </summary>
        protected System.Collections.ArrayList TabControlList = new System.Collections.ArrayList();

        /// <summary>
        /// Collection of XmlNodes used to set the styles of the elements of the tree.
        /// </summary>
        protected System.Collections.Hashtable TreeViewNodeStyles = new System.Collections.Hashtable();

        /// <summary>
        /// Panel to encapsulate the layout and buttons, is like the workspace (without the header) of the control.
        /// </summary>
        private Panel ClipZone = null;

        /// <summary>
        /// panel that allow async refresh, this logic is not suported by sharepoint, at this point.
        /// </summary>
        protected System.Web.UI.UpdatePanel AsyncPanel = null;


        /// <summary>
        /// Image used as link as maximize button for this control if the correct java script is writed by the client.
        /// </summary>
        private Image MaximizeBT = null;

        /// <summary>
        /// Image used as button for collapse(minimize)/restore the control.
        /// </summary>
        private Image MinimizeRestoreBt = null;

        /// <summary>
        /// For show error message to user.
        /// </summary>
        protected Label m_ErrorLabel;


        /// <summary>
        /// Has the collection of layout pages loaded.
        /// </summary>
        protected ArrayList LayoutPages = new ArrayList();

        protected HiddenField m_hdfField = new HiddenField();

        protected Button m_BtnLayoutChanger = new Button();
        /// <summary>
        /// Command event sended when a button is clicked
        /// </summary>
        public event CommandEventHandler CommandEvent;

  //      protected internal object m_Datasource = null;

        protected virtual object m_Datasource
        {
            get
            {
                return ViewState["m_Datasource"];
            }
            set
            {
                ViewState["m_Datasource"] = value;
            }
        }

        private ScriptManager sm;

        private Panel hiddenPanel = null;

        public bool foreignMenu
        {
            get
            {
                object value = ViewState["UseForeingMenu"];
                if (value == null)
                {
                    return false;
                }
                else
                {
                    return (bool)value;
                }
            }
            set
            {
                ViewState["UseForeingMenu"] = value;
            }
        }

        [Category("Layout Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "SelectedLayoutPage")]
        [DefaultValue("")]
        [WebDisplayName("Selected Layout Page:")]
        [WebDescription("Index of the visible layout page.")]
        public int SelectedLayoutPage
        {
            get
            {
                return (ViewState["SelectedLayoutPage"] == null) ? 0 : (int)ViewState["SelectedLayoutPage"];
            }
            set
            {
                if (value < LayoutPages.Count && value >= 0)
                {
                    ViewState["SelectedLayoutPage"] = value;
                }
            }
        }

        [Category("Layout Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LayoutName")]
        [DefaultValue("")]
        [WebDisplayName("Grid Layout:")]
        [WebDescription("The name of the layout that will be displayed.")]
        public virtual string LayoutName
        {
            get
            {
                if (ViewState["XMLLayout"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["XMLLayout"];
                }
            }
            set
            {

                if (ViewState["XMLLayout"] != null)
                {
                    if (ViewState["XMLLayout"].ToString() != value)
                    {
                        ClearLayout();
                    }
                }
                ViewState["XMLLayout"] = value;
            }
        }

        public string ResourcesLibrary
        {
            get
            {
                if (ViewState["ResourcesLibrary"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["ResourcesLibrary"].ToString();
                }
            }
            set
            {
                ViewState["ResourcesLibrary"] = value;
            }
        }

        [ConnectionProvider("Context Provider", "ContextProvider")]
        public virtual clsBaseLayoutWP GetContextProvider()
        {
            return this;
        }

        [WebBrowsable(false), Browsable(false)]
        internal virtual ArrayList ContextMenuLayouts
        {
            get
            {
                if (ViewState["ContextMenu"] == null)
                {
                    ArrayList array = new ArrayList();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(LayoutTextXML);
                    XmlNode root = doc.FirstChild;
                    while (root != null && root.Name.ToUpper() != "FORM")
                    {
                        root = root.NextSibling;
                    }
                    if (root != null)
                    {
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            switch (node.Name.ToUpper())
                            {
                                case "CONTEXTMENU":
                                    array.Add(node.OuterXml);
                                    break;
                            }
                        }
                    }
                    ViewState["ContextMenu"] = array;
                    //return null;
                }
                return (ArrayList)ViewState["ContextMenu"];
            }
            set
            {
                ViewState["ContextMenu"] = value;
            }
        }

        [WebBrowsable(false), Browsable(false)]
        internal virtual bool EnabledCommands
        {
            get
            {
                if (ViewState["EnabledCommands"] == null)
                {
                    ViewState["EnabledCommands"] = true;
                }
                return (bool)ViewState["EnabledCommands"];
            }
            set
            {
                ViewState["EnabledCommands"] = value;
            }
        }

        [Category("Component Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Behavior Type")]
        [DefaultValue("ReadOnly")]
        [WebDisplayName("Behavior Type:")]
        [WebDescription("The behavior of the component: \"ReadOnly\", \"Selector\".")]
        public string ComponentBehaviorType
        {
            get
            {
                if (ViewState["ComponentBehavior"] == null)
                {
                    return "ReadOnly";
                }
                else
                {
                    return (string)ViewState["ComponentBehavior"];
                }
            }
            set
            {

                ViewState["ComponentBehavior"] = value;
            }
        }

        #endregion

        #region Private Methods

        #region "DisplayForm interpreter"



        protected virtual void InitLayount(string DisplayForm, object Datasource, bool IsPostback)
        {

            Table MainContainer = new Table();
            MainContainer.CssClass = this.CssClass;
            Controls.Add(MainContainer);

            MainContainer.Width = new Unit(100, UnitType.Percentage);

            TableRow MainContainerRow = new TableRow();
            TableCell MainContainerCell = new TableCell();
            MainContainer.Rows.Add(MainContainerRow);
            MainContainerRow.Cells.Add(MainContainerCell);

            ClipZone = new Panel(); //<div>
            ClipZone.Width = new Unit(100, UnitType.Percentage);
            //ClipZone.Style.Value = "position:relative";
            ClipZone.CssClass = BodyCssClass;
            if (IsCollapsed)
            {
                ClipZone.Style.Add("display", "none");
            }
            if (ShowHeader)
            {
                Table Header = new Table();
                Header.BorderStyle = BorderStyle.None;
                Header.CellPadding = 0;
                Header.CellSpacing = 0;
                Header.Width = new Unit(100, UnitType.Percentage);
                Header.CssClass = "igwgHdrBlue2k7";
                TableRow HeaderRow = new TableRow();

                //Title of the control
                TableCell HeaderCell = new TableCell();
                HeaderCell.CssClass = "headercell";
                Label HeaderLabel = new Label();
                if (Title != "")
                {
                    if (Datasource != null && typeof(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase).IsAssignableFrom(Datasource.GetType()))
                    {
                        HeaderLabel.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)Datasource, Title, ClassName, ((BLBusinessBase)Datasource).DataKey.ToString());
                    }
                    else
                    {
                        HeaderLabel.Text = Title;
                    }
                }

                if (HeaderCssClass != "")
                {
                    Header.CssClass = HeaderCssClass;
                }
                else
                {
                    HeaderLabel.CssClass = "MIP-GridTitles";
                }

                HeaderCell.Controls.Add(HeaderLabel);

                TableCell HeaderLeftCell = new TableCell();
                HeaderLeftCell.CssClass = "headerleftcell";

                HeaderRow.Cells.Add(HeaderLeftCell);
                HeaderRow.Cells.Add(HeaderCell);

                if (ShowMaximizeButton || ShowMinimizeRestoreButton)
                {
                    //Maximaize button
                    HeaderCell = new TableCell();
                    HeaderCell.CssClass = "headercell";
                    HeaderCell.Width = new Unit(10, UnitType.Pixel);
                    HeaderCell.HorizontalAlign = HorizontalAlign.Center;
                    if (ShowMaximizeButton)
                    {
                        MaximizeBT = new Image();
                        if (this.MaximizeBTImage != "")
                        {
                            MaximizeBT.ImageUrl = this.MaximizeBTImage;
                        }
                        else
                        {
                            if (Page != null) 
                            {
                                string imgUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.igpnl_dwn.gif");
                                MaximizeBT.ImageUrl = imgUrl;
                                MaximizeBT.AlternateText = "Expand";
                            }
                        }

                        MaximizeBT.Attributes["OnClick"] = "javascript:" + OnClientClickMaximizeButton + "(\"maximize\",\"\")";
                        HeaderCell.Controls.Add(MaximizeBT);
                    }
                    if (ShowMinimizeRestoreButton)
                    {
                        MinimizeRestoreBt = new Image();
                        MinimizeRestoreBt.ID = "CollapseExpandButton";
                        if (MinimizeButtonImage != "") 
                        {
                            MinimizeRestoreBt.ImageUrl = MinimizeButtonImage;
                        }
                        else 
                        {
                            string imgUrl = "";
                            if (this.IsCollapsed) 
                            {
                                imgUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.igpnl_dwn.gif");
                                MinimizeRestoreBt.AlternateText = "Expand";
                            }
                            else 
                            {
                                imgUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.igpnl_up.gif");
                                MinimizeRestoreBt.AlternateText = "Collapse";
                            }
                            MinimizeRestoreBt.ImageUrl = imgUrl;
                        }
                        
                        MinimizeRestoreBt.Attributes["OnClick"] = "javascript:BLWPCollapseExpand(\"" + ID + "\",\"\")";
                        HeaderCell.Controls.Add(MinimizeRestoreBt);
                        RegisterCollapseScript();
                    }
                    HeaderRow.Controls.Add(HeaderCell);
                }

                TableCell HeaderRightCell = new TableCell();
                HeaderRightCell.CssClass = "headerrightcell";

                HeaderRow.Cells.Add(HeaderRightCell);

                Header.Controls.Add(HeaderRow);
                MainContainerCell.Controls.Add(Header);

            }
            this.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

            //string DisplayForm = Type.DisplayForm;
            System.Xml.XmlDocument XMLForm = new System.Xml.XmlDocument();
            if (DisplayForm != "")
            {
                XMLForm.LoadXml(DisplayForm);
            }

            if (AsyncUpdates)
            {
                EnsureUpdatePanelFixups();
                AsyncPanel = new UpdatePanel();
                AsyncPanel.UpdateMode = UpdatePanelUpdateMode.Always;
                ClipZone.Controls.Add(AsyncPanel);

                m_hdfField.ID = "hdfLayoutName";
                m_hdfField.Value = LayoutName;
                AsyncPanel.ContentTemplateContainer.Controls.Add(m_hdfField);

                m_BtnLayoutChanger.ID = "hdbLayoutChanger";
                m_BtnLayoutChanger.Style[HtmlTextWriterStyle.Display] = "none";
                AsyncPanel.ContentTemplateContainer.Controls.Add(m_BtnLayoutChanger);
            }
            m_ErrorLabel = new Label();
            m_ErrorLabel.ID = this.ID + "_errormsg";
            if (PlaneFieldControls[m_ErrorLabel.ID] != null)
            {
                m_ErrorLabel = (Label)PlaneFieldControls[m_ErrorLabel.ID];
            }
            else
            {
                m_ErrorLabel.CssClass = "ErrorMessage";
                
                PlaneFieldControls.Add(m_ErrorLabel.ID, m_ErrorLabel);
            }
            ClipZone.Controls.Add(m_ErrorLabel);

            MainContainerCell.Controls.Add(ClipZone);

            if (this.HasDatasource())
            {
                if (AsyncUpdates)
                {
                    CreateEditorControl(XMLForm, Datasource, AsyncPanel.ContentTemplateContainer, IsPostback);
                }
                else
                {
                    CreateEditorControl(XMLForm, Datasource, ClipZone, IsPostback);
                }

                //New: Create one present controls with standar layout
                if (PlaneFieldControls.Count == 0)
                {
                    CrateUnlayoutProperies(Datasource, this, IsPostback);
                }
                //Now we need append the buttons for save and cancel
                CreateButtons(Datasource, this);
            }
        }

        public virtual void CreateEditorControl(System.Xml.XmlDocument XMLForm, object Datasource, Control nContainer, bool IsPostback)
        {
            hiddenPanel = new Panel();
            if (CheckAccess(Datasource, BusinessObjectOperations.READ))
            {
                if (XMLForm.ChildNodes.Count > 0)
                {
                    foreach (XmlNode RootNode in XMLForm.ChildNodes)
                    {
                        if (RootNode.Name.ToUpper() == "FORM")
                        {
                            if (RootNode.Attributes["ResourcesLibrary"] != null)
                            {
                                this.ResourcesLibrary = RootNode.Attributes["ResourcesLibrary"].Value;
                            }
                            System.Xml.XmlNode FormNode = RootNode;
                            foreach (System.Xml.XmlNode LayoutNode in FormNode.ChildNodes)
                            {
                                switch (LayoutNode.Name.ToUpper())
                                {
                                    case "MENU":
                                        if (!foreignMenu)
                                        {
                                            CreateMenu(LayoutNode, nContainer, Datasource, IsPostback, null, -1);
                                        }
                                        break;
                                    case "TOOLBAR":
                                        if (!foreignMenu)
                                        {
                                            CreateToolBar(LayoutNode, nContainer, Datasource, IsPostback, null, -1);
                                        }
                                        break;

                                    default:
                                        ParseLayout(LayoutNode, nContainer, Datasource, IsPostback);
                                        break;


                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Literal strReadDeniedError = new Literal();
                strReadDeniedError.Text = "-";
                nContainer.Controls.Add(strReadDeniedError);
            }
        }

        protected virtual void ParseLayout(XmlNode LayoutNode, Control nContainer, object Datasource, bool IsPostback)
        {
            if (LayoutNode.Name.ToUpper() == "LAYOUT")
            {
                Table Content = new Table();
                Content.Width = new Unit("100%");
                Content.Height = new Unit("100%");
                Content.CellPadding = 0;
                Content.CellSpacing = 0;
                Content.BorderStyle = BorderStyle.None;
                SetAppareance((WebControl)Content, LayoutNode);
                if (LayoutNode.Attributes["ID"] != null)
                {
                    Content.ID = LayoutNode.Attributes["ID"].Value;
                }
                int indx = LayoutPages.Add(Content);
                if (indx != SelectedLayoutPage)
                {
                    Content.Visible = false;
                }
                foreach (System.Xml.XmlNode gNode in LayoutNode.ChildNodes)
                {
                    switch (gNode.Name.ToUpper())
                    {
                        case "COMMAND":
                            CreateCommand(gNode, Datasource, Content, IsPostback, null, -1);
                            break;
                        case "GROUP":
                            TableRow grouprow = new TableRow();
                            TableCell groupcell = new TableCell();
                            grouprow.Cells.Add(groupcell);
                            Content.Rows.Add(grouprow);
                            CreateGroup(gNode, Datasource, groupcell, IsPostback, null, -1);
                            break;
                        case "TABGROUP":
                            TableRow tabrow = new TableRow();
                            TableCell tabcell = new TableCell();
                            tabrow.Cells.Add(tabcell);
                            Content.Rows.Add(tabrow);
                            CreateTabGroup(gNode, Datasource, tabcell, IsPostback, null, -1);
                            break;
                        case "CONTROL":
                            CreateControl(gNode, Content, Datasource, IsPostback);
                            break;
                        case "SPLITTER":
                            TableRow splitterrow = new TableRow();
                            TableCell splittercell = new TableCell();
                            splitterrow.Cells.Add(splittercell);
                            Content.Rows.Add(splitterrow);
                            CreateSplitterControl(gNode, splittercell, Datasource, IsPostback);
                            break;
                        case "LABEL":
                            CreateLabel(gNode, Datasource, Content, IsPostback, null, -1);
                            break;
                        default:
                            break;
                    }
                }
                nContainer.Controls.Add(Content);
            }
        }

        protected virtual void CreateSplitterControl(XmlNode parentNode, Control nContainer, object Datasource, bool IsPostback)
        {
            Panel mainContainer = new Panel();

            WebControls.Splitter splitter = new WebControls.Splitter();
            splitter.ID = "splitterControl" + cont;
            cont++;

            if (parentNode.ChildNodes.Count >= 2)
            {
                XmlNode leftChild = parentNode.ChildNodes[0];
                XmlNode rightChild = parentNode.ChildNodes[1];

                Panel leftPanel = new Panel();
                leftPanel.ID = splitter.ID + "_left";
                leftPanel.Style.Value = "width:100%;height:100%;overflow:auto;padding:0px;margin:0px;";
                if (leftChild.Name == "GROUP")
                {
                    CreateGroup(leftChild, Datasource, leftPanel, IsPostback, null, -1);
                }
                else
                {

                }

                Panel rightPanel = new Panel();
                rightPanel.ID = splitter.ID + "_right";
                rightPanel.Width = new Unit("100%");
                rightPanel.Height = new Unit("100%");
                if (rightChild.Name == "GROUP")
                {
                    CreateGroup(rightChild, Datasource, rightPanel, IsPostback, null, -1);
                }
                else
                {

                }

                splitter.MinWidth = 100;
                splitter.MaxWidth = 0;
                splitter.MinHeight = 100;
                splitter.MaxHeight = 0;
                splitter.BackgroundColor = "lightsteelblue";
                splitter.BackgroundColorLimit = "firebrick";
                splitter.BackgroundColorHilite = "steelblue";
                splitter.BackgroundColorResizing = "purple";

                #region Set splitter's properties
                if (parentNode.Attributes["LeftColumnWidth"] != null)
                {
                    splitter.LeftColumnWidth = parentNode.Attributes["LeftColumnWidth"].Value.ToString();
                }
                if (parentNode.Attributes["Orientation"] != null)
                {
                    if (parentNode.Attributes["Orientation"].Value.ToString().Equals("Horizontal"))
                    {
                        splitter.Orientation = Axelerate.BusinessLayerUITools.WebControls.SplitterBarOrientations.Horizontal;
                    }
                    else
                    {
                        splitter.Orientation = Axelerate.BusinessLayerUITools.WebControls.SplitterBarOrientations.Vertical;
                    }
                }
                if (parentNode.Attributes["MinHeight"] != null)
                {
                    splitter.MinHeight = System.Convert.ToInt32(parentNode.Attributes["MinHeight"].Value.ToString());
                }
                if (parentNode.Attributes["MaxHeight"] != null)
                {
                    splitter.MaxHeight = System.Convert.ToInt32(parentNode.Attributes["MaxHeight"].Value.ToString());
                }
                if (parentNode.Attributes["MinWidth"] != null)
                {
                    splitter.MinWidth = System.Convert.ToInt32(parentNode.Attributes["MinWidth"].Value.ToString());
                }
                if (parentNode.Attributes["MaxWidth"] != null)
                {
                    splitter.MaxWidth = System.Convert.ToInt32(parentNode.Attributes["MaxWidth"].Value.ToString());
                }
                if (parentNode.Attributes["TotalHeight"] != null)
                {
                    splitter.TotalHeight = System.Convert.ToInt32(parentNode.Attributes["TotalHeight"].Value.ToString());
                }
                if (parentNode.Attributes["TotalWidth"] != null)
                {
                    splitter.TotalWidth = System.Convert.ToInt32(parentNode.Attributes["TotalWidth"].Value.ToString());
                }
                if (parentNode.Attributes["SplitterHeight"] != null)
                {
                    splitter.SplitterHeight = System.Convert.ToInt32(parentNode.Attributes["SplitterHeight"].Value.ToString());
                }
                if (parentNode.Attributes["SplitterWidth"] != null)
                {
                    splitter.SplitterWidth = System.Convert.ToInt32(parentNode.Attributes["SplitterWidth"].Value.ToString());
                }
                if (parentNode.Attributes["TopRowHeight"] != null)
                {
                    splitter.TopRowHeight = parentNode.Attributes["TopRowHeight"].Value.ToString();
                }
                if (parentNode.Attributes["SaveHeightToElement"] != null)
                {
                    splitter.SaveHeightToElement = parentNode.Attributes["SaveHeightToElement"].Value.ToString();
                }
                if (parentNode.Attributes["SaveWidthToElement"] != null)
                {
                    splitter.SaveWidthToElement = parentNode.Attributes["SaveWidthToElement"].Value.ToString();
                }
                if (parentNode.Attributes["DynamicResizing"] != null)
                {
                    splitter.DynamicResizing = System.Convert.ToBoolean(parentNode.Attributes["DynamicResizing"].Value.ToString());
                }
                #endregion



                Table mainTable = new Table();
                mainTable.Width = new Unit("100%");
                mainTable.Height = new Unit("100%");
                TableRow row = new TableRow();
                TableCell cell = new TableCell();

                cell.Controls.Add(leftPanel);
                cell.ID = splitter.ID + "_LeftOrTopCell";
                row.Cells.Add(cell);
                mainTable.Rows.Add(row);
                if (splitter.Orientation == Axelerate.BusinessLayerUITools.WebControls.SplitterBarOrientations.Vertical)
                {
                    TableCell midCell = new TableCell();
                    midCell.BackColor = System.Drawing.Color.Red;
                    midCell.Style.Value = "width:6px;background-color:lightsteelblue;";
                    row.Cells.Add(midCell);

                    splitter.LeftResizeTargets = cell.ID + ";" + leftPanel.ID;

                    TableCell rightCell = new TableCell();
                    rightCell.Controls.Add(rightPanel);
                    row.Cells.Add(rightCell);

                }
                else
                {
                    splitter.TopResizeTargets = leftPanel.ID;
                    splitter.BottomResizeTargets = rightPanel.ID;
                    TableRow bottomrow = new TableRow();
                    mainTable.Rows.Add(bottomrow);
                    TableCell bottomCell = new TableCell();
                    bottomCell.Controls.Add(rightPanel);
                    bottomrow.Cells.Add(bottomCell);
                }
                mainContainer.Controls.Add(splitter);
                mainContainer.Controls.Add(mainTable);
            }
            nContainer.Controls.Add(mainContainer);
        }

        protected virtual void CreateButtons(object Datasource, Control nCotainer)
        {

        }

        protected virtual System.Web.UI.WebControls.ImageButton CreateButton(string ID, string Command, string CommandArgument, string Text, bool CausesValidation, Control nContainer)
        {
            System.Web.UI.WebControls.ImageButton NewButton = new ImageButton();
            NewButton.ID = ID;
            NewButton.CommandName = Command;
            NewButton.CommandArgument = CommandArgument;
            NewButton.Height = new Unit("22px");
            NewButton.ImageUrl = this.ImageDir + this.ButtonImage;
            NewButton.Width = new Unit("100px");
            NewButton.CausesValidation = CausesValidation;
            NewButton.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);
            NewButton.BorderColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Transparent);
            NewButton.BorderStyle = BorderStyle.Solid;
            NewButton.BorderWidth = new Unit("1px");


            NewButton.Command += new CommandEventHandler(NewButton_Command);
            nContainer.Controls.Add(NewButton);
            return NewButton;
        }

        public void CreateMenu(XmlNode bandNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostBack, MenuEventHandler Handler, int DatasourceIndex)
        {
            CreateMenu(bandNode, nContainer, Datasource, IsPostBack, Handler, DatasourceIndex, true);
        }

        public void CreateMenu(XmlNode bandNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostBack, MenuEventHandler Handler, int DatasourceIndex, bool enabled)
        {
            Menu menu = new Menu();
            nContainer.Controls.Add(menu);
            SetAppareance(menu, bandNode);
            if (bandNode.Attributes["Orientation"] != null)
            {
                switch (bandNode.Attributes["Orientation"].Value.ToUpper())
                {
                    case "HORIZONTAL":
                        menu.Orientation = Orientation.Horizontal;
                        break;
                    case "VERTICAL":
                        menu.Orientation = Orientation.Vertical;
                        break;
                }
            }
            if (bandNode.Attributes["SubMenuImage"] != null)
            {
                menu.StaticPopOutImageUrl = bandNode.Attributes["SubMenuImage"].Value;
            }
            if (bandNode.Attributes["MenuItemBorderWidth"] != null)
            {

                menu.DynamicMenuStyle.BorderWidth = new Unit(bandNode.Attributes["MenuItemBorderWidth"].Value);
            }
            if (bandNode.Attributes["MenuItemBorderColor"] != null)
            {

                menu.DynamicMenuStyle.BorderColor = System.Drawing.Color.FromName(bandNode.Attributes["MenuItemBorderColor"].Value);
            }
            if (bandNode.Attributes["MenuItemBorderStyle"] != null)
            {
                switch (bandNode.Attributes["MenuItemBorderStyle"].Value.ToLower())
                {
                    case "outset":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Outset;
                        break;
                    case "solid":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Solid;
                        break;
                    case "ridge":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Ridge;
                        break;
                    case "inset":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Inset;
                        break;
                    case "groove":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Groove;
                        break;
                    case "double":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Double;
                        break;
                    case "dotted":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Dotted;
                        break;
                    case "dashed":
                        menu.DynamicMenuStyle.BorderStyle = BorderStyle.Dashed;
                        break;
                }
            }
            if (bandNode.Attributes["HoverMenuItemCss"] != null)
            {
                menu.DynamicHoverStyle.CssClass = bandNode.Attributes["HoverMenuItemCss"].Value;
            }
            if (bandNode.Attributes["HoverCssClass"] != null)
            {
                menu.StaticHoverStyle.CssClass = bandNode.Attributes["HoverCssClass"].Value;
            }
            if (bandNode.Attributes["MenuItemCss"] != null)
            {
                menu.DynamicMenuItemStyle.CssClass = bandNode.Attributes["MenuItemCss"].Value;
            }
            if (bandNode.Attributes["Enabled"] != null)
            {
                menu.Enabled = Convert.ToBoolean(bandNode.Attributes["Enabled"].Value);
            }
            CreateMenuItem(bandNode, menu.Items, DatasourceIndex);
            if (Handler != null)
            {
                menu.MenuItemClick += Handler;
            }
            else
            {
                menu.MenuItemClick += new MenuEventHandler(OnMenuItemClick);
            }
            menu.Enabled = enabled;
        }


        void OnMenuItemClick(object sender, MenuEventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");

            String[] value = e.Item.Value.Split('|');
            String command = value[1];
            String arguments = value[0] + "|";
            if (value.Length >= 3)
            {
                arguments += value[2];
            }
            ExecuteCommand(sender, new CommandEventArgs(command, arguments), false);
        }

        private void CreateMenuItem(XmlNode bandNode, MenuItemCollection nContainer, int DatasourceIndex)
        {
            foreach (System.Xml.XmlNode gNode in bandNode)
            {
                MenuItem item = new MenuItem();
                nContainer.Add(item);
                if (gNode.Attributes["ImageUrl"] != null)
                {
                    item.ImageUrl = gNode.Attributes["ImageUrl"].Value;
                }
                if (gNode.Attributes["Text"] != null)
                {
                    item.Text = gNode.Attributes["Text"].Value;
                }
                if (gNode.Attributes["ToolTip"] != null)
                {
                    item.ToolTip = gNode.Attributes["ToolTip"].Value;
                }
                if (gNode.Attributes["Value"] != null)
                {
                    item.Value = gNode.Attributes["Value"].Value;
                }
                if (gNode.Attributes["SeparatorImageUrl"] != null)
                {
                    item.SeparatorImageUrl = gNode.Attributes["SeparatorImageUrl"].Value;
                }
                if (gNode.Attributes["Selectable"] != null)
                {
                    item.Selectable = Convert.ToBoolean(gNode.Attributes["Selectable"].Value);
                }
                if (gNode.Attributes["PopOutImageUrl"] != null)
                {
                    item.PopOutImageUrl = gNode.Attributes["PopOutImageUrl"].Value;
                }
                if (gNode.Attributes["NavigateUrl"] != null)
                {
                    item.NavigateUrl = gNode.Attributes["NavigateUrl"].Value;
                }
                item.Value = DatasourceIndex + "|";
                if (gNode.Attributes["Command"] != null)
                {
                    item.Value += gNode.Attributes["Command"].Value;
                }
                if (gNode.Attributes["CommandArgument"] != null)
                {
                    item.Value += "|" + gNode.Attributes["CommandArgument"].Value;
                }
                if (gNode.HasChildNodes)
                {
                    CreateMenuItem(gNode, item.ChildItems, DatasourceIndex);
                }
                if (gNode.Attributes["Enabled"] != null)
                {
                    item.Enabled = Convert.ToBoolean(gNode.Attributes["Enabled"].Value);
                }

            }
        }

        public void CreateToolBar(XmlNode bandNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostBack, CommandEventHandler Handler, int DatasourceIndex)
        {
            CreateToolBar(bandNode, nContainer, Datasource, IsPostBack, Handler, DatasourceIndex, true);
        }

        public void CreateToolBar(XmlNode bandNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostBack, CommandEventHandler Handler, int DatasourceIndex, bool enabled)
        {
            Table table = new Table();
            nContainer.Controls.Add(table);
            SetAppareance(table, bandNode);

            if (bandNode.Attributes["Condition"] != null)
            {
                string Condition = bandNode.Attributes["Condition"].Value;
                if (!EvaluateCondition(Datasource, Condition))
                {
                    //If the datasource not meet the condition, then don't create the toolbar.
                    return;
                }
            }

            if (bandNode.Attributes["Padding"] != null)
            {
                table.CellPadding = Convert.ToInt32(bandNode.Attributes["Padding"].Value);
            }
            if (bandNode.Attributes["Margin"] != null)
            {
                table.CellSpacing = Convert.ToInt32(bandNode.Attributes["Margin"].Value);
            }

            foreach (System.Xml.XmlNode gNode in bandNode)
            {
                TableRow row = new TableRow();
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                table.Rows.Add(row);
                SetAppareance(cell, gNode);
                switch (gNode.Name.ToUpper())
                {
                    case "GROUP":
                        CreateGroup(gNode, Datasource, cell, IsPostBack, Handler, DatasourceIndex);
                        break;
                    case "COMMAND":
                        CreateCommand(gNode, Datasource, cell, IsPostBack, Handler, DatasourceIndex);
                        break;
                    case "LABEL":
                        CreateLabel(gNode, Datasource, cell, IsPostback, Handler, DatasourceIndex);
                        break;
                }
                if (gNode.Attributes["Halign"] != null)
                {
                    switch (gNode.Attributes["Halign"].Value.ToUpper())
                    {
                        case "CENTER":
                            cell.HorizontalAlign = HorizontalAlign.Center;
                            break;
                        case "JUSTIFY":
                            cell.HorizontalAlign = HorizontalAlign.Justify;
                            break;
                        case "LEFT":
                            cell.HorizontalAlign = HorizontalAlign.Left;
                            break;
                        case "RIGHT":
                            cell.HorizontalAlign = HorizontalAlign.Right;
                            break;
                    }
                }
                if (bandNode.Attributes["Valign"] != null)
                {
                    switch (bandNode.Attributes["Valign"].Value.ToUpper())
                    {
                        case "MIDDLE":
                            cell.VerticalAlign = VerticalAlign.Middle;
                            break;
                        case "BOTTOM":
                            cell.VerticalAlign = VerticalAlign.Bottom;
                            break;
                        case "TOP":
                            cell.VerticalAlign = VerticalAlign.Top;
                            break;
                    }
                }

            }
            table.Enabled = enabled;
        }

        protected void SetAppareance(WebControl ctl, XmlNode gnode)
        {
            if (gnode.Attributes["CssClass"] != null)
            {
                ctl.CssClass = (gnode.Attributes["CssClass"]).Value;
            }
            if (gnode.Attributes["Style"] != null)
            {
                ctl.Style.Value = (gnode.Attributes["Style"]).Value;
            }
            if (gnode.Attributes["Height"] != null)
            {
                ctl.Height = new Unit((gnode.Attributes["Height"]).Value);
            }
            if (gnode.Attributes["Width"] != null)
            {
                ctl.Width = new Unit((gnode.Attributes["Width"]).Value);
            }
            if (gnode.Attributes["BackColor"] != null)
            {
                ctl.BackColor = System.Drawing.Color.FromName(gnode.Attributes["BackColor"].Value);
            }
            if (gnode.Attributes["BorderColor"] != null)
            {
                ctl.BorderColor = System.Drawing.Color.FromName((gnode.Attributes["BorderColor"]).Value);
            }
            if (gnode.Attributes["BorderStyle"] != null)
            {
                switch (gnode.Attributes["BorderStyle"].Value.ToUpper())
                {
                    case "DASHED":
                        ctl.BorderStyle = BorderStyle.Dashed;
                        break;
                    case "DOTTED":
                        ctl.BorderStyle = BorderStyle.Dotted;
                        break;
                    case "DOUBLE":
                        ctl.BorderStyle = BorderStyle.Double;
                        break;
                    case "GROOVE":
                        ctl.BorderStyle = BorderStyle.Groove;
                        break;
                    case "INSET":
                        ctl.BorderStyle = BorderStyle.Inset;
                        break;
                    case "NONE":
                        ctl.BorderStyle = BorderStyle.None;
                        break;
                    case "OUTSET":
                        ctl.BorderStyle = BorderStyle.Outset;
                        break;
                    case "RIDGE":
                        ctl.BorderStyle = BorderStyle.Ridge;
                        break;
                    case "SOLID":
                        ctl.BorderStyle = BorderStyle.Solid;
                        break;
                }
            }
            if (gnode.Attributes["BorderWidth"] != null)
            {
                ctl.BorderWidth = new Unit((gnode.Attributes["BorderWidth"]).Value);
            }
            if (gnode.Attributes["FontName"] != null)
            {
                ctl.Font.Name = (gnode.Attributes["FontName"]).Value;
            }
            if (gnode.Attributes["FontBold"] != null)
            {
                ctl.Font.Bold = Convert.ToBoolean(gnode.Attributes["FontBold"].Value);
            }
            if (gnode.Attributes["FontItalic"] != null)
            {
                ctl.Font.Italic = Convert.ToBoolean(gnode.Attributes["FontItalic"].Value);
            }
            if (gnode.Attributes["FontOverline"] != null)
            {
                ctl.Font.Overline = Convert.ToBoolean(gnode.Attributes["FontOverline"].Value);
            }
            if (gnode.Attributes["FontSize"] != null)
            {
                ctl.Font.Size = new FontUnit(gnode.Attributes["FontSize"].Value);
            }
            if (gnode.Attributes["FontStrikeout"] != null)
            {
                ctl.Font.Strikeout = Convert.ToBoolean(gnode.Attributes["FontStrikeout"].Value);
            }
            if (gnode.Attributes["FontUnderline"] != null)
            {
                ctl.Font.Underline = Convert.ToBoolean(gnode.Attributes["FontUnderline"].Value);
            }
            if (gnode.Attributes["ForeColor"] != null)
            {
                ctl.ForeColor = System.Drawing.Color.FromName(gnode.Attributes["ForeColor"].Value);
            }
            if (gnode.Attributes["Visible"] != null)
            {
                ctl.Visible = Convert.ToBoolean(gnode.Attributes["Visible"].Value);
            }
        }

        internal virtual IButtonControl CreateCommand(XmlNode gnode, object Datasource, Control container, bool IsPostBack, CommandEventHandler Handler, int DatasourceIndex)
        {
            IButtonControl Command = null;
            switch (gnode.Attributes["Type"].Value.ToUpper())
            {
                case "BUTTON":
                    Command = new Button();
                    break;
                case "LINKBUTTON":
                    Command = new LinkButton();
                    break;
                case "IMAGEBUTTON":
                    Axelerate.BusinessLayerUITools.WebControls.ImageButton imagebutton = new Axelerate.BusinessLayerUITools.WebControls.ImageButton();
                    if (gnode.Attributes["ImageUrl"] != null)
                    {
                        imagebutton.ImageUrl = gnode.Attributes["ImageUrl"].Value;
                    }
                    else
                    {
                        imagebutton.ImageUrl = this.ImageDir + this.ButtonImage;
                    }
                    if (gnode.Attributes["MouseOutImageUrl"] != null)
                    {
                        imagebutton.MouseOutImageUrl = gnode.Attributes["MouseOutImageUrl"].Value;
                    }
                    else
                    {
                        imagebutton.MouseOutImageUrl = this.ImageDir + this.ButtonImage;
                    }
                    if (gnode.Attributes["MouseOverImageUrl"] != null)
                    {
                        imagebutton.MouseOverImageUrl = gnode.Attributes["MouseOverImageUrl"].Value;
                    }
                    else
                    {
                        imagebutton.MouseOverImageUrl = this.ImageDir + this.ButtonHoverImage;
                    }
                    if (gnode.Attributes["PressedImageUrl"] != null)
                    {
                        imagebutton.PressedImageUrl = gnode.Attributes["PressedImageUrl"].Value;
                    }
                    else
                    {
                        imagebutton.PressedImageUrl = this.ImageDir + this.ButtonPressedImage;
                    }
                    if (gnode.Attributes["UseImageButtonComposer"] != null)
                    {
                        imagebutton.UseImageButtonComposer = bool.Parse(gnode.Attributes["UseImageButtonComposer"].Value);
                    }
                    else
                    {
                        imagebutton.UseImageButtonComposer = false;
                    }
                    Command = imagebutton;
                    break;
                case "TWOSTATEIMAGEBUTTON" :
                    Axelerate.BusinessLayerUITools.WebControls.TwoStateImageButton twostateimagebutton = new Axelerate.BusinessLayerUITools.WebControls.TwoStateImageButton();
                    if (gnode.Attributes["ImageUrl"] != null)
                    {
                        twostateimagebutton.ImageUrl = gnode.Attributes["ImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.ImageUrl = this.ImageDir + this.ButtonImage;
                    }
                    if (gnode.Attributes["MouseOutImageUrl"] != null)
                    {
                        twostateimagebutton.MouseOutImageUrl = gnode.Attributes["MouseOutImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.MouseOutImageUrl = this.ImageDir + this.ButtonImage;
                    }
                    if (gnode.Attributes["PressedMouseOutImageUrl"] != null)
                    {
                        twostateimagebutton.PressedMouseOutImageUrl = gnode.Attributes["PressedMouseOutImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.PressedMouseOutImageUrl = this.ImageDir + this.ButtonImage;
                    }
                    if (gnode.Attributes["MouseOverImageUrl"] != null)
                    {
                        twostateimagebutton.MouseOverImageUrl = gnode.Attributes["MouseOverImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.MouseOverImageUrl = this.ImageDir + this.ButtonHoverImage;
                    }
                    if (gnode.Attributes["PressedMouseOverImageUrl"] != null)
                    {
                        twostateimagebutton.MouseOverImageUrl = gnode.Attributes["PressedMouseOverImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.MouseOverImageUrl = this.ImageDir + this.ButtonHoverImage;
                    }
                    if (gnode.Attributes["PressedImageUrl"] != null)
                    {
                        twostateimagebutton.PressedImageUrl = gnode.Attributes["PressedImageUrl"].Value;
                    }
                    else
                    {
                        twostateimagebutton.PressedImageUrl = this.ImageDir + this.ButtonPressedImage;
                    }
                    if (gnode.Attributes["UseImageButtonComposer"] != null)
                    {
                        twostateimagebutton.UseImageButtonComposer = bool.Parse(gnode.Attributes["UseImageButtonComposer"].Value);
                    }
                    else
                    {
                        twostateimagebutton.UseImageButtonComposer = false;
                    }
                    Command = twostateimagebutton;
                    break;
            }
            if (Command != null)
            {
                SetAppareance(((WebControl)Command), gnode);
                setButtonProperties(gnode, Command, Datasource, Handler, DatasourceIndex);
                container.Controls.Add((Control)Command);
            }
            return Command;
        }

        internal void CreateLabel(XmlNode gnode, object Datasource, Control container, bool IsPostBack, CommandEventHandler Handler, int DatasourceIndex)
        {
            Label lbl = new Label();
            if(gnode.Attributes["Text"] != null)
            {
                lbl.Text = gnode.Attributes["Text"].Value;
            }
            SetAppareance(lbl, gnode);
            container.Controls.Add(lbl);
        }

        private void setButtonProperties(XmlNode gnode, IButtonControl button, object Datasource, CommandEventHandler Handler, int DatasourceIndex)
        {
            BLBusinessBase businessObj = null;

            if (gnode.Attributes["Text"] != null)
            {
                button.Text = clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["Text"].Value);
                if (Datasource != null)
                {
                    string locatedText = clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["Text"].Value);
                    if (typeof(BLBusinessBase).IsAssignableFrom(Datasource.GetType()))
                    {
                        button.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString((BLBusinessBase)Datasource, locatedText, ((BLBusinessBase)Datasource).GetType().AssemblyQualifiedName, "");
                    }
                    else if (typeof(IBLListBase).IsAssignableFrom(Datasource.GetType()) && DatasourceIndex != -1)
                    {
                        button.Text = clsSharedMethods.SharedMethods.parseBOPropertiesString(((BLBusinessBase)((IBLListBase)Datasource)[DatasourceIndex]), locatedText, ((IBLListBase)Datasource)[DatasourceIndex].GetType().AssemblyQualifiedName, "");
                    }
                }
            }
            if (gnode.Attributes["PostBackUrl"] != null)
            {
                button.PostBackUrl = gnode.Attributes["PostBackUrl"].Value;
            }
            if (gnode.Attributes["CausesValidation"] != null)
            {
                button.CausesValidation = Convert.ToBoolean(gnode.Attributes["CausesValidation"].Value);
            }
            else
            {
                button.CausesValidation = false;
            }
            if (gnode.Attributes["Command"] != null)
            {
                //this.GetType().GetMethod(gnode.Attributes["Command"].Value)
                button.CommandName = gnode.Attributes["Command"].Value;
            }
            button.CommandArgument = DatasourceIndex + " |";
            if (gnode.Attributes["CommandArgument"] != null)
            {
                button.CommandArgument += gnode.Attributes["CommandArgument"].Value;
            }
            if (gnode.Attributes["Target"] != null)
            {
                button.CommandName += "|" + gnode.Attributes["Target"].Value;
            }
            if (gnode.Attributes["RedirectTo"] != null)
            {
                if (gnode.Attributes["Target"] == null || gnode.Attributes["Target"].Value.Trim() == "")
                {
                    button.CommandName += "| ";
                }
                button.CommandName += "|" + gnode.Attributes["RedirectTo"].Value;
            }
            if (gnode.Attributes["AlertMessage"] != null)
            {
                if (gnode.Attributes["RedirectTo"] == null || gnode.Attributes["RedirectTo"].Value.Trim() == "")
                {
                    if (gnode.Attributes["Target"] == null || gnode.Attributes["Target"].Value.Trim() == "")
                    {
                        button.CommandName += "| ";
                    }
                    button.CommandName += "| ";
                }
                button.CommandName += "|" + gnode.Attributes["AlertMessage"].Value;
            }
            if (gnode.Attributes["ConfirmMessage"] != null)
            {
                if (button is Button)
                {
                    ((Button)button).OnClientClick = "javascript:return confirm('" + clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["ConfirmMessage"].Value) + "');";
                }
                else if (button is LinkButton)
                {
                    ((LinkButton)button).OnClientClick = "javascript:return confirm('" + clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["ConfirmMessage"].Value) + "');";
                }
                else if (button is ImageButton)
                {
                    ((ImageButton)button).OnClientClick = "javascript:return confirm('" + clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["ConfirmMessage"].Value) + "');";
                }
                else if (button is Axelerate.BusinessLayerUITools.WebControls.ImageButton)
                {
                    ((Axelerate.BusinessLayerUITools.WebControls.ImageButton)button).OnClientClick = "javascript:return confirm('" + clsSharedMethods.SharedMethods.getStringFromResources(Datasource, gnode.Attributes["ConfirmMessage"].Value) + "');";
                }
            }
            if (gnode.Attributes["OnClientClick"] != null)
            {
                if (button is Button)
                {
                    ((Button)button).OnClientClick += "javascript:" + gnode.Attributes["OnClientClick"].Value + ";";
                }
                else if (button is LinkButton)
                {
                    ((LinkButton)button).OnClientClick += "javascript:" + gnode.Attributes["OnClientClick"].Value + ";";
                }
                else if (button is ImageButton)
                {
                    ((ImageButton)button).OnClientClick += "javascript:" + gnode.Attributes["OnClientClick"].Value + ";";
                }
                else if (button is Axelerate.BusinessLayerUITools.WebControls.ImageButton)
                {
                    ((Axelerate.BusinessLayerUITools.WebControls.ImageButton)button).OnClientClick += "javascript:" + gnode.Attributes["OnClientClick"].Value + ";";
                }
            }
            if (Handler != null)
            {
                button.Command += Handler;
            }
            else
            {
                if (DatasourceIndex != -1)
                {
                    button.Command += new CommandEventHandler(NewButton_Command);
                }
                else
                {
                    button.Command += new CommandEventHandler(context_Command);
                }
            }
        }

        void context_Command(object sender, CommandEventArgs e)
        {
            ExecuteCommand(sender, e, true);
        }

        protected virtual void CreateGroup(System.Xml.XmlNode GroupNode, object Datasource, Control nCotainer, bool IsPostback, CommandEventHandler Handler, int DatasourceIndex)
        {
            Control GroupContainer = null;
            if (GroupNode.Attributes.Count > 0 && (GroupNode.Attributes["Label"] != null))
            {
                string GroupName = (string)GroupNode.Attributes["Label"].Value;
                Panel Group = new Panel();
                Group.GroupingText = GroupName;
                //SetAppareance(Group, GroupNode);
                GroupContainer = Group;
                nCotainer.Controls.Add(GroupContainer);

            }
            else
            {
                //Panel Group = new Panel();
                //Group.Width = new Unit(100, UnitType.Percentage);
                //Group.BackColor = System.Drawing.Color.White;
                GroupContainer = nCotainer;
            }
            SetAppareance((System.Web.UI.WebControls.WebControl)GroupContainer, GroupNode);

            Table Tbl = new Table();

            Tbl.CellPadding = 0;
            Tbl.CellSpacing = 0;
            if (GroupNode.Attributes["CellPadding"] != null)
            {
                int CellPadding = 0;
                if (int.TryParse(GroupNode.Attributes["CellPadding"].Value, out CellPadding))
                {
                    Tbl.CellPadding = CellPadding;
                }
            }
            if (GroupNode.Attributes["CellSpacing"] != null)
            {
                int CellSpacing = 0;
                if (int.TryParse(GroupNode.Attributes["CellSpacing"].Value, out CellSpacing))
                {
                    Tbl.CellSpacing = CellSpacing;
                }

            }

            Tbl.BorderWidth = new Unit(0, UnitType.Pixel);
            TableRow NewRow = new TableRow();
            Tbl.Width = new Unit(100, UnitType.Percentage);
            Tbl.Height= new Unit(100, UnitType.Percentage);

            foreach (System.Xml.XmlNode Col in GroupNode.ChildNodes)
            {
                if (Col.Name.ToUpper() == "COLUMN")
                {
                    TableCell NewCell = new TableCell();
                    SetAppareance(NewCell, Col);
                    /*
                    string width = Col.Attributes["PercentWidth"].Value;
                    NewCell.Width = new Unit(width + "%");
                    */
                    if (Col.Attributes["HorizontalAlign"] != null)
                    {
                        switch (Col.Attributes["HorizontalAlign"].Value.ToLower())
                        {
                            case "center":
                                NewCell.HorizontalAlign = HorizontalAlign.Center;
                                break;
                            case "justify":
                                NewCell.HorizontalAlign = HorizontalAlign.Justify;
                                break;
                            case "left":
                                NewCell.HorizontalAlign = HorizontalAlign.Left;
                                break;
                            case "right":
                                NewCell.HorizontalAlign = HorizontalAlign.Right;
                                break;
                        }
                    }
                    Table ContainerTable = new Table();
                    ContainerTable.CellPadding = 0;
                    ContainerTable.CellSpacing = 0;

                    ContainerTable.BorderWidth = new Unit(0, UnitType.Pixel);
                    ContainerTable.Width = new Unit(100, UnitType.Percentage);
                    foreach (System.Xml.XmlNode InnerNode in Col.ChildNodes)
                    {
                        switch (InnerNode.Name.ToLower())
                        {
                            case "command":
                                CreateCommand(InnerNode, Datasource, NewCell, IsPostback, Handler, DatasourceIndex);
                                break;
                            case "control":
                                CreateControl(InnerNode, ContainerTable, Datasource, IsPostback);
                                break;
                            case "group":
                            case "tabgroup":
                                TableCell InnerGroup = new TableCell();
                                InnerGroup.Width = new Unit(100, UnitType.Percentage);
                                if (ContainerTable.Rows.Count > 0)
                                {
                                    if (ContainerTable.Rows[0].Cells.Count > 1)
                                    {
                                        InnerGroup.ColumnSpan = ContainerTable.Rows[0].Cells.Count;
                                    }
                                    else
                                    {
                                        InnerGroup.ColumnSpan = ContainerTable.Rows[0].Cells[0].ColumnSpan;
                                    }
                                }
                                if (InnerNode.Name.ToLower() == "group")
                                {
                                    CreateGroup(InnerNode, Datasource, InnerGroup, IsPostback, Handler, DatasourceIndex);
                                }
                                else
                                {
                                    CreateTabGroup(InnerNode, Datasource, InnerGroup, IsPostback, Handler, DatasourceIndex);
                                }
                                TableRow InnerGroupRow = new TableRow();
                                InnerGroupRow.Cells.Add(InnerGroup);
                                ContainerTable.Rows.Add(InnerGroupRow);
                                break;
                            case "splitter":
                                TableRow splitterrow = new TableRow();
                                TableCell splittercell = new TableCell();
                                splitterrow.Cells.Add(splittercell);
                                ContainerTable.Rows.Add(splitterrow);
                                CreateSplitterControl(InnerNode, splittercell, Datasource, IsPostback);
                                break;
                            case "menu":
                                CreateMenu(InnerNode, NewCell, Datasource, IsPostback, null, DatasourceIndex);
                                break;
                            case "label":
                                CreateLabel(InnerNode, Datasource, NewCell, IsPostback, Handler, DatasourceIndex);
                                break;
                        }
                    }
                    NewCell.Controls.Add(ContainerTable);
                    NewRow.Cells.Add(NewCell);

                }
            }
            if (NewRow.Cells.Count > 0)
            {
                Tbl.Rows.Add(NewRow);
            }
            GroupContainer.Controls.Add(Tbl);

        }

        protected virtual void CreateControl(System.Xml.XmlNode CtrlNode, Table Container, object Datasource, bool IsPostback)
        {
            string tpy = (CtrlNode.Attributes["Type"] == null) ? string.Empty : CtrlNode.Attributes["Type"].Value;
            string fieldname = (CtrlNode.Attributes["FieldName"] == null) ? string.Empty : CtrlNode.Attributes["FieldName"].Value;
            string LabelText = (CtrlNode.Attributes["Label"] == null) ? string.Empty : CtrlNode.Attributes["Label"].Value;
            string LabelPosition = (CtrlNode.Attributes["LabelPosition"] == null) ? string.Empty : CtrlNode.Attributes["LabelPosition"].Value;
            string LabelCss = (CtrlNode.Attributes["LabelCss"] == null) ? string.Empty : CtrlNode.Attributes["LabelCss"].Value;
            bool AsRadioButton = (CtrlNode.Attributes["AsRadioButton"] == null) ? false : Convert.ToBoolean(CtrlNode.Attributes["AsRadioButton"].Value);
            TableCell NewLabelCell = new TableCell();
            TableCell NewControlCell = new TableCell();
            NewControlCell.Style.Add(HtmlTextWriterStyle.Padding, "4px 4px 4px 4px");
            Label lbl = new Label();
            lbl.CssClass = "MIP-Small";
            if (LabelText != "")
            {
                NewLabelCell.Wrap = false;
                NewLabelCell.Style.Add(HtmlTextWriterStyle.Padding, "4px 4px 4px 4px");
                System.Text.RegularExpressions.Regex AccessKeyLocator = new System.Text.RegularExpressions.Regex(@"&(?<letter>[\w])");
                if (AccessKeyLocator.IsMatch(LabelText))
                {
                    lbl.AccessKey = AccessKeyLocator.Match(LabelText).Groups["letter"].Value;
                    lbl.Text = "<NOBR>" + AccessKeyLocator.Replace(LabelText, "<u>" + AccessKeyLocator.Match(LabelText).Groups["letter"].Value + "</u>") + "</NOBR>";
                }
                else
                {
                    lbl.Text = "<NOBR>" + LabelText.Replace("&", "") + "</NOBR>";
                }
                lbl.CssClass = LabelCss;

                NewLabelCell.Controls.Add(lbl);
            }

            if (CtrlNode.Attributes["Align"] != null)
            {
                switch (CtrlNode.Attributes["Align"].Value.ToUpper())
                {
                    case "CENTER":
                        NewControlCell.HorizontalAlign = HorizontalAlign.Center;
                        break;
                    case "LEFT":
                        NewControlCell.HorizontalAlign = HorizontalAlign.Left;
                        break;
                    case "RIGHT":
                        NewControlCell.HorizontalAlign = HorizontalAlign.Right;
                        break;
                    case "JUSTIFY":
                        NewControlCell.HorizontalAlign = HorizontalAlign.Justify;
                        break;
                    default:
                        break;
                }
            }
            CreateInnerControl(Datasource, CtrlNode, NewControlCell, AsRadioButton);

            TableRow NewRow = new TableRow();
            if (LabelPosition != "")
            {
                switch (LabelPosition.ToLower())
                {
                    case "top":
                        NewRow.Cells.Add(NewLabelCell);
                        Container.Rows.Add(NewRow);
                        NewRow = new TableRow();
                        NewRow.Cells.Add(NewControlCell);
                        Container.Rows.Add(NewRow);
                        break;
                    case "botton":
                        NewRow.Cells.Add(NewControlCell);
                        Container.Rows.Add(NewRow);
                        NewRow = new TableRow();
                        NewRow.Cells.Add(NewLabelCell);
                        Container.Rows.Add(NewRow);
                        break;
                    case "left":
                        if (LabelText != "")
                        {
                            NewLabelCell.Width = new Unit(25, UnitType.Pixel);
                        }
                        else
                        {
                            NewLabelCell.Width = new Unit(0, UnitType.Pixel);
                        }
                        NewRow.Cells.Add(NewLabelCell);
                        NewRow.Cells.Add(NewControlCell);
                        Container.Rows.Add(NewRow);
                        break;
                    case "right":
                        if (LabelText != "")
                        {
                            NewLabelCell.Width = new Unit(25, UnitType.Pixel);
                        }
                        else
                        {
                            NewLabelCell.Width = new Unit(0, UnitType.Pixel);
                        }
                        NewRow.Cells.Add(NewControlCell);
                        NewRow.Cells.Add(NewLabelCell);
                        Container.Rows.Add(NewRow);
                        break;
                }
            }
            else
            {
                NewLabelCell.Width = new Unit(0, UnitType.Pixel);
                NewRow.Cells.Add(NewLabelCell);
                NewRow.Cells.Add(NewControlCell);
                Container.Rows.Add(NewRow);
            }
        }

        /// <summary>
        /// This function create the control specific based on the datasource, type, field and add it to the Container
        /// </summary>
        /// <param name="Datasource"></param>
        /// <param name="typeName"></param>
        /// <param name="fieldName"></param>
        /// <param name="Container"></param>
        protected abstract void CreateInnerControl(object Datasource, XmlNode CtrlNode, Control Container, bool AsRadioButton);

        protected virtual void CreateTabGroup(System.Xml.XmlNode TabGroup, object Datasource, Control nContainer, bool IsPostback, CommandEventHandler Handler, int DatasourceIndex)
        {
            AjaxControlToolkit.TabContainer tabContainer = new AjaxControlToolkit.TabContainer();
            if (TabGroup.Attributes["DoPostBack"] != null)
            {
                tabContainer.AutoPostBack = System.Convert.ToBoolean(TabGroup.Attributes["DoPostBack"].Value);
                tabContainer.ActiveTabChanged += new EventHandler(tabContainer_ActiveTabChanged);
            }

            //AjaxControlToolkit.TabPanel tabpan = new AjaxControlToolkit.TabPanel();

            SetAppareance(tabContainer, TabGroup);

            //Infragistics.WebUI.UltraWebTab.UltraWebTab TabControl = new Infragistics.WebUI.UltraWebTab.UltraWebTab();
            /*TabControl.CssClass = "igwtMainBlue2k7";
            TabControl.EnableViewState = true;
            

            TabControl.HoverTabStyle.CssClass = "igwtTabHoverBlue2k7";
            TabControl.DefaultTabStyle.CssClass = "igwtTabNormalBlue2k7";
            TabControl.RoundedImage.FillStyle = Infragistics.WebUI.UltraWebTab.RoundedImageStyle.LeftMergedWithCenter;
            TabControl.RoundedImage.HoverImage = "igwt_tab_hover.jpg";
            TabControl.RoundedImage.LeftSideWidth = 14;
            TabControl.RoundedImage.NormalImage = "none";
            TabControl.RoundedImage.RightSideWidth = 14;
            TabControl.RoundedImage.SelectedImage = "igwt_tab_selected.jpg";
            TabControl.SelectedTabStyle.CssClass = "igwtTabSelectedBlue2k7";*/

            foreach (System.Xml.XmlNode TabPageNode in TabGroup.ChildNodes)
            {
                if (TabPageNode.Name.ToLower() == "tab")
                {
                    string Label = TabPageNode.Attributes["Label"].Value;
                    AjaxControlToolkit.TabPanel NewTab = new AjaxControlToolkit.TabPanel();
                    SetAppareance(NewTab, TabPageNode);
                    NewTab.HeaderText = Label;

                    Table ContainerTable = new Table();
                    ContainerTable.CellPadding = 0;
                    ContainerTable.CellSpacing = 0;
                    ContainerTable.BorderWidth = new Unit(0, UnitType.Pixel);
                    ContainerTable.Width = new Unit(100, UnitType.Percentage);
                    foreach (System.Xml.XmlNode InnerNode in TabPageNode.ChildNodes)
                    {
                        switch (InnerNode.Name.ToLower())
                        {
                            case "command":
                                TableRow row = new TableRow();
                                ContainerTable.Rows.Add(row);
                                TableCell cell = new TableCell();
                                row.Cells.Add(cell);
                                CreateCommand(InnerNode, Datasource, cell, IsPostback, null, -1);
                                break;
                            case "control":
                                CreateControl(InnerNode, ContainerTable, Datasource, IsPostback);
                                break;
                            case "group":
                            case "tabgroup":
                                TableCell InnerGroup = new TableCell();
                                InnerGroup.Width = new Unit(100, UnitType.Percentage);
                                if (ContainerTable.Rows.Count > 0)
                                {
                                    if (ContainerTable.Rows[0].Cells.Count > 1)
                                    {
                                        InnerGroup.ColumnSpan = ContainerTable.Rows[0].Cells.Count;
                                    }
                                    else
                                    {
                                        InnerGroup.ColumnSpan = ContainerTable.Rows[0].Cells[0].ColumnSpan;
                                    }
                                }
                                if (InnerNode.Name.ToLower() == "group")
                                {
                                    CreateGroup(InnerNode, Datasource, InnerGroup, IsPostback, Handler, DatasourceIndex);
                                }
                                else
                                {
                                    CreateTabGroup(InnerNode, Datasource, InnerGroup, IsPostback, Handler, DatasourceIndex);
                                }
                                TableRow InnerGroupRow = new TableRow();
                                InnerGroupRow.Cells.Add(InnerGroup);
                                ContainerTable.Rows.Add(InnerGroupRow);
                                break;
                        }
                    }
                    NewTab.Controls.Add(ContainerTable);
                    tabContainer.Tabs.Add(NewTab);
                }
            }
            nContainer.Controls.Add(tabContainer);
            TabControlList.Add(tabContainer);

        }

        internal void tabContainer_ActiveTabChanged(object sender, EventArgs e)
        {

        }
        protected virtual void CreateHtmlEditorControl(string FieldName, object Datasource, Control nContainer)
        {
            System.Web.UI.WebControls.TextBox HtmlEditor = new TextBox();
            //Infragistics.WebUI.WebHtmlEditor.WebHtmlEditor HtmlEditor = new Infragistics.WebUI.WebHtmlEditor.WebHtmlEditor();
            //HtmlEditor.BackgroundImageName = "";
            //HtmlEditor.FontFormattingList = "Heading 1=<h1>&Heading 2=<h2>&Heading 3=<h3>&Heading 4=<h4>&Heading 5=<h5>&Normal=<p>";
            HtmlEditor.Font.Names = new string[] { "Arial", "Verdana", "Tahoma", "Courier New", "Georgia" };
            //HtmlEditor.FontSizeList = new Infragistics.WebUI.WebHtmlEditor.StringArrayList("1,2,3,4,5,6,7");
            //HtmlEditor.FontStyleList = "Blue Underline=color:blue;text-decoration:underline;&Red Bold=color:red;font-weight:bold;&ALL CAPS=text-transform:uppercase;&all lowercase=text-transform:lowercase;&Reset=";
            HtmlEditor.Height = new Unit(160, UnitType.Pixel);
            //HtmlEditor.SpecialCharacterList = "&#937;,&#931;,&#916;,&#934;,&#915;,&#936;,&#928;,&#920;,&#926;,&#923;,&#958;,&#956;,&#951;,&#966;,&#969;,&#949;,&#952;,&#948;,&#950;,&#968;,&#946;,&#960;,&#963;,&szlig;,&thorn;,&THORN;,&#402,&#1046;,&#1064;,&#1070;,&#1071;,&#1078;,&#1092;,&#1096;,&#1102;,&#1103;,&#12362;,&#12354;,&#32117;,&AElig;,&Aring;,&Ccedil;,&ETH;,&Ntilde;,&Ouml;,&aelig;,&aring;,&atilde;,&ccedil;,&eth;,&euml;,&ntilde;,&cent;,&pound;,&curren;,&yen;,&#8470;,&#153;,&copy;,&reg;,&#151;,@,&#149;,&iexcl;,&#14;,&#8592;,&#8593;,&#8594;,&#8595;,&#8596;,&#8597;,&#8598;,&#8599;,&#8600;,&#8601;,&#18;,&brvbar;,&sect;,&uml;,&ordf;,&not;,&macr;,&para;,&deg;,&plusmn;,&laquo;,&raquo;,&middot;,&cedil;,&ordm;,&sup1;,&sup2;,&sup3;,&frac14;,&frac12;,&frac34;,&iquest;,&times;,&divide;";
            HtmlEditor.Width = new Unit(100, UnitType.Percentage);
            HtmlEditor.Font.Bold = false;
            HtmlEditor.Font.Italic = false;
            HtmlEditor.Font.Overline = false;
            HtmlEditor.Font.Strikeout = false;
            HtmlEditor.Font.Underline = false;
            /*HtmlEditor.Toolbar.Items.Clear();
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.DoubleSeparator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Bold));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Italic));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Underline));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Cut));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Copy));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Paste));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Undo));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Redo));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.JustifyLeft));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.JustifyCenter));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.JustifyRight));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Indent));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.Outdent));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.UnorderedList));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.OrderedList));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarDialogButton(Infragistics.WebUI.WebHtmlEditor.ToolbarDialogButtonType.FontColor, HtmlEditor));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarDialogButton(Infragistics.WebUI.WebHtmlEditor.ToolbarDialogButtonType.FontHighlight, HtmlEditor));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.Separator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.InsertLink));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarButton(Infragistics.WebUI.WebHtmlEditor.ToolbarButtonType.RemoveLink));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.RowSeparator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarImage(Infragistics.WebUI.WebHtmlEditor.ToolbarImageType.DoubleSeparator));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarDropDown(Infragistics.WebUI.WebHtmlEditor.ToolbarDropDownType.FontName));
            HtmlEditor.Toolbar.Items.Add(new Infragistics.WebUI.WebHtmlEditor.ToolbarDropDown(Infragistics.WebUI.WebHtmlEditor.ToolbarDropDownType.FontSize));*/
            /*HtmlEditor.RightClickMenu.Items.Clear();
            HtmlEditor.RightClickMenu.Items.Add(new Infragistics.WebUI.WebHtmlEditor.HtmlBoxMenuItem(Infragistics.WebUI.WebHtmlEditor.ActionType.Cut));
            HtmlEditor.RightClickMenu.Items.Add(new Infragistics.WebUI.WebHtmlEditor.HtmlBoxMenuItem(Infragistics.WebUI.WebHtmlEditor.ActionType.Copy));
            HtmlEditor.RightClickMenu.Items.Add(new Infragistics.WebUI.WebHtmlEditor.HtmlBoxMenuItem(Infragistics.WebUI.WebHtmlEditor.ActionType.Paste));
            HtmlEditor.RightClickMenu.Items.Add(new Infragistics.WebUI.WebHtmlEditor.HtmlBoxMenuItem(Infragistics.WebUI.WebHtmlEditor.ActionType.PasteHtml));*/

            //WorkItem WItem = (WorkItem)Datasource;
            /*clsProperty Fld = null;
            clsPropertyValue pValue = null;
            if (FieldName != "")
            {
                Fld = (clsProperty)m_CustomProperties.Find("Name", FieldName);
                pValue = GetPropertyValue(Fld, (clsPropertyValues)Datasource);
            }

            if (pValue.PropertyValue != "")
            {
                HtmlEditor.Text = pValue.PropertyValue;
            }
            else
            {
                HtmlEditor.Text = Fld.DefaultValue;
            }*/
            HtmlEditor.Text = GetEditorFieldValue(Datasource, FieldName).ToString();
            nContainer.Controls.Add(HtmlEditor);
            PlaneFieldControls[FieldName] = HtmlEditor;
        }

        protected virtual object GetEditorFieldValue(object Datasource, string FieldName)
        {
            return DataBinder.Eval(Datasource, FieldName);
        }

        /// <summary>
        /// Create controls and layout for all the properies that not come in the XML layout definition (DisplayForm variable).
        /// </summary>
        /// <param name="Datasource">Datasource for new controls</param>
        /// <param name="nContainer">container for the new controls</param>
        protected virtual void CrateUnlayoutProperies(object Datasource, Control nContainer, bool IsPostback)
        {
            //Example:
            //Create a XML Node that agroup all this properties
            /* System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
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
             CreateGroup(UnlayoutProperiesGroup, Datasource, nContainer, IsPostback);*/

        }

        /// <summary>
        /// Create a Infragistics Web UltraWebGrid with all the default DisplayLayout settings.
        /// </summary>
        /// <returns></returns>
        protected virtual System.Web.UI.WebControls.GridView CreateUltraWebGrid()
        {
            GridView grd = new GridView();
            //Infragistics.WebUI.UltraWebGrid.UltraWebGrid grd = new Infragistics.WebUI.UltraWebGrid.UltraWebGrid();
            grd.Width = new Unit(100, UnitType.Percentage);
            grd.Height = new Unit(160, UnitType.Pixel);
            grd.AutoGenerateColumns = false;
            grd.GridLines = GridLines.Both;
            grd.AllowSorting = false;

            //<DisplayLayout DefaultCentury="2000" CellClickActionDefault="RowSelect" BorderCollapseDefault="Separate" TableLayout="Fixed" CompactRendering="False" SelectTypeRowDefault="Single" AllowColSizingDefault="Free" RowHeightDefault="18px" RowSizingDefault="Free" GridLinesDefault="NotSet" HeaderClickActionDefault="SortMulti" AllowSortingDefault="Yes" AutoGenerateColumns="False" RowSelectorsDefault="No">
            /*grd.DisplayLayout.DefaultCentury = 2000;
            grd.DisplayLayout.BorderCollapseDefault = Infragistics.WebUI.UltraWebGrid.BorderCollapse.Separate;
            grd.DisplayLayout.TableLayout = Infragistics.WebUI.UltraWebGrid.TableLayout.Fixed;*/
            //grd.DisplayLayout.CompactRendering = false;
            //SelectTypeRowDefault="Single"
            /*grd.DisplayLayout.SelectTypeRowDefault = Infragistics.WebUI.UltraWebGrid.SelectType.Single;
            grd.DisplayLayout.RowHeightDefault = new Unit(18, UnitType.Pixel);
            grd.DisplayLayout.RowSizingDefault = Infragistics.WebUI.UltraWebGrid.AllowSizing.Free;
            grd.DisplayLayout.GridLinesDefault = Infragistics.WebUI.UltraWebGrid.UltraGridLines.Both;
            grd.DisplayLayout.RowSelectorsDefault = Infragistics.WebUI.UltraWebGrid.RowSelectors.No;*/

            /*grd.DisplayLayout.AllowColSizingDefault = Infragistics.WebUI.UltraWebGrid.AllowSizing.Free;
            grd.DisplayLayout.AllowAddNewDefault = Infragistics.WebUI.UltraWebGrid.AllowAddNew.No;
            grd.DisplayLayout.AllowColumnMovingDefault = Infragistics.WebUI.UltraWebGrid.AllowColumnMoving.None;
            grd.DisplayLayout.AllowSortingDefault = Infragistics.WebUI.UltraWebGrid.AllowSorting.No;
            grd.DisplayLayout.AllowUpdateDefault = Infragistics.WebUI.UltraWebGrid.AllowUpdate.No;
            grd.DisplayLayout.CellClickActionDefault = Infragistics.WebUI.UltraWebGrid.CellClickAction.RowSelect;
            //<RowTemplateStyle BackColor="White" BorderColor="Gray" BorderStyle="Ridge">*/
            /*grd.DisplayLayout.RowStyleDefault.BackColor = System.Drawing.Color.White;
            grd.DisplayLayout.RowStyleDefault.BorderColor = System.Drawing.Color.Gray;
            grd.DisplayLayout.RowStyleDefault.BorderStyle = BorderStyle.Ridge;*/
            grd.RowStyle.BackColor = System.Drawing.Color.White;
            grd.RowStyle.BorderColor = System.Drawing.Color.Gray;
            grd.RowStyle.BorderStyle = BorderStyle.Ridge;

            //  <BorderDetails WidthBottom="3px" WidthLeft="3px" WidthRight="3px" WidthTop="3px" />
            /*grd.DisplayLayout.RowStyleDefault.BorderDetails.WidthBottom = new Unit(3, UnitType.Pixel);
            grd.DisplayLayout.RowStyleDefault.BorderDetails.WidthLeft = new Unit(3, UnitType.Pixel);
            grd.DisplayLayout.RowStyleDefault.BorderDetails.WidthRight = new Unit(3, UnitType.Pixel);
            grd.DisplayLayout.RowStyleDefault.BorderDetails.WidthTop = new Unit(3, UnitType.Pixel);*/
            grd.RowStyle.BorderWidth = new Unit(3, UnitType.Pixel);

            //<ActivationObject BorderColor="181, 196, 223">
            //    <BorderDetails WidthLeft="0px" WidthRight="0px" />
            //</ActivationObject>
            /*grd.DisplayLayout.ActivationObject.BorderColor = System.Drawing.Color.FromArgb(181, 196, 223);
            grd.DisplayLayout.ActivationObject.BorderDetails.WidthLeft = new Unit(0, UnitType.Pixel);
            grd.DisplayLayout.ActivationObject.BorderDetails.WidthRight = new Unit(0, UnitType.Pixel);*/

            // <RowExpAreaStyleDefault CssClass="igwgRowExpBlue2k7">
            //grd.DisplayLayout.RowExpAreaStyleDefault.CssClass = "igwgRowExpBlue2k7";
            //<FooterStyleDefault>
            //  <BorderDetails ColorLeft="White" ColorTop="White" WidthLeft="1px" WidthTop="1px" />
            //</FooterStyleDefault>
            /*grd.DisplayLayout.FooterStyleDefault.BorderDetails.WidthLeft = new Unit(1, UnitType.Pixel);
            grd.DisplayLayout.FooterStyleDefault.BorderDetails.WidthTop = new Unit(1, UnitType.Pixel);*/
            grd.FooterStyle.BorderColor = System.Drawing.Color.White;
            grd.FooterStyle.Width = new Unit(1, UnitType.Pixel);
            //<RowStyleDefault CssClass="igwgRowBlue2k7" BorderStyle="Solid" TextOverflow="Ellipsis" >
            //    <BorderDetails WidthBottom="1px" WidthLeft="0px" WidthRight="1px" WidthTop="1px" ColorRight="236, 233, 216" ColorBottom="236, 233, 216"/>
            //    <Padding Left="3px" />
            //</RowStyleDefault>
            /*grd.DisplayLayout.RowStyleDefault.TextOverflow = Infragistics.WebUI.UltraWebGrid.TextOverflow.Ellipsis;
            grd.DisplayLayout.RowStyleDefault.Padding.Left = new Unit(3, UnitType.Pixel);*/
            grd.RowStyle.CssClass = "igwgRowBlue2k7";
            grd.RowStyle.BorderStyle = BorderStyle.Solid;
            grd.RowStyle.BorderWidth = new Unit(1, UnitType.Pixel);
            grd.RowStyle.BorderColor = System.Drawing.Color.FromArgb(236, 233, 216);

            //<RowSelectorStyleDefault CssClass="igwgRowSlctrBlue2k7">
            grd.SelectedRowStyle.CssClass = "igwgRowSelBlue2k7";
            grd.SelectedRowStyle.Font.Bold = true;

            /*grd.DisplayLayout.RowSelectorStyleDefault.CssClass = "igwgRowSlctrBlue2k7";
            //<FixedHeaderStyleDefault CssClass="igwgHdrFxdBlue2k7">
            grd.DisplayLayout.FixedHeaderStyleDefault.CssClass = "igwgHdrFxdBlue2k7";
            //<SelectedRowStyleDefault CssClass="igwgRowSelBlue2k7" Font-Bold="True">
            grd.DisplayLayout.SelectedRowStyleDefault.CssClass = "igwgRowSelBlue2k7";
            grd.DisplayLayout.SelectedRowStyleDefault.Font.Bold = true;*/
            //<HeaderStyleDefault Font-Bold="True" BackColor="#EEEEEE" BorderStyle="Solid" >
            //    <BorderDetails ColorLeft="White" ColorTop="White" WidthLeft="1px" WidthTop="1px" />
            //</HeaderStyleDefault>
            grd.HeaderStyle.BorderStyle = BorderStyle.Solid;
            grd.HeaderStyle.Font.Bold = true;
            grd.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#EEEEEE");
            grd.HeaderStyle.BorderStyle = BorderStyle.Solid;
            grd.HeaderStyle.BorderColor = System.Drawing.Color.White;
            grd.HeaderStyle.BorderWidth = new Unit(1, UnitType.Pixel);

            //<RowAlternateStyleDefault CssClass="igwgRowAltBlue2k7">
            //    <BorderDetails ColorLeft="197, 202, 219" ColorTop="197, 202, 219" />
            //</RowAlternateStyleDefault>
            grd.AlternatingRowStyle.CssClass = "igwgRowAltBlue2k7";
            grd.AlternatingRowStyle.BorderColor = System.Drawing.Color.FromArgb(127, 202, 219);
            grd.AlternatingRowStyle.BorderStyle = BorderStyle.Solid;
            grd.AlternatingRowStyle.BorderWidth = new Unit(1, UnitType.Pixel);

            //<Images ImageDirectory="../DevCommon/images/WebGrid/">
            //    <ExpandImage Url="ig_tblPlus.gif" />
            //    <SortDescendingImage Url="ig_tblSortDesc.gif" />
            //    <SortAscendingImage Url="ig_tblSortAsc.gif" />
            //    <CollapseImage Url="ig_tblMinus.gif" />
            //</Images>
            /*grd.DisplayLayout.Images.ImageDirectory = this.ImageGrid;
            grd.DisplayLayout.Images.ExpandImage.Url = this.GridExpandImage;
            grd.DisplayLayout.Images.SortDescendingImage.Url = this.GridDescendImage;
            grd.DisplayLayout.Images.SortAscendingImage.Url = this.GridAscendingImage;
            grd.DisplayLayout.Images.CollapseImage.Url = this.GridCollapseImage;*/
            //<SelectedHeaderStyleDefault CssClass="igwgHdrSelBlue2k7">
            /*grd.DisplayLayout.SelectedHeaderStyleDefault.CssClass = "igwgHdrSelBlue2k7";
            //<FormulaErrorStyleDefault CssClass="igwgFormulaErrBlue2k7">
            grd.DisplayLayout.FormulaErrorStyleDefault.CssClass = "igwgFormulaErrBlue2k7";
            //<EditCellStyleDefault CssClass="igwgCellEdtBlue2k7">
            grd.DisplayLayout.EditCellStyleDefault.CssClass = "igwgCellEdtBlue2k7";*/
            //<FrameStyle CssClass="igwgFrameBlue2k7" Width="100%">
            //    <BorderDetails ColorBottom="101, 147, 207" ColorLeft="101, 147, 207" ColorRight="101, 147, 207"
            //StyleBottom="Solid" StyleLeft="Solid" StyleRight="Solid" StyleTop="None" WidthBottom="1px"
            //WidthLeft="1px" WidthRight="1px" WidthTop="0px" />
            //</FrameStyle>
            /*grd.DisplayLayout.FrameStyle.CssClass = "igwgFrameBlue2k7";
            grd.DisplayLayout.FrameStyle.Width = new Unit(100, UnitType.Percentage);
            grd.DisplayLayout.FrameStyle.BorderDetails.ColorBottom = System.Drawing.Color.FromArgb(101, 147, 207);
            grd.DisplayLayout.FrameStyle.BorderDetails.ColorLeft = System.Drawing.Color.FromArgb(101, 147, 207);
            grd.DisplayLayout.FrameStyle.BorderDetails.ColorRight = System.Drawing.Color.FromArgb(101, 147, 207);
            grd.DisplayLayout.FrameStyle.BorderDetails.ColorTop = System.Drawing.Color.FromArgb(101, 147, 207);
            grd.DisplayLayout.FrameStyle.BorderDetails.StyleBottom = BorderStyle.Solid;
            grd.DisplayLayout.FrameStyle.BorderDetails.StyleLeft = BorderStyle.Solid;
            grd.DisplayLayout.FrameStyle.BorderDetails.StyleRight = BorderStyle.Solid;
            grd.DisplayLayout.FrameStyle.BorderDetails.StyleTop = BorderStyle.Solid;
            grd.DisplayLayout.FrameStyle.BorderDetails.WidthBottom = new Unit(1, UnitType.Pixel);
            grd.DisplayLayout.FrameStyle.BorderDetails.WidthLeft = new Unit(1, UnitType.Pixel);
            grd.DisplayLayout.FrameStyle.BorderDetails.WidthRight = new Unit(1, UnitType.Pixel);
            grd.DisplayLayout.FrameStyle.BorderDetails.WidthTop = new Unit(1, UnitType.Pixel);
            // <FixedCellStyleDefault CssClass="igwgCellFxdBlue2k7">
            grd.DisplayLayout.FixedCellStyleDefault.CssClass = "igwgCellFxdBlue2k7";
            //<FixedFooterStyleDefault CssClass="igwgFtrFxdBlue2k7">
            grd.DisplayLayout.FixedFooterStyleDefault.CssClass = "igwgFtrFxdBlue2k7";*/

            grd.AllowPaging = false;
            //grd.DisplayLayout.Pager.AllowPaging = false;

            return grd;
        }

        protected virtual System.Web.UI.WebControls.Menu CreateUltraWebMenu(string MenuId)
        {
            System.Web.UI.WebControls.Menu Menu = new Menu();
            //Infragistics.WebUI.UltraWebNavigator.UltraWebMenu Menu = new Infragistics.WebUI.UltraWebNavigator.UltraWebMenu();
            Menu.ID = MenuId;
            Menu.EnableViewState = true;
            //Menu.EnhancedRendering.EnhancedRendering = true;

            //<ignav:ultrawebmenu id="WITMenu" runat="server" TopAligment="Center"
            //ImageDirectory="../../DevCommon/images/" EnhancedRendering="True" Font-Size="8pt" ForeColor="#3D4D81" 
            //ItemPaddingSubMenus="0" SeparatorClass="" >
            /*Menu.TopAligment = Infragistics.WebUI.UltraWebNavigator.TextAlignment.Center;
            Menu.ForeColor = System.Drawing.ColorTranslator.FromHtml("#3D4D81");
            Menu.ItemPaddingSubMenus = 0;
            Menu.ImageDirectory = this.MenuImage;*/
            Menu.ForeColor = System.Drawing.ColorTranslator.FromHtml("#3D4D81");

            //    <ItemStyle Font-Size="8pt" Font-Names="Tahoma" ForeColor="#3D4D81" BackColor="#E1EEFF" />
            /*Menu.ItemStyle.Font.Size = new FontUnit(8, UnitType.Point);
            Menu.ItemStyle.Font.Name = "Tahoma";
            Menu.ItemStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#3D4D81");
            Menu.ItemStyle.BorderColor = System.Drawing.ColorTranslator.FromHtml("#E1EEFF");
            Menu.ItemStyle.BackColor = System.Drawing.Color.FromArgb(0xe1eeff);*/
            MenuItemStyle ItemStyle = new MenuItemStyle();

            ItemStyle.Font.Size = new FontUnit(8, UnitType.Point);
            ItemStyle.Font.Name = "Tahoma";
            ItemStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#3D4D81");
            ItemStyle.BorderColor = System.Drawing.ColorTranslator.FromHtml("#E1EEFF");
            ItemStyle.BackColor = System.Drawing.Color.FromArgb(0xe1eeff);

            Menu.LevelMenuItemStyles.Add(ItemStyle);

            //<DisabledStyle ForeColor="LightGray" />
            //Menu.DisabledStyle.ForeColor = System.Drawing.Color.LightGray;

            //    <HoverItemStyle Cursor="Hand" BorderWidth="1px" BackColor="#FFCF47" BorderColor="#D1A040">
            //        <BorderDetails ColorTop="111, 157, 217" ColorBottom="111, 157, 217" />
            //    </HoverItemStyle>
            /*Menu.HoverItemStyle.Cursor = Infragistics.WebUI.Shared.Cursors.Hand;
            Menu.HoverItemStyle.BorderWidth = new Unit(1, UnitType.Pixel);
            Menu.HoverItemStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFCF47");
            Menu.HoverItemStyle.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D1A040");
            Menu.HoverItemStyle.BorderDetails.ColorTop = System.Drawing.Color.FromArgb(111, 157, 217);
            Menu.HoverItemStyle.BorderDetails.ColorBottom = System.Drawing.Color.FromArgb(111, 157, 217);*/

            Menu.StaticHoverStyle.BorderWidth = new Unit(1, UnitType.Pixel);
            Menu.StaticHoverStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFCF47");
            Menu.StaticHoverStyle.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D1A040");
            Menu.StaticHoverStyle.BorderColor = System.Drawing.Color.FromArgb(111, 157, 217);

            Menu.DynamicHoverStyle.BorderWidth = new Unit(1, UnitType.Pixel);
            Menu.DynamicHoverStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFCF47");
            Menu.DynamicHoverStyle.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D1A040");
            Menu.DynamicHoverStyle.BorderColor = System.Drawing.Color.FromArgb(111, 157, 217);

            //<ExpandEffects ShadowColor="Gray" Type="Fade" ShadowWidth="0"  Delay="0" Duration="0" RemovalDelay="0" />

            /*Menu.ExpandEffects.ShadowColor = System.Drawing.Color.Gray;
            Menu.ExpandEffects.Type = Infragistics.WebUI.UltraWebNavigator.ExpandEffectType.Fade;
            Menu.ExpandEffects.ShadowWidth = 2;
            Menu.ExpandEffects.Delay = 0;
            Menu.ExpandEffects.Duration = 1;
            Menu.ExpandEffects.RemovalDelay = 0;*/

            //    <TopSelectedStyle Cursor="Hand" BorderWidth="1px" BorderColor="RoyalBlue">
            //        <BorderDetails StyleRight="Solid" />
            //    </TopSelectedStyle>
            /*Menu.TopSelectedStyle.Cursor = Infragistics.WebUI.Shared.Cursors.Hand;
            Menu.TopSelectedStyle.BorderWidth = new Unit("1px");
            Menu.TopSelectedStyle.BorderColor = System.Drawing.Color.RoyalBlue;
            Menu.TopSelectedStyle.BorderDetails.StyleRight = BorderStyle.Solid;*/

            Menu.DynamicSelectedStyle.BorderWidth = new Unit("1px");
            Menu.DynamicSelectedStyle.BorderColor = System.Drawing.Color.RoyalBlue;
            Menu.DynamicSelectedStyle.BorderStyle = BorderStyle.Solid;

            //    <SeparatorStyle BackgroundImage="../../DevCommon/Images/ig_menuSep.gif" CustomRules="background-repeat:repeat-x; " BackColor="#E1EEFF">
            //        <BorderDetails StyleLeft="Solid" StyleRight="Solid" WidthLeft="1px" WidthTop="1px" />
            //    </SeparatorStyle>
            /*Menu.SeparatorStyle.BackgroundImage = this.MenuBGImage;
            Menu.SeparatorStyle.CustomRules = "background-repeat:repeat-x; ";
            Menu.SeparatorStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1EEFF");
            Menu.SeparatorStyle.BorderDetails.StyleLeft = BorderStyle.Solid;
            Menu.SeparatorStyle.BorderDetails.WidthLeft = new Unit("1px");
            Menu.SeparatorStyle.BorderDetails.StyleRight = BorderStyle.Solid;
            Menu.SeparatorStyle.BorderDetails.WidthTop = new Unit("1px");*/

            //    <HeaderStyle BackColor="#FF8080" />
            //Menu.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF8080");

            //    <TopLevelParentItemStyle BackColor="Transparent" Font-Bold="False" Font-Names="Tahoma" Font-Size="8pt">
            //        <BorderDetails StyleRight="Solid" ColorRight="Black" WidthRight="1px" />
            //    </TopLevelParentItemStyle>
            /*Menu.TopLevelParentItemStyle.BackColor = System.Drawing.Color.Transparent;
            Menu.TopLevelParentItemStyle.BorderStyle = BorderStyle.None;
            Menu.TopLevelParentItemStyle.BorderDetails.WidthRight = new Unit("1px");
            Menu.TopLevelParentItemStyle.BorderDetails.StyleRight = BorderStyle.Solid;
            Menu.TopLevelParentItemStyle.BorderDetails.ColorRight = System.Drawing.Color.Black;
            Menu.TopLevelParentItemStyle.Font.Name = "Tahoma";
            Menu.TopLevelParentItemStyle.Font.Size = new FontUnit(8, UnitType.Point);*/
            //    <ParentItemStyle BackColor="Transparent">
            //    </ParentItemStyle>
            //Menu.ParentItemStyle.BackColor = System.Drawing.Color.Transparent;

            return Menu;
        }

        #endregion

        /// <summary>
        /// Most create the object that this control edit and set it to m_Datasource, and
        /// create any aditional business object needed for the databinding.
        /// </summary>
        protected abstract void EnsureDatasourceObject();

        public virtual void ResetDatasource()
        {
            m_Datasource = null;
            EnsureDatasourceObject();
        }

        /// <summary>
        /// Get the modified information from the controls (that come from the post) and fill the
        /// respective values of the datasource (fill the m_CustomPropertiesValues)
        /// </summary>
        public abstract void SetItemFields();

        protected bool CheckDatasource()
        {
            m_ErrorLabel = new Label();
            Controls.Add(m_ErrorLabel);
            m_ErrorLabel.Text = "";
            m_ErrorLabel.Visible = false;
            if (ErrorText != null)
            {
                m_ErrorLabel.Visible = true;
                m_ErrorLabel.Text = ErrorText;
                return false;
            }
            if (!this.HasDatasource())
            {
                CheckParameters();
                m_ErrorLabel.Visible = true;
                m_ErrorLabel.Text = Resources.ErrorMessages.errInvalidDatasource;
                /*              if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                              {
                                  throw new Exception(Resources.ErrorMessages.errInvalidDatasource);
                              }*/
                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual void SetError(String Message)
        {
            if (m_ErrorLabel != null)
            {
                m_ErrorLabel.Visible = true;
                m_ErrorLabel.Text = Message;
                m_ErrorLabel.ForeColor = System.Drawing.Color.Red;
                clsLog.Trace(Message, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Message);
                }
               
            }
        }

        protected virtual void ClearError()
        {
            if (m_ErrorLabel != null)
            {
                m_ErrorLabel.Visible = false;
                m_ErrorLabel.Text = "";
            }
        }

        protected void NewButton_Command(object sender, CommandEventArgs e)
        {
            //CommandsToExecute.Add(e);
            ExecuteCommand(sender, e, false);
        }

        public virtual void ExecuteCommand(object sender, CommandEventArgs e, bool IsContext)
        {
            if (CommandEvent != null)
            {
                CommandEvent(this, e);
            }
            if (e.CommandName.Length > 0)
            {
                string[] commandNameAndTarget = e.CommandName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string CommandName = commandNameAndTarget[0];
                string TargetName = "";
                string RedirectPageTo = "";
                string AlertMessage = "";
                object source = null;
                if (commandNameAndTarget.Length > 1)
                {
                    TargetName = commandNameAndTarget[1].Trim();
                }
                if (commandNameAndTarget.Length > 2)
                {
                    RedirectPageTo = commandNameAndTarget[2].Trim();
                }
                if (commandNameAndTarget.Length > 3)
                {
                    AlertMessage = commandNameAndTarget[3].Trim();
                }

                if (TargetName != "")
                {
                    clsBaseLayoutWP TargetWpt = (clsBaseLayoutWP)ChildEditorControls[TargetName];
                    if (TargetWpt != null)
                    {
                        TargetWpt.ExecuteCommand(sender, new CommandEventArgs(CommandName, e.CommandArgument), true);
                    }
                }
                else
                {
                    if ((CommandName.ToUpper() == "UPDATE") || (CommandName.ToUpper() == "INSERT"))
                    {
                        SaveChanges(CommandName.ToString(), e.CommandArgument.ToString());
                    }
                    else if (CommandName.ToUpper() == "CHANGELAYOUTTO")
                    {
                        if (e.CommandArgument.ToString().Trim() != "")
                        {
                            LayoutName = e.CommandArgument.ToString().Trim();
                        }
                    }
                    else if (CommandName.ToUpper() == "NEXTLAYOUTPAGE")
                    {
                        SelectedLayoutPage += 1;
                    }
                    else if (CommandName.ToUpper() == "PREVIEWSLAYOUTPAGE")
                    {
                        SelectedLayoutPage -= 1;
                    }
                    else if (CommandName.ToUpper() == "NAVIGATETO")
                    {
                        EnsureDatasourceObject();
                        SetItemFields();
                        String[] enteredArgs = e.CommandArgument.ToString().Split('|');
                        int indexRow = -1;
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
                        if (m_Datasource != null && targetURL.Contains("{"))
                        {

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
                                targetURL = clsSharedMethods.SharedMethods.parseBOPropertiesString(BLDatasource, targetURL, BLDatasource.GetType().AssemblyQualifiedName, "GUID");
                            }
                        }
                        Page.Response.Redirect(targetURL);
                    }
                    else
                    {
                        EnsureDatasourceObject();
                        SetItemFields();
                        String[] enteredArgs = e.CommandArgument.ToString().Split('|');
                        int indexRow = Convert.ToInt32(enteredArgs[0]);
                        String args = enteredArgs[1];

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

                        if (indexRow == -1)
                        {
                            method = m_Datasource.GetType().GetMethod(CommandName);
                            source = m_Datasource;
                        }
                        else
                        {
                            IList datasource = (IList)m_Datasource;
                            method = datasource[indexRow].GetType().GetMethod(CommandName);
                            source = datasource[indexRow];
                        }
                        try
                        {

                            if (method != null)
                            {
                                method.Invoke(source, arguments);
                                if (RedirectPageTo != "")
                                {
                                    string ScriptAlertAndRedirect = "<script type='text/javascript' language='javascript'>alert('{0}');window.location='{1}'</script>";
                                    source = null;
                                    if (m_Datasource != null)
                                    {

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
                                            RedirectPageTo = clsSharedMethods.SharedMethods.parseBOPropertiesString(BLDatasource, RedirectPageTo, BLDatasource.GetType().AssemblyQualifiedName, "GUID");
                                        }
                                    }
                                    if (AlertMessage != "")
                                    {
                                        System.Web.UI.ScriptManager.RegisterStartupScript(Page, this.GetType(), "MyScript", string.Format(ScriptAlertAndRedirect, AlertMessage, RedirectPageTo), false);
                                    }
                                    else
                                    {
                                        Page.Response.Redirect(RedirectPageTo);
                                    }
                                }
                            }
                            else
                            {
                                //RegisterStartupScript
                                if (AlertMessage != "")
                                {
                                    string Script = "<script type='text/javascript' language='javascript'>alert('{0}');</script>";
                                    System.Web.UI.ScriptManager.RegisterStartupScript(Page, this.GetType(), "MyScript", string.Format(Script, AlertMessage), false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SetError(ex.Message);
                        }
                        //(ClassName, button.CommandName, button.CommandArgument)
                        //m_Datasource 
                    }
                }
            }
        }

        public virtual bool EvaluateCondition(object Datasource, string Condition)
        {
            try
            {
                object Result = Axelerate.BusinessLayerUITools.Misc.Evaluator.EvalToObject(Condition, Datasource);
                bool bResult = false;
                if (bool.TryParse(Result.ToString(), out bResult))
                {
                    return bResult;
                }
                return false;
            }
            catch (Exception ex)
            {
                //return false;
                throw new Exception(string.Format(Resources.ErrorMessages.errEval0, Condition), ex);
            }
        }

        internal string getArgument(String argument)
        {
            try
            {
                String result = "";
                if (this.Page != null)
                {
                    if (Axelerate.BusinessLogic.SharedBusinessLogic.Support.clsBusinessGlobals.isGlobal(argument))
                    {
                        result = Axelerate.BusinessLogic.SharedBusinessLogic.Support.clsBusinessGlobals.GetGlobalValue(argument).ToString();
                    }
                    else if (this.Page.Request.Params[argument] != null)
                    {
                        result = (string)this.Page.Request.Params[argument];
                    }
                    else
                    {
                        if (System.Web.HttpContext.Current.Session[argument] != null)
                        {
                            ViewState[argument] = System.Web.HttpContext.Current.Session[argument];
                        }
                        if (ViewState[argument] != null)
                        {
                            result = (string)ViewState[argument];
                        }
                    }
                }
                return result;
                //return clsSharedMethods.SharedMethods.parseBOPropertiesString(m_Datasource, result, ClassName, ((IBLListBase)m_Datasource).); result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        protected abstract void SaveChanges(string CommandName, string CommandArguments);

        protected abstract string GetXMLLayout();

        protected virtual bool CheckAccess(object Datasource, BusinessObjectOperations operation)
        {
            if (typeof(BLBusinessBase).IsInstanceOfType(Datasource))
            {
                BLBusinessBase BObject = (BLBusinessBase)Datasource;
                clsSecurityOperation Operation;
                switch (operation)
                {
                    case BusinessObjectOperations.CREATE:
                        Operation = BObject.CreateSecurityOperation;
                        break;
                    default:
                    case BusinessObjectOperations.READ:
                        Operation = BObject.ReadSecurityOperation;
                        break;
                    case BusinessObjectOperations.UPDATE:
                        Operation = BObject.UpdateSecurityOperation;
                        break;
                    case BusinessObjectOperations.DELETE:
                        Operation = BObject.DeleteSecurityOperation;
                        break;
                }

                bool hasAccess = true;
                if (Operation != null)
                {
                    try
                    {
                        if (!Operation.CheckAccess())
                        {
                            hasAccess = false;
                        }
                    }
                    catch (SecurityException ex)
                    {
                        hasAccess = false;
                    }
                }
                return hasAccess;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Constructor

        public clsBaseLayoutWP() : base() { }

        #endregion

        #region Overrides

        protected void RecreateChildControls()
        {
            EnsureChildControls();
        }

        public override void DataBind()
        {
            EnsureDatasourceObject();
            CreateChildControls();
            base.DataBind();
            // CreateChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (ViewState["EditorWasRendered"] != null && HasDatasource())
            {
                //If is Postback then we need set the item with the values that has change and recreate the controls
                //to reflect this changes.
                SetItemFields();
                int[] SelectedTabIndexes = new int[TabControlList.Count];
                for (int i = 0; i < TabControlList.Count; i++)
                {
                    SelectedTabIndexes[i] = ((AjaxControlToolkit.TabContainer)TabControlList[i]).ActiveTabIndex;
                }
                CreateChildControls();
                //I do this check only on case that the displayform xml has change. 
                if (SelectedTabIndexes.Length == TabControlList.Count)
                {
                    for (int i = 0; i < TabControlList.Count; i++)
                    {
                        if (((AjaxControlToolkit.TabContainer)TabControlList[i]).Tabs.Count > SelectedTabIndexes[i])
                        {
                            ((AjaxControlToolkit.TabContainer)TabControlList[i]).ActiveTabIndex = SelectedTabIndexes[i];
                        }
                    }
                }
            }
            else
            {
                ViewState["EditorWasRendered"] = true;
            }
            if (MinimizeRestoreBt != null)
            {
                string imgRestoreUrl = "";
                string imgMinimizeUrl = "";
                if (Page != null) 
                {
                    imgRestoreUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.igpnl_dwn.gif");
                    imgMinimizeUrl = Page.ClientScript.GetWebResourceUrl(this.GetType(), "Axelerate.BusinessLayerUITools.Resources.igpnl_up.gif");
                }
                
                if(MinimizeButtonImage != "")
                {
                    imgMinimizeUrl = MinimizeButtonImage;
                }

                if(RestoreButtonImage != "")
                {
                    imgRestoreUrl = RestoreButtonImage;
                }
                
                MinimizeRestoreBt.Attributes["OnClick"] = "javascript:BLWPCollapseExpand('" + ClipZone.ClientID + "','" + MinimizeRestoreBt.ClientID + "','" + imgMinimizeUrl + "','" + imgRestoreUrl + "')";
            }
        }

        


        private void RegisterCollapseScript()
        {
            System.Text.StringBuilder Script = new System.Text.StringBuilder(200);
            Script.AppendLine("<script type=\"text/javascript\">");
            Script.AppendLine("     function BLWPCollapseExpand(PanelID, ImageID, CollapseImgUrl, ExpandImgUrl) ");
            Script.AppendLine("     { ");
            Script.AppendLine("         var Panel = document.getElementById(PanelID);");
            Script.AppendLine("         var Img = document.getElementById(ImageID);");
            Script.AppendLine("         if(Panel.style.display == \"none\") ");
            Script.AppendLine("         {");
            Script.AppendLine("             Panel.style.display = \"\";");
            Script.AppendLine("             Img.src = CollapseImgUrl;");
            Script.AppendLine("             Img.alt = 'Collapse'");
            Script.AppendLine("         }");
            Script.AppendLine("         else {");
            Script.AppendLine("             Panel.style.display = \"none\"; ");
            Script.AppendLine("             Img.src = ExpandImgUrl;");
            Script.AppendLine("             Img.alt = 'Expand'");
            Script.AppendLine("         }");
            Script.AppendLine("      }");
            Script.AppendLine("</script>");

            ScriptManager.RegisterStartupScript(this, typeof(clsBaseLayoutWP), "BLWP_Collapse_Expand_Scritp", Script.ToString(), false);
        }

        public abstract bool HasDatasource();

        /// <summary>
        /// Create all the child object needed.
        /// </summary>
        protected override void CreateChildControls()
        {
            EnsureDatasourceObject();
            if (CheckDatasource())
            {
                Controls.Clear();



                ContextMenuLayouts = null;
                TabControlList.Clear();
                //PlaneFieldControls.Clear();
                LayoutPages.Clear();
                //           ChildEditorControls.Clear();
                TreeViewNodeStyles.Clear();
                if (this.HeaderTemplate != null)
                {
                    HeaderTemplateContainer header = this.InstantiateHeader();
                    Controls.Add(header);
                }

                InitLayount(LayoutTextXML, m_Datasource, IsPostback);

                if (this.FooterTemplate != null)
                {
                    FooterTemplateContainer footer = this.InstantiateFooter();
                    Controls.Add(footer);
                }
                cont = 0;
            }
        }
        protected void ClearLayout()
        {
            Controls.Clear();
            ContextMenuLayouts = null;
            TabControlList.Clear();
            LayoutPages.Clear();
            TreeViewNodeStyles.Clear();
            PlaneFieldControls.Clear();
            ChildEditorControls.Clear();
        }

        #endregion

        #region Public Properties


        [Category("Command Appearance")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ButtonPressedImage")]
        [DefaultValue("")]
        [WebDisplayName("Default Button Pressed Image:")]
        [WebDescription("Cefault image for buttons when pressed.")]
        public String ButtonPressedImage
        {
            get
            {
                if (ViewState["ButtonPressedImage"] == null)
                {
                    ViewState["ButtonPressedImage"] = clsConfigurationProfile.Current.getPropertyValue("WPButtonPressedImage");
                }
                return (ViewState["ButtonPressedImage"] == null) ? "" : (String)ViewState["ButtonPressedImage"];
            }
            set
            {
                ViewState["ButtonPressedImage"] = value;
            }
        }

        [Category("Command Appearance")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "ButtonImage")]
        [DefaultValue("")]
        [WebDisplayName("Default Button Image:")]
        [WebDescription("Default image for buttons.")]
        public String ButtonImage
        {
            get
            {
                if (ViewState["ButtonImage"] == null)
                {
                    ViewState["ButtonImage"] = clsConfigurationProfile.Current.getPropertyValue("WPButtonImage");
                }
                return (ViewState["ButtonImage"] == null) ? "" : (String)ViewState["ButtonImage"];
            }
            set
            {
                ViewState["ButtonImage"] = value;
            }
        }

        [Bindable(true), Category("Command Appearance"), DefaultValue("")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Button Hover Image")]
        [WebDescription("Default image used for button images when the mouse is over it.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ButtonHoverImage")]
        public String ButtonHoverImage
        {
            get
            {
                if (ViewState["ButtonHoverImage"] == null)
                {
                    ViewState["ButtonHoverImage"] = clsConfigurationProfile.Current.getPropertyValue("WPButtonHoverImage");
                }
                return (ViewState["ButtonHoverImage"] == null) ? "" : (String)ViewState["ButtonHoverImage"];
            }
            set
            {
                ViewState["ButtonHoverImage"] = value;
            }
        }

        public String MenuBGImage
        {
            get
            {
                if (ViewState["MenuBGImage"] == null)
                {
                    ViewState["MenuBGImage"] = "../DevCommon/Images/ig_menuSep.gif";
                }
                return (String)ViewState["MenuBGImage"];
            }
            set
            {
                ViewState["MenuBGImage"] = value;
            }
        }


        public String MenuImage
        {
            get
            {
                if (ViewState["MenuImage"] == null)
                {
                    ViewState["MenuImage"] = "../../DevCommon/images/";
                }
                return (String)ViewState["MenuImage"];
            }
            set
            {
                ViewState["MenuImage"] = value;
            }
        }

        public String GridCollapseImage
        {
            get
            {
                if (ViewState["GridCollapseImage"] == null)
                {
                    ViewState["GridCollapseImage"] = "ig_tblMinus.gif";
                }
                return (String)ViewState["GridCollapseImage"];
            }
            set
            {
                ViewState["GridCollapseImage"] = value;
            }
        }


        public String GridAscendingImage
        {
            get
            {
                if (ViewState["GridAscendingImage"] == null)
                {
                    ViewState["GridAscendingImage"] = "ig_tblSortAsc.gif";
                }
                return (String)ViewState["GridAscendingImage"];
            }
            set
            {
                ViewState["GridAscendingImage"] = value;
            }
        }

        public String GridDescendImage
        {
            get
            {
                if (ViewState["GridDescendImage"] == null)
                {
                    ViewState["GridDescendImage"] = "ig_tblSortDesc.gif";
                }
                return (String)ViewState["GridDescendImage"];
            }
            set
            {
                ViewState["GridDescendImage"] = value;
            }
        }


        public String GridExpandImage
        {
            get
            {
                if (ViewState["GridExpandIMG"] == null)
                {
                    ViewState["GridExpandIMG"] = "";
                }
                return (String)ViewState["GridExpandIMG"];
            }
            set
            {
                ViewState["GridExpandIMG"] = value;
            }
        }

        public String ImageGrid
        {
            get
            {
                if (ViewState["ImageGrid"] == null)
                {
                    ViewState["ImageGrid"] = "../DevCommon/images/WebGrid";
                }
                return (String)ViewState["ImageGrid"];
            }
            set
            {
                ViewState["ImageGrid"] = value;
            }
        }


        public String ImageTab
        {
            get
            {
                if (ViewState["ImageTab"] == null)
                {
                    ViewState["ImageTab"] = "../../DevCommon/images/WebTab/";
                }
                return (String)ViewState["ImageTab"];
            }
            set
            {
                ViewState["ImageTab"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get Css Class image for the header.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Image directory")]
        [WebDescription("Directory path where images go to be.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ImageDir")]
        public String ImageDir
        {
            get
            {
                if (ViewState["ImageDir"] == null)
                {
                    ViewState["ImageDir"] = clsConfigurationProfile.Current.getPropertyValue("WPImageDir");
                }
                return (ViewState["ImageDir"] == null) ? "" : (String)ViewState["ImageDir"];
            }
            set
            {
                ViewState["ImageDir"] = value;
            }
        }

        /// <summary>
        /// Image path to be applied to the Maximize button
        /// </summary>
        public String MaximizeBTImage
        {
            get
            {
                if (ViewState["MaximizeBTImage"] == null)
                {
                    ViewState["MaximizeBTImage"] = ""; //../DevCommon/images/igpnl_up.gif
                }
                return (String)ViewState["MaximizeBTImage"];
            }
            set
            {
                ViewState["MaximizeBTImage"] = value;
            }
        }

        public bool IsPostback
        {
            get
            {
                return ViewState["EditorWasRendered"] != null;
            }
        }

        /// <summary>
        /// Set or Get if the control most show the maximize button
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the maximize button")]
        public bool ShowMaximizeButton
        {
            get
            {
                object val = ViewState["ShowMaximize"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowMaximize"] = value;
            }
        }

        /// <summary>
        /// Set or Get if the control most show the maximize button
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the minimize button")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Show Collapse/Expand button")]
        [WebDescription("Show the collapse/expand button on title header.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ShowMinimizeRestoreButton")]
        public bool ShowMinimizeRestoreButton
        {
            get
            {
                object val = ViewState["ShowMinimizeRestoreButton"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowMinimizeRestoreButton"] = value;
            }
        }


        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most do update syncs")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Do Asyncronizal Updates")]
        [WebDescription("Do Asyncronizal Updates for this webpart.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "AsyncUpdates")]
        public bool AsyncUpdates
        {
            get
            {
                object val = ViewState["AsyncUpdates"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["AsyncUpdates"] = value;
            }
        }

        /// <summary>
        /// Set or Get if the control most show the header
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get if the control most show the header")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Show custom title")]
        [WebDescription("Show the custom title for this webpart. This title can use image for rounder borders appareance")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "ShowHeader")]
        public bool ShowHeader
        {
            get
            {
                object val = ViewState["ShowHeader"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["ShowHeader"] = value;
            }
        }

        [Browsable(true), Category("Behavior"), DefaultValue(false),
        Description("Set or Get the default collapsible state of the control")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Is Collapsed")]
        [WebDescription("Default collapse state of the webpart")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "IsCollapsed")]
        public bool IsCollapsed
        {
            get
            {
                object val = ViewState["IsCollapsed"];
                return (val == null) ? false : (bool)val;
            }
            set
            {
                ViewState["IsCollapsed"] = value;
            }
        }

        [Browsable(true), Category("Behavior"), DefaultValue(true),
         Description("Set or Get if the control most retrieve the layout from the database")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Get Layout from Database")]
        [WebDescription("Set or Get if the control most retrieve the layout from the database")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "GetLayoutFromDatabase")]
        public bool GetLayoutFromDatabase
        {
            get
            {
                object val = ViewState["GetLayoutFromDatabase"];
                return (val == null) ? true : (bool)val;
            }
            set
            {
                ViewState["GetLayoutFromDatabase"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get Css Class image for the header.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Title Css Class")]
        [WebDescription("Css class to apply to custom title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "HeaderCssClass")]
        public string HeaderCssClass
        {
            get
            {
                object val = ViewState["HeaderCssClass"];
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["HeaderCssClass"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get Css Class for the body of the webpart.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Body Css Class")]
        [WebDescription("Css class to apply to body of the webpart.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "BodyCssClass")]
        public string BodyCssClass
        {
            get
            {
                object val = ViewState["BodyCssClass"];
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["BodyCssClass"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get the Minimize Button Image.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Minimize button image")]
        [WebDescription("Image to use in minimize button on title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "MinimizeButtonImage")]
        public string MinimizeButtonImage
        {
            get
            {
                object val = ViewState["MinimizeButtonImage"];
                if (val == null)
                {
                    val = clsConfigurationProfile.Current.getPropertyValue("WPMinimizeButtonImage");
                }
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["MinimizeButtonImage"] = value;
            }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
        Description("Set or Get the Restore Button Image.")]
        [Personalizable(PersonalizationScope.User)]
        [WebDisplayName("Restore Button Image")]
        [WebDescription("Image to use in restore button on title.")]
        [WebBrowsable(true)]
        [XmlElement(ElementName = "RestoreButtonImage")]
        public string RestoreButtonImage
        {
            get
            {
                object val = ViewState["RestoreButtonImage"];
                if (val == null)
                {
                    val = clsConfigurationProfile.Current.getPropertyValue("WPRestoreButtonImage");
                }
                return (val == null) ? string.Empty : (string)val;
            }
            set
            {
                ViewState["RestoreButtonImage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on maximize button
        /// The javascript method must has this signature: function(sender, command,  commandargument)
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(""),
       Description("Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on maximize button\n\rThe javascript method must has this signature: function(sender, command,  commandargument)")]
        public string OnClientClickMaximizeButton
        {
            get
            {
                string val = (string)ViewState["OnClientClickMaximizeButton"];
                return (val == null) ? string.Empty : val;
            }
            set
            {
                ViewState["OnClientClickMaximizeButton"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on cancel button
        /// The javascript method must has this signature: function(command,  commandargument)
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(""),
      Description("Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on cancel button\n\rThe javascript method must has this signature: function(sender, command,  commandargument)")]
        public string OnClientClickCancelButton
        {
            get
            {
                string val = (string)ViewState["OnClientClickCancelButton"];
                return (val == null) ? string.Empty : val;
            }
            set
            {
                ViewState["OnClientClickCancelButton"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on cancel button
        /// The javascript method must has this signature: function(command,  commandargument)
        /// </summary>
        [Bindable(true), Category("Behavior"), DefaultValue(""),
        Description("Gets or sets the client-side script that executes when an ImageButton control's Click event is raised on save button\n\rThe javascript method must has this signature: function(sender, command,  commandargument)")]
        public string OnClientClickSaveButton
        {
            get
            {
                string val = (string)ViewState["OnClientClickSaveButton"];
                return (val == null) ? string.Empty : val;
            }
            set
            {
                ViewState["OnClientClickSaveButton"] = value;
            }
        }

        public string LayoutTextXML
        {
            get
            {
                if (!GetLayoutFromDatabase)
                {
                    object value = ViewState["LayoutTextXML"];
                    if (value == null)
                    {
                        value = GetXMLLayout();
                    }
                    return value.ToString();
                }
                else
                {
                    return GetXMLLayout();
                }
            }
            set
            {
                ViewState["LayoutTextXML"] = value;
            }
        }

        public Panel HiddenPanel
        {
            get
            {
                if (hiddenPanel == null)
                {
                    hiddenPanel = new Panel();
                }
                return hiddenPanel;
            }
        }


        #endregion

        #region Public Methods
        public abstract bool IsNew();

        #endregion

        #region WebPart Connection

        #endregion


        
    }
}
