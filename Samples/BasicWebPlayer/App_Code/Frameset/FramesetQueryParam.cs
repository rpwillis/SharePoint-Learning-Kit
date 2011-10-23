/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.LearningComponents.Storage;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Static class containing the strings that represent query parameters in the frameset.
    /// </summary>
    public sealed partial class FramesetQueryParameter
    {

        // Shared between pages
        /// <summary>The attempt id query string name.</summary>
        public const string AttemptId = "AttemptId";
        /// <summary>Init value.</summary>
        public const string Init = "I";
        /// <summary>The view query string name.</summary>
        public const string View = "View";
        /// <summary>The activity id query string name.</summary>
        public const string ActivityId = "ActId";

        // Content.aspx
        /// <summary>Content FilePath value.</summary>
        public const string ContentFilePath = "PF";

        private FramesetQueryParameter()
        {
            // Don't allow construction
        }

        /// <summary>Gets the string value of a LearningStoreItemIdentifier key..</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is verified
        public static string GetValueAsParameter(LearningStoreItemIdentifier value)
        {
            FramesetUtil.ValidateNonNullParameter("value", value);

            return value.GetKey().ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Gets the string value of a SessionView.</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is verified
        public static string GetValueAsParameter(SessionView value)
        {
            FramesetUtil.ValidateNonNullParameter("value", value);

            return value.ToString();
        }

        /// <summary>Gets the string value of a Guid.</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is verified
        public static string GetValueAsParameter(Guid value)
        {
            FramesetUtil.ValidateNonNullParameter("value", value);

            return value.ToString();
        }
    }
}
