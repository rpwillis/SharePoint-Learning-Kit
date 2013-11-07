<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.Grading" DynamicMasterPageFile="~masterurl/default.master" ValidateRequest="False" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<asp:Literal runat="server" ID="pageTitleInTitlePage" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
    <%//script and styles go here %>

<style type="text/css">
    .ms-vb textarea{
    overflow: auto !important;
    }
    
    .ms-propertysheet a[disabled='disabled']{
    color:#999999;
    text-decoration:none;
    }
    
    .ms-areaseparatorright img{
	visibility:hidden;
	}
</style>
  <link rel="stylesheet" type="text/css" href="Styles.css"/>     
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
	<asp:Literal ID="pageDescription" runat="server" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
<SharePoint:DelegateControl ControlId="SlkStartContent" runat="server"/>
<table cellpadding="0" cellspacing="0">
<tr>
<td Width="715">
<slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
<asp:Label ID = "lblError" Visible ="false" runat ="server" ForeColor="Red"></asp:Label>
<asp:Panel ID="contentPanel" runat="server">
<slk:TableGrid ID="TableGrid1" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>
<slk:SimpleButtonToolbar runat="server" ID="simpleToolbarTop">
	<asp:Button ID="btnTopSave" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="btnSave_Click" EnableViewState="false" />
	<asp:Button ID="btnTopClose" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="btnClose_Click" EnableViewState="false" />
</slk:SimpleButtonToolbar>

<slk:ButtonToolbar runat="server" ID="buttonToolbarTop">
	<slk:SlkButton ID="slkButtonEdit" OnClick="slkButtonEdit_Click" EnableViewState="false" runat="server" />
	<slk:SlkButton ID="slkButtonCollect" OnClick="slkButtonCollect_Click" EnableViewState="false" runat="server" />
	<slk:SlkButton ID="slkButtonUpload" EnableViewState="false" runat="server" Visible="false" />
	<slk:SlkButton ID="slkButtonDownload" EnableViewState="false" runat="server" Visible="false" />
	<slk:SlkButton ID="slkButtonReturn" OnClick="slkButtonReturn_Click" EnableViewState="false" runat="server" />
	<slk:SlkButton ID="slkButtonDelete" OnClick="slkButtonDelete_Click" EnableViewState="false" runat="server" />
</slk:ButtonToolbar>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="SectionLine" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0" Style="border-top: none">
	<slk:TableGridRow>
		<slk:TableGridColumn ColumnType="FormBreak" />
	</slk:TableGridRow>
	<slk:TableGridRow>
		<slk:TableGridColumn CssClass="UserGenericText" ColumnType="FormDefault" Style="border-top: none">
			<asp:Label ID="lblTitle" CssClass="UserGenericHeader" runat="server" /><br /><asp:Label ID="lblDescription" CssClass="UserGenericText" runat="server" />
		</slk:TableGridColumn>
	</slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid CssClass="ms-formtable" style="margin-top: 8px" runat="server" cellspacing="0" cellpadding="0" width="100%">
	<slk:TableGridRow>
		<slk:TableGridColumn ColumnType="FormLabel">
			<asp:Label ID="lblPoints" runat="server" EnableViewState="false" />
		</slk:TableGridColumn>
		<slk:TableGridColumn ColumnType="FormBody">
			<asp:Label ID="lblPointsValue" runat="server" />      
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
	<slk:TableGridRow ID="tgrAutoReturn" runat="server">
		<slk:TableGridColumn ColumnType="FormLabel">
		</slk:TableGridColumn>
		<slk:TableGridColumn ColumnType="FormBody">
			<table id="tblAutoReturn" runat="server" visible="false" border="0" width="100%" cellspacing="0" cellpadding="0">
				<tr>
					<td width="0%" valign="top" style="padding-top: 3px"><asp:Image id="infoImageAutoReturn" ImageUrl="Images/InfoIcon.gif" runat="server" EnableViewState="false" /></td>
					<td class="ms-formbody" style="border-top: none; vertical-align: baseline" width="100%">
						<asp:Label ID="lblAutoReturn" runat="server" EnableViewState="false" />
					</td>
				</tr>
			</table>
			<table id="tblAnswers" runat="server" visible="false" border="0" width="100%" cellspacing="0" cellpadding="0">
				<tr>
					<td width="0%" valign="top" style="padding-top: 3px"><asp:Image id="infoImageAnswers" ImageUrl="Images/InfoIcon.gif" runat="server" EnableViewState="false" /></td>
					<td class="ms-formbody" style="border-top: none; vertical-align: baseline" width="100%">
						<asp:Label ID="lblAnswers" runat="server" EnableViewState="false" />
					</td>
				</tr>
			</table>     
		</slk:TableGridColumn>
	</slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormDefault" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormDefault" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="sectionline" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0" Style="border-top: none">
	<slk:TableGridRow>
		<slk:TableGridColumn ColumnType="FormBreak" />
	</slk:TableGridRow>
	<slk:TableGridRow>
		<slk:TableGridColumn CssClass="UserGenericText" ColumnType="FormDefault" Style="border-top: none">
			<asp:Label ID="lblGradeAssignment" CssClass="UserGenericHeader" runat="server" EnableViewState="false" /><br /><asp:Label ID="lblGradeAssignmentDescription" CssClass="UserGenericText" runat="server" EnableViewState="false" />
		</slk:TableGridColumn>
	</slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>
<!-- grading grid -->
<div>
    <slk:GradingList runat="server" ID="gradingList"></slk:GradingList>
</div>
<slk:TableGrid runat="server" Width="100%" CellPadding="0" CellSpacing="0">
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormLine" /></slk:TableGridRow>
	<slk:TableGridRow><slk:TableGridColumn ColumnType="FormBreak" /></slk:TableGridRow>
</slk:TableGrid>

<slk:SimpleButtonToolbar runat="server" ID="simpleToolbarBottom">
	<asp:Button ID="btnBottomSave" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="btnSave_Click" EnableViewState="false" />
	<asp:Button ID="btnBottomClose" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="btnClose_Click" EnableViewState="false" />
</slk:SimpleButtonToolbar>
<img height=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif" width=590> 
</asp:Panel>
</td>
</tr>
</table>
<SharePoint:DelegateControl ControlId="SlkStartContent" runat="server"/>
</asp:Content>
