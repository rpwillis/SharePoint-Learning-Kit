/* Copyright (c) Microsoft Corporation. All rights reserved. */
// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.

// Default.aspx.cs
//
// BasicWebPlayer sample MLC application.
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = BasicWebPlayer.Schema;

public partial class _Default : BasicWebPlayerBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // set <currentUser> to information about the current user, and add the current user's list
		// of training to the TrainingGrid table; for best performance, both these requests are
		// done in a single call to the database
        LearningStoreJob job = LStore.CreateJob();
        RequestCurrentUserInfo(job);
        RequestMyTraining(job, null);
        ReadOnlyCollection<object> results = job.Execute();
        LStoreUserInfo currentUser = GetCurrentUserInfoResults((DataTable) results[0]);
        GetMyTrainingResultsToHtml((DataTable) results[1], TrainingGrid);

        // update the title
        MyUserName.Text = Server.HtmlEncode(currentUser.Name);

        // if there is no training (i.e. if TrainingGrid contains only the header row), initially
		// hide TrainingGrid and show a message instructing the user to upload content
        if (TrainingGrid.Rows.Count == 1)
            TrainingPanel.Attributes.Add("style", "display: none");
		else
            NoTrainingMessage.Attributes.Add("style", "display: none");

        // since there is no initial selection, intially disable the "Delete Packages" link
        DeletePackagesLink.Attributes.Add("disabled", "true");

        // set the version number
        VersionNumber.Text = System.Text.RegularExpressions.Regex.Match(
			typeof(LearningStore).Assembly.FullName, @"\bVersion=(\d+\.\d+.\d+)\.").Groups[1].Value;
    }

    protected void DeletePackages_ServerClick(object sender, EventArgs e)
    {
        foreach (Control control in Form.Controls)
        {
            System.Diagnostics.Debug.WriteLine(control.ID);
        }
        return;
    }
}

