<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=abc4ed181d6d6a94" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.Content" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
<% /*  NOTE: The following HTML is *ONLY* ever rendered if there was an error in displaying the content page, and 
     the content frame is currently the postable frame. In most normal processing, the entire page is replaced with 
     the content rendered from the package. */ %>

<LINK rel="stylesheet" type="text/css" href="<%=SlkEmbeddedUIPath.OriginalString %>/Theme/Styles.css"/>
<script src="<%=SlkEmbeddedUIPath.OriginalString %>/Include/FramesetMgr.js"></script>
<script>
function OnLoad()
{
    // Get frameset manager
    frameMgr = API_GetFramesetManager();
    
    // Set data on frameset manager
    <%  WriteFrameMgrInit();  %>
    
    frameMgr.WaitForContentCompleted(0);    // since it's an error condition, don't wait for any more content frame loads
}

</script>
</head>
<body class="ErrorBody" onload="OnLoad();">
<form id="formId" runat="server">
<% if (HasError) { %>
<table border="0" width="100%" id="table1" style="border-collapse: collapse">
<tr>
<td width="60">
<p align="center">
<img border="0" src="<%=SlkEmbeddedUIPath.OriginalString %>/Theme/<%=ErrorIcon%>" width="49" height="49"></td>
<td class="ErrorTitle">
<%=ErrorTitleHtml %>                
</td></tr>
<tr>
<td width="61">&nbsp;</td>
<td width="100%"><hr></td>
</tr>
<tr>
<td width="61">&nbsp;</td>
<td class="ErrorMessage">
<%=ErrorMessageHtml %>
</td></tr>
</table>
<% } %>
</form>
</body>
</html>

