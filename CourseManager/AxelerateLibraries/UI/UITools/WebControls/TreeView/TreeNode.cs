using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.ComponentModel;
using System.Security.Permissions;
using Axelerate.BusinessLayerUITools.WebControls;
using System.Drawing.Design;
using System.Web.UI.WebControls;

namespace Axelerate.BusinessLayerUITools.WebControls
{


    /// <summary>
    /// Indicates how a TreeNode should handle expanding and the plus sign.
    /// </summary>
    public enum ExpandableValue
    {
        /// <summary>
        /// Always shows a plus sign and attempts to expand.
        /// </summary>
        Always,

        /// <summary>
        /// Shows a plus sign and allows expanding only when there are children.
        /// </summary>
        Auto
    };

    /// <summary>
    /// Class to eliminate runat="server" requirement and restrict content to approved tags
    /// </summary>
    //internal class TreeNodeControlBuilder : ControlBuilder
    //{
    //    private System.Collections.Hashtable _TagTypeTable;    // Tag to Type relationship table

    //    protected void FillTagTypeTable()
    //    {
    //        Add("treenode", typeof(TreeNode));
    //    }

    //    public TreeNodeControlBuilder(): base()
    //    {
    //        // Create the table
    //        _TagTypeTable = new System.Collections.Hashtable();

    //        // Fill the table with tag to type relationships
    //        FillTagTypeTable();
    //    }

    //    /// <summary>
    //    /// Adds a tagname to type entry.
    //    /// </summary>
    //    /// <param name="tagName">The tag name.</param>
    //    /// <param name="type">The type.</param>
    //    public void Add(string tagName, Type type)
    //    {
    //        _TagTypeTable.Add(tagName.ToLower(), type);
    //    }

    //    /// <summary>
    //    /// Determines a type given a tag name.
    //    /// </summary>
    //    /// <param name="tagName">The tagname.</param>
    //    /// <param name="attribs">Attributes.</param>
    //    /// <returns>The type of the tag.</returns>
    //    public override Type GetChildControlType(string tagName, System.Collections.IDictionary attribs)
    //    {
    //        // Let the base class have the tagname
    //        Type baseType = base.GetChildControlType(tagName, attribs);
    //        if (baseType != null)
    //        {
    //            // If the type returned is valid, then return it
    //            if (_TagTypeTable.ContainsValue(baseType))
    //            {
    //                return baseType;
    //            }
    //        }

    //        // Allows children without runat=server to be added
    //        // and to limit to specific types

    //        string szTagName = tagName.ToLower();
    //        int colon = szTagName.IndexOf(':');
    //        if ((colon >= 0) && (colon < (szTagName.Length + 1)))
    //        {
    //            // Separate the tagname from the namespace
    //            szTagName = szTagName.Substring(colon + 1, szTagName.Length - colon - 1);
    //        }

    //        // Find Type associated with tagname
    //        Object obj = _TagTypeTable[szTagName];

    //        // Return the Type if found
    //        if ((obj != null) && (obj is Type))
    //        {
    //            return (Type)obj;
    //        }

    //        // No Type was found, throw an exception
    //        throw new Exception(String.Format(Resources.ErrorMessages.errInvalidChildTagName, tagName));
    //    }


    //    /// <summary>
    //    /// Rejects appending literal strings.
    //    /// </summary>
    //    /// <param name="s">The string.</param>
    //    public override void AppendLiteralString(string s)
    //    {
    //        if (AllowLiterals())
    //        {
    //            base.AppendLiteralString(s);
    //        }
    //        else
    //        {
    //            s = s.Trim();
    //            if (s != String.Empty)
    //            {
    //                throw new Exception(String.Format(Resources.ErrorMessages.errInvalidLiteralString, s));
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Allows subclasses to override the rejection of literal strings.
    //    /// </summary>
    //    /// <returns>false to reject literals.</returns>
    //    public virtual bool AllowLiterals()
    //    {
    //        // Ignore all literals
    //        return false;
    //    }

    //    /// <summary>
    //    /// Rejects whitespace.
    //    /// </summary>
    //    /// <returns>false to reject whitespace.</returns>
    //    public override bool AllowWhitespaceLiterals()
    //    {
    //        // Ignore whitespace literals
    //        return false;
    //    }
    //}
    internal class TreeNodeControlBuilder : ControlBuilder
    {
        public override Type GetChildControlType(string tagName, System.Collections.IDictionary attribs)
        {
            // override to allow TreeNode without runat=server to be added
            if (tagName.ToLower().EndsWith("treenode"))
                return typeof(TreeNode);

            return null;
        }

        public override void AppendLiteralString(string s)
        {
            // override to ignore literals between items
        }
    }



    [
    ControlBuilderAttribute(typeof(TreeNodeControlBuilder)),
    ToolboxItem(false),
    ParseChildren(true, "ChildNodes") 
    ]
    public class TreeNode : IStateManager, ICloneable//, IParserAccessor
    {
        #region Private Properties
        
        internal System.Web.UI.WebControls.Panel _NodeElement = null;
        private bool _IsTrackingViewState = false;
        private StateBag _ViewState;
        internal Object _Parent = null;
        private System.Web.UI.WebControls.HiddenField _State;
        private System.Web.UI.WebControls.HiddenField _Expanded;
        private System.Web.UI.WebControls.Image _imgCheck;
        private System.Web.UI.WebControls.Label _lblCheck;
        internal TreeNodeCollection _Nodes;
        private int _level;
        private TreeView _treeview;     // parent
        private System.Web.UI.WebControls.Panel _ChildNodes = null;
        private System.Web.UI.WebControls.Image _CollapseExpandImg = null;
        private Style _NodeStyle = new Style();

        #endregion

        #region IStateManager Members

        public bool IsTrackingViewState
        {
            get { return _IsTrackingViewState; }
        }

        /// <summary>
        /// Loads the node's previously saved view state.
        /// </summary>
        /// <param name="state">An Object that contains the saved view state values for the node.</param>
        void IStateManager.LoadViewState(object state)
        {
            ((TreeNode)this).LoadViewState(state);
        }

        /// <summary>
        /// Loads the node's previously saved view state.
        /// </summary>
        /// <param name="state">An Object that contains the saved view state values for the node.</param>
        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] state = (object[])savedState;

