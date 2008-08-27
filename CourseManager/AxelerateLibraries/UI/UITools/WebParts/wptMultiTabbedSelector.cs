using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerUITools.Interfaces;
using Axelerate.BusinessLayerFrameWork.BLCore;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Collections;
using System.Xml.Serialization;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;
using System.Xml;
using System.Web.UI;
using AjaxControlToolkit;


namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptMultiTabbedSelector : clsBaseLayoutWP, ISelectorProvider
    {
        #region Property Members
        private Hashtable SearchHystory
        {
            get
            {
                if (ViewState["SearchHystory"] == null)
                {
                    ViewState["SearchHystory"] = new Hashtable();
                }
                return (Hashtable)ViewState["SearchHystory"];
            }
        }

        private IBLListBase Datasource
        {
            get
            {
                m_Datasource = ViewState["CPEDatasource"];
                return (IBLListBase)m_Datasource;
            }
            set
            {
                ViewState["CPEDatasource"] = value;
                /*m_CustomProperties = null;
                m_CustomPropertyLayout = null;
                m_CustomPropertiesValues = null;*/
                m_Datasource = value;
            }
        }

        [Category("Editor Properties")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "LayoutName")]
        [DefaultValue("")]
        [WebDisplayName("Editor Layout:")]
        [WebDescription("The name of the layout that will be displayed.")]
        public string LayoutName
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
                ViewState["XMLLayout"] = value;
            }
        }

        //private ArrayList m_selectedDatakeys = new ArrayList();

        private ArrayList SelectedDatakeys
        {
            get
            {
                if (ViewState["MultiTSelectedDatakeys"] == null)
                {
                    ViewState["MultiTSelectedDatakeys"] = new ArrayList();
                }
                return (ArrayList)ViewState["MultiTSelectedDatakeys"];
            }
            set
            {
                ViewState["MultiTSelectedDatakeys"] = value;
            }
        }

        private clsBaseLayoutWP ActiveControl
        {
            get
            {
                Object aux = ContextMenuLayouts;
                return _activeControl;
            }
            set
            {
                _activeControl = value;
            }

        }
        private clsBaseLayoutWP _activeControl = null;

        private IList m_SelectedItems;
        private IList m_NotSelectedItems;
        private int searchCounter
        {
            get
            {
                if (ViewState["searchCounter"] == null)
                {
                    ViewState["searchCounter"] = 0;
                }
                return (int)ViewState["searchCounter"];
            }
            set
            {
                ViewState["searchCounter"] = value;
            }
        }

        #endregion

        #region Method Overrides
        protected override bool CheckParameters()
        {
            Type DatasourceType = Type.GetType(ClassName);
            if (DatasourceType == null)
            {
                clsLog.Trace(Resources.ErrorMessages.errInvalidClassName + " " + ClassName, LogLevel.LowPriority);
                if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                {
                    throw new Exception(Resources.ErrorMessages.errInvalidClassName + " " + ClassName);
                }
                ErrorText = Resources.ErrorMessages.errInvalidClassName + " " + ClassName;
                return false;
            }
            return true;
        }

        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {
            EnsureDatasourceObject();
            SetItemFields();
            string typeName = "";
            string returnedProperty = "";
            string factoryParams = "";
            string factoryMethod = "";
            string className = "";
            string layoutName = "";
            string id = "";
            string wptBehavior = ComponentBehaviorType;
            if (CtrlNode.Attributes["ReturnedProperty"] != null)
            {
                if (CtrlNode.Attributes["ReturnedProperty"].Value == "")
                {
                    returnedProperty = null;
                }
                else returnedProperty = CtrlNode.Attributes["ReturnedProperty"].Value;
            }
            else
            {
                returnedProperty = null;
            }
            if (CtrlNode.Attributes["Type"] != null)
            {
                typeName = CtrlNode.Attributes["Type"].Value;
            }
            if (CtrlNode.Attributes["FactoryMethod"] != null)
            {
                factoryMethod = CtrlNode.Attributes["FactoryMethod"].Value;
            }
            if (CtrlNode.Attributes["FactoryParameters"] != null)
            {
                factoryParams = CtrlNode.Attributes["FactoryParameters"].Value;
            }
            if (CtrlNode.Attributes["ClassName"] != null)
            {
                className = CtrlNode.Attributes["ClassName"].Value;
            }
            if (CtrlNode.Attributes["LayoutName"] != null)
            {
                layoutName = CtrlNode.Attributes["LayoutName"].Value;
            }
            if (CtrlNode.Attributes["ComponentBehavior"] != null)
            {
                wptBehavior = CtrlNode.Attributes["ComponentBehavior"].Value;
            }
            if (CtrlNode.Attributes["ID"] != null)
            {
                id = CtrlNode.Attributes["ID"].Value;
                switch (typeName.ToLower())
                {
                    case "gridselectorcontrol":
                        wptGrid GridSelector;
                        if (this.Datasource != null && factoryMethod == "")
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                GridSelector = (wptGrid)ChildEditorControls[id];
                            }
                            else
                            {
                                GridSelector = new wptGrid((IBLListBase)Datasource);
                            }
                        }
                        else
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                GridSelector = (wptGrid)ChildEditorControls[id];
                            }
                            else
                            {
                                GridSelector = new wptGrid();
                            }
                            GridSelector.FactoryMethod = factoryMethod;
                            GridSelector.FactoryParameters = factoryParams;
                        }
                        GridSelector.ClassName = className;
                        GridSelector.LayoutName = layoutName;
                        GridSelector.ComponentBehaviorType = wptBehavior;
                        switch (wptBehavior.ToUpper())
                        {
                            case "SELECTOR":
                                GridSelector.ConsumerType = this.ConsumerType;
                                GridSelector.ReturnedProperty = returnedProperty;
                                foreach (string dk in SelectedDatakeys)
                                {
                                    GridSelector.SelectItem(dk);
                                }
                                PlaneFieldControls[GridSelector] = null;
                                break;
                            default:
                                break;
                        }
                        ChildEditorControls[id] = GridSelector;
                        Container.Controls.Add(GridSelector);
                        break;
                    case "treeselectorcontrol":
                        wptTreeView TreeSelector;
                        if (this.Datasource != null && factoryMethod == "")
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                TreeSelector = (wptTreeView)ChildEditorControls[id];
                            }
                            else
                            {
                                TreeSelector = new wptTreeView(Datasource);
                            }
                            TreeSelector.ReturnedProperty = returnedProperty;
                        }
                        else
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                TreeSelector = (wptTreeView)ChildEditorControls[id];
                            }
                            else
                            {
                                TreeSelector = new wptTreeView();
                            }
                            TreeSelector.FactoryMethod = factoryMethod;
                            TreeSelector.FactoryParameters = factoryParams;

                        }
                        TreeSelector.ClassName = className;
                        TreeSelector.LayoutName = layoutName;
                        TreeSelector.ComponentBehaviorType = wptBehavior;
                        switch (wptBehavior.ToUpper())
                        {
                            case "SELECTOR":
                                TreeSelector.ConsumerType = this.ConsumerType;
                                TreeSelector.ReturnedProperty = returnedProperty;
                                foreach (string dk in SelectedDatakeys)
                                {
                                    TreeSelector.SelectItem(dk);
                                }
                                PlaneFieldControls[TreeSelector] = null;
                                break;
                            default:
                                break;
                        }
                        ChildEditorControls[id] = TreeSelector;
                        Container.Controls.Add(TreeSelector);
                        break;
                    case "editorControl":
                        EnsureDatasourceObject();
                        wptBOEditor editor;
                        if (this.Datasource != null && factoryMethod == "")
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                editor = (wptBOEditor)ChildEditorControls[id];
                            }
                            else
                            {
                                editor = new wptBOEditor((BLBusinessBase)m_Datasource, null);
                            }
                        }
                        else
                        {
                            if (ChildEditorControls[id] != null)
                            {
                                editor = (wptBOEditor)ChildEditorControls[id];
                            }
                            else
                            {
                                editor = new wptBOEditor();
                            }
                            editor.FactoryMethod = factoryMethod;
                            editor.FactoryParameters = factoryParams;
                        }
                        if (className != null)
                        {
                            editor.ClassName = className;
                        }
                        else
                        {
                            editor.ClassName = ClassName;
                        }
                        editor.LayoutName = layoutName;
                        break;
                    default:
                        SetError(string.Format(Resources.GenericWebParts.strErrorControlTypeNotSupported, typeName));
                        break;
                }

            }
            else
            {
                SetError(Resources.ErrorMessages.errTagIDMissingInControl + typeName);
            }
        }

        protected override void CreateTabGroup(System.Xml.XmlNode TabGroup, object Datasource, System.Web.UI.Control nContainer, bool IsPostback, CommandEventHandler Handler, int DatasourceIndex)
        {
            AjaxControlToolkit.TabContainer tabContainer = new AjaxControlToolkit.TabContainer();
            if (TabGroup.Attributes["DoPostBack"] != null)
            {
                tabContainer.AutoPostBack = System.Convert.ToBoolean(TabGroup.Attributes["DoPostBack"].Value);
                tabContainer.ActiveTabChanged += new EventHandler(tabContainer_ActiveTabChanged);
            }

            SetAppareance(tabContainer, TabGroup);


            foreach (System.Xml.XmlNode TabPageNode in TabGroup.ChildNodes)
            {
                if (TabPageNode.Name.ToLower() == "tab")
                {
                    if ((TabPageNode.Attributes["SearchHystory"] != null) && (Convert.ToBoolean(TabPageNode.Attributes["SearchHystory"].Value)))
                    {
                        foreach (object key in SearchHystory.Keys)
                        {
                            BLCriteria crit = (BLCriteria)SearchHystory[key];
                            object Data = ReflectionHelper.GetSharedBusinessClassProperty(ClassName, "GetCollection", new object[] { crit });
                            TabBuilder(TabPageNode, Data, tabContainer, IsPostback, Handler, DatasourceIndex, key);
                        }
                    }
                    else
                    {
                        TabBuilder(TabPageNode, Datasource, tabContainer, IsPostback, Handler, DatasourceIndex, null);
                    }
                }
            }
            nContainer.Controls.Add(tabContainer);
            TabControlList.Add(tabContainer);
        }

        private void TabBuilder(XmlNode TabPageNode, object Datasource, TabContainer tabContainer, bool IsPostback, CommandEventHandler Handler, int DatasourceIndex, object searchKey)
        {
            string Label = TabPageNode.Attributes["Label"].Value;
            String IDadd = "";
            AjaxControlToolkit.TabPanel NewTab = new AjaxControlToolkit.TabPanel();
            SetAppareance(NewTab, TabPageNode);
            if (searchKey != null)
            {
                IDadd += " " + searchKey;
                NewTab.HeaderTemplate = new TabTemplate(this, TabPageNode, searchKey);
            }
            Label += IDadd;
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
                        if (InnerNode.Attributes["ID"] != null)
                        {
                            String tempID = InnerNode.Attributes["ID"].Value;
                            InnerNode.Attributes["ID"].Value = tempID + IDadd;
                            CreateControl(InnerNode, ContainerTable, Datasource, IsPostback);
                            InnerNode.Attributes["ID"].Value = tempID;
                        }
                        else
                        {
                            CreateControl(InnerNode, ContainerTable, Datasource, IsPostback);
                        }
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

        public void DeleteSearch(object key)
        {
            SearchHystory.Remove(key);
        }

        protected override void EnsureDatasourceObject()
        {
            if (m_Datasource == null)
            {
                m_Datasource = GetBusinessClassInstance(); //ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "TryGetObjectByGUID", new object[] { base.getKeyFieldValue(), null });// { base.ObjectGUID, null});
            }
            Datasource = (IBLListBase)m_Datasource;
        }

        public override void SetItemFields()
        {
            switch (ComponentBehaviorType.ToUpper())
            {
                //If the BehaviorType of the MultiTabbed is Selector, then it must coordinate all the selectors
                case "SELECTOR":
                    foreach (ISelectorProvider selector in PlaneFieldControls.Keys)
                    {
                        if (ConsumerType != null) //if the consumer type is not set
                        {
                            IBLListBase SelectedElements = (IBLListBase)selector.SelectedItems;
                            if (SelectedElements != null)
                            {
                                foreach (BLBusinessBase item in SelectedElements)
                                {
                                    if (!SelectedDatakeys.Contains(item.DataKey.ToString()))
                                    {
                                        SelectedDatakeys.Add(item.DataKey.ToString());
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        protected override void SaveChanges(string CommandName, string CommandArguments)
        {
        }

        protected override string GetXMLLayout()
        {
            Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout Layout;
            Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), LayoutName);
            if (Layout.LayoutXML == "")
            {
                Layout = Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties.clsUILayout.PropertyLayout(Type.GetType(ClassName), "Default");
                if (Layout.LayoutXML == "")
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
            }
            return Layout.LayoutXML;
        }

        public override bool HasDatasource()
        {//the Datasource for this control are the inner selectors
            return (m_Datasource != null);
        }

        public override bool IsNew()
        {
            return (((IBLListBase)m_Datasource).Count == 0);
        }
        #endregion

        #region ISelectorProvider Members

        public IList SelectedItems
        {
            get
            {
                if (m_SelectedItems == null || (m_SelectedItems.Count != SelectedDatakeys.Count))
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
            if (m_Datasource != null && ComponentBehaviorType.ToUpper() == "SELECTOR")
            {
                m_NotSelectedItems = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ClassName));
                m_SelectedItems = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ClassName));
                IBLListBase Datasource = (IBLListBase)m_Datasource;
                foreach (ISelectorProvider Selector in PlaneFieldControls.Keys)
                {
                    IBLListBase selectedelements = (IBLListBase)Selector.SelectedItems;
                    if (selectedelements != null)
                    {
                        foreach (BLBusinessBase item in selectedelements)
                        {
                            if (!SelectedDatakeys.Contains(item.DataKey.ToString()))
                            {
                                //SelectedItems.Add(item);
                                SelectedDatakeys.Add(item.DataKey.ToString());
                            }
                        }
                    }
                }
                foreach (BLBusinessBase businessObj in Datasource)
                {
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
            foreach (ISelectorProvider Selector in PlaneFieldControls.Keys)
            {
                Selector.SelectItem(ObjectDataKey);
            }
        }

        public void DeselectItem(string ObjectDatakey)
        {
            SelectedDatakeys.Remove(ObjectDatakey.ToString());
            foreach (ISelectorProvider Selector in PlaneFieldControls.Keys)
            {
                Selector.DeselectItem(ObjectDatakey);
            }
        }

        #endregion


        [Category("Behavior")]
        [WebBrowsable(true)]
        [Browsable(true)]
        [Personalizable(PersonalizationScope.Shared)]
        [XmlElement(ElementName = "Popupwindow")]
        [DefaultValue("")]
        [WebDisplayName("Is Popup Window:")]
        [WebDescription("True if this webpart is used to show as popupwindow (only one can exist per page).")]
        public Boolean IsPopupWindow
        {
            get
            {
                if (ViewState["IsPopupWindow"] == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)ViewState["IsPopupWindow"];
                }
            }
            set
            {
                ViewState["IsPopupWindow"] = value;
                if (value)
                {
                    this.AsyncUpdates = true;

                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            m_BtnLayoutChanger.Click += new EventHandler(m_BtnLayoutChanger_Click);
        }

        void m_BtnLayoutChanger_Click(object sender, EventArgs e)
        {
            LayoutName = m_hdfField.Value;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (IsPopupWindow)
            {
                string Script = "<script type=\"text/javascript\" language=\"javascript\">SetPopupManagerHDFS(\"" + m_hdfField.ClientID + "\",\"" + BaseClasses.clsCtrlWebPartBase.UniqueIDWithDollars(m_BtnLayoutChanger) + "\" )</script>";

                System.Web.UI.ScriptManager.RegisterStartupScript(this, this.GetType(), "popupsetting", Script, false);
                this.AsyncPanel.UpdateMode = System.Web.UI.UpdatePanelUpdateMode.Conditional;
                this.AsyncPanel.ChildrenAsTriggers = true;
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            base.Render(writer);
            Page.ClientScript.RegisterForEventValidation(m_BtnLayoutChanger.UniqueID);
        }

        [ConnectionProvider("Context Provider", "ContextProvider")]
        public override clsBaseLayoutWP GetContextProvider()
        {
            return ActiveControl;
        }

        [ConnectionProvider("Selector Provider", "MTabbedSelector")]
        public ISelectorProvider GetSelectorProvider()
        {
            return this;
        }

        [ConnectionConsumer("Search Consumer", "SearchTabbedConsumer")]
        public void RegisterSearchProvider(ISearchProvider Provider)
        {
            BLCriteria criteria = Provider.Criteria;
            if (criteria != null)
            {
                SearchHystory[searchCounter] = criteria;
                searchCounter++;
                this.DataBind();
            }
        }

        internal override ArrayList ContextMenuLayouts
        {
            get
            {
                //first, the webpart must search in the controls if it has a tabgroup, then see what's the currently selected tab
                //and get the provider of it
                ArrayList Result = new ArrayList();
                foreach (AjaxControlToolkit.TabContainer Container in TabControlList)
                {
                    AjaxControlToolkit.TabPanel selectedPanel = Container.ActiveTab;
                    getSubContextMenuLayouts(selectedPanel, Result);
                }
                return Result;
            }
            set
            {
                base.ContextMenuLayouts = value;
            }
        }

        private void getSubContextMenuLayouts(WebControl webcontrol, ArrayList Result)
        {
            if (typeof(clsBaseLayoutWP).IsAssignableFrom(webcontrol.GetType()))
            {
                ActiveControl = (clsBaseLayoutWP)webcontrol;
                Result.AddRange(((clsBaseLayoutWP)webcontrol).ContextMenuLayouts);
            }
            else
            {
                //the control is a container
                if (webcontrol.Controls.Count > 0)
                {
                    foreach (WebControl wctrl in webcontrol.Controls)
                    {
                        getSubContextMenuLayouts(wctrl, Result);
                    }
                }
            }
        }

        internal override bool EnabledCommands
        {
            get
            {
                //first, the webpart must search in the controls if it has a tabgroup, then see what's the currently selected tab
                //and get the provider of it
                bool Result = false;
                foreach (AjaxControlToolkit.TabContainer Container in TabControlList)
                {
                    AjaxControlToolkit.TabPanel selectedPanel = Container.ActiveTab;
                    Result = getEnabledCommands(selectedPanel);
                }
                return Result;
            }
            set
            {
                base.EnabledCommands = value;
            }
        }

        private bool getEnabledCommands(WebControl webcontrol)
        {
            if (typeof(clsBaseLayoutWP).IsAssignableFrom(webcontrol.GetType()))
            {
                ActiveControl = (clsBaseLayoutWP)webcontrol;
                return ActiveControl.EnabledCommands;
            }
            else
            {
                //the control is a container
                bool Result = false;
                if (webcontrol.Controls.Count > 0)
                {
                    foreach (WebControl wctrl in webcontrol.Controls)
                    {
                        Result = getEnabledCommands(wctrl);
                    }
                    return Result;
                }
                else
                {
                    return false;
                }
            }
        }

        protected override void CreateButtons(object Datasource, System.Web.UI.Control nCotainer)
        {

        }

        #region ISelectorProvider Members

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

        
    }
}
