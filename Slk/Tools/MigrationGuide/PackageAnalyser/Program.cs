/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using MigrationHelper;
using System.Xml;
using System.Data;
using Microsoft.LearningComponents;
using System.IO;

namespace PackageAnalyser
{
    class Program
    {
        /// <summary>
        /// Reads config file to get db connection string and a path to process.
        /// Reads manifest file (index.xml, if index.xml not found then imsmanifest.xml) 
        /// for every learning package in the folder and retrieves the package identifier.
        /// If the processing path is a Class Server 4 activity folder path, then
        /// puts the AssignmentID and respective package identifier into AssignmentPackageTransfer table
        /// for every assigment found.
        /// </summary>
        /// <param name="args">ignores parameters</param>
        static void Main(string[] args)
        {
            //starting log
            MigrationHelper.LogFile log = new LogFile("app.log");
            log.WriteToLogFile(AppResources.StartedProcessing + System.Environment.NewLine);
            log.WriteToLogFile(AppResources.GettingDataFromConfig + System.Environment.NewLine);
            //get processing folder from config
            string activityPath = PackageAnalyser.App.Default.Path;
            bool processingAssignments = App.Default.IsSchoolActivityPath;
            log.WriteToLogFile("SchoolActivityPath = " + activityPath + System.Environment.NewLine);
            //check if path exists
            bool activityPathExists = System.IO.Directory.Exists(activityPath);
            if (activityPathExists)
            {
                log.WriteToLogFile(AppResources.ActivityPathValid + System.Environment.NewLine);
            }
            else
            {
                log.WriteToLogFile(AppResources.ActivityPathInvalid + System.Environment.NewLine);
            }
            bool connectionOK = false;
            string dbConnection = String.Empty;
            if (processingAssignments)
            {
                //get database connection from config
                dbConnection = PackageAnalyser.App.Default.SchoolDBConnectionString;
                log.WriteToLogFile("SchoolDBConnectionString = " + dbConnection + System.Environment.NewLine);
                //test connection
                string connectionStatus = "";
                connectionOK = TestDBConnection(dbConnection, out connectionStatus);
                log.WriteToLogFile(AppResources.TestingDBConnection + connectionStatus + System.Environment.NewLine);
            }
            if ((processingAssignments && connectionOK && activityPathExists)||((!processingAssignments)&&(activityPathExists)))
            {
                //if parameters are valid, start processing
                List<learningResource> packagesInfo = new List<learningResource>();
                try
                {
                    GetAssignmentPackagesInfo(activityPath, ref packagesInfo);
                }
                catch (System.Exception ex)
                {
                    log.WriteToLogFile(AppResources.ErrorGatheringAssignments + ex.Message + System.Environment.NewLine);
                }
                if (packagesInfo.Count == 0)
                {
                    //nothing found
                    //no learning packages found, log
                    log.WriteToLogFile(AppResources.NoPackagesFound + Environment.NewLine);
                }
                else
                {
                    //log the list of packages
                    log.WriteToLogFile(AppResources.FoundAssignments + System.Environment.NewLine);
                    for (int packageInfoIndex = 0; packageInfoIndex < packagesInfo.Count; packageInfoIndex++)
                    {
                        string foundPackagesLog = String.Format(AppResources.Path, packagesInfo[packageInfoIndex].path);
                        if (packagesInfo[packageInfoIndex].assignmentId > 0)
                        {
                            foundPackagesLog += String.Format(AppResources.AssignmentId, packagesInfo[packageInfoIndex].assignmentId);
                        }
                        if (packagesInfo[packageInfoIndex].packageIdentifier != Guid.Empty)
                        {
                            foundPackagesLog += String.Format(AppResources.PackageId, packagesInfo[packageInfoIndex].packageIdentifier);
                        }
                        if (packagesInfo[packageInfoIndex].licensed)
                        {
                            foundPackagesLog += String.Format(AppResources.RequiresLicense);
                        }
                        if (packagesInfo[packageInfoIndex].processingError != null)
                        {
                            foundPackagesLog += packagesInfo[packageInfoIndex].processingError;
                        }
                        log.WriteToLogFile(foundPackagesLog + Environment.NewLine);
                    }
                    if (processingAssignments)
                    {
                        //put the list of assignments into database
                        DataTable assignments = new DataTable();
                        assignments.Columns.Add(new DataColumn("AssignmentID", System.Type.GetType("System.Int32")));
                        assignments.Columns.Add(new DataColumn("PackageIdentifier", System.Type.GetType("System.Guid")));
                        for (int packageInfoIndex = 0; packageInfoIndex < packagesInfo.Count; packageInfoIndex++)
                        {
                            if ((packagesInfo[packageInfoIndex].assignmentId > 0) && (packagesInfo[packageInfoIndex].packageIdentifier != Guid.Empty))
                            {
                                DataRow assignment = assignments.NewRow();
                                assignment[0] = packagesInfo[packageInfoIndex].assignmentId;
                                assignment[1] = packagesInfo[packageInfoIndex].packageIdentifier;
                                assignments.Rows.Add(assignment);
                            }
                        }
                        CS4Database database = new CS4Database(dbConnection);
                        try
                        {
                            database.WriteAssignmentsTransferTable(assignments);
                            log.WriteToLogFile(AppResources.SuccessWritingToDB + System.Environment.NewLine);
                        }
                        catch (System.Exception ex)
                        {
                            log.WriteToLogFile(AppResources.AnErrorWhileSaving + ex.Message + Environment.NewLine);
                        }
                    }
                }
            }
            else
            {
                //one or more parameters are not valid
                log.WriteToLogFile(AppResources.ParametersInvalid + System.Environment.NewLine);
            }
         
            log.WriteToLogFile(AppResources.FinishedProcessing + System.Environment.NewLine);
            log.FinishLogging();
        }


