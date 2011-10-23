/* Copyright (c) Microsoft Corporation. All rights reserved. */

// Admin.cs
//
// SLK administration functionality.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.SharePointLearningKit.Schema;
using System.Configuration;

namespace Microsoft.SharePointLearningKit
{

    /// <summary>Performs SLK administration functions.</summary>
    public static class SlkAdministration
    {
#region public methods
        /// <summary>Loads SLK configuration information from WSS and LearningStore, in a form that's
        /// suitable for copying to Configure.aspx form fields. </summary>
        ///
        /// <param name="spSiteGuid">The GUID of the SPSite to retrieve configuration information
        ///     from.</param>
        ///
        /// <returns>An AdministrationConfiguration.</returns>
        ///
        /// <remarks>
        /// This method is static so it can used outside the context of IIS.  Only SharePoint
        /// administrators can perform this function.
        /// </remarks>
        ///
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.
        /// </exception>
        ///
        public static AdministrationConfiguration LoadConfiguration(Guid spSiteGuid)
        {
            AdministrationConfiguration configuration = new AdministrationConfiguration(spSiteGuid);
            // only SharePoint administrators can perform this action
            CheckPermissions();

            // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
            // information for that SPSite
            SlkSPSiteMapping mapping = SlkSPSiteMapping.GetMapping(spSiteGuid);

            // set "out" parameters based on <mappingExists> and <mapping>
            if (mapping != null)
            {
                // the mapping exists -- set "out" parameters based on <mapping>
                configuration.DatabaseServer = mapping.DatabaseServer;
                configuration.DatabaseName = mapping.DatabaseName;
                configuration.InstructorPermission = mapping.InstructorPermission;
                configuration.LearnerPermission = mapping.LearnerPermission;
                
                // The below given condition will be true only during the migration of SLK from 
                // 'SLK without Observer role' to 'SLK with Observer role' implementation
                if (mapping.ObserverPermission == null)
                {
                    mapping.ObserverPermission = AppResources.DefaultSlkObserverPermissionName;                
                }
                configuration.ObserverPermission = mapping.ObserverPermission;
            }
            else
            {
                configuration.IsNewConfiguration = true;
                mapping = SlkSPSiteMapping.CreateMapping(spSiteGuid);
                // the mapping doesn't exist -- set "out" parameters to default values
                SPWebService adminWebService = SlkAdministration.GetAdminWebService();
                configuration.DatabaseServer = adminWebService.DefaultDatabaseInstance.Server.Address;
                configuration.DatabaseName = AppResources.DefaultSlkDatabaseName;
                mapping.DatabaseServer = configuration.DatabaseServer;
                mapping.DatabaseName = configuration.DatabaseName;
                configuration.InstructorPermission = AppResources.DefaultSlkInstructorPermissionName;
                configuration.LearnerPermission = AppResources.DefaultSlkLearnerPermissionName;
                configuration.ObserverPermission = AppResources.DefaultSlkObserverPermissionName;
            }

            // set "out" parameters that need to be computed
            bool createDatabaseResult = false;
            SlkUtilities.ImpersonateAppPool(delegate()
            {
                createDatabaseResult = !DatabaseExists(mapping.DatabaseServerConnectionString, mapping.DatabaseName);
            });
            configuration.CreateDatabase = createDatabaseResult;

            return configuration;
        }

