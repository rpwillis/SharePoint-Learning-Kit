/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using Microsoft.SharePoint;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Security.Principal;		// for WindowsIndentity
using System.Runtime.InteropServices;	// for dllimport
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;

//
// CachedPackage.cs
//
// This file defines five classes that handle the caching mechanism for packages 
// drawn from a SharePoint document library.
//
//  CacheException - An exception that represents a problem in the
//              caching process.  This could be a missing file, a failure to
//              get a cache lock.
//
//  CachedPackage - A class that controls access to cached packages. It must always be
//              called from within a 'using' statement.  Upon creation, the 
//              cache will unpackage the package (if necessary), and hold the cache
//              locked for reads only; upon destruction, the lock is released.
//              If the cache is already locked exclusively by another process,
//              it will wait a set amount of time before giving up.
//
namespace Microsoft.LearningComponents.SharePoint
{

    #region CacheException class
    /// <summary>
    /// A special case exception indicating that an error occurred in the
    /// caching process.   
    /// </summary>
    [Serializable]
    public class CacheException : Exception
    {
        /// <summary>
        /// Creates an instance of CacheException.
        /// </summary>
        public CacheException() :base()             
        {
        }

        /// <summary>
        /// Creates an instance of CacheException with a message.
        /// </summary>
        /// <param name="message">A string describing the conditions leading to the exception</param>
        public CacheException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Creates an instance of CacheException with serialization information.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="context">The context of serialization.</param>
        protected CacheException(SerializationInfo serializationInfo, StreamingContext context) 
                    : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Creates an instance of CacheException based on another exception.
        /// </summary>
        /// <param name="message">A string describing the conditions leading to the exception.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        public CacheException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
    #endregion

    /// <summary>
	/// CachedPackage is the core of the caching mechanisms that holds files  taken
	/// from a SharePoint document library. Instances must be disposed of 
	/// when finished with.  Best practice is to use within a 
	/// using () {} construct, like this:
	/// 
	/// using ( CachedPackage lr = new CachedPackage(settings, location, cacheAsPackage) )
	/// {
	///     // Load a file from the package
	/// }
	/// 
	/// The cache is locked for reading for as long as the code is inside the
	/// 'using' clause.   If the package being requested isn't already in the cache,
	/// it is read from the file in the doc lib.
	/// 
    /// The cache can contain a package (for instance, an exploded version of a scorm zip package)
    /// or an individual file (for instance, a doc file). 
    /// 
	/// </summary>
	internal sealed class CachedPackage : IDisposable
	{
        /// <summary>
        /// Valid values to store in lock file defining how a file was cached.
        /// </summary>
        private enum CachedFileState
        {
            WrittenAsFile = 0,
            WrittenAsPackage,
            InvalidPackageWrittenAsFile
        }

        // The path to the cache directory for this package
        private string m_cacheDir;

		// The path to the cache directory for the requested package
		private SharePointCacheSettings m_settings = null;

		//A filestream object representing the lock file
		private FileStream m_lockFile;

		// Amount of time to keep trying to lock: 2 minutes
		private TimeSpan m_attemptLockingTime = new TimeSpan( 0, 0, 2, 0, 0 );

        // Characters to use when encoding to Base32
        private const string ENCODER_MAP = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";

        // Id stored in every cache file. Increment this if the format of the lock file changes.
        private string CACHE_VERSION_ID = "1";

        // The max number of packages to delete when cleaning up the cache.
        private int NUM_PACKAGES_PER_CACHE_CLEAN = 10;
        
        // The delegate to call in case this is a test scenario.
		private ICachePackageTest m_tester;

        // Keep track of the first exception encountered. If caching fails, this exception 
        // is thrown.
        private Exception m_firstException;

        // Settings relating to caching packages and files.
        private bool m_cacheAsPackage;  // if false, cache as file

        /// <summary>
        /// Creates a CachedPackage object and starts the caching process. If necessary, it creates and 
        /// populates the cache directory. When this constructor returns, the package is cached and locked.
        /// The code must be called within an elevated privileges state in case it needs to call SharePoint.
        /// </summary>
        /// <remarks>
        /// The constructor has minimal error checking (since it's an internal class) It does not explicitly verify:
        /// &lt;ul&gt;
        /// &lt;li&gt;that the file exists,&lt;/li&gt;
        /// &lt;li&gt;that the identity has access to the cache dir,&lt;/li&gt;
        /// &lt;/ul&gt;
        /// These things are assumed by this code to be true. If they are not, the user will get something unexpected.
        ///     </remarks>
        internal CachedPackage(SharePointCacheSettings settings, SharePointFileLocation packageLocation)
            : this(settings, packageLocation, true /*cacheAsPackage*/, null)
        {
        }

