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
using Microsoft.SharePointLearningKit.Localization;

namespace Microsoft.LearningComponents
{

    #region // Validator classes

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

    /// <summary>
    /// PackageValidator detects variances with the SCORM specification.
    /// </summary>
    /// <remarks>Given a SCORM package, the PackageValidator will verify that the manifest is 
    /// correct (using the ManifestValidator object) and verify that the files referenced in the 
    /// manifest are actually present in the package.
    /// <para>The PackageValidator does not verify that files referenced within assets and not within
    /// the package manifest are present in the package. Put another way: the PackageValidator does 
    /// not read or parse the contents of any file (other than the manifest) within the package.
    /// </para>
    /// <para>
    /// Definitions:
    /// Error = A required node in a SCORM package either does not exist or does not have
    /// an appropriate value.
    /// Warning = A SCORM recommendation is not followed in the package or an optional 
    /// value in the package has an invalid value. </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
    public class PackageValidator
    {
        private PackageValidator() { }

 
        /// <summary>
        /// Validate a package according to specific validation settings. 
        /// </summary>
        /// <remarks>
        /// The <Typ>ValidationResults</Typ> log will contain the warnings and errors encountered during the package
        /// validation process, up to any <Typ>InvalidPackageException</Typ> that is thrown during the process, at
        /// which time the validation process ends.
        /// </remarks>
        /// <returns>
        /// A <Typ>ValidationResults</Typ> log that contains the warnings and errors encountered during the package
        /// validation process.
        /// </returns>
        /// <param name="packageValidatorSettings">The <c>PackageValidatorSettings</c> determining which 
        /// validation rules should be applied during the validation process.</param>
        /// <param name="packageReader">The package to be valdated.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="packageReader"/> is null.</exception>
        /// <exception cref="InvalidPackageException">Thrown if the validation process encounters errors in the package
        /// for which the <paramref name="packageValidatorSettings"/> have a <Typ>ValidationBehavior</Typ> value 
        /// of <c>ValidationBehavior.Enforce</c>.</exception>
        public static ValidationResults Validate(PackageReader packageReader, PackageValidatorSettings packageValidatorSettings)
        {
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();

            Utilities.ValidateParameterNonNull("packageReader", packageReader);
            Utilities.ValidateParameterNonNull("packageValidatorSettings", packageValidatorSettings);

            ValidationResults log = Validate(packageReader);
            if (log.HasErrors || (packageValidatorSettings.ScormRequirementValidation == ValidationBehavior.None && 
                packageValidatorSettings.ScormRecommendationValidation == ValidationBehavior.None && 
                packageValidatorSettings.LrmRequirementValidation == ValidationBehavior.None && 
                packageValidatorSettings.MlcRequirementValidation == ValidationBehavior.None))
            {
                return log;
            }
            
            ManifestReaderSettings manifestSettings = new ManifestReaderSettings(true, true);
            LrmSettings lrmSettings = new LrmSettings(true);

            ManifestReader manifestReader;
            PackageValidator.Validate(packageValidatorSettings, packageReader, false, log, manifestSettings, lrmSettings, out manifestReader);

            return log;
        }

        /// <summary>
        /// Validate the basic structure of a package to ensure it is e-learning content. This method does not 
        /// verify any package details, but rather verifies that the structure of the package indicates it is 
        /// intended to be e-learning content that can be rendered within MLC.
        /// </summary>
        /// <param name="packageReader">The package to validate.</param>
        /// <returns>The results of validation.</returns>
        public static ValidationResults Validate(PackageReader packageReader)
        {
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();

            Utilities.ValidateParameterNonNull("packageReader", packageReader);
            ValidationResults results = new ValidationResults();

            try
            {
                // If the path does not contain imsmanifest.xml return error result
                if (!packageReader.FileExists("imsmanifest.xml"))
                {
                    results.AddResult(new ValidationResult(true, ValidatorResources.ManifestMissing));
                }
            }
            catch (InvalidPackageException e)
            {
                results.AddResult(new ValidationResult(true, e.Message));
            }
            
            return results;
        }

