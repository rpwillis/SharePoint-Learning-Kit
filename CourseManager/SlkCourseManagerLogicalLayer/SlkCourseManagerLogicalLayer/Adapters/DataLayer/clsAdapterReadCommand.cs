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
    /// Internal framework use.
    /// </summary>
    public class clsAdapterReadCommand : clsAdapterCommandBase
    {
        #region "constructors"
        public clsAdapterReadCommand(clsAdapterDataLayer pDataLayer, BLCriteria NewCriteria)
            : base(pDataLayer, NewCriteria)
        {
        }
        #endregion
        
        #region "DataLayerCommandBase Overrides"
        /// <summary>
        /// Internal framework use.
        /// </summary>
        protected override void OnExecute()
        {
            try
            {
                DataSet tmpDataSet = TransformDataSet(InvokeFactory());


                string FilterExpression = Criteria.GetFilterText();
                DataTable MainTable = tmpDataSet.Tables[0];
                DataRow[] Rows;
                if (FilterExpression == "")
                {
                    DataSetXML = tmpDataSet;
                }
                else
                {

                    Rows = MainTable.Select(FilterExpression);
                    if (Rows.Length == MainTable.Rows.Count)
                    {
                        DataSetXML = tmpDataSet;
                    }
                    else
                    {
                        DataSetXML = new DataSet();
                        if (Rows.Length > 0)
                        {
                            DataTable NewTable = MainTable.Clone();
                            int RowsLength = Criteria.MaxRegisters;
                            if ((RowsLength == -1) || (Criteria.MaxRegisters > Rows.Length))
                            {
                                RowsLength = Rows.Length;
                            }

                            foreach (DataRow Row in Rows)
                            {
                                NewTable.Rows.Add(Row.ItemArray);

                            }
                            DataSetXML.Tables.Add(NewTable);
                        }
                    }
                }
                m_CommandState = CommandStateType.DataRetrievalState;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Internal framework use.
        /// </summary>
        /// <param name="SourceDataSet"></param>
        /// <returns></returns>
        private DataSet TransformDataSet(DataSet SourceDataSet)
        {
            if ((SourceDataSet != null) && (SourceDataSet.Tables.Count > 0) && (SourceDataSet.Tables[0].Rows.Count > 0))
            {
                clsAdapterBase AdapterInstance = DataLayer.AdapterInstance;
                DataSet newDataSet = new DataSet();
                DataTable newDataTable = newDataSet.Tables.Add();
                int NumFields = DataLayer.FieldMapList.DataFetchFields.Count;

               
                foreach (BLFieldMap FieldMap in DataLayer.FieldMapList.DataFetchFields)
                {
                    DataColumn Col = newDataTable.Columns.Add();
                    Col.ColumnName = FieldMap.DLFieldName;
                    FieldInfo fInfo = null;
                    try
                    {
                        fInfo = DataLayer.BusinessLogicType.GetField(FieldMap.BLFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Instance);
                    }
                    catch
                    {
                        fInfo = DataLayer.BusinessLogicType.GetField(FieldMap.BLFieldName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Instance);
                    }
                    Col.DataType = fInfo.FieldType;//typeof(string);
                }

                foreach (DataRow Row in SourceDataSet.Tables[0].Rows)
                {
                    int ActualFieldIndex = 0;
                    object[] ItemArray = new object[NumFields];
                    while (ActualFieldIndex < NumFields)
                    {
                        object data = AdapterInstance.DataTransform(ActualFieldIndex, Row);

                        if (typeof(DateTime).IsInstanceOfType(data))
                        {
                            ItemArray[ActualFieldIndex] = data;
                        }
                        else
                        {
                            ItemArray[ActualFieldIndex] = data;
                        }
                        ActualFieldIndex = ActualFieldIndex + 1;
                    }
                    newDataTable.Rows.Add(ItemArray);
                }
                return newDataSet;
            }
            return SourceDataSet;
        }
        /// <summary>
        /// Internal framework use.
        /// </summary>
        public override void Finish()
        {
            m_CommandState = CommandStateType.Finished;
        }
        #endregion

        #region "Private Methods and Properties"
        /// <summary>
        /// Internal framework use.
        /// </summary>
        /// <returns></returns>
        private DataSet InvokeFactory()
        {
            try
            {
                object AdapterInstace = DataLayer.AdapterInstance;
                return (DataSet)DataLayer.AdapterType.InvokeMember(FactoryMethodName,
                    BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                    null, AdapterInstace, PreFilterArgs);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Internal framework use.
        /// </summary>
        private string FactoryMethodName
        {
            get
            {
                string toReturn = "";              
                toReturn = Criteria.FactoryMethodName;
                if (toReturn == "")
                {
                    toReturn = DataLayer.FactoryMethodName;
                }
                return toReturn;
            }
        }
        /// <summary>
        /// Internal framework use.
        /// </summary>
        private object[] PreFilterArgs
        {
            get
            {
                List<Pair<string, object>> PreFilters = null;
                PreFilters = (Criteria).PreFilters;

                if (PreFilters == null)
                    return null;
                if (PreFilters.Count == 0)
                    return null;

                object[] toReturn = new object[PreFilters.Count];
                int i = 0;
                foreach (Pair<string, object> PreFilter in PreFilters)
                {
                    toReturn[i] = PreFilter.Second;
                    i++;
                }

                return toReturn;
            }
        }

        #endregion
    }
}
