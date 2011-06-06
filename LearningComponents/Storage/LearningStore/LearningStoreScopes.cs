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
using System.Threading;
using System.Transactions;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;

#endregion

/*
 * This file contains the LearningStoreTransactionScope and 
 * LearningStorePrivilegedScope classes
 * 
 * Internal error numbers: 3100-3199
 */
 
namespace Microsoft.LearningComponents.Storage
{

    /// <summary>
    /// Makes the LearningStore-related operations in a code block transactional.
    /// </summary>
    /// <remarks>
    /// By default, <Mth>../LearningStoreJob.Execute</Mth> creates a new
    /// transaction every time it is called. If <Mth>../LearningStoreJob.Execute</Mth>
    /// is called on several different jobs, then each job executes within a
    /// different transaction.  
    /// To execute multiple jobs within the same transaction, execute the jobs
    /// within a LearningStoreTransactionScope (that is, between the initialization
    /// of a LearningStoreTransactionScope object and the calling of its
    /// <Mth>Dispose</Mth> method).  The initialization method will create the
    /// transaction, and the <Mth>Dispose</Mth> method will commit or rollback
    /// the transaction.<p/>
    /// When your application completes all work it wants to perform in a scope,
    /// you should call the <Mth>Complete</Mth> method only once to specify that 
    /// it is acceptable to commit the transaction.  If this method is not called,
    /// the transaction is rolled back when the scope exits.<p/>
    /// Transaction scopes can be nested.  However, nested scopes use the
    /// transaction from the containing scope.  Any options specified when
    /// creating nested scopes must be compatible with the outermost scope.
    /// See <Mth>LearningStoreTransactionScope</Mth> for more details.<p/>
    /// LearningStoreTransactionScope is, in some ways, similar to
    /// <Typ>/System.Transactions.TransactionScope</Typ>.  The major differences
    /// include:
    /// <ul>
    /// <li>LearningStoreTransactionScope is independent of ambient transactions.  In
    ///     other words, it only affects LearningStore-related operations.</li>
    /// <li>LearningStoreTransactionScope, together with <Mth>../LearningStoreJob.Execute</Mth>,
    ///     attempts to reuse connections where possible within a transaction.  This
    ///     improves performance when multiple jobs are executed within a transaction.</li>
    /// <li>LearningStoreTransactionScope has fewer options.</li>
    /// </ul>
    /// </remarks>
    /// <example>
    /// The following code finds the user with the name "John Doe" and deletes him
    /// within a single transaction:
    /// <code language="C#">
    /// using(LearningStoreTransactionScope scope = new LearningStoreTransactionScope())
    /// {
    ///     // Create the query
    ///     LearningStoreQuery query = store.CreateQuery("UserItem");
    /// 
    ///     // Add output columns to the query
    ///     query.AddColumn("Id");
    /// 
    ///     // Add a condition
    ///     query.AddCondition("Name", LearningStoreConditionOperator.Equal, "John Doe");
    /// 
    ///     // Create a job and add the "perform query" operation
    ///     LearningStoreJob job = store.CreateJob();
    ///     job.PerformQuery(query);
    /// 
    ///     // Execute the job
    ///     DataTable result = job.Execute&lt;DataTable&gt;();
    /// 
    ///     // Get the Id
    ///     LearningStoreItemIdentifier id = (LearningStoreItemIdentifier)(result[0][0]);
    /// 
    ///     // Create a new job and add the "delete" operation
    ///     job = store.CreateJob();
    ///     job.DeleteItem(id);
    /// 
    ///     // Execute the job
    ///     job.Execute();
    /// 
    ///     // Specify that the operations have succeeded
    ///     scope.Complete();
    /// }
    /// </code>
    /// </example>
    public sealed class LearningStoreTransactionScope : IDisposable
    {
        /// <summary>
        /// Scope for the current thread
        /// </summary>
        [ThreadStatic]
        private static LearningStoreTransactionScope s_currentScope;

        /// <summary>
        /// Thread that created the scope
        /// </summary>
        private Thread m_scopeThread;
        
