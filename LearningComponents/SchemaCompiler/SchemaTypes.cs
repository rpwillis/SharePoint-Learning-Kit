/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Reflection;

#endregion

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Type of a property on an item or column in a view
    /// </summary>
    internal enum LearningStoreValueTypeCode
    {
        /// <summary>
        /// A reference to another item
        /// </summary>
        ItemIdentifier = 1,
        
        /// <summary>
        /// A string
        /// </summary>
        String,
        
        /// <summary>
        /// A boolean
        /// </summary>
        Boolean,
        
        /// <summary>
        /// A DateTime
        /// </summary>
        DateTime,
        
        /// <summary>
        /// A single
        /// </summary>
        Single,
        
        /// <summary>
        /// A double
        /// </summary>
        Double,
        
        /// <summary>
        /// Xml
        /// </summary>
        Xml,
        
        /// <summary>
        /// An enumeration
        /// </summary>
        Enumeration,
        
        /// <summary>
        /// An integer
        /// </summary>
        Int32,
        
        /// <summary>
        /// A byte array
        /// </summary>
        ByteArray,
        
        /// <summary>
        /// A guid
        /// </summary>
        Guid,
    };
    
}
