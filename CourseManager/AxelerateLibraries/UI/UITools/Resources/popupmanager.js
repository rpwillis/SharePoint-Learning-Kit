
var hiddenFieldID = "";
var hiddenFieldBT = "";

function SetPopupManagerHDFS(hndFieldID, hndButtonID) 
{
    hiddenFieldID = hndFieldID;
    hiddenFieldBT = hndButtonID;
}

function ShowHTMLPopup(layoutname) 
{
    var HiddenDiv = document.getElementById("popupdiv");
    var hdField = document.getElementById(hiddenFieldID);
    if(HiddenDiv != null) 
    {
        hdField.value = layoutname;
         __doPostBack(hiddenFieldBT,'');
        HiddenDiv.style.display="block";
    }
    else 
    {
        alert("The 'popupdiv' was not found!!");
    }
    return false;
}

function HideHTMLPopup() 
{
    var HiddenDiv = document.getElementById("popupdiv");
    var hdField = document.getElementById(hiddenFieldID);
    
    if(HiddenDiv != null) 
    {
        hdField.value = "Empty";
        __doPostBack(hiddenFieldBT,'');
        HiddenDiv.style.display="none";
    }
    else 
    {
        alert("The 'popupdiv' was not found!!");
    }
}