using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actReturnToLearners : BLBusinessActivity
    {
        #region "Private Object Data"
        
        #endregion

        #region "Public Properties and Methods"

        public static DependencyProperty CourseUserActivityGUIDProperty = DependencyProperty.Register("CourseUserActivityGUID", typeof(string), typeof(actReturnToLearners));

        [DescriptionAttribute("CourseUserActivityGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string CourseUserActivityGUID
        {
            get
            {
                return ((string)(base.GetValue(actReturnToLearners.CourseUserActivityGUIDProperty)));
            }
            set
            {
                base.SetValue(actReturnToLearners.CourseUserActivityGUIDProperty, value);
            }
        }

        #endregion

        #region "DataLayer Access"

        /// <summary>
        /// 
        /// </summary>
        public override void BLExecuteCommand()
        {
            /*try
            {
                clsCourse_User_Activity cua = clsCourse_User_Activity.TryGetObjectByGUID(CourseUserActivityGUID, null);
                cua.ReturnToLearners();
            }
            catch (Exception e)
            { 
                
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Test()
        {

        }
        #endregion
    }
}
