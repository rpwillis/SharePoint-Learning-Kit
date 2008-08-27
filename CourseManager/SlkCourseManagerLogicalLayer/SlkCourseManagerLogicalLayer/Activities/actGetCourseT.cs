using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;

namespace Axelerate.SlkCourseManagerWorkflowLibrary
{
    public class actGetCourseT : BLBusinessActivity
    {
        #region "Private Object Data"

        /// <summary>
        /// Recovered Course from SharePoint. 
        /// </summary>
        private dtsCourses m_RecoveredCourse = null;

        #endregion

        #region "Public Properties and Methods"

        /// <summary>
        /// Gets the Recoverd Course from SharePoint. 
        /// </summary>
        public dtsCourses RecoveredCourse
        {
            get
            {
                return m_RecoveredCourse;
            }
        }

        /// <summary>
        /// Declaration of Parameter ContextWebURL 
        /// </summary>
        public static DependencyProperty ContextWebURLProperty = System.Workflow.ComponentModel.DependencyProperty.Register("ContextWebURL", typeof(string), typeof(actGetCourseT));

        /// <summary>
        /// Get and Set of Parameter ContextWebURL
        /// </summary>
        [Description("Decription")]
        [Category("Parameters")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ContextWebURL
        {
            get
            {
                return ((string)(base.GetValue(actGetCourseT.ContextWebURLProperty)));
            }
            set
            {
                base.SetValue(actGetCourseT.ContextWebURLProperty, value);
            }
        }

        #endregion

        #region "DataLayer Access"
        /// <summary>
        /// Overrides the method BLExecuteCommand of the base class. 
        /// Attempts to recover course information from SharePoint.
        /// </summary>
        public override void BLExecuteCommand()
        {/*
            try
            {
                clsSLKCourseAdapter courseAdapt = new clsSLKCourseAdapter();
                courseAdapt.ContextWebUrl = "http://pavospvm/site2/";
                courseAdapt.UseCustomSPContext = true;
                m_RecoveredCourse = courseAdapt.GetCourse();
            }
            catch (Exception e)
            {
                throw new Exception("Failed retrieving Coruse Data", e);
            }*/
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

