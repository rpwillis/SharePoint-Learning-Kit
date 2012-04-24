using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Prohibits the enumeration of groups.</summary>
    public class DomainGroupEnumeratorNoGroups : DomainGroupEnumerator
    {
#region constructors
#endregion constructors

#region properties
#endregion properties

#region public methods
        /// <summary>Enumerates a domain group.</summary>
        public override DomainGroupEnumeratorResults EnumerateGroup(SPUser domainGroup, SPWeb web, TimeSpan timeRemaining, bool hideDisabledUsers)
        {
            DomainGroupEnumeratorResults results = new DomainGroupEnumeratorResults();
            results.IncludeGroup = false;
            return results;
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }
}

