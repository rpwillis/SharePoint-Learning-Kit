using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.ContinuousIntegration
{
    /// <summary>
    /// Represent the different Metadata Tables in the MIP Portal.  It inherits two base properties from GUIDNameBusinessTemplate, a GUID property that 
    /// identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsMetadataTable", "clsMetadataTables", "MIPCustom")]
    public class clsMetadataTable : IDNameBusinessTemplate<clsMetadataTable>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsMetadataTable), "MetadataTables", "_mdt", false, false, "Shared");

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

        #region "Shared Properties and Methods"

        #endregion
    }
}