/// <reference name="MicrosoftAjax.js"/>


Type.registerNamespace("Axelerate.BusinessLayerUITools.WebControls");

Axelerate.BusinessLayerUITools.WebControls.Checkbox = function(element) {
    Axelerate.BusinessLayerUITools.WebControls.Checkbox.initializeBase(this, [element]);
    this._state = null;
    this._CheckedImageUrl = null;
    this._UncheckedImageUrl = null;
    this._UndefineImageUrl = null;
}

Axelerate.BusinessLayerUITools.WebControls.Checkbox.prototype = {
    initialize: function() {
        Axelerate.BusinessLayerUITools.WebControls.Checkbox.callBaseMethod(this, 'initialize');
        
        // Add custom initialization here
        this._onclickHandler = Function.createDelegate(this, this._onClick);
        var imgElement = $get("_imgCheck", this.get_element());
        
        $addHandlers(this.get_element(), 
                 { 'click' : this._onClick},
                 this);
       
        if(this._state.value == "Checked") {
            imgElement.src = this._CheckedImageUrl;
        }
        else {
                if(this._state.value == "Unchecked") {
                    imgElement.src = this._UncheckedImageUrl;
                }
                else {
                    if(this._state.value == "Undefine") {
                        imgElement.src = this._UndefineImageUrl;
                    }
               }
        }
       // imgElement.src = "Images/treeimages/T.gif";

    },
    _onClick: function(e) {
        if (this.get_element() && !this.get_element().disabled) {
            var imgElement = $get("_imgCheck", this.get_element());
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
            this.raisePropertyChanged('State');
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
    
    dispose: function() {        
        //Add custom dispose actions here
         $clearHandlers(this.get_element());
        Axelerate.BusinessLayerUITools.WebControls.Checkbox.callBaseMethod(this, 'dispose');
    }
}

Axelerate.BusinessLayerUITools.WebControls.Checkbox.descriptor = {
    properties: [   {name: 'State', type: Number}]  //,
    //events: [ {name: 'tick'} ]
}

Axelerate.BusinessLayerUITools.WebControls.Checkbox.registerClass('Axelerate.BusinessLayerUITools.WebControls.Checkbox', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();