/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.LearningComponents.Storage;
using System.Globalization;
using Microsoft.SharePointLearningKit.Frameset;
using Microsoft.SharePointLearningKit;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.LearningComponents.Frameset
{
    /// <summary>
    /// Static class containing the strings that represent query parameters in the frameset.
    /// </summary>
    public partial class FramesetQueryParameter
    {
#pragma warning disable 1591
        public const string PlayAssignment = "play";
        public const string LearnerAssignmentId = "LearnerAssignmentId";
        public const string SlkView = "SlkView";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Src")]
        public const string Src = "Src";    // the page name that opened this page

        public static string GetValueAsParameter(AssignmentView view)
        {
            return view.ToString();
        }
#pragma warning restore 1591
    }
}
