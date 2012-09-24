using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint;
using Ionic.Zip;

namespace Microsoft.LearningComponents.SharePoint
{
    /// <summary>The cache in a SharePoint library.</summary>
    class SharePointLibraryCache : IDisposable
    {
        SPSite cacheSite;
        SPWeb cacheWeb;
        SPList cacheList;
        SPFolder cacheFolder;
        Dictionary<string, SPFolder> folders = new Dictionary<string, SPFolder>();

#region constructors
        public SharePointLibraryCache(SPWeb web, SPFile file, SharePointFileLocation packageLocation, SharePointCacheSettings cacheSettings)
        {
            Uri baseWebUri = new Uri(web.Url);
            Uri cacheUri = new Uri(baseWebUri, cacheSettings.CachePath);
            cacheSite = new SPSite(cacheUri.ToString());
            cacheWeb = cacheSite.OpenWeb();
            cacheWeb.AllowUnsafeUpdates = true;

            try
            {
                cacheList = cacheWeb.GetList(cacheSettings.CachePath);
            }
            catch (ArgumentException)
            {
                throw new CacheException(string.Format(CultureInfo.CurrentUICulture, Resources.InvalidLibraryCache, cacheSettings.CachePath));
            }

            int packageId = FindOrCreatePackageId(packageLocation);

            CacheFolderUrl = string.Concat(cacheWeb.Url, "/", cacheList.RootFolder.Url, "/", packageId.ToString(CultureInfo.InvariantCulture));
            cacheFolder = cacheWeb.GetFolder(CacheFolderUrl);
            if (cacheFolder.Exists == false)
            {
                cacheFolder = CreateCacheFolder(packageId.ToString(CultureInfo.InvariantCulture));
                UnzipAndCachePackage(file);
            }
        }

#endregion constructors

#region properties
        /// <summary>The url of the cache folder.</summary>
        public string CacheFolderUrl { get; private set; }
#endregion properties

#region public methods
        public void Dispose()
        {
            if (cacheWeb != null)
            {
                cacheWeb.Dispose();
            }

            if (cacheSite != null)
            {
                cacheSite.Dispose();
            }
        }

        /// <summary>Checks whether a given file exists in the package.</summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file exists.</returns>
        public bool FileExists(string filePath)
        {
            SPFile file = FindFile(filePath);
            return file.Exists;
        }

        /// <summary>Gets a file from the package.</summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>The contents of the file.</returns>
        public Stream GetFileStream(string filePath)
        {
            SPFile file = FindFile(filePath);
            MemoryStream stream = new MemoryStream(file.OpenBinary());
            return stream;
        }

        /// <summary>Returns the paths of all files.</summary>
        /// <returns>A collection of all files.</returns>
        public ReadOnlyCollection<string> GetFilePaths()
        {
            List<string> files = new List<string>();

            foreach (SPFile file in cacheFolder.Files)
            {
                files.Add(file.Name);
            }

            foreach (SPFolder folder in cacheFolder.SubFolders)
            {
                ListFolder(folder, string.Empty, files);
            }

            return new ReadOnlyCollection<string>(files);
        }
        
