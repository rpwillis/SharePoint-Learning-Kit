using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    /// <summary>
    /// This class is inherited from the MNGUIDDetailsCollectionTemplate
    /// </summary>
    [Serializable()]
    public class clsRoles : MNGUIDDetailsCollectionTemplate<clsRoles, clsADUser_Role, clsADUser, clsBaseRole>
    {

        #region "Shared Properties and Methods"

        /// <summary>
        /// Returns a collection containing the role with the Role Name
        /// </summary>
        /// <param name="Name">Role's Name</param>
        /// <returns>Returns a collection containing the role with the Name.  An empty collection if the user is not found </returns>
        [staFactory()]
        public static clsRoles GetCollectionByName(string Name)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsRoles));
            Criteria.AddBinaryExpression("Name_rol", "Name", "=", Name, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }

        #endregion
    }
}
