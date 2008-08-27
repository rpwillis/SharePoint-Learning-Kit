
Namespace BLCore

    Public Class BLDynamicList
        Inherits BLListBase(Of BLDynamicList, BLDynamicBusinessObject, BLDynamicBusinessObject.DynamicDataKey)

#Region "Business Properties and Methods"
        ''' <summary>
        ''' Creates a Dataset containing the same information as this collection
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDataSet() As System.Data.DataSet
            Dim NDataSet As New DataSet()
            Dim NDataTable As New DataTable(DataLayer.TableName)

            For Each FieldMap As BLFieldMap In DataLayer.FieldMapList.DataFetchFields
                Dim NColumn As DataColumn = New DataColumn(FieldMap.DLFieldName)
                NDataTable.Columns.Add(NColumn)
            Next

            NDataSet.Tables.Add(NDataTable)
            For Each BO As BLDynamicBusinessObject In Me
                Dim NDataRow As DataRow = NDataTable.NewRow
                For Each FieldMap As BLFieldMap In DataLayer.FieldMapList.DataFetchFields
                    NDataRow.Item(FieldMap.DLFieldName) = BO.FieldValue(FieldMap)
                Next
                NDataTable.Rows.Add(NDataRow)
            Next
            Return NDataSet
        End Function

#End Region
    End Class

End Namespace
