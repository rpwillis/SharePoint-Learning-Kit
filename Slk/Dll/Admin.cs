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

namespace Microsoft.SharePointLearningKit
{

/// <summary>
/// Performs SLK administration functions.
/// </summary>
///
public static class SlkAdministration
{
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

	/// <summary>
	/// Loads SLK configuration information from WSS and LearningStore, in a form that's
	/// suitable for copying to Configure.aspx form fields.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite to retrieve configuration information
	/// 	from.</param>
	///
	/// <param name="databaseServer">Set to the name of the database server associated with
	/// 	the specified SPSite.  If no database is currently associated with
	/// 	<paramref name="spSiteGuid"/>, this parameter is set to the name of the database
    ///     server containing the SharePoint configuration database.
	/// 	</param>
	///
	/// <param name="databaseName">Set to the name of the database associated with the
	/// 	specified SPSite.  If no database is currently associated with
	/// 	<paramref name="spSiteGuid"/>, this parameter is set to a default database name.
	/// 	</param>
	///
	/// <param name="createDatabase">Set to <c>true</c> if the database specified by the values
	/// 	returned in <paramref name="databaseServer"/> and <paramref name="databaseName"/>
	/// 	currently exists, <c>false</c> if not.  This can be used as the default value for the
	/// 	"Create a new database" checkbox in Configure.aspx.</param>
	///
	/// <param name="instructorPermission">Set to the name of the SharePoint permission that
	/// 	identifies instructors.  If no database is currently associated with the specified
	/// 	SPSite, this is set to a default value such as "SLK Instructor".</param>
	///
	/// <param name="learnerPermission">Set to the name of the SharePoint permission that
	/// 	identifies learners.  If no database is currently associated with the specified
	/// 	SPSite, this is set to a default value such as "SLK Learner".</param>
	///
	/// <param name="createPermissions">Set to <c>false</c> if both the permission values returned
	/// 	in parameters <paramref name="instructorPermission"/> and
	/// 	<paramref name="learnerPermission"/> already exist in the root SPWeb of the
	/// 	specified SPSite, <c>true</c> otherwise.  This can be used as the default value
	/// 	for the "Create permissions" checkbox in Configure.aspx.</param>
    /// 
    /// <returns>
    /// <c>true</c> if a mapping between <paramref name="spSiteGuid"/> and a SharePoint Learning
	/// Kit database was found in the SharePoint configuration database, <c>false</c> if not.
	/// In the latter case, the <c>out</c> parameters are set to default values.
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
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool LoadConfiguration(Guid spSiteGuid, out string databaseServer,
		out string databaseName, out bool createDatabase, out string instructorPermission,
		out string learnerPermission, out bool createPermissions)
	{
		// only SharePoint administrators can perform this action
        EnsureFarmAdmin();

        // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
        // information for that SPSite
        SlkSPSiteMapping mapping;
        bool mappingExists = SlkSPSiteMapping.GetMapping(spSiteGuid, out mapping);

        // set "out" parameters based on <mappingExists> and <mapping>
		if (mappingExists)
		{
			// the mapping exists -- set "out" parameters based on <mapping>
            databaseServer = mapping.DatabaseServer;
            databaseName = mapping.DatabaseName;
		    instructorPermission = mapping.InstructorPermission;
		    learnerPermission = mapping.LearnerPermission;
		}
		else
		{
			// the mapping doesn't exist -- set "out" parameters to default values
            SPWebService adminWebService = SlkAdministration.GetAdminWebService();
            databaseServer = adminWebService.DefaultDatabaseInstance.Server.Address;
            databaseName = AppResources.DefaultSlkDatabaseName;
            mapping.DatabaseServer = databaseServer;
            mapping.DatabaseName = databaseName;
            instructorPermission = AppResources.DefaultSlkInstructorPermissionName;
            learnerPermission = AppResources.DefaultSlkLearnerPermissionName;
        }

        // set "out" parameters that need to be computed
        bool createDatabaseResult = false;
        SlkUtilities.ImpersonateAppPool(delegate()
        {
            createDatabaseResult = !DatabaseExists(mapping.DatabaseServerConnectionString, mapping.DatabaseName);
        });
        createDatabase = createDatabaseResult;
		createPermissions = !PermissionsExist(spSiteGuid, instructorPermission, learnerPermission);

		return mappingExists;
	}

