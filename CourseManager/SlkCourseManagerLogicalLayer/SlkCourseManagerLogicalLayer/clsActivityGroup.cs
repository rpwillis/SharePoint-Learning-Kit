
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents an Activity Group on the Course Manager
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivityGroup : GUIDNameBusinessTemplate<clsActivityGroup>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsActivityGroup), "ActivityGroups", "_agr", false, false, "SLKCM");

        /// <summary>
        /// Gets or sets the DataLayer value
        /// </summary>
        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Outcomes
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Outcome = "";

        /// <summary>
        /// Week From
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_WeekFrom = 0;


        /// <summary>
        /// Week To
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_WeekTo = 0;

        /// <summary>
        /// Course GUID
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_CourseGUID = "";

        /// <summary>
        /// Course Object based on the Course GUID
        /// </summary>        
        private clsCourse m_Course = null;

        /// <summary>
        /// Priority
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_Priority = 0;

        /// <summary>
        /// Activities
        /// </summary>
        [ListFieldMap(false, true, true)]
        private clsActivities m_Activities = null;

        /// <summary>
        /// Activity Group's Current Status GUID
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ActivityGroupStatus", false)]
        private string m_ActivityGroupStatusGUID = "1";

        /// <summary>
        /// Activity Group's Current Status based on the Activity Group's Current Status GUID
        /// </summary>
        [CachedForeignObject("ActivityGroupStatus", typeof(clsUser), "ActivityGroupStatusGUID_agr", "GUID_ags", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsActivityGroupStatus m_ActivityGroupStatus = null;


        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute ActivityGroupStatusGUID 
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string ActivityGroupStatusGUID
        {
            get { return m_ActivityGroupStatusGUID; }
            set
            {
                m_ActivityGroupStatusGUID = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value for ActivityGroupStatus
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsActivityGroupStatus ActivityGroupStatus
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsActivityGroupStatus>.GetProperty(this, ref m_ActivityGroupStatus, m_ActivityGroupStatusGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsActivityGroupStatus>.SetProperty(ref m_ActivityGroupStatus, value, ref m_ActivityGroupStatusGUID, true);
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Outcome 
        /// </summary>
        [StringLengthValidation(255), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Outcome
        {
            get { return m_Outcome; }
            set
            {
                m_Outcome = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute WeekFrom
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public int WeekFrom
        {
            get { return m_WeekFrom; }
            set
            {
                int val = value;

                if (val < 0)
                {
                    throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strStartWeek));
                }
                m_WeekFrom = val;

                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute WeekTo
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public int WeekTo
        {
            get { return m_WeekTo; }
            set
            {
                int val = value;

                if (val < 0)
                {
                    throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strEndWeek));
                }
                m_WeekTo = val;

                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute CourseGUID 
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string CourseGUID
        {
            get { return m_CourseGUID; }
            set
            {
                m_CourseGUID = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Course Property
        /// </summary>        
        public clsCourse Course
        {
            get
            {
                if (m_Course == null)
                {
                    m_Course = clsCourse.GetCourse();
                }
                return m_Course;
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Status 
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public int Priority
        {
            get { return m_Priority; }
            set
            {
                m_Priority = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Activities
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsActivities Activities
        {
            get
            {
                if (m_Activities == null)
                {
                    m_Activities = clsActivities.GetCollectionByActivityGroupGUID(m_GUID);
                }
                return m_Activities;
            }
        }

        #endregion

        #region "Other Methods"
        /// <summary>
        /// Increases the Priority of the Activity Group (not used)
        /// </summary>
        public void IncreasePriority()
        {
            int newPriority = m_Priority - 1;
            clsActivityGroup newObj = clsActivityGroup.TryGetObjectByGUID(this.GUID, null);

            BLCriteria Criteria = new BLCriteria(typeof(clsActivityGroup));
            Criteria.AddBinaryExpression("Priority_agr", "Priority", "=", newPriority, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddBinaryExpression("CourseGUID_agr", "CourseGUID", "=", CourseGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
            clsActivityGroup oldObj = clsActivityGroup.TryGetObject(Criteria);


            if (oldObj != null && newObj != null)
            {
                int temppri = oldObj.Priority;
                oldObj.Priority = newObj.Priority;
                newObj.Priority = temppri;
                Priority = temppri;
                newObj = newObj.Save();
                oldObj = oldObj.Save();
                clsActivityGroup.ReIndexPriorities(this.Course.GUID);
            }
        }

        /// <summary>
        /// Decreases the Priority of the Activity Group (not used)
        /// </summary>
        public void DecreasePriority()
        {
            int newPriority = m_Priority + 1;
            clsActivityGroup newObj = clsActivityGroup.TryGetObjectByGUID(this.GUID, null);

            BLCriteria Criteria = new BLCriteria(typeof(clsActivityGroup));
            Criteria.AddBinaryExpression("Priority_agr", "Priority", "=", newPriority, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
            Criteria.AddBinaryExpression("CourseGUID_agr", "CourseGUID", "=", CourseGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorAnd);
            clsActivityGroup oldObj = clsActivityGroup.TryGetObject(Criteria);


            if (oldObj != null && newObj != null)
            {
                int temppri = oldObj.Priority;
                oldObj.Priority = newObj.Priority;
                newObj.Priority = temppri;
                Priority = temppri;
                newObj = newObj.Save();
                oldObj = oldObj.Save();
                clsActivityGroup.ReIndexPriorities(this.Course.GUID);
            }
        }

        /// <summary>
        /// Performs a Reindexing to the Activity Groups' priorities
        /// </summary>
        /// <param name="CourseGUID"></param>
        public static void ReIndexPriorities(string CourseGUID)
        {
            DataLayerParameter[] Parameters = new DataLayerParameter[1];
            Parameters[0] = new DataLayerParameter("p_CourseGUID", CourseGUID);
            BLCommandBase<SQLDataLayer> Command = new BLCommandBase<SQLDataLayer>("SP_ReIndexActivityGroupPriority", Parameters, "Shared");
            Command.Execute();
        }
        #endregion

        #region overrides
        /// <summary>
        /// Deletes the current BO instance.
        /// </summary>
        /// <param name="ParentObject"></param>
        public override void BLDelete(BLBusinessBase ParentObject)
        {
            if (clsActivities.GetFullCollectionByActivityGroupGUID(this.GUID).Count != 0)
            {
                throw new Exception(Resources.ErrorMessages.NotEmptyActivityGroupDeleteError);
            }
            base.BLDelete(ParentObject);
        }
        #endregion
    }
}