        /// <summary>
        /// Saves SLK configuration information.  This method accepts information in a form that's
        /// compatible with Configure.aspx form fields.
        /// </summary>
        ///
        /// <param name="spSiteGuid">The GUID of the SPSite being configured.</param>
        ///
        /// <param name="databaseServer">The name of the database server to associate with the
        ///     specified SPSite.  By default, integrated authentication is used to connect to the
        ///     database; to use a SQL Server user ID and password instead, append the appropriate
        ///     connection string information to the database server name -- for example, instead of
        ///     "MyServer", use "MyServer;user id=myacct;password=mypassword".  For security reasons,
        ///     integrated authentication is strongly recommended.</param>
        ///
        /// <param name="databaseName">The name of the database to associate with the specified
        ///     SPSite.  This database must exist if <paramref name="schemaToCreateDatabase"/> is
        ///     <c>null</c>, and must not exist if <paramref name="schemaToCreateDatabase"/> is
        ///     non-<c>null</c>, otherwise an error message is returned.</param>
        ///
        /// <param name="schemaToCreateDatabase">If non-<c>null</c>, this is the SlkSchema.sql file
        ///     containing the schema of the database, and an SLK database named
        ///     <paramref name="databaseName"/> is created using this schema.  If <c>null</c>,
        ///     <paramref name="databaseName"/> specifies an existing database.</param>
        ///
        /// <param name="instructorPermission">The name of the SharePoint permission that
        ///     identifies instructors.</param>
        ///
        /// <param name="learnerPermission">The name of the SharePoint permission that
        ///     identifies learners.</param>
        ///
        /// <param name="observerPermission">The name of the SharePoint permission that
        ///     identifies observers.</param>
        ///
        /// <param name="createPermissions">If <c>true</c>, the permissions specified by
        ///     <paramref name="instructorPermission"/> and <paramref name="learnerPermission"/>
        ///     are added to the root SPWeb of the specified SPSite (if they don't already
        ///     exist).</param>
        ///
        /// <param name="settingsFileContents">If not <c>null</c>, this is the contents of a SLK
        ///     Settings file to associate with this SPSite.  If <c>null</c>, the previous SLK
        ///     Settings file is used if one exists, or the default SLK settings file is used if a
        ///     database is being created.</param>
        /// 
        /// <param name="defaultSettingsFileContents">The contents of the default SLK Settings file.
        ///     Must not be <n>null</n>.</param>
        /// 
        /// <param name="appPoolAccountName">The name of the application pool account; for example,
        ///     "NT AUTHORITY\NETWORK SERVICE".  If <n>null</n>, then the current Windows identity is
        ///     used, or, if the current identity is impersonated, the original Windows identity is
        ///     used.</param>
        ///
        /// <remarks>
        /// This method is static so it can used outside the context of IIS.  Only SharePoint
        /// administrators can perform this function.
        /// </remarks>
        ///
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.
        /// </exception>
        ///

        public static void SaveConfiguration(Guid spSiteGuid, string databaseServer,
            string databaseName, string schemaToCreateDatabase, string instructorPermission,
            string learnerPermission, string observerPermission, bool createPermissions, string settingsFileContents,
            string defaultSettingsFileContents, string appPoolAccountName)
        {
            CheckParameters(databaseServer, databaseName, instructorPermission, learnerPermission, observerPermission, defaultSettingsFileContents);
            // only SharePoint administrators can perform this action
            CheckPermissions();

            // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection information for that SPSite
            SlkSPSiteMapping mapping = SlkSPSiteMapping.GetMapping(spSiteGuid);

            if (mapping == null)
            {
                mapping = SlkSPSiteMapping.CreateMapping(spSiteGuid);
            }

            mapping.DatabaseServer = databaseServer;
            mapping.DatabaseName = databaseName;
            mapping.InstructorPermission = instructorPermission;
            mapping.LearnerPermission = learnerPermission;
            mapping.ObserverPermission = observerPermission;

            if (mapping.IsDirty)
            {
                mapping.Update();
            }

            // create the database if specified
            if (schemaToCreateDatabase != null)
            {
                CreateDatabase(mapping, databaseName, appPoolAccountName, schemaToCreateDatabase);
            }

            // create permissions if specified
            if (createPermissions)
            {
                // create the permissions if they don't exist yet
                CreatePermission(spSiteGuid, instructorPermission, AppResources.SlkInstructorPermissionDescription, 0);
                CreatePermission(spSiteGuid, learnerPermission, AppResources.SlkLearnerPermissionDescription, 0);
                CreatePermission(spSiteGuid, observerPermission, AppResources.SlkObserverPermissionDescription, 0);
            }

            UpdateSlkSettings(mapping.DatabaseConnectionString, spSiteGuid, settingsFileContents, defaultSettingsFileContents);

        }

