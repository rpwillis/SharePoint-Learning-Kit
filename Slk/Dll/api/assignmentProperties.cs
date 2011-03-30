using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Contains information about a SharePoint Learning Kit assignment, i.e. an e-learning package
    /// (such as a SCORM package) or a non-e-learning document (such as a Microsoft Word document)
    /// assigned to zero or more learners.
    /// </summary>
    ///
    public class AssignmentProperties
    {
        ISlkStore store;

#region properties
        /// <summary>Gets the <c>Guid</c> of the SPSite that contains the SPWeb that this assignment is associated with.</summary>
        public Guid SPSiteGuid { get; set; }

        /// <summary>The Id of the assignment.</summary>
        public AssignmentItemIdentifier Id { get; private set; }

        /// <summary>
        /// Gets the <c>Guid</c> of the SPWeb that this assignment is associated with.
        /// </summary>
        public Guid SPWebGuid { get; set; }

        /// <summary>Gets or sets the title of the assignment. </summary>
        public string Title { get; set; }

        /// <summary>Gets or sets the description of the assignment. </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the nominal maximum number of points possible for the assignment.
        /// <c>null</c> if points possible is not specified.
        /// </summary>
        public Nullable<Single> PointsPossible { get; set; }

        /// <summary>
        /// Gets or sets the date/time that this assignment starts.  Unlike the related value stored in
        /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the due date/time of this assignment.  <c>null</c> if there is no due date.
        /// Unlike the related value stored in the SharePoint Learning Kit database, this value is a
        /// local date/time, not a UTC value.
        /// </summary>
        public Nullable<DateTime> DueDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each learner assignment associated with this
        /// assignment should automatically be returned to the learner when the learner marks it as
        /// complete (after auto-grading), rather than requiring an instructor to manually return the
        /// assignment to the student.
        /// </summary>
        public bool AutoReturn { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether answers will be shown to the learner when a
        /// learner assignment associated with this assignment is returned to the learner.  This only
        /// applies to certain types of e-learning content.
        /// </summary>
        public bool ShowAnswersToLearners { get; set; }

        /// <summary>Gets or sets the <c>UserItemIdentifier</c> of the user who created this assignment. </summary>
        public UserItemIdentifier CreatedById { get; set; }

        /// <summary>
        /// Gets the date/time that this assignment was created.  Unlike the related value stored in
        /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the collection of instructors of this assignment.
        /// </summary>
        public SlkUserCollection Instructors { get; private set; }

        /// <summary>
        /// Gets or sets the collection of learners assigned this assignment.
        /// </summary>
        public SlkUserCollection Learners { get; private set; }

        /// <summary>Indicates if the assignment is e-learning or not.</summary>
        public bool IsELearning
        {
            get { return (PackageFormat != null) ;}
        }

        /// <summary>Indicates if the assignment is e-learning or not.</summary>
        public bool IsNonELearning
        {
            get { return (PackageFormat == null) ;}
        }

        /// <summary>The root activity for SCORM packages.</summary>
        public ActivityPackageItemIdentifier RootActivityId { get; set; }

        /// <summary>Indicates if the assignment is Class Server content or not.</summary>
        public bool IsClassServerContent
        {
            get { return (PackageFormat.HasValue && PackageFormat.Value == Microsoft.LearningComponents.PackageFormat.Lrm) ;}
        }

        /// <summary>
        /// Returns the general file format of the e-learning package associated with this assignment,
        /// or <c>null</c> if a non-e-learning document is associated with this assignment.
        /// </summary>
        public Nullable<PackageFormat> PackageFormat { get; set; }

        /// <summary>
        /// Gets the MLC SharePoint location string of the e-learning package or non-e-learning
        /// document associated with the assignment.
        /// </summary>
        public string Location { get; set; }

        /// <summary>Indicates whether to email assignment changes.</summary>
        public bool EmailChanges { get; set; }
#endregion properties

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(AssignmentItemIdentifier id, ISlkStore store) : this(store)
        {
            this.Id = id;
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(long id, ISlkStore store) : this(store)
        {
            if (id != 0)
            {
                this.Id = new AssignmentItemIdentifier(id);
            }
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        AssignmentProperties(ISlkStore store)
        {
            this.store = store;
            Instructors = new SlkUserCollection();
            Learners = new SlkUserCollection();
        }
#endregion constructors

#region public methods
        /// <summary>Saves the assignment.</summary>
        ///
        /// <param name="web">The <c>SPWeb</c> to create the assignment in.</param>
        ///
        /// <param name="slkRole">The <c>SlkRole</c> to use when creating the assignment.  Use
        ///     <c>SlkRole.Learner</c> to create a self-assigned assignment, i.e. an assignment with
        ///     no instructors for which the current learner is the only learner.  Otherwise, use
        ///     <c>SlkRole.Instructor</c>.</param>
        ///
        /// <remarks>
        /// <para>
        /// When creating a self-assigned assignment, take care to ensure that
        /// <r>AssignmentProperties.AutoReturn</r> is <n>true</n>, otherwise the learner assignments
        /// will never reach
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a>
        /// state.
        /// </para>
        /// <para>
        /// <b>Security:</b>&#160; If <pr>slkRole</pr> is
        /// <a href="Microsoft.SharePointLearningKit.SlkRole.Enumeration.htm">SlkRole.Instructor</a>,
        /// this operation fails if the <a href="SlkApi.htm#AccessingSlkStore">current user</a> doesn't
        /// have SLK
        /// <a href="Microsoft.SharePointLearningKit.SlkSPSiteMapping.InstructorPermission.Property.htm">instructor</a>
        /// permissions on <pr>destinationSPWeb</pr>.  Also fails if the current user doesn't have access to the
        /// package/file.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <pr>destinationSPWeb</pr> is not in the SPSite that this <r>SlkStore</r> is associated
        /// with.
        /// </exception>
        ///
        /// <exception cref="SafeToDisplayException">
        /// An error occurred that can be displayed to a browser user.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// <pr>location</pr> refers to a file that the user does not have access to, the user is not
        /// a Reader on <pr>destinationSPWeb</pr>, or the user is trying to create an assignment
        /// with other learners or instructors but the assignment is self-assigned.
        /// </exception>
        /// <exception cref="NotAnInstructorException">
        /// The user is not an instructor on <pr>destinationSPWeb</pr> and the assignment is not self-assigned.
        /// </exception>
        public void Save(SPWeb web, SlkRole slkRole)
        {
            if (Id == null)
            {
                SaveNewAssignment(web, slkRole);
            }
            else
            {
            }
        }

        /// <summary>Populates the SPUser property of each SlkUser in the collection of SLK members.</summary>
        /// <param name="slkMembers">The collection of SLK members to populate.</param>
        public void PopulateSPUsers(SlkMemberships slkMembers)
        {
            PopulateSPUsers(slkMembers.Learners, Learners);
            PopulateSPUsers(slkMembers.Instructors, Instructors);
        }

        /// <summary>Sets the location of the package.</summary>
        /// <remarks>Security checks:Fails if the user doesn't have access to the package/file</remarks>
        /// <param name="location">The MLC SharePoint location string that refers to the e-learning
        ///     package or non-e-learning document to assign.  Use
        ///     <c>SharePointPackageStore.GetLocation</c> to construct this string.</param>
        /// <param name="organizationIndex">The zero-based index of the organization within the
        ///     e-learning content to assign; this is the value that's used as an index to
        ///     <c>ManifestReader.Organizations</c>.  If the content being assigned is a non-e-learning
        ///     document, use <c>null</c> for <paramref name="organizationIndex"/>.</param>
        public void SetLocation(string location, Nullable<int> organizationIndex)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            // set <rootActivityId> to the ID of the organization, or null if a non-e-learning document is being assigned
            if (organizationIndex != null)
            {
                RootActivityId = store.FindRootActivity(location, organizationIndex.Value);
                // Set PackageFormat so IsNonELearning is set correctly
                PackageFormat = Microsoft.LearningComponents.PackageFormat.V1p3;
            }
            else  // Non-elearning content
            {
                // Access the properties of the file to verify that the user has access to the file
                SPFile file = SlkUtilities.GetSPFileFromPackageLocation(location);
                System.Collections.Hashtable fileProperties = file.Properties;
                RootActivityId = null;
                Location = location;
            }
        }
#endregion public methods

            // Security checks: If assigning as an instructor, fails if user isn't an instructor on
            // the web (implemented by calling EnsureInstructor).  Fails if the user doesn't have access to the package/file
#region private methods
        void SaveNewAssignment(SPWeb web, SlkRole slkRole)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            // Verify that the web is in the site
            if (web.Site.ID != store.SPSiteGuid)
            {
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);
            }
            else
            {
                SPSiteGuid = web.Site.ID;
                SPWebGuid = web.ID;
            }

            VerifyRole(web, slkRole);
            Id = store.CreateAssignment(this);

            //Update the MRU list
            store.AddToUserWebList(web);

            if (IsNonELearning)
            {
                PopulateSPUsers(null);
                //PopulateSPUsers(SlkMembers);
                DropBoxManager dropBoxMgr = new DropBoxManager(this);
                Microsoft.SharePoint.Utilities.SPUtility.ValidateFormDigest();
                dropBoxMgr.CreateAssignmentFolder();
            }
        }

        void VerifyRole(SPWeb web, SlkRole slkRole)
        {
            if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            {
                throw new ArgumentOutOfRangeException("slkRole");
            }

            bool isSelfAssigned = (slkRole == SlkRole.Learner) ? true : false;

            if (isSelfAssigned)
            {
                VerifyIsValidSelfAssigned();
            }
            else
            {
                store.EnsureInstructor(web);
            }
        }

        void PopulateSPUsers(ReadOnlyCollection<SlkUser> members, SlkUserCollection users)
        {
            if (members.Count > 0)
            {
                foreach (SlkUser user in users)
                {
                    if (user.SPUser == null)
                    {
                        foreach (SlkUser member in members)
                        {
                            if (member.UserId.GetKey() == user.UserId.GetKey())
                            {
                                user.SPUser = member.SPUser;
                                break;
                            }
                        }
                    }
                }
            }
        }

        void VerifyIsValidSelfAssigned()
        {
            // set <currentUserId> to the UserItemIdentifier of the current user; note that this
            // requires a round trip to the database
            UserItemIdentifier currentUserId = store.CurrentUserId;

            // verify that <properties> specify no instructors and that the current user is the
            // only learner
            if (Instructors.Count != 0)
            {
                throw new UnauthorizedAccessException(AppResources.AccessDenied);
            }

            if ((Learners.Count != 1) || (Learners[0].UserId != currentUserId))
            {
                throw new UnauthorizedAccessException(AppResources.AccessDenied);
            }
        }
#endregion private methods
    }
}

