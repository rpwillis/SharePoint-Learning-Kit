using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Axelerate.BusinessLayerFrameWork.BLCore;
using Axelerate.BusinessLayerFrameWork.DLCore;
using Axelerate.BusinessLayerFrameWork.BLCore.DataTypes;
using System.Reflection;


namespace Axelerate.BusinessLogic.SharedBusinessLogic.Adapters
{
    public class clsAdapterReadCommand : clsAdapterCommandBase
    {
        #region "constructors"
        public clsAdapterReadCommand(clsAdapterDataLayer pDataLayer, BLCriteria NewCriteria)
            : base(pDataLayer, NewCriteria)
        {
        }
        #endregion


        #region "DataLayerCommandBase Overrides"
        protected override void OnExecute()
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
                    Col.DataType = typeof(string);
                }

                foreach (DataRow Row in SourceDataSet.Tables[0].Rows)
                {
                    int ActualFieldIndex = 0;
                    object[] ItemArray = new object[NumFields];
                    while (ActualFieldIndex < NumFields)
                    {
                        ItemArray[ActualFieldIndex] = AdapterInstance.DataTransform(ActualFieldIndex, Row);                        
                        ActualFieldIndex = ActualFieldIndex + 1;
                    }
                    newDataTable.Rows.Add(ItemArray);
                }
                return newDataSet;
            }
            return SourceDataSet;
        }
       
        public override void Finish()
        {
            m_CommandState = CommandStateType.Finished;
        }
        #endregion

        #region "Private Methods and Properties"

        private DataSet InvokeFactory()
        {
            object AdapterInstace = DataLayer.AdapterInstance;
            return (DataSet)DataLayer.AdapterType.InvokeMember(FactoryMethodName,
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public,
                null, AdapterInstace, PreFilterArgs);
        }

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
