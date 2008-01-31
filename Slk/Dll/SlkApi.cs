/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkApi.cs
//
// Implements SlkStore and related types.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Library;
using Microsoft.SharePoint.Navigation;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Microsoft.SharePointLearningKit
{

/// <summary>
/// Represents a connection to a SharePoint Learning Kit store.  Doesn't hold any information tied
/// to the identity of the current user, so an <c>AnonymousSlkStore</c> can be cached on a Web site
/// and shared by multiple users.
/// </summary>
///
internal class AnonymousSlkStore
{
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>SPSiteGuid</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spSiteGuid;

	/// <summary>
	/// Holds the value of the <c>Mapping</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SlkSPSiteMapping m_mapping;

	/// <summary>
	/// Holds the value of the <c>Settings</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SlkSettings m_settings;

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this <c>AnonymousSlkStore</c>.
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
	/// Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this
	/// <c>AnonymousSlkStore</c>.
	/// </summary>
	public SlkSPSiteMapping Mapping
	{
		[DebuggerStepThrough]
		get
		{
			return m_mapping;
		}
	}

	/// <summary>
	/// Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this
	/// <c>AnonymousSlkStore</c>.
	/// </summary>
	public SlkSettings Settings
	{
		[DebuggerStepThrough]
		get
		{
			return m_settings;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
	/// 	</param>
	///
	/// <param name="mapping">The value to use to initialize the <c>Mapping</c> property.
	/// 	</param>
	///
	/// <param name="settings">The value to use to initialize the <c>Settings</c> property.
	/// 	</param>
	///
	public AnonymousSlkStore(Guid spSiteGuid, SlkSPSiteMapping mapping, SlkSettings settings)
	{
		m_spSiteGuid = spSiteGuid;
		m_mapping = mapping;
		m_settings = settings;
	}
}

/// <summary>
/// Represents a connection to a SharePoint Learning Kit store, containing assignments, references
/// to e-learning packages stored in SharePoint.  Also contains the identity of the user accessing
/// the store (used for authorization purposes).
/// </summary>
///
public class SlkStore
{
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Information about the connection to the SharePoint Learning Kit store that doesn't
	/// include any information tied to the identity of the current user.  The
	/// <c>AnonymousSlkStore</c> referenced by this field is cached on a Web site and may be shared
	/// by multiple users.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	AnonymousSlkStore m_anonymousSlkStore;

	/// <summary>
	/// Holds the value of the <c>LearningStore</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	LearningStore m_learningStore;

    /// <summary>
    /// Holds the value of the <c>PackageStore</c> property.
    /// </summary>
    ///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SharePointPackageStore m_packageStore;

    /// <summary>
    /// Holds the value of the <c>SharePointCacheSettings</c> property.
    /// </summary>
    ///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SharePointCacheSettings m_sharePointCacheSettings;

    /// <summary>
    /// Holds the value of the <c>CurrentUserName</c> property.
    /// </summary>
    ///
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_currentUserName;

    /// <summary>
    /// Enumerating domain groups in <c>GetMemberships</c> will time out if
    /// <c>GetMemberships</c> executes longer for this time span.
    /// </summary>
    static readonly TimeSpan DomainGroupEnumerationTotalTimeout = new TimeSpan(0, 5, 0);
    
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Internal Fields
	//

	/// <summary>
	/// Specifies for how many seconds <c>SlkStore</c> is cached within the current
	/// <c>HttpContext</c>.  This many seconds after being loaded, the next time you create an
	/// instance of <c>SlkStore</c>, information is reloaded from the SharePoint configuration
	/// database, and <c>SlkSettings</c> information is reloaded from the SharePoint Learning Kit
	/// database.
	/// </summary>
	internal const int HttpContextCacheTime = 60;

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.
	/// </summary>
	public Guid SPSiteGuid
	{
		[DebuggerStepThrough]
		get
		{
			return m_anonymousSlkStore.SPSiteGuid;
		}
	}

	/// <summary>
	/// Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this
	/// <c>AnonymousSlkStore</c>.
	/// </summary>
	public SlkSPSiteMapping Mapping
	{
		[DebuggerStepThrough]
		get
		{
			return m_anonymousSlkStore.Mapping;
		}
	}

	/// <summary>
	/// Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.
	/// </summary>
	public SlkSettings Settings
	{
		[DebuggerStepThrough]
		get
		{
            return m_anonymousSlkStore.Settings;
		}
	}

	/// <summary>
	/// Gets the <c>LearningStore</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.
	/// <c>LearningStore</c> contains information about e-learning assignments, learner
	/// assignments (including SCORM attempts), and other e-learning information.
	/// </summary>
	public LearningStore LearningStore
	{
		[DebuggerStepThrough]
		get
		{
			return m_learningStore;
		}
	}

    /// <summary>
    /// Gets a reference to the <c>SharePointPackageStore</c> associated with this SharePoint
	/// Learning Kit store.  <c>SharePointPackageStore</c> holds references to e-learning
	/// packages stored in SharePoint document libraries.
    /// </summary>
    ///
    public SharePointPackageStore PackageStore
    {
        get
        {
            if (m_packageStore == null)
            {
                m_packageStore = new SharePointPackageStore(LearningStore, SharePointCacheSettings);
            }
            return m_packageStore;
        }
    }

    /// <summary>
    /// Gets the user key used by LearningStore to identify the current user.  What's contained in
    /// this string depends on the membership provider used by SharePoint (for example, Windows
    /// authentication or forms-based authentication).
    /// </summary>
    public string CurrentUserKey
    {
		[DebuggerStepThrough]
        get
        {
            return m_learningStore.UserKey;
        }
    }

    /// <summary>
    /// Gets information about the file system cache used by <c>PackageStore</c>.
    /// </summary>
    public SharePointCacheSettings SharePointCacheSettings
    {
        get
        {
			if (m_sharePointCacheSettings == null)
			{
                // set <cachePath> to the path of the package store cache, with environment
                // variables (e.g. "%TEMP%") expanded
                string cachePath = null;
                SlkUtilities.ImpersonateAppPool(delegate()
                {
                    cachePath = Environment.ExpandEnvironmentVariables(
                        Settings.PackageCacheLocation);
                });

                // initialize <m_sharePointCacheSettings>
                if (Settings.PackageCacheExpirationMinutes == null)
                    m_sharePointCacheSettings = new SharePointCacheSettings(cachePath,
                        null, ImpersonationBehavior.UseOriginalIdentity, true);
                else
                {
                    TimeSpan expirationTime = new TimeSpan(0,
						Settings.PackageCacheExpirationMinutes.Value, 0);
                    m_sharePointCacheSettings = new SharePointCacheSettings(cachePath,
                        expirationTime, ImpersonationBehavior.UseOriginalIdentity, true);
                }
			}
            return m_sharePointCacheSettings;
        }
    }

    /// <summary>
    /// Gets the name of the current user.
    /// </summary>
    public string CurrentUserName
    {
		[DebuggerStepThrough]
        get
        {
            return m_currentUserName;
        }
    }

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

    /// <summary>
	/// Returns the <c>SlkStore</c> object associated the SharePoint <c>SPSite</c> that a given
    /// SharePoint <c>SPWeb</c> belongs to.  If no such object exists (i.e. if SharePoint Learning
    /// Kit was not yet configured for the specified SPSite in SharePoint Central Administration),
    /// an exception is thrown.
	/// </summary>
	///
    /// <remarks>
    /// <para>
    /// If there is an active <c>HttpContext</c> (i.e. if the caller is executing from within the
    /// context of ASP.NET web page code), the retrieved <c>SlkStore</c> is cached for a
    /// period of time specified by <c>HttpContextCacheTime</c>: that many seconds after being
    /// loaded, the next time <c>GetStore</c> is called the <c>SlkSPSiteMapping</c>
    /// information is reloaded from the SharePoint configuration database, and the
    /// <c>SlkSettings</c> information is reloaded from the SharePoint Learning Kit database.
    /// </para>
    /// <para>
    /// If there is no active <c>HttpContext</c>, no caching is performed.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="spWeb">The SharePoint <c>SPWeb</c> from which to retrieve the current user's
    ///     identity and the identity of the <c>SPSite</c> of the SLK store to open.</param>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause:
	/// <pr>spWeb</pr> isn't configured for use with SharePoint Learning Kit.
    /// </exception>
    ///
    /// <exception cref="UserNotFoundException">
    /// The current user is not signed into SharePoint.
    /// </exception>
    /// 

    public static SlkStore GetStore(SPWeb spWeb)
    {
        return GetStore(spWeb, "");
    }

    public static SlkStore GetStore(SPWeb spWeb, string learningStoreUserKey)
	{
	    // Security checks: None

        // Check parameters
        if(spWeb == null)
            throw new ArgumentNullException("spWeb");
            	    
	    // Get the interesting information from the SPWeb
	    Guid spSiteGuid = spWeb.Site.ID;
		
        //If the User not signed in throw user not found exception
        if (spWeb.CurrentUser == null)
			throw new UserNotFoundException(AppResources.SlkExUserNotFound);
        //Use SPUser LoginName if Sid is Null or Empty.
        string currentUserKey 
            = String.IsNullOrEmpty(spWeb.CurrentUser.Sid) ? spWeb.CurrentUser.LoginName : spWeb.CurrentUser.Sid;
	    string currentUserName = spWeb.CurrentUser.Name;

		// set <httpContext> to the current HttpContext (null if none)
		HttpContext httpContext = HttpContext.Current;

		// if an AnonymousSlkStore corresponding to <spSiteGuid> is cached, retrieve it, otherwise
		// create one
		string cacheItemName = null;
		AnonymousSlkStore anonymousSlkStore;
		LearningStore learningStore;
		if (httpContext != null)
		{
			cacheItemName = String.Format(CultureInfo.InvariantCulture, "SlkStore_{0}", spSiteGuid);
			anonymousSlkStore = (AnonymousSlkStore) httpContext.Cache.Get(cacheItemName);
			if (anonymousSlkStore != null)
			{
				// create a new SlkStore that wraps the cached AnonymousSlkStore plus a new
				// <currentUserKey>-specific LearningStore
                if (String.IsNullOrEmpty(learningStoreUserKey) == true)
                {
                    learningStore = new LearningStore(anonymousSlkStore.Mapping.DatabaseConnectionString,
                        currentUserKey, ImpersonationBehavior.UseOriginalIdentity);
                }
                else
                {
                    learningStore = new LearningStore(anonymousSlkStore.Mapping.DatabaseConnectionString,
                        learningStoreUserKey, ImpersonationBehavior.UseOriginalIdentity);

                }
                    return new SlkStore(anonymousSlkStore, learningStore, currentUserName);
			}
		}

		// set <mapping> to the SlkSPSiteMapping corresponding to <spSiteGuid>; if no such
		// mapping, exists, a SafeToDisplayException is thrown
		SlkSPSiteMapping mapping = SlkSPSiteMapping.GetMapping(spSiteGuid);

		// load "SlkSettings.xsd" from a resource into <xmlSchema>
		XmlSchema xmlSchema;
		using (StringReader schemaStringReader = new StringReader(AppResources.SlkSettingsSchema))
		{
			xmlSchema = XmlSchema.Read(schemaStringReader,
				delegate(object sender2, ValidationEventArgs e2)
				{
					// ignore warnings (already displayed when SLK Settings file was uploaded)
				});
		}

		// create a LearningStore
        if (String.IsNullOrEmpty(learningStoreUserKey) == true)
        {
            learningStore = new LearningStore(mapping.DatabaseConnectionString, currentUserKey,
                ImpersonationBehavior.UseOriginalIdentity);
        }
        else
        {
            learningStore = new LearningStore(mapping.DatabaseConnectionString, learningStoreUserKey,
                ImpersonationBehavior.UseOriginalIdentity);
        }

		// read the SLK Settings file from the database into <settings>
		SlkSettings settings;
		using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
		{
		    LearningStoreJob job = learningStore.CreateJob();
		    LearningStoreQuery query = learningStore.CreateQuery(Schema.SiteSettingsItem.ItemTypeName);
		    query.AddColumn(Schema.SiteSettingsItem.SettingsXml);
		    query.AddColumn(Schema.SiteSettingsItem.SettingsXmlLastModified);
		    query.AddCondition(Schema.SiteSettingsItem.SiteGuid,
			    LearningStoreConditionOperator.Equal, spSiteGuid);
		    job.PerformQuery(query);
		    DataRowCollection dataRows = job.Execute<DataTable>().Rows;
		    if (dataRows.Count != 1)
			    throw new SlkSettingsException(AppResources.SlkSettingsNotFound, spSiteGuid);
		    DataRow dataRow = dataRows[0];
		    string settingsXml = (string)dataRow[0];
		    DateTime settingsXmlLastModified = ((DateTime)dataRow[1]).ToLocalTime();
		    using (StringReader stringReader = new StringReader(settingsXml))
		    {
			    XmlReaderSettings xmlSettings = new XmlReaderSettings();
			    xmlSettings.Schemas.Add(xmlSchema);
			    xmlSettings.ValidationType = ValidationType.Schema;
			    using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlSettings))
				    settings = SlkSettings.ParseSettingsFile(xmlReader, settingsXmlLastModified);
		    }
        }
        
		// create and (if possible) cache the new AnonymousSlkStore object
		anonymousSlkStore = new AnonymousSlkStore(spSiteGuid, mapping, settings);
		DateTime cacheExpirationTime = DateTime.Now.AddSeconds(HttpContextCacheTime);
		if (httpContext != null)
		{
			httpContext.Cache.Add(cacheItemName, anonymousSlkStore, null, cacheExpirationTime,
				System.Web.Caching.Cache.NoSlidingExpiration,
				System.Web.Caching.CacheItemPriority.Normal, null);
		}

		// return an SlkStore that wraps <anonymousSlkStore> and <learningStore>
		return new SlkStore(anonymousSlkStore, learningStore, currentUserName);
	}

	/// <summary>
	/// Returns the <c>UserItemIdentifier</c> of the current user.  If no UserItem exists in
	/// LearningStore for the current user, one is created.
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// There is a performance cost to calling this method, since it accesses the SLK database.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
	/// may be any user.
	/// </para>
	/// </remarks>
	///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public UserItemIdentifier GetCurrentUserId()
	{
        // Security checks: None

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();
        job.DisableFollowingSecurityChecks();
        
        // request the UserItemIdentifier of the current user -- create a UserItem for that user if
        // they're not yet in the SLK database
        Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
        Dictionary<string, object> updateProperties = new Dictionary<string, object>();
        uniqueProperties.Clear();
        uniqueProperties[Schema.UserItem.Key] = CurrentUserKey;
        updateProperties.Clear();
        updateProperties[Schema.UserItem.Name] = CurrentUserName;
        job.AddOrUpdateItem(Schema.UserItem.ItemTypeName, uniqueProperties, updateProperties,
            null, true);

        // execute the job
        ReadOnlyCollection<object> results = job.Execute();
        IEnumerator<object> resultEnumerator = results.GetEnumerator();

        // retrieve the UserItemIdentifier of the current user from the job results and return it
        UserItemIdentifier currentUserId;
        if (!resultEnumerator.MoveNext())
            throw new InternalErrorException("SLK1003");
        LearningStoreHelper.CastNonNull(resultEnumerator.Current, out currentUserId);
		return currentUserId;
	}

    /// <summary>
    /// Registers a specified version of a given e-learning package (that's stored in a SharePoint
	/// document library) in the <c>SharePointPackageStore</c> associated with this
	/// SharePoint Learning Kit store, and returns its <c>PackageItemIdentifier</c> and content
	/// warnings.  If that version -- with the same last-modified date/time -- is already
	/// registered, its information is returned rather than re-registering the package.
	/// Uses an MLC SharePoint location string to locate the package.
    /// </summary>
	///
    /// <param name="location">The MLC SharePoint location string that refers to the e-learning
    ///     package to register.  Use <c>SharePointPackageStore.GetLocation</c> to construct this
	/// 	string.</param>
    /// 
    /// <param name="packageId">Where the returned <c>PackageItemIdentifier</c> is stored.</param>
	///
    /// <param name="warnings">Where the returned warnings are stored.  This XML consists of
	/// 	a root "&lt;Warnings&gt;" element containing one "&lt;Warning&gt;" element per
	/// 	warning, each of which contains the text of the warning as the content of the element
	/// 	plus the following attributes: the "Code" attribute contains the warning's
    /// 	<c>ValidationResultCode</c>, and the "Type" attribute contains the warning's
    ///     type, either "Error" or "Warning".  <paramref name="warnings"/> is set to <c>null</c>
	/// 	if there are no warnings.</param>
	///
	/// <remarks>
	/// <para>
	/// If the package is valid, <pr>warnings</pr> is set to <n>null</n>.  If the package is not
	/// completely valid, but is valid enough to be assigned within SharePoint Learning Kit,
	/// <pr>warnings</pr> is set to warnings about the package.  If the package has problems
	/// severe enough to prevent it from being assignable within SLK, a
	/// <r>SafeToDisplayException</r> is thrown.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; This operation fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have access to the package.
	/// </para>
	/// </remarks>
	///
    /// <exception cref="ArgumentException">
    /// The syntax of <pr>location</pr> is incorrect.
    /// </exception>
	///
    /// <exception cref="FileNotFoundException">
    /// <pr>location</pr> refers to a file that does not exist.
    /// </exception>
    /// 
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the package has
	/// problems severe enough to prevent it from being assignable within SharePoint Learning Kit.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to.
    /// </exception>
	///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public void RegisterPackage(string location,
        out PackageItemIdentifier packageId, out LearningStoreXml warnings)
    {
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by RegisterAndValidatePackage)

        // Check parameters
        if (location == null)
            throw new ArgumentNullException("location");

        RegisterAndValidatePackage(location, false, out packageId, out warnings);
    }

    /// <summary>
    /// Validates a specified version of a given e-learning package (that's stored in a SharePoint
    /// document library), and returns its content warnings.  If that version -- with the same
    /// last-modified date/time -- is already registered, its information is returned rather than
    /// re-validating the package.
    /// Uses an MLC SharePoint location string to locate the package.
    /// </summary>
    ///
    /// <param name="location">The MLC SharePoint location string that refers to the e-learning
    ///     package to validate.  Use <c>SharePointPackageStore.GetLocation</c> to construct this
    /// 	string.</param>
    /// 
    /// <returns>The warnings.  This XML consists of a root "&lt;Warnings&gt;" element containing
    ///     one "&lt;Warning&gt;" element per warning, each of which contains the text of the
    ///     warning as the content of the element plus the following attributes: the "Code"
    ///     attribute contains the warning's <c>ValidationResultCode</c>, and the "Type" attribute
    ///     contains the warning's type, either "Error" or "Warning".  <n>null</n> is returned
    ///     if there are no warnings.</returns>
	///
	/// <remarks>
	/// <para>
	/// If the package is valid, <n>null</n> is returned.  If the package is not completely valid,
	/// but is valid enough to be assigned within SharePoint Learning Kit, warnings about the
	/// package are returned.  If the package has problems severe enough to prevent it from being
	/// assignable within SLK, a <r>SafeToDisplayException</r> is thrown.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; This operation fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have access to the package.
	/// </para>
	/// </remarks>
	///
    /// <exception cref="FileNotFoundException">
    /// <pr>location</pr> refers to a file that does not exist.
    /// </exception>
    /// 
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the package has
	/// problems severe enough to prevent it from being assignable within SharePoint Learning Kit.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to.
    /// </exception>
    ///
    public LearningStoreXml ValidatePackage(string location)
    {
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by RegisterAndValidatePackage)

        // Check parameters
        if (location == null)
            throw new ArgumentNullException("location");

        LearningStoreXml warnings;
        PackageItemIdentifier packageId;
        RegisterAndValidatePackage(location, true, out packageId, out warnings);
        
        return warnings;
    }

    /// <summary>
    /// Registers a specified version of a given e-learning package (that's stored in a SharePoint
	/// document library) in the <c>SharePointPackageStore</c> associated with this
	/// SharePoint Learning Kit store, and returns its <c>PackageItemIdentifier</c> and content
	/// warnings.  If that version -- with the same last-modified date/time -- is already
	/// registered, its information is returned rather than re-registering the package.
	/// Uses an <c>SPFile</c> and version ID to locate the package.
    /// </summary>
	///
    /// <param name="location">The location string.</param>
	///
    /// <param name="validateOnly">If <c>true</c>, the package is not registered -- instead,
    ///     warnings are retrieved from the database (if the package was previously registered)
    ///     or by validating the package (if the package was not previously registered), and
    ///     <paramref name="packageId"/> will be set to <c>null</c> unless the package was
    ///     previously registered.</param>
	///
    /// <param name="packageId">Where the returned <c>PackageItemIdentifier</c> is stored.</param>
	///
    /// <param name="warnings">Where the returned warnings are stored.  This XML consists of
	/// 	a root "&lt;Warnings&gt;" element containing one "&lt;Warning&gt;" element per
	/// 	warning, each of which contains the text of the warning as the content of the element
	/// 	plus the following attributes: the "Code" attribute contains the warning's
    /// 	<c>ValidationResultCode</c>, and the "Type" attribute contains the warning's
    ///     type, either "Error" or "Warning".  <paramref name="warnings"/> is set to <c>null</c>
	/// 	if there are no warnings.</param>
	///
	/// <remarks>
	/// If the package is valid, <pr>warnings</pr> is set to <n>null</n>.  If the package is not
	/// completely valid, but is valid enough to be assigned within SharePoint Learning Kit,
	/// <pr>warning</pr> is set to warnings about the package.  If the package has problems
	/// severe enough to prevent it from being assignable within SLK, a
	/// <r>SafeToDisplayException</r> is thrown.
	/// </remarks>
    /// 
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the package has
	/// problems severe enough to prevent it from being assignable within SharePoint Learning Kit.
    /// <r>SafeToDisplayException.ValidationResults</r> may contain further information.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// The user is not authorized to access the package.
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
    private void RegisterAndValidatePackage(string location, bool validateOnly,
		out PackageItemIdentifier packageId, out LearningStoreXml warnings)
	{
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by GetFileFromSharePointLocation)

        // Check parameters
        if (location == null)
            throw new ArgumentNullException("location");

        // Set <fileLocation> to the file location
        SharePointFileLocation fileLocation;
        SPFile spFile;
        GetFileFromSharePointLocation(location, out spFile, out fileLocation);
       
		// find the package in the SharePointPackageStore; add it if it doesn't exist;
		// set <packageId> to its PackageItemIdentifier; set <warnings> to refer to warnings
		// about the package contents
        TransactionOptions options = new TransactionOptions();
        options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
			new LearningStoreTransactionScope(options))
		{
		    // Anyone who has access to the package can register it.  Since we verified that
		    // the user has access to the package above, we can skip any package-related
		    // security checks
		    using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
		    {
			    LearningStoreJob job = LearningStore.CreateJob();
			    LearningStoreQuery query = LearningStore.CreateQuery(Schema.PackageItem.ItemTypeName);
			    query.AddColumn(Schema.PackageItem.Id);
			    query.AddColumn(Schema.PackageItem.Warnings);
			    query.AddCondition(Schema.PackageItem.Location, LearningStoreConditionOperator.Equal,
                    fileLocation.ToString());
			    job.PerformQuery(query);
			    DataRowCollection result = job.Execute<DataTable>().Rows;
			    if (result.Count > 0)
			    {
				    // package exists in SharePointPackageStore, i.e. it's listed one or more times in
				    // the PackageItem table -- set <packageId> to the PackageItemIdentifier of the
				    // first of them
				    LearningStoreHelper.CastNonNull(result[0][Schema.PackageItem.Id], out packageId);
				    LearningStoreHelper.Cast(result[0][Schema.PackageItem.Warnings], out warnings);
			    }
			    else
			    {
				    // package doesn't exist in SharePointPackageStore -- register it if <validateOnly>
				    // is false, or just validate it if <validateOnly> is true, and <log> to the
				    // validation log in either case
                    ValidationResults log;
                    if (validateOnly)
                    {
                        // validate the package, but don't register it
                        packageId = null;
                        SharePointPackageReader packageReader;
                        try
                        {
                            packageReader = new SharePointPackageReader(
                                SharePointCacheSettings, fileLocation, false);
                        }
                        catch (InvalidPackageException ex)
                        {
							throw new SafeToDisplayException(
                                String.Format(CultureInfo.CurrentCulture, 
                                              AppResources.PackageNotValid, ex.Message));
                        }
                        using (packageReader)
                        {
                            PackageValidatorSettings settings = new PackageValidatorSettings(
                                ValidationBehavior.LogWarning, ValidationBehavior.None,
                                ValidationBehavior.LogError, ValidationBehavior.LogWarning);
                            try
                            {
                                log = PackageValidator.Validate(packageReader, settings);
                                if (log.HasErrors)
                                {
                                    throw new SafeToDisplayException(log,
                                        String.Format(CultureInfo.CurrentCulture,
                                                  AppResources.PackageNotValid, ""));
                                }
                            }
                            catch (InvalidPackageException ex)
                            {
                                throw new SafeToDisplayException(
                                    String.Format(CultureInfo.CurrentCulture, 
                                                  AppResources.PackageNotValid, ex.Message));
                            }
                        }
                    }
				    else
				    {
                        // validate and register the package
					    PackageEnforcement pe = new PackageEnforcement(false, true, false);
					    RegisterPackageResult registerResult;
					    try
					    {
                            registerResult = PackageStore.RegisterPackage(fileLocation, pe);
					    }
					    catch (PackageImportException ex)
					    {
                            throw new SafeToDisplayException(ex.Log, ex.Message);
                        }
				        packageId = registerResult.PackageId;
                        log = registerResult.Log;
				    }

				    // if there were warnings, set <warnings> to refer to them and update the
				    // PackageItem in LearningStore to contain the warnings (if there is a PackageItem)
				    if (log.HasWarnings || log.HasErrors)
				    {
					    // set <warnings> to XML that refers to the warnings in <registerResult.Log>
					    StringBuilder stringBuilder = new StringBuilder(2000);
					    using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
					    {
						    xmlWriter.WriteStartElement("Warnings");
						    foreach (ValidationResult validationResult in log.Results)
						    {
							    xmlWriter.WriteStartElement("Warning");
							    xmlWriter.WriteAttributeString("Type",
								    validationResult.IsError ? "Error" : "Warning");
							    xmlWriter.WriteString(validationResult.Message);
							    xmlWriter.WriteEndElement();
						    }
						    xmlWriter.WriteEndElement();
					    }
					    string xml = stringBuilder.ToString();
					    using (StringReader stringReader = new StringReader(xml))
					    {
						    using (XmlReader xmlReader = XmlReader.Create(stringReader))
							    warnings = LearningStoreXml.CreateAndLoad(xmlReader);
					    }

					    // copy <warnings> into the PackageItem table row (if any)
                        if (packageId != null)
                        {
                            job = LearningStore.CreateJob();
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties[Schema.PackageItem.Warnings] = warnings;
                            job.UpdateItem(packageId, properties);
                            job.Execute();
                        }
				    }
				    else
					    warnings = null;
			    }			    
			}
			
			scope.Complete();
		}
	}

    //////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	//
	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="anonymousSlkStore">Information about the connection to the SharePoint
	/// 	Learning Kit store that doesn't include any "current user" information.</param>
	///
	/// <param name="learningStore">The <c>LearningStore</c> that connects to the SharePoint
	/// 	Learning Kit store database, configured with a specified "current user" identity.
	/// 	</param>
	///
    /// <param name="currentUserName">The name of the current user.  Use <c>String.Empty</c> if
	/// 	<c>String.Empty</c> is specified as the user key of <paramref name="learningStore"/>.
	/// 	</param>
	///
	SlkStore(AnonymousSlkStore anonymousSlkStore, LearningStore learningStore,
		string currentUserName)
	{
        // Security checks: None

        m_anonymousSlkStore = anonymousSlkStore;
		m_learningStore = learningStore;
		m_currentUserName = currentUserName;
	}

    /// <summary>
    /// Throws an exception if the current user isn't an instructor on an SPWeb
    /// </summary>
    /// 
    /// <param name="spWeb">SPWeb that should be checked.</param>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with.
    /// </exception>
    /// 
    /// <exception cref="NotAnInstructorException">
    /// The user is not an instructor.
    /// </exception>
    ///
    /// <exception cref="UnauthorizedAccessException">
    /// The user is not a Reader on <pr>spWeb</pr>.
    /// </exception>
    ///
    private void EnsureInstructor(SPWeb spWeb)
    {
        // Security checks: Fails if the user doesn't have Reader access (implemented
        // by IsInstructor) or Instructor access.

        // Verify that the web is in the site
        if(spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

		// Verify that the user is an instructor
        if (!IsInstructor(spWeb))
        {
            throw new NotAnInstructorException(
                String.Format(CultureInfo.CurrentCulture,
                              AppResources.SlkExInstructorPermissonNotFound,
                              spWeb.Title));
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the current user is an instructor on a given SPWeb, <c>false</c>
	/// if not.
    /// </summary>
    /// 
    /// <param name="spWeb">SPWeb that should be checked.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the <a href="SlkApi.htm#AccessingSlkStore">current
	/// user</a> doesn't have SharePoint "Reader" access.
	/// </remarks>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with.
    /// </exception>
    ///
    /// <exception cref="UnauthorizedAccessException">
    /// The user is not a Reader on <pr>spWeb</pr>.
    /// </exception>
    ///
    public bool IsInstructor(SPWeb spWeb)
    {
        // Security checks: Fails if the user doesn't have Reader access (implemented
        // by SharePoint)

        // Check parameters
        if (spWeb == null)
            throw new ArgumentNullException("spWeb");

        // Verify that the web is in the site
        if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        foreach (SPRoleDefinition roleDefinition in spWeb.AllRolesForCurrentUser)
        {
            if (roleDefinition.Name == m_anonymousSlkStore.Mapping.InstructorPermission)
                return true;
        }

		return false;
    }

    /// <summary>
    /// Returns <c>true</c> if the current user is an observer on a given SPWeb, <c>false</c>
    /// if not.
    /// </summary>
    /// 
    /// <param name="spWeb">SPWeb that should be checked.</param>
    ///
    /// <remarks>
    /// <b>Security:</b>&#160; Fails if the <a href="SlkApi.htm#AccessingSlkStore">current
    /// user</a> doesn't have SharePoint "Reader" access.
    /// </remarks>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with.
    /// </exception>
    ///
    /// <exception cref="UnauthorizedAccessException">
    /// The user is not a Reader on <pr>spWeb</pr>.
    /// </exception>
    ///
    public bool IsObserver(SPWeb spWeb)
    {
        
        // Security checks: Fails if the user doesn't have Reader access (implemented
        // by SharePoint)

        // Check parameters
        if (spWeb == null)
            throw new ArgumentNullException("spWeb");

        // Verify that the web is in the site
        if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        foreach (SPRoleDefinition roleDefinition in spWeb.AllRolesForCurrentUser)
        {
            if (roleDefinition.Name == m_anonymousSlkStore.Mapping.ObserverPermission)
                return true;
        }

        return false;
        
    }

    public bool IsLearner(SPWeb spWeb)
    {
        // Security checks: Fails if the user doesn't have Reader access (implemented
        // by SharePoint)

        // Check parameters
        if (spWeb == null)
            throw new ArgumentNullException("spWeb");

        // Verify that the web is in the site
        if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        foreach (SPRoleDefinition roleDefinition in spWeb.AllRolesForCurrentUser)
        {
            if (roleDefinition.Name == m_anonymousSlkStore.Mapping.LearnerPermission)
                return true;
        }

        return false;

    }
    
    /// <summary>
    /// Retrieves the lists of instructors, learners, and learner groups from a given SharePoint
    /// <c>SPWeb</c>.  Returns a <c>Boolean</c> value indicating whether information about any
	/// instructors or learners could not be retrieved.
    /// </summary>
    ///
    /// <param name="spWeb">The <c>SPWeb</c> to retrieve information from.</param>
	///
    /// <param name="additionalInstructors">A list of instructors to add to those in the
	/// 	returned <c>SlkMemberships.Instructors</c> list.  The resulting combined list is sorted
	/// 	and duplicates are removed.  <c>SlkUser.SPUser</c> is ignored and may be <c>null</c>
	/// 	for items in <paramref name="additionalInstructors"/>.  If <c>additionalInstructors</c>
	///     is <c>null</c> it is ignored.</param>
	///
    /// <param name="additionalLearners">A list of learners to add to those in the returned
	/// 	<c>SlkMemberships.Learners</c> list.  The resulting combined list is sorted and
	/// 	duplicates are removed.  <c>SlkUser.SPUser</c> is ignored and may be <c>null</c>
	/// 	for items in <paramref name="additionalLearners"/>.  If <c>additionalLearners</c> is
	///     <c>null</c> it is ignored.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the <a href="SlkApi.htm#AccessingSlkStore">current
	/// user</a> doesn't have SLK
	/// <a href="Microsoft.SharePointLearningKit.SlkSPSiteMapping.InstructorPermission.Property.htm">instructor</a>
	/// and SharePoint "Reader" permissions on <pr>spWeb</pr>.
	/// </remarks>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with.
    /// </exception>
	///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public SlkMemberships GetMemberships(SPWeb spWeb, IEnumerable<SlkUser> additionalInstructors,
        IEnumerable<SlkUser> additionalLearners)
    {
        // Security checks: Fails if the user isn't an instructor and a Reader (implemented
        // by other GetMemberships method)

        ReadOnlyCollection<string> groupFailures;
        string groupFailureDetails;
        return GetMemberships(spWeb, additionalInstructors, additionalLearners, out groupFailures,
            out groupFailureDetails);
    }

    /// <summary>
    /// Retrieves the lists of instructors, learners, and learner groups from a given SharePoint
    /// <c>SPWeb</c>.  Returns a <c>Boolean</c> value indicating whether information about any
	/// instructors or learners could not be retrieved.
    /// </summary>
    ///
    /// <param name="spWeb">The <c>SPWeb</c> to retrieve information from.</param>
	///
    /// <param name="additionalInstructors">A list of instructors to add to those in the
	/// 	returned <c>SlkMemberships.Instructors</c> list.  The resulting combined list is sorted
	/// 	and duplicates are removed.  <c>SlkUser.SPUser</c> is ignored and may be <c>null</c>
	/// 	for items in <paramref name="additionalInstructors"/>.  If <c>additionalInstructors</c>
	///     is <c>null</c> it is ignored.</param>
	///
    /// <param name="additionalLearners">A list of learners to add to those in the returned
	/// 	<c>SlkMemberships.Learners</c> list.  The resulting combined list is sorted and
	/// 	duplicates are removed.  <c>SlkUser.SPUser</c> is ignored and may be <c>null</c>
	/// 	for items in <paramref name="additionalLearners"/>.  If <c>additionalLearners</c> is
	///     <c>null</c> it is ignored.</param>
    /// 
    /// <param name="groupFailures">If one or more groups could not be enumerated, then this
    ///     parameter is set to a collection of the names of those groups.  Otherwise, this
    ///     parameter is set to <c>null</c>.</param>
    /// 
    /// <param name="groupFailureDetails">If one or more groups could not be enumerated, then this
    ///     parameter is set to additional information (beyond <paramref name="groupFailures"/>)
    ///     that is appropriate for storing in an error log for later review by an administrator.
    ///     This information may contain security details that are inappropriate for a browser
    ///     user to see.  If no group enumeration problem occurs, this parameter is set to
    ///     <c>null</c>.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the <a href="SlkApi.htm#AccessingSlkStore">current
	/// user</a> doesn't have SLK
	/// <a href="Microsoft.SharePointLearningKit.SlkSPSiteMapping.InstructorPermission.Property.htm">instructor</a>
	/// and SharePoint "Reader" permissions on <pr>spWeb</pr>.
	/// </remarks>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with.
    /// </exception>
	///
    /// <exception cref="UnauthorizedAccessException">
    /// The user is not a Reader on <pr>spWeb</pr>.
    /// </exception>
    ///
    /// <exception cref="NotAnInstructorException">
    /// The user is not an instructor on <pr>spWeb</pr>
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public SlkMemberships GetMemberships(SPWeb spWeb, IEnumerable<SlkUser> additionalInstructors,
		IEnumerable<SlkUser> additionalLearners, out ReadOnlyCollection<string> groupFailures,
        out string groupFailureDetails)
	{
        // Security checks: Fails if the user isn't an instructor and a Reader (implemented
        // by EnsureInstructor)

        // Check parameters
        if (spWeb == null)
            throw new ArgumentNullException("spWeb");

        // keep track of when this method started, for timeout purposes
        DateTime startTime = DateTime.Now;

        // Verify that the SPWeb is in the SPSite associated with this SlkStore, because if it
		// isn't then the code below below may be using the wrong SLK instructor and learner
		// permission names; for example, the SPSite of this SlkStore may name the instructor
		// permission "SLK Instructor", but <spWeb> may be in a different SPSite which might name
		// the instructor permission "SLK Teacher"
        if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        // since we use SPSecurity.RunWithElevatedPrivileges, we need to make sure the current
        // user is an instructor
        EnsureInstructor(spWeb);

        // track the names of groups for which member enumeration failed, and details about those
        // failures
        List<string> groupFailuresList = new List<string>();
        StringBuilder groupFailureDetailsBuilder = new StringBuilder();

        // set <instructorsByUserKey> and <learnersByUserKey> to dictionarys (keyed on SID) of
		// users who have the the SLK instructor permission and SLK learner permission,
		// respectively, in <spWeb>; set <users> to the union of <instructorsByUserKey> and
		// <learnersByUserKey>; set <learnerGroups> to the list of groups that have the SLK
		// learner permission in <spWeb>
        Dictionary<string, SlkUser> instructorsByUserKey = new Dictionary<string, SlkUser>(10);
        Dictionary<string, SlkUser> learnersByUserKey = new Dictionary<string, SlkUser>(100);
        Dictionary<string, SlkUser> users = new Dictionary<string, SlkUser>(100);
        List<SlkGroup> learnerGroups = new List<SlkGroup>(20);
        SlkSPSiteMapping mapping = m_anonymousSlkStore.Mapping;
        SPRoleDefinition instructorRoleDef, learnerRoleDef;
		try
		{
			instructorRoleDef = spWeb.RoleDefinitions[mapping.InstructorPermission];
		}
		catch (SPException)
		{
			instructorRoleDef = null; // no SLK instructor permission defined
		}
		try
		{
			learnerRoleDef = spWeb.RoleDefinitions[mapping.LearnerPermission];
		}
		catch (SPException)
		{
			learnerRoleDef = null; // no SLK learner permission defined
		}
        string spWebUrl = spWeb.Url;
        SPSecurity.RunWithElevatedPrivileges(delegate()
        {
            bool previousValue = SPSecurity.CatchAccessDeniedException;
			SPSite spSite2 = new SPSite(spWebUrl);
			SPWeb spWeb2 = spSite2.OpenWeb();
            spWeb2.AllowUnsafeUpdates = true; // enable spWeb2.AllUsers.AddCollection()
            try
            {
                foreach (SPRoleAssignment roleAssignment in spWeb2.RoleAssignments)
                {
					// determine if this role assignment refers to an instructor or a learner; skip
					// it if it's neither
                    bool isInstructor = (instructorRoleDef == null) ? false :
                        roleAssignment.RoleDefinitionBindings.Contains(instructorRoleDef);
                    bool isLearner =(learnerRoleDef == null) ? false :
						roleAssignment.RoleDefinitionBindings.Contains(learnerRoleDef);
                    if (!isInstructor && !isLearner)
                        continue;

					// process the role assignment
                    SPPrincipal member = roleAssignment.Member;
                    SPUser spUser = member as SPUser;
                    SPGroup spGroup = member as SPGroup;
                    if (spUser != null)
                    {
                        if (spUser.IsDomainGroup)
                        {
                            // role assignment member is a domain group
                            if (!EnumerateDomainGroupMembers(spWeb2, spUser,
                                isInstructor, isLearner, groupFailuresList,
                                groupFailureDetailsBuilder, instructorsByUserKey,
                                learnersByUserKey, users, null, learnerGroups, startTime))
                            {
                                // group enumeration failed -- skip this role assignment
                                continue;
                            }
                        }
                        else
                        {
                            // role assignment member is a SharePoint user
                            SlkUser slkUser;
                            //Use SPUser LoginName if Sid is Null or Empty.
                            string userKey
                                = String.IsNullOrEmpty(spUser.Sid) ? spUser.LoginName : spUser.Sid;
                            if (!users.TryGetValue(userKey, out slkUser))
                                slkUser = new SlkUser(null, spUser, spUser.Name);
                            if (isInstructor)
                                instructorsByUserKey[userKey] = slkUser;
                            if (isLearner)
                                learnersByUserKey[userKey] = slkUser;
                            users[userKey] = slkUser;
                        }
                    }
                    else
                    if (spGroup != null)
                    {
                        // role assignment member is a SharePoint group...

						// create a SlkGroup if this role assignment has the "SLK Learner"
						// permission
                        SlkGroup learnerGroup;
                        if (isLearner)
                        {
                            learnerGroup = new SlkGroup(spGroup, null);
                            learnerGroup.UserKeys = new List<string>(50);
                        }
                        else
                            learnerGroup = null;

						// add users from the domain group to the collections of instructors,
						// learners, and/or this learner group, as appropriate
                        foreach (SPUser spUserInGroup in spGroup.Users)
                        {
                            if (spUserInGroup.IsDomainGroup)
                            {
                                // role assignment member is a domain group
                                if (!EnumerateDomainGroupMembers(spWeb2, spUserInGroup,
                                    isInstructor, isLearner, groupFailuresList,
                                    groupFailureDetailsBuilder, instructorsByUserKey,
                                    learnersByUserKey, users,
                                    ((learnerGroup != null) ? learnerGroup.UserKeys : null),
                                    null, startTime))
                                {
                                    // group enumeration failed -- skip this role assignment
                                    continue;
                                }
                            }
                            else
                            {
                                SlkUser slkUser;
                                //Use SPUser LoginName if Sid is Null or Empty.
                                string userKey = String.IsNullOrEmpty(spUserInGroup.Sid) ?
                                                        spUserInGroup.LoginName : spUserInGroup.Sid;

                                if (!users.TryGetValue(userKey, out slkUser))
                                    slkUser = new SlkUser(null, spUserInGroup, spUserInGroup.Name);

                                if (isInstructor)
                                    instructorsByUserKey[userKey] = slkUser;
                                if (isLearner)
                                {
                                    learnersByUserKey[userKey] = slkUser;
                                    learnerGroup.UserKeys.Add(userKey);
                                }
                                users[userKey] = slkUser;
                            }
                        }
                        if (isLearner)
                            learnerGroups.Add(learnerGroup);
                    }
                }
            }
            finally
            {
				spWeb2.Dispose();
				spSite2.Dispose();
            }
        });

        // set <groupFailures> and <groupFailureDetails> if any group enumeration failures occurred
        if (groupFailuresList.Count > 0)
        {
            groupFailures = new ReadOnlyCollection<string>(groupFailuresList);
            groupFailureDetails = groupFailureDetailsBuilder.ToString();
        }
        else
        {
            groupFailures = null;
            groupFailureDetails = null;
        }

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();
        job.DisableFollowingSecurityChecks();

        // for each user in <users>, ask to get the user's UserItemIdentifier -- create a UserItem
        // for that user if they're not yet in the SLK database
        Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
        Dictionary<string, object> updateProperties = new Dictionary<string, object>();
        foreach (SlkUser user in users.Values)
        {
            uniqueProperties.Clear();
            //Use SPUser LoginName if Sid is Null or Empty.
            uniqueProperties[Schema.UserItem.Key] 
                = String.IsNullOrEmpty(user.SPUser.Sid) ?
                  user.SPUser.LoginName : user.SPUser.Sid;
            updateProperties.Clear();
            updateProperties[Schema.UserItem.Name] = user.Name;
            job.AddOrUpdateItem(Schema.UserItem.ItemTypeName, uniqueProperties, updateProperties,
                null, true);
        }

        // execute the job
        ReadOnlyCollection<object> results = job.Execute();
        IEnumerator<object> resultEnumerator = results.GetEnumerator();

        // for each user in <users>, retrieve the user's UserItemIdentifier from the job results
        foreach (SlkUser user in users.Values)
        {
            if (!resultEnumerator.MoveNext())
                throw new InternalErrorException("SLK1001");
            UserItemIdentifier userId;
            LearningStoreHelper.CastNonNull(resultEnumerator.Current, out userId);
            user.UserId = userId;
        }
		if (resultEnumerator.MoveNext())
			throw new InternalErrorException("SLK1002");

        // populate the Users property of each item in <learnerGroups>
        foreach (SlkGroup learnerGroup in learnerGroups)
        {
            foreach (string userKey in learnerGroup.UserKeys)
                learnerGroup.AddUser(users[userKey]);
        }

		// combine <instructorsByUserKey> and <additionalInstructors> (if any) into
		// <instructorsById>; we add the users from the SPWeb last, so that SharePoint user names
		// take precedence over LearningStore user names
		Dictionary<long, SlkUser> instructorsById =
			new Dictionary<long, SlkUser>(instructorsByUserKey.Count + 50);
		if (additionalInstructors != null)
		{
			foreach (SlkUser slkUser in additionalInstructors)
				instructorsById[slkUser.UserId.GetKey()] = slkUser;
		}
		foreach (SlkUser slkUser in instructorsByUserKey.Values)
			instructorsById[slkUser.UserId.GetKey()] = slkUser;

		// combine <learnersByUserKey> and <additionalLearners> (if any) into <learnersById>;
		// we add the users from the SPWeb last, so that SharePoint user names take precedence over
		// LearningStore user names
		Dictionary<long, SlkUser> learnersById =
			new Dictionary<long, SlkUser>(learnersByUserKey.Count + 50);
		if (additionalLearners != null)
		{
			foreach (SlkUser slkUser in additionalLearners)
				learnersById[slkUser.UserId.GetKey()] = slkUser;
		}
		foreach (SlkUser slkUser in learnersByUserKey.Values)
			learnersById[slkUser.UserId.GetKey()] = slkUser;

		// convert <instructorsById> into an array, <instructors>, sorted by name
		SlkUser[] instructors = new SlkUser[instructorsById.Keys.Count];
        instructorsById.Values.CopyTo(instructors, 0);
        Array.Sort(instructors);

		// convert <learnersById> into an array, <learners>, sorted by name
		SlkUser[] learners = new SlkUser[learnersById.Keys.Count];
        learnersById.Values.CopyTo(learners, 0);
        Array.Sort(learners);

		// sort <learnerGroups>
        learnerGroups.Sort();

		// return a new SlkMemberships object
    	return new SlkMemberships(instructors, learners, learnerGroups.ToArray());
	}

    /// <summary>
    /// Enumerates the members of a domain group.
    /// </summary>
    ///
    /// <param name="spWeb">An <c>SPWeb</c> within the site collection to which users from the
    ///     domain group will be added as needed.</param>
    ///
    /// <param name="domainGroup">The <c>SPUser</c> representing the domain group to enumerate.
    ///     </param>
    ///
    /// <param name="isInstructor"><c>true</c> if this domain group has the "SLK Instructor"
    ///     permission.</param>
    ///
    /// <param name="isLearner"><c>true</c> if this domain group has the "SLK Learner"
    ///     permission.</param>
    ///
    /// <param name="groupFailuresList">If enumeration fails, the name of this domain group will
    ///     added to this list.  This information can be displayed to the browser user.</param>
    ///
    /// <param name="groupFailureDetailsBuilder">If enumeration fails, details about the failure
    ///     will be appended to this <c>StringBuilder</c>.  This information may contain sensitive
    ///     details that should not be displayed to browser user -- it's intended for inclusion
    ///     within a log that can be perused at a later time by a system administrator.</param>
    ///
    /// <param name="instructorsByUserKey">Instructors found within the domain group are added
	/// 	to this dictionary; the key is the user's SLK "user key" (e.g. SID or login name);
	/// 	the value is a <c>SlkUser</c> object containing the user's <c>SPUser</c> and name.
	/// 	</param>
    ///
    /// <param name="learnersByUserKey">Learners found within the domain group are added
	/// 	to this dictionary; the key is the user's SLK "user key" (e.g. SID or login name);
	/// 	the value is a <c>SlkUser</c> object containing the user's <c>SPUser</c> and name.
	/// 	</param>
    ///
    /// <param name="users">All users found within the domain group are added to this dictionary;
	/// 	the key is the user's SLK "user key" (e.g. SID or login name); the value is a
	/// 	<c>SlkUser</c> object containing the user's <c>SPUser</c> and name.</param>
	///
    /// <param name="learnerKeys">The SLK "user key" (e.g. SID or login name) of each learner is
    /// 	added to this collection, unless <paramref name="learnerKeys"/> is <c>null</c>.
    ///     </param>
	///
    /// <param name="learnerGroups">If this domain group has the "SLK Learner" permission then
	/// 	a <c>SlkGroup</c> referring to this domain group is added to this list, unless
	/// 	<paramref name="learnerGroups"/> is <c>null</c>.</param>
    ///
    /// <param name="startTime">The time that the timeout period started.  If this method runs
	/// 	past this time plus <c>DomainGroupEnumerationTotalTimeout</c> (approximately), this
	///		method will time out (updating <paramref name="groupFailuresList"/> and
	///		<paramref name="groupFailureDetailsBuilder"/> as needed, and returning <c>false</c>).
	/// 	</param>
	///
    /// <returns>
	/// <c>true</c> if enumeration succeeds, <c>false</c> if not.
	/// </returns>
	///
    static bool EnumerateDomainGroupMembers(SPWeb spWeb, SPUser domainGroup, bool isInstructor,
		bool isLearner, List<string> groupFailuresList, StringBuilder groupFailureDetailsBuilder, 
		Dictionary<string, SlkUser> instructorsByUserKey, 
		Dictionary<string, SlkUser> learnersByUserKey, Dictionary<string, SlkUser> users, 
        List<string> learnerKeys, List<SlkGroup> learnerGroups, DateTime startTime)
    {
        // if timeout occurred, output a message to <groupFailureDetailsBuilder>
        TimeSpan timeRemaining = DomainGroupEnumerationTotalTimeout - (DateTime.Now - startTime);
        if (timeRemaining <= TimeSpan.Zero)
        {
            groupFailureDetailsBuilder.AppendFormat(
                AppResources.DomainGroupEnumSkippedDueToTimeout, domainGroup.LoginName);
            groupFailureDetailsBuilder.Append("\r\n\r\n");
            return false;
        }

        // get the users in this group, in the form of a collection of SPUserInfo objects 
        ICollection<SPUserInfo> spUserInfos;
        try
        {
            spUserInfos = DomainGroupUtilities.EnumerateDomainGroup(domainGroup.LoginName,
				timeRemaining);
        }
        catch (DomainGroupEnumerationException ex)
        {
            groupFailuresList.Add(domainGroup.Name);
            groupFailureDetailsBuilder.AppendFormat(AppResources.DomainGroupError,
				domainGroup.LoginName, ex);
            groupFailureDetailsBuilder.Append("\r\n\r\n");
            return false;
        }

        // convert <spUserInfos> to <spUsers> (a collection of SPUser); add users to the site
		// collection as needed
        List<SPUserInfo> usersToAddToSPSite = null;
        List<SPUser> spUsers = new List<SPUser>(spUserInfos.Count);
        foreach (SPUserInfo spUserInfo in spUserInfos)
        {
            try
            {
                SPUser spUserInGroup = spWeb.AllUsers[spUserInfo.LoginName];
                spUsers.Add(spUserInGroup);
            }
            catch (SPException)
            {
                // need to add user to site collection (below)
                if (usersToAddToSPSite == null)
                    usersToAddToSPSite = new List<SPUserInfo>();
                usersToAddToSPSite.Add(spUserInfo);
            }
        }
        if (usersToAddToSPSite != null)
        {
            try
            {
                spWeb.AllUsers.AddCollection(usersToAddToSPSite.ToArray());
                foreach (SPUserInfo spUserInfo in usersToAddToSPSite)
                {
                    SPUser spUserInGroup = spWeb.AllUsers[spUserInfo.LoginName];
                    spUsers.Add(spUserInGroup);
                }
            }
            catch (SPException ex)
            {
                groupFailuresList.Add(domainGroup.Name);
                groupFailureDetailsBuilder.AppendFormat(AppResources.ErrorCreatingSPSiteUser,
					spWeb.Site.Url, ex);
                groupFailureDetailsBuilder.Append("\r\n\r\n");
                return false;
            }
        }

        // create a SlkGroup if this role assignment has the "SLK Learner" permission, unless
		// <learnerGroups> is null
        SlkGroup learnerGroup;
        if (isLearner && (learnerGroups != null))
        {
            learnerGroup = new SlkGroup(null, domainGroup);
            learnerGroup.UserKeys = new List<string>(spUserInfos.Count);
        }
        else
            learnerGroup = null;

        // add users from the domain group to the collections of instructors, learners, and/or this
		// learner group, as appropriate
        foreach (SPUser spUserInGroup in spUsers)
        {
            SlkUser slkUser;
            //Use SPUser LoginName if Sid is Null or Empty.
            string userKey = String.IsNullOrEmpty(spUserInGroup.Sid) ?
                                    spUserInGroup.LoginName : spUserInGroup.Sid;

            if (!users.TryGetValue(userKey, out slkUser))
                slkUser = new SlkUser(null, spUserInGroup, spUserInGroup.Name);

            if (isInstructor)
                instructorsByUserKey[userKey] = slkUser;
            if (isLearner)
            {
                learnersByUserKey[userKey] = slkUser;
				if (learnerGroup != null)
					learnerGroup.UserKeys.Add(userKey);
                if (learnerKeys != null)
    				learnerKeys.Add(userKey);
            }
            users[userKey] = slkUser;
        }
        if (learnerGroup != null)
            learnerGroups.Add(learnerGroup);

        return true;
    }

    /// <summary>
    /// Returns an <c>AssignmentProperties</c> object populated with default information for
    /// a new assignment based on a given e-learning package or non-e-learning document.
    /// This method doesn't actually create the assignment -- it just returns information that
    /// can be used as defaults for a form that a user would fill in to create a new assignment.
    /// </summary>
    ///
    /// <param name="destinationSPWeb">The <c>SPWeb</c> that the assignment would be assigned in.</param>
    ///
    /// <param name="location">The MLC SharePoint location string that refers to the e-learning
    ///     package or non-e-learning document that would be assigned.  Use
    ///     <c>SharePointPackageStore.GetLocation</c> to construct this string.</param>
    ///
    /// <param name="organizationIndex">The zero-based index of the organization within the
    ///     e-learning content to assign; this is the value that's used as an index to
    ///     <c>ManifestReader.Organizations</c>.  If the content being assigned is a non-e-learning
    ///     document, use <c>null</c> for <paramref name="organizationIndex"/>.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.
	/// 	Use <c>SlkRole.Learner</c> to get default information for a self-assigned assignment,
	/// 	i.e. an assignment with no instructors for which the current learner is the only
	/// 	learner.  Otherwise, use <c>SlkRole.Instructor</c>.</param>
	///
    /// <param name="packageWarnings">Where returned package-related warnings are stored.  This
	/// 	XML consists of a root "&lt;Warnings&gt;" element containing one "&lt;Warning&gt;"
	/// 	element per warning, each of which contains the text of the warning as the content of
	/// 	the element plus the following attributes: the "Code" attribute contains the warning's
    /// 	<c>ValidationResultCode</c>, and the "Type" attribute contains the warning's
    ///     type, either "Error" or "Warning".  <paramref name="packageWarnings"/> is set to
	/// 	<c>null</c> if there are no warnings.</param>
	///
	/// <remarks>
	/// <para>
	/// If <paramref name="slkRole"/> is <c>SlkRole.Learner</c>, default properties for a
	/// self-assigned assignment are returned.  In this case, the returned
	/// <c>AssignmentProperties</c> will contain no users in 
	/// <c>AssignmentProperties.Instructors</c>, and <c>AssignmentProperties.Learners</c> will
	/// contain only the current user.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; If <pr>slkRole</pr> is
	/// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Instructor</a>,
	/// this operation fails if the <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't
	/// have SLK
	/// <a href="Microsoft.SharePointLearningKit.SlkSPSiteMapping.InstructorPermission.Property.htm">instructor</a>
    /// permissions on <pr>destinationSPWeb</pr>.  Also fails if the current user doesn't have access to the
	/// package/file.
	/// </para>
	/// </remarks>
	///
    /// <exception cref="FileNotFoundException">
    /// <pr>location</pr> refers to a file that does not exist.
    /// </exception>
    /// 
    /// <exception cref="InvalidOperationException">
    /// <pr>destinationSPWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated
	/// with.
    /// </exception>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the package has
	/// problems severe enough to prevent it from being assignable within SharePoint Learning Kit.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to, or the user is not
    /// a Reader on <pr>destinationSPWeb</pr>
    /// </exception>
    ///
    /// <exception cref="NotAnInstructorException">
    /// The user is not an instructor on <pr>destinationSPWeb</pr> and the assignment is not
    /// self-assigned.
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public AssignmentProperties GetNewAssignmentDefaultProperties(SPWeb destinationSPWeb,
		string location, Nullable<int> organizationIndex, SlkRole slkRole,
		out LearningStoreXml packageWarnings)
	{
        // Security checks: Fails if the user isn't an instructor on the web if SlkRole=Instructor
        // (verified by EnsureInstructor).  Fails if the user doesn't have access to the
        // package (verified by calling RegisterPackage or by accessing
        // the properties of the non-elearning file).

        // Check parameters
        if (destinationSPWeb == null)
            throw new ArgumentNullException("destinationSPWeb");
        if (location == null)
            throw new ArgumentNullException("location");
        if((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            throw new ArgumentOutOfRangeException("slkRole");

        // Verify that the web is in the site
        if (destinationSPWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        // Set <isSelfAssigned> to true if this is a self-assigned assignment (no instructors,
        // user is the only learner); verify that the current user is an instructor if
        // necessary
		bool isSelfAssigned;
		if (slkRole == SlkRole.Instructor)
		{
            EnsureInstructor(destinationSPWeb);
			isSelfAssigned = false;
		}
		else
		if (slkRole == SlkRole.Learner)
		{
			isSelfAssigned = true;
		}
		else
			throw new ArgumentException(AppResources.InvalidSlkRole, "slkRole");

		// set <currentUserId> to the UserItemIdentifier of the current user; note that this
		// requires a round trip to the database
        UserItemIdentifier currentUserId = GetCurrentUserId();

        // get information about the package/file being assigned
        string title;
        string description;
        float? pointsPossible;
        PackageFormat? packageFormat;
        if (organizationIndex != null)
        {
            // <location> refers to an e-learning package (e.g. SCORM)...

			// register the package with PackageStore (if it's not registered yet), and set
			// <packageWarnings> to warnings about the content (or null if none)
			PackageItemIdentifier packageId;
			RegisterPackage(location, out packageId, out packageWarnings);

            // The RegisterPackage call above will fail if the user doesn't have access to
            // the package.  Therefore, since we've already verified that the user has
            // access to the package, we don't need to check package-related security
            // again.  So just perform the package-related operations within a
            // privileged scope
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                SPFile spFile;
                SharePointFileLocation fileLocation;
                
                // set <spFile> and <packageReader> to refer to the package
                GetFileFromSharePointLocation(location, out spFile, out fileLocation);
                using (PackageReader packageReader = PackageStore.GetPackageReader(packageId))
				{
					// set <manifestReader> and <metadataReader> to refer to the package; set <title> and
					// <description> to the package-level title and description
					ManifestReader manifestReader;
					GetPackageInformation(packageReader, spFile, out manifestReader,
						out title, out description);

					// validate <organizationIndex>
					if ((organizationIndex.Value < 0) ||
						(organizationIndex.Value >= manifestReader.Organizations.Count))
						throw new SafeToDisplayException(AppResources.InvalidOrganizationIndex);

					// set <organizationNodeReader> to refer to the organization
					OrganizationNodeReader organizationNodeReader =
						manifestReader.Organizations[organizationIndex.Value];

					// if there is more than one organization, append the organization title, if any
					if (manifestReader.Organizations.Count > 1)
					{
						if (!String.IsNullOrEmpty(organizationNodeReader.Title))
                            title = String.Format(CultureInfo.CurrentCulture, 
                                                  AppResources.SlkPackageAndOrganizationTitle, title, organizationNodeReader.Title);
					}

					// set <pointsPossible> to the points-possible value stored in the manifest, or null
					// if none
					switch (manifestReader.PackageFormat)
					{
					case PackageFormat.Lrm:
						pointsPossible = organizationNodeReader.PointsPossible;
						break;
					case PackageFormat.V1p3:
						pointsPossible = 100;
						break;
					default:
						pointsPossible = null;
						break;
					}

					// set <packageFormat>
					packageFormat = manifestReader.PackageFormat;
				}
            }
        }
        else
        {
            // <location> refers to a non-e-learning package (e.g. Microsoft Word)...

			// set <title> and <description>
            SPFile spFile = SlkUtilities.GetSPFileFromPackageLocation(location);
			title = spFile.Title;
			if (String.IsNullOrEmpty(title))
				title = Path.GetFileNameWithoutExtension(spFile.Name);
			description = "";

			// "Points Possible" defaults to null for non-e-learning content
			pointsPossible = null;

            // no package warnings
            packageWarnings = null;

            // non-e-learning package
            packageFormat = null;
        }

        // return a new AssignmentProperties object populated with the information retrieved above
        // plus a few defaults
        AssignmentProperties ap = new AssignmentProperties();
        ap.Title = title;
        ap.Description = description;
        ap.PointsPossible = pointsPossible;
		ap.StartDate = DateTime.Today; // midnight today
        ap.DueDate = null;
    	ap.AutoReturn = isSelfAssigned;
        ap.ShowAnswersToLearners = isSelfAssigned;
    	ap.CreatedById = currentUserId;
		if (isSelfAssigned)
			ap.Learners.Add(new SlkUser(currentUserId));
		else
		{
			ap.Instructors.Add(new SlkUser(currentUserId));
		}
        ap.PackageFormat = packageFormat;
		return ap;
	}

    /// <summary>
    /// Retrieves an <n>SPFile</n> given an MLC SharePoint location string.
    /// </summary>
    ///
    /// <param name="location">The MLC SharePoint location string.</param>
    ///
    /// <param name="spFile">The retrieved <n>SPFile</n>.</param>
    ///
    /// <param name="spFileLocation">The <r>SharePointFileLocation</r> corresponding to
    ///     <pr>location</pr>.</param>
	///
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to.
    /// </exception>
	///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
    private static void GetFileFromSharePointLocation(string location,
        out SPFile spFile, out SharePointFileLocation spFileLocation)
    {
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by accessing the assignment properties)

        if (!SharePointFileLocation.TryParse(location, out spFileLocation))
            throw new ArgumentException(AppResources.IncorrectLocationStringSyntax, "location");

        using (SPSite spSite = new SPSite(spFileLocation.SiteId))
        {
			// get <spWeb>, <spFile>, and <versionId> from the parsed information above
            using(SPWeb spWeb = spSite.OpenWeb(spFileLocation.WebId))
            {
                spFile = spWeb.GetFile(spFileLocation.FileId);
                Hashtable fileProperties = spFile.Properties;
                GC.KeepAlive(fileProperties);
            }
        }
    }

    /// <summary>
    /// Retrieves a <r>SharePointPackageReader</r> given an MLC SharePoint location string.
    /// </summary>
    ///
    /// <param name="location">The MLC SharePoint location string.</param>
    ///
    /// <param name="cacheSettings">Settings used to determine how the file cached used by
    ///     <r>SharePointPackageReader</r> operates.</param>
    ///
    /// <param name="spFile">On exit, contains the <n>SPFile</n> referred to by
    ///     <pr>location</pr>.</param>
    ///
    /// <returns>
    /// The <r>SharePointPackageReader</r> referred to by <pr>location</pr>.
    /// </returns>
	///
    /// <exception cref="FileNotFoundException">
    /// <pr>location</pr> refers to a file that does not exist.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to.
    /// </exception>
	///
    private static PackageReader GetPackageReaderFromSharePointLocation(string location,
        SharePointCacheSettings cacheSettings, out SPFile spFile)
    {
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by GetFileFromSharePointLocation and SharePointPackageReader)

        SharePointFileLocation spFileLocation;
        GetFileFromSharePointLocation(location, out spFile, out spFileLocation);
		return new SharePointPackageReader(cacheSettings, spFileLocation, false);
    }
    
	/// <summary>
	/// Retrieves general information about an assignment.
	/// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	retrieve information about.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
	///
	/// <returns>
	/// An <c>AssignmentProperties</c> object containing information about the assignment.
	/// Note that only the <c>UserId</c> of each <c>SlkUser</c> object within the returned
	/// <c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>
	/// collections is valid.
	/// </returns>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; If <pr>slkRole</pr> is
	/// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Instructor</a>,
	/// this operation fails if the <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't
	/// an instructor on the assignment.  If <pr>slkRole</pr> is
	/// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Learner</a>,
	/// this operation fails if the current user isn't a learner on the assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user
	/// isn't an instructor on the assignment (if <pr>slkRole</pr> is <r>SlkRole.Instructor</r>),
	/// or the user isn't a learner on the assignment (if <pr>slkRole</pr> is
	/// <r>SlkRole.Learner</r>).
    /// </exception>
	///
	public AssignmentProperties GetAssignmentProperties(
		AssignmentItemIdentifier assignmentId, SlkRole slkRole)
	{
        // Security checks: Fails if the user isn't an instructor
        // on the assignment (if SlkRole=Instructor) or if the user isn't
        // a learner on the assignment (if SlkRole=Learner), since it
        // calls the other GetAssignmentProperties method.

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");
        if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            throw new ArgumentOutOfRangeException("slkRole");

        return GetAssignmentProperties(assignmentId, slkRole, false);
	}

	/// <summary>
	/// Updates general information about an assignment.
	/// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	store information about.</param>
	///
	/// <param name="newProperties">An <c>AssignmentProperties</c> object containing updated
	/// 	information about the assignment.  The following properties of
	/// 	<c>AssignmentProperties</c> are not updatable and are ignored: <c>SPSiteGuid</c>,
	/// 	<c>SPWebGuid</c>, <c>CreatedBy</c>, <c>DateCreated</c>.  Also, within
	/// 	<c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>, all
	/// 	properties except <c>UserId</c> are ignored.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; This operation fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
	/// assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user isn't
	/// an instructor on the assignment.
    /// </exception>
	///
    public void SetAssignmentProperties(AssignmentItemIdentifier assignmentId,
        AssignmentProperties newProperties)
    {
        // Security checks: Fails if the user isn't an instructor on the assignment
        // (since it calls GetAssignmentProperties with SlkRole.Instructor)

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");
        if (newProperties == null)
            throw new ArgumentNullException("newProperties");

        // perform the operation within a transaction so that if the operation fails no data is
		// committed to the database
        TransactionOptions options = new TransactionOptions();
        options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(options))
        {
			// set <oldProperties> to the current (stored-in-database) properties of the assignment
			AssignmentProperties oldProperties = GetAssignmentProperties(assignmentId,
                SlkRole.Instructor);

            // The above line will fail if the user doesn't have instructor access to the assignment.
            // Once we've verified that he does, we don't need to perform any other
            // security checks
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
			    // create a LearningStore job
			    LearningStoreJob job = LearningStore.CreateJob();

                // request the InstructorAssignmentItemIdentifier of each instructor that the caller
			    // wants removed from the assignment
                foreach (SlkUser slkUser in oldProperties.Instructors)
                {
                    if (!newProperties.Instructors.Contains(slkUser.UserId))
                    {
                        // the caller wants to delete instructor <slkUser>
					    LearningStoreQuery query = LearningStore.CreateQuery(
						    Schema.InstructorAssignmentItem.ItemTypeName);
					    query.AddColumn(Schema.InstructorAssignmentItem.Id);
                        query.AddCondition(Schema.InstructorAssignmentItem.AssignmentId,
                            LearningStoreConditionOperator.Equal, assignmentId);
                        query.AddCondition(Schema.InstructorAssignmentItem.InstructorId,
                            LearningStoreConditionOperator.Equal, slkUser.UserId);
                        job.PerformQuery(query);
                    }
                }

                // request the LearnerAssignmentItemIdentifier of each learner that the caller wants
			    // removed from the assignment
                foreach (SlkUser slkUser in oldProperties.Learners)
                {
                    if (!newProperties.Learners.Contains(slkUser.UserId))
                    {
                        // the caller wants to delete learner <slkUser>
					    LearningStoreQuery query = LearningStore.CreateQuery(
						    Schema.LearnerAssignmentItem.ItemTypeName);
					    query.AddColumn(Schema.LearnerAssignmentItem.Id);
                        query.AddCondition(Schema.LearnerAssignmentItem.AssignmentId,
                            LearningStoreConditionOperator.Equal, assignmentId);
                        query.AddCondition(Schema.LearnerAssignmentItem.LearnerId,
                            LearningStoreConditionOperator.Equal, slkUser.UserId);
                        job.PerformQuery(query);
                    }
                }

			    // set <itemsToDelete> to the list of LearningStore items to delete
			    ReadOnlyCollection<object> results = job.Execute();
			    List<LearningStoreItemIdentifier> itemsToDelete =
				    new List<LearningStoreItemIdentifier>(results.Count);
			    foreach (DataTable result in results)
                {
                    if (result.Rows.Count != 1)
                        throw new InternalErrorException("SLK1014");
                    itemsToDelete.Add((LearningStoreItemIdentifier)result.Rows[0][0]);
                }

			    // create a new LearningStore job
			    job = LearningStore.CreateJob();

			    // update properties of the assignment
			    Dictionary<string,object> properties = new Dictionary<string,object>();
			    properties[Schema.AssignmentItem.Title] = newProperties.Title;
			    properties[Schema.AssignmentItem.StartDate] =
					newProperties.StartDate.ToUniversalTime();
                properties[Schema.AssignmentItem.DueDate] =
                    ((newProperties.DueDate == null)
					 ? (DateTime?) null : newProperties.DueDate.Value.ToUniversalTime());
			    properties[Schema.AssignmentItem.PointsPossible] = newProperties.PointsPossible;
			    properties[Schema.AssignmentItem.Description] = newProperties.Description;
			    properties[Schema.AssignmentItem.AutoReturn] = newProperties.AutoReturn;
			    properties[Schema.AssignmentItem.ShowAnswersToLearners] =
				    newProperties.ShowAnswersToLearners;
			    job.UpdateItem(assignmentId, properties);

                // add instructors that the caller wants added to the assignment
                foreach (SlkUser slkUser in newProperties.Instructors)
                {
                    if (!oldProperties.Instructors.Contains(slkUser.UserId))
                    {
                        // add instructor <slkUser>
                        properties.Clear();
					    properties[Schema.InstructorAssignmentItem.AssignmentId] = assignmentId;
                        properties[Schema.InstructorAssignmentItem.InstructorId] = slkUser.UserId;
					    job.AddItem(Schema.InstructorAssignmentItem.ItemTypeName, properties);
                    }
                }

                // add learners that the caller wants added to the assignment
                foreach (SlkUser slkUser in newProperties.Learners)
                {
                    if (!oldProperties.Learners.Contains(slkUser.UserId))
                    {
                        // add learner <slkUser>
                        properties.Clear();
                        properties[Schema.LearnerAssignmentItem.AssignmentId] = assignmentId;
                        properties[Schema.LearnerAssignmentItem.LearnerId] = slkUser.UserId;
					    properties[Schema.LearnerAssignmentItem.IsFinal] = false;
					    properties[Schema.LearnerAssignmentItem.NonELearningStatus] = null;
					    properties[Schema.LearnerAssignmentItem.FinalPoints] = null;
					    properties[Schema.LearnerAssignmentItem.InstructorComments] = "";
					    job.AddItem(Schema.LearnerAssignmentItem.ItemTypeName, properties);
                    }
                }

			    // delete instructors and learners that the caller wanted to delete from the
			    // assignment
			    foreach (LearningStoreItemIdentifier itemToDelete in itemsToDelete)
				    job.DeleteItem(itemToDelete);

                // execute the LearningStore job
                job.Execute();
            }
            
			// finish the transaction
            scope.Complete();
        }
    }

    /// <summary>
    /// Creates a new SharePoint Learning Kit assignment.
    /// </summary>
    ///
    /// <param name="destinationSPWeb">The <c>SPWeb</c> to create the assignment in.</param>
    ///
    /// <param name="location">The MLC SharePoint location string that refers to the e-learning
    ///     package or non-e-learning document to assign.  Use
    ///     <c>SharePointPackageStore.GetLocation</c> to construct this string.</param>
    ///
    /// <param name="organizationIndex">The zero-based index of the organization within the
    ///     e-learning content to assign; this is the value that's used as an index to
    ///     <c>ManifestReader.Organizations</c>.  If the content being assigned is a non-e-learning
    ///     document, use <c>null</c> for <paramref name="organizationIndex"/>.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> to use when creating the assignment.  Use
	/// 	<c>SlkRole.Learner</c> to create a self-assigned assignment, i.e. an assignment with
	/// 	no instructors for which the current learner is the only learner.  Otherwise, use
	/// 	<c>SlkRole.Instructor</c>.</param>
	///
	/// <param name="properties">Properties of the new assignment.  Note that, within
	/// 	<c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>, all
	/// 	properties except <c>UserId</c> are ignored.  Also, if <paramref name="slkRole"/> is
	/// 	<c>SlkRole.Learner</c>, then <c>AssignmentProperties.Instructors</c> must be
	/// 	empty, and <c>AssignmentProperties.Learners</c> must contain only the current user.
    ///     The <c>SPSiteGuid</c>, <c>SPWebGuid</c>, <c>Location</c>, and <c>CreatedById</c>
    ///     properties are ignored when creating a new assignment.
	/// 	</param>
	///
	/// <remarks>
	/// <para>
	/// When creating a self-assigned assignment, take care to ensure that
	/// <r>AssignmentProperties.AutoReturn</r> is <n>true</n>, otherwise the learner assignments
	/// will never reach
	/// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a>
	/// state.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; If <pr>slkRole</pr> is
	/// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Instructor</a>,
	/// this operation fails if the <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't
	/// have SLK
	/// <a href="Microsoft.SharePointLearningKit.SlkSPSiteMapping.InstructorPermission.Property.htm">instructor</a>
    /// permissions on <pr>destinationSPWeb</pr>.  Also fails if the current user doesn't have access to the
	/// package/file.
	/// </para>
	/// </remarks>
	///
    /// <exception cref="InvalidOperationException">
    /// <pr>destinationSPWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated
	/// with.
    /// </exception>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to, the user is not
    /// a Reader on <pr>destinationSPWeb</pr>, or the user is trying to create an assignment
    /// with other learners or instructors but the assignment is self-assigned.
    /// </exception>
    ///
    /// <exception cref="NotAnInstructorException">
    /// The user is not an instructor on <pr>destinationSPWeb</pr> and the assignment is not self-assigned.
    /// </exception>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
    public AssignmentItemIdentifier CreateAssignment(SPWeb destinationSPWeb, string location,
		Nullable<int> organizationIndex, SlkRole slkRole, AssignmentProperties properties)
	{
        // Security checks: If assigning as an instructor, fails if user isn't an instructor on
        // the web (implemented by calling EnsureInstructor).  Fails if the user doesn't have access
        // to the package/file (implemented by calling RegisterPackage or accessing the properties
        // of the file)

        // Check parameters
        if (destinationSPWeb == null)
            throw new ArgumentNullException("destinationSPWeb");
        if (location == null)
            throw new ArgumentNullException("location");
        if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            throw new ArgumentOutOfRangeException("slkRole");
        if(properties == null)
            throw new ArgumentNullException("properties");
            
        // Verify that the web is in the site
        if (destinationSPWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

        // Set <isSelfAssigned> to true if this is a self-assigned assignment (no instructors,
        // user is the only learner); verify that the current user is an instructor if
        // necessary
        bool isSelfAssigned;
		if (slkRole == SlkRole.Instructor)
		{
			isSelfAssigned = false;
		}
		else
		if (slkRole == SlkRole.Learner)
		{
			isSelfAssigned = true;
		}
		else
			throw new ArgumentException(AppResources.InvalidSlkRole, "slkRole");

		// do some security checking
		if (isSelfAssigned)
		{
			// self-assigned assignment...

			// set <currentUserId> to the UserItemIdentifier of the current user; note that this
			// requires a round trip to the database
			UserItemIdentifier currentUserId = GetCurrentUserId();

			// verify that <properties> specify no instructors and that the current user is the
			// only learner
			if (properties.Instructors.Count != 0)
				throw new UnauthorizedAccessException(AppResources.AccessDenied);
			if ((properties.Learners.Count != 1) ||
			    (properties.Learners[0].UserId != currentUserId))
				throw new UnauthorizedAccessException(AppResources.AccessDenied);
		}
        else
        {
            // regular (non self-assigned) assignment...

            EnsureInstructor(destinationSPWeb);
        }
        
        // set <rootActivityId> to the ID of the organization, or null if a non-e-learning
		// document is being assigned
        ActivityPackageItemIdentifier rootActivityId;
		if (organizationIndex != null)
		{
            // register the package with PackageStore (if it's not registered yet), and set
            // <packageId> to its PackageItemIdentifier
            PackageItemIdentifier packageId;
            LearningStoreXml packageWarnings;
            RegisterPackage(location, out packageId, out packageWarnings);

            // The RegisterPackage call above will fail if the user doesn't have access to
            // the package.  Therefore, since we've already verified that the user has
            // access to the package, we don't need to check package-related security
            // again.  So just perform the package-related operations within a
            // privileged scope
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                // set <rootActivityId>
			    LearningStoreJob job = LearningStore.CreateJob();
			    LearningStoreQuery query = LearningStore.CreateQuery(
				    Schema.ActivityPackageItemView.ViewName);
                query.AddColumn(Schema.ActivityPackageItemView.Id);
                query.AddCondition(Schema.ActivityPackageItemView.PackageId,
				    LearningStoreConditionOperator.Equal, packageId);
                query.AddCondition(Schema.ActivityPackageItemView.ParentActivityId,
                    LearningStoreConditionOperator.Equal, null);
                query.AddSort(Schema.ActivityPackageItemView.OriginalPlacement,
				    LearningStoreSortDirection.Ascending);
                job.PerformQuery(query);
			    DataRowCollection result = job.Execute<DataTable>().Rows;
			    if (result.Count == 0)
			    {
                    // this error message includes the package ID, but that package ID came from
                    // another API call (above) -- it's pretty unlikely the user will see it
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, 
					    AppResources.PackageNotFoundInDatabase, packageId.GetKey()));
			    }
			    if ((organizationIndex < 0) || (organizationIndex >= result.Count))
                    throw new SafeToDisplayException(AppResources.InvalidOrganizationIndex);
			    LearningStoreHelper.CastNonNull(
                    result[organizationIndex.Value][Schema.ActivityPackageItemView.Id],
				    out rootActivityId);
            }
		}
		else
		{
		    // Access the properties of the file to verify that the user has
		    // access to the file
            SPFile file = SlkUtilities.GetSPFileFromPackageLocation(location);
		    Hashtable fileProperties = file.Properties;
		    GC.KeepAlive(fileProperties);
		    
			rootActivityId = null;
        }

        // create the assignment; set <assignmentId> to its AssignmentItemIdentifier
        AssignmentItemIdentifier assignmentId;
        TransactionOptions options = new TransactionOptions();
        options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(options))
        {
            // We've already verified all the security above, so just turn off
            // security checks for the rest of this
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {            
			    // create a LearningStore job
			    LearningStoreJob job = LearningStore.CreateJob();

                // request the UserItemIdentifier of the current user -- create a UserItem for that user if
                // they're not yet in the SLK database
                Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
                Dictionary<string, object> updateProperties = new Dictionary<string, object>();
                uniqueProperties[Schema.UserItem.Key] = CurrentUserKey;
                updateProperties[Schema.UserItem.Name] = CurrentUserName;
                UserItemIdentifier currentUserId = new UserItemIdentifier(job.AddOrUpdateItem(
                    Schema.UserItem.ItemTypeName, uniqueProperties, updateProperties, null, false));

                // create an AssignmentItem corresponding to the properties of this object; set
			    // <tempAssignmentId> to a temporary AssignmentItemIdentifier (that can be used only
			    // within this job) of the new assignment
			    // to the UserItemIdentifier of the instructor
			    bool isNonELearning = (organizationIndex == null);
			    Dictionary<string, object> dbProperties = new Dictionary<string, object>();
                dbProperties[Schema.AssignmentItem.SPSiteGuid] = destinationSPWeb.Site.ID;
                dbProperties[Schema.AssignmentItem.SPWebGuid] = destinationSPWeb.ID;
			    dbProperties[Schema.AssignmentItem.Title] = properties.Title;
			    dbProperties[Schema.AssignmentItem.StartDate] = properties.StartDate.ToUniversalTime();
			    dbProperties[Schema.AssignmentItem.DueDate] =
				    ((properties.DueDate == null) ? null :
					    (object) properties.DueDate.Value.ToUniversalTime());
			    dbProperties[Schema.AssignmentItem.PointsPossible] = properties.PointsPossible;
			    if (isNonELearning)
			    {
				    dbProperties[Schema.AssignmentItem.RootActivityId] = null;
				    dbProperties[Schema.AssignmentItem.NonELearningLocation] = location;
			    }
			    else
			    {
				    dbProperties[Schema.AssignmentItem.RootActivityId] = rootActivityId;
				    dbProperties[Schema.AssignmentItem.NonELearningLocation] = null;
			    }
			    dbProperties[Schema.AssignmentItem.Description] = properties.Description;
			    dbProperties[Schema.AssignmentItem.AutoReturn] = properties.AutoReturn;
			    dbProperties[Schema.AssignmentItem.ShowAnswersToLearners] =
				    properties.ShowAnswersToLearners;
			    dbProperties[Schema.AssignmentItem.CreatedBy] = currentUserId;
			    dbProperties[Schema.AssignmentItem.DateCreated] = DateTime.Now.ToUniversalTime();
			    AssignmentItemIdentifier tempAssignmentId = new AssignmentItemIdentifier(
				    job.AddItem(Schema.AssignmentItem.ItemTypeName, dbProperties, true));

			    // create one InstructorAssignmentItem for each instructor of the assignment
			    foreach (SlkUser instructor in properties.Instructors)
			    {
				    dbProperties = new Dictionary<string, object>();
				    dbProperties[Schema.InstructorAssignmentItem.AssignmentId] = tempAssignmentId;
				    dbProperties[Schema.InstructorAssignmentItem.InstructorId] = instructor.UserId;
				    job.AddItem(Schema.InstructorAssignmentItem.ItemTypeName, dbProperties);
			    }

			    // create one LearnerAssignmentItem for each learner of the assignment
			    foreach (SlkUser learner in properties.Learners)
			    {
				    dbProperties = new Dictionary<string, object>();
				    dbProperties[Schema.LearnerAssignmentItem.AssignmentId] = tempAssignmentId;
				    dbProperties[Schema.LearnerAssignmentItem.LearnerId] = learner.UserId;
				    dbProperties[Schema.LearnerAssignmentItem.IsFinal] = false;
				    dbProperties[Schema.LearnerAssignmentItem.NonELearningStatus] = null;
				    dbProperties[Schema.LearnerAssignmentItem.FinalPoints] = null;
				    dbProperties[Schema.LearnerAssignmentItem.InstructorComments] = "";
				    job.AddItem(Schema.LearnerAssignmentItem.ItemTypeName, dbProperties);
			    }

			    // execute the job
			    ReadOnlyCollection<object> results = job.Execute();

			    // execute the job; set <assignmentId> to the "real" (permanent)
			    // AssignmentItemIdentifier of the newly-created assignment
			    LearningStoreHelper.CastNonNull(results[0], out assignmentId);

			    // finish the transaction
                scope.Complete();
            }
        }

		return assignmentId;
	}

    /// <summary>
    /// Deletes a SharePoint Learning Kit assignment.
    /// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	delete.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; This operation fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't either an instructor on the
	/// assignment or the person who created the assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user
	/// isn't an instructor on the assignment, and the assignment isn't self-assigned.
    /// </exception>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public void DeleteAssignment(AssignmentItemIdentifier assignmentId)
	{
        // Security checks: Fails if not an instructor on the assignment or not the user who
		// created the assignment (because of rules in the schema XML)

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();

		// delete the assignment
		job.DeleteItem(assignmentId);

		// execute the job
        try
        {
            job.Execute();
        }
        catch (LearningStoreItemNotFoundException)
        {
            // if the user doesn't have access, LearningStoreItemNotFoundException isn't thrown --
            // instead, LearningStoreSecurityException is thrown            
			throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, 
				AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
        }
        catch (LearningStoreSecurityException)
        {
            throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
        }
	}

    /// <summary>
    /// Returns the <c>LearnerAssignmentItemIdentifier</c> of the learner assignment belonging to
    /// the current user, for a given assignment.  Returns <c>null</c> if the current user isn't a
    /// learner of the assignment.
    /// </summary>
    /// 
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	retrieve information about.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
	/// may be any user.
	/// </remarks>
	///
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public Guid GetCurrentUserLearnerAssignment(
        AssignmentItemIdentifier assignmentId)
    {
        // Security checks: Returns null if not an learner on the assignment
        // (because of rules in the schema XML)

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");

        LearningStoreJob job = LearningStore.CreateJob();
		LearningStoreQuery query = LearningStore.CreateQuery(
			Schema.LearnerAssignmentListForLearners.ViewName);
        query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerAssignmentGuidId);
        query.AddCondition(Schema.LearnerAssignmentListForLearners.AssignmentId,
			LearningStoreConditionOperator.Equal, assignmentId);
		job.PerformQuery(query);
		DataRowCollection dataRows = job.Execute<DataTable>().Rows;
		if (dataRows.Count != 1)
			return Guid.Empty; // learner assignment not found (or multiple learner assignments?)
		Guid learnerAssignmentGuidId;
		LearningStoreHelper.CastNonNull(
            dataRows[0][Schema.LearnerAssignmentListForLearners.LearnerAssignmentGuidId],
			out learnerAssignmentGuidId);
		return learnerAssignmentGuidId;
    }

    /// <summary>
    /// Starts an attempt on a learner assignment
    /// </summary>
    /// 
    /// <param name="learnerAssignmentId">The learner assignment</param>
    /// 
    /// <returns>The id of the new attempt</returns>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have the right to create
	/// an attempt on the assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible causes: the user
    /// isn't an instructor or learner on the learner assignment, or an attempt has already
    /// been created for the learner assignment.
    /// </exception>
    /// 
    public AttemptItemIdentifier StartAttemptOnLearnerAssignment(
        Guid learnerAssignmentGuidId)
    {
        // Security checks: Fails if the user doesn't have the right to create an
        // attempt on the assignment (checked using a rule in the schema)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
            throw new ArgumentNullException("learnerAssignmentGuidId");

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();

        AttemptItemIdentifier attemptId = null;
        
        TransactionOptions options = new TransactionOptions();
        options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
			new LearningStoreTransactionScope(options))
        {
            // Demand the related right
            Dictionary<string,object> securityParameters = new Dictionary<string,object>();
            securityParameters.Add(Schema.StartAttemptOnLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
            job.DemandRight(Schema.StartAttemptOnLearnerAssignmentRight.RightName, securityParameters);
            
            // Verified that the user has the right to create an attempt -- so perform the other
            // actions with security checks turned off
            using(LearningStorePrivilegedScope priviligedScope = new LearningStorePrivilegedScope())
            {
                // Request information about the learner assignment
                LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentView.ViewName);
    	        query.AddColumn(Schema.LearnerAssignmentView.LearnerId);
		        query.AddColumn(Schema.LearnerAssignmentView.RootActivityId);
		        query.AddColumn(Schema.LearnerAssignmentView.AttemptId);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentId);
		        query.AddCondition(Schema.LearnerAssignmentView.LearnerAssignmentGuidId, LearningStoreConditionOperator.Equal,
		            learnerAssignmentGuidId);
		        job.PerformQuery(query);
    		    
		        // Execute the job
		        DataRowCollection dataRows;
		        try
		        {
                    dataRows = job.Execute<DataTable>().Rows;
                }
                catch(LearningStoreSecurityException)
                {
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                        AppResources.LearnerAssignmentNotFoundInDatabase, learnerAssignmentGuidId.ToString()));
                }
                if (dataRows.Count != 1)
                {
                    // this error message includes the learner assignment ID, but note that it's
                    // very unlikely the user will see this since security rules would have
                    // prevented code from getting this far if the learner assignment couldn't
                    // be found in the database
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, 
                        AppResources.LearnerAssignmentNotFoundInDatabase, learnerAssignmentGuidId.ToString()));
                }
                DataRow dataRow = dataRows[0];
            
                // Copy information out of <DataRow>
                
                UserItemIdentifier learnerId;
                LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentView.LearnerId],
                    out learnerId);
                    
                ActivityPackageItemIdentifier rootActivityId;
                LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentView.RootActivityId],
                    out rootActivityId);

                LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentView.AttemptId],
                    out attemptId);

                // Verify that there isn't already an attempt
                if(attemptId != null)
				    throw new InternalErrorException("SLK1013");

                LearningStoreItemIdentifier learnerAssignmentId;
                LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentView.LearnerAssignmentId],
                    out learnerAssignmentId);

                // Create the attempt
                StoredLearningSession session = StoredLearningSession.CreateAttempt(PackageStore,
                    learnerId, rootActivityId, Settings.LoggingOptions);
                attemptId = session.AttemptId;
                
                // Start the attempt
                try
                {
                    session.Start();
                }
                catch (SequencingException e)
                {
                    // The start command failed. As long at it was because flow was not enabled, then just 
                    // continue. Otherwise, rethrow the exception.
                    if (e.Code != "SB.2.2-1")
                        throw;
                }
                session.CommitChanges();

                // Attach the attempt to the assignment
                job = LearningStore.CreateJob();
		        Dictionary<string,object> properties = new Dictionary<string,object>();
		        properties[Schema.AttemptItem.LearnerAssignmentId] = learnerAssignmentId;
                job.UpdateItem(attemptId, properties);
		        job.Execute();
    		    
    		}
    		
    		scope.Complete();
        }
        
        return attemptId;
    }

    /// <summary>
    /// Returns the GuidId corresponding to the LearnerAssignment identifier
    /// </summary>
    /// <param name="learnerAssignmentId">The ID of the learner assignment corresponding
    ///     to the required GuidId</param>
    /// <returns>The GuidId of the LeanerAssignment item</returns>
    public Guid GetLearnerAssignmentGuidId(LearningStoreItemIdentifier learnerAssignmentId)
    {
        // Since this is a harmless query operation which returns a corresponding identifier
        // we disable security checks by executing in the privileged scope

        using (LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
        {
            LearningStoreJob job = LearningStore.CreateJob();

            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentView.ViewName);
            query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentGuidId);
            query.AddCondition(Schema.LearnerAssignmentView.LearnerAssignmentId,
                    LearningStoreConditionOperator.Equal, learnerAssignmentId);
            job.PerformQuery(query);

            DataRowCollection dataRows;
            try
            {
                dataRows = job.Execute<DataTable>().Rows;
            }
            catch (LearningStoreSecurityException)
            {
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentId.GetKey()));
            }

            if (dataRows.Count != 1)
            {
                // this error message includes the learner assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // learner assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentId.GetKey()));
            }
            DataRow dataRow = dataRows[0];
            Guid learnerAssignmentGuidId;
            LearningStoreHelper.CastNonNull(dataRow[0], out learnerAssignmentGuidId);


            return learnerAssignmentGuidId;
        }



    }
    
	/// <summary>
	/// Retrieves the properties of a SharePoint Learning Kit learner assignment (i.e. information
	/// about an assignment related to a single learner) given the learner assignment's identifier.
	/// </summary>
	///
    /// <param name="learnerAssignmentId">The ID of the learner assignment to return information
	/// 	about.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have access to the learner
	/// assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user
	/// isn't an instructor on the assignment (if <pr>slkRole</pr> is <r>SlkRole.Instructor</r>),
	/// or the user isn't the learner of this learner assignment (if <pr>slkRole</pr> is
	/// <r>SlkRole.Learner</r>).
    /// </exception>
	///
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public LearnerAssignmentProperties GetLearnerAssignmentProperties(
        Guid learnerAssignmentGuidId, SlkRole slkRole)
    {
        // Security checks: Fails if the user doesn't have access to the learner assignment (since
        // the query is limited to only the information the user has access to, and an exception
        // is thrown if zero rows are returned)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty)== true)
            throw new ArgumentNullException("learnerAssignmentId");
        if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner) && (slkRole != SlkRole.Observer))
            throw new ArgumentOutOfRangeException("slkRole");

        // set <instructorRole> based on <slkRole>
        bool instructorRole = false;
        bool observerRole = false;
        if (slkRole == SlkRole.Instructor)
            instructorRole = true;
        else
            if (slkRole == SlkRole.Learner)
                instructorRole = false;
            else
                if (slkRole == SlkRole.Observer)
                    observerRole = true;
                else
                    throw new ArgumentException(AppResources.InvalidSlkRole, "slkRole");

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();

        // request information about the specified learner assignment
        LearningStoreQuery query;
        if (instructorRole)
        {
            query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentSPSiteGuid);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentSPWebGuid);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.PackageLocation);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentNonELearningLocation);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentTitle);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentDescription);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentPointsPossible);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentStartDate);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentDueDate);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentAutoReturn);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentCreatedById);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AssignmentCreatedByName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.HasInstructors);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId,
                LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            job.PerformQuery(query);
        }
        else
        if (observerRole)
        {
            query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForObservers.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentSPSiteGuid);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentSPWebGuid);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.PackageLocation);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentNonELearningLocation);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentTitle);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentDescription);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentPointsPossible);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentStartDate);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentDueDate);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentAutoReturn);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentCreatedById);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AssignmentCreatedByName);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.LearnerName);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentListForObservers.HasInstructors);
            query.AddCondition(Schema.LearnerAssignmentListForObservers.LearnerAssignmentGuidId,
                LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            job.PerformQuery(query);
            string str = query.ToString();
        }
        else
        {
            query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForLearners.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentSPSiteGuid);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentSPWebGuid);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.PackageLocation);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentNonELearningLocation);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentTitle);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentDescription);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentPointsPossible);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentStartDate);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentDueDate);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentAutoReturn);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentCreatedById);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AssignmentCreatedByName);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerName);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.HasInstructors);
            query.AddCondition(Schema.LearnerAssignmentListForLearners.LearnerAssignmentGuidId,
                LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            job.PerformQuery(query);
        }
        // execute the job; set <resultEnumerator> to enumerate the results
        ReadOnlyCollection<object> results = job.Execute();
        IEnumerator<object> resultEnumerator = results.GetEnumerator();

		// retrieve from <resultEnumerator> information about the learner assignment
		if (!resultEnumerator.MoveNext())
			throw new InternalErrorException("SLK1012");
		DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
        if (dataRows.Count != 1)
		{
            // this error message includes the learner assignment ID, but that's okay since
            // the information we provide does not allow the user to distinguish between the
            // learner assignment not existing and the user not having access to it
            throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, 
				AppResources.LearnerAssignmentNotFoundInDatabase, learnerAssignmentGuidId.ToString()));
		}
        DataRow dataRow = dataRows[0];
        LearnerAssignmentProperties lap = new LearnerAssignmentProperties(learnerAssignmentGuidId);

		// copy information from <dataRow> into properties of <lap>...

        if(observerRole)
        {
            // This user is an Observer
            AssignmentItemIdentifier assignmentId;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentId], out assignmentId);
            lap.AssignmentId = assignmentId;

            LearnerAssignmentItemIdentifier learnerAssignmentId;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.LearnerAssignmentId], out learnerAssignmentId);
            lap.LearnerAssignmentId = learnerAssignmentId;

            Guid guid;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentSPSiteGuid], out guid);
            lap.SPSiteGuid = guid;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentSPWebGuid], out guid);
            lap.SPWebGuid = guid;

            ActivityPackageItemIdentifier activityId;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.RootActivityId], out activityId);
            lap.RootActivityId = activityId;
            bool isNonELearning = (activityId == null);

            string stringValue;
            if (isNonELearning)
            {
                LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentNonELearningLocation], out stringValue);
            }
            else
            {
                LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.PackageLocation], out stringValue);
            }
            lap.Location = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentTitle], out stringValue);
            lap.Title = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentDescription], out stringValue);
            lap.Description = stringValue;

            float? nullableFloat;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentPointsPossible], out nullableFloat);
            lap.PointsPossible = nullableFloat;

            DateTime utc;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentStartDate], out utc);
            lap.StartDate = utc.ToLocalTime();

            DateTime? utcOrNull;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentDueDate], out utcOrNull);
            if (utcOrNull == null)
                lap.DueDate = null;
            else
                lap.DueDate = utcOrNull.Value.ToLocalTime();

            bool boolValue;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentAutoReturn], out boolValue);
            lap.AutoReturn = boolValue;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentShowAnswersToLearners], out boolValue);
            lap.ShowAnswersToLearners = boolValue;

            UserItemIdentifier userId;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentCreatedById], out userId);
            lap.CreatedById = userId;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.AssignmentCreatedByName], out stringValue);
            lap.CreatedByName = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.LearnerId], out userId);
            lap.LearnerId = userId;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.LearnerName], out stringValue);
            lap.LearnerName = stringValue;

            LearnerAssignmentState status;
            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.LearnerAssignmentState], out status);
            lap.Status = status;

            AttemptItemIdentifier attemptId;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AttemptId], out attemptId);
            lap.AttemptId = attemptId;

            CompletionStatus? completionStatus;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AttemptCompletionStatus], out completionStatus);
            lap.CompletionStatus = completionStatus ?? CompletionStatus.Unknown;

            SuccessStatus? successStatus;
            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AttemptSuccessStatus], out successStatus);
            lap.SuccessStatus = successStatus ?? SuccessStatus.Unknown;

            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.AttemptGradedPoints], out nullableFloat);
            lap.GradedPoints = nullableFloat;

            LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentListForObservers.FinalPoints], out nullableFloat);
            lap.FinalPoints = nullableFloat;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.InstructorComments], out stringValue);
            lap.InstructorComments = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForObservers.HasInstructors], out boolValue);
            lap.HasInstructors = boolValue;

        }
        else
        {
            // This is user can be a Learner or Instructor. Set the assignment properties accordingly
            AssignmentItemIdentifier assignmentId;
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentId
                : Schema.LearnerAssignmentListForLearners.AssignmentId],
                out assignmentId);
            lap.AssignmentId = assignmentId;

            LearnerAssignmentItemIdentifier learnerAssignmentId;
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId
                : Schema.LearnerAssignmentListForLearners.LearnerAssignmentId],
                out learnerAssignmentId);
            lap.LearnerAssignmentId = learnerAssignmentId;


            Guid guid;
            
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentSPSiteGuid
                : Schema.LearnerAssignmentListForLearners.AssignmentSPSiteGuid],
                out guid);
            lap.SPSiteGuid = guid;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentSPWebGuid
                : Schema.LearnerAssignmentListForLearners.AssignmentSPWebGuid],
                out guid);
            lap.SPWebGuid = guid;

            ActivityPackageItemIdentifier activityId;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.RootActivityId
                : Schema.LearnerAssignmentListForLearners.RootActivityId],
                out activityId);
            lap.RootActivityId = activityId;
            bool isNonELearning = (activityId == null);

            string stringValue;
            if (isNonELearning)
            {
                LearningStoreHelper.CastNonNull(dataRow[instructorRole
                    ? Schema.LearnerAssignmentListForInstructors.AssignmentNonELearningLocation
                    : Schema.LearnerAssignmentListForLearners.AssignmentNonELearningLocation],
                    out stringValue);
            }
            else
            {
                LearningStoreHelper.CastNonNull(dataRow[instructorRole
                    ? Schema.LearnerAssignmentListForInstructors.PackageLocation
                    : Schema.LearnerAssignmentListForLearners.PackageLocation],
                    out stringValue);
            }
            lap.Location = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentTitle
                : Schema.LearnerAssignmentListForLearners.AssignmentTitle],
                out stringValue);
            lap.Title = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentDescription
                : Schema.LearnerAssignmentListForLearners.AssignmentDescription],
                out stringValue);
            lap.Description = stringValue;

            float? nullableFloat;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentPointsPossible
                : Schema.LearnerAssignmentListForLearners.AssignmentPointsPossible],
                out nullableFloat);
            lap.PointsPossible = nullableFloat;

            DateTime utc;
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentStartDate
                : Schema.LearnerAssignmentListForLearners.AssignmentStartDate],
                out utc);
            lap.StartDate = utc.ToLocalTime();

            DateTime? utcOrNull;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentDueDate
                : Schema.LearnerAssignmentListForLearners.AssignmentDueDate],
                out utcOrNull);
            if (utcOrNull == null)
                lap.DueDate = null;
            else
                lap.DueDate = utcOrNull.Value.ToLocalTime();

            bool boolValue;
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentAutoReturn
                : Schema.LearnerAssignmentListForLearners.AssignmentAutoReturn],
                out boolValue);
            lap.AutoReturn = boolValue;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentShowAnswersToLearners
                : Schema.LearnerAssignmentListForLearners.AssignmentShowAnswersToLearners],
                out boolValue);
            lap.ShowAnswersToLearners = boolValue;

            UserItemIdentifier userId;
            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentCreatedById
                : Schema.LearnerAssignmentListForLearners.AssignmentCreatedById],
                out userId);
            lap.CreatedById = userId;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AssignmentCreatedByName
                : Schema.LearnerAssignmentListForLearners.AssignmentCreatedByName],
                out stringValue);
            lap.CreatedByName = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.LearnerId
                : Schema.LearnerAssignmentListForLearners.LearnerId],
                out userId);
            lap.LearnerId = userId;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.LearnerName
                : Schema.LearnerAssignmentListForLearners.LearnerName],
                out stringValue);
            lap.LearnerName = stringValue;

            LearnerAssignmentState status;
            LearningStoreHelper.CastNonNull(
                dataRow[instructorRole
                    ? Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState
                    : Schema.LearnerAssignmentListForLearners.LearnerAssignmentState],
                out status);
            lap.Status = status;

            AttemptItemIdentifier attemptId;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AttemptId
                : Schema.LearnerAssignmentListForLearners.AttemptId],
                out attemptId);
            lap.AttemptId = attemptId;

            CompletionStatus? completionStatus;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AttemptCompletionStatus
                : Schema.LearnerAssignmentListForLearners.AttemptCompletionStatus],
                out completionStatus);
            lap.CompletionStatus = completionStatus ?? CompletionStatus.Unknown;

            SuccessStatus? successStatus;
            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AttemptSuccessStatus
                : Schema.LearnerAssignmentListForLearners.AttemptSuccessStatus],
                out successStatus);
            lap.SuccessStatus = successStatus ?? SuccessStatus.Unknown;

            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.AttemptGradedPoints
                : Schema.LearnerAssignmentListForLearners.AttemptGradedPoints],
                out nullableFloat);
            lap.GradedPoints = nullableFloat;

            LearningStoreHelper.Cast(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.FinalPoints
                : Schema.LearnerAssignmentListForLearners.FinalPoints],
                out nullableFloat);
            lap.FinalPoints = nullableFloat;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.InstructorComments
                : Schema.LearnerAssignmentListForLearners.InstructorComments],
                out stringValue);
            lap.InstructorComments = stringValue;

            LearningStoreHelper.CastNonNull(dataRow[instructorRole
                ? Schema.LearnerAssignmentListForInstructors.HasInstructors
                : Schema.LearnerAssignmentListForLearners.HasInstructors],
                out boolValue);
            lap.HasInstructors = boolValue;
        }

		// done
		return lap;
	}

	/// <summary>
	/// Retrieves grading-related information about an assignment from the SLK database.
	/// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	retrieve information about.</param>
	///
	/// <param name="basicAssignmentProperties">Where returned basic assignment properties are
	/// 	stored.  <paramref name="basicAssignmentProperties"/> is populated with all its
	/// 	properties except for <c>AssignmentProperties.Instructors</c> and
	/// 	<c>AssignmentProperties.Instructors</c></param>
	///
	/// <returns>
	/// A collection of <c>GradingProperties</c> objects, one for each learner assignment in the
	/// assignment, sorted by learner name.
	/// </returns>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
	/// assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user isn't
	/// an instructor on the assignment.
    /// </exception>
	///
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public ReadOnlyCollection<GradingProperties> GetGradingProperties(
		AssignmentItemIdentifier assignmentId, out AssignmentProperties basicAssignmentProperties)
	{
        // Security checks: Fails if the user isn't an instructor on the assignment (since
        // the query is limited to only the information the user has access to, and an exception
        // is thrown if zero rows are returned)

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();

		// add to <job> request(s) to get basic information about the assignment
		BeginGetAssignmentProperties(job, assignmentId, SlkRole.Instructor, false);

		// add to <job> a request to get information about each learner assignment
		LearningStoreQuery query = LearningStore.CreateQuery(
			Schema.LearnerAssignmentListForInstructors.ViewName);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptCompletionStatus);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptSuccessStatus);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptGradedPoints);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.FinalPoints);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.InstructorComments);
        query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId);
        query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId,
            LearningStoreConditionOperator.Equal, assignmentId);
        query.AddSort(Schema.LearnerAssignmentListForInstructors.LearnerName,
            LearningStoreSortDirection.Ascending);
		job.PerformQuery(query);

        // execute the job; set <resultEnumerator> to enumerate the results
        ReadOnlyCollection<object> results;
        try
        {
            results = job.Execute();
        }
        catch(LearningStoreSecurityException)
        {
            // this error message includes the assignment ID, but that's okay since
            // the information we provide does not allow the user to distinguish between the
            // assignment not existing and the user not having access to it
            throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
        }        
        IEnumerator<object> resultEnumerator = results.GetEnumerator();

		// retrieve from <resultEnumerator> information requested by BeginGetAssignmentProperties()
		basicAssignmentProperties = EndGetAssignmentProperties(resultEnumerator,
            assignmentId, SlkRole.Instructor, false);

		// retrieve from <resultEnumerator> information about each learner assignment
		if (!resultEnumerator.MoveNext())
			throw new InternalErrorException("SLK1004");
		DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
		List<GradingProperties> gpList = new List<GradingProperties>(dataRows.Count);
		foreach (DataRow dataRow in dataRows)
		{
			// set <gp> to a new GradingProperties object
			LearnerAssignmentItemIdentifier learnerAssignmentId;
			LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId],
				out learnerAssignmentId);
			GradingProperties gp = new GradingProperties(learnerAssignmentId);

			// set <gp.LearnerId>
			UserItemIdentifier userId;
			LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.LearnerId],
				out userId);
			gp.LearnerId = userId;

			// set <gp.LearnerName>
			string stringValue;
			LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.LearnerName],
				out stringValue);
			gp.LearnerName = stringValue;

			// set <gp.Status>
			LearnerAssignmentState status;
			LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState],
				out status);
			gp.Status = status;

			// set <gp.CompletionStatus>
			CompletionStatus? completionStatus;
			LearningStoreHelper.Cast(
                dataRow[Schema.LearnerAssignmentListForInstructors.AttemptCompletionStatus],
				out completionStatus);
			gp.CompletionStatus = completionStatus ?? CompletionStatus.Unknown;

			// set <gp.SuccessStatus>
			SuccessStatus? successStatus;
			LearningStoreHelper.Cast(
                dataRow[Schema.LearnerAssignmentListForInstructors.AttemptSuccessStatus],
				out successStatus);
			gp.SuccessStatus = successStatus ?? SuccessStatus.Unknown;

			// set <gp.GradedPoints>
			float? nullableFloat;
			LearningStoreHelper.Cast(dataRow[
                Schema.LearnerAssignmentListForInstructors.AttemptGradedPoints],
				out nullableFloat);
			gp.GradedPoints = nullableFloat;

			// set <gp.FinalPoints>
            LearningStoreHelper.Cast(dataRow[
				Schema.LearnerAssignmentListForInstructors.FinalPoints],
				out nullableFloat);
			gp.FinalPoints = nullableFloat;

			// set <gp.InstructorComments>
			LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.InstructorComments],
				out stringValue);
			gp.InstructorComments = stringValue;

            Guid learnerAssignmentGuidId;
            LearningStoreHelper.CastNonNull(
                dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId],
                out learnerAssignmentGuidId);
            gp.LearnerAssignmentGuidId = learnerAssignmentGuidId;

			// add <gp> to <gpList>
			gpList.Add(gp);
		}

        return new ReadOnlyCollection<GradingProperties>(gpList);
	}

	/// <summary>
	/// Stores grading-related information about an assignment into the SLK database.  The user
	/// must be an instructor on the assignment.
	/// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	store information about.</param>
	///
	/// <param name="gradingPropertiesList">Contains one <c>GradingProperties</c> object for each
	/// 	learner assignment to update.  Note that <c>GradingProperties.LearnerId</c>,
	/// 	<c>GradingProperties.LearnerName</c>, and <c>GradingProperties.GradedPoints</c>
	/// 	are ignored.  Also, see <c>GradingProperties.Status</c> for information about which
	/// 	<c>LearnerAssignmentState</c> changes are permitted.</param>
	///
	/// <returns>
	/// If the operation is fully successful, <c>null</c> is returned.  Note that data loss caused
	/// by one instructor overwriting the changes of another is considered normal behavior and is
	/// therefore treated as fully successful.  If the operation is partially successul, a
	/// warning string is returned.  In both the fully-successful and partially-successful cases
	/// the caller should assume that all data that could be saved to the database was saved, and
	/// if the data is to be redisplayed to the user the caller should reload the data using
	/// <c>GetGradingProperties</c>.  If the operation fails, an exception is thrown -- in this
	/// case, the caller should assume no data was stored in the database.
	/// </returns>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
	/// assignment.
	/// </remarks>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the user isn't
	/// an instructor on the assignment.
    /// </exception>
	///
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public string SetGradingProperties(AssignmentItemIdentifier assignmentId,
		IEnumerable<GradingProperties> gradingPropertiesList)
	{
        // Security checks: Fails if the user isn't an instructor on the assignment
        // (since it tries to access the file using AssignmentListForInstructors, and
        // that returns zero rows if the user isn't an instructor on the assignment)

        // Check parameters
        if (assignmentId == null)
            throw new ArgumentNullException("assignmentId");
        if (gradingPropertiesList == null)
            throw new ArgumentNullException("gradingPropertiesList");

        // if learner assignments couldn't be saved, the names of the learners of the affected
		// learner assignments are added to <warningLearners>
		List<string> warningLearners = null;

		// perform the operation within a transaction so that if the operation fails no data is
		// committed to the database
        TransactionOptions options = new TransactionOptions();
        options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
			new LearningStoreTransactionScope(options))
		{
			// create a LearningStore job
			LearningStoreJob job = LearningStore.CreateJob();

			// request basic information about the specified assignment
			LearningStoreQuery query = LearningStore.CreateQuery(
				Schema.AssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.AssignmentListForInstructors.RootActivityId);
            query.AddColumn(Schema.AssignmentListForInstructors.AssignmentAutoReturn);
            query.AddCondition(Schema.AssignmentListForInstructors.AssignmentId,
				LearningStoreConditionOperator.Equal, assignmentId);
			job.PerformQuery(query);

			// request current (stored-in-database) information about each learner assignment
			// in <gradingPropertiesList>; note that not all properties are retrieved
			query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId,
				LearningStoreConditionOperator.Equal, assignmentId);
			job.PerformQuery(query);

			// execute the job; set <resultEnumerator> to enumerate the results
			IEnumerator<object> resultEnumerator = job.Execute().GetEnumerator();

			// set <isNonELearning> to true if this assignment has non-e-learning content;
			// set <isAutoReturn> if this is an auto-return assignment
			if (!resultEnumerator.MoveNext())
				throw new InternalErrorException("SLK1005");
			DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
			if (dataRows.Count != 1)
			{
                // this error message includes the assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
					AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
			}
            ActivityPackageItemIdentifier rootActivityId;
            LearningStoreHelper.Cast(dataRows[0][0], out rootActivityId);
            bool isAutoReturn;
            LearningStoreHelper.CastNonNull(dataRows[0][1], out isAutoReturn);

			// store the information about each learner assignment <allOldProperties>
			if (!resultEnumerator.MoveNext())
				throw new InternalErrorException("SLK1006");
			DataTable dataTable = (DataTable) resultEnumerator.Current;
            Dictionary<LearnerAssignmentItemIdentifier, GradingProperties> allOldProperties =
                new Dictionary<LearnerAssignmentItemIdentifier, GradingProperties>(
					dataRows.Count);
			foreach (DataRow dataRow in dataTable.Rows)
			{
                LearnerAssignmentItemIdentifier learnerAssignmentId;
                LearningStoreHelper.CastNonNull(
                    dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId],
					out learnerAssignmentId);
				GradingProperties gp = new GradingProperties(learnerAssignmentId);

                UserItemIdentifier learnerId;
                LearningStoreHelper.CastNonNull(
                    dataRow[Schema.LearnerAssignmentListForInstructors.LearnerId],
					out learnerId);
				gp.LearnerId = learnerId;

				string learnerName;
                LearningStoreHelper.CastNonNull(
                    dataRow[Schema.LearnerAssignmentListForInstructors.LearnerName],
					out learnerName);
				gp.LearnerName = learnerName;

                LearnerAssignmentState status;
                LearningStoreHelper.CastNonNull(
                    dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState],
					out status);
				gp.Status = status;

                AttemptItemIdentifier attemptId;
                LearningStoreHelper.Cast(
                    dataRow[Schema.LearnerAssignmentListForInstructors.AttemptId],
                    out attemptId);
                gp.AttemptId = attemptId;

                Guid learnerAssignmentGuidId;
                LearningStoreHelper.CastNonNull(dataRow[Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId],
                    out learnerAssignmentGuidId);
                gp.LearnerAssignmentGuidId = learnerAssignmentGuidId;

                allOldProperties[learnerAssignmentId] = gp;
            }

            // The above code will throw an exception if the user doesn't have
            // instructor access to the assignment.  So since we've verified that,
            // we can turn off security for the rest of the work
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
			    // create another LearningStore job
			    job = LearningStore.CreateJob();

			    // update the status of each learner assignment
			    foreach (GradingProperties newProperties in gradingPropertiesList)
			    {
				    // skip this learner assignment if it doesn't exist in the database (e.g. another
				    // instructor deleted it while this instructor was viewing the Grading page)
                    GradingProperties oldProperties;
                    if (!allOldProperties.TryGetValue(newProperties.LearnerAssignmentId,
						    out oldProperties))
                        continue;

				    // set <properties> to properties that need to be changed on this learner
				    // assignment
				    Dictionary<string, object> properties = new Dictionary<string, object>();
					if (!newProperties.IgnoreFinalPoints)
						properties[Schema.LearnerAssignmentItem.FinalPoints] = newProperties.FinalPoints;
				    properties[Schema.LearnerAssignmentItem.InstructorComments] =
					    newProperties.InstructorComments;

				    // update the status of this learner assignment, if requested
				    if (newProperties.Status != null)
				    {
					    // status is changing
                        try
                        {
                            ChangeLearnerAssignmentState(oldProperties.LearnerAssignmentId, job,
                                oldProperties.Status.Value, newProperties.Status.Value,
                                rootActivityId, oldProperties.LearnerId, oldProperties.AttemptId,
                                isAutoReturn, properties);
                        }
                        catch (InvalidOperationException)
                        {
                            // disallowed state transition -- log it in <warningLearners>
                            if (warningLearners == null)
                                warningLearners = new List<string>();
                            warningLearners.Add(oldProperties.LearnerName);

                            // ignore this learner assignment
                            continue;
                        }
				    }
				    else
					    job.UpdateItem(newProperties.LearnerAssignmentId, properties);
			    }

                // execute the LearningStore job
                job.Execute();
            }
            
			// finish the transaction
			scope.Complete();
		}

		// the operation succeeded, with or without warnings
		if (warningLearners == null)
			return null;
		else
		{
            // tell the caller that some learner assignments could not be saved
            return String.Format(CultureInfo.CurrentCulture, AppResources.SomeLearnerAssignmentsNotSaved,
				String.Join(AppResources.CommaSpace, warningLearners.ToArray()));
		}
	}

    /// <summary>
    /// Sets the final points for a learner assignment.
    /// </summary>
    ///
    /// <param name="learnerAssignmentId">The <c>LearnerAssignmentItemIdentifier</c> of the
    ///     learner assignment to change.</param>
    ///
    /// <param name="finalPoints">New final points.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
	/// assignment.
	/// </remarks>
    ///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible causes: the user isn't
    /// an instructor on the assignment, the learner assignment isn't in the correct state.
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public void SetFinalPoints(Guid learnerAssignmentGuidId,
        float? finalPoints)
    {
        // Security checks: Fails if the user isn't an instructor on the assignment
        // (since it tries to access the learner assignment using LearnerAssignmentListForInstructors,
        // and that returns zero rows if the user isn't an instructor on the learner assignment)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
            throw new ArgumentNullException("learnerAssignmentGuidId");

        // Create a transaction
        TransactionOptions transactionOptions = new TransactionOptions();
        transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(transactionOptions))
        {
            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();

            // request current (stored-in-database) information about the learner assignment
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId,
                LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            job.PerformQuery(query);

            // execute the LearningStore job; set <status> to the learner assignment state
            DataRowCollection dataRows;
            try
            {
                dataRows = job.Execute<DataTable>().Rows;
            }
            catch (LearningStoreSecurityException)
            {
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentGuidId.ToString()));
            }
            if (dataRows.Count != 1)
            {
                // this error message includes the learner assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // learner assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentGuidId.ToString()));
            }
            DataRow dataRow = dataRows[0];
            LearnerAssignmentState status;
            LearningStoreHelper.CastNonNull<LearnerAssignmentState>(dataRow[0], out status);

            LearningStoreItemIdentifier learnerAssignmentId;
            LearningStoreHelper.CastNonNull(dataRow[1], out learnerAssignmentId);

            // Only valid if we are in the completed or final state
            if ((status != LearnerAssignmentState.Completed) && (status != LearnerAssignmentState.Final))
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotInCorrectStateForFinish));

            // We've verified that the user has the correct right, so do everything else
            // with security checks turned off
            using (LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                // create another LearningStore job
                job = LearningStore.CreateJob();

                // Update the properties of the LearnerAssignment
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties[Schema.LearnerAssignmentItem.FinalPoints] = finalPoints;
                job.UpdateItem(learnerAssignmentId, properties);

                // execute the LearningStore job
                job.Execute();
            }

            // finish the transaction
            scope.Complete();
        }
    }

    /// <summary>
    /// Increments the final points for a learner assignment.
    /// </summary>
    ///
    /// <param name="learnerAssignmentId">The <c>LearnerAssignmentItemIdentifier</c> of the
    ///     learner assignment to change.</param>
    ///
    /// <param name="points">Points to add.</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
	/// assignment.
	/// </remarks>
    ///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible causes: the user isn't
    /// an instructor on the assignment, the learner assignment isn't in the correct state.
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public void IncrementFinalPoints(Guid learnerAssignmentGuidId,
        float points)
    {
        // Security checks: Fails if the user isn't an instructor on the assignment
        // (since it tries to access the learner assignment using LearnerAssignmentListForInstructors,
        // and that returns zero rows if the user isn't an instructor on the learner assignment)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
            throw new ArgumentNullException("learnerAssignmentGuidId");

        // Create a transaction
        TransactionOptions transactionOptions = new TransactionOptions();
        transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(transactionOptions))
        {
            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();

            // request current (stored-in-database) information about the learner assignment
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId,
                LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            job.PerformQuery(query);

            // execute the LearningStore job; set <status> to the learner assignment state
            DataRowCollection dataRows;
            try
            {
                dataRows = job.Execute<DataTable>().Rows;
            }
            catch (LearningStoreSecurityException)
            {
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentGuidId.ToString()));
            }
            if (dataRows.Count != 1)
            {
                // this error message includes the learner assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // learner assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotFoundInDatabase,
                        learnerAssignmentGuidId.ToString()));
            }
            DataRow dataRow = dataRows[0];
            LearnerAssignmentState status;
            LearningStoreHelper.CastNonNull<LearnerAssignmentState>(dataRow[0], out status);
            float? finalPoints;
            LearningStoreHelper.Cast<float>(dataRow[1], out finalPoints);
            LearnerAssignmentItemIdentifier learnerAssignmentId;
            LearningStoreHelper.CastNonNull(dataRow[2], out learnerAssignmentId);

            // Only valid if we are in the completed or final state
            if ((status != LearnerAssignmentState.Completed) && (status != LearnerAssignmentState.Final))
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                    AppResources.LearnerAssignmentNotInCorrectStateForFinish));

            // Increment final points
            if(finalPoints != null)
                points += finalPoints.Value;
                
            // We've verified that the user has the correct right, so do everything else
            // with security checks turned off
            using (LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                // create another LearningStore job
                job = LearningStore.CreateJob();

                // Update the properties of the LearnerAssignment
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties[Schema.LearnerAssignmentItem.FinalPoints] = points;
                job.UpdateItem(learnerAssignmentId, properties);

                // execute the LearningStore job
                job.Execute();
            }

            // finish the transaction
            scope.Complete();
        }
    }

    /// <summary>
    /// Changes the <c>LearnerAssignmentState</c> of a learner assignment.  Must be called within
	/// the scope of a database transaction.
    /// </summary>
	///
    /// <param name="learnerAssignmentId">The ID of the learner assignment to change the
	/// 	<c>LearnerAssignmentState</c> of.</param>
	///
    /// <param name="job">A <c>LearningStoreJob</c> which will be used to perform any database
	/// 	update operations.  The caller is responsible for subsequently calling
	/// 	<c>LearningStoreJob.Execute</c>, and the caller cannot make any assumptions about what
	/// 	operations were added to the job by this method.</param>
	///
    /// <param name="oldStatus">The current status (stored in the database) of the learner
	/// 	assignment.  This value must have been obtained within the same database transaction
	/// 	scope which encloses this method call.</param>
	///
    /// <param name="newStatus">The <c>LearnerAssignmentState</c> to transition this learner
	/// 	assignment to.  See the Remarks section of the other override of this method for more
	/// 	information.</param>
	///
    /// <param name="rootActivityId">The <c>ActivityPackageItemIdentifier</c> of the organization
	/// 	assigned, if the assigned file was an e-learning package; <c>null</c> for a
	/// 	non-e-learning document.</param>
	///
	/// <param name="learnerId">The <c>UserItemIdentifier</c> of the learner.</param>
	///
	/// <param name="attemptId">The <c>AttemptItemIdentifier</c> of the attempt associated with
	/// 	this learner assignment; <c>null</c> if none.</param>
	///
	/// <param name="isAutoReturn"><c>true</c> if a state transition of <c>Active</c> to
	/// 	<c>Completed</c> should automatically trigger a transition from <c>Completed</c> to
	/// 	<c>Final</c>.</param>
    /// 
    /// <param name="properties">Property name/value pairs to set on the learner assignment.
	/// 	May be <c>null</c> if the caller has no property changes to request.</param>
    /// 
    /// <remarks>
	/// See the Remarks section of the other override of this method.
    /// </remarks>
    ///
    /// <exception cref="InvalidOperationException">
    /// The requested state transition is not supported.
    /// </exception>
    ///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause:
    /// the user doesn't have the right to switch to the requested state.
    /// </exception>
    ///
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void ChangeLearnerAssignmentState(LearnerAssignmentItemIdentifier learnerAssignmentId,
		LearningStoreJob job, LearnerAssignmentState oldStatus, LearnerAssignmentState newStatus,
		ActivityPackageItemIdentifier rootActivityId, UserItemIdentifier learnerId,
        AttemptItemIdentifier attemptId, bool isAutoReturn, IDictionary<String, Object> properties)
	{
		// set <isNonELearning> to true if this assignment has non-e-learning content
		bool isNonELearning = (rootActivityId == null);

		// <makeFinal> will be set to true below if IsFinal = true needs to be set on the learner
		// assignment
		bool makeFinal = false;

		// initialize <properties> if the caller didn't pass it in
		if (properties == null)
			properties = new Dictionary<string, object>();

        // perform the state transition
        if ((oldStatus == LearnerAssignmentState.Completed) &&
            (newStatus == LearnerAssignmentState.Completed))
        {
            if (!isAutoReturn)
                // Nothing to do
                return;

            // Since it is auto-returned, set it to final
            makeFinal = true;
        }
        else
        if (oldStatus == newStatus)
        {
            // Nothing to do
            return;
        }
		else
        if (oldStatus == LearnerAssignmentState.NotStarted)
        {
            // NotStarted --> Active or Completed or Final
            if (!isNonELearning)
            {
                // create an attempt for this learner assignment
                StoredLearningSession learningSession =
                    StoredLearningSession.CreateAttempt(PackageStore, learnerId, rootActivityId,
                        Settings.LoggingOptions);

                // start the assignment, forcing selection of a first activity
                learningSession.Start(true);

                // if NotStarted --> Completed or Final, transition to the Completed state
                if ((newStatus == LearnerAssignmentState.Completed) ||
                    (newStatus == LearnerAssignmentState.Final))
                {
					// transition to Completed
                    learningSession.Exit();

					// initialize LearnerAssignmentItem.FinalPoints to
                    // LearningSession.TotalPoints, (overwrites the value in
                    // <properties> if any)
                    properties[Schema.LearnerAssignmentItem.FinalPoints] =
                            learningSession.TotalPoints;
                }

                // save changes to <learningSession>
                learningSession.CommitChanges();

                // attach the attempt to the learner assignment
                Dictionary<string, object> attemptProperties = new Dictionary<string, object>();
                attemptProperties[Schema.AttemptItem.LearnerAssignmentId] = learnerAssignmentId;
                job.UpdateItem(learningSession.AttemptId, attemptProperties);
            }

            // if this is an auto-return assignment, and we're transitioning to Completed state,
			// and this is an auto-return assignment, transition to Final state next; if we're
			// transitioning to Final state directly, do that next
            if (newStatus == LearnerAssignmentState.Completed)
			{
				if (isAutoReturn)
					makeFinal = true;
			}
			else
            if (newStatus == LearnerAssignmentState.Final)
				makeFinal = true;
        }
		else
		if ((oldStatus == LearnerAssignmentState.Active) &&
			((newStatus == LearnerAssignmentState.Completed) ||
			 (newStatus == LearnerAssignmentState.Final)))
		{
			// Active --> Completed or Final
			if (!isNonELearning)
			{
				// transition to Completed state...

				// set <learningSession> to refer to the attempt associated with this
				// learner assignment
				if (attemptId == null)
					throw new InternalErrorException("SLK1007");
				StoredLearningSession learningSession =
					new StoredLearningSession(SessionView.Execute, attemptId, PackageStore);

				// transition the attempt to "Completed" state; note that this will initialize
                // the "content score", i.e. the score computed from the content
                if (learningSession.HasCurrentActivity)
                {
                    // make sure that if the content wants to suspend itself, it does
                    learningSession.ProcessNavigationRequests();    
                }
				learningSession.Exit();
				learningSession.CommitChanges();

				// initialize LearnerAssignmentItem.FinalPoints to LearningSession.TotalPoints, 
				// (overwrites the value in <properties> if any)
                properties[Schema.LearnerAssignmentItem.FinalPoints] =
                        learningSession.TotalPoints;
			}

			// if this is an auto-return assignment, or if <newStatus> specifies Final, transition
			// to Final state next
			if (isAutoReturn || (newStatus == LearnerAssignmentState.Final))
				makeFinal = true;
		}
		else
		if ((oldStatus == LearnerAssignmentState.Completed) &&
			(newStatus == LearnerAssignmentState.Final))
		{
			// Completed --> Final

			// transition the attempt to "Final" state; note that "Final" is an SLK
			// concept -- MLC is not involved
			makeFinal = true;
		}
		else
		if (((oldStatus == LearnerAssignmentState.Completed) ||
             (oldStatus == LearnerAssignmentState.Final))&&
			(newStatus == LearnerAssignmentState.Active))
		{
			// Final --> Active (i.e. reactivate)
			if (!isNonELearning)
			{
                // set <learningSession> to refer to the attempt associated with this
                // learner assignment
                if (attemptId == null)
                    throw new InternalErrorException("SLK1010");
                StoredLearningSession learningSession =
                    new StoredLearningSession(SessionView.RandomAccess, attemptId, PackageStore);

				// reactivate the attempt
                learningSession.Reactivate(ReactivateSettings.ResetEvaluationPoints);
				learningSession.CommitChanges();

                // restart the attempt
                learningSession = new StoredLearningSession(SessionView.Execute, attemptId,
                    PackageStore);
                learningSession.Start(true);
                learningSession.CommitChanges();
				// NOTE: if (learningSession.AttemptStatus != AttemptStatus.Active) then the
				// restart process failed -- but there's not much we can do about it, and throwing
				// an exception may make matters worse

				// clear FinalPoints
                properties[Schema.LearnerAssignmentItem.FinalPoints] = null;
            }

			// if the SLK status of the assignment is "Final" change it so that it reflects
			// the status of the attempt
			properties[Schema.LearnerAssignmentItem.IsFinal] = false;
		}
		else
		{
			// state transition not supported
            throw new InvalidOperationException(AppResources.LearnerAssignmentTransitionNotSupported);
		}

		// set IsFinal = true on the learner assignment if specified above
		if (makeFinal)
			properties[Schema.LearnerAssignmentItem.IsFinal] = true;

		// if this is non-e-learning content, update LearnerAssignmentItem.NonELearningStatus
        if (isNonELearning)
        {
            AttemptStatus? newAttemptStatus;
            switch (newStatus)
            {
            case LearnerAssignmentState.NotStarted:
            default:
                newAttemptStatus = null;
                break;
            case LearnerAssignmentState.Active:
                newAttemptStatus = AttemptStatus.Active;
                break;
            case LearnerAssignmentState.Completed:
                newAttemptStatus = AttemptStatus.Completed;
                break;
            case LearnerAssignmentState.Final:
                newAttemptStatus = AttemptStatus.Completed;
                break;
            }
            properties[Schema.LearnerAssignmentItem.NonELearningStatus] = newAttemptStatus;
        }

		// specify that the learner assignment be updated
		job.UpdateItem(learnerAssignmentId, properties);
    }

    /// <summary>
    /// Changes the <c>LearnerAssignmentState</c> of a learner assignment.
    /// </summary>
	///
    /// <param name="learnerAssignmentId">The ID of the learner assignment to change the
	/// 	<c>LearnerAssignmentState</c> of.</param>
	///
    /// <param name="newStatus">The <c>LearnerAssignmentState</c> to transition this learner
	/// 	assignment to.  See Remarks for more information.</param>
	///
	/// <returns>
	/// <c>true</c> if the state transition succeeded, <c>false</c> if the state transition failed
	/// because that state transition is not supported.  If any other error occurs while performing
	/// this operation, an exception is thrown.
	/// </returns>
    /// 
    /// <remarks>
	/// <para>
	/// Only the following state transitions are supported by this method:
	/// </para>
	/// <list type="bullet">
	/// 	<item><description><c>Active</c> to <c>Completed</c>.  This "submits" (learner gives
	/// 		assignment back to instructor) or "collects" (instructor takes assignment back
	/// 		from learner) or "marks as complete" (learner record a learner-created assignment
	/// 		as complete) a learner assignment.  This transition may be performed by the
	/// 		instructor of the assignment or the learner who owns this learner assignment.
	/// 		Note that if <c>AssignmentProperties.AutoReturn</c> is <c>true</c> for this
	/// 		assignment, this state transition will automatically cause a second transition,
	/// 		from <c>Completed</c> to <c>Final</c>
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Completed</c> to <c>Final</c>.  This "returns" the assignment
	///			from the instructor to the learner -- in this case the user must be an instructor
	/// 		on the assignment.  This state transition may also be performed in the case where
	/// 		the instructor caused <c>AssignmentProperties.AutoReturn</c> to be set to
	/// 		<c>true</c> <u>after</u> this assignment transitioned from <c>Active</c> to
	/// 		<c>Completed</c> state -- in this case ("auto-return") the user may be either an
	/// 		instructor of the assignment or the learner who owns this learner assignment.
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Completed</c> or <c>Final</c> to <c>Active</c>.  This
    ///         "reactivates" the assignment, so that the learner may once again work on it.
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Active</c> to <c>Final</c>, equivalent to <c>Active</c> to
	/// 		<c>Completed</c> followed by <c>Completed</c> to <c>Final</c>.</description>
	/// 	</item>
	/// 	<item><description><c>NotStarted</c> to <c>Active</c>, <c>Completed</c>, or
	/// 		<c>Final</c>, equivalent to beginning the learner assignment and then transitioning
	/// 		states as described above.</description>
	/// 	</item>
	/// </list>
    /// </remarks>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have the right to switch to
	/// the requested state.
	/// </remarks>
    ///
    /// <exception cref="InvalidOperationException">
    /// The requested state transition is not supported.
    /// </exception>
	///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause:
    /// the user doesn't have the right to switch to the requested state.
    /// </exception>
    ///
	public void ChangeLearnerAssignmentState(Guid learnerAssignmentGuidId,
		LearnerAssignmentState newStatus)
	{
        // Security checks: Fails if the user doesn't have the right to switch to
        // the requested state (checked by using a rule in the schema)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
            throw new ArgumentNullException("learnerAssignmentGuidId");
        if ((newStatus != LearnerAssignmentState.Active) && (newStatus != LearnerAssignmentState.Completed) &&
            (newStatus != LearnerAssignmentState.Final) && (newStatus != LearnerAssignmentState.NotStarted))
            throw new ArgumentOutOfRangeException("newStatus");

        // the other override of ChangeLearnerAssignmentState requires a transaction
        TransactionOptions transactionOptions = new TransactionOptions();
        transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(transactionOptions))
		{
			// create a LearningStore job
			LearningStoreJob job = LearningStore.CreateJob();

            // Demand the correct security
            Dictionary<string,object> securityParameters = new Dictionary<string,object>();
            switch(newStatus)
            {
                case LearnerAssignmentState.Active:
                    securityParameters.Add(Schema.ActivateLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
                    job.DemandRight(Schema.ActivateLearnerAssignmentRight.RightName, securityParameters);
                    break;
                case LearnerAssignmentState.Completed:
                    securityParameters.Add(Schema.CompleteLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
                    job.DemandRight(Schema.CompleteLearnerAssignmentRight.RightName, securityParameters);
                    break;
                case LearnerAssignmentState.Final:
                    securityParameters.Add(Schema.FinalizeLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
                    job.DemandRight(Schema.FinalizeLearnerAssignmentRight.RightName, securityParameters);
                    break;
                default:
					throw new InvalidOperationException(AppResources.LearnerAssignmentTransitionNotSupported);
            }

            // We've verified that the user has the correct right, so do everything else
            // with security checks turned off
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
			    // request the current (stored in database) status of the learner assignment
			    LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentView.ViewName);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentState);
                query.AddColumn(Schema.LearnerAssignmentView.RootActivityId);
                query.AddColumn(Schema.LearnerAssignmentView.AttemptId);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerId);
                query.AddColumn(Schema.LearnerAssignmentView.AssignmentAutoReturn);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentId);
                query.AddCondition(Schema.LearnerAssignmentView.LearnerAssignmentGuidId,
				    LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
			    job.PerformQuery(query);

                // execute the LearningStore job; set <oldStatus> to the current
			    // LearnerAssignmentState; set <packageId> to the ActivityPackageItemIdentifier of the
			    // organization assigned, if the assigned file was an e-learning package, or null if
			    // a non-e-learning document was assigned; set <learnerId> to the UserItemIdentifier
			    // of the learner; set <isAutoReturn> to true if a state transition of Active->Completed
			    // should trigger a transition from Competed->Final
			    DataRowCollection dataRows;
			    try
			    {
			        dataRows = job.Execute<DataTable>().Rows;
			    }
			    catch(LearningStoreSecurityException)
			    {
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                        AppResources.LearnerAssignmentNotFoundInDatabase,
                            learnerAssignmentGuidId.ToString()));
                }
			    
			    if (dataRows.Count != 1)
			    {
                    // this error message includes the learner assignment ID, but that's okay since
                    // the information we provide does not allow the user to distinguish between the
                    // learner assignment not existing and the user not having access to it
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
					    AppResources.LearnerAssignmentNotFoundInDatabase,
						    learnerAssignmentGuidId.ToString()));
			    }
			    DataRow dataRow = dataRows[0];
			    LearnerAssignmentState oldStatus;
			    LearningStoreHelper.CastNonNull<LearnerAssignmentState>(dataRow[0], out oldStatus);
                ActivityPackageItemIdentifier rootActivityId;
                LearningStoreHelper.Cast(dataRow[1], out rootActivityId);
			    AttemptItemIdentifier attemptId;
			    LearningStoreHelper.Cast(dataRow[2], out attemptId);
                UserItemIdentifier learnerId;
                LearningStoreHelper.CastNonNull(dataRow[3], out learnerId);
                bool isAutoReturn;
			    LearningStoreHelper.CastNonNull(dataRow[4], out isAutoReturn);
                LearnerAssignmentItemIdentifier learnerAssignmentId;
                LearningStoreHelper.CastNonNull(dataRow[5], out learnerAssignmentId);


			    // create another LearningStore job
			    job = LearningStore.CreateJob();

			    // execute the other override of this method
                ChangeLearnerAssignmentState(learnerAssignmentId, job, oldStatus, newStatus,
                    rootActivityId, learnerId, attemptId, isAutoReturn, null);

			    // execute the LearningStore job
			    job.Execute();
            }
            
			// finish the transaction
			scope.Complete();
		}
	}

    /// <summary>
    /// Finishes the learner assignment.
    /// </summary>
    ///
    /// <param name="learnerAssignmentId">The ID of the learner assignment to finish.</param>
    ///
    /// <remarks>
    /// This method should be called after a session is completed in order to update the information
    /// in the LearnerAssignment, auto-return the LearnerAssignment if necessary, and other
    /// related operations.
    /// </remarks>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have the right to finish
	/// the learner assignment.
	/// </remarks>
    ///
    /// <exception cref="SafeToDisplayException">
    /// An error occurred that can be displayed to a browser user.  Possible cause: the learner
    /// assignment isn't in the correct state, it is an elearning assignment, or the user isn't
    /// an instructor or learner on the assignment.
    /// </exception>
    ///
    public void FinishLearnerAssignment(Guid learnerAssignmentGuidId)
    {
        // Security checks: Fails if the user doesn't have the right to finish
        // the learner assignment (checked by using a rule in the schema)

        // Check parameters
        if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
            throw new ArgumentNullException("learnerAssignmentGuidId");

        // Create a transaction
        TransactionOptions transactionOptions = new TransactionOptions();
        transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
        using (LearningStoreTransactionScope scope =
            new LearningStoreTransactionScope(transactionOptions))
        {
            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();

            // Demand the correct security
            Dictionary<string, object> securityParameters = new Dictionary<string, object>();
            securityParameters.Add(Schema.FinishLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
            job.DemandRight(Schema.FinishLearnerAssignmentRight.RightName, securityParameters);

            // We've verified that the user has the correct right, so do everything else
            // with security checks turned off
            using (LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                // request the current (stored in database) status of the learner assignment
                LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentView.ViewName);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentState);
                query.AddColumn(Schema.LearnerAssignmentView.RootActivityId);
                query.AddColumn(Schema.LearnerAssignmentView.AttemptGradedPoints);
                query.AddColumn(Schema.LearnerAssignmentView.AssignmentAutoReturn);
                query.AddColumn(Schema.LearnerAssignmentView.LearnerAssignmentId);
                query.AddCondition(Schema.LearnerAssignmentView.LearnerAssignmentGuidId,
                    LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
                job.PerformQuery(query);

                // execute the LearningStore job; set <status> to the current
                // LearnerAssignmentState; set <rootActivityId> to the ActivityPackageItemIdentifier of the
                // organization assigned, if the assigned file was an e-learning package, or null if
                // a non-e-learning document was assigned; set <gradedPoints> to the graded points;
                // set <isAutoReturn> to true if a state transition of Competed->Final should be performed.
                DataRowCollection dataRows;
                try
                {
                    dataRows = job.Execute<DataTable>().Rows;
                }
                catch(LearningStoreSecurityException)
                {
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                        AppResources.LearnerAssignmentNotFoundInDatabase,
                            learnerAssignmentGuidId.ToString()));
                }
                if (dataRows.Count != 1)
                {
                    // this error message includes the learner assignment ID, but that's okay since
                    // the information we provide does not allow the user to distinguish between the
                    // learner assignment not existing and the user not having access to it
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                        AppResources.LearnerAssignmentNotFoundInDatabase,
                            learnerAssignmentGuidId.ToString()));
                }
                DataRow dataRow = dataRows[0];
                LearnerAssignmentState status;
                LearningStoreHelper.CastNonNull<LearnerAssignmentState>(dataRow[0], out status);
                ActivityPackageItemIdentifier rootActivityId;
                LearningStoreHelper.Cast(dataRow[1], out rootActivityId);
                float? gradedPoints;
                LearningStoreHelper.Cast(dataRow[2], out gradedPoints);                
                bool isAutoReturn;
                LearningStoreHelper.CastNonNull(dataRow[3], out isAutoReturn);
                LearningStoreItemIdentifier learnerAssignmentId;
                LearningStoreHelper.CastNonNull(dataRow[4], out learnerAssignmentId);

                // Only valid if there's an attempt and we are in the completed state
                if((rootActivityId == null) || (status != LearnerAssignmentState.Completed))                
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                        AppResources.LearnerAssignmentNotInCorrectStateForFinish));

                // create another LearningStore job
                job = LearningStore.CreateJob();

                // Update the properties of the LearnerAssignment
                Dictionary<string,object> properties = new Dictionary<string,object>();
                if(isAutoReturn)
                    properties[Schema.LearnerAssignmentItem.IsFinal] = true;
                properties[Schema.LearnerAssignmentItem.FinalPoints] = gradedPoints;
                job.UpdateItem(learnerAssignmentId, properties);

                // execute the LearningStore job
                job.Execute();
            }

            // finish the transaction
            scope.Complete();
        }
    }

    /// <summary>
    /// Retrieves some information about an e-learning package.
    /// </summary>
    ///
    /// <param name="packageReader">A <c>PackageReader</c> open onto the e-learning package to
    ///     retrieve information about.</param>
    ///
    /// <param name="spFile">The <c>SPFile</c> of the package to retrieve information about.
    /// 	</param>
    ///
    /// <param name="manifestReader">Set to a <c>ManifestReader</c> open onto the package file.
    /// 	</param>
    ///
    /// <param name="title">The title of the package.  This will always be non-null and non-blank.
    /// 	</param>
    ///
    /// <param name="description">The description of the package.  This will always be non-null,
    /// 	but it may be blank.</param>
    ///
    private static void GetPackageInformation(PackageReader packageReader, SPFile spFile,
        out ManifestReader manifestReader, out string title,
        out string description)
    {
        // Security checks: None
        
        // set <packageReader>, <manifestReader>, and <metadataReader> to refer to the package
        manifestReader = new ManifestReader(packageReader, new ManifestReaderSettings(true, true));
        MetadataNodeReader metadataReader = manifestReader.Metadata;

        // set <titleFromMetadata> to the package title specified in metadata, or null if none
        string titleFromMetadata = metadataReader.GetTitle(CultureInfo.CurrentCulture);
        if ((titleFromMetadata != null) && (titleFromMetadata.Length == 0))
            titleFromMetadata = null;

        // set <descriptionFromMetadata> to the package description specified in metadata, or null
        // if none
        string descriptionFromMetadata;
        ReadOnlyCollection<string> descriptions =
            metadataReader.GetDescriptions(CultureInfo.CurrentCulture);
        if (descriptions.Count > 0)
            descriptionFromMetadata = descriptions[0];
        else
            descriptionFromMetadata = null;

        // set <title> to the title to display for the package, using these rules:
        //   1. if there's a Title column value, use it;
        //   2. otherwise, if there's a title specified in the package metadata, use it;
        //   3. otherwise, use the file name without the extension

        if (!String.IsNullOrEmpty(spFile.Title))
            title = spFile.Title;
        else
        if (titleFromMetadata != null)
            title = titleFromMetadata;
        else
            title = Path.GetFileNameWithoutExtension(spFile.Name);

        // set <description> to the package description
        description = (descriptionFromMetadata ?? String.Empty);
    }

    /// <summary>
    /// Retrieves some information about an e-learning package.
    /// </summary>
    ///
    /// <param name="location">Location string for the package</param>
	///
	/// <remarks>
	/// <b>Security:</b>&#160; Fails if the
	/// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have access to the
	/// package.
	/// </remarks>
	///
    /// <exception cref="FileNotFoundException">
    /// <pr>location</pr> refers to a file that does not exist.
    /// </exception>
    /// 
    /// <exception cref="UnauthorizedAccessException">
    /// <pr>location</pr> refers to a file that the user does not have access to.
    /// </exception>
    ///
    public PackageInformation GetPackageInformation(string location)
    {
        // Security checks: Fails if user doesn't have access to the package (implemented
        // by GetPackageReaderFromSharePointLocation)

        // Check parameters
        if (location == null)
            throw new ArgumentNullException("location");

        // set <manifestReader> to refer to the package's manifest
        string title;
        string description;
        ManifestReader manifestReader;
        SPFile spFile;
        using(PackageReader packageReader = SlkStore.GetPackageReaderFromSharePointLocation(
            location, SharePointCacheSettings,
            out spFile))
        {
            SlkStore.GetPackageInformation(packageReader, spFile, out manifestReader,
                out title, out description);
        }
        
        // Return the structure
        PackageInformation information = new PackageInformation();
        information.SPFile = spFile;
        information.ManifestReader = manifestReader;
        information.Title = title;
        information.Description = description;

        return information;
    }

    /// <summary>
	/// Returns the SharePoint Learning Kit user web list for the current user.
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// The user web list is the list of Web sites that the user added to their E-Learning Actions
	/// page.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
	/// may be any user.
	/// </para>
	/// </remarks>
	///
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public ReadOnlyCollection<SlkUserWebListItem> GetUserWebList()
	{
        // Security checks: None
        
        LearningStoreJob job = LearningStore.CreateJob();
        LearningStoreQuery query = LearningStore.CreateQuery(
            Schema.UserWebList.ViewName);
        query.AddColumn(Schema.UserWebList.SPSiteGuid);
        query.AddColumn(Schema.UserWebList.SPWebGuid);
        query.AddColumn(Schema.UserWebList.LastAccessTime);
        query.AddSort(Schema.UserWebList.LastAccessTime,
            LearningStoreSortDirection.Descending);
        job.PerformQuery(query);
		DataRowCollection dataRows = job.Execute<DataTable>().Rows;
		List<SlkUserWebListItem> userWebList = new List<SlkUserWebListItem>(dataRows.Count);
		foreach (DataRow dataRow in dataRows)
		{
			Guid spSiteGuid;
			Guid spWebGuid;
			DateTime lastAccessTime;
			LearningStoreHelper.CastNonNull(dataRow[0], out spSiteGuid);
			LearningStoreHelper.CastNonNull(dataRow[1], out spWebGuid);
			LearningStoreHelper.CastNonNull(dataRow[2], out lastAccessTime);
			userWebList.Add(new SlkUserWebListItem(spSiteGuid, spWebGuid, lastAccessTime));
		}
		return new ReadOnlyCollection<SlkUserWebListItem>(userWebList);
    }

    /// <summary>
    /// Adds a given <c>SPWeb</c> to the current user's SharePoint Learning Kit user web list.
	/// If the <c>SPWeb</c> already exists in the user web list, it's last-access time is updated
	/// to be the current time.
    /// </summary>
	///
	/// <remarks>
	/// <para>
	/// The user web list is the list of Web sites that the user added to their E-Learning Actions
	/// page.
	/// </para>
	/// <para>
	/// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
	/// may be any user.
	/// </para>
	/// </remarks>
	///
    /// <param name="spWeb">The <c>SPWeb</c> to add.</param>
	///
    public void AddToUserWebList(SPWeb spWeb)
    {
        // Security checks: None

        // Check parameters
        if (spWeb == null)
            throw new ArgumentNullException("spWeb");

        // The code below only updates the web list for the current user.  All users
        // are allowed to do that, so we don't really need to check security
        using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
        {
            LearningStoreJob job = LearningStore.CreateJob();
		    Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
		    Dictionary<string, object> updateProperties = new Dictionary<string, object>();
            uniqueProperties.Add(Schema.UserWebListItem.OwnerKey, CurrentUserKey);
            uniqueProperties.Add(Schema.UserWebListItem.SPSiteGuid, spWeb.Site.ID);
            uniqueProperties.Add(Schema.UserWebListItem.SPWebGuid, spWeb.ID);
            updateProperties.Add(Schema.UserWebListItem.LastAccessTime,
                DateTime.Now.ToUniversalTime());
            job.AddOrUpdateItem(Schema.UserWebListItem.ItemTypeName, uniqueProperties,
                updateProperties);
            job.Execute();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	//

	/// <summary>
	/// Retrieves general information about an assignment.
	/// </summary>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	retrieve information about.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
	///
	/// <param name="basicOnly">If <c>true</c>, the <c>Instructors</c> and <c>Learners</c>
	/// 	properties of the returned <c>AssignmentProperties</c> object are not set.</param>
	///
	/// <returns>
	/// An <c>AssignmentProperties</c> object containing information about the assignment.
	/// Note that only the <c>UserId</c> and <c>Name</c> of each <c>SlkUser</c> object within the
	/// returned <c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>
	/// collections is valid, and the <c>Name</c> is a cache of the user's name from the first
	/// time the user was added to the SLK store (which may not be the user's current name in
	/// SharePoint).
	/// </returns>
	///
	private AssignmentProperties GetAssignmentProperties(
		AssignmentItemIdentifier assignmentId, SlkRole slkRole, bool basicOnly)
	{
        // Security checks: Fails if the user isn't an instructor
        // on the assignment (if SlkRole=Instructor) or if the user isn't
        // a learner on the assignment (if SlkRole=Learner).  Implemented
        // by schema rules.

        // create a LearningStore job
        LearningStoreJob job = LearningStore.CreateJob();

		// add to <job> request(s) to get information about the assignment
		BeginGetAssignmentProperties(job, assignmentId, slkRole, basicOnly);

        IEnumerator<object> resultEnumerator;
        try
        {
            // execute the job; set <resultEnumerator> to enumerate the results
            resultEnumerator = job.Execute().GetEnumerator();
        }
        catch(LearningStoreSecurityException)
        {
            // this error message includes the assignment ID, but that's okay since
            // the information we provide does not allow the user to distinguish between the
            // assignment not existing and the user not having access to it
            throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture,
                AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
        }

		// retrieve from <resultEnumerator> information requested by BeginGetAssignmentProperties()
		return EndGetAssignmentProperties(resultEnumerator, assignmentId, slkRole, basicOnly);
	}

	/// <summary>
	/// Adds to an existing LearningStore job request(s) to retrieve general information about an
	/// assignment.
	/// </summary>
	///
	/// <param name="job">The <c>LearningStoreJob</c> add request(s) to.</param>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
	/// 	retrieve information about.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
	///
	/// <param name="basicOnly">If <c>true</c>, the <c>Instructors</c> and <c>Learners</c>
	/// 	properties of the returned <c>AssignmentProperties</c> object are not set.</param>
	///
	/// <remarks>
	/// <para>
	/// After executing the LearningStore job, call <c>EndGetAssignmentProperties</c> to retrieve
	/// from the results information requested by this method.
	/// </para>
	/// </remarks>
	///
	private void BeginGetAssignmentProperties(LearningStoreJob job,
		AssignmentItemIdentifier assignmentId, SlkRole slkRole, bool basicOnly)
	{
        // Security checks: None (since it doesn't call the database or
        // SharePoint)
                
        // request basic information about the specified assignment
		LearningStoreQuery query = LearningStore.CreateQuery(
			Schema.AssignmentPropertiesView.ViewName);
        query.SetParameter(Schema.AssignmentPropertiesView.AssignmentId, assignmentId);
		query.SetParameter(Schema.AssignmentPropertiesView.IsInstructor,
			(slkRole == SlkRole.Instructor));
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentSPSiteGuid);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentSPWebGuid);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentTitle);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentStartDate);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDueDate);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentPointsPossible);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDescription);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentAutoReturn);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentShowAnswersToLearners);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentCreatedById);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDateCreated);
		query.AddColumn(Schema.AssignmentPropertiesView.PackageFormat);
		query.AddColumn(Schema.AssignmentPropertiesView.PackageLocation);
		query.AddColumn(Schema.AssignmentPropertiesView.AssignmentNonELearningLocation);
		job.PerformQuery(query);

		// if specified, request collections
		if (!basicOnly && (slkRole == SlkRole.Instructor))
		{
            // request the collection of instructors of this assignment
            query = LearningStore.CreateQuery(Schema.InstructorAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.InstructorAssignmentListForInstructors.InstructorId);
            query.AddColumn(Schema.InstructorAssignmentListForInstructors.InstructorName);
            query.AddCondition(Schema.InstructorAssignmentListForInstructors.AssignmentId,
                LearningStoreConditionOperator.Equal, assignmentId);
            job.PerformQuery(query);

            // request the collection of learners of this assignment
            query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId,
                LearningStoreConditionOperator.Equal, assignmentId);
            job.PerformQuery(query);
		}
	}

	/// <summary>
	/// Retrieves from the results of an executed LearningStore job information requested by a
	/// previous call to <c>BeginGetAssignmentProperties</c>.
	/// </summary>
	///
	/// <param name="resultEnumerator">An enumerator that enumerates the results of the executed
	/// 	LearningStore job.  This method will call <c>MoveNext</c> on the enumerator to retrieve
	///		information specified by a previous call to <c>BeginGetAssignmentProperties</c>.
	/// 	</param>
	///
	/// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment for which
	/// 	information is being retrieved.</param>
	///
	/// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
	///
	/// <param name="basicOnly">If <c>true</c>, the <c>Instructors</c> and <c>Learners</c>
	/// 	properties of the returned <c>AssignmentProperties</c> object are not set.</param>
	///
	/// <remarks>
	/// <para>
	/// If <paramref name="slkRole"/> is <c>SlkRole.Learner</c>, then <c>Instructors</c> and
	/// <c>Learners</c> in the returned <c>AssignmentProperties</c> object will be <c>null</c>,
	/// since learners are not permitted to retrieve that information.
	/// </para>
	/// </remarks>
	///
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static AssignmentProperties EndGetAssignmentProperties(IEnumerator<object> resultEnumerator,
		AssignmentItemIdentifier assignmentId, SlkRole slkRole, bool basicOnly)
	{
        // Security checks: None (since it doesn't call the database or
        // SharePoint)
        
        // set <instructorRole> based on <slkRole>
		bool instructorRole;
        if (slkRole == SlkRole.Instructor)
			instructorRole = true;
        else
        if (slkRole == SlkRole.Learner)
			instructorRole = false;
        else
            throw new ArgumentException(AppResources.InvalidSlkRole, "slkRole");

		// extract the basic assignment information from the job results
		if (!resultEnumerator.MoveNext())
			throw new InternalErrorException("SLK1008");
        DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
        if (dataRows.Count != 1)
		{
            // this error message includes the assignment ID, but that's okay since
            // the information we provide does not allow the user to distinguish between the
            // assignment not existing and the user not having access to it
            throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, 
				AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
		}
        DataRow dataRow = dataRows[0];
        AssignmentProperties ap = new AssignmentProperties();

		// copy information from <dataRow> into properties of <ap>...

		Guid guid;
        LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentSPSiteGuid], out guid);
		ap.SPSiteGuid = guid;

        LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentSPWebGuid], out guid);
		ap.SPWebGuid = guid;

		string stringValue;
		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentTitle], out stringValue);
		ap.Title = stringValue;

        DateTime utc;
		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentStartDate], out utc);
        ap.StartDate = utc.ToLocalTime();

        DateTime? utcOrNull;
		LearningStoreHelper.Cast(
			dataRow[Schema.AssignmentPropertiesView.AssignmentDueDate], out utcOrNull);
        if (utcOrNull == null)
            ap.DueDate = null;
        else
            ap.DueDate = utcOrNull.Value.ToLocalTime();

		float? nullableFloat;
		LearningStoreHelper.Cast(
			dataRow[Schema.AssignmentPropertiesView.AssignmentPointsPossible], out nullableFloat);
		ap.PointsPossible = nullableFloat;

		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentDescription], out stringValue);
		ap.Description = stringValue;

		bool boolValue;
		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentAutoReturn], out boolValue);
		ap.AutoReturn = boolValue;

		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentShowAnswersToLearners],
			out boolValue);
		ap.ShowAnswersToLearners = boolValue;

		UserItemIdentifier userId;
		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentCreatedById], out userId);
		ap.CreatedById = userId; 
		LearningStoreHelper.CastNonNull(
			dataRow[Schema.AssignmentPropertiesView.AssignmentDateCreated], out utc);
        ap.DateCreated = utc.ToLocalTime();

        PackageFormat? packageFormat;
        LearningStoreHelper.Cast<PackageFormat>(
            dataRow[Schema.AssignmentPropertiesView.PackageFormat], out packageFormat);
        ap.PackageFormat = packageFormat;

		if (packageFormat != null)
		{
			// e-learning package
			LearningStoreHelper.CastNonNull(
				dataRow[Schema.AssignmentPropertiesView.PackageLocation], out stringValue);
			ap.Location = stringValue;
		}
		else
		{
			// non-e-learning document
            LearningStoreHelper.CastNonNull(
				dataRow[Schema.AssignmentPropertiesView.AssignmentNonELearningLocation],
				out stringValue);
            ap.Location = stringValue;
		}

        // if specified, extract the collections from the job results
        if (!basicOnly && instructorRole)
        {
            // populate AssignmentProperties.Instructors
            if (!resultEnumerator.MoveNext())
                throw new InternalErrorException("SLK1009");
            dataRows = ((DataTable)resultEnumerator.Current).Rows;
            string userName;
            foreach (DataRow dataRow2 in dataRows)
            {
                LearningStoreHelper.CastNonNull(dataRow2[
                    Schema.InstructorAssignmentListForInstructors.InstructorId], out userId);
                LearningStoreHelper.CastNonNull(dataRow2[
                    Schema.InstructorAssignmentListForInstructors.InstructorName], out userName);
                ap.Instructors.Add(new SlkUser(userId, null, userName));
            }

            // populate AssignmentProperties.Learners
            if (!resultEnumerator.MoveNext())
                throw new InternalErrorException("SLK1011");
            dataRows = ((DataTable)resultEnumerator.Current).Rows;
            foreach (DataRow dataRow2 in dataRows)
            {
                LearningStoreHelper.CastNonNull(dataRow2[
                    Schema.LearnerAssignmentListForInstructors.LearnerId], out userId);
                LearningStoreHelper.CastNonNull(dataRow2[
                    Schema.LearnerAssignmentListForInstructors.LearnerName], out userName);
                ap.Learners.Add(new SlkUser(userId, null, userName));
            }
        }

		// done
		return ap;
	}	
}

