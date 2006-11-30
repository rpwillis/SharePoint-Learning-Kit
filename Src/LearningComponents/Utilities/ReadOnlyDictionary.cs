/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Implements a read-only version of <Typ>IDictionary</Typ>.
    /// </summary>
    /// <remarks>Ensures that RloHandlers don’t change the collection of form controls that is provided by the application. 
    /// Makes the interface cleaner so that it can be expanded later with different RloHandlers with less hassle.
    /// </remarks>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    internal class ReadOnlyDictionary<K, V> : IDictionary<K, V>
    {
        IDictionary<K, V> m_dictionary;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dictionary">The <Typ>IDictionary</Typ> to wrap.</param>
        internal ReadOnlyDictionary(IDictionary<K, V> dictionary)
        {
            m_dictionary = dictionary;
        }

        #region IDictionary<K, V> Members

        /// <summary>
        /// Not supported. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public void Add(K key, V value)
        {
            throw new NotSupportedException(Resources.ListCannotBeModified);
        }

        /// <summary>
        /// Determines whether the <Typ>ReadOnlyDictionary</Typ> contains an element with the specified key.  
        /// </summary>
        /// <param name="key">The key to locate in the <Typ>ReadOnlyDictionary</Typ>.</param>
        /// <returns><c>true</c> if the <Typ>ReadOnlyDictionary</Typ> contains an element with the key; otherwise, <c>false</c>. </returns>
        public bool ContainsKey(K key)
        {
            return m_dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets an <Typ>ICollection</Typ> containing the keys of the <Typ>ReadOnlyDictionary</Typ>. 
        /// </summary>
        public ICollection<K> Keys
        {
            get { return m_dictionary.Keys; }
        }
        
        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public bool Remove(K key)
        {
            throw new NotSupportedException(Resources.ListCannotBeModified);
        }

        /// <summary>
        /// Gets the value associated with the specified key.  
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the object that implements <Typ>ReadOnlyDictionary</Typ> contains an element with the specified key; 
        /// otherwise, <c>false</c>. </returns>
        public bool TryGetValue(K key, out V value)
        {
            return m_dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <Typ>ICollection</Typ> containing the values in the <Typ>IDictionary</Typ>. 
        /// </summary>
        public ICollection<V> Values
        {
            get { return m_dictionary.Values; }
        }

        /// <summary>
        /// Gets the element with the specified key.  Set is not supported.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>The element with the specified key.</returns>
        public V this[K key]
        {
            get
            {
                return m_dictionary[key];
            }
            set
            {
                throw new NotSupportedException(Resources.ListCannotBeModified);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public void Add(KeyValuePair<K, V> item)
        {
            throw new NotSupportedException(Resources.ListCannotBeModified);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public void Clear()
        {
            throw new NotSupportedException(Resources.ListCannotBeModified);
        }

        /// <summary>
        /// Determines whether the <Typ>ICollection</Typ> contains a specific value. 
        /// </summary>
        /// <param name="item">The object to locate in the <Typ>ICollection</Typ>.</param>
        /// <returns><c>true</c> if item is found in the <Typ>ICollection</Typ>; otherwise, <c>false</c>. </returns>
        public bool Contains(KeyValuePair<K, V> item)
        {
            ICollection<KeyValuePair<K, V>> coll = m_dictionary as ICollection<KeyValuePair<K, V>>;
            return coll.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <Typ>ICollection</Typ> to an <Typ>Array</Typ>, starting at a particular <Typ>Array</Typ> index. 
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from <Typ>ICollection</Typ>. 
        /// The <Typ>Array</Typ> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            ICollection<KeyValuePair<K, V>> coll = m_dictionary as ICollection<KeyValuePair<K, V>>;
            coll.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements in the <Typ>ICollection</Typ>.
        /// </summary>
        public int Count
        {
            get { return m_dictionary.Count; }
        }

        /// <summary>
        /// Always returns <c>true</c>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotSupportedException(Resources.ListCannotBeModified);
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        /// <summary>
        /// Supports a simple iteration over a nongeneric collection. 
        /// </summary>
        /// <returns><Typ>IEnumerator</Typ> base interface.</returns>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            IEnumerable<KeyValuePair<K, V>> d = m_dictionary;
            return d.GetEnumerator();

        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection. 
        /// </summary>
        /// <returns>An <Typ>IEnumerator</Typ> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            System.Collections.IEnumerable d = m_dictionary as System.Collections.IEnumerable;
            return d.GetEnumerator();
        }

        #endregion
    }
}
