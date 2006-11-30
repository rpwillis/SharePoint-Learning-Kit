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

function document.onkeypress()
{
	switch (event.srcElement.id)
	{
		case "imgPrevious":
		case "imgNext":
		case "imgSave":
		case "imgCloseToc":
		case "imgOpenToc":
		{
			if ((event.keyCode == 13) || (event.keyCode == 32))
				document.onclick();
			break;
		}
		default:
			return;
	}
							
	event.cancelBubble = true;
	event.returnValue = false;
}

function document.onclick()
{
	switch (event.srcElement.id)
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
							
	event.cancelBubble = true;
	event.returnValue = false;
}

function CloseTOC()
{
	// save the current frameset width
	parent.strFrameCols = parent.frames.framesetParentUI.cols;

	// collapse the frameset
	parent.frames.framesetParentUI.cols = "0px,*";

	// increase the height of our frameset to accommodate larger graphics
	parent.frames.framesetParentContent.rows = "21px,*";
				
	// swap NavClosed.htm images so we can allow the end-user to restore the TOC frameset
	parent.frames.frameNavClosed.document.all("TOCFrameVisibleDiv").style.display = "none";
	parent.frames.frameNavClosed.document.all("TOCFrameHiddenDiv").style.display = "inline";
}

function OpenTOC()
{
	// restore frameset to its original width
	parent.framesetParentUI.cols = window.parent.strFrameCols;

	// Reset frameset height to its original size
	parent.framesetParentContent.rows = "12px,*";
				
	// swap NavClosed.htm images, restoring them to their defaults
	parent.frameNavClosed.document.all("TOCFrameVisibleDiv").style.display = "inline";
	parent.frameNavClosed.document.all("TOCFrameHiddenDiv").style.display = "none";
}

function document.onmouseover()
{
	switch (event.srcElement.id)
	{
		case "imgPrevious":
			document.all("imgPrevious").src = IMG_PREVIOUS_BUTTON_HOVER;
			break;

		case "imgNext":
			document.all("imgNext").src = IMG_NEXT_BUTTON_HOVER;
			break;

		case "imgSave":
			document.all("imgSave").src = IMG_SAVE_BUTTON_HOVER;
			break;

		case "imgCloseToc":
			document.all("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_HOVER;
			break;

		case "imgOpenToc":
			document.all("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_HOVER;
			break;

		default:
			return;
	}

	event.cancelBubble = true;
	event.returnValue = false;
}

function document.onmouseout()
{
	switch (event.srcElement.id)
	{
		case "imgPrevious":
			document.all("imgPrevious").src = IMG_PREVIOUS_BUTTON_NORMAL;
			break;

		case "imgNext":
			document.all("imgNext").src = IMG_NEXT_BUTTON_NORMAL;
			break;

		case "imgSave":
			document.all("imgSave").src = IMG_SAVE_BUTTON_NORMAL;
			break;

		case "imgCloseToc":
			document.all("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_NORMAL;
			break;

		case "imgOpenToc":
			document.all("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_NORMAL;
			break;

		default:
			return;
	}

	event.cancelBubble = true;
	event.returnValue = false;
}

function document.onmousedown()
{
	switch (event.srcElement.id)
	{
		case "imgPrevious":
			document.all("imgPrevious").src = IMG_PREVIOUS_BUTTON_PRESSED;
			break;

		case "imgNext":
			document.all("imgNext").src = IMG_NEXT_BUTTON_PRESSED;
			break;

		case "imgSave":
			document.all("imgSave").src = IMG_SAVE_BUTTON_PRESSED;
			break;

		case "imgCloseToc":
			document.all("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_PRESSED;
			break;

		case "imgOpenToc":
			document.all("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_PRESSED;
			break;

		default:
			return;
	}

	event.cancelBubble = true;
	event.returnValue = false;
}

function document.onmouseup()
{
	switch (event.srcElement.id)
	{
		case "imgPrevious":
			document.all("imgPrevious").src = IMG_PREVIOUS_BUTTON_NORMAL;
			break;

		case "imgNext":
			document.all("imgNext").src = IMG_NEXT_BUTTON_NORMAL;
			break;

		case "imgSave":
			document.all("imgSave").src = IMG_SAVE_BUTTON_NORMAL;
			break;

		case "imgCloseToc":
			document.all("imgCloseToc").src = IMG_TOC_MINIMIZE_BUTTON_NORMAL;
			break;

		case "imgOpenToc":
			document.all("imgOpenToc").src = IMG_TOC_OPEN_BUTTON_NORMAL;
			break;

		default:
			return;
	}

	event.cancelBubble = true;
	event.returnValue = false;
}

function OnLoad( frameName )
{
    frameMgr = API_GetFramesetManager()
    // Register with framemanager that loading is complete
    frameMgr.RegisterFrameLoad(frameName); 
}