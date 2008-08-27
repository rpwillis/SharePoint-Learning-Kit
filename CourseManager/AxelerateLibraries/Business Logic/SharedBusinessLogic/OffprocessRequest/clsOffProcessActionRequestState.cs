using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.OffprocessRequest
{
    /// <summary>
    /// Represent the different Lock Types in the MIP Portal.  It inherits two base properties from GUIDNameBusinessTemplate, a GUID property that 
    /// identifies the instance and a Name property that describes the instance.
    /// </summary>
    [Serializable(), SecurityToken("clsOffProcessActionRequestState", "clsOffProcessActionRequestStates", "MIPCustom")]
    public class clsOffProcessActionRequestState : GUIDNameBusinessTemplate<clsOffProcessActionRequestState>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsOffProcessActionRequestState), "OffProcessActionRequestStates", "_oat", false, false, "Shared");

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
        /// <summary>
        /// Default Action State: Pending
        /// </summary>
        private static clsOffProcessActionRequestState m_DefaultState = null;

        /// <summary>
        /// Completed Action State
        /// </summary>
        private static clsOffProcessActionRequestState m_CompletedState = null;

        /// <summary>
        /// Faield Action State
        /// </summary>
        private static clsOffProcessActionRequestState m_FailedState = null;

        /// <summary>
        /// Gets the default action State.
        /// </summary>
        public static clsOffProcessActionRequestState DefaultState
        {
            get
            {                
                if (m_DefaultState == null)
                    m_DefaultState = clsOffProcessActionRequestState.GetObjectByGUID("1", null);
                return m_DefaultState;
            }
        }

        /// <summary>
        /// Gets the pending action State.
        /// </summary>
        public static clsOffProcessActionRequestState PendingState
        {
            get
            {
                return DefaultState;
            }
        }

        /// <summary>
        /// Gets the Completed action State.
        /// </summary>
        public static clsOffProcessActionRequestState CompletedState
        {
            get
            {
                if (m_CompletedState == null)
                    m_CompletedState = clsOffProcessActionRequestState.GetObjectByGUID("2", null);
                return m_CompletedState;
            }
        }

        /// <summary>
        /// Gets the Failed action State.
        /// </summary>
        public static clsOffProcessActionRequestState FailedState
        {
            get
            {
                if (m_FailedState == null)
                    m_FailedState = clsOffProcessActionRequestState.GetObjectByGUID("3", null);
                return m_FailedState;
            }
        }

        #endregion
    }
}