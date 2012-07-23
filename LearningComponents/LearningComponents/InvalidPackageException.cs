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
    /// Exception to indicate the package contents are not valid.
    /// </summary>
    [Serializable]
    public class InvalidPackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        public InvalidPackageException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidPackageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InvalidPackageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
