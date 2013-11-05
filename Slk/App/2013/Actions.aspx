<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.ActionsPage" DynamicMasterPageFile="~masterurl/default.master" ValidateRequest="False" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<asp:HyperLink runat="server" ID="DocLibLink" />:
	<asp:Label ID="ResourceFileName" runat="server" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
	<script type="text/javascript">
	function DisplayAddSiteUi()
	{
		document.getElementById("AddSiteLink").style.display = "none";
		document.getElementById("AddSiteUi").style.display = "block";
		document.getElementById(addSiteUrl).focus();
	}
	function TestSiteUrl()
	{
		try
		{
			window.open(document.getElementById(addSiteUrl).value, "_blank");
		}
		catch (e)
		{
			alert(addSiteUrlMessage);
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
<SharePoint:DelegateControl ControlId="SlkStartContent" runat="server"/>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="SectionLine" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>

<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow>
	<slk:TableGridColumn ColumnSpan="2" ColumnType="FormDefault" CssClass="UserGenericText" Width="615">
		<asp:Label ID="lblTitle" CssClass="UserGenericHeader" runat="server" /><br />
		<asp:Label ID="lblDescription" CssClass="UserGenericText" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnSpan="2" ColumnType="FormBreak"></slk:TableGridColumn></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnSpan="2" ColumnType="FormBreak"></slk:TableGridColumn></slk:TableGridRow>
<slk:TableGridRow ID="organizationRow" runat="server">
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblOrganization" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:DropDownList ID="organizationList" CssClass="ms-long" AutoPostBack="true" runat="server" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow ID="organizationRowBottomLine" runat="server">
	<slk:TableGridColumn ColumnSpan="2" ColumnType="FormLine"></slk:TableGridColumn>
</slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="SectionLine" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>

<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormDefault">
		<asp:Label ID="lblWhatHeader" CssClass="UserGenericHeader" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>

<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
<slk:TableGridRow ID="selfAssignRow" runat="server">
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblSelfAssignHeader" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:LinkButton ID="lnkAssignSelf" OnClick="lnkAssignSelf_Click" CausesValidation="false" runat="server" EnableViewState="false"
		/>&nbsp;<asp:Label ID="lblAssignSelf" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnType="FormLabel">
		<asp:Label ID="lblSelfAssignAssign" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
	<slk:TableGridColumn ColumnType="FormBody">
		<asp:label ID="lblChoose" runat="server" EnableViewState="false" />
		<asp:PlaceHolder ID="siteList" runat="server" EnableViewState="false" />
		<asp:LinkButton ID="lnkMRUShowAll" CausesValidation="false" OnClick="lnkMRUShowAll_Click" runat="server" EnableViewState="false" />
		<p id="AddSiteLink" style="margin-top: 8px; margin-bottom: 2px;"><asp:HyperLink ID="lnkMRUAddSite" NavigateUrl="javascript:DisplayAddSiteUi()" runat="server" EnableViewState="false" /></p>
		<p id="AddSiteUi" style="display: none; margin-top: 8px; margin-bottom: 4px;">
		<asp:Label ID="lblMRUAddress" runat="server" EnableViewState="false" /><br/>
		<asp:TextBox ID="txtNewSite" CssClass="ms-long" Width="300px" runat="server" />&nbsp;<asp:Button ID="addButton" CssClass="ms-ButtonHeightWidth" OnClick="addButton_Click" runat="server" EnableViewState="false" />
		<br/>
		<asp:HyperLink ID="lnkMRUTestLink" runat="server" NavigateUrl="javascript:TestSiteUrl()" EnableViewState="false" />
		<asp:RequiredFieldValidator ControlToValidate="txtNewSite" Display="none" ID="newSiteRequired" runat="server" ErrorMessage="" EnableViewState="false" />
		<asp:ValidationSummary ID="newSiteRequiredSummary" ShowSummary="false" DisplayMode="SingleParagraph" ShowMessageBox="true" runat="server" EnableViewState="false" />
	</slk:TableGridColumn>
</slk:TableGridRow>
<slk:TableGridRow>
	<slk:TableGridColumn ColumnSpan="2" ColumnType="FormLine"><img height=1 width=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif"></slk:TableGridColumn>
</slk:TableGridRow>
</slk:TableGrid>
<img height=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif" width=590> 
<SharePoint:DelegateControl ControlId="SlkEndContent" runat="server"/>
</asp:Panel>
</td>
</tr>
</table>
</asp:Content>

