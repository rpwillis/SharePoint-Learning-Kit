using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Threading;
using Schema = Microsoft.SharePointLearningKit.Schema;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Xml;
using System.Xml.Schema;

using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint.Administration;
using Resources.Properties;

using System.Linq;
using InstallerHelper;

using System.Windows.Forms;

namespace CourseManagerConfCs
{

    /// <summary>
    /// Code-behind for Configure.aspx.
    /// </summary>
    public class ConfigurePage : System.Web.UI.Page
    {
        // controls
        protected System.Web.UI.WebControls.Label LabelErrorMessage;
        protected SiteAdministrationSelector SPSiteSelector;
        protected InputFormTextBox TxtDatabaseServer;
        protected InputFormTextBox TxtDatabaseName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chk")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Chk")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn")]
        protected System.Web.UI.WebControls.Button BtnOK;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn")]
        protected System.Web.UI.WebControls.Button BtnCancel;
        protected System.Web.UI.WebControls.Panel OperationCompletedPanel;
        protected Literal pageTitle;
        protected Literal pageTitleInTitlePage;
        protected Literal pageDescription;
        protected Image imagePleaseWait;
        protected System.Web.UI.WebControls.Label labelPleaseWait;
        protected Image imageConfigurationComplete;
        protected System.Web.UI.WebControls.Label labelConfigurationComplete;
        protected HyperLink linkConfigureAnother;
        protected InputFormSection inputSiteSelector;
        protected InputFormSection inputDatabase;
        protected System.Web.UI.WebControls.Label labelDatabaseDescription;
        protected InputFormControl inputDatabaseServer;
        protected InputFormControl inputDatabaseName;

        //Web Config Parameters Section
        protected InputFormSection inputWebConfigConnection;
        protected System.Web.UI.WebControls.Label labelWebConfigConnectionDescription;
        protected InputFormControl inputConnectionType;
        protected InputFormTextBox TxtDatabaseAccount;
        protected InputFormTextBox TxtDatabasePassword;

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        /// <summary>
        /// Prerender the CM Configure settings
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnPreRender(EventArgs e)
        {
            SetResourceText();

            bool enable = !String.IsNullOrEmpty(SPSiteSelector.CurrentId);
            EnableUi(enable, false);


            // nothing further to do if there's no SPSite specified
            if (!enable)
                return;

            // initialize form fields
            try
            {
                string databaseServer;
                string databaseName;
                bool createDatabase;
                string instructorPermission;
                string learnerPermission;
                string observerPermission;
                bool createPermissions;
            }
            catch (Exception ex)
            {
            }

            // let the base class do its thing
            base.OnPreRender(e);
        }

        /// <summary>
        /// New SPSite selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SPSiteSelector_OnContextChange(object sender, EventArgs e)
        {
            // the user selected a new SPSite
        }

