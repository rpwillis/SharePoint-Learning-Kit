<%-- _lcid="1033" _version="12.0.4518" _dal="1" --%>
<%-- _LocalBinding --%>
<%@ Page language="C#" MasterPageFile="../../_catalogs/masterpage/CourseManager.master"    Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=12.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" meta:progid="SharePoint.WebPartPage.Document" meta:webpartpageexpansion="full" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Import Namespace="Microsoft.SharePoint" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="WpNs0" Namespace="Axelerate.SlkCourseManagerLogicalLayer.WebParts" Assembly="Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4"%>
<asp:Content ID="Content1" ContentPlaceHolderId="PlaceHolderMain" runat="server">
	<SharePoint:ListProperty ID="ListProperty1" Property="Title" runat="server"/> - 
	<SharePoint:ListItemProperty ID="ListItemProperty1" Property="BaseName" MaxLength=40 runat="server"/>
	<META Name="GENERATOR" Content="Microsoft SharePoint">
	<META Name="ProgId" Content="SharePoint.WebPartPage.Document">
	<META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=utf-8">
	<META Name="CollaborationServer" Content="SharePoint Team Web Site">
	<script>
	var navBarHelpOverrideKey = "wssmain";
	</script>
		<table cellpadding="4" cellspacing="0" border="0" width="100%">
				<tr>
					<td id="_invisibleIfEmpty" name="_invisibleIfEmpty" valign="top" width="100%"> 
					<WebPartPages:WebPartZone runat="server" Title="loc:FullPage" ID="FullPage" FrameType="TitleBarOnly" AllowCustomization="True"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone> </td>
				</tr>
				<script language="javascript">if(typeof(MSOLayout_MakeInvisibleIfEmpty) == "function") {MSOLayout_MakeInvisibleIfEmpty();}</script>
		</table>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<WebPartPages:WebPartZone runat="server" Title="loc:TitleBar" ID="TitleBar" AllowLayoutChange="false" AllowPersonalization="false"><ZoneTemplate>
</ZoneTemplate></WebPartPages:WebPartZone>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderId="PlaceHolderTitleAreaClass" runat="server">
	<style type="text/css">
	Div.ms-titleareaframe {
	height: 100%;
	}
	.ms-pagetitleareaframe table {
	background: none;
	}
  </style>
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderId="PlaceHolderSearchArea" runat="server">
	<SharePoint:DelegateControl ID="DelegateControl1" runat="server"
		ControlId="SmallSearchInputBox"/>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderId="PlaceHolderLeftActions" runat="server">
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
	<SharePoint:ProjectProperty ID="ProjectProperty1" Property="Description" runat="server"/>
</asp:Content>
<asp:Content ID="Content7" ContentPlaceHolderId="PlaceHolderBodyRightMargin" runat="server">
	<div height=100% class="ms-pagemargin"><IMG SRC="/_layouts/images/blank.gif" width=10 height=1 alt=""></div>
</asp:Content>
<asp:Content ID="Content8" ContentPlaceHolderId="PlaceHolderPageImage" runat="server"></asp:Content>
<asp:Content ID="Content9" ContentPlaceHolderId="PlaceHolderLeftNavBar" runat="server"></asp:Content>
<asp:Content ID="Content10" ContentPlaceHolderId="PlaceHolderNavSpacer" runat="server"></asp:Content>

