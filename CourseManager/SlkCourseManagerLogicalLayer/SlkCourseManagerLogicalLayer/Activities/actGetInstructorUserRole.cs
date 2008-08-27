using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetInstructorUserRole : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Holds the retrieved User Role.
        /// </summary>
        private clsUserRole m_InstructorRole = null;

        #endregion

        #region "Public Properties and Methods"        
             
        /// <summary>
        /// Gets the retrieved User Role.
        /// </summary>
        public clsUserRole InstructorRole 
        {
            get 
            {
                return m_InstructorRole;  
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Trys to retrieve the Instructor User Role from Course Manager Database.
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                m_InstructorRole = clsUserRole.InstructorUserRole;
            }catch(Exception e){
                throw new Exception("Failed retrieving instructor user role.",e);
            }
        }         

        /// <summary>
        /// This method verifies that User Role retrieved is the Instructor User Role.
        /// </summary>
        public override void Test()
        {
            if (m_InstructorRole == null)
            {
                throw new Exception("Retrieved data for Instructor user role is null.");
            }
            if (m_InstructorRole.GUID.CompareTo("1")!=0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve intructor user role.");
            }            
        }
        #endregion

    }
}