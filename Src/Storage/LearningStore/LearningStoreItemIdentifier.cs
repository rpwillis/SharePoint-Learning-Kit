/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.LearningComponents;

#endregion

/*
 * This file contains the LearningStoreItemIdentifier class
 * 
 * Internal error numbers: 1700-1799
 */


namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents an identifier to an item in a store (or an identifier to an item that will be added to a store).
    /// </summary>
    /// <remarks>
    /// A LearningStoreItemIdentifier consists of two things that, when used together, uniquely identify an item in a store:<ul>
    /// <li>The name of an item type</li>
    /// <li>A 64-bit integer key (only available if the item exists in a store)</li>
    /// </ul>
    /// </remarks>
    public class LearningStoreItemIdentifier: IEquatable<LearningStoreItemIdentifier>
    {
        /// <summary>
        /// Represents an object that returns temporary keys for use by the
        /// LearningStoreItemIdentifier.CreateTemporaryItemIdentifier method
        /// </summary>
        private static class TemporaryKeyGenerator
        {
            /// <summary>
            /// Last temporary key that was generated
            /// </summary>
            private static long s_lastKey;
            
            /// <summary>
            /// Return a new temporary key
            /// </summary>
            /// <returns>The new temporary key</returns>
            public static long GetNewKey()
            {
                return Interlocked.Decrement(ref s_lastKey);
            }
        }
        
        /// <summary>
        /// Name of the item type that this identifier refers to
        /// </summary>
        private string m_itemTypeName;

        /// <summary>
        /// Key of the identifier
        /// </summary>
        /// <remarks>
        /// If this value is positive, it contains a value created from
        /// the database.  If this value is negative, it contains a value
        /// created by a particular job, and is only "understood" by that
        /// particular job.  See the documentation for LearningStoreJob.AddItem
        /// for more details on where this is useful.  A value of zero is
        /// invalid.
        /// </remarks>
        private long m_key;

        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreItemIdentifier()
        {
        }

        /// <summary>
        /// Create a new instance of the <Typ>LearningStoreItemIdentifier</Typ> class.
        /// </summary>
        /// <param name="itemTypeName">Name of the item type to which the identifier refers.</param>
        /// <param name="key">The unique integer value assigned to the item.  This
        ///     must be a positive integer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="itemTypeName"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        /// <example>
        /// The following code creates a reference to the user with a key of
        /// 50:
        /// <code language="C#">
        /// LearningStoreItemIdentifier id = new LearningStoreItemIdentifier("UserItem", 50);
        /// </code>
        /// </example>
        public LearningStoreItemIdentifier(string itemTypeName, long key)
        {
            // Check parameters
            if (itemTypeName == null)
                throw new ArgumentNullException("itemTypeName");
            if (key <= 0)
                throw new ArgumentOutOfRangeException("key",
                    LearningStoreStrings.PositiveValueExpected);

            // Save the data
            m_itemTypeName = itemTypeName;
            m_key = key;
        }

        /// <summary>
        /// Create a new instance of the <Typ>LearningStoreItemIdentifier</Typ> class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="ArgumentNullException"><paramref name="id"/> is a null reference.</exception>
        protected LearningStoreItemIdentifier(LearningStoreItemIdentifier id)
        {
            // Check parameters
            if(id == null)
                throw new ArgumentNullException("id");
            
            m_itemTypeName = id.m_itemTypeName;
            m_key = id.m_key;
        }
        
        /// <summary>
        /// Retrieve the unique integer value assigned to the item.
        /// </summary>
        /// <remarks>
        /// Note that this value only uniquely identifies an item within a particular
        /// item type.  Two items with two different <Prp>ItemTypeName</Prp>s may return
        /// the same value from <Mth>GetKey</Mth>.
        /// <p/>
        /// A unique integer value is not always available.  In particular, a unique
        /// integer value is not available from <Typ>LearningStoreItemIdentifier</Typ>s
        /// returned from <Mth>../LearningStoreJob.AddItem</Mth>.  Identifiers
        /// returned from that method have not yet been added to the store, and therefore
        /// do not yet have a unique integer value.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">A unique integer
        ///     value is not available.</exception>
        /// <example>
        /// The following code creates a <Typ>LearningStoreItemIdentifier</Typ> and then retrieves
        /// the unique integer value:
        /// <code language="C#">
        /// LearningStoreItemIdentifier id = new LearningStoreItemIdentifier("UserItem", 50);
        /// long key = id.GetKey();
        /// </code>
        /// After the above code is executed, key contains 50.
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1024")] // Might throw an exception in common situations, so it should be a method
        public long GetKey()
        {
            if (m_key > 0)
                return m_key;
            else
                throw new InvalidOperationException(LearningStoreStrings.UniqueKeyNotAvailable);
        }

        /// <summary>
        /// Returns a string representation of the current object
        /// </summary>
        /// <returns>A string representation of the current object</returns>
        public override string ToString()
        {
            return "ItemType:" + m_itemTypeName + ((m_key > 0) ?
                (", Key:" + m_key.ToString(CultureInfo.InvariantCulture)) : "");
        }

        /// <summary>
        /// True if a unique key is available
        /// </summary>
        internal bool HasKey
        {
            get
            {
                return m_key > 0;
            }
        }

        /// <summary>
        /// The actual key (which may be positive or negative)
        /// </summary>
        internal long RawKey
        {
            get
            {
                return m_key;
            }
        }

        /// <summary>
        /// Name of the item type referenced by this Id.
        /// </summary>
        /// <example>
        /// <code language="C#">
        /// LearningStoreItemIdentifier id = new LearningStoreItemIdentifier("UserItem", 50);
        /// string name = id.ItemTypeName;
        /// </code>
        /// After the above code is executed, name contains "UserItem"
        /// </example>
        public string ItemTypeName
        {
            get
            {
                return m_itemTypeName;
            }
        }

        /// <summary>
        /// Returns a value indicating whether two instances of LearningStoreItemIdentifier represent the same item. 
        /// </summary>
        /// <param name="obj">A LearningStoreItemIdentifier to compare to this instance.</param>
        /// <returns>True if <paramref name="obj"/> is equal to this instance; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            LearningStoreItemIdentifier otherId = obj as LearningStoreItemIdentifier;            
            if(otherId == null)
                return false;
            return ((IEquatable<LearningStoreItemIdentifier>)this).Equals(otherId);
        }

        /// <summary>
        /// Returns a value indicating whether two instances of LearningStoreItemIdentifier represent the same item. 
        /// </summary>
        /// <param name="other">A LearningStoreItemIdentifier to compare to this instance.</param>
        /// <returns>True if <paramref name="other"/> is equal to this instance; otherwise, false</returns>
        public bool Equals(LearningStoreItemIdentifier other)
        {
            if(other == null)
                return false;
            if (m_key != other.m_key)
                return false;
            if (String.Compare(m_itemTypeName, other.m_itemTypeName, StringComparison.Ordinal) != 0)
                return false;
            return true;
        }
        
        /// <summary>
        /// Returns the hash code for this instance. 
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return m_itemTypeName.GetHashCode() ^ m_key.GetHashCode();
        }

        /// <summary>
        /// Compares two LearningStoreItemIdentifier objects to determine whether they are equal.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the two instances are equal. Otherwise false.</returns>
        public static bool operator ==(LearningStoreItemIdentifier left, LearningStoreItemIdentifier right)
        {
            if(Object.ReferenceEquals(left,right))
                return true;
            else if((object)left == null)
                return false;
            else if((object)right == null)
                return false;
            else
                return left.Equals(right);
        }
        
        /// <summary>
        /// Compares two LearningStoreItemIdentifier objects to determine whether they are different.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>True if the two instances are different. Otherwise false.</returns>
        public static bool operator !=(LearningStoreItemIdentifier left, LearningStoreItemIdentifier right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Create a new temporary LearningStoreIdentifier (that doesn't have a positive id value)
        /// </summary>
        /// <param name="itemTypeName">Item type name</param>
        /// <returns>The new LearningStoreItemIdentifier</returns>
        internal static LearningStoreItemIdentifier CreateTemporaryItemIdentifier(string itemTypeName)
        {
            // Check input parameters
            if (itemTypeName == null)
                throw new LearningComponentsInternalException("LSTR1700");

            LearningStoreItemIdentifier id = new LearningStoreItemIdentifier();
            id.m_itemTypeName = itemTypeName;
            id.m_key = TemporaryKeyGenerator.GetNewKey();
            return id;
        }


    }
}
