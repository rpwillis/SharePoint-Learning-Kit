using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetPendingStartActivityGroupSatatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityGroupStatus m_PendingStartStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityGroupStatus PendingStartStatus
        {
            get
            {
                return m_PendingStartStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_PendingStartStatus = clsActivityGroupStatus.PendingStartSatatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Pending Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_PendingStartStatus == null)
            {
                throw new Exception("Retrieved data for Pending Start status is null.");
            }
            if (m_PendingStartStatus.GUID.CompareTo("1") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Pending Start Status.");
            }
        }
        #endregion
    }
}
