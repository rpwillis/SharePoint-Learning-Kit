/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// CreateAttempt.aspx.cs
//
// Dialog box for creating an attempt on a learning package.
//
// The parent page passes the ID of the package to begin an attempt on using <dialogArguments>.
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

public partial class CreateAttempt : BasicWebPlayerBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void CreateAttemptButton_Click(object sender, EventArgs e)
    {
        // the hidden "Create Attempt" button was auto-clicked by script on page load...

        // prevent script from clicking "Create Attempt" again
        AutoPostScript.Visible = false;

        // hide the "please wait" panel
        PleaseWait.Visible = false;

		// the OrganizationId hidden form element contains the ID of the organization to attempt --
		// try to create a new attempt based on that ID; an organization is a root-level activity,
		// so OrganizationId is actually an ActivityPackageItemIdentifier
        try
        {
            // set <currentUser> to information about the current user; we
            // need the current user's UserItemIdentifier
            LStoreUserInfo currentUser  = GetCurrentUserInfo();

			// set <organizationId> from the OrganizationId hidden form element as described above
			ActivityPackageItemIdentifier organizationId = new ActivityPackageItemIdentifier(
				Convert.ToInt64(OrganizationId.Value, CultureInfo.InvariantCulture));

			// create an attempt on <organizationId>
            StoredLearningSession session = StoredLearningSession.CreateAttempt(PStore,
                currentUser.Id, organizationId, LoggingOptions.LogAll);

            // the operation was successful, and there are no messages to display to the user, so
			// update the AttemptId hidden form element with the ID of the newly-created attempt,
			// update the parent page, and close the dialog
            AttemptId.Value = Convert.ToString(session.AttemptId.GetKey(), CultureInfo.InvariantCulture);
            UpdateParentPageScript.Visible = true;
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
                "An exception occurred while creating an attempt:\n\n{0}\n\n", ex.ToString());
            Buttons.Visible = true;
        }
    }

}
