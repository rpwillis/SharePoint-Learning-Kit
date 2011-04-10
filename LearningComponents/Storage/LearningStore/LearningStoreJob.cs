/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Transactions;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;

#endregion

/*
 * This file contains the core of the LearningStoreJob class
 * 
 * Internal error numbers: 1800-1999
 */
 
namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a job that can be executed by a store
    /// </summary>
    /// <remarks>
    /// A <b>job</b> is a collection of operations that can be performed
    /// on a store.  All of the operations are executed as a batch, and if
    /// any of the operations fail, then the entire job fails.  To create
    /// and execute a job, perform the following steps:<ol>
    /// <li>Create a new <Typ>LearningStoreJob</Typ> on a store by calling
    ///     <Mth>../LearningStore.CreateJob</Mth>.</li>
    /// <li>Call methods on the <Typ>LearningStoreJob</Typ> to add operations
    ///     to the job.  The operations will be executed in the order in which
    ///     they are added to the job.</li>
    /// <li>Call <Mth>Execute</Mth> on the job to execute it.  The results of
    /// the job are returned.</li>
    /// </ol>
    /// <p/>
    /// Here's some example code that adds a new user to a store:
    /// <code language="C#">
    /// // Create the job
    /// LearningStoreJob job = store.CreateJob();
    /// 
    /// // Add the "add" operation
    /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Name"] = "John Doe";
    /// propertyValues["Key"] = "Company\JohnDoe";
    /// job.AddItem("UserItem", propertyValues);
    /// 
    /// // Execute the job
    /// job.Execute();
    /// </code>
    /// Multiple operations can be performed in the same job.  For example,
    /// here's some code that adds a new user, a new global objective, and a new
    /// learner global objective:
    /// <code language="C#">
    /// // Create the job
    /// LearningStoreJob job = store.CreateJob();
    ///
    /// // Add the first "add" operation that adds the user
    /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Name"] = "John Doe";
    /// propertyValues["Key"] = "Company\JohnDoe";
    /// LearningStoreItemIdentifier newUserId = job.AddItem("UserItem", propertyValues);
    ///
    /// // Add the second "add" operation that adds the global objective
    /// propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Key"] = "MyGlobalObjective";
    /// propertyValues["Description"] = "MyGlobalObjective Description";
    /// LearningStoreItemIdentifier newGlobalObjectiveId = job.AddItem("GlobalObjectiveItem", propertyValues);
    ///
    /// // Add the third "add operation" that adds the learner global objective
    /// propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["LearnerId"] = newUserId;
    /// propertyValues["GlobalObjectiveId"] = newGlobalObjectiveId;
    /// job.AddItem("LearnerGlobalObjectiveItem", propertyValues);
    /// 
    /// // Execute the job, which adds all the items
    /// job.Execute();
    /// </code>
    /// Operations in a job may return results.  For example, the "add item" operation,
    /// with the correct parameters, can return the Id of the added item.  If multiple
    /// operations are added to a job, the results are returned in the order
    /// of the operations performed.  Example:
    /// <code language="C#">
    /// // Create the job
    /// LearningStoreJob job = store.CreateJob();
    ///
    /// // Add the first "add" operation that adds the user
    /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Name"] = "John Doe";
    /// propertyValues["Key"] = "Company\JohnDoe";
    /// LearningStoreItemIdentifier newUserId = job.AddItem("UserItem", propertyValues, true); // passing true forces the operation to return the Id of the new item in the store in the results
    ///
    /// // Add the second "add" operation that adds the global objective
    /// propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Key"] = "MyGlobalObjective";
    /// propertyValues["Description"] = "MyGlobalObjective Description";
    /// LearningStoreItemIdentifier newGlobalObjectiveId = job.AddItem("GlobalObjectiveItem", propertyValues);  
    ///
    /// // Add the third "add operation" that adds the learner global objective
    /// propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["LearnerId"] = newUserId;
    /// propertyValues["GlobalObjectiveId"] = newGlobalObjectiveId;
    /// job.AddItem("LearnerGlobalObjectiveItem", propertyValues, true); // passing true forces the operation to return the Id of the new item in the store in the results
    /// 
    /// // Execute the job, which adds all the items
    /// ReadOnlyCollection&lt;object&gt; results = job.Execute();
    /// </code>
    /// After the code above is executed, results[0] contains the Id of the user
    /// (the first operation that returns a result), and results[1] contains the Id of the
    /// learner global objective (the second operation that returns a result).  See the
    /// specific methods for more information about what results are returned
    /// (e.g., see the <Mth>AddItem</Mth> method for more information about the results
    /// that are returned when adding an item).
    /// <p/>
    /// See the <Mth>Execute</Mth> method documentation for more information about
    /// executing jobs, including information about how jobs interact with
    /// transactions.
    /// </remarks>
    public class LearningStoreJob
    {
        /// <summary>
        /// Represents a fragment of SQL that will be executed when the
        /// job is executed. 
        /// </summary>
        /// <remarks>
        /// The fragment consists of:<p/>
        /// 1) Command text.  The text must follow the following rules:<p/>
        /// 1a) All variables declared in the text must be unique.  This
        ///     can be guaranteed by using the CreateVariableName method.<p/>
        /// 1b) BEGIN TRANSACTION, COMMIT TRANSACTION, and ROLLBACK TRANSACTION
        ///     must not be used.  All commands already run within a transaction.
        ///     <p/>
        /// 1c) RAISERROR('LSERROR',...) can be used to throw a
        ///     LearningStore-specific exception.  If state=1, a
        ///     LearningStoreItemNotFoundException is thrown.  If state=2,
        ///     an InvalidOperationException is thrown.  If state=3,
        ///     a LearningStoreSecurityException is thrown.<p/>
        /// 1d) The variables @LastError, @LastRowCount, and @UserKey should not be defined.<p/>
        /// 2) A set of input parameters.  These should be created
        ///    using the CreateParameter method.  If Parameter.Name
        ///    is read in the SQL code, the current value of the
        ///    parameter will be returned.<p/>
        /// 3) An optional output parameter.  This should be created
        ///    using the CreateParameter method.  If
        ///    Parameter.Name is written in the SQL code, the current
        ///    value of the parameter will be changed.<p/>
        /// 4) An option result that will be returned to the caller.
        ///    This should be created using the Create*Result
        ///    methods.
        /// </remarks>
        private class CommandFragment
        {
            /// <summary>
            /// Command text
            /// </summary>
            private string m_commandText;

            /// <summary>
            /// Input parameters
            /// </summary>
            private ReadOnlyCollection<CommandFragmentParameter> m_inputParameters;

            /// <summary>
            /// Output parameter
            /// </summary>
            private CommandFragmentParameter m_outputParameter;

            /// <summary>
            /// Result
            /// </summary>
            private CommandFragmentResult m_result;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="commandText"></param>
            /// <param name="inputParameters"></param>
            /// <param name="outputParameter"></param>
            /// <param name="result"></param>
            public CommandFragment(string commandText,
                ReadOnlyCollection<CommandFragmentParameter> inputParameters,
                CommandFragmentParameter outputParameter,
                CommandFragmentResult result)
            {
                m_commandText = commandText;
                m_inputParameters = inputParameters;
                m_outputParameter = outputParameter;
                m_result = result;
            }

            /// <summary>
            /// Command text
            /// </summary>    
            public string CommandText
            {
                get
                {
                    return m_commandText;
                }
            }

            /// <summary>
            /// Input parameters
            /// </summary>
            public ReadOnlyCollection<CommandFragmentParameter> InputParameters
            {
                get
                {
                    return m_inputParameters;
                }
            }

            /// <summary>
            /// Output parameters
            /// </summary>
            public CommandFragmentParameter OutputParameter
            {
                get
                {
                    return m_outputParameter;
                }
            }

            /// <summary>
            /// Result
            /// </summary>
            public CommandFragmentResult Result
            {
                get
                {
                    return m_result;
                }
            }
        }

        /// <summary>
        /// Represents a parameter that can be attached to CommandFragment(s).
        /// </summary>
        /// <remarks>
        /// Parameters can be "input only" or "input/output,"
        /// depending on how they are used in the job's
        /// CommandFragments.  If a parameter is never used as an
        /// output parameter, then it is "input only."  If a
        /// parameter is used as an output parameter, then it
        /// is "input/output."
        /// </remarks>
        private class CommandFragmentParameter
        {
            /// <summary>
            /// SQL type of the parameter
            /// </summary>
            private SqlDbType m_dbType;

            /// <summary>
            /// SQL name of the parameter
            /// </summary>
            private string m_name;

            /// <summary>
            /// Initial value
            /// </summary>
            /// <remarks>
            /// Should never be null (but DBNull.Value is allowed)
            /// </remarks>
            private object m_initialValue;

            /// <summary>
            /// Current value
            /// </summary>
            /// <remarks>
            /// For "input only" parameters, this is always equal to
            /// <Fld>m_initialValue</Fld>.  For "input/output" parameters,
            /// this value could change during job execution.
            /// Should never be null (by DBNull.Value is allowed)
            /// </remarks>
            private object m_currentValue;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="dbType"></param>
            /// <param name="name"></param>
            /// <param name="initialValue"></param>
            public CommandFragmentParameter(SqlDbType dbType, string name,
                object initialValue)
            {
                if(initialValue == null)
                    throw new LearningComponentsInternalException("LSTR1800");
                m_dbType = dbType;
                m_name = name;
                m_currentValue = m_initialValue = initialValue;
            }

            /// <summary>
            /// Name
            /// </summary>
            public string Name
            {
                get
                {
                    return m_name;
                }
            }

            /// <summary>
            /// Initial Value
            /// </summary>
            public object InitialValue
            {
                get
                {
                    return m_initialValue;
                }
            }

            /// <summary>
            /// SQL type of the parameter
            /// </summary>
            public SqlDbType DbType
            {
                get
                {
                    return m_dbType;
                }
            }

            /// <summary>
            /// Current value
            /// </summary>
            public object CurrentValue
            {
                get
                {
                    return m_currentValue;
                }
                set
                {
                    if (value == null)
                        throw new LearningComponentsInternalException("LSTR1805");
                    m_currentValue = value;
                }
            }
        }

        /// <summary>
        /// Represents the type of a result for a CommandFragment
        /// </summary>
        private enum CommandFragmentResultType
        {
            /// <summary>
            /// One item identifier
            /// </summary>
            ItemIdentifier = 1,

            /// <summary>
            /// One DataTable
            /// </summary>
            DataTable,
        }

        /// <summary>
        /// Represents a result for a CommandFragment
        /// </summary>
        private class CommandFragmentResult
        {
            /// <summary>
            /// Type of the result
            /// </summary>
            private CommandFragmentResultType m_resultType;

            /// <summary>
            /// Item type of the result.
            /// </summary>
            /// <remarks>
            /// Valid only when <Fld>m_resultType</Fld>=ItemIdentifier
            /// </remarks>
            private LearningStoreItemType m_itemType;

            /// <summary>
            /// Columns for the result
            /// </summary>
            /// <remarks>
            /// Valid only when <Fld>m_resultType</Fld>=DataTable
            /// </remarks>
            private IList<LearningStoreViewColumn> m_columns;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="resultType"></param>
            /// <param name="itemType"></param>
            /// <param name="columns"></param>
            public CommandFragmentResult(CommandFragmentResultType resultType,
                LearningStoreItemType itemType,
                IList<LearningStoreViewColumn> columns)
            {
                m_resultType = resultType;
                m_itemType = itemType;
                m_columns = columns;
            }

            /// <summary>
            /// Result type
            /// </summary>
            public CommandFragmentResultType ResultType
            {
                get
                {
                    return m_resultType;
                }
            }

            /// <summary>
            /// Item type
            /// </summary>
            public LearningStoreItemType ItemType
            {
                get
                {
                    return m_itemType;
                }
            }

            /// <summary>
            /// Columns
            /// </summary>
            public ReadOnlyCollection<LearningStoreViewColumn> Columns
            {
                get
                {
                    return new ReadOnlyCollection<LearningStoreViewColumn>(m_columns);
                }
            }
        }

        /// <summary>
        /// The store on which this job was created
        /// </summary>
        private LearningStore m_store;
        
        /// <summary>
        /// The schema for <Fld>m_store</Fld>
        /// </summary>
        private LearningStoreSchema m_schema;

        /// <summary>
        /// The connection string for <Fld>m_connectionString</Fld>
        /// </summary>
        private string m_connectionString;

        /// <summary>
        /// Location in which to write the information sent to/from SQL
        /// </summary>
        /// <remarks>
        /// null if the information shouldn't be written.
        /// </remarks>
        private TextWriter m_debugLog;
        
        /// <summary>
        /// Locale used to convert to/from strings
        /// </summary>
        private CultureInfo m_locale;

        /// <summary>
        /// User key of user performing the job
        /// </summary>
        private string m_userKey;

        /// <summary>
        /// True if security checks should be skipped
        /// </summary>
        private bool m_disableSecurityChecks;

        /// <summary>
        /// Impersonation behavior
        /// </summary>
        private ImpersonationBehavior m_impersonationBehavior;
                        
        /// <summary>
        /// Id used to create unique variable/parameter names
        /// </summary>
        /// <remarks>
        /// Incremented whenever a new name is created
        /// </remarks>
        private int m_uniqueNameIndex;

        /// <summary>
        /// Command fragments that will be executed when the job is executed
        /// </summary>
        private List<CommandFragment> m_commandFragments = new List<CommandFragment>();

        /// <summary>
        /// List of items being added during this job.
        /// </summary>
        /// <remarks>
        /// The key is the RawKey of the LearningStoreItemIdentifier returned from the method that
        /// was adding the item (e.g., AddItem).  The value is the parameter containing
        /// that Id in SQL.<p/>
        /// Used so e.g., a user can add two related items in one job.<p/>
        /// </remarks>
        private Dictionary<long, CommandFragmentParameter> m_addedItemIds =
            new Dictionary<long, CommandFragmentParameter>();

        /// <summary>
        /// Number of retries before failing due to a deadlock
        /// </summary>
        private const int DeadlockAttemptsBeforeFailing = 5;

        /// <summary>
        /// Maximum number of parameters that should be send in a SQL batch.
        /// </summary>
        private const int MaximumSqlParameters = 1800;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreJob</Typ> class.
        /// </summary>
        /// <param name="store">Store on which the job should be created.</param>
        /// <param name="connectionString">Connection string to use when connecting to the database.</param>
        /// <param name="schema">The schema in the store.</param>
        /// <param name="locale">Locale to use when converting to/from strings.</param>
        /// <param name="debugLog">Location in which to write the data being sent to/from SQL.</param>
        /// <param name="userKey">User key of user</param>
        /// <param name="disableSecurityChecks">True if security checks should be skipped</param>
        /// <param name="impersonationBehavior">Identifies which <c>WindowsIdentity</c> is used to
        ///     access the database when impersonation is involved.</param>
        internal LearningStoreJob(LearningStore store, string connectionString,
            LearningStoreSchema schema, CultureInfo locale, TextWriter debugLog,
            string userKey, bool disableSecurityChecks, ImpersonationBehavior impersonationBehavior)
        {
            // Check input parameters
            if (store == null)
                throw new LearningComponentsInternalException("LSTR1810");
            if (connectionString == null)
                throw new LearningComponentsInternalException("LSTR1815");
            if (schema == null)
                throw new LearningComponentsInternalException("LSTR1820");
            if (locale == null)
                throw new LearningComponentsInternalException("LSTR1830");
            if (userKey == null)
                throw new LearningComponentsInternalException("LSTR1840");
            if ((impersonationBehavior != ImpersonationBehavior.UseOriginalIdentity) &&
                (impersonationBehavior != ImpersonationBehavior.UseImpersonatedIdentity))
                throw new LearningComponentsInternalException("LSTR1841");
                
            // Save the information
            m_store = store;
            m_schema = schema;
            m_connectionString = connectionString;
            m_locale = locale;
            m_debugLog = debugLog;
            m_userKey = userKey;
            m_disableSecurityChecks = disableSecurityChecks;
            m_impersonationBehavior = impersonationBehavior;
        }

        /// <summary>
        /// Are security checks disabled?
        /// </summary>
        private bool SecurityChecksDisabled
        {
            get
            {
                if (LearningStorePrivilegedScope.Current != null)
                    return true;
                return m_disableSecurityChecks;
            }
        }
        
        /// <summary>
        /// Create a unique variable name
        /// </summary>
        /// <returns>A new variable name that can be used within the SQL text.</returns>
        private string CreateVariableName()
        {
            return "@V" + (++m_uniqueNameIndex).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Append a command fragment to the job.  The fragment will be executed when
        /// the job is executed.
        /// </summary>
        /// <param name="commandText">Text for the fragment.</param>
        /// <param name="inputParameters">Input parameters for the fragment.</param>
        /// <param name="outputParameter">Output parameter for the fragment.</param>
        /// <param name="result">Result for the fragment.</param>
        private void AppendCommandFragment(string commandText,
            ReadOnlyCollection<CommandFragmentParameter> inputParameters,
            CommandFragmentParameter outputParameter,
            CommandFragmentResult result)
        {
            // Create the CommandFragment item
            CommandFragment commandFragment = new CommandFragment(
                commandText, inputParameters, outputParameter,
                result);
            m_commandFragments.Add(commandFragment);
        }

        /// <summary>
        /// Create a new parameter for a <Typ>CommandFragment</Typ>.
        /// </summary>
        /// <param name="dbType">Type of the parameter.</param>
        /// <param name="value">Initial value for the parameter</param>
        /// <returns>A new parameter.</returns>
        private CommandFragmentParameter CreateParameter(SqlDbType dbType,
            object value)
        {
            // Create a unique parameter name
            string name = CreateVariableName();
            return new CommandFragmentParameter(dbType, name, value);
        }

        /// <summary>
        /// Create a new result for a <Typ>CommandFragment</Typ>.  The result
        /// contains a <Typ>LearningStoreItemIdentifier</Typ>.
        /// </summary>
        /// <param name="itemType">Item type that will be returned in the result.</param>
        /// <returns>A new result</returns>
        private static CommandFragmentResult CreateItemIdentifierResult(LearningStoreItemType itemType)
        {
            return new CommandFragmentResult(
                CommandFragmentResultType.ItemIdentifier, itemType, null);
        }

        /// <summary>
        /// Create a new result for a <Typ>CommandFragment</Typ>.  The result
        /// contains a <Typ>/System.DataTable</Typ>
        /// </summary>
        /// <param name="columns">Columns that will be returned in the result.</param>
        /// <returns>A new result.</returns>
        private static CommandFragmentResult CreateDataTableResult(IList<LearningStoreViewColumn> columns)
        {
            return new CommandFragmentResult(
                CommandFragmentResultType.DataTable, null, columns);
        }

        /// <summary>
        /// Reset all the output parameters in all the command fragments to their
        /// initial value
        /// </summary>
        private void ResetOutputParameterCurrentValues()
        {
            foreach(CommandFragment fragment in m_commandFragments)
            {
                if(fragment.OutputParameter != null)
                    fragment.OutputParameter.CurrentValue = fragment.OutputParameter.InitialValue;
            }
        }
               
        /// <summary>
        /// Try to return <paramref name="id"/> as a string that can be used within
        /// SQL command text.
        /// </summary>
        /// <remarks>
        /// May add a parameter to <paramref name="parameters"/>
        /// </remarks>
        /// <param name="id">Id</param>
        /// <param name="parameters">List of parameters for the current command.
        ///     This method may add an item to this list.  This method will not
        ///     read items from this list.</param>
        /// <param name="valueText">Output text</param>
        /// <returns>True if the operation succeeded, or false if <paramref name="id"/>
        ///    refers to an id that isn't already being added in this job.</returns>        
        private bool TryGetTextForIdentifier(LearningStoreItemIdentifier id,
            List<CommandFragmentParameter> parameters, out string valueText)
        {
            if (!id.HasKey)
            {
                // The id references an item that doesn't have an id from
                // the database yet.  Search for the item in the list
                // of items that will be added in this job
                CommandFragmentParameter parameter = null;
                if (m_addedItemIds.TryGetValue(id.RawKey, out parameter))
                {
                    // Found it!  So add the parameter to the list of
                    // parameters (assuming it doesn't already exist)
                    // and return it
                    if (!parameters.Contains(parameter))
                        parameters.Add(parameter);
                    valueText = parameter.Name;
                    return true;
                }
                // Not found
                valueText = null;
                return false;
            }
            else
            {
                // We've been given the key -- so send it to SQL as a parameter
                CommandFragmentParameter parameter = CreateParameter(SqlDbType.BigInt, id.RawKey);
                parameters.Add(parameter);
                valueText = parameter.Name;
                return true;
            }
        }

        /// <summary>
        /// Try to return <paramref name="value"/> as a string that can be used within
        /// SQL command text.
        /// </summary>
        /// <param name="valueType">Type of the value in <paramref name="value"/></param>
        /// <param name="value">Value</param>
        /// <param name="parameters">List of parameters for the current command.
        ///     This method may add an item to this list.  This method will not
        ///     read items from this list.</param>
        /// <param name="valueText">Output text</param>
        /// <returns>True if the operation succeeded, or false if <paramref name="valueType"/>
        ///    refers to an id that isn't already being added in this job.</returns>        
        private bool TryGetTextForValue(LearningStoreValueType valueType,
            object value, List<CommandFragmentParameter> parameters, out string valueText)
        {
            if (value == null)
            {
                valueText = "null";
                return true;
            }
            switch (valueType.TypeCode)
            {
                case LearningStoreValueTypeCode.Boolean:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.Bit, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.ByteArray:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.VarBinary, (byte[])value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.DateTime:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.DateTime, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.Double:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.Float, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.Guid:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.UniqueIdentifier, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.Single:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.Real, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.String:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.NVarChar, value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.Enumeration:
                case LearningStoreValueTypeCode.Int32:
                    {
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.Int, (int)value);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                case LearningStoreValueTypeCode.ItemIdentifier:
                    return TryGetTextForIdentifier(value as LearningStoreItemIdentifier, parameters, out valueText);
                case LearningStoreValueTypeCode.Xml:
                    {
                        LearningStoreXml xmlValue = value as LearningStoreXml;
                        CommandFragmentParameter parameter = CreateParameter(SqlDbType.Xml, xmlValue.SqlXml);
                        parameters.Add(parameter);
                        valueText = parameter.Name;
                        return true;
                    }
                default:
                    throw new LearningComponentsInternalException("LSTR1850");
            }
        }

        /// <summary>
        /// Execute the commands in the job
        /// </summary>
        /// <returns>A collection of output data from the job</returns>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreItemNotFoundException">
        ///     The job failed because an item was not found in the store.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreConstraintViolationException">
        ///     The job failed because of a constraint violation.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreSecurityException">
        ///     The job failed because a security check failed.</exception>
        /// <exception cref="System.InvalidOperationException">An AddOrUpdateItem command failed
        ///     because multiple matching items were found.</exception>
        /// <remarks>
        /// See the <Typ>LearningStoreJob</Typ> documentation for more information
        /// about jobs.
        /// <p/>
        /// This method participates in transactions as follows:<ul>
        /// <li>If this method is executed within a <Typ>LearningStoreTransactionScope</Typ>,
        ///     the job participates in that transaction.  If the job fails, the
        ///     transaction is rolled back.  If a SQL deadlock occurs when executing
        ///     the job and the job is chosen as the deadlock victim, the caller
        ///     will receive a <Typ>SqlException</Typ>.</li>
        /// <li>If this method is not executed within a <Typ>LearningStoreTransactionScope</Typ>,
        ///     then a new serializable transaction is created and used.  If the job
        ///     succeeds, the transaction is committed.  If the job fails, the
        ///     transaction is rolled back.  If a SQL deadlock occurs when
        ///     executing the job and the job is chosen as the deadlock victim,
        ///     the job is retried several times.</li>
        /// </ul>
        /// </remarks>
        /// <example>
        /// The following code adds a new user to the store specified by "store":
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// propertyValues["Key"] = "Company\JohnDoe";
        /// LearningStoreJob job = store.CreateJob();
        /// job.AddItem("UserItem", propertyValues);
        /// ReadOnlyCollection&lt;object&gt; results = job.Execute();
        /// </code>
        /// After this code is executed, results[0] contains a
        /// <Typ>LearningStoreItemIdentifier</Typ> for the newly-added item.
        /// </example>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")] // Much of the complexity is due to error checking and switch statements, not sure it would be simpler another way
        [SuppressMessage("Microsoft.Usage", "CA2223")] // FxCop bug
        public ReadOnlyCollection<object> Execute()
        {
            // Try executing the command the correct number of attempts
            int attemptsLeft = DeadlockAttemptsBeforeFailing;
            while (attemptsLeft-- > 0)
            {
                using (Disposer disposer = new Disposer())
                {
                    // This try/catch block is here for security reasons. Search MSDN for 
                    // "WrapVulnerableFinallyClausesInOuterTry" to see details.
                    try
                    {
                        WindowsImpersonationContext impersonationContext = null;
                        
                        try
                        {
                            // Reset all the output parameters to their initial values
                            ResetOutputParameterCurrentValues();

                            // Initialize the list of results
                            List<object> results = new List<object>(m_commandFragments.Count);

                            // Create a Transaction if one doesn't already exist
                            LearningStoreTransactionScope createdTransactionScope = null;
                            if (LearningStoreTransactionScope.Current == null)
                            {
                                TransactionOptions transactionOptions = new TransactionOptions();
                                transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;                                
                                createdTransactionScope = new LearningStoreTransactionScope(transactionOptions);
                                disposer.Push(createdTransactionScope);
                            }
                            else
                            {                            
                                // We're already inside a transaction -- so we can't retry on deadlock
                                attemptsLeft = 0;
                            }

                            // Impersonate if necessary
                            if (m_impersonationBehavior == ImpersonationBehavior.UseOriginalIdentity)
                                // Not adding it to the disposer, since that could fail (e.g., OutOfMemoryException),
                                // which could cause a security hole.  Instead, we'll clean it up manually later.
                                impersonationContext = WindowsIdentity.Impersonate(IntPtr.Zero);

                            // Create the connection
                            SqlConnection connection = LearningStoreTransactionScope.Current.GetConnection(m_connectionString);

                            // We'll be sending the CommandFragments in chunks if there are
                            // too many parameters.  So remember which fragment we're planning
                            // to send next

                            // Next fragment to send
                            int nextFragmentIndex = 0;

                            // Send as many chunks as are needed
                            while (nextFragmentIndex < m_commandFragments.Count)
                            {
                                using (Disposer disposer2 = new Disposer())
                                {
                                    // Remember the first fragment in this chunk
                                    int firstFragmentInChunkIndex = nextFragmentIndex;

                                    // Create the command
                                    LogableSqlCommand command = new LogableSqlCommand(connection, m_debugLog);
                                    disposer2.Push(command);

                                    // Start building the command text
                                    StringBuilder commandText = new StringBuilder(
                                        "SET XACT_ABORT ON\r\n" +
                                        "DECLARE @LastRowCount int\r\n" +
                                        "DECLARE @LastError int\r\n");

                                    // Always pass the UserKey
                                    SqlParameter userKeyParameter = new SqlParameter("@UserKey", SqlDbType.NVarChar); 
                                    userKeyParameter.Value = m_userKey;
                                    command.AddParameter(userKeyParameter);
                                    
                                    // Remember all the parameters that have been added to the command.
                                    // We can't add each one more than once.
                                    Dictionary<CommandFragmentParameter, SqlParameter> parameterMap =
                                        new Dictionary<CommandFragmentParameter, SqlParameter>();

                                    // Enumerate through the command fragments to build
                                    // up the text/parameters to send to SQL                                
                                    while (nextFragmentIndex < m_commandFragments.Count)
                                    {
                                        CommandFragment fragment = m_commandFragments[nextFragmentIndex++];

                                        // Verify that there's enough room in the SqlCommand for all the
                                        // parameters
                                        if ((command.GetParameterCount() + fragment.InputParameters.Count +
                                            ((fragment.OutputParameter != null) ? 1 : 0)) > MaximumSqlParameters)
                                        {
                                            // Too many parameters -- need to send this fragment
                                            // and any later fragments in a new chunk
                                            nextFragmentIndex--;
                                            break;
                                        }

                                        // Add the command text
                                        commandText.Append(fragment.CommandText);

                                        // Add input parameters
                                        foreach (CommandFragmentParameter parameter in fragment.InputParameters)
                                        {
                                            // If the input parameter maps to a parameter already added to the
                                            // chunk, then don't add it
                                            if (parameterMap.ContainsKey(parameter))
                                                continue;

                                            // Create the corresponding SqlParameter
                                            SqlParameter newSqlParameter = new SqlParameter(parameter.Name,
                                                parameter.DbType);
                                            newSqlParameter.Value = parameter.CurrentValue;
                                            command.AddParameter(newSqlParameter);

                                            // Remember that we added it
                                            parameterMap.Add(parameter, newSqlParameter);
                                        }

                                        // Add output parameter
                                        if (fragment.OutputParameter != null)
                                        {
                                            // Create the corresponding SqlParameter
                                            SqlParameter newSqlParameter = new SqlParameter(fragment.OutputParameter.Name,
                                                fragment.OutputParameter.DbType);
                                            newSqlParameter.Direction = ParameterDirection.Output;
                                            command.AddParameter(newSqlParameter);

                                            // Remember that we added it
                                            parameterMap.Add(fragment.OutputParameter, newSqlParameter);
                                        }
                                    }

                                    // Execute
                                    command.Execute(commandText.ToString());

                                    // Remember if we're in the first result
                                    bool firstResult = true;

                                    // Step through each of the results we're expecting
                                    for (int fragmentIndex = firstFragmentInChunkIndex;
                                        fragmentIndex < nextFragmentIndex; fragmentIndex++)
                                    {
                                        CommandFragment fragment = m_commandFragments[fragmentIndex];

                                        // Skip if there isn't a result for this fragment
                                        if (fragment.Result == null)
                                            continue;

                                        // Move to the next result from SQL if needed
                                        if (!firstResult)
                                        {
                                            if (command.NextResult() == false)
                                                throw new LearningComponentsInternalException("LSTR1860");
                                        }

                                        // Add the result to <results>    
                                        switch (fragment.Result.ResultType)
                                        {
                                            // Item identifier
                                            case CommandFragmentResultType.ItemIdentifier:
                                                results.Add(SqlDataReaderHelpers.ReadItemIdentifierResult(command,
                                                    fragment.Result.ItemType));
                                                break;
                                            // DataTable
                                            case CommandFragmentResultType.DataTable:
                                                results.Add(SqlDataReaderHelpers.ReadDataTableResult(command,
                                                    fragment.Result.Columns, m_locale));
                                                break;
                                            default:
                                                throw new LearningComponentsInternalException("LSTR1870");
                                        }

                                        // We're past the first result
                                        firstResult = false;
                                    }

                                    // Something is wrong if there's another result
                                    if (command.NextResult() != false)
                                        throw new LearningComponentsInternalException("LSTR1880");

                                    // Step through each of the output parameters that we're expecting
                                    for (int fragmentIndex = firstFragmentInChunkIndex;
                                        fragmentIndex < nextFragmentIndex; fragmentIndex++)
                                    {
                                        CommandFragment fragment = m_commandFragments[fragmentIndex];
                                        if (fragment.OutputParameter == null)
                                            continue;

                                        // Remember the current value for the next chunk                                    
                                        fragment.OutputParameter.CurrentValue = parameterMap[fragment.OutputParameter].Value;
                                    }
                                }
                            }

                            // Success -- so the transaction completed
                            if (createdTransactionScope != null)
                            {
                                createdTransactionScope.Complete();
                            }

                            // All done, so return the results
                            return new ReadOnlyCollection<object>(results);
                        }
                        catch (SqlException e)
                        {
                            // Try again if we're a deadlock victim AND
                            // we can try again
                            if ((e.Number == 1205) && (attemptsLeft > 0))
                                continue;

                            if ((e.Number == 50000) && (e.Message == "LSERROR"))
                            {
                                if (e.State == 1)
                                    throw new LearningStoreItemNotFoundException(
                                        LearningStoreStrings.ItemNotFound);
                                else if (e.State == 2)
                                    throw new InvalidOperationException(
                                        LearningStoreStrings.MoreThanOneItemFoundInAddOrUpdate);
                                else if (e.State == 3)
                                    throw new LearningStoreSecurityException(
                                        LearningStoreStrings.SecurityCheckFailed);
                                else
                                    throw new LearningComponentsInternalException("LSTR1890");                            
                            }
                            else if ((e.Number == 547) || (e.Number == 2627) || (e.Number == 2601))
                            {
                                // FK violation in or
                                // CHECK constraint violation or
                                // UNIQUE constraint violation or
                                // Duplicate key
                                throw new LearningStoreConstraintViolationException(
                                    LearningStoreStrings.OperationFailedBecauseOfConstraintViolation, e);
                            }
                            throw;
                        }
                        finally
                        {
                            if(impersonationContext != null)
                                impersonationContext.Dispose();
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            // Should never get here
            throw new LearningComponentsInternalException("LSTR1900");
        }

        /// <summary>
        /// Execute the commands in the job.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <returns>The single piece of output data from the job</returns>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreItemNotFoundException">
        ///     The job failed because an item was not found in the store.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreConstraintViolationException">
        ///     The job failed because of a constraint violation.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreSecurityException">
        ///     The job failed because a security check failed.</exception>
        /// <exception cref="System.InvalidOperationException">The job successfully executed but
        ///     there was more than one piece of output data or the output data couldn't be
        ///     cast to the specified type, or  an AddOrUpdateItem command failed
        ///     because multiple matching items were found.</exception>
        /// <seealso cref="Execute"/>
        /// <remarks>
        /// Calls <Mth>Execute</Mth> and then casts the result to type <typeparamref name="T"/>.
        /// If the result can't be cast to type <typeparamref name="T"/>, or there is more
        /// than one result, an exception is thrown.
        /// </remarks>
        /// <example>
        /// The following code adds a new user to the store specified by "store":
        /// <code language="C#">
        /// LearningStore store = new LearningStore("Server=Learning;Database=Mls;Integrated Security=true", "Bob");
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// propertyValues["Key"] = "Company\JohnDoe";
        /// LearningStoreJob job = store.CreateJob();
        /// job.AddItem("UserItem", propertyValues);
        /// LearningStoreItemIdentifier id = job.Execute&lt;LearningStoreItemIdentifier&gt;();
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Usage", "CA2223")] // FxCop bug
        [SuppressMessage("Microsoft.Design", "CA1004")] // There's another method that doesn't use generics if this is too confusing
        public T Execute<T>() where T : class
        {
            ReadOnlyCollection<object> results = Execute();
            if (results.Count != 1)
                throw new InvalidOperationException(
                    LearningStoreStrings.ResultsOfJobCanNotBeCast);
            T result = results[0] as T;
            if ((result == null) && (results[0] != null))
                throw new InvalidOperationException(
                    LearningStoreStrings.ResultsOfJobCanNotBeCast);
            return result;
        }

		// ----- Operations -----

        /// <summary>
        /// Add a new item
        /// </summary>
        /// <param name="itemTypeName">The name of the item type to add</param>
        /// <param name="propertyValues">Property values for the item.</param>
        /// <returns>A temporary <Typ>LearningStoreItemIdentifier</Typ> that identifies the
        ///     item being added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="itemTypeName"/> or <paramref name="propertyValues"/>is a null reference.</exception>
        /// <exception cref="System.InvalidOperationException">The item type
        ///     in <paramref name="itemTypeName"/> doesn't exist in the store, a
        ///     property name passed in <paramref name="propertyValues"/>
        ///     doesn't exist on the item type, a property value passed in
        ///     <paramref name="propertyValues"/> is not a valid value for the
        ///     corresponding property, or a required property wasn't passed in
        ///     <paramref name="propertyValues"/>.
        /// </exception>
        /// <remarks>
        /// Calls <Mth>AddItem</Mth>(<paramref name="itemTypeName"/>, <paramref name="propertyValues"/>, false).
        /// See that overload for more details.
        /// </remarks>
        public LearningStoreItemIdentifier AddItem(string itemTypeName, IDictionary<string,object> propertyValues)
        {
            // Immediately call the other method
            return AddItem(itemTypeName, propertyValues, false);
        }

        /// <summary>
        /// Add a new item to a store.
        /// </summary>
        /// <param name="itemTypeName">The name of the item type to add</param>
        /// <param name="propertyValues">Property values for the item.  The key is the
        ///     property name, and the value is the value to be placed in the property.</param>
        /// <param name="requestItemId">If true, a <Typ>LearningStoreItemIdentifier</Typ> 
        ///     for the newly-added item in the store will be returned in the results.</param>
        /// <returns>A temporary <Typ>LearningStoreItemIdentifier</Typ> that identifies the
        ///     item being added.  This value may be used to reference the item in other
        ///     commands in this job, but it becomes invalid after the job has been executed.  To
        ///     retrieve a <Typ>LearningStoreItemIdentifier</Typ> that is valid after
        ///     the job has been executed, call this method with <paramref name="requestItemId"/>=true,
        ///     and the <Typ>LearningStoreItemIdentifier</Typ> will be returned as one of the
        ///     results of the job.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="itemTypeName"/> or
        ///     <paramref name="propertyValues"/>is a null reference.</exception>
        /// <exception cref="System.InvalidOperationException">The item type
        ///     in <paramref name="itemTypeName"/> doesn't exist in the store, a
        ///     property name passed in <paramref name="propertyValues"/>
        ///     doesn't exist on the item type, a property value passed in
        ///     <paramref name="propertyValues"/> is not a valid value for the
        ///     corresponding property, or a required property wasn't passed in
        ///     <paramref name="propertyValues"/>.
        /// </exception>
        /// <remarks>
        /// Steps required to add a new item to a store:<ul>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the property values.</li>
        /// <li>Call this method to add the "add item" operation to a job.</li>
        /// <li>Execute the job.  After this step completes, the item actually appears
        /// in the store.</li>
        /// </ul>
        /// <paramref name="propertyValues"/> should contain one entry for each property
        /// that should have a non-default value.  The value type differs depending on
        /// the type of the property on the item:
        /// <ul>
        /// <li><b>Boolean:</b> A System.Boolean, or an object that can be converted into
        ///     a System.Boolean using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>ByteArray</b> A System.Byte array.</li>
        /// <li><b>DateTime:</b> A System.DateTime, or an object that can be converted into
        ///     a System.DateTime using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Double:</b> A System.Double, or an object that can be converted into
        ///     a System.Double using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Enumeration:</b> An System.Int32, or an object that can be converted into
        ///     a System.Int32 using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Guid</b> A System.Guid or a string representing a Guid.</li>
        /// <li><b>Item identifier:</b> A <Typ>LearningStoreItemIdentifier</Typ> 
        ///     of the associated item type.</li>
        /// <li><b>Int32:</b> A System.Int32, or an object that can be converted into
        ///     a System.Int32 using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Single:</b> A System.Single, or an object that can be converted into
        ///     a System.Single using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>String:</b> A System.String, or an object that can be converted into
        ///     a System.String using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Xml:</b> A <Typ>LearningStoreXml</Typ>.</li>
        /// </ul>
        /// Null or DBNull.Value are also permitted, assuming that the property is nullable.
        /// <para/>
        /// This operation returns a <Typ>LearningStoreItemIdentifier</Typ> as a result
        /// if <paramref name="requestItemId"/> is true.  It returns nothing as a result
        /// if <paramref name="requestItemId"/> is false.
        /// </remarks>
        /// <example>
        /// The following code adds a new user item to the store specified by "store":
        /// <code language="C#">
        /// // Create the property values
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// propertyValues["Key"] = "Company\JohnDoe";
        /// 
        /// // Create a job and add the "add item" operation
        /// LearningStoreJob job = store.CreateJob();
        /// job.AddItem("UserItem", propertyValues, false);
        /// 
        /// // Execute the job
        /// job.Execute();
        /// </code>
        /// The following code adds a new user item to the store specified by "store"
        /// and returns the Id of the newly-added item:
        /// <code language="C#">
        /// // Create the property values
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// propertyValues["Key"] = "Company\JohnDoe";
        /// 
        /// // Create a job and add the "add item" operation
        /// LearningStoreJob job = store.CreateJob();
        /// job.AddItem("UserItem", propertyValues, true);
        /// 
        /// // Execute the job, asking for a Id to be returned
        /// LearningStoreItemIdentifier id = job.Execute&lt;LearningStoreItemIdentifier&gt;();
        /// </code>
        /// The following code adds a new user, a new global objective item, and a new
        /// learner global objective item:
        /// <code language="C#">
        /// // Create the job
        /// LearningStoreJob job = store.CreateJob();
        ///
        /// // Add the first "add" operation that adds the user
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// propertyValues["Key"] = "Company\JohnDoe";
        /// LearningStoreItemIdentifier newUserId = job.AddItem("UserItem", propertyValues, true);
        ///
        /// // Add the second "add" operation that adds the global objective
        /// propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Key"] = "MyGlobalObjective";
        /// propertyValues["Description"] = "MyGlobalObjective Description";
        /// LearningStoreItemIdentifier newGlobalObjectiveId = job.AddItem("GlobalObjectiveItem", propertyValues, true);
        ///
        /// // Add the third "add operation" that adds the learner global objective
        /// propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["LearnerId"] = newUserId;
        /// propertyValues["GlobalObjectiveId"] = newGlobalObjectiveId;
        /// job.AddItem("LearnerGlobalObjectiveItem", propertyValues, true);
        /// 
        /// // Execute the job, which adds all the items
        /// ReadOnlyCollection&lt;object&gt; results = job.Execute();
        /// </code>
        /// After this code is executed, results[0] contains the Id of the new user,
        /// results[1] contains the Id of the new global objective, and results[2] contains
        /// the Id of the new learner global objective.
        /// </example>
        public LearningStoreItemIdentifier AddItem(string itemTypeName, IDictionary<string,object> propertyValues, bool requestItemId)
        {
            // Check input parameters
            if (itemTypeName == null)
                throw new ArgumentNullException("itemTypeName");
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            // Find the item type in the schema
            LearningStoreItemType itemType = null;
            if (!m_schema.TryGetItemTypeByName(itemTypeName, out itemType))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ItemTypeNotFoundInSchema, itemTypeName));

            // Create a list for all of the input parameters
            List<CommandFragmentParameter> inputParameters = new List<CommandFragmentParameter>();
            
            // Create an output parameter that will contain the Id
            CommandFragmentParameter outputParameter = CreateParameter(SqlDbType.BigInt, DBNull.Value);

            // Create the object that will construct the SQL text
            AddOperationBlock statement = new AddOperationBlock(
                itemType, outputParameter.Name, requestItemId, SecurityChecksDisabled);
                
            // Enumerate through each property that was passed to us, and add it
            // to the statement
            foreach (KeyValuePair<string, object> propertyValue in propertyValues)
            {
                // Verify that the property name exists on the item
                LearningStoreItemTypeProperty propertyType;
                if(!itemType.TryGetPropertyByName(propertyValue.Key, out propertyType))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyNotFound, propertyValue.Key));
                
                // Can't set the ID property
                if(propertyType.IsId)
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.CannotChangeIdProperty));
                        
                // Cast the passed-in value as necessary
                object value = propertyType.CastValue(propertyValue.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValueWithDescription,
                            propertyValue.Key, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if(!TryGetTextForValue(propertyType.ValueType, value,
                    inputParameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValue, propertyValue.Key));

                // Add it to the statement
                statement.AppendColumn(propertyType,valueText);
            }

            // Verify that the user gave us values for anything that doesn't have a default
            foreach(LearningStoreItemTypeProperty property in itemType.PropertiesWithoutDefaultValue)
            {
                if(!propertyValues.ContainsKey(property.Name))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyValueNotFound, property.Name));
            }
            
            // Create the result value
            CommandFragmentResult result = null;
            if(requestItemId)
                result = CreateItemIdentifierResult(itemType);
                
            // Add the command fragment
            StringBuilder statementText = new StringBuilder();
            statement.Write(statementText);
            AppendCommandFragment(statementText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(inputParameters),
                outputParameter, result);
                        
            // Create the id that identifies the item in this job
            LearningStoreItemIdentifier id = LearningStoreItemIdentifier.CreateTemporaryItemIdentifier(itemType.Name);
            
            // Allow the id to be referenced in future commands in the same
            // job
            m_addedItemIds.Add(id.RawKey, outputParameter);
            
            return id;
        }

        /// <summary>
        /// Update properties on an item
        /// </summary>
        /// <param name="id">Identifies the item being updated.</param>
        /// <param name="propertyValues">Property values to be updated.  The key is the
        ///     property name, and the value is the value to be placed in the property.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> or
        ///     <paramref name="propertyValues"/> is a null reference.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreItemNotFoundException">
        ///     The item was not found in the store.</exception>
        /// <exception cref="System.InvalidOperationException">A
        ///     property name passed in <paramref name="propertyValues"/>
        ///     doesn't exist on the item or a property value passed in
        ///     <paramref name="propertyValues"/> is not a valid value for the
        ///     corresponding property.
        /// </exception>
        /// <remarks>
        /// Steps required to update an item in the store:<ul>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the property values to
        ///     be changed.</li>
        /// <li>Call this method to add the "update item" operation to a job.</li>
        /// <li>Execute the job.  After this step completes, the item is actually
        /// updated in the store.</li>
        /// </ul>
        /// <paramref name="propertyValues"/> should contain one entry for each property
        /// that should be changed.  The value type differs depending on
        /// the type of the property on the item:
        /// <ul>
        /// <li><b>Boolean:</b> A System.Boolean, or an object that can be converted into
        ///     a System.Boolean using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>ByteArray</b> A System.Byte array.</li>
        /// <li><b>DateTime:</b> A System.DateTime, or an object that can be converted into
        ///     a System.DateTime using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Double:</b> A System.Double, or an object that can be converted into
        ///     a System.Double using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Enumeration:</b> An System.Int32, or an object that can be converted into
        ///     a System.Int32 using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Guid</b> A System.Guid or a string representing a Guid.</li>
        /// <li><b>Item identifier:</b> A <Typ>LearningStoreItemIdentifier</Typ> 
        ///     of the associated item type.</li>
        /// <li><b>Int32:</b> A System.Int32, or an object that can be converted into
        ///     a System.Int32 using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Single:</b> A System.Single, or an object that can be converted into
        ///     a System.Single using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>String:</b> A System.String, or an object that can be converted into
        ///     a System.String using <Typ>/System.IConvertible</Typ>.</li>
        /// <li><b>Xml:</b> A <Typ>LearningStoreXml</Typ>.</li>
        /// </ul>
        /// Null or DBNull.Value are also permitted, assuming that the property is nullable.
        /// <para/>
        /// This operation does not return anything as a result.
        /// </remarks>
        /// <example>
        /// The following code updates the name of the user item with an Id of 50:
        /// <code language="C#">
        /// // Create the property values
        /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
        /// propertyValues["Name"] = "John Doe";
        /// 
        /// // Create the job and add the "update properties" operation
        /// LearningStoreJob job = store.CreateJob();
        /// LearningStoreItemIdentifier id = new LearningStoreItemIdentifier("UserItem", 50);
        /// job.UpdateItem(id, propertyValues);
        /// 
        /// // Execute the job
        /// job.Execute();
        /// </code>
        /// </example>
        public void UpdateItem(LearningStoreItemIdentifier id, IDictionary<string,object> propertyValues)
        {
            // Check input parameters
            if (id == null)
                throw new ArgumentNullException("id");
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");
            
            // Get the schema information about the item
            LearningStoreItemType itemType = null;
            if (!m_schema.TryGetItemTypeByName(id.ItemTypeName, out itemType))
                throw new LearningStoreItemNotFoundException(
                    LearningStoreStrings.ItemNotFound);

            // Exit if no properties have been changed
            if (propertyValues.Count == 0)
                return;

            // Need to build up a list of parameters
            List<CommandFragmentParameter> parameters = new List<CommandFragmentParameter>();

            // Get text containing the ID value
            string idText;
            if(!TryGetTextForIdentifier(id, parameters, out idText))
                throw new LearningStoreItemNotFoundException(
                    LearningStoreStrings.ItemNotFound);                            

            // Create an object that will generate the command text
            UpdateOperationBlock statement = new UpdateOperationBlock(itemType, idText, SecurityChecksDisabled);

            // Enumerate through each property that was passed to us, and add it to the statement
            foreach(KeyValuePair<string,object> propertyValue in propertyValues)
            {
                // Verify that the property name exists on the item
                LearningStoreItemTypeProperty propertyType;
                if(!itemType.TryGetPropertyByName(propertyValue.Key, out propertyType))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyNotFound, propertyValue.Key));
                
                // Can't set the ID property
                if(propertyType.IsId)
                    throw new InvalidOperationException(LearningStoreStrings.CannotChangeIdProperty);
                        
                // Cast the passed-in value as necessary
                object value = propertyType.CastValue(propertyValue.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValueWithDescription,
                            propertyValue.Key, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if(!TryGetTextForValue(propertyType.ValueType,
                    value, parameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValue, propertyValue.Key));

                // Add it to the statement
                statement.AppendColumn(propertyType, valueText);
            }

            // Add the command
            StringBuilder statementText = new StringBuilder();
            statement.Write(statementText);
            AppendCommandFragment(statementText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(parameters),
                null, null);
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="id">Identifies the item to be deleted</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is a null reference.</exception>
        /// <exception cref="Microsoft.LearningComponents.Storage.LearningStoreItemNotFoundException">
        ///     The item was not found in the store.</exception>
        /// <remarks>
        /// Steps required to delete an item from the store:<ol>
        /// <li>Create a <Typ>LearningStoreItemIdentifier</Typ> identifying the item,
        /// either by calling the <Typ>LearningStoreItemIdentifier</Typ> constructor
        /// or by retrieving the value from elsewhere.</li>
        /// <li>Call this method to add the "delete item" operation to the job.</li>
        /// <li>Execute the job.</li>
        /// </ol>
        /// <p/>
        /// This operation does not return anything as a result.
        /// <p/>
        /// If other items reference <paramref name="id"></paramref>, and
        /// <paramref name="id"></paramref> is marked as the "container" of those
        /// items, then the other items are also deleted.
        /// </remarks>
        /// <example>
        /// The following code deletes the user with an Id of 50 from the store.
        /// <code language="C#">
        /// // Create the Id
        /// LearningStoreItemIdentifier id = new LearningStoreItemIdentifier("UserItem", 50);
        /// 
        /// // Create the job and add the "delete item" operation
        /// LearningStoreJob job = store.CreateJob();
        /// job.DeleteItem(id);
        /// 
        /// // Execute the job
        /// job.Execute();
        /// </code>
        /// </example>
        public void DeleteItem(LearningStoreItemIdentifier id)
        {
            // Check input parameters
            if (id == null)
                throw new ArgumentNullException("id");

            // Get the schema information about the item
            LearningStoreItemType itemType = null;
            if (!m_schema.TryGetItemTypeByName(id.ItemTypeName, out itemType))
                throw new LearningStoreItemNotFoundException(
                    LearningStoreStrings.ItemNotFound);

            // Command information            
            StringBuilder commandText = new StringBuilder();
            List<CommandFragmentParameter> parameters = new List<CommandFragmentParameter>();

            // Get text containing the ID value
            string idText;
            if(!TryGetTextForIdentifier(id, parameters, out idText))
                throw new LearningStoreItemNotFoundException(
                    LearningStoreStrings.ItemNotFound);                

            // Append the text
            if (!SecurityChecksDisabled)
            {
                if(itemType.DeleteSecurityFunction != null)
                {
                    commandText.Append(
                        "IF dbo.[" + itemType.DeleteSecurityFunction + "](@UserKey,"+idText+") = 0\r\n" +
                        "BEGIN\r\n" +
                        "    RAISERROR('LSERROR',16,3)\r\n" +
                        "    RETURN\r\n" +
                        "END\r\n" +
                        "IF @@ERROR <> 0\r\n" +
                        "    RETURN\r\n");
                }
                else
                {
                    commandText.Append(
                            "RAISERROR('LSERROR',16,3)\r\n" +
                            "RETURN\r\n");
                }                
            }
            commandText.Append(
                "DELETE [" + itemType.Name + "]\r\n" +
                "WHERE Id=" + idText + "\r\n" +
                "IF @@ROWCOUNT=0\r\n" +
                "BEGIN\r\n" +
                "    RAISERROR('LSERROR',16,1)\r\n" +
                "    RETURN\r\n" +
                "END\r\n");

            // Add the command
            AppendCommandFragment(commandText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(parameters),
                null, null);
        }

        /// <summary>
        /// Perform a query
        /// </summary>
        /// <param name="query">The query to be performed</param>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="query"/>
        ///     was not created from this <Typ>LearningStore</Typ>, 
        ///     or at least one output column was not specified in the query.
        /// </exception>
        /// <remarks>
        /// Steps required to perform a query:<ul>
        /// <li>Create a <Typ>LearningStoreQuery</Typ> object using
        /// <Mth>../LearningStore.CreateQuery</Mth>.</li>
        /// <li>Call methods on the <Typ>LearningStoreQuery</Typ> to add output columns,
        /// conditions, and sorting information.</li>
        /// <li>Call this method to add the "perform query" operation to a job.</li>
        /// <li>Execute the job.  A <Typ>/System.Data.DataTable</Typ> is returned
        /// as a result.</li>
        /// </ul><p/>
        /// The <Typ>/System.Data.DataTable</Typ> returned in the result contains
        /// one column for each output column added to the <Typ>LearningStoreQuery</Typ>
        /// using <Mth>../LearningStoreQuery.AddColumn</Mth>.  See
        /// <Mth>../LearningStoreQuery.AddCondition</Mth> for a list of the possible
        /// types that are returned.
        /// <p/>
        /// See the <Typ>LearningStoreQuery</Typ> documentation for more information
        /// about queries.
        /// </remarks>
        /// <example>
        /// The following code performs a query to retrieve the
        /// names of all the users:
        /// <code language="C#">
        /// // Create the query
        /// LearningStoreQuery query = store.CreateQuery("UserItem");
        /// 
        /// // Add output columns to the query
        /// query.AddColumn("Name");
        /// 
        /// // Create a job and add the "perform query" operation
        /// LearningStoreJob job = store.CreateJob();
        /// job.PerformQuery(query);
        /// 
        /// // Execute the job
        /// DataTable result = job.Execute&lt;DataTable&gt;();
        /// </code>
        /// After this code is executed, <c>result</c> contains a DataTable
        /// with one column named "Name." The DataTable contains one row for every user.
        /// </example>
        public void PerformQuery(LearningStoreQuery query)
        {
            // Check parameters
            if (query == null)
                throw new ArgumentNullException("query");

            // Verify that the query is on the correct store
            if (!Object.ReferenceEquals(query.Store, m_store))
                throw new InvalidOperationException(
                    LearningStoreStrings.QueryNotCompatibleWithStore);

            // Error if there are no output columns
            if (query.Columns.Count == 0)
                throw new InvalidOperationException(
                    LearningStoreStrings.AtLeastOneOutputColumnMustBeSpecified);
            
            // Create an object that will generate the command text
            QueryOperationBlock statement = new QueryOperationBlock(query.View, SecurityChecksDisabled);
            
            // Maintain a list of parameters
            List<CommandFragmentParameter> parameters = new List<CommandFragmentParameter>();
            
            // Enumerate through each parameter
            foreach(LearningStoreQueryParameterValue queryParameterValue in query.ParameterValues)
            {
                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(queryParameterValue.Parameter.ValueType, queryParameterValue.Value,
                    parameters, out valueText))
                    // This should've been caught by the query object
                    throw new LearningComponentsInternalException("LSTR2003");

                // Add the text to the query
                statement.AddParameterValue(queryParameterValue.Parameter, valueText);
            }
            
            // Enumerate through each column
            foreach (LearningStoreViewColumn column in query.Columns)
            {
                // Add to the select clause
                statement.AppendSelect(column.Name);
            }

            // Enumerate through each condition
            foreach (LearningStoreCondition condition in query.Conditions)
            {
                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(condition.Column.ValueType, condition.ConditionValue,
                    parameters, out valueText))
                    // This should've been caught by the query object
                    throw new LearningComponentsInternalException("LSTR2005");

                // Get some text identifying the operator and condition
                string operatorText;
                if (condition.ConditionValue == null)
                {
                    if (condition.ConditionOperator == LearningStoreConditionOperator.Equal)
                        operatorText = "IS";
                    else if (condition.ConditionOperator == LearningStoreConditionOperator.NotEqual)
                        operatorText = "IS NOT";
                    else
                        throw new LearningComponentsInternalException("LSTR2010");
                }
                else
                {
                    switch (condition.ConditionOperator)
                    {
                        case LearningStoreConditionOperator.Equal:
                            operatorText = "=";
                            break;
                        case LearningStoreConditionOperator.GreaterThan:
                            operatorText = ">";
                            break;
                        case LearningStoreConditionOperator.GreaterThanEqual:
                            operatorText = ">=";
                            break;
                        case LearningStoreConditionOperator.LessThan:
                            operatorText = "<";
                            break;
                        case LearningStoreConditionOperator.LessThanEqual:
                            operatorText = "<=";
                            break;
                        case LearningStoreConditionOperator.NotEqual:
                            operatorText = "<>";
                            break;
                        default:
                            throw new LearningComponentsInternalException("LSTR2020");
                    }
                }

                // Add the text to the WHERE segment
                bool includeNull = (condition.ConditionOperator == LearningStoreConditionOperator.NotEqual) &&
                    (condition.ConditionValue != null);
                statement.AppendWhere(condition.Column.Name, operatorText, valueText, includeNull);
            }

            // Enumerate through each sort
            foreach (LearningStoreQuerySort sort in query.Sorts)
            {
                // Append to the ORDER BY
                statement.AppendOrderBy(sort.Column.Name, sort.Direction);
            }

            // Create the result
            List<LearningStoreViewColumn> columns = new List<LearningStoreViewColumn>(query.Columns);
            CommandFragmentResult result = CreateDataTableResult(columns);
            
            // Add the command
            StringBuilder statementText = new StringBuilder();
            statement.Write(statementText);
            AppendCommandFragment(statementText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(parameters),
                null, result);
        }

        /// <summary>
        /// Add an item, or update the item properties if the item already exists
        /// </summary>
        /// <param name="itemTypeName">The name of the item type to add</param>
        /// <param name="uniquePropertyValues">Property values that uniquely identify the item
        ///     to be added or updated.</param>
        /// <param name="propertyValuesToUpdate">Property values to be updated if the item is updated,
        ///     or set if the item is added.</param>
        /// <returns>A temporary <Typ>LearningStoreItemIdentifier</Typ> that identifies the
        ///     item being added or updated.
        /// </returns>
        /// <remarks>
        /// Calls <Mth>AddOrUpdateItem</Mth>(<paramref name="itemTypeName"/>, <paramref name="uniquePropertyValues"/>,
        /// <paramref name="propertyValuesToUpdate"/>, null, false).
        /// See that overload for more details.
        /// </remarks>
        public LearningStoreItemIdentifier AddOrUpdateItem(string itemTypeName,
            IDictionary<string, object> uniquePropertyValues,
            IDictionary<string, object> propertyValuesToUpdate)
        {
            return AddOrUpdateItem(itemTypeName, uniquePropertyValues,
                propertyValuesToUpdate, null, false);
        }

        /// <summary>
        /// Add an item, or update the item properties if the item already exists
        /// </summary>
        /// <param name="itemTypeName">The name of the item type to add</param>
        /// <param name="uniquePropertyValues">Property values that uniquely identify the item
        ///     to be added or updated.</param>
        /// <param name="propertyValuesToUpdate">Property values to be updated if the item is updated,
        ///     or set if the item is added.</param>
        /// <param name="propertyValuesForAdd">Property values to be set if the item is added.</param>
        /// <returns>A temporary <Typ>LearningStoreItemIdentifier</Typ> that identifies the
        ///     item being added or updated.
        /// </returns>
        /// <remarks>
        /// Calls <Mth>AddOrUpdateItem</Mth>(<paramref name="itemTypeName"/>, <paramref name="uniquePropertyValues"/>,
        /// <paramref name="propertyValuesToUpdate"/>, <paramref name="propertyValuesForAdd"/>, false).
        /// See that overload for more details.
        /// </remarks>
        public LearningStoreItemIdentifier AddOrUpdateItem(string itemTypeName,
            IDictionary<string, object> uniquePropertyValues,
            IDictionary<string, object> propertyValuesToUpdate,
            IDictionary<string,object> propertyValuesForAdd)
        {
            return AddOrUpdateItem(itemTypeName, uniquePropertyValues,
                propertyValuesToUpdate, propertyValuesForAdd, false);
        }

        /// <summary>
        /// Add an item, or update the item properties if the item already exists
        /// </summary>
        /// <param name="itemTypeName">The name of the item type to add</param>
        /// <param name="uniquePropertyValues">Property values that uniquely identify the item
        ///     to be added or updated.  The key is the property name, and the value is the value
        ///     of the property.</param>
        /// <param name="propertyValuesToUpdate">Property values to be updated if the item is updated,
        ///     or set if the item is added.  The key is the property name, and the value is the value
        ///     to be placed in the property.</param>
        /// <param name="propertyValuesForAdd">Property values to be set if the item is added.
        ///     The key is the property name, and the value is the value to be placed in the property.
        ///     If null, then no extra values will be set if the item is added.</param>
        /// <param name="requestItemId">If true, a <Typ>LearningStoreItemIdentifier</Typ> 
        ///     for the newly-added item or updated item in the store will be returned in the results.</param>
        /// <returns>A temporary <Typ>LearningStoreItemIdentifier</Typ> that identifies the
        ///     item being added or updated.  This value may be used to reference the item in other
        ///     commands in this job, but it becomes invalid after the job has been executed.  To
        ///     retrieve a <Typ>LearningStoreItemIdentifier</Typ> that is valid after
        ///     the job has been executed, call this method with <paramref name="requestItemId"/>=true,
        ///     and the <Typ>LearningStoreItemIdentifier</Typ> will be returned as one of the
        ///     results of the job.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="uniquePropertyValues"/> must contain
        ///     at least one value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="itemTypeName"/> or <paramref name="uniquePropertyValues"/>
        ///     or <paramref name="propertyValuesToUpdate"/> is a null reference.</exception>
        /// <exception cref="System.InvalidOperationException">The item type
        ///     in <paramref name="itemTypeName"/> doesn't exist in the store, a
        ///     property name passed in <paramref name="uniquePropertyValues"/>,
        ///     <paramref name="propertyValuesToUpdate"/> or <paramref name="propertyValuesForAdd"/>
        ///     doesn't exist on the item type, a property value passed in <paramref name="uniquePropertyValues"/>,
        ///     <paramref name="propertyValuesToUpdate"/> or <paramref name="propertyValuesForAdd"/> is not a valid
        ///     value for the corresponding property, or a required property value wasn't passed in.
        /// </exception>
        /// <remarks>
        /// Steps required to add or update an item in a store:<ul>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the property values that
        /// uniquely identify the item.</li>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the property values that
        /// should be set if the item exists or is added.</li>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the property values that
        /// should be set if the item is added.</li>
        /// <li>Call this method to add the "add or update item" operation to a job.</li>
        /// <li>Execute the job.  After this step completes, the updated/added item actually
        /// appears in the store.</li>
        /// </ul>
        /// If the item is added, then the values from <paramref name="uniquePropertyValues"/>,
        /// <paramref name="propertyValuesToUpdate"/>, and <paramref name="propertyValuesForAdd"/> are
        /// stored in the new item.  If the item is updated, then the values from
        /// <paramref name="propertyValuesToUpdate"/> are updated on the item.  The entries in
        /// <paramref name="uniquePropertyValues"/>, <paramref name="propertyValuesToUpdate"/>, and
        /// <paramref name="propertyValuesForAdd"/> must have values that are valid for
        /// the corresponding property.  See <Mth>AddItem</Mth> for a list of the valid property
        /// types.  Since the Id property is automatically generated and can not
        /// be changed, <paramref name="uniquePropertyValues"/>, <paramref name="propertyValuesToUpdate"/>,
        /// and <paramref name="propertyValuesForAdd"/> can not contain a value for the Id property.
        /// If an item is added and a value for a property isn't provided, then the
        /// default value for the property is used.
        /// <para/>
        /// If more than one item matches <paramref name="uniquePropertyValues"/> when the job is executed,
        /// then an <Typ>/System.InvalidOperationException</Typ> is thrown.
        /// <para/>
        /// This operation returns a <Typ>LearningStoreItemIdentifier</Typ> as a result
        /// if <paramref name="requestItemId"/> is true.  It returns nothing as a result
        /// if <paramref name="requestItemId"/> is false.
        /// </remarks>
        /// <example>
        /// The following code searches for the user item with a User key "Company\JDoe"
        /// in the store specified by "store."  If the item exists, then the name is changed
        /// to "John Doe."  If the item does not exist, then a new user item is created
        /// with the key "Company\JDoe", the name "John Doe", and an AudioLevel of 0.5:
        /// <code language="C#">
        /// // Create the unique values
        /// Dictionary&lt;string,object&gt; uniqueValues = new Dictionary&lt;string,object&gt;();
        /// uniqueValues["Key"] = "Company\JDoe";
        /// 
        /// // Create the property values to be updated
        /// Dictionary&lt;string,object&gt; updateValues = new Dictionary&lt;string,object&gt;();
        /// updateValues["Name"] = "John Doe";
        /// 
        /// // Create the property values to be set if the item is added
        /// Dictionary&lt;string,object&gt; addValues = new Dictionary&lt;string,object&gt;();
        /// addValues["AudioLevel"] = 0.5;
        ///
        /// // Create a job and add the "add or update item" operation
        /// LearningStoreJob job = store.CreateJob();
        /// job.AddOrUpdateItem("UserItem", uniqueValues, updateValues, addValues, false);
        /// 
        /// // Execute the job
        /// job.Execute();
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")] //  Much of the complexity is due to fairly simple error checking.  Not sure if it would truly be less complex if it was split into different methods...
        public LearningStoreItemIdentifier AddOrUpdateItem(string itemTypeName,
            IDictionary<string,object> uniquePropertyValues,
            IDictionary<string,object> propertyValuesToUpdate,
            IDictionary<string,object> propertyValuesForAdd,
            bool requestItemId)
        {
            // Check input parameters
            if (itemTypeName == null)
                throw new ArgumentNullException("itemTypeName");
            if (uniquePropertyValues == null)
                throw new ArgumentNullException("uniquePropertyValues");
            if (propertyValuesToUpdate == null)
                throw new ArgumentNullException("propertyValuesToUpdate");

            // Initialize propertyValuesForAdd if the user passed null
            if(propertyValuesForAdd == null)
                propertyValuesForAdd = new Dictionary<string,object>();

            // Verify that the same property value isn't included in two lists
            foreach (string propertyName in uniquePropertyValues.Keys)
            {
                if (propertyValuesToUpdate.ContainsKey(propertyName) || propertyValuesForAdd.ContainsKey(propertyName))
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.DuplicatePropertyValue, propertyName));
            }
            foreach (string propertyName in propertyValuesToUpdate.Keys)
            {
                if (propertyValuesForAdd.ContainsKey(propertyName))
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.DuplicatePropertyValue, propertyName));
            }
                        
            // Fail if there are no unique properties
            if (uniquePropertyValues.Count == 0)
                throw new ArgumentException(LearningStoreStrings.AtLeastOneValueMustBeProvided, "uniquePropertyValues");

            // Find the item type in the schema
            LearningStoreItemType itemType = null;
            if (!m_schema.TryGetItemTypeByName(itemTypeName, out itemType))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ItemTypeNotFoundInSchema, itemTypeName));

            // Create an output parameter that will contain the Id
            CommandFragmentParameter outputParameter = CreateParameter(SqlDbType.BigInt, DBNull.Value);

            // Create a list for all of the input parameters
            List<CommandFragmentParameter> inputParameters = new List<CommandFragmentParameter>();

            // Create the object that will generate the command text
            UpdateOrAddOperationBlock statement = new UpdateOrAddOperationBlock(
                itemType, outputParameter.Name, requestItemId, SecurityChecksDisabled);

            // Enumerate through each unique value that was passed to us, and add it
            // to the statement.
            foreach (KeyValuePair<string, object> propertyValue in uniquePropertyValues)
            {
                // Verify that the property name exists on the item
                LearningStoreItemTypeProperty propertyType;
                if (!itemType.TryGetPropertyByName(propertyValue.Key, out propertyType))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyNotFound, propertyValue.Key));

                // Can't use the ID property
                if (propertyType.IsId)
                    throw new InvalidOperationException(
                       LearningStoreStrings.CannotChangeIdProperty);

                // Can't use an XML property
                if (propertyType.IsXml)
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.XmlPropertyCannotBeSearchedOn, propertyType.Name));

                // Cast the passed-in value as necessary
                object value = propertyType.CastValue(propertyValue.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValueWithDescription,
                            propertyValue.Key, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(propertyType.ValueType, value,
                    inputParameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValue, propertyValue.Key));

                // Append the column to the list of unique values
                statement.AppendUniqueColumn(propertyType, (value == null) ? "IS" : "=",
                    (value == null) ? "NULL" : valueText);                    
            }

            // Enumerate through each property for update that was passed to us, and add it
            // to the statement.
            foreach (KeyValuePair<string, object> propertyValue in propertyValuesToUpdate)
            {
                // Verify that the property name exists on the item
                LearningStoreItemTypeProperty propertyType;
                if (!itemType.TryGetPropertyByName(propertyValue.Key, out propertyType))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyNotFound, propertyValue.Key));

                // Can't set the ID property
                if (propertyType.IsId)
                    throw new InvalidOperationException(LearningStoreStrings.CannotChangeIdProperty);

                // Cast the passed-in value as necessary
                object value = propertyType.CastValue(propertyValue.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValueWithDescription,
                            propertyValue.Key, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(propertyType.ValueType, value,
                    inputParameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValue, propertyValue.Key));

                // Append the column to the list of properties for update
                statement.AppendUpdateColumn(propertyType, valueText);                
            }

            // Enumerate through property for add that was passed to us, and add it
            // to the statement.
            foreach (KeyValuePair<string, object> propertyValue in propertyValuesForAdd)
            {
                // Verify that the property name exists on the item
                LearningStoreItemTypeProperty propertyType;
                if (!itemType.TryGetPropertyByName(propertyValue.Key, out propertyType))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyNotFound, propertyValue.Key));

                // Can't set the ID property
                if (propertyType.IsId)
                    throw new InvalidOperationException(LearningStoreStrings.CannotChangeIdProperty);

                // Cast the passed-in value as necessary
                object value = propertyType.CastValue(propertyValue.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValueWithDescription,
                            propertyValue.Key, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(propertyType.ValueType, value,
                    inputParameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidPropertyValue, propertyValue.Key));

                // Append the column to the list of properties for add
                statement.AppendAddColumn(propertyType, valueText);
            }

            // Verify that the user didn't forget to pass us a property
            foreach (LearningStoreItemTypeProperty property in itemType.PropertiesWithoutDefaultValue)
            {
                if (!uniquePropertyValues.ContainsKey(property.Name) && !propertyValuesToUpdate.ContainsKey(property.Name) &&
                    !propertyValuesForAdd.ContainsKey(property.Name))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.PropertyValueNotFound, property.Name));
            }

            // Create the result value
            CommandFragmentResult result = null;
            if (requestItemId)
                result = CreateItemIdentifierResult(itemType);

            // Add the command fragment
            StringBuilder statementText = new StringBuilder();
            statement.Write(statementText);
            AppendCommandFragment(statementText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(inputParameters),
                outputParameter, result);

            // Create the id that identifies the item in this job
            LearningStoreItemIdentifier id = LearningStoreItemIdentifier.CreateTemporaryItemIdentifier(itemType.Name);

            // Allow the id to be referenced in future commands in the same
            // job
            m_addedItemIds.Add(id.RawKey, outputParameter);

            return id;
        }


        /// <summary>
        /// Demand a right.  When the job is executed, the job will be
        /// aborted unless the current user has the specified right.
        /// </summary>
        /// <param name="rightName">The name of the right to demand</param>
        /// <exception cref="ArgumentNullException"><paramref name="rightName"/> is a null reference.</exception>
        /// <exception cref="System.InvalidOperationException">The right
        ///     in <paramref name="rightName"/> doesn't exist in the store.
        /// </exception>
        /// <remarks>
        /// Calls <Mth>DemandRight</Mth>(<paramref name="rightName"/>, null).
        /// See that overload for more details.
        /// </remarks>
        public void DemandRight(string rightName)
        {
            // Immediately call the other method
            DemandRight(rightName, null);
        }

        /// <summary>
        /// Demand a right.  When the job is executed, the job will be
        /// aborted unless the current user has the specified right.
        /// </summary>
        /// <param name="rightName">The name of the right to demand</param>
        /// <param name="parameterValues">Extra information used to determine if the current user
        ///     has the right, or null if no extra information is provided.  The key is the
        ///     parameter name, and the value is the value for the parameter.  If a value for a
        ///     particular parameter isn't provided, a value of null is assumed.  See the
        ///     documentation for the right specified by <paramref name="rightName"/>
        ///     for a list of parameters accepted by that right.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rightName"/>
        ///     is a null reference.</exception>
        /// <exception cref="System.InvalidOperationException">The right
        ///     in <paramref name="rightName"/> doesn't exist in the store, a
        ///     parameter name passed in <paramref name="parameterValues"/>
        ///     doesn't exist on the right, or a parameter value passed in
        ///     <paramref name="parameterValues"/> is not a valid value for the
        ///     corresponding parameter.
        /// </exception>
        /// <remarks>
        /// Steps required to demand a right:<ul>
        /// <li>Create a Dictionary&lt;string,object&gt; containing the parameter values.</li>
        /// <li>Call this method to add the "demand right" operation to a job.</li>
        /// <li>Execute the job.  If the current user has the right, then the execution
        /// succeeds.  If the current user does not have the right, then a
        /// LearningStoreSecurityException is thrown.</li>
        /// </ul>
        /// <paramref name="parameterValues"/> should contain one entry for each parameter
        /// that should have a non-null value.  The value type differs depending on
        /// the type of the property on the item.  See <Mth>../LearningStoreQuery.SetParameter</Mth>
        /// for more details.
        /// </remarks>
        /// <example>
        /// The following code demands that the user has the "ReadPackageRight" for the package
        /// specified by "packageId".  If the user has the right, then the location of the
        /// package is retrieved.  If the user doesn't have the right, then an exception is thrown.
        /// <code language="C#">
        /// // Create a job
        /// LearningStoreJob job = store.CreateJob();
        /// 
        /// // Create the parameters for the demand
        /// Dictionary&lt;string,object&gt; parameterValues = new Dictionary&lt;string,object&gt;();
        /// parameterValues["PackageId"] = packageId;
        /// 
        /// // Demand the right
        /// job.DemandRight("ReadPackageRight", parameterValues);
        /// 
        /// // Create the query that will return the location
        /// LearningStoreQuery query = store.CreateQuery("PackageItem");
        /// query.AddColumn("Location");
        /// query.AddCondition("Id", LearningStoreConditionOperator.Equal, packageId);
        /// job.PerformQuery(query);
        /// 
        /// // Execute the job
        /// DataTable result = job.Execute&lt;DataTable&gt;();
        /// </code>
        /// </example>
        public void DemandRight(string rightName, 
            IDictionary<string, object> parameterValues)
        {
            // Check input parameters
            if (rightName == null)
                throw new ArgumentNullException("rightName");

            // Initialize parameterValues if the user passed null
            if (parameterValues == null)
                parameterValues = new Dictionary<string, object>();

            // Find the right in the schema
            LearningStoreRight right = null;
            if (!m_schema.TryGetRightByName(rightName, out right))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.RightNotFoundInSchema, rightName));

            // Create an object that will generate the command text
            DemandRightOperationBlock statement = new DemandRightOperationBlock(right, SecurityChecksDisabled);
            
            // Maintain a list of parameters
            List<CommandFragmentParameter> parameters = new List<CommandFragmentParameter>();
            
            // Enumerate through each parameter
            foreach(KeyValuePair<string,object> kvp in parameterValues)
            {
                // Find the parameter in the right
                LearningStoreRightParameter parameter = null;
                if (!right.TryGetParameterByName(kvp.Key, out parameter))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ParameterNotFoundInRight, kvp.Key));

                // Cast the comparison value to the associated property type
                object value = parameter.CastValue(kvp.Value, m_locale,
                    delegate(string reason, Exception innerException)
                    {
                        return new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidParameterValueWithDescriptionAndName,
                                parameter.Name, reason), innerException);
                    }
                );

                // Get some text identifying the value
                string valueText;
                if (!TryGetTextForValue(parameter.ValueType, value, parameters, out valueText))
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidParameterValue, kvp.Key));

                // Add the text to the statement
                statement.AddParameterValue(parameter, valueText);
            }
            
            // Add the command
            StringBuilder statementText = new StringBuilder();
            statement.Write(statementText);
            AppendCommandFragment(statementText.ToString(), new ReadOnlyCollection<CommandFragmentParameter>(parameters),
                null, null);
        }

        /// <summary>
        /// Disable security checks for the operations added to the job after
        /// this call.
        /// </summary>
        /// <example>
        /// The following code demands that the user has the "ReadPackageRight" for the package
        /// specified by "packageId".  If the user has the right, then then the security checks are disabled
        /// for the rest of the job.  If the user doesn't have the right, then an exception is thrown.
        /// <code language="C#">
        /// // Create a job
        /// LearningStoreJob job = store.CreateJob();
        /// 
        /// // Create the parameters for the demand
        /// Dictionary&lt;string,object&gt; parameterValues = new Dictionary&lt;string,object&gt;();
        /// parameterValues["PackageId"] = packageId;
        /// 
        /// // Demand the right
        /// job.DemandRight("ReadPackageRight", parameterValues);
        /// 
        /// // Skip security checks for the rest of the job
        /// job.DisableFollowingSecurityChecks();
        /// 
        /// // Add other operations to the job.  No security checks will happen
        /// // on these operations.
        /// 
        /// // Execute the job
        /// job.Execute();
        /// </code>
        /// </example>
        public void DisableFollowingSecurityChecks()
        {
            m_disableSecurityChecks = true;
        }

        /// <summary>
        /// Enable security checks for the operations added to the job after
        /// this call.
        /// </summary>
        public void EnableFollowingSecurityChecks()
        {
            m_disableSecurityChecks = false;
        }

		// ----- Statements -----

        /// <summary>
        /// Represents a block that checks the security for an add operation
        /// </summary>
        private class AddSecurityCheckBlock
        {
            // Resulting block is of this form:
            // IF <TableName>$AddSecurity(@UserKey,@PropertyX,...) = 0
            // BEGIN
            //    RAISERROR(...)
            //    RETURN
            // END
            // IF @@ERROR <> 0
            //    RETURN
            
            /// <summary>
            /// Item type
            /// </summary>
            private LearningStoreItemType m_itemType;

            /// <summary>
            /// Property values
            /// </summary>
            private Dictionary<LearningStoreItemTypeProperty,string> m_propertyValues =
                new Dictionary<LearningStoreItemTypeProperty,string>();
            
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemType">Item type of the table</param>            
            public AddSecurityCheckBlock(LearningStoreItemType itemType)
            {
                m_itemType = itemType;
            }

            /// <summary>
            /// Add a property value
            /// </summary>
            /// <param name="property">Property</param>
            /// <param name="value">Text representing the property value.</param>
            public void AddPropertyValue(LearningStoreItemTypeProperty property, string value)
            {
                m_propertyValues.Add(property, value);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if(m_itemType.AddSecurityFunction != null)
                {
                    sb.Append("IF dbo.[");
                    sb.Append(m_itemType.AddSecurityFunction);                
                    sb.Append("](@UserKey");
                    foreach(LearningStoreItemTypeProperty property in m_itemType.Properties)
                    {
                        if(property.IsId)
                            continue;
                        string value;
                        m_propertyValues.TryGetValue(property, out value);
                        if(value == null)
                            sb.Append(",DEFAULT");
                        else
                        {
                            sb.Append(",");
                            sb.Append(value);
                        }
                    }
                    sb.Append(") = 0\r\n" +
                              "BEGIN\r\n" +
                              "    RAISERROR('LSERROR',16,3)\r\n" +
                              "    RETURN\r\n" +
                              "END\r\n" +
                              "IF @@ERROR <> 0\r\n" +
                              "    RETURN\r\n");
                }
                else
                {
                    sb.Append("RAISERROR('LSERROR',16,3)\r\n" +
                              "RETURN\r\n");
                }
            }
        }

        /// <summary>
        /// Represents a block that performs an add operation (not including checking security or returning
        /// the identity of the added item)
        /// </summary>
        private class AddBlock
        {
            // Resulting block is of this form:
            // INSERT INTO <tablename> ( ... ) VALUES (...)
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END
            // SELECT <identity-variable-name>=CAST(SCOPE_IDENTITY() as bigint)
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END

            /// <summary>
            /// Name of the table
            /// </summary>
            private string m_tableName;

            /// <summary>
            /// List of column names, seperated by commas
            /// </summary>
            private StringBuilder m_columnNames = new StringBuilder();

            /// <summary>
            /// List of values for each column seperated by commas
            /// </summary>
            private StringBuilder m_columnValues = new StringBuilder();
            
            /// <summary>
            /// Name of the SQL variable into which the identity of the
            /// new item should be placed
            /// </summary>
            private string m_identityVariableName;

            /// <summary>
            /// Create a new instance of the statement
            /// </summary>
            /// <param name="tableName">Name of the table</param>
            /// <param name="identityVariableName">Name of the variable into which the identity should be placed</param>
            public AddBlock(string tableName, string identityVariableName)
            {
                m_tableName = tableName;
                m_identityVariableName = identityVariableName;
            }

            /// <summary>
            /// Append a column
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <param name="columnValue">Value for the column.</param>            
            public void AppendColumn(string columnName, string columnValue)
            {
                if (m_columnNames.Length != 0)
                {
                    m_columnNames.Append(",");
                    m_columnValues.Append(",");
                }

                m_columnNames.Append("[");
                m_columnNames.Append(columnName);
                m_columnNames.Append("]");
                m_columnValues.Append(columnValue);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                sb.Append("INSERT INTO [");
                sb.Append(m_tableName);
                sb.Append("]");
                if(m_columnNames.Length == 0)
                    sb.Append(" DEFAULT VALUES\r\n");
                else
                {
                    sb.Append(" (\r\n");
                    sb.Append(m_columnNames.ToString());
                    sb.Append("\r\n) VALUES (\r\n");
                    sb.Append(m_columnValues.ToString());
                    sb.Append("\r\n)\r\n");
                }
                sb.Append(
                    "IF @@ERROR <> 0\r\n" +
                    "BEGIN\r\n" +
                    "    RETURN\r\n" +
                    "END\r\n" +
                    "SELECT ");
                sb.Append(m_identityVariableName);
                sb.Append("=CAST(SCOPE_IDENTITY() AS bigint)\r\n" +
                          "IF @@ERROR <> 0\r\n" +
                          "BEGIN\r\n" +
                          "    RETURN\r\n" +
                          "END\r\n");
            }
        }

        /// <summary>
        /// Represents a block that checks security for an add operation (optionally), performs the add operation,
        /// and then returns the identity as a result (optionally)
        /// </summary>
        private class AddOperationBlock
        {
            // Resulting block is of this form:
            // -- This piece implemented by AddSecurityCheckBlock
            // -- This piece is also optional (not included if security checks are skipped)
            // IF <TableName>$AddSecurity(@UserKey,@PropertyX,...) = 0
            // BEGIN
            //    RAISERROR(...)
            //    RETURN
            // END
            // IF @@ERROR <> 0
            //    RETURN
            // -- End AddSecurityCheckBlock
            //
            // -- This piece implemented by AddBlock
            // INSERT INTO <tablename> ( ... ) VALUES (...)
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END
            // SELECT <identity-variable-name>=CAST(SCOPE_IDENTITY() as bigint)
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END
            // -- End AddBlock
            //
            // -- Following piece is optional (not included if the identity isn't requested)
            // SELECT <identity-variable-name>
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END
            
            /// <summary>
            /// Generates AddSecurityCheckBlock piece of code, or null if the AddSecurityCheckBlock
            /// isn't needed.
            /// </summary>
            private AddSecurityCheckBlock m_securityCheckBlock;
            
            /// <summary>
            /// Generates AddBlock piece of code
            /// </summary>
            private AddBlock m_addBlock;

            /// <summary>
            /// Name of the SQL variable into which the identity of the
            /// new item should be placed
            /// </summary>
            private string m_identityVariableName;

            /// <summary>
            /// Should the identity be returned as a result set?
            /// </summary>
            private bool m_requestIdentity;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemType">Item type being added</param>
            /// <param name="identityVariableName">Name of the variable into which the identity value should be placed</param>
            /// <param name="requestIdentity">True if the identity should be returned as a result set</param>
            /// <param name="disableSecurityChecks">True if security checks should be skipped</param>
            public AddOperationBlock(LearningStoreItemType itemType,
                string identityVariableName, bool requestIdentity, bool disableSecurityChecks)
            {
                if(!disableSecurityChecks)
                    m_securityCheckBlock = new AddSecurityCheckBlock(itemType);
                m_addBlock = new AddBlock(itemType.Name, identityVariableName);
                m_identityVariableName = identityVariableName;
                m_requestIdentity = requestIdentity;
            }

            /// <summary>
            /// Append a column
            /// </summary>
            /// <param name="property">Property.</param>
            /// <param name="propertyValue">Value for the property.</param>            
            public void AppendColumn(LearningStoreItemTypeProperty property, string propertyValue)
            {
                m_addBlock.AppendColumn(property.Name, propertyValue);
                if(m_securityCheckBlock != null)
                    m_securityCheckBlock.AddPropertyValue(property, propertyValue);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if(m_securityCheckBlock != null)
                    m_securityCheckBlock.Write(sb);
                m_addBlock.Write(sb);
                if(m_requestIdentity)
                {
                    sb.Append("SELECT ");
                    sb.Append(m_identityVariableName);
                    sb.Append("\r\n" +
                              "IF @@ERROR <> 0\r\n" +
                              "BEGIN\r\n" +
                              "    RETURN\r\n" +
                              "END\r\n");
                }
            }
        }

        /// <summary>
        /// Represents a block that checks the security for an update operation
        /// </summary>
        private class UpdateSecurityCheckBlock
        {
            // Resulting block is of this form:
            // IF <TableName>$UpdateSecurity(@UserKey,@Id,@PropertyXChanged,@PropertyX,...) = 0
            // BEGIN
            //     RAISERROR(...)
            //     RETURN
            // END
            // IF @@ERROR <> 0
            //    RETURN

            /// <summary>
            /// Item type
            /// </summary>
            private LearningStoreItemType m_itemType;

            /// <summary>
            /// Text that identifies the ID of the item being updated.
            /// </summary>
            private string m_idText;

           /// <summary>
            /// Property values
            /// </summary>
            private Dictionary<LearningStoreItemTypeProperty, string> m_propertyValues =
                new Dictionary<LearningStoreItemTypeProperty, string>();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemType">Item type of the table</param>            
            /// <param name="idText">Text that identifies the ID of the item being updated</param>
            public UpdateSecurityCheckBlock(LearningStoreItemType itemType, string idText)
            {
                m_itemType = itemType;
                m_idText = idText;
            }

            /// <summary>
            /// Add a property value
            /// </summary>
            /// <param name="property">Property</param>
            /// <param name="value">Text representing the property value.</param>
            public void AddPropertyValue(LearningStoreItemTypeProperty property, string value)
            {
                m_propertyValues.Add(property, value);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if(m_itemType.UpdateSecurityFunction != null)
                {
                    sb.Append("IF dbo.[");
                    sb.Append(m_itemType.UpdateSecurityFunction);
                    sb.Append("](@UserKey,");
                    sb.Append(m_idText);
                    foreach (LearningStoreItemTypeProperty property in m_itemType.Properties)
                    {
                        if (property.IsId)
                            continue;
                        string value;
                        m_propertyValues.TryGetValue(property, out value);
                        if (value == null)
                            sb.Append(",0,NULL");
                        else
                        {
                            sb.Append(",1,");
                            sb.Append(value);
                        }
                    }
                    sb.Append(") = 0\r\n" +
                              "BEGIN\r\n" +
                              "    RAISERROR('LSERROR',16,3)\r\n" +
                              "    RETURN\r\n" +
                              "END\r\n" +
                              "IF @@ERROR <> 0\r\n" +
                              "    RETURN\r\n");
                }
                else
                {
                    sb.Append("RAISERROR('LSERROR',16,3)\r\n" +
                              "RETURN\r\n");
                }
            }
        }

        /// <summary>
        /// Represents a block that performs an update operation (not including checking security)
        /// </summary>
        private class UpdateBlock
        {
            // Resulting block is of this form:
            // UPDATE <tablename>
            // SET <column>=<value>
            // WHERE Id=<id>
            // SELECT @LastError = @@ERROR, @LastRowCount = @@ROWCOUNT
            // IF @LastError<>0
            // BEGIN
            //    RETURN
            // END
            // IF @LastRowCount<>0
            // BEGIN
            //    RAISERROR(...)
            //    RETURN
            // END

            /// <summary>
            /// Name of the table
            /// </summary>
            private string m_tableName;

            /// <summary>
            /// Text that identifies the ID of the item being updated.
            /// </summary>
            private string m_idText;

            /// <summary>
            /// Text of all the column=value values seperated by commas
            /// </summary>
            private StringBuilder m_assignments = new StringBuilder();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="tableName">Name of the table</param>
            /// <param name="idText">Text that identifies the ID of the item being updated</param>
            public UpdateBlock(string tableName, string idText)
            {
                m_tableName = tableName;
                m_idText = idText;
            }

            /// <summary>
            /// Append a column
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <param name="columnValue">Value for the column.</param>            
            public void AppendColumn(string columnName, string columnValue)
            {
                if(m_assignments.Length != 0)
                {
                    m_assignments.Append(",");
                }
                m_assignments.Append("[");                
                m_assignments.Append(columnName);
                m_assignments.Append("]=");
                m_assignments.Append(columnValue);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                sb.Append("UPDATE [");
                sb.Append(m_tableName);
                sb.Append("]\r\n" +
                          "SET ");
                sb.Append(m_assignments.ToString());
                sb.Append("\r\n" +
                          "WHERE Id=");
                sb.Append(m_idText);
                sb.Append("\r\n" +
                          "SELECT @LastError = @@ERROR, @LastRowCount = @@ROWCOUNT\r\n" +
                          "IF @LastError <> 0\r\n" +
                          "BEGIN\r\n" +
                          "    RETURN\r\n" +
                          "END\r\n" +
                          "IF @LastRowCount = 0\r\n" +
                          "BEGIN\r\n" +
                          "    RAISERROR('LSERROR',16,1)\r\n" +
                          "    RETURN\r\n" +
                          "END\r\n");
            }
        }

        /// <summary>
        /// Represents a block that checks security for an update operation (optionally) and then performs the update operation
        /// </summary>
        private class UpdateOperationBlock
        {
            // Resulting block is of this form:
            // -- This piece implemented by UpdateSecurityCheckBlock
            // -- This piece is also optional (not included if security checks are skipped)
            // IF <TableName>$UpdateSecurity(@UserKey,@Id,@PropertyXChanged,@PropertyX,...) = 0
            // BEGIN
            //     RAISERROR(...)
            //     RETURN
            // END
            // IF @@ERROR <> 0
            //    RETURN
            // -- End UpdateSecurityCheckBlock
            //
            // -- This piece implemented by UpdateBlock
            // UPDATE <tablename>
            // SET <column>=<value>
            // WHERE Id=<id>
            // SELECT @LastError = @@ERROR, @LastRowCount = @@ROWCOUNT
            // IF @LastError<>0
            // BEGIN
            //    RETURN
            // END
            // IF @LastRowCount<>0
            // BEGIN
            //    RAISERROR(...)
            //    RETURN
            // END
            // -- End UpdateBlock

            /// <summary>
            /// Generates UpdateSecurityCheckBlock piece of code, or null if the UpdateSecurityCheckBlock
            /// isn't needed.
            /// </summary>
            private UpdateSecurityCheckBlock m_securityCheckBlock;

            /// <summary>
            /// Generates UpdateBlock piece of code
            /// </summary>
            private UpdateBlock m_updateBlock;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemType">Item type being updated</param>
            /// <param name="idText">Text identifying the id of the item being updated</param>
            /// <param name="disableSecurityChecks">True if security checks should be skipped</param>
            public UpdateOperationBlock(LearningStoreItemType itemType,
                string idText, bool disableSecurityChecks)
            {
                if (!disableSecurityChecks)                
                    m_securityCheckBlock = new UpdateSecurityCheckBlock(itemType, idText);
                m_updateBlock = new UpdateBlock(itemType.Name, idText);
            }

            /// <summary>
            /// Append a column
            /// </summary>
            /// <param name="property">Property.</param>
            /// <param name="propertyValue">Value for the property.</param>            
            public void AppendColumn(LearningStoreItemTypeProperty property, string propertyValue)
            {
                m_updateBlock.AppendColumn(property.Name, propertyValue);
                if (m_securityCheckBlock != null)
                    m_securityCheckBlock.AddPropertyValue(property, propertyValue);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if (m_securityCheckBlock != null)
                    m_securityCheckBlock.Write(sb);
                m_updateBlock.Write(sb);
            }
        }


        /// <summary>
        /// Represents a block that performs an update operation (not including checking security),
        /// does not verify that the item was updated
        /// </summary>
        private class UpdateBlock2
        {
            // Resulting block is of this form:
            // UPDATE <tablename>
            // SET <column>=<value>
            // WHERE Id=<id>
            // IF @@ERROR<>0
            // BEGIN
            //    RETURN
            // END

            /// <summary>
            /// Name of the table
            /// </summary>
            private string m_tableName;

            /// <summary>
            /// Text that identifies the ID of the item being updated.
            /// </summary>
            private string m_idText;

            /// <summary>
            /// Text of all the column=value values seperated by commas
            /// </summary>
            private StringBuilder m_assignments = new StringBuilder();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="tableName">Name of the table</param>
            /// <param name="idText">Text that identifies the ID of the item being updated</param>
            public UpdateBlock2(string tableName, string idText)
            {
                m_tableName = tableName;
                m_idText = idText;
            }

            /// <summary>
            /// Append a column
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <param name="columnValue">Value for the column.</param>            
            public void AppendColumn(string columnName, string columnValue)
            {
                if(m_assignments.Length != 0)
                {
                    m_assignments.Append(",");
                }
                m_assignments.Append("[");                
                m_assignments.Append(columnName);
                m_assignments.Append("]=");
                m_assignments.Append(columnValue);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if(m_assignments.Length == 0)
                {
                    // Nothing to update
                    return;
                }
                
                sb.Append("UPDATE [");
                sb.Append(m_tableName);
                sb.Append("]\r\n" +
                          "SET ");
                sb.Append(m_assignments.ToString());
                sb.Append("\r\n" +
                          "WHERE Id=");
                sb.Append(m_idText);
                sb.Append("\r\n" +
                          "IF @@ERROR <> 0\r\n" +
                          "BEGIN\r\n" +
                          "    RETURN\r\n" +
                          "END\r\n");
            }
        }

        /// <summary>
        /// Represents a block that checks security for an update or add operation (optionally), performs the update or add operation,
        /// and then returns the identity as a result (optionally)
        /// </summary>
        private class UpdateOrAddOperationBlock
        {
            // Resulting block is of this form:            
            // SELECT <identityVariableName> = Id
            // FROM <tableName> WITH(SERIALIZABLE)
            // WHERE (<uniqueColumn> = <uniqueValue>) AND
            //       (<uniqueColumn2> = <uniqueValue2>) ...
            // SELECT @LastRowCount = @@ROWCOUNT, @LastError = @@ERROR
            // IF @LastError <> 0
            // BEGIN
            //     RETURN
            // END
            // ELSE IF @LastRowCount = 0
            // BEGIN
            //     -- This piece is implemented by AddSecurityCheckBlock
            //     -- This piece is also optional (not included if security checks are skipped)
            //     IF <ItemName>$AddSecurity(@UserKey,@PropertyX,...) = 0
            //     BEGIN
            //         RAISERROR(...)
            //     END
            //     IF @@ERROR <> 0
            //        RETURN
            //     -- End AddSecurityCheckBlock
            //     -- This piece is implemented by AddBlock
            //     INSERT INTO <tablename> ( ... ) VALUES (...)
            //     IF @@ERROR<>0
            //     BEGIN
            //        RETURN
            //     END
            //     SELECT <identityVariableName>=CAST(SCOPE_IDENTITY() as bigint)
            //     IF @@ERROR<>0
            //     BEGIN
            //        RETURN
            //     END
            //     -- End AddBlock
            // END
            // ELSE
            // BEGIN
            //     -- This piece is implemented by UpdateSecurityCheckBlock
            //     -- This piece is also optional (not included if security checks are skipped)
            //     IF <ItemName>$UpdateSecurity(@UserKey,@Id,@PropertyXChanged,@PropertyX,...) = 0
            //     BEGIN
            //         RAISERROR(...)
            //     END
            //     IF @@ERROR <> 0
            //        RETURN
            //     -- End UpdateSecurityCheckBlock
            //     IF @LastRowCount > 1
            //     BEGIN
            //         RAISERROR ...
            //         RETURN
            //     END
            //     -- This piece is implemented by UpdateBlock2
            //     UPDATE <tablename>
            //     SET <column>=<value>
            //     WHERE Id=<id>
            //     IF @@ERROR<>0
            //     BEGIN
            //        RETURN
            //     END
            //     -- End UpdateBlock2
            // END
            // -- If requestIdentity=true
            // SELECT <identityVariableName>
            // IF @@ERROR<>0
            // BEGIN
            //     RETURN
            // END

            /// <summary>
            /// Generates AddSecurityCheckBlock piece of code, or null if the AddSecurityCheckBlock
            /// isn't needed.
            /// </summary>
            private AddSecurityCheckBlock m_addSecurityCheckBlock;

            /// <summary>
            /// Generates UpdateSecurityCheckBlock piece of code, or null if the UpdateSecurityCheckBlock
            /// isn't needed.
            /// </summary>
            private UpdateSecurityCheckBlock m_updateSecurityCheckBlock;

            /// <summary>
            /// Generates AddBlock piece of code
            /// </summary>
            private AddBlock m_addBlock;

            /// <summary>
            /// Generates UpdateBlock2 piece of code
            /// </summary>
            private UpdateBlock2 m_updateBlock;

            /// <summary>
            /// Item type
            /// </summary>
            private LearningStoreItemType m_itemType;

            /// <summary>
            /// Name of the SQL variable into which the identity of the
            /// new/updated item should be placed
            /// </summary>
            private string m_identityVariableName;

            /// <summary>
            /// Should the identity be returned as a result set?
            /// </summary>
            private bool m_requestIdentity;

            /// <summary>
            /// Text of the checks related to the unique values in the WHERE clause, seperated by "AND"s
            /// </summary>
            private StringBuilder m_whereChecks = new StringBuilder();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="itemType">Item type being updated</param>
            /// <param name="identityVariableName">Name of the variable into which the identity value should be placed</param>
            /// <param name="requestIdentity">True if the id should be returned as a result.</param>
            /// <param name="disableSecurityChecks">True if security checks should be skipped</param>
            public UpdateOrAddOperationBlock(LearningStoreItemType itemType,
                string identityVariableName, bool requestIdentity, bool disableSecurityChecks)
            {                
                if (!disableSecurityChecks)
                {
                    m_addSecurityCheckBlock = new AddSecurityCheckBlock(itemType);
                    m_updateSecurityCheckBlock = new UpdateSecurityCheckBlock(itemType, identityVariableName);
                }
                m_addBlock = new AddBlock(itemType.Name, identityVariableName);
                m_updateBlock = new UpdateBlock2(itemType.Name, identityVariableName);
                m_itemType = itemType;
                m_identityVariableName = identityVariableName;
                m_requestIdentity = requestIdentity;
            }

            /// <summary>
            /// Append a unique column
            /// </summary>
            /// <param name="property">Property.</param>
            /// <param name="op">Operator</param>
            /// <param name="propertyValue">Value for the property.</param>            
            public void AppendUniqueColumn(LearningStoreItemTypeProperty property, string op, string propertyValue)
            {
                if(m_whereChecks.Length != 0)
                    m_whereChecks.Append(" AND ");
                m_whereChecks.Append("[");
                m_whereChecks.Append(property.Name);
                m_whereChecks.Append("] ");
                m_whereChecks.Append(op);
                m_whereChecks.Append(" ");
                m_whereChecks.Append(propertyValue);
                
                if(m_addSecurityCheckBlock != null)
                    m_addSecurityCheckBlock.AddPropertyValue(property, propertyValue);
                m_addBlock.AppendColumn(property.Name, propertyValue);                
            }

            /// <summary>
            /// Append a column for update
            /// </summary>
            /// <param name="property">Property</param>
            /// <param name="propertyValue">Value for the property</param>
            public void AppendUpdateColumn(LearningStoreItemTypeProperty property, string propertyValue)
            {
                if(m_addSecurityCheckBlock != null)
                    m_addSecurityCheckBlock.AddPropertyValue(property, propertyValue);
                if(m_updateSecurityCheckBlock != null)
                    m_updateSecurityCheckBlock.AddPropertyValue(property, propertyValue);
                m_addBlock.AppendColumn(property.Name, propertyValue);
                m_updateBlock.AppendColumn(property.Name, propertyValue);
            }

            /// <summary>
            /// Append a column for add
            /// </summary>
            /// <param name="property">Property</param>
            /// <param name="propertyValue">Value for the property</param>            
            public void AppendAddColumn(LearningStoreItemTypeProperty property, string propertyValue)
            {
                if (m_addSecurityCheckBlock != null)
                    m_addSecurityCheckBlock.AddPropertyValue(property, propertyValue);
                m_addBlock.AppendColumn(property.Name, propertyValue);
            }
            
            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                sb.Append("SELECT ");
                sb.Append(m_identityVariableName);
                sb.Append(" = Id\r\n" +
                          "FROM [");
                sb.Append(m_itemType.Name);
                sb.Append("] WITH(SERIALIZABLE)\r\n");
                if(m_whereChecks.Length > 0)
                {
                    sb.Append("WHERE ");
                    sb.Append(m_whereChecks.ToString());
                    sb.Append("\r\n");
                }
                sb.Append("SELECT @LastRowCount = @@ROWCOUNT, @LastError = @@ERROR\r\n" +
                          "IF @LastError <> 0\r\n" +
                          "BEGIN\r\n" +
                          "    RETURN\r\n" +
                          "END\r\n" +
                          "ELSE IF @LastRowCount = 0\r\n" +
                          "BEGIN\r\n");
                if(m_addSecurityCheckBlock != null)
                    m_addSecurityCheckBlock.Write(sb);
                m_addBlock.Write(sb);
                sb.Append("END\r\n" +
                          "ELSE\r\n" +
                          "BEGIN\r\n");
                if(m_updateSecurityCheckBlock != null)
                    m_updateSecurityCheckBlock.Write(sb);
                sb.Append("    IF @LastRowCount > 1\r\n" +
                          "    BEGIN\r\n" +
                          "        RAISERROR('LSERROR',16,2)\r\n" +
                          "        RETURN\r\n" +
                          "    END\r\n");
                m_updateBlock.Write(sb);
                sb.Append("END\r\n");
                if(m_requestIdentity)
                {
                    sb.Append("SELECT ");
                    sb.Append(m_identityVariableName);
                    sb.Append("\r\n" +
                              "IF @@ERROR <> 0\r\n" +
                              "BEGIN\r\n" +
                              "    RETURN\r\n" +
                              "END\r\n");
                }
            }
        }

        /// <summary>
        /// Represents a block that checks security for a query operation, and then executes
        /// the query operation.
        /// </summary>
        private class QueryOperationBlock
        {
            // Resulting block is of this form:
            // IF <ViewName>$Security(@UserKey) = 0
            // BEGIN
            //     RAISERROR(...)
            //     RETURN
            // END
            // IF @@ERROR <> 0
            //     RETURN
            // SELECT ...
            // FROM <ViewName>(@UserKey)
            // WHERE ...
            // ORDER BY ...
            // IF @@Error <> 0 THEN
            // BEGIN
            //     RETURN
            // END

            /// <summary>
            /// Name of the view
            /// </summary>
            private LearningStoreView m_view;

            /// <summary>
            /// Text of the SELECT clause in the query
            /// </summary>            
            private StringBuilder m_selectClause;

            /// <summary>
            /// Text of the WHERE clause in the query
            /// </summary>
            private StringBuilder m_whereClause;

            /// <summary>
            /// Text of the ORDER BY clause in the query
            /// </summary>
            private StringBuilder m_orderByClause;

            /// <summary>
            /// Text of the additional parameter values
            /// </summary>
            private Dictionary<LearningStoreViewParameter, string> m_parameterValues =
                new Dictionary<LearningStoreViewParameter, string>();

            /// <summary>
            /// Should security checks be skipped?  If true, the "IF NOT EXISTS" is not included
            /// </summary>
            private bool m_disableSecurityChecks;
            
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="view">View</param>
            /// <param name="disableSecurityChecks">Should security checks be skipped?</param>
            public QueryOperationBlock(LearningStoreView view, bool disableSecurityChecks)
            {
                m_view = view;
                m_disableSecurityChecks = disableSecurityChecks;
            }

            /// <summary>
            /// Add a parameter value
            /// </summary>
            /// <param name="parameter">Parameter</param>
            /// <param name="value">Text representing the parameter value, or null to use the default</param>
            public void AddParameterValue(LearningStoreViewParameter parameter, string value)
            {
                m_parameterValues.Add(parameter, value);
            }

            /// <summary>
            /// Append to the SELECT clause text
            /// </summary>
            /// <param name="column">Name of column from the view</param>
            public void AppendSelect(string column)
            {
                if (m_selectClause == null)
                {
                    m_selectClause = new StringBuilder(
                        "SELECT ");
                }
                else
                    m_selectClause.Append(",\r\n");
                m_selectClause.Append("[QueryTable].[");
                m_selectClause.Append(column);
                m_selectClause.Append("]");
            }

            /// <summary>
            /// Append to the ORDER BY clause text
            /// </summary>
            /// <param name="column">Name of column from the view</param>
            /// <param name="direction">Direction</param>
            public void AppendOrderBy(string column, LearningStoreSortDirection direction)
            {
                // Create the ORDER BY clause if needed, or append the comma                 
                if (m_orderByClause == null)
                {
                    m_orderByClause = new StringBuilder(
                        "ORDER BY ");
                }
                else
                {
                    m_orderByClause.Append(
                        ",\r\n");
                }

                m_orderByClause.Append("[QueryTable].[");
                m_orderByClause.Append(column);
                m_orderByClause.Append("]");
                if (direction == LearningStoreSortDirection.Descending)
                    m_orderByClause.Append(" DESC");
            }

            /// <summary>
            /// Append to the WHERE clause text
            /// </summary>
            /// <param name="column">Column to compare.</param>
            /// <param name="compareOperator">Comparison operator</param>
            /// <param name="compareText">Comparison value</param>
            /// <param name="includeOrIsNull">Include "OR IS NULL" in the query</param>
            public void AppendWhere(string column, string compareOperator, string compareText, bool includeOrIsNull)
            {
                if (m_whereClause == null)
                {
                    m_whereClause = new StringBuilder(
                        "WHERE ");
                }
                else
                {
                    m_whereClause.Append(
                        "\r\n AND ");
                }

                m_whereClause.Append("([QueryTable].[");
                m_whereClause.Append(column);
                m_whereClause.Append("] ");
                m_whereClause.Append(compareOperator);
                m_whereClause.Append(" ");
                m_whereClause.Append(compareText);
                if(includeOrIsNull)
                {
                    m_whereClause.Append(" OR [QueryTable].[");
                    m_whereClause.Append(column);
                    m_whereClause.Append("] IS NULL");
                }
                m_whereClause.Append(")");
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                StringBuilder parameterString = new StringBuilder();
                foreach (LearningStoreViewParameter parameter in m_view.Parameters)
                {
                    string value;
                    m_parameterValues.TryGetValue(parameter, out value);
                    if (value == null)
                        parameterString.Append(",DEFAULT");
                    else
                    {
                        parameterString.Append(",");
                        parameterString.Append(value);
                    }
                }

                if (!m_disableSecurityChecks)
                {
                    if(m_view.SecurityFunction != null)
                    {
                        sb.Append("IF dbo.[");
                        sb.Append(m_view.SecurityFunction);
                        sb.Append("](@UserKey");
                        sb.Append(parameterString.ToString());
                        sb.Append(") = 0\r\n" +
                                  "BEGIN\r\n" +
                                  "    RAISERROR('LSERROR',16,3)\r\n" +
                                  "    RETURN\r\n" +
                                  "END\r\n" +
                                  "IF @@ERROR <> 0\r\n" +
                                  "    RETURN\r\n");
                    }
                    else
                    {
                        sb.Append("RAISERROR('LSERROR',16,3)\r\n" +
                                  "RETURN\r\n");
                    }
                }
                sb.Append(m_selectClause.ToString());
                sb.Append("\r\n");
                sb.Append("FROM [");
                sb.Append(m_view.Function);
                sb.Append("](@UserKey");
                sb.Append(parameterString.ToString());
                sb.Append(") QueryTable\r\n");
                if(m_whereClause != null)
                {
                    sb.Append(m_whereClause.ToString());
                    sb.Append("\r\n");
                }
                if(m_orderByClause != null)
                {
                    sb.Append(m_orderByClause.ToString());
                    sb.Append("\r\n");
                }
                sb.Append(
                    "IF @@ERROR <> 0\r\n" +
                    "BEGIN\r\n" +
                    "    RETURN\r\n" +
                    "END\r\n");
            }

        }

        /// <summary>
        /// Represents a block that executes a demand right operation
        /// </summary>
        private class DemandRightOperationBlock
        {
            // Resulting block is of this form:
            // IF <RightName>(@UserKey) = 0
            // BEGIN
            //     RAISERROR(...)
            //     RETURN
            // END
            // IF @@ERROR <> 0
            //     RETURN

            /// <summary>
            /// Right
            /// </summary>
            private LearningStoreRight m_right;

            /// <summary>
            /// Text of the additional parameter values
            /// </summary>
            private Dictionary<LearningStoreRightParameter, string> m_parameterValues =
                new Dictionary<LearningStoreRightParameter, string>();

            /// <summary>
            /// Should security checks be skipped?  If true, then the entire statement is skipped
            /// </summary>
            private bool m_disableSecurityChecks;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="right">Right</param>
            /// <param name="disableSecurityChecks">Should security checks be skipped?</param>
            public DemandRightOperationBlock(LearningStoreRight right, bool disableSecurityChecks)
            {
                m_right = right;
                m_disableSecurityChecks = disableSecurityChecks;
            }

            /// <summary>
            /// Add a parameter value
            /// </summary>
            /// <param name="parameter">Parameter</param>
            /// <param name="value">Text representing the parameter value, or null to use the default</param>
            public void AddParameterValue(LearningStoreRightParameter parameter, string value)
            {
                m_parameterValues.Add(parameter, value);
            }

            /// <summary>
            /// Write the output
            /// </summary>
            /// <param name="sb"></param>
            public void Write(StringBuilder sb)
            {
                if (!m_disableSecurityChecks)
                {
                    StringBuilder parameterString = new StringBuilder();
                    foreach (LearningStoreRightParameter parameter in m_right.Parameters)
                    {
                        string value;
                        m_parameterValues.TryGetValue(parameter, out value);
                        if (value == null)
                            parameterString.Append(",DEFAULT");
                        else
                        {
                            parameterString.Append(",");
                            parameterString.Append(value);
                        }
                    }

                    if(m_right.SecurityFunction != null)
                    {
                        sb.Append("IF dbo.[");
                        sb.Append(m_right.SecurityFunction);
                        sb.Append("](@UserKey");
                        sb.Append(parameterString.ToString());
                        sb.Append(") = 0\r\n" +
                                  "BEGIN\r\n" +
                                  "    RAISERROR('LSERROR',16,3)\r\n" +
                                  "    RETURN\r\n" +
                                  "END\r\n" +
                                  "IF @@ERROR <> 0\r\n" +
                                  "    RETURN\r\n");
                    }
                    else
                    {
                        sb.Append("RAISERROR('LSERROR',16,3)\r\n" +
                                  "RETURN\r\n");
                    }
                    
                }
            }

        }
	}
}

