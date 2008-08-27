using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

using System.IO;
namespace Axelerate.BusinessLogic.SharedBusinessLogic.Trace
{
    /// <summary>
    /// Represents the generic log for the MIP Portal.  It inherits two base properties from GUIDNameBusinessTemplate, a GUID property that 
    /// identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsGenericLog", "clsGenericLogs", "MIPCustom")]
    public class clsGenericLog : GUIDTemplate<clsGenericLog>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsGenericLog), "GenericLogs", "_glo", false, false, "MIPWarehouse");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// The name of the domain for the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Domain = "";
        /// <summary>
        /// The name of the user for the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_User = "";
        /// <summary>
        /// The  module of the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Module = "";
        /// <summary>
        /// The action that represents the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Action = "";

        /// <summary>
        /// The description of the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        /// <summary>
        /// The exception of the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Exception = "";

        /// <summary>
        /// The category of the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Category = "";

        /// <summary>
        /// Additional information about the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Arguments = "";

        /// <summary>
        /// The GUID of the project that is related to the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ProjectGUID = "";

        /// <summary>
        /// The GUID of the portal that is related to the log.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_PortalGUID = "";
        /// <summary>
        /// Date of the log generation.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_EntryDate;//TODO: Csla no longer supported SmartDate


        /// <summary>
        /// The priority level of the log
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_Level;
        #endregion

        #region "Business Properties and Methods"

        [StringLengthValidation(512)]
        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                PropertyHasChanged();
            }
        }

        [StringLengthValidation(512)]
        public string Exception
        {
            get { return m_Exception; }
            set
            {
                m_Exception = value;
                PropertyHasChanged();
            }
        }

        [StringLengthValidation(512)]
        public string Action
        {
            get { return m_Action; }
            set
            {
                m_Action = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(255)]
        public string Domain
        {
            get { return m_Domain; }
            set
            {
                m_Domain = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(50)]
        public string User
        {
            get { return m_User; }
            set
            {
                m_User = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(128)]
        public string Module
        {
            get { return m_Module; }
            set
            {
                m_Module = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(255)]
        public string Category
        {
            get { return m_Category; }
            set
            {
                m_Category = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(512)]
        public string Arguments
        {
            get { return m_Arguments; }
            set
            {
                m_Arguments = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(255)]
        public string PortalGUID
        {
            get { return m_PortalGUID; }
            set
            {
                m_PortalGUID = value;
                PropertyHasChanged();
            }
        }
        [StringLengthValidation(255)]
        public string ProjectGUID
        {
            get { return m_ProjectGUID; }
            set
            {
                m_ProjectGUID = value;
                PropertyHasChanged();
            }
        }

        public DateTime EntryDate
        {
            get { return m_EntryDate.Date; }
            set
            {
                //m_EntryDate.Date = value;
                m_EntryDate = value;
                PropertyHasChanged();
            }
        }

        public int Level
        {
            get { return m_Level; }
            set
            {
                m_Level = value;
                PropertyHasChanged();
            }
        }



        #endregion

        #region "Shared Properties and Methods"

        #endregion
    }
}