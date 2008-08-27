using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetLearnerCollectionFromCurrentSite : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Recovered Users from SLK after running the Test. 
        /// </summary>
        private clsUsers m_RecoveredUsers = null;    

        #endregion

        #region "Public Properties and Methods"        
             
        /// <summary>
        /// Gets the Recoverd users form SLK.
        /// </summary>
        public clsUsers RecoveredUsers 
        {
            get 
            {
                return m_RecoveredUsers;  
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Attempts to recover all Learner Users from SLK.
        /// If any user is no present in the Course Manager DB it is inserted.
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                m_RecoveredUsers = clsUsers.GetCollection();                
            }catch(Exception e){
                throw new Exception("Failed retrieving list of SLK Learners",e);
            }
        }         

        /// <summary>
        /// This method verifies that User collection was correctly recovered from SLK and that all users were created in the Course Manager Data Base.
        /// </summary>
        public override void Test()
        {
            if (m_RecoveredUsers == null)
            {
                throw new Exception("Retrieved Empty List of SLK Learners");
            }
            foreach (clsUser user in m_RecoveredUsers)
            {
                clsUser us = clsUser.TryGetObjectByGUID(user.GUID,null);
                if (us == null) 
                {
                    throw new Exception("User with ID: "+user.GUID+" and Name: "+ user.Name+" was retrieved from SLK but is not present in Course Manager Data Base.");
                }                
            }

            
        }
        #endregion

    }
}
