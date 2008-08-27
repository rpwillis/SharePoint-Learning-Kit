using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    [Serializable(), SecurityToken("clsPermissionType", "clsPermissionTypes", "MIPCustom")]
    public class clsPermissionType : GUIDNameBusinessTemplate<clsPermissionType>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsPermissionType), "PermissionTypes", "_pet", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        private bool m_IsRoleSelected = false;  // Helper for this class only

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Category = "";

        #endregion

        #region "Business Properties and Methods"

        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
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
        public bool IsRoleSelected
        {
            get { return m_IsRoleSelected; }
            set
            {
                m_IsRoleSelected = value;
            }
        }
        public string ExtendedName
        {
            get { return Name + " - " + Description; }
        }


        #endregion


    }
}