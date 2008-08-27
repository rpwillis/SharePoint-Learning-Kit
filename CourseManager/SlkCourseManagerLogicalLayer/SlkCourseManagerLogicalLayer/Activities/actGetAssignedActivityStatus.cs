using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;


namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetAssignedActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityStatus m_AssignedStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityStatus AssignedStatus
        {
            get
            {
                return m_AssignedStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_AssignedStatus = clsActivityStatus.AssignedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Assigned Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_AssignedStatus == null)
            {
                throw new Exception("Retrieved data for Assigned status is null.");
            }
            if (m_AssignedStatus.GUID.CompareTo("2") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Assigned Status.");
            }
        }
        #endregion
    }
}
