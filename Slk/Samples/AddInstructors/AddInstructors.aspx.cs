/* MICROSOFT PROVIDES SAMPLE CODE “AS IS” AND WITH ALL FAULTS, 
AND WITHOUT ANY WARRANTY WHATSOEVER.  MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES 
WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS NO WARRANTY OF TITLE OR 
NONINFRINGEMENT FOR THE SOURCE CODE. */

/* Copyright (c) Microsoft Corporation. All rights reserved. */

// AddInstructors.aspx.cs
//
// This file is part of the SharePoint Learning Kit "AddInstructors" sample code.  This sample
// consists of two parts:
//
//   1. An ASP.NET Web page, AddInstructors.aspx, which is based on a SharePoint master page (so
//      that it includes common SharePoint user interface elements).  This page leverages the
//      SharePoint "people picker" control.
//
//   2. A managed-code assembly, AddInstructorsSample.dll, that gets built into the global assembly
//      cache.  This DLL contains the "code behind" AddInstructors.aspx.  This file,
//      AddInstructors.aspx.cs, contains most of the source code for AddInstructorsSample.dll;
//      the remaining files (e.g. AssemblyInfo.cs and KeyPair.snk, and the Visual Studio project
//      and solution files) are available in the SLK/MLC SDK.  Note that the usual .aspx CodeBehind
//      feature isn't used, because that feature isn't well supported by SharePoint.
//      
// This sample code is located in Samples\SLK\AddInstructors within SLK-SDK-n.n.nnn-ENU.zip.
//
// This sample demonstrates how to add instructors to existing SLK assignments.  See ReadMe.txt for
// more information.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePointLearningKit;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace AddInstructorsSample
{

public class AddInstructorsPage : System.Web.UI.Page
{
    // controls
    protected System.Web.UI.WebControls.Panel SuccessPanel;
    protected System.Web.UI.WebControls.Label SuccessLabel;
    protected Microsoft.SharePoint.WebControls.InputFormSection OriginalInstructorSection;
    protected System.Web.UI.WebControls.DropDownList OriginalInstructor;
    protected Microsoft.SharePoint.WebControls.InputFormSection NewInstructorsSection;
    protected Microsoft.SharePoint.WebControls.PeopleEditor NewInstructors;
    protected Microsoft.SharePoint.WebControls.ButtonSection ButtonSection;
    protected System.Web.UI.WebControls.Button BtnOk;

    protected override void OnLoad(EventArgs e)
    {
        // ensure the user is an administrator, then execute the remaining code within a
        // LearningStorePrivilegedScope (which grants full access to database views)
        if (!SPFarm.Local.CurrentUserIsAdministrator())
		{
            throw new UnauthorizedAccessException(
				"Access is denied. Only adminstrators can access this page.");
		}
        using (new LearningStorePrivilegedScope())
        {
            // skip the code below during postback since <OriginalInstructor> will have been
            // populated from view state already
            if (IsPostBack)
                return;

            // populate the <OriginalInstructor> drop-down list with the names and SLK user
            // identifiers of all users who are instructors on any assignments in the current
            // SharePoint site collection 
            using (SPWeb spWeb = SPControl.GetContextWeb(HttpContext.Current))
            {
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                LearningStoreJob job = slkStore.LearningStore.CreateJob();
                LearningStoreQuery query = slkStore.LearningStore.CreateQuery(
					"AllAssignmentInstructors");
                query.SetParameter("SPSiteGuid", spWeb.Site.ID);
                query.AddColumn("InstructorName");
                query.AddColumn("InstructorId");
                query.AddSort("InstructorName", LearningStoreSortDirection.Ascending);
                job.PerformQuery(query);
                DataRowCollection rows = job.Execute<DataTable>().Rows;
                OriginalInstructor.Items.Add(String.Empty);
                foreach (DataRow row in rows)
                {
                    ListItem listItem = new ListItem();
                    listItem.Text = (string) row["InstructorName"];
                    UserItemIdentifier originalInstructorId =
                        new UserItemIdentifier((LearningStoreItemIdentifier)row["InstructorId"]);
                    listItem.Value = originalInstructorId.GetKey().ToString(
						CultureInfo.InvariantCulture);
                    OriginalInstructor.Items.Add(listItem);
                }
            }
        }
    }

	protected void BtnOk_Click(object sender, EventArgs e)
	{
		// the user clicked the OK button...

        // ensure the user is an administrator, then execute the remaining code within a
        // LearningStorePrivilegedScope (which grants full access to database views)
        if (!SPFarm.Local.CurrentUserIsAdministrator())
		{
            throw new UnauthorizedAccessException(
				"Access is denied. Only adminstrators can access this page.");
		}
        using (new LearningStorePrivilegedScope())
        {
            // if the user didn't select an "original instructor", do nothing
            if (OriginalInstructor.SelectedValue.Length == 0)
                return; // real code would display an error message here

            // if the user didn't enter any "new instructors", do nothing
            if (NewInstructors.Accounts.Count == 0)
                return; // the <NewInstructors> control already displays a validation error

            // set <originalInstructorId> to the SLK identifier of the selected "original
			// instructor"
            UserItemIdentifier originalInstructorId = new UserItemIdentifier(
                long.Parse(OriginalInstructor.SelectedValue, CultureInfo.InvariantCulture));

            // execute the following code within the context of the current SharePoint Web site;
            // in fact, the operations below are actually done across the entire site *collection*,
            // but SlkStore.GetStore needs to be passed a Web site, so we use the current site
            using (SPWeb spWeb = SPControl.GetContextWeb(HttpContext.Current))
            {
                // set <assignmentIds> to a list containing the IDs of the assignments for which
                // <originalInstructorId> is an instructor
                List<AssignmentItemIdentifier> assignmentIds = new List<AssignmentItemIdentifier>();
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                LearningStoreJob job = slkStore.LearningStore.CreateJob();
                LearningStoreQuery query = slkStore.LearningStore.CreateQuery("AllAssignmentIds");
                query.AddCondition("SPSiteGuid", LearningStoreConditionOperator.Equal,
					spWeb.Site.ID);
                query.AddCondition("InstructorId", LearningStoreConditionOperator.Equal,
					originalInstructorId);
                query.AddColumn("AssignmentId");
                job.PerformQuery(query);
                DataRowCollection rows = job.Execute<DataTable>().Rows;
                OriginalInstructor.Items.Add(String.Empty);
                foreach (DataRow row in rows)
                {
                    assignmentIds.Add(new AssignmentItemIdentifier(
                        (LearningStoreItemIdentifier)row["AssignmentId"]));
                }

                // set <newInstructorIds> to a list of SLK numeric user IDs corresponding to the
                // users in the <NewInstructors> control
                List<UserItemIdentifier> newInstructorIds = new List<UserItemIdentifier>();
                foreach (string loginName in NewInstructors.Accounts)
                {
                    // set <spUser> to the SharePoint SPUser corresponding to <loginName> (which
                    // was retrieved from the <NewInstructors> control>; quit with an error
                    // message if that user isn't in "All People" for this site collection
                    SPUser spUser;
                    try
                    {
                        spUser = spWeb.AllUsers[loginName];
                    }
                    catch (SPException)
                    {
                        NewInstructors.ErrorMessage = "User isn't in \"All People\": " +
                            loginName;
                        return;
                    }

					// set <userKey> to the SLK "user key", which is a string used to identify a
					// user in the SLK database; SLK uses a user's security identifier (SID) if
					// they have one, or their login name if they don't (e.g. in the case of forms
					// authentication)
					string userKey = String.IsNullOrEmpty(spUser.Sid)
						? spUser.LoginName : spUser.Sid;

                    // set <userId> to the SLK UserItemIdentifier of the user <spUser> by
					// searching the SLK UserItem table; if the user isn't found in UserItem,
					// add them
                    UserItemIdentifier userId;
                    job = slkStore.LearningStore.CreateJob();
                    query = slkStore.LearningStore.CreateQuery("UserItem");
                    query.AddCondition("Key", LearningStoreConditionOperator.Equal, userKey);
                    query.AddColumn("Id");
                    job.PerformQuery(query);
                    rows = job.Execute<DataTable>().Rows;
                    if (rows.Count != 0)
                    {
						// found user in the SLK UserItem table
                        userId = new UserItemIdentifier(
							(LearningStoreItemIdentifier) rows[0]["Id"]);
                    }
					else
					{
						// user not found in SLK UserItem table -- add them; we use
						// LearningStoreJob.AddOrUpdateItem rather than LearningStoreJob.AddItem
						// to account for the rare case where the user may be added simultaneously
						// by another process
                        job = slkStore.LearningStore.CreateJob();
                        Dictionary<string,object> findProperties = new Dictionary<string,object>();
						findProperties["Key"] = userKey;
                        Dictionary<string,object> setProperties = new Dictionary<string,object>();
						setProperties["Name"] = spUser.Name;
						job.AddOrUpdateItem("UserItem", findProperties, setProperties, null, true);
                        userId = new UserItemIdentifier(
							job.Execute<LearningStoreItemIdentifier>());
					}

                    // update <newInstructorIds>
                    newInstructorIds.Add(userId);
                }

                // add each user in <newInstructorIds> as an instructor to each assignment in
                // <assignmentIds>; set <updatedAssignmentCount> to the number of assignments that
                // were updated (note that we don't update assignments for which the new
                // instructors are already instructors)
                Dictionary<UserItemIdentifier, bool> oldInstructors =
                    new Dictionary<UserItemIdentifier, bool>();
                int updatedAssignmentCount = 0;
                foreach (AssignmentItemIdentifier assignmentId in assignmentIds)
                {
                    AssignmentProperties assignmentProperties =
                        slkStore.GetAssignmentProperties(assignmentId, SlkRole.Instructor);
                    oldInstructors.Clear();
                    foreach (SlkUser slkUser in assignmentProperties.Instructors)
                        oldInstructors[slkUser.UserId] = true;
                    int oldInstructorCount = oldInstructors.Count;
                    foreach (UserItemIdentifier userId in newInstructorIds)
                    {
                        if (!oldInstructors.ContainsKey(userId))
                            assignmentProperties.Instructors.Add(new SlkUser(userId));
                    }
                    if (assignmentProperties.Instructors.Count != oldInstructorCount)
                    {
                        slkStore.SetAssignmentProperties(assignmentId, assignmentProperties);
                        updatedAssignmentCount++;
                    }
                }

                // provide user feedback
                SuccessPanel.Visible = true;
                SuccessLabel.Text =
					String.Format("Found {0} assignment(s); updated {1} assignment(s).",
						assignmentIds.Count, updatedAssignmentCount);
                OriginalInstructorSection.Visible = false;
                NewInstructorsSection.Visible = false;
                ButtonSection.Visible = false;
            }
        }
	}
}

}

