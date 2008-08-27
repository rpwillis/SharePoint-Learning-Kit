
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    
    /// <summary>
    /// This class represents a user in the Course Manager
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsUser : GUIDNameBusinessTemplate<clsUser>
    {
        #region "DataLayer Overrides"
        
        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        //private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsUser), "Users", "_usr", false, false, String.Empty);
        private static clsAdapterDataLayer m_DataLayer = new clsAdapterDataLayer(typeof(clsUser), typeof(clsSLKUserAdapter).AssemblyQualifiedName, "GetUsers", "_usr");

        /// <summary>
        /// Gets or sets the DataLayer value
        /// </summary>
        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Email
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Email = "";

        /// <summary>
        /// Login Name
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_LoginName = "";
        
        /// <summary>
        /// User Role GUID
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "UserRole", false)]
        private string m_UserRoleGUID = "";

        /// <summary>
        /// User Role
        /// </summary>
        [CachedForeignObject("UserRole", typeof(clsUser), "UserRoleGUID_usr", "GUID_uro", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsUserRole m_UserRole = null;

        /// <summary>
        /// Assigned Activities
        /// </summary>
        [ListFieldMap(false, true, true)]
        private clsUser_Assignments m_AssignedActivities = null; 

        /// <summary>
        /// Grade
        /// </summary>
        [FieldMap(false, false, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private double m_Grade = 0;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute Email
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Email
        {
            get { return m_Email; }            
        }

        /// <summary>
        /// Gets or sets the value of the attribute LoginName
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string LoginName
        {
            get { return m_LoginName; }            
        }

        /// <summary>
        /// Gets or sets the value of the attribute UserRoleGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string UserRoleGUID
        {
            get { return m_UserRoleGUID; }           
        }

        /// <summary>
        /// Gets or sets the correct Guid to the class UserRole
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsUserRole UserRole
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsUserRole>.GetProperty(this, ref m_UserRole, m_UserRoleGUID);
            }            
        }

        /// <summary>
        /// Gets a collection of Assigned Activities
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsUser_Assignments AssignedActivities
        {
            get
            {
                if (m_UserRoleGUID.CompareTo(clsUserRole.LearnerUserRole.GUID) == 0)
                {
                    if (m_AssignedActivities == null)
                    {
                        m_AssignedActivities = clsUser_Assignments.GetCollectionByLearnerGUID(m_GUID);
                    }
                }
                return m_AssignedActivities;
            }
        }

        /// <summary>
        /// Gets a collection of Assigned Activities
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Grade
        {
            get
            {
                double PP=0;
                double FP=0;

                foreach(clsUser_Assignment ua in this.AssignedActivities)
                {
                    BLCriteria criteria = new BLCriteria(typeof(clsActivity));
                    criteria.AddBinaryExpression("SLKGUID_act", "SLKGUID", "=", ua.AssignmentGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                    clsActivities act = clsActivities.GetCollection(criteria);
                    if (act.Count != 0)
                    {
                        if (act[0].Gradeable)
                        {
                            if (ua.FinalPoints != -1)
                            {
                                PP += (ua.FinalPoints * (double)act[0].Weight) / ua.PointsPossible;
                                FP += act[0].Weight;
                            }
                        }
                    }
                    else 
                    {
                        if (ua.FinalPoints != -1 && ua.PointsPossible > 0)
                        {
                            PP += ua.FinalPoints;
                            FP += ua.PointsPossible;
                        }
                    }
                }
                return Math.Round(PP, 2).ToString() + "/" + Math.Round(FP, 2).ToString();
            }
        }


        #endregion              
        
        #region Custom Methods
        
        /// <summary>
        /// Returns true if current user is a SLK Instructor and false othrewise.          
        /// </summary> 
        public static bool IsInstructor()
        {
            try
            {
                clsSLKUserAdapter usrAdapter = new clsSLKUserAdapter();
                return usrAdapter.IsInstructor();
            }
            catch(Exception e)
            {
                throw new Exception(Resources.ErrorMessages.CheckForInstructorError, e);
            }
        }
        
        #endregion

        #region "Factory Methods"
        
        /// <summary>
        /// Gets User From SLK Database.         
        /// </summary> 
        /// <param name="GUID">Target User's GUID</param>                
        public static clsUser GetUser(string GUID)
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsUsers));
                Criteria.AddPreFilter("GUID", GUID);
                clsUsers users = clsUsers.GetCollection(Criteria);

                if (users.Count > 0)
                {
                    return users[0];
                }
                return null;
            }
            catch (Exception e) 
            {
                throw new Exception(Resources.ErrorMessages.RetrievingUserError +" "+ GUID,e);
            }
        }
                       
        #endregion               

    }
}
