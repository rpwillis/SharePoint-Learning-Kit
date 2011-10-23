/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;

namespace Microsoft.LearningComponents
{
    public class CompressionException: Exception
    {
        public CompressionException()
        {
        }

        public CompressionException(string message)
            : base(message)
        {
        }

        public CompressionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CompressionException(System.Runtime.Serialization.SerializationInfo serializationInfo,System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

    }
}
