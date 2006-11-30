/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// UploadPackage.aspx.cs
//
// Dialog box for uploading a learning package to PackageStore.
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = BasicWebPlayer.Schema;


public partial class UploadPackage : BasicWebPlayerBase
{
    protected void UploadPackageButton_OnClick(object sender, EventArgs e)
    {
        // the user clicked "Upload"...

        // do nothing if the user didn't select a file to upload
        if (!UploadedPackageFile.HasFile)
            return;

        // hide the upload panel and show the message panel; the message panel will hold
		// information about the success or failure of the package upload operation
        UploadPanel.Visible = false;
        MessagePanel.Visible = true;

        // attempt to import the uploaded file into PackageStore
        try
        {
            // set <currentUser> to information about the current user; we need the current user's
			// UserItemIdentifier
            LStoreUserInfo currentUser  = GetCurrentUserInfo();

            // import the package file; set packageId to the ID of the uploaded
			// package; set importLog to a collection of warnings (if any)
			// about the import process
			PackageItemIdentifier packageId;
            ValidationResults importLog;
            using (PackageReader packageReader = PackageReader.Create(UploadedPackageFile.FileContent))
            {
                // Add package, asking to fix anything that can be fixed.
                AddPackageResult result = PStore.AddPackage(packageReader, new PackageEnforcement(false, false, false));
                packageId = result.PackageId;
                importLog = result.Log;
            }

            // fill in the application-specific columns of the PackageItem table
            LearningStoreJob job = LStore.CreateJob();
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[Schema.PackageItem.Owner] = currentUser.Id;
            properties[Schema.PackageItem.FileName] = UploadedPackageFile.FileName;
            properties[Schema.PackageItem.UploadDateTime] = DateTime.Now;
            job.UpdateItem(packageId, properties);
            job.Execute();

            // retrieve information about the package
            job = LStore.CreateJob();
            RequestMyTraining(job, packageId);
            GetMyTrainingResultsToHtml(job.Execute<DataTable>(), TrainingGrid);

            // when the page loads in the browser, copy information about the from the TrainingGrid
			// hidden table to the main page using client-side script
            UpdateParentPageScript.Visible = true;

            // provide user feedback
            if (importLog.HasWarnings)
            {
                // display warnings
                WarningIntro.Visible = true;
                WarningMessages.Visible = true;
                foreach (ValidationResult result in importLog.Results)
                    ScrollingMessagesList.Items.Add(new ListItem(result.Message));
            }
            else
            {
                // the operation was successful, and there are no messages to display to the user,
				// so close the dialog
                CloseDialogScript.Visible = true;
            }
        }
        catch (PackageImportException ex)
        {
            // a package import failure occurred -- display the error message
            ErrorIntro.Visible = true;
            ErrorMessage.Visible = true;
            
            foreach (ValidationResult result in ex.Log.Results)
                ErrorMessageScrollingList.Items.Add(new ListItem(result.Message));

            if (ex.InnerException != null)
                ErrorMessageScrollingList.Items.Add(new ListItem( 
                            Server.HtmlEncode(ex.InnerException.Message)));                

        }
        catch (Exception ex)
        {
            // an unexpected error occurred -- display a generic message that doesn't include the
			// exception message (since that message may include sensitive information), and write
			// the exception message to the event log
            ErrorIntro.Visible = true;
            ErrorMessage.Visible = true;
            ErrorMessage.Controls.Add(new System.Web.UI.LiteralControl(
                Server.HtmlEncode("A serious error occurred.  Please contact your system administrator.  More information has been written to the server event log.")));
            LogEvent(System.Diagnostics.EventLogEntryType.Error,
                "An exception occurred while uploading a package:\n\n{0}\n\n", ex.ToString());
        }
    }
}

