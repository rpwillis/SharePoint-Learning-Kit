using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetStartedActivityGroupStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityGroupStatus m_StartedStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityGroupStatus StartedStatus
        {
            get
            {
                return m_StartedStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_StartedStatus = clsActivityGroupStatus.StartedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Started Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_StartedStatus == null)
            {
                throw new Exception("Retrieved data for Started status is null.");
            }
            if (m_StartedStatus.GUID.CompareTo("2") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Started Status.");
            }
        }
        #endregion
    }
}
