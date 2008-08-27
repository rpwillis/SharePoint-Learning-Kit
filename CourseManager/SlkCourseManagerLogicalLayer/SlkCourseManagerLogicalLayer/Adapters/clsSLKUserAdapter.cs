using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents.SharePoint;
using Schema = Microsoft.SharePointLearningKit.Schema;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// User Roles
    /// </summary>
    public enum UserRole { Instructor = 1, Learner = 2, Observer = 3 };
 
    /// <summary>
    /// Adapter to manage users in SLK (SharePoint).
    /// </summary>
    public class clsSLKUserAdapter : clsAdapterBase
    {        

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public clsSLKUserAdapter() : base()
        {

        }

        #endregion

        #region ApdaterBase
        /// <summary>
        /// Gets the Adapter's Type
        /// </summary>
        public override clsAdapterBase.AdapterCapabilities AdapterType
        {
            get
            {
                return clsAdapterBase.AdapterCapabilities.AdapterReadOnly;
            }
        }
        /// <summary>
        /// Transform Data.
        /// </summary>
        /// <param name="FieldIndex"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public override object DataTransform(int FieldIndex, System.Data.DataRow Row)
        {
            switch (FieldIndex)
            {
                case 0:
                    //Email
                    return (string)Row[1];
                case 1:
                    //LoginName
                    return (string)Row[2];
                case 2:
                    //LoginRole
                    return (string)Row[4];
                case 3:
                    //Name
                    return (string)Row[3];
                case 4:
                    //ID
                    return (string)Row[0];
            }
            return null;
        }

        #endregion
                
        #region GetUsers
        /// <summary>
        /// Gets a  Collection of all users (learners and Instructors) from SLK.
        /// </summary>        
        public dtsUser GetUsers()
        {            
            try
            {
                dtsUser users = new dtsUser();

                SPWeb spWeb = SPContext.Current.Web;
                //Get list of all Users in SLK
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                SlkMemberships memberships = slkStore.GetMemberships(spWeb, null, null);

                //Search user in Learners
                foreach (SlkUser learner in memberships.Learners)
                {
                    dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                    row.GUID = learner.UserId.GetKey().ToString();
                    row.Email = learner.SPUser.Email;
                    row.LoginName = learner.SPUser.LoginName;
                    row.Name = learner.SPUser.Name;
                    row.LoginRole = "2";

                    users.tblUsers.AddtblUsersRow(row);
                }

                //Search user in instuctors
                foreach (SlkUser instructor in memberships.Instructors)
                {
                    dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                    row.GUID = instructor.UserId.GetKey().ToString();
                    row.Email = instructor.SPUser.Email;
                    row.LoginName = instructor.SPUser.LoginName;
                    row.Name = instructor.SPUser.Name;
                    row.LoginRole = "1";

                    users.tblUsers.AddtblUsersRow(row);
                }

                return users;
            }
            catch (Exception e)
            {                
                throw new Exception(Resources.ErrorMessages.GetUserDataError);
            }            
        }

        /// <summary>
        /// Gets a User Data form SLK
        /// </summary>
        /// <param name="GUID">Target user's Identifier</param>
        public dtsUser GetUsers(string GUID)
        {            
            try
            {
                dtsUser users = new dtsUser();

                SPWeb spWeb = SPContext.Current.Web;
                //Get list of all Users in SLK
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                SlkMemberships memberships = slkStore.GetMemberships(spWeb, null, null);
                //Search user in instuctors
                foreach (SlkUser instructor in memberships.Instructors)
                {
                    if (instructor.UserId.GetKey().ToString().CompareTo(GUID) == 0)
                    {                      

                        dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                        row.GUID = instructor.UserId.GetKey().ToString();
                        row.Email = instructor.SPUser.Email;
                        row.LoginName = instructor.SPUser.LoginName;
                        row.Name = instructor.SPUser.Name;
                        row.LoginRole = "1";

                        users.tblUsers.AddtblUsersRow(row);

                        return users;
                    }
                }
                //Search user in learners
                foreach (SlkUser learner in memberships.Learners)
                {
                    if (learner.UserId.GetKey().ToString().CompareTo(GUID) == 0)
                    {                        
                        dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                        row.GUID = learner.UserId.GetKey().ToString();
                        row.Email = learner.SPUser.Email;
                        row.LoginName = learner.SPUser.LoginName;
                        row.Name = learner.SPUser.Name;
                        row.LoginRole = "2";

                        users.tblUsers.AddtblUsersRow(row);

                        return users;
                    }
                }
                return users;
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetUserDataError);
            }            
        }

        /// <summary>
        /// Gets a  Collection of all users (learners and Instructors) from SLK.
        /// </summary>        
        /// <param name="UserRole">Type of User to Query</param>
        public dtsUser GetUsers(UserRole userRole)
        {
            try
            {
                dtsUser users = new dtsUser();

                SPWeb spWeb = SPContext.Current.Web;
                //Get list of all Users in SLK
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                SlkMemberships memberships = slkStore.GetMemberships(spWeb, null, null);

                switch (userRole) 
                {
                    case UserRole.Learner:
                        //Search user in Learners
                        foreach (SlkUser learner in memberships.Learners)
                        {
                            dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                            row.GUID = learner.UserId.GetKey().ToString();
                            row.Email = learner.SPUser.Email;
                            row.LoginName = learner.SPUser.LoginName;
                            row.Name = learner.SPUser.Name;
                            row.LoginRole = "2";

                            users.tblUsers.AddtblUsersRow(row);
                        }
                        break;

                    case UserRole.Instructor:
                        //Search user in instuctors
                        foreach (SlkUser instructor in memberships.Instructors)
                        {
                            dtsUser.tblUsersRow row = users.tblUsers.NewtblUsersRow();

                            row.GUID = instructor.UserId.GetKey().ToString();
                            row.Email = instructor.SPUser.Email;
                            row.LoginName = instructor.SPUser.LoginName;
                            row.Name = instructor.SPUser.Name;
                            row.LoginRole = "1";

                            users.tblUsers.AddtblUsersRow(row);
                        }
                        break;                
                }
                return users;
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetUserDataError);
            }
        }


        #endregion

        #region IsInstructor

        /// <summary>
        /// Gets a User Data form SLK
        /// </summary>        
        public bool IsInstructor()
        {
            try
            {
                SPWeb spWeb = SPContext.Current.Web;
                SlkStore slkStore = SlkStore.GetStore(spWeb);
                return slkStore.IsInstructor(spWeb);
            }
            catch (Exception e)
            {                
                throw new Exception(Resources.ErrorMessages.CheckForInstructorError);
            }           
        }

        #endregion
    }
}
       