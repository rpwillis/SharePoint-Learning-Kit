<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NavOpen.aspx.cs" Inherits="Microsoft.LearningComponents.Frameset.Frameset_NavOpen" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >

<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
     
<head runat="server">
<LINK rel="stylesheet" type="text/css" href="Theme/Styles.css"/>
<SCRIPT src="./Include/FramesetMgr.js"></SCRIPT>
<SCRIPT src="./Include/Nav.js"></SCRIPT>

<script>
    
</script>
</head>
<BODY class=NavOpenBody tabIndex=1 onload="OnLoad( NAVOPEN_FRAME ); ">

<TABLE cellSpacing=0 cellPadding=0 width="100%" border=0 valign="middle" background="Theme/nav_bg.gif">
	<TBODY>
		<TR>
			<TD align=left>
				<TABLE cellSpacing=0 cellPadding=0 border=0>
					<TBODY>
						<TR>
							<TD width=7 height=20><IMG height=2 src="Theme/1px.gif" width=7></TD>
							<TD height=20><div id="divPrevious" style="visibility:hidden"><nobr><SPAN class=NavOpenNav>
							        <IMG tabIndex=1 id=imgPrevious title="<%=PreviousTitleHtml%>" src="Theme/Prev.gif"></SPAN></nobr></div></TD>
							<TD height=20><div id="divNext" style="visibility:hidden"><SPAN class=NavOpenNav>
							        <IMG tabIndex=1 id=imgNext title="<%=NextTitleHtml%>" src="Theme/Next.gif" border=0></SPAN></div></TD>
							<TD width=6 height=20><IMG height=2 src="Theme/1px.gif" width=6 border=0></TD>
							<TD height=20><div id="divSave" style="visibility:hidden"><nobr><SPAN class=NavOpenNav>
							        <IMG tabIndex=1 id=imgSave title="<%=SaveTitleHtml%>" src="Theme/Save.gif"></SPAN></nobr></div></TD>
						</TR>
					</TBODY>
				</TABLE>
			</TD>
			<TD align=right>
				<TABLE cellSpacing=0 cellPadding=0 border=0>
					<TBODY>
						<TR>
							<TD width=53 height=20><IMG height=2 src="Theme/1px.gif" width=53 border=0 </TD>
							<TD height=20><NOBR><SPAN class=NavOpenCloseToc><IMG id=imgCloseToc title="<%=MinimizeTitleHtml%>" src="Theme/TocClose.gif" border=0 tabIndex=1></SPAN></NOBR></TD>
							<TD width=7 height=20><IMG height=2 src="Theme/1px.gif" width=7 border=0></TD>
						</TR>
					</TBODY>
				</TABLE>
			</TD>
		</TR>
	</TBODY>
</TABLE>
</BODY>
</html>
