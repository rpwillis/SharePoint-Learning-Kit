using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetActivityGroupCollectionByCourse : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityGroups m_RetrievedActivityGroups = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityGroups RetrievedActivityGroups
        {
            get 
            {
                return m_RetrievedActivityGroups;
            }
        }
        
        #endregion

        #region "DataLayer Access"

        /// <summary>
        /// 
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsActivityGroups));
                m_RetrievedActivityGroups = clsActivityGroups.GetCollectionByCourse(Criteria);                
            }
            catch (Exception e)
            {
                throw new Exception("Error retrieving activity group",e);    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Test()
        {
            if (RetrievedActivityGroups == null)
            {
                throw new Exception("Empty ActivityGroup collection retrieved.");   
            }
        }
        #endregion
    }
}
