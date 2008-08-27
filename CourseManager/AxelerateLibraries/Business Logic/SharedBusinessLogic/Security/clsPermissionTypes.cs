using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Security
{
    [Serializable()]
    public class clsPermissionTypes : MNGUIDDetailsCollectionTemplate<clsPermissionTypes, clsPermission, clsBaseRole, clsPermissionType>
    {
    }
}