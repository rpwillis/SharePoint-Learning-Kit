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
    /// <summary>Contains methods for parsing a file with schema "urn:schemas-microsoft-com:sharepoint-learning-kit:settings" and managing the results.</summary>
    public class SlkSettings
    {
        Dictionary<string,QueryDefinition> queryDictionary = new Dictionary<string,QueryDefinition>();
        Dictionary<string,QuerySetDefinition> querySetDictionary = new Dictionary<string,QuerySetDefinition>();
        Collection<string> approvedAttachmentTypes;
        Collection<string> eLearningIisCompatibilityModeExtensions;
        Collection<string> nonELearningIisCompatibilityModeExtensions;
        List<QueryDefinition> queryDefinitions;
        List<QuerySetDefinition> querySetDefinitions;

#region properties
        /// <summary>The settings for email.</summary>
        public EmailSettings EmailSettings { get; private set; }

        /// <summary>The settings for Drop Box.</summary>
        public DropBoxSettings DropBoxSettings { get; private set; }

        /// <summary>The settings for Quick Assignments.</summary>
        public QuickAssignmentSettings QuickAssignmentSettings { get; private set; }

        /// <summary>The type of the domain group enumerator.</summary>
        public string DomainGroupEnumeratorType { get; private set; }
        /// <summary>The assembly of the domain group enumerator.</summary>
        public string DomainGroupEnumeratorAssembly { get; private set; }

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
                return new ReadOnlyCollection<string>(approvedAttachmentTypes);
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
                return new ReadOnlyCollection<string>(eLearningIisCompatibilityModeExtensions);
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
                return new ReadOnlyCollection<string>(nonELearningIisCompatibilityModeExtensions);
            }
        }

        /// <summary>
        /// Gets the value of the "LoggingOptions" attribute of the "&lt;Settings&gt;" element
        /// within the SLK Settings XML file.  This specifies what type of logging (if any) of SCORM
        /// sequencing actions is enabled for content that supports sequencing.
        /// </summary>
        public LoggingOptions LoggingOptions { get; private set; }

        /// <summary>
        /// Gets the value of the "MaxAttachmentKilobytes" attribute of the "&lt;Settings&gt;" element
        /// within the SLK Settings XML file.  This is the maximum size of a file that a learner may
        /// attach to an attachment-type question in LRM content, measured in kilobytes.
        /// </summary>
        public int MaxAttachmentKilobytes { get; private set; }

        /// <summary>
        /// Gets the value of the "PackageCacheExpirationMinutes" attribute of the "&lt;Settings&gt;"
        /// element within the SLK Settings XML file.  This is the minimum amount of time that a
        /// package is retained in the SharePoint Learning Kit file system cache, measured in minutes.
        /// If "PackageCacheExpirationMinutes" is absent, <c>null</c> is returned, which should be
        /// interpreted as no expiration (i.e. packages are never removed from the cache).
        /// </summary>
        public Nullable<int> PackageCacheExpirationMinutes { get; private set; }

        ///<summary>Indicates whether to hide disabled users on the assignment page.</summary>
        public bool HideDisabledUsers { get; private set; }

        /// <summary>Indicates whether to automatically select all learners when creating a new assignment.</summary>
        public bool SelectAllLearners { get; private set; }

        /// <summary>Indicates whether to automatically select all instructors when creating a new assignment.</summary>
        public bool SelectAllInstructors { get; private set; }

        /// <summary>The custom property list to use.</summary>
        public string CustomPropertyList { get; private set; }

        /// <summary>Indicates whether to use grades as well as points.</summary>
        public bool UseGrades { get; private set; }

        /// <summary>Indicates whether to hide points for non-scorm packages. Only takes affect if UseGrades = true.</summary>
        public bool HidePointsForNonELearning { get; private set; }

        /// <summary>The url to use to link to a learner report.</summary>
        public string LearnerReportUrl { get; private set; }

        /// <summary>
        /// Gets the value of the "PackageCacheLocation" attribute of the "&lt;Settings&gt;" element
        /// within the SLK Settings XML file.  This is the full path to the SharePoint Learning Kit
        /// package cache location used for each front-end server.  This path can be a network file
        /// share so multiple front-end servers can use the same package cache.  Note that this path
        /// may contain environment variable references such as "%TEMP%".
        /// </summary>
        public string PackageCacheLocation { get; private set; }

        /// <summary>
        /// Gets the value of the "UserWebListMruSize" attribute of the "&lt;Settings&gt;" element
        /// within the SLK Settings XML file.  This is the number of Web sites displayed in the
        /// most-recently-used list of Web sites in the SharePoint Learning Kit E-Learning Actions
        /// page.  All Web site references are retained, and the user can display them by clicking a
        /// link to see the complete list.  The MRU is just the most-recently-used subset of Web sites
        /// that's displayed by default.
        /// </summary>
        public int UserWebListMruSize { get; private set; }

        ///<summary>Indicates whether to automatically version document libraries if they are unversioned.</summary>
        public bool AutoVersionLibrariesIfUnversioned { get; private set; }

        ///<summary>Indicates whether to use the standard master page for the SLK application pages.</summary>
        public bool UseMasterPageForApplicationPages { get; private set; }

        /// <summary>
        /// Gets the collection of MIME type mappings parsed from "&lt;MimeTypeMapping&gt;" elements
        /// within the SLK Settings XML file.  Each mapping has a key which is a file name extension
        /// including the leading period (for example, ".jpg" or ".jpeg") and an associated MIME type
        /// (for example, "image/jpeg").  This MIME type mapping table determines the MIME type
        /// returned to the browser when content (for example, images) within e-learning packages are
        /// displayed.
        /// </summary>
        public IDictionary<string, string> MimeTypeMappings { get; private set; }

        /// <summary>
        /// Gets the collection of query definitions parsed from "&lt;Query&gt;" elements within the
        /// SLK Settings XML file.
        /// </summary>
        public ReadOnlyCollection<QueryDefinition> QueryDefinitions
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<QueryDefinition>(queryDefinitions);
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
                return new ReadOnlyCollection<QuerySetDefinition>(querySetDefinitions);
            }
        }

        /// <summary>
        /// Gets the date/time that the SLK Settings file was uploaded to the database.
        /// </summary>
        public DateTime WhenUploaded { get; private set; }
