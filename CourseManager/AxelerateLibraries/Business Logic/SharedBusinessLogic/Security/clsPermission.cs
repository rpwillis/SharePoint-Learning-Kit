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
    [Serializable(), SecurityToken("clsPermission", "clsPermissions", "MIPCustom")]
    public class clsPermission : MNGUIDRelationBusinessTemplate<clsPermission, clsBaseRole, clsPermissionType>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsPermission), "Permissions", "_prm", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_IsActive = false;
        #endregion

        #region "Business Properties and Methods"
         public bool IsActive
        {
            get { return m_IsActive; }
            set
            {
                m_IsActive = value;
                PropertyHasChanged();
            }
        }

        #endregion


    }
}