using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// Adapter's DataLayer.
    /// </summary>
    public class clsAdapterDataLayer : DataLayerAbstraction
    {
        #region "Private Data"
        /// <summary>
        /// Adapter's Type
        /// </summary>
        System.Type m_AdapterType = null;

        #endregion
        #region "constructors"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BusinessObjectType"></param>
        /// <param name="pAdapterTypeName"></param>
        /// <param name="pFactoryMethodName"></param>
        /// <param name="DatasetSuffix"></param>
        public clsAdapterDataLayer(System.Type BusinessObjectType, string pAdapterTypeName,
            string pFactoryMethodName, string DatasetSuffix)
            :
            base(pFactoryMethodName, DatasetSuffix, BusinessObjectType, pAdapterTypeName)
        {
            BeginDataDefinition();
            AddFieldsFromType(BusinessObjectType);
            EndDataDefinition(false);

            try
            {
                m_AdapterType = ReflectionHelper.CreateBusinessType(pAdapterTypeName);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region "Public Properties and Methods"
        /// <summary>
        /// Gets the Factory Method Name
        /// </summary>
        public string FactoryMethodName
        {
            get
            {
                return TableName;
            }

        }
        /// <summary>
        /// Gets the Adapter's Type Name
        /// </summary>
        public string AdapterTypeName
        {
            get
            {
                return DefaultDataSourceName;
            }
        }

        /// <summary>
        /// Gets the Adapter's Type
        /// </summary>
        public System.Type AdapterType
        {
            get
            {
                return m_AdapterType;
            }
        }

        /// <summary>
        /// Gets an instance of the Adapter
        /// </summary>
        public clsAdapterBase AdapterInstance
        {
            get
            {
                return (clsAdapterBase)System.Activator.CreateInstance(AdapterType);
            }
        }

        #endregion

        #region "DataLayerAbstraction Overrides"
        /// <summary>
        /// Creates a new clsAdapterReadCommand based on a Criteria
        /// </summary>
        /// <param name="Caller">Calling Object</param>
        /// <param name="Criteria">Criteria</param>
        /// <param name="CachedObjects">Cached Objects</param>
        /// <returns></returns>
        public override DataLayerCommandBase ReadCommand(object Caller, Axelerate.BusinessLayerFrameWork.BLCore.BLCriteria Criteria, List<Axelerate.BusinessLayerFrameWork.BLCore.BLFieldMap> CachedObjects)
        {
            return new clsAdapterReadCommand(this, Criteria);
        }
        /// <summary>
        /// Creates a new clsAdapterDeleteCommand
        /// </summary>
        /// <param name="BusinessObject">Business object</param>
        /// <returns></returns>
        public override DataLayerCommandBase DeleteCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterDeleteCommand(this, BusinessObject);
        }

        /// <summary>
        /// Creates a new clsAdapterInsertCommnad
        /// </summary>
        /// <param name="BusinessObject"></param>
        /// <returns></returns>
        public override DataLayerCommandBase InsertCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterInsertCommand(this, BusinessObject);
        }

        /// <summary>
        /// Creates a new clsAdapterUpdateCommnad
        /// </summary>
        /// <param name="BusinessObject"></param>
        /// <returns></returns>
        public override DataLayerCommandBase UpdateCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterUpdateCommand(this, BusinessObject);
        }

        #endregion

    }
}
