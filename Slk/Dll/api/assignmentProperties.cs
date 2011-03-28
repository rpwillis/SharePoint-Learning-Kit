using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

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
#endregion properties

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(AssignmentItemIdentifier id) : this()
        {
            this.Id = id;
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(long id) : this()
        {
            if (id != 0)
            {
                this.Id = new AssignmentItemIdentifier(id);
            }
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        AssignmentProperties()
        {
            Instructors = new SlkUserCollection();
            Learners = new SlkUserCollection();
        }
#endregion constructors

#region public methods
        /// <summary>Populates the SPUser property of each SlkUser in the collection of SLK members.</summary>
        /// <param name="slkMembers">The collection of SLK members to populate.</param>
        public void PopulateSPUsers(SlkMemberships slkMembers)
        {
            PopulateSPUsers(slkMembers.Learners, Learners);
            PopulateSPUsers(slkMembers.Instructors, Instructors);
        }
#endregion public methods

#region private methods
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
#endregion private methods
    }
}

