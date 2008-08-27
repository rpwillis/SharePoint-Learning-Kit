
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

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a Course on the Course Manager
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.Read)]
    public class clsCourse : GUIDNameBusinessTemplate<clsCourse>
    {
        #region "DataLayer Overrides"
        //private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsCourse), "Courses", "_crs", false, false, String.Empty);
        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static clsAdapterDataLayer m_DataLayer = new clsAdapterDataLayer(typeof(clsCourse), typeof(clsSLKCourseAdapter).AssemblyQualifiedName, "GetCourse","_crs");
        
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

        #endregion

        #region "Business Properties and Methods"
        
        #endregion
        
        #region "Factory Methods"
        
        /// <summary>
        /// Gets Course Information from SLK.
        /// </summary>    
        public static clsCourse GetCourse()
        {
            try
            {
                clsCourses courses = clsCourses.GetCollection();

                if (courses.Count > 0)
                {
                    return courses[0];
                }
                return null;

            }
            catch (Exception e)
            {
                throw e; //Throws error to UI to be handled there.     
            }            
        }
        
        /// <summary>
        /// Gets Course Information from SLK.
        /// </summary>    
        public static clsCourse GetCourse(string WebURL)
        {
            try
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsCourse));
                Criteria.AddPreFilter("WebURL",WebURL);
                clsCourses courses = clsCourses.GetCollection(Criteria);

                if (courses.Count > 0)
                {
                    return courses[0];
                }
                return null;
            }
            catch (Exception e)
            {
                throw e; //Throws error to UI to be handled there.     
            } 
        }
               
        #endregion
      
    }
}
