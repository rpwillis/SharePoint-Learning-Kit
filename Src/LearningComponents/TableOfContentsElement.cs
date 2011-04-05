/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Represents an element in the Table of Contents
    /// </summary>
    public abstract class TableOfContentsElement
    {
        /// <summary>
        /// Internal constructor added so no 3rd parties can extend this class
        /// </summary>
        internal TableOfContentsElement()
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
        }

        /// <summary>
        /// Gets the title for this table of contents element.
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Gets the children of this table of contents element.
        /// </summary>
        public abstract ReadOnlyCollection<TableOfContentsElement> Children { get; }

        /// <summary>
        /// Gets whether or not this table of contents element is valid to navigate to via Choice navigation.
        /// </summary>
        /// <remarks>
        /// This value is only guaranteed valid at the time immediately after the calling of
        /// <c>LoadTableOfContents</c>.  Changes made to the current activity's data model or
        /// navigations performed after this call may change whether or not this element is 
        /// actually valid to navigate to, but this will not be reflected in this value unless
        /// or until <c>LoadTableOfContents</c> is called again.
        /// </remarks>
        public abstract bool IsValidChoiceNavigationDestination { get; }

        /// <summary>
        /// Gets the type of the resource associated with this table of contents element.
        /// </summary>
        public abstract ResourceType ResourceType { get; }

        /// <summary>
        /// Gets a unique identifier for the activity associated with this table of contents element.
        /// </summary>
        /// <remarks>
        /// Using this identifier within <c>NavigateTo</c> instead of the activity's string identifier
        /// will result in improved performance.
        /// </remarks>
        public abstract long ActivityId { get; }

        /// <summary>
        /// Gets whether or not this table of contents element is displayed when the structure of the package is displayed or rendered.
        /// </summary>
        public abstract bool IsVisible { get; }

        /// <summary>
        /// Gets whether or not this table of contents element has at least one descendant child, any level deep, with a value of
        /// <c>true</c> for <Mth>IsVisible</Mth>.
        /// </summary>
        /// <remarks>
        /// This value is used to determine how to display the UI of nodes - whether the node should be displayed as having available
        /// children or not.
        /// </remarks>
        public abstract bool HasVisibleChildren { get; }
    }
}
