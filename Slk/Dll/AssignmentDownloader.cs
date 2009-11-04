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
    public class AssignmentDownloader
    {
        /// <summary>
        /// Downloads the assignment as zipped file
        /// </summary>
        /// <param name="assignmentProperties">The assignment properties</param>
        public void DownloadAssignment(
                            AssignmentProperties assignmentProperties, 
                            out string finalTempFolderPath,
                            out string zippedFileName,
                            out string tempZippedFilePath)
        {
            using (SPSite site = new SPSite(assignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
            {
                using (SPWeb web = site.OpenWeb(assignmentProperties.SPWebGuid))
                {
                    SPList list = web.Lists[AppResources.DropBoxDocLibName];

                    /* Building the assignment name with the following format : "AssignmentTitle 
                       AssignmentCreationDate" (This is the naming format defined in 
                       AssignmentProperties.aspx.cs page) */

                    StringBuilder assignmentFolderName = new StringBuilder();
                    assignmentFolderName.AppendFormat("{0} {1}{2}{3}", assignmentProperties.Title,
                                            assignmentProperties.DateCreated.Month.ToString(),
                                            assignmentProperties.DateCreated.Day.ToString(),
                                            assignmentProperties.DateCreated.Year.ToString());

                    ///Searching for the assignment folder by name                         
                    SPQuery query = new SPQuery();
                    query.Folder = list.RootFolder;
                    query.Query = "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>" + assignmentFolderName.ToString() + "</Value></Eq></Where>";
                    SPListItemCollection assignmentFolders = list.GetItems(query);

                    if (assignmentFolders.Count == 0)
                    {
                        throw new Exception(AppResources.SubmittedFilesNoAssignmentFolderException);
                    }

                    string systemTempPath = Path.GetTempPath();

                    if (!systemTempPath.EndsWith("\\"))
                    {
                        systemTempPath += "\\";
                    }

                    CreateTempFolder(systemTempPath, out finalTempFolderPath);

                    string assignmentFolderPath;
                    CreateAssignmentFolder(finalTempFolderPath, assignmentFolderName.ToString(),
                                            out assignmentFolderPath);

                    CreateSubFolders(assignmentFolders[0].Folder, assignmentFolderPath);

                    StringBuilder zippedFilePath = new StringBuilder();
                    zippedFilePath.AppendFormat("{0}{1}{2}", systemTempPath,
                                                assignmentFolderName.ToString(), ".zip");

                    FastZip zipper = new FastZip();
                    zipper.CreateZip(zippedFilePath.ToString(), finalTempFolderPath, true, null);

                    zippedFileName = assignmentFolderName.ToString();
                    tempZippedFilePath = zippedFilePath.ToString();

                }
            }
        }

        /// <summary>
        /// Deletes the temporary folder and the temporary zipped file
        /// </summary>
        /// <param name="finalTempFolderPath">Path of the temp folder containing the assignment</param>
        /// <param name="zippedFilePath">Path of the temp zipped file</param>
        public void DeleteTemp(string finalTempFolderPath, string zippedFilePath)
        {
            ///Delete the temp folder
            Directory.Delete(finalTempFolderPath, true);

            ///Delete the zipped file from the temp location
            File.Delete(zippedFilePath);
        }

        /// <summary>
        /// Creates a temp folder to hold the assignment files while keeping their correct
        /// folder structure as in the document library
        /// </summary>
        /// <param name="systemTempPath">Path of the current system's temporary folder</param>
        /// <param name="finalTempFolderPath">Final path to the temp folder created</param>
        private void CreateTempFolder(string systemTempPath, out string finalTempFolderPath)
        {
            StringBuilder tempFolderPath = new StringBuilder();
            tempFolderPath.AppendFormat("{0}{1}", systemTempPath, "SLK_Temp");

            if (Directory.Exists(tempFolderPath.ToString()))
            {
                int folderNameSuffix = 1;

                finalTempFolderPath = tempFolderPath.ToString() + folderNameSuffix.ToString();

                while (Directory.Exists(finalTempFolderPath))
                {
                    folderNameSuffix++;
                    finalTempFolderPath = tempFolderPath.ToString() + folderNameSuffix.ToString();
                }

                Directory.CreateDirectory(finalTempFolderPath);
            }
            else
            {
                finalTempFolderPath = tempFolderPath.ToString();
                Directory.CreateDirectory(finalTempFolderPath);
            }
        }

        /// <summary>
        /// Creates the assignment folder
        /// </summary>
        /// <param name="finalTempFolderPath">Path to the temp folder to create the assignment
        /// folder in</param>
        /// <param name="assignmentFolderName">Name of the assignment folder</param>
        /// <param name="assignmentFolderPath">Path to the assignment folder</param>
        private void CreateAssignmentFolder(string finalTempFolderPath, string assignmentFolderName,
                                            out string assignmentFolderPath)
        {
            StringBuilder pathToAssignmentFolder = new StringBuilder();
            pathToAssignmentFolder.AppendFormat("{0}{1}{2}", finalTempFolderPath,
                                                @"\", assignmentFolderName);

            assignmentFolderPath = pathToAssignmentFolder.ToString();

            Directory.CreateDirectory(assignmentFolderPath);
        }

        /// <summary>
        /// Creates the assignment sub folders (students' folders)
        /// </summary>
        /// <param name="assignmentFolder">Assignment folder located in the document library</param>
        /// <param name="finalAssignmentFolderPath">Path of the assignment folder to 
        /// create the subfolders in</param>
        private void CreateSubFolders(SPFolder assignmentFolder, string assignmentFolderPath)
        {
            foreach (SPFolder subFolder in assignmentFolder.SubFolders)
            {
                StringBuilder subFolderPath = new StringBuilder();
                subFolderPath.AppendFormat("{0}{1}{2}", assignmentFolderPath, @"\", subFolder.Name);

                Directory.CreateDirectory(subFolderPath.ToString());

                foreach (SPFile file in subFolder.Files)
                {
                    if ((bool)file.Item["IsLatest"] == true)
                    {
                        StringBuilder filePath = new StringBuilder();
                        filePath.AppendFormat("{0}{1}{2}", subFolderPath.ToString(),
                                                    @"\", file.Item["Name"].ToString());

                        byte[] data = file.OpenBinary();
                        FileStream fileStream = new FileStream(
                                                    filePath.ToString(),
                                                    FileMode.CreateNew);
                        BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                        binaryWriter.Write(data, 0, (int)file.Length);
                        binaryWriter.Close();
                        fileStream.Close();
                    }
                }
            }
        }
    }
}
