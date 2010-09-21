/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Configure.aspx.cs
//
// Code-behind for Configure.aspx (used in SharePoint Central Administration to configure SLK).
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using System.Threading;
using Schema = Microsoft.SharePointLearningKit.Schema;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.SharePointLearningKit.AdminPages
{

    /// <summary>
    /// Code-behind for Configure.aspx.
    /// </summary>
    public class ConfigurePage : System.Web.UI.Page
    {
#region fields
        #pragma warning disable 1591
        protected Label LabelErrorMessage;
        protected SiteAdministrationSelector SPSiteSelector;
        protected InputFormTextBox TxtDatabaseServer;
        protected InputFormTextBox TxtDatabaseName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chk")]
        protected InputFormCheckBox ChkCreateDatabase;
        protected InputFormTextBox TxtInstructorPermission;
        protected InputFormTextBox TxtLearnerPermission;
        protected InputFormTextBox TxtObserverPermission;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chk")]
        protected InputFormCheckBox ChkCreatePermissions;
        protected HyperLink LinkCurrentSettingsFile;
        protected HyperLink LinkDefaultSettingsFile;
        protected FileUpload FileUploadSlkSettings;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn")]
        protected Button BtnOK;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn")]
        protected Button BtnCancel;
        protected Panel OperationCompletedPanel;
        protected Literal pageTitle;
        protected Literal pageTitleInTitlePage;
        protected Literal pageDescription;
        protected Image imagePleaseWait;
        protected Label labelPleaseWait;
        protected Image imageConfigurationComplete;
        protected Label labelConfigurationComplete;
        protected HyperLink linkConfigureAnother;
        protected InputFormSection inputSiteSelector;
        protected InputFormSection inputDatabase;
        protected InputFormSection inputPermissions;
        protected Label labelDatabaseDescription;
        protected Label labelPermissionsDescription;
        protected InputFormControl inputDatabaseServer;
        protected InputFormControl inputInstructorPermissions;
        protected InputFormControl inputLearnerPermissions;
        protected InputFormControl inputObserverPermissions;
        protected InputFormControl inputSlkSettingsFile;
        protected Label labelSlkSettingDescription;
        protected InputFormSection inputSlkSettings;
        #pragma warning restore 1591
#endregion fields
        
#region protected methods
        /// <summary>The OnPreRender event.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnPreRender(EventArgs e)
        {
            SetResourceText();

            // enable form controls only if an SPSite is specified; exception:<LinkDefaultSettingsFile> may always be enabled
            bool enable = String.IsNullOrEmpty(SPSiteSelector.CurrentId) == false;
            EnableUi(enable, false);

            LinkDefaultSettingsFile.NavigateUrl = "DownloadSettings.aspx/Default/SlkSettings.xml";

            // update the URL of <LinkCurrentSettingsFile> based on the selected SPSite (if any)
            if (enable)
            {
                LinkCurrentSettingsFile.NavigateUrl = String.Format(CultureInfo.InvariantCulture, "DownloadSettings.aspx/{0}/SlkSettings.xml", SPSiteSelector.CurrentId);
            }

            if (enable == true)
            {
                InitializeFormFields();
            }

            base.OnPreRender(e);
        }

        /// <summary>Event raised when a new SPSite is chosen.</summary>
        protected void SPSiteSelector_OnContextChange(object sender, EventArgs e)
        {
            // the user selected a new SPSite. Work is in OnPreRender.
        }

        /// <summary>Event raised by OK button.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void BtnOk_Click(object sender, EventArgs e)
        {
            // if the user didn't select an SPSite, just display an error message and quit
            if (String.IsNullOrEmpty(SPSiteSelector.CurrentId))
            {
                LabelErrorMessage.Text = AppResources.SelectSiteCollectionHtml;
                return;
            }

            ProcessForm();
        }
#endregion protected methods

#region private members
        void ProcessForm()
        {
            try
            {
                // if the user requested that a database be created, load the contents of the SLK
                // LearningStore schema file into <schemaFileContents>, otherwise set
                // <schemaFileContents> to null
                string schemaFileContents = null;
                if (ChkCreateDatabase.Checked)
                {
                    schemaFileContents = File.ReadAllText(Server.MapPath("SlkSchema.sql"));
                }

                // if an SLK Settings file was uploaded, set <settingsFileContents> to its contents,
                // otherwise set <settingsFileContents> to null 
                string settingsFileContents = null;

                if (FileUploadSlkSettings.HasFile)
                {
                    using (StreamReader streamReader = new StreamReader(FileUploadSlkSettings.FileContent))
                    {
                        settingsFileContents = streamReader.ReadToEnd();
                    }
                }
                else
                {
                    if (FileUploadSlkSettings.FileName.Length > 0)
                    {
                        throw new SafeToDisplayException(AppResources.InvalidSlkSettingsFilePath);
                    }
                }

                // set <defaultSettingsFileContents> to the default SLK Settings file, which is stored
                // with a ".dat" extension to prevent it from being downloadable (for security reasons)
                string defaultSettingsFileContents = File.ReadAllText(Server.MapPath("SlkSettings.xml.dat"));

                // save the SLK configuration for this SPSite
                SlkAdministration.SaveConfiguration(new Guid(SPSiteSelector.CurrentId),
                    TxtDatabaseServer.Text, TxtDatabaseName.Text, schemaFileContents,
                    TxtInstructorPermission.Text, TxtLearnerPermission.Text, TxtObserverPermission.Text,
                    ChkCreatePermissions.Checked, settingsFileContents, defaultSettingsFileContents,
                    null, ImpersonationBehavior.UseOriginalIdentity);

                BtnOK.Visible = false;
                OperationCompletedPanel.Visible = true;
            }
            catch (SafeToDisplayException ex)
            {
                // exception that's safe to show to browser user
                LabelErrorMessage.Text = Html(ex.Message);
            }
            catch (Exception ex)
            {
                // exception that may contain sensitive information -- since the user is an
                // administrator, we'll show them the error, but we'll write additional information
                // (e.g. stack trace) to the event log
                EnableUi(false, true);
                LabelErrorMessage.Text = Html(String.Format(CultureInfo.CurrentCulture, AppResources.AdminGenericException, ex.Message));
                SlkUtilities.LogEvent(EventLogEntryType.Error, "{0}", ex.ToString());
            }
        }

        void InitializeFormFields()
        {
            try
            {
                Guid siteGuid = string.IsNullOrEmpty(SPSiteSelector.CurrentId) ? Guid.Empty : new Guid(SPSiteSelector.CurrentId);
                AdministrationConfiguration configuration = SlkAdministration.LoadConfiguration(siteGuid);

                TxtDatabaseServer.Text = configuration.DatabaseServer;
                TxtDatabaseName.Text = configuration.DatabaseName;
                ChkCreateDatabase.Checked = configuration.CreateDatabase;
                TxtInstructorPermission.Text = configuration.InstructorPermission;
                TxtLearnerPermission.Text = configuration.LearnerPermission;
                TxtObserverPermission.Text = configuration.ObserverPermission;
                ChkCreatePermissions.Checked = configuration.CreatePermissions;
            }
            catch (SafeToDisplayException ex)
            {
                // exception that's safe to show to browser user
                LabelErrorMessage.Text = Html(ex.Message);
            }
            catch (Exception ex)
            {
                // exception that may contain sensitive information -- since the user is an
                // administrator, we'll show them the error, but we'll write additional information
                // (e.g. stack trace) to the event log
                LabelErrorMessage.Text = Html(String.Format(CultureInfo.CurrentCulture, AppResources.AdminGenericException, ex.Message));
                SlkUtilities.LogEvent(EventLogEntryType.Error, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Enables or disables form UI.
        /// </summary>
        ///
        /// <param name="enable"><c>true</c> to enable form fields, <c>false</c> to disable them.
        /// </param>
        ///
        /// <param name="linkDefaultSettingsFileToo"><c>true</c> to enable or disable
        /// <c>LinkDefaultSettingsFile</c>, <c>false</c> to leave it as-is.</param>
        ///
        void EnableUi(bool enable, bool linkDefaultSettingsFileToo)
        {
            TxtDatabaseServer.Enabled = enable;
            TxtDatabaseName.Enabled = enable;
            ChkCreateDatabase.Enabled = enable;
            TxtInstructorPermission.Enabled = enable;
            TxtLearnerPermission.Enabled = enable;
            TxtObserverPermission.Enabled = enable;
            ChkCreatePermissions.Enabled = enable;
            LinkCurrentSettingsFile.Enabled = enable;
            if (linkDefaultSettingsFileToo)
            {
                LinkDefaultSettingsFile.Enabled = enable;
            }
            FileUploadSlkSettings.Enabled = enable;
            BtnOK.Enabled = enable;
        }

        /// <summary>
        /// Converts plain text to HTML.  Newlines are converted to HTML line breaks.
        /// </summary>
        ///
        string Html(string text)
        {
            return Server.HtmlEncode(text).Replace("\r\n", "<br>\r\n");
        }

        /// <summary>
        /// Copies localizable strings from string resources to UI.
        /// </summary>
        private void SetResourceText()
        {
            AppResources.Culture = LocalizationManager.GetCurrentCulture();

            pageTitle.Text = AppResources.ConfigureTitle;
            pageTitleInTitlePage.Text = AppResources.ConfigureTitle;
            pageDescription.Text = AppResources.ConfigureDescription;
            imagePleaseWait.ToolTip = AppResources.ConfigureImagePleaseWait;
            labelPleaseWait.Text = AppResources.ConfigureLabelPleaseWait;
            imageConfigurationComplete.ToolTip = AppResources.ConfigureImageConfigurationComplete;
            labelConfigurationComplete.Text = AppResources.ConfigureLabelConfigurationComplete;
            linkConfigureAnother.Text = AppResources.ConfigureLinkConfigureAnother;
            inputSiteSelector.Title = AppResources.ConfigureInputSiteSelectorTitle;
            inputSiteSelector.Description = AppResources.ConfigureInputSiteSelectorDescription;
            inputDatabase.Title = AppResources.ConfigureInputDatabaseTitle;
            labelDatabaseDescription.Text = AppResources.ConfigureLabelDatabaseDescriptionText;
            inputDatabaseServer.LabelText = AppResources.ConfigureInputDatabaseServerLabelText;
            TxtDatabaseServer.ToolTip = AppResources.ConfigureTxtDatabaseServerToolTip;
            TxtDatabaseName.ToolTip = AppResources.ConfigureTxtDatabaseNameToolTip;
            ChkCreateDatabase.LabelText = AppResources.ConfigureChkCreateDatabaseLabelText;
            inputPermissions.Title = AppResources.ConfigureInputPermissionsTitle;
            labelPermissionsDescription.Text = AppResources.ConfigureLabelPermissionsDescriptionText;
            inputInstructorPermissions.LabelText = AppResources.ConfigureInputInstructorPermissionsLabelText;
            TxtInstructorPermission.ToolTip = AppResources.ConfigureTxtInstructorPermissionToolTip;
            inputLearnerPermissions.LabelText = AppResources.ConfigureInputLearnerPermissionsLabelText;
            TxtLearnerPermission.ToolTip = AppResources.ConfigureTxtLearnerPermissionToolTip;
            inputObserverPermissions.LabelText = AppResources.ConfigureInputObserverPermissionsLabelText;
            TxtObserverPermission.ToolTip = AppResources.ConfigureTxtObserverPermissionToolTip;
            ChkCreatePermissions.LabelText = AppResources.ConfigureChkCreatePermissionsLabelText;
            inputSlkSettings.Title = AppResources.ConfigureInputSlkSettingsTitle;
            labelSlkSettingDescription.Text = AppResources.ConfigureLabelSlkSettingDescriptionText;
            LinkCurrentSettingsFile.Text = AppResources.ConfigureLinkCurrentSettingsFileText;
            LinkDefaultSettingsFile.Text = AppResources.ConfigureLinkDefaultSettingsFileText;
            inputSlkSettingsFile.LabelText = AppResources.ConfigureInputSlkSettingsFileLabelText;
            BtnOK.Text = AppResources.ConfigureBtnOKText;
        }
#endregion private members
    }
}

