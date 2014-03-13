using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;

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

        /// <summary>Gets information about the file system cache used by <c>PackageStore</c>. </summary>
        SharePointCacheSettings SharePointCacheSettings { get; }


        /// <summary>Gets a reference to the <c>SharePointPackageStore</c> associated with this SharePoint
        /// Learning Kit store.  <c>SharePointPackageStore</c> holds references to e-learning packages stored in SharePoint document libraries.</summary>
        PackageStore PackageStore { get; }

        /// <summary>Gets the <c>SlkSPSiteMapping</c> of the <c>SPSite</c> associated with this <c>AnonymousSlkStore</c>.</summary>
        SlkSPSiteMapping Mapping { get; }

        /// <summary>Gets the <c>SlkSettings</c> of the <c>SPSite</c> associated with this <c>SlkStore</c>.</summary>
        SlkSettings Settings { get; }

        /// <summary>
        /// Gets the user key used by LearningStore to identify the current user.  What's contained in
        /// this string depends on the membership provider used by SharePoint (for example, Windows
        /// authentication or forms-based authentication).
        /// </summary>
        string CurrentUserKey { get; }

        /// <summary>Throws an exception if the current user isn't an instructor on an SPWeb</summary>
        /// <param name="spWeb">SPWeb that should be checked.</param>
        /// <exception cref="InvalidOperationException"><pr>spWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated with. </exception>
        /// <exception cref="NotAnInstructorException">The user is not an instructor. </exception>
        /// <exception cref="UnauthorizedAccessException">The user is not a Reader on <pr>spWeb</pr>. </exception>
        void EnsureInstructor(SPWeb spWeb);

        /// <summary>The SLK ID of the current user.</summary>
        UserItemIdentifier CurrentUserId  { get; }

        /// <summary>Finds the root activity of a package.</summary>
        /// <param name="packageId">The id of the package.</param>
        /// <param name="organizationIndex">The index of the organization.</param>
        /// <returns>The root identifier.</returns>
        ActivityPackageItemIdentifier FindRootActivity(PackageItemIdentifier packageId, int organizationIndex);

        /// <summary>Creates a new SharePoint Learning Kit assignment. </summary>
        /// <param name="properties">Properties of the new assignment.  Note that, within
        ///     <c>AssignmentProperties.Instructors</c> and <c>AssignmentProperties.Learners</c>, all
        ///     properties except <c>UserId</c> are ignored.  Also, if slkRole is
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
        AssignmentProperties LoadAssignmentProperties(AssignmentItemIdentifier assignmentId, SlkRole slkRole);

        /// <summary>Deletes a SharePoint Learning Kit assignment. </summary>
        /// <param name="assignmentId">The <c>AssignmentItemIdentifier</c> of the assignment to delete.</param>
        /// <remarks>
        /// <b>Security:</b>&#160; This operation fails if the
        /// <a href="SlkApi.htm#AccessingSlkStore">current user</a> isn't either an instructor on the
        /// assignment or the person who created the assignment.
        /// </remarks>
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.  Possible cause: the user
        /// isn't an instructor on the assignment, and the assignment isn't self-assigned.
        /// </exception>
        void DeleteAssignment(AssignmentItemIdentifier assignmentId);

        /// <summary>
        /// Registers a specified version of a given e-learning package (that's stored in a SharePoint
        /// document library) in the <c>SharePointPackageStore</c> associated with this
        /// SharePoint Learning Kit store, and returns its <c>PackageItemIdentifier</c> and content
        /// warnings.  If that version -- with the same last-modified date/time -- is already
        /// registered, its information is returned rather than re-registering the package.
        /// Uses an <c>SPFile</c> and version ID to locate the package.
        /// </summary>
        /// <param name="reader">A reader for the package.</param>
        /// <remarks>
        /// If the package is valid, <pr>warnings</pr> is set to <n>null</n>.  If the package is not
        /// completely valid, but is valid enough to be assigned within SharePoint Learning Kit,
        /// <pr>warning</pr> is set to warnings about the package.  If the package has problems
        /// severe enough to prevent it from being assignable within SLK, a
        /// <r>SafeToDisplayException</r> is thrown.
        /// </remarks>
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.  Possible cause: the package has
            /// problems severe enough to prevent it from being assignable within SharePoint Learning Kit.
        /// <r>SafeToDisplayException.ValidationResults</r> may contain further information.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The user is not authorized to access the package.
        /// </exception>
        PackageDetails RegisterAndValidatePackage(PackageReader reader);

        /// <summary>Returns the SharePoint Learning Kit user web list for the current user. </summary>
        /// <remarks>
        /// <para>The user web list is the list of Web sites that the user added to their E-Learning Actions page.</para>
        /// <para><b>Security:</b>&#160; None.  The <a href="SlkApi.htm#AccessingSlkStore">current user</a>
        /// may be any user.</para>
        /// </remarks>
        ReadOnlyCollection<SlkUserWebListItem> FetchUserWebList();

        /// <summary>Loads an assignment for grading.</summary>
        /// <param name="id">The id of the assignment.</param>
        /// <returns>An <see cref="AssignmentProperties"/>.</returns>
        AssignmentProperties GetGradingProperties(AssignmentItemIdentifier id);

        /// <summary>Changes the <c>LearnerAssignmentState</c> of a learner assignment.</summary>
        /// <param name="learnerAssignmentId">The ID of the learner assignment to change the
        ///     <c>LearnerAssignmentState</c> of.</param>
        /// <param name="isFinal">Whether the assignment is final or not. Null is do not set.</param>
        /// <param name="nonELearningStatus">The Non ELearning Status to set. Null is do not set.</param>
        /// <param name="saveFinalPoints">Whether to save final points or not.</param>
        /// <param name="finalPoints">The final points.</param>
        /// <remarks>
        /// <para>
        /// Only the following state transitions are supported. Enforced by LearnerAssignmentProperties.
        /// </para>
        /// <list type="bullet">
        ///     <item><description><c>Active</c> to <c>Completed</c>.  This "submits" (learner gives
        ///         assignment back to instructor) or "collects" (instructor takes assignment back
        ///         from learner) or "marks as complete" (learner record a learner-created assignment
        ///         as complete) a learner assignment.  This transition may be performed by the
        ///         instructor of the assignment or the learner who owns this learner assignment.
        ///         Note that if <c>AssignmentProperties.AutoReturn</c> is <c>true</c> for this
        ///         assignment, this state transition will automatically cause a second transition,
        ///         from <c>Completed</c> to <c>Final</c>
        ///         </description>
        ///     </item>
        ///     <item><description><c>Completed</c> to <c>Final</c>.  This "returns" the assignment
        ///            from the instructor to the learner -- in this case the user must be an instructor
        ///         on the assignment.  This state transition may also be performed in the case where
        ///         the instructor caused <c>AssignmentProperties.AutoReturn</c> to be set to
        ///         <c>true</c> <u>after</u> this assignment transitioned from <c>Active</c> to
        ///         <c>Completed</c> state -- in this case ("auto-return") the user may be either an
        ///         instructor of the assignment or the learner who owns this learner assignment.
        ///         </description>
        ///     </item>
        ///     <item><description><c>Completed</c> or <c>Final</c> to <c>Active</c>.  This
        ///         "reactivates" the assignment, so that the learner may once again work on it.
        ///         </description>
        ///     </item>
        ///     <item><description><c>Active</c> to <c>Final</c>, equivalent to <c>Active</c> to
        ///         <c>Completed</c> followed by <c>Completed</c> to <c>Final</c>.</description>
        ///     </item>
        ///     <item><description><c>NotStarted</c> to <c>Active</c>, <c>Completed</c>, or
        ///         <c>Final</c>, equivalent to beginning the learner assignment and then transitioning
        ///         states as described above.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <remarks>
        /// <b>Security:</b>&#160; Fails if the
        /// <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't have the right to switch to
        /// the requested state.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The requested state transition is not supported.</exception>
        /// <exception cref="SafeToDisplayException">An error occurred that can be displayed to a browser user.  Possible cause:
        /// the user doesn't have the right to switch to the requested state.</exception>
        void ChangeLearnerAssignmentState(LearnerAssignmentItemIdentifier learnerAssignmentId, bool? isFinal, 
                AttemptStatus? nonELearningStatus, bool saveFinalPoints, float? finalPoints);

        /// <summary>Loads the instructors for an assignment.</summary>
        /// <param name="id">The id of the assignment.</param>
        /// <param name="instructors">The collection to add the instructors to.</param>
        void LoadInstructors(AssignmentItemIdentifier id, SlkUserCollection instructors);

        /// <summary>Saves changes to the learner assignment.</summary>
        /// <param name="learnerAssignmentId">The id of the LearnerAssignment.</param>
        /// <param name="ignoreFinalPoints">Whether to ignore the final points.</param>
        /// <param name="finalPoints">The final points to save.</param>
        /// <param name="instructorComments">The instructor comments.</param>
        /// <param name="grade">The grade.</param>
        /// <param name="isFinal">Whether the assignment is final or not. Null is do not set.</param>
        /// <param name="nonELearningStatus">The Non ELearning Status to set. Null is do not set.</param>
        /// <param name="currentJob">The current transaction.</param>
        void SaveLearnerAssignment(LearnerAssignmentItemIdentifier learnerAssignmentId, bool ignoreFinalPoints, float? finalPoints, string instructorComments, 
                            string grade, bool? isFinal, AttemptStatus? nonELearningStatus, ICurrentJob currentJob);

        /// <summary>Saves the learner comment.</summary>
        /// <param name="learnerAssignmentId">The id of the LearnerAssignment.</param>
        /// <param name="comment">The comment to save.</param>
        void SaveLearnerComment(LearnerAssignmentItemIdentifier learnerAssignmentId, string comment);

        /// <summary>Loads all possible assignment reminders.</summary>
        /// <param name="minDueDate">The minimum due date to return.</param>
        /// <param name="maxDueDate">The maximum due date to return.</param>
        IEnumerable<AssignmentProperties> LoadAssignmentReminders(DateTime minDueDate, DateTime maxDueDate);

        /// <summary>Loads a self assignment for a particular location if one exists.</summary>
        /// <param name="location">The location of the assignment.</param>
        /// <returns>An <see cref="AssignmentProperties"/>.</returns>
        AssignmentProperties LoadSelfAssignmentForLocation(SharePointFileLocation location);

        /// <summary>Loads the package details from the store.</summary>
        /// <param name="location">The location of the package.</param>
        /// <returns>The details of the package.</returns>
        PackageDetails LoadPackageFromStore(SharePointFileLocation location);

        /// <summary>Creates a package reader for a package without accessing the store.</summary>
        /// <param name="file">The package.</param>
        /// <param name="location">The package location.</param>
        /// <returns></returns>
        PackageReader CreatePackageReader(SPFile file, SharePointFileLocation location);

        /// <summary>Logs an exception.</summary>
        /// <param name="exception"></param>
        void LogException(Exception exception);

        /// <summary>Logs an exception.</summary>
        void LogError(string format, params object[] arguments);

        /// <summary>Starts a transaction.</summary>
        ICurrentJob CreateCurrentJob();

        /// <summary>Versions a library if not already versioned.</summary>
        /// <param name="list">The library to version.</param>
        void VersionLibrary(SPDocumentLibrary list);
    }
}

