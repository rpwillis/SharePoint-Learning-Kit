<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages.Administration, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="Microsoft.SharePointLearningKit.AdminPages.ConfigurePage" MasterPageFile="~/_admin/admin.master" ValidateRequest="False" %>

<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register Tagprefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" src="~/_controltemplates/ButtonSection.ascx" %>

<asp:Content contentplaceholderid="PlaceHolderPageTitle" runat="server">
	<asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:content>

<asp:Content contentplaceholderid="PlaceHolderPageTitleInTitleArea" runat="server">
    <asp:Literal runat="server" ID="pageTitleInTitlePage" EnableViewState="false" />
</asp:Content>

<asp:content contentplaceholderid="PlaceHolderAdditionalPageHead" runat="server">
  <script language="javascript" type="text/javascript">
    function BtnOK_OnClientClick(strBtnId)
    {
        if (Page_IsValid != null && Page_IsValid == true) 
        {
            if (window.g_OKClicked == undefined)
            {
                TBodyError.style.display = 'none';
                TBodyWait.style.display = 'block';
                TBodyDone.style.display = 'none';
                TBodyMain.style.display = 'none';
                TBodyButtons.style.display = 'none';
                window.g_OKClicked = true;
                setTimeout('aspnetForm.' + strBtnId + '.click()', 1);
                return false;
            }
            else
                return true;
        }
        else
            return false;
    }
    function OperationCompleted()
    {
        TBodyError.style.display = 'none';
        TBodyWait.style.display = 'none';
        TBodyDone.style.display = 'block';
        TBodyMain.style.display = 'none';
        TBodyButtons.style.display = 'none';
    }
  </script>
</asp:content>

<asp:content contentplaceholderid="PlaceHolderPageDescription" runat="server">
    <asp:Literal ID="pageDescription" runat="server" EnableViewState="false" />
</asp:content>

<asp:content contentplaceholderid="PlaceHolderMain" runat="server">

<table width="100%" class="propertysheet" cellspacing="0" cellpadding="0" border="0">
<tbody id="TBodyError">
  <tr>
    <td class="ms-descriptionText">
      <asp:Label ID="LabelMessage" Runat="server" EnableViewState="False" CssClass="ms-descriptionText"/>
    </td>
  </tr>
  <tr>
    <td class="ms-error"><asp:Label ID="LabelErrorMessage" Runat="server" EnableViewState="False" /></td>
  </tr>
  <tr>
    <td class="ms-descriptionText">
      <asp:ValidationSummary ID="ValSummary" HeaderText="<%$SPHtmlEncodedResources:spadmin, ValidationSummaryHeaderText%>" DisplayMode="BulletList" ShowSummary="True" runat="server">
      </asp:ValidationSummary>
    </td>
  </tr>
  <tr>
    <td><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="10" height="1" alt="" /></td>
  </tr>
</tbody>
</table>

<table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="100%">
<tbody id="TBodyWait" style="display: none">
  <tr>
    <td>
      <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
          <td style="padding-top:0px;padding-left: 20px;padding-right:20px;" >
			<asp:Image ID="imagePleaseWait" runat="server" ImageUrl="Wait.gif" />
          </td>
          <td width="100%">
			<asp:Label ID="labelPleaseWait" CssClass="ms-sectionheader" runat="server" />
            <span class="ms-descriptiontext" />
          </td>
        </tr>
        <tr>
          <td height=1 colspan=2><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1" height="8" alt="" /></td>
        </tr>
        <tr>
          <td class="ms-sectionline" height="1" colspan="2"><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1" height="1" alt="" /></td>
        </tr>
      </table>
    </td>
  </tr>
</tbody>

<tbody id="TBodyDone" style="display: none">
  <tr>
    <td>
      <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
          <td style="padding-top:0px;padding-left: 20px;padding-right:20px;" >
			<asp:Image ID="imageConfigurationComplete" runat="server" ImageUrl="Info.gif" />
          </td>
          <td width="100%">
			<asp:Label ID="labelConfigurationComplete" CssClass="ms-sectionheader" runat="server" />
            <span class="ms-descriptiontext"></span>
          </td>
        </tr>
        <tr>
          <td height=1 colspan=2><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1" height="8" alt=""></td>
        </tr>
        <tr>
          <td colspan="2" class="ms-descriptiontext ms-inputformdescription"><asp:HyperLink NavigateUrl="?" ID="linkConfigureAnother" runat="server" /></td>
        </tr>
        <tr>
          <td height=1 colspan=2><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1" height="8" alt=""></td>
        </tr>
        <tr>
          <td class="ms-sectionline" height="1" colspan="2"><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1" height="1" alt=""></td>
        </tr>
      </table>
    </td>
  </tr>
</tbody>

<tbody id="TBodyMain">
<wssuc:InputFormSection id="inputSiteSelector" runat="server">
  <Template_InputFormControls>
    <tr>
      <td style="position: relative; left: -5px;">
        <SharePoint:SiteAdministrationSelector id="SPSiteSelector" runat="server" OnContextChange="SPSiteSelector_OnContextChange" AllowAdministrationWebApplication="false"/>
      </td>
    </tr>
  </Template_InputFormControls>
