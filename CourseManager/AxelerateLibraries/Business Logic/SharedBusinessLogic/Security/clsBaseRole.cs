using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLogic.SharedBusinessLogic.ConfigurationProperties;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    [Serializable(), SecurityToken("clsBaseRole", "clsRoles", "MIPCustom")]
    public class clsBaseRole : GUIDNameBusinessTemplate<clsBaseRole>
    {
        #region "DataLayer Overrides"

        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsBaseRole), "Roles", "_rol", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_RoleLevel = "";

        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Constant = "";

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

        public string RoleLevel
        {
            get { return m_RoleLevel; }
            set
            {
                m_RoleLevel = value;
                PropertyHasChanged();
            }
        }
        public string Constant
        {
            get { return m_Constant; }
            set
            {
                m_Constant = value;
                PropertyHasChanged();
            }
        }        /// <summary>
        /// Link to the projects's page
        /// </summary>
        public string RoleLink
        {
            get
            {
                return clsConfigurationProfile.Current.getPropertyValue("WebApplicationPath") +
                    clsConfigurationProfile.Current.getPropertyValue("Role_Page") + "?RoleGUID=" + GUID;
            }
        }

        /// <summary>
        /// Role Name in TFS Server of this Role Object
        /// </summary>
        public string TFSRoleName
        {
            get { return ""; }
            set { }
        }

        /// <summary>
        /// Get the users that has this role
        /// </summary>
        /// <returns></returns>
        public clsADUsers GetUsers()
        {
            return null;
        }

        /// <summary>
        /// Collection of Permission associated with this role
        /// </summary>
        public clsPermissions Permissions
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Set the Permission for this role, if isSet is true GRANT the permission if set to isFalse DENY the permission
        /// if a Permission is not set for a role is assumed has not has permission to do the action.
        /// </summary>
        /// <param name="Permission">Permission to be setted</param>
        /// <param name="isSet">if true Grant the permission else Deny the permission</param>
        public void SetPermission(clsPermission Permission, bool isSet)
        {
        }
        
        /// <summary>
        /// Set the Permission for this role, if isSet is true GRANT the permission if set to isFalse DENY the permission
        /// if a Permission is not set for a role is assumed has not has permission to do the action.
        /// </summary>
        /// <param name="Permission">Permission to be setted</param>
        /// <param name="isSet">if true Grant the permission else Deny the permission</param>
        public void SetPermission(string PermissionGUID, bool isSet)
        {
        }

        /// <summary>
        /// Remove the Permission from the role.
        /// </summary>
        /// <param name="PermissionToRemove">Permission to remove</param>
        public void RemovePermission(clsPermission PermissionToRemove)
        {
        }

        /// <summary>
        /// Remove the Permission from the role.
        /// </summary>
        /// <param name="PermissionToRemove">GUID of the permission to remove</param>
        public void RemovePermission(string PermissionToRemove)
        {
        }

        /// <summary>
        /// Add the User to this Role for a specific Object
        /// When the user is added to this role, automatically the user is added to the corresponding TFS Server role. 
        /// </summary>
        /// <param name="User">User to be added</param>
        /// <param name="Object">Business object supplying the GUID for the relation object</param>
        public void AddUser(clsADUser User, BLBusinessBase Object)
        {
            clsADUsers_Roles UserRoles = clsADUsers_Roles.GetCollection(User);
            //First search if the user already has the role assigned
            bool Assigned = false;

            string ObjectGUID = Object["GUID"].ToString();

            for (int i = 0; (i < UserRoles.Count) && (Assigned == false); i++)
            {
                clsADUser_Role UserRole = UserRoles[i];
                //If the roll and the GUID for the project match, the role is already assigned
                if ((UserRole.DetailObject.GUID == GUID) && (UserRole.ObjectGUID == ObjectGUID))
                    Assigned = true;
            };


            //If not assigned creates the new role
            if (!Assigned)
            {
                clsADUser_Role NewUserRole = new clsADUser_Role();
                NewUserRole.MasterObject = User;
                NewUserRole.DetailObject = this;
                NewUserRole.ObjectGUID = ObjectGUID;
                NewUserRole.Active = true;
                NewUserRole.Save();
            };
        }

        /// <summary>
        /// Add the User with login "LoginUser" to this Role for a specific Object
        /// When the user is added to this role, automatically the user is added to the corresponding TFS Server role. 
        /// </summary>
        /// <param name="LoginUser"></param>
        /// <param name="ObjectGUID"></param>
        public void AddUser(string LoginUser, string ObjectGUID)
        {
            clsADUser User = clsADUser.GetByName(LoginUser);
            clsADUsers_Roles UserRoles = clsADUsers_Roles.GetCollection(User);
            //First search if the user already has the role assigned
            bool Assigned = false;
            
            for (int i = 0; (i < UserRoles.Count) && (Assigned == false); i++)
            {
                clsADUser_Role UserRole = UserRoles[i];
                //If the roll and the GUID for the project match, the role is already assigned
                if ((UserRole.DetailObject.GUID == GUID) && (UserRole.ObjectGUID == ObjectGUID))
                    Assigned = true;
            };


            //If not assigned creates the new role
            if (!Assigned)
            {
                clsADUser_Role NewUserRole = new clsADUser_Role();
                NewUserRole.MasterObject = User;
                NewUserRole.DetailObject = this;
                NewUserRole.ObjectGUID = ObjectGUID;
                NewUserRole.Active = true;
                NewUserRole.Save();
            };

        }

        /// <summary>
        /// Remove the User form the this role for specific Object
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Object"></param>
        public void RemoveUser(clsADUser User, BLBusinessBase Object)
        {

        }

        /// <summary>
        /// Remove the User with login "LoginUser" form the this role for specific Object with guid ObjectGUID
        /// </summary>
        /// <param name="User"></param>
        /// <param name="Object"></param>
        public void RemoveUser(string LoginUser, string ObjectGUID)
        {

        }

        #endregion

        #region "Static Properties and Methods"
        
        #endregion

        #region "Users"

        #endregion



    }
}