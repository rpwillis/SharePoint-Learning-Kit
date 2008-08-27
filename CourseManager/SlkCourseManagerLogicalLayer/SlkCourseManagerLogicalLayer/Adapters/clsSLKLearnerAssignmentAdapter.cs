using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Adapter for SLK LearnerAssignments with the Axelerate Framework
    /// </summary>
    public class clsSLKLearnerAssignmentAdapter : clsAdapterBase 
    {

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public clsSLKLearnerAssignmentAdapter()
            : base()
        {
        }

        #endregion

        #region ApdaterBase
        /// <summary>
        /// Gets the Method's Name
        /// </summary>
        /// <param name="MethodType"></param>
        /// <returns></returns>
        public override string GetMethodName(clsAdapterBase.AdapterMethodType MethodType)
        {
            switch (MethodType)
            {
                case AdapterMethodType.AdapterFactory:
                    return "GetLearnerAssignments";
                case AdapterMethodType.AdapterUpdate:
                    return "UpdateLearnerAssignment";                                                  
            }
            return base.GetMethodName(MethodType);
        }
        
        /// <summary>
        /// Get's the Adapter's Type
        /// </summary>
        public override clsAdapterBase.AdapterCapabilities AdapterType
        {
            get
            {
                return clsAdapterBase.AdapterCapabilities.AdapterUpdatable;
            }
        }

        /// <summary>
        /// Transforms Data
        /// </summary>
        /// <param name="FieldIndex"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public override object DataTransform(int FieldIndex, System.Data.DataRow Row)
        {
            switch (FieldIndex)
            {
                case 0:
                    //GradedPoints
                    return Row[3].ToString();
                case 1:
                    //Final Points
                    return Row[2].ToString();
                case 2:
                    //Instructor Comments
                    return Row[4].ToString();
                case 3:
                    //Learner GUID
                    return Row[5].ToString();
                case 4:
                    //Points Possible
                    return Row[6].ToString();
                case 5:
                    //Status
                    return Row[7].ToString();
                case 6:
                    //Assignment GUID
                    return Row[1].ToString();
                case 7:
                    //GUID 
                    return Row[0].ToString();
            }
            return null;

        }
        #endregion
           
        #region GetLearnerAssigments
        /// <summary>
        /// Get a List of the Assignment GUIDs in a SLK Store
        /// </summary>
        /// <param name="slkStore">SLK Store</param>
        /// <returns>DataRowCollection with the GUID List</returns>
        private DataRowCollection GetAssignmentGUIDList(SlkStore slkStore) 
        {
            try
            {
                if (!slkStore.IsInstructor(SPContext.Current.Web))
                {
                    throw new Exception("Instructor permissions required.");
                }

                using (new LearningStorePrivilegedScope())
                {
                    LearningStore learningStore = slkStore.LearningStore;
                    LearningStoreQuery query = learningStore.CreateQuery(Schema.AssignmentItem.ItemTypeName);
                    LearningStoreJob job = learningStore.CreateJob();
                    query.AddColumn(Schema.AssignmentItem.Id);

                    query.AddCondition(Schema.AssignmentItem.SPWebGuid, LearningStoreConditionOperator.Equal, SPContext.Current.Web.ID.ToString());

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
        /// Transforms Data from SLK to valid data in CM.
        /// </summary>
        /// <param name="assignmentGUID">Assignment's GUID</param>
        /// <param name="assignmentProperties">Assignment's Properties</param>
        /// <param name="gradingProperties">Grading Properties</param>
        /// <param name="learnerAssignments">Learner's Assignments</param>
        /// <returns></returns>
        private dtsLearnerAssignments.tblLearnerAssignmentsRow TransformSLKData(string assignmentGUID, AssignmentProperties assignmentProperties, GradingProperties gradingProperties, dtsLearnerAssignments learnerAssignments)
        {
            try
            {
                dtsLearnerAssignments.tblLearnerAssignmentsRow row = learnerAssignments.tblLearnerAssignments.NewtblLearnerAssignmentsRow();

                row.GUID = gradingProperties.LearnerAssignmentId.GetKey().ToString();
                row.AssignmentGUID = assignmentGUID;
                if (gradingProperties.FinalPoints != null)
                {
                    row.FinalPoints = (double)gradingProperties.FinalPoints;
                }
                else
                {
                    row.FinalPoints = -1;
                }
                if (gradingProperties.GradedPoints != null)
                {
                    row.GradedPoints = (double)gradingProperties.GradedPoints;
                }
                else
                {
                    row.GradedPoints = 0;
                }
                row.InstructorComments = gradingProperties.InstructorComments;
                row.LearnerGUID = gradingProperties.LearnerId.GetKey().ToString();

                if (assignmentProperties.PointsPossible != null)
                {
                    row.PointsPossible = (double)assignmentProperties.PointsPossible;
                }
                else
                {
                    row.PointsPossible = 0;
                }

                /*   GP Status   |   FinalPoints |  Return
                 * --------------------------------------------
                 *   NotStarted  |      Any      |    1  (Pending Submit)
                 *   Active      |      Any      |    1  (Pending Submit)      
                 *   Completed   |      Null     |    2  (Sitting on inbox)
                 *   Completed   |      Set      |    3  (Marked and ready to return)
                 *   Final       |      Any      |    4  (Returned to student)
                 */

                switch (gradingProperties.Status.Value.ToString())
                {
                    case "NotStarted":
                        row.Status = "1";
                        break;
                    case "Active":
                        row.Status = "1";
                        break;
                    case "Completed":
                        if (gradingProperties.FinalPoints == null)
                        {
                            row.Status = "2";
                        }
                        else
                        {
                            row.Status = "3";
                        }                       
                        break;
                    case "Final":
                        row.Status = "4";
                        break;
                    default:
                        row.Status = "1";
                        break;
                }
                return row;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Gets all  LearnerAssignments on a Course.
        /// </summary> 
        public dtsLearnerAssignments GetLearnerAssignments() 
        {
            try
            {   
                dtsLearnerAssignments learnerAssignments = new dtsLearnerAssignments();
                                
                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);

                //Gets a list of all assignments on the current Web. 
                DataRowCollection DataRows = GetAssignmentGUIDList(slkStore);

                foreach (DataRow dataRow in DataRows)
                {
                    try
                    {
                        LearningStoreItemIdentifier iId = (LearningStoreItemIdentifier)dataRow[Schema.AssignmentItem.Id];
                        string assignmentGUID = iId.GetKey().ToString();
                        AssignmentItemIdentifier aID = new AssignmentItemIdentifier(long.Parse(assignmentGUID));
                        AssignmentProperties assignmentProperties;

                        ReadOnlyCollection<GradingProperties> gradingPropertiesCollection = slkStore.GetGradingProperties(aID, out assignmentProperties);

                        foreach (GradingProperties gp in gradingPropertiesCollection)
                        {
                            dtsLearnerAssignments.tblLearnerAssignmentsRow row = TransformSLKData(assignmentGUID, assignmentProperties, gp, learnerAssignments);
                            learnerAssignments.tblLearnerAssignments.AddtblLearnerAssignmentsRow(row);
                        }
                    }
                    catch (SafeToDisplayException SDE)
                    {
                        //If user doesn't have permission dont show and keep going. 
                    }                    
                }
                return learnerAssignments;
            }            
            catch(Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetLearnerAssignmentDataError);
            }
        }

        /// <summary>
        /// Gets a list of LearnerAssignments for a particular User.    
        /// </summary>
        /// <param name="LearnerGUID">User Identifier</param>    
        public dtsLearnerAssignments GetLearnerAssignments(string LearnerGUID)
        {
            try
            {
                dtsLearnerAssignments learnerAssignments = new dtsLearnerAssignments();

                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);

                //Gets a list of all assignments on the current Web. 
                DataRowCollection DataRows = GetAssignmentGUIDList(slkStore);

                foreach (DataRow dataRow in DataRows)
                {
                    try
                    {
                        LearningStoreItemIdentifier iId = (LearningStoreItemIdentifier)dataRow[Schema.AssignmentItem.Id];
                        string assignmentGUID = iId.GetKey().ToString();
                        AssignmentItemIdentifier aID = new AssignmentItemIdentifier(long.Parse(assignmentGUID));
                        AssignmentProperties assignmentProperties;

                        ReadOnlyCollection<GradingProperties> gradingPropertiesCollection = slkStore.GetGradingProperties(aID, out assignmentProperties);

                        foreach (GradingProperties gp in gradingPropertiesCollection)
                        {
                            if (gp.LearnerId.GetKey().ToString().CompareTo(LearnerGUID) == 0)
                            {
                                dtsLearnerAssignments.tblLearnerAssignmentsRow row = TransformSLKData(assignmentGUID, assignmentProperties, gp, learnerAssignments);
                                learnerAssignments.tblLearnerAssignments.AddtblLearnerAssignmentsRow(row);
                            }
                        }
                    }
                    catch (SafeToDisplayException SDE)
                    {
                        //If user doesn't have permission dont show and keep going. 
                    }
                }
                return learnerAssignments;
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetLearnerAssignmentDataError);
            }
        }

        #endregion

        #region UpdateLearnerAssignment
        /// <summary>
        /// Updates the Grade of a Learner Assignment
        /// </summary>
        /// <param name="FinalPoints">Grade</param>
        /// <param name="LearnerAssignmentGUID">Learner Assignment's GUID</param>
        public void UpdateLearnerAssignment(double FinalPoints, string LearnerAssignmentGUID) 
        {          
            try
            {   
                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);
                Guid laGUID = slkStore.GetLearnerAssignmentGuidId(new LearningStoreItemIdentifier(Schema.LearnerAssignmentItem.ItemTypeName, long.Parse(LearnerAssignmentGUID)));
                LearnerAssignmentProperties laProperties = slkStore.GetLearnerAssignmentProperties(laGUID,SlkRole.Instructor);
                if (laProperties.Status.CompareTo(LearnerAssignmentState.NotStarted) == 0 || laProperties.Status.CompareTo(LearnerAssignmentState.Active) == 0)
                {
                    slkStore.ChangeLearnerAssignmentState(laGUID, LearnerAssignmentState.Completed);
                }
                if (FinalPoints == -1)
                {
                    slkStore.SetFinalPoints(laGUID, null);
                }
                else 
                {
                    slkStore.SetFinalPoints(laGUID, (float)FinalPoints);
                }
            }
            catch (SafeToDisplayException SDE)
            {
                throw new NotAnInstructorException(Resources.ErrorMessages.UserNotInstructorError);
            }
            catch (Exception e)
            {                
                throw new Exception(Resources.ErrorMessages.UpdateLearnerAssignmentError + " " + LearnerAssignmentGUID);
            }
        }
                
        #endregion

        #region ReturnToLearner

        /// <summary>
        /// Returns an the Assignment to a learner.
        /// </summary>
        /// <param name="LearnerAssignmentGUID">LearnerAssignment Identifier</param>      
        public void ReturnToLearner(string LearnerAssignmentGUID)
        {
            try
            {
                SlkStore slkStore = SlkStore.GetStore(SPContext.Current.Web);
                Guid laGUID = slkStore.GetLearnerAssignmentGuidId(new LearningStoreItemIdentifier(Schema.LearnerAssignmentItem.ItemTypeName, long.Parse(LearnerAssignmentGUID)));
                LearnerAssignmentProperties laProperties = slkStore.GetLearnerAssignmentProperties(laGUID, SlkRole.Instructor);
                slkStore.ChangeLearnerAssignmentState(laGUID, LearnerAssignmentState.Final);               
            }
            catch (SafeToDisplayException SDE)
            {
                throw new NotAnInstructorException(Resources.ErrorMessages.UserNotInstructorError);
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.ReturnLearnerAssignmentError);
            }
        }

        #endregion
    }
}
