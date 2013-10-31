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
    /// SharePointLibraryPackageStore represents the packages stored in SharePoint that are available for 
    /// viewing by Microsoft Learning Components. This class does not add or modify files in SharePoint, 
    /// but rather updates LearningStore to contain references to learning packages already stored in SharePoint.
    /// </summary>
    public class SharePointLibraryPackageStore : PackageStore
    {
        static readonly SPContentTypeId permanentCacheContentType = new SPContentTypeId("0x010100CF3A5A3D3C324CF5876663C33D4EE8AA0101");
        
        /// <summary>
        /// Create the store.
        /// </summary>
        /// <param name="learningStore">The learning store to use.</param>
        /// <param name="cacheSettings">Cache settings, including the file system location where the packages in the store can be cached.
        /// </param>
        /// <remarks>
        /// </remarks>
        public SharePointLibraryPackageStore(LearningStore learningStore, SharePointCacheSettings cacheSettings) : base(learningStore)
        {
            Utilities.ValidateParameterNonNull("learningStore", learningStore);
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);

            CacheSettings = cacheSettings;
        }

        /// <summary>
        /// Gets the SharePointCacheSettings for the store.
        /// </summary>
        internal SharePointCacheSettings CacheSettings { get; private set; }

        /// <summary>
        /// Registers a package with the SharePointLibraryPackageStore. Any changes to the original file after the package is 
        /// registered are not reflected in the store.        
        /// </summary>
        /// <param name="packageReader">A reader positioned on the package.</param>
        /// <param name="packageEnforcement">The settings to determine whether the package should be modified to 
        /// allow it to be added to the store.</param>
        /// <returns>The results of adding the package, including a log of any warnings or errors that occurred in the process.
        /// </returns>
        /// <exception cref="PackageImportException">Thrown if the package could not be registered with the store. The exception may 
        /// include a log indicating a cause for the failure.</exception>
        /// <remarks>The package is read from SharePoint under elevated privileges. The current user does not need access to the 
        /// package in order to register it. However, the current user does need to have permission to add a package to 
        /// LearningStore in order to register it.</remarks>
        public RegisterPackageResult RegisterPackage(FileSystemBasedSharePointPackageReader packageReader, PackageEnforcement packageEnforcement)
        {
            Utilities.ValidateParameterNonNull("packageEnforcement", packageEnforcement);
            Utilities.ValidateParameterNonNull("packageReader", packageReader);

            // Add the reference to the package. The process of importing the package will cause the package reader to cache 
            // the package in the file system.
            AddPackageReferenceResult refResult = AddPackageReference(packageReader, packageReader.Location.ToString(),  packageEnforcement);  
            return new RegisterPackageResult(refResult);
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
            return new SharePointLibraryPackageReader(CacheSettings, new SharePointFileLocation(packageLocation), null);
        }
        #endregion

        /// <summary>Creates a package reader for a package without accessing the store.</summary>
        /// <param name="file">The package.</param>
        /// <param name="location">The package location.</param>
        /// <param name="runWithElevatedPrivileges">Whether to run with elevated privileges or not.</param>
        /// <returns></returns>
        public PackageReader CreatePackageReader(SPFile file, SharePointFileLocation location, bool runWithElevatedPrivileges)
        {
            return new SharePointLibraryPackageReader(CacheSettings, location, file);
        }

    }
}
