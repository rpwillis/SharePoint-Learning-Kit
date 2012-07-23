/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.IO;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.LearningComponents.Storage;
using Microsoft.SharePoint;
using System.Globalization;
using System.Threading;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// Represents a reader of a package that has been registered into a SharePointPackageStore.
    /// The reader accesses files for one package. The related PackageStore manages all the package references
    /// in a LearningStore.
    /// </summary>
    /// <remarks>This class reads manifest information from LearningStore and package files from 
    /// SharePointPackageReader (which reads files in the cache or SharePoint) 
    /// in order to read any file represented in the package.
    /// </remarks>
    internal class SharePointPackageStoreReader : PackageReader
    {
        SharePointPackageStore m_store;

        // The package this reader is going to read.
        PackageItemIdentifier m_packageId;

        SharePointPackageReader m_spPackageReader;  // the PackageReader to read resource files

        private bool m_isDisposed;

        /// <summary>
        /// Constructor. Accesses all SharePoint files with elevated privilege.
        /// </summary>
        /// <param name="packageStore">The PackageStore that contains information about the package.</param>
        /// <param name="cachePath">The absolute path to the location of the package files.</param>
        /// <param name="packageId">The package id of the package to load.</param>
        /// <param name="packageLocation">The location of the package, as defined in 
        /// LearningStore PackageItem.Location column. This cannot be null.</param>
        /// <param name="spIdentity">The user who has access to the package related to the store.</param>
        /// <remarks></remarks>
        internal SharePointPackageStoreReader(SharePointPackageStore packageStore, PackageItemIdentifier packageId, string packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("packageLocation", packageLocation);
            Utilities.ValidateParameterNotEmpty("packageLocation", packageLocation);

            m_store = packageStore;
            m_packageId = packageId;

            SharePointFileLocation location;

            if (!SharePointFileLocation.TryParse(packageLocation, out location))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.SPFormatInvalid, packageLocation));
            }

            m_spPackageReader = new SharePointPackageReader(m_store.CacheSettings, location, true);
        }

        /// <summary>
        /// Releases all resources used by this object
        /// </summary>
        /// <param name="disposing">True if this method was called from
        ///    <Typ>/System.IDisposable.Dispose</Typ></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (m_isDisposed)
                    return;

                if (m_spPackageReader != null)
                {
                    m_spPackageReader.Dispose();
                    m_spPackageReader = null;
                    m_isDisposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Return a file located at the specified package-relative path.
        /// </summary>
        /// <param name="relativePath">The package-relative path to the file in the package.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>This method reads files from the file system. It combines the path
        /// information with the cachePath value to determine the file location.
        /// </para>
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="relativePath"/> 
        /// contains a reference to a directory that does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="relativePath"/> 
        /// contains a reference to a file that does not exist.</exception>
        public override Stream GetFileStream(string relativePath)
        {
            if (m_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            // If it's the manifest, ask to get it from the store. Otherwise have the SharePointPackageReader
            // get it.
            if (IsManifest(relativePath))
                return m_store.GetManifestFile(m_packageId);

            return m_spPackageReader.GetFileStream(relativePath);
        }

        // Return true if relativePath represents the manifest file
        private static bool IsManifest(string relativePath)
        {
            Utilities.ValidateParameterNonNull("relativePath", relativePath);
            relativePath = relativePath.Trim();
            return relativePath.Equals(@"imsmanifest.xml", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Return true if a file exists at a specific location within the package.
        /// </summary>
        /// <param name="relativePath">The package-relative path to the requested file.</param>
        /// <returns>True if the package exists.</returns>
        /// <exception cref="ArgumentException">Returned if the relativePath is null or empty or
        /// is not a relative path.</exception>
        public override bool FileExists(string relativePath)
        {
            if (m_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            // If the package exists, the manifest exists.
            if (IsManifest(relativePath))
                return true;

            return m_spPackageReader.FileExists(relativePath);
        }

        /// <summary>
        /// Gets the collection of file paths in the package.
        /// </summary>
        /// <returns>Collection of file paths in the package.</returns>
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            return m_spPackageReader.GetFilePaths();
        }

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response">The response to write to.</param>
        public override void TransmitFile(string filePath, System.Web.HttpResponse response)
        {
            if (m_isDisposed)
                throw new ObjectDisposedException(GetType().Name);

            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            Utilities.ValidateParameterNonNull("response", response);

            // If the file is the manifest, then copy the streams (ie, do it slowly).
            if (IsManifest(filePath))
            {
                using (Stream manifestStream = GetFileStream(filePath))
                {
                    Utilities.CopyStream(manifestStream, ImpersonationBehavior.UseImpersonatedIdentity, response.OutputStream, ImpersonationBehavior.UseImpersonatedIdentity);
                }
                return;
            }

            m_spPackageReader.TransmitFile(filePath, response);
        }

        /// <summary>
        /// Gets the directory that the package (specified by packageLocation)
        /// would be cached into. 
        /// </summary>
        /// <param name="cachePath">The folder to use for a cache of all SharePoint files.</param>
        /// <param name="identity">The user identity that has read/write permissions to the
        /// <paramref name="cachePath"/> location.</param>
        /// <param name="packageLocation">The package loacation in LearningStore format.</param>
        /// <returns>The directory path to the cached package.</returns>
        internal static string GetCacheDirectory(string cachePath, ImpersonationBehavior impersonationBehavior, string packageLocation)
        {
            SharePointFileLocation location;

            if (!SharePointFileLocation.TryParse(packageLocation, out location))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.SPFormatInvalid, packageLocation));

            return SharePointPackageReader.GetCacheDirectory(cachePath, impersonationBehavior, location);
        }

    }
}