        /// <summary>
        /// Recursively traverses activityPath. 
        /// Fills in list structure with packages details.
        /// </summary>
        /// <param name="activityPath">The path to analyse</param>
        /// <param name="learningResources">The list to fill with package details</param>
        private static void GetAssignmentPackagesInfo(string activityPath, ref List<learningResource> learningResources)
        {
            bool learningResourceFound = false;
            bool licensed = false;
            string processingError = String.Empty;
            Guid assignmentIdentifier = System.Guid.Empty;
            //if activity path is a folder then work path = activity path
            //however if activity path is .lrm, .ims or .zip file then work path = path of unzipped package
            string workPath = String.Empty;
            //check if activity path is a learning resource to be unzipped
            if ((activityPath.ToLower().EndsWith(".lrm")) || 
                (activityPath.ToLower().EndsWith(".ims")) || 
                (activityPath.ToLower().EndsWith(".zip")))
            {
                //unzipping package, workpath to assign the unzip path
                try
                {
                    workPath = UnZipPackage(activityPath);
                }
                catch(Exception ex)
                {
                    learningResource newResource = new learningResource();
                    newResource.path = activityPath;
                    newResource.processingError = ex.Message;
                    learningResources.Add(newResource);
                    return;
                }                  
            }
            else
            {
                workPath = activityPath;
            }
            //see if there is index.xml or imsmanifest.xml in the folder
            string[] xmlIndexFiles = System.IO.Directory.GetFiles(workPath, "index.xml");
            if (xmlIndexFiles.Length > 0)
            {                
                learningResourceFound = true;
                //we analyse only one manifest file per folder
                if (ManifestParser.ManifestIncludesLicense(xmlIndexFiles[0]))
                {
                    licensed = true;
                }
                else
                {
                    try
                    {
                        assignmentIdentifier = ManifestParser.ParseIndexXml(xmlIndexFiles[0]);
                    }
                    catch (Exception ex)
                    {
                        processingError = ex.Message;
                    }
                }
            }
            else
            {
                string[] xmlIMSFiles = System.IO.Directory.GetFiles(workPath, "imsmanifest.xml");
                if (xmlIMSFiles.Length > 0)
                {
                    learningResourceFound = true;
                    //we analyse only one manifest file per folder
                    if (ManifestParser.ManifestIncludesLicense(xmlIMSFiles[0]))
                    {
                        licensed = true;
                    }
                    else
                    {
                        try
                        {
                            assignmentIdentifier = ManifestParser.ParseImsManifestXml(xmlIMSFiles[0]);
                        }
                        catch (Exception ex)
                        {
                            processingError = ex.Message;
                        }
                    }
                }
            }
            if (learningResourceFound)
            {
                //parsing AssignmentID from folder name, if possible                
                string[] pathFolders = activityPath.Split(new Char[] { '\\' });
                string curFolderName = pathFolders[pathFolders.Length-1];
                int assignmentId = 0;
                if (curFolderName.IndexOf("_") >= 0)
                {
                    Int32.TryParse(curFolderName.Substring(0, curFolderName.IndexOf("_")), out assignmentId);
                }

                //add information about the resource into the list
                learningResource newResource = new learningResource();
                newResource.assignmentId = assignmentId;
                newResource.path = activityPath;
                newResource.packageIdentifier = assignmentIdentifier;
                newResource.processingError = processingError;
                newResource.licensed = licensed;
                //if there was no errors and not licensed but the identifier is empty
                //notify the user
                if ((assignmentIdentifier == Guid.Empty) && (!licensed) &&(processingError.Length == 0))
                {
                    newResource.processingError = AppResources.NoIdentifier;
                }
                learningResources.Add(newResource);
            }
            //getting the list of packages in this folder and recursing
            string[] allFiles = System.IO.Directory.GetFiles(workPath, "*", System.IO.SearchOption.TopDirectoryOnly);
            for (int fileIndex = 0; fileIndex < allFiles.Length; fileIndex++)
            {
                if ((allFiles[fileIndex].ToLower().EndsWith(".lrm")) ||
                    (allFiles[fileIndex].ToLower().EndsWith(".ims")) ||
                    (allFiles[fileIndex].ToLower().EndsWith(".zip")))
                {
                    GetAssignmentPackagesInfo(allFiles[fileIndex], ref learningResources);
                }
            
            }
            //getting the list of folders in this folder and recursing
            string[] innerDirectories = System.IO.Directory.GetDirectories(workPath, "*", System.IO.SearchOption.TopDirectoryOnly);
            for (int dirIndex = 0; dirIndex < innerDirectories.Length; dirIndex++)
            {
                GetAssignmentPackagesInfo(innerDirectories[dirIndex], ref learningResources);
            }            
        }

