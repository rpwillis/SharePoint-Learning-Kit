/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.LearningComponents.Manifest;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.LearningComponents
{

    /// <summary>
    /// The results from a validation operation.
    /// </summary>
    /// <remarks>
    /// This is a class to hold logged warnings and errors of type <Typ>ValidationResult</Typ>.
    /// </remarks>
    ///
    [Serializable]
    public class ValidationResults
    {
        private List<ValidationResult> m_results;
        private bool m_hasWarnings;
        private bool m_hasErrors;

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ValidationResults()
        {
            m_results = new List<ValidationResult>();
        }

        /// <summary>
        /// List of errors and warnings in the package.
        /// </summary>
        public ReadOnlyCollection<ValidationResult> Results { get { return new ReadOnlyCollection<ValidationResult>(m_results); } }

        /// <summary>
        /// True if there are errors in the log.  False if there are not.
        /// </summary>
        public bool HasErrors { get { return m_hasErrors; } }

        /// <summary>
        /// True if there are warnings in the log. False if there are not.
        /// </summary>
        public bool HasWarnings { get { return m_hasWarnings; } }

        /// <summary>
        /// Adds a warning to the log, if provided.
        /// </summary>
        /// <param name="message">The message to log.</param>
        internal void LogWarning(string message)
        {
            m_results.Add(new ValidationResult(false, message));
            m_hasWarnings = true;
        }

        /// <summary>
        /// Adds an error to the log, if provided.  Throws a <Typ>InvalidPackageException</Typ> if the
        /// <paramref name="throwInvalidPackageException"/> is <c>true</c>.
        /// </summary>
        /// <param name="throwInvalidPackageException">True to throw a <Typ>InvalidPackageException</Typ> containing the <paramref name="message"/>.
        /// </param>
        /// <param name="message">The message to log.</param>
        internal void LogError(bool throwInvalidPackageException, string message)
        {
            m_results.Add(new ValidationResult(true, message));
            m_hasErrors = true;

            if (throwInvalidPackageException)
            {
                throw new InvalidPackageException(message);
            }
        }

        /// <summary>Converts the log into Xml.</summary>
        public XmlReader ToXml()
        {
            // set <warnings> to XML that refers to the warnings in <registerResult.Log>
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
            {
                xmlWriter.WriteStartElement("Warnings");
                foreach (ValidationResult validationResult in Results)
                {
                    xmlWriter.WriteStartElement("Warning");
                    xmlWriter.WriteAttributeString("Type", validationResult.IsError ? "Error" : "Warning");
                    xmlWriter.WriteString(validationResult.Message);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }

            StringReader stringReader = new StringReader(stringBuilder.ToString());
            return XmlReader.Create(stringReader);
        }

        /// <summary>
        /// Adds a new <Typ>ValidationResult</Typ> item to the log.  
        /// </summary>
        /// <param name="result">The <Typ>ValidationResult</Typ> log item to add to the log.</param>
        internal void AddResult(ValidationResult result)
        {
            m_results.Add(result);
            if (result.IsError) m_hasErrors = true;
            else m_hasWarnings = true;
        }
    }
}
