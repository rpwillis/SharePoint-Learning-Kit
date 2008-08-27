using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerUITools.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerUITools.BaseClasses;
using Axelerate.BusinessLayerUITools.Interfaces;
using System.Collections;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;
using Axelerate.BusinessLogic.SharedBusinessLogic.Trace;
using System.Xml;


namespace Axelerate.BusinessLayerUITools.WebParts
{
    public class wptTreeView : clsBaseTreeview, ISelectorProvider
    {
        #region Constructors
        public wptTreeView()
            : base()
        {
        }

        public wptTreeView(Object Datasource)
            : base()
        {
            m_Datasource = Datasource;
        }
        #endregion

        #region Private Members
        private TreeView mainTreeView;
        private IList m_SelectedItems;
        private IList m_NodesDatasource;

        private IList NodesDatasource
        {
            get
            {
                if (m_NodesDatasource == null)
                {
                    if (ViewState["DatasourceNodes"] == null)
                    {
                        ViewState["DatasourceNodes"] = System.Activator.CreateInstance(Type.GetType(ClassName));
                    }
                    m_NodesDatasource = (IList)ViewState["DatasourceNodes"];
                }
                return m_NodesDatasource;
            }
            set
            {
                m_NodesDatasource = value;
                ViewState["DatasourceNodes"] = value;
            }
        }

        private ArrayList m_SelectedDatakeys;
        private ArrayList SelectedDatakeys
        {
            get
            {
                if (ViewState["SelectedDatakeys"] == null)
                {
                    ViewState["SelectedDatakeys"] = new ArrayList();
                }
                m_SelectedDatakeys = (ArrayList)ViewState["SelectedDatakeys"];
                return m_SelectedDatakeys;
            }
            set
            {
                ViewState["SelectedDatakeys"] = value;
                m_SelectedDatakeys = value;
            }
        }
        #endregion

        #region Method Overrides
        protected override void CreateInnerControl(object Datasource, System.Xml.XmlNode CtrlNode, System.Web.UI.Control Container, bool AsRadioButton)
        {

        }

        protected override void EnsureDatasourceObject()
        {
            if (m_Datasource == null)
            {
                Object source = GetBusinessClassInstance(); //ReflectionHelper.GetSharedBusinessClassProperty(base.ClassName, "TryGetObjectByGUID", new object[] { base.getKeyFieldValue(), null });// { base.ObjectGUID, null});
                if (source == null)
                {
                    if (Convert.ToBoolean(clsConfigurationProfile.Current.getPropertyValue("ThrowExceptions")))
                    {
                        throw new Exception(Resources.ErrorMessages.errInvalidDatasource);
                    }
                    SetError(Resources.ErrorMessages.errInvalidDatasource);
                }
                Datasource = source;
            }
            Datasource = m_Datasource;
        }



        public override void SetItemFields()
        {

        }