        /// <summary>
        /// Realize the configure actions when the OK button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Btn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void BtnOk_Click(object sender, EventArgs e)
        {
            // if the user didn't select an SPSite, just display an error message and quit
            if (String.IsNullOrEmpty(SPSiteSelector.CurrentId))
            {
                return;
            }


            // process the form and redirect back to the Application Management page
            try
            {
                string schemaFileContents;
                schemaFileContents = File.ReadAllText(Server.MapPath("CourseManagerDBSchema.sql"));
                string connectionString = "Server=" + TxtDatabaseServer.Text + ";Database=master;Trusted_Connection=True;";

                // if <appPoolAccountName> is null, set it to the name of the application pool account
                // (e.g. "NT AUTHORITY\NETWORK SERVICE"); set <appPoolSid> to its SID
                byte[] appPoolSid = null;
                string appPoolAccountName = null;

                ImpersonateAppPool(delegate()
                {
                    WindowsIdentity appPool = WindowsIdentity.GetCurrent();
                    appPoolAccountName = appPool.Name;
                    appPoolSid = new byte[appPool.User.BinaryLength];
                    appPool.User.GetBinaryForm(appPoolSid, 0);
                    appPool.Impersonate();
                });

                string databaseName = TxtDatabaseName.Text;
                if (databaseName.Contains(" "))
                {
                    throw new FormatException("The database name can not have white spaces ' '.");
                }
                else
                {
                    ImpersonateAppPool(delegate()
                    {
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            // open the connection
                            sqlConnection.Open();

                            // create the database
                            using (SqlCommand command = new SqlCommand(
                                String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE {0}",
                                databaseName),
                                    sqlConnection))
                                command.ExecuteNonQuery();

                            // grant the application pool account access to SQL Server
                            using (SqlCommand command = new SqlCommand(
                                String.Format(CultureInfo.InvariantCulture,
                                    "IF NOT EXISTS(SELECT * FROM sys.server_principals WHERE sid=@sid) " +
                                    "BEGIN " +
                                    "CREATE LOGIN [{0}] FROM WINDOWS " +
                                    "END", appPoolAccountName), sqlConnection))
                            {
                                command.Parameters.AddWithValue("@sid", appPoolSid);
                                command.ExecuteNonQuery();
                            }
                        }

                        // perform operations that require a database connection string that specifies the SLK
                        // database
                        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                        {
                            // open the connection
                            sqlConnection.Open();

                            // grant the application pool account access to the database
                            int principalId;
                            using (SqlCommand command = new SqlCommand(
                                String.Format(CultureInfo.InvariantCulture,
                                    "IF NOT EXISTS(SELECT * from sys.database_principals WHERE sid=@sid) " +
                                    "BEGIN " +
                                    "CREATE USER [{0}] FOR LOGIN [{0}] " +
                                    "END " +
                                    "SELECT principal_id FROM sys.database_principals WHERE sid=@sid", appPoolAccountName), sqlConnection))
                            {
                                command.Parameters.AddWithValue("@sid", appPoolSid);
                                principalId = (int)command.ExecuteScalar();
                            }

                            // execute the SLK LearningStore schema script; we need to split the text based on
                            // "GO" commands, since "GO" isn't an actual SQL command
                            string[] schemaSqlBatches = schemaFileContents.Split(new string[] { "\r\nGO" },
                                StringSplitOptions.None);

                            foreach (string schemaSqlBatch in schemaSqlBatches)
                            {
                                if (schemaSqlBatch.Contains("INSERT INTO UILayouts"))
                                {
                                    //FileStream file = new FileStream(@"C:/path.txt", FileMode.Append, FileAccess.Write);
                                    //StreamWriter sw = new StreamWriter(file);
                                    //sw.Write(command.CommandText + "\r\n");
                                    //sw.Close();
                                }
                                string schemaSqlBatch2 = schemaSqlBatch.Replace("SLKCourseManager",
                                    databaseName);
                                using (SqlCommand command = new SqlCommand(schemaSqlBatch2, sqlConnection))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    });

                    #region Web Config
                    string sitecollection = SPSiteSelector.CurrentName;
                    string databaseserver = TxtDatabaseServer.Text;
                    TxtDatabaseServer.Text = "";
                    string databasename = databaseName;
                    TxtDatabaseName.Text = "";

                    InstallerHelperWebConf.DeleteWebConfigModificationsCache(sitecollection);
                    ConfigureAppSettings(sitecollection);
                    ConfigureHttpHandlers(sitecollection);
                    ConfigureSafeControls(sitecollection);
                    ConfigureSectionGroup(sitecollection);
                    ConfigureSection(sitecollection);
                    ConfigurePageControls(sitecollection);
                    ConfigureAddAssemblies(sitecollection);
                    ConfigureAssemblyBinding(sitecollection);
                    AddWebSiteGUID(sitecollection);


                    ConfigureConnectionStringIntegratedSecurity(sitecollection, databaseserver, databasename);
                    #endregion

                    BtnOK.Visible = true;
                    BtnOK.Enabled = true;
                    OperationCompletedPanel.Visible = true;
                }
            }
            catch (SafeToDisplayException ex)
            {
                // exception that's safe to show to browser user
                LabelErrorMessage.Text = Html(ex.Message);
            }
            catch (FormatException ex)
            {
                EnableUi(false, true);
                LabelErrorMessage.Text = Html(ex.Message);
                //String.Format(CultureInfo.CurrentCulture, AppResources.AdminGenericException, ex.Message));
                //SlkUtilities.LogEvent(EventLogEntryType.Error, "{0}", ex.Message);
            }
            catch (Exception ex)
            {
                // exception that may contain sensitive information -- since the user is an
                // administrator, we'll show them the error, but we'll write additional information
                // (e.g. stack trace) to the event log
                EnableUi(false, true);
                LabelErrorMessage.Text = Html(ex.ToString());
            }
        }

