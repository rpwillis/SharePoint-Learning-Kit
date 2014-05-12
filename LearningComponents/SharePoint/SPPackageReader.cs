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
    class SharePointPackageReader : FileSystemBasedSharePointPackageReader
    {
        // Settings regarding the cache
        private SharePointCacheSettings m_settings;

#region constructors

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
        /// <param name="file">The SPFile to read.</param>
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
        public SharePointPackageReader(SharePointCacheSettings cacheSettings, SharePointFileLocation packageLocation, SPFile file, bool runWithElevatedPrivileges) : base(packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);

            RunWithElevatedPrivileges useRequestedPrivileges;
            if (runWithElevatedPrivileges)
            {
                useRequestedPrivileges = SPSecurity.RunWithElevatedPrivileges;
            }
            else
            {
                useRequestedPrivileges = RunWithCurrentUserPrivileges;
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

            CheckFileExists(file, packageLocation);
           
            // Store variables.
            m_settings = cacheSettings.Clone();

            CachedPackage cachedPackage = null;
            useRequestedPrivileges(delegate
            {
                cachedPackage = new CachedPackage(m_settings, Location, true);
            });

            using (cachedPackage)
            {
                Initialize(new DirectoryInfo(cachedPackage.CacheDir), m_settings.ImpersonationBehavior);
            }
        }
#endregion constructors

#region properties
#endregion properties

        void CheckFileExists(SPFile file, SharePointFileLocation packageLocation)
        {
            if (file.Exists == false)
            {
                throw new FileNotFoundException(Resources.SPFileNotFoundNoName);
            }
            
            string filename = file.Name;
            
            // Now check if the version of the file exists.
            if (FileExistsInSharePoint(file, packageLocation.VersionId) == false)
            {
                throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileNotFound, filename));
            }
        }

        /// <summary>
        /// Define the delegate that mimics the SPSecurity delegate that allows accessing SharePoint 
        /// files with elevated privileges.
        /// </summary>
        private delegate void RunWithElevatedPrivileges(SPSecurity.CodeToRunElevated codeToRun);

        /// <summary>
        /// Delegate to run without elevated privileges. Just run as the current user.
        /// </summary>
        private void RunWithCurrentUserPrivileges(SPSecurity.CodeToRunElevated codeToRun)
        {
            // Don't impersonate, just run the code.
            codeToRun();
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
        /// Gets the directory that the package (specified by site, web, file, etc)
        /// would be cached into. 
        /// </summary>
        /// <param name="cachePath">The folder to use for a cache of all SharePoint files.</param>
        /// <param name="impersonationBehavior">The impersonation behaviour to use.</param>
        /// <param name="packageLocation">The location of the package in SharePoint.</param>
        /// <returns>The directory path to the cached package.</returns>
        internal static string GetCacheDirectory(string cachePath, ImpersonationBehavior impersonationBehavior, SharePointFileLocation packageLocation)
        {
            return CachedPackage.GetCacheDirectory(cachePath, packageLocation, impersonationBehavior);
        }
    }
}
