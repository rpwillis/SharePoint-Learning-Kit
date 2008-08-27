Namespace DLCore
    Public Class DataLayerXMLDeleteCommand
        Inherits DataLayerXMLCommand

        Public Sub New(ByVal DataLayer As XMLDataLayer, ByVal DataSourceName As String)
            MyBase.New(DataLayer, Nothing, DataSourceName)
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
                        nDataTableInformation.Lock.AcquireWriterLock(10000)
                        Try
                            Dim FilterExpression As String = BusinessObject.KeyCriteria.GetFilterText()
                            Dim Rows() As DataRow = nDataTable.Select(FilterExpression)
                            If Rows.Length > 0 Then
                                nDataTable.Rows.Remove(Rows(0))
                            End If
                        Catch ex As Exception
                        Finally
                            nDataTableInformation.Lock.ReleaseWriterLock()
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

            m_CommandState = CommandStateType.Finished
        End Sub

#End Region
    End Class
End Namespace