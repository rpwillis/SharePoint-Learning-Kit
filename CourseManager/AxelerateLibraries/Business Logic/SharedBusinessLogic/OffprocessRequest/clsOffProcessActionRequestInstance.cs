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
using Axelerate.BusinessLogic.SharedBusinessLogic.Security;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.OffprocessRequest
{

    /// <summary>
    /// Represents a TFS Project It inherits two base properties from DetailGUIDBussinessTemplate, 
    /// a GUID property that identifies the instance and a MasterObject that relates this server to its parent TFS Server.
    /// </summary>
    [Serializable(), SecurityToken("clsOffProcessActionRequestInstance", "clsOffProcessActionRequestInstances", "MIPCustom")]
    public class clsOffProcessActionRequestInstance : DetailGUIDBussinessTemplate<clsOffProcessActionRequest, clsOffProcessActionRequestInstance>
    {
        #region "DataLayer Overrides"


        private static SQLDataLayer m_DataLayer = new SQLDataLayer(typeof(clsOffProcessActionRequestInstance), "OffProcessActionRequestInstances", "_oai", false, false, "Shared");

        public override DataLayerAbstraction DataLayer
        {
            get { return m_DataLayer; }
            set { }
        }
        #endregion

        #region "Business Object Data"

        /// <summary>
        /// Action Request's Date
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_RequestDate = DateTime.Now; //TODO: Csla no longer supported SmartDate

        /// <summary>
        /// Action Execution's Date
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private DateTime m_ExecutionDate = new DateTime(); //TODO: Csla no longer supported SmartDate

        /// <summary>
        /// Foreign GUID for the reflected object to be created
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ObjectGUID = "";

        /// <summary>
        /// Foreign GUID for the ADUser that created this request
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "ADUser", false)]
        private string m_ADUserGUID= "";

        /// <summary>
        /// Foreign GUID for the State of this request
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "State", false)]
        private string m_StateGUID = clsOffProcessActionRequestState.DefaultState.GUID;

        /// <summary>
        /// Exception Message if failed
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_ExceptionInfo = "";

        /// <summary>
        /// Statck Trace for the Exception if failed
        /// </summary>
        [FieldMap(false, true, true, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", false)]
        private string m_StackTrace= "";

        /// <summary>
        /// Cache for the ADUser
        /// </summary>
        [CachedForeignObject("ADUser", typeof(clsADUser), "ADUserGUID_oai", "GUID_adu", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsADUser m_ADUser = null;

        /// <summary>
        /// Cache for the State
        /// </summary>
        [CachedForeignObject("State", typeof(clsOffProcessActionRequestState), "StateGUID_oai", "GUID_oat", "", "", "", "", CachedForeignObjectAttribute.CachedObjectLoadType.OnDemand)]
        private clsOffProcessActionRequestState m_State = null;

        [ListFieldMap(true, true, false)]
        private clsOffProcessActionRequestParameterValues m_ParameterValues = null;
        #endregion

        #region "Business Properties and Methods"

        /// <summary>
        /// Gets or Sets the Foreign GUID for the reflected object to be created
        /// </summary>
        public string ObjectGUID
        {
            get { return m_ObjectGUID; }
            set
            {
                m_ObjectGUID = value;
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Gets or Sets the request creation date
        /// </summary>
        public DateTime RequestDate
        {
            get { return m_RequestDate.Date; }
            set
            {
                //m_RequestDate = new SmartDate(value);
                m_RequestDate = value;
                PropertyHasChanged();
            }
        }

        /// <summary>
        /// Gets or Sets the request execution date
        /// </summary>
        public DateTime ExecutionDate
        {
            get { return m_ExecutionDate.Date; }
            set
            {
                //m_ExecutionDate = new SmartDate(value);
                m_ExecutionDate = value;
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Gets or Sets the Foreign ID for the ADUser that created this request
        /// </summary>
        public string ADUserGUID
        {
            get { return m_ADUserGUID; }
            set
            {
                m_ADUserGUID = value;
                PropertyHasChanged();
            }
        }

        public clsADUser ADUser
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsADUser>.GetProperty(this, ref m_ADUser, m_ADUserGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsADUser>.SetProperty(ref m_ADUser, value, ref m_ADUserGUID, true);
                PropertyHasChanged();
            }
        }



        /// <summary>
        /// Gets or Sets the Foreign ID for the State of this request
        /// </summary>
        public string StateGUID
        {
            get { return m_StateGUID; }
            set
            {
                m_StateGUID = value;
                PropertyHasChanged();
            }
        }

        public clsOffProcessActionRequestState State
        {
            get
            {
                return BLGUIDForeignPropertyCache<clsOffProcessActionRequestState>.GetProperty(this, ref m_State, m_StateGUID);
            }
            set
            {
                BLGUIDForeignPropertyCache<clsOffProcessActionRequestState>.SetProperty(ref m_State, value, ref m_StateGUID, true);
                PropertyHasChanged();
            }
        }

        public clsOffProcessActionRequestParemeters  Parameters
        {
            get 
            {
                return clsOffProcessActionRequestParemeters.GetDetailsInMaster(this.MasterObject);
            }
        }

        clsOffProcessActionRequestParameterValues ParameterValues
        {
            get
            {
                if (m_ParameterValues == null)
                    m_ParameterValues = new clsOffProcessActionRequestParameterValues();
                return m_ParameterValues;
            }

        }

        public void SetParameterValue(clsOffProcessActionRequestParameter Parameter, string Value)
        {
            int i = 0;
            clsOffProcessActionRequestParameterValue ParameterValue = null;
            while ((ParameterValue == null) && (i < ParameterValues.Count))
            {
                if (ParameterValues[i].DetailGUID == Parameter.GUID)
                {
                    ParameterValue = ParameterValues[i];
                }
                i = i + 1;
            }

            if (ParameterValue == null)
            {
                ParameterValue = new clsOffProcessActionRequestParameterValue();
                ParameterValue.MasterObject = this;
                ParameterValue.DetailObject = Parameter;
                //ParameterValue.MarkAsChild();
                ParameterValues.Add(ParameterValue);
            }

            ParameterValue.Value = Value;
        }

        public string GetParameterValue(clsOffProcessActionRequestParameter Parameter)
        {

            int i = 0;
            clsOffProcessActionRequestParameterValue ParameterValue = null;
            while ((ParameterValue == null) && (i < ParameterValues.Count))
            {
                if (ParameterValues[i].DetailGUID == Parameter.GUID)
                {
                    ParameterValue = ParameterValues[i];
                }
                i = i + 1;
            }

            if (ParameterValue == null)
            {
                return "";
            }

            return ParameterValue.Value;
        }

        public bool Execute()
        {
            if (State.GUID != clsOffProcessActionRequestState.CompletedState.GUID)
            {
                ExecutionDate = DateTime.Now;
                try
                {
                    BLBusinessBase BusinessBase = null;

                    //Creates a new business instancee of the class that is specified by the EmailActionRequest.ObjectName using reflection
                    BLBusinessBase AuxBusinessBase = (BLBusinessBase)ReflectionHelper.InvokeBusinessFactoryMethod(MasterObject.ObjectName, FactoryMethodType.FMTNew, null);

                    //Uses the new instance to obtain the datalayer suffix to be able to create a Criteria that will retrieve
                    //the business object from the datastore using a GUID as the object key
                    string DBFieldName = "GUID" + AuxBusinessBase.DataLayer.DataLayerFieldSuffix;
                    BLCriteria Criteria = new BLCriteria(ReflectionHelper.CreateBusinessType(MasterObject.ObjectName));
                    Criteria.AddBinaryExpression(DBFieldName, "GUID", "=", ObjectGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                    object[] ProjectCriteria = { Criteria };
                    BusinessBase = (BLBusinessBase)ReflectionHelper.InvokeBusinessFactoryMethod(MasterObject.ObjectName, FactoryMethodType.FMTGet, ProjectCriteria);

                    //Invokes the method specified by ActionRequest using the respective parameters
                    MethodInfo AuxMethodInfo = BusinessBase.GetType().GetMethod(MasterObject.MethodName);
                    object[] MethodParameters = new object[ParameterValues.Count];
                    int i = 0;

                    SortedList<int, clsOffProcessActionRequestParameterValue> OrderedValues = new SortedList<int, clsOffProcessActionRequestParameterValue>();
                    
                    foreach (clsOffProcessActionRequestParameterValue ParameterValue in ParameterValues)                    
                    {
                        OrderedValues.Add(ParameterValue.DetailObject.Order, ParameterValue);
                    };

                    foreach (KeyValuePair<int, clsOffProcessActionRequestParameterValue> ParameterValue in OrderedValues)                    
                    {
                        
                        MethodParameters[i] = ParameterValue.Value.Value;
                        i = i + 1;
                    };
                    AuxMethodInfo.Invoke(BusinessBase, MethodParameters);

                    //Saves the business object
                    BusinessBase.Save();

                    State = clsOffProcessActionRequestState.CompletedState;
                    ExceptionInfo = "";
                    StackTrace = "";           
                }
                catch (System.Exception ex)
                {
                    State = clsOffProcessActionRequestState.FailedState;
                    ExceptionInfo = ex.GetBaseException().ToString();
                    StackTrace = ex.StackTrace.ToString();
                };

                //The process did execute
                return true;
            }

            //The process didn't execute
            return false;
        }

        clsOffProcessSchedule Schedule
        {
            get
            {
                BLCriteria Criteria = new BLCriteria(typeof(clsOffProcessSchedule));
                Criteria.AddBinaryExpression("MasterGUID_ops", "MasterGUID", "=", GUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone);
                clsOffProcessSchedules Schedules = clsOffProcessSchedules.GetCollection(Criteria);
                if (Schedules.Count > 0)
                {
                    return Schedules[0];
                }
                else
                {
                    return new clsOffProcessSchedule();
                }


            }
        }

        public string ParameterName
        {
            get { return MasterObject.Name; }
        }

        public string ParametersString
        {
            get {
                string ToReturn = "";
                foreach (clsOffProcessActionRequestParameterValue Value in ParameterValues)
                {
                    ToReturn = Value.DetailObject.Name + " = " + Value.Value + "; ";
                }
                return ToReturn;
            }
        }

        public string ActionDetails
        {
            get
            {
                return MasterObject.Name;
            }
        }

        /// <summary>
        /// Gets or Sets the Exception Info field
        /// </summary>
        public string ExceptionInfo
        {
            get { return m_ExceptionInfo; }
            set
            {
                m_ExceptionInfo = value;
                PropertyHasChanged();
            }
        }


        /// <summary>
        /// Gets or Sets the Stack Trace
        /// </summary>
        public string StackTrace
        {
            get { return m_StackTrace; }
            set
            {
                m_StackTrace = value;
                PropertyHasChanged();
            }
        }


        #endregion

        #region "Data Access"
        public override void BLUpdate(BLBusinessBase ParentObject)
        {
            clsOffProcessSchedule tmpSchedule = Schedule;
            if (!tmpSchedule.IsNew)
            {
                if (tmpSchedule.isRecurring)
                {
                    tmpSchedule.NextExecutionDate = tmpSchedule.NextExecutionDate.AddDays(tmpSchedule.RecurAfterDays);
                    StateGUID = clsOffProcessActionRequestState.PendingState.GUID;
                }
            }

            base.BLUpdate(ParentObject);
            tmpSchedule.BLUpdate(ParentObject);           


        }

        #endregion
    }
}