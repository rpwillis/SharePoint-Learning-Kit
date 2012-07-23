/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using System.Xml.XPath;
using System.Security.Principal;
using Microsoft.SharePoint;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security;
using System.Security.Policy;
using System.Web;
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// Represents a reader that can access packages stored in SharePoint. 
    /// </summary>
    public class SharePointPackageReader : PackageReader
    {
        private FileSystemPackageReader m_fsPackageReader;
        private CachedPackage m_cachedPackage;

        // Settings regarding the cache
        private SharePointCacheSettings m_settings;

        // Information about package to read
        private SharePointFileLocation m_pkgLocation;

        // Temporary instance variables used to pass data to delegates
        private Stream m_stream;

        // Delegate to access SharePoint files with elevated privileges.
        private RunWithElevatedPrivileges m_useRequestedPrivileges;

        private bool m_disposed; // indicates this object has been disposed

#region constructors
        /// <summary>
        /// Creates a package reader to read the specified package from SharePoint. The package 
        /// must be valid e-learning content.The package 
        /// is read using the current user's credentials.
        /// </summary>
        /// <param name="cacheSettings">The settings to use for the caching of this package. 
        /// A subdirectory will be created in the cacheSettings.CachePath location with a cached version of this package.</param>
        /// <param name="packageLocation">The location of the package to be read. Any changes to this SharePointFileLocation
        /// object after the SharePointPackageReader is created are not reflected in the behavior of this object.</param>
        /// <remarks>
        /// <para>
        /// In addition to the exceptions listed below, this method may throw exceptions caused by the 
        /// identity not having access to the <paramref name="cacheSettings"/> CachePath location.
        /// </para>
        /// <para>
        /// The contents of the package are not read in the constructor.  The contents of the package are read
        /// only once when they are first needed.  If the referenced SharePoint file does not contain a 
        /// valid e-learning package, accessing other methods and properties on this object will result 
        /// in an <c>InvalidPackageException</c>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the CachePath property of <paramref name="cacheSettings"/>
        /// does not exist prior to calling this constructor.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the requested file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the identity doesn't have access to the CachePath provided in the 
        /// cache settings.</exception>
        public SharePointPackageReader(SharePointCacheSettings cacheSettings, SharePointFileLocation packageLocation) : this(cacheSettings, packageLocation, false)
        {
        }

        /// <summary>
        /// Creates a package reader to read the specified package from SharePoint. This constructor
        /// optionally causes the file to be read from SharePoint using elevated permissions.
        /// </summary>
        /// <param name="cacheSettings">The settings to use for the caching of this package. 
        /// A subdirectory will be created in the cacheSettings.CachePath location with a cached version of this package.</param>
        /// <param name="packageLocation">The location of the package to be read. Any changes to this SharePointFileLocation
        /// object after the SharePointPackageReader is created are not reflected in the behavior of this object.</param>
        /// <param name="runWithElevatedPrivileges">If true, files in SharePoint are accessed using elevated privileges.
        /// If false, the current user credentials are used to access SharePoint files.</param>
        /// <remarks>
        /// <para>
        /// In addition to the exceptions listed below, this method may throw exceptions caused by the 
        /// identity not having access to the <paramref name="cacheSettings"/> CachePath location.
        /// </para>
        /// <para>
        /// The contents of the package are not read in the constructor. The contents of the package are read
        /// only once when they are first needed.  If the referenced SharePoint file does not contain a 
        /// valid e-learning package, accessing other methods and properties on this object will result 
        /// in an <c>InvalidPackageException</c>.
        /// </para>
        /// <para>
        /// If the  <paramref name="cacheSettings"/> CacheInvalidPackageAsFile value is true,
        /// e-learning packages that do not contain basic package information are saved as 
        /// files in the cache. In particular, this may increase performance in processing of zip files that do not contain 
        /// e-learning content. If false, SharePointPackageReader will not cache zip files that are not e-learning 
        /// content. In that case, an application that wants to cache this file would need to cache it as a 
        /// CachedSharePointFile. Regardless of the value of this parameter, the SharePointPackageReader will not allow 
        /// accessing files from within a package that is not e-learning content.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the CachePath property of <paramref name="cacheSettings"/>
        /// does not exist prior to calling this constructor.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the requested file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the identity doesn't have access to the CachePath provided in the 
        /// cache settings.</exception>
        public SharePointPackageReader(SharePointCacheSettings cacheSettings, SharePointFileLocation packageLocation, bool runWithElevatedPrivileges)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);
            Utilities.ValidateParameterNonNull("packageLocation", packageLocation);

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

            // Run with elevated permissions to access SharePoint
            UseRequestedPrivileges(delegate
            {
                // These methods will throw FileNotFoundException if the package does not exist.

                // If the site does not exist, this throws FileNotFound
                using (SPSite siteElevatedPermissions = new SPSite(packageLocation.SiteId))
                {
                    // If the web does not exist, this throws FileNotFound
                    using (SPWeb webElevatedPermissions = siteElevatedPermissions.OpenWeb(packageLocation.WebId))
                    {
                        SPFile fileElevatedPermissions = webElevatedPermissions.GetFile(packageLocation.FileId);
                        if (!fileElevatedPermissions.Exists)
                        {
                            throw new FileNotFoundException(Resources.SPFileNotFoundNoName);
                        }
                        
                        string filename = fileElevatedPermissions.Name;
                        
                        // Now check if the version of the file exists.
                        if (!FileExistsInSharePoint(fileElevatedPermissions, packageLocation.VersionId))
                        {
                            throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, filename));
                        }
                    }
                }
               
            });
           
            // Store variables.
            m_pkgLocation = new SharePointFileLocation(packageLocation);
            m_settings = new SharePointCacheSettings(cacheSettings);
        }
