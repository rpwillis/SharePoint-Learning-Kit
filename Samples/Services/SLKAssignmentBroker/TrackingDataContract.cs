using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharePointLearningKit.Services
{
    /// <summary>
    /// ActivityAttempt is a set of CMI interaction data which is scoped to one Attempt with one Activity.
    /// 
    /// Activity is defined as a set of reusable educational content.  The amount of content / duration it would take to complete an Activity is left up
    /// to the content author.  An Activity can be thought of as the 'atom' (or separatable / reusable unit) of educational content.  In book terms, an
    /// activity is roughly a Chapter -- perhaps with both pages of content and the 'problems at the end of the chapter' together.
    /// 
    /// While the definition of 'attempt' has been purposely left nebulous, in SLK/Grava's terms it means "one download of the content which results in
    /// reaching the end of the content".
    /// 
    /// As SLK/grava matures, 'attempt' may be redefined to mean 'any time the content is opened and then closed' as we start to track much more detail
    /// about how a student is working on an assignment.
    /// </summary>
    /// <remarks>
    /// ActivityAttempt roughly corresponds to the cocdType object from the IEEE 1484.11.3-2005 XSD specification.  It has been modified to align with the SLK
    /// database schema and to be compatible with .NET naming conventions, the DataContract serializer, and C# datastructures.  Examples of changes are:
    ///  * replacing "specified" fields with ? (AKA Nullable&lt;T&gt;) -- e.g. exitField/exitFieldSpecified becomes Exit?
    ///  * replacing field types with "more appropriate" .NET types -- e.g. string totalTimeField becomes TimeSpan? TotalTime
    ///  * replacing pascalCasing with CamelCasing -- e.g. totalTime becomes TotalTile.
    /// </remarks>
    [DataContract(Namespace = "")]
    public class ActivityAttempt
    {
        public ActivityAttempt(string learnerAssignmentIdentifier, string activityIdentifier, CompletionStatus completionStatus)
        {
            LearnerAssignmentIdentifier = learnerAssignmentIdentifier;
            ActivityIdentifier = activityIdentifier;
            CompletionStatus = completionStatus;
        }

        /// <summary>
        /// LearnerAssignmentId is the key that binds this attempt data back to the learner-assignment it's related to in the LMS.
        /// </summary>
        /// <remarks>
        /// Not in the IEEE 1484.11.3-2005 spec, this is an extension that allows this data to be directly associated with SLK's
        /// assignment workflow.
        /// </remarks>
        [DataMember]
        public string LearnerAssignmentIdentifier;

        /// <summary>
        /// Identifier for the Activity which this data is for
        /// </summary>
        /// <remarks>
        /// Not in the IEEE 1484.11.3-2005 spec, this is an extension that allows this data to be directly associated with SLK's
        /// assignment workflow.
        /// </remarks>
        [DataMember]
        public string ActivityIdentifier;

        [DataMember]
        public CompletionStatus CompletionStatus;

        [DataMember]
        public Exit? Exit;

        [DataMember]
        public LessonStatus? LessonStatus;

        [DataMember]
        public string Location;

        [DataMember]
        public decimal? ProgressMeasure;

        [DataMember]
        public Score Score;

        [DataMember]
        public List<Interaction> Interactions;

        /// <remarks>
        /// Not in the IEEE 1484.11.3-2005 spec, this is an extension that allows this data to be directly associated with SLK's
        /// assignment workflow.
        /// </remarks>
        [DataMember]
        public DateTime? SessionStart;

        [DataMember]
        public TimeSpan? SessionTime;

        [DataMember]
        public SuccessStatus? SuccessStatus;
        
        [DataMember]
        public string SuspendData;
        
        [DataMember]
        public TimeSpan? TotalTime;
    }

    [DataContract]
    public class CommentFromLearner
    {
        [DataMember]
        public string Comment;
        
        [DataMember]
        public string Location;
        
        [DataMember]
        public DateTime? Timestamp;
    }

    [DataContract]
    public class Interaction
    {
        [DataMember]
        public string Identifier;
        
        [DataMember]
        public string Description;
        
        [DataMember]
        public List<Objective> Objectives;
        
        [DataMember]
        public LearnerResponse LearnerResponse;
        
        [DataMember]
        public float? Latency;
        
        [DataMember]
        public string Result;
        
        [DataMember]
        public DateTime? Timestamp;
        
        [DataMember]
        public InteractionType? InteractionType;
        
        [DataMember]
        public decimal? Weighting;
    }

    [DataContract]
    public class Objective
    {
        [DataMember]
        public string Identifier;

        [DataMember]
        public CompletionStatus CompletionStatus;
        
        [DataMember]
        public string Description;
        
        [DataMember]
        public Score Score;
        
        [DataMember]
        public SuccessStatus SuccessStatus;
    }

    public interface LearnerResponse { }

    [DataContract]
    public class ChoiceResponse : LearnerResponse
    {
        [DataMember]
        public string Value;
    }

    [DataContract]
    public class ChoicesResponse : LearnerResponse
    {
        [DataMember]
        public List<string> Values;
    }

    [DataContract]
    public class StringResponse : LearnerResponse
    {
        [DataMember]
        public string Value;
    }

    [DataContract]
    public class MatchPatternResponse : LearnerResponse
    {
        [DataMember]
        public Dictionary<string, string> Pairs;
    }

    [DataContract]
    public class NumberResponse : LearnerResponse
    {
        [DataMember]
        public decimal Value;
    }

    [DataContract]
    public class StepResponse : LearnerResponse
    {
        [DataMember]
        public KeyValuePair<string, string> Step;
    }

    [DataContract]
    public class StepsResponse : LearnerResponse
    {
        [DataMember]
        public Dictionary<string, string> Steps;
    }

    [DataContract]
    public class BooleanResponse : LearnerResponse
    {
        [DataMember]
        public bool Value;
    }

    [DataContract]
    public class Score
    {
        [DataMember]
        public decimal? Max;
        
        [DataMember]
        public decimal? Min;
        
        [DataMember]
        public decimal? Raw;
        
        [DataMember]
        public decimal? Scaled;
    }

    public enum SuccessStatus
    {
        Unknown,
        Failed,
        Passed
    }

    public enum CompletionStatus
    {
        Unknown,
        Completed,
        Incomplete,
        NotAttempted
    }

    public enum InteractionType
    {
        Other,
        FillIn,
        Likert,
        LongFillIn,
        Matching,
        MultipleChoice,
        Numeric,
        Performance,
        Sequencing,
        TrueFalse,
        Essay,
        Attachment
    }

    public enum Exit
    {
        Undetermined,
        Logout,
        Normal,
        TimeOut,
        Suspended
    }

    public enum LessonStatus
    {
        NotAttempted,
        Browsed,
        Completed,
        Failed,
        Incomplete,
        Passed
    }
}