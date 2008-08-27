
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;

namespace Axelerate.SlkCourseManagerLogicalLayer
{

     /// <summary>
    /// This class represents an Activity Group Status.
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivityGroupStatus : GUIDNameBusinessTemplate<clsActivityGroupStatus>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsActivityGroupStatus), "ActivityGroupStatuses", "_ags", false, false, "SLKCM");

        /// <summary>
        /// Gets or sets the DataLayer value
        /// </summary>
        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }

        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Description
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        /// <summary>
        /// Icon
        /// </summary>
        //[FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private byte[] m_Icon = null;


        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute Description
        /// </summary>
        [StringLengthValidation(100), SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
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
        /// Gets or sets the value of the attribute Icon
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public byte[] Icon
        {
            get { return m_Icon; }
            set
            {
                m_Icon = value;
                PropertyHasChanged();
            }
        }
              
        /// <summary>
        /// Pending Start Status
        /// </summary>
        private static clsActivityGroupStatus m_PendingStartStatus = null;
        
        /// <summary>
        /// Started Status
        /// </summary>
        private static clsActivityGroupStatus m_StartedStatus = null;
        
        /// <summary>
        /// Completed Status
        /// </summary>
        private static clsActivityGroupStatus m_CompletedStatus = null;
        
        /// <summary>
        /// Gets the Pending Start Status Object
        /// </summary>
        public static clsActivityGroupStatus PendingStartSatatus
        {
            get
            {
                if (m_PendingStartStatus == null)
                {
                    m_PendingStartStatus = TryGetObjectByGUID("1", null);
                }
                return m_PendingStartStatus;
            }
        }
        
        /// <summary>
        /// Gets the Started Status Object
        /// </summary>
        public static clsActivityGroupStatus StartedStatus
        {
            get
            {
                if (m_StartedStatus == null)
                {
                    m_StartedStatus = TryGetObjectByGUID("2", null);
                }
                return m_StartedStatus;
            }
        }

        /// <summary>
        /// Gets the Completed Status Object
        /// </summary>
        public static clsActivityGroupStatus CompletedStatus
        {
            get
            {
                if (m_CompletedStatus == null)
                {
                    m_CompletedStatus = TryGetObjectByGUID("3", null);
                }
                return m_CompletedStatus;
            }
        }

        #endregion
    }
}
