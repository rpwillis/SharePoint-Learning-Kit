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
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;

            Utilities.ValidateParameterNonNull("packageReader", packageReader);
            Utilities.ValidateParameterNonNull("packageValidatorSettings", packageValidatorSettings);

            ValidationResults log = ValidateIsELearning(packageReader);
            if (log.HasErrors || (packageValidatorSettings.ScormRequirementValidation == ValidationBehavior.None && 
                packageValidatorSettings.ScormRecommendationValidation == ValidationBehavior.None && 
                packageValidatorSettings.LrmRequirementValidation == ValidationBehavior.None && 
                packageValidatorSettings.MlcRequirementValidation == ValidationBehavior.None))
            {
                return log;
            }
            
            ManifestReaderSettings manifestSettings = new ManifestReaderSettings(true, true);

            ManifestReader manifestReader;
            bool fixLrmViolations = true;
            PackageValidator.Validate(packageValidatorSettings, packageReader, false, log, manifestSettings, fixLrmViolations, out manifestReader);

            return log;
        }

        /// <summary>
        /// Validate the basic structure of a package to ensure it is e-learning content. This method does not 
        /// verify any package details, but rather verifies that the structure of the package indicates it is 
        /// intended to be e-learning content that can be rendered within MLC.
        /// </summary>
        /// <param name="packageReader">The package to validate.</param>
        /// <returns>The results of validation.</returns>
        public static ValidationResults ValidateIsELearning(PackageReader packageReader)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;

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
                            bool fixLrmViolations,
                            out ManifestReader manifestReader)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;

            if (packageReader == null)
                throw new ArgumentNullException("packageReader");

            ValidationResults manifestLog;
            XPathNavigator manifest;
            packageReader.CreateManifestNavigator(packageValidatorSettings.LrmRequirementValidation, fixLrmViolations, out manifestLog, out manifest);

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
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;

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
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;
            
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
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;;

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

}
