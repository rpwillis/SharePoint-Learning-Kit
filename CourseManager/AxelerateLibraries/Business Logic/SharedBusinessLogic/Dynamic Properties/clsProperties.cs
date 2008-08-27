using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.DataTypes;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.Language;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties
{
    [Serializable()]
    public class clsProperties : BLListBase<clsProperties, clsProperty, clsProperty.BOGUIDDataKey>
    {
        #region "Factory Methods"
        [staFactory("ObjectType",typeof(clsLanguage))]
        public static clsProperties GetCollection(Type ObjectType)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsProperty));
            Criteria.AddBinaryExpression("BusinessObjectType_prp", "BusinessObjectType", "=", ObjectType.FullName, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);

            //Order the query by the following fields
            List<Pair<string, bool>> OrderByFields = new List<Pair<string, bool>> ();
            OrderByFields.Add(new Pair<string, bool> ("Category_prp", true));
            OrderByFields.Add(new Pair<string, bool> ("DefaultPosition_prp", true));

            Criteria.OrderByFields = OrderByFields;
            return GetCollection(Criteria);
        }

        #endregion
    }
}
