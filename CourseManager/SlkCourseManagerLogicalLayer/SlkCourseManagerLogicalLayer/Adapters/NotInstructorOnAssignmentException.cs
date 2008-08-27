using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// Exception thrown when there are no Instructors on an Assignment. 
    /// </summary>
    public class NotInstructorOnAssignmentException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public NotInstructorOnAssignmentException():base()
        { 
        }
        /// <summary>
        /// Constructor with an specific error message.
        /// </summary>
        /// <param name="errorMessage"></param>
        public NotInstructorOnAssignmentException(string errorMessage): base(errorMessage)
        {
        }
        
        /// <summary>
        /// Constructor with an specific error message and an inner exception
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="innerEx"></param>
        public NotInstructorOnAssignmentException (string errorMessage, Exception innerEx): base(errorMessage, innerEx) 
        {
        }
        
    }
}