        /// <summary>
        /// Prior scope
        /// </summary>
        private LearningStoreTransactionScope m_priorScope;

        /// <summary>
        /// Next scope
        /// </summary>
        private LearningStoreTransactionScope m_nextScope;
        
        /// <summary>
        /// Transaction that should be committed when the scope
        /// is exited if Complete() is called on the scope.
        /// </summary>
        private CommittableTransaction m_committableTransaction;
        
        /// <summary>
        /// Transaction on which Complete() should be called
        /// when the scope is exited if Complete() is called
        /// on the scope.
        /// </summary>
        private DependentTransaction m_dependentTransaction;
        
        /// <summary>
        /// "Root" transaction for the scope
        /// </summary>
        private Transaction m_transaction;
        
        /// <summary>
        /// Connections that should be disposed when the scope
        /// is exited
        /// </summary>
        private Dictionary<string,SqlConnection> m_createdConnections;
        
        /// <summary>
        /// Connections being used
        /// </summary>
        private Dictionary<string,SqlConnection> m_connections;
        
        /// <summary>
        /// True if disposed
        /// </summary>
        private bool m_disposed;

        /// <summary>
        /// Has Complete() been called?
        /// </summary>
        private bool m_complete;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreTransactionScope</Typ> class.
        /// </summary>
        /// <remarks>
        /// See <Typ>LearningStoreTransactionScope</Typ> for more information.
        /// </remarks>
        public LearningStoreTransactionScope()
            : this(new TransactionOptions())
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreTransactionScope</Typ> class with
        /// the specified options.
        /// </summary>
        /// <param name="transactionOptions">The transaction options to use if a new transaction is created (i.e.,
        ///     if this is the outermost scope).  If a new transaction is not created (i.e., this scope is nested
        ///     inside another), then the isolation level must either be unspecified or identical to the
        ///     currently-existing transaction, and the timeout value must be zero.
        ///     </param>
        /// <exception cref="System.InvalidOperationException">The transaction options specified are invalid.</exception>
        /// <remarks>
        /// See <Typ>LearningStoreTransactionScope</Typ> for more information.
        /// </remarks>
        public LearningStoreTransactionScope(TransactionOptions transactionOptions)
        {
            m_priorScope = s_currentScope;
            m_scopeThread = Thread.CurrentThread;

            if (m_priorScope == null)
            {
                // Create new transaction

                m_committableTransaction = new CommittableTransaction(transactionOptions);
                m_transaction = m_committableTransaction;
                m_createdConnections = new Dictionary<string,SqlConnection>();
                m_connections = m_createdConnections;
            }
            else
            {
                // Transaction already exists

                // Verify that the passed-in options are compatible
                if ((transactionOptions.IsolationLevel != System.Transactions.IsolationLevel.Unspecified) &&
                   (transactionOptions.IsolationLevel != m_priorScope.Transaction.IsolationLevel))
                {
                    throw new InvalidOperationException(LearningStoreStrings.MismatchedIsolationLevel);
                }
                if (transactionOptions.Timeout != TimeSpan.Zero)
                {
                    throw new InvalidOperationException(LearningStoreStrings.TimeoutMustBeZero);
                }

                m_dependentTransaction = m_priorScope.m_transaction.DependentClone(DependentCloneOption.RollbackIfNotComplete);
                m_transaction = m_priorScope.m_transaction;
                m_connections = m_priorScope.m_connections;
            }

            // Enter the scope
            if(m_priorScope != null)
                m_priorScope.m_nextScope = this;
            s_currentScope = this;
        }
        
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreTransactionScope</Typ> class.  LearningStore
        /// jobs executed within the scope use the specified transaction.
        /// </summary>
        /// <param name="transaction">Transaction used by LearningStore operations within the scope.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transaction"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">A transaction already exists in this scope (i.e.,
        ///     this scope is nested inside another).</exception>
        /// <remarks>
        /// See <Typ>LearningStoreTransactionScope</Typ> for more information.
        /// </remarks>
        public LearningStoreTransactionScope(Transaction transaction)
        {
            // Check parameters
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            // Fail if there's already a scope
            if (s_currentScope != null)
                throw new InvalidOperationException(LearningStoreStrings.TransactionAlreadyExists);

            m_scopeThread = Thread.CurrentThread;

            // Use the transaction passed to us
            m_dependentTransaction = transaction.DependentClone(DependentCloneOption.RollbackIfNotComplete);
            m_transaction = transaction;
            m_createdConnections = new Dictionary<string,SqlConnection>();
            m_connections = m_createdConnections;

            // Enter the scope
            s_currentScope = this;
        }

