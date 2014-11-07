<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.TOC" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml"> 
<head>
<link rel="stylesheet" type="text/css" href="Theme/Styles.css" />
<script type="text/javascript" src="./Include/FramesetMgr.js"></script>
<script type="text/javascript" src="./Include/Toc.js"></script>
<script type="text/javascript" src="./Include/vernum.js"></script>

<script language="javascript" type="text/javascript">
    g_currentActivityId = null;
    g_previousActivityId = null;
    g_frameMgr = API_GetFramesetManager();
    
    function body_onunload()
    {
        if (g_frameMgr) {
            // With IE9 and above, unless in compatibility mode, then the functions cannot be called when unloaded, so set to null
            g_frameMgr.ShowActivityId = null;        
            g_frameMgr.ResetActivityId = null;
            g_frameMgr.SetTocNodes = null;
        }
    }

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

</script>
</head>
<body class=NavBody onclick="body_onclick(event);" onload="body_onload();" onunload="body_onunload();">
<div id="divMain" style="visibility:hidden;MARGIN: 5px">
	<div  noWrap>
		<!-- <p class="NavClosedPreviousBtnGrphic">&nbsp;</p> -->
		<% WriteToc(); %>		
    </div>
</div>
<script type="text/javascript" defer="true">
        
  // If the version of the page differs from the version of the script, don't render
  if ("<%=TocVersion %>" != JsVersion())
  {
    document.writeln("<%=InvalidVersionHtml %>");
    document.writeln("Version: <%=TocVersion %>");
    document.writeln("JsVersion: " + JsVersion());
  }
        
</script>
</body>
</html>
