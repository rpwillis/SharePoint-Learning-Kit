<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>
<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!-- MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
     MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
     LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
     NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE. -->
<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <title>BasicWebPlayer</title>
    
    <link rel="stylesheet" href="Styles.css" type="text/css" />

    <style type="text/css">
    .Grid TR
    {
        height: 17pt;
    }
    .Grid .Select_
    {
        width: 1%;
    }
    .Grid .Name_
    {
        width: 96%;
    }
    .Grid .Uploaded_
    {
        width: 1%;
    }
    .Grid .Status_
    {
        width: 1%;
    }
    .Grid .Score_
    {
        width: 1%;
    }
    .MessageTable
    {
        width: 100%;
        height: 100pt;
        text-align: center;
    }
    </style>
    
    <script type="text/javascript">
    
        function UploadPackage()
        {
            // display the dialog to upload a package; if a package is added
			// successfully, AddRowToTrainingGrid() will be called from the
			// the dialog to update TrainingGrid
            var args = new Object;
            args.AddRowToTrainingGrid = AddRowToTrainingGrid;
			ShowDialog("UploadPackage.aspx", args, 740, 500, false);
        }
        
        function AddRowToTrainingGrid(rowId, aCells, aClassNames)
        {
			// add a row to TrainingGrid; <rowId> is the HTML ID to use; <aCells> is an array
			// containing the HTML for each cell in the row; <aClassNames> is corresponding HTML
			// class names
			var row = document.createElement("tr");
			row.id = rowId;
			row.style.backgroundColor = "#FFFFE0"; // highlight new row
			for (var iCell = 0; iCell < aCells.length; iCell++)
			{
				var cell = document.createElement("td");
				cell.className = aClassNames[iCell];
				cell.insertAdjacentHTML("beforeEnd", aCells[iCell]);
				row.appendChild(cell);
			}
			TrainingGrid.tBodies[0].appendChild(row);

			// if TrainingGrid previously was hidden (i.e. because there's no
			// training to show), display it now, and hide NoTrainingMessage
            TrainingPanel.style.display = "inline";
            if (typeof(NoTrainingMessage) != "undefined")
                NoTrainingMessage.style.display = "none";
                
            // update the selection state
            OnSelectionChanged();
        }   
        
        function DeletePackages()
        {
            // display the dialog to delete packages; if packages are deleted
            // successfully, DeleteRowsFromTrainingGrid() will be called from
            // the dialog once per deleted package, then AfterDeletingRows()
            // will be called
            var args = new Object;
            args.PackagesToDelete = ForEachSelectionCheckbox(false, false);
            
            // if there is nothing selected, then do nothing
            if (args.PackagesToDelete.length == 0)
                return;
                
            args.DeleteRowsFromTrainingGrid = DeleteRowsFromTrainingGrid;
            args.AfterDeletingRows = AfterDeletingRows;
            ShowDialog("DeletePackages.aspx", args, 450, 250, false);
        }
        
        function DeleteRowsFromTrainingGrid(rowId)
        {
            // delete all rows from TrainingGrid that have <rowId> is their
			// HTML ID
			while (true)
			{
				var arow = document.getElementsByName(rowId);
				if ((arow == null) || (arow.length == 0))
					break;
			    arow[0].removeNode(true);
			}
        }
        
        function AfterDeletingRows()
        {
            // perform UI cleanup that should happen after deleting rows from
            // TrainingGrid...

			// update UI based on the fact that the selection changed
			OnSelectionChanged();
			
			// if all packages were deleted, hide TrainingGrid and show
			// NoTrainingMessage
			if (TrainingGrid.rows.length == 1)
			{
                TrainingPanel.style.display = "none";
                NoTrainingMessage.style.display = "inline";
			}
        } 

		function OpenTraining(strOrgOrAtt)
		{
			// open training content; <strOrgOrAtt> is either of the form "Org:<organizationId>"
			// (for content that has not been launched yet) or "Att:<attemptId>" for content that's
			// previously been launched -- in the former case we need to create an attempt for the
			// content...
			var a;
			if ((a = strOrgOrAtt.match(/^Org:([0-9]+)$/)) != null)
			{
				// display the dialog to create an attempt on this organization; if the attempt is
				// successfully created, OnAttemptCreated() will be called from the the dialog to
				// update TrainingGrid and display the training
				var args = new Object;
				args.OrganizationId = a[1];
				args.OnAttemptCreated = OnAttemptCreated;
				ShowDialog("CreateAttempt.aspx", args, 450, 250, false);
			}
			else
			if ((a = strOrgOrAtt.match(/^Att:([0-9]+)$/)) != null)
			{
				// open training in a new window
				OpenFrameset(a[1]);
			}
		}

		function OnAttemptCreated(strOrganizationId, strAttemptId)
		{
			// called after CreateAttempt.aspx has successfully created an attempt; update the
			// anchor tag to include the attempt number, then open the frameset
			var anchor = document.all["Org_" + strOrganizationId];
			anchor.href = "javascript:OpenTraining('Att:" + strAttemptId + "')";
			anchor.title = "Continue training";
			anchor.parentElement.parentElement.cells[3].innerHTML =
			    "<A href=\"javascript:ShowLog(" + strAttemptId + ")\" title=\"Show Log\">Active</A>";
			OpenFrameset(strAttemptId);
		}

		function OpenFrameset(strAttemptId)
		{
			// open the frameset for viewing training content; <strAttemptId> is the attempt ID
			window.open("Frameset/Frameset.aspx?View=0&AttemptId=" + strAttemptId, "_blank");
		}

		function ShowLog(strAttemptId)
		{
			// displays the sequencing log for this attempt
            ShowDialog("SequencingLog.aspx?AttemptId=" + strAttemptId, null, 900, 650, true);
		}
        
        function OnSelectionChanged()
        {
            // called when the list of selected checkboxes has changed
            var cSelected = ForEachSelectionCheckbox(false, false).length;
            var fAnySelected = (cSelected > 0);
            var fAllSelected = (cSelected == g_cSelectionCheckboxes);
            DeletePackagesLink.disabled = !fAnySelected;
            pageForm.SelectAll.checked = fAllSelected;           
        }

        function OnSelectAllClicked()
        {
            // called when the "Select All" checkbox is clicked
            if (pageForm.SelectAll.checked)
                ForEachSelectionCheckbox(true, false);
            else
                ForEachSelectionCheckbox(false, true);
            OnSelectionChanged();
        }

        function ForEachSelectionCheckbox(fSelect, fDeselect)
        {
            // for each selection checkbox (excluding the "Select All"
			// checkbox: select it if <fSelect> is true; deselect it if
			// <fDeselect> is true; return an array of IDs of selected
			// checkboxes; side effect: set global variable
			// <g_cSelectionCheckboxes> to the number of checkboxes
			var inputs = document.all.tags("INPUT");
            var aSelected = new Array; // IDs of selected training
			g_cSelectionCheckboxes = 0;
			for (var iInput = 0; iInput < inputs.length; iInput++)
			{
			    var input = inputs[iInput];
			    if (input.type != "checkbox")
			        continue;
			    var a = input.id.match(/^Select([0-9]+)$/);
			    if (a == null)
			        continue;
			    if (fSelect)
			        input.checked = true;
			    if (fDeselect)
			        input.checked = false;
			    if (input.checked)
			        aSelected.push(a[1]);
				g_cSelectionCheckboxes++;
			}
			return aSelected;
        }
        
        function ShowDialog(strUrl, args, cx, cy, fScroll)
        {
            // display a dialog box with URL <strUrl>, arguments <args>, width <cx>, height <cy>,
			// scrollbars if <fScroll>; this can be done using either showModalDialog() or
			// window.open(): the former has better modal behavior; the latter allows selection
			// within the window
            var useShowModalDialog = false;
			var strScroll = fScroll ? "yes" : "no";
            if (useShowModalDialog)
            {
			    showModalDialog(strUrl, args,
				    "dialogWidth: " + cx + "px; dialogHeight: " + cy +
					"px; center: yes; resizable: yes; scroll: " + strScroll + ";");
            }
            else
            {
				dialogArguments = args; // global variable accessed by dialog
				var x = Math.max(0, (screen.width - cx) / 2);
				var y = Math.max(0, (screen.height - cy) / 2);
				window.open(strUrl, "_blank", "left=" + x + ",top=" + y +
					",width=" + cx + ",height=" + cy +
					",location=no,menubar=no,scrollbars=" + strScroll +
					",status=no,toolbar=no,resizable=yes");
            }
        }
    
    </script>

