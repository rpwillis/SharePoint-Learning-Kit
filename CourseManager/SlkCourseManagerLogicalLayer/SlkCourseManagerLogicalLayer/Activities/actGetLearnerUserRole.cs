using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetLearnerUserRole : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Holds the retrieved User Role.
        /// </summary>
        private clsUserRole m_LearnerRole = null;

        #endregion

        #region "Public Properties and Methods"        
             
        /// <summary>
        /// Gets the retrieved User Role.
        /// </summary>
        public clsUserRole LearnerRole 
        {
            get 
            {
                return m_LearnerRole;  
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Trys to retrieve the Learner User Role from Course Manager Database.
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                m_LearnerRole = clsUserRole.LearnerUserRole;
            }catch(Exception e){
                throw new Exception("Failed retrieving Learner user role.",e);
            }
        }         

        /// <summary>
        /// This method verifies that User Role retrieved is the Learner User Role.
        /// </summary>
        public override void Test()
        {
            if (m_LearnerRole == null)
            {
                throw new Exception("Retrieved data for learner user role is null.");
            }
            if (m_LearnerRole.GUID.CompareTo("1")!=0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve learner user role.");
            }            
        }
        #endregion

    }
}