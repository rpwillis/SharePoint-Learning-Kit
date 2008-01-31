/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Resources;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

/*
 * Classes that represent information about a schema
 * Internal error numbers: 3000-3999
 */
namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents all the data about an enum read from schema files
    /// </summary>
    internal class EnumInfo
    {
        /// <summary>
        /// Name of the enum
        /// </summary>
        private string m_name;

        /// <summary>
        /// Value names
        /// </summary>
        private List<string> m_valueNames = new List<string>();
        
        /// <summary>
        /// Value integers
        /// </summary>
        private Dictionary<string, int> m_values = new Dictionary<string, int>();

        /// <summary>
        /// Documentation for each value.  If the value doesn't have any documentation,
        /// then a value doesn't exist in this dictionary.
        /// </summary>
        private Dictionary<string,string> m_documentationForValues = new Dictionary<string,string>();
        
        /// <summary>
        /// Is this enum in the base schema?
        /// </summary>
        private bool m_inBaseSchema;
        
        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;
                
        /// <summary>
        /// Constructor
        /// </summary>
        private EnumInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Value names
        /// </summary>
        public ReadOnlyCollection<string> ValueNames
        {
            get
            {
                return new ReadOnlyCollection<string>(m_valueNames);
            }
        }
        
        /// <summary>
        /// Returns true if the value is in the enum
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>True if the value is in the enum</returns>
        public bool HasValue(int value)
        {
            return m_values.ContainsValue(value);
        }
        
        /// <summary>
        /// Returns the integer value for a particular value name
        /// </summary>
        /// <param name="valueName">Value name</param>
        /// <returns>Integer value</returns>
        public int GetIntegerValue(string valueName)
        {
            return m_values[valueName];
        }

        /// <summary>
        /// Returns the name of the corresponding value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>The name</returns>
        public string GetValueName(int value)
        {
            foreach(KeyValuePair<string,int> kvp in m_values)
            {
                if(kvp.Value == value)
                    return kvp.Key;
            }

            throw new InternalException("SCMP3000");
        }
        
        /// <summary>
        /// Try to retrieve the documentation for a particular value name
        /// </summary>
        /// <param name="valueName">Value name</param>
        /// <param name="documentation">Location in which to store documentation</param>
        /// <returns>True if documentation was found</returns>
        public bool TryGetDocumentationForValue(string valueName, out string documentation)
        {
            return m_documentationForValues.TryGetValue(valueName, out documentation);
        }
        
        /// <summary>
        /// True if this enum was declared in the base schema
        /// </summary>        
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }
        
        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the Enum element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="inBaseSchema">True if the enum being loaded is from the base schema.</param>
        /// <returns>A new object containing the information.</returns>
        public static EnumInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver,
            bool inBaseSchema)
        {
            // Create the object
            EnumInfo en = new EnumInfo();
            en.m_inBaseSchema = inBaseSchema;
            
            // Get the name of the enum
            en.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Get the values
            XPathNodeIterator iterator = navigator.Select("s:Values/s:Value", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                // Read the name
                string valueName = SchemaFileUtil.ReadRequiredStringAttribute(childNavigator, "Name");

                // Read the value
                int valueValue = SchemaFileUtil.ReadRequiredInt32Attribute(childNavigator, "Value");

                // Remember it
                en.m_valueNames.Add(valueName);
                en.m_values.Add(valueName, valueValue);
                
                // Get the documentation
                XPathNavigator childDocumentation = childNavigator.SelectSingleNode("Documentation", resolver);
                if(childDocumentation != null)
                {
                    en.m_documentationForValues.Add(valueName, childDocumentation.InnerXml);
                }
            }

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if(documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if(summaryDocumentation != null)
                {
                    en.m_summaryDocumentation = summaryDocumentation.InnerXml;
                }
                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if(remarksDocumentation != null)
                {
                    en.m_remarksDocumentation = remarksDocumentation.InnerXml;
                }
            }
            
            return en;
        }
    }

    /// <summary>
    /// Represents all the data about a item type property from schema files
    /// </summary>
    internal class ItemTypePropertyInfo
    {
        /// <summary>
        /// The corresponding item type
        /// </summary>
        private ItemTypeInfo m_itemType;

        /// <summary>
        /// True if this property was declared in the base schema
        /// </summary>
        private bool m_inBaseSchema;
        
        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type code
        /// </summary>
        private LearningStoreValueTypeCode m_typeCode;

        /// <summary>
        /// Nullable?
        /// </summary>
        private bool m_nullable;
        
        /// <summary>
        /// Length, or null if a length wasn't defined
        /// </summary>
        private int? m_length;
        
        /// <summary>
        /// Cascade delete?
        /// </summary>
        private bool m_cascadeDelete;
        
        /// <summary>
        /// Referenced item type name, or null if none
        /// </summary>
        private string m_referencedItemTypeName;

        /// <summary>
        /// Enum name, or null if none
        /// </summary>
        private string m_enumName;

        /// <summary>
        /// True if this property has a default value
        /// </summary>
        private bool m_hasDefault;
        
        /// <summary>
        /// Default value.  Valid only when m_hasDefault=true.
        /// </summary>
        /// <remarks>
        /// The type of this object varies based on m_typeCode:
        /// - Xml: string
        /// - Value type (e.g., int, boolean): That type
        /// - Enum: An integer
        /// - ByteArray: byte[]
        /// </remarks>
        private object m_default;

        /// <summary>
        /// True if the default value is defined as a function
        /// </summary>
        private bool m_isDefaultAFunction;

        /// <summary>
        /// True if this property's RowGuid is true. Valid only for the LearningStore GUID datatype
        /// </summary>
        private bool m_rowGuidIsTrue;

        /// <summary>
        /// Constraints
        /// </summary>
        private List<string> m_constraints = new List<string>();
        
        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;

        /// <summary>
        /// Constructor
        /// </summary>
        private ItemTypePropertyInfo()
        {
        }

        /// <summary>
        /// Item type
        /// </summary>
        public ItemTypeInfo ItemType
        {
            get
            {
                return m_itemType;
            }
        }

        /// <summary>
        /// True if this property was declared in the base schema
        /// </summary>        
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Type code for value
        /// </summary>
        public LearningStoreValueTypeCode ValueTypeCode
        {
            get
            {
                return m_typeCode;
            }
        }

        /// <summary>
        /// True if the property is nullable
        /// </summary>        
        public bool Nullable
        {
            get
            {
                return m_nullable;
            }
        }

        /// <summary>
        /// True if the property has a default value which is a function
        /// </summary>

        public bool IsDefaultAFunction
        {
            get
            {
                return m_isDefaultAFunction;
            }
        }
        
        /// <summary>
        /// True if the GUID column is a RowGuidCol
        /// </summary>        
        public bool RowGuidIsTrue
        {
            get
            {
                return m_rowGuidIsTrue;
            }
        }

        /// <summary>
        /// Length, or null if there isn't a length
        /// </summary>
        public int? Length
        {
            get
            {
                return m_length;
            }
        }

        /// <summary>
        /// True if cascade delete is enabled
        /// </summary>        
        public bool CascadeDelete
        {
            get
            {
                return m_cascadeDelete;
            }
        }
        
        /// <summary>
        /// Referenced item type name
        /// </summary>
        public string ReferencedItemTypeName
        {
            get
            {
                return m_referencedItemTypeName;
            }
        }

        /// <summary>
        /// True if there is a default value
        /// </summary>        
        public bool HasDefaultValue
        {
            get
            {
                return m_hasDefault;
            }
        }

        /// <summary>
        /// Get the default value
        /// </summary>        
        public object GetDefaultValue()
        {
            // Nobody should be reading this if there isn't a default value
            if(!m_hasDefault)
                throw new InternalException("SCMP3010");

            // If this property references an enum and there's a default, verify that the value
            // is valid
            if ((m_typeCode == LearningStoreValueTypeCode.Enumeration) && (m_default != null))
            {
                if (!GetEnum().HasValue((int)m_default))
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.InvalidDefaultValue, m_name, m_itemType.Name));
            }
                
            return m_default;
        }
        
        /// <summary>
        /// Get the referenced item type
        /// </summary>
        /// <returns>The referenced item type, or null if there isn't one.</returns>
        public ItemTypeInfo GetReferencedItemType()
        {
            if (m_referencedItemTypeName == null)
                return null;

            ItemTypeInfo itemType;
            if(!m_itemType.Schema.TryGetItemTypeByName(m_referencedItemTypeName, out itemType))
                throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.ReferencedItemTypeNotFoundForProperty, m_name, m_itemType.Name, m_referencedItemTypeName));
            
            return itemType;
        }

        /// <summary>
        /// Get the enum
        /// </summary>
        /// <returns>The referenced enum, or null if there isn't one.</returns>
        public EnumInfo GetEnum()
        {
            if(m_enumName == null)
                return null;
                
            EnumInfo en;
            if (!m_itemType.Schema.TryGetEnumByName(m_enumName, out en))
                throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.EnumTypeNotFoundForProperty, m_name, m_itemType.Name, m_enumName));

            return en;
        }

        /// <summary>
        /// Get the constraints
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> Constraints
        {
            get
            {
                return new ReadOnlyCollection<string>(m_constraints);
            }
        }

        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// Perform extra validation once everything in the schema has been loaded
        /// </summary>
        public void Validate(object sender, EventArgs args)
        {
            // Verify that the referenced item type exists (if any)
            GetReferencedItemType();
            
            // Verify that the enum exists (if any)
            GetEnum();

            // If this property references an enum and there's a default, verify that the value
            // is valid
            if(m_hasDefault)
            {
                GetDefaultValue();
            }
        }

        /// <summary>
        /// Extend the property based on information from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ExtendProperty element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        public void Extend(XPathNavigator navigator, IXmlNamespaceResolver resolver)
        {
            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    m_summaryDocumentation += summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    m_remarksDocumentation += remarksDocumentation.InnerXml;
            }
        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the Column element.</param>
        /// <returns>A new object containing the information.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public static ItemTypePropertyInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver,
            ItemTypeInfo itemType, bool inBaseSchema)
        {
            // Create the object
            ItemTypePropertyInfo property = new ItemTypePropertyInfo();
            property.m_itemType = itemType;
            property.m_inBaseSchema = inBaseSchema;

            // Get the name of the column
            property.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Calculate the type code
            property.m_typeCode = SchemaFileUtil.TypeStringToTypeCode(SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Type"));

            // Get the referenced item name
            property.m_referencedItemTypeName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "ReferencedItemTypeName", null);
            if (property.m_typeCode == LearningStoreValueTypeCode.ItemIdentifier)
            {
                if (property.m_referencedItemTypeName == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ReferencedItemTypeAttributeNotFoundForProperty, property.m_name, itemType.Name));
            }
            else
            {
                if (property.m_referencedItemTypeName != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedReferencedItemTypeAttributeForProperty, property.m_name, itemType.Name));
            }

            // Get the enum name
            property.m_enumName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "EnumName", null);
            if (property.m_typeCode == LearningStoreValueTypeCode.Enumeration)
            {
                if (property.m_enumName == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.EnumAttributeNotFoundForProperty, property.m_name, itemType.Name));
            }
            else
            {
                if (property.m_enumName != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedEnumAttributeForProperty, property.m_name, itemType.Name));
            }

            // Get the length
            property.m_length = SchemaFileUtil.ReadOptionalInt32Attribute(navigator, "Length", null);
            if (property.m_typeCode == LearningStoreValueTypeCode.String)
            {
                if (property.m_length != null)
                {
                    if ((property.m_length < 1) || (property.m_length > SchemaFileUtil.GetMaxStringLength()))
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.InvalidLengthForStringProperty, property.m_name, itemType.Name));
                }
            }
            else if (property.m_typeCode == LearningStoreValueTypeCode.ByteArray)
            {
                if (property.m_length != null)
                {
                    if ((property.m_length < 1) || (property.m_length > SchemaFileUtil.GetMaxByteArrayLength()))
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.InvalidLengthForByteArrayProperty, property.m_name, itemType.Name));
                }
            }
            else
            {
                if (property.m_length != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedLengthAttributeForProperty, property.m_name, itemType.Name));
            }

            // Get the nullable flag
            property.m_nullable = (bool)SchemaFileUtil.ReadOptionalBooleanAttribute(navigator, "Nullable", false);

            // Get the rowguid flag
            property.m_rowGuidIsTrue = (bool)SchemaFileUtil.ReadOptionalBooleanAttribute(navigator, "RowGuid", false);
            
            // Get the cascade delete
            bool? cascadeDelete = SchemaFileUtil.ReadOptionalBooleanAttribute(navigator, "CascadeDelete", null);
            if (property.m_typeCode != LearningStoreValueTypeCode.ItemIdentifier)
            {
                if (cascadeDelete != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedCascadeDeleteAttributeForProperty, property.m_name, itemType.Name));
            }
            property.m_cascadeDelete = cascadeDelete ?? false;

            // Get the default value node
            XPathNavigator defaultValue = navigator.SelectSingleNode("s:Default", resolver);
            if(defaultValue != null)
            {
                // Found a default value
                property.m_hasDefault = true;
                
                // Determine if it is a function
                bool? isDefaultAFunction = SchemaFileUtil.ReadOptionalBooleanAttribute(defaultValue, "IsFunction", false);
                if (isDefaultAFunction == true)
                {
                    property.m_isDefaultAFunction = true;
                }

                // Determine if it is null
                bool? nullAttribute = SchemaFileUtil.ReadOptionalBooleanAttribute(defaultValue, "Null", null);
                if((nullAttribute == false) || (nullAttribute == null))
                {
                    defaultValue.MoveToChild(XPathNodeType.Text);
                    if(defaultValue == null)
                    {
                        // No value found
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.InvalidDefaultValue, property.m_name, itemType.Name));
                    }

                    if (isDefaultAFunction == true)
                    {
                        property.m_default = defaultValue.Value;
                    }
                    else
                    {
                        try
                        {
                            switch (property.m_typeCode)
                            {
                                case LearningStoreValueTypeCode.Boolean:
                                    property.m_default = defaultValue.ValueAsBoolean;
                                    break;
                                case LearningStoreValueTypeCode.ByteArray:
                                    property.m_default = Convert.FromBase64String(defaultValue.Value);
                                    break;
                                case LearningStoreValueTypeCode.DateTime:
                                    property.m_default = defaultValue.ValueAsDateTime;
                                    break;
                                case LearningStoreValueTypeCode.Double:
                                    property.m_default = defaultValue.ValueAsDouble;
                                    break;
                                case LearningStoreValueTypeCode.Enumeration:
                                    property.m_default = defaultValue.ValueAsInt;
                                    break;
                                case LearningStoreValueTypeCode.Guid:
                                    property.m_default = XmlConvert.ToGuid(defaultValue.Value);
                                    break;
                                case LearningStoreValueTypeCode.Int32:
                                    property.m_default = defaultValue.ValueAsInt;
                                    break;
                                case LearningStoreValueTypeCode.ItemIdentifier:
                                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                                        Resources.InvalidDefaultForItemIdentifierProperty, property.m_name, itemType.Name));
                                case LearningStoreValueTypeCode.Single:
                                    property.m_default = XmlConvert.ToSingle(defaultValue.Value);
                                    break;
                                case LearningStoreValueTypeCode.String:
                                    property.m_default = defaultValue.Value;
                                    break;
                                case LearningStoreValueTypeCode.Xml:
                                    property.m_default = defaultValue.Value;
                                    break;
                                default:
                                    throw new InternalException("SCMP3020");
                            }
                        }
                        catch (FormatException e)
                        {
                            throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                                Resources.InvalidDefaultValueWithError, property.m_name, itemType.Name, e.Message));
                        }
                    }
                }
                else
                {
                    if (!property.m_nullable)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.NullDefaultForNonNullableProperty, property.m_name, itemType.Name));
                    
                }
            }

            // Get the constraints
            XPathNodeIterator iterator = navigator.Select("s:Constraints/s:Constraint", resolver);
            foreach (XPathNavigator constraint in iterator)
            {
                constraint.MoveToChild(XPathNodeType.Text);
                if (constraint == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3030");
                property.m_constraints.Add(constraint.Value);
            }

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                {
                    property.m_summaryDocumentation = summaryDocumentation.InnerXml;
                }
                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                {
                    property.m_remarksDocumentation = remarksDocumentation.InnerXml;
                }
            }

            // Need to do more checking after all the other items are loaded
            itemType.Schema.Validate += new EventHandler<EventArgs>(property.Validate);

            return property;
        }
    }

    /// <summary>
    /// Represents all the data about an item type read from schema files
    /// </summary>
    internal class ItemTypeInfo
    {
        /// <summary>
        /// Schema
        /// </summary>
        private SchemaInfo m_schema;

        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Is this item type in the base schema?
        /// </summary>
        private bool m_inBaseSchema;

        /// <summary>
        /// Properties
        /// </summary>
        private List<ItemTypePropertyInfo> m_properties = new List<ItemTypePropertyInfo>();

        /// <summary>
        /// Expressions that give a user access to the default view
        /// </summary>
        private List<string> m_queryRightExpressions = new List<string>();

        /// <summary>
        /// Expressions that allow a user to delete an item
        /// </summary>        
        private List<string> m_deleteRightExpressions = new List<string>();
        
        /// <summary>
        /// Expressions that allow a user to add an item
        /// </summary>
        private List<string> m_addRightExpressions = new List<string>();

        /// <summary>
        /// Expressions that allow a user to update an item
        /// </summary>        
        private List<string> m_updateRightExpressions = new List<string>();

        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;
        
        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;
        
        /// <summary>
        /// Indexes
        /// </summary>
        private List<string> m_indexes = new List<string>();

        /// <summary>
        /// SqlAfter elements
        /// </summary>
        private List<string> m_sqlAfter = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        private ItemTypeInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Schema
        /// </summary>
        public SchemaInfo Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// True if this item type was declared in the base schema
        /// </summary>        
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Properties
        /// </summary>
        public ReadOnlyCollection<ItemTypePropertyInfo> Properties
        {
            get
            {
                // Bad thing to do if all the properties haven't been loaded yet
                if(!m_schema.Loaded)
                    throw new InternalException("SCMP3040");
                    
                return new ReadOnlyCollection<ItemTypePropertyInfo>(m_properties);
            }
        }

        /// <summary>
        /// Security for the default view
        /// </summary>
        public ReadOnlyCollection<string> QueryRightExpressions
        {
            get
            {
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3050");

                return new ReadOnlyCollection<string>(m_queryRightExpressions);
            }
        }

        /// <summary>
        /// Security for the delete operation
        /// </summary>
        public ReadOnlyCollection<string> DeleteRightExpressions
        {
            get
            {    
                
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3060");

                return new ReadOnlyCollection<string>(m_deleteRightExpressions);
            }
        }

        /// <summary>
        /// Security for the add operation
        /// </summary>
        public ReadOnlyCollection<string> AddRightExpressions
        {
            get
            {
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3070");

                return new ReadOnlyCollection<string>(m_addRightExpressions);
            }
        }

        /// <summary>
        /// Security for the update operation
        /// </summary>
        public ReadOnlyCollection<string> UpdateRightExpressions
        {
            get
            {
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3080");

                return new ReadOnlyCollection<string>(m_updateRightExpressions);
            }
        }

        /// <summary>
        /// Indexes
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> Indexes
        {
            get
            {
                // Bad thing to do if all the indexes haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3090");

                return new ReadOnlyCollection<string>(m_indexes);
            }
        }
        
        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// SqlAfter
        /// </summary>
        public ReadOnlyCollection<string> SqlAfter
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3095");

                return new ReadOnlyCollection<string>(m_sqlAfter);
            }
        }

        /// <summary>
        /// Extend the item based on information from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ExtendItemType element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="inBaseSchema">True if this is being extended from the base schema</param>
        public void Extend(XPathNavigator navigator, IXmlNamespaceResolver resolver, bool inBaseSchema)
        {
            // Delete the indexes if necessary
            if((bool)SchemaFileUtil.ReadOptionalBooleanAttribute(navigator, "ReplaceIndexes", false))
            {
                m_indexes.Clear();
            }

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    m_summaryDocumentation += summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    m_remarksDocumentation += remarksDocumentation.InnerXml;
            }

            // Get the expressions for security
            XPathNodeIterator iterator = navigator.Select("s:GrantQueryRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_queryRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantAddRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_addRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantUpdateRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_updateRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantDeleteRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_deleteRightExpressions.Add(childNavigator.Value);
            }

            // Get the properties
            iterator = navigator.Select("s:Properties/s:Property", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                // Load the property
                ItemTypePropertyInfo newProperty = ItemTypePropertyInfo.Load(childNavigator, resolver, this, inBaseSchema);

                // Verify that it doesn't already exist
                ItemTypePropertyInfo property = m_properties.Find(delegate(ItemTypePropertyInfo info)
                {
                    return info.Name == newProperty.Name;
                });
                if(property != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.PropertyAlreadyDefined, newProperty.Name, m_name));
                        
                // Add the property
                m_properties.Add(newProperty);
            }

            // Get the indexes
            iterator = navigator.Select("s:Indexes/s:Index", resolver);
            foreach (XPathNavigator index in iterator)
            {
                index.MoveToChild(XPathNodeType.Text);
                if (index == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3100");
                m_indexes.Add(index.Value);
            }
            
            // Get the extend properties
            iterator = navigator.Select("s:Properties/s:ExtendProperty", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                // Get the name
                string propertyName = SchemaFileUtil.ReadRequiredStringAttribute(childNavigator, "Name");

                // Find the property
                ItemTypePropertyInfo property = m_properties.Find(delegate(ItemTypePropertyInfo info)
                {
                    return info.Name == propertyName;
                });

                if (property == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.PropertyNotFound, propertyName, m_name));

                property.Extend(childNavigator, resolver);
            }

            // Get SqlAfter
            iterator = navigator.Select("s:SqlAfter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                childNavigator.MoveToChild(XPathNodeType.Text);
                if (childNavigator == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3105");
                m_sqlAfter.Add(childNavigator.Value);
            }

        }
                
        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ItemType element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="schema">Schema</param>
        /// <param name="inBaseSchema">True if the item type being loaded is from the base schema.</param>
        /// <returns>A new object containing the information.</returns>
        public static ItemTypeInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver,
            SchemaInfo schema, bool inBaseSchema)
        {
            // Create the object
            ItemTypeInfo itemType = new ItemTypeInfo();
            itemType.m_inBaseSchema = inBaseSchema;

            // Get the name of the item type
            itemType.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");
            itemType.m_schema = schema;

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if(summaryDocumentation != null)
                    itemType.m_summaryDocumentation = summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    itemType.m_remarksDocumentation = remarksDocumentation.InnerXml;
            }
            
            // Get the select statements for security
            XPathNodeIterator iterator = navigator.Select("s:GrantQueryRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                itemType.m_queryRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantAddRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                itemType.m_addRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantUpdateRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                itemType.m_updateRightExpressions.Add(childNavigator.Value);
            }
            iterator = navigator.Select("s:GrantDeleteRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                itemType.m_deleteRightExpressions.Add(childNavigator.Value);
            }

            // Get the properties
            iterator = navigator.Select("s:Properties/s:Property", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                itemType.m_properties.Add(ItemTypePropertyInfo.Load(childNavigator, resolver, itemType, inBaseSchema));
            }

            // Get the indexes
            iterator = navigator.Select("s:Indexes/s:Index", resolver);
            foreach (XPathNavigator index in iterator)
            {
                index.MoveToChild(XPathNodeType.Text);
                if (index == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3110");
                itemType.m_indexes.Add(index.Value);
            }

            // Get SqlAfter
            iterator = navigator.Select("s:SqlAfter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                childNavigator.MoveToChild(XPathNodeType.Text);
                if (childNavigator == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3115");
                itemType.m_sqlAfter.Add(childNavigator.Value);
            }

            return itemType;
        }
    }

    /// <summary>
    /// Represents all the data about a view column directly defined in a schema file
    /// </summary>
    internal class ViewColumnInfo
    {
        /// <summary>
        /// The corresponding view
        /// </summary>
        private ViewInfo m_view;

        /// <summary>
        /// Is this column in the base schema?
        /// </summary>
        private bool m_inBaseSchema;

        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type code
        /// </summary>
        private LearningStoreValueTypeCode m_typeCode;

        /// <summary>
        /// Referenced item type name
        /// </summary>
        private string m_referencedItemTypeName;

        /// <summary>
        /// Enum name
        /// </summary>
        private string m_enumName;

        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;

        /// <summary>
        /// Constructor
        /// </summary>
        private ViewColumnInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// True if this column was declared in the base schema
        /// </summary>        
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Type code for value
        /// </summary>
        public LearningStoreValueTypeCode ValueTypeCode
        {
            get
            {
                return m_typeCode;
            }
        }

        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// View
        /// </summary>
        public ViewInfo View
        {
            get
            {
                return m_view;
            }
        }
        
        /// <summary>
        /// Referenced item type
        /// </summary>        
        public ItemTypeInfo GetReferencedItemType()
        {
            if (m_referencedItemTypeName == null)
                return null;

            ItemTypeInfo itemType;
            if (!m_view.Schema.TryGetItemTypeByName(m_referencedItemTypeName, out itemType))
                throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.ReferencedItemTypeNotFoundInColumn, m_name, m_view.Name, m_referencedItemTypeName));

            return itemType;        
        }

        /// <summary>
        /// Enum
        /// </summary>        
        public EnumInfo GetEnum()
        {
            if(m_enumName == null)
                return null;
                
            EnumInfo en;
            if (!m_view.Schema.TryGetEnumByName(m_enumName, out en))
                throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.EnumTypeNotFoundInColumn, m_name, m_view.Name, m_enumName));

            return en;
        }

        /// <summary>
        /// Perform extra validation once everything in the schema has been loaded
        /// </summary>
        public void Validate(object sender, EventArgs args)
        {
            // If this column references an item type, verify that the item type exists
            GetReferencedItemType();

            // If this column references an enum, verify that the enum exists
            GetEnum();
        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the Column element.</param>
        /// <param name="resolver">Resolver</param>
        /// <param name="view">View</param>
        /// <param name="inBaseSchema">True if this column is being loaded from the base schema</param>
        /// <returns>A new object containing the information.</returns>
        public static ViewColumnInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver,
            ViewInfo view, bool inBaseSchema)
        {
            // Create the object
            ViewColumnInfo column = new ViewColumnInfo();
            column.m_view = view;
            column.m_inBaseSchema = inBaseSchema;

            // Get the name of the column
            column.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Calculate the type code
            column.m_typeCode = SchemaFileUtil.TypeStringToTypeCode(SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Type"));

            // Get the referenced item name
            column.m_referencedItemTypeName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "ReferencedItemTypeName", null);
            if (column.m_typeCode == LearningStoreValueTypeCode.ItemIdentifier)
            {
                if (column.m_referencedItemTypeName == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ReferencedItemTypeAttributeNotFoundInColumn, column.m_name, view.Name));
            }
            else
            {
                if (column.m_referencedItemTypeName != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedReferencedItemTypeAttributeInColumn, column.m_name, view.Name));
            }

            // Get the enum name
            column.m_enumName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "EnumName", null);
            if (column.m_typeCode == LearningStoreValueTypeCode.Enumeration)
            {
                if (column.m_enumName == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.EnumAttributeNotFoundInColumn, column.m_name, view.Name));
            }
            else
            {
                if (column.m_enumName != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.UnexpectedEnumAttributeInColumn, column.m_name, view.Name));
            }

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    column.m_summaryDocumentation = summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    column.m_remarksDocumentation = remarksDocumentation.InnerXml;
            }

            // Need to do more checking after all the other items are loaded
            view.Schema.Validate += new EventHandler<EventArgs>(column.Validate);

            return column;
        }
    }

    /// <summary>
    /// Represents all the data about a parameter on a view or right
    /// </summary>
    internal class ParameterInfo
    {
        /// <summary>
        /// The view, if this is a parameter on a view
        /// </summary>
        private ViewInfo m_view;

        /// <summary>
        /// The right, if this is a parameter on a right
        /// </summary>
        private RightInfo m_right;
        
        /// <summary>
        /// Is this parameter in the base schema?
        /// </summary>
        private bool m_inBaseSchema;

        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Type code
        /// </summary>
        private LearningStoreValueTypeCode m_typeCode;

        /// <summary>
        /// Referenced item type name
        /// </summary>
        private string m_referencedItemTypeName;

        /// <summary>
        /// Enum name
        /// </summary>
        private string m_enumName;

        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;

        /// <summary>
        /// Constructor
        /// </summary>
        private ParameterInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Schema
        /// </summary>
        private SchemaInfo Schema
        {
            get
            {
                if(m_view != null)
                    return m_view.Schema;
                else if(m_right != null)
                    return m_right.Schema;
                else
                    throw new InternalException("SCMP3120");
            }
        }
        
        /// <summary>
        /// True if this column was declared in the base schema
        /// </summary>        
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Type code for value
        /// </summary>
        public LearningStoreValueTypeCode ValueTypeCode
        {
            get
            {
                return m_typeCode;
            }
        }

        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// View, or null if this parameter isn't on a view
        /// </summary>
        public ViewInfo View
        {
            get
            {
                return m_view;
            }
        }

        /// <summary>
        /// Right, or null if this parameter isn't on a right
        /// </summary>
        public RightInfo Right
        {
            get
            {
                return m_right;
            }
        }
        
        /// <summary>
        /// Referenced item type
        /// </summary>        
        public ItemTypeInfo GetReferencedItemType()
        {
            if (m_referencedItemTypeName == null)
                return null;

            ItemTypeInfo itemType;
            if (!Schema.TryGetItemTypeByName(m_referencedItemTypeName, out itemType))
            {
                if(m_view != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ReferencedItemTypeNotFoundInViewParameter, m_name, m_view.Name, m_referencedItemTypeName));
                else if(m_right != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ReferencedItemTypeNotFoundInRightParameter, m_name, m_right.Name, m_referencedItemTypeName));
                else
                    throw new InternalException("SCMP3130");
            }
            return itemType;
        }

        /// <summary>
        /// Enum
        /// </summary>        
        public EnumInfo GetEnum()
        {
            if (m_enumName == null)
                return null;

            EnumInfo en;
            if (!Schema.TryGetEnumByName(m_enumName, out en))
            {
                if(m_view != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.EnumTypeNotFoundInViewParameter, m_name, m_view.Name, m_enumName));
                else if(m_right != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.EnumTypeNotFoundInRightParameter, m_name, m_right.Name, m_enumName));
                else
                    throw new InternalException("SCMP3140");
            }                                
            return en;
        }

        /// <summary>
        /// Perform extra validation once everything in the schema has been loaded
        /// </summary>
        public void Validate(object sender, EventArgs args)
        {
            // If this parameter references an item type, verify that the item type exists
            GetReferencedItemType();

            // If this parameter references an enum, verify that the enum exists
            GetEnum();
        }

        /// <summary>
        /// Extend the parameter based on information from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ExtendParameter element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        public void Extend(XPathNavigator navigator, IXmlNamespaceResolver resolver)
        {
            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    m_summaryDocumentation += summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    m_remarksDocumentation += remarksDocumentation.InnerXml;
            }
        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the Parameter element.</param>
        /// <param name="resolver">Resolver</param>
        /// <param name="view">View, or null if not attached to a view</param>
        /// <param name="right">Right, or null if not attached to a right</param>
        /// <param name="inBaseSchema">True if this parameter is being loaded from the base schema</param>
        /// <returns>A new object containing the information.</returns>
        public static ParameterInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver,
            ViewInfo view, RightInfo right, bool inBaseSchema)
        {
            if((view == null) && (right == null))
                throw new InternalException("SCMP3150");
            if((view != null) && (right != null))
                throw new InternalException("SCMP3160");
            
            // Create the object
            ParameterInfo parameter = new ParameterInfo();
            parameter.m_view = view;
            parameter.m_right = right;
            parameter.m_inBaseSchema = inBaseSchema;

            // Get the name of the parameter
            parameter.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Calculate the type code
            parameter.m_typeCode = SchemaFileUtil.TypeStringToTypeCode(SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Type"));

            // Get the referenced item name
            parameter.m_referencedItemTypeName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "ReferencedItemTypeName", null);
            if (parameter.m_typeCode == LearningStoreValueTypeCode.ItemIdentifier)
            {
                if (parameter.m_referencedItemTypeName == null)
                {
                    if(view != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.ReferencedItemTypeAttributeNotFoundInViewParameter, parameter.m_name, view.Name));
                    else if(right != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.ReferencedItemTypeAttributeNotFoundInRightParameter, parameter.m_name, right.Name));
                    else
                        throw new InternalException("SCMP3170");
                }
            }
            else
            {
                if (parameter.m_referencedItemTypeName != null)
                {
                    if(view != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.UnexpectedReferencedItemTypeAttributeInViewParameter, parameter.m_name, view.Name));
                    else if(right != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.UnexpectedReferencedItemTypeAttributeInRightParameter, parameter.m_name, right.Name));
                    else
                        throw new InternalException("SCMP3180");
                }
            }

            // Get the enum name
            parameter.m_enumName = SchemaFileUtil.ReadOptionalStringAttribute(navigator, "EnumName", null);
            if (parameter.m_typeCode == LearningStoreValueTypeCode.Enumeration)
            {
                if (parameter.m_enumName == null)
                {
                    if(view != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.EnumAttributeNotFoundInViewParameter, parameter.m_name, view.Name));
                    else if(right != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.EnumAttributeNotFoundInRightParameter, parameter.m_name, right.Name));
                    else
                        throw new InternalException("SCMP3190");
                }
            }
            else
            {
                if (parameter.m_enumName != null)
                {
                    if(view != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.UnexpectedEnumAttributeInViewParameter, parameter.m_name, view.Name));
                    else if(right != null)
                        throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.UnexpectedEnumAttributeInRightParameter, parameter.m_name, right.Name));
                    else
                        throw new InternalException("SCMP3200");
                }
            }

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    parameter.m_summaryDocumentation = summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    parameter.m_remarksDocumentation = remarksDocumentation.InnerXml;
            }

            // Need to do more checking after all the other items are loaded
            if(view != null)
                view.Schema.Validate += new EventHandler<EventArgs>(parameter.Validate);
            else if(right != null)
                right.Schema.Validate += new EventHandler<EventArgs>(parameter.Validate);
            else
                throw new InternalException("SCMP3210");             

            return parameter;
        }
    }

    /// <summary>
    /// Represents all the data about a view directly defined in a schema file
    /// </summary>
    internal class ViewInfo
    {
        /// <summary>
        /// Schema
        /// </summary>
        private SchemaInfo m_schema;

        /// <summary>
        /// Was this view declared in the base schema?
        /// </summary>
        private bool m_inBaseSchema;
        
        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Columns
        /// </summary>
        private List<ViewColumnInfo> m_columns = new List<ViewColumnInfo>();

        /// <summary>
        /// Parameters
        /// </summary>
        private List<ParameterInfo> m_parameters = new List<ParameterInfo>();
        
        /// <summary>
        /// Select statement that defines the view
        /// </summary>
        private string m_implementation;

        /// <summary>
        /// List of expressions used for security
        /// </summary>
        private List<string> m_rightExpressions = new List<string>();

        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;

        /// <summary>
        /// Constructor
        /// </summary>
        private ViewInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Schema
        /// </summary>
        public SchemaInfo Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Was this view declared in the base schema?
        /// </summary>
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// Columns
        /// </summary>
        public ReadOnlyCollection<ViewColumnInfo> Columns
        {
            get
            {
                // Bad thing to do if all the columns haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3220");                    
            
                return new ReadOnlyCollection<ViewColumnInfo>(m_columns);
            }
        }

        /// <summary>
        /// Parameters
        /// </summary>
        public ReadOnlyCollection<ParameterInfo> Parameters
        {
            get
            {
                // Bad thing to do if all the parameters haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3230");

                return new ReadOnlyCollection<ParameterInfo>(m_parameters);
            }
        }

        /// <summary>
        /// SQL text that defines the view
        /// </summary>        
        public string Implementation        
        {
            get
            {
                return m_implementation;
            }
        }

        /// <summary>
        /// Expressions used for security
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> RightExpressions
        {
            get
            {
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3240");

                return new ReadOnlyCollection<string>(m_rightExpressions);    
            }
        }

        /// <summary>
        /// Extend the view based on information from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ExtendView element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        public void Extend(XPathNavigator navigator, IXmlNamespaceResolver resolver)
        {
            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    m_summaryDocumentation += summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    m_remarksDocumentation += remarksDocumentation.InnerXml;
            }

            // Get the expressions for security
            XPathNodeIterator iterator = navigator.Select("s:GrantQueryRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_rightExpressions.Add(childNavigator.Value);
            }
        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the View element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="schema">Schema</param>
        /// <param name="inBaseSchema">Is this view being loaded from the base schema?</param>
        /// <returns>A new object containing the information.</returns>
        public static ViewInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver, SchemaInfo schema,
            bool inBaseSchema)
        {
            // Create the object
            ViewInfo view = new ViewInfo();
            view.m_schema = schema;
            view.m_inBaseSchema = inBaseSchema;

            // Get the name of the view
            view.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Get the implementation
            XPathNavigator implementation = navigator.SelectSingleNode("s:Implementation", resolver);
            if (implementation == null)
                // XSD should've caught this
                throw new InternalException("SCMP3250");
            implementation.MoveToChild(XPathNodeType.Text);
            if (implementation == null)
                // XSD should've caught this
                throw new InternalException("SCMP3260");
            view.m_implementation = implementation.Value;

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    view.m_summaryDocumentation = summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    view.m_remarksDocumentation = remarksDocumentation.InnerXml;
            }

            // Get the expressions for security
            XPathNodeIterator iterator = navigator.Select("s:GrantQueryRight/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                view.m_rightExpressions.Add(childNavigator.Value);
            }

            // Get the columns
            iterator = navigator.Select("s:Columns/s:Column", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                view.m_columns.Add(ViewColumnInfo.Load(childNavigator, resolver, view, inBaseSchema));
            }

            // Get the parameters
            iterator = navigator.Select("s:Parameters/s:Parameter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                view.m_parameters.Add(ParameterInfo.Load(childNavigator, resolver, view, null, inBaseSchema));
            }

            return view;
        }
    }

    /// <summary>
    /// Represents all the data about a right defined in a schema file
    /// </summary>
    internal class RightInfo
    {
        /// <summary>
        /// Schema
        /// </summary>
        private SchemaInfo m_schema;

        /// <summary>
        /// Was this right declared in the base schema?
        /// </summary>
        private bool m_inBaseSchema;

        /// <summary>
        /// Name
        /// </summary>
        private string m_name;

        /// <summary>
        /// Parameters
        /// </summary>
        private List<ParameterInfo> m_parameters = new List<ParameterInfo>();

        /// <summary>
        /// List of expressions used for security
        /// </summary>
        private List<string> m_rightExpressions = new List<string>();

        /// <summary>
        /// Summary documentation, or null if none
        /// </summary>
        private string m_summaryDocumentation;

        /// <summary>
        /// Remarks documentation, or null if none
        /// </summary>
        private string m_remarksDocumentation;

        /// <summary>
        /// Constructor
        /// </summary>
        private RightInfo()
        {
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Schema
        /// </summary>
        public SchemaInfo Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Was this right declared in the base schema?
        /// </summary>
        public bool InBaseSchema
        {
            get
            {
                return m_inBaseSchema;
            }
        }

        /// <summary>
        /// Summary documentation
        /// </summary>
        public string SummaryDocumentation
        {
            get
            {
                return m_summaryDocumentation;
            }
        }

        /// <summary>
        /// Remarks documentation
        /// </summary>
        public string RemarksDocumentation
        {
            get
            {
                return m_remarksDocumentation;
            }
        }

        /// <summary>
        /// Parameters
        /// </summary>
        public ReadOnlyCollection<ParameterInfo> Parameters
        {
            get
            {
                // Bad thing to do if all the parameters haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3270");

                return new ReadOnlyCollection<ParameterInfo>(m_parameters);
            }
        }

        /// <summary>
        /// Expressions used for security
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> RightExpressions
        {
            get
            {
                // Bad thing to do if all the expressions haven't been loaded yet
                if (!m_schema.Loaded)
                    throw new InternalException("SCMP3280");

                return new ReadOnlyCollection<string>(m_rightExpressions);
            }
        }

        /// <summary>
        /// Extend the right based on information from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the ExtendRight element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="inBaseSchema">Is this right being extended from the base schema?</param>
        public void Extend(XPathNavigator navigator, IXmlNamespaceResolver resolver, bool inBaseSchema)
        {
            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    m_summaryDocumentation += summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    m_remarksDocumentation += remarksDocumentation.InnerXml;
            }

            // Get the expressions for security
            XPathNodeIterator iterator = navigator.Select("s:Grant/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                m_rightExpressions.Add(childNavigator.Value);
            }

            // Get the parameters
            iterator = navigator.Select("s:Parameters/s:Parameter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                // Load the parameter
                ParameterInfo newParameter = ParameterInfo.Load(childNavigator, resolver, null, this, inBaseSchema);

                // Verify that it doesn't already exist
                ParameterInfo parameter = m_parameters.Find(delegate(ParameterInfo info)
                {
                    return info.Name == newParameter.Name;
                });
                if (parameter != null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ParameterAlreadyDefinedInRight, newParameter.Name, m_name));

                // Add the parameter
                m_parameters.Add(newParameter);
            }

            // Get the extend parameters
            iterator = navigator.Select("s:Parameters/s:ExtendParameter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                // Get the name
                string parameterName = SchemaFileUtil.ReadRequiredStringAttribute(childNavigator, "Name");

                // Find the parameter
                ParameterInfo parameter = m_parameters.Find(delegate(ParameterInfo info)
                {
                    return info.Name == parameterName;
                });

                if (parameter == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.ParameterNotFoundInRight, parameterName, m_name));

                parameter.Extend(childNavigator, resolver);
            }

        }

        /// <summary>
        /// Load the data from an XPathNavigator
        /// </summary>
        /// <param name="navigator">Navigator.  Positioned on the Right element.</param>
        /// <param name="resolver">Namespace resolver.</param>
        /// <param name="schema">Schema</param>
        /// <param name="inBaseSchema">Is this right being loaded from the base schema?</param>
        /// <returns>A new object containing the information.</returns>
        public static RightInfo Load(XPathNavigator navigator, IXmlNamespaceResolver resolver, SchemaInfo schema,
            bool inBaseSchema)
        {
            // Create the object
            RightInfo right = new RightInfo();
            right.m_schema = schema;
            right.m_inBaseSchema = inBaseSchema;

            // Get the name of the view
            right.m_name = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

            // Get the documentation
            XPathNavigator documentation = navigator.SelectSingleNode("s:Documentation", resolver);
            if (documentation != null)
            {
                XPathNavigator summaryDocumentation = documentation.SelectSingleNode("Summary", resolver);
                if (summaryDocumentation != null)
                    right.m_summaryDocumentation = summaryDocumentation.InnerXml;

                XPathNavigator remarksDocumentation = documentation.SelectSingleNode("Remarks", resolver);
                if (remarksDocumentation != null)
                    right.m_remarksDocumentation = remarksDocumentation.InnerXml;
            }

            // Get the expressions for security
            XPathNodeIterator iterator = navigator.Select("s:Grant/s:Expression/text()", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                right.m_rightExpressions.Add(childNavigator.Value);
            }

            // Get the parameters
            iterator = navigator.Select("s:Parameters/s:Parameter", resolver);
            foreach (XPathNavigator childNavigator in iterator)
            {
                right.m_parameters.Add(ParameterInfo.Load(childNavigator, resolver, null, right, inBaseSchema));
            }

            return right;
        }
    }

    /// <summary>
    /// Represents a schema
    /// </summary>
    internal class SchemaInfo
    {
        /// <summary>
        /// True if all of the data has been loaded
        /// </summary>
        /// <remarks>
        /// Used to prevent bugs e.g., some code asking for an enum that might exist
        /// but hasn't been loaded yet.
        /// </remarks>
        private bool m_allLoaded;

        /// <summary>
        /// Enums
        /// </summary>    
        private List<EnumInfo> m_enums = new List<EnumInfo>();

        /// <summary>
        /// Item types
        /// </summary>    
        private List<ItemTypeInfo> m_itemTypes = new List<ItemTypeInfo>();

        /// <summary>
        /// Rights
        /// </summary>
        private List<RightInfo> m_rights = new List<RightInfo>();
        
        /// <summary>
        /// Views
        /// </summary>    
        private List<ViewInfo> m_views = new List<ViewInfo>();

        /// <summary>
        /// SqlBefore elements
        /// </summary>
        private List<string> m_sqlBefore = new List<string>();

        /// <summary>
        /// SqlAfter elements
        /// </summary>
        private List<string> m_sqlAfter = new List<string>();

        /// <summary>
        /// List of all names used by enums, item types, views, etc.
        /// </summary>
        private List<string> m_names = new List<string>();
        
        /// <summary>
        /// Constructor
        /// </summary>
        private SchemaInfo()
        {
        }
        
        /// <summary>
        /// Event raised when everything in the schema files have been loaded and validation should
        /// happen
        /// </summary>
        public event EventHandler<EventArgs> Validate;

        /// <summary>
        /// True if the entire schema has been loaded
        /// </summary>
        public bool Loaded
        {
            get
            {
                return m_allLoaded;
            }
        }
                
        /// <summary>
        /// Try to retrieve an item type by name
        /// </summary>
        /// <param name="itemTypeName">Item type name.</param>
        /// <param name="itemTypeInfo">Location in which to store the information about the item type.</param>
        /// <returns>True if the item type is found</returns>
        public bool TryGetItemTypeByName(string itemTypeName, out ItemTypeInfo itemType)
        {
            // Bad thing to do if everything hasn't been loaded yet
            if (!m_allLoaded)
                throw new InternalException("SCMP3290");
            
            itemType = m_itemTypes.Find(delegate(ItemTypeInfo info)
            {
                return info.Name == itemTypeName;
            });
            
            return itemType != null;                
        }

        /// <summary>
        /// Try to retrieve an enum by name
        /// </summary>
        /// <param name="itemTypeName">Enum name.</param>
        /// <param name="itemTypeInfo">Location in which to store the information about the enum.</param>
        /// <returns>True if the enum is found</returns>
        public bool TryGetEnumByName(string enumName, out EnumInfo en)
        {
            // Bad thing to do if everything hasn't been loaded yet
            if (!m_allLoaded)
                throw new InternalException("SCMP3300");

            en = m_enums.Find(delegate(EnumInfo info)
            {
                return info.Name == enumName;
            });

            return en != null;
        }

        /// <summary>
        /// Enums
        /// </summary>
        public ReadOnlyCollection<EnumInfo> Enums
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3310");

                return new ReadOnlyCollection<EnumInfo>(m_enums);
            }
        }

        /// <summary>
        /// Item types
        /// </summary>
        public ReadOnlyCollection<ItemTypeInfo> ItemTypes
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3320");

                return new ReadOnlyCollection<ItemTypeInfo>(m_itemTypes);
            }
        }

        /// <summary>
        /// Rights
        /// </summary>
        public ReadOnlyCollection<RightInfo> Rights
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3330");

                return new ReadOnlyCollection<RightInfo>(m_rights);
            }
        }

        /// <summary>
        /// Views
        /// </summary>
        public ReadOnlyCollection<ViewInfo> Views
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3340");

                return new ReadOnlyCollection<ViewInfo>(m_views);
            }
        }

        /// <summary>
        /// SqlBefore
        /// </summary>
        public ReadOnlyCollection<string> SqlBefore
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3341");

                return new ReadOnlyCollection<string>(m_sqlBefore);
            }
        }

        /// <summary>
        /// SqlAfter
        /// </summary>
        public ReadOnlyCollection<string> SqlAfter
        {
            get
            {
                // Bad thing to do if everything hasn't been loaded yet
                if (!m_allLoaded)
                    throw new InternalException("SCMP3342");

                return new ReadOnlyCollection<string>(m_sqlAfter);
            }
        }

        /// <summary>
        /// Adds the information from a schema XML file to the passed-in schema object
        /// </summary>
        /// <param name="schema">Destination of schema information read from
        ///     the XML file.</param>
        /// <param name="stream">Stream containing the XML file.</param>
        /// <param name="baseSchema">True if the XML file is the base schema.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        private static void ApplyFileToSchema(SchemaInfo schema, Stream stream,
            bool baseSchema)
        {
            // Load the document
            XPathDocument doc = SchemaFileUtil.LoadSchemaFileWithValidation(stream);
            
            // Create the namespace manager
            XPathNavigator docNavigator = doc.CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(docNavigator.NameTable);
            manager.AddNamespace("s", "urn:schemas-microsoft-com:learning-components:learning-store-schema");

            // Move the document navigator to the root element
            docNavigator = docNavigator.SelectSingleNode("/s:StoreSchema", manager);
            if(docNavigator == null)
                throw new ValidationException(Resources.RootElementNotFound);
            
            // Create the information about the enums
            XPathNodeIterator iterator = docNavigator.Select("s:Enum", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                EnumInfo en = EnumInfo.Load(navigator, manager, baseSchema);

                if(schema.m_names.Contains(en.Name))
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.DuplicateIdentifier, en.Name));

                schema.m_enums.Add(en);
                schema.m_names.Add(en.Name);
            }

            // Create the information about the item types
            iterator = docNavigator.Select("s:ItemType", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                ItemTypeInfo itemType = ItemTypeInfo.Load(navigator, manager, schema, baseSchema);
                
                if(schema.m_names.Contains(itemType.Name))
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.DuplicateIdentifier, itemType.Name));

                schema.m_itemTypes.Add(itemType);
                schema.m_names.Add(itemType.Name);
            }

            // Create the information about the views
            iterator = docNavigator.Select("s:View", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                ViewInfo view = ViewInfo.Load(navigator, manager, schema, baseSchema);

                if(schema.m_names.Contains(view.Name))
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.DuplicateIdentifier, view.Name));

                schema.m_views.Add(view);
                schema.m_names.Add(view.Name);                
            }

            // Create the information about the rights
            iterator = docNavigator.Select("s:Right", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                RightInfo right = RightInfo.Load(navigator, manager, schema, baseSchema);

                if (schema.m_names.Contains(right.Name))
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.DuplicateIdentifier, right.Name));

                schema.m_rights.Add(right);
                schema.m_names.Add(right.Name);
            }

            // Extend item types
            iterator = docNavigator.Select("s:ExtendItemType", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                // Get the name
                string itemTypeName = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");
           
                // Find the item type
                ItemTypeInfo itemType = schema.m_itemTypes.Find(delegate(ItemTypeInfo info)
                {
                    return info.Name == itemTypeName;
                });

                if(itemType == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.IdentifierNotFound, itemTypeName));

                itemType.Extend(navigator, manager, baseSchema);
            }
            
            // Extend views
            iterator = docNavigator.Select("s:ExtendView", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                // Get the name
                string viewName = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

                // Find the item type
                ViewInfo view = schema.m_views.Find(delegate(ViewInfo info)
                {
                    return info.Name == viewName;
                });

                if (view == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.IdentifierNotFound, viewName));

                view.Extend(navigator, manager);
            }

            // Extend rights
            iterator = docNavigator.Select("s:ExtendRight", manager);
            foreach (XPathNavigator navigator in iterator)
            {
                // Get the name
                string rightName = SchemaFileUtil.ReadRequiredStringAttribute(navigator, "Name");

                // Find the item type
                RightInfo right = schema.m_rights.Find(delegate(RightInfo info)
                {
                    return info.Name == rightName;
                });

                if (right == null)
                    throw new ValidationException(String.Format(CultureInfo.CurrentCulture,
                        Resources.IdentifierNotFound, rightName));

                right.Extend(navigator, manager, baseSchema);
            }

            // Get SqlBefore
            iterator = docNavigator.Select("s:SqlBefore", manager);
            foreach (XPathNavigator childNavigator in iterator)
            {
                childNavigator.MoveToChild(XPathNodeType.Text);
                if (childNavigator == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3345");
                schema.m_sqlBefore.Add(childNavigator.Value);
            }

            // Get SqlAfter
            iterator = docNavigator.Select("s:SqlAfter", manager);
            foreach (XPathNavigator childNavigator in iterator)
            {
                childNavigator.MoveToChild(XPathNodeType.Text);
                if (childNavigator == null)
                    // XSD should've caught this
                    throw new InternalException("SCMP3347");
                schema.m_sqlAfter.Add(childNavigator.Value);
            }


        }

        /// <summary>
        /// Create a schema
        /// </summary>
        /// <param name="baseSchemaPath">Path to the base schema.  If null, then the
        ///     base schema built into the executable is used.</param>
        /// <param name="inputPath">Path to the schema input file, or null if there isn't one.</param>
        /// <returns>A new SchemaInfo object representing the schema</returns>
        private static SchemaInfo InternalCreateFrom(string baseSchemaPath,
            string inputPath)
        {
            // Create the object
            SchemaInfo schema = new SchemaInfo();

            // Load the base schema information
            Stream baseStream;
            if (baseSchemaPath == null)
            {
                baseStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "SchemaCompiler.BaseSchema.xml");
            }
            else
            {
                baseStream = new FileStream(baseSchemaPath, FileMode.Open, FileAccess.Read);
            }
            using(baseStream)
            {
                ApplyFileToSchema(schema, baseStream, true);
            }
            
            // Load information from the provided schema
            if(inputPath != null)
            {
                Stream stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
                using(stream)
                {
                    ApplyFileToSchema(schema, stream, false);
                }
            }
            
            // All loaded
            schema.m_allLoaded = true;
            
            // Perform validation
            EventHandler<EventArgs> tempEvent = schema.Validate;
            if(tempEvent != null)
                tempEvent(schema, new EventArgs());
                
            return schema;            
        }
        
        /// <summary>
        /// Create a schema from the base schema definition
        /// </summary>
        /// <param name="baseSchemaPath">Path to the base schema.  If null, then the
        ///     base schema built into the executable is used.</param>
        /// <returns>A new SchemaInfo object representing the schema</returns>
        public static SchemaInfo CreateFromBaseSchema(string baseSchemaPath)
        {
            return InternalCreateFrom(baseSchemaPath, null);
        }
        
        /// <summary>
        /// Create a schema from the base schema definition and a file
        /// </summary>
        /// <param name="baseSchemaPath">Path to the base schema.  If null, then the
        ///     base schema built into the executable is used.</param>
        /// <param name="inputPath">Path to the schema input file</param>
        /// <returns>A new SchemaInfo object representing the schema</returns>
        public static SchemaInfo CreateFromBaseSchemaAndFile(string baseSchemaPath, string inputPath)
        {
            return InternalCreateFrom(baseSchemaPath, inputPath);
        }
    }

    /// <summary>
    /// Helper methods used to read information from schema definition files
    /// </summary>
    internal static class SchemaFileUtil
    {
        /// <summary>
        /// Load a schema file with XSD validation
        /// </summary>
        /// <param name="stream">Stream containing the file</param>
        /// <returns>The document</returns>
        public static XPathDocument LoadSchemaFileWithValidation(Stream stream)
        {
            // Load the Xml schema
            Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "SchemaCompiler.Schema.xsd");
            XmlSchema schema;
            using(schemaStream)
            {
                schema = XmlSchema.Read(schemaStream, delegate(object sender, ValidationEventArgs args)
                {
                });
            }

            // Create a validating XML reader
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Schemas.Add(schema);
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.ValidationEventHandler += delegate(object sender, ValidationEventArgs args)
            {
                throw new ValidationException(String.Format(Resources.XmlSchemaException, args.Exception.LineNumber,
                    args.Exception.LinePosition, args.Message));
            
            };
            XmlReader reader = XmlReader.Create(stream, readerSettings);
            using(reader)
            {                
                // Load the file
                return new XPathDocument(reader, XmlSpace.Preserve);
            }
        }

        /// <summary>
        /// Read a required string attribute from XML
        /// </summary>
        /// <param name="navigator">Navigator pointing to the node containing the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>Value from the attribute</returns>
        public static string ReadRequiredStringAttribute(XPathNavigator navigator, string attributeName)
        {
            XPathNavigator attribute = navigator.Clone();
            if (!attribute.MoveToAttribute(attributeName, String.Empty))
                // XSD should've caught this
                throw new InternalException("SCMP3350");

            return attribute.Value;
        }

        /// <summary>
        /// Read an optional string attribute from XML
        /// </summary>
        /// <param name="navigator">Navigator pointing to the node containing the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value from the attribute, or defaultValue if the attribute doesn't exist</returns>
        public static string ReadOptionalStringAttribute(XPathNavigator navigator, string attributeName, string defaultValue)
        {
            XPathNavigator attribute = navigator.Clone();
            if (!attribute.MoveToAttribute(attributeName, String.Empty))
                return defaultValue;

            return attribute.Value;
        }

        /// <summary>
        /// Read a required Int32 attribute from XML
        /// </summary>
        /// <param name="navigator">Navigator pointing to the node containing the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>Value from the attribute</returns>
        public static int ReadRequiredInt32Attribute(XPathNavigator navigator, string attributeName)
        {
            XPathNavigator attribute = navigator.Clone();
            if (!attribute.MoveToAttribute(attributeName, String.Empty))
                // XSD should've caught this
                throw new InternalException("SCMP3360");

            return attribute.ValueAsInt;
        }

        /// <summary>
        /// Read a optional Int32 attribute from XML
        /// </summary>
        /// <param name="navigator">Navigator pointing to the node containing the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value from the attribute, or defaultValue if the attribute doesn't exist</returns>
        public static int? ReadOptionalInt32Attribute(XPathNavigator navigator, string attributeName, int? defaultValue)
        {
            XPathNavigator attribute = navigator.Clone();
            if (!attribute.MoveToAttribute(attributeName, String.Empty))
                return defaultValue;

            return attribute.ValueAsInt;
        }

        /// <summary>
        /// Read a optional Boolean attribute from XML
        /// </summary>
        /// <param name="navigator">Navigator pointing to the node containing the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value from the attribute, or defaultValue if the attribute doesn't exist</returns>
        public static bool? ReadOptionalBooleanAttribute(XPathNavigator navigator, string attributeName, bool? defaultValue)
        {
            XPathNavigator attribute = navigator.Clone();
            if (!attribute.MoveToAttribute(attributeName, String.Empty))
                return defaultValue;

            return attribute.ValueAsBoolean;
        }

        /// <summary>
        /// Convert a type code string into a type code value
        /// </summary>
        /// <param name="typeString">Type code string</param>
        /// <returns>Type code value</returns>
        public static LearningStoreValueTypeCode TypeStringToTypeCode(string typeString)
        {
            switch (typeString)
            {
                case "ItemIdentifier": return LearningStoreValueTypeCode.ItemIdentifier;
                case "String": return LearningStoreValueTypeCode.String;
                case "Boolean": return LearningStoreValueTypeCode.Boolean;
                case "DateTime": return LearningStoreValueTypeCode.DateTime;
                case "Single": return LearningStoreValueTypeCode.Single;
                case "Double": return LearningStoreValueTypeCode.Double;
                case "Xml": return LearningStoreValueTypeCode.Xml;
                case "Enum": return LearningStoreValueTypeCode.Enumeration;
                case "Int32": return LearningStoreValueTypeCode.Int32;
                case "ByteArray": return LearningStoreValueTypeCode.ByteArray;
                case "Guid": return LearningStoreValueTypeCode.Guid;
                default:
                    throw new InternalException("SCMP3370");
            }
        }

        /// <summary>
        /// Get the maximum length of a string property in characters
        /// </summary>
        /// <returns></returns>
        public static int GetMaxStringLength()
        {
            return (Int32.MaxValue - 2) / 2;
        }

        /// <summary>
        /// Get the maximum length of a byte array property in bytes
        /// </summary>
        /// <returns></returns>        
        public static int GetMaxByteArrayLength()
        {
            return (Int32.MaxValue - 2);
        }
    }
}
