using System;
using System.Globalization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The mode to open a drop box file in.</summary>
    public enum DropBoxEditMode
    {
        /// <summary>Open in view mode.</summary>
        View,
        /// <summary>Open in edit mode.</summary>
        Edit
    }

    /// <summary>The details for a drop box edit.</summary>
    public struct DropBoxEditDetails
    {
        /// <summary>The url to use.</summary>
        public string Url;
        /// <summary>The on click event value.</summary>
        public string OnClick;
    }

}

