using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;

namespace Axelerate.SlkCourseManagerLogicalLayer
{

    /// <summary>
    /// This class represents a collection of Activity Group Status
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivityGroupStatuses : BLListBase<clsActivityGroupStatuses, clsActivityGroupStatus, clsActivityGroupStatus.BOGUIDDataKey>
    {
        #region "Factory Methods"
        #endregion

        #region "ExtendedFilters"
        #endregion
    }
 }
