/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;
using System.Data;
using System.Xml.XPath;
using System.Transactions;
using Microsoft.LearningComponents.DataModel;
using System.Xml;
using System.Collections;
using System.IO;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Implements a <c>Navigator</c> that uses the database and executes an attempt.
    /// </summary>
    internal class ExecuteNavigator : DatabaseNavigator
    {
        /// <summary>
        /// Initializes an <c>ExecuteNavigator</c> object.
        /// </summary>
        /// <remarks>
        /// This is only used by the static <Mth>CreateExecuteNavigator</Mth> method.  It should be kept private
        /// to avoid it being called by external callers who may not initialize the other fields as necessary.
        /// </remarks>
        private ExecuteNavigator()
        {
        }

        /// <summary>
        /// Initializes a new <Typ>ExecuteNavigator</Typ> object.
        /// </summary>
        /// <param name="store">A <Typ>LearningStore</Typ> object that references the database to use.</param>
        /// <param name="attemptId">The unique identifier of the attempt item in the database.</param>
        public ExecuteNavigator(LearningStore store, long attemptId)
        {
            m_store = store;
            m_attemptId = attemptId;
        }

        /// <summary>
        /// Determines whether this activity is valid to be auto-graded.
        /// </summary>
        /// <param name="activity">The activity to check.</param>
        /// <returns>Whether or not this activity is valid to be auto-graded.</returns>
        static private bool IsValidForAutoGrading(Activity activity)
        {
            return !String.IsNullOrEmpty(activity.ResourceKey);
        }

        /// <summary>
        /// Set to true when auto-grading mode is active.
        /// </summary>
        private bool m_autoGradingMode;

        /// <summary>
        /// Reactivates a completed attempt.
        /// </summary>
        public override void Reactivate()
        {
            throw new InvalidOperationException(Resources.NotAllowedInExecuteMode);
        }

        /// <summary>
        /// Begins auto grading mode for a completed or abandoned attempt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calling BeginAutoGradingMode() when AttemptStatus is Abandoned or Complete will cause the effective 
        /// "Current Activity" to be equal to the left most activity in the tree that has a resource attached 
        /// to it.  At this point, calling Navigate(NavigationCommand.Continue) or 
        /// Navigate(NavigationCommand.Previous) will move to the next/previous activities that have a resource 
        /// without regard to any sequencing, making that activity the current one and allowing access to that 
        /// activity's data model.  No changes to the data model will affect any sequencing done in 
        /// auto-grading mode, nor will they be rolled up [NOTE: The justification for this is that autograding 
        /// is only applicable to LRM activities, which are SCORM 1.2, and thus don't have rollup anyway].  No 
        /// other navigation command (including choice navigation) is valid in auto-grading mode.  
        /// </para>
        /// <para>
        /// If Navigate(NavigationCommand.Continue) or Navigate(NavigationCommand.Previous) moves beyond either 
        /// end of the activity tree while in auto grading mode, a SequencingException will occur.
        /// </para>
        /// <para>
        /// Calling Save() while in auto-grading mode will result in an InvalidOperationException.  
        /// EndAutoGradingMode() must be called prior to a Save().
        /// </para>
        /// <para>
        /// It is perfectly permissable to call BeginAutoGradingMode() multiple times, as long as EndAutoGradingMode() 
        /// is called after each one.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the Status of the attempt is Active or Suspended, or if BeginAtuoGradingMode() has already been called.</exception>
        public void BeginAutoGradingMode()
        {
            LoadActivityTree();
            if(Status == AttemptStatus.Active ||
                Status == AttemptStatus.Suspended)
            {
                throw new InvalidOperationException(Resources.AttemptHasNotEnded);
            }
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            m_autoGradingMode = true;
            foreach(Activity act in Traverse)
            {
                if(IsValidForAutoGrading(act))
                {
                    m_currentActivity = act;
                    m_currentActivity.DataModel.ActivityIsActive = true;
                    break;
                }
            }

        }

        /// <summary>
        /// Ends auto-grading mode for this attempt.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if auto-grading mode is not active for this attempt.</exception>
        public void EndAutoGradingMode()
        {
            if(!m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.AutoGradingModeNotBegun);
            }
            m_autoGradingMode = false;
            m_currentActivity.DataModel.ActivityIsActive = false;
            m_currentActivity = null;
        }

        /// <summary>
        /// Adds all the activities to the navigator, and performs initial setup.
        /// </summary>
        /// <param name="d">DataTable containing the activities to add.</param>
        /// <param name="rootActivityId">Root activity identifier.</param>
        /// <param name="learnerName">Name of the learner.</param>
        /// <param name="learnerLanguage">Preferred language used by the learner.</param>
        /// <param name="learnerCaption">Preferred captioning setting used by the learner.</param>
        /// <param name="learnerAudioLevel">Preferred audio level setting for the learner.</param>
        /// <param name="learnerDeliverySpeed">Preferred delivery speed for the learner.</param>
        /// <remarks>
        /// This is only called by ExecuteNavigator().  Removed from the middle of that function to get
        /// FxCop to shut up.
        /// </remarks>
        private void AddActivities(DataTable d, long rootActivityId, string learnerName, string learnerLanguage, 
                                   AudioCaptioning learnerCaption, float learnerAudioLevel, float learnerDeliverySpeed)
        {
            // create activity objects and store them in a big dictionary
            foreach(DataRow row in d.Rows)
            {
                Activity act = new Activity(this, ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityTreeView.Id]).GetKey(),
                    ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityTreeView.DataModelCache] as LearningStoreXml, true),
                    null, null, null, WrapAttachment, WrapAttachmentGuid, -1, 
                    (bool)row[Schema.SeqNavActivityTreeView.ObjectivesGlobalToSystem], 
                    DataModelWriteValidationMode.AllowWriteOnlyIfActive, 
                    m_learnerId.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                    learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
                m_activities[act.ActivityId] = act;
                if(act.ActivityId == rootActivityId)
                {
                    RootActivity = act;
                    if(!(row[Schema.SeqNavActivityTreeView.ParentActivityId] is DBNull))
                    {
                        throw new LearningStoreItemNotFoundException(Resources.InvalidRootActivityId);
                    }
                }
            }

            // now that we have all the activities in a big dictionary, find all the parents to build the tree
            // in theory this could be done in the first loop if I sort the query by parentid, but that's making a lot
            // of assumptions about the structure of the database that I don't think are safe to make
            foreach(DataRow row in d.Rows)
            {
                LearningStoreItemIdentifier parentId = row[Schema.SeqNavActivityTreeView.ParentActivityId] as LearningStoreItemIdentifier;
                if (parentId != null)
                {
                    long id = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityTreeView.Id]).GetKey();
                    m_activities[id].Parent = m_activities[parentId.GetKey()];
                    m_activities[parentId.GetKey()].AddChild(m_activities[id]);
                }
            }

            // make sure children of each parent are in the right order, and set Previous and Next pointer correctly.
            SortActivityTree();

            // step through the activity tree searching for selection and randomization instructions
            // this must be done before the ActivityAttemptItems are saved because they may reorder
            // and potentially even remove children.
