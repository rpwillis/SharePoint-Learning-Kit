/* Copyright (c) Microsoft Corporation. All rights reserved. */

// SlkSettings.cs
//
// Defines classes <c>SlkSettings</c>, <c>QueryDefinition</c>, <c>QuerySetDefinition</c>, and
// related types.
//
// See "SLK Settings Schema.doc" for more information.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{

/// <summary>
/// Contains methods for parsing a file with schema
/// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings" and managing the results.
/// </summary>
///
public class SlkSettings
{
	//////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>ApprovedAttachmentTypes</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Collection<string> m_approvedAttachmentTypes;

    /// <summary>
    /// Holds the value of the <c>ELearningIisCompatibilityModeExtensions</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Collection<string> m_eLearningIisCompatibilityModeExtensions;

    /// <summary>
    /// Holds the value of the <c>NonELearningIisCompatibilityModeExtensions</c> property.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Collection<string> m_nonELearningIisCompatibilityModeExtensions;

	/// <summary>
	/// Holds the value of the <c>LoggingOptions</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	LoggingOptions m_loggingOptions;

	/// <summary>
	/// Holds the value of the <c>MaxAttachmentKilobytes</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int m_maxAttachmentKilobytes;

	/// <summary>
	/// Holds the value of the <c>PackageCacheExpirationMinutes</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int? m_packageCacheExpirationMinutes;

        ///<summary>Indicates whether to hide disabled users on the assignment page.</summary>
        bool hideDisabledUsers;

	/// <summary>
	/// Holds the value of the <c>PackageCacheLocation</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_packageCacheLocation;

	/// <summary>
	/// Holds the value of the <c>UserWebListMruSize</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int m_userWebListMruSize;

	/// <summary>
	/// Holds the value of the <c>MimeTypeMappings</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Dictionary<string, string> m_mimeTypeMappings;

	/// <summary>
	/// Holds the value of the <c>QueryDefinitions</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	List<QueryDefinition> m_queryDefinitions;

	/// <summary>
	/// Holds the value of the <c>QuerySetDefinitions</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	List<QuerySetDefinition> m_querySetDefinitions;

	/// <summary>
	/// Holds the value of the <c>WhenUploaded</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	DateTime m_whenUploaded;

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
    /// Gets the collection of approved attachment file name extensions from the
	/// "ApprovedAttachmentTypes" attribute of the "&lt;Settings&gt;" element within the SLK
	/// Settings XML file.  Each key in this dictionary is a file name extension, including a
	/// leading period; for example, ".jpg".
	/// </summary>
	public ReadOnlyCollection<string> ApprovedAttachmentTypes
	{
		get
		{
			return new ReadOnlyCollection<string>(m_approvedAttachmentTypes);
		}
	}

    /// <summary>
    /// Gets the collection of file extensions (including the preceding period) for files 
    /// that should be downloaded to a client in IIS compability mode. That is, these files 
    /// will be sent in a way similar to how IIS sends the files, but this may cause a decrease in 
    /// performance.  Applies to files within e-learning packages (SCORM and Class Server).
    /// </summary>
    public ICollection<string> ELearningIisCompatibilityModeExtensions
    {
        get
        {
            return new ReadOnlyCollection<string>(m_eLearningIisCompatibilityModeExtensions);
        }
    }

    /// <summary>
    /// Gets the collection of file extensions (including the preceding period) for files 
    /// that should be downloaded to a client in IIS compability mode. That is, these files 
    /// will be sent in a way similar to how IIS sends the files, but this may cause a decrease in 
    /// performance.  Applies to non-e-learning files.
    /// </summary>
    public ICollection<string> NonELearningIisCompatibilityModeExtensions
    {
        get
        {
            return new ReadOnlyCollection<string>(m_nonELearningIisCompatibilityModeExtensions);
        }
    }

	/// <summary>
    /// Gets the value of the "LoggingOptions" attribute of the "&lt;Settings&gt;" element
	/// within the SLK Settings XML file.  This specifies what type of logging (if any) of SCORM
	/// sequencing actions is enabled for content that supports sequencing.
	/// </summary>
	public LoggingOptions LoggingOptions
	{
		[DebuggerStepThrough]
		get
		{
			return m_loggingOptions;
		}
	}

	/// <summary>
    /// Gets the value of the "MaxAttachmentKilobytes" attribute of the "&lt;Settings&gt;" element
	/// within the SLK Settings XML file.  This is the maximum size of a file that a learner may
	/// attach to an attachment-type question in LRM content, measured in kilobytes.
	/// </summary>
	public int MaxAttachmentKilobytes
	{
		[DebuggerStepThrough]
		get
		{
			return m_maxAttachmentKilobytes;
		}
	}

	/// <summary>
    /// Gets the value of the "PackageCacheExpirationMinutes" attribute of the "&lt;Settings&gt;"
	/// element within the SLK Settings XML file.  This is the minimum amount of time that a
	/// package is retained in the SharePoint Learning Kit file system cache, measured in minutes.
    /// If "PackageCacheExpirationMinutes" is absent, <c>null</c> is returned, which should be
	/// interpreted as no expiration (i.e. packages are never removed from the cache).
	/// </summary>
	public Nullable<int> PackageCacheExpirationMinutes
	{
		[DebuggerStepThrough]
		get
		{
			return m_packageCacheExpirationMinutes;
		}
	}

        ///<summary>Indicates whether to hide disabled users on the assignment page.</summary>
        public bool HideDisabledUsers
        {
            get { return hideDisabledUsers ;}
        }

	/// <summary>
    /// Gets the value of the "PackageCacheLocation" attribute of the "&lt;Settings&gt;" element
	/// within the SLK Settings XML file.  This is the full path to the SharePoint Learning Kit
	/// package cache location used for each front-end server.  This path can be a network file
	/// share so multiple front-end servers can use the same package cache.  Note that this path
	/// may contain environment variable references such as "%TEMP%".
	/// </summary>
	public string PackageCacheLocation
	{
		[DebuggerStepThrough]
		get
		{
			return m_packageCacheLocation;
		}
	}

	/// <summary>
    /// Gets the value of the "UserWebListMruSize" attribute of the "&lt;Settings&gt;" element
	/// within the SLK Settings XML file.  This is the number of Web sites displayed in the
	/// most-recently-used list of Web sites in the SharePoint Learning Kit E-Learning Actions
	/// page.  All Web site references are retained, and the user can display them by clicking a
	/// link to see the complete list.  The MRU is just the most-recently-used subset of Web sites
	/// that's displayed by default.
	/// </summary>
	public int UserWebListMruSize
	{
		[DebuggerStepThrough]
		get
		{
			return m_userWebListMruSize;
		}
	}

