/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Manifest;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;

namespace Microsoft.LearningComponents
{
    #region PackageReader classes
    
    /// <summary>
    /// Identifies which <c>WindowsIdentity</c> is used to perform operations
    /// when impersonation is involved.  
    /// </summary>
    public enum ImpersonationBehavior
    {
        /// <summary>
        /// Use the original (non-impersonated) identity.  
        /// </summary>
        UseOriginalIdentity = 0,
        
        /// <summary>
        /// Use the identity that has been impersonated.
        /// </summary>
        UseImpersonatedIdentity
    }
    
    /// <summary>
	/// Provides read-only access to the files contained within a single package.
    /// </summary>
	///
	/// <remarks>
	/// <c>PackageReader</c> is an abstract class which provides an application with a consistent way of accessing
	/// content within a package, regardless of how the package is stored.  Microsoft Learning Components provides
	/// the following implementations of <c>PackageReader</c>:
	/// <ul>
	/// <li><Typ>FileSystemPackageReader</Typ> provides access to a package stored in the file system.</li>
	/// <li><Typ>ZipPackageReader</Typ> provides access to a package stored in a zipped file.</li>
    /// <li><Typ>LrmPackageReader</Typ> provides access to a package stored in an LRM file.</li>
    /// <li><Typ>Microsoft.LearningComponents.SharePoint.SharePointPackageReader</Typ>
    ///     provides access to a package stored in
	/// 	a Windows SharePoint Services document library.  This class is defined in
	/// 	Microsoft.LearningComponents.SharePoint.dll.</li>
	/// </ul>
    /// <para>
    /// <c>PackageReader</c> implements <Typ>IDisposable</Typ>.  Always call the <Mth>Dispose</Mth> method when finished
    /// with a <c>PackageReader</c> object, or use a <c>using</c> statement, to ensure that unmanaged resources are
    /// explicitly and properly released.
    /// </para>
	/// </remarks>
	///
    public abstract class PackageReader : IDisposable
    {
        private ConversionResult m_result;
#region constructors
        // protected constructor to set Culture for resources
        /// <summary>
        /// TODO
        /// </summary>
        protected PackageReader()
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;
            Resources.Culture = Thread.CurrentThread.CurrentCulture;
        }
#endregion constructors

#region properties
        /// <summary>A string which uniquely identifies the package.</summary>
        public virtual string UniqueLocation
        {
            get { throw new NotSupportedException() ;}
        }
#endregion properties

#region public methods
         /// <summary>
        /// Returns either a <Typ>ZipPackageReader</Typ> or <Typ>LrmPackageReader</Typ>, according to the type of
        /// package provided.
        /// </summary>
        /// <param name="package">The zipped package (.zip or .ims) or .lrm package.</param>
        /// <returns>A <Typ>PackageReader</Typ> that reads the supplied package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is null.</exception>
        /// <remarks>The stream is analyzed to determine if it is a zipped file.  If it is not, it is assumed to
        /// be an .lrm package.  In either case, the correctness of the package is not checked here, but rather
        /// in subsequent calls to e.g. <Mth>CreateManifestNavigator</Mth>.
        /// <para>
        /// The PackageReader that is returned uses the current user's credentials to read files from the package.
        /// </para></remarks>
        public static PackageReader Create(Stream package)
        {
            // returns either ZipPackageReader or LrmPackageReader û which is used is determined by
            // looking at the first four bytes in the file to determine if it is a Zip file.  Assume it is
            // an LRM file if it is not a .zip.

            // because the stream may not be seekable, we must save the stream to a file first
            // before determining its type - this should be fine since we must save the files before
            // the zip or lrm package readers can use it.
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            int[] firstFour = new int[4];
            if (package != null)
            {
                FileInfo file = new FileInfo(Path.GetTempFileName());
                using (FileStream fileStream = file.Create())
                {
                    int i;
                    int index = 0;
                    while ((i = package.ReadByte()) != -1)
                    {
                        if (index < 4) firstFour[index++] = i;
                        fileStream.WriteByte((byte)i);
                        if (index == 4)
                        {
                            // we wrote the first four bytes using WriteByte(); write the
                            // remaining bytes more efficiently
                            Utilities.CopyStream(package, ImpersonationBehavior.UseImpersonatedIdentity,
                                fileStream, ImpersonationBehavior.UseImpersonatedIdentity);
                            break;
                        }
                    }
                }
                if (firstFour[0] == 0x50 && firstFour[1] == 0x4b && firstFour[2] == 0x03 && firstFour[3] == 0x04)
                {
                    return new ZipPackageReader(file, true);
                }
                else
                {
                    return new LrmPackageReader(file, true);
                }
            }
            else
            {
                throw new ArgumentNullException("package");
            }
        }

        /// <summary>
        /// Returns either a <Typ>ZipPackageReader</Typ> or <Typ>LrmPackageReader</Typ>, according to the type of
        /// package provided.
        /// </summary>
        /// <param name="fileContents">The zipped package (.zip or .ims) or .lrm package.</param>
        /// <returns>A <Typ>PackageReader</Typ> that reads the supplied package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="package"/> is null.</exception>
        /// <remarks>The contents are analyzed to determine if it is a zipped file.  If it is not, it is assumed to
        /// be an .lrm package.  In either case, the correctness of the package is not checked here, but rather
        /// in subsequent calls to e.g. <Mth>CreateManifestNavigator</Mth>.
        /// <para>
        /// The PackageReader that is returned uses the current user's credentials to read files from the package.
        /// </para></remarks>
        public static PackageReader Create(byte[] fileContents)
        {
            // returns either ZipPackageReader or LrmPackageReader – which is used is determined by
            // looking at the first four bytes in the file to determine if it is a Zip file.  Assume it is
            // an LRM file if it is not a .zip.
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            if (fileContents != null)
            {
                FileInfo file = new FileInfo(Path.GetTempFileName());
                using (FileStream fileStream = file.Create())
                {
                    fileStream.Write(fileContents, 0, fileContents.Length);
                }

                if (fileContents[0] == 0x50 && fileContents[1] == 0x4b && fileContents[2] == 0x03 && fileContents[3] == 0x04)
                {
                    return new ZipPackageReader(file, true);
                }
                else
                {
                    return new LrmPackageReader(file, true);
                }
            }
            else
            {
                throw new ArgumentNullException("fileContents");
            }
        }

        /// <summary>
        /// Returns a <Typ>/System.IO.Stream</Typ> that can read the specified file.
        /// </summary>
        /// 
        /// <param name="filePath">The package-relative path to the file. If this path indicates 
        /// a file that is not in the package, a <c>FileNotFoundException</c> is thrown. 
        /// </param>
        /// 
        /// <returns>A <Typ>/System.IO.Stream</Typ> for the specified file.</returns>
        /// 
        public abstract Stream GetFileStream(string filePath);

        /// <summary>
        /// Returns an <Typ>XPathNavigator</Typ> that points to the &lt;manifest&gt; node of the package manifest.
        /// </summary>
        /// <exception cref="InvalidPackageException">The imsmanifest.xml file is missing from the package, or the
        /// &lt;manifest&gt; is missing from the imsmanifest.xml file.</exception>
        /// <returns><Typ>XPathNavigator</Typ> that points to the &lt;manifest&gt; node of the package manifest.</returns>
        public virtual XPathNavigator CreateManifestNavigator()
        {
            // By default do not log anything and try to do conversion, with replacing invalid values.
            XPathNavigator manifest;
            ValidationResults log;
            CreateManifestNavigator(ValidationBehavior.None, true, out log, out manifest);
            return manifest;
        }

        /// <summary>
        /// Gets a value indicating whether the file exists in the package.
        /// </summary>
        /// 
        /// <param name="filePath">The package-relative path to the file; for example, "imsmanifest.xml"
		///		or "page3/diagram4.gif".  This path should have no URL encoding.</param>
        /// 
        /// <returns><c>true</c> if the file exists; otherwise <c>false</c> if the file does not exist or if the 
        /// file is a directory.</returns>
        /// 
        public abstract bool FileExists(string filePath);

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response">The response to write to.</param>
        public abstract void TransmitFile(string filePath, HttpResponse response); 

