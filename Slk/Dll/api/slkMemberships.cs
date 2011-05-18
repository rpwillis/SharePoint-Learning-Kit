using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Contains lists of instructors, learners, and learner groups on a given <c>SPWeb</c>. </summary>
    /// <remarks>
    /// A user is considered an instructor on a given SharePoint <c>SPWeb</c> if they have the
    /// instructor permission on that group, i.e. the permission defined by
    /// <c>SlkSPSiteMapping.InstructorPermission</c>.  Similarly, a user is considered a learner
    /// if they have the learner permission, <c>SlkSPSiteMapping.LearnerPermission</c>.
    /// </remarks>
    public class SlkMemberships
    {
        DateTime startTime;
        List<string> groupFailuresList = new List<string>();
        StringBuilder groupFailureDetailsBuilder = new StringBuilder();
        Dictionary<string, SlkUser> instructors = new Dictionary<string, SlkUser>();
        Dictionary<string, SlkUser> learners = new Dictionary<string, SlkUser>();
        Dictionary<string, SlkUser> users = new Dictionary<string, SlkUser>();
        Dictionary<long, SlkUser> usersById = new Dictionary<long, SlkUser>();
        List<SlkGroup> learnerGroups = new List<SlkGroup>();
        /// <summary>
        /// Enumerating domain groups in <c>GetMemberships</c> will time out if
        /// <c>GetMemberships</c> executes longer for this time span.
        /// </summary>
        static readonly TimeSpan DomainGroupEnumerationTotalTimeout = new TimeSpan(0, 5, 0);

#region fields
#endregion fields

#region properties
        /// <summary>Returns all group failures.</summary>
        public ReadOnlyCollection<string> GroupFailures
        {
            get { return new ReadOnlyCollection<string>(groupFailuresList) ;}
        }

        /// <summary>The details of the group failures.</summary>
        public string GroupFailureDetails
        {
            get { return groupFailureDetailsBuilder.ToString() ;}
        }

        /// <summary>
        /// Gets the collection of instructors on the SharePoint <c>SPWeb</c> associated with this
        /// object.  A user is considered an instructor on a given <c>SPWeb</c> if they have the
        /// instructor permission on that <c>SPWeb</c>, i.e. the permission defined by
        /// <c>SlkSPSiteMapping.InstructorPermission</c>.
        /// </summary>
        public ReadOnlyCollection<SlkUser> Instructors
        {
            [DebuggerStepThrough]
            get { return SortedDictionary(instructors); }
        }

        /// <summary>
        /// Gets the collection of learners on the SharePoint <c>SPWeb</c> associated with this object.
        /// A user is considered a learner on a given <c>SPWeb</c> if they have the learner permission
        /// on that <c>SPWeb</c>, i.e. the permission defined by
        /// <c>SlkSPSiteMapping.LearnerPermission</c>.
        /// </summary>
        public ReadOnlyCollection<SlkUser> Learners
        {
            [DebuggerStepThrough]
            get { return SortedDictionary(learners); }
        }

        /// <summary>
        /// Gets the collection of learner groups on the SharePoint <c>SPWeb</c> associated with this
        /// object.  A SharePoint <c>SPGroup</c> or domain group is considered a learner group on a
        /// given <c>SPWeb</c> if it has the learner permission on that <c>SPWeb</c>, i.e. the
        /// permission defined by <c>SlkSPSiteMapping.LearnerPermission</c>.
        /// </summary>
        public ReadOnlyCollection<SlkGroup> LearnerGroups
        {
            [DebuggerStepThrough]
            get { return new ReadOnlyCollection<SlkGroup>(learnerGroups); }
        }

        /// <summary>Returns the SlkUser for the given key.</summary>
        public SlkUser this[long key]
        {
            get 
            { 
                if (usersById.Count == 0)
                {
                    foreach (SlkUser user in users.Values)
                    {
                        usersById.Add(user.UserId.GetKey(), user);
                    }
                }
                return usersById[key] ;
            }
        }
#endregion properties

#region constructors
        /// <summary>Initializes a new instance of <see cref="SlkMemberships"/>.</summary>
        public SlkMemberships() : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of <see cref="SlkMemberships"/>.</summary>
        /// <param name="instructors">The collection of instructors/>. (See the <c>Instructors</c> property.)</param>
        /// <param name="learners">The collection of learners />. (See the <c>Learners</c> property.)</param>
        /// <param name="learnerGroups">The collection of learner groups />. (See the <c>LearnerGroups</c> property.)</param>
        public SlkMemberships(IEnumerable<SlkUser> instructors, IEnumerable<SlkUser> learners, IEnumerable<SlkGroup> learnerGroups)
        {
            if (instructors != null)
            {
                foreach (SlkUser instructor in instructors)
                {
                    this.instructors.Add(instructor.Key, instructor);
                }
            }

            if (learners != null)
            {
                foreach (SlkUser learner in learners)
                {
                    this.learners.Add(learner.Key, learner);
                }
            }

            if (learnerGroups != null)
            {
                foreach (SlkGroup group in learnerGroups)
                {
                    this.learnerGroups.Add(group);
                }
            }
        }
#endregion constructors

#region public methods
        /// <summary>Finds all Slk members of a site.</summary>
        /// <param name="web">The site to get the members for.</param>
        /// <param name="store">The ISlkStore to use.</param>
        /// <param name="instructorsOnly">Whether to only get instructors or not.</param>
        public void FindAllSlkMembers(SPWeb web, ISlkStore store, bool instructorsOnly)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            // keep track of when this method started, for timeout purposes
            startTime = DateTime.Now;

            // Verify that the SPWeb is in the SPSite associated with this SlkStore, because if it
            // isn't then the code below below may be using the wrong SLK instructor and learner
            // permission names; for example, the SPSite of this SlkStore may name the instructor
            // permission "SLK Instructor", but <web> may be in a different SPSite which might name
            // the instructor permission "SLK Teacher"
            if (web.Site.ID != store.SPSiteGuid)
            {
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);
            }

            // Security checks: Fails if the user isn't an instructor and a Reader (implemented by EnsureInstructor)
            // since we use SPSecurity.RunWithElevatedPrivileges, we need to make sure the current user is an instructor
            if (instructorsOnly == false)
            {
                store.EnsureInstructor(web);
            }

            SlkSPSiteMapping mapping = store.Mapping;
            SPRoleDefinition instructorRole = FindRole(web, mapping.InstructorPermission);
            SPRoleDefinition learnerRole = FindRole(web, mapping.LearnerPermission);

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site2 = new SPSite(web.Url))
                {
                    using (SPWeb elevatedWeb = site2.OpenWeb())
                    {
                        IterateWebRoles(elevatedWeb, instructorRole, learnerRole, instructorsOnly, store.Settings.HideDisabledUsers);
                    }
                }
            });

            store.AssignUserItemIdentifier(users.Values);

            // populate the Users property of each item in <learnerGroups>
            foreach (SlkGroup learnerGroup in learnerGroups)
            {
                foreach (string userKey in learnerGroup.UserKeys)
                {
                    learnerGroup.AddUser(users[userKey]);
                }
            }

            learnerGroups.Sort();
        }
