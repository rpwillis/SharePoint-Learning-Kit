using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.ComponentModel;

namespace Axelerate.BusinessLayerUITools.WebControls
{
    public class TreeNodeCollection : CollectionBase, ICloneable, IStateManager
    {
        private Object _Parent;
        private TreeNode _tnSelected;
        private bool _Tracking = false;
        private bool _Reloading = false;
        private ArrayList _Actions = new ArrayList(4);


        #region privates

        /// <summary>
        /// List of actions made to this collection. Used for state tracking.
        /// </summary>
        private ArrayList Actions
        {
            get { return _Actions; }
        }

        /// <summary>
        /// Type of action made to this collection.
        /// </summary>
        private enum ActionType { Clear, Insert, Remove };

        /// <summary>
        /// Stores information about an action made to this collection
        /// </summary>
        private class Action
        {
            /// <summary>
            /// The type of action that this object represents (Clear, Insert, or Remove)
            /// </summary>
            public ActionType ActionType;

            /// <summary>
            /// The index at which this action occurred.
            /// </summary>
            public int Index;

            private string nodeType = String.Empty;
            private int nodeTypeIndex = -1;
            private static string[] knownTypes =
            {
                typeof(Axelerate.BusinessLayerUITools.WebControls.TreeNode).AssemblyQualifiedName,
            };

            /// <summary>
            /// The type name of the node that is being inserted.
            /// </summary>
            public string NodeType
            {
                get { return nodeType; }
                set
                {
                    nodeType = value;
                    int index = System.Array.IndexOf(knownTypes, nodeType);
                    if (index >= 0)
                    {
                        nodeTypeIndex = index;
                    }
                }
            }

            /// <summary>
            /// Loads an Action from ViewState.
            /// </summary>
            /// <param name="stateObj">The state object.</param>
            public void Load(object stateObj)
            {
                if (stateObj != null)
                {
                    object[] state = (object[])stateObj;
                    ActionType = (ActionType)state[0];

                    switch (ActionType)
                    {
                        case ActionType.Insert:
                            Index = (int)state[1];
                            if (state[2] is string)
                            {
                                nodeType = (string)state[2];
                            }
                            else
                            {
                                // Load from an index
                                nodeTypeIndex = (int)state[2];
                                nodeType = (string)knownTypes[nodeTypeIndex];
                            }
                            break;

                        case ActionType.Remove:
                            Index = (int)state[1];
                            break;
                    }
                }
            }

            /// <summary>
            /// Saves an Action to ViewState.
            /// </summary>
            /// <returns>The state object.</returns>
            public object Save()
            {
                object[] state;
                switch (ActionType)
                {
                    case ActionType.Insert:
                        state = new object[3];
                        state[1] = Index;
                        state[2] = (nodeTypeIndex >= 0) ? (object)nodeTypeIndex : (object)nodeType;
                        break;

                    case ActionType.Remove:
                        state = new object[2];
                        state[1] = Index;
                        break;

                    default:
                        state = new object[1];
                        break;
                }
                state[0] = ActionType;

                return state;
            }

            /// <summary>
            /// Clones an Action object.
            /// </summary>
            /// <returns>The copy.</returns>
            public Action Clone()
            {
                Action action = new Action();
                action.ActionType = this.ActionType;
                action.Index = this.Index;
                action.nodeType = this.nodeType;
                action.nodeTypeIndex = this.nodeTypeIndex;
                return action;
            }
        }

        /// <summary>
        /// Sets the entire view state for the collection and everything under it to dirty
        /// </summary>
        internal virtual void SetViewStateDirty()
        {
            if (!((IStateManager)this).IsTrackingViewState)
            {
                ((IStateManager)this).TrackViewState();
            }

            Actions.Clear();
            Action action = new Action();
            action.ActionType = ActionType.Clear;
            Actions.Add(action);
            for (int index = 0; index < Count; index++)
            {
                TreeNode item = (TreeNode)List[index];

                action = new Action();
                action.ActionType = ActionType.Insert;
                action.Index = index;
                action.NodeType = item.GetType().FullName;

                Actions.Add(action);

                item.SetViewStateDirty();
            }
        }


