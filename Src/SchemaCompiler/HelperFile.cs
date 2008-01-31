/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CSharp;

/*
 * Code that writes the .cs helper files
 * Internal error numbers: 4000-4999
 */
namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Methods that write a C# helper file
    /// </summary>
    internal static class HelperFile
    {
        /// <summary>
        /// Get the maximum length of a property (in characters or bytes) as a string
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static int GetMaxPropertyLength(ItemTypePropertyInfo property)
        {
            switch (property.ValueTypeCode)
            {
                case LearningStoreValueTypeCode.String:
                    if (property.Length == null)
                        return (Int32.MaxValue - 2) / 2;
                    else
                        return (int)property.Length;
                case LearningStoreValueTypeCode.ByteArray:
                    if (property.Length == null)
                        return Int32.MaxValue - 2;
                    else
                        return (int)property.Length;
                default:
                    throw new InternalException("SCMP4000");
            }
        }

        /// <summary>
        /// Get the tag that refers to an enum definition in either the base
        /// schema or the derived schema
        /// </summary>
        /// <param name="en">Enum</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <returns></returns>
        private static string GetEnumTag(EnumInfo en, string typeNamespace)
        {
            if (en.InBaseSchema)
                return "/Microsoft.LearningComponents." + en.Name;
            else
                return "/" + typeNamespace + "." + en.Name;
        }

        /// <summary>
        /// Get the tag that refers to an enum value definition in either the base
        /// schema or the derived schema
        /// </summary>
        /// <param name="en">Enum</param>
        /// <param name="valueName">Value name</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <returns></returns>
        private static string GetEnumValueTag(EnumInfo en, string valueName, string typeNamespace)
        {
            return GetEnumTag(en, typeNamespace) + "." + valueName;
        }

        /// <summary>
        /// Get name of a parameter/column type that will be placed in the documentation
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="en"></param>
        /// <param name="referencedItemType"></param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <returns></returns>
        private static string GetViewParameterOrColumnTypeDocumentation(LearningStoreValueTypeCode typeCode,
            EnumInfo en, ItemTypeInfo referencedItemType, string typeNamespace)
        {
            switch (typeCode)
            {
                case LearningStoreValueTypeCode.Boolean:
                    return Resources.HelperValueTypeBoolean;
                case LearningStoreValueTypeCode.ByteArray:
                    return Resources.HelperValueTypeByteArrayWithoutLength;
                case LearningStoreValueTypeCode.DateTime:
                    return Resources.HelperValueTypeDateTime;
                case LearningStoreValueTypeCode.Double:
                    return Resources.HelperValueTypeDouble;
                case LearningStoreValueTypeCode.Enumeration:
                    return String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeEnum,
                        GetEnumTag(en, typeNamespace));
                case LearningStoreValueTypeCode.Guid:
                    return Resources.HelperValueTypeGuid;
                case LearningStoreValueTypeCode.Int32:
                    return Resources.HelperValueTypeInt32;
                case LearningStoreValueTypeCode.ItemIdentifier:
                    return String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeItemIdentifier,
                        referencedItemType.Name);
                case LearningStoreValueTypeCode.Single:
                    return Resources.HelperValueTypeSingle;
                case LearningStoreValueTypeCode.String:
                    return Resources.HelperValueTypeStringWithoutLength;
                case LearningStoreValueTypeCode.Xml:
                    return Resources.HelperValueTypeXml;
                default:
                    throw new InternalException("SCMP4010");
            }
        }

        /// <summary>
        /// Add SuppressMessage attributes to avoid naming warnings
        /// </summary>
        /// <param name="member"></param>
        private static void AddNamingSuppressMessages(CodeTypeMember member)
        {
            member.CustomAttributes.Add(new CodeAttributeDeclaration("SuppressMessageAttribute",
                new CodeAttributeArgument(new CodePrimitiveExpression("Microsoft.Naming")),
                new CodeAttributeArgument(new CodePrimitiveExpression("CA1726"))));
            member.CustomAttributes.Add(new CodeAttributeDeclaration("SuppressMessageAttribute",
                new CodeAttributeArgument(new CodePrimitiveExpression("Microsoft.Naming")),
                new CodeAttributeArgument(new CodePrimitiveExpression("CA1702"))));
            member.CustomAttributes.Add(new CodeAttributeDeclaration("SuppressMessageAttribute",
                new CodeAttributeArgument(new CodePrimitiveExpression("Microsoft.Naming")),
                new CodeAttributeArgument(new CodePrimitiveExpression("CA1704"))));        
        }
        
        /// <summary>
        /// Add comments in a "pretty" format
        /// </summary>
        /// <param name="s">The documentation</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddComments(string s, CodeCommentStatementCollection commentCollection)
        {
            // First split the string into a set of lines based on "next line" characters
            List<string> lines = new List<string>(s.Split(new string[] { "\r\n", "\r", "\n" },
                Int32.MaxValue, StringSplitOptions.None));

            // Trim all the spaces off of each line
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = lines[i].Trim();
            }

            // Remove any empty lines at the top of the documentation            
            while ((lines.Count > 0) && String.IsNullOrEmpty(lines[0]))
                lines.RemoveAt(0);

            // Remove any empty lines at the bottom of the documentation
            while ((lines.Count > 0) && String.IsNullOrEmpty(lines[lines.Count - 1]))
                lines.RemoveAt(lines.Count - 1);

            // Add each of the comment statements
            foreach (string line in lines)
                commentCollection.Add(new CodeCommentStatement(line, true));
        }

        /// <summary>
        /// Add the comments for an enum type
        /// </summary>
        /// <param name="info">Enum info</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddEnumComments(EnumInfo en, CodeCommentStatementCollection commentCollection)
        {
            // Get the user-provided documentation
            string summaryDocumentation = en.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = en.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Add comments
            AddComments("<summary>", commentCollection);
            if (summaryDocumentation == null)
                AddComments(en.Name, commentCollection);
            else
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            if (remarksDocumentation != null)
            {
                AddComments("<remarks>", commentCollection);
                AddComments(remarksDocumentation, commentCollection);
                AddComments("</remarks>", commentCollection);
            }
        }

        /// <summary>
        /// Add the comments for an enum value
        /// </summary>
        /// <param name="info">Enum info</param>
        /// <param name="valueName">Name of the value</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddEnumValueComments(EnumInfo en, string valueName, CodeCommentStatementCollection commentCollection)
        {
            // Get the user-provided documentation
            string documentation = null;
            if (en.TryGetDocumentationForValue(valueName, out documentation))
                documentation = documentation.Trim();

            // Construct doc string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperEnumValueSummary.Trim(), valueName,
                en.GetIntegerValue(valueName)), commentCollection);
            if (documentation != null)
            {
                AddComments(documentation, commentCollection);
            }
            AddComments("</summary>", commentCollection);
        }

        /// <summary>
        /// Add the comments for an item type
        /// </summary>
        /// <param name="itemType">The item type</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddItemTypeComments(ItemTypeInfo itemType, CodeCommentStatementCollection commentCollection)
        {
            // Get the documentation
            string summaryDocumentation = itemType.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = itemType.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Create a list of the properties on this item type, sorted by name
            List<ItemTypePropertyInfo> sortedProperties = new List<ItemTypePropertyInfo>(itemType.Properties);
            sortedProperties.Sort(delegate(ItemTypePropertyInfo property1, ItemTypePropertyInfo property2)
            {
                return property1.Name.CompareTo(property2.Name);
            });

            // Construct doc string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypeSummary.Trim(), itemType.Name), commentCollection);
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            AddComments(Resources.HelperItemTypePropertyList.Trim(), commentCollection);
            AddComments("<ul>", commentCollection);
            AddComments("<li><Fld>Id</Fld></li>", commentCollection);
            foreach (ItemTypePropertyInfo property in sortedProperties)
            {
                AddComments("<li><Fld>" + property.Name + "</Fld></li>", commentCollection);
            }
            AddComments("</ul>", commentCollection);
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Add the documentation for the maximum length of a property on an item type
        /// </summary>
        /// <param name="property">The property</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddItemTypePropertyMaxLengthComments(ItemTypePropertyInfo property, CodeCommentStatementCollection commentCollection)
        {
            // Construct doc string
            AddComments("<summary>", commentCollection);
            if (property.ValueTypeCode == LearningStoreValueTypeCode.String)
            {
                AddComments(String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperItemTypePropertyMaximumLengthOfString.Trim(), property.Name), commentCollection);
            }
            else if (property.ValueTypeCode == LearningStoreValueTypeCode.ByteArray)
            {
                AddComments(String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperItemTypePropertyMaximumLengthOfByteArray.Trim(), property.Name), commentCollection);
            }
            else
                throw new InternalException("SCMP4020");
            AddComments("</summary>", commentCollection);
        }

        /// <summary>
        /// Add the documentation for an item type property
        /// </summary>
        /// <param name="property">The property</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]  // Much of the complexity is due to simple switch statements
        private static void AddItemTypePropertyComments(ItemTypePropertyInfo property,
            string typeNamespace, CodeCommentStatementCollection commentCollection)
        {
            // Get the user-provided documentation
            string summaryDocumentation = property.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = property.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Get the property type
            string propertyType;
            switch (property.ValueTypeCode)
            {
                case LearningStoreValueTypeCode.Boolean:
                    propertyType = Resources.HelperValueTypeBoolean;
                    break;
                case LearningStoreValueTypeCode.ByteArray:
                    propertyType = String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeByteArray,
                        GetMaxPropertyLength(property));
                    break;
                case LearningStoreValueTypeCode.DateTime:
                    propertyType = Resources.HelperValueTypeDateTime;
                    break;
                case LearningStoreValueTypeCode.Double:
                    propertyType = Resources.HelperValueTypeDouble;
                    break;
                case LearningStoreValueTypeCode.Enumeration:
                    propertyType = String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeEnum,
                        GetEnumTag(property.GetEnum(), typeNamespace));
                    break;
                case LearningStoreValueTypeCode.Guid:
                    propertyType = Resources.HelperValueTypeGuid;
                    break;
                case LearningStoreValueTypeCode.Int32:
                    propertyType = Resources.HelperValueTypeInt32;
                    break;
                case LearningStoreValueTypeCode.ItemIdentifier:
                    propertyType = String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeItemIdentifier,
                        property.ReferencedItemTypeName);
                    break;
                case LearningStoreValueTypeCode.Single:
                    propertyType = Resources.HelperValueTypeSingle;
                    break;
                case LearningStoreValueTypeCode.String:
                    propertyType = String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeString,
                        GetMaxPropertyLength(property));
                    break;
                case LearningStoreValueTypeCode.Xml:
                    propertyType = Resources.HelperValueTypeXml;
                    break;
                default:
                    throw new InternalException("SCMP4030");
            }

            // Get the default value
            string defaultValue = null;
            if (property.HasDefaultValue)
            {
                if (property.GetDefaultValue() == null)
                    defaultValue = "null";
                else if (property.IsDefaultAFunction == true)
                    defaultValue = property.GetDefaultValue().ToString();
                else
                {
                    switch (property.ValueTypeCode)
                    {
                        case LearningStoreValueTypeCode.Boolean:
                            defaultValue = ((Boolean)property.GetDefaultValue()).ToString(CultureInfo.CurrentCulture);
                            break;
                        case LearningStoreValueTypeCode.ByteArray:
                            defaultValue = BitConverter.ToString((byte[])property.GetDefaultValue());
                            break;
                        case LearningStoreValueTypeCode.DateTime:
                            defaultValue = ((DateTime)property.GetDefaultValue()).ToString(CultureInfo.CurrentCulture);
                            break;
                        case LearningStoreValueTypeCode.Double:
                            defaultValue = ((Double)property.GetDefaultValue()).ToString(CultureInfo.CurrentCulture);
                            break;
                        case LearningStoreValueTypeCode.Enumeration:
                            defaultValue = "<Fld>" + GetEnumValueTag(property.GetEnum(),
                                property.GetEnum().GetValueName((Int32)property.GetDefaultValue()), typeNamespace) + "</Fld>";
                            break;
                        case LearningStoreValueTypeCode.Guid:
                            defaultValue = ((Guid)property.GetDefaultValue()).ToString();
                            break;
                        case LearningStoreValueTypeCode.Int32:
                            defaultValue = ((Int32)property.GetDefaultValue()).ToString(CultureInfo.CurrentCulture);
                            break;
                        case LearningStoreValueTypeCode.Single:
                            defaultValue = ((Single)property.GetDefaultValue()).ToString(CultureInfo.CurrentCulture);
                            break;
                        case LearningStoreValueTypeCode.String:
                            defaultValue = "\"" + property.GetDefaultValue().ToString().Replace("\"", "\\\"" + "\"") + "\"";
                            break;
                        case LearningStoreValueTypeCode.Xml:
                            defaultValue = System.Web.HttpUtility.HtmlEncode(property.GetDefaultValue().ToString());
                            break;
                        default:
                            throw new InternalException("SCMP4040");
                    }
                }
            }

            // Construct the string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypePropertyNameSummary.Trim(),
                property.Name, property.ItemType.Name), commentCollection);
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypePropertyType.Trim(), propertyType) + "<p/>", commentCollection);
            if (property.Nullable)
                AddComments(Resources.HelperItemTypePropertyNullable.Trim() + "<p/>", commentCollection);
            else
                AddComments(Resources.HelperItemTypePropertyNotNullable.Trim() + "<p/>", commentCollection);
            if (property.HasDefaultValue)
            {
                AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypePropertyDefault.Trim(), defaultValue) + "<p/>", commentCollection);
            }
            else
            {
                AddComments(Resources.HelperItemTypePropertyNoDefault.Trim() + "<p/>", commentCollection);
            }
            if ((property.ValueTypeCode == LearningStoreValueTypeCode.ItemIdentifier) &&
               (property.CascadeDelete))
                AddComments(Resources.HelperItemTypePropertyAutoDelete.Trim() + "<p/>", commentCollection);
            AddComments("</remarks>", commentCollection);

        }

        /// <summary>
        /// Add the comments for the ID property on an item type
        /// </summary>
        /// <param name="itemType">The item type</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddItemTypeIdPropertyComments(ItemTypeInfo itemType, CodeCommentStatementCollection commentCollection)
        {
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypePropertyNameSummary.Trim(),
                "Id", itemType.Name), commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            AddComments(Resources.HelperItemTypeIdPropertyRemarks.Trim() + "<p/>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperItemTypePropertyType.Trim(),
                String.Format(CultureInfo.CurrentCulture, Resources.HelperValueTypeItemIdentifier, itemType.Name)) + "<p/>", commentCollection);
            AddComments(Resources.HelperItemTypePropertyNotNullable.Trim() + "<p/>", commentCollection);
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Add the comments for a view
        /// </summary>
        /// <param name="view">The view</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddViewComments(ViewInfo view, CodeCommentStatementCollection commentCollection)
        {
            // Get the documentation
            string summaryDocumentation = view.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = view.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Create a list of the columns on this view, sorted by name
            List<ViewColumnInfo> sortedColumns = new List<ViewColumnInfo>(view.Columns);
            sortedColumns.Sort(delegate(ViewColumnInfo column1, ViewColumnInfo column2)
            {
                return column1.Name.CompareTo(column2.Name);
            });

            // Create a list of the parameters on this view, sorted by name
            List<ParameterInfo> sortedParameters = new List<ParameterInfo>(view.Parameters);
            sortedParameters.Sort(delegate(ParameterInfo parameter1, ParameterInfo parameter2)
            {
                return parameter1.Name.CompareTo(parameter2.Name);
            });

            // Construct doc string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperViewSummary.Trim(), view.Name), commentCollection);
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            AddComments(Resources.HelperViewColumnList.Trim(), commentCollection);
            AddComments("<ul>", commentCollection);
            foreach (ViewColumnInfo column in sortedColumns)
            {
                AddComments("<li><Fld>" + column.Name + "</Fld></li>", commentCollection);
            }
            AddComments("</ul>", commentCollection);
            AddComments(Resources.HelperViewParameterList.Trim(), commentCollection);
            if (sortedParameters.Count == 0)
            {
                AddComments(Resources.HelperNone, commentCollection);
            }
            else
            {
                AddComments("<ul>", commentCollection);
                foreach (ParameterInfo parameter in sortedParameters)
                {
                    AddComments("<li><Fld>" + parameter.Name + "</Fld></li>", commentCollection);
                }
                AddComments("</ul>", commentCollection);
            }
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Add the comments for a view column
        /// </summary>
        /// <param name="column">The column</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddViewColumnComments(ViewColumnInfo column, string typeNamespace, CodeCommentStatementCollection commentCollection)
        {
            // Get the user-provided documentation
            string summaryDocumentation = column.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = column.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Get the column type
            string columnType = GetViewParameterOrColumnTypeDocumentation(
                column.ValueTypeCode, column.GetEnum(), column.GetReferencedItemType(), typeNamespace);

            // Construct the string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperViewColumnNameSummary.Trim(),
                column.Name, column.View.Name), commentCollection);
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperViewColumnType.Trim(), columnType), commentCollection);
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Write the documentation for a parameter on a view or right
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddParameterComments(ParameterInfo parameter, string typeNamespace, CodeCommentStatementCollection commentCollection)
        {
            // Get the user-provided documentation
            string summaryDocumentation = parameter.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = parameter.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Get the parameter type
            string parameterType = GetViewParameterOrColumnTypeDocumentation(
                parameter.ValueTypeCode, parameter.GetEnum(), parameter.GetReferencedItemType(), typeNamespace);

            // Construct the string
            AddComments("<summary>", commentCollection);
            if(parameter.View != null)
                AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperViewParameterNameSummary.Trim(),
                    parameter.Name, parameter.View.Name), commentCollection);
            else if(parameter.Right != null)
                AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperRightParameterNameSummary.Trim(),
                    parameter.Name, parameter.Right.Name), commentCollection);
            else
                throw new InternalException("SCMP4050");
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            if(parameter.View != null)
                AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperViewParameterType.Trim(), parameterType), commentCollection);
            else if(parameter.Right != null)
                AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperRightParameterType.Trim(), parameterType), commentCollection);
            else
                throw new InternalException("SCMP4060");            
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Add the comments for a right
        /// </summary>
        /// <param name="right">The right</param>
        /// <param name="commentCollection">Location in which to add the comments</param>
        private static void AddRightComments(RightInfo right, CodeCommentStatementCollection commentCollection)
        {
            // Get the documentation
            string summaryDocumentation = right.SummaryDocumentation;
            if (summaryDocumentation != null)
                summaryDocumentation = summaryDocumentation.Trim();
            string remarksDocumentation = right.RemarksDocumentation;
            if (remarksDocumentation != null)
                remarksDocumentation = remarksDocumentation.Trim();

            // Create a list of the parameters on this right, sorted by name
            List<ParameterInfo> sortedParameters = new List<ParameterInfo>(right.Parameters);
            sortedParameters.Sort(delegate(ParameterInfo parameter1, ParameterInfo parameter2)
            {
                return parameter1.Name.CompareTo(parameter2.Name);
            });

            // Construct doc string
            AddComments("<summary>", commentCollection);
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperRightSummary.Trim(), right.Name), commentCollection);
            if (summaryDocumentation != null)
                AddComments(summaryDocumentation, commentCollection);
            AddComments("</summary>", commentCollection);
            AddComments("<remarks>", commentCollection);
            if (remarksDocumentation != null)
                AddComments(remarksDocumentation + "<p/>", commentCollection);
            AddComments(Resources.HelperRightParameterList.Trim(), commentCollection);
            if (sortedParameters.Count == 0)
            {
                AddComments(Resources.HelperNone, commentCollection);
            }
            else
            {
                AddComments("<ul>", commentCollection);
                foreach (ParameterInfo parameter in sortedParameters)
                {
                    AddComments("<li><Fld>" + parameter.Name + "</Fld></li>", commentCollection);
                }
                AddComments("</ul>", commentCollection);
            }
            AddComments("</remarks>", commentCollection);
        }

        /// <summary>
        /// Add declaration of enum
        /// </summary>
        /// <param name="info">Enum info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddEnum(EnumInfo en, CodeNamespace ns)
        {
            // Add the type
            CodeTypeDeclaration type = new CodeTypeDeclaration(en.Name);
            type.IsEnum = true;
            type.TypeAttributes = System.Reflection.TypeAttributes.Public;
            ns.Types.Add(type);

            // Add the documentation
            AddEnumComments(en, type.Comments);

            // Add the enum values
            foreach (string valueName in en.ValueNames)
            {
                CodeMemberField field = new CodeMemberField(typeof(int), valueName);
                field.InitExpression = new CodePrimitiveExpression(en.GetIntegerValue(valueName));
                AddNamingSuppressMessages(field);
                AddEnumValueComments(en, valueName, field.Comments);
                type.Members.Add(field);
            }
        }

        /// <summary>
        /// Add declaration of identifier class
        /// </summary>
        /// <param name="itemType">Item type info</param>
        /// <param name="itemTypeNamespace">Namespace for the item type class</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddIdentifier(ItemTypeInfo itemType, string itemTypeNamespace,
            CodeNamespace ns)
        {
            // Calculate the name of the identifier class
            string className = itemType.Name + "Identifier";

            // Add the start of the class
            CodeTypeDeclaration type = new CodeTypeDeclaration(className);
            type.TypeAttributes = System.Reflection.TypeAttributes.Public;
            type.BaseTypes.Add(new CodeTypeReference("LearningStoreItemIdentifier"));
            AddComments(
                String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperIdentifierDocumentation, "/" + itemTypeNamespace + "." + itemType.Name), type.Comments);
            ns.Types.Add(type);

            // Add constructor #1
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(long), "key"));
            constructor.BaseConstructorArgs.Add(new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(itemTypeNamespace + "." + itemType.Name), "ItemTypeName"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("key"));
            AddComments(
                String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperIdentifierConstructor1Documentation, className, itemType.Name), constructor.Comments);
            type.Members.Add(constructor);

            // Add constructor #2
            constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression("LearningStoreItemIdentifier", "id"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("id"));
            AddComments(
                String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperIdentifierConstructor2Documentation, className, itemType.Name), constructor.Comments);
            constructor.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeVariableReferenceExpression("id"), CodeBinaryOperatorType.IdentityEquality,
                            new CodePrimitiveExpression(null)),
                    new CodeThrowExceptionStatement(
                        new CodeObjectCreateExpression("ArgumentNullException",
                            new CodePrimitiveExpression("id")))));
            constructor.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(
                            new CodeVariableReferenceExpression("id"), "ItemTypeName"), CodeBinaryOperatorType.IdentityInequality,
                            new CodeFieldReferenceExpression(
                                new CodeTypeReferenceExpression(itemTypeNamespace + "." + itemType.Name), "ItemTypeName")),
                    new CodeThrowExceptionStatement(
                        new CodeObjectCreateExpression("ArgumentOutOfRangeException",
                            new CodePrimitiveExpression("id")))));
            type.Members.Add(constructor);
        }

        /// <summary>
        /// Add declaration of item type
        /// </summary>
        /// <param name="itemType">Item type info</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="buildingStorageHelper">True if the storage helper assembly is being built</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddItemType(ItemTypeInfo itemType, string typeNamespace, bool buildingStorageHelper,
            CodeNamespace ns)
        {
            // Add the type
            CodeTypeDeclaration type = new CodeTypeDeclaration(itemType.Name);
            type.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            AddNamingSuppressMessages(type);
            ns.Types.Add(type);

            // Add the documentation
            AddItemTypeComments(itemType, type.Comments);

            // Add a constant containing the name of the LearningStoreItem
            CodeMemberField field = new CodeMemberField(typeof(string), "ItemTypeName");
            field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
            CodeExpression fieldValue = null;
            if ((itemType.InBaseSchema) && (!buildingStorageHelper))
                fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                    "Microsoft.LearningComponents.Storage.BaseSchema." + itemType.Name), "ItemTypeName");
            else
                fieldValue = new CodePrimitiveExpression(itemType.Name);            
            field.InitExpression = fieldValue;
            AddComments(String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperItemTypeNameDocumentation, itemType.Name), field.Comments);
            type.Members.Add(field);

            // Add a constant containing the name of the ID property
            field = new CodeMemberField(typeof(string), "Id");
            field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
            fieldValue = null;
            if ((itemType.InBaseSchema) && (!buildingStorageHelper))
                fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                    "Microsoft.LearningComponents.Storage.BaseSchema." + itemType.Name), "Id");
            else
                fieldValue = new CodePrimitiveExpression("Id");            
            field.InitExpression = fieldValue;
            AddItemTypeIdPropertyComments(itemType, field.Comments);
            type.Members.Add(field);

            // Enumerate through the properties
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                // Add a static constant containing the name of the property
                field = new CodeMemberField(typeof(string), property.Name);
                field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                AddNamingSuppressMessages(field);
                fieldValue = null;
                if ((property.InBaseSchema) && (!buildingStorageHelper))
                    fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                        "Microsoft.LearningComponents.Storage.BaseSchema." + itemType.Name), property.Name);
                else
                    fieldValue = new CodePrimitiveExpression(property.Name);
                field.InitExpression = fieldValue;
                AddItemTypePropertyComments(property, typeNamespace, field.Comments);
                type.Members.Add(field);
                
                // Write lengths
                if ((property.ValueTypeCode == LearningStoreValueTypeCode.String) ||
                   (property.ValueTypeCode == LearningStoreValueTypeCode.ByteArray))
                {
                    // Add a const containing the length of the property
                    field = new CodeMemberField(typeof(int), "Max" + property.Name + "Length");
                    field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                    AddNamingSuppressMessages(field);
                    fieldValue = null;
                    if (buildingStorageHelper)
                        fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                            "BaseSchemaInternal." + itemType.Name), "Max" + property.Name + "Length");
                    else if (property.InBaseSchema)
                        fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                            "Microsoft.LearningComponents.Storage.BaseSchema." + itemType.Name), "Max" + property.Name + "Length");
                    else
                        fieldValue = new CodePrimitiveExpression(GetMaxPropertyLength(property));
                    field.InitExpression = fieldValue;
                    AddItemTypePropertyMaxLengthComments(property, field.Comments);
                    type.Members.Add(field);
                }
            }
        }

        /// <summary>
        /// Add declaration of item type that appears in the components assembly
        /// </summary>
        /// <param name="itemType">Item type info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddItemTypeForComponents(ItemTypeInfo itemType, CodeTypeDeclaration baseType)
        {
            // Add the type
            CodeTypeDeclaration type = new CodeTypeDeclaration(itemType.Name);
            type.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            AddNamingSuppressMessages(type);
            baseType.Members.Add(type);

            // Add the documentation
            AddComments(String.Format(CultureInfo.CurrentCulture, Resources.HelperBaseSchemaInternalItemTypeDocumentation,
                itemType.Name), type.Comments);
            
            // Enumerate through the properties
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                // Write lengths
                if ((property.ValueTypeCode == LearningStoreValueTypeCode.String) ||
                    (property.ValueTypeCode == LearningStoreValueTypeCode.ByteArray))
                {
                    // Add a const containing the length of the property
                    CodeMemberField field = new CodeMemberField(typeof(int), "Max" + property.Name + "Length");
                    field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                    field.InitExpression = new CodePrimitiveExpression(GetMaxPropertyLength(property));
                    AddNamingSuppressMessages(field);
                    if (property.ValueTypeCode == LearningStoreValueTypeCode.String)
                        AddComments(String.Format(CultureInfo.CurrentCulture,
                            Resources.HelperBaseSchemaInternalItemTypePropertyMaximumLengthOfString, property.Name),
                            field.Comments);
                    else
                        AddComments(String.Format(CultureInfo.CurrentCulture,
                            Resources.HelperBaseSchemaInternalItemTypePropertyMaximumLengthOfByteArray, property.Name),
                            field.Comments);
                    type.Members.Add(field);
                }
            }
        }

        /// <summary>
        /// Add declaration of defined view
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="buildingStorageHelper">True if the storage helper assembly is being built</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddDefinedView(ViewInfo view, string typeNamespace, bool buildingStorageHelper,
            CodeNamespace ns)
        {
            // Add the type
            CodeTypeDeclaration type = new CodeTypeDeclaration(view.Name);
            type.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            AddNamingSuppressMessages(type);
            ns.Types.Add(type);

            // Add the documentation
            AddViewComments(view, type.Comments);

            // Add a static constant containing the name of the view
            CodeMemberField field = new CodeMemberField(typeof(string), "ViewName");
            field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
            AddComments(
                String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperViewNameDocumentation, view.Name), field.Comments);
            CodeExpression fieldValue = null;
            if ((view.InBaseSchema) && (!buildingStorageHelper))
                fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                    "Microsoft.LearningComponents.Storage.BaseSchema." + view.Name), "ViewName");
            else
                fieldValue = new CodePrimitiveExpression(view.Name);
            field.InitExpression = fieldValue;
            type.Members.Add(field);                      

            // Enumerate through the columns
            foreach (ViewColumnInfo column in view.Columns)
            {
                // Add a constant containing the name of the column
                field = new CodeMemberField(typeof(string), column.Name);
                field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                AddNamingSuppressMessages(field);
                AddViewColumnComments(column, typeNamespace, field.Comments);
                fieldValue = null;
                if ((column.InBaseSchema) && (!buildingStorageHelper))
                    fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                        "Microsoft.LearningComponents.Storage.BaseSchema." + view.Name), column.Name);
                else
                    fieldValue = new CodePrimitiveExpression(column.Name);
                field.InitExpression = fieldValue;
                type.Members.Add(field);
            }

            // Enumerate through the parameters
            foreach (ParameterInfo parameter in view.Parameters)
            {
                // Add a static constant containing the name of the column
                field = new CodeMemberField(typeof(string), parameter.Name);
                field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                AddNamingSuppressMessages(field);
                AddParameterComments(parameter, typeNamespace, field.Comments);
                fieldValue = null;
                if ((parameter.InBaseSchema) && (!buildingStorageHelper))
                    fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                        "Microsoft.LearningComponents.Storage.BaseSchema." + view.Name), parameter.Name);
                else
                    fieldValue = new CodePrimitiveExpression(parameter.Name);
                field.InitExpression = fieldValue;
                type.Members.Add(field);
            }
        }

        /// <summary>
        /// Add declaration of right
        /// </summary>
        /// <param name="right">Right</param>
        /// <param name="typeNamespace">Namespace for the non-base types (e.g., Enums)</param>
        /// <param name="buildingStorageHelper">True if the storage helper assembly is being built</param>
        /// <param name="writer">Location in which to write data</param>
        private static void AddRight(RightInfo right, string typeNamespace, bool buildingStorageHelper,
            CodeNamespace ns)
        {
            // Add the type
            CodeTypeDeclaration type = new CodeTypeDeclaration(right.Name);
            type.TypeAttributes = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Abstract;
            AddNamingSuppressMessages(type);
            ns.Types.Add(type);

            // Add the documentation
            AddRightComments(right, type.Comments);

            // Add a static constant containing the name of the view
            CodeMemberField field = new CodeMemberField(typeof(string), "RightName");
            field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
            AddComments(
                String.Format(CultureInfo.CurrentCulture,
                    Resources.HelperRightNameDocumentation, right.Name), field.Comments);
            CodeExpression fieldValue = null;
            if ((right.InBaseSchema) && (!buildingStorageHelper))
                fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                    "Microsoft.LearningComponents.Storage.BaseSchema." + right.Name), "RightName");
            else
                fieldValue = new CodePrimitiveExpression(right.Name);
            field.InitExpression = fieldValue;
            type.Members.Add(field);

            // Enumerate through the parameters
            foreach (ParameterInfo parameter in right.Parameters)
            {
                // Add a static constant containing the name of the column
                field = new CodeMemberField(typeof(string), parameter.Name);
                field.Attributes = MemberAttributes.Const | MemberAttributes.Public;
                AddNamingSuppressMessages(field);
                AddParameterComments(parameter, typeNamespace, field.Comments);
                fieldValue = null;
                if ((parameter.InBaseSchema) && (!buildingStorageHelper))
                    fieldValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(
                        "Microsoft.LearningComponents.Storage.BaseSchema." + right.Name), parameter.Name);
                else
                    fieldValue = new CodePrimitiveExpression(parameter.Name);
                field.InitExpression = fieldValue;
                type.Members.Add(field);
            }
        }

        /// <summary>
        /// Write the helper file that gets compiled into the LearningComponents assembly
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="path">Output path</param>
        public static void WriteComponentsHelper(SchemaInfo schema, string path)
        {
            // Create the C# code provider
            using(CSharpCodeProvider provider = new CSharpCodeProvider())
            {

                // Create the compile unit
                CodeCompileUnit unit = new CodeCompileUnit();

                // Add the first namespace
                CodeNamespace ns = new CodeNamespace("Microsoft.LearningComponents");
                unit.Namespaces.Add(ns);
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics.CodeAnalysis"));

                // Add the enums
                foreach (EnumInfo en in schema.Enums)
                {
                    AddEnum(en, ns);
                }

                // Add the class that will contain all the item type classes
                CodeTypeDeclaration type = new CodeTypeDeclaration("BaseSchemaInternal");
                type.TypeAttributes = System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.NestedAssembly;
                ns.Types.Add(type);
                AddComments(Resources.HelperBaseSchemaInternalClassDocumentation.Trim(), type.Comments);

                // Add the item type classes
                foreach (ItemTypeInfo itemType in schema.ItemTypes)
                {
                    AddItemTypeForComponents(itemType, type);
                }

                // Write the code
                StreamWriter stringWriter = new StreamWriter(path);
                using (stringWriter)
                {
                    IndentedTextWriter writer = new IndentedTextWriter(stringWriter);
                    using (writer)
                    {
                        provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions());
                    }
                }
            }
        }

        /// <summary>
        /// Write the helper file that gets compiled into the Storage assembly
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="path">Output path</param>
        public static void WriteStorageHelper(SchemaInfo schema, string path)
        {
            // Create the C# code provider
            using(CSharpCodeProvider provider = new CSharpCodeProvider())
            {

                // Create the compile unit
                CodeCompileUnit unit = new CodeCompileUnit();

                // Add the first namespace
                CodeNamespace ns = new CodeNamespace("Microsoft.LearningComponents.Storage.BaseSchema");
                unit.Namespaces.Add(ns);
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics.CodeAnalysis"));

                // Add the item type classes
                foreach (ItemTypeInfo itemType in schema.ItemTypes)
                {
                    AddItemType(itemType, "Microsoft.LearningComponents", true, ns);
                }

                // Add the view classes    
                foreach (ViewInfo view in schema.Views)
                {
                    AddDefinedView(view, "Microsoft.LearningComponents", true, ns);
                }

                // Add the right classes
                foreach (RightInfo right in schema.Rights)
                {
                    AddRight(right, "Microsoft.LearningComponents", true, ns);
                }
                
                // Add the second namespace
                ns = new CodeNamespace("Microsoft.LearningComponents.Storage");
                unit.Namespaces.Add(ns);
                ns.Imports.Add(new CodeNamespaceImport("System"));

                // Add the identifier classes
                foreach (ItemTypeInfo itemType in schema.ItemTypes)
                {
                    AddIdentifier(itemType, "Microsoft.LearningComponents.Storage.BaseSchema", ns);
                }

                // Write the code
                StreamWriter stringWriter = new StreamWriter(path);
                using (stringWriter)
                {
                    IndentedTextWriter writer = new IndentedTextWriter(stringWriter);
                    using (writer)
                    {
                        provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions());
                    }
                }
            }
        }

        /// <summary>
        /// Write "standard" helper file
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="path">Output path</param>
        /// <param name="namespaceName">Namespace for the types (e.g., Enums)</param>
        /// <param name="schemaNamespaceName">Namespace for the constants</param>
        public static void Write(SchemaInfo schema, string path, string namespaceName, string schemaNamespaceName)
        {
            // Create the C# code provider
            using(CSharpCodeProvider provider = new CSharpCodeProvider())
            {
            
                // Create the compile unit
                CodeCompileUnit unit = new CodeCompileUnit();
                
                // Add the first namespace
                CodeNamespace ns = new CodeNamespace(schemaNamespaceName);
                unit.Namespaces.Add(ns);
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics.CodeAnalysis"));

                // Add the item type classes
                foreach (ItemTypeInfo itemType in schema.ItemTypes)
                {
                    AddItemType(itemType, namespaceName, false, ns);
                }

                // Add the view classes    
                foreach (ViewInfo view in schema.Views)
                {
                    AddDefinedView(view, namespaceName, false, ns);
                }

                // Add the right classes
                foreach (RightInfo right in schema.Rights)
                {
                    AddRight(right, namespaceName, false, ns);
                }
                
                // Add the second namespace
                ns = new CodeNamespace(namespaceName);
                unit.Namespaces.Add(ns);
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics.CodeAnalysis"));
                ns.Imports.Add(new CodeNamespaceImport("Microsoft.LearningComponents.Storage"));
                
                // Add the enums
                foreach (EnumInfo en in schema.Enums)
                {
                    // Skip any that are in the base schema
                    if (en.InBaseSchema)
                        continue;

                    AddEnum(en, ns);
                }

                // Add the identifier classes
                foreach (ItemTypeInfo itemType in schema.ItemTypes)
                {
                    // Skip any that are in the base schema
                    if (itemType.InBaseSchema)
                        continue;

                    AddIdentifier(itemType, schemaNamespaceName, ns);
                }

                // Write the code
                StreamWriter stringWriter = new StreamWriter(path);
                using (stringWriter)
                {
                    IndentedTextWriter writer = new IndentedTextWriter(stringWriter);
                    using (writer)
                    {
                        provider.GenerateCodeFromCompileUnit(unit, writer, new CodeGeneratorOptions());
                    }
                }
            }
        } 
    }
}
