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
        string tempFolder;
        string zippedFileName;
        string zippedFilePath;

#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentDownloader"/>.</summary>
        /// <param name="assignmentProperties">The assignment properties</param>
        public AssignmentDownloader(AssignmentProperties assignmentProperties)
        {
            /* Building the assignment name with the following format : "AssignmentTitle 
               AssignmentCreationDate" (This is the naming format defined in 
               AssignmentProperties.aspx.cs page) */

            string assignmentFolderName = string.Format("{0} {1}{2}{3}", assignmentProperties.Title,
                                    assignmentProperties.DateCreated.Month.ToString(),
                                    assignmentProperties.DateCreated.Day.ToString(),
                                    assignmentProperties.DateCreated.Year.ToString());

            // Create temporary folder
            tempFolder = Path.Combine(Path.GetTempPath(), "SLK_Temp" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            // Create assignment folder
            string assignmentFolderPath = Path.Combine(tempFolder, assignmentFolderName);
            Directory.CreateDirectory(assignmentFolderPath);

            // TODO: Potential clash here if 2 people download at same time
            zippedFilePath = Path.Combine(Path.GetTempPath(), assignmentFolderName + ".zip");

            ExtractAssignmentsFromSharePoint(assignmentProperties, assignmentFolderName, assignmentFolderPath);

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
        /// <summary>
        /// Creates the assignment sub folders (students' folders)
        /// </summary>
        /// <param name="assignmentFolder">Assignment folder located in the document library</param>
        /// <param name="finalAssignmentFolderPath">Path of the assignment folder to 
        /// create the subfolders in</param>
        void CreateSubFolders(SPFolder assignmentFolder, string assignmentFolderPath)
        {
            foreach (SPFolder subFolder in assignmentFolder.SubFolders)
            {
                string subFolderPath = Path.Combine(assignmentFolderPath, subFolder.Name);
                Directory.CreateDirectory(subFolderPath);

                foreach (SPFile file in subFolder.Files)
                {
                    if ((bool)file.Item["IsLatest"])
                    {
                        object nameValue = file.Item["Name"];
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
        }

        void ExtractAssignmentsFromSharePoint(AssignmentProperties assignmentProperties, string assignmentFolderName, string assignmentFolderPath)
        {
            using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
            {
                using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                {
                    SPList list = web.Lists[AppResources.DropBoxDocLibName];
                    CreateSubFolders(FindAssignmentFolder(list, assignmentFolderName), assignmentFolderPath);
                }
            }

        }

        SPFolder FindAssignmentFolder(SPList list, string assignmentFolderName)
        {
            ///Searching for the assignment folder by name                         
            SPQuery query = new SPQuery();
            query.Folder = list.RootFolder;
            query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + assignmentFolderName + "</Value></Eq></Where>";
            SPListItemCollection assignmentFolders = list.GetItems(query);

            if (assignmentFolders.Count == 0)
            {
                throw new Exception(AppResources.SubmittedFilesNoAssignmentFolderException);
            }
            else
            {
                 return assignmentFolders[0].Folder;
            }
        }

#endregion private methods
    }
}