        /// <summary>
        /// Make the impersonation
        /// </summary>
        /// <param name="del"></param>
        internal static void ImpersonateAppPool(VoidDelegate del)
        {
            try
            {
                WindowsImpersonationContext m_context = null;
                try
                {
                    m_context = WindowsIdentity.Impersonate(IntPtr.Zero);
                    del();
                }
                finally
                {
                    if (m_context != null)
                        m_context.Dispose();
                }
            }
            catch
            {
                // prevent exception filter exploits
                throw;
            }
        }

        /// <summary>
        /// Enables or disables form UI.
        /// </summary>
        ///
        /// <param name="enable"><c>true</c> to enable form fields, <c>false</c> to disable them.
        /// 	</param>
        ///
        /// <param name="linkDefaultSettingsFileToo"><c>true</c> to enable or disable
        /// 	<c>LinkDefaultSettingsFile</c>, <c>false</c> to leave it as-is.</param>
        ///
        void EnableUi(bool enable, bool linkDefaultSettingsFileToo)
        {
            TxtDatabaseServer.Enabled = enable;
            TxtDatabaseName.Enabled = enable;

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
            pageTitle.Text = "Configure Course Manager";
            pageTitleInTitlePage.Text = "Configure Course Manager";
            imagePleaseWait.ToolTip = "Please wait while your changes are processed.";
            labelPleaseWait.Text = "Please wait while your changes are processed.";
            imageConfigurationComplete.ToolTip = "Configuration complete.";
            labelConfigurationComplete.Text = "Configuration complete.";
            linkConfigureAnother.Text = "Configure another Site Collection";
            inputSiteSelector.Title = "Site Collection";
            inputSiteSelector.Description = "Select a Site Collection to configure.";
            inputDatabase.Title = "SLK Course Manager Database";
            labelDatabaseDescription.Text = "Specify the Course Manager Database Server and Course Manager Database Name.";
            inputDatabaseServer.LabelText = "Database server:";
            inputDatabaseName.LabelText = "Database name:";
            TxtDatabaseServer.ToolTip = "Database Server";
            TxtDatabaseName.ToolTip = "Database Name";
            TxtDatabaseServer.Text = "";
            TxtDatabaseName.Text = "";
            BtnOK.Text = "OK";

        }



        /// <summary>
        /// write into the web config the website guid
        /// </summary>
        internal static void AddWebSiteGUID(string Direction)
        {

            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("appSettings", "configuration", 1, Modific);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();

            InstallerHelperWebConf.EditAppSetting(Direction, "WebSiteGUID", new SPSite(Direction).ID.ToString() );
        }

