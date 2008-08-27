<%@ Page Language="C#" inherits="Microsoft.SharePoint.WebPartPages.WebPartPage, Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> <%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Register tagprefix="WebControls" namespace="Axelerate.SlkCourseManagerLogicalLayer.WebControls" assembly="Axelerate.SlkCourseManagerLogicalLayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=5bbd900fcde291a4" %>

<html dir="ltr">

<head runat="server">
<meta name="ProgId" content="SharePoint.WebPartPage.Document">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<SharePoint:CssLink runat="server"/>
<SharePoint:ScriptLink language="javascript" name="core.js" Defer="true" runat="server"/>
<SharePoint:ScriptLink runat="server" language="javascript" name="SPDocumentSelectorIFrame.js" id="ScriptLink1"/>
<title>DocumentViewer.aspx</title>
<meta name="Microsoft Theme" content="none, default">

</head>

<body style="margin:0px 0px 0px 0px;background-color:#83B0EC">


<form id="form1" runat="server">

<WebPartPages:SPWebPartManager runat="server" id="WebPartManager"></WebPartPages:SPWebPartManager>
<table border="0" cellpadding="0" cellspacing="0" style="width:100%;height:100%">
<tr>
	<td height="0px">
	<div style="width:100%; height:100%;  padding:10px 10px 10px 10px" class="ms-pagebottommarginleft">
	<asp:Button Style="background-image:url('/DevCommon/images/up_one_level.gif'); background-repeat:no-repeat;background-position:left;" runat="server" Text="Up" ID="btUP" OnClientClick="javascript:parent.frames['SPPopupDivID_IFrame'].history.back();return false;" Width="66px" CssClass="ms-quickLaunch"/>
	</div>

	</td>
</tr>
<tr>
	<td height="100%" valign="top" style=" background-color:white ">
	
	<WebPartPages:WebPartZone id="main" runat="server" title="Zone 1" Height="100%" Width="100%" __designer:Preview="&lt;Regions&gt;&lt;Region Name=&quot;0&quot; Editable=&quot;True&quot; Content=&quot;&quot; NamingContainer=&quot;True&quot; /&gt;&lt;/Regions&gt;&lt;table cellspacing=&quot;0&quot; cellpadding=&quot;0&quot; border=&quot;0&quot; id=&quot;main&quot; style=&quot;height:100%;width:100%;&quot;&gt;
	&lt;tr&gt;
		&lt;td style=&quot;white-space:nowrap;&quot;&gt;&lt;table cellspacing=&quot;0&quot; cellpadding=&quot;2&quot; border=&quot;0&quot; style=&quot;width:100%;&quot;&gt;
			&lt;tr&gt;
				&lt;td style=&quot;white-space:nowrap;&quot;&gt;Zone 1&lt;/td&gt;
			&lt;/tr&gt;
		&lt;/table&gt;&lt;/td&gt;
	&lt;/tr&gt;&lt;tr&gt;
		&lt;td style=&quot;height:100%;&quot;&gt;&lt;table cellspacing=&quot;0&quot; cellpadding=&quot;2&quot; border=&quot;0&quot; style=&quot;border-color:Gray;border-width:1px;border-style:Solid;width:100%;height:100%;&quot;&gt;
			&lt;tr valign=&quot;top&quot;&gt;
				&lt;td _designerRegion=&quot;0&quot;&gt;&lt;table cellspacing=&quot;0&quot; cellpadding=&quot;2&quot; border=&quot;0&quot; style=&quot;width:100%;&quot;&gt;
					&lt;tr&gt;
						&lt;td style=&quot;height:100%;&quot;&gt;&lt;/td&gt;
					&lt;/tr&gt;
				&lt;/table&gt;&lt;/td&gt;
			&lt;/tr&gt;
		&lt;/table&gt;&lt;/td&gt;
	&lt;/tr&gt;
&lt;/table&gt;" __designer:Values="&lt;P N='Title' ID='1' T='Zone 1' /&gt;&lt;P N='HeaderText' R='1' /&gt;&lt;P N='DisplayTitle' R='1' /&gt;&lt;P N='MenuPopupStyle'&gt;&lt;P N='CellPadding' T='1' /&gt;&lt;P N='CellSpacing' T='0' /&gt;&lt;/P&gt;&lt;P N='ControlStyle'&gt;&lt;P N='BorderColor' T='Gray' /&gt;&lt;P N='BorderWidth' T='1px' /&gt;&lt;P N='BorderStyle' E='4' /&gt;&lt;P N='Font' ID='2' /&gt;&lt;P N='Height' T='100%' /&gt;&lt;P N='Width' T='100%' /&gt;&lt;/P&gt;&lt;P N='Font' R='2' /&gt;&lt;P N='Height' T='100%' /&gt;&lt;P N='Width' T='100%' /&gt;&lt;P N='ID' ID='3' T='main' /&gt;&lt;P N='Page' ID='4' /&gt;&lt;P N='TemplateControl' R='4' /&gt;&lt;P N='AppRelativeTemplateSourceDirectory' R='-1' /&gt;" __designer:Templates="&lt;Group Name=&quot;ZoneTemplate&quot;&gt;&lt;Template Name=&quot;ZoneTemplate&quot; Content=&quot;&quot; /&gt;&lt;/Group&gt;"><ZoneTemplate></ZoneTemplate></WebPartPages:WebPartZone>
	</td>
</tr>
<tr>
	<td height="0px">
	
		<div style="width:100%; height:100%; text-align:right; padding:10px 10px 10px 10px" class="ms-pagebottommarginleft">
		<asp:Button runat="server" Text="Close" id="btCancel" OnClientClick="parent.SPDocumentSelector_Hide(); return false;" CssClass="ms-topnavselected" Height="26px" Width="86px"></asp:Button>
		</div>

	</td>
</tr>
</table>


</form>

</body>

</html>
