using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
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
        bool hasInstructors;
        bool hasInstructorsIsSet;
        bool isSelfAssigned;
        SlkUserCollection instructors;
        Dictionary<long, LearnerAssignmentProperties> keyedResults = new Dictionary<long, LearnerAssignmentProperties>();
        Dictionary<long, LearnerAssignmentProperties> userResults = new Dictionary<long, LearnerAssignmentProperties>();

#region properties
        /// <summary>The ISlkStore to use.</summary>
        public ISlkStore Store { get; private set; }

        /// <summary>Indicates if it is a self-assignment.</summary>
        public bool IsSelfAssignment
        {
            get { return !HasInstructors ;}
        }

        /// <summary>Indicates whether to hide points or not.</summary>
        public bool HidePoints
        {
            get
            {
                return (IsNonELearning && Store.Settings.UseGrades && Store.Settings.HidePointsForNonELearning);
            }
        }

        /// <summary>Indicates if the assignment has any instructors.</summary>
        public bool HasInstructors
        {
            get 
            {
                if (hasInstructorsIsSet)
                {
                    return hasInstructors;
                }
                else
                {
                    return Instructors.Count != 0;
                }
            }

            set
            {
                hasInstructors = value;
                hasInstructorsIsSet = true;
            }
        }
        /// <summary>Indicates if the assignment is complete or not.</summary>
        /// <value></value>
        public bool IsCompleted
        {
            get
            {
                if (Results == null)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    foreach (LearnerAssignmentProperties learnerGrading in Results)
                    {
                        if (learnerGrading.Status != LearnerAssignmentState.Completed)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        /// <summary>The results of the assignment.</summary>
        public ReadOnlyCollection<LearnerAssignmentProperties> Results { get; private set; }

        /// <summary>Returns the LearnerAssignmentProperties.</summary>
        public LearnerAssignmentProperties this[long id]
        {
            get { return keyedResults[id] ;}
        }

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

        /// <summary>Gets or sets the name of the user who created this assignment. </summary>
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets the date/time that this assignment was created.  Unlike the related value stored in
        /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the collection of instructors of this assignment.
        /// </summary>
        public SlkUserCollection Instructors 
        { 
            get 
            { 
                if (instructors == null)
                {
                    // Lazy Load
                    Instructors = new SlkUserCollection();
                    if (hasInstructorsIsSet && HasInstructors)
                    {
                        Store.LoadInstructors(Id, instructors);
                    }
                }

                return instructors ;
            }

            private set { instructors = value ;}
        }

        /// <summary>
        /// Gets or sets the collection of learners assigned this assignment.
        /// </summary>
        public SlkUserCollection Learners { get; private set; }

        /// <summary>Indicates if the assignment is e-learning or not.</summary>
        public bool IsELearning
        {
            get { return IsNonELearning == false; }
        }

        /// <summary>Indicates if the assignment is e-learning or not.</summary>
        public bool IsNonELearning
        {
            get { return (PackageFormat == null && RootActivityId == null) ;}
        }

        /// <summary>Indicates if the assignment is a no package assignment.</summary>
        public bool IsNoPackageAssignment
        {
            get { return Location == Package.NoPackageLocation.ToString() ;}
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
        AssignmentProperties(ISlkStore store)
        {
            this.Store = store;
            Learners = new SlkUserCollection();
        }
#endregion constructors

#region public methods
        /// <summary>Gets the result for a learner.</summary>
        /// <param name="learner">The learner to get the result for.</param>
        public LearnerAssignmentProperties ResultForLearner(SlkUser learner)
        {
            LearnerAssignmentProperties result = null;
            userResults.TryGetValue(learner.UserId.GetKey(), out result);
            return result;
        }

        /// <summary>Sends a reminder email.</summary>
        public void SendReminderEmail()
        {
            if (EmailChanges)
            {
                List<SlkUser> users = new List<SlkUser>();

                foreach (LearnerAssignmentProperties learnerAssignment in keyedResults.Values)
                {
                    if (learnerAssignment.Status != null)
                    {
                        switch (learnerAssignment.Status.Value)
                        {
                            case LearnerAssignmentState.NotStarted:
                            case LearnerAssignmentState.Active:
                                users.Add(learnerAssignment.User);
                                break;
                        }
                    }
                }

                if (users.Count > 0)
                {
                    using (AssignmentEmailer emailer = new AssignmentEmailer(this, Store.Settings.EmailSettings, SPSiteGuid, SPWebGuid))
                    {
                        emailer.SendReminderEmail(users);
                    }
                }
            }
        }

        /// <summary>Sends and email when a learner submits an assignment.</summary>
        /// <param name="name">The name of the learner.</param>
        public void SendSubmitEmail(string name)
        {
            if (EmailChanges)
            {
                using (AssignmentEmailer emailer = new AssignmentEmailer(this, Store.Settings.EmailSettings, SPSiteGuid, SPWebGuid))
                {
                    emailer.SendSubmitEmail(name);
                }
            }
        }

        /// <summary>Deletes the assignment.</summary>
        public void Delete(SPWeb web)
        {
            Store.DeleteAssignment(Id);

            if (EmailChanges)
            {
                using (AssignmentEmailer emailer = new AssignmentEmailer(this, Store.Settings.EmailSettings, web))
                {
                    emailer.SendCancelEmail(Learners);
                }
            }

            //Delete corresponding assignment folder from the drop box if exists
            if (IsNonELearning)
            {
                DropBoxManager dropBoxManager = new DropBoxManager(this);
                dropBoxManager.DeleteAssignmentFolder();
            }
        }

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
        public void Save(SPWeb web, SlkRole slkRole)
        {
            // Verify that the web is in the site
            if (web.Site.ID != Store.SPSiteGuid)
            {
                throw new InvalidOperationException(SlkCulture.GetResources().SPWebDoesNotMatchSlkSPSite);
            }

            if (Id == null)
            {
                SaveNewAssignment(web, slkRole);
            }
            else
            {
                UpdateAssignment(web);
            }
        }

        /// <summary>Makes the assignment be a no package assignemnt.</summary>
        public void MakeNoPackageAssignment(string title)
        {
            Location = Package.NoPackageLocation.ToString();
            if (String.IsNullOrEmpty(Title))
            {
                if (string.IsNullOrEmpty(title))
                {
                    Title = SlkCulture.GetResources().NoPackageTitle;
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
        public void SetLocation(SharePointFileLocation location, Nullable<int> organizationIndex)
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
        public void SetLocation(SharePointFileLocation location, Nullable<int> organizationIndex, string title)
        {
            if (location == null)
            {
                throw new ArgumentNullException("location");
            }

            if (location.ToString() == Package.NoPackageLocation.ToString())
            {
                MakeNoPackageAssignment(title);
                return;
            }

            // Access the properties of the file to verify that the user has access to the file
            SPFile file = location.LoadFile();
            System.Collections.Hashtable fileProperties = file.Properties;

            // set <rootActivityId> to the ID of the organization, or null if a non-e-learning document is being assigned
            if (organizationIndex != null)
            {
                using (Package package = new Package(Store, file, location))
                {
                    package.Register();

                    RootActivityId = package.FindRootActivity(organizationIndex.Value);

                    bool existingTitle = (string.IsNullOrEmpty(Title) == false);

                    if (existingTitle == false)
                    {
                        Title = package.Title;
                    }

                    if (string.IsNullOrEmpty(Description))
                    {
                        Description = package.Description;
                    }

                    // validate <organizationIndex>
                    if ((organizationIndex.Value < 0) || (organizationIndex.Value >= package.Organizations.Count))
                    {
                        throw new SafeToDisplayException(SlkCulture.GetResources().InvalidOrganizationIndex);
                    }

                    PackageFormat = package.PackageFormat;

                    // set <organizationNodeReader> to refer to the organization
                    OrganizationNodeReader organizationNodeReader = package.Organizations[organizationIndex.Value];

                    // if there is more than one organization, append the organization title, if any
                    if (existingTitle == false && package.Organizations.Count > 1)
                    {
                        if (!String.IsNullOrEmpty(organizationNodeReader.Title))
                        {
                            SlkCulture culture = new SlkCulture();
                            Title = culture.Format(culture.Resources.SlkPackageAndOrganizationTitle, Title, organizationNodeReader.Title);
                        }
                    }

                    // set <pointsPossible> to the points-possible value stored in the manifest, or null if none
                    switch (PackageFormat)
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
            }
            else  // Non-elearning content
            {
                RootActivityId = null;
                Location = location.ToString();
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

        /// <summary>Creates an AssignmentSaver to save the assignment.</summary>
        public AssignmentSaver CreateSaver()
        {
            return new AssignmentSaver(Store, this);
        }

#endregion public methods

#region private methods
        void CopyInvariantProperties(AssignmentProperties properties)
        {
            SPSiteGuid = properties.SPSiteGuid;
            SPWebGuid = properties.SPWebGuid;
            CreatedById = properties.CreatedById;
            CreatedByName = properties.CreatedByName;
            DateCreated = properties.DateCreated;
            RootActivityId = properties.RootActivityId;
            PackageFormat = properties.PackageFormat;
            Location = properties.Location;
        }

        void UpdateAssignment(SPWeb web)
        {
            AssignmentProperties oldProperties = Store.LoadAssignmentProperties(Id, SlkRole.Instructor);
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
                Store.UpdateAssignment(this, corePropertiesChanged, instructorChanges, learnerChanges);

                if (IsNonELearning)
                {
                    // Update the assignment folder in the Drop Box
                    DropBoxManager dropBoxMgr = new DropBoxManager(this);
                    dropBoxMgr.UpdateAssignment(oldProperties);
                }

                if (EmailChanges)
                {
                    using (AssignmentEmailer emailer = new AssignmentEmailer(this, Store.Settings.EmailSettings, web))
                    {
                        emailer.SendNewEmail(learnerChanges.Additions);
                        emailer.SendCancelEmail(learnerChanges.Removals);
                    }
                }
            }
        }

        void SaveNewAssignment(SPWeb web, SlkRole slkRole)
        {
            // Security checks: If assigning as an instructor, fails if user isn't an instructor on
            // the web (implemented by calling EnsureInstructor).  Fails if the user doesn't have access to the package/file
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (PackageFormat == null && Location == null)
            {
                throw new InvalidOperationException(SlkCulture.GetResources().InvalidNewAssignment);
            }

            SPSiteGuid = web.Site.ID;
            SPWebGuid = web.ID;

            VerifyRole(web, slkRole);
            Id = Store.CreateAssignment(this);

            if (isSelfAssigned == false)
            {
                //Update the MRU list
                Store.AddToUserWebList(web);
            }

            if (IsNonELearning)
            {
                DropBoxManager dropBoxMgr = new DropBoxManager(this);
                Microsoft.SharePoint.Utilities.SPUtility.ValidateFormDigest();
                dropBoxMgr.CreateAssignmentFolder();
            }

            if (EmailChanges)
            {
                using (AssignmentEmailer emailer = new AssignmentEmailer(this, Store.Settings.EmailSettings, web))
                {
                    emailer.SendNewEmail(Learners);
                }
            }
        }

        void VerifyRole(SPWeb web, SlkRole slkRole)
        {
            if ((slkRole != SlkRole.Instructor) && (slkRole != SlkRole.Learner))
            {
                throw new ArgumentOutOfRangeException("slkRole");
            }

            isSelfAssigned = (slkRole == SlkRole.Learner) ? true : false;

            if (isSelfAssigned)
            {
                VerifyIsValidSelfAssigned();
            }
            else
            {
                Store.EnsureInstructor(web);
            }
        }

        void VerifyIsValidSelfAssigned()
        {
            // set <currentUserId> to the UserItemIdentifier of the current user; note that this
            // requires a round trip to the database
            UserItemIdentifier currentUserId = Store.CurrentUserId;

            // verify that <properties> specify no instructors and that the current user is the
            // only learner
            if (Instructors.Count != 0)
            {
                throw new UnauthorizedAccessException(SlkCulture.GetResources().InvalidSelfAssignment);
            }

            if ((Learners.Count != 1) || (Learners[0].UserId != currentUserId))
            {
                throw new UnauthorizedAccessException(SlkCulture.GetResources().InvalidSelfAssignment);
            }
        }
#endregion private methods

        internal void AssignResults(List<LearnerAssignmentProperties> results)
        {
            foreach (LearnerAssignmentProperties result in results)
            {
                keyedResults[result.LearnerAssignmentId.GetKey()] = result;

                if (result.User != null && result.User.UserId != null)
                {
                    userResults[result.User.UserId.GetKey()] = result;
                }
            }

            Results = new ReadOnlyCollection<LearnerAssignmentProperties>(results) ;
        }

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
                throw new InvalidOperationException(SlkCulture.GetResources().SPWebDoesNotMatchSlkSPSite);
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
                throw new ArgumentException(SlkCulture.GetResources().InvalidSlkRole, "slkRole");
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

        /// <summary>Loads the assignment properties.</summary>
        public static AssignmentProperties Load(AssignmentItemIdentifier assignmentItemIdentifier, SlkStore store)
        {
            return store.LoadAssignmentProperties(assignmentItemIdentifier, SlkRole.Instructor);
        }

        /// <summary>Loads an AssignmentProperties for grading.</summary>
        /// <param name="assignmentItemIdentifier">The id of the assignment.</param>
        /// <param name="store"></param>
        /// <returns></returns>
        public static AssignmentProperties LoadForGrading(AssignmentItemIdentifier assignmentItemIdentifier, ISlkStore store)
        {
            return store.GetGradingProperties(assignmentItemIdentifier);
        }

        /// <summary>Creates a self assingment.</summary>
        /// <returns>The ID of the learner assignment.</returns>
        public static Guid CreateSelfAssignment(SlkStore store, SPWeb web, SharePointFileLocation location, Nullable<int> organizationIndex)
        {
            AssignmentProperties properties = CreateNewAssignmentObject(store, web, SlkRole.Learner);
            properties.SetLocation(location, organizationIndex);
            // Have to allow unsafe updates or self assignment fails
            bool allowUnsafeUpdates = web.AllowUnsafeUpdates;
            web.AllowUnsafeUpdates = true;
            try
            {
                properties.Save(web, SlkRole.Learner);
            }
            finally
            {
                web.AllowUnsafeUpdates = allowUnsafeUpdates;
            }

            return properties.Learners[0].AssignmentUserGuidId ;
        }

        /// <summary>Loads a self assignment for a particular location if one exists.</summary>
        /// <param name="location">The location of the assignment.</param>
        /// <param name="store">The <see cref="ISlkStore"/> to use.</param>
        /// <returns>An <see cref="AssignmentProperties"/>.</returns>
        public static AssignmentProperties LoadSelfAssignmentForLocation(SharePointFileLocation location, ISlkStore store)
        {
            return store.LoadSelfAssignmentForLocation(location);
        }
#endregion public static methods

#region DropBoxUpdate
        class DropBoxUpdate
        {
            public LearnerAssignmentState State { get; private set; }
            public SPUser User { get; private set; }

            public DropBoxUpdate(LearnerAssignmentState state, SPUser user)
            {
                State = state;
                User = user;
            }
        }
#endregion DropBoxUpdate

    }

}

