/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Transactions;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;
using System.Globalization;

#endregion

/*
 * This file contains the LearningStore class
 * 
 * Internal error numbers: 1400-1599
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a store
    /// </summary>
    /// <remarks>
    /// A <b>store</b> is a SQL Server 2005 database that contains learning-related
    /// data.  A store contains a set of items.  Each item maintains a set of properties.
    /// Any number of items may be stored in a store.
    /// <p/>
    /// <b>Manipulating the items in a store</b>:<ol>
    /// <li>Open the store by creating a new <Typ>LearningStore</Typ> instance.</li>
    /// <li>Call the <Mth>CreateJob</Mth> method to create a new job.</li>
    /// <li>Call methods on the returned <Typ>LearningStoreJob</Typ> to perform
    /// the necessary manipulations.  See the <Typ>LearningStoreJob</Typ>
    /// documentation for more information.</li>
    /// </ol>
    /// </remarks>
    public class LearningStore
    {
        /// <summary>
        /// Cache of schema information for all stores in the AppDomain
        /// </summary>
        /// <remarks>
        /// The key is the connection string.  The value is a <Typ>LearningStoreSchema</Typ>
        /// that represents the schema.<p/>
        /// Before accessing, users must lock on <Fld>s_allSchemasLock</Fld>
        /// </remarks>
        private static Dictionary<string, LearningStoreSchema> s_allSchemas =
            new Dictionary<string, LearningStoreSchema>(StringComparer.OrdinalIgnoreCase);
            
        /// <summary>
        /// Lock for <Fld>s_allSchemas</Fld>
        /// </summary>
        private static object s_allSchemasLock = new object();

        /// <summary>
        /// Connection string that was passed into the constructor
        /// </summary>
        private string m_connectionString;

        /// <summary>
        /// Key of the user accessing LearningStore
        /// </summary>
        private string m_userKey;

        /// <summary>
        /// Impersonation behavior
        /// </summary>
        private ImpersonationBehavior m_impersonationBehavior;
                
        /// <summary>
        /// Should security checks be skipped?
        /// </summary>
        private bool m_disableSecurityChecks;
        
        /// <summary>
        /// Locale used for conversions to/from strings
        /// </summary>
        private CultureInfo m_locale;

        /// <summary>
        /// Schema for this store.
        /// </summary>
        /// <remarks>
        /// Loaded only when needed.
        /// </remarks>
        private LearningStoreSchema m_schema;

        /// <summary>
        /// Location to which the debug log should be written.
        /// </summary>
        /// <remarks>
        /// The TextWriter is not "owned" by this object.  It needs to be
        /// closed/disposed by the user that sets the property.
        /// </remarks>
        private TextWriter m_debugLog;

        /// <summary>
        /// Version number of the LearningStore engine
        /// </summary>
        internal const int EngineVersion = 1;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStore</Typ> class.
        /// </summary>
        /// <param name="connectionString">The connection used to access the SQL server
        ///     database. The string is the same format as a connection string passed to
        ///     <Typ>/System.Data.SqlClient.SqlConnection</Typ></param>
        /// <param name="userKey">The unique key of the user accessing the database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> or <paramref name="userKey"/> is a null reference.</exception>
        /// <example>The following code opens the store located in the "Mls"
        /// database of the "Learning" server with the user key "Bob":
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// </code>
        /// </example>
        public LearningStore(string connectionString, string userKey): this(connectionString, userKey, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStore</Typ> class.
        /// </summary>
        /// <param name="connectionString">The connection used to access the SQL server
        ///     database. The string is the same format as a connection string passed to
        ///     <Typ>/System.Data.SqlClient.SqlConnection</Typ></param>
        /// <param name="userKey">The unique key of the user accessing the database.</param>
        /// <param name="impersonationBehavior">Identifies which <c>WindowsIdentity</c> is used to
        ///     access the database when impersonation is involved.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> or <paramref name="userKey"/>
        ///     is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="impersonationBehavior"/>
        ///     has an invalid value</exception>
        /// <example>The following code opens the store located in the "Mls"
        /// database of the "Learning" server with the user key "Bob".  The original (non-impersonated)
        /// identity is used when accessing the database:
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob", ImpersonationBehavior.UseOriginalIdentity);
        /// </code>
        /// </example>
        public LearningStore(string connectionString, string userKey, ImpersonationBehavior impersonationBehavior):
            this(connectionString, userKey, impersonationBehavior, false)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStore</Typ> class.
        /// </summary>
        /// <param name="connectionString">The connection used to access the SQL server
        ///     database. The string is the same format as a connection string passed to
        ///     <Typ>/System.Data.SqlClient.SqlConnection</Typ></param>
        /// <param name="userKey">The unique key of the user accessing the database.</param>
        /// <param name="disableSecurityChecks">True if security checks should be skipped when performing operations
        ///     in the database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> or <paramref name="userKey"/> is a null reference.</exception>
        /// <example>The following code opens the store located in the "Mls"
        /// database of the "Learning" server with the user key "Bob":
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob", false);
        /// </code>
        /// </example>
        public LearningStore(string connectionString, string userKey, bool disableSecurityChecks):
            this(connectionString, userKey, ImpersonationBehavior.UseOriginalIdentity, disableSecurityChecks)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStore</Typ> class.
        /// </summary>
        /// <param name="connectionString">The connection used to access the SQL server
        ///     database. The string is the same format as a connection string passed to
        ///     <Typ>/System.Data.SqlClient.SqlConnection</Typ></param>
        /// <param name="userKey">The unique key of the user accessing the database.</param>
        /// <param name="impersonationBehavior">Identifies which <c>WindowsIdentity</c> is used to
        ///     access the database when impersonation is involved.</param>
        /// <param name="disableSecurityChecks">True if security checks should be skipped when performing operations
        ///     in the database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> or <paramref name="userKey"/>
        ///     is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="impersonationBehavior"/>
        ///     has an invalid value</exception>
        /// <example>The following code opens the store located in the "Mls"
        /// database of the "Learning" server with the user key "Bob".  The original (non-impersonated)
        /// identity is used when accessing the database:
        /// <code language="C#">
        /// WindowsIdentity applicationIdentity = ...
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob", ImpersonationBehavior.UseOriginalIdentity, false);
        /// </code>
        /// </example>
        public LearningStore(string connectionString, string userKey, ImpersonationBehavior impersonationBehavior, bool disableSecurityChecks)
        {
            // Check the input parameters
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (userKey == null)
                throw new ArgumentNullException("userKey");
            if ((impersonationBehavior != ImpersonationBehavior.UseImpersonatedIdentity) &&
                (impersonationBehavior != ImpersonationBehavior.UseOriginalIdentity))
                throw new ArgumentOutOfRangeException("impersonationBehavior");
                
            // Save the passed-in values
            m_connectionString = connectionString;
            m_userKey = userKey;
            m_impersonationBehavior = impersonationBehavior;
            m_disableSecurityChecks = disableSecurityChecks;

            // Default locale is the current thread locale
            m_locale = CultureInfo.CurrentCulture;
        }
        
        /// <summary>
        /// The connection string passed in the constructor
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
        }

        /// <summary>
        /// The UserKey passed in the constructor
        /// </summary>
        public string UserKey
        {
            get
            {
                return m_userKey;
            }
        }
        
        /// <summary>
        /// Gets or sets the locale information used to convert property values or column values
        /// from one type to another.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is the current system CultureInfo.
        /// </para>
        /// <para>
        /// Setting this value does not affect currently-existing <Typ>LearningStoreQuery</Typ> or
        /// <Typ>LearningStoreJob</Typ> objects.
        /// </para>
        /// </remarks>
        public CultureInfo Locale
        {
            get
            {
                return m_locale;
            }
            
            set
            {
                if(value == null)
                    throw new ArgumentNullException("value");
                m_locale = value;
            }
        }

        /// <summary>
        /// Clears the schema cache.
        /// </summary>
        /// <remarks>
        /// The schema cache is an appdomain-wide cache that maps connection strings
        /// to schema information.  When a new <Typ>LearningStore</Typ> instance is
        /// created, the schema information will be read from the cache (rather than
        /// from the database) if available.  This method clears the cache, which
        /// will force schema information to be re-read from the database.
        /// </remarks>
        public static void ClearSchemaCache()
        {
            lock(s_allSchemasLock)
            {
                s_allSchemas.Clear();
            }
        }

        /// <summary>
        /// Create a new instance of the <Typ>LearningStoreJob</Typ> class.
        /// </summary>
        /// <returns>A new <Typ>LearningStoreJob</Typ></returns>
        /// <exception cref="InvalidOperationException">The store was created
        ///     by a different version of this product, or the schema information stored
        ///     inside the database is invalid.</exception>
        /// <exception cref="SqlException">The database could not be
        ///     opened or other SQL error.</exception>
        /// <example>The following code opens a store and creates a new job:
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// LearningStoreJob job = store.CreateJob();
        /// </code>
        /// </example>
        public LearningStoreJob CreateJob()
        {
            // Create the job
            LearningStoreJob job = new LearningStoreJob(this, m_connectionString, GetSchema(),
                m_locale, m_debugLog, m_userKey, m_disableSecurityChecks, m_impersonationBehavior);

            return job;
        }

        /// <summary>
        /// Create a new instance of the <Typ>LearningStoreQuery</Typ> class
        /// </summary>
        /// <returns>A new <Typ>LearningStoreQuery</Typ></returns>
        /// <param name="viewName">Name of the view for the query.  If this is an item type name,
        ///     then the default view for the item type is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">The store was created
        ///     by a different version of this product, the schema information stored
        ///     inside the database is invalid, or <paramref name="viewName"/>
        ///     does not refer to a view or item type in the store.</exception>
        /// <exception cref="SqlException">The database could not be
        ///     opened or other SQL error.</exception>
        /// <remarks>
        /// See the <Typ>LearningStoreQuery</Typ> documentation for more information
        /// about constructing queries.
        /// </remarks>
        /// <example>The following code opens a store and creates a new query:
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// LearningStoreQuery query = store.CreateQuery("AttemptItem");
        /// </code>
        /// </example>
        public LearningStoreQuery CreateQuery(string viewName)
        {
            // Check parameters
            if (viewName == null)
                throw new ArgumentNullException("viewName");

            // Get the information about the item
            LearningStoreSchema schema = GetSchema();
            LearningStoreView view = null;
            if (!schema.TryGetViewByName(viewName, out view))
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    LearningStoreStrings.ViewNotFoundInSchema, viewName));

            return new LearningStoreQuery(this, view, m_locale);
        }

        /// <summary>
        /// Enables debug log
        /// </summary>
        /// <param name="destination">Object to which the debug
        ///     output should be written.</param>
        /// <remarks>
        /// <para>
        /// When the debug log is enabled, information useful for debugging
        /// is written to <paramref name="destination"/>.  This currently includes:
        /// <ul>
        /// <li>Information sent and read from SQL Server due to method calls on this object.
        ///     Includes the command text, parameters, and result sets.</li>
        /// <li>Information sent and read from SQL Server due to method calls on <b>new</b>
        ///     <Typ>LearningStoreJob</Typ> instances.  Includes the command text,
        ///     parameters, and result sets.</li>
        /// </ul>
        /// </para>
        /// <para>
        /// There is no guarantee that all information output by SQL Server is in the log.  Only
        /// the information that is used is added to the log.  For example, SQL Server may
        /// output a result set and then output an error.  Since the error causes the transaction
        /// to be rolled back, the first result set is no longer used.  Therefore, the result
        /// set is not included in the log.
        /// </para>
        /// <para>
        /// The output is in a pseudo-HTML format.  It isn't 100% valid HTML, since it doesn't
        /// include e.g., a &lt;HTML&gt; tag.  However, it can be viewed in a browser.
        /// </para>
        /// <para>
        /// The information written may change from version-to-version.  Information written to
        /// the log may also security-sensitive information (e.g., information that the user wouldn't
        /// normally have access to).
        /// </para>
        /// <para>
        /// Enabling the debug log will decrease the performance of the object.  So enabling
        /// the debug log on a "live" system is not recommended.
        /// </para>
        /// <para>
        /// The caller "owns" the object passed in <paramref name="destination"/>.  So if, for example,
        /// it refers to a file, the file will not be closed until the caller closes it.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code language="C#">
        /// StreamWriter writer = new StreamWriter("c:\\log.htm");
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// store.EnableDebugLog(writer);
        /// ...
        /// writer.Dispose();
        /// </code>
        /// </example>
        public void EnableDebugLog(TextWriter destination)
        {
            if(destination == null)
                throw new ArgumentNullException("destination");
            if(m_debugLog != null)
                throw new InvalidOperationException(LearningStoreStrings.DebugLogAlreadyEnabled);
            
            m_debugLog = destination;            
        }

        /// <summary>
        /// Get the schema information for this store.
        /// </summary>
        /// <returns>The schema information</returns>
        /// <remarks>
        /// If it has already been retrieved for this store, then just
        /// return that copy.  If it hasn't been retrieved, then grab
        /// it from the AppDomain-wide cache or from the database.
        /// </remarks>
        internal LearningStoreSchema GetSchema()
        {
            // If the information isn't available, then call the static
            // method to cache schema information
            if (m_schema == null)
                m_schema = GetSchemaInformationFromCache(m_connectionString, m_impersonationBehavior, m_debugLog);

            // Return it
            return m_schema;
        }

        /// <summary>
        /// Get the schema information for this store from the cache or from the database.
        /// </summary>
        /// <param name="connectionString">Connection string used to access the store.</param>
        /// <param name="impersonationBehavior">Identifies which <c>WindowsIdentity</c> is used to
        ///     access the database when impersonation is involved.</param>
        /// <param name="debugLog">Location to which the debug log should be written, or
        ///     null if there isn't a debug log.</param>
        /// <returns>The schema information.</returns>
        private static LearningStoreSchema GetSchemaInformationFromCache(string connectionString,
            ImpersonationBehavior impersonationBehavior, TextWriter debugLog)
        {
            // Try to find the connection in the cache
            lock(s_allSchemasLock)
            {
                LearningStoreSchema schema;
                if(s_allSchemas.TryGetValue(connectionString, out schema))
                    return schema;
            }

            // Not found in the cache -- so go get it from the database
            
            // This try/catch block is here for security reasons. Search MSDN for 
            // "WrapVulnerableFinallyClausesInOuterTry" to see details.
            try
            {
                WindowsImpersonationContext impersonationContext = null;
                
                try
                {
                    // Impersonate if necessary                        
                    if (impersonationBehavior == ImpersonationBehavior.UseOriginalIdentity)
                        // Not adding it to the disposer, since that could fail (e.g., OutOfMemoryException),
                        // which could cause a security hole.  Instead, we'll clean it up manually later.
                        impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);

                    using (Microsoft.LearningComponents.Disposer disposer = new Microsoft.LearningComponents.Disposer())
                    {
                        // Create a connection
                        SqlConnection connection = new SqlConnection(connectionString);
                        disposer.Push(connection);
                        connection.Open();

                        // Create a command to retrieve information from the configuration table
                        LogableSqlCommand command = new LogableSqlCommand(connection, debugLog); 
                        disposer.Push(command);

                        // Execute
                        command.Execute(
                            "SELECT EngineVersion,\r\n" +
                            "    SchemaDefinition\r\n" +
                            "FROM Configuration\r\n");

                        // Read return values from the database                
                        if (!command.Read())
                            throw new LearningComponentsInternalException("LSTR1500");
                        if(command.GetFieldCount() != 2)
                            throw new LearningComponentsInternalException("LSTR1510");
                        int engineVersion = command.GetInt32(0);
                        SqlXml schemaXml = command.GetSqlXml(1);
                        XPathDocument schemaDoc;
                        using(XmlReader reader = schemaXml.CreateReader())
                        {
                            schemaDoc = new XPathDocument(reader);
                        }
                        LearningStoreSchema newSchema = LearningStoreSchema.CreateSchema(schemaDoc);
                        if (command.Read())
                            throw new LearningComponentsInternalException("LSTR1520");
                        if (command.NextResult())
                            throw new LearningComponentsInternalException("LSTR1530");

                        // Fail if a different engine created this
                        if (engineVersion != LearningStore.EngineVersion)
                            throw new InvalidOperationException(LearningStoreStrings.IncompatibleEngineVersion);
                            
                        // Save it in the cache
                        lock(s_allSchemasLock)
                        {
                            LearningStoreSchema schema;
                            if(s_allSchemas.TryGetValue(connectionString, out schema))
                                return schema;
                            s_allSchemas.Add(connectionString, newSchema);                
                        }
                        
                        return newSchema;
                    }
                }
                finally
                {
                    if (impersonationContext != null)
                        impersonationContext.Dispose();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
