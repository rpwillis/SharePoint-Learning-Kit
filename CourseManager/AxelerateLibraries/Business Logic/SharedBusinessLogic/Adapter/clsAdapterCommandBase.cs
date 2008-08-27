using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using System.Data;
namespace Axelerate.BusinessLogic.SharedBusinessLogic.Adapters
{
    public abstract class clsAdapterCommandBase : DataLayerCommandBase
    {

        #region "Private Data"

        clsAdapterDataLayer m_DataLayer = null;
        BLCriteria m_Criteria = null;
        DataSet m_DataSetXML = null;
        BLBusinessBase m_BusinessObject = null;
        int m_RecordIndex = -1;
        int m_TableIndex = 0;
        clsAdapterBase m_AdapterInstace = null;

        #endregion

        #region "Public Properties"
        public new clsAdapterDataLayer DataLayer
        {
            get
            {
                return m_DataLayer;
            }
            set
            {
                m_DataLayer = value;
            }

        }

        public BLCriteria Criteria
        {
            get
            {
                return m_Criteria;
            }
            set
            {
                m_Criteria = value;
            }
        }

        public DataSet DataSetXML
        {
            get
            {
                return m_DataSetXML;
            }
            set
            {
                m_DataSetXML = value;
            }
        }

        public BLBusinessBase BusinessObject
        {
            get
            {
                return m_BusinessObject;
            }
            set
            {
                m_BusinessObject = value;
            }
        }

        public clsAdapterBase AdapterInstance
        {
            get
            {
                if (m_AdapterInstace == null)
                    m_AdapterInstace = DataLayer.AdapterInstance;
                return m_AdapterInstace;
            }
        }

        #endregion

        #region "Constructors"
        public clsAdapterCommandBase(clsAdapterCommandBase Command, BLCriteria NewCriteria)
            : base("", Command.DataLayer)
        {
            DataLayer = Command.DataLayer;
            Criteria = NewCriteria;
            BusinessObject = Command.BusinessObject;
            m_CommandState = CommandStateType.ReadyToExecuteState;
        }

        public clsAdapterCommandBase(clsAdapterDataLayer pDataLayer, BLCriteria NewCriteria)
            : base("", pDataLayer)
        {
            DataLayer = pDataLayer;
            Criteria = NewCriteria;
            m_CommandState = CommandStateType.ReadyToExecuteState;

        }

        public clsAdapterCommandBase(clsAdapterDataLayer pDataLayer, BLBusinessBase NewBusinessObject)
            : base("", pDataLayer)
        {
            DataLayer = pDataLayer;
            BusinessObject = NewBusinessObject;
            m_CommandState = CommandStateType.ReadyToExecuteState;

        }
        #endregion

        #region "DataLayerCommandBase Overrides"
        public override bool NextTable()
        {
            if (CommandState == CommandStateType.DataRetrievalState)
            {
                if (DataSetXML != null)
                {
                    if (DataSetXML.Tables.Count > m_TableIndex + 1)
                    {
                        m_TableIndex = m_TableIndex + 1;
                        m_RecordIndex = -1;
                        return true;
                    }
                }

            }
            else
            {
                throw new System.Exception(Resources.ErrorMessages.errCommandInvalidState);
            }
            return false;
        }

        public override bool NextRecord()
        {
            if (CommandState == CommandStateType.DataRetrievalState)
            {
                if (DataSetXML != null)
                {
                    if (DataSetXML.Tables.Count > m_TableIndex)
                    {
                        DataTable Table = DataSetXML.Tables[m_TableIndex];
                        if (Table.Rows.Count > m_RecordIndex + 1)
                        {
                            m_RecordIndex = m_RecordIndex + 1;
                            return true;
                        }
                    }

                }
                return false;
            }
            else
            {
                throw new System.Exception(Resources.ErrorMessages.errCommandInvalidState);
            }
        }

        public override object ReadData(int index)
        {
            if (DataSetXML != null)
            {
                if (DataSetXML.Tables.Count > m_TableIndex)
                {
                    DataTable Table = DataSetXML.Tables[m_TableIndex];
                    if (Table.Rows.Count > m_RecordIndex)
                    {
                        DataRow Row = Table.Rows[m_RecordIndex];
                        return Row[index];
                    }
                }
            }
            return null;
        }

        public override int FieldCount
        {
            get
            {
                if (m_TableIndex >= 0)
                {
                    return DataSetXML.Tables[m_TableIndex].Columns.Count;
                }
                return 0;
            }
        }
        
        public override string get_FieldName(int pIndex)
        {
            if (m_TableIndex >= 0)
            {
                return DataSetXML.Tables[m_TableIndex].Columns[pIndex].ColumnName;
            }
            return pIndex.ToString();
        }
        
        #endregion
    }
}
