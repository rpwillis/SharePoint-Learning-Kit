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
    /// ValidationResult is the information about a particular error 
    /// or warning that was received during the process of validating a package.
    /// </summary>  
    [Serializable]
    public class ValidationResult : ISerializable
    {
        private string m_message;
        private bool m_isError;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isError"><c>true</c> if this is a critical problem in the manifest, or <c>false</c> if this is a non-critical 
        /// problem in the manifest.</param>
        /// <param name="message">The result message.</param>
        internal ValidationResult(bool isError, string message)
        {
            m_isError = isError;
            m_message = message;
        }

        /// <summary>
        /// Constructor called during de-serialization.
        /// </summary>
        /// <param name="info">Info about object.</param>
        /// <param name="context">Context of deserialization.</param>
        protected ValidationResult(SerializationInfo info, StreamingContext context)
        {
            Utilities.ValidateParameterNonNull("info", info);

            m_isError = info.GetBoolean("isError");
            m_message = info.GetString("message");
        }

        /// <summary>
        /// True if this represents a warning.
        /// </summary>
        public bool IsWarning { get { return !m_isError; } }

        /// <summary>
        /// True if this represents an error.
        /// </summary>
        public bool IsError { get { return m_isError; } }

        /// <summary>
        /// The human-readable message for this warning or error.
        /// </summary>
        public string Message { get { return m_message; } }  // E.g. "There are multiple instances of element, <metadata>.  All but the first are ignored."

        #region ISerializable Members

        /// <summary>
        /// Return serialization data.
        /// </summary>
        /// <param name="info">The information for serialization</param>
        /// <param name="context">The context of serialization</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> of <paramref name="context"/>
        /// is not provided.</exception>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)] // Required by fxcop
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Utilities.ValidateParameterNonNull("info", info);

            info.AddValue("isError", IsError);
            info.AddValue("message", Message);
        }

        #endregion
    }
}
