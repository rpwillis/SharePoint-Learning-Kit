using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Contacts
{
    /// <summary>
    /// Relates a Contact with a DistributionList, defining a M to N relationship between them.  
    /// The inherited template implements a MasterObject which defines the DistributionList, a DetailObject which defines
    /// the related Contact, and a GUID that identifies the relation instance.
    /// </summary>
    [Serializable(), SecurityToken("clsDistributionList_Contact", "clsDistributionList_Contacts", "MIPCustom")]
    public class clsDistributionList_Contact : MNGUIDRelationBusinessTemplate<clsDistributionList_Contact, clsDistributionList, clsContact>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsDistributionList_Contact), "DistributionLists_Contacts", "_dlc", false, false, "Shared");

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


    }
}