        internal static void Validate(PackageValidatorSettings packageValidatorSettings,
                            PackageReader packageReader,
                            bool logReplacement,
                            ValidationResults log,
                            ManifestReaderSettings manifestSettings,
                            LrmSettings lrmSettings,
                            out ManifestReader manifestReader)
        {
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();

            if (packageReader == null)
                throw new ArgumentNullException("packageReader");

            ValidationResults manifestLog;
            XPathNavigator manifest;
            packageReader.CreateManifestNavigator(packageValidatorSettings.LrmRequirementValidation, lrmSettings.FixLrmViolations,
                out manifestLog, out manifest);

            if (manifestLog != null)
                foreach (ValidationResult result in manifestLog.Results)
                {
                    log.AddResult(result);
                }

            manifestReader = new ManifestReader(packageReader, manifestSettings, packageValidatorSettings, logReplacement, log, manifest);

            if (packageValidatorSettings.MlcRequirementValidation != ValidationBehavior.None)
            {
                int activityCount = 0;
                if (manifestReader.Organizations.Count > 0)
                {
                    foreach (OrganizationNodeReader nodeReader in manifestReader.Organizations)
                    {
                        activityCount += nodeReader.Activities.Count;
                    }
                    if (activityCount == 0)
                    {
                        ProcessError(packageValidatorSettings.MlcRequirementValidation,
                            ValidatorResources.MlcViolationActivityMissing,
                            log);
                    }
                }
                else
                {
                    ProcessError(packageValidatorSettings.MlcRequirementValidation,
                        ValidatorResources.MlcViolationOrganizationMissing,
                        log);
                }
            }
            ManifestValidator.Validate(manifestReader);

            // Add all files in the manifest to a Dictionary
            Dictionary<string, bool> manifestFilePaths = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (ResourceNodeReader r in manifestReader.Resources.Values)
            {
                foreach (FileNodeReader fileNode in r.Files)
                {
                    if (!fileNode.Location.IsAbsoluteUri)
                    {
                        // get the path component of the Location property, and URL decode the string
                        // so it is a proper file path.
                        string path = System.Web.HttpUtility.UrlDecode(RemoveQueryAndAnchor(fileNode.Location.OriginalString));
                        if (!manifestFilePaths.ContainsKey(path))
                        {
                            manifestFilePaths.Add(path, true);
                        }
                    }
                }
            }

			ReadOnlyCollection<string> packageFilePaths = packageReader.GetFilePaths();

			CheckManifestFiles(packageFilePaths, manifestFilePaths, log, packageValidatorSettings);
			CheckPackageFiles(packageFilePaths, manifestFilePaths, log, packageValidatorSettings);
			
		}

        /// <summary>
        /// Used by RemoveQueryAndAnchor()
        /// </summary>
        private static char[] queryOrAnchor = new char[] {'?','#'};
        /// <summary>
        /// Given a string containing a URI, remove any query string or anchor (e.g. anything after a "?" or "#").
        /// </summary>
        private static string RemoveQueryAndAnchor(string uri)
        {
            int i = uri.IndexOfAny(queryOrAnchor);
            if (i > -1)
            {
                return uri.Substring(0, i);
            }
            else
            {
                return uri;
            }
        }

		private static void CheckPackageFiles(ReadOnlyCollection<string> packageFilePaths,
			Dictionary<string, bool> manifestFilePaths, ValidationResults log,
			PackageValidatorSettings packageValidatorSettings)
		{
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();

            // Check all of the files in the package to make sure they are referenced in the manifest.
			foreach (string filePath in packageFilePaths)
            {
                string fileExtension = Path.GetExtension(filePath);
                if (filePath != "imsmanifest.xml"
                    && string.Compare(fileExtension, ".xsd", StringComparison.OrdinalIgnoreCase) != 0
                    && string.Compare(fileExtension, ".dtd", StringComparison.OrdinalIgnoreCase) != 0
					&& !manifestFilePaths.ContainsKey(filePath))
                {
                    ProcessError(packageValidatorSettings.ScormRecommendationValidation,
                        string.Format(CultureInfo.CurrentCulture,
                        ValidatorResources.OrphanedFile, filePath), log);
                }
            }
        }

		private static void CheckManifestFiles(ReadOnlyCollection<string> packageFilePaths,
			Dictionary<string, bool> manifestFilePaths, ValidationResults log,
			PackageValidatorSettings packageValidatorSettings)
		{
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();
            
            // Check all of the files in the manifest to make sure they are referenced in the package.
			Dictionary<string, bool> packageFileDictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

			foreach (string filePath in packageFilePaths)
			{
				packageFileDictionary.Add(filePath, true);
			}

			foreach (string filePath in manifestFilePaths.Keys)
			{
				if (!packageFileDictionary.ContainsKey(filePath))
				{
					ProcessError(packageValidatorSettings.ScormRequirementValidation,
						string.Format(CultureInfo.CurrentCulture,
						ValidatorResources.FileMissing, filePath), log);
				}
			}

		}

        private static void ProcessError(ValidationBehavior validationBehavior, string errorMessage, ValidationResults log)
        {
            ValidatorResources.Culture = LocalizationManager.GetCurrentCulture();

            switch (validationBehavior)
            {
                case ValidationBehavior.Enforce:
                    // throw an exception
                    log.AddResult(new ValidationResult(true, errorMessage));
                    throw new InvalidPackageException(errorMessage);
                case ValidationBehavior.LogError:
                    // log an error
                    log.AddResult(new ValidationResult(true, errorMessage));
                    break;
                case ValidationBehavior.LogWarning:
                    // log a warning
                    log.AddResult(new ValidationResult(false, errorMessage));
                    break;
                case ValidationBehavior.None:
                    // do nothing
                    break;
            }
        }
    }

