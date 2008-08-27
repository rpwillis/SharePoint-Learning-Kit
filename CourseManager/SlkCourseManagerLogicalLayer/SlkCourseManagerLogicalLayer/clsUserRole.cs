
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    
    /// <summary>
    /// This class represents an User Role within SLK.
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsUserRole : GUIDNameBusinessTemplate<clsUserRole>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsUserRole), "UserRoles", "_uro", false, false, "SLKCM");

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
        /// Learner User Role
        /// </summary>
        private static clsUserRole m_LearnerUserRole = null;

        /// <summary>
        /// Instructor User Role
        /// </summary>
        private static clsUserRole m_InstructorUserRole = null;
        
        /// <summary>
        /// Observer User Role
        /// </summary>
        private static clsUserRole m_ObserverUserRole = null;

        #endregion

        #region "Business Properties and Methods"
        /// <summary>
        /// Gets the Learner User Role
        /// </summary>
        public static clsUserRole LearnerUserRole
        {
            get
            {
                if (m_LearnerUserRole == null)
                {
                    m_LearnerUserRole = TryGetObjectByGUID("2", null);
                }
                return m_LearnerUserRole;
            }
        }

        /// <summary>
        /// Gets the Instructor User Role
        /// </summary>
        public static clsUserRole InstructorUserRole
        {
            get
            {
                if (m_InstructorUserRole == null)
                {
                    m_InstructorUserRole = TryGetObjectByGUID("1", null);
                }
                return m_InstructorUserRole;
            }
        }
        
        /// <summary>
        /// Gets the Observer User Role
        /// </summary>
        public static clsUserRole ObserverUserRole
        {
            get
            {
                if (m_ObserverUserRole == null)
                {
                    m_ObserverUserRole = TryGetObjectByGUID("3", null);
                }
                return m_ObserverUserRole;
            }
        }

        #endregion
    }
}
