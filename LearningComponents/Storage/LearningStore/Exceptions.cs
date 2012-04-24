/* Copyright (c) Microsoft Corporation. All rights reserved. */

#region Using directives

using System;
using System.Runtime.Serialization;

#endregion

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Exception thrown when an item can't be found
    /// </summary>
    [Serializable]
    public class LearningStoreItemNotFoundException: Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        public LearningStoreItemNotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LearningStoreItemNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public LearningStoreItemNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        /// <param name="info">TODO</param>
        /// <param name="context">TODO</param>
        protected LearningStoreItemNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a constraint is violated during an operation.
    /// For example, attempting to add an item that references an item that doesn't
    /// exist.
    /// </summary>
    [Serializable]
    public class LearningStoreConstraintViolationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreConstraintViolationException</Typ> class.
        /// </summary>
        public LearningStoreConstraintViolationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreConstraintViolationException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LearningStoreConstraintViolationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreConstraintViolationException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public LearningStoreConstraintViolationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        /// <param name="info">TODO</param>
        /// <param name="context">TODO</param>
        protected LearningStoreConstraintViolationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a security check performed by LearningStore fails
    /// </summary>
    [Serializable]
    public class LearningStoreSecurityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreSecurityException</Typ> class.
        /// </summary>
        public LearningStoreSecurityException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreSecurityException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LearningStoreSecurityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreSecurityException</Typ> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public LearningStoreSecurityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>LearningStoreItemNotFoundException</Typ> class.
        /// </summary>
        /// <param name="info">TODO</param>
        /// <param name="context">TODO</param>
        protected LearningStoreSecurityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

}

