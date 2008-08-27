using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Language
{
    /// <summary>
    /// Represent a Language in the MIP Portal.  It inherits two base properties from GUIDNameBusinessTemplate, a GUID property that 
    /// identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsLanguage", "clsLanguages", "MIPCustom"),SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsLanguage : GUIDNameBusinessTemplate<clsLanguage>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsLanguage), "Languages", "_lan", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"


        #endregion

        #region "Business Properties and Methods"


        #endregion

        #region "Shared Properties and Methods"
        private static clsLanguage m_DefaultLanguage = null;
        public static clsLanguage DefaultLanguage
        {
            get 
            {
                if (m_DefaultLanguage == null)
                    m_DefaultLanguage = clsLanguage.GetObjectByGUID("1",null);
                return m_DefaultLanguage;
            }
        }

        #endregion
    }
}