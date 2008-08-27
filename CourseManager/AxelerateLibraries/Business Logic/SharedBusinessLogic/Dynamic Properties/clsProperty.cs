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
    public class clsProperty : GUIDTemplate<clsProperty>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsProperty), "Properties", "_prp", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_Name = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_Type = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_DefaultValue = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        bool m_Required = false;

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_Category = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        int m_DefaultPosition = -1;

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_AllowedValues = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        string m_BusinessObjectType = "";


        #endregion

        #region "Business Properties and Methods"

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                PropertyHasChanged();
            }
        }

        public string Type
        {
            get { return m_Type; }
            set
            {
                m_Type = value;
                PropertyHasChanged();
            }
        }

        public string DefaultValue
        {
            get { return m_DefaultValue; }
            set
            {
                m_DefaultValue = value;
                PropertyHasChanged();
            }
        }

        public bool Required
        {
            get { return m_Required; }
            set
            {
                m_Required = value;
                PropertyHasChanged();
            }
        }

        public string Category
        {
            get { return m_Category; }
            set
            {
                m_Category = value;
                PropertyHasChanged();
            }
        }

        public int DefaultPosition
        {
            get { return m_DefaultPosition; }
            set
            {
                m_DefaultPosition = value;
                PropertyHasChanged();
            }
        }

        public string AllowedValues
        {
            get { return m_AllowedValues; }
            set
            {
                m_AllowedValues = value;
                PropertyHasChanged();
            }
        }

        public string BusinessObjectType
        {
            get { return m_BusinessObjectType; }
            set
            {
                m_BusinessObjectType = value;
                PropertyHasChanged();
            }
        }


        #endregion


    }
}
