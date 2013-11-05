<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.DownloadDialog"
    MasterPageFile="~/_layouts/application.master" ValidateRequest="False" %>

<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls"
    Assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<asp:Content ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    <asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
    <asp:Literal runat="server" ID="pageTitleInTitlePage" EnableViewState="false" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
    <asp:Literal ID="pageDescription" runat="server" EnableViewState="false" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderMain" runat="server">
    
</asp:Content>

