using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a collection of Assignments on SLK Storage.
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsSLKAssignments : BLListBase<clsSLKAssignments, clsSLKAssignment, clsSLKAssignment.BOGUIDDataKey>
    {
        #region "Factory Methods"
        /// <summary>
        /// Returns the SLK Assignments that have been created using the Course Manager
        /// </summary>
        /// <returns></returns>
        public static clsSLKAssignments GetSLKAssignmentsInCM() 
        {
            try
            {
                bool search = false;
                int expCounter = 1;
                BLCriteria criteria = new BLCriteria(typeof(clsActivityGroups));
                clsActivityGroups groups = clsActivityGroups.GetCollectionByCourse(criteria);

                BLCriteria AssignmentCriteria = new BLCriteria(typeof(clsSLKAssignments));

                foreach (clsActivityGroup ag in groups)
                {
                    criteria = new BLCriteria(typeof(clsActivities));
                    clsActivities activitiesOnCM = clsActivities.GetCollectionByActivityGroupGUID(ag.GUID);
                    foreach (clsActivity act in activitiesOnCM)
                    {
                        search = true;
                        AssignmentCriteria.AddBinaryExpression("GUID_ass:" + expCounter, "SLKGUID", "=", act.SLKGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorOr);
                        expCounter++;
                    }
                }
                if (search)
                {
                    return clsSLKAssignments.GetCollection(AssignmentCriteria);
                }
                else
                {
                    return new clsSLKAssignments();
                }
            }
            catch (Exception e)
            {
                throw e; //throw error to next level.
            }
        }
        /// <summary>
        /// Returns the Assignments in SLK that were not created using Course Manager.
        /// </summary>
        /// <returns></returns>
        public static clsSLKAssignments GetSLKAssignmentsNotInCM()
        {
            try
            {
                int expCounter = 1;
                BLCriteria criteria = new BLCriteria(typeof(clsActivityGroups));
                clsActivityGroups groups = clsActivityGroups.GetCollectionByCourse(criteria);

                BLCriteria AssignmentCriteria = new BLCriteria(typeof(clsSLKAssignments));

                foreach (clsActivityGroup ag in groups)
                {
                    criteria = new BLCriteria(typeof(clsActivities));
                    clsActivities activitiesOnCM = clsActivities.GetCollectionByActivityGroupGUID(ag.GUID);
                    foreach (clsActivity act in activitiesOnCM)
                    {
                        if (act.ActivityStatusGUID.CompareTo(clsActivityStatus.UnassignedStatus.GUID) != 0)
                        {
                            AssignmentCriteria.AddBinaryExpression("GUID_ass:" + expCounter, "SLKGUID", "<>", act.SLKGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                            expCounter++;
                        }
                    }
                }
                return clsSLKAssignments.GetCollection(AssignmentCriteria);
            }
            catch (Exception e)
            {
                throw e; //throw error to next level.
            }
        }
        /// <summary>
        /// Returns all the Assignments that are gradeable and have a MaxPoints/PointsPossible greater than 0.
        /// </summary>
        /// <returns></returns>
        public static clsSLKAssignments GetSLKAssignmentsForMA()
        {
            try
            {
                int expCounter = 1;
                BLCriteria criteria = new BLCriteria(typeof(clsActivityGroups));
                clsActivityGroups groups = clsActivityGroups.GetCollectionByCourse(criteria);

                BLCriteria AssignmentCriteria = new BLCriteria(typeof(clsSLKAssignments));

                foreach (clsActivityGroup ag in groups)
                {
                    criteria = new BLCriteria(typeof(clsActivities));
                    clsActivities activitiesOnCM = clsActivities.GetCollectionByActivityGroupGUID(ag.GUID);
                    foreach (clsActivity act in activitiesOnCM)
                    {
                        if (!act.Gradeable) 
                        {
                            AssignmentCriteria.AddBinaryExpression("GUID_ass:" + expCounter, "SLKGUID", "<>", act.SLKGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                            
                            expCounter++;
                        }
                    }
                }
                AssignmentCriteria.AddBinaryExpression("PointsPossible_ass", "PointsPossible", ">", 0, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
                clsSLKAssignments assignments = clsSLKAssignments.GetCollection(AssignmentCriteria);

                
                    
                return OrderSLKAssignments(assignments);
            }
            catch (Exception e)
            {
                throw e; //throw error to next level.
            }
        }

        /// <summary>
        /// Sort the Slk Assignments by Due Date.
        /// </summary>
        /// <param name="InputList"></param>
        /// <returns></returns>
        private static clsSLKAssignments OrderSLKAssignments(clsSLKAssignments InputList) 
        {
            List<clsSLKAssignment> list = new List<clsSLKAssignment>();
            list.AddRange(InputList);// add(assignment);
            list.Sort(delegate(clsSLKAssignment a1, clsSLKAssignment a2) { return a1.DueDate.CompareTo(a2.DueDate); });

            clsSLKAssignments OutpuList = new clsSLKAssignments();

            foreach (clsSLKAssignment assignment in list)
            {
                OutpuList.Add(assignment);
            }
            return OutpuList;
        }

        #endregion

        #region "ExtendedFilters"
        #endregion
    }
}