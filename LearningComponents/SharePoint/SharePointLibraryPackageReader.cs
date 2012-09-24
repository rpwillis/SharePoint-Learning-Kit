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
    /// Represents a reader that can access packages stored and cached in a SharePoint library. 
    /// </summary>
    class SharePointLibraryPackageReader : SharePointLocationPackageReader
    {
        SharePointLibraryCache cache;

#region constructors

        /// <summary>
        /// Creates a package reader to read the specified package from SharePoint. This constructor
        /// optionally causes the file to be read from SharePoint using elevated permissions.
        /// </summary>
        /// <param name="cacheSettings">The settings to use for the caching of this package. 
        /// A subdirectory will be created in the cacheSettings.CachePath location with a cached version of this package.</param>
        /// <param name="packageLocation">The location of the package to be read. Any changes to this SharePointFileLocation
        /// object after the SharePointLibraryPackageReader is created are not reflected in the behavior of this object.</param>
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
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the requested file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the identity doesn't have access to the CachePath provided in the 
        /// cache settings.</exception>
        public SharePointLibraryPackageReader(SharePointCacheSettings cacheSettings, SharePointFileLocation packageLocation, SPFile file) : base(packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("cacheSettings", cacheSettings);

            SPWeb web = SPContext.Current.Web;
            cache = new SharePointLibraryCache(web, file, packageLocation, cacheSettings);
        }
#endregion constructors

#region properties
#endregion properties

#region IDisposable
        /// <summary>Disposes objects.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cache.Dispose();
            }

            base.Dispose(disposing);
        }
#endregion IDisposable

#region Required Overrides
        /// <summary>See <see cref="PackageReader.GetFileStream"/>.</summary>
        public override Stream GetFileStream(string filePath)
        {
            return cache.GetFileStream(filePath);
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
            return cache.FileExists(filePath);
        }

        /// <summary>
        /// Gets the list of package relative file paths of all files in the package.
        /// </summary>
        /// <returns></returns>
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            return cache.GetFilePaths();
        }

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response">The response to write to.</param>
        public override void TransmitFile(string filePath, HttpResponse response)
        {
            cache.TransmitFile(filePath, response);
        }

        #endregion

    }
}