	/// <summary>
    /// Gets the collection of MIME type mappings parsed from "&lt;MimeTypeMapping&gt;" elements
	/// within the SLK Settings XML file.  Each mapping has a key which is a file name extension
	/// including the leading period (for example, ".jpg" or ".jpeg") and an associated MIME type
	/// (for example, "image/jpeg").  This MIME type mapping table determines the MIME type
	/// returned to the browser when content (for example, images) within e-learning packages are
	/// displayed.
	/// </summary>
	public IDictionary<string, string> MimeTypeMappings
	{
		[DebuggerStepThrough]
		get
		{
			return m_mimeTypeMappings;
		}
	}

	/// <summary>
    /// Gets the collection of query definitions parsed from "&lt;Query&gt;" elements within the
	/// SLK Settings XML file.
	/// </summary>
	public ReadOnlyCollection<QueryDefinition> QueryDefinitions
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyCollection<QueryDefinition>(m_queryDefinitions);
		}
	}

	/// <summary>
    /// Gets the collection of query set definitions parsed from "&lt;QuerySet&gt;" elements within
	/// the SLK Settings XML file.
	/// </summary>
	public ReadOnlyCollection<QuerySetDefinition> QuerySetDefinitions
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyCollection<QuerySetDefinition>(m_querySetDefinitions);
		}
	}

	/// <summary>
    /// Gets the date/time that the SLK Settings file was uploaded to the database.
	/// </summary>
	public DateTime WhenUploaded
	{
		[DebuggerStepThrough]
		get
		{
			return m_whenUploaded;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	//

    /// <summary>
    /// Parses an SLK Settings XML file, i.e. an XML file with schema
	/// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".  Returns a new
	/// <c>SlkSettings</c> object containing information from the parsed file.
    /// </summary>
	///
    /// <param name="xmlReader">An <c>XmlReader</c> positioned at the beginning of the SLK Settings
	/// 	XML file.  This <c>XmlReader</c> must have "SlkSettings.xsd" (the SLK Setttings XML
	/// 	schema) attached using the <c>XmlReaderSettings</c> parameter of the <c>XmlReader</c>
	/// 	constructor, with <c>XmlReaderSettings.ValidationType</c> equal to
	/// 	<c>ValidationType.Schema</c>.</param>
	///
    /// <param name="whenUploaded">The date/time that the SLK Settings XML file was uploaded
	/// 	to the database.</param>
	///
	/// <exception cref="SlkSettingsException">
	/// The SLK Settings XML file is invalid.  The exception message includes the line number and
	/// other information about the error.
	/// </exception>
	///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public static SlkSettings ParseSettingsFile(XmlReader xmlReader, DateTime whenUploaded)
	{
        // Check parameters
        if (xmlReader == null)
            throw new ArgumentNullException("xmlReader");

        // parse the SLK Settings XML file, and fill in <slkSettings>; use <queryDictionary> (which
		// maps a query name to a QueryDefinition) and <querySetDictionary> (which maps a query set
		// name to a QuerySetDefinition) to check for duplicate and invalid query and query set
		// names (including a query name the same as query set name, which isn't allowed) -- note
		// that the .xsd file requires that all "<Query>" elements appear before any "<QuerySet>"
		// elements
		SlkSettings slkSettings = new SlkSettings();
		slkSettings.m_whenUploaded = whenUploaded;
		Dictionary<string,QueryDefinition> queryDictionary =
			new Dictionary<string,QueryDefinition>();
		Dictionary<string,QuerySetDefinition> querySetDictionary =
			new Dictionary<string,QuerySetDefinition>();
		try
		{
			QueryDefinition unused;
			QuerySetDefinition unused2;
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.EndElement)
					break;
				else
				if (xmlReader.NodeType != XmlNodeType.Element)
					continue;
				else
                if (xmlReader.Name == "Settings")
                {
					// parse "ApprovedAttachmentTypes" attribute
                    ParseExtensionList(
                        xmlReader.GetAttribute("ApprovedAttachmentTypes"),
                        slkSettings.m_approvedAttachmentTypes);

                    // parse "ELearningIisCompatibilityModeExtensions" attribute
                    ParseExtensionList(
                        xmlReader.GetAttribute("ELearningIisCompatibilityModeExtensions"),
                        slkSettings.m_eLearningIisCompatibilityModeExtensions);

                    // parse "NonELearningIisCompatibilityModeExtensions" attribute
                    ParseExtensionList(xmlReader.GetAttribute(
                        "NonELearningIisCompatibilityModeExtensions"),
                        slkSettings.m_nonELearningIisCompatibilityModeExtensions);

                    // parse "PackageCacheLocation" attribute
					slkSettings.m_packageCacheLocation =
						xmlReader.GetAttribute("PackageCacheLocation");

					// parse "LoggingOptions" attribute
					string loggingOptions = xmlReader.GetAttribute("LoggingOptions");
					switch (loggingOptions)
					{
					case "None":
						slkSettings.m_loggingOptions = LoggingOptions.None;
						break;
					case "LogDetailSequencing":
						slkSettings.m_loggingOptions = LoggingOptions.LogDetailSequencing;
						break;
					case "LogFinalSequencing":
						slkSettings.m_loggingOptions = LoggingOptions.LogFinalSequencing;
						break;
					case "LogRollup":
						slkSettings.m_loggingOptions = LoggingOptions.LogRollup;
						break;
					case "LogAllSequencing":
						slkSettings.m_loggingOptions = LoggingOptions.LogAllSequencing;
						break;
					case "LogAll":
						slkSettings.m_loggingOptions = LoggingOptions.LogAll;
						break;
					default:
						// XSD validation should have caught the error
                        throw new InternalErrorException("SLKSET1001");
					}

					// parse "MaxAttachmentKilobytes" attribute
					if (!xmlReader.MoveToAttribute("MaxAttachmentKilobytes"))
                                        {
                                            throw new InternalErrorException("SLKSET1002");
                                        }
					slkSettings.m_maxAttachmentKilobytes = xmlReader.ReadContentAsInt();

					// parse "HideDisabledUsers" attribute
					if (!xmlReader.MoveToAttribute("HideDisabledUsers"))
                                        {
                                            slkSettings.hideDisabledUsers = false;
                                        }
					else
                                        {
                                            slkSettings.hideDisabledUsers = xmlReader.ReadContentAsBoolean();
                                        }

					// parse "PackageCacheExpirationMinutes" attribute
					if (!xmlReader.MoveToAttribute("PackageCacheExpirationMinutes"))
						slkSettings.m_packageCacheExpirationMinutes = null;
					else
						slkSettings.m_packageCacheExpirationMinutes = xmlReader.ReadContentAsInt();

					// parse "UserWebListMruSize" attribute
					if (!xmlReader.MoveToAttribute("UserWebListMruSize"))
                        throw new InternalErrorException("SLKSET1003");
					slkSettings.m_userWebListMruSize = xmlReader.ReadContentAsInt();

					// done parsing attributes
					xmlReader.MoveToElement();
                }
				else
                if (xmlReader.Name == "MimeTypeMapping")
                {
					slkSettings.m_mimeTypeMappings[xmlReader.GetAttribute("Extension")] =
						xmlReader.GetAttribute("MimeType");
                }
				else
				if (xmlReader.Name == "Query")
				{
					string queryName = xmlReader.GetAttribute("Name");
					if (queryDictionary.TryGetValue(queryName, out unused))
					{
						throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesDuplicateQueryName,
							queryName);
					}
					QueryDefinition queryDefinition = new QueryDefinition(queryName,
                        xmlReader.GetAttribute("Title"), xmlReader.GetAttribute("ViewName"),
                        xmlReader.GetAttribute("CountViewColumnName"));
					while (xmlReader.Read())
					{
                        if (xmlReader.NodeType == XmlNodeType.EndElement)
                            break;
                        else
						if (xmlReader.NodeType != XmlNodeType.Element)
							continue;
						else
						if (xmlReader.Name == "Column")
						{
							xmlReader.MoveToAttribute("Wrap");
							bool wrap = xmlReader.ReadContentAsBoolean();
							xmlReader.MoveToElement();
							queryDefinition.AddIncludeColumn(
								new ColumnDefinition(
									xmlReader.GetAttribute("Title"),
									xmlReader.GetAttribute("RenderAs"),
									xmlReader.GetAttribute("ViewColumnName"),
									xmlReader.GetAttribute("ViewColumnName2"),
									xmlReader.GetAttribute("ViewColumnName3"),
									xmlReader.GetAttribute("CellFormat"),
									xmlReader.GetAttribute("NullDisplayString"),
									xmlReader.GetAttribute("ToolTipFormat"),
									wrap,
									((IXmlLineInfo) xmlReader).LineNumber));
						}
						else
						if (xmlReader.Name == "Condition")
						{
							xmlReader.MoveToAttribute("NoConditionOnNull");
							bool noConditionOnNull = xmlReader.ReadContentAsBoolean();
							xmlReader.MoveToElement();
							try
							{
								queryDefinition.AddCondition(
									new ConditionDefinition(
										xmlReader.GetAttribute("ViewColumnName"),
										xmlReader.GetAttribute("Operator"),
										xmlReader.GetAttribute("Value"),
										xmlReader.GetAttribute("MacroName"),
										noConditionOnNull,
                                        ((IXmlLineInfo) xmlReader).LineNumber));
							}
							catch (ArgumentException)
							{
								throw new SlkSettingsException(xmlReader,
									AppResources.SlkUtilitiesConditionException);
							}
						}
						else
						if (xmlReader.Name == "Sort")
						{
							string viewColumnName = xmlReader.GetAttribute("ViewColumnName");
							xmlReader.MoveToAttribute("Ascending");
							bool ascending = xmlReader.ReadContentAsBoolean();
							xmlReader.MoveToElement();
							queryDefinition.AddSort(new SortDefinition(viewColumnName, ascending));
						}
					}
					slkSettings.m_queryDefinitions.Add(queryDefinition);
                    queryDictionary.Add(queryName, queryDefinition);
				}
				else
				if (xmlReader.Name == "QuerySet")
				{
					string querySetName = xmlReader.GetAttribute("Name");
					if (querySetDictionary.TryGetValue(querySetName, out unused2))
					{
						throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesDuplicateQuerySetName,
							querySetName);
					}
					if (queryDictionary.TryGetValue(querySetName, out unused))
					{
						throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesQuerySetSameAsQuery,
							querySetName);
					}
					string defaultQueryName = xmlReader.GetAttribute("DefaultQueryName");
					if (!queryDictionary.TryGetValue(defaultQueryName, out unused))
					{
						throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesUndefinedQuery,
							defaultQueryName);
					}
					QuerySetDefinition querySetDefinition = new QuerySetDefinition(querySetName,
						xmlReader.GetAttribute("Title"), defaultQueryName);
					while (xmlReader.Read())
					{
						if (xmlReader.NodeType == XmlNodeType.EndElement)
							break;
						else
						if (xmlReader.NodeType != XmlNodeType.Element)
							continue;
						else
						if (xmlReader.Name == "IncludeQuery")
						{
							string queryName = xmlReader.GetAttribute("QueryName");
							QueryDefinition queryDef;
							try
							{
								queryDef = queryDictionary[queryName];
							}
							catch (KeyNotFoundException)
							{
								throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesUndefinedQuery,
									queryName);
							}
							querySetDefinition.AddIncludeQuery(queryDef);
						}
					}
					slkSettings.m_querySetDefinitions.Add(querySetDefinition);
					querySetDictionary.Add(querySetName, querySetDefinition);
				}
			}
		}
		catch (XmlException ex)
		{
			throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesSettingsExceptionDefaultFormat, ex.Message);
		}
		catch (XmlSchemaValidationException ex)
		{
			throw new SlkSettingsException(xmlReader, AppResources.SlkUtilitiesSettingsExceptionDefaultFormat, ex.Message);
		}

		return slkSettings;
	}

	/// <summary>
	/// Locates a <c>QueryDefinition</c> in a list of queries given its name.
	/// </summary>
    ///
    /// <param name="queryName">The name of the query to look for.</param>
	///
	/// <returns>
	/// The requested <c>QueryDefinition</c>, or <c>null</c> if none was found.
	/// </returns>
    ///
	public QueryDefinition FindQueryDefinition(string queryName)
	{
	    // Check parameters
	    if(queryName == null)
	        throw new ArgumentNullException("queryName");
	        
		return m_queryDefinitions.Find(delegate(QueryDefinition queryDef)
			{
				return queryDef.Name == queryName;
			});
	}

	/// <summary>
	/// Locates a <c>QuerySetDefinition</c> in a list of query sets given its name.  If requested,
	/// the list of queries is searched in addition to the list of query sets; in that case, if a
	/// query name matching the query set name is found, a <c>QuerySetDefinition</c> containing
	/// only that query is returned.
	/// </summary>
    ///
    /// <param name="querySetName">The name of the query set (or query) or query to look for.
	/// 	</param>
	///
	/// <param name="searchQueries">If <c>true</c>, the list of queries is searched (for a query
	/// 	named <paramref name="querySetName"/>) in addition to the list of query sets.  If
	/// 	<c>false</c>, only the list of query sets is searched.</param>
	///
	/// <returns>
	/// The requested <c>QuerySetDefinition</c>, or <c>null</c> if none was found.
	/// </returns>
    ///
	public QuerySetDefinition FindQuerySetDefinition(string querySetName, bool searchQueries)
	{
        // Check parameters
        if (querySetName == null)
            throw new ArgumentNullException("querySetName");
	        	
		// search query sets
		QuerySetDefinition foundQuerySetDef = m_querySetDefinitions.Find(
			delegate(QuerySetDefinition querySetDef)
			{
				return querySetDef.Name == querySetName;
			});
		if (foundQuerySetDef != null)
            return foundQuerySetDef;

		// search queries, if specified
		if (searchQueries)
		{
			QueryDefinition foundQueryDef = m_queryDefinitions.Find(
				delegate(QueryDefinition queryDef)
				{
					return queryDef.Name == querySetName;
				});
			if (foundQueryDef != null)
			{
				foundQuerySetDef = new QuerySetDefinition(foundQueryDef.Name,
					foundQueryDef.Title, foundQueryDef.Name);
				foundQuerySetDef.AddIncludeQuery(foundQueryDef);
				return foundQuerySetDef;
			}
		}

		// not found
		return null;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Internal Methods
	//

    /// <summary>
	/// Initializes an instance of this class.
    /// </summary>
	///
	internal SlkSettings()
	{
		m_approvedAttachmentTypes = new Collection<string>();
		m_eLearningIisCompatibilityModeExtensions = new Collection<string>();
		m_nonELearningIisCompatibilityModeExtensions = new Collection<string>();
		m_mimeTypeMappings = new Dictionary<string, string>(50);
		m_queryDefinitions = new List<QueryDefinition>(20);
		m_querySetDefinitions = new List<QuerySetDefinition>(10);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	//

    /// <summary>
    /// Parses a comma-separated list of file name extensions, such as "foo, bar", and adds them
    /// to a given collection of strings.  Prepends a period to any extension that doesn't start
    /// with a period.  Ignores leading and trailing white space around any extension in the list.
    /// </summary>
    ///
    /// <param name="extensions">The comma-separated list of extensions.  If <pr>extensions</pr>
    ///     is null, no strings are added to <pr>collections</pr>.</param>
    ///
    /// <param name="collection">The string collection to add to.</param>
    ///
    static void ParseExtensionList(string extensions, Collection<string> collection)
    {
        if (extensions == null)
            return;
        foreach (string value in extensions.Split(','))
        {
            string extension = value.Trim();
            if (extension.Length == 0)
                continue;
            if (!extension.StartsWith(".", StringComparison.Ordinal))
                extension = "." + extension;
            if (!collection.Contains(extension))
            {
                collection.Add(extension);
            }
        }
    }


}

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
			return m_title;
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
	/// 	since it will be assumed that the purpose of executing the query is purely to count the
    ///     rows.  If <c>false</c>, all columns specified by the query definition are included
	/// 	in the query.</param>
    ///
	/// <param name="macroResolver">A delegate for resolving macros in the query.  May be
	/// 	<c>null</c>.</param>
    /// 
    /// <param name="columnMap">Where to store a column map: element <c>i,j</c> is the index of
	/// 	the column in the query results that's mapped from <c>Columns[i]</c> and one of
	/// 	<c>ColumnDefinition.ViewColumnName</c> (if <c>j</c> is 0),
	/// 	<c>ColumnDefinition.ViewColumnName2</c> (if <c>j</c> is 1), or
	/// 	<c>ColumnDefinition.ViewColumnName3</c> (if <c>j</c> is 2).  If
    ///     <paramref name="countOnly"/> is true, <c>null</c> is stored in
    ///     <paramref name="columnMap"/></param>
	///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body"), SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
    public LearningStoreQuery CreateQuery(LearningStore learningStore, bool countOnly,
		MacroResolver macroResolver, out int[,] columnMap)
	{
	    // Check parameters
	    if(learningStore == null)
	        throw new ArgumentNullException("learningStore");
	    
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
			Dictionary<string, int> viewColumnsAdded = new Dictionary<string,int>(
				m_columnDefinitions.Count * ColumnDefinition.MaxViewColumns);
			int iColumnDefinition = 0;
			foreach (ColumnDefinition column in Columns)
			{
				for (int iViewColumn = 0;
				     iViewColumn < ColumnDefinition.MaxViewColumns;
					 iViewColumn++)
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
							throw new SlkSettingsException(column.LineNumber,
								AppResources.SlkUtilitiesColumnNotDefined,
								viewColumnName, ViewName);
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
			object value;
			bool noCondition = false; // if true, don't add the condition to the query
			if (condition.MacroName != null)
			{
				if ((macroResolver == null) ||
					((value = macroResolver(condition.MacroName)) == null))
				{
                    if (condition.NoConditionOnNull)
                    {
                        noCondition = true;
                        value = null;
                    }
                    else
                    {
                        throw new SlkSettingsException(condition.LineNumber,
                            AppResources.SlkUtilitiesMacroNotDefined, condition.MacroName);
                    }
				}
			}
			else
			if (condition.Value != null)
				value = new XmlValue(condition.Value);
			else
				value = null;
			if (!noCondition)
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
    ///
    /// <param name="dataRow">A <c>DataRow</c> from a <c>DataTable</c> returned from a
	/// 	LearningStore query created by <c>CreateQuery</c>.</param>
    /// 
    /// <param name="columnMap">The <c>columnMap</c> returned from
	/// 	<c>QueryDefinition.CreateQuery</c>.</param>
	///
    /// <param name="spWebResolver">A delegate for resolving columns with
	/// 	<c>ColumnDefinition.RenderAs</c> equal to <c>ColumnRenderAs.SPWebName</c> May be
	/// 	<c>null</c>.</param>
	///
    /// <returns>
	/// An array of <c>Columns.Count</c> <c>RenderedCell</c> values, each containing the rendered
	/// text of the corresponding "&lt;Column&gt;" child element of this "&lt;Query&lt;" element.
    /// For a column using <c>ColumnRenderAs.SPWebName</c>, the corresponding element of the array
    /// will be of type <c>WebNameRenderedCell</c>.
	/// </returns>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "1#"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public RenderedCell[] RenderQueryRowCells(DataRow dataRow, int[,] columnMap,
		SPWebResolver spWebResolver)
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
                        cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat, cellValue);
					else
						cellToolTip = null;
					if (columnDefinition.CellFormat != null)
					{
						text = FormatValue(cellValue, columnDefinition.CellFormat);
						cellValue = text;
						cellSortKey = text.ToLower(CultureInfo.CurrentCulture);
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
                        dateTime = ((DateTime)cellValue).ToLocalTime();
					}
					catch (InvalidCastException)
					{
						throw new SlkSettingsException(columnDefinition.LineNumber,
							AppResources.SlkUtilitiesViewColumnNameNonDateTime);
					}
					cellValue = cellSortKey = dateTime;
					if (columnDefinition.CellFormat != null)
						cellValue = FormatValue(dateTime, columnDefinition.CellFormat);
                    cellId = null;
                    if (columnDefinition.ToolTipFormat != null)
                        cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat, dateTime);
					else
						cellToolTip = null;
				}
				break;

			case ColumnRenderAs.SPWebName:

                cellWebGuid = GetQueryCell<Guid>(dataRow, columnMap, columnDefinition,
                    iColumnDefinition, 0);
                cellSiteGuid = GetQueryCell<Guid>(dataRow, columnMap, columnDefinition,
                    iColumnDefinition, 1);
                if (spWebResolver != null)
                    spWebResolver(cellWebGuid.Value, cellSiteGuid.Value, out text, out cellWebUrl);
                else
                    text = null;
				if (text == null)
					text = cellWebGuid.Value.ToString();
				cellValue = text;
                cellSortKey = text.ToLower(CultureInfo.CurrentCulture);
				cellId = null;
				if (columnDefinition.ToolTipFormat != null)
                    cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat, text);
				else
					cellToolTip = null;
				break;

			case ColumnRenderAs.Link:

                text = GetQueryCell<string>(dataRow, columnMap, columnDefinition,
                    iColumnDefinition, 0);
                cellId = GetQueryCell<LearningStoreItemIdentifier>(dataRow, columnMap,
					columnDefinition, iColumnDefinition, 1);
				cellValue = text;
                cellSortKey = text.ToLower(CultureInfo.CurrentCulture);
				if (columnDefinition.ToolTipFormat != null)
                    cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat, text);
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
                    cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat, text);
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
                    text = string.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesPointsValue, FormatValue(finalPointsRounded, columnDefinition.CellFormat));
                    textNotRounded = string.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesPointsValue, finalPoints);
				}
				else
				{
					// FinalPoints is NULL
					text = AppResources.SlkUtilitiesPointsNoValue;
					textNotRounded = AppResources.SlkUtilitiesPointsNoValue;
                }
                if (!noPointsPossible)
                {
                    // PointsPossible is not NULL
                    text = String.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesPointsPossible, text,
                        FormatValue(pointsPossibleRounded, columnDefinition.CellFormat));
                    textNotRounded = String.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesPointsPossible, textNotRounded,
                        pointsPossible);
                }
				cellValue = text;
				cellId = null;
				if ((columnDefinition.ToolTipFormat != null) &&
                    (!noFinalPoints || !noPointsPossible))
				{
                    cellToolTip = String.Format(CultureInfo.CurrentCulture, columnDefinition.ToolTipFormat,
						textNotRounded);
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
                text = String.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesSubmitted, countCompletedOrFinal, countTotal);
                cellValue = text;
                cellId = null;
                cellToolTip = null;
				cellSortKey = countCompletedOrFinal;
				break;

			default:

				throw new SlkSettingsException(columnDefinition.LineNumber,
					AppResources.SlkUtilitiesUnknownRenderAsValue, columnDefinition.RenderAs);
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
	/// 	sort.</param>
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
	public static void SortRenderedRows(IList<RenderedCell[]> renderedRows, int columnIndex,
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
				throw new ArgumentException(AppResources.SlkUtilitiesColumnIndexOutOfRange, "columnIndex");

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
				throw new InvalidOperationException(String.Format(AppResources.SlkUtilitiesCannotSortColumnType, sortKeyX.GetType().Name));
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
	/// 	<c>QueryDefinition.CreateQuery</c>.</param>
	///
    /// <param name="columnDefinition">The <c>ColumnDefinition</c> of the cell.</param>
	///
    /// <param name="iColumnDefinition">The index of <c>ColumnDefinition</c> within
	/// 	<c>QueryDefinition.Columns</c>.</param>
	///
    /// <param name="iViewName">The index of the "ViewColumnName*" attribute: 0 for
	/// 	"ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
	///
	static T GetQueryCell<T>(DataRow dataRow, int[,] columnMap, ColumnDefinition columnDefinition,
		int iColumnDefinition, int iViewName)
	{
		bool isNull;
		T value = GetQueryCell<T>(dataRow, columnMap, columnDefinition, iColumnDefinition,
			iViewName, out isNull);
		if (isNull)
		{
			throw new SlkSettingsException(columnDefinition.LineNumber,
				AppResources.SlkUtilitiesColumnReturnedNull, GetViewColumnName(iColumnDefinition));
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
	/// 	<c>QueryDefinition.CreateQuery</c>.</param>
	///
    /// <param name="columnDefinition">The <c>ColumnDefinition</c> of the cell.</param>
	///
    /// <param name="iColumnDefinition">The index of <c>ColumnDefinition</c> within
	/// 	<c>QueryDefinition.Columns</c>.</param>
	///
    /// <param name="iViewName">The index of the "ViewColumnName*" attribute: 0 for
	/// 	"ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
	///
	/// <param name="isNull">If the cell value is <c>DBNull</c>, <paramref name="isNull"/> is set
	/// 	to <c>true</c> and <c>default(T)</c> is returned.  Otherwise,
	/// 	<paramref name="isNull"/> is set to <c>false</c>.</param>
	///
	static T GetQueryCell<T>(DataRow dataRow, int[,] columnMap, ColumnDefinition columnDefinition,
		int iColumnDefinition, int iViewName, out bool isNull)
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
				AppResources.SlkUtilitiesColumnReturnedUnexpectedType,
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
	/// 	"ViewColumnName", 1 for "ViewColumnName2", and so on.</param>
    ///
    /// <returns>
	/// 	"ViewColumnName" if <paramref name="iViewName"/> is 0, "ViewColumnName2" if
	/// 	<paramref name="iViewName"/> is 1, and so on.
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
	/// 	the default conversion to <c>System.String</c> is used.</param>
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
	/// 	<paramref name="title"/> is <c>null</c>, the value of <c>Name</c> is used.</param>
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
	/// 	child element to add.</param>
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
	/// 	element to add.</param>
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
	/// 	element to add.</param>
	///
	internal void AddSort(SortDefinition sortDefinition)
	{
		m_sortDefinitions.Add(sortDefinition);
	}
}

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
			return m_title;
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
			return m_nullDisplayString;
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
			return m_toolTipFormat;
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
	/// 	<paramref name="title"/> is <c>null</c>, the value of <c>ViewColumnName</c> is used.
	/// 	</param>
	///
	/// <param name="renderAs">The value to use for the <c>RenderAs</c> property.</param>
	///
	/// <param name="viewColumnName">The value to use for the <c>ViewColumnName</c> property.
	/// 	</param>
	///
	/// <param name="viewColumnName2">The value to use for the <c>ViewColumnName2</c> property.
	/// 	</param>
	///
	/// <param name="viewColumnName3">The value to use for the <c>ViewColumnName3</c> property.
	/// 	</param>
	///
	/// <param name="cellFormat">The value to use for the <c>CellFormat</c> property.</param>
	///
	/// <param name="toolTipFormat">The value to use for the <c>ToolTipFormat</c> property.
	/// 	</param>
	///
	/// <param name="nullDisplayString">The value to use for the <c>NullDisplayString</c> property.
	/// 	</param>
	///
	/// <param name="wrap">The value to use for the <c>Wrap</c> property.</param>
	///
	/// <param name="lineNumber">The line number of the "&lt;Column&gt;" element with the
	/// 	XML file.</param>
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
		case "Submitted":
			m_renderAs = ColumnRenderAs.Submitted;
			break;
		}
	}
}

