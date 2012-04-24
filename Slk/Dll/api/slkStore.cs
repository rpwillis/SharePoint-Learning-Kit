using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;
using System.Web;
using Resources.Properties;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePointLearningKit.Schema;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Starts a job to go over multiple calls.</summary>
    /// <summary>
    /// Represents a connection to a SharePoint Learning Kit store, containing assignments, references
    /// to e-learning packages stored in SharePoint.  Also contains the identity of the user accessing
    /// the store (used for authorization purposes).
    /// </summary>
    ///
    public class SlkStore : ISlkStore
    {
#region fields
        CurrentJob currentJob;
        UserItemIdentifier currentUserId;

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

#endregion fields

#region internal constants
        /// <summary>
        /// Specifies for how many seconds <c>SlkStore</c> is cached within the current
        /// <c>HttpContext</c>.  This many seconds after being loaded, the next time you create an
        /// instance of <c>SlkStore</c>, information is reloaded from the SharePoint configuration
        /// database, and <c>SlkSettings</c> information is reloaded from the SharePoint Learning Kit
        /// database.
        /// </summary>
        internal const int HttpContextCacheTime = 60;
#endregion internal constants

#region properties
        /// <summary>See <see cref="ISlkStore.SPSiteGuid"/>.</summary>
        public Guid SPSiteGuid
        {
            [DebuggerStepThrough]
            get { return m_anonymousSlkStore.SPSiteGuid; }
        }

        /// <summary>See <see cref="ISlkStore.Mapping"/>.</summary>
        public SlkSPSiteMapping Mapping
        {
            [DebuggerStepThrough]
            get { return m_anonymousSlkStore.Mapping; }
        }

        /// <summary>See <see cref="ISlkStore.Settings"/>.</summary>
        public SlkSettings Settings
        {
            [DebuggerStepThrough]
            get { return m_anonymousSlkStore.Settings; }
        }

        /// <summary>
        /// Gets the <c>LearningStore</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.
        /// <c>LearningStore</c> contains information about e-learning assignments, learner
        /// assignments (including SCORM attempts), and other e-learning information.
        /// </summary>
        public LearningStore LearningStore
        {
            [DebuggerStepThrough]
            get { return m_learningStore; }
        }

        /// <summary>See <see cref="ISlkStore.PackageStore"/>.</summary>
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

        /// <summary>The SLK ID of the current user.</summary>
        public UserItemIdentifier CurrentUserId 
        {
            get 
            { 
                if (currentUserId == null)
                {
                    RetrieveCurrentUserId();
                }

                return currentUserId ;
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
#endregion properties

#region internal methods
        /// <summary>See <see cref="ISlkStore.StartBatchJobs"/>.</summary>
        public void StartBatchJobs()
        {
            currentJob = new CurrentJob(LearningStore);
        }

        /// <summary>See <see cref="ISlkStore.EndBatchJobs"/>.</summary>
        public void EndBatchJobs()
        {
            currentJob.Complete();
            currentJob.Dispose();
        }

        /// <summary>See <see cref="ISlkStore.EndBatchJobs"/>.</summary>
        public void CancelBatchJobs()
        {
            currentJob.Cancel();
            currentJob.Dispose();
        }
#endregion internal methods

#region public methods
        /// <summary>See <see cref="ISlkStore.LoadAssignmentReminders"/>.</summary>
        public IEnumerable<AssignmentProperties> LoadAssignmentReminders(DateTime minDueDate, DateTime maxDueDate)
        {
            List<AssignmentProperties> collection = new List<AssignmentProperties>();

            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                LearningStoreJob job = LearningStore.CreateJob();
                job.PerformQuery(ReminderEmailQuery(minDueDate, maxDueDate));
                DataRowCollection dataRows = job.Execute<DataTable>().Rows;

                AssignmentProperties properties = null;
                AssignmentItemIdentifier previousId = null;
                List<LearnerAssignmentProperties> learnerList = null;
                SPSite site = null;
                SPWeb web = null;

                try
                {
                    foreach (DataRow dataRow in dataRows)
                    {
                        AssignmentItemIdentifier id = CastNonNullIdentifier<AssignmentItemIdentifier>(dataRow[LearnerAssignmentList.AssignmentId]);
                        Guid siteId = CastNonNull<Guid>(dataRow[LearnerAssignmentList.AssignmentSPSiteGuid]);
                        Guid webId = CastNonNull<Guid>(dataRow[LearnerAssignmentList.AssignmentSPWebGuid]);

                        // If this row is for a different assignment then set up all the objects
                        if (id != previousId)
                        {
                            if (properties != null)
                            {
                                properties.AssignResults(learnerList);
                            }
                            properties = PopulateAssignmentProperties(id, dataRow);
                            collection.Add(properties);
                            learnerList = new List<LearnerAssignmentProperties>();
                            previousId = id;

                            if (site == null || siteId != site.ID)
                            {
                                if (site != null)
                                {
                                    site.Dispose();
                                }

                                site = new SPSite(siteId);
                            }

                            if (web == null || webId != web.ID)
                            {
                                if (web != null)
                                {
                                    web.Dispose();
                                }

                                web = site.OpenWeb(webId);
                            }

                        }

                        Guid learnerAssignmentGuid = CastNonNull<Guid>(dataRow[LearnerAssignmentList.LearnerAssignmentGuidId]);
                        LearnerAssignmentProperties learnerProperties = PopulateLearnerAssignmentProperties(dataRow, properties, learnerAssignmentGuid);

                        learnerProperties.User = LoadUser(web, dataRow, LearnerAssignmentList.LearnerId, LearnerAssignmentList.LearnerName, LearnerAssignmentList.LearnerKey, learnerAssignmentGuid);
                        learnerList.Add(learnerProperties);
                    }

                    if (properties != null)
                    {
                        properties.AssignResults(learnerList);
                    }
                }
                finally
                {
                    if (web != null)
                    {
                        web.Dispose();
                    }

                    if (site != null)
                    {
                        site.Dispose();
                    }
                }
            }

            return collection;
        }
#endregion public methods


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

        /// <summary>
        /// Returns the <c>SlkStore</c> object associated the SharePoint <c>SPSite</c> that a given
        /// SharePoint <c>SPWeb</c> belongs to.  If no such object exists (i.e. if SharePoint Learning
        /// Kit was not yet configured for the specified SPSite in SharePoint Central Administration),
        /// an exception is thrown.
        /// </summary>
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
        /// <param name="spWeb">The SharePoint <c>SPWeb</c> from which to retrieve the current user's
        ///     identity and the identity of the <c>SPSite</c> of the SLK store to open.</param>
        /// <param name="learningStoreUserKey">The key of the current learning store.</param>
        /// <returns></returns>
        public static SlkStore GetStore(SPWeb spWeb, string learningStoreUserKey)
        {
                // Security checks: None

            // Check parameters
            if(spWeb == null)
            {
                throw new ArgumentNullException("spWeb");
            }
                        
            // Get the interesting information from the SPWeb
            Guid spSiteGuid = spWeb.Site.ID;
                    
            //If the User not signed in throw user not found exception
            if (spWeb.CurrentUser == null)
            {
                throw new UserNotFoundException(AppResources.SlkExUserNotFound);
            }

            //Use SPUser LoginName if Sid is Null or Empty.
            SlkUser currentUser = new SlkUser(spWeb.CurrentUser);
            string currentUserKey = currentUser.Key;
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
            SlkSPSiteMapping mapping = SlkSPSiteMapping.GetRequiredMapping(spSiteGuid);

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
                DateTime settingsXmlLastModified = ((DateTime)dataRow[1]);
                using (StringReader stringReader = new StringReader(settingsXml))
                {
                        XmlReaderSettings xmlSettings = new XmlReaderSettings();
                        xmlSettings.Schemas.Add(xmlSchema);
                        xmlSettings.ValidationType = ValidationType.Schema;
                        using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlSettings))
                        {
                            settings = new SlkSettings(xmlReader, settingsXmlLastModified);
                        }
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
        void RetrieveCurrentUserId()
        {
            LearningStoreJob job = LearningStore.CreateJob();
            job.DisableFollowingSecurityChecks();
            
            // request the UserItemIdentifier of the current user -- create a UserItem for that user if
            // they're not yet in the SLK database
            Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
            Dictionary<string, object> updateProperties = new Dictionary<string, object>();
            uniqueProperties[Schema.UserItem.Key] = CurrentUserKey;
            updateProperties[Schema.UserItem.Name] = CurrentUserName;
            job.AddOrUpdateItem(Schema.UserItem.ItemTypeName, uniqueProperties, updateProperties, null, true);

            ReadOnlyCollection<object> results = job.Execute();
            IEnumerator<object> resultEnumerator = results.GetEnumerator();

            // retrieve the UserItemIdentifier of the current user from the job results and return it
            if (!resultEnumerator.MoveNext())
            {
                throw new InternalErrorException("SLK1003");
            }

            currentUserId = CastNonNullIdentifier<UserItemIdentifier>(resultEnumerator.Current);
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
        ///     string.</param>
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
            {
                throw new ArgumentNullException("location");
            }

            PackageDetails package = RegisterAndValidatePackage(location, true);
            return package.Warnings;
        }

        /// <summary>See <see cref="ISlkStore.RegisterAndValidatePackage"/>.</summary>
        public PackageDetails RegisterAndValidatePackage(string location)
        {
            return RegisterAndValidatePackage(location, false);
        }

        PackageDetails RegisterAndValidatePackage(string location, bool validateOnly)
        {
            PackageDetails package = new PackageDetails();
            
            // Security checks: Fails if user doesn't have access to the package (implemented by GetFileFromSharePointLocation)
            FileAndLocation fileLocation = GetFileFromSharePointLocation(location);
           
            // find the package in the SharePointPackageStore; add it if it doesn't exist;
            // set <packageId> to its PackageItemIdentifier; set <warnings> to refer to warnings
            // about the package contents
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
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
                    query.AddCondition(Schema.PackageItem.Location, LearningStoreConditionOperator.Equal, fileLocation.Location.ToString());
                    job.PerformQuery(query);
                    DataRowCollection result = job.Execute<DataTable>().Rows;
                    if (result.Count > 0)
                    {
                        // package exists in SharePointPackageStore, i.e. it's listed one or more times in
                        // the PackageItem table -- set <packageId> to the PackageItemIdentifier of the
                        // first of them
                        package.PackageId = CastNonNullIdentifier<PackageItemIdentifier>(result[0][Schema.PackageItem.Id]);
                        package.Warnings = Cast<LearningStoreXml>(result[0][Schema.PackageItem.Warnings]);
                    }
                    else
                    {
                        // package doesn't exist in SharePointPackageStore -- register it if <validateOnly>
                        // is false, or just validate it if <validateOnly> is true, and <log> to the
                        // validation log in either case
                        if (validateOnly)
                        {
                            package = ValidateActualPackage(fileLocation.Location);
                        }
                        else
                        {
                            package = RegisterAndValidatePackage(fileLocation.Location);
                        }
                    }
                }
                    
                scope.Complete();
            }

            return package;
        }

        /// <summary>Validates a package.</summary>
        /// <param name="location">The location of the package.</param>
        /// <returns>The package details.</returns>
        PackageDetails ValidateActualPackage(SharePointFileLocation location)
        {
            PackageDetails package = new PackageDetails();

            // validate the package, but don't register it
            package.PackageId = null;
            SharePointPackageReader packageReader;

            try
            {
                packageReader = new SharePointPackageReader(SharePointCacheSettings, location, false);
            }
            catch (InvalidPackageException ex)
            {
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.PackageNotValid, ex.Message));
            }

            using (packageReader)
            {
                PackageValidatorSettings settings = new PackageValidatorSettings(ValidationBehavior.LogWarning, ValidationBehavior.None, ValidationBehavior.LogError, ValidationBehavior.LogWarning);
                try
                {
                    ValidationResults log = PackageValidator.Validate(packageReader, settings);
                    if (log.HasErrors)
                    {
                        throw new SafeToDisplayException(log, String.Format(CultureInfo.CurrentCulture, AppResources.PackageNotValid, ""));
                    }
                    else if (log.HasWarnings)
                    {
                        using (XmlReader xmlLog = log.ToXml())
                        {
                            package.Warnings = LearningStoreXml.CreateAndLoad(xmlLog);
                        }
                    }

                    return package;
                }
                catch (InvalidPackageException ex)
                {
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.PackageNotValid, ex.Message));
                }
            }
        }

        /// <summary>Register the package in the SharePointPackageStore.</summary>
        PackageDetails RegisterAndValidatePackage(SharePointFileLocation location)
        {
            PackageDetails package = new PackageDetails();

            ValidationResults log;
            // validate and register the package
            PackageEnforcement pe = new PackageEnforcement(false, true, false);
            try
            {
                RegisterPackageResult registerResult = PackageStore.RegisterPackage(location, pe);
                package.PackageId = registerResult.PackageId;
                log = registerResult.Log;
            }
            catch (PackageImportException ex)
            {
                throw new SafeToDisplayException(ex.Log, ex.Message);
            }

            // if there were warnings, set <warnings> to refer to them and update the
            // PackageItem in LearningStore to contain the warnings (if there is a PackageItem)
            if (log.HasWarnings || log.HasErrors)
            {
                using (XmlReader xmlLog = log.ToXml())
                {
                    package.Warnings = LearningStoreXml.CreateAndLoad(xmlLog);
                }

                LearningStoreJob job = LearningStore.CreateJob();
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties[Schema.PackageItem.Warnings] = package.Warnings;
                job.UpdateItem(package.PackageId, properties);
                job.Execute();
            }

            return package;
        }                

        //////////////////////////////////////////////////////////////////////////////////////////////
            // Private Methods
            //
            /// <summary>
            /// Initializes an instance of this class.
            /// </summary>
            ///
            /// <param name="anonymousSlkStore">Information about the connection to the SharePoint
            ///     Learning Kit store that doesn't include any "current user" information.</param>
            ///
            /// <param name="learningStore">The <c>LearningStore</c> that connects to the SharePoint
            ///     Learning Kit store database, configured with a specified "current user" identity.
            ///     </param>
            ///
        /// <param name="currentUserName">The name of the current user.  Use <c>String.Empty</c> if
            ///     <c>String.Empty</c> is specified as the user key of <paramref name="learningStore"/>.
            ///     </param>
            ///
            SlkStore(AnonymousSlkStore anonymousSlkStore, LearningStore learningStore,
                    string currentUserName)
            {
            // Security checks: None

            m_anonymousSlkStore = anonymousSlkStore;
                    m_learningStore = learningStore;
                    m_currentUserName = currentUserName;
            }

        /// <summary>See <see cref="ISlkStore.EnsureInstructor"/>.</summary>
        public void EnsureInstructor(SPWeb spWeb)
        {
            // Security checks: Fails if the user doesn't have Reader access (implemented
            // by IsInstructor) or Instructor access.

            // Verify that the web is in the site
            if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
            {
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);
            }

            // Verify that the user is an instructor
            if (!IsInstructor(spWeb))
            {
                throw new NotAnInstructorException(String.Format(CultureInfo.CurrentCulture,AppResources.SlkExInstructorPermissonNotFound, spWeb.Title));
            }
        }

        private bool IsInRole(SPWeb spWeb, string roleDefinitionName)
        {
            // Security checks: Fails if the user doesn't have Reader access (implemented
            // by SharePoint)

            // Check parameters
            if (spWeb == null)
                throw new ArgumentNullException("spWeb");

            // Verify that the web is in the site
            if (spWeb.Site.ID != m_anonymousSlkStore.SPSiteGuid)
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);

            bool isInRole = false;

            SPRoleDefinitionBindingCollection roleDefinitionCollection = null;
            try
            {
                // In some cases this fails and we must take an alternate approach.
                roleDefinitionCollection = spWeb.AllRolesForCurrentUser;
            }
            catch (SPException spException)
            {
                System.Runtime.InteropServices.COMException comException = spException.InnerException as System.Runtime.InteropServices.COMException;
                if (comException == null || comException.ErrorCode != unchecked((int)0x80040E14)) // Not the specific case we're looking for, rethrow
                    throw;

                // Use a brute force iteration approach if the attempt to get AllRolesForCurrentUser fails with COMException 0x80040E14
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (SPSite site = new SPSite(spWeb.Site.ID))
                    {
                        using (SPWeb web = site.OpenWeb(spWeb.ID))
                        {
                            foreach (SPRoleAssignment roleAssignment in web.RoleAssignments)
                            {
                                SPUser user = roleAssignment.Member as SPUser;
                                if (user != null)
                                {
                                    if (string.Compare(user.LoginName, HttpContext.Current.User.Identity.Name, true, CultureInfo.InvariantCulture) != 0)
                                    {
                                        continue; // the roleAssignment is for a different user
                                    }
                                }
                                else
                                {
                                    SPGroup group = roleAssignment.Member as SPGroup;
                                    if (group != null)
                                    {
                                        if (!group.ContainsCurrentUser)
                                        {
                                            continue; // the roleAssignment is for a group the user is not a member of
                                        }
                                    }
                                }

                                foreach (SPRoleDefinition roleDefinition in roleAssignment.RoleDefinitionBindings)
                                {
                                    if (roleDefinition.Name == roleDefinitionName)
                                    {
                                        isInRole = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                });
            }

            if (roleDefinitionCollection != null)
            {
                foreach (SPRoleDefinition roleDefinition in roleDefinitionCollection)
                {
                    if (roleDefinition.Name == roleDefinitionName)
                    {
                        isInRole = true;
                        break;
                    }
                }
            }

            return isInRole;
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
                    return IsInRole(spWeb, m_anonymousSlkStore.Mapping.InstructorPermission);
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
            return IsInRole(spWeb, m_anonymousSlkStore.Mapping.ObserverPermission);        
        }

        /// <summary>
        /// Returns <c>true</c> if the current user is a learner on a given SPWeb, <c>false</c>
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
        public bool IsLearner(SPWeb spWeb)
        {
            return IsInRole(spWeb, m_anonymousSlkStore.Mapping.LearnerPermission);        
        }
        
        /// <summary>See <see cref="ISlkStore.AssignUserItemIdentifier"/>.</summary>
        public void AssignUserItemIdentifier(IEnumerable<SlkUser> users)
        {
            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();
            job.DisableFollowingSecurityChecks();

            // for each user in <users>, ask to get the user's UserItemIdentifier -- create a UserItem
            // for that user if they're not yet in the SLK database
            Dictionary<string, object> uniqueProperties = new Dictionary<string, object>();
            Dictionary<string, object> updateProperties = new Dictionary<string, object>();
            foreach (SlkUser user in users)
            {
                uniqueProperties[Schema.UserItem.Key] = user.Key;
                updateProperties[Schema.UserItem.Name] = user.Name;
                job.AddOrUpdateItem(Schema.UserItem.ItemTypeName, uniqueProperties, updateProperties, null, true);
            }

            // execute the job
            ReadOnlyCollection<object> results = job.Execute();
            IEnumerator<object> resultEnumerator = results.GetEnumerator();

            LearningStoreJob jobForSites = LearningStore.CreateJob();
            jobForSites.DisableFollowingSecurityChecks();

            // for each user in <users>, retrieve the user's UserItemIdentifier from the job results
            foreach (SlkUser user in users)
            {
                if (!resultEnumerator.MoveNext())
                {
                    throw new InternalErrorException("SLK1001");
                }

                user.UserId = CastNonNullIdentifier<UserItemIdentifier>(resultEnumerator.Current);

                // Save the link to the site collection back to the database
                Dictionary<string, object> uniquePropertiesForSite = new Dictionary<string, object>();
                Dictionary<string, object> updatePropertiesForSite = new Dictionary<string, object>();
                uniquePropertiesForSite[Schema.UserItemSite.UserId] = user.UserId;
                uniquePropertiesForSite[Schema.UserItemSite.SPSiteGuid] = SPSiteGuid;
                updatePropertiesForSite[Schema.UserItemSite.SPUserId] = user.SPUser.ID;
                jobForSites.AddOrUpdateItem(Schema.UserItemSite.ItemTypeName, uniquePropertiesForSite, updatePropertiesForSite);
            }

            if (resultEnumerator.MoveNext())
            {
                throw new InternalErrorException("SLK1002");
            }

            jobForSites.Execute();
        }


        /// <summary>Retrieves an <n>SPFile</n> given an MLC SharePoint location string. </summary>
        /// <param name="location">The MLC SharePoint location string.</param>
        /// <exception cref="UnauthorizedAccessException"><pr>location</pr> refers to a file that the user does not have access to.</exception>
        private static FileAndLocation GetFileFromSharePointLocation(string location)
        {
            // Security checks: Fails if user doesn't have access to the package (implemented
            // by accessing the assignment properties)

            FileAndLocation fileAndLocation;
            SharePointFileLocation fileLocation;

            if (SharePointFileLocation.TryParse(location, out fileLocation) == false)
            {
                throw new ArgumentException(AppResources.IncorrectLocationStringSyntax, "location");
            }

            fileAndLocation.Location = fileLocation;

            // Access the file to check user has access to it.
            using (SPSite spSite = new SPSite(fileLocation.SiteId))
            {
                using (SPWeb spWeb = spSite.OpenWeb(fileLocation.WebId))
                {
                    fileAndLocation.File = spWeb.GetFile(fileLocation.FileId);
                    Hashtable fileProperties = fileAndLocation.File.Properties;
                }
            }

            return fileAndLocation;
        }

        /// <summary>See <see cref="ISlkStore.LoadInstructors"/>.</summary>
        public void LoadInstructors(AssignmentItemIdentifier id, SlkUserCollection instructors)
        {
            LearningStoreJob job = LearningStore.CreateJob();
            AddInstructorsQuery(job, id);

            try
            {
                // execute the job; set <resultEnumerator> to enumerate the results
                IEnumerator<object> resultEnumerator = job.Execute().GetEnumerator();

                using (SPSite site = new SPSite(SPSiteGuid))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        AddSlkUsers(web, resultEnumerator, instructors, InstructorAssignmentList.InstructorId, InstructorAssignmentList.InstructorName, InstructorAssignmentList.InstructorKey, 
                                InstructorAssignmentList.InstructorAssignmentId, "SLK1011");
                    }
                }

            }
            catch (LearningStoreSecurityException)
            {
                // this error message includes the assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, id.GetKey()));
            }
        }

        /// <summary>See <see cref="ISlkStore.LoadAssignmentProperties"/>.</summary>
        public AssignmentProperties LoadAssignmentProperties(AssignmentItemIdentifier assignmentId, SlkRole slkRole)
        {
            // Security checks: Fails if the user isn't an instructor
            // on the assignment (if SlkRole=Instructor) or if the user isn't
            // a learner on the assignment (if SlkRole=Learner), since it
            // calls the other LoadAssignmentProperties method.

            // Check parameters
            if (assignmentId == null)
            {
                throw new ArgumentNullException("assignmentId");
            }

            if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            {
                throw new ArgumentOutOfRangeException("slkRole");
            }

            // Security checks: Fails if the user isn't an instructor
            // on the assignment (if SlkRole=Instructor) or if the user isn't
            // a learner on the assignment (if SlkRole=Learner).  Implemented
            // by schema rules.

            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();

            // add to <job> request(s) to get information about the assignment
            BeginGetAssignmentProperties(job, assignmentId, slkRole);

            IEnumerator<object> resultEnumerator;
            try
            {
                // execute the job; set <resultEnumerator> to enumerate the results
                resultEnumerator = job.Execute().GetEnumerator();
            }
            catch (LearningStoreSecurityException)
            {
                // this error message includes the assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
            }
            catch (Exception)
            {
                throw;
            }

            // retrieve from <resultEnumerator> information requested by BeginGetAssignmentProperties()
            return PopulateAssignmentProperties(resultEnumerator, assignmentId, slkRole);
        }

        /// <summary>See <see cref="ISlkStore.UpdateAssignment"/>.</summary>
        public void UpdateAssignment(AssignmentProperties assignment, bool corePropertiesChanged, SlkUserCollectionChanges instructorChanges, SlkUserCollectionChanges learnerChanges)
        {
            // Check parameters
            if (assignment == null)
            {
                throw new ArgumentNullException("assignment");
            }

            // perform the operation within a transaction so that if the operation fails no data is
            // committed to the database
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
                {
                    LearningStoreJob job = LearningStore.CreateJob();
                    Dictionary<string,object> properties = new Dictionary<string,object>();

                    // update properties of the assignment
                    if (corePropertiesChanged)
                    {
                        properties[Schema.AssignmentItem.Title] = assignment.Title;
                        properties[Schema.AssignmentItem.StartDate] = assignment.StartDate.ToUniversalTime();
                        DateTime sd = assignment.StartDate;
                        DateTime? dueDate = (assignment.DueDate == null) ? (DateTime?) null : assignment.DueDate.Value.ToUniversalTime();
                        properties[Schema.AssignmentItem.DueDate] = dueDate;
                        properties[Schema.AssignmentItem.PointsPossible] = assignment.PointsPossible;
                        properties[Schema.AssignmentItem.Description] = assignment.Description;
                        properties[Schema.AssignmentItem.AutoReturn] = assignment.AutoReturn;
                        properties[Schema.AssignmentItem.EmailChanges] = assignment.EmailChanges;
                        properties[Schema.AssignmentItem.ShowAnswersToLearners] = assignment.ShowAnswersToLearners;
                        job.UpdateItem(assignment.Id, properties);
                    }

                    // add instructors that the caller wants added to the assignment
                    foreach (SlkUser slkUser in instructorChanges.Additions)
                    {
                        properties.Clear();
                        properties[Schema.InstructorAssignmentItem.AssignmentId] = assignment.Id;
                        properties[Schema.InstructorAssignmentItem.InstructorId] = slkUser.UserId;
                        job.AddItem(Schema.InstructorAssignmentItem.ItemTypeName, properties);
                    }

                    // add learners that the caller wants added to the assignment
                    foreach (SlkUser slkUser in learnerChanges.Additions)
                    {
                        properties.Clear();
                        properties[Schema.LearnerAssignmentItem.AssignmentId] = assignment.Id;
                        properties[Schema.LearnerAssignmentItem.LearnerId] = slkUser.UserId;
                        properties[Schema.LearnerAssignmentItem.IsFinal] = false;
                        properties[Schema.LearnerAssignmentItem.NonELearningStatus] = null;
                        properties[Schema.LearnerAssignmentItem.FinalPoints] = null;
                        properties[Schema.LearnerAssignmentItem.InstructorComments] = String.Empty;
                        job.AddItem(Schema.LearnerAssignmentItem.ItemTypeName, properties);
                    }

                    foreach (SlkUser slkUser in instructorChanges.Removals)
                    {
                        job.DeleteItem(slkUser.AssignmentUserId);
                    }

                    foreach (SlkUser slkUser in learnerChanges.Removals)
                    {
                        job.DeleteItem(slkUser.AssignmentUserId);
                    }

                    // execute the LearningStore job
                    job.Execute();
                }
                
                scope.Complete();
            }
        }

        /// <summary>See <see cref="ISlkStore.FindRootActivity"/>.</summary>
        public ActivityPackageItemIdentifier FindRootActivity(PackageItemIdentifier packageId, int organizationIndex)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }

            // The RegisterPackage call above will fail if the user doesn't have access to
            // the package.  Therefore, since we've already verified that the user has
            // access to the package, we don't need to check package-related security
            // again.  So just perform the package-related operations within a
            // privileged scope
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                LearningStoreJob job = LearningStore.CreateJob();
                LearningStoreQuery query = LearningStore.CreateQuery(Schema.ActivityPackageItemView.ViewName);
                query.AddColumn(Schema.ActivityPackageItemView.Id);
                query.AddCondition(Schema.ActivityPackageItemView.PackageId, LearningStoreConditionOperator.Equal, packageId);
                query.AddCondition(Schema.ActivityPackageItemView.ParentActivityId, LearningStoreConditionOperator.Equal, null);
                query.AddSort(Schema.ActivityPackageItemView.OriginalPlacement, LearningStoreSortDirection.Ascending);
                job.PerformQuery(query);
                DataRowCollection result = job.Execute<DataTable>().Rows;
                if (result.Count == 0)
                {
                    // this error message includes the package ID, but that package ID came from
                    // another API call (above) -- it's pretty unlikely the user will see it
                    throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.PackageNotFoundInDatabase, packageId.GetKey()));
                }

                if ((organizationIndex < 0) || (organizationIndex >= result.Count))
                {
                    throw new SafeToDisplayException(AppResources.InvalidOrganizationIndex);
                }

                return CastNonNullIdentifier<ActivityPackageItemIdentifier>( result[organizationIndex][Schema.ActivityPackageItemView.Id]);
            }
        }

        void PopulateLearnerAssignmentIds(SlkUserCollection learners, AssignmentItemIdentifier assignmentId)
        {
            LearningStoreJob job = LearningStore.CreateJob();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(LearnerAssignmentListForInstructors.LearnerAssignmentGuidId);
            query.AddColumn(LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddColumn(LearnerAssignmentListForInstructors.LearnerId);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId, LearningStoreConditionOperator.Equal, assignmentId);
            job.PerformQuery(query);
            DataTable results = job.Execute<DataTable>();

            foreach (DataRow row in results.Rows)
            {
                Guid learnerAssignmentId = (Guid)row[LearnerAssignmentListForInstructors.LearnerAssignmentGuidId];
                LearningStoreItemIdentifier storeId = (LearningStoreItemIdentifier)row[LearnerAssignmentListForInstructors.LearnerId];
                UserItemIdentifier id = new UserItemIdentifier(storeId);
                learners[id].AssignmentUserGuidId = learnerAssignmentId;
            }
        }

        /// <summary>See <see cref="ISlkStore.CreateAssignment"/>.</summary>
        public AssignmentItemIdentifier CreateAssignment(AssignmentProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            AssignmentItemIdentifier assignmentId;
                
            // create the assignment; set <assignmentId> to its AssignmentItemIdentifier
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                // We've already verified all the security above, so just turn off security checks for the rest of this
                using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
                {            
                    // create a LearningStore job
                    LearningStoreJob job = LearningStore.CreateJob();

                    // create an AssignmentItem corresponding to the properties of this object; set
                    // <tempAssignmentId> to a temporary AssignmentItemIdentifier (that can be used only
                    // within this job) of the new assignment
                    // to the UserItemIdentifier of the instructor
                    Dictionary<string, object> dbProperties = new Dictionary<string, object>();
                    dbProperties[Schema.AssignmentItem.SPSiteGuid] = properties.SPSiteGuid;
                    dbProperties[Schema.AssignmentItem.SPWebGuid] = properties.SPWebGuid;
                    dbProperties[Schema.AssignmentItem.Title] = properties.Title;
                    dbProperties[Schema.AssignmentItem.StartDate] = properties.StartDate.ToUniversalTime();
                    dbProperties[Schema.AssignmentItem.DueDate] = ((properties.DueDate == null) ? null :
                                                (object) properties.DueDate.Value.ToUniversalTime());
                    dbProperties[Schema.AssignmentItem.PointsPossible] = properties.PointsPossible;
                    dbProperties[Schema.AssignmentItem.RootActivityId] = properties.RootActivityId;
                    if (properties.IsNonELearning)
                    {
                        dbProperties[Schema.AssignmentItem.NonELearningLocation] = properties.Location;
                    }
                    else
                    {
                        dbProperties[Schema.AssignmentItem.NonELearningLocation] = null;
                    }

                    dbProperties[Schema.AssignmentItem.Description] = properties.Description;
                    dbProperties[Schema.AssignmentItem.AutoReturn] = properties.AutoReturn;
                    dbProperties[Schema.AssignmentItem.EmailChanges] = properties.EmailChanges;
                    dbProperties[Schema.AssignmentItem.ShowAnswersToLearners] = properties.ShowAnswersToLearners;
                    dbProperties[Schema.AssignmentItem.CreatedBy] = CurrentUserId;
                    dbProperties[Schema.AssignmentItem.DateCreated] = DateTime.Now.ToUniversalTime();
                    AssignmentItemIdentifier tempAssignmentId = new AssignmentItemIdentifier(job.AddItem(Schema.AssignmentItem.ItemTypeName, dbProperties, true));

                    // create one InstructorAssignmentItem for each instructor of the assignment
                    foreach (SlkUser instructor in properties.Instructors)
                    {
                        Dictionary<string, object> instructorProperties = new Dictionary<string, object>();
                        instructorProperties[Schema.InstructorAssignmentItem.AssignmentId] = tempAssignmentId;
                        instructorProperties[Schema.InstructorAssignmentItem.InstructorId] = instructor.UserId;
                        job.AddItem(Schema.InstructorAssignmentItem.ItemTypeName, instructorProperties);
                    }

                    // create one LearnerAssignmentItem for each learner of the assignment
                    foreach (SlkUser learner in properties.Learners)
                    {
                        Dictionary<string, object> learnerProperties = new Dictionary<string, object>();
                        learnerProperties[Schema.LearnerAssignmentItem.AssignmentId] = tempAssignmentId;
                        learnerProperties[Schema.LearnerAssignmentItem.LearnerId] = learner.UserId;
                        learnerProperties[Schema.LearnerAssignmentItem.IsFinal] = false;
                        learnerProperties[Schema.LearnerAssignmentItem.NonELearningStatus] = null;
                        learnerProperties[Schema.LearnerAssignmentItem.FinalPoints] = null;
                        learnerProperties[Schema.LearnerAssignmentItem.InstructorComments] = string.Empty;
                        job.AddItem(Schema.LearnerAssignmentItem.ItemTypeName, learnerProperties);
                    }

                    // execute the job
                    ReadOnlyCollection<object> results = job.Execute();

                    // execute the job; set <assignmentId> to the "real" (permanent)
                    // AssignmentItemIdentifier of the newly-created assignment
                    LearningStoreHelper.CastNonNull(results[0], out assignmentId);

                    PopulateLearnerAssignmentIds(properties.Learners, assignmentId);
                    /*
                    int learnerPosition = 0;
                    foreach (SlkUser learner in properties.Learners)
                    {
                        learnerPosition++;
                        LearningStoreItemIdentifier learnerId;
                        LearningStoreHelper.CastNonNull(results[learnerPosition], out learnerId);
                        learner.AssignmentUserId = learnerId;
                    }
                    */

                    // finish the transaction
                    scope.Complete();
                }
            }

            return assignmentId;
        }

        /// <summary>See <see cref="ISlkStore.DeleteAssignment"/>.</summary>
        public void DeleteAssignment(AssignmentItemIdentifier assignmentId)
        {
            // Security checks: Fails if not an instructor on the assignment or not the user who
            // created the assignment (because of rules in the schema XML)

            // Check parameters
            if (assignmentId == null)
            {
                throw new ArgumentNullException("assignmentId");
            }

            try
            {
                LearningStoreJob job = LearningStore.CreateJob();
                job.DeleteItem(assignmentId);
                job.Execute();
            }
            catch (LearningStoreItemNotFoundException)
            {
                // if the user doesn't have access, LearningStoreItemNotFoundException isn't thrown --
                // instead, LearningStoreSecurityException is thrown            
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
            }
            catch (LearningStoreSecurityException)
            {
                //User doesn't have access
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
            }
        }

        /// <summary>
        /// Returns the <c>LearnerAssignmentItemIdentifier</c> of the learner assignment belonging to
        /// the current user, for a given assignment.  Returns <c>null</c> if the current user isn't a
        /// learner of the assignment.
        /// </summary>
        /// 
            /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
            ///     retrieve information about.</param>
            ///
            /// <remarks>
            /// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
            /// may be any user.
            /// </remarks>
            ///
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public Guid GetCurrentUserLearnerAssignment(AssignmentItemIdentifier assignmentId)
        {
            // Security checks: Returns null if not an learner on the assignment
            // (because of rules in the schema XML)

            // Check parameters
            if (assignmentId == null)
                throw new ArgumentNullException("assignmentId");

            LearningStoreJob job = LearningStore.CreateJob();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForLearners.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForLearners.LearnerAssignmentGuidId);
            query.AddCondition(Schema.LearnerAssignmentListForLearners.AssignmentId, LearningStoreConditionOperator.Equal, assignmentId);
            job.PerformQuery(query);
            DataRowCollection dataRows = job.Execute<DataTable>().Rows;
            if (dataRows.Count != 1)
            {
                return Guid.Empty; // learner assignment not found (or multiple learner assignments?)
            }
            else
            {
                return CastNonNull<Guid>(dataRows[0][Schema.LearnerAssignmentListForLearners.LearnerAssignmentGuidId]);
            }
        }

        /// <summary>Starts an attempt on a learner assignment.</summary>
        /// <param name="learnerAssignmentGuidId">The learner assignment</param>
        /// <returns>The id of the new attempt</returns>
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.  Possible causes: the user
        /// isn't an instructor or learner on the learner assignment, or an attempt has already
        /// been created for the learner assignment.
        /// </exception>
        public AttemptItemIdentifier StartAttemptOnLearnerAssignment(Guid learnerAssignmentGuidId)
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
            options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
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
                    
                    UserItemIdentifier learnerId = CastIdentifier<UserItemIdentifier>(dataRow[Schema.LearnerAssignmentView.LearnerId]);
                        
                    ActivityPackageItemIdentifier rootActivityId = CastIdentifier<ActivityPackageItemIdentifier>(dataRow[Schema.LearnerAssignmentView.RootActivityId]);

                    LearningStoreHelper.Cast(dataRow[Schema.LearnerAssignmentView.AttemptId],
                        out attemptId);

                    // Verify that there isn't already an attempt
                    if(attemptId != null)
                    {
                        throw new InternalErrorException("SLK1013");
                    }

                    LearningStoreItemIdentifier learnerAssignmentId = Cast<LearningStoreItemIdentifier>(dataRow[Schema.LearnerAssignmentView.LearnerAssignmentId]);

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
                        {
                            throw;
                        }
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
                return CastNonNull<Guid>(dataRow[0]);
            }
        }
        
        /// <summary></summary>
        /// <param name="learnerAssignmentGuidId"></param>
        /// <param name="slkRole"></param>
        /// <returns></returns>
        public AssignmentProperties LoadAssignmentPropertiesForLearner(Guid learnerAssignmentGuidId, SlkRole slkRole)
        {
            // Security checks: Fails if the user doesn't have access to the learner assignment (since
            // the query is limited to only the information the user has access to, and an exception
            // is thrown if zero rows are returned)

            // Check parameters
            if (learnerAssignmentGuidId.Equals(Guid.Empty)== true)
            {
                throw new ArgumentNullException("learnerAssignmentId");
            }

            // create a LearningStore job
            LearningStoreJob job = LearningStore.CreateJob();

            // request information about the specified learner assignment
            LearningStoreQuery query = CreateQueryForLearnerAssignmentProperties(learnerAssignmentGuidId, slkRole);
            job.PerformQuery(query);
            ReadOnlyCollection<object> results = job.Execute();
            IEnumerator<object> resultEnumerator = results.GetEnumerator();

            // retrieve from <resultEnumerator> information about the learner assignment
            if (!resultEnumerator.MoveNext())
            {
                throw new InternalErrorException("SLK1012");
            }

            DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
            if (dataRows.Count != 1)
            {
                // this error message includes the learner assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // learner assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.LearnerAssignmentNotFoundInDatabase, learnerAssignmentGuidId.ToString()));
            }

            DataRow dataRow = dataRows[0];

            AssignmentItemIdentifier id = CastNonNullIdentifier<AssignmentItemIdentifier>(dataRow[LearnerAssignmentList.AssignmentId]);
            AssignmentProperties properties = PopulateAssignmentProperties(id, dataRow);
            LearnerAssignmentProperties learnerProperties = PopulateLearnerAssignmentProperties(dataRow, properties, learnerAssignmentGuidId);

            if (properties.IsNonELearning)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (SPSite site = new SPSite(properties.SPSiteGuid))
                    {
                        using (SPWeb web = site.OpenWeb(properties.SPWebGuid))
                        {
                            learnerProperties.User = LoadUser(web, dataRow, LearnerAssignmentList.LearnerId, LearnerAssignmentList.LearnerName, LearnerAssignmentList.LearnerKey);
                        }
                    }
                });
            }

            List<LearnerAssignmentProperties> list = new List<LearnerAssignmentProperties>();
            list.Add(learnerProperties);
            properties.AssignResults(list);

            return properties;
        }

        /// <summary>Retrieves grading-related information about an assignment from the SLK database.</summary>
        /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
        ///     retrieve information about.</param>
        /// <returns>
        /// An AssignmentProperties object containing 
        /// a collection of <c>LearnerAssignmentProperties</c> objects, one for each learner assignment in the
        /// assignment, sorted by learner name.
        /// </returns>
        /// <remarks>
        /// <b>Security:</b>&#160; Fails if the
        /// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't an instructor on the
        /// assignment.
        /// </remarks>
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.  Possible cause: the user isn't
        /// an instructor on the assignment.
        /// </exception>
        public AssignmentProperties GetGradingProperties(AssignmentItemIdentifier assignmentId)
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
            BeginGetAssignmentProperties(job, assignmentId, SlkRole.Instructor);

            // add to <job> a request to get information about each learner assignment
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentGuidId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
            query.AddColumn(Schema.UserItemSite.SPUserId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerKey);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.Grade);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentListForInstructors.AttemptId);
            query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId, LearningStoreConditionOperator.Equal, assignmentId);
            query.AddSort(Schema.LearnerAssignmentListForInstructors.LearnerName, LearningStoreSortDirection.Ascending);
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
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
            }        
            IEnumerator<object> resultEnumerator = results.GetEnumerator();

            // retrieve from <resultEnumerator> information requested by BeginGetAssignmentProperties()
            AssignmentProperties properties = PopulateAssignmentProperties(resultEnumerator, assignmentId, SlkRole.Instructor);

            // retrieve from <resultEnumerator> information about each learner assignment
            if (!resultEnumerator.MoveNext())
            {
                    throw new InternalErrorException("SLK1004");
            }

            DataRowCollection dataRows = ((DataTable) resultEnumerator.Current).Rows;
            List<LearnerAssignmentProperties> gpList = new List<LearnerAssignmentProperties>(dataRows.Count);

            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(properties.SPSiteGuid))
                {
                    using (SPWeb web = site.OpenWeb(properties.SPWebGuid))
                    {

                        foreach (DataRow dataRow in dataRows)
                        {
                            // set <gp> to a new LearnerAssignmentProperties object
                            LearnerAssignmentItemIdentifier learnerAssignmentId;
                            LearningStoreHelper.CastNonNull(dataRow[LearnerAssignmentListForInstructors.LearnerAssignmentId], out learnerAssignmentId);
                            LearnerAssignmentProperties gp = new LearnerAssignmentProperties(learnerAssignmentId, properties);
                            gp.LearnerId = CastNonNullIdentifier<UserItemIdentifier>( dataRow[LearnerAssignmentListForInstructors.LearnerId]);

                            gp.LearnerName = CastNonNull<string>(dataRow[LearnerAssignmentListForInstructors.LearnerName]);
                            gp.Status = CastNonNull<LearnerAssignmentState>(dataRow[LearnerAssignmentListForInstructors.LearnerAssignmentState]);
                            gp.GradedPoints = Cast<float?>(dataRow[LearnerAssignmentListForInstructors.AttemptGradedPoints]);

                            gp.CompletionStatus = Cast<CompletionStatus>(dataRow[LearnerAssignmentListForInstructors.AttemptCompletionStatus], CompletionStatus.Unknown);

                            gp.SuccessStatus = Cast<SuccessStatus>(dataRow[LearnerAssignmentListForInstructors.AttemptSuccessStatus], SuccessStatus.Unknown);

                            gp.FinalPoints = Cast<float?>(dataRow[LearnerAssignmentListForInstructors.FinalPoints]);
                            gp.Grade = Cast<string>(dataRow[LearnerAssignmentListForInstructors.Grade]);
                            gp.InstructorComments = CastNonNull<string>(dataRow[LearnerAssignmentListForInstructors.InstructorComments]);
                            gp.LearnerAssignmentGuidId = CastNonNull<Guid>(dataRow[LearnerAssignmentListForInstructors.LearnerAssignmentGuidId]);
                            gp.AttemptId = CastIdentifier<AttemptItemIdentifier>( dataRow[LearnerAssignmentListForInstructors.AttemptId]);
                            gp.User = LoadUser(web, dataRow, LearnerAssignmentList.LearnerId, LearnerAssignmentList.LearnerName, LearnerAssignmentList.LearnerKey);
                            gp.User.AssignmentUserGuidId = gp.LearnerAssignmentGuidId;

                            gpList.Add(gp);

                        }
                    }
                }
            });

            properties.AssignResults(gpList);
            return properties;
        }

        /// <summary></summary>
        /// <summary>
        /// Sets the final points for a learner assignment.
        /// </summary>
        ///
        /// <param name="learnerAssignmentGuidId">The <c>LearnerAssignmentItemIdentifier</c> of the
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
        public void SetFinalPoints(Guid learnerAssignmentGuidId, float? finalPoints)
        {
            // Security checks: Fails if the user isn't an instructor on the assignment
            // (since it tries to access the learner assignment using LearnerAssignmentListForInstructors,
            // and that returns zero rows if the user isn't an instructor on the learner assignment)

            // Check parameters
            if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
                throw new ArgumentNullException("learnerAssignmentGuidId");

            // Create a transaction
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
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
                LearnerAssignmentState status = CastNonNull<LearnerAssignmentState>(dataRow[0]);

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
        /// <param name="learnerAssignmentGuidId">The <c>LearnerAssignmentItemIdentifier</c> of the
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
        public void IncrementFinalPoints(Guid learnerAssignmentGuidId, float points)
        {
            // Security checks: Fails if the user isn't an instructor on the assignment
            // (since it tries to access the learner assignment using LearnerAssignmentListForInstructors,
            // and that returns zero rows if the user isn't an instructor on the learner assignment)

            // Check parameters
            if (learnerAssignmentGuidId.Equals(Guid.Empty) == true)
                throw new ArgumentNullException("learnerAssignmentGuidId");

            // Create a transaction
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
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
                LearnerAssignmentState status = CastNonNull<LearnerAssignmentState>(dataRow[0]);
                float? finalPoints = Cast<float>(dataRow[1]);
                LearnerAssignmentItemIdentifier learnerAssignmentId;
                LearningStoreHelper.CastNonNull(dataRow[2], out learnerAssignmentId);

                // Only valid if we are in the completed or final state
                if ((status != LearnerAssignmentState.Completed) && (status != LearnerAssignmentState.Final))
                {
                    throw new SafeToDisplayException(AppResources.LearnerAssignmentNotInCorrectStateForFinish);
                }

                // Increment final points
                if(finalPoints != null)
                {
                    points += finalPoints.Value;
                }
                    
                // We've verified that the user has the correct right, so do everything else with security checks turned off
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

        /// <summary>See <see cref="ISlkStore.SaveLearnerAssignment"/>.</summary>
        public void SaveLearnerAssignment(LearnerAssignmentItemIdentifier learnerAssignmentId, bool ignoreFinalPoints, float? finalPoints, string instructorComments, 
                            string grade, bool? isFinal, AttemptStatus? nonELearningStatus)
        {
            // Call will be wrapped within a transaction
            
            Dictionary<string, object> properties = new Dictionary<string, object>();

            if (ignoreFinalPoints == false)
            {
                properties[Schema.LearnerAssignmentItem.FinalPoints] = finalPoints;
            }

            properties[Schema.LearnerAssignmentItem.InstructorComments] = instructorComments;
            if (Settings.UseGrades)
            {
                properties[Schema.LearnerAssignmentItem.Grade] = grade;
            }

            if (isFinal != null)
            {
                properties[Schema.LearnerAssignmentItem.IsFinal] = isFinal.Value;
            }

            if (nonELearningStatus != null)
            {
                properties[Schema.LearnerAssignmentItem.NonELearningStatus] = nonELearningStatus.Value;
            }

            currentJob.Job.UpdateItem(learnerAssignmentId, properties);
            currentJob.Job.Execute();
        }

        /// <summary>See <see cref="ISlkStore.ChangeLearnerAssignmentState"/>.</summary>
        public void ChangeLearnerAssignmentState(LearnerAssignmentItemIdentifier learnerAssignmentId, bool? isFinal,
                AttemptStatus? nonELearningStatus, bool saveFinalPoints, float? finalPoints)
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(transactionOptions))
            {
                using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
                {
                    LearningStoreJob job = LearningStore.CreateJob();
                    IDictionary<String, Object> properties = new Dictionary<string, object>();

                    if (isFinal != null)
                    {
                        properties[Schema.LearnerAssignmentItem.IsFinal] = isFinal.Value;
                    }

                    if (saveFinalPoints)
                    {
                        properties[Schema.LearnerAssignmentItem.FinalPoints] = finalPoints;
                    }

                    if (nonELearningStatus != null)
                    {
                        properties[Schema.LearnerAssignmentItem.NonELearningStatus] = nonELearningStatus.Value;
                    }
                    job.UpdateItem(learnerAssignmentId, properties);
                    job.Execute();
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Finishes the learner assignment.
        /// </summary>
        ///
        /// <param name="learnerAssignmentGuidId">The ID of the learner assignment to finish.</param>
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
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
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
                    LearnerAssignmentState status = CastNonNull<LearnerAssignmentState>(dataRow[0]);
                    ActivityPackageItemIdentifier rootActivityId = CastIdentifier<ActivityPackageItemIdentifier>(dataRow[1]);
                    bool isAutoReturn = CastNonNull<bool>(dataRow[3]);
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
                    {
                        properties[Schema.LearnerAssignmentItem.IsFinal] = true;
                    }
                    properties[Schema.LearnerAssignmentItem.FinalPoints] = Cast<float?>(dataRow[2]);                
                    job.UpdateItem(learnerAssignmentId, properties);

                    // execute the LearningStore job
                    job.Execute();
                }

                // finish the transaction
                scope.Complete();
            }
        }

        /// <summary>Retrieves some information about an e-learning package. </summary>
        /// <param name="packageReader">A <c>PackageReader</c> open onto the e-learning package to retrieve information about.</param>
        /// <param name="spFile">The <c>SPFile</c> of the package to retrieve information about.</param>
        static PackageInformation GetPackageInformation(PackageReader packageReader, SPFile spFile)
        {
            PackageInformation information = new PackageInformation();
            information.SPFile = spFile;

            information.ManifestReader = new ManifestReader(packageReader, new ManifestReaderSettings(true, true));
            MetadataNodeReader metadataReader = information.ManifestReader.Metadata;

            // set <title> to the title to display for the package, using these rules:
            //   1. if there's a Title column value, use it;
            //   2. otherwise, if there's a title specified in the package metadata, use it;
            //   3. otherwise, use the file name without the extension
            if (String.IsNullOrEmpty(spFile.Title) == false)
            {
                information.Title = spFile.Title;
            }
            else 
            {
                string titleFromMetadata = metadataReader.GetTitle(CultureInfo.CurrentCulture);
                if (string.IsNullOrEmpty(titleFromMetadata) == false)
                {
                    information.Title = titleFromMetadata;
                }
                else
                {
                    information.Title = Path.GetFileNameWithoutExtension(spFile.Name);
                }
            }

            // set description to the package description specified in metadata, or null if none
            ReadOnlyCollection<string> descriptions = metadataReader.GetDescriptions(CultureInfo.CurrentCulture);
            if (descriptions.Count > 0)
            {
                information.Description = descriptions[0];
                if (information.Description == null)
                {
                    information.Description = string.Empty;
                }
            }
            else
            {
                information.Description = string.Empty;
            }

            return information;
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
            // Check parameters
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            // Security checks: Fails if user doesn't have access to the package (implemented
            // by GetFileFromSharePointLocation and SharePointPackageReader)

            FileAndLocation fileLocation = GetFileFromSharePointLocation(location);
            SPFile spFile = fileLocation.File;

            using(PackageReader packageReader = new SharePointPackageReader(SharePointCacheSettings, fileLocation.Location, false))
            {
                return SlkStore.GetPackageInformation(packageReader, spFile);
            }
        }

        /// <summary>See <see cref="ISlkStore.GetPackageInformation"/>.</summary>
        public PackageInformation GetPackageInformation(PackageItemIdentifier packageId, SPFile file)
        {
            using(LearningStorePrivilegedScope privilegedScope = new LearningStorePrivilegedScope())
            {
                using (PackageReader packageReader = PackageStore.GetPackageReader(packageId))
                {
                    // set <manifestReader> and <metadataReader> to refer to the package; set <title> and
                    // <description> to the package-level title and description
                    return GetPackageInformation(packageReader, file);
                }
            }
        }

        /// <summary>See <see cref="ISlkStore.FetchUserWebList"/>.</summary>
        public ReadOnlyCollection<SlkUserWebListItem> FetchUserWebList()
        {
            // Security checks: None
            
            LearningStoreJob job = LearningStore.CreateJob();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.UserWebList.ViewName);
            query.AddColumn(Schema.UserWebList.SPSiteGuid);
            query.AddColumn(Schema.UserWebList.SPWebGuid);
            query.AddColumn(Schema.UserWebList.LastAccessTime);
            query.AddSort(Schema.UserWebList.LastAccessTime, LearningStoreSortDirection.Descending);
            job.PerformQuery(query);
            DataRowCollection dataRows = job.Execute<DataTable>().Rows;
            List<SlkUserWebListItem> userWebList = new List<SlkUserWebListItem>(dataRows.Count);
            foreach (DataRow dataRow in dataRows)
            {
                Guid spSiteGuid = CastNonNull<Guid>(dataRow[0]);
                Guid spWebGuid = CastNonNull<Guid>(dataRow[1]);
                DateTime lastAccessTime = CastNonNull<DateTime>(dataRow[2]);
                userWebList.Add(new SlkUserWebListItem(spSiteGuid, spWebGuid, lastAccessTime));
            }

            return new ReadOnlyCollection<SlkUserWebListItem>(userWebList);
        }

        /// <summary>
        /// Adds a given <c>SPWeb</c> to the current user's SharePoint Learning Kit user web list.
        /// If the <c>SPWeb</c> already exists in the user web list, it's last-access time is updated
        /// to be the current time.
        /// </summary>
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
        /// <param name="spWeb">The <c>SPWeb</c> to add.</param>
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
        /// Adds to an existing LearningStore job request(s) to retrieve general information about an
        /// assignment.
        /// </summary>
        ///
        /// <param name="job">The <c>LearningStoreJob</c> add request(s) to.</param>
        ///
        /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to
        ///     retrieve information about.</param>
        ///
        /// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
        ///
        /// <remarks>
        /// <para>
        /// After executing the LearningStore job, call <c>EndGetAssignmentProperties</c> to retrieve
        /// from the results information requested by this method.
        /// </para>
        /// </remarks>
        ///
        private void BeginGetAssignmentProperties(LearningStoreJob job, AssignmentItemIdentifier assignmentId, SlkRole slkRole)
        {
            // Security checks: None (since it doesn't call the database or
            // SharePoint)
                    
            // request basic information about the specified assignment
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.AssignmentPropertiesView.ViewName);
            query.SetParameter(Schema.AssignmentPropertiesView.AssignmentId, assignmentId);
            query.SetParameter(Schema.AssignmentPropertiesView.IsInstructor, (slkRole == SlkRole.Instructor));
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentSPSiteGuid);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentSPWebGuid);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentTitle);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentStartDate);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDueDate);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentPointsPossible);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDescription);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentAutoReturn);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentEmailChanges);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentCreatedById);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentDateCreated);
            query.AddColumn(Schema.AssignmentPropertiesView.PackageFormat);
            query.AddColumn(Schema.AssignmentPropertiesView.PackageLocation);
            query.AddColumn(Schema.AssignmentPropertiesView.AssignmentNonELearningLocation);
            query.AddColumn(Schema.AssignmentPropertiesView.RootActivityId);
            job.PerformQuery(query);

            // request the collection of instructors of this assignment
            AddInstructorsQuery(job, assignmentId);

            if (slkRole == SlkRole.Instructor)
            {
                // request the collection of learners of this assignment
                query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
                query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerId);
                query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerName);
                query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerKey);
                query.AddColumn(Schema.LearnerAssignmentListForInstructors.LearnerAssignmentId);
                query.AddColumn(Schema.UserItemSite.SPUserId);
                query.AddCondition(Schema.LearnerAssignmentListForInstructors.AssignmentId, LearningStoreConditionOperator.Equal, assignmentId);
                job.PerformQuery(query);
            }
        }

        /// <summary>Populates an AssignmentProperties object.</summary>
        /// <remarks>
        /// <para>
        /// If <paramref name="slkRole"/> is <c>SlkRole.Learner</c>, then <c>Instructors</c> and
        /// <c>Learners</c> in the returned <c>AssignmentProperties</c> object will be <c>null</c>,
        /// since learners are not permitted to retrieve that information.
        /// </para>
        /// </remarks>
        /// <param name="resultEnumerator">An enumerator that contains the data.</param>
        /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment.</param>
        /// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
        /// <returns></returns>
        AssignmentProperties PopulateAssignmentProperties(IEnumerator<object> resultEnumerator, AssignmentItemIdentifier assignmentId, SlkRole slkRole)
        {
            if (!resultEnumerator.MoveNext())
            {
                throw new InternalErrorException("SLK1008");
            }
            DataRowCollection dataRows = ((DataTable)resultEnumerator.Current).Rows;
            if (dataRows.Count != 1)
            {
                // this error message includes the assignment ID, but that's okay since
                // the information we provide does not allow the user to distinguish between the
                // assignment not existing and the user not having access to it
                throw new SafeToDisplayException(String.Format(CultureInfo.CurrentCulture, AppResources.AssignmentNotFoundInDatabase, assignmentId.GetKey()));
            }
            DataRow dataRow = dataRows[0];
            AssignmentProperties ap = new AssignmentProperties(assignmentId, this);

            // copy information from <dataRow> into properties of <ap>...

            ap.SPSiteGuid = CastNonNull<Guid>(dataRow[Schema.AssignmentPropertiesView.AssignmentSPSiteGuid]);

            ap.SPWebGuid = CastNonNull<Guid>(dataRow[Schema.AssignmentPropertiesView.AssignmentSPWebGuid]);

            ap.Title = CastNonNull<string>(dataRow[Schema.AssignmentPropertiesView.AssignmentTitle]);

            ap.StartDate = ToUtcTime(dataRow[Schema.AssignmentPropertiesView.AssignmentStartDate]);

            ap.DueDate = ToNullableUtcTime(dataRow[Schema.AssignmentPropertiesView.AssignmentDueDate]);
            ap.RootActivityId = CastIdentifier<ActivityPackageItemIdentifier>(dataRow[LearnerAssignmentList.RootActivityId]);

            ap.PointsPossible = Cast<float?>(dataRow[Schema.AssignmentPropertiesView.AssignmentPointsPossible]);

            ap.Description = CastNonNull<string>(dataRow[Schema.AssignmentPropertiesView.AssignmentDescription]);

            ap.AutoReturn = CastNonNull<bool>(dataRow[Schema.AssignmentPropertiesView.AssignmentAutoReturn]);
            ap.EmailChanges = CastNonNull<bool>(dataRow[Schema.AssignmentPropertiesView.AssignmentEmailChanges]);

            ap.ShowAnswersToLearners = CastNonNull<bool>(dataRow[Schema.AssignmentPropertiesView.AssignmentShowAnswersToLearners]);

            ap.CreatedById = CastNonNullIdentifier<UserItemIdentifier>(dataRow[Schema.AssignmentPropertiesView.AssignmentCreatedById]);
            ap.DateCreated = ToUtcTime(dataRow[Schema.AssignmentPropertiesView.AssignmentDateCreated]);

            ap.PackageFormat = CastNullableEnum<PackageFormat>(dataRow[Schema.AssignmentPropertiesView.PackageFormat]);

            if (ap.PackageFormat != null)
            {
                // e-learning package
                ap.Location = CastNonNull<string>(dataRow[Schema.AssignmentPropertiesView.PackageLocation]);
            }
            else
            {
                // non-e-learning document
                ap.Location = CastNonNull<string>(dataRow[Schema.AssignmentPropertiesView.AssignmentNonELearningLocation]);
            }

            using (SPSite site = new SPSite(SPSiteGuid))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    AddSlkUsers(web, resultEnumerator, ap.Instructors, InstructorAssignmentList.InstructorId, InstructorAssignmentList.InstructorName, InstructorAssignmentList.InstructorKey, 
                            InstructorAssignmentList.InstructorAssignmentId, "SLK1011");
                    if (slkRole == SlkRole.Instructor)
                    {
                        AddSlkUsers(web, resultEnumerator, ap.Learners, LearnerAssignmentListForInstructors.LearnerId, LearnerAssignmentListForInstructors.LearnerName, LearnerAssignmentListForInstructors.LearnerKey, 
                                LearnerAssignmentListForInstructors.LearnerAssignmentId, "SLK1011");
                    }
                }
            }

            return ap;
        }

        void AddSlkUsers(SPWeb web, IEnumerator<object> resultEnumerator, SlkUserCollection users, string idColumn, string nameColumn, string keyColumn, string assignmentIdColumn, string errorNumberIfMissing)
        {
            if (!resultEnumerator.MoveNext())
            {
                throw new InternalErrorException(errorNumberIfMissing);
            }
            DataRowCollection dataRows = ((DataTable)resultEnumerator.Current).Rows;
            foreach (DataRow dataRow in dataRows)
            {
                SlkUser user = LoadUser(web, dataRow, idColumn, nameColumn, keyColumn);
                user.AssignmentUserId = CastNonNullIdentifier<LearnerAssignmentItemIdentifier>(dataRow[assignmentIdColumn]);
                users.Add(user);
            }
        }

        SlkUser LoadUser(SPWeb web, DataRow dataRow, string idColumn, string nameColumn, string keyColumn)
        {
            return LoadUser(web, dataRow, idColumn, nameColumn, keyColumn, Guid.Empty);
        }

        SlkUser LoadUser(SPWeb web, DataRow dataRow, string idColumn, string nameColumn, string keyColumn, Guid learnerAssignmentId)
        {
            UserItemIdentifier userId = CastNonNullIdentifier<UserItemIdentifier>(dataRow[idColumn]);
            string userName = CastNonNull<string>(dataRow[nameColumn]);
            string key = CastNonNull<string>(dataRow[keyColumn]);
            int spUserId = Cast<int>(dataRow[UserItemSite.SPUserId]);
            SPUser spUser = null;

            if (spUserId != 0)
            {
                try
                {
                    spUser = web.SiteUsers.GetByID(spUserId);
                }
                catch (SPException)
                {
                    // user no longer present. Will be fixed next time assignment properties page is opened
                }
            }

            SlkUser user = new SlkUser(userId, userName, key, spUser);
            user.AssignmentUserGuidId = learnerAssignmentId;
            return user;
        }

        LearningStoreQuery CreateQueryForLearnerAssignmentProperties(Guid learnerAssignmentGuidId, SlkRole role)
        {
            LearningStoreQuery query;

            switch (role)
            {
                case SlkRole.Instructor:
                    query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForInstructors.ViewName);
                    break;
                case SlkRole.Observer:
                    query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForObservers.ViewName);
                    break;
                default:
                    query = LearningStore.CreateQuery(Schema.LearnerAssignmentListForLearners.ViewName);
                    break;
            }

            query.AddColumn(Schema.LearnerAssignmentList.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentId);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentSPSiteGuid);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentSPWebGuid);
            query.AddColumn(Schema.LearnerAssignmentList.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentList.PackageLocation);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentNonELearningLocation);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentTitle);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentDescription);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentPointsPossible);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentStartDate);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentDueDate);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentAutoReturn);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentEmailChanges);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentCreatedById);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentCreatedByName);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerKey);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerName);
            query.AddColumn(Schema.UserItemSite.SPUserId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentList.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentList.Grade);
            query.AddColumn(Schema.LearnerAssignmentList.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentList.HasInstructors);
            query.AddCondition(Schema.LearnerAssignmentList.LearnerAssignmentGuidId, LearningStoreConditionOperator.Equal, learnerAssignmentGuidId);
            return query;
        }

        void DemandRight(LearningStoreJob job, string right, Guid learnerAssignmentGuidId)
        {
            Dictionary<string,object> securityParameters = new Dictionary<string,object>();
            securityParameters.Add(Schema.ActivateLearnerAssignmentRight.LearnerAssignmentGuidId, learnerAssignmentGuidId);
            job.DemandRight(right, securityParameters);
        }

        struct FileAndLocation
        {
            public SPFile File;
            public SharePointFileLocation Location;
        }

        void AddInstructorsQuery(LearningStoreJob job, AssignmentItemIdentifier assignmentId)
        {
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.InstructorAssignmentList.ViewName);
            query.AddColumn(Schema.InstructorAssignmentList.InstructorId);
            query.AddColumn(Schema.InstructorAssignmentList.InstructorName);
            query.AddColumn(Schema.InstructorAssignmentList.InstructorKey);
            query.AddColumn(Schema.InstructorAssignmentList.InstructorAssignmentId);
            query.AddColumn(Schema.UserItemSite.SPUserId);
            query.AddCondition(Schema.InstructorAssignmentList.AssignmentId, LearningStoreConditionOperator.Equal, assignmentId);
            job.PerformQuery(query);
        }

