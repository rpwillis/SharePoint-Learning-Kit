using System;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using System.Data;
namespace Axelerate.SlkCourseManagerLogicalLayer.Adapters
{
    /// <summary>
    /// Base Class for the AdapterCommand
    /// </summary>
    public abstract class clsAdapterCommandBase : DataLayerCommandBase
    {

        #region "Private Data"
        /// <summary>
        /// Datalayer.
        /// </summary>
        clsAdapterDataLayer m_DataLayer = null;
        /// <summary>
        /// Criteria
        /// </summary>
        BLCriteria m_Criteria = null;
        /// <summary>
        /// DataSet.
        /// </summary>
        DataSet m_DataSetXML = null;
        /// <summary>
        /// Bound BusinessObject
        /// </summary>
        BLBusinessBase m_BusinessObject = null;
        /// <summary>
        /// RecordIndex
        /// </summary>
        int m_RecordIndex = -1;
        /// <summary>
        /// TableIndex
        /// </summary>
        int m_TableIndex = 0;
        /// <summary>
        /// Adapter's Instance
        /// </summary>
        clsAdapterBase m_AdapterInstace = null;

        #endregion

        #region "Public Properties"
        /// <summary>
        /// Gets or Sets the DataLayer.
        /// </summary>
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

        /// <summary>
        /// Gets or Sets the Criteria
        /// </summary>
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
        /// <summary>
        /// Gets or Sets the DataSet
        /// </summary>
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
        /// <summary>
        /// Gets or Sets the Business Object
        /// </summary>
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

        /// <summary>
        /// Gets the Adapter's Instance
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="NewCriteria"></param>
        public clsAdapterCommandBase(clsAdapterCommandBase Command, BLCriteria NewCriteria)
            : base("", Command.DataLayer)
        {
            DataLayer = Command.DataLayer;
            Criteria = NewCriteria;
            BusinessObject = Command.BusinessObject;
            m_CommandState = CommandStateType.ReadyToExecuteState;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataLayer"></param>
        /// <param name="NewCriteria"></param>
        public clsAdapterCommandBase(clsAdapterDataLayer pDataLayer, BLCriteria NewCriteria)
            : base("", pDataLayer)
        {
            DataLayer = pDataLayer;
            Criteria = NewCriteria;
            m_CommandState = CommandStateType.ReadyToExecuteState;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataLayer"></param>
        /// <param name="NewBusinessObject"></param>
        public clsAdapterCommandBase(clsAdapterDataLayer pDataLayer, BLBusinessBase NewBusinessObject)
            : base("", pDataLayer)
        {
            DataLayer = pDataLayer;
            BusinessObject = NewBusinessObject;
            m_CommandState = CommandStateType.ReadyToExecuteState;

        }
        #endregion

        #region "DataLayerCommandBase Overrides"
        /// <summary>
        /// Exists Next Table?
        /// </summary>
        /// <returns></returns>
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
                throw new System.Exception("The command is in an invalid State");
            }
            return false;
        }
        /// <summary>
        /// Exists Next Record?
        /// </summary>
        /// <returns></returns>
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
                throw new System.Exception("The command is in an invalid State");
            }
        }
        /// <summary>
        /// Reads data from the DataSet
        /// </summary>
        /// <param name="index">Index on the DataSet</param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets the Field count.
        /// </summary>
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

        /// <summary>
        /// Returns the FieldName based on the Index
        /// </summary>
        /// <param name="pIndex"></param>
        /// <returns></returns>
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
