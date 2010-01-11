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
        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>SPSiteGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spSiteGuid = Guid.Empty;

        AssignmentItemIdentifier id;

        /// <summary>
        /// Holds the value of the <c>SPWebGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spWebGuid = Guid.Empty;

        /// <summary>
        /// Holds the value of the <c>Title</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_title;

        /// <summary>
        /// Holds the value of the <c>Description</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_description;

        /// <summary>
        /// Holds the value of the <c>PointsPossible</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        float? m_pointsPossible;

        /// <summary>
        /// Holds the value of the <c>StartDate</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DateTime m_startDate;

        /// <summary>
        /// Holds the value of the <c>DueDate</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DateTime? m_dueDate;

        /// <summary>
        /// Holds the value of the <c>AutoReturn</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_autoReturn;

        /// <summary>
        /// Holds the value of the <c>ShowAnswersToLearners</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_showAnswersToLearners;

        /// <summary>
        /// Holds the value of the <c>CreatedById</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        UserItemIdentifier m_createdById;

        /// <summary>
        /// Holds the value of the <c>DateCreated</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        DateTime m_dateCreated;

        /// <summary>
        /// Holds the value of the <c>Instructors</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkUserCollection m_instructors = new SlkUserCollection();

        /// <summary>
        /// Holds the value of the <c>Learners</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SlkUserCollection m_learners = new SlkUserCollection();

        /// <summary>
        /// Holds the value of the <c>PackageFormat</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        PackageFormat? m_packageFormat;

        /// <summary>
        /// Holds the value of the <c>Location</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_location;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the <c>Guid</c> of the SPSite that contains the SPWeb that this assignment is
        /// associated with.
        /// </summary>
        public Guid SPSiteGuid
        {
            [DebuggerStepThrough]
            get
            {
                return m_spSiteGuid;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_spSiteGuid = value;
            }
        }

        /// <summary>The Id of the assignment.</summary>
        public AssignmentItemIdentifier Id
        {
            get { return id ;}
        }

        /// <summary>
        /// Gets the <c>Guid</c> of the SPWeb that this assignment is associated with.
        /// </summary>
        public Guid SPWebGuid
        {
            [DebuggerStepThrough]
            get
            {
                return m_spWebGuid;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_spWebGuid = value;
            }
        }

        /// <summary>
        /// Gets or sets the title of the assignment.
        /// </summary>
        public string Title
        {
            [DebuggerStepThrough]
            get
            {
                return m_title;
            }
            [DebuggerStepThrough]
            set
            {
                m_title = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the assignment.
        /// </summary>
        public string Description
        {
            [DebuggerStepThrough]
            get
            {
                return m_description;
            }
            [DebuggerStepThrough]
            set
            {
                m_description = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal maximum number of points possible for the assignment.
        /// <c>null</c> if points possible is not specified.
        /// </summary>
        public Nullable<Single> PointsPossible
        {
            [DebuggerStepThrough]
            get
            {
                return m_pointsPossible;
            }
            [DebuggerStepThrough]
            set
            {
                m_pointsPossible = value;
            }
        }

        /// <summary>
        /// Gets or sets the date/time that this assignment starts.  Unlike the related value stored in
        /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime StartDate
        {
            [DebuggerStepThrough]
            get
            {
                return m_startDate;
            }
            [DebuggerStepThrough]
            set
            {
                m_startDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the due date/time of this assignment.  <c>null</c> if there is no due date.
        /// Unlike the related value stored in the SharePoint Learning Kit database, this value is a
        /// local date/time, not a UTC value.
        /// </summary>
        public Nullable<DateTime> DueDate
        {
            [DebuggerStepThrough]
            get
            {
                return m_dueDate;
            }
            [DebuggerStepThrough]
            set
            {
                m_dueDate = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether each learner assignment associated with this
        /// assignment should automatically be returned to the learner when the learner marks it as
        /// complete (after auto-grading), rather than requiring an instructor to manually return the
        /// assignment to the student.
        /// </summary>
        public bool AutoReturn
        {
            [DebuggerStepThrough]
            get
            {
                return m_autoReturn;
            }
            [DebuggerStepThrough]
            set
            {
                m_autoReturn = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether answers will be shown to the learner when a
        /// learner assignment associated with this assignment is returned to the learner.  This only
        /// applies to certain types of e-learning content.
        /// </summary>
        public bool ShowAnswersToLearners
        {
            [DebuggerStepThrough]
            get
            {
                return m_showAnswersToLearners;
            }
            [DebuggerStepThrough]
            set
            {
                m_showAnswersToLearners = value;
            }
        }

        /// <summary>
        /// Gets or sets the <c>UserItemIdentifier</c> of the user who created this assignment.
        /// </summary>
        public UserItemIdentifier CreatedById
        {
            [DebuggerStepThrough]
            get
            {
                return m_createdById;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_createdById = value;
            }
        }

        /// <summary>
        /// Gets the date/time that this assignment was created.  Unlike the related value stored in
        /// the SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime DateCreated
        {
            [DebuggerStepThrough]
            get
            {
                return m_dateCreated;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_dateCreated = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of instructors of this assignment.
        /// </summary>
        public SlkUserCollection Instructors
        {
            [DebuggerStepThrough]
            get
            {
                return m_instructors;
            }
        }

        /// <summary>
        /// Gets or sets the collection of learners assigned this assignment.
        /// </summary>
        public SlkUserCollection Learners
        {
            [DebuggerStepThrough]
            get
            {
                return m_learners;
            }
        }

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
        public Nullable<PackageFormat> PackageFormat
        {
            [DebuggerStepThrough]
            get
            {
                return m_packageFormat;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_packageFormat = value;
            }
        }

        /// <summary>
        /// Gets the MLC SharePoint location string of the e-learning package or non-e-learning
        /// document associated with the assignment.
        /// </summary>
        public string Location
        {
            [DebuggerStepThrough]
            get
            {
                return m_location;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_location = value;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(AssignmentItemIdentifier id)
        {
            this.id = id;
        }

        /// <summary>Initializes a new instance of <see cref="AssignmentProperties"/>.</summary>
        public AssignmentProperties(long id)
        {
            if (id != 0)
            {
                this.id = new AssignmentItemIdentifier(id);
            }
        }

        public void PopulateSPUsers(SlkMemberships slkMembers)
        {
            PopulateSPUsers(slkMembers.Learners, Learners);
            PopulateSPUsers(slkMembers.Instructors, Instructors);
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
    }
}

