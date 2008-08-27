using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetCurrentCourse : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Recovered Course from SharePoint. 
        /// </summary>
        private clsCourse m_RecoveredCourse = null;

        #endregion

        #region "Public Properties and Methods"
        /// <summary>
        /// Gets the Recoverd Course from SharePoint. 
        /// </summary>
        public clsCourse RecoveredCourse
        {
            get
            {
                return m_RecoveredCourse;
            }
        }
        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Attempts to recover course information from SharePoint.
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                m_RecoveredCourse = clsCourse.GetCourse();
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Coruse Data", e);
            }
        }

        /// <summary>
        /// This method verifies the course information retrieved from SharePoint.
        /// </summary>
        public override void Test()
        {
            if (m_RecoveredCourse == null)
            {
                throw new Exception("No Course Data Retrieved");
            }
        }
        #endregion

    }
}
