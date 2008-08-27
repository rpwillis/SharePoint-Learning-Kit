using System;
using System.Collections.Generic;
using System.Text;

using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

using Axelerate.BusinessLogic.SharedBusinessLogic.Support;
namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    /// <summary>
    /// This class is inherited from the MNGUIDRelationBusinessTemplate
    /// </summary>
    [Serializable()]
    public class clsADUser_Role : MNGUIDRelationBusinessTemplate<clsADUser_Role, clsADUser, clsBaseRole>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsADUser_Role), "ADUsers_Roles", "_adr", false, true, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Project, Portal or Branch GUID, it depends on the Roles.RoleLevel to make the link
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ObjectGUID = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_Active = true;

        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "UserRole", false)]
        private string m_RoleName = "";

        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_RoleLevel = "";

        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ADUserName", false)]
        private string m_UserName = "";

        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ADUserLogin", false)]
        private string m_LoginName = "";

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the User Role using a valid GUID
        /// </summary>
        public string ObjectGUID
        {
            get { return m_ObjectGUID; }
            set
            {
                m_ObjectGUID = value;
                PropertyHasChanged();
            }
        }
        /// <summary>
        /// Gets the AD User name for this AD User Role
        /// </summary>
        public string ADUserName
        {
            get
            {
                return m_UserName;
            }
        }
        /// <summary>
        /// Gets the full user name of this AD User
        /// </summary>
        public string ADUserLogin
        {
            get
            {
                return m_LoginName;
            }
        }

        /// <summary>
        /// Gets the user role name
        /// </summary>
        public string UserRole
        {
            get
            {
                return m_RoleName;
            }
        }

        /// <summary>
        /// Gets the user role level
        /// </summary>
        public string RoleLevel
        {
            get
            {
                return m_RoleLevel;
            }
        }

        /// <summary>
        /// Return if user is active or not
        /// </summary>
        public bool Active
        {
            get { return m_Active; }
            set
            {
                m_Active = value;
                PropertyHasChanged();
            }
        }
        #endregion
        #region "Add a User Role"
        /// <summary>
        /// Add a new user role. If a user has already a role with the same TargetGUID, the call
        /// returns false;
        /// </summary>
        /// <param name="RoleGUID">Role GUID</param>
        /// <param name="TargetGUID">"PORTAL" or Project GUID</param>
        /// <returns></returns>
        public bool AddUserRole(string UserName, string RoleGUID, string TargetGUID, bool RoleActive)
        {
            // If the user making the request isn't in the list of active users, it is created
            clsADUser ADUser;
            clsADUsers ADUsers = clsADUsers.GetCollectionByUserName(UserName);
            if (ADUsers.Count == 0)
            {
                // Insert this user
                ADUser = clsActiveDirectory.CreateUserFromAD(UserName);
            }
            else
                ADUser = ADUsers[0];
            // Check to make sure that the user does not have role here
            BLCriteria Criteria = new BLCriteria(typeof(clsADUser_Role));
            Criteria.AddBinaryExpression("MasterGUID_adr", "MasterGUID", "=", ADUser.GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddBinaryExpression("ObjectGUID_adr", "ObjectGUID", "=", TargetGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);

            clsADUser_Role UserRole = null;
            try
            {
                UserRole = clsADUser_Role.GetObject(Criteria);
                if(UserRole!=null)
                    return false;
            }
            catch (Exception ex)
            {
            }
            this.MasterGUID = ADUser.GUID;
            this.ObjectGUID = TargetGUID;
            this.DetailGUID = RoleGUID;
            this.Active = RoleActive;
            Save();
            return true;
        }
        #endregion
    }
}
