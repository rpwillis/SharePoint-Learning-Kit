/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.IO;
using Microsoft.LearningComponents.Manifest;
using System.Diagnostics.CodeAnalysis;
using Microsoft.LearningComponents.Storage;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Transactions;
using System.Data;
using System.Xml;
using System.Security.AccessControl;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;

namespace Microsoft.LearningComponents.Storage
{
    
    /// <summary>
    /// Class to import information if the information in the PackageItem table in LearningStore 
    /// refers to files stored in the filesystem. 
    /// </summary>
    /// <remarks>Using this class (as opposed to another PackageStoreclass) is determined by the schema
    /// installed into LearningStore. The packages being imported can exist anywhere, as long 
    /// as there is a PackageReader available to access it. In the case of the core schema, this 
    /// class is used if the PackageItem.Location column contains information about file path locations 
    /// of the package files.
    /// <p/>
    /// In this design, LearningStore contains information that points to 
    /// a location on the file system. The actual resource files are then stored on the file system. 
    /// The manifest is *not* stored in the file system, but rather is stored in the database both 
    /// in a complete form as an xml column, as well as read and imported into a variety of columns 
    /// in LearningStore. 
    /// There is no copy of package resource files stored in LearningStore. 
    /// 
    /// <p/>
    /// One reason for copying the package files to an application-specific location is so that 
    /// the source package files can exist in any package format. For instance, an application may 
    /// import a package in zip format (using ZipPackageReader) to LearningStore. When the package
    /// is added, it is unzipped and stored in the basePath location.
    /// 
    /// <p/>
    /// When the package is added to LearningStore, the files are copied into the basePath location. 
    /// The manifest information is read and then stored in appropriate tables in LearningStore
    /// and the manifest xml is also stored in an xml column in LearningStore. This happens using
    /// ImportManifest() method. Any changes to the source files once the package has 
    /// been added to this PackageStore do not affect the information stored in LearningStore
    /// and are not available to be read using this PackageStore.
    /// 
    /// </remarks>
    public class FileSystemPackageStore : PackageStore
    {
        internal const string DIR_PREFIX = "Package";

        // Base directory where all package files are stored. Each package has a subdirectory in this location.
        private string m_basePath;

        // Identity that has access to the file system. When accessing the files, impersonate this identity.
        private ImpersonationBehavior m_impersonationBehavior = ImpersonationBehavior.UseImpersonatedIdentity;

        /// <summary>
        /// Constructor that creates a FileSystemPackageStore.
        /// </summary>
        /// <param name="learningStore">The <c>LearningStore</c> where packages in this store
        /// are kept.</param>
        /// <param name="basePath">The base path to which files should be 'stored'. That is, the 
        /// Microsoft Learning Components
        /// core schema defines a base path for all packages. When packages are imported
        /// into LearningStore, they are copied to a subdirectory of this basePath directory.Every 
        /// package is given a unique directory for its files when the package is added to the system. 
        /// The application should not modify files in this directory. The directory must exist
        /// before calling this constructor.
        /// </param>
        /// <param name="impersonationBehavior">A user that has read/write access to the <P>basePath</P>
        /// location. In some cases, the identity must also have permission to remove files from the <P>basePath</P>.
        /// The constructor does not verify all the permissions are available. If the identity does not have the 
        /// appropriate permissions, an exception may be thrown from other methods as those permissions are required.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if <P>basePath</P> is not an absolute path 
        /// to an existing directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if <P>impersonationBehavior</P> does not have FileSystemRights.Read to the 
        /// <P>basePath</P> directory.</exception>
        public FileSystemPackageStore(LearningStore learningStore, string basePath, ImpersonationBehavior impersonationBehavior)
            : base(learningStore)
        {
            Utilities.ValidateParameterNonNull("basePath", basePath);
            Utilities.ValidateParameterNonNull("learningStore", learningStore);

            basePath = basePath.Trim();
            Utilities.ValidateParameterNotEmpty("basePath", basePath);

            // The outer try/catch block is there for security reasons. Search MSDN for 
            // "WrapVulnerableFinallyClausesInOuterTry" to see details.
            try
            {
                using (ImpersonateIdentity id = new ImpersonateIdentity(impersonationBehavior))
                {
                    if (!Directory.Exists(basePath))
                    {
                        throw new DirectoryNotFoundException(String.Format(CultureInfo.CurrentCulture, PackageResources.FSPS_BasePathDirNotFound, basePath));
                    }

                    // Test that the identity has read access to the directory. This will throw security exception if it 
                    // does not.
                    Directory.GetFiles(basePath);
                }
            }
            catch
            {
                throw;
            }
                        
            m_basePath = basePath;
            m_impersonationBehavior = impersonationBehavior;
        }

