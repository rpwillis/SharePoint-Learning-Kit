<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NavClosed.aspx.cs" Inherits="Microsoft.LearningComponents.Frameset.Frameset_NavClosed" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >

<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
     
<head runat="server">
    <LINK rel="stylesheet" type="text/css" href="Theme/Styles.css" />
    <SCRIPT src="./Include/FramesetMgr.js"></SCRIPT>
    <SCRIPT src="./Include/Nav.js"></SCRIPT>
</head>
<BODY  tabindex=1 onload="OnLoad( NAVCLOSED_FRAME );">
<DIV id=TOCFrameVisibleDiv>
	<TABLE height=12 cellSpacing=0 cellPadding=0 width="100%" border=0>
		<TBODY>
			<TR vAlign=top>
				<TD vAlign=top align=left width="100%"><IMG id=HeadShadow1 height=12 src="Theme/HeadShadow.gif" width="100%" border=0 tabIndex=1></TD>
				<TD vAlign=top align=right width=28><IMG id=HeadCornerRight1 height=21 src="Theme/HeadCornerRt.gif" width=28 border=0 tabIndex=1></TD>
			</TR>
		</TBODY>
	</TABLE>
</DIV>

<DIV id=TOCFrameHiddenDiv style="DISPLAY: none">
	<TABLE height=21 cellSpacing=0 cellPadding=0 width="100%" border=0>
		<TBODY>
			<TR vAlign=top>
				<TD vAlign=top align=left width=179><IMG id=TocClosedTab height=20 src="Theme/TocClosedTab.gif" width=179 border=0 ></TD>
				<TD vAlign=top align=left width="100%"><IMG id=HeadShadow2 height=12 src="Theme/HeadShadow.gif" width="100%" border=0 ></TD>
				<TD vAlign=top align=right width=28><IMG id=HeadCornerRight2 height=21 src="Theme/HeadCornerRt.gif" width=28 border=0 ></TD>
			</TR>
		</TBODY>
	</TABLE>
	
	<DIV class=NavClosedPreviousBtnGrphic id="divPrevious">
		<IMG id=imgPrevious title="<%=PreviousTitleHtml%>" height=15 src="Theme/Prev.gif" width=15 border=0 tabindex=1>
	</DIV>
	<DIV class=NavClosedNextBtnGrphic id="divNext">
		<IMG id=imgNext title="<%=NextTitleHtml%>" height=15 src="Theme/Next.gif" width=15 border=0 tabindex=1>
	</DIV>
	<DIV class=NavClosedSaveBtnGrphic id="divSave">
		<IMG id=imgSave title="<%=SaveTitleHtml%>" height=15 src="Theme/Save.gif" width=15 border=0 tabindex=1>
	</DIV>
	<DIV class=NavClosedShowTOCGrphic>
		<IMG id=imgOpenToc title="<%=MaximizeTitleHtml%>" height=14 src="Theme/TocOpen.gif" width=14 border=0 tabindex=1>
    </DIV>
</DIV>
</BODY>

<%
// <body>
//     <form id="form1" runat="server">
//     <div>
//     
//     </div>
//    </form>
// </body>
 %>
</html>
