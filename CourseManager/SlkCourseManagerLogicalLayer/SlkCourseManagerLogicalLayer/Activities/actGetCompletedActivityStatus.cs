using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetCompletedActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityStatus m_CompletedStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityStatus CompletedStatus
        {
            get
            {
                return m_CompletedStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_CompletedStatus = clsActivityStatus.CompletedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Completed Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_CompletedStatus == null)
            {
                throw new Exception("Retrieved data for Completed status is null.");
            }
            if (m_CompletedStatus.GUID.CompareTo("3") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Completed Status.");
            }
        }
        #endregion
    }
}
