using System;
using System.Globalization;
using System.Diagnostics;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
    /// about an assignment related to one of the learners of the assignment).  These are properties
    /// generally accessible to the learner.
    /// </summary>
    ///
    public class LearnerAssignmentProperties
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>LearnerAssignmentId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        LearnerAssignmentItemIdentifier m_learnerAssignmentId;

        /// <summary>
        /// Holds the value of the <c>GuidId</c> property. The GuidId like the LearnerAssignmentId 
        /// represents the LearnerAssignment uniquely
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_learnerAssignmentGuidId = Guid.Empty;
        /// <summary>
        /// Holds the value of the <c>AssignmentId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AssignmentItemIdentifier m_assignmentId;

        /// <summary>
        /// Holds the value of the <c>SPSiteGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spSiteGuid = Guid.Empty;

        /// <summary>
        /// Holds the value of the <c>SPWebGuid</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Guid m_spWebGuid = Guid.Empty;

        /// <summary>
        /// Holds the value of the <c>RootActivityId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ActivityPackageItemIdentifier m_rootActivityId;

        /// <summary>
        /// Holds the value of the <c>Location</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_location;

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
        /// Holds the value of the <c>HasInstructors</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_hasInstructors;

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
        /// Holds the value of the <c>CreatedByName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_createdByName;

        /// <summary>
        /// Holds the value of the <c>LearnerId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        UserItemIdentifier m_learnerId;

        /// <summary>
        /// Holds the value of the <c>LearnerName</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_learnerName;

        /// <summary>
        /// Holds the value of the <c>Status</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        LearnerAssignmentState m_status;

        /// <summary>
        /// Holds the value of the <c>AttemptId</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        AttemptItemIdentifier m_attemptId;

        /// <summary>
        /// Holds the value of the <c>CompletionStatus</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        CompletionStatus m_completionStatus;

        /// <summary>
        /// Holds the value of the <c>SuccessStatus</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        SuccessStatus m_successStatus;

        /// <summary>
        /// Holds the value of the <c>GradedPoints</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        float? m_gradedPoints;

        /// <summary>
        /// Holds the value of the <c>FinalPoints</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        float? m_finalPoints;

        /// <summary>
        /// Holds the value of the <c>InstructorComments</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_instructorComments;

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the identifier of the learner assignment represented by this object.
        /// </summary>
        public LearnerAssignmentItemIdentifier LearnerAssignmentId
        {
            [DebuggerStepThrough]
            get
            {
                return m_learnerAssignmentId;
            }
            internal set
            {
                m_learnerAssignmentId = value;
            }
        }

        /// <summary>
        /// Gets the GUID identifier of the learner assignment represented by this object.
        /// </summary>
        public Guid LearnerAssignmentGuidId
        {
            [DebuggerStepThrough]
            get
            {
                return m_learnerAssignmentGuidId;
            }
        }

        /// <summary>
        /// Gets the identifier of the assignment that this learner assignment is associated with.
        /// </summary>
        public AssignmentItemIdentifier AssignmentId
        {
            [DebuggerStepThrough]
            get
            {
                return m_assignmentId;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_assignmentId = value;
            }
        }

        /// <summary>
        /// Gets the <c>Guid</c> of the SPSite that contains the SPWeb that the assignment is
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

        /// <summary>
        /// Gets the <c>Guid</c> of the SPWeb that the assignment is associated with.
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
        /// Gets the identifier of the root activity of the e-learning package (SCORM or LRM) that this
        /// assignment is associated with.  <c>null</c> if a non-e-learning document is associated
        /// with the assignment.
        /// </summary>
        public ActivityPackageItemIdentifier RootActivityId
        {
            [DebuggerStepThrough]
            get
            {
                return m_rootActivityId;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_rootActivityId = value;
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

        /// <summary>
        /// Gets the title of the assignment.
        /// </summary>
        public string Title
        {
            [DebuggerStepThrough]
            get
            {
                return m_title;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_title = value;
            }
        }

        /// <summary>
        /// Gets the description of the assignment.
        /// </summary>
        public string Description
        {
            [DebuggerStepThrough]
            get
            {
                return m_description;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_description = value;
            }
        }

        /// <summary>
        /// Gets the nominal maximum number of points possible for the assignment.  <c>null</c> if
        /// points possible is not specified.
        /// </summary>
        public Nullable<Single> PointsPossible
        {
            [DebuggerStepThrough]
            get
            {
                return m_pointsPossible;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_pointsPossible = value;
            }
        }

        /// <summary>
        /// Gets the date/time that the assignment starts.  Unlike the related value stored in the
        /// SharePoint Learning Kit database, this value is a local date/time, not a UTC value.
        /// </summary>
        public DateTime StartDate
        {
            [DebuggerStepThrough]
            get
            {
                return m_startDate;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_startDate = value;
            }
        }

        /// <summary>
        /// Gets the due date/time of the assignment.  <c>null</c> if there is no due date.  Unlike
        /// the related value stored in the SharePoint Learning Kit database, this value is a local
        /// date/time, not a UTC value.
        /// </summary>
        public Nullable<DateTime> DueDate
        {
            [DebuggerStepThrough]
            get
            {
                return m_dueDate;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_dueDate = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether each learner assignment associated with this assignment
        /// should automatically be returned to the learner when the learner marks it as complete
        /// (after auto-grading), rather than requiring an instructor to manually return the assignment
        /// to the student.
        /// </summary>
        public bool AutoReturn
        {
            [DebuggerStepThrough]
            get
            {
                return m_autoReturn;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_autoReturn = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the assignment associated with this learner assignment
        /// has instructors.  This value is <c>false</c> in the case of a self-assigned assignment.
        /// </summary>
        public bool HasInstructors
        {
            [DebuggerStepThrough]
            get
            {
                return m_hasInstructors;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_hasInstructors = value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether answers will be shown to the learner when a learner
        /// assignment associated with the assignment is returned to the learner.  This only applies to
        /// certain types of e-learning content.
        /// </summary>
        public bool ShowAnswersToLearners
        {
            [DebuggerStepThrough]
            get
            {
                return m_showAnswersToLearners;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_showAnswersToLearners = value;
            }
        }

        /// <summary>
        /// Gets the <c>UserItemIdentifier</c> of the user who created the assignment.
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
        /// Gets the name of the user who created the assignment.
        /// </summary>
        public string CreatedByName
        {
            [DebuggerStepThrough]
            get
            {
                return m_createdByName;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_createdByName = value;
            }
        }

        /// <summary>
        /// Gets the <c>UserItemIdentifier</c> of the user that this learner assignment is assigned to.
        /// </summary>
        public UserItemIdentifier LearnerId
        {
            [DebuggerStepThrough]
            get
            {
                return m_learnerId;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_learnerId = value;
            }
        }

        /// <summary>
        /// Gets the name of the user that this learner assignment is assigned to.
        /// </summary>
        public string LearnerName
        {
            [DebuggerStepThrough]
            get
            {
                return m_learnerName;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_learnerName = value;
            }
        }

        /// <summary>
        /// Gets the <c>LearnerAssignmentState</c> of this learner assignment.
        /// </summary>
        public LearnerAssignmentState Status
        {
            [DebuggerStepThrough]
            get
            {
                return m_status;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_status = value;
            }
        }

        /// <summary>
        /// Gets the <c>AttemptId</c> of this learner assignment.  <c>null</c> if the assignment is a
        /// non-e-learning assignment, or if it's an e-learning assignment which the learner hasn't
        /// yet launched for the first time.
        /// </summary>
        public AttemptItemIdentifier AttemptId
        {
            [DebuggerStepThrough]
            get
            {
                return m_attemptId;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_attemptId = value;
            }
        }

        /// <summary>
        /// Gets a <c>CompletionStatus</c> value indicating whether the SCORM 2005 package associated
        /// with this content considers the content to be completed by the learner.  This property is
        /// only used for assignments of SCORM 2004 packages; <c>CompletionStatus.Unknown</c> is
        /// returned for SCORM 1.2 and Class Server LRM assignments.
        /// </summary>
        public CompletionStatus CompletionStatus
        {
            [DebuggerStepThrough]
            get
            {
                return m_completionStatus;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_completionStatus = value;
            }
        }

        /// <summary>
        /// Gets a <c>SuccessStatus</c> value indicating whether the SCORM package associated with
        /// this content considers the learner to have succeeded (as defined by the content).  This
        /// property is only used for assignments of SCORM 2004 packages; <c>SuccessStatus.Unknown</c>
        /// is returned for SCORM 1.2 and Class Server LRM assignments.
        /// </summary>
        public SuccessStatus SuccessStatus
        {
            [DebuggerStepThrough]
            get
            {
                return m_successStatus;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_successStatus = value;
            }
        }

        /// <summary>
        /// Gets the number of points the learner received from automatic and manual grading of the
        /// learner assignment.  If the content type does not support grading, or if the grade is
        /// "blank", <c>GradedPoints</c> will be <c>null</c>.
        /// </summary>
        public Nullable<Single> GradedPoints
        {
            [DebuggerStepThrough]
            get
            {
                return m_gradedPoints;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_gradedPoints = value;
            }
        }

        /// <summary>
        /// Gets the number of points the learner received on this learner assignment.  When the
        /// learner submits the assignment, <c>FinalPoints</c> is initially the same as
        /// <c>GradedPoints</c>, but the instructor may manually change the value of <c>FinalPoint</c>.
        /// For example, the instructor may award bonus points to the learner.
        /// </summary>
        public Nullable<Single> FinalPoints
        {
            [DebuggerStepThrough]
            get
            {
                return m_finalPoints;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_finalPoints = value;
            }
        }

        /// <summary>
        /// Gets comments from the instructor (if any) on this learner assignment; <c>String.Empty</c>
        /// if none. 
        /// </summary>
        public string InstructorComments
        {
            [DebuggerStepThrough]
            get
            {
                return m_instructorComments;
            }
            [DebuggerStepThrough]
            internal set
            {
                m_instructorComments = value;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by
        ///     this object.</param>
        ///
        internal LearnerAssignmentProperties(LearnerAssignmentItemIdentifier learnerAssignmentId)
        {
            m_learnerAssignmentId = learnerAssignmentId;
        }
        internal LearnerAssignmentProperties(Guid learnerAssignmentGuidId)
        {
            m_learnerAssignmentGuidId = learnerAssignmentGuidId;
        }
    }
}