    /// <summary>
    /// The results from a validation operation.
    /// </summary>
    /// <remarks>
    /// This is a class to hold logged warnings and errors of type <Typ>ValidationResult</Typ>.
    /// </remarks>
    ///
    [Serializable]
    public class ValidationResults
    {
        private List<ValidationResult> m_results;
        private bool m_hasWarnings;
        private bool m_hasErrors;

        /// <summary>
        /// Constructor.
        /// </summary>
        internal ValidationResults()
        {
            m_results = new List<ValidationResult>();
        }

        /// <summary>
        /// List of errors and warnings in the package.
        /// </summary>
        public ReadOnlyCollection<ValidationResult> Results { get { return new ReadOnlyCollection<ValidationResult>(m_results); } }

        /// <summary>
        /// True if there are errors in the log.  False if there are not.
        /// </summary>
        public bool HasErrors { get { return m_hasErrors; } }

        /// <summary>
        /// True if there are warnings in the log. False if there are not.
        /// </summary>
        public bool HasWarnings { get { return m_hasWarnings; } }

        /// <summary>
        /// Adds a warning to the <paramref name="log"/>, if provided.
        /// </summary>
        /// <param name="log">If non-null, the warning is added to it.</param>
        /// <param name="message">The message to log.</param>
        internal static void LogWarning(ValidationResults log, string message)
        {
            if (log != null)
            {
                log.m_results.Add(new ValidationResult(false, message));
                log.m_hasWarnings = true;
            }
        }

        /// <summary>
        /// Adds an error to the <paramref name="log"/>, if provided.  Throws a <Typ>InvalidPackageException</Typ> if the
        /// <paramref name="throwInvalidPackageException"/> is <c>true</c>.
        /// </summary>
        /// <param name="log">If non-null, the error is added to it.</param>
        /// <param name="throwInvalidPackageException">True to throw a <Typ>InvalidPackageException</Typ> containing the <paramref name="message"/>.
        /// </param>
        /// <param name="message">The message to log.</param>
        internal static void LogError(ValidationResults log, bool throwInvalidPackageException, string message)
        {
            if (log != null)
            {
                log.m_results.Add(new ValidationResult(true, message));
                log.m_hasErrors = true;
            }
            if (throwInvalidPackageException)
            {
                throw new InvalidPackageException(message);
            }
        }

        /// <summary>
        /// Adds a new <Typ>ValidationResult</Typ> item to the log.  
        /// </summary>
        /// <param name="result">The <Typ>ValidationResult</Typ> log item to add to the <paramref name="log"/>.</param>
        internal void AddResult(ValidationResult result)
        {
            m_results.Add(result);
            if (result.IsError) m_hasErrors = true;
            else m_hasWarnings = true;
        }
    }

    /// <summary>
    /// ValidationResult is the information about a particular error 
    /// or warning that was received during the process of validating a package.
    /// </summary>  
    [Serializable]
    public class ValidationResult : ISerializable
    {
        private string m_message;
        private bool m_isError;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isError"><c>true</c> if this is a critical problem in the manifest, or <c>false</c> if this is a non-critical 
        /// problem in the manifest.</param>
        /// <param name="message">The result message.</param>
        internal ValidationResult(bool isError, string message)
        {
            m_isError = isError;
            m_message = message;
        }

        /// <summary>
        /// Constructor called during de-serialization.
        /// </summary>
        /// <param name="info">Info about object.</param>
        /// <param name="context">Context of deserialization.</param>
        protected ValidationResult(SerializationInfo info, StreamingContext context)
        {
            Utilities.ValidateParameterNonNull("info", info);

            m_isError = info.GetBoolean("isError");
            m_message = info.GetString("message");
        }

        /// <summary>
        /// True if this represents a warning.
        /// </summary>
        public bool IsWarning { get { return !m_isError; } }

        /// <summary>
        /// True if this represents an error.
        /// </summary>
        public bool IsError { get { return m_isError; } }

        /// <summary>
        /// The human-readable message for this warning or error.
        /// </summary>
        public string Message { get { return m_message; } }  // E.g. "There are multiple instances of element, <metadata>.  All but the first are ignored."

        #region ISerializable Members

        /// <summary>
        /// Return serialization data.
        /// </summary>
        /// <param name="info">The information for serialization</param>
        /// <param name="context">The context of serialization</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="info"/> of <paramref name="context"/>
        /// is not provided.</exception>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)] // Required by fxcop
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Utilities.ValidateParameterNonNull("info", info);

            info.AddValue("isError", IsError);
            info.AddValue("message", Message);
        }