        /// <summary>Transmits a file.</summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="response">The response to write to.</param>
        public void TransmitFile(string filePath, System.Web.HttpResponse response)
        {
            SPFile file = FindFile(filePath);
            Debug("TransmitFile {0}", file.Exists);
            byte[] contents = file.OpenBinary();
            response.OutputStream.Write(contents, 0, contents.Length);
            response.Flush();
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
        int FindOrCreatePackageId(SharePointFileLocation packageLocation)
        {
            SPList packagesList = cacheWeb.Lists["Packages"];
            string location = packageLocation.ToString();

            SPQuery query = new SPQuery();
            query.Query = String.Concat(@"<Where>
                                                <Eq>
                                                    <FieldRef Name='Title'/>
                                                    <Value Type='Text'>", location, @"</Value>
                                                </Eq>
                                            </Where>");
            SPListItemCollection items = packagesList.GetItems(query);

            if (items.Count == 0)
            {
                SPListItem item = packagesList.Items.Add();
                item["Location"] = location;
                item.Update();
                return item.ID;
            }
            else
            {
                return items[0].ID;
            }
        }

        void ListFolder(SPFolder folder, string root, List<string> files)
        {
            string folderPath = string.Concat(root, folder.Name, "\\");

            foreach (SPFile file in folder.Files)
            {
                files.Add(string.Concat(folderPath, file.Name));
            }

            foreach (SPFolder subFolder in folder.SubFolders)
            {
                ListFolder(subFolder, folderPath, files);
            }
        }

        SPFile FindFile(string filePath)
        {
            Utilities.ValidateParameterNotEmpty("filePath", filePath);
            string url = string.Concat(CacheFolderUrl, "/", filePath);
            Debug("FindFile {0}", url);
            return cacheWeb.GetFile(url);
        }

        public static void Debug(string message, params object[] arguments)
        {
            using (StreamWriter writer = new StreamWriter("c:\\temp\\slkdebug.txt", true))
            {
                writer.WriteLine(message, arguments);
            }
        }


        void UnzipAndCachePackage(SPFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            Stream fileContents = file.OpenBinaryStream();
            fileContents = new MemoryStream(file.OpenBinary());
            using (ZipFile zip = ZipFile.Read(fileContents))
            {
                foreach (ZipEntry entry in zip)
                {
                    if (entry.IsDirectory)
                    {
                        FindFolder(entry.FileName, true);
                    }
                    else
                    {
                        SPFolder folder = FindFolder(Path.GetDirectoryName(entry.FileName), true);
                        SaveFile(folder, Path.GetFileName(entry.FileName), entry);
                    }
                }
            }
        }

        SPFolder CreateCacheFolder(string name)
        {
            return CreateFolder(cacheList.RootFolder, name);
        }

        SPFolder CreateFolder(SPFolder folder, string name)
        {
            SPFolder newFolder = null;
            try
            {
                newFolder = folder.SubFolders[name];
            }
            catch (ArgumentException)
            {
            }

            if (newFolder == null)
            {
                newFolder = folder.SubFolders.Add(name);
                newFolder.Update();
                folder.Update();
            }

            return newFolder;
        }

        SPFolder FindFolder(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return cacheFolder;
            }
            else
            {
                if (folders.ContainsKey(directory))
                {
                    return folders[directory];
                }

                string parent = Path.GetDirectoryName(directory);
                SPFolder folder;

                if (string.IsNullOrEmpty(parent))
                {
                    folder = FindFolder(cacheFolder, directory);
                }
                else
                {
                    SPFolder parentFolder = FindFolder(parent);
                    folder = FindFolder(parentFolder, Path.GetFileName(directory));
                }

                folders.Add(directory, folder);
                return folder;
            }
        }

        SPFolder FindFolder(SPFolder folder, string name)
        {
            return folder.SubFolders[name];
        }

        SPFolder FindFolder(string directory, bool forCreate)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return cacheFolder;
            }
            else
            {
                if (folders.ContainsKey(directory))
                {
                    return folders[directory];
                }

                string parent = Path.GetDirectoryName(directory);

                SPFolder parentFolder;
                if (string.IsNullOrEmpty(parent))
                {
                    parentFolder = cacheFolder;
                }
                else
                {
                    parentFolder = FindFolder(parent, forCreate);
                }

                SPFolder folder;
                if (forCreate)
                {
                    folder = CreateFolder(parentFolder, Path.GetFileName(directory));
                }
                else
                {
                    folder = FindFolder(parentFolder, Path.GetFileName(directory));
                }

                folders.Add(directory, folder);
                return folder;
            }
        }

        void SaveFile(SPFolder folder, string fileName, ZipEntry entry)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                entry.Extract(stream);
                stream.Flush();
                SPFile file = folder.Files.Add(fileName, stream, true);
                folder.Update();
            }
        }
#endregion private methods

#region static members
#endregion static members
    }
}