        /// <summary>
        /// Adds a package to PackageStore. The process of adding a package copies the package into a unique 
        /// directory the PackageStore base path. Any changes to the original file after the package is 
        /// added are not reflected in the store.        
        /// </summary>
        /// <param name="packageReader">The package to add to the store.</param>
        /// <param name="packageEnforcement">The settings to determine whether the package should be modified to 
        /// allow it to be added to the store.</param>
        /// <returns>The results of adding the package, including a log of any warnings or errors that occurred in the process.
        /// </returns>
        /// <exception cref="PackageImportException">Thrown if the package could not be added to the store. The exception may 
        /// include a log indicating a cause for the failure.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageReader"/> or <paramref name="packageEnforcement"/>
        /// is not provided.</exception>
        // Justification: The catch is used to make sure the first exception thrown is bubbled to the caller.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public AddPackageResult AddPackage(PackageReader packageReader, PackageEnforcement packageEnforcement)
        {
            Utilities.ValidateParameterNonNull("packageReader", packageReader);
            Utilities.ValidateParameterNonNull("packageEnforcement", packageEnforcement);

            AddPackageReferenceResult refResult;
            string packageLocation = null;
            try
            {
                TransactionOptions options = new TransactionOptions();
                options.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
                using (LearningStoreTransactionScope scope = new LearningStoreTransactionScope(options))
                {
                    // Add the reference to the package
                    refResult = AddPackageReference(packageReader, "$Invalid Value$", packageEnforcement);

                    // Add the resource files and determine the location of them. The location is derived from the packageId.
                    packageLocation = ImportFiles(refResult.PackageId, packageReader);

                    // Update the package location in LearningStore
                    UpdatePackageLocation(refResult.PackageId, packageLocation);

                    scope.Complete();
                }
            }
            catch(Exception)
            {
                try
                {
                    // Delete files if necessary
                    if (!String.IsNullOrEmpty(packageLocation))
                        DeleteFiles(packageLocation);
                }
                catch
                {
                    // If the deletion failed, make sure the original exception is thrown to the caller
                }
                throw;
            }
            return new AddPackageResult(refResult);
        }

