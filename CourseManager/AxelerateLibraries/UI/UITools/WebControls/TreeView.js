/// <reference name="MicrosoftAjax.js"/>


Type.registerNamespace("Axelerate.BusinessLayerUITools.WebControls");

Axelerate.BusinessLayerUITools.WebControls.TreeView = function(element) {
    Axelerate.BusinessLayerUITools.WebControls.TreeView.initializeBase(this, [element]);
   
    //checkbox properties
    this._CheckedImageUrl = null;
    this._UncheckedImageUrl = null;
    this._UndefineImageUrl = null;
    
    //Treeview properties
    this._ImageUrl = null;
    this._SelectedImageUrl = null;
    this._ExpandedImageUrl = null;
    this._Target = null;
    this._AutoPostBack = null;
    this._ShowLines = null;
    this._ShowToolTip = null;
    this._SelectExpands = null;
    this._AutoSelect = null;
    
    this._NodeStyle = null;
    this._SelectedNodeStyle = null;
    this._HoverNodeStyle = null;
    
    this._NodeCss = null;
    this._SelectedNodeCss = null;
    this._HoverNodeCss = null;
    
    
    this._Nodes = new Array();
    
}

Axelerate.BusinessLayerUITools.WebControls.TreeView.prototype = {
    initialize: function() {
        Axelerate.BusinessLayerUITools.WebControls.TreeView.callBaseMethod(this, 'initialize');
         
    },
 
    get_NodeStyle: function () {
        return this._NodeStyle;
    },
    set_NodeStyle: function(value) {
        if (this._NodeStyle !== value) {
            this._NodeStyle = value;
            this.raisePropertyChanged('NodeStyle');
        }
    },
    
    get_SelectedNodeStyle: function () {
        return this._SelectedNodeStyle;
    },
    set_SelectedNodeStyle: function(value) {
        if (this._SelectedNodeStyle !== value) {
            this._SelectedNodeStyle = value;
            this.raisePropertyChanged('SelectedNodeStyle');
        }
    },
    
    get_HoverNodeStyle: function () {
        return this._HoverNodeStyle;
    },
    set_HoverNodeStyle: function(value) {
        if (this._HoverNodeStyle !== value) {
            this._HoverNodeStyle = value;
            this.raisePropertyChanged('HoverNodeStyle');
        }
    },
    
    get_NodeCss: function () {
        return this._NodeCss;
    },
    set_NodeCss: function(value) {
        if (this._NodeCss !== value) {
            this._NodeCss = value;
            this.raisePropertyChanged('NodeCss');
        }
    },
    
    get_SelectedNodeCss: function () {
        return this._SelectedNodeCss;
    },
    set_SelectedNodeCss: function(value) {
        if (this._SelectedNodeCss !== value) {
            this._SelectedNodeCss = value;
            this.raisePropertyChanged('SelectedNodeCss');
        }
    },
    
    get_HoverNodeCss: function () {
        return this._HoverNodeCss;
    },
    set_HoverNodeCss: function(value) {
        if (this._HoverNodeCss !== value) {
            this._HoverNodeCss = value;
            this.raisePropertyChanged('HoverNodeCss');
        }
    },
    
    get_ImageUrl: function () {
        return this._ImageUrl;
    },
    set_ImageUrl: function(value) {
        if (this._ImageUrl !== value) {
            this._ImageUrl = value;
            this.raisePropertyChanged('ImageUrl');
        }
    },
    
    get_SelectedImageUrl: function () {
        return this._SelectedImageUrl;
    },
    set_SelectedImageUrl: function(value) {
        if (this._SelectedImageUrl !== value) {
            this._SelectedImageUrl = value;
            this.raisePropertyChanged('SelectedImageUrl');
        }
    },
    
    get_ExpandedImageUrl: function () {
        return this._ExpandedImageUrl;
    },
    set_ExpandedImageUrl: function(value) {
        if (this._ExpandedImageUrl !== value) {
            this._ExpandedImageUrl = value;
            this.raisePropertyChanged('ExpandedImageUrl');
        }
    },
   
    get_Target: function () {
        return this._Target;
    },
    set_Target: function(value) {
        if (this._Target !== value) {
            this._Target = value;
            this.raisePropertyChanged('Target');
        }
    },
    
    get_ShowLines: function () {
        return this._ShowLines;
    },
    set_ShowLines: function(value) {
        if (this._ShowLines !== value) {
            this._ShowLines = value;
            this.raisePropertyChanged('ShowLines');
        }
    },
    
    get_AutoPostBack: function () {
        return this._AutoPostBack;
    },
    set_AutoPostBack: function(value) {
        if (this._AutoPostBack !== value) {
            this._AutoPostBack = value;
            this.raisePropertyChanged('AutoPostBack');
        }
    },
    
    get_ShowToolTip: function () {
        return this._ShowToolTip;
    },
    set_ShowToolTip: function(value) {
        if (this._ShowToolTip !== value) {
            this._ShowToolTip = value;
            this.raisePropertyChanged('ShowToolTip');
        }
    },
    
    get_SelectExpands: function () {
        return this._SelectExpands;
    },
    set_SelectExpands: function(value) {
        if (this._SelectExpands !== value) {
            this._SelectExpands = value;
            this.raisePropertyChanged('SelectExpands');
        }
    },
    
    get_AutoSelect: function () {
        return this._SelectExpands;
    },
    set_AutoSelect: function(value) {
        if (this._SelectExpands !== value) {
            this._SelectExpands = value;
            this.raisePropertyChanged('SelectExpands');
        }
    },
    
    get_CheckedImageUrl: function () {
        return this._CheckedImageUrl;
    },
    set_CheckedImageUrl: function(value) {
        if (this._CheckedImageUrl !== value) {
            this._CheckedImageUrl = value;
            this.raisePropertyChanged('CheckedImageUrl');
        }
    },
    get_UncheckedImageUrl: function () {
        return this._UncheckedImageUrl;
    },
    set_UncheckedImageUrl: function(value) {
        if (this._UncheckedImageUrl !== value) {
            this._UncheckedImageUrl = value;
            this.raisePropertyChanged('UncheckedImageUrl');
        }
    },
    get_UndefineImageUrl: function () {
        return this._UndefineImageUrl;
    },
    set_UndefineImageUrl: function(value) {
        if (this._UndefineImageUrl !== value) {
            this._UndefineImageUrl = value;
            this.raisePropertyChanged('UndefineImageUrl');
        }
    },
    
    get_Nodes: function () {
        return this._Nodes;
    },
    set_Nodes: function(value) {
        var nodesids = value.split(";");
        for(var ni=0; ni < nodesids.length; ni++) 
        {
            var childnode = $find(nodesids[ni]);
            
            if(childnode != null) {
                childnode.set_TreeView(this);
                childnode._parent = null;
                
                this._Nodes[this._Nodes.length]= childnode;
            }
        }
       
        
        this.raisePropertyChanged('Nodes');
    },
    
    dispose: function() {        
        //Add custom dispose actions here
        
        Axelerate.BusinessLayerUITools.WebControls.TreeView.callBaseMethod(this, 'dispose');
    }
}

