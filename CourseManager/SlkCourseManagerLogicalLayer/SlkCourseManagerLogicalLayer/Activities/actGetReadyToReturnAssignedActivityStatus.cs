using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetReadyToReturnAssignedActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsAssignedActivityStatus m_ReadyToReturnStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsAssignedActivityStatus ReadyToReturnStatus
        {
            get
            {
                return m_ReadyToReturnStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_ReadyToReturnStatus = clsAssignedActivityStatus.ReadyToReturnStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Ready To Return Status.", e);
            }
        }

        public override void Test()
        {
            if (m_ReadyToReturnStatus == null)
            {
                throw new Exception("Retrieved data for Ready To Return status is null.");
            }
            if (m_ReadyToReturnStatus.GUID.CompareTo("3") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Ready To Return Status.");
            }
        }
        #endregion
    }
}
