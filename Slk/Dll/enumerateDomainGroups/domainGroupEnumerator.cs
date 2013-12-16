using System;
using System.Globalization;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The standard way of enumerating domain groups.</summary>
    public abstract class DomainGroupEnumerator
    {
#region constructors
#endregion constructors

#region properties
#endregion properties

#region public methods
        /// <summary>Enumerates a domain group.</summary>
        /// <param name="domainGroup">The domain group to enumerate.</param>
        /// <param name="web">The SPWeb the permission is for. This may be needed to add users to SharePoint if they aren't already added. Used by the default implementation as not all group members may have accessed SharePoint before. May not be needed for other implementation.</param>
        /// <param name="timeRemaining">The time remaining to enumerate all groups. This can be ignored as SLK will check it before enumerating any more groups if there are any. Used by the default implementation to check time elapsed before enumerating nested groups.</param>
        /// <param name="hideDisabledUsers">Whether to hide disabled members of the group. Primarily used by the default implementation.</param>
        /// <returns></returns>
        public abstract DomainGroupEnumeratorResults EnumerateGroup(SPUser domainGroup, SPWeb web, TimeSpan timeRemaining, bool hideDisabledUsers);
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }
}