        /// <summary>
        /// Configure the AppSettings section of the web.config file
        /// </summary>
        /// <param name="Direction">Site to Configure</param>
        private void ConfigureAppSettings(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("appSettings", "configuration", 1, Modific);
            SPFarm.Local.Services.GetValue<SPWebService>().ApplyWebConfigModifications();
            webApp.Update();

            InstallerHelperWebConf.EditAppSetting(Direction, "FeedCacheTime", "300");
            InstallerHelperWebConf.EditAppSetting(Direction, "FeedPageUrl", "/_layouts/feed.aspx?");
            InstallerHelperWebConf.EditAppSetting(Direction, "FeedXsl1", "/Style Library/Xsl Style Sheets/Rss.xsl");
            InstallerHelperWebConf.EditAppSetting(Direction, "ReportViewerMessages", "Microsoft.SharePoint.Portal.Analytics.UI.ReportViewerMessages, Microsoft.SharePoint.Portal, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            InstallerHelperWebConf.EditAppSetting(Direction, "EnableSecurity", "false");
        }

        /// <summary>
        /// Configure the Connection Strings section of the web.config file
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        /// <param name="ServerName">Server on which the site is located</param>
        /// <param name="DatabaseName">Database to connect</param>
        /// <param name="UserId">Id to connect to the database</param>
        /// <param name="UserPassword">Password to connect to the database</param>
        private void ConfigureConnectionString(string Direction, string ServerName, string DatabaseName,
            string UserId, string UserPassword)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("connectionStrings", "configuration", 1, Modific);

            InstallerHelperWebConf.EditConnString(Direction, "Shared", ServerName, DatabaseName, UserId, UserPassword, "120", "System.Data.SqlClient");
            InstallerHelperWebConf.EditConnString(Direction, "SLKCM", ServerName, DatabaseName, UserId, UserPassword, "120", "System.Data.SqlClient");
        }

        /// <summary>
        /// Configure the Connection Strings (authenticated) section of the web.config file
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        /// <param name="ServerName">Server on which the site is located</param>
        /// <param name="DatabaseName">Database to connect</param>
        private void ConfigureConnectionStringIntegratedSecurity(string Direction, string ServerName, string DatabaseName)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;

            SPSite site = new SPSite(Direction);

            InstallerHelperWebConf.SetConnectionString("Shared" + site.ID.ToString(),
                "Data Source=" + ServerName + ";Initial Catalog=" + DatabaseName +
                ";Integrated Security=SSPI;Timeout=120",
                "System.Data.SqlClient");