        /// <summary>
        /// Recreates a node of type nodeTypeName and inserts it into the index location.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the node.</param>
        /// <param name="nodeTypeName">The name of the type of the node.</param>
        private void RedoInsert(int index, String nodeTypeName)
        {
            try
            {
                // Create a Type object from the type name
                Type nodeType = Type.GetType(nodeTypeName);

                if (nodeType != null)
                {
                    // Invoke the constructor, creating the object
                    object node = Activator.CreateInstance(nodeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);

                    // Insert the node if creation was successful
                    if ((node != null) && (node is TreeNode))
                    {
                        List.Insert(index, node);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        /// <summary>
        /// Gets a value indicating whether a server control is tracking its view state changes.
        /// </summary>
        bool IStateManager.IsTrackingViewState
        {
            get { return _Tracking; }
        }

        /// <summary>
        /// Indicates whether the collection is reloading its items
        /// </summary>
        internal bool Reloading
        {
            get { return _Reloading; }
        }

        /// <summary>
        /// Loads the collection's previously saved view state.
        /// </summary>
        /// <param name="state">An Object that contains the saved view state values for the collection.</param>
        void IStateManager.LoadViewState(object state)
        {
            if (state == null)
            {
                return;
            }

            object[] viewState = (object[])state;

            if (viewState[0] != null)
            {
                _Reloading = true;

                // Restore and re-do actions
                object[] actions = (object[])viewState[0];

                ArrayList newActionList = new ArrayList();
                foreach (object actionState in actions)
                {
                    Action action = new Action();
                    action.Load(actionState);

                    newActionList.Add(action);

                    switch (action.ActionType)
                    {
                        case ActionType.Clear:
                            List.Clear();
                            break;

                        case ActionType.Remove:
                            List.RemoveAt(action.Index);
                            break;

                        case ActionType.Insert:
                            RedoInsert(action.Index, action.NodeType);
                            break;
                    }
                }

                _Actions = newActionList;
                _Reloading = false;
            }

            if (viewState[1] != null)
            {
                // Load view state changes

                object[] lists = (object[])viewState[1];
                ArrayList indices = (ArrayList)lists[0];
                ArrayList items = (ArrayList)lists[1];

                for (int i = 0; i < indices.Count; i++)
                {
                    ((IStateManager)List[(int)indices[i]]).LoadViewState(items[i]);
                }
            }
        }

        /// <summary>
        /// Saves the changes to the collection's view state to an Object.
        /// </summary>
        /// <returns>The Object that contains the view state changes.</returns>
        object IStateManager.SaveViewState()
        {
            object[] state = new object[2];

            if (Actions.Count > 0)
            {
                // Save the actions made to the collection
                object[] obj = new object[Actions.Count];
                for (int i = 0; i < Actions.Count; i++)
                {
                    obj[i] = ((Action)Actions[i]).Save();
                }
                state[0] = obj;
            }

            if (List.Count > 0)
            {
                // Save the view state changes of the child nodes

                ArrayList indices = new ArrayList(4);
                ArrayList items = new ArrayList(4);

                for (int i = 0; i < List.Count; i++)
                {
                    object item = ((IStateManager)List[i]).SaveViewState();
                    if (item != null)
                    {
                        indices.Add(i);
                        items.Add(item);
                    }
                }

                if (indices.Count > 0)
                {
                    state[1] = new object[] { indices, items };
                }
            }

            if ((state[0] != null) || (state[1] != null))
            {
                return state;
            }

            return null;
        }

        /// <summary>
        /// Instructs the collection to track changes to its view state.
        /// </summary>
        void IStateManager.TrackViewState()
        {
            _Tracking = true;

            // Tracks view state changes in the child nodes
            foreach (TreeNode node in List)
            {
                ((IStateManager)node).TrackViewState();
            }
        }
        
        /// <summary>
        /// Creates a new deep copy of the current collection.
        /// </summary>
        /// <returns>A new object that is a deep copy of this instance.</returns>
        public virtual object Clone()
        {
            TreeNodeCollection copy = (TreeNodeCollection)Activator.CreateInstance(this.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);

            foreach (TreeNode node in List)
            {
                copy.List.Add(node.Clone());
            }

            copy._Tracking = this._Tracking;
            copy._Reloading = this._Reloading;
            copy._Actions.Clear();
            foreach (Action action in this._Actions)
            {
                copy._Actions.Add(action.Clone());
            }

            return copy;
        }

        /// <summary>
        /// Mark all nodes as expanded, sending the respecive event
        /// </summary>
        public void ExpandAll()
        {
            foreach (TreeNode node in this)
            {
                node.ExpandAll();
            }
        }
        
        /// <summary>
        /// Mark all nodes as collapsed, sending the respecive event
        /// </summary>
        public void CollapseAll()
        {
            foreach (TreeNode node in this)
            {
                node.CollapseAll();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of a TreeNodeCollection.
        /// </summary>
        /// <param name="parent">The parent TreeNode of this collection.</param>
        public TreeNodeCollection(Object parent)
            : base()
        {
            _Parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of a TreeNodeCollection.
        /// </summary>
        public TreeNodeCollection()
            : base()
        {
            _Parent = null;
        }

        /// <summary>
        /// The parent object of this collection.
        /// </summary>
        public Object Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the item being inserted.</param>
        /// <param name="value">The item being inserted.</param>
        protected override void OnInsert(int index, object value)
        {

            if (((TreeNode)value)._Parent != null)
            {
                //throw new Exception(Resources.ErrorMessages.errTreeNodeAlreadyInCollection);
            }

            SetItemProperties((TreeNode)value);

            if (!Reloading)
            {
                TreeView tv;
                if (Parent is TreeNode)
                    tv = ((TreeNode)Parent).ParentTreeView;
                else
                    tv = (TreeView)Parent;

                if ((tv != null) && tv.IsInitialized)
                {
                    _tnSelected = tv.GetNodeFromIndex(tv.SelectedNodeIndex);
                }
            }

            base.OnInsert(index, value);

            if (((IStateManager)this).IsTrackingViewState)
            {
                Action action = new Action();
                action.ActionType = ActionType.Insert;
                action.Index = index;
                action.NodeType = value.GetType().AssemblyQualifiedName;

                Actions.Add(action);

                // The new node needs to start tracking its view state
                ((IStateManager)value).TrackViewState();
                ((TreeNode)value).SetViewStateDirty();
            }

        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the item being inserted.</param>
        /// <param name="value">The item being inserted.</param>
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);

            if (!Reloading)
            {
                TreeView tv;
                if (Parent is TreeNode)
                    tv = ((TreeNode)Parent).ParentTreeView;
                else
                    tv = (TreeView)Parent;

                if ((tv != null) && tv.IsInitialized)
                {
                    if (_tnSelected != null)
                        tv.SelectedNodeIndex = _tnSelected.GetNodeIndex();
                    else
                        tv.SelectedNodeIndex = "0";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the item being changed.</param>
        /// <param name="oldValue">The old item.</param>
        /// <param name="newValue">The new item.</param>
        protected override void OnSet(int index, object oldValue, object newValue)
        {
            SetItemProperties((TreeNode)newValue);

            base.OnSet(index, oldValue, newValue);
            if (((IStateManager)this).IsTrackingViewState &&
               !oldValue.GetType().Equals(newValue.GetType()))
            {
                Action action = new Action();
                action.ActionType = ActionType.Remove;
                action.Index = index;

                Actions.Add(action);

                action = new Action();
                action.ActionType = ActionType.Insert;
                action.Index = index;
                action.NodeType = newValue.GetType().FullName;

                Actions.Add(action);

                // The new node needs to start tracking its view state
                ((IStateManager)newValue).TrackViewState();
                ((TreeNode)newValue).SetViewStateDirty();
            }
        }

        /// <summary>
        /// Sets properties of the TreeNode before being added.
        /// </summary>
        /// <param name="item">The TreeNode to be set.</param>
        private void SetItemProperties(TreeNode item)
        {
            item._Parent = Parent;
        }

        /// <summary>
        /// Adds a TreeNode to the collection.
        /// </summary>
        /// <param name="item">The TreeNode to add.</param>
        public void Add(TreeNode item)
        {
            List.Add(item);
        }

        /// <summary>
        /// Adds a TreeNode to the collection at a specific index.
        /// </summary>
        /// <param name="index">The index at which to add the item.</param>
        /// <param name="item">The TreeNode to add.</param>
        public void AddAt(int index, TreeNode item)
        {
            List.Insert(index, item);
        }

        /// <summary>
        /// Determines if a TreeNode is in the collection.
        /// </summary>
        /// <param name="item">The TreeNode to search for.</param>
        /// <returns>true if the TreeNode exists within the collection. false otherwise.</returns>
        public bool Contains(TreeNode item)
        {
            return List.Contains(item);
        }

        /// <summary>
        /// Determines zero-based index of a TreeNode within the collection.
        /// </summary>
        /// <param name="item">The TreeNode to locate within the collection.</param>
        /// <returns>The zero-based index.</returns>
        public int IndexOf(TreeNode item)
        {
            return List.IndexOf(item);
        }

        /// <summary>
        /// Removes a TreeNode from the collection.
        /// </summary>
        /// <param name="item">The TreeNode to remove.</param>
        public void Remove(TreeNode item)
        {
            List.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the item being removed.</param>
        /// <param name="value">The item being removed.</param>
        protected override void OnRemove(int index, object value)
        {
            if (!Reloading)
            {
                TreeNode node = (TreeNode)value;
                TreeView tv = node.ParentTreeView;
                if (tv != null)
                {
                    if (tv.SelectedNodeIndex.IndexOf(node.GetNodeIndex()) == 0)
                    {
                        // The node being removed is the selected node or one of its parents
                        TreeNode newNode = null;
                        if (Count > 1)
                        {
                            // Set the new selected node index to the next node
                            // or the previous one if the node is the last node
                            if (index == (Count - 1))
                            {
                                newNode = this[index - 1];
                            }
                            else
                            {
                                newNode = this[index + 1];
                            }
                        }
                        else if ((Parent != null) && (Parent is TreeNode))
                        {
                            // There are no other nodes in this collection, so
                            // try setting to its parent
                            newNode = (TreeNode)Parent;
                        }

                        _tnSelected = newNode;
                    }
                    else
                    {
                        // The selected node does not need to change, but its
                        // index may be affected by this removal.
                        _tnSelected = tv.GetNodeFromIndex(tv.SelectedNodeIndex);
                    }
                }
            }

            base.OnRemove(index, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">The index of the item being removed.</param>
        /// <param name="value">The item being removed.</param>
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            if (((IStateManager)this).IsTrackingViewState)
            {
                Action action = new Action();
                action.ActionType = ActionType.Remove;
                action.Index = index;

                Actions.Add(action);
            }

            TreeNode node = (TreeNode)value;
            node.ParentTreeView = null;
            node._Parent = null;

            TreeView tv = (Parent is TreeNode) ? ((TreeNode)Parent).ParentTreeView : (TreeView)Parent;
            if (!Reloading && (tv != null))
            {
                if (_tnSelected != null)
                {
                    tv.SelectedNodeIndex = _tnSelected.GetNodeIndex();
                }
                else
                {
                    tv.SelectedNodeIndex = null;
                }

                if (tv.HoverNodeIndex != null && tv.GetNodeFromIndex(tv.HoverNodeIndex) == null)
                {
                    tv.HoverNodeIndex = "";
                }
            }
        }
       
        /// <summary>
        /// Adjusts the SelectedNodeIndex of the TreeView when a clear is performed.
        /// </summary>
        protected override void OnClear()
        {
            bool needClear = (Count > 0);
            if (!Reloading && (Parent != null))
            {
                if (Parent is TreeView)
                {
                    ((TreeView)Parent).SelectedNodeIndex = null;
                }
                else
                {
                    TreeView tv = ((TreeNode)Parent).ParentTreeView;
                    if (tv != null)
                    {
                        string parentIndex = ((TreeNode)Parent).GetNodeIndex();
                        if (tv.SelectedNodeIndex.IndexOf(parentIndex) == 0)
                        {
                            tv.SelectedNodeIndex = parentIndex;
                        }
                    }
                }
            }

            base.OnClear();
            if (needClear && ((IStateManager)this).IsTrackingViewState)
            {
                // Since the entire list is cleared, all prior
                // actions do not need to be saved
                Actions.Clear();

                Action action = new Action();
                action.ActionType = ActionType.Clear;

                Actions.Add(action);
            }
        }

        /// <summary>
        /// Indexer into the collection.
        /// </summary>
        public TreeNode this[int index]
        {
            get { return (TreeNode)List[index]; }
        }
    }
}
