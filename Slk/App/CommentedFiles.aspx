<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.CommentedFiles"
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
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td>
                <slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
                <asp:Panel ID="contentPanel" runat="server">
                    <slk:TableGrid ID="TableGrid1" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
                        <slk:TableGridRow>
                            <slk:TableGridColumn BorderStyle="None" ColumnType="FormBody">
                                <asp:Label ID="headerMessage" runat="server"></asp:Label><br /><br />
                      <asp:FileUpload id="commentedFileUpload" runat="server" class="ms-fileinput" size="35"></asp:FileUpload>
                        <br />
                      <asp:Button id="UploadButton" Text="Upload file" CssClass="ms-ButtonHeightWidth" OnClick="UploadButton_Click"   runat="server"> </asp:Button>    
                      <br /> <br />
                       <asp:Label id="UploadStatusLabel" runat="server" ForeColor="Green"></asp:Label>
                       <asp:Label id="UploadErrorStatusLabel" runat="server" ForeColor="Red"></asp:Label>
                            </slk:TableGridColumn>
                        </slk:TableGridRow>
                    </slk:TableGrid>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>

