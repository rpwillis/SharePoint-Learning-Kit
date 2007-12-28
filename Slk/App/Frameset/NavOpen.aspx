<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.NavOpen" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
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
							        <IMG id=imgPrevious title="<%=PreviousTitleHtml%>" tabIndex=3 src="Theme/Prev.gif"></SPAN></nobr></div></TD>
							<TD height=20><div id="divNext" style="visibility:hidden"><SPAN class=NavOpenNav>
							        <IMG id=imgNext title="<%=NextTitleHtml%>"  tabIndex=2 src="Theme/Next.gif" border=0></SPAN></div></TD>
							<TD width=6 height=20><IMG height=2 src="Theme/1px.gif" width=6 border=0></TD>
							<TD height=20><div id="divSave" style="visibility:hidden"><nobr><SPAN class=NavOpenNav>
							        <IMG id=imgSave title="<%=SaveTitleHtml%>" tabIndex=4 src="Theme/Save.gif"></SPAN></nobr></div></TD>
						</TR>
					</TBODY>
				</TABLE>
			</TD>
			<TD align=right>
				<TABLE cellSpacing=0 cellPadding=0 border=0>
					<TBODY>
						<TR>
							<TD width=53 height=20><IMG height=2 src="Theme/1px.gif" width=53 border=0></TD>
							<TD height=20><NOBR><SPAN class=NavOpenCloseToc><IMG id=imgCloseToc title="<%=MinimizeTitleHtml%>" tabIndex=5 src="Theme/TocClose.gif" border=0></SPAN></NOBR></TD>
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
