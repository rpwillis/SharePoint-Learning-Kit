using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;


namespace Axelerate.BusinessLogic.SharedBusinessLogic.DynamicProperties
{
    [Serializable()]
    public class clsPropertyValue : DetailGUIDBussinessTemplate<clsProperty, clsPropertyValue>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsPropertyValue), "PropertyValues", "_prv", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_PropertyValue = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_ObjectGUID = "";

        #endregion

        #region "Business Properties and Methods"

        public string PropertyValue
        {
            get { return m_PropertyValue; }
            set
            {
                m_PropertyValue = value;
                PropertyHasChanged();
            }
        }

        public string ObjectGUID
        {
            get { return m_ObjectGUID; }
            set
            {
                m_ObjectGUID = value;
                PropertyHasChanged();
            }
        }
        #endregion


    }
}
