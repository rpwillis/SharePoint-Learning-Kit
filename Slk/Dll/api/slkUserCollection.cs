using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents a collection of <c>SlkUser</c> objects.  The key for the collection is the
    /// <c>UserItemIdentifier</c> of each <c>SlkUser</c>.
    /// </summary>
    ///
    public class SlkUserCollection : KeyedCollection<UserItemIdentifier, SlkUser>
    {
        /// <summary>
        /// Identifies <c>SlkUser.UserId</c> as the key for items in this collection.
        /// </summary>
        protected override UserItemIdentifier GetKeyForItem(SlkUser item)
        {
            return item.UserId;
        }
    }
}