/// <summary>
/// Represents an item in a SharePoint Learning Kit user web list, which is a list of SharePoint
/// SPWebs that a given user has added to the list of Web sites in the E-Learning Actions page.
/// Each user has zero or more associated <c>SlkUserWebListItem</c> objects stored in the
/// SharePoint Learning Kit database.
/// </summary>
///
[DebuggerDisplay("SlkUserWebListItem {m_spWebGuid}, {m_lastAccessTime}")]
public class SlkUserWebListItem
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>SPSiteGuid</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spSiteGuid;

	/// <summary>
	/// Holds the value of the <c>SPWebGuid</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spWebGuid;

	/// <summary>
	/// Holds the value of the <c>LastAccessTime</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	DateTime m_lastAccessTime;

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the <c>Guid</c> of the <c>SPSite</c> associated with this user web list item.
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
	/// Gets the <c>Guid</c> of the <c>SPWeb</c> associated with this user web list item.
	/// </summary>
	public Guid SPWebGuid
	{
		[DebuggerStepThrough]
		get
		{
			return m_spWebGuid;
		}
	}

	/// <summary>
	/// Gets the <c>DateTime</c> that the Web site specified by this user web list item was
	/// accessed using SharePoint Learning Kit.  Unlike the related value stored in the SharePoint
	/// Learning Kit database, this value is a local date/time, not a UTC value.
	/// </summary>
	public DateTime LastAccessTime
	{
		[DebuggerStepThrough]
		get
		{
			return m_lastAccessTime;
		}
	}

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
    /// <param name="spSiteGuid">The value to use to initialize the <c>SPSiteGuid</c> property.
	/// 	</param>
	///
    /// <param name="spWebGuid">The value to use to initialize the <c>SPWebGuid</c> property.
	/// 	</param>
	///
    /// <param name="lastAccessTime">The value to use to initialize the <c>LastAccessTime</c>
	/// 	property.</param>
	///
    internal SlkUserWebListItem(Guid spSiteGuid, Guid spWebGuid, DateTime lastAccessTime)
	{
		m_spSiteGuid = spSiteGuid;
		m_spWebGuid = spWebGuid;
		m_lastAccessTime = lastAccessTime;
	}
}

