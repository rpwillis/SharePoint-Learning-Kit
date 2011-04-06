using System;
using System.Globalization;
using Microsoft.SharePoint;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents a SharePoint Learning Kit learner or instructor.
    /// </summary>
    public class SlkUser : IComparable<SlkUser>
    {
        string key;

#region properties
        /// <summary>The key for the user item.</summary>
        public string Key
        {
            get
            {
                if (key == null)
                {
                    key = SPUser.Sid;

                    if (String.IsNullOrEmpty(key))
                    {
                        string userName = SPUser.LoginName;
                        key = userName;
                        int pipeIndex = userName.IndexOf("|");
                        if (pipeIndex > -1)
                        {
                            // Claims based authentication
                            string claimUserName = userName.Substring(pipeIndex + 1);

                            if (claimUserName.IndexOf("\\") > -1)
                            {
                                // Probably Active Directory account
                                NTAccount account = new NTAccount(claimUserName);
                                try
                                {
                                    SecurityIdentifier sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
                                    key = sid.Value;
                                }
                                catch (IdentityNotMappedException)
                                {
                                    // Not a valid AD account so use the login name
                                }
                            }
                        }
                    }
                }

                return key;
            }
        }

        /// <summary>Gets the SharePoint Learning Kit <c>UserItemIdentifier</c> of the user. </summary>
        public UserItemIdentifier UserId { get; internal set; }

        /// <summary>Gets the SharePoint Learning Kit <c>LearningStoreItemIdentifier</c> of the user as associated with an assignment. </summary>
        public LearningStoreItemIdentifier AssignmentUserId { get; set; }

        /// <summary>Gets the SharePoint <c>SPUser</c> object that represents this user. </summary>
        public SPUser SPUser { get; internal set; }

        /// <summary>Gets the name of the user. </summary>
        public string Name { get; private set;}
#endregion properties

#region constructors
        /// <summary>Initializes an instance of this class, given a <c>UserItemIdentifier</c>.</summary>
        /// <remarks>When this constructor is used, the <c>SPUser</c> and <c>Name</c> properties are <c>null</c>.</remarks>
        /// <param name="userId">The SharePoint Learning Kit <c>UserItemIdentifier</c> of the user.</param>
        public SlkUser(UserItemIdentifier userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException("userId");
            }
                
            UserId = userId;
        }

        // If this constructor is used the calling code is responsible for setting the userId.
        internal SlkUser(SPUser spUser)
        {
            SPUser = spUser;
            Name = spUser.Name;
        }

        internal SlkUser(UserItemIdentifier userId, string name, string key) : this (userId)
        {
            Name = name;
            this.key = key;
        }
#endregion constructors

#region IComparable
        int IComparable<SlkUser>.CompareTo(SlkUser other)
        {
            return String.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }
#endregion IComparable
    }
}

