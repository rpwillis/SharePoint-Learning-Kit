using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetUnassignedActivityStatus : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivityStatus m_UnassignedStatus = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivityStatus UnassignedStatus
        {
            get
            {
                return m_UnassignedStatus;
            }
        }

        #endregion

        #region "DataLayer Access"

        public override void BLExecuteCommand()
        {
            try
            {
                m_UnassignedStatus = clsActivityStatus.UnassignedStatus;
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Unassigned Start Status.", e);
            }
        }

        public override void Test()
        {
            if (m_UnassignedStatus == null)
            {
                throw new Exception("Retrieved data for Unassigned status is null.");
            }
            if (m_UnassignedStatus.GUID.CompareTo("1") != 0)
            {
                throw new Exception("Wrong data obtained when tring to retrieve Unassigned Status.");
            }
        }
        #endregion
    }
}
