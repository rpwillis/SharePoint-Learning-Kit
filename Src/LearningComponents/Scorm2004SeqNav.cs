/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.LearningComponents.DataModel;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.XPath;
using System.Globalization;
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Base class that exposes SCORM sequencing and navigation
    /// </summary>
    internal abstract partial class Navigator
    {
        private abstract class SeqNav
        {
            /// <summary>
            /// The navigator that the sequencing applies to.
            /// </summary>
            protected NavigatorData m_navigator;

            /// <summary>
            /// The command that is being executed currently.
            /// </summary>
            protected NavigationCommand m_command;

            /// <summary>
            /// Initializes the class that performs sequencing/navigation.
            /// </summary>
            /// <param name="navigator">The <Typ>Navigator</Typ> performing the sequencing operation.</param>
            public SeqNav(NavigatorData navigator)
            {
                m_navigator = navigator;
            }

            /// <summary>
            /// Performs the requested sequencing/navigation using SCORM 2004 sequencing/navigation rules.
            /// </summary>
            /// <param name="command">The navigation command to perform.</param>
            /// <param name="destination"><c>Activity</c> that is the destination of a Choice navigation command.</param>
            /// <returns>True if the sequencing session has ended.</returns>
            /// <exception cref="SequencingException">Occurs when there is an invalid navigation performed.</exception>
            public abstract bool OverallSequencingProcess(NavigationCommand command, Activity destination);
        }

        /// <summary>
        /// Class that handles SCORM 2004 sequencing and navigation.
        /// </summary>
        private class Scorm2004SeqNav : SeqNav
        {
            /// <summary>
            /// All possible sequencing requests
            /// </summary>
            private enum SequencingRequest
            {
                Choice,
                Continue,
                Exit,
                Previous,
                ResumeAll,
                Retry,
                Start
            }

            /// <summary>
            /// All possible termination requests
            /// </summary>
            private enum TerminationRequest
            {
                Abandon,
                AbandonAll,
                Exit,
                ExitAll,
                ExitParent,
                SuspendAll
            }

            /// <summary>
            /// Direction of traversal
            /// </summary>
            private enum TraversalDirection
            {
                Backward,
                Forward,
                NotApplicable
            }

            /// <summary>
            /// Initializes the class that performs sequencing/navigation using SCORM 2004 sequencing/navigation rules.
            /// </summary>
            /// <param name="navigator">The <Typ>Navigator</Typ> performing the sequencing operation.</param>
            public Scorm2004SeqNav(NavigatorData navigator) : base(navigator)
            {
            }

            /// <summary>
            /// Performs the requested sequencing/navigation using SCORM 2004 sequencing/navigation rules.
            /// </summary>
            /// <param name="command">The navigation command to perform.</param>
            /// <param name="destination"><c>Activity</c> that is the destination of a Choice navigation command.</param>
            /// <returns>True if the sequencing session has ended.</returns>
            /// <remarks>Corresponds to OP.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            /// <exception cref="SequencingException">Occurs when there is an invalid navigation performed.</exception>
            public override bool OverallSequencingProcess(NavigationCommand command, Activity destination)
            {
                SequencingRequest? seqRequest;
                TerminationRequest? termRequest;
                Activity deliveryRequest = null;
                bool exitSession;
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                m_command = command;
                ProcessNavigationRequest(destination, out seqRequest, out termRequest);
                if(termRequest != null)
                {
                    SequencingRequest? newSeqRequest;
                    newSeqRequest = ProcessTerminationRequest(termRequest.Value);
                    if(newSeqRequest != null)
                    {
                        seqRequest = newSeqRequest;
                    }
                }
                if(seqRequest != null)
                {
                    ProcessSequencingRequest(seqRequest.Value, destination, out deliveryRequest, out exitSession);
                    if(exitSession)
                    {
                        m_navigator.CurrentActivity = null;
                        m_navigator.LogSequencing(SequencingEventType.FinalNavigation, m_command, Resources.NavigationFinishedDone);
                        return true;
                    }
                }
                if(deliveryRequest != null)
                {
                    ProcessDeliveryRequest(deliveryRequest);
                    ContentDeliveryEnvironment(deliveryRequest);
                    m_navigator.LogSequencing(SequencingEventType.FinalNavigation, m_command, Resources.NavigationFinishedNewActivity, deliveryRequest.Key);
                }
                else
                {
                    m_navigator.LogSequencing(SequencingEventType.FinalNavigation, m_command, Resources.NavigationFinishedDone);
                }
                return false;
            }

            /// <summary>
            /// Resets the attempt data for a specific activity, if specified to do so by the sequencing node.
            /// </summary>
            /// <param name="activity">The activity to reset.</param>
            /// <param name="useCurrentAttemptObjectiveInfo">Whether or not to reset objective information.</param>
            /// <param name="useCurrentAttemptProgressInfo">Whether or not to reset progress information.</param>
            static private void ResetAttemptData(Activity activity, bool useCurrentAttemptObjectiveInfo, 
                bool useCurrentAttemptProgressInfo)
            {
                if(useCurrentAttemptProgressInfo)
                {
                    activity.DataModel.ClearAttemptProgressInfo();
                }
                if(useCurrentAttemptObjectiveInfo)
                {
                    activity.DataModel.ClearAttemptObjectiveInfo();
                }
            }

            /// <summary>
            /// Performs necessary data model initialization for an activity to be delivered.
            /// </summary>
            /// <param name="activity">The activity to be delivered</param>
            private void PerformDataModelInitialization(Activity activity)
            {
                bool save = activity.DataModel.ActivityIsActive;
                activity.DataModel.ActivityIsActive = true; // so I can write to the data model
                m_navigator.UpdateActivityData(activity);  // load data model information from database, if needed
                activity.DataModel.InitializeForDelivery();
                foreach(Objective obj in activity.DataModel.Objectives)
                {
                    bool satisfied = false;
                    float measure = (float)0;

                    // From SCORM 2004 2nd Edition Addendum, section 2.25, the "read" objective maps are not honored for non-tracked items.
                    if(activity.Sequencing.Tracked && m_navigator.ReadGlobalObjectiveSatisfiedStatus(obj.GlobalObjectiveReadSatisfiedStatus, ref satisfied))
                    {
                        // REQ_72.3.3.3
                        if(satisfied)
                        {
                            obj.SuccessStatus = SuccessStatus.Passed;
                        }
                        else
                        {
                            obj.SuccessStatus = SuccessStatus.Failed;
                        }
                        if(activity == m_navigator.RootActivity && obj.IsPrimaryObjective)
                        {
                            m_navigator.SuccessStatus = obj.SuccessStatus;
                        }
                    }
                    // From SCORM 2004 2nd Edition Addendum, section 2.25, the "read" objective maps are not honored for non-tracked items.
                    if(activity.Sequencing.Tracked && m_navigator.ReadGlobalObjectiveNormalizedMeasure(obj.GlobalObjectiveReadNormalizedMeasure, ref measure))
                    {
                        // REQ_72.3.3.2
                        obj.Score.Scaled = measure;
                        if(activity == m_navigator.RootActivity)
                        {
                            m_navigator.TotalPoints = obj.Score.Scaled.Value * 100;
                        }
                    }
                }
                activity.DataModel.ActivityIsActive = save;
            }

            /// <summary>
            /// Applies any randomization to a cluster, if applicable
            /// </summary>
            /// <param name="rootOfCluster">The root of the cluster to randomize.</param>
            static private void ApplyRandomization(Activity rootOfCluster)
            {
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

                // if this activity has requested randomization of its children and 
                // it is not the first attempt on this activity (this case is handled when
                // the tree is first created), randomize the children.
                if(rootOfCluster.Sequencing.RandomizationTiming == RandomizationTiming.OnEachNewAttempt &&
                    rootOfCluster.Sequencing.ReorderChildren &&
                    rootOfCluster.DataModel.ActivityAttemptCount > 0)
                {
                    List<Activity> randlist = new List<Activity>();
                    while(rootOfCluster.Children.Count > 0)
                    {
                        int i = rand.Next(rootOfCluster.Children.Count);
                        randlist.Add((Activity)rootOfCluster.Children[i]);
                        rootOfCluster.RemoveChild(i);
                    }
                    for(int i = 0 ; i < randlist.Count ; ++i)
                    {
                        randlist[i].RandomPlacement = i;
                        rootOfCluster.AddChild(randlist[i]);
                    }
                    rootOfCluster.SortChildren();
                }
            }

            /// <summary>
            /// Performs actions necessary to delivery an activity's content.
            /// </summary>
            /// <param name="activity">The activity to deliver.</param>
            /// <remarks>Corresponds to DB.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ContentDeliveryEnvironment(Activity activity)
            {
                // If the attempt on the current activity has not been terminated, we cannot deliver new content
                if(m_navigator.CurrentActivity != null && m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                {
                    // Delivery request is invalid - The Current Activity has not been terminated
                    throw new SequencingException(SequencingExceptionCode.DB_2__1);
                }
                // Content is about to be delivered, clear any existing suspend all state.
                if(activity != m_navigator.SuspendedActivity)
                {
                    ClearSuspendedActivity(activity);
                }
                else
                {
                    m_navigator.SuspendedActivity = null;
                }

                if(m_navigator.CurrentActivity != null)
                {
                    TerminateDescendentAttempts(m_navigator.CurrentActivity, activity);
                }
                // Set Current Activity to the activity identified for delivery
                m_navigator.CurrentActivity = activity;
                Stack<Activity> activityList = new Stack<Activity>();
                for(Activity a = activity ; a != null ; a = a.Parent)
                {
                    activityList.Push(a);
                }
                while (activityList.Count > 0)
                {
                    Activity a = activityList.Pop();
                    if (!a.DataModel.ActivityIsActive)
                    {
                        // make sure we set ActivityIsActive to true first, otherwise
                        // it may throw due to DataModelWriteValidationMode
                        a.DataModel.ActivityIsActive = true;
                        if(a.Sequencing.Tracked)
                        {
                            if(a.DataModel.ActivityIsSuspended)
                            {
                                // If the previous attempt on the activity ended due to a 
                                // suspension, clear the suspended state; do not start a new attempt
                                a.DataModel.ActivityIsSuspended = false;
                                // according to the scorm addendum, clear the cmi.exit value no matter what
                                // so that we don't automatically suspend again each time this activity is executed
                                a.DataModel.InitializeForDeliveryAfterSuspend();
                            }
                            else
                            {
                                // Begin a new attempt on the activity
                                ++a.DataModel.ActivityAttemptCount;

                                // Is this the first attempt on the activity?
                                if (a.DataModel.ActivityAttemptCount == 1)
                                {
                                    a.DataModel.ActivityProgressStatus = true;
                                }
                                ResetAttemptData(a, a.Sequencing.UseCurrentAttemptObjectiveInfo,
                                    a.Sequencing.UseCurrentAttemptProgressInfo);
                                PerformDataModelInitialization(a);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Clears flags indicating an activity is suspended for an activity and its ancestors.
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to DB.2.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ClearSuspendedActivity(Activity activity)
            {
                if(m_navigator.SuspendedActivity != null)
                {
                    Activity commonAncestor = FindCommonAncestor(activity, m_navigator.SuspendedActivity);
                    for(Activity a = m_navigator.SuspendedActivity ; a != commonAncestor.Parent ; a = a.Parent)
                    {
                        if(a.IsLeaf)
                        {
                            a.DataModel.ActivityIsSuspended = false;
                        }
                        else
                        {
                            bool setSuspended = true;
                            foreach(Activity child in a.Children)
                            {
                                if(child.DataModel.ActivityIsSuspended)
                                {
                                    setSuspended = false;
                                    break;
                                }
                            }
                            if(setSuspended)
                            {
                                a.DataModel.ActivityIsSuspended = false;
                            }
                        }
                    }
                    m_navigator.SuspendedActivity = null;
                }
            }

            /// <summary>
            /// Processes a termination request.
            /// </summary>
            /// <param name="termRequest">The termination request to process</param>
            /// <returns>A sequencing request, or null if no new sequencing request is called for.</returns>
            /// <remarks>Corresponds to TB.2.3 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            SequencingRequest? ProcessTerminationRequest(TerminationRequest termRequest)
            {
                if(termRequest != TerminationRequest.AbandonAll && m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.TB_2_3__1);
                }
                // If the current activity has already been terminated, there is nothing to terminate
                if((termRequest == TerminationRequest.Exit || termRequest == TerminationRequest.Abandon) &&
                    !m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                {
                    throw new SequencingException(SequencingExceptionCode.TB_2_3__2);
                }
                SequencingRequest? seqRequest = null;
                switch(termRequest)
                {
                case TerminationRequest.Exit:
                    // Ensure the state of the current activity is up to date
                    EndAttemptProcess(m_navigator.CurrentActivity);

                    // Check if any of the current activity's ancestors need to terminate
                    SequencingExitActionRules(m_navigator.CurrentActivity);

                    bool processedExit;
                    do
                    {
                        processedExit = false;
                        TerminationRequest? newTermRequest;
                        SequencingPostConditionRules(m_navigator.CurrentActivity, out seqRequest, out newTermRequest);
                        if(newTermRequest == TerminationRequest.ExitAll)
                        {
                            // Process an Exit All Termination Request
                            goto exitAllLabel;
                        }
                        else if(newTermRequest == TerminationRequest.ExitParent)
                        {
                            // If we exit the parent of the current activity, move 
                            // the current activity to the parent of the current activity.
                            if(m_navigator.CurrentActivity == m_navigator.RootActivity)
                            {
                                // The root of the activity tree does not have a parent to exit
                                throw new SequencingException(SequencingExceptionCode.TB_2_3__4);
                            }
                            m_navigator.CurrentActivity = m_navigator.CurrentActivity.Parent;
                            EndAttemptProcess(m_navigator.CurrentActivity);
                            processedExit = true;
                        }
                        else
                        {
                            // If the attempt on the root of the Activity Tree is ending without a 
                            // Retry, the Sequencing Session also ends
                            if(m_navigator.CurrentActivity == m_navigator.RootActivity && seqRequest != SequencingRequest.Retry)
                            {
                                seqRequest = SequencingRequest.Exit;
                            }
                        }
                    } while(processedExit);
                    break;
                case TerminationRequest.ExitAll:
exitAllLabel:
                    if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                    {
                        EndAttemptProcess(m_navigator.CurrentActivity);
                    }
                    TerminateDescendentAttempts(m_navigator.CurrentActivity, m_navigator.RootActivity);
                    EndAttemptProcess(m_navigator.RootActivity);
                    m_navigator.CurrentActivity = m_navigator.RootActivity;
                    if(seqRequest == null)
                    {
                        seqRequest = SequencingRequest.Exit;
                    }
                    break;
                case TerminationRequest.SuspendAll:
                    // If the current activity is active or already suspended, suspend it and all of its descendents
                    if(m_navigator.CurrentActivity.DataModel.ActivityIsActive || m_navigator.CurrentActivity.DataModel.ActivityIsSuspended)
                    {
                        m_navigator.SuspendedActivity = m_navigator.CurrentActivity;
                    }
                    else
                    {
                         // Make sure the current activity is not the root of the activity tree
                        if(m_navigator.CurrentActivity != m_navigator.RootActivity)
                        {
                            m_navigator.SuspendedActivity = m_navigator.CurrentActivity.Parent;
                        }
                        else
                        {
                             // Nothing to suspend
                            throw new SequencingException(SequencingExceptionCode.TB_2_3__3);
                        }
                    }
                    for(Activity a = m_navigator.SuspendedActivity ; a != null ; a = a.Parent)
                    {
                        a.DataModel.ActivityIsSuspended = true;
                        a.DataModel.ActivityIsActive = false;
                        ExtendedRollup(a);
                    }
                    if(m_navigator.SuspendedActivity.IsLeaf)
                    {
                        FinalizeDataModelPriorToExit(m_navigator.SuspendedActivity);
                    }
                    m_navigator.CurrentActivity = m_navigator.RootActivity;
                    seqRequest = SequencingRequest.Exit;
                    break;
                case TerminationRequest.Abandon:
                    m_navigator.CurrentActivity.DataModel.ActivityIsActive = false;
                    ExtendedRollup(m_navigator.CurrentActivity);
                    break;
                case TerminationRequest.AbandonAll:
                    for(Activity a = m_navigator.CurrentActivity ; a != null ; a = a.Parent)
                    {
                        a.DataModel.ActivityIsActive = false;
                        ExtendedRollup(a);
                    }
                    m_navigator.CurrentActivity = m_navigator.RootActivity;
                    seqRequest = SequencingRequest.Exit;
                    break;
                }
                return seqRequest;
            }

            /// <summary>
            /// Processes post condition rules on an activity, and returns a sequencing request and/or a termination request.
            /// </summary>
            /// <param name="activity">Activity to perform post condition rules on.</param>
            /// <param name="seqRequest">Sequencing request to perform based on the post condition rules, or null if none.</param>
            /// <param name="termRequest">Termination request to perform based on the post condition rules, or null if none.</param>
            /// <remarks>Corresponds to TB.2.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void SequencingPostConditionRules(Activity activity, out SequencingRequest? seqRequest, out TerminationRequest? termRequest)
            {
                seqRequest = null;
                termRequest = null;
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(activity.DataModel.ActivityIsSuspended)
                {
                    return;
                }
                SequencingRuleAction? action = SequencingRulesCheck(activity, activity.Sequencing.PostConditionRules);
                switch(action)
                {
                case SequencingRuleAction.Retry:
                    // Attempt to override any pending sequencing request with this one
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestRetry);
                    seqRequest = SequencingRequest.Retry;
                    return;
                case SequencingRuleAction.Continue:
                    // Attempt to override any pending sequencing request with this one
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestContinue);
                    seqRequest = SequencingRequest.Continue;
                    return;
                case SequencingRuleAction.Previous:
                    // Attempt to override any pending sequencing request with this one
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestPrevious);
                    seqRequest = SequencingRequest.Previous;
                    return;
                case SequencingRuleAction.ExitParent:
                    // Terminate the appropriate activity(s)
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestExitParent);
                    termRequest = TerminationRequest.ExitParent;
                    return;
                case SequencingRuleAction.ExitAll:
                    // Terminate the appropriate activity(s)
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestExitAll);
                    termRequest = TerminationRequest.ExitAll;
                    return;
                case SequencingRuleAction.RetryAll:
                    // Terminate all active activities and move the current activity to the root of the activity tree; then perform an 'in-process' start
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPostConditionRuleResult, activity.Key, Resources.SequencingPostConditionRuleRequestRetryAll);
                    seqRequest = SequencingRequest.Retry;
                    termRequest = TerminationRequest.ExitAll;
                    return;
                }
            }

            /// <summary>
            /// Checks for exit condition rules for an activity and its parents
            /// </summary>
            /// <param name="activity">The activity being exited.</param>
            /// <remarks>Corresponds to TB.2.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void SequencingExitActionRules(Activity activity)
            {
                Stack<Activity> activities = new Stack<Activity>();
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                // Form the activity path as the ordered series of activities from the root 
                // of the activity tree to the parent of the CurrentActivity, inclusive
                for(Activity a = activity.Parent ; a != null ; a = a.Parent)
                {
                    activities.Push(a);
                }
                while(activities.Count > 0)
                {
                    Activity act = activities.Pop();
                    if(SequencingRulesCheck(act, act.Sequencing.ExitConditionRules, SequencingRuleAction.Exit))
                    {
                        m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingExitRuleExecuted, act.Key);
                        TerminateDescendentAttempts(m_navigator.CurrentActivity, act);
                        EndAttemptProcess(act);
                        m_navigator.CurrentActivity = act;
                        return;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="currentActivity"></param>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to UP.3 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void TerminateDescendentAttempts(Activity currentActivity, Activity activity)
            {
                Activity commonAncestor = FindCommonAncestor(currentActivity, activity);

                if(commonAncestor == currentActivity)
                {
                    return;
                }
                // Form the activity path as the ordered series of activities from the 
                // CurrentActivity to the common ancestor, exclusive of the CurrentActivity 
                // and the common ancestor 
                // The current activity must have already been exited
                for(Activity a = currentActivity.Parent ; a != commonAncestor ; a = a.Parent)
                {
                    // End the current attempt on each activity
                    EndAttemptProcess(a);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            private void FinalizeDataModelPriorToExit(Activity activity)
            {
                bool save = activity.DataModel.AdvancedAccess;
                activity.DataModel.AdvancedAccess = true;
                if(activity.Sequencing.Tracked)
                {
                    // perform assignments in the specific order listed on SN-4-32
                    if(activity.DataModel.SuccessStatus != SuccessStatus.Unknown)
                    {
                        activity.PrimaryObjective.SuccessStatus = activity.DataModel.SuccessStatus;
                        if(activity == m_navigator.RootActivity)
                        {
                            m_navigator.SuccessStatus = activity.PrimaryObjective.SuccessStatus;
                        }
                    }
                    if(activity.DataModel.Score.Scaled.HasValue)
                    {
                        activity.PrimaryObjective.Score.Scaled = activity.DataModel.Score.Scaled;
                        if(activity == m_navigator.RootActivity)
                        {
                            m_navigator.TotalPoints = activity.PrimaryObjective.Score.Scaled.Value * 100;
                        }
                    }

                    // The sequencer will not affect the state of suspended activities
                    if(!activity.DataModel.ActivityIsSuspended)
                    {
                        // Did the content inform the sequencer of the activity's completion status?
                        if(!activity.DataModel.AttemptProgressStatus && !activity.Sequencing.CompletionSetByContent)
                        {
                            activity.DataModel.CompletionStatus = CompletionStatus.Completed;
                            if(activity == m_navigator.RootActivity)
                            {
                                m_navigator.CompletionStatus = activity.DataModel.CompletionStatus;
                            }
                        }
                        // Did the content inform the sequencer of the activity's rolled-up objective status?
                        if(!activity.PrimaryObjective.ObjectiveProgressStatus && !activity.Sequencing.ObjectiveSetByContent)
                        {
                            activity.PrimaryObjective.SuccessStatus = SuccessStatus.Passed;
                            if(activity == m_navigator.RootActivity)
                            {
                                m_navigator.SuccessStatus = activity.PrimaryObjective.SuccessStatus;
                            }
                        }
                    }
                }
                activity.DataModel.TotalTime += activity.DataModel.SessionTime;
                activity.DataModel.SessionTime = TimeSpan.Zero;
                activity.DataModel.AdvancedAccess = save;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to UP.4 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void EndAttemptProcess(Activity activity)
            {
                if(activity.IsLeaf)
                {
                    FinalizeDataModelPriorToExit(activity);
                }
                else
                {
                    activity.DataModel.ActivityIsSuspended = false;
                    foreach(Activity child in activity.Children)
                    {
                        if(child.DataModel.ActivityIsSuspended)
                        {
                            activity.DataModel.ActivityIsSuspended = true;
                            break;
                        }
                    }
                }
                activity.DataModel.ActivityIsActive = false;  // this has to be in this order for rollup to work correctly
                OverallRollup(activity);
                ExtendedRollup(activity);
            }

            public void Rollup(Activity activity)
            {
                if(activity.IsLeaf)
                {
                    FinalizeDataModelPriorToExit(activity);
                }
                OverallRollup(activity);
                ExtendedRollup(activity);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to RB.1.5 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void OverallRollup(Activity activity)
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                m_navigator.LogSequencing(SequencingEventType.Rollup, m_command, Resources.RollupInitiated, activity.Key);

                // Form the activity path as the ordered series of activities from the root 
                // of the activity tree to the activity, inclusive, in reverse order.
                for(Activity act = activity ; act != null ; act = act.Parent)
                {
                    if(!act.IsLeaf)
                    {
                        MeasureRollup(act);
                    }

                    // Apply the appropriate behavior described in section RB.1.2, based on the activity's defined sequencing information
                    if(act.PrimaryObjective != null && act.PrimaryObjective.SatisfiedByMeasure)
                    {
                        ObjectiveRollupByMeasure(act);
                    }
                    else
                    {
                        ObjectiveRollupByRules(act);
                    }
                    ActivityProgressRollup(act);
                    m_navigator.WriteGlobalObjectives(act);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to the "Extended Rollup Process" outlined on SN-4-34 and SCORM 2004 Addendum Version 2 2-47.</remarks>
            private void ExtendedRollup(Activity activity)
            {
                // Form the "rollup set" of tracked activities that includes all parents of any activity (read Objective Map)
                // that shares a global objective with the Current Activity (write Objective Map).
                List<Activity> rollupSet = new List<Activity>();
                List<Objective> globalObjectives = new List<Objective>();
                foreach(Objective obj in activity.DataModel.Objectives)
                {
                    if(obj.GlobalObjectiveWriteNormalizedMeasureCount > 0 || obj.GlobalObjectiveWriteSatisfiedStatusCount > 0)
                    {
                        globalObjectives.Add(obj);
                    }
                }
                AddBranchToExtendedRollupSet(m_navigator.RootActivity, ref rollupSet, globalObjectives);

                // Apply the Overall Rollup Process, starting with the deepest activity in the Activity Tree.
                while(rollupSet.Count > 0)
                {
                    OverallRollup(rollupSet[0]);
                    // During the Overall Rollup Process, remove activities from the "rollup set" that are encountered.
                    for(Activity act = rollupSet[0] ; act != null ; act = act.Parent)
                    {
                        if(rollupSet.Contains(act))
                        {
                            rollupSet.Remove(act);
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="act"></param>
            /// <param name="rollupSet"></param>
            /// <param name="globalObjectives"></param>
            private void AddBranchToExtendedRollupSet(Activity act, ref List<Activity> rollupSet, List<Objective> globalObjectives)
            {
                if(!act.Sequencing.Tracked)
                {
                    return;
                }
                foreach(Activity child in act.Children)
                {
                    if(!child.IsLeaf)
                    {
                        AddBranchToExtendedRollupSet(child, ref rollupSet, globalObjectives);
                    }
                }
                foreach(Activity child in act.Children)
                {
                    if(child.IsLeaf)
                    {
                        AddBranchToExtendedRollupSet(child, ref rollupSet, globalObjectives);
                    }
                }
                if(ContainsReadObjectiveMap(act, globalObjectives))
                {
                    // as per SCORM 2004 Addendum Version 1.2, section 2.18.2, add the parent of the activity to the rollup set,
                    // not the activity itself.
                    if(act.Parent != null)
                    {
                        if(!rollupSet.Contains(act.Parent))
                        {
                            rollupSet.Add(act.Parent);
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="globalObjectives"></param>
            /// <returns></returns>
            private static bool ContainsReadObjectiveMap(Activity activity, List<Objective> globalObjectives)
            {
                foreach(Objective objective in activity.DataModel.Objectives)
                {
                    foreach(Objective glob in globalObjectives)
                    {
                        foreach(string str in glob.GlobalObjectiveWriteNormalizedMeasure)
                        {
                            if(objective.GlobalObjectiveReadNormalizedMeasure == str)
                            {
                                return true;
                            }
                        }
                        foreach(string str in glob.GlobalObjectiveWriteSatisfiedStatus)
                        {
                            if(objective.GlobalObjectiveReadSatisfiedStatus == str)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to RB.1.3 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ActivityProgressRollup(Activity activity)
            {
                if(RollupRuleCheck(activity, RollupAction.Incomplete))
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.DataModel.CompletionStatus = CompletionStatus.Incomplete;
                    activity.DataModel.AdvancedAccess = false;
                }
                if(RollupRuleCheck(activity, RollupAction.Completed))
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.DataModel.CompletionStatus = CompletionStatus.Completed;
                    activity.DataModel.AdvancedAccess = false;
                }
                if(activity == m_navigator.RootActivity)
                {
                    m_navigator.CompletionStatus = activity.DataModel.CompletionStatus;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to RB.1.2a in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ObjectiveRollupByMeasure(Activity activity)
            {
                Utilities.Assert(activity.PrimaryObjective != null);

                // If the objective is satisfied by measure, test the rolled-up measure against the defined threshold
                bool objectiveMeasureStatus = activity.PrimaryObjective.ObjectiveMeasureStatus;
                float objectiveNormalizedMeasure = activity.PrimaryObjective.ObjectiveNormalizedMeasure;

                // From SCORM 2004 2nd Edition Addendum, section 2.25, the "read" objective maps are not honored for non-tracked items.
                if(!objectiveMeasureStatus && activity.Sequencing.Tracked)
                {
                    // No measure known, maybe its global
                    objectiveMeasureStatus = m_navigator.ReadGlobalObjectiveNormalizedMeasure(activity.PrimaryObjective.GlobalObjectiveReadNormalizedMeasure, ref objectiveNormalizedMeasure);
                }
                // Still No Measure known, so objective status is unreliable
                if(!objectiveMeasureStatus)
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.PrimaryObjective.SuccessStatus = SuccessStatus.Unknown;
                    activity.DataModel.AdvancedAccess = false;
                }
                else
                {
                    if(!activity.DataModel.ActivityIsActive ||
                        activity.Sequencing.MeasureSatisfactionIfActive)
                    {
                        if(objectiveNormalizedMeasure >= activity.PrimaryObjective.MinNormalizedMeasure)
                        {
                            activity.DataModel.AdvancedAccess = true;
                            activity.PrimaryObjective.SuccessStatus = SuccessStatus.Passed;
                            activity.DataModel.AdvancedAccess = false;
                        }
                        else
                        {
                            activity.DataModel.AdvancedAccess = true;
                            activity.PrimaryObjective.SuccessStatus = SuccessStatus.Failed;
                            activity.DataModel.AdvancedAccess = false;
                        }
                    }
                    else
                    {
                         // Incomplete information, do not evaluate objective status
                        activity.DataModel.AdvancedAccess = true;
                        activity.PrimaryObjective.SuccessStatus = SuccessStatus.Unknown;
                        activity.DataModel.AdvancedAccess = false;
                    }
                }
                if(activity == m_navigator.RootActivity)
                {
                    m_navigator.SuccessStatus = activity.PrimaryObjective.SuccessStatus;
                }
            }

            /// <summary>
            /// default rollup rules
            /// </summary>
            static ReadOnlyCollection<SequencingRollupRuleNodeReader> s_defaultRules;

            /// <summary>
            /// Builds a sequencing node with the default rollup rules, for use in cases where a rollup rule is not
            /// specified.
            /// </summary>
            /// <returns></returns>
            static private ReadOnlyCollection<SequencingRollupRuleNodeReader> GetDefaultRollupRules()
            {
                if(s_defaultRules == null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(
                        "<sequencing xmlns=\"http://www.imsglobal.org/xsd/imsss\">" +
                            "<rollupRules>" +
                                "<rollupRule childActivitySet=\"all\">" +
                                    "<rollupConditions>" +
                                        "<rollupCondition condition=\"completed\"/>" +
                                    "</rollupConditions>" +
                                    "<rollupAction action=\"completed\"/>" +
                                "</rollupRule>" +
                                "<rollupRule childActivitySet=\"all\">" +
                                    "<rollupConditions conditionCombination=\"any\">" +
                                        "<rollupCondition condition=\"attempted\"/>" +
                                        "<rollupCondition condition=\"completed\" operator=\"not\"/>" +
                                    "</rollupConditions>" +
                                    "<rollupAction action=\"incomplete\"/>" +
                                "</rollupRule>" +
                                "<rollupRule childActivitySet=\"all\">" +
                                    "<rollupConditions>" +
                                        "<rollupCondition condition=\"satisfied\"/>" +
                                    "</rollupConditions>" +
                                    "<rollupAction action=\"satisfied\"/>" +
                                "</rollupRule>" +
                                "<rollupRule childActivitySet=\"all\">" +
                                    "<rollupConditions conditionCombination=\"any\">" +
                                        "<rollupCondition condition=\"attempted\"/>" +
                                        "<rollupCondition condition=\"satisfied\" operator=\"not\"/>" +
                                    "</rollupConditions>" +
                                    "<rollupAction action=\"notSatisfied\"/>" +
                                "</rollupRule>" +
                            "</rollupRules>" +
                        "</sequencing>");
                    XPathNavigator nav = doc.CreateNavigator();
                    nav.MoveToChild("sequencing", "http://www.imsglobal.org/xsd/imsss");
                    SequencingNodeReader seq = new SequencingNodeReader(nav, new ManifestReaderSettings(false, false),
                        new PackageValidatorSettings(ValidationBehavior.Enforce, ValidationBehavior.None, ValidationBehavior.None, ValidationBehavior.None), false, null);
                    s_defaultRules = seq.RollupRules;
                }
                return s_defaultRules;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="action"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to RB.1.4 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private bool RollupRuleCheck(Activity activity, RollupAction action)
            {
                // as per bullet #10 on SN-4-35, "Rollup rules have no effect if defined on a leaf activity - there is nothing
                // to rollup.
                if (activity.IsLeaf)
                {
                    return false;
                }
                ReadOnlyCollection<SequencingRollupRuleNodeReader> rules;
                bool useDefault = true;

                foreach(SequencingRollupRuleNodeReader rule in activity.Sequencing.RollupRules)
                {
                    //if(action == rule.Action)
                    //{
                    //    useDefault = false;
                    //    break;
                    //}

                    if(action == RollupAction.NotSatisfied || action == RollupAction.Satisfied)
                    {
                        if(rule.Action == RollupAction.NotSatisfied || rule.Action == RollupAction.Satisfied)
                        {
                            useDefault = false;
                            break;
                        }
                    }
                    else
                    {
                        if(rule.Action == RollupAction.Completed || rule.Action == RollupAction.Incomplete)
                        {
                            useDefault = false;
                            break;
                        }
                    }
                }
                if(useDefault)
                {
                    rules = GetDefaultRollupRules();
                }
                else
                {
                    rules = activity.Sequencing.RollupRules;
                }
                foreach(SequencingRollupRuleNodeReader rule in rules)
                {
                    if(rule.Action == action)
                    {
                        List<bool?> contributingChildren = new List<bool?>();

                        foreach(Activity child in activity.Children)
                        {
                            if(child.Sequencing.Tracked)
                            {
                                 // Make sure this child contributes to the status of its parent
                                if(CheckChildForRollup(child, action))
                                {
                                    contributingChildren.Add(EvaluateRollupConditions(child, rule));
                                }
                            }
                        }
                        // Determine if the appropriate children contributed to rollup; if they did, the status of the activity should be changed
                        bool status = false;
                        int count = 0;
                        switch(rule.ChildActivitySet)
                        {
                        case RollupChildActivitySet.All:
                            status = true;
                            foreach(bool? b in contributingChildren)
                            {
                                if(b != true)
                                {
                                    status = false;
                                    break;
                                }
                            }
                            break;
                        case RollupChildActivitySet.Any:
                            foreach(bool? b in contributingChildren)
                            {
                                if(b == true)
                                {
                                    status = true;
                                    break;
                                }
                            }
                            break;
                        case RollupChildActivitySet.None:
                            status = true;
                            foreach(bool? b in contributingChildren)
                            {
                                if(b == true || b == null)
                                {
                                    status = false;
                                    break;
                                }
                            }
                            break;
                        case RollupChildActivitySet.AtLeastCount:
                            foreach(bool? b in contributingChildren)
                            {
                                if(b == true)
                                {
                                    ++count;
                                }
                            }
                            if(count >= rule.MinimumCount)
                            {
                                status = true;
                            }
                            break;
                        case RollupChildActivitySet.AtLeastPercent:
                            foreach(bool? b in contributingChildren)
                            {
                                if(b == true)
                                {
                                    ++count;
                                }
                            }
                            double percent = 0;
                            if(contributingChildren.Count > 0)
                            {
                                percent = (double)count / (double)contributingChildren.Count;
                            }
                            if(percent >= rule.MinimumPercent)
                            {
                                status = true;
                            }
                            break;
                        }
                        if(status)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="rule"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to RB.1.4.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private bool? EvaluateRollupConditions(Activity activity, SequencingRollupRuleNodeReader rule)
            {
                bool? bRet = null;
                bool firstRule = true;

                foreach(SequencingRollupConditionNodeReader condition in rule.Conditions)
                {
                    bool? b = null;
                    
                    // Evaluate each condition against the activity's tracking information. This evaluation may result in 'unknown'
                    switch(condition.Condition)
                    {
                    case RollupCondition.ActivityProgressKnown:
                        b = activity.DataModel.AttemptProgressStatus;
                        break;
                    case RollupCondition.Attempted:
                        //if(activity.DataModel.ActivityProgressStatus)
                        //{
                            b = (activity.DataModel.ActivityAttemptCount > 0);
                        //}
                        break;
                    case RollupCondition.AttemptLimitExceeded:
                        //if(activity.DataModel.ActivityProgressStatus)
                        //{
                            b = (activity.DataModel.ActivityAttemptCount >= activity.Sequencing.AttemptLimit);
                        //}
                        break;
                    case RollupCondition.Completed:
                        if(activity.DataModel.AttemptProgressStatus)
                        {
                            b = (activity.DataModel.AttemptCompletionStatus);
                        }
                        break;
                    case RollupCondition.ObjectiveMeasureKnown:
                        b = activity.PrimaryObjective.ObjectiveMeasureStatus;
                        break;
                    case RollupCondition.ObjectiveStatusKnown:
                        b = activity.PrimaryObjective.ObjectiveProgressStatus;
                        break;
                    case RollupCondition.OutsideAvailableTimeRange:
                        // not supported
                        b = false;
                        break;
                    case RollupCondition.Satisfied:
                        bool progressStatus;
                        bool satisfiedStatus;
                        GetObjectiveSatisfiedStatus(activity, null, out progressStatus, out satisfiedStatus);
                        if(progressStatus)
                        {
                            b = satisfiedStatus;
                        }
                        break;
                    case RollupCondition.TimeLimitExceeded:
                        // not supported
                        b = false;
                        break;
                    default:
                        Utilities.Assert(false);
                        b = false;
                        break;
                    }
                    if(condition.Operator == SequencingConditionOperator.Not && b.HasValue)
                    {
                        b = !b;
                    }
                    if(rule.ConditionCombination == SequencingConditionCombination.All)
                    {
                        if(firstRule)
                        {
                            firstRule = false;
                            bRet = b;
                        }
                        else
                        {
                            // perform AND as per truth table on page SN-4-36
                            switch(b)
                            {
                            case true:
                                // any value AND with true results in that same value
                                break;
                            case false:
                                // any value AND with false results in false
                                bRet = false;
                                break;
                            case null:
                                // true or unknown AND with unknown results in unknown
                                // false AND with unknown results in false
                                if(bRet != false)
                                {
                                    bRet = null;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if(firstRule)
                        {
                            firstRule = false;
                            bRet = b;
                        }
                        else
                        {
                            // perform OR as per truth table on page SN-4-36
                            switch(b)
                            {
                            case true:
                                // any value OR with true results in true
                                bRet = true;
                                break;
                            case false:
                                // any value OR with false results that same value
                                break;
                            case null:
                                // false or unknown OR with unknown results in unknown
                                // true OR with unknown results in true
                                if(bRet != true)
                                {
                                    bRet = null;
                                }
                                break;
                            }
                        }
                    }
                }
                return bRet;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="action"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to RB.1.4.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private bool CheckChildForRollup(Activity activity, RollupAction action)
            {
                bool included = false;

                if(action == RollupAction.Satisfied || action == RollupAction.NotSatisfied)
                {
                    if(activity.Sequencing.RollupObjectiveSatisfied) // Test the objective rollup control
                    {
                        included = true;    // Default Behavior - adlseq:requiredFor[xxx] == always
                        if((action == RollupAction.Satisfied && activity.Sequencing.RequiredForSatisfied == RollupConsideration.IfNotSuspended) ||
                            (action == RollupAction.NotSatisfied && activity.Sequencing.RequiredForNotSatisfied == RollupConsideration.IfNotSuspended))
                        {
                            if(activity.DataModel.ActivityAttemptCount > 0 && activity.DataModel.ActivityIsSuspended)
                            {
                                included = false;
                            }
                        }
                        else
                        {
                            if((action == RollupAction.Satisfied && activity.Sequencing.RequiredForSatisfied == RollupConsideration.IfAttempted) ||
                                (action == RollupAction.NotSatisfied && activity.Sequencing.RequiredForNotSatisfied == RollupConsideration.IfAttempted))
                            {
                                if(activity.DataModel.ActivityAttemptCount == 0)
                                {
                                    included = false;
                                }
                            }
                            else
                            {
                                if((action == RollupAction.Satisfied && activity.Sequencing.RequiredForSatisfied == RollupConsideration.IfNotSkipped) ||
                                    (action == RollupAction.NotSatisfied && activity.Sequencing.RequiredForNotSatisfied == RollupConsideration.IfNotSkipped))
                                {
                                    if(SequencingRulesCheck(activity, activity.Sequencing.PreConditionRules, SequencingRuleAction.Skip))
                                    {
                                        included = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else if(action == RollupAction.Completed || action == RollupAction.Incomplete)
                {
                     // Test the progress rollup control
                    if(activity.Sequencing.RollupProgressCompletion)
                    {
                        included = true;    // Default Behavior - adlseq:requiredFor[xxx] == always
                        if((action == RollupAction.Completed && activity.Sequencing.RequiredForCompleted == RollupConsideration.IfNotSuspended) ||
                            (action == RollupAction.Incomplete && activity.Sequencing.RequiredForIncomplete == RollupConsideration.IfNotSuspended))
                        {
                            if(activity.DataModel.ActivityAttemptCount > 0 && activity.DataModel.ActivityIsSuspended)
                            {
                                included = false;
                            }
                        }
                        else
                        {
                            if((action == RollupAction.Completed && activity.Sequencing.RequiredForCompleted == RollupConsideration.IfAttempted) ||
                                (action == RollupAction.Incomplete && activity.Sequencing.RequiredForIncomplete == RollupConsideration.IfAttempted))
                            {
                                if(activity.DataModel.ActivityAttemptCount == 0)
                                {
                                    included = false;
                                }
                            }
                            else
                            {
                                if((action == RollupAction.Completed && activity.Sequencing.RequiredForCompleted == RollupConsideration.IfNotSkipped) ||
                                    (action == RollupAction.Incomplete && activity.Sequencing.RequiredForIncomplete == RollupConsideration.IfNotSkipped))
                                {
                                    if(SequencingRulesCheck(activity, activity.Sequencing.PreConditionRules, SequencingRuleAction.Skip))
                                    {
                                        included = false;
                                    }
                                }
                            }
                        }
                    }
                }
                return included;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to RB.1.2b in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ObjectiveRollupByRules(Activity activity)
            {
                // Process all Not Satisfied rules first
                if(RollupRuleCheck(activity, RollupAction.NotSatisfied))
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.PrimaryObjective.SuccessStatus = SuccessStatus.Failed;
                    activity.DataModel.AdvancedAccess = false;
                }
                // Process all Satisfied rules last
                if(RollupRuleCheck(activity, RollupAction.Satisfied))
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.PrimaryObjective.SuccessStatus = SuccessStatus.Passed;
                    activity.DataModel.AdvancedAccess = false;
                }
                if(activity == m_navigator.RootActivity)
                {
                    m_navigator.SuccessStatus = activity.PrimaryObjective.SuccessStatus;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to RB.1.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void MeasureRollup(Activity activity)
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                float totalWeightedMeasure = (float)0.0;
                bool validData = false;
                double countedMeasures = 0.0;

                foreach(Activity child in activity.Children)
                {
                    if(child.Sequencing.Tracked)// Only include tracked children
                    {
                        countedMeasures += child.Sequencing.ObjectiveMeasureWeight;
                        bool objectiveMeasureStatus = child.PrimaryObjective.ObjectiveMeasureStatus;
                        float objectiveNormalizedMeasure = child.PrimaryObjective.ObjectiveNormalizedMeasure;

                        if(!objectiveMeasureStatus)
                        {
                            // No measure known, maybe its global
                            objectiveMeasureStatus = m_navigator.ReadGlobalObjectiveNormalizedMeasure(child.PrimaryObjective.GlobalObjectiveReadNormalizedMeasure, ref objectiveNormalizedMeasure);
                        }
                        if(objectiveMeasureStatus)
                        {
                            totalWeightedMeasure += (float)(objectiveNormalizedMeasure * child.Sequencing.ObjectiveMeasureWeight);
                            validData = true;
                        }
                    }
                }
                if(!validData)
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.PrimaryObjective.Score.Scaled = null;
                    activity.DataModel.AdvancedAccess = false;
                }
                if(countedMeasures > 0.0)
                {
                    activity.DataModel.AdvancedAccess = true;
                    activity.PrimaryObjective.Score.Scaled = (float)(totalWeightedMeasure / countedMeasures);
                    if(activity == m_navigator.RootActivity)
                    {
                        m_navigator.TotalPoints = activity.PrimaryObjective.Score.Scaled.Value * 100;
                    }
                    activity.DataModel.AdvancedAccess = false;
                    m_navigator.LogSequencing(SequencingEventType.Rollup, m_command, Resources.RollupSettingNormalizedMeasure, activity.PrimaryObjective.ObjectiveNormalizedMeasure, activity.Key);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <remarks>Corresponds to DB.1.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ProcessDeliveryRequest(Activity activity)
            {
                if(!activity.IsLeaf)
                {
                    throw new SequencingException(SequencingExceptionCode.DB_1_1__1);
                }

                // Form the activity path as the ordered series of activities from the root of
                // the activity tree to the activity specified in the delivery request, inclusive
                Stack<Activity> activityPath = new Stack<Activity>();

                for(Activity a = activity ; a != null ; a = a.Parent)
                {
                    activityPath.Push(a);
                }
                while(activityPath.Count > 0)
                {
                    Activity a = activityPath.Pop();
                    if(!a.IsLeaf && !a.DataModel.ActivityIsActive && !a.DataModel.ActivityIsSuspended)
                    {
                        foreach(Activity child in a.Children)
                        {
                            ResetAttemptData(child, a.Sequencing.UseCurrentAttemptObjectiveInfo,
                                a.Sequencing.UseCurrentAttemptProgressInfo);
                        }
                        ResetAttemptData(a, a.Sequencing.UseCurrentAttemptObjectiveInfo,
                            a.Sequencing.UseCurrentAttemptProgressInfo);
                    }
                    if(CheckActivity(a))
                    {
                        throw new SequencingException(SequencingExceptionCode.DB_1_1__3);
                    }
                }
            }

            /// <summary>
            /// Does initial processing of navigation requests.
            /// </summary>
            /// <param name="destination"><c>Activity</c> that is the destination of a Choice navigation command.</param>
            /// <param name="seqRequest">Returned sequencing request.</param>
            /// <param name="termRequest">Returned termination request.</param>
            /// <remarks>Corresponds to NB.2.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private void ProcessNavigationRequest(Activity destination, out SequencingRequest? seqRequest, 
                out TerminationRequest? termRequest)
            {
                seqRequest = null;
                termRequest = null;
                switch(m_command)
                {
                case NavigationCommand.Abandon:
                    if(m_navigator.CurrentActivity != null)
                    {
                        if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                        {
                            termRequest = TerminationRequest.Abandon;
                            seqRequest = SequencingRequest.Exit;
                        }
                        else
                        {
                            throw new SequencingException(SequencingExceptionCode.NB_2_1__12);
                        }
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    break;
                case NavigationCommand.AbandonAll:
                    // the check for CurrentActivity != null was removed
                    // so this function should always succeed
                    termRequest = TerminationRequest.AbandonAll;
                    seqRequest = SequencingRequest.Exit;
                    break;
                case NavigationCommand.Choose:
                    if(destination == m_navigator.RootActivity || destination.Parent.Sequencing.Choice)
                    {
                        if(m_navigator.CurrentActivity == null)
                        {
                            seqRequest = SequencingRequest.Choice;
                            return;
                        }
                        // We are always allowed to choose a sibling of the current activity - INCORRECT DESPITE BEING IN THE SCORM PSEUDOCODE
                        // if(destination.Parent != m_navigator.CurrentActivity.Parent)
                        {
                            // The common ancestor will not terminate as a result of processing the choice sequencing request, 
                            // unless the common ancestor is the CurrentActivity - the current activity should always 
                            // be included in the activity path
                            Activity ancestor = FindCommonAncestor(m_navigator.CurrentActivity, destination);
                            Activity final;
                            if(ancestor == m_navigator.CurrentActivity)
                            {
                                final = ancestor.Parent;
                            }
                            else
                            {
                                final = ancestor;
                            }

                            // Make sure that 'choosing' the target will not force an active activity to terminate, 
                            // if that activity does not allow choice to terminate it
                            for(Activity a = m_navigator.CurrentActivity ; a != final ; a = a.Parent)
                            {
                                if(a.DataModel.ActivityIsActive && !a.Sequencing.ChoiceExit)
                                {
                                    throw new SequencingException(SequencingExceptionCode.NB_2_1__8);
                                }
                            }
                        }
                        if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                        {
                            termRequest = TerminationRequest.Exit;
                        }
                        seqRequest = SequencingRequest.Choice;
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__10);
                    }
                    break;
                case NavigationCommand.Continue:
                    if(m_navigator.CurrentActivity == null)
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    // Validate that a 'flow' sequencing request can be processed from the current activity
                    if(m_navigator.CurrentActivity != m_navigator.RootActivity && m_navigator.CurrentActivity.Parent.Sequencing.Flow)
                    {
                        seqRequest = SequencingRequest.Continue;

                        // If the current activity has not been terminated, terminate the current the activity
                        if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                        {
                            termRequest = TerminationRequest.Exit;
                        }
                    }
                    else
                    {
                        // Flow is not enabled or the current activity is the root of the activity tree
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__4);
                    }
                    break;
                case NavigationCommand.ExitAll:
                    if(m_navigator.CurrentActivity != null)
                    {
                        // If the sequencing session has already begun, unconditionally terminate all active activities
                        termRequest = TerminationRequest.ExitAll;
                        seqRequest = SequencingRequest.Exit;
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    break;
                case NavigationCommand.Previous:
                    if(m_navigator.CurrentActivity == null)
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    // Validate that a 'flow' sequencing request can be processed from the current activity
                    if(m_navigator.CurrentActivity != m_navigator.RootActivity)
                    {
                        if(m_navigator.CurrentActivity.Parent.Sequencing.Flow && !m_navigator.CurrentActivity.Parent.Sequencing.ForwardOnly)
                        {
                            seqRequest = SequencingRequest.Previous;

                            // If the current activity has not been terminated, terminate the current the activity
                            if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                            {
                                termRequest = TerminationRequest.Exit;
                            }
                        }
                        else
                        {
                            // Violates control mode
                            throw new SequencingException(SequencingExceptionCode.NB_2_1__5);
                        }
                    }
                    else
                    {
                        // // Cannot move backward from the root of the activity tree
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__6);
                    }
                    break;
                case NavigationCommand.ResumeAll:
                    if(m_navigator.CurrentActivity == null)
                    {
                        if(m_navigator.SuspendedActivity != null)
                        {
                            seqRequest = SequencingRequest.ResumeAll;
                        }
                        else
                        {
                            throw new SequencingException(SequencingExceptionCode.NB_2_1__3);
                        }
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__1);
                    }
                    break;
                case NavigationCommand.Start:
                    if(m_navigator.CurrentActivity != null)
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__1);
                    }
                    seqRequest = SequencingRequest.Start;
                    break;
                case NavigationCommand.SuspendAll:
                    if(m_navigator.CurrentActivity != null)
                    {
                        termRequest = TerminationRequest.SuspendAll;
                        seqRequest = SequencingRequest.Exit;
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    break;
                case NavigationCommand.UnqualifiedExit:
                    if(m_navigator.CurrentActivity == null)
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                    }
                    if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                    {
                        termRequest = TerminationRequest.Exit;
                        seqRequest = SequencingRequest.Exit;
                    }
                    else
                    {
                        throw new SequencingException(SequencingExceptionCode.NB_2_1__12);
                    }
                    break;
                default:
                    throw new LearningComponentsInternalException("SSN0001");
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <param name="considerChildren"></param>
            /// <param name="previousDirection"></param>
            /// <param name="newDirection"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity FlowTreeTraversal(Activity activity, TraversalDirection dir, bool considerChildren, TraversalDirection previousDirection, out TraversalDirection newDirection)
            {
                bool reversed = false;

                // Test if we have skipped all of the children in a forward only cluster moving backward
                if(previousDirection == TraversalDirection.Backward && activity.Parent != null && activity.Next == null)
                {
                    dir = TraversalDirection.Backward;
                    activity = (Activity)activity.Parent.Children[0];
                    reversed = true;
                }
                if(dir == TraversalDirection.Forward)
                {
                    // Cannot walk off the activity tree
                    if(activity.IsLastActivityInTree)
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_1__1);
                    }
                    if(activity.IsLeaf || !considerChildren)
                    {
                        if(activity.Next == null)
                        {
                            // Recursion - Move to the activity's parent's next forward sibling
                            return FlowTreeTraversal(activity.Parent, TraversalDirection.Forward, false, TraversalDirection.NotApplicable, out newDirection);
                        }
                        else
                        {
                            newDirection = dir;
                            return activity.Next;
                        }
                    }
                    else // Entering a cluster - Forward
                    {
                        if(activity.IsLeaf)
                        {
                            throw new SequencingException(SequencingExceptionCode.SB_2_1__2);
                        }
                        ApplyRandomization(activity);
                        newDirection = dir;
                        return (Activity)activity.Children[0];
                    }
                }
                else // direction is backwards
                {
                    Utilities.Assert(dir == TraversalDirection.Backward);

                    if(activity.Parent == null) // Cannot walk off the root of the activity tree
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_1__3);
                    }
                    if(activity.IsLeaf || !considerChildren)
                    {
                        if(!reversed) // Only test 'forward only' if we are not going to leave this forward only cluster.
                        {
                            if(activity.Parent.Sequencing.ForwardOnly) // Test the control mode before traversing
                            {
                                throw new SequencingException(SequencingExceptionCode.SB_2_1__4);
                            }
                        }
                        if(activity.Previous == null)
                        {
                            // Recursion - Move to the activity's parent's next backward sibling
                            return FlowTreeTraversal(activity.Parent, TraversalDirection.Backward, false, TraversalDirection.NotApplicable, out newDirection);
                        }
                        else
                        {
                            newDirection = dir;
                            return activity.Previous;
                        }
                    }
                    else // Entering a cluster - Backward
                    {
                        if(activity.IsLeaf)
                        {
                            throw new SequencingException(SequencingExceptionCode.SB_2_1__2);
                        }
                        ApplyRandomization(activity);
                        if(activity.Sequencing.ForwardOnly)
                        {
                            // Start at the beginning of a forward only cluster
                            newDirection = TraversalDirection.Forward;
                            return (Activity)activity.Children[0];
                        }
                        else
                        {
                            // Start at the end of the cluster if we are backing into it
                            newDirection = TraversalDirection.Backward;
                            return (Activity)activity.Children[activity.Children.Count - 1];
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the normalized measure for the objective referenced.
            /// </summary>
            /// <param name="activity">Activity the objective belongs to.</param>
            /// <param name="objectiveId">The objective Id</param>
            /// <param name="objectiveMeasureStatus">The returned Objective Measure Status.</param>
            /// <param name="objectiveNormalizedMeasure">The returned Objective Normalized Measure.</param>
            /// <remarks>
            /// This code takes into account whether or not the objective actually exists or is just implicit, if 
            /// it is the primary objective or not, and it may refer to global objective information.
            /// </remarks>
            private void GetObjectiveNormalizedMeasure(Activity activity, string objectiveId, 
                out bool objectiveMeasureStatus, out float objectiveNormalizedMeasure)
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                Objective obj;
                if(String.IsNullOrEmpty(objectiveId))
                {
                    obj = activity.PrimaryObjective;
                }
                else if(activity.DataModel.Objectives.Contains(objectiveId))
                {
                    obj = activity.DataModel.Objectives[objectiveId];
                }
                else
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.RequredObjectiveNotFound, objectiveId));
                }
                objectiveMeasureStatus = obj.ObjectiveMeasureStatus;
                objectiveNormalizedMeasure = obj.ObjectiveNormalizedMeasure;

                // From SCORM 2004 2nd Edition Addendum, section 2.25, the "read" objective maps are not honored for non-tracked items.
                if(!objectiveMeasureStatus && activity.Sequencing.Tracked)
                {
                    // No measure known, maybe its global
                    objectiveMeasureStatus = m_navigator.ReadGlobalObjectiveNormalizedMeasure(obj.GlobalObjectiveReadNormalizedMeasure, ref objectiveNormalizedMeasure);
                }
            }

            /// <summary>
            /// Gets the satisfied status for the objective referenced.
            /// </summary>
            /// <param name="activity">Activity the objective belongs to.</param>
            /// <param name="objectiveId">The objective Id</param>
            /// <param name="progressStatus">The returned Objective Progress Status.</param>
            /// <param name="satisfiedStatus">The returned Objective Satisfied Status.</param>
            /// <remarks>
            /// This code takes into account whether or not the objective actually exists or is just implicit, if 
            /// it is the primary objective or not, and it may refer to global objective information.
            /// </remarks>
            private void GetObjectiveSatisfiedStatus(Activity activity, string objectiveId, 
                out bool progressStatus, out bool satisfiedStatus)
            {
                Objective obj;
                if(String.IsNullOrEmpty(objectiveId))
                {
                    obj = activity.PrimaryObjective;
                }
                else if(activity.DataModel.Objectives.Contains(objectiveId))
                {
                    obj = activity.DataModel.Objectives[objectiveId];
                }
                else
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.RequredObjectiveNotFound, objectiveId));
                }
                if(obj.SatisfiedByMeasure)
                {
                    if(!activity.DataModel.ActivityIsActive || activity.Sequencing.MeasureSatisfactionIfActive)
                    {
                        progressStatus = obj.ObjectiveMeasureStatus;
                        satisfiedStatus = (obj.ObjectiveNormalizedMeasure >= obj.MinNormalizedMeasure);
                    }
                    else
                    {
                        progressStatus = false;
                        satisfiedStatus = false;
                    }
                }
                else
                {
                    progressStatus = obj.ObjectiveProgressStatus;
                    satisfiedStatus = obj.ObjectiveSatisfiedStatus;
                }

                // From SCORM 2004 2nd Edition Addendum, section 2.25, the "read" objective maps are not honored for non-tracked items.
                if(!progressStatus && activity.Sequencing.Tracked)
                {
                    // If its satisfied by measure, only read the measure
                    if(obj.SatisfiedByMeasure)
                    {
                        if(!activity.DataModel.ActivityIsActive || activity.Sequencing.MeasureSatisfactionIfActive)
                        {
                            float measure = 0;
                            if(m_navigator.ReadGlobalObjectiveNormalizedMeasure(obj.GlobalObjectiveReadNormalizedMeasure, ref measure))
                            {
                                progressStatus = true;
                                if(measure >= obj.MinNormalizedMeasure)
                                {
                                    satisfiedStatus = true;
                                }
                            }
                            else
                            {
                                progressStatus = false;
                                satisfiedStatus = false;
                            }
                        }
                    }
                    else
                    {
                        progressStatus = m_navigator.ReadGlobalObjectiveSatisfiedStatus(obj.GlobalObjectiveReadSatisfiedStatus, ref satisfiedStatus);
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="rule"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to UP.2.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private bool SequencingRuleCheck(Activity activity, SequencingRuleNodeReader rule) // [UP.2.1] (for an activity and a Sequencing Rule; returns True if the rule applies, False if the rule does not apply, and Unknown if the condition(s) cannot be evaluated):
            {
                bool? bRet = null;
                bool firstRule = true;

                // Evaluate each condition against the activity's tracking information
                foreach(SequencingRuleConditionNodeReader condition in rule.Conditions)
                {
                    bool? b = null;
                    bool objectiveMeasureStatus;
                    float objectiveNormalizedMeasure;
                    bool progressStatus;
                    bool satisfiedStatus;

                    switch(condition.Condition)
                    {
                    case SequencingRuleCondition.Always:
                        b = true;
                        break;
                    case SequencingRuleCondition.Satisfied:
                        GetObjectiveSatisfiedStatus(activity, condition.ReferencedObjectiveId, out progressStatus, out satisfiedStatus);
                        if(progressStatus)
                        {
                            b = satisfiedStatus;
                        }
                        break;
                    case SequencingRuleCondition.ObjectiveStatusKnown:
                        GetObjectiveSatisfiedStatus(activity, condition.ReferencedObjectiveId, out progressStatus, out satisfiedStatus);
                        b = progressStatus;
                        break;
                    case SequencingRuleCondition.ObjectiveMeasureKnown:
                        GetObjectiveNormalizedMeasure(activity, condition.ReferencedObjectiveId, out objectiveMeasureStatus, out objectiveNormalizedMeasure);
                        b = objectiveMeasureStatus;
                        break;
                    case SequencingRuleCondition.ObjectiveMeasureGreaterThan:
                        GetObjectiveNormalizedMeasure(activity, condition.ReferencedObjectiveId, out objectiveMeasureStatus, out objectiveNormalizedMeasure);
                        if(objectiveMeasureStatus)
                        {
                            b = (objectiveNormalizedMeasure > (float)condition.MeasureThreshold);
                        }
                        break;
                    case SequencingRuleCondition.ObjectiveMeasureLessThan:
                        GetObjectiveNormalizedMeasure(activity, condition.ReferencedObjectiveId, out objectiveMeasureStatus, out objectiveNormalizedMeasure);
                        if(objectiveMeasureStatus)
                        {
                            b = (objectiveNormalizedMeasure < (float)condition.MeasureThreshold);
                        }
                        break;
                    case SequencingRuleCondition.Completed:
                        if(activity.DataModel.AttemptProgressStatus)
                        {
                            b = activity.DataModel.AttemptCompletionStatus;
                        }
                        break;
                    case SequencingRuleCondition.ActivityProgressKnown:
                        b = activity.DataModel.ActivityProgressStatus & activity.DataModel.AttemptProgressStatus;
                        break;
                    case SequencingRuleCondition.Attempted:
                        //if(activity.DataModel.ActivityProgressStatus)
                        //{
                            b = (activity.DataModel.ActivityAttemptCount > 0);
                        //}
                        break;
                    case SequencingRuleCondition.AttemptLimitExceeded:
                        //if(activity.DataModel.ActivityProgressStatus)
                        //{
                            b = (activity.DataModel.ActivityAttemptCount >= activity.Sequencing.AttemptLimit);
                        //}
                        break;
                    case SequencingRuleCondition.OutsideAvailableTimeRange:
                        // not supported
                        b = false;
                        break;
                    case SequencingRuleCondition.TimeLimitExceeded:
                        // not supported
                        b = false;
                        break;
                    default:
                        Utilities.Assert(false);
                        b = false;
                        break;
                    }
                    if(condition.Operator == SequencingConditionOperator.Not && b.HasValue)
                    {
                        b = !b;
                    }
                    if(rule.Combination == SequencingConditionCombination.All)
                    {
                        if(firstRule)
                        {
                            firstRule = false;
                            bRet = b;
                        }
                        else
                        {
                            // perform AND as per truth table on page SN-4-36
                            switch(b)
                            {
                            case true:
                                // any value AND with true results in that same value
                                break;
                            case false:
                                // any value AND with false results in false
                                bRet = false;
                                break;
                            case null:
                                // true or unknown AND with unknown results in unknown
                                // false AND with unknown results in false
                                if(bRet != false)
                                {
                                    bRet = null;
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if(firstRule)
                        {
                            firstRule = false;
                            bRet = b;
                        }
                        else
                        {
                            // perform OR as per truth table on page SN-4-36
                            switch(b)
                            {
                            case true:
                                // any value OR with true results in true
                                bRet = true;
                                break;
                            case false:
                                // any value OR with false results that same value
                                break;
                            case null:
                                // false or unknown OR with unknown results in unknown
                                // true OR with unknown results in true
                                if(bRet != true)
                                {
                                    bRet = null;
                                }
                                break;
                            }
                        }
                    }
                }
                return (bRet == true);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="rules"></param>
            /// <param name="ruleAction"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to UP.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private bool SequencingRulesCheck(Activity activity, ICollection<SequencingRuleNodeReader> rules, SequencingRuleAction ruleAction)
            {
                foreach(SequencingRuleNodeReader rule in rules)
                {
                    if(rule.Action == ruleAction)
                    {
                        // Evaluate each rule, one at a time
                        if(SequencingRuleCheck(activity, rule))
                        {
                            // Stop at the first rule that evaluates to true - perform the associated action
                            return true;
                        }
                    }
                }
                // No rules evaluated to true - do not perform any action
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="rules"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to UP.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private SequencingRuleAction? SequencingRulesCheck(Activity activity, ICollection<SequencingRuleNodeReader> rules)
            {
                foreach(SequencingRuleNodeReader rule in rules)
                {
                    // Evaluate each rule, one at a time
                    if(SequencingRuleCheck(activity, rule))
                    {
                        // Stop at the first rule that evaluates to true - perform the associated action
                        return rule.Action;
                    }
                }
                // No rules evaluated to true - do not perform any action
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to UP.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private static bool LimitConditionsCheck(Activity activity)
            {
                if(!activity.Sequencing.Tracked)
                {
                    // If the activity is not tracked, its limit conditions cannot be violated
                    return false;
                }
                if(activity.DataModel.ActivityIsActive || activity.DataModel.ActivityIsSuspended)
                {
                    // Only need to check activities that will begin a new attempt
                    return false;
                }
                if(activity.Sequencing.AttemptLimit != null &&
                    activity.DataModel.ActivityAttemptCount >= activity.Sequencing.AttemptLimit)
                {
                    return true;
                }
                /*
    // BEGIN OPTIONAL CODE

    4. 		If the Limit Condition Activity Absolute Duration Control for the activity is True Then
    4.1. 		If the Activity Progress Status for the activity is True And the Activity Absolute Duration for the activity is greater than or equal (>=) to Limit Condition Activity Absolute Duration Limit for the activity Then
    4.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If
    5. 		If the Limit Condition Activity Experienced Duration Control for the activity is True Then
    5.1. 		If the Activity Progress Status for the activity is True And the Activity Experienced Duration for the activity is greater than or equal (>=) to the Limit Condition Activity Experienced Duration Limit for the activity Then
    5.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If
    6. 		If the Limit Condition Attempt Absolute Duration Control for the activity is True Then
    6.1. 		If the Activity Progress Status for the activity is True And the Attempt Progress Status for the activity is True And the Attempt Absolute Duration for the activity is greater than or equal (>=) to the Limit Condition Attempt Absolute Duration Limit for the activity Then
    6.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If
    7. 		If the Limit Condition Attempt Experienced Duration Control for the activity is True Then
    7.1. 		If the Activity Progress Status for the activity is True And the Attempt Progress Status for the activity is True And the Attempt Experienced Duration for the activity is greater than or equal (>=) to the Limit Condition Attempt Experienced Duration Limit for the activity Then
    7.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If
    8. 		If the Limit Condition Begin Time Limit Control for the activity is True Then
    8.1. 		If the current time point is before the Limit Condition Begin Time Limit for the activity Then
    8.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If
    9. 		If the Limit Condition End Time Limit Control for the activity is True Then
    9.1. 		If the current time point is after the Limit Condition End Time Limit for the activity Then
    9.1.1. 			Exit Limit Conditions Check Process (Limit Condition Violated: True) // Limit conditions have been violated
                End If
            End If

    // END OPTIONAL CODE
                 */
                return false;
            }

            /// <summary>
            /// Returns True if the activity is disabled or violates any of its limit conditions
            /// </summary>
            /// <param name="activity"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to UP.5 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private bool CheckActivity(Activity activity)
            {
                if(SequencingRulesCheck(activity, activity.Sequencing.PreConditionRules, SequencingRuleAction.Disabled))
                {
                    return true;
                }
                if(LimitConditionsCheck(activity))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <param name="previousDirection"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity FlowActivityTraversal(Activity activity, TraversalDirection dir, TraversalDirection previousDirection)
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(!activity.Parent.Sequencing.Flow)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_2__1);
                }
                if(SequencingRulesCheck(activity, activity.Sequencing.PreConditionRules, SequencingRuleAction.Skip))
                {
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingPreConditionSkipped, activity.Key);
                    TraversalDirection newDirection;
                    Activity newActivity = FlowTreeTraversal(activity, dir, false, previousDirection, out newDirection);
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingFlowingTo, newActivity.Key);

                    // Make sure the recursive call is considers the correct direction
                    if(previousDirection == TraversalDirection.Backward && newDirection == TraversalDirection.Backward)
                    {
                        // Recursive call - make sure the "next" activity is OK
                        newActivity = FlowActivityTraversal(newActivity, newDirection, TraversalDirection.NotApplicable);
                    }
                    else
                    {
                        // Recursive call - make sure the "next" activity is OK
                        newActivity = FlowActivityTraversal(newActivity, newDirection, previousDirection);
                    }
                    return newActivity;
                }
                if(CheckActivity(activity))
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_2__2);
                }
                if(!activity.IsLeaf)
                {
                    TraversalDirection newDirection;
                    Activity newActivity = FlowTreeTraversal(activity, dir, true, TraversalDirection.NotApplicable, out newDirection);
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingFlowingTo, newActivity.Key);

                    // Check if we are flowing backward through a forward only cluster - must move forward instead
                    if(dir == TraversalDirection.Backward && newDirection == TraversalDirection.Forward)
                    {
                        // Recursive call - make sure the "next" activity is OK
                        newActivity = FlowActivityTraversal(newActivity, TraversalDirection.Forward, TraversalDirection.Backward);
                    }
                    else
                    {
                        // Recursive call - make sure the "next" activity is OK
                        newActivity = FlowActivityTraversal(newActivity, dir, TraversalDirection.NotApplicable);
                    }
                    return newActivity;
                }
                return activity;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <param name="considerChildren"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.3 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity Flow(Activity activity, TraversalDirection dir, bool considerChildren)
            {
                TraversalDirection newDirection;
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                activity = FlowTreeTraversal(activity, dir, considerChildren, TraversalDirection.NotApplicable, out newDirection);
                m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, m_command, Resources.SequencingFlowingTo, activity.Key);
                return FlowActivityTraversal(activity, dir, TraversalDirection.NotApplicable);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.5 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity StartSequencingRequest()
            {
                // Make sure the sequencing session has not already begun
                if(m_navigator.CurrentActivity != null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_5__1);
                }
                if(m_navigator.RootActivity.IsLeaf)
                {
                    return m_navigator.RootActivity;
                }
                return Flow(m_navigator.RootActivity, TraversalDirection.Forward, true);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.7 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity ContinueSequencingRequest()
            {
                // Make sure the sequencing session has already begun
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_7__1);
                }
                if(m_navigator.CurrentActivity != m_navigator.RootActivity)
                {
                    // Confirm a 'flow' traversal is allowed from the activity
                    if(!m_navigator.CurrentActivity.Parent.Sequencing.Flow)
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_7__2);
                    }
                }
                // Flow in a forward direction to the next allowed activity
                return Flow(m_navigator.CurrentActivity, TraversalDirection.Forward, false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.8 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity PreviousSequencingRequest()
            {
                // Make sure the sequencing session has already begun
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_8__1);
                }
                if(m_navigator.CurrentActivity != m_navigator.RootActivity)
                {
                    // Confirm a 'flow' traversal is allowed from the activity
                    if(!m_navigator.CurrentActivity.Parent.Sequencing.Flow)
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_8__2);
                    }
                }
                // Flow in a forward direction to the next allowed activity
                return Flow(m_navigator.CurrentActivity, TraversalDirection.Backward, false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.11 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private bool ExitSequencingRequest()
            {
                // Make sure the sequencing session has already begun
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_11__1);
                }
                // Make sure the current activity has already been terminated
                if(m_navigator.CurrentActivity.DataModel.ActivityIsActive)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_11__2);
                }
                if(m_navigator.CurrentActivity == m_navigator.RootActivity)
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.6 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity ResumeAllSequencingRequest()
            {
                // Make sure the sequencing session has not already begun
                if(m_navigator.CurrentActivity != null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_6__1);
                }
                if(m_navigator.SuspendedActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_6__2);
                }
                return m_navigator.SuspendedActivity;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.10 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private Activity RetrySequencingRequest()
            {
                // Make sure the sequencing session has already begun
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_10__1);
                }
                // Cannot retry an activity that is still active or suspended
                if(m_navigator.CurrentActivity.DataModel.ActivityIsActive || m_navigator.CurrentActivity.DataModel.ActivityIsSuspended)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_10__2);
                }
                if(!m_navigator.CurrentActivity.IsLeaf)
                {
                    return Flow(m_navigator.CurrentActivity, TraversalDirection.Forward, true);
                }
                return m_navigator.CurrentActivity;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <remarks>Corresponds to SB.2.4 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ChoiceActivityTraversal(Activity activity, TraversalDirection dir)
            {
                if(dir == TraversalDirection.Forward)
                {
                    if(SequencingRulesCheck(activity, activity.Sequencing.PreConditionRules, SequencingRuleAction.StopForwardTraversal))
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_4__1);
                    }
                }
                else
                {
                    Utilities.Assert(dir == TraversalDirection.Backward);
                    if(activity.Parent != null)
                    {
                        if(activity.Parent.Sequencing.ForwardOnly)
                        {
                            throw new SequencingException(SequencingExceptionCode.SB_2_4__2);
                        }
                    }
                    else
                    {
                        // Cannot walk backward from the root of the activity tree
                        throw new SequencingException(SequencingExceptionCode.SB_2_4__3);
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.9.1 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            Activity ChoiceFlow(Activity activity, TraversalDirection dir)
            {
                Activity a = ChoiceFlowTreeTraversal(activity, dir);
                if(a == null)
                {
                    return activity;
                }
                return a;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activity"></param>
            /// <param name="dir"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.9.2 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            Activity ChoiceFlowTreeTraversal(Activity activity, TraversalDirection dir)
            {
                if(dir == TraversalDirection.Forward)
                {
                    if(activity.IsLastActivityInTree)
                    {
                        return null;
                    }
                    if(activity.Next == null)
                    {
                        if(activity.Parent == null)
                        {
                            return null;
                        }
                        return ChoiceFlowTreeTraversal(activity.Parent, TraversalDirection.Forward);
                    }
                    return activity.Next;
                }
                else
                {
                    Utilities.Assert(dir == TraversalDirection.Backward);
                    if(activity == m_navigator.RootActivity)
                    {
                        return null;
                    }
                    if(activity.Previous == null)
                    {
                        return ChoiceFlowTreeTraversal(activity.Parent, TraversalDirection.Backward);
                    }
                    return activity.Previous;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destination"></param>
            /// <returns></returns>
            /// <remarks>Corresponds to SB.2.9 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            [SuppressMessage("Microsoft.Maintainability", "CA1502")]
            private Activity ChoiceSequencingRequest(Activity destination)
            {
                // Form the activity path as the ordered series of activities from the root of the activity tree 
                // to the target activity, inclusive
                Stack<Activity> activityList = new Stack<Activity>();
                TraversalDirection dir;

                for(Activity a = destination ; a != null ; a = a.Parent)
                {
                    activityList.Push(a);
                }
                while(activityList.Count > 0)
                {
                    Activity a = activityList.Pop();
                    if(SequencingRulesCheck(a, a.Sequencing.PreConditionRules, SequencingRuleAction.HiddenFromChoice))
                    {
                        throw new SequencingException(SequencingExceptionCode.SB_2_9__3);
                    }
                }
                if(destination != m_navigator.RootActivity && !destination.Parent.Sequencing.Choice)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_9__4);
                }
                
                Activity ancestor;

                if(m_navigator.CurrentActivity == null)
                {
                    ancestor = m_navigator.RootActivity;
                }
                else
                {
                    ancestor = FindCommonAncestor(m_navigator.CurrentActivity, destination);
                }

                // Case #1 - select the current activity
                if(destination == m_navigator.CurrentActivity)
                {
                    // Nothing to do in this case
                    return destination;
                }
                // Case #2 - same cluster; move toward the target activity
                else if(m_navigator.CurrentActivity != null && destination.Parent == m_navigator.CurrentActivity.Parent)
                {
                    int from = destination.Parent.Children.IndexOf(m_navigator.CurrentActivity);
                    int to = destination.Parent.Children.IndexOf(destination);
                    int incr;

                    if(from > to)
                    {
                        dir = TraversalDirection.Backward;
                        incr = -1;
                    }
                    else
                    {
                        dir = TraversalDirection.Forward;
                        incr = 1;
                    }
                    // Form the activity path as the ordered series of activities from the common ancestor 
                    // to the target activity, exclusive of the target activity
                    for(int i = from ; i != to ; i += incr)
                    {
                        ChoiceActivityTraversal((Activity)destination.Parent.Children[i], dir);
                    }
                }
                // Case #3 - path to the target is forward in the activity tree
                else if(m_navigator.CurrentActivity == null || m_navigator.CurrentActivity == ancestor)
                {
                    // Form the activity path as the ordered series of activities from the common ancestor 
                    // to the target activity, exclusive of the target activity
                    for(Activity a = destination.Parent ; a != ancestor.Parent ; a = a.Parent)
                    {
                        activityList.Push(a);
                    }
                    while(activityList.Count > 0)
                    {
                        Activity a = activityList.Pop();
                        ChoiceActivityTraversal(a, TraversalDirection.Forward);
                        // If the activity being considered is not already active, make sure we are allowed to activate it
                        if(!a.DataModel.ActivityIsActive && a != ancestor && a.Sequencing.PreventActivation)
                        {
                            throw new SequencingException(SequencingExceptionCode.SB_2_9__6);
                        }
                    }
                }
                // Case #4 - path to the target is backward in the activity tree
                else if(destination == ancestor)
                {
                    // Form the activity path as the ordered series of activities from the CurrentActivity 
                    // to the target activity, exclusive of the target activity
                    if(m_navigator.CurrentActivity != null)
                    {
                        for(Activity a = m_navigator.CurrentActivity ; a != ancestor ; a = a.Parent)
                        {
                            // Make sure an activity that should not exit will exit if the target is delivered.
                            if(!a.Sequencing.ChoiceExit)
                            {
                                throw new SequencingException(SequencingExceptionCode.SB_2_9__7);
                            }
                        }
                    }
                }
                // Case #5 - target is a descendant activity of the common ancestor
                else
                {
                    Activity constrained = null;

                    // Walk up the tree to the common ancestor
                    for(Activity a = m_navigator.CurrentActivity ; a != ancestor.Parent ; a = a.Parent)
                    {
                        if(a != ancestor && !a.Sequencing.ChoiceExit)
                        {
                            throw new SequencingException(SequencingExceptionCode.SB_2_9__7);
                        }
                        // Find the closest constrained activity to the current activity
                        if(constrained == null)
                        {
                            if(a.Sequencing.ConstrainChoice)
                            {
                                constrained = a;
                            }
                        }
                    }
                    if(constrained != null && destination != constrained)
                    {
                        dir = TraversalDirection.NotApplicable;

                        foreach(Activity temp in m_navigator.Traverse)
                        {
                            if(temp == constrained)
                            {
                                dir = TraversalDirection.Forward;
                                break;
                            }
                            else if(temp == destination)
                            {
                                dir = TraversalDirection.Backward;
                                break;
                            }
                        }
                        Utilities.Assert(dir != TraversalDirection.NotApplicable);
                        Activity consider = ChoiceFlow(constrained, dir);
                        // Make sure the target activity is within the set of 'flow' constrained choices
                        // If the target activity is Not an available descendent of the activity to consider And the target activity is Not the activity to considered And the target activity is Not the constrained activity Then
                        if(destination != consider)
                        {
                            Activity a;
                            for(a = destination.Parent ; a != null ; a = a.Parent)
                            {
                                if(a == consider)
                                {
                                    break;
                                }
                            }
                            if(a == null)  // destination is not a child of the constrained activity
                            {
                                throw new SequencingException(SequencingExceptionCode.SB_2_9__8);
                            }
                        }
                    }
                    // Form the activity path as the ordered series of activities from the common ancestor 
                    // to the target activity, exclusive of the target activity
                    for(Activity a = destination.Parent ; a != ancestor.Parent ; a = a.Parent)
                    {
                        activityList.Push(a);
                    }

                    // If the target activity is forward in the activity tree 
                    // relative to the CurrentActivity

                    dir = TraversalDirection.NotApplicable;

                    foreach(Activity temp in m_navigator.Traverse)
                    {
                        if(temp == m_navigator.CurrentActivity)
                        {
                            dir = TraversalDirection.Forward;
                            break;
                        }
                        else if(temp == destination)
                        {
                            dir = TraversalDirection.Backward;
                            break;
                        }
                    }
                    Utilities.Assert(dir != TraversalDirection.NotApplicable);

                    if(dir == TraversalDirection.Forward)
                    {
                        while(activityList.Count > 0)
                        {
                            Activity a = activityList.Pop();
                            ChoiceActivityTraversal(a, dir);
                            if(a != ancestor && !a.DataModel.ActivityIsActive && a.Sequencing.PreventActivation)
                            {
                                throw new SequencingException(SequencingExceptionCode.SB_2_9__6);
                            }
                        }
                    }
                    else
                    {
                        while(activityList.Count > 0)
                        {
                            Activity a = activityList.Pop();
                            if(a != ancestor && !a.DataModel.ActivityIsActive && a.Sequencing.PreventActivation)
                            {
                                throw new SequencingException(SequencingExceptionCode.SB_2_9__6);
                            }
                        }
                    }
                }
                if(destination.IsLeaf)
                {
                    return destination;
                }
                return Flow(destination, TraversalDirection.Forward, true);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="seqRequest"></param>
            /// <param name="destination"></param>
            /// <param name="deliveryRequest"></param>
            /// <param name="exitSession"></param>
            /// <remarks>Corresponds to SB.2.12 in the SCORM 2004 Sequencing/Navigation manual, appendix C.</remarks>
            private void ProcessSequencingRequest(SequencingRequest seqRequest, Activity destination, out Activity deliveryRequest, out bool exitSession)
            {
                switch(seqRequest)
                {
                case SequencingRequest.Choice:
                    deliveryRequest = ChoiceSequencingRequest(destination);
                    exitSession = false;
                    return;
                case SequencingRequest.Continue:
                    deliveryRequest = ContinueSequencingRequest();
                    exitSession = false;
                    return;
                case SequencingRequest.Exit:
                    deliveryRequest = null;
                    exitSession = ExitSequencingRequest();
                    return;
                case SequencingRequest.Previous:
                    deliveryRequest = PreviousSequencingRequest();
                    exitSession = false;
                    return;
                case SequencingRequest.Retry:
                    deliveryRequest = RetrySequencingRequest();
                    exitSession = false;
                    return;
                case SequencingRequest.ResumeAll:
                    deliveryRequest = ResumeAllSequencingRequest();
                    exitSession = false;
                    return;
                case SequencingRequest.Start:
                    deliveryRequest = StartSequencingRequest();
                    exitSession = false;
                    return;
                }
                Utilities.Assert(false);
                deliveryRequest = null;
                exitSession = false;
                return;
            }
        }
    }
}
