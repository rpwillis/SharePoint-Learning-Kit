<asp:Content ContentPlaceHolderId="PlaceHolderLeftNavBar" runat="server"/>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleLeftBorder" runat="server">
<table cellpadding=0 height=100% width=100% cellspacing=0>
 <tr><td class="ms-areaseparatorleft"><IMG SRC="/_layouts/images/blank.gif" width=1 height=1 alt=""></td></tr>
</table>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleAreaClass" runat="server">
<script id="onetidPageTitleAreaFrameScript">
	document.getElementById("onetidPageTitleAreaFrame").className="ms-areaseparator";
</script>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyAreaClass" runat="server">
<style type="text/css">
.ms-bodyareaframe {
	padding: 8px;
	border: none;
}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyLeftBorder" runat="server">
<div class='ms-areaseparatorleft'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleRightMargin" runat="server">
<div class='ms-areaseparatorright'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderBodyRightMargin" runat="server">
<div class='ms-areaseparatorright'><IMG SRC="/_layouts/images/blank.gif" width=8 height=100% alt=""></div>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderTitleAreaSeparator" runat="server"/>
<%@ Page language="C#" MasterPageFile="~masterurl/default.master"    Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=12.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Import Namespace="Microsoft.SharePoint" %> <%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:Content ContentPlaceHolderId="PlaceHolderPageImage" runat="server">
	<IMG SRC="/_layouts/images/blank.gif" width=1 height=1 alt="">
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
<script language=javascript>
var fCtl=false;
function EnsureUploadCtl()
{
	return browseris.ie5up && !browseris.mac &&
		null != document.getElementById("idUploadCtl");
}
function MultipleUploadView()
{
	if (EnsureUploadCtl())
	{
		treeColor = GetTreeColor();
		document.all.idUploadCtl.SetTreeViewColor(treeColor);
		if(!fCtl)
		{
			rowsArr = document.all.formTbl.rows;
			for(i=0; i < rowsArr.length; i++)
			{
				if ((rowsArr[i].id != "OverwriteField") &&
					(rowsArr[i].id != "trUploadCtl"))
				{
					rowsArr[i].removeNode(true);
					i=i-1;
				}
			}
			document.all.reqdFldTxt.removeNode(true);
			newCell = document.all.OverwriteField.insertCell();
			newCell.innerHTML = "&nbsp;";
			newCell.style.width="60%";
			document.all("dividMultipleView").style.display="inline";
			fCtl = true;
		}
	}
}
function RemoveMultipleUploadItems()
{
	if(browseris.nav || browseris.mac ||
		!EnsureUploadCtl()
	)
	{
		formTblObj = document.getElementById("formTbl");
		if(formTblObj)
		{
			rowsArr = formTblObj.rows;
			for(i=0; i < rowsArr.length; i++)
			{
				if (rowsArr[i].id == "trUploadCtl" || rowsArr[i].id == "diidIOUploadMultipleLink")
				{
					formTblObj.deleteRow(i);
				}
			}
		}
	}
}
function DocumentUpload()
{
	if (fCtl)
	{
		document.all.idUploadCtl.MultipleUpload();
	}
	else
	{
		ClickOnce();
	}
}
function GetTreeColor()
{
	var bkColor="";
	if(null != document.all("onetidNavBar"))
		bkColor = document.all.onetidNavBar.currentStyle.backgroundColor;
	if(bkColor=="")
	{
		numStyleSheets = document.styleSheets.length;
		for(i=numStyleSheets-1; i>=0; i--)
		{
			numRules = document.styleSheets(i).rules.length;
			for(ruleIndex=numRules-1; ruleIndex>=0; ruleIndex--)
			{
				if(document.styleSheets[i].rules.item(ruleIndex).selectorText==".ms-uploadcontrol")
					uploadRule = document.styleSheets[i].rules.item(ruleIndex);
			}
		}
		if(uploadRule)
			bkColor = uploadRule.style.backgroundColor;
	}
	return(bkColor);
}
</script>
<script language="javascript">
	function _spBodyOnLoad()
	{
		var frm = document.forms[MSOWebPartPageFormName];
		frm.encoding="multipart/form-data";
	}
</script>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
		<WebPartPages:WebPartZone runat="server" FrameType="None" ID="Main" Title="loc:Main" />
	<input TYPE="hidden" NAME="VTI-GROUP" VALUE="0">
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<SharePoint:EncodedLiteral runat="server" text="<%$Resources:wss,upload_pagetitle_form%>" EncodeMethod='HtmlEncode'/>
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<SharePoint:EncodedLiteral runat="server" text="<%$Resources:wss,upload_pagetitle_form%>" EncodeMethod='HtmlEncode'/>: <SharePoint:ListProperty Property="LinkTitle" runat="server"/>
</asp:Content>
