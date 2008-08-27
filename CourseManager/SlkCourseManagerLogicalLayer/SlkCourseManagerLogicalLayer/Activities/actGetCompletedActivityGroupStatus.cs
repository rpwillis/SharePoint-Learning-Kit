using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    class actCompletedActivityGroupStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityGroupStatus m_CompletedActivityStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityGroupStatus CompletedActivityStatus
        {
            get
            {
                return m_CompletedActivityStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_CompletedActivityStatus = clsActivityGroupStatus.StartedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Completed Activity Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_CompletedActivityStatus == null)
            {
                throw new Exception("Retrieved data for Completed Activity status is null.");
            }
            if (m_CompletedActivityStatus.GUID.CompareTo("3") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Completed Activity Status.");
            }
        }
        #endregion
    }
}