/// <summary>
/// Specifies how to render the a column defined by a <c>ColumnDefinition</c>.
/// </summary>
///
[SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
public enum ColumnRenderAs
{
	/// <summary>
	/// Render the column as a string, converted from the column specified by
	/// <c>ColumnDefinition.ViewColumnName</c>.
	/// </summary>
	Default,

	/// <summary>
	/// Render the column as a string, converted from the column specified by
	/// <c>ColumnDefinition.ViewColumnName</c>, which must be a date/time value represented in the
	/// UTC (GMT) time zone.  The date/time is converted to local time for display purposes.
	/// </summary>
	UtcAsLocalDateTime,

	/// <summary>
	/// Render the name of an SPWeb.  The column specified by
	/// <c>ColumnDefinition.ViewColumnName</c> must contain the SPWeb GUID, and the column
	/// specified by <c>ColumnDefinition.ViewColumnName2</c> must contain the SPSite GUID.
	/// </summary>
	SPWebName,

	/// <summary>
	/// Render a link to another page.  The column specified by
	/// <c>ColumnDefinition.ViewColumnName</c> must contain the title of the link, and the column
	/// specified by <c>ColumnDefinition.ViewColumnName2</c> must contain the
	/// <c>LearningStoreItemIdentifier</c> of the information to display in the linked-to page.
	/// </summary>
	Link,

	/// <summary>
	/// Render the status of a learner assignment.  The column specified by
	/// <c>ColumnDefinition.ViewColumnName</c> must contain a <c>bool</c> value: <c>true</c> if the
	/// learner assignment has been completed, <c>false</c> if not.  The column specified by
	/// <c>ColumnDefinition.ViewColumnName2</c> must contain the <c>AttemptStatus</c> of the
	/// learner assignment.
	/// </summary>
	LearnerAssignmentStatus,

	/// <summary>
	/// Render the number of points the learner received (if applicable) and the number of points
	/// possible for the assignment (if applicable); for example, "7/10".  The column specified by
	/// <c>ColumnDefinition.ViewColumnName</c> must contain the number of points the learner
	/// received (e.g. FinalPoints column in LearnerAssignmentLearnerView); this value may be NULL
	/// if the assignment does not award points, or if the assignment hasn't been graded.
	/// The column specified by <c>ViewColumnName2</c> must contain the nominal maximum number of
	/// points that learners may receive on the assignment (e.g. AssignmentPointsPossible column
	/// in LearnerAssignmentLearnerView); may be NULL if not applicable.
	/// </summary>
	ScoreAndPossible,


	/// <summary>
	/// Render the number of learner assignments submitted and the number of learner assignments
	/// for the assignment; for example, "10/12".  Each value is an integer.  Neither value can be
	/// NULL.
	/// </summary>
	Submitted,
}

/// <summary>
/// A "&lt;Condition&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
/// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
/// </summary>
///
[DebuggerDisplay("ConditionDefinition {ViewColumnName} {Operator} ...")]
public class ConditionDefinition
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
	/// Holds the value of the <c>Operator</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	LearningStoreConditionOperator m_operator;

	/// <summary>
	/// Holds the value of the <c>Value</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_value;

	/// <summary>
	/// Holds the value of the <c>MacroName</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_macroName;

	/// <summary>
	/// Holds the value of the <c>NoConditionOnNull</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool m_noConditionOnNull;

	/// <summary>
	/// Holds the value of the <c>LineNumber</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int m_lineNumber;

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
	/// The value of the "Operator" attribute.  This is the operator used to compare the
	/// database column value with the value obtained using the <c>Value</c> or
	/// <c>MacroName</c> property.
	/// </summary>
	public LearningStoreConditionOperator Operator
	{
		[DebuggerStepThrough]
		get
		{
			return m_operator;
		}
	}

	/// <summary>
	/// The value of the "Value" attribute.  This is the value (if any) that the database
	/// column value is being compared against; <c>null</c> if no value was specified.
	/// </summary>
	public string Value
	{
		[DebuggerStepThrough]
		get
		{
			return m_value;
		}
	}

	/// <summary>
	/// The value of the "MacroName" attribute.  This is the name of the macro variable that
	/// the value (if any) that the database column value is being compared against;
	/// <c>null</c> if no macro name was specified.
	/// </summary>
	public string MacroName
	{
		[DebuggerStepThrough]
		get
		{
			return m_macroName;
		}
	}

	/// <summary>
	/// The value of the "NoConditionOnNull" attribute.  If <c>true</c>, then this condition is
	/// omitted from the query if the value of the macro is <c>null</c>.
	/// </summary>
	public bool NoConditionOnNull
	{
		[DebuggerStepThrough]
		get
		{
			return m_noConditionOnNull;
		}
	}

	/// <summary>
	/// The line number of the "&lt;Condition&gt;" element with the XML file.
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
	/// <param name="viewColumnName">The value to use for the <c>ViewColumnName</c> property.
	/// 	</param>
	///
	/// <param name="oper">The value to use for the <c>Operator</c> property.  Note that
	/// 	if this value is "IsNull" or "IsNotNull" then
	/// 	<c>LearningStoreConditionOperator.Equal</c> or
	/// 	<c>LearningStoreConditionOperator.NotEqual</c>, respectively, is used for the
	/// 	<c>Operator</c> property; in this case <c>null</c> should be used for
	/// 	<paramref name="value"/>.</param>
	///
	/// <param name="value">The value to use for the <c>Value</c> property.</param>
	///
	/// <param name="macroName">The value to use for the <c>MacroName</c> property.</param>
	///
	/// <param name="noConditionOnNull">The value to use for the <c>NoConditionOnNull</c> property.
	/// 	</param>
	///
	/// <param name="lineNumber">The line number of the "&lt;Condition&gt;" element with the
	/// 	XML file.</param>
	///
	/// <remarks>
	/// Note that one of <paramref name="value"/> and <paramref name="macroName"/> must be
	/// non-null unless <paramref name="oper"/> equals "IsNull" or "IsNotNull".
	/// </remarks>
	///
	internal ConditionDefinition(string viewColumnName, string oper, string value,
		string macroName, bool noConditionOnNull, int lineNumber)
	{
		// store state
		m_viewColumnName = viewColumnName;
		switch (oper)
		{
		case "Equal":
			m_operator = LearningStoreConditionOperator.Equal;
			break;
		case "GreaterThan":
			m_operator = LearningStoreConditionOperator.GreaterThan;
			break;
		case "GreaterThanEqual":
			m_operator = LearningStoreConditionOperator.GreaterThanEqual;
			break;
		case "LessThan":
			m_operator = LearningStoreConditionOperator.LessThan;
			break;
		case "LessThanEqual":
			m_operator = LearningStoreConditionOperator.LessThanEqual;
			break;
		case "NotEqual":
			m_operator = LearningStoreConditionOperator.NotEqual;
			break;
		case "IsNull":
			m_operator = LearningStoreConditionOperator.Equal;
			if ((value != null) || (macroName != null))
			{
				throw new ArgumentException(
					AppResources.SlkUtilitiesValueNullifIsNull,
					"operator");
			}
			break;
		case "IsNotNull":
			m_operator = LearningStoreConditionOperator.NotEqual;
			if ((value != null) || (macroName != null))
			{
				throw new ArgumentException(
					AppResources.SlkUtilitiesValueNullifIsNotNull,
					"operator");
			}
			break;
		}
		m_value = value;
		m_macroName = macroName;
		m_noConditionOnNull = noConditionOnNull;
		m_lineNumber = lineNumber;

		// only one of <value> and <macroName> may be provided
		if ((value != null) && (macroName != null))
		{
			throw new ArgumentException(
				AppResources.SlkUtilitiesOneValueNonNull,
				"value");
		}

		// if <noConditionOnNull> is true, <macroName> must be provided
		if (noConditionOnNull && (macroName == null))
		{
			throw new ArgumentException(
				AppResources.SlkUtilitiesMacroNameNotProvided,
				"noConditionOnNull");
		}
	}
}

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
	/// 	</param>
	///
	/// <param name="ascending">The value to use for the <c>Ascending</c> property.</param>
	///
	internal SortDefinition(string viewColumnName, bool ascending)
	{
		m_viewColumnName = viewColumnName;
		m_ascending = ascending;
	}
}

