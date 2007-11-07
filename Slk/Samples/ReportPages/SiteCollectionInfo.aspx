<%-- MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. --%>

<%-- Copyright (c) Microsoft Corporation. All rights reserved. --%>

<%-- SiteCollectionInfo.aspx
  --
  -- This is SharePoint Learning Kit sample code that executes as an ASP.NET Web page.  ReadMe.txt
  -- contains instructions for installing this Web page into an existing SharePoint Learning Kit
  -- installation.  Visual Studio is not required.
  --
  -- This sample code is located in Samples\SLK\ReportPages within SLK-SDK-n.n.nnn-ENU.zip.
  --
  -- This Web page demonstrates how to create a report that queries the SLK database directly,
  -- rather than using the SLK API.  The advantage of this approach is that the user can access
  -- information that would not normally be available through the SLK API (either for functionality
  -- or security reasons).  The disadvantage is that SLK cannot ensure that the current user is
  -- entitled to view the information displayed -- however, since the database is being accessed
  -- directly using the current user's credentials, the page will fail if the current user doesn't
  -- have permission to directly access the database.
  --
  -- This report displays information about SLK's mapping between the current SharePoint site
  -- collection (specified in the URL path) and the SharePoint Learning Kit database, as well as
  -- summary information about assignments within sites in that site collection.
  --%>

<%-- NOTE: If this page displays "File Not Found", check the build number (1.0.nnn.0) below --%>
<%@ Assembly Name="Microsoft.SharePointLearningKit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=24e5ae139825747e" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" MasterPageFile="~/_layouts/application.master" ValidateRequest="False" %>
<%@ Import namespace="System.Collections.Generic" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="System.Data.SqlClient" %>
<%@ Import namespace="Microsoft.SharePoint" %>
<%@ Import namespace="Microsoft.SharePoint.Administration" %>
<%@ Import namespace="Microsoft.SharePoint.WebControls" %>
<%@ Import namespace="Microsoft.SharePointLearningKit" %>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitle" runat="server">
    SLK Sample Report - Site Collection Information
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageTitleInTitleArea" runat="server">
    SLK Sample Report - Site Collection Information
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderPageDescription" runat="server">
    This is a SharePoint Learning Kit sample report that displays information about SLK's mapping
    between the current SharePoint site collection (specified in the URL path) and the SharePoint
    Learning Kit database, as well as summary information about assignments within sites in that
    site collection.  You must have full access to the database to use this report.
</asp:Content>

<asp:Content ContentPlaceHolderId="PlaceHolderMain" runat="server">

<%
    SPSite spSite;
    SlkSPSiteMapping mapping;
    int assignmentCount, webCount, instructorAssignmentCount, instructorCount,
        learnerAssignmentCount, learnerCount;
    Stack<IDisposable> disposer = new Stack<IDisposable>();
    try
    {
        // set <spSite> to refer to the site collection specified by the portion of the URL
        // before "_layouts"
        SPWeb spWeb = SPControl.GetContextWeb(Context);
        spSite = spWeb.Site;
        disposer.Push(spSite);
        
        // set <mapping> to information about SLK associated with this site collection
        mapping = SlkSPSiteMapping.GetMapping(spSite.ID);

        // execute a few SQL queries to get information about the SLK assignments in this site
        // collection
        SqlConnection connection = new SqlConnection(mapping.DatabaseConnectionString);
        disposer.Push(connection);
        connection.Open();
        SqlCommand command = new SqlCommand(
            @"
                SELECT COUNT(*) AssignmentCount,
                       COUNT(DISTINCT SPWebGuid) WebCount
                FROM AssignmentItem
                WHERE SPSiteGuid = @SPSiteGuid

                SELECT COUNT(*) InstructorAssignmentCount,
                       COUNT(DISTINCT iai.InstructorId) InstructorCount
                FROM InstructorAssignmentItem iai
                INNER JOIN AssignmentItem ai
                ON iai.AssignmentId = ai.Id
                WHERE ai.SPSiteGuid = @SPSiteGuid

                SELECT COUNT(*) LearnerAssignmentCount,
                       COUNT(DISTINCT lai.LearnerId) LearnerCount
                FROM LearnerAssignmentItem lai
                INNER JOIN AssignmentItem ai
                ON lai.AssignmentId = ai.Id
                WHERE ai.SPSiteGuid = @SPSiteGuid
            ", connection);
        disposer.Push(command);
        command.Parameters.AddWithValue("@SpSiteGuid", spSite.ID);
        SqlDataReader reader = command.ExecuteReader();
        disposer.Push(reader);
        reader.Read();
        assignmentCount = (int) reader["AssignmentCount"];
        webCount = (int) reader["WebCount"];
        reader.NextResult();
        reader.Read();
        instructorAssignmentCount = (int) reader["InstructorAssignmentCount"];
        instructorCount = (int) reader["InstructorCount"];
        reader.NextResult();
        reader.Read();
        learnerAssignmentCount = (int) reader["LearnerAssignmentCount"];
        learnerCount = (int) reader["LearnerCount"];
    }
    catch (SqlException ex)
    {
        Response.Write("<div class=\"ms-formvalidation\">SQL Server error: ");
        Response.Write(Server.HtmlEncode(ex.Message));
        Response.Write("</div>");
        Response.End();
        throw new Exception(); // not reached -- avoids "Use of unassigned local variable" errors
    }
    finally
    {
        // dispose of objects used by this method
        while (disposer.Count > 0)
            disposer.Pop().Dispose();
    }
%>

<table style="font-size: 8.5pt; border-collapse: collapse; border: 1px inset;">
    <col style="padding-left: 4pt; padding-right: 8pt" />
    <col style="padding-right: 4pt" />
    <tr>
        <td>Site collection URL:</td>
        <td><%=Server.HtmlEncode(spSite.Url)%></td>
    </tr>
    <tr>
        <td>Site collection GUID:</td>
        <td><%=Server.HtmlEncode(spSite.ID.ToString())%></td>
    </tr>
    <tr>
        <td>Database server:</td>
        <td><%=Server.HtmlEncode(Regex.Replace(mapping.DatabaseServer, ";.*", ""))%></td>
    </tr>
    <tr>
        <td>Database name:</td>
        <td><%=Server.HtmlEncode(mapping.DatabaseName)%></td>
    </tr>
    <tr>
        <td>Instructor permission:</td>
        <td><%=Server.HtmlEncode(mapping.InstructorPermission)%></td>
    </tr>
    <tr>
        <td>Learner permission:</td>
        <td><%=Server.HtmlEncode(mapping.LearnerPermission)%></td>
    </tr>

    <tr>
        <td>Sites with assignments:</td>
        <td><%=webCount%></td>
    </tr>
    <tr>
        <td>Instructors:</td>
        <td><%=instructorCount%></td>
    </tr>
    <tr>
        <td>Learners:</td>
        <td><%=learnerCount%></td>
    </tr>
    <tr>
        <td>Assignments:</td>
        <td><%=assignmentCount%></td>
    </tr>
    <tr>
        <td>Learner assignments:</td>
        <td><%=learnerAssignmentCount%></td>
    </tr>
    <% if (assignmentCount > 0) { %>
    <tr>
        <td>Average instructors per assignment:</td>
        <td><%=String.Format("{0:n2}", (double) instructorAssignmentCount / assignmentCount)%></td>
    </tr>
    <tr>
        <td>Average learners per assignment:</td>
        <td><%=String.Format("{0:n2}", (double) learnerAssignmentCount / assignmentCount)%></td>
    </tr>
    <% } %>
</table>

</asp:Content>

