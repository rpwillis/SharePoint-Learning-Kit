/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;

#endregion

/*
 * This file contains the core of the LearningStoreQuery class
 * 
 * Internal error numbers: 2200-2399
 */
 
namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Specifies how to sort the results of a query
    /// </summary>
    public enum LearningStoreSortDirection
    {
        /// <summary>
        /// Ascending
        /// </summary>
        Ascending = 0,

        /// <summary>
        /// Descending
        /// </summary>
        Descending,
    };

    /// <summary>
    /// Condition operator
    /// </summary>
    public enum LearningStoreConditionOperator
    {
        /// <summary>
        /// Condition evaluates to "true" if the value is
        /// equal to a constant.
        /// </summary>
        Equal = 0,

        /// <summary>
        /// Condition evaluates to "true" if the value is
        /// greater than a constant.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Condition evaluates to "true" if the value is
        /// greater than or equal to a constant.
        /// </summary>
        GreaterThanEqual,

        /// <summary>
        /// Condition evaluates to "true" if the value is 
        /// less than a constant.
        /// </summary>
        LessThan,

        /// <summary>
        /// Condition evaluates to "true" if the value is
        /// less then or equal to a constant.
        /// </summary>
        LessThanEqual,

        /// <summary>
        /// Condition evaluates to "true" if the value is
        /// not equal to a constant.
        /// </summary>
        NotEqual,
    };

    /// <summary>
    /// Represents a condition in a LearningStoreQuery
    /// </summary>
    internal class LearningStoreCondition
    {
        /// <summary>
        /// Column that should be compared
        /// </summary>
        private LearningStoreViewColumn m_column;

        /// <summary>
        /// Comparison operator
        /// </summary>
        private LearningStoreConditionOperator m_conditionOperator;

        /// <summary>
        /// Value to be compared against
        /// </summary>
        private object m_conditionValue;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreCondition</Typ> class.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="conditionOperator"></param>
        /// <param name="conditionValue"></param>
        public LearningStoreCondition(LearningStoreViewColumn column,
            LearningStoreConditionOperator conditionOperator,
            object conditionValue)
        {
            // Check the parameters
            if (column == null)
                throw new LearningComponentsInternalException("LSTR2200");
            if ((conditionOperator != LearningStoreConditionOperator.Equal) &&
                (conditionOperator != LearningStoreConditionOperator.GreaterThan) &&
                (conditionOperator != LearningStoreConditionOperator.GreaterThanEqual) &&
                (conditionOperator != LearningStoreConditionOperator.LessThan) &&
                (conditionOperator != LearningStoreConditionOperator.LessThanEqual) &&
                (conditionOperator != LearningStoreConditionOperator.NotEqual))
                throw new LearningComponentsInternalException("LSTR2210");

            m_column = column;
            m_conditionOperator = conditionOperator;
            m_conditionValue = conditionValue;
        }

        /// <summary>
        /// Column that should be compared
        /// </summary>
        public LearningStoreViewColumn Column
        {
            get
            {
                return m_column;
            }
        }

        /// <summary>
        /// Comparison operator
        /// </summary>
        public LearningStoreConditionOperator ConditionOperator
        {
            get
            {
                return m_conditionOperator;
            }
        }

        /// <summary>
        /// Value to be compared against
        /// </summary>
        public object ConditionValue
        {
            get
            {
                return m_conditionValue;
            }
        }
    }

    /// <summary>
    /// Represents a parameter value in a LearningStoreQuery
    /// </summary>
    internal class LearningStoreQueryParameterValue
    {
        /// <summary>
        /// Parameter
        /// </summary>
        private LearningStoreViewParameter m_parameter;

        /// <summary>
        /// Value
        /// </summary>
        private object m_value;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreQueryParameter</Typ> class.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value"></param>
        public LearningStoreQueryParameterValue(LearningStoreViewParameter parameter,
            object value)
        {
            // Check the parameters
            if (parameter == null)
                throw new LearningComponentsInternalException("LSTR2215");

            m_parameter = parameter;
            m_value = value;
        }

        /// <summary>
        /// Parameter
        /// </summary>
        public LearningStoreViewParameter Parameter
        {
            get
            {
                return m_parameter;
            }
        }

        /// <summary>
        /// Value
        /// </summary>
        public object Value
        {
            get
            {
                return m_value;
            }
        }
    }

    /// <summary>
    /// Represents a sort in a LearningStoreQuery
    /// </summary>
    internal class LearningStoreQuerySort
    {
        /// <summary>
        /// Column that should be sorted
        /// </summary>
        private LearningStoreViewColumn m_column;

        /// <summary>
        /// Direction of sort
        /// </summary>
        private LearningStoreSortDirection m_direction;

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreQuerySort</Typ> class.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        public LearningStoreQuerySort(LearningStoreViewColumn column, LearningStoreSortDirection direction)
        {
            // Check the parameters
            if (column == null)
                throw new LearningComponentsInternalException("LSTR2220");

            m_column = column;
            m_direction = direction;
        }

        /// <summary>
        /// Column that should be sorted
        /// </summary>
        public LearningStoreViewColumn Column
        {
            get
            {
                return m_column;
            }
        }

        /// <summary>
        /// Direction of sort
        /// </summary>
        public LearningStoreSortDirection Direction
        {
            get
            {
                return m_direction;
            }
        }
    }

    /// <summary>
    /// Represents a query that can be used to retrieve information from the database.
    /// </summary>
    /// <remarks>
    /// Steps required to perform a query on a store:<ol>
    /// <li>Create a <Typ>LearningStoreQuery</Typ> object on the store using
    /// <Mth>../LearningStore.CreateQuery</Mth>.</li>
    /// <li>Call methods on the <Typ>LearningStoreQuery</Typ> to finish defining the query.
    /// Add parameter values, output columns, conditions, and/or sorting information.</li>
    /// <li>Call <Mth>../LearningStoreJob.PerformQuery</Mth> to add the "perform query"
    /// operation to a job on the store.</li>
    /// <li>Execute the job.  A <Typ>/System.Data.DataTable</Typ> is returned
    /// as a result.  See the <Mth>../LearningStoreJob.PerformQuery</Mth> documentation
    /// for more information about the results returned.</li>
    /// </ol>
    /// <p/>
    /// The definition of an query includes:<ul>
    /// <li>A view or item type that has been defined in the schema.  This is specified when calling
    /// <Mth>../LearningStore.CreateQuery</Mth>.  If an item type
    /// is specified, then the default view for the item type is used.</li>
    /// <li>A value for each parameter defined in the view.  These are set by calling
    /// the <Mth>SetParameter</Mth> method.  If a parameter isn't set, it will have
    /// a value of null.</li>
    /// <li>A set of output columns.  These are added to a query by calling the
    /// <Mth>AddColumn</Mth> method.  For a view, these columns were specified in the
    /// schema.  For an item type, one column exists for each property on the item
    /// type.</li>
    /// <li>A set of conditions.  These are added to the query by calling the
    /// <Mth>AddCondition</Mth> method.  The conditions are "anded" together.</li>
    /// <li>A set of sort operations.  These are added to the query by calling
    /// the <Mth>AddSort</Mth> method.</li>
    /// </ul>
    /// Here's a very simple query:
    /// <code language="C#">
    /// LearningStoreQuery query = store.CreateQuery("UserItem");
    /// query.AddColumn("Name");
    /// </code>
    /// The query above will return the names of all the users in the store.
    /// The output DataTable will contain one row for each UserItem.  The output
    /// DataTable will contain one column named "Name."  That column will
    /// contain the value of the "Name" property on the corresponding UserItem.
    /// Here's a more complicated query:
    /// <code language="C#">
    /// LearningStoreQuery query = store.CreateQuery("UserItem");
    /// query.AddColumn("Id");
    /// query.AddColumn("Name");
    /// query.AddCondition("Name", LearningStoreConditionOperator.LessThan, "Joe");
    /// query.AddSort("Name", LearningStoreSortDirection.Ascending);
    /// </code>
    /// This query will return the Ids and names of all the users having a
    /// name less than "Joe."  The DataTable will again contain one row for each
    /// UserItem.  The DataTable will contain two columns.  The first
    /// column will be named "Id" and will contain the Id of the corresponding
    /// UserItem.  The second column will be named "Name" and will
    /// contain the Name of the corresponding UserItem.  The users will be
    /// sorted in ascending order by their name.
    /// </remarks>
    /// 
    public class LearningStoreQuery
    {
        // "Converting" a LearningStoreQuery into a SQL statement (and related
        // information) takes place in two phases:
        //    1) The information provided by the user (e.g., by calling
        //       AddColumn) is converted into an intermediate format stored
        //       in this object.  This phase is performed by the LearningStoreQuery
        //       object as methods are called on it.
        //    2) The information from the intermediate format is used to
        //       construct the SQL statement (and related information).  This
        //       phase is performed by the PerformQuery method of the 
        //       LearningStoreJob object.  Additional details about that phase
        //       are described in that location.

        /// <summary>
        /// Store on which the query object was created
        /// </summary>
        private LearningStore m_store;

        /// <summary>
        /// View for the query
        /// </summary>        
        private LearningStoreView m_view;

        /// <summary>
        /// Locale used for conversion to/from strings
        /// </summary>
        private CultureInfo m_locale;

        /// <summary>
        /// Columns in the query
        /// </summary>        
        private List<LearningStoreViewColumn> m_columns = new List<LearningStoreViewColumn>();

        /// <summary>
        /// Sorts in the query
        /// </summary>
        private List<LearningStoreQuerySort> m_sorts = new List<LearningStoreQuerySort>();
        
        /// <summary>
        /// Parameters in the query
        /// </summary>
        private List<LearningStoreQueryParameterValue> m_parameterValues = new List<LearningStoreQueryParameterValue>();
        
        /// <summary>
        /// Conditions in the query
        /// </summary>
        private List<LearningStoreCondition> m_conditions = new List<LearningStoreCondition>();

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreQuerySort</Typ> class.
        /// </summary>
        /// <param name="store">Store on which the query is created on.</param>
        /// <param name="view">View for the query.</param>
        /// <param name="locale">Locale used for conversion to/from strings.</param>
        internal LearningStoreQuery(LearningStore store, LearningStoreView view,
            CultureInfo locale)
        {
            // Check parameters
            if (store == null)
                throw new LearningComponentsInternalException("LSTR2230");
            if (view == null)
                throw new LearningComponentsInternalException("LSTR2240");
            if (locale == null)
                throw new LearningComponentsInternalException("LSTR2250");
                
            // Save the simple info
            m_store = store;
            m_view = view;
            m_locale = locale;
        }
        
        /// <summary>
        /// Add a new output column in the result.
        /// </summary>
        /// <param name="columnName">Name of the column that should be returned in the result.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">
        ///     The column has already been added, or the column name wasn't found in the view.</exception>
        /// <remarks>
        /// See the <Typ>LearningStoreQuery</Typ> documentation for more
        /// information about creating and executing queries.
        /// </remarks>
        /// <example>
        /// The following code creates a new query and adds two
        /// output columns:
        /// <code language="C#">
        /// LearningStoreQuery query = store.CreateQuery("UserItem");
        /// query.AddColumn("Id");
        /// query.AddColumn("Name");
        /// </code>
        /// The result of the query will be a DataTable with two
        /// columns: "Id" (containing the Id of the user) and
        /// "Name" (containing the name of the user).  One row
        /// will exist in the table for each user.
        /// </example>
        public void AddColumn(string columnName)
        {            
            // Check parameters
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            // Find the column in the view
            LearningStoreViewColumn column = null;
            if (!m_view.TryGetColumnByName(columnName, out column))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ColumnNotFound, columnName));
            
            // Verify that the column name doesn't already exist
            if(m_columns.Contains(column))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ColumnAlreadyExistsInQuery,
                        columnName));

            // Add the column
            m_columns.Add(column);
        }

        /// <summary>
        /// Sets the value for a parameter in a query.
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="value">Value of the parameter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameterName"/> is a null reference.</exception>
        /// <exception cref="InvalidOperationException">Invalid
        ///     <paramref name="value"/> value.</exception>
        /// <remarks>
        /// See the <Typ>LearningStoreQuery</Typ> documentation for more
        /// information about creating and executing queries.
        /// <p/>
        /// <paramref name="value"/> must contain a value
        ///     that is valid based on the type of the parameter specified by
        ///     <paramref name="parameterName"/>:
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
        /// <p/>
        /// <paramref name="value"/> must not contain a <Typ>LearningStoreItemIdentifier</Typ> returned
        ///     from <Mth>../LearningStoreJob.AddItem</Mth>.
        /// </remarks>
        public void SetParameter(string parameterName, object value)
        {
            // Check input parameters
            if (parameterName == null)
                throw new ArgumentNullException("parameterName");

            // Find the parameter in the view
            LearningStoreViewParameter parameter = null;
            if (!m_view.TryGetParameterByName(parameterName, out parameter))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ParameterNotFound, parameterName));

            // Cast the comparison value to the associated property type
            value = parameter.CastValue(value, m_locale,
                delegate(string reason, Exception innerException)
                {
                    return new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidParameterValueWithDescription,
                        reason), innerException);
                }
            );

            // Verify special rules for LearningStoreItemIdentifier properties
            LearningStoreItemIdentifier idValue = value as LearningStoreItemIdentifier;
            if ((idValue != null) &&
               (!idValue.HasKey))
                throw new InvalidOperationException(
                     LearningStoreStrings.InvalidIdParameterValue);

            // Add the value
            int existingParameterValueIndex = m_parameterValues.FindIndex(delegate(LearningStoreQueryParameterValue existingParameterValue) {
                return Object.ReferenceEquals(existingParameterValue.Parameter, parameter);
            });
            if(existingParameterValueIndex == -1)
                m_parameterValues.Add(new LearningStoreQueryParameterValue(parameter, value));
            else
                m_parameterValues[existingParameterValueIndex] = new LearningStoreQueryParameterValue(parameter, value);
        }

        /// <summary>
        /// Add a condition that restricts the data returned in a query.
        /// </summary>
        /// <param name="columnName">Name of the column to be compared</param>
        /// <param name="conditionOperator">Condition operator</param>
        /// <param name="conditionValue">Value to be compared against.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="conditionOperator"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Invalid comparison, the column name wasn't found in the view, or an invalid
        ///     <paramref name="conditionValue"/> value.</exception>
        /// <remarks>
        /// See the <Typ>LearningStoreQuery</Typ> documentation for more
        /// information about creating and executing queries.
        /// <p/>
        /// <paramref name="conditionValue"/> must contain a value
        ///     that is valid based on the type of the column specified by
        ///     <paramref name="columnName"/>:
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
        /// <p/>
        /// <paramref name="conditionValue"/> must not contain a <Typ>LearningStoreItemIdentifier</Typ> returned
        ///     from <Mth>../LearningStoreJob.AddItem</Mth>.
        /// <p/>
        /// If <paramref name="columnName"/> refers to an XML column, then the following restrictions apply:
        /// <ul>
        /// <li><paramref name="conditionOperator"/> must be <Fld>../LearningStoreConditionOperator.Equal</Fld> or
        ///     <Fld>../LearningStoreConditionOperator.NotEqual</Fld></li>
        /// <li><paramref name="conditionValue"/> must be null.</li>
        /// </ul>
        /// <p/>
        /// If <paramref name="columnName"/> refers to an Guid column, then the following restrictions apply:
        /// <ul>
        /// <li><paramref name="conditionOperator"/> must be <Fld>../LearningStoreConditionOperator.Equal</Fld> or
        ///     <Fld>../LearningStoreConditionOperator.NotEqual</Fld></li>
        /// </ul>
        /// <p/>
        /// If <paramref name="conditionOperator"/> is <Fld>../LearningStoreConditionOperator.GreaterThan</Fld>,
        ///     <Fld>../LearningStoreConditionOperator.GreaterThanEqual</Fld>,
        ///     <Fld>../LearningStoreConditionOperator.LessThan</Fld>, or 
        ///     <Fld>../LearningStoreConditionOperator.LessThanEqual</Fld>, then
        ///     <paramref name="conditionValue"/> must not contain null.
        /// <p/>
        /// Care must be taken when null is involved in a comparison.  In
        /// particular:<ul>
        /// <li>null == null: True</li>
        /// <li>null != null: False</li>
        /// <li>null == any other value: False</li>
        /// <li>null != any other value: True</li>
        /// <li>null &lt; any other value: False</li>
        /// <li>null &gt; any other value: False</li>
        /// </ul>
        /// </remarks>
        /// <example>
        /// The following code creates a new query that returns the id and name
        /// of the users with a name less than "Joe":
        /// <code language="C#">
        /// LearningStoreQuery query = store.CreateQuery("UserItem");
        /// query.AddColumn("Id");
        /// query.AddColumn("Name");
        /// query.AddCondition("Name", LearningStoreConditionOperator.LessThan, "Joe");
        /// </code>
        /// </example>
        public void AddCondition(string columnName, LearningStoreConditionOperator
            conditionOperator, object conditionValue)
        {
            // Check input parameters
            if (columnName == null)
                throw new ArgumentNullException("columnName");
            if ((conditionOperator != LearningStoreConditionOperator.Equal) &&
                (conditionOperator != LearningStoreConditionOperator.GreaterThan) &&
                (conditionOperator != LearningStoreConditionOperator.GreaterThanEqual) &&
                (conditionOperator != LearningStoreConditionOperator.LessThan) &&
                (conditionOperator != LearningStoreConditionOperator.LessThanEqual) &&
                (conditionOperator != LearningStoreConditionOperator.NotEqual))
                throw new ArgumentOutOfRangeException("conditionOperator");

            // Find the column in the view
            LearningStoreViewColumn column = null;
            if (!m_view.TryGetColumnByName(columnName, out column))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ColumnNotFound, columnName));
            
            // Cast the comparison value to the associated property type
            conditionValue = column.CastValue(conditionValue, m_locale,
                delegate (string reason, Exception innerException)
                {
                    return new InvalidOperationException(
                        String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.InvalidConditionValueWithDescription,
                        reason), innerException);
                }
            );

            // Fail if operator isn't compatible with null
            if ((conditionValue == null) &&
                (conditionOperator != LearningStoreConditionOperator.Equal) &&
                (conditionOperator != LearningStoreConditionOperator.NotEqual))
                throw new InvalidOperationException(
                    LearningStoreStrings.InvalidOperatorForNullConditionValue);

            // Verify special rules for Xml properties
            if ((column.IsXml) &&
                (conditionValue != null))
                throw new InvalidOperationException(
                    LearningStoreStrings.InvalidConditionValueForXmlColumn);

            // Verify special rules for Guid properties
            if ((column.IsGuid) &&
                (conditionOperator != LearningStoreConditionOperator.Equal) &&
                (conditionOperator != LearningStoreConditionOperator.NotEqual))
                throw new InvalidOperationException(
                    LearningStoreStrings.InvalidConditionOperatorForGuidColumn);

            // Verify special rules for LearningStoreItemIdentifier properties
            LearningStoreItemIdentifier idConditionValue = conditionValue as LearningStoreItemIdentifier;
            if ((idConditionValue != null) &&
               (!idConditionValue.HasKey))
                throw new InvalidOperationException(
                     LearningStoreStrings.InvalidIdConditionValue);

            // Add the condition
            LearningStoreCondition condition = new LearningStoreCondition(column, conditionOperator,
                conditionValue);
            m_conditions.Add(condition);
        }

        /// <summary>
        /// Add a sort order for the returned data
        /// </summary>
        /// <param name="columnName">Name of the column to be sorted on.  This must
        ///     refer to a non-XML, non-Guid column.</param>
        /// <param name="direction">Direction for the sort.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">
        ///     The column was not found or is not sortable</exception>
        /// <example>
        /// The following code creates a new query that returns the names of
        /// all the users in the system sorted by name:
        /// <code language="C#">
        /// LearningStoreQuery query = store.CreateQuery("UserItemView");
        /// query.AddColumn("Name");
        /// query.AddSort("Name", LearningStoreSortDirection.Ascending);
        /// </code>
        /// </example>
        public void AddSort(string columnName, LearningStoreSortDirection direction)
        {
            // Check input parameters
            if (columnName == null)
                throw new ArgumentNullException("columnName");
            if ((direction != LearningStoreSortDirection.Ascending) &&
                (direction != LearningStoreSortDirection.Descending))
                throw new ArgumentOutOfRangeException("direction");

            // Find the column in the view
            LearningStoreViewColumn column = null;
            if (!m_view.TryGetColumnByName(columnName, out column))
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ColumnNotFound, columnName));
            
            // Can't sort on some columns
            if (!column.Sortable)
                throw new InvalidOperationException(
                    LearningStoreStrings.ColumnCannotBeSorted);

            // Verify that the same sort hasn't already been added
            if (m_sorts.Exists(delegate(LearningStoreQuerySort otherSort)
            {
                return Object.ReferenceEquals(column, otherSort.Column);
            }))
            {
                throw new InvalidOperationException(
                    LearningStoreStrings.ColumnAlreadySortedOn);
            }

            // Add the sort
            LearningStoreQuerySort sort = new LearningStoreQuerySort(
                column, direction);
            m_sorts.Add(sort);
        }

        /// <summary>
        /// Store on which the query was created
        /// </summary>
        internal LearningStore Store
        {
            get
            {
                return m_store;
            }
        }

        /// <summary>
        /// View for the query
        /// </summary>
        internal LearningStoreView View
        {
            get
            {
                return m_view;
            }
        }

        /// <summary>
        /// Parameter values in the query
        /// </summary>
        internal ReadOnlyCollection<LearningStoreQueryParameterValue> ParameterValues
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreQueryParameterValue>(m_parameterValues);
            }
        }
        
        /// <summary>
        /// Columns in the query
        /// </summary>        
        internal ReadOnlyCollection<LearningStoreViewColumn> Columns
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreViewColumn>(m_columns);
            }
        }

        /// <summary>
        /// Conditions in the query
        /// </summary>        
        internal ReadOnlyCollection<LearningStoreCondition> Conditions
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreCondition>(m_conditions);
            }
        }

        /// <summary>
        /// Sorts in the query
        /// </summary>        
        internal ReadOnlyCollection<LearningStoreQuerySort> Sorts
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreQuerySort>(m_sorts);
            }
        }        
    }
}
