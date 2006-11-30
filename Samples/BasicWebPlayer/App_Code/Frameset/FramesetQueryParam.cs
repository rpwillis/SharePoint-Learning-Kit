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
        public const string AttemptId = "AttemptId";
        public const string Init = "I";
        public const string View = "View";
        public const string ActivityId = "ActId";

        // Content.aspx
        public const string ContentFilePath = "PF";

        private FramesetQueryParameter()
        {
            // Don't allow construction
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is verified
        public static string GetValueAsParameter(LearningStoreItemIdentifier value)
        {
            FramesetUtil.ValidateNonNullParameter("value", value);

            return value.GetKey().ToString(NumberFormatInfo.InvariantInfo);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]    // parameter is verified
        public static string GetValueAsParameter(SessionView value)
        {
            FramesetUtil.ValidateNonNullParameter("value", value);

            return value.ToString();
        }
    }
}
