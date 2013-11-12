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
    /// A "&lt;Sort&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
    /// </summary>
    ///
    [DebuggerDisplay("SortDefinition {ViewColumnName}")]
    public class SortDefinition
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>ViewColumnName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_viewColumnName;

        /// <summary>
        /// Holds the value of the <c>Ascending</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_ascending;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// The value of the "ViewColumnName" attribute.  This is the name of the LearningStore column
        /// of the view specified by <c>QueryDefinition.ViewName</c>.
        /// </summary>
        public string ViewColumnName
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewColumnName;
            }
        }

        /// <summary>
        /// The value of the "Ascending" attribute: <c>true</c> indicates an ascending-order sort,
        /// <c>false</c> indicates a descending-order sort.
        /// </summary>
        public bool Ascending
        {
            [DebuggerStepThrough]
            get
            {
                return m_ascending;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="viewColumnName">The value to use for the <c>ViewColumnName</c> property.
        ///     </param>
        ///
        /// <param name="ascending">The value to use for the <c>Ascending</c> property.</param>
        ///
        internal SortDefinition(string viewColumnName, bool ascending)
        {
            m_viewColumnName = viewColumnName;
            m_ascending = ascending;
        }
    }
}