        /// <summary>
        /// Read the files from a package and import the package information into LearningStore. Only packages
        /// which may be executed can be imported into LearningStore.
        /// See the class overview information for details.
        /// </summary>
        /// <param name="packageReader">A reader to read the files in the package to be imported.</param>
        /// <param name="packageId">The identifier of the package whose files are being imported.</param>
        /// <returns>Returns the location of the package that was added.</returns>
        /// <remarks>
        /// <p/>This method copies all package files that are referenced in the manifest into a unique
        /// subdirectory in the basePath directory. 
        /// 
        /// <p/>This method will validate that the package does not have any errors when it is 
        /// processed by the <c>PackageValidator</c> class. Warnings that occur during validaton
        /// will have no effect on adding the package.
        /// 
        /// <p/>Only packages which can be excuted may be imported into LearningStore. A package may be 
        /// executed if there is at least one &lt;Organization&gt; nodes within it.
        /// 
        /// <p/>This method creates a transaction, regardless of whether or not it is called within 
        /// a transaction.
        /// 
        /// <p/>The identity passed to the constructor will be used to write the files to the file system. This
        /// account must have appropriate permissions to write to the basePath directory for the package store.
        /// 
        /// <p/>The exceptions thrown by Directory.CreateDirectory() may also be thrown by this method.
        /// </remarks>
        /// <exception cref="PackageImportException">Thrown if the package to be added is not a 
        /// <c>PackageType.ContentAggregation</c> or does not contain
        /// at least one &lt;item&gt; node.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the identity used to create this object
        /// does not have sufficient permissions in the file system directory.</exception>
        private string ImportFiles(PackageItemIdentifier packageId, PackageReader packageReader)
        {
            string relativePackageLocation; // package location unique to this pacakge
                    
            // The outer try/catch block is there for security reasons. Search MSDN for 
            // "WrapVulnerableFinallyClausesInOuterTry" to see details.
            try
            {
                string absPackageLocation = null;

                // Create directories using the identity account that was passed to the store. 
                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // Create the directory, relative to m_basePath
                    relativePackageLocation = CreatePackageDirectory(packageId.GetKey(), 100);

                    // Get the absolution package location of the new package directory
                    absPackageLocation = PackageReader.SafePathCombine(m_basePath, relativePackageLocation);
                }

                if (packageReader.GetType().Equals(typeof(ZipPackageReader)))
                {
                    // Let the zip reader do its own copy, as it's more efficient. Do not impersonate, as the package reader
                    // needs to use its own identity (not the the store's identity) to access the files

                    // ZipPackageReader doesn't want the directory to exist. (We had to create it above to verify it was 
                    // possible).
                    using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                    {
                        Directory.Delete(absPackageLocation);
                    }

                    ZipPackageReader zipReader = packageReader as ZipPackageReader;
                    zipReader.CopyTo(absPackageLocation);
                }
                else
                {
                    foreach (string filePath in packageReader.GetFilePaths())
                    {
                        using (Disposer disposer = new Disposer())
                        {
                            string absFilePath; // absolute location of the file to write
                            string absDirPath;  // absolute location of the drectory to write to 
                            FileStream outputStream;    // stream to write to

                            // Get stream for file from package
                            Stream pkgStream = packageReader.GetFileStream(filePath);
                            disposer.Push(pkgStream);

                            // Create subdirectory, if it's required
                            using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                            {
                                absFilePath = PackageReader.SafePathCombine(absPackageLocation, filePath);
                                absDirPath = Path.GetDirectoryName(absFilePath);
                                if (!File.Exists(absDirPath) && !Directory.Exists(absDirPath))
                                {
                                    // Create it
                                    Directory.CreateDirectory(absDirPath);
                                }

                                // Create file location to write
                                outputStream = new FileStream(absFilePath, FileMode.Create);
                                disposer.Push(outputStream);
                            }

                            // Copy from the pkgStream to the outputStream, using the correct identities
                            Utilities.CopyStream(pkgStream, ImpersonationBehavior.UseImpersonatedIdentity, outputStream, m_impersonationBehavior);
                        }
                   }
                }

                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // Remove imsmanifest.xml from the target directory. It'll be stored in LearningStore and providing two 
                    // copies may cause confusion or sync issues.
                    string manifestFilePath = PackageReader.SafePathCombine(absPackageLocation, "imsmanifest.xml");
                    File.Delete(manifestFilePath);
                }
                
            }
            catch
            {
                throw;
            }
        
