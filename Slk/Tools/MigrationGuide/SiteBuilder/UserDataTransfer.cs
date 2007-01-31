/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using System.ComponentModel;
using MigrationHelper;
using System.Collections;

namespace SiteBuilder
{
    /// <summary>
    /// Retrieves the user assignments and grades from CS4 database and places them into SLK system
    /// </summary>
    class UserDataTransfer
    {
        /// <summary>
        /// Reads Class Server 4 settings xml file, 
        /// class by class, and transfers assignments for every class.
        /// Works from a non-UI thread using BackgroundWorker. Handles all exceptions.
        /// Reports progress.
        /// </summary>
        /// <param name="xmlFilePath">path to Classes.xml</param>
        /// <param name="logFilePath">path to log file</param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <returns>true if there were no exceptions during the transfer</returns>
        public bool MoveUserAssignments(string xmlFilePath, string logFilePath,
            BackgroundWorker worker, DoWorkEventArgs e)
        {
            bool status = false;
            LogFile log = null;
            try
            {
                log = new LogFile(logFilePath);
                //gather info about learning packages available in the document gallery
                SLKSite site = new SLKSite();
                log.WriteToLogFile(TextResources.ReadingPackages + Environment.NewLine);
                worker.ReportProgress(0, TextResources.ReadingPackages);
                Hashtable learningResourcesSLK = site.GetAllLearningResources(SiteBuilder.Default.SLKDocLibraryWeb, SiteBuilder.Default.SLKDocLibraryName);
                //loop through classes and transfer assignments
                ConfigXMLFileReader configReader = new ConfigXMLFileReader(xmlFilePath);
                CS4Class nextClass = configReader.GetNextClass();
                while (nextClass != null)
                {
                    //if class is marked for transfer in the xml file
                    if (nextClass.Transfer)
                    {
                        log.WriteToLogFile(String.Format(TextResources.TransferringAssignmentsForClass, nextClass.ClassWeb) + Environment.NewLine);
                        worker.ReportProgress(0, TextResources.ProcessingClass + nextClass.ClassWeb);
                        string logText = "";
                        MoveAssignments(nextClass, ref logText, learningResourcesSLK);
                        log.WriteToLogFile(logText);
                    }
                    else
                    {
                        //assignments for this class will not be transferred, log
                        log.WriteToLogFile(String.Format(TextResources.NotTransferringAssignmentsClassNotForTransfer,nextClass.ClassWeb) + Environment.NewLine);
                    }
                    nextClass = configReader.GetNextClass();
                }
                configReader.Dispose();
                status = true;
            }
            catch (System.Exception ex)
            {
                worker.ReportProgress(0, TextResources.AnError + ex.Message);
                log.WriteToLogFile(TextResources.AnError + ex.Message + Environment.NewLine);
            }
            finally
            {
                try
                {
                    if (log != null)
                    {
                        log.FinishLogging();
                    }
                }
                catch (System.Exception ex)
                {
                    worker.ReportProgress(0, TextResources.AnError + ex.Message);
                }
            }

            return status;
        }

