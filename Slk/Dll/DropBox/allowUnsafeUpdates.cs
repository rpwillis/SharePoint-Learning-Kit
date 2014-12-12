using System;
using System.Globalization;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>A class that controls allowing unsafe updates for a web.</summary>
    sealed class AllowUnsafeUpdates : IDisposable
    {
        SPWeb web;
        bool currentAllowUnsafeUpdates;

#region constructors
        public AllowUnsafeUpdates(SPWeb web)
        {
            this.web = web;
            currentAllowUnsafeUpdates = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;
        }
#endregion constructors

#region properties
#endregion properties

#region public methods
        public void Dispose()
        {
            web.AllowUnsafeUpdates = currentAllowUnsafeUpdates;
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

