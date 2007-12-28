﻿<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.MainFrames" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <LINK rel="stylesheet" type="text/css" href="Theme/Styles.css" />
</head>
<FRAMESET id=framesetParentMain border=0 frameSpacing=0 rows=*,14,1 frameBorder=0>
	<FRAMESET id=framesetParentUI cols=180,*>
		<FRAMESET id=framesetToc rows=22,*>
			<FRAME class=NavOpenFrame id=frameNavOpen name=frameNavOpen 
			       marginWidth=0 frameSpacing=0 marginHeight=0 src="NavOpen.aspx" frameBorder=0 scrolling=no>
			                
			<FRAME id=frameToc name=frameToc marginWidth=0 frameSpacing=0 marginHeight=0 src="<%=TocFrameUrl %>" frameBorder=0>
		</FRAMESET>

		<FRAMESET class=ContentFrameLeftBorder id=framesetParentContent border=0 rows=12,*>
			<FRAME id=frameNavClosed tabIndex=-1 name=frameNavClosed marginWidth=0 frameSpacing=0 marginHeight=0 src="NavClosed.aspx" frameBorder=0 scrolling=no>

			<FRAMESET id=framesetContent border=0 cols=*,16>
			
				<FRAME id=frameContent name=frameContent src="Content.aspx" frameBorder=0>
				<FRAME id=frameScrollbarReplacement tabIndex=-1 name=frameScrollbarReplacement marginWidth=0 frameSpacing=0 marginHeight=0 src="NoScroll.htm" frameBorder=0 scrolling=no>
				
			</FRAMESET>

		</FRAMESET>

	</FRAMESET>

	<FRAME class=BottomFrame id=frameBottom tabIndex=-1 name=frameBottom 
	            marginWidth=0 frameSpacing=0 marginHeight=0 src="Bottom.htm" frameBorder=0 noResize scrolling=no>
	
	<FRAME class=HiddenFrame id=frameHidden tabIndex=-1 name=frameHidden visible=false
	            marginWidth=0 frameSpacing=0 marginHeight=0 src="<%=HiddenFrameUrl %>" frameBorder=0 noResize scrolling=no>

</FRAMESET>
</html>