#endregion public methods

#region private methods
        static SPRoleDefinition FindRole(SPWeb web, string roleName)
        {
            try
            {
                    return web.RoleDefinitions[roleName];
            }
            catch (SPException)
            {
                // permission not defined
                return null;
            }
        }

        void IterateWebRoles(SPWeb web, SPRoleDefinition instructorRole, SPRoleDefinition learnerRole, bool instructorsOnly, bool hideDisabledUsers)
        {
            web.AllowUnsafeUpdates = true; // enable web.AllUsers.AddCollection()
            foreach (SPRoleAssignment roleAssignment in web.RoleAssignments)
            {
                // determine if this role assignment refers to an instructor or a learner; skip it if it's neither
                bool isInstructor = false;
                if (instructorRole != null)
                {
                    isInstructor = roleAssignment.RoleDefinitionBindings.Contains(instructorRole);
                }

                bool isLearner = false;
                if (instructorsOnly == false && learnerRole != null)
                {
                    isLearner = roleAssignment.RoleDefinitionBindings.Contains(learnerRole);
                }

                if (isInstructor == false && isLearner == false)
                {
                    continue;
                }

                // process the role assignment
                SPPrincipal member = roleAssignment.Member;
                SPUser spUser = member as SPUser;
                SPGroup spGroup = member as SPGroup;
                if (spUser != null)
                {
                    AddSPUserAsMember(web, spUser, isInstructor, isLearner, startTime, null, hideDisabledUsers);
                }
                else if (spGroup != null)
                {
                    // role assignment member is a SharePoint group...
                    SlkGroup learnerGroup = null;
                    if (isLearner)
                    {
                        learnerGroup = new SlkGroup(spGroup, null);
                        learnerGroup.UserKeys = new List<string>();
                    }

                    // add users from the domain group to the collections of instructors, learners, and/or this learner group, as appropriate
                    foreach (SPUser spUserInGroup in spGroup.Users)
                    {
                        AddSPUserAsMember(web, spUserInGroup, isInstructor, isLearner, startTime, learnerGroup, hideDisabledUsers);
                    }

                    if (isLearner)
                    {
                        if (learnerGroup.UserKeys.Count > 0)
                        {
                            learnerGroups.Add(learnerGroup);
                        }
                    }
                }
            }
        }

        void AddSPUserAsMember(SPWeb web, SPUser user, bool isInstructor, bool isLearner, DateTime startTime, SlkGroup learnerGroup, bool hideDisabledUsers)
        {
            if (user.IsDomainGroup)
            {
                // role assignment member is a domain group
                EnumerateDomainGroupMembers(web, user, isInstructor, isLearner, ((learnerGroup != null) ? learnerGroup.UserKeys : null), hideDisabledUsers);
            }
            else
            {
                AddUser(user, isInstructor, isLearner, learnerGroup, null);
            }
        }

        void AddUser(SPUser user, bool isInstructor, bool isLearner, SlkGroup learnerGroup, List<string> learnerKeys)
        {
            SlkUser slkUser = new SlkUser(user);
            SlkUser slkUser2;
            string userKey = slkUser.Key;

            if (users.TryGetValue(userKey, out slkUser2))
            {
                slkUser = slkUser2;
            }
            else
            {
                users[userKey] = slkUser;
            }

            if (isInstructor)
            {
                instructors[userKey] = slkUser;
            }

            if (isLearner)
            {
                learners[userKey] = slkUser;

                if (learnerGroup != null)
                {
                    learnerGroup.UserKeys.Add(userKey);
                }

                if (learnerKeys != null)
                {
                    learnerKeys.Add(userKey);
                }
            }
        }

        /// <summary>Enumerates the members of a domain group. </summary>
        /// <param name="spWeb">An <c>SPWeb</c> within the site collection to which users from the domain group will be added as needed.</param>
        /// <param name="domainGroup">The <c>SPUser</c> representing the domain group to enumerate.</param>
        /// <param name="isInstructor"><c>true</c> if this domain group has the "SLK Instructor" permission.</param>
        /// <param name="isLearner"><c>true</c> if this domain group has the "SLK Learner" permission.</param>
        /// <param name="learnerKeys">The SLK "user key" (e.g. SID or login name) of each learner is
        ///     added to this collection, unless <paramref name="learnerKeys"/> is <c>null</c>.</param>
        /// <param name="hideDisabledUsers">Whether to hide disabled users or not.</param>
        /// <returns><c>true</c> if enumeration succeeds, <c>false</c> if not. </returns>
        bool EnumerateDomainGroupMembers(SPWeb spWeb, SPUser domainGroup, bool isInstructor, bool isLearner, List<string> learnerKeys, bool hideDisabledUsers)
        {
            // if timeout occurred, output a message to <groupFailureDetailsBuilder>
            TimeSpan timeRemaining = DomainGroupEnumerationTotalTimeout - (DateTime.Now - startTime);
            if (timeRemaining <= TimeSpan.Zero)
            {
                groupFailureDetailsBuilder.AppendFormat( AppResources.DomainGroupEnumSkippedDueToTimeout, domainGroup.LoginName);
                groupFailureDetailsBuilder.Append("\r\n\r\n");
                return false;
            }

            // get the users in this group, in the form of a collection of SPUserInfo objects 
            ICollection<SPUserInfo> spUserInfos;
            try
            {
                DomainGroupUtilities enumerateDomainGroups = new DomainGroupUtilities(timeRemaining, hideDisabledUsers);
                spUserInfos = enumerateDomainGroups.EnumerateDomainGroup(domainGroup);

                foreach (string error in enumerateDomainGroups.Errors)
                {
                    groupFailuresList.Add(string.Format(CultureInfo.InvariantCulture, "{0} : {1}", domainGroup.Name, error));
                    groupFailureDetailsBuilder.AppendFormat(AppResources.DomainGroupError, domainGroup.LoginName, error);
                    groupFailureDetailsBuilder.Append("\r\n\r\n");
                }
            }
            catch (DomainGroupEnumerationException ex)
            {
                groupFailuresList.Add(string.Format(CultureInfo.InvariantCulture, "{0} : {1}", domainGroup.Name, ex.Message));
                groupFailureDetailsBuilder.AppendFormat(AppResources.DomainGroupError, domainGroup.LoginName, ex);
                groupFailureDetailsBuilder.Append("\r\n\r\n");
                return false;
            }

            // convert <spUserInfos> to <spUsers> (a collection of SPUser); add users to the site collection as needed
            List<SPUserInfo> usersToAddToSPSite = null;
            List<SPUser> spUsers = new List<SPUser>(spUserInfos.Count);
            foreach (SPUserInfo spUserInfo in spUserInfos)
            {
                try
                {
                    SPUser spUserInGroup = spWeb.AllUsers[spUserInfo.LoginName];
                    spUsers.Add(spUserInGroup);
                }
                catch (SPException)
                {
                    // need to add user to site collection (below)
                    if (usersToAddToSPSite == null)
                    {
                        usersToAddToSPSite = new List<SPUserInfo>();
                    }
                    usersToAddToSPSite.Add(spUserInfo);
                }
            }

            if (usersToAddToSPSite != null)
            {
                try
                {
                    spWeb.SiteUsers.AddCollection(usersToAddToSPSite.ToArray());
                    foreach (SPUserInfo spUserInfo in usersToAddToSPSite)
                    {
                        SPUser spUserInGroup = spWeb.AllUsers[spUserInfo.LoginName];
                        spUsers.Add(spUserInGroup);
                    }
                }
                catch (SPException ex)
                {
                    groupFailuresList.Add(domainGroup.Name);
                    groupFailureDetailsBuilder.AppendFormat(AppResources.ErrorCreatingSPSiteUser, spWeb.Site.Url, ex);
                    groupFailureDetailsBuilder.Append("\r\n\r\n");
                    return false;
                }
            }

            // create a SlkGroup if this role assignment has the "SLK Learner" permission, unless <learnerGroups> is null
            SlkGroup learnerGroup = null;
            if (isLearner && (learnerGroups != null))
            {
                learnerGroup = new SlkGroup(null, domainGroup);
                learnerGroup.UserKeys = new List<string>(spUserInfos.Count);
                learnerGroups.Add(learnerGroup);
            }

            // add users from the domain group to the collections of instructors, learners, and/or this
                    // learner group, as appropriate
            foreach (SPUser spUserInGroup in spUsers)
            {
                AddUser(spUserInGroup, isInstructor, isLearner, learnerGroup, learnerKeys);
            }

            return true;
        }

        ReadOnlyCollection<SlkUser> SortedDictionary(Dictionary<string, SlkUser> users)
        {
            List<SlkUser> list = new List<SlkUser>(users.Values);
            list.Sort();
            return new ReadOnlyCollection<SlkUser>(list);
        }

#endregion private methods
    }
}