        /// <summary>
        /// Uses Microsoft.LearningComponents.Compression to unzip the package 
        /// into a randomly named subfolder in temp folder.
        /// If lrm package is passed then processes an additional exception type 
        /// related to multiple LR.
        /// </summary>
        /// <param name="packagePath">Full path to the package to be unzipped</param>
        /// <returns>The path to unzipped package</returns>
        private static string UnZipPackage(string packagePath)
        {
            DirectoryInfo unZipPath = new DirectoryInfo(packagePath);
            bool done = false;
            while (!done)
            {
                unZipPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                if (unZipPath.Exists) continue;
                unZipPath.Create();
                done = true;
            }
            // explode the zip file
            if (packagePath.ToLower().EndsWith(".lrm"))
            {
                try
                {
                    Compression.Unbundle(packagePath, unZipPath.FullName);
                }
                catch (CompressionException e)
                {
                    string message;
                    if (e.GetErrorCode() == 0x0004400F) // multi-LR
                    {
                        message = AppResources.IsMultipleLR;
                    }
                    else
                    {
                        message = e.Message;
                    }
                    throw new System.Exception(message, e);
                }
            }
            else
            {
                Compression.Unzip(packagePath, unZipPath.FullName);
            }
            return unZipPath.FullName;
        }


        /// <summary>
        /// Contains logic for testing of the connection string
        /// </summary>
        /// <param name="dbConnection">connection string</param>
        /// <param name="status">returns status of the connection attempt</param>
        /// <returns>true if the connection successful</returns>
        private static bool TestDBConnection(string dbConnection, out string status)
        {
            bool result = false;
            CS4Database database = new CS4Database(dbConnection);
            try
            {
                database.TestConnection();
                result = true;
                status = AppResources.ConnectionSuccessful;
            }
            catch (SystemException ex)
            {
                status = AppResources.ConnectionFailed + ex.Message;
            }
            return result;

        }

        /// <summary>
        /// structure to hold details about a learning package
        /// </summary>
        private struct learningResource
        {
            internal string path;
            internal int assignmentId;
            internal Guid packageIdentifier;
            internal bool licensed;
            internal string processingError;
        }
    }
}