        /// <summary>
        /// Gets the collection of file paths in the package.  All paths are relative to the root of the package.
        /// </summary>
        /// <returns>Collection of file paths, relative to the root of the package, in the package.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")] // this is a method and not a property because it may do significant work        
        public abstract ReadOnlyCollection<string> GetFilePaths();
#endregion public methods

#region internal methods
        /// <summary>
        /// Returns an <Typ>XPathNavigator</Typ> that points to the &lt;manifest&gt; node of the package manifest.
        /// </summary>
        /// <exception cref="InvalidPackageException">The imsmanifest.xml file is missing from the package, or the
        /// &lt;manifest&gt; is missing from the imsmanifest.xml file.</exception>
        internal void CreateManifestNavigator(ValidationBehavior lrmValidation, bool fixLrmViolations, out ValidationResults log, out XPathNavigator manifest)
        {
            Stream stream;
            log = null;
            manifest = null;

            // Get Index.xml -- see if it's Class Server package.  This is an optimization since the imsmanifest.xml
            // stream can be gotten directly.
            if (FileExists("Index.xml"))
            {
                using (stream = GetFileStream("Index.xml"))
                {
                    // Allow this conversion to occur again if this method is called, but cache
                    // the result for use by the GetFileStream("imsmanifest.xml") method.
                    m_result = ManifestConverter.ConvertFromIndexXml(stream, GetFilePaths(), fixLrmViolations, lrmValidation);
                }
                log = m_result.Log;
                manifest = m_result.Manifest;
            }
            else
            {
                // If it was not a Class Server package, then see if it's a SCORM package
                try
                {
                    stream = GetFileStream("imsmanifest.xml");
                }
                catch (FileNotFoundException)
                {
                    throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                }
                catch (DirectoryNotFoundException)
                {
                    throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                }
                using (stream)
                {
                    try
                    {
                        XPathDocument doc = new XPathDocument(stream);
                        XPathNavigator nav = doc.CreateNavigator();
                        // move to the first manifest node. First try SCORM 2004
                        XPathExpression expr = Helper.CreateExpressionV1p3(nav, Helper.Strings.Imscp + ":" + Helper.Strings.Manifest);
                        manifest = nav.SelectSingleNode(expr);
                        if (manifest == null)
                        {
                            // Didn't find a SCORM 2004 manifest node.  Try SCORM 1.2.
                            expr = Helper.CreateExpressionV1p2(nav, Helper.Strings.Imscp + ":" + Helper.Strings.Manifest);
                            manifest = nav.SelectSingleNode(expr);
                            if (manifest == null)
                            {
                                throw new InvalidPackageException(String.Format(CultureInfo.CurrentCulture, ValidatorResources.RequiredElementMissing, Helper.Strings.Manifest, ValidatorResources.ManifestNodeMissing));
                            }
                        }
                    }
                    catch (XmlException e)
                    {
                        throw new InvalidPackageException(ValidatorResources.BadXmlInManifest, e);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a reader for the manifest of the package.
        /// </summary>
        /// <param name="manifestSettings">The <Typ>ManifestReaderSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="packageValidatorSettings">The <Typ>PackageValidatorSettings</Typ> to use when parsing the
        /// manifest.  Cannot be null.</param>
        /// <param name="logReplacement">Write message that 'x was replaced by y' when the <Typ>ManifestReaderSettings</Typ>
        /// indicate to fix a value.</param>
        /// <param name="log">Optional log in which to put warnings and errors.  Can be null.</param>
        /// <remarks>
        /// Each call to this creates a new <Typ>ManifestReader</Typ> instance.
        /// </remarks>
        /// <exception cref="XmlException">An error was encountered in the XML data.</exception>
        /// <exception cref="InvalidPackageException">There is no imsmanifest.xml in the package,
        /// or the &lt;manifest&gt; node in the imsmanifest.xml is missing,
        /// or <c>ManifestReaderSettings.FixScormRequirementViolations=false</c> is set and the xml:base attribute on the &lt;manifest&gt;
        /// node is invalid.</exception>
        internal ManifestReader GetManifestReader(ManifestReaderSettings manifestSettings, PackageValidatorSettings packageValidatorSettings,
            bool logReplacement, ValidationResults log)
        {
            XPathNavigator manifest = CreateManifestNavigator();

            return new ManifestReader(this, manifestSettings, packageValidatorSettings, logReplacement, log, manifest);
        }

        /// <summary>
        /// Helper method for the GetFilePaths() public method.  
        /// </summary>
        /// <param name="rootPath">Root path of the manifest.</param>
        /// <returns>Collection of file paths in the package.</returns>
        internal ReadOnlyCollection<string> GetFilePaths(DirectoryInfo rootPath)
        {
            Collection<string> absoluteFilePaths = new Collection<string>();
            Collection<string> relativeFilePaths = new Collection<string>();
            RecursivelyGetFilePaths(rootPath, ref absoluteFilePaths);
            // remove the root path from all the strings
            int rootPathLength = rootPath.FullName.Length;
            // if the rootPath doesn't end in a slash, add one to the length to account for removing it
            // from the beginning of the relative paths.
            if (!rootPath.FullName.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                && !rootPath.FullName.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rootPathLength++;
            }
            foreach (string path in absoluteFilePaths)
            {
                relativeFilePaths.Add(path.Substring(rootPathLength).Replace('\\', '/'));
            }
            return new ReadOnlyCollection<string>(relativeFilePaths);
        }

        /// <summary>
        /// Append a subdirectory to a root path, where the root is a known safe value and the subdirectory
        /// is potentially hostile data.  The root path defines the "highest" in the directory hierarchy
        /// allowed.  E.g. if the root is "c:\foo\bar", only directories beneath "bar" are allowed to be
        /// accessed.
        /// </summary>
        /// 
        /// <param name="root">The root path, which should be a known safe value only, and not hostile
        /// data from an unknown source.</param>
        /// <param name="relativePath">The relativePath beneath the root path.</param>
        /// 
        /// <returns>The combination of the root path and the relative path.</returns>
        /// 
        /// <exception cref="ArgumentException"><paramref name="root"/> or <paramref name="relativePath"/>
        /// contain one or more of the invalid characters defined in InvalidPathChars, or contains a 
        /// wildcard character. 
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="root"/> or <paramref name="relativePath"/>
        /// are null.</exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="relativePath"/> is not a simple
        /// relative path (e.g. it navigates out of the subdirectory hierarchy of the root directory.)</exception>
        internal static string SafePathCombine(string root, string relativePath)
        {
            ValidatorResources.Culture = Thread.CurrentThread.CurrentCulture;
            Resources.Culture = Thread.CurrentThread.CurrentCulture;

            // Check for relativePath being null
            if (relativePath == null)
            {
                throw new ArgumentNullException();
            }
            // make sure root is a full path root
            root = Path.GetFullPath(root);
            // Use the Path.Combine internal method to put together the two paths.
            // Then check to make sure the absolute path still starts with the root.
            string combinedPath = Path.GetFullPath(Path.Combine(root, relativePath));
            // if the combined path is exactly the same as the root, just return the root
            if (String.Compare(root, combinedPath, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return root;
            }
            string rootWithSlash;
            if (!root.EndsWith(@"\", StringComparison.Ordinal) && !root.EndsWith("/", StringComparison.Ordinal)) rootWithSlash = root + @"\";
            else rootWithSlash = root;
            if (!combinedPath.StartsWith(rootWithSlash, StringComparison.OrdinalIgnoreCase))
            {
                // don't wrap the inner exception to obfuscate any system file info that might exist
                throw new UnauthorizedAccessException(String.Format(CultureInfo.InvariantCulture, Resources.UnauthorizedAccess, relativePath));
            }
            return combinedPath;
        }

        /// <summary>
        /// Returns true if filePath == "imsmanifest.xml" case-insensitive, culture invariant.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static bool IsImsManifest(string filePath)
        {
            if (String.Compare(filePath, "imsmanifest.xml", StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            else return false;
        }

        /// <summary>
        /// If there is an m_result cached from a call to CreateManifestNavigator, return that manifest.
        /// Otherwise, create a manifest using a relaxed conversion.
        /// </summary>
        /// <returns>Contents of the imsmanifest.xml converted from the index.xml.</returns>
        internal Stream ConvertFromIndexXml()
        {
            ConversionResult result;
            if (m_result == null || m_result.Manifest == null)
            {
                result = ManifestConverter.ConvertFromIndexXml(GetFileStream("index.xml"), GetFilePaths(),
                    true, ValidationBehavior.None);
            }
            else
            {
                result = m_result;
            }
            using(DetachableStream output = new DetachableStream(result.Manifest.OuterXml.Length))
            {
                using (XmlWriter writer = XmlWriter.Create(output))
                {
                    result.Manifest.WriteSubtree(writer);
                    writer.Flush();
                    output.Detach();
                }
                output.Stream.Seek(0, SeekOrigin.Begin);
                return output.Stream;
            }
        }

#endregion internal methods

#region private methods
        /// <summary>
        /// Recursively get the file paths from the given directory and all subdirectories.  The paths gotten are absolute paths,
        /// not paths relative to the package root.
        /// </summary>
        /// <param name="path">Directory to parse.</param>
        /// <param name="filePaths">Collection in which to place the file paths</param>
        private void RecursivelyGetFilePaths(DirectoryInfo path, ref Collection<string> filePaths)
        {
            FileInfo[] files = path.GetFiles();
            foreach (FileInfo file in files)
            {
                filePaths.Add(file.FullName);
            }
            foreach (DirectoryInfo sub in path.GetDirectories())
            {
                RecursivelyGetFilePaths(sub, ref filePaths);
            }
        }

#endregion private methods

        #region IDisposable Members
        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        /// <remarks>
        /// On the base <Typ>PackageReader</Typ> this currently does nothing.  However, good practice dictates
        /// that classes derived from <Typ>PackageReader</Typ> still call the base class <c>Dispose(bool disposing)</c>
        /// as the final operation of a derived class's <c>Dispose(bool disposing)</c> method.
        /// </remarks>
        /// <param name="disposing">True if this method was called from
        ///    <Mth>/System.IDisposable.Dispose</Mth></param>
        protected virtual void Dispose(bool disposing) { /* no op */ }

        /// <summary>
        /// This method supports the .NET Framework infrastructure and is not intended to be used
        /// directly from your code.
        /// <para>
        /// Releases all resources used by the <Typ>PackageReader</Typ>.
        /// </para>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
	/// Provides read-only access to the files contained within a single package that is stored in the file system.
    /// </summary>
	///
	/// <remarks>
	/// <para>
	/// The package can be stored on the local hard drive, removable storage, or on a network file share accessible
	/// via a mapped drive letter or UNC path.
	/// </para>
	/// <para>
	/// See <Typ>PackageReader</Typ> for examples of how classes based on <Typ>PackageReader</Typ> (such as this one)
	/// can be used.
	/// </para>
    /// <para>
    /// The only state maintained by <c>FileSystemPackageReader</c> is the file system path to the package, which is
    /// set in the constructor and never changes for the lifetime of the object.
    /// </para>
	/// </remarks>
	///
    public class FileSystemPackageReader : PackageReader
    {
        /// <summary>
        /// The path to the directory that is the root of the package, and contains the manifest
        /// for the package.  This is valid after the <c>FileSystemPackageReader</c>constructor executes,
        /// however the directory isn't checked for existence until <c>GetFileStream</c> is called.
        /// </summary>
        private string m_packageBasePath;

        /// <summary>
        /// The identity used to access the cache of files. This identity must have read access to the source 
        /// files of the package and read/write access to the package base path.
        /// </summary>
        private ImpersonationBehavior m_impersonationBehavior;

        /// <summary>
        /// Initializes a new instance of the <c>FileSystemPackageReader</c> class with the
        /// package that is stored in the specified file system path. Use the current user's identity to 
        /// access the package.
        /// </summary>
        /// 
        /// <param name="packageBasePath">The path to the directory that is the base of the package, and
        /// contains the manifest for the package.</param>
        /// 
        /// <exception cref="ArgumentException"><paramref name="packageBasePath"/> contains invalid characters
        /// such as ", &lt;, &gt;, or |.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="PathTooLongException">The specified path exceeds the 
        /// system-defined maximum length. For example, on Windows-based platforms, paths must be less 
        /// than 248 characters, and file names must be less than 260 characters. The specified path
        /// is too long.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="packageBasePath"/> is a null
        /// reference.</exception>
        /// 
        /// <remarks>
        /// The package should be exploded (not in zip format) on the file system.  Zipped packages can be
        /// read with the <Typ>ZipPackageReader</Typ>.  Learning Resource packages can be read with the
        /// <Typ>LrmPackageReader</Typ>.
        /// <para>
        /// This constructor does not check if a directory exists. This constructor is a placeholder 
        /// for a string that is used to access the disk in subsequent operations.
        /// </para>
        /// <para>
        /// The <paramref name="packageBasePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// 
        public FileSystemPackageReader(string packageBasePath) : this(packageBasePath, ImpersonationBehavior.UseImpersonatedIdentity)
        {
            // Create an object without identifying an identity. Use the current identity for file accesses.
        }

        /// <summary>
        /// Initializes a new instance of the <c>FileSystemPackageReader</c> class with the
        /// package that is stored in the specified file system path. Uses the specified identity 
        /// to access files in the package.
        /// </summary>
        /// 
        /// <param name="packageBasePath">The path to the directory that is the base of the package, and
        /// contains the manifest for the package.</param>
        /// <param name="impersonationBehavior">The identity which has read access to the files in the located in 
        /// <paramref name="packageBasePath"/>. </param>
        /// 
        /// <exception cref="ArgumentException"><paramref name="packageBasePath"/> contains invalid characters
        /// such as ", &lt;, &gt;, or |.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="PathTooLongException">The specified path exceeds the 
        /// system-defined maximum length. For example, on Windows-based platforms, paths must be less 
        /// than 248 characters, and file names must be less than 260 characters. The specified path
        /// is too long.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="packageBasePath"/> is a null
        /// reference.</exception>
        /// 
        /// <remarks>
        /// The package should be exploded (not in zip format) on the file system.  Zipped packages can be
        /// read with the <Typ>ZipPackageReader</Typ>.  Learning Resource packages can be read with the
        /// <Typ>LrmPackageReader</Typ>.
        /// <para>
        /// This constructor does not check if a directory exists. This constructor is a placeholder 
        /// for a string that is used to access the disk in subsequent operations.
        /// </para>
        /// <para>
        /// The <paramref name="packageBasePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// 
        public FileSystemPackageReader(string packageBasePath, ImpersonationBehavior impersonationBehavior)
        {
            // Modeling this after the DirectoryInfo constructor.  The DirectoryInfo constructor throws
            // the exceptions noted above.  The overhead of creating and destroying the DirectoryInfo
            // structure is made up for by the robustness of using an established .NET object for
            // checking for these exceptions.
            m_packageBasePath = new DirectoryInfo(packageBasePath).FullName;

            m_impersonationBehavior = impersonationBehavior;
        }

        /// <summary>
        /// Returns true if the manifest exists
        /// </summary>
        private bool ManifestExists()
        {
            try
            {
                if(File.Exists(Path.Combine(m_packageBasePath, "imsmanifest.xml")))
                    return true;
                if (File.Exists(Path.Combine(m_packageBasePath, "index.xml")))
                    return true;
            }
            // catch exceptions that should be converted into a "false" file exists.
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
        /// Returns a <Typ>/System.IO.Stream</Typ> that can read the specified file.
        /// </summary>
        /// 
        /// <param name="filePath">The package-relative path to the file. If this path indicates 
        /// a file that is not in the package, a <Typ>/System.IO.FileNotFoundException</Typ> is thrown. 
        /// </param>
        /// 
        /// <returns>A <Typ>/System.IO.Stream</Typ> for the specified file.</returns>
        /// 
        /// <exception cref="ArgumentException"><paramref name="filePath"/> contains one or more invalid
        /// characters, or contains a wildcard character.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined 
        /// maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, 
        /// and file names must be less than 260 characters.</exception>
        /// <exception cref="FileNotFoundException">The file specified was not found.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in an invalid format.</exception>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>/System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// <c>packageBasePath</c> parameter of the <Typ>FileSystemPackageReader</Typ>
        /// constructor.
        /// </remarks>
        /// 
        public override Stream GetFileStream(string filePath)
        {
            return GetFileStream(filePath, false);
        }

        /// <summary>
        /// Returns a <Typ>/System.IO.Stream</Typ> that can read the specified file, providing the option to 
        /// validate the package.
        /// </summary>
        internal Stream GetFileStream(string filePath, bool skipValidation)
        {
            // SafePathCombine throws the ArgumentException and ArgumentNullException noted above.
            // File.OpenRead throws UnauthorizedAccessException, PathTooLongException,
            // DirectoryNotFoundException, FileNotFoundException, and NotSupportedException.

            if (!skipValidation)
            {
                // If the current user has access problems with the package, the package is not Validated
                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // Check if user has access to the files
                    Directory.GetFiles(m_packageBasePath);
                    // Check if package has valid e-learning content
                    if (!ManifestExists())
                        throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                }
            }

            // if filePath is imsmanifest.xml and there is an index.xml, assume this is IMS+ content
            // and return the imsmanifest.xml converted from index.xml instead of the actual imsmanifest.xml.
            if (IsImsManifest(filePath) && FileExists("index.xml"))
            {
                try
                {
                    return ConvertFromIndexXml();
                }
                catch (InvalidPackageException)
                {
                    // don't wrap the inner exception to obfuscate any system file info that might exist
                    throw new FileNotFoundException(Resources.PackageFileNotFound, filePath);
                }
            }
            else
            {
                return ReadFile(m_packageBasePath, filePath, m_impersonationBehavior);
            }
        }

        /// <summary>
        /// Shared helper function to read a file from the package location and wrap the resulting exceptions
        /// to be consistent within MLC.
        /// </summary>
        /// <param name="packagePath">The path to the root folder of the package.</param>
        /// <param name="filePath">The package-relative path of the file within the package.</param>
        /// <param name="impersonationBehavior">Indicates which identity to use when reading the file.</param>
        /// <returns>A stream containing the file.</returns>
        internal static Stream ReadFile(string packagePath, string filePath, ImpersonationBehavior impersonationBehavior)
        {
            Stream stream = null;
            try
            {
                using (ImpersonateIdentity id = new ImpersonateIdentity(impersonationBehavior))
                {
                    stream = File.OpenRead(SafePathCombine(packagePath, filePath));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // don't wrap the inner exception to obfuscate any system file info that might exist
                throw new UnauthorizedAccessException(String.Format(CultureInfo.InvariantCulture, Resources.UnauthorizedAccess, filePath));
            }
            catch (FileNotFoundException)
            {
                // don't wrap the inner exception to obfuscate any system file info that might exist
                throw new FileNotFoundException(Resources.PackageFileNotFound, filePath);
            }
            catch (DirectoryNotFoundException)
            {
                // don't wrap the inner exception to obfuscate any system file info that might exist
                throw new DirectoryNotFoundException(String.Format(CultureInfo.InvariantCulture, Resources.DirectoryNotFound, filePath));
            }
            return stream;
        }

        /// <summary>
        /// Gets a value indicating whether the file exists in the package.
        /// </summary>
        /// 
        /// <param name="filePath">The package-relative path to the file.  This path should have no URL encoding.</param>
        /// 
        /// <returns><c>true</c> if the file exists and the caller has permission to access it; otherwise <c>false</c>.
        /// </returns>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// <c>packageBasePath</c> parameter of the <Typ>FileSystemPackageReader</Typ>
        /// constructor.
        /// </remarks>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// 
        public override bool FileExists(string filePath)
        {
            return FileExists(filePath, false);
        }

        /// <summary>
        /// Gets a value indicating whether the file exists in the package, providing the option to 
        /// validate the package.
        /// </summary>
        internal bool FileExists(string filePath, bool skipValidation)
        {
            bool fileExists = false;
            try
            {
                if (!skipValidation)
                {
                    // If the current user has access problems with the package, the package is not Validated
                    using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                    {
                        // Check if user has access to the files
                        Directory.GetFiles(m_packageBasePath);
                        // Check if package has valid e-learning content
                        if (!ManifestExists())
                            throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                    }
                }

                // SafePathCombine throws the ArgumentException and ArgumentNullException noted above.
                // File.Exists will throw UnauthorizedAccessException if user does not have FileIOPermission.
                string path;
                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    path = SafePathCombine(m_packageBasePath, filePath);
                    fileExists = File.Exists(path);
                    // imsmanifest.xml is special, since it is created from index.xml.
                    if (!fileExists && IsImsManifest(filePath))
                    {
                        // one more chance for imsmanifest.xml - if it doesn't exist as "imsmanifest.xml" it
                        // can still exist as a conversion from index.xml, so return true if index.xml exists.
                        fileExists = ManifestExists();
                    }
                }
            }
            // catch exceptions that should be converted into a "false" file exists.
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
            return fileExists;
        }

        /// <summary>
        /// Gets the collection of file paths in the package.
        /// </summary>
        /// <returns>Collection of file paths in the package.</returns>
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            return GetFilePaths(false);
        }

        /// <summary>
        /// Gets the collection of file paths in the package, with the option to skip package validation.
        /// </summary>
        internal ReadOnlyCollection<string> GetFilePaths(bool skipValidation)
        {
            ReadOnlyCollection<string> filePaths = null;
            using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
            {
                // If the user does not have access, the UnauthorizedAccess Exception is thrown
                Directory.GetFiles(m_packageBasePath);
                // If validation is requested, check if package has valid e-learning content
                if ((!skipValidation) && (!ManifestExists()))
                    throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                filePaths = GetFilePaths(new DirectoryInfo(m_packageBasePath));
            }
            return filePaths;
        }

        /// <summary>
        /// Writes the requested file from the package directly to the response.
        /// This method clears the response before sending the file.
        /// </summary>
        /// <param name="filePath">The package-relative path to a file.</param>
        /// <param name="response">The response to write to.</param>
        public override void TransmitFile(string filePath, HttpResponse response)
        {
            TransmitFile(filePath, response, false);
        }

        /// <summary>
        /// Writes the requested file from the package directly to the response.
        /// This method clears the response before sending the file and provides an option to skip 
        /// package validation.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="response"></param>
        /// <param name="skipValidation"></param>
        internal void TransmitFile(string filePath, HttpResponse response, bool skipValidation)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            Utilities.ValidateParameterNonNull("response", response);

            if (!skipValidation)
            {
                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // If user does not have access, a UnauthorizedAccess Exception is thrown
                    Directory.GetFiles(m_packageBasePath);
                    // Check if package has valid e-learning content
                    if (!ManifestExists())
                        throw new InvalidPackageException(Resources.ImsManifestXmlMissing);
                }
            }

            string absoluteFilePath = SafePathCombine(m_packageBasePath, filePath);
            response.Clear();
            response.Buffer = false;
            response.BufferOutput = false;
            using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
            {
                response.TransmitFile(absoluteFilePath);
            }
        }
    }

    /// <summary>
    /// Provides read-only access to the files contained within a single package, when the package is stored in LRM format.
    /// This is most commonly *.lrm files. The reader will only allow reading packages that are single Learning Resource files.
    /// Other types of files (for instance, multi-LR LRMs, license files or remote content files) will cause exceptions when 
    /// files from the package are requested.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The LRM file can be stored on the local hard drive, removable storage, or on a network file share accessible
    /// via a mapped drive letter or UNC path.  However, note that performance may be very poor if the file name is a
    /// UNC path.
    /// </para>
    /// <para>
    /// This class can be used to import a package contained in an LRM file, or for instance
    /// to access the contents of an LRM package.
    /// </para>
    /// <para>
    /// The LRM package is unbundled to a temporary directory on the first call to <Mth>FileExists</Mth> or
    /// <Mth>GetFileStream</Mth>.  This temporary directory is maintained for the life of the <c>LrmPackageReader</c>,
    /// and is deleted in the <Mth>Dispose</Mth> or finalizer. The identity passed into the constructor, or the current 
    /// user must have permissions to allow unbundling and reading the package files in the temporary directory.
    /// </para>
    /// <para>
    /// See <Typ>PackageReader</Typ> for examples of how classes based on <Typ>PackageReader</Typ> (such as this one)
    /// can be used.
    /// </para>
    /// <para>
    /// The <Typ>LrmPackageReader</Typ> class calls unmanaged code.  Since this assembly is used in the GAC, it has unmanaged code permissions
    /// implicitly.
    /// </para>
    /// <para>
    /// The <Typ>FileSystemPackageReader</Typ> and <Typ>LrmPackageReader</Typ> classes both require <Typ>FileIOPermission</Typ> to
    /// write to the file system.  Again, this is implicit as this assembly is a "full trust" assembly.
    /// </para>
    /// </remarks>
    public class LrmPackageReader : PackageReader
    {
        /// <summary>
        /// The LRM file containing the package.
        /// </summary>
        private FileInfo m_lrm;

        /// <summary>
        /// True if Dispose() should delete the file referenced by m_lrm.
        /// </summary>
        bool m_mustDeleteFile;

        /// <summary>
        /// A stream containing the package.
        /// </summary>
        private Stream m_stream;

        /// <summary>
        /// The path into which the files in <paramref name="m_lrm"/> should be exploded.
        /// </summary>
        private DirectoryInfo m_unbundlePath;

        /// <summary>
        /// The identity which should be impersonated when reading / writing files in the file system.
        /// </summary>
        private ImpersonationBehavior m_impersonationBehavior = ImpersonationBehavior.UseImpersonatedIdentity;

        /// <summary>
        /// States this reader can be in.
        /// </summary>
        private enum LrmPackageReaderState
        {
            /// <summary>
            /// Initial state.  Lrm file has not been exploded.
            /// </summary>
            Init,
            /// <summary>
            /// Lrm file has been exploded successfully.
            /// </summary>
            Exploded,
            /// <summary>
            /// After Dispose() is called.
            /// </summary>
            Disposed
        };

        /// <summary>
        /// Current reader state.
        /// </summary>
        private LrmPackageReaderState m_state = LrmPackageReaderState.Init;

        // Underlying package reader used once the stream has been decompressed to the file system.
        private FileSystemPackageReader m_fsPackageReader;
        /// <summary>
        /// Underlying package reader used once the stream has been decompressed to the file system.
        /// </summary>
        private FileSystemPackageReader FsPackageReader
        {
            get
            {
                if (m_fsPackageReader == null)
                {
                    m_fsPackageReader = new FileSystemPackageReader(ExplodeLrmIfNeeded(), m_impersonationBehavior);
                }
                return m_fsPackageReader;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <c>LrmPackageReader</c> class with the specified LRM package.
        /// Uses the current user's identity to access files in the package.
        /// </summary>
        /// 
        /// <param name="filePath">The path to the LRM file containing the package, e.g. "C:\Foo\Bar.lrm".</param>
        /// 
        /// <remarks>
        /// <para>
        /// This constructor does not check if the file exists. This constructor is a placeholder 
        /// for a string that is used to access the file in subsequent operations.
        /// </para>
        /// <para>
        /// The <paramref name="filePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        public LrmPackageReader(string filePath) : this(filePath, ImpersonationBehavior.UseImpersonatedIdentity)
        {
            // m_impersonationBehavior is not set, resulting in current user's identity being used for all file accesses
        }

        /// <summary>
        /// Initializes a new instance of the <c>LrmPackageReader</c> class with the specified LRM package.
        /// Uses a specified user's identity to access files in the package.
        /// </summary>
        /// 
        /// <param name="filePath">The path to the LRM file containing the package, e.g. "C:\Foo\Bar.lrm".</param>
        /// <param name="impersonationBehavior">The identity which has access to the files in the package.</param>
        /// 
        /// <remarks>
        /// <para>
        /// The <paramref name="filePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        public LrmPackageReader(string filePath, ImpersonationBehavior impersonationBehavior)
        {
            m_impersonationBehavior = impersonationBehavior;
            m_lrm = new FileInfo(filePath);
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="filePath">The path to the bundled file containing the package, e.g. "C:\Foo\Bar.lrm".</param>
        /// <param name="disposeFile"><c>true</c> to delete <paramref name="filePath"/> on <c>Dispose()</c>.</param>
        internal LrmPackageReader(FileInfo filePath, bool disposeFile)
        {
            m_lrm = filePath;
            m_mustDeleteFile = disposeFile;
        }

        /// <summary>
        /// Initializes a new instance of the <c>LrmPackageReader</c> class with the specified stream.
        /// </summary>
        /// <param name="stream">Stream containing a valid LRM package.</param>
        /// <remarks>
        /// <para>
        /// This constructor does not check if the stream contains a valid LRM package. This constructor is a placeholder 
        /// for a stream that is used to access the data in subsequent operations.
        /// </para>
        /// </remarks>
        public LrmPackageReader(Stream stream)
        {
            m_stream = stream;
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage
        /// collector reclaims the <c>LrmPackageReader</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// The garbage collector calls this when the current object is ready to be finalized.
        /// </remarks>
        /// 
        ~LrmPackageReader()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by this object
        /// </summary>
        /// <param name="disposing">True if this method was called from
        ///    <Mth>/System.IDisposable.Dispose</Mth></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (m_fsPackageReader != null)
                {
                    m_fsPackageReader.Dispose();
                    m_fsPackageReader = null;
                }

                if (m_state == LrmPackageReaderState.Disposed)
                {
                    return;
                }
                // If the current state is "exploded", delete the directory, files, and all
                // subdirectories and files.  Note that the only way the current state becomes
                // "exploded" is with a successful unbundle operation.
                if (m_state == LrmPackageReaderState.Exploded)
                {
                    if (m_unbundlePath != null)
                    {

                        try
                        {
                            m_unbundlePath.Delete(true);
                        }
                        catch (IOException)
                        {
                            // file can't be deleted if currently open.  Unfortunately this means there will be
                            // files left behind.
                            // noop
                        }
                    }
                    // delete the created file if needed.
                    if (m_mustDeleteFile && m_lrm != null)
                    {
                        m_lrm.Delete();
                    }
                }
            }
            finally
            {
                m_state = LrmPackageReaderState.Disposed;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Explodes the m_lrm file into a newly created (randomly named, in the temporary directory)
		/// destination directory if this hasn't been done already.
        /// </summary>
        /// <remarks>
        /// <c>m_state</c> affects what this method does, and can be changed by this method.
        /// <c>m_unbundlePath</c> can be changed by this method.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">State of the reader is "Closed".</exception>
        /// <exception cref="InvalidPackageException">There is a problem with the LRM package.</exception>
        /// <returns>Package root path.</returns>
        private string ExplodeLrmIfNeeded()
        {
            if (m_state == LrmPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("LrmPackageReader");
            }
            if (m_state != LrmPackageReaderState.Exploded)
            {
                // If there is an m_stream but no m_lrm, we still need to stream the stream into a temp file.
                if (m_stream != null && m_lrm == null)
                {
                    using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                    {
                        m_lrm = new FileInfo(Path.GetTempFileName());
                        using (FileStream fileStream = m_lrm.Create())
                        {
                            // Read from the stream and write to the exploded location using the specified identity.
                            Utilities.CopyStream(m_stream, m_impersonationBehavior, fileStream, m_impersonationBehavior);
                        }
                    }
                    m_mustDeleteFile = true;
                }

                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // Note that Path.Combine is safe here since Path.GetTempPath() and Path.GetRandomFileName()
                    // are known to be safe.
                    // Just in case Path.GetRandomFileName() returns a filename that already exists, keep trying
                    // until success.
                    bool done = false;
                    while (!done)
                    {
                        m_unbundlePath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                        if (m_unbundlePath.Exists) continue;
                        m_unbundlePath.Create();
                        done = true;
                    }
                    // explode the lrm file
                    try
                    {
                        Compression.Unbundle(m_lrm, m_unbundlePath);
                        m_state = LrmPackageReaderState.Exploded;
                    }
                    catch (Exception e)
                    {
                        string message = "";
                        // wrap this message in another descriptive message
                        message = String.Format(CultureInfo.InvariantCulture, ValidatorResources.PackageCouldNotBeOpened, message);
                        throw new InvalidPackageException(message, e);
                    }
                }
            }
            return m_unbundlePath.ToString();
        }

        /// <summary>
        /// Returns a <Typ>/System.IO.Stream</Typ> for the file inside the LRM package referenced by 
        /// <paramref name="filePath"/>.
        /// </summary>
        /// 
        /// <param name="filePath">The file to retrieve.  The path is relative to the root of the file
        /// hierarchy in the LRM package.  If this path indicates a file that is not in the LRM
        /// package, a <Typ>/System.IO.FileNotFoundException</Typ> is thrown.</param>
        /// 
        /// <returns><Typ>/System.IO.Stream</Typ> for the specified file.</returns>
        /// 
        /// <exception cref="IOException">The file is already open.</exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> contains invalid characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The <Typ>LrmPackageReader</Typ> is in an error state.</exception>
        /// <exception cref="FileNotFoundException">The file specified was not found.</exception>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for creating directories and
        /// reading and writing files.  Associated enumerations: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp> and
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Write</Prp>.
        /// </permission>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// root of the LRM package.
        /// <para>
        /// The LRM package will be exploded into a temporary directory when this method is called,
        /// if not done previously.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override Stream GetFileStream(string filePath)
        {
            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            Stream retVal = FsPackageReader.GetFileStream(filePath);
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Gets a value indicating whether a file exists in the LRM package. 
        /// </summary>
        /// 
        /// <param name="filePath">The file to check.  The path is relative to the root of the file
        /// hierarchy in the LRM package.  This path should have no URL encoding.</param>
        /// 
        /// <returns><c>true</c> if the file exists; otherwise <c>false</c> if the file does not exist, if the 
        /// file is a directory, or if the <paramref name="filePath"/> is <c>null</c> or <c>String.Empty</c>.</returns>
        /// 
        /// <exception cref="ArgumentException"><paramref name="filePath"/> contains invalid characters.</exception>
        /// <exception cref="InvalidOperationException">The <Typ>LrmPackageReader</Typ> is in an error state.</exception>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// root of the LRM package.
        /// <para>
        /// The LRM package will be exploded into a temporary directory when this method is called,
        /// if not done previously.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override bool FileExists(string filePath)
        {
            if (m_state == LrmPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("LrmPackageReader");
            }
            bool retVal = FsPackageReader.FileExists(filePath);
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Copies the unbundled files from the cache to the location specified.
        /// </summary>
        /// <param name="path">The location to copy the unbundled files.  See remarks.</param>
        /// <remarks>The specified location, <paramref name="path"/> must
        /// be a directory that does not exist.  The path to the directory must exist.  E.g. if the path
        /// "c:\manifests\unbundled packages\new package" is desired, the "new package" directory must not exist,
        /// but the "c:\manifests\unbundled packages" directory must exist.
        /// </remarks>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for writing files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Write</Prp>.
        /// </permission>
        /// <exception cref="InvalidOperationException">The <paramref name="path"/> already exists, or the reader
        /// is in an error state.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        internal void CopyTo(string path)
        {
            if (m_state == LrmPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("LrmPackageReader");
            }
            // If the path already exists, throw exception
            DirectoryInfo newDir;
            using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
            {
                newDir = new DirectoryInfo(path);
                if (newDir.Exists)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.PackageDirectoryAlreadyExists, path));
                }
            }
            ExplodeLrmIfNeeded();
            ZipPackageReader.RecursiveCopy(m_unbundlePath, newDir, m_impersonationBehavior);
            GC.KeepAlive(this);
        }

        /// <summary>
        /// Gets the collection of file paths in the package.
        /// </summary>
        /// <returns>Collection of file paths in the package.</returns>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// <exception cref="InvalidOperationException">The <Typ>LrmPackageReader</Typ> is in an error state.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            ReadOnlyCollection<string> retVal = FsPackageReader.GetFilePaths();
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")] // the resources are files on the disk
        public override void TransmitFile(string filePath, HttpResponse response)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            Utilities.ValidateParameterNonNull("response", response);

            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            FsPackageReader.TransmitFile(filePath, response);
            GC.KeepAlive(this);
        }
    }


    /// <summary>
    /// Provides read-only access to the files contained within a single package that is stored in a zipped file
    /// on the file system.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    /// The zipped package can be stored on the local hard drive, removable storage, or on a network file share accessible
    /// via a mapped drive letter or UNC path.  However, note that performance may be very poor if the file name is a
    /// UNC path.
    /// </para>
    /// <para>
    /// This class can be used to import a package contained in a zipped file, or for instance
    /// to access the contents of a zipped package.
    /// </para>
    /// <para>
    /// The zipped package is unzipped to a temporary directory on the first call to <Mth>FileExists</Mth> or
    /// <Mth>GetFileStream</Mth>.  This temporary directory is maintained for the life of the <c>ZipPackageReader</c>,
    /// and is deleted in the <Mth>Dispose</Mth> or finalizer.
    /// </para>
    /// <para>
    /// See <Typ>PackageReader</Typ> for examples of how classes based on <Typ>PackageReader</Typ> (such as this one)
    /// can be used.
    /// </para>
    /// <para>
    /// The <Typ>ZipPackageReader</Typ> class calls unmanaged code.  Since this assembly is used in the GAC, it has unmanaged code permissions
    /// implicitly.
    /// </para>
    /// <para>
    /// The <Typ>FileSystemPackageReader</Typ> and <Typ>ZipPackageReader</Typ> classes both require <Typ>FileIOPermission</Typ> to
    /// write to the file system.  Again, this is implicit as this assembly is a "full trust" assembly.
    /// </para>
    /// </remarks>
    public class ZipPackageReader : PackageReader
    {
        /// <summary>
        /// The zip file containing the package.
        /// </summary>
        private FileInfo m_zip;

        /// <summary>
        /// True if Dispose() should delete the file referenced by m_zip.
        /// </summary>
        bool m_mustDeleteFile;

        /// <summary>
        /// A stream containing the package.
        /// </summary>
        private Stream m_stream;

        /// <summary>
        /// The path into which the files in <paramref name="m_zip"/> should be exploded.
        /// </summary>
        private DirectoryInfo m_unzipPath;

        /// <summary>
        /// The identity which should be impersonated when reading / writing files in from the 
        /// original zip file into the temporary directory containing the unzipped files.
        /// </summary>
        private ImpersonationBehavior m_impersonationBehavior = ImpersonationBehavior.UseImpersonatedIdentity;

        /// <summary>
        /// States this reader can be in.
        /// </summary>
        private enum ZipPackageReaderState
        {
            /// <summary>
            /// Initial state.  Zip file has not been exploded.
            /// </summary>
            Init,
            /// <summary>
            /// Zip file has been exploded successfully.
            /// </summary>
            Exploded,
            /// <summary>
            /// After Dispose() is called.
            /// </summary>
            Disposed
        };

        /// <summary>
        /// Current reader state.
        /// </summary>
        private ZipPackageReaderState m_state = ZipPackageReaderState.Init;

        /// <summary>
        /// Initializes a new instance of the <c>ZipPackageReader</c> class with the specified zipped package.
        /// Uses the current user's credentials to access the package files.
        /// </summary>
        /// 
        /// <param name="filePath">The path to the zipped file containing the package, e.g. "C:\Foo\Bar.zip".</param>
        /// 
        /// <remarks>
        /// <para>
        /// This constructor does not check if the file exists. This constructor is a placeholder 
        /// for a string that is used to access the file in subsequent operations.
        /// </para>
        /// <para>
        /// The <paramref name="filePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        public ZipPackageReader(string filePath)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);

            m_zip = new FileInfo(filePath);
        }

        /// <summary>
        /// Initializes a new instance of the <c>ZipPackageReader</c> class with the specified zipped package. Uses the 
        /// specified user's credentials to access package files.
        /// </summary>
        /// 
        /// <param name="filePath">The path to the zipped file containing the package, e.g. "C:\Foo\Bar.zip".</param>
        /// <param name="impersonationBehavior">The identity to use when accessing package files. </param>
        /// 
        /// <remarks>
        /// <para>
        /// This constructor does not check if the file exists. This constructor is a placeholder 
        /// for a string that is used to access the file in subsequent operations.
        /// </para>
        /// <para>
        /// The <paramref name="filePath"/> parameter can be a directory on a network file share accessible
        /// via a mapped drive letter or UNC path.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is a null reference.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> contains a colon (:) in the middle of the string.</exception>
        public ZipPackageReader(string filePath, ImpersonationBehavior impersonationBehavior)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);

            m_zip = new FileInfo(filePath);
            m_impersonationBehavior = impersonationBehavior;
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="filePath">The path to the zipped file containing the package, e.g. "C:\Foo\Bar.zip".</param>
        /// <param name="disposeFile"><c>true</c> to delete <paramref name="filePath"/> on <c>Dispose()</c>.</param>
        internal ZipPackageReader(FileInfo filePath, bool disposeFile)
        {
            m_zip = filePath;
            m_mustDeleteFile = disposeFile;
        }

        /// <summary>
        /// Initializes a new instance of the <c>ZipPackageReader</c> class with the specified stream.
        /// </summary>
        /// <param name="stream">Stream containing a valid zipped package.</param>
        /// <remarks>
        /// <para>
        /// This constructor does not check if the stream contains a valid zipped package. This constructor is a placeholder 
        /// for a stream that is used to access the data in subsequent operations.
        /// </para>
        /// </remarks>
        public ZipPackageReader(Stream stream)
        {
            Utilities.ValidateParameterNonNull("stream", stream);
            
            m_stream = stream;
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage
        /// collector reclaims the <c>ZipPackageReader</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// The garbage collector calls this when the current object is ready to be finalized.
        /// </remarks>
        /// 
        ~ZipPackageReader()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by this object
        /// </summary>
        /// <param name="disposing">True if this method was called from
        ///    <Mth>/System.IDisposable.Dispose</Mth></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (m_fsPackageReader != null)
                {
                    m_fsPackageReader.Dispose();
                    m_fsPackageReader = null;
                }

                if (m_state == ZipPackageReaderState.Disposed)
                {
                    return;
                }
                // If the current state is "exploded", delete the directory, files, and all
                // subdirectories and files.  Note that the only way the current state becomes
                // "exploded" is with a successful unzip operation.
                if (m_state == ZipPackageReaderState.Exploded)
                {
                    if (m_unzipPath != null)
                    {
                        try
                        {
                            m_unzipPath.Delete(true);
                        }
                        catch (IOException)
                        {
                            // file can't be deleted if currently open.  Unfortunately this means there will be
                            // files left behind.
                            // noop
                        }
                    }
                    // delete the created file if needed.
                    if (m_mustDeleteFile && m_zip != null)
                    {
                        m_zip.Delete();
                    }
                }
            }
            finally
            {
                m_state = ZipPackageReaderState.Disposed;
                base.Dispose(disposing);
            }
        }

        // Underlying package reader used once the stream has been decompressed to the file system.
        private FileSystemPackageReader m_fsPackageReader;
        private FileSystemPackageReader FsPackageReader
        {
            get
            {
                if (m_fsPackageReader == null)
                {
                    m_fsPackageReader = new FileSystemPackageReader(ExplodeZipIfNeeded(), m_impersonationBehavior);
                }
                return m_fsPackageReader;
            }
        }

        /// <summary>
        /// Explodes the m_zip file into a newly created (randomly named, in the temporary directory)
		/// destination directory if this hasn't been done already.
        /// </summary>
        /// <remarks>
        /// <c>m_state</c> affects what this method does, and can be changed by this method.
        /// <c>m_unzipPath</c> can be changed by this method.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">State of the reader is "Closed".</exception>
        /// <exception cref="InvalidPackageException">There is a problem with the zip package.</exception>
        /// <returns>Package root path.</returns>
        private string ExplodeZipIfNeeded()
        {
            if (m_state == ZipPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("ZipPackageReader");
            }
            if (m_state != ZipPackageReaderState.Exploded)
            {
                // If there is an m_stream but no m_zip, we still need to stream the stream into a temp file.
                if (m_stream != null && m_zip == null)
                {
                    using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                    {
                        m_zip = new FileInfo(Path.GetTempFileName());
                        using (FileStream fileStream = m_zip.Create())
                        {
                            // Read from the stream and write to the exploded location using the specified identity.
                            Utilities.CopyStream(m_stream, m_impersonationBehavior, fileStream, m_impersonationBehavior);
                        }
                    }
                    m_mustDeleteFile = true;
                }

                using (ImpersonateIdentity id = new ImpersonateIdentity(m_impersonationBehavior))
                {
                    // Note that Path.Combine is safe here since Path.GetTempPath() and Path.GetRandomFileName()
                    // are known to be safe.
                    // Just in case Path.GetRandomFileName() returns a filename that already exists, keep trying
                    // until success.
                    bool done = false;
                    while (!done)
                    {
                        m_unzipPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                        if (m_unzipPath.Exists) continue;
                        m_unzipPath.Create();
                        done = true;
                    }
                    // explode the zip file
                    try
                    {
                        Compression.Unzip(m_zip, m_unzipPath);
                        m_state = ZipPackageReaderState.Exploded;
                    }
                    catch (Exception e)
                    {
                        // on Exception, convert into InvalidPackageException.

                        // wrap this message into a descriptive message
                        string message = "";
                        message = String.Format(CultureInfo.InvariantCulture, ValidatorResources.PackageCouldNotBeOpened, message);
                        throw new InvalidPackageException(message, e);
                    }
                }
            }
            return m_unzipPath.ToString();
        }

        /// <summary>
        /// Returns a <Typ>/System.IO.Stream</Typ> for the file inside the zipped package referenced by 
        /// <paramref name="filePath"/>.
        /// </summary>
        /// 
        /// <param name="filePath">The file to retrieve.  The path is relative to the root of the file
        /// hierarchy in the zipped package.  If this path indicates a file that is not in the zipped
        /// package, a <Typ>/System.IO.FileNotFoundException</Typ> is thrown.</param>
        /// 
        /// <returns><Typ>/System.IO.Stream</Typ> for the specified file.</returns>
        /// 
        /// <exception cref="IOException">The file is already open.</exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> contains invalid characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The <Typ>ZipPackageReader</Typ> is in an error state.</exception>
        /// <exception cref="FileNotFoundException">The file specified was not found.</exception>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for creating directories and
        /// reading and writing files.  Associated enumerations: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp> and
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Write</Prp>.
        /// </permission>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// root of the zipped package.
        /// <para>
        /// The zipped package will be exploded into a temporary directory when this method is called,
        /// if not done previously.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override Stream GetFileStream(string filePath)
        {
            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            Stream retVal = FsPackageReader.GetFileStream(filePath);
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Gets a value indicating whether a file exists in the zipped package. 
        /// </summary>
        /// 
        /// <param name="filePath">The file to check.  The path is relative to the root of the file
        /// hierarchy in the zipped package.  This path should have no URL encoding.</param>
        /// 
        /// <returns><c>true</c> if the file exists; otherwise <c>false</c> if the file does not exist or if the 
        /// file is a directory.</returns>
        /// 
        /// <exception cref="ArgumentException"><paramref name="filePath"/> contains invalid characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The <Typ>ZipPackageReader</Typ> is in an error state.</exception>
        /// 
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// 
        /// <remarks><paramref name="filePath"/> is a relative path to the
        /// root of the zipped package.
        /// <para>
        /// The zipped package will be exploded into a temporary directory when this method is called,
        /// if not done previously.
        /// </para>
        /// </remarks>
        /// 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override bool FileExists(string filePath)
        {
            if (m_state == ZipPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("ZipPackageReader");
            }
            bool retVal = FsPackageReader.FileExists(filePath);
      
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Recursively copy all package files from one directory to another.
        /// </summary>
        /// <param name="from">Directory to copy from.</param>
        /// <param name="to">Directory to copy to.</param>
        /// <param name="impersonationBehavior">The identity that can read the files in the <paramref name="from"/> folder.
        /// If this identity can also write to the <paramref name="to"/> folder, the method performs faster
        /// than if it does not have write permissions.</param>
        internal static void RecursiveCopy(DirectoryInfo from, DirectoryInfo to, ImpersonationBehavior impersonationBehavior)
        {
            bool copySucceeded = false;
            try
            {
                using (ImpersonateIdentity id = new ImpersonateIdentity(impersonationBehavior))
                {
                    try
                    {
                        to.Create();
                    }
                    catch (IOException ex)
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                            Resources.PackageDirectoryAlreadyExists, to.FullName), ex);
                    }

                    RecursiveCopy(from, to);
                    copySucceeded = true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // This means the identity probably did not have write privileges to the 'to' folder.
                // So, try continue and try loading files into memory before writing them to the output folder.
            }

            if (!copySucceeded)
            {
                // Load files into memory before writing them to the output
                RecursiveCopyStreams(from, impersonationBehavior, to);
            }
        }

        /// <summary>
        /// Recursively copy from one directory to another. This method does not impersonate.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private static void RecursiveCopy(DirectoryInfo from, DirectoryInfo to)
        {
            FileInfo[] files = from.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(to.FullName, file.Name));
            }
            foreach (DirectoryInfo sub in from.GetDirectories())
            {
                DirectoryInfo newSub = Directory.CreateDirectory(Path.Combine(to.FullName, sub.Name));
                RecursiveCopy(sub, newSub);
            }
        }

        /// <summary>
        /// Recursively copy from one folder to another by reading the files into memory. 
        /// </summary>
        /// <param name="fromDir">The folder to copy from.</param>
        /// <param name="fromImpersonationBehavior">The identity that has rights to read the <paramref name="fromDir"/>.</param>
        /// <param name="toDir">The folder to write to. Writing is not done in an impersonation block.</param>
        private static void RecursiveCopyStreams(DirectoryInfo fromDir, ImpersonationBehavior fromImpersonationBehavior, DirectoryInfo toDir)
        {
            FileInfo[] files;

            using (ImpersonateIdentity id = new ImpersonateIdentity(fromImpersonationBehavior))
            {
                files = fromDir.GetFiles();
            }
            foreach (FileInfo file in files)
            {
                using(Disposer disposer = new Disposer())
                {
                    FileStream fromStream;
                    using (ImpersonateIdentity id = new ImpersonateIdentity(fromImpersonationBehavior))
                    {
                         fromStream = file.OpenRead();
                         disposer.Push(fromStream);
                    }
                    string toPath = PackageReader.SafePathCombine(toDir.FullName, file.Name);
                    FileInfo toFileInfo = new FileInfo(toPath);
                    FileStream toStream = toFileInfo.OpenWrite();
                    disposer.Push(toStream);

                    Utilities.CopyStream(fromStream, fromImpersonationBehavior, toStream, ImpersonationBehavior.UseImpersonatedIdentity);
                }
            }
            foreach (DirectoryInfo sub in fromDir.GetDirectories())
            {
                DirectoryInfo newSub = Directory.CreateDirectory(Path.Combine(toDir.FullName, sub.Name));
                RecursiveCopy(sub, newSub);
            }
        }

        /// <summary>
        /// Copies the unzipped files from the cache to the location specified.
        /// </summary>
        /// <param name="path">The location to copy the unzipped files.  See remarks.</param>
        /// <remarks>The specified location, <paramref name="path"/> must
        /// be a directory that does not exist.  The path to the directory must exist.  E.g. if the path
        /// "c:\manifests\unzipped packages\new package" is desired, the "new package" directory must not exist,
        /// but the "c:\manifests\unzipped packages" directory must exist. The current user must have sufficient 
        /// permissions to copy the package files into this directory.
        /// </remarks>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for writing files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Write</Prp>.
        /// </permission>
        /// <exception cref="InvalidOperationException">The <paramref name="path"/> already exists, or the reader
        /// is in an error state.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        internal void CopyTo(string path)
        {
            if (m_state == ZipPackageReaderState.Disposed)
            {
                throw new ObjectDisposedException("ZipPackageReader");
            }
            
            // No impersonation here. The current user must be able to read/write the target path.
            DirectoryInfo newDir = new DirectoryInfo(path);
            if (newDir.Exists)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.PackageDirectoryAlreadyExists, path));
            }

            ExplodeZipIfNeeded();
            RecursiveCopy(m_unzipPath, newDir, m_impersonationBehavior);
            GC.KeepAlive(this);
        }

