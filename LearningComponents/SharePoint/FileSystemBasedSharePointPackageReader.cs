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
    /// Represents a reader that can access packages stored in SharePoint and permanently cached to the file system. 
    /// </summary>
    public abstract class FileSystemBasedSharePointPackageReader : PackageReader
    {
        private FileSystemPackageReader fileSystemPackageReader;
        ImpersonationBehavior impersonationBehavior;

        private bool m_disposed; // indicates this object has been disposed

#region constructors
        /// <summary>
        /// Creates a package reader which reads the package from a file system cache.
        /// </summary>
        /// <param name="packageLocation">The location of the package to be read. Any changes to this SharePointFileLocation
        /// object after the FileSystemBasedSharePointPackageReader is created are not reflected in the behavior of this object.</param>
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
        /// <exception cref="DirectoryNotFoundException">Thrown if the CachePath property of <paramref name="cacheDirectory"/>
        /// does not exist prior to calling this constructor.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the requested file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the identity doesn't have access to the CachePath provided in the 
        /// cache settings.</exception>
        public FileSystemBasedSharePointPackageReader(SharePointFileLocation packageLocation)
        {
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Utilities.ValidateParameterNonNull("packageLocation", packageLocation);

            Location = new SharePointFileLocation(packageLocation);
        }
#endregion constructors

#region properties
        /// <summary>The location of the package.</summary>
        public SharePointFileLocation Location { get; private set; }

        /// <summary>The cache directory.</summary>
        public DirectoryInfo CacheDirectory { get; private set; }

#endregion properties

#region private methods
        /// <summary>
        /// The FileSystemPackageReader used by this reader
        /// </summary>
        private FileSystemPackageReader FileSystemPackageReader
        {
            get
            {
                if (fileSystemPackageReader == null)
                {
                    fileSystemPackageReader = new FileSystemPackageReader(CacheDirectory.FullName, impersonationBehavior);
                }

                return fileSystemPackageReader;
            }
        }

        /// <summary>
        /// Throws <Typ>ObjectDisposedException</Typ> if this object has been disposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException("SharePointPackageReader");
        }
        
#endregion private methods
        
#region protected methods
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
                if (fileSystemPackageReader != null)
                {
                    fileSystemPackageReader.Dispose();
                    fileSystemPackageReader = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>Initializes the reader with the cache directory.</summary>
        protected void Initialize(DirectoryInfo cacheDirectory, ImpersonationBehavior impersonationBehavior)
        {
            CacheDirectory = cacheDirectory;
            this.impersonationBehavior = impersonationBehavior;
        }
#endregion protected methods

#region Required Overrides
        public override Stream GetFileStream(string filePath)
        {
            CheckDisposed();
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
                        
            return FileSystemPackageReader.GetFileStream(filePath);
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

    }
}
