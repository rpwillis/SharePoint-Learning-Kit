using System;
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
    }
}
