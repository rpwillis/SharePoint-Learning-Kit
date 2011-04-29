/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Data.SqlClient;
using System.Data.SqlTypes;

#endregion

/*
 * This file contains classes that help read results from the database
 * into LearningStore-specific structures
 * 
 * Internal error numbers: 1000-1199
 */
namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// A set of helpers that can read LearningStore-specific information
    /// from a LogableSqlCommand
    /// </summary>
    internal class SqlDataReaderHelpers
    {
        /// <summary>
        /// Create a LearningStoreItemIdentifier from some data within a row of a
        /// LogableSqlCommand
        /// </summary>
        /// <param name="command">The LogableSqlCommand.  The current position is
        ///     not modified.</param>
        /// <param name="startingColumn">The index of the first column to be
        ///     examined.  On exit, this index contains the last column examined
        ///     plus one.</param>
        /// <param name="itemType">Information about the identifier type to be
        ///     created.</param>
        /// <returns>The<Typ>LearningStoreItemIdentifier</Typ>, or null if
        ///     a value is not found.</returns>
        /// <remarks>Only examines one column.  Assumes that the column contains
        ///     an int64 value, containing the key of the Id.  If the column is
        ///     non-null, a LearningStoreItemIdentifier is returned.  If the
        ///     column is null, null is returned.
        /// </remarks>
        private static LearningStoreItemIdentifier ReadItemIdentifierColumns(LogableSqlCommand command,
            ref int startingColumn, LearningStoreItemType itemType)
        {
            if (command.IsDBNull(startingColumn))
            {
                startingColumn++;
                return null;
            }
            else
            {
                long idkey = command.GetInt64(startingColumn);
                startingColumn++;
                return new LearningStoreItemIdentifier(itemType.Name, idkey);
            }
        }

        /// <summary>
        /// Create a LearningStoreXml from some data within a row of a
        /// LogableSqlCommand
        /// </summary>
        /// <param name="command">The LogableSqlCommand.  The current position is
        ///     not modified.</param>
        /// <param name="startingColumn">The index of the first column to be
        ///     examined.  On exit, this index contains the last column examined
        ///     plus one.</param>
        /// <returns>The <Typ>LearningStoreXml</Typ>, or null if a 
        ///     value is not found.</returns>
        /// <remarks>Only examines one column.  Assumes that the column contains
        ///     an XML value.  If the column is non-null, a LearningStoreXml
        ///     is returned.  If the column is null, null is returned.
        /// </remarks>
        private static LearningStoreXml ReadXmlColumns(LogableSqlCommand command,
            ref int startingColumn)
        {
            if (command.IsDBNull(startingColumn))
            {
                startingColumn++;
                return null;
            }
            else
            {
                SqlXml xml = command.GetSqlXml(startingColumn);
                startingColumn++;
                return new LearningStoreXml(xml);
            }
        }

        /// <summary>
        /// Create a LearningStoreItemIdentifier from a result within a LogableSqlCommand
        /// </summary>
        /// <param name="command">The LogableSqlCommand.  On exit, the entire
        ///     current result has been read.</param>
        /// <param name="itemType">Information about the item type that should be read.</param>
        /// <returns>The<Typ>LearningStoreItemIdentifier</Typ></returns>
        /// <remarks>Assumes that the result has one rows.  Assumes that the
        ///     columns within the row contain exactly the correct data
        ///     for the <Mth>ReadItemIdentifierColumns</Mth> method.</remarks>
        public static LearningStoreItemIdentifier ReadItemIdentifierResult(LogableSqlCommand command, LearningStoreItemType itemType)
        {
            // Check input parameters
            if (command == null)
                throw new LearningComponentsInternalException("LSTR1000");
            if (itemType == null)
                throw new LearningComponentsInternalException("LSTR1010");

            if (!command.Read())
                throw new LearningComponentsInternalException("LSTR1020");

            // Start at the first column
            int startingColumn = 0;

            // Read the item
            LearningStoreItemIdentifier id = ReadItemIdentifierColumns(command,
                ref startingColumn, itemType);

            // Verify that the correct number of values are in the row
            if (command.GetFieldCount() != startingColumn)
                throw new LearningComponentsInternalException("LSTR1030");

            if (command.Read())
                throw new LearningComponentsInternalException("LSTR1040");

            return id;
        }

        /// <summary>
        /// Create a DataTable from a result within a LogableSqlCommand
        /// </summary>
        /// <param name="command">The LogableSqlCommand.  On exit, the entire
        ///     result has been read.</param>
        /// <param name="columns">List of columns that should be read into
        ///     the table.</param>
        /// <param name="locale">Locale for the DataTable</param>
        /// <returns>The DataTable</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]  // Much of the complexity is due to simple switch statements
        public static DataTable ReadDataTableResult(LogableSqlCommand command, IList<LearningStoreViewColumn> columns, CultureInfo locale)
        {
            // Check input parameters
            if (command == null)
                throw new LearningComponentsInternalException("LSTR1050");
            if (columns == null)
                throw new LearningComponentsInternalException("LSTR1060");

            // Create the DataTable
            DataTable table = new DataTable();
            table.Locale = locale;

            // Add the columns
            foreach (LearningStoreViewColumn column in columns)
            {
                Type t = null;
                switch(column.ValueType.TypeCode)
                {
                    case LearningStoreValueTypeCode.Boolean:
                        t = typeof(Boolean);
                        break;
                    case LearningStoreValueTypeCode.DateTime:
                        t = typeof(DateTime);
                        break;
                    case LearningStoreValueTypeCode.Double:
                        t = typeof(Double);
                        break;
                    case LearningStoreValueTypeCode.Enumeration:
                        t = typeof(Int32);
                        break;
                    case LearningStoreValueTypeCode.Int32:
                        t = typeof(Int32);
                        break;
                    case LearningStoreValueTypeCode.ItemIdentifier:
                        t = typeof(LearningStoreItemIdentifier);
                        break;
                    case LearningStoreValueTypeCode.Single:
                        t = typeof(Single);
                        break;
                    case LearningStoreValueTypeCode.String:
                        t = typeof(String);
                        break;
                    case LearningStoreValueTypeCode.Xml:
                        t = typeof(LearningStoreXml);
                        break;
                    case LearningStoreValueTypeCode.ByteArray:
                        t = typeof(System.Byte[]);
                        break;
                    case LearningStoreValueTypeCode.Guid:
                        t = typeof(Guid);
                        break;
                    default:
                        throw new LearningComponentsInternalException("LSTR1070");
                }
                    
                table.Columns.Add(column.Name, t);
            }

            // Begin loading the data
            table.BeginLoadData();

            // Enumerate through each row
            while (command.Read())
            {
                // Remember the current input column index
                int inputColumnIndex = 0;

                // Create a new array that will hold the items
                object[] data = new object[columns.Count];

                // Enumerate through each column
                for (int outputColumnIndex = 0; outputColumnIndex < columns.Count; outputColumnIndex++)
                {
                    // Get the column
                    LearningStoreViewColumn column = columns[outputColumnIndex];

                    // Read the value
                    switch (column.ValueType.TypeCode)
                    {
                        case LearningStoreValueTypeCode.Boolean:
                        case LearningStoreValueTypeCode.DateTime:
                        case LearningStoreValueTypeCode.Double:
                        case LearningStoreValueTypeCode.Int32:
                        case LearningStoreValueTypeCode.Single:
                        case LearningStoreValueTypeCode.String:
                        case LearningStoreValueTypeCode.Guid:
                        case LearningStoreValueTypeCode.Enumeration:
                        case LearningStoreValueTypeCode.ByteArray:
                            if (command.IsDBNull(inputColumnIndex))
                                data[outputColumnIndex] = null;
                            else
                                data[outputColumnIndex] = command.GetValue(inputColumnIndex);
                            inputColumnIndex++;
                            break;
                        case LearningStoreValueTypeCode.ItemIdentifier:
                            data[outputColumnIndex] = ReadItemIdentifierColumns(command, ref inputColumnIndex, column.ValueType.ReferencedItemType);
                            break;
                        case LearningStoreValueTypeCode.Xml:
                            data[outputColumnIndex] = ReadXmlColumns(command, ref inputColumnIndex);
                            break;
                        default:
                            throw new LearningComponentsInternalException("LSTR1080");
                    }
                }

                table.Rows.Add(data);
            }

            // Finish loading the data
            table.EndLoadData();
            table.AcceptChanges();

            return table;
        }
    }
}
