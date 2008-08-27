using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore;

namespace Axelerate.BusinessLogic.SharedBusinessLogic.Adapters
{
    public class clsAdapterDataLayer : DataLayerAbstraction
    {
        #region "Private Data"

        System.Type m_AdapterType = null;

        #endregion
        #region "constructors"

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
        public string FactoryMethodName
        {
            get
            {
                return TableName;
            }

        }

        public string AdapterTypeName
        {
            get
            {
                return DefaultDataSourceName;
            }
        }

        public System.Type AdapterType
        {
            get
            {
                return m_AdapterType;
            }
        }

        public clsAdapterBase AdapterInstance
        {
            get
            {
                return (clsAdapterBase)System.Activator.CreateInstance(AdapterType);
            }
        }

        #endregion

        #region "DataLayerAbstraction Overrides"

        public override DataLayerCommandBase ReadCommand(object Caller, Axelerate.BusinessLayerFrameWork.BLCore.BLCriteria Criteria, List<Axelerate.BusinessLayerFrameWork.BLCore.BLFieldMap> CachedObjects)
        {
            return new clsAdapterReadCommand(this, Criteria);
        }

        public override DataLayerCommandBase DeleteCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterDeleteCommand(this, BusinessObject);
        }

        public override DataLayerCommandBase InsertCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterInsertCommand(this, BusinessObject);
        }

        public override DataLayerCommandBase UpdateCommand(Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase BusinessObject)
        {
            return new clsAdapterUpdateCommand(this, BusinessObject);
        }

        #endregion

    }
}
