/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.
// BasicWebPlayerBase.cs
//
// Contains classes that help implement the BasicWebPlayer application.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Security.Principal;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = BasicWebPlayer.Schema;
using LearningComponentsHelper;

// <summary>
// Base class for all pages in the BasicWebPlayer application.  Contains functionality common to
// the entire application.
// </summary>
//
public class BasicWebPlayerBase : PageHelper
{
    /// <summary>
    /// Requests that the list of training for the current user be retrieved from the LearningStore
	/// database.  Adds the request to a given <c>LearningStoreJob</c> for later execution.
    /// </summary>
    /// 
    /// <param name="job">A <c>LearningStoreJob</c> to add the new query to.</param>
    /// 
    /// <param name="packageId">To request information related to a single pass the
	/// 	<c>PackageItemIdentifier</c> of the package as this parameter.  Otherwise, leave this
	/// 	parameter <c>null</c>.</param>
    /// 
    /// <remarks>
    /// After executing this method, and later calling <c>Job.Execute</c>, call
	/// <c>GetMyTrainingResultsToHtml</c> to convert the <c>DataTable</c> returned by
	/// <c>Job.Execute</c> into HTML.
    /// </remarks>
    ///
    protected void RequestMyTraining(LearningStoreJob job,
        PackageItemIdentifier packageId)
    {
        LearningStoreQuery query = LStore.CreateQuery(Schema.MyAttemptsAndPackages.ViewName);
        query.AddColumn(Schema.MyAttemptsAndPackages.PackageId);
        query.AddColumn(Schema.MyAttemptsAndPackages.PackageFileName);
        query.AddColumn(Schema.MyAttemptsAndPackages.OrganizationId);
        query.AddColumn(Schema.MyAttemptsAndPackages.OrganizationTitle);
        query.AddColumn(Schema.MyAttemptsAndPackages.AttemptId);
        query.AddColumn(Schema.MyAttemptsAndPackages.UploadDateTime);
        query.AddColumn(Schema.MyAttemptsAndPackages.AttemptStatus);
        query.AddColumn(Schema.MyAttemptsAndPackages.TotalPoints);
        query.AddSort(Schema.MyAttemptsAndPackages.UploadDateTime,
			LearningStoreSortDirection.Ascending);
        query.AddSort(Schema.MyAttemptsAndPackages.OrganizationId,
			LearningStoreSortDirection.Ascending);
        if (packageId != null)
        {
            query.AddCondition(Schema.MyAttemptsAndPackages.PackageId,
                LearningStoreConditionOperator.Equal, packageId);
        }
        job.PerformQuery(query);
    }