                ((IStateManager)ViewState).LoadViewState(state[0]);
                ((IStateManager)ChildNodes).LoadViewState(state[1]);
                ((IStateManager)_NodeStyle).LoadViewState(state[2]);

            }
        }

        /// <summary>
        /// Saves the changes to the node's view state to an Object.
        /// </summary>
        /// <returns>The Object that contains the view state changes.</returns>
        object IStateManager.SaveViewState()
        {
            return ((TreeNode)this).SaveViewState();
        }

        /// <summary>
        /// Saves the changes to the node's view state to an Object.
        /// </summary>
        /// <returns>The Object that contains the view state changes.</returns>
        protected virtual object SaveViewState()
        {
            object SavedState = null;
            if (_ViewState != null)
            {
                SavedState = ((IStateManager)_ViewState).SaveViewState();
            }
            object[] state = new object[]
            {
                SavedState,
                ((IStateManager)ChildNodes).SaveViewState(),
                ((IStateManager)_NodeStyle).SaveViewState(),
            };
            
            // Check to see if we're really saving anything
            foreach (object obj in state)
            {
                if (obj != null)
                {
                    return state;
                }
            }
            return null;
        }

        /// <summary>
        /// Instructs the node to track changes to its view state.
        /// </summary>
        void IStateManager.TrackViewState()
        {
            ((TreeNode)this).TrackViewState();
        }

        /// <summary>
        /// Instructs the node to track changes to its view state.
        /// </summary>
        protected virtual void TrackViewState()
        {
            _IsTrackingViewState = true;

            if (_ViewState != null)
            {
                ((IStateManager)_ViewState).TrackViewState();
            }
            ((IStateManager)ChildNodes).TrackViewState();
        }

        /// <summary>
        /// Sets all items within the StateBag to be dirty
        /// </summary>
        protected internal virtual void SetViewStateDirty()
        {
            if (_ViewState != null)
            {
                foreach (StateItem item in ViewState.Values)
                {
                    item.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Sets all items within the StateBag to be clean
        /// </summary>
        protected internal virtual void SetViewStateClean()
        {
            if (_ViewState != null)
            {
                foreach (StateItem item in ViewState.Values)
                {
                    item.IsDirty = false;
                }
            }
        }

        /// <summary>
        /// An instance of the StateBag class that contains the view state information.
        /// </summary>
        protected StateBag ViewState
        {
            get
            {
                // To concerve resources, especially on the page,
                // only create the view state when needed.
                if (_ViewState == null)
                {
                    _ViewState = new StateBag();
                    if (((IStateManager)this).IsTrackingViewState)
                    {
                        ((IStateManager)_ViewState).TrackViewState();
                    }
                }

                return _ViewState;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            TreeNode copy = (TreeNode)Activator.CreateInstance(this.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);

            // Merge in the properties from this object into the copy
            copy._IsTrackingViewState = this._IsTrackingViewState;

            if (this._ViewState != null)
            {
                StateBag viewState = copy.ViewState;
                foreach (string key in this.ViewState.Keys)
                {
                    object item = this.ViewState[key];
                    if (item is ICloneable)
                    {
                        item = ((ICloneable)item).Clone();
                    }

                    viewState[key] = item;
                }
            }

            return copy;
        }

        #endregion

        #region Constructor
        public TreeNode()
        {
            _State = new System.Web.UI.WebControls.HiddenField();
            _Expanded = new System.Web.UI.WebControls.HiddenField();
            _imgCheck = new System.Web.UI.WebControls.Image();
            _lblCheck = new System.Web.UI.WebControls.Label();
            _Nodes = new TreeNodeCollection(this);
            _level = -1;
            _treeview = null;
        }
        #endregion

        #region TreeNode Properties

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String CheckedImageUrl
        {
            get
            {
                if (ViewState["_CheckedImageUrl"] == null)
                {
                    return ((this._treeview != null) ? this._treeview.CheckedImageUrl : String.Empty);
                }
                else
                {
                    return (String)ViewState["_CheckedImageUrl"];
                }
            }
            set
            {
                ViewState["_CheckedImageUrl"] = value;
            }
        }

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String UncheckedImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return ((this._treeview != null) ? this._treeview.UncheckedImageUrl : String.Empty);
                }
                else
                {
                    return (String)ViewState["_UncheckedImageUrl"];
                }
            }
            set
            {
                ViewState["_UncheckedImageUrl"] = value;
            }
        }

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        public String UndefineImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return ((this._treeview != null) ? this._treeview.UndefineImageUrl : String.Empty);
                }
                else
                {
                    return (String)ViewState["_UndefineImageUrl"];
                }
            }
            set
            {
                ViewState["_UndefineImageUrl"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the node's check box state
        /// </summary>
        public Checkbox.CheckboxState CheckState
        {
            get
            {
                if (_State.Value == null)
                {
                    _State.Value = Checkbox.CheckboxState.Undefine.ToString();
                }
                return (Checkbox.CheckboxState)Enum.Parse(typeof(Checkbox.CheckboxState), _State.Value);

            }
            set
            {
                _State.Value = value.ToString();
                SetCheckImg(_imgCheck, value);
            }
        }


        /// <summary>
        /// Returns the parent object.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Object Parent
        {
            get
            {
                return _Parent;
            }
        }

        /// <summary>
        /// The ID of the node.
        /// </summary>
        [DefaultValue("")]
        [ParenthesizePropertyName(true)]
        [Description("ID of the TreeNode")]
        public virtual string ID
        {
            get
            {
                object obj = ViewState["ID"];
                return (obj == null) ? String.Empty : (string)obj;
            }

            set { ViewState["ID"] = value; }
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
            if (ID != String.Empty)
            {
                return ID;
            }
            else
            {
                return this.GetType().Name;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Description("Node Style"),
        ]
        public System.Web.UI.WebControls.Style NodeStyle
        {
            get
            {
                if (_NodeStyle == null)
                {
                    _NodeStyle = new Style();
                    if (IsTrackingViewState)
                        ((IStateManager)_NodeStyle).TrackViewState();
                }
                return _NodeStyle;
            }
        }

        /// <summary>
        /// Url of the image to display when not selected, expanded, or hovered
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Editor(typeof(System.Web.UI.Design.UrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when not selected, expanded, or hovered"),
        ]
        public String ImageUrl
        {
            get
            {
                object str = ViewState["ImageUrl"];
                return ((str == null) ? ((this._treeview != null) ? this._treeview.ImageUrl : String.Empty) : (String)str);
            }
            set
            {
                ViewState["ImageUrl"] = value;
            }
        }

        /// <summary>
        /// Url of the image to display when selected
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Editor(typeof(System.Web.UI.Design.UrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when selected"),
        ]
        public String SelectedImageUrl
        {
            get
            {
                object str = ViewState["SelectedImageUrl"];
                return ((str == null) ? ((this._treeview != null) ? this._treeview.SelectedImageUrl : String.Empty) : (String)str);
            }
            set
            {
                ViewState["SelectedImageUrl"] = value;
            }
        }

        /// <summary>
        /// Url of the image to display when expanded
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Editor(typeof(System.Web.UI.Design.UrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when expanded"),
        ]
        public String ExpandedImageUrl
        {
            get
            {
                object str = ViewState["ExpandedImageUrl"];
                return ((str == null) ? ((this._treeview != null)?this._treeview.ExpandedImageUrl:String.Empty)  : (String)str);
            }
            set
            {
                ViewState["ExpandedImageUrl"] = value;
            }
        }

        /// <summary>
        /// id of the window to target with a navigation upon selecting this node
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("d of the window to target with a navigation upon selecting this node"),
        ]
        public String Target
        {
            get
            {
                object str = ViewState["Target"];
                return ((str == null) ? ((this._treeview != null) ? this._treeview.Target : String.Empty) : (String)str);
            }
            set
            {
                ViewState["Target"] = value;
            }
        }

        /// <summary>
        /// Controls how often the node determines whather or not it can be expanded
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(ExpandableValue.Auto),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Controls how often the node determines whather or not it can be expanded"),
        ]
        public ExpandableValue Expandable
        {
            get
            {
                object b = ViewState["Expandable"];
                return ((b == null) ? ExpandableValue.Auto : (ExpandableValue)b);
            }
            set
            {
                ViewState["Expandable"] = value;
            }
        }

        /// <summary>
        /// Text to display
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Text to display"),
        ]
        public String Text
        {
            get
            {
                object str = ViewState["Text"];
                return ((str == null) ? String.Empty : (String)str);
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Url to navigate to when node is selected
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Url to navigate to when node is selected"),
        ]
        public String NavigateUrl
        {
            get
            {
                object str = ViewState["NavigateUrl"];
                return ((str == null) ? String.Empty : (String)str);
            }
            set
            {
                ViewState["NavigateUrl"] = value;
            }
        }

        /// <summary>
        /// Whether or not the node is expanded
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Whether or not the node is expanded"),
        ]
        public bool Expanded
        {
            get
            {
                string b =_Expanded.Value;
                if (b == "")
                {
                    if (ParentTreeView.ExpandLevel > Level)
                        return true;
                    else
                        return false;
                }
                else
                    return Boolean.Parse(b);
            }
            set
            {
                _Expanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Whether or not the node is currently visible (all parents expanded)
        /// </summary>
        internal bool IsVisible
        {
            get
            {
                TreeNode prev = this;
                while (prev.Parent is TreeNode)
                {
                    prev = (TreeNode)prev.Parent;
                    if (!prev.Expanded)
                        return false;
                }
                return true;
            }
        }

        [
       Category("Data"),
       DefaultValue(""),
       PersistenceMode(PersistenceMode.Attribute),
       Description("Value of the Node"),
       ]
        public string Value
        {
            get
            {
                object str = ViewState["NodeData"];
                if (str == null)
                    return String.Empty;
                else
                    return (string)str;
            }
            set
            {
                ViewState["NodeData"] = value;
            }
        }


        /// <summary>
        /// Whether or not the node is selected
        /// </summary>
        /// 
        internal bool _Selected
        {
            get
            {
                object b = ViewState["Selected"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["Selected"] = value;
            }
        }

        /// <summary>
        /// Whether or not the node is selected
        /// </summary>
        [
         Category("Behavior"),
         DefaultValue(""),
         Browsable(false),
         Description("Whether or not the node is selected"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
         ]
         public bool Selected
        {
            get
            {
                object b = ViewState["Selected"];
                return ((b == null) ? false : (bool)b);
            }
        }


        /// <summary>
        /// Whether or not to display a checkbox
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Whether or not to display a checkbox"),
        ]
        public bool ShowCheckBox
        {
            get
            {
                object b = ViewState["ShowCheckBox"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["ShowCheckBox"] = value;
            }
        }

        /// <summary>
        /// Returns a reference to the parent TreeView.
        /// </summary>
        protected internal TreeView ParentTreeView
        {
            get
            {
                if (_treeview == null)
                {
                    // Get the parent treeview
                    TreeNode prev = this;
                    while (prev.Parent is TreeNode)
                        prev = (TreeNode)prev.Parent;
                    _treeview = (TreeView)prev.Parent;
                }
                return _treeview;
            }
            set
            {
                _treeview = value;
                foreach (TreeNode node in ChildNodes)
                {
                    node.ParentTreeView = value;
                }
            }
        }

        /// <summary>
        /// Returns the level in the tree.
        /// </summary>
        protected int Level
        {
            get
            {
                if (_level == -1)
                {
                    Object prev = Parent;
                    while (prev is TreeNode)
                    {
                        prev = ((TreeNode)prev).Parent;
                        _level++;
                    }
                    return ++_level;
                }
                else
                    return _level;
            }
        }


        /// <summary>
        /// Returns the number of sibling TreeNodes before this one.
        /// </summary>
        internal int SibIndex
        {
            get
            {
                if (Parent != null)
                {
                    if (Parent is TreeNode)
                        return ((TreeNode)Parent).ChildNodes.IndexOf(this);
                    else
                        return ((TreeView)Parent).Nodes.IndexOf(this);
                }
                else
                    return -1;
            }
        }

        /// <summary>
        /// Tooltip to show when the mouse is over the image
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Tooltip to show when the mouse is over the image"),
        ]
        public string ImageToolTip
        {
            get
            {
                return (ViewState["ImageToolTip"] == null) ? "" : Convert.ToString(ViewState["ImageToolTip"]);
            }
            set
            {
                ViewState["ImageToolTip"] = value;
            }
        }

        /// <summary>
        /// Gets the collection of nodes in the control.
        /// </summary>
        [
        Category("Data"),
        DefaultValue(null),
        MergableProperty(false),
        Browsable(true),
        PersistenceMode(PersistenceMode.InnerDefaultProperty),
        Description("Gets the collection of nodes in the control.")
        ]
        public virtual TreeNodeCollection ChildNodes
        {
            get { return _Nodes; }
        }
        #endregion
        
        #region TreeNode Methods
        /// <summary>
        /// Remove the TreeNode and any children from its parent's Nodes collection.
        /// </summary>
        public void Remove()
        {
            TreeNodeCollection colParent = GetSiblingNodeCollection();
            if (colParent != null)
                colParent.Remove(this);
        }

        /// <summary>
        /// Retrieves the TreeNodeCollection this TreeNode is in.
        /// </summary>
        /// <returns>The TreeNodeCollection</returns>
        public TreeNodeCollection GetSiblingNodeCollection()
        {
            if (Parent is TreeView || Parent is TreeNode)
            {
                return GetNodeCollection(Parent);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the TreeNodeCollection from the given object.
        /// The object must be a TreeView or TreeNode.
        /// </summary>
        /// <param name="obj">The object on which to retrieve its Nodes collection.</param>
        /// <returns>The TreeNodeCollection.</returns>
        private TreeNodeCollection GetNodeCollection(Object obj)
        {
            if (obj is TreeView)
                return ((TreeView)obj).Nodes;
            else if (obj is TreeNode)
                return ((TreeNode)obj).ChildNodes;
            else
                throw new Exception(Resources.ErrorMessages.errInvalidTreeViewObject);
        }

        public void ExpandAll()
        {
            OnExpand(new EventArgs()); //expand this node
            ChildNodes.ExpandAll();
        }

        public void CollapseAll()
        {
            OnCollapse(new EventArgs()); //expand this node
            ChildNodes.ExpandAll();
        }
        
        /// <summary>
        /// Return the attribute from the first place it's defined among:
        /// * the node itself
        /// * the node's type (directly declared or inherited via ChildType)
        /// * the treeview
        /// </summary>
        /// <param name="strAtt">The name of the attribute to lookup.</param>
        /// <returns>The value of the attribute.</returns>
        public object FindNodeAttribute(String strAtt)
        {
            //object obj = GetStateVar(strAtt);
            //if (obj != null && !(obj is CssCollection && ((CssCollection)obj).CssText == String.Empty))
            //    return obj;

            //obj = GetNodeTypeAttribute(strAtt);
            //if (obj != null && !(obj is CssCollection && ((CssCollection)obj).CssText == String.Empty))
            //    return obj;

            return ParentTreeView.GetStateVar(strAtt);
        }
        /// <summary>
        /// Returns a x.y.z format node index string representing the node's position in the hierarchy.
        /// </summary>
        /// <returns>The x.y.z formatted index.</returns>
        public string GetNodeIndex()
        {
            string strIndex = "";
            Object node = this;
            while (node is TreeNode)
            {
                if (((TreeNode)node).SibIndex == -1)
                    return String.Empty;
                if (strIndex.Length == 0)
                    strIndex = ((TreeNode)node).SibIndex.ToString();
                else
                    strIndex = ((TreeNode)node).SibIndex.ToString() + "." + strIndex;
                node = ((TreeNode)node).Parent;
            }
            return strIndex;
        }
        /// <summary>
        /// Returns the node's previous TreeNode sibling, or null
        /// </summary>
        /// <returns>The node's previous TreeNode sibling, or null.</returns>
        protected TreeNode GetPreviousSibling()
        {
            TreeNodeCollection sibs;
            if (Parent is TreeNode)
                sibs = ((TreeNode)Parent).ChildNodes;
            else
                sibs = ((TreeView)Parent).Nodes;

            int iIndex = sibs.IndexOf(this) - 1;
            if (iIndex >= 0)
                return sibs[iIndex];
            else
                return null;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event handler for the OnSelectedIndexChange event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns>true to bubble, false to cancel</returns>
        protected virtual bool OnSelectedIndexChange(EventArgs e)
        {
            return true;
        }

        /// <summary>
        /// Event handler for the OnExpand event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns>true to bubble, false to cancel</returns>
        protected virtual bool OnExpand(EventArgs e)
        {
            if (!Expanded && CanExpand())
            {
                Expanded = true;
                
                SetExpandCollapse();

                this._treeview.OnExpand(new TreeViewClickEventArgs(GetNodeIndex()));
               
                //Object obj = FindNodeAttribute("Expandable");
                //if (obj == null)
                //{
                //    obj = Expandable; // get default value if none is explicitly set
                //}

                ////if ((ExpandableValue)obj == ExpandableValue.CheckOnce)
                ////    CheckedExpandable = true;

                ////if (!_bBound)
                ////    ReadXmlSrc();

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Event handler for the OnCollapse event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns>true to bubble, false to cancel</returns>
        protected virtual bool OnCollapse(EventArgs e)
        {
            if (Expanded)
            {
                Expanded = false;
                
                SetExpandCollapse();
                
                // If this node was selected, unselect it and make the parent the selected node
                // Note: The client handles this nicely uplevel; we only need to do this downlevel
               
                String strNewIndex = GetNodeIndex();
                String strOldIndex = ParentTreeView.SelectedNodeIndex;
                if (strOldIndex.StartsWith(strNewIndex) && strOldIndex != strNewIndex)
                {
                    TreeViewSelectEventArgs e2 = new TreeViewSelectEventArgs(strOldIndex, strNewIndex);
                    ParentTreeView.DoSelectedIndexChange(e2);
                    // Since this only gets called downlevel, we don't need to worry about other selection
                    // changes being queued-- this will be the only one, and so we can queue an event for it 
                    ParentTreeView._eventList.Add("s");
                    ParentTreeView._eventList.Add(e2);
                }
                
                this._treeview.OnCollapse(new TreeViewClickEventArgs(GetNodeIndex()));
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Process the event string, calling the appropriate event handler.
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        /// <returns>true to bubble, false otherwise.</returns>
        private bool ProcessEvent(string eventArg)
        {
            int nSep = eventArg.IndexOf(',');
            if (nSep < 0)
                nSep = eventArg.Length;

            string strEvent = eventArg.Substring(0, nSep);
            EventArgs e = new EventArgs();
            if (strEvent.Equals("onexpand"))
                return OnExpand(e);
            else if (strEvent.Equals("oncollapse"))
                return OnCollapse(e);
            else if (strEvent.Equals("onselectedindexchange"))
                return OnSelectedIndexChange(e);
            else if (strEvent.Equals("oncheck"))
                return OnCheck(e);
            else
                return false;
        }


        //
        // OnCheck
        //
        /// <summary>
        /// Event handler for OnCheck event. For future implementation
        /// </summary>
        protected virtual bool OnCheck(EventArgs e)
        {
            switch (CheckState)
            {
                case Checkbox.CheckboxState.Checked:
                    CheckState = Checkbox.CheckboxState.Unchecked;
                    break;
                case Checkbox.CheckboxState.Unchecked:
                    CheckState = Checkbox.CheckboxState.Undefine;
                    break;
                case Checkbox.CheckboxState.Undefine:
                    CheckState = Checkbox.CheckboxState.Checked;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Public entry point used by treeview to "bubble down" an event, thus freeing treenodes from having
        /// to include postback HTML for default treenode activity
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        internal virtual void LowerPostBackEvent(string eventArg)
        {
            ProcessEvent(eventArg);
        }

        //
        // OnInit
        //
        /// <summary>
        /// Initializes the TreeNode.
        /// </summary>
        internal void OnInit()
        {
            if (Expanded)
            {
                //Databind();
            }
           
        }

        #endregion
        
        #region Privates
       
        /// <summary>
        /// Determine if the current node is an L, T, or Root junction.
        /// Note: This code is essentially duplicated from the client behavior.
        /// </summary>
        /// <returns>A character defining the type of junction.</returns>
        protected char CalcJunction()
        {
            if (Parent is TreeView && GetPreviousSibling() == null)
            {
                // Get index of node and add 1 to the last value
                string strIndex = GetNodeIndex();
                int iIndexPos = strIndex.LastIndexOf('.');
                int iIndexVal = Convert.ToInt32(strIndex.Substring(iIndexPos + 1)) + 1;
                strIndex = strIndex.Substring(0, iIndexPos + 1) + iIndexVal.ToString();
                // if the node exists, we're an "F" node.
                if (ParentTreeView.GetNodeFromIndex(strIndex) != null)
                    return 'F';
                else
                    return 'R';
            }
            else
            {
                TreeNodeCollection col;
                if (Parent is TreeView)
                    col = ((TreeView)Parent).Nodes;
                else
                    col = ((TreeNode)Parent).ChildNodes;
                int i = col.IndexOf(this) + 1;
                if (i < col.Count)
                    return 'T';
                return 'L';
            }
        }

        /// <summary>
        /// Adds attributes to the HtmlTextWriter.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter object that receives the content.</param>
        protected virtual void AddAttributesToRender(HtmlTextWriter output)
        {
            if (ImageUrl != String.Empty)
                output.AddAttribute("ImageUrl", ImageUrl);

            if (SelectedImageUrl != String.Empty)
                output.AddAttribute("SelectedImageUrl", SelectedImageUrl);

            if (ExpandedImageUrl != String.Empty)
                output.AddAttribute("ExpandedImageUrl", ExpandedImageUrl);
            
            if (Target != String.Empty)
                output.AddAttribute("Target", Target);

            output.AddAttribute("CheckState", CheckState.ToString());

            if (ID != String.Empty)
            {
                output.AddAttribute("ID", ID);


            }
        }
        
        /// <summary>
        /// Writes attributes to the HtmlTextWriter.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter object that receives the content.</param>
        protected virtual void WriteAttributes(HtmlTextWriter writer)
        {
            if (ID != String.Empty)
            {
                writer.WriteAttribute("ID", ID);
            }
        }

        protected internal void CreateChildControls(ControlCollection Controls)
        {
            //Controls.Clear();
            //base.CreateChildControls();
            
            string NodeIndex = GetNodeIndex();
            _NodeElement = new System.Web.UI.WebControls.Panel();
            _NodeElement.ID = "Node_" + NodeIndex;

            System.Web.UI.WebControls.Table tNode = new System.Web.UI.WebControls.Table();
            System.Web.UI.WebControls.TableRow rNode = new System.Web.UI.WebControls.TableRow();
            _ChildNodes = new System.Web.UI.WebControls.Panel();

            //Format Node Table
            tNode.ID = "TableNode_" + NodeIndex;
            tNode.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
            tNode.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
            tNode.CellPadding = 0;
            tNode.CellSpacing = 0;

            rNode.ID = "RowNode_" + NodeIndex;
            tNode.Rows.Add(rNode);

            _ChildNodes.ID = "ChildNodesDiv_" + NodeIndex;

            char cJunction = CalcJunction();
            Object obWalk = Parent;

            String imageSrc = "";
            int i;

            System.Web.UI.WebControls.TableCell NodeCell;
            System.Web.UI.WebControls.Image Img;
            System.Collections.ArrayList cellList = new System.Collections.ArrayList(2);
            if (ParentTreeView.ShowLines == true)
            {
                int iCount = 0;
                string strLines = String.Empty;
                while (obWalk is TreeNode)
                {
                    NodeCell = new System.Web.UI.WebControls.TableCell();
                    Img = new System.Web.UI.WebControls.Image();

                    TreeNode elWalk = (TreeNode)obWalk;
                    TreeNodeCollection kids = GetNodeCollection(elWalk.Parent);
                    i = kids.IndexOf(elWalk) + 1;
                    if (i < kids.Count)
                    {
                        if (iCount > 0)
                        {
                            Img.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
                            Img.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
                            Img.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
                            Img.Width = new System.Web.UI.WebControls.Unit(iCount * ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
                            Img.Height = new System.Web.UI.WebControls.Unit(1, System.Web.UI.WebControls.UnitType.Pixel);
                            Img.ImageUrl = ParentTreeView.SystemImagesPath + "white.gif";
                            NodeCell.Controls.Add(Img);
                            NodeCell.Width = new System.Web.UI.WebControls.Unit(ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
                            iCount = 0;
                            cellList.Add(NodeCell);
                            NodeCell = new System.Web.UI.WebControls.TableCell();
                            Img = new System.Web.UI.WebControls.Image();
                        }
                        
                        Img.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
                        Img.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
                        Img.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
                        Img.Width = new Unit();
                        Img.Height = new Unit();
                        Img.ImageUrl = ParentTreeView.SystemImagesPath + "I.gif";
                        NodeCell.Controls.Add(Img);
                        NodeCell.Width = new System.Web.UI.WebControls.Unit(iCount * ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
                    }
                    else
                        iCount++;
                    cellList.Add(NodeCell);
                    obWalk = elWalk.Parent;
                }
                if (iCount > 0)
                {
                    Img = new System.Web.UI.WebControls.Image();
                    NodeCell = new System.Web.UI.WebControls.TableCell();

                    Img.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
                    Img.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
                    Img.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.Width = new System.Web.UI.WebControls.Unit(iCount * ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.Height = new System.Web.UI.WebControls.Unit(1, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.ImageUrl = ParentTreeView.SystemImagesPath + "white.gif";
                    NodeCell.Controls.Add(Img);
                    NodeCell.Width = new System.Web.UI.WebControls.Unit(ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
                    cellList.Add(NodeCell);
                }
                
                //Add all the nodes in the correct order
                for (i = cellList.Count - 1; i >= 0; i--)
                {
                    rNode.Cells.Add((System.Web.UI.WebControls.TableCell)cellList[i]);
                }
            }
            else
            {
                if (cJunction != 'R' && cJunction != 'F')
                {
                    NodeCell = new System.Web.UI.WebControls.TableCell();
                    Img = new System.Web.UI.WebControls.Image();

                    int iIndent = 0;
                    while (obWalk is TreeNode && ((TreeNode)obWalk).Parent != null)
                    {
                        iIndent += ParentTreeView.Indent;
                        obWalk = ((TreeNode)obWalk).Parent;
                    }
                    Img.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
                    Img.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
                    Img.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.Width = new System.Web.UI.WebControls.Unit(iIndent, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.Height = new System.Web.UI.WebControls.Unit(1, System.Web.UI.WebControls.UnitType.Pixel);
                    Img.ImageUrl = ParentTreeView.SystemImagesPath + "white.gif";
                    NodeCell.Controls.Add(Img);
                    NodeCell.Width = new System.Web.UI.WebControls.Unit(iIndent, System.Web.UI.WebControls.UnitType.Pixel);
                    rNode.Cells.Add(NodeCell);
                }
            }

            // is this a branch node?

            imageSrc = "";

            NodeCell = new System.Web.UI.WebControls.TableCell();
            _CollapseExpandImg = new System.Web.UI.WebControls.Image();
            _CollapseExpandImg.ID = "CollapseExpandImg_" + NodeIndex;
            _CollapseExpandImg.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
            _CollapseExpandImg.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
            _CollapseExpandImg.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
            _CollapseExpandImg.Style.Value = "cursor: hand;";
            _Expanded.ID = "ExpandedState_" + NodeIndex;
            
            _NodeElement.Controls.Add(_Expanded);

            SetExpandCollapse();

            NodeCell.Controls.Add(_CollapseExpandImg);
            NodeCell.Width = new System.Web.UI.WebControls.Unit(ParentTreeView.Indent, System.Web.UI.WebControls.UnitType.Pixel);
            rNode.Cells.Add(NodeCell); //Add de + or - or white or line 

            //output.Write("<IMG align='top' border='0' class='icon' SRC=" + imageSrc);

            //if (bParent && ParentTreeView.ShowPlus == true)
            //    output.RenderEndTag();

            // Render a checkbox
            _State = new System.Web.UI.WebControls.HiddenField();
            _State.ID = "CheckBoxState_" + NodeIndex;
            _State.Value = Checkbox.CheckboxState.Undefine.ToString();
            _NodeElement.Controls.Add(_State); //I add all the time the checkbox state to allow use in javascript if needed

            
            _imgCheck = new System.Web.UI.WebControls.Image();
            _imgCheck.Height = new System.Web.UI.WebControls.Unit(16, System.Web.UI.WebControls.UnitType.Pixel);
            _imgCheck.Width = new System.Web.UI.WebControls.Unit(16, System.Web.UI.WebControls.UnitType.Pixel);

            _imgCheck.ID = "ImageCheckBox_" + NodeIndex;
            SetCheckImg(_imgCheck, this.CheckState);
            NodeCell = new System.Web.UI.WebControls.TableCell();
            NodeCell.Controls.Add(_State);
            NodeCell.Controls.Add(_imgCheck);

            if (!this.ShowCheckBox)
            {
                NodeCell.Style.Add("display", "none"); //this trick allow activate the checkbox on client script :D
            }
            rNode.Cells.Add(NodeCell);

            imageSrc = "";

            //If has icon add icon cell
            if (Selected && SelectedImageUrl != "")
            {
                imageSrc = SelectedImageUrl;
            }
            else
            {
                if (ExpandedImageUrl != "" && Expanded)
                {
                    imageSrc = ExpandedImageUrl;
                }
                else
                {
                    if (ImageUrl != "")
                    {
                        imageSrc = ImageUrl;
                    }
                }
            }
            if (imageSrc != "")
            {
                NodeCell = new System.Web.UI.WebControls.TableCell();
                Img = new System.Web.UI.WebControls.Image();

                Img.ImageAlign = System.Web.UI.WebControls.ImageAlign.Top;
                Img.BorderStyle = System.Web.UI.WebControls.BorderStyle.None;
                Img.BorderWidth = new System.Web.UI.WebControls.Unit(0, System.Web.UI.WebControls.UnitType.Pixel);
                Img.ImageUrl = imageSrc;
                Img.ToolTip = this.ImageToolTip;
                Img.CssClass = "icon";
                NodeCell.Controls.Add(Img);
                NodeCell.Width = new System.Web.UI.WebControls.Unit(1, System.Web.UI.WebControls.UnitType.Pixel);
                NodeCell.Style.Add(HtmlTextWriterStyle.PaddingLeft, "2px");
                NodeCell.Style.Add(HtmlTextWriterStyle.PaddingRight, "2px");
                rNode.Cells.Add(NodeCell);
            }

            NodeCell = new System.Web.UI.WebControls.TableCell();
            NodeCell.ID = "Text_" + NodeIndex;
            NodeCell.Style.Value = "WHITE-SPACE: nowrap";
            System.Web.UI.WebControls.HyperLink Link = new System.Web.UI.WebControls.HyperLink();
            Link.Style.Value = "display:inline; cursor: hand; text-decoration:none; overflow:hidden; ";
            Link.ID = "Link_" + NodeIndex;
            if (Text != "")
            {
                Link.Text = "&nbsp;" + Text;
            }
            //If navigate url is setted then set the respective link
            string href = "";
            if (this.NavigateUrl != "")
            {
                Link.NavigateUrl = this.NavigateUrl;
            }
            else
            {
                if (ParentTreeView.Page != null)
                    href = "javascript:" + ParentTreeView.Page.ClientScript.GetPostBackEventReference(ParentTreeView, "onselectedindexchange," + _treeview.SelectedNodeIndex + "," + NodeIndex);
                else
                    href = String.Empty;
                Link.NavigateUrl = href;
            }

            NodeCell.Controls.Add(Link);
            rNode.Cells.Add(NodeCell);

            _NodeElement.Controls.Add(tNode);

            foreach (TreeNode tn in ChildNodes)
            {
                //_ChildNodes.Controls.Add(tn);
                tn.CreateChildControls(_ChildNodes.Controls);
            }


            _NodeElement.Controls.Add(_ChildNodes);
            
            if (Controls != null)
            {
                Controls.Add(_NodeElement);
            }
        }

        private string GetExpandCollapseImgUrl (bool nExpand)
        {
            char cJunction = CalcJunction();
            bool bParent = false;
            string NodeIndex = GetNodeIndex();
            string imageSrc = "";
            if (ChildNodes.Count > 0 || (ExpandableValue)Expandable == ExpandableValue.Always)
            {
                bParent = true;
            }
            if (ParentTreeView.ShowLines == true)
            {

                switch (cJunction)
                {
                    case 'L':
                        imageSrc = "L";
                        break;
                    case 'T':
                        imageSrc = "T";
                        break;
                    case 'R':
                        imageSrc = "R";
                        break;
                    case 'F':
                        imageSrc = "F";
                        break;
                }
            }
            imageSrc = ParentTreeView.SystemImagesPath + imageSrc;
            if (bParent)
            {
                if (nExpand == true)
                {
                    imageSrc += "minus.gif";
                }
                else
                {
                    imageSrc += "plus.gif";
                }
                //output.AddAttribute(HtmlTextWriterAttribute.Href, href);
                //output.RenderBeginTag(HtmlTextWriterTag.A);

            }
            else if (ParentTreeView.ShowLines == true)
                imageSrc += ".gif";
            else
                imageSrc = ParentTreeView.SystemImagesPath + "white.gif";
            
            return imageSrc;
        }

        private void SetExpandCollapse()
        {
            bool bParent = false;
            string NodeIndex = GetNodeIndex();
            if (_CollapseExpandImg != null)
            {
                if (ChildNodes.Count > 0 || (ExpandableValue)Expandable == ExpandableValue.Always)
                {
                    bParent = true;
                }

                string imageSrc = "";
                string href = "";
                if (bParent)
                {
                    if (ParentTreeView.Page != null && ( _treeview._HasExandHander() || _treeview._HasCollapseHander()))
                        href = "javascript:" + ParentTreeView.Page.ClientScript.GetPostBackEventReference(ParentTreeView, (Expanded ? "oncollapse," : "onexpand,") + NodeIndex);
                    else
                        href = String.Empty;
               
                }
                imageSrc = GetExpandCollapseImgUrl(this.Expanded);

                _CollapseExpandImg.ImageUrl = imageSrc;
                _CollapseExpandImg.Attributes["onclick"] = href;
            }
            if (_ChildNodes != null)
            {   //if expanded then display the child nodes, else hide it.
                _ChildNodes.Style["display"] = Expanded?"":"none";
            }
        }
        

        /// <summary>
        /// Outputs content to a provided HtmlTextWriter output stream.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter object that receives the control content.</param>
        //protected internal virtual void Render(HtmlTextWriter output)
        //{
        
        //    // if this node is a navigating leaf, use its nav information.

        //    obj = FindNodeAttribute("NavigateUrl");
        //    if (obj != null && (!(ParentTreeView.SelectExpands && bParent && !Expanded)))
        //    {
        //        href = (string)obj;
        //        obj = FindNodeAttribute("Target");
        //        if (obj != null)
        //        {
        //            output.AddAttribute(HtmlTextWriterAttribute.Target, (string)obj);
        //            obj = null;
        //        }
        //    }
        //    else if (ParentTreeView.Page != null)
        //    {
        //        if (!Selected)
        //        {
        //            href = "javascript:" + ParentTreeView.Page.ClientScript.GetPostBackEventReference(ParentTreeView, "onselectedindexchange," + ParentTreeView.SelectedNodeIndex + "," + GetNodeIndex());
        //            if (bParent && ParentTreeView.SelectExpands == true)
        //                href = href.Substring(0, href.Length - 2) + (";" + (Expanded ? "oncollapse," : "onexpand,") + GetNodeIndex()) + "')";
        //        }
        //        else
        //            if (bParent && ParentTreeView.SelectExpands == true)
        //                href = "javascript:" + ParentTreeView.Page.ClientScript.GetPostBackEventReference(ParentTreeView, (Expanded ? "oncollapse," : "onexpand,") + GetNodeIndex());
        //            else
        //                href = String.Empty;
        //    }
        //    else
        //        href = String.Empty;

        //    if (href != String.Empty)
        //    {
        //        output.AddAttribute(HtmlTextWriterAttribute.Href, href);
        //        output.RenderBeginTag(HtmlTextWriterTag.A);
        //    }

        //    obj = null;
        //    if (Selected == true)
        //        obj = FindNodeAttribute("SelectedImageUrl");
        //    if (obj == null)
        //    {
        //        if (bParent == true && Expanded == true)
        //            obj = FindNodeAttribute("ExpandedImageUrl");
        //        if (obj == null)
        //            obj = FindNodeAttribute("ImageUrl");
        //    }
        //    if (obj != null)
        //        output.Write("<IMG align='top' border='0' class='icon' SRC='" + (string)obj + "'>");
            
        //    CssStyleCollection cStyle = new CssStyleCollection();
        //    cStyle.Value  = "display:inline; font-size:10pt; font-face:Times; color: black; text-decoration:none; cursor: hand; overflow:hidden;";
        //    System.Web.UI.WebControls.Style st;
            
        //    //object o = FindNodeAttribute("DefaultStyle");
        //    //if (o != null)
        //    //{
                
        //    //    cssStyle.Merge((CssCollection)o, true);
        //    //}
        //    if (Selected)
        //    {
        //        //CssCollection temp = new CssCollection("color: #00FFFF; background-color: #08246B;");
        //        //o = FindNodeAttribute("SelectedStyle");
        //        //if (o != null)
        //        //{
        //        //    temp.Merge((CssCollection)o, true);
        //        //}
        //        //cssStyle.Merge(temp, true);
        //    }

        //    /*
        //               // Links will be blue by default.  We want them to be the color specified in our style.
        //               // So we have to override the link color by adding a font tag inside the A tag.
        //               string curColor = cssStyle.GetColor();
        //               if (!Selected && (curColor != null) && (curColor != String.Empty))
        //               {
        //                   output.AddStyleAttribute(HtmlTextWriterStyle.Color, curColor);
        //               }
        //   */
        //    //cssStyle.AddAttributesToRender(output);
        //    //cssStyle.RenderBeginFontTag(output);
        //    //cssStyle.RenderBeginModalTags(output);

        //    if (Text != null && Text != String.Empty)
        //        output.Write("&nbsp;" + Text);

        //    //cssStyle.RenderEndModalTags(output);
        //    //cssStyle.RenderEndFontTag(output);

        //    if (href != String.Empty)
        //        output.RenderEndTag();

        //    output.Write("</TD></TR>\n");

        //    // Render child treenodes
        //    if (Expanded)
        //    {
        //        foreach (TreeNode item in Nodes)
        //        {
        //            item.Render(output);
        //        }
        //    }
        //}

        /// <summary>
        /// Utilitary function to retrive usign reflection a property of this class.
        /// </summary>
        /// <param name="att"></param>
        /// <returns></returns>
        internal object GetStateVar(String att)
        {
            if (att.EndsWith("Style"))
                return this.GetType().InvokeMember(att, BindingFlags.Default | BindingFlags.GetProperty, null, this, new object[] { });
            else
                return ViewState[att];
        }

        /// <summary>
        /// True if the node has child nodes and is marked as Expandable.
        /// </summary>
        /// <returns>true or false depending the Expandable attribute and if has childs.</returns>
        private bool CanExpand()
        {
            Object exp = FindNodeAttribute("Expandable");
            if (exp == null)
                exp = Expandable;
            if ((ExpandableValue)exp == ExpandableValue.Always)
                return true;
            if (ChildNodes.Count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Set checkbox image based on the state
        /// </summary>
        /// <param name="iChk">Checkbox image to set</param>
        /// <param name="nState">State of the check box</param>
        private void SetCheckImg(System.Web.UI.WebControls.Image iChk, Checkbox.CheckboxState nState)
        {
            switch (nState)
            {
                case Checkbox.CheckboxState.Undefine:
                    if (CheckedImageUrl == "")
                    {
                        iChk.ImageUrl = "Images/undefine.gif";
                    }
                    else
                    {
                        iChk.ImageUrl = UndefineImageUrl;
                    }
                    break;
                case Checkbox.CheckboxState.Checked:
                    if (CheckedImageUrl == "")
                    {
                        iChk.ImageUrl = "Images/checked.gif";
                    }
                    else
                    {
                        iChk.ImageUrl = CheckedImageUrl;
                    }
                    break;
                case Checkbox.CheckboxState.Unchecked:
                    if (CheckedImageUrl == "")
                    {
                        iChk.ImageUrl = "Images/unchecked.gif";
                    }
                    else
                    {
                        iChk.ImageUrl = UncheckedImageUrl;
                    }
                    break;
            }
        }

        internal virtual void OnPrerender() 
        {
            //before generate the descriptors, ajust some properies based on the state loaded.
            string NodeIndex = GetNodeIndex();
            SetCheckImg(_imgCheck, CheckState);
            SetExpandCollapse();
            if (_treeview != null)
            {
                System.Web.UI.WebControls.Table tNode = (System.Web.UI.WebControls.Table)_NodeElement.Controls[1];
                System.Web.UI.WebControls.HyperLink Link = (System.Web.UI.WebControls.HyperLink)this._NodeElement.FindControl("Link_" + GetNodeIndex());
                if (Selected)
                {
                    tNode.Style.Value = _treeview.SelectedNodeStyle.GetStyleAttributes(tNode).Value;
                    tNode.CssClass = _treeview.SelectedNodeStyle.CssClass;
                    Link.Style.Value += _treeview.SelectedNodeStyle.GetStyleAttributes(tNode).Value;
                }
                else
                {
                    string nStyle = this.NodeStyle.GetStyleAttributes(tNode).Value;

                    if (nStyle != null && nStyle != "")
                    {
                        tNode.Style.Value = nStyle;
                        Link.Style.Value += nStyle;
                    }
                    else
                    {
                        tNode.Style.Value = _treeview.NodeStyle.GetStyleAttributes(tNode).Value;
                        Link.Style.Value += _treeview.NodeStyle.GetStyleAttributes(tNode).Value;
                        
                    }
                    if (tNode.Style.Value == null)
                    {
                        tNode.Style.Value = "";
                    }
                    if (this.NodeStyle.CssClass != "")
                    {
                        tNode.CssClass = this.NodeStyle.CssClass;
                    }
                    else
                    {
                        tNode.CssClass = _treeview.NodeStyle.CssClass;
                    }
                }
            }
            foreach (TreeNode tn in ChildNodes)
            {
                tn.OnPrerender();
            }
        }

        internal IEnumerable<ScriptDescriptor>
                GetScriptDescriptors()
        {
            
            string nodeindex = this.GetNodeIndex();
            System.Web.UI.WebControls.Table tNode = (System.Web.UI.WebControls.Table)_NodeElement.Controls[1];
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Axelerate.BusinessLayerUITools.WebControls.TreeNode", this._NodeElement.ClientID);
            //descriptor.AddProperty("state", State);
            descriptor.AddElementProperty("state", _State.ClientID);
            descriptor.AddElementProperty("expanded", _Expanded.ClientID);

            descriptor.AddElementProperty("imgCEButtonElement", this._CollapseExpandImg.ClientID);
            descriptor.AddElementProperty("imgChkElement", this._imgCheck.ClientID);
            descriptor.AddElementProperty("hplElement", this._NodeElement.FindControl("Text_" + nodeindex).ClientID );
            descriptor.AddElementProperty("tbElement", this._NodeElement.FindControl("TableNode_" + nodeindex).ClientID);
            descriptor.AddElementProperty("ChildNodesDiv", this._NodeElement.FindControl("ChildNodesDiv_" + nodeindex).ClientID);
            descriptor.AddElementProperty("nodeLink", this._NodeElement.FindControl("Link_" + nodeindex).ClientID);


            if (CheckedImageUrl == "")
            {
                descriptor.AddProperty("CheckedImageUrl", "Images/checked.gif");
            }
            else
            {
                descriptor.AddProperty("CheckedImageUrl", CheckedImageUrl);
            }
            if (UncheckedImageUrl == "")
            {
                descriptor.AddProperty("UncheckedImageUrl", "Images/unchecked.gif");
            }
            else
            {
                descriptor.AddProperty("UncheckedImageUrl", UncheckedImageUrl);
            }
            if (UndefineImageUrl == "")
            {
                descriptor.AddProperty("UndefineImageUrl", "Images/undefine.gif");
            }
            else
            {
                descriptor.AddProperty("UndefineImageUrl", UndefineImageUrl);
            }

            descriptor.AddProperty("ImageUrl", this.ImageUrl);//this._ImageUrl = null;
            descriptor.AddProperty("SelectedImageUrl", this.SelectedImageUrl); // this._SelectedImageUrl = null;
            descriptor.AddProperty("ExpandedImageUrl", this.ExpandedImageUrl); //this._ExpandedImageUrl = null;
            descriptor.AddProperty("Target", this.Target); //this._Target = null;

            descriptor.AddProperty("CollapseButtonImageUrl", this.GetExpandCollapseImgUrl(false)); //this._Target = null;
            descriptor.AddProperty("ExpandedButtonImageUrl", this.GetExpandCollapseImgUrl(true)); //this._Target = null;
            descriptor.AddProperty("Expandable", (ChildNodes.Count > 0 || (ExpandableValue)Expandable == ExpandableValue.Always)); //this._Target = null;
            descriptor.AddProperty("Selected", this.Selected);

            string nStyle =this._NodeStyle.GetStyleAttributes(tNode).Value;
            if (nStyle != null && nStyle != "")
            {
                descriptor.AddProperty("NodeStyle", nStyle);
            }
            else
            {
                nStyle = this.ParentTreeView.NodeStyle.GetStyleAttributes(tNode).Value;
                if (nStyle != null)
                {
                    descriptor.AddProperty("NodeStyle", this.ParentTreeView.NodeStyle.GetStyleAttributes(tNode).Value);
                }
                else
                {
                    descriptor.AddProperty("NodeStyle", "");
                }
            }
            if (this._NodeStyle.CssClass != null && this._NodeStyle.CssClass != "")
            {
                descriptor.AddProperty("NodeCss", this._NodeStyle.CssClass);
            }
            else
            {
                if (this.ParentTreeView.NodeStyle.CssClass != null)
                {
                    descriptor.AddProperty("NodeCss", this.ParentTreeView.NodeStyle.CssClass);
                }
                else
                {
                    descriptor.AddProperty("NodeCss", "");
                }
            }
            descriptor.AddProperty("nodeindex", nodeindex);
            string ChildNodesIDs = "";
            foreach (TreeNode tn in this.ChildNodes)
            {
                if(ChildNodesIDs != "") {
                    ChildNodesIDs+= ";";
                }
                ChildNodesIDs += tn._NodeElement.ClientID;
                
                foreach (ScriptDescriptor sd in tn.GetScriptDescriptors())
                {
                    yield return sd;
                }
                
            }
            descriptor.AddProperty("Nodes", ChildNodesIDs);
           
            yield return descriptor;

            //descriptor.AddProperty("ShowLines", this.ShowLines);
            


            /*this._treeview = null;
            this._parent = null;

           
            this._level = null;
              
            this._Text = null;
            this._NavigateUrl = null;
            this._NodeData = null;
            this._Selected = null;
            this._CheckBox = null;
            this._SibIndex = null;
            this._ExpandedButtonImageUrl = null;
            this._CollapseButtonImageUrl = null;

            this._TriggerExpandEvent = null;
            this._TriggerCollapseEvent = null;
            this._TriggerSelectEvent = null;

            this._Nodes = new Array();*/


           
        }

        #endregion
        
        //#region IParserAccessor Members

        //void IParserAccessor.AddParsedSubObject(object obj)
        //{
        //    if (obj is TreeNode)
        //    {
        //        _Nodes.Add((TreeNode)obj);
        //    }
        //}

        //#endregion
    }

    
}
