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
    /// A "&lt;QuerySet&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".  Identifies a collection of
    /// queries.
    /// </summary>
    ///
    [DebuggerDisplay("QuerySetDefinition {Name}")]
    public class QuerySetDefinition
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

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
        /// Holds the value of the <c>DefaultQueryName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_defaultQueryName;

        /// <summary>
        /// Holds the value of the <c>Queries</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        List<QueryDefinition> m_queryDefinitions = new List<QueryDefinition>();

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
        /// The value of the "Title" attribute.  This is the human-readable label of the query set.
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
        /// The value of the "DefaultQueryName" attribute.  This is the "QueryName" attribute of
        /// the "&lt;IncludeQuery&gt;" element that is the default query to display; <c>null</c>
        /// if none.
        /// </summary>
        public string DefaultQueryName
        {
            [DebuggerStepThrough]
            get
            {
                return m_defaultQueryName;
            }
        }

        /// <summary>
        /// Information about each query included in the query set by an "&lt;IncludeQuery&gt;" child
        /// element.
        /// </summary>
        public ReadOnlyCollection<QueryDefinition> Queries
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<QueryDefinition>(m_queryDefinitions);
            }
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
        /// <param name="defaultQueryName">The value to use for the <c>DefaultQueryName</c>
        ///     property.</param>
        ///
        internal QuerySetDefinition(string name, string title, string defaultQueryName)
        {
            m_name = name;
            m_title = (title ?? name);
            m_defaultQueryName = defaultQueryName;
        }

        /// <summary>
        /// Adds information about a query included in the query set using an "&lt;IncludeQuery&gt;"
        /// child element.
        /// </summary>
        ///
        /// <param name="queryDefinition">The query to add.</param>
        ///
        internal void AddIncludeQuery(QueryDefinition queryDefinition)
        {
            m_queryDefinitions.Add(queryDefinition);
        }
    }
}
