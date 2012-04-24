using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The results from a <see cref="DomainGroupEnumerator"/>.</summary>
    public class DomainGroupEnumeratorResults
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="DomainGroupEnumeratorResults"/>.</summary>
        public DomainGroupEnumeratorResults()
        {
            Users = new List<SPUser>();
            Errors = new List<string>();
            IncludeGroup = true;
        }
#endregion constructors

#region properties
        /// <summary>Whether to include the group or not.</summary>
        public bool IncludeGroup { get; set; }

        /// <summary>The list of users in the group.</summary>
        public List<SPUser> Users { get; private set; }

        /// <summary>A list of errors.</summary>
        public List<string> Errors { get; private set; }
#endregion properties

#region public methods
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }
}

