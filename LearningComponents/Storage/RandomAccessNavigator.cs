/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.DataModel;

namespace Microsoft.LearningComponents.Storage
{
    internal class RandomAccessNavigator : DatabaseNavigator
    {
        /// <summary>
        /// Initializes a new <Typ>RandomAccessNavigator</Typ> object.
        /// </summary>
        /// <param name="store">A <Typ>LearningStore</Typ> object that references the database to use.</param>
        /// <param name="attemptId">The unique identifier of the attempt item in the database.</param>
        public RandomAccessNavigator(LearningStore store, long attemptId)
        {
            m_store = store;
            m_attemptId = attemptId;
        }

        /// <summary>
        /// Loads the table of contents into memory and verifies which elements are valid to navigate to.
        /// </summary>
        /// <param name="evaluateSequencingRules">This parameter has no affect on this type of navigator,
        /// because all activities are always valid to navigate to.</param>
        /// <returns>The root table of contents element.</returns>
        public override TableOfContentsElement LoadTableOfContents(bool evaluateSequencingRules)
        {
            LoadActivityTree();
            foreach(Activity a in Traverse)
            {
                a.SetValidToNavigateTo(true);
            }
            return RootActivity;
        }

        /// <summary>
        /// Determines if the passed navigation command is valid to execute or not.
        /// </summary>
        /// <param name="command">The navigation command to test.</param>
        /// <returns>True if the navigation would succeed, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is not equal to NavigationCommand.Continue or NavigationCommand.Previous.</exception>
        public override bool IsNavigationValid(NavigationCommand command)
        {
            LoadActivityTree();
            if(m_currentActivity == null)
            {
                return false;
            }
            
            Activity a;
            if(command == NavigationCommand.Continue)
            {
                a = m_currentActivity.NextInPreorderTraversal;
            }
            else if(command == NavigationCommand.Previous)
            {
                a = m_currentActivity.PreviousInPreorderTraversal;
            }
            else
            {
                // we only support Continue/Previous
                throw new LearningComponentsInternalException("RAN1000");
            }
            return (a != null);
        }

        /// <summary>
        /// Determines if a choice navigation to the activity identified by the string identifier passed will succeed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The destination passed is not a valid activity in this activity tree.</exception>
        public override bool IsNavigationToValid(string destination)
        {
            LoadActivityTree();
            Activity act = ActivityKeyToActivity(destination);
            if(act == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a choice navigation to the activity identified by the unique identifier passed will succeed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The destination passed is not a valid activity in this activity tree.</exception>
        public override bool IsNavigationToValid(long id)
        {
            LoadActivityTree();
            Activity act;
            if(!m_activities.TryGetValue(id, out act))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Loads data for the current activity and its data model into memory.  If this data
        /// is already in memory, this call does nothing.
        /// </summary>
        /// <remarks>
        /// This method and <Mth>LoadActivityTree</Mth> (and <Mth>CreateExecuteNavigator</Mth>, which is 
        /// equivalent) are the only two methods that load Navigator data from the database.  This method
        /// specifically does not load the entire activity tree, and a subsequent call to LoadActivityTree
        /// will still result in a database access.  The reverse is not true - if LoadActivityTree is called
        /// first, this method will do nothing.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid.</exception>
        protected override void LoadLearningDataModel()
        {
            if(m_attemptItemInformationIsValid)
            {
                return;
            }
            // we can't load anything about the "current" activity, since that's not a concept the Random Access
            // mode has, so just load everything.
            LoadActivityTree();
        }

        /// <summary>
        /// Loads the entire activity tree from the database, if it is not already in memory.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid.</exception>
        public override void LoadActivityTree()
        {
            if(RootActivity != null)
            {
                return;
            }
            base.LoadActivityTree();
            m_currentActivity = RootActivity;
            ReadGlobalObjectivesAsNecessary();
        }

        /// <summary>
        /// Reads global objective information when the activity is to be "delivered".
        /// </summary>
        private void ReadGlobalObjectivesAsNecessary()
        {
            if(m_packageFormat != PackageFormat.V1p3)
            {
                // global objectives don't exist prior to SCORM 2004
                return;
            }
            if(!m_currentActivity.Sequencing.Tracked)
            {
                // don't read global objectives if the activity isn't tracked
                return;
            }
            foreach(Objective obj in m_currentActivity.DataModel.Objectives)
            {
                bool satisfied = false;
                float measure = (float)0;

                if(ReadGlobalObjectiveSatisfiedStatus(obj.GlobalObjectiveReadSatisfiedStatus, ref satisfied))
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
                }
                if(ReadGlobalObjectiveNormalizedMeasure(obj.GlobalObjectiveReadNormalizedMeasure, ref measure))
                {
                    // REQ_72.3.3.2
                    obj.Score.Scaled = measure;
                }
            }
        }

        /// <summary>
        /// Performs a Continue or Previous navigation command.  Any other command will throw.
        /// </summary>
        /// <param name="command">Navigation command to execute.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is not equal to NavigationCommand.Continue or NavigationCommand.Previous.</exception>
        public override void Navigate(NavigationCommand command)
        {
            LoadActivityTree();
            if(m_currentActivity == null)
            {
                 throw new SequencingException(SequencingExceptionCode.NB_2_1__2);
            }
            Activity a;
            if(command == NavigationCommand.Continue)
            {
                a = m_currentActivity.NextInPreorderTraversal;
                if(a == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_1__1);
                }
            }
            else if(command == NavigationCommand.Previous)
            {
                a = m_currentActivity.PreviousInPreorderTraversal;
                if(a == null)
                {
                    throw new SequencingException(SequencingExceptionCode.SB_2_1__3);
                }
            }
            else
            {
                // we only support Continue/Previous
                throw new LearningComponentsInternalException("RAN1010");
            }
            m_currentActivity = a;
            UpdateCurrentActivityData();
            ReadGlobalObjectivesAsNecessary();
        }

        /// <summary>
        /// Performs a choice navigation to the activity identified by the string identifier passed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="InvalidOperationException">The destination passed is not a valid activity in this activity tree.</exception>
        public override void NavigateTo(string destination)
        {
            LoadActivityTree();
            Activity act = ActivityKeyToActivity(destination);
            if(act == null)
            {
                throw new InvalidOperationException(Resources.InvalidActivity);
            }
            m_currentActivity = act;
            UpdateCurrentActivityData();
            ReadGlobalObjectivesAsNecessary();
        }

        /// <summary>
        /// Performs a choice navigation to the activity identified by the unique identifier passed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="InvalidOperationException">The destination passed is not a valid activity in this activity tree.</exception>
        public override void NavigateTo(long id)
        {
            LoadActivityTree();
            Activity act;
            if(!m_activities.TryGetValue(id, out act))
            {
                throw new InvalidOperationException(Resources.InvalidActivity);
            }
            m_currentActivity = act;
            UpdateCurrentActivityData();
            ReadGlobalObjectivesAsNecessary();
        }

        /// <summary>
        /// Saves any data that has been changed to the database, if necessary.  Specifically excludes saving
        /// data to the AttemptItem.
        /// </summary>
        public override void Save()
        {
            m_changed = false;
            base.Save();
        }


        /// <summary>
        /// Does nothing.
        /// </summary>
        public override bool ProcessDataModelNavigation()
        {
            return false;
        }
    }
}
