using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The standard way of enumerating domain groups.</summary>
    public class DomainGroupEnumeratorSlk : DomainGroupEnumerator
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
            ICollection<SPUserInfo> spUserInfos = new List<SPUserInfo>();
            try
            {
                DomainGroupUtilities enumerateDomainGroups = new DomainGroupUtilities(timeRemaining, hideDisabledUsers);
                spUserInfos = enumerateDomainGroups.EnumerateDomainGroup(domainGroup);

                foreach (string error in enumerateDomainGroups.Errors)
                {
                    results.Errors.Add(error);
                }

            }
            catch (DomainGroupEnumerationException exception)
            {
                results.Errors.Add(exception.Message);
                if (exception.InnerException != null)
                {
                    results.DetailedExceptions.Add(exception.InnerException);
                }
            }

            foreach (SPUserInfo spUserInfo in spUserInfos)
            {
                try
                {
                    SPUser spUserInGroup = web.EnsureUser(spUserInfo.LoginName);
                    results.Users.Add(spUserInGroup);
                }
                catch (SPException exception)
                {
                    SlkCulture culture = new SlkCulture();
                    results.Errors.Add(string.Format(culture.Culture, culture.Resources.ErrorCreatingSPSiteUser, web.Site.Url, exception));
                }
            }

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

