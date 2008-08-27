Partial Class DSDataLayerMetaData
    Partial Class BusinessObjects_DataLayersDataTable

        Private Sub BusinessObjects_DataLayersDataTable_ColumnChanging(ByVal sender As System.Object, ByVal e As System.Data.DataColumnChangeEventArgs) Handles Me.ColumnChanging
            If (e.Column.ColumnName = Me.BusinessObjectNameColumn.ColumnName) Then
                'Add user code here
            End If

        End Sub

    End Class

End Class
