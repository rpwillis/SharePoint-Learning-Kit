<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.LobbyPage" MasterPageFile="~/_layouts/application.master" ValidateRequest="False" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
   <asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
    <asp:Literal runat="server" ID="pageTitleInTitlePage" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
 
<style type="text/css">
.ms-propertysheet a[disabled='disabled']{
color:#999999;
text-decoration:none;
}

.ms-areaseparatorright img{
visibility:hidden;
}

</style>


<script type="text/javascript">

// When Frameset closes, this method is called with the above information. The only data this page cares 
// about is the dLearnerAssignmentId and the strStatus. Other information is ignored. 
// dLearnerAssignmentId is the id of the assignment
// strStatus is the text-format string that should be displayed to the user for assignment status
// fFinalPoints is the current final points for the learner assignment
// fAutogradePoints is the total points assigned by the content, including teacher grades of individual questions
function SlkUpdateLearnerAssignment(dLearnerAssignmentId, strStatus, fFinalPoints, fAutogradePoints)
{
	if (typeof(dLearnerAssignmentId) != "undefined")
	{
		if (document.getElementById(lblStatusValue).innerText != strStatus)
		{
			var url = window.location.href;
			// #in the url will make the refresh not work
			// before refresh, get rid of the #
			var iPosition = url.indexOf("#")
			if (iPosition == -1)
				window.location.href = url;
			else
				window.location.href = url.substring(0, iPosition);
		}
	}
}

var slk_OpenedFrameSet;
// Opens a new window for the Frameset.
function SlkOpenFramesetWindow(navigateUrl)
{
	// If the new window is outside the domain of this window
	// we will get an Access Denied error. If so, show an alert stating
	// that the assignment is already open in another window.
	try
	{
		if((slk_OpenedFrameSet != undefined) &&
			(slk_OpenedFrameSet != null) &&
			(!slk_OpenedFrameSet.closed))
		{
			slk_OpenedFrameSet.focus();
		}
		else
		{
			var openWindow = window.open(navigateUrl , "_blank");
			slk_OpenedFrameSet = openWindow;
		}
	}
	catch (err)
	{
		alert(SlkWindowAlreadyOpen);
	}
}

</script>
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
    <asp:Literal ID="pageDescription" runat="server" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
<table cellpadding="0" cellspacing="0">
<tr>
<td Width="615">
<slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
<asp:Panel ID="contentPanel" Visible="false" runat="server">
<slk:ButtonToolbar runat="server" ID="buttonToolbarTop" EnableViewState="false">
	<slk:SlkButton ID="slkButtonBegin" runat="server" />
	<slk:SlkButton ID="slkButtonSubmitFiles" runat="server" />
	<slk:SlkButton ID="slkButtonReviewSubmitted" Visible="false" runat="server" />
	<slk:SlkButton ID="slkButtonSubmit" OnClick="slkButtonSubmit_Click" runat="server" />
	<slk:SlkButton ID="slkButtonDelete" OnClick="slkButtonDelete_Click" runat="server" />
</slk:ButtonToolbar>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="SectionLine" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow EnableViewState="false">
	<slk:TableGridColumn CssClass="UserGenericText" ColumnType="FormDefault" Width="615">
		<asp:Label ID="lblTitle" CssClass="UserGenericHeader" runat="server" EnableViewState="false" /><br />
		<asp:Label ID="lblDescription" CssClass="UserGenericText" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
</slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid CssClass="ms-formtable" style="margin-top: 8px" runat="server" cellspacing="0" cellpadding="0" width="100%">
<slk:TableGridRow ID="tgrSite" runat="server">
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblSite" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody" Width="450">
		<asp:HyperLink ID="lnkSite" runat="server" /><asp:Label ID="lblSiteValue" Visible="false" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblScore" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:Label ID="lblScoreValue" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblStatus" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:Label ID="lblStatusValue" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblStart" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:Label ID="lblStartValue" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblDue" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:Label ID="lblDueValue" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow ID="tgrComments" runat="server">
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblComments" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:Label ID="lblCommentsValue" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow ID="tgrAutoReturn" runat="server">
	<slk:TableGridColumn ColumnType="FormLabel">
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<table border="0" width="100%" cellspacing="0" cellpadding="0">
			<tr>
				<td width="0%" valign="top" style="padding-top: 3px"><asp:Image id="infoImage" ImageUrl="Images/InfoIcon.gif" runat="server" EnableViewState="false" /></td>
				<td class="ms-formbody" style="border-top: none; vertical-align: baseline" width="100%">
					<asp:Label ID="lblAutoReturn" runat="server" EnableViewState="false" />
				</td>
			</tr>
		</table>     
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnSpan="2" ColumnType="FormLine"><img height=1 width=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif"></slk:TableGridColumn>
</slk:TableGridRow>
</slk:TableGrid>
<img height=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif" width=590> 
</asp:Panel>
</td>
</tr>
</table>
</asp:Content>
