<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DeletePackages.aspx.cs" Inherits="DeletePackages" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
     
<head id="Head1" runat="server">

    <title>Delete Packages</title>

    <script type="text/javascript">
    
        // allow this dialog box to post to itself
        window.name = "ThisDialog";

        // if this is a simulated dialog box (using window.open instead of
		// showModalDialog), initialize dialogArguments from the parent window
        if (typeof(dialogArguments) == "undefined")
            dialogArguments = opener.dialogArguments;

        function PleaseWait()
        {
            // clears content from the window and displays a "Please wait" message
            document.body.innerHTML = "<table class='FullPageMessage'><tr><td>Please wait...</td></tr></table>";
        }

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

        <!-- panel that displays when this form is loaded -->
        <asp:Panel ID="ConfirmMessage" CssClass="FixedMessage_" runat="server">
		    <table style="height: 90%; width: 100%;">
		        <tr style="vertical-align: bottom">
                    <td style="text-align: center">Are you sure you want delete the selected package(s)?</td>
			    </tr>
			</table>
		</asp:Panel>
        
		<!-- panel for displaying an error message -->
        <asp:Panel ID="ErrorIntro" CssClass="Top_" visible="false" runat="server">
            The package(s) could not be deleted due to the following problem:
        </asp:Panel>
        <asp:Panel ID="ErrorMessage" CssClass="Content_" visible="false" Style="" runat="server" />

        <!-- buttons at the bottom, in either case -->
        <asp:Panel ID="Buttons" CssClass="Bottom_" runat="server">
            <asp:Button ID="DeletePackagesButton" Text="OK" CssClass="ActionButton" OnClick="DeletePackagesButton_Click"
                OnClientClick="setTimeout(PleaseWait, 1)" runat="server" />
            <asp:Button ID="CloseButton" Text="Cancel" CssClass="ActionButton" OnClientClick="window.close(); return false;" runat="server" />
        </asp:Panel>

    </asp:Panel>
    
    <asp:HiddenField ID="PackagesToDelete" runat="server" />
    <asp:HiddenField ID="PackagesSuccessfullyDeleted" runat="server" />

    </form>
    
    <script type="text/javascript">
    
        // initialize the PackagesToDelete hidden field with the
		// comma-separated list of packages to delete from the parent page
        pageForm.PackagesToDelete.value = dialogArguments.PackagesToDelete;

    </script>

    <asp:Literal ID="UpdateParentPageScript" Visible="false" runat="server">
    <script type="text/javascript" defer="defer">

        // after a package are deleted, this script is executed to update the
		// parent page: the hidden form element PackagesSuccessfullyDeleted
		// contains a comma-delimited list of numerical IDs of packages that
		// were deleted -- we need to call the parent page to update the
		// TrainingGrid on that page
        var strPackageIdList = pageForm.PackagesSuccessfullyDeleted.value;
        if (strPackageIdList.length > 0)
        {
            var astrPackageId = strPackageIdList.split(",");
		    for (var istrPackageId = 0; istrPackageId < astrPackageId.length; istrPackageId++)
		    {
		        var strPackageId = astrPackageId[istrPackageId];
			    dialogArguments.DeleteRowsFromTrainingGrid("Package" + strPackageId);
		    }
    	    dialogArguments.AfterDeletingRows();
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

