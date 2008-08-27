using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Templates;
using Axelerate.BusinessLayerFrameWork.BLCore.Security;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.Attributes;

using System.Reflection;
namespace Axelerate.BusinessLogic.SharedBusinessLogic.OffprocessRequest
{

    /// <summary>
    /// Represents the time where an off process action will be execute.
    /// </summary>
    [Serializable(), SecurityToken("clsOffProcessSchedule", "clsOffProcessSchedules", "MIPCustom")]
    public class clsOffProcessSchedule : OneToOneGUIDBusinessTemplate<clsOffProcessActionRequestInstance, clsOffProcessSchedule>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsOffProcessSchedule), "OffProcessSchedules", "_ops", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// The date when the action is going to be executed.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_NextExecutionDate = DateTime.Now; //TODO: Csla no longer supported SmartDate

        /// <summary>
        /// The date when the action is going to be executed.
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private int m_RecurAfterDays = 0;

        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// True if the scheduled action is recurring.  If it is the RecurAfterDays property .
        /// can be checked to know when the next recurrence should occur
        /// </summary>
        public bool isRecurring
        {
            get
            {
                return RecurAfterDays > 0;
            }

        }
        /// <summary>
        /// Indicate when the next recurrence should occur. If 0 the schedule is not recurring
        /// </summary>
        public int RecurAfterDays
        {
            get { return m_RecurAfterDays; }
            set
            {
                m_RecurAfterDays = value;
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Indicates the time when the operation should be executed.
        /// </summary>
        public DateTime NextExecutionDate
        {
            get { return m_NextExecutionDate.Date; }
            set
            {
                //m_NextExecutionDate = new SmartDate(value);
                m_NextExecutionDate = value;
                PropertyHasChanged();
            }
        }



        #endregion

    }
}