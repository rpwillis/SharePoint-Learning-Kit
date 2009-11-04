<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.SubmittedFiles" ValidateRequest="False" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="..\1033\STYLES\CORE.CSS" rel="stylesheet" type="text/css" />
</head>
<body>
    <table height="100%" cellspacing="0" cellpadding="0" width="100%">
        <tr>
            <td class="ms-areaseparator" width="100%" valign="top">
                <table>
                    <tr>
                        <td class="ms-pagetitle">
                            <h2 class="ms-pagetitle">
                                <asp:Literal ID="pageTitle" runat="server" EnableViewState="false" />
                            </h2>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                <table>
                    <tr valign="top">
                        <td class="ms-descriptiontext">
                            <asp:Literal ID="pageDescription" runat="server" EnableViewState="false" />
                            <br /><br />
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" height="100%">
                            <table cellpadding="0" cellspacing="0" class="ms-propertysheet" style="font-size : medium">
                                <tr>
                                    <td>
                                        <slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
                                        <asp:Panel ID="contentPanel" runat="server">
                                            <slk:TableGrid ID="TableGrid1" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
                                                <slk:TableGridRow>
                                                    <slk:TableGridColumn BorderStyle="None" ColumnType="FormBody">
                                                        <asp:Label ID="headerMessage" runat="server"></asp:Label><br /><br />
                                                        <asp:HyperLink ID="file1" runat="server" Style="display: none"></asp:HyperLink><br />
                                                        <asp:HyperLink ID="file2" runat="server" Style="display: none"></asp:HyperLink><br />
                                                        <asp:HyperLink ID="file3" runat="server" Style="display: none"></asp:HyperLink><br />
                                                        <asp:HyperLink ID="file4" runat="server" Style="display: none"></asp:HyperLink><br />
                                                        <asp:HyperLink ID="file5" runat="server" Style="display: none"></asp:HyperLink><br /><br /><br />
                                                        <asp:Label ID="instructorMessage" runat="server" Style="display: none"></asp:Label>
                                                        <asp:HyperLink ID="instructorLink" runat="server" Style="display: none"></asp:HyperLink>
                                                    </slk:TableGridColumn>
                                                </slk:TableGridRow>
                                            </slk:TableGrid>
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
   </table>
</body>
</html>

    