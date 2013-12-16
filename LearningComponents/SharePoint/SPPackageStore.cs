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

        /// <summary>Gets a <see cref="PackageReader"/> for an item.</summary>
        /// <param name="packageId">the id of the package.</param>
        /// <param name="packageLocation">The locatoin of the package.</param>
        /// <returns>A <see cref="PackageReader"/>.</returns>
        protected internal override PackageReader GetPackageReader(PackageItemIdentifier packageId, string packageLocation)
        {
            return new SharePointPackageStoreReader(this, packageId, packageLocation);
        }
        #endregion

        /// <summary>Creates a package reader for a package without accessing the store.</summary>
        /// <param name="file">The package.</param>
        /// <param name="location">The package location.</param>
        /// <param name="runWithElevatedPrivileges">Whether to run with elevated privileges or not.</param>
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
}
