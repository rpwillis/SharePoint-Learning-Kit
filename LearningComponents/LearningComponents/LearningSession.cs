/* Copyright (c) Microsoft Corporation. All rights reserved. */


using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using Microsoft.LearningComponents.DataModel;
using System.Collections.Generic;
using System;
using Microsoft.LearningComponents;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;
using Microsoft.SharePointLearningKit.Localization;


namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Represents a user interaction with a SCORM package. Subclasses represent sessions that are 
    /// stored in LearningStore (StoredLearningSession), and those that are in-memory 
    /// only representations (UnstoredLearningSession). 
    /// </summary>
    /// <remarks>
    /// 
    /// <p/>TODO: Concepts to define
    /// <ol>
    /// <li>Exit</li>
    /// <li>Terminate</li>
    /// <li>Suspend</li>
    /// <li>Abandon</li>
    /// <li>Active session</li>
    /// <li>Current activity</li>
    /// <li>Root activity</li>
    /// <li>Session status (or Attempt status)</li>
    /// <li>Attempt -- Does this need to be defined?</li>
    /// <li>View</li>
    /// </ol>
    /// 
    /// <p/>A description of the views of LearningSession:
    /// <ol>
    /// <li>Execute view (SessionView.Execute): Represents a view of a learner interacting (attempting)
    /// a course. All navigation rules are processed in this view. 
    /// Committed data is stored in LearningStore. Data persists 
    /// between instances of LearningSession.</li>
    /// <li>Review view (SessionView.Review): Represents a view of a previously executed session, without 
    /// regard to navigation rules. Allows accessing any activity that contains a resource. The session
    /// does not need to be ended in order to review it. Data of the executed session was previously stored 
    /// in LearningStore and this view allows viewing it. It does not allow modifying any of the stored data. 
    /// </li>
    /// <li>Random access view (SessionView.RandomAccess): During random access, the application can 
    /// access any activity in any order, primarily for the purpose of reading and/or updating 
    /// the SCORM Data Model for that activity, without regard to SCORM Sequencing and Navigation rules. 
    /// Allows accessing any activity in the activity tree of the content being executed. The
    /// attempt does not have to be ended to view a session in this way.
    /// Activities without associated resources may be modified in this view.
    /// Committed data is stored in LearningStore. Data persists between instances of LearningSession.
    /// When changes are committed, RandomAccessView saves the data model changes, 
    /// not any global information.  It will also not save any changes made to 
    /// the CurrentActivity or other attempt level data.</li>
    /// </ol>
    /// </remarks>
    public abstract class LearningSession
    {
        private Navigator m_navigator;
        private SessionView m_view;
        
        /// <summary>
        /// Constructor to create the LearningSession. This is internal, as only MLC subclasses can create one.
        /// </summary>
        /// <param name="view">The view of the session, for example Review, Execute, RandomAccess.</param>
        /// <remarks>
        /// Constructor is internal. Don't allow classes outside of our product to create one.
        /// 
        /// Subclass constructors MUST assign a value to m_navigator.
        /// </remarks>
        internal LearningSession(SessionView view) 
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            m_view = view;
        }

        /// <summary>
        /// Returns false if the value is not a valid value for a SessionView.
        /// </summary>
        /// <param name="viewValue">A view to validate.</param>
        internal static bool ValidateSessionViewValue(SessionView viewValue)
        {
            return ((viewValue >= SessionView.Execute) && (viewValue <= SessionView.Review));
        }
        
        /// <summary>
        /// Gets the view of the current session.
        /// </summary>
        /// <remarks>See the class definition for the meaning of the views. </remarks>
        public SessionView View 
        { 
            get { return m_view; }
            internal set { m_view = value; } 
        }

        /// <summary>
        /// Saves any data of this session that has not been saved. You must call this 
        /// method to ensure changes are persisted in long-term storage.
        /// </summary>
        /// <remarks>
        /// <p/>Applications should always call this function 
        /// after performing any operation that changes the state of the session, such as 
        /// <Mth>MoveToNext</Mth>, <Mth>MoveToPrevious</Mth> or <Mth>Exit</Mth>.
        /// 
        /// <p/>If there are no changes to save, this method has no effect.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the state of the session and / or 
        /// its associated data model are in an inconsistent state.</exception>
        public virtual void CommitChanges()
        {
            Navigator.Save();
        }

        /// <summary>
        /// Gets a points value for the session. 
        /// </summary>
        /// <remarks>
        /// The value of this score is determined by the type of content that is being used in the 
        /// session. 
        /// <ul>
        /// <li>In SCORM 2004 content, this is the same value as the root activity primary objective's normalized measure. 
        /// As such, in this case, if the value is not null, it will be between -1 and 1 (inclusive).</li>
        /// <li>In SCORM 1.2 content, this is the sum of the raw score for the attempts on each activity in the session. 
        /// </li>
        /// <li>In LRM content, this is the sum of all LearningDataModel.EvaluationPoints for all activites in the session.</li>
        /// </ul>
        /// </remarks>
        public float? TotalPoints
        {
            get
            {
                return Navigator.TotalPoints;
            }
        }

        /// <summary>
        /// Gets success status for the session.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is only valid in SCORM 2004 packages.  Other package formats
        /// will always return SuccessStatus.Unknown.
        /// </para>
        /// </remarks>
        public SuccessStatus SuccessStatus
        {
            get
            {
                return Navigator.SuccessStatus;
            }
        }

        /// <summary>
        /// Gets or sets completion status for the session.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is only valid in SCORM 2004 packages.  Other package formats
        /// will always return CompletionStatus.Unknown.
        /// </para>
        /// </remarks>
        public CompletionStatus CompletionStatus
        {
            get
            {
                return Navigator.CompletionStatus;
            }
        }

        #region Session Ending

        /// <summary>
        /// Immediately exits the session
        /// without processing any pending navigation requests.
        /// </summary>
        /// <remarks>
        /// <p/>The attempt associated with this session must have started and 
        /// identified an activity for delivery prior to calling this 
        /// method.
        /// 
        /// <p/>This method does not save changes that result from exiting the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object
        /// to ensure changes are persisted.
        ///
        /// <p/>All of the activities of the session will be exited and 
        /// the session will end. 
        /// None of the postConditionRules for any activity in the session will be evaluated. 
        /// 
        /// <p/>No further navigation can be taken on the session. The caller cannot resume the session 
        /// it has exited. To allow resuming the session, use the <Mth>Suspend</Mth> method instead 
        /// of <Mth>Exit</Mth>.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if it is not valid to call this 
        /// method. Use <Prp>IsExitValid</Prp> to anticipate if this exception will be thrown.</exception>
        public virtual void Exit() 
        {
            if (!IsExitValid)
                throw new InvalidOperationException(Resources.LS_ExitNotValid);

            m_navigator.Navigate(NavigationCommand.ExitAll);
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which the 
        /// <Mth>Exit</Mth> method may be called.
        /// </summary>
        public virtual bool IsExitValid
        {
            get { return false; }
        }

        /// <summary>
        /// Temporarily suspends the the current session. It may be resumed at a later time.
        /// </summary>
        /// <remarks>
        /// <p/>This method does not save changes that result from suspending the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object to ensure changes are persisted.
        /// 
        /// <p/>It may not be valid to 
        /// call this method based on the state of the session. Call <Prp>IsSuspendValid</Prp> before 
        /// calling this method to determine
        /// if this method is likely to succeed.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session is not in the appropriate state to 
        /// be suspended. Use <Prp>IsSuspendValid</Prp> to anticipate if this exception will be thrown.</exception>
        public virtual void Suspend()
        {
            if (!IsSuspendValid)
                throw new InvalidOperationException(Resources.LS_SuspendNotValid);

            m_navigator.Navigate(NavigationCommand.SuspendAll);
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Suspend</Mth> method may be called.
        /// </summary>
        public virtual bool IsSuspendValid
        {
            get { return false; }
        }

        /// <summary>
        /// Resumes a suspended session. Once the session has been resumed, it is considered active.
        /// </summary>
        /// <remarks>
        /// <p/>This method does not save changes that result from resuming the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object to ensure changes are persisted.
        /// 
        /// <p/>The only time that resuming a session is valid is if the session has been previously 
        /// suspended. Call <Prp>IsResumeValid</Prp> before 
        /// calling this method to determine
        /// if the session is in a state which can be resumed.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session is not 
        /// in the appropriate state to 
        /// be resumed. Use <Prp>IsResumeValid</Prp> to anticipate if this exception will be thrown.</exception>
         public virtual void Resume()
        {
            if (!IsResumeValid)
                throw new InvalidOperationException(Resources.LS_ResumeNotValid);

            m_navigator.Navigate(NavigationCommand.ResumeAll);
        }

        /// <summary>
        /// Reactivate a session that has been completed or abandoned.
        /// </summary>
        /// <param name="settings">Settings to determine which values, if any, are cleared when the session is 
        /// reactivated.</param>
        /// <exception cref="InvalidOperationException">Thrown if the session is not in the appropriate 
        /// state to be reactivated. Also thrown if the current <Prp>View</Prp> is not RandomAccess.</exception>
        public abstract void Reactivate(ReactivateSettings settings); 

        /// <summary>
        /// Returns true if it the session is in a state where <Mth>Reactivate</Mth> may be called.
        /// </summary>
        public virtual bool IsReactivateValid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Resume</Mth> method may be called.
        /// </summary>
        public virtual bool IsResumeValid
        {
            get { return false;  }
        }

        /// <summary>
        /// Stops the session by abandoning all activities in the session.
        /// </summary>
        /// <remarks>
        /// <p/>This method does not save changes that result from abandoning the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object 
        /// to ensure changes are persisted. 
        /// 
        /// <p/>PostCondition rules are not run after abandoning the session.
		/// Any current value of <Prp>NavigationRequest.ExitMode</Prp> of
		/// <Prp>LearningDataModel.NavigationRequest</Prp> will be ignored.
        /// 
        /// <p/>A session that has been abandoned cannot be resumed at a later time.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session is in a state which
        /// cannot be abandoned. Use <Prp>IsAbandonValid</Prp> to anticipate if this 
        /// exception will be thrown.</exception>
        public virtual void Abandon()
        {
            if (!IsAbandonValid)
                throw new InvalidOperationException(Resources.LS_AbandonNotValid);

            m_navigator.Navigate(NavigationCommand.AbandonAll);
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Abandon</Mth> method may be called.
        /// </summary>
        public virtual bool IsAbandonValid
        {
            get { return false; } 
        }

        #endregion // session ending

        #region Package Information

        /// <summary>
        /// Reads a file from within the package associated with this session and writes it to an HttpResponse.
        /// </summary>
        /// <param name="context">The context within which to render the file.</param>
        /// <remarks>
        /// At a minimum, the caller must set the following properties in the <paramref name="context"/>:
        /// <ul>
        /// <li><c>RelativePath</c></li>
        /// <li><c>Response</c></li>
        /// </ul>
        /// 
        /// On return, the following values in <paramref name="context"/> may have been set by the session:
        /// <ul>
        /// <li><c>MimeType</c></li>
        /// <li><c>FormId</c>, if a default was used.</li>
        /// </ul>
        /// In addition, the content was rendered into the <c>Response</c> of <paramref name="context"/>.
        /// 
        /// <p>This method will not read and write content that is not contained within the package. For instance, 
        /// if the entry point for an activity is an absolute URL and the caller passes it as the 
        /// <c>context.RelativePath</c>, this method will not succeed. It also will not render the package manifest.</p>
        /// 
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is not provided.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <c>context.RelativePath</c> or one of 
        /// <c>Response</c>is not provided, or there is no current activity identitified in the session.
        /// Also thrown if an Execute session requests to render content when the attempt is not active.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file referenced in <c>context.RelativePath</c>
        /// cannot be found, or if the caller requests to render the package manifest.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the file refernced in <c>context.RelativePath</c>
        /// includes a directory that does not exist in the package.</exception>
        public abstract void Render(RenderContext context);
        
        /// <summary>
        /// Process the data returned from the client.
        /// </summary>
        /// <param name="formData">The collection of name/value pairs returned from a posted form.</param>
        /// <param name="files">Collection of valid files posted in the request.</param>
        public abstract void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile> files);

        /// <summary>
        /// Gets the table of contents information for the activity tree of the current session.
        /// </summary>
        /// <param name="evaluateSequencingRules">If true, the sequencing rules are evaluated against the current 
        /// state of the session to determine which activities are available for navigation.</param>
        /// <returns>The root node of the table of contents.</returns>
        /// <remarks>Each time <c>GetTableOfCOntents</c> is called, the table of contents 
        /// may be reloaded from its source. This may be a lengthly operation.
        /// <para>
        /// If <paramref name="evaluateSequencingRules"/> is true, the results may only be valid until any data is 
        /// changed in the data model of the session. 
        /// </para>
        /// </remarks>
        public TableOfContentsElement GetTableOfContents(bool evaluateSequencingRules) { return Navigator.LoadTableOfContents(evaluateSequencingRules); }

        /// <summary>
        /// Gets the table of contents information for the activity tree of the current session, without evaluating 
        /// the sequencing rules to determine if activities are accessible in the current state of the session.
        /// </summary>
        /// <returns>The root node of the table of contents.</returns>
        /// <remarks>Each time <c>GetTableOfCOntents</c> is called, the table of contents 
        /// may be reloaded from its source. This may be a lengthly operation.
        /// </remarks>
        public TableOfContentsElement GetTableOfContents() { return Navigator.LoadTableOfContents(false); }
         
        /// <summary>
        /// Gets the format of the package being processed during this session.
        /// </summary>
        public PackageFormat PackageFormat { get { return Navigator.PackageFormat; } }

        /// <summary>
        /// Gets the title of the session. This is the title of the root activity in the session, if provided, otherwise,
        /// a default title is returned.
        /// </summary>
        public string Title 
        { 
            get 
            {
                if (Navigator.RootActivity == null)
                    Navigator.LoadTableOfContents(false);

                return Navigator.RootActivity.Title; 
            } 
        }

        /// <summary>
        /// Gets a PackageReader for this session that can read the package 
        /// associated with the session.
        /// </summary>
        /// <returns>The PackageReader for this session.</returns>
        /// <remarks>
        /// This allows the subclasses to ensure the 
        /// package reader is ready to read package files.
        /// <para>The caller should ensure the returned PackageReader is disposed 
        /// in a timely manner.</para>
        /// </remarks>
        internal abstract PackageReader GetPackageReader();

        #endregion

        #region Show*Ui methods

        /// <summary>
        /// Returns true if the associated user interface element indicating 'continue to next 
        /// activity' should be shown.
        /// </summary>
        public virtual bool ShowNext 
        {
            get
            {
                // If there is no current activity, then the answer is always false. Otherwise, the activity
                // definition provides the response.
                if (Navigator.CurrentActivity == null)
                    return false;

                return !Navigator.CurrentActivity.HideContinueUI;
            }
        } 
        
        /// <summary>
        /// Gets an indication if the user interface element indicating 'return to previous activity' should be shown.
        /// </summary>
        /// <returns>Returns true if the user interface element should be shown.
        /// </returns>
        public virtual bool ShowPrevious
        {  
            get
            {
                // If there is no current activity, then the answer is always false. Otherwise, the activity
                // definition provides the response.
                if (Navigator.CurrentActivity == null)
                    return false;

                return !Navigator.CurrentActivity.HidePreviousUI;
            }
        } 
        
        /// <summary>
        /// Gets an indication if the user interface element indicating 'exit' should be shown. 
        /// </summary>
        /// <returns>Returns true if the user interface element indicating 'exit' should be shown. </returns>
        /// <remarks>If the exit option is visible, the application 
        /// determines the behavior when the exit action is triggered. It may opt to exit 
        /// the current activity or the session. 
        /// </remarks>
        public virtual bool ShowExit
        {
            get
            {
                if (Navigator.CurrentActivity == null)
                    return false;

                return !Navigator.CurrentActivity.HideExitUI;
            }
        }
        
        /// <summary>
        /// Gets an indication if the user interface element to abandon the session should be shown. 
        /// </summary>
        /// <returns>Returns true if the user interface element to abandon the session should be shown. </returns>
        /// <remarks>If the abandon option is visible, the application 
        /// determines the behavior when the abandon action is triggered. It may opt to abandon
        /// the current activity or the session. 
        /// </remarks>
        public virtual bool ShowAbandon
        {
            get
            {
                if (Navigator.CurrentActivity == null)
                    return false;

                return !Navigator.CurrentActivity.HideAbandonUI;
            }
        }

        /// <summary>
        /// Gets an indication if the user interface element to save the session should be shown. 
        /// </summary>
        /// <returns>Returns true if the user interface element to save the session should be shown.</returns>
        public virtual bool ShowSave
        {
            get
            {
                // In most views, the session cannot be saved. Subclasses will override.
                return false;
            }
        }

        #endregion  // Show*Ui methods

        #region Navigation Commands

        /// <summary>
        /// Moves to the next activity in the session. For UnstoredLearningSession, this is determined
        /// by a pre-order traversal of the activity tree and navigation rules as defined in the package 
        /// manifest are not enforced. For a session that represents an active attempt, this is determined 
        /// through sequencing rules of the package and the progress of the user through it.
        /// </summary>
        /// <remarks>This method requires a current activity in
        /// the session.
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed during this process.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session or if the session is not in a state that allows moving to the next activity. </exception>
        /// <exception cref="SequencingException">Thrown if a new activity could not be identified for delivery.</exception>
        public virtual void MoveToNext()
        {
            ThrowIfCurrentActivityIsNull();

            Navigator.Navigate(NavigationCommand.Continue);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the next activity is a valid operation.
        /// </summary>
        /// <returns>Returns true if <Mth>MoveToNext()</Mth> would result in a new active activity in the 
        /// session.</returns>
        /// <remarks>
        /// </remarks>
        public virtual bool IsMoveToNextValid() 
        {
            // A warning: This method must actually do the navigation to figure out if it will succeed. 
            // Don't call this from other LearningSession methods.

            return m_navigator.IsNavigationValid(NavigationCommand.Continue); 
        }
        
        /// <summary>
        /// Moves to the previous activity in the session. For UnstoredLearningSession or views of sessions
        /// that are not active, this is determined
        /// by an in order traversal of the activity tree and navigation rules as defined in the package 
        /// manifest are not enforced. For a session that represents an active attempt, this is determined 
        /// through sequencing rules of the package.
        /// </summary>
        /// <remarks>This method requires a current activity in
        /// the session.
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed during this process.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session or if the session is not in a state that allows moving to the previous activity.</exception>
        /// <exception cref="SequencingException">Thrown if a new activity could not be identified for delivery.</exception>
        public virtual void MoveToPrevious()
        {
            ThrowIfCurrentActivityIsNull();

            Navigator.Navigate(NavigationCommand.Previous);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the previous activity is a valid operation.
        /// </summary>
        /// <returns>Returns true if <Mth>MoveToPrevious()</Mth> would result in a new active activity in the 
        /// session.</returns>
        /// <remarks>
        /// </remarks>
        public virtual bool IsMoveToPreviousValid() 
        {
            // A warning: This method must actually do the navigation to figure out if it will succeed. 
            // Don't call this from other LearningSession methods.

            return m_navigator.IsNavigationValid(NavigationCommand.Previous); 
        }

        /// <summary>
        /// Makes the selected activity the current activity.
        /// </summary>
        /// <param name="activityKey">The id from the manifest corresponding to the activity
        /// to become the current activity.</param>
        /// <remarks>
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed during this process.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="activityKey"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="activityKey"/> is an empty string.</exception>
        /// <exception cref="SequencingException">Thrown if the <paramref name="activityKey"/>
        /// cannot become the current activity to restrictions in the sequencing rules of the package.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the current state of the session does not allow
        /// moving to the selected activity or if the activity does not exist in the package.</exception>
        public virtual void MoveToActivity(string activityKey)
        {
            if (activityKey == null)
                throw new ArgumentNullException("activityKey");
            if (String.IsNullOrEmpty(activityKey))
                throw new ArgumentOutOfRangeException("activityKey");

            Navigator.NavigateTo(activityKey);
        }

        /// <summary>
        /// Makes the selected activity the current activity, using the unique activity id. 
        /// </summary>
        /// <param name="activityId">The unique identifier for the selected activity.</param>
        /// <remarks>
        /// <p/>Whenever possible, use this method, which takes a long value instead of 
        /// the activity key, as it will improve performance.
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed during this process.
        /// </remarks>
        /// <exception cref="SequencingException">Thrown if the <paramref name="activityId"/>
        /// cannot become the current activity to restrictions in the sequencing rules of the package.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the current state of the session does not allow
        /// moving to the selected activity or if the activity does not exist in the package.</exception>
        public virtual void MoveToActivity(long activityId)
        {
            Navigator.NavigateTo(activityId);
        }


        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the selected activity is a 
        /// valid operation. The activity is identified by it's unique numerical id.
        /// </summary>
        /// <param name="activityId">The unique numeric identifier of the selected activity.</param>
        /// <returns>Returns true if moving to the selected activity would result in making that 
        /// activity become the current activity.</returns>
        public virtual bool IsMoveToActivityValid(long activityId)
        {
            return m_navigator.IsNavigationToValid(activityId);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the selected activity is a 
        /// valid operation.
        /// </summary>
        /// <param name="activityKey">The key that uniquely identifies the activity.</param>
        /// <returns>Returns true if moving to the selected activity would result in making that 
        /// activity become the current activity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="activityKey"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="activityKey"/> is an empty string.</exception>
        public virtual bool IsMoveToActivityValid(string activityKey) 
        {
            // A warning: This method must actually do the navigation to figure out if it will succeed. 
            // Don't call this from other LearningSession methods.


            if (activityKey == null)
                throw new ArgumentNullException("activityKey");
            if (String.IsNullOrEmpty(activityKey))
                throw new ArgumentOutOfRangeException("activityKey");

            return m_navigator.IsNavigationToValid(activityKey); 
        }

        #endregion

        #region CurrentActivity information and actions

        /// <summary>
        /// Determines if the session has a current activity. 
        /// </summary>
        public virtual bool HasCurrentActivity
        {
            get
            {
                return (Navigator.CurrentActivity != null);
            }
        }

        /// <summary>
        /// Gets a value which indicates whether the session has a current activity that is active.
        /// Returns false if there is no current activity in the session.
        /// </summary>
        public bool CurrentActivityIsActive
        {
            get
            {
                if (HasCurrentActivity)
                    return Navigator.CurrentActivity.DataModel.ActivityIsActive;

                return false;
            }
        }
           
        /// <summary>
        /// Gets the data model associated with the current activity in the session. 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <p/>Any changes made to the data model must be saved using the <Mth>CommitChanges</Mth> method.
        /// If <Mth>CommitChanges</Mth> is not called and the session is ended,
        /// changes to the data model will be lost.
        /// 
        /// <p/>Initially retrieving the data model
        /// could be a long-running task involving database or package file parsing. Subsequent property
        /// accesses are fast.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public virtual LearningDataModel CurrentActivityDataModel
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.DataModel;
            }
        }

        /// <summary>
        /// Gets the type of resource of the current activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public virtual ResourceType CurrentActivityResourceType
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.ResourceType;
            }
        }


        /// <summary>
        /// Gets the launching point of the current activity. This may be an absolute Uri, which indicates
        /// the resource is not in the package, or a package-relative Uri.
        /// </summary>
        public virtual Uri CurrentActivityEntryPoint
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return new Uri(Navigator.CurrentActivity.DefaultResourceFile, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Gets the xmlbase information for the resource of the currect activity. This uri will be the base uri for any references
        /// contained in that resource to other files in the package. The value may be a package-relative or absolute Uri.
        /// If the resource does not have an xmlbase value, this property returns null.
        /// </summary>
        /// <remarks>
        /// For instance, if a package contains a resource that has the following resources node:
        /// 
        /// &lt;resources xml:base="TestPackageContent/"&gt;
        /// &lt;resource identifier="base1_htm" href="Base1.htm" type="webcontent" adlcp:scormType="sco"&gt;
        ///     &lt;file href="Base1.htm"&gt;&lt;/file&gt;
        /// &lt;/resource&gt;
        /// &lt;/resources&gt;
        /// 
        /// then the return value of this property would be a uri containing the relative path "TestPackageContent/". This would 
        /// indicate that, for instance, the file Base1.htm exists in a package subdirectory named TestPackageContent.
        /// </remarks>
        public Uri CurrentActivityResourceXmlBase
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                string xmlBase = Navigator.CurrentActivity.ResourceXmlBase;
                if (String.IsNullOrEmpty(xmlBase))
                    return null; 

                return new Uri(xmlBase, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Processes all of the pending navigation requests in the current activity's 
        /// data model. 
        /// </summary>
        /// <returns>True if a navigation request was processed as a result of this call.</returns>
        /// <remarks>
        /// <p/>After this method, the state of the session and its current activity  
        /// is determined by the navigation commands (if any) 
        /// that were set in the LearningDataModel of the current activity 
        /// prior to calling this method. 
        /// If there is a navigation
        /// request to move to a different activity, or to exit the current activity, this method 
        /// will trigger that pending navigation request. 
        /// 
        /// <p/>Pending navigation requests for activities other than the current activity are 
        /// ignored.
        /// 
        /// <p/>The LearningSession current activity may change after 
        /// calling this method. It may have changed due to a pending navigation request in the 
        /// current activity's data model. It may have changed due to the postConditionRules associated
        /// with the activity in the package manifest.
        /// 
        /// <p/>If the activity is being paused, and should be left in a state where it can be resumed,
        /// then the caller should set the <Prp>NavigationRequest.ExitMode</Prp> value of
		/// <Prp>LearningDataModel.NavigationRequest</Prp>
        /// to Suspend before calling this method.
        /// 
        /// <p/>This method does not save the data model after processing these
        /// navigation requests, so it is recommended that the 
        /// application call <Mth>CommitChanges</Mth> after calling this method.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        /// <exception cref="SequencingException">Thrown if the state of the session, such as the 
        /// current sequencing rules, do not allow processing the 
        /// requested navigation.</exception>
        public virtual bool ProcessNavigationRequests()
        {
            ThrowIfCurrentActivityIsNull();
            ThrowIfCurrentActivityIsInactive();

            return Navigator.ProcessDataModelNavigation();
        }

        /// <summary>
        /// Immediately exits the current activity without processing any pending 
        /// navigation requests and without saving 
        /// any data model values that have not been committed.
        /// </summary>
        /// <remarks>
        /// <p/>It is recommended the application call <Mth>CommitChanges</Mth> after exiting the current 
        /// activity to ensure changes made by this method are saved.
        /// 
        /// <p/>If the caller wants to be able to resume the current activity at a future time, it should 
        /// set the <Prp>NavigationRequest.ExitMode</Prp> value of
		/// <Prp>LearningDataModel.NavigationRequest</Prp> to Suspend before 
        /// calling this method.
        /// 
        /// <p/>In SCORM 2004 Content: This method will process the postConditionRules for the current activity. This may result in another 
        /// activity becoming the current activity in the session. It may also result in not being able to identify 
        /// a current activity for the session.
        /// <p/>In SCORM 1.2 and LRM Content: This method will exit the current activity and make no effort to identify 
        /// another activity for delivery. 
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        /// <exception cref="SequencingException">Thrown if the state of the session, such as the 
        /// current sequencing rules, do not allow processing the 
        /// requested navigation.</exception>
        public virtual void ExitCurrentActivity()
        {
            ThrowIfCurrentActivityIsNull();

            Navigator.Navigate(NavigationCommand.UnqualifiedExit);
        }

        /// <summary>
        /// Stops the current activity by abandoning it without saving data and without further 
        /// processing of navigation rules.
        /// </summary>
        /// <remarks>
        /// 
        /// <p/>Pending data model changes and other session changes that result from this method
        /// are not saved and should be persisted to long-term storage by calling 
        /// <Mth>CommitChanges</Mth> after this method and prior to disposing the object.
        /// 
        /// <p/>In Scorm 2004 content: PostCondition rules are not run after the activity has been abandoned.
		/// Any current value of <Prp>NavigationRequest.ExitMode</Prp> of
		/// <Prp>LearningDataModel.NavigationRequest</Prp> will be ignored, and the 
        /// <Mth>AttemptStatus</Mth> of the attempt on the activity will be set to Abandonded.
        /// 
        /// <p/>In Scorm 2004 content: Once this method is called, the current activity will no longer be active for this session. That is,
        /// <Prp>CurrentActivityIsActive</Prp> will be false. To continue
        /// the session, select an activity to attempt 
        /// using one of the <Mth>MoveToActivity</Mth> methods.
        /// <p/>In Scorm 1.2 and LRM content: Once this method is called, there will no longer be a 
        /// current activity in the session.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this method is called when 
        /// there is no current activity in 
        /// the session.</exception>
        /// <exception cref="SequencingException">Thrown if the state of the session, such as the 
        /// current sequencing rules, do not allow processing the 
        /// requested navigation.</exception>
        public virtual void AbandonCurrentActivity()
        {
            ThrowIfCurrentActivityIsNull();

            Navigator.Navigate(NavigationCommand.Abandon);
        }

        /// <summary>
        /// Gets the identifier of the current activity, as provided in the manifest. This is unique per session.
        /// </summary>
        /// <remarks>
        /// <p/>When possible, use <Prp>CurrentActivityId</Prp> to refer to activities, as it will afford better 
        /// performance.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public string CurrentActivityKey
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.Key;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the current activity. It is unique for all activities in the 
        /// persisted storage location. 
        /// </summary>
        /// <remarks>
        /// <p/>When possible, use this activity identifier (rather than <Prp>CurrentActivityKey</Prp>) to access 
        /// activity information, as it will provide better performance.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public long CurrentActivityId
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.ActivityId;
            }
        }

        /// <summary>
        /// Gets the parameter string to provide to the resource in the activity when the resource is being 
        /// rendered. An empty string is returned if the value does not exist.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public string CurrentActivityResourceParameters
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.Parameters;
            }
        }

        /// <summary>
        /// Gets the identifier of the resource associated with the current activity. 
        /// The key is derived from the resource identifier in the manifest. It is unique per session.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public string CurrentActivityResourceKey
        {
            get
            {
                ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.ResourceKey;
            }
        }

        #endregion

        #region XrloHandler Processing

        /// <summary>
        /// Gets a stream containing the input stream to the RloHandler. This contains the 'raw' file that will be processed
        /// to provide the output stream.
        /// </summary>
        /// <returns>The stream with the content from the package written to it.</returns>
        internal Stream GetInputStream(string relativePath)
        {
            Uri tempUri;
            if (!Uri.TryCreate(relativePath, UriKind.Relative, out tempUri))
            {
                // The relativePath was not a valid relative path
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.DRLO_InvalidRelativePath, relativePath));
            }
            Stream retStream;
            using (PackageReader packageReader = GetPackageReader())
            {
                if (!packageReader.FileExists(relativePath))
                {
                    throw new FileNotFoundException(Resources.LS_ResourceNotInPackage);
                }

                retStream = packageReader.GetFileStream(relativePath);
            }
            return retStream;
        }

        #endregion

        #region internal helper Methods
        /// <summary>
        ///  Return the navigator for this session.
        /// </summary>
        /// <remarks>It may seem odd that this is internal, not protected. 
        /// Basically, since Navigator is an internal class, it cannot be returned
        /// from a property that is protected. The same reason is actually why this 
        /// property is here at all -- a protected member variable containing an 
        /// internal type is not allowed by the compiler.</remarks>
        internal Navigator Navigator { get { return m_navigator; } set { m_navigator = value; } }

        /// <summary>
        /// Private helper function to throw an InvalidOperationException if there is no current activity.
        /// </summary>
        internal void ThrowIfCurrentActivityIsNull()
        {
            if (!HasCurrentActivity)
                throw new InvalidOperationException(Resources.SLS_CurrentActivityRequired);
        }

        /// <summary>
        /// Private helper function to throw an InvalidOperationException if the current activity is not active.
        /// </summary>
        internal void ThrowIfCurrentActivityIsInactive()
        {
            if (!CurrentActivityIsActive)
                throw new InvalidOperationException(Resources.SLS_CurrentActiveActivityRequired);
        }

        /// <summary>
        /// Validates that the requested path can be rendered. Throws the appropriate exception if the 
        /// request is not allowed.
        /// </summary>
        /// <param name="relativePath">The package-relative path that is being requested to render.</param>
        /// <param name="parameterName">The name of the parameter to include in any error message</param>
        protected void ValidateRenderPath(string relativePath, string parameterName)
        {
            Utilities.ValidateParameterNonNull(parameterName, relativePath);
            Utilities.ValidateParameterNotEmpty(parameterName, relativePath);

            // Validating that context.RelativePath is really relative is done when the value is used.

            // Remove any trailing path characters
            string trimmedPath = relativePath.TrimEnd(new char[] { '/', '\\'});

            // Verify they are not requesting to render the manifest file
            if (String.Compare("imsmanifest.xml", trimmedPath, StringComparison.OrdinalIgnoreCase) == 0)
                throw new FileNotFoundException(Resources.LS_CantRenderManifest, relativePath);
            else
            {
                if ((PackageFormat == PackageFormat.Lrm)
                    && (String.Compare("Index.xml", trimmedPath, StringComparison.OrdinalIgnoreCase) == 0))
                    throw new FileNotFoundException(Resources.LS_CantRenderManifest, relativePath);
            }
        }
        #endregion
    }
    
    /// <summary>
    /// Exception thrown when invalid data is detected in posted data.
    /// </summary>
    [Serializable]
    public class InvalidFormDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidFormDataException</Typ> class.
        /// </summary>
        public InvalidFormDataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidFormDataException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidFormDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidFormDataException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidFormDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidFormDataException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InvalidFormDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// The view of a session. 
    /// </summary>
    public enum SessionView
	{
        /// <summary>
        /// The view to allow a learner to attempt a root activity in a package.
        /// </summary>
        Execute = 0,

        /// <summary>
        /// The view to access and edit every node in the activity tree associated with this
        /// session. 
        /// </summary>
        RandomAccess,

        /// <summary>
        /// A read-only view on a previously executed attempt.
        /// </summary>
        Review
	}

    /// <summary>
    /// Represents a context within which to render a session file.
    /// </summary>
    /// <remarks>
    /// This class provides minimal validation when properties are set on it. 
    /// An instance of the RenderContext class is passed to a LearningSession object when 
    /// <c>LearningSession.Render</c> is called. When the <c>Render</c> method is called, then the values
    /// in <c>RenderContext</c> are validated as required at the time the request is made to render and may 
    /// result in an exception at that time. 
    /// </remarks>
    public class RenderContext
    {
        private string m_relativePath;  // relative path to the requested file to render
        private Stream m_outputStream;  // the output stream to render to
        private HttpResponse m_response;
        private string m_mimeType;
        private Dictionary<string, string> m_mimeTypeMapping;
        private Dictionary<string, string> m_hiddenControls;    // list of name/value pairs for hidden controls to render
        private JScriptString m_script;     // JScript to render in onload handler
        private Uri m_resourcePath;         // path to resource to render
        private bool m_showCorrectAnswers;  // if true, show correct answers in some views
        private bool m_showReviewerInfo;    // if true, show information for reviewer
        private ICollection<string> m_iisCompatibilityModeExtensions;

        /// <summary>
        /// Constructor. Create a RenderContext in order to display content of a LearningSession.
        /// </summary>
        public RenderContext()
        {
            m_mimeTypeMapping = new Dictionary<string, string>(100, StringComparer.OrdinalIgnoreCase);
            m_hiddenControls = new Dictionary<string, string>(100, StringComparer.OrdinalIgnoreCase);
            m_iisCompatibilityModeExtensions = new List<string>();
        }

        /// <summary>
        /// Constructor. Create a RenderContext with the minimum information required. 
        /// </summary>
        /// <param name="relativePath">The relative path to the content to be rendered.</param>
        internal RenderContext(string relativePath) : this()
        {
            Utilities.ValidateParameterNonNull("relativePath", relativePath);
            Utilities.ValidateParameterNotEmpty("relativePath", relativePath);
            
            RelativePath = relativePath;            
        }

        /// <summary>
        /// Constructor. Create a RenderContext that renders session content to an HTTP response.
        /// </summary>
        /// <param name="relativePath">The path relative to the root of the session package of the content to be rendered.</param>
        /// <param name="response">The response to which the content information will be written.</param>
        public RenderContext(string relativePath, HttpResponse response) : this()
        {
            Utilities.ValidateParameterNonNull("relativePath", relativePath);
            Utilities.ValidateParameterNotEmpty("relativePath", relativePath);
            Utilities.ValidateParameterNonNull("response", response);

            RelativePath = relativePath;
            Response = response;
        }


        /// <summary>
        /// Gets and sets the package-relative path of the requested file to render. 
        /// </summary>
        /// <remarks>
        /// The value may not be set to null or an empty string.
        /// <p>
        /// This value must be relative to either:
        /// <ol>
        /// <li>The root of the package.</li>
        /// <li>The <Prp>EmbeddedUIResourcePath</Prp> for dynamically rendered content.</li>
        /// </ol>
        /// the <Prp>EmbeddedUIResourcePath</Prp>.
        /// </p>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri")]
        public string RelativePath
        {
            get { return m_relativePath; }
            set
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                ValidatePropertyNotEmpty(value);

                try
                {
                    // Verify path is relative. This causes an fxcop warning because the value is never used, however this is 
                    // the best way to ensure the uri has correct format, so warning is suppressed.
                    new Uri(value, UriKind.Relative);
                }
                catch (UriFormatException)
                {
                    throw new InvalidOperationException(Resources.LS_ResourceNotInPackage);
                }

                m_relativePath = value;
            }
        }

        /// <summary>
        /// Gets and sets the path to the folder containing resources for UI elements contained in the 
        /// dynamically generated content. This is not a path to a file within the content, but rather a path outside the package
        /// to resources that are part of the Microsoft Learning Components installation and 
        /// required to render dynamic views of the content such as LRM content.
        /// </summary>
        /// <remarks>
        /// <p>This property is ignored by <c>LearningSession.Render</c> unless <c>LearningSession.CurrentActivityResourceType</c> is 
        /// ResourceType.Lrm.</p>
        /// <p>The value may not be set to null.</p>
        /// </remarks>
        public Uri EmbeddedUIResourcePath
        {
            get { return m_resourcePath; }
            set 
            {
                ValidatePropertyNotNull(value);
                m_resourcePath = value;
            }
        }

        /// <summary>
        /// Gets and sets the stream to when a file will be rendered. This does not need to be 
        /// set if <Prp>Response</Prp> has been set.
        /// </summary>
        /// <remarks>
        /// <p>The value may not be set to null.</p>
        /// </remarks>
        internal Stream OutputStream
        {
            get {
                if (m_outputStream == null) 
                {
                    if (m_response == null)
                        return null;

                    return m_response.OutputStream;
                }
                else
                {
                    return m_outputStream; 
                }
            }
            set
            {
                ValidatePropertyNotNull(value);
                m_outputStream = value;
            }
        }

        /// <summary>
        /// Gets the response associated with this context.
        /// </summary>
        public HttpResponse Response
        {
            get { return m_response; }
            internal set
            {
                ValidatePropertyNotNull(value);
                m_response = value;
            }
        }

        /// <summary>
        /// Gets the mime type of the content written to the output stream.
        /// </summary>
        /// <remarks>The value is determined by the <Prp>MimeTypeMapping</Prp>. If that collection does 
        /// not provide an appropriate mapping, a default is returned.
        /// </remarks>
        public string MimeType
        {
            get { return m_mimeType; }

            // This value is set by rlo handler, if there is one.
            internal set
            {
                ValidatePropertyNotEmpty(value);
                m_mimeType = value;
            }
        }
        

        /// <summary>
        /// Gets the mapping between file name extension and mime type. The key is the file name extension 
        /// (including the initial period), 
        /// the value is the mime type mapped to that extension.
        /// </summary>
        /// <remarks>
        /// <p>Entries in the collection are culture-sensitive and case-insensitive.</p>
        /// </remarks>
        public IDictionary<string, string> MimeTypeMapping
        {
            get { return m_mimeTypeMapping; }
        }

        /// <summary>
        /// The collection of extensions for which files will be transmitted in a way that is most 
        /// compatible with the method used in IIS. This is slower than the normal process of rendering 
        /// files, but can be used for cases where specific file types do not render correctly.
        /// </summary>
        /// <remarks>Entries in this list should contain the period prior to the extension. E.g., ".txt". </remarks>
        public ICollection<string> IisCompatibilityModeExtensions
        {
            get { return m_iisCompatibilityModeExtensions; }
        }

        /// <summary>
        /// Gets a list of hidden controls to be added in the rendered form. The key in the collection is the 
        /// control id, the value is the value of the control. If a form is not rendered, this value is ignored.
        /// </summary>
        /// <remarks>
        /// The key and value in this collection should be provided in plain text. That is, they should not be html encoded.
        /// <p>Entries in the collection are culture-sensitive and case-sensitive.</p>
        /// </remarks>
        public IDictionary<string, string> FormHiddenControls
        {
            get { return m_hiddenControls; }
        }

        /// <summary>
        /// Gets or sets Represents JScript source code, i.e. a series of Unicode characters
        /// intended to be written into a &lt;script&gt;...&lt;/script&gt; block of
        /// a rendered HTML file. The script will be placed in such a way that it is run immediately following the page load.
        /// </summary>
        /// <remarks>
        /// 
        /// This script will only be added to pages rendered dynamically as LRM content. Pages that are contained within packages that 
        /// do not have <c>LearningSession.CurrentActivityResourceType</c> of <c>ResourceType.Lrm</c> will ignore this value.
        /// 
        /// <p>The value may not be set to null or an empty string.</p>
        /// <p>If the value has not been initialized, null is returned when the property get accessor is called.</p>
        /// </remarks>
        public string Script
        {
            get { return (m_script == null ) ? null : m_script.ToString(); }
            set
            {
                ValidatePropertyNotEmpty(value);
                m_script = new JScriptString(value);
            }
        }

        /// <summary>
        /// Get and sets a value indicating whether correct answers should be 
        /// displayed in the content, if possible. Regardless of this value, answers may only be shown if 
        /// it is supported in the current activity format for the current <c>LearningSession.View</c>.
        /// If not set, answers will not be shown. 
        /// </summary>
        /// <remarks>
        /// This setting only affects content which is dynamically rendered, such as Lrm content, and only
        /// in Review and Grading views. In other views and content formats, the setting is ignored.
        /// </remarks>
        public bool ShowCorrectAnswers
        {
            get { return m_showCorrectAnswers; }
            set { m_showCorrectAnswers = value; }
        }

        /// <summary>
        /// Gets and sets a value indicating whether sections of the content intended for a reviewer of a session
        /// (and not the learner)
        /// should be displayed in the rendered content. 
        /// If not set, reviewer information will not be shown. 
        /// </summary>
        /// <remarks>
        /// This setting only affects content which is dynamically rendered, such as Lrm content, and only
        /// in Review and Grading views. In other views and content formats, the setting is ignored.
        /// </remarks>
        public bool ShowReviewerInformation
        {
            get { return m_showReviewerInfo; }
            set { m_showReviewerInfo = value; }
        }

        /// <summary>
        /// Throws approrpriate exception if trimmed string is null or empty.
        /// </summary>
        private static void ValidatePropertyNotEmpty(string propertyValue)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if (propertyValue == null)
                throw new ArgumentNullException("value", Resources.LS_StringCannotBeEmpty);
            if (String.IsNullOrEmpty(propertyValue.Trim()))
                throw new ArgumentException(Resources.LS_StringCannotBeEmpty, "value");
        }

        /// <summary>
        /// Throws approrpriate exception if value is null.
        /// </summary>
        private static void ValidatePropertyNotNull(object propertyValue)
        {
            Resources.Culture = LocalizationManager.GetCurrentCulture();
            if (propertyValue == null)
                throw new ArgumentNullException("value", Resources.LS_PropertyCannotBeNull);
        }
    }

    /// <summary>
    /// Settings to determine behavior of the session when it is reactivated.
    /// </summary>
    [Flags]
    public enum ReactivateSettings
    {
        /// <summary>
        /// Reactivate the session without modifying <c>LearningDataModel</c> values.
        /// </summary>
        None = 0,

        /// <summary>
        /// When the session is reactivated, reinitialize the <c>LearningDataModel</c> Interaction
        /// Evaluation points.
        /// </summary>
        ResetEvaluationPoints = 1,

        /// <summary>
        /// When the session is reactivated, reinitialize the <c>LearningDataModel</c> Interaction
        /// Evaluation comments.
        /// </summary>
        ResetEvaluationComments = 2,

        /// <summary>
        /// When the session is reactivated, reinitialize all the <c>LearningDataModel</c> Interaction
        /// information.
        /// </summary>
        ResetAll = (ResetEvaluationComments | ResetEvaluationPoints)
    }

}
