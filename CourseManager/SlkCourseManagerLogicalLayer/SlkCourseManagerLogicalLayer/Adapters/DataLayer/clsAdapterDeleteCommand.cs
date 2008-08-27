using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.DataTypes;
using System.Reflection;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// Internal use.
    /// </summary>
    class clsAdapterDeleteCommand : clsAdapterCommandBase
    {
        #region "Constructors"
        /// <summary>
        /// Internal framework use.
        /// </summary>
        /// <param name="pDataLayer"></param>
        /// <param name="NewBusinessObject"></param>
        public clsAdapterDeleteCommand(clsAdapterDataLayer pDataLayer, BLBusinessBase NewBusinessObject)
            : base(pDataLayer, NewBusinessObject)
        {
        }
        #endregion

        #region "DataLayerCommandBase Overrides"
        /// <summary>
        /// Internal framework use.
        /// </summary>
        protected override void OnExecute()
        {
            clsAdapterBase AdapterInstance = DataLayer.AdapterInstance;
            string InserMethodName = AdapterInstance.GetMethodName(clsAdapterBase.AdapterMethodType.AdapaterDelete);

            object[] ParamsValues = new object[DataLayer.FieldMapList.KeyFields.Count];
            int i = 0;
            foreach (BLFieldMap param in DataLayer.FieldMapList.KeyFields)
            {
                ParamsValues[i] = BusinessObject.get_FieldValue(param.Field);
                i++;
            }

            DataLayer.AdapterType.InvokeMember(InserMethodName,
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                null, AdapterInstance, ParamsValues); 
        }

        /// <summary>
        /// Internal framework use.
        /// </summary>
        public override void Finish()
        {
            m_CommandState = CommandStateType.Finished;
        }
        #endregion
    }
}