Axelerate.BusinessLayerUITools.WebControls.TreeNode = function(element) {
    Axelerate.BusinessLayerUITools.WebControls.TreeNode.initializeBase(this, [element]);
   
    //checkbox properties
    
    this._treeview = null;
    this._parent = null;
    
    this._state = null; //checkboxstate
    this._expanded = null; //store the expanded state for this node.
    this._level = null;
    this._nodeindex = null;
    this._ImageUrl = null;
    this._SelectedImageUrl = null;
    this._ExpandedImageUrl = null;
    this._Target = null;
    this._Expandable = null;
    this._Text = null;
    this._NavigateUrl = null;
    this._Expanded = null;
    this._NodeData = null;
    this._Selected = null;
    this._ShowCheckBox = null;
    this._SibIndex = null;
    this._ExpandedButtonImageUrl = null;
    this._CollapseButtonImageUrl = null;
    this._Expandable = null;
    
    this._TriggerExpandEvent = null;
    this._TriggerCollapseEvent = null;
    this._TriggerSelectEvent = null;
    
    this._NodeStyle = null;
    this._NodeCss = null;
    
    this._Nodes = new Array();
    
    this.imgCEButtonElement = null;
    this.imgChkElement = null;
    this.hplElement = null;
    this.tbElement = null;
    this.nodeLink = null;
    this.ChildNodesDiv = null;
    
}

