using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actReturnAssignmentToLearners : BLBusinessActivity
    {
        #region "Private Object Data"

        #endregion

        #region "Public Properties and Methods"

        public static DependencyProperty CourseUserActivityGUIDProperty = DependencyProperty.Register("CourseUserActivityGUID", typeof(string), typeof(actReturnAssignmentToLearners));

        [DescriptionAttribute("CourseUserActivityGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string CourseUserActivityGUID
        {
            get
            {
                return ((string)(base.GetValue(actReturnAssignmentToLearners.CourseUserActivityGUIDProperty)));
            }
            set
            {
                base.SetValue(actReturnAssignmentToLearners.CourseUserActivityGUIDProperty, value);
            }
        }
        


        #endregion

        #region "DataLayer Access"

        /// <summary>
        /// 
        /// </summary>
        public override void BLExecuteCommand()
        {/*
            try
            {
                clsCourse_User_Activity cua = clsCourse_User_Activity.TryGetObjectByGUID(CourseUserActivityGUID, null);
                cua.ReturnToLearners();
            }
            catch (Exception e)
            {
                throw new Exception("Failed Returning Assigment with GUID: " + CourseUserActivityGUID, e);
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Test()
        {
            /*
            clsCourse_User_Activity cua = clsCourse_User_Activity.TryGetObjectByGUID(CourseUserActivityGUID, null);

            Adapters.clsSLKLearnerAssignmentAdapter laAdapter = new Axelerate.SlkCourseManagerLogicalLayer.Adapters.clsSLKLearnerAssignmentAdapter();
            Adapters.dtsGragingProperties gps = laAdapter.GetGragingProperties(cua.DetailObject.SLKGUID, cua.MasterObject.UserGUID);
            Adapters.dtsGragingProperties.GradingPropertiesRow gpRow = (Adapters.dtsGragingProperties.GradingPropertiesRow)gps.GradingProperties.Rows[0];
            
            if(gpRow.Status.CompareTo("4")!=0){
                throw new Exception("Status of activity with GUID: " + CourseUserActivityGUID + " was not set to completed on SLK database.");  
            }
            
            if (cua.AssignedActivityStatusGUID.CompareTo("4") != 0){
                throw new Exception("Status of activity with GUID: " + CourseUserActivityGUID + " was not set to completed on Course Manager database. The activity may or may not have been returned correctly. Check SLK Database.");  
            }*/
        }
        #endregion
    }
}
