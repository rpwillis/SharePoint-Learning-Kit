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
    internal abstract partial class Navigator
    {
        /// <summary>
        /// Class that handles SCORM 1.2 sequencing and navigation.
        /// </summary>
        private class Scorm12SeqNav : SeqNav
        {
            /// <summary>
            /// Initializes the class that performs sequencing/navigation using SCORM 1.2004 sequencing/navigation rules.
            /// </summary>
            /// <param name="navigator">The <Typ>Navigator</Typ> performing the sequencing operation.</param>
            public Scorm12SeqNav(NavigatorData navigator) : base(navigator)
            {
            }

            /// <summary>
            /// Returns whether or not the activity is a valid target for navigation.
            /// </summary>
            /// <param name="activity">The activity to check</param>
            /// <returns>True if the activity is a valid target for navigation, false otherwise.</returns>
            static private bool IsValidToNavigateTo(Activity activity)
            {
                return !String.IsNullOrEmpty(activity.ResourceKey);
            }

            /// <summary>
            /// Performs the Abandon navigation command
            /// </summary>
            private void Abandon()
            {
                if(m_navigator.CurrentActivity != null)
                {
                    ExitActivity(m_navigator.CurrentActivity);
                    m_navigator.CurrentActivity = null;
                }
                else
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
            }

            /// <summary>
            /// Performs the AbandonAll navigation command
            /// </summary>
            private void AbandonAll()
            {
                // the check for CurrentActivity != null was removed by request
                // so this function should always succeed
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                ExitActivity(m_navigator.CurrentActivity);
                ExitActivity(m_navigator.SuspendedActivity);
                m_navigator.CurrentActivity = null;
                m_navigator.SuspendedActivity = null;
                m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.AbandonAll, Resources.NavigationFinishedDone);
            }

            /// <summary>
            /// Performs the Choose navigation command
            /// </summary>
            /// <param name="destination">The destination activity requested.</param>
            private void Choose(Activity destination)
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(IsValidToNavigateTo(destination))
                {
                    ExitActivity(m_navigator.CurrentActivity);
                    DeliverActivity(destination);
                    m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.Choose, Resources.NavigationFinishedNewActivity, destination.Key);
                }
                else
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__10);
                }
            }

            /// <summary>
            /// Performs the Continue navigation command
            /// </summary>
            private void Continue()
            {
                Activity a;
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
                for(a = m_navigator.CurrentActivity.NextInPreorderTraversal ; a != null && !IsValidToNavigateTo(a) ; a = a.NextInPreorderTraversal)
                {
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, NavigationCommand.Continue, Resources.SequencingFlowingTo, a.Key);
                }
                if(a == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_1__1);
                }
                ExitActivity(m_navigator.CurrentActivity);
                DeliverActivity(a);
                m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.Continue, Resources.NavigationFinishedNewActivity, a.Key);
            }

            /// <summary>
            /// Performs the ExitAll navigation command
            /// </summary>
            private void ExitAll()
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
                ExitActivity(m_navigator.CurrentActivity);
                ExitActivity(m_navigator.SuspendedActivity);
                m_navigator.CurrentActivity = null;
                m_navigator.SuspendedActivity = null;
                m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.ExitAll, Resources.NavigationFinishedDone);
            }

            /// <summary>
            /// Performs the Previous navigation command
            /// </summary>
            private void Previous()
            {
                Activity a;
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
                for(a = m_navigator.CurrentActivity.PreviousInPreorderTraversal ; a != null && !IsValidToNavigateTo(a) ; a = a.PreviousInPreorderTraversal)
                {
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, NavigationCommand.Previous, Resources.SequencingFlowingTo, a.Key);
                }
                if(a == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_1__3);
                }
                ExitActivity(m_navigator.CurrentActivity);
                DeliverActivity(a);
                m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.Previous, Resources.NavigationFinishedNewActivity, a.Key);
            }

            /// <summary>
            /// Performs the ResumeAll navigation command
            /// </summary>
            private void ResumeAll()
            {
                if(m_navigator.CurrentActivity == null)
                {
                    if(m_navigator.SuspendedActivity != null)
                    {
                        DeliverActivity(m_navigator.SuspendedActivity);
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
            }

            /// <summary>
            /// Performs the Start navigation command
            /// </summary>
            private void Start()
            {
                Resources.Culture = LocalizationManager.GetCurrentCulture();
                if(m_navigator.CurrentActivity != null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__1);
                }
                foreach(Activity act in m_navigator.Traverse)
                {
                    m_navigator.LogSequencing(SequencingEventType.IntermediateNavigation, NavigationCommand.Start, Resources.SequencingFlowingTo, act.Key);
                    if(IsValidToNavigateTo(act))
                    {
                        DeliverActivity(act);
                        m_navigator.LogSequencing(SequencingEventType.FinalNavigation, NavigationCommand.Start, Resources.NavigationFinishedNewActivity, act.Key);
                        break;
                    }
                }
            }

            /// <summary>
            /// Performs the SuspendAll navigation command
            /// </summary>
            private void SuspendAll()
            {
                if(m_navigator.CurrentActivity != null)
                {
                    ExitActivity(m_navigator.CurrentActivity);
                    m_navigator.CurrentActivity.DataModel.ActivityIsSuspended = true;
                    m_navigator.SuspendedActivity = m_navigator.CurrentActivity;
                    m_navigator.CurrentActivity = null;
                }
                else
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
            }

            /// <summary>
            /// Performs the UnqualifiedExit navigation command
            /// </summary>
            private void UnqualifiedExit()
            {
                if(m_navigator.CurrentActivity == null)
                {
                    throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
                }
                ExitActivity(m_navigator.CurrentActivity);
                m_navigator.CurrentActivity = null;
            }

            /// <summary>
            /// Performs the requested sequencing/navigation using SCORM 1.2 sequencing/navigation rules.
            /// </summary>
            /// <param name="command">The navigation command to perform.</param>
            /// <param name="destination"><c>Activity</c> that is the destination of a Choice navigation command.</param>
            /// <returns>True if the sequencing session has ended.</returns>
            /// <exception cref="SequencingException">Occurs when there is an invalid navigation performed.</exception>
            public override bool OverallSequencingProcess(NavigationCommand command, Activity destination)
            {
                switch(command)
                {
                case NavigationCommand.Abandon:
                    Abandon();
                    break;
                case NavigationCommand.AbandonAll:
                    AbandonAll();
                    return true;
                case NavigationCommand.Choose:
                    Choose(destination);
                    break;
                case NavigationCommand.Continue:
                    Continue();
                    break;
                case NavigationCommand.ExitAll:
                    ExitAll();
                    return true;
                case NavigationCommand.Previous:
                    Previous();
                    break;
                case NavigationCommand.ResumeAll:
                    ResumeAll();
                    break;
                case NavigationCommand.Start:
                    Start();
                    break;
                case NavigationCommand.SuspendAll:
                    SuspendAll();
                    return true;
                case NavigationCommand.UnqualifiedExit:
                    UnqualifiedExit();
                    break;
                default:
                    throw new LearningComponentsInternalException("SSN0001");
                }
                return false;
            }

            private LessonStatus GetLessonStatus(string identifier)
            {
                Activity activity = m_navigator.ActivityKeyToActivity(identifier);
                Utilities.Assert(activity != null, "S2N0001");
                return activity.DataModel.LessonStatus;
            }

            /// <summary>
            /// Performs actions necessary to make an activity 'active'
            /// </summary>
            /// <param name="activity">The activity to activate.  Null is not allowed.</param>
            private void DeliverActivity(Activity activity)
            {
                if(!Microsoft.LearningComponents.Manifest.Helper.PrerequisitesParser.Evaluate(activity.Prerequisites, GetLessonStatus))
                {
                    throw new SequencingException(SequencingExceptionCode.DB_1_1__3);
                }
                m_navigator.CurrentActivity = activity;
                m_navigator.SuspendedActivity = null;

                m_navigator.UpdateActivityData(activity);  // load data model information from database, if needed
                if(activity.DataModel.ActivityIsSuspended)
                {
                    activity.DataModel.ActivityIsSuspended = false;
                    activity.DataModel.InitializeForDeliveryAfterSuspend();
                }
                else
                {
                    ++activity.DataModel.ActivityAttemptCount; // need to add to ActivityAttemptCount before calling InitializeEntry
                    activity.DataModel.InitializeForDelivery();
                }
                activity.DataModel.ActivityIsActive = true;
            }

            /// <summary>
            /// Performs actions necessary to make an activity inactive
            /// </summary>
            /// <param name="activity">Activity to deactivate.  Null is handled.</param>
            private static void ExitActivity(Activity activity)
            {
                if(activity != null)
                {
                    bool save = activity.DataModel.AdvancedAccess;
                    activity.DataModel.AdvancedAccess = true;
                    // the SCORM 1.2 spec Run Time Environment page 3-25 says that the LMS 
                    // should always overwrite the lessonstatus to Completed, however the page before
                    // says it should never overwrite it if it was set by the SCO.  I chose the latter.
                    if(activity.DataModel.LessonStatus == LessonStatus.NotAttempted)
                    {
                        activity.DataModel.LessonStatus = LessonStatus.Completed;
                    }

                    // set to passed or failed if there is a mastery score, a score, and credit is true
                    if(activity.DataModel.Credit && activity.DataModel.MasteryScore.HasValue && activity.DataModel.Score.Raw.HasValue)
                    {
                        if(activity.DataModel.Score.Raw.Value >= activity.DataModel.MasteryScore)
                        {
                            activity.DataModel.LessonStatus = LessonStatus.Passed;
                        }
                        else
                        {
                            activity.DataModel.LessonStatus = LessonStatus.Failed;
                        }
                    }
                    //activity.DataModel.ActivityIsSuspended = false;
                    activity.DataModel.ActivityIsActive = false;
                    activity.DataModel.TotalTime += activity.DataModel.SessionTime;
                    activity.DataModel.SessionTime = TimeSpan.Zero;
                    activity.DataModel.AdvancedAccess = save;
                }
            }
        }
    }
}
