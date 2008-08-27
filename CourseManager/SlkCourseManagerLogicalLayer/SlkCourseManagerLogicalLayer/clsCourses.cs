using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;

namespace Axelerate.SlkCourseManagerLogicalLayer
{

    /// <summary>
    /// This class represents a collection of Courses
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsCourses : BLListBase<clsCourses, clsCourse, clsCourse.BOGUIDDataKey>
    {
        #region "Factory Methods"
        #endregion

        #region "ExtendedFilters"
        #endregion
    }
}