/// <summary>
/// A delegate which resolves a macro name (used within a "MacroName" attribute of a
/// "&lt;Condition&gt;" element in an XML file with schema
/// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings") to a value.
/// </summary>
///
/// <param name="macroName">The name of the macro.</param>
///
/// <returns>
/// The value of the macro, or <c>null</c> if the macro is not defined.
/// </returns>
///
public delegate object MacroResolver(string macroName);

/// <summary>
/// A delegate which resolves an SPWeb GUID and an SPSite GUID into an SPWeb name and URL.
/// </summary>
///
/// <param name="spWebGuid">The GUID of the SPWeb.</param>
///
/// <param name="spSiteGuid">The GUID of the SPSite containing the SPWeb.</param>
///
/// <param name="webName">Where to store the name of the SPWeb, or <c>null</c> if the SPWeb
///     cannot be found.</param>
///
/// <param name="webUrl">Where to store the URL of the SPWeb, or <c>null</c> if the SPWeb
///     cannot be found.</param>
///
public delegate void SPWebResolver(Guid spWebGuid, Guid spSiteGuid, out string webName,
    out string webUrl);

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
			return m_title;
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
	/// 	<paramref name="title"/> is <c>null</c>, the value of <c>Name</c> is used.</param>
	///
	/// <param name="defaultQueryName">The value to use for the <c>DefaultQueryName</c>
	/// 	property.</param>
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

