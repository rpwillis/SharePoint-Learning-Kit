/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.SharePoint;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Security.Principal;        // for WindowsIndentity
using System.Runtime.InteropServices;    // for dllimport
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>
    /// Represents the settings to be specified for caching e-learning packages that are stored in 
    /// SharePoint.
    /// </summary>
    public class SharePointCacheSettings
    {
        private string m_cachePath;
        private TimeSpan? m_expirationTime;
        private ImpersonationBehavior m_impersonationBehavior = ImpersonationBehavior.UseImpersonatedIdentity;

        /// <summary>
        /// Create settings for a class that manages a cache of SharePoint files.
        /// </summary>
        /// <param name="cachePath">The path to the folder containing the cached files.</param>
        /// <param name="expirationTime">The maximum amount of time that packages are stored in the cache. After this time has passed, 
        /// the package will be removed from the cache if it has not been accessed. If not provided, files will never be removed from the 
        /// cache.</param>
        /// <param name="impersonationBehavior">The identity used to access the cache. </param>
        /// <param name="cacheInvalidPackageAsFile">If true, a package that is invalid will be cached as a file. While it 
        /// cannot be read as a package in this case, setting this to true may improve performance.</param>
        /// <remarks>
        /// This class does not verify that the <paramref name="cachePath"/>
        /// directory exists, however, when the settings are passed to SharePointPackageReader or SharePointPackageStore, those 
        /// classes will require the directory to exist.</remarks>
        public SharePointCacheSettings(string cachePath, TimeSpan? expirationTime, ImpersonationBehavior impersonationBehavior, bool cacheInvalidPackageAsFile)
        {
            Utilities.ValidateParameterNotEmpty("cachePath", cachePath);

            m_cachePath = cachePath;
            m_expirationTime = expirationTime;
            m_impersonationBehavior = impersonationBehavior;
            m_cacheInvalidPackageAsFile = cacheInvalidPackageAsFile;
        }

        /// <summary>
        /// Create settings for a class that manages a cache of SharePoint files. By default, the cache files will not be 
        /// cleaned up, and invalid packages will not be cached as a file.
        /// </summary>
        /// <param name="cachePath">The path to the folder containing the cached files. </param>
        /// <remarks>
        /// This class does not verify that the <paramref name="cachePath"/>
        /// directory exists, however, when the settings are passed to SharePointPackageReader or SharePointPackageStore, those 
        /// classes will require the directory to exist.
        /// </remarks>
        public SharePointCacheSettings(string cachePath)
        {
            Utilities.ValidateParameterNotEmpty("cachePath", cachePath);

            m_expirationTime = null;
            m_cachePath = cachePath;
        }

        /// <summary>
        /// The path to the folder containing the cached files.
        /// </summary>
        public string CachePath
        {
            get { return m_cachePath; }
        }

        /// <summary>
        /// The minimum amount of time that packages are stored in the cache. After this time has passed, 
        /// the package will be removed from the cache if it has not been accessed. If null, files will never be removed from the 
        /// cache. 
        /// </summary>
        public TimeSpan? ExpirationTime
        {
            get { return m_expirationTime; }
        }

        /// <summary>
        /// Gets and sets the identity used to access the cache.
        /// </summary>
        public ImpersonationBehavior ImpersonationBehavior
        {
            get { return m_impersonationBehavior; }
            set { m_impersonationBehavior = value; }
        }

        /// <summary>
        /// If true, e-learning packages that do not contain basic package information are saved as 
        /// files in the cache. </summary>
        /// <remarks>In particular, this may increase performance in processing of zip files that 
        /// do not contain e-learning content. If false, SharePointPackageReader will not cache zip files 
        /// that are not e-learning content. In that case, an application that wants to cache this file would 
        /// need to cache it as a CachedSharePointFile.
        /// </remarks>
        public bool CacheInvalidPackageAsFile
        {
            get { return m_cacheInvalidPackageAsFile; } 
            set { m_cacheInvalidPackageAsFile = value; }
        }
        private bool m_cacheInvalidPackageAsFile;


        /// <summary>Clones the settings.</summary>
        /// <param name="copyFrom"></param>
        public SharePointCacheSettings Clone()
        {
            return new SharePointCacheSettings(CachePath, ExpirationTime, ImpersonationBehavior, CacheInvalidPackageAsFile);
        }
    }
}
