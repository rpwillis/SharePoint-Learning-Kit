/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.Collections.Specialized;
using System.IO;
using Microsoft.LearningComponents.DataModel;
using Microsoft.LearningComponents.Storage;
using System.Transactions;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using System.Globalization;
using System.Web;
using System.Threading;

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a learning session that persists information in LearningStore. 
    /// </summary>
    public class StoredLearningSession : LearningSession
    {
        private PackageStore m_packageStore;
        private DatabaseNavigator m_dbNav;
        private RloHandler m_rloHandler;

        /// <summary>
        /// Create a StoredLearningSession using an existing Navigator object. This constructor
        /// does minimal work, it simply stores values.
        /// </summary>
        /// <param name="nav">The navigator for the session.</param>
        /// <param name="packageStore">The package to read from.</param>
        private StoredLearningSession(ExecuteNavigator nav, PackageStore packageStore) : base(SessionView.Execute)
        {
            SessionResources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("packageStore", packageStore);

            m_packageStore = packageStore;
            View = SessionView.Execute;

            Navigator = nav;
            m_dbNav = Navigator as DatabaseNavigator;
        }

        /// <summary>
        /// Access a LearningSession that is based on a user's attempt of the the content. The attempt must 
        /// already have been started (<Mth>Start</Mth>) in order to use this constructor.
        /// </summary>
        /// <param name="view">The view which will be shown of the attempt. Valid values are: Execute, 
        /// Review and RandomAccess. 
        /// </param>
        /// <param name="attemptId">The LearningStore identifier of the attempt for the session.</param>
        /// <param name="packageStore">The package store that contains the package files of the 
        /// package related to the attempt.</param>
        /// <remarks>
        /// <p/>If an <paramref name="attemptId"/> is passed to this constructor that does not represent a 
        /// valid attempt, this method may not throw an exception. Instead, the first time the attempt is 
        /// accessed in LearningStore, that method or property will throw the exception.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="view"/> does not 
        /// represent a valid view for the session, of if <paramref name="attemptId"/> is not a positive value.</exception>
        /// <exception cref="ArgumentNullException">Thrown if a <paramref name="packageStore"/> or 
        /// <paramref name="attemptId"/> is not provided.</exception>
        public StoredLearningSession(SessionView view, AttemptItemIdentifier attemptId, PackageStore packageStore) : base(view)
        {
            SessionResources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("packageStore", packageStore);
            Utilities.ValidateParameterNonNull("attemptId", attemptId);

            long attemptKey = attemptId.GetKey();

            // view is checked below
            if (attemptKey <= 0)
                throw new ArgumentOutOfRangeException("attemptId");
            
            m_packageStore = packageStore;
            
            // assign Navigator based on the view:
            switch (view)
            {
               case SessionView.Execute:
                {
                    Navigator = new ExecuteNavigator(m_packageStore.LearningStore, attemptKey);
                   break;
                }
               case SessionView.Review:
               {
                   Navigator = new ReviewNavigator(m_packageStore.LearningStore, attemptKey);
                   break;
                }
               case (SessionView.RandomAccess):
               {
                   Navigator = new RandomAccessNavigator(m_packageStore.LearningStore, attemptKey);
                   break;
               }
               default:
                    throw new ArgumentOutOfRangeException("view");
            }
            m_dbNav = Navigator as DatabaseNavigator;
        }

        /// <summary>
        /// Create an attempt on a specific package. The package must have already been 
        /// added to LearningStore. Following this method, <Mth>Start</Mth> 
        /// or <Mth>MoveToActivity</Mth> must be called in order to 
        /// deliver the first activity.
        /// </summary>
        /// <param name="packageStore">The store which contains files from the package associated
        /// with the attempt.</param>
        /// <param name="learnerId">The learner who is starting the attempt.</param>
        /// <param name="rootActivityId">The LearningStore id of the organization (ie, root activity) 
        /// to be attempted.
        /// </param>
        /// <param name="loggingOptions">Value to indicate the level of logging requested
        /// during an attempt.
        /// </param>
        /// <returns>Returns a session representing the new attempt. The <c>AttemptStatus</c>
        /// of the session will be Active. 
        /// </returns>
        /// <remarks>
        /// This method adds attempt information to LearningStore. 
        /// <p/>After creating the attempt, the caller must call <Mth>Start</Mth> 
        /// or <Mth>MoveToActivity</Mth> to identify the first
        /// activity for delivery.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when 
        /// <paramref name="packageStore"/>, <paramref name="learnerId"/> or <paramref name="rootActivityId"/> parameters
        /// are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="loggingOptions"/>
        /// does not contain a valid value.</exception>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the <paramref name="learnerId"/> or 
        /// <paramref name="rootActivityId"/> do not represent valid objects in LearningStore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]    // Restricted type is better type-safety
        public static StoredLearningSession CreateAttempt(PackageStore packageStore,
                    UserItemIdentifier learnerId,
                    ActivityPackageItemIdentifier rootActivityId,
                    LoggingOptions loggingOptions)
        {
            return CreateAttempt(packageStore, learnerId, null, rootActivityId, loggingOptions);
        }

        /// <summary>
        /// Create an attempt on a specific package. The package must have already been 
        /// added to LearningStore. Following this method, <Mth>Start</Mth> 
        /// or <Mth>MoveToActivity</Mth> must be called in order to 
        /// deliver the first activity.
        /// </summary>
        /// <param name="packageStore">The store which contains files from the package associated
        /// with the attempt.</param>
        /// <param name="learnerId">The learner assignment. Only used in SLK.</param>
        /// <param name="learnerAssignmentId">The id of the learner assignment.</param>
        /// <param name="rootActivityId">The LearningStore id of the organization (ie, root activity) 
        /// to be attempted.
        /// </param>
        /// <param name="loggingOptions">Value to indicate the level of logging requested
        /// during an attempt.
        /// </param>
        /// <returns>Returns a session representing the new attempt. The <c>AttemptStatus</c>
        /// of the session will be Active. 
        /// </returns>
        /// <remarks>
        /// This method adds attempt information to LearningStore. 
        /// <p/>After creating the attempt, the caller must call <Mth>Start</Mth> 
        /// or <Mth>MoveToActivity</Mth> to identify the first
        /// activity for delivery.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when 
        /// <paramref name="packageStore"/>, <paramref name="learnerId"/> or <paramref name="rootActivityId"/> parameters
        /// are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="loggingOptions"/>
        /// does not contain a valid value.</exception>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the <paramref name="learnerId"/> or 
        /// <paramref name="rootActivityId"/> do not represent valid objects in LearningStore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]    // Restricted type is better type-safety
        public static StoredLearningSession CreateAttempt(PackageStore packageStore,
                    UserItemIdentifier learnerId,
                    LearningStoreItemIdentifier learnerAssignmentId,
                    ActivityPackageItemIdentifier rootActivityId,
                    LoggingOptions loggingOptions)
        {
            if (packageStore == null)
                throw new ArgumentNullException("packageStore");
            if (!ValidateLoggingFlagValue(loggingOptions))
                throw new ArgumentOutOfRangeException("loggingOptions");
            Utilities.ValidateParameterNonNull("learnerId", learnerId);
            Utilities.ValidateParameterNonNull("rootActivityId", rootActivityId);

            // Create the attempt in LearningStore
            long learnerAssignmentKey = learnerAssignmentId == null ? 0 : learnerAssignmentId.GetKey();
            ExecuteNavigator nav = ExecuteNavigator.CreateExecuteNavigator(packageStore.LearningStore, rootActivityId.GetKey(), learnerId.GetKey(), learnerAssignmentKey, loggingOptions);
            
            // At this point, the attempt information is in the LearningStore. Attempt has not started.
            return new StoredLearningSession(nav, packageStore);
        }

        /// <summary>
        /// Create an attempt on a specific package. The package must have already been 
        /// added to LearningStore. Following this method, <Mth>Start</Mth> must be called in order to 
        /// deliver the first activity. This method does not provide logging information for the session.
        /// </summary>
        /// <param name="packageStore">The store which contains files from the package associated
        /// with the attempt.</param>
        /// <param name="learnerId">The learner who is starting the attempt.</param>
        /// <param name="rootActivityId">The LearningStore id of the organization (ie, root activity) 
        /// to be attempted.
        /// </param>
        /// <returns>Returns a session representing the new attempt. The <c>AttemptStatus</c>
        /// of the session will be Active. 
        /// </returns>
        /// <remarks>
        /// This method adds attempt information to LearningStore. 
        /// <p/>After creating the attempt, the caller must call <Mth>Start</Mth> to identify the first
        /// activity for delivery.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when 
        /// <paramref name="packageStore"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="learnerId"/> or 
        /// <paramref name="rootActivityId"/> do not represent valid objects.</exception>
        public static StoredLearningSession CreateAttempt(PackageStore packageStore,
                    UserItemIdentifier learnerId,
                    ActivityPackageItemIdentifier rootActivityId)
        {
            return CreateAttempt(packageStore, learnerId, rootActivityId, LoggingOptions.None);
        }

        /// <summary>
        /// Start the attempt associated with this session without taking special action to identify 
        /// an activity for delivery.
        /// </summary>
        /// <remarks>
        /// In order to navigate within a session, the attempt must be started. An attempt is started when the 
        /// first activity is identified for delivery.
        /// 
        /// <p/>This method does not save the state of the session. The application should call 
        /// <Mth>CommitChanges</Mth> after calling this method and before disposing the session object
        /// to save the session state in long-term storage.
        /// 
        /// <p/>This method only uses sequencing rules to determine the first activity. 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the attempt is not active (that
        /// is, <Prp>AttemptStatus</Prp> is not AttemptStatus.Active, or if the attempt has 
        /// already started.</exception>
        public void Start()
        {
            this.Start(false);
        }

        /// <summary>
        /// Start the attempt associated with this session.
        /// </summary>
        /// <param name="ensureInitialActivity">If true, this method will attempt to select an initial activity 
        /// for delivery even if the default sequencing rules in the package do not select one. 
        /// If false, only the sequencing rules within the package will be used to attempt to select an 
        /// initial activity for the session. If this does not result 
        /// in identifying an activity for delivery, the application will need to select one using the 
        /// <Mth>MoveToActivity</Mth> method in order to start the session.</param>
        /// <remarks>
        /// In order to navigate within a session, the attempt must be started. An attempt is started when the 
        /// first activity is identified for delivery.
        /// 
        /// <p/>This method does not save the state of the session. The application should call 
        /// <Mth>CommitChanges</Mth> after calling this method and before disposing the session object
        /// to save the session state in long-term storage.
        /// 
        /// 
        /// <p/>If the application passes <paramref name="ensureInitialActivity"/> as true, indicating 
        /// the session will attempt to determine the first activity even if the sequencing rules do 
        /// not identify one, and an 
        /// initial activity cannot be identified, then the package cannot be attempted since there is 
        /// no way to identify an initial activity.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the attempt is not active (that
        /// is, <Prp>AttemptStatus</Prp> is not AttemptStatus.Active, or if the attempt has 
        /// already started.</exception>
        /// <exception cref="InvalidPackageException">Thrown if <p>ensureInitialActivity</p> is true and 
        /// an activity cannot be identified for delivery. This indicates the package cannot be attempted.</exception>
        /// <exception cref="SequencingException">Thrown if the application passes <paramref name="ensureInitialActivity"/> 
        /// as false, indicating only the sequencing rules should be used to determine the first activity, and an 
        /// initial activity cannot be identified. In this case, the attempt has not started and 
        /// the application will need to select an activity and start the attempt using the 
        /// <Mth>MoveToActivity</Mth> method.</exception>
        public void Start(bool ensureInitialActivity)
        {
            if (View == SessionView.Execute)
            {
                if (AttemptHasStarted)
                    throw new InvalidOperationException(SessionResources.SLS_AttemptHasStarted);

                if (AttemptStatus != AttemptStatus.Active)
                    throw new InvalidOperationException(SessionResources.SLS_StartNotValid);
            }
            else
            {
                // never valid in other views
                throw new InvalidOperationException(SessionResources.SLS_StartNotValid);
            }
            
            // If it is possible, then start the attempt. If it's not possible, 
            // it could be that the package manifest does not have any sequencing information. 
            // In that case, if the application has requested (using ensureInitialActivity) to identify
            // an activity to deliver, then do so.
            try
            {
                DatabaseNavigator.Navigate(NavigationCommand.Start);
            }
            catch (SequencingException)
            {
                if (ensureInitialActivity)
                {
                    // Attempt to choose an activity
                    try
                    {
                        DatabaseNavigator.Navigate(NavigationCommand.ChoiceStart);
                    }
                    catch (SequencingException)
                    {
                        // If even the ChoiceStart option did not find an activity, there's something wrong with the package.
                        throw new InvalidPackageException(SessionResources.SLS_CouldNotFindFirstActivity);
                    }
                }
                else
                {
                    // Don't attempt to choose an activity. Rethrow exception.
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets an indication if the attempt has already started. An attempt that has started 
        /// may be ended. 
        /// </summary>
        private bool AttemptHasStarted
        {
            get { return HasCurrentActivity; }
        }

        /// <summary>
        /// Saves any changes in the session that have occurred since the session was created or the 
        /// session was previously committed. You must call this 
        /// method before disposing the session to ensure session changes to the session are persisted to LearningStore.
        /// </summary>
        /// <remarks>
        /// <p/>Applications should always call this function after exiting the session or
        /// performing any operation that changes the state of the session, such as 
        /// <Mth>MoveNext()</Mth>, <Mth>MovePrevious()</Mth> or setting any values in the <Prp>CurrentActivityDataModel</Prp>.
        /// 
        /// <p/>This method commits session changes to LearningStore. As such, it is a relatively slow operation. 
        /// Therefore, it is recommended that applications use this StoredLearningSession object to modify as much as 
        /// possible before calling CommitChanges. At that point, all previous changes will be saved.
        /// 
        /// For example:
        /// <code>
        /// LearningStore learningStore;    // this should be properly initialized
        /// FileSystemPackageStore packageStore;    // this should be properly initialized
        /// PackageItemIdentifier packageId;    // this should be properly initialized 
        /// 
        /// // Load an existing session.
        /// StoredLearningSession session = new StoredLearningSession( learningStore, SessionView.Execute, attemptId, packageStore);
        /// if (session.AttemptStatus == AttemptStatus.Suspended)
        /// {
        ///     session.Resume();
        /// }
        /// 
        /// // Change a data model element 
        /// session.CurrentActivityDataModel.Score.Raw = 95;
        /// 
        /// // Navigate to the next activity in the session
        /// session.MoveToNext();
        /// 
        /// // Save all previous changes
        /// session.CommitChanges();
        /// </code>
        /// 
        /// By committing the changes after all the state has been changed, including resuming the session, changing a data model value
        /// and then moving to a new activity, the code optimizes performance by only writing to LearningStore once. While it is valid to 
        /// call CommitChanges at any time, for performance reasons it is recommended to call it when all operations on the session 
        /// object have been completed. 
        /// 
        /// <p/>If there have been no changes to the session, this method has no effect.
        /// 
        /// <p/>It is not valid to call this method in Review view.
        /// 
        /// <p/>In RandomAccess view, data model changes are saved, but
        /// the results of navigation changes are not. In that case,
        /// subsequent instantiations of the StoredLearningSession will reload the attempt data
        /// without showing the effect of navigations performed in RandomAccess view.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the state of the session and / or 
        /// its associated data model are in an inconsistent state. For instance, if a stream provided as an 
        /// attachment to the data model exceeds the size allowed by the <Prp>MaxExtensionDataSize</Prp> of
        /// this session, this exception will be thrown.
        /// </exception>
        public override void CommitChanges()
        {
            if (View == SessionView.Review)
                throw new InvalidOperationException(SessionResources.SLS_CommitChangesNotValid);

            Navigator.Save();
        }

        /// <summary>
        /// Delete an existing attempt. This action permanently removes all data relating to this attempt from
        /// LearningStore. There is no way to recover deleted information.
        /// </summary>
        /// <param name="learningStore">The learningStore containing the attempt to be deleted.</param>
        /// <param name="attemptId">The unique identifier of the attempt to be deleted.</param>
        /// <remarks>
        /// The method uses <c>LearningStore.CreateJob</c> and <c>LearningStore.Execute</c> methods to 
        /// delete the attempt. Any exceptions thrown from those methods may be thrown from this method as well.
        /// <p/>If there isn't a current transaction when this method is called then a new 
        /// transaction is created and used. If the job succeeds, the transaction is committed. If the 
        /// job fails, the transaction is rolled back. If a SQL deadlock occurs when executing the job 
        /// and the job is chosen as the deadlock victim, the job is retried several times.
        /// </remarks>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the <paramref name="attemptId"/>
        /// does not represent a attempt.</exception>
        /// <exception cref="ArgumentNullException">Thrown if learningStore is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]    // Using AttemptItemIdentifier is tighter type-safety
        public static void DeleteAttempt(LearningStore learningStore, AttemptItemIdentifier attemptId)
        {
            if (learningStore == null)
                throw new ArgumentNullException("learningStore");

            LearningStoreJob job = learningStore.CreateJob();
            Dictionary<string,object> parameters = new Dictionary<string,object>();
            parameters[Schema.DeleteAttemptRight.AttemptId] = attemptId;
            job.DemandRight(Schema.DeleteAttemptRight.RightName, parameters);
            job.DisableFollowingSecurityChecks();
            job.DeleteItem(attemptId);
        
            // Does not create a transaction because LearningStore will create one for us. 
            job.Execute();
        }

        /// <summary>
        /// Gets or sets the level of logging in this session. In order to log all sequencing operations,
        /// this value should be set before any session requests related to 
        /// navigating the package, such as MoveNext or IsMoveToActivityValid.
        /// </summary>
        /// <remarks>
        /// It is not valid to set this value when <Prp>Microsoft.LearningComponents.LearningSession.View</Prp> is <c>SessionView.Review.</c>. 
        /// In RandomAccess view, changing this value has no effect, as sequencing rules are ignored
        /// in this mode.
        /// <p/>This value of this property is saved to LearningStore on the next call to 
        /// <Mth>CommitChanges</Mth>, before disposing the session object.
        /// <p/>By default, no logging is done in the session.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when caller attempts to set 
        /// an invalid value for the enumeration.</exception>
        /// <exception cref="InvalidOperationException">Thrown when callers attempts to set the value
        /// when <c>View</c> is <c>Review</c>.</exception>
        public LoggingOptions LoggingOptions
        { 
            get 
            {
                return DatabaseNavigator.LoggingFlags;
            } 
            set 
            {
                if (View == SessionView.Review)
                    throw new InvalidOperationException(SessionResources.SLS_LoggingFlagsSetNotValid);

                // If the value isn't a valid value, throw
                if (!ValidateLoggingFlagValue(value))
                    throw new ArgumentOutOfRangeException("value");  
  
                DatabaseNavigator.LoggingFlags = value;
            } 
        }

        /// <summary>
        /// Returns false if the value is not a valid value for a LoggingFlag.
        /// </summary>
        /// <param name="flagValue">A logging flag value to validate.</param>
        private static bool ValidateLoggingFlagValue(LoggingOptions flagValue)
        {
            if ((flagValue < LoggingOptions.None) || (flagValue > LoggingOptions.LogAll))
                return false;

            return true;
        }

        /// <summary>
        /// Returns false if the value is not a valid value for a ReactivateSetting.
        /// </summary>
        /// <param name="settingsValue">A value to validate.</param>
        private static bool ValidateReactivateSettingValue(ReactivateSettings settingsValue)
        {
            if ((settingsValue < ReactivateSettings.None) || (settingsValue > ReactivateSettings.ResetAll))
                return false;

            return true;
        }

        /// <summary>
        /// Reads a file from within the package associated with this session and writes it to a stream. 
        /// </summary>
        /// <param name="context">The context within which to render the file.</param>
        /// <remarks>
        /// At a minimum, the caller must set the following properties in the <paramref name="context"/>:
        /// <ul>
        /// <li><c>RelativePath</c></li>
        /// <li><c>OutputStream</c> or <c>Response</c></li>
        /// </ul>
        /// 
        /// On return, the following values in <paramref name="context"/> may have been set by the session:
        /// <ul>
        /// <li><c>MimeType</c></li>
        /// <li><c>FormId</c>, if a default was used.</li>
        /// </ul>
        /// In addition, the content was rendered into the <c>OutputStream</c> or <c>Response</c> of <paramref name="context"/>.
        /// 
        /// <p>This method will not read and write content that is not contained within the package. For instance, 
        /// if the entry point for an activity is an absolute URL and the caller passes it as the 
        /// <c>context.RelativePath</c>, this method will not succeed. It also will not render the package manifest.</p>
        /// 
        /// <p>In an Execute session, this method may only be called when <Prp>AttemptStatus</Prp> is 
        /// <c>AttemptStatus.Active.</c></p>
        /// 
        /// <p>This method may cause changes to the state of the session. After calling this method
        /// and prior to 
        /// disposing the object, the application
        /// should call <Mth>CommitChanges</Mth>  to ensure changes are persisted.</p>
        ///
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is not provided.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <c>context.RelativePath</c> or 
        /// <c>context.Response</c> is not provided or if the caller requests to render the package manifest.
        /// Also thrown if an Execute session requests to render content when the attempt is not active
        /// or there is no current, active activity identitified in the session.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file referenced in <c>context.RelativePath</c>
        /// cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the file refernced in <c>context.RelativePath</c>
        /// includes a directory that does not exist in the package.</exception>
        public override void Render(RenderContext context)
        {
            Utilities.ValidateParameterNonNull("context", context);
            ValidateRenderPath(context.RelativePath, "context.RelativePath");
            
            // In execute view, the attempt must be active and there must be a current, active activity.
            if (View == SessionView.Execute)
            {
                if (AttemptStatus != AttemptStatus.Active)
                {
                    throw new InvalidOperationException(SessionResources.SLS_RenderNotValid);
                }
                   
                ThrowIfCurrentActivityIsInactive();
            }


            // Create rloContext and fill it in
            RloRenderContext rloContext = new RloRenderContext(this, context);

            RloHandler.Render(rloContext);
        }
        /// <summary>
        /// Process the data returned from the client.
        /// </summary>
        /// <param name="formData">The collection of name/value pairs returned from a posted form.</param>
        /// <param name="files">Collection of valid posted files posted in the <c>HttpRequest.Files</c> collection.</param>
        /// <remarks>
        /// </remarks>
        public override void ProcessFormData(NameValueCollection formData, IDictionary<string, HttpPostedFile> files)
        {
            Utilities.ValidateParameterNonNull("formData", formData);
            Utilities.ValidateParameterNonNull("files", files);

            RloProcessFormDataContext context = new RloProcessFormDataContext(this);
            RloHandler.ProcessFormData(context, formData, files);
        }
       
        /// <summary>
        /// Gets the RloHandler for the current activity. Returns null if there is no current activity.
        /// Note that this value is not cached, since it may change every time the current activity changes.
        /// </summary>
        /// <remarks>This property returns null if there is no active, current activity in the session.</remarks>
        private RloHandler RloHandler
        {
            get
            {
                // If true, return an object, not null;
                bool returnRloHandler = (View == SessionView.Execute) ? CurrentActivityIsActive : HasCurrentActivity;

                if (returnRloHandler)
                {
                    switch (CurrentActivityResourceType)
                    {
                        case ResourceType.Lrm:
                            m_rloHandler = new LrmRloHandler();
                            break;
                        default:
                            m_rloHandler = new DefaultRloHandler();
                            break;
                    }
                }
                else
                {
                    m_rloHandler = null;
                }
                return m_rloHandler;
            }
        }

        /// <summary>
        /// Private helper property to get the package reader for this attempt.
        /// </summary>
        /// <returns>The package reader for files in this session.</returns>
        /// <remarks>Note that the caller is responsible for disposing the 
        /// returned object.</remarks>
        internal override PackageReader GetPackageReader()
        {
            return m_packageStore.GetPackageReader(new PackageItemIdentifier(DatabaseNavigator.PackageId), DatabaseNavigator.PackageLocation);
        }
                
        #region Show*Ui methods

        /// <summary>
        /// Returns true if the associated user interface element indicating 'continue to next 
        /// activity' should be shown.
        /// </summary>
        public override bool ShowNext
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    if (AttemptStatus == AttemptStatus.Active)
                    {
                        if (Navigator.CurrentActivity != null)
                            return !Navigator.CurrentActivity.HideContinueUI;
                        else
                            return false;   // active attempt but no current activity
                    }
                    else
                        return false;   // inactive attempt in execute view
                }

                return true;    // views other than execute
            }
        }

        /// <summary>
        /// Gets an indication if the user interface element indicating 'return to previous activity' should be shown.
        /// </summary>
        /// <returns>Returns true if the user interface element should be shown.
        /// </returns>
        public override bool ShowPrevious
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    if (AttemptStatus == AttemptStatus.Active)
                    {
                        if (Navigator.CurrentActivity != null)
                            return !Navigator.CurrentActivity.HidePreviousUI;
                        else
                            return false;   // active attempt but no current activity
                    }
                    else
                        return false;   // inactive attempt in execute view
                }

                return true;    // views other than execute
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
        public override bool ShowExit
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    if (AttemptStatus == AttemptStatus.Active)
                    {
                        if (Navigator.CurrentActivity != null)
                            return !Navigator.CurrentActivity.HideExitUI;
                    }
                }

                return false;
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
        public override bool ShowAbandon
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    if (AttemptStatus == AttemptStatus.Active)
                    {
                        if (Navigator.CurrentActivity != null)
                            return !Navigator.CurrentActivity.HideAbandonUI;
                    }
                }

                // in all other cases, it's not allowed.
                return false;
            }
        }

        /// <summary>
        /// Gets an indication if the user interface element to save the session should be shown. 
        /// </summary>
        /// <returns>Returns true if the user interface element to save the session should be shown.</returns>
        public override bool ShowSave
        {
            get {
                // Disallowed in Review mode.
                if (View == SessionView.Review)
                    return false;

                if (HasCurrentActivity)
                {
                    if ((CurrentActivityResourceType != ResourceType.Sco) 
                        && (CurrentActivityResourceType != ResourceType.Lrm))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion  // Show*Ui methods

        #region Exit operations
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
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object to ensure changes are persisted.
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
        public override void Exit()
        {
            // In Execute view, attemptstatus is required to check if this is valid. 
            // In that case, load the full data model to get the whole tree since if it's successful, the 
            // navigation command will require whole activity tree.
            if (View == SessionView.Execute)
                DatabaseNavigator.LoadActivityTree();

            // First check if the session is in a valid state to exit
            if (!IsExitValid)
                throw new InvalidOperationException(SessionResources.SLS_ExitNotValid);

            // If the attempt has started, then have navigator exit all activities. If it has not started, 
            // then just set the attempt status. 
            if (AttemptHasStarted)
                Navigator.Navigate(NavigationCommand.ExitAll);  // this sets attempt status
            else
            {
                DatabaseNavigator.Status = AttemptStatus.Completed;
            }

            // Autograde the session. This really only affects LRM content.
            Autograde();
        }

       
        private bool m_inAutogradeMode;
        private bool InAutogradeMode
        {
            get { return m_inAutogradeMode; }
            set { m_inAutogradeMode = value; }
        }
        
        /// <summary>
        /// Called when an attempt has just been completed or abandoned (Exit() or Abandon() called). 
        /// This method should only be called on execute sessions.
        /// </summary>
        private void Autograde()
        {
            // There's no concept of autograding for non-lrm content
            if (PackageFormat != PackageFormat.Lrm)
                return;

            // Go into autograding mode
            ExecuteNavigator evNav = BeginAutogradeMode();

            try
            {
                RloDataModelContext context = new RloDataModelContext(this);
                LrmRloHandler lrmHandler = new LrmRloHandler();

                // Iterate through activities in the session. Activities that have LRM resources 
                // need to be able to process that the session is ending and do autograding.
                while (true)
                {                    
                    if (evNav.CurrentActivity.ResourceType == ResourceType.Lrm)
                    {
                        lrmHandler.ProcessSessionEnd(context);

                        // Make sure that all the activities are suspended. This is not the normal way to 
                        // do it, but gets around some checks that do not allow calling other methods 
                        // during autograding.
                        evNav.CurrentActivity.DataModel.ActivityIsActive = false; 
                        evNav.CurrentActivity.DataModel.ActivityIsSuspended = true;  
                    }
                    evNav.Navigate(NavigationCommand.Continue);
                }
            }
            catch (SequencingException e)
            {
                // If the exception has the code of SB.2.1-1, it indicates this is the end of the session 
                // and is expected. Other codes indicate a real problem, so we rethrow those.
                if (e.Code != "SB.2.1-1")
                {
                    throw;
                }
            }
            finally
            {
                // Just being safe... make sure to exit this mode
                EndAutogradeMode(evNav);
            }
        }

        /// <summary>
        /// Start the process of autograding.
        /// </summary>
        /// <returns>Returns the ExecuteNavigator that can be used for navigation during 
        /// autograding.</returns>
        private ExecuteNavigator BeginAutogradeMode()
        {
            InAutogradeMode = true;

            ExecuteNavigator evNav = Navigator as ExecuteNavigator;
            evNav.BeginAutoGradingMode();

            return evNav;
        }

        /// <summary>
        /// End the process of autograding.
        /// </summary>
        private void EndAutogradeMode(ExecuteNavigator evNav)
        {
            InAutogradeMode = false;
            evNav.EndAutoGradingMode();
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which the 
        /// <Mth>Exit</Mth> method may be called. 
        /// </summary>
        public override bool IsExitValid
        {
            get
            {
                // If in Execute view, only valid if Attempt is active.
                if (View == SessionView.Execute)
                {
                    // Note: If this logic changes, make sure to check if Exit() still needs 
                    // to load the full data model.
                    return (AttemptStatus == AttemptStatus.Active);
                }

                return false;   // in all other views, this is never valid.
            }
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
        /// be suspended.</exception>
        public override void Suspend()
        {
            // In Execute view, attemptstatus is required to check if this is valid. 
            // In that case, load the full data model to get the whole tree since if it's successful, the 
            // navigation command will require whole activity tree.
            if (View == SessionView.Execute)
                DatabaseNavigator.LoadActivityTree();

            if (!IsSuspendValid)
                throw new InvalidOperationException(SessionResources.SLS_SuspendNotValid);

            Navigator.Navigate(NavigationCommand.SuspendAll);
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Suspend</Mth> method may be called.
        /// </summary>
        public override bool IsSuspendValid
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    // Note: If this logic changes, make sure to check if Suspend() still needs 
                    // to load the full data model.
                    return ((AttemptStatus == AttemptStatus.Active) && AttemptHasStarted);
                }

                return false;   // in all other views, this is never valid.
            }
        }

        /// <summary>
        /// Resumes a suspended session. Once the session has been resumed, it is considered active.
        /// </summary>
        /// <remarks>
        /// <p/>This method does not save changes that result from resuming the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object to ensure changes are persisted.
        /// 
        /// <p/>The only time that resuming a session is valid is if the session has been previously 
        /// suspended. That is, if the <Prp>AttemptStatus</Prp> is Suspended.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session is not 
        /// in the appropriate state to 
        /// be resumed. Use <Prp>IsResumeValid</Prp> to predict if the 
        /// operation will succeed.</exception>
        public override void Resume()
        {
            // In Execute view, attemptstatus is required to check if this is valid. 
            // In that case, load the full data model to get the whole tree since if it's successful, the 
            // navigation command will require whole activity tree.
            if (View == SessionView.Execute)
                DatabaseNavigator.LoadActivityTree();

            if (!IsResumeValid)
                throw new InvalidOperationException(SessionResources.SLS_ResumeNotValid);

            Navigator.Navigate(NavigationCommand.ResumeAll);
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Resume</Mth> method may be called.
        /// </summary>
        public override bool IsResumeValid
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    // Note: If this logic changes, make sure to check if Resume() still needs 
                    // to load the full data model.
                    return (AttemptStatus == AttemptStatus.Suspended);
                }

                return false;   // in all other views, this is never valid.
            }
        }

        /// <summary>
        /// Reactivate a session that has been completed or abandoned.
        /// </summary>
        /// <param name="settings">Settings that determine the behavior of the session during 
        /// reactivation.</param>
        /// <exception cref="InvalidOperationException">Thrown if the session is not in the appropriate 
        /// state to be reactivated. Also thrown if the current <Prp>View</Prp> is not RandomAccess.</exception>
        public override void Reactivate(ReactivateSettings settings)
        {
            if (!ValidateReactivateSettingValue(settings))
                throw new ArgumentOutOfRangeException("settings");

            if (!IsReactivateValid)
                throw new InvalidOperationException(SessionResources.SLS_ReactivateNotValid);

            RandomAccessNavigator raNav = DatabaseNavigator as RandomAccessNavigator;

            // Reactivate the session. This does not clear any values in the data model.
            raNav.Reactivate();

            // If there is nothing to clear, then return
            if (settings == ReactivateSettings.None)
                return;

            // If this is not LRM content, there's nothing to clear
            if (PackageFormat != PackageFormat.Lrm)
                return;

            // There is something to clear, so iterate through the activities with resources and 
            // find LRM pages. Each of those have work to do for reactivate.

            try
            {
                RloReactivateContext context = new RloReactivateContext(this, settings);
                LrmRloHandler lrmHandler = new LrmRloHandler();
                
                // Iterate through activities in the session. Activities that have LRM resources 
                // need to be able to clear information
                while (true)
                {
                    if (CurrentActivityResourceType == ResourceType.Lrm)
                    {
                        lrmHandler.Reactivate(context);
                    }
                    raNav.Navigate(NavigationCommand.Continue);
                }
            }
            catch (SequencingException e)
            {
                // If the exception has the code of SB.2.1-1, it indicates this is the end of the session 
                // and is expected. Other codes indicate a real problem, so we rethrow those.
                if (e.Code != "SB.2.1-1")
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns true if the session is in a state where <Mth>Reactivate</Mth> may be called.
        /// </summary>
        public override bool IsReactivateValid
        {
            get
            {
                if (View != SessionView.RandomAccess)
                    return false;

                if ((AttemptStatus != AttemptStatus.Abandoned)
                    && (AttemptStatus != AttemptStatus.Completed))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Stops the session by abandoning all activities in the session. 
        /// </summary>
        /// <remarks>
        /// <p/>The AttemptStatus must be Active
        /// in order to suspend a session in Execute view. It is not valid to suspend a session in other
        /// views.
        /// 
        /// <p/>This method does not save changes that result from abandoning the session. The application
        /// should call <Mth>CommitChanges</Mth> after this method and prior to disposing the object to ensure changes are persisted. 
        /// 
        /// <p/>PostCondition rules are not run after abandoning the session. Any current value of 
        /// <Prp>NavigationRequest.ExitMode</Prp> value of
        /// <Prp>LearningDataModel.NavigationRequest</Prp> will be ignored.
        /// 
        /// <p/>A session that has been abandoned cannot be resumed at a later time.
        /// 
        /// <p/>This method will read the activity tree associated with the session from
        /// LearningStore,
        /// if has not already been loaded.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session is not in a state in
        /// which the attempt can be abandoned. Use <Prp>IsAbandonValid</Prp> to predict if the 
        /// operation will succeed.</exception>
        public override void Abandon()
        {
            if (View == SessionView.Execute)
            {
                // Since attemptstatus is required, load the full data model on the assumption that the 
                // next thing that happens is the abandon operation, which requires the full tree.
                DatabaseNavigator.LoadActivityTree();
            }

            if (!IsAbandonValid)
                throw new InvalidOperationException(SessionResources.SLS_AbandonNotValid);

            Navigator.Navigate(NavigationCommand.AbandonAll);

            // Autograde the session. This really only affects LRM content.
            Autograde();
        }

        /// <summary>
        /// Gets a boolean indicating if the session is in a state in which
        /// the <Mth>Abandon</Mth> method may be called.
        /// </summary>
        /// <remarks>This property will read the activity tree associated with the session from
        /// LearningStore,
        /// if has not already been loaded.</remarks>
        public override bool IsAbandonValid
        {
            get
            {
                // In execute view, it's only valid for active attempts
                if (View == SessionView.Execute)
                {
                    // Note: If this logic changes, make sure to check if Resume() still needs 
                    // to load the full data model.
                    return (AttemptStatus == AttemptStatus.Active);
                }

                return false;   // in all other views, this is never valid.
            }
        }

        #endregion // exit Operations

        #region Navigation commands

        /// <summary>
        /// Moves to the next activity in the session. 
        /// For a session that represents an active attempt, this is determined 
        /// through sequencing rules of the package and the progress of the learner through it.
        /// In other views, this returns the next activity in a preorder traversal of the activity tree.
        /// </summary>
        /// <remarks>For a SessionView of Execute, this method requires a current activity in
        /// the session.
        /// <p/>Use <Prp>IsMoveToNextValid</Prp> to determine if an activity will identified as the current
        /// activity after calling this method.
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed during this process.
        /// <p>This method changes the state of the session. In order to save those changes in long-term storage, <Mth>CommitChanges</Mth>
        /// must be called after this method, and before the StoredLearningSession object is disposed. See the <Mth>CommitChanges</Mth> 
        /// documentation for a further explanation of how to optimize performance while ensuring session changes are properly saved to 
        /// LearningStore.</p>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session or if the session is not in a state that allows moving to the next activity. </exception>
        /// <exception cref="SequencingException">Thrown if a new activity could not be identified for delivery.</exception>
        public override void MoveToNext()
        {
            // Load the full data model to allow checking if the current activity is available.
            DatabaseNavigator.LoadActivityTree();

            if (View == SessionView.Execute)
            {
                if (AttemptStatus != AttemptStatus.Active)
                    throw new InvalidOperationException(SessionResources.SLS_MoveToNextNotValid);
                ThrowIfCurrentActivityIsNull();
            }

            Navigator.Navigate(NavigationCommand.Continue);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the next activity is a valid operation.
        /// </summary>
        /// <returns>Returns true if <Mth>MoveToNext()</Mth> would result in a new active activity in the 
        /// session.</returns>
        /// <remarks>
        /// </remarks>
        public override bool IsMoveToNextValid()
        {
            // Warning: This method executes the navigation engine. Do not call it from other session methods.

            // Load the full data model to allow checking if the current activity is available.
            DatabaseNavigator.LoadActivityTree();

            if (View == SessionView.Execute)
            {
                if (!HasCurrentActivity)
                    return false;
                
                // It's also only valid for active attempts. However, since inactive attempts will never 
                // have a current activity (in execute view), checking for current activity has the same 
                // effect.
            }

            return Navigator.IsNavigationValid(NavigationCommand.Continue);
        }

        /// <summary>
        /// Moves to the previous activity in the session. 
        /// For a session that represents an active attempt, this is determined 
        /// through sequencing rules of the package. In other cases, it is determined 
        /// by an in order traversal of the activity tree and navigation rules as defined in the package 
        /// manifest are not enforced.
        /// </summary>
        /// <remarks>For a SessionView of Execute, this method requires a current activity in
        /// the session.
        /// <p/>In the process of navigation, the current activity may be exited before the 
        /// navigation command is able to complete. If the navigation command then throws a SequencingException,
        /// the state of the session is undefined. The current activity may have 
        /// changed or become inactive during this process.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session or if the session is not in a state that allows moving to the previous activity.</exception>
        /// <exception cref="SequencingException">Thrown if a new activity could not be identified for delivery.</exception>
        public override void MoveToPrevious()
        {
            // Load the full data model to allow checking if the current activity is available.
            DatabaseNavigator.LoadActivityTree();

            if (View == SessionView.Execute)
            {
                if (AttemptStatus != AttemptStatus.Active)
                    throw new InvalidOperationException(SessionResources.SLS_MoveToPreviousNotValid);
                ThrowIfCurrentActivityIsNull();
            }
            Navigator.Navigate(NavigationCommand.Previous);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the previous activity is a valid operation.
        /// </summary>
        /// <returns>Returns true if <Mth>MoveToPrevious()</Mth> would result in a new active activity in the 
        /// session.</returns>
        /// <remarks>
        /// </remarks>
        public override bool IsMoveToPreviousValid()
        {
            // Warning: This method executes the navigation engine. Do not call it from other session methods.

            // Load the full data model to allow checking if the current activity is available.
            DatabaseNavigator.LoadActivityTree();

            if (View == SessionView.Execute)
            {
                if (!HasCurrentActivity)
                    return false;

                // It's also only valid for active attempts. However, since inactive attempts will never 
                // have a current activity (in execute view), checking for current activity has the same 
                // effect.
            }
            return Navigator.IsNavigationValid(NavigationCommand.Previous);
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
        public override void MoveToActivity(string activityKey)
        {
            if (activityKey == null)
                throw new ArgumentNullException("activityKey");
            if (String.IsNullOrEmpty(activityKey))
                throw new ArgumentOutOfRangeException("activityKey");

            // Load the full data model to allow checking the current attempt status.
            DatabaseNavigator.LoadActivityTree();

            if ((View == SessionView.Execute) && (AttemptStatus != AttemptStatus.Active))
                throw new InvalidOperationException(SessionResources.SLS_MoveToActivityNotValid);
                
            Navigator.NavigateTo(activityKey);
        }

        /// <summary>
        /// Makes the selected activity the current activity, using the unique activity id. 
        /// </summary>
        /// <param name="activityId">The unique identifier for the selected activity.</param>
        /// <remarks>
        /// <p/>Whenever possible, use this method, which takes a <c>long</c> value instead of 
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
        public override void MoveToActivity(long activityId)
        {
            // Load the full data model to allow checking the current attempt status.
            DatabaseNavigator.LoadActivityTree();

            if ((View == SessionView.Execute) && (AttemptStatus != AttemptStatus.Active))
                throw new InvalidOperationException(SessionResources.SLS_MoveToActivityNotValid);

            Navigator.NavigateTo(activityId);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the selected activity is a 
        /// valid operation.
        /// </summary>
        /// <param name="activityKey"></param>
        /// <returns>Returns true if moving to the selected activity would result in making that 
        /// activity become the current activity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="activityKey"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="activityKey"/> is an empty string.</exception>
        public override bool IsMoveToActivityValid(string activityKey)
        {
            if (activityKey == null)
                throw new ArgumentNullException("activityKey");
            if (String.IsNullOrEmpty(activityKey))
                throw new ArgumentOutOfRangeException("activityKey");

            // Load the full data model to allow checking the current attempt status.
            DatabaseNavigator.LoadActivityTree();

            if ((View == SessionView.Execute) && (AttemptStatus != AttemptStatus.Active))
                return false;

            return Navigator.IsNavigationToValid(activityKey);
        }

        /// <summary>
        /// Gets a boolean value indicating whether or not moving to the selected activity is a 
        /// valid operation. The selected activity is identified by its unique numerical id.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <returns>Returns true if moving to the selected activity would result in making that 
        /// activity become the current activity.</returns>
        public override bool IsMoveToActivityValid(long activityId)
        {
            // Load the full data model to allow checking the current attempt status.
            DatabaseNavigator.LoadActivityTree();

            if ((View == SessionView.Execute) && (AttemptStatus != AttemptStatus.Active))
                return false;

            return Navigator.IsNavigationToValid(activityId);
        }


        #endregion // Navigation commands

        #region Current activity information

        /// <summary>
        /// Gets a value which indicates if the session has a current activity. 
        /// </summary>
        public override bool HasCurrentActivity
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    if (AttemptStatus != AttemptStatus.Active)
                        return false;
                }
                return (Navigator.CurrentActivity != null);
            }
        }

        /// <summary>
        /// Gets the data model associated with the current activity in the session. 
        /// </summary>
        /// <returns>The data model associated with the current activity.</returns>
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
        /// the session or if the current activity is not active.</exception>
        public override LearningDataModel CurrentActivityDataModel
        {
            get
            {
                if (View == SessionView.Execute)
                {
                    // If we are in autograde mode, then we allow accessing the current activity
                    if (!InAutogradeMode)
                        ThrowIfCurrentActivityIsInactive();
                }
                else
                {
                    ThrowIfCurrentActivityIsNull();
                }

                return Navigator.CurrentActivity.DataModel;
            }
        }

        /// <summary>
        /// Gets the type of resource of the current activity.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        public override ResourceType CurrentActivityResourceType
        {
            get
            {
                // If we are in autograde mode, then we allow accessing the current activity
                if (!InAutogradeMode)
                    ThrowIfCurrentActivityIsNull();

                return Navigator.CurrentActivity.ResourceType;
            }
        }

        /// <summary>
        /// Gets the launching point of the current activity. This may be an absolute Uri, which indicates
        /// the resource is not in the package, or a package-relative Uri.
        /// </summary>
        public override Uri CurrentActivityEntryPoint
        {
            get
            {
                // If we are in autograde mode, then we allow accessing the current activity
                if (!InAutogradeMode)
                    ThrowIfCurrentActivityIsNull();

                return new Uri(Navigator.CurrentActivity.DefaultResourceFile, UriKind.RelativeOrAbsolute);
            }
        }

        /// <summary>
        /// Processes all of the pending navigation requests in the current activity's 
        /// data model. Returns true if this caused a navigation request to execute.
        /// </summary>
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
        /// then the caller should set the 
        /// <Prp>NavigationRequest.ExitMode</Prp> value of
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
        public override bool ProcessNavigationRequests()
        {
            // Load the full data model to allow checking if the current activity is available.
            DatabaseNavigator.LoadActivityTree();

            if (View == SessionView.Execute)
            {
                if (AttemptStatus != AttemptStatus.Active)
                    throw new InvalidOperationException(SessionResources.SLS_ProcessNavigationRequestsNotValid);                
            }
            else
            {
                throw new InvalidOperationException(SessionResources.SLS_ProcessNavigationRequestsNotValid);
            }
            ThrowIfCurrentActivityIsNull();

            if (!CurrentActivityIsActive)
                return false; 
            
            return Navigator.ProcessDataModelNavigation();
        }

        /// <summary>
        /// Immediately exits the current activity without processing any pending 
        /// navigation requests and without saving 
        /// any data model values that have not been committed.
        /// </summary>
        /// <remarks>
        /// <p/>It is recommended the application call <Mth>CommitChanges</Mth> after exiting the current 
        /// activity to ensure any pending data changes are saved.
        /// 
        /// <p/>If the caller wants to be able to resume the current activity at a future time, it should 
        /// set <Prp>NavigationRequest.ExitMode</Prp> value of
        /// <Prp>LearningDataModel.NavigationRequest</Prp> to Suspend before 
        /// calling this method.
        /// 
        /// <p/>This method will process the postConditionRules for the current activity. This may result in another 
        /// activity becoming the current activity in the session. It may also result in not being able to identify 
        /// a current activity for the session.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if there is no current activity in 
        /// the session.</exception>
        /// <exception cref="SequencingException">Thrown if the state of the session, such as the 
        /// current sequencing rules, do not allow processing the 
        /// requested navigation.</exception>
        public override void ExitCurrentActivity()
        {
            // Load the data model because the navigation request will require it. This avoids an 
            // extra db access to get just the current activity information.
            DatabaseNavigator.LoadActivityTree();

            // The only time this is a valid request is if this is Execute view, attempt is active
            // and there is a current activity. 
            if ((View == SessionView.Execute) && (AttemptStatus == AttemptStatus.Active))
            {
                ThrowIfCurrentActivityIsNull();
                ThrowIfCurrentActivityIsInactive();

                // Do the command.
                Navigator.Navigate(NavigationCommand.UnqualifiedExit);
            }
            else
                throw new InvalidOperationException(SessionResources.SLS_ExitCurrentActivityNotValid);

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
        /// <p/>PostCondition rules are not run after the activity has been abandoned. Any current value of 
        /// <Prp>NavigationRequest.ExitMode</Prp> of
        /// <Prp>LearningDataModel.NavigationRequest</Prp> will be ignored, and the 
        /// <Mth>AttemptStatus</Mth> of the attempt on the activity will be set to Abandonded.
        /// 
        /// <p/>Once this method is called, the current activity will no longer be active for this session. That is,
        /// <Prp>LearningSession.CurrentActivityIsActive</Prp> will be false. To continue
        /// the session, select an activity to attempt 
        /// using one of the <Mth>MoveToActivity</Mth> methods.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this method is called when 
        /// there is no current activity in 
        /// the session.</exception>
        /// <exception cref="SequencingException">Thrown if the state of the session, such as the 
        /// current sequencing rules, do not allow processing the 
        /// requested navigation.</exception>
        public override void AbandonCurrentActivity()
        {
            // Load the data model because the navigation request will require it. This avoids an 
            // extra db access just to start by getting the current activity information.
            DatabaseNavigator.LoadActivityTree();

            // The only time this is a valid request is if this is Execute view, attempt is active
            // and there is a current activity. 
            if ((View == SessionView.Execute) && (AttemptStatus == AttemptStatus.Active))
            {
                ThrowIfCurrentActivityIsNull();
                ThrowIfCurrentActivityIsInactive();

                // Do the command.
                Navigator.Navigate(NavigationCommand.Abandon);
            }
            else
                throw new InvalidOperationException(SessionResources.SLS_AbandonCurrentActivityNotValid);

        }

        #endregion

        #region Attempt information
        /// <summary>
        /// Gets the unique id of the attempt being used in this session.
        /// </summary>
        public AttemptItemIdentifier AttemptId { get { return new AttemptItemIdentifier(DatabaseNavigator.AttemptId) ; } }

        /// <summary>
        /// Returns the state of attempt that is being managed by this session. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public AttemptStatus AttemptStatus { get { return DatabaseNavigator.Status; } }

        /// <summary>
        /// Gets the time which the attempt associated with this session started. The returned time 
        /// is UTC.
        /// </summary>
        public DateTime AttemptStartTime { get { return DatabaseNavigator.AttemptStartTime; } }

        /// <summary>
        /// Gets the time at which the attempt associated with this session ended. The returned time
        /// is UTC.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt associated 
        /// with this session has not ended. That is, it is not valid to access this property 
        /// if the <Prp>AttemptStatus</Prp> is 
        /// Active or Suspended.
        /// </exception>
        public DateTime AttemptEndTime
        {
            get
            {
                if ((this.AttemptStatus == AttemptStatus.Active)
                        || AttemptStatus == AttemptStatus.Suspended)
                    throw new InvalidOperationException(SessionResources.SLS_AttemptEndTimeNotValid);

                return DatabaseNavigator.AttemptEndTime;
            }
        }

        #endregion // Attempt information

        /// <summary>
        /// Removes the cached version of session data for all activities in the 
        /// non-active session. This method should be called prior to an application 
        /// directly modifying any data within LearningStore pertaining to this attempt.
        /// </summary>
        /// <remarks>  
        /// If an application makes changes to data in LearningStore that is 
        /// associated with this 
        /// session, it should call this method to remove the cached version of the data.
        /// This allows future accesses by the Review or RandomAccess views to rebuild the 
        /// cache.  Calling this method while an attempt in the session is active
        /// is not valid and will throw an InvalidOperationException.
        /// 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the session represents 
        /// an active attempt. That is, if <Prp>AttemptStatus</Prp>is Active. </exception>
        public void InvalidateSessionCache()
        {
            // Never valid if AttemptStatus == Active
            if (AttemptStatus == AttemptStatus.Active)
                throw new InvalidOperationException(SessionResources.SLS_InvalidateSessionCacheNotValid);

            DatabaseNavigator.DeleteDataModelCache();
        }

        /// <summary>
        /// Expands information from the cache information about the session 
        /// into distinct column values within LearningStore. 
        /// </summary>
        /// <remarks>
        /// <p/>This method is only valid for views of an active attempt. 
        /// <p/>The cache will be expanded when an attempt is completed, 
        /// causing information from the session cache to be written into the various tables 
        /// relating to the attempt.  
        /// <p/>If an application needs data from LearningStore
        /// for reporting purposes prior to the attempt being completed, it should call this method
        /// before reading information from LearningStore.
        /// </remarks>
        public void ExpandSessionCache()
        {
            DatabaseNavigator.ExpandDataModelCache();
        }  

        /// <summary>
        /// Return the navigator for this session. 
        /// </summary>
        private DatabaseNavigator DatabaseNavigator
        {
            get
            {
                return m_dbNav;
            }
        }
    }

    /// <summary>
    /// Represents options for levels of logging during a session.
    /// </summary>
    /// <remarks>
    /// When LoggingOptions are set to anything other than LoggingOptions.None, the session will 
    /// write logging information to LearningStore, in the SequencingLogEntryItem table. 
    /// </remarks>
    [Flags]
    public enum LoggingOptions
    {
        /// <summary>
        /// Do not log this session.
        /// </summary>
        None = 0,

        /// <summary>
        /// Creates log entries in the SequencingLogEntryItem table in LearningStore describing every step taken during the 
        /// Sequencing and Navigation process and each decision that was made during 
        /// that process.  This does not also include the final result of the Sequencing 
        /// and Navigation process.
        /// </summary>
        LogDetailSequencing = 1,

        /// <summary>
        /// Creates log entries in the SequencingLogEntryItem table in LearningStore describing the final result 
        /// of each Sequencing and Navigation request.
        /// </summary>
        LogFinalSequencing = 2,

        /// <summary>
        /// Creates log entries in the SequencingLogEntryItem table in LearningStore describing the actions 
        /// taken during Rollup, as described in the SCORM 2004 specification.
        /// This has no effect for sessions involving SCORM 1.2 content.
        /// </summary>
        LogRollup = 4,

        /// <summary>
        /// Equal to specifying both LogDetailSequencing and LogFinalSequencing.  
        /// All information pertinent to Sequencing and Navigation will be logged.
        /// </summary>
        LogAllSequencing = (LogDetailSequencing | LogFinalSequencing),

        /// <summary>
        /// Equal to specifying LogDetailSequencing, LogFinalSequencing, and 
        /// LogRollup.  All information pertinent to Sequencing and Navigation or 
        /// rollup will be logged.
        /// </summary>
        /// <remarks>Rollup logging only applies to sessions involving SCORM 1.2 
        /// content. Therefore, for SCORM 1.2 content <c>LogAll</c> is equivalent 
        /// to <c>LogAllSequencing</c>.</remarks>
        LogAll = (LogDetailSequencing | LogFinalSequencing | LogRollup)
    }
}
