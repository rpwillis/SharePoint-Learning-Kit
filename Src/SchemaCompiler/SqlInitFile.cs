/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.CodeDom.Compiler;

/*
 * Code that writes the .sql init file
 * Internal error numbers: 5000-5999
 */

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Class that can write a .sql init file for a schema
    /// </summary>
    internal static class SqlInitFile
    {
        /// <summary>
        /// Write the code that creates and fills the enum table
        /// </summary>
        /// <param name="en">Enum</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteEnum(EnumInfo en, IndentedTextWriter writer)
        {
            // Add code that creates the table
            writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateTable, en.Name));
            writer.WriteLine("CREATE TABLE [" + en.Name + "](");
            writer.Indent++;
            writer.WriteLine("Id int IDENTITY PRIMARY KEY,");
            writer.WriteLine("Name varchar(63) NOT NULL");
            writer.Indent--;
            writer.WriteLine(")");
            writer.WriteLine("GRANT SELECT ON [" + en.Name + "] TO LearningStore");

            // Add code that adds each enum value
            foreach (string valueName in en.ValueNames)
            {
                writer.WriteLine("INSERT INTO [" + en.Name + "](");
                writer.Indent++;
                writer.WriteLine("Id,");
                writer.WriteLine("Name");
                writer.Indent--;
                writer.WriteLine(") VALUES (");
                writer.Indent++;
                writer.WriteLine(en.GetIntegerValue(valueName) + ",");
                writer.WriteLine("'" + valueName + "'");
                writer.Indent--;
                writer.WriteLine(")");
            }

            writer.WriteLine();
        }

        /// <summary>
        /// Write the code that defines a property on an item type
        /// </summary>
        /// <param name="property">Property info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemTypeProperty(ItemTypePropertyInfo property, IndentedTextWriter writer)
        {
            string sqlType = GetSqlParameterType(property);
            if (!property.Nullable)
                sqlType += " NOT NULL";
            if (property.HasDefaultValue)
            {
                sqlType += " DEFAULT " + GetSqlDefaultValueConstant(property);
            }
            if((property.ValueTypeCode == LearningStoreValueTypeCode.String) &&
               (property.Length != null) &&
               (property.Length > 4000))
            {
                sqlType += " CHECK(LEN([" + property.Name + "])<=" + property.Length.ToString() + ")";
            }
            if((property.ValueTypeCode == LearningStoreValueTypeCode.ByteArray) &&
               (property.Length != null) &&
               (property.Length > 8000))
            {
                sqlType += " CHECK(LEN([" + property.Name + "])<=" + property.Length.ToString() + ")";
            }
            foreach(string constraint in property.Constraints)
            {
                sqlType += " " + constraint;
            }
            
            writer.Write("[" + property.Name + "] " + sqlType);
        }

        /// <summary>
        /// Write the code that creates the table for an item type
        /// </summary>
        /// <param name="itemType">Item type info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemType(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            // Add code that creates the table
            writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateTable, itemType.Name));
            writer.WriteLine("CREATE TABLE [" + itemType.Name + "](");
            writer.Indent++;
            writer.Write("Id bigint IDENTITY PRIMARY KEY NOT NULL");            
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                writer.WriteLine(",");
                WriteItemTypeProperty(property, writer);
            }            
            writer.WriteLine();
            writer.Indent--;
            writer.WriteLine(")");
            writer.WriteLine("GRANT SELECT, INSERT, DELETE, UPDATE ON [" + itemType.Name + "] TO LearningStore");
            writer.WriteLine();
        }

        /// <summary>
        /// Write the code that adds foreign key constraints to a property
        /// </summary>
        /// <param name="property">Property info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemTypePropertyConstraint(ItemTypePropertyInfo property, IndentedTextWriter writer)
        {
            if (property.ValueTypeCode == LearningStoreValueTypeCode.ItemIdentifier)
            {
                writer.WriteLine("ALTER TABLE [" + property.ItemType.Name + "]");
                writer.WriteLine("ADD CONSTRAINT FK_" + property.ItemType.Name + "_" + property.Name + " FOREIGN KEY ([" + property.Name + "])");
                writer.Write("REFERENCES [" + property.GetReferencedItemType().Name + "] (Id)");
                if (property.CascadeDelete)
                    writer.Write(" ON DELETE CASCADE");
                writer.WriteLine();
                writer.WriteLine();
            }
            else if (property.ValueTypeCode == LearningStoreValueTypeCode.Enumeration)
            {
                writer.WriteLine("ALTER TABLE [" + property.ItemType.Name + "]");
                writer.WriteLine("ADD CONSTRAINT FK_" + property.ItemType.Name + "_" + property.Name + " FOREIGN KEY ([" + property.Name + "])");
                writer.WriteLine("REFERENCES [" + property.GetEnum().Name + "] (Id)");
                writer.WriteLine();
            }
        }
        
        /// <summary>
        /// Write the code that adds foreign key constraints to an item type
        /// </summary>
        /// <param name="itemType">Item type info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemTypeConstraintsAndIndexes(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            // Write the constraints on the properties
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                WriteItemTypePropertyConstraint(property, writer);
            }
            
            // Write the indexes
            foreach(string index in itemType.Indexes)
            {
                WriteStatements(index, writer);
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Write code that defines a default view
        /// </summary>
        /// <param name="itemType">View info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemTypeDefaultView(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            // Create the implementation
            StringBuilder sb = new StringBuilder("SELECT Id");
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                sb.Append(", [" + property.Name + "]");
            }
            sb.Append("\r\n");
            sb.Append(" FROM [" + itemType.Name + "]");
        
            // Add code that creates a function defining the view
            writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateDefaultViewFunction, itemType.Name));
            writer.WriteLine("CREATE FUNCTION [" + itemType.Name + "$DefaultView](@UserKey nvarchar(250))");
            writer.WriteLine("RETURNS TABLE");
            writer.WriteLine("AS");
            writer.WriteLine("RETURN (");
            writer.Indent++;
            WriteStatements(sb.ToString(), writer);
            writer.Indent--;
            writer.WriteLine(")");
            writer.WriteLine("GO");
            writer.WriteLine("GRANT SELECT ON [" + itemType.Name + "$DefaultView] TO LearningStore");
            writer.WriteLine("GO");
            writer.WriteLine();
        }

        /// <summary>
        /// Write the code that defines the security functions for a default view
        /// </summary>
        /// <param name="view">View info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteItemTypeDefaultViewSecurity(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            if(itemType.QueryRightExpressions.Count > 0)
            {
                // Add code that creates a function defining the security on the default view
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateDefaultViewSecurityFunction, itemType.Name));
                writer.WriteLine("CREATE FUNCTION [" + itemType.Name + "$DefaultViewSecurity](@UserKey nvarchar(250))");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, itemType.QueryRightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + itemType.Name + "$DefaultViewSecurity] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Write the code that defines the security functions for an item type
        /// </summary>
        /// <param name="info"></param>
        /// <param name="writer"></param>
        private static void WriteItemTypeSecurity(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            if(itemType.DeleteRightExpressions.Count > 0)
            {
                // Add code that creates a function defining the delete security on the item type
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateDeleteItemSecurityFunction, itemType.Name));
                writer.WriteLine("CREATE FUNCTION [" + itemType.Name + "$DeleteSecurity](@UserKey nvarchar(250),@Id bigint)");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, itemType.DeleteRightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + itemType.Name + "$DeleteSecurity] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }

            if(itemType.UpdateRightExpressions.Count > 0)
            {            
                // Create a list of parameters for the update security
                StringBuilder updateSecurityParameters = new StringBuilder();
                foreach (ItemTypePropertyInfo property in itemType.Properties)
                {
                    updateSecurityParameters.Append(",@");
                    updateSecurityParameters.Append(property.Name);
                    updateSecurityParameters.Append("$Changed bit,@");
                    updateSecurityParameters.Append(property.Name);
                    updateSecurityParameters.Append(" ");
                    updateSecurityParameters.Append(GetSqlParameterType(property));
                }
            
                // Add code that creates a function defining the update security on the item type
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateUpdateItemSecurityFunction, itemType.Name));
                writer.WriteLine("CREATE FUNCTION [" + itemType.Name + "$UpdateSecurity](@UserKey nvarchar(250),@Id bigint" + updateSecurityParameters.ToString() + ")");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, itemType.UpdateRightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + itemType.Name + "$UpdateSecurity] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }
            
            if(itemType.AddRightExpressions.Count > 0)
            {            
                // Create a list of parameters for the add security
                StringBuilder addSecurityParameters = new StringBuilder();
                foreach (ItemTypePropertyInfo property in itemType.Properties)
                {
                    addSecurityParameters.Append(",@");
                    addSecurityParameters.Append(property.Name);
                    addSecurityParameters.Append(" ");
                    addSecurityParameters.Append(GetSqlParameterType(property));
                    if(property.HasDefaultValue)
                    {
                        addSecurityParameters.Append("=");
                        addSecurityParameters.Append(GetSqlDefaultValueConstant(property));
                    }
                }
                
                // Add code that creates a function defining the add security on the item type
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateAddItemSecurityFunction, itemType.Name));
                writer.WriteLine("CREATE FUNCTION [" + itemType.Name + "$AddSecurity](@UserKey nvarchar(250)" + addSecurityParameters.ToString() + ")");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, itemType.AddRightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + itemType.Name + "$AddSecurity] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Write code that defines a view
        /// </summary>
        /// <param name="view">View info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteView(ViewInfo view, IndentedTextWriter writer)
        {
            // Add code that creates a function defining the view
            writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateViewFunction, view.Name));
            writer.WriteLine("CREATE FUNCTION [" + view.Name + "](@UserKey nvarchar(250)" + GetParameterText(view.Parameters) + ")");
            writer.WriteLine("RETURNS TABLE");
            writer.WriteLine("AS");
            writer.WriteLine("RETURN (");
            writer.Indent++;
            WriteStatements(view.Implementation, writer);
            writer.Indent--;
            writer.WriteLine(")");
            writer.WriteLine("GO");
            writer.WriteLine("GRANT SELECT ON [" + view.Name + "] TO LearningStore");
            writer.WriteLine("GO");
            writer.WriteLine();
        }

        /// <summary>
        /// Write the code that defines the security functions for a view
        /// </summary>
        /// <param name="view">View info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteViewSecurity(ViewInfo view, IndentedTextWriter writer)
        {
            if(view.RightExpressions.Count > 0)
            {
                // Add code that creates a function defining the security on the view
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateViewSecurityFunction, view.Name));
                writer.WriteLine("CREATE FUNCTION [" + view.Name + "$Security](@UserKey nvarchar(250)" + GetParameterText(view.Parameters) + ")");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, view.RightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + view.Name + "$Security] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Write the code that defines the security functions for a right
        /// </summary>
        /// <param name="right">Right info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteRightSecurity(RightInfo right, IndentedTextWriter writer)
        {
            if(right.RightExpressions.Count > 0)
            {
                // Add code that creates a function defining the security on the right
                writer.WriteLine("-- " + String.Format(CultureInfo.CurrentCulture, Resources.SqlInitCreateRightSecurityFunction, right.Name));
                writer.WriteLine("CREATE FUNCTION [" + right.Name + "](@UserKey nvarchar(250)" + GetParameterText(right.Parameters) + ")");
                writer.WriteLine("RETURNS bit");
                writer.WriteLine("AS");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                WriteUnionedSecurityExpressions(writer, right.RightExpressions);
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("GO");
                writer.WriteLine("GRANT EXECUTE ON [" + right.Name + "] TO LearningStore");
                writer.WriteLine("GO");
                writer.WriteLine();
            }
        }

        /// <summary>
        /// Write the code that drops all the security functions pointed to in
        /// the schema
        /// </summary>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteDropFunctions(IndentedTextWriter writer)
        {
            // Declare variables
            writer.WriteLine("DECLARE @name varchar(100)");
            writer.WriteLine("DECLARE @command varchar(max)");

            // Write code that deletes the functions
            writer.WriteLine("DECLARE DropCursor CURSOR LOCAL FOR");
            writer.WriteLine("SELECT V.value('@Function', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/View') AS Result(V)");
            writer.WriteLine("WHERE V.value('@Function','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@SecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/View') AS Result(V)");
            writer.WriteLine("WHERE V.value('@SecurityFunction','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@ViewSecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/ItemType') AS Result(V)");
            writer.WriteLine("WHERE V.value('@ViewSecurityFunction','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@AddSecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/ItemType') AS Result(V)");
            writer.WriteLine("WHERE V.value('@AddSecurityFunction','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@DeleteSecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/ItemType') AS Result(V)");
            writer.WriteLine("WHERE V.value('@DeleteSecurityFunction','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@UpdateSecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/ItemType') AS Result(V)");
            writer.WriteLine("WHERE V.value('@UpdateSecurityFunction','nvarchar(100)') IS NOT NULL");
            writer.WriteLine("UNION");
            writer.WriteLine("SELECT V.value('@SecurityFunction', 'nvarchar(100)')");
            writer.WriteLine("FROM Configuration");
            writer.WriteLine("CROSS APPLY SchemaDefinition.nodes('/StoreSchema/Right') AS Result(V)");
            writer.WriteLine("WHERE V.value('@SecurityFunction','nvarchar(100)') IS NOT NULL");            
            writer.WriteLine("OPEN DropCursor");
            writer.WriteLine("FETCH NEXT FROM DropCursor INTO @name");
            writer.WriteLine("WHILE @@FETCH_STATUS = 0");
            writer.WriteLine("BEGIN");
            writer.Indent++;
            writer.WriteLine("SET @command = 'DROP FUNCTION [' + @name + ']'");
            writer.WriteLine("EXEC(@command)");
            writer.WriteLine("FETCH NEXT FROM DropCursor INTO @name");
            writer.Indent--;
            writer.WriteLine("END");            
            writer.WriteLine("DEALLOCATE DropCursor");
            writer.WriteLine();
        }
        
        /// <summary>
        /// Write a string that contains schema information about the parameters on a view or right
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteSchemaParametersString(IEnumerable<ParameterInfo> parameters, IndentedTextWriter writer)
        {
            // Write the parameters
            foreach (ParameterInfo parameter in parameters)
            {
                writer.Write("'<Parameter Name=\"" + parameter.Name +
                    "\" TypeCode=\"" + XmlConvert.ToString((int)parameter.ValueTypeCode) +
                    "\" Nullable=\"true\"");

                // Write the referenced item information if needed
                if (parameter.ValueTypeCode == LearningStoreValueTypeCode.ItemIdentifier)
                {
                    writer.Write(" ReferencedItemTypeName=\"" + parameter.GetReferencedItemType().Name + "\"");
                }

                // Write the enum information if needed
                if (parameter.ValueTypeCode == LearningStoreValueTypeCode.Enumeration)
                {
                    writer.Write(" EnumName=\"" + parameter.GetEnum().Name + "\"");
                }

                writer.WriteLine("/>' +");
            }
        }

        /// <summary>
        /// Write a string that contains schema information about an enum
        /// </summary>
        /// <param name="en">Enum info</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteSchemaEnumString(EnumInfo en, IndentedTextWriter writer)
        {            
            writer.WriteLine("'<Enum Name=\"" + en.Name + "\">' + ");
            writer.Indent++;

            // Write the values            
            foreach (string valueName in en.ValueNames)
            {
                writer.WriteLine("'<Value Name=\"" + valueName +
                    "\" Value=\"" + XmlConvert.ToString(en.GetIntegerValue(valueName)) + "\"/>' + ");
            }

            writer.Indent--;
            writer.WriteLine("'</Enum>'");
        }

        /// <summary>
        /// Write a string that contains schema information about an item type
        /// </summary>
        /// <param name="itemType">Item type</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteSchemaItemTypeString(ItemTypeInfo itemType, IndentedTextWriter writer)
        {
            writer.Write("'<ItemType Name=\"" + itemType.Name +
                "\" ViewFunction=\"" + itemType.Name + "$DefaultView" + "\"");
            if(itemType.QueryRightExpressions.Count > 0)
                writer.Write(" ViewSecurityFunction=\"" + itemType.Name + "$DefaultViewSecurity\"");
            if(itemType.AddRightExpressions.Count > 0)
                writer.Write(" AddSecurityFunction=\"" + itemType.Name + "$AddSecurity\"");
            if (itemType.DeleteRightExpressions.Count > 0)
                writer.Write(" DeleteSecurityFunction=\"" + itemType.Name + "$DeleteSecurity\"");
            if (itemType.UpdateRightExpressions.Count > 0)
                writer.Write(" UpdateSecurityFunction=\"" + itemType.Name + "$UpdateSecurity\"");
            writer.WriteLine(">' + ");
            writer.Indent++;

            // Write the properties            
            foreach (ItemTypePropertyInfo property in itemType.Properties)
            {
                writer.Write("'<Property Name=\"" + property.Name +
                    "\" TypeCode=\"" + XmlConvert.ToString((int)property.ValueTypeCode) +
                    "\" Nullable=\"" + XmlConvert.ToString(property.Nullable) +
                    "\" HasDefault=\"" + XmlConvert.ToString(property.HasDefaultValue) + "\"");

                // Write the referenced item information if needed
                if (property.ValueTypeCode == LearningStoreValueTypeCode.ItemIdentifier)
                {
                    writer.Write(" ReferencedItemTypeName=\"" + property.GetReferencedItemType().Name + "\"");
                }

                // Write the enum information if needed
                if (property.ValueTypeCode == LearningStoreValueTypeCode.Enumeration)
                {
                    writer.Write(" EnumName=\"" + property.GetEnum().Name + "\"");
                }

                writer.WriteLine("/>' +");
            }

            writer.Indent--;
            writer.WriteLine("'</ItemType>'");
        }

        /// <summary>
        /// Write a string that contains schema information about a view
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteSchemaViewString(ViewInfo view, IndentedTextWriter writer)
        {
            writer.Write("'<View Name=\"" + view.Name +
                "\" Function=\"" + view.Name + "\"");
            if(view.RightExpressions.Count > 0)
                writer.Write(" SecurityFunction=\"" + view.Name + "$Security\"");
            writer.WriteLine(">' + ");
            writer.Indent++;

            // Write the columns            
            ReadOnlyCollection<ViewColumnInfo> columns = view.Columns;
            foreach(ViewColumnInfo column in columns)
            {
                writer.Write("'<Column Name=\"" + column.Name +
                    "\" TypeCode=\"" + XmlConvert.ToString((int)column.ValueTypeCode) +
                    "\" Nullable=\"true\"");

                // Write the referenced item information if needed
                if (column.ValueTypeCode == LearningStoreValueTypeCode.ItemIdentifier)
                {
                    writer.Write(" ReferencedItemTypeName=\"" + column.GetReferencedItemType().Name + "\"");
                }

                // Write the enum information if needed
                if (column.ValueTypeCode == LearningStoreValueTypeCode.Enumeration)
                {
                    writer.Write(" EnumName=\"" + column.GetEnum().Name + "\"");
                }

                writer.WriteLine("/>' +");
            }

            // Write the parameters
            WriteSchemaParametersString(view.Parameters, writer);
            
            writer.Indent--;
            writer.WriteLine("'</View>'");
        }

        /// <summary>
        /// Write a string that contains schema information about a right
        /// </summary>
        /// <param name="right">Right</param>
        /// <param name="writer">Location in which to write data</param>
        private static void WriteSchemaRightString(RightInfo right, IndentedTextWriter writer)
        {
            writer.Write("'<Right Name=\"" + right.Name + "\"");
            if(right.RightExpressions.Count > 0)
                writer.Write(" SecurityFunction=\"" + right.Name + "\"");
            writer.WriteLine(">' + ");
            writer.Indent++;

            // Write the parameters
            WriteSchemaParametersString(right.Parameters, writer);

            writer.Indent--;
            writer.WriteLine("'</Right>'");
        }

        /// <summary>
        /// Write code that assigns schema information to the "@schema" variable.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="writer"></param>        
        private static void WriteSchema(SchemaInfo schema, IndentedTextWriter writer)
        {
            // Write the start of the declaration
            writer.WriteLine("DECLARE @schema varchar(max)");
            writer.WriteLine("SET @schema = '<StoreSchema>'");

            // Write the item types
            foreach(ItemTypeInfo itemType in schema.ItemTypes)
            {
                writer.WriteLine("SET @schema = @schema +");
                writer.Indent++;
                WriteSchemaItemTypeString(itemType, writer);
                writer.Indent--;
            }

            // Write the enums
            foreach(EnumInfo en in schema.Enums)
            {
                writer.WriteLine("SET @schema = @schema +");
                writer.Indent++;
                WriteSchemaEnumString(en, writer);
                writer.Indent--;
            }

            // Write the views
            foreach(ViewInfo view in schema.Views)
            {
                writer.WriteLine("SET @schema = @schema +");
                writer.Indent++;
                WriteSchemaViewString(view, writer);
                writer.Indent--;
            }

            // Write the rights
            foreach (RightInfo right in schema.Rights)
            {
                writer.WriteLine("SET @schema = @schema +");
                writer.Indent++;
                WriteSchemaRightString(right, writer);
                writer.Indent--;
            }

            // Write the end of the declaration
            writer.WriteLine("SET @schema = @schema + '</StoreSchema>'");
        }

        /// <summary>
        /// Write code that removes old view and security-related information from the
        /// @schema variable, and adds the new view and security-related information
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="writer"></param>
        private static void WriteSchemaUpgrade(SchemaInfo schema, IndentedTextWriter writer)
        {
            // Declare variables
            writer.WriteLine("DECLARE @command varchar(max)");

            // Write code that removes the security functions from the item types
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/ItemType/@ViewSecurityFunction')");
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/ItemType/@AddSecurityFunction')");
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/ItemType/@DeleteSecurityFunction')");
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/ItemType/@UpdateSecurityFunction')");
            
            // Write code that removes the views
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/View')");
            
            // Write code that removes the rights
            writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('delete /StoreSchema/Right')");
            
            // Write code that re-adds security functions for item types
            foreach(ItemTypeInfo itemType in schema.ItemTypes)
            {
                if(itemType.QueryRightExpressions.Count > 0)
                    writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('insert attribute ViewSecurityFunction{\"" +
                        itemType.Name + "$DefaultViewSecurity\"} into /StoreSchema[1]/ItemType[@Name=\"" +
                        itemType.Name + "\"][1]')");
                if (itemType.AddRightExpressions.Count > 0)
                    writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('insert attribute AddSecurityFunction{\"" +
                        itemType.Name + "$AddSecurity\"} into /StoreSchema[1]/ItemType[@Name=\"" +
                        itemType.Name + "\"][1]')");
                if (itemType.DeleteRightExpressions.Count > 0)
                    writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('insert attribute DeleteSecurityFunction{\"" +
                        itemType.Name + "$DeleteSecurity\"} into /StoreSchema[1]/ItemType[@Name=\"" +
                        itemType.Name + "\"][1]')");
                if (itemType.UpdateRightExpressions.Count > 0)
                    writer.WriteLine("UPDATE Configuration SET SchemaDefinition.modify('insert attribute UpdateSecurityFunction{\"" +
                        itemType.Name + "$UpdateSecurity\"} into /StoreSchema[1]/ItemType[@Name=\"" +
                        itemType.Name + "\"][1]')");
            }

            // Write code that re-adds the views
            foreach(ViewInfo view in schema.Views)
            {
                writer.WriteLine("SET @command = 'UPDATE Configuration SET SchemaDefinition.modify(''insert ' +");
                writer.Indent++;
                WriteSchemaViewString(view, writer);
                writer.WriteLine("+ ' into /StoreSchema[1]'')'");
                writer.Indent--;
                writer.WriteLine("EXEC(@command)");
            }
            
            // Write code that re-adds the rights
            foreach (RightInfo right in schema.Rights)
            {
                writer.WriteLine("SET @command = 'UPDATE Configuration SET SchemaDefinition.modify(''insert ' +");
                writer.Indent++;
                WriteSchemaRightString(right, writer);
                writer.WriteLine("+ ' into /StoreSchema[1]'')'");
                writer.Indent--;
                writer.WriteLine("EXEC(@command)");
            }            
        }
        
        /// <summary>
        /// Write a .sql init file for a schema
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="path">Path to the output file</param>
        public static void Write(SchemaInfo schema, string path)
        {
            // Create a writer to write the code
            StreamWriter stringWriter = new StreamWriter(path);
            using(stringWriter)
            {
                IndentedTextWriter writer = new IndentedTextWriter(stringWriter);
                using(writer)
                {
                    // Write the comment at the top saying that the code was generated by a tool
                    writer.Write(Resources.SqlInitHeader);
                    writer.WriteLine();

                    // Write code that fails if the master DB is being used
                    writer.WriteLine("IF DB_NAME()='master'");
                    writer.WriteLine("BEGIN");
                    writer.Indent++;
                    writer.WriteLine("RAISERROR(N'" + Resources.SqlInitCantUseMaster + "',16,1)");
                    writer.WriteLine("RETURN");
                    writer.Indent--;
                    writer.WriteLine("END");
                    writer.WriteLine();

                    // Write code that sets the configuration
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine("SET XACT_ABORT ON");
                    writer.WriteLine("SET IMPLICIT_TRANSACTIONS ON");
                    writer.WriteLine("SET QUOTED_IDENTIFIER ON");
                    writer.WriteLine();

                    // Write SqlBefore
                    foreach(string sqlBefore in schema.SqlBefore)
                    {
                        WriteStatements(sqlBefore, writer);
                        writer.WriteLine();
                    }
                    
                    // Write code that creates the configuration table
                    writer.WriteLine("-- " + Resources.SqlInitCreateConfigurationTable);
                    writer.WriteLine("CREATE TABLE Configuration (");
                    writer.Indent++;
                    writer.WriteLine("EngineVersion int NOT NULL,");
                    writer.WriteLine("SchemaDefinition xml NOT NULL");
                    writer.Indent--;
                    writer.WriteLine(")");

                    // Write code that creates the variable containing the schema information
                    WriteSchema(schema, writer);

                    // Write code that adds a row to the configuration table
                    writer.WriteLine("INSERT INTO Configuration (");
                    writer.Indent++;
                    writer.WriteLine("EngineVersion,");
                    writer.WriteLine("SchemaDefinition");
                    writer.Indent--;
                    writer.WriteLine(") VALUES (");
                    writer.Indent++;
                    writer.WriteLine("1,@schema");
                    writer.Indent--;
                    writer.WriteLine(")");
                    writer.WriteLine();

                    // Create the role
                    writer.WriteLine("-- " + Resources.SqlInitCreateRole);
                    writer.WriteLine("CREATE ROLE LearningStore");
                    writer.WriteLine("GRANT SELECT ON Configuration TO LearningStore");
                    writer.WriteLine();
                                        
                    // Enumerate through each enum type
                    foreach (EnumInfo en in schema.Enums)
                    {
                        WriteEnum(en, writer);
                    }

                    // Enumerate through each item type
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemType(itemType, writer);
                    }

                    // Enumerate through each item type again to add constraints & indexes
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemTypeConstraintsAndIndexes(itemType, writer);
                    }

                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Enumerate through each item type again to add security
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemTypeSecurity(itemType, writer);
                    }

                    // Enumerate through each view
                    foreach (ViewInfo view in schema.Views)
                    {
                        WriteView(view, writer);
                        WriteViewSecurity(view, writer);
                    }

                    // Enumerate through each right
                    foreach (RightInfo right in schema.Rights)
                    {
                        WriteRightSecurity(right, writer);
                    }

                    // Enumerate through each item type again to add default views
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemTypeDefaultView(itemType, writer);
                        WriteItemTypeDefaultViewSecurity(itemType, writer);
                    }

                    // Write SqlAfter
                    foreach(ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        foreach (string sqlAfter in itemType.SqlAfter)
                        {
                            WriteStatements(sqlAfter, writer);
                            writer.WriteLine();
                        }
                    }
                    foreach (string sqlAfter in schema.SqlAfter)
                    {
                        WriteStatements(sqlAfter, writer);
                        writer.WriteLine();
                    }

                    // Write code that commits the transaction
                    writer.WriteLine("COMMIT TRANSACTION");
                    writer.WriteLine("GO");
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Write a .sql file that upgrades the view, right, and security definitions.
        /// It doesn't update items or enums.
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <param name="path">Path to the output file</param>
        public static void WriteUpgrade(SchemaInfo schema, string path)
        {
            // Create a writer to write the code
            StreamWriter stringWriter = new StreamWriter(path);
            using (stringWriter)
            {
                IndentedTextWriter writer = new IndentedTextWriter(stringWriter);
                using (writer)
                {
                    // Write the comment at the top saying that the code was generated by a tool
                    writer.Write(Resources.SqlInitHeader);
                    writer.WriteLine();

                    // Write code that fails if the master DB is being used
                    writer.WriteLine("IF DB_NAME()='master'");
                    writer.WriteLine("BEGIN");
                    writer.Indent++;
                    writer.WriteLine("RAISERROR(N'" + Resources.SqlInitCantUseMaster + "',16,1)");
                    writer.WriteLine("RETURN");
                    writer.Indent--;
                    writer.WriteLine("END");
                    writer.WriteLine();

                    // Write code that verifies that the configuration table exists
                    writer.WriteLine("IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Configuration')");
                    writer.WriteLine("BEGIN");
                    writer.Indent++;
                    writer.WriteLine("RAISERROR(N'" + Resources.SqlInitCantUpgradeEmptyDatabase + "',16,1)");
                    writer.WriteLine("RETURN");
                    writer.Indent--;
                    writer.WriteLine("END");
                    writer.WriteLine();

                    // Write code that sets the configuration
                    writer.WriteLine("SET NOCOUNT ON");
                    writer.WriteLine("SET XACT_ABORT ON");
                    writer.WriteLine("SET IMPLICIT_TRANSACTIONS ON");
                    writer.WriteLine("SET QUOTED_IDENTIFIER ON");
                    writer.WriteLine();

                    // Write the code that drops all the view and security functions
                    writer.WriteLine("-- " + Resources.SqlInitDropFunctions);
                    WriteDropFunctions(writer);

                    writer.WriteLine("GO");
                    writer.WriteLine();

                    // Write the code that updates the schema
                    writer.WriteLine("-- " + Resources.SqlInitUpdateSchemaXml);
                    WriteSchemaUpgrade(schema, writer);

                    writer.WriteLine("GO");
                    writer.WriteLine();
                    
                    // Enumerate through each item type
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemTypeSecurity(itemType, writer);
                    }

                    // Enumerate through each view
                    foreach (ViewInfo view in schema.Views)
                    {
                        WriteView(view, writer);
                        WriteViewSecurity(view, writer);
                    }

                    // Enumerate through each right
                    foreach (RightInfo right in schema.Rights)
                    {
                        WriteRightSecurity(right, writer);
                    }

                    // Enumerate through each item type again to add default views
                    foreach (ItemTypeInfo itemType in schema.ItemTypes)
                    {
                        WriteItemTypeDefaultViewSecurity(itemType, writer);
                    }

                    // Write code that commits the transaction
                    writer.WriteLine("COMMIT TRANSACTION");
                    writer.WriteLine("GO");
                    writer.WriteLine();
                }
            }
        }
        
        /// <summary>
        /// Write a RETURN statement that "ors" together expressions, or just "RETURN 0" if there are
        /// no expressions
        /// </summary>
        /// <param name="writer">Location in which to write data</param>
        /// <param name="expressions">List of expressions</param>
        private static void WriteUnionedSecurityExpressions(IndentedTextWriter writer, IList<string> expressions)
        {
            if(expressions.Count == 0)
                throw new InternalException("SCMP5005");
            else
            {
                StringBuilder ret = new StringBuilder("RETURN ");
                for(int i=0; i<expressions.Count; i++)
                {
                    if(i > 0)
                        ret.Append(" | ");
                    ret.Append("(");
                    ret.Append(expressions[i]);
                    ret.Append(")");
                }
                ret.Append("\r\n");
                WriteStatements(ret.ToString(), writer);
            }
        }

        /// <summary>
        /// Write the statements read from the XML file (an InnerXml of one node)
        /// </summary>
        /// <param name="s">The statements</param>
        /// <param name="writer">Location in which to write the data</param>
        private static void WriteStatements(string s, IndentedTextWriter writer)
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

            // Write each of the lines to the output
            foreach (string line in lines)
                writer.WriteLine(line);
        }

        /// <summary>
        /// Return a string containing the default value of the property (in terms SQL can understand)
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns>The constant</returns>
        private static string GetSqlDefaultValueConstant(ItemTypePropertyInfo property)
        {
            if(!property.HasDefaultValue)
                throw new InternalException("SCMP5000");
            object value = property.GetDefaultValue();            

            if (value == null)
                return "NULL";

            switch (property.ValueTypeCode)
            {
                case LearningStoreValueTypeCode.ByteArray:
                    byte[] byteValue = (byte[])value;
                    return "0x" + BitConverter.ToString(byteValue, 0,
                            byteValue.Length).Replace("-", "");

                case LearningStoreValueTypeCode.Boolean:
                    return (((bool)value) ? '1' : '0').ToString();

                case LearningStoreValueTypeCode.DateTime:
                    return "{ts '" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "'}";

                case LearningStoreValueTypeCode.Double:
                case LearningStoreValueTypeCode.Single:
                case LearningStoreValueTypeCode.Int32:
                case LearningStoreValueTypeCode.Enumeration:
                    return value.ToString();

                case LearningStoreValueTypeCode.String:
                    return "'" + value.ToString().Replace("'", "''") + "'";

                case LearningStoreValueTypeCode.Xml:
                    string xmlValue = value.ToString();
                    xmlValue = xmlValue.Replace("\r\n", " ");
                    xmlValue = xmlValue.Replace("\r"," ");
                    xmlValue = xmlValue.Replace("\n"," ");
                    return "'" + xmlValue.Replace("'", "''") + "'";

                case LearningStoreValueTypeCode.Guid:
                    return "'" + ((Guid)value).ToString() + "'";

                default:
                    throw new InternalException("SCMP5010");
            }
        }

        /// <summary>
        /// Return a string containing the type of the property in SQL text (not
        /// including nullable, default value, etc.)
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        private static string GetSqlParameterType(ItemTypePropertyInfo property)
        {
            string sqlType = null;
            switch (property.ValueTypeCode)
            {
                case LearningStoreValueTypeCode.ByteArray:
                    sqlType = "varbinary(" +
                        (((property.Length == null) || (property.Length > 8000)) ? "max" : property.Length.ToString()) +
                        ")";
                    break;

                case LearningStoreValueTypeCode.Boolean:
                    sqlType = "bit";
                    break;

                case LearningStoreValueTypeCode.DateTime:
                    sqlType = "datetime";
                    break;

                case LearningStoreValueTypeCode.Double:
                    sqlType = "float(53)";
                    break;

                case LearningStoreValueTypeCode.ItemIdentifier:
                    sqlType = "bigint";
                    break;

                case LearningStoreValueTypeCode.Single:
                    sqlType = "float(24)";
                    break;

                case LearningStoreValueTypeCode.String:
                    sqlType = "nvarchar(" +
                        (((property.Length == null) || (property.Length > 4000)) ? "max" : property.Length.ToString()) +
                        ")";
                    break;

                case LearningStoreValueTypeCode.Xml:
                    sqlType = "xml";
                    break;

                case LearningStoreValueTypeCode.Enumeration:
                    sqlType = "int";
                    break;

                case LearningStoreValueTypeCode.Int32:
                    sqlType = "int";
                    break;

                case LearningStoreValueTypeCode.Guid:
                    sqlType = "uniqueidentifier";
                    break;

                default:
                    throw new InternalException("SCMP5020");
            }

            return sqlType;
        }

        /// <summary>
        /// Return a string containing the type of the parameter in SQL text (not
        /// including nullable, default value, etc.)
        /// </summary>
        /// <param name="parameter">Parameter</param>
        /// <returns></returns>
        private static string GetSqlParameterType(ParameterInfo parameter)
        {
            string sqlType = null;
            switch (parameter.ValueTypeCode)
            {
                case LearningStoreValueTypeCode.ByteArray:
                    sqlType = "varbinary(max)";
                    break;

                case LearningStoreValueTypeCode.Boolean:
                    sqlType = "bit";
                    break;

                case LearningStoreValueTypeCode.DateTime:
                    sqlType = "datetime";
                    break;

                case LearningStoreValueTypeCode.Double:
                    sqlType = "float(53)";
                    break;

                case LearningStoreValueTypeCode.ItemIdentifier:
                    sqlType = "bigint";
                    break;

                case LearningStoreValueTypeCode.Single:
                    sqlType = "float(24)";
                    break;

                case LearningStoreValueTypeCode.String:
                    sqlType = "nvarchar(max)";
                    break;

                case LearningStoreValueTypeCode.Xml:
                    sqlType = "xml";
                    break;

                case LearningStoreValueTypeCode.Enumeration:
                    sqlType = "int";
                    break;

                case LearningStoreValueTypeCode.Int32:
                    sqlType = "int";
                    break;

                case LearningStoreValueTypeCode.Guid:
                    sqlType = "uniqueidentifier";
                    break;

                default:
                    throw new InternalException("SCMP5030");
            }

            return sqlType;
        }

        /// <summary>
        /// Return a string containing parameter names in order, ready to be sent as a
        /// function call to SQL
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <returns></returns>
        private static string GetParameterText(IEnumerable<ParameterInfo> parameters)
        {
            StringBuilder parametersString = new StringBuilder();
            foreach (ParameterInfo parameter in parameters)
            {
                parametersString.Append(",@");
                parametersString.Append(parameter.Name);
                parametersString.Append(" ");
                parametersString.Append(GetSqlParameterType(parameter));
                parametersString.Append("=NULL");
            }

            return parametersString.ToString();
        }

    }
}
