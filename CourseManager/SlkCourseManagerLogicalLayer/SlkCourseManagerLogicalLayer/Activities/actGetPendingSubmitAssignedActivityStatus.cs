using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    class actGetPendingSubmitActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsAssignedActivityStatus m_PendingSubmitStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsAssignedActivityStatus PendingSubmitStatus
        {
            get
            {
                return m_PendingSubmitStatus;
            }
        }

        #endregion

        #region "DataLayer Access"
        
        public override void BLExecuteCommand()
        {
            try
            {
                m_PendingSubmitStatus = clsAssignedActivityStatus.PendingSubmitStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Pending Submit Status.", e);
            }
        }

        public override void Test()
        {
            if (m_PendingSubmitStatus == null)
            {
                throw new Exception("Retrieved data for Pending Submit status is null.");
            }
            if (m_PendingSubmitStatus.GUID.CompareTo("1") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Pending Submit Status.");
            }
        }
        #endregion

    }
}
