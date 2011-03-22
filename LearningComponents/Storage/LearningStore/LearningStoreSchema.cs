/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Microsoft.LearningComponents;

#endregion

/*
 * This file contains schema-related classes
 * 
 * Internal error numbers: 2400-2999
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Utility functions used by the other schema functionality
    /// </summary>
    internal static class LearningStoreSchemaUtilities
    {
        /// <summary>
        /// Determines if <paramref name="identifier"/> is a valid identifier
        /// </summary>
        /// <param name="identifier">Identifier to check</param>
        /// <returns>True if <paramref name="identifier"/> is a valid identifier</returns>
        /// <remarks>
        /// Used for item type names, enum names, property names, etc.
        /// </remarks>
        public static bool IsIdentifierValid(string identifier)
        {
            // Must be between 1 and 48 characters in length
            if((identifier.Length == 0) ||
               (identifier.Length > 48))
               return false;
               
            // First character must be a letter or underscore
            if((!Char.IsLetter(identifier[0])) && (identifier[0] != '_'))
                return false;
                
            // All following characters must be letters, numbers, or underscores
            for(int i=1; i<identifier.Length; i++)
            {
                if((!Char.IsLetterOrDigit(identifier, i)) && (identifier[i] != '_'))
                    return false;
            }
            
            return true;
        }
    }

    /// <summary>
    /// Represents the type of a property on an item type or the type of a column in a view
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreValueType
    {
        /// <summary>
        /// If CastValue discovers an error, this delegate is called to
        /// retrieve the Exception that should be thrown.
        /// </summary>
        /// <param name="reason">Reason for the error</param>
        /// <param name="innerException">Inner exception, or null if none</param>
        public delegate Exception GetCastException(string reason, Exception innerException);
        
        /// <summary>
        /// Schema in which this type exists
        /// </summary>
        private LearningStoreSchema m_schema;
        
        /// <summary>
        /// Type code
        /// </summary>
        private LearningStoreValueTypeCode m_typeCode;

        /// <summary>
        /// Can null be stored in the type?
        /// </summary>
        private bool m_nullable;

        /// <summary>
        /// Name of the item type referenced by this type
        /// </summary>
        /// <remarks>
        /// Null unless <Fld>m_typeCode</Fld>=ItemIdentifier
        /// </remarks>
        private string m_referencedItemTypeName;

        /// <summary>
        /// Name of the enum type referenced by this type
        /// </summary>
        /// <remarks>
        /// Null unless <Fld>m_typeCode</Fld>=Enumeration
        /// </remarks>
        private string m_enumTypeName;

        /// <summary>
        /// Enum type referenced by this type
        /// </summary>
        /// <remarks>
        /// Null unless <Fld>m_typeCode</Fld>=Enumeration<p/>
        /// Filled in when first accessed.
        /// </remarks>
        private LearningStoreEnumType m_enumType;

        /// <summary>
        /// Item type referenced by this type
        /// </summary>
        /// <remarks>
        /// Null unless <Fld>m_typeCode</Fld>=ItemIdentifier<p/>
        /// Filled in when first accessed.
        /// </remarks>
        private LearningStoreItemType m_referencedItemType;

        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreValueType()
        {
        }

        /// <summary>
        /// Type code
        /// </summary>
        public LearningStoreValueTypeCode TypeCode
        {
            get
            {
                return m_typeCode;
            }
        }

        /// <summary>
        /// Can null be stored in the type?
        /// </summary>
        public bool Nullable
        {
            get
            {
                return m_nullable;
            }
        }

        /// <summary>
        /// Item type referenced by this type
        /// </summary>
        /// <remarks>
        /// Will throw an exception if <Prp>TypeCode</Prp>!=ItemIdentifier
        /// </remarks>
        public LearningStoreItemType ReferencedItemType
        {
            get
            {
                Utilities.Assert(m_typeCode == LearningStoreValueTypeCode.ItemIdentifier);
                if (m_referencedItemType == null)
                {
                    // Fill it in
                    LearningStoreItemType itemType = m_schema.GetItemTypeByName(m_referencedItemTypeName);
                    Interlocked.CompareExchange<LearningStoreItemType>(
                        ref m_referencedItemType, itemType, null);
                }
                return m_referencedItemType;
            }
        }

        /// <summary>
        /// Enum type referenced by this type
        /// </summary>
        /// <remarks>
        /// Will throw an exception if <Prp>TypeCode</Prp>!=Enumeration
        /// </remarks>
        public LearningStoreEnumType EnumType
        {
            get
            {
                Utilities.Assert(m_typeCode == LearningStoreValueTypeCode.Enumeration);
                if (m_enumType == null)
                {
                    // Fill it in
                    LearningStoreEnumType enumType = m_schema.GetEnumTypeByName(m_enumTypeName);
                    Interlocked.CompareExchange<LearningStoreEnumType>(
                        ref m_enumType, enumType, null);
                }
                return m_enumType;
            }
        }

        /// <summary>
        /// Cast the passed-in value to a value that can be stored
        /// in something of this type
        /// </summary>
        /// <param name="value">Value to cast</param>
        /// <param name="formatprovider"><Typ>IFormatProvider</Typ> used to convert to/from strings</param>
        /// <param name="getCastException">Delegate used to create an Exception when an error is found.</param>
        /// <returns>The value cast to something of this type.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800")] // FxCop doesn't seem to notice that only one of the casts will actually be used
        public object CastValue(object value, IFormatProvider formatprovider, GetCastException getCastException)
        {
            try
            {
                // Handle null
                if (value == null)
                {
                    if(!m_nullable)
                        throw getCastException(
                            LearningStoreStrings.ValueCannotBeNull, null);
                    return null;
                }

                // Handle DBNull.Value
                if(Object.ReferenceEquals(DBNull.Value, value))
                {
                    if(!m_nullable)
                        throw getCastException(
                            LearningStoreStrings.ValueCannotBeNull, null);
                    return null;
                }

                switch (m_typeCode)
                {
                    // Handle Boolean
                    case LearningStoreValueTypeCode.Boolean:
                        {
                            IConvertible convertible = (IConvertible)value;
                            return convertible.ToBoolean(formatprovider);
                        }

                    // Handle ByteArray
                    case LearningStoreValueTypeCode.ByteArray:
                        {
                            if(!(value is byte[]))
                                throw getCastException(
                                    LearningStoreStrings.ValueMustBeByteArray, null);
                            return value;
                        }

                    // Handle DateTime                    
                    case LearningStoreValueTypeCode.DateTime:
                        {
                            IConvertible convertible = (IConvertible)value;
                            DateTime convertedValue = convertible.ToDateTime(formatprovider);
                            if((convertedValue < (DateTime)SqlDateTime.MinValue) ||
                               (convertedValue > (DateTime)SqlDateTime.MaxValue))
                                throw getCastException(
                                    LearningStoreStrings.ValueMustBeWithinSqlDateTimeRange, null);
                            return (DateTime)new SqlDateTime(convertedValue);
                        }

                    // Handle Double
                    case LearningStoreValueTypeCode.Double:
                        {
                            IConvertible convertible = (IConvertible)value;
                            double convertedValue = convertible.ToDouble(formatprovider);
                            // Verify that it can be converted to a SqlDouble
                            SqlDouble sqlValue = new SqlDouble(convertedValue);
                            return convertedValue;
                        }

                    // Handle Guid
                    case LearningStoreValueTypeCode.Guid:
                        {
                            if(value is Guid)
                                return value;
                            string valueAsString = value as string;
                            if(valueAsString != null)
                                return new Guid(valueAsString);
                            throw getCastException(
                                LearningStoreStrings.ValueMustBeGuid, null);
                        }

                    // Handle ItemIdentifier
                    case LearningStoreValueTypeCode.ItemIdentifier:
                        {
                            LearningStoreItemIdentifier id = value as LearningStoreItemIdentifier;
                            if (id != null)
                            {
                                if (id.ItemTypeName == m_referencedItemTypeName)
                                    return id;
                            }
                            throw getCastException(
                                String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ValueMustBeItemIdentifier, m_referencedItemTypeName), null);
                        }

                    // Handle Single
                    case LearningStoreValueTypeCode.Single:
                        {
                            IConvertible convertible = (IConvertible)value;
                            Single convertedValue = convertible.ToSingle(formatprovider);
                            // Verify that it can be converted to a SqlSingle
                            SqlSingle sqlValue = new SqlSingle(convertedValue);
                            return convertedValue;                        
                        }

                    // Handle string
                    case LearningStoreValueTypeCode.String:
                        {
                            IConvertible convertible = (IConvertible)value;
                            return convertible.ToString(formatprovider);
                        }

                    // Handle Xml
                    case LearningStoreValueTypeCode.Xml:
                        {
                            if (!(value is LearningStoreXml))
                                throw getCastException(
                                    LearningStoreStrings.ValueMustBeXml, null);
                            return value;
                        }

                    // Handle enumeration
                    case LearningStoreValueTypeCode.Enumeration:
                        {
                            IConvertible convertible = (IConvertible)value;
                            int convertedValue = convertible.ToInt32(formatprovider);

                            if (!EnumType.ContainsValue(convertedValue))
                                throw getCastException(
                                    String.Format(CultureInfo.CurrentCulture, LearningStoreStrings.ValueMustBeValidEnumValue, EnumType.Name), null);
                            return convertedValue;
                        }

                    // Handle Int32
                    case LearningStoreValueTypeCode.Int32:
                        {
                            IConvertible convertible = (IConvertible)value;
                            return convertible.ToInt32(formatprovider);
                        }

                    default:
                        throw new LearningComponentsInternalException("LSTR2400");

                }
            }
            catch (FormatException e)
            {
                throw getCastException(LearningStoreStrings.ValueHasInvalidFormat, e);
            }
            catch (OverflowException e)
            {
                throw getCastException(LearningStoreStrings.ValueCausesOverflow, e);
            }
            catch (InvalidCastException e)
            {
                throw getCastException(LearningStoreStrings.ValueCausesInvalidCast, e);
            }
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreValueType</Typ>.  The new instance
        /// refers to an item type
        /// </summary>
        /// <param name="schema">Schema in which the type should be created.</param>
        /// <param name="referencedItemTypeName">Name of the item type to which
        ///     the new type refers to.</param>
        /// <returns>The new <Typ>LearningStoreValueType</Typ></returns>
        public static LearningStoreValueType CreateIdValueType(LearningStoreSchema schema,
            string referencedItemTypeName)
        {
            // Check parameters
            if(schema == null)
                throw new LearningComponentsInternalException("LSTR2410");
            if(referencedItemTypeName == null)
                throw new LearningComponentsInternalException("LSTR2420");
                
            // Create the value type
            LearningStoreValueType valueType = new LearningStoreValueType();
            
            // Save the schema
            valueType.m_schema = schema;
            
            // Set the type code
            valueType.m_typeCode = LearningStoreValueTypeCode.ItemIdentifier;
            
            // Set the referenced item info
            valueType.m_referencedItemTypeName = referencedItemTypeName;
            
            return valueType;
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreValueType</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="schema">Schema in which the type should be created.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreValueType</Typ></returns>
        public static LearningStoreValueType CreateValueType(LearningStoreSchema schema,
            XPathNavigator navigator)
        {
            // Check parameters
            if(schema == null)
                throw new LearningComponentsInternalException("LSTR2430");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2440");
                
            // Create the value type
            LearningStoreValueType valueType = new LearningStoreValueType();

            // Save the schema
            valueType.m_schema = schema;

            // Read the type code
            valueType.m_typeCode = (LearningStoreValueTypeCode)XmlConvert.ToInt32(navigator.GetAttribute("TypeCode", String.Empty));
            if((valueType.m_typeCode < LearningStoreValueTypeCode.ItemIdentifier) || (valueType.m_typeCode > LearningStoreValueTypeCode.Guid))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read if the value is nullable
            valueType.m_nullable = XmlConvert.ToBoolean(navigator.GetAttribute("Nullable", String.Empty));

            // Read the referenced item info if needed
            if (valueType.m_typeCode == LearningStoreValueTypeCode.ItemIdentifier)
            {
                // Read the referenced item name
                valueType.m_referencedItemTypeName = navigator.GetAttribute("ReferencedItemTypeName", String.Empty);
                if(!LearningStoreSchemaUtilities.IsIdentifierValid(valueType.m_referencedItemTypeName))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
            }

            // Read the enumeration info if required
            if (valueType.m_typeCode == LearningStoreValueTypeCode.Enumeration)
            {
                // Read the referenced enumeration name
                valueType.m_enumTypeName = navigator.GetAttribute("EnumName", String.Empty);
                if (!LearningStoreSchemaUtilities.IsIdentifierValid(valueType.m_enumTypeName))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
            }

            return valueType;
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreValueType</Typ> for a column
        /// in a default view
        /// </summary>
        /// <param name="property">Property that is being exposed.</param>
        /// <returns>The new <Typ>LearningStoreValueType</Typ></returns>
        public static LearningStoreValueType CreateValueTypeForDefaultView(LearningStoreItemTypeProperty property)
        {
            // Check parameters
            if (property == null)
                throw new LearningComponentsInternalException("LSTR2445");

            // Create the value type
            LearningStoreValueType valueType = new LearningStoreValueType();

            // Copy the data
            valueType.m_enumTypeName = property.ValueType.m_enumTypeName;
            valueType.m_nullable = true;
            valueType.m_referencedItemTypeName = property.ValueType.m_referencedItemTypeName;
            valueType.m_schema = property.ValueType.m_schema;
            valueType.m_typeCode = property.ValueType.m_typeCode;

            return valueType;
        }
    }    

    /// <summary>
    /// Represents an enumeration type in a schema
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreEnumType
    {
        /// <summary>
        /// Name of the enumeration type
        /// </summary>
        private string m_name;
        
        /// <summary>
        /// Dictionary mapping numeric value to value name
        /// </summary>
        private Dictionary<int,string> m_valueMap = new Dictionary<int,string>();

        /// <summary>
        /// List of all the numeric values in the enumeration type
        /// </summary>
        private List<int> m_values = new List<int>();

        /// <summary>
        /// List of all the value names in the enumeration type
        /// </summary>        
        private List<string> m_valueNames = new List<string>();
        
        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreEnumType()
        {
        }

        /// <summary>
        /// Name of the enumeration type
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// List of all the numeric values in the enumerated type
        /// </summary>
        public ReadOnlyCollection<int> Values
        {
            get
            {
                return new ReadOnlyCollection<int>(m_values);
            }
        }

        /// <summary>
        /// List of all the value names in the enumerated type
        /// </summary>
        public ReadOnlyCollection<string> ValueNames
        {
            get
            {
                return new ReadOnlyCollection<string>(m_valueNames);
            }
        }

        /// <summary>
        /// Checks if an enumerated type contains a value with the corresponding
        /// numeric value.
        /// </summary>
        /// <param name="value">Value to look for</param>
        /// <returns>True if the value exists in the enumerated type.</returns>
        public bool ContainsValue(int value)
        {
            return m_valueMap.ContainsKey(value);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreEnumType</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="schema">Schema in which the type should be created.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreEnumType</Typ></returns>
        public static LearningStoreEnumType CreateEnumType(LearningStoreSchema schema, XPathNavigator navigator)
        {
            // Check parameters
            if(schema == null)
                throw new LearningComponentsInternalException("LSTR2450");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2460");
                
            // Create the enum
            LearningStoreEnumType enumType = new LearningStoreEnumType();
            
            // Read the name
            enumType.m_name = navigator.GetAttribute("Name", String.Empty);
            if(!LearningStoreSchemaUtilities.IsIdentifierValid(enumType.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the values
            foreach(XPathNavigator subNode in navigator.SelectChildren("Value", String.Empty))
            {
                // Read the name
                string valueName = subNode.GetAttribute("Name", String.Empty);
                if(!LearningStoreSchemaUtilities.IsIdentifierValid(valueName))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
                if (enumType.m_valueNames.Contains(valueName))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
                                    
                // Read the value
                int valueValue = XmlConvert.ToInt32(subNode.GetAttribute("Value", String.Empty));
                if(enumType.m_values.Contains(valueValue))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
                    
                // Add
                enumType.m_values.Add(valueValue);
                enumType.m_valueNames.Add(valueName);
                enumType.m_valueMap.Add(valueValue, valueName);
            }

            return enumType;
        }
    }

    /// <summary>
    /// Represents a property on an item type
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreItemTypeProperty
    {
        /// <summary>
        /// Name of the property
        /// </summary>        
        private string m_name;

        /// <summary>
        /// Type of the property
        /// </summary>        
        private LearningStoreValueType m_valueType;

        /// <summary>
        /// Does the property have a default value?
        /// </summary>
        private bool m_hasDefaultValue;

        /// <summary>
        /// Is the property the unique Id of the item type?
        /// </summary>
        private bool m_isId;
        
        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreItemTypeProperty()
        {
        }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// True if this property is the unique Id of the item type
        /// </summary>
        public bool IsId
        {
            get
            {
                return m_isId;
            }
        }
        
        /// <summary>
        /// True if this property has a type of Xml
        /// </summary>
        public bool IsXml
        {
            get
            {
                return m_valueType.TypeCode == LearningStoreValueTypeCode.Xml;
            }
        }

        /// <summary>
        /// Type of the property
        /// </summary>
        public LearningStoreValueType ValueType
        {
            get
            {
                return m_valueType;
            }
        }

        /// <summary>
        /// True if this property has a default value
        /// </summary>
        public bool HasDefaultValue
        {
            get
            {
                return m_hasDefaultValue;
            }
        }

        /// <summary>
        /// Cast the passed-in value to a value that can be stored
        /// in a property of this type.
        /// </summary>
        /// <param name="value">Value to cast</param>
        /// <param name="formatprovider"><Typ>IFormatProvider</Typ> used to convert to/from strings</param>
        /// <param name="getCastException">Delegate called to retrieve the exception to be thrown on error.</param>
        /// <returns>The value cast to something of this type.</returns>
        public object CastValue(object value, IFormatProvider formatprovider,
            LearningStoreValueType.GetCastException getCastException)
        {
            return m_valueType.CastValue(value, formatprovider, getCastException);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreItemTypeProperty</Typ>.  The new instance
        /// refers to the "Id" property of <paramref name="itemType"/>.
        /// </summary>
        /// <param name="itemType">Item type on which the Id exists on.</param>
        /// <returns>The new <Typ>LearningStoreItemTypeProperty</Typ></returns>
        public static LearningStoreItemTypeProperty CreateCurrentIdProperty(LearningStoreItemType itemType)
        {        
            // Check parameters
            if (itemType == null)
                throw new LearningComponentsInternalException("LSTR2470");

            // Create the property
            LearningStoreItemTypeProperty property = new LearningStoreItemTypeProperty();
            property.m_valueType = LearningStoreValueType.CreateIdValueType(itemType.Schema, itemType.Name);
            property.m_name = "Id";
            property.m_isId = true;

            return property;
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreItemTypeProperty</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="itemType">Item type on which the property exists on.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreItemTypeProperty</Typ></returns>
        public static LearningStoreItemTypeProperty CreateProperty(LearningStoreItemType itemType,
            XPathNavigator navigator)
        {
            // Check parameters
            if(itemType == null)
                throw new LearningComponentsInternalException("LSTR2480");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2490");
                
            // Create the property
            LearningStoreItemTypeProperty property = new LearningStoreItemTypeProperty();

            // Read the name
            property.m_name = navigator.GetAttribute("Name", String.Empty);
            if(!LearningStoreSchemaUtilities.IsIdentifierValid(property.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the value type
            property.m_valueType = LearningStoreValueType.CreateValueType(itemType.Schema, navigator);
            
            // Read the default value information
            property.m_hasDefaultValue = XmlConvert.ToBoolean(navigator.GetAttribute("HasDefault", String.Empty));

            return property;
        }
    }    

    /// <summary>
    /// Represents an item type in a schema
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreItemType
    {
        /// <summary>
        /// Schema in which this type exists
        /// </summary>
        private LearningStoreSchema m_schema;
        
        /// <summary>
        /// Name of the item type
        /// </summary>
        private string m_name;

        /// <summary>
        /// Name of the SQL function that defines the default view
        /// </summary>
        private string m_viewFunction;

        /// <summary>
        /// Name of the SQL function that defines the default view security,
        /// or null if it doesn't exist
        /// </summary>
        private string m_viewSecurityFunction;

        /// <summary>
        /// Name of the SQL function that defines the add security,
        /// or null if it doesn't exist
        /// </summary>
        private string m_addSecurityFunction;

        /// <summary>
        /// Name of the SQL function that defines the delete security,
        /// or null if it doesn't exist
        /// </summary>
        private string m_deleteSecurityFunction;

        /// <summary>
        /// Name of the SQL function that defines the update security,
        /// or null if it doesn't exist
        /// </summary>
        private string m_updateSecurityFunction;

        /// <summary>
        /// Properties on the item type
        /// </summary>        
        private List<LearningStoreItemTypeProperty> m_properties =
            new List<LearningStoreItemTypeProperty>();

        /// <summary>
        /// Properties without a default value on the item type
        /// </summary>
        private List<LearningStoreItemTypeProperty> m_propertiesWithoutDefaultValue =
            new List<LearningStoreItemTypeProperty>();

        /// <summary>
        /// Dictionary of properties indexed by name
        /// </summary>
        private Dictionary<string,LearningStoreItemTypeProperty> m_propertiesByName =
            new Dictionary<string,LearningStoreItemTypeProperty>();
            
        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreItemType()
        {
        }

        /// <summary>
        /// Schema in which this type exists
        /// </summary>
        public LearningStoreSchema Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Name of the item type
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// SQL function that defines the default view
        /// </summary>
        public string DefaultViewFunction
        {
            get
            {
                return m_viewFunction;
            }
        }

        /// <summary>
        /// SQL function that defines the default view security
        /// </summary>
        public string DefaultViewSecurityFunction
        {
            get
            {
                return m_viewSecurityFunction;
            }
        }

        /// <summary>
        /// SQL function that defines the add security
        /// </summary>
        public string AddSecurityFunction
        {
            get
            {
                return m_addSecurityFunction;
            }
        }

        /// <summary>
        /// SQL function that defines the delete security
        /// </summary>
        public string DeleteSecurityFunction
        {
            get
            {
                return m_deleteSecurityFunction;
            }
        }

        /// <summary>
        /// SQL function that defines the update security
        /// </summary>
        public string UpdateSecurityFunction
        {
            get
            {
                return m_updateSecurityFunction;
            }
        }

        /// <summary>
        /// Properties on the item type
        /// </summary>
        public ReadOnlyCollection<LearningStoreItemTypeProperty> Properties
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreItemTypeProperty>(m_properties);
            }
        }

        /// <summary>
        /// Properties without a default value on the item type
        /// </summary>
        public ReadOnlyCollection<LearningStoreItemTypeProperty> PropertiesWithoutDefaultValue
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreItemTypeProperty>(m_propertiesWithoutDefaultValue);
            }
        }

        /// <summary>
        /// Attempts to get a property by name.
        /// </summary>
        /// <param name="name">Name of the property to find.</param>
        /// <param name="foundProperty">The found property, or null if the property wasn't found.</param>
        /// <returns>True if the property was found.</returns>
        public bool TryGetPropertyByName(string name,
            out LearningStoreItemTypeProperty foundProperty)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_propertiesByName.TryGetValue(name, out foundProperty);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreItemType</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="schema">Schema in which the item type exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreItemType</Typ></returns>
        public static LearningStoreItemType CreateItemType(LearningStoreSchema schema, XPathNavigator navigator)
        {
            // Check input parameters
            if(schema == null)
                throw new LearningComponentsInternalException("LSTR2500");
            if(navigator == null)
                throw new LearningComponentsInternalException("LSTR2510");
                
            // Create an item type
            LearningStoreItemType itemType = new LearningStoreItemType();

            // Save the schema
            itemType.m_schema = schema;

            // Read the name
            itemType.m_name = navigator.GetAttribute("Name", String.Empty);
            if(!LearningStoreSchemaUtilities.IsIdentifierValid(itemType.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the SQL function information
            itemType.m_viewFunction = navigator.GetAttribute("ViewFunction", String.Empty);
            if(String.IsNullOrEmpty(itemType.m_viewFunction))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);            
            itemType.m_viewSecurityFunction = navigator.GetAttribute("ViewSecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(itemType.m_viewSecurityFunction))
                itemType.m_viewSecurityFunction = null;
            itemType.m_addSecurityFunction = navigator.GetAttribute("AddSecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(itemType.m_addSecurityFunction))
                itemType.m_addSecurityFunction = null;
            itemType.m_deleteSecurityFunction = navigator.GetAttribute("DeleteSecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(itemType.m_deleteSecurityFunction))
                itemType.m_deleteSecurityFunction = null;
            itemType.m_updateSecurityFunction = navigator.GetAttribute("UpdateSecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(itemType.m_updateSecurityFunction))
                itemType.m_updateSecurityFunction = null;
            
            // Add the "Id" property
            LearningStoreItemTypeProperty property = LearningStoreItemTypeProperty.CreateCurrentIdProperty(itemType);
            itemType.m_properties.Add(property);
            itemType.m_propertiesByName.Add(property.Name, property);

            // Read the other properties
            foreach (XPathNavigator subNode in navigator.SelectChildren("Property", String.Empty))
            {
                // Read the property
                property = LearningStoreItemTypeProperty.CreateProperty(itemType, subNode);

                // Verify that it doesn't already exist
                if(itemType.m_propertiesByName.ContainsKey(property.Name))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                // Add it
                itemType.m_properties.Add(property);
                itemType.m_propertiesByName.Add(property.Name, property);                
                if(!property.HasDefaultValue)
                    itemType.m_propertiesWithoutDefaultValue.Add(property);
            }

            return itemType;
        }
    }

    /// <summary>
    /// Represents a column in a view
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreViewColumn
    {
        /// <summary>
        /// Name of the column
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type of the column
        /// </summary>
        private LearningStoreValueType m_valueType;

        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreViewColumn()
        {
        }

        /// <summary>
        /// Name of the column
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Type of the column
        /// </summary>
        public LearningStoreValueType ValueType
        {
            get
            {
                return m_valueType;
            }
        }

        /// <summary>
        /// True if the column is sortable
        /// </summary>
        public bool Sortable
        {
            get
            {
                return (m_valueType.TypeCode != LearningStoreValueTypeCode.Xml) &&
                       (m_valueType.TypeCode != LearningStoreValueTypeCode.Guid);
            }
        }

        /// <summary>
        /// True if the column contains Xml
        /// </summary>
        public bool IsXml
        {
            get
            {
                return m_valueType.TypeCode == LearningStoreValueTypeCode.Xml;
            }
        }

        /// <summary>
        /// True if the column contains a Guid
        /// </summary>
        public bool IsGuid
        {
            get
            {
                return m_valueType.TypeCode == LearningStoreValueTypeCode.Guid;
            }
        }

        /// <summary>
        /// Cast the passed-in value to a value that can be stored
        /// in a column of this type.
        /// </summary>
        /// <param name="value">Value to cast</param>
        /// <param name="formatprovider"><Typ>IFormatProvider</Typ> used to convert to/from strings</param>
        /// <param name="getCastException">Delegate called to retrieve the exception to be thrown on error.</param>
        /// <returns>The value cast to something of this type.</returns>
        public object CastValue(object value, IFormatProvider formatprovider,
            LearningStoreValueType.GetCastException getCastException)
        {
            return m_valueType.CastValue(value, formatprovider, getCastException);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreViewColumn</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="view">View in which the column exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreViewColumn</Typ></returns>
        public static LearningStoreViewColumn CreateColumn(LearningStoreView view,
            XPathNavigator navigator)
        {
            // Check parameters
            if (view == null)
                throw new LearningComponentsInternalException("LSTR2520");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2530");

            // Create the column
            LearningStoreViewColumn column = new LearningStoreViewColumn();

            // Read the name
            column.m_name = navigator.GetAttribute("Name", String.Empty);
            if (!LearningStoreSchemaUtilities.IsIdentifierValid(column.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the value type
            column.m_valueType = LearningStoreValueType.CreateValueType(view.Schema, navigator);

            return column;
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreViewColumn</Typ> for a default
        /// view
        /// </summary>
        /// <param name="property">Property that the view exposes.</param>
        /// <returns>The new <Typ>LearningStoreViewColumn</Typ></returns>
        public static LearningStoreViewColumn CreateColumnForDefaultView(LearningStoreItemTypeProperty property)
        {
            // Check parameters
            if (property == null)
                throw new LearningComponentsInternalException("LSTR2532");

            // Create the column
            LearningStoreViewColumn column = new LearningStoreViewColumn();

            // Copy the name and value type
            column.m_name = property.Name;
            column.m_valueType = LearningStoreValueType.CreateValueTypeForDefaultView(property);

            return column;
        }
    }

    /// <summary>
    /// Represents a parameter in a view
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreViewParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type of the parameter
        /// </summary>
        private LearningStoreValueType m_valueType;

        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreViewParameter()
        {
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public LearningStoreValueType ValueType
        {
            get
            {
                return m_valueType;
            }
        }

        /// <summary>
        /// Cast the passed-in value to a value that can be stored
        /// in a parameter of this type.
        /// </summary>
        /// <param name="value">Value to cast</param>
        /// <param name="formatprovider"><Typ>IFormatProvider</Typ> used to convert to/from strings</param>
        /// <param name="getCastException">Delegate called to retrieve the exception to be thrown on error.</param>
        /// <returns>The value cast to something of this type.</returns>
        public object CastValue(object value, IFormatProvider formatprovider,
            LearningStoreValueType.GetCastException getCastException)
        {
            return m_valueType.CastValue(value, formatprovider, getCastException);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreViewParameter</Typ> using data
        /// read from a navigator.
        /// </summary>
        /// <param name="view">View in which the parameter exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreViewParameter</Typ></returns>
        public static LearningStoreViewParameter CreateParameter(LearningStoreView view,
            XPathNavigator navigator)
        {
            // Check parameters
            if (view == null)
                throw new LearningComponentsInternalException("LSTR2533");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2536");

            // Create the parameter
            LearningStoreViewParameter parameter = new LearningStoreViewParameter();

            // Read the name
            parameter.m_name = navigator.GetAttribute("Name", String.Empty);
            if (!LearningStoreSchemaUtilities.IsIdentifierValid(parameter.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the value type
            parameter.m_valueType = LearningStoreValueType.CreateValueType(view.Schema, navigator);

            return parameter;
        }
    }

    /// <summary>
    /// Represents a view in a schema
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreView
    {
        /// <summary>
        /// Schema in which this view exists
        /// </summary>
        private LearningStoreSchema m_schema;

        /// <summary>
        /// Name of the view
        /// </summary>
        private string m_name;

        /// <summary>
        /// Name of the SQL function that defines the view
        /// </summary>
        private string m_function;

        /// <summary>
        /// Name of the SQL function that defines the view security,
        /// or null if it doesn't exist
        /// </summary>
        private string m_securityFunction;

        /// <summary>
        /// Columns in the view
        /// </summary>
        private List<LearningStoreViewColumn> m_columns =
            new List<LearningStoreViewColumn>();

        /// <summary>
        /// Dictionary of columns indexed by name
        /// </summary>
        private Dictionary<string,LearningStoreViewColumn> m_columnsByName =
            new Dictionary<string,LearningStoreViewColumn>();

        /// <summary>
        /// Parameters in the view
        /// </summary>
        private List<LearningStoreViewParameter> m_parameters =
            new List<LearningStoreViewParameter>();

        /// <summary>
        /// Dictionary of parameters indexed by name
        /// </summary>
        private Dictionary<string, LearningStoreViewParameter> m_parametersByName =
            new Dictionary<string, LearningStoreViewParameter>();

        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreView()
        {
        }

        /// <summary>
        /// Schema in which this view exists
        /// </summary>
        public LearningStoreSchema Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Name of the view
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// SQL function that defines the view
        /// </summary>
        public string Function
        {
            get
            {
                return m_function;
            }
        }

        /// <summary>
        /// SQL function that defines the view security
        /// </summary>
        public string SecurityFunction
        {
            get
            {
                return m_securityFunction;
            }
        }

        /// <summary>
        /// Columns in the view
        /// </summary>
        public ReadOnlyCollection<LearningStoreViewColumn> Columns
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreViewColumn>(m_columns);
            }
        }

        /// <summary>
        /// Parameters in the view
        /// </summary>
        public ReadOnlyCollection<LearningStoreViewParameter> Parameters
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreViewParameter>(m_parameters);
            }
        }

        /// <summary>
        /// Attempts to get a parameter by name.
        /// </summary>
        /// <param name="name">Name of the parameter to find.</param>
        /// <param name="foundParameter">The found parameter, or null if the parameter wasn't found.</param>
        /// <returns>True if the parameter was found.</returns>
        public bool TryGetParameterByName(string name,
            out LearningStoreViewParameter foundParameter)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_parametersByName.TryGetValue(name, out foundParameter);
        }

        /// <summary>
        /// Attempts to get a column by name.
        /// </summary>
        /// <param name="name">Name of the column to find.</param>
        /// <param name="foundColumn">The found column, or null if the column wasn't found.</param>
        /// <returns>True if the column was found.</returns>
        public bool TryGetColumnByName(string name,
            out LearningStoreViewColumn foundColumn)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_columnsByName.TryGetValue(name, out foundColumn);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreView</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="schema">Schema in which the view exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreView</Typ></returns>
        public static LearningStoreView CreateView(LearningStoreSchema schema, XPathNavigator navigator)
        {
            // Check input parameters
            if (schema == null)
                throw new LearningComponentsInternalException("LSTR2540");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2550");

            // Create a view
            LearningStoreView view = new LearningStoreView();

            // Save the schema
            view.m_schema = schema;

            // Read the name
            view.m_name = navigator.GetAttribute("Name", String.Empty);
            if (!LearningStoreSchemaUtilities.IsIdentifierValid(view.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the SQL function information
            view.m_function = navigator.GetAttribute("Function", String.Empty);
            if (String.IsNullOrEmpty(view.m_function))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
            view.m_securityFunction = navigator.GetAttribute("SecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(view.m_securityFunction))
                view.m_securityFunction = null;

            // Read the columns
            foreach (XPathNavigator subNode in navigator.SelectChildren("Column", String.Empty))
            {
                // Read the column
                LearningStoreViewColumn column = LearningStoreViewColumn.CreateColumn(view, subNode);

                // Verify that it doesn't already exist
                if(view.m_columnsByName.ContainsKey(column.Name))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                // Add it
                view.m_columns.Add(column);
                view.m_columnsByName.Add(column.Name, column);
            }
            
            // Read the parameters
            foreach (XPathNavigator subNode in navigator.SelectChildren("Parameter", String.Empty))
            {
                // Read the parameter
                LearningStoreViewParameter parameter = LearningStoreViewParameter.CreateParameter(view, subNode);

                // Verify that it doesn't already exist
                if (view.m_parametersByName.ContainsKey(parameter.Name))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                // Add it
                view.m_parameters.Add(parameter);
                view.m_parametersByName.Add(parameter.Name, parameter);                    
            }
            
            return view;
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreView</Typ> for a default
        /// view
        /// </summary>
        /// <param name="itemType">Item type.</param>
        /// <returns>The new <Typ>LearningStoreView</Typ></returns>
        public static LearningStoreView CreateDefaultView(LearningStoreItemType itemType)
        {
            // Check input parameters
            if (itemType == null)
                throw new LearningComponentsInternalException("LSTR2555");

            // Create a view
            LearningStoreView view = new LearningStoreView();

            // Copy the info
            view.m_schema = itemType.Schema;
            view.m_name = itemType.Name;
            view.m_function = itemType.DefaultViewFunction;
            view.m_securityFunction = itemType.DefaultViewSecurityFunction;

            // Create the columns
            foreach(LearningStoreItemTypeProperty property in itemType.Properties)
            {
                // Create the column
                LearningStoreViewColumn column = LearningStoreViewColumn.CreateColumnForDefaultView(property);
                
                // Add it
                view.m_columns.Add(column);
                view.m_columnsByName.Add(column.Name, column);
            }

            return view;
        }
    }

    /// <summary>
    /// Represents a parameter in a right
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreRightParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type of the parameter
        /// </summary>
        private LearningStoreValueType m_valueType;

        /// <summary>
        /// Constructor
        /// </summary>        
        private LearningStoreRightParameter()
        {
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public LearningStoreValueType ValueType
        {
            get
            {
                return m_valueType;
            }
        }

        /// <summary>
        /// Cast the passed-in value to a value that can be stored
        /// in a parameter of this type.
        /// </summary>
        /// <param name="value">Value to cast</param>
        /// <param name="formatprovider"><Typ>IFormatProvider</Typ> used to convert to/from strings</param>
        /// <param name="getCastException">Delegate called to retrieve the exception to be thrown on error.</param>
        /// <returns>The value cast to something of this type.</returns>
        public object CastValue(object value, IFormatProvider formatprovider,
            LearningStoreValueType.GetCastException getCastException)
        {
            return m_valueType.CastValue(value, formatprovider, getCastException);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreRightParameter</Typ> using data
        /// read from a navigator.
        /// </summary>
        /// <param name="right">Right in which the parameter exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreViewParameter</Typ></returns>
        public static LearningStoreRightParameter CreateParameter(LearningStoreRight right,
            XPathNavigator navigator)
        {
            // Check parameters
            if (right == null)
                throw new LearningComponentsInternalException("LSTR2560");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2570");

            // Create the parameter
            LearningStoreRightParameter parameter = new LearningStoreRightParameter();

            // Read the name
            parameter.m_name = navigator.GetAttribute("Name", String.Empty);
            if (!LearningStoreSchemaUtilities.IsIdentifierValid(parameter.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the value type
            parameter.m_valueType = LearningStoreValueType.CreateValueType(right.Schema, navigator);

            return parameter;
        }
    }

    /// <summary>
    /// Represents a right in a schema
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreRight
    {
        /// <summary>
        /// Schema in which this right exists
        /// </summary>
        private LearningStoreSchema m_schema;

        /// <summary>
        /// Name of the right
        /// </summary>
        private string m_name;

        /// <summary>
        /// Name of the SQL function that defines the security, or null
        /// if it doesn't exist
        /// </summary>
        private string m_securityFunction;

        /// <summary>
        /// Parameters in the right
        /// </summary>
        private List<LearningStoreRightParameter> m_parameters =
            new List<LearningStoreRightParameter>();

        /// <summary>
        /// Dictionary of parameters indexed by name
        /// </summary>
        private Dictionary<string, LearningStoreRightParameter> m_parametersByName =
            new Dictionary<string, LearningStoreRightParameter>();

        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreRight()
        {
        }

        /// <summary>
        /// Schema in which this right exists
        /// </summary>
        public LearningStoreSchema Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Name of the right
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// SQL function that defines the right security
        /// </summary>
        public string SecurityFunction
        {
            get
            {
                return m_securityFunction;
            }
        }

        /// <summary>
        /// Parameters in the view
        /// </summary>
        public ReadOnlyCollection<LearningStoreRightParameter> Parameters
        {
            get
            {
                return new ReadOnlyCollection<LearningStoreRightParameter>(m_parameters);
            }
        }

        /// <summary>
        /// Attempts to get a parameter by name.
        /// </summary>
        /// <param name="name">Name of the parameter to find.</param>
        /// <param name="foundParameter">The found parameter, or null if the parameter wasn't found.</param>
        /// <returns>True if the parameter was found.</returns>
        public bool TryGetParameterByName(string name,
            out LearningStoreRightParameter foundParameter)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_parametersByName.TryGetValue(name, out foundParameter);
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreRight</Typ> using data
        /// read from an XPathNavigator.
        /// </summary>
        /// <param name="schema">Schema in which the right exists.</param>
        /// <param name="navigator">Location from which the data should be read</param>
        /// <returns>The new <Typ>LearningStoreRight</Typ></returns>
        public static LearningStoreRight CreateRight(LearningStoreSchema schema, XPathNavigator navigator)
        {
            // Check input parameters
            if (schema == null)
                throw new LearningComponentsInternalException("LSTR2580");
            if (navigator == null)
                throw new LearningComponentsInternalException("LSTR2590");

            // Create a right
            LearningStoreRight right = new LearningStoreRight();
             
            // Save the schema
            right.m_schema = schema;

            // Read the name
            right.m_name = navigator.GetAttribute("Name", String.Empty);
            if (!LearningStoreSchemaUtilities.IsIdentifierValid(right.m_name))
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

            // Read the SQL function information
            right.m_securityFunction = navigator.GetAttribute("SecurityFunction", String.Empty);
            if (String.IsNullOrEmpty(right.m_securityFunction))
                right.m_securityFunction = null;

            // Read the parameters
            foreach (XPathNavigator subNode in navigator.SelectChildren("Parameter", String.Empty))
            {
                // Read the parameter
                LearningStoreRightParameter parameter = LearningStoreRightParameter.CreateParameter(right, subNode);

                // Verify that it doesn't already exist
                if (right.m_parametersByName.ContainsKey(parameter.Name))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                // Add it
                right.m_parameters.Add(parameter);
                right.m_parametersByName.Add(parameter.Name, parameter);
            }

            return right;
        }

    }

    /// <summary>
    /// Represents a schema
    /// </summary>
    /// <remarks>
    /// Immutable (from the user's point-of-view) once created
    /// </remarks>
    internal class LearningStoreSchema
    {
        /// <summary>
        /// Item types in the schema
        /// </summary>
        private List<LearningStoreItemType> m_itemTypes =
            new List<LearningStoreItemType>();

        /// <summary>
        /// Enum types in the schema
        /// </summary>
        private List<LearningStoreEnumType> m_enumTypes =
            new List<LearningStoreEnumType>();

        /// <summary>
        /// Views in the schema
        /// </summary>
        private List<LearningStoreView> m_views =
            new List<LearningStoreView>();

        /// <summary>
        /// Rights in the schema
        /// </summary>
        private List<LearningStoreRight> m_rights =
            new List<LearningStoreRight>();
            
        /// <summary>
        /// Item types in the schema indexed by name
        /// </summary>
        private Dictionary<string, LearningStoreItemType> m_itemTypesByName =
            new Dictionary<string,LearningStoreItemType>();

        /// <summary>
        /// Enum types in the schema indexed by name
        /// </summary>            
        private Dictionary<string,LearningStoreEnumType> m_enumTypesByName =
            new Dictionary<string,LearningStoreEnumType>();
        
        /// <summary>
        /// Views in the schema indexed by name
        /// </summary>
        private Dictionary<string, LearningStoreView> m_viewsByName =
            new Dictionary<string,LearningStoreView>();
        
        /// <summary>
        /// Rights in the schema indexed by name
        /// </summary>
        private Dictionary<string, LearningStoreRight> m_rightsByName =
            new Dictionary<string,LearningStoreRight>();
                
        /// <summary>
        /// Constructor
        /// </summary>
        private LearningStoreSchema()
        {
        }

        /// <summary>
        /// Attempts to get an item type by name.
        /// </summary>
        /// <param name="name">Name of the item type to find.</param>
        /// <param name="foundItemType">The found item type, or null if the item type wasn't found.</param>
        /// <returns>True if the item type was found.</returns>
        public bool TryGetItemTypeByName(string name,
            out LearningStoreItemType foundItemType)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_itemTypesByName.TryGetValue(name, out foundItemType);
        }

        /// <summary>
        /// Attempts to get an enum type by name.
        /// </summary>
        /// <param name="name">Name of the enum type to find.</param>
        /// <param name="foundEnumType">The found enum type, or null if the enum type wasn't found.</param>
        /// <returns>True if the enum type was found.</returns>
        public bool TryGetEnumTypeByName(string name,
            out LearningStoreEnumType foundEnumType)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_enumTypesByName.TryGetValue(name, out foundEnumType);
        }

        /// <summary>
        /// Attempts to get a view by name.
        /// </summary>
        /// <param name="name">Name of the view to find.</param>
        /// <param name="foundView">The found view, or null if the view wasn't found.</param>
        /// <returns>True if the view was found.</returns>
        public bool TryGetViewByName(string name,
            out LearningStoreView foundView)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_viewsByName.TryGetValue(name, out foundView);
        }

        /// <summary>
        /// Attempts to get a right by name.
        /// </summary>
        /// <param name="name">Name of the right to find.</param>
        /// <param name="foundRight">The found right, or null if the right wasn't found.</param>
        /// <returns>True if the view right found.</returns>
        public bool TryGetRightByName(string name,
            out LearningStoreRight foundRight)
        {
            // Check the input parameters
            Utilities.Assert(name != null);

            return m_rightsByName.TryGetValue(name, out foundRight);
        }

        /// <summary>
        /// Get an item type
        /// </summary>
        /// <param name="name">Name of the item type to find.</param>
        /// <returns>The found item type.</returns>
        public LearningStoreItemType GetItemTypeByName(string name)
        {
            // Check input parameters
            Utilities.Assert(name != null);
                
            LearningStoreItemType foundItemType;
            if(TryGetItemTypeByName(name, out foundItemType))
                return foundItemType;
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                Microsoft.LearningComponents.Storage.LearningStoreStrings.ItemTypeNotFoundInSchema, name));
        }

        /// <summary>
        /// Get an enum type by name
        /// </summary>
        /// <param name="name">Name of the enum type to find.</param>
        /// <returns>The found enum type.</returns>
        public LearningStoreEnumType GetEnumTypeByName(string name)
        {
            // Check input parameters
            Utilities.Assert(name != null);

            LearningStoreEnumType foundEnumType;
            if(TryGetEnumTypeByName(name, out foundEnumType))
                return foundEnumType;
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, 
                Microsoft.LearningComponents.Storage.LearningStoreStrings.EnumTypeNotFoundInSchema, name));
        }

        /// <summary>
        /// Get a view by name
        /// </summary>
        /// <param name="name">Name of the view to find.</param>
        /// <returns>The found view.</returns>
        public LearningStoreView GetViewByName(string name)
        {
            // Check input parameters
            Utilities.Assert(name != null);

            LearningStoreView foundView;
            if (TryGetViewByName(name, out foundView))
                return foundView;
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, 
                Microsoft.LearningComponents.Storage.LearningStoreStrings.ViewNotFoundInSchema, name));
        }

        /// <summary>
        /// Create a new instance of <Typ>LearningStoreSchema</Typ> using data
        /// read from an XPathDocument
        /// </summary>
        /// <param name="document">The document</param>
        /// <returns>The new <Typ>LearningStoreSchema</Typ></returns>
        public static LearningStoreSchema CreateSchema(XPathDocument document)
        {
            // Check input parameters
            if (document == null)
                throw new LearningComponentsInternalException("LSTR2600");
                
            // Create a schema
            LearningStoreSchema schema = new LearningStoreSchema();

            // Read the stream
            try
            {
                // Create the navigator
                XPathNavigator navigator = document.CreateNavigator();

                // Verify the node name
                if(navigator.NodeType != XPathNodeType.Root)
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
                if(!navigator.MoveToChild(XPathNodeType.Element))
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
                if(navigator.Name != "StoreSchema")                
                    throw new InvalidOperationException(
                        Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                // Read the item types
                foreach (XPathNavigator subNode in navigator.SelectChildren("ItemType", String.Empty))
                {
                    // Read the item type information
                    LearningStoreItemType itemType =
                        LearningStoreItemType.CreateItemType(schema, subNode);

                    // Verify that it doesn't already exist
                    if (schema.m_itemTypesByName.ContainsKey(itemType.Name))
                        throw new InvalidOperationException(
                            Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                    // Add it
                    schema.m_itemTypes.Add(itemType);
                    schema.m_itemTypesByName.Add(itemType.Name, itemType);
                    
                    // Create the default view for the item type
                    LearningStoreView view =
                        LearningStoreView.CreateDefaultView(itemType);

                    // Verify that it doesn't already exist
                    if (schema.m_viewsByName.ContainsKey(view.Name))
                        throw new InvalidOperationException(
                            Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                    // Add it
                    schema.m_views.Add(view);
                    schema.m_viewsByName.Add(view.Name, view);
                }
                
                // Read the enum types
                foreach (XPathNavigator subNode in navigator.SelectChildren("Enum", String.Empty))
                {
                    // Read the enum information
                    LearningStoreEnumType enumType =
                        LearningStoreEnumType.CreateEnumType(schema, subNode);

                    // Verify that it doesn't already exist
                    if (schema.m_enumTypesByName.ContainsKey(enumType.Name))
                        throw new InvalidOperationException(
                            Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                    // Add it
                    schema.m_enumTypes.Add(enumType);
                    schema.m_enumTypesByName.Add(enumType.Name, enumType);
                }
                
                // Read the views
                foreach (XPathNavigator subNode in navigator.SelectChildren("View", String.Empty))
                {
                    // Read the view information
                    LearningStoreView view =
                        LearningStoreView.CreateView(schema, subNode);

                    // Verify that it doesn't already exist
                    if (schema.m_viewsByName.ContainsKey(view.Name))
                        throw new InvalidOperationException(
                            Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                    // Add it
                    schema.m_views.Add(view);
                    schema.m_viewsByName.Add(view.Name, view);
                }

                // Read the rights
                foreach (XPathNavigator subNode in navigator.SelectChildren("Right", String.Empty))
                {
                    // Read the right information
                    LearningStoreRight right =
                        LearningStoreRight.CreateRight(schema, subNode);

                    // Verify that it doesn't already exist
                    if (schema.m_rightsByName.ContainsKey(right.Name))
                        throw new InvalidOperationException(
                            Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);

                    // Add it
                    schema.m_rights.Add(right);
                    schema.m_rightsByName.Add(right.Name, right);
                }
            }
            catch (XmlException)
            {
                throw new InvalidOperationException(
                    Microsoft.LearningComponents.Storage.LearningStoreStrings.InvalidSchemaDefinition);
            }

            return schema;
        }
    }
}
