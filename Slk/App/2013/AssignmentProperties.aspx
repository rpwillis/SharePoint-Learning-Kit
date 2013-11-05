<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Page Language="C#" Inherits="Microsoft.SharePointLearningKit.ApplicationPages.AssignmentPropertiesPage" DynamicMasterPageFile="~masterurl/default.master" ValidateRequest="False" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="slk" Namespace="Microsoft.SharePointLearningKit.WebControls" Assembly="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>

<asp:Content ID="titleContent" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
   <asp:Literal runat="server" ID="pageTitle" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
    <asp:Literal runat="server" ID="pageTitleinTitlePage" />
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
  <%// script and styles go here %>
  <link rel="stylesheet" type="text/css" href="Include/Styles.css"/>

<style type="text/css">
	.ms-titlearearight .ms-areaseparatorright img{
		display:none !important;
	}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderPageDescription" runat="server">
    <asp:Literal ID="pageDescription" runat="server"></asp:Literal>
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <!--// this table contains two columns: the first holds the actual content, the second contains a
  // blank GIF whose purpose is to impose a minimum width on the form -->
    <table cellpadding="0" cellspacing="0">
        <tr>
            <td valign="top">
                <table cellpadding="0" cellspacing="0" width="100%">
                    <tr>
                        <td width="615">
                            <table cellpadding="0" cellspacing="0" width="100%">
                                <tr>
                                    <td width="100%">
                                        <!-- Error Banner -->
                                        <slk:ErrorBanner ID="errorBanner" runat="server"></slk:ErrorBanner>
                                    </td>
                                </tr>
                                <tr><td></td></tr>
                                <!-- Validation Summary  -->
                                <tr>
                                    <td class="ms-formbody" >
                                        <asp:ValidationSummary CssClass="SlkError" ID="appValidationSummary" runat="server" />
                                    </td>
                                </tr>
                            </table>
                            <!-- Empty line  -->
                          
                            <slk:TableGrid ID="tgEmptyLine" runat="server"  Width="100%" CellPadding="0" CellSpacing="0">
                                <slk:TableGridRow>
                                    <slk:TableGridColumn  ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server"></slk:TableGridColumn>
                                </slk:TableGridRow>                                    
                            </slk:TableGrid>    
                                                       
                            <asp:Panel ID="panelAssignmentProperties" runat="server" Width="100%" >
                                <!-- upper OK/Cancel buttons -->
                                <slk:SimpleButtonToolbar runat="server" ID="simpleToolbarTop">
                                    <asp:Button ID="btnTopOK" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="SubmitAssignment" />
                                </slk:SimpleButtonToolbar>
                                <!-- Empty line  -->
                               
                                <slk:TableGrid ID="tgEmptyTop" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server"></slk:TableGridColumn>
                                    </slk:TableGridRow> 
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="SectionLine" runat="server">                                                    
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>                                                                       
                                </slk:TableGrid>                                
                                
                                <!--Main Form -->
                                <slk:TableGrid ID="tgAssignmentProp" runat="server" Width="100%" CellPadding="0"
                                    CellSpacing="0" Style="border-top: none">
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormLine" Width="100%" runat="server" Style="border-top: none">                            
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn  ColumnSpan="2" runat="server" Style="border-top: none" Width="100%">
                                            <asp:Label ID="lblAssignmentPropHeader" runat="server" CssClass ="UserGenericHeader" EnableViewState="false"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormDefault" runat="server" Width="100%" CssClass="UserGenericText" Style="border-top: none">
                                          <asp:Label ID="lblAssignmentPropText" runat="server" EnableViewState="false"></asp:Label>                 
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server"></slk:TableGridColumn>
                                    </slk:TableGridRow>
                                </slk:TableGrid>
                                
                                <!--Main Form Content -->
                                <slk:TableGrid ID="tgMainContent" runat="server" CssClass="ms-formtable" Style="margin-top: 8px"
                                    CellSpacing="0" CellPadding="0" Width="100%">
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblTitle" runat="server" EnableViewState="false" ></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                              <asp:TextBox ID="txtTitle" runat="server" CssClass="ms-long" TextMode="MultiLine" style="overflow:visible; height:40px; width:98%"></asp:TextBox>                                                      
                                              <div>
                                                <asp:RequiredFieldValidator ID="rfvAppTitle" runat="server" ></asp:RequiredFieldValidator> 
                                                <asp:RegularExpressionValidator ID="regexAppTitle" runat="server" ></asp:RegularExpressionValidator>
                                              </div>                                             
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn runat="server" ColumnType="FormLabel">
                                             <h3 class="ms-standardheader"><nobr><asp:Label ID="lblDescription" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                              <asp:TextBox ID="txtDescription" runat="server" CssClass="ms-long"  TextMode="MultiLine" Height="60" style="width:98%"></asp:TextBox>                                               
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn runat="server" ColumnType="FormLabel">
                                             <h3 class="ms-standardheader"><nobr><asp:Label ID="lblPoints" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                            <asp:TextBox ID="txtPoints" runat="server" CssClass="ms-input"></asp:TextBox>&nbsp;<asp:Label ID="lblPointsPossible" runat="server" EnableViewState="false"></asp:Label>                                              
                                            <div>
                                                <asp:RangeValidator ID="rvPointsPossible" runat="server"></asp:RangeValidator>
                                            </div>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblStart"  runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                            <SharePoint:DateTimeControl ID="spDateTimeStart" runat="server"></SharePoint:DateTimeControl>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                             <h3 class="ms-standardheader"><nobr><asp:Label ID="lblDue" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                             <SharePoint:DateTimeControl ID="spDateTimeDue" runat="server"></SharePoint:DateTimeControl>                                             
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">                                
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                            <table border="0" width="100%"  cellspacing="0" cellpadding="0">
                                                <tr>
                                                  <td width="0%" valign="top">  
                                                     <asp:CheckBox ID="checkboxEmail" runat="server"></asp:CheckBox>
                                                   </td>
                                                   <td class="ms-formbody" style="border-top: none; padding-top:1px" width="100%" valign="top">
                                                      <asp:Label ID="labelEmail"  runat="server" AssociatedControlID="checkboxEmail" EnableViewState="false"></asp:Label>  
                                                   </td>
                                                </tr>
                                                <tr>
                                                  <td width="0%" valign="top">  
                                                     <asp:CheckBox ID="chkAutoReturnLearners" runat="server"></asp:CheckBox>
                                                   </td>
                                                   <td class="ms-formbody" style="border-top: none; padding-top:1px" width="100%" valign="top">
                                                      <asp:Label ID="lblAutoReturnLearners"  runat="server" AssociatedControlID="chkAutoReturnLearners" EnableViewState="false"></asp:Label>  
                                                   </td>
                                                </tr>
                                                <tr>
                                                  <td width="0%" valign="top">   
                                                     <asp:CheckBox ID="chkShowAnswersToLearners" runat="server" ></asp:CheckBox>                                                       
                                                   </td>
                                                  <td class="ms-formbody" style="border-top: none; padding-top:1px" valign="top" width="100%" >
                                                      <asp:Label ID="lblShowAnswersToLearners"  runat="server" AssociatedControlID="chkShowAnswersToLearners" EnableViewState="false"></asp:Label> 
                                                  </td>
                                                </tr>
                                            </table>              
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
									<slk:TableGridRow>
										<slk:TableGridColumn ColumnSpan="2" ColumnType="FormLine"><img height=1 width=1 alt="" src="/_layouts/SharePointLearningKit/Images/Blank.gif"></slk:TableGridColumn>
									</slk:TableGridRow>
                                </slk:TableGrid>

                                <slk:TableGrid ID="TgUploadDocument" runat="server" Width="100%" CellPadding="0" CellSpacing="0" Visible="false">
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormLabel" runat="server" Width="100%" Style="border-top: none">
                                            <asp:Label ID="LabelUploadDocumentHeader" runat="server" CssClass="UserGenericHeader" EnableViewState="false"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormDefault" runat="server" Width="100%" CssClass="UserGenericText" Style="border-top: none">
                                            <asp:Label ID="LabelUploadDocumentText" runat="server" EnableViewState="false"></asp:Label>                 
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                     <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server"></slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="LabelUploadDocumentName" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                            <asp:FileUpload id="FileUploadDocument" runat="server" class="ms-fileinput"></asp:FileUpload>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="LabelUploadDocumentLibrary" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                            <asp:DropDownList ID="UploadDocumentLibraries" CssClass="ms-long" runat="server" />
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>

                                </slk:TableGrid>
                                                                
                                <!-- Distribute the Assignment -->
                                <slk:TableGrid ID="tgDistAssignment" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server">&nbsp;</slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormLabel" runat="server" Width="100%" Style="border-top: none">
                                                <asp:Label ID="lblDistributeAssignmentHeader" runat="server" CssClass="UserGenericHeader" EnableViewState="false"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormDefault" runat="server" Width="100%" CssClass="UserGenericText" Style="border-top: none">
                                             <asp:Label ID="lblDistributeAssignmentText" runat="server" EnableViewState="false"></asp:Label>                 
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                     <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormBreak" runat="server"></slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow runat="server">
                                        <slk:TableGridColumn runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblSharePointSite" runat="server" EnableViewState="false"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                            <asp:HyperLink ID="lnkSharePointSite" runat="server"></asp:HyperLink>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                             <h3 class="ms-standardheader"><nobr><asp:Label ID="lblInstructorsHeader"  runat="server" EnableViewState="false"></asp:Label></nobr></h3>                                
                                             <table cellpadding="0" cellspacing="0"><tr><td class="ms-propertysheet"><asp:Label ID="lblInstructorsText" runat="server" EnableViewState="false"></asp:Label></td></tr></table> 
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">
                                           <slk:CustomCheckBoxList ID="chkListInstructors" runat="server"></slk:CustomCheckBoxList>    
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow  runat="server">
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <asp:Label ID="lblLearnersHeader" runat="server" EnableViewState="false"></asp:Label> 
                                            <table cellpadding="0" cellspacing="0"><tr><td class="ms-propertysheet"><asp:Label ID="lblLearnersText"  runat="server" EnableViewState="false"></asp:Label></td></tr></table>                                                           
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">                                                                    
                                            <table class="UserGenericText" cellspacing="0" cellpadding="0" border="0" style="border-collapse:collapse;" width="100%">
                                                <tr>
                                                  <td class="ms-formbodysurvey" style="padding: 0" width="50%" valign="top">                                                     
                                                       <slk:CustomCheckBoxList ID="chkListGroups" runat="server"></slk:CustomCheckBoxList>
                                                   </td>
                                                  <td class="ms-formbodysurvey" style="padding: 0" width="50%" valign="top">                                                                                                    
                                                       <slk:CustomCheckBoxList ID="chkListLearners" runat="server"></slk:CustomCheckBoxList>
                                                  </td>
                                                </tr>
                                            </table>   
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                </slk:TableGrid>                                
                                <!--End of Main Content -->
                                <!-- Empty line  -->
                                <slk:TableGrid ID="tgEmptyBottom" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormLine" runat="server"></slk:TableGridColumn>                                      
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormLine" runat="server" Style="border-top: none"><img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="1px" height="6px" alt="" /></slk:TableGridColumn>                                                                            
                                    </slk:TableGridRow>                                    
                                </slk:TableGrid>
                                <slk:SimpleButtonToolbar runat="server" ID="simpleToolbarBottom" >
                                    <asp:Button ID="btnBottomOK" CssClass="ms-ButtonHeightWidth" runat="server" OnClick="SubmitAssignment"  />
                                </slk:SimpleButtonToolbar>
                                
                                <!-- end of single-cell tables surrounding form -->                                
                            </asp:Panel>
                        </td>
                    </tr>
                    
                    <tr>
                        <td>
                              <!--Confirmation Page Form -->
                            <asp:Panel ID="panelConfirmation" runat="server" Width="80%">
                                                        
                                  <slk:TableGrid ID="tgConfirmAssignment" runat="server"  CellPadding="0" CellSpacing="0" Style="border-top: none" Width="100%">
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormBreak" runat="server" Width="100%">            
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                     <slk:TableGridRow>
                                        <slk:TableGridColumn  ColumnSpan="2" ColumnType="SectionLine" runat="server" Width="100%">            
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormBreak" Width="100%" runat="server" Style="border-top: none">                            
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnType="FormDefault" ColumnSpan="2" runat="server" Style="border-top: none" Width="100%">
                                            <asp:Label ID="lblAssignmentTitle" runat="server" CssClass ="UserGenericHeader"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" ColumnType="FormDefault" runat="server" CssClass="UserGenericText" Style="border-top: none" Width="100%">
                                            <asp:Label ID="lblAssignmentDescription" runat="server"></asp:Label> 
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn  ColumnSpan="2" ColumnType="FormBreak"  runat="server" Width="100%">&nbsp;</slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow >
                                        <slk:TableGridColumn  runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblAssignmentPoints" runat="server"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn  runat="server" ColumnType="FormBody">                                
                                             <asp:Label ID="lblAssignmentPointsText" runat="server" CssClass="UserGenericText"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblAssignmentStart"  runat="server"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                            <asp:Label ID="lblAssignmentStartText" runat="server" CssClass="UserGenericText"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn runat="server" ColumnType="FormLabel">
                                            <h3 class="ms-standardheader"><nobr><asp:Label ID="lblAssignmentDue" runat="server"></asp:Label></nobr></h3>
                                        </slk:TableGridColumn>
                                        <slk:TableGridColumn runat="server" ColumnType="FormBody">
                                            <asp:Label ID="lblAssignmentDueText" runat="server" CssClass="UserGenericText"></asp:Label>     
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="SectionLine"  runat="server"></slk:TableGridColumn>                                      
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>                                         
                                        <slk:TableGridColumn  ColumnSpan="2" ColumnType="FormBreak" runat="server" Width="100%">&nbsp;</slk:TableGridColumn>
                                    </slk:TableGridRow>
                                     <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" runat="server" Style="border-top: none" Width="100%">
                                            <asp:Label ID="lblAPPConfirmWhatNext" runat="server" CssClass ="UserGenericHeader"></asp:Label>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>                                    
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn  ColumnSpan="2" ColumnType="FormDefault" runat="server" Width="100%">
                                            <asp:BulletedList ID="lstNavigateBulletedList" runat="server" CssClass="UserGenericText" ></asp:BulletedList>
                                        </slk:TableGridColumn>
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn  ColumnSpan="2" ColumnType="FormBreak" runat="server" Width="100%">            
                                        </slk:TableGridColumn>                                        
                                    </slk:TableGridRow>
                                    <slk:TableGridRow>
                                        <slk:TableGridColumn ColumnSpan="2" Width="100%" ColumnType="FormLine" runat="server"></slk:TableGridColumn>                                      
                                    </slk:TableGridRow>
                                </slk:TableGrid>
                            </asp:Panel>
                        </td>
                    </tr>
                </table>               
                <!--// end of the table whose purpose is to impose a minimum width on the form -->
                <img src="/_layouts/SharePointLearningKit/Images/Blank.gif" width="590" height="1" alt="" />
            </td>
        </tr>
    </table>
    <!-- end -->
    <!--END_CONTENT-->
    
</asp:Content>
