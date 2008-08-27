using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actCreateActivityOnCurrentSite : BLBusinessActivity
    {
        #region "Private Object Data"

        #endregion

        #region "Public Properties and Methods"    

        public static DependencyProperty ActivityGUIDProperty = DependencyProperty.Register("ActivityGUID", typeof(string), typeof(actCreateActivityOnCurrentSite));

        [DescriptionAttribute("ActivityGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string ActivityGUID
        {
            get
            {
                return ((string)(base.GetValue(actCreateActivityOnCurrentSite.ActivityGUIDProperty)));
            }
            set
            {
                base.SetValue(actCreateActivityOnCurrentSite.ActivityGUIDProperty, value);
            }
        }      

        #endregion
        
        #region "DataLayer Access"
        
        /// <summary>
        /// 
        /// </summary>
        public override void BLExecuteCommand()
        {
            try
            {
                clsActivity act = clsActivity.TryGetObjectByGUID(ActivityGUID, null);
                act.Assign();
            }
            catch (Exception e)
            {
                throw new Exception("Failed Assigning the Activity with GUID: " + ActivityGUID,e);
            }

        }         

        /// <summary>
        /// 
        /// </summary>
        public override void Test()
        {/*
            Adapters.clsSLKAssignmentAdapter adapter = new Axelerate.SlkCourseManagerLogicalLayer.Adapters.clsSLKAssignmentAdapter();
            Adapters.dtsAssignment assignmentDataSet = adapter.GetAssignmentbyGUID(ActivityGUID);

            if (assignmentDataSet == null)
            {
                throw new Exception("Assignment for activity with GUID: " + ActivityGUID + " could not be retrieved from SLK.");
            }
            else 
            {
                Adapters.dtsAssignment.AssignmentPropertiesRow assignment = (Adapters.dtsAssignment.AssignmentPropertiesRow)assignmentDataSet.AssignmentProperties.Rows[0];
                if (assignment.AssignmentId.CompareTo(ActivityGUID) != 0) 
                {
                    throw new Exception("Assignment for activity with GUID: " + ActivityGUID+" could not be retrieved from SLK.");
                }
            } */       
        }
        
        #endregion

    }
}
