<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SequencingLog.aspx.cs" Inherits="SequencingLog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
     
<head id="Head1" runat="server">

    <title>Sequencing Log</title>

    <link rel="stylesheet" href="Styles.css" type="text/css" />

    <style type="text/css">
    .Grid TR
    {
    }
    .Grid .Id_
    {
        width: 1%;
    }
    .Grid .Time_
    {
        width: 1%;
    }
    .Grid .EventType_
    {
        width: 1%;
    }
    .Grid .NavigationCommand_
    {
        width: 1%;
    }
    .Grid .Message_
    {
        width: 97%;
    }
    </style>

</head>

<body>
    
    <form id="pageForm" target="ThisDialog" runat="server">
    
        <!-- title panel -->
        <asp:Panel ID="LogTitle" CssClass="Title" runat="server">
		    <table style="width: 100%">
			    <tr>
				    <td class="Left_">Sequencing Log - <asp:Label ID="TrainingName" runat="server" Text="Label" /></td>
				    <td class="Right_"><asp:HyperLink ID="RefreshLink" runat="server">Refresh</asp:HyperLink></td>
			    </tr>
		    </table>
        </asp:Panel>

	    <!-- sequencing log panel -->
	    <asp:Panel ID="LogContents" runat="server">
		    <asp:Table ID="LogGrid" CssClass="Grid" runat="server">
			    <asp:TableHeaderRow CssClass="Header_">
				    <asp:TableCell CssClass="Id_" Wrap="false">ID</asp:TableCell>
				    <asp:TableCell CssClass="Time_" Wrap="false">Time</asp:TableCell>
				    <asp:TableCell CssClass="EventType_" Wrap="false">Event Type</asp:TableCell>
				    <asp:TableCell CssClass="NavigationCommand_" Wrap="false">Navigation Command</asp:TableCell>
				    <asp:TableCell CssClass="Message_">Message</asp:TableCell>
			    </asp:TableHeaderRow>
		    </asp:Table>
	    </asp:Panel>

	    <!-- error message panel -->
	    <asp:Panel ID="ErrorMessage" CssClass="Content_" visible="false" Style="" runat="server" />

    </form>

</body>

</html>