/// <summary>
/// Represents the data in one column of one row of the results of a query generated by
/// <c>QueryDefinition.CreateQuery</c>.  <c>RenderedCell</c> values are generated by
/// <c>QueryDefinition.RenderQueryRowCells</c>.  <c>RenderedCell</c> values are generated by
/// </summary>
///
public class RenderedCell
{
	//////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>Value</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object m_value;

	/// <summary>
	/// Holds the value of the <c>SortKey</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	object m_sortKey;

	/// <summary>
	/// Holds the value of the <c>Id</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	LearningStoreItemIdentifier m_id;

	/// <summary>
	/// Holds the value of the <c>ToolTip</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_toolTip;

	/// <summary>
	/// Holds the value of the <c>Wrap</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	bool m_wrap;

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the rendered value of the cell.  In some cases, such as <c>ColumnRenderAs.Default</c>
	/// rendering with no <c>ColumnDefinition.CellFormat</c>, this is the original value returned
	/// from the query.  In other cases this is a formatted string.
	/// </summary>
	public object Value
	{
		[DebuggerStepThrough]
		get
		{
			return m_value;
		}
	}

	/// <summary>
	/// Gets the sort key of the cell.  When sorting cells, compare <c>SortKey</c> values.
	/// </summary>
	public object SortKey
	{
		[DebuggerStepThrough]
		get
		{
			return m_sortKey;
		}
	}

