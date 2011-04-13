using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.Manifest;
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
        internal const string noPackageLocation = "{00000}";
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

        /// <summary>Indicates if the assignment is a no package assignment.</summary>
        public bool IsNoPackageAssignment
        {
            get { return Location == noPackageLocation ;}
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

        /// <summary>Any packabe warnings.</summary>
        public LearningStoreXml PackageWarnings { get; private set; }
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
        /// <param name="web">The <c>SPWeb</c> to create the assignment in.</param>
        /// <param name="slkRole">The <c>SlkRole</c> to use when creating the assignment.  Use
        ///     <c>SlkRole.Learner</c> to create a self-assigned assignment, i.e. an assignment with
        ///     no instructors for which the current learner is the only learner.  Otherwise, use
        ///     <c>SlkRole.Instructor</c>.</param>
        /// <param name="slkMembers">The Slk membership of the web. Needs refactoring to remove.</param>
        public void Save(SPWeb web, SlkRole slkRole, SlkMemberships slkMembers)
        {
            // Verify that the web is in the site
            if (web.Site.ID != store.SPSiteGuid)
            {
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);
            }

            if (Id == null)
            {
                SaveNewAssignment(web, slkRole, slkMembers);
            }
            else
            {
                UpdateAssignment(slkMembers);
            }
        }

        /// <summary>Deletes the assignment.</summary>
        public void Delete()
        {
        }

        /// <summary>Populates the SPUser property of each SlkUser in the collection of SLK members.</summary>
        /// <param name="slkMembers">The collection of SLK members to populate.</param>
        public void PopulateSPUsers(SlkMemberships slkMembers)
        {
            PopulateSPUsers(slkMembers, Learners);
            PopulateSPUsers(slkMembers, Instructors);
        }

        /// <summary>Makes the assignment be a no package assignemnt.</summary>
        public void MakeNoPackageAssignment(string title)
        {
            Location = noPackageLocation;
            if (String.IsNullOrEmpty(Title))
            {
                if (string.IsNullOrEmpty(title))
                {
                    Title = AppResources.NoPackageTitle;
                }
                else
                {
                    Title = title;
                }
            }

            if (Description == null)
            {
                Description = String.Empty;
            }
            return;
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
            SetLocation(location, organizationIndex, null);
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
        /// <param name="title">Any title that has been passed in.</param>
        public void SetLocation(string location, Nullable<int> organizationIndex, string title)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            if (location == noPackageLocation)
            {
                MakeNoPackageAssignment(title);
                return;
            }

            // Access the properties of the file to verify that the user has access to the file
            SPFile file = SlkUtilities.GetSPFileFromPackageLocation(location);
            System.Collections.Hashtable fileProperties = file.Properties;

            // set <rootActivityId> to the ID of the organization, or null if a non-e-learning document is being assigned
            if (organizationIndex != null)
            {
                // register the package with PackageStore (if it's not registered yet), and get the PackageItemIdentifier
                PackageDetails package = store.RegisterAndValidatePackage(location);

                RootActivityId = store.FindRootActivity(package.PackageId, organizationIndex.Value);
                PackageInformation information = store.GetPackageInformation(package.PackageId, file);

                bool existingTitle = (string.IsNullOrEmpty(Title) == false);

                if (existingTitle == false)
                {
                    Title = information.Title;
                }

                if (string.IsNullOrEmpty(Description))
                {
                    Description = information.Description;
                }

                // validate <organizationIndex>
                if ((organizationIndex.Value < 0) || (organizationIndex.Value >= information.ManifestReader.Organizations.Count))
                {
                    throw new SafeToDisplayException(AppResources.InvalidOrganizationIndex);
                }

                PackageFormat = information.ManifestReader.PackageFormat;

                // set <organizationNodeReader> to refer to the organization
                OrganizationNodeReader organizationNodeReader = information.ManifestReader.Organizations[organizationIndex.Value];

                // if there is more than one organization, append the organization title, if any
                if (existingTitle == false && information.ManifestReader.Organizations.Count > 1)
                {
                    if (!String.IsNullOrEmpty(organizationNodeReader.Title))
                    {
                        Title = String.Format(CultureInfo.CurrentCulture, AppResources.SlkPackageAndOrganizationTitle, Title, organizationNodeReader.Title);
                    }
                }

                // set <pointsPossible> to the points-possible value stored in the manifest, or null if none
                switch (information.ManifestReader.PackageFormat)
                {
                    case Microsoft.LearningComponents.PackageFormat.Lrm:
                        PointsPossible = organizationNodeReader.PointsPossible;
                        break;
                    case Microsoft.LearningComponents.PackageFormat.V1p3:
                        PointsPossible = 100;
                        break;
                    default:
                        PointsPossible = null;
                        break;
                }
            }
            else  // Non-elearning content
            {
                RootActivityId = null;
                Location = location;
                if (string.IsNullOrEmpty(Title))
                {
                    Title = file.Title;
                    if (String.IsNullOrEmpty(Title))
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    }
                }

                if (string.IsNullOrEmpty(Description))
                {
                    Description = string.Empty;
                }

                if (PointsPossible == null)
                {
                    PointsPossible = null;      // "Points Possible" defaults to null for non-e-learning content
                    PackageWarnings = null;     // no package warnings
                    PackageFormat = null;       // non-e-learning package
                }
            }
        }
#endregion public methods

