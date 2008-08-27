using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Contacts
{
    [Serializable()]
    public class clsContacts : BLListBase<clsContacts, clsContact, clsContact.BOGUIDDataKey>
    {
    }
}