	/// <summary>
	/// Gets the <c>LearningStoreItemIdentifier</c>, if any, associated with the cell, or
	/// <c>null</c> if none.
	/// </summary>
	public LearningStoreItemIdentifier Id
	{
		[DebuggerStepThrough]
		get
		{
			return m_id;
		}
	}

	/// <summary>
	/// Gets the tooltip text, if any, associated with the cell, or <c>null</c> if none.
	/// </summary>
	public string ToolTip
	{
		[DebuggerStepThrough]
		get
		{
			return m_toolTip;
		}
	}

	/// <summary>
	/// The value of the "Wrap" attribute: <c>true</c> if this rendered cell can wrap to the next
	/// line, <c>false</c> if not.
	/// </summary>
	public bool Wrap
	{
		[DebuggerStepThrough]
		get
		{
			return m_wrap;
		}
	}

    //////////////////////////////////////////////////////////////////////////////////////////////
    // Public Methods
    //

    /// <summary>
    /// Returns a string representation of this object -- the same as <r>Value</r>, except
    /// <r>ToString</r> returns <n>String.Empty</n> instead of <n>null</n>.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
		if (m_value == null)
			return String.Empty;
		else
			return m_value.ToString();
    }

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Internal Methods
	//

	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="value">The value of the <c>Value</c> property.</param>
	///
	/// <param name="sortKey">The value of the <c>SortKey</c> property.</param>
	///
	/// <param name="id">The value of the <c>Id</c> property.</param>
	///
	/// <param name="toolTip">The value of the <c>ToolTip</c> property.</param>
	///
	/// <param name="wrap">The value of the <c>Wrap</c> property.</param>
	///
	internal RenderedCell(object value, object sortKey, LearningStoreItemIdentifier id,
		string toolTip, bool wrap)
	{
		m_value = value;
		m_sortKey = sortKey;
		m_id = id;
		m_toolTip = toolTip;
		m_wrap = wrap;
	}
}