</head>

<body style="overflow:auto">
    
    <form id="pageForm" runat="server">

        <!-- title panel -->
        <asp:Panel ID="Title" CssClass="Title" runat="server">
			<table style="width: 100%">
				<tr>
					<td class="Left_">My Training (<asp:Label ID="MyUserName" runat="server" Text="Label" />)</td>
					<td class="Right_"><asp:Label ID="VersionNumber" runat="server" /></td>
				</tr>
			</table>
        </asp:Panel>

        <!-- action links -->
        <asp:Panel ID="Nav" CssClass="Nav" runat="server">
        <a href="javascript:UploadPackage()">Upload Training Package</a>
        <span class="Sep">|</span>
        <asp:HyperLink ID="DeletePackagesLink" NavigateUrl="javascript:DeletePackages()" runat="server">Delete Selected Packages</asp:HyperLink>
        </asp:Panel>

        <asp:Panel ID="TrainingPanel" runat="server">
            <!-- table of this user's training -->
            <asp:Table ID="TrainingGrid" CssClass="Grid" runat="server">
                <asp:TableHeaderRow CssClass="Header_">
                    <asp:TableCell CssClass="Select_"><input id="SelectAll" type="checkbox" title="Select All" onclick="OnSelectAllClicked()" /></asp:TableCell>
                    <asp:TableCell CssClass="Name_">Name</asp:TableCell>
                    <asp:TableCell CssClass="Uploaded_">Uploaded</asp:TableCell>
                    <asp:TableCell CssClass="Status_">Status</asp:TableCell>
                    <asp:TableCell CssClass="Score_">Score</asp:TableCell>
                </asp:TableHeaderRow>
            </asp:Table>
        </asp:Panel>

        <asp:Panel ID="NoTrainingMessage" runat="server">
            <!-- message that's displayed if the TrainingGrid is empty (and is therefore hidden) -->
            <asp:Table ID="NoTrainingMessageTable" CssClass="MessageTable" runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <p>Click <span style="font-weight: bold">Upload Training Package </span> above to upload a training package.</p>
                        <p>(Training packages can be SCORM 2004, SCORM 1.2, Class Server LRM,<br/> or Class Server IMS+ format.)</p>
				    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </asp:Panel>

    </form>

</body>

</html>