        /// <summary>
        /// Creates a CachedPackage object and starts the caching process. If necessary, it creates and 
        /// populates the cache directory. When this constructor returns, the package is cached and locked.
        /// The code must be called within an elevated privileges state in case it needs to call SharePoint.
        /// </summary>
        /// <param name="cachedPackageSettings">Flags that indicate how to deal with caching packages as files.</param>
        /// <remarks>
        /// The constructor has minimal error checking (since it's an internal class) It does not explicitly verify:
        /// &lt;ul&gt;
        /// &lt;li&gt;that the file exists,&lt;/li&gt;
        /// &lt;li&gt;that the identity has access to the cache dir,&lt;/li&gt;
        /// &lt;/ul&gt;
        /// These things are assumed by this code to be true. If they are not, the user will get something unexpected.
        ///     </remarks>
        internal CachedPackage(SharePointCacheSettings settings, SharePointFileLocation packageLocation, 
            bool cacheAsPackage)
            : this(settings, packageLocation, cacheAsPackage, null)
        {
        }

        /// <summary>
        /// Creates a CachedPackage object and starts the caching process. This constructor has the option of passing a 
        /// test delegate. Should only be used for unit test code. If necessary, it creates and 
        /// populates the cache directory. When this constructor returns, the package is cached and locked.
        /// The code must be called within an elevated privileges state.
        /// </summary>
        /// <remarks>
        /// The constructor has minimal error checking (since it's an internal class) It does not explicitly verify:
        /// &lt;ul&gt;
        /// &lt;li&gt;that the file exists,&lt;/li&gt;
        /// &lt;li&gt;that the identity has access to the cache dir,&lt;/li&gt;
        /// &lt;/ul&gt;
        /// These things are assumed by this code to be true. If they are not, the user will get something unexpected.
        ///     </remarks>
        internal CachedPackage(SharePointCacheSettings settings, SharePointFileLocation packageLocation,
                                    bool cacheAsPackage,    
                                    ICachePackageTest cacheTester )
        {
            m_tester = cacheTester;

            m_settings = settings;
            m_cacheAsPackage = cacheAsPackage;

            CachedFileInfo fileInfo = new CachedFileInfo(packageLocation);

            //Construct the names of the lock file & the directory to use for the package
            string cacheDir = GetCacheDirectory(m_settings.CachePath, fileInfo, m_settings.ImpersonationBehavior);
            string lockFileName = cacheDir + ".lock";

            // Try to lock the cache.
            DateTime startLocking = DateTime.Now;
            while (!LockCache(lockFileName, cacheDir, fileInfo))
            {
                if (DateTime.Now - startLocking > m_attemptLockingTime)
                {
                    throw new CacheException(Resources.SPLockTriesExceeded, CachingException);
                }

                System.Threading.Thread.Sleep(750);
            }
        }

        // If true, an invalid package should be cached as a single file
        private bool CacheInvalidPackageAsFile   { get { return m_settings.CacheInvalidPackageAsFile; } }

        // If true, package should never be cached as a package. Always as a file.
        private bool AlwaysCacheAsFile { get { return !m_cacheAsPackage; } }

        // If true, package should be cached as a package. The CacheInvalidPackageAsFile indicates whether on failure, 
        // it should cache as a file.
        private bool AttemptCacheAsPackage  { get { return m_cacheAsPackage; } }

        /// <summary>
        /// Gets and sets the first exception encountered while caching the package.
        /// </summary>
        private Exception CachingException
        {
            get
            {
                return m_firstException;
            }
            set
            {
                // Only save the first one
                if (m_firstException == null)
                    m_firstException = value;
            }
        }

        /// <summary>
        /// Gets the full path to the cache directory for this package. This method does not verify that the 
        /// directory exists and does not validate that the input ids represent valid objects. 
        /// </summary>
        /// <param name="cachePath">The path to the folder containing all packages in the cache.</param>
        /// <param name="packageLocation">The location of the package to cache.</param>
        /// <param name="impersonationBehavior">The identity that has access to the cache folder.</param>
        /// <returns></returns>
        public static string GetCacheDirectory(string cachePath, SharePointFileLocation packageLocation, ImpersonationBehavior impersonationBehavior)
        {
            return GetCacheDirectory(cachePath, new CachedFileInfo(packageLocation), impersonationBehavior);
        }

        /// <summary>
        /// Encode a Guid using a Base32-like encoding
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static string EncodeGuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();

            StringBuilder sb = new StringBuilder(26);

            // First fifteen bytes
            for (int segmentStart = 0; segmentStart < 15; segmentStart += 5)
            {
                ulong segment = ((ulong)bytes[segmentStart] << 32) + ((ulong)bytes[segmentStart + 1] << 24) +
                    ((ulong)bytes[segmentStart + 2] << 16) + ((ulong)bytes[segmentStart + 3] << 8) + ((ulong)bytes[segmentStart + 4]);
                for (int c = 0; c < 8; c++)
                {
                    sb.Append(ENCODER_MAP[(int)(segment & 0x1f)]);
                    segment >>= 5;
                }
            }

