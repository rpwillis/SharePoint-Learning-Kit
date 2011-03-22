/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;
using System.Xml;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.LearningComponents.Manifest;
using System.Globalization;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents.DataModel
{
    /// <summary>
	/// Provides a means of returning a <c>Guid</c> from an object that has a GUID identifier.
	/// </summary>
	/// <remarks>
    /// Used by internal code that already has a guid associated with an attachment.  If this interface does
    /// not exist on the attachment, then a new guid will be generated.
    /// </remarks>
    internal interface IAttachment
    {
        /// <summary>
        /// Creates a new stream object that is able to read from the object.  The caller may only call
		/// <c>Read</c> and <c>Seek</c> on the returned <c>Stream</c>.
        /// </summary>
        /// <returns>A newly created Stream.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024", Justification="GetBytes() is not a property because it may involve an unknown amount of processing.")]
        byte[] GetBytes();

        /// <summary>
        /// Gets the GUID associated with this attachment.
        /// </summary>
        Guid Guid
        {
            get;
        }
    }

    internal static class DataModelUtilities
    {
        /// <summary>
        /// Sets an XML attribute based on an enum value.  If the attribute does not already exist, it is created.
        /// </summary>
        /// <typeparam name="T">The enum to use</typeparam>
        /// <param name="elementNav">An XPathNavigator that points to the XML element to which the attribute belongs.</param>
        /// <param name="attribute">The name of the XML attribute.</param>
        /// <param name="value">The value of the enum to set.</param>
        internal static void SetEnumAttribute<T>(XPathNavigator elementNav, string attribute, Nullable<T> value) where T : struct
        {
            XPathNavigator nav = elementNav.Clone();
            
            // if we are nulling out the value, delete the attribute.
            if(!value.HasValue)
            {
                if(nav.MoveToAttribute(attribute, String.Empty))
                {
                    nav.DeleteSelf();
                }
                return;
            }

            // if we can't move to the attribute, then it doesn't exist, so add it.
            if(!nav.MoveToAttribute(attribute, String.Empty))
            {
                nav.CreateAttribute(String.Empty, attribute, String.Empty, Enum.GetName(typeof(T), value.Value));
            }
            else
            {
                nav.SetValue(Enum.GetName(typeof(T), value.Value));
            }
        }

        /// <summary>
        /// Gets an XML attribute based on an enum value.  If the attribute does not exist, a default value is returned.
        /// </summary>
        /// <typeparam name="T">The enum to use</typeparam>
        /// <param name="elementNav">An XPathNavigator that points to the XML element to which the attribute belongs.</param>
        /// <param name="attribute">The name of the XML attribute.</param>
        /// <param name="defaultValue">The value to return in case the attribute does not exist.</param>
        /// <returns>A correctly typed enum value based on the XML data provided.</returns>
        internal static Nullable<T> GetEnumAttribute<T>(XPathNavigator elementNav, string attribute, Nullable<T> defaultValue) where T : struct
        {
            XPathNavigator nav = elementNav.Clone();
            if(!nav.MoveToAttribute(attribute, String.Empty))
            {
                return defaultValue;
            }
            return (T)Enum.Parse(typeof(T), nav.Value);
        }

        /// <summary>
        /// Gets an XML attribute of a specified type.  If that attribute does not exist, a default value is returned.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="elementNav">An XPathNavigator that points to the XML element to which the attribute belongs.</param>
        /// <param name="attribute">The name of the XML attribute.</param>
        /// <param name="defaultValue">The value to return in case the attribute does not exist.</param>
        /// <returns>A correctly typed value based on the XML data provided.</returns>
        internal static T GetAttribute<T>(XPathNavigator elementNav, string attribute, T defaultValue)
        {
            XPathNavigator nav = elementNav.Clone();
            if(!nav.MoveToAttribute(attribute, String.Empty))
            {
                return defaultValue;
            }
            return (T)nav.ValueAs(typeof(T), null);
        }

        /// <summary>
        /// Gets an XML attribute of a specified type.  If that attribute does not exist, a default value is returned.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="elementNav">An XPathNavigator that points to the XML element to which the attribute belongs.</param>
        /// <param name="attribute">The name of the XML attribute.</param>
        /// <param name="defaultValue">The value to return in case the attribute does not exist.</param>
        /// <returns>A correctly typed value based on the XML data provided.</returns>
        internal static Nullable<T> GetNullableAttribute<T>(XPathNavigator elementNav, string attribute, Nullable<T> defaultValue) where T: struct
        {
            XPathNavigator nav = elementNav.Clone();
            if(!nav.MoveToAttribute(attribute, String.Empty))
            {
                return defaultValue;
            }
            return (T)nav.ValueAs(typeof(T), null);
        }

        /// <summary>
        /// Sets an XML attribute based on a value.  If the attribute does not already exist, it is created.
        /// </summary>
        /// <param name="elementNav">An XPathNavigator that points to the XML element to which the attribute belongs.</param>
        /// <param name="attribute">The name of the XML attribute.</param>
        /// <param name="value">The value to set, already converted to a string.</param>
        internal static void SetAttribute(XPathNavigator elementNav, string attribute, string value)
        {
            XPathNavigator nav = elementNav.Clone();

            // if we are nulling out the value, delete the attribute.
            if(value == null)
            {
                if(nav.MoveToAttribute(attribute, String.Empty))
                {
                    nav.DeleteSelf();
                }
                return;
            }
            
            // if we can't move to the attribute, then it doesn't exist, so add it.
            if(!nav.MoveToAttribute(attribute, String.Empty))
            {
                nav.CreateAttribute(String.Empty, attribute, String.Empty, value);
            }
            else
            {
                nav.SetValue(value);
            }
        }

        /// <summary>
        /// Delegate to validate the identifier.
        /// </summary>
        /// <param name="identifier">Identifier to validate.</param>
        internal delegate void ValidateIdentifier(string identifier);

        /// <summary>
        /// Validates and sets an identifier, which must be unique within the list.
        /// </summary>
        /// <param name="elementNav">XPathNavigator that points to the element that represents the current object.</param>
        /// <param name="parentElementName">The name of the parent element of the list.</param>
        /// <param name="elementName">The name of the element that represents the object.</param>
        /// <param name="identifier">The new identifier to be set.</param>
        /// <param name="validationDelegate">Delegate to perform simple validation on the identifier.</param>
        internal static void SetIdentifier(XPathNavigator elementNav, string parentElementName, string elementName, string identifier, ValidateIdentifier validationDelegate)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            // unlike most properties, call validation first, mainly to catch assigning empty string or null which is never valid
            // even if the current identifier is null.
            if(validationDelegate != null)
            {
                validationDelegate(identifier);
            }
            if(identifier != elementNav.GetAttribute("id", String.Empty))
            {
                XPathNavigator nav = elementNav.SelectSingleNode("..");

                if(nav.Name == parentElementName)
                {
                    if(null != nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[@{1}={2}]", elementName, "id", Utilities.StringToXPathLiteral(identifier))))
                    {
                        throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.NonUniqueId, identifier));
                    }
                }
                nav = elementNav.Clone();
                if(!nav.MoveToAttribute("id", String.Empty))
                {
                    nav.CreateAttribute(String.Empty, "id", String.Empty, identifier);
                }
                else
                {
                    nav.SetValue(identifier);
                }
            }
        }

        /// <summary>
        /// Creates a new writable navigator to a newly created XML block.
        /// </summary>
        /// <remarks>
        /// The xml block created is always of the form:
        /// <code>
        /// &lt;rootElement&gt;
        ///     &lt;element1&gt;
        ///     &lt;element2&gt;
        ///     ...
        /// &lt;/rootElement&gt;
        /// </code>
        /// </remarks>
        /// <param name="rootElement">The name of the root element for the newly created xml block</param>
        /// <param name="elements">The list of child elements for the newly created xml block</param>
        /// <returns>An XPathNavigator that points to the root element.</returns>
        internal static XPathNavigator CreateNavigator(string rootElement, params string[] elements)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement(rootElement));
            foreach(string element in elements)
            {
                root.AppendChild(doc.CreateElement(element));
            }
            return root.CreateNavigator();
        }
    }

    //--------------------------------------------

    /// <summary>
    /// Abstract class used internally for list elements based on XML nodes.  This cannot be an interface because
    /// that would make its members public, and we don't want that.  The class itself must be public so that public
    /// classes can inherit from it, but since all its members are internal that should be okay.
    /// </summary>
    public abstract class DataModelListElement
    {
        /// <summary>
        /// Gets the string unique identifier for the object.
        /// </summary>
        internal abstract string UniqueId
        {
            get;
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal abstract LearningDataModel DataModel
        {
            get;
        }

        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal abstract XPathNavigator Navigator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal abstract bool IsInDataModel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal abstract bool IsInList
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <c>IList</c> implementation on top of a given XML block.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// The caller initializes a <c>DataModelList</c> by passing it XML such as the following:
    /// </para>
	/// <code>
    /// &lt;examples&gt;
    ///      &lt;example id="foo"/&gt;
    ///      &lt;example id="bar"/&gt;
    /// &lt;/examples&gt;
	/// </code>
	/// <para>
	/// In this example, if the <c>DataModelList</c> were enumerated, the returned objects
	/// would refer to the second-level elements, i.e.<c>&lt;example id="foo"/&gt;</c> and
	/// <c>&lt;example id="bar"/&gt;</c>, assuming "example" is used for the <c>elementName</c>
	/// parameter of the constructor.
	/// </para>
	/// <para>
	/// <c>DataModelList</c> optionally checks for uniqueness of a caller-specified attribute,
	/// such as <c>"id"</c> in the example above.  This is specified using the
	/// <c>uniqueIdAttribute</c> parameter of the constructor.
	/// </para>
	/// <para>
	/// The <c>elementName</c> parameter of the constructor must be in the the default
	/// (blank) namespace.
	/// </para>
	/// <para>
	/// This class is used by LearningDataModel to implement lists such as
	/// <c>LearningDataModel.Objectives</c>, <c>LearningDataModel.CommentsFromLearner</c>, etc.
	/// </para>
	/// </remarks>
    /// <typeparam name="T">The type of element in the list.  <c>T</c> must support DataModelListElement.</typeparam>
    internal class DataModelList<T> : IList<T> where T : DataModelListElement
    {
        /// <summary>
        /// The owning data model, in order to call the DataChanged delegate.
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// XPathNavigator that points to the owning element of the list, e.g.
		/// &lt;examples&gt; in the example in the documentation for this class.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// The name of the XML sub-element that represents an element in the list.
		/// This is a local name, without a prefix.
        /// </summary>
        private string m_elementName;

        /// <summary>
        /// The name of an attribute on the sub-element that indicates a unique identifier.  
        /// If no unique identifier exists, this must be String.Empty.
        /// </summary>
        private string m_uniqueIdAttribute;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        public bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
                if(m_list != null)
                {
                    foreach(T item in m_list)
                    {
                        item.IsInDataModel = value;
                    }
                }
            }
        }

        /// <summary>
        /// The list of real objects, not created until needed.
        /// </summary>
        private Collection<T> m_list = null;

        /// <summary>
        /// A delegate that can be used to create a new object of type <c>T</c>.
        /// </summary>
        /// <remarks>
		/// We can't just use the <c>new()</c> constraint on this class since that would require a public
		/// constructor, which we don't want for security (attack surface) reasons.
		/// </remarks>
        /// <param name="dataModel">LearningDataModel used to initialize the object.</param>
        /// <param name="nav">XPathNavigator that points to the XML representation of the object.</param>
        /// <param name="isInDataModel">Indicates whether the child is marked as in the data model or not.</param>
        /// <returns>A newly created object of type T.</returns>
        internal delegate T CreateObject(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel);

        /// <summary>
        /// The delegate used to create new objects of type <c>T</c>.
        /// </summary>
        private CreateObject m_createObject;

        /// <summary>
        /// Initializes a list class used by LearningDataModel that uses an XML block as list elements.  The list itself
        /// is represented by an element that contains a group of sub-elements.
		/// </summary>
		///
		/// <remarks>
		/// See the documentation for the <Typ>DataModelList</Typ> for an example.
		/// </remarks>
        /// 
        /// <param name="dataModel">The LearningDataModel that owns this list.</param>
        /// <param name="nav">An XPathNavigator that points to the owning XML element of the list.</param>
        /// <param name="elementName">The name of the second-level XML element that represents an element in the list.</param>
        /// <param name="uniqueIdAttribute">The name of an attribute on second-level XML elements that indicates the unique identifier for that element.  
        /// If no unique identifier exists, this must be String.Empty.</param>
        /// <param name="isInDataModel">Whether or not this list belongs to the data model, and thus sends notifcations when changes are made.</param>
        /// <param name="createObject">A delegate that can construct a new object of type <c>T</c>.</param>
        internal DataModelList(LearningDataModel dataModel, XPathNavigator nav, string elementName, string uniqueIdAttribute, bool isInDataModel, CreateObject createObject)
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
            m_elementName = elementName;
            m_uniqueIdAttribute = uniqueIdAttribute;
            m_isInDataModel = isInDataModel;
            m_createObject = createObject;
        }

        /// <summary>
        /// An internal helper method to validate additions to the list.
        /// </summary>
        /// <param name="item">The item that is to be added to the list</param>
        /// <exception cref="System.ArgumentException">Thrown if there is no unique identifier defined on the object to be added, 
        /// or if the identifier already exists in the list, or if the data model of the object being added is not the same as the
        /// data model of this list.</exception>
        private void Validate(T item)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(item.DataModel != m_dataModel)
            {
                throw new ArgumentException(Resources.WrongDataModel);
            }
            if(item.IsInList)
            {
                throw new ArgumentException(Resources.AlreadyInList);
            }
            if(!String.IsNullOrEmpty(m_uniqueIdAttribute))
            {
                if(String.IsNullOrEmpty(item.UniqueId))
                {
                    throw new ArgumentException(Resources.NoIdentifier);
                }
                if(null != m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[@{1}={2}]", m_elementName, m_uniqueIdAttribute, Utilities.StringToXPathLiteral(item.UniqueId))))
                {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.NonUniqueId, item.UniqueId));
                }
            }
        }

        /// <summary>
        /// Creates the collection of actual objects, if it is not already created.
        /// </summary>
        private void CreateListIfNecessary()
        {
            if(m_list != null)
            {
                return;
            }
            XPathNodeIterator iter = m_nav.Select(m_elementName);
            m_list = new Collection<T>();
            while(iter.MoveNext())
            {
                m_list.Add(m_createObject(m_dataModel, iter.Current, m_isInDataModel));
            }
        }

        /// <summary>
        /// Detaches an item from the list.  It will no longer be part of any list, the data model, and any changes made
        /// to it will not be reflected anywhere else.
        /// </summary>
        /// <param name="item">Item to detach from the list.</param>
        private void DetachItemFromList(T item)
        {
            XmlDocument doc = new XmlDocument();
            using(XmlReader reader = item.Navigator.ReadSubtree())
            {
                doc.Load(reader);
            }
            item.Navigator = doc.SelectSingleNode(m_elementName).CreateNavigator();
            item.IsInList = false;
            item.IsInDataModel = false;
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            // if there is no list, this object can't exist in the list
            if(m_list == null)
            {
                return -1;
            }
            return m_list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            CreateListIfNecessary();
            if(index > m_list.Count || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if(m_list.Count == index)
            {
                Add(item);
            }
            else
            {
                Validate(item);
                XPathNavigator nav = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", m_elementName, index + 1));
                nav.InsertBefore(item.Navigator);
                item.Navigator = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", m_elementName, index + 1));
                item.IsInDataModel = m_isInDataModel;
                item.IsInList = true;
                m_list.Insert(index, item);
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        public void RemoveAt(int index)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            CreateListIfNecessary();
            if(index >= m_list.Count || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            XPathNavigator nav = m_list[index].Navigator.Clone();
            DetachItemFromList(m_list[index]);
            nav.DeleteSelf();
            m_list.RemoveAt(index);
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
        }

        public T this[int index]
        {
            get
            {
                CreateListIfNecessary();
                return m_list[index];
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                CreateListIfNecessary();
                if(index >= m_list.Count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                Validate(value);
                XPathNavigator nav = m_list[index].Navigator.Clone();
                DetachItemFromList(m_list[index]);
                nav.ReplaceSelf(value.Navigator);
                m_list[index] = value;
                m_list[index].IsInDataModel = m_isInDataModel;
                m_list[index].IsInList = true;
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            CreateListIfNecessary();
            Validate(item);
            m_nav.AppendChild(item.Navigator);
            item.Navigator = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[last()]", m_elementName));
            item.IsInDataModel = m_isInDataModel;
            item.IsInList = true;
            m_list.Add(item);
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
        }

        public void Clear()
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            CreateListIfNecessary();
            if(m_list.Count > 0)
            {
                foreach(T item in m_list)
                {
                    DetachItemFromList(item);
                }
                XPathNavigator nav = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[1]", m_elementName));
                nav.DeleteRange(m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[last()]", m_elementName)));
                m_list.Clear();
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        public bool Contains(T item)
        {
            // if there is no list, this item is not contained in it
            if(m_list == null)
            {
                return false;
            }
            return m_list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CreateListIfNecessary();
            m_list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                if(m_list == null)
                {
                    return m_nav.Select(m_elementName).Count;
                }
                return m_list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            
            if(index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            CreateListIfNecessary();
            return m_list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            CreateListIfNecessary();
            return m_list.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// An <c>IList</c> implementation on top of a given XML block.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// The caller initializes a <c>DataModelList</c> by passing it XML such as the following:
    /// </para>
	/// <code>
    /// &lt;examples&gt;
    ///      &lt;example id="foo"/&gt;
    ///      &lt;example id="bar"/&gt;
    /// &lt;/examples&gt;
	/// </code>
	/// <para>
	/// In this example, if the <c>DataModelList</c> were enumerated, the returned objects
	/// would refer to the second-level elements, i.e.<c>&lt;example id="foo"/&gt;</c> and
	/// <c>&lt;example id="bar"/&gt;</c>, assuming "example" is used for the <c>elementName</c>
	/// parameter of the constructor.
	/// </para>
	/// <para>
	/// <c>DataModelList</c> optionally checks for uniqueness of a caller-specified attribute,
	/// such as <c>"id"</c> in the example above.  This is specified using the
	/// <c>uniqueIdAttribute</c> parameter of the constructor.
	/// </para>
	/// <para>
	/// The <c>elementName</c> parameter of the constructor must be in the the default
	/// (blank) namespace.
	/// </para>
	/// <para>
	/// This class is used by LearningDataModel to implement lists such as
	/// <c>LearningDataModel.Interactions</c>.
	/// </para>
	/// </remarks>
    /// <typeparam name="T">The type of element in the list.  <c>T</c> must support DataModelListElement.</typeparam>
    internal sealed class DataModelKeyedList<T> : KeyedCollection<string, T> where T : DataModelListElement
    {
        /// <summary>
        /// The owning data model, in order to call the DataChanged delegate.
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// XPathNavigator that points to the owning element of the list, e.g.
		/// &lt;examples&gt; in the example in the documentation for this class.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// The name of the XML sub-element that represents an element in the list.
		/// This is a local name, without a prefix.
        /// </summary>
        private string m_elementName;

        /// <summary>
        /// The name of an attribute on the sub-element that indicates a unique identifier.  
        /// If no unique identifier exists, this must be String.Empty.
        /// </summary>
        private string m_uniqueIdAttribute;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Set during the constructor to allow the use of the Add() without invoking the underlying
        /// code to add the xml (which already exists in the list) to the list again.
        /// </summary>
        private bool m_initialLoad;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        public bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
                foreach(T item in Items)
                {
                    item.IsInDataModel = value;
                }
            }
        }

        /// <summary>
        /// A delegate that can be used to create a new object of type <c>T</c>.
        /// </summary>
        /// <remarks>
		/// We can't just use the <c>new()</c> constraint on this class since that would require a public
		/// constructor, which we don't want for security (attack surface) reasons.
		/// </remarks>
        /// <param name="dataModel">LearningDataModel used to initialize the object.</param>
        /// <param name="nav">XPathNavigator that points to the XML representation of the object.</param>
        /// <param name="isInDataModel">Indicates whether the child is marked as in the data model or not.</param>
        /// <returns>A newly created object of type T.</returns>
        internal delegate T CreateObject(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel);

        /// <summary>
        /// Initializes a list class used by LearningDataModel that uses an XML block as list elements.  The list itself
        /// is represented by an element that contains a group of sub-elements.
		/// </summary>
		///
		/// <remarks>
		/// See the documentation for the <Typ>DataModelList</Typ> for an example.
		/// </remarks>
        /// 
        /// <param name="dataModel">The LearningDataModel that owns this list.</param>
        /// <param name="nav">An XPathNavigator that points to the owning XML element of the list.</param>
        /// <param name="elementName">The name of the second-level XML element that represents an element in the list.</param>
        /// <param name="uniqueIdAttribute">The name of an attribute on second-level XML elements that indicates the unique identifier for that element.  
        /// This must not be String.Empty.</param>
        /// <param name="isInDataModel">Whether or not this list belongs to the data model, and thus sends notifcations when changes are made.</param>
        /// <param name="createObject">A delegate that can construct a new object of type <c>T</c>.</param>
        internal DataModelKeyedList(LearningDataModel dataModel, XPathNavigator nav, string elementName, string uniqueIdAttribute, bool isInDataModel, CreateObject createObject)
            : base()
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
            m_elementName = elementName;
            Utilities.Assert(!String.IsNullOrEmpty(uniqueIdAttribute), "LDM0001");
            m_uniqueIdAttribute = uniqueIdAttribute;
            m_isInDataModel = isInDataModel;
            XPathNodeIterator iter = m_nav.Select(m_elementName);
            m_initialLoad = true;
            while(iter.MoveNext())
            {
                Add(createObject(m_dataModel, iter.Current, m_isInDataModel));
            }
            m_initialLoad = false;
        }

        /// <summary>
        /// An internal helper method to validate additions to the list.
        /// </summary>
        /// <param name="item">The item that is to be added to the list</param>
        /// <exception cref="System.ArgumentException">Thrown if there is no unique identifier defined on the object to be added, 
        /// or if the identifier already exists in the list, or if the data model of the object being added is not the same as the
        /// data model of this list.</exception>
        private void Validate(T item)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(item.DataModel != m_dataModel)
            {
                throw new ArgumentException(Resources.WrongDataModel);
            }
            if(item.IsInList)
            {
                throw new ArgumentException(Resources.AlreadyInList);
            }
            if(String.IsNullOrEmpty(item.UniqueId))
            {
                throw new ArgumentException(Resources.NoIdentifier);
            }
            if(null != m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[@{1}={2}]", m_elementName, m_uniqueIdAttribute, Utilities.StringToXPathLiteral(item.UniqueId))))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.NonUniqueId, item.UniqueId));
            }
        }

        /// <summary>
        /// Detaches an item from the list.  It will no longer be part of any list, the data model, and any changes made
        /// to it will not be reflected anywhere else.
        /// </summary>
        /// <param name="item">Item to detach from the list.</param>
        private void DetachItemFromList(T item)
        {
            XmlDocument doc = new XmlDocument();
            using(XmlReader reader = item.Navigator.ReadSubtree())
            {
                doc.Load(reader);
            }
            item.Navigator = doc.SelectSingleNode(m_elementName).CreateNavigator();
            item.IsInList = false;
            item.IsInDataModel = false;
        }

        /// <summary>
        /// Allows a key to be changed.
        /// </summary>
        /// <param name="item">Item who's key is being changed</param>
        /// <param name="newKey">The new key</param>
        internal void ChangeKey(T item, string newKey)
        {
            base.ChangeItemKey(item, newKey);
        }

        protected override string GetKeyForItem(T item)
        {
            return item.UniqueId;
        }

        protected override void ClearItems()
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            if(m_elementName == "objective" && m_dataModel.Format == PackageFormat.V1p3)
            {
                // need this to prevent deletion of primary objective
                if(Count > 1)
                {
                    int n = 1;
                    foreach(T item in Items)
                    {
                        Objective obj = item as Objective;

                        if(!obj.IsPrimaryObjective)
                        {
                            DetachItemFromList(item);
                            XPathNavigator nav = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", m_elementName, n));
                            nav.DeleteSelf();
                        }
                        else
                        {
                            n = 2;
                        }
                    }
                    n = 0;
                    while(Count > 1)
                    {
                        Objective obj = Items[n] as Objective;

                        if(obj.IsPrimaryObjective)
                        {
                            n = 1;
                        }
                        else
                        {
                            base.RemoveAt(n);
                        }
                    }
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
            else
            {
                if(Count > 0)
                {
                    foreach(T item in Items)
                    {
                        DetachItemFromList(item);
                    }
                    XPathNavigator nav = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[1]", m_elementName));
                    nav.DeleteRange(m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[last()]", m_elementName)));
                    base.ClearItems();
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if(!m_initialLoad)
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                Validate(item);
                if(Count == index)
                {
                    m_nav.AppendChild(item.Navigator);
                    item.Navigator = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[last()]", m_elementName));
                }
                else
                {
                    XPathNavigator nav = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", m_elementName, index + 1));
                    nav.InsertBefore(item.Navigator);
                    item.Navigator = m_nav.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", m_elementName, index + 1));
                }
                item.IsInDataModel = m_isInDataModel;
                item.IsInList = true;
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            if(m_elementName == "objective")
            {
                Objective obj = base.Items[index] as Objective;

                if(obj.IsPrimaryObjective)
                {
                    throw new InvalidOperationException(Resources.CannotDeletePrimaryObjective);
                }
            }
            XPathNavigator nav = Items[index].Navigator.Clone();
            DetachItemFromList(Items[index]);
            nav.DeleteSelf();
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            if(m_elementName == "objective")
            {
                Objective obj = base.Items[index] as Objective;

                if(obj.IsPrimaryObjective)
                {
                    throw new InvalidOperationException(Resources.CannotDeletePrimaryObjective);
                }
            }
            Validate(item);
            XPathNavigator nav = Items[index].Navigator.Clone();
            DetachItemFromList(Items[index]);
            nav.ReplaceSelf(item.Navigator);
            item.IsInDataModel = m_isInDataModel;
            item.IsInList = true;
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
            base.SetItem(index, item);
        }
    }

    /// <summary>
    /// An <c>IDictionary</c> implementation on top of a <c>&lt;extensionData&gt;</c> XML block.
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// See DataModel.doc for the schema of <c>&lt;extensionData&gt;</c>.
	/// </para>
	/// <para>
	/// While this class is initialized from XML data and eventually stored as xml data, it is not stored
	/// always as xml data due to complications with attachments (i.e. it is difficult to accurately represent
	/// the data of the attachment within an XML only representation).
	/// </para>
    /// <para>
    /// </para>
	/// </remarks>
    internal class DataModelExtensionDictionary : IDictionary<string, object>
    {
        /// <summary>
        /// A regular dictionary class used internally for storage of the actual data
        /// </summary>
        private Dictionary<string, object> m_extensions = new Dictionary<string, object>();

        /// <summary>
        /// An XPathNavigator that points the the owning &lt;extensionData&gt; element that represents this dictionary.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// The owning data model of this dictionary.
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// True if the list is part of the data model, indicating that the data model must be notified
        /// when changes are made to the list.
        /// </summary>
        public bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }

        /// <summary>
        /// A simple list of supported types
        /// </summary>
        private enum DataModelExtensionType
        {
            Attachment,
            Bool,
            Datetime,
            Double,
            Int,
            String
        }

        /// <summary>
        /// Initializes a custom dictionary class for use by LearningDataModel.  Unlike DataModelList,
        /// this class is only used for one purpose so the XML elements are pre-defined.
        /// </summary>
        /// <param name="dataModel">The owning data model of this dictionary.</param>
        /// <param name="nav">An XPathNavigator that points the the owning &lt;extensionData&gt; element that represents this dictionary.</param>
        /// <param name="isInDataModel">Whether or not this list belongs to the data model, and thus sends notifcations when changes are made.</param>
        public DataModelExtensionDictionary(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;

            XPathNodeIterator iter = m_nav.Select("extensionDataVariable");
            while(iter.MoveNext())
            {
                string str = iter.Current.GetAttribute("type", String.Empty);
                if(str == DataModelExtensionType.Attachment.ToString())
                {
                    IAttachment attachment = m_dataModel.WrapAttachmentGuid(XmlConvert.ToGuid(iter.Current.Value), m_dataModel.InternalActivityId);
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), attachment);
                }
                else if(str == DataModelExtensionType.Bool.ToString())
                {
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), iter.Current.ValueAsBoolean);
                }
                else if(str == DataModelExtensionType.Datetime.ToString())
                {
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), iter.Current.ValueAsDateTime);
                }
                else if(str == DataModelExtensionType.Double.ToString())
                {
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), iter.Current.ValueAsDouble);
                }
                else if(str == DataModelExtensionType.Int.ToString())
                {
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), iter.Current.ValueAsInt);
                }
                else if(str == DataModelExtensionType.String.ToString())
                {
                    m_extensions.Add(iter.Current.GetAttribute("name", String.Empty), iter.Current.Value);
                }
                else
                {
                    // someone added to the enum and didn't tell me
                    Utilities.Assert(false, "LDM0002");
                }
            }
        }

        /// <summary>
        /// Refreshes the &lt;extensionData&gt; node that is represented by this class,
		/// i.e. copies the information from <c>m_extensions</c> back into <c>m_nav</c>.
		/// </summary>
        private void RefreshXml()
        {
			// first, delete all the "<extensionDataVariable>" elements in this
			// "<extensionData>" element
			XPathNavigator nav = m_nav.SelectSingleNode("extensionDataVariable[1]");
            if(nav != null)
            {
                nav.DeleteRange(m_nav.SelectSingleNode("extensionDataVariable[last()]"));
            }

			// copy from <m_extensions> into new "<extensionDataVariable>" elements
            if(m_extensions.Count > 0)
            {
                XmlWriter writer = m_nav.AppendChild();
                foreach(KeyValuePair<string, object> item in m_extensions)
                {
                    writer.WriteStartElement("extensionDataVariable");
                    writer.WriteAttributeString("name", item.Key);
                    if(item.Value is string)
                    {
                        writer.WriteAttributeString("type", DataModelExtensionType.String.ToString());
                        writer.WriteValue((string)item.Value);
                    }
                    else if(item.Value is int)
                    {
                        writer.WriteAttributeString("type", DataModelExtensionType.Int.ToString());
                        writer.WriteValue((int)item.Value);
                    }
                    else if(item.Value is bool)
                    {
                        writer.WriteAttributeString("type", DataModelExtensionType.Bool.ToString());
                        writer.WriteValue((bool)item.Value);
                    }
                    else if(item.Value is double)
                    {
                        writer.WriteAttributeString("type", DataModelExtensionType.Double.ToString());
                        writer.WriteValue((double)item.Value);
                    }
                    else if(item.Value is DateTime)
                    {
                        writer.WriteAttributeString("type", DataModelExtensionType.Datetime.ToString());
                        writer.WriteValue((DateTime)item.Value);
                    }
                    else
                    {
                        IAttachment attachment = item.Value as IAttachment;

                        // this should always be non-null, otherwise we have a bug
                        writer.WriteAttributeString("type", DataModelExtensionType.Attachment.ToString());
                        writer.WriteValue(attachment.Guid.ToString());
                    }
                    writer.WriteEndElement();
                }
                writer.Close();
            }
        }

        /// <summary>
		/// Converts a value to one of the types listed in <c>DataModelExtensionType</c>; throws a
		/// <c>ArgumentException</c> if the value can't be converted to one of those types.
		/// In most cases, the conversion is a no-op -- this method just does validation.
		/// In the case of an attachment (a byte[] array), a wrapper for the attachment is 
        /// returned (see Remarks).
		/// </summary>
		/// <remarks>
		/// <para>
        /// If <paramref name="value"/> is one of the primitive data model extension types
		/// (string, int, bool, double, DateTime), then that value is returned directly.  If 
        /// a float is passed, it is converted to a double.  The only other type supported is 
        /// byte[] array, and in this case this method will call the
		/// <Prp>LearningDataModel.WrapAttachment</Prp> delegate to "wrap" the value in
		/// an object that implements <c>IAttachment</c>.
		/// </para>
		/// <para>
        /// This method should only be called when adding a new object is to the dictionary.
		/// Otherwise, the exception text may be misleading.
		/// </para>
        /// </remarks>
        /// <param name="value">The object to validate for type</param>
        /// <returns>The same object, or a wrapper for that object if it is an attachment</returns>
        private object ConvertToDataModelExtensionValue(object value)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            if(value is string ||
                value is int ||
                value is bool ||
                value is double ||
                value is DateTime)
            {
                return value;
            }
            else if(value is float)
            {
                return (double)(float)value;
            }
            byte[] bytearray = value as byte[];
            if(bytearray != null)
            {
                return m_dataModel.WrapAttachment(bytearray, m_dataModel.InternalActivityId);
            }
            if(m_dataModel.AdvancedAccess)
            {
                if(value is Guid)
                {
                    return m_dataModel.WrapAttachmentGuid((Guid)value, m_dataModel.InternalActivityId);
                }
            }
            throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidExtensionType, value.GetType().FullName));
        }

        /// <summary>
        /// Converts an element in the dictionary into a value that can be returned to the user.
        /// </summary>
        /// <param name="value">A value stored in the internal dictionary.</param>
        /// <returns>A value ready to be used by the end user</returns>
        /// <remarks>This method does not do any conversion of any item except for attachments, which
        /// are converted from an IAttachment object to a byte array.
        /// </remarks>
        private object ConvertFromDataModelExtensionValue(object value)
        {
            IAttachment attachment = value as IAttachment;
            if(attachment != null)
            {
                if(m_dataModel.AdvancedAccess)
                {
                    return attachment;
                }
                else
                {
                    return attachment.GetBytes();
                }
            }
            return value;
        }

        #region IDictionary<string,object> Members

        public void Add(string key, object value)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            m_extensions.Add(key, ConvertToDataModelExtensionValue(value));
            RefreshXml();
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
        }

        public bool ContainsKey(string key)
        {
            return m_extensions.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get
            {
                return m_extensions.Keys;
            }
        }

        public bool Remove(string key)
        {
            bool ret = m_extensions.Remove(key);
            if(ret)
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                RefreshXml();
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
            return ret;
        }

        public bool TryGetValue(string key, out object value)
        {
            return m_extensions.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get
            {
                Collection<object> c = new Collection<object>();
                foreach(object o in m_extensions.Values)
                {
                    c.Add(ConvertFromDataModelExtensionValue(o));
                }
                return c;
            }
        }

        public object this[string key]
        {
            get
            {
                return ConvertFromDataModelExtensionValue(m_extensions[key]);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                m_extensions[key] = ConvertToDataModelExtensionValue(value);
                RefreshXml();
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<string, object> item)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            m_extensions.Add(item.Key, ConvertToDataModelExtensionValue(item.Value));
            RefreshXml();
            if(m_isInDataModel)
            {
                m_dataModel.CallDataChanged();
            }
        }

        public void Clear()
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            if(m_extensions.Count > 0)
            {
                m_extensions.Clear();
                RefreshXml();
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return (m_extensions.ContainsKey(item.Key) && (ConvertFromDataModelExtensionValue(m_extensions[item.Key]).Equals(item.Value)));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if(array == null)
            {
                throw new ArgumentNullException("array");
            }
            if(arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            if(m_extensions.Count + arrayIndex > array.Length)
            {
                throw new ArgumentException(Resources.IndexOutOfRange, "arrayIndex");
            }
            foreach(KeyValuePair<string, object> kv in m_extensions)
            {
                KeyValuePair<string, object> newkv = new KeyValuePair<string, object>(kv.Key, ConvertFromDataModelExtensionValue(kv.Value));
                array[arrayIndex++] = newkv;
            }
        }

        public int Count
        {
            get
            {
                return m_extensions.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if(m_isInDataModel)
            {
                m_dataModel.CheckIfWriteIsAllowed();
            }
            if(m_extensions.Remove(item.Key))
            {
                RefreshXml();
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach(KeyValuePair<string, object> kv in m_extensions)
            {
                KeyValuePair<string, object> newkv = new KeyValuePair<string, object>(kv.Key, ConvertFromDataModelExtensionValue(kv.Value));
                yield return newkv;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach(KeyValuePair<string, object> kv in m_extensions)
            {
                KeyValuePair<string, object> newkv = new KeyValuePair<string, object>(kv.Key, ConvertFromDataModelExtensionValue(kv.Value));
                yield return newkv;
            }
        }

        #endregion
    }

    /// <summary>
    /// This enumeration defines what the DataModel should do when any data is written to it.
    /// </summary>
    internal enum DataModelWriteValidationMode
    {
        NeverAllowWrite,
        AlwaysAllowWrite,
        AllowWriteOnlyIfActive
    }

    /// <summary>
    /// This enumeration defines the various states an activity's data model may be in upon the entry of the activity.
    /// </summary>
    public enum EntryMode
    {
        /// <summary>
        /// Indicates that the learner has not accessed this RLO during the current learner attempt
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1706", Justification = "Ab is not an acronym, contrary to what the warning tries to imply.")]
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "AbInitio is not mispelled.")]
        AbInitio,
        /// <summary>
        /// Indicates that (1) the learner has previously accessed the RLO, and (2) upon exiting, the cmi.exit data model element was "suspend" or "logout"
        /// </summary>
        Resume,
        /// <summary>
        /// Indicates that (1) the learner has previously accessed the RLO, and (2) upon exiting, the cmi.exit data model element was NOT "suspend" or "logout"
        /// </summary>
        AllOtherConditions
    }

    /// <summary>
    /// An in-memory representation of the data model associated with one activity of one attempt of one package.
    /// </summary>
    /// <remarks>
    /// <para>
	/// <c>LearningDataModel</c> represents the SCORM RTE data model data associated with one activity.
	/// In other words, a <c>LearningDataModel</c> instance represents information related to one learner's
	/// execution of one RLO within an attempt of one package.
    /// </para>
    /// <para>
	/// <c>LearningDataModel</c> exposes the data model on an object derived from <Typ>LearningSession</Typ>.
	/// An application cannot directly create an instance of <c>LearningDataModel</c> or save the data stored within it.
	/// Instead, the application uses the <c>CurrentActivityDataModel</c> property of 
    /// <Typ>Microsoft.LearningComponents.Storage.StoredLearningSession</Typ> to load a data model into memory, and one of various methods of
    /// <Typ>Microsoft.LearningComponents.Storage.StoredLearningSession</Typ> (such as <c>CommitChanges</c>) to save the data.
    /// </para>
    /// <para>
	/// An activity's data model includes both read-only and read/write data.  Read-only data comes from
	/// the package manifest or application and is not set by calls to <c>LearningDataModel</c>.
	/// Read/write data can be set the application or the RLO by calls to <c>LearningDataModel</c>.
	/// Some read/write data (including some objective information) is initialized from the package
	/// manifest but is read/write and can be overwritten by the application or RLO.  A data model may
    /// also become entirely read-only in certain views (e.g. Review view) or if the data model is not
    /// associated with the currently active activity.
	/// </para>
    /// </remarks>
    public class LearningDataModel
    {
        /// <summary>
        /// The format of the package for this LearningDataModel.
        /// </summary>
        private PackageFormat m_format;

        /// <summary>
        /// A class that will verify settable properties.  Which class is used depends on whether the
		/// package version is SCORM 1.2 or 2004.
        /// </summary>
        private DataModelVerifier m_verifier;

        /// <summary>
        /// XML representation of static data.  This XML data will never be written to.
        /// </summary>
        private XPathNavigator m_staticData;

        /// <summary>
        /// XML representation of dynamic sequencing data.  See the constructor for more information about
		/// the difference between <c>m_sequencingData</c> and <c>m_dynamicData</c>.
        /// </summary>
        private XPathNavigator m_sequencingData;

        /// <summary>
        /// XML representation of dynamic sequencing data.  See the constructor for more information about
		/// the difference between <c>m_sequencingData</c> and <c>m_dynamicData</c>.
        /// </summary>
        /// <remarks>
        /// This needs to be exposed because the <Typ>Activity</Typ> class may need to clone the data 
        /// contained within.
        /// </remarks>
        internal XPathNavigator SequencingData
        {
            get
            {
                return m_sequencingData;
            }
        }

        /// <summary>
        /// Represents the Tracked value in the SequencingData.
        /// </summary>
        private bool m_tracked = true;

        /// <summary>
        /// Represents the Tracked value in the SequencingData.  Set by the Activity upon creation of the
        /// LearningDataModel.  Used to restrict Objective Progress Information for untracked activities,
        /// as per SCORM SN 4.2.1 and the SCORM 2004 Addendum Version 1.2 section 2.25.
        /// </summary>
        internal bool Tracked
        {
            get
            {
                return m_tracked;
            }
            set
            {
                m_tracked = value;
            }
        }

        /// <summary>
        /// Represents the Objective Set by Content value in the SequencingData.
        /// </summary>
        private bool m_objectiveSetByContent;

        /// <summary>
        /// Represents the Objective Set by Content value in the SequencingData.  Set by the Activity upon creation of the
        /// LearningDataModel.  When <c>true</c>, prevents setting Objective Progress Status.
        /// </summary>
        internal bool ObjectiveSetByContent
        {
            get
            {
                return m_objectiveSetByContent;
            }
            set
            {
                m_objectiveSetByContent = value;
            }
        }

        /// <summary>
        /// Represents the Completion Set by Content value in the SequencingData.
        /// </summary>
        private bool m_completionSetByContent;

        /// <summary>
        /// Represents the Completion Set by Content value in the SequencingData.  Set by the Activity upon creation of the
        /// LearningDataModel.  When <c>true</c>, prevents setting Attempt Progress Status.
        /// </summary>
        internal bool CompletionSetByContent
        {
            get
            {
                return m_completionSetByContent;
            }
            set
            {
                m_completionSetByContent = value;
            }
        }

        /// <summary>
        /// XML representation of dynamic data not related to sequencing.  See the constructor for more information
		/// about the difference between <c>m_sequencingData</c> and <c>m_dynamicData</c>.
        /// </summary>
        private XPathNavigator m_dynamicData;

        /// <summary>
        /// Whether or not either a non-null dynamic data parameter was passed to the constructor
        /// or the <Mth>SetDynamicData</Mth> method has been called.
        /// </summary>
        private bool m_dynamicDataIsValid;

        /// <summary>
        /// Whether the comments from lms data is valid or not.  If this value is false, the 
        /// <Mth>SetDynamicData</Mth> method may override any existing data within m_commentsFromLms.
        /// </summary>
        private bool m_commentsFromLmsAreValid;

        /// <summary>
        /// Delegate definition for a function that will be called when any data is changed.
        /// </summary>
        internal delegate void DataChangeDelegate();

        /// <summary>
        /// Occurs when any data model elements change.
        /// </summary>
        private event DataChangeDelegate m_dataChange;

        /// <summary>
        /// Delegate definition for a function that will be called when the overall score for this
        /// activity is updated.
        /// </summary>
        /// <param name="oldScore">The previous score for this activity.</param>
        /// <param name="newScore">The new score for this activity.</param>
        internal delegate void UpdateScoreDelegate(float? oldScore, float? newScore);

        /// <summary>
        /// Called when either the primary Score.Scaled changes (in V1p3 content) or when
        /// EvaluationPoints changes (in Lrm or V1p2 content).
        /// </summary>
        private UpdateScoreDelegate m_updateScore;

        /// <summary>
        /// Gets or sets the delegate called when either the primary Score.Scaled 
        /// changes (in V1p3 content) or when EvaluationPoints changes (in Lrm or V1p2 content).
        /// </summary>
        internal UpdateScoreDelegate UpdateScore
        {
            get
            {
                return m_updateScore;
            }
            set
            {
                m_updateScore = value;
            }
        }

        /// <summary>
        /// Delegate called when an <Typ>IAttachment</Typ> is required from a <Typ>Guid</Typ>, so that the caller of this
		/// <Typ>LearningDataModel</Typ> can load the data of the attachment.
        /// </summary>
        /// <param name="guid">Guid that corresponds to an attachment.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        /// <returns>An object that implements <Typ>IAttachment</Typ>.</returns>
		/// <remarks>
		/// This delegate is called on construction of <Typ>DataModelExtensionDictionary</Typ>.
		/// </remarks>
        internal delegate IAttachment WrapAttachmentGuidDelegate(Guid guid, long internalActivityId);
        
        /// <summary>
        /// Delegate called when a new <Typ>Guid</Typ> is required for a byte[] array that represents an attachment.
        /// </summary>
        /// <param name="attachment">A byte[] array that represents an attachment.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        /// <returns>An object that implements <Typ>IAttachment</Typ>.</returns>
		/// <remarks>
		/// This delegate is called on each "set value" call for attachments.
		/// </remarks>
        internal delegate IAttachment WrapAttachmentDelegate(byte[] attachment, long internalActivityId);

        /// <summary>
        /// The delegate called to wrap a <Typ>Guid</Typ> with the interfaces required internally for attachments.
        /// </summary>
        private WrapAttachmentGuidDelegate m_wrapAttachmentGuid;

        /// <summary>
        /// The delegate called to wrap a byte[] array with the interfaces required internally for attachments.
        /// </summary>
        private WrapAttachmentDelegate m_wrapAttachment;

        /// <summary>
        /// it is sometimes necessary for the data model to know what activity it is attached to, though
        /// this is really only necessary when saving attachment extension data to the database.
        /// </summary>
        private long m_internalActivityId;

        /// <summary>
        /// it is sometimes necessary for the data model to know what activity it is attached to, though
        /// this is really only necessary when saving attachment extension data to the database.
        /// </summary>
        internal long InternalActivityId
        {
            get
            {
                return m_internalActivityId;
            }
            set
            {
                m_internalActivityId = value;
            }
        }

        // lists for dynamic non-sequencing data

        /// <summary>
        /// List of comments from learner (cmi.comments_from_learner)
        /// </summary>
        private DataModelList<Comment> m_commentsFromLearner;

        /// <summary>
        /// List of comments from LMS (cmi.comments_from_lms)
        /// </summary>
        private DataModelList<CommentFromLms> m_commentsFromLms;

        /// <summary>
        /// List of interactions (cmi.interactions)
        /// </summary>
        private DataModelList<Interaction> m_interactions;

        /// <summary>
        /// Dictionary of extension data information
        /// </summary>
        private DataModelExtensionDictionary m_extensionData;

        /// <summary>
        /// Learner information
        /// </summary>
        private Learner m_learner;

        /// <summary>
        /// The score associated with this data model
        /// </summary>
        private Score m_score;

        // dynamic data classes

        /// <summary>
        /// List of objectives (cmi.objectives)
        /// </summary>
        private DataModelKeyedList<Objective> m_objectives;

        /// <summary>
        /// NavigationRequest information (cmi.exit, adl.nav.request)
        /// </summary>
        private NavigationRequest m_navigationRequest;

        /// <summary>
        /// Determines how to check whether write is valid or not.
        /// </summary>
        private DataModelWriteValidationMode m_writeValidationMode;

        /// <summary>
        /// Set during ExpandDataModelCache and ReconstituteDataModelCache to allow extra
        /// information to be set/retrieved with regard to attachments.  Also always allows 
        /// write access to the data model.
        /// </summary>
        private bool m_advancedAccess;

        /// <summary>
        /// Set during ExpandDataModelCache and ReconstituteDataModelCache to allow extra
        /// information to be set/retrieved with regard to attachments.  Also always allows 
        /// write access to the data model.
        /// </summary>
        internal bool AdvancedAccess
        {
            get
            {
                return m_advancedAccess;
            }
            set
            {
                m_advancedAccess = value;
            }
        }

        /// <summary>
        /// private stored versions of the learner information, so it can be reset
        /// when necessary (upon re-entry to this activity).
        /// </summary>
        private string m_learnerId;
        private string m_learnerName;
        private string m_learnerLanguage;
        private AudioCaptioning m_learnerCaption;
        private float m_learnerAudioLevel;
        private float m_learnerDeliverySpeed;

        /// <summary>
        /// Creates a new LearningDataModel.
        /// </summary>
        /// <remarks>
		/// <para>
        /// All learner data passed is assumed to be in SCORM 2004 format, whether or not this LearningDataModel is
        /// to be used for SCORM 1.2 or not.  This is how they are stored in the database.
		/// </para>
		/// <para>
		/// When <c>Navigator</c> performs a navigation operation, it only needs to initialize
		/// <paramref name="staticData"/> and <paramref name="sequencingData"/>, but not
		/// <paramref name="dynamicData"/>, so the latter isn't loaded in that case.
		/// <paramref name="dynamicData"/> contains all data related to an activity that isn't
		/// in <paramref name="staticData"/> or <paramref name="sequencingData"/>.
		/// </para>
        /// </remarks>
        /// <param name="format">The SCORM version, used for validation of values.</param>
        /// <param name="staticData">Static data, originally derived from the manifest (plus other data
		/// not from the manifest, e.g. comments_from_lms, currently in the internal XML format documented in
		/// DataModel.doc.  This is a "&lt;item&gt;" element.  This corresponds to the
		/// ActivityPackageItem.DataModelCache column.  This may not be null.</param>
		/// <param name="sequencingData">Dynamic data, used for sequencing; also an "&lt;item&gt;" element.  This
		/// corresponds to the ActivityAttemptItem.SequencingDataCache column.  This may be null, in which case
		/// defaults are used.</param>
		/// <param name="dynamicData">Dynamic data which is not used for sequencing; also an "&lt;item&gt;" element.
		/// This corresponds to the ActivityAttemptItem.DataModelCache column.  This may be null, in which case
		/// defaults are used.</param>
        /// <param name="commentsFromLms">An xml block in LMS Comments XML format.</param>
        /// <param name="wrapAttachment">A delegate to wrap attachments.</param>
        /// <param name="wrapAttachmentGuid">A delegate to wrap guids as attachments.</param>
        /// <param name="writeValidationMode">Validation mode to determine if the data model is writable.</param>
        /// <param name="learnerId">The unique identifier of the learner.</param>
        /// <param name="learnerName">The name of the learner.</param>
        /// <param name="learnerLanguage">The language code for the learner.</param>
        /// <param name="learnerCaption">The AudioCaptioning setting for the learner.</param>
        /// <param name="learnerAudioLevel">The audio level setting for the learner.</param>
        /// <param name="learnerDeliverySpeed">The delivery speed setting for the learner.</param>
        internal LearningDataModel(PackageFormat format, XPathNavigator staticData, XPathNavigator sequencingData, 
            XPathNavigator dynamicData, XPathNavigator commentsFromLms, WrapAttachmentDelegate wrapAttachment, 
            WrapAttachmentGuidDelegate wrapAttachmentGuid, DataModelWriteValidationMode writeValidationMode,
            string learnerId, string learnerName, string learnerLanguage, AudioCaptioning learnerCaption, float learnerAudioLevel, float learnerDeliverySpeed)
        {
            // since this is internal, Asserts are OK instead of exceptions.
            Utilities.Assert(staticData != null, "LDM0004");

            m_format = format;
            switch(m_format)
            {
            case PackageFormat.Lrm:
                m_verifier = new DataModelVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new DataModelVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new DataModelVerifierV1p3();
                break;
            default:
                Utilities.Assert(false, "LDM0003");
                break;
            }

            m_wrapAttachment = wrapAttachment;
            m_wrapAttachmentGuid = wrapAttachmentGuid;
            m_writeValidationMode = writeValidationMode;

            // make a clone of the passed XPathNavigator, so that the caller is free to change it
            m_staticData = staticData.SelectSingleNode("/item");

            // initialize m_commentsFromLms

            XPathNavigator n;
            if(commentsFromLms == null)
            {
                // Create a <commentsFromLMS/> xml block
                n = DataModelUtilities.CreateNavigator("commentsFromLMS");
                // whether or not comments from lms is valid is undetermined
            }
            else
            {
                n = commentsFromLms.SelectSingleNode("/commentsFromLMS");
                m_commentsFromLmsAreValid = true;
            }
            m_commentsFromLms = new DataModelList<CommentFromLms>(this, n, "comment", String.Empty, true, 
                delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                {
                    return new CommentFromLms(dataModel, nav);
                });

            // if there is no dynamic data, create an empty block
            if(dynamicData == null)
            {
                // Create a <item><commentsFromLearner/><learner/><interactions/><extensionData/></item> xml block
                m_dynamicData = DataModelUtilities.CreateNavigator("item", "commentsFromLearner", "learner", "interactions", "extensionData");
                // whether or not dynamic data is valid is undetermined.
            }
            else
            {
                m_dynamicData = dynamicData.SelectSingleNode("/item");
                m_dynamicDataIsValid = true;
            }

			// initialize more stuff from dynamic data
            m_commentsFromLearner = new DataModelList<Comment>(this, m_dynamicData.SelectSingleNode("commentsFromLearner").CreateNavigator(), "comment", String.Empty, true,
                delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                {
                    return new Comment(dataModel, nav, isInDataModel, true);
                });
            m_interactions = new DataModelList<Interaction>(this, m_dynamicData.SelectSingleNode("interactions").CreateNavigator(), "interaction", String.Empty, true,
                delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                {
                    return new Interaction(dataModel, nav, isInDataModel, true);
                });
            m_extensionData = new DataModelExtensionDictionary(this, m_dynamicData.SelectSingleNode("extensionData").CreateNavigator(), true);
            m_learnerId = learnerId;
            m_learnerName = learnerName;
            m_learnerLanguage = learnerLanguage;
            m_learnerCaption = learnerCaption;
            m_learnerAudioLevel = learnerAudioLevel;
            m_learnerDeliverySpeed = learnerDeliverySpeed;
            m_learner = new Learner(this, m_dynamicData.SelectSingleNode("learner"), m_learnerId, m_learnerName, m_learnerLanguage, m_learnerCaption, m_learnerAudioLevel, m_learnerDeliverySpeed);

            // if there is no dynamic sequencing data, create an empty block.
            // since the objective data is a combination of static and dynamic data, copy over
            // the static information so its all in one convenient place.
            if(sequencingData == null)
            {
                // Create a <item><navigationRequest/><score/><objectives/></item> xml block
                m_sequencingData = DataModelUtilities.CreateNavigator("item", "navigationRequest", "score", "objectives");
                CopyStaticObjectives();
            }
            else
            {
                m_sequencingData = sequencingData.SelectSingleNode("/item");
            }

			// initialize more stuff from sequencing data
            m_objectives = new DataModelKeyedList<Objective>(this, m_sequencingData.SelectSingleNode("objectives").CreateNavigator(), "objective", "id", true,
                delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                {
                    return new Objective(dataModel, nav, isInDataModel, true);
                });
            m_score = new Score(this, m_sequencingData.SelectSingleNode("score"), true);
            m_navigationRequest = new NavigationRequest(this, m_sequencingData.SelectSingleNode("navigationRequest"));
        }

        /// <summary>
        /// Throws an InvalidOperationException if this data model cannot be written to.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal void CheckIfWriteIsAllowed()
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if(!m_advancedAccess)
            {
                switch(m_writeValidationMode)
                {
                case DataModelWriteValidationMode.AlwaysAllowWrite:
                    break;
                case DataModelWriteValidationMode.NeverAllowWrite:
                    throw new InvalidOperationException(Resources.CantWriteToDataModel);
                case DataModelWriteValidationMode.AllowWriteOnlyIfActive:
                    if(!ActivityIsActive)
                    {
                        throw new InvalidOperationException(Resources.CantWriteToDataModel);
                    }
                    break;
                default:
                    throw new LearningComponentsInternalException("LDM0001");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the dynamic data is valid or not.  If this value is false, the 
        /// <Mth>SetDynamicData</Mth> method may override any existing data within m_dynamicData.
        /// </summary>
        internal bool DynamicDataIsValid
        {
            get
            {
                return m_dynamicDataIsValid;
            }
            set
            {
                Utilities.Assert(value, "LDM1000");  // only allow this to be set to true;
                m_dynamicDataIsValid = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the comments from lms data is valid or not.  If this value is false, the 
        /// <Mth>SetDynamicData</Mth> method may override any existing data within m_commentsFromLms.
        /// </summary>
        internal bool CommentsFromLmsAreValid
        {
            get
            {
                return m_commentsFromLmsAreValid;
            }
            set
            {
                Utilities.Assert(value, "LDM1001");  // only allow this to be set to true;
                m_commentsFromLmsAreValid = value;
            }
        }
        
        /// <summary>
        /// Sets the dynamic data not used for sequencing for this data model.
        /// </summary>
		/// <param name="dynamicData">Dynamic data which is not used for sequencing; also an "&lt;item&gt;" element.
		/// This corresponds to the ActivityAttemptItem.DataModelCache column.  This may be null, in which case
		/// defaults are used.</param>
        /// <param name="commentsFromLms">An xml block in LMS Comments XML format.</param>
        /// <remarks>
        /// Initially these fields are filled with defaults.  When an activity becomes the current activity,
        /// these fields are read from the database and filled in as necessary.  The values read from the
        /// database, and thus passed to this function may still be null.
        /// </remarks>
        internal void SetDynamicData(XPathNavigator dynamicData, XPathNavigator commentsFromLms)
        {
            // initialize m_commentsFromLms, if it exists, otherwise keep the defaults we created in the constructor
            if(commentsFromLms != null)
            {
                Utilities.Assert(!m_commentsFromLmsAreValid, "LDM1002");
                XPathNavigator n = commentsFromLms.SelectSingleNode("/commentsFromLMS");
                m_commentsFromLms = new DataModelList<CommentFromLms>(this, n, "comment", String.Empty, true, 
                    delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                    {
                        return new CommentFromLms(dataModel, nav);
                    });
                m_commentsFromLmsAreValid = true;
            }

            // initialize dynamic data and associated lists, if it exists, otherwise keep the 
            // defaults we created in the constructor
            if(dynamicData != null)
            {
                Utilities.Assert(!m_dynamicDataIsValid, "LDM1003");
                m_dynamicData = dynamicData.SelectSingleNode("/item");

			    // initialize more stuff from dynamic data
                m_commentsFromLearner = new DataModelList<Comment>(this, m_dynamicData.SelectSingleNode("commentsFromLearner").CreateNavigator(), "comment", String.Empty, true,
                    delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                    {
                        return new Comment(dataModel, nav, isInDataModel, true);
                    });
                m_interactions = new DataModelList<Interaction>(this, m_dynamicData.SelectSingleNode("interactions").CreateNavigator(), "interaction", String.Empty, true,
                    delegate(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
                    {
                        return new Interaction(dataModel, nav, isInDataModel, true);
                    });
                m_extensionData = new DataModelExtensionDictionary(this, m_dynamicData.SelectSingleNode("extensionData").CreateNavigator(), true);
                
                // for learner, copy over the parts that were created in the constructor to be used as defaults
                // in case these have not been set in the xml already.
                Learner learner = new Learner(this, m_dynamicData.SelectSingleNode("learner"), m_learner.Id, m_learner.Name, m_learner.Language, m_learner.AudioCaptioning, m_learner.AudioLevel, m_learner.DeliverySpeed);
                m_learner = learner;
                m_dynamicDataIsValid = true;
            }
        }

        /// <summary>
        /// Copies static objective information from <c>m_staticData</c> to the dynamic data xml
		/// (<c>m_sequencingData</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The actual XML is not copied directly, but massaged into a more easily usable format.  See DataModel.doc
        /// for the actual representation.
        /// </para>
        /// <para>
        /// Note that the data within the <Typ>ManifestReaderSequencing</Typ> object is presumed to be
        /// pre-validated.  No error conditions are checked for in this method.
        /// </para>
        /// </remarks>
        private void CopyStaticObjectives()
        {
            if(m_format == PackageFormat.V1p3)
            {
                XmlNamespaceManager ns = new XmlNamespaceManager(m_staticData.NameTable);
                ns.AddNamespace("imsss", "http://www.imsglobal.org/xsd/imsss");
                XPathNavigator from = m_staticData.SelectSingleNode("/item/imsss:sequencing/imsss:objectives", ns);
                XPathNavigator to = m_sequencingData.SelectSingleNode("/item/objectives");
                if(from == null)
                {
                    // no objectives, nothing to copy.
                    // create a PrimaryObjective anyway
                    CreateDefaultPrimaryObjective(to);
                    return;
                }
                from.MoveToParent();
                SequencingNodeReader sequencing = new SequencingNodeReader(from, new ManifestReaderSettings(false, false),
                    new PackageValidatorSettings(ValidationBehavior.Enforce, ValidationBehavior.None, ValidationBehavior.None, ValidationBehavior.None), false, null);

                foreach(SequencingObjectiveNodeReader obj in sequencing.Objectives.Values)
                {
                    CopyObjective(obj, to);
                }
            }
        }

        /// <summary>
        /// Creates a primary objective with all default attributes
        /// </summary>
        /// <remarks>
        /// <para>
        /// By always creating a primary objective, several other sections of code that rely on having a 
        /// primary objective are made easier to deal with.
        /// </para>
        /// </remarks>
        /// <param name="to">XPathNavigator that points to the parent element within "sequencing data" XML to which
        /// children &lt;objective&gt; items should be added.</param>
        private static void CreateDefaultPrimaryObjective(XPathNavigator to)
        {
            XmlWriter writer = to.AppendChild();
            writer.WriteStartElement("objective");

            writer.WriteAttributeString("isPrimary", "true");

            // don't write an id, the primary objective need not have one

            writer.WriteElementString("score", String.Empty);
            writer.WriteElementString("extensionData", String.Empty);
            writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// Copies static information about a single objective from "static data" XML to "sequencing data" XML.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The actual XML is not copied directly, but massaged into a more easily usable format.  See DataModel.doc
        /// for the actual representation.
        /// </para>
        /// <para>
        /// Note that the data within the <Typ>ManifestReaderSequencingObjective</Typ> object is presumed to be
        /// pre-validated.  No error conditions are checked for in this method.
        /// </para>
        /// </remarks>
        /// <param name="obj">The objective</param>
        /// <param name="to">XPathNavigator that points to the parent element within "sequencing data" XML to which
		/// children &lt;objective&gt; items should be added.</param>
        private static void CopyObjective(SequencingObjectiveNodeReader obj, XPathNavigator to)
        {
            XmlWriter writer = to.AppendChild();
            writer.WriteStartElement("objective");
            
            if(obj.IsPrimaryObjective)
            {
                writer.WriteAttributeString("isPrimary", "true");
            }

            // the primary objective may have an empty id
            if(!String.IsNullOrEmpty(obj.Id))
            {
                writer.WriteAttributeString("id", obj.Id);
            }

            // false is the default, so don't bother to write it if it is false
            if(obj.SatisfiedByMeasure)
            {
                writer.WriteAttributeString("satisfiedByMeasure", XmlConvert.ToString(true));
            }

            // 1.0 is the default, so don't bother to write it if it is 1.0
            if(obj.MinimumNormalizedMeasure != 1.0)
            {
                writer.WriteAttributeString("minNormalizedMeasure", XmlConvert.ToString(obj.MinimumNormalizedMeasure));
            }

            int writeSatisfiedStatusCount = 0;
            int writeNormalizedMeasureCount = 0;
            foreach(SequencingObjectiveMapNodeReader map in obj.Mappings)
            {
                if(map.ReadSatisfiedStatus)
                {
                    writer.WriteAttributeString("readSatisfiedStatus", map.TargetObjectiveId);
                }
                if(map.ReadNormalizedMeasure)
                {
                    writer.WriteAttributeString("readNormalizedMeasure", map.TargetObjectiveId);
                }
                if(map.WriteSatisfiedStatus)
                {
                    writer.WriteAttributeString(String.Format(CultureInfo.InvariantCulture, "writeSatisfiedStatus{0}", writeSatisfiedStatusCount++), map.TargetObjectiveId);
                }
                if(map.WriteNormalizedMeasure)
                {
                    writer.WriteAttributeString(String.Format(CultureInfo.InvariantCulture, "writeNormalizedMeasure{0}", writeNormalizedMeasureCount++), map.TargetObjectiveId);
                }
            }
            if(writeSatisfiedStatusCount > 0)
            {
                writer.WriteAttributeString("writeSatisfiedStatusCount", XmlConvert.ToString(writeSatisfiedStatusCount));
            }
            if(writeNormalizedMeasureCount > 0)
            {
                writer.WriteAttributeString("writeNormalizedMeasureCount", XmlConvert.ToString(writeNormalizedMeasureCount));
            }

            writer.WriteElementString("score", String.Empty);
            writer.WriteElementString("extensionData", String.Empty);
            writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// Retrieves the LearningDataModel's dynamic xml data, so that it may be persisted.
        /// </summary>
        /// <param name="sequencingData">Dynamic XML data necessary for sequencing.</param>
        /// <param name="dynamicData">Dynamic XML data not necessary for sequencing.</param>
        internal void Export(out XPathNavigator sequencingData, out XPathNavigator dynamicData)
        {
            sequencingData = m_sequencingData.Clone();
            dynamicData = m_dynamicData.Clone();
        }

        /// <summary>
        /// Gets the delegate called to wrap a byte[] array with the interfaces required internally for attachments.
        /// </summary>
        /// <remarks>
        /// This delegate is called when a new attachment (i.e. a byte[] array) is assigned to
        /// extension data.  A wrapper class must be generated what supports IAttachment.  A new 
        /// Guid should be generated for the attachment at this time in order to be returned 
        /// from IAttachment.
        /// </remarks>
        internal WrapAttachmentDelegate WrapAttachment
        {
            get
            {
                return m_wrapAttachment;
            }
        }

        /// <summary>
        /// Gets the delegate called to wrap a <c>Guid</c> with the interfaces required internally for attachments.
        /// </summary>
        /// <remarks>
        /// This delegate is called when there is already an attachment referred to in the existing XML that will 
        /// initialize the extension data.  In this case only the GUID is saved within the xml, and it is up to the 
        /// wrapper class to be able to read from some source associated with that GUID and create a Stream from it.
        /// IAttachment must be supported by the wrapper class returned.
        /// </remarks>
        internal WrapAttachmentGuidDelegate WrapAttachmentGuid
        {
            get
            {
                return m_wrapAttachmentGuid;
            }
        }

        /// <summary>
        /// The delegate called when data has been changed.
        /// </summary>
        /// <remarks>
        /// This delegate, if set, is called every time any data is added, removed, or altered in the
        /// data model.  The exact information of what was changed is not conveyed, only the fact that
        /// a change has occurred.
        /// </remarks>
        internal DataChangeDelegate DataChange
        {
            get
            {
                return m_dataChange;
            }
            set
            {
                m_dataChange = value;
            }
        }

        /// <summary>
        /// Meant to be called by internal classes when data changes, this merely calls the <c>m_dataChange</c> delegate
        /// associated with this LearningDataStore.
        /// </summary>
        internal void CallDataChanged()
        {
            if(m_dataChange != null)
            {
                m_dataChange();
            }
        }

        /// <summary>
        /// Creates a <Typ>Comment</Typ> object, which may then be used to be added to the
        /// <Prp>CommentsFromLearner</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new comment and adding it to the
		/// <Prp>CommentsFromLearner</Prp> collection.
		/// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// Comment c = session.CurrentActivityDataModel.CreateComment();
        /// c.Location = "Question #3";
        /// c.CommentText = "I don't understand this question.";
        /// c.Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");
        /// session.CurrentActivityDataModel.CommentsFromLearner.Add(c);
        /// </code>
		/// </example>
        /// <returns>A new Comment object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public Comment CreateComment()
        {
            // Create a <comment/> xml block
            return new Comment(this, DataModelUtilities.CreateNavigator("comment"), false, false);
        }

        /// <summary>
        /// Creates a <Typ>Interaction</Typ> object, which may then be used to be added to
        /// the <Prp>Interactions</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new interaction and adding it to the
		/// <Prp>Interactions</Prp> collection.
        /// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// Interaction i = session.CurrentActivityDataModel.CreateInteraction();
        /// i.Id = "interaction1"; // A unique identifier is required before this is added to the list
        /// i.InteractionType = InteractionType.TrueFalse;
        /// session.CurrentActivityDataModel.Interactions.Add(i);
        /// </code></example>
        /// <returns>A new <Typ>Interaction</Typ> object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public Interaction CreateInteraction()
        {
            // Create a <interaction><objectives/><score/><evaluation/><correctResponses/><result/><extensionData/><rubrics/></interaction> xml block
            return new Interaction(this, DataModelUtilities.CreateNavigator("interaction", "objectives", "score", 
                "evaluation", "correctResponses", "result", "extensionData", "rubrics"), false, false);
        }

        /// <summary>
        /// Creates a <Typ>Objective</Typ> object, which may then be used to be added to
        /// the <Prp>Objectives</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new objective and adding it to the
		/// <Prp>Objectives</Prp> collection.
        /// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// Objective o = session.CurrentActivityDataModel.CreateObjective();
        /// o.Id = "objective1"; // A unique identifier is required before this is added to the list
        /// o.Description = "a description of objective 1";
        /// session.CurrentActivityDataModel.Objectives.Add(o);
        /// </code>
        /// </example>
        /// <returns>A new <Typ>Objective</Typ> object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public Objective CreateObjective()
        {
            // Create a <objective><score/><extensionData/></objective> xml block
            return new Objective(this, DataModelUtilities.CreateNavigator("objective", "score", "extensionData"), false, false);
        }

        /// <summary>
        /// Creates a <Typ>Rubric</Typ> object, which may then be used to be added to
        /// an <Prp>Interaction.Rubrics</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new rubric and adding it to an
		/// <Prp>Interaction.Rubrics</Prp> collection.
        /// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// Interaction i = session.CurrentActivityDataModel.CreateInteraction();
        /// i.Id = "interaction1"; // A unique identifier is required before this is added to the list
        /// i.InteractionType = InteractionType.TrueFalse;
        /// Rubric r = session.CurrentActivityDataModel.CreateRubric();
        /// r.Points = 3;
        /// i.Rubrics.Add(r);
        /// session.CurrentActivityDataModel.Interactions.Add(i);
        /// </code>
        /// </example>
        /// <returns>A new <Typ>Rubric</Typ> object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public Rubric CreateRubric()
        {
            // Create a <rubric></rubric> xml block
            return new Rubric(this, DataModelUtilities.CreateNavigator("rubric"), false, false);
        }

        /// <summary>
        /// Creates a <Typ>InteractionObjective</Typ> object, which may then be used to be added to
        /// the <Prp>../Interaction.Objectives</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new interaction objective and adding it to the
		/// <Prp>../Interaction.Objectives</Prp> collection.
        /// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// InteractionObjective o = session.CurrentActivityDataModel.CreateInteractionObjective();
        /// o.Id = "objective1"; // A unique identifier is required before this is added to the list
        /// session.CurrentActivityDataModel.Interactions[0].Objectives.Add(o);
        /// </code></example>
        /// <returns>A new <Typ>InteractionObjective</Typ> object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public InteractionObjective CreateInteractionObjective()
        {
            // Create a <objective/> xml block
            return new InteractionObjective(this, DataModelUtilities.CreateNavigator("objective"), false, false);
        }

        /// <summary>
        /// Creates a <c>CorrectResponse</c> object, which may then be used to be added to
        /// the <Prp>../Interaction.CorrectResponses</Prp> list.
        /// </summary>
        /// <example>
		/// The following example demonstrates creating a new correct response and adding it to the
		/// <Prp>../Interaction.CorrectResponses</Prp> collection.
        /// <code>
		/// LearningSession session = ... ; // get LearningSession object
		///
        /// CorrectResponse c = session.CurrentActivityDataModel.CreateCorrectResponse();
        /// c.Pattern = "true";
        /// session.CurrentActivityDataModel.Interactions[0].CorrectResponses.Add(c);
        /// </code></example>
        /// <returns>A new <Typ>CorrectResponse</Typ> object associated with this <Typ>LearningDataModel</Typ>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public CorrectResponse CreateCorrectResponse()
        {
            // Create a <correctResponse/> xml block
            return new CorrectResponse(this, DataModelUtilities.CreateNavigator("correctResponse"), false, false);
        }

        /// <summary>
        /// Gets format of the package associated with this data model
        /// </summary>
        public PackageFormat Format
        {
            get
            {
                return m_format;
            }
        }

        /// <summary>
        /// Gets the list of comments from the learner about the learning experience.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_learner.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.comments.</para>
        /// <para>
        /// Each activity has the option to store any number of comments from the learner, on a per 
        /// attempt basis.  These comments consist of freeform text and an optional timestamp and 
        /// location information.
		/// </para>
        /// </remarks>
        public Collection<Comment> CommentsFromLearner
        {
            get
            {
                return new Collection<Comment>(m_commentsFromLearner);
            }
        }

        /// <summary>
        /// Gets the list of comments generated by the LMS intended to be seen by all learners.  
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_lms.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.comments_from_lms.</para>
        /// <para>
        /// Similar in structure and concept to the <Typ>CommentsFromLearner</Typ>, however these 
        /// comments are not writable at runtime and are not stored on a per-attempt basis but apply
        /// to all attempts on a particular activity.
        /// </para>
        /// <para>
        /// There is no specific API to set these comments, they must be written to LearningStore
        /// directly.  Each time an attempt starts on an activity the appropriate rows for this 
        /// activity are read from LearningStore and this list is populated.
        /// </para>
        /// </remarks>
        public ReadOnlyCollection<CommentFromLms> CommentsFromLms
        {
            get
            {
                return new ReadOnlyCollection<CommentFromLms>(m_commentsFromLms);
            }
        }

        /// <summary>
        /// Gets or sets whether the learner has completed the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.completion_status.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This value indicates whether or not the learner has completed the current
        /// RLO.  This value may or may not correspond to the value actually set,
        /// but may be determined from the <Typ>ProgressMeasure</Typ> and/or <Typ>CompletionThreshold</Typ>
        /// values, as determined by the table in the SCORM 2004 RTE documentation, table
        /// "4.2.4.1a: Completion Status Determination".
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the CompletionStatus enumerator is passed.</exception>
        public CompletionStatus CompletionStatus
        {
            get
            {
                //return PrimaryObjective.CompletionStatus;
                CompletionStatus status = DataModelUtilities.GetEnumAttribute<CompletionStatus>(m_sequencingData, "completionStatus", CompletionStatus.Unknown).Value;
                if(ProgressMeasure == 1.0)
                {
                    return CompletionStatus.Completed;
                }
                else if(ProgressMeasure == 0.0)
                {
                    return CompletionStatus.NotAttempted;
                }
                else if(!ProgressMeasure.HasValue || !CompletionThreshold.HasValue)
                {
                    return status;
                }
                else if(ProgressMeasure >= CompletionThreshold)
                {
                    return CompletionStatus.Completed;
                }
                return CompletionStatus.Incomplete;
            }
            set
            {
                //PrimaryObjective.CompletionStatus = value;
                CheckIfWriteIsAllowed();
                m_verifier.ValidateCompletionStatus(value);
                DataModelUtilities.SetEnumAttribute<CompletionStatus>(m_sequencingData, "completionStatus", value);
                //if(PackageFormat == PackageFormat.V1p3)
                //{
                //    switch (value)
                //    {
                //    case CompletionStatus.Unknown:
                //        SetAttemptProgressStatus(false, true);
                //        break;
                //    case CompletionStatus.Completed:
                //        SetAttemptProgressStatus(true, true);
                //        AttemptCompletionStatus = true;
                //        break;
                //    case CompletionStatus.Incomplete:
                //    case CompletionStatus.NotAttempted:
                //        SetAttemptProgressStatus(true, true);
                //        AttemptCompletionStatus = false;
                //        break;
                //    }
                //}
                CallDataChanged();
            }
        }

        /// <summary>
        /// Gets or sets the entry mode for this RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.entry.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.entry.</para>
        /// <para>
        /// If this RLO has not yet been accessed, this should be EntryMode.AbInitio.  Otherwise, this
        /// value is dependant on how previous attempts on this activity were exited.
        /// </para>
        /// <para>
        /// This value is set automatically as part of the sequencing process, so it should not ever be necessary to
        /// set this property manually, although this is permitted.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the EntryMode enumerator is passed.</exception>
        public EntryMode Entry
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<EntryMode>(m_sequencingData, "entry", EntryMode.AbInitio).Value;
            }
            set
            {
                CheckIfWriteIsAllowed();
                m_verifier.ValidateEntry(value);
                DataModelUtilities.SetEnumAttribute<EntryMode>(m_sequencingData, "entry", value);
                CallDataChanged();
            }
        }

        /// <summary>
        /// Initializes necessary data model elements for delivery as required by SCORM, after the
        /// activity was suspended
        /// </summary>
        internal void InitializeForDeliveryAfterSuspend()
        {
            bool save = AdvancedAccess;
            AdvancedAccess = true;
            if(m_format != PackageFormat.V1p3 && NavigationRequest.ExitMode == ExitMode.Logout)
            {
                // for SCORM 1.2 (and LRM), if the previous attempt on this activity exited with
                // cmi.core.exit = "logout", then the data needs to be preserved yet the entry mode
                // must not be "resume", so set it here to "" (AllOtherConditions).
                Entry = EntryMode.AllOtherConditions;
            }
            else
            {
                Entry = EntryMode.Resume;
            }
            AdvancedAccess = save;
            DataModelUtilities.SetAttribute(m_navigationRequest.Navigator, "exit", null); // REQ_63.3
        }

        /// <summary>
        /// Initializes necessary data model elements for delivery as required by SCORM.
        /// </summary>
        internal void InitializeForDelivery()
        {
            bool save = AdvancedAccess;
            AdvancedAccess = true;
            if(m_format == PackageFormat.V1p3 || ActivityAttemptCount == 1)
            {
                Entry = EntryMode.AbInitio;
            }
            else
            {
                Entry = EntryMode.AllOtherConditions;
            }
            m_commentsFromLearner.Clear();
            m_extensionData.Clear();
            m_interactions.Clear();
            EvaluationPoints = null;  // set property so that updatescore will be called.
            AdvancedAccess = save;

            DataModelUtilities.SetAttribute(Score.Navigator, "scaled", null); // inferred
            DataModelUtilities.SetAttribute(m_sequencingData, "successStatus", null); // REQ_72.5.3
            DataModelUtilities.SetAttribute(m_sequencingData, "progressMeasure", null); //inferred
            DataModelUtilities.SetAttribute(m_navigationRequest.Navigator, "exit", null); // REQ_63.3

            // delete all objectives
            XPathNavigator nav = m_sequencingData.SelectSingleNode("/item/objectives");
            XPathNavigator child = nav.Clone();
            if(child.MoveToFirstChild())
            {
                child.DeleteRange(nav.SelectSingleNode("objective[last()]"));
            }
            // then re-add the ones from the manifest (if any)
            CopyStaticObjectives();
            m_objectives = new DataModelKeyedList<Objective>(this, m_sequencingData.SelectSingleNode("objectives").CreateNavigator(), "objective", "id", true,
                delegate(LearningDataModel dataModel, XPathNavigator _nav, bool isInDataModel)
                {
                    return new Objective(dataModel, _nav, isInDataModel, true);
                });
            m_primaryObjective = null;

            // other data model variables inferred to be cleared, but not specifically mentioned in the spec
            DataModelUtilities.SetAttribute(Score.Navigator, "maximum", null);
            DataModelUtilities.SetAttribute(Score.Navigator, "minimum", null);
            DataModelUtilities.SetAttribute(Score.Navigator, "raw", null);
            DataModelUtilities.SetAttribute(m_navigationRequest.Navigator, "command", null);
            DataModelUtilities.SetAttribute(m_navigationRequest.Navigator, "destination", null);
            if(m_format == PackageFormat.V1p3)
            {
                DataModelUtilities.SetAttribute(m_dynamicData, "suspendData", null);
                DataModelUtilities.SetAttribute(m_sequencingData, "totalTime", null);  // contrary to the spec (IMO), but it's what ADL says to do
            }
            nav = m_dynamicData.SelectSingleNode("learner");
            DataModelUtilities.SetAttribute(nav, "language", null);
            DataModelUtilities.SetAttribute(nav, "audioCaptioning", null);
            DataModelUtilities.SetAttribute(nav, "audioLevel", null);
            DataModelUtilities.SetAttribute(nav, "deliverySpeed", null);
            m_learner = new Learner(this, nav, m_learnerId, m_learnerName, m_learnerLanguage, m_learnerCaption, m_learnerAudioLevel, m_learnerDeliverySpeed);

            CallDataChanged();
        }

        /// <summary>
        /// Clears attempt progress information for the current attempt, used by sequencing when 
        /// useCurrentAttemptProgressInfo is true.
        /// </summary>
        internal void ClearAttemptProgressInfo()
        {
            DataModelUtilities.SetAttribute(m_sequencingData, "completionStatus", null); //REQ_59.3
            CallDataChanged();
        }

        /// <summary>
        /// Clears attempt objective information for the current attempt, used by sequencing when 
        /// useCurrentAttemptProgressInfo is true.
        /// </summary>
        internal void ClearAttemptObjectiveInfo()
        {
            foreach(Objective obj in Objectives)
            {
                DataModelUtilities.SetAttribute(obj.Score.Navigator, "scaled", null); // inferred
                DataModelUtilities.SetAttribute(obj.Navigator, "successStatus", null); // REQ_72.5.3
            }
            DataModelUtilities.SetAttribute(m_sequencingData, "successStatus", null);
            CallDataChanged();
        }

        /// <summary>
        /// Gets the threshold which indicates whether the RLO should be considered complete.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.completion_threshold.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This value may be used to determine whether the RLO is considered complete.  See the 
        /// description of <Typ>CompletionStatus</Typ>.  This value is initialized from the manifest
        /// and may not be changed.
        /// </para>
        /// </remarks>
        public float? CompletionThreshold
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_staticData, "completionThreshold", null);
            }
        }

        /// <summary>
        /// Gets the threshold which indicates whether the RLO should be considered complete.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this is invalid.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_data.mastery_score.</para>
        /// </remarks>
        public float? MasteryScore
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_staticData, "masteryScore", null);
            }
        }

        /// <summary>
        /// Gets the credit setting for the data model.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.credit.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.credit.</para>
        /// <para>
        /// This value defaults to true, and may not be changed at run-time.  It may only be changed
        /// by altering the Credit column of the appropriate ActivityPackageItem.
        /// </para>
        /// </remarks>
        public bool Credit
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_dynamicData, "credit", true);
            }
            internal set
            {
                DataModelUtilities.SetAttribute(m_dynamicData, "credit", XmlConvert.ToString(value));
            }
        }

        /// <summary>
        /// Gets the list of responses to individual questions or tasks.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.</para>
        /// <para>
        /// The property represents a collection of individual questions and learner responses
        /// to those questions, keyed by a unique string identifier.  This data is set by
        /// the RLO and returned to the application.  Whether or not and how the application
        /// uses this information is undefined.
        /// </para>
        /// </remarks>
        public Collection<Interaction> Interactions
        {
            get
            {
                return new Collection<Interaction>(m_interactions);
            }
        }

        /// <summary>
        /// Determines whether or not the interactions list contains an element with the 
        /// passed identifier.
        /// </summary>
        /// <param name="interactionId">The identifier to search for within the interactions list.</param>
        /// <returns>True if one or more elements with the requested interaction identifier exists within
        /// the interactions list.</returns>
        public bool InteractionListContains(string interactionId)
        {
            foreach(Interaction inter in m_interactions)
            {
                if(inter.Id == interactionId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the first interaction element found with the requested identifier within 
        /// the interactions list.
        /// </summary>
        /// <param name="interactionId">The identifier to search for within the interactions list.</param>
        /// <returns>The first interaction element found within the interactions list with the requested 
        /// identifier, or null if no corresponding item is found.</returns>
        public Interaction InteractionListElement(string interactionId)
        {
            foreach(Interaction inter in m_interactions)
            {
                if(inter.Id == interactionId)
                {
                    return inter;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the data from the manifest intended to provide an RLO with some initial launch information.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.launch_data.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.launch_data.</para>
        /// <para>
        /// This data represents a string stored in the manifest that is passed to the RLO on each new
        /// attempt.  It may not be written to, and the format of the data is completely freeform.
        /// </para>
        /// <para>If the value has not been set before in the manifest, null is returned.</para>
        /// </remarks>
        public string LaunchData
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_staticData, "dataFromLMS", null);
            }
        }

        /// <summary>
        /// Gets learner information and learner preferences.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner*.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student*.</para>
        /// <para>
        /// This object identifies the learner and provides a means to store preferences for that 
        /// learner.  Learner preferences are stored globally and used to initialize these values, 
        /// and changes made to these values will never affect any other RLO, or any other attempt 
        /// on the current RLO.
        /// </para>
        /// </remarks>
        public Learner Learner
        {
            get
            {
                return m_learner;
            }
        }

        /// <summary>
        /// Gets or sets the current student status as determined by the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this is invalid.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.lesson_status.</para>
        /// <para>
        /// See SCORM 1.2 RTE document section 3.4.4.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to, 
        /// or if this is not a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the LessonStatus enumerator is passed.</exception>
        public LessonStatus LessonStatus
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<LessonStatus>(m_sequencingData, "lessonStatus", LessonStatus.NotAttempted).Value;
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(LessonStatus != value)
                {
                    m_verifier.ValidateLessonStatus(value);
                    DataModelUtilities.SetEnumAttribute<LessonStatus>(m_sequencingData, "lessonStatus", value);
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets location information which may be used as a bookmark within a RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.location.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.lesson_location.</para>
        /// <para>
        /// A string designed to be opaque to the application, for use only by the RLO.  This may be used, for
        /// example, as a bookmark to return to a specific location after the RLO has been suspended and
        /// resumed.
        /// </para>
        /// <para>
        /// For SCORM 2004 packages, this string is limited to a length defined by 
        /// BaseSchemaInternal.ActivityAttemptItem.MaxLocationLength.  For SCORM 1.2 packages, this
        /// string is limited to 255 characters.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is too long.</exception>
        public string Location
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_dynamicData, "location", null);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(Location != value)
                {
                    m_verifier.ValidateLocation(value);
                    DataModelUtilities.SetAttribute(m_dynamicData, "location", value);
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the maximum time allowed for this RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.max_time_allowed.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_data.max_time_allowed.</para>
        /// <para>
        /// This value corresponds to the amount of time the learner is allowed to use an RLO in this attempt.  Automatic 
        /// checking and validation of this limit is not supported by default.
        /// </para>
        /// </remarks>
        public TimeSpan? MaxTimeAllowed
        {
            get
            {
                if(m_format != PackageFormat.V1p3)
                {
                    return DataModelUtilities.GetNullableAttribute<TimeSpan>(m_staticData, "maxTimeAllowed", null);
                }
                XmlNamespaceManager ns = new XmlNamespaceManager(m_staticData.NameTable);
                ns.AddNamespace("imsss", "http://www.imsglobal.org/xsd/imsss");
                XPathNavigator nav = m_staticData.SelectSingleNode("/item/imsss:sequencing/imsss:limitConditions/@attemptAbsoluteDurationLimit", ns);
                if(nav == null)
                {
                    return null;
                }
                //return XmlConvert.ToTimeSpan(nav.Value);
                return Utilities.StringToTimeSpanScormV1p3(nav.Value);
            }
        }

        /// <summary>
        /// Gets the list of objectives for the activity, used to track RLO defined goals.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.objectives.</para>
        /// <para>
        /// This property corresponds to a list of objectives, keyed by unique identifer.  This list 
        /// is initialized originally from data in the manifest, but it may be added to or modified 
        /// at runtime by an RLO.
        /// </para>
        /// <para>
        /// Objectives contain many indicators to determine whether or not they have been satisfied
        /// and have optional scoring information as well.
        /// </para>
        /// </remarks>
        public KeyedCollection<string, Objective> Objectives
        {
            get
            {
                return m_objectives;
            }
        }

        /// <summary>
        /// Gets or sets the measure of the progress the learner has made toward completing the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.progress_measure.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property represents a measure of the progress made by the learner toward completion of the
        /// RLO.  Setting this value will affect the value of <Typ>CompletionStatus</Typ>.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is not a SCORM 2004 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is less than 0.0 or greater than 1.0.</exception>
        public float? ProgressMeasure
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_sequencingData, "progressMeasure", null);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(ProgressMeasure != value)
                {
                    m_verifier.ValidateProgressMeasure(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_sequencingData, "progressMeasure", s);
                    CallDataChanged();
                }
                // Setting cmi.progress_measure is akin to the SCO "communicating completion information" to satisfy
                // SN 3.13.2, therefore set progress status to true.
                //SetAttemptProgressStatus(true, true);
            }
        }

        /// <summary>
        /// Gets the scaled score required to master the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.scaled_passing_score.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property represents the score required to successfully complete the RLO.  This value 
        /// affects the value of <Typ>SuccessStatus</Typ>.  The value originates in the manifest and must
        /// be scaled to a value from -1.0 to 1.0 inclusive.
        /// </para>
        /// </remarks>
        public float? ScaledPassingScore
        {
            get
            {
                XmlNamespaceManager ns = new XmlNamespaceManager(m_staticData.NameTable);
                ns.AddNamespace("imsss", "http://www.imsglobal.org/xsd/imsss");
                XPathNavigator nav = m_staticData.SelectSingleNode("/item/imsss:sequencing/imsss:objectives/imsss:primaryObjective", ns);
                if(nav == null || !DataModelUtilities.GetAttribute<bool>(nav, "satisfiedByMeasure", false))
                {
                    return null;
                }
                nav = m_staticData.SelectSingleNode("/item/imsss:sequencing/imsss:objectives/imsss:primaryObjective/imsss:minNormalizedMeasure", ns);
                if(nav == null)
                {
                    return 1.0f;
                }
                return (float)nav.ValueAsDouble;
            }
        }

        /// <summary>
        /// Gets the score for the data model.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.score.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.score.</para>
        /// <para>
        /// Setting the Score.Scaled property will affect the value of <Typ>SuccessStatus</Typ>.  Also, setting any of these values will 
        /// alter the corresponding values on the primary objective, as defined within the manifest.
        /// </para>
        /// </remarks>
        public Score Score
        {
            get
            {
                return m_score;
            }
        }

        /// <summary>
        /// Gets or sets whether the learner has mastered the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.success_status.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This value indicates whether or not the learner has succeeded in current
        /// RLO.  This value may or may not correspond to the value actually set,
        /// but may be determined from the <Typ>Score.Scaled</Typ> and/or <Typ>ScaledPassingScore</Typ>
        /// values, as determined by the table in the SCORM 2004 RTE documentation, table
        /// "4.2.22.1a: Success Status Determination".
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is not a SCORM 2004 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is not a valid value for the SuccessStatus enumeration.</exception>
        public SuccessStatus SuccessStatus
        {
            get
            {
                SuccessStatus status = DataModelUtilities.GetEnumAttribute<SuccessStatus>(m_sequencingData, "successStatus", SuccessStatus.Unknown).Value;
                if(!ScaledPassingScore.HasValue)
                {
                    return status;
                }
                else if(!Score.Scaled.HasValue)
                {
                    return SuccessStatus.Unknown;
                }
                else if(Score.Scaled >= ScaledPassingScore)
                {
                    return SuccessStatus.Passed;
                }
                return SuccessStatus.Failed;
            }
            set
            {
                CheckIfWriteIsAllowed();
                m_verifier.ValidateSuccessStatus(value);
                DataModelUtilities.SetEnumAttribute<SuccessStatus>(m_sequencingData, "successStatus", value);
                CallDataChanged();
            }
        }

        // Tracking model variables

        /// <summary>
        /// Cached primary objective
        /// </summary>
        private Objective m_primaryObjective;

        /// <summary>
        /// Gets the primary objective associated with this data model
        /// </summary>
        internal Objective PrimaryObjective
        {
            get
            {
                Utilities.Assert(m_objectives.Count > 0, "LDM1004");
                if(m_primaryObjective == null)
                {
                    foreach(Objective obj in m_objectives)
                    {
                        if(obj.IsPrimaryObjective)
                        {
                            m_primaryObjective = obj;
                            break;
                        }
                    }
                }
                Utilities.Assert(m_primaryObjective != null, "LDM1005");
                return m_primaryObjective;
            }
        }

        /// <summary>
        /// Gets or sets data used to persist the current RLO state during a suspension.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.suspend_data.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.suspend_data.</para>
        /// <para>
        /// A string designed to be opaque to the application, for use only by the RLO.  This may 
        /// be used to store any data necessary to restart the RLO after a suspend operation.
        /// </para>
        /// <para>If the value has not been set before being accessed, null is returned.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.ActivityAttemptItem.MaxSuspendDataLength characters in SCORM 2004 packages,
        /// or longer than 4096 characters in SCORM 1.2 packages.</exception>
        public string SuspendData
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_dynamicData, "suspendData", null);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(SuspendData != value)
                {
                    m_verifier.ValidateSuspendData(value);
                    DataModelUtilities.SetAttribute(m_dynamicData, "suspendData", value);
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the action to take if <c>MaxTimeAllowed</c> is exceeded.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.time_limit_action.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_data.time_limit_action.</para>
        /// <para>
        /// Indicates what the RLO should do upon a timeout.  If this value is not specified within the
        /// manifest, the default value of TimeLimitAction.ContinueNoMessage is returned.
        /// </para>
        /// </remarks>
        public TimeLimitAction TimeLimitAction
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<TimeLimitAction>(m_staticData, "timeLimitAction", TimeLimitAction.ContinueNoMessage).Value;
            }
        }

        /// <summary>
        /// Gets or sets the sum of all time spent in this RLO prior to the current session.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.total_time.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.total_time.</para>
        /// <para>
        /// This value represents the sum total of all the learner's session times (<Typ>SessionTime</Typ>) 
        /// accumulated for prior attempts on this RLO within the context of this learner attempt.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public TimeSpan TotalTime
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "totalTime", TimeSpan.Zero);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(TotalTime != value)
                {
                    m_verifier.ValidateTotalTime(value);
                    DataModelUtilities.SetAttribute(m_sequencingData, "totalTime", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the session time for the current session of the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.session_time.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.session_time.</para>
        /// <para>
        /// This value represents the amount of time spent in the current attempt on the RLO.  The RLO
        /// is responsible for setting this value, this is never automatically generated.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public TimeSpan SessionTime
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "sessionTime", TimeSpan.Zero);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(SessionTime != value)
                {
                    m_verifier.ValidateSessionTime(value);
                    DataModelUtilities.SetAttribute(m_sequencingData, "sessionTime", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the points assigned by autograding or by the instructor for this activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is infinity or not a number.</exception>
        public float? EvaluationPoints
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_sequencingData, "evaluationPoints", null);
            }
            set
            {
                CheckIfWriteIsAllowed();
                if(EvaluationPoints != value)
                {
                    float? old = DataModelUtilities.GetNullableAttribute<float>(m_sequencingData, "evaluationPoints", null);
                    m_verifier.ValidateEvaluationPoints(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_sequencingData, "evaluationPoints", s);
                    if((m_format == PackageFormat.Lrm || m_format == PackageFormat.V1p2) && m_updateScore != null)
                    {
                        m_updateScore(old, value);
                    }
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the navigation request.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to adl.nav.request and cmi.exit.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.exit.</para>
        /// </remarks>
        public NavigationRequest NavigationRequest
        {
            get
            {
                return m_navigationRequest;
            }
        }

        /// <summary>
        /// Gets the extension data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Extension data does not correspond to any SCORM defined variables.  It is used to store information
        /// on a per activity basis that cannot be stored in SCORM specifc variables.
        /// </para>
        /// <para>
        /// Only certain types of data are valid within the extension data.  These include <Typ>Boolean</Typ>, 
        /// <Typ>Int32</Typ>, <Typ>DateTime</Typ>, <Typ>Double</Typ>, <Typ>String</Typ>, and <Typ>byte</Typ>[] array.
        /// If a <Typ>Single</Typ> is passed, this will automatically be converted to a <Typ>Double</Typ>.
        /// </para>
        /// <para>
        /// All keys into this dictionary are unique, and if the same key is used twice it will overwrite the value 
        /// and possibly the type information associated with that element.  SCORM naming conventions are recommended
        /// but not required for key (i.e. variable) names.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // assigns a boolean value of true to "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = true;
        /// 
        /// // assigns a double value of 3.1415927 to "foo.bar.something"
        /// // this changes the type as well as the value of the variable "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = 3.1415927
        /// </code>
        /// <code>
        /// byte[] attachment = ReadFileAsByteArray("c:\\assignment.doc");
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.attachment"] = attachment;
        /// </code>
        /// </example>
        public IDictionary<string, object> ExtensionData
        {
            get
            {
                return m_extensionData;
            }
        }

        // Tracking model variables

        /// <summary>
        /// Gets or sets whether the activity progress information is meaningful for the activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <remarks>Always returns <c>false</c> for activities that are not tracked.</remarks>
        internal bool ActivityProgressStatus
        {
            get
            {
                if (Tracked)
                {
                    return DataModelUtilities.GetAttribute<bool>(m_sequencingData, "activityProgressStatus", false);
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if(ActivityProgressStatus != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityProgressStatus", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the cumulative duration of all attempts on this activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal TimeSpan ActivityAbsoluteDuration
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "activityAbsoluteDuration", TimeSpan.MinValue);
            }
            set
            {
                if(ActivityAbsoluteDuration != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityAbsoluteDuration", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the cumulative duration of all attempts on this activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal TimeSpan ActivityExperiencedDuration
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "activityExperiencedDuration", TimeSpan.MinValue);
            }
            set
            {
                if(ActivityAbsoluteDuration != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityExperiencedDuration", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the number of attempts on the activity.
        /// </summary>
        /// <remarks>
        /// Returns the number of attempts on this activity, including the currently active attempt.  This
        /// number gets incremented each time a new attempt begins on an activity, before it is delivered
        /// for execution.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public int ActivityAttemptCount
        {
            get
            {
                return DataModelUtilities.GetAttribute<int>(m_sequencingData, "activityAttemptCount", 0);
            }
            internal set
            {
                if(ActivityAttemptCount != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityAttemptCount", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets an indication of whether the attempt progress information is meaningful.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <remarks>Always returns <c>false</c> for activities that are not tracked.
        /// <para>When <c>CompletionSetByContent == true</c>, setting this property will have no affect.</para>
        /// </remarks>
        internal bool AttemptProgressStatus
        {
            get
            {
                if(Tracked)
                {
                    //return DataModelUtilities.GetAttribute<bool>(m_sequencingData, "attemptProgressStatus", false);
                    return (CompletionStatus != CompletionStatus.Unknown);
                }
                else
                {
                    return false;
                }
            }
            //set
            //{
            //    SetAttemptProgressStatus(value, false);
            //}
        }

        /// <summary>
        /// Gets or sets an indication of whether the attempt is completed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal bool AttemptCompletionStatus
        {
            get
            {
                return (CompletionStatus == CompletionStatus.Completed);
            }
        }

        /// <summary>
        /// Gets or sets the duration of this attempt on this activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal TimeSpan AttemptAbsoluteDuration
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "attemptAbsoluteDuration", TimeSpan.MinValue);
            }
            set
            {
                if(AttemptAbsoluteDuration != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "attemptAbsoluteDuration", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the duration of this attempt on this activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal TimeSpan AttemptExperiencedDuration
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_sequencingData, "attemptExperiencedDuration", TimeSpan.MinValue);
            }
            set
            {
                if(AttemptExperiencedDuration != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "attemptExperiencedDuration", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets an indication of whether the activity is active.
        /// </summary>
        internal bool ActivityIsActive
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_sequencingData, "activityIsActive", false);
            }
            set
            {
                if(ActivityIsActive != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityIsActive", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets an indication of whether the activity is suspended.
        /// </summary>
        internal bool ActivityIsSuspended
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_sequencingData, "activityIsSuspended", false);
            }
            set
            {
                if(ActivityIsSuspended != value)
                {
                    DataModelUtilities.SetAttribute(m_sequencingData, "activityIsSuspended", XmlConvert.ToString(value));
                    CallDataChanged();
                }
            }
        }
    }

    /// <summary>
    /// Represents a comment from the learner.
    /// </summary>
    /// <remarks>
    /// <para>In SCORM 2004, this corresponds to cmi.comments_from_learner.</para>
    /// <para>In SCORM 1.2, this corresponds to cmi.comments.</para>
    /// <para>
    /// See <Prp>LearningDataModel.CommentsFromLearner</Prp> for more information about the usage of this class.
    /// </para>
    /// </remarks>
    public class Comment : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;comment&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private CommentVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new Comment object that is already part of the data model.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;comment&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal Comment(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new CommentVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new CommentVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new CommentVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
        }

        /// <summary>
        /// Gets or sets the textual comment itself.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_learner.n.comment.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.comments.</para>
        /// <para>
        /// Freeform text defined by the RLO, indicating a comment from the learner.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is greater than BaseSchemaInternal.CommentFromLearnerItem.MaxCommentLength for SCORM 2004 packages, 
        /// or greater than 4096 characters for SCORM 1.2 packages.</exception>
        public string CommentText
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "text", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != CommentText)
                {
                    m_verifier.ValidateComment(value);
                    DataModelUtilities.SetAttribute(m_nav, "text", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the point in the RLO to which the comment applies.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_learner.n.location.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property represents an RLO defined location string, perhaps as a bookmark to indicate
        /// where the comment belongs.  The format of this string is freeform and its use is optional.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to, or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is greater than BaseSchemaInternal.CommentFromLearnerItem.MaxLocationLength</exception>
        public string Location
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "location", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != Location)
                {
                    m_verifier.ValidateLocation(value);
                    DataModelUtilities.SetAttribute(m_nav, "location", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the point in time at which the comment was most recently changed.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_learner.n.timestamp.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property indicates when the comment was created or most recently changed.
        /// </para>
        /// <para>
        /// This field is a string in the same format as the SCORM specification for timestamps.  This 
        /// may be achieved easily within C# by code as shown in the example.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        ///     // this saves the timestamp with the current timezone information
        ///     session.CurrentActivityDataModel.CommentsFromLearner[0].Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzzz");
        /// 
        ///     // this saves a UTC timestamp, which is recommended to avoid issues with timezones
        ///     session.CurrentActivityDataModel.CommentsFromLearner[0].Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");
        /// 
        ///     // this saves timestamp with no specific timezone information
        ///     session.CurrentActivityDataModel.CommentsFromLearner[0].Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        /// </code>
        /// <para>If the value has not been set before it is accessed, null is returned.</para>
        /// </example>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to, or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the year is less than 1970 or greater than 2038.</exception>
        /// <exception cref="FormatException">Thrown if the passed string is not in the correct format.</exception>
        public string Timestamp
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "timestamp", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != Timestamp)
                {
                    m_verifier.ValidateTimeStamp(value);
                    DataModelUtilities.SetAttribute(m_nav, "timestamp", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// This property is unsupported for this class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                throw new LearningComponentsInternalException("LDM4000");
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }

        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = true;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }
    }

    /// <summary>
    /// Represents a read-only comment from the LMS.
    /// </summary>
    /// <remarks>
    /// <para>In SCORM 2004, this corresponds to cmi.comments_from_lms.</para>
    /// <para>In SCORM 1.2, this corresponds to cmi.comments_from_lms.</para>
    /// <para>
    /// See <Prp>LearningDataModel.CommentsFromLms</Prp> for more information about the usage of this class.
    /// </para>
    /// </remarks>
    public class CommentFromLms : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;comment&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// Initializes a new CommentFromLms object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;comment&gt; element.</param>
        internal CommentFromLms(LearningDataModel dataModel, XPathNavigator nav)
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
        }

        /// <summary>
        /// Gets the textual comment itself.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_lms.n.comment.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.comments_from.learner.</para>
        /// <para>
        /// Freeform text describing a comment that applies to all attempts on this activity.
        /// </para>
        /// </remarks>
        public string CommentText
        {
            get
            {
                return m_nav.Value;
            }
        }

        /// <summary>
        /// Gets the point in the RLO to which the comment applies.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_lms.n.location.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property represents an RLO defined location string, perhaps as a bookmark to indicate
        /// where the comment belongs.  The format of this string is freeform and its use is optional.
        /// </para>
        /// <para>If the value has not been set, null is returned.</para>
        /// </remarks>
        public string Location
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "location", null);
            }
        }

        /// <summary>
        /// Gets the point in time at which the comment was most recently changed.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.comments_from_lms.n.timestamp.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This value represents the point in time at which the comment was created or most recently changed.
        /// </para>
        /// <para>If the value has not been set, null is returned.</para>
        /// </remarks>
        public string Timestamp
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "timestamp", null);
            }
        }

        /// <summary>
        /// This property is unsupported for this class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                throw new LearningComponentsInternalException("LDM40003");
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }

        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// This value is always true for this class as the data model exposes this list as completely read-only.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return true;
            }
            set
            {
                throw new LearningComponentsInternalException("LDM40001");
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// This value is always true for this class as the data model exposes this list as completely read-only.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return true;
            }
            set
            {
                throw new LearningComponentsInternalException("LDM40002");
            }
        }
    }

    /// <summary>
    /// A class representing the learner data for the data model.
    /// </summary>
    /// <remarks>
    /// <para>In SCORM 2004, this corresponds to cmi.learner*.</para>
    /// <para>In SCORM 1.2, this corresponds to cmi.student*.</para>
    /// <para>
    /// This object identifies the learner and provides a means to store preferences for that 
    /// learner.  Learner preferences are stored globally and used to initialize these values, 
    /// and changes made to these values will never affect any other activity.
    /// </para>
    /// </remarks>
    public class Learner
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;learner&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private LearnerVerifier m_verifier;

        /// <summary>
        /// Unique identifier for this learner
        /// </summary>
        private string m_id;

        /// <summary>
        /// Learner name.
        /// </summary>
        private string m_name;

        /// <summary>
        /// default value for language
        /// </summary>
        private string m_language = String.Empty;

        /// <summary>
        /// default value for audio captioning
        /// </summary>
        private AudioCaptioning m_audioCaptioning = AudioCaptioning.NoChange;

        /// <summary>
        /// default value for audio level.
        /// </summary>
        private float m_audioLevel = (float)1.0;

        /// <summary>
        /// default level for delivery speed
        /// </summary>
        private float m_deliverySpeed = (float)1.0;

        /// <summary>
        /// Initializes a new Learner object.
        /// </summary>
        /// <remarks>
        /// All parameters for this constructor are assumed to be in compliant to SCORM 2004 ranges, whether or
        /// not the data model in question is actually SCORM 2004.  This is because this is the way they are stored within
        /// LearningStore.  If this is a SCORM 1.2 object, these values will be converted to appropriate values.
        /// </remarks>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;learner&gt; element.</param>
        /// <param name="learnerId">The unique identifier of the learner, as a URI.</param>
        /// <param name="learnerName">The name of the learner.</param>
        /// <param name="learnerLanguage">The language preference of the learner.</param>
        /// <param name="learnerCaption">The audio captioning preference of the learner.</param>
        /// <param name="learnerAudioLevel">The audio level preference of the learner.</param>
        /// <param name="learnerDeliverySpeed">The delivery speed preference of the learner.</param>
        internal Learner(LearningDataModel dataModel, XPathNavigator nav,
            string learnerId, string learnerName, string learnerLanguage, AudioCaptioning learnerCaption, float learnerAudioLevel, float learnerDeliverySpeed)
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new LearnerVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new LearnerVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new LearnerVerifierV1p3();
                break;
            }
            if(m_dataModel.Format == PackageFormat.V1p2 || m_dataModel.Format == PackageFormat.Lrm)
            {
                // translate SCORM 2004 audio level to SCORM 1.2 valid value
                if(learnerAudioLevel == 0f)
                {
                    learnerAudioLevel = -1f;
                }
                else if(learnerAudioLevel == 1f)
                {
                    learnerAudioLevel = 0f;
                }
                else
                {
                    // this translation is sorta bogus - it's not easily translatable to
                    // go from a ratio value from "1" to a 1 to 100 value, both of which are
                    // implementation dependant
                    learnerAudioLevel = (float)(int)(learnerAudioLevel * 10);
                    if(learnerAudioLevel > 100f)
                    {
                        learnerAudioLevel = 100f;
                    }
                }
                // since language in 1.2 is freeform text, don't convert from 2004 values.

                // translate SCORM 2004 delivery speed to SCORM 1.2 valid value
                learnerDeliverySpeed = (float)(int)((learnerDeliverySpeed - 1) * 100);
                if(learnerDeliverySpeed < -100f)
                {
                    learnerDeliverySpeed = -100f;
                }
                else if(learnerDeliverySpeed > 100f)
                {
                    learnerDeliverySpeed = 100f;
                }

                // AudioCaptioning does not need to be translated
            }
            m_id = learnerId;
            m_name = learnerName;

            // sets default values for these.  These are only written to xml if they are changed
            if(DataModelUtilities.GetAttribute<string>(m_nav, "language", null) == null)
            {
                DataModelUtilities.SetAttribute(m_nav, "language", learnerLanguage);
                m_language = learnerLanguage;
            }
            if(DataModelUtilities.GetEnumAttribute<AudioCaptioning>(m_nav, "audioCaptioning", null) == null)
            {
                DataModelUtilities.SetEnumAttribute<AudioCaptioning>(m_nav, "audioCaptioning", learnerCaption);
                m_audioCaptioning = learnerCaption;
            }
            if(DataModelUtilities.GetNullableAttribute<float>(m_nav, "audioLevel", null) == null)
            {
                DataModelUtilities.SetAttribute(m_nav, "audioLevel", XmlConvert.ToString(learnerAudioLevel));
                m_audioLevel = learnerAudioLevel;
            }
            if(DataModelUtilities.GetNullableAttribute<float>(m_nav, "deliverySpeed", null) == null)
            {
                DataModelUtilities.SetAttribute(m_nav, "deliverySpeed", XmlConvert.ToString(learnerDeliverySpeed));
                m_deliverySpeed = learnerDeliverySpeed;
            }
        }

        /// <summary>
        /// Gets an identifier that represents the learner uniquely.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_id.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.student_id.</para>
        /// <para>
        /// This property is a unique identifier for the user, defined by MLC.  Any properties other 
        /// than uniqueness are not guaranteed for the contents of the string.
        /// </para>
        /// </remarks>
        public string Id
        {
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Gets the name provided for the learner by the LMS.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_name.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.student_name.</para>
        /// </remarks>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Gets or sets the audio level preference of the learner.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_preference.audio_level.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_preference.audio.</para>
        /// <para>
        /// In SCORM 2004, this property is a multiplier value that specifies an intended change in perceived 
        /// audio volume level relative to an implementation-specific reference level where 1 indicates “no change”.  
        /// For example, the value 0 specifies "off", the value of 0.5 represents half volume and a value of 2
        /// indicates double volume.
        /// </para>
        /// <para>
        /// In SCORM 1.2, any negative number represents audio being "off", 0 indicates no change of status, and a number
        /// from 1 to 100 indicates a volume level from softest to loudest.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is less than 0.0 for SCORM 2004 packages, or if 
        /// it is not an integer or is less than -32768 or greater than 100 for SCORM 1.2 packages.</exception>
        public float AudioLevel
        {
            get
            {
                return DataModelUtilities.GetAttribute<float>(m_nav, "audioLevel", m_audioLevel);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(AudioLevel != value)
                {
                    m_verifier.ValidateAudioLevel(value);
                    DataModelUtilities.SetAttribute(m_nav, "audioLevel", XmlConvert.ToString(value));
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the learner's preferred language, for those RLO's with multi-language capability.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_preference.language.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_preference.language.</para>
        /// <para>In SCORM 2004, this value corresponds to a standard language code as per iso-646.</para>
        /// <para>In SCORM 1.2, this value is freeform text, e.g. "English", "French".</para>
        /// <para>In either case, an empty string indicates the use of the default system language.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longet than BaseSchemaInternal.UserItem.MaxLanguageLength 
        /// for SCORM 2004 packages, or longer than 255 characters for SCORM 1.2 packages.</exception>
        public string Language
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "language", m_language);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(Language != value)
                {
                    m_verifier.ValidateLanguage(value);
                    DataModelUtilities.SetAttribute(m_nav, "language", value);
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the delivery speed preference of the learner.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_preference.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_preference.speed.</para>
        /// <para>
        /// In SCORM 2004, this proeprty represents a multiplier that specifies the learner’s preferred 
        /// relative speed of content delivery expressed as a change in speed relative to an 
        /// implementation-specific reference speed. For example, 2 is twice as fast as 
        /// the reference speed and 0.5 is one half the reference speed. The default value is 1.
        /// </para>
        /// <para>
        /// In SCORM 1.2, this is a number from -100 to 100, where -100 is the slowest speed, 0 is no change, and
        /// 100 is the fastest speed.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if this valus is less than 0.0 for SCORM 2004 packages, or less than -100
        /// or greater than 100 for SCORM 1.2 packages.</exception>
        public float DeliverySpeed
        {
            get
            {
                return DataModelUtilities.GetAttribute<float>(m_nav, "deliverySpeed", m_deliverySpeed);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(DeliverySpeed != value)
                {
                    m_verifier.ValidateDeliverySpeed(value);
                    DataModelUtilities.SetAttribute(m_nav, "deliverySpeed", XmlConvert.ToString(value));
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether captioning text corresponding to audio is displayed.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.learner_preference.audio_captioning.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.student_preference.text.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the AudioCaptioning enumerator is passed.</exception>
        public AudioCaptioning AudioCaptioning
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<AudioCaptioning>(m_nav, "audioCaptioning", m_audioCaptioning).Value;
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(AudioCaptioning != value)
                {
                    m_verifier.ValidateAudioCaptioning(value);
                    DataModelUtilities.SetEnumAttribute<AudioCaptioning>(m_nav, "audioCaptioning", value);
                    m_dataModel.CallDataChanged();
                }
            }
        }
    }

    /// <summary>
    /// A class representing a single interaction for the data model.
    /// </summary>
    /// <remarks>
    /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.*.</para>
    /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.*.</para>
    /// <para>
    /// The interactions data model element defines a set of learner responses that can 
    /// be passed from the RLO to the application. Interactions are intended to be responses to 
    /// individual questions or tasks that the RLO developer wants to record. The application
    /// has no implied behavior for any data including within the Interaction class.
    /// </para>
    /// </remarks>
    public class Interaction : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;interaction&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private InteractionVerifier m_verifier;

        /// <summary>
        /// The score associated with this interaction
        /// </summary>
        private Score m_score;

        /// <summary>
        /// Represents an external evaluation of a learner's response to an interation.
        /// </summary>
        private Evaluation m_evaluation;

        /// <summary>
        /// Extension data associated with this interaction.
        /// </summary>
        private DataModelExtensionDictionary m_extensionData;

        /// <summary>
        /// A list of objective IDs associated with this interaction
        /// </summary>
        private DataModelList<InteractionObjective> m_objectives;

        /// <summary>
        /// A list of correct response patterns associated with this interaction.
        /// </summary>
        private DataModelList<CorrectResponse> m_correctResponses;

        /// <summary>
        /// A list of rubrics associated with this interaction.
        /// </summary>
        private DataModelList<Rubric> m_rubrics;

        /// <summary>
        /// A judgment of the correctness of the learner response.
        /// </summary>
        private InteractionResult m_result;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new Interaction object that already belongs to this data model.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;comment&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal Interaction(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new InteractionVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new InteractionVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new InteractionVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
            ResetNavigator();
        }

        /// <summary>
        /// Resets all internal data fields that are dependent upon the navigator, called when the navigator changes
        /// for any reason.
        /// </summary>
        private void ResetNavigator()
        {
            m_score = new Score(m_dataModel, m_nav.SelectSingleNode("score"), m_isInDataModel);
            m_result = new InteractionResult(m_dataModel, m_nav.SelectSingleNode("result"), m_isInDataModel);
            m_evaluation = new Evaluation(m_dataModel, m_nav.SelectSingleNode("evaluation"), m_isInDataModel);
            m_extensionData = new DataModelExtensionDictionary(m_dataModel, m_nav.SelectSingleNode("extensionData"), m_isInDataModel);
            m_rubrics = new DataModelList<Rubric>(m_dataModel, m_nav.SelectSingleNode("rubrics"), "rubric", String.Empty, m_isInDataModel,
                delegate(LearningDataModel _dataModel, XPathNavigator _nav, bool _isInDataModel)
                {
                    return new Rubric(_dataModel, _nav, _isInDataModel, true);
                });
            m_objectives = new DataModelList<InteractionObjective>(m_dataModel, m_nav.SelectSingleNode("objectives"), "objective", "id", m_isInDataModel,
                delegate(LearningDataModel _dataModel, XPathNavigator _nav, bool _isInDataModel)
                {
                    return new InteractionObjective(_dataModel, _nav, _isInDataModel, true);
                });
            m_correctResponses = new DataModelList<CorrectResponse>(m_dataModel, m_nav.SelectSingleNode("correctResponses"), "correctResponse", String.Empty, m_isInDataModel,
                delegate(LearningDataModel _dataModel, XPathNavigator _nav, bool _isInDataModel)
                {
                    return new CorrectResponse(_dataModel, _nav, _isInDataModel, true);
                });
        }

        /// <summary>
        /// Gets or sets the unique identifier of this interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.id.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.id.</para>
        /// <para>
        /// This property represents the unique string identifier for this interaction.  This identifier
        /// is only required to be unique within the context of this particular data model.
        /// </para>
        /// <para>
        /// This value must be set prior to this object's insertion into the <c>LearningDataModel.Interactions</c>
        /// list.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this interaction is already part of a list.</exception>
        /// <exception cref="ArgumentException">In SCORM 2004 packages, thrown if the value passed is empty or is a duplicate, or if the string is not a valid URI.  In SCORM 1.2 packages, this is thrown if the value passed is empty or if any of the characters in the string are not alphanumeric or - or _.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.InteractionItem.MaxInteractionIdFromCmiLength characters in SCORM 2004 packages,
        /// or longer than 255 in SCORM 1.2 packages.</exception>
        public string Id
        {
            get
            {
                return m_nav.GetAttribute("id", String.Empty);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                m_verifier.ValidateId(value);
                DataModelUtilities.SetAttribute(m_nav, "id", value);
                //DataModelUtilities.SetIdentifier(m_nav, "interactions", "interaction", value, m_verifier.ValidateId);
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the list of rubrics associated with this interaction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public Collection<Rubric> Rubrics
        {
            get
            {
                return new Collection<Rubric>(m_rubrics);
            }
        }

        /// <summary>
        /// Gets the instructor's evalutation associated with this interaction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public Evaluation Evaluation
        {
            get
            {
                return m_evaluation;
            }
        }

        /// <summary>
        /// Gets or sets which type of interaction this object corresponds to.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.type.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.type.</para>
        /// <para>
        /// Indicates the type of interaction that this instance represents and determines how the response should
        /// be interpreted.
        /// </para>
        /// <para>
        /// If this property has not yet been set, null will be returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the InteractionType enumerator is passed.</exception>
        public InteractionType? InteractionType
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<InteractionType>(m_nav, "interactionType", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(InteractionType != value)
                {
                    m_verifier.ValidateInteractionType(value);
                    // if the type changes, the response is no longer valid
                    LearnerResponse = null;
                    DataModelUtilities.SetEnumAttribute<InteractionType>(m_nav, "interactionType", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of objectives associated with this interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.type.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.type.</para>
        /// <para>
        /// The objectives referred to in this list are merely identifiers and do not contain
        /// all the data of an <Typ>Objective</Typ> object.
        /// </para>
        /// <para>
        /// The objective identifiers may or may not correspond to the objective identifiers 
        /// found in the <c>LearningDataModel.Objectives</c> data model element. Whether or not there 
        /// is a relationship to the objective identifiers is implementation specific. The RLO may be 
        /// designed to track this information and relationship.
        /// </para>
        /// </remarks>
        public Collection<InteractionObjective> Objectives
        {
            get
            {
                return new Collection<InteractionObjective>(m_objectives);
            }
        }

        /// <summary>
        /// Gets or sets the point in time at which the interaction was first made available to the learner.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.timespan.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.time.</para>
        /// <para>
        /// This property represents the point in time which the interaction was first made available
        /// to the learner for interaction and response.
        /// </para>
        /// <para>
        /// This field is in the same format as the SCORM specification for timestamps.  This may be achieved
        /// easily within C# by code like the example.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        ///     // this saves the timestamp with the current timezone information
        ///     session.CurrentActivityDataModel.Interactions[0].Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzzz");
        /// 
        ///     // this saves a UTC timestamp, which is recommended to avoid issues with timezones
        ///     session.CurrentActivityDataModel.Interactions[0].Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");
        /// 
        ///     // this saves timestamp with no specific timezone information
        ///     session.CurrentActivityDataModel.Interactions[0].Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        /// 
        ///     // In SCORM 1.2, the timestamp is simply a 24-hour clock, so the following format should be used
        ///     session.CurrentActivityDataModel.Interactions[0].Timestamp = DateTime.Now.ToString("HH:mm:ss");
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">In SCORM 2004 packages, thrown if the year is less than 1970 or greater than 2038.</exception>
        /// <exception cref="FormatException">Thrown if the passed string is not in the correct format.</exception>
        public string Timestamp
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "timestamp", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != Timestamp)
                {
                    m_verifier.ValidateTimeStamp(value);
                    DataModelUtilities.SetAttribute(m_nav, "timestamp", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of correct responses associated with this interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.correct_responses.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.correct_responses.</para>
        /// <para>
        /// The <Prp>CorrectResponses</Prp> collection is a list of correct responses 
        /// for an interaction. Depending on the type <Prp>InteractionType</Prp> of interaction, the 
        /// number of correct response patterns required to be supported varies. For SCORM 2004, refer 
        /// to RTE Section 4.2.9.1: Correct Responses Pattern Data Model Element Specifics for more 
        /// details on each type.  For SCORM 1.2, see the description of the 
        /// cmi.interactions.n.correct_responses.n.pattern data model element.
        /// </para>
        /// </remarks>
        public Collection<CorrectResponse> CorrectResponses
        {
            get
            {
                return new Collection<CorrectResponse>(m_correctResponses);
            }
        }

        /// <summary>
        /// Gets or sets the weight given to the interaction used to compute the value for a score.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.weighting.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.weighting.</para>
        /// <para>
        /// This property represents a weight that may be applied to the value of a score for this interaction.
        /// </para>
        /// <para>
        /// If this value has not been set, null will be returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public float? Weighting
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "weighting", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != Weighting)
                {
                    m_verifier.ValidateWeighting(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "weighting", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the data generated when a learner responds to the interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.learner_response.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.student_response.</para>
        /// <para>
        /// This property represents the data generated by the learner's response to an interaction.  The
        /// object type and format of the response is dependent upon the <Prp>InteractionType</Prp> of this
        /// interaction.
        /// </para>
        /// <para>
        /// If <Prp>InteractionType</Prp> is <c>InteractionType.TrueFalse</c>, this value will be returned 
        /// as a <c>Boolean</c> and is expected to be set as a <c>Boolean</c>.  If <Prp>InteractionType</Prp>
        /// is <c>InteractionType.Numeric</c>, this value will be returned as a <c>Single</c> and is expected
        /// to be set as a <c>Single</c>.  Otherwise, a string value is returned and expected.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentException">Thrown if the value passed is not the correct type.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is a string and is longer than BaseSchemaInternal.InteractionItem.MaxLearnerResponseStringLength in SCORM 2004 packages, 
        /// or greater than 255 characters in SCORM 1.2 packages.</exception>
        public object LearnerResponse
        {
            get
            {
                XPathNavigator nav = m_nav.Clone();

                if(!nav.MoveToAttribute("learnerResponse", String.Empty))
                {
                    return null;
                }
                if(InteractionType == Microsoft.LearningComponents.InteractionType.TrueFalse)
                {
                    return nav.ValueAsBoolean;
                }
                if(InteractionType == Microsoft.LearningComponents.InteractionType.Numeric)
                {
                    return (float)nav.ValueAsDouble;
                }
                return nav.Value;
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value == null && LearnerResponse == null)
                {
                    return;
                }
                if(value == null)
                {
                    DataModelUtilities.SetAttribute(m_nav, "learnerResponse", null);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                    return;
                }
                if(InteractionType == Microsoft.LearningComponents.InteractionType.TrueFalse)
                {
                    if((bool?)value != (bool?)LearnerResponse)
                    {
                        m_verifier.ValidateLearnerResponse(value);
                        DataModelUtilities.SetAttribute(m_nav, "learnerResponse", XmlConvert.ToString((bool)value));
                        if(m_isInDataModel)
                        {
                            m_dataModel.CallDataChanged();
                        }
                    }
                }
                else if(InteractionType == Microsoft.LearningComponents.InteractionType.Numeric)
                {
                    if((float?)value != (float?)LearnerResponse)
                    {
                        m_verifier.ValidateLearnerResponse(value);
                        DataModelUtilities.SetAttribute(m_nav, "learnerResponse", XmlConvert.ToString((float)value));
                        if(m_isInDataModel)
                        {
                            m_dataModel.CallDataChanged();
                        }
                    }
                }
                else
                {
                    string s = (string)value;
                    if(s != (string)LearnerResponse)
                    {
                        m_verifier.ValidateLearnerResponse(value);
                        DataModelUtilities.SetAttribute(m_nav, "learnerResponse", s);
                        if(m_isInDataModel)
                        {
                            m_dataModel.CallDataChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a judgment of the correctness of the learner response.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.result.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.result.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public InteractionResult Result
        {
            get
            {
                return m_result;
            }
        }

        /// <summary>
        /// Gets or sets the elapsed time between the time the interaction was made available to the learner 
        /// and the time of the first response.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.latency.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.latency.</para>
        /// <para>
        /// This property represents the amount of time elapsed between the <Prp>Timestamp</Prp> and the
        /// time of the first learner response.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public TimeSpan Latency
        {
            get
            {
                return DataModelUtilities.GetAttribute<TimeSpan>(m_nav, "latency", TimeSpan.Zero);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(value != Latency)
                {
                    m_verifier.ValidateLatency(value);
                    DataModelUtilities.SetAttribute(m_nav, "latency", XmlConvert.ToString(value));
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a brief informative description of the interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.description.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>If the value is accessed before it is set, null is returned</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.InteractionItem.MaxDescriptionLength characters.</exception>
        public string Description
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "description", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Description != value)
                {
                    m_verifier.ValidateDescription(value);
                    DataModelUtilities.SetAttribute(m_nav, "description", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the score for the interaction.
        /// </summary>
        /// <remarks>
        /// <para>This has no corresponding value in any SCORM setting.</para>
        /// </remarks>
        public Score Score
        {
            get
            {
                return m_score;
            }
        }

        /// <summary>
        /// Gets the extension data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Extension data does not correspond to any SCORM defined variables.  It is used to store information
        /// on a per activity basis that cannot be stored in SCORM specifc variables.
        /// </para>
        /// <para>
        /// Only certain types of data are valid within the extension data.  These include <Typ>Boolean</Typ>, 
        /// <Typ>Int32</Typ>, <Typ>DateTime</Typ>, <Typ>Double</Typ>, <Typ>String</Typ>, and <Typ>byte</Typ>[] array.
        /// If a <Typ>Single</Typ> is passed, this will automatically be converted to a <Typ>Double</Typ>.
        /// </para>
        /// <para>
        /// All keys into this dictionary are unique, and if the same key is used twice it will overwrite the value 
        /// and possibly the type information associated with that element.  SCORM naming conventions are recommended
        /// but not required for key (i.e. variable) names.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // assigns a boolean value of true to "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = true;
        /// 
        /// // assigns a double value of 3.1415927 to "foo.bar.something"
        /// // this changes the type as well as the value of the variable "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = 3.1415927
        /// </code>
        /// <code>
        /// byte[] attachment = ReadFileAsByteArray("c:\\assignment.doc");
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.attachment"] = attachment;
        /// </code>
        /// </example>
        public IDictionary<string, object> ExtensionData
        {
            get
            {
                return m_extensionData;
            }
        }

        /// <summary>
        /// Internally generated unique Id for SCORM 1.2 only.
        /// </summary>
        private string m_v1p2UniqueId;

        /// <summary>
        /// Gets the unique identifier for this class, for use internally by the <Typ>DataModelList</Typ> class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                if(m_dataModel.Format == PackageFormat.V1p2)
                {
                    if(m_v1p2UniqueId == null)
                    {
                        m_v1p2UniqueId = Guid.NewGuid().ToString();
                    }
                    return m_v1p2UniqueId;
                }
                return Id;
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }

        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
                ResetNavigator();
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
                m_score.IsInDataModel = value;
                m_extensionData.IsInDataModel = value;
                m_correctResponses.IsInDataModel = value;
                m_objectives.IsInDataModel = value;
                m_result.IsInDataModel = value;
                m_rubrics.IsInDataModel = value;
                m_evaluation.IsInDataModel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }
    }

    /// <summary>
    /// Represents an external evaluation of a learner's response to an interaction.
    /// </summary>
    public class Evaluation
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;result&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private EvaluationVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// A list of comments associated with this evaluation
        /// </summary>
        private DataModelList<Comment> m_comments;

        internal Evaluation(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new EvaluationVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new EvaluationVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new EvaluationVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;

            // this sub-node isn't created by default because I was too lazy to extend
            // CreateNavigator (used in CreateInteraction, which is the public API that creates this ultimately)
            // to handle more than 1 level of nesting of nodes.
            // Therefore, create the node here if it doesn't already exist.
            XPathNavigator commentsNav = m_nav.Clone();
            if(!commentsNav.MoveToChild("comments", String.Empty))
            {
                commentsNav.AppendChildElement(String.Empty, "comments", String.Empty, String.Empty);
                commentsNav.MoveToChild("comments", String.Empty);
            }

            m_comments = new DataModelList<Comment>(m_dataModel, commentsNav, "comment", String.Empty, true,
                delegate(LearningDataModel _dataModel, XPathNavigator _nav, bool _isInDataModel)
                {
                    return new Comment(_dataModel, _nav, _isInDataModel, true);
                });
        }

        /// <summary>
        /// Gets or sets the points assigned by the instructor for this evaluation.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is infinity or not a number.</exception>
        public float? Points
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "points", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Points != value)
                {
                    m_verifier.ValidatePoints(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "points", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of comments made by the instructor associated with this evaluation.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public Collection<Comment> Comments
        {
            get 
            {
                return new Collection<Comment>(m_comments);
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
                m_comments.IsInDataModel = value;
            }
        }
    }
    
    /// <summary>
    /// Represents a rubric in the list of rubrics belonging to a particular interaction.
    /// </summary>
    public class Rubric : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;objective&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private RubricVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new Rubric object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;objective&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal Rubric(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new RubricVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new RubricVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new RubricVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
        }

        /// <summary>
        /// Gets or sets the whether or not this rubric has been satisfied.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public bool? IsSatisfied
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<bool>(m_nav, "isSatisfied", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(IsSatisfied != value)
                {
                    m_verifier.ValidateIsSatisfied(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "isSatisfied", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the point value of the rubric if it has been satisfied.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is infinity or not a number.</exception>
        public float? Points
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "points", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Points != value)
                {
                    m_verifier.ValidatePoints(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "points", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the unique identifier for this class, for use internally by the <Typ>DataModelList</Typ> class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                throw new LearningComponentsInternalException("LDM4008");
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }
    
        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }
    }


    /// <summary>
    /// The result of an interaction, which is a judgement of correctness 
    /// of the learner response.
    /// </summary>
    /// <remarks>
    /// Provides information relating to cmi.interactions.n.result value.
    /// </remarks>
    public class InteractionResult
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;result&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private InteractionVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        internal InteractionResult(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new InteractionVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new InteractionVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new InteractionVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
        }

        /// <summary>
        /// Gets and sets the state of the interaction result. If not initialized, the value is InteractionResultState.None.
        /// If the value is InteractionResultState.Numeric, then the value should be set in <Prp>NumericResult</Prp>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value passed is not a valid value for the InteractionResultState enumeration.</exception>
        public InteractionResultState State 
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<InteractionResultState>(m_nav, "state", InteractionResultState.None).Value;
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if (State != value)
                {
                    m_verifier.ValidateResultState(value);
                    DataModelUtilities.SetEnumAttribute<InteractionResultState>(m_nav, "state", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the numeric indication of the correctness of the learner response. This value is only valid if 
        /// <Prp>State</Prp> is InteractionResultState.Numeric.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        public float? NumericResult
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "numeric", null);
            }
            set
            {
                if (m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if (NumericResult != value)
                {
                    m_verifier.ValidateNumericResult(value);
                    string s = null;
                    if (value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "numeric", s);
                    if (m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }
    }

    /// <summary>
    /// Represents an objective in the list of objectives belonging to a particular interaction.
    /// </summary>
    public class InteractionObjective : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;objective&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private InteractionObjectiveVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new InteractionObjective object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;objective&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal InteractionObjective(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new InteractionObjectiveVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new InteractionObjectiveVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new InteractionObjectiveVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
        }

        /// <summary>
        /// Gets or sets the unique identifier of this interaction.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.objectives.n.id.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.objectives.n.id.</para>
        /// <para>
        /// This property represents a unique identifier for an objective.  This identifier may or may not
        /// correspond to an objective defined within the <Prp>LearningDataModel.Objectives</Prp> collection.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentException">In SCORM 2004 packages, thrown if the value passed is empty, or if the string is not a valid URI.  In SCORM 1.2 packages, this is thrown if the value passed is empty or if any of the characters in the string are not alphanumeric or - or _.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength characters in SCORM 2004 packages,
        /// or longer than 255 in SCORM 1.2 packages.</exception>
        public string Id
        {
            get
            {
                return m_nav.GetAttribute("id", String.Empty);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                DataModelUtilities.SetIdentifier(m_nav, "objectives", "objective", value, m_verifier.ValidateId);
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets the unique identifier for this class, for use internally by the <Typ>DataModelList</Typ> class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                return Id;
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }
    
        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }
    }

    /// <summary>
    /// Represents a group of correct values, the exact format of which varies based on the 
    /// <Prp>../Interaction.InteractionType</Prp> property.
    /// </summary>
    public class CorrectResponse : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;correctResponse&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private CorrectResponseVerifier m_verifier;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new CorrectResponse object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;correctResponse&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal CorrectResponse(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new CorrectResponseVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new CorrectResponseVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new CorrectResponseVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
        }

        /// <summary>
        /// Gets or sets a pattern corresponding to a correct response of the type defined by <Prp>Interaction.InteractionType</Prp>.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.interactions.n.correct_responses.n.pattern.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.interactions.n.correct_responses.n.pattern.</para>
        /// <para>
        /// For the exact definition of the format of this value, see the SCORM 2004 specification, RTE 4.2.9.1a.
        /// </para>
        /// <para>
        /// This information is not checked for compliance with the SCORM specification, except for length.
        /// </para>
        /// <para>If the value has not been set before it is accessed, null is returned.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.CorrectResponseItem.MaxResponsePatternLength characters in SCORM 2004 packages,
        /// or longer than 255 in SCORM 1.2 packages.</exception>
        public string Pattern
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "pattern", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Pattern != value)
                {
                    m_verifier.ValidatePattern(value);
                    DataModelUtilities.SetAttribute(m_nav, "pattern", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// This is invalid for this class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                throw new LearningComponentsInternalException("LDM4006");
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }
    
        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
                m_isInDataModel = true;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }
    }

    /// <summary>
    /// Represents an objectives for an activity, used to track RLO defined goals.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An Objective represents a trackable objective associated with a particular
    /// activity.  The status of objectives may influence sequencing decisions.
    /// </para>
    /// </remarks>
    public class Objective : DataModelListElement
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to an &lt;objective&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private ObjectiveVerifier m_verifier;

        /// <summary>
        /// The score associated with this objective
        /// </summary>
        private Score m_score;

        /// <summary>
        /// Extension data associated with this objective
        /// </summary>
        private DataModelExtensionDictionary m_extensionData;

        /// <summary>
        /// Whether or not this should notify the data model of changes made to it.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Whether or not this object is already part of a DataModelList.
        /// </summary>
        private bool m_isInList;

        /// <summary>
        /// Initializes a new Objective object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;objective&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this should notify the data model of changes made to it.</param>
        /// <param name="isInList">Whether or not this object is already part of a <Typ>DataModelList</Typ>.</param>
        internal Objective(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel, bool isInList)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new ObjectiveVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new ObjectiveVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new ObjectiveVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
            m_isInList = isInList;
            ResetNavigator();
        }

        /// <summary>
        /// Resets all internal data fields that are dependent upon the navigator, called when the navigator changes
        /// for any reason.
        /// </summary>
        private void ResetNavigator()
        {
            m_score = new Score(m_dataModel, m_nav.SelectSingleNode("score"), m_isInDataModel);
            m_extensionData = new DataModelExtensionDictionary(m_dataModel, m_nav.SelectSingleNode("extensionData"), m_isInDataModel);
        }

        /// <summary>
        /// Gets or sets the unique identifier of this objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.id.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.objectives.n.id.</para>
        /// <para>
        /// This property represents a identifier for the objective, which is required to be unique within
        /// the context of this RLO.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentException">Thrown if the string passed is a duplicate value, or in SCORM 2004 packages, thrown if the value passed is empty, or if the string is not a valid URI.  In SCORM 1.2 packages, this is thrown if any of the characters in the string are not alphanumeric or - or _.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.AttemptObjectiveItem.MaxKeyLength characters in SCORM 2004 packages,
        /// or longer than 255 in SCORM 1.2 packages.</exception>
        public string Id
        {
            get
            {
                return m_nav.GetAttribute("id", String.Empty);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(IsPrimaryObjective && String.IsNullOrEmpty(value))
                {
                    // this is a special case - no need to validate
                    // IsPrimaryObjective will only be true in SCORM 2004 content
                    if(m_isInList && m_dataModel.Format != PackageFormat.V1p2)
                    {
                        ((DataModelKeyedList<Objective>)m_dataModel.Objectives).ChangeKey(this, value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "id", value);
                }
                else
                {
                    m_verifier.ValidateId(value);
                    if(m_isInList && m_dataModel.Format != PackageFormat.V1p2)
                    {
                        ((DataModelKeyedList<Objective>)m_dataModel.Objectives).ChangeKey(this, value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "id", value);
                }
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether or not the objective is the primary objective for this activity.
        /// </summary>
        internal bool IsPrimaryObjective 
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_nav, "isPrimary", false);
            }
        }

        /// <summary>
        /// Gets the score for the objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.score.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.objectives.n.score.</para>
        /// </remarks>
        public Score Score
        {
            get
            {
                return m_score;
            }
        }

        /// <summary>
        /// Gets or sets an indication of whether the learner has mastered the objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.success_status.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property indicates whether or not the learner has mastered the objective.  How the RLO
        /// determines this status is implementation dependent.
        /// </para>
        /// <para>
        /// This value will default to SuccessStatus.Unknown if not explicitly set by the RLO.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the SuccessStatus enumerator is passed.</exception>
        public SuccessStatus SuccessStatus
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<SuccessStatus>(m_nav, "successStatus", SuccessStatus.Unknown).Value;
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                m_verifier.ValidateSuccessStatus(value);
                DataModelUtilities.SetEnumAttribute<SuccessStatus>(m_nav, "successStatus", value);
                if(m_isInDataModel)
                {
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets an indication of whether the learner has completed the objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.completion_status.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This value will default to CompletionStatus.Unknown if not explicitly set by the RLO.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the CompletionStatus enumerator is passed.</exception>
        public CompletionStatus CompletionStatus
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<CompletionStatus>(m_nav, "completionStatus", CompletionStatus.Unknown).Value;
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(CompletionStatus != value)
                {
                    m_verifier.ValidateCompletionStatus(value);
                    DataModelUtilities.SetEnumAttribute<CompletionStatus>(m_nav, "completionStatus", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the status of this objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this is invalid.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.objectives.n.status.</para>
        /// <para>
        /// The status of the RLO's objective obtained by the student after each attempt to master the RLO's objective.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 2004 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the LessonStatus enumerator is passed.</exception>
        public LessonStatus? Status
        { 
            get
            {
                return DataModelUtilities.GetEnumAttribute<LessonStatus>(m_nav, "status", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Status != value)
                {
                    m_verifier.ValidateStatus(value);
                    DataModelUtilities.SetEnumAttribute<LessonStatus>(m_nav, "status", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the measure of the progress the learner has made toward completing the objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.progress_measure.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>
        /// This property represents a measure of the progress of that the learner has made toward completion of
        /// the RLO expressed as a number from 0.0 to 1.0.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value being assigned is outside the range 0.0 to 1.0.</exception>
        public float? ProgressMeasure
        { 
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "progressMeasure", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(ProgressMeasure != value)
                {
                    m_verifier.ValidateProgressMeasure(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "progressMeasure", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
                // Setting cmi.objectives.n.progress_measure is akin to the SCO "communicating completion information" to satisfy
                // SN 3.13.2, therefore set progress status to true.
                // DM I disagree with this - what use is this information without pass/fail information also?
                //SetObjectiveProgressStatus(true, true);
            }
        }

        /// <summary>
        /// Gets or sets a description of the objective.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.objectives.n.description.</para>
        /// <para>In SCORM 1.2, this is invalid.</para>
        /// <para>If the value has not been set before it is accessed, null is returned.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.AttemptObjectiveItem.MaxDescriptionLength</exception>
        public string Description
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "description", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Description != value)
                {
                    m_verifier.ValidateDescription(value);
                    DataModelUtilities.SetAttribute(m_nav, "description", value);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the extension data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Extension data does not correspond to any SCORM defined variables.  It is used to store information
        /// on a per activity basis that cannot be stored in SCORM specifc variables.
        /// </para>
        /// <para>
        /// Only certain types of data are valid within the extension data.  These include <Typ>Boolean</Typ>, 
        /// <Typ>Int32</Typ>, <Typ>DateTime</Typ>, <Typ>Double</Typ>, <Typ>String</Typ>, and <Typ>byte</Typ>[] array.
        /// If a <Typ>Single</Typ> is passed, this will automatically be converted to a <Typ>Double</Typ>.
        /// </para>
        /// <para>
        /// All keys into this dictionary are unique, and if the same key is used twice it will overwrite the value 
        /// and possibly the type information associated with that element.  SCORM naming conventions are recommended
        /// but not required for key (i.e. variable) names.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // assigns a boolean value of true to "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = true;
        /// 
        /// // assigns a double value of 3.1415927 to "foo.bar.something"
        /// // this changes the type as well as the value of the variable "foo.bar.something"
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.something"] = 3.1415927
        /// </code>
        /// <code>
        /// byte[] attachment = ReadFileAsByteArray("c:\\assignment.doc");
        /// session.CurrentActivityDataModel.ExtensionData["foo.bar.attachment"] = attachment;
        /// </code>
        /// </example>
        public IDictionary<string, object> ExtensionData
        {
            get
            {
                return m_extensionData;
            }
        }

        /// <summary>
        /// Indicates whether or not the <Prp>MinNormalizedMeasure</Prp> should be used in place of any other 
        /// method to determine if this objective is satisfied.
        /// </summary>
        internal bool SatisfiedByMeasure
        {
            get
            {
                return DataModelUtilities.GetAttribute<bool>(m_nav, "satisfiedByMeasure", false);
            }
        }

        /// <summary>
        /// Identifies the minimum satisfaction measure for the objective, normalized to -1 to 1.
        /// </summary>
        internal float MinNormalizedMeasure
        {
            get
            {
                return DataModelUtilities.GetAttribute<float>(m_nav, "minNormalizedMeasure", (float)1.0);
            }
        }

        /// <summary>
        /// If not empty, the name of the global ojective to read the satisfied status from.
        /// </summary>
        internal string GlobalObjectiveReadSatisfiedStatus 
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "readSatisfiedStatus", String.Empty);
            }
        }

        /// <summary>
        /// If not empty, the name of the global ojective to read the normalized measure from.
        /// </summary>
        internal string GlobalObjectiveReadNormalizedMeasure
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "readNormalizedMeasure", String.Empty);
            }
        }

        /// <summary>
        /// Gets an enumerator that contains the names of the global ojectives to write the satisfied status to.
        /// </summary>
        internal IEnumerable<string> GlobalObjectiveWriteSatisfiedStatus
        {
            get
            {
                int numObjectives = GlobalObjectiveWriteSatisfiedStatusCount;

                for(int i = 0 ; i < numObjectives ; ++i)
                {
                    yield return DataModelUtilities.GetAttribute<string>(m_nav, String.Format(CultureInfo.InvariantCulture, "writeSatisfiedStatus{0}", i), String.Empty);
                }
            }
        }

        /// <summary>
        /// Gets an enumerator that contains the names of the global ojectives to write the normalized measure to.
        /// </summary>
        internal IEnumerable<string> GlobalObjectiveWriteNormalizedMeasure
        {
            get
            {
                int numObjectives = GlobalObjectiveWriteNormalizedMeasureCount;

                for(int i = 0 ; i < numObjectives ; ++i)
                {
                    yield return DataModelUtilities.GetAttribute<string>(m_nav, String.Format(CultureInfo.InvariantCulture, "writeNormalizedMeasure{0}", i), String.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the number of global objectives that the satisfied status should be written to.
        /// </summary>
        internal int GlobalObjectiveWriteSatisfiedStatusCount
        {
            get
            {
                return DataModelUtilities.GetAttribute<int>(m_nav, "writeSatisfiedStatusCount", 0);
            }
        }

        /// <summary>
        /// Gets the number of global objectives that the normalized measure should be written to.
        /// </summary>
        internal int GlobalObjectiveWriteNormalizedMeasureCount
        {
            get
            {
                return DataModelUtilities.GetAttribute<int>(m_nav, "writeNormalizedMeasureCount", 0);
            }
        }

        /// <summary>
        /// Internally generated unique Id for SCORM 1.2 only.
        /// </summary>
        private string m_v1p2UniqueId;

        /// <summary>
        /// Gets the unique identifier for this class, for use internally by the <Typ>DataModelList</Typ> class.
        /// </summary>
        internal override string UniqueId
        {
            get
            {
                if(m_dataModel.Format == PackageFormat.V1p2)
                {
                    if(m_v1p2UniqueId == null)
                    {
                        m_v1p2UniqueId = Guid.NewGuid().ToString();
                    }
                    return m_v1p2UniqueId;
                }
                return Id;
            }
        }

        /// <summary>
        /// Gets the data model associated with this object
        /// </summary>
        internal override LearningDataModel DataModel
        {
            get
            {
                return m_dataModel;
            }
        }

        /// <summary>
        /// Gets or sets the XPathNavigator associated with this object
        /// </summary>
        internal override XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
            set
            {
                m_nav = value;
                ResetNavigator();
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object refers to an item that already exists in the <Typ>LearningDataModel</Typ>.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the data model is notified of changes made to the object.  If it is false,
        /// the data model is not notified of any changes made.
        /// </remarks>
        internal override bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
                m_score.IsInDataModel = value;
                m_extensionData.IsInDataModel = value;
            }
        }

        /// <summary>
        /// Returns whether this objective should be tracked.
        /// </summary>
        private bool Tracked
        {
            get
            {
                if(m_isInDataModel)
                {
                    return DataModel.Tracked;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this object is a member of an existing DataModelList.
        /// </summary>
        /// <remarks>
        /// If this value is true, that means the object cannot be added to any other list.
        /// </remarks>
        internal override bool IsInList
        {
            get
            {
                return m_isInList;
            }
            set
            {
                m_isInList = value;
            }
        }

        // Tracking model variables

        /// <summary>
        /// Gets or sets whether the objective currently has a valid satsifaction value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <remarks>Always returns <c>false</c> for activities that are not tracked.
        /// <para>If <c>DataModel.ObjectiveSetByContent == true</c>, setting this property will have no affect.</para>
        /// </remarks>
        internal bool ObjectiveProgressStatus
        {
            get
            {
                if(Tracked)
                {
                    return (SuccessStatus != SuccessStatus.Unknown);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the objective is satisfied.
        /// </summary>
        /// <remarks>
        /// This value is unreliable unless <Prp>ObjectiveProgressStatus </Prp> is true.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal bool ObjectiveSatisfiedStatus
        {
            get
            {
                return (SuccessStatus == SuccessStatus.Passed);
            }
        }

        /// <summary>
        /// Gets or sets whether the objective has a measure value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <remarks>Always returns <c>false</c> for activities that are not tracked.</remarks>
        internal bool ObjectiveMeasureStatus
        {
            get
            {
                if(Tracked)
                {
                    return Score.Scaled.HasValue;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the standardized score for the objective
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        internal float ObjectiveNormalizedMeasure
        {
            get
            {
                if(Score.Scaled.HasValue)
                {
                    return Score.Scaled.Value;
                }
                return 0f;
            }
        }
    }

    /// <summary>
    /// The score data model element is the learner’s score for the SCO.
    /// </summary>
    /// <remarks>
    /// This object is a child object used in various places.  It may refer to the score of the RLO as a whole,
    /// an <c>Interaction</c>, or an <c>Objective.</c>
    /// </remarks>
    public class Score
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;score&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// Gets the XPathNavigator that points to a &lt;navigationRequest&gt; element.
        /// </summary>
        internal XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
        }

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private ScoreVerifier m_verifier;

        /// <summary>
        /// Whether or not this object belongs to the data model.
        /// </summary>
        private bool m_isInDataModel;

        /// <summary>
        /// Gets or sets whether or not this object belongs to the data model.
        /// </summary>
        internal bool IsInDataModel
        {
            get
            {
                return m_isInDataModel;
            }
            set
            {
                m_isInDataModel = value;
            }
        }

        /// <summary>
        /// Initializes a new Score object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;score&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this object belongs to the data model.</param>
        private void Initialize(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
        {
            m_dataModel = dataModel;
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new ScoreVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new ScoreVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new ScoreVerifierV1p3();
                break;
            }
            m_nav = nav.Clone();
            m_isInDataModel = isInDataModel;
        }

        /// <summary>
        /// Initializes a new Score object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this score.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;score&gt; element.</param>
        /// <param name="isInDataModel">Whether or not this object belongs to the data model.</param>
        internal Score(LearningDataModel dataModel, XPathNavigator nav, bool isInDataModel)
        {
            Initialize(dataModel, nav, isInDataModel);
        }

        /// <summary>
        /// Gets or sets the scaled score value of the parent object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is a number that reflects the performance of the learner. The value 
        /// is scaled to fit the range –1.0 to 1.0 inclusive.
        /// </para>
        /// <para>
        /// If this value has not yet been set, null is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value being assigned is outside the range -1.0 to 1.0.</exception>
        public float? Scaled
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "scaled", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Scaled != value)
                {
                    m_verifier.ValidateScaled(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "scaled", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the raw score value of the parent object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is used to store a raw, non-scaled score.  This value should lie between
        /// the <Prp>Minimum</Prp> and <Prp>Maximum</Prp> values, however since these are optional
        /// properties no validation is performed against them.
        /// </para>
        /// <para>
        /// If this value has not yet been set, null is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If this is a SCORM 1.2 package, thrown if the value being assigned is outside the range 0 to 100.</exception>
        public float? Raw
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "raw", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Raw != value)
                {
                    m_verifier.ValidateRaw(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "raw", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum raw score value of the parent object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this value has not yet been set, null is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If this is a SCORM 1.2 package, thrown if the value being assigned is outside the range 0 to 100.</exception>
        public float? Minimum
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "minimum", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Minimum != value)
                {
                    m_verifier.ValidateMinimum(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "minimum", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum raw score value of the parent object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this value has not yet been set, null is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If this is a SCORM 1.2 package, thrown if the value being assigned is outside the range 0 to 100.</exception>
        public float? Maximum
        {
            get
            {
                return DataModelUtilities.GetNullableAttribute<float>(m_nav, "maximum", null);
            }
            set
            {
                if(m_isInDataModel)
                {
                    m_dataModel.CheckIfWriteIsAllowed();
                }
                if(Maximum != value)
                {
                    m_verifier.ValidateMaximum(value);
                    string s = null;
                    if(value.HasValue)
                    {
                        s = XmlConvert.ToString(value.Value);
                    }
                    DataModelUtilities.SetAttribute(m_nav, "maximum", s);
                    if(m_isInDataModel)
                    {
                        m_dataModel.CallDataChanged();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents an exit status and/or a request for navigation made by the RLO.
    /// </summary>
    /// <remarks>
    /// <para>This class is intended for use by RLO's, not by controlling applications.  It provides a method
    /// to identify what their status was when they were exited and what they would prefer the next navigation
    /// to be.  Controlling applications may simply call the desired navigation commands directly.
    /// </para>
    /// <para>
    /// The entire contents of this class is ignored in all cases except when <c>ProcessDataModelNavigation</c> is explicitly called
    /// on the <c>Session</c> object.
    /// </para>
    /// <para>
    /// When <c>ProcessDataModelNavigation</c> is called, the <Prp>ExitMode</Prp> property is checked first.  If this value is set to 
    /// <c>ExitMode.TimeOut</c>, a navigation request of <c>NavigationCommand.ExitAll</c> is performed.  
    /// If the value is set to <c>ExitMode.Logout</c>, a navigation request of <c>NavigationCommand.SuspendAll</c>
    /// is performed.  Otherwise, the navigation from <Prp>Command</Prp> is performed.
    /// </para>
    /// </remarks>
    public class NavigationRequest
    {
        /// <summary>
        /// The data model associated with this object
        /// </summary>
        private LearningDataModel m_dataModel;

        /// <summary>
        /// An XPathNavigator that points to a &lt;navigationRequest&gt; element.
        /// </summary>
        private XPathNavigator m_nav;

        /// <summary>
        /// Gets the XPathNavigator that points to a &lt;navigationRequest&gt; element.
        /// </summary>
        internal XPathNavigator Navigator
        {
            get
            {
                return m_nav;
            }
        }

        /// <summary>
        /// An internal class that is used to validate data.
        /// </summary>
        private NavigationRequestVerifier m_verifier;

        /// <summary>
        /// Initializes a new NavigationRequest object.
        /// </summary>
        /// <param name="dataModel">The data model associated with this navigationRequest.</param>
        /// <param name="nav">An XPathNavigator that points to a &lt;navigationRequest&gt; element.</param>
        internal NavigationRequest(LearningDataModel dataModel, XPathNavigator nav)
        {
            m_dataModel = dataModel;
            m_nav = nav.Clone();
            switch(m_dataModel.Format)
            {
            case PackageFormat.Lrm:
                m_verifier = new NavigationRequestVerifierLrm();
                break;
            case PackageFormat.V1p2:
                m_verifier = new NavigationRequestVerifierV1p2();
                break;
            case PackageFormat.V1p3:
                m_verifier = new NavigationRequestVerifierV1p3();
                break;
            }
        }
        
        /// <summary>
        /// Gets or sets the navigation command requested by the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this is a superset of the values allowed by the adl.nav.request command.</para>
        /// <para>In SCORM 1.2, there is no corresponding command.</para>
        /// <para>
        /// In the case of a call to <c>ProcessDataModelNavigation()</c> on the <c>Session</c> object, this navigation will
        /// be attempted automatically.  In any other case, this field is ignored.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the NavigationCommand enumerator is passed.</exception>
        public NavigationCommand? Command
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<NavigationCommand>(m_nav, "command", null);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(Command != value)
                {
                    m_verifier.ValidateCommand(value);
                    DataModelUtilities.SetEnumAttribute<NavigationCommand>(m_nav, "command", value);
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the exit status of the RLO.
        /// </summary>
        /// <remarks>
        /// <para>In SCORM 2004, this corresponds to cmi.exit.</para>
        /// <para>In SCORM 1.2, this corresponds to cmi.core.exit.</para>
        /// <para>
        /// In the case of a call to <c>ProcessDataModelNavigation()</c> on the <c>Session</c> object, this may cause a navigation 
        /// to occur, and may change some initial state in subsequent returns to this RLO.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a value not in the ExitMode enumerator is passed.</exception>
        public ExitMode? ExitMode
        {
            get
            {
                return DataModelUtilities.GetEnumAttribute<ExitMode>(m_nav, "exit", null);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(ExitMode != value)
                {
                    m_verifier.ValidateExitMode(value);
                    DataModelUtilities.SetEnumAttribute<ExitMode>(m_nav, "exit", value);
                    m_dataModel.CallDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the destination of a choice navigation.
        /// </summary>
        /// <remarks>
        /// <para>If <Prp>Command</Prp> is <c>NavigationCommand.Choice</c>, this property represents the 
        /// unique identifier of the activity that the navigation should move to.
        /// </para>
        /// <para>If <Prp>Command</Prp> is not <c>NavigationCommand.Choice</c>, this property is ignored.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this LearningDataModel object cannot be written to or if this is a SCORM 1.2 package.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the string passed is longer than BaseSchemaInternal.ActivityPackageItem.MaxActivityIdFromManifestLength characters</exception>
        public string Destination
        {
            get
            {
                return DataModelUtilities.GetAttribute<string>(m_nav, "destination", null);
            }
            set
            {
                m_dataModel.CheckIfWriteIsAllowed();
                if(Destination != value)
                {
                    m_verifier.ValidateDestination(value);
                    DataModelUtilities.SetAttribute(m_nav, "destination", value);
                    m_dataModel.CallDataChanged();
                }
            }
        }
    }
}
