using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    /// <summary>
    /// Event arguments for the OnSelectedIndexChange event
    /// </summary>
    public class TreeViewSelectEventArgs : EventArgs
    {
        private string _oldnodeindex;
        private string _newnodeindex;

        /// <summary>
        /// The previously selected node.
        /// </summary>
        public string OldNode
        {
            get { return _oldnodeindex; }
        }

        /// <summary>
        /// The newly selected node.
        /// </summary>
        public string NewNode
        {
            get { return _newnodeindex; }
        }

        /// <summary>
        /// Initializes a new instance of a TreeViewSelectEventArgs object.
        /// </summary>
        /// <param name="strOldNodeIndex">The old node.</param>
        /// <param name="strNewNodeIndex">The new node.</param>
        public TreeViewSelectEventArgs(string strOldNodeIndex, string strNewNodeIndex)
        {
            _oldnodeindex = strOldNodeIndex;
            _newnodeindex = strNewNodeIndex;
        }
    }

    /// <summary>
    /// Event arguments for the OnClick event 
    /// </summary>
    public class TreeViewClickEventArgs : EventArgs
    {
        private string _nodeid;

        /// <summary>
        /// The ID of the node that was clicked.
        /// </summary>
        public string Node
        {
            get { return _nodeid; }
        }

        /// <summary>
        /// Initializes a new instance of a TreeViewClickEventArgs object.
        /// </summary>
        /// <param name="node">The ID of the node that was clicked</param>
        public TreeViewClickEventArgs(string node)
        {
            _nodeid = node;
        }
    }


    /// <summary>
    /// Delegate to handle click events on the TreeView.
    /// </summary>
    public delegate void ClickEventHandler(object sender, TreeViewClickEventArgs e);

    /// <summary>
    /// Delegate to handle select events on the TreeView.
    /// </summary>
    public delegate void SelectEventHandler(object sender, TreeViewSelectEventArgs e);



    /// <summary>
    /// Class to eliminate runat="server" requirement and restrict content to approved tags
    /// </summary>
    public class TreeViewBuilder : ControlBuilder
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
    /// <summary>
    /// TreeView class: Represents a tree.
    /// </summary>
    [
    ControlBuilderAttribute(typeof(TreeViewBuilder)),
    ToolboxData("<{0}:TreeView runat=server></{0}:TreeView>"),
    Designer(typeof(TreeViewDesigner)),
    DefaultProperty("Nodes"),
    ParseChildren(true, "Nodes")
    ]
    public class TreeView : ScriptControl, IPostBackEventHandler, IPostBackDataHandler, INamingContainer
    {

        #region private properties

        private string _szHelperID;
        private string _szHelperData = String.Empty;

        private TreeNodeCollection _Nodes;
        private bool _bFocused;
        private int _scrollTop;
        private int _scrollLeft;
        private int _parentTop;
        private int _parentLeft;

        private Style _hoverNodeStyle = new Style();
        private Style _selectedNodeStyle = new Style();
        private Style _NodeStyle = new Style();

        private bool _bCreated;
        internal System.Collections.ArrayList _eventList;

        private Panel m_Container = null;

        #endregion


        public TreeView()
            : base()
        {
            _Nodes = new TreeNodeCollection(this);
            _bFocused = false;
            _bCreated = false;
            _scrollTop = _scrollLeft = -1;
            _parentTop = _parentLeft = -1;
            _eventList = new System.Collections.ArrayList();
        }


        #region EventHandlers
        /// <summary>
        /// Event fired when a TreeNode is expanded.
        /// </summary>
        [Description("Event fired when a TreeNode is expanded.")]
        public event ClickEventHandler Expand;

        /// <summary>
        /// Event fired when a TreeNode is collapsed.
        /// </summary>
        [Description(" Event fired when a TreeNode is collapsed.")]
        public event ClickEventHandler Collapse;

        /// <summary>
        /// Event fired when a TreeNode's checkbox is clicked. (for future implementation)
        /// </summary>
        [Description("Event fired when a TreeNode's checkbox is clicked.")]
        private event ClickEventHandler Check;

        /// <summary>
        /// Event fired when the selected TreeNode changes.
        /// </summary>
        [Description("Event fired when the selected TreeNode changes.")]
        public event SelectEventHandler SelectedIndexChange;

        /// <summary>
        /// Event fired when the selected TreeNode was changed
        /// </summary>
        [Description("Event fired when the selected TreeNode was changed.")]
        public event EventHandler SelectedNodeChanged;

        #endregion

        #region OnEvent
        /// <summary>
        /// Called when a TreeNode expands.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal virtual void OnExpand(TreeViewClickEventArgs e)
        {
            if (Expand != null)
                Expand(this, e);
        }

        /// <summary>
        /// Called when a TreeNode collapses.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        internal virtual void OnCollapse(TreeViewClickEventArgs e)
        {
            if (Collapse != null)
                Collapse(this, e);
        }

        /// <summary>
        /// Called when a TreeNode's checkbox is clicked. For future implementation
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnCheck(TreeViewClickEventArgs e)
        {
            if (Check != null)
                Check(this, e);
        }

        /// <summary>
        /// Called when the selected TreeNode changes.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected internal virtual void DoSelectedIndexChange(TreeViewSelectEventArgs e)
        {
            // select/deselect nodes
            TreeNode node = GetNodeFromIndex(e.OldNode);
            if (node != null)
                node._Selected = false;

            node = GetNodeFromIndex(e.NewNode);
            if (node != null)
            {
                node._Selected = true;
                SelectedNodeIndex = e.NewNode;
            }
            OnSelectedNodeChanged(new EventArgs());
        }

        /// <summary>
        /// Event handler for selection changes.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected internal virtual void OnSelectedIndexChange(TreeViewSelectEventArgs e)
        {
            if (SelectedIndexChange != null)
                SelectedIndexChange(this, e);
        }


        /// <summary>
        /// Event handler for selection changes.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected internal virtual void OnSelectedNodeChanged(EventArgs e)
        {
            if (SelectedNodeChanged != null)
                SelectedNodeChanged(this, e);
        }

        internal bool _HasExandHander()
        {
            return (Expand != null);
        }

        internal bool _HasCollapseHander()
        {
            return (Collapse != null);
        }

        /// <summary>
        /// Overridden. Verifies certain properties.
        /// </summary>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Controls.Clear();

            // Ensure that LoadPostData is called
            if (Page != null)
            {
                Page.RegisterRequiresPostBack(this);
            }


            // Databind from XML source
            if (!Page.IsPostBack)
            {

                foreach (TreeNode node in Nodes)
                {
                    node.OnInit();
                }
            }
        }

        /// <summary>
        /// On a postback, review state information so we can expand nodes as needed
        /// </summary>
        /// <param name="e"> </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _bCreated = true; // first chance to do this on non-postback

            // initialize SelectedNodeIndex, if needed
            if ((SelectedNodeIndex == "" || SelectedNodeIndex == String.Empty) && Nodes.Count > 0)
                SelectedNodeIndex = "0";

            TreeNode node = GetNodeFromIndex(SelectedNodeIndex);
            if (node != null)
                node._Selected = true;
        }

        ///// <summary>
        ///// Overridden. Creates an EmptyControlCollection to prevent controls from
        ///// being added to the ControlCollection.
        ///// </summary>
        ///// <returns>An EmptyControlCollection object.</returns>
        //protected override ControlCollection CreateControlCollection()
        //{
        //    return new EmptyControlCollection(this);
        //}

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            // Controls.Clear();
            m_Container = new Panel();
            m_Container.ID = this.ID + "Panel";
            Controls.Add(m_Container);
            foreach (TreeNode tn in Nodes)
            {
                tn.CreateChildControls(m_Container.Controls);
            }
        }

        /// <summary>
        /// Determines whether the FORMAT of the given index string is valid
        /// (this is, containing only digits and periods).  No validation of the
        /// actual index being referenced is made.
        /// </summary>
        /// <param name="strIndex">Index to validate</param>
        /// <returns>true if valid, false otherwise</returns>
        private bool IsValidIndex(string strIndex)
        {
            if (strIndex == null || strIndex == String.Empty)
                return true;

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("[^0-9.]");
            if (r.IsMatch(strIndex))    // a character other than a digit or .
                return false;

            if (strIndex[0] == '.')     // mustn't begin with a .
                return false;

            if (strIndex.IndexOf("..") != -1)   // mustn't have two consecutive periods
                return false;

            return true;
        }

        #endregion

        #region Properties

        [System.ComponentModel.Bindable(true), System.ComponentModel.Browsable(true), System.ComponentModel.Category("Images"), DefaultValue("")]
        internal String CheckedImageUrl
        {
            get
            {
                if (ViewState["_CheckedImageUrl"] == null)
                {
                    return "";
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
        internal String UncheckedImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return "";
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
        internal String UndefineImageUrl
        {
            get
            {
                if (ViewState["_UncheckedImageUrl"] == null)
                {
                    return "";
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

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Url of the image to display when not selected, expanded, or hovered
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when not selected, expanded, or hovered"),
        ]
        public String ImageUrl
        {
            get
            {
                object str = ViewState["ImageUrl"];
                return ((str == null) ? String.Empty : (String)str);
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
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when selected"),
        ]
        public String SelectedImageUrl
        {
            get
            {
                object str = ViewState["SelectedImageUrl"];
                return ((str == null) ? String.Empty : (String)str);
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
        Editor(typeof(System.Web.UI.Design.ImageUrlEditor), typeof(UITypeEditor)),
        Description("Url of the image to display when expanded"),
        ]
        public String ExpandedImageUrl
        {
            get
            {
                object str = ViewState["ExpandedImageUrl"];
                return ((str == null) ? String.Empty : (String)str);
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
        Description("id of the window to target with a navigation upon selecting this node"),
        ]
        public String Target
        {
            get
            {
                object str = ViewState["Target"];
                return ((str == null) ? String.Empty : (String)str);
            }
            set
            {
                ViewState["Target"] = value;
            }
        }

        [
        Category("Data"),
        DefaultValue(null),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerDefaultProperty),
        Description("Collection of child nodes"),
        ]
        public TreeNodeCollection Nodes
        {
            get
            {
                return _Nodes;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        Description("AutoPostBack"),
        ]
        public bool AutoPostBack
        {
            get
            {
                object b = ViewState["AutoPostBack"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["AutoPostBack"] = value;
            }
        }

        /// <summary>
        /// Show dotted lines representing tree hierarchy
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Show dotted lines representing tree hierarchy?"),
        ]
        public bool ShowLines
        {
            get
            {
                object b = ViewState["ShowLines"];
                return ((b == null) ? true : (bool)b);
            }
            set
            {
                ViewState["ShowLines"] = value;
            }
        }

        /// <summary>
        /// Show tooltips on parent nodes
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Show tooltips on parent nodes"),
        ]
        public bool ShowToolTip
        {
            get
            {
                object b = ViewState["ShowToolTip"];
                return ((b == null) ? true : (bool)b);
            }
            set
            {
                ViewState["ShowToolTip"] = value;
            }
        }
        /// <summary>
        /// Expand/collapse a node by clicking on it
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Expand/collapse a node by clicking on it"),
        ]
        public bool SelectExpands
        {
            get
            {
                object b = ViewState["SelectExpands"];
                return ((b == null) ? true : (bool)b);
            }
            set
            {
                ViewState["SelectExpands"] = value;
            }
        }

        /// <summary>
        /// Automatically select a node when hovered with the keyboard
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Automatically select a node when hovered with the keyboard"),
        ]
        public bool AutoSelect
        {
            get
            {
                object b = ViewState["AutoSelect"];
                return ((b == null) ? false : (bool)b);
            }
            set
            {
                ViewState["AutoSelect"] = value;
            }
        }

        /// <summary>
        /// If ShowLines=false, number of pixels to indent each level of tree
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(19),
        PersistenceMode(PersistenceMode.Attribute),
        Description("If ShowLines=false, number of pixels to indent each level of tree"),
        ]
        public int Indent
        {
            get
            {
                object i = ViewState["Indent"];
                return ((i == null) ? 19 : (int)i);
            }
            set
            {
                ViewState["Indent"] = value;
            }
        }

        /// <summary>
        /// Path to the directory holding images (lines, +, -, etc) required by the control
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Path to the directory holding images (lines, +, -, etc) required by the control"),
        ]
        public string SystemImagesPath
        {
            get
            {
                object str = ViewState["SystemImagesPath"];
                return ((str == null) ? "treeimages/" : (string)str);
            }
            set
            {
                String str = value;
                if (str.Length > 0 && str[str.Length - 1] != '/')
                    str = str + '/';
                ViewState["SystemImagesPath"] = str;
            }
        }

        /// <summary>
        /// Index of the selected node
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Index of the selected node"),
        ]
        public string SelectedNodeIndex
        {
            get
            {
                object str = ViewState["SelectedNodeIndex"];

                if (str == null)
                    return (Nodes.Count > 0) ? "0" : String.Empty;
                else
                    return (string)str;
            }
            set
            {
                if (IsValidIndex(value))
                {
                    TreeNode node = GetNodeFromIndex(value);
                    if (!_bCreated || (value == null) || (value == String.Empty) || ((node != null) && node.IsVisible))
                    {
                        if (value != (string)ViewState["SelectedNodeIndex"])
                            ViewState["SelectedNodeIndex"] = value;
                    }
                    else
                        throw new Exception(String.Format(Resources.ErrorMessages.errTreeInvisibleSelectedNode, value));
                }
                else
                    throw new Exception(String.Format(Resources.ErrorMessages.errTreeInvalidIndexFormat, value));
            }
        }

        /// <summary>
        /// TreeNode selected on the treeview, if any
        /// </summary>
        [
        Browsable(false),
        DefaultValue(null),
        Description("TreeNode selected on the treeview, if any")
        ]
        public TreeNode SelectedNode
        {
            get
            {
                return this.GetNodeFromIndex(SelectedNodeIndex);
            }
        }

        /// <summary>
        /// Number of levels deep to expand the tree by default
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(0),
        PersistenceMode(PersistenceMode.Attribute),
        Description("Number of levels deep to expand the tree by default"),
        ]
        public int ExpandLevel
        {
            get
            {
                object i = ViewState["ExpandLevel"];
                return ((i == null) ? 0 : (int)i);
            }
            set
            {
                ViewState["ExpandLevel"] = value;
            }
        }

        /// <summary>
        /// Color of the text within the control.
        /// </summary>
        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// The font used for text within the control.
        /// </summary>
        [Browsable(false)]
        public override FontInfo Font
        {
            get { return base.Font; }
        }

        /// <summary>
        /// The tooltip displayed when the mouse is over the control.
        /// </summary>
        [Browsable(false)]
        public override string ToolTip
        {
            get { return base.ToolTip; }
            set { base.ToolTip = value; }
        }

        /// <summary>
        /// Selected Node Style
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Description("Selected Node Style")
        ]
        public System.Web.UI.WebControls.Style SelectedNodeStyle
        {
            get
            {
                return _selectedNodeStyle;
            }
        }

        /// <summary>
        /// Node Style
        /// </summary>
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
        /// Hover Node Style
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Description("Hover Node Style")
        ]
        public System.Web.UI.WebControls.Style HoverNodeStyle
        {
            get
            {
                return _hoverNodeStyle;
            }
        }

        #endregion

        /// <summary>
        /// true if currently in design mode.
        /// </summary>
        protected bool IsDesignMode
        {
            get { return (Site != null) ? Site.DesignMode : false; }
        }
        /// <summary>
        /// Indicates that the SelectedNodeIndex can be modified on insertions.
        /// </summary>
        internal bool IsInitialized
        {
            get { return IsTrackingViewState; }
        }
        /// <summary>
        /// Returns the TreeNode at the given index location
        /// </summary>
        /// <param name="strIndex">string of dot-separated indices (e.g. "1.0.3") where 
        /// each index is the 0-based position of a node at the next deeper level of the tree</param>
        /// <returns>The TreeNode, if found, or null</returns>
        public TreeNode GetNodeFromIndex(string strIndex)
        {
            if (strIndex != null && strIndex.Length != 0)
            {
                // convert index string into array of strings
                string[] a = strIndex.Split(new Char[] { '.' });
                int i = 0;
                int index;
                TreeNodeCollection colNodes = Nodes;
                while (i < a.GetLength(0) - 1)
                {
                    index = Convert.ToInt32(a[i]);

                    if (index >= colNodes.Count)
                        return null;
                    colNodes = colNodes[index].ChildNodes;
                    i++;
                }
                index = Convert.ToInt32(a[i]);
                if (index >= colNodes.Count)
                    return null;
                else
                    return colNodes[index];
            }
            else
            {
                return null;
            }
        }

        #region privates
        /// <summary>
        /// Adds parsed child objects to the TreeView.
        /// </summary>
        /// <param name="obj">Child object to add, must be either a TreeNode or TreeNodeType.</param>
        protected override void AddParsedSubObject(Object obj)
        {
            if (obj is TreeNode)
            {
                _Nodes.Add((TreeNode)obj);
            }
        }



        /// <summary>
        ///  Searches for an object's parents for a Form object
        /// </summary>
        internal static Control FindForm(Control child)
        {
            Control parent = child;

            while (parent != null)
            {
                if (parent is System.Web.UI.HtmlControls.HtmlForm)
                    break;

                parent = parent.Parent;
            }

            return parent;
        }

        /// <summary>
        /// The ID of the hidden helper.
        /// </summary>
        protected virtual string HelperID
        {
            get
            {
                if (_szHelperID == null)
                {
                    _szHelperID = "__" + ClientID + "_State__";
                }

                return _szHelperID;
            }
        }
        /// <summary>
        /// Client-side script ID of the hidden helper.
        /// </summary>
        protected internal virtual string ClientHelperID
        {
            get
            {
                Control form = FindForm(this);
                if (form != null)
                {
                    return "document." + form.ClientID + "." + HelperID;
                }

                return HelperID;
            }
        }

        /// <summary>
        /// The data to store inside the hidden helper.
        /// </summary>
        protected virtual string HelperData
        {
            get { return _szHelperData; }
            set { _szHelperData = value; }
        }
        /// <summary>
        /// Processes post back data for a server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>true if the server control's state changes as a result of the post back; otherwise false.</returns>
        protected virtual bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            EnsureChildControls();


            string szData = postCollection[HelperID];
            if (szData != null)
            {
                return ProcessData(szData);
            }


            return false;
        }


        /// <summary>
        /// Sets the hidden helper's data if a hidden helper is needed.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register the hidden input
            if (Page != null)
            {
                Page.ClientScript.RegisterHiddenField(HelperID, HelperData);
            }

            if ((SelectedNodeIndex == "" || SelectedNodeIndex == String.Empty) && Nodes.Count > 0)
                SelectedNodeIndex = "0";
            else
                SelectedNodeIndex = SelectedNodeIndex; // verify current index

            // We only check HoverNodeIndex for validity here, because it's possible to have had many events
            // stacked up on the client if AutoPostBack=false.  The HoverNodeIndex might not validate until
            // after these events are processed, so we have to wait until now to check.
            if (!IsValidIndex(HoverNodeIndex))
                throw new Exception(String.Format(Resources.ErrorMessages.errTreeInvalidIndexFormat, HoverNodeIndex));

            foreach (TreeNode tn in Nodes)
            {
                tn.OnPrerender();
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.DesignMode)
            {
                EnsureChildControls();
            }
            base.Render(writer);
        }

        #region IPostBackDataHandler and IPostBackEventHandler
        // -------------------------------------------------------------------
        // Implementation of IPostBackDataHandler and IPostBackEventHandler
        // Override protected instance methods (at top) instead.
        // -------------------------------------------------------------------

        /// <summary>
        /// Processes post back data for a server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control.</param>
        /// <param name="postCollection">The collection of all incoming name values.</param>
        /// <returns>true if the server control's state changes as a result of the post back; otherwise false.</returns>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        /// <summary>
        /// Signals the server control object to notify the ASP.NET application that the state of the control has changed.
        /// </summary>
        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        /// <summary>
        /// Enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A String that represents an optional event argument to be passed to the event handler.</param>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            this.RaisePostBackEvent(eventArgument);
        }

        #endregion

        /// <summary>
        /// Process data being posted back.
        /// </summary>
        /// <param name="strData">The data that was posted.</param>
        /// <returns>true if data changed, false otherwise.</returns>
        protected bool ProcessData(String strData)
        {
            if (strData == null || strData == String.Empty)
                return false;

            // Split data using the pipe character as a delimeter
            String[] astrData = strData.Split(new Char[] { '|' });

            // Extract whether the tree was focused before the postback
            if (astrData[0] == "1")
                _bFocused = true;

            // Extract the hovered node index
            HoverNodeIndex = astrData[1];

            // Extract the scroll top value
            if (astrData[2] != String.Empty)
            {
                try
                {
                    _scrollTop = Convert.ToInt32(astrData[2]);
                }
                catch
                {
                    // Ignore
                }
            }

            // Extract the scroll left value
            if (astrData[3] != String.Empty)
            {
                try
                {
                    _scrollLeft = Convert.ToInt32(astrData[3]);
                }
                catch
                {
                    // Ignore
                }
            }

            // Extract the parent's scroll top value
            if (astrData[4] != String.Empty)
            {
                try
                {
                    _parentTop = Convert.ToInt32(astrData[4]);
                }
                catch
                {
                    // Ignore
                }
            }

            // Extract the parent's scroll left value
            if (astrData[5] != String.Empty)
            {
                try
                {
                    _parentLeft = Convert.ToInt32(astrData[5]);
                }
                catch
                {
                    // Ignore
                }
            }

            // Extract the queued events
            if (astrData[6] != null && astrData[6] != String.Empty)
                return ProcessEvents(astrData[6]);

            return false;
        }

        /// <summary>
        /// Called when a downlevel browser submits the form
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected void RaisePostBackEvent(string eventArg)
        {
            ProcessEvents(eventArg);
            RaisePostDataChangedEvent();
        }

        /// <summary>
        /// Called when the TreeView on the client-side submitted the form.
        /// </summary>
        /// <param name="eventArg">Event argument.</param>
        protected bool ProcessEvents(string eventArg)
        {
            if (eventArg == null || eventArg == String.Empty || eventArg == " ") // Don't know why, but the framework is giving a " " eventArg instead of null
                return false;

            TreeNode tn = null;
            String[] events = eventArg.Split(new Char[] { ';' });
            foreach (string strWholeEvent in events)
            {
                String[] parms = strWholeEvent.Split(new Char[] { ',' });
                if (parms[0].Length > 0)
                {
                    if (parms[0].Equals("onselectedindexchange") && parms.GetLength(0) == 3)
                    {
                        TreeViewSelectEventArgs e = new TreeViewSelectEventArgs(this.SelectedNodeIndex, parms[2]);
                        tn = GetNodeFromIndex(parms[2]);
                        if (tn != null)
                            tn.LowerPostBackEvent(this.SelectedNodeIndex);
                        DoSelectedIndexChange(e);
                        _eventList.Add("s");
                        _eventList.Add(e);
                    }
                    else if ((parms[0].Equals("onexpand") || parms[0].Equals("oncollapse") || parms[0].Equals("oncheck")) && parms.GetLength(0) == 2)
                    {
                        TreeViewClickEventArgs e = new TreeViewClickEventArgs(parms[1]);
                        if (parms[0].Equals("onexpand"))
                            _eventList.Add("e");
                        else if (parms[0].Equals("oncollapse"))
                            _eventList.Add("c");
                        else
                            _eventList.Add("k");
                        _eventList.Add(e);
                        tn = GetNodeFromIndex(parms[1]);
                        if (tn != null)
                            tn.LowerPostBackEvent(parms[0]);
                    }
                }
            }
            if (_eventList.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Fires pending events from a postback
        /// </summary>
        protected void RaisePostDataChangedEvent()
        {
            for (int i = 0; i < _eventList.Count; i += 2)
            {
                String str = (String)_eventList[i];
                if (str == "s")
                    OnSelectedIndexChange((TreeViewSelectEventArgs)_eventList[i + 1]);
                else if (str == "e")
                    OnExpand((TreeViewClickEventArgs)_eventList[i + 1]);
                else if (str == "c")
                    OnCollapse((TreeViewClickEventArgs)_eventList[i + 1]);
                else
                    OnCheck((TreeViewClickEventArgs)_eventList[i + 1]);
            }
            _eventList.Clear();
        }

        private TreeNodeCollection GetNodeCollection(Object obj)
        {
            if (obj is TreeView)
                return ((TreeView)obj).Nodes;
            else if (obj is TreeNode)
                return ((TreeNode)obj).ChildNodes;
            else
                throw new Exception(Resources.ErrorMessages.errInvalidTreeViewObject);
        }

        internal object GetStateVar(String att)
        {
            if (att.EndsWith("Style"))
                return typeof(TreeView).InvokeMember(att, BindingFlags.Default | BindingFlags.GetProperty, null, this, new object[] { });
            else
                return ViewState[att];
        }

        /// <summary>
        /// Loads the control's previously saved view state.
        /// </summary>
        /// <param name="savedState">An object that contains the saved view state values for the control.</param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] state = (object[])savedState;

                base.LoadViewState(state[0]);
                ((IStateManager)Nodes).LoadViewState(state[1]);
                ((IStateManager)_NodeStyle).LoadViewState(state[2]);
                ((IStateManager)_selectedNodeStyle).LoadViewState(state[3]);
                ((IStateManager)_hoverNodeStyle).LoadViewState(state[4]);
            }
            _bCreated = true;
        }

        /// <summary>
        /// Saves the changes to the control's view state to an Object.
        /// </summary>
        /// <returns>The object that contains the view state changes.</returns>
        protected override object SaveViewState()
        {
            object[] state = new object[]
            {
                base.SaveViewState(),
                ((IStateManager)Nodes).SaveViewState(),
                ((IStateManager)_NodeStyle).SaveViewState(),
                ((IStateManager)_selectedNodeStyle).SaveViewState(),
                ((IStateManager)_hoverNodeStyle).SaveViewState(),
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
        /// Instructs the control to track changes to its view state.
        /// </summary>
        protected override void TrackViewState()
        {
            base.TrackViewState();

            ((IStateManager)Nodes).TrackViewState();
        }

        /// <summary>
        /// Index of the hovered node
        /// </summary>
        internal string HoverNodeIndex
        {
            get
            {
                object str = ViewState["HoverNodeIndex"];
                return ((str == null) ? String.Empty : (string)str);
            }
            set
            {
                if ((string)ViewState["HoverNodeIndex"] != value)
                    ViewState["HoverNodeIndex"] = value;
            }
        }

        bool DescriptorsDone = false;
        protected override IEnumerable<ScriptDescriptor>
                GetScriptDescriptors()
        {
            if (DescriptorsDone)
            {
                ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Axelerate.BusinessLayerUITools.WebControls.TreeView", m_Container.ClientID);
                //descriptor.AddProperty("state", State);
                //descriptor.AddElementProperty("state", _State.ClientID);
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

                descriptor.AddProperty("ImageUrl", this.ImageUrl);
                descriptor.AddProperty("SelectedImageUrl", this.SelectedImageUrl);
                descriptor.AddProperty("ExpandedImageUrl", this.ExpandedImageUrl);
                descriptor.AddProperty("Target", this.Target);
                descriptor.AddProperty("AutoPostBack", this.AutoPostBack);
                descriptor.AddProperty("ShowLines", this.ShowLines);
                descriptor.AddProperty("ShowToolTip", this.ShowToolTip);
                descriptor.AddProperty("SelectExpands", this.SelectExpands);
                descriptor.AddProperty("AutoSelect", this.AutoSelect);

                descriptor.AddProperty("NodeStyle", this._NodeStyle.GetStyleAttributes(this).Value);
                descriptor.AddProperty("SelectedNodeStyle", this._selectedNodeStyle.GetStyleAttributes(this).Value);
                descriptor.AddProperty("HoverNodeStyle", this._hoverNodeStyle.GetStyleAttributes(this).Value);

                descriptor.AddProperty("NodeCss", this._NodeStyle.CssClass);
                descriptor.AddProperty("SelectedNodeCss", this._selectedNodeStyle.CssClass);
                descriptor.AddProperty("HoverNodeCss", this._hoverNodeStyle.CssClass);

                string ChildNodesIDs = "";
                foreach (TreeNode tn in this.Nodes)
                {
                    if (ChildNodesIDs != "")
                    {
                        ChildNodesIDs += ";";
                    }
                    ChildNodesIDs += tn._NodeElement.ClientID;
                    foreach (ScriptDescriptor sd in tn.GetScriptDescriptors())
                    {
                        yield return sd;
                    }
                }
                descriptor.AddProperty("Nodes", ChildNodesIDs);

                yield return descriptor;
                //this._Nodes = new Array();
            }
            else
            {
                DescriptorsDone = true;
            }


        }

        // Generate the script reference
        protected override IEnumerable<ScriptReference>
                GetScriptReferences()
        {
            yield return new ScriptReference("Axelerate.BusinessLayerUITools.WebControls.TreeView.js", this.GetType().Assembly.FullName);
        }
        #endregion
    }


    internal class TreeViewDesigner : System.Web.UI.Design.ControlDesigner
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the control can be resized.
        /// </summary>
        public override bool AllowResize
        {
            get { return true; }
        }

        /// <summary>
        /// Retrieves the HTML to display in the designer.
        /// </summary>
        /// <returns>The design-time HTML.</returns>
        public override string GetDesignTimeHtml()
        {
            TreeView tree = (TreeView)Component;

            // If the TreeView is empty, then show instructions
            if (tree.Nodes.Count == 0)
            {
                return CreatePlaceHolderDesignTimeHtml("Add nodes to the tree");
            }
            //System.IO.StringWriter strWriter = new System.IO.StringWriter();
            //HtmlTextWriter output = new HtmlTextWriter(strWriter);
            //tree.RenderControl(output);

            return base.GetDesignTimeHtml();
        }

    }
}
