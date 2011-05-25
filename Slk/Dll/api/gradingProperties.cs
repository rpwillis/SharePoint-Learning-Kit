using System;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
    /// about an assignment related to one of the learners of the assignment) that's used by an
    /// instructor during grading.
    /// </summary>
    ///
    public class GradingProperties
    {
#region properties
        /// <summary>The SlkUser the result is for.</summary>
        public SlkUser User { get; internal set; }

        /// <summary>The assignment the properties are for.</summary>
        public AssignmentProperties Assignment { get; private set; }

        /// <summary>Gets the identifier of the learner assignment represented by this object.</summary>
        public LearnerAssignmentItemIdentifier LearnerAssignmentId { get; private set; }

        /// <summary>Gets the GUID identifier of the learner assignment represented by this object.</summary>
        public Guid LearnerAssignmentGuidId { get; internal set; }

        /// <summary>Gets the <c>UserItemIdentifier</c> of the user that this learner assignment is assigned to.</summary>
        public UserItemIdentifier LearnerId { get; internal set; }

        /// <summary>Gets the name of the user that this learner assignment is assigned to.</summary>
        public string LearnerName { get; internal set; }

        /// <summary>Gets or sets the <c>LearnerAssignmentState</c> of this learner assignment.</summary>
        /// <remarks>
        /// <para>
        /// Changing the value of <c>Status</c>, and then calling
        /// <c>SlkStore.SetGradingProperties</c>, will transition this learner assignment to another
        /// <c>LearnerAssignmentState</c> value.  Only the following state transitions are supported
        /// by <c>SlkStore.SetGradingProperties</c>:
        /// </para>
        /// <list type="bullet">
        ///     <item><description><c>NotStarted</c> to <c>Completed</c>.
        ///         </description>
        ///     </item>
        ///     <item><description><c>Active</c> to <c>Completed</c>.
        ///         </description>
        ///     </item>
        ///     <item><description><c>Completed</c> to <c>Final</c>.
        ///         </description>
        ///     </item>
        ///     <item><description><c>Final</c> to <c>Active</c>.
        ///         </description>
        ///     </item>
        /// </list>
        /// <para>
        /// When setting <c>Status</c>, use the value <c>null</c> to indicate that you don't want to
        /// change the status of the assignment.  If you use the current value, the status also won't
        /// be changed, but you run the risk of another user changing the status between your calls
        /// to <c>SlkStore.GetGradingProperties</c> and <c>SlkStore.SetGradingProperties</c>.
        /// </para>
        /// </remarks>
        public Nullable<LearnerAssignmentState> Status { get; set; }

        /// <summary>
        /// Gets a <c>CompletionStatus</c> value indicating whether the SCORM 2005 package associated
        /// with this content considers the content to be completed by the learner.  This property is
        /// only used for assignments of SCORM 2004 packages; <c>CompletionStatus.Unknown</c> is
        /// returned for SCORM 1.2 and Class Server LRM assignments.
        /// </summary>
        public CompletionStatus CompletionStatus { get; internal set; }

        /// <summary>
        /// Gets a <c>SuccessStatus</c> value indicating whether the SCORM package associated with
        /// this content considers the learner to have succeeded (as defined by the content).  This
        /// property is only used for assignments of SCORM 2004 packages; <c>SuccessStatus.Unknown</c>
        /// is returned for SCORM 1.2 and Class Server LRM assignments.
        /// </summary>
        public SuccessStatus SuccessStatus { get; internal set; }

        /// <summary>
        /// Gets the number of points the learner received from automatic and manual grading of the
        /// learner assignment.  If the content type does not support grading, or if the grade is
        /// "blank", <c>GradedPoints</c> will be <c>null</c>.
        /// </summary>
        public Nullable<Single> GradedPoints { get; internal set; }

        /// <summary>Gets or sets the number of points the learner received on this learner assignment.</summary>
        public Nullable<Single> FinalPoints { get; set; }

        /// <summary>The Grade the learner is marked as.</summary>
        public string Grade { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <c>FinalPoints</c> will be ignore in subsequent
        /// calls to <c>SlkStore.SetGradingProperties</c>.  In that case, the current value of
        /// <c>FinalPoints</c> in the database is not changed to the value of
        /// <c>GradingProperties.FinalPoints</c>.
        /// </summary>
        public bool IgnoreFinalPoints { get; set; }

        /// <summary>Gets or sets comments from the instructor (if any) on this learner assignment;<c>String.Empty</c> if none.</summary>
        public string InstructorComments { get; internal set; }

        /// <summary>Gets or sets the <c>AttemptItemIdentifier</c> of the attempt associated with this learnerssignment, or <c>null</c> if none.</summary>
        internal AttemptItemIdentifier AttemptId { get; set; }
#endregion properties

#region constructors
        /// <summary>Initializes an instance of this class.</summary>
        /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by this object.</param>
        /// <param name="assignment">The assignment the result is for.</param>
        public GradingProperties(LearnerAssignmentItemIdentifier learnerAssignmentId, AssignmentProperties assignment)
        {
            if(learnerAssignmentId == null)
            {
                throw new ArgumentNullException("learnerAssignmentId");
            }
                
            LearnerAssignmentId = learnerAssignmentId;
            Assignment = assignment;
        }
#endregion constructors
    }
}
