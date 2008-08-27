using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{

    /// <summary>
    /// Adapter for SLK Courses with the Axelerate Framework
    /// </summary>
    public class clsSLKCourseAdapter : clsAdapterBase 
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public clsSLKCourseAdapter() : base()
        {
        }

        #endregion

        #region ApdaterBase
        /// <summary>
        /// Gets the Adapter's Type
        /// </summary>
        public override clsAdapterBase.AdapterCapabilities AdapterType
        {
            get
            {
                return clsAdapterBase.AdapterCapabilities.AdapterReadOnly;
            }
        }
        /// <summary>
        /// Transform Data
        /// </summary>
        /// <param name="FieldIndex">Field Index</param>
        /// <param name="Row">Data Row</param>
        /// <returns></returns>
        public override object DataTransform(int FieldIndex, System.Data.DataRow Row)
        {
            switch (FieldIndex)
            {
                case 0:
                    //GUID
                    return (string)Row[1];
                case 1:
                    //Name
                    return (string)Row[0];
            }
            return null;

        } 
        #endregion
              
        #region GetCourse

        /// <summary>
        /// Gets current Course data from SLK.
        /// </summary>
        public dtsCourses GetCourse()
        {            
            try
            {
                dtsCourses course = new dtsCourses();
                dtsCourses.tblCoursesRow row = course.tblCourses.NewtblCoursesRow(); 

                row.GUID = SPContext.Current.Web.ID.ToString(); 
                row.Name = SPContext.Current.Web.Name;

                course.tblCourses.Rows.Add(row);

                return course;
            }catch(Exception e){
                throw new Exception(Resources.ErrorMessages.GetCourseDataError);
            }
            
        }

        /// <summary>
        /// Gets a Course data from SLK
        /// </summary>
        /// <param name="WebURL">URL of the Web to be retrieved</param>
        public dtsCourses GetCourse(string WebURL)
        {            
            try
            {
                dtsCourses course = new dtsCourses();
                
                dtsCourses.tblCoursesRow row = course.tblCourses.NewtblCoursesRow();

                SPSite site = new SPSite(WebURL);
                SPWeb web = site.OpenWeb();
                                
                row.GUID = web.ID.ToString();
                row.Name = web.Name;

                course.tblCourses.Rows.Add(row);

                site.Dispose();

                return course;
            }
            catch (Exception e)
            {                
                throw new Exception(Resources.ErrorMessages.GetCourseDataError);
            }           
        }

        #endregion

        #region Other Methods
        /// <summary>
        /// Gets the GUID for the current Course (SPWeb).
        /// </summary>
        /// <returns></returns>
        public static string getCourseGUID()
        {
            return SPContext.Current.Web.ID.ToString();
        }

        #endregion
    }
}