        /// <summary>
        /// Ends the transaction scope.
        /// </summary>
        /// <exception cref="InvalidOperationException">A thread other than the creating thread
        ///     is disposing the object.</exception>
        /// <remarks>
        /// See <Typ>LearningStoreTransactionScope</Typ> for more information.
        /// </remarks>
        public void Dispose()
        {
            // Skip if already disposed
            if(m_disposed)
                return;

            // Fail if this is being called on a different thread
            if(m_scopeThread != Thread.CurrentThread)
                throw new InvalidOperationException(LearningStoreStrings.ScopeDisposedInInvalidThread);
                
            if(m_nextScope != null)
            {
                // There are scopes "above" this one -- which means the user called
                // Dispose out-of-order.
                
                // First dispose the scope(s) above this one
                m_nextScope.Dispose();
                if(m_nextScope != null)
                    throw new LearningComponentsInternalException("LSTR3100");
                if(s_currentScope != this)
                    throw new LearningComponentsInternalException("LSTR3110");
            }

            // Leave the scope
            if(m_priorScope != null)
                m_priorScope.m_nextScope = null;
            s_currentScope = m_priorScope;

            // Mark as disposed            
            m_disposed = true;

            // Dispose the connections if necessary
            if(m_createdConnections != null)
            {
                foreach(SqlConnection connection in m_createdConnections.Values)
                {
                    connection.Dispose();
                }
                m_createdConnections.Clear();
            }

            try
            {
                // Handle the dependent transaction if necessary
                if(m_dependentTransaction != null)
                {
                    if(m_complete)
                        m_dependentTransaction.Complete();
                    else
                        m_dependentTransaction.Rollback();
                }
                
                // Handle the committable transaction if necessary
                if(m_committableTransaction != null)
                {
                    if(m_complete)
                        m_committableTransaction.Commit();
                    else
                        m_committableTransaction.Rollback();
                }
            }
            finally
            {
                if(m_dependentTransaction != null)
                    m_dependentTransaction.Dispose();
                if(m_committableTransaction != null)
                    m_committableTransaction.Dispose();
            }
        }

        /// <summary>
        /// Current scope
        /// </summary>
        public static LearningStoreTransactionScope Current
        {
            get
            {
                return s_currentScope;
            }
        }

