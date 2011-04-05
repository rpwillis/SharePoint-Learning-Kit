/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// DeletePackages.aspx.cs
//
// Dialog box for deleting learning packages from PackageStore.
//
// The parent page passes a comma-separated list of packages to delete to this dialog using
// <dialogArguments>.
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

public partial class DeletePackages : BasicWebPlayerBase
{
    protected void DeletePackagesButton_Click(object sender, EventArgs e)
    {
        // the user clicked "Upload"...

        // hide the confirmation panel
        ConfirmMessage.Visible = false;

		// the PackagesToDelete hidden form element contains a comma-delimited list of IDs of
		// packages to delete (copied from <dialogArguments> on the client) -- attempt to delete
		// those packages, and set <deleted> to the IDs of packages successfully deleted
		List<string> deleted = new List<string>();
        try
        {
            // loop once for each package to delete
            foreach (string id in PackagesToDelete.Value.Split(','))
            {
                // set <packageId> to the ID of this package
                PackageItemIdentifier packageId = new PackageItemIdentifier(
                    Convert.ToInt64(id, CultureInfo.InvariantCulture));

                // before we delete the package, we need to delete all attempts on the package --
                // the following query looks for those attempts
                LearningStoreJob job = LStore.CreateJob();
                LearningStoreQuery query = LStore.CreateQuery(
					Schema.MyAttemptsAndPackages.ViewName);
                query.AddCondition(Schema.MyAttemptsAndPackages.PackageId,
                    LearningStoreConditionOperator.Equal, packageId);
                query.AddCondition(Schema.MyAttemptsAndPackages.AttemptId,
                    LearningStoreConditionOperator.NotEqual, null);
                query.AddColumn(Schema.MyAttemptsAndPackages.AttemptId);
                query.AddSort(Schema.MyAttemptsAndPackages.AttemptId,
                    LearningStoreSortDirection.Ascending);
                job.PerformQuery(query);
                DataTable dataTable = job.Execute<DataTable>();
                AttemptItemIdentifier previousAttemptId = null;

				// loop once for each attempt on this package
                foreach (DataRow dataRow in dataTable.Rows)
                {
					// set <attemptId> to the ID of this attempt
                    AttemptItemIdentifier attemptId;
                    LStoreHelper.CastNonNull(dataRow["AttemptId"], out attemptId);

					// if <attemptId> is a duplicate attempt ID, skip it; note that the query
					// results are sorted by attempt ID (see above)
                    if ((previousAttemptId != null) &&
					    (previousAttemptId.GetKey() == attemptId.GetKey()))
						continue;

					// delete this attempt
                    StoredLearningSession.DeleteAttempt(LStore, attemptId);

					// continue to the next attempt
                    previousAttemptId = attemptId;
                }

                // delete the package
                PStore.DeletePackage(packageId);

                // add the package ID to the list of deleted packages
                deleted.Add(id);
            }

            // the operation was successful, and there are no messages to
			// display to the user, so close the dialog
            CloseDialogScript.Visible = true;
        }
        catch (Exception ex)
        {
            // an unexpected error occurred -- display a generic message that
            // doesn't include the exception message (since that message may
            // include sensitive information), and write the exception message
            // to the event log
            ErrorIntro.Visible = true;
            ErrorMessage.Visible = true;
            ErrorMessage.Controls.Add(new System.Web.UI.LiteralControl(
                Server.HtmlEncode("A serious error occurred.  Please contact your system administrator.  More information has been written to the server event log.")));
            LogEvent(System.Diagnostics.EventLogEntryType.Error,
                "An exception occurred while deleting a package:\n\n{0}\n\n", ex.ToString());
        }

        // update the buttons
        DeletePackagesButton.Visible = false;
        CloseButton.Text = "OK";

		// set the hidden form element PackagesSuccessfullyDeleted to a
		// comma-separated list of IDs of packages that were successfully
		// deleted, and enable the client-side script that communicates this
		// information to the parent page
        PackagesSuccessfullyDeleted.Value = String.Join(",", deleted.ToArray());
        UpdateParentPageScript.Visible = true;
    }
}

