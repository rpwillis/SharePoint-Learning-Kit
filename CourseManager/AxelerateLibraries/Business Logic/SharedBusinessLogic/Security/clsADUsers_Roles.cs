using System;
using System.Collections.Generic;
using System.Text;

using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    [Serializable()]
    public class clsADUsers_Roles : BLListBase<clsADUsers_Roles, clsADUser_Role, clsADUser_Role.BOGUIDDataKey>
    {
        #region "Factory Methods"
        [staFactory()]
        public static clsADUsers_Roles GetCollection(clsADUser ADUser)
        {
            BLCriteria Criteria = new BLCriteria(typeof (clsADUsers_Roles));
            Criteria.AddBinaryExpression("MasterGUID_adr", "MasterGUID", "=", ADUser.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }
        [staFactory()]
        public static clsADUsers_Roles GetCollectionByUserGUID(string GUID)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsADUsers_Roles));
            Criteria.AddBinaryExpression("MasterGUID_adr", "MasterGUID", "=", GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }
        #endregion
    }
}
