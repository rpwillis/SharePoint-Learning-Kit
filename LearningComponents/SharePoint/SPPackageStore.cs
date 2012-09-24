/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents.Storage;
using System.Security.Principal;
using Microsoft.LearningComponents;
using Microsoft.SharePoint;
using System.IO;
using System.Globalization;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// SharePointPackageStore represents the packages stored in SharePoint that are available for 
    /// viewing by Microsoft Learning Components. This class does not add or modify files in SharePoint, 
    /// but rather updates LearningStore to contain references to learning packages already stored in SharePoint.
    /// </summary>
    public class SharePointPackageStore : PackageStore
    {
        static readonly SPContentTypeId permanentCacheContentType = new SPContentTypeId("0x010100CF3A5A3D3C324CF5876663C33D4EE8AA0101");
        private SharePointCacheSettings m_cacheSettings;
        
        /// <summary>
        /// Create the store.
        /// </summary>
        /// <param name="learningStore">The learning store to use.</param>
        /// <param name="cacheSettings">Cache settings, including the file system location where the packages in the store can be cached.
        /// </param>
        /// <remarks>
        /// </remarks>
        public SharePointPackageStore(LearningStore learningStore, SharePointCacheSettings cacheSettings)
            : base(learningStore)
        {
            Utilities.ValidateParameterNonNull("learningStore", learningStore);
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);

            m_cacheSettings = cacheSettings;
        }

        /// <summary>
        /// Gets the SharePointCacheSettings for the store.
        /// </summary>
        internal SharePointCacheSettings CacheSettings
        {
            get { return m_cacheSettings; }
        }

        /// <summary>
        /// Registers a package with the SharePointPackageStore. Any changes to the original file after the package is 
        /// registered are not reflected in the store.        
        /// </summary>
        /// <param name="packageLocation">The location of the package to be registered.</param>
        /// <param name="packageEnforcement">The settings to determine whether the package should be modified to 
        /// allow it to be added to the store.</param>
        /// <returns>The results of adding the package, including a log of any warnings or errors that occurred in the process.
        /// </returns>
        /// <exception cref="PackageImportException">Thrown if the package could not be registered with the store. The exception may 
        /// include a log indicating a cause for the failure.</exception>
        /// <remarks>The package is read from SharePoint under elevated privileges. The current user does not need access to the 
        /// package in order to register it. However, the current user does need to have permission to add a package to 
        /// LearningStore in order to register it.</remarks>
        public RegisterPackageResult RegisterPackage(SharePointLocationPackageReader packageReader, PackageEnforcement packageEnforcement)
        {
            Utilities.ValidateParameterNonNull("packageEnforcement", packageEnforcement);
            Utilities.ValidateParameterNonNull("packageReader", packageReader);

            // Add the reference to the package. The process of importing the package will cause the package reader to cache 
            // the package in the file system.
            AddPackageReferenceResult refResult = AddPackageReference(packageReader, packageReader.Location.ToString(),  packageEnforcement);  
            return new RegisterPackageResult(refResult);
        }

        /// <summary>
        /// Remove the cache of a package. 
        /// </summary>
        /// <param name="packageLocation">The Location string from PackageItem table.</param>
        private void RemoveCache(string packageLocation)
        {
            // Verify parameters
            Utilities.ValidateParameterNotEmpty("packageLocation", packageLocation);

            // Get the cache directory, outside of the impersonation block.
            string packageDirectory = SharePointPackageStoreReader.GetCacheDirectory(m_cacheSettings.CachePath, m_cacheSettings.ImpersonationBehavior, packageLocation);
                    
            using (ImpersonateIdentity id = new ImpersonateIdentity(m_cacheSettings.ImpersonationBehavior))
            {
                // Delete the directory and all subdirectories.
                Directory.Delete(packageDirectory, true);
            }
        }

        /// <summary>
        /// Unregister the package from the store. Once unregistered, the package cannot be viewed. 
        /// </summary>
        /// <param name="packageId">The <Typ>PackageItemIdentifier</Typ> of the package to unregister.</param>
        /// <remarks></remarks>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the <P>packageId</P> does not 
        /// represent a package in the store.</exception>
        /// <exception cref="LearningStoreConstraintViolationException">Thrown if the package cannot be deleted 
        /// from LearningStore because there are other items in the store that depend upon it. For instance,
        /// a package for which there are attempts in LearningStore, cannot be unregistered.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // see comment in catch block
        public void UnregisterPackage(PackageItemIdentifier packageId)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);

            string packageLocation = RemovePackageReference(packageId);

            try
            {
                RemoveCache(packageLocation);
            }
            catch
            {
                // It does not matter why this fails, ignore it. At this point, the package reference is gone from
                // LearningStore. So, even if we can't remove the files now they will eventually time out and be 
                // deleted from the cache because they have not been used.
            }
        }

        #region Required Overrides

        /// <summary>
        /// Gets a PackageReader that can read files from the specified package.
        /// </summary>
        /// <param name="packageId">The identifier of a package that exists in the store.</param>
        /// <returns>A PackageReader for the requested <paramref name="packageId"/>.</returns>
        public override PackageReader GetPackageReader(PackageItemIdentifier packageId)
        {
            string packageLocation = GetPackageLocationFromLS(packageId);

            return GetPackageReader(packageId, packageLocation);
        }

        protected internal override PackageReader GetPackageReader(PackageItemIdentifier packageId, string packageLocation)
        {
            return new SharePointPackageStoreReader(this, packageId, packageLocation);
        }
        #endregion

        /// <summary>Creates a package reader for a package without accessing the store.</summary>
        /// <param name="file">The package.</param>
        /// <param name="location">The package location.</param>
        /// <returns></returns>
        public PackageReader CreatePackageReader(SPFile file, SharePointFileLocation location, bool runWithElevatedPrivileges)
        {
            if (SPContentTypeId.FindCommonParent(file.Item.ContentType.Id, permanentCacheContentType) == permanentCacheContentType)
            {
                object directoryValue = file.Item[new Guid("a76de874-b256-4fd6-8933-813aa8587163")];
                if (directoryValue == null)
                {
                    throw new CacheException(Resources.PermanentCacheNoDirectory);
                }
                else
                {
                    DirectoryInfo directory;
                    try
                    {
                        directory = new DirectoryInfo(directoryValue.ToString());
                    }
                    catch (ArgumentException)
                    {
                        throw new CacheException(Resources.PermanentCacheInvalidDirectory);
                    }
                    catch (PathTooLongException)
                    {
                        throw new CacheException(Resources.PermanentCacheInvalidDirectory);
                    }

                    return new PermanentCacheSharePointPackageReader(directory, location);
                }
            }
            else
            {
                return new SharePointPackageReader(CacheSettings, location, file, runWithElevatedPrivileges);
            }
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
