<%-- MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. --%>

<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%-- Report2.aspx
  --
  -- This is SharePoint Learning Kit sample code that executes as an ASP.NET Web page.  ReadMe.txt
  -- contains instructions for installing this Web page into an existing SharePoint Learning Kit
  -- installation.  Visual Studio is not required.
  --
  -- This sample code is located in Samples\SLK\ReportPages within SLK-SDK-n.n.nnn-ENU.zip.
  --
  -- This Web page demonstrates how to create a report page that's not based a SharePoint master
  -- page which displays a grid of "final score" values for each graded learner assignment for
  -- which the current user is an instructor.  Clicking an assignment title opens the Grading page
  -- for that assignment.
  --%>

<%-- NOTE: If this page displays "File Not Found", check the build number (1.0.nnn.0) below --%>
<%@ Assembly Name="Microsoft.LearningComponents.Storage, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.3.1.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" ValidateRequest="False" %>
<%@ Import namespace="System.Collections.Generic" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Microsoft.SharePoint" %>
<%@ Import namespace="Microsoft.SharePoint.WebControls" %>
<%@ Import namespace="Microsoft.SharePointLearningKit" %>
<%@ Import namespace="Microsoft.SharePointLearningKit.Schema" %>
<%@ Import namespace="Microsoft.LearningComponents.Storage" %>

<html>

<head>

<title>Sample Report 2 - Graded Learner Assignment Grid</title>

<style>
body
{
    font: 8.5pt Verdana;
    color: black;
    background-color: white;
}
a
{
    color: #3966BF;
    text-decoration: none;
}
a:hover
{
    color: black;
    text-decoration: underline;
}
</style>

</head>

<body>

<%
    // get a reference to the SLK store for the current user
    SlkStore slkStore = SlkStore.GetStore(SPControl.GetContextWeb(HttpContext.Current));

    // query SLK for all learner assignments that are in the "Final" state
    LearningStoreQuery query = slkStore.LearningStore.CreateQuery(
        LearnerAssignmentListForInstructors.ViewName);
    query.AddColumn(LearnerAssignmentListForInstructors.AssignmentId);
    query.AddColumn(LearnerAssignmentListForInstructors.AssignmentTitle);
    query.AddColumn(LearnerAssignmentListForInstructors.LearnerId);
    query.AddColumn(LearnerAssignmentListForInstructors.LearnerName);
    query.AddColumn(LearnerAssignmentListForInstructors.FinalPoints);
    query.AddCondition(LearnerAssignmentListForInstructors.LearnerAssignmentState,
        LearningStoreConditionOperator.Equal, LearnerAssignmentState.Final);
    LearningStoreJob job = slkStore.LearningStore.CreateJob();
    job.PerformQuery(query);
    DataRowCollection dataRows = job.Execute<DataTable>().Rows;

    // <learnerMap> will map the LearningStore ID of each learner to a Learner object (defined
    // below) containing their ID and name
    Dictionary<UserItemIdentifier, Learner> learnerMap =
        new Dictionary<UserItemIdentifier, Learner>(100);

    // <assignmentMap> will map the LearningStore ID of each assignment to an Assignment object
    // (defined below) containing its ID and name
    Dictionary<AssignmentItemIdentifier, Assignment> assignmentMap =
        new Dictionary<AssignmentItemIdentifier, Assignment>(1000);

    // <finalPointsMap> will map the LearningStore learner ID and assignment ID to the final score
    // that that learner received on that assignment
    Dictionary<LearnerAssignmentLocator, float?> finalPointsMap =
        new Dictionary<LearnerAssignmentLocator, float?>(dataRows.Count);

    // iterate through the learner assignments returned from the database query, and populate
    // <learnerMap>, <assignmentMap>, and <learnerAssignmentMap>
    foreach (DataRow dataRow in dataRows)
    {
        // get information about this learner assignment
        Assignment assignment = new Assignment();
        assignment.Id = new AssignmentItemIdentifier(
            (LearningStoreItemIdentifier)dataRow[LearnerAssignmentListForInstructors.AssignmentId]);
        assignment.Title = (string)dataRow[LearnerAssignmentListForInstructors.AssignmentTitle];
        Learner learner = new Learner();
        learner.Id = new UserItemIdentifier(
            (LearningStoreItemIdentifier)dataRow[LearnerAssignmentListForInstructors.LearnerId]);
        learner.Name = (string)dataRow[LearnerAssignmentListForInstructors.LearnerName];
        object value = dataRow[LearnerAssignmentListForInstructors.FinalPoints];
        float? finalPoints = (value is float) ? (float?) value : (float?) null;

        // update <learnerMap>
        learnerMap[learner.Id] = learner;

        // update <assignmentMap>
        assignmentMap[assignment.Id] = assignment;

        // update <finalPointsMap>
        LearnerAssignmentLocator locator = new LearnerAssignmentLocator();
        locator.LearnerId = learner.Id;
        locator.AssignmentId = assignment.Id;
        finalPointsMap[locator] = finalPoints;
    }

    // sort the learners by name; store the result in <learners>
    Learner[] learners = new Learner[learnerMap.Values.Count];
    learnerMap.Values.CopyTo(learners, 0);
    Array.Sort(learners);

    // sort the assignments by title; store the result in <assignments>
    Assignment[] assignments = new Assignment[assignmentMap.Values.Count];
    assignmentMap.Values.CopyTo(assignments, 0);
    Array.Sort(assignments);
