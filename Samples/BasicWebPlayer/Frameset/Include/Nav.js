<!-- Copyright (c) Microsoft Corporation. All rights reserved. -->

// path for images
var VIEWER_ART_PATH = "Theme/";

// navbar images
var IMG_PREVIOUS_BUTTON_NORMAL = VIEWER_ART_PATH + "Prev.gif";
var IMG_PREVIOUS_BUTTON_HOVER = VIEWER_ART_PATH + "PrevHover.gif";
var IMG_PREVIOUS_BUTTON_PRESSED = VIEWER_ART_PATH + "PrevHover.gif";
var IMG_NEXT_BUTTON_NORMAL = VIEWER_ART_PATH + "Next.gif";
var IMG_NEXT_BUTTON_HOVER = VIEWER_ART_PATH + "NextHover.gif";
var IMG_NEXT_BUTTON_PRESSED = VIEWER_ART_PATH + "NextHover.gif";
var IMG_SAVE_BUTTON_NORMAL = VIEWER_ART_PATH + "Save.gif";
var IMG_SAVE_BUTTON_HOVER = VIEWER_ART_PATH + "SaveHover.gif";
var IMG_SAVE_BUTTON_PRESSED = VIEWER_ART_PATH + "SaveHover.gif";
var IMG_TOC_OPEN_BUTTON_NORMAL = VIEWER_ART_PATH + "TocOpen.gif";
var IMG_TOC_OPEN_BUTTON_HOVER = VIEWER_ART_PATH + "TocOpenHover.gif";
var IMG_TOC_OPEN_BUTTON_PRESSED = VIEWER_ART_PATH + "TocOpenHover.gif";
var IMG_TOC_MINIMIZE_BUTTON_NORMAL = VIEWER_ART_PATH + "TocClose.gif";
var IMG_TOC_MINIMIZE_BUTTON_HOVER = VIEWER_ART_PATH + "TocCloseHover.gif";
var IMG_TOC_MINIMIZE_BUTTON_PRESSED = VIEWER_ART_PATH + "TocCloseHover.gif";

// other images
var IMG_ONE_PIXEL = VIEWER_ART_PATH + "1px.gif";

var frameMgr;

document.onkeypress = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event
   
	switch (target.id)
	{
		case "imgPrevious":
		case "imgNext":
		case "imgSave":
		case "imgCloseToc":
		case "imgOpenToc":
		{
			if ((e.keyCode == 13) || (e.keyCode == 32)){
				document.onclick();
			break;
			}
			else if ((e.which == 13) || (e.which == 32)){
				document.onclick();
			break;
			}
		}
		default:
			return;
	}
							
	e.cancelBubble = true;
    e.returnValue = false;
 }

document.onclick = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event
   
	switch (target.id)
	{
		case "imgPrevious":

			frameMgr.DoPrevious();
			break;
				
		case "imgNext":

			frameMgr.DoNext();
			break;

		case "imgSave":

			frameMgr.Save();
			break;

		case "imgCloseToc":

			CloseTOC();
			break;

		case "imgOpenToc":

			OpenTOC();
			break;
			
		default:
			return;
	}
							
	e.cancelBubble = true;
    e.returnValue = false;
}

function CloseTOC()
{
 	// save the current frameset width
	parent.strFrameCols = parent.document.getElementById("framesetParentUI").cols;
 
 	// collapse the frameset
	parent.document.getElementById("framesetParentUI").cols = "0px,*";
 
 	// increase the height of our frameset to accommodate larger graphics
	parent.document.getElementById("framesetParentContent").rows = "21px,*";
 				
 	// swap NavClosed.htm images so we can allow the end-user to restore the TOC frameset
	parent.document.getElementById("frameNavClosed").contentWindow.document.getElementById("TOCFrameVisibleDiv").style.display = "none";
	parent.document.getElementById("frameNavClosed").contentWindow.document.getElementById("TOCFrameHiddenDiv").style.display = "inline";
}

