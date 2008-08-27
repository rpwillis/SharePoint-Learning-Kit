using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.ContinuousIntegration
{
    [Serializable()]
    public class clsMetadataTables : BLListBase<clsMetadataTables, clsMetadataTable, clsMetadataTable.BOAutoNumericIDDataKey>
    {
    }
}