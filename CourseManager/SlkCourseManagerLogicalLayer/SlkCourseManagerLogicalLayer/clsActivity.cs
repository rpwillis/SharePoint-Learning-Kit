
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;
using Microsoft.SharePoint;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a Learning Activity stored on the CM database.
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivity : GUIDNameBusinessTemplate<clsActivity>
    {

        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsActivity), "Activities", "_act", false, false, "SLKCM");

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
        /// Defines the Activity's Assign Date.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_AssignDate = DateTime.Now;

        /// <summary>
        /// Defines the Activity's Due Date.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_DueDate = DateTime.Now;

        /// <summary>
        /// Defines the Activity's Max Score
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_MaxScore = 0;

        /// <summary>
        /// Defines if the activity id greadable or not.
        /// Not Greadable activities are not taken into account on the final score on the Monitor and Assess web part.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private bool m_Gradeable = false;

        /// <summary>
        /// Defines the Activity's Weight.
        /// Defines how much weight will the activity have on the final score on the Monitor and Assess web part.
        /// Activities with a weight of 0 will not show on the Monitor and Assess web part.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_Weight = 0;

        /// <summary>
        /// Defines the Activity's associated SLK Assignment GUID on the SLK.
        /// Blank if the Activity hasn't been assigned.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_SLKGUID = "";

        /// <summary>
        /// Defines the Activity's Activity Group GUID.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ActivityGroup", false)]
        private string m_ActivityGroupGUID = "";

        /// <summary>
        /// Defines the Activity's Activity Group Object. 
        /// </summary>
        [CachedForeignObject("ActivityGroup", typeof(clsActivityGroup), "ActivityGroupGUID_act", "GUID_agr", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsActivityGroup m_ActivityGroup = null;

        /// <summary>
        /// Defines the Activity's Activity Status GUID.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ActivityStatus", false)]
        private string m_ActivityStatusGUID = "1";

        /// <summary>
        /// Defines the Activity's Activity Status Object.
        /// </summary>
        [CachedForeignObject("ActivityStatus", typeof(clsActivityStatus), "ActivityGroupGUID_act", "GUID_ast", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsActivityStatus m_ActivityStatus = null;
        
        /// <summary>
        /// Defines the Activity's Description
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        /// <summary>
        /// Defines the Associeted file location. 
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_FileURL = SPContext.Current.Web.Url + "/CourseManagerPages/Forms/Placeholder.txt";

        /// <summary>
        /// Defines the Activity's associated SLK Assignment object on the SLK.
        /// Blank if the Activity hasn't been assigned.
        /// </summary>
        private clsSLKAssignment m_SLKAssignment = null;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets the value of the attribute AssignDate as DateTime
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public DateTime AssignDateTime
        {
            get { return m_AssignDate.Date; }
        }

        /// <summary>
        /// Gets or sets the value of the attribute AssignDate
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public DateTime AssignDate
        {
            get { return m_AssignDate; }
            set
            {
                m_AssignDate = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets the DueDate in a String Format.
        /// </summary>
        public string DueDateToString
        {
            get
            {
                if (m_DueDate.CompareTo(new DateTime(1973, 1, 1)) == 0)
                {
                    return "";
                }
                return m_DueDate.ToShortDateString();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute DueDate
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public DateTime DueDate
        {
            get { return m_DueDate; }
            set
            {
                m_DueDate = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute MaxScore
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public int MaxScore
        {
            get { return m_MaxScore; }
            set
            {
                int val = value;

                if (val < 0)
                {
                    throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strMaxScore));
                }
                m_MaxScore = val;

                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Gradeable
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public bool Gradeable
        {
            get { return m_Gradeable; }
            set
            {
                m_Gradeable = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Weight
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public int Weight
        {
            get { return m_Weight; }
            set
            {
                int val = value;

                if (val < 0)
                {
                    throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strWeight));
                }
                m_Weight = val;

                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute ActivityGroupGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string SLKGUID
        {
            get { return m_SLKGUID; }
            set
            {
                m_SLKGUID = value;
                PropertyHasChanged();
            }
        }
        /// <summary>
        /// Gets or sets the value of the attribute ActivityGroupGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string ActivityGroupGUID
        {
            get { return m_ActivityGroupGUID; }
            set
            {
                m_ActivityGroupGUID = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the correct Guid to the class ActivityGroup
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsActivityGroup ActivityGroup
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsActivityGroup>.GetProperty(this, ref m_ActivityGroup, m_ActivityGroupGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsActivityGroup>.SetProperty(ref m_ActivityGroup, value, ref m_ActivityGroupGUID, true);
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Gets or sets the value of the attribute ActivityStatusGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string ActivityStatusGUID
        {
            get { return m_ActivityStatusGUID; }
            set
            {
                m_ActivityStatusGUID = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the correct Guid to the class ActivityStatus
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsActivityStatus ActivityStatus
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsActivityStatus>.GetProperty(this, ref m_ActivityStatus, m_ActivityStatusGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsActivityStatus>.SetProperty(ref m_ActivityStatus, value, ref m_ActivityStatusGUID, true);
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute Description
        /// </summary>
        [StringLengthValidation(255), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Gets or sets the value of the attribute FileURL
        /// </summary>
        [StringLengthValidation(255), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string FileURL
        {
            get
            {
                if (m_FileURL == "")
                {
                    m_FileURL = SPContext.Current.Web.Url + "/CourseManagerPages/Forms/Placeholder.txt";
                }
                return m_FileURL;
            }
            set
            {
                if (!this.IsNew)
                {
                    if (this.ActivityStatusGUID.CompareTo(clsActivityStatus.AssignedStatus.GUID) == 0)
                    {
                        throw new Exception(Resources.ErrorMessages.AssignedActivityResourceChangeError);
                    }
                }
                m_FileURL = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets the associated File Name for this activy.
        /// </summary>
        public string FileName
        {
            get { return System.IO.Path.GetFileName(m_FileURL); }
        }

        /// <summary>
        /// Gets the Associated SLK Assignment for this activity.
        /// </summary>
        public clsSLKAssignment SLKAssignment
        {
            get
            {
                if (this.SLKGUID != "")
                {
                    if (m_SLKAssignment == null)
                    {
                        m_SLKAssignment = clsSLKAssignment.GetAssignment(this.SLKGUID);
                    }
                }
                return m_SLKAssignment;
            }
        }

        #endregion

        #region Overrides
        /// <summary>
        /// Deletes the activity (current instance of the clsActivity object).
        /// </summary>
        /// <param name="ParentObject">Parent Object</param>
        public override void BLDelete(BLBusinessBase ParentObject)
        {
            if (this.ActivityStatusGUID.CompareTo(clsActivityStatus.AssignedStatus.GUID) == 0)
            {
                throw new Exception(Resources.ErrorMessages.AssignedActivityDeleteError);
            }
            base.BLDelete(ParentObject);
        }

        /// <summary>
        /// Inserts the current instance as a new activity.
        /// </summary>
        /// <param name="ParentObject"></param>
        public override void BLInsert(BLBusinessBase ParentObject)
        {
            if (this.DueDate.CompareTo(this.AssignDate) < 0)
            {
                throw new Exception(Resources.ErrorMessages.InvalidDueDateError);
            }
            if (this.MaxScore == 0)
            {
                this.Gradeable = false;
            }
            base.BLInsert(ParentObject);
        }

        /// <summary>
        /// Saves the current instance of clsActivity.
        /// </summary>
        /// <returns></returns>
        public override clsActivity Save()
        {
            if (this.DueDate.CompareTo(this.AssignDate) < 0)
            {
                throw new Exception(Resources.ErrorMessages.InvalidDueDateError);
            }
            if (this.MaxScore == 0)
            {
                this.Gradeable = false;
            }
            return base.Save();
        }

        /// <summary>
        /// Updates the current instance of clsActivity.
        /// </summary>
        /// <param name="ParentObject"></param>
        public override void BLUpdate(BLBusinessBase ParentObject)
        {
            if (this.DueDate.CompareTo(this.AssignDate) < 0 && this.DueDate.CompareTo(new DateTime(1973, 1, 1)) != 0)
            {
                throw new Exception(Resources.ErrorMessages.InvalidDueDateError);
            }
            if (this.MaxScore == 0)
            {
                this.Gradeable = false;
            }
            if (this.SLKAssignment != null)
            {
                if (this.m_AssignDate.CompareTo(this.SLKAssignment.StartDate) != 0)
                {
                    this.SLKAssignment.StartDate = AssignDate;
                }
                if (this.m_DueDate.CompareTo(this.SLKAssignment.DueDate) != 0)
                {
                    this.SLKAssignment.DueDate = this.DueDate;
                }
                if (this.Description.CompareTo(this.SLKAssignment.Description) != 0)
                {
                    this.SLKAssignment.Description = this.Description;
                }
                if (this.Name.CompareTo(this.SLKAssignment.Name) != 0)
                {
                    this.SLKAssignment.Name = this.Name;
                }
                if (this.MaxScore != (int)this.SLKAssignment.PointsPossible)
                {
                    this.SLKAssignment.PointsPossible = (double)this.MaxScore;
                }
                this.m_SLKAssignment = this.SLKAssignment.Save();
            }
            base.BLUpdate(ParentObject);
        }

        #endregion

        #region Assgin-UnAssign
        /// <summary>
        /// Assigns an activity to all Learners on the current Course (Creates the activity on the SLK).
        /// </summary>
        public void Assign()
        {
            try
            {
                clsActivity act = clsActivity.TryGetObjectByGUID(this.GUID, null);

                if (act.ActivityStatusGUID.CompareTo(clsActivityStatus.AssignedStatus.GUID) != 0)
                {
                    clsSLKAssignment assignment = new clsSLKAssignment();
                    assignment.Name = this.Name;
                    assignment.PointsPossible = this.MaxScore;
                    assignment.DueDate = this.DueDate;
                    assignment.StartDate = this.AssignDate;

                    //If it's a one day activity, set the assign date's time to: 12:00:00am (begin of day) and Due Date's time to: 11:59:59pm (end of day)
                    if (AssignDate.CompareTo(DueDate) == 0)
                    {
                        assignment.StartDate = this.AssignDate.Subtract(new TimeSpan(this.AssignDate.Hour, this.AssignDate.Minute, this.AssignDate.Second));
                        assignment.DueDate = this.DueDate.Add(new TimeSpan(23 - this.AssignDate.Hour, 59 - this.AssignDate.Minute, 59 - this.AssignDate.Second));
                    }
                    assignment.Description = this.Description;
                    assignment.PackageLocation = this.FileURL;

                    assignment = assignment.Save();


                    act.SLKGUID = assignment.GUID;
                    act.AssignDate = assignment.StartDate;
                    act.DueDate = assignment.DueDate;
                    act.ActivityStatusGUID = clsActivityStatus.AssignedStatus.GUID;
                    act = act.Save();
                    this.SLKGUID = assignment.GUID;
                    this.ActivityStatusGUID = clsActivityStatus.AssignedStatus.GUID;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        /// <summary>
        /// Unassigns an activity from all Learners on the current Course (Deletes the assigned activities on the SLK).
        /// </summary>
        public void Unassign()
        {
            try
            {
                clsActivity act = clsActivity.TryGetObjectByGUID(this.GUID, null);

                if (act.ActivityStatusGUID.CompareTo(clsActivityStatus.UnassignedStatus.GUID) != 0)
                {
                    BLCriteria criteria = new BLCriteria(typeof(clsSLKAssignments));
                    criteria.AddBinaryExpression("GUID_ass", "GUID", "=", this.SLKGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                    clsSLKAssignments asses = clsSLKAssignments.GetCollection(criteria);
                    if (asses.Count == 0)
                    {
                        throw new Exception(Resources.ErrorMessages.NoPermissionToUnassignError + " " + this.Name);
                    }
                    foreach (clsSLKAssignment assignment in asses)
                    {
                        assignment.Delete();
                        assignment.Save();
                    }
                    asses.Clear();
                    asses = asses.Save();

                    act.ActivityStatus = clsActivityStatus.UnassignedStatus;
                    act.SLKGUID = "";
                    act = act.Save();

                    this.ActivityStatus = clsActivityStatus.UnassignedStatus;
                    this.SLKGUID = "";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region CustomMethods
        /// <summary>
        /// Updates the Course Manager activities on any information changes done in the SLK Activities.
        /// </summary>
        public void UpdateFromSLK()
        {
            if (this.ActivityStatusGUID.CompareTo(clsActivityStatus.AssignedStatus.GUID) == 0 && this.SLKAssignment != null)
             {
                if (this.m_AssignDate.CompareTo(this.SLKAssignment.StartDate) != 0)
                {
                    this.AssignDate = this.SLKAssignment.StartDate;
                }
                if (this.m_DueDate.CompareTo(this.SLKAssignment.DueDate) != 0)
                {
                    this.DueDate = this.SLKAssignment.DueDate;
                }
                if (this.Description.CompareTo(this.SLKAssignment.Description) != 0)
                {
                    this.Description = this.SLKAssignment.Description;
                }
                if (this.Name.CompareTo(this.SLKAssignment.Name) != 0)
                {
                    this.Name = this.SLKAssignment.Name;
                }
                if (this.MaxScore != (int)this.SLKAssignment.PointsPossible)
                {
                    this.MaxScore = (int)this.SLKAssignment.PointsPossible;
                }
            }
        }

        #endregion
    }
}