Axelerate.BusinessLayerUITools.WebControls.TreeNode.prototype = {
    initialize: function() {
        Axelerate.BusinessLayerUITools.WebControls.TreeNode.callBaseMethod(this, 'initialize');
        
        // Add custom initialization here
        this._onselectHandler = Function.createDelegate(this, this._onSelect);
        this._oncheckHandler = Function.createDelegate(this, this._onCheck);
        this._onmouseoverHandler = Function.createDelegate(this, this._onMouseOver);
        this._onmouseoutHandler = Function.createDelegate(this, this._onMouseOut);
        this._onclickExpandCollapseButtonHandler = Function.createDelegate(this, this._onClickExpandCollapseButton);
       
        /*var imgCEButtonElement = $get("CollapseExpandImg_" + this._nodeindex, this.get_element());
        var imgChkElement = $get("ImageCheckBox_" + this._nodeindex, this.get_element());
        var hplElement = $get("Text_" + this._nodeindex, this.get_element());
        var tbElement = $get("TableNode_" + this._nodeindex, this.get_element());*/
        
        $addHandlers(this.tbElement, 
                 { 'mouseover' : this._onMouseOver},
                 this);
        $addHandlers(this.tbElement, 
                 { 'mouseout' : this._onMouseOut},
                 this);
        
        $addHandlers(this.imgChkElement, 
                 { 'click' : this._onCheck},
                 this);
                 
        if(this._Expandable) {
            $addHandlers(this.imgCEButtonElement, 
                 { 'click' : this._onClickExpandCollapseButton},
                 this);
        }
        
        $addHandlers(this.imgCEButtonElement, 
                 { 'click' : this._onSelect},
                 this);
       
        if(this._state.value == "Checked") {
            this.imgChkElement.src = this._CheckedImageUrl;
        }
        else {
                if(this._state.value == "Unchecked") {
                    this.imgChkElement.src = this._UncheckedImageUrl;
                }
                else {
                    if(this._state.value == "Undefine") {
                        this.imgChkElement.src = this._UndefineImageUrl;
                    }
               }
        }
       // imgElement.src = "Images/treeimages/T.gif";
       
    },
    
    get_NodeCss: function () {
        return this._NodeCss;
    },
    set_NodeCss: function(value) {
        if (this._NodeCss !== value) {
            this._NodeCss = value;
            this.raisePropertyChanged('NodeCss');
        }
    },
    
     get_NodeStyle: function () {
        return this._NodeStyle;
    },
    set_NodeStyle: function(value) {
        if (this._NodeStyle !== value) {
            this._NodeStyle = value;
            this.raisePropertyChanged('NodeStyle');
        }
    },
    get_ChildNodesDiv : function() {
        return this.ChildNodesDiv;
    },

    set_ChildNodesDiv : function(value) {
        if (this.ChildNodesDiv !== value) {
            this.ChildNodesDiv = value;
            this.raisePropertyChanged('ChildNodesDiv');
        }
    },
    
    get_imgCEButtonElement : function() {
        return this.imgCEButtonElement;
    },

    set_imgCEButtonElement : function(value) {
        if (this.imgCEButtonElement !== value) {
            this.imgCEButtonElement = value;
            this.raisePropertyChanged('imgCEButtonElement');
        }
    },
    
    get_imgChkElement : function() {
        return this.imgChkElement;
    },

    set_imgChkElement : function(value) {
        if (this.imgChkElement !== value) {
            this.imgChkElement = value;
            this.raisePropertyChanged('imgChkElement');
        }
    },
    
    get_hplElement : function() {
        return this.hplElement;
    },

    set_hplElement : function(value) {
        if (this.hplElement !== value) {
            this.hplElement = value;
            this.raisePropertyChanged('hplElement');
        }
    },
    
    get_tbElement : function() {
        return this.tbElement;
    },

    set_tbElement : function(value) {
        if (this.tbElement !== value) {
            this.tbElement = value;
            this.raisePropertyChanged('tbElement');
        }
    },
    
    get_nodeLink : function() {
        return this.nodeLink;
    },

    set_nodeLink : function(value) {
        if (this.nodeLink !== value) {
            this.nodeLink = value;
            this.raisePropertyChanged('nodeLink');
        }
    },
    
    set_TreeView: function(treeview) 
    {
        this._treeview = treeview;
        for(var i=0; i < this._Nodes.length; i++)
        {
            if(treeview != null) 
            {
                this._Nodes[i].set_TreeView(treeview);
                
            }
        }
    },
    
    get_Nodes: function () {
        return this._Nodes;
    },
    set_Nodes: function(value) {
        var nodesids = value.split(";");
        for(var idx=0; idx < nodesids.length; idx++) 
        {
            var childnode = $find(nodesids[idx]);
           
            if(childnode != null) {
                childnode.set_TreeView(this._treeview);
                childnode._parent = this;
                 
                this._Nodes[this._Nodes.length]= childnode;
            }
        }
       
        
        this.raisePropertyChanged('Nodes');
    },
    
    _onMouseOver: function(e) {
        var TElement = this.get_tbElement(); // $get("TableNode_" + this._nodeindex, this.get_element());
        if(this._treeview != null) {
            //TElement.setStyle(this._treeview._HoverNodeStyle);
            if (this._treeview._HoverNodeStyle !== null)
            {
                this._setStyle(TElement, this._treeview._HoverNodeStyle);
                this._setStyle(this.get_nodeLink(), this._treeview._HoverNodeStyle);
            }
            TElement.className = this._treeview._HoverNodeCss;
        }
    },
    _onMouseOut: function(e) {
        var TElement = this.get_tbElement(); //  $get("TableNode_" + this._nodeindex, this.get_element());
        if(this._treeview != null) {
             if(this._Selected) 
             {
                //TElement.setAttribute("style", this._treeview._SelectedNodeStyle);
                if (this._treeview._SelectedNodeStyle !== null)
                {
                    this._setStyle(TElement, this._treeview._SelectedNodeStyle);
                    this._setStyle(this.get_nodeLink(), this._treeview._SelectedNodeStyle);
                }
                TElement.className = this._treeview._SelectedNodeCss;
             }
             else {
                //TElement.setAttribute("style", this._treeview._NodeStyle);
                if (this._treeview._NodeStyle !== null)
                {
                    this._setStyle(TElement, this._NodeStyle);
                    this._setStyle(this.get_nodeLink(), this._NodeStyle);
                }
                TElement.className = this._NodeCss;
             }
        }
    },
    
    _setStyle: function(elm, sty) 
    {
        var st = sty.split(';');  
        for(var i=0; i<st.length; i++){
            var stl = st[i].split(':');
            if(stl.length == 2){
                  stl[0] = stl[0].replace(/^\s*|\s*$/g,"");      
                  stl[1] = stl[1].replace(/^\s*|\s*$/g,"");     
                  stl[1] = stl[1].replace(/'/g, "\"");      
                  tmpStl = stl[0].split('-');
                  stl[0] = tmpStl[0];      
                  for(var j=1; j < tmpStl.length; j++){
                          stl[0] += tmpStl[j].charAt(0).toUpperCase() + tmpStl[j].substr(1).toLowerCase();
                  }      
                  delete tmpStl;      
                  if(stl[0].length > 3){
                    eval('elm.style.' + stl[0] + ' = \'' + stl[1] + '\';');
                  }    
            }  
        }
    },
    
    _onSelect: function(e) {
    
    },
    _onClickExpandCollapseButton: function(e) {
        if (this.get_element() && !this.get_element().disabled) {
            var imgCEButtonElement = this.get_imgCEButtonElement();//$get("CollapseExpandImg_" + this._nodeindex, this.get_element());
            var divChildNodesElement = this.get_ChildNodesDiv(); //$get("ChildNodesDiv_" + this._nodeindex, this.get_element());
            if(divChildNodesElement.style.display == "none") {
                imgCEButtonElement.src = this._ExpandedButtonImageUrl;
                this._expanded.value = "true";
                divChildNodesElement.style.display = "block";
            }
            else {
                imgCEButtonElement.src =  this._CollapseButtonImageUrl;
                this._expanded.value = "false";
                divChildNodesElement.style.display = "none";
            }
        }
    },
    
    _onCheck: function(e) {
        if (this.get_element() && !this.get_element().disabled) {
            var imgElement = this.get_imgChkElement();//$get("ImageCheckBox_" + this._nodeindex, this.get_element());
            if(this._state.value == "Checked") {
                imgElement.src = this._UncheckedImageUrl;
                this._state.value ="Unchecked";
                this.raisePropertyChanged('State');
            }
            else {
                if(this._state.value == "Unchecked") {
                    imgElement.src = this._UndefineImageUrl;
                    this._state.value = "Undefine";
                    this.raisePropertyChanged('State');
                }
                else {
                    if(this._state.value == "Undefine") {
                        imgElement.src = this._CheckedImageUrl;
                        this._state.value = "Checked";
                        this.raisePropertyChanged('State');
                    }
                } 
            }      
        }
    },
    
    get_state : function() {
        return this._state;
    },

    set_state : function(value) {
        if (this._state !== value) {
            this._state = value;
            this.raisePropertyChanged('state');
        }
    },
    
    get_expanded : function() {
        return this._expanded;
    },

    set_expanded : function(value) {
        if (this._expanded !== value) {
            this._expanded = value;
            this.raisePropertyChanged('expanded');
        }
    },
    
    get_CheckedImageUrl: function () {
        return this._CheckedImageUrl;
    },
    set_CheckedImageUrl: function(value) {
        if (this._CheckedImageUrl !== value) {
            this._CheckedImageUrl = value;
            this.raisePropertyChanged('CheckedImageUrl');
        }
    },
    get_UncheckedImageUrl: function () {
        return this._UncheckedImageUrl;
    },
    set_UncheckedImageUrl: function(value) {
        if (this._UncheckedImageUrl !== value) {
            this._UncheckedImageUrl = value;
            this.raisePropertyChanged('UncheckedImageUrl');
        }
    },
    get_UndefineImageUrl: function () {
        return this._UndefineImageUrl;
    },
    set_UndefineImageUrl: function(value) {
        if (this._UndefineImageUrl !== value) {
            this._UndefineImageUrl = value;
            this.raisePropertyChanged('UndefineImageUrl');
        }
    },
    
    get_ImageUrl: function () {
        return this._ImageUrl;
    },
    set_ImageUrl: function(value) {
        if (this._ImageUrl !== value) {
            this._ImageUrl = value;
            this.raisePropertyChanged('ImageUrl');
        }
    },
    
    get_SelectedImageUrl: function () {
        return this._SelectedImageUrl;
    },
    set_SelectedImageUrl: function(value) {
        if (this._SelectedImageUrl !== value) {
            this._SelectedImageUrl = value;
            this.raisePropertyChanged('SelectedImageUrl');
        }
    },
    
    get_ExpandedImageUrl: function () {
        return this._ExpandedImageUrl;
    },
    set_ExpandedImageUrl: function(value) {
        if (this._ExpandedImageUrl !== value) {
            this._ExpandedImageUrl = value;
            this.raisePropertyChanged('ExpandedImageUrl');
        }
    },
   
    get_Target: function () {
        return this._Target;
    },
    set_Target: function(value) {
        if (this._Target !== value) {
            this._Target = value;
            this.raisePropertyChanged('Target');
        }
    },
    get_ShowCheckBox: function () {
        return this._ShowCheckBox;
    },
    set_ShowCheckBox: function(value) {
        if (this._ShowCheckBox !== value) {
            this._ShowCheckBox = value;
            this.raisePropertyChanged('ShowCheckBox');
        }
    },
    get_Selected: function () {
        return this._Selected;
    },
    set_Selected: function(value) {
        if (this._Selected !== value) {
            this._Selected = value;
            this.raisePropertyChanged('Selected');
        }
    },
    get_ExpandedButtonImageUrl: function () {
        return this._ExpandedButtonImageUrl;
    },
    set_ExpandedButtonImageUrl: function(value) {
        if (this._ExpandedButtonImageUrl !== value) {
            this._ExpandedButtonImageUrl = value;
            this.raisePropertyChanged('ExpandedButtonImageUrl');
        }
    },
    get_CollapseButtonImageUrl: function () {
        return this._ExpandedButtonImageUrl;
    },
    set_CollapseButtonImageUrl: function(value) {
        if (this._CollapseButtonImageUrl !== value) {
            this._CollapseButtonImageUrl = value;
            this.raisePropertyChanged('CollapseButtonImageUrl');
        }
    },
    get_Expandable: function () {
        return this._Expandable;
    },
    set_Expandable: function(value) {
        if (this._Expandable !== value) {
            this._Expandable = value;
            this.raisePropertyChanged('Expandable');
        }
    },
    
    get_nodeindex: function () {
        return this._nodeindex;
    },
    set_nodeindex: function(value) {
        if (this._nodeindex !== value) {
            this._nodeindex = value;
            this.raisePropertyChanged('nodeindex');
        }
    },
    
    dispose: function() {        
        //Add custom dispose actions here
        //var imgCEButtonElement = $get("CollapseExpandImg_" + this._nodeindex, this.get_element());
        //var imgChkElement = $get("CollapseExpandImg_" + this._nodeindex, this.get_element());
        //var hplElement = $get("Text_" + this._nodeindex, this.get_element());
        
        $clearHandlers(this.imgCEButtonElement);
        $clearHandlers(this.imgChkElement);
        $clearHandlers(this.hplElement);
        
        $clearHandlers(this.get_element());
        Axelerate.BusinessLayerUITools.WebControls.TreeNode.callBaseMethod(this, 'dispose');
    }
}

Axelerate.BusinessLayerUITools.WebControls.TreeNode.descriptor = {
    properties: [   {name: 'State', type: Number}],
    properties: [   {name: 'TreeView', type: Object}],
    properties: [   {name: 'Parent', type: Object}],
    properties: [   {name: 'Level', type: Number}],
    properties: [   {name: 'NodeIndex', type: String}]//,
    //events: [ {name: 'tick'} ]
    
/*  this._ImageUrl = null;
    this._SelectedImageUrl = null;
    this._ExpandedImageUrl = null;
    this._Target = null;
    this._Expandable = null;
    this._Text = null;
    this._NavigateUrl = null;
    this._Expanded = null;
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
Axelerate.BusinessLayerUITools.WebControls.TreeView.registerClass('Axelerate.BusinessLayerUITools.WebControls.TreeView', Sys.UI.Control);
Axelerate.BusinessLayerUITools.WebControls.TreeNode.registerClass('Axelerate.BusinessLayerUITools.WebControls.TreeNode', Sys.UI.Control);


if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
