using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actReIndexPriorities : BLBusinessActivity
    {
        #region "Private Object Data"

        #endregion

        #region "Public Properties and Methods"

        public static DependencyProperty CourseGUIDProperty = DependencyProperty.Register("CourseGUID", typeof(string), typeof(actReIndexPriorities));

        [DescriptionAttribute("CourseGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string CourseGUID
        {
            get
            {
                return ((string)(base.GetValue(actReIndexPriorities.CourseGUIDProperty)));
            }
            set
            {
                base.SetValue(actReIndexPriorities.CourseGUIDProperty, value);
            }
        }

        #endregion

        #region "DataLayer Access"


        public override void BLExecuteCommand()
        {
            try
            {
                clsActivityGroup.ReIndexPriorities(CourseGUID);                
            }
            catch (Exception e)
            {
                throw new Exception("Failed reindexing priorities.", e);
            }
        }


        public override void Test()
        {
            try
            {
                BLCriteria criteria = new BLCriteria(typeof(clsActivityGroups));
                criteria.AddOrderedField("Priority_agr", true);
                clsActivityGroups ActGrps = clsActivityGroups.GetCollectionByCourse(criteria);
                int priority = ActGrps[0].Priority;
                ActGrps.Remove(ActGrps[0]);
                foreach (clsActivityGroup grp in ActGrps)
                {
                    if (priority != grp.Priority + 1){
                        throw new Exception("The Priority Reindexing was not done correctly. Item with Priority: " + (grp.Priority+1) + "in Activity Groups for Course with GUID: " + CourseGUID + "is missing.");
                    }
                    priority = grp.Priority;
                }
            }
            catch (Exception e) 
            {
                throw new Exception("Error while testing if the Priority Reindexing was done correctly. The process may or may not have worked correctly.",e);                
            }
            

            
        }
        #endregion

    }
}
