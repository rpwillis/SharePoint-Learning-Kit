using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// A "&lt;Query&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
    /// </summary>
    ///
    [DebuggerDisplay("QueryDefinition {Name}")]
    public class QueryDefinition
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        SlkCulture culture = new SlkCulture();
        /// <summary>
        /// Holds the value of the <c>Name</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_name;

        /// <summary>
        /// Holds the value of the <c>Title</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_title;

        /// <summary>
        /// Holds the value of the <c>ViewName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_viewName;

        /// <summary>
        /// Holds the value of the <c>CountViewColumnName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_countViewColumnName;

        /// <summary>
        /// Holds the value of the <c>Columns</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<ColumnDefinition> m_columnDefinitions =
            new List<ColumnDefinition>();

        /// <summary>
        /// Holds the value of the <c>Conditions</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<ConditionDefinition> m_conditionDefinitions = new List<ConditionDefinition>();

        /// <summary>
        /// Holds the value of the <c>Sorts</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<SortDefinition> m_sortDefinitions = new List<SortDefinition>();

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// The value of the "Name" attribute.  This is the unique internal name of the query; use
        /// <c>Title</c> if you want a human-readable label.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// The value of the "Title" attribute.  This is the human-readable label of the query.
        /// </summary>
        public string Title
        {
            [DebuggerStepThrough]
            get
            {
                return QuerySetLocalization.LocalizedValue(m_title);
            }
        }

        /// <summary>
        /// The value of the "ViewName" attribute.  This is the name of the LearningStore view that
        /// this query uses to retrieve data.
        /// </summary>
        public string ViewName
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewName;
            }
        }

        /// <summary>
        /// The value of the "CountViewColumnName" attribute.  This is the name of a column in the
        /// LearningStore view specified by <c>ViewName</c> that can be used when the application needs
        /// to determine the number of rows returned in a query.  In that situation, the query must
        /// include one column -- any column will do -- and it helps performance to make that column
        /// be one that contains small data (such as a boolean or integer column).  If
        /// <c>CountViewColumnName</c> is <c>null</c>, the application will use the first column
        /// specified in <c>Columns</c> when creating count-only queries.
        /// </summary>
        public string CountViewColumnName
        {
            [DebuggerStepThrough]
            get
            {
                return m_countViewColumnName;
            }
        }

        /// <summary>
        /// Information about each "&lt;Column&gt;" child element.
        /// </summary>
        public ReadOnlyCollection<ColumnDefinition> Columns
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<ColumnDefinition>(m_columnDefinitions);
            }
        }

        /// <summary>
        /// Information about each "&lt;Condition&gt;" child element.
        /// </summary>
        public ReadOnlyCollection<ConditionDefinition> Conditions
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<ConditionDefinition>(m_conditionDefinitions);
            }
        }

        /// <summary>
        /// Information about each "&lt;Sort&gt;" child element.
        /// </summary>
        public ReadOnlyCollection<SortDefinition> Sorts
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<SortDefinition>(m_sortDefinitions);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>
        /// Returns a new <c>LearningStoreQuery</c> constructed from this query definition.
        /// </summary>
        ///
        /// <param name="learningStore">The <c>LearningStore</c> that will be queried.</param>
        ///
        /// <param name="countOnly">If <c>true</c>, the query will include minimal output columns,
        ///     since it will be assumed that the purpose of executing the query is purely to count the
        ///     rows.  If <c>false</c>, all columns specified by the query definition are included
        ///     in the query.</param>
        ///
        /// <param name="macroResolver">A delegate for resolving macros in the query.  May be
        ///     <c>null</c>.</param>
        /// 
        /// <param name="columnMap">Where to store a column map: element <c>i,j</c> is the index of
        ///     the column in the query results that's mapped from <c>Columns[i]</c> and one of
        ///     <c>ColumnDefinition.ViewColumnName</c> (if <c>j</c> is 0),
        ///     <c>ColumnDefinition.ViewColumnName2</c> (if <c>j</c> is 1), or
        ///     <c>ColumnDefinition.ViewColumnName3</c> (if <c>j</c> is 2).  If
        ///     <paramref name="countOnly"/> is true, <c>null</c> is stored in
        ///     <paramref name="columnMap"/></param>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body"), SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public LearningStoreQuery CreateQuery(LearningStore learningStore, bool countOnly, MacroResolver macroResolver, out int[,] columnMap)
        {
            // Check parameters
            if(learningStore == null)
            {
                throw new ArgumentNullException("learningStore");
            }
            
            // create a new LearningStore query
            LearningStoreQuery query = learningStore.CreateQuery(ViewName);

            // add column(s) to the query
            if (countOnly)
            {
                // add a single column -- the caller only wants the count of rows; if the SLK Settings
                // file specified a column to use in this situation, use it, otherwise use the first
                // column in the query
                if (CountViewColumnName != null)
                    query.AddColumn(CountViewColumnName);
                else
                    query.AddColumn(Columns[0].ViewColumnName);
                columnMap = null;
            }
            else
            {
                // add columns to the query as specified in this query definition; create <columnMap>
                // in the process; <viewColumnsAdded> is used to avoid adding duplicate view columns,
                // and to map view column names to their column index in the query results
                columnMap = new int[m_columnDefinitions.Count, ColumnDefinition.MaxViewColumns];
                Dictionary<string, int> viewColumnsAdded = new Dictionary<string,int>(m_columnDefinitions.Count * ColumnDefinition.MaxViewColumns);
                int iColumnDefinition = 0;
                foreach (ColumnDefinition column in Columns)
                {
                    for (int iViewColumn = 0; iViewColumn < ColumnDefinition.MaxViewColumns; iViewColumn++)
                    {
                        string viewColumnName = column.m_viewColumnNames[iViewColumn];
                        if (viewColumnName == null)
                            continue;
                        int iQueryColumn;
                        if (!viewColumnsAdded.TryGetValue(viewColumnName, out iQueryColumn))
                        {
                            try
                            {
                                query.AddColumn(viewColumnName);
                            }
                            catch (InvalidOperationException)
                            {
                                throw new SlkSettingsException(column.LineNumber, culture.Resources.SlkUtilitiesColumnNotDefined, viewColumnName, ViewName);
                            }
                            iQueryColumn = viewColumnsAdded.Count;
                            viewColumnsAdded.Add(viewColumnName, iQueryColumn);
                        }
                        columnMap[iColumnDefinition, iViewColumn] = iQueryColumn;
                    }

                    iColumnDefinition++;
                }
            }

            // add conditions to the query as specified in this query definition
            foreach (ConditionDefinition condition in Conditions)
            {
                object value = null;

                if (condition.MacroName != null)
                {
                    if (macroResolver != null)
                    {
                        value = macroResolver.Resolve(condition.MacroName);
                    }

                    if (value == null) // Either macroResolver is null or macro resolves to null
                    {
                        if (condition.NoConditionOnNull)
                        {
                            // No need to add condition
                            continue;
                        }
                        else
                        {
                            throw new SlkSettingsException(condition.LineNumber, culture.Resources.SlkUtilitiesMacroNotDefined, condition.MacroName);
                        }
                    }
                }
                else if (condition.Value != null)
                {
                    value = new XmlValue(condition.Value);
                }
                else
                {
                    value = null;
                }

                query.AddCondition(condition.ViewColumnName, condition.Operator, value);
            }

            // add sorts to the query as specified in this query definition (unless the caller only
            // wants a count of rows)
            if (!countOnly)
            {
                foreach (SortDefinition sort in Sorts)
                {
                    query.AddSort(sort.ViewColumnName,
                        sort.Ascending ? LearningStoreSortDirection.Ascending
                            : LearningStoreSortDirection.Descending);
                }
            }

            // done
            return query;
        }
        /// <summary>
        /// Given a <c>DataRow</c> from a <c>DataTable</c> returned from a LearningStore query
        /// created by <c>CreateQuery</c>, this method returns one <c>RenderedCell</c> for each column
        /// in the <c>Columns</c> collection.
        /// </summary>
        /// <param name="dataRow">A <c>DataRow</c> from a <c>DataTable</c> returned from a
        ///     LearningStore query created by <c>CreateQuery</c>.</param>
        /// <param name="columnMap">The <c>columnMap</c> returned from
        ///     <c>QueryDefinition.CreateQuery</c>.</param>
        /// <param name="spWebResolver">A delegate for resolving columns with
        ///     <c>ColumnDefinition.RenderAs</c> equal to <c>ColumnRenderAs.SPWebName</c> May be
        ///     <c>null</c>.</param>
        /// <param name="timeZone">The TimeZone to render dates in.</param>
        /// <returns>
        /// An array of <c>Columns.Count</c> <c>RenderedCell</c> values, each containing the rendered
        /// text of the corresponding "&lt;Column&gt;" child element of this "&lt;Query&lt;" element.
        /// For a column using <c>ColumnRenderAs.SPWebName</c>, the corresponding element of the array
        /// will be of type <c>WebNameRenderedCell</c>.</returns>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "1#"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public RenderedCell[] RenderQueryRowCells(DataRow dataRow, int[,] columnMap, SPWebResolver spWebResolver, SPTimeZone timeZone)
        {
            if(dataRow == null)
                throw new ArgumentNullException("dataRow");
            if(columnMap == null)
                throw new ArgumentNullException("columnMap");
                
            RenderedCell[] renderedCells = new RenderedCell[m_columnDefinitions.Count];
            int iColumnDefinition = 0;
            foreach (ColumnDefinition columnDefinition in m_columnDefinitions)
            {
                // set <cellValue> to the rendered value to be displayed in this cell (i.e. this
                // column of this row); set <cellSortKey> to the cell's sort key value (i.e. the value
                // to use for sorting); set <cellId> to the LearningStoreItemIdentifier associated
                // with this cell (null if none); set <cellToolTip> to the tooltip associated with
                // this cell (null if none)
                object cellValue, cellSortKey;
                LearningStoreItemIdentifier cellId;
                string cellToolTip;
                string text, textNotRounded;
                Guid? cellSiteGuid = null, cellWebGuid = null;
                string cellWebUrl = null;
                switch (columnDefinition.RenderAs)
                {

                case ColumnRenderAs.Default:

                    cellValue = dataRow[columnMap[iColumnDefinition, 0]];
                    if (cellValue is DBNull)
                    {
                        cellValue = (columnDefinition.NullDisplayString ?? String.Empty);
                        cellSortKey = String.Empty;
                        cellId = null;
                        cellToolTip = null;
                    }
                    else
                    {
                        if ((cellId = cellValue as LearningStoreItemIdentifier) != null)
                        {
                            cellValue = cellSortKey = cellId.GetKey();
                        }
                        else
                            cellSortKey = cellValue;
                        if (columnDefinition.ToolTipFormat != null)
                            cellToolTip = culture.Format(columnDefinition.ToolTipFormat, cellValue);
                        else
                            cellToolTip = null;
                        if (columnDefinition.CellFormat != null)
                        {
                            text = FormatValue(cellValue, columnDefinition.CellFormat);
                            cellValue = text;
                            cellSortKey = text.ToLower(culture.Culture);
                        }
                    }
                    break;

                case ColumnRenderAs.UtcAsLocalDateTime:

                    cellValue = dataRow[columnMap[iColumnDefinition, 0]];
                    if (cellValue is DBNull)
                    {
                        cellValue = (columnDefinition.NullDisplayString ?? String.Empty);
                        cellSortKey = String.Empty;
                        cellId = null;
                        cellToolTip = null;
                    }
                    else
                    {
                        DateTime dateTime;
                        try
                        {
                            dateTime = timeZone.UTCToLocalTime((DateTime)cellValue);
                        }
                        catch (InvalidCastException)
                        {
                            throw new SlkSettingsException(columnDefinition.LineNumber, culture.Resources.SlkUtilitiesViewColumnNameNonDateTime);
                        }
                        cellValue = cellSortKey = dateTime;
                        if (columnDefinition.CellFormat != null)
                            cellValue = FormatValue(dateTime, columnDefinition.CellFormat);
                        cellId = null;
                        if (columnDefinition.ToolTipFormat != null)
                            cellToolTip = culture.Format(columnDefinition.ToolTipFormat, dateTime);
                        else
                            cellToolTip = null;
                    }
                    break;

                case ColumnRenderAs.SPWebName:

                    cellWebGuid = GetQueryCell<Guid>(dataRow, columnMap, columnDefinition, iColumnDefinition, 0);
                    cellSiteGuid = GetQueryCell<Guid>(dataRow, columnMap, columnDefinition, iColumnDefinition, 1);
                    if (spWebResolver != null)
                    {
                        spWebResolver(cellWebGuid.Value, cellSiteGuid.Value, out text, out cellWebUrl);
                    }
                    else
                    {
                        text = null;
                    }

                    if (text == null)
                    {
                        text = cellWebGuid.Value.ToString();
                    }

                    cellValue = text;
                    cellSortKey = text.ToLower(culture.Culture);
                    cellId = null;
                    if (columnDefinition.ToolTipFormat != null)
                    {
                        cellToolTip = culture.Format(columnDefinition.ToolTipFormat, text);
                    }
                    else
                    {
                        cellToolTip = null;
                    }

                    break;

                case ColumnRenderAs.Link:

                    text = GetQueryCell<string>(dataRow, columnMap, columnDefinition,
                        iColumnDefinition, 0);
                    cellId = GetQueryCell<LearningStoreItemIdentifier>(dataRow, columnMap,
                        columnDefinition, iColumnDefinition, 1);
                    cellValue = text;
                    cellSortKey = text.ToLower(culture.Culture);
                    if (columnDefinition.ToolTipFormat != null)
                        cellToolTip = culture.Format(columnDefinition.ToolTipFormat, text);
                    else
                        cellToolTip = null;
                    break;

                case ColumnRenderAs.LearnerAssignmentStatus:

                    bool unused;
                    LearnerAssignmentState learnerAssignmentState = 
                        (LearnerAssignmentState) GetQueryCell<int>(dataRow, columnMap,
                            columnDefinition, iColumnDefinition, 0, out unused);
                    text = SlkUtilities.GetLearnerAssignmentState(learnerAssignmentState);
                    cellValue = text;
                    cellSortKey = learnerAssignmentState;
                    cellId = null;
                    if (columnDefinition.ToolTipFormat != null)
                        cellToolTip = culture.Format(columnDefinition.ToolTipFormat, text);
                    else
                        cellToolTip = null;
                    break;

                case ColumnRenderAs.ScoreAndPossible:

                    // get <finalPoints> and <pointsPossible> from <dataRow>
                    bool noFinalPoints;
                    float finalPoints = GetQueryCell<float>(dataRow, columnMap, columnDefinition,
                        iColumnDefinition, 0, out noFinalPoints);
                    bool noPointsPossible;
                    float pointsPossible = GetQueryCell<float>(dataRow, columnMap, columnDefinition,
                        iColumnDefinition, 1, out noPointsPossible);

                    // round to two decimal places
                    float finalPointsRounded = (float) Math.Round(finalPoints, 2);
                    float pointsPossibleRounded = (float) Math.Round(pointsPossible, 2);

                    // format the result
                    if (!noFinalPoints)
                    {
                        // FinalPoints is not NULL
                        text = culture.Format(culture.Resources.SlkUtilitiesPointsValue, FormatValue(finalPointsRounded, columnDefinition.CellFormat));
                        textNotRounded = culture.Format(culture.Resources.SlkUtilitiesPointsValue, finalPoints);
                    }
                    else
                    {
                        // FinalPoints is NULL
                        text = culture.Resources.SlkUtilitiesPointsNoValue;
                        textNotRounded = culture.Resources.SlkUtilitiesPointsNoValue;
                    }
                    if (!noPointsPossible)
                    {
                        // PointsPossible is not NULL
                        text = culture.Format(culture.Resources.SlkUtilitiesPointsPossible, text,
                            FormatValue(pointsPossibleRounded, columnDefinition.CellFormat));
                        textNotRounded = culture.Format(culture.Resources.SlkUtilitiesPointsPossible, textNotRounded,
                            pointsPossible);
                    }
                    cellValue = text;
                    cellId = null;
                    if ((columnDefinition.ToolTipFormat != null) &&
                        (!noFinalPoints || !noPointsPossible))
                    {
                        cellToolTip = culture.Format(columnDefinition.ToolTipFormat, textNotRounded);
                    }
                    else
                        cellToolTip = null;

                    // set <cellSortKey>
                    if (!noFinalPoints)
                    {
                        if (!noPointsPossible)
                            cellSortKey = ((double)finalPoints) / pointsPossible;
                        else
                            cellSortKey = (double)finalPoints;
                    }
                    else
                        cellSortKey = (double) 0;
                    break;

                case ColumnRenderAs.Submitted:

                    int countCompletedOrFinal = GetQueryCell<int>(dataRow, columnMap, columnDefinition,
                        iColumnDefinition, 0);
                    int countTotal = GetQueryCell<int>(dataRow, columnMap, columnDefinition,
                        iColumnDefinition, 1);
                    text = culture.Format(culture.Resources.SlkUtilitiesSubmitted, countCompletedOrFinal, countTotal);
                    cellValue = text;
                    cellId = null;
                    cellToolTip = null;
                    cellSortKey = countCompletedOrFinal;
                    break;

                default:

                    throw new SlkSettingsException(columnDefinition.LineNumber, culture.Resources.SlkUtilitiesUnknownRenderAsValue, columnDefinition.RenderAs);
                }

                // add to <renderedCells>
                RenderedCell renderedCell;
                if (cellSiteGuid != null)
                {
                    renderedCell = new WebNameRenderedCell(cellValue, cellSortKey, cellId, cellToolTip,
                        columnDefinition.Wrap, cellSiteGuid.Value, cellWebGuid.Value, cellWebUrl);
                }
                else
                {
                    renderedCell = new RenderedCell(cellValue, cellSortKey, cellId, cellToolTip,
                        columnDefinition.Wrap);
                }
                renderedCells[iColumnDefinition++] = renderedCell;
            }

            return renderedCells;
        }

        /// <summary>
        /// Sorts list of rows, where each row consists of an array of <c>RenderedCell</c> objects. 
        /// </summary>
        ///
        /// <param name="renderedRows">A list of <c>RenderedCell</c> arrays to sort.</param>
        ///
        /// <param name="columnIndex">The zero-based index of the column to sort on.</param>
        ///
        /// <param name="ascending"><c>true</c> for an ascending sort, <c>false</c> for a descending
        ///     sort.</param>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="columnIndex"/> is greater than or equal to the length of a row in
        /// <paramref name="renderedRows"/>.
        /// </exception>
        /// 
        /// <exception cref="InvalidOperationException">
        /// The column being sorted on is of a type that does not implement <c>IComparable</c>.
        /// (Note that columns of type <c>LearningStoreItemIdentifier</c> can be sorted on.)
        /// <paramref name="renderedRows"/>.
        /// </exception>
        ///
        public void SortRenderedRows(IList<RenderedCell[]> renderedRows, int columnIndex,
            bool ascending)
        {
            // Check parameters
            if(renderedRows == null)
                throw new ArgumentNullException("renderedRows");
                
            // copy <renderedRows> into <rowsToSort>, which includes the original row indices (used as
            // a secondary sort field so that rows that compare equally on column <columnIndex>
            // maintain their original order)
            RowToSort[] rowsToSort = new RowToSort[renderedRows.Count];
            for (int i = 0; i < rowsToSort.Length; i++)
            {
                RowToSort rowToSort = new RowToSort();
                rowToSort.OriginalIndex = i;
                rowToSort.Cells = renderedRows[i];
                rowsToSort[i] = rowToSort;
            }

            // sort <rowsToSort>
            Array.Sort(rowsToSort, delegate(RowToSort rowToSortX, RowToSort rowToSortY)
            {
                // get the rows being compared
                RenderedCell[] rowX = rowToSortX.Cells;
                RenderedCell[] rowY = rowToSortY.Cells;

                // check if <columnIndex> is out of range
                if ((columnIndex >= rowX.Length) || (columnIndex >= rowY.Length))
                    throw new ArgumentException(culture.Resources.SlkUtilitiesColumnIndexOutOfRange, "columnIndex");

                // get the sort keys for the rows being compared
                object sortKeyX = rowX[columnIndex].SortKey;
                object sortKeyY = rowY[columnIndex].SortKey;

                // if either sort key is blank (i.e. originally DBNull), it sorts first
                if (IsNullOrEmptyObject(sortKeyX))
                {
                    if (IsNullOrEmptyObject(sortKeyY))
                        return 0;
                    else
                        return ascending ? -1 : 1;
                }
                else
                if (IsNullOrEmptyObject(sortKeyY))
                    return ascending ? 1 : -1;

                // if the cells being compared are of type LearningStoreItemIdentifier (or a subclass),
                // set the sort keys to their keys
                if (sortKeyX is LearningStoreItemIdentifier)
                {
                    sortKeyX = ((LearningStoreItemIdentifier) sortKeyX).GetKey();
                    sortKeyY = ((LearningStoreItemIdentifier) sortKeyY).GetKey();
                }

                // get an IComparable implementation to use for comparing the cells
                IComparable comparableX;
                try
                {
                    comparableX = (IComparable) sortKeyX;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException(culture.Format(culture.Resources.SlkUtilitiesCannotSortColumnType, sortKeyX.GetType().Name));
                }

                // do the comparison; if equal, maintain the original order of the rows
                int result = comparableX.CompareTo(sortKeyY);
                if (result == 0)
                    return rowToSortX.OriginalIndex - rowToSortY.OriginalIndex;
                else
                    return ascending ? result : -result;
            });

            // copy sorted <rowsToSort> back into <renderedRows>
            for (int i = 0; i < rowsToSort.Length; i++)
                renderedRows[i] = rowsToSort[i].Cells;
        }

        /// <summary>
        /// Returns <c>true</c> if a given object is <c>null</c>, <c>DBNull</c>, or a zero-length
        /// string, <c>false</c> otherwise.
        /// </summary>
        static bool IsNullOrEmptyObject(object value)
        {
            // check for null reference
            if (value == null)
                return true;

            // check for DBNull
            if (value is DBNull)
                return true;

            // check for empty string
            string stringValue = value as string;
            if ((stringValue != null) && (stringValue.Length == 0))
                return true;

            return false;
        }

        /// <summary>
        /// Helper class used by <c>SortRenderedRows</c>.
        /// </summary>
        class RowToSort
        {
            /// <summary>
            /// The index of this row in the unsorted array.  This is used as a secondary sort value,
            /// so that two rows that are equivalent when compared using the column being sorted on
            /// will maintain their original order.
            /// </summary>
            public int OriginalIndex;

            /// <summary>
            /// The cells of the row.
            /// </summary>
            public RenderedCell[] Cells;
        }

        /// <summary>
        /// Returns the value of one cell of one <c>DataRow</c> from a <c>DataTable</c> returned from
        /// a LearningStore query created by <c>CreateQuery</c>, cast to a specified type.  An
        /// exception is thrown if the value is <c>null</c>.
        /// </summary>
        ///
        /// <typeparam name="T">The type of the cell.</typeparam>
        ///
        /// <param name="dataRow">The <c>DataRow</c> from the query.</param>
        ///
        /// <param name="columnMap">The <c>columnMap</c> returned from
        ///     <c>QueryDefinition.CreateQuery</c>.</param>
        ///
        /// <param name="columnDefinition">The <c>ColumnDefinition</c> of the cell.</param>
        ///
        /// <param name="iColumnDefinition">The index of <c>ColumnDefinition</c> within
        ///     <c>QueryDefinition.Columns</c>.</param>
        ///
        /// <param name="iViewName">The index of the "ViewColumnName*" attribute: 0 for
        ///     "ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
        ///
        private T GetQueryCell<T>(DataRow dataRow, int[,] columnMap, ColumnDefinition columnDefinition, int iColumnDefinition, int iViewName)
        {
            bool isNull;
            T value = GetQueryCell<T>(dataRow, columnMap, columnDefinition, iColumnDefinition,
                iViewName, out isNull);
            if (isNull)
            {
                throw new SlkSettingsException(columnDefinition.LineNumber, culture.Resources.SlkUtilitiesColumnReturnedNull, GetViewColumnName(iColumnDefinition));
            }
            else
                return value;
        }

        /// <summary>
        /// Returns the value of one cell of one <c>DataRow</c> from a <c>DataTable</c> returned from
        /// a LearningStore query created by <c>CreateQuery</c>, cast to a specified type.  The cell
        /// value may be null.
        /// </summary>
        ///
        /// <typeparam name="T">The type of the cell.</typeparam>
        ///
        /// <param name="dataRow">The <c>DataRow</c> from the query.</param>
        ///
        /// <param name="columnMap">The <c>columnMap</c> returned from
        ///     <c>QueryDefinition.CreateQuery</c>.</param>
        ///
        /// <param name="columnDefinition">The <c>ColumnDefinition</c> of the cell.</param>
        ///
        /// <param name="iColumnDefinition">The index of <c>ColumnDefinition</c> within
        ///     <c>QueryDefinition.Columns</c>.</param>
        ///
        /// <param name="iViewName">The index of the "ViewColumnName*" attribute: 0 for
        ///     "ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
        ///
        /// <param name="isNull">If the cell value is <c>DBNull</c>, <paramref name="isNull"/> is set
        ///     to <c>true</c> and <c>default(T)</c> is returned.  Otherwise,
        ///     <paramref name="isNull"/> is set to <c>false</c>.</param>
        ///
        private T GetQueryCell<T>(DataRow dataRow, int[,] columnMap, ColumnDefinition columnDefinition, int iColumnDefinition, int iViewName, out bool isNull)
        {
            object value = dataRow[columnMap[iColumnDefinition, iViewName]];
            if (value is DBNull)
            {
                isNull = true;
                return default(T);
            }
            if (!(value is T))
            {
                throw new SlkSettingsException(columnDefinition.LineNumber,
                    culture.Resources.SlkUtilitiesColumnReturnedUnexpectedType,
                    GetViewColumnName(iColumnDefinition), value.GetType().Name, typeof(T).Name);
            }
            isNull = false;
            return (T) value;
        }

        /// <summary>
        /// Returns the name of a "ViewColumnName*" attribute.
        /// </summary>
        ///
        /// <param name="iViewName">The index of the "ViewColumnName*" attribute: 0 for
        ///     "ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
        ///
        /// <returns>
        ///     "ViewColumnName" if <paramref name="iViewName"/> is 0, "ViewColumnName2" if
        ///     <paramref name="iViewName"/> is 1, and so on.
        /// </returns>
        ///
        static string GetViewColumnName(int iViewName)
        {
            if (iViewName == 0)
                return "ViewColumnName";
            else
                return String.Format(CultureInfo.InvariantCulture, "ViewColumnName{0}", iViewName + 1);
        }

        /// <summary>
        /// Formats a value using a given format string (e.g. "n2" to format a number to include two
        /// decimal places).
        /// </summary>
        ///
        /// <param name="value">The value to format.</param>
        ///
        /// <param name="cellFormat">The format string, e.g. "n2"; if <c>null</c>, it's ignored and
        ///     the default conversion to <c>System.String</c> is used.</param>
        ///
        static string FormatValue(object value, string cellFormat)
        {
            if (value is DBNull)
                return string.Empty;
            if (cellFormat == null)
                return value.ToString();
            IFormattable formattable = value as IFormattable;
            if (value == null)
                return value.ToString();
            else
                return formattable.ToString(cellFormat, null);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="name">The value to use for the <c>Name</c> property.</param>
        ///
        /// <param name="title">The value to use for the <c>Title</c> property.  If
        ///     <paramref name="title"/> is <c>null</c>, the value of <c>Name</c> is used.</param>
        ///
        /// <param name="countViewColumnName">The value to use for the <c>CountViewColumnName</c>
        /// property.</param>
        ///
        /// <param name="viewName">The value to use for the <c>ViewName</c> property.</param>
        ///
        internal QueryDefinition(string name, string title, string viewName,
            string countViewColumnName)
        {
            m_name = name;
            m_title = (title ?? name);
            m_viewName = viewName;
            m_countViewColumnName = countViewColumnName;
        }

        /// <summary>
        /// Adds information about an "&lt;Column&gt;" child element to the
        /// <c>Columns</c> collection.
        /// </summary>
        ///
        /// <param name="columnDefinition">Information about the "&lt;Column&gt;"
        ///     child element to add.</param>
        ///
        internal void AddIncludeColumn(ColumnDefinition columnDefinition)
        {
            m_columnDefinitions.Add(columnDefinition);
        }

        /// <summary>
        /// Adds information about a "&lt;Condition&gt;" child element to the
        /// <c>Conditions</c> collection.
        /// </summary>
        ///
        /// <param name="conditionDefinition">Information about the "&lt;Condition&gt;" child
        ///     element to add.</param>
        ///
        internal void AddCondition(ConditionDefinition conditionDefinition)
        {
            m_conditionDefinitions.Add(conditionDefinition);
        }

        /// <summary>
        /// Adds information about a "&lt;Sort&gt;" child element to the <c>Sorts</c> collection.
        /// </summary>
        ///
        /// <param name="sortDefinition">Information about the "&lt;Sort&gt;" child
        ///     element to add.</param>
        ///
        internal void AddSort(SortDefinition sortDefinition)
        {
            m_sortDefinitions.Add(sortDefinition);
        }
    }
}

