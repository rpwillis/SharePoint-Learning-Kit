using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a collection of Users in Course Manager
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsUsers : BLListBase<clsUsers, clsUser, clsUser.BOGUIDDataKey>
    {
        #region "Factory Methods"
        
        /// <summary>
        /// Gets the Users with Learners Permissions on the current Course.
        /// </summary>
        /// <returns></returns>
        public static clsUsers GetLearners() 
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsUsers));
                Criteria.AddPreFilter("UserRole", UserRole.Learner);
                return clsUsers.GetCollection(Criteria);
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetUserCollectionError, e);
            }
        }

        /// <summary>
        /// Gets the Users with Instructor Permissions on the current Course.
        /// </summary>
        /// <returns></returns>
        public static clsUsers GetInstructors()
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsUsers));
                Criteria.AddPreFilter("UserRole", UserRole.Instructor);
                return clsUsers.GetCollection(Criteria);
            }            
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.GetUserCollectionError,e);
            }
        }        
        
        #endregion

        #region "ExtendedFilters"
        #endregion

        #region override
        
        /// <summary>
        /// Save the current instance of the User.
        /// </summary>
        /// <returns></returns>
        public override clsUsers Save()
        {
            try
            {
                foreach (clsUser user in this)
                {
                    user.AssignedActivities.Save();
                }
                return this;
            }
            catch(Exception e)
            {
                throw new Exception(Resources.ErrorMessages.SaveUserError,e);
            }
        }

        #endregion
    }
}

