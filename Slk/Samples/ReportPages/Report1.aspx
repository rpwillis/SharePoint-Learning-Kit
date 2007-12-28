<%-- MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. --%>

<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%-- Report1.aspx
  --
  -- This is SharePoint Learning Kit sample code that executes as an ASP.NET Web page.  ReadMe.txt
  -- contains instructions for installing this Web page into an existing SharePoint Learning Kit
  -- installation.  Visual Studio is not required.
  --
  -- This sample code is located in Samples\SLK\ReportPages within SLK-SDK-n.n.nnn-ENU.zip.
  --
  -- This Web page demonstrates how to create a report page, based on a SharePoint master page (so
  -- that it includes common SharePoint user interface elements), which displays a list of all
  -- fully graded assignments for which the current user is an instructor.  Clicking an assignment
  -- title opens the Grading page for that assignment.
  --%>

<%-- NOTE: If this page displays "File Not Found", check the build number (1.0.nnn.0) below --%>
<%@ Assembly Name="Microsoft.LearningComponents.Storage, Version=%VERSION4%, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.0.2, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" MasterPageFile="~/_layouts/application.master" ValidateRequest="False" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Microsoft.SharePoint" %>
<%@ Import namespace="Microsoft.SharePoint.WebControls" %>
<%@ Import namespace="Microsoft.SharePointLearningKit" %>
<%@ Import namespace="Microsoft.SharePointLearningKit.Schema" %>
<%@ Import namespace="Microsoft.LearningComponents.Storage" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
    Sample Report 1 - Graded Assignment List
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
    Sample Report 1 - Graded Assignment List
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
    This is a SharePoint Learning Kit sample report.
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">

<table style="font-size: 8.5pt; border-collapse: collapse">
    <col style="padding-right: 8pt">
    <col style="padding-right: 8pt">
    <col style="padding-right: 8pt">
    <col style="padding-right: 8pt">
    <col style="padding-right: 8pt; text-align: right">
    <col style="padding-right: 8pt; text-align: right">
    <col style="padding-left: 4pt; padding-right: 4pt">
    <col style="padding-right: 4pt">
    <tr style="text-decoration: underline">
        <td>ID</t>
        <td>Assignment Title</t>
        <td>Due Date/Time</t>
        <td>Created By</t>
        <td>Learners</t>
        <td colspan="3">Average Score</t>
    </tr>
<%
    // get a reference to the SLK store for the current user
    SlkStore slkStore = SlkStore.GetStore(SPControl.GetContextWeb(HttpContext.Current));

    // query SLK for all assignments for which the current user is an instructor (that filtering
    // is done automatically by AssignmentGradingView) and which are fully graded (i.e. all
    // learner assignments of each assignment are in the "Final" state)
    LearningStoreQuery query = slkStore.LearningStore.CreateQuery(AssignmentListForInstructors.ViewName);
    query.AddColumn(AssignmentListForInstructors.AssignmentId);
    query.AddColumn(AssignmentListForInstructors.AssignmentTitle);
    query.AddColumn(AssignmentListForInstructors.AssignmentDueDate);
    query.AddColumn(AssignmentListForInstructors.AssignmentCreatedByName);
    query.AddColumn(AssignmentListForInstructors.CountTotal);
    query.AddColumn(AssignmentListForInstructors.AvgFinalPoints);
    query.AddColumn(AssignmentListForInstructors.AssignmentPointsPossible);
    query.AddCondition(AssignmentListForInstructors.CountNotFinal,
        LearningStoreConditionOperator.Equal, 0);
    query.AddSort(AssignmentListForInstructors.AssignmentTitle, LearningStoreSortDirection.Ascending);
    LearningStoreJob job = slkStore.LearningStore.CreateJob();
    job.PerformQuery(query);
    DataRowCollection dataRows = job.Execute<DataTable>().Rows;

    // display information about each assignment
    foreach (DataRow dataRow in dataRows)
    {
        AssignmentItemIdentifier id = new AssignmentItemIdentifier(
            (LearningStoreItemIdentifier)dataRow[AssignmentListForInstructors.AssignmentId]);
        string title = (string)dataRow[AssignmentListForInstructors.AssignmentTitle];
        object value = dataRow[AssignmentListForInstructors.AssignmentDueDate];
        DateTime? dueDate = (value is DateTime) ? ((DateTime?) value).Value.ToLocalTime() : (DateTime?) null;
        string createdBy = (string)dataRow[AssignmentListForInstructors.AssignmentCreatedByName];
        int learnerCount = (int)dataRow[AssignmentListForInstructors.CountTotal];
        value = dataRow[AssignmentListForInstructors.AvgFinalPoints];
        double? averageScore = (value is double) ? (double?) value : (double?) null;
        value = dataRow[AssignmentListForInstructors.AssignmentPointsPossible];
        float? pointsPossible = (value is float) ? (float?) value : (float?) null;
%>
    <tr>
        <td><%=id.GetKey()%></td>
        <td><a href="Grading.aspx?AssignmentId=<%=id.GetKey()%>"><%=Server.HtmlEncode(title)%></a></td>
        <td><%=(dueDate == null) ? "" : String.Format("{0:d} at {0:t}", dueDate)%></td>
        <td><%=Server.HtmlEncode(createdBy)%></td>
        <td style="padding-right: 16pt"><%=learnerCount%></td>
        <td><%=(averageScore == null) ? "--" : String.Format("{0:n0}", averageScore)%></td>
        <td>/</td>
        <td><%=(pointsPossible == null) ? "--" : String.Format("{0:n0}", pointsPossible)%></td>
    </tr>
<%
    }
%>

</table>

</asp:Content>

