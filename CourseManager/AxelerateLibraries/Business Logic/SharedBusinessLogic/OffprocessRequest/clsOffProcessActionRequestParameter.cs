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

    /// <summary>
    /// Represents a TFS Project It inherits two base properties from DetailGUIDBussinessTemplate, 
    /// a GUID property that identifies the instance and a MasterObject that relates this server to its parent TFS Server.
    /// </summary>
    [Serializable(), SecurityToken("clsOffProcessActionRequestParameter", "clsOffProcessActionRequestParameters", "MIPCustom")]
    public class clsOffProcessActionRequestParameter : DetailGUIDBussinessTemplate<clsOffProcessActionRequest, clsOffProcessActionRequestParameter>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsOffProcessActionRequestParameter), "OffProcessActionRequestParameters", "_oap", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Parameter Name
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Name = "";

        /// <summary>
        /// Parameter Call Order
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_Order = 0;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or Sets the Parameter Name
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Parameter Order
        /// </summary>
        public int Order
        {
            get { return m_Order; }
            set
            {
                m_Order = value;
                PropertyHasChanged();
            }
        }


        #endregion


    }
}