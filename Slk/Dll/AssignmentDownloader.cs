using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using ICSharpCode.SharpZipLib.Zip;
using System.Configuration;
using Resources.Properties;
using System.IO;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>Encapsulates the creation of the download zip file.</summary>
    public class AssignmentDownloader : IDisposable
    {
        DropBoxManager manager;
        string tempFolder;
        string zippedFileName;
        string zippedFilePath;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentDownloader"/>.</summary>
        /// <param name="assignmentProperties">The assignment properties</param>
        public AssignmentDownloader(AssignmentProperties assignmentProperties)
        {
            manager = new DropBoxManager(assignmentProperties);

            // Create temporary folder
            tempFolder = Path.Combine(Path.Combine(Path.GetTempPath(), "SLK_Temp"), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            // Create assignment folder
            string assignmentFolderName = manager.FolderName;
            string assignmentFolderPath = Path.Combine(tempFolder, assignmentFolderName);
            Directory.CreateDirectory(assignmentFolderPath);

            // TODO: Potential clash here if 2 people download at same time
            zippedFilePath = Path.Combine(tempFolder, assignmentFolderName + ".zip");

            ExtractAssignmentsFromSharePoint(assignmentFolderPath);

            FastZip zipper = new FastZip();
            zipper.CreateZip(zippedFilePath, assignmentFolderPath, true, null);

            zippedFileName = assignmentFolderName;
        }
#endregion constructors

#region properties
        /// <summary>The name of the zipped assignment file.</summary>
        public string ZippedFileName
        {
             get { return zippedFileName ;}
        }

        /// <summary>The path to the zipped assignment file.</summary>
        public string ZippedFilePath
        {
             get { return zippedFilePath ;}
        }
#endregion properties

#region public methods
        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        public void Dispose()
        {
            if (Directory.Exists(tempFolder))
            {
                 Directory.Delete(tempFolder);
            }

            if (File.Exists(zippedFilePath))
            {
                 File.Delete(zippedFilePath);
            }
        }
#endregion public methods

#region private methods
        void ExtractAssignmentsFromSharePoint(string assignmentFolderPath)
        {
            Dictionary<string, List<SPFile>> files = manager.AllFiles();

            foreach (string learner in files.Keys)
            {
                string subFolderPath = Path.Combine(assignmentFolderPath, learner);
                Directory.CreateDirectory(subFolderPath);

                foreach (SPFile file in files[learner])
                {
                    object nameValue = file.Item[SPBuiltInFieldId.FileLeafRef];
                    if (nameValue != null)
                    {
                        string name = nameValue.ToString();
                        if (name.Length != 0)
                        {
                            byte[] data = file.OpenBinary();

                            using (FileStream fileStream = new FileStream(Path.Combine(subFolderPath, name), FileMode.CreateNew))
                            {
                                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                                {
                                    binaryWriter.Write(data, 0, (int)file.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

#endregion private methods
    }
}
