
Namespace DLCore
    Public Class DataLayerXMLInsertCommand
        Inherits DataLayerXMLCommand

        Public Sub New(ByVal DataLayer As XMLDataLayer, ByVal DataSourceName As String)
            MyBase.New(DataLayer, Nothing, DataSourceName)


        End Sub

#Region "DataLayerCommandBase Overrides"


        Protected Overrides Sub OnExecute()
            Dim XMLDataset As XMLDataSetContainer = XmlCache.GetXMLDataSet(DataLayer.XMLName)
            XMLDataset.Lock.AcquireReaderLock(10000)
            Try
                Dim nDataTable As DataTable = XmlCache.GetXMLDataTable(DataLayer.XMLName, DataLayer.TableName, DataLayer.FieldMapList)
                Dim nDataTableInformation As DataTableInformation = CType(XMLDataset.DataTableInformations(DataLayer.TableName), DataTableInformation)
                nDataTableInformation.Lock.AcquireWriterLock(10000)
                Try
                    Dim FieldMap As BLFieldMap
                    Dim FilterExpression As String = BusinessObject.KeyCriteria.GetFilterText()
                    Dim Rows() As DataRow = nDataTable.Select(FilterExpression)
                    If Rows.Length = 0 Then
                        Dim NewRow As DataRow = nDataTable.NewRow()
                        For Each FieldMap In DataLayer.FieldMapList.OrderedFields
                            If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField And FieldMap.isUpdateField Then
                                Dim FieldName As String = FieldMap.BLFieldName 'nombre de los campos
                                If FieldMap.AutoNumericType = BLFieldMap.AutoNumericTypeEnum.IdentityColumn Then
                                    NewRow.Item(FieldMap.DLFieldName) = XmlCache.IncAutonumericValue(DataLayer.XMLName, DataLayer.TableName, FieldMap.DLFieldName)
                                Else
                                    Dim FieldValue As Object = BusinessObject.FieldValue(FieldMap)
                                    NewRow.Item(FieldMap.DLFieldName) = FieldValue
                                End If
                            End If
                        Next
                        nDataTable.Rows.Add(NewRow)
                    End If
                Catch ex As Exception
                Finally
                    nDataTableInformation.Lock.ReleaseWriterLock()
                End Try

            Catch ex As Exception
            Finally
                XMLDataset.Lock.ReleaseReaderLock()
            End Try
            m_CommandState = CommandStateType.DataRetrievalState
        End Sub



        Public Overrides Sub Finish()

            m_CommandState = CommandStateType.Finished
        End Sub
#End Region
    End Class
End Namespace
