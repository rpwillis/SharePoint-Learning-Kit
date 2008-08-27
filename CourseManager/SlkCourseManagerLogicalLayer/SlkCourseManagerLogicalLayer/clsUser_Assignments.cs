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
    /// This class represents a collection of Types of Storages supported
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsUser_Assignments : BLListBase<clsUser_Assignments, clsUser_Assignment, clsUser_Assignment.BOGUIDDataKey>
    {
        #region "Factory Methods"
        
        /// <summary>
        /// Returns in ascending order the Collection of clsCourse_User_Activities of a User in a Course.
        /// </summary>
        /// <param name="Course_UserGUID">The GUID of the Coures_User association</param>
        /// <returns>Return the Collection of clsCourse_User_Activities of a User in a Course in ascending order</returns>
        [staFactory()]
        public static clsUser_Assignments GetCollectionByLearnerGUID(string LearnerGUID)
        {
            BLCriteria criteria = new BLCriteria(typeof(clsUser_Assignment));
            criteria.AddPreFilter("LearnerGUID", LearnerGUID);
            return clsUser_Assignments.GetCollection(criteria);           
        }                

        /// <summary>
        /// Returns the Collection of clsCourse_User_Activities of all User in a Course.
        /// </summary>
        /// <param name="ActivityGUID">The GUID of the activity association</param>
        /// <returns>/ Returns the Collection of clsCourse_User_Activities of all User in a Course.</returns>
        [staFactory()]
        public static clsUser_Assignments GetCollectionByActivity(string ActivityGUID)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsUser_Assignments));
            Criteria.AddBinaryExpression("AssignementGUID_cua", "AssignementGUID", "=", ActivityGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }
        
        #endregion

        #region "ExtendedFilters"
        #endregion

        #region Overrides
        
        #endregion
    }
}
