using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a collection of Activity Groups
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivityGroups : BLListBase<clsActivityGroups, clsActivityGroup, clsActivityGroup.BOGUIDDataKey>
    {
        //private static clsActivityGroup activityGroup = null;
        /// <summary>
        /// Course GUID
        /// </summary>
        private string courseGUID = "";
        #region "Factory Methods"
        /// <summary>
        /// Gets the Collection of Activity Groups By Course
        /// </summary>
        /// <param name="webURL">Course's Web URL</param>
        /// <returns>Activity Group Collection of that Course</returns>
        public static clsActivityGroups GetCollectionByCourse(string webURL)
        {            
            clsSLKCourseAdapter courseAdapter = new clsSLKCourseAdapter();

            dtsCourses course = courseAdapter.GetCourse(webURL);
            dtsCourses.tblCoursesRow row = (dtsCourses.tblCoursesRow)course.tblCourses.Rows[0];
            string _courseGUID = row.GUID;

            BLCriteria Criteria = new BLCriteria(typeof(clsActivityGroups));

            BLCriteriaExpression Expr = Criteria.Expression.FindExpression("CourseGUID_agr");
            if (Expr == null)
            {
                Criteria.AddBinaryExpression("CourseGUID_agr", "CourseGUID", "=", _courseGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            }
            else
            {
                ((BLCriteriaExpression.BinaryOperatorExpression)Expr).Value = _courseGUID;
            }
            Criteria.AddOrderedField("WeekTo_agr", true);
            clsActivityGroups collection = GetCollection(Criteria);
            //if (collection.Count > 0)
            //{
            //    activityGroup = collection[0];
            //}
            collection.courseGUID = _courseGUID;
            return collection;
        }

        /// <summary>
        /// Gets the Collection of Activity Groups of the current Course following a special Criteria
        /// </summary>
        /// <param name="Criteria"></param>
        /// <returns></returns>
        [staFactory()]
        public static clsActivityGroups GetCollectionByCourse(BLCriteria Criteria)
        {
            bool search = false;

            clsCourse course = clsCourse.GetCourse();            
            BLCriteriaExpression Expr = Criteria.Expression.FindExpression("CourseGUID_agr");
            if (Expr == null)
            {
                Criteria.AddBinaryExpression("CourseGUID_agr", "CourseGUID", "=", course.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            }
            else
            {
                ((BLCriteriaExpression.BinaryOperatorExpression)Expr).Value = course.GUID;
            }
            Criteria.AddOrderedField("WeekTo_agr", true);
            clsActivityGroups collection = GetCollection(Criteria);
            //if (collection.Count > 0)
            //{
            //    activityGroup = collection[0];
            //}
            collection.courseGUID = course.GUID;                 

            foreach (clsActivityGroup grp in collection) 
            {
                foreach(clsActivity act in grp.Activities)
                {
                    act.UpdateFromSLK();
                }                
            }
            collection = collection.Save();
            return collection;
        }
               
        /// <summary>
        /// Creates a new instance of an Activity Group
        /// </summary>
        /// <returns></returns>
        public override BLBusinessBase NewBusinessObject()
        {
            clsActivityGroup newActivityGroup = (clsActivityGroup)base.NewBusinessObject();
            newActivityGroup.ActivityGroupStatus = clsActivityGroupStatus.PendingStartSatatus;
            //newActivityGroup.ActivityGroupStatusGUID = newActivityGroup.ActivityGroupStatus.GUID;

            if (courseGUID != "")
            {
                //newActivityGroup.Course = activityGroup.Course;
                newActivityGroup.CourseGUID = clsSLKCourseAdapter.getCourseGUID();
                newActivityGroup.Priority = this.Count;
            }

            return newActivityGroup;
        }

        /// <summary>
        /// Saves the current instance of the BO
        /// </summary>
        /// <returns></returns>
        public override clsActivityGroups Save()
        {
            clsActivityGroups temp = base.Save();
            if (courseGUID != "")
            {
                clsActivityGroup.ReIndexPriorities(courseGUID);
            }
            return temp;
        }

        #endregion

        #region "ExtendedFilters"
        #endregion
    } 
}
