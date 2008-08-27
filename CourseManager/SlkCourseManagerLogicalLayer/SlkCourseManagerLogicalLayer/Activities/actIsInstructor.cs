using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actIsInstructor : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Holds the Test's result.
        /// Null if the test has not been run yet.
        /// </summary>
        private Nullable<bool> m_IsInstructor = null;
        
        /// <summary>
        /// Defines the expected result from Test. True if Current User is an instructor, false otherwise. 
        /// </summary>
        private bool m_ExpectedResult = false;

        #endregion

        #region "Public Properties and Methods"        
             
        /// <summary>
        /// Gets the test result
        /// </summary>
        public bool? IsInstructor 
        {
            get 
            {
                return m_IsInstructor;  
            }
        }

        /// <summary>
        /// Gets or sets the expected result from Test. 
        /// True if Current User is an instructor, false otherwise.  
        /// </summary>
        public bool ExpectedResult
        {
            set 
            {
                m_ExpectedResult = value;
            }
            get
            {
                return m_ExpectedResult;
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Checks if Current Share Point User is instructor in SLK or not.
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {                
                m_IsInstructor = clsUser.IsInstructor();
            }catch(Exception e){
                throw new Exception("Failed checking if current Share Point user is Instructor or not.",e);
            }
        }         

        /// <summary>
        /// This method verifies that User was correctly recovered from SLK.
        /// </summary>
        public override void Test()
        {
            if (m_IsInstructor != m_ExpectedResult)
            {
                throw new Exception("The test returned different results than those expected. Returned: "+m_IsInstructor.ToString() + "Expected: " +m_ExpectedResult.ToString());
            }
            if (m_IsInstructor == null)
            {
                throw new Exception("Failed checking if current Share Point user is Instructor or not.");
            }
        }
        #endregion

    }
}