/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.IO;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using System.Diagnostics.CodeAnalysis;
using Microsoft.LearningComponents.Storage;
using System.Xml.XPath;
using System.Xml;
using System.Runtime.Serialization;
using System.Globalization;
using Schema=Microsoft.LearningComponents.Storage.BaseSchema;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a store which contains SCORM packages. Subclasses represent various means
    /// of persisting the store in long-term storage.
    /// </summary>
    /// <remarks>
    /// If an application installed a LearningStore schema that contains information
    /// about packages, an implementation of this class will need to be provided.
    /// this class.</remarks>
    public abstract class PackageStore
    {
        // Keep track of ids that are throw in LearningComponentsInternalExceptions
        private class InternalErrorIds
        {
            private InternalErrorIds() { }

            public const string PS1001 = "PS1001";
            public const string PS1002 = "PS1002";
            public const string PS1003 = "PS1003";
            public const string PS1004 = "PS1004";
            public const string PS1005 = "PS1005";
            public const string PS1006 = "PS1006";
            public const string PS1007 = "PS1007";
            public const string PS1008 = "PS1008";
            public const string PS1009 = "PS1009";
        }

        // These are private to minimize the ability of derived classes to depend on 
        // them.
        private LearningStore m_store;

        /// <summary>
        /// Protected constructor. Only subclasses may be instantiated.
        /// </summary>
        /// <param name="learningStore">The LearningStore to keep information regarding packages.</param>
        protected PackageStore(LearningStore learningStore)
        {
            PackageResources.Culture = Thread.CurrentThread.CurrentCulture;
            m_store = learningStore;
        }

        /// <summary>
        /// Gets the LearningStore associated with this PackageStore. The LearningStore
        /// contains the package information.
        /// </summary>
        public LearningStore LearningStore
        {
            get { return m_store; }
        }

        /// <summary>
        /// Registers a package with the SharePointPackageStore. Any changes to the original file after the package is 
        /// registered are not reflected in the store.        
        /// </summary>
        /// <param name="packageReader">A reader for the package.</param>
        /// <param name="packageEnforcement">The settings to determine whether the package should be modified to 
        /// allow it to be added to the store.</param>
        /// <returns>The results of adding the package, including a log of any warnings or errors that occurred in the process.
        /// </returns>
        /// <exception cref="PackageImportException">Thrown if the package could not be registered with the store. The exception may 
        /// include a log indicating a cause for the failure.</exception>
        /// <remarks>The package is read from SharePoint under elevated privileges. The current user does not need access to the 
        /// package in order to register it. However, the current user does need to have permission to add a package to 
        /// LearningStore in order to register it.</remarks>
        public RegisterPackageResult RegisterPackage(PackageReader packageReader, PackageEnforcement packageEnforcement)
        {
            Utilities.ValidateParameterNonNull("packageEnforcement", packageEnforcement);
            Utilities.ValidateParameterNonNull("packageReader", packageReader);

            // Add the reference to the package. The process of importing the package will cause the package reader to cache 
            // the package in the file system.
            AddPackageReferenceResult refResult = AddPackageReference(packageReader, packageReader.UniqueLocation,  packageEnforcement);  
            return new RegisterPackageResult(refResult);
        }

        /// <summary>
        /// Creates and executes a LearningStoreJob to add a package reference to the PackageStore. 
        /// </summary>
        /// <param name="packageReader">The package to be added to PackageStore. This is required.</param>
        /// <param name="location">The value of the Location field of the package. This should uniquely identify the 
        /// location of the package so that it can be used to retrieve files from the package at a later time. The value of the 
        /// location may be updated later using <Mth>UpdatePackageLocation</Mth>. The location value may 
        /// not be a null or empty string.</param>
        /// <param name="packageEnforcement">The settings to determine whether data should be modified in order 
        /// to enable the package reference to be added. Requesting the data to be modified will increase the likelihood the 
        /// package reference will be added successfully, however it may cause unexpected problems later when the package 
        /// is executed.</param>
        /// <returns>The results of the addition of the reference, including a log containing any warnings 
        /// encountered during the process.</returns>
        /// <remarks>
        /// <p>This method validates the package according the to requested manifestSettings. If the validation results in 
        /// any errors, the package reference will not be added. The reason for the error will be available in the log which 
        /// is included in the exception.
        /// The validation process will stop at the first error, so if the process is stopped,
        /// the log may not contain a complete listing of all problems in the package.</p>
        /// </remarks>
        /// <exception cref="PackageImportException">Thrown if the package reference could not be added.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageReader"/> or <paramref name="location"/>
        /// or <paramref name="packageEnforcement"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="location"/> is not provided. </exception>
        protected AddPackageReferenceResult AddPackageReference(PackageReader packageReader, string location, PackageEnforcement packageEnforcement)
        {
            // Validate parameters 
            Utilities.ValidateParameterNonNull("packageReader", packageReader);
            Utilities.ValidateParameterNonNull("location", location);
            Utilities.ValidateParameterNonNull("packageEnforcement", packageEnforcement);
            Utilities.ValidateParameterNotEmpty("location", location);
            
            // Create ManifestReaderSettings from the packageEnforcement information
            ManifestReaderSettings manifestSettings = new ManifestReaderSettings(
                !packageEnforcement.EnforceScormRequirements, !packageEnforcement.EnforceMlcRequirements);

            // Validate package before adding reference. Determine the proper PackageValidationSettings based on 
            // whether or not the caller has requested to try to fix the values in the package to allow import.
            ValidationBehavior scormRequirementValidation;
            ValidationBehavior mlcRequirementValidation;
            ValidationBehavior lrmRequirementValidation;
            if (manifestSettings.FixScormRequirementViolations)
                scormRequirementValidation = ValidationBehavior.LogWarning;
            else
                scormRequirementValidation = ValidationBehavior.Enforce;

            if (manifestSettings.FixMlcRequirementViolations)
                mlcRequirementValidation = ValidationBehavior.LogWarning;
            else
                mlcRequirementValidation = ValidationBehavior.Enforce;

            if (packageEnforcement.EnforceLrmRequirements)
                lrmRequirementValidation = ValidationBehavior.Enforce;
            else
                lrmRequirementValidation = ValidationBehavior.LogWarning;

            PackageValidatorSettings pvSettings = new PackageValidatorSettings(scormRequirementValidation, ValidationBehavior.None, mlcRequirementValidation,
                lrmRequirementValidation);

            // Do the validation
            ManifestReader manifestReader;
            ValidationResults log = new ValidationResults();
            try
            {
                bool fixLrmErrors = !packageEnforcement.EnforceLrmRequirements;
                PackageValidator.Validate(pvSettings, packageReader, true, log, manifestSettings, fixLrmErrors, out manifestReader);
            }
            catch (InvalidPackageException e)
            {
                throw new PackageImportException(PackageResources.PS_ValidationFailed, e, log);
            }

            // At this point, we know the package is valid (enough) to add to the store, so do it

            PackageItemIdentifier packageId;    // assigned by adding the item row
                             
            LearningStoreJob job = LearningStore.CreateJob();

            // Check security
            job.DemandRight(Schema.AddPackageReferenceRight.RightName);
            job.DisableFollowingSecurityChecks();
                        
            Dictionary<string, object> packageProperties = new Dictionary<string, object>(10);

            // Add the package row and get the new packageId back from the package.
            using (XmlReader manifestXmlReader = manifestReader.CreateNavigator().ReadSubtree())
            {
                packageProperties[Schema.PackageItem.Manifest] = LearningStoreXml.CreateAndLoad(manifestXmlReader);
                packageProperties[Schema.PackageItem.PackageFormat] = manifestReader.PackageFormat;
                packageProperties[Schema.PackageItem.Location] = location;
                LearningStoreItemIdentifier tmpPackageId = job.AddItem(Schema.PackageItem.ItemTypeName, packageProperties, true);

                // Add manifest information to LearningStore
                ImportManifest(job, tmpPackageId, manifestReader);

                // Execute the job and get the results
                ReadOnlyCollection<object> packageJobResults = job.Execute();
                packageId = new PackageItemIdentifier((LearningStoreItemIdentifier)packageJobResults[0]);
            }
            return new AddPackageReferenceResult(packageId, log);

        }

        /// <summary>
        /// Update the package reference to a new location;
        /// </summary>
        /// <param name="packageId">The unique identifier of the package to update.</param>
        /// <param name="location">The new location of the package reference. This may not be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="location"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageId"/> or <paramref name="location"/>
        /// are not provided.</exception>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if <paramref name="packageId"/> does not 
        /// represent a package in LearningStore.</exception>
        // Justification: The method actually requires the value to represent a package. Better to be type safe here.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected void UpdatePackageLocation(PackageItemIdentifier packageId, string location)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);
            Utilities.ValidateParameterNonNull("location", location);
            Utilities.ValidateParameterNotEmpty("location", location);
                   
            LearningStoreJob job = LearningStore.CreateJob();

            // Check security
            job.DemandRight(Schema.AddPackageReferenceRight.RightName);
            job.DisableFollowingSecurityChecks();

            Dictionary<string, object> packageProperties = new Dictionary<string, object>(10);

            packageProperties[Schema.PackageItem.Location] = location;

            job.UpdateItem(packageId, packageProperties);

            job.Execute();       
        }

        /// <summary>
        /// Creates and executes a LearningStoreJob that removes the package referenced by packageId from PackageStore.
        /// This action is permanent and deleted information cannot be recovered.
        /// </summary>
        /// <param name="packageId">The package reference to remove.</param>
        /// <returns>The Location value of the package. The derived class should take appropriate action to 
        /// clean up any cached files in this location.</returns>
        /// <remarks>In order to remove the package reference, there must no other dependencies on the package. For instance, 
        /// if the package has been executed, the resulting attempt information must be removed from LearningStore
        /// before the package reference may be removed.</remarks>
        /// <exception cref="LearningStoreConstraintViolationException">Thrown if the package cannot be deleted 
        /// from LearningStore because there are other items in the store that depend upon it. For instance,
        /// a package for which there are attempts in LearningStore, cannot be deleted.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageId"/> is not provided.</exception>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if <paramref name="packageId"/> does not represent a package 
        /// in LearningStore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected string RemovePackageReference(PackageItemIdentifier packageId)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);

            string packageLocation;

            // Delete the package from LearningStore. In the same job, get the Location of the package.
            LearningStoreJob job = LearningStore.CreateJob();
            Dictionary<string,object> parameters = new Dictionary<string,object>();
            parameters[Schema.RemovePackageReferenceRight.PackageId] = packageId;
            job.DemandRight(Schema.RemovePackageReferenceRight.RightName, parameters);
            job.DisableFollowingSecurityChecks();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.PackageItem.ItemTypeName);
            query.AddColumn(Schema.PackageItem.Location);
            query.AddCondition(Schema.PackageItem.Id, LearningStoreConditionOperator.Equal, packageId);
            job.PerformQuery(query);

            job.DeleteItem(packageId);
            DataTable results;
            try
            {
                 results = job.Execute<DataTable>();
            }
            catch(LearningStoreItemNotFoundException)
            {
                // Throw the same exception with a more helpful message.
                string message = String.Format(CultureInfo.CurrentCulture, PackageResources.FSPS_InvalidPackageId, packageId.GetKey().ToString(NumberFormatInfo.CurrentInfo));
                throw new LearningStoreItemNotFoundException(message);
            }
            packageLocation = (string)results.Rows[0][Schema.PackageItem.Location];
                    
            return packageLocation;
        }

        /// <summary>
        /// Returns a <c>PackageReader</c> that can read files from a package in 
        /// this store. 
        /// </summary>
        /// <param name="packageId">The identifier of the package to be read.</param>
        /// <returns>A PackageReader that can read files from the requested package.</returns>
        /// <remarks>
        /// <p/>It is recommended that derived classes of PackageStore also implement a 
        /// derived class of PackageReader that is capable of reading files on behalf of the subclassed 
        /// PackageStore.
        /// </remarks>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if <paramref name="packageId"/>
        /// does not represent a package in the store.</exception>
        public abstract PackageReader GetPackageReader(PackageItemIdentifier packageId);

        /// <summary>
        /// Internal function that allows StoredLearningSession to reduce database calls. 
        /// </summary>
        /// <param name="packageId">The id of the package to read.</param>
        /// <param name="packageLocation">The value of the Location column in LearningStore for this package.
        /// Note that this may not be validated by the derived class as matching <paramref name="packageId"/>. 
        /// If it does not match the packageId, 
        /// the results are unpredictable, but the package should be read from this location.</param>
        /// <returns>A reader for the requested package.</returns>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if <paramref name="packageId"/>
        /// does not represent a package in the store.</exception>
        protected internal abstract PackageReader GetPackageReader(PackageItemIdentifier packageId, string packageLocation);

        /// <summary>
        /// Adds information to a LearningStoreJob to import manifest information into LearningStore. 
        /// </summary>
        /// <param name="job">The LearningStore job to add the manifest information to.</param>
        /// <param name="packageId">The id of the package in the Packages table to update with the
        /// manifest information. The package information must already be in LearningStore. </param>
        /// <param name="manifest">The manifest to import.</param>
        /// <remarks>
        /// <p>This method does not update the PackageItem table.</p>
        /// </remarks>
        /// <exception cref="PackageImportException">Thrown if the manifest does not contain at least one 
        /// &lt;organization&gt; node that contains one &lt;item&gt; node.</exception>
        /// <exception cref="InvalidPackageException">Thrown if the <paramref name="manifest"/> detects 
        /// an error in the manifest.</exception>
        private void ImportManifest(LearningStoreJob job, LearningStoreItemIdentifier packageId, ManifestReader manifest)
        {
            Utilities.ValidateParameterNonNull("manifest", manifest);

            if (!PackageHasActivities(manifest))
                throw new PackageImportException(PackageResources.PS_NoOrganization);

            // Add all the resource entries. Then, as we traverse through the activity tree, only resources 
            // that are embedded within the activity tree will be additional. Resources that do not have id's 
            // are not added to this table, and therefore not added to the ResourceItem table
            Dictionary<string, LearningStoreItemIdentifier> resourceTableEntries
                                    = new Dictionary<string, LearningStoreItemIdentifier>(20);

            foreach (KeyValuePair<string, ResourceNodeReader> kvResource in manifest.Resources)
            {
                ResourceNodeReader resource = kvResource.Value;
                if (!String.IsNullOrEmpty(resource.Id))
                {
                    resourceTableEntries.Add(resource.Id, AddResourceToJob(job, packageId, resource));
                }
            }

            // Add organizations to LearningStore
            int placement = 0;  // which organization in the manifest?
            foreach (OrganizationNodeReader org in manifest.Organizations)
            {
                AddOrganization(job, manifest.PackageFormat, packageId, org, placement++, resourceTableEntries);
            }
        }

        /// <summary>
        /// Helper function. Returns true if the manifest contains at least one activity.
        /// </summary>
        internal static bool PackageHasActivities(ManifestReader manifest)
        {
            foreach (OrganizationNodeReader org in manifest.Organizations)
            {
                if (org.Activities.Count > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add the information in <paramref name="resource"/> to the LearningStoreJob. If the resource 
        /// does not have an id, it will not be added.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="packageId"></param>
        /// <param name="resource"></param>
        /// <returns>Returns the resource id that was added to the job.</returns>
        private static LearningStoreItemIdentifier AddResourceToJob(LearningStoreJob job, LearningStoreItemIdentifier packageId, ResourceNodeReader resource)
        {
            Dictionary<string, object> resProperties = new Dictionary<string, object>(5);
            resProperties[Schema.ResourceItem.PackageId] = packageId;
            using (XmlReader xmlReader = resource.CreateNavigator().ReadSubtree())
            {
                resProperties[Schema.ResourceItem.ResourceXml] = LearningStoreXml.CreateAndLoad(xmlReader);
            }
            return job.AddItem(Schema.ResourceItem.ItemTypeName, resProperties, true);
        }

        /// <summary>
        /// Add the organization information to a LearningStoreJob that will add it to LS.
        /// </summary>
        /// <param name="job">The organization information is added to this job.</param>
        /// <param name="packageId">The packageId reference for the organization.</param>
        /// <param name="packageFormat">The format of the package.</param>
        /// <param name="orgReader">The organization to add.</param>
        /// <param name="placement">The order of this organization in the manifest.</param>
        /// <param name="resourcesToAdd">The list of resources that will be added to the store.
        /// The key is the id from the manifest, the value is the corresponding LearningStoreItemId.</param>
        private void AddOrganization(LearningStoreJob job, PackageFormat packageFormat,
            LearningStoreItemIdentifier packageId,
             OrganizationNodeReader orgReader, int placement,
            Dictionary<string, LearningStoreItemIdentifier> resourcesToAdd)
        {
            Dictionary<string, object> orgProperties = new Dictionary<string, object>(10);
           
            // Add the organization to the ActivityPackageItem table.

            // OrganizationId can never be null -- manifest reader would have removed it
            orgProperties[Schema.ActivityPackageItem.ActivityIdFromManifest] = orgReader.Id;
            orgProperties[Schema.ActivityPackageItem.OriginalPlacement] = placement;
            orgProperties[Schema.ActivityPackageItem.PackageId] = packageId;
            orgProperties[Schema.ActivityPackageItem.Title] = GetDefaultTitle(orgReader.Title, orgReader.Id);
            orgProperties[Schema.ActivityPackageItem.ObjectivesGlobalToSystem] = orgReader.ObjectivesGlobalToSystem;
            orgProperties[Schema.ActivityPackageItem.DataModelCache] = GetStaticDataModelCache(orgReader, placement);
            // All other values in ActivityPackageItem don't apply to organizations, so use the defaults. (Note that some 
            // entries in ActivityPackageItem map to sequencing information, if it exists. Those entries will be added 
            // later.)

            LearningStoreItemIdentifier orgId = job.AddItem(Schema.ActivityPackageItem.ItemTypeName, orgProperties, true);

            // This represents the order of the activity relative to its peers. Basically, it's order in the xml.
            int activityPlacement = 0;
            Dictionary<string, LearningStoreItemIdentifier> globalObjectivesToAdd = null;

            if (packageFormat == PackageFormat.V1p3)
            {
                // Keep a list of items that are being 'AddOrUpdate'(d) to the GlobalObjectives table. The reason is that as we
                // are traversing the activity tree, multiple activities may map to the same global objective. In that case we do
                // not want to make multiple calls to AddOrUpdate -- as that would do a linear search through the GlobalObjective
                // table. 
                globalObjectivesToAdd = new Dictionary<string, LearningStoreItemIdentifier>(100);

                // If there is sequencing information, add the related objectives
                if (orgReader.Sequencing != null)
                {
                    AddSequencing(job, orgId, orgReader, orgReader.Sequencing,
                                        globalObjectivesToAdd, orgProperties, orgId);
                }
            }

            // Add all the activities.
            foreach (ActivityNodeReader activityReader in orgReader.Activities)
            {
                AddActivityTree(job, packageFormat, orgId, orgId, packageId, orgReader,
                    activityReader, activityPlacement++, resourcesToAdd, globalObjectivesToAdd);
            }
        }

        /// <summary>
        /// Helper function to get a title for an activity or organization. If it is not in the manifest,
        /// then format the string for the default case.
        /// </summary>
        /// <param name="title">The title, as returned from ManifestReader classes. If this is empty,
        /// the default will be constructed. If it is not empty, it is returned unchanged.</param>
        /// <param name="id">The id of the element of which the title is being determined. If the 
        /// <paramref name="title"/> is not provided, this becomes part of the default title.</param>
        /// <returns>The title to use.</returns>
        private static string GetDefaultTitle(string title, string id)
        {
            if (String.IsNullOrEmpty(title))
            {
                // Ids can be much longer than titles, so truncate the id to fit in the title column.
                int maxTitleLen = BaseSchemaInternal.ActivityPackageItem.MaxTitleLength;
                if (id.Length > maxTitleLen)
                    return id.Substring(0, maxTitleLen);
                else
                    return id;
            }
            return title;
        }

        /// <summary>
        /// Add an activity tree (this node and it's children) to a LearningStoreJob. This function is 
        /// recursive -- it adds the <paramref name="activityReader"/> then adds it's children by calling 
        /// this method.
        /// </summary>
        /// <param name="job">The LearningStoreJob to add the tree to.</param>
        /// <param name="packageFormat">The format of the package.</param>
        /// <param name="orgId">The organization id that is the root of this activity tree.</param>
        /// <param name="parentId">Parent of the activity to add. Must reference an ActivityPackageItem
        /// that was previously added to the job.</param>
        /// <param name="packageId">The package id that represents the package that contains this activity</param>
        /// <param name="orgReader">The organization which this activity is a member of.</param>
        /// <param name="activityReader">The root of the tree of activities to add. This activity will be added 
        /// as part of the tree.</param>
        /// <param name="placement">The ordinal of this activity relative to its peers. This is 0-based.</param>
        /// <param name="resourcesToAdd">The list of resource nodes that have already been added to LearningStore.
        /// The key is the resource id from the manifest. The value is the LearningStoreItemIdentier of the 
        /// resource in LearningStore.</param>
        /// <param name="globalObjectivesToAdd">The list of GlobalObjectives to be added to LearningStore.
        /// The key is the objective id from the manifest. The value is the LearningStoreItemIdentifier of the 
        /// global objective in LearningStore. This value may be null for 1.2 and LRM content.</param>
        private void AddActivityTree(LearningStoreJob job, PackageFormat packageFormat,
                            LearningStoreItemIdentifier orgId,
                            LearningStoreItemIdentifier parentId,
                            LearningStoreItemIdentifier packageId,
                            OrganizationNodeReader orgReader,
                            ActivityNodeReader activityReader,
                            int placement,
                            Dictionary<string, LearningStoreItemIdentifier> resourcesToAdd,
                            Dictionary<string, LearningStoreItemIdentifier> globalObjectivesToAdd)
        {
            Dictionary<string, object> actProperties = new Dictionary<string, object>(20);
            bool hasSequencingInfo = (activityReader.Sequencing != null);

            // Add the activity to the ActivityPackageItem table
            actProperties[Schema.ActivityPackageItem.ActivityIdFromManifest] = activityReader.Id;
            actProperties[Schema.ActivityPackageItem.CompletionThreshold] = activityReader.CompletionThreshold;
            actProperties[Schema.ActivityPackageItem.HideAbandon] = activityReader.HideAbandonUI;
            actProperties[Schema.ActivityPackageItem.HideContinue] = activityReader.HideContinueUI;
            actProperties[Schema.ActivityPackageItem.HidePrevious] = activityReader.HidePreviousUI;
            actProperties[Schema.ActivityPackageItem.HideExit] = activityReader.HideExitUI;
            actProperties[Schema.ActivityPackageItem.IsVisibleInContents] = activityReader.IsVisible;
            actProperties[Schema.ActivityPackageItem.LaunchData] = activityReader.DataFromLms;
            actProperties[Schema.ActivityPackageItem.OriginalPlacement] = placement;
            actProperties[Schema.ActivityPackageItem.PackageId] = packageId;
            actProperties[Schema.ActivityPackageItem.ParentActivityId] = parentId;
            actProperties[Schema.ActivityPackageItem.ResourceParameters] = activityReader.ResourceParameters;
            actProperties[Schema.ActivityPackageItem.TimeLimitAction] = activityReader.TimeLimitAction;
            actProperties[Schema.ActivityPackageItem.Title] = GetDefaultTitle(activityReader.Title, activityReader.Id);
            actProperties[Schema.ActivityPackageItem.DataModelCache] = GetStaticDataModelCache(packageFormat, activityReader, placement);
                        
            if (packageFormat == PackageFormat.V1p2)
            {
                if (activityReader.MaximumTimeAllowed != null)
                    actProperties[Schema.ActivityPackageItem.MaxTimeAllowed] = activityReader.MaximumTimeAllowed.Value.Ticks;
                if (activityReader.MasteryScore != null)
                    actProperties[Schema.ActivityPackageItem.MasteryScore] = activityReader.MasteryScore;
            }
            else if (packageFormat == PackageFormat.V1p3)
            {
                if (hasSequencingInfo)
                {
                    actProperties[Schema.ActivityPackageItem.MaxTimeAllowed] = activityReader.Sequencing.AttemptAbsoluteDurationLimit.Ticks;
                }
            }

            // Add the activity's resource to the ResourceItem table. 
            if (activityReader.Resource != null)
            {
                // If the resource is invalid, it might not have an id. In that case, don't add the resource to the tables.
                if (!String.IsNullOrEmpty(activityReader.Resource.Id))
                {
                    if (resourcesToAdd.ContainsKey(activityReader.Resource.Id))
                    {
                        // Assign the id to this activity.
                        actProperties[Schema.ActivityPackageItem.ResourceId] = resourcesToAdd[activityReader.Resource.Id];
                    }
                }

                if (activityReader.Resource.EntryPoint != null)
                {
                    actProperties[Schema.ActivityPackageItem.PrimaryResourceFromManifest] = activityReader.Resource.EntryPoint.ToString();
                }
            }

            // Add the activity
            LearningStoreItemIdentifier activityId = job.AddItem(Schema.ActivityPackageItem.ItemTypeName, actProperties, true);

            // Add activity objectives, if they exist
            if (hasSequencingInfo)
            {
                actProperties.Clear();
                AddSequencing(job, orgId, orgReader, activityReader.Sequencing, globalObjectivesToAdd, actProperties, activityId);
            }

            // Now add the activity's children (recursively call this function)
            int childPlacement = 0;
            foreach (ActivityNodeReader child in activityReader.ChildActivities)
            {
                AddActivityTree(job, packageFormat, orgId, activityId,
                    packageId, orgReader, child, childPlacement++, resourcesToAdd, globalObjectivesToAdd);
            }
        }

        /// <summary>
        /// Helper function to add sequencing information (mainly objectives) to a job. This is only called for SCORM 2004 content.
        /// </summary>
        /// <param name="job">The job to add the information to.</param>
        /// <param name="orgId">The organization which includes the sequencing information. 
        /// </param>
        /// <param name="orgReader">The reader for the associated organization.</param>
        /// <param name="sequencingReader">The reader for the sequencing node to add.</param>
        /// <param name="globalObjectivesToAdd">The collection of global objectives that are added in the 
        /// package.</param>
        /// <param name="actProperties">The properties collection for the ActivityPackageItem that is 
        /// associated with the sequencing information. The job will be updated to include these properties.
        /// </param>
        /// <param name="activityPackageItemId">The activityPackageItem to associate with this sequencing
        /// information.</param>
        private static void AddSequencing(LearningStoreJob job,
                                            LearningStoreItemIdentifier orgId,
                                            OrganizationNodeReader orgReader,
                                            SequencingNodeReader sequencingReader,
                                            Dictionary<string, LearningStoreItemIdentifier> globalObjectivesToAdd,
                                            Dictionary<string, object> actProperties,
                                            LearningStoreItemIdentifier activityPackageItemId)
        {
            Dictionary<string, object> objProperties = new Dictionary<string, object>(10);
            foreach (KeyValuePair<string, SequencingObjectiveNodeReader> kvPair in sequencingReader.Objectives)
            {
                SequencingObjectiveNodeReader objective = kvPair.Value;

                // Add ActivityObjective entry
                objProperties[Schema.ActivityObjectiveItem.ActivityPackageId] = activityPackageItemId;
                objProperties[Schema.ActivityObjectiveItem.IsPrimaryObjective] = objective.IsPrimaryObjective;
                objProperties[Schema.ActivityObjectiveItem.MinNormalizedMeasure] = objective.MinimumNormalizedMeasure;
                objProperties[Schema.ActivityObjectiveItem.SatisfiedByMeasure] = objective.SatisfiedByMeasure;
                objProperties[Schema.ActivityObjectiveItem.Key] = objective.Id;
                LearningStoreItemIdentifier activityObjId = job.AddItem(Schema.ActivityObjectiveItem.ItemTypeName, objProperties, true);

                // Add properties to ActivityPackageItem
                if (objective.IsPrimaryObjective)
                {
                    actProperties[Schema.ActivityPackageItem.PrimaryObjectiveId] = activityObjId;
                    actProperties[Schema.ActivityPackageItem.ScaledPassingScore] = objective.MinimumNormalizedMeasure;
                }

                Dictionary<string, object> mapProperties = new Dictionary<string, object>(10);
                foreach (SequencingObjectiveMapNodeReader map in objective.Mappings)
                {
                    LearningStoreItemIdentifier globalObjId;

                    // If the target objective id doesn't exist, don't add the mapping
                    if (!String.IsNullOrEmpty(map.TargetObjectiveId))
                    {

                        // If this global objective has not been added yet, add it now.
                        if (!globalObjectivesToAdd.ContainsKey(map.TargetObjectiveId))
                        {
                            // the 3 is arbitrary -- slightly bigger than current requirements
                            Dictionary<string, object> uniqueValues = new Dictionary<string, object>(3);
                            Dictionary<string, object> updateValues = new Dictionary<string, object>(3);
                            Dictionary<string, object> propertyValues = new Dictionary<string, object>(3);

                            uniqueValues[Schema.GlobalObjectiveItem.Key] = map.TargetObjectiveId;

                            // Add the reference to the orgId if this global objective should apply only to the package.
                            // Otherwise, add the null value in that column to see if any other package has already
                            // added the objective.
                            if (orgReader.ObjectivesGlobalToSystem)
                            {
                                uniqueValues[Schema.GlobalObjectiveItem.OrganizationId] = null;
                            }
                            else
                            {
                                uniqueValues[Schema.GlobalObjectiveItem.OrganizationId] = orgId;
                            }
                            globalObjId = job.AddOrUpdateItem(Schema.GlobalObjectiveItem.ItemTypeName, uniqueValues, updateValues, propertyValues);
                            globalObjectivesToAdd.Add(map.TargetObjectiveId, globalObjId);
                        }
                        else
                        {
                            globalObjId = globalObjectivesToAdd[map.TargetObjectiveId];
                        }

                        // Add MapActivityObjectiveToGlobalObjective entry
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.GlobalObjectiveId] = globalObjId;
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.ActivityObjectiveId] = activityObjId;
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.ReadNormalizedMeasure] = map.ReadNormalizedMeasure;
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.ReadSatisfiedStatus] = map.ReadSatisfiedStatus;
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.WriteNormalizedMeasure] = map.WriteNormalizedMeasure;
                        mapProperties[Schema.MapActivityObjectiveToGlobalObjectiveItem.WriteSatisfiedStatus] = map.WriteSatisfiedStatus;
                        job.AddItem(Schema.MapActivityObjectiveToGlobalObjectiveItem.ItemTypeName, mapProperties);
                        mapProperties.Clear();

                    }   // end: map.TargetObjectiveId must exist

                }   // end: sequencing map node loop

            } // end: sequencing objective node loop

            // Add ActivityPackageItem entries from sequencing node
            actProperties[Schema.ActivityPackageItem.MaxAttempts] = sequencingReader.AttemptLimit;
            actProperties[Schema.ActivityPackageItem.MaxTimeAllowed] = sequencingReader.AttemptAbsoluteDurationLimit.Ticks;
            job.UpdateItem(activityPackageItemId, actProperties);
            actProperties.Clear();
        }

        /// <summary>
        /// Create the cache of static data relating to the data model for an item node. 
        /// This is per-activity information that does not 
        /// change based on attempts.
        /// </summary>
        /// <param name="packageFormat"></param>
        /// <param name="activityNode"></param>
        /// <param name="originalPlacement"></param>
        /// <returns></returns>
        private static LearningStoreXml GetStaticDataModelCache(PackageFormat packageFormat, ActivityNodeReader activityNode, int originalPlacement)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("item"));
            XPathNavigator docNav = root.CreateNavigator();
            XmlWriter attributes = docNav.CreateAttributes();

            attributes.WriteAttributeString("title", GetDefaultTitle(activityNode.Title, activityNode.Id));
            attributes.WriteAttributeString("id", activityNode.Id);
            attributes.WriteAttributeString("origPlacement", XmlConvert.ToString(originalPlacement));
            if (!activityNode.IsVisible)
            {
                attributes.WriteAttributeString("isvisible", XmlConvert.ToString(false));
            }
            if (activityNode.TimeLimitAction != TimeLimitAction.ContinueNoMessage)
            {
                attributes.WriteAttributeString("timeLimitAction", Enum.GetName(typeof(TimeLimitAction), activityNode.TimeLimitAction));
            }
            if (!String.IsNullOrEmpty(activityNode.DataFromLms))
            {
                attributes.WriteAttributeString("dataFromLMS", activityNode.DataFromLms);
            }
            if (activityNode.CompletionThreshold.HasValue)
            {
                attributes.WriteAttributeString("completionThreshold", XmlConvert.ToString(activityNode.CompletionThreshold.Value));
            }

            ResourceNodeReader res = activityNode.Resource;

            if ((res != null) && (res.EntryPoint != null))
            {
                attributes.WriteAttributeString("resourceId", res.Id);
                attributes.WriteAttributeString("parameters", activityNode.ResourceParameters);
                attributes.WriteAttributeString("resourceType", Enum.GetName(typeof(ResourceType), res.ResourceType));
                attributes.WriteAttributeString("defaultResourceFile", res.EntryPoint.ToString());
                if (res.XmlBase != null)
                {
                    attributes.WriteAttributeString("xmlBase", res.XmlBase.ToString());
                }
            }

            if (activityNode.HideAbandonUI)
            {
                attributes.WriteAttributeString("hideAbandonUI", XmlConvert.ToString(true));
            }
            if (activityNode.HideContinueUI)
            {
                attributes.WriteAttributeString("hideContinueUI", XmlConvert.ToString(true));
            }
            if (activityNode.HideExitUI)
            {
                attributes.WriteAttributeString("hideExitUI", XmlConvert.ToString(true));
            }
            if (activityNode.HidePreviousUI)
            {
                attributes.WriteAttributeString("hidePreviousUI", XmlConvert.ToString(true));
            }
            if (packageFormat == PackageFormat.V1p2)
            {
                float? masteryScore = activityNode.MasteryScore;
                if (masteryScore != null)
                {
                    attributes.WriteAttributeString("masteryScore", XmlConvert.ToString((float)masteryScore));
                }

                TimeSpan? maxTimeAllowed = activityNode.MaximumTimeAllowed;
                if (maxTimeAllowed != null)
                {
                    attributes.WriteAttributeString("maxTimeAllowed", XmlConvert.ToString((TimeSpan)maxTimeAllowed));
                }
            }

            attributes.Close();

            if (packageFormat == PackageFormat.V1p2)    
            {
                string prereqs = activityNode.Prerequisites;
                if (!String.IsNullOrEmpty(prereqs))
                {
                    docNav.AppendChildElement("adlcp", "prerequisites", "http://www.adlnet.org/xsd/adlcp_rootv1p2", prereqs);
                }                
            }
            else if (packageFormat == PackageFormat.V1p3)  // package format is 2004
            {
                if (activityNode.Sequencing != null)
                {
                    docNav.AppendChild(GetSequencingNode(activityNode.Sequencing));
                }
            }

            LearningStoreXml retVal;
            using (XmlReader xmlReader = doc.CreateNavigator().ReadSubtree())
            {
                retVal = LearningStoreXml.CreateAndLoad(xmlReader);
            }
            return retVal;
        }

        /// <summary>
        /// Create the cache of static data relating to the data model for an 
        /// organization node. This is per-organization information that does not 
        /// change based on attempts.
        /// </summary>
        /// <param name="orgNode"></param>
        /// <param name="originalPlacement"></param>
        /// <returns></returns>
        private static LearningStoreXml GetStaticDataModelCache(OrganizationNodeReader orgNode, int originalPlacement)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("item"));
            XPathNavigator docNav = root.CreateNavigator();
            XmlWriter attributes = docNav.CreateAttributes();

            attributes.WriteAttributeString("title", GetDefaultTitle(orgNode.Title, orgNode.Id));
            attributes.WriteAttributeString("id", orgNode.Id);
            attributes.WriteAttributeString("origPlacement", XmlConvert.ToString(originalPlacement));
            attributes.Close();

            if (orgNode.Sequencing != null)
            {
                docNav.AppendChild(GetSequencingNode(orgNode.Sequencing));
            }

            LearningStoreXml retVal;
            using(XmlReader xmlReader = doc.CreateNavigator().ReadSubtree())
            {
                retVal = LearningStoreXml.CreateAndLoad(xmlReader);
            }
            return retVal;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]   // It's complex node parsing.
        private static XPathNavigator GetSequencingNode(SequencingNodeReader seqReader)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.CreateNavigator().NameTable);

            // SCORM 2004 namespaces
            nsMgr.AddNamespace("imscp", "http://www.imsglobal.org/xsd/imscp_v1p1");
            nsMgr.AddNamespace("adlcp", "http://www.adlnet.org/xsd/adlcp_v1p3");
            nsMgr.AddNamespace("lom", "http://ltsc.ieee.org/xsd/LOM");
            nsMgr.AddNamespace("imsss", "http://www.imsglobal.org/xsd/imsss");
            nsMgr.AddNamespace("adlseq", "http://www.adlnet.org/xsd/adlseq_v1p3");
            nsMgr.AddNamespace("adlnav", "http://www.adlnet.org/xsd/adlnav_v1p3");

            XmlNode sequencingNode = doc.CreateElement("imsss:sequencing", "http://www.imsglobal.org/xsd/imsss");

            bool nodeAdded = false;
            if (!seqReader.Choice)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "choice", "false", ref nodeAdded);

            if (!seqReader.ChoiceExit)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "choiceExit", "false", ref nodeAdded);

            if (seqReader.Flow)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "flow", "true", ref nodeAdded);

            if (seqReader.ForwardOnly)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "forwardOnly", "true", ref nodeAdded);

            if (!seqReader.UseCurrentAttemptObjectiveInfo)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "useCurrentAttemptObjectiveInfo", "false", ref nodeAdded);

            if (!seqReader.UseCurrentAttemptProgressInfo)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "controlMode", "useCurrentAttemptProgressInfo", "false", ref nodeAdded);

            // Add <sequencingRules>
            nodeAdded = false;
            if (seqReader.PreConditionRules.Count > 0)
            {
                WriteSequencingRules(sequencingNode, "preConditionRule", seqReader.PreConditionRules, ref nodeAdded);
            }
            if (seqReader.PostConditionRules.Count > 0)
            {
                WriteSequencingRules(sequencingNode, "postConditionRule", seqReader.PostConditionRules, ref nodeAdded);
            }
            if (seqReader.ExitConditionRules.Count > 0)
            {
                WriteSequencingRules(sequencingNode, "exitConditionRule", seqReader.ExitConditionRules, ref nodeAdded);
            }

            // Add <limitConditions>
            nodeAdded = false;
            int? attemptLimit = seqReader.AttemptLimit;
            if(attemptLimit.HasValue)
            {
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "limitConditions", "attemptLimit", XmlConvert.ToString(attemptLimit.Value), ref nodeAdded);
            }
            if (seqReader.AttemptAbsoluteDurationLimit != TimeSpan.Zero)
            {
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "limitConditions", "attemptAbsoluteDurationLimit", XmlConvert.ToString(seqReader.AttemptAbsoluteDurationLimit), ref nodeAdded);
            }

            // Add <rollupRules> to sequencing node.
            nodeAdded = false;
            if ((!seqReader.RollupObjectiveSatisfied)           // These are the cases where the rollupRules node is required in the cache.
                    || (!seqReader.RollupProgressCompletion) 
                    || (seqReader.ObjectiveMeasureWeight != 1.0)
                    || (seqReader.RollupRules.Count > 0))
            {
                XmlNode rollupRulesNode = AppendImsssChild(sequencingNode, "rollupRules");
                if (!seqReader.RollupObjectiveSatisfied)
                    AddAttribute(rollupRulesNode, "rollupObjectiveSatisfied", seqReader.RollupObjectiveSatisfied ? "true" : "false");
                if (!seqReader.RollupProgressCompletion)
                    AddAttribute(rollupRulesNode, "rollupProgressCompletion", seqReader.RollupProgressCompletion ? "true" : "false");
                if (seqReader.ObjectiveMeasureWeight != 1.0)
                    AddAttribute(rollupRulesNode, "objectiveMeasureWeight", XmlConvert.ToString(seqReader.ObjectiveMeasureWeight));
                
                WriteRollupRules(rollupRulesNode, seqReader.RollupRules);
            }

            // Add <objectives> to sequencing node
            if (seqReader.Objectives.Count > 0)
            {
                XmlNode objectivesNode = AppendImsssChild(sequencingNode, "objectives");

                WriteObjectives(objectivesNode, seqReader.Objectives);
            }

            // Add <randomizationControls> to the sequencing node
            nodeAdded = false;
            if (seqReader.RandomizationTiming != RandomizationTiming.Never)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "randomizationControls", "randomizationTiming", GetRandomizationTiming(seqReader.RandomizationTiming), ref nodeAdded);
            if (seqReader.RandomizationSelectCount != 0)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "randomizationControls", "selectCount", XmlConvert.ToString(seqReader.RandomizationSelectCount), ref nodeAdded);
            if (seqReader.ReorderChildren)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "randomizationControls", "reorderChildren", "true", ref nodeAdded);
            if (seqReader.SelectionTiming != RandomizationTiming.Never)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "randomizationControls", "selectionTiming", GetRandomizationTiming(seqReader.SelectionTiming), ref nodeAdded);

            // Add <deliveryControls> to the sequencing node
            nodeAdded = false;
            if (!seqReader.Tracked)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "deliveryControls", "tracked", "false", ref nodeAdded);
            if (seqReader.CompletionSetByContent)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "deliveryControls", "completionSetByContent", "true", ref nodeAdded);
            if (seqReader.ObjectiveSetByContent)
                WriteNodeWithAttribute(sequencingNode, AppendImsssChild,  "deliveryControls", "objectiveSetByContent", "true", ref nodeAdded);

            // Add <constrainedChoiceConsiderations> element to the sequencing node
            nodeAdded = false;
            if (seqReader.PreventActivation)
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "constrainedChoiceConsiderations", "preventActivation", "true", ref nodeAdded);
            if (seqReader.ConstrainChoice)
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "constrainedChoiceConsiderations", "constrainChoice", "true", ref nodeAdded);

            nodeAdded = false;
            if (seqReader.RequiredForSatisfied != RollupConsideration.Always)
            {
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "rollupConsiderations", "requiredForSatisfied",
                                                GetRollupConsideration(seqReader.RequiredForSatisfied), ref nodeAdded);
            }
            if (seqReader.RequiredForNotSatisfied != RollupConsideration.Always)
            {
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "rollupConsiderations", "requiredForNotSatisfied",
                                                GetRollupConsideration(seqReader.RequiredForNotSatisfied), ref nodeAdded);
            }
            if (seqReader.RequiredForCompleted != RollupConsideration.Always)
            {
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "rollupConsiderations", "requiredForCompleted",
                                                GetRollupConsideration(seqReader.RequiredForCompleted), ref nodeAdded);
            }
            if (seqReader.RequiredForIncomplete != RollupConsideration.Always)
            {
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "rollupConsiderations", "requiredForIncomplete",
                                                GetRollupConsideration(seqReader.RequiredForIncomplete), ref nodeAdded);
            }
            if (!seqReader.MeasureSatisfactionIfActive) // Note: the addendum changed the default value from true to false
            {
                WriteNodeWithAttribute(sequencingNode, AppendAdlseqChild, "rollupConsiderations", "measureSatisfactionIfActive",
                                                    "false", ref nodeAdded);
            }

            doc.AppendChild(sequencingNode);
            return doc.CreateNavigator().SelectSingleNode("/imsss:sequencing", nsMgr);
        }

        // Add a node with an attribute. The node will be written as an imsss node.
        // If the node <nodeName> does not already exist as a child of <parentNode>, it is added.
        // The attribute is added to the new or existing node.
        private static void WriteNodeWithAttribute(XmlNode parentNode, AppendChild appendChildDelegate, string nodeName, string attrName, string value, ref bool nodeAlreadyAdded)
        {
            XmlNode newNode = null;
            if (!nodeAlreadyAdded)
            {
                newNode = appendChildDelegate(parentNode, nodeName);
                nodeAlreadyAdded = true;
            }
            else
            {
                newNode = FindNode(parentNode.ChildNodes, nodeName);
                
                if (newNode == null)
                {
                    // Should never happen
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1001);
                }
            }

            AddAttribute(newNode, attrName, value);
        }

        // Helper function to find a node with a specific name in a list of nodes. This method returns the first one
        // found.
        private static XmlNode FindNode(XmlNodeList nodeList, string nodeName)
        {
            foreach (XmlNode child in nodeList)
            {
                if (child.LocalName == nodeName)
                {
                    return child;
                }
            }
            return null;
        }

        // Add the sequencingRules collection to the sequencingNode. The rules will have the ruleNodeName (such as 'preConditionRules').
        private static void WriteSequencingRules(XmlNode sequencingNode, string ruleNodeName, 
                                ReadOnlyCollection<SequencingRuleNodeReader> rules, ref bool sequencingRulesAdded)
        {
            XmlNode sequencingRulesNode = null;
            if (!sequencingRulesAdded)
            {
                sequencingRulesAdded = true;
                sequencingRulesNode = AppendImsssChild(sequencingNode, "sequencingRules");
            }
            else
            {
                sequencingRulesNode = FindNode(sequencingNode.ChildNodes, "sequencingRules");
                if (sequencingRulesNode == null)
                {
                    // Should never happen
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1002);
                }
            }


            // <imsss:sequencingRules>      --> sequencingRulesNode
            //      <imsss:preConditionRule>    -->newRuleNode (rules contains a list of these)
            //          <imsss:ruleConditions>  --> newConditionsNode
            //              <imsss:ruleCondition condition = "satisfied"/>  --> newConditionNode
            //          </imsss:ruleConditions>    
            //          <imsss:ruleAction action = "disabled"/>  --> actionNode
            //      </imsss:preConditionRule>
            foreach (SequencingRuleNodeReader ruleReader in rules)
            {
                // Create, for instance, <imsss: preConditionRule>
                XmlNode newRuleNode = AppendImsssChild(sequencingRulesNode, ruleNodeName);

                if (ruleReader.Conditions.Count > 0)
                {
                    // Create the ruleConditions node (<imss:ruleConditions>)
                    XmlNode newConditionsNode = AppendImsssChild(newRuleNode, "ruleConditions");
                    AddAttribute(newConditionsNode, "conditionCombination", GetSequencingConditionCombination(ruleReader.Combination));
                    
                    // Create the set of ruleCondition nodes (<imsss:ruleCondition>)
                    foreach (SequencingRuleConditionNodeReader conditionReader in ruleReader.Conditions)
                    {
                        XmlNode newConditionNode = AppendImsssChild(newConditionsNode, "ruleCondition");

                        string objId = conditionReader.ReferencedObjectiveId;
                        if (!String.IsNullOrEmpty(objId))
                        {
                            AddAttribute(newConditionNode, "referencedObjective", objId);
                        }

                        double threshold = conditionReader.MeasureThreshold;
                        AddAttribute(newConditionNode, "measureThreshold", XmlConvert.ToString(threshold));

                        AddAttribute(newConditionNode, "operator", GetSequencingConditionOperator(conditionReader.Operator));
                        
                        AddAttribute(newConditionNode, "condition", GetRuleConditionCondition(conditionReader.Condition));
                    }
                }

                if (ruleReader.Action != SequencingRuleAction.NoAction)
                {
                    // If there is no action, the node should be ignored. In that case, don't add it.

                    // Add ruleAction node (<imsss:ruleAction>)
                    XmlNode actionNode = AppendImsssChild(newRuleNode, "ruleAction");
                    AddAttribute(actionNode, "action", GetAction(ruleReader.Action));
                }
            }   // end: foreach rule
        }

        //Add the rollup rules in <rules> to the <rollupRulesNode>.
        private static void WriteRollupRules(XmlNode rollupRulesNode, ReadOnlyCollection<SequencingRollupRuleNodeReader> rules)
        {
            foreach (SequencingRollupRuleNodeReader ruleNodeReader in rules)
            {
                XmlNode newRollupRule = AppendImsssChild(rollupRulesNode, "rollupRule");
                // if (ruleNodeReader.ChildActivitySet != RollupChildActivitySet.All)
                    AddAttribute(newRollupRule, "childActivitySet", GetChildActivitySet(ruleNodeReader.ChildActivitySet));

                if ((ruleNodeReader.ChildActivitySet == RollupChildActivitySet.AtLeastCount) 
                    || (ruleNodeReader.MinimumCount != 0))
                    AddAttribute(newRollupRule, "minimumCount", XmlConvert.ToString(ruleNodeReader.MinimumCount));

                if ((ruleNodeReader.ChildActivitySet == RollupChildActivitySet.AtLeastPercent) 
                    || (ruleNodeReader.MinimumPercent != 0))
                    AddAttribute(newRollupRule, "minimumPercent", XmlConvert.ToString(ruleNodeReader.MinimumPercent));

                // Add conditions, if there are any (there should be)
                if (ruleNodeReader.Conditions.Count > 0)
                {
                    XmlNode rollupConditionsNode = AppendImsssChild(newRollupRule, "rollupConditions");
                    //if (ruleNodeReader.ConditionCombination != SequencingConditionCombination.Any)
                        AddAttribute(rollupConditionsNode, "conditionCombination", GetSequencingConditionCombination(ruleNodeReader.ConditionCombination));

                    foreach (SequencingRollupConditionNodeReader conditionNode in ruleNodeReader.Conditions)
                    {
                        XmlNode rollupConditionNode = AppendImsssChild(rollupConditionsNode, "rollupCondition");
                        //if (conditionNode.Operator != SequencingConditionOperator.NoOp)
                        AddAttribute(rollupConditionNode, "operator", GetSequencingConditionOperator(conditionNode.Operator));

                        AddAttribute(rollupConditionNode, "condition", GetRollupConditionCondition(conditionNode.Condition));
                    }
                }

                XmlNode actionNode = AppendImsssChild(newRollupRule, "rollupAction");
                AddAttribute(actionNode, "action", GetAction(ruleNodeReader.Action));
            }
        }

        // Write the <objectives> into the objectivesNode.
        private static void  WriteObjectives(XmlNode objectivesNode, IDictionary<string, SequencingObjectiveNodeReader> objectives)
        {
            foreach(KeyValuePair<string, SequencingObjectiveNodeReader> kvp in objectives)
            {
                SequencingObjectiveNodeReader objectiveReader = kvp.Value;
                XmlNode objNode;
                if (objectiveReader.IsPrimaryObjective)
                {
                    objNode = AppendImsssChild(objectivesNode, "primaryObjective");
                }
                else
                {
                    objNode = AppendImsssChild(objectivesNode, "objective");
                }
                if (objectiveReader.SatisfiedByMeasure)
                    AddAttribute(objNode, "satisfiedByMeasure", "true");
                if (!String.IsNullOrEmpty(objectiveReader.Id))
                    AddAttribute(objNode, "objectiveID", objectiveReader.Id);


                if (objectiveReader.MinimumNormalizedMeasure != 1.0)
                {
                    XmlNode node = AppendImsssChild(objNode, "minNormalizedMeasure");
                    node.InnerText = XmlConvert.ToString(objectiveReader.MinimumNormalizedMeasure);
                }

                if (objectiveReader.Mappings.Count > 0)
                {
                    foreach (SequencingObjectiveMapNodeReader mapReader in objectiveReader.Mappings)
                    {
                        XmlNode mapNode = AppendImsssChild(objNode, "mapInfo");
                        AddAttribute(mapNode, "targetObjectiveID", mapReader.TargetObjectiveId);
                        if (!mapReader.ReadNormalizedMeasure)
                            AddAttribute(mapNode, "readNormalizedMeasure", "false");

                        if (!mapReader.ReadSatisfiedStatus)
                            AddAttribute(mapNode, "readSatisfiedStatus", "false");

                        if (mapReader.WriteNormalizedMeasure)
                            AddAttribute(mapNode, "writeNormalizedMeasure", "true");

                        if (mapReader.WriteSatisfiedStatus)
                            AddAttribute(mapNode, "writeSatisfiedStatus", "true");
                    }
                }
            }
        }

        private delegate XmlElement AppendChild(XmlNode parent, string nodeName);

        // Helper function to append a child to <nav> that is in imsss namespace and has no value
        private static XmlElement AppendImsssChild(XmlNode parent, string nodeName)
        {
            XmlElement child = parent.OwnerDocument.CreateElement("imsss:" + nodeName, "http://www.imsglobal.org/xsd/imsss");
            parent.AppendChild(child);
            return child;
        }

        // Helper function to append a child to <nav> that is in adlcp namespace and has no value
        private static XmlElement AppendAdlseqChild(XmlNode parent, string nodeName)
        {
            XmlElement child = parent.OwnerDocument.CreateElement("adlseq:" + nodeName, "http://www.adlnet.org/xsd/adlseq_v1p3");
            parent.AppendChild(child);
            return child;
        }

        // Helper function to add an attribute to an XmlNode.
        private static void AddAttribute(XmlNode parent, string name, string value)
        {
            XmlAttribute attr = parent.OwnerDocument.CreateAttribute(name);
            attr.Value = value;
            parent.Attributes.Append(attr);
        }

        // Return the string for the xml that matches the condition
        private static string GetRuleConditionCondition(SequencingRuleCondition condition)
        {
            switch(condition)
            {
                case SequencingRuleCondition.ActivityProgressKnown:
                    return "activityProgressKnown";
                case SequencingRuleCondition.Attempted:
                    return "attempted";
                case SequencingRuleCondition.AttemptLimitExceeded:
                    return "attemptLimitExceeded";
                case SequencingRuleCondition.Completed:
                    return "completed";
                case SequencingRuleCondition.ObjectiveMeasureGreaterThan:
                    return "objectiveMeasureGreaterThan";
                case SequencingRuleCondition.ObjectiveMeasureKnown:
                    return "objectiveMeasureKnown";
                case SequencingRuleCondition.ObjectiveMeasureLessThan:
                    return "objectiveMeasureLessThan";
                case SequencingRuleCondition.ObjectiveStatusKnown:
                    return "objectiveStatusKnown";
                case SequencingRuleCondition.OutsideAvailableTimeRange:
                    return "outsideAvailableTimeRange";
                case SequencingRuleCondition.Satisfied:
                    return "satisfied";
                case SequencingRuleCondition.TimeLimitExceeded:
                    return "timeLimitExceeded";
                default:
                    return "always";
            }
        }

        private static string GetSequencingConditionCombination(SequencingConditionCombination conditionCombination)
        {
            switch (conditionCombination)
            {
                case SequencingConditionCombination.Any:
                    return "any";
                default:
                    return "all";
            }
        }

        private static string GetSequencingConditionOperator(SequencingConditionOperator conditionOperator)
        {
            switch (conditionOperator)
            {
                case SequencingConditionOperator.Not:
                    return "not";
                default:
                    return "noOp";
            }
        }

        private static string GetRollupConditionCondition(RollupCondition condition)
        {
            switch (condition)
            {
                case RollupCondition.ActivityProgressKnown:
                    return "activityProgressKnown";
                case RollupCondition.Attempted:
                    return "attempted";
                case RollupCondition.AttemptLimitExceeded:
                    return "attemptLimitExceeded";
                case RollupCondition.Completed:
                    return "completed";
                case RollupCondition.ObjectiveMeasureKnown:
                    return "objectiveMeasureKnown";
                case RollupCondition.ObjectiveStatusKnown:
                    return "objectiveStatusKnown";
                case RollupCondition.OutsideAvailableTimeRange:
                    return "outsideAvailableTimeRange";
                case RollupCondition.Satisfied:
                    return "satisfied";
                case RollupCondition.TimeLimitExceeded:
                    return "timeLimitExceeded";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1006);
            }
        }


        private static string GetRollupConsideration(RollupConsideration consideration)
        {
            switch(consideration)
            {
                case RollupConsideration.Always:
                    return "always";
                case RollupConsideration.IfAttempted:
                    return "ifAttempted";
                case RollupConsideration.IfNotSkipped:
                    return "ifNotSkipped";
                case RollupConsideration.IfNotSuspended:
                    return "ifNotSuspended";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1009);
            }
        }

        // Helper function to return a string matching the specified action
        private static string GetAction(SequencingRuleAction action)
        {
            switch(action)
            {
                case SequencingRuleAction.Continue:
                    return "continue";
                case SequencingRuleAction.Disabled:
                    return "disabled";
                case SequencingRuleAction.Exit:
                    return "exit";
                case SequencingRuleAction.ExitAll:
                    return "exitAll";
                case SequencingRuleAction.ExitParent:
                    return "exitParent"; 
                case SequencingRuleAction.HiddenFromChoice:
                    return "hiddenFromChoice";
                case SequencingRuleAction.Previous:
                    return "previous";
                case SequencingRuleAction.Retry:
                    return "retry";
                case SequencingRuleAction.RetryAll:
                    return "retryAll";
                case SequencingRuleAction.Skip:
                    return "skip";
                case SequencingRuleAction.StopForwardTraversal:
                    return "stopForwardTraversal";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1003);
            }
        }

        private static string GetAction(RollupAction action)
        {
            switch(action)
            {
                case RollupAction.Completed:
                    return "completed";
                case RollupAction.Incomplete:
                    return "incomplete";
                case RollupAction.NotSatisfied:
                    return "notSatisfied";
                case RollupAction.Satisfied:
                    return "satisfied";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1007);
            }
        }

        private static string GetChildActivitySet(RollupChildActivitySet activitySet)
        {
            switch(activitySet)
            {
                case RollupChildActivitySet.All:
                    return "all";
                case RollupChildActivitySet.Any:
                    return "any";
                case RollupChildActivitySet.AtLeastCount:
                    return "atLeastCount";
                case RollupChildActivitySet.AtLeastPercent:
                    return "atLeastPercent";
                case RollupChildActivitySet.None:
                    return "none";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1005);
            }
        }

        private static string GetRandomizationTiming(RandomizationTiming timing)
        {
            switch(timing)
            {
                case RandomizationTiming.Never:
                    return "never";
                case RandomizationTiming.Once:
                    return "once";
                case RandomizationTiming.OnEachNewAttempt:
                    return "onEachNewAttempt";
                default:
                    throw new LearningComponentsInternalException(InternalErrorIds.PS1008);

            }
        }

        /// <summary>
        /// Gets the manifest from the specified package in the store. The package must have been 
        /// previously added to the store.
        /// </summary>
        /// <param name="packageId">The package containing the manifest to be read.</param>
        /// <returns>A stream containing the manifest of the package, as it is stored in LearningStore.</returns>
        /// <remarks>
        /// <p/>This method simply returns the imsmanifsest.xml file, as it is stored in the PackageStore. 
        /// It does not update it with information in related columns from LearningStore.
        /// </remarks>
        internal Stream GetManifestFile(PackageItemIdentifier packageId)
        {
            DetachableStream detachStream;
            Stream returnStream;
            using (Disposer disposer = new Disposer())
            {
                LearningStoreXml manifestStoreXml = GetPackageManifestFromLS(packageId);   

                XmlReader reader = manifestStoreXml.CreateReader();
                disposer.Push(reader);

                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                detachStream = new DetachableStream(doc.OuterXml.Length);
                disposer.Push(detachStream);

                using (StreamWriter sw = new StreamWriter(detachStream))
                {
                    sw.Write(doc.OuterXml);
                    sw.Flush();

                    detachStream.Seek(0, SeekOrigin.Begin);
                    detachStream.Detach();
                }
                returnStream = detachStream.Stream;
            }

            return returnStream;
        }

        /// <summary>
        /// Fill in package information from LearningStore. If the packageId is not in LS, this will 
        /// throw a LearningStoreItemNotFound exception. This method enforces LearningStore security. The 
        /// current user must have access to the requested package.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns>The location string.</returns>
        internal string GetPackageLocationFromLS(LearningStoreItemIdentifier packageId)
        {
            LearningStoreJob job = LearningStore.CreateJob();
            // This is basically the security for the PackageStore.GetPackageReader methods.
            // Security on LearningStore is *not* disabled.

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[Schema.RemovePackageReferenceRight.PackageId] = packageId;
            job.DemandRight(Schema.ReadPackageRight.RightName, parameters);
            job.DisableFollowingSecurityChecks();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.PackageItem.ItemTypeName);
            query.AddCondition(Schema.PackageItem.Id, LearningStoreConditionOperator.Equal, packageId);
            query.AddColumn(Schema.PackageItem.Id);
            query.AddColumn(Schema.PackageItem.Location);
            job.PerformQuery(query);
            DataTable results = job.Execute<DataTable>();
            if (results.Rows.Count == 0)
            {
                // Could not find the packageId. Return empty string.
                return String.Empty;
            }
            return (string)results.Rows[0][Schema.PackageItem.Location];
        }

        /// <summary>
        /// Fill in package information from LearningStore. If the packageId is not in LS, this will 
        /// throw a LearningStoreItemNotFound exception.
        /// </summary>
        private LearningStoreXml GetPackageManifestFromLS(LearningStoreItemIdentifier packageId)
        {
            LearningStoreJob job = LearningStore.CreateJob();
            
            // Nobody can call this unless we have verified that
            // they have the right to read the package.  So there's no need to verify that again here.
            job.DisableFollowingSecurityChecks();
            LearningStoreQuery query = LearningStore.CreateQuery(Schema.PackageItem.ItemTypeName);
            query.AddCondition(Schema.PackageItem.Id, LearningStoreConditionOperator.Equal, packageId);
            query.AddColumn(Schema.PackageItem.Id);
            query.AddColumn(Schema.PackageItem.Manifest);
            job.PerformQuery(query);
            DataTable results = job.Execute<DataTable>();
            if (results.Rows.Count == 0)
                throw new LearningStoreItemNotFoundException(String.Format(CultureInfo.CurrentCulture,
                    PackageResources.FSPS_InvalidPackageId, packageId.GetKey().ToString(CultureInfo.CurrentCulture)));

            return (LearningStoreXml)results.Rows[0][Schema.PackageItem.Manifest];
        }
    }

    /// <summary>
    /// Represents the successful results of adding a package reference to PackageStore.
    /// </summary>
    public class AddPackageReferenceResult
    {
        private PackageItemIdentifier m_packageId;
        private ValidationResults m_log;

        /// <summary>
        /// Create an AddPackageReferenceResult.
        /// </summary>
        /// <param name="packageId">The packageId of the added reference. This value is not validated to ensure that it represents 
        /// a package in LearningStore.</param>
        /// <param name="log">The log of requested errors and warnings that occurred during the course of adding the package reference.
        /// This may not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageId"/> or <paramref name="log"/>
        /// are not provided.</exception>
        public AddPackageReferenceResult(PackageItemIdentifier packageId, ValidationResults log)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);
            Utilities.ValidateParameterNonNull("log", log);

            m_packageId = packageId;
            m_log = log;
        }

        /// <summary>
        /// The reference to the package that was added to PackageStore.
        /// </summary>
        public PackageItemIdentifier PackageId { get { return m_packageId;  } }
        
        /// <summary>
        /// The log of errors and warnings that occurred in the course of adding the package reference to PackageStore.
        /// </summary>
        public ValidationResults Log   { get { return m_log;  } }
    }

    /// <summary>
    /// Represents settings relating to enforcing requirements when a package reference is added to a PackageStore.
    /// </summary>
    public class PackageEnforcement
    {
        private bool m_enforceScormRequirements;
        private bool m_enforceMlcRequirements;
        private bool m_enforceLrmRequirements;

        /// <summary>
        /// Create a PackageEnforcement to indicate requirements that will be enforced when a package reference is added to
        /// PackageStore.
        /// </summary>
        /// <param name="enforceScormRequirements">If true, Scorm requirements will be enforced when a package reference is added 
        /// to PackageStore. In this case, any violation of Scorm requirements will cause the import process to fail. If false, an 
        /// attempt will be made to fix the package so that it can be imported. In this case, problems 
        /// may occur when the package is executed.</param>
        /// <param name="enforceMlcRequirements">If true, Microsoft Learning Components requirements will be enforced when a package reference is added 
        /// to PackageStore. In this case, any violation of Microsoft Learning Components requirements will cause the import 
        /// process to fail. If false, an 
        /// attempt will be made to fix the package so that it can be imported. In this case, problems 
        /// may occur when the package is executed.</param>
        /// <param name="enforceLrmRequirements">If true, requirements for Lrm contents will be enforced when a package reference is added 
        /// to PackageStore. In this case, any significant violation of Lrm format requirements will cause the import 
        /// process to fail. If false, an 
        /// attempt will be made to fix the package so that it can be imported. In this case, problems 
        /// may occur when the package is executed. Only Lrm values that are required for import are validated in this process.</param>
        /// <remarks>These settings do not cause changes to the initial package being added a reference into a PackageStore. Instead, 
        /// the imported package reference information is modified to allow import, without changing the original package.</remarks>
        public PackageEnforcement(bool enforceScormRequirements, bool enforceMlcRequirements, bool enforceLrmRequirements)
        {
            m_enforceMlcRequirements = enforceMlcRequirements;
            m_enforceScormRequirements = enforceScormRequirements;
            m_enforceLrmRequirements = enforceLrmRequirements;
        }

        /// <summary>
        /// Gets a value indicating whether the package should be modified if it violates Scorm requirements, 
        /// in order to allow it to be imported. If true, the 
        /// package should not be modified in order to allow the import. If false, the package import process will attempt to fix 
        /// issues in the package relating to Scorm requirements in order to facilitate importing the package. In some cases, 
        /// this may cause problems when the package is executed.
        /// </summary>
        public bool EnforceScormRequirements { get { return m_enforceScormRequirements; }  }

        /// <summary>
        /// Gets a value indicating whether the package should be modified if it violates Microsoft Learning Components requirements, 
        /// in order to allow it to be imported. If true, the 
        /// package should not be modified in order to allow the import. If false, the package import process will attempt to fix 
        /// issues in the package relating to Microsoft Learning Components requirements in order to facilitate importing 
        /// the package. In some cases, 
        /// this may cause problems when the package is executed.
        /// </summary>
        public bool EnforceMlcRequirements { get { return m_enforceMlcRequirements; } }
        
        /// <summary>
        /// Gets a value indicating whether the package should be modified if it violates LRM format requirements, 
        /// in order to allow it to be imported. If true, the 
        /// package should not be modified in order to allow the import. If false, the package import process will attempt to fix 
        /// issues in the package relating to LRM format requirements in order to facilitate importing 
        /// the package. In some cases, 
        /// this may cause problems when the package is executed.
        /// </summary>
        public bool EnforceLrmRequirements { get { return m_enforceLrmRequirements; } }
    }
    
    /// <summary>
    /// Represents an exception caused by adding an invalid package to a PackageStore. This 
    /// exception will only contain messages appropriate for untrusted callers indicating a 
    /// problem with the package being imported.
    /// </summary>
    [Serializable]  
    public class PackageImportException : Exception
    {
        ValidationResults m_log = new ValidationResults();

        /// <summary>
        /// Constructor. Represents a problem occurred when importing a package.
        /// </summary>
        public PackageImportException()
        {
        }

        /// <summary>
        /// Constructor, providing a custom message.
        /// </summary>
        /// <param name="message">Message to describe the cause of the problem.</param>
        public PackageImportException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor, provideing a custom message and a log of warnings and errors the occurred 
        /// before the exception.
        /// </summary>
        /// <param name="message">Message to describe the cause of the problem.</param>
        /// <param name="log">The log containing errors and warnings that occurred prior to the 
        /// exception.</param>
        public PackageImportException(string message, ValidationResults log) 
            : base(message)
        {
            m_log = log;
        }

        /// <summary>
        /// Constructor, providing a message and the exception that triggered this exception.
        /// </summary>
        /// <param name="message">Message to describe the cause of the problem.</param>
        /// <param name="innerException"></param>
        public PackageImportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor, providing a message and the exception that triggered this exception as 
        /// well a a log of warnings and errors the occurred 
        /// before the exception.
        /// </summary>
        /// <param name="message">Message to describe the cause of the problem.</param>
        /// <param name="innerException"></param>
        /// <param name="log">A log of warnings and errors the occurred 
        /// before the exception.</param>
        public PackageImportException(string message, Exception innerException, ValidationResults log)
            : base(message, innerException)
        {
            m_log = log;
        }

        /// <summary>
        /// Initializes a new instance of the PackageImportException class with serialized data. 
        /// </summary>
        /// <param name="serializationInfo">The <Typ>/System.Runtime.Serialization.SerializationInfo</Typ> that holds 
        /// the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <Typ>/System.Runtime.Serialization.StreamingContext</Typ> that contains contextual 
        /// information about the source or destination</param>
        protected PackageImportException(SerializationInfo serializationInfo, StreamingContext context)
            :base(serializationInfo, context)
        {
            Utilities.ValidateParameterNonNull("serializationInfo", serializationInfo);
            Utilities.ValidateParameterNonNull("context", context);

            m_log = new ValidationResults();
            int numResults = serializationInfo.GetInt32("numLogEntries");
            for (int i = 0; i<numResults; i++)
            {
                ValidationResult result = (ValidationResult)serializationInfo.GetValue(ResultName(i), typeof(ValidationResult));
                m_log.AddResult(result);
            }
        }

        /// <summary>
        /// Gets and sets the log of errors and warnings that occurred before the package import process was terminated. 
        /// The log cannot be set to null.
        /// </summary>
        public ValidationResults Log
        { 
            get { return m_log; }
            set 
            {
                Utilities.ValidateParameterNonNull("value", value);
                m_log = value; 
            }
        }

        /// <summary>
        /// Retrieve the object data for serialization.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)] // Required by fxcop
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Need to add log entries to serialized info
            int count = 0;
            info.AddValue("numLogEntries", Log.Results.Count);
            foreach (ValidationResult result in Log.Results)
            {
                // The names of the entries in the info collection have to be unique, so add a count to each name.
                info.AddValue(ResultName(count), result);
                count++;
            }
            
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Helper method to return string for the name of a result to serialize.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string ResultName(int count)
        {
            return String.Format(CultureInfo.CurrentCulture, "result{0}", Convert.ToString(count, NumberFormatInfo.InvariantInfo));
        }
    }

    /// <summary>
    /// The results of registering a package in the package store.
    /// </summary>
    public class RegisterPackageResult
    {
        private ValidationResults m_log;
        private PackageItemIdentifier m_packageId;

        /// <summary>
        /// Create an AddPackageResult from the results of adding a reference to a package in PackageStore.
        /// </summary>
        /// <param name="addReferenceResult">The results of adding a package reference to PackageStore.</param>
        internal RegisterPackageResult(AddPackageReferenceResult addReferenceResult)
        {
            m_log = addReferenceResult.Log;
            m_packageId = addReferenceResult.PackageId;
        }

        /// <summary>
        /// Create an AddPackageResult object to encapsulate the results of adding a package.
        /// </summary>
        /// <param name="packageId">The unique identifier of the package that was added.</param>
        /// <param name="log">The log containing any warnings or errors that occurred during the process of adding the package.</param>
        public RegisterPackageResult(PackageItemIdentifier packageId, ValidationResults log)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);

            m_packageId = packageId;
            m_log = log;
        }

        /// <summary>
        /// The reference that was added to PackageStore.
        /// </summary>
        public PackageItemIdentifier PackageId { get { return m_packageId; } }

        /// <summary>
        /// The log of errors and warnings that occurred in the course of adding the package to PackageStore.
        /// </summary>
        public ValidationResults Log { get { return m_log; } }
    }
}

