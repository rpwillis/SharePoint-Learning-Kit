using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties
{
    [Serializable(), SecurityToken("clsProfile_Property", "clsProfile_Properties", "MIPCustom")]
    public class clsProfile_Property : MNGUIDRelationBusinessTemplate<clsProfile_Property, clsConfigurationProfile, clsConfigurationProperty>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsProfile_Property), "Profiles_Properties", "_ppr", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"
        [BusinessLayerFrameWork.BLCore.Attributes.FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Value = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_Encryption = false;

        #endregion

        #region "Business Properties and Methods"

        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }

        public bool Encrypted
        {
            get
            {
                return m_Encryption;
            }
        }

        #endregion
    }
}
