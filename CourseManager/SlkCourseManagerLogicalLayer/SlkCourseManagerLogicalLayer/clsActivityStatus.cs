
using System;
using System.Text;
using System.Collections.Generic;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;
using Axelerate.BusinessLayerFrameWork.BLCore.Validation.ValidationAttributes;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;

namespace Axelerate.SlkCourseManagerLogicalLayer
{
    /// <summary>
    /// This class represents a Status of an Activity in the Course Manager
    /// <threadsafety static="true" instance="false"/>
    /// </summary>
    [Serializable(), SecurableClass(SecurableClassAttribute.SecurableTypes.CRUD)]
    public class clsActivityStatus : GUIDNameBusinessTemplate<clsActivityStatus>
    {
        #region "DataLayer Overrides"

        /// <summary>
        /// The Data layer is required to establish the connection
        /// </summary>
        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsActivityStatus), "ActivityStatuses", "_ast", false, false, "SLKCM");

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
        /// Status Icon
        /// </summary>
        //[FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private byte[] m_StatusIcon = null;


        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or sets the value of the Description
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
        /// Gets or sets the value of the Status Icon
        /// </summary>
        [SecurableProperty(SecurablePropertyAttribute.SecurableTypes.Update)]
        public Bitmap StatusIcon
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
        /// Returns the HTML Tag representing the Status' Image Icon
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
        /// Unassigned Status
        /// </summary>
        private static clsActivityStatus m_UnassignedStatus = null;
        /// <summary>
        /// Assigned Status
        /// </summary>
        private static clsActivityStatus m_AssignedStatus = null;
        /// <summary>
        /// Completed Status
        /// </summary>
        private static clsActivityStatus m_CompletedStatus = null;

        /// <summary>
        /// Gets the Unassigned status object
        /// </summary>
        public static clsActivityStatus UnassignedStatus
        {
            get
            {
                if (m_UnassignedStatus == null)
                {
                    m_UnassignedStatus = TryGetObjectByGUID("1", null);
                }
                return m_UnassignedStatus;
            }
        }
        /// <summary>
        /// Gets the Assigned status object
        /// </summary>
        public static clsActivityStatus AssignedStatus
        {
            get
            {
                if (m_AssignedStatus == null)
                {
                    m_AssignedStatus = TryGetObjectByGUID("2", null);
                }
                return m_AssignedStatus;
            }
        }

        /// <summary>
        /// Gets the Completed status object
        /// </summary>
        public static clsActivityStatus CompletedStatus
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