        /// <summary>
        /// For the class requested gets the assignments data from Class Server 4 database
        /// and transfers assignments and grading points and teacher comments using SLK API
        /// </summary>
        /// <param name="classData">class information from Class Server 4 classes config file</param>
        /// <param name="logText">returns log of operations performed</param>
        /// <param name="learningPackages">information about learning packages available on SLK school site</param>
        private void MoveAssignments(CS4Class classData, ref string logText, Hashtable learningPackages)
        {
            
            SharePointV3 assignmentSite = new SharePointV3();
            string assignmentSiteUrl = assignmentSite.BuildSubSiteUrl(SiteBuilder.Default.SLKSchoolWeb, classData.ClassWeb);
            
            //for the getmemberships operation to succeed the current user has to be an SLK instructor on the web site                
            //looping through all the class users to see if any of them are instructors
            //and trying to open the class web with and instructor's token
            SPWeb assignmentWeb = null;
            SPWeb assignmentWebUnderCurrentUser = assignmentSite.OpenWeb(assignmentSiteUrl);
            SPRoleDefinition instructorsRole = assignmentWebUnderCurrentUser.RoleDefinitions[SiteBuilder.Default.SLKInstructorSharePointRole];
            bool foundInstructor = false;
            foreach (CS4User user in classData.Users)
            {
                if ((user.IsTeacher) && (user.Transfer))
                {
                    try
                    {
                        SPUser spUser = assignmentWebUnderCurrentUser.SiteUsers[user.UserLoginWithDomain];
                        SPUserToken token = spUser.UserToken;                        
                        SPSite site = new SPSite(assignmentSiteUrl, token);
                        assignmentWeb = site.OpenWeb();
                        if (assignmentWeb.AllRolesForCurrentUser.Contains(instructorsRole))
                        {
                            foundInstructor = true;
                            break;
                        }
                    }
                    catch
                    {
                        
                        //doing nothing, will try the next instructor
                    }
                }
            }
            if (!foundInstructor)
            {
                logText += TextResources.AssignmentsTransferErrorNoClassInstructors + Environment.NewLine;
                return;
            }
                  
            //open the Class SLK store
            //note we are using SPWeb opened with an instructor's SPUserToken 
            Microsoft.SharePointLearningKit.SlkStore slkStore = SlkStore.GetStore(assignmentWeb);
            //get all learners and instructors for the class   
            SlkMemberships memberships = slkStore.GetMemberships(assignmentWeb, null, null);
            Dictionary<string, SlkUser> allLearners = new Dictionary<string, SlkUser>(
                StringComparer.OrdinalIgnoreCase);
            foreach (SlkUser learner in memberships.Learners)
                allLearners.Add(learner.SPUser.LoginName, learner);

            Dictionary<string, SlkUser> allInstructors = new Dictionary<string, SlkUser>(
                StringComparer.OrdinalIgnoreCase);
            foreach (SlkUser instructor in memberships.Instructors)
                allInstructors.Add(instructor.SPUser.LoginName, instructor);

            //instructors list will always be the same for all assignments
            //because there is no link between assignments and teachers in CS4
            SlkUserCollection classInstructors = new SlkUserCollection();
            foreach (CS4User user in classData.Users)
            {
                if ((user.IsTeacher)&&(user.Transfer))
                {
                    SlkUser slkUser;
                    if (allInstructors.TryGetValue(user.UserLoginWithDomain, out slkUser))
                    {
                        classInstructors.Add(slkUser);
                    }
                    else
                    {
                        //instructor not found on slk site, log
                        logText += String.Format(TextResources.InstructorNotRegisteredWithSLKSite, user.UserLoginWithDomain, assignmentSiteUrl) + Environment.NewLine;
                    }
                }
            }

            //get assignments for this class from the CS4 data base
            CS4Database database = new CS4Database(SiteBuilder.Default.ClassServerDBConnectionString);
            DataTable assignmentItems;
            DataTable userAssignments;
            int numAssignments = database.GetAssignments(classData.ClassId, out assignmentItems, out userAssignments);
            
            //loop through assignments list
            for (int assignmentIndex = 0; assignmentIndex < numAssignments; assignmentIndex++)
            {                    
                try
                {                    
                    string packageIdent = (assignmentItems.Rows[assignmentIndex]["PackageIdentifier"] != System.DBNull.Value ? assignmentItems.Rows[assignmentIndex]["PackageIdentifier"].ToString() : String.Empty); 
                    int assignmentId = (assignmentItems.Rows[assignmentIndex]["AssignmentID"] != System.DBNull.Value ? System.Convert.ToInt32(assignmentItems.Rows[assignmentIndex]["AssignmentID"]) : 0);                    
                    logText += String.Format(TextResources.TransferringAssignment, assignmentId.ToString()) + Environment.NewLine;

                    //get assignment's package identifier
                    string packageLocation = GetPackageLocation(packageIdent, learningPackages);
                    if (packageLocation.Length == 0)
                    {
                        if (packageIdent.Length == 0)
                        {
                            //log: not importing assignment as the package cannot be identified
                            logText += String.Format(TextResources.CantTransferAssignmentUnknownLearningResource, assignmentId) + Environment.NewLine;
                        }
                        else
                        {
                            //log - assignment cannot be imported as the package is not imported into slk
                            logText += String.Format(TextResources.CantTransferAssignmentNoLearningResource, assignmentId) + Environment.NewLine;
                        }
                        //move on to the next assignment
                        break;
                    }
                    //set assignment properties
                    AssignmentProperties properties = ReadAssignmentPropertiesFromDataRow(assignmentItems.Rows[assignmentIndex]);
                    Hashtable gradingPoints = new Hashtable();
                    Hashtable gradingComments = new Hashtable();

                    //set instructors list
                    foreach (SlkUser classInstructor in classInstructors)
                    {
                        properties.Instructors.Add(classInstructor);
                    }
                    //set learners list
                    for (int userAssignmentIndex = 0; userAssignmentIndex < userAssignments.Rows.Count; userAssignmentIndex++)
                    {
                        DataRow assignmentRow = userAssignments.Rows[userAssignmentIndex];
                        int userAssignmentTableID = (assignmentRow["AssignmentID"] == System.DBNull.Value) ? 0 : System.Convert.ToInt32(assignmentRow["AssignmentID"]);
                        int userId = (assignmentRow["StudentID"] == System.DBNull.Value) ? 0 : System.Convert.ToInt32(assignmentRow["StudentID"]);
                        bool isAssignmentGraded = (assignmentRow["HasTeacherGraded"].ToString().ToLower() == "true" ? true : false);
                        float points = (assignmentRow["Points"] == System.DBNull.Value) ? 0 : System.Convert.ToSingle(assignmentRow["Points"]);
                        string instructorComments = assignmentRow["TeacherComments"].ToString();

                        //to minimize sql queries the UserAssignments table contains all assignments for the class
                        //so we need to check if this row is for the assignment currently being processed                        
                        if (assignmentId == userAssignmentTableID)
                        {
                            //find this user in list of users from classes.xml
                            CS4User user = classData.Users.GetByUserId(userId);
                            if (user != null)
                            {
                                //see if this user is for transfer in classes.xml                           
                                if (user.Transfer)
                                {
                                    //see if this user is a learner member on SLK site
                                    SlkUser slkUser;
                                    if (allLearners.TryGetValue(user.UserLoginWithDomain, out slkUser))
                                    {
                                        properties.Learners.Add(slkUser);
                                        //save grading info for this learner to be used later
                                        if (isAssignmentGraded)
                                        {
                                            gradingPoints.Add(slkUser.UserId, points);
                                            gradingComments.Add(slkUser.UserId, instructorComments);
                                        }
                                    }
                                    else
                                    {
                                        //user not found on slk site, log
                                        logText += String.Format(TextResources.UserNotRegisteredWithSLKSite, user.UserLoginWithDomain, assignmentSiteUrl, assignmentId) + Environment.NewLine;
                                    }
                                }
                                else
                                {
                                    //user assignments will not be transferred as user is marked "not for transfer"
                                    logText += String.Format(TextResources.UserNotForTransfer, user.UserLoginWithDomain) + Environment.NewLine;
                                }
                            }
                            else
                            {
                                //user is not found in xml file, log
                                logText += String.Format(TextResources.UserNotFoundInXMLFile, userId, assignmentSiteUrl, SiteBuilder.Default.ClassStructureXML, assignmentId) + Environment.NewLine;
                            }
                        }

                        //create the assignment
                        AssignmentItemIdentifier assignmentIdSLK = slkStore.CreateAssignment(assignmentWeb, packageLocation, 0, SlkRole.Instructor, properties);
                        //transfer the grading results for the assignments
                        AssignmentProperties basicAssignmentProperties;
                        ReadOnlyCollection<GradingProperties> gradingPropertiesList =
                            slkStore.GetGradingProperties(assignmentIdSLK, out basicAssignmentProperties);
                        for (int learnerIndex = 0; learnerIndex < gradingPropertiesList.Count; learnerIndex++)
                        {
                            // set <gradingProperties> to information about this learner assignment
                            GradingProperties gradingProperties = gradingPropertiesList[learnerIndex];
                            if (gradingPoints.ContainsKey(gradingProperties.LearnerId))
                            {
                                //assignment has been graded, transfer grade and comment to SLK
                                gradingProperties.Status = LearnerAssignmentState.Final;
                                gradingProperties.FinalPoints = (float)gradingPoints[gradingProperties.LearnerId];
                                gradingProperties.InstructorComments = gradingComments[gradingProperties.LearnerId].ToString();
                            }
                            
                        }
                        //this call will not save the grade, but it will set the correct state and
                        //put the teacher's comment.
                        logText += slkStore.SetGradingProperties(assignmentIdSLK, gradingPropertiesList) + Environment.NewLine;
                        //calling the second time to save grades                        
                        for (int learnerIndex = 0; learnerIndex < gradingPropertiesList.Count; learnerIndex++)
                        {
                            gradingPropertiesList[learnerIndex].Status = null;
                        }
                        logText += slkStore.SetGradingProperties(assignmentIdSLK, gradingPropertiesList) + Environment.NewLine;
                        logText += String.Format(TextResources.TransferredAssignment, assignmentId.ToString(), assignmentIdSLK.GetKey().ToString()) + Environment.NewLine;
                    }
                }
                catch (System.Exception ex)
                {
                    //exception when transferring an assignment
                    logText += TextResources.AnError + ex.Message + Environment.NewLine;
                }
            }
        }

