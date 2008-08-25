<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.Frameset.FramesetCode" ValidateRequest="False" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title><%=PageTitleHtml%></title>
    <LINK rel="stylesheet" type="text/css" href="/_layouts/SharePointLearningKit/Frameset/Theme/Styles.css" />

<% if (!ShowError)  // don't write script if there is an error on the page
{ %>
    
    <script src="./Include/Rte1p2Api.js"></script>
    <script src="./Include/Rte2004Api.js" ></script> 
    <script src="./Include/parser.js"> </script>
    <script src="./Include/parser1p2.js"> </script>
    <script src="./Include/typevalidators.js"> </script>
    <script src="./Include/typevalidators1p2.js"> </script>
    <script src="./Include/RteApiSite.js"> </script>
    
    <script src="./Include/FramesetMgr.js"> </script>
    
    <script>

    // Constants
    SCORM_2004 = "V1p3";
    SCORM_12 = "V1p2";

    // FrameMgr is called from all frames
    var g_frameMgr = new FramesetManager;

    var g_scormVersion = "<%=ScormVersionHtml %>";	// Version of current session

    var API_1484_11 = null; // Name of RTE object for 2004 -- name is determined by SCORM.
    var API = null; // Name of RTE object for 1.2 -- name is determined by SCORM

    // Internal RTE object -- it's the same object as the api objects, just easier to reference.
    var g_API = g_frameMgr.GetRteApi(g_scormVersion, <%=RteRequired %>);  

    if (g_scormVersion == SCORM_2004)
    {
        API_1484_11 = g_API;
    }
    else
    {
        API  = g_API;
    }
    </script>
   <% } %>
    
    <script>
    // Small object to hold information about learner assignment unique to SLK
    function SlkFramesetManager()
    {
        // long. Value of the learner assignment id.
        this.LearnerAssignmentId;
        
        // String. Value to display in status field.
        this.Status;
        
        // String. Value to indicate what to display in the pass/fail column. This 
        // may be either "passed" or "failed", not localized. The value may also be 
        // undefined.
        this.PassFail; 
        
        // Float. The final points for the assignment. This is invariant culture.
        this.FinalPoints;
        
        // Float. Value to display in the Autograde points field. 
        this.ComputedPoints;
    }
    
    // Slk information from the frameset
    var g_slkFrameMgr = new SlkFramesetManager;
    var g_slkUpdateLearnerAssignment = null; 
    
    // The onUnload event handler.
    // bSendData: If true, then data is sent to opener. If false, the event handler in opener is 
    // called, but contains no data.
    function onUnload(bSendData)
    {
        if (g_slkUpdateLearnerAssignment == null)
            return;

        // Put this into a try/catch because the window.opener may have been closed before the frameset was closed.
        try
        {            
            if (bSendData)
                g_slkUpdateLearnerAssignment(g_slkFrameMgr.LearnerAssignmentId, g_slkFrameMgr.Status, g_slkFrameMgr.PassFail , g_slkFrameMgr.FinalPoints , g_slkFrameMgr.ComputedPoints );
            else
                g_slkUpdateLearnerAssignment();
        }
        catch(e)
        {
            // Do nothing.
        }
    }
    
    function onLoad()
    {
        if ((typeof(window.opener) == "undefined") || (window.opener == null))
        {
            <% if (OpenedByGradingPage) { %>
                alert("<%=CannotUpdateGradingHtml%>");
                <%} %>
             return; 
        }
           
        if (typeof(window.opener.SlkUpdateLearnerAssignment) == "undefined")
        {
            <% if (OpenedByGradingPage) { %>
                alert("<%=CannotUpdateGradingHtml%>");
                <%} %>
            return;
            }
            
        g_slkUpdateLearnerAssignment = window.opener.SlkUpdateLearnerAssignment;
        
        window.opener = null;
    }
    	
    </script>

</head>
<% if (ShowError)
   {  %>
   <body class="ErrorBody" onunload="onUnload(false);" onload="onLoad();">

        <table border="0" width="100%" id="table1" style="border-collapse: collapse">
	    <tr>
		    <td width="60">
		    <p align="center">
		    <img border="0" src="/_layouts/SharePointLearningKit/Frameset/Theme/Error.gif" width="49" height="49"></td>
		    <td class="ErrorTitle"><% =ErrorTitle %></td>
	    </tr>
	    <tr>
		    <td width="61">&nbsp;</td>
		    <td><hr></td>
	    </tr>
	    <tr>
		    <td width="61">&nbsp;</td>
		    <td class="ErrorMessage"><% =ErrorMsg %></td>
	    </tr>
        </table>

    </body>
   <%} else   // no error, so show frameset	
   { %>
<FRAMESET id="framesetOuter" name="framesetOuter" rows="53,*" framespacing="0" frameborder="0" border="0" onunload="onUnload(true);" onload="onLoad();" >

   <FRAME class="frameTitle" name="frameTitle" src="Title.htm" id="frameTitle" 
                    marginwidth="0" marginheight="0" framespacing="0" noresize="noresize" scrolling="no" frameborder="0" />

   <FRAME class="ShellFrame" id="frameLearnTask" name="frameLearnTask" src="<% =MainFramesUrl %>" 
                scrolling="no" marginwidth="0" marginheight="0" framespacing="0" frameborder="0" />

</FRAMESET>
<%} %>
</html>
