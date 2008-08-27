using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Activities
{
    class actIncreasePriority : BLBusinessActivity
    {
        #region "Private Object Data"

        private int m_OldPriority = 0;

        private int m_NewPriority = 0;
        
        #endregion

        #region "Public Properties and Methods"

        public int OldPriority
        {
            get
            {
                return m_OldPriority;
            }
        }

        public int NewPriority
        {
            get
            {
                return m_NewPriority;
            }
        }

        public static DependencyProperty ActivityGrupoGUIDProperty = DependencyProperty.Register("ActivityGrupoGUID", typeof(string), typeof(actIncreasePriority));


        [DescriptionAttribute("ActivityGrupoGUID")]
        [CategoryAttribute("Parameters")]
        [BrowsableAttribute(true)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Visible)]
        public string ActivityGrupoGUID
        {
            get
            {
                return ((string)(base.GetValue(actIncreasePriority.ActivityGrupoGUIDProperty)));
            }
            set
            {
                base.SetValue(actIncreasePriority.ActivityGrupoGUIDProperty, value);
            }
        }

        #endregion

        #region "DataLayer Access"
        

        public override void BLExecuteCommand()
        {
            try
            {
                clsActivityGroup actGroup = clsActivityGroup.TryGetObjectByGUID(ActivityGrupoGUID, null);
                m_OldPriority = actGroup.Priority;
                actGroup.DecreasePriority();                
            }
            catch (Exception e)
            {
                throw new Exception("Failed increasing activity group priority", e);
            }
        }

    
        public override void Test()
        {
            clsActivityGroup actGroup = clsActivityGroup.TryGetObjectByGUID(ActivityGrupoGUID, null);
            m_NewPriority = actGroup.Priority;

            if (m_NewPriority == m_OldPriority+1)
            {
                throw new Exception("Activity group priority was not increased correctly.");
            }            
        }
        #endregion
    }
}
