/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.IO;
using Microsoft.LearningComponents;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.LearningComponents.Storage
{
    /// <summary>
    /// Represents a reader of a SCORM package that has been stored in the file system after being added into 
    /// LearningStore. The reader accesses files for one package. The related PackageStore manages all the packages
    /// in a LearningStore.
    /// </summary>
    /// <remarks>This class reads manifest information from LearningStore and package files from 
    /// the file system in order to read any file represented in the package.
    /// </remarks>
    internal class FileSystemPackageStoreReader : PackageReader
    {
        FileSystemPackageStore m_store;
        
        // The package this reader is going to read.
        PackageItemIdentifier m_packageId;

        // The absolute path to the location of the package files. This directory has imsmanifest.xml.
        string m_packageBasePath;

        // The user who has access to the filesystem for this store.
        ImpersonationBehavior m_impersonationBehavior = ImpersonationBehavior.UseImpersonatedIdentity;

        FileSystemPackageReader m_fsPackageReader;  // the PackageReader to read resource files

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="packageBasePath">The absolute path to the location where the packages 
        /// are located in the filesystem.</param>
        /// <param name="packageStore">The PackageStore that contains information about the package.</param>
        /// <param name="packageId">The package id of the package to load.</param>
        /// <param name="packageLocation">The location of the package, as defined in 
        /// LearningStore PackageItem.Location column. This cannot be null.</param>
        /// <param name="impersonationBehavior">The user who has access to the file system related to the store.</param>
        /// <remarks></remarks>
        internal FileSystemPackageStoreReader(string packageBasePath, FileSystemPackageStore packageStore,
            PackageItemIdentifier packageId, string packageLocation, ImpersonationBehavior impersonationBehavior )
        {
            m_store = packageStore;
            m_packageId = packageId;
            m_impersonationBehavior = impersonationBehavior;

            m_packageBasePath = PackageReader.SafePathCombine(packageBasePath, packageLocation);

            // This does not verify that m_packageBasePath actually exists
            m_fsPackageReader = new FileSystemPackageReader(m_packageBasePath, m_impersonationBehavior);
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
                if (m_fsPackageReader != null)
                {
                    m_fsPackageReader.Dispose();
                    m_fsPackageReader = null;
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
        /// information with the basePath value to determine the file location.
        /// </para>
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="relativePath"/> 
        /// contains a reference to a directory that does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="relativePath"/> 
        /// contains a reference to a file that does not exist.</exception>
        public override Stream GetFileStream(string relativePath)
        {
            // If it's the manifest, ask to get it from the store. Otherwise have the FileSystemPackageReader
            // get it.
            if (IsManifest(relativePath))
                return m_store.GetManifestFile(m_packageId);

            return m_fsPackageReader.GetFileStream(relativePath, true);
        }

        // Return true if relativePath represents the manifest file
        private static bool IsManifest(string relativePath)
        {
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
            // If the package exists, the manifest exists.
            if (IsManifest(relativePath))
                return true;

            return m_fsPackageReader.FileExists(relativePath, true);
        }

        /// <summary>
        /// Gets the collection of file paths in the package.
        /// </summary>
        /// <returns>Collection of file paths in the package.</returns>
        public override ReadOnlyCollection<string> GetFilePaths()
        {
    
            ReadOnlyCollection<string> resourceFiles = m_fsPackageReader.GetFilePaths(true);
            string[] allFiles = new string[resourceFiles.Count + 1];
            resourceFiles.CopyTo(allFiles, 0);
            allFiles[allFiles.Length - 1] = @"imsmanifest.xml";
            ReadOnlyCollection<string> allFilesRO = new ReadOnlyCollection<string>(allFiles);
            return allFilesRO;
        }

        /// <summary>
        /// When possible, writes the file directly to the response.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="response"></param>
        public override void TransmitFile(string filePath, System.Web.HttpResponse response)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            Utilities.ValidateParameterNonNull("response", response);

            // If the file is the manifest, then copy the streams (ie, do it slowly).
            if (IsManifest(filePath))
            {
                using (Stream manifestStream = GetFileStream(filePath))
                {
                    Utilities.CopyStream(manifestStream, m_impersonationBehavior, response.OutputStream, m_impersonationBehavior);
                }
                return;
            }

            // The file was not the manifest, so ask the package reader to send it
            m_fsPackageReader.TransmitFile(filePath, response, true);
        }
    }
}