<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=abc4ed181d6d6a94" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.ChangeActivity" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
<LINK rel="stylesheet" type="text/css" href="<%=SlkEmbeddedUIPath.OriginalString %>/Theme/Styles.css"/>
<script src="<%=SlkEmbeddedUIPath.OriginalString %>/Include/FramesetMgr.js"></script>
	
	<script language="jscript">
	
	function onLoad()
	{ 
	    var frameMgr = API_GetFramesetManager();
	    
	    <% if (HasError) { %>
	    
            frameMgr.SetPostFrame(HIDDEN_FRAME);
            frameMgr.SetPostableForm(window.top.frames[MAIN_FRAME].document.all[HIDDEN_FRAME].contentWindow.document.forms[0]);
	        frameMgr.ShowErrorMessage("<%=ErrorTitleHtml %>", "<%=ErrorMsgHtml %>");
	        
	    <% }
	    else
	    {  %>
	        setTimeout(PleaseWait, 500);
	        <% WriteFrameMgrInit(); %>    
	   <% } %>

        return false;
	}
	
	 function PleaseWait()
    {
        try
        {
            // clears content from the window and displays a "Please wait" message
            document.body.innerHTML = "<table width='100%' class='ErrorTitle'><tr><td align='center'><%=PleaseWaitHtml %></td></tr></table>";
        }
        catch(e)
        {
            // only happens in odd boundary cases. Retry the message after another timeout.
            setTimeout(PleaseWait, 500);
        }
    }
	
	</script>
</head>
<body class="ErrorBody" onload="onLoad();">
    &nbsp;
</body>
</html>