/// <summary>
/// Represents the data in one column of one row of the results of a query generated by
/// <c>QueryDefinition.CreateQuery</c>.  Specifically, represents the data of a cell that
/// represents the name of a SharePoint Web site.  <c>WebNameRenderedCell</c> is based on
/// <c>RenderedCell</c>.
/// </summary>
public class WebNameRenderedCell : RenderedCell
{
	//////////////////////////////////////////////////////////////////////////////////////////////
	// Private Fields
	//

	/// <summary>
	/// Holds the value of the <c>SPSiteGuid</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spSiteGuid;

	/// <summary>
	/// Holds the value of the <c>SPWebGuid</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	Guid m_spWebGuid;

	/// <summary>
	/// Holds the value of the <c>SPWebUrl</c> property.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	string m_spWebUrl;

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	//

	/// <summary>
	/// Gets the GUID of the SharePoint site collection (<c>SPSite</c>) represented by this cell.
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
	/// Gets the GUID of the SharePoint Web site (<c>SPWeb</c>) represented by this cell.
	/// </summary>
	public Guid SPWebGuid
	{
		[DebuggerStepThrough]
		get
		{
			return m_spWebGuid;
		}
	}

	/// <summary>
	/// Gets the URL of the SharePoint Web site (<c>SPWeb</c>) represented by this cell.
	/// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
    public string SPWebUrl
	{
		[DebuggerStepThrough]
		get
		{
			return m_spWebUrl;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Internal Methods
	//

	/// <summary>
	/// Initializes an instance of this class.
	/// </summary>
	///
	/// <param name="value">The value of the <c>Value</c> property.</param>
	///
	/// <param name="sortKey">The value of the <c>SortKey</c> property.</param>
	///
	/// <param name="id">The value of the <c>Id</c> property.</param>
	///
	/// <param name="toolTip">The value of the <c>ToolTip</c> property.</param>
	///
	/// <param name="wrap">The value of the <c>Wrap</c> property.</param>
    /// 
    /// <param name="spSiteGuid">The value of the <c>SPSiteGuid</c> property.</param>
    /// 
    /// <param name="spWebGuid">The value of the <c>SPWebGuid</c> property.</param>
    /// 
    /// <param name="spWebUrl">The value of the <c>SPWebUrl</c> property.</param>
    ///
    internal WebNameRenderedCell(object value, object sortKey, LearningStoreItemIdentifier id,
    		string toolTip, bool wrap, Guid spSiteGuid, Guid spWebGuid, string spWebUrl) :
        base(value, sortKey, id, toolTip, wrap)
	{
		m_spSiteGuid = spSiteGuid;
		m_spWebGuid = spWebGuid;
		m_spWebUrl = spWebUrl;
	}
}

/// <summary>
/// Indicates an error within an SLK Settings XML file, i.e. an XML file with schema
/// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
/// </summary>
///
[Serializable]
public class SlkSettingsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
    /// </summary>
    public SlkSettingsException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SlkSettingsException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SlkSettingsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected SlkSettingsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
    
	/// <summary>
	/// Initializes an instance of this class, by formatting an error message.
	/// </summary>
	///
	/// <param name="format">A <c>String.Format</c>-style format string.</param>
	///
	/// <param name="args">Formatting arguments.</param>
	///
	internal SlkSettingsException(string format, params object[] args) :
        base(String.Format(CultureInfo.CurrentCulture, format, args))
	{
	}

	/// <summary>
	/// Initializes an instance of this class, by formatting an error message and prepending
	/// the line number within the XML file.
	/// </summary>
	///
	/// <param name="xmlReader">The line number of the exception message will include the
	/// 	current line number of this <c>XmlReader</c>.</param>
	///
	/// <param name="format">A <c>String.Format</c>-style format string.</param>
	///
	/// <param name="args">Formatting arguments.</param>
	///
	internal SlkSettingsException(XmlReader xmlReader, string format, params object[] args) :
        base(String.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesSettingsException, ((IXmlLineInfo)xmlReader).LineNumber,
            String.Format(CultureInfo.CurrentCulture, format, args)))
	{
	}