    /// <summary>
    /// Reads a <c>DataTable</c>, returned by <c>Job.Execute</c>, containing the results requested
	/// by a previous call to <c>RequestMyTraining</c>.  Converts the results to HTML rows, added
	/// to a given HTML table.
    /// </summary>
    ///
    /// <param name="dataTable">A <c>DataTable</c> returned from <c>Job.Execute</c>.</param>
    ///
    /// <param name="htmlTable">The HTML table to write to.</param>
    ///
    protected void GetMyTrainingResultsToHtml(DataTable dataTable,
        Table htmlTable)
    {
        // loop once for each organization of each package
        PackageItemIdentifier previousPackageId = null;
        foreach (DataRow dataRow in dataTable.Rows)
        {
            // extract information from <dataRow> into local variables
            PackageItemIdentifier packageId;
            LStoreHelper.CastNonNull(dataRow[Schema.MyAttemptsAndPackages.PackageId],
				out packageId);
			string packageFileName;
            LStoreHelper.CastNonNull(dataRow[Schema.MyAttemptsAndPackages.PackageFileName],
				out packageFileName);
            ActivityPackageItemIdentifier organizationId;
            LStoreHelper.CastNonNull(dataRow[Schema.MyAttemptsAndPackages.OrganizationId],
				out organizationId);
			string organizationTitle;
            LStoreHelper.CastNonNull(dataRow[Schema.MyAttemptsAndPackages.OrganizationTitle],
				out organizationTitle);
            AttemptItemIdentifier attemptId;
            LStoreHelper.Cast(dataRow[Schema.MyAttemptsAndPackages.AttemptId],
				out attemptId);
			DateTime? uploadDateTime;
            LStoreHelper.Cast(dataRow[Schema.MyAttemptsAndPackages.UploadDateTime],
				out uploadDateTime);
			AttemptStatus? attemptStatus;
            LStoreHelper.Cast(dataRow[Schema.MyAttemptsAndPackages.AttemptStatus],
				out attemptStatus);
			float? score;
            LStoreHelper.Cast(dataRow[Schema.MyAttemptsAndPackages.TotalPoints],
				out score);

            // if this <dataRow> is another organization (basically another "table of contents")
			// within the same package as the previous <dataRow>, set <samePackage> to true
            bool samePackage = ((previousPackageId != null) &&
				(packageId.GetKey() == previousPackageId.GetKey()));

            // begin the HTML table row
            TableRow htmlRow = new TableRow();
            htmlRow.ID = "Package" + packageId.GetKey().ToString();

            // set <trainingName> to a name to use for this row (i.e. one
            // organization of one package)
            string trainingName;
            if (organizationTitle.Length == 0)
                trainingName = packageFileName;
            else
                trainingName = String.Format("{0} - {1}", packageFileName, organizationTitle);

            // add the "Select" HTML cell, unless this row is for the same
            // package as the previous row
            TableCell htmlCell = new TableCell();
			if (samePackage)
				htmlCell.CssClass = "Select_ SamePackage_";
            else
			{
				htmlCell.CssClass = "Select_ NewPackage_";
				CheckBox checkBox = new CheckBox();
				checkBox.ID = "Select" + packageId.GetKey().ToString();
				checkBox.Attributes.Add("onclick", "OnSelectionChanged()");
				checkBox.ToolTip = "Select";
				htmlCell.Controls.Add(checkBox);
			}
            htmlRow.Cells.Add(htmlCell);

            // add the "Name" HTML cell
            htmlCell = new TableCell();
            htmlCell.CssClass = "Name_";
            HtmlAnchor anchor = new HtmlAnchor();
			if (attemptId == null)
			{
				// attempt has not yet been created for this training -- create an URL that begins
				// with "Org:", indicating that the ID is an OrganizationId which identifies
				// which organization of which package to launch
				anchor.HRef = String.Format("javascript:OpenTraining('Org:{0}')",
					organizationId.GetKey());

				// give this anchor an ID that allows client-side script to update its URL once
				// training has been launched
				anchor.ID = String.Format("Org_{0}", organizationId.GetKey());

                // provide a tooltip
                anchor.Attributes.Add("title", "Begin training");
            }
			else
			{
				// attempt was already created -- include its ID in the URL
				anchor.HRef = String.Format("javascript:OpenTraining('Att:{0}')",
					attemptId.GetKey());

                // provide a tooltip
                anchor.Attributes.Add("title", "Continue training");
            }
            anchor.InnerText = trainingName;
            htmlCell.Controls.Add(anchor);
            htmlRow.Cells.Add(htmlCell);

            // add the "Uploaded" HTML cell
            htmlCell = new TableCell();
            htmlCell.CssClass = "Uploaded_";
            htmlCell.Wrap = false;
            htmlCell.Text = NoBr(Server.HtmlEncode(
				String.Format("{0:d} {0:t}", uploadDateTime)));
            htmlRow.Cells.Add(htmlCell);

            // add the "Status" HTML cell
            string attemptStatusString;
            if (attemptStatus == null)
                attemptStatusString = "Not Started";
            else
                attemptStatusString = attemptStatus.ToString();
            htmlCell = new TableCell();
            htmlCell.CssClass = "Status_";
            htmlCell.Wrap = false;
            if (attemptId != null)
            {
                anchor = new HtmlAnchor();
                anchor.HRef = String.Format("javascript:ShowLog({0})", attemptId.GetKey());
                anchor.InnerHtml = NoBr(Server.HtmlEncode(attemptStatusString));
                anchor.Attributes.Add("title", "Show Log");
                htmlCell.Controls.Add(anchor);
            }
            else
                htmlCell.Text = NoBr(Server.HtmlEncode(attemptStatusString));
            htmlRow.Cells.Add(htmlCell);

            // add the "Score" HTML cell
			string scoreString;
			if (score == null)
				scoreString = "";
			else
				scoreString = String.Format("{0:0}%", Math.Round(score.Value));
            htmlCell = new TableCell();
            htmlCell.CssClass = "Score_";
            htmlCell.Wrap = false;
            htmlCell.Text = NoBr(Server.HtmlEncode(scoreString));
            htmlRow.Cells.Add(htmlCell);

            // end the table HTML row; add it to the HTML table
            htmlTable.Rows.Add(htmlRow);
            previousPackageId = packageId;
        }
    }

	/// <summary>
	/// Adds "no-break" HTML elements around a given string of HTML.
	/// </summary>
	///
	/// <remarks>
	/// <c>TableCell.Wrap = false</c> can also be used, but that approach won't work if script is
	/// used to copy table rows from one window to another at runtime.
	/// </remarks>
	///
	protected static string NoBr(string html)
	{
		return "<nobr>" + html + "</nobr>";
	}
}

