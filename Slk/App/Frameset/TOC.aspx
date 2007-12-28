<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.TOC" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
<LINK rel="stylesheet" type="text/css" href="Theme/Styles.css" />
<SCRIPT src="./Include/FramesetMgr.js"></SCRIPT>
<SCRIPT src="./Include/Toc.js"></SCRIPT>
<SCRIPT src="./Include/vernum.js"></SCRIPT>

<SCRIPT language="jscript">
    g_currentActivityId = null;
    g_previousActivityId = null;
    g_frameMgr = API_GetFramesetManager();
    
    function body_onload()
    {
        // Tell frameMgr to call back when current activity changes
        g_frameMgr.ShowActivityId = SetCurrentElement;        
        g_frameMgr.ResetActivityId = ResetToPreviousSelection;
        
        // Tell frameMgr to call back with TOC active / inactive state changes
        g_frameMgr.SetTocNodes = SetTocNodes;
         
        // Register with framemanager that loading is complete
	    g_frameMgr.RegisterFrameLoad(TOC_FRAME); 
    }
</SCRIPT>
</head>
<body class=NavBody onclick="body_onclick();" onload="body_onload()">
<DIV id=divMain style="visibility:hidden;MARGIN: 5px">
	<DIV noWrap >
		<!-- <p class="NavClosedPreviousBtnGrphic">&nbsp;</p> -->
		<% WriteToc(); %>		
    </DIV>
</DIV>
<script type="text/javascript" defer="true">
        
  // If the version of the page differs from the version of the script, don't render
  if ("<%=TocVersion %>" != JsVersion())
  {
    document.writeln("<%=InvalidVersionHtml %>");
    document.writeln("Version: <%=TocVersion %>");
    document.writeln("JsVersion: " + JsVersion());
  }
        
</script>
</BODY>
</html>