#if DEBUG
            Random rand;
            if(NavigatorData.Random != null)
            {
                rand = NavigatorData.Random;
            }
            else
            {
                rand = new Random();
            }
#else
            Random rand = new Random();
#endif
            foreach(Activity a in Traverse)
            {
                if(!a.IsLeaf)
                {
                    // if there's a valid selection instruction, remove excess
                    // child activities randomly
                    if(a.Sequencing.SelectionTiming == RandomizationTiming.Once &&
                        a.Sequencing.RandomizationSelectCount > 0 &&
                        a.Sequencing.RandomizationSelectCount < a.Children.Count)
                    {
                        int selCount = a.Sequencing.RandomizationSelectCount;
                        while(a.Children.Count > selCount)
                        {
                            a.RemoveChild(rand.Next(a.Children.Count));
                        }
                        a.SortChildren();
                    }

                    // if there's a valid randomization instruction, randomize order of the children
                    // of this activity
                    if((a.Sequencing.RandomizationTiming == RandomizationTiming.Once ||
                        a.Sequencing.RandomizationTiming == RandomizationTiming.OnEachNewAttempt) &&
                        a.Sequencing.ReorderChildren)
                    {
                        List<Activity> randlist = new List<Activity>();
                        while(a.Children.Count > 0)
                        {
                            int i = rand.Next(a.Children.Count);
                            randlist.Add((Activity)a.Children[i]);
                            a.RemoveChild(i);
                        }
                        for(int i = 0 ; i < randlist.Count ; ++i)
                        {
                            randlist[i].RandomPlacement = i;
                            a.AddChild(randlist[i]);
                        }
                        a.SortChildren();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <Typ>ExecuteNavigator</Typ> object in memory and its representation in the database.
        /// </summary>
        /// <param name="store">A <Typ>LearningStore</Typ> object that references the database to use.</param>
        /// <param name="rootActivityId">The database identifier for the root activity (i.e. organization) of the activity tree to attempt.</param>
        /// <param name="learnerId">The database identifier for the learner information.</param>
        /// <param name="loggingFlags">Flags specifying which actions to log.</param>
        /// <returns>A new <Typ>ExecuteNavigator</Typ> object.</returns>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the learner ID or root activity ID is invalid, or if the package information cannot be found.</exception>
        static public ExecuteNavigator CreateExecuteNavigator(LearningStore store, long rootActivityId, long learnerId, LoggingOptions loggingFlags)
        {
            ExecuteNavigator eNav = new ExecuteNavigator();
            
            LearningStoreJob job = store.CreateJob();

            // first add security
            Dictionary<string,object> securityParameters = new Dictionary<string,object>();
            securityParameters[Schema.CreateAttemptRight.RootActivityId] =
                new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, rootActivityId);
            securityParameters[Schema.CreateAttemptRight.LearnerId] =
                new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, learnerId);
            job.DemandRight(Schema.CreateAttemptRight.RightName, securityParameters);
            job.DisableFollowingSecurityChecks();
            
            // first query for all information about the learner
            LearningStoreQuery query = store.CreateQuery(Schema.UserItem.ItemTypeName);
            query.AddColumn(Schema.UserItem.Id);
            query.AddColumn(Schema.UserItem.Name);
            query.AddColumn(Schema.UserItem.Language);
            query.AddColumn(Schema.UserItem.AudioCaptioning);
            query.AddColumn(Schema.UserItem.AudioLevel);
            query.AddColumn(Schema.UserItem.DeliverySpeed);
            query.AddCondition(Schema.UserItem.Id, LearningStoreConditionOperator.Equal,
                new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, learnerId));
            job.PerformQuery(query);

            // then query the package information
            query = store.CreateQuery(Schema.SeqNavActivityPackageView.ViewName);
            LearningStoreItemIdentifier rootId = new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, rootActivityId);
            query.AddColumn(Schema.SeqNavActivityPackageView.PackageId);
            query.AddColumn(Schema.SeqNavActivityPackageView.PackageFormat);
            query.AddColumn(Schema.SeqNavActivityPackageView.PackagePath);
            query.AddCondition(Schema.SeqNavActivityPackageView.Id, LearningStoreConditionOperator.Equal, rootId);
            job.PerformQuery(query);

            // then query for the activity tree
            query = store.CreateQuery(Schema.SeqNavActivityTreeView.ViewName);
            query.AddColumn(Schema.SeqNavActivityTreeView.DataModelCache);
            query.AddColumn(Schema.SeqNavActivityTreeView.ParentActivityId);
            query.AddColumn(Schema.SeqNavActivityTreeView.Id);
            query.AddColumn(Schema.SeqNavActivityTreeView.ObjectivesGlobalToSystem);
            query.AddCondition(Schema.SeqNavActivityTreeView.RootActivityId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, rootActivityId));
            job.PerformQuery(query);

            DataTable d;  // used to store results of query
            ReadOnlyCollection<object> ids;

            // for this transaction we need to read from the activitypackageitem table and write new records to
            // the activityattemptitem and attemptitem tables
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            using(LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                // execute the query
                ReadOnlyCollection<object> c = job.Execute();
                Utilities.Assert(c.Count == 3);

                if(((DataTable)c[0]).Rows.Count < 1)
                {
                    throw new LearningStoreItemNotFoundException(Resources.InvalidLearnerId);
                }
                if(((DataTable)c[1]).Rows.Count < 1)
                {
                    throw new LearningStoreItemNotFoundException(Resources.InvalidRootActivityId);
                }
                if(((DataTable)c[2]).Rows.Count < 1)
                {
                    throw new LearningStoreItemNotFoundException(Resources.InvalidRootActivityId);
                }

                d = (DataTable)c[0];  // save learner information

                string learnerName = (string)d.Rows[0][Schema.UserItem.Name];
                AudioCaptioning learnerCaption = (AudioCaptioning)d.Rows[0][Schema.UserItem.AudioCaptioning];
                float learnerAudioLevel = (float)d.Rows[0][Schema.UserItem.AudioLevel];
                float learnerDeliverySpeed = (float)d.Rows[0][Schema.UserItem.DeliverySpeed];
                string learnerLanguage = (string)d.Rows[0][Schema.UserItem.Language];

                d = (DataTable)c[1];  // save package information

                // we need to create the activity tree within the transaction because it may affect the data written
                // by means of the selection and randomization processes.

                eNav.m_packageFormat = (PackageFormat)d.Rows[0][Schema.SeqNavActivityPackageView.PackageFormat];
                eNav.m_packageId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavActivityPackageView.PackageId]).GetKey();
                eNav.m_packageLocation = (string)d.Rows[0][Schema.SeqNavActivityPackageView.PackagePath];
                eNav.m_store = store;
                eNav.m_learnerId = learnerId;
                eNav.m_loggingFlags = loggingFlags;

                // we must set this here so that the Activity  constructor doesn't try and load missing info
                eNav.m_attemptItemInformationIsValid = true;

                d = (DataTable)c[2];  // save data to create activity tree later, when we are done with sql processing

                eNav.AddActivities(d, rootActivityId, learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
                /*
                // create activity objects and store them in a big dictionary
                foreach(DataRow row in d.Rows)
                {
                    Activity act = new Activity(eNav, ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityTreeView.Id]).GetKey(),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityTreeView.DataModelCache] as LearningStoreXml, true),
                        null, null, null, eNav.WrapAttachment, eNav.WrapAttachmentGuid, -1, 
                        (bool)row[Schema.SeqNavActivityTreeView.ObjectivesGlobalToSystem], eNav.m_learnerId.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                        learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
                    eNav.m_activities[act.ActivityId] = act;
                    if(act.ActivityId == rootActivityId)
                    {
                        eNav.RootActivity = act;
                        if(!(row[Schema.SeqNavActivityTreeView.ParentActivityId] is DBNull))
                        {
                            throw new LearningStoreItemNotFoundException(Resources.InvalidRootActivityId);
                        }
                    }
                }

                // now that we have all the activities in a big dictionary, find all the parents to build the tree
                // in theory this could be done in the first loop if I sort the query by parentid, but that's making a lot
                // of assumptions about the structure of the database that I don't think are safe to make
                foreach(DataRow row in d.Rows)
                {
                    LearningStoreItemIdentifier parentId = row[Schema.SeqNavActivityTreeView.ParentActivityId] as LearningStoreItemIdentifier;
                    if (parentId != null)
                    {
                        long id = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityTreeView.Id]).GetKey();
                        eNav.m_activities[id].Parent = eNav.m_activities[parentId.GetKey()];
                        eNav.m_activities[parentId.GetKey()].AddChild(eNav.m_activities[id]);
                    }
                }

                // make sure children of each parent are in the right order, and set Previous and Next pointer correctly.
                eNav.SortActivityTree();
                
                // step through the activity tree searching for selection and randomization instructions
                // this must be done before the ActivityAttemptItems are saved because they may reorder
                // and potentially even remove children.
                Random rand = new Random();
                foreach(Activity a in eNav.Traverse)
                {
                    if(!a.IsLeaf)
                    {
                        // if there's a valid selection instruction, remove excess
                        // child activities randomly
                        if(a.Sequencing.SelectionTiming == RandomizationTiming.Once &&
                            a.Sequencing.RandomizationSelectCount > 0 &&
                            a.Sequencing.RandomizationSelectCount < a.Children.Count)
                        {
                            int selCount = a.Sequencing.RandomizationSelectCount;
                            for(int i = 0 ; i < a.Children.Count ; ++i)
                            {
                                if(rand.Next(a.Children.Count) < selCount)
                                {
                                    --selCount;
                                }
                                else
                                {
                                    a.RemoveChild(i);
                                }
                            }
                            a.SortChildren();
                        }

                        // if there's a valid randomization instruction, randomize order of the children
                        // of this activity
                        if((a.Sequencing.RandomizationTiming == RandomizationTiming.Once ||
                            a.Sequencing.RandomizationTiming == RandomizationTiming.OnEachNewAttempt) &&
                            a.Sequencing.ReorderChildren)
                        {
                            List<Activity> randlist = new List<Activity>();
                            while(a.Children.Count > 0)
                            {
                                int i = rand.Next(a.Children.Count);
                                randlist.Add((Activity)a.Children[i]);
                                a.RemoveChild(i);
                            }
                            for(int i = 0 ; i < randlist.Count ; ++i)
                            {
                                randlist[i].RandomPlacement = i;
                                a.AddChild(randlist[i]);
                            }
                            a.SortChildren();
                        }
                    }
                }
                 * */
                
                // create the attemptitem
                // fill in fields with no defaults
                job = store.CreateJob();
                job.DisableFollowingSecurityChecks();
                Dictionary<string, object> attempt = new Dictionary<string,object>();
                attempt[Schema.AttemptItem.RootActivityId] = new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, rootActivityId);
                attempt[Schema.AttemptItem.LearnerId] = new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, learnerId);
                attempt[Schema.AttemptItem.PackageId] = new LearningStoreItemIdentifier(Schema.PackageItem.ItemTypeName, eNav.m_packageId);
                eNav.m_attemptStartTime = DateTime.UtcNow;
                attempt[Schema.AttemptItem.StartedTimestamp] = eNav.m_attemptStartTime;
                attempt[Schema.AttemptItem.LogDetailSequencing] = ((loggingFlags & LoggingOptions.LogDetailSequencing) == LoggingOptions.LogDetailSequencing);
                attempt[Schema.AttemptItem.LogFinalSequencing] = ((loggingFlags & LoggingOptions.LogFinalSequencing) == LoggingOptions.LogFinalSequencing);
                attempt[Schema.AttemptItem.LogRollup] = ((loggingFlags & LoggingOptions.LogRollup) == LoggingOptions.LogRollup);
                LearningStoreItemIdentifier attemptId = job.AddItem(Schema.AttemptItem.ItemTypeName, attempt, true);

                // create activityattemptitems for each activity in the tree
                foreach(Activity a in eNav.Traverse)
                {
                    Dictionary<string, object> activity = new Dictionary<string, object>();

                    // fill in everything that has no default
                    activity[Schema.ActivityAttemptItem.AttemptId] = attemptId;
                    activity[Schema.ActivityAttemptItem.ActivityPackageId] = new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, a.ActivityId);
                    if(a.RandomPlacement >= 0)
                    {
                        activity[Schema.ActivityAttemptItem.RandomPlacement] = a.RandomPlacement;
						// sibling activities are ordered by RandomPlacement first,
						// then by OriginalPlacment; RandomPlacement is -1 for all siblings if not randomization occurs
                    }
                    job.AddItem(Schema.ActivityAttemptItem.ItemTypeName, activity, true);
                }
                ids = job.Execute();
                scope.Complete();
            }

            // fill in some vital data for the navigator
            eNav.m_attemptId = ((LearningStoreItemIdentifier)ids[0]).GetKey();
            int index = 1;
            foreach(Activity a in eNav.Traverse)
            {
                a.InternalActivityId = ((LearningStoreItemIdentifier)ids[index++]).GetKey();
            }

            return eNav;
        }

        /// <summary>
        /// Loads the entire activity tree from the database, if it is not already in memory.
        /// </summary>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        protected void LoadActivityTreeAndCheckStatus()
        {
            if(m_attemptItemInformationIsValid)
            {
                if(m_status == AttemptStatus.Completed)
                {
                    throw new InvalidOperationException(Resources.AttemptComplete);
                }
                else if(m_status == AttemptStatus.Abandoned)
                {
                    throw new InvalidOperationException(Resources.AttemptAbandoned);
                }
            }

            LoadActivityTree();

            // wait till the end for this check, so that in the case where somebody
            // keeps calling navigator functions on a completed attempt, the second+ times
            // no database calls will be required to tell him its invalid.
            if(m_status == AttemptStatus.Completed)
            {
                throw new InvalidOperationException(Resources.AttemptComplete);
            }
            else if(m_status == AttemptStatus.Abandoned)
            {
                throw new InvalidOperationException(Resources.AttemptAbandoned);
            }
        }

        /// <summary>
        /// Recursively sets up a subtree within an activity tree.
        /// </summary>
        /// <param name="activity">Root of the subtree.</param>
        /// <param name="evaluateSequencingRules"><c>True</c> to evaluate sequencing rules when determining
        /// which elements are valid to navigate to.  <c>False</c> to determine which elements are valid
        /// to navigate to using only the static data in the manifest.</param>
        private void SetupTocSubtree(Activity activity, bool evaluateSequencingRules)
        {
            if(activity.Parent != null && !activity.Parent.Sequencing.Choice)
            {
                // if the parent disallows choice, this is always false
                activity.SetValidToNavigateTo(false);
            }
            else if(evaluateSequencingRules)
            {
                // PERFORMANCE ISSUE:  This call makes a partial copy of the 
                // activity tree.  Try to limit calls to this to an absolute minimum.
                //
                // The alternative to calling this function is to duplicate a large 
                // amount of the sequencing code (for both SCORM 2004 and SCORM 1.2)
                // with lots of special cases for changes of state that should have 
                // happened but could not because of the temporary nature of this 
                // request.  This makes it a maintenance nightmare.

                activity.SetValidToNavigateTo(IsNavigationToValid(activity));
            }
            else
            {
                // if we are not evaluating sequencing rules, set the leaf activity to "true"
                activity.SetValidToNavigateTo(true);
            }
            if(!activity.IsLeaf)
            {
                // process each child of this activity
                foreach(Activity a in activity.Children)
                {
                    SetupTocSubtree(a, evaluateSequencingRules);
                }
            }
        }

        /// <summary>
        /// Loads the table of contents into memory and verifies which elements are valid to navigate to.
        /// </summary>
        /// <param name="evaluateSequencingRules"><c>True</c> to evaluate sequencing rules when determining
        /// whih elements are valid to navigate to.  <c>False</c> to determine which elements are valid
        /// to navigate to using only the static data in the manifest.</param>
        /// <returns>The root table of contents element.</returns>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override TableOfContentsElement LoadTableOfContents(bool evaluateSequencingRules)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            // load the activity tree into memory, if necessary
            LoadActivityTree();

            if(m_packageFormat == PackageFormat.V1p3)
            {
                // first initialize the tree to be all invalid targets for navigation 
                foreach(Activity a in Traverse)
                {
                    a.SetValidToNavigateTo(false);
                }

                if(Status == AttemptStatus.Completed || Status == AttemptStatus.Suspended ||
                    Status == AttemptStatus.Abandoned)
                {
                    return RootActivity;
                }

                if(m_currentActivity != null)
                {
                    // if there is a current activity, we may be limit the subtree to search
                    // based on the choiceExit sequencing flag
                    Activity top = RootActivity;
                    for(Activity a = m_currentActivity ; a != null ; a = a.Parent)
                    {
                        if(!a.Sequencing.ChoiceExit)
                        {
                            top = a;
                            break;
                        }
                    }
                    SetupTocSubtree(top, evaluateSequencingRules);
                }
                else
                {
                    // if there is no current activity, use the whole activity tree.
                    SetupTocSubtree(RootActivity, evaluateSequencingRules);
                }
            }
            else
            {
                foreach(Activity a in Traverse)
                {
                    a.SetValidToNavigateTo(a.ResourceType != ResourceType.None);
                }
            }
            return RootActivity;
        }

        /// <summary>
        /// Determines if the passed navigation command is valid to execute or not.
        /// </summary>
        /// <param name="command">The navigation command to test.</param>
        /// <returns>True if the navigation would succeed, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is equal to NavigationCommand.Choose.</exception>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override bool IsNavigationValid(NavigationCommand command)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            Utilities.Assert(command != NavigationCommand.Choose, "EXN0010");
            if(command == NavigationCommand.ChoiceStart)
            {
                return IsNavigationToValid(RootActivity.ActivityId);
            }
            LoadActivityTreeAndCheckStatus();
            return IsNavigationValid(command, null);
        }

        /// <summary>
        /// Determines if a choice navigation to the activity identified by the string identifier passed will succeed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override bool IsNavigationToValid(string destination)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            LoadActivityTreeAndCheckStatus();
            Activity act = ActivityKeyToActivity(destination);
            if(act == null)
            {
                return false;
            }
            return IsNavigationValid(NavigationCommand.Choose, act);
        }

        /// <summary>
        /// Determines if a choice navigation to the activity identified by the unique identifier passed will succeed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The destination passed is not a valid activity in this activity tree.</exception>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override bool IsNavigationToValid(long id)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            LoadActivityTreeAndCheckStatus();
            Activity act;
            if(!m_activities.TryGetValue(id, out act))
            {
                return false;
            }
            return IsNavigationValid(NavigationCommand.Choose, act);
        }

        /// <summary>
        /// Performs any navigation request, except for Choice navigation.
        /// </summary>
        /// <param name="command">Navigation command to execute.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is equal to NavigationCommand.Choose.</exception>
        /// <exception cref="InvalidOperationException">The attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override void Navigate(NavigationCommand command)
        {
            Utilities.Assert(command != NavigationCommand.Choose, "EXN0010");
            if(m_autoGradingMode)
            {
                Activity a;
                LoadActivityTree();
                if(m_currentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
                if(command == NavigationCommand.Continue)
                {
                    for(a = m_currentActivity.NextInPreorderTraversal ; a != null && !IsValidForAutoGrading(a) ; a = a.NextInPreorderTraversal)
                        ;
                    if(a == null)
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_1__1);
                    }
                }
                else if(command == NavigationCommand.Previous)
                {
                    for(a = m_currentActivity.PreviousInPreorderTraversal ; a != null && !IsValidForAutoGrading(a) ; a = a.PreviousInPreorderTraversal)
                        ;
                    if(a == null)
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_1__3);
                    }
                }
                else
                {
                    // we only support Continue/Previous
                    throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
                }
                m_currentActivity.DataModel.ActivityIsActive = false;
                m_currentActivity = a;
                m_currentActivity.DataModel.ActivityIsActive = true;
                UpdateCurrentActivityData();
                return;
            }
            LoadActivityTreeAndCheckStatus();
            if(command == NavigationCommand.ChoiceStart)
            {
                bool shouldBreak = false;
                foreach(Activity act in Traverse)
                {
                    if(act.IsLeaf)
                    {
                        shouldBreak = true;
                        try
                        {
                            NavigateTo(act.ActivityId);
                        }
                        catch(SequencingException)
                        {
                            shouldBreak = false;
                        }
                        if(shouldBreak)
                        {
                            break;
                        }
                    }
                }
                if(!shouldBreak) // made it through the loop without successfully navigating to a leaf
                {
                    throw new InvalidPackageException(SessionResources.SLS_CouldNotFindFirstActivity);
                }
            }
            else
            {
                Activity current = m_currentActivity;
                Activity suspended = m_suspendedActivity;
                bool shouldExit = Navigate(command, null);
                if(current != m_currentActivity || suspended != m_suspendedActivity)
                {
                    m_changed = true;
                    UpdateCurrentActivityData();
                }
                UpdateStatus(command, shouldExit);
            }
        }

        /// <summary>
        /// Performs a choice navigation to the activity identified by the string identifier passed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="InvalidOperationException">The destination passed is not a valid activity in this activity tree, or
        /// the attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override void NavigateTo(string destination)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            LoadActivityTreeAndCheckStatus();
            Activity act = ActivityKeyToActivity(destination);
            if(act == null)
            {
                throw new InvalidOperationException(Resources.InvalidActivity);
            }
            Activity current = m_currentActivity;
            Activity suspended = m_suspendedActivity;
            bool shouldExit = Navigate(NavigationCommand.Choose, act);
            if(current != m_currentActivity || suspended != m_suspendedActivity)
            {
                m_changed = true;
                UpdateCurrentActivityData();
            }
            UpdateStatus(NavigationCommand.Choose, shouldExit);
        }

        /// <summary>
        /// Performs a choice navigation to the activity identified by the unique identifier passed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="InvalidOperationException">The destination passed is not a valid activity in this activity tree, or
        /// the attempt status is not valid for this call, or the attempt ID is invalid.</exception>
        public override void NavigateTo(long id)
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            LoadActivityTreeAndCheckStatus();
            Activity act;
            if(!m_activities.TryGetValue(id, out act))
            {
                throw new InvalidOperationException(Resources.InvalidActivity);
            }
            Activity current = m_currentActivity;
            Activity suspended = m_suspendedActivity;
            bool shouldExit = Navigate(NavigationCommand.Choose, act);
            if(current != m_currentActivity || suspended != m_suspendedActivity)
            {
                m_changed = true;
                UpdateCurrentActivityData();
            }
            UpdateStatus(NavigationCommand.Choose, shouldExit);
        }

        /// <summary>
        /// Performs a navigation operation based on the data model of the current activity, if one is specified.
        /// </summary>
        /// <returns>True if a navigation request was executed.</returns>
        /// <remarks>This method may result in one of several navigation requests being executed: ExitAll, SuspendAll, 
        /// Choice, Continue, and Previous are those supported by ADL, but a direct assignment to NavigationRequest.Command 
        /// may allow more.  If an ExitMode is set, this always overrides a NavigationRequest.Command.  Setting
        /// ExitMode to ExitMode.Suspend will result in a change of state for the current activity, but does not actually 
        /// perform any navigation, and false is returned in this case.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid or there is no current activity.</exception>
        public override bool ProcessDataModelNavigation()
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            LoadActivityTreeAndCheckStatus();
            if(m_currentActivity == null)
            {
                throw new InvalidOperationException(Resources.NoCurrentActivity);
            }
            NavigationCommand? command = null;

            if(m_currentActivity.DataModel.NavigationRequest.ExitMode != null)
            {
                if(m_packageFormat == PackageFormat.V1p3)
                {
                    switch(m_currentActivity.DataModel.NavigationRequest.ExitMode)
                    {
                    case ExitMode.TimeOut:
                        command = NavigationCommand.ExitAll;
                        break;
                    case ExitMode.Suspended:
                        m_currentActivity.DataModel.ActivityIsActive = false;
                        m_currentActivity.DataModel.ActivityIsSuspended = true;
                        // according to SCORM, an LMS may do rollup at any time.  So, do rollup now (as long as there
                        // is an activity tree.)
                        Rollup(m_currentActivity);
                        return false;
                    case ExitMode.Logout:
                        command = NavigationCommand.SuspendAll;
                        break;
                    }
                }
                else
                {
                    switch(m_currentActivity.DataModel.NavigationRequest.ExitMode)
                    {
                    case ExitMode.Suspended:
                        m_currentActivity.DataModel.ActivityIsActive = false;
                        m_currentActivity.DataModel.ActivityIsSuspended = true;
                        return false;
                    case ExitMode.Logout:
                        command = NavigationCommand.SuspendAll;
                        break;
                    }
                }
            }

            if(( command == null) && (m_currentActivity.DataModel.NavigationRequest.Command != null))
            {
                command = m_currentActivity.DataModel.NavigationRequest.Command;
            }
            if(command != null)
            {
                if(command == NavigationCommand.Choose)
                {
                    NavigateTo(m_currentActivity.DataModel.NavigationRequest.Destination);
                }
                else
                {
                    Navigate(command.Value);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves any data that has been changed to the database, if necessary.
        /// </summary>
        public override void Save()
        {
            if(m_autoGradingMode)
            {
                throw new InvalidOperationException(Resources.NotAllowedWhileInAutoGradingMode);
            }
            base.Save();
        }
    }
}
