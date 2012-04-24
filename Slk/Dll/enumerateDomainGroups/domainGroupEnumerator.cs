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

