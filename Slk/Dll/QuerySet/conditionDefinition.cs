using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// A "&lt;Condition&gt;" element from an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
    /// </summary>
    ///
    [DebuggerDisplay("ConditionDefinition {ViewColumnName} {Operator} ...")]
    public class ConditionDefinition
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>ViewColumnName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_viewColumnName;

        /// <summary>
        /// Holds the value of the <c>Operator</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        LearningStoreConditionOperator m_operator;

        /// <summary>
        /// Holds the value of the <c>Value</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_value;

        /// <summary>
        /// Holds the value of the <c>MacroName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_macroName;

        /// <summary>
        /// Holds the value of the <c>NoConditionOnNull</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_noConditionOnNull;

        /// <summary>
        /// Holds the value of the <c>LineNumber</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int m_lineNumber;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// The value of the "ViewColumnName" attribute.  This is the name of the LearningStore column
        /// of the view specified by <c>QueryDefinition.ViewName</c>.
        /// </summary>
        public string ViewColumnName
        {
            [DebuggerStepThrough]
            get
            {
                return m_viewColumnName;
            }
        }

        /// <summary>
        /// The value of the "Operator" attribute.  This is the operator used to compare the
        /// database column value with the value obtained using the <c>Value</c> or
        /// <c>MacroName</c> property.
        /// </summary>
        public LearningStoreConditionOperator Operator
        {
            [DebuggerStepThrough]
            get
            {
                return m_operator;
            }
        }

        /// <summary>
        /// The value of the "Value" attribute.  This is the value (if any) that the database
        /// column value is being compared against; <c>null</c> if no value was specified.
        /// </summary>
        public string Value
        {
            [DebuggerStepThrough]
            get
            {
                return m_value;
            }
        }

        /// <summary>
        /// The value of the "MacroName" attribute.  This is the name of the macro variable that
        /// the value (if any) that the database column value is being compared against;
        /// <c>null</c> if no macro name was specified.
        /// </summary>
        public string MacroName
        {
            [DebuggerStepThrough]
            get
            {
                return m_macroName;
            }
        }

        /// <summary>
        /// The value of the "NoConditionOnNull" attribute.  If <c>true</c>, then this condition is
        /// omitted from the query if the value of the macro is <c>null</c>.
        /// </summary>
        public bool NoConditionOnNull
        {
            [DebuggerStepThrough]
            get
            {
                return m_noConditionOnNull;
            }
        }

        /// <summary>
        /// The line number of the "&lt;Condition&gt;" element with the XML file.
        /// </summary>
        public int LineNumber
        {
            [DebuggerStepThrough]
            get
            {
                return m_lineNumber;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="viewColumnName">The value to use for the <c>ViewColumnName</c> property.
        ///     </param>
        ///
        /// <param name="oper">The value to use for the <c>Operator</c> property.  Note that
        ///     if this value is "IsNull" or "IsNotNull" then
        ///     <c>LearningStoreConditionOperator.Equal</c> or
        ///     <c>LearningStoreConditionOperator.NotEqual</c>, respectively, is used for the
        ///     <c>Operator</c> property; in this case <c>null</c> should be used for
        ///     <paramref name="value"/>.</param>
        ///
        /// <param name="value">The value to use for the <c>Value</c> property.</param>
        ///
        /// <param name="macroName">The value to use for the <c>MacroName</c> property.</param>
        ///
        /// <param name="noConditionOnNull">The value to use for the <c>NoConditionOnNull</c> property.
        ///     </param>
        ///
        /// <param name="lineNumber">The line number of the "&lt;Condition&gt;" element with the
        ///     XML file.</param>
        ///
        /// <remarks>
        /// Note that one of <paramref name="value"/> and <paramref name="macroName"/> must be
        /// non-null unless <paramref name="oper"/> equals "IsNull" or "IsNotNull".
        /// </remarks>
        ///
        internal ConditionDefinition(string viewColumnName, string oper, string value, string macroName, bool noConditionOnNull, int lineNumber)
        {
            // store state
            m_viewColumnName = viewColumnName;
            switch (oper)
            {
            case "Equal":
                m_operator = LearningStoreConditionOperator.Equal;
                break;
            case "GreaterThan":
                m_operator = LearningStoreConditionOperator.GreaterThan;
                break;
            case "GreaterThanEqual":
                m_operator = LearningStoreConditionOperator.GreaterThanEqual;
                break;
            case "LessThan":
                m_operator = LearningStoreConditionOperator.LessThan;
                break;
            case "LessThanEqual":
                m_operator = LearningStoreConditionOperator.LessThanEqual;
                break;
            case "NotEqual":
                m_operator = LearningStoreConditionOperator.NotEqual;
                break;
            case "IsNull":
                m_operator = LearningStoreConditionOperator.Equal;
                if ((value != null) || (macroName != null))
                {
                    throw new ArgumentException(SlkCulture.GetResources().SlkUtilitiesValueNullifIsNull, "operator");
                }
                break;
            case "IsNotNull":
                m_operator = LearningStoreConditionOperator.NotEqual;
                if ((value != null) || (macroName != null))
                {
                    throw new ArgumentException(SlkCulture.GetResources().SlkUtilitiesValueNullifIsNotNull, "operator");
                }
                break;
            }

            m_value = value;

            if (string.IsNullOrEmpty(macroName) == false)
            {
                MacroResolver.ValidateMacro(macroName);
            }

            m_macroName = macroName;
            m_noConditionOnNull = noConditionOnNull;
            m_lineNumber = lineNumber;

            // only one of <value> and <macroName> may be provided
            if ((value != null) && (macroName != null))
            {
                throw new ArgumentException(SlkCulture.GetResources().SlkUtilitiesOneValueNonNull, "value");
            }

            // if <noConditionOnNull> is true, <macroName> must be provided
            if (noConditionOnNull && (macroName == null))
            {
                throw new ArgumentException(SlkCulture.GetResources().SlkUtilitiesMacroNameNotProvided, "noConditionOnNull");
            }
        }
    }
}

