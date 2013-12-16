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
    /// Wraps a string in an object that includes an implementation of <c>IConvertable</c> that
    /// converts the string from an XML-format value (e.g. "3.14" for a floating-point number,
    /// "2006-03-21" for a date) to a given type (e.g. <c>double</c>, <c>DateTime</c>, etc.)
    /// </summary>
    ///
    internal class XmlValue : IConvertible
    {
        string m_stringValue;

        public XmlValue(string stringValue)
        {
            m_stringValue = stringValue;
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return XmlConvert.ToBoolean(m_stringValue);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return XmlConvert.ToByte(m_stringValue);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return XmlConvert.ToChar(m_stringValue);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return XmlConvert.ToDateTime(m_stringValue, XmlDateTimeSerializationMode.Unspecified);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return XmlConvert.ToDecimal(m_stringValue);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return XmlConvert.ToDouble(m_stringValue);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return XmlConvert.ToInt16(m_stringValue);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return XmlConvert.ToInt32(m_stringValue);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return XmlConvert.ToInt64(m_stringValue);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return XmlConvert.ToSByte(m_stringValue);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return XmlConvert.ToSingle(m_stringValue);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return m_stringValue;
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(Guid))
                return XmlConvert.ToGuid(m_stringValue);
            else
            if (conversionType == typeof(bool))
                return XmlConvert.ToBoolean(m_stringValue);
            else

            if (conversionType == typeof(byte))
                return XmlConvert.ToByte(m_stringValue);
            else

            if (conversionType == typeof(char))
                return XmlConvert.ToChar(m_stringValue);
            else

            if (conversionType == typeof(DateTime))
                return XmlConvert.ToDateTime(m_stringValue, XmlDateTimeSerializationMode.Unspecified);
            else

            if (conversionType == typeof(decimal))
                return XmlConvert.ToDecimal(m_stringValue);
            else

            if (conversionType == typeof(double))
                return XmlConvert.ToDouble(m_stringValue);
            else

            if (conversionType == typeof(short))
                return XmlConvert.ToInt16(m_stringValue);
            else

            if (conversionType == typeof(int))
                return XmlConvert.ToInt32(m_stringValue);
            else

            if (conversionType == typeof(long))
                return XmlConvert.ToInt64(m_stringValue);
            else

            if (conversionType == typeof(sbyte))
                return XmlConvert.ToSByte(m_stringValue);
            else

            if (conversionType == typeof(float))
                return XmlConvert.ToSingle(m_stringValue);
            else

            if (conversionType == typeof(string))
                return m_stringValue;
            else

            if (conversionType == typeof(ushort))
                return XmlConvert.ToUInt16(m_stringValue);
            else

            if (conversionType == typeof(uint))
                return XmlConvert.ToUInt32(m_stringValue);
            else

            if (conversionType == typeof(ulong))
                return XmlConvert.ToUInt64(m_stringValue);
            else
                throw new InvalidCastException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return XmlConvert.ToUInt16(m_stringValue);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return XmlConvert.ToUInt32(m_stringValue);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return XmlConvert.ToUInt64(m_stringValue);
        }

    }
}