%>

<table style="font-size: 8.5pt; border-collapse: collapse">

<%-- write <col> elements to specify column styles --%>
    <col style="padding-right: 8pt">
<% for (int i = 0; i < learners.Length; i++) { %>
    <col style="padding-right: 4pt">
<% } %>

<%-- write table header --%>
    <tr style="text-decoration: underline">
        <td>ID</t>
        <td>Assignment Title</t>
<% foreach (Learner learner in learners) { %>
        <td title="<%=Server.HtmlEncode(learner.Name)%>"><%=Server.HtmlEncode(learner.GetInitials())%></td>
<% } %>
    </tr>

<%-- write table body --%>
<% foreach (Assignment assignment in assignments) { %>
    <tr onmouseover="this.style.backgroundColor = '#EBF3FF'" onmouseout="this.style.backgroundColor = ''">
        <td><%=assignment.Id.GetKey()%></td>
        <td nowrap><a href="Grading.aspx?AssignmentId=<%=assignment.Id.GetKey()%>"><%=Server.HtmlEncode(assignment.Title)%></a></td>
    <% foreach (Learner learner in learners) { %>
        <%
        LearnerAssignmentLocator locator = new LearnerAssignmentLocator();
        locator.LearnerId = learner.Id;
        locator.AssignmentId = assignment.Id;
        string scoreDisplay;
        float? finalPoints;
        if (finalPointsMap.TryGetValue(locator, out finalPoints))
        {
            if (finalPoints == null)
                scoreDisplay = "--";
            else
                scoreDisplay = String.Format("{0:n0}", finalPoints);
        }
        else
            scoreDisplay = String.Empty;
        %>
        <td <%=(scoreDisplay.Length == 0) ? "" : String.Format(" title=\"{0}\"",
            Server.HtmlEncode(learner.Name))%>><%=Server.HtmlEncode(scoreDisplay)%></td>
    <% } %>
    </tr>

<% } %>

</table>

</body>

<script runat="server">

class Learner : IComparable<Learner>
{
    // information about a learner
    public UserItemIdentifier Id;
    public string Name;

    public int CompareTo(Learner other)
    {
        // case-insensitive comparison of learner names
        return String.Compare(Name, other.Name, true);
    }

    public string GetInitials()
    {
        // return the learner's initials
        StringBuilder initials = new StringBuilder(10);
        foreach (string word in
                Name.Split(new char[] {' ', '-'}, StringSplitOptions.RemoveEmptyEntries))
            initials.Append(word[0]);
        return initials.ToString();
    }
}

class Assignment : IComparable<Assignment>
{
    // information about an assignment
    public AssignmentItemIdentifier Id;
    public string Title;

    public int CompareTo(Assignment other)
    {
        // case-insensitive comparison of assignment titles
        return String.Compare(Title, other.Title, true);
    }
}

class LearnerAssignmentLocator
{
    // information about a learner assignment
    public UserItemIdentifier LearnerId;
    public AssignmentItemIdentifier AssignmentId;

    public override int GetHashCode()
    {
        // case-insensitive comparison of assignment titles
        return (LearnerId.GetKey() ^ AssignmentId.GetKey()).GetHashCode();
    }

    public override bool Equals(object other)
    {
        LearnerAssignmentLocator otherLocator = other as LearnerAssignmentLocator;
        if (otherLocator == null)
            return false;
        return ((LearnerId == otherLocator.LearnerId) &&
                (AssignmentId == otherLocator.AssignmentId));
    }
}

</script>

