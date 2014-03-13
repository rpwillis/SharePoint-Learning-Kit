<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.SubmittedFiles" MasterPageFile="~/_layouts/dialog.master" ValidateRequest="False" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderDialogHeaderPageTitle" runat="server">
    <asp:Literal ID="PageTitle" runat="server" EnableViewState="false" /> 
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <SharePoint:CssRegistration ID="CssRegistration1" runat="server" Name="ows.css" />
    <SharePoint:ScriptLink ID="ScriptLink1" Language="javascript" Name="core.js" runat="server" />
  <SharePoint:FormDigest ID="FormDigest1" runat="server" />
  <style>
  #buttonRow {
    display:none;
  }
  </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderDialogImage" runat="server">
  <img src="/_layouts/images/allcontent32.png" alt="" />
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="PlaceHolderDialogBodyHeaderSection" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="PlaceHolderDialogDescription" runat="server">
  <div id="selectTermDescription" class="none-wordbreak">
    <table class="ms-dialogHeaderDescription">
      <tbody>
        <tr>
          <td><asp:Literal ID="PageDescription" runat="server" Text="Take register" EnableViewState="false" /></td>
        </tr>
      </tbody>
    </table>
  </div>
</asp:Content>

<asp:Content ID="Content8" ContentPlaceHolderID="PlaceHolderHelpLink" runat="server">
  <!-- Remove the default help link -->
</asp:Content>
<asp:Content ID="Content6" ContentPlaceHolderID="PlaceHolderDialogBodyMainSection"     runat="server">
    <slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
    <asp:Panel ID="contentPanel" runat="server">
            <asp:Label ID="headerMessage" runat="server"></asp:Label><br /><br />
            <asp:Panel ID="FilePanel" runat="server">
            </asp:Panel>
            <asp:HyperLink ID="instructorLink" runat="server" Style="display: none"></asp:HyperLink>
    </asp:Panel>
</asp:Content>


<asp:Content ID="ContentButton" ContentPlaceHolderID="PlaceHolderAdditionalPreButton" runat="server">
</asp:Content>