function OpenTOC()
{
   	// restore frameset to its original width
	parent.document.getElementById("framesetParentUI").cols = window.parent.strFrameCols;
 
 	// Reset frameset height to its original size
	parent.document.getElementById("framesetParentContent").rows = "12px,*";
 				
 	// swap NavClosed.htm images, restoring them to their defaults
	parent.document.getElementById("frameNavClosed").contentWindow.document.getElementById("TOCFrameVisibleDiv").style.display = "inline";
	parent.document.getElementById("frameNavClosed").contentWindow.document.getElementById("TOCFrameHiddenDiv").style.display = "none";
}

document.onmouseover = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event

	switch (target.id)
	{
		case "imgPrevious":
			document.getElementById("imgPrevious").src = IMG_PREVIOUS_BUTTON_HOVER;
			break;

		case "imgNext":
			document.getElementById("imgNext").src = IMG_NEXT_BUTTON_HOVER;
			break;

		case "imgSave":
			document.getElementById("imgSave").src = IMG_SAVE_BUTTON_HOVER;
			break;

		case "imgCloseToc":
			document.getElementById("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_HOVER;
			break;

		case "imgOpenToc":
			document.getElementById("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_HOVER;
			break;

		default:
			return;
	}

	e.cancelBubble = true;
    e.returnValue = false;
}

document.onmouseout = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event
   
	switch (target.id)
	{
		case "imgPrevious":
			document.getElementById("imgPrevious").src = IMG_PREVIOUS_BUTTON_NORMAL;
			break;

		case "imgNext":
			document.getElementById("imgNext").src = IMG_NEXT_BUTTON_NORMAL;
			break;

		case "imgSave":
			document.getElementById("imgSave").src = IMG_SAVE_BUTTON_NORMAL;
			break;

		case "imgCloseToc":
			document.getElementById("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_NORMAL;
			break;

		case "imgOpenToc":
			document.getElementById("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_NORMAL;
			break;

		default:
			return;
	}

	e.cancelBubble = true;
    e.returnValue = false;
}

document.onmousedown = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event
   
	switch (target.id)
	{
		case "imgPrevious":
			document.getElementById("imgPrevious").src = IMG_PREVIOUS_BUTTON_PRESSED;
			break;

		case "imgNext":
			document.getElementById("imgNext").src = IMG_NEXT_BUTTON_PRESSED;
			break;

		case "imgSave":
			document.getElementById("imgSave").src = IMG_SAVE_BUTTON_PRESSED;
			break;

		case "imgCloseToc":
			document.getElementById("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_PRESSED;
			break;

		case "imgOpenToc":
			document.getElementById("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_PRESSED;
			break;

		default:
			return;
	}

	e.cancelBubble = true;
    e.returnValue = false;
}

document.onmouseup = function(e)
{
   var e = e || window.event;
   //notice that when assigning variables you can use the OR to differentiate between the methods each broswer accepts

   var target = e.target || e.srcElement;
   //now we have established the target which fired the event
   
	switch (target.id)
	{
		case "imgPrevious":
			document.getElementById("imgPrevious").src = IMG_PREVIOUS_BUTTON_NORMAL;
			break;

		case "imgNext":
			document.getElementById("imgNext").src = IMG_NEXT_BUTTON_NORMAL;
			break;

		case "imgSave":
			document.getElementById("imgSave").src = IMG_SAVE_BUTTON_NORMAL;
			break;

		case "imgCloseToc":
			document.getElementById("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_NORMAL;
			break;

		case "imgOpenToc":
			document.getElementById("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_NORMAL;
			break;

		default:
			return;
	}

	e.cancelBubble = true;
    e.returnValue = false;
}

function OnLoad( frameName )
{
    frameMgr = API_GetFramesetManager();
    // Register with framemanager that loading is complete
    frameMgr.RegisterFrameLoad(frameName);
}