/// <summary>
/// Represents a SharePoint Learning Kit learner or instructor.
/// </summary>
[DebuggerDisplay("SlkUser {UserId.GetKey()}: {Name}")]
public class SlkUser : IComparable<SlkUser>
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>UserId</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	UserItemIdentifier m_userId;

	/// <summary>
	/// Holds the value of the <c>SPUser</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SPUser m_spUser;

	/// <summary>
	/// Holds the value of the <c>Name</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_name;

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.
	/// </summary>
	public UserItemIdentifier UserId
	{
        [DebuggerStepThrough]
        get
        {
            return m_userId;
        }
   		[DebuggerStepThrough]
		internal set
		{
            m_userId = value;
		}
	}

	/// <summary>
	/// Gets the SharePoint <c>SPUser</c> object that represents this user.
	/// </summary>
	public SPUser SPUser
	{
		[DebuggerStepThrough]
		get
		{
			return m_spUser;
		}
	}

	/// <summary>
	/// Gets the name of the user.
	/// </summary>
	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return m_name;
		}
	}

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Initializes an instance of this class, given a <c>UserItemIdentifier</c>.
    /// </summary>
	///
	/// <param name="userId">The SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.
	/// 	</param>
	///
	/// <remarks>
	/// When this constructor is used, the <c>SPUser</c> and <c>Name</c> properties are
	/// <c>null</c>.
	/// </remarks>
    ///
    public SlkUser(UserItemIdentifier userId)
    {
        if(userId == null)
            throw new ArgumentNullException("userId");
            
		m_userId = userId;
    }

   
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
	/// <param name="userId">The SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.
	/// 	</param>
	///
	/// <param name="spUser">The SharePoint <c>SPUser</c> object that represents this user.</param>
	///
	/// <param name="name">The name of the user.</param>
    ///
    internal SlkUser(UserItemIdentifier userId, SPUser spUser, string name)
    {
		m_userId = userId;
		m_spUser = spUser;
		m_name = name;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// IComparable<SlkUser> Implementation
	//

    int IComparable<SlkUser>.CompareTo(SlkUser other)
    {
        return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}

/// <summary>
/// Represents a collection of SharePoint Learning Kit learners and/or instructors contained
/// within a SharePoint <c>SPGroup</c>.
/// </summary>
[DebuggerDisplay("SlkGroup: {Name}")]
public class SlkGroup : IComparable<SlkGroup>
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>SPGroup</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SPGroup m_spGroup;

    /// <summary>
    /// Holds the value of the <c>DomainGroup</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPUser m_domainGroup;

	/// <summary>
	/// Holds the value of the <c>Users</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	List<SlkUser> m_users;

	/// <summary>
	/// Holds the value of the <c>UserKeys</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	List<string> m_userKeys;

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the SharePoint <c>SPGroup</c> that is represented by this <c>SlkGroup</c>, or
    /// <c>null</c> if this <c>SlkGroup</c> represents a domain group.
	/// </summary>
	public SPGroup SPGroup
	{
		[DebuggerStepThrough]
		get
		{
			return m_spGroup;
		}
	}

    /// <summary>
    /// Gets the SharePoint <c>SPUser</c> of the domain group that is represented by this
    /// <c>SlkGroup</c>, or <c>null</c> if this <c>SlkGroup</c> represents a SharePoint group
    /// instead of a domain group.
    /// </summary>
    public SPUser DomainGroup
    {
        [DebuggerStepThrough]
        get
        {
            return m_domainGroup;
        }
    }

	/// <summary>
	/// Gets the members of this group, represented as a collection of <c>SlkUser</c> objects.
	/// </summary>
	public ReadOnlyCollection<SlkUser> Users
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyCollection<SlkUser>(m_users);
		}
	}

	/// <summary>
	/// Gets or sets the members of this group, represented as a collection of SID strings.
	/// </summary>
	internal List<string> UserKeys
	{
		[DebuggerStepThrough]
		get
		{
			return m_userKeys;
		}
		[DebuggerStepThrough]
		set
		{
			m_userKeys = value;
		}
	}

	/// <summary>
	/// Gets the name of the group.
	/// </summary>
	public string Name
	{
		[DebuggerStepThrough]
		get
		{
            if (m_spGroup != null)
                return m_spGroup.Name;
            else
                return m_domainGroup.Name;
		}
	}

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Internal Methods
	//

	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="spGroup">The SharePoint <c>SPGroup</c> represented by this <c>SlkGroup</c>,
    ///     if this <c>SlkGroup</c> represents a SharePoint group; <c>null</c> otherwise.</param>
    /// 
    /// <param name="domainGroup">The SharePoint <c>SPUser</c> represented by this <c>SlkGroup</c>
    ///     if this <c>SlkGroup</c> represents a domain group; <c>null</c> otherwise.</param>
	///
	internal SlkGroup(SPGroup spGroup, SPUser domainGroup)
	{
		m_spGroup = spGroup;
        m_domainGroup = domainGroup;
		m_users = new List<SlkUser>();
	}

	/// <summary>
	/// Adds a user to <c>Users</c>.
	/// </summary>
	///
	/// <param name="slkUser">The user to add.</param>
	///
	internal void AddUser(SlkUser slkUser)
	{
		m_users.Add(slkUser);
	}

    ///////////////////////////////////////////////////////////////////////////////////////////////
	// IComparable<SlkGroup> Implementation
	//

    /// <summary>
    /// Compares this <r>SlkGroup</r> to another.
    /// </summary>
    ///
    /// <param name="other">The other <r>SlkGroup</r>.</param>
    ///
    /// <returns>
    /// A value less than, equal to, or greater than zero, depending on whether this
    /// <r>SlkGroup</r> is less than, equal to, or greather than <pr>other</pr>.
    /// </returns>
    ///
    int IComparable<SlkGroup>.CompareTo(SlkGroup other)
    {
        return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}

