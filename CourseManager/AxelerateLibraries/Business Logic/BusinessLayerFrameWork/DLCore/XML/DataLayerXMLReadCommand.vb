Namespace DLCore
    Public Class DataLayerXMLReadCommand
        Inherits DataLayerXMLCommand
        Public Sub New(ByVal DataLayer As XMLDataLayer, ByVal NewCriteria As BLCriteria, ByVal DataSourceName As String)
            MyBase.New(DataLayer, NewCriteria, DataSourceName)


        End Sub

#Region "DataLayerCommandBase Overrides"


        Protected Overrides Sub OnExecute()
            Dim XMLDataset As XMLDataSetContainer = XmlCache.GetXMLDataSet(DataLayer.XMLName, False)
            If XMLDataset Is Nothing Then
            Else
                XMLDataset.Lock.AcquireReaderLock(10000)
                Try
                    Dim nDataTable As DataTable = XmlCache.GetXMLDataTable(DataLayer.XMLName, DataLayer.TableName, DataLayer.FieldMapList, False)
                    If nDataTable Is Nothing Then
                    Else
                        Dim nDataTableInformation As DataTableInformation = CType(XMLDataset.DataTableInformations(DataLayer.TableName), DataTableInformation)
                        nDataTableInformation.Lock.AcquireReaderLock(10000)
                        Try
                            Dim i As Integer
                            Dim FilterExpression As String = Criteria.GetFilterText
                            Dim Rows() As DataRow = nDataTable.Select(FilterExpression)
                            If Rows.Length > 0 Then
                                DataSetXML = New DataSet
                                Dim NewTable As DataTable = nDataTable.Clone()
                                Dim RowsLength As Integer = Criteria.MaxRegisters
                                If ((RowsLength = -1) Or (RowsLength > Rows.Length)) Then
                                    RowsLength = Rows.Length
                                End If
                                For i = 0 To RowsLength - 1
                                    SyncLock Rows(i)
                                        NewTable.Rows.Add(Rows(i).ItemArray)
                                    End SyncLock
                                Next
                                DataSetXML.Tables.Add(NewTable)
                            End If
                Catch ex As Exception
                Finally
                    nDataTableInformation.Lock.ReleaseReaderLock()
                End Try
                    End If
                Catch ex As Exception
                Finally
                    XMLDataset.Lock.ReleaseReaderLock()
                End Try
            End If
            m_CommandState = CommandStateType.DataRetrievalState
        End Sub



        Public Overrides Sub Finish()


        End Sub
#End Region
    End Class
End Namespace