        /// <summary>
        /// Retrieves the SLK Settings XML for a given SPSite.
        /// </summary>
        /// 
        /// <param name="spSiteGuid">The GUID of the SPSite to retrieve SLK Settings for.</param>
        ///
        /// <returns>
        /// A string containing SLK Settings XML, or null if <pr>spSiteGuid</pr> is not configured for
        /// use with SLK.
        /// </returns>
        ///
        /// <remarks>
        /// This method is static so it can used outside the context of IIS.  Only SharePoint
        /// administrators can perform this function.
        /// </remarks>
        ///
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.
        /// </exception>
        /// 
        public static string GetSettingsXml(Guid spSiteGuid)
        {
            // only SharePoint administrators can perform this action
            CheckPermissions();

            // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
            // information for that SPSite
            SlkSPSiteMapping mapping = SlkSPSiteMapping.GetMapping(spSiteGuid);
            if (mapping == null)
            {
                return null;
            }

            // return the SLK Settings XML for this SPSite
            LearningStore learningStore = new LearningStore(mapping.DatabaseConnectionString,
                String.Empty, ImpersonationBehavior.UseOriginalIdentity);
            using (LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                LearningStoreJob job = learningStore.CreateJob();
                LearningStoreQuery query = learningStore.CreateQuery(Schema.SiteSettingsItem.ItemTypeName);
                query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
                query.AddCondition(Schema.SiteSettingsItem.SiteGuid,LearningStoreConditionOperator.Equal, spSiteGuid);
                job.PerformQuery(query);
                DataRowCollection dataRows = job.Execute<DataTable>().Rows;
                if (dataRows.Count != 1)
                {
                    throw new SafeToDisplayException(AppResources.SlkSettingsNotFound, spSiteGuid);
                }
                DataRow dataRow = dataRows[0];
                return (string) dataRow[0];
            }
        }
#endregion public methods

        /// <summary>Throws an exception if this computer isn't part of a SharePoint farm, or the caller doesn't
        /// have the necessary permissions to access instances of this class.</summary>
        static internal void CheckPermissions()
        {
            SPFarm farm = SPFarm.Local;
            if (farm == null)
            {
                throw new InvalidOperationException(AppResources.SharePointFarmNotFound);
            }
            else if (!farm.CurrentUserIsAdministrator())
            {
                throw new SafeToDisplayException(AppResources.NotSharePointAdmin);
            }
        }

        /// <summary> Returns an instance of the SharePoint Central Administration web service.</summary>
        internal static SPWebService GetAdminWebService()
        {
            SPAdministrationWebApplication adminWebApp = SPAdministrationWebApplication.Local;
            return (SPWebService) adminWebApp.Parent;
        }

    #region private methods

        static void UpdateSlkSettings(string connectionString, Guid spSiteGuid, string settingsFileContents, string defaultSettingsFileContents)
        {
            // make sure we can access LearningStore; while we're at it, find out if there's a row
            // corresponding to this SPSite in the SiteSettingsItem table
            LearningStore learningStore = new LearningStore(connectionString, "", true);
            LearningStoreJob job = learningStore.CreateJob();
            LearningStoreQuery query = learningStore.CreateQuery(Schema.SiteSettingsItem.ItemTypeName);
            query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
            query.AddCondition(Schema.SiteSettingsItem.SiteGuid, LearningStoreConditionOperator.Equal, spSiteGuid);
            job.PerformQuery(query);
            DataRowCollection results = job.Execute<DataTable>().Rows;
            if (results.Count == 0)
            {
                // this SPSite isn't listed in the SiteSettingsItem table, so we need to add a row
                if (settingsFileContents == null)
                {
                    settingsFileContents = defaultSettingsFileContents;
                }
            }
            else
            {
                object currentSettingsFileContents = results[0][Schema.SiteSettingsItem.SettingsXml];
                if ((currentSettingsFileContents == null) ||
                    (currentSettingsFileContents is DBNull) ||
                    (((string)currentSettingsFileContents).Length == 0))
                {
                    // the SLK Settings for this SPSite are missing, so we need to add them
                    if (settingsFileContents == null)
                    {
                        settingsFileContents = defaultSettingsFileContents;
                    }
                }
            }

            // upload the SLK Settings file if needed
            if (settingsFileContents != null)
            {
                // load "SlkSettings.xsd" from a resource into <xmlSchema>
                XmlSchema xmlSchema;
                using (StringReader schemaStringReader = new StringReader(
                    AppResources.SlkSettingsSchema))
                {
                    xmlSchema = XmlSchema.Read(schemaStringReader,
                        delegate(object sender2, ValidationEventArgs e2)
                        {
                            // ignore warnings (already displayed when SLK Settings file was uploaded)
                        });
                }

                // validate <settingsFileContents>
                using (StringReader stringReader = new StringReader(settingsFileContents))
                {
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    xmlSettings.Schemas.Add(xmlSchema);
                    xmlSettings.ValidationType = ValidationType.Schema;
                    using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlSettings))
                    {
                        try
                        {
                            SlkSettings settings = new SlkSettings(xmlReader, DateTime.MinValue);
                        }
                        catch (SlkSettingsException ex)
                        {
                            throw new SafeToDisplayException(AppResources.SlkSettingsFileError, ex.Message);
                        }
                    }
                }