            // Last byte
            byte b = bytes[15];
            sb.Append(ENCODER_MAP[b & 0x1f]);
            b >>= 5;
            sb.Append(ENCODER_MAP[b & 0x1f]);

            return sb.ToString();
        }

        /// <summary>
        /// Encode an integer using a Base32-like encoding
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string EncodeInteger(int i)
        {
            StringBuilder sb = new StringBuilder(7);

            for (int c = 0; c < 7; c++)
            {
                sb.Append(ENCODER_MAP[(int)(i & 0x1f)]);
                i >>= 5;
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Helper function to actually determine location of the cache directory for a package.
        /// </summary>
        private static string GetCacheDirectory(string cachePath, CachedFileInfo fileInfo, ImpersonationBehavior impersonationBehavior)
        {
            string relativePath = String.Format(CultureInfo.CurrentCulture, "{0}_{1}",
                EncodeGuid(fileInfo.FileId), EncodeInteger(fileInfo.VersionId));
            string absolutePath = null;

            try
            {
                using (new ImpersonateIdentity(impersonationBehavior))
                {
                    absolutePath = PackageReader.SafePathCombine(cachePath, relativePath);
                }
            }
            catch
            {
                throw;
            }
            return absolutePath;
        }

        /// <summary>
        /// The directory in which the package is cached.
        /// </summary>
        public string CacheDir
        {
            get { return m_cacheDir; }
        }

		/// <summary>
		/// If the package in question hasn't been cached yet, then create the
		/// cache lock file, the cache directory and write the package, then
		/// lock it and return.
		/// 
		/// If the package does already exist in the cache, just try to lock
		/// it and return.
		/// </summary>
		/// <param name="lockFileName">The filename of the lockfile for the 
		/// package in question</param>
		/// <returns>True of the cache directory was successfully locked,
		/// false if a lock couldn't be obtained</returns>
		private bool LockCache( string lockFileName, string cacheDir, CachedFileInfo fileInfo )
		{
			bool lockFileFound = false;

            Debug2(m_settings.ImpersonationBehavior, lockFileName);
			using ( new ImpersonateIdentity(m_settings.ImpersonationBehavior) )
			{
				FileInfo fi = new FileInfo( lockFileName );
				lockFileFound = fi.Exists;
			}

			// If the cache doesn't yet exist, create it.
            if (!lockFileFound)
            {
                Debug1(m_settings.ImpersonationBehavior, lockFileName);
                // And then create the cache directory for the package
                if (!CreateCacheDirectory(lockFileName, cacheDir, fileInfo))
                {
                    Debug3(m_settings.ImpersonationBehavior, lockFileName);
                    return false;
                }
            }
            else
            {
                // The cache exists. However, it may not contain the correct package format.

                using (new ImpersonateIdentity(m_settings.ImpersonationBehavior))
                {
                    if (Directory.Exists(cacheDir)) 
                        Directory.GetFiles(cacheDir);
                }
            }

            // Now lock the cache directory.
			return LockCacheDirectory( lockFileName, cacheDir, fileInfo);
        }

        #region TestingHooks
        [Conditional("MLCDEBUG")]
        private void Debug1(ImpersonationBehavior impersonationBehavior, string lockFileName)
        {
            // Called after lockFileName was determined to exist.
            if ((m_tester != null) && (m_tester.Debug1 != null)) m_tester.Debug1(impersonationBehavior, lockFileName);  
        }

        [Conditional("MLCDEBUG")]
        private void Debug2(ImpersonationBehavior impersonationBehavior, string lockFileName)
        {
            // Before checking if lockFileName exists
            if ((m_tester != null) && (m_tester.Debug2 != null)) m_tester.Debug2(impersonationBehavior, lockFileName);
        }

        [Conditional("MLCDEBUG")]
        private void Debug3(ImpersonationBehavior impersonationBehavior, string lockFileName)
        {
            // After CreateCacheDirectory fails
            if ((m_tester != null) && (m_tester.Debug3 != null)) m_tester.Debug3(impersonationBehavior, lockFileName);
        }

        [Conditional("MLCDEBUG")]
        private void Debug4(ImpersonationBehavior impersonationBehavior, string lockFileName)
        {
            // About to try to create the lockFile with exclusive write privileges
            if ((m_tester != null) && (m_tester.Debug4 != null)) m_tester.Debug4(impersonationBehavior, lockFileName);
        }

        [Conditional("MLCDEBUG")]
        private void Debug5(string lockFileName)
        {
            // About to try to create the lockFile with exclusive write privileges
            if ((m_tester != null) && (m_tester.Debug5 != null)) m_tester.Debug5(lockFileName);
        }
        #endregion

        /// <summary>
		/// Does the actual tasks involved with creating the package cache directory 
		/// and unbundling the package to it. The parent cache directory (m_settings.CachePath)
        /// must already exist. 
		/// </summary>
		/// <param name="lockFileName">The path to the lock file to use. This is located in the directory above the 
        /// cacheDir.</param>
        /// <param name="cacheDir">The cache directory to contain the package. This directory should not exist prior to calling 
        /// this method.</param>
		/// <returns>Returns true if the directory was created.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not have read and write permissions to 
        /// cacheDir.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // exception in the finally block is saved in case it's significant, but is most likely not
        private bool CreateCacheDirectory( string lockFileName, string cacheDir, CachedFileInfo fileInfo )
		{
		    FileStream lockFile = null;
		    bool deleteLockFileAndDir = false;
		    
		    try
		    {		    
			    using (new ImpersonateIdentity(m_settings.ImpersonationBehavior) )
			    {
				    // Try to create the lock file (with exclusive write privs); if it fails, retry
                    try
                    {
                        Debug4(m_settings.ImpersonationBehavior, lockFileName);
                        lockFile = new FileStream(lockFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    }
                    catch (IOException e)
                    {
                        // If we can't get information about the parent directory,
                        // then this will never work, so throw an exception. Note that we don't rethrow the existing 
                        // exception, since in the case of UnauthorizedAccessException (and maybe others?) it may contain
                        // sensitive information, such as the cache directory name.
                        try
                        {
                            Directory.GetFiles(m_settings.CachePath, "*.tmp");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            throw new CacheException(Resources.CacheCachePathNotAccessible, ex);
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            throw new CacheException(Resources.CacheCachePathNotAccessible, ex);
                        }
                        catch (IOException ex)
                        {
                            throw new CacheException(Resources.CacheCachePathNotAccessible, ex);
                        }

                        // retry
                        CachingException = e;
                        return false;
                    }
				    deleteLockFileAndDir = true;
                    
				    // Make sure the directory doesn't exist. (Yes, 
                    // this may get created before we attempt to create it. This is just the preliminary check.)
                    if(Directory.Exists( cacheDir ))
                    {
                        // retry
                        return false;
                    }
                    
                    // Get the file from SharePoint
                    Stream stream;
                    DateTime dateTimeLastModified;  // time the file in SharePoint was last modified
                    string filename;

                    GetSharePointFileData(fileInfo, out stream, out dateTimeLastModified, out filename);
                    using (stream)
                    {

                        // Indicates whether SPFile was written as file or package.
                        CachedFileState cachedFileState;

                        // If application requested to always cache as a file, then do that. Otherwise, attempt to 
                        // cache it as a package and if that fails then perhaps try to cache it as a file.
                        if (AlwaysCacheAsFile)
                        {
                            CacheAsFile(cacheDir, filename, stream);
                            cachedFileState = CachedFileState.WrittenAsFile;
                        }
                        else
                        {
                            try
                            {
                                // The content might be an e-learning package, so try caching it as a package.
                                CacheAsPackage(cacheDir, stream);
                                cachedFileState = CachedFileState.WrittenAsPackage;
                            }
                            catch (InvalidPackageException)
                            {
                                // The package is not valid. If we are supposed to cache it as a file, then do so.
                                // In either case, throw the exception so that caller knows it was not cached as package. 
                                if (CacheInvalidPackageAsFile)
                                {
                                    CacheAsFile(cacheDir, filename, stream);

                                    // If this succeeded, write the information to the lock file
                                    WriteLockFile(CachedFileState.InvalidPackageWrittenAsFile, ref lockFile, stream.Length, dateTimeLastModified);

                                    // Mark it as succeeded so that the lock file and directory are not deleted on exit
                                    deleteLockFileAndDir = false;
                                }

                                // Rethrow the exception. 
                                throw;
                            }
                        }

                        // Write information to lock file and close it.
                        WriteLockFile(cachedFileState, ref lockFile, stream.Length, dateTimeLastModified);
                    }

                    // Everything succeeded
                    deleteLockFileAndDir = false;

			    } // Impersonate
			    return true;
			}
			finally
			{
                // If there is a lock file and there was an error, remove the file and the directory (if they exist)
			    if ((lockFile != null) || deleteLockFileAndDir)
			    {
			        using (new ImpersonateIdentity(m_settings.ImpersonationBehavior))
			        {
			            if (deleteLockFileAndDir)
			            {
			                try 
                            { 
                                Directory.Delete( cacheDir, true ); 
                            }
                            catch (Exception e)
                            {
                                // Save the exception, in case it's the first one and there's a problem that needs to be 
                                // thrown to caller. In most cases, this is ignored.
                                CachingException = new CacheException(Resources.CacheCreateDirCleanupFailed, e);
                            }
			            }
			            if(lockFile != null)
			                lockFile.Close();
			            if (deleteLockFileAndDir)
			            {
			                try { File.Delete( lockFileName ); }
                            catch (Exception e)
                            {
                                // Save the exception, in case it's the first one and there's a problem that needs to be 
                                // thrown to caller. In most cases, this is ignored.
                                CachingException = new CacheException(Resources.CacheCreateDirCleanupFailed, e);
                            }
			            }
			        }
                }			    			   
			}
		}

        /// <summary>
        /// Write the contents of the lock file. The file is closed and lockFile set to null on successful return from 
        /// this method.
        /// </summary>
        /// <param name="writtenAsFile">If true, contents was written as a single file.</param>
        /// <param name="lockFile">The lock file to write to. The caller needs to close this if the method does not succeed.</param>
        /// <param name="lengthOfFile">The length of the SPFile, in bytes.</param>
        /// <param name="dateTimeLastModified">The time the SPFile was last modified.</param>
        private void WriteLockFile(CachedFileState cachedFileState, ref FileStream lockFile, long lengthOfFile, DateTime dateTimeLastModified)
        {
            // write the information to the lock file
            DetachableStream lockDetachableStr = null;
            StreamWriter sw = null;
            try
            {
                lockDetachableStr = new DetachableStream(lockFile);
                using (sw = new StreamWriter(lockDetachableStr))
                {
                    // first 'segment' (required) is the version of the file
                    sw.Write(CACHE_VERSION_ID + " ");

                    // second 'segment' (required), the date/time the sharepoint
                    // file was modified.
                    sw.Write(dateTimeLastModified.Ticks.ToString(NumberFormatInfo.InvariantInfo));

                    // third 'segment' (required), the length of the file in bytes
                    sw.Write(" " + lengthOfFile);

                    // forth 'segment' (required), the format which the package was written
                    sw.Write(" " + ((int)cachedFileState).ToString(NumberFormatInfo.InvariantInfo));

                    // flush the writer so we can detach the stream
                    sw.Flush();

                    lockDetachableStr.Detach();
                }
            }
            catch
            {
                throw new CacheException(Resources.SPCannotWriteToLockFile);
            }

            lockFile.Close();
            lockFile = null;
        }

        /// <summary>
        /// Cache a file into cacheDir without modification. The directory is locked before calling 
        /// this method. This method does not impersonate.
        /// </summary>
        /// <param name="cacheDir"></param>
        /// <param name="stream"></param>
        private static void CacheAsFile(string cacheDir, string filename, Stream stream)
        {
            // Create cache directory
            Directory.CreateDirectory(cacheDir);
            
            // Write the file to the directory
			stream.Seek(0, SeekOrigin.Begin);
            using (FileStream output = new FileStream(PackageReader.SafePathCombine(cacheDir, filename),
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Utilities.CopyStream(stream, ImpersonationBehavior.UseImpersonatedIdentity,
                    output, ImpersonationBehavior.UseImpersonatedIdentity);
            }
        }

        /// <summary>
        /// Cache an e-learning content package to the cacheDir. The directory is locked before calling 
        /// this method. This method with throw InvalidPackageException if the stream does not 
        /// contain a package that has the basic format of an e-learning package.
        /// This method does not impersonate.
        /// </summary>
        private static void CacheAsPackage(string cacheDir, Stream stream)
        {
            // Get a PackageReader to read the package. This will throw InvalidPackageException if the byteArray
            // does not contain a package.
            using (Disposer disposer = new Disposer())
            {
				stream.Seek(0, SeekOrigin.Begin);
                PackageReader packageReader = PackageReader.Create(stream);
                disposer.Push(packageReader);
           
                // If this is not a valid package, throw an exception
                ValidatePackage(packageReader);           
                
                if (packageReader.GetType().Equals(typeof(ZipPackageReader)))
                {
                    // Let the zip reader do its own copy, as it's more efficient
                    ZipPackageReader zipReader = packageReader as ZipPackageReader;

                    zipReader.CopyTo(cacheDir);
                }
                else if (packageReader.GetType().Equals(typeof(LrmPackageReader)))
                {
                    // Treat LrmPackageReader the same as ZipPackageReader
                    LrmPackageReader lrmReader = packageReader as LrmPackageReader;
                    lrmReader.CopyTo(cacheDir);
                }
                else
                {
                    // In other cases, get the list of files and write each one to the cache
                    Directory.CreateDirectory(cacheDir);

                    foreach (string filePath in packageReader.GetFilePaths())
                    {
                        using (Disposer d2 = new Disposer())
                        {
                            // Create subdirectory, if it's required
                            string absFilePath = PackageReader.SafePathCombine(cacheDir, filePath);
                            string absDirPath = Path.GetDirectoryName(absFilePath);
                            if (!File.Exists(absDirPath) && !Directory.Exists(absDirPath))
                            {
                                // Create it
                                Directory.CreateDirectory(absDirPath);
                            }

                            // Get stream for file from package
                            Stream pkgStream = packageReader.GetFileStream(filePath);
                            d2.Push(pkgStream);

                            // Create file location to write
                            FileStream outputStream = new FileStream(absFilePath, FileMode.Create);
                            d2.Push(outputStream);

                            Utilities.CopyStream(pkgStream, ImpersonationBehavior.UseImpersonatedIdentity, outputStream, ImpersonationBehavior.UseImpersonatedIdentity);
                        }
                    }
                }
            }
        }

        private static void ValidatePackage(PackageReader reader)
        {
            ValidationResults results = PackageValidator.Validate(reader);
            StringBuilder logMessage = new StringBuilder(100);
            if (results.HasErrors)
            {
                // Get the list of messages from the log and append them to the message
                foreach (ValidationResult result in results.Results)
                {
                    logMessage.AppendLine(result.Message);
                }
                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, Resources.SPInvalidPackage, logMessage));
            }
            
        }

        /// <summary>
        /// Get information, including the file contents, about a file stored in SharePoint. This method does not impersonate.
        /// </summary>
        /// <param name="spFile">The file to retrieve.</param>
        /// <param name="versionId">The version of the file to retrieve.</param>
        /// <param name="fileContents">A Stream open onto the file.</param>
        /// <param name="lastModified">The time the file was last modified in SharePoint.</param>
        private static void GetSharePointFileData(CachedFileInfo fileInfo, out Stream fileContents, out DateTime lastModified, out string filename)
        {
            SPFile spFile = GetSharePointFile(fileInfo);

            fileInfo.SPFile = spFile;            
            filename = spFile.Name;

            if ( (spFile.Versions.Count == 0) || spFile.UIVersion == fileInfo.VersionId)
            {
                if (spFile.UIVersion != fileInfo.VersionId)
                {
                    throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileVersionNotFound, 
                                                        fileInfo.VersionId.ToString(NumberFormatInfo.CurrentInfo), spFile.Name));
                }

                fileContents = spFile.OpenBinaryStream();
                lastModified = spFile.TimeLastModified;
            }
            else
            {
                // The specified version isn't the current one
                SPFileVersion spFileVersion = spFile.Versions.GetVersionFromID(fileInfo.VersionId);

                if (spFileVersion == null)
                {
                    throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, Resources.SPFileVersionNotFound, spFile.Name,
                                                        fileInfo.VersionId.ToString(NumberFormatInfo.CurrentInfo)));
                }