        #endregion
    }

    /// <summary>
    /// Validation behaviors used by <Typ>PackageValidatorSettings</Typ>.
    /// </summary>
    public enum ValidationBehavior
    {
        /// <summary>
        /// Validation violations are ignored.
        /// </summary>
        None = 0,
        /// <summary>
        /// Validation violations are logged as warnings.
        /// </summary>
        LogWarning,
        /// <summary>
        /// Validation violations are logged as errors.
        /// </summary>
        LogError,
        /// <summary>
        /// Validation violations throw exceptions.
        /// </summary>
        Enforce
    }

    /// <summary>
    /// Validation behaviors used during package validation.
    /// </summary>
    public class PackageValidatorSettings
    {
        private ValidationBehavior m_scormRequirementValidation;
        private ValidationBehavior m_scormRecommendationValidation;
        private ValidationBehavior m_mlcRequirementValidation;
        private ValidationBehavior m_lrmRequirementValidation;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scormRequirementValidation">How to validate SCORM requirement violations.</param>
        /// <param name="scormRecommendationValidation">How to validate SCORM recommendation violations.</param>
        /// <param name="mlcRequirementValidation">How to validate MLC requirement violations.</param>
        /// <param name="lrmRequirementValidation">How to validate LRM requirement violations.</param>
        public PackageValidatorSettings(ValidationBehavior scormRequirementValidation,
            ValidationBehavior scormRecommendationValidation,
            ValidationBehavior mlcRequirementValidation,
            ValidationBehavior lrmRequirementValidation)
        {
            m_mlcRequirementValidation = mlcRequirementValidation;
            m_scormRecommendationValidation = scormRecommendationValidation;
            m_scormRequirementValidation = scormRequirementValidation;
            m_lrmRequirementValidation = lrmRequirementValidation;
        }

        /// <summary>
        /// How to validate SCORM requirement violations.
        /// </summary>
        public ValidationBehavior ScormRequirementValidation
        {
            get
            {
                return m_scormRequirementValidation;
            }
            set
            {
                m_scormRequirementValidation = value;
            }
        }

        /// <summary>
        /// How to validate SCORM recommendation violations.
        /// </summary>
        public ValidationBehavior ScormRecommendationValidation
        {
            get
            {
                return m_scormRecommendationValidation;
            }
            set
            {
                m_scormRecommendationValidation = value;
            }
        }

        /// <summary>
        /// How to validate MLC requirement violations.
        /// </summary>
        public ValidationBehavior MlcRequirementValidation
        {
            get
            {
                return m_mlcRequirementValidation;
            }
            set
            {
                m_mlcRequirementValidation = value;
            }
        }

        /// <summary>
        /// How to validate Lrm requirement violations.
        /// </summary>
        public ValidationBehavior LrmRequirementValidation
        {
            get
            {
                return m_lrmRequirementValidation;
            }
            set
            {
                m_lrmRequirementValidation = value;
            }
        }


        /// <summary>
        /// Returns true if the behavior is LogError or LogWarning.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        private static bool RequiresLog(ValidationBehavior behavior)
        {
            if (behavior == ValidationBehavior.LogError || behavior == ValidationBehavior.LogWarning)
                return true;
            else return false;
        }

        /// <summary>
        /// Returns true if any of the validation requirements is LogError or LogWarning.
        /// </summary>
        /// <returns></returns>
        internal bool RequiresLog()
        {
            if (RequiresLog(m_mlcRequirementValidation)) return true;
            if (RequiresLog(m_scormRecommendationValidation)) return true;
            if (RequiresLog(m_scormRequirementValidation)) return true;
            if (RequiresLog(m_lrmRequirementValidation)) return true;
            return false;
        }
    }
    #endregion

    /// <summary>
    /// Exception to indicate the package contents are not valid.
    /// </summary>
    [Serializable]
    public class InvalidPackageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        public InvalidPackageException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidPackageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <Typ>InvalidPackageException</Typ> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InvalidPackageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// Settings used to determine how to handle problems with an LRM package.
    /// </summary>
    internal class LrmSettings
    {
        private bool m_fixViolations;

        /// <summary>
        /// Constructor for LrmSettings, which determines whether or not to fix a package that violates LRM requirements.
        /// </summary>
        /// <param name="fixLrmViolations">
        /// If <c>true</c>, when an LRM requirement is violated the value will be changed to a valid
        /// value.  If <c>false</c>, throw an exception.
        /// </param>
        public LrmSettings(bool fixLrmViolations)
        {
            m_fixViolations = fixLrmViolations;
        }

        /// <summary>
        /// If <c>true</c>, when an LRM requirement is violated the value will be changed to a valid
        /// value.  If <c>false</c>, throw an exception.
        /// </summary>
        public bool FixLrmViolations
        {
            get { return m_fixViolations; }
            set { m_fixViolations = value; }
        }
    }

}