/// <summary>
/// Represents a collection of <c>SlkUser</c> objects.  The key for the collection is the
/// <c>UserItemIdentifier</c> of each <c>SlkUser</c>.
/// </summary>
///
public class SlkUserCollection : KeyedCollection<UserItemIdentifier, SlkUser>
{
	// <summary> /*internal*/
	// Identifies <c>SlkUser.UserId</c> as the key for items in this collection.
	// </summary>
	//
	protected override UserItemIdentifier GetKeyForItem(SlkUser item)
	{
        return item.UserId;
	}
}

/// <summary>
/// Contains lists of instructors, learners, and learner groups on a given <c>SPWeb</c>.
/// </summary>
///
/// <remarks>
/// A user is considered an instructor on a given SharePoint <c>SPWeb</c> if they have the
/// instructor permission on that group, i.e. the permission defined by
/// <c>SlkSPSiteMapping.InstructorPermission</c>.  Similarly, a user is considered a learner
/// if they have the learner permission, <c>SlkSPSiteMapping.LearnerPermission</c>.
/// </remarks>
///
public class SlkMemberships
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>Instructors</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SlkUser[] m_instructors;

	/// <summary>
	/// Holds the value of the <c>Learners</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SlkUser[] m_learners;

	/// <summary>
	/// Holds the value of the <c>LearnerGroups</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	SlkGroup[] m_learnerGroups;

	///////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the collection of instructors on the SharePoint <c>SPWeb</c> associated with this
	/// object.  A user is considered an instructor on a given <c>SPWeb</c> if they have the
	/// instructor permission on that <c>SPWeb</c>, i.e. the permission defined by
	/// <c>SlkSPSiteMapping.InstructorPermission</c>.
	/// </summary>
	public ReadOnlyCollection<SlkUser> Instructors
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyCollection<SlkUser>(m_instructors);
		}
	}

	/// <summary>
	/// Gets the collection of learners on the SharePoint <c>SPWeb</c> associated with this object.
	/// A user is considered a learner on a given <c>SPWeb</c> if they have the learner permission
	/// on that <c>SPWeb</c>, i.e. the permission defined by
	/// <c>SlkSPSiteMapping.LearnerPermission</c>.
	/// </summary>
	public ReadOnlyCollection<SlkUser> Learners
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyCollection<SlkUser>(m_learners);
		}
	}

	/// <summary>
	/// Gets the collection of learner groups on the SharePoint <c>SPWeb</c> associated with this
	/// object.  A SharePoint <c>SPGroup</c> or domain group is considered a learner group on a
    /// given <c>SPWeb</c> if it has the learner permission on that <c>SPWeb</c>, i.e. the
    /// permission defined by <c>SlkSPSiteMapping.LearnerPermission</c>.
	/// </summary>
	public ReadOnlyCollection<SlkGroup> LearnerGroups
	{
		[DebuggerStepThrough]
		get
		{           
			return new ReadOnlyCollection<SlkGroup>(m_learnerGroups);
		}
	}

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Constructor
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
	/// <param name="instructors">The collection of instructors/>.
	/// 	(See the <c>Instructors</c> property.)</param>
	///
	/// <param name="learners">The collection of learners />.
	/// 	(See the <c>Learners</c> property.)</param>
	///
	/// <param name="learnerGroups">The collection of learner groups />.
	/// 	(See the <c>LearnerGroups</c> property.)</param>
    ///
    public SlkMemberships(SlkUser[] instructors, SlkUser[] learners,
		SlkGroup[] learnerGroups)
    {
        //Copies the passed arrays to the respective members.
        //Creates an empty array when the passed array is null.
        if (instructors != null)
        {
            m_instructors = new SlkUser[instructors.Length];
            instructors.CopyTo(m_instructors, 0);
        }
        else
        {
            m_instructors = new SlkUser[0];
        }

        if (learners != null)
        {
            m_learners = new SlkUser[learners.Length];
            learners.CopyTo(m_learners, 0);
        }
        else
        {
            m_learners = new SlkUser[0];
        }

        if (learnerGroups != null)
        {
            m_learnerGroups = new SlkGroup[learnerGroups.Length];
            learnerGroups.CopyTo(m_learnerGroups, 0);
        }
        else
        {
            m_learnerGroups = new SlkGroup[0];
        }        
    }
}

