using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;


namespace Axelerate.SlkCourseManagerLogicalLayer
{

    /// <summary>
    /// This class represents a collection of Assigned Activity Status
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsAssignedActivityStatuses : BLListBase<clsAssignedActivityStatuses, clsAssignedActivityStatus, clsAssignedActivityStatus.BOGUIDDataKey>
    {
        #region "Factory Methods"
        #endregion

        #region "ExtendedFilters"
        #endregion
    }
}