	/// <summary>
	/// Initializes an instance of this class, by formatting an error message and prepending
	/// the line number within the XML file.
	/// </summary>
	///
	/// <param name="lineNumber">The line number (within the XML file) to include in the
	/// 	exception message.</param>
	///
	/// <param name="format">A <c>String.Format</c>-style format string.</param>
	///
	/// <param name="args">Formatting arguments.</param>
	///
	internal SlkSettingsException(int lineNumber, string format, params object[] args) :
        base(String.Format(CultureInfo.CurrentCulture, AppResources.SlkUtilitiesSettingsException, lineNumber,
            String.Format(CultureInfo.CurrentCulture, format, args)))
	{
	}
}

/// <summary>
/// Wraps a string in an object that includes an implementation of <c>IConvertable</c> that
/// converts the string from an XML-format value (e.g. "3.14" for a floating-point number,
/// "2006-03-21" for a date) to a given type (e.g. <c>double</c>, <c>DateTime</c>, etc.)
/// </summary>
///
internal class XmlValue : IConvertible
{
    string m_stringValue;

    public XmlValue(string stringValue)
    {
        m_stringValue = stringValue;
    }

    TypeCode IConvertible.GetTypeCode()
    {
        return TypeCode.Object;
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
        return XmlConvert.ToBoolean(m_stringValue);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
        return XmlConvert.ToByte(m_stringValue);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
        return XmlConvert.ToChar(m_stringValue);
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
        return XmlConvert.ToDateTime(m_stringValue, XmlDateTimeSerializationMode.Unspecified);
    }

    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
        return XmlConvert.ToDecimal(m_stringValue);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
        return XmlConvert.ToDouble(m_stringValue);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
        return XmlConvert.ToInt16(m_stringValue);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
        return XmlConvert.ToInt32(m_stringValue);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
        return XmlConvert.ToInt64(m_stringValue);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
        return XmlConvert.ToSByte(m_stringValue);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
        return XmlConvert.ToSingle(m_stringValue);
    }

    string IConvertible.ToString(IFormatProvider provider)
    {
        return m_stringValue;
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
        if (conversionType == typeof(Guid))
            return XmlConvert.ToGuid(m_stringValue);
		else
		if (conversionType == typeof(bool))
			return XmlConvert.ToBoolean(m_stringValue);
		else

		if (conversionType == typeof(byte))
			return XmlConvert.ToByte(m_stringValue);
		else

		if (conversionType == typeof(char))
			return XmlConvert.ToChar(m_stringValue);
		else

		if (conversionType == typeof(DateTime))
			return XmlConvert.ToDateTime(m_stringValue, XmlDateTimeSerializationMode.Unspecified);
		else

		if (conversionType == typeof(decimal))
			return XmlConvert.ToDecimal(m_stringValue);
		else

		if (conversionType == typeof(double))
			return XmlConvert.ToDouble(m_stringValue);
		else

		if (conversionType == typeof(short))
			return XmlConvert.ToInt16(m_stringValue);
		else

		if (conversionType == typeof(int))
			return XmlConvert.ToInt32(m_stringValue);
		else

		if (conversionType == typeof(long))
			return XmlConvert.ToInt64(m_stringValue);
		else

		if (conversionType == typeof(sbyte))
			return XmlConvert.ToSByte(m_stringValue);
		else

		if (conversionType == typeof(float))
			return XmlConvert.ToSingle(m_stringValue);
		else

		if (conversionType == typeof(string))
			return m_stringValue;
		else

		if (conversionType == typeof(ushort))
			return XmlConvert.ToUInt16(m_stringValue);
		else

		if (conversionType == typeof(uint))
			return XmlConvert.ToUInt32(m_stringValue);
		else

		if (conversionType == typeof(ulong))
			return XmlConvert.ToUInt64(m_stringValue);
		else
			throw new InvalidCastException();
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
        return XmlConvert.ToUInt16(m_stringValue);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
        return XmlConvert.ToUInt32(m_stringValue);
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
        return XmlConvert.ToUInt64(m_stringValue);
    }
}

}

