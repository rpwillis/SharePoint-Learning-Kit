function SPDocumentSelector_SetDispEX ()
{
    if (parent !== null)
    {
    	parent._RealDispEx = DispEx;
        DispEx = parent.spdoc_DispEx;
    }
}

function Custom_AddDocLibMenuItems(m, ctx)
{
   var currentItemURL = GetAttributeFromItemTable(itemTable, "Url", "ServerUrl"); //itemTable.ServerUrl;
   var currenItemType =  GetAttributeFromItemTable(itemTable, "OType", "FSObjType");
   
   if (currenItemType !== "1") 
   {
       var displayText = "Select Document";
       var strAction = "";
       if (parent !== null)
       {
            strAction = "parent.SPDocumentSelector_Select('" + parent._SPDocumentSelectorID + "','" + currentItemURL+ + "', '" + currentItemURL + "')";
       }
       CAMOpt(m, displayText, strAction, "");
   }
   return true;
}

function GoBack()
{
   parent.window.history.go(-1);
   //history.back();
   return false;
}

setTimeout("SPDocumentSelector_SetDispEX()", 2);