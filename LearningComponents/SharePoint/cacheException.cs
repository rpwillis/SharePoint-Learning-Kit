/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.SharePoint;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Security.Principal;		// for WindowsIndentity
using System.Runtime.InteropServices;	// for dllimport
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

//  CacheException - An exception that represents a problem in the
//              caching process.  This could be a missing file, a failure to
//              get a cache lock.
namespace Microsoft.LearningComponents.SharePoint
{

    #region CacheException class
    /// <summary>
    /// A special case exception indicating that an error occurred in the
    /// caching process.   
    /// </summary>
    [Serializable]
    public class CacheException : Exception
    {
        /// <summary>
        /// Creates an instance of CacheException.
        /// </summary>
        public CacheException() :base()             
        {
        }

        /// <summary>
        /// Creates an instance of CacheException with a message.
        /// </summary>
        /// <param name="message">A string describing the conditions leading to the exception</param>
        public CacheException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Creates an instance of CacheException with serialization information.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="context">The context of serialization.</param>
        protected CacheException(SerializationInfo serializationInfo, StreamingContext context) 
                    : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Creates an instance of CacheException based on another exception.
        /// </summary>
        /// <param name="message">A string describing the conditions leading to the exception.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        public CacheException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
    #endregion
}