#endregion properties

#region constructors
        /// <summary>Initializes a new instance of <see cref="SlkSettings"/>.</summary>
        /// <param name="xmlReader">The XmlReader containing the settings.</param>
        /// <param name="whenUploaded">The date and time when uploaded.</param>
        public SlkSettings(XmlReader xmlReader, DateTime whenUploaded)
        {
            approvedAttachmentTypes = new Collection<string>();
            eLearningIisCompatibilityModeExtensions = new Collection<string>();
            nonELearningIisCompatibilityModeExtensions = new Collection<string>();
            MimeTypeMappings = new Dictionary<string, string>(50, StringComparer.OrdinalIgnoreCase);
            InitalizeMimeTypeMappings();
            queryDefinitions = new List<QueryDefinition>(20);
            querySetDefinitions = new List<QuerySetDefinition>(10);
            WhenUploaded = whenUploaded;
            DropBoxSettings = new DropBoxSettings();
            QuickAssignmentSettings = new QuickAssignmentSettings();
            EmailSettings = new EmailSettings();
            ParseSettingsFile(xmlReader);
        }
#endregion constructors

#region public static methods
        /// <summary>Reads a boolean attribute.</summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>A boolean value. Defaults to false if not present.</returns>
        public static bool BooleanAttribute(XmlReader reader, string attribute)
        {
            string value = reader.GetAttribute(attribute);
            if (string.IsNullOrEmpty(value) == false)
            {
                return (value.ToUpperInvariant() == "TRUE");
            }
            else
            {
                return false;
            }
        }
