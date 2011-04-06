using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.SharePoint;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The SLK store.</summary>
    public interface ISlkStore
    {
        /// <summary>
        /// Adds a given <c>SPWeb</c> to the current user's SharePoint Learning Kit user web list.
        /// If the <c>SPWeb</c> already exists in the user web list, it's last-access time is updated
        /// to be the current time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The user web list is the list of Web sites that the user added to their E-Learning Actions
        /// page.
        /// </para>
        /// <para>
        /// <b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
        /// may be any user.
        /// </para>
        /// </remarks>
        /// <param name="spWeb">The <c>SPWeb</c> to add.</param>
        void AddToUserWebList(SPWeb spWeb);

        /// <summary>Gets the <c>Guid</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.</summary>
        Guid SPSiteGuid { get; }

        /// <summary>Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this <c>AnonymousSlkStore</c>.</summary>
        SlkSPSiteMapping Mapping { get; }

        /// <summary>Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.</summary>
        SlkSettings Settings { get; }

        /// <summary>Throws an exception if the current user isn't an instructor on an SPWeb</summary>
        /// <param name="spWeb">SPWeb that should be checked.</param>
        /// <exception cref="InvalidOperationException"><pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with. </exception>
        /// <exception cref="NotAnInstructorException">The user is not an instructor. </exception>
        /// <exception cref="UnauthorizedAccessException">The user is not a Reader on <pr>spWeb</pr>. </exception>
        void EnsureInstructor(SPWeb spWeb);

        /// <summary>The SLK ID of the current user.</summary>
        UserItemIdentifier CurrentUserId  { get; }

        /// <summary>Finds the root activity of a package.</summary>
        /// <param name="location">The location of the package in MLC format.</param>
        /// <param name="organizationIndex">The index of the chosen organization.</param>
        /// <returns>The root identifier.</returns>
        ActivityPackageItemIdentifier FindRootActivity(string location, int organizationIndex);

        /// <summary>Creates a new SharePoint Learning Kit assignment. </summary>
        /// <param name="properties">Properties of the new assignment.  Note that, within
        ///     <c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>, all
        ///     properties except <c>UserId</c> are ignored.  Also, if <paramref name="slkRole"/> is
        ///     <c>SlkRole.Learner</c>, then <c>AssignmentProperties.Instructors</c> must be
        ///     empty, and <c>AssignmentProperties.Learners</c> must contain only the current user.
        ///  </param>
        AssignmentItemIdentifier CreateAssignment(AssignmentProperties properties);

        /// <summary>Assigns the <see cref="UserItemIdentifier"/> to each SlkUser.</summary>
        /// <param name="users">The users to have their identifiers assigned.</param>
        void AssignUserItemIdentifier(IEnumerable<SlkUser> users);

        /// <summary>Updates an <see cref="AssignmentProperties"/>.</summary>
        /// <param name="assignment">The assignment to update.</param>
        /// <param name="corePropertiesChanged">Whether any of the core properties have changed.</param>
        /// <param name="instructorChanges">The changes in the instructors.</param>
        /// <param name="learnerChanges">The changes in the learners.</param>
        void UpdateAssignment(AssignmentProperties assignment, bool corePropertiesChanged, SlkUserCollectionChanges instructorChanges, SlkUserCollectionChanges learnerChanges);

        /// <summary>Retrieves general information about an assignment. </summary>
        /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to retrieve information about.</param>
        /// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.</param>
        /// <returns>
        /// An <c>AssignmentProperties</c> object containing information about the assignment.
        /// Note that only the <c>UserId</c> of each <c>SlkUser</c> object within the returned
        /// <c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>
        /// collections is valid.
        /// </returns>
        /// <remarks>
        /// <b>Security:</b>&#160; If <pr>slkRole</pr> is
        /// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Instructor</a>,
        /// this operation fails if the <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't
        /// an instructor on the assignment.  If <pr>slkRole</pr> is
        /// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Learner</a>,
        /// this operation fails if the current user isn't a learner on the assignment.
        /// </remarks>
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.  Possible cause: the user
        /// isn't an instructor on the assignment (if <pr>slkRole</pr> is <r>SlkRole.Instructor</r>),
        /// or the user isn't a learner on the assignment (if <pr>slkRole</pr> is
        /// <r>SlkRole.Learner</r>).
        /// </exception>
        AssignmentProperties GetAssignmentProperties(AssignmentItemIdentifier assignmentId, SlkRole slkRole);
    }
}
