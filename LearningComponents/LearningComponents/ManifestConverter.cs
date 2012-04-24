/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.LearningComponents.Manifest
{
    #region ManifestConverter
    /// <summary>
    /// Holds the results of a conversion from Class Server Index.xml to a SCORM compatible
	/// manifest.
    /// </summary>
    public class ConversionResult
    {
        XPathNavigator m_manifest;
        ValidationResults m_log;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="manifest"><Typ>XPathNavigator</Typ> pointing to the manifest node of the
        /// SCORM compatible manifest generated from the index.xml of a Learning Resource.</param>
        internal ConversionResult(XPathNavigator manifest)
        {
            m_manifest = manifest;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="manifest"><Typ>XPathNavigator</Typ> pointing to the manifest node of the
        /// SCORM compatible manifest generated from the index.xml of a Learning Resource.</param>
        /// <param name="log">Error/warning log containing errors and warnings generated while
        /// parsing the index.xml of the Learning Resource.</param>
        internal ConversionResult(XPathNavigator manifest, ValidationResults log)
        {
            m_manifest = manifest;
            m_log = log;
        }

        /// <summary>
        /// Error/warning log containing errors and warnings generated while
        /// parsing the index.xml of the Learning Resource.
        /// </summary>
        public ValidationResults Log
        {
            get { return m_log; }
        }

        /// <summary>
        /// <Typ>XPathNavigator</Typ> pointing to the manifest node of the
        /// SCORM compatible manifest generated from the index.xml of a Learning Resource.
        /// </summary>
        public XPathNavigator Manifest
        {
            get { return m_manifest; }
        }
    }

    /// <summary>
    /// Converts from a stream containing an Index.xml LRM file to an XmlDocument containing a &lt;manifest&gt; node
    /// of an imsmanifest.xml file.
    /// </summary>
    public class ManifestConverter
    {
        /// <summary>
        /// Handles violations according to the values for ValidationBehavior and FixLmsViolations.
        /// </summary>
        internal class ViolationHandler
        {
            private ValidationBehavior m_behavior;
            private bool m_fixViolations;
            private ValidationResults m_log;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="behavior">Desired validation behavior. See <Typ>ValidationBehavior</Typ>.</param>
            /// <param name="fixLmsViolations">true to fix bad content in the index.xml when possible.</param>
            /// <param name="log">Log in which to log warnings and errors when <c>ValidationBehavior.LogWarning</c>
            /// or <c>ValidationBehavior.LogError</c> is chosen.</param>
            /// <exception cref="ArgumentException"><paramref name="log"/> is null and <c>ValidationBehavior.LogWarning</c>
            /// or <c>ValidationBehavior.LogError</c> was chosen for <paramref name="behavior"/>.</exception>
            internal ViolationHandler(ValidationBehavior behavior, bool fixLmsViolations, ValidationResults log)
            {
                if (log == null &&
                    (behavior == ValidationBehavior.LogError || behavior == ValidationBehavior.LogWarning))
                {
                    throw new ArgumentException(ValidatorResources.LrLogCannotBeNullWhenRequired);
                }
                m_behavior = behavior;
                m_fixViolations = fixLmsViolations;
                m_log = log;
            }

            /// <summary>
            /// Returns the value of "fixLrmViolations".
            /// </summary>
            internal bool FixViolations
            {
                get
                {
                    return m_fixViolations;
                }
            }

            /// <summary>
            /// Adds a message to m_log according to m_behavior.
            /// </summary>
            /// <param name="message"></param>
            internal void Log(string message)
            {
                if (m_behavior == ValidationBehavior.Enforce)
                {
                    throw new InvalidPackageException(message);
                }
                else if (m_behavior == ValidationBehavior.LogWarning)
                {
                    m_log.AddResult(new ValidationResult(false, message));
                }
                else if (m_behavior == ValidationBehavior.LogError)
                {
                    m_log.AddResult(new ValidationResult(true, message));
                }
            }

            /// <summary>
            /// Checks the current <Typ>ValidationBehavior</Typ> and whether <c>fixLmsViolations</c> is set to determine 
            /// whether to return the default value or throw an exception.
            /// </summary>
            /// <param name="defaultValue"></param>
            /// <param name="nodeName"></param>
            /// <returns>The value of <paramref name="defaultValue"/> if <c>fixLmsViolations</c> was set to <c>true</c> in
            /// the constructor of the <Typ>ViolationHandler</Typ>.</returns>
            /// <exception cref="InvalidPackageException">If <c>fixLmsViolations</c> was set to <c>false</c> in
            /// the constructor of the <Typ>ViolationHandler</Typ>.</exception>
            internal string HandleDefaultNodeValue(string defaultValue, string nodeName)
            {
                if (m_behavior == ValidationBehavior.Enforce)
                {
                    throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                        ValidatorResources.LrNodeValueException, nodeName));
                }
                else if (m_fixViolations)
                {
                    Log(String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrDefaultValueUsed,
                        defaultValue, nodeName));
                    return defaultValue;
                }
                else throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, 
                    ValidatorResources.LrFixLmsViolationsNotSet, nodeName));
            }

            /// <summary>
            /// Checks the current <Typ>ValidationBehavior</Typ> and whether <c>fixLmsViolations</c> is set to determine 
            /// whether to return the default value or throw an exception.
            /// </summary>
            /// <param name="defaultValue"></param>
            /// <param name="nodeName"></param>
            /// <param name="attributeName"></param>
            /// <returns>The value of <paramref name="defaultValue"/> if <c>fixLmsViolations</c> was set to <c>true</c> in
            /// the constructor of the <Typ>ViolationHandler</Typ>.</returns>
            /// <exception cref="InvalidPackageException">If <c>fixLmsViolations</c> was set to <c>false</c> in
            /// the constructor of the <Typ>ViolationHandler</Typ>.</exception>
            internal string HandleDefaultAttributeValue(string defaultValue, string nodeName, string attributeName)
            {
                if (m_fixViolations && m_behavior != ValidationBehavior.Enforce)
                {
                    Log(String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrDefaultAttributeUsed,
                        defaultValue, nodeName, attributeName));
                    return defaultValue;
                }
                else
                {
                    throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                        ValidatorResources.LrAttributeValueException, nodeName, attributeName));
                }
            }

            /// <summary>
            /// Handles truncating node values according to LR rules and validation behavior.  (E.g. m_fixViolations
            /// and m_behavior.)
            /// </summary>
            /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
            /// greater.  0 for any length.</param>
            /// <param name="value">String to check for length.  May not be null.</param>
            /// <param name="nodeName"></param>
            /// <param name="attributeName">If null, this is a node value.</param>
            /// <returns></returns>
            internal string HandleTruncation(int maxLength, string value, string nodeName, string attributeName)
            {
                if (maxLength > 0 && value.Length > maxLength)
                {
                    if (m_fixViolations)
                    {
                        if (m_behavior == ValidationBehavior.Enforce)
                        {
                            if (attributeName == null)
                            {
                                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                                    ValidatorResources.LrNodeLengthException, nodeName, value.Substring(0, 10)));
                            }
                            else
                            {
                                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                                    ValidatorResources.LrAttributeLengthException, nodeName, value.Substring(0, 10), attributeName));
                            }
                        }
                        else
                        {
                            string newValue = value.Substring(0, maxLength);
                            if (attributeName == null)
                            {
                                Log(String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrNodeLengthViolation, nodeName, newValue));
                            }
                            else
                            {
                                Log(String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrAttributeLengthViolation,
                                    nodeName, attributeName, newValue));
                            }
                            return newValue;
                        }
                    }
                    else
                    {
                        if (attributeName == null)
                        {
                            Log(String.Format(CultureInfo.CurrentCulture,
                                    ValidatorResources.LrNodeLengthException, nodeName, value.Substring(0, 10)));
                        }
                        else
                        {
                            Log(String.Format(CultureInfo.CurrentCulture,
                                    ValidatorResources.LrAttributeLengthException, nodeName, value.Substring(0, 10), attributeName));
                        }
                    }
                }
                return value;
            }

            /// <summary>
            /// Either throws an <Typ>InvalidPackageException</Typ>, issues a warning or error to the log, or does
            /// nothing, depending on the chosen <Typ>ValidationBehavior</Typ>.
            /// </summary>
            /// <param name="text">Message to put into the exception or log entry.</param>
            internal void GenericViolation(string text)
            {
                switch(m_behavior)
                {
                    case ValidationBehavior.Enforce:
                        throw new InvalidPackageException(text);
                    case ValidationBehavior.LogError:
                    case ValidationBehavior.LogWarning:
                        Log(text);
                        break;
                    default:
                    case ValidationBehavior.None:
                        // do nothing
                        break;
                }
            }

            /// <summary>
            /// Constructs a "child in the correct namespace not found" message.
            /// </summary>
            /// <param name="nodeName">Child node name.</param>
            /// <param name="parentNode">Parent node.</param>
            internal void ChildNodeInNamespaceNotFound(string nodeName, XPathNavigator parentNode)
            {
                string message;
                string parent = String.Format(CultureInfo.CurrentCulture, ValidatorResources.ParentXml, Helper.GetNodeText(parentNode));

                if (m_behavior == ValidationBehavior.Enforce)
                {
                    message = String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrChildInLrmNamespaceNotFound, nodeName, parent);
                }
                else
                {
                    message = String.Format(CultureInfo.CurrentCulture, ValidatorResources.LrChildInLrmNamespaceNotFoundTryOther, nodeName, parent);
                }
                GenericViolation(message);
            }

            /// <summary>
            /// Constructs a "required node missing" message and then calls <Mth>GenericViolation</Mth>.
            /// </summary>
            /// <param name="nodeName">Missing node name.</param>
            /// <param name="parentNode">Parent that should contain the missing node.</param>
            /// <exception cref="LearningComponentsInternalException">If <paramref name="parentNode"/> is null.</exception>
            internal void RequiredNodeMissing(string nodeName, XPathNavigator parentNode)
            {
                string message;
                string parent = String.Format(CultureInfo.CurrentCulture, ValidatorResources.ParentXml, Helper.GetNodeText(parentNode));

                if(m_behavior == ValidationBehavior.Enforce)
                {
                    message = String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissing, nodeName, parent);
                }
                else
                {
                    message = String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissingDefault, nodeName, parent);
                }
                GenericViolation(message);
            }

            /// <summary>
            /// Constructs an "invalid duplicate nodes" message and then calls <Mth>GenericViolation</Mth>.
            /// </summary>
            /// <param name="nodeName">Duplicated node name.</param>
            /// <param name="parentNode">Parent that contains the duplicate nodes.</param>
            /// <exception cref="LearningComponentsInternalException">If <paramref name="parentNode"/> is null.</exception>
            internal void InvalidDuplicateNodes(string nodeName, XPathNavigator parentNode)
            {
                GenericViolation(String.Format(CultureInfo.CurrentCulture, ValidatorResources.InvalidDuplicateNodes, nodeName,
                    String.Format(CultureInfo.CurrentCulture, ValidatorResources.ParentXml, Helper.GetNodeText(parentNode))));
            }

        }

        /// <summary>
        /// Parses index.xml files
        /// </summary>
        internal class IndexParser
        {
            private ViolationHandler m_violationHandler;
            private XPathNavigator m_learningResourceNode;
            private XPathNodeIterator m_packageDescriptionNodes;
            private XPathNodeIterator m_generalNodes;
            private XPathNodeIterator m_learningResourceDataNodes;
            private XPathNodeIterator m_organizationNodes;
            private PageIterator m_pageNodes;
            private XPathNodeIterator m_tableOfContentsNodes;
            private string m_language;
            private string m_title;
            private string m_description;
            private string m_version;
            private string m_instructions;
            private string m_pointsPossible; // note that once this is filled with data, it is either String.Empty or a valid float value.

            /// <summary>
            /// Represents the PackageDescription\General\LearningResource\Page node.
            /// </summary>
            internal class Page
            {
                private XPathNavigator m_nav;
                private IndexParser m_parser;
                private string m_pageId;
                private string m_title;
                private string m_imsId;
                private string m_href;
                private string m_subdirectory;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="nav">Points to Page node.</param>
                /// <param name="parser"></param>
                public Page(XPathNavigator nav, IndexParser parser)
                {
                    if (nav.LocalName != Strings.Page)
                        throw new LearningComponentsInternalException("MC005");
                    if (parser == null) throw new LearningComponentsInternalException("MC006");
                    m_nav = nav.Clone();
                    m_parser = parser;
                }

                /// <summary>
                /// Check if the Page node contains the requested attribute.
                /// </summary>
                /// <param name="localName"></param>
                /// <returns></returns>
                private bool ContainsAttribute(string localName)
                {
                    return !String.IsNullOrEmpty(m_nav.GetAttribute(localName, String.Empty));
                }

                /// <summary>
                /// Cache the string value of the requested attribute, with a callback delegate for further checking.
                /// </summary>
                /// <param name="attributeName"></param>
                /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
                /// greater.  0 for any length.</param>
                /// <param name="cache"></param>
                /// <param name="validateStringDelegate">Callback method to do extra checking.  May not be null.</param>
                /// <returns></returns>
                private string CacheStringAttribute(string attributeName, int maxLength, ref string cache, 
                    ValidateStringDelegate validateStringDelegate)
                {
                    return m_parser.CacheStringAttribute(Strings.Page, attributeName, String.Empty, m_nav, maxLength,
                        String.Empty, ref cache, validateStringDelegate);
                }

                /// <summary>
                /// Cache the string value of the requested attribute.
                /// </summary>
                /// <param name="attributeName"></param>
                /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
                /// greater.  0 for any length.</param>
                /// <param name="cache"></param>
                /// <returns></returns>
                private string CacheStringAttribute(string attributeName, int maxLength, ref string cache)
                {
                    return m_parser.CacheStringAttribute(Strings.Page, attributeName, String.Empty, m_nav, maxLength,
                        String.Empty, ref cache);
                }

                /// <summary>
                /// Returns the PageID attribute value of the Page node.
                /// </summary>
                /// <remarks>
                /// May be an integer from 1 to 1999999998.  Default value is String.Empty, in which case the caller
                /// should ignore the entire Page node.
                /// </remarks>
                public string PageID
                {
                    get
                    {
                        return CacheStringAttribute(Strings.PageID, 0, ref m_pageId,
                            delegate(string value, string defaultValue)
                            {
                                // Ignores the defaultValue parameter in favor of forcing String.Empty as the default.
                                try
                                {
                                    int i = Convert.ToInt32(value);
                                    if (i < 1 || i > 1999999998) value = String.Empty;
                                }
                                catch (FormatException)
                                {
                                    value = String.Empty;
                                }
                                catch (OverflowException)
                                {
                                    value = String.Empty;
                                }
                                if (String.IsNullOrEmpty(value))
                                {
                                    m_parser.m_violationHandler.GenericViolation(String.Format(CultureInfo.InvariantCulture,
                                        ValidatorResources.LrPageNodeSkipped, Helper.GetNodeText(m_nav)));
                                }
                                return value;
                            });
                    }
                }

                /// <summary>
                /// Returns an identifier to be used for a resource node associated with this page.
                /// </summary>
                public string ResourceId
                {
                    get
                    {
                        return Strings.ResourceIdPrefix + PageID;
                    }
                }

                /// <summary>
                /// Returns an identifier to be used for an item node associated with this page.
                /// </summary>
                public string ItemId
                {
                    get
                    {
                        return Strings.ItemIdPrefix + PageID;
                    }
                }

                /// <summary>
                /// Returns the Title attribute value of the Page node.
                /// </summary>
                /// <remarks>
                /// Default value is <c>String.Empty</c>.  Max length is 200.
                /// </remarks>
                public string Title
                {
                    get
                    {
                        return CacheStringAttribute(Strings.Title, 200, ref m_title);
                    }
                }

                /// <summary>
                /// Returns the href attribute value of the Page node, or "Page.htm" if there is none.
                /// </summary>
                /// <remarks>All backward slashes are converted to forward slashes.  Because remote
                /// content is not supported, throws <Typ>InvalidPackageException</Typ> if the href
                /// is absolute.</remarks>
                public string Href
                {
                    get
                    {
                        if (m_href == null)
                        {
                            if (ContainsAttribute(Strings.href))
                            {
                                CacheStringAttribute(Strings.href, 0, ref m_href);
                                // if m_href is absolute, throw InvalidPackageException.
                                if (Uri.IsWellFormedUriString(m_href, UriKind.Absolute))
                                {
                                    throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                                        ValidatorResources.InvalidNode, Helper.GetNodeText(m_nav),
                                        ValidatorResources.LrRemoteContentNotSupported));
                                }
                                m_href = m_href.Replace('\\', '/');
                            }
                            else
                            {
                                m_href = "Page.htm";
                            }
                        }
                        return m_href;
                    }
                }

                /// <summary>
                /// Returns the ImsId attribute value of the Page node.
                /// </summary>
                /// <remarks>This is <c>String.Empty</c> for non-IMS pages.</remarks>
                public string ImsId
                {
                    get
                    {
                        if (m_imsId == null)
                        {
                            if (ContainsAttribute(Strings.ImsId))
                            {
                                CacheStringAttribute(Strings.ImsId, 0, ref m_imsId);
                            }
                            else m_imsId = String.Empty;
                        }
                        return m_imsId;
                    }
                }

                /// <summary>
                /// Returns the name of the directory containing the files for this page.  Note that there is no
                /// trailing slash.
                /// </summary>
                /// <remarks>
                /// For Class Server pages, this returns "P" + PageID.  For IMS pages, this returns the path component
                /// of the href attribute.
                /// </remarks>
                public string Subdirectory
                {
                    get
                    {
                        if (m_subdirectory == null)
                        {
                            if (IsSco)
                            {
                                if (Href.Contains("/"))
                                {
                                    m_subdirectory = Href.Substring(0, Href.LastIndexOf('/'));
                                }
                                else m_subdirectory = String.Empty;
                            }
                            else m_subdirectory = "P" + PageID;
                        }
                        return m_subdirectory;
                    }
                }

                /// <summary>
                /// Returns a key to use as an identifier in a list of resource nodes.
                /// </summary>
                /// <remarks>For Class Server pages returns Subdirectory.  For IMS pages returns the PageID.</remarks>
                public string Key
                {
                    get
                    {
                        if (IsSco) return PageID;
                        else return Subdirectory;
                    }
                }

                /// <summary>
                /// Returns a value for the xml:base of a resource node for this page.
                /// </summary>
                /// <remarks>
                /// If this is a SCO, this always returns String.Empty for xml:base.
                /// </remarks>
                public string XmlBase
                {
                    get
                    {
                        if (IsSco || String.IsNullOrEmpty(Subdirectory)) return String.Empty;
                        else return Subdirectory + "/";
                    }
                }

                /// <summary>
                /// Returns true if this is a sco or false if not.  It is a sco if there is an ImsID attribute value.
                /// </summary>
                public bool IsSco
                {
                    get
                    {
                        if (String.IsNullOrEmpty(ImsId)) return false;
                        else return true;
                    }
                }
            }

            /// <summary>
            /// Holds the list of Page nodes.
            /// </summary>
            internal class PageIterator : XPathNodeIterator
            {
                private XPathNodeIterator m_itr;
                private IndexParser m_parser;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="itr">Holds the list of Page nodes.</param>
                /// <param name="parser"></param>
                public PageIterator(XPathNodeIterator itr, IndexParser parser)
                {
                    if (itr != null)
                    {
                        m_itr = itr.Clone();
                        m_parser = parser;
                    }
                }

                /// <summary>
                /// Clone the PageIterator.
                /// </summary>
                /// <returns></returns>
                public override XPathNodeIterator Clone()
                {
                    return new PageIterator(m_itr, m_parser);
                }

                /// <summary>
                /// Returns a count of the Pages.
                /// </summary>
                public override int Count
                {
                    get
                    {
                        if (m_itr == null) return 0;
                        return m_itr.Count;
                    }
                }

                /// <summary>
                /// Returns the current Page navigator.
                /// </summary>
                public override XPathNavigator Current
                {
                    get
                    {
                        if (m_itr == null) return null;
                        return m_itr.Current;
                    }
                }

                /// <summary>
                /// Returns the current Page.
                /// </summary>
                public Page CurrentPage
                {
                    get
                    {
                        if (m_itr == null) return null;
                        return new Page(m_itr.Current, m_parser);
                    }
                }

                /// <summary>
                /// Returns the index of the current Page.
                /// </summary>
                public override int CurrentPosition
                {
                    get
                    {
                        if (m_itr == null) return 0;
                        return m_itr.CurrentPosition;
                    }
                }

                /// <summary>
                /// Moves to the next page.
                /// </summary>
                /// <returns></returns>
                public override bool MoveNext()
                {
                    if (m_itr == null) return false;
                    return m_itr.MoveNext();
                }
            }

            /// <summary>
            /// Ensures that the root node is LearningResource, and sets the m_LearningResource variable to it.
            /// </summary>
            internal IndexParser(XPathNavigator root, ViolationHandler violationHandler)
            {
                if (root == null) throw new LearningComponentsInternalException("MC003");
                if (violationHandler == null) throw new LearningComponentsInternalException("MC004");

                m_violationHandler = violationHandler;
                // move to the root node and validate it is a LearningResource node.  If it is in the incorrect namespace,
                // accept it if "fixLrmViolations" is true.
                XPathNavigator lrNav = root.Clone();
                if (!lrNav.MoveToFollowing(XPathNodeType.Element)
                    || (String.CompareOrdinal(lrNav.Name, Strings.LearningResource) == 0
                    && String.CompareOrdinal(lrNav.NamespaceURI, Strings.XmlnsLearningResource) == 0))
                {
                    if (String.CompareOrdinal(lrNav.NamespaceURI, Strings.XmlnsLearningResource) != 0)
                    {
                        m_violationHandler.GenericViolation(String.Format(CultureInfo.CurrentCulture,
                            ValidatorResources.RequiredElementMissing, Strings.LearningResource, ValidatorResources.LrMustHaveCorrectRoot));
                        // if it didn't throw above, "Enforce" is not the current validation behavior.  However,
                        // if "fixLrmViolation" is false, we must still throw.
                        if (!m_violationHandler.FixViolations)
                        {
                            throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                                ValidatorResources.RequiredElementMissing, Strings.LearningResource, ValidatorResources.LrMustHaveCorrectRoot));
                        }
                    }
                    m_learningResourceNode = lrNav;
                }
                else
                {
                    // If the root is not <LearningResource> it is an unrecoverable error.
                    throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture,
                        ValidatorResources.RequiredElementMissing, Strings.LearningResource, ValidatorResources.LrMustHaveCorrectRoot));
                }
            }

            /// <summary>
            /// Get the full xml of the index.xml.
            /// </summary>
            internal string FullXml
            {
                get
                {
                    return m_learningResourceNode.OuterXml;
                }
            }

            /// <summary>
            /// Find the requested child node.  Handle error conditions when there are duplicate child nodes of the same name.
            /// </summary>
            /// <param name="childName"></param>
            /// <param name="nav"></param>
            /// <param name="itr"></param>
            /// <returns></returns>
            private XPathNavigator CacheRequiredChildNoDuplicates(string childName, XPathNavigator nav, ref XPathNodeIterator itr)
            {
                return CacheChildNoDuplicates(childName, nav, ref itr, true);
            }

            /// <summary>
            /// Helper method that calls the <Typ>XPathNavigator</Typ>'s SelectChildren method using the XmlnsLearningResource
            /// namespace.  If no children are found, and fixLrmViolations is "true" (and ValidationBehavior is not "Enforce"),
            /// it logs this fact and finds any children with the given name, but in any namespace.
            /// </summary>
            private XPathNodeIterator SelectChildren(XPathNavigator nav, string childName)
            {
                XPathNodeIterator itr;
                itr = nav.SelectChildren(childName, Strings.XmlnsLearningResource);
                if (itr.Count == 0 && m_violationHandler.FixViolations)
                {
                    // get a list of all children
                    XPathNodeIterator allChildren = nav.SelectChildren(XPathNodeType.Element);
                    while (allChildren.MoveNext())
                    {
                        if (String.CompareOrdinal(allChildren.Current.Name, childName) == 0)
                        {
                            // if a child name match is found, report that the original namespace wasn't
                            // found, and then set itr to the children of that name in the new namespace.
                            // the following will throw if validation behavior is "Enforce"
                            m_violationHandler.ChildNodeInNamespaceNotFound(childName, nav);
                            itr = nav.SelectChildren(childName, allChildren.Current.NamespaceURI);
                            break;
                        }
                    }
                }
                return itr;
            }

            /// <summary>
            /// Find the requested child node.  Handle error conditions when there are duplicate child nodes of the same name.
            /// </summary>
            /// <param name="childName"></param>
            /// <param name="nav"></param>
            /// <param name="itr"></param>
            /// <param name="required"></param>
            /// <returns></returns>
            private XPathNavigator CacheChildNoDuplicates(string childName, XPathNavigator nav, ref XPathNodeIterator itr, bool required)
            {
                if (itr == null)
                {
                    if (nav == null)
                    {
                        return null;
                    }
                    else
                    {
                        itr = SelectChildren(nav, childName);
                        if (itr.MoveNext())
                        {
                            if (itr.Count > 1)
                            {
                                m_violationHandler.InvalidDuplicateNodes(childName, nav);
                            }
                        }
                        else
                        {
                            if (required)
                            {
                                m_violationHandler.RequiredNodeMissing(childName, nav);
                            }
                            return null;
                        }
                    }
                }
                return itr.Current;
            }

            /// <summary>
            /// Returns the requested node's value, handling error condition if there are duplicate nodes of the same name.
            /// </summary>
            /// <param name="childName"></param>
            /// <param name="nav"></param>
            /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
            /// greater.  0 for any length.</param>
            /// <param name="defaultValue"></param>
            /// <param name="cache"></param>
            /// <returns></returns>
            private string CacheRequiredStringNoDuplicates(string childName, XPathNavigator nav, int maxLength, string defaultValue, ref string cache)
            {
                if (cache == null)
                {
                    if (nav == null)
                    {
                        cache = m_violationHandler.HandleDefaultNodeValue(defaultValue, childName);
                    }
                    else
                    {
                        XPathNodeIterator nodes = null;
                        XPathNavigator child = CacheChildNoDuplicates(childName, nav, ref nodes, true);
                        if (child == null)
                        {
                            cache = m_violationHandler.HandleDefaultNodeValue(defaultValue, childName);
                        }
                        else
                        {
                            cache = child.Value;
                        }
                        cache = m_violationHandler.HandleTruncation(maxLength, cache, childName, null);
                    }
                }
                return cache;
            }

            /// <summary>
            /// Returns the requested node's value, handling error condition if there are duplicate nodes of the same name.
            /// </summary>
            /// <param name="childName"></param>
            /// <param name="nav"></param>
            /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
            /// greater.  0 for any length.</param>
            /// <param name="defaultValue"></param>
            /// <param name="cache"></param>
            /// <returns></returns>
            private string CacheOptionalStringNoDuplicates(string childName, XPathNavigator nav, int maxLength, string defaultValue, ref string cache)
            {
                if (cache == null)
                {
                    if (nav == null)
                    {
                        cache = m_violationHandler.HandleDefaultNodeValue(defaultValue, childName);
                    }
                    else
                    {
                        XPathNodeIterator nodes = null;
                        XPathNavigator child = CacheChildNoDuplicates(childName, nav, ref nodes, false);
                        if (child == null)
                        {
                            cache = defaultValue;
                        }
                        else
                        {
                            cache = child.Value;
                        }
                        cache = m_violationHandler.HandleTruncation(maxLength, cache, childName, null);
                    }
                }
                return cache;
            }

            /// <summary>
            /// Returns the requested attribute value.
            /// </summary>
            /// <param name="nodeName"></param>
            /// <param name="attributeName"></param>
            /// <param name="attributeNamespace"></param>
            /// <param name="nav"></param>
            /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
            /// greater.  0 for any length.</param>
            /// <param name="defaultValue"></param>
            /// <param name="cache"></param>
            /// <returns></returns>
            private string CacheStringAttribute(string nodeName, string attributeName, string attributeNamespace,
                XPathNavigator nav, int maxLength, string defaultValue, ref string cache)
            {
                if (cache == null)
                {
                    if (nav == null)
                    {
                        cache = m_violationHandler.HandleDefaultAttributeValue(defaultValue, nodeName, attributeName);
                    }
                    else
                    {
                        cache = nav.GetAttribute(attributeName, attributeNamespace);
                        if (String.IsNullOrEmpty(cache))
                        {
                            cache = m_violationHandler.HandleDefaultAttributeValue(defaultValue, nodeName, attributeName);
                        }
                        else
                        {
                            cache = m_violationHandler.HandleTruncation(maxLength, cache, nodeName, attributeName);
                        }
                    }
                }
                return cache;
            }

            /// <summary>
            /// Returns the requested optional attribute value.
            /// </summary>
            /// <param name="nodeName"></param>
            /// <param name="attributeName"></param>
            /// <param name="attributeNamespace"></param>
            /// <param name="nav"></param>
            /// <param name="maxLength">Max string length to return.  Errors are logged if the string length is
            /// greater.  0 for any length.</param>
            /// <param name="defaultValue"></param>
            /// <param name="cache"></param>
            /// <returns></returns>
            private string CacheOptionalStringAttribute(string nodeName, string attributeName, string attributeNamespace,
                XPathNavigator nav, int maxLength, string defaultValue, ref string cache)
            {
                if (cache == null)
                {
                    // if nav is null, that's okay since this is an optional value.  Just return the default.
                    if (nav == null)
                    {
                        cache = defaultValue;
                    }
                    else
                    {
                        cache = nav.GetAttribute(attributeName, attributeNamespace);
                        if (String.IsNullOrEmpty(cache))
                        {
                            // if cache is empty, that's okay since this is an optional value.  Just return the default.
                            cache = defaultValue;
                        }
                        else
                        {
                            cache = m_violationHandler.HandleTruncation(maxLength, cache, nodeName, attributeName);
                        }
                    }
                }
                return cache;
            }

            private delegate string ValidateStringDelegate(string value, string defaultValue);
            /// <summary>
            /// Returns the requested attribute value.
            /// </summary>
            private string CacheStringAttribute(string nodeName, string attributeName, string attributeNamespace,
                XPathNavigator nav, int maxLength, string defaultValue, ref string cache, ValidateStringDelegate validateStringDelegate)
            {
                CacheStringAttribute(nodeName, attributeName, attributeNamespace, nav, maxLength, defaultValue, ref cache);
                return validateStringDelegate(cache, defaultValue);
            }

            /// <summary>
            /// Returns the requested attribute value.
            /// </summary>
            private string CacheOptionalStringAttribute(string nodeName, string attributeName, string attributeNamespace,
                XPathNavigator nav, int maxLength, string defaultValue, ref string cache, ValidateStringDelegate validateStringDelegate)
            {
                CacheOptionalStringAttribute(nodeName, attributeName, attributeNamespace, nav, maxLength, defaultValue, ref cache);
                return validateStringDelegate(cache, defaultValue);
            }

            /// <summary>
            /// Points to the &lt;LearningResource&gt; node.
            /// </summary>
            private XPathNavigator LearningResource
            {
                get
                {
                    return m_learningResourceNode;
                }
            }

            /// <summary>
            /// Returns true if there are any &lt;License&gt; nodes.
            /// </summary>
            internal bool RequiresLicense
            {
                get
                {
                    if (LearningResourceData != null)
                    {
                        XPathNodeIterator licensesNodes = SelectChildren(LearningResourceData, Strings.Licenses);
                        foreach (XPathNavigator nav in licensesNodes)
                        {
                            XPathNodeIterator nodes = SelectChildren(nav, Strings.License);
                            if (nodes.Count > 0)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }

            /// <summary>
            /// Returns the LearningResourceData/PointsPossible.
            /// </summary>
            /// <remarks>Default value is <c>String.Empty</c>.</remarks>
            /// <exception cref="InvalidPackageException">The <c>&lt;PointsPossible&gt;</c> node contains a non-float number
            /// or is less than 0, or is greater than 10000, and
            /// <c>fixLrmViolations</c> was set to <c>false</c> in the <Typ>ManifestConverter</Typ> constructor.</exception>
            internal string PointsPossible
            {
                get
                {
                    return CacheOptionalStringAttribute(Strings.LearningResourceData, Strings.PointsPossible, String.Empty, LearningResourceData,
                        0, String.Empty, ref m_pointsPossible, delegate(string value, string defaultValue)
                        {
                            if (String.IsNullOrEmpty(value)) return value;
                            try
                            {
                                double d = XmlConvert.ToDouble(value);
                                if (d < 0 || d > 10000)
                                {
                                    value = defaultValue;
                                }
                            }
                            catch (FormatException)
                            {
                                value = defaultValue;
                            }
                            catch (OverflowException)
                            {
                                value = defaultValue;
                            }
                            // If pointsPossible is now String.Empty, it means there were errors above, so log that.
                            if (String.IsNullOrEmpty(value))
                            {
                                m_violationHandler.HandleDefaultNodeValue(defaultValue, Strings.PointsPossible);
                            }
                            return value;
                        });
                }
            }

            /// <summary>
            /// Points to the &lt;PackageDescription&gt; node.  Returns null if there is no such node in the index.xml.
            /// </summary>
            private XPathNavigator PackageDescription
            {
                get
                {
                    return CacheRequiredChildNoDuplicates(Strings.PackageDescription, m_learningResourceNode, ref m_packageDescriptionNodes);
                }
            }

            /// <summary>
            /// Points to the &lt;General&gt; node.  Returns null if there is no such node in the index.xml.
            /// </summary>
            private XPathNavigator General
            {
                get
                {
                    return CacheRequiredChildNoDuplicates(Strings.General, PackageDescription, ref m_generalNodes);
                }
            }

            /// <summary>
            /// Points to the &lt;LearningResourceData&gt; node.  Returns null if there is no such node in the index.xml.
            /// </summary>
            private XPathNavigator LearningResourceData
            {
                get
                {
                    return CacheRequiredChildNoDuplicates(Strings.LearningResourceData, General, ref m_learningResourceDataNodes);
                }
            }

            /// <summary>
            /// Points to the &lt;Organization&gt; node.  Returns null if there is no such node in the index.xml.
            /// </summary>
            private XPathNavigator Organization
            {
                get
                {
                    return CacheRequiredChildNoDuplicates(Strings.Organization, LearningResource, ref m_organizationNodes);
                }
            }

            /// <summary>
            /// Points to the &lt;TableOfContents&gt; node.  Returns null if there is no such node in the index.xml.
            /// </summary>
            private XPathNavigator TableOfContents
            {
                get
                {
                    return CacheRequiredChildNoDuplicates(Strings.TableOfContents, Organization, ref m_tableOfContentsNodes);
                }
            }

            /// <summary>
            /// Returns the PackageDescription\General\Language.
            /// </summary>
            /// <remarks>Default value is <c>String.Empty</c>.  Max length is 5.</remarks>
            /// <exception cref="InvalidPackageException">The <c>&lt;Language&gt;</c> node is missing and 
            /// <c>fixLrmViolations</c> was set to <c>false</c> in the <Typ>ManifestConverter</Typ> constructor.</exception>
            internal string Language
            {
                get
                {
                    return CacheRequiredStringNoDuplicates(Strings.Language, General, 5, String.Empty, ref m_language);
                }
            }

            /// <summary>
            /// Returns the PackageDescription\General\Title.
            /// </summary>
            /// <remarks>Default value is <c>String.Empty</c>.  Max length is 255.</remarks>
            /// <exception cref="InvalidPackageException">The <c>&lt;Title&gt;</c> node is missing and 
            /// <c>fixLrmViolations</c> was set to <c>false</c> in the <Typ>ManifestConverter</Typ> constructor.</exception>
            internal string Title
            {
                get
                {
                    return CacheRequiredStringNoDuplicates(Strings.Title, General, 255, String.Empty, ref m_title);
                }
            }

            /// <summary>
            /// Returns PackageDescription\General\Description
            /// </summary>
            /// <remarks>Default value is <c>String.Empty</c>.  Max length is 1024.</remarks>
            internal string Description
            {
                get
                {
                    return CacheRequiredStringNoDuplicates(Strings.Description, General, 1024, String.Empty, ref m_description);
                }
            }

            /// <summary>
            /// Returns PackageDescription\General\LearningResourceData\Instructions
            /// </summary>
            /// <remarks>Default value is <c>String.Empty</c>.  Max length is 4096.</remarks>
            internal string Instructions
            {
                get
                {
                    return CacheOptionalStringNoDuplicates(Strings.Instructions, LearningResourceData, 4096, String.Empty, ref m_instructions);
                }
            }

            /// <summary>
            /// Returns LearningResource.Version attribute.
            /// </summary>
            /// <remarks>Default value is "3.0".  The only legal values are "1.0", "2.0", and "3.0".</remarks>
            internal string Version
            {
                get
                {
                    return CacheStringAttribute(Strings.LearningResource, Strings.Version, String.Empty, LearningResource,
                        0, "3.0", ref m_version, delegate(string value, string defaultValue)
                        {
                            switch(value)
                            {
                                case "1.0":
                                case "2.0":
                                case "3.0":
                                    // do nothing, the above are valid
                                    break;
                                default:
                                    // change to the default
                                    value = m_violationHandler.HandleDefaultAttributeValue(defaultValue, Strings.LearningResource, Strings.Version);
                                    break;
                            }
                            return value;
                        });
                }
            }

            /// <summary>
            /// Returns the LearningResource\Organization\TableOfContents\Page nodes.  This property returns a <Typ>PageIterator</Typ>
            /// that has not yet had <Mth>MoveNext</Mth> called on it.  The caller of this property should hold a local copy and
            /// call <Mth>MoveNext</Mth> on it rather than calling the property multiple times.
            /// </summary>
            /// <remarks>
            /// No validation is done on the Page nodes.  The caller should validate the attributes are correct.
            /// </remarks>
            internal PageIterator Pages
            {
                get
                {
                    if (m_pageNodes == null)
                    {
                        if (TableOfContents == null)
                        {
                            m_pageNodes = new PageIterator(null, null);
                        }
                        else
                        {
                            m_pageNodes = new PageIterator(SelectChildren(TableOfContents, Strings.Page), this);
                        }
                    }
                    return (PageIterator)m_pageNodes.Clone();
                }
            }
        }

        /// <summary>
        /// Provides a template to begin an empty SCORM 1.2 manifest.
        /// {0} = manifest identifier - should be a GUID with no punctuation.
        /// {1} is the entire indexXml, not including the xml header.
        /// </summary>
        private const string EmptyManifestTemplate =
            "<?xml version = \"1.0\" standalone = \"no\"?>" +
            "<manifest identifier = \"M{0}\" version = \"1.2\" " +
                      "xmlns = \"" + Helper.Strings.ImscpNamespaceV1p2 + "\" " +
                      "xmlns:imscp = \"" + Helper.Strings.ImscpNamespaceV1p2 + "\" " +
                      "xmlns:adlcp = \"" + Helper.Strings.AdlcpNamespaceV1p2 + "\" " +
                      "xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\" " +
                      "xmlns:mlc = \"" + Helper.Strings.MlcNamespace + "\" " +
                      "xsi:schemaLocation = \"" + Helper.Strings.ImscpNamespaceV1p2 + ".xsd " +
                                             Helper.Strings.AdlcpNamespaceV1p2 + ".xsd\" >" +
                "<metadata>" +
                    "<schema>ADL SCORM</schema>" +
                    "<schemaversion>1.2</schemaversion>" +
                "</metadata>" +
                "<organizations default=\"LRM\">" +
                    "<organization identifier=\"LRM\">" +
                    "</organization>" +
                "</organizations>" +
                "{1}" + // full index.xml
            "</manifest>";
        private IndexParser m_indexParser;
        private ViolationHandler m_violationHandler;
        private List<string> m_filePaths;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filePaths"></param>
        /// <param name="log"></param>
        /// <param name="fixLrmViolations"></param>
        /// <param name="lrmValidationBehavior"></param>
        internal ManifestConverter(XPathNavigator root, IEnumerable<string> filePaths, ValidationResults log,
            bool fixLrmViolations, ValidationBehavior lrmValidationBehavior)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            m_violationHandler = new ViolationHandler(lrmValidationBehavior, fixLrmViolations, log);
            m_indexParser = new IndexParser(root, m_violationHandler);
            m_filePaths = new List<string>();
            if (filePaths != null)
            {
                IEnumerator<string> files = filePaths.GetEnumerator();
                while (files.MoveNext())
                {
                    m_filePaths.Add(files.Current);
                }
            }
        }


        /// <summary>
        /// Helper class to generate the resource nodes.
        /// </summary>
        private class ResourcesNode
        {
            private XPathNavigator m_root; // resources node to be inserted at the end of processing
            private IndexParser.PageIterator m_pages;
            private List<string> m_filePaths;
            private ViolationHandler m_violationHandler;

            /// <summary>
            /// Given a list of file paths, organizes them into lists of file paths inside the top level subdirectory
            /// of each file path in the original list.
            /// </summary>
            /// <remarks>
            /// E.g. given "sub1\file1", "sub2\file2", "sub1\sub\file3", two lists would be generated: "sub1" containing "file1"
            /// and "sub\file3", and "sub2" containing "file2".
            /// </remarks>
            private class SubdirectoryList : Dictionary<string, List<string>>
            {
                /// <summary>
                /// Add a file to the list of subdirectories under the requested key, which is usually the name of the
                /// subdirectory.
                /// </summary>
                /// <param name="key">Uniquely identifies a subdirectory.</param>
                /// <param name="value">Name of the file.</param>
                internal void AddToList(string key, string value)
                {
                    if (ContainsKey(key))
                    {
                        this[key].Add(value);
                    }
                    else
                    {
                        List<string> list = new List<string>();
                        list.Add(value);
                        Add(key, list);
                    }
                }

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="filePaths">List of file paths, relative to the package root.</param>
                /// <param name="violationHandler">May not be null.</param>
                internal SubdirectoryList(IEnumerable<string> filePaths, ViolationHandler violationHandler)
                {
                    if (violationHandler == null) throw new LearningComponentsInternalException("MC020");
                    foreach (string filePath in filePaths)
                    {
                        int firstSeparator = filePath.IndexOf('\\');
                        if (firstSeparator == -1) firstSeparator = filePath.IndexOf('/');
                        if (firstSeparator > 0)
                        {
                            string subdir = filePath.Substring(0, firstSeparator);
                            string file = filePath.Substring(firstSeparator + 1, filePath.Length - firstSeparator - 1);
                            AddToList(subdir, file);
                        }
                        else
                        {
                            // file is in the root of the package
                            AddToList(String.Empty, filePath);
                        }
                    }
                }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="pages"></param>
            /// <param name="filePaths"></param>
            /// <param name="violationHandler"></param>
            internal ResourcesNode(IndexParser.PageIterator pages, List<string> filePaths, ViolationHandler violationHandler)
            {
                m_pages = pages;
                m_filePaths = filePaths;
                m_violationHandler = violationHandler;
            }

            /// <summary>
            /// Create a resource node.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="href"></param>
            /// <param name="xmlBase"></param>
            /// <param name="isSco"></param>
            /// <param name="isLrm"></param>
            /// <param name="isWebContent"></param>
            /// <returns></returns>
            private XPathNavigator CreateResourceNode(string id, string href, string xmlBase, bool isSco, bool isLrm, bool isWebContent)
            {
                // insert a new resource node as the first child of the resources node.
                m_root.PrependChildElement(String.Empty, Helper.Strings.Resource, Helper.Strings.ImscpNamespaceV1p2, String.Empty);
                XPathNavigator resource = m_root.Clone();
                resource.MoveToFollowing(Helper.Strings.Resource, Helper.Strings.ImscpNamespaceV1p2);
                // create the attributes on it
                resource.CreateAttribute(String.Empty, Helper.Strings.Identifier, String.Empty, id);
                if (!String.IsNullOrEmpty(href))
                {
                    resource.CreateAttribute(String.Empty, Helper.Strings.Href, String.Empty, href);
                }
                else
                {
                    // since this is a SCORM 1.2 manifest, all <resource> nodes require an href.  In this case, since
                    // this node represents a place-holder for files rather than a true <resource> with an entry point,
                    // just output a blank href.
                    resource.CreateAttribute(String.Empty, Helper.Strings.Href, String.Empty, "");
                }
                if (!String.IsNullOrEmpty(xmlBase))
                {
                    resource.CreateAttribute("xml", "base", resource.LookupNamespace("xml"), xmlBase);
                }
                if (isWebContent)
                {
                    resource.CreateAttribute(String.Empty, "type", String.Empty, "webcontent");
                }
                resource.CreateAttribute(Helper.Strings.Adlcp, "scormType", resource.LookupNamespace(Helper.Strings.Adlcp), isSco ? "sco" : "asset");
                if (isLrm)
                {
                    resource.CreateAttribute(Strings.MlcPrefix, "xrloType", resource.LookupNamespace(Strings.MlcPrefix), "lrm");
                }
                return resource;
            }

            /// <summary>
            /// Returns a navigator pointing to the resources node.
            /// </summary>
            /// <returns></returns>
            internal XPathNavigator CreateNavigator()
            {
                if (m_root == null)
                {
                    XmlDocument doc = new XmlDocument();
                    const string EmptyResources =
                        "<?xml version = \"1.0\" standalone = \"no\"?>" +
                        "<manifest xmlns = \"" + Helper.Strings.ImscpNamespaceV1p2 + "\" " +
                                  "xmlns:imscp = \"" + Helper.Strings.ImscpNamespaceV1p2 + "\" " +
                                  "xmlns:adlcp = \"" + Helper.Strings.AdlcpNamespaceV1p2 + "\" " +
                                  "xmlns:mlc = \"" + Helper.Strings.MlcNamespace + "\">" +
                                  "<resources/></manifest>";
                    doc.LoadXml(EmptyResources);
                    m_root = doc.CreateNavigator();
                    m_root.MoveToFollowing(Helper.Strings.Resources, Helper.Strings.ImscpNamespaceV1p2);

                    // create resource nodes: one per each page, one for the shared directory, and one for other files.
                    // Use a dictionary to keep track of them where the key is the capital letter subdirectory name.
                    Dictionary<string, XPathNavigator> resourceNodes = new Dictionary<string, XPathNavigator>();
                    IndexParser.PageIterator pages = (IndexParser.PageIterator)m_pages.Clone();
                    while(pages.MoveNext())
                    {
                        IndexParser.Page page = pages.CurrentPage;
                        // verify PageID.
                        if (String.IsNullOrEmpty(page.PageID))
                        {
                            // ignore pages with no or erroneous page id's.
                            continue;
                        }
                        string key = page.Key;
                        if(!resourceNodes.ContainsKey(key))
                        {
                            resourceNodes.Add(key, CreateResourceNode(page.ResourceId, page.Href, page.XmlBase, page.IsSco, !page.IsSco, true));
                            // create dependency element pointing to shared files
                            XPathNavigator nav = resourceNodes[key].Clone();
                            // For IMS pages, if the file pointed to by href is in the list of files, add it as a file node and
                            // remove it from the list.
                            // File nodes for Class Server pages are handled below.
                            if (page.IsSco)
                            {
                                // Make a dependency to the OTHER resource node.
                                XPathNavigator clone = nav.Clone();
                                clone.PrependChildElement(String.Empty, Helper.Strings.Dependency, Helper.Strings.ImscpNamespaceV1p2, String.Empty);
                                clone.MoveToFollowing(Helper.Strings.Dependency, Helper.Strings.ImscpNamespaceV1p2);
                                clone.CreateAttribute(String.Empty, Helper.Strings.IdentifierRef, String.Empty, Strings.Other);
                                foreach (string file in m_filePaths)
                                {
                                    if (String.Compare(file, page.Href, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        AddFileNode(nav, page.Href);
                                        m_filePaths.Remove(file);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Class Server page resource nodes need a dependency to the SHARED resource node.
                                nav.PrependChildElement(String.Empty, Helper.Strings.Dependency, Helper.Strings.ImscpNamespaceV1p2, String.Empty);
                                nav.MoveToFollowing(Helper.Strings.Dependency, Helper.Strings.ImscpNamespaceV1p2);
                                nav.CreateAttribute(String.Empty, Helper.Strings.IdentifierRef, String.Empty, Strings.Shared);
                            }
                        }
                    }
                    // just in case there was a page with the id "shared" check.
                    if (!resourceNodes.ContainsKey(Strings.Shared))
                    {
                        resourceNodes.Add(Strings.Shared, CreateResourceNode(Strings.Shared, null, Strings.Shared + "/", false, false, false));
                    }
                    // ditto for "other"
                    if (!resourceNodes.ContainsKey(Strings.Other))
                    {
                        resourceNodes.Add(Strings.Other, CreateResourceNode(Strings.Other, null, null, false, false, false));
                    }

                    SubdirectoryList subdirectories = new SubdirectoryList(m_filePaths, m_violationHandler);
                    // Add the file nodes.
                    foreach (string subdirectory in subdirectories.Keys)
                    {
                        string uppercase = subdirectory.ToUpper(CultureInfo.InvariantCulture);
                        if (resourceNodes.ContainsKey(uppercase))
                        {
                            foreach (string file in subdirectories[subdirectory])
                            {
                                XPathNavigator nav = resourceNodes[uppercase].Clone();
                                AddFileNode(nav, file);
                            }
                        }
                        else // other files
                        {
                            foreach (string file in subdirectories[subdirectory])
                            {
                                // skip imsmanifest.xml
                                if (String.Compare("imsmanifest.xml", file, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    continue;
                                }

                                XPathNavigator nav = resourceNodes[Strings.Other].Clone();
                                string filePath;
                                if (String.IsNullOrEmpty(subdirectory))
                                {
                                    filePath = file;
                                }
                                else
                                {
                                    filePath = subdirectory + "/" + file;
                                }
                                AddFileNode(nav, filePath);
                            }
                        }
                    }
                }
                XPathNavigator resources = m_root.Clone();
                resources.MoveToFollowing(Helper.Strings.Resources, Helper.Strings.ImscpNamespaceV1p2);
                return resources;
            }

            /// <summary>
            /// Add a file node to a resource.
            /// </summary>
            /// <param name="nav"></param>
            /// <param name="href"></param>
            private static void AddFileNode(XPathNavigator nav, string href)
            {
                XPathNavigator clone = nav.Clone();
                clone.PrependChildElement(String.Empty, Helper.Strings.File, Helper.Strings.ImscpNamespaceV1p2, String.Empty);
                clone.MoveToFollowing(Helper.Strings.File, Helper.Strings.ImscpNamespaceV1p2);
                clone.CreateAttribute(String.Empty, Helper.Strings.Href, String.Empty, href);
            }
        }

        /// <summary>
        /// Returns the starting template manifest.
        /// </summary>
        internal string EmptyManifest
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, EmptyManifestTemplate,
                    Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture), m_indexParser.FullXml);
            }
        }

        /// <summary>
        /// String constants.
        /// </summary>
        internal static class Strings
        {
            internal const string description = "description";
            internal const string Description = "Description";
            internal const string General = "General";
            internal const string href = "href";
            internal const string ImsId = "ImsId";
            internal const string instructions = "instructions";
            internal const string Instructions = "Instructions";
            internal const string ItemIdPrefix = "ITEM"; // used for generating an item identifier from a PageID
            internal const string Language = "Language";
            internal const string LearningResource = "LearningResource";
            internal const string LearningResourceData = "LearningResourceData";
            internal const string License = "License";
            internal const string Licenses = "Licenses";
            internal const string MlcPrefix = "mlc";
            internal const string Organization = "Organization";
            internal const string Other = "OTHER"; // used for the "other" resource id
            internal const string PackageDescription = "PackageDescription";
            internal const string Page = "Page";
            internal const string PageID = "PageID";
            internal const string PointsPossible = "PointsPossible";
            internal const string pointsPossible = "pointsPossible";
            internal const string ResourceIdPrefix = "RESOURCE"; // used for generating a resource identifier from a PageID
            internal const string Shared = "SHARED"; // used for the shared resource id
            internal const string TableOfContents = "TableOfContents";
            internal const string Title = "Title";
            internal const string Version = "Version";
            internal const string XmlnsLearningResource = "urn:schemas-microsoft-com:learning-resource";
            
        }

        /// <summary>
        /// Returns an <Typ>XPathNavigator</Typ> pointing to the root of an XML document of a SCORM compatible manifest.
        /// </summary>
        /// <returns></returns>
        internal XPathNavigator ConvertFromIndexXml()
        {
            // First make sure this isn't licensed content
            if (m_indexParser.RequiresLicense)
            {
                throw new InvalidPackageException(ValidatorResources.LrLicensedContentNotSupported);
            }
            // Create a new empty manifest.
            XmlDocument manifest = new XmlDocument();
            manifest.LoadXml(EmptyManifest);
            XPathNavigator rootNav = manifest.CreateNavigator();

            AddManifestMetadata(rootNav);

            XPathNavigator organizationNav = rootNav.Clone();
            if (organizationNav.MoveToFollowing(Helper.Strings.Organization, Helper.Strings.ImscpNamespaceV1p2))
            {
                // Create a <title> node under the <organization>
                organizationNav.PrependChildElement(String.Empty, Helper.Strings.Title, Helper.Strings.ImscpNamespaceV1p2,
                    m_indexParser.Title);
                
                // If there is a PointsPossible, Description or Instructions, enter the metadata under the <organization>
                if (!String.IsNullOrEmpty(m_indexParser.Description) || !String.IsNullOrEmpty(m_indexParser.Instructions)
                    || !String.IsNullOrEmpty(m_indexParser.PointsPossible))
                {
                    organizationNav.PrependChildElement(String.Empty, Helper.Strings.Metadata, Helper.Strings.ImscpNamespaceV1p2,
                        String.Empty);
                    XPathNavigator metadata = organizationNav.Clone();
                    if (metadata.MoveToFollowing(Helper.Strings.Metadata, Helper.Strings.ImscpNamespaceV1p2))
                    {
                        if (!String.IsNullOrEmpty(m_indexParser.Description))
                        {
                            metadata.PrependChildElement(Strings.MlcPrefix, Strings.description, metadata.LookupNamespace(Strings.MlcPrefix), 
                                m_indexParser.Description);
                        }
                        if (!String.IsNullOrEmpty(m_indexParser.Instructions))
                        {
                            metadata.PrependChildElement(Strings.MlcPrefix, Strings.instructions, metadata.LookupNamespace(Strings.MlcPrefix), 
                                m_indexParser.Instructions);
                        }
                        if (!String.IsNullOrEmpty(m_indexParser.PointsPossible))
                        {
                            metadata.PrependChildElement(Strings.MlcPrefix, Strings.pointsPossible, metadata.LookupNamespace(Strings.MlcPrefix),
                                m_indexParser.PointsPossible);
                        }
                    }
                }
                // Create pages
                IndexParser.PageIterator pages = m_indexParser.Pages;
                // keep track of page id's that have already been used, to log when there are duplicates
                Collection<string> pageIds = new Collection<string>();
                while (pages.MoveNext())
                {
                    IndexParser.Page page = pages.CurrentPage;
                    if (!String.IsNullOrEmpty(page.PageID))
                    {
                        // log duplicate page id's
                        if (pageIds.Contains(page.PageID))
                        {
                            m_violationHandler.GenericViolation(String.Format(CultureInfo.InvariantCulture,
                                ValidatorResources.LrPageIdDuplicate, page.PageID));
                        }
                        else
                        {
                            pageIds.Add(page.PageID);
                        }
                        using (XmlWriter writer = organizationNav.AppendChild())
                        {
                            writer.WriteStartElement(Helper.Strings.Item);
                            writer.WriteAttributeString(Helper.Strings.Identifier, page.ItemId);
                            writer.WriteAttributeString(Helper.Strings.IdentifierRef, page.ResourceId);
                            writer.WriteElementString(Helper.Strings.Imscp, Helper.Strings.Title, 
                                organizationNav.LookupNamespace(Helper.Strings.Imscp),
                                page.Title);
                            writer.WriteEndElement(); // </item>
                        }
                    }
                }
                // Insert resources node after <organizations/>
                XPathNavigator organizationsNav = rootNav.Clone();
                organizationsNav.MoveToFollowing(Helper.Strings.Organizations, Helper.Strings.ImscpNamespaceV1p2);
                ResourcesNode resourcesNode = new ResourcesNode(m_indexParser.Pages, m_filePaths, m_violationHandler);
                organizationsNav.InsertAfter(resourcesNode.CreateNavigator());
                // return an XPathNavigator pointing to the manifest node
                rootNav.MoveToFollowing(Helper.Strings.Manifest, Helper.Strings.ImscpNamespaceV1p2);
                return rootNav;
            }
            else
            {
                // This should never happen, and can only happen if a code change causes a bug.
                throw new LearningComponentsInternalException("MC001");
            }
        }

        /// <summary>
        /// Adds metadata information to the manifest.
        /// </summary>
        /// <param name="rootNav"></param>
        private void AddManifestMetadata(XPathNavigator rootNav)
        {
            XPathNavigator nav = rootNav.Clone();
            nav.MoveToFollowing(Helper.Strings.Metadata, Helper.Strings.ImscpNamespaceV1p2);
            XmlWriter writer;
            using (writer = nav.AppendChild())
            {
                writer.WriteStartElement(Helper.Strings.Lom, Helper.Strings.LomNamespaceV1p2);
                writer.WriteStartElement(Helper.Strings.General, Helper.Strings.LomNamespaceV1p2);

                // Create a manifest/metadata/lom/general/title/langstring xml:lang={0} node where
                // {0} is the language from PackageDescription/General/Language, or the CurrentCulture.Name
                // if it doesn't exist.
                writer.WriteStartElement(Helper.Strings.Title, Helper.Strings.LomNamespaceV1p2);
                writer.WriteStartElement(Helper.Strings.LangString, Helper.Strings.LomNamespaceV1p2);
                string language;
                try
                {
                    language = m_indexParser.Language;
                }
                catch (InvalidPackageException)
                {
                    language = CultureInfo.CurrentCulture.Name;
                }
                if(String.IsNullOrEmpty(language)) language = CultureInfo.CurrentCulture.Name;
                writer.WriteAttributeString("xml", "lang", String.Empty, language);
                writer.WriteValue(m_indexParser.Title);
                writer.WriteEndElement(); // </strings>
                writer.WriteEndElement(); // </title>
                // Create a manifest/metadata/lom/general/description/langstring xml:lang={0} node where
                // {0} is the language from PackageDescription/General/Language, or the CurrentCulture.Name
                // if it doesn't exist.
                writer.WriteStartElement(Helper.Strings.Description, Helper.Strings.LomNamespaceV1p2);
                writer.WriteStartElement(Helper.Strings.LangString, Helper.Strings.LomNamespaceV1p2);
                writer.WriteAttributeString("xml", "lang", String.Empty, language);
                writer.WriteValue(m_indexParser.Description);
                writer.WriteEndElement(); // </strings>
                writer.WriteEndElement(); // </description>
                writer.WriteEndElement(); // </general>
                writer.WriteEndElement(); // </lom>
            }
        }

        /// <summary>
        /// Converts from a stream containing an Index.xml LRM file to an XmlDocument containing a &lt;manifest&gt; node
        /// of an imsmanifest.xml file.
        /// </summary>
        /// <param name="indexXml">Learning Resource index xml, obtained from the index.xml file inside of a
        /// Learning Resource.</param>
        /// <param name="filePaths">List of file in the package.  In general, this list should only contain file paths
        /// that are under the root of the package.  However, no check is made for this case.</param>
        /// <remarks>The <paramref name="indexXml"/> must contain a valid Learning Resource index.  Invalid content
        /// will cause exceptions.</remarks>
        /// <returns>
        /// A <Typ>ConversionResult</Typ> containing the <Typ>XPathNavigator</Typ> pointing to the root of an XML document 
        /// of a SCORM compatible manifest and a <Typ>ValidationResults</Typ> log.
        /// </returns>
        public static ConversionResult ConvertFromIndexXml(Stream indexXml, IEnumerable<string> filePaths)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            return ConvertFromIndexXml(indexXml, filePaths, false, ValidationBehavior.Enforce);
        }

        /// <summary>
        /// Converts from index.xml to xpathnavigator, allowing the caller to indicate how the conversion takes place. Currently, this is used 
        /// for the package import process.
        /// </summary>
        /// <param name="indexXml">Learning Resource index xml, obtained from the index.xml file inside of a
        /// Learning Resource.</param>
        /// <param name="filePaths">List of file in the package.  In general, this list should only contain file paths
        /// that are under the root of the package.  However, no check is made for this case.</param>
        /// <param name="fixLrmViolations">true to use defaults when LRM violations occur in the index.xml.  Otherwise
        /// an <Typ>InvalidPackageException</Typ> will be thrown.</param>
        /// <param name="lrmValidationBehavior">Whether to log LRM violations as errors or warnings, not to log, or always
        /// throw <Typ>InvalidPackageException</Typ> regardless of the value of <paramref name="fixLrmViolations"/>.d</param>
        /// <returns>
        /// A <Typ>ConversionResult</Typ> containing the <Typ>XPathNavigator</Typ> pointing to the root of an XML document 
        /// of a SCORM compatible manifest and a <Typ>ValidationResults</Typ> log.
        /// </returns>
        internal static ConversionResult ConvertFromIndexXml(Stream indexXml, IEnumerable<string> filePaths, bool fixLrmViolations, ValidationBehavior lrmValidationBehavior)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;

            // indexXml must contain valid XML
            XPathDocument doc = new XPathDocument(indexXml);
            XPathNavigator nav = doc.CreateNavigator();
            ValidationResults log = new ValidationResults();
            // Convert indexXml to XPathNavigator by creating xml document.
            ManifestConverter converter = new ManifestConverter(nav, filePaths, log, fixLrmViolations, lrmValidationBehavior);
            return new ConversionResult(converter.ConvertFromIndexXml(), log);
        }
    }
    #endregion
}
