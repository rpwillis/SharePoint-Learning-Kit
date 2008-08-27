using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using Axelerate.SlkCourseManagerLogicalLayer.Adapters;

namespace Axelerate.SlkCourseManagerLogicalLayer
{

    /// <summary>
    /// This class represents an Activity Assigned to the user.
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsUser_Assignment : GUIDTemplate<clsUser_Assignment>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        //private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsCourse_User_Activity), "Courses_Users_Activities", "_cua", false, false, String.Empty);
        private static clsAdapterDataLayer m_DataLayer = new clsAdapterDataLayer(typeof(clsUser_Assignment), typeof(clsSLKLearnerAssignmentAdapter).AssemblyQualifiedName , "GetLearnerAssignments", "_cua");
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
        /// Defines the Attribute GadedPoints.
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private double m_GadedPoints = 0;  

        /// <summary>
        /// Defines the Attribute Final Points.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false), DoubleNullable()]
        private double m_FinalPoints = -1;

        /// <summary>
        /// Defines the Attribute GadedPoints.
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_InstructorComments = "";

        /// <summary>
        /// Defines the Attribute LearnerGUID.
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_LearnerGUID = "";

        /// <summary>
        /// Defines the Attribute Learner.
        /// </summary>
        [FieldMap(false, false, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private clsUser m_Learner = null;
        
        /// <summary>
        /// Defines the Attribute PointsPossible.
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private double m_PointsPossible = 0; 
        
        /// <summary>
        /// Defines the Attribute AssignedActivityStatusGUID. 
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "AssignedActivityStatus", false)]
        private string m_AssignedActivityStatusGUID = "";

        /// <summary>
        /// Defines a Class AssignedActivityStatus
        /// </summary>
        [CachedForeignObject("AssignedActivityStatus", typeof(clsAssignedActivityStatus), "AssignedActivityStatusGUID_cua", "GUID_aas", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsAssignedActivityStatus m_AssignedActivityStatus = null;
        
        /// <summary>
        /// Defines the Attribute AssignmentGUID.
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_AssignmentGUID = "";

        /// <summary>
        /// Defines the Attribute Assignment.
        /// </summary>
        [FieldMap(false, false, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private clsSLKAssignment m_Assignment = null;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute GadedPoints
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public double GradePoints
        {
            get
            {
                return m_GadedPoints;
            }
        }                
             
        /// <summary>
        /// Gets a String with the Final Score. If the Activity hasn't been submited or graded it returns a " " value.
        /// </summary>
        [StringLengthValidation(5), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string FinalPointsToString
        {
            get 
            {             
                if (this.AssignedActivityStatusGUID.CompareTo(clsAssignedActivityStatus.PendingSubmitStatus.GUID) == 0)
                {
                    return " ";
                }
                else
                {
                    if (m_FinalPoints == -1)
                    {
                        return "";
                    }
                    else
                    {
                        return Math.Round(m_FinalPoints, 2).ToString();
                    }
                }                
            }
            set 
            {
                double val;
                if (value.CompareTo("") == 0)
                {
                    m_FinalPoints = -1;
                }
                else
                {
                    if (double.TryParse(value, out val))
                    {
                        if (val < 0)
                        {
                            throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strGrade));
                        }
                        m_FinalPoints = val;
                    }
                    else
                    {
                        throw new Exception(String.Format(Resources.ErrorMessages.InvalidFieldError, Resources.Messages.strGrade));
                    }
                }
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute FinalPoints as double
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public double FinalPoints 
        {
            get { return m_FinalPoints; }            
        }

        /// <summary>
        /// Gets or sets the value of the attribute InstructorComments
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string InstructorComments 
        {
            get { return m_InstructorComments; }            
        }
       
        /// <summary>
        /// Gets or sets the value of the attribute LearnerGUID
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string LearnerGUID 
        {
            get { return m_LearnerGUID; }            
        }

        /// <summary>
        /// Gets or sets the value of the attribute Learner
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsUser Learner
        {
            get
            {
                if (m_LearnerGUID.CompareTo("") != 0)
                {
                    if (m_Learner == null)
                    {
                        m_Learner = clsUser.GetUser(m_LearnerGUID); ;
                    }

                }
                return m_Learner;
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute PointsPossible
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public double PointsPossible 
        {
            get { return m_PointsPossible; }            
        }               

        /// <summary>
        /// Gets or sets the value of the attribute AssignedActivityStatusGUID
        /// </summary>
        [StringLengthValidation(50), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string AssignedActivityStatusGUID
        {
            get{ return m_AssignedActivityStatusGUID; }            
        }

        /// <summary>
        /// Gets or sets the correct Guid to the class ActivityStatus
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsAssignedActivityStatus AssignedActivityStatus
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsAssignedActivityStatus>.GetProperty(this, ref m_AssignedActivityStatus, m_AssignedActivityStatusGUID);
            }            
        }

        /// <summary>
        /// Gets or sets the value of the attribute AssignmentGUID
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string AssignmentGUID
        {
            get { return m_AssignmentGUID; }
        }

        /// <summary>
        /// Gets or sets the value of the attribute AssignmentGUID
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsSLKAssignment Assignment
        {
            get
            {
                if (m_AssignmentGUID.CompareTo("") != 0)
                {
                    if (m_Assignment == null)
                    {
                        m_Assignment = clsSLKAssignment.GetAssignment(m_AssignmentGUID);
                    }
                }
                return m_Assignment;
            }                
        }


        #endregion
                
        #region Other
        /// <summary>
        /// Return the activity to the Assigned Learner
        /// </summary>
        public void ReturnToLearner() {
            try
            {
                if (this.AssignedActivityStatusGUID.CompareTo(clsAssignedActivityStatus.SittingOnInboxStatus.GUID) == 0 || this.AssignedActivityStatusGUID.CompareTo(clsAssignedActivityStatus.ReadyToReturnStatus.GUID) == 0)
                {
                    this.m_AssignedActivityStatusGUID = "4";
                    clsSLKLearnerAssignmentAdapter laAdapter = new clsSLKLearnerAssignmentAdapter();
                    laAdapter.ReturnToLearner(this.m_GUID);
                }
            }
            catch (Exception e)
            {
                throw new Exception(Resources.ErrorMessages.ReturnLearnerAssignmentError, e);
            }
        }

        #endregion      
    }
}