#region private methods
        void CopyInvariantProperties(AssignmentProperties properties)
        {
            SPSiteGuid = properties.SPSiteGuid;
            SPWebGuid = properties.SPWebGuid;
            CreatedById = properties.CreatedById;
            DateCreated = properties.DateCreated;
            RootActivityId = properties.RootActivityId;
            PackageFormat = properties.PackageFormat;
            Location = properties.Location;
        }

        void UpdateAssignment(SlkMemberships slkMembers)
        {
            AssignmentProperties oldProperties = store.GetAssignmentProperties(Id, SlkRole.Instructor);
            CopyInvariantProperties(oldProperties);

            bool corePropertiesChanged = false;

            if (Title != oldProperties.Title || StartDate != oldProperties.StartDate || DueDate != oldProperties.DueDate || PointsPossible != oldProperties.PointsPossible
                    || Description != oldProperties.Description || AutoReturn != oldProperties.AutoReturn || ShowAnswersToLearners != oldProperties.ShowAnswersToLearners 
                    || EmailChanges != oldProperties.EmailChanges)
            {
                corePropertiesChanged = true;
            }

            SlkUserCollectionChanges instructorChanges = new SlkUserCollectionChanges(oldProperties.Instructors, Instructors);
            SlkUserCollectionChanges learnerChanges = new SlkUserCollectionChanges(oldProperties.Learners, Learners);

            if (corePropertiesChanged || instructorChanges.HasChanges || learnerChanges.HasChanges)
            {
                store.UpdateAssignment(this, corePropertiesChanged, instructorChanges, learnerChanges);

                if (IsNonELearning)
                {
                    // Update the assignment folder in the Drop Box
                    oldProperties.PopulateSPUsers(slkMembers);
                    PopulateSPUsers(slkMembers);
                    DropBoxManager dropBoxMgr = new DropBoxManager(this);
                    dropBoxMgr.UpdateAssignment(oldProperties);
                }

            }

        }

        void SaveNewAssignment(SPWeb web, SlkRole slkRole, SlkMemberships slkMembers)
        {
            // Security checks: If assigning as an instructor, fails if user isn't an instructor on
            // the web (implemented by calling EnsureInstructor).  Fails if the user doesn't have access to the package/file
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (PackageFormat == null && Location == null)
            {
                throw new InvalidOperationException(AppResources.InvalidNewAssignment);
            }

            SPSiteGuid = web.Site.ID;
            SPWebGuid = web.ID;

            VerifyRole(web, slkRole);
            Id = store.CreateAssignment(this);

            //Update the MRU list
            store.AddToUserWebList(web);

            if (IsNonELearning)
            {
                PopulateSPUsers(slkMembers);
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

        void PopulateSPUsers(SlkMemberships members, SlkUserCollection users)
        {
            if (members != null)
            {
                foreach (SlkUser user in users)
                {
                    if (user.SPUser == null)
                    {
                        SlkUser member = members[user.UserId.GetKey()];
                        if (member != null)
                        {
                            user.SPUser = member.SPUser;
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

#region public static methods
        /// <summary>
        /// Returns an <c>AssignmentProperties</c> object populated with default information for
        /// a new assignment based on a given e-learning package or non-e-learning document.
        /// This method doesn't actually create the assignment -- it just returns information that
        /// can be used as defaults for a form that a user would fill in to create a new assignment.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <paramref name="slkRole"/> is <c>SlkRole.Learner</c>, default properties for a
        /// self-assigned assignment are returned.  In this case, the returned
        /// <c>AssignmentProperties</c> will contain no users in 
        /// <c>AssignmentProperties.Instructors</c>, and <c>AssignmentProperties.Learners</c> will
        /// contain only the current user.
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
        /// <param name="store"></param>
        /// <param name="destinationSPWeb">The <c>SPWeb</c> that the assignment would be assigned in.</param>
        /// <param name="slkRole">The <c>SlkRole</c> for which information is to be retrieved.
        ///     Use <c>SlkRole.Learner</c> to get default information for a self-assigned assignment,
        ///     i.e. an assignment with no instructors for which the current learner is the only
        ///     learner.  Otherwise, use <c>SlkRole.Instructor</c>.</param>
        /// <returns></returns>
        public static AssignmentProperties CreateNewAssignmentObject(ISlkStore store, SPWeb destinationSPWeb, SlkRole slkRole)
        {
            // Security checks: Fails if the user isn't an instructor on the web if SlkRole=Instructor
            // (verified by EnsureInstructor).  Fails if the user doesn't have access to the
            // package (verified by calling RegisterPackage or by accessing
            // the properties of the non-elearning file).

            // Check parameters
            if (destinationSPWeb == null)
            {
                throw new ArgumentNullException("destinationSPWeb");
            }

            // Verify that the web is in the site
            if (destinationSPWeb.Site.ID != store.SPSiteGuid)
            {
                throw new InvalidOperationException(AppResources.SPWebDoesNotMatchSlkSPSite);
            }

            UserItemIdentifier currentUserId = store.CurrentUserId;
            AssignmentProperties assignment = new AssignmentProperties(null, store);
            assignment.StartDate = DateTime.Today; // midnight today
            assignment.DueDate = null;
            assignment.EmailChanges = false;
            assignment.CreatedById = currentUserId;

            // Role checking and determine if self assigned
            bool isSelfAssigned;
            if (slkRole == SlkRole.Instructor)
            {
                store.EnsureInstructor(destinationSPWeb);
                isSelfAssigned = false;
            }
            else if (slkRole == SlkRole.Learner)
            {
                isSelfAssigned = true;
            }
            else
            {
                throw new ArgumentException(AppResources.InvalidSlkRole, "slkRole");
            }

            assignment.ShowAnswersToLearners = isSelfAssigned;
            assignment.AutoReturn = isSelfAssigned;
            if (isSelfAssigned)
            {
                assignment.Learners.Add(new SlkUser(currentUserId, destinationSPWeb.CurrentUser));
            }
            else
            {
                assignment.Instructors.Add(new SlkUser(currentUserId, destinationSPWeb.CurrentUser));
            }

            return assignment;
        }
#endregion public static methods
    }
}

