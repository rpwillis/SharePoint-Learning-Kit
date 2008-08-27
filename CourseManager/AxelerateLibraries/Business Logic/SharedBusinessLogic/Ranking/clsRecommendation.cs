using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

using Axelerate.BusinessLogic.SharedBusinessLogic.Security;
namespace Axelerate.BusinessLogic.SharedBusinessLogic.Ranking
{
    [Serializable()]
    public class clsRecommendation : GUIDTemplate<clsRecommendation>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsRecommendation), "Recommendations", "_rcm", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
       #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        DateTime m_Date = new DateTime(); //TODO: Csla no longer supported SmartDate

        //FK field
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ADUserRecommended", false)]
        string m_ADUserRecommendedGUID = "";

        //FK field
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ADUserJudge", false)]
        string m_ADUserJudgeGUID = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        int m_Score = -1;

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_Comment = "";



        [CachedForeignObject("ADUserRecommended", typeof(clsADUser), "ADUserRecommendedGUID_rcm", "GUID_adu", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsADUser m_ADUserRecommended = null;


        [CachedForeignObject("ADUserJudge", typeof(clsADUser), "ADUserJudgeGUID_rcm", "GUID_adu", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsADUser m_ADUserJudge = null;
        #endregion

        #region "Business Properties and Methods"


        public DateTime Date
        {
            get { return m_Date.Date; }
            set
            {
                //m_Date = new SmartDate(value);
                m_Date = value;
                PropertyHasChanged();
            }
        }

        //FK property
        public string ADUserRecommendedGUID
        {
            get { return m_ADUserRecommendedGUID; }
            set
            {
                m_ADUserRecommendedGUID = value;
                PropertyHasChanged();
            }
        }

        //FK property
        public string ADUserJudgeGUID
        {
            get { return m_ADUserJudgeGUID; }
            set
            {
                m_ADUserJudgeGUID = value;
                PropertyHasChanged();
            }
        }

        public int Score
        {
            get { return m_Score; }
            set
            {
                m_Score = value;
                PropertyHasChanged();
            }
        }

        public string Comment
        {
            get { return m_Comment; }
            set
            {
                m_Comment = value;
                PropertyHasChanged();
            }
        }
        /// <summary>
        /// Gets or sets the project's language using a clsLanguage instance.  
        /// </summary>
        public clsADUser ADUserRecommended
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsADUser>.GetProperty(this, ref m_ADUserRecommended, m_ADUserRecommendedGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsADUser>.SetProperty(ref m_ADUserRecommended, value, ref m_ADUserRecommendedGUID, true);
                PropertyHasChanged();
            }
        }
        /// <summary>
        /// Gets or sets the project's language using a clsLanguage instance.  
        /// </summary>
        public clsADUser ADUserJudge
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsADUser>.GetProperty(this, ref m_ADUserJudge, m_ADUserJudgeGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsADUser>.SetProperty(ref m_ADUserJudge, value, ref m_ADUserJudgeGUID, true);
                PropertyHasChanged();
            }
        }
        #endregion

    }
}