        /// <summary>
        /// for the learning resource Id requested looks up the learning reources SharePoint
        /// location
        /// </summary>
        /// <param name="packageIdent">Learning Resource Id</param>
        /// <param name="learningPackages">Hashtable with association 
        /// between Learning Resource Ids and Sharepoint location of these reources</param>
        /// <returns>Package Location or empty string if this Id is not found in the hashtable</returns>
        private string GetPackageLocation(string packageIdent, Hashtable learningPackages)
        {
            string packageLocation = String.Empty;
            if (learningPackages.ContainsKey(packageIdent))
            {
                packageLocation = learningPackages[packageIdent].ToString();
            }
            return packageLocation;
        }

        /// <summary>
        /// Converts assignment properties from Class Server 4 database format into 
        /// AssignmentProperties SLK format
        /// </summary>
        /// <param name="propertiesRow">datarow from Class Servr 4 database</param>
        /// <returns>converted assignment properties</returns>
        private AssignmentProperties ReadAssignmentPropertiesFromDataRow(DataRow propertiesRow)
        {
            AssignmentProperties properties = new AssignmentProperties();
            properties.Title = propertiesRow["Title"].ToString();
            properties.Description = propertiesRow["StudentInstructions"].ToString();
            float maxPoints;
            if (float.TryParse(propertiesRow["MaxPoints"].ToString(),out maxPoints))
            {
                properties.PointsPossible = maxPoints;
            }
            if (!propertiesRow.IsNull("StartDate"))
            {
                properties.StartDate = (DateTime)propertiesRow["StartDate"];
            }
            if (!propertiesRow.IsNull("DueDate"))
            {
                properties.DueDate = (DateTime)propertiesRow["DueDate"];
            }
            properties.AutoReturn = (propertiesRow["IsAutoReturned"].ToString() == "1" ? true : false);
            properties.ShowAnswersToLearners = (propertiesRow["ShowCorrectAnswersToStudents"].ToString() == "1" ? true : false);
            return properties;
        }

    }
}
