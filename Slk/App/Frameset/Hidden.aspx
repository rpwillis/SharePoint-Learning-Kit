<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.Hidden" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <script type="text/javascript" src="./Include/FramesetMgr.js"></script>
	
<script language="javascript">
function FindSlkFrmMgr(win)
{
   var frmDepthCount = 0;
   while ((win.g_slkFrameMgr == null) && (win.parent != null) && (win.parent != win))
   {
      frmDepthCount++;
      
      if (frmDepthCount > 20) 
      {
         return null;
      }
      
      win = win.parent;
   }
   return win.g_slkFrameMgr;
}

// Recurse up the frame hierarchy to find the FramesetManager object.
function Slk_GetSlkManager()
{
    return FindSlkFrmMgr(window);
}

function OnLoad()
{
    // Get frameset manager
    frameMgr = API_GetFramesetManager();
    
    slkMgr = Slk_GetSlkManager();
    
    // Set data on frameset manager
    <%  WriteFrameMgrInit();  %>
     
    // Register with framemanager that loading is complete
	frameMgr.RegisterFrameLoad(HIDDEN_FRAME); 
	
}
</script>
</head>
<body onload="OnLoad();">
    <form id="form1" runat="server">
    <div>
    <% WriteHiddenControls(); %>
    </div>
    </form>
</body>
</html>
