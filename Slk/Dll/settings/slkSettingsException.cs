using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Indicates an error within an SLK Settings XML file, i.e. an XML file with schema
    /// "urn:schemas-microsoft-com:sharepoint-learning-kit:settings".
    /// </summary>
    ///
    [Serializable]
    public class SlkSettingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
        /// </summary>
        public SlkSettingsException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SlkSettingsException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SlkSettingsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>SlkSettingsException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SlkSettingsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        
        /// <summary>
        /// Initializes an instance of this class, by formatting an error message.
        /// </summary>
        ///
        /// <param name="format">A <c>String.Format</c>-style format string.</param>
        ///
        /// <param name="args">Formatting arguments.</param>
        ///
        internal SlkSettingsException(string format, params object[] args) : base(String.Format(SlkCulture.GetCulture(), format, args))
        {
        }

        /// <summary>
        /// Initializes an instance of this class, by formatting an error message and prepending
        /// the line number within the XML file.
        /// </summary>
        ///
        /// <param name="xmlReader">The line number of the exception message will include the
        ///     current line number of this <c>XmlReader</c>.</param>
        ///
        /// <param name="format">A <c>String.Format</c>-style format string.</param>
        ///
        /// <param name="args">Formatting arguments.</param>
        ///
        internal SlkSettingsException(XmlReader xmlReader, string format, params object[] args) :
            base(String.Format(SlkCulture.GetCulture(), SlkCulture.GetResources().SlkUtilitiesSettingsException, ((IXmlLineInfo)xmlReader).LineNumber,
                String.Format(SlkCulture.GetCulture(), format, args)))
        {
        }

        /// <summary>
        /// Initializes an instance of this class, by formatting an error message and prepending
        /// the line number within the XML file.
        /// </summary>
        ///
        /// <param name="lineNumber">The line number (within the XML file) to include in the
        ///     exception message.</param>
        ///
        /// <param name="format">A <c>String.Format</c>-style format string.</param>
        ///
        /// <param name="args">Formatting arguments.</param>
        ///
        internal SlkSettingsException(int lineNumber, string format, params object[] args) :
            base(String.Format(SlkCulture.GetCulture(), SlkCulture.GetResources().SlkUtilitiesSettingsException, lineNumber,
                String.Format(SlkCulture.GetCulture(), format, args)))
        {
        }
    }
}

