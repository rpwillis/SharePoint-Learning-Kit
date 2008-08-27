using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLogic.SharedBusinessLogic.Security;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Ranking
{
    [Serializable()]
    public class clsRecommendations : BLListBase<clsRecommendations, clsRecommendation, clsRecommendation.BOGUIDDataKey>
    {
        #region "Factory Methods"

        public static clsRecommendations GetCollection(clsADUser ADUser)
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsRecommendations));
            Criteria.AddBinaryExpression("ADUserRecommendedGUID_rcm", "ADUserRecommendedGUID", "=", ADUser.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            return clsRecommendations.GetCollection(Criteria);

        }

        #endregion
    }
}