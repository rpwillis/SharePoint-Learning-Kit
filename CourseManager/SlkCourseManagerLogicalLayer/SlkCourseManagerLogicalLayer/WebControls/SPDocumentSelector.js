var _SPDocumentSelectorID = "";
var _SPPopupDivID;
var _RealDispEx = null;
var _tmphref = "";
var _ele = null;

function SPDocumentSelector_Init(SPDocumentSelectorID)
{
   
    var SPDocumentSelector = document.getElementById(SPDocumentSelectorID);
    if (SPDocumentSelector !== null)
    {
        SPDocumentSelector.baseSetAttribute = SPDocumentSelector.setAttribute;
        SPDocumentSelector.setAttribute = SPDocumentSelector_setAttribute;
    }
}

function SPDocumentSelector_setAttribute(name, value)
{
    this.baseSetAttribute(name, value);
    if(name.toUpperCase() == "VALUE")
    {
        var lbl = document.getElementById(this.getAttribute("dvcontainerid"));
        if (lbl != null)
        {
            lbl.innerHTML = unescape(value.toString().substring(value.toString().lastIndexOf('/')+1));
        }
    }
    
}

function SPDocumentSelector_Show(SPDocumentSelectorID, SPPopupDivID)
{
   _SPDocumentSelectorID = SPDocumentSelectorID;
   _SPPopupDivID = SPPopupDivID;
   var SPPopupDiv = document.getElementById(SPPopupDivID);
   var SPPopupDiv_frame = document.getElementById(SPPopupDivID + "_frame");
   SPPopupDiv.style.display = "";
   SPPopupDiv_frame.style.display = "";
   _RealDispEx = DispEx;
   DispEx = spdoc_DispEx;
   var frm = document.getElementById(SPPopupDivID + "_iframe");
   frm.contentWindow.SPDocumentSelector_SetDispEX(); 
   return false;
}

function spdoc_DispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension,
	fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect,
	strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask)
{
    var _ele = ele;
    var _tmphref = ele.href;
    var currenItemType =  GetAttributeFromItemTable(itemTable, "OType", "FSObjType");
   
    if (currenItemType !== 1) 
    {
        //ele.href = "";
        SPDocumentSelector_Select(_SPDocumentSelectorID, _tmphref, ele.innerHTML);
        objEvent.cancelBubble=true;
        objEvent.returnValue=false;
    		
        return false;
    }
    else {
        return _RealDispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension,
	    fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect,
	    strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask);
    }
}

function SPDocumentSelector_Hide()
{
   var SPPopupDiv = document.getElementById(_SPPopupDivID);
   var SPPopupDiv_frame = document.getElementById(_SPPopupDivID + "_frame");
   if (SPPopupDiv !== null)
   {
        SPPopupDiv.style.display = "none";
        SPPopupDiv_frame.style.display = "none";
   }
   _SPDocumentSelectorID = "";
   _SPPopupDivID = "";
   DispEx = _RealDispEx;
   //_ele.href = _tmphref;
}

function Custom_AddDocLibMenuItems(m, ctx)
{
   var currentItemURL = itemTable.ServerUrl;
   var currenItemType = itemTable.FSObjType;
   
   if (currenItemType !== 1) 
   {
       var displayText = "Select Document";
       var strAction = "SPDocumentSelector_Select('" + _SPDocumentSelectorID + "','" + currentItemURL+ + "', '" + currenItemType + "')";
       CAMOpt(m, displayText, strAction, "");
   }
   return true;
}

function SPDocumentSelector_Select(SPDocumentSelectorID, url, docType)
{

    var SPDocSelector = document.getElementById(SPDocumentSelectorID);
    if (SPDocSelector !== null)
    {
        SPDocSelector.setAttribute("value", url);
        SPDocSelector.setAttribute("displayvalue", "<table border='0' cellpadding='0' cellspacing='0' width='100%'><tr><td style='width:auto' ><span title='" + url + "'>'&nbsp;" + unescape(url.toString().substring(url.toString().lastIndexOf('/')+1)) + "'</span></td><td style='width:16px' ><input type='button' value='...' title='Select Document' disabled='disabled' /></td></tr></table>");
        var lbl = document.getElementById(SPDocSelector.getAttribute("dvcontainerid"));
        if (lbl != null)
        {
            lbl.innerHTML = "&nbsp;" + unescape(url.toString().substring(url.toString().lastIndexOf('/')+1));
        }
        setTimeout("SPDocumentSelector_Hide()", 1);
    }
}