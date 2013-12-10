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

    /// <summary>
    /// Represents a mapping from an <c>SPSite</c> to a SharePoint Learning Kit LearningStore database.
    /// </summary>
    ///
    public class SlkSPSiteMapping : SPPersistedObject
    {

        /// <summary>
        /// The format string used to construct the name of this object as persisted in SharePoint's
        /// configuration database.
        /// </summary>
        const string PersistedObjectNameFormat = "SharePointLearningKitMapping_{0}";

#region fields
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

        /// <summary>
        /// Holds the value of the <c>ObserverPermission</c> property.
        /// </summary>
        [Persisted]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_observerPermission;

        /// <summary>
        /// Holds the value of the <c>DropBoxFilesExtensions</c> property.
        /// </summary>
        [Persisted]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_dropBoxFilesExtensions;
#endregion fields

#region properties
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
                if (m_databaseServer != value)
                {
                    m_databaseServer = value;
                    IsDirty = true;
                }
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
                if (m_databaseName != value)
                {
                    m_databaseName = value;
                    IsDirty = true;
                }
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
                if (m_instructorPermission != value)
                {
                    m_instructorPermission = value;
                    IsDirty = true;
                }
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
                if (m_learnerPermission != value)
                {
                    m_learnerPermission = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>Shows if the object is changed or not.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Sets or gets the name of SharePoint permission used to identify observers.
        /// </summary>
        public string ObserverPermission
        {
            [DebuggerStepThrough]
            get
            {
                return m_observerPermission;
            }
            [DebuggerStepThrough]
            set
            {
                if (m_observerPermission != value)
                {
                    m_observerPermission = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Sets or gets the allowed extensions for DropBox uploaded files.
        /// </summary>
        string DropBoxFilesExtensions
        {
            [DebuggerStepThrough]
            get
            {
                return m_dropBoxFilesExtensions;
            }
            [DebuggerStepThrough]
            set
            {
                m_dropBoxFilesExtensions = value;
            }
        }
#endregion properties

#region constructors
        /// <summary>
        /// Initializes an instance of this class.  This constructor is only used for deserialization
        /// and should not be used directly by applications.
        /// </summary>
        ///
        public SlkSPSiteMapping()
        {
        }
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="persistedObjectName">The name of this object as it will be persisted in
        ///     SharePoint's configuration database.</param>
        ///
        /// <param name="adminWebService">An instance of the SharePoint Central Administration web
        ///     service.</param>
        ///
        /// <param name="spSiteGuid">The GUID of the SPSite represented by this
        ///     SPSite-to-LearningStore mapping.</param>
        ///
        SlkSPSiteMapping(string persistedObjectName, SPWebService adminWebService, Guid spSiteGuid)
            : base(persistedObjectName, adminWebService)
        {
            m_spSiteGuid = spSiteGuid;
            IsDirty = true;
        }
#endregion constructors

        /// <summary>
        /// Returns the SPSite-to-LearningStore mapping represented by a given SPSite GUID.  If no
        /// such mapping exists, an exception is thrown.
        /// </summary>
        ///
        /// <param name="site">The SPSite to retrieve a mapping for.</param>
        ///
        /// <exception cref="SlkNotConfiguredException">
        /// SLK is not configured for SharePoint site collection.
        /// (This configuration is performed in SharePoint Central Administration.)
        /// </exception>
        ///
        public static SlkSPSiteMapping GetRequiredMapping(SPSite site)
        {
            SlkSPSiteMapping mapping = GetMapping(site.ID);

            if (mapping == null)
            {
                mapping = GetMapping(site.WebApplication.Id);
            }

            if (mapping == null)
            {
                throw new SlkNotConfiguredException(SlkCulture.GetResources().SlkNotEnabled);
            }
            else
            {
                return mapping;
            }
        }

        /// <summary>
        /// Retrieves the SPSite-to-LearningStore mapping represented by a given SPSite GUID.  If no
        /// such mapping exists, a new mapping object is created -- in that case, the caller should
        /// set the properties of the <c>SlkSPSiteMapping</c> object to their correct values and then
        /// call <c>Update</c> to store the mapping into the SharePoint configuration database.
        /// </summary>
        /// <param name="spSiteGuid">The GUID of the SPSite to retrieve a mapping for.</param>
        /// <returns>
        /// <c>true</c> if an existing mapping was found; <c>false</c> if a new mapping is created.
        /// In the latter case, the new mapping is not stored in the SharePoint configuration
        /// database until <c>Update</c> is called.
        /// </returns>
        ///
        public static SlkSPSiteMapping GetMapping(Guid spSiteGuid)
        {
            SlkSPSiteMappingCollection mappingCollection = GetMappingInfo();

            // set <mapping> to the SlkSPSiteMapping corresponding to <spSiteGuid>
            string persistedObjectName = String.Format(CultureInfo.InvariantCulture, PersistedObjectNameFormat, spSiteGuid);
            SlkSPSiteMapping mapping = mappingCollection.GetValue<SlkSPSiteMapping>(persistedObjectName);
            return mapping;
        }

        /// <summary>Creates a new mapping.</summary>
        /// <param name="spSiteGuid">The site the mapping is for.</param>
        public static SlkSPSiteMapping CreateMapping(Guid spSiteGuid)
        {
            string persistedObjectName = String.Format(CultureInfo.InvariantCulture, PersistedObjectNameFormat, spSiteGuid);
            return new SlkSPSiteMapping(persistedObjectName, SlkAdministration.GetAdminWebService(), spSiteGuid);
        }

        /// <summary>
        /// Returns the collection of <c>SlkSPSiteMapping</c> objects stored in SharePoint's
        /// configuration database.
        /// </summary>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static SlkSPSiteMappingCollection GetMappings()
        {
            // return the collection of mappings
            return GetMappingInfo();
        }


        /// <summary>Retrieves various pieces of information related to SharePoint Central Administration and
        /// the SPSite-to-LearningStore mappings. </summary>
        /// <returns>Set to the collection of <c>SlkSPSiteMapping</c> objects
        ///     stored in SharePoint's configuration database.</returns>
        static SlkSPSiteMappingCollection GetMappingInfo()
        {
            return GetMappingInfo(null);
        }

        /// <summary>Retrieves various pieces of information related to SharePoint Central Administration and
        /// the SPSite-to-LearningStore mappings. </summary>
        /// <param name="adminWebService">The SharePoint Central Administrationt web service.</param>
        /// <returns>Set to the collection of <c>SlkSPSiteMapping</c> objects
        ///     stored in SharePoint's configuration database.</returns>
        static SlkSPSiteMappingCollection GetMappingInfo(SPWebService adminWebService)
        {
            if (adminWebService == null)
            {
                adminWebService = SlkAdministration.GetAdminWebService();
            }
            return new SlkSPSiteMappingCollection(adminWebService);
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

