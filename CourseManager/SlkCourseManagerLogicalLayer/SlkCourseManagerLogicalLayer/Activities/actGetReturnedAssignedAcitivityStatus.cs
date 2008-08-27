using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetReturnedAssignedAcitivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsAssignedActivityStatus m_ReturnedStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsAssignedActivityStatus ReturnedStatus
        {
            get
            {
                return m_ReturnedStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_ReturnedStatus = clsAssignedActivityStatus.ReturnedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Returned Status.", e);
            }
        }

        public override void Test()
        {
            if (m_ReturnedStatus == null)
            {
                throw new Exception("Retrieved data for Returned status is null.");
            }
            if (m_ReturnedStatus.GUID.CompareTo("4") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Returned Status.");
            }
        }
        #endregion
    }
}
