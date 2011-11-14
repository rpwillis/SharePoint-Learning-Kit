using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A collections of changes between 2 collections.</summary>
    public class SlkUserCollectionChanges
    {
        /// <summary>The collection of additional users.</summary>
        public List<SlkUser> Additions { get; private set; }
        /// <summary>The collection of removed users.</summary>
        public List<SlkUser> Removals { get; private set; }

        /// <summary>Initializes a new instance of <see cref="SlkUserCollectionChanges"/>.</summary>
        /// <param name="originalCollection">The original collection of users.</param>
        /// <param name="newCollection">The new collection of users.</param>
        public SlkUserCollectionChanges(SlkUserCollection originalCollection, SlkUserCollection newCollection)
        {
            Additions = new List<SlkUser>();
            Removals = new List<SlkUser>();

            foreach (SlkUser user in newCollection)
            {
                if (originalCollection.Contains(user.UserId) == false)
                {
                    Additions.Add(newCollection[user.UserId]);
                }
            }

            if (newCollection.Count != originalCollection.Count + Additions.Count)
            {
                // There are some to remove
                foreach (SlkUser user in originalCollection)
                {
                    if (newCollection.Contains(user.UserId) == false)
                    {
                        Removals.Add(originalCollection[user.UserId]);
                    }
                }
            }
        }

        /// <summary>Shows if there are changes.</summary>
        public bool HasChanges
        {
            get 
            { 
                return (Additions.Count > 0 || Removals.Count > 0);
            }
        }
    }

}

