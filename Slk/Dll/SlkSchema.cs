namespace Microsoft.SharePointLearningKit.Schema {
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    
    /// <summary>
    /// Contains constants related to the ActivityAttemptItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#Packages">activity</a> in the context of one <a href="SlkConcepts.htm#Assignments">attempt</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityPackageId</Fld></li>
    /// <li><Fld>AttemptCount</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>EvaluationPoints</Fld></li>
    /// <li><Fld>Exit</Fld></li>
    /// <li><Fld>LessonStatus</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>ProgressMeasure</Fld></li>
    /// <li><Fld>RandomPlacement</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SequencingDataCache</Fld></li>
    /// <li><Fld>SessionStartTimestamp</Fld></li>
    /// <li><Fld>SessionTime</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// <li><Fld>SuspendData</Fld></li>
    /// <li><Fld>TotalTime</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ActivityAttemptItem {
        
        /// <summary>
        /// Name of the <Typ>ActivityAttemptItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.Id;
        
        /// <summary>
        /// Name of the AttemptId property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>AttemptId is the identifier for the attempt on the package.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.AttemptId;
        
        /// <summary>
        /// Name of the ActivityPackageId property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>ActivityPackageId is the identifier for the activity information that is contained in the package.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityPackageId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.ActivityPackageId;
        
        /// <summary>
        /// Name of the CompletionStatus property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>CompletionStatus indicates whether or not the activity has been completed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.CompletionStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.CompletionStatus;
        
        /// <summary>
        /// Name of the AttemptCount property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>AttemptCount is a count of the number of sessions this attempt has taken. Every time the attempt is suspended and resumed, this is updated.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCount = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.AttemptCount;
        
        /// <summary>
        /// Name of the DataModelCache property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>DataModelCache is an XML representation of the dynamic data not required for sequencing relating to this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.DataModelCache;
        
        /// <summary>
        /// Name of the EvaluationPoints property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>EvaluationPoints is the point value assigned by an instructor as an evaluation of learner responses.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EvaluationPoints = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.EvaluationPoints;
        
        /// <summary>
        /// Name of the Exit property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>Exit indicates how or why a user left the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.ExitMode</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Exit = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.Exit;
        
        /// <summary>
        /// Name of the LessonStatus property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>LessonStatus indicates the status of the attempt.  This value is only used in SCORM 1.2.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.LessonStatus</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LessonStatus = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.LessonStatus;
        
        /// <summary>
        /// Name of the Location property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>Location is the information used by the SCO to determine the learner's position within the SCO, similar in concept to a bookmark.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1000]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.Location;
        
        /// <summary>
        /// Maximum length of the <Fld>Location</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLocationLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.MaxLocationLength;
        
        /// <summary>
        /// Name of the MinScore property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>MinScore is the minimum score allowed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.MinScore;
        
        /// <summary>
        /// Name of the MaxScore property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>MaxScore is the maximum score allowed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.MaxScore;
        
        /// <summary>
        /// Name of the ProgressMeasure property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>ProgressMeasure is the progress toward completing the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ProgressMeasure = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.ProgressMeasure;
        
        /// <summary>
        /// Name of the RandomPlacement property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>RandomPlacement is the order assigned to this activity relative to its siblings. This information is most useful when the cluster is to be randomized.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RandomPlacement = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.RandomPlacement;
        
        /// <summary>
        /// Name of the RawScore property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>RawScore is the score that reflects the performance of the learner, between MinScore and MaxScore.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.RawScore;
        
        /// <summary>
        /// Name of the ScaledScore property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>ScaledScore is the score that reflects the performance of the learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.ScaledScore;
        
        /// <summary>
        /// Name of the SequencingDataCache property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>SequencingDataCache is an XML representation of the dynamic data required for sequencing relating to this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SequencingDataCache = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.SequencingDataCache;
        
        /// <summary>
        /// Name of the SessionStartTimestamp property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>SessionStartTimestamp is the time (UTC) that the current session started.</para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SessionStartTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.SessionStartTimestamp;
        
        /// <summary>
        /// Name of the SessionTime property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>SessionTime is the duration of the session as determined by SCO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Double<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SessionTime = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.SessionTime;
        
        /// <summary>
        /// Name of the SuccessStatus property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>SuccessStatus indicates whether the learner has mastered the SCO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.SuccessStatus;
        
        /// <summary>
        /// Name of the SuspendData property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>SuspendData is the data saved by SCO when the attempt on the activity is suspended.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuspendData = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.SuspendData;
        
        /// <summary>
        /// Maximum length of the <Fld>SuspendData</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxSuspendDataLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.MaxSuspendDataLength;
        
        /// <summary>
        /// Name of the TotalTime property on the <Typ>ActivityAttemptItem</Typ> item type.
        /// <para>TotalTime is the time (in seconds) spent on previous sessions in this attempt on an activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Double<p/>
        /// Property can contain null.<p/>
        /// Default value: 0<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TotalTime = Microsoft.LearningComponents.Storage.BaseSchema.ActivityAttemptItem.TotalTime;
    }
    
    /// <summary>
    /// Contains constants related to the ActivityObjectiveItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#ScormConcepts">local objective</a> of
    /// one <a href="SlkConcepts.htm#Packages">activity</a> within one e-learning package.
    /// Objectives that are added to a package by a
    /// <a href="SlkConcepts.htm#Packages">SCO</a> via the
    /// <a href="SlkConcepts.htm#ScormConcepts">RTE</a> are not included in this table.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityPackageId</Fld></li>
    /// <li><Fld>IsPrimaryObjective</Fld></li>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>MinNormalizedMeasure</Fld></li>
    /// <li><Fld>SatisfiedByMeasure</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ActivityObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>ActivityObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>ActivityObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.Id;
        
        /// <summary>
        /// Name of the ActivityPackageId property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// <para>ActivityPackageId is the identifier for the activity related to this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityPackageId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.ActivityPackageId;
        
        /// <summary>
        /// Name of the IsPrimaryObjective property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// <para>If IsPrimaryObjective is true, this is the primary objective for the related activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsPrimaryObjective = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.IsPrimaryObjective;
        
        /// <summary>
        /// Name of the Key property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// <para>Key is the identifier from the manifest that identifies this objective. Not necessarily unique.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.Key;
        
        /// <summary>
        /// Maximum length of the <Fld>Key</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxKeyLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.MaxKeyLength;
        
        /// <summary>
        /// Name of the MinNormalizedMeasure property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// <para>MinNormalizedMeasure is the minimum measure required to satisfy the measure.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can not contain null.<p/>
        /// Default value: 1<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinNormalizedMeasure = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.MinNormalizedMeasure;
        
        /// <summary>
        /// Name of the SatisfiedByMeasure property on the <Typ>ActivityObjectiveItem</Typ> item type.
        /// <para>SatisfiedByMeasure is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SatisfiedByMeasure = Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.SatisfiedByMeasure;
    }
    
    /// <summary>
    /// Contains constants related to the ActivityPackageItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#Packages">activity</a>
    /// within one e-learning package.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityIdFromManifest</Fld></li>
    /// <li><Fld>CompletionThreshold</Fld></li>
    /// <li><Fld>Credit</Fld></li>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>HideAbandon</Fld></li>
    /// <li><Fld>HideContinue</Fld></li>
    /// <li><Fld>HideExit</Fld></li>
    /// <li><Fld>HidePrevious</Fld></li>
    /// <li><Fld>IsVisibleInContents</Fld></li>
    /// <li><Fld>LaunchData</Fld></li>
    /// <li><Fld>MasteryScore</Fld></li>
    /// <li><Fld>MaxAttempts</Fld></li>
    /// <li><Fld>MaxTimeAllowed</Fld></li>
    /// <li><Fld>ObjectivesGlobalToSystem</Fld></li>
    /// <li><Fld>OriginalPlacement</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>ParentActivityId</Fld></li>
    /// <li><Fld>PrimaryObjectiveId</Fld></li>
    /// <li><Fld>PrimaryResourceFromManifest</Fld></li>
    /// <li><Fld>ResourceId</Fld></li>
    /// <li><Fld>ResourceParameters</Fld></li>
    /// <li><Fld>ScaledPassingScore</Fld></li>
    /// <li><Fld>TimeLimitAction</Fld></li>
    /// <li><Fld>Title</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ActivityPackageItem {
        
        /// <summary>
        /// Name of the <Typ>ActivityPackageItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>ActivityPackageItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.Id;
        
        /// <summary>
        /// Name of the ActivityIdFromManifest property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ActivityIdFromManifest is the identifier for this attempt from the manifest. Guaranteed unique within a package, not unique in the table.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityIdFromManifest = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ActivityIdFromManifest;
        
        /// <summary>
        /// Maximum length of the <Fld>ActivityIdFromManifest</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxActivityIdFromManifestLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxActivityIdFromManifestLength;
        
        /// <summary>
        /// Name of the OriginalPlacement property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>OriginalPlacement is the order assigned to the activity relative to its siblings based on the appearance of the activity in the manifest. The value is zero-based.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string OriginalPlacement = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.OriginalPlacement;
        
        /// <summary>
        /// Name of the ParentActivityId property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ParentActivityId is the identifier of the activity that is a parent of this activity in the activity tree of an organization. If null, this is the root activity being attempted.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ParentActivityId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ParentActivityId;
        
        /// <summary>
        /// Name of the PackageId property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>PackageId is the identifier of the package associated with this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>PackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.PackageId;
        
        /// <summary>
        /// Name of the PrimaryObjectiveId property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>PrimaryObjectiveId is the identifier of the objective which is identified as the primary objective for this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityObjectiveItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PrimaryObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.PrimaryObjectiveId;
        
        /// <summary>
        /// Name of the ResourceId property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ResourceId is the identifier of the resource information for this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ResourceItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResourceId = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ResourceId;
        
        /// <summary>
        /// Name of the PrimaryResourceFromManifest property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>PrimaryResourceFromManifest is the primary resource to launch for an attempt on this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[2000]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PrimaryResourceFromManifest = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.PrimaryResourceFromManifest;
        
        /// <summary>
        /// Maximum length of the <Fld>PrimaryResourceFromManifest</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxPrimaryResourceFromManifestLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxPrimaryResourceFromManifestLength;
        
        /// <summary>
        /// Name of the DataModelCache property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>DataModelCache is an XML representation of the static data relating to this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.DataModelCache;
        
        /// <summary>
        /// Name of the CompletionThreshold property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>CompletionThreshold is the completion threshold (as defined in RTE) for this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionThreshold = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.CompletionThreshold;
        
        /// <summary>
        /// Name of the Credit property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>Credit indicates whether the learner will be credited for completion of the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can contain null.<p/>
        /// Default value: True<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Credit = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.Credit;
        
        /// <summary>
        /// Name of the HideContinue property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>HideContinue is the flag indicating the UI should not display the 'Continue' button.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HideContinue = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.HideContinue;
        
        /// <summary>
        /// Name of the HidePrevious property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>HidePrevious is the flag indicating the UI should not display the 'Previous' button.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HidePrevious = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.HidePrevious;
        
        /// <summary>
        /// Name of the HideExit property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>HideExit is the flag indicating the UI should not display the 'Exit' button.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HideExit = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.HideExit;
        
        /// <summary>
        /// Name of the HideAbandon property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>HideAbandon is the flag indicating the UI should not display the 'Abandon' button.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HideAbandon = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.HideAbandon;
        
        /// <summary>
        /// Name of the IsVisibleInContents property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>If IsVisibleInContents is true, the activity is eligible to be visible in the table of contents.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: True<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsVisibleInContents = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.IsVisibleInContents;
        
        /// <summary>
        /// Name of the LaunchData property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>LaunchData is the data provided to start an activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LaunchData = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.LaunchData;
        
        /// <summary>
        /// Maximum length of the <Fld>LaunchData</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLaunchDataLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxLaunchDataLength;
        
        /// <summary>
        /// Name of the MasteryScore property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>MasteryScore is the 'passing' score in SCORM 1.2 content.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MasteryScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MasteryScore;
        
        /// <summary>
        /// Name of the MaxAttempts property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>MaxAttempts is the maximum number of attempts allowed on this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxAttempts = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxAttempts;
        
        /// <summary>
        /// Name of the MaxTimeAllowed property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>MaxTimeAllowed is the maximum time allowed for the user to complete a single attempt on the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Double<p/>
        /// Property can contain null.<p/>
        /// Default value: 0<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxTimeAllowed = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxTimeAllowed;
        
        /// <summary>
        /// Name of the ResourceParameters property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ResourceParameters is the parameter string used in conjunction with the primary resource for this activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1000]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResourceParameters = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ResourceParameters;
        
        /// <summary>
        /// Maximum length of the <Fld>ResourceParameters</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxResourceParametersLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxResourceParametersLength;
        
        /// <summary>
        /// Name of the ScaledPassingScore property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ScaledPassingScore is the passing score required to 'pass' the SCO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledPassingScore = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ScaledPassingScore;
        
        /// <summary>
        /// Name of the TimeLimitAction property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>TimeLimitAction indicates what a SCO should do when time limit is exceeded.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.TimeLimitAction</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TimeLimitAction = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.TimeLimitAction;
        
        /// <summary>
        /// Name of the Title property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>Title is the title of the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[200]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Title = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.Title;
        
        /// <summary>
        /// Maximum length of the <Fld>Title</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTitleLength = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.MaxTitleLength;
        
        /// <summary>
        /// Name of the ObjectivesGlobalToSystem property on the <Typ>ActivityPackageItem</Typ> item type.
        /// <para>ObjectivesGlobalToSystem is the flag indicating whether the global objectives referenced in this activity tree are global for the learner and the content organization (false) or global for the lifetime of the learner(true).</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: True<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ObjectivesGlobalToSystem = Microsoft.LearningComponents.Storage.BaseSchema.ActivityPackageItem.ObjectivesGlobalToSystem;
    }
    
    /// <summary>
    /// Contains constants related to the PackageGlobalObjectiveItem item type.
    /// <para>Each item contains information about progress on one
    /// <a href="SlkConcepts.htm#ScormConcepts">global objective</a> related to one
    /// <a href="SlkConcepts.htm#Assignments">organization</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>GlobalObjectiveId</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class PackageGlobalObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>PackageGlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.Id;
        
        /// <summary>
        /// Name of the LearnerId property on the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// <para>LearnerId is the identifier of the learner related to this entry.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.LearnerId;
        
        /// <summary>
        /// Name of the GlobalObjectiveId property on the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// <para>GlobalObjectiveId is the identifier for the objective related to this entry.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>GlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string GlobalObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.GlobalObjectiveId;
        
        /// <summary>
        /// Name of the ScaledScore property on the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// <para>ScaledScore is the score that reflects the performance of the learner on this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus property on the <Typ>PackageGlobalObjectiveItem</Typ> item type.
        /// <para>SuccessStatus indicates whether or not the learner has successfully met this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.SuccessStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.PackageGlobalObjectiveItem.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the AttemptItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#Assignments">attempt</a>, i.e. one
    /// instance of one learner executing one
    /// <a href="SlkConcepts.htm#Packages">organization</a> of one e-learning package.
    /// If the same learner attempts the same organization twice, two AttemptItem rows are
    /// created.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.  SLK <a href="SlkSchema.xml.htm">extends</a> this item type by adding the
    /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LearnerAssignmentId.Field.htm">LearnerAssignmentId</a> property/column and a SQL index.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AttemptStatus</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>CurrentActivityId</Fld></li>
    /// <li><Fld>FinishedTimestamp</Fld></li>
    /// <li><Fld>LearnerAssignmentId</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>LogDetailSequencing</Fld></li>
    /// <li><Fld>LogFinalSequencing</Fld></li>
    /// <li><Fld>LogRollup</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// <li><Fld>StartedTimestamp</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// <li><Fld>SuspendedActivityId</Fld></li>
    /// <li><Fld>TotalPoints</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AttemptItem {
        
        /// <summary>
        /// Name of the <Typ>AttemptItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>AttemptItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>AttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.Id;
        
        /// <summary>
        /// Name of the LearnerId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>LearnerId is the identifier of the learner of this attempt.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.LearnerId;
        
        /// <summary>
        /// Name of the RootActivityId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>RootActivityId is the identifier of the root activity of the package.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.RootActivityId;
        
        /// <summary>
        /// Name of the CompletionStatus property on the <Typ>AttemptItem</Typ> item type.
        /// <para>CompletionStatus is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.CompletionStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.CompletionStatus;
        
        /// <summary>
        /// Name of the CurrentActivityId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>CurrentActivityId is the identifier of the current active activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CurrentActivityId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.CurrentActivityId;
        
        /// <summary>
        /// Name of the SuspendedActivityId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>SuspendedActivityId is the identifier of the previously suspended activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuspendedActivityId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.SuspendedActivityId;
        
        /// <summary>
        /// Name of the PackageId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>PackageId is the identifier of the package that is being attempted.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>PackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.PackageId;
        
        /// <summary>
        /// Name of the AttemptStatus property on the <Typ>AttemptItem</Typ> item type.
        /// <para>AttemptStatus is the status of the attempt.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.AttemptStatus.Active</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.AttemptStatus;
        
        /// <summary>
        /// Name of the FinishedTimestamp property on the <Typ>AttemptItem</Typ> item type.
        /// <para>FinishedTimestamp is the date/time (UTC) the attempt was completed or abandoned.</para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinishedTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.FinishedTimestamp;
        
        /// <summary>
        /// Name of the LogDetailSequencing property on the <Typ>AttemptItem</Typ> item type.
        /// <para>If LogDetailSequencing is true, details of navigation are logged, from the initial activity up to but not including the final destination.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogDetailSequencing = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.LogDetailSequencing;
        
        /// <summary>
        /// Name of the LogFinalSequencing property on the <Typ>AttemptItem</Typ> item type.
        /// <para>If LogFinalSequencing is true, final destination arrived at when navigating is logged.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogFinalSequencing = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.LogFinalSequencing;
        
        /// <summary>
        /// Name of the LogRollup property on the <Typ>AttemptItem</Typ> item type.
        /// <para>If LogRollup is true, rollup operations are logged.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogRollup = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.LogRollup;
        
        /// <summary>
        /// Name of the StartedTimestamp property on the <Typ>AttemptItem</Typ> item type.
        /// <para>StartedTimestamp is the date/time (UTC) when the attempt was started.</para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StartedTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.StartedTimestamp;
        
        /// <summary>
        /// Name of the SuccessStatus property on the <Typ>AttemptItem</Typ> item type.
        /// <para>SuccessStatus is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.SuccessStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.SuccessStatus;
        
        /// <summary>
        /// Name of the TotalPoints property on the <Typ>AttemptItem</Typ> item type.
        /// <para>TotalPoints is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TotalPoints = Microsoft.LearningComponents.Storage.BaseSchema.AttemptItem.TotalPoints;
        
        /// <summary>
        /// Name of the LearnerAssignmentId property on the <Typ>AttemptItem</Typ> item type.
        /// <para>
        /// LearnerAssignmentId is the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentItemIdentifier.Class.htm">LearnerAssignmentItemIdentifier</a>
        /// of the
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>
        /// associated with this attempt.
        /// </para>
        /// <para>
        /// This property/column is available only in <a href="Default.htm">SLK</a> (not in
        /// <a href="Mlc.htm">MLC</a>).
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentId = "LearnerAssignmentId";
    }
    
    /// <summary>
    /// Contains constants related to the AttemptObjectiveItem item type.
    /// <para>Each item contains information about the learner's progress on one
    /// <a href="SlkConcepts.htm#ScormConcepts">local objective</a> for one
    /// <a href="SlkConcepts.htm#Packages">activity</a> of one
    /// <a href="SlkConcepts.htm#Assignments">attempt</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>ActivityObjectiveId</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>IsPrimaryObjective</Fld></li>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>LessonStatus</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>ProgressMeasure</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AttemptObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>AttemptObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.Id;
        
        /// <summary>
        /// Name of the ActivityAttemptId property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>ActivityAttemptId is the identifier of the activity related to this attempt.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.ActivityAttemptId;
        
        /// <summary>
        /// Name of the ActivityObjectiveId property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>ActivityObjectiveId is the identifier for the objective related to this entry.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityObjectiveItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.ActivityObjectiveId;
        
        /// <summary>
        /// Name of the CompletionStatus property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>CompletionStatus indicates whether or not the learner has completed work on this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.CompletionStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.CompletionStatus;
        
        /// <summary>
        /// Name of the Description property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>Description is the description of the objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.Description;
        
        /// <summary>
        /// Maximum length of the <Fld>Description</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxDescriptionLength = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.MaxDescriptionLength;
        
        /// <summary>
        /// Name of the IsPrimaryObjective property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>If IsPrimaryObjective is true, then this is the primary objective for the related activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsPrimaryObjective = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.IsPrimaryObjective;
        
        /// <summary>
        /// Name of the Key property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>Key is the identifier for the activity. The identifier is not necessarily unique.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.Key;
        
        /// <summary>
        /// Maximum length of the <Fld>Key</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxKeyLength = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.MaxKeyLength;
        
        /// <summary>
        /// Name of the LessonStatus property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>LessonStatus is the SCORM 1.2 compliance field to include status of the current user with respect to this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.LessonStatus</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.LessonStatus.NotAttempted</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LessonStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.LessonStatus;
        
        /// <summary>
        /// Name of the RawScore property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>RawScore is the score that reflects the performance of the learner on this objective, between MinScore and MaxScore.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.RawScore;
        
        /// <summary>
        /// Name of the MinScore property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>MinScore is the minimum score allowed on this objective for the learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.MinScore;
        
        /// <summary>
        /// Name of the MaxScore property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>MaxScore is the maximum score allowed on this objective for this learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.MaxScore;
        
        /// <summary>
        /// Name of the ProgressMeasure property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>ProgressMeasure is the progress of learner in completing this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ProgressMeasure = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.ProgressMeasure;
        
        /// <summary>
        /// Name of the ScaledScore property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>ScaledScore is the score that reflects the performance of the learner on this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus property on the <Typ>AttemptObjectiveItem</Typ> item type.
        /// <para>SuccessStatus indicates whether or not the learner has successfully met this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.SuccessStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the CommentFromLearnerItem item type.
    /// <para>Each item contains one comment from the learner related to one
    /// <a href="SlkConcepts.htm#Packages">activity</a> of one
    /// <a href="SlkConcepts.htm#Assignments">attempt</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class CommentFromLearnerItem {
        
        /// <summary>
        /// Name of the <Typ>CommentFromLearnerItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>CommentFromLearnerItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.Id;
        
        /// <summary>
        /// Name of the ActivityAttemptId property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// <para>ActivityAttemptId is the identifier for the attempt on the activity related to this comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.ActivityAttemptId;
        
        /// <summary>
        /// Name of the Comment property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// <para>Comment is the text of the comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.Comment;
        
        /// <summary>
        /// Maximum length of the <Fld>Comment</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxCommentLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.MaxCommentLength;
        
        /// <summary>
        /// Name of the Location property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// <para>Location is the text indicating where the comment applies.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.Location;
        
        /// <summary>
        /// Maximum length of the <Fld>Location</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLocationLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.MaxLocationLength;
        
        /// <summary>
        /// Name of the Ordinal property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// <para>Ordinal is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.Ordinal;
        
        /// <summary>
        /// Name of the Timestamp property on the <Typ>CommentFromLearnerItem</Typ> item type.
        /// <para>Timestamp is the timestamp (UTC) indicating when the comment was created or most recently changed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[28]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.Timestamp;
        
        /// <summary>
        /// Maximum length of the <Fld>Timestamp</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTimestampLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLearnerItem.MaxTimestampLength;
    }
    
    /// <summary>
    /// Contains constants related to the CommentFromLmsItem item type.
    /// <para>Each item contains a <a href="SlkConcepts.htm#ScormConcepts">"comments from LMS"</a> string related to a
    /// specific <a href="SlkConcepts.htm#Packages">activity</a> within an e-learning package.
    /// These are read-only by <a href="SlkConcepts.htm#Packages">SCOs</a>.  SLK does not add
    /// rows to the CommentsFromLmsItem table.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityPackageId</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class CommentFromLmsItem {
        
        /// <summary>
        /// Name of the <Typ>CommentFromLmsItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>CommentFromLmsItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>CommentFromLmsItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.Id;
        
        /// <summary>
        /// Name of the ActivityPackageId property on the <Typ>CommentFromLmsItem</Typ> item type.
        /// <para>ActivityPackageId is the identifier of the activity in a specific package associated with this comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityPackageId = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.ActivityPackageId;
        
        /// <summary>
        /// Name of the Comment property on the <Typ>CommentFromLmsItem</Typ> item type.
        /// <para>Comment is the text of the comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.Comment;
        
        /// <summary>
        /// Maximum length of the <Fld>Comment</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxCommentLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.MaxCommentLength;
        
        /// <summary>
        /// Name of the Location property on the <Typ>CommentFromLmsItem</Typ> item type.
        /// <para>Location is the text indicating where the comment applies.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.Location;
        
        /// <summary>
        /// Maximum length of the <Fld>Location</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLocationLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.MaxLocationLength;
        
        /// <summary>
        /// Name of the Timestamp property on the <Typ>CommentFromLmsItem</Typ> item type.
        /// <para>Timestamp is the timestamp (UTC) indicating when comment was created or most recently changed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[28]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.Timestamp;
        
        /// <summary>
        /// Maximum length of the <Fld>Timestamp</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTimestampLength = Microsoft.LearningComponents.Storage.BaseSchema.CommentFromLmsItem.MaxTimestampLength;
    }
    
    /// <summary>
    /// Contains constants related to the CorrectResponseItem item type.
    /// <para>Each item contains information about one SCORM <a href="SlkConcepts.htm#ScormConcepts">correct response string</a>
    /// for one <a href="Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Class.htm">InteractionItem</a>.
    /// The CorrectResponseItem table contains one row in this table per correct response per
    /// interaction.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>ResponsePattern</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class CorrectResponseItem {
        
        /// <summary>
        /// Name of the <Typ>CorrectResponseItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.CorrectResponseItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>CorrectResponseItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>CorrectResponseItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.CorrectResponseItem.Id;
        
        /// <summary>
        /// Name of the InteractionId property on the <Typ>CorrectResponseItem</Typ> item type.
        /// <para>InteractionId is the identifier of the interaction for which this is a correct response.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.CorrectResponseItem.InteractionId;
        
        /// <summary>
        /// Name of the ResponsePattern property on the <Typ>CorrectResponseItem</Typ> item type.
        /// <para>ResponsePattern is the pattern of the correct response.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1073741822]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResponsePattern = Microsoft.LearningComponents.Storage.BaseSchema.CorrectResponseItem.ResponsePattern;
        
        /// <summary>
        /// Maximum length of the <Fld>ResponsePattern</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxResponsePatternLength = Microsoft.LearningComponents.Storage.BaseSchema.CorrectResponseItem.MaxResponsePatternLength;
    }
    
    /// <summary>
    /// Contains constants related to the EvaluationCommentItem item type.
    /// <para>Each item contains a comment from an evaluator (i.e. an instructor grading a
    /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>) related to one
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Class.htm">InteractionItem</a>.  Only used for e-learning packages in Class Server format.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class EvaluationCommentItem {
        
        /// <summary>
        /// Name of the <Typ>EvaluationCommentItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>EvaluationCommentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.Id;
        
        /// <summary>
        /// Name of the InteractionId property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// <para>InteractionId is the identifier of the interaction for which this is a comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.InteractionId;
        
        /// <summary>
        /// Name of the Comment property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// <para>Comment is the text of the comment.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.Comment;
        
        /// <summary>
        /// Maximum length of the <Fld>Comment</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxCommentLength = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.MaxCommentLength;
        
        /// <summary>
        /// Name of the Location property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// <para>Location is the text indicating where the comment applies.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.Location;
        
        /// <summary>
        /// Maximum length of the <Fld>Location</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLocationLength = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.MaxLocationLength;
        
        /// <summary>
        /// Name of the Ordinal property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// <para>Ordinal is the identifier for the comment that is unique within the interaction.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.Ordinal;
        
        /// <summary>
        /// Name of the Timestamp property on the <Typ>EvaluationCommentItem</Typ> item type.
        /// <para>Timestamp is the timestamp (UTC) indicating when comment was created or most recently changed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[28]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.Timestamp;
        
        /// <summary>
        /// Maximum length of the <Fld>Timestamp</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTimestampLength = Microsoft.LearningComponents.Storage.BaseSchema.EvaluationCommentItem.MaxTimestampLength;
    }
    
    /// <summary>
    /// Contains constants related to the ExtensionDataItem item type.
    /// <para>Each item contains one "extension" <a href="MlcDataModel.htm">data model element</a>.  An "extension" data
    /// model element is a data model element whose name does not begin with "cmi." or "adl.".</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttachmentGuid</Fld></li>
    /// <li><Fld>AttachmentValue</Fld></li>
    /// <li><Fld>AttemptObjectiveId</Fld></li>
    /// <li><Fld>BoolValue</Fld></li>
    /// <li><Fld>DateTimeValue</Fld></li>
    /// <li><Fld>DoubleValue</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>IntValue</Fld></li>
    /// <li><Fld>Name</Fld></li>
    /// <li><Fld>StringValue</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ExtensionDataItem {
        
        /// <summary>
        /// Name of the <Typ>ExtensionDataItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>ExtensionDataItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>ExtensionDataItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.Id;
        
        /// <summary>
        /// Name of the ActivityAttemptId property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>ActivityAttemptId is the identifier of the attempt activity associated with this element.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.ActivityAttemptId;
        
        /// <summary>
        /// Name of the InteractionId property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>InteractionId is the TODO.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.InteractionId;
        
        /// <summary>
        /// Name of the AttemptObjectiveId property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>If this data is related to an objective, AttemptObjectiveId is the related identifier.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.AttemptObjectiveId;
        
        /// <summary>
        /// Name of the Name property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>Name is the key of the element.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[256]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Name = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.Name;
        
        /// <summary>
        /// Maximum length of the <Fld>Name</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxNameLength = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.MaxNameLength;
        
        /// <summary>
        /// Name of the AttachmentGuid property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>AttachmentGuid is the value of an element represented by a GUID (globally unique identifier).</para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentGuid = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.AttachmentGuid;
        
        /// <summary>
        /// Name of the AttachmentValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>AttachmentValue is the value of the element represented as an array of bytes.</para>
        /// </summary>
        /// <remarks>
        /// Property type: ByteArray[2147483645]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.AttachmentValue;
        
        /// <summary>
        /// Maximum length of the <Fld>AttachmentValue</Fld> property in bytes.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxAttachmentValueLength = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.MaxAttachmentValueLength;
        
        /// <summary>
        /// Name of the BoolValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>BoolValue is the value of the element represented as a Boolean.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string BoolValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.BoolValue;
        
        /// <summary>
        /// Name of the DateTimeValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>DateTimeValue is the value of the element represented as a DateTime. The value should be UTC.</para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DateTimeValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.DateTimeValue;
        
        /// <summary>
        /// Name of the DoubleValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>DoubleValue is the value of the element represented as a Double.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Double<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DoubleValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.DoubleValue;
        
        /// <summary>
        /// Name of the IntValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>IntValue is the value of the element stored as an Int32.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IntValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.IntValue;
        
        /// <summary>
        /// Name of the StringValue property on the <Typ>ExtensionDataItem</Typ> item type.
        /// <para>StringValue is the value of the element stored as a string.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StringValue = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.StringValue;
        
        /// <summary>
        /// Maximum length of the <Fld>StringValue</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxStringValueLength = Microsoft.LearningComponents.Storage.BaseSchema.ExtensionDataItem.MaxStringValueLength;
    }
    
    /// <summary>
    /// Contains constants related to the GlobalObjectiveItem item type.
    /// <para>Each item contains information about one
    /// <a href="SlkConcepts.htm#ScormConcepts">global objective</a> related to one
    /// <a href="SlkConcepts.htm#Packages">organization</a> of one e-learning package, or one
    /// global objective that's not constrained to any one organization of any package.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>OrganizationId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class GlobalObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>GlobalObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>GlobalObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>GlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.Id;
        
        /// <summary>
        /// Name of the OrganizationId property on the <Typ>GlobalObjectiveItem</Typ> item type.
        /// <para>OrganizationId is the organization which contains a reference to this objective. If null, the objective applies to entire system.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string OrganizationId = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.OrganizationId;
        
        /// <summary>
        /// Name of the Key property on the <Typ>GlobalObjectiveItem</Typ> item type.
        /// <para>Key is an identifier for the global objective. If PackageId is null, this is unique within the table, otherwise, it is unique within the package.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.Key;
        
        /// <summary>
        /// Maximum length of the <Fld>Key</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxKeyLength = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.MaxKeyLength;
        
        /// <summary>
        /// Name of the Description property on the <Typ>GlobalObjectiveItem</Typ> item type.
        /// <para>Description is the text description of the objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.Description;
        
        /// <summary>
        /// Maximum length of the <Fld>Description</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxDescriptionLength = Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.MaxDescriptionLength;
    }
    
    /// <summary>
    /// Contains constants related to the InteractionItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#ScormConcepts">interaction</a> within
    /// one <a href="SlkConcepts.htm#Packages">activity</a> of one
    /// <a href="SlkConcepts.htm#Assignments">attempt</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>EvaluationPoints</Fld></li>
    /// <li><Fld>InteractionIdFromCmi</Fld></li>
    /// <li><Fld>InteractionType</Fld></li>
    /// <li><Fld>Latency</Fld></li>
    /// <li><Fld>LearnerResponseBool</Fld></li>
    /// <li><Fld>LearnerResponseNumeric</Fld></li>
    /// <li><Fld>LearnerResponseString</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ResultNumeric</Fld></li>
    /// <li><Fld>ResultState</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// <li><Fld>Weighting</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class InteractionItem {
        
        /// <summary>
        /// Name of the <Typ>InteractionItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>InteractionItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Id;
        
        /// <summary>
        /// Name of the ActivityAttemptId property on the <Typ>InteractionItem</Typ> item type.
        /// <para>ActivityAttemptId is the identifier for the activity that contains this interaction.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.ActivityAttemptId;
        
        /// <summary>
        /// Name of the InteractionIdFromCmi property on the <Typ>InteractionItem</Typ> item type.
        /// <para>InteractionIdFromCmi is the identifier that is provided by the SCO, through the cmi.interactions.n.id value.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[4096]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionIdFromCmi = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.InteractionIdFromCmi;
        
        /// <summary>
        /// Maximum length of the <Fld>InteractionIdFromCmi</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxInteractionIdFromCmiLength = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MaxInteractionIdFromCmiLength;
        
        /// <summary>
        /// Name of the InteractionType property on the <Typ>InteractionItem</Typ> item type.
        /// <para>InteractionType is the type of interaction (for example, true-false).</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.InteractionType</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionType = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.InteractionType;
        
        /// <summary>
        /// Name of the Timestamp property on the <Typ>InteractionItem</Typ> item type.
        /// <para>Timestamp is the point in time (UTC) when the interaction was first made available to the learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[28]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Timestamp;
        
        /// <summary>
        /// Maximum length of the <Fld>Timestamp</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTimestampLength = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MaxTimestampLength;
        
        /// <summary>
        /// Name of the Weighting property on the <Typ>InteractionItem</Typ> item type.
        /// <para>Weighting is the weight given to the interaction. This is commonly used for calculating the score on the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Weighting = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Weighting;
        
        /// <summary>
        /// Name of the ResultState property on the <Typ>InteractionItem</Typ> item type.
        /// <para>ResultState is an analysis of the result of the interaction; for example, "correct".</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.InteractionResultState</Typ><p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResultState = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.ResultState;
        
        /// <summary>
        /// Name of the ResultNumeric property on the <Typ>InteractionItem</Typ> item type.
        /// <para>ResultNumeric is the numeric value of the interaction result.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResultNumeric = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.ResultNumeric;
        
        /// <summary>
        /// Name of the Latency property on the <Typ>InteractionItem</Typ> item type.
        /// <para>Latency is the duration of time between the first interaction and the first response.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Double<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Latency = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Latency;
        
        /// <summary>
        /// Name of the Description property on the <Typ>InteractionItem</Typ> item type.
        /// <para>Description is a brief description of the interaction.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Description;
        
        /// <summary>
        /// Maximum length of the <Fld>Description</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxDescriptionLength = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MaxDescriptionLength;
        
        /// <summary>
        /// Name of the LearnerResponseBool property on the <Typ>InteractionItem</Typ> item type.
        /// <para>LearnerResponseBool is the response from the user to a true-false question.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseBool = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.LearnerResponseBool;
        
        /// <summary>
        /// Name of the LearnerResponseString property on the <Typ>InteractionItem</Typ> item type.
        /// <para>LearnerResponseString is the response from the user to a sequencing, multiple choice or likert question.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1073741822]<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseString = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.LearnerResponseString;
        
        /// <summary>
        /// Maximum length of the <Fld>LearnerResponseString</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLearnerResponseStringLength = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MaxLearnerResponseStringLength;
        
        /// <summary>
        /// Name of the LearnerResponseNumeric property on the <Typ>InteractionItem</Typ> item type.
        /// <para>LearnerResponseNumeric is the response from the user to a numeric question.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseNumeric = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.LearnerResponseNumeric;
        
        /// <summary>
        /// Name of the ScaledScore property on the <Typ>InteractionItem</Typ> item type.
        /// <para>ScaledScore is the score that reflects the performance of the learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.ScaledScore;
        
        /// <summary>
        /// Name of the RawScore property on the <Typ>InteractionItem</Typ> item type.
        /// <para>RawScore is the score that reflects the performance of the learner, between MinScore and MinScore.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.RawScore;
        
        /// <summary>
        /// Name of the MinScore property on the <Typ>InteractionItem</Typ> item type.
        /// <para>MinScore is the minimum score allowed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MinScore;
        
        /// <summary>
        /// Name of the MaxScore property on the <Typ>InteractionItem</Typ> item type.
        /// <para>MaxScore is the maximum score allowed.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.MaxScore;
        
        /// <summary>
        /// Name of the EvaluationPoints property on the <Typ>InteractionItem</Typ> item type.
        /// <para>EvaluationPoints is the point value assigned to the interaction within the context of the Evaluation.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EvaluationPoints = Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.EvaluationPoints;
    }
    
    /// <summary>
    /// Contains constants related to the InteractionObjectiveItem item type.
    /// <para>Each item relates one
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.InteractionItem.Class.htm">InteractionItem</a> with one
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.AttemptObjectiveItem.Class.htm">AttemptObjectiveItem</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AttemptObjectiveId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class InteractionObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>InteractionObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.InteractionObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>InteractionObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>InteractionObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.InteractionObjectiveItem.Id;
        
        /// <summary>
        /// Name of the InteractionId property on the <Typ>InteractionObjectiveItem</Typ> item type.
        /// <para>InteractionId is the identifier of the interaction for this mapping.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.InteractionObjectiveItem.InteractionId;
        
        /// <summary>
        /// Name of the AttemptObjectiveId property on the <Typ>InteractionObjectiveItem</Typ> item type.
        /// <para>AttemptObjectiveId is the identifier of the AttemptObjectiveItem related to this objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.InteractionObjectiveItem.AttemptObjectiveId;
    }
    
    /// <summary>
    /// Contains constants related to the LearnerGlobalObjectiveItem item type.
    /// <para>Each item contains information about the progress of one learner against a particular
    /// global objective, across all <a href="SlkConcepts.htm#Packages">activities</a> in all
    /// e-learning packages.  These objectives may be global to a single package or global to
    /// the system -- the data in this table does not distinguish between these two
    /// cases.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>GlobalObjectiveId</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerGlobalObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>LearnerGlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.Id;
        
        /// <summary>
        /// Name of the LearnerId property on the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// <para>LearnerId is the identifier of the learner.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.LearnerId;
        
        /// <summary>
        /// Name of the GlobalObjectiveId property on the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// <para>GlobalObjectiveId is the identifier of the objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>GlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string GlobalObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.GlobalObjectiveId;
        
        /// <summary>
        /// Name of the ScaledScore property on the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// <para>ScaledScore is the measure of result (i.e, a score) for the objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus property on the <Typ>LearnerGlobalObjectiveItem</Typ> item type.
        /// <para>SuccessStatus indicates whether the objective has been satisfied.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.SuccessStatus.Unknown</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.LearnerGlobalObjectiveItem.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the MapActivityObjectiveToGlobalObjectiveItem item type.
    /// <para>Each item contains a mapping between an
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.ActivityObjectiveItem.Class.htm">ActivityObjectiveItem</a> and a
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.GlobalObjectiveItem.Class.htm">GlobalObjectiveItem</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityObjectiveId</Fld></li>
    /// <li><Fld>GlobalObjectiveId</Fld></li>
    /// <li><Fld>ReadNormalizedMeasure</Fld></li>
    /// <li><Fld>ReadSatisfiedStatus</Fld></li>
    /// <li><Fld>WriteNormalizedMeasure</Fld></li>
    /// <li><Fld>WriteSatisfiedStatus</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class MapActivityObjectiveToGlobalObjectiveItem {
        
        /// <summary>
        /// Name of the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.Id;
        
        /// <summary>
        /// Name of the ActivityObjectiveId property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>ActivityObjectiveId is the identifier for the activity objective being mapped to a global objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.ActivityObjectiveId;
        
        /// <summary>
        /// Name of the GlobalObjectiveId property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>GlobalObjectiveId is the identifier for the global objective being mapped to a local (activity) objective.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>GlobalObjectiveItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string GlobalObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.GlobalObjectiveId;
        
        /// <summary>
        /// Name of the ReadSatisfiedStatus property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>ReadSatisfiedStatus indicates that the satisfaction status for the identified local objective should be retrieved from the identified shared global objective when the progress for this objective is undefined.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: True<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ReadSatisfiedStatus = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.ReadSatisfiedStatus;
        
        /// <summary>
        /// Name of the ReadNormalizedMeasure property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>ReadNormalizedMeasure is indicates that the normalized measure for the identified local objective should be retrieved from the identified shared global objective when the measure for this objective is undefined.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: True<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ReadNormalizedMeasure = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.ReadNormalizedMeasure;
        
        /// <summary>
        /// Name of the WriteSatisfiedStatus property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>WriteSatisfiedStatus indicates that the normalized measure for the this objective should be retrieved from the identified shared global objective when the measure for the this objective is undefined.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string WriteSatisfiedStatus = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.WriteSatisfiedStatus;
        
        /// <summary>
        /// Name of the WriteNormalizedMeasure property on the <Typ>MapActivityObjectiveToGlobalObjectiveItem</Typ> item type.
        /// <para>WriteNormalizedMeasure indicates that the normalized measure for the identified local objective should be transferred to the identified shared global objective upon termination of the attempt on the activity.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Default value: False<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string WriteNormalizedMeasure = Microsoft.LearningComponents.Storage.BaseSchema.MapActivityObjectiveToGlobalObjectiveItem.WriteNormalizedMeasure;
    }
    
    /// <summary>
    /// Contains constants related to the PackageItem item type.
    /// <para>Each item contains information about one <a href="SlkConcepts.htm#Packages">e-learning
    /// package</a>.  Before a package can be
    /// <a href="SlkConcepts.htm#Assignments">attempted</a>, a row referring to it must be
    /// added to this table.  When that happens, detailed information about the package is
    /// added to other tables in the schema; for example, information about each
    /// <a href="SlkConcepts.htm#Packages">activity</a> is added to the ActivityPackageItem
    /// table.  SLK <a href="SlkSchema.xml.htm">extends</a> this item type by adding the <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Warnings.Field.htm">Warnings</a> property/column.
    /// </para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Manifest</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>Warnings</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class PackageItem {
        
        /// <summary>
        /// Name of the <Typ>PackageItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>PackageItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>PackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.Id;
        
        /// <summary>
        /// Name of the PackageFormat property on the <Typ>PackageItem</Typ> item type.
        /// <para>PackageFormat is the type of content in the package: SCORM 2004, SCORM 1.2, or Class Server format.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ><p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.PackageFormat;
        
        /// <summary>
        /// Name of the Location property on the <Typ>PackageItem</Typ> item type.
        /// <para>Location is the location of the files within the package. The format of this field is determined by the application.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[260]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.Location;
        
        /// <summary>
        /// Maximum length of the <Fld>Location</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLocationLength = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.MaxLocationLength;
        
        /// <summary>
        /// Name of the Manifest property on the <Typ>PackageItem</Typ> item type.
        /// <para>Manifest is the imsmanifest.xml of the package.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Manifest = Microsoft.LearningComponents.Storage.BaseSchema.PackageItem.Manifest;
        
        /// <summary>
        /// Name of the Warnings property on the <Typ>PackageItem</Typ> item type.
        /// <para>
        /// Warnings contains warnings that SLK detected when the package was
        /// <a href="Microsoft.SharePointLearningKit.SlkStore.RegisterPackage.Method.htm">registered</a>.
        /// This XML consists of a root "&lt;Warnings&gt;" element containing one
        /// "&lt;Warning&gt;" element per warning, each of which contains the text of the
        /// warning as the content of the element plus the following attributes: the
        /// "Code" attribute contains the warning's validation result code, and "Type"
        /// attribute contains the warning's type, either "Error" or "Warning".  Warnings
        /// is <b>null</b> if there are no warnings.
        /// </para>
        /// <para>
        /// This property/column is available only in <a href="Default.htm">SLK</a> (not in
        /// <a href="Mlc.htm">MLC</a>).
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Warnings = "Warnings";
    }
    
    /// <summary>
    /// Contains constants related to the RubricItem item type.
    /// <para>Each item contains information about a rubric that is attached to an
    /// <a href="SlkConcepts.htm#ScormConcepts">interaction</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>IsSatisfied</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Points</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class RubricItem {
        
        /// <summary>
        /// Name of the <Typ>RubricItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>RubricItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>RubricItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.Id;
        
        /// <summary>
        /// Name of the InteractionId property on the <Typ>RubricItem</Typ> item type.
        /// <para>InteractionId is the identifier of the interaction that owns this rubric.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>InteractionItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.InteractionId;
        
        /// <summary>
        /// Name of the Ordinal property on the <Typ>RubricItem</Typ> item type.
        /// <para>Ordinal is the identifier of the rubric. Unique within an interaction.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Int32<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.Ordinal;
        
        /// <summary>
        /// Name of the IsSatisfied property on the <Typ>RubricItem</Typ> item type.
        /// <para>IsSatisfied indicates if the user has satisfied this rubric.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsSatisfied = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.IsSatisfied;
        
        /// <summary>
        /// Name of the Points property on the <Typ>RubricItem</Typ> item type.
        /// <para>Points is the point value allowed for this rubric.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Default value: null<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Points = Microsoft.LearningComponents.Storage.BaseSchema.RubricItem.Points;
    }
    
    /// <summary>
    /// Contains constants related to the ResourceItem item type.
    /// <para>Each item contains information about <a href="SlkConcepts.htm#ScormConcepts">resources</a> used in
    /// activities, including the XML of each <tt>&lt;resource&gt;</tt> element in the
    /// package.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>ResourceXml</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ResourceItem {
        
        /// <summary>
        /// Name of the <Typ>ResourceItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.ResourceItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>ResourceItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>ResourceItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.ResourceItem.Id;
        
        /// <summary>
        /// Name of the PackageId property on the <Typ>ResourceItem</Typ> item type.
        /// <para>PackageId is the identifier of the package that contains this resource.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>PackageItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.ResourceItem.PackageId;
        
        /// <summary>
        /// Name of the ResourceXml property on the <Typ>ResourceItem</Typ> item type.
        /// <para>ResourceXml is the &lt;resources&gt; node from the package corresponding to these resources.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Xml<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResourceXml = Microsoft.LearningComponents.Storage.BaseSchema.ResourceItem.ResourceXml;
    }
    
    /// <summary>
    /// Contains constants related to the SequencingLogEntryItem item type.
    /// <para>Each item contains one sequencing log entry, related to one
    /// <a href="SlkConcepts.htm#Packages">activity</a> of one
    /// <a href="SlkConcepts.htm#Assignments">attempt</a>, or related to an entire
    /// <a href="SlkConcepts.htm#Assignments">attempt</a>.
    /// See <a href="Microsoft.LearningComponents.Storage.StoredLearningSession.LoggingOptions.Property.htm">StoredLearningSession.LoggingOptions</a>.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> Access is granted to no users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>EventType</Fld></li>
    /// <li><Fld>Message</Fld></li>
    /// <li><Fld>NavigationCommand</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SequencingLogEntryItem {
        
        /// <summary>
        /// Name of the <Typ>SequencingLogEntryItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>SequencingLogEntryItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.Id;
        
        /// <summary>
        /// Name of the AttemptId property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>AttemptId is the identifier for the attempt to which the log entry applies.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AttemptItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.AttemptId;
        
        /// <summary>
        /// Name of the ActivityAttemptId property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>ActivityAttemptId is the identifier of the activity that is active after this log entry was created.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.ActivityAttemptId;
        
        /// <summary>
        /// Name of the EventType property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>EventType is the type of event that caused this entry.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.SequencingEventType</Typ><p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EventType = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.EventType;
        
        /// <summary>
        /// Name of the Message property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>Message is the description that includes why the event happened.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1073741822]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Message = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.Message;
        
        /// <summary>
        /// Maximum length of the <Fld>Message</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxMessageLength = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.MaxMessageLength;
        
        /// <summary>
        /// Name of the NavigationCommand property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>NavigationCommand is the navigation command that caused this event.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.NavigationCommand</Typ><p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NavigationCommand = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.NavigationCommand;
        
        /// <summary>
        /// Name of the Timestamp property on the <Typ>SequencingLogEntryItem</Typ> item type.
        /// <para>Timestamp is the time (UTC) at which the log entry was created.</para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.SequencingLogEntryItem.Timestamp;
    }
    
    /// <summary>
    /// Contains constants related to the UserItem item type.
    /// <para>Each item contains information about one user, including a numeric identifier
    /// (<a href="Microsoft.LearningComponents.Storage.UserItemIdentifier.Class.htm">UserItemIdentifier</a>)
    /// used elsewhere in the database to refer to the user.  UserItem items contain a
    /// <a href="Microsoft.LearningComponents.Storage.BaseSchema.UserItem.Key.Field.htm">Key</a>
    /// property (database column) which, for SLK, contains either the security ID (SID) of the
    /// user or the user's login name.  (The login name is used for authentication mechanisms
    /// such as forms-based authentication which don't use security IDs.)</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is available in both <a href="Default.htm">SLK</a> and <a href="Mlc.htm">MLC</a>.  SLK <a href="SlkSchema.xml.htm">extends</a> this item type by granting access to all users.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> In MLC, access is granted to no users.  In SLK, query access only is granted to all users.</para>
    /// <para>In MLC, this item type is defined in the namespace Microsoft.LearningComponents.Storage.BaseSchema, in the assembly Microsoft.LearningComponents.Storage.dll.</para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AudioCaptioning</Fld></li>
    /// <li><Fld>AudioLevel</Fld></li>
    /// <li><Fld>DeliverySpeed</Fld></li>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>Language</Fld></li>
    /// <li><Fld>Name</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class UserItem {
        
        /// <summary>
        /// Name of the <Typ>UserItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.ItemTypeName;
        
        /// <summary>
        /// Name of the Id property on the <Typ>UserItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.Id;
        
        /// <summary>
        /// Name of the Key property on the <Typ>UserItem</Typ> item type.
        /// <para>Key is a string that uniquely identifies this user.  The format of the key is defined by the application.  For example, SLK uses a security ID (SID) if available, or a login name for authentication mechanisms (such as forms-based authentication) that don't support SIDs.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[250]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.Key;
        
        /// <summary>
        /// Maximum length of the <Fld>Key</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxKeyLength = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.MaxKeyLength;
        
        /// <summary>
        /// Name of the Name property on the <Typ>UserItem</Typ> item type.
        /// <para>Name is the "friendly name" of the learner -- typically first and last name.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Name = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.Name;
        
        /// <summary>
        /// Maximum length of the <Fld>Name</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxNameLength = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.MaxNameLength;
        
        /// <summary>
        /// Name of the AudioCaptioning property on the <Typ>UserItem</Typ> item type.
        /// <para>AudioCaptioning specifies whether audio captioning is enabled for this user.</para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.AudioCaptioning</Typ><p/>
        /// Property can not contain null.<p/>
        /// Default value: <Fld>/Microsoft.LearningComponents.AudioCaptioning.NoChange</Fld><p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AudioCaptioning = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.AudioCaptioning;
        
        /// <summary>
        /// Name of the AudioLevel property on the <Typ>UserItem</Typ> item type.
        /// <para>AudioLevel is the user's preferred audio listening level.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can not contain null.<p/>
        /// Default value: 1<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AudioLevel = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.AudioLevel;
        
        /// <summary>
        /// Name of the DeliverySpeed property on the <Typ>UserItem</Typ> item type.
        /// <para>DeliverySpeed is a scale for how fast content is delivered.</para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can not contain null.<p/>
        /// Default value: 1<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DeliverySpeed = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.DeliverySpeed;
        
        /// <summary>
        /// Name of the Language property on the <Typ>UserItem</Typ> item type.
        /// <para>Language is the preferred language of the user.</para>
        /// </summary>
        /// <remarks>
        /// Property type: String[255]<p/>
        /// Property can not contain null.<p/>
        /// Default value: ""<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Language = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.Language;
        
        /// <summary>
        /// Maximum length of the <Fld>Language</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxLanguageLength = Microsoft.LearningComponents.Storage.BaseSchema.UserItem.MaxLanguageLength;
    }
    
    /// <summary>
    /// Contains constants related to the UserItemSite item type.
    /// <para>Each item contains information about the SPUser of one user in a particular site collection.</para>
    /// <para>This <a href="SlkSchema.htm">LearningStore item type</a> is only available in <a href="Default.htm">SLK</a>.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><a href="SlkSchema.htm">Default operation-level security</a>:</b> In SLK, query access only is granted to all users.</para>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>UserId</Fld></li>
    /// <li><Fld>SPSiteGuid</Fld></li>
    /// <li><Fld>SPUserId</Fld></li>
    /// </ul>
    /// </remarks>
    public abstract class UserItemSite {
        
        /// <summary>
        /// Name of the <Typ>UserItemSite</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "UserItemSite";
        
        /// <summary>Name of the UserId property on the <Typ>UserItemSite</Typ> item type.</summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string UserId = "UserId";

        /// <summary>
        /// Name of the SPSiteGuid property on the <Typ>UserItemSite</Typ> item type.
        /// <para>
        /// SPSiteGuid is the GUID of the SharePoint site collection (SPSite) that this
        /// user is associated with.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.UserItemSite.SPSiteGuid.Property.htm">UserItemSite.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        public const string SPSiteGuid = "SPSiteGuid";

        /// <summary>Name of the SPUserId property on the <Typ>UserItemSite</Typ> item type.</summary>
        /// <remarks>
        /// Property type: Reference to an id of an SPUser in a particular site collection.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string SPUserId = "SPUserId";

    }
    
    /// <summary>
    /// Contains constants related to the SiteSettingsItem item type.
    /// <para>
    /// Each item of this <a href="SlkSchema.htm">LearningStore item type</a> contains the
    /// SLK Settings corresponding to a given SharePoint site collectino (SPSite).
    /// </para>
    /// <para>
    /// This item type is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>SettingsXml</Fld></li>
    /// <li><Fld>SettingsXmlLastModified</Fld></li>
    /// <li><Fld>SiteGuid</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SiteSettingsItem {
        
        /// <summary>
        /// Name of the <Typ>SiteSettingsItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "SiteSettingsItem";
        
        /// <summary>
        /// Name of the Id property on the <Typ>SiteSettingsItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>SiteSettingsItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the SiteGuid property on the <Typ>SiteSettingsItem</Typ> item type.
        /// <para>
        /// SiteGuid is the GUID of the SharePoint site collection (SPSite) that this
        /// <a href="Microsoft.SharePointLearningKit.Schema.SiteSettingsItem.Class.htm">SiteSettingsItem</a>
        /// refers to.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SiteGuid = "SiteGuid";
        
        /// <summary>
        /// Name of the SettingsXml property on the <Typ>SiteSettingsItem</Typ> item type.
        /// <para>
        /// SettingsXml is the <a href="SlkSettings.htm">SLK Settings</a> XML of the
        /// SharePoint site collection (SPSite) that this
        /// <a href="Microsoft.SharePointLearningKit.Schema.SiteSettingsItem.Class.htm">SiteSettingsItem</a>
        /// refers to.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This XML is stored as a string, not as a SQL Server XML column, to allow for
        /// line numbers within error messages.
        /// </para><p/>
        /// Property type: String[1073741822]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SettingsXml = "SettingsXml";
        
        /// <summary>
        /// Maximum length of the <Fld>SettingsXml</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxSettingsXmlLength = 1073741822;
        
        /// <summary>
        /// Name of the SettingsXmlLastModified property on the <Typ>SiteSettingsItem</Typ> item type.
        /// <para>
        /// SettingsXmlLastModified is the date/time (UTC) that the SLK Settings referred
        /// to by this
        /// <a href="Microsoft.SharePointLearningKit.Schema.SiteSettingsItem.Class.htm">SiteSettingsItem</a>
        /// was uploaded.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SettingsXmlLastModified = "SettingsXmlLastModified";
    }
    
    /// <summary>
    /// Contains constants related to the AssignmentItem item type.
    /// <para>
    /// Each item of this <a href="SlkSchema.htm">LearningStore item type</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">assignment</a>,
    /// i.e. one <a href="SlkConcepts.htm#Packages">organization</a> of one e-learning
    /// package, or one <a href="SlkConcepts.htm#Packages">non-e-learning document</a>,
    /// assigned to one or more learners, with zero or more instructors.
    /// </para>
    /// <para>
    /// This item type is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// For each learner on this assignment, there is one
    /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Class.htm">LearnerAssignmentItem</a>.
    /// For each instructor on this assignment, there is one
    /// <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Class.htm">InstructorAssignmentItem</a>.
    /// If this assignment is associated with an
    /// <a href="SlkConcepts.htm#Packages">e-learning package</a> (not a non-e-learning
    /// document), then when the learner begins the assignment an MLC
    /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.Class.htm">AttemptItem</a> is created.
    /// </para>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AutoReturn</Fld></li>
    /// <li><Fld>EmailChanges</Fld></li>
    /// <li><Fld>CreatedBy</Fld></li>
    /// <li><Fld>DateCreated</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>DueDate</Fld></li>
    /// <li><Fld>NonELearningLocation</Fld></li>
    /// <li><Fld>PointsPossible</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// <li><Fld>ShowAnswersToLearners</Fld></li>
    /// <li><Fld>SPSiteGuid</Fld></li>
    /// <li><Fld>SPWebGuid</Fld></li>
    /// <li><Fld>StartDate</Fld></li>
    /// <li><Fld>Title</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AssignmentItem {
        
        /// <summary>
        /// Name of the <Typ>AssignmentItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "AssignmentItem";
        
        /// <summary>
        /// Name of the Id property on the <Typ>AssignmentItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>AssignmentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the SPSiteGuid property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// SPSiteGuid is the GUID of the SharePoint site collection (SPSite) that this
        /// assignment is associated with.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.SPSiteGuid.Property.htm">AssignmentProperties.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPSiteGuid = "SPSiteGuid";
        
        /// <summary>
        /// Name of the SPWebGuid property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// SPWebGuid is the GUID of the SharePoint Web site (SPWeb) that this assignment is
        /// associated with.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.SPWebGuid.Property.htm">AssignmentProperties.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPWebGuid = "SPWebGuid";
        
        /// <summary>
        /// Name of the RootActivityId property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// If e-learning content was assigned, RootActivityId is the
        /// <a href="Microsoft.LearningComponents.Storage.ActivityPackageItemIdentifier.Class.htm">ActivityPackageItemIdentifier</a>
        /// of the assigned SCORM <a href="SlkConcepts.htm#Packages">organization</a>.
        /// If non-e-learning content was assigned, RootActivityId is <b>null</b> -- in that
        /// case, <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">NonELearningLocation</a> is used.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>ActivityPackageItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the NonELearningLocation property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// If non-e-learning content was assigned, NonELearningLocation is the
        /// MLC SharePoint location string (i.e. the same value used in
        /// <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>)
        /// of the assigned <a href="SlkConcepts.htm#Packages">non-e-learning</a> document.
        /// If e-learning content was assigned, NonELearningLocation is <b>null</b> -- in that
        /// case, <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">RootActivityId</a> is used.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1000]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NonELearningLocation = "NonELearningLocation";
        
        /// <summary>
        /// Maximum length of the <Fld>NonELearningLocation</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxNonELearningLocationLength = 1000;
        
        /// <summary>
        /// Name of the Title property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// Title is the title of the assignment.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.Title.Property.htm">AssignmentProperties.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1000]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Title = "Title";
        
        /// <summary>
        /// Maximum length of the <Fld>Title</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxTitleLength = 1000;
        
        /// <summary>
        /// Name of the StartDate property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// StartDate is the start date/time (UTC) of the assignment.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.StartDate.Property.htm">AssignmentProperties.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StartDate = "StartDate";
        
        /// <summary>
        /// Name of the DueDate property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// DueDate is the due date/time (UTC) of the assignment, or <b>null</b> if the
        /// assignment has no due date/time.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.DueDate.Property.htm">AssignmentProperties.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DueDate = "DueDate";
        
        /// <summary>
        /// Name of the PointsPossible property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// The nominal maximum number of points that can be awarded to a learner on this
        /// assignment, or <b>null</b> if this information is not provided.  The actual
        /// number of points awarded may exceed this value.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.PointsPossible.Property.htm">AssignmentProperties.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PointsPossible = "PointsPossible";
        
        /// <summary>
        /// Name of the Description property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// Description is the description of the assignment, including instructions to the
        /// learner (if any).
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.Description.Property.htm">AssignmentProperties.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1073741822]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = "Description";
        
        /// <summary>
        /// Maximum length of the <Fld>Description</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxDescriptionLength = 1073741822;
        
        /// <summary>
        /// Name of the EmailChanges property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// EmailChanges is <b>true</b> if emails should be sent to the learners on creation and modification of assignments.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.EmailChanges.Property.htm">AssignmentProperties.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        public const string EmailChanges = "EmailChanges";
        
        /// <summary>
        /// Name of the AutoReturn property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// AutoReturn is <b>true</b> if the assignment should be automatically returned to
        /// learners when they submit it to the instructor or (for self-assigned assignments)
        /// mark the assignment as complete.  If <b>false</b>, the instructor has an
        /// opportunity to grade the learner's work.  AutoReturn should always be <b>true</b>
        /// for self-assigned assignments.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.AutoReturn.Property.htm">AssignmentProperties.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AutoReturn = "AutoReturn";
        
        /// <summary>
        /// Name of the ShowAnswersToLearners property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// ShowAnswersToLearners is <b>true</b> if answers will be shown to the learner when a
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>
        /// associated with this assignment is returned to the learner.
        /// This only applies to certain types of e-learning content.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.ShowAnswersToLearners.Property.htm">AssignmentProperties.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ShowAnswersToLearners = "ShowAnswersToLearners";
        
        /// <summary>
        /// Name of the CreatedBy property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// CreatedBy is the
        /// <a href="Microsoft.LearningComponents.Storage.UserItemIdentifier.Class.htm">UserItemIdentifier</a>
        /// of the user who created the assignment.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.CreatedById.Property.htm">AssignmentProperties.CreatedById</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CreatedBy = "CreatedBy";
        
        /// <summary>
        /// Name of the DateCreated property on the <Typ>AssignmentItem</Typ> item type.
        /// <para>
        /// DateCreated is the date/time (UTC) that the assignment was created.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.AssignmentProperties.DateCreated.Property.htm">AssignmentProperties.DateCreated</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DateCreated = "DateCreated";
    }
    
    /// <summary>
    /// Contains constants related to the InstructorAssignmentItem item type.
    /// <para>
    /// Each item of this <a href="SlkSchema.htm">LearningStore item type</a> contains
    /// information about one
    /// <a href="SlkConcepts.htm#Assignments">instructor assignment</a>,
    /// i.e. the mapping of one instructor to one
    /// <a href="SlkConcepts.htm#Assignments">assignment</a>.
    /// For example, if an assignment has three instructors, there will be three
    /// InstructorAssignmentItem table rows associated with that assignment.
    /// </para>
    /// <para>
    /// This item type is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>InstructorId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class InstructorAssignmentItem {
        
        /// <summary>
        /// Name of the <Typ>InstructorAssignmentItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "InstructorAssignmentItem";
        
        /// <summary>
        /// Name of the Id property on the <Typ>InstructorAssignmentItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>InstructorAssignmentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the AssignmentId property on the <Typ>InstructorAssignmentItem</Typ> item type.
        /// <para>
        /// AssignmentId is the
        /// <a href="Microsoft.SharePointLearningKit.AssignmentItemIdentifier.Class.htm">AssignmentItemIdentifier</a>
        /// of the assignment associated with this
        /// <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Class.htm">InstructorAssignmentItem</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AssignmentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the InstructorId property on the <Typ>InstructorAssignmentItem</Typ> item type.
        /// <para>
        /// InstructorId is the
        /// <a href="Microsoft.LearningComponents.Storage.UserItemIdentifier.Class.htm">UserItemIdentifier</a>
        /// of the instructor associated with this
        /// <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Class.htm">InstructorAssignmentItem</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorId = "InstructorId";
    }
    
    /// <summary>
    /// Contains constants related to the LearnerAssignmentItem item type.
    /// <para>
    /// Each item of this <a href="SlkSchema.htm">LearningStore item type</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">learner assignment</a>,
    /// i.e. the mapping of one learner to one
    /// <a href="SlkConcepts.htm#Assignments">assignment</a>.  For example, if an
    /// assignment has 30 learners, there will be 30 LearnerAssignmentItem table rows
    /// associated with that assignment.
    /// </para>
    /// <para>
    /// This item type is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>FinalPoints</Fld></li>
    /// <li><Fld>Grade</Fld></li>
    /// <li><Fld>GuidId</Fld></li>
    /// <li><Fld>InstructorComments</Fld></li>
    /// <li><Fld>IsFinal</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>NonELearningStatus</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerAssignmentItem {
        
        /// <summary>
        /// Name of the <Typ>LearnerAssignmentItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "LearnerAssignmentItem";
        
        /// <summary>
        /// Name of the Id property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the GuidId property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// GuidId is a column that is similar to the Id column that identifies a LearnerAssignment
        /// uniquely. It is here as a more non-guessable entity compared to the Id column
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can contain null.<p/>
        /// Default value: (newid())<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string GuidId = "GuidId";
        
        /// <summary>
        /// Name of the AssignmentId property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// AssignmentId is the
        /// <a href="Microsoft.SharePointLearningKit.AssignmentItemIdentifier.Class.htm">AssignmentItemIdentifier</a>
        /// of the assignment associated with this
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Class.htm">LearnerAssignmentItem</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>AssignmentItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// Item will automatically be deleted when the referred-to item is deleted.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the LearnerId property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// LearnerId is the
        /// <a href="Microsoft.LearningComponents.Storage.UserItemIdentifier.Class.htm">UserItemIdentifier</a>
        /// of the learner associated with this
        /// <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Class.htm">InstructorAssignmentItem</a>.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.GradingProperties.LearnerId.Property.htm">GradingProperties.LearnerId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Reference to a <Typ>UserItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = "LearnerId";
        
        /// <summary>
        /// Name of the IsFinal property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// IsFinal is <b>true</b> if the
        /// <a href="SlkApi.htm#LearnerAssignmentStates">learner assignment state</a> is
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">Final</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Boolean<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsFinal = "IsFinal";
        
        /// <summary>
        /// Name of the NonELearningStatus property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// NonELearningStatus is the
        /// <a href="Microsoft.LearningComponents.AttemptStatus.Enumeration.htm">AttemptStatus</a>
        /// of the
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>, if a
        /// <a href="SlkConcepts.htm#Packages">non-e-learning document</a> was assigned
        /// (unused if an e-learning package was assigned).  <b>null</b> if the learner hasn't
        /// yet started the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ><p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NonELearningStatus = "NonELearningStatus";
        
        /// <summary>
        /// Name of the FinalPoints property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// FinalPoints is the number of points the learner received on this
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// When the learner submits the assignment, FinalPoints is initially the same as
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>,
        /// but the instructor may manually change the value of FinalPoints.  For example, the
        /// instructor may award bonus points to the learner.  <b>null</b> if the final points
        /// value is blank.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.GradingProperties.FinalPoints.Property.htm">GradingProperties.FinalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Single<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinalPoints = "FinalPoints";
        
        /// <summary>
        /// Name of the Grade property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// Grade is a manually marked grade on this <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.GradingProperties.Grade.Property.htm">GradingProperties.Grade</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[20]<p/>
        /// Property can contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        public const string Grade = "Grade";
        
        /// <summary>
        /// Name of the InstructorComments property on the <Typ>LearnerAssignmentItem</Typ> item type.
        /// <para>
        /// InstructorComments contains free-form comments, if any, from the instructor
        /// about this
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// Corresponds to <a href="Microsoft.SharePointLearningKit.GradingProperties.InstructorComments.Property.htm">GradingProperties.InstructorComments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[1073741822]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorComments = "InstructorComments";
        
        /// <summary>
        /// Maximum length of the <Fld>InstructorComments</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxInstructorCommentsLength = 1073741822;
    }
    
    /// <summary>
    /// Contains constants related to the UserWebListItem item type.
    /// <para>
    /// Each item of this <a href="SlkSchema.htm">LearningStore item type</a> represents
    /// one entry in one user's <b>user Web list</b>, for one site collection.  A user Web
    /// list is the list of Web sites that appears on the SLK E-Learning Actions page for
    /// all document libraries within a given site collection.  Each user has at most one
    /// user Web list for each site collection (SPSite).
    /// </para>
    /// <para>
    /// This item type is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Properties on the item type:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>LastAccessTime</Fld></li>
    /// <li><Fld>OwnerKey</Fld></li>
    /// <li><Fld>SPSiteGuid</Fld></li>
    /// <li><Fld>SPWebGuid</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class UserWebListItem {
        
        /// <summary>
        /// Name of the <Typ>UserWebListItem</Typ> item type.
        /// </summary>
        public const string ItemTypeName = "UserWebListItem";
        
        /// <summary>
        /// Name of the Id property on the <Typ>UserWebListItem</Typ> item type.
        /// </summary>
        /// <remarks>
        /// The value stored in this property is generated automatically when a new item is
        /// added.  It is unique across all items of the item type.<p/>
        /// Property type: Reference to a <Typ>UserWebListItem</Typ> item type.<p/>
        /// Property can not contain null.<p/>
        /// </remarks>
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the OwnerKey property on the <Typ>UserWebListItem</Typ> item type.
        /// <para>
        /// OwnerKey is the
        /// <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>
        /// value of the user that owns this user Web list item.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: String[250]<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string OwnerKey = "OwnerKey";
        
        /// <summary>
        /// Maximum length of the <Fld>OwnerKey</Fld> property in characters.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const int MaxOwnerKeyLength = 250;
        
        /// <summary>
        /// Name of the SPSiteGuid property on the <Typ>UserWebListItem</Typ> item type.
        /// <para>
        /// SPSiteGuid is the GUID of the SharePoint site collection (SPSite) that this
        /// user Web list item is associated with.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPSiteGuid = "SPSiteGuid";
        
        /// <summary>
        /// Name of the SPWebGuid property on the <Typ>UserWebListItem</Typ> item type.
        /// <para>
        /// SPWebGuid is the GUID of the SharePoint Web site (SPWeb) that this user Web list
        /// item is associated with.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: Guid<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPWebGuid = "SPWebGuid";
        
        /// <summary>
        /// Name of the LastAccessTime property on the <Typ>UserWebListItem</Typ> item type.
        /// <para>
        /// LastAccessTime is the date/time (UTC) that this user Web list item was last
        /// accessed via an operation such as assignment creation.  (Viewing the item within
        /// the list on the E-Learning Actions page in SLK doesn't count as an access.)
        /// </para>
        /// </summary>
        /// <remarks>
        /// Property type: DateTime<p/>
        /// Property can not contain null.<p/>
        /// Property does not have a default value.<p/>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LastAccessTime = "LastAccessTime";
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavOrganizationGlobalObjectiveView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>OrganizationId</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavOrganizationGlobalObjectiveView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.ViewName;
        
        /// <summary>
        /// Name of the OrganizationId column on the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string OrganizationId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.OrganizationId;
        
        /// <summary>
        /// Name of the Key column on the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.Key;
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.LearnerId;
        
        /// <summary>
        /// Name of the ScaledScore column on the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus column on the <Typ>SeqNavOrganizationGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavOrganizationGlobalObjectiveView.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavLearnerGlobalObjectiveView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavLearnerGlobalObjectiveView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavLearnerGlobalObjectiveView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavLearnerGlobalObjectiveView.ViewName;
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>SeqNavLearnerGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavLearnerGlobalObjectiveView.LearnerId;
        
        /// <summary>
        /// Name of the Key column on the <Typ>SeqNavLearnerGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavLearnerGlobalObjectiveView.Key;
        
        /// <summary>
        /// Name of the ScaledScore column on the <Typ>SeqNavLearnerGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavLearnerGlobalObjectiveView.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus column on the <Typ>SeqNavLearnerGlobalObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavLearnerGlobalObjectiveView.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavActivityPackageView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackagePath</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavActivityPackageView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavActivityPackageView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityPackageView.ViewName;
        
        /// <summary>
        /// Name of the Id column on the <Typ>SeqNavActivityPackageView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityPackageView.Id;
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>SeqNavActivityPackageView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityPackageView.PackageId;
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>SeqNavActivityPackageView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityPackageView.PackageFormat;
        
        /// <summary>
        /// Name of the PackagePath column on the <Typ>SeqNavActivityPackageView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackagePath = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityPackageView.PackagePath;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavActivityTreeView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ObjectivesGlobalToSystem</Fld></li>
    /// <li><Fld>ParentActivityId</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavActivityTreeView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.ViewName;
        
        /// <summary>
        /// Name of the Id column on the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.Id;
        
        /// <summary>
        /// Name of the ParentActivityId column on the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ParentActivityId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.ParentActivityId;
        
        /// <summary>
        /// Name of the DataModelCache column on the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.DataModelCache;
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.RootActivityId;
        
        /// <summary>
        /// Name of the ObjectivesGlobalToSystem column on the <Typ>SeqNavActivityTreeView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ObjectivesGlobalToSystem = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityTreeView.ObjectivesGlobalToSystem;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptStatus</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>CurrentActivityId</Fld></li>
    /// <li><Fld>FinishedTimestamp</Fld></li>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>LearnerAudioCaptioning</Fld></li>
    /// <li><Fld>LearnerAudioLevel</Fld></li>
    /// <li><Fld>LearnerDeliverySpeed</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>LearnerLanguage</Fld></li>
    /// <li><Fld>LearnerName</Fld></li>
    /// <li><Fld>LogDetailSequencing</Fld></li>
    /// <li><Fld>LogFinalSequencing</Fld></li>
    /// <li><Fld>LogRollup</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackagePath</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// <li><Fld>StartedTimestamp</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// <li><Fld>SuspendedActivityId</Fld></li>
    /// <li><Fld>TotalPoints</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.ViewName;
        
        /// <summary>
        /// Name of the Id column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.Id;
        
        /// <summary>
        /// Name of the AttemptStatus column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.AttemptStatus;
        
        /// <summary>
        /// Name of the LogDetailSequencing column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogDetailSequencing = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LogDetailSequencing;
        
        /// <summary>
        /// Name of the LogFinalSequencing column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogFinalSequencing = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LogFinalSequencing;
        
        /// <summary>
        /// Name of the LogRollup column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LogRollup = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LogRollup;
        
        /// <summary>
        /// Name of the CurrentActivityId column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CurrentActivityId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.CurrentActivityId;
        
        /// <summary>
        /// Name of the SuspendedActivityId column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuspendedActivityId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.SuspendedActivityId;
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.RootActivityId;
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.PackageId;
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.PackageFormat;
        
        /// <summary>
        /// Name of the PackagePath column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackagePath = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.PackagePath;
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerId;
        
        /// <summary>
        /// Name of the LearnerName column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerName;
        
        /// <summary>
        /// Name of the LearnerAudioCaptioning column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AudioCaptioning</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAudioCaptioning = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerAudioCaptioning;
        
        /// <summary>
        /// Name of the LearnerAudioLevel column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAudioLevel = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerAudioLevel;
        
        /// <summary>
        /// Name of the LearnerDeliverySpeed column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerDeliverySpeed = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerDeliverySpeed;
        
        /// <summary>
        /// Name of the LearnerLanguage column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerLanguage = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.LearnerLanguage;
        
        /// <summary>
        /// Name of the StartedTimestamp column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StartedTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.StartedTimestamp;
        
        /// <summary>
        /// Name of the FinishedTimestamp column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinishedTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.FinishedTimestamp;
        
        /// <summary>
        /// Name of the TotalPoints column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TotalPoints = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.TotalPoints;
        
        /// <summary>
        /// Name of the SuccessStatus column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.SuccessStatus;
        
        /// <summary>
        /// Name of the CompletionStatus column on the <Typ>SeqNavAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptView.CompletionStatus;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavActivityAttemptView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityPackageId</Fld></li>
    /// <li><Fld>AttemptCount</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>EvaluationPoints</Fld></li>
    /// <li><Fld>Exit</Fld></li>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>LessonStatus</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>ObjectivesGlobalToSystem</Fld></li>
    /// <li><Fld>ParentId</Fld></li>
    /// <li><Fld>ProgressMeasure</Fld></li>
    /// <li><Fld>RandomPlacement</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SequencingDataCache</Fld></li>
    /// <li><Fld>SessionStartTimestamp</Fld></li>
    /// <li><Fld>SessionTime</Fld></li>
    /// <li><Fld>StaticDataModelCache</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// <li><Fld>SuspendData</Fld></li>
    /// <li><Fld>TotalTime</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavActivityAttemptView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ViewName;
        
        /// <summary>
        /// Name of the Id column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.Id;
        
        /// <summary>
        /// Name of the DataModelCache column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.DataModelCache;
        
        /// <summary>
        /// Name of the SequencingDataCache column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SequencingDataCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.SequencingDataCache;
        
        /// <summary>
        /// Name of the RandomPlacement column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RandomPlacement = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.RandomPlacement;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.AttemptId;
        
        /// <summary>
        /// Name of the ActivityPackageId column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityPackageId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ActivityPackageId;
        
        /// <summary>
        /// Name of the StaticDataModelCache column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StaticDataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.StaticDataModelCache;
        
        /// <summary>
        /// Name of the ParentId column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ParentId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ParentId;
        
        /// <summary>
        /// Name of the ObjectivesGlobalToSystem column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ObjectivesGlobalToSystem = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ObjectivesGlobalToSystem;
        
        /// <summary>
        /// Name of the CompletionStatus column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.CompletionStatus;
        
        /// <summary>
        /// Name of the AttemptCount column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCount = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.AttemptCount;
        
        /// <summary>
        /// Name of the EvaluationPoints column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EvaluationPoints = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.EvaluationPoints;
        
        /// <summary>
        /// Name of the Exit column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.ExitMode</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Exit = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.Exit;
        
        /// <summary>
        /// Name of the LessonStatus column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.LessonStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LessonStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.LessonStatus;
        
        /// <summary>
        /// Name of the Location column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.Location;
        
        /// <summary>
        /// Name of the MinScore column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.MinScore;
        
        /// <summary>
        /// Name of the MaxScore column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.MaxScore;
        
        /// <summary>
        /// Name of the ProgressMeasure column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ProgressMeasure = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ProgressMeasure;
        
        /// <summary>
        /// Name of the RawScore column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.RawScore;
        
        /// <summary>
        /// Name of the ScaledScore column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.ScaledScore;
        
        /// <summary>
        /// Name of the SessionStartTimestamp column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SessionStartTimestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.SessionStartTimestamp;
        
        /// <summary>
        /// Name of the SessionTime column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SessionTime = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.SessionTime;
        
        /// <summary>
        /// Name of the SuccessStatus column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.SuccessStatus;
        
        /// <summary>
        /// Name of the SuspendData column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuspendData = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.SuspendData;
        
        /// <summary>
        /// Name of the TotalTime column on the <Typ>SeqNavActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TotalTime = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavActivityAttemptView.TotalTime;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavCurrentActivityAttemptView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>Credit</Fld></li>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>ObjectivesGlobalToSystem</Fld></li>
    /// <li><Fld>RandomPlacement</Fld></li>
    /// <li><Fld>SequencingDataCache</Fld></li>
    /// <li><Fld>StaticDataModelCache</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavCurrentActivityAttemptView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.ViewName;
        
        /// <summary>
        /// Name of the Id column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.Id;
        
        /// <summary>
        /// Name of the DataModelCache column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.DataModelCache;
        
        /// <summary>
        /// Name of the SequencingDataCache column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SequencingDataCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.SequencingDataCache;
        
        /// <summary>
        /// Name of the RandomPlacement column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RandomPlacement = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.RandomPlacement;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.AttemptId;
        
        /// <summary>
        /// Name of the StaticDataModelCache column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StaticDataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.StaticDataModelCache;
        
        /// <summary>
        /// Name of the ObjectivesGlobalToSystem column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ObjectivesGlobalToSystem = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.ObjectivesGlobalToSystem;
        
        /// <summary>
        /// Name of the Credit column on the <Typ>SeqNavCurrentActivityAttemptView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Credit = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentActivityAttemptView.Credit;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavCurrentCommentFromLmsView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>DataModelCache</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavCurrentCommentFromLmsView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.ViewName;
        
        /// <summary>
        /// Name of the Comment column on the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.Comment;
        
        /// <summary>
        /// Name of the Location column on the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.Location;
        
        /// <summary>
        /// Name of the Timestamp column on the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.Timestamp;
        
        /// <summary>
        /// Name of the DataModelCache column on the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DataModelCache = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.DataModelCache;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavCurrentCommentFromLmsView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavCurrentCommentFromLmsView.AttemptId;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptObjectiveView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptObjectiveId</Fld></li>
    /// <li><Fld>CompletionStatus</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>IsPrimaryObjective</Fld></li>
    /// <li><Fld>Key</Fld></li>
    /// <li><Fld>LessonStatus</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>ProgressMeasure</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>SuccessStatus</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptObjectiveView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.AttemptId;
        
        /// <summary>
        /// Name of the AttemptObjectiveId column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.AttemptObjectiveId;
        
        /// <summary>
        /// Name of the ActivityAttemptId column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.ActivityAttemptId;
        
        /// <summary>
        /// Name of the CompletionStatus column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.CompletionStatus;
        
        /// <summary>
        /// Name of the Description column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.Description;
        
        /// <summary>
        /// Name of the IsPrimaryObjective column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsPrimaryObjective = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.IsPrimaryObjective;
        
        /// <summary>
        /// Name of the Key column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Key = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.Key;
        
        /// <summary>
        /// Name of the LessonStatus column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.LessonStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LessonStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.LessonStatus;
        
        /// <summary>
        /// Name of the RawScore column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.RawScore;
        
        /// <summary>
        /// Name of the MinScore column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.MinScore;
        
        /// <summary>
        /// Name of the MaxScore column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.MaxScore;
        
        /// <summary>
        /// Name of the ProgressMeasure column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ProgressMeasure = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.ProgressMeasure;
        
        /// <summary>
        /// Name of the ScaledScore column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.ScaledScore;
        
        /// <summary>
        /// Name of the SuccessStatus column on the <Typ>SeqNavAttemptObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SuccessStatus = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveView.SuccessStatus;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptCommentFromLearnerView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>CommentFromLearnerId</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptCommentFromLearnerView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.AttemptId;
        
        /// <summary>
        /// Name of the ActivityAttemptId column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.ActivityAttemptId;
        
        /// <summary>
        /// Name of the CommentFromLearnerId column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>CommentFromLearnerItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CommentFromLearnerId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.CommentFromLearnerId;
        
        /// <summary>
        /// Name of the Comment column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.Comment;
        
        /// <summary>
        /// Name of the Location column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.Location;
        
        /// <summary>
        /// Name of the Timestamp column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.Timestamp;
        
        /// <summary>
        /// Name of the Ordinal column on the <Typ>SeqNavAttemptCommentFromLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCommentFromLearnerView.Ordinal;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptInteractionView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>Description</Fld></li>
    /// <li><Fld>EvaluationPoints</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>InteractionIdFromCmi</Fld></li>
    /// <li><Fld>InteractionType</Fld></li>
    /// <li><Fld>Latency</Fld></li>
    /// <li><Fld>LearnerResponseBool</Fld></li>
    /// <li><Fld>LearnerResponseNumeric</Fld></li>
    /// <li><Fld>LearnerResponseString</Fld></li>
    /// <li><Fld>MaxScore</Fld></li>
    /// <li><Fld>MinScore</Fld></li>
    /// <li><Fld>RawScore</Fld></li>
    /// <li><Fld>ResultNumeric</Fld></li>
    /// <li><Fld>ResultState</Fld></li>
    /// <li><Fld>ScaledScore</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// <li><Fld>Weighting</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptInteractionView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.AttemptId;
        
        /// <summary>
        /// Name of the ActivityAttemptId column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.ActivityAttemptId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.InteractionId;
        
        /// <summary>
        /// Name of the InteractionIdFromCmi column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionIdFromCmi = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.InteractionIdFromCmi;
        
        /// <summary>
        /// Name of the InteractionType column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.InteractionType</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionType = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.InteractionType;
        
        /// <summary>
        /// Name of the Timestamp column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.Timestamp;
        
        /// <summary>
        /// Name of the Weighting column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Weighting = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.Weighting;
        
        /// <summary>
        /// Name of the ResultState column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.InteractionResultState</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResultState = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.ResultState;
        
        /// <summary>
        /// Name of the ResultNumeric column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResultNumeric = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.ResultNumeric;
        
        /// <summary>
        /// Name of the Latency column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Latency = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.Latency;
        
        /// <summary>
        /// Name of the Description column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Description = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.Description;
        
        /// <summary>
        /// Name of the LearnerResponseBool column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseBool = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.LearnerResponseBool;
        
        /// <summary>
        /// Name of the LearnerResponseString column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseString = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.LearnerResponseString;
        
        /// <summary>
        /// Name of the LearnerResponseNumeric column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerResponseNumeric = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.LearnerResponseNumeric;
        
        /// <summary>
        /// Name of the ScaledScore column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.ScaledScore;
        
        /// <summary>
        /// Name of the RawScore column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RawScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.RawScore;
        
        /// <summary>
        /// Name of the MinScore column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.MinScore;
        
        /// <summary>
        /// Name of the MaxScore column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxScore = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.MaxScore;
        
        /// <summary>
        /// Name of the EvaluationPoints column on the <Typ>SeqNavAttemptInteractionView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EvaluationPoints = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionView.EvaluationPoints;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptEvaluationCommentLearnerView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>Comment</Fld></li>
    /// <li><Fld>EvaluationCommentId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>Location</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Timestamp</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptEvaluationCommentLearnerView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.AttemptId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.InteractionId;
        
        /// <summary>
        /// Name of the EvaluationCommentId column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>EvaluationCommentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string EvaluationCommentId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.EvaluationCommentId;
        
        /// <summary>
        /// Name of the Comment column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Comment = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.Comment;
        
        /// <summary>
        /// Name of the Location column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Location = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.Location;
        
        /// <summary>
        /// Name of the Timestamp column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Timestamp = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.Timestamp;
        
        /// <summary>
        /// Name of the Ordinal column on the <Typ>SeqNavAttemptEvaluationCommentLearnerView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptEvaluationCommentLearnerView.Ordinal;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptCorrectResponseView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>CorrectResponseId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>ResponsePattern</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptCorrectResponseView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptCorrectResponseView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCorrectResponseView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptCorrectResponseView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCorrectResponseView.AttemptId;
        
        /// <summary>
        /// Name of the CorrectResponseId column on the <Typ>SeqNavAttemptCorrectResponseView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>CorrectResponseItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CorrectResponseId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCorrectResponseView.CorrectResponseId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptCorrectResponseView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCorrectResponseView.InteractionId;
        
        /// <summary>
        /// Name of the ResponsePattern column on the <Typ>SeqNavAttemptCorrectResponseView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResponsePattern = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptCorrectResponseView.ResponsePattern;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptInteractionObjectiveView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptObjectiveId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>InteractionObjectiveId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptInteractionObjectiveView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptInteractionObjectiveView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionObjectiveView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptInteractionObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionObjectiveView.AttemptId;
        
        /// <summary>
        /// Name of the InteractionObjectiveId column on the <Typ>SeqNavAttemptInteractionObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionObjectiveItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionObjectiveView.InteractionObjectiveId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptInteractionObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionObjectiveView.InteractionId;
        
        /// <summary>
        /// Name of the AttemptObjectiveId column on the <Typ>SeqNavAttemptInteractionObjectiveView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionObjectiveView.AttemptObjectiveId;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptExtensionDataView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityAttemptId</Fld></li>
    /// <li><Fld>AttachmentGuid</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>BoolValue</Fld></li>
    /// <li><Fld>DateTimeValue</Fld></li>
    /// <li><Fld>DoubleValue</Fld></li>
    /// <li><Fld>ExtensionDataId</Fld></li>
    /// <li><Fld>IntValue</Fld></li>
    /// <li><Fld>Name</Fld></li>
    /// <li><Fld>StringValue</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptExtensionDataView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.AttemptId;
        
        /// <summary>
        /// Name of the ActivityAttemptId column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityAttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityAttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.ActivityAttemptId;
        
        /// <summary>
        /// Name of the ExtensionDataId column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ExtensionDataItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ExtensionDataId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.ExtensionDataId;
        
        /// <summary>
        /// Name of the Name column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Name = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.Name;
        
        /// <summary>
        /// Name of the StringValue column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StringValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.StringValue;
        
        /// <summary>
        /// Name of the IntValue column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IntValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.IntValue;
        
        /// <summary>
        /// Name of the BoolValue column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string BoolValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.BoolValue;
        
        /// <summary>
        /// Name of the DoubleValue column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DoubleValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.DoubleValue;
        
        /// <summary>
        /// Name of the DateTimeValue column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DateTimeValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.DateTimeValue;
        
        /// <summary>
        /// Name of the AttachmentGuid column on the <Typ>SeqNavAttemptExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentGuid = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptExtensionDataView.AttachmentGuid;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptObjectiveExtensionDataView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttachmentGuid</Fld></li>
    /// <li><Fld>AttachmentValue</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptObjectiveId</Fld></li>
    /// <li><Fld>BoolValue</Fld></li>
    /// <li><Fld>DateTimeValue</Fld></li>
    /// <li><Fld>DoubleValue</Fld></li>
    /// <li><Fld>ExtensionDataId</Fld></li>
    /// <li><Fld>IntValue</Fld></li>
    /// <li><Fld>Name</Fld></li>
    /// <li><Fld>StringValue</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptObjectiveExtensionDataView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.AttemptId;
        
        /// <summary>
        /// Name of the ExtensionDataId column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ExtensionDataItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ExtensionDataId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.ExtensionDataId;
        
        /// <summary>
        /// Name of the AttemptObjectiveId column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptObjectiveItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptObjectiveId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.AttemptObjectiveId;
        
        /// <summary>
        /// Name of the Name column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Name = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.Name;
        
        /// <summary>
        /// Name of the StringValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StringValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.StringValue;
        
        /// <summary>
        /// Name of the IntValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IntValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.IntValue;
        
        /// <summary>
        /// Name of the BoolValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string BoolValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.BoolValue;
        
        /// <summary>
        /// Name of the DoubleValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DoubleValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.DoubleValue;
        
        /// <summary>
        /// Name of the DateTimeValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DateTimeValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.DateTimeValue;
        
        /// <summary>
        /// Name of the AttachmentGuid column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentGuid = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.AttachmentGuid;
        
        /// <summary>
        /// Name of the AttachmentValue column on the <Typ>SeqNavAttemptObjectiveExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: ByteArray[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptObjectiveExtensionDataView.AttachmentValue;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptInteractionExtensionDataView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttachmentGuid</Fld></li>
    /// <li><Fld>AttachmentValue</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>BoolValue</Fld></li>
    /// <li><Fld>DateTimeValue</Fld></li>
    /// <li><Fld>DoubleValue</Fld></li>
    /// <li><Fld>ExtensionDataId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>IntValue</Fld></li>
    /// <li><Fld>Name</Fld></li>
    /// <li><Fld>StringValue</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptInteractionExtensionDataView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.AttemptId;
        
        /// <summary>
        /// Name of the ExtensionDataId column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ExtensionDataItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ExtensionDataId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.ExtensionDataId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.InteractionId;
        
        /// <summary>
        /// Name of the Name column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Name = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.Name;
        
        /// <summary>
        /// Name of the StringValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string StringValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.StringValue;
        
        /// <summary>
        /// Name of the IntValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IntValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.IntValue;
        
        /// <summary>
        /// Name of the BoolValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string BoolValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.BoolValue;
        
        /// <summary>
        /// Name of the DoubleValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DoubleValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.DoubleValue;
        
        /// <summary>
        /// Name of the DateTimeValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string DateTimeValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.DateTimeValue;
        
        /// <summary>
        /// Name of the AttachmentGuid column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentGuid = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.AttachmentGuid;
        
        /// <summary>
        /// Name of the AttachmentValue column on the <Typ>SeqNavAttemptInteractionExtensionDataView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: ByteArray[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttachmentValue = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptInteractionExtensionDataView.AttachmentValue;
    }
    
    /// <summary>
    /// Contains constants related to the SeqNavAttemptRubricView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>InteractionId</Fld></li>
    /// <li><Fld>IsSatisfied</Fld></li>
    /// <li><Fld>Ordinal</Fld></li>
    /// <li><Fld>Points</Fld></li>
    /// <li><Fld>RubricItemId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class SeqNavAttemptRubricView {
        
        /// <summary>
        /// Name of the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        public const string ViewName = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.ViewName;
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.AttemptId;
        
        /// <summary>
        /// Name of the RubricItemId column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>RubricItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RubricItemId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.RubricItemId;
        
        /// <summary>
        /// Name of the InteractionId column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InteractionItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InteractionId = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.InteractionId;
        
        /// <summary>
        /// Name of the Ordinal column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Ordinal = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.Ordinal;
        
        /// <summary>
        /// Name of the IsSatisfied column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsSatisfied = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.IsSatisfied;
        
        /// <summary>
        /// Name of the Points column on the <Typ>SeqNavAttemptRubricView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Points = Microsoft.LearningComponents.Storage.BaseSchema.SeqNavAttemptRubricView.Points;
    }
    
    /// <summary>
    /// Contains constants related to the LearnerAssignmentView view.
    /// <para>
    /// This <a href="SlkSchema.htm">LearningStore view</a> contains specific information
    /// about all <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
    /// By default, access is granted to no users; consider using
    /// <a href="SlkSchema.htm">other views</a>.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to no users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AttemptGradedPoints</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// <li><Fld>LearnerAssignmentId</Fld></li>
    /// <li><Fld>LearnerAssignmentState</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerAssignmentView {
        
        /// <summary>
        /// Name of the <Typ>LearnerAssignmentView</Typ> view.
        /// </summary>
        public const string ViewName = "LearnerAssignmentView";
        
        /// <summary>
        /// Name of the LearnerAssignmentId column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// LearnerAssignmentId corresponds to <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Id.Field.htm">LearnerAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentId = "LearnerAssignmentId";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// Holds the value of the GuidId column of the LearnerAssignmentItem
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// LearnerId corresponds to <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.LearnerId.Field.htm">LearnerAssignmentItem.LearnerId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = "LearnerId";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// AssignmentAutoReturn corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";
        
        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// RootActivityId corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// AttemptId corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.Id.Field.htm">AttemptItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = "AttemptId";
        
        /// <summary>
        /// Name of the AttemptGradedPoints column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// AttemptGradedPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptGradedPoints = "AttemptGradedPoints";
        
        /// <summary>
        /// Name of the LearnerAssignmentState column on the <Typ>LearnerAssignmentView</Typ> view.
        /// <para>
        /// LearnerAssignmentState is the state of this <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.SharePointLearningKit.LearnerAssignmentState</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentState = "LearnerAssignmentState";
    }
    
    /// <summary>
    /// Contains constants related to the AssignmentPropertiesView view.
    /// <para>
    /// This <a href="SlkSchema.htm">LearningStore view</a> contains information about one
    /// <a href="SlkConcepts.htm#Assignments">SLK assignment</a>, as specified by the
    /// "AssignmentId" view parameter (the
    /// <a href="Microsoft.SharePointLearningKit.AssignmentItemIdentifier.Class.htm">AssignmentItemIdentifier</a> of an assignment) and the "IsInstructor" view parameter (<b>true</b> if the
    /// current user is an instructor on the assignment, <b>false</b> if they're a
    /// learner).
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to instructors and learners of the assignment.
    /// Learners have no access before the start date of the assignment.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDateCreated</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// <ul>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>IsInstructor</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AssignmentPropertiesView {
        
        /// <summary>
        /// Name of the <Typ>AssignmentPropertiesView</Typ> view.
        /// </summary>
        public const string ViewName = "AssignmentPropertiesView";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";

        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the AssignmentDateCreated column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// AssignmentDateCreated holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DateCreated.Field.htm">AssignmentItem.DateCreated</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDateCreated = "AssignmentDateCreated";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// RootActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>AssignmentPropertiesView</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the AssignmentId parameter on the <Typ>AssignmentPropertiesView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the IsInstructor parameter on the <Typ>AssignmentPropertiesView</Typ> view.
        /// </summary>
        /// <remarks>
        /// Parameter type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsInstructor = "IsInstructor";
    }
    
    /// <summary>
    /// Contains constants related to the AssignmentListForInstructors view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one
    /// <a href="SlkConcepts.htm#Assignments">assignment</a>, as well as
    /// information about the e-learning package (if any) associated with the assignment.
    /// This view contains one row for each assignment for which the current user is an
    /// instructor.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDateCreated</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>AvgFinalPoints</Fld></li>
    /// <li><Fld>AvgGradedPoints</Fld></li>
    /// <li><Fld>CountActive</Fld></li>
    /// <li><Fld>CountCompleted</Fld></li>
    /// <li><Fld>CountCompletedOrFinal</Fld></li>
    /// <li><Fld>CountFinal</Fld></li>
    /// <li><Fld>CountNotFinal</Fld></li>
    /// <li><Fld>CountNotStarted</Fld></li>
    /// <li><Fld>CountNotStartedOrActive</Fld></li>
    /// <li><Fld>CountStarted</Fld></li>
    /// <li><Fld>CountTotal</Fld></li>
    /// <li><Fld>MaxFinalPoints</Fld></li>
    /// <li><Fld>MaxGradedPoints</Fld></li>
    /// <li><Fld>MinFinalPoints</Fld></li>
    /// <li><Fld>MinGradedPoints</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AssignmentListForInstructors {
        
        /// <summary>
        /// Name of the <Typ>AssignmentListForInstructors</Typ> view.
        /// </summary>
        public const string ViewName = "AssignmentListForInstructors";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";
        
        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the AssignmentDateCreated column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDateCreated holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DateCreated.Field.htm">AssignmentItem.DateCreated</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDateCreated = "AssignmentDateCreated";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// RootActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the CountTotal column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountTotal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountTotal = "CountTotal";
        
        /// <summary>
        /// Name of the CountNotStarted column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountNotStarted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotStarted = "CountNotStarted";
        
        /// <summary>
        /// Name of the CountActive column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountActive is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Active</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountActive = "CountActive";
        
        /// <summary>
        /// Name of the CountCompleted column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountCompleted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Completed</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountCompleted = "CountCompleted";
        
        /// <summary>
        /// Name of the CountFinal column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountFinal = "CountFinal";
        
        /// <summary>
        /// Name of the CountStarted column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountStarted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are not in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountStarted = "CountStarted";
        
        /// <summary>
        /// Name of the CountNotStartedOrActive column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountNotStartedOrActive is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> or
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Active</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotStartedOrActive = "CountNotStartedOrActive";
        
        /// <summary>
        /// Name of the CountCompletedOrFinal column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountCompletedOrFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Completed</a> or
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountCompletedOrFinal = "CountCompletedOrFinal";
        
        /// <summary>
        /// Name of the CountNotFinal column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// CountNotFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are not in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotFinal = "CountNotFinal";
        
        /// <summary>
        /// Name of the MinGradedPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// MinGradedPoints is the minimum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinGradedPoints = "MinGradedPoints";
        
        /// <summary>
        /// Name of the MaxGradedPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// MaxGradedPoints is the maximum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxGradedPoints = "MaxGradedPoints";
        
        /// <summary>
        /// Name of the AvgGradedPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AvgGradedPoints is the average value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AvgGradedPoints = "AvgGradedPoints";
        
        /// <summary>
        /// Name of the MinFinalPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// MinFinalPoints is the minimum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinFinalPoints = "MinFinalPoints";
        
        /// <summary>
        /// Name of the MaxFinalPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// MaxFinalPoints is the maximum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxFinalPoints = "MaxFinalPoints";
        
        /// <summary>
        /// Name of the AvgFinalPoints column on the <Typ>AssignmentListForInstructors</Typ> view.
        /// <para>
        /// AvgFinalPoints is the average value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AvgFinalPoints = "AvgFinalPoints";
    }
    
    /// <summary>
    /// Contains constants related to the InstructorAssignmentListForInstructors view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">
    /// instructor
    /// assignment
    /// </a>, as well as information about the e-learning package (if any)
    /// associated with the assignment.  This view returns one row for each instructor on
    /// each assignment for which the current user is an instructor.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>InstructorAssignmentId</Fld></li>
    /// <li><Fld>InstructorId</Fld></li>
    /// <li><Fld>InstructorKey</Fld></li>
    /// <li><Fld>InstructorName</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>PackageManifest</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class InstructorAssignmentListForInstructors {
        
        /// <summary>
        /// Name of the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// </summary>
        public const string ViewName = "InstructorAssignmentListForInstructors";
        
        /// <summary>
        /// Name of the InstructorAssignmentId column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// InstructorAssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Id.Field.htm">InstructorAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InstructorAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorAssignmentId = "InstructorAssignmentId";
        
        /// <summary>
        /// Name of the InstructorId column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// InstructorId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.InstructorId.Field.htm">InstructorAssignmentItem.InstructorId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorId = "InstructorId";
        
        /// <summary>
        /// Name of the InstructorName column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// InstructorName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the instructor.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorName = "InstructorName";
        
        /// <summary>
        /// Name of the InstructorKey column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// InstructorKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the instructor.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorKey = "InstructorKey";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";
        
        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the PackageManifest column on the <Typ>InstructorAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Manifest.Field.htm">PackageItem.Manifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageManifest = "PackageManifest";
    }
    
    /// <summary>
    /// Contains constants related to the InstructorAssignmentList view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">
    /// instructor
    /// assignment
    /// </a>, as well as information about the e-learning package (if any)
    /// associated with the assignment.  This view returns one row for each instructor on
    /// each assignment for which the current user is an instructor.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>InstructorAssignmentId</Fld></li>
    /// <li><Fld>InstructorId</Fld></li>
    /// <li><Fld>InstructorKey</Fld></li>
    /// <li><Fld>InstructorName</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>PackageManifest</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class InstructorAssignmentList {
        
        /// <summary>
        /// Name of the <Typ>InstructorAssignmentList</Typ> view.
        /// </summary>
        public const string ViewName = "InstructorAssignmentList";
        
        /// <summary>
        /// Name of the InstructorAssignmentId column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// InstructorAssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.Id.Field.htm">InstructorAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>InstructorAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorAssignmentId = "InstructorAssignmentId";
        
        /// <summary>
        /// Name of the InstructorId column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// InstructorId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.InstructorId.Field.htm">InstructorAssignmentItem.InstructorId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorId = "InstructorId";
        
        /// <summary>
        /// Name of the InstructorName column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// InstructorName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the instructor.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorName = "InstructorName";
        
        /// <summary>
        /// Name of the InstructorKey column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// InstructorKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the instructor.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorKey = "InstructorKey";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";

        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the PackageManifest column on the <Typ>InstructorAssignmentList</Typ> view.
        /// <para>
        /// PackageManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Manifest.Field.htm">PackageItem.Manifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageManifest = "PackageManifest";
    }
    
    /// <summary>
    /// Contains constants related to the LearnerAssignmentListForLearners view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">learner assignment</a>,
    /// as well as information about the e-learning package (if any) associated with the
    /// assignment.  This view returns one row for each assignment for which the current
    /// user is a learner, excluding assignments which have not yet started.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDateCreated</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>AttemptCompletionStatus</Fld></li>
    /// <li><Fld>AttemptCurrentActivityId</Fld></li>
    /// <li><Fld>AttemptFinishedTimestamp</Fld></li>
    /// <li><Fld>AttemptGradedPoints</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptLogDetailSequencing</Fld></li>
    /// <li><Fld>AttemptLogFinalSequencing</Fld></li>
    /// <li><Fld>AttemptLogRollup</Fld></li>
    /// <li><Fld>AttemptStartedTimestamp</Fld></li>
    /// <li><Fld>AttemptStatus</Fld></li>
    /// <li><Fld>AttemptSuccessStatus</Fld></li>
    /// <li><Fld>AttemptSuspendedActivityId</Fld></li>
    /// <li><Fld>FileSubmissionState</Fld></li>
    /// <li><Fld>FinalPoints</Fld></li>
    /// <li><Fld>Grade</Fld></li>
    /// <li><Fld>HasInstructors</Fld></li>
    /// <li><Fld>InstructorComments</Fld></li>
    /// <li><Fld>IsFinal</Fld></li>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// <li><Fld>LearnerAssignmentId</Fld></li>
    /// <li><Fld>LearnerAssignmentState</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>LearnerKey</Fld></li>
    /// <li><Fld>LearnerName</Fld></li>
    /// <li><Fld>NonELearningStatus</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerAssignmentListForLearners {
        
        /// <summary>
        /// Name of the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// </summary>
        public const string ViewName = "LearnerAssignmentListForLearners";
        
        /// <summary>
        /// Name of the LearnerAssignmentId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// LearnerAssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Id.Field.htm">LearnerAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentId = "LearnerAssignmentId";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// Holds the value of the GuidId column of the LearnerAssignmentItem
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// LearnerId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.LearnerId.Field.htm">LearnerAssignmentItem.LearnerId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = "LearnerId";
        
        /// <summary>
        /// Name of the LearnerName column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// LearnerName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerName = "LearnerName";
        
        /// <summary>
        /// Name of the LearnerKey column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// LearnerKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerKey = "LearnerKey";
        
        /// <summary>
        /// Name of the IsFinal column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// IsFinal holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.IsFinal.Field.htm">LearnerAssignmentItem.IsFinal</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsFinal = "IsFinal";
        
        /// <summary>
        /// Name of the NonELearningStatus column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// NonELearningStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.NonELearningStatus.Field.htm">LearnerAssignmentItem.NonELearningStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NonELearningStatus = "NonELearningStatus";
        
        /// <summary>
        /// Name of the FinalPoints column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// FinalPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinalPoints = "FinalPoints";
        
        /// <summary>
        /// Name of the Grade column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// Grade holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Grade.Field.htm">LearnerAssignmentItem.Grade</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[20]
        /// </remarks>
        public const string Grade = "Grade";
        
        /// <summary>
        /// Name of the InstructorComments column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// InstructorComments holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.InstructorComments.Field.htm">LearnerAssignmentItem.InstructorComments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorComments = "InstructorComments";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";
        
        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the AssignmentDateCreated column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AssignmentDateCreated holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DateCreated.Field.htm">AssignmentItem.DateCreated</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDateCreated = "AssignmentDateCreated";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// RootActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.Id.Field.htm">AttemptItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = "AttemptId";
        
        /// <summary>
        /// Name of the AttemptCurrentActivityId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptCurrentActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CurrentActivityId.Field.htm">AttemptItem.CurrentActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCurrentActivityId = "AttemptCurrentActivityId";
        
        /// <summary>
        /// Name of the AttemptSuspendedActivityId column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptSuspendedActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuspendedActivityId.Field.htm">AttemptItem.SuspendedActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuspendedActivityId = "AttemptSuspendedActivityId";
        
        /// <summary>
        /// Name of the AttemptStatus column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.AttemptStatus.Field.htm">AttemptItem.AttemptStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStatus = "AttemptStatus";
        
        /// <summary>
        /// Name of the AttemptFinishedTimestamp column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptFinishedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.FinishedTimestamp.Field.htm">AttemptItem.FinishedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptFinishedTimestamp = "AttemptFinishedTimestamp";
        
        /// <summary>
        /// Name of the AttemptLogDetailSequencing column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptLogDetailSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogDetailSequencing.Field.htm">AttemptItem.LogDetailSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogDetailSequencing = "AttemptLogDetailSequencing";
        
        /// <summary>
        /// Name of the AttemptLogFinalSequencing column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptLogFinalSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogFinalSequencing.Field.htm">AttemptItem.LogFinalSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogFinalSequencing = "AttemptLogFinalSequencing";
        
        /// <summary>
        /// Name of the AttemptLogRollup column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptLogRollup holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogRollup.Field.htm">AttemptItem.LogRollup</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogRollup = "AttemptLogRollup";
        
        /// <summary>
        /// Name of the AttemptStartedTimestamp column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptStartedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.StartedTimestamp.Field.htm">AttemptItem.StartedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStartedTimestamp = "AttemptStartedTimestamp";
        
        /// <summary>
        /// Name of the AttemptCompletionStatus column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptCompletionStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CompletionStatus.Field.htm">AttemptItem.CompletionStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCompletionStatus = "AttemptCompletionStatus";
        
        /// <summary>
        /// Name of the AttemptSuccessStatus column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptSuccessStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuccessStatus.Field.htm">AttemptItem.SuccessStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuccessStatus = "AttemptSuccessStatus";
        
        /// <summary>
        /// Name of the AttemptGradedPoints column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// AttemptGradedPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptGradedPoints = "AttemptGradedPoints";
        
        /// <summary>
        /// Name of the LearnerAssignmentState column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// LearnerAssignmentState is the state of this <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.SharePointLearningKit.LearnerAssignmentState</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentState = "LearnerAssignmentState";
        
        /// <summary>
        /// Name of the HasInstructors column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// HasInstructors is <b>true</b> if the assignment has instructors, <b>false</b> if not.  Note that self-assigned assignments have no instructors.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HasInstructors = "HasInstructors";
        
        /// <summary>
        /// Name of the FileSubmissionState column on the <Typ>LearnerAssignmentListForLearners</Typ> view.
        /// <para>
        /// FileSubmissionState is set to <b>NA</b> if the assignment is e-learning content. In case the assignment is non e-learning
        /// content, FileSubmissionState has one of three values: <b>Submit File(s)</b> in case the assignment is active or not started,
        /// <b>Submitted LINK</b> in case the assignment is final, and <b>Submitted</b> otherwise (in case the assignment is completed).
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FileSubmissionState = "FileSubmissionState";
    }
    
    /// <summary>
    /// Contains constants related to the LearnerAssignmentListForObservers view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">learner assignment</a>,
    /// as well as information about the e-learning package (if any) associated with the
    /// assignment. It also holds aggregated information about each assignment.
    /// This view returns one row for each assignment for which the input
    /// user is a learner, including assignments which have not yet started.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDateCreated</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>AttemptCompletionStatus</Fld></li>
    /// <li><Fld>AttemptCurrentActivityId</Fld></li>
    /// <li><Fld>AttemptFinishedTimestamp</Fld></li>
    /// <li><Fld>AttemptGradedPoints</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptLogDetailSequencing</Fld></li>
    /// <li><Fld>AttemptLogFinalSequencing</Fld></li>
    /// <li><Fld>AttemptLogRollup</Fld></li>
    /// <li><Fld>AttemptStartedTimestamp</Fld></li>
    /// <li><Fld>AttemptStatus</Fld></li>
    /// <li><Fld>AttemptSuccessStatus</Fld></li>
    /// <li><Fld>AttemptSuspendedActivityId</Fld></li>
    /// <li><Fld>AvgFinalPoints</Fld></li>
    /// <li><Fld>AvgGradedPoints</Fld></li>
    /// <li><Fld>CountActive</Fld></li>
    /// <li><Fld>CountCompleted</Fld></li>
    /// <li><Fld>CountCompletedOrFinal</Fld></li>
    /// <li><Fld>CountFinal</Fld></li>
    /// <li><Fld>CountNotFinal</Fld></li>
    /// <li><Fld>CountNotStarted</Fld></li>
    /// <li><Fld>CountNotStartedOrActive</Fld></li>
    /// <li><Fld>CountStarted</Fld></li>
    /// <li><Fld>CountTotal</Fld></li>
    /// <li><Fld>FileSubmissionState</Fld></li>
    /// <li><Fld>FinalPoints</Fld></li>
    /// <li><Fld>Grade</Fld></li>
    /// <li><Fld>HasInstructors</Fld></li>
    /// <li><Fld>InstructorComments</Fld></li>
    /// <li><Fld>IsFinal</Fld></li>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// <li><Fld>LearnerAssignmentId</Fld></li>
    /// <li><Fld>LearnerAssignmentState</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>LearnerKey</Fld></li>
    /// <li><Fld>LearnerName</Fld></li>
    /// <li><Fld>MaxFinalPoints</Fld></li>
    /// <li><Fld>MaxGradedPoints</Fld></li>
    /// <li><Fld>MinFinalPoints</Fld></li>
    /// <li><Fld>MinGradedPoints</Fld></li>
    /// <li><Fld>NonELearningStatus</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerAssignmentListForObservers {
        
        /// <summary>
        /// Name of the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// </summary>
        public const string ViewName = "LearnerAssignmentListForObservers";
        
        /// <summary>
        /// Name of the LearnerAssignmentId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// LearnerAssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Id.Field.htm">LearnerAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentId = "LearnerAssignmentId";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// Holds the value of the GuidId column of the LearnerAssignmentItem
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// LearnerId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.LearnerId.Field.htm">LearnerAssignmentItem.LearnerId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = "LearnerId";
        
        /// <summary>
        /// Name of the LearnerName column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// LearnerName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerName = "LearnerName";
        
        /// <summary>
        /// Name of the LearnerKey column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// LearnerKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerKey = "LearnerKey";
        
        /// <summary>
        /// Name of the IsFinal column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// IsFinal holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.IsFinal.Field.htm">LearnerAssignmentItem.IsFinal</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsFinal = "IsFinal";
        
        /// <summary>
        /// Name of the NonELearningStatus column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// NonELearningStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.NonELearningStatus.Field.htm">LearnerAssignmentItem.NonELearningStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NonELearningStatus = "NonELearningStatus";
        
        /// <summary>
        /// Name of the FinalPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// FinalPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinalPoints = "FinalPoints";
        
        /// <summary>
        /// Name of the Grade column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// Grade holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Grade.Field.htm">LearnerAssignmentItem.Grade</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[20]
        /// </remarks>
        public const string Grade = "Grade";
        
        /// <summary>
        /// Name of the InstructorComments column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// InstructorComments holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.InstructorComments.Field.htm">LearnerAssignmentItem.InstructorComments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorComments = "InstructorComments";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";

        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the AssignmentDateCreated column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AssignmentDateCreated holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DateCreated.Field.htm">AssignmentItem.DateCreated</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDateCreated = "AssignmentDateCreated";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// RootActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the CountTotal column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountTotal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountTotal = "CountTotal";
        
        /// <summary>
        /// Name of the CountNotStarted column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountNotStarted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotStarted = "CountNotStarted";
        
        /// <summary>
        /// Name of the CountActive column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountActive is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Active</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountActive = "CountActive";
        
        /// <summary>
        /// Name of the CountCompleted column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountCompleted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Completed</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountCompleted = "CountCompleted";
        
        /// <summary>
        /// Name of the CountFinal column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountFinal = "CountFinal";
        
        /// <summary>
        /// Name of the CountStarted column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountStarted is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are not in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountStarted = "CountStarted";
        
        /// <summary>
        /// Name of the CountNotStartedOrActive column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountNotStartedOrActive is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.NotStarted</a> or
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Active</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotStartedOrActive = "CountNotStartedOrActive";
        
        /// <summary>
        /// Name of the CountCompletedOrFinal column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountCompletedOrFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Completed</a> or
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountCompletedOrFinal = "CountCompletedOrFinal";
        
        /// <summary>
        /// Name of the CountNotFinal column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// CountNotFinal is the number of <a href="SlkConcepts.htm#Assignments">learner assignments</a> that are not in the
        /// <a href="Microsoft.SharePointLearningKit.LearnerAssignmentState.Enumeration.htm">LearnerAssignmentState.Final</a> state.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CountNotFinal = "CountNotFinal";
        
        /// <summary>
        /// Name of the MinGradedPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// MinGradedPoints is the minimum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinGradedPoints = "MinGradedPoints";
        
        /// <summary>
        /// Name of the MaxGradedPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// MaxGradedPoints is the maximum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxGradedPoints = "MaxGradedPoints";
        
        /// <summary>
        /// Name of the AvgGradedPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AvgGradedPoints is the average value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AvgGradedPoints = "AvgGradedPoints";
        
        /// <summary>
        /// Name of the MinFinalPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// MinFinalPoints is the minimum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MinFinalPoints = "MinFinalPoints";
        
        /// <summary>
        /// Name of the MaxFinalPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// MaxFinalPoints is the maximum value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxFinalPoints = "MaxFinalPoints";
        
        /// <summary>
        /// Name of the AvgFinalPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AvgFinalPoints is the average value of
        /// <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>
        /// among the <a href="SlkConcepts.htm#Assignments">learner assignments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AvgFinalPoints = "AvgFinalPoints";
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.Id.Field.htm">AttemptItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = "AttemptId";
        
        /// <summary>
        /// Name of the AttemptCurrentActivityId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptCurrentActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CurrentActivityId.Field.htm">AttemptItem.CurrentActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCurrentActivityId = "AttemptCurrentActivityId";
        
        /// <summary>
        /// Name of the AttemptSuspendedActivityId column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptSuspendedActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuspendedActivityId.Field.htm">AttemptItem.SuspendedActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuspendedActivityId = "AttemptSuspendedActivityId";
        
        /// <summary>
        /// Name of the AttemptStatus column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.AttemptStatus.Field.htm">AttemptItem.AttemptStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStatus = "AttemptStatus";
        
        /// <summary>
        /// Name of the AttemptFinishedTimestamp column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptFinishedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.FinishedTimestamp.Field.htm">AttemptItem.FinishedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptFinishedTimestamp = "AttemptFinishedTimestamp";
        
        /// <summary>
        /// Name of the AttemptLogDetailSequencing column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptLogDetailSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogDetailSequencing.Field.htm">AttemptItem.LogDetailSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogDetailSequencing = "AttemptLogDetailSequencing";
        
        /// <summary>
        /// Name of the AttemptLogFinalSequencing column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptLogFinalSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogFinalSequencing.Field.htm">AttemptItem.LogFinalSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogFinalSequencing = "AttemptLogFinalSequencing";
        
        /// <summary>
        /// Name of the AttemptLogRollup column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptLogRollup holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogRollup.Field.htm">AttemptItem.LogRollup</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogRollup = "AttemptLogRollup";
        
        /// <summary>
        /// Name of the AttemptStartedTimestamp column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptStartedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.StartedTimestamp.Field.htm">AttemptItem.StartedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStartedTimestamp = "AttemptStartedTimestamp";
        
        /// <summary>
        /// Name of the AttemptCompletionStatus column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptCompletionStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CompletionStatus.Field.htm">AttemptItem.CompletionStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCompletionStatus = "AttemptCompletionStatus";
        
        /// <summary>
        /// Name of the AttemptSuccessStatus column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptSuccessStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuccessStatus.Field.htm">AttemptItem.SuccessStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuccessStatus = "AttemptSuccessStatus";
        
        /// <summary>
        /// Name of the AttemptGradedPoints column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// AttemptGradedPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptGradedPoints = "AttemptGradedPoints";
        
        /// <summary>
        /// Name of the LearnerAssignmentState column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// LearnerAssignmentState is the state of this <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.SharePointLearningKit.LearnerAssignmentState</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentState = "LearnerAssignmentState";
        
        /// <summary>
        /// Name of the HasInstructors column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// HasInstructors is <b>true</b> if the assignment has instructors, <b>false</b> if not.  Note that self-assigned assignments have no instructors.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HasInstructors = "HasInstructors";
        
        /// <summary>
        /// Name of the FileSubmissionState column on the <Typ>LearnerAssignmentListForObservers</Typ> view.
        /// <para>
        /// FileSubmissionState is set to <b>NA</b> if the assignment is e-learning content. In case the assignment is non e-learning
        /// content, FileSubmissionState has one of three values: <b>Not Submitted</b> in case the assignment is active or not started,
        /// <b>Submitted LINK</b> in case the assignment is final, and <b>Submitted</b> otherwise (in case the assignment is completed).
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FileSubmissionState = "FileSubmissionState";
    }
    
    /// <summary>
    /// Contains constants related to the LearnerAssignmentListForInstructors view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> contains
    /// information about one <a href="SlkConcepts.htm#Assignments">learner assignment</a>,
    /// as well as information about the e-learning package (if any) associated with the
    /// assignment.  This view returns one row for each learner on each assignment for
    /// which the current user is an instructor.
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>AssignmentAutoReturn</Fld></li>
    /// <li><Fld>AssignmentEmailChanges</Fld></li>
    /// <li><Fld>AssignmentCreatedById</Fld></li>
    /// <li><Fld>AssignmentCreatedByKey</Fld></li>
    /// <li><Fld>AssignmentCreatedByName</Fld></li>
    /// <li><Fld>AssignmentDescription</Fld></li>
    /// <li><Fld>AssignmentDueDate</Fld></li>
    /// <li><Fld>AssignmentId</Fld></li>
    /// <li><Fld>AssignmentNonELearningLocation</Fld></li>
    /// <li><Fld>AssignmentPointsPossible</Fld></li>
    /// <li><Fld>AssignmentShowAnswersToLearners</Fld></li>
    /// <li><Fld>AssignmentSPSiteGuid</Fld></li>
    /// <li><Fld>AssignmentSPWebGuid</Fld></li>
    /// <li><Fld>AssignmentStartDate</Fld></li>
    /// <li><Fld>AssignmentTitle</Fld></li>
    /// <li><Fld>AttemptCompletionStatus</Fld></li>
    /// <li><Fld>AttemptCurrentActivityId</Fld></li>
    /// <li><Fld>AttemptFinishedTimestamp</Fld></li>
    /// <li><Fld>AttemptGradedPoints</Fld></li>
    /// <li><Fld>AttemptId</Fld></li>
    /// <li><Fld>AttemptLogDetailSequencing</Fld></li>
    /// <li><Fld>AttemptLogFinalSequencing</Fld></li>
    /// <li><Fld>AttemptLogRollup</Fld></li>
    /// <li><Fld>AttemptStartedTimestamp</Fld></li>
    /// <li><Fld>AttemptStatus</Fld></li>
    /// <li><Fld>AttemptSuccessStatus</Fld></li>
    /// <li><Fld>AttemptSuspendedActivityId</Fld></li>
    /// <li><Fld>FinalPoints</Fld></li>
    /// <li><Fld>Grade</Fld></li>
    /// <li><Fld>HasInstructors</Fld></li>
    /// <li><Fld>InstructorComments</Fld></li>
    /// <li><Fld>IsFinal</Fld></li>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// <li><Fld>LearnerAssignmentId</Fld></li>
    /// <li><Fld>LearnerAssignmentState</Fld></li>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>LearnerKey</Fld></li>
    /// <li><Fld>LearnerName</Fld></li>
    /// <li><Fld>NonELearningStatus</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>PackageManifest</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class LearnerAssignmentListForInstructors {
        
        /// <summary>
        /// Name of the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// </summary>
        public const string ViewName = "LearnerAssignmentListForInstructors";
        
        /// <summary>
        /// Name of the LearnerAssignmentId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// LearnerAssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Id.Field.htm">LearnerAssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>LearnerAssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentId = "LearnerAssignmentId";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// Holds the value of the GuidId column of the LearnerAssignmentItem
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
        
        /// <summary>
        /// Name of the LearnerId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// LearnerId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.LearnerId.Field.htm">LearnerAssignmentItem.LearnerId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = "LearnerId";
        
        /// <summary>
        /// Name of the LearnerName column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// LearnerName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerName = "LearnerName";
        
        /// <summary>
        /// Name of the LearnerKey column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// LearnerKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the learner.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerKey = "LearnerKey";
        
        /// <summary>
        /// Name of the IsFinal column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// IsFinal holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.IsFinal.Field.htm">LearnerAssignmentItem.IsFinal</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsFinal = "IsFinal";
        
        /// <summary>
        /// Name of the NonELearningStatus column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// NonELearningStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.NonELearningStatus.Field.htm">LearnerAssignmentItem.NonELearningStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string NonELearningStatus = "NonELearningStatus";
        
        /// <summary>
        /// Name of the FinalPoints column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// FinalPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.FinalPoints.Field.htm">LearnerAssignmentItem.FinalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string FinalPoints = "FinalPoints";
        
        /// <summary>
        /// Name of the Grade column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// Grade holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.Grade.Field.htm">LearnerAssignmentItem.Grade</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[20]
        /// </remarks>
        public const string Grade = "Grade";
        
        /// <summary>
        /// Name of the InstructorComments column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// InstructorComments holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.InstructorComments.Field.htm">LearnerAssignmentItem.InstructorComments</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string InstructorComments = "InstructorComments";
        
        /// <summary>
        /// Name of the AssignmentId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Id.Field.htm">AssignmentItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AssignmentItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentId = "AssignmentId";
        
        /// <summary>
        /// Name of the AssignmentSPSiteGuid column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPSiteGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPSiteGuid.Field.htm">AssignmentItem.SPSiteGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPSiteGuid = "AssignmentSPSiteGuid";
        
        /// <summary>
        /// Name of the AssignmentSPWebGuid column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentSPWebGuid holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.SPWebGuid.Field.htm">AssignmentItem.SPWebGuid</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentSPWebGuid = "AssignmentSPWebGuid";
        
        /// <summary>
        /// Name of the AssignmentNonELearningLocation column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentNonELearningLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.NonELearningLocation.Field.htm">AssignmentItem.NonELearningLocation</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentNonELearningLocation = "AssignmentNonELearningLocation";
        
        /// <summary>
        /// Name of the AssignmentTitle column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentTitle holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Title.Field.htm">AssignmentItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentTitle = "AssignmentTitle";
        
        /// <summary>
        /// Name of the AssignmentStartDate column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentStartDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.StartDate.Field.htm">AssignmentItem.StartDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentStartDate = "AssignmentStartDate";
        
        /// <summary>
        /// Name of the AssignmentDueDate column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDueDate holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.DueDate.Field.htm">AssignmentItem.DueDate</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDueDate = "AssignmentDueDate";
        
        /// <summary>
        /// Name of the AssignmentPointsPossible column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentPointsPossible holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.PointsPossible.Field.htm">AssignmentItem.PointsPossible</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentPointsPossible = "AssignmentPointsPossible";
        
        /// <summary>
        /// Name of the AssignmentDescription column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentDescription holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.Description.Field.htm">AssignmentItem.Description</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentDescription = "AssignmentDescription";
        
        /// <summary>
        /// Name of the AssignmentAutoReturn column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentAutoReturn holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.AutoReturn.Field.htm">AssignmentItem.AutoReturn</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentAutoReturn = "AssignmentAutoReturn";
        
        /// <summary>
        /// Name of the AssignmentEmailChanges column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentEmailChanges corresponds to <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.EmailChanges.Field.htm">AssignmentItem.EmailChanges</a>.
        /// </para>
        /// </summary>
        /// <remarks>Column type: Boolean</remarks>
        public const string AssignmentEmailChanges = "AssignmentEmailChanges";
        
        /// <summary>
        /// Name of the AssignmentShowAnswersToLearners column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentShowAnswersToLearners holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.ShowAnswersToLearners.Field.htm">AssignmentItem.ShowAnswersToLearners</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentShowAnswersToLearners = "AssignmentShowAnswersToLearners";
        
        /// <summary>
        /// Name of the AssignmentCreatedById column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedById holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.CreatedBy.Field.htm">AssignmentItem.CreatedBy</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedById = "AssignmentCreatedById";
        
        /// <summary>
        /// Name of the AssignmentCreatedByName column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByName holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Name.Field.htm">UserItem.Name</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByName = "AssignmentCreatedByName";
        
        /// <summary>
        /// Name of the AssignmentCreatedByKey column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AssignmentCreatedByKey holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.UserItem.Key.Field.htm">UserItem.Key</a>.  Refers to the user who created the assignment.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AssignmentCreatedByKey = "AssignmentCreatedByKey";
        
        /// <summary>
        /// Name of the RootActivityId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// RootActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AssignmentItem.RootActivityId.Field.htm">AssignmentItem.RootActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = "RootActivityId";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the PackageManifest column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// PackageManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Manifest.Field.htm">PackageItem.Manifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageManifest = "PackageManifest";
        
        /// <summary>
        /// Name of the AttemptId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.Id.Field.htm">AttemptItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = "AttemptId";
        
        /// <summary>
        /// Name of the AttemptCurrentActivityId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptCurrentActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CurrentActivityId.Field.htm">AttemptItem.CurrentActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCurrentActivityId = "AttemptCurrentActivityId";
        
        /// <summary>
        /// Name of the AttemptSuspendedActivityId column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptSuspendedActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuspendedActivityId.Field.htm">AttemptItem.SuspendedActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuspendedActivityId = "AttemptSuspendedActivityId";
        
        /// <summary>
        /// Name of the AttemptStatus column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.AttemptStatus.Field.htm">AttemptItem.AttemptStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.AttemptStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStatus = "AttemptStatus";
        
        /// <summary>
        /// Name of the AttemptFinishedTimestamp column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptFinishedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.FinishedTimestamp.Field.htm">AttemptItem.FinishedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptFinishedTimestamp = "AttemptFinishedTimestamp";
        
        /// <summary>
        /// Name of the AttemptLogDetailSequencing column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptLogDetailSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogDetailSequencing.Field.htm">AttemptItem.LogDetailSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogDetailSequencing = "AttemptLogDetailSequencing";
        
        /// <summary>
        /// Name of the AttemptLogFinalSequencing column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptLogFinalSequencing holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogFinalSequencing.Field.htm">AttemptItem.LogFinalSequencing</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogFinalSequencing = "AttemptLogFinalSequencing";
        
        /// <summary>
        /// Name of the AttemptLogRollup column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptLogRollup holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.LogRollup.Field.htm">AttemptItem.LogRollup</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptLogRollup = "AttemptLogRollup";
        
        /// <summary>
        /// Name of the AttemptStartedTimestamp column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptStartedTimestamp holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.StartedTimestamp.Field.htm">AttemptItem.StartedTimestamp</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptStartedTimestamp = "AttemptStartedTimestamp";
        
        /// <summary>
        /// Name of the AttemptCompletionStatus column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptCompletionStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.CompletionStatus.Field.htm">AttemptItem.CompletionStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.CompletionStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptCompletionStatus = "AttemptCompletionStatus";
        
        /// <summary>
        /// Name of the AttemptSuccessStatus column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptSuccessStatus holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.SuccessStatus.Field.htm">AttemptItem.SuccessStatus</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.SuccessStatus</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptSuccessStatus = "AttemptSuccessStatus";
        
        /// <summary>
        /// Name of the AttemptGradedPoints column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// AttemptGradedPoints holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.AttemptItem.TotalPoints.Field.htm">AttemptItem.TotalPoints</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptGradedPoints = "AttemptGradedPoints";
        
        /// <summary>
        /// Name of the LearnerAssignmentState column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// LearnerAssignmentState is the state of this <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.SharePointLearningKit.LearnerAssignmentState</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentState = "LearnerAssignmentState";
        
        /// <summary>
        /// Name of the HasInstructors column on the <Typ>LearnerAssignmentListForInstructors</Typ> view.
        /// <para>
        /// HasInstructors is <b>true</b> if the assignment has instructors, <b>false</b> if not.  Note that self-assigned assignments have no instructors.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string HasInstructors = "HasInstructors";
    }
    
    /// <summary>
    /// Contains constants related to the UserWebList view.
    /// <para>
    /// Each row of this <a href="SlkSchema.htm">LearningStore view</a> represents one
    /// entry in one user's "user Web list", for one site collection.  A user Web list is
    /// the list of Web sites that appears on the SLK E-Learning Actions page for all
    /// document libraries within a given site collection.  Each user has at most one user
    /// Web list for each site collection (SPSite).
    /// </para>
    /// <para>
    /// This view is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>
    /// <a href="SlkSchema.htm">Default operation-level security</a>:
    /// </b>
    /// Access is granted to all users.
    /// </para><p/>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>LastAccessTime</Fld></li>
    /// <li><Fld>SPSiteGuid</Fld></li>
    /// <li><Fld>SPWebGuid</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class UserWebList {
        
        /// <summary>
        /// Name of the <Typ>UserWebList</Typ> view.
        /// </summary>
        public const string ViewName = "UserWebList";
        
        /// <summary>
        /// Name of the SPSiteGuid column on the <Typ>UserWebList</Typ> view.
        /// <para>
        /// SPSiteGuid is the GUID of the SharePoint site collection (SPSite) that this
        /// user Web list item is associated with.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPSiteGuid = "SPSiteGuid";
        
        /// <summary>
        /// Name of the SPWebGuid column on the <Typ>UserWebList</Typ> view.
        /// <para>
        /// SPWebGuid is the GUID of the SharePoint Web site (SPWeb) that this user Web list
        /// item is associated with.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string SPWebGuid = "SPWebGuid";
        
        /// <summary>
        /// Name of the LastAccessTime column on the <Typ>UserWebList</Typ> view.
        /// <para>
        /// LastAccessTime is the date/time (UTC) that this user Web list item was last
        /// accessed via an operation such as assignment creation.  (Viewing the item within
        /// the list on the E-Learning Actions page in SLK doesn't count as an access.)
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: DateTime
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LastAccessTime = "LastAccessTime";
    }
    
    /// <summary>
    /// Contains constants related to the ActivityPackageItemView view.
    /// </summary>
    /// <remarks>
    /// Columns in the view:
    /// <ul>
    /// <li><Fld>ActivityIdFromManifest</Fld></li>
    /// <li><Fld>CompletionThreshold</Fld></li>
    /// <li><Fld>Credit</Fld></li>
    /// <li><Fld>Id</Fld></li>
    /// <li><Fld>IsVisibleInContents</Fld></li>
    /// <li><Fld>LaunchData</Fld></li>
    /// <li><Fld>MaxAttempts</Fld></li>
    /// <li><Fld>MaxTimeAllowed</Fld></li>
    /// <li><Fld>ObjectivesGlobalToSystem</Fld></li>
    /// <li><Fld>OriginalPlacement</Fld></li>
    /// <li><Fld>PackageFormat</Fld></li>
    /// <li><Fld>PackageId</Fld></li>
    /// <li><Fld>PackageLocation</Fld></li>
    /// <li><Fld>PackageManifest</Fld></li>
    /// <li><Fld>ParentActivityId</Fld></li>
    /// <li><Fld>PrimaryObjectiveId</Fld></li>
    /// <li><Fld>PrimaryResourceIdFromManifest</Fld></li>
    /// <li><Fld>ResourceId</Fld></li>
    /// <li><Fld>ResourceParameters</Fld></li>
    /// <li><Fld>ScaledPassingScore</Fld></li>
    /// <li><Fld>TimeLimitAction</Fld></li>
    /// <li><Fld>Title</Fld></li>
    /// </ul>
    /// Parameters in the view:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ActivityPackageItemView {
        
        /// <summary>
        /// Name of the <Typ>ActivityPackageItemView</Typ> view.
        /// </summary>
        public const string ViewName = "ActivityPackageItemView";
        
        /// <summary>
        /// Name of the PackageId column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PackageId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Id.Field.htm">PackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = "PackageId";
        
        /// <summary>
        /// Name of the PackageFormat column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PackageFormat holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.PackageFormat.Field.htm">PackageItem.PackageFormat</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.PackageFormat</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageFormat = "PackageFormat";
        
        /// <summary>
        /// Name of the PackageLocation column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PackageLocation holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Location.Field.htm">PackageItem.Location</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageLocation = "PackageLocation";
        
        /// <summary>
        /// Name of the PackageManifest column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PackageManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.PackageItem.Manifest.Field.htm">PackageItem.Manifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Xml
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageManifest = "PackageManifest";
        
        /// <summary>
        /// Name of the Id column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// Id holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.Id.Field.htm">ActivityPackageItem.Id</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Id = "Id";
        
        /// <summary>
        /// Name of the ActivityIdFromManifest column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ActivityIdFromManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ActivityIdFromManifest.Field.htm">ActivityPackageItem.ActivityIdFromManifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ActivityIdFromManifest = "ActivityIdFromManifest";
        
        /// <summary>
        /// Name of the OriginalPlacement column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// OriginalPlacement holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.OriginalPlacement.Field.htm">ActivityPackageItem.OriginalPlacement</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string OriginalPlacement = "OriginalPlacement";
        
        /// <summary>
        /// Name of the ParentActivityId column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ParentActivityId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ParentActivityId.Field.htm">ActivityPackageItem.ParentActivityId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ParentActivityId = "ParentActivityId";
        
        /// <summary>
        /// Name of the PrimaryObjectiveId column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PrimaryObjectiveId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.PrimaryObjectiveId.Field.htm">ActivityPackageItem.PrimaryObjectiveId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ActivityObjectiveItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PrimaryObjectiveId = "PrimaryObjectiveId";
        
        /// <summary>
        /// Name of the ResourceId column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ResourceId holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ResourceId.Field.htm">ActivityPackageItem.ResourceId</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Reference to a <Typ>ResourceItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResourceId = "ResourceId";
        
        /// <summary>
        /// Name of the PrimaryResourceIdFromManifest column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// PrimaryResourceIdFromManifest holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.PrimaryResourceFromManifest.Field.htm">ActivityPackageItem.PrimaryResourceFromManifest</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PrimaryResourceIdFromManifest = "PrimaryResourceIdFromManifest";
        
        /// <summary>
        /// Name of the CompletionThreshold column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// CompletionThreshold holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.CompletionThreshold.Field.htm">ActivityPackageItem.CompletionThreshold</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string CompletionThreshold = "CompletionThreshold";
        
        /// <summary>
        /// Name of the Credit column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// Credit holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.Credit.Field.htm">ActivityPackageItem.Credit</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Credit = "Credit";
        
        /// <summary>
        /// Name of the IsVisibleInContents column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// IsVisibleInContents holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.IsVisibleInContents.Field.htm">ActivityPackageItem.IsVisibleInContents</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string IsVisibleInContents = "IsVisibleInContents";
        
        /// <summary>
        /// Name of the LaunchData column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// LaunchData holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.LaunchData.Field.htm">ActivityPackageItem.LaunchData</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LaunchData = "LaunchData";
        
        /// <summary>
        /// Name of the MaxAttempts column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// MaxAttempts holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.MaxAttempts.Field.htm">ActivityPackageItem.MaxAttempts</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Int32
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxAttempts = "MaxAttempts";
        
        /// <summary>
        /// Name of the MaxTimeAllowed column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// MaxTimeAllowed holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.MaxTimeAllowed.Field.htm">ActivityPackageItem.MaxTimeAllowed</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Double
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string MaxTimeAllowed = "MaxTimeAllowed";
        
        /// <summary>
        /// Name of the ResourceParameters column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ResourceParameters holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ResourceParameters.Field.htm">ActivityPackageItem.ResourceParameters</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ResourceParameters = "ResourceParameters";
        
        /// <summary>
        /// Name of the ScaledPassingScore column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ScaledPassingScore holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ScaledPassingScore.Field.htm">ActivityPackageItem.ScaledPassingScore</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Single
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ScaledPassingScore = "ScaledPassingScore";
        
        /// <summary>
        /// Name of the TimeLimitAction column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// TimeLimitAction holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.TimeLimitAction.Field.htm">ActivityPackageItem.TimeLimitAction</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: <Typ>/Microsoft.LearningComponents.TimeLimitAction</Typ>
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string TimeLimitAction = "TimeLimitAction";
        
        /// <summary>
        /// Name of the Title column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// Title holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.Title.Field.htm">ActivityPackageItem.Title</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: String[]
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string Title = "Title";
        
        /// <summary>
        /// Name of the ObjectivesGlobalToSystem column on the <Typ>ActivityPackageItemView</Typ> view.
        /// <para>
        /// ObjectivesGlobalToSystem holds the same value as <a href="Microsoft.SharePointLearningKit.Schema.ActivityPackageItem.ObjectivesGlobalToSystem.Field.htm">ActivityPackageItem.ObjectivesGlobalToSystem</a>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Column type: Boolean
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string ObjectivesGlobalToSystem = "ObjectivesGlobalToSystem";
    }
    
    /// <summary>
    /// Contains constants related to the AddPackageReferenceRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// None
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class AddPackageReferenceRight {
        
        /// <summary>
        /// Name of the <Typ>AddPackageReferenceRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.AddPackageReferenceRight.RightName;
    }
    
    /// <summary>
    /// Contains constants related to the RemovePackageReferenceRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>PackageId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class RemovePackageReferenceRight {
        
        /// <summary>
        /// Name of the <Typ>RemovePackageReferenceRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.RemovePackageReferenceRight.RightName;
        
        /// <summary>
        /// Name of the PackageId parameter on the <Typ>RemovePackageReferenceRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.RemovePackageReferenceRight.PackageId;
    }
    
    /// <summary>
    /// Contains constants related to the ReadPackageRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>PackageId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ReadPackageRight {
        
        /// <summary>
        /// Name of the <Typ>ReadPackageRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.ReadPackageRight.RightName;
        
        /// <summary>
        /// Name of the PackageId parameter on the <Typ>ReadPackageRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>PackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string PackageId = Microsoft.LearningComponents.Storage.BaseSchema.ReadPackageRight.PackageId;
    }
    
    /// <summary>
    /// Contains constants related to the CreateAttemptRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerId</Fld></li>
    /// <li><Fld>RootActivityId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class CreateAttemptRight {
        
        /// <summary>
        /// Name of the <Typ>CreateAttemptRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.CreateAttemptRight.RightName;
        
        /// <summary>
        /// Name of the RootActivityId parameter on the <Typ>CreateAttemptRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>ActivityPackageItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string RootActivityId = Microsoft.LearningComponents.Storage.BaseSchema.CreateAttemptRight.RootActivityId;
        
        /// <summary>
        /// Name of the LearnerId parameter on the <Typ>CreateAttemptRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>UserItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerId = Microsoft.LearningComponents.Storage.BaseSchema.CreateAttemptRight.LearnerId;
    }
    
    /// <summary>
    /// Contains constants related to the DeleteAttemptRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class DeleteAttemptRight {
        
        /// <summary>
        /// Name of the <Typ>DeleteAttemptRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.DeleteAttemptRight.RightName;
        
        /// <summary>
        /// Name of the AttemptId parameter on the <Typ>DeleteAttemptRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.DeleteAttemptRight.AttemptId;
    }
    
    /// <summary>
    /// Contains constants related to the ExecuteSessionRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ExecuteSessionRight {
        
        /// <summary>
        /// Name of the <Typ>ExecuteSessionRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.ExecuteSessionRight.RightName;
        
        /// <summary>
        /// Name of the AttemptId parameter on the <Typ>ExecuteSessionRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.ExecuteSessionRight.AttemptId;
    }
    
    /// <summary>
    /// Contains constants related to the ReviewSessionRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ReviewSessionRight {
        
        /// <summary>
        /// Name of the <Typ>ReviewSessionRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.ReviewSessionRight.RightName;
        
        /// <summary>
        /// Name of the AttemptId parameter on the <Typ>ReviewSessionRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.ReviewSessionRight.AttemptId;
    }
    
    /// <summary>
    /// Contains constants related to the RandomAccessSessionRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>AttemptId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class RandomAccessSessionRight {
        
        /// <summary>
        /// Name of the <Typ>RandomAccessSessionRight</Typ> right.
        /// </summary>
        public const string RightName = Microsoft.LearningComponents.Storage.BaseSchema.RandomAccessSessionRight.RightName;
        
        /// <summary>
        /// Name of the AttemptId parameter on the <Typ>RandomAccessSessionRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Reference to a <Typ>AttemptItem</Typ> item type.
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string AttemptId = Microsoft.LearningComponents.Storage.BaseSchema.RandomAccessSessionRight.AttemptId;
    }
    
    /// <summary>
    /// Contains constants related to the StartAttemptOnLearnerAssignmentRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class StartAttemptOnLearnerAssignmentRight {
        
        /// <summary>
        /// Name of the <Typ>StartAttemptOnLearnerAssignmentRight</Typ> right.
        /// </summary>
        public const string RightName = "StartAttemptOnLearnerAssignmentRight";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId parameter on the <Typ>StartAttemptOnLearnerAssignmentRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
    }
    
    /// <summary>
    /// Contains constants related to the FinishLearnerAssignmentRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class FinishLearnerAssignmentRight {
        
        /// <summary>
        /// Name of the <Typ>FinishLearnerAssignmentRight</Typ> right.
        /// </summary>
        public const string RightName = "FinishLearnerAssignmentRight";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId parameter on the <Typ>FinishLearnerAssignmentRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
    }
    
    /// <summary>
    /// Contains constants related to the CompleteLearnerAssignmentRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class CompleteLearnerAssignmentRight {
        
        /// <summary>
        /// Name of the <Typ>CompleteLearnerAssignmentRight</Typ> right.
        /// </summary>
        public const string RightName = "CompleteLearnerAssignmentRight";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId parameter on the <Typ>CompleteLearnerAssignmentRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
    }
    
    /// <summary>
    /// Contains constants related to the FinalizeLearnerAssignmentRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class FinalizeLearnerAssignmentRight {
        
        /// <summary>
        /// Name of the <Typ>FinalizeLearnerAssignmentRight</Typ> right.
        /// </summary>
        public const string RightName = "FinalizeLearnerAssignmentRight";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId parameter on the <Typ>FinalizeLearnerAssignmentRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
    }
    
    /// <summary>
    /// Contains constants related to the ActivateLearnerAssignmentRight right.
    /// </summary>
    /// <remarks>
    /// Parameters in the right:
    /// <ul>
    /// <li><Fld>LearnerAssignmentGuidId</Fld></li>
    /// </ul>
    /// </remarks>
    [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
    [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
    public abstract class ActivateLearnerAssignmentRight {
        
        /// <summary>
        /// Name of the <Typ>ActivateLearnerAssignmentRight</Typ> right.
        /// </summary>
        public const string RightName = "ActivateLearnerAssignmentRight";
        
        /// <summary>
        /// Name of the LearnerAssignmentGuidId parameter on the <Typ>ActivateLearnerAssignmentRight</Typ> right.
        /// </summary>
        /// <remarks>
        /// Parameter type: Guid
        /// </remarks>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        public const string LearnerAssignmentGuidId = "LearnerAssignmentGuidId";
    }
}
namespace Microsoft.SharePointLearningKit {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.LearningComponents.Storage;
    
    
    /// <summary>
    /// Represents the <a href="SlkApi.htm#LearnerAssignmentStates">state</a>
    /// of a <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For more information, see <a href="SlkApi.htm#LearnerAssignmentStates">
    /// Learner
    /// Assignment States.
    /// </a>
    /// </para>
    /// <para>
    /// This enumeration is available only in <a href="Default.htm">SLK</a> (not in
    /// <a href="Mlc.htm">MLC</a>).
    /// </para>
    /// </remarks>
    public enum LearnerAssignmentState {
        
        /// <summary>
        /// NotStarted (0).
        /// The learner has not yet begun the assignment.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        NotStarted = 0,
        
        /// <summary>
        /// Active (1).
        /// The learner has begun working on the assignment, but has not yet submitted it
        /// to the instructor. For self-assigned assignments, the learner has not yet marked
        /// the assignment as "complete".
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        Active = 1,
        
        /// <summary>
        /// Completed (2).
        /// The learner submitted the assignment to the instructor, but the instructor has
        /// not yet completed grading of the
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>.
        /// Self-assigned and auto-returned
        /// assignments do not stay in this state -- they're automatically transitioned to
        /// Final state.
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        Completed = 2,
        
        /// <summary>
        /// Final (3).
        /// The instructor has graded the
        /// <a href="SlkConcepts.htm#Assignments">learner assignment</a>
        /// (or the learner assignment was
        /// automatically graded, or both) and returned it to the learner (or it was
        /// automatically returned to the learner).
        /// </summary>
        [SuppressMessageAttribute("Microsoft.Naming", "CA1726")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1702")]
        [SuppressMessageAttribute("Microsoft.Naming", "CA1704")]
        Final = 3,
    }
    
    /// <summary>
    /// Represents an identifier to a <Typ>/Microsoft.SharePointLearningKit.Schema.SiteSettingsItem</Typ> in a store.
    /// </summary>
    public class SiteSettingsItemIdentifier : LearningStoreItemIdentifier {
        
        /// <summary>
        /// Create a new instance of the SiteSettingsItemIdentifier class.
        /// </summary>
        /// <param name="key">The unique integer value assigned to the item.  This must be a positive integer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        public SiteSettingsItemIdentifier(long key) : 
                base(Microsoft.SharePointLearningKit.Schema.SiteSettingsItem.ItemTypeName, key) {
        }
        
        /// <summary>
        /// Create a new instance of the SiteSettingsItemIdentifier class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is a null refrence.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="id"/> does not contain
        /// a SiteSettingsItem identifier.</exception>
        public SiteSettingsItemIdentifier(LearningStoreItemIdentifier id) : 
                base(id) {
            if ((id == null)) {
                throw new ArgumentNullException("id");
            }
            if ((id.ItemTypeName != Microsoft.SharePointLearningKit.Schema.SiteSettingsItem.ItemTypeName)) {
                throw new ArgumentOutOfRangeException("id");
            }
        }
    }
    
    /// <summary>
    /// Represents an identifier to a <Typ>/Microsoft.SharePointLearningKit.Schema.AssignmentItem</Typ> in a store.
    /// </summary>
    public class AssignmentItemIdentifier : LearningStoreItemIdentifier {
        
        /// <summary>
        /// Create a new instance of the AssignmentItemIdentifier class.
        /// </summary>
        /// <param name="key">The unique integer value assigned to the item.  This must be a positive integer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        public AssignmentItemIdentifier(long key) : 
                base(Microsoft.SharePointLearningKit.Schema.AssignmentItem.ItemTypeName, key) {
        }
        
        /// <summary>
        /// Create a new instance of the AssignmentItemIdentifier class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is a null refrence.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="id"/> does not contain
        /// a AssignmentItem identifier.</exception>
        public AssignmentItemIdentifier(LearningStoreItemIdentifier id) : 
                base(id) {
            if ((id == null)) {
                throw new ArgumentNullException("id");
            }
            if ((id.ItemTypeName != Microsoft.SharePointLearningKit.Schema.AssignmentItem.ItemTypeName)) {
                throw new ArgumentOutOfRangeException("id");
            }
        }
    }
    
    /// <summary>
    /// Represents an identifier to a <Typ>/Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem</Typ> in a store.
    /// </summary>
    public class InstructorAssignmentItemIdentifier : LearningStoreItemIdentifier {
        
        /// <summary>
        /// Create a new instance of the InstructorAssignmentItemIdentifier class.
        /// </summary>
        /// <param name="key">The unique integer value assigned to the item.  This must be a positive integer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        public InstructorAssignmentItemIdentifier(long key) : 
                base(Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.ItemTypeName, key) {
        }
        
        /// <summary>
        /// Create a new instance of the InstructorAssignmentItemIdentifier class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is a null refrence.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="id"/> does not contain
        /// a InstructorAssignmentItem identifier.</exception>
        public InstructorAssignmentItemIdentifier(LearningStoreItemIdentifier id) : 
                base(id) {
            if ((id == null)) {
                throw new ArgumentNullException("id");
            }
            if ((id.ItemTypeName != Microsoft.SharePointLearningKit.Schema.InstructorAssignmentItem.ItemTypeName)) {
                throw new ArgumentOutOfRangeException("id");
            }
        }
    }
    
    /// <summary>
    /// Represents an identifier to a <Typ>/Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem</Typ> in a store.
    /// </summary>
    public class LearnerAssignmentItemIdentifier : LearningStoreItemIdentifier {
        
        /// <summary>
        /// Create a new instance of the LearnerAssignmentItemIdentifier class.
        /// </summary>
        /// <param name="key">The unique integer value assigned to the item.  This must be a positive integer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        public LearnerAssignmentItemIdentifier(long key) : 
                base(Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.ItemTypeName, key) {
        }
        
        /// <summary>
        /// Create a new instance of the LearnerAssignmentItemIdentifier class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is a null refrence.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="id"/> does not contain
        /// a LearnerAssignmentItem identifier.</exception>
        public LearnerAssignmentItemIdentifier(LearningStoreItemIdentifier id) : 
                base(id) {
            if ((id == null)) {
                throw new ArgumentNullException("id");
            }
            if ((id.ItemTypeName != Microsoft.SharePointLearningKit.Schema.LearnerAssignmentItem.ItemTypeName)) {
                throw new ArgumentOutOfRangeException("id");
            }
        }

        /// <summary>Initializes a new instance of <see cref="LearnerAssignmentItemIdentifier"/>.</summary>
        public LearnerAssignmentItemIdentifier()
        {
        }
    }
    
    /// <summary>
    /// Represents an identifier to a <Typ>/Microsoft.SharePointLearningKit.Schema.UserWebListItem</Typ> in a store.
    /// </summary>
    public class UserWebListItemIdentifier : LearningStoreItemIdentifier {
        
        /// <summary>
        /// Create a new instance of the UserWebListItemIdentifier class.
        /// </summary>
        /// <param name="key">The unique integer value assigned to the item.  This must be a positive integer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="key"/> is not a valid positive integer.</exception>
        public UserWebListItemIdentifier(long key) : 
                base(Microsoft.SharePointLearningKit.Schema.UserWebListItem.ItemTypeName, key) {
        }
        
        /// <summary>
        /// Create a new instance of the UserWebListItemIdentifier class.
        /// </summary>
        /// <param name="id">Identifier that should be copied.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is a null refrence.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="id"/> does not contain
        /// a UserWebListItem identifier.</exception>
        public UserWebListItemIdentifier(LearningStoreItemIdentifier id) : 
                base(id) {
            if ((id == null)) {
                throw new ArgumentNullException("id");
            }
            if ((id.ItemTypeName != Microsoft.SharePointLearningKit.Schema.UserWebListItem.ItemTypeName)) {
                throw new ArgumentOutOfRangeException("id");
            }
        }
    }
}