#endregion public static methods

        /// <summary>
        /// Parses an SLK Settings XML file, i.e. an XML file with schema
        /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".  Returns a new
        /// <c>SlkSettings</c> object containing information from the parsed file.
        /// </summary>
        /// <param name="xmlReader">An <c>XmlReader</c> positioned at the beginning of the SLK Settings
        ///     XML file.  This <c>XmlReader</c> must have "SlkSettings.xsd" (the SLK Setttings XML
        ///     schema) attached using the <c>XmlReaderSettings</c> parameter of the <c>XmlReader</c>
        ///     constructor, with <c>XmlReaderSettings.ValidationType</c> equal to
        ///     <c>ValidationType.Schema</c>.</param>
        /// <exception cref="SlkSettingsException">
        /// The SLK Settings XML file is invalid.  The exception message includes the line number and
        /// other information about the error.
        /// </exception>
        void ParseSettingsFile(XmlReader xmlReader)
        {
            // Check parameters
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }

            // parse the SLK Settings XML file, and fill in <slkSettings>; use <queryDictionary> (which
            // maps a query name to a QueryDefinition) and <querySetDictionary> (which maps a query set
            // name to a QuerySetDefinition) to check for duplicate and invalid query and query set
            // names (including a query name the same as query set name, which isn't allowed) -- note
            // that the .xsd file requires that all "<Query>" elements appear before any "<QuerySet>"
            // elements
            try
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                    else if (xmlReader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    else
                    {
                        switch (xmlReader.Name)
                        {
                            case "Settings":
                                ParseSettingsAttributes(xmlReader);
                                xmlReader.MoveToElement();
                                break;

                            case "MimeTypeMapping":
                                // Add or override standard mime-type mappings
                                string extension = xmlReader.GetAttribute("Extension");
                                if (string.IsNullOrEmpty(extension) == false)
                                {
                                    extension = extension.ToUpperInvariant();
                                    string mimeType = xmlReader.GetAttribute("MimeType");
                                    // Standard slksettings.xml had invalid png mime-type in
                                    if (extension != ".PNG" && mimeType != "image/x-png")
                                    {
                                        MimeTypeMappings[extension] = mimeType;
                                    }
                                }

                                break;

                            case "DropBoxSettings":
                                DropBoxSettings = new DropBoxSettings(xmlReader);
                                break;

                            case "QuickAssignmentSettings":
                                QuickAssignmentSettings = new QuickAssignmentSettings(xmlReader);
                                break;

                            case "EmailSettings":
                                EmailSettings = new EmailSettings(xmlReader);
                                break;

                            case "Query":
                                ParseQuery(xmlReader);
                                break;

                            case "QuerySet":
                                ParseQuerySet(xmlReader);
                                break;

                            case "DomainGroupEnumerator":
                                DomainGroupEnumeratorType = xmlReader.GetAttribute("Type");
                                DomainGroupEnumeratorAssembly = xmlReader.GetAttribute("Assembly");
                                break;
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesSettingsExceptionDefaultFormat, ex.Message);
            }
            catch (XmlSchemaValidationException ex)
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesSettingsExceptionDefaultFormat, ex.Message);
            }
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
            {
                throw new ArgumentNullException("queryName");
            }

            QueryDefinition query = null;
            queryDictionary.TryGetValue(queryName, out query);
            return query;
        }

        /// <summary>
        /// Locates a <c>QuerySetDefinition</c> in a list of query sets given its name.  If requested,
        /// the list of queries is searched in addition to the list of query sets; in that case, if a
        /// query name matching the query set name is found, a <c>QuerySetDefinition</c> containing
        /// only that query is returned.
        /// </summary>
        ///
        /// <param name="querySetName">The name of the query set (or query) or query to look for.
        ///     </param>
        ///
        /// <param name="searchQueries">If <c>true</c>, the list of queries is searched (for a query
        ///     named <paramref name="querySetName"/>) in addition to the list of query sets.  If
        ///     <c>false</c>, only the list of query sets is searched.</param>
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
            QuerySetDefinition foundQuerySetDef = null;
            if (querySetDictionary.TryGetValue(querySetName, out foundQuerySetDef) == false)
            {
                QueryDefinition foundQueryDef;
                if (queryDictionary.TryGetValue(querySetName, out foundQueryDef))
                {
                    foundQuerySetDef = new QuerySetDefinition(foundQueryDef.Name, foundQueryDef.Title, foundQueryDef.Name);
                    foundQuerySetDef.AddIncludeQuery(foundQueryDef);
                }
            }

            return foundQuerySetDef;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //


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

        static string ParseAttributeAsString(XmlReader xmlReader, string name)
        {
            if (!xmlReader.MoveToAttribute(name))
            {
                return null;
            }
            else
            {
                return xmlReader.ReadContentAsString();
            }
        }

        static bool ParseAttributeAsBoolean(XmlReader xmlReader, string name)
        {
            if (!xmlReader.MoveToAttribute(name))
            {
                return false;
            }
            else
            {
                return xmlReader.ReadContentAsBoolean();
            }
        }

        void ParseSettingsAttributes(XmlReader xmlReader)
        {
            // parse "ApprovedAttachmentTypes" attribute
            ParseExtensionList(xmlReader.GetAttribute("ApprovedAttachmentTypes"), approvedAttachmentTypes);

            // parse "ELearningIisCompatibilityModeExtensions" attribute
            ParseExtensionList(xmlReader.GetAttribute("ELearningIisCompatibilityModeExtensions"), eLearningIisCompatibilityModeExtensions);

            // parse "NonELearningIisCompatibilityModeExtensions" attribute
            ParseExtensionList(xmlReader.GetAttribute("NonELearningIisCompatibilityModeExtensions"), nonELearningIisCompatibilityModeExtensions);

            // parse "PackageCacheLocation" attribute
            PackageCacheLocation = xmlReader.GetAttribute("PackageCacheLocation");

            // parse "LoggingOptions" attribute
            string loggingOptions = xmlReader.GetAttribute("LoggingOptions");
            switch (loggingOptions)
            {
                case "None":
                    LoggingOptions = LoggingOptions.None;
                    break;
                case "LogDetailSequencing":
                    LoggingOptions = LoggingOptions.LogDetailSequencing;
                    break;
                case "LogFinalSequencing":
                    LoggingOptions = LoggingOptions.LogFinalSequencing;
                    break;
                case "LogRollup":
                    LoggingOptions = LoggingOptions.LogRollup;
                    break;
                case "LogAllSequencing":
                    LoggingOptions = LoggingOptions.LogAllSequencing;
                    break;
                case "LogAll":
                    LoggingOptions = LoggingOptions.LogAll;
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

            MaxAttachmentKilobytes = xmlReader.ReadContentAsInt();
            HideDisabledUsers = ParseAttributeAsBoolean(xmlReader, "HideDisabledUsers");
            UseMasterPageForApplicationPages = ParseAttributeAsBoolean(xmlReader, "UseMasterPageForApplicationPages");
            AutoVersionLibrariesIfUnversioned = ParseAttributeAsBoolean(xmlReader, "AutoVersionLibrariesIfUnversioned");
            UseGrades = ParseAttributeAsBoolean(xmlReader, "UseGrades");
            HidePointsForNonELearning = ParseAttributeAsBoolean(xmlReader, "HidePointsForNonELearning");
            SelectAllInstructors = ParseAttributeAsBoolean(xmlReader, "SelectAllInstructors");
            SelectAllLearners = ParseAttributeAsBoolean(xmlReader, "SelectAllLearners");
            LearnerReportUrl = ParseAttributeAsString(xmlReader, "LearnerReportUrl");
            CustomPropertyList = ParseAttributeAsString(xmlReader, "CustomPropertyList");;

            // parse "PackageCacheExpirationMinutes" attribute
            if (!xmlReader.MoveToAttribute("PackageCacheExpirationMinutes"))
            {
                PackageCacheExpirationMinutes = null;
            }
            else
            {
                PackageCacheExpirationMinutes = xmlReader.ReadContentAsInt();
            }

            // parse "UserWebListMruSize" attribute
            if (!xmlReader.MoveToAttribute("UserWebListMruSize"))
            {
                throw new InternalErrorException("SLKSET1003");
            }

            UserWebListMruSize = xmlReader.ReadContentAsInt();
        }

        void ParseQuery(XmlReader xmlReader)
        {
            QueryDefinition unused;
            string queryName = xmlReader.GetAttribute("Name");
            if (queryDictionary.TryGetValue(queryName, out unused))
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesDuplicateQueryName, queryName);
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
                else if (xmlReader.Name == "Column")
                {
                    bool wrap = false;
                    if (xmlReader.MoveToAttribute("Wrap"))
                    {
                        wrap = xmlReader.ReadContentAsBoolean();
                    }
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
                else if (xmlReader.Name == "Condition")
                {
                    bool noConditionOnNull = false;
                    if (xmlReader.MoveToAttribute("NoConditionOnNull"))
                    {
                        noConditionOnNull = xmlReader.ReadContentAsBoolean();
                    }
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
                        throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesConditionException);
                    }
                }
                else if (xmlReader.Name == "Sort")
                {
                    string viewColumnName = xmlReader.GetAttribute("ViewColumnName");
                    xmlReader.MoveToAttribute("Ascending");
                    bool ascending = xmlReader.ReadContentAsBoolean();
                    xmlReader.MoveToElement();
                    queryDefinition.AddSort(new SortDefinition(viewColumnName, ascending));
                }
            }
            queryDefinitions.Add(queryDefinition);
            queryDictionary.Add(queryName, queryDefinition);
        }

        void ParseQuerySet(XmlReader xmlReader)
        {
            string querySetName = xmlReader.GetAttribute("Name");
            QuerySetDefinition toFind;
            if (querySetDictionary.TryGetValue(querySetName, out toFind))
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesDuplicateQuerySetName, querySetName);
            }

            QueryDefinition unused;
            if (queryDictionary.TryGetValue(querySetName, out unused))
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesQuerySetSameAsQuery, querySetName);
            }
            string defaultQueryName = xmlReader.GetAttribute("DefaultQueryName");
            if (!queryDictionary.TryGetValue(defaultQueryName, out unused))
            {
                throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesUndefinedQuery, defaultQueryName);
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
                        throw new SlkSettingsException(xmlReader, Resources.SlkUtilitiesUndefinedQuery, queryName);
                    }
                    querySetDefinition.AddIncludeQuery(queryDef);
                }
            }
            querySetDefinitions.Add(querySetDefinition);
            querySetDictionary.Add(querySetName, querySetDefinition);
        }

        private static AppResourcesLocal Resources
        {
            get { return SlkCulture.GetResources() ;}
        }

        /// <summary>Adds base mime type mappings.</summary>
        /// <remarks>This is done here as SP2013 adds a header X-Content-Type-Options: nosniff.
        /// This prevents IE from displaying items if no (or wrong) mime-type set and the standard
        /// slksettings.xml had some image types missing.</remarks>
        private void InitalizeMimeTypeMappings()
        {
            MimeTypeMappings[".BMP"] = "image/bmp";
            MimeTypeMappings[".GIF"] = "image/gif";
            MimeTypeMappings[".CSS"] = "text/css";
            MimeTypeMappings[".DOC"] = "application/msword";
            MimeTypeMappings[".DOCM"] = "application/vnd.ms-word.document.macroEnabled.12";
            MimeTypeMappings[".DOCX"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            MimeTypeMappings[".DOTM"] = "application/vnd.ms-word.template.macroEnabled.12";
            MimeTypeMappings[".DOTX"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
            MimeTypeMappings[".HTM"] = "text/html";
            MimeTypeMappings[".HTML"] = "text/html";
            MimeTypeMappings[".JPE"] = "image/jpeg";
            MimeTypeMappings[".JPEG"] = "image/jpeg";
            MimeTypeMappings[".JPG"] = "image/jpeg";
            MimeTypeMappings[".JS"] = "application/javascript";
            MimeTypeMappings[".MP4"] = "video/mp4";
            MimeTypeMappings[".PDF"] = "application/pdf";
            MimeTypeMappings[".PNG"] = "image/png";
            MimeTypeMappings[".POTM"] = "application/vnd.ms-powerpoint.template.macroEnabled.12";
            MimeTypeMappings[".POTX"] = "application/vnd.openxmlformats-officedocument.presentationml.template";
            MimeTypeMappings[".PPAM"] = "application/vnd.ms-powerpoint.addin.macroEnabled.12";
            MimeTypeMappings[".PPSM"] = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
            MimeTypeMappings[".PPSX"] = "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
            MimeTypeMappings[".PPT"] = "application/vnd.ms-powerpoint";
            MimeTypeMappings[".PPTM"] = "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
            MimeTypeMappings[".PPTX"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            MimeTypeMappings[".PUB"] = "application/x-mspublisher";
            MimeTypeMappings[".SWF"] = "application/x-shockwave-flash";
            MimeTypeMappings[".TIF"] = "image/tiff";
            MimeTypeMappings[".TIFF"] = "image/tiff";
            MimeTypeMappings[".TXT"] = "text/plain";
            MimeTypeMappings[".VSD"] = "application/vnd.visio";
            MimeTypeMappings[".XBM"] = "image/image/x-xbitmap";
            MimeTypeMappings[".XLAM"] = "application/vnd.ms-excel.addin.macroEnabled.12";
            MimeTypeMappings[".XLS"] = "application/vnd.ms-excel";
            MimeTypeMappings[".XLSB"] = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
            MimeTypeMappings[".XLSM"] = "application/vnd.ms-excel.sheet.macroEnabled.12";
            MimeTypeMappings[".XLSX"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            MimeTypeMappings[".XLTM"] = "application/vnd.ms-excel.template.macroEnabled.12";
            MimeTypeMappings[".XLTX"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
            MimeTypeMappings[".XML"] = "text/xml";
        }

    }
}
