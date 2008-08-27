/// <reference name="MicrosoftAjax.js"/>


Type.registerNamespace("Axelerate.BusinessLayerUITools.WebControls");

Axelerate.BusinessLayerUITools.WebControls.Pager = function(element) {
    Axelerate.BusinessLayerUITools.WebControls.Pager.initializeBase(this, [element]);
    this._pagenumber = null;
    this._pagecount = null;
    this._postbackevent = null;
    this._sliderstepsize = null;
    this._clicked = false;
    this._slideroffsetleft = 0;
    this._startx = null;
}

Axelerate.BusinessLayerUITools.WebControls.Pager.prototype = {
    initialize: function() {
        Axelerate.BusinessLayerUITools.WebControls.Pager.callBaseMethod(this, 'initialize');
    
        //SliderButton
        var imgSliderButton = $get(this.get_element().id + "_SliderButton", this.get_element());
        //SliderBar
        var tclSliderBar = $get(this.get_element().id + "_SliderBar", this.get_element());
         
        imgSliderButton.Title = (this._pagenumber + 1).toString();
        
        this._sliderstepsize = tclSliderBar.offsetWidth / (this._pagecount - 1);
        imgSliderButton.style.left = (this._sliderstepsize * this._pagenumber) + "px";
        this._slideroffsetleft = imgSliderButton.offsetLeft;
        
        //onmousedown onmousemove onmouseup
        this._onmousedownHandler = Function.createDelegate(this, this._onMouseDown);
        this._onmousemoveHandler = Function.createDelegate(this, this._onMouseMove);
        this._onmouseupHandler = Function.createDelegate(this, this._onMouseUp);
        this._clicked = false;
        
         $addHandlers(imgSliderButton, 
                 { 'mousedown' : this._onMouseDown},
                 this);
         $addHandlers(tclSliderBar, 
                 { 'mousemove' : this._onMouseMove},
                 this);
    
         $addHandlers(document, 
                 { 'mouseup' : this._onMouseUp},
                 this);
    },
    
    _onMouseDown: function(e) {
        if(e.button == 0) {
            this._clicked = true;
            this._startx = e.offsetX;
        }
    },
    _onMouseMove: function(e) {
       if(this._clicked) 
       {
           //SliderButton
           var imgSliderButton = $get(this.get_element().id +"_SliderButton", this.get_element());
           //SliderBar
           var tclSliderBar = $get(this.get_element().id +"_SliderBar", this.get_element());
           if(e.target != imgSliderButton) 
           {
                imgSliderButton.style.left = (e.offsetX - this._startx) + "px";
           }
           else {
                var x = parseInt(imgSliderButton.style.left,10);
                imgSliderButton.style.left = (x + (e.offsetX - this._startx)) + "px";
           }
           if (parseInt(imgSliderButton.style.left,10) < 0) 
           {
                imgSliderButton.style.left = "0px";
           }
           if (parseInt(imgSliderButton.style.left,10) > tclSliderBar.offsetWidth ) 
           {
                imgSliderButton.style.left = tclSliderBar.offsetWidth + "px";
           }
           
       }
    },
    _onMouseUp: function(e) {
       if(this._clicked) 
       {
           //SliderButton
           var imgSliderButton = $get(this.get_element().id +"_SliderButton", this.get_element());
           //SliderBar
           var tclSliderBar = $get(this.get_element().id +"_SliderBar", this.get_element());
           
           var actualX = parseInt(imgSliderButton.style.left,10);
           var newpage = actualX / this._sliderstepsize;
           
           if(newpage > (this._pagecount - 1))
           {
                newpage = this._pagecount - 1;
           }
           
           imgSliderButton.style.left = (this._sliderstepsize * parseInt(newpage, 0)) + "px";
           //alert(parseInt(newpage, 0));
           eval(this._postbackevent.replace(/[[]SelectedPage[]]/i,  parseInt(newpage, 0)));  
       }
        this._clicked = false;
    },
    
    get_pagenumber: function () {
        return this._pagenumber;
    },
    set_pagenumber: function(value) {
        if (this._pagenumber !== value) {
            this._pagenumber = value;
            this.raisePropertyChanged('pagenumber');
        }
    },
    get_pagecount: function () {
        return this._pagecount;
    },
    set_pagecount: function(value) {
        if (this._pagecount !== value) {
            this._pagecount = value;
            this.raisePropertyChanged('pagecount');
        }
    },
    get_postbackevent: function () {
        return this._postbackevent;
    },
    set_postbackevent: function(value) {
        if (this._postbackevent !== value) {
            this._postbackevent = value;
            this.raisePropertyChanged('postbackevent');
        }
    },
    
     dispose: function() {        
        var imgSliderButton = $get(this.get_element().id +"_SliderButton", this.get_element());
        $clearHandlers(imgSliderButton);
        
        //SliderBar
        var tclSliderBar = $get(this.get_element().id +"_SliderBar", this.get_element());
        $clearHandlers(tclSliderBar);
           
        $clearHandlers(this.get_element());
        $clearHandlers(document);
        Axelerate.BusinessLayerUITools.WebControls.Pager.callBaseMethod(this, 'dispose');
    }
        
}

Axelerate.BusinessLayerUITools.WebControls.Pager.descriptor = {
    properties: [   {name: 'Text', type: Number}]  //,
    //events: [ {name: 'tick'} ]
}

Axelerate.BusinessLayerUITools.WebControls.Pager.registerClass('Axelerate.BusinessLayerUITools.WebControls.Pager', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();