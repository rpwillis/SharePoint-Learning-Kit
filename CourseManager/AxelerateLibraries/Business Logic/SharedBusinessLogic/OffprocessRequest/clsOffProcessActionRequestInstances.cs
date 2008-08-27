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
    public class clsOffProcessActionRequestInstances : DetailsGUIDCollectionTemplate<clsOffProcessActionRequestInstances, clsOffProcessActionRequest, clsOffProcessActionRequestInstance>
    {
        #region "Business Properties and Methods"
        public static clsOffProcessActionRequestInstances GetPendingRequests()
        {
            BLCriteria Criteria = new BLCriteria(typeof(clsOffProcessActionRequestInstance));
            Criteria.Filter = new clsPendingRequestsFilter();
            Criteria.AddOrderedField("RequestDate_oai", true);
            return GetCollection(Criteria);
        }

        #endregion 

        #region "Business Properties and Methods"
        public void ExectutePendingRequests()
        {
            foreach (clsOffProcessActionRequestInstance Request in this)
            {
                if (Request.Execute())
                    Save();
            }
            

        }
            
        #endregion

        #region "ExtendedFilters"
        public class clsPendingRequestsFilter : DataLayerFilterBase
        {

            public clsPendingRequestsFilter()
            {
            }


            public override string SelectCommandText(DataLayerAbstraction pDataLayer, BLFieldMapList FieldMapList, string AditionalFilter, ref System.Collections.Generic.List<DataLayerParameter> Parameters)
            {

                SQLDataLayer TypedDataLayer = (SQLDataLayer)pDataLayer;
                string NSelectSQL = " SELECT " + TypedDataLayer.get_FieldListString(FieldMapList, "") +
                                    " FROM         OffProcessActionRequestInstances LEFT OUTER JOIN " +
                                    " OffProcessSchedules ON OffProcessActionRequestInstances.GUID_oai = OffProcessSchedules.MasterGUID_ops " +
                                    " WHERE     (OffProcessSchedules.NextExecutionDate_ops IS NULL OR " +
                                    "           OffProcessSchedules.NextExecutionDate_ops >= GETDATE()) AND (OffProcessActionRequestInstances.StateGUID_oai = '1')";
                if (AditionalFilter != "")
                {
                    AddAditionalFilter(ref NSelectSQL, AditionalFilter, "AND");
                }
                //return the filtered SQL to use in the Project.GetCollection functions that use this object as Extended Filter Criteria.
                return NSelectSQL;

            }
        }
        #endregion

    }
}