	/// <summary>
	/// Saves SLK configuration information.  This method accepts information in a form that's
	/// compatible with Configure.aspx form fields.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite being configured.</param>
	///
	/// <param name="databaseServer">The name of the database server to associate with the
	/// 	specified SPSite.  By default, integrated authentication is used to connect to the
	/// 	database; to use a SQL Server user ID and password instead, append the appropriate
	/// 	connection string information to the database server name -- for example, instead of
	/// 	"MyServer", use "MyServer;user id=myacct;password=mypassword".  For security reasons,
	/// 	integrated authentication is strongly recommended.</param>
	///
	/// <param name="databaseName">The name of the database to associate with the specified
	/// 	SPSite.  This database must exist if <paramref name="schemaToCreateDatabase"/> is
	/// 	<c>null</c>, and must not exist if paramref name="schemaToCreateDatabase"/> is
	/// 	non-<c>null</c>, otherwise an error message is returned.</param>
	///
	/// <param name="schemaToCreateDatabase">If non-<c>null</c>, this is the SlkSchema.sql file
	/// 	containing the schema of the database, and an SLK database named
	/// 	<paramref name="databaseName"/> is created using this schema.  If <c>null</c>,
	/// 	<paramref name="databaseName"/> specifies an existing database.</param>
	///
	/// <param name="instructorPermission">The name of the SharePoint permission that
	/// 	identifies instructors.</param>
	///
	/// <param name="learnerPermission">The name of the SharePoint permission that
	/// 	identifies learners.</param>
	///
	/// <param name="createPermissions">If <c>true</c>, the permissions specified by
	/// 	<paramref name="instructorPermission"/> and <paramref name="learnerPermission"/>
	/// 	are added to the root SPWeb of the specified SPSite (if they don't already
	/// 	exist).</param>
	///
	/// <param name="settingsFileContents">If not <c>null</c>, this is the contents of a SLK
	/// 	Settings file to associate with this SPSite.  If <c>null</c>, the previous SLK
	/// 	Settings file is used if one exists, or the default SLK settings file is used if a
	/// 	database is being created.</param>
    /// 
    /// <param name="defaultSettingsFileContents">The contents of the default SLK Settings file.
    ///     Must not be <n>null</n>.</param>
    /// 
    /// <param name="appPoolAccountName">The name of the application pool account; for example,
	/// 	"NT AUTHORITY\NETWORK SERVICE".  If <n>null</n>, then the current Windows identity is
	/// 	used, or, if the current identity is impersonated, the original Windows identity is
	/// 	used.</param>
	///
    /// <param name="createDatabaseImpersonationBehavior">Identifies which <c>WindowsIdentity</c>
    ///     is used to create the database.</param>
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
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public static void SaveConfiguration(Guid spSiteGuid, string databaseServer,
		string databaseName, string schemaToCreateDatabase, string instructorPermission,
		string learnerPermission, bool createPermissions, string settingsFileContents,
        string defaultSettingsFileContents, string appPoolAccountName,
        ImpersonationBehavior createDatabaseImpersonationBehavior)
	{
        // Check parameters
        if (databaseServer == null)
            throw new ArgumentNullException("databaseServer");
        if (databaseName == null)
            throw new ArgumentNullException("databaseName");
        if (instructorPermission == null)
            throw new ArgumentNullException("instructorPermission");
        if (learnerPermission == null)
            throw new ArgumentNullException("learnerPermission");
        if (defaultSettingsFileContents == null)
            throw new ArgumentNullException("defaultSettingsFileContents");
            
        // only SharePoint administrators can perform this action
        EnsureFarmAdmin();

        // check arguments
        if (defaultSettingsFileContents == null)
            throw new ArgumentNullException("defaultSettingsFileContents");

        // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
        // information for that SPSite
        SlkSPSiteMapping mapping;
        bool mappingExists = SlkSPSiteMapping.GetMapping(spSiteGuid, out mapping);

        // update <mapping>
		bool mappingChanged = false;
        if (mapping.DatabaseServer != databaseServer)
		{
			mapping.DatabaseServer = databaseServer;
			mappingChanged = true;
		}
        if (mapping.DatabaseName != databaseName)
		{
			mapping.DatabaseName = databaseName;
			mappingChanged = true;
		}
        if (mapping.InstructorPermission != instructorPermission)
		{
			mapping.InstructorPermission = instructorPermission;
			mappingChanged = true;
		}
        if (mapping.LearnerPermission != learnerPermission)
		{
			mapping.LearnerPermission = learnerPermission;
			mappingChanged = true;
		}
		if (mappingChanged || !mappingExists)
			mapping.Update();

		// create the database if specified
		if (schemaToCreateDatabase != null)
		{
			// restrict the characters in <databaseName>
			if (!Regex.Match(databaseName, @"^\w+$").Success)
				throw new SafeToDisplayException(AppResources.InvalidDatabaseName, databaseName);

			// if <appPoolAccountName> is null, set it to the name of the application pool account
			// (e.g. "NT AUTHORITY\NETWORK SERVICE"); set <appPoolSid> to its SID
			byte[] appPoolSid = null;
            if (appPoolAccountName == null)
            {
                SlkUtilities.ImpersonateAppPool(delegate()
                {
                    WindowsIdentity appPool = WindowsIdentity.GetCurrent();
                    appPoolAccountName = appPool.Name;
                    appPoolSid = new byte[appPool.User.BinaryLength];
                    appPool.User.GetBinaryForm(appPoolSid, 0);
                });
            }
            else
            {
                NTAccount appPoolAccount = new NTAccount(appPoolAccountName);
                SecurityIdentifier securityId =
                    (SecurityIdentifier) appPoolAccount.Translate(typeof(SecurityIdentifier));
                appPoolSid = new byte[securityId.BinaryLength];
                securityId.GetBinaryForm(appPoolSid, 0);
            }

            switch (createDatabaseImpersonationBehavior)
            {
                case ImpersonationBehavior.UseImpersonatedIdentity:
                    CreateDatabase(mapping.DatabaseServerConnectionString, databaseName,
                        mapping.DatabaseConnectionString, appPoolAccountName,
                        appPoolSid, schemaToCreateDatabase);
                    break;

                case ImpersonationBehavior.UseOriginalIdentity:
                    SlkUtilities.ImpersonateAppPool(delegate()
                    {
                        CreateDatabase(mapping.DatabaseServerConnectionString, databaseName,
                            mapping.DatabaseConnectionString, appPoolAccountName,
                            appPoolSid, schemaToCreateDatabase);
                    });
                    break;
                    
                default:
                    throw new InternalErrorException("SLK1100");
            }
		}

		// create permissions if specified
		if (createPermissions)
		{
			// create the permissions if they don't exist yet
            CreatePermission(spSiteGuid, instructorPermission,
                AppResources.SlkInstructorPermissionDescription, 0);
            CreatePermission(spSiteGuid, learnerPermission,
				AppResources.SlkLearnerPermissionDescription, 0);
        }

        // make sure we can access LearningStore; while we're at it, find out if there's a row
        // corresponding to this SPSite in the SiteSettingsItem table
        LearningStore learningStore = new LearningStore(mapping.DatabaseConnectionString, "", true);
        LearningStoreJob job = learningStore.CreateJob();
        LearningStoreQuery query = learningStore.CreateQuery(
            Schema.SiteSettingsItem.ItemTypeName);
        query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
        query.AddCondition(Schema.SiteSettingsItem.SiteGuid,
            LearningStoreConditionOperator.Equal, spSiteGuid);
        job.PerformQuery(query);
        DataRowCollection results = job.Execute<DataTable>().Rows;
        if (results.Count == 0)
        {
            // this SPSite isn't listed in the SiteSettingsItem table, so we need to add a row
            if (settingsFileContents == null)
                settingsFileContents = defaultSettingsFileContents;
        }
        else
        {
            object currentSettingsFileContents =
                results[0][Schema.SiteSettingsItem.SettingsXml];
            if ((currentSettingsFileContents == null) ||
                (currentSettingsFileContents is DBNull) ||
                (((string) currentSettingsFileContents).Length == 0))
			{
				// the SLK Settings for this SPSite are missing, so we need to add them
				if (settingsFileContents == null)
					settingsFileContents = defaultSettingsFileContents;
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
                        SlkSettings.ParseSettingsFile(xmlReader,
                            DateTime.MinValue);
                    }
                    catch (SlkSettingsException ex)
                    {
                        throw new SafeToDisplayException(AppResources.SlkSettingsFileError,
                            ex.Message);
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
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static string GetSettingsXml(Guid spSiteGuid)
    {
        // only SharePoint administrators can perform this action
        EnsureFarmAdmin();

        // set <mapping> to the mapping between <spSiteGuid> and the LearningStore connection
        // information for that SPSite
        SlkSPSiteMapping mapping;
        if (!SlkSPSiteMapping.GetMapping(spSiteGuid, out mapping))
            return null;

		// return the SLK Settings XML for this SPSite
		LearningStore learningStore = new LearningStore(mapping.DatabaseConnectionString,
			String.Empty, ImpersonationBehavior.UseOriginalIdentity);
		using (LearningStorePrivilegedScope privilegedScope =
			new LearningStorePrivilegedScope())
		{
			LearningStoreJob job = learningStore.CreateJob();
			LearningStoreQuery query = learningStore.CreateQuery(
				Schema.SiteSettingsItem.ItemTypeName);
			query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
			query.AddCondition(Schema.SiteSettingsItem.SiteGuid,
				LearningStoreConditionOperator.Equal, spSiteGuid);
			job.PerformQuery(query);
			DataRowCollection dataRows = job.Execute<DataTable>().Rows;
			if (dataRows.Count != 1)
				throw new SafeToDisplayException(AppResources.SlkSettingsNotFound, spSiteGuid);
			DataRow dataRow = dataRows[0];
			return (string) dataRow[0];
		}
    }

    /// <summary>
    /// Throws an exception if this computer isn't part of a SharePoint farm, or the caller doesn't
    /// have the necessary permissions to access instances of this class.
    /// </summary>
    ///
    static internal void EnsureFarmAdmin()
    {
        SPFarm farm = SPFarm.Local;
        if (farm == null)
            throw new InvalidOperationException(AppResources.SharePointFarmNotFound);
        else
            if (!farm.CurrentUserIsAdministrator())
                throw new UnauthorizedAccessException(AppResources.NotSharePointAdmin);
    }

    /// <summary>
    /// Returns an instance of the SharePoint Central Administration web service.
    /// </summary>
    ///
    internal static SPWebService GetAdminWebService()
    {
        SPAdministrationWebApplication adminWebApp = SPAdministrationWebApplication.Local;
        return (SPWebService) adminWebApp.Parent;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	//

	/// <summary>
	/// Returns <c>true</c> if all SharePoint permissions specified by a given set of permission
	/// names exist in the root web site of a given SPSite, <c>false</c> if not.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite to retrieve information about.</param>
	///
	/// <param name="permissionNames">The names of the permissions to look for.</param>
	///
	static bool PermissionsExist(Guid spSiteGuid, params string[] permissionNames)
	{
        // populate <existingPermissions> with the existing permissions on the root SPWeb of the
        // site with GUID <spSiteGuid>
        Dictionary<string, bool> existingPermissions = new Dictionary<string, bool>(20);
        using (SPSite spSite = new SPSite(spSiteGuid))
        {
            using (SPWeb rootWeb = spSite.RootWeb)
            {
                foreach (SPRoleDefinition roleDef in rootWeb.RoleDefinitions)
                    existingPermissions[roleDef.Name] = true;
            }
        }

        // check if all permissions listed in <permissionNames> are in <existingPermissions>
        foreach (string permissionName in permissionNames)
        {
            bool unused;
            if (!existingPermissions.TryGetValue(permissionName, out unused))
                return false; // permission not found
        }

        // success -- all permissions were found
        return true;
	}

	/// <summary>
	/// Adds a given permission to the root SPWeb of a given SPSite, if it doesn't exist.
    /// An optional set of base permissions are added to the permission.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite to add permissions to the root SPWeb of.
	/// 	</param>
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
	    using (SqlConnection sqlConnection = new SqlConnection(
		    databaseServerConnectionString))
	    {
		    // open the connection
		    sqlConnection.Open();

		    // create the database
            using (SqlCommand command = new SqlCommand(
                String.Format(CultureInfo.InvariantCulture, "CREATE DATABASE {0}", databaseName),
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

}

/// <summary>
/// Represents a mapping from an <c>SPSite</c> to a SharePoint Learning Kit LearningStore database.
/// </summary>
///
public class SlkSPSiteMapping : SPPersistedObject
{
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// The format string used to construct the name of this object as persisted in SharePoint's
	/// configuration database.
	/// </summary>
	const string PersistedObjectNameFormat = "SharePointLearningKitMapping_{0}";

	/// <summary>
	/// Holds the value of the <c>SPSiteGuid</c> property.
	/// configuration database.
	/// </summary>
	[Persisted]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spSiteGuid;

	/// <summary>
	/// Holds the value of the <c>DatabaseServer</c> property.
	/// </summary>
	[Persisted]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_databaseServer;

	/// <summary>
	/// Holds the value of the <c>DatabaseName</c> property.
	/// </summary>
	[Persisted]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_databaseName;

	/// <summary>
	/// Holds the value of the <c>InstructorPermission</c> property.
	/// </summary>
	[Persisted]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_instructorPermission;

	/// <summary>
	/// Holds the value of the <c>LearnerPermission</c> property.
	/// </summary>
	[Persisted]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_learnerPermission;

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the GUID of the SPSite represented by this SPSite-to-LearningStore mapping.
	/// </summary>
	public Guid SPSiteGuid
	{
		[DebuggerStepThrough]
		get
		{
			return m_spSiteGuid;
		}
	}

	/// <summary>
	/// Sets or gets the name of the LearningStore database server represented by this
	/// SPSite-to-LearningStore mapping, or <c>null</c> if none.  Note that if a SQL Server user
	/// ID and password are used to connect to the LearningStore database instead of integrated
	/// authentication, <c>DatabaseServer</c> will contain a fragment of a connection string such 
	/// as "MyServer;user id=myacct;password=mypassword".
	/// </summary>
	public string DatabaseServer
	{
		[DebuggerStepThrough]
		get
		{
			return m_databaseServer;
		}
		[DebuggerStepThrough]
		set
		{
			m_databaseServer = value;
		}
	}

	/// <summary>
	/// Sets or gets the name of the LearningStore database represented by this
	/// SPSite-to-LearningStore mapping, or <c>null</c> if none.
	/// </summary>
	public string DatabaseName
	{
		[DebuggerStepThrough]
		get
		{
			return m_databaseName;
		}
		[DebuggerStepThrough]
		set
		{
			m_databaseName = value;
		}
	}

	/// <summary>
	/// Gets the SQL Server connection string specified by this mapping's <c>DatabaseServer</c>
	/// and <c>DatabaseName</c> properties.  Returns <c>null</c> if either <c>DatabaseServer</c>
	/// or <c>DatabaseName</c> are <c>null</c>.
	/// </summary>
	public string DatabaseConnectionString
	{
		[DebuggerStepThrough]
		get
		{
			if ((m_databaseServer == null) || (m_databaseName == null))
				return null;
			else
			if (m_databaseServer.Contains(";"))
			{
				// SQL Server authentication, e.g. "MyServer;user id=myacct;password=mypassword"
                return String.Format(CultureInfo.InvariantCulture, "Server={0};Database={1}", m_databaseServer, m_databaseName);
			}
			else
			{
				// integrated authentication
				return String.Format(CultureInfo.InvariantCulture, "Server={0};Database={1};Integrated Security=true",
                    m_databaseServer, m_databaseName);
			}
		}
	}

	/// <summary>
	/// Gets the SQL Server connection string specified by this mapping's <c>DatabaseServer</c>
	/// property; this connection string ignores <c>DatabaseName</c>.  Returns <c>null</c> if
	/// <c>DatabaseServer</c> is <c>null</c>.
	/// </summary>
	public string DatabaseServerConnectionString
	{
		[DebuggerStepThrough]
		get
		{
			if (m_databaseServer == null)
				return null;
			else
			if (m_databaseServer.Contains(";"))
			{
				// SQL Server authentication, e.g. "MyServer;user id=myacct;password=mypassword"
                return String.Format(CultureInfo.InvariantCulture, "Server={0}", m_databaseServer);
			}
			else
			{
				// integrated authentication
				return String.Format(CultureInfo.InvariantCulture, "Server={0};Integrated Security=true", m_databaseServer);
			}
		}
	}

	/// <summary>
	/// Sets or gets the name of SharePoint permission used to identify instructors.
	/// </summary>
	public string InstructorPermission
	{
		[DebuggerStepThrough]
		get
		{
			return m_instructorPermission;
		}
		[DebuggerStepThrough]
		set
		{
			m_instructorPermission = value;
		}
	}

	/// <summary>
	/// Sets or gets the name of SharePoint permission used to identify learners.
	/// </summary>
	public string LearnerPermission
	{
		[DebuggerStepThrough]
		get
		{
			return m_learnerPermission;
		}
		[DebuggerStepThrough]
		set
		{
			m_learnerPermission = value;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

	/// <summary>
	/// Initializes an instance of this class.  This constructor is only used for deserialization
	/// and should not be used directly by applications.
	/// </summary>
	///
	public SlkSPSiteMapping()
	{
	}

	/// <summary>
	/// Returns the SPSite-to-LearningStore mapping represented by a given SPSite GUID.  If no
	/// such mapping exists, an exception is thrown.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite to retrieve a mapping for.</param>
	///
    /// <exception cref="SlkNotConfiguredException">
	/// SLK is not configured for SharePoint site collection <paramref name="spSiteGuid"/>.
    /// (This configuration is performed in SharePoint Central Administration.)
	/// </exception>
	///
	public static SlkSPSiteMapping GetMapping(Guid spSiteGuid)
	{
		SlkSPSiteMapping mapping;
		if (!GetMapping(spSiteGuid, out mapping))
            throw new SlkNotConfiguredException(AppResources.SlkNotEnabled);
		return mapping;
	}

	/// <summary>
	/// Retrieves the SPSite-to-LearningStore mapping represented by a given SPSite GUID.  If no
	/// such mapping exists, a new mapping object is created -- in that case, the caller should
	/// set the properties of the <c>SlkSPSiteMapping</c> object to their correct values and then
	/// call <c>Update</c> to store the mapping into the SharePoint configuration database.
	/// </summary>
	///
	/// <param name="spSiteGuid">The GUID of the SPSite to retrieve a mapping for.</param>
	///
	/// <param name="mapping">Set to the retrieved or newly-created mapping.</param>
	///
	/// <returns>
	/// <c>true</c> if an existing mapping was found; <c>false</c> if a new mapping is created.
	/// In the latter case, the new mapping is not stored in the SharePoint configuration
	/// database until <c>Update</c> is called.
	/// </returns>
	///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool GetMapping(Guid spSiteGuid, out SlkSPSiteMapping mapping)
	{
    	// set <adminWebService> to an instance of the SharePoint Central Administration web
		// service; set <mappingCollection> to the collection of SlkSPSiteMapping objects stored in
		// SharePoint's configuration database
		SPWebService adminWebService;
		SlkSPSiteMappingCollection mappingCollection;
		GetMappingInfo(out adminWebService, out mappingCollection);

		// set <mapping> to the SlkSPSiteMapping corresponding to <spSiteGuid>
        string persistedObjectName = String.Format(CultureInfo.InvariantCulture, PersistedObjectNameFormat, spSiteGuid);
		mapping = mappingCollection.GetValue<SlkSPSiteMapping>(persistedObjectName);

		// return the mapping if found; if not found, create a new mapping
		if (mapping != null)
		{
			return true;
		}
		else
		{
			mapping = new SlkSPSiteMapping(persistedObjectName, adminWebService, spSiteGuid);
			return false;
		}
	}

	/// <summary>
	/// Returns the collection of <c>SlkSPSiteMapping</c> objects stored in SharePoint's
	/// configuration database.
	/// </summary>
	///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public static SlkSPSiteMappingCollection GetMappings()
	{
    	// set <adminWebService> to an instance of the SharePoint Central Administration web
		// service; set <mappingCollection> to the collection of SlkSPSiteMapping objects stored in
		// SharePoint's configuration database
		SPWebService adminWebService;
		SlkSPSiteMappingCollection mappingCollection;
		GetMappingInfo(out adminWebService, out mappingCollection);

		// return the collection of mappings
		return mappingCollection;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	//

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
    /// <param name="persistedObjectName">The name of this object as it will be persisted in
	/// 	SharePoint's configuration database.</param>
	///
    /// <param name="adminWebService">An instance of the SharePoint Central Administration web
	/// 	service.</param>
	///
    /// <param name="spSiteGuid">The GUID of the SPSite represented by this
	/// 	SPSite-to-LearningStore mapping.</param>
	///
	SlkSPSiteMapping(string persistedObjectName, SPWebService adminWebService, Guid spSiteGuid)
		: base(persistedObjectName, adminWebService)
	{
		m_spSiteGuid = spSiteGuid;
	}

	/// <summary>
	/// Retrieves various pieces of information related to SharePoint Central Administration and
	/// the SPSite-to-LearningStore mappings.
	/// </summary>
	///
    /// <param name="adminWebService">Set to an instance of the SharePoint Central Administration
	/// 	web service.</param>
	///
    /// <param name="mappingCollection">Set to the collection of <c>SlkSPSiteMapping</c> objects
	/// 	stored in SharePoint's configuration database.</param>
	///
	static void GetMappingInfo(out SPWebService adminWebService,
		out SlkSPSiteMappingCollection mappingCollection)
	{
        adminWebService = SlkAdministration.GetAdminWebService();
		mappingCollection = new SlkSPSiteMappingCollection(adminWebService);
	}
}

/// <summary>
/// Represents a collection of <c>SlkSPSiteMapping</c> objects.
/// </summary>
///
public class SlkSPSiteMappingCollection :
	SPPersistedChildCollection<SlkSPSiteMapping>
{
	internal SlkSPSiteMappingCollection(SPWebService webService)
		: base(webService)
	{
	}
}

}

