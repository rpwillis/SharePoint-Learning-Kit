using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
#region fields
        /// <summary>Holds the value of the <c>Instructors</c> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkUser[] m_instructors;

        /// <summary>Holds the value of the <c>Learners</c> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkUser[] m_learners;

        /// <summary>Holds the value of the <c>LearnerGroups</c> property. </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkGroup[] m_learnerGroups;
#endregion fields

#region properties
        /// <summary>
        /// Gets the collection of instructors on the SharePoint <c>SPWeb</c> associated with this
        /// object.  A user is considered an instructor on a given <c>SPWeb</c> if they have the
        /// instructor permission on that <c>SPWeb</c>, i.e. the permission defined by
        /// <c>SlkSPSiteMapping.InstructorPermission</c>.
        /// </summary>
        public ReadOnlyCollection<SlkUser> Instructors
        {
            [DebuggerStepThrough]
            get
            {
                return new ReadOnlyCollection<SlkUser>(m_instructors);
            }
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
            get
            {
                return new ReadOnlyCollection<SlkUser>(m_learners);
            }
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
            get
            {           
                return new ReadOnlyCollection<SlkGroup>(m_learnerGroups);
            }
        }
#endregion properties

#region constructors
        /// <summary>Initializes an instance of this class. </summary>
        /// <param name="instructors">The collection of instructors/>. (See the <c>Instructors</c> property.)</param>
        /// <param name="learners">The collection of learners />. (See the <c>Learners</c> property.)</param>
        /// <param name="learnerGroups">The collection of learner groups />. (See the <c>LearnerGroups</c> property.)</param>
        public SlkMemberships(SlkUser[] instructors, SlkUser[] learners, SlkGroup[] learnerGroups)
        {
            if (instructors != null)
            {
                m_instructors = new SlkUser[instructors.Length];
                instructors.CopyTo(m_instructors, 0);
            }
            else
            {
                m_instructors = new SlkUser[0];
            }

            if (learners != null)
            {
                m_learners = new SlkUser[learners.Length];
                learners.CopyTo(m_learners, 0);
            }
            else
            {
                m_learners = new SlkUser[0];
            }

            if (learnerGroups != null)
            {
                m_learnerGroups = new SlkGroup[learnerGroups.Length];
                learnerGroups.CopyTo(m_learnerGroups, 0);
            }
            else
            {
                m_learnerGroups = new SlkGroup[0];
            }        
        }
#endregion constructors
    }
}
