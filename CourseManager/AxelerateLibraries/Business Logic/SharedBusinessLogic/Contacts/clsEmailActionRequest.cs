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
    /// Represents a TFS Project It inherits two base properties from DetailGUIDBussinessTemplate, 
    /// a GUID property that identifies the instance and a MasterObject that relates this server to its parent TFS Server.
    /// </summary>
    [Serializable(), SecurityToken("clsEmailActionRequest", "clsEmailActionRequests", "MIPCustom")]
    public class clsEmailActionRequest : DetailGUIDBussinessTemplate<clsEmailMessage, clsEmailActionRequest>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsEmailActionRequest), "EmailActionRequests", "_ear", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Action Request's Name
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Name = "";

        /// <summary>
        /// Full name of the object that is going to be created by reflection to execute the action request
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ObjectName = "";

        /// <summary>
        /// Name of the object's method that is going to be called by reflection to execute the action request 
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_MethodName = "";

        /// <summary>
        /// Message that will be showed to the user if the action succeds
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_SuccessMessage = "";

        /// <summary>
        /// Message that will be showed to the user if the action fails
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_FailMessage = "";

        /// <summary>
        /// Message that will be concatenated at the end of the orignal email body message.  This message will appear as  a
        /// link to the user and if clicked will execute the requested action
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Message = "";

        /// <summary>
        /// String containing a detailed description for the object.  
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ObjectDetailsMessage = "";
        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or Sets the Action Request's Name
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Action object's Full Name.
        /// </summary>
        public string ObjectName
        {
            get { return m_ObjectName; }
            set
            {
                m_ObjectName = value;
                PropertyHasChanged();
            }
        }
        /// <summary>
        /// Gets or Sets the Action's method Name.
        /// </summary>

        public string MethodName
        {
            get { return m_MethodName; }
            set
            {
                m_MethodName = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Action's success message.
        /// </summary>
        public string SuccessMessage
        {
            get { return m_SuccessMessage; }
            set
            {
                m_SuccessMessage = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Action's failure message.
        /// </summary>
        public string FailMessage
        {
            get { return m_FailMessage; }
            set
            {
                m_FailMessage = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Action's message.  This message will be concatenated at the end of the orignal email body message.  
        /// It will appear as a link to the user and if clicked will execute the requested action
        /// </summary>
        public string Message
        {
            get { return m_Message; }
            set
            {
                m_Message = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the Action object details message.  
        /// </summary>
        public string ObjectDetailsMessage
        {
            get { return m_ObjectDetailsMessage; }
            set
            {
                m_ObjectDetailsMessage = value;
                PropertyHasChanged();
            }
        }

        #endregion


    }
}