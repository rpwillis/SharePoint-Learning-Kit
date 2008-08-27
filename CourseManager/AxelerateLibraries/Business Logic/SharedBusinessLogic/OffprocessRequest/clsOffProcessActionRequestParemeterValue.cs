using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.OffprocessRequest
{
    [Serializable(), SecurityToken("clsOffProcessActionRequestParameterValue", "clsOffProcessActionRequestParameterValues", "MIPCustom")]
    public class clsOffProcessActionRequestParameterValue : MNGUIDRelationBusinessTemplate<clsOffProcessActionRequestParameterValue, clsOffProcessActionRequestInstance, clsOffProcessActionRequestParameter>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsOffProcessActionRequestParameterValue), "OffProcessActionRequestParameterValues", "_oav", false,  true, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Parameter Value
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Value = "";


        #endregion

        #region "Business Properties and Methods"
        /// <summary>
        /// Gets or Sets the Parameter Value
        /// </summary>
        public string Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                PropertyHasChanged();
            }
        }

        #endregion


    }
}