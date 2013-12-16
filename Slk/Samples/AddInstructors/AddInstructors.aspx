<%-- MICROSOFT PROVIDES SAMPLE CODE AS IS AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER. MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. --%>

<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

// AddInstructors.aspx
//
// This file is part of the SharePoint Learning Kit "AddInstructors" sample code.  See ReadMe.txt
// and AddInstructors.aspx.cs for more information.
//      
// This sample code is located in Samples\SLK\AddInstructors within SLK-SDK-n.n.nnn-ENU.zip.
//

<%@ Assembly Name="Microsoft.SharePoint.ApplicationPages, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"%>
<%@ Assembly Name="AddInstructorsSample, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fac4eacf658b669a"%>
<%@ Page Language="C#" Inherits="AddInstructorsSample.AddInstructorsPage" MasterPageFile="~/_layouts/application.master"      %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="wssuc" TagName="ButtonSection" src="~/_controltemplates/ButtonSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register Tagprefix="wssawc" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<asp:content contentplaceholderid="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>
<asp:Content contentplaceholderid="PlaceHolderPageTitle" runat="server">
Add Instructors to Assignments
</asp:Content>
<asp:content contentplaceholderid="PlaceHolderPageTitleInTitleArea" runat="server">
Add Instructors to Assignments
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
This SharePoint Learning Kit sample page demonstrates how to add instructors to SLK assignments for which a given user is already an instructor.
This is particularly useful if that user left the organization, and one or more different users need to be made instructors on the departed user's
assignments.
</asp:Content>
<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">
<table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet">
	<asp:Panel id="SuccessPanel" Visible="false" runat="server">
		<p><br/><b><asp:Label id="SuccessLabel" runat="server" /></b></p>
		<p><a href="?">Add instructors to other assignments</a></p>
	</asp:Panel>
	<wssuc:InputFormSection id="OriginalInstructorSection" Title="Select Original Instructor" runat="server">
		<Template_Description>
			All assignments for which this user is an instructor will be retrieved.
			This list contains all users that are instructors of at least one assignment
			within this SharePoint site collection.
		</Template_Description>
		<Template_InputFormControls>
			<wssuc:InputFormControl LabelText="Original Instructor:" runat="server">
				<Template_Control>
					<asp:DropDownList id="OriginalInstructor" runat="server" />
				</Template_Control>
			</wssuc:InputFormControl>
		</Template_InputFormControls>
	</wssuc:InputFormSection>
	<wssuc:InputFormSection id="NewInstructorsSection" Title="Enter Additional Instructors" runat="server">
		<Template_Description>
			These users will be added as instructors of the retrieved assignments.  These users must
			already be members of "All People" in this SharePoint site collection.
		</Template_Description>
		<Template_InputFormControls>
			<wssuc:InputFormControl LabelText="New Instructors:" runat="server">
				<Template_Control>
					<wssawc:PeopleEditor AllowEmpty=false ValidatorEnabled="true" id="NewInstructors" runat="server"
								ShowCreateButtonInActiveDirectoryAccountCreationMode="true"
								SelectionSet="User,SecGroup,SPGroup"/>
				</Template_Control>
			</wssuc:InputFormControl>
		</Template_InputFormControls>
	</wssuc:InputFormSection>
	<wssuc:ButtonSection id="ButtonSection" runat="server">
		<Template_Buttons>
			<asp:PlaceHolder runat="server">
				<asp:Button UseSubmitBehavior="false" class="ms-ButtonHeightWidth" Text="OK" id="BtnOk" OnClick="BtnOk_Click" runat="server" />
			</asp:PlaceHolder>
		</Template_Buttons>
	</wssuc:ButtonSection>
</table>
</asp:Content>