        protected void addChilds(TreeNode SelectedNode, System.Collections.ArrayList SelectedNodes)
        {
            if (SelectedNode.ChildNodes.Count == 0) //If node is leaf add it to the hashtable
            {
                if (!SelectedNodes.Contains(SelectedNode.Value))
                {
                    SelectItem(SelectedNode.Value);
                    //SelectedDatakeys.Add(SelectedNode.Value);
                }
            }
            else
            {
                foreach (TreeNode childNode in SelectedNode.ChildNodes)
                {
                    addChilds(childNode, SelectedNodes);
                }
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
        {
            return (m_Datasource != null);
        }

        public override bool IsNew()
        {
            return ((BLBusinessBase)m_Datasource).IsNew;
        }

        

        protected override void CreateTreeView(System.Xml.XmlNode parentNode, System.Web.UI.Control nContainer, object Datasource, bool IsPostback)
        {
            EnsureDatasourceObject();
            //Get the child collection of the Inode or the static node.
            //It is the first node.
            if (parentNode.Name.ToUpper() == "NODECOLLECTION")
            {
                if (mainTreeView == null)
                {

                    mainTreeView = new TreeView();
                    mainTreeView.EnableViewState = true;
                    mainTreeView.ShowLines = true;
                    if (ComponentBehaviorType.ToUpper() == "SELECTOR")
                    {
                        mainTreeView.SelectedNodeChanged += new EventHandler(mainTreeView_SelectedNodeChanged);
                        NodesDatasource = (IList)System.Activator.CreateInstance(Type.GetType(ConsumerType));
                    }
                    SetAppareance(mainTreeView, parentNode);
                    foreach (System.Xml.XmlNode gNode in parentNode.ChildNodes)
                    {
                        CreateInnerNodes(gNode, mainTreeView.Nodes, Datasource);
                    }
                }
                nContainer.Controls.Add(mainTreeView);
                if (mainTreeView.SelectedNode != null)
                {
                    mainTreeView.SelectedNode.ExpandAll();
                }
            }
        }

        void mainTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            if (IsPostback)
            {
                if (mainTreeView.SelectedNode != null)
                {
                    mainTreeView.SelectedNode.ExpandAll();
                    addChilds(mainTreeView.SelectedNode, SelectedDatakeys);
                }
            }
        }

        [ConnectionProvider("Selector Provider", "TreeSelector")]
        public ISelectorProvider GetSelectorProvider()
        {
            return this;
        }

        private void CreateInnerNodes(System.Xml.XmlNode gXMLNode, TreeNodeCollection ParentTreeViewNodeCollection, object Datasource)
        {
            TreeNode elementNode;
            if (gXMLNode.Name.ToUpper() == "STATICNODE")
            {
                elementNode = new TreeNode();
                if (gXMLNode.Attributes["Text"] != null)
                {
                    elementNode.Text = gXMLNode.Attributes["Text"].Value;
                }
                loadXMLNodeStyle(gXMLNode, elementNode, "StaticTreeNode", null);
                ParentTreeViewNodeCollection.Add(elementNode);
                foreach (System.Xml.XmlNode gInnerXmlNode in gXMLNode.ChildNodes)
                {
                    CreateInnerNodes(gInnerXmlNode, elementNode.ChildNodes, Datasource);
                }
            }
            else if (gXMLNode.Name.ToUpper() == "INODE")
            {
                //First check if the datasource is a collection
                if (typeof(IList).IsAssignableFrom(Datasource.GetType()))
                {
                    IList INodeCollection = (IList)Datasource;
                    foreach (INode iNodeObject in INodeCollection)
                    {
                        CreateInnerNodes(gXMLNode, ParentTreeViewNodeCollection, iNodeObject);
                    }
                }
                else if (typeof(BLBusinessBase).IsAssignableFrom(Datasource.GetType()))
                {
                    INode INodeDatasource = (INode)Datasource;
                    if (gXMLNode.Attributes["IncludeNode"] == null || gXMLNode.Attributes["IncludeNode"].Value.ToUpper() == "TRUE")
                    {
                        BLBusinessBase BLDatasource = ((BLBusinessBase)Datasource);
                        elementNode = new TreeNode();
                        elementNode.Text = ((INode)Datasource).Name;
                        if (ReturnedProperty != null)
                        {
                            elementNode.Value = BLDatasource.GetType().AssemblyQualifiedName + "|" + BLDatasource.DataKey.ToString();
                        }
                        else
                        {
                            elementNode.Value = BLDatasource.DataKey.ToString();
                        }
                        if (gXMLNode.Attributes["ShowImage"] != null && gXMLNode.Attributes["ShowImage"].Value.ToUpper() == "TRUE")
                        {
                            elementNode.ImageUrl = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.getImageUrl(INodeDatasource);
                        }
                        loadXMLNodeStyle(gXMLNode, elementNode, Datasource.GetType().FullName, INodeDatasource);
                        elementNode.NavigateUrl = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.parseBOPropertiesString(BLDatasource, elementNode.NavigateUrl, BLDatasource.GetType().AssemblyQualifiedName, "GUID");
                        elementNode.ImageUrl = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.parseBOPropertiesString(BLDatasource, elementNode.ImageUrl, BLDatasource.GetType().AssemblyQualifiedName, "GUID");
                        ParentTreeViewNodeCollection.Add(elementNode);
                        if (gXMLNode.Attributes["TreeSelector"] != null && INodeDatasource.HasChilds(gXMLNode.Attributes["TreeSelector"].Value))
                        {
                            foreach (INode childElement in INodeDatasource.Childs(gXMLNode.Attributes["TreeSelector"].Value))
                            {
                                CreateInnerNodes(gXMLNode, elementNode.ChildNodes, childElement);
                            }
                        }
                        //If the Tree is a selector, then add the item to the NodesDatasource List.
                        if (ComponentBehaviorType.ToUpper() == "SELECTOR")
                        {
                            BLBusinessBase Element = null;
                            if (ReturnedProperty == null)
                            {
                                Element = (BLBusinessBase)Datasource;
                            }
                            else
                            {
                                if (Datasource.GetType().GetMember(ReturnedProperty).Length > 0)
                                {
                                    Element = (BLBusinessBase)((BLBusinessBase)Datasource)[ReturnedProperty];
                                }
                            }
                            if (Element != null)
                            {
                                IBLListBase DestinyCollection = (IBLListBase)System.Activator.CreateInstance(Type.GetType(ConsumerType));
                                if (Element.GetType().Name == DestinyCollection.DataLayer.BusinessLogicType.Name)
                                {
                                    // Element.MarkAsChild();
                                    NodesDatasource.Add(Element);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (gXMLNode.Attributes["TreeSelector"] != null && INodeDatasource.HasChilds(gXMLNode.Attributes["TreeSelector"].Value))
                        {
                            foreach (INode childElement in INodeDatasource.Childs(gXMLNode.Attributes["TreeSelector"].Value))
                            {
                                CreateInnerNodes(gXMLNode, ParentTreeViewNodeCollection, childElement);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the style attributes of the gXMLNode to the properties of elementNode
        /// </summary>
        /// <param name="gXMLNode"></param>
        /// <param name="elementNode"></param>
        private void loadXMLNodeStyle(System.Xml.XmlNode gXMLNode, TreeNode elementNode, string ClassType, INode INodeDatasource)
        {
            System.Xml.XmlNode styleNode = (System.Xml.XmlNode)TreeViewNodeStyles[ClassType];
            // CssClass="" Style="" ShowChilds="false"
            if (styleNode != null)
            {
                if (styleNode.Attributes["ImageUrl"] != null)
                {
                    elementNode.ImageUrl = styleNode.Attributes["ImageUrl"].Value;
                }
                if (styleNode.Attributes["NavigateUrl"] != null)
                {
                    elementNode.NavigateUrl = styleNode.Attributes["NavigateUrl"].Value;
                }
                if (styleNode.Attributes["Target"] != null)
                {
                    elementNode.Target = styleNode.Attributes["Target"].Value;
                }
                if (styleNode.Attributes["ShowCheckBox"] != null)
                {
                    elementNode.ShowCheckBox = System.Convert.ToBoolean(styleNode.Attributes["ShowCheckBox"].Value);
                }
                if (styleNode.Attributes["ImageTooltip"] != null)
                {
                    elementNode.ImageToolTip = styleNode.Attributes["ImageTooltip"].Value;
                }
                if (styleNode.Attributes["ShowImage"] != null && styleNode.Attributes["ShowImage"].Value.ToUpper() == "TRUE" && INodeDatasource != null)
                {
                    elementNode.ImageUrl = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.getImageUrl(INodeDatasource);
                }
                //if (styleNode.Attributes["ImageTooltip"] != null)
                //{
                //    // = styleNode.Attributes["ImageTooltip"].Value;
                //}
                //if (styleNode.Attributes["CssClass"] != null)
                //{
                //    //elementNode.css = styleNode.Attributes["CssClass"].Value;
                //}
            }
            if (gXMLNode.Attributes["ImageUrl"] != null)
            {
                elementNode.ImageUrl = gXMLNode.Attributes["ImageUrl"].Value;
            }
            if (gXMLNode.Attributes["NavigateUrl"] != null)
            {
                elementNode.NavigateUrl = gXMLNode.Attributes["NavigateUrl"].Value;
            }
            if (gXMLNode.Attributes["ShowCheckBox"] != null)
            {
                elementNode.ShowCheckBox = System.Convert.ToBoolean(gXMLNode.Attributes["ShowCheckBox"].Value);
            }
            if (gXMLNode.Attributes["ImageTooltip"] != null)
            {
                elementNode.ImageToolTip = gXMLNode.Attributes["ImageTooltip"].Value;
            }
            if (gXMLNode.Attributes["ShowImage"] != null && gXMLNode.Attributes["ShowImage"].Value.ToUpper() == "TRUE")
            {
                elementNode.ImageUrl = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.getImageUrl(INodeDatasource);
            }
            //if (gXMLNode.Attributes["CssClass"] != null)
            //{
            //    elementNode.ImageToolTip = gXMLNode.Attributes["CssClass"].Value;
            //}
        }

        protected override void CreateButtons(object Datasource, System.Web.UI.Control nCotainer)
        {

        }
        #endregion

        #region Properties
        public Object Datasource
        {
            get
            {
                m_Datasource = ViewState["TreeDatasource"];
                return m_Datasource;
            }
            set
            {
                ViewState["TreeDatasource"] = value;
                m_Datasource = value;

            }
        }
        #endregion

        private void GetCheckedNodes(TreeNodeCollection Nodes, ArrayList CheckedNodes)
        {
            foreach (TreeNode node in Nodes)
            {
                if (node.CheckState == Checkbox.CheckboxState.Checked)
                {
                    CheckedNodes.Add(node.Value);
                }
                if (node.ChildNodes.Count > 0)
                {
                    GetCheckedNodes(node.ChildNodes, CheckedNodes);
                }
            }
        }

        private void GetUnCheckedNodes(TreeNodeCollection Nodes, ArrayList CheckedNodes)
        {
            foreach (TreeNode node in Nodes)
            {
                if (node.CheckState == Checkbox.CheckboxState.Unchecked)
                {
                    CheckedNodes.Add(node.Value);
                }
                if (node.ChildNodes.Count > 0)
                {
                    GetUnCheckedNodes(node.ChildNodes, CheckedNodes);
                }
            }
        }

        private void GetNotCheckedNodes(TreeNodeCollection Nodes, ArrayList CheckedNodes)
        {
            foreach (TreeNode node in Nodes)
            {
                if (node.CheckState == Checkbox.CheckboxState.Undefine)
                {
                    CheckedNodes.Add(node.Value);
                }
                if (node.ChildNodes.Count > 0)
                {
                    GetNotCheckedNodes(node.ChildNodes, CheckedNodes);
                }
            }
        }

        #region ISelectorProvider Members

        public IList SelectedItems
        {
            get
            {
                if (m_SelectedItems == null || m_SelectedItems.Count == 0)
                {
                    InitializeItems();

                }
                return m_SelectedItems;
                
            }
        }
        
        public IList CheckedItems
        {
            get
            {
                ArrayList Checkeditems = new ArrayList();
                if (mainTreeView != null)
                {
                    GetCheckedNodes(mainTreeView.Nodes, Checkeditems);
                }
                return Checkeditems;
            }
        }

        public IList NotSelectedItems
        {
            get
            {
                return (IList)Datasource;
            }
        }
        
        public IList NotCheckedItems
        {
            get
            {
                ArrayList NotCheckeditems = new ArrayList();
                if (mainTreeView != null)
                {
                    GetNotCheckedNodes(mainTreeView.Nodes, NotCheckeditems);
                }
                return NotCheckeditems;
            }
        }

        public IList UnCheckedItems
        {
            get
            {
                ArrayList UnCheckeditems = new ArrayList();
                if (mainTreeView != null)
                {
                    GetUnCheckedNodes(mainTreeView.Nodes, UnCheckeditems);
                }
                return UnCheckeditems;
            }
        }

        private void InitializeItems()
        {
            if (ComponentBehaviorType.ToUpper() == "SELECTOR")
            {
                m_SelectedItems = (IList)System.Activator.CreateInstance(Type.GetType(ConsumerType));

                if (NodesDatasource != null)
                {
                    foreach (BLBusinessBase businessObj in NodesDatasource)
                    {
                        if (ReturnedProperty != null)
                        {
                            object property = businessObj;
                            if (typeof(IList).IsAssignableFrom(property.GetType()))
                            {
                                IList prop = (IList)property;

                                if (SelectedDatakeys.Contains(businessObj.DataKey.ToString()))
                                {
                                    foreach (BLBusinessBase obj in prop)
                                    {
                                        // obj.MarkAsChild();

                                        m_SelectedItems.Add(obj);
                                    }
                                }
                            }
                            else
                            {
                                BLBusinessBase prop = (BLBusinessBase)property;
                                // prop.MarkAsChild();
                                if (SelectedDatakeys.Contains(businessObj.DataKey.ToString()))
                                {
                                    m_SelectedItems.Add(prop);
                                }
                            }
                        }
                        else
                        {
                            if (SelectedDatakeys.Contains(businessObj.DataKey.ToString()))
                            {
                                // businessObj.MarkAsChild();
                                m_SelectedItems.Add(businessObj);
                            }
                        }
                    }
                }
            }
        }

        public string ReturnedProperty
        {
            get
            {
                if (ViewState["TreeViewReturnedProperty"] != null)
                {
                    return ViewState["TreeViewReturnedProperty"].ToString();
                }
                else return null;
            }
            set
            {
                ViewState["TreeViewReturnedProperty"] = value;
            }
        }

        public void SelectItem(string ObjectDataKey)
        {
            m_SelectedItems = null;
            if (!MultiSelect)
            {
                SelectedDatakeys.Clear();
            }
            if (ReturnedProperty != null)
            {
                ///TODO: Build a DatakeyObject, then use GetObject(Datakey) to get the object and then get the property
                string[] strClassAndDatakey = ObjectDataKey.Split(new char[] { '|' });
                if (strClassAndDatakey.Length == 2)
                {
                    BLDataKey dk = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.getDatakeyFromString(strClassAndDatakey[0], strClassAndDatakey[1]);
                    if (dk != null)
                    {
                        BLBusinessBase DatasourceObject = (BLBusinessBase)ReflectionHelper.GetSharedBusinessClassProperty(strClassAndDatakey[0], "GetObject", new object[] { dk });
                        if (DatasourceObject.GetType().GetMember(ReturnedProperty).Length > 0)
                        {
                            //This assumes that the returned property will be a BLBusinessBase
                            if (!(SelectedDatakeys.Contains(((BLBusinessBase)DatasourceObject[ReturnedProperty]).DataKey.ToString())))
                            {
                                SelectedDatakeys.Add(((BLBusinessBase)DatasourceObject[ReturnedProperty]).DataKey.ToString());
                            }
                        }
                        else
                        {
                            SetError(string.Format(Resources.ErrorMessages.errNotHasProperty,ReturnedProperty));
                        }
                    }
                    //SetError("Invalid Datakey");
                }
                else
                {
                    if (!(SelectedDatakeys.Contains(ObjectDataKey)))
                    {
                        SelectedDatakeys.Add(ObjectDataKey);
                    }
                }
            }
            else
            {
                if (!(SelectedDatakeys.Contains(ObjectDataKey)))
                {
                    SelectedDatakeys.Add(ObjectDataKey);
                }
            }
        }

        /*private BLDataKey GetDatakey(string ClassName, string DataKeyString)
        {
            BLBusinessBase DatakeyOwner = System.Activator.CreateInstance(Type.GetType(ClassName));
            
            DatakeyOwner.DataKey.GetType().GetProperty("").SetValue = 
        }*/

        public void DeselectItem(string ObjectDatakey)
        {
            m_SelectedItems = null;
            SelectedDatakeys.Remove(ObjectDatakey);
        }

        [Browsable(true), Category("Behavior"), DefaultValue(true),
         Description("Set or Get if the control allow do multiselection")]
        [Personalizable(PersonalizationScope.Shared)]
        [WebDisplayName("Allow MultiSelect")]
        [WebDescription("Set or Get if the control allow do multiselection")]
        [WebBrowsable(true)]
        public bool MultiSelect
        {
            get
            {
                if (ViewState["MultiSelect"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["MultiSelect"];
                }
            }
            set
            {
                ViewState["MultiSelect"] = value;
            }
        }
        public string ConsumerType
        {
            get
            {
                if (ViewState["TreeConsumerType"] == null)
                {
                    return null;
                }
                else
                {
                    return (string)ViewState["TreeConsumerType"];
                }
            }
            set
            {
                ViewState["TreeConsumerType"] = value;
            }
        }

        #endregion

        #region ISelectorProvider Members


       

        #endregion
    }
}
