/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Collections.ObjectModel;
using Microsoft.LearningComponents.Manifest;
using System.Diagnostics.CodeAnalysis;

// General comments applied to all (or most) of these classes:
// * Re: Properties that get or set XPathNavigators. The classes will maintain an XPathNavigator.
//      It will return a copy of that when a 'get' accessor is accessed.
//      When a 'set' accessor is called, the class will make a copy of the provided object.
// 
// * Classes will hold private variables that correspond to property / method values. In the 
//      read/write classes, these values will be the ones that are 'set' by accessors. When
//      the file is written out, the original XmlDocument will be modified and the new values 
//      added (rather than the whole file re-written). This allows information that we don't know
//      about to be persisted in the file.
namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Represents the &lt;imsss:rollupRule&gt; childActivitySet token value.
    /// </summary>
    public enum RollupChildActivitySet
    {
        /// <summary>
        /// &lt;imsss:rollupRule childActivitySet="all"/&gt;
        /// </summary>
        All,

        /// <summary>
        /// &lt;imsss:rollupRule childActivitySet="any"/&gt;
        /// </summary>
        Any,

        /// <summary>
        /// &lt;imsss:rollupRule childActivitySet="none"/&gt;
        /// </summary>
        None,

        /// <summary>
        /// &lt;imsss:rollupRule childActivitySet="atLeastCount"/&gt;
        /// </summary>
        AtLeastCount,

        /// <summary>
        /// &lt;imsss:rollupRule childActivitySet="atLeastPercentage"/&gt;
        /// </summary>
        AtLeastPercent
    }
    /// <summary>
    /// Represents the &lt;imsss:ruleConditions&gt; conditionCombination attribute.
    /// </summary>
    public enum SequencingConditionCombination
    {
        /// <summary>
        /// &lt;imsss:ruleConditions conditionCombination="all"/&gt;
        /// </summary>
        All,

        /// <summary>
        /// &lt;imsss:ruleConditions conditionCombination="any"/&gt;
        /// </summary>
        Any,
    }

    /// <summary>
    /// Represents the &lt;imsss:rollupCondition&gt; condition attribute.
    /// </summary>
    public enum RollupCondition
    {
        /// <summary>
        /// &lt;imsss:rollupCondition condition="activityProgressKnown"/&gt;
        /// </summary>
        ActivityProgressKnown, 

        /// <summary>
        /// &lt;imsss:rollupCondition condition="attempted"/&gt;
        /// </summary>
        Attempted,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="attemptLimitExceeded"/&gt;
        /// </summary>
        AttemptLimitExceeded,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="completed"/&gt;
        /// </summary>
        Completed,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="objectiveMeasureKnown"/&gt;
        /// </summary>
        ObjectiveMeasureKnown,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="objectiveStatusKnown"/&gt;
        /// </summary>
        ObjectiveStatusKnown,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="outsideAvailableTimeRange"/&gt;
        /// </summary>
        OutsideAvailableTimeRange,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="satisfied"/&gt;
        /// </summary>
        Satisfied,

        /// <summary>
        /// &lt;imsss:rollupCondition condition="timeLimitExceeded"/&gt;
        /// </summary>
        TimeLimitExceeded
    }
    /// <summary>
    /// Represents the &lt;imsss:rollupAction&gt; action attribute.
    /// </summary>
    public enum RollupAction
    {
        /// <summary>
        /// &lt;imsss:rollupAction action="satisfied"/&gt;
        /// </summary>
        Satisfied,

        /// <summary>
        /// &lt;imsss:rollupAction action="notSatisfied"/&gt;
        /// </summary>
        NotSatisfied,

        /// <summary>
        /// &lt;imsss:rollupAction action="completed"/&gt;
        /// </summary>
        Completed,

        /// <summary>
        /// &lt;imsss:rollupAction action="incomplete"/&gt;
        /// </summary>
        Incomplete,

    }
    /// <summary>
    /// Represents the &lt;imsss:rollupConsiderations&gt; requiredForSatisfied, requiredForNotSatisfied,
    /// requiredForCompleted, and requiredForIncomplete attributes.
    /// </summary>
    public enum RollupConsideration 
    {
        /// <summary>
        /// Example: &lt;imsss:rollupConsiderations requiredForSatisfied="always"/&gt;
        /// </summary>
        Always,

        /// <summary>
        /// Example: &lt;imsss:rollupConsiderations requiredForSatisfied="ifAttempted"/&gt;
        /// </summary>
        IfAttempted,

        /// <summary>
        /// Example: &lt;imsss:rollupConsiderations requiredForSatisfied="ifNotSkipped"/&gt;
        /// </summary>
        IfNotSkipped,

        /// <summary>
        /// Example: &lt;imsss:rollupConsiderations requiredForSatisfied="ifNotSuspended"/&gt;
        /// </summary>
        IfNotSuspended
    }

    /// <summary>
    /// Represents the &lt;imsss:randomizationControls&gt; randomizationTiming attribute.
    /// </summary>
    public enum RandomizationTiming
    {
        /// <summary>
        /// &lt;imsss:randomizationControls randomizationTiming="once"/&gt;
        /// </summary>
        Once,

        /// <summary>
        /// &lt;imsss:randomizationControls randomizationTiming="onEachNewAttempt"/&gt;
        /// </summary>
        OnEachNewAttempt,

        /// <summary>
        /// &lt;imsss:randomizationControls randomizationTiming="never"/&gt;
        /// </summary>
        Never
    }

    /// <summary>
    /// Respresents the &lt;imsss:rollupCondition&gt; operator attribute and &lt;imsss:ruleCondition&gt; operator attribute.
    /// </summary>
    public enum SequencingConditionOperator
    {
        /// <summary>
        /// Example: &lt;imsss:rollupCondition operator="noOp"/&gt;
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1706")] // NoOp is the correct capitalization
        NoOp,

        /// <summary>
        /// Example: &lt;imsss:rollupCondition operator="not"/&gt;
        /// </summary>
        Not
    }

    /// <summary>
    /// Represents the &lt;imsss:ruleCondition&gt; condition attribute.
    /// </summary>
    public enum SequencingRuleCondition
    {
        /// <summary>
        /// &lt;imsss:ruleCondition condition="satisfied"/&gt;
        /// </summary>
        Satisfied,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="objectiveStatusKnown"/&gt;
        /// </summary>
        ObjectiveStatusKnown,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="objectiveMeasureKnown"/&gt;
        /// </summary>
        ObjectiveMeasureKnown,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="objectiveMeasureGreaterThan"/&gt;
        /// </summary>
        ObjectiveMeasureGreaterThan,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="objectiveMeasureLessThan"/&gt;
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702")] // MeasureLess is the correct capitalization
        ObjectiveMeasureLessThan,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="completed"/&gt;
        /// </summary>
        Completed,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="activityProgressKnown"/&gt;
        /// </summary>
        ActivityProgressKnown,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="attempted"/&gt;
        /// </summary>
        Attempted,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="attemptLimitExceeded"/&gt;
        /// </summary>
        AttemptLimitExceeded,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="timeLimitExceeded"/&gt;
        /// </summary>
        TimeLimitExceeded,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="outsideAvailableTimeRange"/&gt;
        /// </summary>
        OutsideAvailableTimeRange,

        /// <summary>
        /// &lt;imsss:ruleCondition condition="always"/&gt;
        /// </summary>
        Always
    }

    /// <summary>
    /// Represents the &lt;imsss:ruleAction&gt; action attribute.
    /// </summary>
    public enum SequencingRuleAction
    {
        /// <summary>
        /// Doesn't map to a SCORM token.  Means that the &lt;imsss:ruleAction&gt; node is either missing or lacking
        /// the "action" attribute value.
        /// </summary>
        NoAction = 0,

        /// <summary>
        /// &lt;imsss:ruleAction action="skip"/&gt;
        /// </summary>
        Skip,

        /// <summary>
        /// &lt;imsss:ruleAction action="disabled"/&gt;
        /// </summary>
        Disabled,

        /// <summary>
        /// &lt;imsss:ruleAction action="hiddenFromChoice"/&gt;
        /// </summary>
        HiddenFromChoice,

        /// <summary>
        /// &lt;imsss:ruleAction action="stopForwardTraversal"/&gt;
        /// </summary>
        StopForwardTraversal,

        /// <summary>
        /// &lt;imsss:ruleAction action="exitParent"/&gt;
        /// </summary>
        ExitParent,

        /// <summary>
        /// &lt;imsss:ruleAction action="exitAll"/&gt;
        /// </summary>
        ExitAll,

        /// <summary>
        /// &lt;imsss:ruleAction action="retry"/&gt;
        /// </summary>
        Retry,

        /// <summary>
        /// &lt;imsss:ruleAction action="retryAll"/&gt;
        /// </summary>
        RetryAll,

        /// <summary>
        /// &lt;imsss:ruleAction action="continue"/&gt;
        /// </summary>
        Continue,

        /// <summary>
        /// &lt;imsss:ruleAction action="previous"/&gt;
        /// </summary>
        Previous,

        /// <summary>
        /// &lt;imsss:ruleAction action="exit"/&gt;
        /// </summary>
        Exit
    }

    /// <summary>
    /// Define the type of resources included in SCORM content.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Indicates an activity with no associated resource.
        /// </summary>
        None = 0,

        /// <summary>
        /// Uses RTE on client.
        /// </summary>
        Sco,

        /// <summary>
        /// Does not use RTE on the client and is not Lrm content.
        /// </summary>
        Asset,

        /// <summary>
        /// Is Class Server Lrm content.
        /// </summary>
        Lrm
    }

    /// <summary>
    /// The type of package. 
    /// </summary>
    public enum PackageType
    {
        /// <summary>
        /// A resource package, with no organization node. 
        /// </summary>
        Resource,

        /// <summary>
        /// A package that aggregates and organizes content.
        /// </summary>
        ContentAggregation
        // Issue: Are there other potential types?
    }
}

