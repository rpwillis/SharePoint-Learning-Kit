/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Collections.ObjectModel;
using System.Xml;
using Microsoft.LearningComponents.DataModel;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.LearningComponents
{
    internal enum SequencingExceptionCode
    {
        NB_2_1__1,
        NB_2_1__2,
        NB_2_1__3,
        NB_2_1__4,
        NB_2_1__5,
        NB_2_1__6,
        NB_2_1__7,
        NB_2_1__8,
        NB_2_1__9,
        NB_2_1__10,
        NB_2_1__11,
        NB_2_1__12,
        NB_2_1__13,
        TB_2_3__1,
        TB_2_3__2,
        TB_2_3__3,
        TB_2_3__4,
        TB_2_3__5,
        TB_2_3__6,
        TB_2_3__7,
        SB_2_1__1,
        SB_2_1__2,
        SB_2_1__3,
        SB_2_1__4,
        SB_2_2__1,
        SB_2_2__2,
        SB_2_4__1,
        SB_2_4__2,
        SB_2_4__3,
        SB_2_5__1,
        SB_2_6__1,
        SB_2_6__2,
        SB_2_7__1,
        SB_2_7__2,
        SB_2_8__1,
        SB_2_8__2,
        SB_2_9__1,
        SB_2_9__2,
        SB_2_9__3,
        SB_2_9__4,
        SB_2_9__5,
        SB_2_9__6,
        SB_2_9__7,
        SB_2_9__8,
        SB_2_9__9,
        SB_2_10__1,
        SB_2_10__2,
        SB_2_10__3,
        SB_2_11__1,
        SB_2_11__2,
        SB_2_12__1,
        DB_1_1__1,
        DB_1_1__2,
        DB_1_1__3,
        DB_2__1
    }

    /// <summary>
    /// Exception thrown when an invalid navigation request is received.
    /// </summary>
    [Serializable]
    public sealed class SequencingException : Exception, ISerializable
    {
        /// <summary>
        /// Map from error code enumeration to the appropriate string.
        /// </summary>
        static private string[] s_exceptionStrings = 
            {
                Resources.NB_2_1__1,
                Resources.NB_2_1__2,
                Resources.NB_2_1__3,
                Resources.NB_2_1__4,
                Resources.NB_2_1__5,
                Resources.NB_2_1__6,
                Resources.NB_2_1__7,
                Resources.NB_2_1__8,
                Resources.NB_2_1__9,
                Resources.NB_2_1__10,
                Resources.NB_2_1__11,
                Resources.NB_2_1__12,
                Resources.NB_2_1__13,
                Resources.TB_2_3__1,
                Resources.TB_2_3__2,
                Resources.TB_2_3__3,
                Resources.TB_2_3__4,
                Resources.TB_2_3__5,
                Resources.TB_2_3__6,
                Resources.TB_2_3__7,
                Resources.SB_2_1__1,
                Resources.SB_2_1__2,
                Resources.SB_2_1__3,
                Resources.SB_2_1__4,
                Resources.SB_2_2__1,
                Resources.SB_2_2__2,
                Resources.SB_2_4__1,
                Resources.SB_2_4__2,
                Resources.SB_2_4__3,
                Resources.SB_2_5__1,
                Resources.SB_2_6__1,
                Resources.SB_2_6__2,
                Resources.SB_2_7__1,
                Resources.SB_2_7__2,
                Resources.SB_2_8__1,
                Resources.SB_2_8__2,
                Resources.SB_2_9__1,
                Resources.SB_2_9__2,
                Resources.SB_2_9__3,
                Resources.SB_2_9__4,
                Resources.SB_2_9__5,
                Resources.SB_2_9__6,
                Resources.SB_2_9__7,
                Resources.SB_2_9__8,
                Resources.SB_2_9__9,
                Resources.SB_2_10__1,
                Resources.SB_2_10__2,
                Resources.SB_2_10__3,
                Resources.SB_2_11__1,
                Resources.SB_2_11__2,
                Resources.SB_2_12__1,
                Resources.DB_1_1__1,
                Resources.DB_1_1__2,
                Resources.DB_1_1__3,
                Resources.DB_2__1
            };

        /// <summary>
        /// A string representation of the sequencing exception code.
        /// </summary>
        private string m_code;

        /// <summary>
        /// Gets a string representation of the sequencing exception code.
        /// </summary>
        public string Code
        {
            get
            {
                return m_code;
            }
        }


        /// <summary>
        /// Initializes a SequencingException
        /// </summary>
        /// <param name="code">The sequencing exception code, from 
        /// SCORM 2004 Sequencing and Navigation Appendix D.</param>
        internal SequencingException(SequencingExceptionCode code) : base(s_exceptionStrings[(int)code])
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            m_code = code.ToString().Replace("__", "-").Replace('_', '.');
        }

        /// <summary>
        /// This constructor is provided for compatibility purposes only.  Use of this 
        /// constructor will not provide information to make this a meaningful SequencingException, 
        /// although it will remain a meaningful Exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        private SequencingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            m_code = info.GetString("SequencingExceptionCode");
        }

        /// <summary>
        /// This constructor is provided for compatibility purposes only.  Use of this 
        /// constructor will not provide information to make this a meaningful SequencingException, 
        /// although it will remain a meaningful Exception.
        /// </summary>
        public SequencingException()
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// This constructor is provided for compatibility purposes only.  Use of this 
        /// constructor will not provide information to make this a meaningful SequencingException, 
        /// although it will remain a meaningful Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SequencingException(string message)
            : base(message)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// This constructor is provided for compatibility purposes only.  Use of this 
        /// constructor will not provide information to make this a meaningful SequencingException, 
        /// although it will remain a meaningful Exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="ex">The exception that is the cause of the current exception.</param>
        public SequencingException(string message, Exception ex)
            : base(message, ex)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// GetObjectData performs a custom serialization.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags= SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("SequencingExceptionCode", m_code);
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Represents a tree of activities, with a single root and 0 or more children per node.
    /// </summary>
    internal class ActivityTree
    {
        /// <summary>
        /// The root of the tree or subtree.
        /// </summary>
        private Activity m_root;

        /// <summary>
        /// The root of the tree or subtree.
        /// </summary>
        internal Activity Root
        {
            get
            {
                return m_root;
            }
        }

        /// <summary>
        /// Initializes an activity tree object.
        /// </summary>
        /// <param name="root">The root of the tree or subtree.</param>
        public ActivityTree(Activity root)
        {
            m_root = root;
        }

        /// <summary>
        /// Enumerates through a tree in pre-order traversal order.
        /// </summary>
        public IEnumerable<Activity> PreOrder
        {
            get
            {
                return ScanPreOrder(m_root);
            }
        }

        /// <summary>
        /// Enumerates through a tree in pre-order traversal order.
        /// </summary>
        /// <param name="root">The root of the tree or subtree being traversed.</param>
        /// <returns>An enumerable.</returns>
        private IEnumerable<Activity> ScanPreOrder(Activity root)
        {
            yield return root;

            foreach(Activity a in root.Children)
            {
                foreach(Activity aa in ScanPreOrder(a))
                {
                    yield return aa;
                }
            }
        }
    }

    /// <summary>
    /// The basic data required for navigation, basically just the activity tree.
    /// </summary>
    internal class NavigatorData
    {
#if DEBUG
        private static Random s_random;

        /// <summary>
        /// Used for debugging purposes only, allows unit tests to set a seed 
        /// so that consistent results will be used for randomization and selection processing.
        /// </summary>
        public static Random Random
        {
            get
            {
                return NavigatorData.s_random;
            }
            set
            {
                NavigatorData.s_random = value;
            }
        }
#endif

        /// <summary>
        /// The entire activity tree belonging to this navigator.
        /// </summary>
	    protected ActivityTree m_activityTree;

        /// <summary>
        /// The current activity of the navigator.
        /// </summary>
        protected Activity m_currentActivity;

        /// <summary>
        /// The suspended activity of the navigator.
        /// </summary>
        protected Activity m_suspendedActivity;

        /// <summary>
        /// Total points for the entire activity tree.
        /// </summary>
        protected float? m_totalPoints;

        /// <summary>
        /// Gets or sets total points for the entire activity tree.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is designed to be set automatically by the data model when
        /// appropriate.  Setting it in other conditions will result in unknown results.
        /// </para><para>
        /// This value is only null if it has never been set.  Once any data model updates this
        /// value, it will never return to null value.
        /// </para>
        /// </remarks>
        public virtual float? TotalPoints
        {
            get
            {
                return m_totalPoints;
            }
            set
            {
                m_totalPoints = value;
            }
        }

        /// <summary>
        /// Completion status for the entire activity tree.
        /// </summary>
        protected CompletionStatus m_completionStatus = CompletionStatus.Unknown;

        /// <summary>
        /// Gets or sets completion status for the entire activity tree.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is designed to be set automatically by the data model when
        /// appropriate.  Setting it in other conditions will result in unknown results.
        /// </para><para>
        /// This value is only valid in SCORM 2004 packages.  Before being set or in other packages,
        /// this value will be CompletionStatus.Unknown.
        /// </para>
        /// </remarks>
        public virtual CompletionStatus CompletionStatus
        {
            get
            {
                return m_completionStatus;
            }
            set
            {
                m_completionStatus = value;
            }
        }

        /// <summary>
        /// Success status for the entire activity tree.
        /// </summary>
        protected SuccessStatus m_successStatus = SuccessStatus.Unknown;

        /// <summary>
        /// Gets or sets success status for the entire activity tree.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is designed to be set automatically by the data model when
        /// appropriate.  Setting it in other conditions will result in unknown results.
        /// </para><para>
        /// This value is only valid in SCORM 2004 packages.  Before being set or in other packages,
        /// this value will be SuccessStatus.Unknown.
        /// </para>
        /// </remarks>
        public virtual SuccessStatus SuccessStatus
        {
            get
            {
                return m_successStatus;
            }
            set
            {
                m_successStatus = value;
            }
        }

        /// <summary>
        /// The dictionary containing all activities that belong to this navigator.
        /// </summary>
        protected Dictionary<long, Activity> m_activities = new Dictionary<long, Activity>();

        /// <summary>
        /// Gets the dictionary containing all activities that belong to this navigator.
        /// </summary>
        public Dictionary<long, Activity> Activities
        {
            get
            {
                return m_activities;
            }
        }

        /// <summary>
        /// Gets or sets the current activity of the navigator.
        /// </summary>
        public virtual Activity CurrentActivity
        {
            get
            {
                return m_currentActivity;
            }
            internal set
            {
                m_currentActivity = value;
            }
        }

        /// <summary>
        /// Gets or sets the suspended activity of the navigator.
        /// </summary>
        public Activity SuspendedActivity
        {
            get
            {
                return m_suspendedActivity;
            }
            internal set
            {
                m_suspendedActivity = value;
            }
        }

        /// <summary>
        /// Gets or sets the root activity of the navigator.
        /// </summary>
        /// <remarks>Once this value has been set, it cannot be changed.</remarks>
        public Activity RootActivity
        {
            get
            {
                if(m_activityTree == null)
                {
                    return null;
                }
                return m_activityTree.Root;
            }
            internal set
            {
                Utilities.Assert(m_activityTree == null, "NAV0010");
                m_activityTree = new ActivityTree(value);
            }
        }

        /// <summary>
        /// Enumerates through the activity tree in pre-order traversal order.
        /// </summary>
        public IEnumerable<Activity> Traverse
        {
            get
            {
                return m_activityTree.PreOrder;
            }
        }

        /// <summary>
        /// Recursively clones children of the specified activity and inserts them into the clone tree.
        /// </summary>
        /// <param name="navigator">The cloned navigator data object.</param>
        /// <param name="cloneParent">The cloned parent to add the children to.</param>
        /// <param name="originalParent">The original parent to clone the children from.</param>
        protected void CloneChildren(NavigatorData navigator, Activity cloneParent, Activity originalParent)
        {
            foreach(Activity child in originalParent.Children)
            {
                Activity clone = child.CloneForNavigationTest();
                cloneParent.AddChild(clone);
                clone.Parent = cloneParent;
                if(CurrentActivity == child)
                {
                    navigator.CurrentActivity = clone;
                }
                if(SuspendedActivity == child)
                {
                    navigator.SuspendedActivity = clone;
                }
                navigator.Activities.Add(clone.ActivityId, clone);
                CloneChildren(navigator, clone, child);
            }
        }

        /// <summary>
        /// Produces an incomplete clone sufficient for testing sequencing and navigation.
        /// </summary>
        /// <returns>A new object with enough data cloned to be viable for sequencing and navigation.</returns>
        public virtual NavigatorData CloneForNavigationTest()
        {
            NavigatorData clone = new NavigatorData();

            clone.RootActivity = m_activityTree.Root.CloneForNavigationTest();
            clone.Activities.Add(clone.RootActivity.ActivityId, clone.RootActivity);
            if(CurrentActivity == RootActivity)
            {
                clone.CurrentActivity = clone.RootActivity;
            }
            if(SuspendedActivity == RootActivity)
            {
                clone.SuspendedActivity = clone.RootActivity;
            }
            CloneChildren(clone, clone.RootActivity, m_activityTree.Root);
            clone.SortActivityTree();
            return clone;
        }

        /// <summary>
        /// Makes sure the activity tree is sorted properly
        /// </summary>
        internal void SortActivityTree()
        {
            foreach(Activity act in Traverse)
            {
                act.SortChildren();
            }
        }

        /// <summary>
        /// Provides a hook to log sequencing messages.
        /// </summary>
        /// <param name="eventType">The type of sequencing event.</param>
        /// <param name="command">The actual navigation command that initiated this event.</param>
        /// <param name="message">A string to be logged.</param>
        /// <param name="args">Arguments that may be supplied to the string to be logged.</param>
        public virtual void LogSequencing(SequencingEventType eventType, NavigationCommand command, string message, params object[] args)
        {
        }

        /// <summary>
        /// Reads the Satisfied Status of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="satisfied">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal virtual bool ReadGlobalObjectiveSatisfiedStatus(string globalObjective, ref bool satisfied)
        {
            return false;
        }

        /// <summary>
        /// Reads the Normalized Measure of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="measure">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal virtual bool ReadGlobalObjectiveNormalizedMeasure(string globalObjective, ref float measure)
        {
            return false;
        }

        /// <summary>
        /// Writes the global objective information for all objectives associated with this activity, if any.
        /// </summary>
        /// <param name="activity">The activity whose objectives to save.</param>
        internal virtual void WriteGlobalObjectives(Activity activity)
        {
        }

        /// <summary>
        /// Loads information about the specified activity that is not vital to sequencing
        /// but it necessary for an activity to be delivered.
        /// </summary>
        /// <param name="activity">Activity to update.</param>
        public virtual void UpdateActivityData(Activity activity)
        {
        }

        /// <summary>
        /// Loads the entire activity tree from an external source, if it is not already in memory.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid.</exception>
        public virtual void LoadActivityTree()
        {
        }

        private Dictionary<string, Activity> m_activityTable;

        /// <summary>
        /// Finds an activity within the activity tree based on the string identifier.
        /// </summary>
        /// <param name="key">The string identifier to search for.</param>
        /// <returns>An activity object, or null if the identifier was not found.</returns>
        internal Activity ActivityKeyToActivity(string key)
        {
            if(m_activityTable == null)
            {
                m_activityTable = new Dictionary<string, Activity>();
                foreach(Activity act in Traverse)
                {
                    m_activityTable.Add(act.Key, act);
                }
            }
            Activity ret;
            m_activityTable.TryGetValue(key, out ret);
            return ret;
        }
    }

    /// <summary>
    /// Base class that exposes SCORM sequencing and navigation
    /// </summary>
    internal abstract partial class Navigator : NavigatorData
    {
        /// <summary>
        /// Gets the SCORM version of the schema associated with this attempt.
        /// </summary>
        protected PackageFormat m_packageFormat;

        /// <summary>
        /// The collection of dirty activity (those in which data has been changed since
        /// they were loaded into memory).
        /// </summary>
        protected Dictionary<long, Activity> m_dirtyActivities = new Dictionary<long,Activity>();

        /// <summary>
        /// Gets the collection of dirty activity (those in which data has been changed since
        /// they were loaded into memory).
        /// </summary>
        internal Dictionary<long, Activity> DirtyActivities
        {
            get
            {
                return m_dirtyActivities;
            }
        }

        /// <summary>
        /// Gets the package format of the schema associated with this attempt.
        /// </summary>
        public virtual PackageFormat PackageFormat
        {
            get
            {
                return m_packageFormat;
            }
        }

        /// <summary>
        /// Sequencing/navigation class for this object, created as needed.
        /// </summary>
        private SeqNav m_seqNav;

        // these methods perform database operations (for those subclasses
        // that use the LearningStore), or otherwise may parse xml or do lengthy
        // operations

        /// <summary>
        /// Loads the table of contents into memory and verifies which elements are valid to navigate to.
        /// </summary>
        /// <param name="evaluateSequencingRules"><c>True</c> to evaluate sequencing rules when determining
        /// whih elements are valid to navigate to.  <c>False</c> to determine which elements are valid
        /// to navigate to using only the static data in the manifest.</param>
        /// <returns>The root table of contents element.</returns>
	    public abstract TableOfContentsElement LoadTableOfContents(bool evaluateSequencingRules);

        /// <summary>
        /// Determines if the passed navigation command is valid to execute or not.
        /// </summary>
        /// <param name="command">The navigation command to test.</param>
        /// <returns>True if the navigation would succeed, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is equal to NavigationCommand.Choose.</exception>
	    public abstract bool IsNavigationValid(NavigationCommand command);
	    
        /// <summary>
        /// Determines if a choice navigation to the activity identified by the string identifier passed will succeed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The destination passed is not a valid activity in this activity tree.</exception>
        public abstract bool IsNavigationToValid(string destination);
	    
        /// <summary>
        /// Performs any navigation request, except for Choice navigation.
        /// </summary>
        /// <param name="command">Navigation command to execute.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The command passed is not a valid enumeration value, or is equal to NavigationCommand.Choose.</exception>
        public abstract void Navigate(NavigationCommand command);

        /// <summary>
        /// Performs a choice navigation to the activity identified by the string identifier passed.
        /// </summary>
        /// <param name="destination">String identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
	    public abstract void NavigateTo(string destination);

        /// <summary>
        /// Performs a choice navigation to the activity identified by the unique identifier passed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <exception cref="SequencingException">Thrown if the navigation fails because a sequencing rule failed.</exception>
	    public abstract void NavigateTo(long id);

        /// <summary>
        /// Determines if a choice navigation to the activity identified by the unique identifier passed will succeed.
        /// </summary>
        /// <param name="id">Unique identifier of the activity to navigate to.</param>
        /// <returns>True if a choice navigation will succeed.</returns>
	    public abstract bool IsNavigationToValid(long id);

        /// <summary>
        /// Saves any changed data to storage.
        /// </summary>
	    public abstract void Save();

        /// <summary>
        /// Performs a navigation operation based on the data model of the current activity, if one is specified.
        /// </summary>
        /// <returns>True if a navigation request was executed.</returns>
        public abstract bool ProcessDataModelNavigation();

        /// <summary>
        /// Indicated whether a choice navigation to the indicated activity would succeed.
        /// </summary>
        /// <param name="activity">The destination activity.</param>
        /// <returns>True if the navigation would be valid, false otherwise.</returns>
        internal bool IsNavigationToValid(Activity activity)
        {
            Utilities.Assert(m_activities.ContainsValue(activity), "NAV0020");
            return IsNavigationValid(NavigationCommand.Choose, activity);
        }

        protected void Rollup(Activity activity)
        {
            if(m_packageFormat == PackageFormat.V1p3)
            {
                if(m_seqNav == null)
                {
                    m_seqNav = new Scorm2004SeqNav(this);
                }
                ((Scorm2004SeqNav)m_seqNav).Rollup(activity);
            }
        }

        /// <summary>
        /// Indicates whether a navigation would be valid if it were to be performed.
        /// </summary>
        /// <param name="command">The navigation command to test.</param>
        /// <param name="activity">The destination activity, if this is a choice navigation.</param>
        /// <returns>True if the navigation would be valid, false otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This makes a partial copy of the activity tree for processing and attempts to
        /// perform a navigation on that copy.  This may result in poor performance if this 
        /// is called many times, but the alternative is to duplicate most of the sequencing
        /// code with lots of special cases for changes of state that should have happened but 
        /// could not because of the temporary nature of this request.  This makes it a 
        /// maintenance nightmare, for something that should only rarely be called anyway 
        /// except in the case of <Mth>LoadTableOfContents</Mth>.
        /// </para>
        /// <para>
        /// The case of <Mth>LoadTableOfContents</Mth> is tough, but steps are taken within
        /// that method to reduce the number of calls to this method to a bare minimum.
        /// </para>
        /// </remarks>
        protected bool IsNavigationValid(NavigationCommand command, Activity activity)
        {
            SeqNav seqNav;

            try
            {
                NavigatorData clone = CloneForNavigationTest();
                Activity destination;

                if(activity != null)
                {
                    destination = clone.Activities[activity.ActivityId];
                }
                else
                {
                    destination = null;
                }
                if(m_packageFormat == PackageFormat.V1p3)
                {
                    seqNav = new Scorm2004SeqNav(clone);
                }
                else
                {
                    seqNav = new Scorm12SeqNav(clone);
                }
                seqNav.OverallSequencingProcess(command, destination);
            }
            catch(SequencingException)
            {
                // if there was a sequencing exception,
                // this is a failure condition.
                return false;
            }
            return true;
        }

        /// <summary>
        /// Performs a full SCORM 2004 or SCORM 1.2 navigation on the current activity tree.
        /// </summary>
        /// <param name="command">The navigation command to perform.</param>
        /// <param name="destination">The destination activity for choice navigation.</param>
        /// <returns>True if the attempt is completed and should not be returned to.</returns>
        protected bool Navigate(NavigationCommand command, Activity destination)
        {
            if(m_seqNav == null)
            {
                if(m_packageFormat == PackageFormat.V1p2 || m_packageFormat == PackageFormat.Lrm)
                {
                    m_seqNav = new Scorm12SeqNav(this);
                }
                else
                {
                    m_seqNav = new Scorm2004SeqNav(this);
                }
            }
            return m_seqNav.OverallSequencingProcess(command, destination);
        }

        /// <summary>
        /// Finds a common ancestor of two activities within an activity tree.
        /// </summary>
        /// <param name="activity1">An activity in an activity tree.</param>
        /// <param name="activity2">An activity in an activity tree.</param>
        /// <returns>The activity that is the common ancestor.</returns>
        static protected Activity FindCommonAncestor(Activity activity1, Activity activity2)
        {
            if(activity1 == null)
            {
                Utilities.Assert(activity2 != null, "NAV0030");
                return activity2;
            }
            if(activity2 == null)
            {
                Utilities.Assert(activity1 != null, "NAV0040");
                return activity1;
            }
            Dictionary<long, Activity> activities = new Dictionary<long, Activity>();
            for(Activity a = activity1 ; a != null ; a = a.Parent)
            {
                activities.Add(a.ActivityId, a);
            }
            for(Activity a = activity2 ; a != null ; a = a.Parent)
            {
                if(activities.ContainsKey(a.ActivityId))
                {
                    return a;
                }
            }
            Utilities.Assert(false, "NAV0050");  // should always be a common ancestor
            return null;
        }
    }
}
