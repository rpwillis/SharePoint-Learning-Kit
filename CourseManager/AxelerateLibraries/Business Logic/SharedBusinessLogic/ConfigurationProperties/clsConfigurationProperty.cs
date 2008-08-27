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
    [Serializable(), SecurityToken("clsConfigurationProperty", "clsConfigurationProperties", "MIPCustom")]
    public class clsConfigurationProperty : GUIDNameBusinessTemplate<clsConfigurationProperty>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsConfigurationProperty), "ConfigurationProperties", "_cpr", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_DefaultValue = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_Encryption = false;

        #endregion

        #region "Business Properties and Methods"

        public String DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                m_DefaultValue = value;
                PropertyHasChanged();
            }
        }

        public bool Encrypted
        {
            get
            {
                return m_Encryption;
            }
        }

        public static clsConfigurationProperty GetProperty(String propertyName)
        {
            BLCriteria criteria = new BLCriteria(typeof(clsConfigurationProperties));
            criteria.AddBinaryExpression("Name_cpr", "Name", "=", propertyName, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            clsConfigurationProperties properties = clsConfigurationProperties.GetCollection(criteria);
            if (properties.Count == 0)
            {
                return null;
            }
            else
            {
                return properties[0];
            }
        }
        #endregion
    }
}