        /// <summary>
        /// Gets the collection of file paths in the package.
        /// </summary>
        /// <returns>Collection of file paths in the package.</returns>
        /// <permission cref="System.Security.Permissions.FileIOPermission"> for reading files.
        /// Associated enumeration: 
        /// <Prp>System.Security.Permissions.FileIOPermissionAccess.Read</Prp>.
        /// </permission>
        /// <exception cref="InvalidOperationException">The <Typ>ZipPackageReader</Typ> is in an error state.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // the resources are files on the disk
        public override ReadOnlyCollection<string> GetFilePaths()
        {
            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            ReadOnlyCollection<string> retVal = FsPackageReader.GetFilePaths();
            GC.KeepAlive(this);
            return retVal;
        }

        /// <summary>
        /// Writes a file directly to a web page response. This method should be used whenever possible, as it has 
        /// much better performance than reading a file into a stream and copying it to the response.
        /// </summary>
        /// <param name="filePath">The package-relative path to the file.</param>
        /// <param name="response"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]   // required to maintain cached files on disk
        public override void TransmitFile(string filePath, HttpResponse response)
        {
            Utilities.ValidateParameterNonNull("filePath", filePath);
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            Utilities.ValidateParameterNonNull("response", response);

            // Throws an InvalidPackageException if the package is invalid. This check is done in the FileSystemPackageReader
            FsPackageReader.TransmitFile(filePath, response);
            GC.KeepAlive(this);
        }
    }
    #endregion
}
