using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

namespace CourseManagerConfCs
{
    public class CourseManagerMapping : SPPersistedObject
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


        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>
        /// Initializes an instance of this class.  This constructor is only used for deserialization
        /// and should not be used directly by applications.
        /// </summary>
        ///
        public CourseManagerMapping()
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
        public static CourseManagerMapping GetMapping(Guid spSiteGuid)
        {
            CourseManagerMapping mapping;
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
        public static bool GetMapping(Guid spSiteGuid, out CourseManagerMapping mapping)
        {
            // set <adminWebService> to an instance of the SharePoint Central Administration web
            // service; set <mappingCollection> to the collection of SlkSPSiteMapping objects stored in
            // SharePoint's configuration database
            SPWebService adminWebService;
            CourseManagerMappingCollection mappingCollection;
            GetMappingInfo(out adminWebService, out mappingCollection);

            // set <mapping> to the SlkSPSiteMapping corresponding to <spSiteGuid>
            string persistedObjectName = String.Format(CultureInfo.InvariantCulture, PersistedObjectNameFormat, spSiteGuid);
            mapping = mappingCollection.GetValue<CourseManagerMapping>(persistedObjectName);

            // return the mapping if found; if not found, create a new mapping
            if (mapping != null)
            {
                return true;
            }
            else
            {
                mapping = new CourseManagerMapping(persistedObjectName, adminWebService, spSiteGuid);
                return false;
            }
        }

        /// <summary>
        /// Returns the collection of <c>SlkSPSiteMapping</c> objects stored in SharePoint's
        /// configuration database.
        /// </summary>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static CourseManagerMappingCollection GetMappings()
        {
            // set <adminWebService> to an instance of the SharePoint Central Administration web
            // service; set <mappingCollection> to the collection of SlkSPSiteMapping objects stored in
            // SharePoint's configuration database
            SPWebService adminWebService;
            CourseManagerMappingCollection mappingCollection;
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
        CourseManagerMapping(string persistedObjectName, SPWebService adminWebService, Guid spSiteGuid)
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
            out CourseManagerMappingCollection mappingCollection)
        {
            adminWebService = CourseManagerAdministrator.GetAdminWebService();
            mappingCollection = new CourseManagerMappingCollection(adminWebService);
        }
    }

    /// <summary>
    /// Represents a collection of <c>SlkSPSiteMapping</c> objects.
    /// </summary>
    ///
    public class CourseManagerMappingCollection :
        SPPersistedChildCollection<SlkSPSiteMapping>
    {
        internal CourseManagerMappingCollection(SPWebService webService)
            : base(webService)
        {
        }
    }

}