                fileContents = new MemoryStream(spFileVersion.OpenBinary()); // there's no SPFileVersion.OpenBinaryStream

                // There is no 'last modified' of a version, so use the time the version was created.
                lastModified = spFileVersion.Created;
            }
        }

        /// <summary>
        /// Returns the time the file was last modified. If the fileInfo does not have an spFile, this method accesses SharePoint 
        /// to retrieve it.
        /// </summary>
        /// <param name="fileInfo"></param>
        private static DateTime GetLastModifiedTime(CachedFileInfo fileInfo)
        {
            if (fileInfo.SPFile == null)
                fileInfo.SPFile = GetSharePointFile(fileInfo);

            return fileInfo.SPFile.TimeLastModified;
        }

        /// <summary>
        /// Gets the spFile from SharePoint that matches the information in fileInfo.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]    // the objects are disposed by Disposer
        private static SPFile GetSharePointFile(CachedFileInfo fileInfo)
        {
            SPFile spFile;
            using (Disposer disposer = new Disposer())
            {
                SPSite spSite = new SPSite(fileInfo.SiteId, SPContext.Current.Site.Zone);
                disposer.Push(spSite);

                SPWeb spWeb = spSite.OpenWeb(fileInfo.WebId);
                disposer.Push(spWeb);

                spFile = spWeb.GetFile(fileInfo.FileId);
            }
            return spFile;  
        }


        /// <summary>
		/// Performs the actual tasks involved with just locking a cache directory
		/// </summary>
		/// <param name="lockFileName">The full path of the lockfile for the package.</param>
        /// <param name="cacheDir">The directory containing the cached spFile.</param>
		/// <returns>True if the cache directory was successfully locked, 
		/// False if a lock couldn't be obtained</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // exception in the finally block is saved in case it's significant, but is most likely not
        private bool LockCacheDirectory( string lockFileName, string cacheDir, CachedFileInfo fileInfo )
		{
            FileStream lockFile = null;
            bool deleteLockFileAndDir = false;

			using ( new ImpersonateIdentity(m_settings.ImpersonationBehavior) )
			{
			    try
			    {			    
				    // Try to open the lock file with shared Read privs; if it fails, retry
                    try
				    {
					    lockFile = new FileStream( lockFileName, FileMode.Open, FileAccess.Read, FileShare.Read );
		            }
		            catch (IOException e)
		            {
                        CachingException = e;
		                return false;
		            }

                    // From this point on, assume that every error requires
                    // rebuilding the lock file and directory
                    deleteLockFileAndDir = true;

                    String contents;
                    DetachableStream detachableLockFile = new DetachableStream(lockFile);
                    using (StreamReader sr = new StreamReader(detachableLockFile))
                    {
                       contents = sr.ReadLine();
                       detachableLockFile.Detach();
                    }

				    if (contents == null)
				    {
					    // empty file, something broke during the caching process
					    return false;
				    }
    				else
    				{
    					string[] segments = contents.Split(' ');

    					string cacheVersion = segments[0];

					    // if it's 'version 1'
					    if (cacheVersion == CACHE_VERSION_ID)
					    {
						    // there are 4 required segments: cache version id, date, length and <cachedAsPackage or cachedAsFile>
						    if (segments.Length != 4)
						    {
							    // bad format
							    return false;
						    }

                            // If the lock file date last accessed was not within the last two minutes,
                            // then check if the file has changed. Note that WSS has a filetime granularity of 
                            // 1 minute, which is why we wait 2 minutes to check.
                            if (File.GetLastAccessTime(Path.Combine(m_settings.CachePath, lockFileName))
                                < (DateTime.Now.AddMinutes(-2)))
                            {
                                // Compare the last modified value for the file in SharePoint and see if it is
                                // newer than the one in the cache.  If it is, then remove the cache 
                                // and reload it.
                                long ticksAtCreation = long.Parse(segments[1], NumberFormatInfo.InvariantInfo);

                                if (ticksAtCreation != GetLastModifiedTime(fileInfo).Ticks)
                                {
                                    return false;
                                }
                            }

                            // If the current request is to cache the SPFile in a different format than currently 
                            // cached, stop this process and remove the cache.
                            string cacheFormat = segments[3];
                            CachedFileState cachedFileState = (CachedFileState)Enum.Parse(typeof(CachedFileState), cacheFormat);

                            switch (cachedFileState)
                            {
                                case CachedFileState.InvalidPackageWrittenAsFile:
                                    // This never requires re-try, as the package is invalid, so requesting to 
                                    // cache as package will give the same result.
                                    break;

                                case CachedFileState.WrittenAsPackage:
                                    // If request is to cache as file, then need to retry cache
                                    if (AlwaysCacheAsFile)
                                        return false;
                                    break;

                                case CachedFileState.WrittenAsFile:
                                    // It was cached as a file, without first trying to cache as package. Return
                                    // false so caller can attempt to cache as package.
                                    if (AttemptCacheAsPackage)
                                        return false;
                                    break;
                                default:
                                    return false;   // bad format
                            }                            
					    }
					    else
					    {
						    // bad format
						    return false;
    					} 
    				}
                
				    // If the lock file exists but the directory doesn't, then another
				    // process is probably in the process of deleting the cache, retry
				    // and create it.

				    if (!Directory.Exists( cacheDir) )
				    {
					    // retry
					    return false;
				    }

                    // Check that the lock file has something in it.  If it doesn't
				    // then another process may have failed while unbundling.  Clear it
				    // out, and retry.                
				    if (lockFile.Length == 0)
				    {
					    // retry
					    return false;
				    }
                    
				    // Done!
                    m_lockFile = lockFile;
                    lockFile = null;
                    m_cacheDir = cacheDir;
                    deleteLockFileAndDir = false;
                                        
        			return true;
        	    }
        	    finally
        	    {
        	        if( deleteLockFileAndDir )
        	        {
        	            try { Directory.Delete( cacheDir, true ); } 
                        catch (Exception e)
                        { 
                            // Save the exception, in case it's the first one and there's a problem that needs to be 
                            // thrown to caller. In most cases, this is ignored.
                            CachingException = new CacheException(Resources.CacheLockDirCleanupFailed, e);
                        }
        	        }
        	        if(lockFile != null)
        	           lockFile.Close();
        	        if( deleteLockFileAndDir )
        	        {
                        try { File.Delete(lockFileName); }
                        catch (Exception e)
                        {
                            // Save the exception, in case it's the first one and there's a problem that needs to be 
                            // thrown to caller. In most cases, this is ignored.
                            CachingException = new CacheException(Resources.CacheLockDirCleanupFailed, e);
                        }
        	        } 
        	    }
    	    }
		}

        /// <summary>
        /// Clean up the cache. 
        /// </summary>
        public void Dispose()
		{
            using ( new ImpersonateIdentity(m_settings.ImpersonationBehavior) )
			{
				UnlockCache();
				CleanCache();
			}
		}


		/// <summary>
		/// Performs the tasks associated with unlocking a cache directory. This method does not 
        /// impersonate.
		/// </summary>	
        private void UnlockCache()
		{
			// Close the lock file and update the last accessed time
			if( m_lockFile != null )
				m_lockFile.Close();

			// Update the last accessed time, if it fails then it's likely that
			// another process has a read lock on this.
			try
			{
                Debug5(m_lockFile.Name);
				File.SetLastAccessTime( m_lockFile.Name, DateTime.Now );
			}
			catch (IOException)
			{
                // Not a problem. Another process has a lock and that process will set the access time.
			}
		}

		/// <summary>
		/// Iterates through the cache directories, looking for those that
		/// have expired, then tries to delete them. This method does not impersonate.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")] // the exceptions are suppressed so that the process can continue
        private void CleanCache()
		{
			if( m_settings.CachePath != null )
			{
                string[] lockFileNames = Directory.GetFiles(m_settings.CachePath, "*.lock");

                // Count the number of packages we have removed. Only remove a specified amount each time the 
                // cache is cleared. This prevents huge delays when a database backup is restored and the cache directory
                // contains out of date information on a large number of files.
                int deletedPackages = 0;
                TimeSpan? keepAliveTime = m_settings.ExpirationTime;

				foreach (string filename in lockFileNames)
				{
                    // If we have exceeded the number of packages to delete per process, then stop.
                    if (deletedPackages >= NUM_PACKAGES_PER_CACHE_CLEAN)
                        break;

                    // reinitialize local variables
                    bool dirDeleted = false;
                    bool lockDeleted = false;
                    
					// if we fail in here, it's most likely because there's an exclusive lock
					// on the lock file and the GetLastAccess() or Delete() calls failed.
					// In those cases, just move on to the next one.
                    try
                    {
                        DateTime lastAccess = File.GetLastAccessTime(filename);

                        TimeSpan timeSinceLastAccess = DateTime.Now - lastAccess;

                        // alive for more than the lifetime
                        if ((keepAliveTime != null) && (timeSinceLastAccess > keepAliveTime))
                        {
                            // Try to open the lock file (with exclusive write privs); if it fails, retry
                            FileStream lockFile = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.None);
                            string dirname = filename.Substring(0, filename.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));

                            // this isn't the current cache dir being cleaned up, so we can't 
                            // use the RemoveCache() function, though it does the same steps.

                            try
                            {
                                Directory.Delete(dirname, true);
                                dirDeleted = true;
                            }
                            catch
                            {
                                // Don't care why it failed. Skip it.
                            }

                            lockFile.Close();
                            try
                            {
                                File.Delete(filename);
                                lockDeleted = true;
                            }
                            catch
                            {
                                // Don't care why it failed. Skip it.
                            }
                        }
                    }
                    catch
                    {
                        // Don't care why it failed. Skip it.
                    }
                    
                    if (dirDeleted && lockDeleted)
                        deletedPackages++;

				} // foreach
			}

		} // CleanCache()

        /// <summary>
        /// Private class to encapsulate information about the cached file
        /// </summary>
        private class CachedFileInfo
        {
            private SharePointFileLocation m_location;
            private SPFile m_file;

            public CachedFileInfo(SharePointFileLocation location)
            {
                m_location = location;
            }
           
            public Guid SiteId {  get { return m_location.SiteId; }  }

            public Guid WebId  {  get { return m_location.WebId; }   }

            public Guid FileId { get { return  m_location.FileId; } }

            public int VersionId { get { return m_location.VersionId; } }

            public SPFile SPFile
            {
                // This will be null until it is explicitly set
                get { return m_file; }
                set { m_file = value; }
            } 
        }
	}

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
        /// Copy constructor to create new cache settings.
        /// </summary>
        /// <param name="copyFrom"></param>
        internal SharePointCacheSettings(SharePointCacheSettings copyFrom)
        {
            Utilities.ValidateParameterNonNull("copyFrom", copyFrom);

            m_cachePath = copyFrom.CachePath;
            m_expirationTime = copyFrom.ExpirationTime;
            m_impersonationBehavior = copyFrom.ImpersonationBehavior;
        }

        /// <summary>
        /// Create settings for a class that manages a cache of SharePoint files.
        /// </summary>
        /// <param name="cachePath">The path to the folder containing the cached files.</param>
        /// <param name="expirationTime">The minimum amount of time that packages are stored in the cache. After this time has passed, 
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
            Utilities.ValidateParameterNonNull("cachePath", cachePath);
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
            Utilities.ValidateParameterNonNull("cachePath", cachePath);
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
    }

    internal delegate void TestMethod1(ImpersonationBehavior impersonationBehavior, string info);
    internal delegate void TestMethod2(string info);
    internal interface ICachePackageTest
    {
        
        TestMethod1 Debug1 { get; set; } // Called after lockFileName was determined to exist.
        TestMethod1 Debug2 { get; set; } // Called before checking if lockFileName exists
        TestMethod1 Debug3 { get; set; } // Called after CreateCacheDirectory fails
        TestMethod1 Debug4 { get; set; } // Called before trying to create lockFileName
        TestMethod2 Debug5 { get; set; } // Called before trying to 
    }
}


