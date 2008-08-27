using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties
{
    [Serializable()]
    public class clsUILayouts : BLListBase<clsUILayouts, clsUILayout, clsUILayout.BOGUIDDataKey>
    {
        public static clsUILayouts GetCollection(Type ObjectType)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsUILayout));
            Criteria.AddBinaryExpression("ObjectType_ply", "ObjectType", "=", ObjectType.FullName, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return clsUILayouts.GetCollection(Criteria);
        }
    }
}