#region conversion methods
        static DateTime ToUtcTime(object value)
        {
            DateTime castValue = Cast<DateTime>(value);
            return DateTime.SpecifyKind(castValue, DateTimeKind.Utc);
        }

        static DateTime? ToNullableUtcTime(object value)
        {
            DateTime? castValue = Cast<DateTime?>(value);
            if (castValue == null)
            {
                return null;
            }
            else
            {
                return DateTime.SpecifyKind(castValue.Value, DateTimeKind.Utc);
            }
        }

        static T Cast<T>(object value, T defaultValue)
        {
            if (value is DBNull)
            {
                return defaultValue;
            }
            else
            {
                return (T) value;
            }
        }

        /// <summary>Converts a value returned from a LearningStore query to a given type, or <c>null</c> if the value is <c>DBNull</c>.</summary>
        /// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c> returned from a LearningStore query.</param>
        static T? CastNullableEnum<T>(object value) where T:struct
        {
            if (value is DBNull)
            {
                return null;
            }
            else
            {
                return new T?((T)value);
            }
        }

        /// <summary>Converts a value returned from a LearningStore query to a given type, or <c>null</c> if the value is <c>DBNull</c>.</summary>
        /// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c> returned from a LearningStore query.</param>
        static T Cast<T>(object value)
        {
            if (value is DBNull)
            {
                return default(T);
            }
            else
            {
                return (T)value;
            }
        }

        /// <summary>Converts a value returned from a LearningStore query to a given type.  Throws an exception if the value is <c>DBNull</c>.</summary>
        /// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c> returned from a LearningStore query.</param>
        static T CastNonNull<T>(object value)
        {
            if (value is DBNull)
            {
                throw new ArgumentException(AppResources.UnexpectedDBNull);
            }
            else
            {
                return (T)value;
            }
        }

        /// <summary>
        /// Converts a value returned from a LearningStore query to a <c>UserItemIdentifier</c>,
        /// or <c>null</c> if the value is <c>DBNull</c>.
        /// </summary>
        ///
        /// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
        ///     returned from a LearningStore query.</param>
        static T CastIdentifier<T>(object value) where T : LearningStoreItemIdentifier, new()
        {
            LearningStoreItemIdentifier id;
            LearningStoreHelper.Cast(value, out id);
            if (id == null)
            {
                return null;
            }
            else
            {
                T result = new T();
                result.AssignIdentifier(id);
                return result;
            }
        }

        /// <summary>
        /// Converts a value returned from a LearningStore query to a
        /// <c>UserItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
        /// </summary>
        ///
        /// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
        ///     returned from a LearningStore query.</param>
        static T CastNonNullIdentifier<T>(object value) where T : LearningStoreItemIdentifier, new()
        {
            LearningStoreItemIdentifier id;
            LearningStoreHelper.CastNonNull(value, out id);
            T result = new T();
            result.AssignIdentifier(id);
            return result;
        }
