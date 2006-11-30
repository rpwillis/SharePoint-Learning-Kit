<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CreateAttempt.aspx.cs" Inherits="CreateAttempt" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
<head id="Head1" runat="server">

    <title>Begin Training</title>

    <script type="text/javascript">
    
        // allow this dialog box to post to itself
        window.name = "ThisDialog";

        // if this is a simulated dialog box (using window.open instead of
		// showModalDialog), initialize dialogArguments from the parent window
        if (typeof(dialogArguments) == "undefined")
            dialogArguments = opener.dialogArguments;

    </script>
    
    <link rel="stylesheet" href="Styles.css" type="text/css" />

    <style type="text/css">
    HTML
    {
        overflow: hidden;
    }
    BODY
    {
        height: 100%;
        margin: 0;
    }
    .MessagePanel .Content_
    {
        height: 35%;
        padding: 8pt;
        overflow-x: hidden; /* works best in IE */
    }
    .ActionButton
    {
        width: 60pt;
        margin: 6pt;
    }
    </style>

</head>

<body>
    
    <form id="pageForm" target="ThisDialog" runat="server">
    
    <asp:Panel ID="MessagePanel" CssClass="MessagePanel" runat="server">

        <!-- table that displays when this form is loaded -->
		<asp:Table ID="PleaseWait" CssClass="FullPageMessage" runat="server">
			<asp:TableRow>
				<asp:TableCell>Please wait...</asp:TableCell>
			</asp:TableRow>
		</asp:Table>

		<!-- panel for displaying an error message -->
        <asp:Panel ID="ErrorIntro" CssClass="Top_" visible="false" runat="server">
            The training could not be launched due to the following problem:
        </asp:Panel>
        <asp:Panel ID="ErrorMessage" CssClass="Content_" visible="false" Style="" runat="server" />

        <!-- button at the bottom -->
        <asp:Panel ID="Buttons" CssClass="Bottom_" visible="false" runat="server">
            <asp:Button ID="CloseButton" Text="OK" CssClass="ActionButton" OnClientClick="window.close(); return false;" runat="server" />
        </asp:Panel>

    </asp:Panel>

	<asp:Button ID="CreateAttemptButton" Text="Create Attempt" CssClass="Hidden" OnClick="CreateAttemptButton_Click" runat="server" />

    <asp:HiddenField ID="OrganizationId" runat="server" />
    <asp:HiddenField ID="AttemptId" runat="server" />

    </form>
    
    <script type="text/javascript">
    
        // initialize the OrganizationId hidden field with information from the parent page
        pageForm.OrganizationId.value = dialogArguments.OrganizationId;

    </script>

    <asp:Literal ID="AutoPostScript" Visible="true" runat="server">
    <script type="text/javascript" defer="defer">

		// when the dialog is first loaded, after the "please wait" message has been displayed to
		// the user, this script is executed to cause an automatic post to the server to begin the
		// create attempt operation
		pageForm.CreateAttemptButton.click();

    </script>
    </asp:Literal>

    <asp:Literal ID="UpdateParentPageScript" Visible="false" runat="server">
    <script type="text/javascript" defer="defer">

        // after an attempt has been created, this script is executed to update the parent page:
		// the script function OnAttemptCreated is called
        if (pageForm.AttemptId.value.length > 0)
		{
			dialogArguments.OnAttemptCreated(pageForm.OrganizationId.value,
				pageForm.AttemptId.value);
		}
 
    </script>
    </asp:Literal>
        
    <asp:Literal ID="CloseDialogScript" Visible="false" runat="server">
    <script type="text/javascript" defer="defer">
        
        // if the operation performed by this dialog was successful, this
        // script is executed to close the dialog
        window.close();
        
    </script>
    </asp:Literal>

</body>

</html>