            // Return the new package
            return relativePackageLocation;
        }

        /// <summary>
        /// Creates the directory for the package files. This method does not impersonate. 
        /// </summary>
        /// <param name="initialDirectorySuffix">The initial suffix for the directory. The 
        /// new directory will be DIR_PREFIXsuffix.</param>
        /// <param name="retries">The number of tries to find a unique directory name.</param>
        /// <returns>The path to the new directory.</returns>
        /// <exception cref="PackageImportException">If the directory could not be created within the specified number
        /// of retries. (Note that other exceptions relating to security, etc, may also be thrown by the 
        /// Directory.CreateDirectory call.)</exception>
        private string CreatePackageDirectory(long initialDirectorySuffix, int retries)
        {
            string directoryPath = null;
       
            //context = m_identity.Impersonate();
            long dirSuffix = initialDirectorySuffix;
            
            for (int i=0; i<retries; i++, dirSuffix++)
            {
                directoryPath = DIR_PREFIX + dirSuffix.ToString(CultureInfo.InvariantCulture);
                try
                {
                    string dirFullPath = PackageReader.SafePathCombine(m_basePath, directoryPath);
                    // There's some odd behavior in .NET that the directory.create method does not fail if a file
                    // already exists with that name. So, check it explicitly.
                    if (File.Exists(dirFullPath) || Directory.Exists(dirFullPath))
                    {
                        // Try deleting the directory -- it is out of date
                        try
                        {
                            Directory.Delete(dirFullPath, true);
                        }
                        catch(IOException)
                        {
                            // Ignore...the real issue is whether the directory exists, and we're about to 
                            // check that explicitly.
                        }

                        // If it's still there, try finding a different directory
                        if (File.Exists(dirFullPath) || Directory.Exists(dirFullPath))
                        {
                            directoryPath = String.Empty;   // clear it for error detection
                            continue;
                        }
                    }

                    Directory.CreateDirectory(dirFullPath);
                    break;
                }
                catch (IOException)
                {
                    // Directory already exists. Try another one. (We actually should not get here except in 
                    // very special cases where the directory is created by another thread after it's checked here.)

                    directoryPath = String.Empty;   // clear it for error detection
                    continue;
                }
            }
        
            if (String.IsNullOrEmpty(directoryPath))
                throw new PackageImportException(PackageResources.FSPS_CouldNotFindDirectory);

            return directoryPath;
        }

        /// <summary>
        /// Delete a package that was previously added to the store. Deleted information cannot be recovered.
        /// </summary>
        /// <param name="packageId">The package to remove.</param>
        /// <remarks>
        /// <p/>If a file in the package cannot be deleted, the FileDeletionFailed event will be raised.
        /// </remarks>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if the <P>packageId</P> does not 
        /// represent a package in the store.</exception>
        /// <exception cref="LearningStoreConstraintViolationException">Thrown if the package cannot be deleted 
        /// from LearningStore because there are other items in the store that depend upon it. For instance,
        /// a package for which there are attempts in LearningStore, cannot be deleted.</exception>
        public void DeletePackage(PackageItemIdentifier packageId)
        {
            string packageLocation = RemovePackageReference(packageId);

            DeleteFiles(packageLocation);
        }

        /// <summary>
        /// Event handler for FileDeletionFailed event.
        /// </summary>
        public event EventHandler<FileDeletionFailedEventArgs> FileDeletionFailed;

        /// <summary>
        /// Raises the DeletionFailed event.
        /// </summary>
        /// <param name="location">The location of the package which was being deleted.</param>
        /// <param name="errorMessage">The reason for the failure.</param>
        private void OnDeletionFailed(string location, string errorMessage)
        {
            EventHandler<FileDeletionFailedEventArgs> tempEvent = FileDeletionFailed;
            if (tempEvent != null)
                tempEvent(this, new FileDeletionFailedEventArgs(location, errorMessage));
        }

        /// <summary>
        /// Remove the files of the specified package from the file system where they are stored. 
        /// Deleted files cannot be recovered.
        /// </summary>
        /// <param name="packageLocation">The unique location of the files to remove.</param>
        /// <remarks>
        /// If a file in the package could not be removed, a DeletionFailed event is raised. Regardless 
        /// of the number of files or directories that could not be deleted, only one event is raised on
        /// the first error encountered.
        /// </remarks>
        //Justification: All exceptions are caught, regardless of cause, and an event is raised for the application.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void DeleteFiles(string packageLocation)
        {
            // The outer try/catch block is there for security reasons. Search MSDN for 
            // "WrapVulnerableFinallyClausesInOuterTry" to see details.
            try
            {
                string directoryToDelete = null;

                // Now recursively delete the package directory. If this doesn't work, raise events for the application.
                try
                {
                    using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                    {
                        directoryToDelete = PackageReader.SafePathCombine(m_basePath, packageLocation);
                        
                        RecursiveDelete(directoryToDelete);                        
                    }
                }
                catch (Exception e)
                {
                    // Catch all exceptions, since it does not matter why it failed. Just raise an event.
                    OnDeletionFailed(directoryToDelete, e.Message);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Recursively delete the directory and all the files. If something cannot be deleted, the method does
        /// not catch any exceptions, so the cause of the failure will be contained in the system exception. This 
        /// method does not impersonate. 
        /// </summary>
        /// <param name="absDirectoryPath">The absolute path to the directory to remove.</param>
        private void RecursiveDelete(string absDirectoryPath)
        {
            string[] directories = Directory.GetDirectories(absDirectoryPath);
            foreach (string dir in directories)
            {
                // If one failed, give up
                RecursiveDelete(dir);
            }

            // Delete the files. Raise an event if it fails.
            string[] files = Directory.GetFiles(absDirectoryPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            // Delete the directory.
            Directory.Delete(absDirectoryPath);
        }

        /// <summary>
        /// Gets a package reader to read the package from the store.
        /// </summary>
        /// <param name="packageId">The unique identifier of the package to read.</param>
        /// <returns>A package reader to read the requested package</returns>
        /// <exception cref="LearningStoreItemNotFoundException">Thrown if <paramref name="packageId"/>
        /// does not represent a package in the store.</exception>
        public override PackageReader GetPackageReader(PackageItemIdentifier packageId)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);

            string packageLocation;
            
            // Getting the location will fail if the current user does not have access to the package.
            packageLocation = GetPackageLocationFromLS(packageId);

            if (String.IsNullOrEmpty(packageLocation))
                throw new LearningStoreItemNotFoundException(String.Format(CultureInfo.CurrentCulture, 
                    PackageResources.FSPS_InvalidPackageId, packageId.GetKey().ToString(CultureInfo.CurrentCulture)));
            
            return GetPackageReader(packageId, packageLocation);
        }

        /// <summary>
        /// Return a PackageReader that can read files from a package that 
        /// is stored in LearningStore.
        /// </summary>
        /// <param name="packageId">The identifier of the package to be read.</param>
        /// <param name="packageLocation">The location information which defines 
        /// the location of the package. This is the value in the PackageItem.Location column in 
        /// LearningStore. It cannot be String.Empty.</param>
        /// <returns>A package reader for a package referenced in LearningStore.</returns>
        /// <remarks>
        /// </remarks>
        protected internal override PackageReader GetPackageReader(PackageItemIdentifier packageId,
                                                string packageLocation)
        {
            return new FileSystemPackageStoreReader(m_basePath, this, packageId, packageLocation, m_impersonationBehavior);
        }
        
    }


    /// <summary>
    /// Represents the results of adding a package to a PackageStore.
    /// </summary>
    public class AddPackageResult
    {
        private ValidationResults m_log;
        private PackageItemIdentifier m_packageId;

        /// <summary>
        /// Create an AddPackageResult from the results of adding a reference to a package in PackageStore.
        /// </summary>
        /// <param name="addReferenceResult">The results of adding a package reference to PackageStore.</param>
        internal AddPackageResult(AddPackageReferenceResult addReferenceResult)
        {
            m_log = addReferenceResult.Log;
            m_packageId = addReferenceResult.PackageId;
        }

        /// <summary>
        /// Create an AddPackageResult object to encapsulate the results of adding a package.
        /// </summary>
        /// <param name="packageId">The packageId of the package that was added to the store. 
        /// This value is not validated to ensure that it represents 
        /// a package in LearningStore.</param>
        /// <param name="log">The log of errors and warnings that occurred during the course of adding the package to the store.
        /// This may not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageId"/> or <paramref name="log"/>
        /// are not provided.</exception>
        public AddPackageResult(PackageItemIdentifier packageId, ValidationResults log)
        {
            Utilities.ValidateParameterNonNull("packageId", packageId);
            Utilities.ValidateParameterNonNull("log", log);

            m_packageId = packageId;
            m_log = log;
        }

        /// <summary>
        /// The reference that was added to PackageStore.
        /// </summary>
        public PackageItemIdentifier PackageId { get { return m_packageId; } }

        /// <summary>
        /// The log of errors and warnings that occurred in the course of adding the package to PackageStore.
        /// </summary>
        public ValidationResults Log { get { return m_log; } }
    }

    /// <summary>
    /// The arguments passed to a DeletionFailedEventHandler when the event is raised.
    /// </summary>
    public class FileDeletionFailedEventArgs : EventArgs
    {
        private string m_location;
        private string m_message;

        /// <summary>
        /// Create the arguments for the event handler.
        /// </summary>
        /// <param name="location">The location of the package that could not be deleted. The string cannot be null or empty.</param>
        /// <param name="message">A message indicating the cause of the failure. This may be null or empty.</param>
        public FileDeletionFailedEventArgs(string location, string message)
        {
            Utilities.ValidateParameterNonNull("location", location);
            Utilities.ValidateParameterNotEmpty("location", location);

            m_location = location;
            m_message = message;
        }

        /// <summary>
        /// Gets the location of the package that could not be deleted. This should provide enough 
        /// information for the event handler to determine the file or directory that could not be deleted.
        /// </summary>
        public string Location
        {
            get { return m_location; }
        }

        /// <summary>
        /// A message indicating the cause of the failure.
        /// </summary>
        public string Message
        {
            get { return m_message; }
        }
    }
}