</wssuc:InputFormSection>
<wssuc:InputFormSection id="inputDatabase" runat="server">
  <Template_Description>
	<asp:Label ID="labelDatabaseDescription" runat="server" />
  </Template_Description>
  <Template_InputFormControls>
    <wssuc:InputFormControl runat="server" id="inputDatabaseServer">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtDatabaseServer" Columns="35" Runat="server" MaxLength=128 />
      </Template_control>
    </wssuc:InputFormControl>
    <wssuc:InputFormControl runat="server">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtDatabaseName" Columns="35" Runat="server" MaxLength=128 />
      </Template_control>
    </wssuc:InputFormControl>
    <wssuc:InputFormControl runat="server">
      <Template_Control>
        <wssawc:InputFormCheckBox ID="ChkCreateDatabase" runat="server"/>
      </Template_Control>
    </wssuc:InputFormControl>
  </Template_InputFormControls>
</wssuc:InputFormSection>
<wssuc:InputFormSection id="inputPermissions" runat="server">
  <Template_Description>
	<asp:Label ID="labelPermissionsDescription" runat="server" />
  </Template_Description>
  <Template_InputFormControls>
    <wssuc:InputFormControl runat="server" id="inputInstructorPermissions">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtInstructorPermission" Columns="35" Runat="server" MaxLength=128 />
      </Template_control>
    </wssuc:InputFormControl>
    <wssuc:InputFormControl runat="server" id="inputLearnerPermissions">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtLearnerPermission" Columns="35" Runat="server" MaxLength=128 />
      </Template_control>
    </wssuc:InputFormControl>
        <wssuc:InputFormControl runat="server" id="inputObserverPermissions">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtObserverPermission" Columns="35" Runat="server" MaxLength=128 />
      </Template_control>
    </wssuc:InputFormControl>

    <wssuc:InputFormControl runat="server">
      <Template_Control>
        <wssawc:InputFormCheckBox ID="ChkCreatePermissions" runat="server"/>
      </Template_Control>
    </wssuc:InputFormControl>
  </Template_InputFormControls>
</wssuc:InputFormSection>

<wssuc:InputFormSection id="inputDropBoxFilesExtensionsSection" runat="server">
  <Template_Description>
	<asp:Label ID="labelDropBoxFilesExtensionsDescription" runat="server" />
  </Template_Description>
  <Template_InputFormControls>
    <wssuc:InputFormControl runat="server" id="inputDropBoxFilesExtensions">
      <Template_control>
        <wssawc:InputFormTextBox CssClass="ms-input" ID="TxtDropBoxFilesExtensions" Columns="35" Runat="server" MaxLength=128 />
        <br />
        <asp:RegularExpressionValidator runat="server" id="REVDropBoxFilesExtensions" Display="Dynamic" controltovalidate="TxtDropBoxFilesExtensions" validationexpression="(\s*)([a-zA-Z]{3,5}(\s*);(\s*))*([a-zA-Z]{3,5})(\s*)" errormessage="Enter valid extensions separated by semicolons (e.g. doc;docx;xslx)" />
        <asp:RequiredFieldValidator ID="RFVDropBoxFilesExtensions" runat="server" Display="Dynamic" controltovalidate="TxtDropBoxFilesExtensions" errormessage="Enter Drop Box files extensions"></asp:RequiredFieldValidator>
      </Template_control>
    </wssuc:InputFormControl>
  </Template_InputFormControls>
</wssuc:InputFormSection>

<wssuc:InputFormSection id="inputSlkSettings" runat="server">
  <Template_Description>
	<asp:Label ID="labelSlkSettingDescription" runat="server" />
	<p style="margin-top: 0pt; margin-bottom: 0pt;">
		<ul style="margin-top: 0pt; margin-bottom: 0pt;">
			<li style="margin-bottom: 6pt"><asp:HyperLink ID="LinkCurrentSettingsFile" Target="_blank" runat="server" /></li>
			<li><asp:HyperLink ID="LinkDefaultSettingsFile" Target="_blank" runat="server"/></li></p>
		</ul>
	</p>
  </Template_Description>
  <Template_InputFormControls>
    <wssuc:InputFormControl id="inputSlkSettingsFile" runat="server">
      <Template_control>
        <asp:FileUpload ID="FileUploadSlkSettings" CssClass="ms-input" style="margin-left: 7pt; width: 169pt" runat="server" />
      </Template_control>
    </wssuc:InputFormControl>
  </Template_InputFormControls>
</wssuc:InputFormSection>
</tbody>

<tbody id="TBodyButtons">
<wssuc:ButtonSection runat="server">
  <Template_Buttons>
    <asp:Button ID="BtnOK" UseSubmitBehavior="false" CssClass="ms-ButtonHeightWidth" OnClick="BtnOk_Click" OnClientClick="if (!BtnOK_OnClientClick(this.id)) return false;" Enabled="true" runat="server" />
  </Template_Buttons>
</wssuc:ButtonSection>
</tbody>
</table>

<asp:Panel ID="OperationCompletedPanel" Visible="false" runat="server">
    <script language="javascript" type="text/javascript">
        OperationCompleted();
    </script>
</asp:Panel>

</asp:content>
