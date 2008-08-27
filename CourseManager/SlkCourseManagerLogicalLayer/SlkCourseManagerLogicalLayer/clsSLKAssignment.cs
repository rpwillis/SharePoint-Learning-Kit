
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
    /// This class represents a Assignment stored in SLK. 
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsSLKAssignment : GUIDNameBusinessTemplate<clsSLKAssignment>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static clsAdapterDataLayer m_DataLayer = new clsAdapterDataLayer(typeof(clsSLKAssignment), typeof(clsSLKAssignmentAdapter).AssemblyQualifiedName, "GetAssignments", "_ass");

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
        /// Description
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        /// <summary>
        /// Start Date
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_StartDate = new DateTime(); //date

        /// <summary>
        /// Due Date
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_DueDate = new DateTime(); //date
        
        /// <summary>
        /// Points Possible
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private double m_PointsPossible = 0; //int

        /// <summary>
        /// Course GUID
        /// </summary>
        [FieldMap(false, true, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_CourseGUID = "";

        /// <summary>
        /// Course
        /// </summary>
        [FieldMap(false, false, false, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private clsCourse m_Course = null;

        /// <summary>
        /// Package Location.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_PackageLocation = "";
        
        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute Description
        /// </summary>
        [StringLengthValidation(500), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value;
                  PropertyHasChanged();}
        }

        public string StartDateToString
        {
            get
            {
                if (m_StartDate.CompareTo(new DateTime(1973, 1, 1)) == 0)
                {
                    return "";
                }
                return m_StartDate.ToShortDateString();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute StartDate
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public DateTime StartDate 
        {
            get { return m_StartDate; }
            set
            {
                m_StartDate = value;
                PropertyHasChanged();
            }
        }

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
            get 
            {                
                return m_DueDate; 
            }
            set
            {
                m_DueDate = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute PointsPossible
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public double PointsPossible 
        {
            get { return m_PointsPossible; }
            set 
            {
                m_PointsPossible = value;
                PropertyHasChanged();
            }
        }       
               

        /// <summary>
        /// Gets or sets the value of the attribute ActivityGroupGUID
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
        /// Gets or sets the correct Guid to the class Course
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public clsCourse Course
        {           
            get
            {
                if (m_CourseGUID.CompareTo("") != 0)
                {
                    if (m_Course == null)
                    {
                        m_Course = clsCourse.GetCourse();                           
                    }

                }
                return m_Course;
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute PackageLocation
        /// </summary>
        [StringLengthValidation(500), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public string PackageLocation
        {
            get { return m_PackageLocation; }
            set
            {
                m_PackageLocation = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the attribute NameWithLink
        /// </summary>
        public string NameWithLink
        {
            get
            {
                if (this.GUID != "")
                {
                    return "<a href=\"" + SPContext.Current.Web.Url + "/_layouts/SharePointLearningKit/Grading.aspx?AssignmentId=" + this.GUID + "\">&nbsp;&nbsp;&nbsp;" + this.Name + "&nbsp;&nbsp;&nbsp;</a>";
                }
                else
                {
                    return Name;
                }
            }
        }

        #endregion
        
        #region Factory Methods
        /// <summary>
        /// Returns an Assignment with the GUID used as a parameter.
        /// </summary>
        /// <param name="AssignmentGUID">GUID of the assignment to be loaded</param>
        /// <returns></returns>
        public static clsSLKAssignment GetAssignment(string AssignmentGUID) 
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsSLKAssignments));
                Criteria.AddPreFilter("AssignmentGUID", AssignmentGUID);
                clsSLKAssignments asses = clsSLKAssignments.GetCollection(Criteria);
                if (asses.Count == 0)
                {
                    return null;
                }
                else
                {
                    return asses[0];
                }
            }
            catch (Exception e)
            {
                throw e; //throw error to next level
            }
        }
        
        #endregion
    }
}
