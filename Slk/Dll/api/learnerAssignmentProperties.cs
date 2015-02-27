using System;
using System.Globalization;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents properties of a SharePoint Learning Kit learner assignment (i.e. the information
    /// about an assignment related to one of the learners of the assignment) that's used by an
    /// instructor during grading.
    /// </summary>
    ///
    public class LearnerAssignmentProperties
    {
        bool fullSave;

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
        /// <c>LearnerAssignmentProperties.FinalPoints</c>.
        /// </summary>
        public bool IgnoreFinalPoints { get; set; }

        /// <summary>Gets or sets comments from the instructor (if any) on this learner assignment;<c>String.Empty</c> if none.</summary>
        public string InstructorComments { get; internal set; }

        /// <summary>Gets or sets comments from the learner (if any) on this learner assignment;<c>String.Empty</c> if none.</summary>
        public string LearnerComments { get; internal set; }

        /// <summary>Gets or sets the <c>AttemptItemIdentifier</c> of the attempt associated with this learnerssignment, or <c>null</c> if none.</summary>
        internal AttemptItemIdentifier AttemptId { get; set; }
#endregion properties

#region constructors
        /// <summary>Initializes an instance of this class.</summary>
        /// <param name="learnerAssignmentId">The identifier of the learner assignment represented by this object.</param>
        /// <param name="assignment">The assignment the result is for.</param>
        public LearnerAssignmentProperties(LearnerAssignmentItemIdentifier learnerAssignmentId, AssignmentProperties assignment)
        {
            if(learnerAssignmentId == null)
            {
                throw new ArgumentNullException("learnerAssignmentId");
            }
                
            LearnerAssignmentId = learnerAssignmentId;
            Assignment = assignment;
        }
#endregion constructors

#region public methods
        /// <summary>Saves changes to the assignment.</summary>
        /// <param name="moveStatusForward">True if move the status forward from an instructor's point of view.</param>
        /// <param name="returnAssignment">True if assignment should be returned.</param>
        /// <param name="saver">The <see cref="AssignmentSaver"/> to use.</param>
        public void Save(bool moveStatusForward, bool returnAssignment, AssignmentSaver saver)
        {
            if (returnAssignment)
            {
                fullSave = true;
                Return(saver);
            }
            else if (moveStatusForward)
            {
                fullSave = true;

                try
                {
                    switch (Status)
                    {
                        case LearnerAssignmentState.NotStarted:
                            // Collect as instructor is calling
                            Collect(saver);
                            break;

                        case LearnerAssignmentState.Active:
                            // Collect
                            Collect(saver);
                            break;

                        case LearnerAssignmentState.Completed:
                            // Make Final
                            Return(saver);
                            break;

                        case LearnerAssignmentState.Final:
                            // Reactivate
                            Reactivate(saver);
                            break;
                    }
                }
                finally
                {
                    fullSave = false;
                }
            }
            else
            {
                // Just save
                Assignment.Store.SaveLearnerAssignment(LearnerAssignmentId, IgnoreFinalPoints, FinalPoints, InstructorComments, Grade, null, null, saver.CurrentJob);
            }
        }

        /// <summary>Saves the learner comment.</summary>
        /// <param name="comment">The comment to save.</param>
        public void SaveLearnerComment(string comment)
        {
            Assignment.Store.SaveLearnerComment(LearnerAssignmentId, comment);
        }

        /// <summary>Starts the assignment.</summary>
        public void Start()
        {
            CheckUserIsLearner();

            // Check the status
            switch (Status)
            {
                case LearnerAssignmentState.NotStarted:
                    break;

                case LearnerAssignmentState.Active:
                    return;

                case LearnerAssignmentState.Completed:
                    throw InvalidTransitionException(LearnerAssignmentState.Completed, LearnerAssignmentState.Active);

                case LearnerAssignmentState.Final:
                    throw InvalidTransitionException(LearnerAssignmentState.Final, LearnerAssignmentState.Active);

                default:
                    // New status added
                    break;
            }

            StoredLearningSession session = CreateAttemptIfRequired(false);

            LearnerAssignmentState newStatus = LearnerAssignmentState.Active;

            if (session == null)
            {
                Save(newStatus, null, NonELearningStatus(AttemptStatus.Active), null, null);
            }
            else
            {
                Save(newStatus, null, NonELearningStatus(AttemptStatus.Active), null, null);
            }

            Status = newStatus;
        }

        /// <summary>Returns the assignment.</summary>
        public void Return(AssignmentSaver saver)
        {
            CheckUserIsInstructor();

            StoredLearningSession session = null;

            // Check the status
            switch (Status)
            {
                case LearnerAssignmentState.NotStarted:
                    // Force collection & return
                    session = CreateAttemptIfRequired(false);
                    break;

                case LearnerAssignmentState.Active:
                    // Force collection & return
                    break;

                case LearnerAssignmentState.Completed:
                    break;

                case LearnerAssignmentState.Final:
                    // No need to return
                    return;

                default:
                    // New status added
                    break;
            }

            LearnerAssignmentState newStatus = LearnerAssignmentState.Final;

            if (session == null)
            {
                Save(newStatus, true, NonELearningStatus(AttemptStatus.Completed), null, saver);
            }
            else
            {
                Save(newStatus, true, NonELearningStatus(AttemptStatus.Completed), session.TotalPoints, saver);
            }

            Status = newStatus;

            if (Assignment.EmailChanges)
            {
                saver.SendReturnEmail(User, this);
            }

            if (Assignment.IsNonELearning)
            {
                saver.UpdateDropBoxPermissions(newStatus, User);
            }
        }

        /// <summary>Collects the assignment.</summary>
        public void Collect(AssignmentSaver saver)
        {
            CheckUserIsInstructor();
            StoredLearningSession session = null;

            // Check the status
            switch (Status)
            {
                case LearnerAssignmentState.NotStarted:
                    session = CreateAttemptIfRequired(true);
                    break;

                case LearnerAssignmentState.Active:
                    break;

                case LearnerAssignmentState.Completed:
                    // No need to collect
                    return;

                case LearnerAssignmentState.Final:
                    // No need to collect
                    return;

                default:
                    // New status added
                    break;
            }

            CompleteAssignment(session, saver);

            if (Assignment.EmailChanges)
            {
                saver.SendCollectEmail(User);
            }

            if (Assignment.IsNonELearning)
            {
                saver.UpdateDropBoxPermissions(LearnerAssignmentState.Completed, User);
            }
        }

        /// <summary>reactivates the assignment.</summary>
        public void Reactivate(AssignmentSaver saver)
        {
            CheckUserIsInstructor();

            // Check the status
            switch (Status)
            {
                case LearnerAssignmentState.NotStarted:
                    throw InvalidTransitionException(LearnerAssignmentState.NotStarted, LearnerAssignmentState.Active);

                case LearnerAssignmentState.Active:
                    return; // Already active

                case LearnerAssignmentState.Completed:
                    break;

                case LearnerAssignmentState.Final:
                    break;

                default:
                    // New status added
                    break;
            }

            LearnerAssignmentState newStatus = LearnerAssignmentState.Active;

            if (Assignment.IsELearning)
            {
                ReactivateSession();
            }
            else
            {
                saver.UpdateDropBoxPermissions(newStatus, User);
            }

            Save(newStatus, false, NonELearningStatus(AttemptStatus.Active), null, saver);
            Status = newStatus;

            if (Assignment.EmailChanges)
            {
                saver.SendReactivateEmail(User);
            }
        }

        /// <summary>Submits the assignment.</summary>
        public void Submit()
        {
            CheckUserIsLearner();

            // Check the status
            switch (Status)
            {
                case LearnerAssignmentState.NotStarted:
                    if (Assignment.IsELearning)
                    {
                        throw InvalidTransitionException(LearnerAssignmentState.NotStarted, LearnerAssignmentState.Completed);
                    }
                    break;

                case LearnerAssignmentState.Active:
                    break;

                case LearnerAssignmentState.Completed:
                    // Need to transition to Final if auto return assignment
                    if (Assignment.AutoReturn != true)
                    {
                        return;
                    }
                    else
                    {
                        break;
                    }

                case LearnerAssignmentState.Final:
                    // Already complete so leave
                    return;

                default:
                    // New status added
                    break;
            }

            CompleteAssignment(null, null);

            if (Assignment.IsNonELearning)
            {
                DropBoxManager dropBoxMgr = new DropBoxManager(Assignment);
                dropBoxMgr.ApplySubmittedPermissions();
            }

            if (Assignment.EmailChanges)
            {
                Assignment.SendSubmitEmail(LearnerName);
            }
        }

        /// <summary>Uploads files and submits the assignment.</summary>
        /// <param name="files">The files to upload.</param>
        /// <param name="existingFilesToKeep">Existing files to keep.</param>
        public void UploadFilesAndSubmit(AssignmentUpload[] files, int[] existingFilesToKeep)
        {
            DropBoxManager manager = new DropBoxManager(Assignment);
            manager.UploadFiles(files, existingFilesToKeep);
            Submit();
        }
#endregion public methods

#region private methods
        void CompleteAssignment(StoredLearningSession newSession, AssignmentSaver saver)
        {
            LearnerAssignmentState newStatus = LearnerAssignmentState.Completed;
            bool? isFinal = false;
            if (Assignment.AutoReturn)
            {
                newStatus = LearnerAssignmentState.Final;
                isFinal = true;
            }


            if (newSession == null)
            {
                float? finalPoints = null;
                if (Status == LearnerAssignmentState.Active && Assignment.IsELearning)
                {
                    finalPoints = FinishSession();
                }

                Save(newStatus, isFinal, NonELearningStatus(AttemptStatus.Completed), finalPoints, saver);
            }
            else
            {
                Save(newStatus, isFinal, NonELearningStatus(AttemptStatus.Completed), newSession.TotalPoints, saver);
            }

            Status = newStatus;
        }

        void CheckUserIsLearner()
        {
            if (Assignment.Store.CurrentUserId != LearnerId)
            {
                throw new SafeToDisplayException(SlkCulture.GetResources().SubmitAssignmentNotLearner);
            }
        }

        void CheckUserIsInstructor()
        {
            UserItemIdentifier current = Assignment.Store.CurrentUserId;

            foreach (SlkUser instructor in Assignment.Instructors)
            {
                if (instructor.UserId == current)
                {
                    return;
                }
            }

            throw new SafeToDisplayException(SlkCulture.GetResources().ChangeLearnerAssignmentNotInstructor);
        }

        StoredLearningSession CreateAttemptIfRequired(bool transitionToComplete)
        {
            // NotStarted --> Active or Completed or Final
            if (Assignment.IsELearning)
            {
                // create an attempt for this learner assignment
                StoredLearningSession learningSession = StoredLearningSession.CreateAttempt(Assignment.Store.PackageStore, LearnerId, LearnerAssignmentId, Assignment.RootActivityId, 
                        Assignment.Store.Settings.LoggingOptions);

                // start the assignment, forcing selection of a first activity
                learningSession.Start(true);

                // if NotStarted --> Completed or Final, transition to the Completed state
                if (transitionToComplete)
                {
                    // transition to Completed
                    learningSession.Exit();
                }

                // save changes to <learningSession>
                learningSession.CommitChanges();

                return learningSession;
            }
            else
            {
                return null;
            }
        }

        float? FinishSession()
        {
            if (AttemptId == null)
            {
                throw new InternalErrorException("SLK1007");
            }

            // set <learningSession> to refer to the attempt associated with this learner assignment
            StoredLearningSession learningSession = new StoredLearningSession(SessionView.Execute, AttemptId, Assignment.Store.PackageStore);

            // transition the attempt to "Completed" state; note that this will initialize the "content score", i.e. the score computed from the content
            if (learningSession.HasCurrentActivity)
            {
                // make sure that if the content wants to suspend itself, it does
                learningSession.ProcessNavigationRequests();    
            }

            learningSession.Exit();
            learningSession.CommitChanges();
            return learningSession.TotalPoints;
        }

        void ReactivateSession()
        {
            if (AttemptId == null)
            {
                throw new InternalErrorException("SLK1010");
            }

            StoredLearningSession learningSession = new StoredLearningSession(SessionView.RandomAccess, AttemptId, Assignment.Store.PackageStore);

            // reactivate the attempt
            learningSession.Reactivate(ReactivateSettings.ResetEvaluationPoints);
            learningSession.CommitChanges();

            // restart the attempt
            learningSession = new StoredLearningSession(SessionView.Execute, AttemptId, Assignment.Store.PackageStore);
            learningSession.Start(true);
            learningSession.CommitChanges();
            // NOTE: if (learningSession.AttemptStatus != AttemptStatus.Active) then the
            // restart process failed -- but there's not much we can do about it, and throwing
            // an exception may make matters worse
        }

        Exception InvalidTransitionException(LearnerAssignmentState oldStatus, LearnerAssignmentState newStatus)
        {
            SlkCulture culture = new SlkCulture();
            string message = culture.Format(culture.Resources.LearnerAssignmentTransitionNotSupported, oldStatus, newStatus);
            return new InvalidOperationException(message);
        }

        AttemptStatus? NonELearningStatus(AttemptStatus state)
        {
            if (Assignment.IsNonELearning)
            {
                return state;
            }
            else
            {
                return null;
            }
        }

        void Save(LearnerAssignmentState newStatus, bool? isFinal, AttemptStatus? nonELearningStatus, float? finalPoints, AssignmentSaver saver)
        {
            if (fullSave)
            {
                if (saver == null)
                {
                    throw new ArgumentNullException("saver");
                }

                bool ignoreFinalPoints = finalPoints != null ? false : IgnoreFinalPoints;
                float? pointsToSend = IgnoreFinalPoints == false ? FinalPoints : finalPoints;
                Assignment.Store.SaveLearnerAssignment(LearnerAssignmentId, ignoreFinalPoints, pointsToSend, InstructorComments, Grade, isFinal, nonELearningStatus, saver.CurrentJob);
            }
            else
            {
                bool saveFinalPoints = (finalPoints != null || newStatus == LearnerAssignmentState.Active);
                Assignment.Store.ChangeLearnerAssignmentState(LearnerAssignmentId, isFinal, nonELearningStatus, saveFinalPoints, finalPoints);
            }
        }
#endregion private methods
    }
}
