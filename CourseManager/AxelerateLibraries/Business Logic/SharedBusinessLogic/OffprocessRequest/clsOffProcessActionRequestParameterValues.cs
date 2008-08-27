using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.OffprocessRequest
{
    [Serializable()]
    public class clsOffProcessActionRequestParameterValues : BLListBase<clsOffProcessActionRequestParameterValues, clsOffProcessActionRequestParameterValue, clsOffProcessActionRequestParameterValue.BOGUIDDataKey>
    {
        #region "Factory Methods"
        [staFactory()]
        public static clsOffProcessActionRequestParameterValues GetCollection(clsOffProcessActionRequestInstance Request, DataLayerContextInfo ContextInfo)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsOffProcessActionRequestParameterValue));
            Criteria.DataLayerContextInfo = ContextInfo;
            Criteria.AddBinaryExpression("MasterGUID_oav", "MasterGUID", "=", Request.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return GetCollection(Criteria);
        }
        #endregion




    }
}