#endregion conversion methods

#region private methods
        AssignmentProperties PopulateAssignmentProperties(AssignmentItemIdentifier id, DataRow dataRow)
        {
            AssignmentProperties properties = new AssignmentProperties(id, this);
            properties.SPSiteGuid = CastNonNull<Guid>(dataRow[LearnerAssignmentList.AssignmentSPSiteGuid]);
            properties.SPWebGuid = CastNonNull<Guid>(dataRow[LearnerAssignmentList.AssignmentSPWebGuid]);
            properties.RootActivityId = CastIdentifier<ActivityPackageItemIdentifier>(dataRow[LearnerAssignmentList.RootActivityId]);

            if (properties.IsNonELearning)
            {
                properties.Location = CastNonNull<string>(dataRow[LearnerAssignmentList.AssignmentNonELearningLocation]);
            }
            else
            {
                properties.Location = CastNonNull<string>(dataRow[LearnerAssignmentList.PackageLocation]);
            }

            properties.Title = CastNonNull<string>(dataRow[LearnerAssignmentList.AssignmentTitle]);
            properties.Description = CastNonNull<string>(dataRow[LearnerAssignmentList.AssignmentDescription]);
            properties.PointsPossible = Cast<float?>(dataRow[LearnerAssignmentList.AssignmentPointsPossible]);
            properties.StartDate = ToUtcTime(dataRow[LearnerAssignmentList.AssignmentStartDate]);
            properties.DueDate = ToNullableUtcTime(dataRow[LearnerAssignmentList.AssignmentDueDate]);
            properties.AutoReturn = CastNonNull<bool>(dataRow[LearnerAssignmentList.AssignmentAutoReturn]);
            properties.ShowAnswersToLearners = CastNonNull<bool>(dataRow[LearnerAssignmentList.AssignmentShowAnswersToLearners]);
            properties.CreatedById = CastNonNullIdentifier<UserItemIdentifier>(dataRow[LearnerAssignmentList.AssignmentCreatedById]);
            properties.CreatedByName = CastNonNull<string>(dataRow[LearnerAssignmentList.AssignmentCreatedByName]);
            properties.HasInstructors = CastNonNull<bool>(dataRow[LearnerAssignmentList.HasInstructors]);
            properties.EmailChanges = CastNonNull<bool>(dataRow[LearnerAssignmentList.AssignmentEmailChanges]);
            return properties;
        }

        static LearnerAssignmentProperties PopulateLearnerAssignmentProperties(DataRow dataRow, AssignmentProperties properties, Guid learnerAssignmentGuidId)
        {
            LearnerAssignmentItemIdentifier learnerId = CastNonNullIdentifier<LearnerAssignmentItemIdentifier>(dataRow[LearnerAssignmentList.LearnerAssignmentId]);
            LearnerAssignmentProperties learnerProperties = new LearnerAssignmentProperties(learnerId, properties);
            learnerProperties.LearnerAssignmentGuidId = learnerAssignmentGuidId;
            learnerProperties.LearnerId = CastNonNullIdentifier<UserItemIdentifier>(dataRow[LearnerAssignmentList.LearnerId]);
            learnerProperties.LearnerName = CastNonNull<string>(dataRow[LearnerAssignmentList.LearnerName]);
            learnerProperties.Status = CastNonNull<LearnerAssignmentState>(dataRow[LearnerAssignmentList.LearnerAssignmentState]);
            learnerProperties.AttemptId = CastIdentifier<AttemptItemIdentifier>(dataRow[LearnerAssignmentList.AttemptId]);
            learnerProperties.CompletionStatus = Cast<CompletionStatus>(dataRow[LearnerAssignmentList.AttemptCompletionStatus]);
            learnerProperties.SuccessStatus = Cast<SuccessStatus>(dataRow[LearnerAssignmentList.AttemptSuccessStatus]);
            learnerProperties.GradedPoints = Cast<float?>(dataRow[LearnerAssignmentList.AttemptGradedPoints]);
            learnerProperties.FinalPoints = Cast<float?>(dataRow[LearnerAssignmentList.FinalPoints]);
            learnerProperties.Grade = Cast<string>(dataRow[LearnerAssignmentList.Grade]);
            learnerProperties.InstructorComments = CastNonNull<string>(dataRow[LearnerAssignmentList.InstructorComments]);
            return learnerProperties;
        }

        LearningStoreQuery ReminderEmailQuery(DateTime minDueDate, DateTime maxDueDate)
        {
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.LearnerAssignmentList.BaseViewName);

            query.AddColumn(Schema.LearnerAssignmentList.AssignmentId);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentSPSiteGuid);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentSPWebGuid);
            query.AddColumn(Schema.LearnerAssignmentList.RootActivityId);
            query.AddColumn(Schema.LearnerAssignmentList.PackageLocation);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentNonELearningLocation);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentTitle);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentDescription);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentPointsPossible);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentStartDate);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentDueDate);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentAutoReturn);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentEmailChanges);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentShowAnswersToLearners);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentCreatedById);
            query.AddColumn(Schema.LearnerAssignmentList.AssignmentCreatedByName);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerKey);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerName);
            query.AddColumn(Schema.UserItemSite.SPUserId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerAssignmentId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerAssignmentGuidId);
            query.AddColumn(Schema.LearnerAssignmentList.LearnerAssignmentState);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptId);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptCompletionStatus);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptSuccessStatus);
            query.AddColumn(Schema.LearnerAssignmentList.AttemptGradedPoints);
            query.AddColumn(Schema.LearnerAssignmentList.FinalPoints);
            query.AddColumn(Schema.LearnerAssignmentList.Grade);
            query.AddColumn(Schema.LearnerAssignmentList.InstructorComments);
            query.AddColumn(Schema.LearnerAssignmentList.HasInstructors);

            query.AddCondition(Schema.LearnerAssignmentList.AssignmentEmailChanges, LearningStoreConditionOperator.Equal, 1);
            query.AddCondition(Schema.LearnerAssignmentList.IsFinal, LearningStoreConditionOperator.NotEqual, 1);
            query.AddCondition(Schema.LearnerAssignmentList.LearnerAssignmentState, LearningStoreConditionOperator.LessThan, 2);
            query.AddCondition(Schema.LearnerAssignmentList.AssignmentDueDate, LearningStoreConditionOperator.NotEqual, null);
            //query.AddCondition(Schema.LearnerAssignmentList.AssignmentDueDate, LearningStoreConditionOperator.LessThan, maxDueDate);
            query.AddCondition(Schema.LearnerAssignmentList.AssignmentDueDate, LearningStoreConditionOperator.GreaterThan, minDueDate);
            query.AddCondition(Schema.LearnerAssignmentList.AssignmentSPSiteGuid, LearningStoreConditionOperator.Equal, SPSiteGuid);

            query.AddSort(LearnerAssignmentList.AssignmentSPSiteGuid, LearningStoreSortDirection.Ascending);
            query.AddSort(LearnerAssignmentList.AssignmentSPWebGuid, LearningStoreSortDirection.Ascending);
            query.AddSort(LearnerAssignmentList.AssignmentId, LearningStoreSortDirection.Ascending);

            return query;
        }
#endregion private methods

#region CurrentJob
        class CurrentJob : IDisposable
        {
            LearningStoreTransactionScope currentTransaction;
            LearningStorePrivilegedScope privilegedScope;

            public LearningStoreJob Job { get; private set; }

            public CurrentJob(LearningStore learningStore)
            {
                Job = learningStore.CreateJob();
                TransactionOptions options = new TransactionOptions();
                options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                currentTransaction = new LearningStoreTransactionScope(options);
                privilegedScope = new LearningStorePrivilegedScope();
            }

            public void Complete()
            {
                currentTransaction.Complete();
                Job.Execute();
            }

            public void Cancel()
            {
            }

            public void Dispose()
            {

                if (privilegedScope != null)
                {
                    privilegedScope.Dispose();
                }

                if (currentTransaction != null)
                {
                    currentTransaction.Dispose();
                }
            }
        }
#endregion CurrentJob
    }
}
