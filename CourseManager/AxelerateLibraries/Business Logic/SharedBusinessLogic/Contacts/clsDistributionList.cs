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
    /// Represents an Email distribution list.  It is a collection of email recipients that can be used to
    /// send mail to all of them at once. It inherits two base properties from GUIDNameBusinessTemplate, a GUID property that 
    /// identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsDistributionList", "clsDistributionLists", "MIPCustom")]
    public class clsDistributionList : GUIDNameBusinessTemplate<clsDistributionList>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsDistributionList), "DistributionLists", "_dlt", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        #endregion

        #region "Business Properties and Methods"

        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Iterates through all contacts in the list and generate a ";" separated list string that includes
        /// every contact Email Address
        /// </summary>
        /// <returns></returns>
        public string GetRecipient()
        {
            clsMNDistributionListContacts Contacts = clsMNDistributionListContacts.GetDetailsInMaster(this);
            string Recipient = "";
            foreach (clsContact Contact in Contacts)
            {
                if (Contact.Email != "")
                {
                    if (Recipient == "")
                    {
                        Recipient = Contact.Email;
                    }
                    else
                    {
                        Recipient = Recipient + "," + Contact.Email;
                    }
                }
            }
            return Recipient;
        }

        #endregion


    }
}