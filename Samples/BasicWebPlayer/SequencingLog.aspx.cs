/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// SequencingLog.aspx.cs
//
// Dialog box that displays the sequencing log of a given attempt.
//
// The parent page passes the ID of the attempt to view the log of using <dialogArguments>.
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = BasicWebPlayer.Schema;
using LearningComponentsHelper;

public partial class SequencingLog : BasicWebPlayerBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
		// set <attemptId> to the ID of this attempt
		AttemptItemIdentifier attemptId = new AttemptItemIdentifier(
			Convert.ToInt64(Request.QueryString["AttemptId"], CultureInfo.InvariantCulture));

        // set <previousEntryId> to the ID of the sequencing log entry that we most recently
        // displayed; newer entries than that will be highlighted
        long previousEntryId;
        string value = Request.QueryString["PreviousEntryId"];
		if (value == null)
			previousEntryId = long.MaxValue;
		else
			previousEntryId = Convert.ToInt64(value, CultureInfo.InvariantCulture);

        try
        {
            // create a job to execute two queries
            LearningStoreJob job = LStore.CreateJob();

            // first query: get the package name and organization title of this attempt
            LearningStoreQuery query = LStore.CreateQuery(Schema.MyAttemptsAndPackages.ViewName);
            query.AddColumn(Schema.MyAttemptsAndPackages.PackageFileName);
            query.AddColumn(Schema.MyAttemptsAndPackages.OrganizationTitle);
            query.AddCondition(Schema.MyAttemptsAndPackages.AttemptId,
                LearningStoreConditionOperator.Equal, attemptId);
            job.PerformQuery(query);

            // second query: get the contents of the sequencing log for this attempt
            query = LStore.CreateQuery(Schema.SequencingLog.ViewName);
            query.AddColumn(Schema.SequencingLog.Id);
            query.AddColumn(Schema.SequencingLog.Timestamp);
            query.AddColumn(Schema.SequencingLog.EventType);
            query.AddColumn(Schema.SequencingLog.NavigationCommand);
            query.AddColumn(Schema.SequencingLog.Message);
            query.SetParameter(Schema.SequencingLog.AttemptId, attemptId);
            query.AddSort(Schema.SequencingLog.Id,
                LearningStoreSortDirection.Descending);
            job.PerformQuery(query);

            // execute the job
            ReadOnlyCollection<object> results = job.Execute();
            DataTable attemptInfoDataTable = (DataTable)results[0];
            DataTable sequencingLogDataTable = (DataTable)results[1];

            // extract information from the first query into local variables
            DataRow attemptInfo = attemptInfoDataTable.Rows[0];
            string packageFileName;
            LStoreHelper.CastNonNull(attemptInfo[Schema.MyAttemptsAndPackages.PackageFileName],
                out packageFileName);
            string organizationTitle;
            LStoreHelper.CastNonNull(attemptInfo[Schema.MyAttemptsAndPackages.OrganizationTitle],
                out organizationTitle);

            // set <trainingName> to the name to use for this attempt
            string trainingName;
            if (organizationTitle.Length == 0)
                trainingName = packageFileName;
            else
                trainingName = String.Format("{0} - {1}", packageFileName, organizationTitle);

            // copy <trainingName> to the UI
            TrainingName.Text = Server.HtmlEncode(trainingName);

			// set <maxLogEntryId> to the highest-numbered log entry ID -- which is the first one,
			// since they're sorted by descending entry ID
            SequencingLogEntryItemIdentifier maxLogEntryId;
			if (sequencingLogDataTable.Rows.Count > 0)
			{
				DataRow dataRow = sequencingLogDataTable.Rows[0];
                LStoreHelper.CastNonNull(dataRow[Schema.SequencingLog.Id],
                    out maxLogEntryId);
			}
			else
                maxLogEntryId = new SequencingLogEntryItemIdentifier(-1);

            // loop once for each item in the sequencing log
            foreach (DataRow dataRow in sequencingLogDataTable.Rows)
            {
                // extract information from <dataRow> into local variables
                SequencingLogEntryItemIdentifier logEntryId;
                LStoreHelper.CastNonNull(dataRow[Schema.SequencingLog.Id],
                    out logEntryId);
                DateTime? time;
                LStoreHelper.Cast(dataRow[Schema.SequencingLog.Timestamp],
                    out time);
                SequencingEventType? eventType;
                LStoreHelper.Cast(dataRow[Schema.SequencingLog.EventType],
                    out eventType);
                NavigationCommand? navigationCommand;
                LStoreHelper.Cast(dataRow[Schema.SequencingLog.NavigationCommand],
                    out navigationCommand);
                string message;
                LStoreHelper.CastNonNull(dataRow[Schema.SequencingLog.Message],
                    out message);

                // begin the HTML table row
                TableRow htmlRow = new TableRow();

                // highlight this row if it's new since the last refresh
                if (logEntryId.GetKey() > previousEntryId)
                    htmlRow.CssClass = "Highlight";

                // add the "ID" HTML cell
                TableCell htmlCell = new TableCell();
                htmlCell.CssClass = "Id_";
                htmlCell.Wrap = false;
                htmlCell.Text = NoBr(Server.HtmlEncode(logEntryId.GetKey().ToString()));
                htmlRow.Cells.Add(htmlCell);

                // add the "Time" HTML cell
                htmlCell = new TableCell();
                htmlCell.CssClass = "Time_";
                htmlCell.Wrap = false;
                htmlCell.Text = NoBr(Server.HtmlEncode(String.Format("{0:d} {0:t}", time)));
                htmlRow.Cells.Add(htmlCell);

                // add the "EventType" HTML cell
                htmlCell = new TableCell();
                htmlCell.CssClass = "EventType_";
                htmlCell.Wrap = false;
                htmlCell.Text = NoBr(Server.HtmlEncode(eventType.ToString()));
                htmlRow.Cells.Add(htmlCell);

                // add the "NavigationCommand" HTML cell
                htmlCell = new TableCell();
                htmlCell.CssClass = "NavigationCommand_";
                htmlCell.Wrap = false;
                htmlCell.Text = NoBr(Server.HtmlEncode(navigationCommand.ToString()));
                htmlRow.Cells.Add(htmlCell);

                // add the "Message" HTML cell
                htmlCell = new TableCell();
                htmlCell.CssClass = "Message_";
                htmlCell.Wrap = false;
                htmlCell.Text = Server.HtmlEncode(message);
                htmlRow.Cells.Add(htmlCell);

                // end the table HTML row; add it to the HTML table
                LogGrid.Rows.Add(htmlRow);
            }

            // update <RefreshLink>
            RefreshLink.NavigateUrl = String.Format("?AttemptId={0}&PreviousEntryId={1}",
                attemptId.GetKey(), maxLogEntryId.GetKey());
        }
        catch (Exception ex)
        {
            // an unexpected error occurred -- display a generic message that doesn't include the
			// exception message (since that message may include sensitive information), and write
			// the exception message to the event log
            LogTitle.Visible = false;
            LogContents.Visible = false;
            ErrorMessage.Visible = true;
            ErrorMessage.Controls.Add(new System.Web.UI.LiteralControl(
                Server.HtmlEncode("A serious error occurred.  Please contact your system administrator.  More information has been written to the server event log.")));
            LogEvent(System.Diagnostics.EventLogEntryType.Error,
                "An exception occurred while accessing the sequencing log for attempt #{0}:\n\n{1}\n\n",
				attemptId.GetKey(), ex.ToString());
        }
    }
}