            InstallerHelperWebConf.SetConnectionString("SLKCM" + site.ID.ToString(),
                "Data Source=" + ServerName + ";Initial Catalog=" + DatabaseName +
                ";Integrated Security=SSPI;Timeout=120",
                "System.Data.SqlClient");

        }


        /// <summary>
        /// Configure the HttpHandlers of the Web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureHttpHandlers(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("httpHandlers", "configuration/system.web", 1, Modific);

            //clsSharePointUtility.EditHttpHandlersRemove(Direction, "*", "*.asmx");
            InstallerHelperWebConf.EditHttpHandlers(Direction, "GET,HEAD", "ScriptResource.axd",
                "System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions", "3.5.0.0", "neutral",
                "31BF3856AD364E35", "false");
            InstallerHelperWebConf.EditHttpHandlers(Direction, "GET,HEAD,POST", "*/Images.asmx",
                "Axelerate.BusinessLayerUITools.ImageProcessing.ImageHandler, Axelerate.BusinessLayerUITools",
                "1.0.0.0", "neutral", "4f9a37917b02d1a2", "false");
            InstallerHelperWebConf.EditHttpHandlers(Direction, "GET,HEAD,POST", "*/ImgCmp.asmx",
                "Axelerate.BusinessLayerUITools.ImageProcessing.ImgCmpHandler, Axelerate.BusinessLayerUITools",
                "1.0.0.0", "neutral", "4f9a37917b02d1a2", "false");
        }

        /// <summary>
        /// Configure the SafeControl Section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureSafeControls(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("SafeControls", "configuration/SharePoint", 1, Modific);

            InstallerHelperWebConf.EditSafeControl(Direction, "System.Web.Extensions", "3.5.0.0", "neutral",
                "31BF3856AD364E35", "System.Web.UI", "*", "True");
        }

        /// <summary>
        /// Configure the SectionGroup Section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureSectionGroup(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("configSections", "configuration", 1, Modific);
            InstallerHelperWebConf.EditSectionGroup(Direction,
                "configuration/configSections",
                "System.Workflow.ComponentModel.WorkflowCompiler",
                "System.Workflow.ComponentModel.Compiler.WorkflowCompilerConfigurationSectionGroup, System.Workflow.ComponentModel",
                "3.0.0.0", "neutral", "31bf3856ad364e35");
            InstallerHelperWebConf.EditSectionGroup(Direction,
                "configuration/configSections",
                "system.web.extensions",
                "System.Web.Configuration.SystemWebExtensionsSectionGroup, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35");
            InstallerHelperWebConf.EditSectionGroup(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']",
                "scripting",
                "System.Web.Configuration.ScriptingSectionGroup, System.Web.Extensions", "3.5.0.0",
                "neutral", "31BF3856AD364E35");
            InstallerHelperWebConf.EditSectionGroup(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']",
                "webServices",
                "System.Web.Configuration.ScriptingWebServicesSectionGroup, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35");
        }

        /// <summary>
        /// Configure the section section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureSection(string Direction)
        {
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='System.Workflow.ComponentModel.WorkflowCompiler']",
                "authorizedTypes",
                "System.Workflow.ComponentModel.Compiler.AuthorizedTypesSectionHandler, System.Workflow.ComponentModel",
                "3.0.0.0", "neutral", "31bf3856ad364e35", "false", "Everywhere");
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']",
                "scriptResourceHandler",
                "System.Web.Configuration.ScriptingScriptResourceHandlerSection, System.Web.Extensions",
                "3.5.0.0", "neutral", "31bf3856ad364e35", "false", "MachineToApplication");
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                "jsonSerialization",
                "System.Web.Configuration.ScriptingJsonSerializationSection, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "Everywhere");
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                "profileService",
                "System.Web.Configuration.ScriptingProfileServiceSection, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                "authenticationService",
                "System.Web.Configuration.ScriptingAuthenticationServiceSection, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
            InstallerHelperWebConf.EditSection(Direction,
                "configuration/configSections/sectionGroup[@name='system.web.extensions']/sectionGroup[@name='scripting']/sectionGroup[@name='webServices']",
                "roleService",
                "System.Web.Configuration.ScriptingRoleServiceSection, System.Web.Extensions",
                "3.5.0.0", "neutral", "31BF3856AD364E35", "false", "MachineToApplication");
        }

        /// <summary>
        /// Configure the controls section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigurePageControls(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("controls", "configuration/system.web/pages", 1, Modific);

            InstallerHelperWebConf.EditControls(Direction, "configuration/system.web/pages/controls",
                "asp", "System.Web.UI.WebControls", "System.Web.Extensions", "3.5.0.0", "neutral",
                "31BF3856AD364E35");
            InstallerHelperWebConf.EditControls(Direction, "configuration/system.web/pages/controls",
                "asp", "System.Web.UI", "System.Web.Extensions", "3.5.0.0", "neutral", "31BF3856AD364E35");
        }

        /// <summary>
        /// Configure the AddAssemblies section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureAddAssemblies(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            InstallerHelperWebConf.CreateSection("assemblies", "configuration/system.web/compilation", 1, Modific);

            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "System.Xml.Linq", "3.5.0.0", "neutral", "B77A5C561934E089");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "Axelerate.BusinessLayerFrameWork", "1.3.0.1", "neutral", "783DBD9B30AFB604");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "Axelerate.SlkCourseManagerLogicalLayer", "1.0.0.0", "neutral", "5BBD900FCDE291A4");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "Axelerate.DataAccessApplicationBlock", "1.0.0.2", "neutral", "8AECA469E4966909");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "System.Core", "3.5.0.0", "neutral", "B77A5C561934E089");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "System.Web.Extensions", "3.5.0.0", "neutral", "31BF3856AD364E35");
            InstallerHelperWebConf.EditAddAssemblies(Direction, "configuration/system.web/compilation/assemblies",
                "System.Data.DataSetExtensions", "3.5.0.0", "neutral", "B77A5C561934E089");
        }

        /// <summary>
        /// Configure the AssemblyBinding section of the web.config
        /// </summary>
        /// <param name="Direction">Site to configure</param>
        private void ConfigureAssemblyBinding(string Direction)
        {
            SPWebApplication webApp = new SPSite(Direction).WebApplication;
            System.Collections.ObjectModel.Collection<SPWebConfigModification> Modific = webApp.WebConfigModifications;
            //clsSharePointUtility.CreateSection("assemblyBinding", "configuration/runtime", 1, Modific);

            InstallerHelperWebConf.EditNode("*[local-name()='dependentAssembly'][*/@name='System.Web.Extensions'][*/@publicKeyToken='31bf3856ad364e35'][*/@culture='neutral']",
                "configuration/runtime/*[local-name()='assemblyBinding' and namespace-uri()='urn:schemas-microsoft-com:asm.v1']",
                "<dependentAssembly><assemblyIdentity name='System.Web.Extensions' publicKeyToken='31bf3856ad364e35' culture='neutral' /><bindingRedirect oldVersion='1.0.0.0-1.1.0.0' newVersion='3.5.0.0' /></dependentAssembly>",
                2, Modific);
            InstallerHelperWebConf.EditNode("*[local-name()='dependentAssembly'][*/@name='System.Web.Extensions.Design'][*/@publicKeyToken='31bf3856ad364e35'][*/@culture='neutral']",
                "configuration/runtime/*[local-name()='assemblyBinding' and namespace-uri()='urn:schemas-microsoft-com:asm.v1']",
                "<dependentAssembly><assemblyIdentity name='System.Web.Extensions.Design' publicKeyToken='31bf3856ad364e35' culture='neutral' /><bindingRedirect oldVersion='1.0.0.0-1.1.0.0' newVersion='3.5.0.0' /></dependentAssembly>",
                2, Modific);
        }

    }

    /// <summary>
    /// Code-behind for DownloadSettings.aspx.
    /// </summary>
    public class DownloadSettingsPage : System.Web.UI.Page
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        public DownloadSettingsPage()
        {
            Load += new EventHandler(DownloadSettingsPage_Load);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void DownloadSettingsPage_Load(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    // The page URL is of the following two forms:
                    //   1. http://.../DownloadSettings.aspx/<guid>/SlkSettings.xml
                    //   2. http://.../DownloadSettings.aspx/Default/SlkSettings.xml
                    // The following code parses <guid> into <siteGuid> (for case 1), or sets
                    // <siteGuid> to null (for case 2).
                    Uri uri = Request.Url;
                    if ((uri.Segments.Length < 3) ||
                        !String.Equals(uri.Segments[uri.Segments.Length - 1], "SlkSettings.xml",
                            StringComparison.OrdinalIgnoreCase)) ;
                    string siteGuidOrDefault = uri.Segments[uri.Segments.Length - 2];
                    siteGuidOrDefault = siteGuidOrDefault.Substring(0, siteGuidOrDefault.Length - 1);
                    Guid? spSiteGuid;
                    if (String.Equals(siteGuidOrDefault, "Default",
                            StringComparison.OrdinalIgnoreCase))
                        spSiteGuid = null;
                    else
                    {
                        try
                        {
                            spSiteGuid = new Guid(siteGuidOrDefault);
                        }
                        catch (FormatException)
                        {
                            throw new SafeToDisplayException();
                        }
                        catch (OverflowException)
                        {
                            throw new SafeToDisplayException();
                        }
                    }

                    // set <settingXml> to the SLK Settings XML for <spSiteGuid> -- use the default
                    // SLK Settings XML if <spSiteGuid> is null or <spSiteGuid> is not configured for
                    // use with SLK
                    string settingsXml = null;
                    if (spSiteGuid != null)
                        settingsXml = SlkAdministration.GetSettingsXml(spSiteGuid.Value);
                    if (settingsXml == null)
                    {
                        // load the default SLK Settings
                        settingsXml = File.ReadAllText(Server.MapPath("SlkSettings.xml.dat"));
                    }

                    // write the XML to the browser
                    Response.ContentType = "text/xml";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(settingsXml);
                }
                catch (SafeToDisplayException ex)
                {
                    // an expected exception occurred
                    Response.Clear();
                    Response.ContentType = "text/html";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.End();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // thrown by Response.End above
                throw;
            }
            catch (Exception ex)
            {
                // an unexpected exception occurred
                Response.Clear();
                Response.ContentType = "text/html";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.End();
            }
        }
    }

}
