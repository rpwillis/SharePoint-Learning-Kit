using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetSittingOnInboxAssignedActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsAssignedActivityStatus m_SittingOnInboxStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsAssignedActivityStatus SittingOnInboxStatus
        {
            get
            {
                return m_SittingOnInboxStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_SittingOnInboxStatus = clsAssignedActivityStatus.SittingOnInboxStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Sitting On Inbox Status.", e);
            }
        }

        public override void Test()
        {
            if (m_SittingOnInboxStatus == null)
            {
                throw new Exception("Retrieved data for Sitting On Inbox status is null.");
            }
            if (m_SittingOnInboxStatus.GUID.CompareTo("2") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Sitting On Inbox Status.");
            }
        }
        #endregion
    }
}