                // store <settingsFileContents> in the database
                job = learningStore.CreateJob();
                Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
                uniqueProperties.Add(Schema.SiteSettingsItem.SiteGuid, spSiteGuid);
                Dictionary<string, object> updateProperties = new Dictionary<string, object>();
                updateProperties.Add(Schema.SiteSettingsItem.SettingsXml, settingsFileContents);
                updateProperties.Add(Schema.SiteSettingsItem.SettingsXmlLastModified,
                    DateTime.Now.ToUniversalTime());
                job.AddOrUpdateItem(Schema.SiteSettingsItem.ItemTypeName, uniqueProperties,
                    updateProperties);
                job.Execute();
            }
        }

        /// <summary>
        /// Adds a given permission to the root SPWeb of a given SPSite, if it doesn't exist.
        /// An optional set of base permissions are added to the permission.
        /// </summary>
        ///
        /// <param name="spSiteGuid">The GUID of the SPSite to add permissions to the root SPWeb of.
        ///     </param>
        ///
        /// <param name="permissionName">The name of the permission to create.</param>
        ///
        /// <param name="permissionDescription">The description of the permission to create.</param>
        /// 
        /// <param name="basePermissionsToAdd">Base permissions to add to the permission.  Use 0 if
        ///     no base permissions need to be added.</param>
        ///
        static void CreatePermission(Guid spSiteGuid, string permissionName,
            string permissionDescription, SPBasePermissions basePermissionsToAdd)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite spSite = new SPSite(spSiteGuid))
                {
                    using(SPWeb rootWeb = spSite.RootWeb)
                    {
                        SPRoleDefinitionCollection roleDefs = rootWeb.RoleDefinitions;
                        try
                        {
                            SPRoleDefinition roleDef = roleDefs[permissionName];
                            roleDef.BasePermissions |= basePermissionsToAdd;
                            roleDef.Update();
                            // permission already exists
                        }
                        catch (SPException)
                        {
                            // permission doesn't exist -- create it
                            SPRoleDefinition roleDef = new SPRoleDefinition();
                            roleDef.Name = permissionName;
                            roleDef.Description = permissionDescription;
                            roleDef.BasePermissions |= basePermissionsToAdd;
                            roleDefs.Add(roleDef);
                        }
                    }
                }
            });
        }

        static void CreateDatabase(SlkSPSiteMapping mapping, string databaseName, string appPoolAccountName, string databaseSchema)
        {
            // restrict the characters in <databaseName>
            if (!Regex.Match(databaseName, @"^\w+$").Success)
            {
                throw new SafeToDisplayException(AppResources.InvalidDatabaseName, databaseName);
            }

            // if <appPoolAccountName> is null, set it to the name of the application pool account
            // (e.g. "NT AUTHORITY\NETWORK SERVICE"); set <appPoolSid> to its SID
            byte[] appPoolSid = null;
            if (appPoolAccountName == null)
            {
                using (SPSite site = new SPSite(mapping.SPSiteGuid))
                {
                    appPoolAccountName = site.WebApplication.ApplicationPool.Username;
                }
            }

            NTAccount appPoolAccount = new NTAccount(appPoolAccountName);
            SecurityIdentifier securityId =
                (SecurityIdentifier)appPoolAccount.Translate(typeof(SecurityIdentifier));
            appPoolSid = new byte[securityId.BinaryLength];
            securityId.GetBinaryForm(appPoolSid, 0);

            SlkUtilities.ImpersonateAppPool(delegate()
            {
                CreateDatabase(mapping.DatabaseServerConnectionString, databaseName,
                    mapping.DatabaseConnectionString, appPoolAccountName,
                    appPoolSid, databaseSchema);
            });
        }

        /// <summary>
        /// Create a database and give the application pool account access to it
        /// </summary>
        /// 
        /// <param name="databaseServerConnectionString">Connection string for
        ///     connecting to the server.</param>
        /// 
        /// <param name="databaseName">Database name.</param>
        /// 
        /// <param name="databaseConnectionString">Connection string for
        ///     connecting to the database.</param>
        /// 
        /// <param name="appPoolAccountName">Name of the application pool account.
        ///     </param>
        /// 
        /// <param name="appPoolSid">Sid of the application pool account.</param>
        /// 
        /// <param name="databaseSchema">Schema for the database.</param>
        /// 
        static void CreateDatabase(string databaseServerConnectionString, string databaseName,
            string databaseConnectionString, string appPoolAccountName, byte[] appPoolSid,
            string databaseSchema)
        {
            // perform operations that require a database connection string that doesn't specify a
            // particular database
            using (SqlConnection sqlConnection = new SqlConnection(databaseServerConnectionString))
            {
                // open the connection
                sqlConnection.Open();

                // create the database
                string createDatabaseSql = String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE {0}", databaseName);
                using (SqlCommand command = new SqlCommand(createDatabaseSql, sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
                
                // grant the application pool account access to SQL Server
                string grantAccessSql = @"IF NOT EXISTS(SELECT * FROM sys.server_principals WHERE sid=@sid) 
                                            BEGIN 
                                            CREATE LOGIN [{0}] FROM WINDOWS
                                            END";
                grantAccessSql = string.Format(CultureInfo.InvariantCulture, grantAccessSql, appPoolAccountName);

                using (SqlCommand command = new SqlCommand(grantAccessSql, sqlConnection))
                {
                    command.Parameters.AddWithValue("@sid", appPoolSid);
                    command.ExecuteNonQuery();
                }

            }

            // perform operations that require a database connection string that specifies the SLK
            // database
            using (SqlConnection sqlConnection = new SqlConnection(databaseConnectionString))
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
                string[] schemaSqlBatches = databaseSchema.Split(new string[] { "\r\nGO" },
                    StringSplitOptions.None);
                foreach(string schemaSqlBatch in schemaSqlBatches)
                {
                    using(SqlCommand command = new SqlCommand(schemaSqlBatch, sqlConnection))
                        command.ExecuteNonQuery();
                }

                // add the application pool account to the "LearningStore" role, but only
                // if it isn't dbo
                if(principalId != 1)
                {
                    using (SqlCommand command = new SqlCommand("sp_addrolemember", sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@rolename", "LearningStore");
                        command.Parameters.AddWithValue("@membername", appPoolAccountName);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns <c>true</c> if the database specified by <c>DatabaseServer</c> and
        /// <c>DatabaseName</c> exists, <c>false</c> if not, or if either <c>DatabaseServer</c> or
        /// <c>DatabaseName</c> is <c>null</c>, or if the database server is inaccessible.
        /// </summary>
        ///
        /// <param name="connectionString">Connection string for the server.</param>
        /// 
        /// <param name="databaseName">Database name.</param>
        /// 
        static bool DatabaseExists(string connectionString, string databaseName)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                using (sqlConnection)
                {
                    // Open the connection
                    sqlConnection.Open();

                    // Get information about the database
                    using (SqlCommand command = new SqlCommand(
                        String.Format(CultureInfo.InvariantCulture,
                            "SELECT CASE WHEN EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE name = '{0}') THEN 1 ELSE 0 END",
                            databaseName), sqlConnection))
                    {
                        return (((int)command.ExecuteScalar()) != 0) ? true : false;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
            catch (ArgumentException) // e.g. invalid connection string parameter
            {
                return false;
            }
        }

        static void CheckParameters(string databaseServer, string databaseName, string instructorPermission, string learnerPermission, string observerPermission, string defaultSettingsFileContents)
        {
            if (databaseServer == null)
            {
                throw new ArgumentNullException("databaseServer");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (instructorPermission == null)
            {
                throw new ArgumentNullException("instructorPermission");
            }
            if (learnerPermission == null)
            {
                throw new ArgumentNullException("learnerPermission");
            }
            if (observerPermission == null)
            {
                throw new ArgumentNullException("observerPermission");
            }
            if (defaultSettingsFileContents == null)
            {
                throw new ArgumentNullException("defaultSettingsFileContents");
            }
        }
    #endregion private methods

    }

}

