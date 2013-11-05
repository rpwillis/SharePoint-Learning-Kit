<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.FilesUploadPage" MasterPageFile="~/_layouts/application.master" AutoEventWireup ="true" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<asp:Label runat="server" ID="pageTitleInTitlePage" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td>
                <slk:ErrorBanner ID="errorBanner" Visible="false" EnableViewState="false" runat="server" />
                <asp:Panel ID="pnlOldFiles" runat="server" CssClass="ms-descriptiontext"></asp:Panel>
                <br /><br />
            </td>
        </tr>
        <tr>
            <td>
                <table class="ms-propertysheet" cellspacing="0" cellpadding="0" width="100%" border="0">
                    <tr>
                        <td colspan="2">
                            <table border="1" cellpadding="0" cellspacing="0" frame="below" width="100%">
                                <tr>
                                    <td></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td class="ms-descriptiontext" valign="top">
                            <table cellspacing="0" cellpadding="1" border="0" width="100%">
                                <tr>
                                    <td class="ms-sectionheader" height="22" valign="top" style="padding-top: 4px;">
                                        <h3 class="ms-standardheader">
                                            <asp:Label runat="server" ID="documentUpload" />
                                        </h3>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="ms-descriptiontext ms-inputformdescription">
                                        <asp:Label runat="server" ID="documentUploadDescription" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td class="ms-authoringcontrols ms-inputformcontrols" align="left" valign="top">
                            <table cellspacing="0" cellpadding="0" border="0" width="100%">
                                <tr>
                                    <td class="ms-authoringcontrols">
                                        <table class="ms-authoringcontrols" cellspacing="0" cellpadding="0" border="0" width="100%">
                                            <td class="ms-authoringcontrols">
                                                <asp:Label ID="name" runat="server" ></asp:Label>
                                                <br />
                                            </td>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="ms-authoringcontrols" width="99%">
                                        <table class="ms-authoringcontrols" width="100%">
                                            <tr>
                                                <td>
                                                    <input id="uploadFile1" type="file"  runat ="server" class="ms-fileinput" size="35" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <input id="uploadFile2" type="file"  runat ="server" class="ms-fileinput" size="35" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <input id="uploadFile3" type="file"  runat ="server" class="ms-fileinput" size="35" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <input id="uploadFile4" type="file"  runat ="server" class="ms-fileinput" size="35" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <input id="uploadFile5" type="file"  runat ="server" class="ms-fileinput" size="35" />
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <table border="1" cellpadding="0" cellspacing="0" frame="above" width="100%">
                                <tr>
                                    <td></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <table cellspacing="0" cellpadding="0" width="100%">
                                <colgroup>
                                    <col width="99%"/>
                                    <col width="1%"/>
                                </colgroup>
                                <tr>
                                    <td>   </td>
                                    <td nowrap="">
                                        <br />
                                        <asp:Button ID = "btnOK" runat ="server" Text = 'OK' OnClick = "btnOK_Click" Width="80px" Height="20px" Font-Size="11px" />
                                        <asp:Button ID = "btnCancel" runat ="server" Text='Cancel' OnClick = "btnCancel_Click" Width="80px" Height="20px" Font-Size="11px" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="ms-descriptiontext ms-inputformdescription">
                            <asp:Label ID="lblMessage" runat="server"></asp:Label>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
