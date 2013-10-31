/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;

namespace Microsoft.LearningComponents
{
    /// <summary>An Exception raised by the compression module.</summary>
    public class CompressionException: Exception
    {
        /// <summary>Initializes a new instance of <see cref="CompressionException"/>.</summary>
        public CompressionException()
        {
        }

        /// <summary>Initializes a new instance of <see cref="CompressionException"/>.</summary>
        public CompressionException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of <see cref="CompressionException"/>.</summary>
        public CompressionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of <see cref="CompressionException"/>.</summary>
        protected CompressionException(System.Runtime.Serialization.SerializationInfo serializationInfo,System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

    }
}
