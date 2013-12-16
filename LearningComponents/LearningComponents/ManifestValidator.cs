/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security.Principal;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.LearningComponents.Manifest;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.LearningComponents
{


    /// <summary>
    /// ManifestValidator validates that the contents of the manifest conforms to the SCORM requirements,
    /// for those manifest elements and attributes that are relevant to Microsoft Learning Components.
    /// </summary>
    /// <remarks>
    /// ManifestValidator does not attempt to validate that files external to the manifest
    /// exist in the locations indicated in the manifest. Use PackageValidator for that level 
    /// of validation.
    /// </remarks>
    internal class ManifestValidator
    {
        // The ManifestValidator works by calling all possible properties, child properties, and applicable
        // methods on a ManifestReader. Since the ManifestReader validates itself as properties are read, all
        // that is needed to validiate it is to reader every property.
        //  - ValidateCollection methods call ValidateNode on each item in the passed in collection
        //  - ValidateNode methods read the properties of the node passed in and call the
        //    proper method depending on whether the property is a Collection, Node, or value
        //  - The ValidateProperty method is an empty method used for getting the manifest to validate whether
        //    a property is valid or not.
        private ManifestValidator()
        {
        }

        /// <summary>
        /// Validate the manifest. Errors and warnings are returned in the results
        /// </summary>
        /// <param name="manifestReader">The <c>ManifestReader</c> to validate.</param>
        public static void Validate(ManifestReader manifestReader)
        {
            if (manifestReader == null)
                throw new ArgumentNullException("reader");

            ValidateNode(manifestReader);
        }

        #region private methods
        // The ValidateCollection methods call Validate node on each item in the passed in collection
        private static void ValidateCollection(ReadOnlyCollection<OrganizationNodeReader> collection)
        {
            foreach (OrganizationNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<ActivityNodeReader> collection)
        {
            foreach (ActivityNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<FileNodeReader> collection)
        {
            foreach (FileNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<SequencingRuleNodeReader> collection)
        {
            foreach (SequencingRuleNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<SequencingRuleConditionNodeReader> collection)
        {
            foreach (SequencingRuleConditionNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<SequencingObjectiveMapNodeReader> collection)
        {
            foreach (SequencingObjectiveMapNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<SequencingRollupRuleNodeReader> collection)
        {
            foreach (SequencingRollupRuleNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(ReadOnlyCollection<SequencingRollupConditionNodeReader> collection)
        {
            foreach (SequencingRollupConditionNodeReader nodeReader in collection)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(IDictionary<string, SequencingNodeReader> collection)
        {
            foreach (SequencingNodeReader nodeReader in collection.Values)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(IDictionary<string, ResourceNodeReader> collection)
        {
            foreach (ResourceNodeReader nodeReader in collection.Values)
                ValidateNode(nodeReader);
        }
        private static void ValidateCollection(IDictionary<string, SequencingObjectiveNodeReader> collection)
        {
            foreach (SequencingObjectiveNodeReader nodeReader in collection.Values)
                ValidateNode(nodeReader);
        }
        // The ValidateNode methods read the properties of the node passed in and call the
        // proper method depending on whether the property is a collection, Node, or regular value
        private static void ValidateNode(ManifestReader reader)
        {
            if (reader == null)
                return;
            ValidateCollection(reader.Organizations); // caught BadProperties1
            ValidateCollection(reader.SequencingCollection); // caught BadProperties2
            ValidateCollection(reader.Resources); // caught BadProperties3
            ValidateNode(reader.DefaultOrganization); // caught BadProperties1
            ValidateProperty(reader.Id); // caught BadProperties3
            ValidateProperty(reader.Log); // This doesn't come from the manifest file
            ValidateProperty(reader.Metadata); // caught BadProperties3
            ValidateProperty(reader.PackageType); // derived value, doesn't throw an exception
            ValidateProperty(reader.Version); // doesn't throw an exception
            ValidateProperty(reader.ResourcesXmlBase); // Issue: not sure how to trap this
            ValidateProperty(reader.PackageFormat); // this value is set on opening the document so won't throw here
            ValidateProperty(reader.XmlBase); // possibly a derived value, doesn't throw an exception
        }
        private static void ValidateNode(OrganizationNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.Activities); // caught BadProperties2
            ValidateNode(nodeReader.Sequencing); // Issue: not sure how to trap this
            ValidateProperty(nodeReader.Id); // Issue: errors here are thrown in the parent collection
            ValidateProperty(nodeReader.Metadata); // caught BadProperties3
            ValidateProperty(nodeReader.ObjectivesGlobalToSystem); // caught BadProperties3
            ValidateProperty(nodeReader.Structure); // derived value, doesn't throw an exception
            ValidateProperty(nodeReader.Title); // caught BadProperties4
            ValidateProperty(nodeReader.Instructions);
            ValidateProperty(nodeReader.Description);
            ValidateProperty(nodeReader.PointsPossible);
        }
        private static void ValidateNode(ResourceNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;
            // To avoid getting caught in circular dependency loops, only validate the
            // top level of a ResourceNodeReader.Dependencies.
            foreach (ResourceNodeReader resource in nodeReader.Dependencies)
            {
                ValidateResource(resource); // caught BadProperties4
            }
            ValidateResource(nodeReader);
        }
        private static void ValidateResource(ResourceNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;
            ValidateCollection(nodeReader.Files); // caught BadProperties1
            ValidateProperty(nodeReader.EntryPoint); // Issue: not sure how to trap this
            ValidateProperty(nodeReader.Id); // Issue: errors here are thrown in the parent collection
            ValidateProperty(nodeReader.Metadata); // Issue: not sure how to trap this
            ValidateProperty(nodeReader.ResourceType); // caught BadProperties1
            ValidateProperty(nodeReader.XmlBase); // Issue: not sure how to trap this
        }
        private static void ValidateNode(SequencingNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.ExitConditionRules); // Issue: not sure how to trap this
            ValidateCollection(nodeReader.Objectives); // Issue: not sure how to trap this
            ValidateCollection(nodeReader.PostConditionRules); // Issue: not sure how to trap this
            ValidateCollection(nodeReader.PreConditionRules); // Issue: not sure how to trap this
            ValidateCollection(nodeReader.RollupRules); // Issue: not sure how to trap this
            ValidateProperty(nodeReader.AttemptAbsoluteDurationLimit); // caught BadProperties3
            ValidateProperty(nodeReader.AttemptLimit); // caught BadProperties2
            ValidateProperty(nodeReader.Choice); // caught BadProperties2
            ValidateProperty(nodeReader.ChoiceExit); // caught BadProperties3
            ValidateProperty(nodeReader.CompletionSetByContent); // caught BadProperties3
            ValidateProperty(nodeReader.ConstrainChoice); // caught BadProperties3
            ValidateProperty(nodeReader.Flow); // caught BadProperties2
            ValidateProperty(nodeReader.ForwardOnly); // caught BadProperties3
            ValidateProperty(nodeReader.MeasureSatisfactionIfActive); // caught BadProperties3
            ValidateProperty(nodeReader.ObjectiveMeasureWeight); // caught BadProperties3
            ValidateProperty(nodeReader.ObjectiveSetByContent); // caught BadProperties3
            ValidateProperty(nodeReader.PreventActivation); // caught BadProperties3
            ValidateProperty(nodeReader.RandomizationSelectCount); // caught BadProperties3
            ValidateProperty(nodeReader.RandomizationTiming); // caught BadProperties3
            ValidateProperty(nodeReader.ReorderChildren); // caught BadProperties3
            ValidateProperty(nodeReader.RequiredForCompleted); // caught BadProperties2
            ValidateProperty(nodeReader.RequiredForIncomplete); // caught BadProperties3
            ValidateProperty(nodeReader.RequiredForNotSatisfied); // caught BadProperties3
            ValidateProperty(nodeReader.RequiredForSatisfied); // caught BadProperties3
            ValidateProperty(nodeReader.RollupObjectiveSatisfied); // caught BadProperties2
            ValidateProperty(nodeReader.RollupProgressCompletion); // caught BadProperties3
            ValidateProperty(nodeReader.SelectionTiming); // caught BadProperties3
            ValidateProperty(nodeReader.Tracked); // caught BadProperties3
            ValidateProperty(nodeReader.UseCurrentAttemptObjectiveInfo); // caught BadProperties3
            ValidateProperty(nodeReader.UseCurrentAttemptProgressInfo); // caught BadProperties3

        }
        private static void ValidateNode(ActivityNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.ChildActivities); // caught BadProperties4
            ValidateNode(nodeReader.Resource); // caught BadProperties2
            ValidateNode(nodeReader.Sequencing); // Issue: not sure how to trap this
            ValidateProperty(nodeReader.CompletionThreshold); // caught BadProperties3
            ValidateProperty(nodeReader.DataFromLms); // caught BadProperties3
            ValidateProperty(nodeReader.HideAbandonUI); // caught BadProperties3
            ValidateProperty(nodeReader.HideContinueUI); // caught BadProperties3
            ValidateProperty(nodeReader.HideExitUI); // caught BadProperties3
            ValidateProperty(nodeReader.HidePreviousUI); // caught BadProperties3
            ValidateProperty(nodeReader.Id); // errors here throw on ChildActivities
            ValidateProperty(nodeReader.IsVisible); // BadProperties2
            ValidateProperty(nodeReader.MasteryScore);
            ValidateProperty(nodeReader.MaximumTimeAllowed);
            ValidateProperty(nodeReader.Metadata); // caught BadProperties2
            ValidateProperty(nodeReader.Prerequisites);
            ValidateProperty(nodeReader.ResourceParameters); // Issue: not able to trap
            ValidateProperty(nodeReader.TimeLimitAction); // caught BadProperties2
            ValidateProperty(nodeReader.Title); // caught BadProperties2
        }
        private static void ValidateNode(FileNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateProperty(nodeReader.IsEntryPoint);  // Issue: not sure if I can trap this.
            ValidateProperty(nodeReader.Location); // Issue: errors here are thrown in the parent collection
            ValidateProperty(nodeReader.Metadata); // Issue: errors here are thrown in the parent collection

        }
        private static void ValidateNode(SequencingRuleNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.Conditions); // caught BadProperties2
            ValidateProperty(nodeReader.Action); // caught BadProperties2
            ValidateProperty(nodeReader.Combination); // caught BadProperties3

        }
        private static void ValidateNode(SequencingObjectiveNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.Mappings); // caught BadProperties2
            ValidateProperty(nodeReader.Id); // Issue: a bad id throws on the parent collection
            ValidateProperty(nodeReader.IsPrimaryObjective); // Issue: don't know how to test
            ValidateProperty(nodeReader.MinimumNormalizedMeasure); // caught BadProperties2
            ValidateProperty(nodeReader.SatisfiedByMeasure); // caught BadProperties2

        }
        private static void ValidateNode(SequencingRuleConditionNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateProperty(nodeReader.Condition); // caught BadProperties2
            ValidateProperty(nodeReader.MeasureThreshold); // caught BadProperties3
            ValidateProperty(nodeReader.Operator); // caught BadProperties3
            ValidateProperty(nodeReader.ReferencedObjectiveId); // caught BadProperties3

        }
        private static void ValidateNode(SequencingObjectiveMapNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateProperty(nodeReader.ReadNormalizedMeasure); // errors get trapped at the Mapping node
            ValidateProperty(nodeReader.ReadSatisfiedStatus); // errors get trapped at the Mapping node
            ValidateProperty(nodeReader.TargetObjectiveId); // errors get trapped at the Mapping node
            ValidateProperty(nodeReader.WriteNormalizedMeasure); // errors get trapped at the Mapping node
            ValidateProperty(nodeReader.WriteSatisfiedStatus); // errors get trapped at the Mapping node

        }
        private static void ValidateNode(SequencingRollupRuleNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateCollection(nodeReader.Conditions); // caught BadProperties2
            ValidateProperty(nodeReader.Action); // caught BadProperties2
            ValidateProperty(nodeReader.ChildActivitySet); // caught BadProperties2
            ValidateProperty(nodeReader.ConditionCombination); // caught BadProperties2
            ValidateProperty(nodeReader.MinimumCount); // caught BadProperties2
            ValidateProperty(nodeReader.MinimumPercent); // caught BadProperties2

        }
        private static void ValidateNode(SequencingRollupConditionNodeReader nodeReader)
        {
            if (nodeReader == null)
                return;

            ValidateProperty(nodeReader.Condition); // caught BadProperties2
            ValidateProperty(nodeReader.Operator); // caught BadProperties2

        }
        // The ValidateProperty method is an empty method used for getting the manifest to validate whether
        // a property is valid or not.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "s")]
        private static void ValidateProperty<T>(T value)
        {
            // At the moment, simply referencing the property is enough to validate it.
        }

        #endregion private methods

    }
}
