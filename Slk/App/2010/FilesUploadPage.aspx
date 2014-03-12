<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.FilesUploadPage" DynamicMasterPageFile="~masterurl/default.master" AutoEventWireup ="true" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
	<asp:Literal runat="server" ID="pageTitle" EnableViewState="false" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
	<asp:Label runat="server" ID="pageTitleInTitlePage" />
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">

<script type="text/javascript">
		
    var counter = 0;

	function createUploadInput(currentInput){

            counter++;
            var container = document.getElementById("uploadContainer");	

            var div = document.createElement("div");
            container.appendChild(div);			

            var fileInput = document.createElement("input");
            fileInput.type = "file";			
            fileInput.id = 'uploadFile' + counter;
            fileInput.name = fileInput.id
            fileInput.className = 'ms-fileinput'
            fileInput.onchange = function() {createUploadInput(this);}
            div.appendChild(fileInput);			

            var clearButton = document.createElement("input");
            clearButton.type = 'image';
            clearButton.src = '/_layouts/images/DelItem.gif';
            clearButton.onclick = function() {
            removeInput(currentInput.id);
            return false;
            };
            currentInput.parentNode.appendChild(clearButton);			
        }

    function removeInput(id) {
        var element = document.getElementById(id);
        if (element != null)
        {
            element.parentNode.parentNode.removeChild(element.parentNode);
        }
    }
</script>
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
<SharePoint:DelegateControl ControlId="SlkStartContent" runat="server"/>
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
                                    <td id="uploadContainer" class="ms-authoringcontrols" width="99%">
    <div>
	<input type="file" id="uploadFile0" name="uploadFile0" class="ms-fileinput" onchange="javascript:createUploadInput(this)" />
    </div>

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
                                        <asp:Button ID = "btnOK" runat ="server" CssClass="ms-ButtonHeightWidth" Text = 'OK' OnClick = "btnOK_Click" />
                                        <asp:Button ID = "btnCancel" runat ="server" CssClass="ms-ButtonHeightWidth" Text='Cancel' OnClick = "btnCancel_Click" />
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
<SharePoint:DelegateControl ControlId="SlkEndContent" runat="server"/>
</asp:Content>
