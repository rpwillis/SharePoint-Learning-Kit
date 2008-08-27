using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{

    /// <summary>
    /// Adapter for SLK Assigments with the Axelerate Framework
    /// </summary>
    public class clsSLKAssignmentAdapter : clsAdapterBase 
    {
        #region ApdaterBase
        /// <summary>
        /// Returns the Method Name
        /// </summary>
        /// <param name="MethodType"></param>
        /// <returns></returns>
        public override string GetMethodName(clsAdapterBase.AdapterMethodType MethodType)
        {
            switch (MethodType)
            {
                case AdapterMethodType.AdapterFactory:
                    return "GetAssignments";
                case AdapterMethodType.AdapterUpdate:
                    return "UpdateAssignment";
                case AdapterMethodType.AdapterInsert:
                    return "CreateAssigment";
                case AdapterMethodType.AdapaterDelete:
                    return "DeleteAssigment";                
            }
            return base.GetMethodName(MethodType);
        }
        
        /// <summary>
        /// Gets the Adapter's Type
        /// </summary>
        public override clsAdapterBase.AdapterCapabilities AdapterType
        {
            get
            {
                return clsAdapterBase.AdapterCapabilities.AdapterAddDelete;
            }
        }

        /// <summary>
        /// Transform Data from a Data Row.
        /// </summary>
        /// <param name="FieldIndex"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public override object DataTransform(int FieldIndex, System.Data.DataRow Row)
        {
            switch (FieldIndex)
            {
                case 0:
                    //Description
                    return Row[2];
                case 1:
                    //StartDate
                    return Row[3];
                case 2:
                    //DueDate
                    return Row[4];
                case 3:
                    //Points Possible
                    return Row[5];
                case 4:
                    //Course GUID
                    return Row[6];
                case 5:
                    //Package Location
                    return Row[7];
                case 6:
                    //Name 
                    return Row[1];
                case 7:
                    //GUID 
                    return Row[0];
            }
            return null;

        }

        #endregion
              
        #region GetAssignments
        /// <summary>
        /// Return all the SLK Assignments Data.
        /// </summary>
        /// <param name="slkStore">Slk Store</param>
        /// <returns>Data as a DataRowCollection</returns>
        private DataRowCollection GetAssignmentsData(SlkStore slkStore)
        {
            try
            {
                if (!slkStore.IsInstructor(SPContext.Current.Web))
                {
                    throw new Exception("Instructor permissions required for this operation.");
                }

                using (new LearningStorePrivilegedScope())
                {
                    LearningStore learningStore = slkStore.LearningStore;
                    LearningStoreQuery query = learningStore.CreateQuery(Schema.AssignmentItem.ItemTypeName);
                    LearningStoreJob job = learningStore.CreateJob();
                    query.AddColumn(Schema.AssignmentItem.Id);
                    query.AddColumn(Schema.AssignmentItem.Title);
                    query.AddColumn(Schema.AssignmentItem.Description);
                    query.AddColumn(Schema.AssignmentItem.StartDate);
                    query.AddColumn(Schema.AssignmentItem.DueDate);
                    query.AddColumn(Schema.AssignmentItem.PointsPossible);
                    query.AddColumn(Schema.AssignmentItem.SPWebGuid);
                    query.AddColumn(Schema.AssignmentItem.NonELearningLocation);
                    query.AddColumn(Schema.AssignmentItem.RootActivityId);
                    
                    query.AddCondition(Schema.AssignmentItem.SPWebGuid, LearningStoreConditionOperator.Equal, SPContext.Current.Web.ID.ToString());

                    query.AddSort(Schema.AssignmentItem.DueDate, LearningStoreSortDirection.Ascending); 

                    job.PerformQuery(query);
                    return job.Execute<DataTable>().Rows;
                }
            }
            catch (Exception e)
            {
                throw e; //Throw exception to next level. 
            }
        }
        
        /// <summary>
        /// Transforms Data loaded from SLK
        /// </summary>
        /// <param name="dataRow">DataRow</param>
        /// <param name="assignments">Dataset with assignments</param>
        /// <param name="slkStore">SLK Store</param>
        /// <returns></returns>
        private dtsAssignments.tblAssignmentsRow TransformSLKData(DataRow dataRow, dtsAssignments assignments,SlkStore slkStore)
        {
            try
            {
                dtsAssignments.tblAssignmentsRow row = assignments.tblAssignments.NewtblAssignmentsRow();

                LearningStoreItemIdentifier iId = (LearningStoreItemIdentifier)dataRow[Schema.AssignmentItem.Id];
                row.GUID = iId.GetKey().ToString();
                row.Name = (string)dataRow[Schema.AssignmentItem.Title];
                row.Description = (string)dataRow[Schema.AssignmentItem.Description];
                row.StartDate = ((DateTime)dataRow[Schema.AssignmentItem.StartDate]).ToLocalTime();
                object dueDate = dataRow[Schema.AssignmentItem.DueDate];
                if(!typeof(System.DBNull).IsInstanceOfType(dueDate))                
                {
                    row.DueDate = ((DateTime)dueDate).ToLocalTime();
                }                
                else 
                {
                    row.DueDate = new DateTime(1973,1,1);
                }
                 
                object pp = dataRow[Schema.AssignmentItem.PointsPossible];
                if (!typeof(System.DBNull).IsInstanceOfType(pp))     
                {
                    float ppf = (float)pp;
                    row.PointsPossible = (double)ppf;
                }
                else 
                {
                    row.PointsPossible = 0;
                }
                Guid webGuid = (Guid)dataRow[Schema.AssignmentItem.SPWebGuid];                            
                row.CourseGUID = webGuid.ToString();
                object pl = dataRow[Schema.AssignmentItem.NonELearningLocation];
                if (!typeof(System.DBNull).IsInstanceOfType(pl))
                {
                    row.PackageLocation = (string)pl;
                }
                else
                {
                    /*
                    ActivityPackageItemIdentifier pii = (ActivityPackageItemIdentifier)dataRow[Schema.AssignmentItem.RootActivityId];
                    slkStore.ge
                    PackageReader reader = slkStore.PackageStore.GetPackageReader();
                    row.PackageLocation = reader.GetFilePaths()[0];
                    */
                    row.PackageLocation = "E-Learning Content";
                }

                return row;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// Gets a collection of all the Assigments for the current course (site). 
        /// </summary>
        public dtsAssignments GetAssignments()
        {     
            try
            {
                dtsAssignments assignments = new dtsAssignments();

                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);

                DataRowCollection dataRows = GetAssignmentsData(slkStore);

                string assignmentGUID = "";

                foreach (DataRow dataRow in dataRows)
                {
                    try
                    {
                        LearningStoreItemIdentifier iId = (LearningStoreItemIdentifier)dataRow[Schema.AssignmentItem.Id];
                        assignmentGUID = iId.GetKey().ToString();
                        AssignmentItemIdentifier aID = new AssignmentItemIdentifier(long.Parse(assignmentGUID));
                        AssignmentProperties ap = slkStore.GetAssignmentProperties(aID, SlkRole.Instructor);
                        assignments.tblAssignments.AddtblAssignmentsRow(TransformSLKData(dataRow, assignments,slkStore));
                    }
                    catch (SafeToDisplayException SDE)
                    {
                        //If user doesn't have permission dont show and keep going. 
                    }   
                }

                return assignments;
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetAssignmentDataError);
            }            
        }

        /// <summary>
        /// Gets an especific Assignment Data. 
        /// </summary>
        public dtsAssignments GetAssignments(string AssignmentGUID)
        {
            try
            {
                dtsAssignments assignments = new dtsAssignments();

                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);

                DataRowCollection dataRows = GetAssignmentsData(slkStore);

                string assignmentGUID = "";

                foreach (DataRow dataRow in dataRows)
                {
                    try
                    {
                        LearningStoreItemIdentifier iId = (LearningStoreItemIdentifier)dataRow[Schema.AssignmentItem.Id];
                        assignmentGUID = iId.GetKey().ToString();
                        if (assignmentGUID.CompareTo(AssignmentGUID) == 0)
                        {
                            AssignmentItemIdentifier aID = new AssignmentItemIdentifier(long.Parse(assignmentGUID));
                            AssignmentProperties ap = slkStore.GetAssignmentProperties(aID, SlkRole.Instructor);
                            assignments.tblAssignments.AddtblAssignmentsRow(TransformSLKData(dataRow, assignments,slkStore));
                        }
                    }
                    catch (SafeToDisplayException SDE)
                    {
                        //If user doesn't have permission dont show and keep going. 
                    }
                }

                return assignments;
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetAssignmentDataError);
            }
        }

        #endregion
               
        #region CreateAssigment

        /// <summary>
        /// Creates a new Assingment in SLK
        /// </summary>
        /// <param name="packageUrl">URL for the associated package.</param>
        /// <param name="assignmentTitle">New assignment's title</param>
        /// <param name="dueDate">New assignment Due Date</param>
        /// <param name="pointsPossible">New assignment possible points</param>
        /// <param name="description">New assignment description</param>
        public string CreateAssigment(string description, DateTime startDateS, DateTime dueDateS, double pointsPossible, string packageUrl, string assignmentTitle, string AssignmentGUID)
        {
            DateTime startDate = startDateS;
            DateTime dueDate = dueDateS;

            AssignmentItemIdentifier assignmentId = null;

            try
            {
                SPSite spSite = SPContext.Current.Site;
                SPUserToken instructorToken;

                instructorToken = SPContext.Current.Web.CurrentUser.UserToken;

                using (SPSite packageSite = new SPSite(packageUrl, instructorToken))
                {
                    using (SPWeb packageWeb = packageSite.OpenWeb())
                    {
                        SPFile spFile = packageWeb.GetFile(packageUrl);
                        SharePointFileLocation spFileLocation = new SharePointFileLocation(packageWeb, spFile.UniqueId, spFile.UIVersion);
                        string packageLocationString = spFileLocation.ToString();
                        SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);

                        // SharePointCacheSettings spcs = new SharePointCacheSettings();

                        SharePointPackageReader spreader = new SharePointPackageReader(slkStore.SharePointCacheSettings, spFileLocation);
                        bool IsNonElearning = PackageValidator.Validate(spreader).HasErrors;

                        int? organizationIndex = 0;

                        if (IsNonElearning)
                        {
                            organizationIndex = null;
                        }

                        LearningStoreXml packageWarnings;
                        AssignmentProperties assignmentProperties = slkStore.GetNewAssignmentDefaultProperties(SPContext.Current.Web, packageLocationString, organizationIndex, SlkRole.Instructor, out packageWarnings);

                        // set the assignment title
                        assignmentProperties.Title = assignmentTitle;
                        assignmentProperties.DueDate = dueDate;
                        assignmentProperties.StartDate = startDate;
                        assignmentProperties.PointsPossible = (float)pointsPossible;
                        assignmentProperties.Description = description;

                        SlkMemberships memberships = slkStore.GetMemberships(SPContext.Current.Web, null, null);

                        if (memberships.Learners.Count == 0)
                        {
                            throw new NoLearnersOnSiteException(Resources.ErrorMessages.NotLearnersOnSiteError);
                        }

                        assignmentProperties.Learners.Clear();
                        assignmentProperties.Instructors.Clear();
                        foreach (SlkUser learner in memberships.Learners)
                        {
                            if (!assignmentProperties.Learners.Contains(learner))
                            {
                                //todo:
                                try
                                {
                                    assignmentProperties.Learners.Add(learner);
                                }
                                catch { }
                            }
                        }

                        foreach (SlkUser instructor in memberships.Instructors)
                        {
                            if (!assignmentProperties.Instructors.Contains(instructor))
                            {
                                //todo:
                                try
                                {
                                    assignmentProperties.Instructors.Add(instructor);
                                }
                                catch { }
                            }
                        }
                        // create the assignment
                        assignmentId = slkStore.CreateAssignment(SPContext.Current.Web, packageLocationString, organizationIndex, SlkRole.Instructor, assignmentProperties);
                    }
                }
            }
            catch (NoLearnersOnSiteException nls) 
            {
                throw nls;
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new Exception(Resources.ErrorMessages.InvalidUriError);
            }
            catch (UriFormatException ufe)
            {
                throw new Exception(Resources.ErrorMessages.InvalidUriError);
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.CreateAssignmentError);
            }
            return assignmentId.GetKey().ToString();
        }       

        #endregion
                             
        #region DeleteAssigment

        /// <summary>
        /// Deletes an Assigment from SLK
        /// </summary>
        /// <param name="AssigmentGUID">Assigment's identifier in Axelerate Libraries</param>
        public void DeleteAssigment(string AssigmentGUID)
        {
            try
            {
                SPWeb spWeb = SPContext.Current.Web;
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                long key = long.Parse(AssigmentGUID);
                AssignmentItemIdentifier assignmentID = new AssignmentItemIdentifier(key);
                slkStore.DeleteAssignment(assignmentID);
            }
            catch (SafeToDisplayException SDE)
            {
                throw new NotAnInstructorException(Resources.ErrorMessages.UserNotInstructorError); 
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.DeleteAssignmentError + " " + AssigmentGUID, e);
            }           
        }
        
        #endregion               

        #region UpdateAssignment
        /// <summary>
        /// Updates an assignemnt on the SLK
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="StartDate">Start Date</param>
        /// <param name="DueDate">Due Date</param>
        /// <param name="PointsPossible">Points Possible</param>
        /// <param name="PackageLocation">Package Location</param>
        /// <param name="Name">Name</param>
        /// <param name="AssigmentGUID">Assignment's GUID</param>
        public void UpdateAssignment(string description, DateTime StartDate, DateTime DueDate, double PointsPossible, string PackageLocation, string Name, string AssigmentGUID) 
        {
            try
            {
                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);
                long key = long.Parse(AssigmentGUID);
                AssignmentItemIdentifier assignmentID = new AssignmentItemIdentifier(key);
                AssignmentProperties assignmentProperties = slkStore.GetAssignmentProperties(assignmentID, SlkRole.Instructor);
                assignmentProperties.Description = description;
                assignmentProperties.StartDate = StartDate;
                assignmentProperties.DueDate = DueDate;
                assignmentProperties.PointsPossible = (float)PointsPossible;
                assignmentProperties.Title = Name;
                //assignmentProperties.Location = PackageLocation;

                slkStore.SetAssignmentProperties(assignmentID, assignmentProperties);
            }
            catch (SafeToDisplayException SDE)
            {
                throw new NotAnInstructorException(Resources.ErrorMessages.UserNotInstructorError);
            }
            catch (Exception e)
            {                
                throw new Exception(Resources.ErrorMessages.UpdateAssignmentError + " " + AssigmentGUID);
            }
        }

        #endregion
    }
}
