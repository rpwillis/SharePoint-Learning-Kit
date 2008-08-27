using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actGetActivityCollectionByActivityGroupGUID : BLBusinessActivity
    {
        #region "Private Object Data"

        private clsActivities m_RetrievedActivities = null;

        #endregion

        #region "Public Properties and Methods"

        public clsActivities RetrievedActivities
        {
            get
            {
                return m_RetrievedActivities;
            }
        }

        public static DependencyProperty ActivityGroupGUIDProperty = DependencyProperty.Register("ActivityGroupGUID", typeof(string), typeof(actGetActivityCollectionByActivityGroupGUID));
                       
        [DescriptionAttribute("ActivityGroupGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string ActivityGroupGUID
        {
            get
            {
                return ((string)(base.GetValue(actGetActivityCollectionByActivityGroupGUID.ActivityGroupGUIDProperty)));
            }
            set
            {
                base.SetValue(actGetActivityCollectionByActivityGroupGUID.ActivityGroupGUIDProperty, value);
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
                m_RetrievedActivities = clsActivities.GetCollectionByActivityGroupGUID(ActivityGroupGUID);
            }
            catch (Exception e)
            {
                throw new Exception("Error retrieving Activity Collection", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Test()
        {
            if (m_RetrievedActivities == null) 
            {
                throw new Exception("Empty activity collection retrieved");
            }
        }
        #endregion
    }
}