#endregion constructors

#region properties
        /// <summary>The location of the package.</summary>
        public SharePointFileLocation Location
        {
            get { return m_pkgLocation ;}
        }
#endregion properties

        /// <summary>
        /// Define the delegate that mimics the SPSecurity delegate that allows accessing SharePoint 
        /// files with elevated privileges.
        /// </summary>
        private delegate void RunWithElevatedPrivileges(SPSecurity.CodeToRunElevated codeToRun);

        /// <summary>
        /// The FileSystemPackageReader used by this reader
        /// </summary>
        private FileSystemPackageReader FileSystemPackageReader
        {
            get
            {
                if(m_fsPackageReader == null)
                {
                    UseRequestedPrivileges(delegate
                    {
                        m_cachedPackage = new CachedPackage(m_settings, m_pkgLocation, true);
                    });
                    m_fsPackageReader = new FileSystemPackageReader(m_cachedPackage.CacheDir, m_settings.ImpersonationBehavior);
                }
                return m_fsPackageReader;
            }
        }
        
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
        /// Releases all resources used by this object
        /// </summary>
        /// <param name="disposing">True if this method was called from
        ///    <Typ>/System.IDisposable.Dispose</Typ></param>
        protected override void Dispose(bool disposing)
        {
            m_disposed = true;
            try
            {
                if (m_cachedPackage != null)
                {
                    m_cachedPackage.Dispose();
                    m_cachedPackage = null;
                }
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
        /// Returns true if the version of the file exists in SharePoint. Must be called in the context 
        /// of appropriate permissions, as this method does not impersonate.
        /// </summary>
        private static bool FileExistsInSharePoint(SPFile spFile, int versionId)
        {
            if (spFile.Exists == false)
            {
                    return false;
            }

            // If there are no versions, or if the current version is the requested version
            if ((spFile.Versions.Count == 0) || spFile.UIVersion == versionId)
            {
                return (spFile.UIVersion == versionId) && spFile.Exists;
            }
            else
            {
                // The specified version isn't the current one
                SPFileVersion spFileVersion = spFile.Versions.GetVersionFromID(versionId);

                return (spFileVersion != null) && spFileVersion.File.Exists;
            }
        }

        /// <summary>
        /// Throws <Typ>ObjectDisposedException</Typ> if this object has been disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException("SharePointPackageReader");
        }
        
        #region Required Overrides
        public override Stream GetFileStream(string filePath)
        {
            CheckDisposed();
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
                        
            m_stream = FileSystemPackageReader.GetFileStream(filePath);

            return m_stream;
        }

        /// <summary>
        /// Returns true if a file exists at the specified path and the caller has the 
        /// required permissions. 
        /// 
        /// </summary>
        /// <param name="filePath">The package-relative path to a file in the package.</param>
        /// <returns>Returns true if a file exists. This method also returns false if path is a null 
        /// reference, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to 
        /// read the specified file, no exception is thrown and the method returns false regardless of the 
        /// existence of path. </returns>
        public override bool FileExists(string filePath)
        {
            CheckDisposed();
            try
            {
                Utilities.ValidateParameterNonNull("filePath", filePath);
                Utilities.ValidateParameterNotEmpty("filePath", filePath);

                return FileSystemPackageReader.FileExists(filePath);
            }
            // Catch exceptions that should be converted into a "false" file exists. (Same list as File.Exists())
            catch (DirectoryNotFoundException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return false;
        }

        /// <summary>
        /// Gets the list of package relative file paths of all files in the package.
        /// </summary>
        /// <returns></returns>
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            CheckDisposed();
            return FileSystemPackageReader.GetFilePaths();

        }

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response">The response to write to.</param>
        public override void TransmitFile(string filePath, HttpResponse response)
        {
            CheckDisposed();
            FileSystemPackageReader.TransmitFile(filePath, response);
        }

        #endregion

        /// <summary>
        /// Gets the directory that the package (specified by site, web, file, etc)
        /// would be cached into. 
        /// </summary>
        /// <param name="cachePath">The folder to use for a cache of all SharePoint files.</param>
        /// <param name="identity">The user identity that has read/write permissions to the
        /// <paramref name="cachePath"/> location.</param>
        /// <param name="packageLocation">The location of the package in SharePoint.</param>
        /// <returns>The directory path to the cached package.</returns>
        internal static string GetCacheDirectory(string cachePath, ImpersonationBehavior impersonationBehavior, SharePointFileLocation packageLocation)
        {
            return CachedPackage.GetCacheDirectory(cachePath, packageLocation, impersonationBehavior);
        }
    }
}
