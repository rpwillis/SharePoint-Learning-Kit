
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
    /// This class represents the status of an Assigned Activity
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsAssignedActivityStatus : GUIDNameBusinessTemplate<clsAssignedActivityStatus>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsAssignedActivityStatus), "AssignedActivityStatuses", "_aas", false, false, "SLKCM");

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
        /// Defines the Attribute Description, this attribute is the description of the Upload Ticket State.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_Description = "";

        /// <summary>
        /// Defines the Attribute Description, this attribute is the description of the Upload Ticket State.
        /// </summary>
        //[FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private byte[] m_StatusIcon = null;


        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the attribute StatusTooltip
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
        /// Gets or sets the value of the attribute StatusIcon
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public System.Drawing.Bitmap StatusIcon
        {
            get
            {
                switch (GUID)
                {
                    case "1":
                        return Axelerate.SlkCourseManagerLogicalLayer.Resources.ActivityStatusImages.slk_assigned;
                    case "2":
                        return Axelerate.SlkCourseManagerLogicalLayer.Resources.ActivityStatusImages.slk_unassigned;
                    case "3":
                        return Axelerate.SlkCourseManagerLogicalLayer.Resources.ActivityStatusImages.slk_submitted;
                    case "4":
                        return Axelerate.SlkCourseManagerLogicalLayer.Resources.ActivityStatusImages.slk_returntolearners;
                    default:
                        return null;
                        break;
                }
            }

        }

        /// <summary>
        /// Gets the HTML Tag for the Status' Image Icon
        /// </summary>
        public string StatusImageHTMLTag
        {
            get
            {
                switch (GUID)
                {
                    case "1":
                        return "<img src=\"/Images.asmx?ObjectClass=" + this.GetType().AssemblyQualifiedName + "&FixedImageURL=slk_assigned\" alt=\"Assigned\" />";
                    case "2":
                        return "<img src=\"/Images.asmx?ObjectClass=" + this.GetType().AssemblyQualifiedName + "&FixedImageURL=slk_submitted\" alt=\"Submitted by the Learner\" />";
                    case "3":
                        return "<img src=\"/Images.asmx?ObjectClass=" + this.GetType().AssemblyQualifiedName + "&FixedImageURL=slk_submitted\" alt=\"Graded\" />";
                    case "4":
                        return "<img src=\"/Images.asmx?ObjectClass=" + this.GetType().AssemblyQualifiedName + "&FixedImageURL=slk_returntolearners\" alt=\"Returned to the Learner\" />";
                    default:
                        return null;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the HTML Tag for the Status' Image Icon. Deprecated.
        /// </summary>
        public string StatusIconHttpImageTag
        {
            get
            {
                switch (GUID)
                {
                    case "1":
                        return "<img src='/DevCommon/Images/timer_16.gif'>";
                    case "2":
                        return "<img src='/DevCommon/Images/inbox_purple16_h.gif'>";
                    case "3":
                        return "<img src='/DevCommon/Images/tick_16_h.gif'>";
                    case "4":
                        return "<img src='/DevCommon/Images/outbox_purple16_h.gif'>";
                    default:
                        return null;
                        break;
                }
            }
        }

        /// <summary>
        /// Pending Submit Status
        /// </summary>
        private static clsAssignedActivityStatus m_PendingSubmitStatus = null;
        /// <summary>
        /// Sittin on Inbox Status
        /// </summary>
        private static clsAssignedActivityStatus m_SittingOnInboxStatus = null;
        /// <summary>
        /// Ready To Return Status
        /// </summary>
        private static clsAssignedActivityStatus m_ReadyToReturnStatus = null;
        /// <summary>
        /// Returned Status
        /// </summary>
        private static clsAssignedActivityStatus m_ReturnedStatus = null;

        /// <summary>
        /// Gets an instance of PendingSubmitStatus object.
        /// </summary>
        public static clsAssignedActivityStatus PendingSubmitStatus 
        {
            get
            {
                if (m_PendingSubmitStatus == null)
                {
                    m_PendingSubmitStatus = TryGetObjectByGUID("1", null);
                }
                return m_PendingSubmitStatus;
            }        
        }

        /// <summary>
        /// Gets an instance of SittingOnInboxStatus object.
        /// </summary>
        public static clsAssignedActivityStatus SittingOnInboxStatus
        {
            get
            {
                if (m_SittingOnInboxStatus == null)
                {
                    m_SittingOnInboxStatus = TryGetObjectByGUID("2", null);
                }
                return m_SittingOnInboxStatus;
            }
        }

        /// <summary>
        /// Gets an instance of ReadyToReturnStatus object.
        /// </summary>
        public static clsAssignedActivityStatus ReadyToReturnStatus
        {
            get
            {
                if (m_ReadyToReturnStatus == null)
                {
                    m_ReadyToReturnStatus = TryGetObjectByGUID("3", null);
                }
                return m_ReadyToReturnStatus;
            }
        }

        /// <summary>
        /// Gets an instance of ReturnedStatus object.
        /// </summary>
        public static clsAssignedActivityStatus ReturnedStatus
        {
            get
            {
                if (m_ReturnedStatus == null)
                {
                    m_ReturnedStatus = TryGetObjectByGUID("4", null);
                }
                return m_ReturnedStatus;
            }
        }


        #endregion

    }
}