/// <summary>
/// Contains information about a SharePoint Learning Kit package.
/// </summary>
/// 
public class PackageInformation
{
    /// <summary>
    /// Holds the value of the <c>SPFile</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SPFile m_spFile;

    /// <summary>
    /// Holds the value of the <c>ManifestReader</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ManifestReader m_manifestReader;

    /// <summary>
    /// Holds the value of the <c>Title</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_title;

    /// <summary>
    /// Holds the value of the <c>Description</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_description;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// The <c>SPFile</c> of the package.
    /// </summary>
    public SPFile SPFile
    {
        [DebuggerStepThrough]
        get
        {
            return m_spFile;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_spFile = value;
        }
    }

    /// <summary>
    /// The manifest reader for the package
    /// </summary>
    public ManifestReader ManifestReader
    {
        [DebuggerStepThrough]
        get
        {
            return m_manifestReader;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_manifestReader = value;
        }
    }

    /// <summary>
    /// Title of the package
    /// </summary>
    public string Title
    {
        [DebuggerStepThrough]
        get
        {
            return m_title;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_title = value;
        }
    }

    /// <summary>
    /// Description for the package
    /// </summary>
    public string Description
    {
        [DebuggerStepThrough]
        get
        {
            return m_description;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_description = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    internal PackageInformation()
    {
    }
}

/// <summary>
/// Contains information about a SharePoint Learning Kit assignment, i.e. an e-learning package
/// (such as a SCORM package) or a non-e-learning document (such as a Microsoft Word document)
/// assigned to zero or more learners.
/// </summary>
///
public class AssignmentProperties
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>SPSiteGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spSiteGuid = Guid.Empty;

    /// <summary>
    /// Holds the value of the <c>SPWebGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spWebGuid = Guid.Empty;

    /// <summary>
    /// Holds the value of the <c>Title</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_title;

    /// <summary>
    /// Holds the value of the <c>Description</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_description;

    /// <summary>
    /// Holds the value of the <c>PointsPossible</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_pointsPossible;

    /// <summary>
    /// Holds the value of the <c>StartDate</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime m_startDate;

    /// <summary>
    /// Holds the value of the <c>DueDate</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime? m_dueDate;

    /// <summary>
    /// Holds the value of the <c>AutoReturn</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool m_autoReturn;

    /// <summary>
    /// Holds the value of the <c>ShowAnswersToLearners</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool m_showAnswersToLearners;

    /// <summary>
    /// Holds the value of the <c>CreatedById</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    UserItemIdentifier m_createdById;

    /// <summary>
    /// Holds the value of the <c>DateCreated</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime m_dateCreated;

    /// <summary>
    /// Holds the value of the <c>Instructors</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkUserCollection m_instructors = new SlkUserCollection();

    /// <summary>
    /// Holds the value of the <c>Learners</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SlkUserCollection m_learners = new SlkUserCollection();

    /// <summary>
    /// Holds the value of the <c>PackageFormat</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    PackageFormat? m_packageFormat;

    /// <summary>
    /// Holds the value of the <c>Location</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_location;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the <c>Guid</c> of the SPSite that contains the SPWeb that this assignment is
    /// associated with.
    /// </summary>
    public Guid SPSiteGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spSiteGuid;
        }
        [DebuggerStepThrough]
		internal set
		{
            m_spSiteGuid = value;
		}
    }

    /// <summary>
    /// Gets the <c>Guid</c> of the SPWeb that this assignment is associated with.
    /// </summary>
    public Guid SPWebGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spWebGuid;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_spWebGuid = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the assignment.
    /// </summary>
    public string Title
    {
        [DebuggerStepThrough]
        get
        {
            return m_title;
        }
        [DebuggerStepThrough]
        set
        {
            m_title = value;
        }
    }

    /// <summary>
    /// Gets or sets the description of the assignment.
    /// </summary>
    public string Description
    {
        [DebuggerStepThrough]
        get
        {
            return m_description;
        }
        [DebuggerStepThrough]
        set
        {
            m_description = value;
        }
    }

    /// <summary>
    /// Gets or sets the nominal maximum number of points possible for the assignment.
    /// <c>null</c> if points possible is not specified.
    /// </summary>
    public Nullable<Single> PointsPossible
    {
        [DebuggerStepThrough]
        get
        {
            return m_pointsPossible;
        }
        [DebuggerStepThrough]
        set
        {
            m_pointsPossible = value;
        }
    }

    /// <summary>
    /// Gets or sets the date/time that this assignment starts.  Unlike the related value stored in
    /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
    /// </summary>
    public DateTime StartDate
    {
        [DebuggerStepThrough]
        get
        {
            return m_startDate;
        }
        [DebuggerStepThrough]
        set
        {
            m_startDate = value;
        }
    }

    /// <summary>
    /// Gets or sets the due date/time of this assignment.  <c>null</c> if there is no due date.
    /// Unlike the related value stored in the SharePoint Learning Kit database, this value is a
    /// local date/time, not a UTC value.
    /// </summary>
    public Nullable<DateTime> DueDate
    {
        [DebuggerStepThrough]
        get
        {
            return m_dueDate;
        }
        [DebuggerStepThrough]
        set
        {
            m_dueDate = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether each learner assignment associated with this
    /// assignment should automatically be returned to the learner when the learner marks it as
    /// complete (after auto-grading), rather than requiring an instructor to manually return the
    /// assignment to the student.
    /// </summary>
    public bool AutoReturn
    {
        [DebuggerStepThrough]
        get
        {
            return m_autoReturn;
        }
        [DebuggerStepThrough]
        set
        {
            m_autoReturn = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether answers will be shown to the learner when a
    /// learner assignment associated with this assignment is returned to the learner.  This only
    /// applies to certain types of e-learning content.
    /// </summary>
    public bool ShowAnswersToLearners
    {
        [DebuggerStepThrough]
        get
        {
            return m_showAnswersToLearners;
        }
        [DebuggerStepThrough]
        set
        {
            m_showAnswersToLearners = value;
        }
    }

    /// <summary>
    /// Gets or sets the <c>UserItemIdentifier</c> of the user who created this assignment.
    /// </summary>
    public UserItemIdentifier CreatedById
    {
        [DebuggerStepThrough]
        get
        {
            return m_createdById;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_createdById = value;
        }
    }

    /// <summary>
    /// Gets the date/time that this assignment was created.  Unlike the related value stored in
    /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
    /// </summary>
    public DateTime DateCreated
    {
        [DebuggerStepThrough]
        get
        {
            return m_dateCreated;
        }
        [DebuggerStepThrough]
		internal set
		{
            m_dateCreated = value;
		}
    }

    /// <summary>
    /// Gets or sets the collection of instructors of this assignment.
    /// </summary>
    public SlkUserCollection Instructors
    {
        [DebuggerStepThrough]
        get
        {
            return m_instructors;
        }
    }

    /// <summary>
    /// Gets or sets the collection of learners assigned this assignment.
    /// </summary>
    public SlkUserCollection Learners
    {
        [DebuggerStepThrough]
        get
        {
            return m_learners;
        }
    }

    /// <summary>
    /// Returns the general file format of the e-learning package associated with this assignment,
	/// or <c>null</c> if a non-e-learning document is associated with this assignment.
    /// </summary>
	public Nullable<PackageFormat> PackageFormat
	{
        [DebuggerStepThrough]
        get
        {
            return m_packageFormat;
		}
        [DebuggerStepThrough]
        internal set
        {
            m_packageFormat = value;
        }
	}

    /// <summary>
	/// Gets the MLC SharePoint location string of the e-learning package or non-e-learning
	/// document associated with the assignment.
    /// </summary>
    public string Location
    {
        [DebuggerStepThrough]
        get
        {
			return m_location;
        }
        [DebuggerStepThrough]
        internal set
        {
			m_location = value;
		}
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
    ///
    public AssignmentProperties()
    {
    }

}

/// <summary>
/// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
/// about an assignment related to one of the learners of the assignment).  These are properties
/// generally accessible to the learner.
/// </summary>
///
public class LearnerAssignmentProperties
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>LearnerAssignmentId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentItemIdentifier m_learnerAssignmentId;

    /// <summary>
    /// Holds the value of the <c>GuidId</c> property. The GuidId like the LearnerAssignmentId 
    /// represents the LearnerAssignment uniquely
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_learnerAssignmentGuidId = Guid.Empty;
    /// <summary>
    /// Holds the value of the <c>AssignmentId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    AssignmentItemIdentifier m_assignmentId;

    /// <summary>
    /// Holds the value of the <c>SPSiteGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spSiteGuid = Guid.Empty;

    /// <summary>
    /// Holds the value of the <c>SPWebGuid</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_spWebGuid = Guid.Empty;

    /// <summary>
    /// Holds the value of the <c>RootActivityId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ActivityPackageItemIdentifier m_rootActivityId;

    /// <summary>
    /// Holds the value of the <c>Location</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_location;

    /// <summary>
    /// Holds the value of the <c>Title</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_title;

    /// <summary>
    /// Holds the value of the <c>Description</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_description;

    /// <summary>
    /// Holds the value of the <c>PointsPossible</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_pointsPossible;

    /// <summary>
    /// Holds the value of the <c>StartDate</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime m_startDate;

    /// <summary>
    /// Holds the value of the <c>DueDate</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime? m_dueDate;

    /// <summary>
    /// Holds the value of the <c>AutoReturn</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool m_autoReturn;

	/// <summary>
	/// Holds the value of the <c>HasInstructors</c> property.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool m_hasInstructors;

	/// <summary>
	/// Holds the value of the <c>ShowAnswersToLearners</c> property.
	/// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool m_showAnswersToLearners;

    /// <summary>
    /// Holds the value of the <c>CreatedById</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    UserItemIdentifier m_createdById;

    /// <summary>
    /// Holds the value of the <c>CreatedByName</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_createdByName;

    /// <summary>
    /// Holds the value of the <c>LearnerId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    UserItemIdentifier m_learnerId;

    /// <summary>
    /// Holds the value of the <c>LearnerName</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_learnerName;

    /// <summary>
    /// Holds the value of the <c>Status</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentState m_status;

    /// <summary>
    /// Holds the value of the <c>AttemptId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    AttemptItemIdentifier m_attemptId;

    /// <summary>
    /// Holds the value of the <c>CompletionStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    CompletionStatus m_completionStatus;

    /// <summary>
    /// Holds the value of the <c>SuccessStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SuccessStatus m_successStatus;

    /// <summary>
    /// Holds the value of the <c>GradedPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_gradedPoints;

    /// <summary>
    /// Holds the value of the <c>FinalPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_finalPoints;

    /// <summary>
    /// Holds the value of the <c>InstructorComments</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_instructorComments;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the identifier of the learner assignment represented by this object.
    /// </summary>
    public LearnerAssignmentItemIdentifier LearnerAssignmentId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentId;
        }
        internal set
        {
            m_learnerAssignmentId = value;
        }
    }

    /// <summary>
    /// Gets the GUID identifier of the learner assignment represented by this object.
    /// </summary>
    public Guid LearnerAssignmentGuidId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentGuidId;
        }
    }

    /// <summary>
    /// Gets the identifier of the assignment that this learner assignment is associated with.
    /// </summary>
    public AssignmentItemIdentifier AssignmentId
    {
        [DebuggerStepThrough]
        get
        {
            return m_assignmentId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_assignmentId = value;
        }
    }

    /// <summary>
    /// Gets the <c>Guid</c> of the SPSite that contains the SPWeb that the assignment is
    /// associated with.
    /// </summary>
    public Guid SPSiteGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spSiteGuid;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_spSiteGuid = value;
        }
    }

    /// <summary>
    /// Gets the <c>Guid</c> of the SPWeb that the assignment is associated with.
    /// </summary>
    public Guid SPWebGuid
    {
        [DebuggerStepThrough]
        get
        {
            return m_spWebGuid;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_spWebGuid = value;
        }
    }

    /// <summary>
    /// Gets the identifier of the root activity of the e-learning package (SCORM or LRM) that this
    /// assignment is associated with.  <c>null</c> if a non-e-learning document is associated
    /// with the assignment.
    /// </summary>
    public ActivityPackageItemIdentifier RootActivityId
    {
        [DebuggerStepThrough]
        get
        {
            return m_rootActivityId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_rootActivityId = value;
        }
    }

    /// <summary>
	/// Gets the MLC SharePoint location string of the e-learning package or non-e-learning
	/// document associated with the assignment.
    /// </summary>
    public string Location
    {
        [DebuggerStepThrough]
        get
        {
			return m_location;
        }
        [DebuggerStepThrough]
        internal set
        {
			m_location = value;
		}
    }

    /// <summary>
    /// Gets the title of the assignment.
    /// </summary>
    public string Title
    {
        [DebuggerStepThrough]
        get
        {
            return m_title;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_title = value;
        }
    }

    /// <summary>
    /// Gets the description of the assignment.
    /// </summary>
    public string Description
    {
        [DebuggerStepThrough]
        get
        {
            return m_description;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_description = value;
        }
    }

    /// <summary>
    /// Gets the nominal maximum number of points possible for the assignment.  <c>null</c> if
    /// points possible is not specified.
    /// </summary>
    public Nullable<Single> PointsPossible
    {
        [DebuggerStepThrough]
        get
        {
            return m_pointsPossible;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_pointsPossible = value;
        }
    }

    /// <summary>
    /// Gets the date/time that the assignment starts.  Unlike the related value stored in the
    /// SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
    /// </summary>
    public DateTime StartDate
    {
        [DebuggerStepThrough]
        get
        {
            return m_startDate;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_startDate = value;
        }
    }

    /// <summary>
    /// Gets the due date/time of the assignment.  <c>null</c> if there is no due date.  Unlike
    /// the related value stored in the SharePoint Learning Kit database, this value is a local
    /// date/time, not a UTC value.
    /// </summary>
    public Nullable<DateTime> DueDate
    {
        [DebuggerStepThrough]
        get
        {
            return m_dueDate;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_dueDate = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether each learner assignment associated with this assignment
    /// should automatically be returned to the learner when the learner marks it as complete
    /// (after auto-grading), rather than requiring an instructor to manually return the assignment
    /// to the student.
    /// </summary>
    public bool AutoReturn
    {
        [DebuggerStepThrough]
        get
        {
            return m_autoReturn;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_autoReturn = value;
        }
    }

	/// <summary>
	/// Gets a value that indicates whether the assignment associated with this learner assignment
	/// has instructors.  This value is <c>false</c> in the case of a self-assigned assignment.
	/// </summary>
	public bool HasInstructors
	{
		[DebuggerStepThrough]
		get
		{
			return m_hasInstructors;
		}
		[DebuggerStepThrough]
		internal set
		{
			m_hasInstructors = value;
		}
	}

	/// <summary>
	/// Gets a value that indicates whether answers will be shown to the learner when a learner
	/// assignment associated with the assignment is returned to the learner.  This only applies to
	/// certain types of e-learning content.
	/// </summary>
	public bool ShowAnswersToLearners
	{
		[DebuggerStepThrough]
		get
		{
			return m_showAnswersToLearners;
		}
		[DebuggerStepThrough]
		internal set
		{
			m_showAnswersToLearners = value;
		}
	}

    /// <summary>
    /// Gets the <c>UserItemIdentifier</c> of the user who created the assignment.
    /// </summary>
    public UserItemIdentifier CreatedById
    {
        [DebuggerStepThrough]
        get
        {
            return m_createdById;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_createdById = value;
        }
    }

    /// <summary>
    /// Gets the name of the user who created the assignment.
    /// </summary>
    public string CreatedByName
    {
        [DebuggerStepThrough]
        get
        {
            return m_createdByName;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_createdByName = value;
        }
    }

    /// <summary>
    /// Gets the <c>UserItemIdentifier</c> of the user that this learner assignment is assigned to.
    /// </summary>
    public UserItemIdentifier LearnerId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerId = value;
        }
    }

    /// <summary>
    /// Gets the name of the user that this learner assignment is assigned to.
    /// </summary>
    public string LearnerName
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerName;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerName = value;
        }
    }

    /// <summary>
    /// Gets the <c>LearnerAssignmentState</c> of this learner assignment.
    /// </summary>
    public LearnerAssignmentState Status
    {
        [DebuggerStepThrough]
        get
        {
            return m_status;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_status = value;
        }
    }

    /// <summary>
    /// Gets the <c>AttemptId</c> of this learner assignment.  <c>null</c> if the assignment is a
	/// non-e-learning assignment, or if it's an e-learning assignment which the learner hasn't
	/// yet launched for the first time.
    /// </summary>
    public AttemptItemIdentifier AttemptId
    {
        [DebuggerStepThrough]
        get
        {
            return m_attemptId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_attemptId = value;
        }
    }

    /// <summary>
    /// Gets a <c>CompletionStatus</c> value indicating whether the SCORM 2005 package associated
	/// with this content considers the content to be completed by the learner.  This property is
	/// only used for assignments of SCORM 2004 packages; <c>CompletionStatus.Unknown</c> is
	/// returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public CompletionStatus CompletionStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_completionStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_completionStatus = value;
        }
    }

    /// <summary>
    /// Gets a <c>SuccessStatus</c> value indicating whether the SCORM package associated with
	/// this content considers the learner to have succeeded (as defined by the content).  This
	/// property is only used for assignments of SCORM 2004 packages; <c>SuccessStatus.Unknown</c>
	/// is returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public SuccessStatus SuccessStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_successStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_successStatus = value;
        }
    }

    /// <summary>
    /// Gets the number of points the learner received from automatic and manual grading of the
	/// learner assignment.  If the content type does not support grading, or if the grade is
	/// "blank", <c>GradedPoints</c> will be <c>null</c>.
    /// </summary>
    public Nullable<Single> GradedPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_gradedPoints;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_gradedPoints = value;
        }
    }

    /// <summary>
    /// Gets the number of points the learner received on this learner assignment.  When the
	/// learner submits the assignment, <c>FinalPoints</c> is initially the same as
	/// <c>GradedPoints</c>, but the instructor may manually change the value of <c>FinalPoint</c>.
	/// For example, the instructor may award bonus points to the learner.
    /// </summary>
    public Nullable<Single> FinalPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_finalPoints;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_finalPoints = value;
        }
    }

    /// <summary>
    /// Gets comments from the instructor (if any) on this learner assignment; <c>String.Empty</c>
	/// if none. 
    /// </summary>
    public string InstructorComments
    {
        [DebuggerStepThrough]
        get
        {
            return m_instructorComments;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_instructorComments = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
    /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by
	/// 	this object.</param>
	///
	internal LearnerAssignmentProperties(LearnerAssignmentItemIdentifier learnerAssignmentId)
	{
		m_learnerAssignmentId = learnerAssignmentId;
	}
    internal LearnerAssignmentProperties(Guid learnerAssignmentGuidId)
    {
        m_learnerAssignmentGuidId = learnerAssignmentGuidId;
    }
}

/// <summary>
/// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
/// about an assignment related to one of the learners of the assignment) that's used by an
/// instructor during grading.
/// </summary>
///
public class GradingProperties
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Private Fields
    //

    /// <summary>
    /// Holds the value of the <c>LearnerAssignmentId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentItemIdentifier m_learnerAssignmentId;

    /// <summary>
    /// Holds the value of the <c>LearnerAssignmentGuidId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid m_learnerAssignmentGuidId;

    /// <summary>
    /// Holds the value of the <c>LearnerId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    UserItemIdentifier m_learnerId;

    /// <summary>
    /// Holds the value of the <c>LearnerName</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_learnerName; // from: LearnerName

    /// <summary>
    /// Holds the value of the <c>Status</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    LearnerAssignmentState? m_status; // from: LearnerAssignmentState

    /// <summary>
    /// Holds the value of the <c>CompletionStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    CompletionStatus m_completionStatus;

    /// <summary>
    /// Holds the value of the <c>SuccessStatus</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SuccessStatus m_successStatus;

    /// <summary>
    /// Holds the value of the <c>GradedPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_gradedPoints;

    /// <summary>
    /// Holds the value of the <c>FinalPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    float? m_finalPoints; // from: FinalPoints

    /// <summary>
    /// Holds the value of the <c>IgnoreFinalPoints</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool m_ignoreFinalPoints;

    /// <summary>
    /// Holds the value of the <c>InstructorComments</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string m_instructorComments; // from: InstructorComments

    /// <summary>
    /// Holds the value of the <c>AttemptId</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    AttemptItemIdentifier m_attemptId;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Properties
    //

    /// <summary>
    /// Gets the identifier of the learner assignment represented by this object.
    /// </summary>
    public LearnerAssignmentItemIdentifier LearnerAssignmentId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentId;
        }
    }

    /// <summary>
    /// Gets the GUID identifier of the learner assignment represented by this object.
    /// </summary>
    public Guid LearnerAssignmentGuidId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerAssignmentGuidId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerAssignmentGuidId = value;
        }
    }

    /// <summary>
    /// Gets the <c>UserItemIdentifier</c> of the user that this learner assignment is assigned to.
    /// </summary>
    public UserItemIdentifier LearnerId
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerId;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerId = value;
        }
    }

    /// <summary>
    /// Gets the name of the user that this learner assignment is assigned to.
    /// </summary>
    public string LearnerName
    {
        [DebuggerStepThrough]
        get
        {
            return m_learnerName;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_learnerName = value;
        }
    }

    /// <summary>
    /// Gets or sets the <c>LearnerAssignmentState</c> of this learner assignment.
    /// </summary>
	///
	/// <remarks>
	/// <para>
	/// Changing the value of <c>Status</c>, and then calling
	/// <c>SlkStore.SetGradingProperties</c>, will transition this learner assignment to another
	/// <c>LearnerAssignmentState</c> value.  Only the following state transitions are supported
	/// by <c>SlkStore.SetGradingProperties</c>:
	/// </para>
	/// <list type="bullet">
	/// 	<item><description><c>NotStarted</c> to <c>Completed</c>.
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Active</c> to <c>Completed</c>.
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Completed</c> to <c>Final</c>.
	/// 		</description>
	/// 	</item>
	/// 	<item><description><c>Final</c> to <c>Active</c>.
	/// 		</description>
	/// 	</item>
	/// </list>
	/// <para>
	/// When setting <c>Status</c>, use the value <c>null</c> to indicate that you don't want to
	/// change the status of the assignment.  If you use the current value, the status also won't
	/// be changed, but you run the risk of another user changing the status between your calls
	/// to <c>SlkStore.GetGradingProperties</c> and <c>SlkStore.SetGradingProperties</c>.
	/// </para>
	/// </remarks>
	///
    public Nullable<LearnerAssignmentState> Status
    {
        [DebuggerStepThrough]
        get
        {
            return m_status;
        }
        [DebuggerStepThrough]
        set
        {
            m_status = value;
        }
    }

    /// <summary>
    /// Gets a <c>CompletionStatus</c> value indicating whether the SCORM 2005 package associated
	/// with this content considers the content to be completed by the learner.  This property is
	/// only used for assignments of SCORM 2004 packages; <c>CompletionStatus.Unknown</c> is
	/// returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public CompletionStatus CompletionStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_completionStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_completionStatus = value;
        }
    }

    /// <summary>
    /// Gets a <c>SuccessStatus</c> value indicating whether the SCORM package associated with
	/// this content considers the learner to have succeeded (as defined by the content).  This
	/// property is only used for assignments of SCORM 2004 packages; <c>SuccessStatus.Unknown</c>
	/// is returned for SCORM 1.2 and Class Server LRM assignments.
    /// </summary>
    public SuccessStatus SuccessStatus
    {
        [DebuggerStepThrough]
        get
        {
            return m_successStatus;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_successStatus = value;
        }
    }

    /// <summary>
    /// Gets the number of points the learner received from automatic and manual grading of the
	/// learner assignment.  If the content type does not support grading, or if the grade is
	/// "blank", <c>GradedPoints</c> will be <c>null</c>.
    /// </summary>
    public Nullable<Single> GradedPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_gradedPoints;
        }
        [DebuggerStepThrough]
        internal set
        {
            m_gradedPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets the number of points the learner received on this learner assignment.
    /// </summary>
    public Nullable<Single> FinalPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_finalPoints;
        }
        [DebuggerStepThrough]
        set
        {
            m_finalPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether <c>FinalPoints</c> will be ignore in subsequent
    /// calls to <c>SlkStore.SetGradingProperties</c>.  In that case, the current value of
	/// <c>FinalPoints</c> in the database is not changed to the value of
	/// <c>GradingProperties.FinalPoints</c>.
    /// </summary>
    public bool IgnoreFinalPoints
    {
        [DebuggerStepThrough]
        get
        {
            return m_ignoreFinalPoints;
        }
        [DebuggerStepThrough]
        set
        {
            m_ignoreFinalPoints = value;
        }
    }

    /// <summary>
    /// Gets or sets comments from the instructor (if any) on this learner assignment;
	/// <c>String.Empty</c> if none. 
    /// </summary>
    public string InstructorComments
    {
        [DebuggerStepThrough]
        get
        {
            return m_instructorComments;
        }
        [DebuggerStepThrough]
        set
        {
            m_instructorComments = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Internal Properties
	//

    /// <summary>
    /// Gets or sets the <c>AttemptItemIdentifier</c> of the attempt associated with this learner
	/// assignment, or <c>null</c> if none.
    /// </summary>
    internal AttemptItemIdentifier AttemptId
    {
        [DebuggerStepThrough]
        get
        {
            return m_attemptId;
        }
        [DebuggerStepThrough]
        set
        {
            m_attemptId = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Initializes an instance of this class.
    /// </summary>
	///
    /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by
	/// 	this object.</param>
	///
	public GradingProperties(LearnerAssignmentItemIdentifier learnerAssignmentId)
	{
	    if(learnerAssignmentId == null)
	        throw new ArgumentNullException("learnerAssignmentId");
	        
		m_learnerAssignmentId = learnerAssignmentId;
	}
}

/// <summary>
/// Specifies a role that a user may have in SLK, with respect to a Web site or an assignment.
/// A user may have multiple roles.
/// </summary>
public enum SlkRole
{
    /// <summary>
    /// No role
    /// </summary>
    None = 0,
    
    /// <summary>
    /// The user is an instructor on the Web site or assignment.
    /// </summary>
    Instructor = 1,

    /// <summary>
    /// The user is a learner on the Web site or assignment.
    /// </summary>
    Learner = 2,

    /// <summary>
    /// The user is an observer on the Web site or assignment.
    /// </summary>
    Observer = 3,
}

}

