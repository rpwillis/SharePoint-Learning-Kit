/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.Xml;
using System.Data;
using Microsoft.LearningComponents.DataModel;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.LearningComponents.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Transactions;
using System.Data.SqlTypes;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using System.Threading;

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a global objective in memory, corresponding to anything pointed to by a &lt;mapInfo&gt; tag.
    /// </summary>
    internal class GlobalObjective
    {
        /// <summary>
        /// The unique identifier of the objective.
        /// </summary>
        private string m_objectiveId;

        /// <summary>
        /// Gets the unique identifier of the objective.
        /// </summary>
        public string ObjectiveId
        {
            get
            {
                return m_objectiveId;
            }
        }

        /// <summary>
        /// Whether or not the global objective is satisfied.
        /// </summary>
        private bool? m_satisfiedStatus;

        /// <summary>
        /// Gets or sets whether or not the global objective is satisfied.
        /// </summary>
        public bool? SatisfiedStatus
        {
            get
            {
                return m_satisfiedStatus;
            }
            set
            {
                m_isChanged = true;
                m_satisfiedStatus = value;
            }
        }

        /// <summary>
        /// The normalized measure (between 0.0 and 1.0) of the objective.
        /// </summary>
        private float? m_normalizedMeasure;

        /// <summary>
        /// Gets or sets the normalized measure (between 0.0 and 1.0) of the objective.
        /// </summary>
        public float? NormalizedMeasure
        {
            get
            {
                return m_normalizedMeasure;
            }
            set
            {
                m_isChanged = true;
                m_normalizedMeasure = value;
            }
        }

        /// <summary>
        /// Whether or not this objective has been changed since being read.
        /// </summary>
        private bool m_isChanged;

        /// <summary>
        /// Gets or sets whether or not this objective has been changed since being read.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return m_isChanged;
            }
            set
            {
                m_isChanged = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of a <Typ>GlobalObjective</Typ>.
        /// </summary>
        /// <param name="objectiveId">Unique identifier for the global objective.</param>
        public GlobalObjective(string objectiveId)
        {
            m_objectiveId = objectiveId;
        }

        /// <summary>
        /// Resets the objective (e.g. after a database write) to indicate that no changes have been made. 
        /// </summary>
        public void ClearChanged()
        {
            m_isChanged = false;
        }
    }

    /// <summary>
    /// Wraps a GUID and provides an IAttachment interface to the data associated with that GUID in the database.
    /// </summary>
    internal class AttachmentWrapper : IAttachment
    {
        /// <summary>
        /// GUID of the attachment in the database.
        /// </summary>
        private Guid m_guid;

        /// <summary>
        /// The actual bytes of the attachment data.
        /// </summary>
        private byte[] m_bytes;

        /// <summary>
        /// LearningStore to load the data from.
        /// </summary>
        private LearningStore m_store;

        /// <summary>
        /// Navigator that is being used to access the data
        /// </summary>
        private DatabaseNavigator m_navigator;

        /// <summary>
        /// internal activity id that this attachment belongs to
        /// </summary>
        private long m_internalActivityId;
        
        /// <summary>
        /// Initializes a new attachment with a specific guid.
        /// </summary>
        /// <param name="navigator">Navigator used to access the data</param>
        /// <param name="store">LearningStore where the attachment is stored.</param>
        /// <param name="guid">GUID of the attachment in the database.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        public AttachmentWrapper(DatabaseNavigator navigator, LearningStore store, Guid guid, long internalActivityId)
        {
            m_guid = guid;
            m_store = store;
            m_navigator = navigator;
            m_internalActivityId = internalActivityId;
        }
        
        /// <summary>
        /// Initializes a wrapper for a caller provided attachment.
        /// </summary>
        /// <param name="bytes">Actual bytes of the attachment.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        public AttachmentWrapper(byte[] bytes, long internalActivityId)
        {
            Utilities.Assert(bytes != null);
            m_guid = Guid.NewGuid();
            m_bytes = bytes;
            m_internalActivityId = internalActivityId;
        }

        public long InternalActivityId
        {
            get
            {
                return m_internalActivityId;
            }
        }
        
        #region IAttachment Members

        public byte[] GetBytes()
        {
            if(m_bytes == null)
            {
                LearningStoreJob job = m_store.CreateJob();
                m_navigator.DemandSessionRight(job);
                LearningStoreQuery query = m_store.CreateQuery(Schema.ExtensionDataItem.ItemTypeName);
                query.AddColumn(Schema.ExtensionDataItem.AttachmentValue);
                query.AddCondition(Schema.ExtensionDataItem.AttachmentGuid, LearningStoreConditionOperator.Equal, m_guid);
                job.PerformQuery(query);
                DataTable d = job.Execute<DataTable>();
                m_bytes = (byte[])d.Rows[0][Schema.ExtensionDataItem.AttachmentValue];
            }
            return m_bytes;
        }

        public Guid Guid
        {
            get
            {
                return m_guid;
            }
        }

        #endregion
    }

    internal class DatabaseNavigatorData : NavigatorData
    {
        /// <summary>
        /// LearningStore used to read and write to the database.
        /// </summary>
        private LearningStore m_store;

        public LearningStore Store
        {
            get
            {
                return m_store;
            }
            set
            {
                m_store = value;
            }
        }

        /// <summary>
        /// Unique learner identifier from the database.
        /// </summary>
        private long m_learnerId;

        public long LearnerId
        {
            get
            {
                return m_learnerId;
            }
            set
            {
                m_learnerId = value;
            }
        }

        public delegate void DemandSessionRight(LearningStoreJob job);

        private DemandSessionRight m_demandRight;

        public DemandSessionRight DemandRight
        {
            get
            {
                return m_demandRight;
            }
            set
            {
                m_demandRight = value;
            }
        }

        private Dictionary<string, GlobalObjective> m_globalObjectives = new Dictionary<string,GlobalObjective>();

        internal Dictionary<string, GlobalObjective> GlobalObjectives
        {
            get
            {
                return m_globalObjectives;
            }
            set
            {
                m_globalObjectives = value;
            }
        }

        /// <summary>
        /// Reads the global objective information from LearningStore.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective.  Null is not allowed.</param>
        private void ReadGlobalObjectiveFromLearningStore(string globalObjective)
        {
            Utilities.Assert(!String.IsNullOrEmpty(globalObjective));

            LearningStoreJob job = m_store.CreateJob();
            m_demandRight(job);
            LearningStoreQuery query;
            string statusColumnName;
            string measureColumnName;
            if(RootActivity.ObjectivesGlobalToSystem)
            {
                query = m_store.CreateQuery(Schema.SeqNavLearnerGlobalObjectiveView.ViewName);
                statusColumnName = Schema.SeqNavLearnerGlobalObjectiveView.SuccessStatus;
                query.AddColumn(statusColumnName);
                measureColumnName = Schema.SeqNavLearnerGlobalObjectiveView.ScaledScore;
                query.AddColumn(measureColumnName);
                query.AddCondition(Schema.SeqNavLearnerGlobalObjectiveView.Key, LearningStoreConditionOperator.Equal, globalObjective);
                query.AddCondition(Schema.SeqNavLearnerGlobalObjectiveView.LearnerId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
            }
            else
            {
                query = m_store.CreateQuery(Schema.SeqNavOrganizationGlobalObjectiveView.ViewName);
                statusColumnName = Schema.SeqNavOrganizationGlobalObjectiveView.SuccessStatus;
                query.AddColumn(statusColumnName);
                measureColumnName = Schema.SeqNavOrganizationGlobalObjectiveView.ScaledScore;
                query.AddColumn(measureColumnName);
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.Key, LearningStoreConditionOperator.Equal, globalObjective);
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.LearnerId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.OrganizationId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, RootActivity.ActivityId));
            }
            job.PerformQuery(query);

            DataTable d = job.Execute<DataTable>();
            if(d.Rows.Count > 1)
            {
                // should never happen - throw
                throw new InvalidOperationException(Resources.GlobalObjectiveDatabaseStateNotValid);
            }

            // we always create a record in the dictionary, just so we won't have to go through this again.
            GlobalObjective glob = new GlobalObjective(globalObjective);
            if(d.Rows.Count > 0)
            {
                if(!(d.Rows[0][statusColumnName] is DBNull))
                {
                    switch((SuccessStatus)d.Rows[0][statusColumnName])
                    {
                    case SuccessStatus.Failed:
                        glob.SatisfiedStatus = false;
                        break;
                    case SuccessStatus.Passed:
                        glob.SatisfiedStatus = true;
                        break;
                    default:
                    case SuccessStatus.Unknown:
                        glob.SatisfiedStatus = null;
                        break;
                    }
                }
                if(!(d.Rows[0][measureColumnName] is DBNull))
                {
                    glob.NormalizedMeasure = (float)d.Rows[0][measureColumnName];
                }
            }
            m_globalObjectives.Add(globalObjective, glob);
        }

        /// <summary>
        /// Reads the Satisfied Status of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="satisfied">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal override bool ReadGlobalObjectiveSatisfiedStatus(string globalObjective, ref bool satisfied)
        {
            Utilities.Assert(RootActivity != null);

            if(String.IsNullOrEmpty(globalObjective))
            {
                return false;
            }
            if(!m_globalObjectives.ContainsKey(globalObjective))
            {
                ReadGlobalObjectiveFromLearningStore(globalObjective);  // this always adds to the dictionary
            }
            if(m_globalObjectives[globalObjective].SatisfiedStatus.HasValue)
            {
                satisfied = m_globalObjectives[globalObjective].SatisfiedStatus.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the Normalized Measure of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="measure">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal override bool ReadGlobalObjectiveNormalizedMeasure(string globalObjective, ref float measure)
        {
            Utilities.Assert(RootActivity != null);

            if(String.IsNullOrEmpty(globalObjective))
            {
                return false;
            }
            if(!m_globalObjectives.ContainsKey(globalObjective))
            {
                ReadGlobalObjectiveFromLearningStore(globalObjective);  // this always adds to the dictionary
            }
            if(m_globalObjectives[globalObjective].NormalizedMeasure.HasValue)
            {
                measure = m_globalObjectives[globalObjective].NormalizedMeasure.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes the global objective information for all objectives associated with this activity, if any.
        /// </summary>
        /// <param name="activity">The activity whose objectives to save.</param>
        internal override void WriteGlobalObjectives(Activity activity)
        {
            Utilities.Assert(RootActivity != null);

            foreach(Objective obj in activity.DataModel.Objectives)
            {
                foreach(string objId in obj.GlobalObjectiveWriteSatisfiedStatus)
                {
                    if(!m_globalObjectives.ContainsKey(objId))
                    {
                        ReadGlobalObjectiveFromLearningStore(objId);  // this always adds a dictionary entry
                    }
                    if(obj.ObjectiveProgressStatus)
                    {
                        m_globalObjectives[objId].SatisfiedStatus = obj.ObjectiveSatisfiedStatus;
                    }
                    else
                    {
                        m_globalObjectives[objId].SatisfiedStatus = null;
                    }
                }
                foreach(string objId in obj.GlobalObjectiveWriteNormalizedMeasure)
                {
                    if(!m_globalObjectives.ContainsKey(objId))
                    {
                        ReadGlobalObjectiveFromLearningStore(objId);  // this always adds a dictionary entry
                    }
                    if(obj.ObjectiveMeasureStatus)
                    {
                        m_globalObjectives[objId].NormalizedMeasure = obj.ObjectiveNormalizedMeasure;
                    }
                    else
                    {
                        m_globalObjectives[objId].NormalizedMeasure = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Root class for all Navigators that use the database.
    /// </summary>
    internal abstract class DatabaseNavigator : Navigator
    {
        //protected constructor for setting the culture for the resources
        protected DatabaseNavigator()
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// LearningStore used to read and write to the database.
        /// </summary>
        protected LearningStore m_store;

        /// <summary>
        /// Unique attempt identifier from the database.
        /// </summary>
        protected long m_attemptId;

        /// <summary>
        /// Unique learner identifier from the database.
        /// </summary>
        protected long m_learnerId;

        /// <summary>
        /// Status of this Navigator.
        /// </summary>
        protected AttemptStatus m_status = AttemptStatus.Active;

        /// <summary>
        /// backup of dirty activities, for rollback support
        /// </summary>
        protected Dictionary<long, Activity> m_dirtyActivitiesBackup = new Dictionary<long, Activity>();

        /// <summary>
        /// Boolean that indicates whether anything in the base AttemptItem EXCEPT m_status
        /// has changed and thus should be saved.
        /// </summary>
        protected bool m_changed;

        /// <summary>
        /// Boolean that indicates whether m_status has changed and thus should be saved.
        /// </summary>
        protected bool m_statusChanged;

        /// <summary>
        /// Boolean that indicates whether m_totalPoints has changed and thus should be saved.
        /// </summary>
        protected bool m_totalPointsChanged;

        /// <summary>
        /// Unique identifier of the package associated with this attempt.
        /// </summary>
        protected long m_packageId;

        /// <summary>
        /// Location field from the package in the database.
        /// </summary>
        protected string m_packageLocation;

        /// <summary>
        /// Indicates whether or not the AttemptItem data has been read from the database.
        /// </summary>
        protected bool m_attemptItemInformationIsValid;

        /// <summary>
        /// A list of new attachments that have been added to any data model, so that they may be
        /// saved upon Save().
        /// </summary>
        protected Dictionary<Guid, AttachmentWrapper> m_newAttachments = new Dictionary<Guid, AttachmentWrapper>();

        /// <summary>
        /// Backup of attachment data for rollback purposes
        /// </summary>
        protected Dictionary<Guid, AttachmentWrapper> m_newAttachmentsBackup = new Dictionary<Guid, AttachmentWrapper>();

        /// <summary>
        /// Flags that indicate what portions of the sequencing process to log to the database.
        /// </summary>
        protected LoggingOptions m_loggingFlags = LoggingOptions.None;

        /// <summary>
        /// UTC Time the attempt was started
        /// </summary>
        protected DateTime m_attemptStartTime;

        /// <summary>
        /// True only if the attempt just ended and a Save() hasd not been performed, so that the next time
        /// Save() is called an automatic ExpandDataModelCache() will be performed.
        /// </summary>
        private bool m_expandOnNextSave;

        private Dictionary<string, GlobalObjective> m_globalObjectives = new Dictionary<string, GlobalObjective>();

        /// <summary>
        /// Backup of changed objectives for rollback purposes
        /// </summary>
        private Dictionary<string, GlobalObjective> m_globalObjectivesBackup = new Dictionary<string, GlobalObjective>();

        /// <summary>
        /// Gets the UTC time the attempt was started
        /// </summary>
        public DateTime AttemptStartTime
        {
            get
            {
                LoadLearningDataModel();
                return m_attemptStartTime;
            }
        }

        /// <summary>
        /// UTC Time the attempt ended
        /// </summary>
        protected DateTime m_attemptEndTime = SqlDateTime.MinValue.Value;

        /// <summary>
        /// Gets the UTC time the attempt ended
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt has not yet ended.</exception>
        public DateTime AttemptEndTime
        {
            get
            {
                LoadLearningDataModel();
                if(m_status == AttemptStatus.Active || m_status == AttemptStatus.Suspended)
                {
                    throw new InvalidOperationException(Resources.AttemptHasNotEnded);
                }
                return m_attemptEndTime;
            }
        }

        /// <summary>
        /// Gets or sets the current activity of the navigator.
        /// </summary>
        public override Activity CurrentActivity
        {
            get
            {
                LoadLearningDataModel();
                UpdateCurrentActivityData();
                return m_currentActivity;
            }
            internal set
            {
                Utilities.Assert(m_attemptItemInformationIsValid);
                m_currentActivity = value;
            }
        }

        /// <summary>
        /// Gets the SCORM version of the schema associated with this attempt.
        /// </summary>
        public override PackageFormat PackageFormat
        {
            get
            {
                LoadLearningDataModel();
                return m_packageFormat;
            }
        }

        /// <summary>
        /// Gets or sets flags that indicate what portions of the sequencing process to log to the database.
        /// </summary>
        public LoggingOptions LoggingFlags
        {
            get
            {
                LoadLearningDataModel();
                return m_loggingFlags;
            }
            set
            {
                LoadLearningDataModel();
                if(value != m_loggingFlags)
                {
                    m_loggingFlags = value;
                    m_changed = true;
                }
            }
        }

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
        public override float? TotalPoints
        {
            get
            {
                LoadLearningDataModel();
                return m_totalPoints;
            }
            set
            {
                LoadLearningDataModel();
                if(value != m_totalPoints)
                {
                    m_totalPoints = value;
                    m_totalPointsChanged = true;
                }
            }
        }

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
        public override CompletionStatus CompletionStatus
        {
            get
            {
                LoadLearningDataModel();
                return m_completionStatus;
            }
            set
            {
                LoadLearningDataModel();
                if(value != m_completionStatus)
                {
                    m_completionStatus = value;
                    m_changed = true;
                }
            }
        }

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
        public override SuccessStatus SuccessStatus
        {
            get
            {
                LoadLearningDataModel();
                return m_successStatus;
            }
            set
            {
                LoadLearningDataModel();
                if(value != m_successStatus)
                {
                    m_successStatus = value;
                    m_changed = true;
                }
            }
        }

        /// <summary>
        /// Gets the status of this Navigator.
        /// </summary>
        public AttemptStatus Status
        {
            get
            {
                LoadLearningDataModel();
                return m_status;
            }
            set
            {
                LoadLearningDataModel();
                if(m_status != value)
                {
                    m_status = value;
                    if(m_status == AttemptStatus.Abandoned || m_status == AttemptStatus.Completed)
                    {
                        m_expandOnNextSave = true;
                        m_attemptEndTime = DateTime.UtcNow;
                    }
                    m_statusChanged = true;
                }
            }
        }

        /// <summary>
        /// Reactivates a completed attempt.
        /// </summary>
        virtual public void Reactivate()
        {
            LoadLearningDataModel();

            if(m_status != AttemptStatus.Completed && m_status != AttemptStatus.Abandoned)
            {
                throw new InvalidOperationException(Resources.AttemptHasNotEnded);
            }
            m_status = AttemptStatus.Active;
            m_statusChanged = true;
        }

        /// <summary>
        /// Gets the unique learner identifier associated with this Navigator.
        /// </summary>
        public long LearnerId
        {
            get
            {
                LoadLearningDataModel();
                return m_learnerId;
            }
        }

        /// <summary>
        /// Gets the unique attempt identifier associated with this Navigator.
        /// </summary>
        public long AttemptId
        {
            get
            {
                return m_attemptId;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the package associated with this attempt.
        /// </summary>
        public long PackageId
        {
            get
            {
                LoadLearningDataModel();
                return m_packageId;
            }
        }

        /// <summary>
        /// Gets the location field from the package in the database.
        /// </summary>
        public string PackageLocation
        {
            get
            {
                LoadLearningDataModel();
                return m_packageLocation;
            }
        }

        /// <summary>
        /// Produces an incomplete clone sufficient for testing sequencing and navigation.
        /// </summary>
        /// <returns>A new object with enough data cloned to be viable for sequencing and navigation.</returns>
        public override NavigatorData CloneForNavigationTest()
        {
            DatabaseNavigatorData clone = new DatabaseNavigatorData();

            clone.Store = m_store;
            clone.LearnerId = m_learnerId;
            clone.DemandRight = DemandSessionRight;
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
            foreach(KeyValuePair<string, GlobalObjective> kv in m_globalObjectives)
            {
                clone.GlobalObjectives.Add(kv.Key, kv.Value);
            }
            return clone;
        }

        /// <summary>
        /// Deletes all the records related to this attempt
        /// </summary>
        /// <param name="deleteJob">LearningStoreJob to use for deletions</param>
        private void DeleteOldExpandedDataModel(LearningStoreJob deleteJob)
        {
            // first we need to find out which IDs need to be deleted
            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);

            // it is VERY IMPORTANT that this list has the extension data
            // items before the objective and interaction items.  DO NOT MUCK WITH
            // THE ORDER OF THIS LIST.
            string[] views = {Schema.SeqNavAttemptExtensionDataView.ViewName,
                              Schema.SeqNavAttemptObjectiveExtensionDataView.ViewName,
                              Schema.SeqNavAttemptInteractionExtensionDataView.ViewName,
                              Schema.SeqNavAttemptCommentFromLearnerView.ViewName,
                              Schema.SeqNavAttemptCorrectResponseView.ViewName,
                              Schema.SeqNavAttemptInteractionObjectiveView.ViewName,
                              Schema.SeqNavAttemptObjectiveView.ViewName,
                              Schema.SeqNavAttemptEvaluationCommentLearnerView.ViewName,
                              Schema.SeqNavAttemptRubricView.ViewName,
                              Schema.SeqNavAttemptInteractionView.ViewName};
            string[] ids = {Schema.SeqNavAttemptExtensionDataView.ExtensionDataId,
                            Schema.SeqNavAttemptObjectiveExtensionDataView.ExtensionDataId,
                            Schema.SeqNavAttemptInteractionExtensionDataView.ExtensionDataId,
                            Schema.SeqNavAttemptCommentFromLearnerView.CommentFromLearnerId,
                            Schema.SeqNavAttemptCorrectResponseView.CorrectResponseId,
                            Schema.SeqNavAttemptInteractionObjectiveView.InteractionObjectiveId,
                            Schema.SeqNavAttemptObjectiveView.AttemptObjectiveId,
                            Schema.SeqNavAttemptEvaluationCommentLearnerView.EvaluationCommentId,
                            Schema.SeqNavAttemptRubricView.RubricItemId,
                            Schema.SeqNavAttemptInteractionView.InteractionId};
            string[] attemptIds = {Schema.SeqNavAttemptExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptObjectiveExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptInteractionExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptCommentFromLearnerView.AttemptId,
                                   Schema.SeqNavAttemptCorrectResponseView.AttemptId,
                                   Schema.SeqNavAttemptInteractionObjectiveView.AttemptId,
                                   Schema.SeqNavAttemptObjectiveView.AttemptId,
                                   Schema.SeqNavAttemptEvaluationCommentLearnerView.AttemptId,
                                   Schema.SeqNavAttemptRubricView.AttemptId,
                                   Schema.SeqNavAttemptInteractionView.AttemptId};
            string[] mustBeNull = {Schema.SeqNavAttemptExtensionDataView.AttachmentGuid,
                                   Schema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid,
                                   Schema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid,
                                   null,
                                   null,
                                   null,
                                   null,
                                   null,
                                   null,
                                   null};

            LearningStoreQuery query;
            LearningStoreItemIdentifier attemptId = new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId);

            for(int i = 0 ; i < views.Length ; ++i)
            {
                query = m_store.CreateQuery(views[i]);
                query.AddColumn(ids[i]);
                query.AddCondition(attemptIds[i], LearningStoreConditionOperator.Equal, attemptId);
                if(mustBeNull[i] != null)
                {
                    // this prevents attachments from being deleted.  The only way to delete an attachment
                    // from the database is to delete the attempt data.
                    query.AddCondition(mustBeNull[i], LearningStoreConditionOperator.Equal, null);
                }
                job.PerformQuery(query);
            }
            // make sure the attachments get cleared of extraneous pointers to objectives and interactions 
            // that no longer exist
            query = m_store.CreateQuery(Schema.SeqNavAttemptObjectiveExtensionDataView.ViewName);
            query.AddColumn(Schema.SeqNavAttemptObjectiveExtensionDataView.ExtensionDataId);
            query.AddCondition(Schema.SeqNavAttemptObjectiveExtensionDataView.AttemptId, LearningStoreConditionOperator.Equal, attemptId);
            query.AddCondition(Schema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid, LearningStoreConditionOperator.NotEqual, null);
            job.PerformQuery(query);
            query = m_store.CreateQuery(Schema.SeqNavAttemptInteractionExtensionDataView.ViewName);
            query.AddColumn(Schema.SeqNavAttemptInteractionExtensionDataView.ExtensionDataId);
            query.AddCondition(Schema.SeqNavAttemptInteractionExtensionDataView.AttemptId, LearningStoreConditionOperator.Equal, attemptId);
            query.AddCondition(Schema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid, LearningStoreConditionOperator.NotEqual, null);
            job.PerformQuery(query);

            ReadOnlyCollection<object> c = job.Execute();

            // first clear the interaction and objective IDs from all the attachment data.
            DataTable d = (DataTable)c[views.Length];
            Dictionary<string, object> extItem = new Dictionary<string, object>();
            extItem[Schema.ExtensionDataItem.AttemptObjectiveId] = null;
            extItem[Schema.ExtensionDataItem.InteractionId] = null;
            LearningStoreItemIdentifier lsii;
            foreach(DataRow row in d.Rows)
            {
                lsii = (LearningStoreItemIdentifier)row[Schema.SeqNavAttemptObjectiveExtensionDataView.ExtensionDataId];
                deleteJob.UpdateItem(lsii, extItem);
            }
            d = (DataTable)c[views.Length + 1];
            foreach(DataRow row in d.Rows)
            {
                lsii = (LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionExtensionDataView.ExtensionDataId];
                deleteJob.UpdateItem(lsii, extItem);
            }
            for(int i = 0 ; i < views.Length ; ++i)
            {
                d = (DataTable)c[i];
                foreach(DataRow row in d.Rows)
                {
                    lsii = (LearningStoreItemIdentifier)row[ids[i]];
                    deleteJob.DeleteItem(lsii);
                }
            }
        }
        
        /// <summary>
        /// Queries all information needed to rebuild the xml for the data models for all activities in this attempt.
        /// </summary>
        /// <param name="job">LearningStoreJob to use for queries</param>
        private void QueryExpandedDataModel(LearningStoreJob job)
        {
            // first we need to find out which IDs need to be deleted
            DemandSessionRight(job);
            string[] views = {Schema.SeqNavActivityAttemptView.ViewName,
                              Schema.SeqNavAttemptObjectiveView.ViewName,
                              Schema.SeqNavAttemptCommentFromLearnerView.ViewName,
                              Schema.SeqNavAttemptInteractionView.ViewName,
                              Schema.SeqNavAttemptCorrectResponseView.ViewName,
                              Schema.SeqNavAttemptInteractionObjectiveView.ViewName,
                              Schema.SeqNavAttemptExtensionDataView.ViewName,
                              Schema.SeqNavAttemptObjectiveExtensionDataView.ViewName,
                              Schema.SeqNavAttemptInteractionExtensionDataView.ViewName,
                              Schema.SeqNavAttemptEvaluationCommentLearnerView.ViewName,
                              Schema.SeqNavAttemptRubricView.ViewName};
            string[][] cols = {new string[] {Schema.SeqNavActivityAttemptView.ActivityPackageId,
                                             Schema.SeqNavActivityAttemptView.AttemptCount,
                                             Schema.SeqNavActivityAttemptView.CompletionStatus,
                                             Schema.SeqNavActivityAttemptView.Exit,
                                             Schema.SeqNavActivityAttemptView.LessonStatus,
                                             Schema.SeqNavActivityAttemptView.Location,
                                             Schema.SeqNavActivityAttemptView.MaxScore,
                                             Schema.SeqNavActivityAttemptView.MinScore,
                                             Schema.SeqNavActivityAttemptView.ProgressMeasure,
                                             Schema.SeqNavActivityAttemptView.RandomPlacement,
                                             Schema.SeqNavActivityAttemptView.RawScore,
                                             Schema.SeqNavActivityAttemptView.ScaledScore,
                                             Schema.SeqNavActivityAttemptView.SessionTime,
                                             Schema.SeqNavActivityAttemptView.SuccessStatus,
                                             Schema.SeqNavActivityAttemptView.SuspendData,
                                             Schema.SeqNavActivityAttemptView.TotalTime,
                                             Schema.SeqNavActivityAttemptView.EvaluationPoints},
                               new string[] {Schema.SeqNavAttemptObjectiveView.AttemptObjectiveId,
                                             Schema.SeqNavAttemptObjectiveView.ActivityAttemptId,
                                             Schema.SeqNavAttemptObjectiveView.CompletionStatus,
                                             Schema.SeqNavAttemptObjectiveView.Description,
                                             Schema.SeqNavAttemptObjectiveView.IsPrimaryObjective,
                                             Schema.SeqNavAttemptObjectiveView.Key,
                                             Schema.SeqNavAttemptObjectiveView.LessonStatus,
                                             Schema.SeqNavAttemptObjectiveView.MaxScore,
                                             Schema.SeqNavAttemptObjectiveView.MinScore,
                                             Schema.SeqNavAttemptObjectiveView.ProgressMeasure,
                                             Schema.SeqNavAttemptObjectiveView.RawScore,
                                             Schema.SeqNavAttemptObjectiveView.ScaledScore,
                                             Schema.SeqNavAttemptObjectiveView.SuccessStatus},
                               new string[] {Schema.SeqNavAttemptCommentFromLearnerView.ActivityAttemptId,
                                             Schema.SeqNavAttemptCommentFromLearnerView.Comment,
                                             Schema.SeqNavAttemptCommentFromLearnerView.Location,
                                             Schema.SeqNavAttemptCommentFromLearnerView.Ordinal,
                                             Schema.SeqNavAttemptCommentFromLearnerView.Timestamp},
                               new string[] {Schema.SeqNavAttemptInteractionView.InteractionId,
                                             Schema.SeqNavAttemptInteractionView.ActivityAttemptId,
                                             Schema.SeqNavAttemptInteractionView.Description,
                                             Schema.SeqNavAttemptInteractionView.EvaluationPoints,
                                             Schema.SeqNavAttemptInteractionView.InteractionIdFromCmi,
                                             Schema.SeqNavAttemptInteractionView.InteractionType,
                                             Schema.SeqNavAttemptInteractionView.Latency,
                                             Schema.SeqNavAttemptInteractionView.LearnerResponseBool,
                                             Schema.SeqNavAttemptInteractionView.LearnerResponseNumeric,
                                             Schema.SeqNavAttemptInteractionView.LearnerResponseString,
                                             Schema.SeqNavAttemptInteractionView.MaxScore,
                                             Schema.SeqNavAttemptInteractionView.MinScore,
                                             Schema.SeqNavAttemptInteractionView.RawScore,
                                             Schema.SeqNavAttemptInteractionView.ResultState,
                                             Schema.SeqNavAttemptInteractionView.ResultNumeric,
                                             Schema.SeqNavAttemptInteractionView.ScaledScore,
                                             Schema.SeqNavAttemptInteractionView.Timestamp,
                                             Schema.SeqNavAttemptInteractionView.Weighting},
                               new string[] {Schema.SeqNavAttemptCorrectResponseView.InteractionId,
                                             Schema.SeqNavAttemptCorrectResponseView.ResponsePattern},
                               new string[] {Schema.SeqNavAttemptInteractionObjectiveView.InteractionId,
                                             Schema.SeqNavAttemptInteractionObjectiveView.AttemptObjectiveId},
                               new string[] {Schema.SeqNavAttemptExtensionDataView.ActivityAttemptId,
                                             Schema.SeqNavAttemptExtensionDataView.Name,
                                             Schema.SeqNavAttemptExtensionDataView.StringValue,
                                             Schema.SeqNavAttemptExtensionDataView.IntValue,
                                             Schema.SeqNavAttemptExtensionDataView.BoolValue,
                                             Schema.SeqNavAttemptExtensionDataView.DoubleValue,
                                             Schema.SeqNavAttemptExtensionDataView.DateTimeValue,
                                             Schema.SeqNavAttemptExtensionDataView.AttachmentGuid},
                               new string[] {Schema.SeqNavAttemptObjectiveExtensionDataView.AttemptObjectiveId,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.Name,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.StringValue,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.IntValue,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.BoolValue,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.DoubleValue,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.DateTimeValue,
                                             Schema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid},
                               new string[] {Schema.SeqNavAttemptInteractionExtensionDataView.InteractionId,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.Name,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.StringValue,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.IntValue,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.BoolValue,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.DoubleValue,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.DateTimeValue,
                                             Schema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid},
                               new string[] {Schema.SeqNavAttemptEvaluationCommentLearnerView.Comment,
                                             Schema.SeqNavAttemptEvaluationCommentLearnerView.InteractionId,
                                             Schema.SeqNavAttemptEvaluationCommentLearnerView.Location,
                                             Schema.SeqNavAttemptEvaluationCommentLearnerView.Ordinal,
                                             Schema.SeqNavAttemptEvaluationCommentLearnerView.Timestamp},
                               new string[] {Schema.SeqNavAttemptRubricView.InteractionId,
                                             Schema.SeqNavAttemptRubricView.IsSatisfied,
                                             Schema.SeqNavAttemptRubricView.Ordinal,
                                             Schema.SeqNavAttemptRubricView.Points}};
            string[] attemptIds = {Schema.SeqNavActivityAttemptView.AttemptId,
                                   Schema.SeqNavAttemptObjectiveView.AttemptId,
                                   Schema.SeqNavAttemptCommentFromLearnerView.AttemptId,
                                   Schema.SeqNavAttemptInteractionView.AttemptId,
                                   Schema.SeqNavAttemptCorrectResponseView.AttemptId,
                                   Schema.SeqNavAttemptInteractionObjectiveView.AttemptId,
                                   Schema.SeqNavAttemptExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptObjectiveExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptInteractionExtensionDataView.AttemptId,
                                   Schema.SeqNavAttemptEvaluationCommentLearnerView.AttemptId,
                                   Schema.SeqNavAttemptRubricView.AttemptId};

            LearningStoreQuery query;

            for(int i = 0 ; i < views.Length ; ++i)
            {
                query = m_store.CreateQuery(views[i]);
                foreach(string col in cols[i])
                {
                    query.AddColumn(col);
                }
                query.AddCondition(attemptIds[i], LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
                job.PerformQuery(query);
            }
        }

        /// <summary>
        /// Sorts the rows of a datatable by a number intended as an ordinal.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="columnName">The name of the column representing the ordinal value</param>
        /// <returns>an array of the sorted DataRows</returns>
        private static DataRow[] SortByOrdinal(DataTable d, string columnName)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            DataRow[] rows = new DataRow[d.Rows.Count];

            foreach(DataRow row in d.Rows)
            {
                rows[(int)row[columnName]] = row;
            }
            return rows;
        }

        /// <summary>
        /// Applies data rows associated with activities.
        /// </summary>
        /// <param name="d">The data table</param>
        private void ApplyActivityData(DataTable d)
        {
            foreach(DataRow row in d.Rows)
            {
                Activity act = m_activities[((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey()];
                LearningDataModel dm = act.DataModel;

                dm.AdvancedAccess = true;
                dm.ActivityAttemptCount = (int)row[Schema.SeqNavActivityAttemptView.AttemptCount];
                dm.CompletionStatus = (CompletionStatus)row[Schema.SeqNavActivityAttemptView.CompletionStatus];
                dm.NavigationRequest.ExitMode = (ExitMode?)(row[Schema.SeqNavActivityAttemptView.Exit] as int?);
                if(row[Schema.SeqNavActivityAttemptView.LessonStatus] is LessonStatus)
                {
                    dm.LessonStatus = (LessonStatus)row[Schema.SeqNavActivityAttemptView.LessonStatus];
                }
                dm.Location = row[Schema.SeqNavActivityAttemptView.Location] as string;
                dm.Score.Maximum = row[Schema.SeqNavActivityAttemptView.MaxScore] as float?;
                dm.Score.Minimum = row[Schema.SeqNavActivityAttemptView.MinScore] as float?;
                dm.ProgressMeasure = row[Schema.SeqNavActivityAttemptView.ProgressMeasure] as float?;
                act.RandomPlacement = (int)row[Schema.SeqNavActivityAttemptView.RandomPlacement];
                dm.Score.Raw = row[Schema.SeqNavActivityAttemptView.RawScore] as float?;
                dm.Score.Scaled = row[Schema.SeqNavActivityAttemptView.ScaledScore] as float?;
                dm.SessionTime = new TimeSpan((long)(double)row[Schema.SeqNavActivityAttemptView.SessionTime]);
                dm.SuccessStatus = (SuccessStatus)row[Schema.SeqNavActivityAttemptView.SuccessStatus];
                dm.SuspendData = row[Schema.SeqNavActivityAttemptView.SuspendData] as string;
                dm.TotalTime = new TimeSpan((long)(double)row[Schema.SeqNavActivityAttemptView.TotalTime]);
                dm.EvaluationPoints = row[Schema.SeqNavActivityAttemptView.EvaluationPoints] as float?;

                // make sure other data is cleared
                dm.CommentsFromLearner.Clear();
                dm.Objectives.Clear();
                dm.Interactions.Clear();
                dm.ExtensionData.Clear();
                dm.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with objectives.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="activitiesByInternalId">dictionary to look up activities by their SQL id</param>
        private static Dictionary<long, Objective> ApplyObjectiveData(DataTable d, Dictionary<long, Activity> activitiesByInternalId)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            Dictionary<long, Objective> objectives = new Dictionary<long, Objective>();

            foreach(DataRow row in d.Rows)
            {
                LearningStoreItemIdentifier id = (LearningStoreItemIdentifier)row[Schema.SeqNavAttemptObjectiveView.AttemptObjectiveId];
                LearningDataModel dm = activitiesByInternalId[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptObjectiveView.ActivityAttemptId]).GetKey()].DataModel;

                dm.AdvancedAccess = true;
                if((bool)row[Schema.SeqNavAttemptObjectiveView.IsPrimaryObjective])
                {
                    // don't actually add it to the list since there is already always a primary objective in the list
                    dm.PrimaryObjective.CompletionStatus = (CompletionStatus)row[Schema.SeqNavAttemptObjectiveView.CompletionStatus];
                    dm.PrimaryObjective.Description = row[Schema.SeqNavAttemptObjectiveView.Description] as string;
                    dm.PrimaryObjective.Id = row[Schema.SeqNavAttemptObjectiveView.Key] as string;
                    dm.PrimaryObjective.Status = row[Schema.SeqNavAttemptObjectiveView.LessonStatus] as LessonStatus?;
                    dm.PrimaryObjective.Score.Maximum = row[Schema.SeqNavAttemptObjectiveView.MaxScore] as float?;
                    dm.PrimaryObjective.Score.Minimum = row[Schema.SeqNavAttemptObjectiveView.MinScore] as float?;
                    dm.PrimaryObjective.ProgressMeasure = row[Schema.SeqNavAttemptObjectiveView.ProgressMeasure] as float?;
                    dm.PrimaryObjective.Score.Raw = row[Schema.SeqNavAttemptObjectiveView.RawScore] as float?;
                    dm.PrimaryObjective.Score.Scaled = row[Schema.SeqNavAttemptObjectiveView.ScaledScore] as float?;
                    dm.PrimaryObjective.SuccessStatus = (SuccessStatus)row[Schema.SeqNavAttemptObjectiveView.SuccessStatus];
                    objectives.Add(id.GetKey(), dm.PrimaryObjective);
                }
                else
                {
                    Objective obj = dm.CreateObjective();
                    obj.CompletionStatus = (CompletionStatus)row[Schema.SeqNavAttemptObjectiveView.CompletionStatus];
                    obj.Description = row[Schema.SeqNavAttemptObjectiveView.Description] as string;
                    obj.Id = row[Schema.SeqNavAttemptObjectiveView.Key] as string;
                    obj.Status = row[Schema.SeqNavAttemptObjectiveView.LessonStatus] as LessonStatus?;
                    obj.Score.Maximum = row[Schema.SeqNavAttemptObjectiveView.MaxScore] as float?;
                    obj.Score.Minimum = row[Schema.SeqNavAttemptObjectiveView.MinScore] as float?;
                    obj.ProgressMeasure = row[Schema.SeqNavAttemptObjectiveView.ProgressMeasure] as float?;
                    obj.Score.Raw = row[Schema.SeqNavAttemptObjectiveView.RawScore] as float?;
                    obj.Score.Scaled = row[Schema.SeqNavAttemptObjectiveView.ScaledScore] as float?;
                    obj.SuccessStatus = (SuccessStatus)row[Schema.SeqNavAttemptObjectiveView.SuccessStatus];
                    dm.Objectives.Add(obj);
                    objectives.Add(id.GetKey(), obj);
                }
                dm.AdvancedAccess = false;
            }
            return objectives;
        }

        /// <summary>
        /// Applies data rows associated with comments.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="activitiesByInternalId">dictionary to look up activities by their SQL id</param>
        private static void ApplyCommentData(DataTable d, Dictionary<long, Activity> activitiesByInternalId)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            DataRow[] sortedrows = SortByOrdinal(d, Schema.SeqNavAttemptCommentFromLearnerView.Ordinal);
            for(int i = 0 ; i < sortedrows.GetLength(0) ; ++i)
            {
                LearningDataModel dm = activitiesByInternalId[((LearningStoreItemIdentifier)sortedrows[i][Schema.SeqNavAttemptCommentFromLearnerView.ActivityAttemptId]).GetKey()].DataModel;

                dm.AdvancedAccess = true;
                Comment comment = dm.CreateComment();
                comment.CommentText = sortedrows[i][Schema.SeqNavAttemptCommentFromLearnerView.Comment] as string;
                comment.Location = sortedrows[i][Schema.SeqNavAttemptCommentFromLearnerView.Location] as string;
                comment.Timestamp = sortedrows[i][Schema.SeqNavAttemptCommentFromLearnerView.Timestamp] as string;
                dm.CommentsFromLearner.Add(comment);
                dm.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with interactions.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="activitiesByInternalId">dictionary to look up activities by their SQL id</param>
        private static Dictionary<long, Interaction> ApplyInteractionData(DataTable d, Dictionary<long, Activity> activitiesByInternalId)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            Dictionary<long, Interaction> interactions = new Dictionary<long, Interaction>();
            foreach(DataRow row in d.Rows)
            {
                LearningStoreItemIdentifier id = (LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionView.InteractionId];
                LearningDataModel dm = activitiesByInternalId[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionView.ActivityAttemptId]).GetKey()].DataModel;

                dm.AdvancedAccess = true;
                Interaction interaction = dm.CreateInteraction();
                interaction.Description = row[Schema.SeqNavAttemptInteractionView.Description] as string;
                interaction.Evaluation.Points = row[Schema.SeqNavAttemptInteractionView.EvaluationPoints] as float?;
                interaction.Id = row[Schema.SeqNavAttemptInteractionView.InteractionIdFromCmi] as string;
                interaction.InteractionType = (InteractionType?)(row[Schema.SeqNavAttemptInteractionView.InteractionType] as int?);
                if(row[Schema.SeqNavAttemptInteractionView.Latency] is double)
                {
                    interaction.Latency = new TimeSpan((long)(double)row[Schema.SeqNavAttemptInteractionView.Latency]);
                }
                switch(interaction.InteractionType)
                {
                case InteractionType.Numeric:
                    interaction.LearnerResponse = row[Schema.SeqNavAttemptInteractionView.LearnerResponseNumeric] as float?;
                    break;
                case InteractionType.TrueFalse:
                    interaction.LearnerResponse = row[Schema.SeqNavAttemptInteractionView.LearnerResponseBool] as bool?;
                    break;
                default:
                    interaction.LearnerResponse = row[Schema.SeqNavAttemptInteractionView.LearnerResponseString] as string;
                    break;
                }
                interaction.Score.Maximum = row[Schema.SeqNavAttemptInteractionView.MaxScore] as float?;
                interaction.Score.Minimum = row[Schema.SeqNavAttemptInteractionView.MinScore] as float?;
                interaction.Score.Raw = row[Schema.SeqNavAttemptInteractionView.RawScore] as float?;
                interaction.Result.State = (InteractionResultState)row[Schema.SeqNavAttemptInteractionView.ResultState];
                interaction.Result.NumericResult = row[Schema.SeqNavAttemptInteractionView.ResultNumeric] as float?;
                interaction.Score.Scaled = row[Schema.SeqNavAttemptInteractionView.ScaledScore] as float?;
                interaction.Timestamp = row[Schema.SeqNavAttemptInteractionView.Timestamp] as string;
                interaction.Weighting = row[Schema.SeqNavAttemptInteractionView.Weighting] as float?;
                dm.Interactions.Add(interaction);
                interactions.Add(id.GetKey(), interaction);
                dm.AdvancedAccess = false;
            }
            return interactions;
        }

        /// <summary>
        /// Applies data rows associated with correct response information.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="interactions">dictionary to look up interactions by their SQL id</param>
        private static void ApplyCorrectResponseData(DataTable d, Dictionary<long, Interaction> interactions)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            foreach(DataRow row in d.Rows)
            {
                Interaction interaction = interactions[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptCorrectResponseView.InteractionId]).GetKey()];

                interaction.DataModel.AdvancedAccess = true;
                CorrectResponse cr = interaction.DataModel.CreateCorrectResponse();
                cr.Pattern = row[Schema.SeqNavAttemptCorrectResponseView.ResponsePattern] as string;
                interaction.CorrectResponses.Add(cr);
                interaction.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with interaction objectives.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="interactions">dictionary to look up interactions by their SQL id</param>
        /// <param name="objectives">dictionary to look up objectives by their SQL id</param>
        private static void ApplyInteractionObjectiveData(DataTable d, Dictionary<long, Interaction> interactions, Dictionary<long, Objective> objectives)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            foreach(DataRow row in d.Rows)
            {
                Interaction interaction = interactions[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionObjectiveView.InteractionId]).GetKey()];
                Objective obj = objectives[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionObjectiveView.AttemptObjectiveId]).GetKey()];

                interaction.DataModel.AdvancedAccess = true;
                InteractionObjective io = interaction.DataModel.CreateInteractionObjective();
                io.Id = obj.Id;
                interaction.Objectives.Add(io);
                interaction.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with extension data associated with an activity
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="activitiesByInternalId">dictionary to look up activities by their SQL id</param>
        private static void ApplyActivityExtensionData(DataTable d, Dictionary<long, Activity> activitiesByInternalId)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            foreach(DataRow row in d.Rows)
            {
                LearningDataModel dm = activitiesByInternalId[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptExtensionDataView.ActivityAttemptId]).GetKey()].DataModel;

                dm.AdvancedAccess = true;
                string key = row[Schema.SeqNavAttemptExtensionDataView.Name] as string;
                if(row[Schema.SeqNavAttemptExtensionDataView.BoolValue] is bool)
                {
                    dm.ExtensionData[key] = (bool)row[Schema.SeqNavAttemptExtensionDataView.BoolValue];
                }
                else if(row[Schema.SeqNavAttemptExtensionDataView.DateTimeValue] is DateTime)
                {
                    dm.ExtensionData[key] = (DateTime)row[Schema.SeqNavAttemptExtensionDataView.DateTimeValue];
                }
                else if(row[Schema.SeqNavAttemptExtensionDataView.DoubleValue] is double)
                {
                    dm.ExtensionData[key] = (double)row[Schema.SeqNavAttemptExtensionDataView.DoubleValue];
                }
                else if(row[Schema.SeqNavAttemptExtensionDataView.IntValue] is int)
                {
                    dm.ExtensionData[key] = (int)row[Schema.SeqNavAttemptExtensionDataView.IntValue];
                }
                else if(row[Schema.SeqNavAttemptExtensionDataView.StringValue] is string)
                {
                    dm.ExtensionData[key] = (string)row[Schema.SeqNavAttemptExtensionDataView.StringValue];
                }
                else if(row[Schema.SeqNavAttemptExtensionDataView.AttachmentGuid] is Guid)
                {
                    dm.ExtensionData[key] = (Guid)row[Schema.SeqNavAttemptExtensionDataView.AttachmentGuid];
                }
                dm.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with extension data associated with a particular objective.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="objectives">dictionary to look up objectives by their SQL id</param>
        private static void ApplyObjectiveExtensionData(DataTable d, Dictionary<long, Objective> objectives)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            foreach(DataRow row in d.Rows)
            {
                Objective obj = objectives[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptObjectiveExtensionDataView.AttemptObjectiveId]).GetKey()];

                obj.DataModel.AdvancedAccess = true;
                string key = row[Schema.SeqNavAttemptObjectiveExtensionDataView.Name] as string;
                if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.BoolValue] is bool)
                {
                    obj.ExtensionData[key] = (bool)row[Schema.SeqNavAttemptObjectiveExtensionDataView.BoolValue];
                }
                else if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.DateTimeValue] is DateTime)
                {
                    obj.ExtensionData[key] = (DateTime)row[Schema.SeqNavAttemptObjectiveExtensionDataView.DateTimeValue];
                }
                else if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.DoubleValue] is double)
                {
                    obj.ExtensionData[key] = (double)row[Schema.SeqNavAttemptObjectiveExtensionDataView.DoubleValue];
                }
                else if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.IntValue] is int)
                {
                    obj.ExtensionData[key] = (int)row[Schema.SeqNavAttemptObjectiveExtensionDataView.IntValue];
                }
                else if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.StringValue] is string)
                {
                    obj.ExtensionData[key] = (string)row[Schema.SeqNavAttemptObjectiveExtensionDataView.StringValue];
                }
                else if(row[Schema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid] is Guid)
                {
                    obj.ExtensionData[key] = (Guid)row[Schema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid];
                }
                obj.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with extension data associated with a particular interaction.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="interactions">dictionary to look up interactions by their SQL id</param>
        private static void ApplyInteractionExtensionData(DataTable d, Dictionary<long, Interaction> interactions)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            foreach(DataRow row in d.Rows)
            {
                Interaction interaction = interactions[((LearningStoreItemIdentifier)row[Schema.SeqNavAttemptInteractionExtensionDataView.InteractionId]).GetKey()];

                interaction.DataModel.AdvancedAccess = true;
                string key = row[Schema.SeqNavAttemptInteractionExtensionDataView.Name] as string;
                if(row[Schema.SeqNavAttemptInteractionExtensionDataView.BoolValue] is bool)
                {
                    interaction.ExtensionData[key] = (bool)row[Schema.SeqNavAttemptInteractionExtensionDataView.BoolValue];
                }
                else if(row[Schema.SeqNavAttemptInteractionExtensionDataView.DateTimeValue] is DateTime)
                {
                    interaction.ExtensionData[key] = (DateTime)row[Schema.SeqNavAttemptInteractionExtensionDataView.DateTimeValue];
                }
                else if(row[Schema.SeqNavAttemptInteractionExtensionDataView.DoubleValue] is double)
                {
                    interaction.ExtensionData[key] = (double)row[Schema.SeqNavAttemptInteractionExtensionDataView.DoubleValue];
                }
                else if(row[Schema.SeqNavAttemptInteractionExtensionDataView.IntValue] is int)
                {
                    interaction.ExtensionData[key] = (int)row[Schema.SeqNavAttemptInteractionExtensionDataView.IntValue];
                }
                else if(row[Schema.SeqNavAttemptInteractionExtensionDataView.StringValue] is string)
                {
                    interaction.ExtensionData[key] = (string)row[Schema.SeqNavAttemptInteractionExtensionDataView.StringValue];
                }
                else if(row[Schema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid] is Guid)
                {
                    interaction.ExtensionData[key] = (Guid)row[Schema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid];
                }
                interaction.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with evaluation comments
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="interactions">dictionary to look up interactions by their SQL id</param>
        private static void ApplyEvaluationCommentData(DataTable d, Dictionary<long, Interaction> interactions)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            DataRow[] sortedrows = SortByOrdinal(d, Schema.SeqNavAttemptEvaluationCommentLearnerView.Ordinal);
            for(int i = 0 ; i < sortedrows.GetLength(0) ; ++i)
            {
                Interaction interaction = interactions[((LearningStoreItemIdentifier)sortedrows[i][Schema.SeqNavAttemptEvaluationCommentLearnerView.InteractionId]).GetKey()];

                interaction.DataModel.AdvancedAccess = true;
                Comment comment = interaction.DataModel.CreateComment();
                comment.CommentText = sortedrows[i][Schema.SeqNavAttemptEvaluationCommentLearnerView.Comment] as string;
                comment.Location = sortedrows[i][Schema.SeqNavAttemptEvaluationCommentLearnerView.Location] as string;
                comment.Timestamp = sortedrows[i][Schema.SeqNavAttemptEvaluationCommentLearnerView.Timestamp] as string;
                interaction.Evaluation.Comments.Add(comment);
                interaction.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data rows associated with rubrics.
        /// </summary>
        /// <param name="d">The data table</param>
        /// <param name="interactions">dictionary to look up interactions by their SQL id</param>
        private static void ApplyRubricData(DataTable d, Dictionary<long, Interaction> interactions)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            DataRow[] sortedrows = SortByOrdinal(d, Schema.SeqNavAttemptRubricView.Ordinal);
            for(int i = 0 ; i < sortedrows.GetLength(0) ; ++i)
            {
                Interaction interaction = interactions[((LearningStoreItemIdentifier)sortedrows[i][Schema.SeqNavAttemptRubricView.InteractionId]).GetKey()];

                interaction.DataModel.AdvancedAccess = true;
                Rubric rubric = interaction.DataModel.CreateRubric();
                rubric.IsSatisfied = sortedrows[i][Schema.SeqNavAttemptRubricView.IsSatisfied] as bool?;
                rubric.Points = sortedrows[i][Schema.SeqNavAttemptRubricView.Points] as float?;
                interaction.Rubrics.Add(rubric);
                interaction.DataModel.AdvancedAccess = false;
            }
        }

        /// <summary>
        /// Applies data queried by QueryExpandedDataModel to the in memory representation
        /// of the data models of each activity.  This does not save the data back to the database.
        /// </summary>
        /// <param name="c"></param>
        private void ApplyExpandedDataModel(ReadOnlyCollection<object> c)
        {

            // build this dictionary temporarily just to prevent a few extra inner joins when doing the queries
            Dictionary<long, Activity> activitiesByInternalId = new Dictionary<long, Activity>();
            foreach(Activity act in m_activities.Values)
            {
                activitiesByInternalId[act.InternalActivityId] = act;
            }

            ApplyActivityData((DataTable)c[0]);
            Dictionary<long, Objective> objectives = ApplyObjectiveData((DataTable)c[1], activitiesByInternalId);
            ApplyCommentData((DataTable)c[2], activitiesByInternalId);
            Dictionary<long, Interaction> interactions = ApplyInteractionData((DataTable)c[3], activitiesByInternalId);
            ApplyCorrectResponseData((DataTable)c[4], interactions);
            ApplyInteractionObjectiveData((DataTable)c[5], interactions, objectives);
            ApplyActivityExtensionData((DataTable)c[6], activitiesByInternalId);
            ApplyObjectiveExtensionData((DataTable)c[7], objectives);
            ApplyInteractionExtensionData((DataTable)c[8], interactions);
            ApplyEvaluationCommentData((DataTable)c[9], interactions);
            ApplyRubricData((DataTable)c[10], interactions);
        }

        /// <summary>
        /// Recreates the Data Model after being deleted by <Mth>DeleteDataModelCache</Mth>.  This method is not valid
        /// unless the attempt has been abandoned or completed.  It may be called whether or not <Mth>DeleteDataModelCache</Mth>
        /// has been called, and it will always overwrite the xml data model in favor of data stored in the expanded schema.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt has not completed yet.</exception>
        public void ReconstituteDataModelCache()
        {
            LoadActivityTree();
            if(m_status == AttemptStatus.Active ||
                m_status == AttemptStatus.Suspended)
            {
                throw new InvalidOperationException(Resources.AttemptHasNotEnded);
            }
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            using(LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                LearningStoreJob job = m_store.CreateJob();
                DemandSessionRight(job);
                QueryExpandedDataModel(job);
                ReadOnlyCollection<object> c = job.Execute();
                ApplyExpandedDataModel(c);
                this.Save();
                scope.Complete();
            }
        }

        /// <summary>
        /// Expands the data model xml for all activities in this attempt to all the various columns 
        /// and tables in the <Typ>LearningStore</Typ> that are used for reporting.
        /// </summary>
        public void ExpandDataModelCache()
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            using(LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
            {
                LearningStoreJob job = m_store.CreateJob();
                DemandSessionRight(job);
                ExpandDataModelCache(job);
                job.Execute();
                scope.Complete();
            }
        }

        /// <summary>
        /// Expands the data model xml for all activities in this attempt to all the various columns 
        /// and tables in the <Typ>LearningStore</Typ> that are used for reporting.
        /// </summary>
        /// <param name="job">LearningStoreJob to use for the record deletions and writes.</param>
        /// <remarks>This method assumes that the caller is responsible for transaction handling.  A Serializable 
        /// transaction is recommended for callers.</remarks>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        private void ExpandDataModelCache(LearningStoreJob job)
        {
            LoadFullActivityTree();

            DeleteOldExpandedDataModel(job);

            // go through each activity and expand it all.
            foreach(Activity act in Traverse)
            {
                Dictionary<string, object> unique = new Dictionary<string, object>();
                Dictionary<string, object> item = new Dictionary<string, object>();

                item[Schema.ActivityAttemptItem.AttemptCount] = act.DataModel.ActivityAttemptCount;
                item[Schema.ActivityAttemptItem.CompletionStatus] = act.DataModel.CompletionStatus;
                item[Schema.ActivityAttemptItem.Exit] = act.DataModel.NavigationRequest.ExitMode;
                item[Schema.ActivityAttemptItem.LessonStatus] = act.DataModel.LessonStatus;
                item[Schema.ActivityAttemptItem.Location] = act.DataModel.Location;
                item[Schema.ActivityAttemptItem.MaxScore] = act.DataModel.Score.Maximum;
                item[Schema.ActivityAttemptItem.MinScore] = act.DataModel.Score.Minimum;
                item[Schema.ActivityAttemptItem.ProgressMeasure] = act.DataModel.ProgressMeasure;
                item[Schema.ActivityAttemptItem.RandomPlacement] = act.RandomPlacement;
                item[Schema.ActivityAttemptItem.RawScore] = act.DataModel.Score.Raw;
                item[Schema.ActivityAttemptItem.ScaledScore] = act.DataModel.Score.Scaled;
                item[Schema.ActivityAttemptItem.SessionTime] = act.DataModel.SessionTime.Ticks;
                item[Schema.ActivityAttemptItem.SuccessStatus] = act.DataModel.SuccessStatus;
                item[Schema.ActivityAttemptItem.SuspendData] = act.DataModel.SuspendData;
                item[Schema.ActivityAttemptItem.TotalTime] = act.DataModel.TotalTime.Ticks;
                item[Schema.ActivityAttemptItem.EvaluationPoints] = act.DataModel.EvaluationPoints;
                LearningStoreItemIdentifier activityId = new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, act.InternalActivityId);
                job.UpdateItem(activityId, item);

                // add all extension data that belongs to the datamodel as a whole
                act.DataModel.AdvancedAccess = true;
                foreach(KeyValuePair<string, object> ext in act.DataModel.ExtensionData)
                {
                    IAttachment attachment = ext.Value as IAttachment;
                    if(attachment != null)
                    {
                        item.Clear();
                        unique.Clear();
                        unique[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                        item[Schema.ExtensionDataItem.Name] = ext.Key;
                        unique[Schema.ExtensionDataItem.AttachmentGuid] = attachment.Guid;
                        item[Schema.ExtensionDataItem.AttemptObjectiveId] = null;
                        item[Schema.ExtensionDataItem.InteractionId] = null;
                        item[Schema.ExtensionDataItem.StringValue] = null;
                        item[Schema.ExtensionDataItem.IntValue] = null;
                        item[Schema.ExtensionDataItem.BoolValue] = null;
                        item[Schema.ExtensionDataItem.DoubleValue] = null;
                        item[Schema.ExtensionDataItem.DateTimeValue] = null;
                        item[Schema.ExtensionDataItem.AttachmentValue] = attachment.GetBytes();
                        job.AddOrUpdateItem(Schema.ExtensionDataItem.ItemTypeName, unique, item);
                    }
                    else
                    {
                        item.Clear();
                        item[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                        item[Schema.ExtensionDataItem.Name] = ext.Key;
                        if(ext.Value is string)
                        {
                            item[Schema.ExtensionDataItem.StringValue] = ext.Value;
                        }
                        else if(ext.Value is int)
                        {
                            item[Schema.ExtensionDataItem.IntValue] = ext.Value;
                        }
                        else if(ext.Value is bool)
                        {
                            item[Schema.ExtensionDataItem.BoolValue] = ext.Value;
                        }
                        else if(ext.Value is double)
                        {
                            item[Schema.ExtensionDataItem.DoubleValue] = ext.Value;
                        }
                        else if(ext.Value is DateTime)
                        {
                            item[Schema.ExtensionDataItem.DateTimeValue] = ext.Value;
                        }
                        else
                        {
                            throw new LearningComponentsInternalException("DBN0100");
                        }
                        job.AddItem(Schema.ExtensionDataItem.ItemTypeName, item);
                    }
                }
                act.DataModel.AdvancedAccess = false;

                Dictionary<string, LearningStoreItemIdentifier> attemptObjectives = new Dictionary<string, LearningStoreItemIdentifier>();

                // add all objectives
                // issue:  how is it even possible to connect these to the static ActivityObjectiveItem records?
                //         should we just merge them?  
                //         Carry around an "originalId" field to do searches on in the DataModel.Objective class?
                foreach(Objective obj in act.DataModel.Objectives)
                {
                    item.Clear();
                    item[Schema.AttemptObjectiveItem.ActivityAttemptId] = activityId;
                    item[Schema.AttemptObjectiveItem.CompletionStatus] = obj.CompletionStatus;
                    item[Schema.AttemptObjectiveItem.Description] = obj.Description;
                    item[Schema.AttemptObjectiveItem.IsPrimaryObjective] = obj.IsPrimaryObjective;
                    item[Schema.AttemptObjectiveItem.Key] = obj.Id;
                    item[Schema.AttemptObjectiveItem.LessonStatus] = obj.Status;
                    item[Schema.AttemptObjectiveItem.MaxScore] = obj.Score.Maximum;
                    item[Schema.AttemptObjectiveItem.MinScore] = obj.Score.Minimum;
                    item[Schema.AttemptObjectiveItem.ProgressMeasure] = obj.ProgressMeasure;
                    item[Schema.AttemptObjectiveItem.RawScore] = obj.Score.Raw;
                    item[Schema.AttemptObjectiveItem.ScaledScore] = obj.Score.Scaled;
                    item[Schema.AttemptObjectiveItem.SuccessStatus] = obj.SuccessStatus;
                    LearningStoreItemIdentifier id = job.AddItem(Schema.AttemptObjectiveItem.ItemTypeName, item);
                    attemptObjectives.Add(obj.Id, id);

                    // extension data may be held for each individual objective, so save that.
                    act.DataModel.AdvancedAccess = true;
                    foreach(KeyValuePair<string, object> ext in obj.ExtensionData)
                    {
                        IAttachment attachment = ext.Value as IAttachment;
                        if(attachment != null)
                        {
                            item.Clear();
                            unique.Clear();
                            unique[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                            item[Schema.ExtensionDataItem.Name] = ext.Key;
                            unique[Schema.ExtensionDataItem.AttachmentGuid] = attachment.Guid;
                            item[Schema.ExtensionDataItem.AttemptObjectiveId] = id;
                            item[Schema.ExtensionDataItem.InteractionId] = null;
                            item[Schema.ExtensionDataItem.StringValue] = null;
                            item[Schema.ExtensionDataItem.IntValue] = null;
                            item[Schema.ExtensionDataItem.BoolValue] = null;
                            item[Schema.ExtensionDataItem.DoubleValue] = null;
                            item[Schema.ExtensionDataItem.DateTimeValue] = null;
                            item[Schema.ExtensionDataItem.AttachmentValue] = attachment.GetBytes();
                            job.AddOrUpdateItem(Schema.ExtensionDataItem.ItemTypeName, unique, item);
                        }
                        else
                        {
                            item.Clear();
                            item[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                            item[Schema.ExtensionDataItem.AttemptObjectiveId] = id;
                            item[Schema.ExtensionDataItem.Name] = ext.Key;
                            if(ext.Value is string)
                            {
                                item[Schema.ExtensionDataItem.StringValue] = ext.Value;
                            }
                            else if(ext.Value is int)
                            {
                                item[Schema.ExtensionDataItem.IntValue] = ext.Value;
                            }
                            else if(ext.Value is bool)
                            {
                                item[Schema.ExtensionDataItem.BoolValue] = ext.Value;
                            }
                            else if(ext.Value is double)
                            {
                                item[Schema.ExtensionDataItem.DoubleValue] = ext.Value;
                            }
                            else if(ext.Value is DateTime)
                            {
                                item[Schema.ExtensionDataItem.DateTimeValue] = ext.Value;
                            }
                            else
                            {
                                throw new LearningComponentsInternalException("DBN0200");
                            }
                            job.AddItem(Schema.ExtensionDataItem.ItemTypeName, item);
                        }
                    }
                    act.DataModel.AdvancedAccess = true;
                }

                // save all comments for this activity
                for(int ordinal = 0 ; ordinal < act.DataModel.CommentsFromLearner.Count ; ++ordinal)
                {
                    item.Clear();
                    item[Schema.CommentFromLearnerItem.ActivityAttemptId] = activityId;
                    item[Schema.CommentFromLearnerItem.Comment] = act.DataModel.CommentsFromLearner[ordinal].CommentText;
                    item[Schema.CommentFromLearnerItem.Location] = act.DataModel.CommentsFromLearner[ordinal].Location;
                    item[Schema.CommentFromLearnerItem.Timestamp] = act.DataModel.CommentsFromLearner[ordinal].Timestamp;
                    item[Schema.CommentFromLearnerItem.Ordinal] = ordinal;
                    job.AddItem(Schema.CommentFromLearnerItem.ItemTypeName, item);
                }

                // save all interactions for this activity
                foreach(Interaction interaction in act.DataModel.Interactions)
                {
                    item.Clear();
                    item[Schema.InteractionItem.ActivityAttemptId] = activityId;
                    item[Schema.InteractionItem.Description] = interaction.Description;
                    item[Schema.InteractionItem.EvaluationPoints] = interaction.Evaluation.Points;
                    item[Schema.InteractionItem.InteractionIdFromCmi] = interaction.Id;
                    item[Schema.InteractionItem.InteractionType] = interaction.InteractionType;
                    item[Schema.InteractionItem.Latency] = interaction.Latency.Ticks;
                    object resp = interaction.LearnerResponse;
                    if(resp != null)
                    {
                        if(resp is bool)
                        {
                            item[Schema.InteractionItem.LearnerResponseBool] = resp;
                        }
                        else if(resp is float)
                        {
                            item[Schema.InteractionItem.LearnerResponseNumeric] = resp;
                        }
                        else if(resp is string)
                        {
                            item[Schema.InteractionItem.LearnerResponseString] = resp;
                        }
                        else
                        {
                            throw new LearningComponentsInternalException("DBN0300");
                        }
                    }
                    item[Schema.InteractionItem.MaxScore] = interaction.Score.Maximum;
                    item[Schema.InteractionItem.MinScore] = interaction.Score.Minimum;
                    item[Schema.InteractionItem.RawScore] = interaction.Score.Raw;
                    item[Schema.InteractionItem.ResultState] = interaction.Result.State;
                    item[Schema.InteractionItem.ResultNumeric] = interaction.Result.NumericResult;
                    item[Schema.InteractionItem.ScaledScore] = interaction.Score.Scaled;
                    item[Schema.InteractionItem.Timestamp] = interaction.Timestamp;
                    item[Schema.InteractionItem.Weighting] = interaction.Weighting;
                    LearningStoreItemIdentifier id = job.AddItem(Schema.InteractionItem.ItemTypeName, item);

                    // save extension data associated with this interaction
                    act.DataModel.AdvancedAccess = true;
                    foreach(KeyValuePair<string, object> ext in interaction.ExtensionData)
                    {
                        IAttachment attachment = ext.Value as IAttachment;
                        if(attachment != null)
                        {
                            item.Clear();
                            unique.Clear();
                            unique[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                            item[Schema.ExtensionDataItem.Name] = ext.Key;
                            unique[Schema.ExtensionDataItem.AttachmentGuid] = attachment.Guid;
                            item[Schema.ExtensionDataItem.AttemptObjectiveId] = null;
                            item[Schema.ExtensionDataItem.InteractionId] = id;
                            item[Schema.ExtensionDataItem.StringValue] = null;
                            item[Schema.ExtensionDataItem.IntValue] = null;
                            item[Schema.ExtensionDataItem.BoolValue] = null;
                            item[Schema.ExtensionDataItem.DoubleValue] = null;
                            item[Schema.ExtensionDataItem.DateTimeValue] = null;
                            item[Schema.ExtensionDataItem.AttachmentValue] = attachment.GetBytes();
                            job.AddOrUpdateItem(Schema.ExtensionDataItem.ItemTypeName, unique, item);
                        }
                        else
                        {
                            item.Clear();
                            item[Schema.ExtensionDataItem.ActivityAttemptId] = activityId;
                            item[Schema.ExtensionDataItem.InteractionId] = id;
                            item[Schema.ExtensionDataItem.Name] = ext.Key;
                            if(ext.Value is string)
                            {
                                item[Schema.ExtensionDataItem.StringValue] = ext.Value;
                            }
                            else if(ext.Value is int)
                            {
                                item[Schema.ExtensionDataItem.IntValue] = ext.Value;
                            }
                            else if(ext.Value is bool)
                            {
                                item[Schema.ExtensionDataItem.BoolValue] = ext.Value;
                            }
                            else if(ext.Value is double)
                            {
                                item[Schema.ExtensionDataItem.DoubleValue] = ext.Value;
                            }
                            else if(ext.Value is DateTime)
                            {
                                item[Schema.ExtensionDataItem.DateTimeValue] = ext.Value;
                            }
                            else
                            {
                                throw new LearningComponentsInternalException("DBN0400");
                            }
                            job.AddItem(Schema.ExtensionDataItem.ItemTypeName, item);
                        }
                    }
                    act.DataModel.AdvancedAccess = false;

                    // save all correct response items this interaction
                    foreach(CorrectResponse cr in interaction.CorrectResponses)
                    {
                        item.Clear();
                        item[Schema.CorrectResponseItem.InteractionId] = id;
                        item[Schema.CorrectResponseItem.ResponsePattern] = cr.Pattern;
                        job.AddItem(Schema.CorrectResponseItem.ItemTypeName, item);
                    }

                    // save objective information for this interaction
                    foreach(InteractionObjective io in interaction.Objectives)
                    {
                        LearningStoreItemIdentifier objId;

                        // if an attemptObjectiveItem record already exists with this string, point to it
                        // otherwise, we need to add one.
                        if(!attemptObjectives.TryGetValue(io.Id, out objId))
                        {
                            item.Clear();
                            item[Schema.AttemptObjectiveItem.ActivityAttemptId] = activityId;
                            item[Schema.AttemptObjectiveItem.Key] = io.Id;
                            objId = job.AddItem(Schema.AttemptObjectiveItem.ItemTypeName, item);
                            attemptObjectives.Add(io.Id, objId);
                        }

                        item.Clear();
                        item[Schema.InteractionObjectiveItem.InteractionId] = id;
                        item[Schema.InteractionObjectiveItem.AttemptObjectiveId] = objId;
                        job.AddItem(Schema.InteractionObjectiveItem.ItemTypeName, item);
                    }

                    for(int ordinal = 0 ; ordinal < interaction.Evaluation.Comments.Count ; ++ordinal)
                    {
                        item.Clear();
                        item[Schema.EvaluationCommentItem.Comment] = interaction.Evaluation.Comments[ordinal].CommentText;
                        item[Schema.EvaluationCommentItem.InteractionId] = id;
                        item[Schema.EvaluationCommentItem.Location] = interaction.Evaluation.Comments[ordinal].Location;
                        item[Schema.EvaluationCommentItem.Ordinal] = ordinal;
                        item[Schema.EvaluationCommentItem.Timestamp] = interaction.Evaluation.Comments[ordinal].Timestamp;
                        job.AddItem(Schema.EvaluationCommentItem.ItemTypeName, item);
                    }

                    for(int ordinal = 0 ; ordinal < interaction.Rubrics.Count ; ++ordinal)
                    {
                        item.Clear();
                        item[Schema.RubricItem.InteractionId] = id;
                        item[Schema.RubricItem.IsSatisfied] = interaction.Rubrics[ordinal].IsSatisfied;
                        item[Schema.RubricItem.Ordinal] = ordinal;
                        item[Schema.RubricItem.Points] = interaction.Rubrics[ordinal].Points;
                        job.AddItem(Schema.RubricItem.ItemTypeName, item);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the xml version of the data model for all activities in this attempt.
        /// </summary>
        public void DeleteDataModelCache()
        {
            LoadActivityTree();
            if(m_status == AttemptStatus.Active ||
                m_status == AttemptStatus.Suspended)
            {
                throw new InvalidOperationException(Resources.AttemptHasNotEnded);
            }
            // just in case someone adds to this enum later
            Utilities.Assert(m_status == AttemptStatus.Abandoned || m_status == AttemptStatus.Completed, "DBN1000");

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);
            foreach(Activity activity in Traverse)
            {
                Dictionary<string, object> activityAttempt = new Dictionary<string, object>(2);

                activityAttempt.Add(Schema.ActivityAttemptItem.DataModelCache, null);
                activityAttempt.Add(Schema.ActivityAttemptItem.SequencingDataCache, null);
                job.UpdateItem(new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, activity.InternalActivityId), activityAttempt);
            }
            job.Execute();
        }

        /// <summary>
        /// Converts a column of xml data from <Typ>LearningStore</Typ> to an <Typ>XPathNavigator</Typ>.
        /// </summary>
        /// <param name="xml">The raw column object.</param>
        /// <param name="readOnly">Whether or not the <Typ>XPathNavigator</Typ> created should be read-only.</param>
        /// <returns>A new <Typ>XPathNavigator</Typ>, or null if the column is null.</returns>
        static protected XPathNavigator ConvertLearningStoreXmlToXPathNavigator(LearningStoreXml xml, bool readOnly)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            if(xml == null)
            {
                Utilities.Assert(!readOnly);
                return null;
            }

            if(readOnly)
            {
                XPathDocument doc;
                using(XmlReader reader = xml.CreateReader())
                {
                    doc = new XPathDocument(reader);
                }
                return doc.CreateNavigator();
            }
            XmlDocument doc2 = new XmlDocument();
            using(XmlReader reader = xml.CreateReader())
            {
                doc2.Load(reader);
            }
            return doc2.CreateNavigator();
        }

        /// <summary>
        /// Updates the attempt status based on data in the activity tree.
        /// </summary>
        /// <param name="command">The navigation command that was just performed successfully.</param>
        /// <param name="shouldExit">True if the attempt is being exited.</param>
        protected void UpdateStatus(NavigationCommand command, bool shouldExit)
        {
            Utilities.Assert(m_attemptItemInformationIsValid);
            AttemptStatus status;
            if(shouldExit)
            {
                if(command == NavigationCommand.AbandonAll || command == NavigationCommand.Abandon)
                {
                    status = AttemptStatus.Abandoned;
                }
                else if(command == NavigationCommand.SuspendAll)
                {
                    status = AttemptStatus.Suspended;
                }
                else
                {
                    status = AttemptStatus.Completed;
                }
            }
            else
            {
                status = AttemptStatus.Active;
            }
            if(status != m_status)
            {
                if(status == AttemptStatus.Abandoned || status == AttemptStatus.Completed)
                {
                    m_expandOnNextSave = true;
                    m_attemptEndTime = DateTime.UtcNow;
                }
                m_status = status;
                m_statusChanged = true;
            }
        }

        /// <summary>
        /// Creates an xml block of comments from LMS, or null if there are no comments.
        /// </summary>
        /// <param name="commentData">A DataTable from the database containing the comments.</param>
        /// <returns>An XPathNavigator with valid LMS comments, or null</returns>
        static protected XPathNavigator CreateCommentsFromLms(DataTable commentData)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            XPathNavigator commentNav = null;

            if(commentData != null && commentData.Rows.Count > 0)
            {
                // create xml for comments from lms
                XmlDocument doc = new XmlDocument();
                commentNav = doc.CreateNavigator();
                using(XmlWriter writer = commentNav.AppendChild())
                {
                    writer.WriteStartElement("commentsFromLMS");
                    foreach(DataRow row in commentData.Rows)
                    {
                        writer.WriteStartElement("comment");
                        if(row[Schema.CommentFromLmsItem.Location] is string)
                        {
                            writer.WriteAttributeString("location", (string)row[Schema.CommentFromLmsItem.Location]);
                        }
                        if(row[Schema.CommentFromLmsItem.Timestamp] is string)
                        {
                            writer.WriteAttributeString("timestamp", (string)row[Schema.CommentFromLmsItem.Timestamp]);
                        }
                        writer.WriteValue((string)row[Schema.CommentFromLmsItem.Comment]);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
            return commentNav;
        }

        /// <summary>
        /// Wraps an attachment in a class that will also provide a GUID.
        /// </summary>
        /// <param name="bytes">The bytes of the attachment to wrap.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        /// <returns>A new representation of the attachment.</returns>
        protected IAttachment WrapAttachment(byte[] bytes, long internalActivityId)
        {
            AttachmentWrapper wrapper = new AttachmentWrapper(bytes, internalActivityId);
            m_newAttachments.Add(wrapper.Guid, wrapper);
            return wrapper;
        }

        /// <summary>
        /// Provides a standard interface for accessing a GUID like an attachment
        /// </summary>
        /// <param name="guid">The guid of the attachment stored in the database.</param>
        /// <param name="internalActivityId">Internal activity id used when saving attachments to the database.</param>
        /// <returns>A representation of the attachment.</returns>
        protected IAttachment WrapAttachmentGuid(Guid guid, long internalActivityId)
        {
            AttachmentWrapper attachment;

            if(m_newAttachments.TryGetValue(guid, out attachment))
            {
                return attachment;
            }
            return new AttachmentWrapper(this, m_store, guid, internalActivityId);
        }

        /// <summary>
        /// Loads the entire activity tree from the database, if it is not already in memory.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid.</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public override void LoadActivityTree()
        {
            // if RootActivity is not null, either this method or CreateExecuteNavigator()
            // has been called already.  In either of case, we do not need to load the activity 
            // tree again
            if(RootActivity != null)
            {
                return;
            }
            
            DataModelWriteValidationMode writeValidationMode;

            if(this is ExecuteNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AllowWriteOnlyIfActive;
            }
            else if(this is ReviewNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.NeverAllowWrite;
            }
            else if(this is RandomAccessNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AlwaysAllowWrite;
            }
            else
            {
                throw new LearningComponentsInternalException("DBN2000");
            }

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);

            // first query for all information about the attempt
            LearningStoreQuery query = m_store.CreateQuery(Schema.SeqNavAttemptView.ViewName);

            // if m_attemptItemInformationIsValid is true, these items are already in memory
            if (!m_attemptItemInformationIsValid)
            {
                query.AddColumn(Schema.SeqNavAttemptView.AttemptStatus);
                query.AddColumn(Schema.SeqNavAttemptView.LogDetailSequencing);
                query.AddColumn(Schema.SeqNavAttemptView.LogFinalSequencing);
                query.AddColumn(Schema.SeqNavAttemptView.LogRollup);
                query.AddColumn(Schema.SeqNavAttemptView.PackageId);
                query.AddColumn(Schema.SeqNavAttemptView.PackageFormat);
                query.AddColumn(Schema.SeqNavAttemptView.PackagePath);
                query.AddColumn(Schema.SeqNavAttemptView.CurrentActivityId);
                query.AddColumn(Schema.SeqNavAttemptView.StartedTimestamp);
                query.AddColumn(Schema.SeqNavAttemptView.FinishedTimestamp);
                query.AddColumn(Schema.SeqNavAttemptView.TotalPoints);
                query.AddColumn(Schema.SeqNavAttemptView.CompletionStatus);
                query.AddColumn(Schema.SeqNavAttemptView.SuccessStatus);
            }
            query.AddColumn(Schema.SeqNavAttemptView.SuspendedActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.RootActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerId);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerName);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioCaptioning);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioLevel);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerDeliverySpeed);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerLanguage);
            query.AddCondition(Schema.SeqNavAttemptView.Id, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            // then query for the activity tree
            query = m_store.CreateQuery(Schema.SeqNavActivityAttemptView.ViewName);
            query.AddColumn(Schema.SeqNavActivityAttemptView.Id);
            query.AddColumn(Schema.SeqNavActivityAttemptView.SequencingDataCache);
            query.AddColumn(Schema.SeqNavActivityAttemptView.RandomPlacement);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ActivityPackageId);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ParentId);
            query.AddColumn(Schema.SeqNavActivityAttemptView.StaticDataModelCache);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ObjectivesGlobalToSystem);
            query.AddCondition(Schema.SeqNavActivityAttemptView.AttemptId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            ReadOnlyCollection<object> c = job.Execute();
            // c[0] is learner/attempt information
            // c[1] is the activity tree

            DataTable d = (DataTable)c[0];  // this is the attemptItem
            if(d.Rows.Count < 1)
            {
                throw new LearningStoreItemNotFoundException(Resources.InvalidAttemptId);
            }
            Utilities.Assert(d.Rows.Count == 1);

            string learnerName = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerName];
            AudioCaptioning learnerCaption = (AudioCaptioning)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioCaptioning];
            float learnerAudioLevel = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioLevel];
            float learnerDeliverySpeed = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerDeliverySpeed];
            string learnerLanguage = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerLanguage];
            long rootActivityId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.RootActivityId]).GetKey();

            LearningStoreItemIdentifier curActivityId = null;
            if(!m_attemptItemInformationIsValid)
            {
                curActivityId = d.Rows[0][Schema.SeqNavAttemptView.CurrentActivityId] as LearningStoreItemIdentifier;
                m_packageFormat = (PackageFormat)d.Rows[0][Schema.SeqNavAttemptView.PackageFormat];
                m_packageId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.PackageId]).GetKey();
                m_packageLocation = (string)d.Rows[0][Schema.SeqNavAttemptView.PackagePath];
                m_learnerId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.LearnerId]).GetKey();
                m_status = (AttemptStatus)d.Rows[0][Schema.SeqNavAttemptView.AttemptStatus];
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogDetailSequencing])
                {
                    m_loggingFlags |= LoggingOptions.LogDetailSequencing;
                }
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogFinalSequencing])
                {
                    m_loggingFlags |= LoggingOptions.LogFinalSequencing;
                }
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogRollup])
                {
                    m_loggingFlags |= LoggingOptions.LogRollup;
                }
                m_attemptStartTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.StartedTimestamp], DateTimeKind.Utc);
                if(d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp] is DateTime)
                {
                    m_attemptEndTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp], DateTimeKind.Utc);
                }
                m_totalPoints = d.Rows[0][Schema.SeqNavAttemptView.TotalPoints] as float?;
                m_completionStatus = (CompletionStatus)d.Rows[0][Schema.SeqNavAttemptView.CompletionStatus];
                m_successStatus = (SuccessStatus)d.Rows[0][Schema.SeqNavAttemptView.SuccessStatus];
            }

            // we must set this here so that the Activity  constructor doesn't try and load missing info
            m_attemptItemInformationIsValid = true;

			// need this value for the loop below
            LearningStoreItemIdentifier suspendedActivityId = d.Rows[0][Schema.SeqNavAttemptView.SuspendedActivityId] as LearningStoreItemIdentifier; ;

            d = (DataTable)c[1]; // this is the activity tree
            int randomPosition;
            
            // create activity objects and store them in a big dictionary
            foreach(DataRow row in d.Rows)
            {
                if(row[Schema.SeqNavActivityAttemptView.RandomPlacement] is DBNull)
                {
                    randomPosition = -1;
                }
                else
                {
                    randomPosition = (int)row[Schema.SeqNavActivityAttemptView.RandomPlacement];
                }
                if(m_currentActivity == null || ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey() != m_currentActivity.ActivityId)
                {
                    Activity act = new Activity(this, ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey(),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityAttemptView.StaticDataModelCache] as LearningStoreXml, true),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityAttemptView.SequencingDataCache] as LearningStoreXml, false),
                        null, null, WrapAttachment, WrapAttachmentGuid, randomPosition, 
                        (bool)row[Schema.SeqNavActivityAttemptView.ObjectivesGlobalToSystem], writeValidationMode,
                        m_learnerId.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                        learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
                    act.InternalActivityId = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.Id]).GetKey();
                    m_activities.Add(act.ActivityId, act);
                    if(act.ActivityId == rootActivityId)
                    {
                        RootActivity = act;
                    }
                    if(curActivityId != null && act.ActivityId == curActivityId.GetKey())
                    {
                        m_currentActivity = act;
                    }
                    if(suspendedActivityId != null && act.ActivityId == suspendedActivityId.GetKey())
                    {
                        m_suspendedActivity = act;
                    }
                }
            }

            // now that we have all the activities in a big dictionary, find all the parents to build the tree
            // in theory this could be done in the first loop if I sort the query by parentid, but that's making a lot
            // of assumptions about the structure of the database that I don't think are safe to make
            foreach(DataRow row in d.Rows)
            {
                LearningStoreItemIdentifier parentId = row["ParentId"] as LearningStoreItemIdentifier;
                if(parentId != null)
                {
                    long id = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey();
                    m_activities[id].Parent = m_activities[parentId.GetKey()];
                    m_activities[parentId.GetKey()].AddChild(m_activities[id]);
                }
            }

            SortActivityTree();
        }

        /// <summary>
        /// Loads all the data model data for 
        /// </summary>
        private void LoadAllDataModelData()
        {
            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);
            LearningStoreQuery query;
            bool alreadyRead = true;

            foreach(Activity act in Traverse)
            {
                if(!act.DataModel.DynamicDataIsValid)
                {
                    alreadyRead = false;
                    query = m_store.CreateQuery(Schema.SeqNavActivityAttemptView.ViewName);
                    query.AddColumn(Schema.SeqNavActivityAttemptView.DataModelCache);
                    query.AddCondition(Schema.SeqNavActivityAttemptView.Id, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, act.InternalActivityId));
                    job.PerformQuery(query);
                }
            }

            // apparently we've already read this into memory before - don't do anything
            if(alreadyRead)
            {
                return;
            }

            ReadOnlyCollection<object> c = job.Execute();
            int i = 0;
            foreach(Activity act in Traverse)
            {
                if(!act.DataModel.DynamicDataIsValid)
                {
                    act.DataModel.SetDynamicData(ConvertLearningStoreXmlToXPathNavigator(((DataTable)c[i++]).Rows[0][Schema.SeqNavActivityAttemptView.DataModelCache] as LearningStoreXml, false), null);
                    act.DataModel.DynamicDataIsValid = true;
                }
            }
        }

        /// <summary>
        /// Loads the entire activity tree including all data model information from the database.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the attempt ID is invalid.</exception>
        private void LoadFullActivityTree()
        {
            // there is no real way to determine if the entire data model is already in memory, but there
            // are a few things we can do to minimize these calls.

            
            // if RootActivity is not null, then the entire tree is in memory, but not necessarily the data
            // model data, so just load that
            if(RootActivity != null)
            {
                LoadAllDataModelData();
                return;
            }
            
            DataModelWriteValidationMode writeValidationMode;

            if(this is ExecuteNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AllowWriteOnlyIfActive;
            }
            else if(this is ReviewNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.NeverAllowWrite;
            }
            else if(this is RandomAccessNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AlwaysAllowWrite;
            }
            else
            {
                throw new LearningComponentsInternalException("DBN2000");
            }

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);

            // first query for all information about the attempt
            LearningStoreQuery query = m_store.CreateQuery(Schema.SeqNavAttemptView.ViewName);

            // if m_attemptItemInformationIsValid is true, these items are already in memory
            if (!m_attemptItemInformationIsValid)
            {
                query.AddColumn(Schema.SeqNavAttemptView.AttemptStatus);
                query.AddColumn(Schema.SeqNavAttemptView.LogDetailSequencing);
                query.AddColumn(Schema.SeqNavAttemptView.LogFinalSequencing);
                query.AddColumn(Schema.SeqNavAttemptView.LogRollup);
                query.AddColumn(Schema.SeqNavAttemptView.PackageId);
                query.AddColumn(Schema.SeqNavAttemptView.PackageFormat);
                query.AddColumn(Schema.SeqNavAttemptView.PackagePath);
                query.AddColumn(Schema.SeqNavAttemptView.CurrentActivityId);
                query.AddColumn(Schema.SeqNavAttemptView.StartedTimestamp);
                query.AddColumn(Schema.SeqNavAttemptView.FinishedTimestamp);
            }
            query.AddColumn(Schema.SeqNavAttemptView.SuspendedActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.RootActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerId);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerName);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioCaptioning);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioLevel);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerDeliverySpeed);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerLanguage);
            query.AddCondition(Schema.SeqNavAttemptView.Id, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            // then query for the activity tree
            query = m_store.CreateQuery(Schema.SeqNavActivityAttemptView.ViewName);
            query.AddColumn(Schema.SeqNavActivityAttemptView.Id);
            query.AddColumn(Schema.SeqNavActivityAttemptView.DataModelCache);
            query.AddColumn(Schema.SeqNavActivityAttemptView.SequencingDataCache);
            query.AddColumn(Schema.SeqNavActivityAttemptView.RandomPlacement);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ActivityPackageId);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ParentId);
            query.AddColumn(Schema.SeqNavActivityAttemptView.StaticDataModelCache);
            query.AddColumn(Schema.SeqNavActivityAttemptView.ObjectivesGlobalToSystem);
            query.AddCondition(Schema.SeqNavActivityAttemptView.AttemptId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            ReadOnlyCollection<object> c = job.Execute();
            // c[0] is learner/attempt information
            // c[1] is the activity tree

            DataTable d = (DataTable)c[0];  // this is the attemptItem
            if(d.Rows.Count < 1)
            {
                throw new LearningStoreItemNotFoundException(Resources.InvalidAttemptId);
            }
            Utilities.Assert(d.Rows.Count == 1);

            string learnerName = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerName];
            AudioCaptioning learnerCaption = (AudioCaptioning)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioCaptioning];
            float learnerAudioLevel = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioLevel];
            float learnerDeliverySpeed = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerDeliverySpeed];
            string learnerLanguage = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerLanguage];
            long rootActivityId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.RootActivityId]).GetKey();

            LearningStoreItemIdentifier curActivityId = null;
            if(!m_attemptItemInformationIsValid)
            {
                curActivityId = d.Rows[0][Schema.SeqNavAttemptView.CurrentActivityId] as LearningStoreItemIdentifier;
                m_packageFormat = (PackageFormat)d.Rows[0][Schema.SeqNavAttemptView.PackageFormat];
                m_packageId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.PackageId]).GetKey();
                m_packageLocation = (string)d.Rows[0][Schema.SeqNavAttemptView.PackagePath];
                m_learnerId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.LearnerId]).GetKey();
                m_status = (AttemptStatus)d.Rows[0][Schema.SeqNavAttemptView.AttemptStatus];
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogDetailSequencing])
                {
                    m_loggingFlags |= LoggingOptions.LogDetailSequencing;
                }
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogFinalSequencing])
                {
                    m_loggingFlags |= LoggingOptions.LogFinalSequencing;
                }
                if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogRollup])
                {
                    m_loggingFlags |= LoggingOptions.LogRollup;
                }
                m_attemptStartTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.StartedTimestamp], DateTimeKind.Utc);
                if(d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp] is DateTime)
                {
                    m_attemptEndTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp], DateTimeKind.Utc);
                }
            }

            // we must set this here so that the Activity  constructor doesn't try and load missing info
            m_attemptItemInformationIsValid = true;

			// need this value for the loop below
            LearningStoreItemIdentifier suspendedActivityId = d.Rows[0][Schema.SeqNavAttemptView.SuspendedActivityId] as LearningStoreItemIdentifier; ;

            d = (DataTable)c[1]; // this is the activity tree
            int randomPosition;
            
            // create activity objects and store them in a big dictionary
            foreach(DataRow row in d.Rows)
            {
                if (row[Schema.SeqNavActivityAttemptView.RandomPlacement] is DBNull)
                {
                    randomPosition = -1;
                }
                else
                {
                    randomPosition = (int)row[Schema.SeqNavActivityAttemptView.RandomPlacement];
                }
                if (m_currentActivity == null || ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey() != m_currentActivity.ActivityId)
                {
                    Activity act = new Activity(this, ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey(),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityAttemptView.StaticDataModelCache] as LearningStoreXml, true),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityAttemptView.SequencingDataCache] as LearningStoreXml, false),
                        ConvertLearningStoreXmlToXPathNavigator(row[Schema.SeqNavActivityAttemptView.DataModelCache] as LearningStoreXml, false),
                        null, WrapAttachment, WrapAttachmentGuid, randomPosition, 
                        (bool)row[Schema.SeqNavActivityAttemptView.ObjectivesGlobalToSystem], writeValidationMode,
                        m_learnerId.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                        learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
                    act.InternalActivityId = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.Id]).GetKey();
                    m_activities[act.ActivityId] = act;
                    if(act.ActivityId == rootActivityId)
                    {
                        RootActivity = act;
                    }
                    if(curActivityId != null && act.ActivityId == curActivityId.GetKey())
                    {
                        m_currentActivity = act;
                    }
                    if(suspendedActivityId != null && act.ActivityId == suspendedActivityId.GetKey())
                    {
                        m_suspendedActivity = act;
                    }
                    act.DataModel.DynamicDataIsValid = true;
                }
            }

            // now that we have all the activities in a big dictionary, find all the parents to build the tree
            // in theory this could be done in the first loop if I sort the query by parentid, but that's making a lot
            // of assumptions about the structure of the database that I don't think are safe to make
            foreach(DataRow row in d.Rows)
            {
                LearningStoreItemIdentifier parentId = row["ParentId"] as LearningStoreItemIdentifier;
                if(parentId != null)
                {
                    long id = ((LearningStoreItemIdentifier)row[Schema.SeqNavActivityAttemptView.ActivityPackageId]).GetKey();
                    m_activities[id].Parent = m_activities[parentId.GetKey()];
                    m_activities[parentId.GetKey()].AddChild(m_activities[id]);
                }
            }

            SortActivityTree();
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
        protected virtual void LoadLearningDataModel()
        {
            if(m_attemptItemInformationIsValid)
            {
                return;
            }
            DataModelWriteValidationMode writeValidationMode;

            if(this is ExecuteNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AllowWriteOnlyIfActive;
            }
            else if(this is ReviewNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.NeverAllowWrite;
            }
            else if(this is RandomAccessNavigator)
            {
                writeValidationMode = DataModelWriteValidationMode.AlwaysAllowWrite;
            }
            else
            {
                throw new LearningComponentsInternalException("DBN3000");
            }

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);

            // query for information about the attempt
            LearningStoreQuery query = m_store.CreateQuery(Schema.SeqNavAttemptView.ViewName);
            query.AddColumn(Schema.SeqNavAttemptView.AttemptStatus);
            query.AddColumn(Schema.SeqNavAttemptView.LogDetailSequencing);
            query.AddColumn(Schema.SeqNavAttemptView.LogFinalSequencing);
            query.AddColumn(Schema.SeqNavAttemptView.LogRollup);
            query.AddColumn(Schema.SeqNavAttemptView.PackageId);
            query.AddColumn(Schema.SeqNavAttemptView.PackageFormat);
            query.AddColumn(Schema.SeqNavAttemptView.PackagePath);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerId);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerName);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioCaptioning);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerAudioLevel);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerDeliverySpeed);
            query.AddColumn(Schema.SeqNavAttemptView.LearnerLanguage);
            query.AddColumn(Schema.SeqNavAttemptView.CurrentActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.SuspendedActivityId);
            query.AddColumn(Schema.SeqNavAttemptView.StartedTimestamp);
            query.AddColumn(Schema.SeqNavAttemptView.FinishedTimestamp);
            query.AddColumn(Schema.SeqNavAttemptView.TotalPoints);
            query.AddColumn(Schema.SeqNavAttemptView.CompletionStatus);
            query.AddColumn(Schema.SeqNavAttemptView.SuccessStatus);
            query.AddCondition(Schema.SeqNavAttemptView.Id, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            // query information about the current activity
            query = m_store.CreateQuery(Schema.SeqNavCurrentActivityAttemptView.ViewName);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.Id);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.DataModelCache);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.SequencingDataCache);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.RandomPlacement);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.StaticDataModelCache);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.ObjectivesGlobalToSystem);
            query.AddColumn(Schema.SeqNavCurrentActivityAttemptView.Credit);
            query.AddCondition(Schema.SeqNavCurrentActivityAttemptView.AttemptId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            // query comments from LMS related to the current activity
            query = m_store.CreateQuery(Schema.SeqNavCurrentCommentFromLmsView.ViewName);
            query.AddColumn(Schema.SeqNavCurrentCommentFromLmsView.Comment);
            query.AddColumn(Schema.SeqNavCurrentCommentFromLmsView.Location);
            query.AddColumn(Schema.SeqNavCurrentCommentFromLmsView.Timestamp);
            query.AddCondition(Schema.SeqNavCurrentCommentFromLmsView.AttemptId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId));
            job.PerformQuery(query);

            ReadOnlyCollection<object> c = job.Execute();
            Utilities.Assert(c.Count == 3);
            DataTable d = d = (DataTable)c[0];

            // if d.Rows.Count < 1 then the attempt id is invalid
            if(d.Rows.Count < 1)
            {
                throw new LearningStoreItemNotFoundException(Resources.InvalidAttemptId);
            }
            Utilities.Assert(d.Rows.Count == 1);

            m_packageFormat = (PackageFormat)d.Rows[0][Schema.SeqNavAttemptView.PackageFormat];
            m_packageId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.PackageId]).GetKey();
            m_packageLocation = (string)d.Rows[0][Schema.SeqNavAttemptView.PackagePath];
            m_learnerId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavAttemptView.LearnerId]).GetKey();
            m_status = (AttemptStatus)d.Rows[0][Schema.SeqNavAttemptView.AttemptStatus];
            m_totalPoints = d.Rows[0][Schema.SeqNavAttemptView.TotalPoints] as float?;
            m_completionStatus = (CompletionStatus)d.Rows[0][Schema.SeqNavAttemptView.CompletionStatus];
            m_successStatus = (SuccessStatus)d.Rows[0][Schema.SeqNavAttemptView.SuccessStatus];
            if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogDetailSequencing])
            {
                m_loggingFlags |= LoggingOptions.LogDetailSequencing;
            }
            if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogFinalSequencing])
            {
                m_loggingFlags |= LoggingOptions.LogFinalSequencing;
            }
            if ((bool)d.Rows[0][Schema.SeqNavAttemptView.LogRollup])
            {
                m_loggingFlags |= LoggingOptions.LogRollup;
            }
            m_attemptStartTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.StartedTimestamp], DateTimeKind.Utc);
            if(d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp] is DateTime)
            {
                m_attemptEndTime = DateTime.SpecifyKind((DateTime)d.Rows[0][Schema.SeqNavAttemptView.FinishedTimestamp], DateTimeKind.Utc);
            }

            m_attemptItemInformationIsValid = true;

            LearningStoreItemIdentifier curActivityId = d.Rows[0][Schema.SeqNavAttemptView.CurrentActivityId] as LearningStoreItemIdentifier;
            if(curActivityId == null)
            {
                return;
            }

            string learnerName = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerName];
            AudioCaptioning learnerCaption = (AudioCaptioning)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioCaptioning];
            float learnerAudioLevel = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerAudioLevel];
            float learnerDeliverySpeed = (float)d.Rows[0][Schema.SeqNavAttemptView.LearnerDeliverySpeed];
            string learnerLanguage = (string)d.Rows[0][Schema.SeqNavAttemptView.LearnerLanguage];
            d = (DataTable)c[1];
            DataTable commentData = (DataTable)c[2];

            // this is an assert because we should have already exited if there is no current activity
            Utilities.Assert(d.Rows.Count == 1);
            
            int randomPosition;
            if (d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.RandomPlacement] is DBNull)
            {
                randomPosition = -1;
            }
            else
            {
                randomPosition = (int)d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.RandomPlacement];
            }

            Activity act = new Activity(this, curActivityId.GetKey(),
                ConvertLearningStoreXmlToXPathNavigator(d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.StaticDataModelCache] as LearningStoreXml, true),
                ConvertLearningStoreXmlToXPathNavigator(d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.SequencingDataCache] as LearningStoreXml, false),
                ConvertLearningStoreXmlToXPathNavigator(d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.DataModelCache] as LearningStoreXml, false),
                CreateCommentsFromLms(commentData), WrapAttachment, WrapAttachmentGuid, randomPosition, 
                (bool)d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.ObjectivesGlobalToSystem], writeValidationMode, 
                m_learnerId.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                learnerName, learnerLanguage, learnerCaption, learnerAudioLevel, learnerDeliverySpeed);
            act.InternalActivityId = ((LearningStoreItemIdentifier)d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.Id]).GetKey();
            if(d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.Credit] is bool)
            {
                act.DataModel.Credit = (bool)d.Rows[0][Schema.SeqNavCurrentActivityAttemptView.Credit];
            }
            m_activities[act.ActivityId] = act;
            m_currentActivity = act;
            m_currentActivity.DataModel.DynamicDataIsValid = true;
            m_currentActivity.DataModel.CommentsFromLmsAreValid = true;
        }
    
        /// <summary>
        /// Loads the information about the current activity that would not have been read if
        /// the activity had not been the "current" activity previously.
        /// </summary>
        protected void UpdateCurrentActivityData()
        {
            UpdateActivityData(m_currentActivity);
        }

        /// <summary>
        /// Loads information about the specified activity that is not vital to sequencing
        /// but it necessary for an activity to be delivered.
        /// </summary>
        /// <param name="activity">Activity to update.</param>
        public override void UpdateActivityData(Activity activity)
        {
            // if either of these is non-null, then the CurrentActivity
            // contains a valid value.
            if(activity == null || 
                (activity.DataModel.DynamicDataIsValid && activity.DataModel.CommentsFromLmsAreValid))
            {
                return;
            }

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);
            LearningStoreQuery query;

            if(!activity.DataModel.DynamicDataIsValid)
            {
                // get the dynamic data
                query = m_store.CreateQuery(Schema.SeqNavActivityAttemptView.ViewName);
                query.AddColumn(Schema.SeqNavActivityAttemptView.DataModelCache);
                query.AddCondition(Schema.SeqNavActivityAttemptView.Id, LearningStoreConditionOperator.Equal, 
                    new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, activity.InternalActivityId));
                job.PerformQuery(query);
            }

            if(!activity.DataModel.CommentsFromLmsAreValid)
            {
                // query comments from LMS related to the activity
                query = m_store.CreateQuery(Schema.CommentFromLmsItem.ItemTypeName);
                query.AddColumn(Schema.CommentFromLmsItem.Comment);
                query.AddColumn(Schema.CommentFromLmsItem.Location);
                query.AddColumn(Schema.CommentFromLmsItem.Timestamp);
                query.AddCondition(Schema.CommentFromLmsItem.ActivityPackageId, LearningStoreConditionOperator.Equal, 
                    new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, activity.ActivityId));
                job.PerformQuery(query);
            }

            ReadOnlyCollection<object> c = job.Execute();

            int i = 0;
            XPathNavigator dynamicData;
            XPathNavigator commentData;
            if(!activity.DataModel.DynamicDataIsValid)
            {
                dynamicData = ConvertLearningStoreXmlToXPathNavigator(((DataTable)c[i++]).Rows[0][Schema.SeqNavActivityAttemptView.DataModelCache] as LearningStoreXml, false);
            }
            else
            {
                dynamicData = null;
            }
            if(!activity.DataModel.CommentsFromLmsAreValid)
            {
                commentData = CreateCommentsFromLms((DataTable)c[i]);
            }
            else
            {
                commentData = null;
            }

            activity.DataModel.SetDynamicData(dynamicData, commentData);
            activity.DataModel.DynamicDataIsValid = true;
            activity.DataModel.CommentsFromLmsAreValid = true;
        }

        /// <summary>
        /// Saves any data that has been changed to the database, if necessary.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public override void Save()
        {
            if(m_changed || m_statusChanged || m_totalPointsChanged || m_dirtyActivities.Count > 0 || m_newAttachments.Count > 0 || m_logEventList.Count > 0 || m_expandOnNextSave)
            {
                Dictionary<string, object> attempt = new Dictionary<string, object>();
                LearningStoreJob job = m_store.CreateJob();
                DemandSessionRight(job);

                // if m_changed is true, something in the AttemptItem has changed, so save it.
                if(m_changed || m_statusChanged || m_totalPointsChanged)
                {
                    if(m_changed)
                    {
                        if(m_currentActivity == null)
                        {
                            attempt[Schema.AttemptItem.CurrentActivityId] = null;
                        }
                        else
                        {
                            attempt[Schema.AttemptItem.CurrentActivityId] = new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, m_currentActivity.ActivityId);
                        }
                        if(m_suspendedActivity == null)
                        {
                            attempt[Schema.AttemptItem.SuspendedActivityId] = null;
                        }
                        else
                        {
                            attempt[Schema.AttemptItem.SuspendedActivityId] = new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, m_suspendedActivity.ActivityId);
                        }
                        attempt[Schema.AttemptItem.LogDetailSequencing] = ((m_loggingFlags & LoggingOptions.LogDetailSequencing) == LoggingOptions.LogDetailSequencing);
                        attempt[Schema.AttemptItem.LogFinalSequencing] = ((m_loggingFlags & LoggingOptions.LogFinalSequencing) == LoggingOptions.LogFinalSequencing);
                        attempt[Schema.AttemptItem.LogRollup] = ((m_loggingFlags & LoggingOptions.LogRollup) == LoggingOptions.LogRollup);
                        attempt[Schema.AttemptItem.StartedTimestamp] = m_attemptStartTime;
                        attempt[Schema.AttemptItem.FinishedTimestamp] = m_attemptEndTime;
                        attempt[Schema.AttemptItem.CompletionStatus] = m_completionStatus;
                        attempt[Schema.AttemptItem.SuccessStatus] = m_successStatus;
                    }
                    if(m_totalPointsChanged)
                    {
                        attempt[Schema.AttemptItem.TotalPoints] = m_totalPoints;
                    }
                    if(m_statusChanged)
                    {
                        attempt[Schema.AttemptItem.AttemptStatus] = m_status;
                    }
                    job.UpdateItem(new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId), attempt);
                }

                // save each activity in the collection of dirty activities.
                foreach(Activity a in m_dirtyActivities.Values)
                {
                    Dictionary<string, object> activity = new Dictionary<string, object>();
                    XPathNavigator seq;
                    XPathNavigator dyn;

                    a.DataModel.Export(out seq, out dyn);
                    using(XmlReader reader = dyn.ReadSubtree())
                    {
                        activity[Schema.ActivityAttemptItem.DataModelCache] = LearningStoreXml.CreateAndLoad(reader);
                    }
                    using(XmlReader reader = seq.ReadSubtree())
                    {
                        activity[Schema.ActivityAttemptItem.SequencingDataCache] = LearningStoreXml.CreateAndLoad(reader);
                    }
                    if(a.RandomPlacement >= 0)
                    {
                        activity[Schema.ActivityAttemptItem.RandomPlacement] = a.RandomPlacement;
                    }
                    job.UpdateItem(new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, a.InternalActivityId), activity);
                }

                foreach(GlobalObjective glob in m_globalObjectives.Values)
                {
                    if(glob.IsChanged)
                    {
                        Dictionary<string, object> globalObjective = new Dictionary<string, object>(2);
                        Dictionary<string, object> updateItems = new Dictionary<string, object>(1);
                        Dictionary<string, object> unique = new Dictionary<string, object>(2);

                        if (!RootActivity.ObjectivesGlobalToSystem)
                        {
                            globalObjective.Add(Schema.GlobalObjectiveItem.OrganizationId, new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, RootActivity.ActivityId));
                        }
                        else
                        {
                            globalObjective.Add(Schema.GlobalObjectiveItem.OrganizationId, null);
                        }
                        globalObjective.Add(Schema.GlobalObjectiveItem.Key, glob.ObjectiveId);

                        LearningStoreItemIdentifier globItem = job.AddOrUpdateItem(Schema.GlobalObjectiveItem.ItemTypeName, globalObjective, new Dictionary<string, object>());

                        if(RootActivity.ObjectivesGlobalToSystem)
                        {
                            unique.Add(Schema.LearnerGlobalObjectiveItem.GlobalObjectiveId, globItem);
                            unique.Add(Schema.LearnerGlobalObjectiveItem.LearnerId, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
                            if (glob.NormalizedMeasure.HasValue)
                            {
                                updateItems.Add(Schema.LearnerGlobalObjectiveItem.ScaledScore, glob.NormalizedMeasure.Value);
                            }
                            if (glob.SatisfiedStatus.HasValue)
                            {
                                if (glob.SatisfiedStatus.Value)
                                {
                                    updateItems.Add(Schema.LearnerGlobalObjectiveItem.SuccessStatus, SuccessStatus.Passed);
                                }
                                else
                                {
                                    updateItems.Add(Schema.LearnerGlobalObjectiveItem.SuccessStatus, SuccessStatus.Failed);
                                }
                            }
                            else
                            {
                                updateItems.Add(Schema.LearnerGlobalObjectiveItem.SuccessStatus, SuccessStatus.Unknown);
                            }
                            job.AddOrUpdateItem(Schema.LearnerGlobalObjectiveItem.ItemTypeName, unique, updateItems);
                        }
                        else
                        {
                            unique.Add(Schema.PackageGlobalObjectiveItem.GlobalObjectiveId, globItem);
                            updateItems.Add(Schema.PackageGlobalObjectiveItem.LearnerId, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
                            if(glob.NormalizedMeasure.HasValue)
                            {
                                updateItems.Add(Schema.PackageGlobalObjectiveItem.ScaledScore, glob.NormalizedMeasure.Value);
                            }
                            if (glob.SatisfiedStatus.HasValue)
                            {
                                if (glob.SatisfiedStatus.Value)
                                {
                                    updateItems.Add(Schema.PackageGlobalObjectiveItem.SuccessStatus, SuccessStatus.Passed);
                                }
                                else
                                {
                                    updateItems.Add(Schema.PackageGlobalObjectiveItem.SuccessStatus, SuccessStatus.Failed);
                                }
                            }
                            else
                            {
                                updateItems.Add(Schema.PackageGlobalObjectiveItem.SuccessStatus, SuccessStatus.Unknown);
                            }
                            job.AddOrUpdateItem(Schema.PackageGlobalObjectiveItem.ItemTypeName, unique, updateItems);
                        }
                    }
                }

                // save each new attachment that we got as part of extention data.
                foreach(AttachmentWrapper attachment in m_newAttachments.Values)
                {
                    Dictionary<string, object> extensionData = new Dictionary<string, object>();
                    
                    extensionData[Schema.ExtensionDataItem.ActivityAttemptId] = new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, attachment.InternalActivityId);
                    extensionData[Schema.ExtensionDataItem.AttachmentGuid] = attachment.Guid;
                    extensionData[Schema.ExtensionDataItem.AttachmentValue] = attachment.GetBytes();
                    job.AddItem(Schema.ExtensionDataItem.ItemTypeName, extensionData);
                }

                // save the sequencing log
                if(m_logEventList.Count > 0)
                {
                    LearningStoreItemIdentifier attemptId = new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId);

                    foreach(LogEvent ev in m_logEventList)
                    {
                        Dictionary<string, object> log = new Dictionary<string, object>();

                        log[Schema.SequencingLogEntryItem.AttemptId] = attemptId;
                        log[Schema.SequencingLogEntryItem.ActivityAttemptId] = ev.ActivityId;
                        log[Schema.SequencingLogEntryItem.EventType] = ev.EventType;
                        log[Schema.SequencingLogEntryItem.Message] = ev.Message;
                        log[Schema.SequencingLogEntryItem.NavigationCommand] = ev.Command;
                        log[Schema.SequencingLogEntryItem.Timestamp] = ev.TimeStamp;
                        job.AddItem(Schema.SequencingLogEntryItem.ItemTypeName, log);
                    }
                }

                TransactionOptions options = new TransactionOptions();
                options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
                using(LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
                {
                    NavigatorEnlistment enlist = new NavigatorEnlistment(this);

                    scope.Transaction.EnlistVolatile(enlist, EnlistmentOptions.None);
                    if(m_expandOnNextSave)
                    {
                        ExpandDataModelCache(job);
                        m_expandOnNextSave = false;
                    }

                    // execute the database job
                    job.Execute();
                    scope.Complete();
                }
                PendingCommitClearDirtyIndicators();
            }
        }

        /// <summary>
        /// Clear all indicators saying we are dirty, but save data so it can be rolled back
        /// </summary>
        public void PendingCommitClearDirtyIndicators()
        {
            foreach(KeyValuePair<string, GlobalObjective> kv in m_globalObjectives)
            {
                if(kv.Value.IsChanged)
                {
                    m_globalObjectivesBackup[kv.Key] = kv.Value;
                    kv.Value.ClearChanged();
                }
            }

            foreach(KeyValuePair<long, Activity> kv in m_dirtyActivities)
            {
                m_dirtyActivitiesBackup[kv.Key] = kv.Value;
            }
            m_dirtyActivities.Clear();

            foreach(KeyValuePair<Guid, AttachmentWrapper> kv in m_newAttachments)
            {
                m_newAttachmentsBackup[kv.Key] = kv.Value;
            }
            m_newAttachments.Clear();

            foreach(LogEvent ev in m_logEventList)
            {
                m_logEventListBackup.Add(ev);
            }
            m_logEventList.Clear();
            m_changed = false;
            m_statusChanged = false;
            m_totalPointsChanged = false;
        }

        /// <summary>
        /// Clear all indicators saying we are dirty
        /// </summary>
        public void ClearDirtyIndicators()
        {
            m_globalObjectivesBackup.Clear();
            m_dirtyActivitiesBackup.Clear();
            m_newAttachmentsBackup.Clear();
            m_logEventListBackup.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RollbackDirtyIndicators()
        {
            foreach(GlobalObjective glob in m_globalObjectivesBackup.Values)
            {
                glob.IsChanged = true;
            }
            foreach(KeyValuePair<long, Activity> kv in m_dirtyActivitiesBackup)
            {
                m_dirtyActivities[kv.Key] = kv.Value;
            }
            foreach(KeyValuePair<Guid, AttachmentWrapper> kv in m_newAttachmentsBackup)
            {
                m_newAttachments[kv.Key] = kv.Value;
            }
            foreach(LogEvent ev in m_logEventListBackup)
            {
                m_logEventList.Add(ev);
            }
            m_changed = true;
            m_statusChanged = true;
            m_totalPointsChanged = true;
        }

        /// <summary>
        /// Class to handle enlistment in parent transactions properly
        /// </summary>
        class NavigatorEnlistment : IEnlistmentNotification
        {
            private DatabaseNavigator m_navigator;

            public NavigatorEnlistment(DatabaseNavigator navigator)
            {
                m_navigator = navigator;
            }

            #region IEnlistmentNotification Members

            public void Commit(Enlistment enlistment)
            {
                m_navigator.ClearDirtyIndicators();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                m_navigator.RollbackDirtyIndicators();
                enlistment.Done();
            }

            #endregion
        }

        /// <summary>
        /// Reads the global objective information from LearningStore.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective.  Null is not allowed.</param>
        private void ReadGlobalObjectiveFromLearningStore(string globalObjective)
        {
            Utilities.Assert(!String.IsNullOrEmpty(globalObjective));

            LearningStoreJob job = m_store.CreateJob();
            DemandSessionRight(job);
            LearningStoreQuery query;
            string statusColumnName;
            string measureColumnName;
            if(RootActivity.ObjectivesGlobalToSystem)
            {
                query = m_store.CreateQuery(Schema.SeqNavLearnerGlobalObjectiveView.ViewName);
                statusColumnName = Schema.SeqNavLearnerGlobalObjectiveView.SuccessStatus;
                query.AddColumn(statusColumnName);
                measureColumnName = Schema.SeqNavLearnerGlobalObjectiveView.ScaledScore;
                query.AddColumn(measureColumnName);
                query.AddCondition(Schema.SeqNavLearnerGlobalObjectiveView.Key, LearningStoreConditionOperator.Equal, globalObjective);
                query.AddCondition(Schema.SeqNavLearnerGlobalObjectiveView.LearnerId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
            }
            else
            {
                query = m_store.CreateQuery(Schema.SeqNavOrganizationGlobalObjectiveView.ViewName);
                statusColumnName = Schema.SeqNavOrganizationGlobalObjectiveView.SuccessStatus;
                query.AddColumn(statusColumnName);
                measureColumnName = Schema.SeqNavOrganizationGlobalObjectiveView.ScaledScore;
                query.AddColumn(measureColumnName);
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.Key, LearningStoreConditionOperator.Equal, globalObjective);
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.LearnerId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.UserItem.ItemTypeName, m_learnerId));
                query.AddCondition(Schema.SeqNavOrganizationGlobalObjectiveView.OrganizationId, LearningStoreConditionOperator.Equal, new LearningStoreItemIdentifier(Schema.ActivityPackageItem.ItemTypeName, RootActivity.ActivityId));
            }
            job.PerformQuery(query);

            DataTable d = job.Execute<DataTable>();
            if(d.Rows.Count > 1)
            {
                // should never happen - throw
                throw new InvalidOperationException(Resources.GlobalObjectiveDatabaseStateNotValid);
            }

            // we always create a record in the dictionary, just so we won't have to go through this again.
            GlobalObjective glob = new GlobalObjective(globalObjective);
            if(d.Rows.Count > 0)
            {
                if(!(d.Rows[0][statusColumnName] is DBNull))
                {
                    switch((SuccessStatus)d.Rows[0][statusColumnName])
                    {
                        case SuccessStatus.Failed:
                            glob.SatisfiedStatus = false;
                            break;
                        case SuccessStatus.Passed:
                            glob.SatisfiedStatus = true;
                            break;
                        default:
                        case SuccessStatus.Unknown:
                            glob.SatisfiedStatus = null;
                            break;
                    }
                }
                if(!(d.Rows[0][measureColumnName] is DBNull))
                {
                    glob.NormalizedMeasure = (float)d.Rows[0][measureColumnName];
                }
            }
            m_globalObjectives.Add(globalObjective, glob);
        }

        /// <summary>
        /// Reads the Satisfied Status of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="satisfied">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal override bool ReadGlobalObjectiveSatisfiedStatus(string globalObjective, ref bool satisfied)
        {
            Utilities.Assert(RootActivity != null);

            if(String.IsNullOrEmpty(globalObjective))
            {
                return false;
            }
            if(!m_globalObjectives.ContainsKey(globalObjective))
            {
                ReadGlobalObjectiveFromLearningStore(globalObjective);  // this always adds to the dictionary
            }
            if(m_globalObjectives[globalObjective].SatisfiedStatus.HasValue)
            {
                satisfied = m_globalObjectives[globalObjective].SatisfiedStatus.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the Normalized Measure of the global objective specified, if any.
        /// </summary>
        /// <param name="globalObjective">Name of the global objective, if any.</param>
        /// <param name="measure">Where the result is stored, if it is read.</param>
        /// <returns>True if data was read, false otherwise.</returns>
        internal override bool ReadGlobalObjectiveNormalizedMeasure(string globalObjective, ref float measure)
        {
            Utilities.Assert(RootActivity != null);

            if(String.IsNullOrEmpty(globalObjective))
            {
                return false;
            }
            if(!m_globalObjectives.ContainsKey(globalObjective))
            {
                ReadGlobalObjectiveFromLearningStore(globalObjective);  // this always adds to the dictionary
            }
            if(m_globalObjectives[globalObjective].NormalizedMeasure.HasValue)
            {
                measure = m_globalObjectives[globalObjective].NormalizedMeasure.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes the global objective information for all objectives associated with this activity, if any.
        /// </summary>
        /// <param name="activity">The activity whose objectives to save.</param>
        internal override void WriteGlobalObjectives(Activity activity)
        {
            Utilities.Assert(RootActivity != null);

            foreach(Objective obj in activity.DataModel.Objectives)
            {
                foreach(string objId in obj.GlobalObjectiveWriteSatisfiedStatus)
                {
                    if(!m_globalObjectives.ContainsKey(objId))
                    {
                        ReadGlobalObjectiveFromLearningStore(objId);  // this always adds a dictionary entry
                    }
                    if(obj.ObjectiveProgressStatus)
                    {
                        m_globalObjectives[objId].SatisfiedStatus = obj.ObjectiveSatisfiedStatus;
                    }
                    else
                    {
                        m_globalObjectives[objId].SatisfiedStatus = null;
                    }
                    m_changed = true;
                }
                foreach(string objId in obj.GlobalObjectiveWriteNormalizedMeasure)
                {
                    if(!m_globalObjectives.ContainsKey(objId))
                    {
                        ReadGlobalObjectiveFromLearningStore(objId);  // this always adds a dictionary entry
                    }
                    if(obj.ObjectiveMeasureStatus)
                    {
                        m_globalObjectives[objId].NormalizedMeasure = obj.ObjectiveNormalizedMeasure;
                    }
                    else
                    {
                        m_globalObjectives[objId].NormalizedMeasure = null;
                    }
                    m_changed = true;
                }
            }
        }

        /// <summary>
        /// Adds operations to the job that demands the right for a session, and then
        /// disables security checks for the rest of the job
        /// </summary>
        /// <param name="job">The job to which the operations should be added.</param>
        public void DemandSessionRight(LearningStoreJob job)
        {
            Dictionary<string,object> parameters = new Dictionary<string,object>();
            string rightName = null;
            
            if (this is ExecuteNavigator)
            {
                rightName = Schema.ExecuteSessionRight.RightName;
                parameters[Schema.ExecuteSessionRight.AttemptId] =
                    new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId);
            }
            else if (this is ReviewNavigator)
            {
                rightName = Schema.ReviewSessionRight.RightName;
                parameters[Schema.ReviewSessionRight.AttemptId] =
                    new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId);
            }
            else if (this is RandomAccessNavigator)
            {
                rightName = Schema.RandomAccessSessionRight.RightName;
                parameters[Schema.RandomAccessSessionRight.AttemptId] =
                    new LearningStoreItemIdentifier(Schema.AttemptItem.ItemTypeName, m_attemptId);
            }
            else
            {
                throw new LearningComponentsInternalException("DBN4000");
            }
            
            job.DemandRight(rightName, parameters);
            job.DisableFollowingSecurityChecks();
        }
        
        #region Logging stuff

        /// <summary>
        /// Represents a single entry in the log of sequencing events.
        /// </summary>
        private class LogEvent
        {
            /// <summary>
            /// The type of event that caused this log entry.
            /// </summary>
            private SequencingEventType m_eventType;
            public SequencingEventType EventType
            {
                get
                {
                    return m_eventType;
                }
            }

            /// <summary>
            /// The text message of the log entry.
            /// </summary>
            private string m_message;
            public string Message
            {
                get
                {
                    return m_message;
                }
            }

            /// <summary>
            /// The navigation command that caused this event.
            /// </summary>
            private NavigationCommand m_command;
            public NavigationCommand Command
            {
                get
                {
                    return m_command;
                }
            }

            /// <summary>
            /// Time at which this log entry was created.
            /// </summary>
            private DateTime m_timeStamp;
            public DateTime TimeStamp
            {
                get
                {
                    return m_timeStamp;
                }
            }

            /// <summary>
            /// The id of the activity that is active after this log entry was created.
            /// </summary>
            private LearningStoreItemIdentifier m_activityId;
            public LearningStoreItemIdentifier ActivityId
            {
                get
                {
                    return m_activityId;
                }
            }

            /// <summary>
            /// Initializes a LogEvent.
            /// </summary>
            /// <param name="eventType"></param>
            /// <param name="command"></param>
            /// <param name="activityId"></param>
            /// <param name="message"></param>
            public LogEvent(SequencingEventType eventType, NavigationCommand command, LearningStoreItemIdentifier activityId, string message)
            {
                m_eventType = eventType;
                m_command = command;
                m_message = message;
                m_activityId = activityId;
                m_timeStamp = DateTime.Now;
            }
        }

        private List<LogEvent> m_logEventList = new List<LogEvent>();

        private List<LogEvent> m_logEventListBackup = new List<LogEvent>();

        public override void LogSequencing(SequencingEventType eventType, NavigationCommand command, string message, params object[] args)
        {
            if((eventType == SequencingEventType.FinalNavigation && (m_loggingFlags & LoggingOptions.LogFinalSequencing) != 0) ||
                (eventType == SequencingEventType.IntermediateNavigation && (m_loggingFlags & LoggingOptions.LogDetailSequencing) != 0) ||
                (eventType == SequencingEventType.Rollup && (m_loggingFlags & LoggingOptions.LogRollup) != 0))
            {
                if(m_currentActivity == null)
                {
                    m_logEventList.Add(new LogEvent(eventType, command, null, String.Format(CultureInfo.CurrentCulture, message, args)));
                }
                else
                {
                    m_logEventList.Add(new LogEvent(eventType, command,
                        new LearningStoreItemIdentifier(Schema.ActivityAttemptItem.ItemTypeName, m_currentActivity.InternalActivityId),
                        String.Format(CultureInfo.CurrentCulture, message, args)));
                }
            }
        }

        #endregion
    }
}
