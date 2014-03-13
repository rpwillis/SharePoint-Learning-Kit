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
    /// An "&lt;Column&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
    /// </summary>
    ///
    [DebuggerDisplay("ColumnDefinition {Title}")]
    public class ColumnDefinition
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>Title</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_title;

        /// <summary>
        /// Holds the value of the <c>RenderAs</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ColumnRenderAs m_renderAs;

        /// <summary>
        /// Holds the value of the <c>CellFormat</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_formatString;

        /// <summary>
        /// Holds the value of the <c>NullDisplayString</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_nullDisplayString;

        /// <summary>
        /// Holds the value of the <c>ToolTipFormat</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_toolTipFormat;

        /// <summary>
        /// Holds the value of the <c>Wrap</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_wrap;

        /// <summary>
        /// Holds the value of the <c>LineNumber</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int m_lineNumber;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Fields
        //

        /// <summary>
        /// The maximum number of LearningStore view columns that a <c>ColumnDefinition</c> can map to.
        /// </summary>
        internal const int MaxViewColumns = 3;

        /// <summary>
        /// Holds the value of the <c>ViewColumnName*</c> properties.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string[] m_viewColumnNames = new string[MaxViewColumns];

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// The value of the "Title" attribute.  This is the human-readable label of the column.
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
        /// The value of the "RenderAs" attribute.  Specifies how values in this column are rendered.
        /// </summary>
        public ColumnRenderAs RenderAs
        {
            [DebuggerStepThrough]
            get
            {
                return m_renderAs;
            }
        }

        /// <summary>
        /// The value of the "ViewColumnName" attribute.  This is the name of the first (and possibly
        /// only) LearningStore column (of the view specified by <c>QueryDefinition.ViewName</c>)
        /// that's used to the text values of this column.  (The <c>RenderAs</c> property determines
        /// how rendering occurs, and which columns.)
        /// </summary>
        public string ViewColumnName
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewColumnNames[0];
            }
        }

        /// <summary>
        /// The value of the "ViewColumnName2" attribute.  This is the name of the second LearningStore
        /// column (of the view specified by <c>QueryDefinition.ViewName</c>) that may be used
        /// (depending on <c>RenderAs</c>) to render text values of this column.
        /// </summary>
        public string ViewColumnName2
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewColumnNames[1];
            }
        }

        /// <summary>
        /// The value of the "ViewColumnName3" attribute.  This is the name of the third LearningStore
        /// column (of the view specified by <c>QueryDefinition.ViewName</c>) that may be used
        /// (depending on <c>RenderAs</c>) to render text values of this column.
        /// </summary>
        public string ViewColumnName3
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewColumnNames[2];
            }
        }

        /// <summary>
        /// The value of the "CellFormat" attribute.  This is a .NET format specifier that may be
        /// used (depending on <c>RenderAs</c>) to format the text values in this column.
        /// </summary>
        public string CellFormat
        {
            [DebuggerStepThrough]
            get
            {
                return m_formatString;
            }
        }

        /// <summary>
        /// The value of the "NullDisplayString" attribute.  This string is displayed in a column if
        /// the value in the LearningStore view column specified by <c>ViewColumnName</c> is NULL
        /// (unless overridden by <c>RenderAs</c>).
        /// </summary>
        public string NullDisplayString
        {
            [DebuggerStepThrough]
            get
            {
                return QuerySetLocalization.LocalizedValue(m_nullDisplayString);
            }
        }

        /// <summary>
        /// The value of the "ToolTipFormat" attribute.  This <c>String.Format</c> formatting
        /// specification that may include "{0}" (optionally with additional formatting parameters)
        /// indicating where the column value should included (if at all), and how it should be
        /// formatted.  <c>null</c> if not provided.
        /// </summary>
        public string ToolTipFormat
        {
            [DebuggerStepThrough]
            get
            {
                return QuerySetLocalization.LocalizedValue(m_toolTipFormat);
            }
        }

        /// <summary>
        /// The value of the "Wrap" attribute: <c>true</c> if values in this column can wrap to the
        /// next line, <c>false</c> if not.
        /// </summary>
        public bool Wrap
        {
            [DebuggerStepThrough]
            get
            {
                return m_wrap;
            }
        }

        /// <summary>
        /// The line number of the "&lt;ColumnDefinition&gt;" element with the XML file.
        /// </summary>
        public int LineNumber
        {
            [DebuggerStepThrough]
            get
            {
                return m_lineNumber;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="title">The value to use for the <c>Title</c> property.  If
        ///     <paramref name="title"/> is <c>null</c>, the value of <c>ViewColumnName</c> is used.
        ///     </param>
        ///
        /// <param name="renderAs">The value to use for the <c>RenderAs</c> property.</param>
        ///
        /// <param name="viewColumnName">The value to use for the <c>ViewColumnName</c> property.
        ///     </param>
        ///
        /// <param name="viewColumnName2">The value to use for the <c>ViewColumnName2</c> property.
        ///     </param>
        ///
        /// <param name="viewColumnName3">The value to use for the <c>ViewColumnName3</c> property.
        ///     </param>
        ///
        /// <param name="cellFormat">The value to use for the <c>CellFormat</c> property.</param>
        ///
        /// <param name="toolTipFormat">The value to use for the <c>ToolTipFormat</c> property.
        ///     </param>
        ///
        /// <param name="nullDisplayString">The value to use for the <c>NullDisplayString</c> property.
        ///     </param>
        ///
        /// <param name="wrap">The value to use for the <c>Wrap</c> property.</param>
        ///
        /// <param name="lineNumber">The line number of the "&lt;Column&gt;" element with the
        ///     XML file.</param>
        ///
        internal ColumnDefinition(string title, string renderAs, string viewColumnName,
            string viewColumnName2, string viewColumnName3, string cellFormat,
            string nullDisplayString, string toolTipFormat, bool wrap, int lineNumber)
        {
            m_title = (title ?? viewColumnName);
            m_viewColumnNames[0] = viewColumnName;
            m_viewColumnNames[1] = viewColumnName2;
            m_viewColumnNames[2] = viewColumnName3;
            m_formatString = cellFormat;
            m_nullDisplayString = nullDisplayString;
            m_toolTipFormat = toolTipFormat;
            m_wrap = wrap;
            m_lineNumber = lineNumber;
            if (renderAs == null)
                m_renderAs = ColumnRenderAs.Default;
            else
            switch (renderAs)
            {
            case "Default":
                m_renderAs = ColumnRenderAs.Default;
                break;
            case "UtcAsLocalDateTime":
                m_renderAs = ColumnRenderAs.UtcAsLocalDateTime;
                break;
            case "SPWebName":
                m_renderAs = ColumnRenderAs.SPWebName;
                break;
            case "Link":
                m_renderAs = ColumnRenderAs.Link;
                break;
            case "LearnerAssignmentStatus":
                m_renderAs = ColumnRenderAs.LearnerAssignmentStatus;
                break;
            case "ScoreAndPossible":
                m_renderAs = ColumnRenderAs.ScoreAndPossible;
                break;
            case "IfEmpty":
                m_renderAs = ColumnRenderAs.IfEmpty;
                break;
            case "Submitted":
                m_renderAs = ColumnRenderAs.Submitted;
                break;
            }
        }
    }
}