        /// <summary>
        /// Indicates that all operations within the scope have completed successfully. 
        /// </summary>
        /// <exception cref="InvalidOperationException">This method has already been called.</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <remarks>
        /// When you are satisfied that all operations within the scope have completed successfully,
        /// you should call this method only once to specify that the transaction can be committed.
        /// Failing to call this method aborts the transaction.<p/>
        /// See <Typ>LearningStoreTransactionScope</Typ> for more information.
        /// </remarks>
        public void Complete()
        {
            if(m_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if(m_complete)
                throw new InvalidOperationException(LearningStoreStrings.CompleteAlreadyCalled);
                                
            m_complete = true;
        }

        /// <summary>
        /// Transaction
        /// </summary>
        internal Transaction Transaction
        {
            get
            {
                return m_transaction;
            }
        }

        /// <summary>
        /// Get the connection for a particular connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Cached or new connection</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        internal SqlConnection GetConnection(string connectionString)
        {
            SqlConnection connection;
            if (m_connections.TryGetValue(connectionString, out connection))
            {
                // Connection has already been created and opened -- so just
                // use it.
                return connection;
            }

            // Connection must be created
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString);
            sb.Enlist = false;
            connection = new SqlConnection(sb.ToString());
            try
            {
                connection.Open();
                try
                {
                    connection.EnlistTransaction(m_transaction);
                }
                catch(NullReferenceException e)
                {
                    // If the database connection is lost, EnlistTransaction seems to throw a NullReferenceException.
                    // In order to give the user more information when this happens, we need to throw a new
                    // exception with additional information.
                    throw new NullReferenceException(LearningStoreStrings.NullReferenceWhenAccessingDatabase, e);
                }
                
                m_connections.Add(connectionString, connection);
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

    }

    /// <summary>
    /// Marks LearningStore-related operations in a code block as
    /// privileged, which means that security checks are not performed
    /// </summary>
    /// <remarks>
    /// To execute a set of LearningStore-related operations without security
    /// checks, add the commands to the job within a LearningStorePrivilegedScope
    /// (that is, between the initialization of a LearningStorePrivilegedScope
    /// object and the calling of its <Mth>Dispose</Mth> method).<p/>
    /// Privileged scopes can be nested.
    /// </remarks>
    /// <example>
    /// The following code adds a new user item to the store specified by "store"
    /// with security checks turned off:
    /// <code language="C#">
    /// // Create the property values
    /// Dictionary&lt;string,object&gt; propertyValues = new Dictionary&lt;string,object&gt;();
    /// propertyValues["Name"] = "John Doe";
    /// propertyValues["Key"] = "Company\JohnDoe";
    /// 
    /// // Create a job and add the "add item" operation
    /// LearningStoreJob job = store.CreateJob();
    /// 
    /// using(LearningStorePrivilegedScope scope = new LearningStorePrivilegedScope())
    /// {
    ///     // add the "add item" operation"
    ///     job.AddItem("UserItem", propertyValues, false);
    /// }
    /// 
    /// // Execute the job
    /// job.Execute();
    /// </code>
    /// </example>
    public sealed class LearningStorePrivilegedScope: IDisposable
    {
        /// <summary>
        /// Scope for the current thread
        /// </summary>
        [ThreadStatic]
        private static LearningStorePrivilegedScope s_currentScope;

        /// <summary>
        /// Prior scope
        /// </summary>
        private LearningStorePrivilegedScope m_priorScope;

        /// <summary>
        /// Next scope
        /// </summary>
        private LearningStorePrivilegedScope m_nextScope;

        /// <summary>
        /// True if disposed
        /// </summary>
        private bool m_disposed;

        /// <summary>
        /// Initializes a new instance of the <c>LearningStorePrivilegedScope</c>
        /// </summary>
        /// <remarks>
        /// See <Typ>LearningStorePrivilegedScope</Typ> for more information.
        /// </remarks>
        public LearningStorePrivilegedScope()
        {
            m_priorScope = s_currentScope;

            // Enter the scope
            if (m_priorScope != null)
                m_priorScope.m_nextScope = this;
            s_currentScope = this;
        }

        /// <summary>
        /// Ends the privileged scope.
        /// </summary>
        /// <remarks>
        /// See <Typ>LearningStorePrivilegedScope</Typ> for more information.
        /// </remarks>
        public void Dispose()
        {
            // Skip if already disposed
            if (m_disposed)
                return;

            if (m_nextScope != null)
            {
                // There are scopes "above" this one -- which means the user called
                // Dispose out-of-order.

                // First dispose the scope(s) above this one
                m_nextScope.Dispose();
                if (m_nextScope != null)
                    throw new LearningComponentsInternalException("LSTR3120");
                if (s_currentScope != this)
                    throw new LearningComponentsInternalException("LSTR3130");
            }

            // Leave the scope
            if (m_priorScope != null)
                m_priorScope.m_nextScope = null;
            s_currentScope = m_priorScope;

            // Mark as disposed            
            m_disposed = true;
        }

        /// <summary>
        /// Current scope
        /// </summary>
        public static LearningStorePrivilegedScope Current
        {
            get
            {
                return s_currentScope;
            }
        }

    }
}
