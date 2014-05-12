/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using System.Web;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// A file which exists in a SharePoint document library, and is cached locally for faster access.
    /// The file may be read, but the file is not opened or its contents processed.
    /// </summary>
    /// <remarks>
/// <c>CachedSharePointFile</c> implements <Typ>IDisposable</Typ>.  Always call the <Mth>Dispose</Mth> method when finished
    /// with a <c>CachedSharePointFile</c> object, or use a <c>using</c> statement, to ensure that unmanaged resources are
    /// explicitly and properly released.
    /// </remarks>
    public class CachedSharePointFile : IDisposable
    {
        private SharePointCacheSettings m_settings;
        private CachedPackage m_cachedPackage;
        private SharePointFileLocation m_location;
        private string m_fileName;  // the name without path information, such as "myBook.doc"

        private bool m_disposed; // indicates this object has been disposed

        /// <summary>
        /// Create a cached copy of a file in SharePoint. 
        /// </summary>
        /// <param name="cacheSettings">The settings that determine how the file is cached.</param>
        /// <param name="location">The location of the file in SharePoint.</param>
        /// <remarks>
        /// This method does only verifies that the <c>settings.WindowsIdentity</c> has access to the 
        /// <c>settings.CachePath</c>. It does not verify that the file exists in SharePoint.
        /// The contents of the package are read only once when they are first needed.
        /// </remarks>
        public CachedSharePointFile(SharePointCacheSettings cacheSettings, SharePointFileLocation location)
            : this(cacheSettings, location, false)
        {
        }

        /// <summary>
        /// Create a representation of a file in SharePoint, running with elevated privileges when 
        /// accessing the SharePoint file.
        /// </summary>
        /// <param name="cacheSettings">The settings that determine how the file is cached.</param>
        /// <param name="location">The location of the file in SharePoint.</param>
        /// <param name="runWithElevatedPrivileges">If true, SharePoint file will be accessed using elevated privileges.</param>
        /// <remarks>
        /// This method does only verifies that the <c>settings.WindowsIdentity</c> has access to the 
        /// <c>settings.CachePath</c>. It does not verify that the file exists in SharePoint.
        /// The contents of the package are read only once when they are first needed.
        /// </remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if the <c>settings.WindowsIdentity</c>
        /// does not have appropriate permissions to the <c>settings.CachePath</c> folder.</exception>
        public CachedSharePointFile(SharePointCacheSettings cacheSettings, SharePointFileLocation location, bool runWithElevatedPrivileges)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);
            Utilities.ValidateParameterNonNull("location", location);
            
            m_settings = cacheSettings.Clone();
            m_location = location;

            if (runWithElevatedPrivileges)
            {
                m_useRequestedPrivileges = SPSecurity.RunWithElevatedPrivileges;
            }
            else
            {
                m_useRequestedPrivileges = RunWithCurrentUserPrivileges;
            }

            try
            {
                // Impersonate the identity to access the cache
                using (new ImpersonateIdentity(cacheSettings.ImpersonationBehavior))
                {
                    if (!Directory.Exists(cacheSettings.CachePath))
                    {
                        Directory.CreateDirectory(cacheSettings.CachePath);
                    }

                    // Test that the identity has read access to the directory. This will throw UnauthorizedAccessException if it 
                    // does not.
                    Directory.GetFiles(cacheSettings.CachePath);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases all resources used by this object
        /// </summary>
        /// <param name="disposing">True if this method was called from
        ///    <Typ>/System.IDisposable.Dispose</Typ></param>
        protected virtual void Dispose(bool disposing)
        {
            m_disposed = true;
            if (m_cachedPackage != null)
            {
                m_cachedPackage.Dispose();
                m_cachedPackage = null;
            }
        }

        /// <summary>
        /// This method supports the .NET Framework infrastructure and is not intended to be used
        /// directly from your code.
        /// <para>
        /// Releases all resources used by the <Typ>CachedSharePointFile</Typ>.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws <Typ>ObjectDisposedException</Typ> if this object has been disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException("CachedSharePointFile");
        }

        // Delegate to access SharePoint files with elevated privileges.
        private RunWithElevatedPrivileges m_useRequestedPrivileges;

        /// <summary>
        /// Define the delegate that mimics the SPSecurity delegate that allows accessing SharePoint 
        /// files with elevated privileges.
        /// </summary>
        private delegate void RunWithElevatedPrivileges(SPSecurity.CodeToRunElevated codeToRun);

        /// <summary>
        /// Gets the delegate to use when a request may require elevated privileges.
        /// </summary>
        private RunWithElevatedPrivileges UseRequestedPrivileges
        {
            get { return m_useRequestedPrivileges; }
        }

        /// <summary>
        /// Delegate to run without elevated privileges. Just run as the current user.
        /// </summary>
        private void RunWithCurrentUserPrivileges(SPSecurity.CodeToRunElevated codeToRun)
        {
            // Don't impersonate, just run the code.
            codeToRun();
        }

        /// <summary>
        /// The CachedPackage used by this object
        /// </summary>
        private CachedPackage CachedPackage
        {
            get
            {
                if (m_cachedPackage == null)
                {
                    UseRequestedPrivileges(delegate
                    {
                        m_cachedPackage = new CachedPackage(m_settings, m_location, false);
                    });
                }
                return m_cachedPackage;
            }
        }

        /// <summary>
        /// Gets the file as a stream.
        /// </summary>
        /// <returns>The cached file as a stream. The caller must close this.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // this method does significant work
        public Stream GetFileStream()
        {
            CheckDisposed();

            // Get the stream while running in elevated privileges mode.
            string cacheDir = CachedPackage.CacheDir;
            SetFileName(cacheDir);
            return FileSystemPackageReader.ReadFile(cacheDir, FileName, m_settings.ImpersonationBehavior);
        }

        /// <summary>
        /// Save the FileName value. This just saves some time if the file is cached before the filename is requested.
        /// </summary>
        /// <param name="filePath">The root folder of the cache directory where the file is saved.</param>
        private void SetFileName(string filePath)
        {
            // Only need to set this once. It cannot change.
            if (!String.IsNullOrEmpty(m_fileName))
                return;

            using (ImpersonateIdentity id = new ImpersonateIdentity(m_settings.ImpersonationBehavior))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(filePath);
                FileInfo[] fiList = dirInfo.GetFiles();
                m_fileName = fiList[0].Name;
            }
        }

        /// <summary>
        /// Gets the file name of the file being cached. This method requires
        /// </summary>
        public string FileName
        {
            get
            {
                CheckDisposed();

                if (string.IsNullOrEmpty(m_fileName))
                {
                    SetFileName(CachedPackage.CacheDir);
                }

                return m_fileName;
            }
        }

        /// <summary>
        /// Writes the file directly to the response. 
        /// </summary>
        /// <param name="response"></param>
        public void TransmitFile(HttpResponse response)
        {
            CheckDisposed();
            Utilities.ValidateParameterNonNull("response", response);

            // Run under elevated privileges in case we have to read from SP
            string absoluteFilePath = PackageReader.SafePathCombine(CachedPackage.CacheDir, FileName);
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            using (ImpersonateIdentity id = new ImpersonateIdentity(m_settings.ImpersonationBehavior))
            {
                response.TransmitFile(absoluteFilePath);
            }
        }
    }
}
