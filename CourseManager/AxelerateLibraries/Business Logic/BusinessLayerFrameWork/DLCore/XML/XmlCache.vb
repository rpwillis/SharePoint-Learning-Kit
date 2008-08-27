Imports System.Configuration.ConfigurationManager
Namespace DLCore
    'Falta las varas para que solo uno pueda grabar a la vez y cosas asi
    Public Class XmlCache
        Shared m_DataSetCache As Hashtable = New Hashtable

 
        Shared Sub SaveDataSet(ByVal DataSetName As String)
            If m_DataSetCache.ContainsKey(DataSetName) = True Then
                SaveDataSet(CType(m_DataSetCache.Item(DataSetName), DataSet))
            End If

        End Sub
        Shared Sub SaveDataSet(ByVal nDataSet As DataSet)
            'schema

            Dim SchemaName As String = "Schema_" + nDataSet.DataSetName 'TODO  
            Dim PathSchema As String = ConnectionStrings(SchemaName).ConnectionString()
            Dim PahtDataSet As String = ConnectionStrings(nDataSet.DataSetName).ConnectionString()

            Dim SchemaStream As IO.FileStream

            SchemaStream = New IO.FileStream(PathSchema, IO.FileMode.Open)
            Try
                nDataSet.WriteXmlSchema(SchemaStream)
            Catch ex As Exception
            Finally
                SchemaStream.Close()
            End Try
            Dim Stream As IO.FileStream
            Stream = New IO.FileStream(PahtDataSet, IO.FileMode.Open)
            Try
                nDataSet.WriteXml(Stream)
            Catch ex As Exception
            Finally
                Stream.Close()
            End Try
            'contenido



        End Sub
        Shared Sub SaveAll()
            If m_DataSetCache.Count > 0 Then
                'Else
                Dim Enumerator As IDictionaryEnumerator = m_DataSetCache.GetEnumerator()
                Dim i As Integer

                For i = 0 To m_DataSetCache.Count - 1
                    Enumerator.MoveNext()
                    Dim XMLDataSetActual As XMLDataSetContainer = CType(Enumerator.Value, XMLDataSetContainer) 'xml
                    Dim DataSetActual As DataSet = XMLDataSetActual.XMLDataSet 'xml
                    XMLDataSetActual.Lock.AcquireWriterLock(10000)
                    Try

                        SaveDataSet(DataSetActual)
                    Catch ex As Exception

                    Finally
                        XMLDataSetActual.Lock.ReleaseWriterLock()
                    End Try


                Next

            End If
  
        End Sub


        Shared Function GetXMLDataTable(ByVal DataSetName As String, ByVal DataTableName As String, ByVal FieldMapList As BLFieldMapList, Optional ByVal CanCreateFile As Boolean = True) As DataTable
            Dim NXMLDataSetContainer As XMLDataSetContainer = GetXMLDataSet(DataSetName, CanCreateFile)
            If NXMLDataSetContainer Is Nothing Then
                Return Nothing
            End If
            Dim nDataTable As DataTable = Nothing
            NXMLDataSetContainer.Lock.AcquireReaderLock(10000)
            Try

                If NXMLDataSetContainer.XMLDataSet.Tables.Contains(DataTableName) = False Then
                    If CanCreateFile Then
                        'construye la tabla usando el FieldMapList
                        Dim FieldMap As BLFieldMap
                        Dim NewTable As New DataTable(DataTableName)

                        For Each FieldMap In FieldMapList.OrderedFields
                            If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Then
                                NewTable.Columns.Add(FieldMap.DLFieldName, FieldMap.Field.FieldType)
                            End If
                        Next
                        NXMLDataSetContainer.XMLDataSet.Tables.Add(NewTable)
                        nDataTable = NewTable
                        'NXMLDataSetContainer.AddTableInformation(NewTable, FieldMapList)
                    End If
                Else
                    nDataTable = NXMLDataSetContainer.XMLDataSet.Tables(DataTableName)
                    'NXMLDataSetContainer.AddTableInformation(nDataTable, FieldMapList)


                End If
                If nDataTable Is Nothing Then
                Else
                    NXMLDataSetContainer.AddTableInformation(nDataTable, FieldMapList)
                End If

            Catch ex As Exception
            Finally
                NXMLDataSetContainer.Lock.ReleaseReaderLock()

            End Try
            Return nDataTable

        End Function
        '        Shared Function GetXMLDataTable(ByVal DataSetName As String, ByVal DataTableName As String, ByVal FieldMapList As BLFieldMapList) As DataTable
        'Dim NXMLDataSetContainer As XMLDataSetContainer = GetXMLDataSet(DataSetName)
        'Dim nDataTable As DataTable = Nothing
        '    NXMLDataSetContainer.Lock.AcquireReaderLock(10000)
        '    Try'

        'If NXMLDataSetContainer Is Nothing Then
        '    NXMLDataSetContainer.Lock.ReleaseReaderLock()
        '    Return Nothing
        'End If
        'If NXMLDataSetContainer.XMLDataSet.Tables.Contains(DataTableName) = False Then
        '    NXMLDataSetContainer.Lock.ReleaseReaderLock()
        '    Return Nothing
        'End If
        'nDataTable = NXMLDataSetContainer.XMLDataSet.Tables(DataTableName)
        'NXMLDataSetContainer.AddTableInformation(nDataTable, FieldMapList)
        ' Catch ex As Exception
        ' Finally
        '     NXMLDataSetContainer.Lock.ReleaseReaderLock()
        ' End Try
        '            Return nDataTable
        '        End Function
        'Shared Sub AddTableInformation(ByVal nDataTable As DataTable, ByVal FieldMapList As BLFieldMapList)

        'End Sub
        Shared Function GetXMLDataSet(ByVal DataSetName As String, Optional ByVal CanCreateFile As Boolean = True) As XMLDataSetContainer

            If m_DataSetCache.ContainsKey(DataSetName) = False Then
                NewXMLDataSet(DataSetName, CanCreateFile)
            End If
            Dim ActualObject As Object = m_DataSetCache.Item(DataSetName)
            Dim nXMLDataSetContainer As XMLDataSetContainer = CType(ActualObject, XMLDataSetContainer)

            Return nXMLDataSetContainer

        End Function
        '        Shared Function GetXMLDataSet(ByVal DataSetName As String) As XMLDataSetContainer

        '           If m_DataSetCache.ContainsKey(DataSetName) = False Then
        '               NewXMLDataSet(DataSetName, False)
        '           End If
        '       Dim ActualObject As Object = m_DataSetCache.Item(DataSetName)
        '       Dim nXMLDataSetContainer As XMLDataSetContainer = CType(ActualObject, XMLDataSetContainer)
        '           Return nXMLDataSetContainer

        '      End Function
        Shared Sub NewXMLDataSet(ByVal DataSetName As String, Optional ByVal CanCreateFile As Boolean = True)

            'NXMLDataSetContainer.Lock.AcquireWriterLock(10000)
            Try

                Dim Stream As IO.FileStream
                Dim SchemaStream As IO.FileStream
                Dim SchemaName As String = "Schema_" + DataSetName
                Dim PathSchema As String = ConnectionStrings(SchemaName).ConnectionString()
                Dim PahtDataSet As String = ConnectionStrings(DataSetName).ConnectionString()

                If IO.File.Exists(PahtDataSet) = False Then
                    If CanCreateFile Then

                        Dim NXMLDataSetContainer As New XMLDataSetContainer(DataSetName)
                        Stream = New IO.FileStream(PahtDataSet, IO.FileMode.OpenOrCreate)
                        Try
                            NXMLDataSetContainer.XMLDataSet.WriteXml(Stream)
                        Catch ex As Exception
                        Finally
                            Stream.Close()
                        End Try

                        SchemaStream = New IO.FileStream(PathSchema, IO.FileMode.OpenOrCreate)

                        Try
                            NXMLDataSetContainer.XMLDataSet.WriteXmlSchema(SchemaStream)
                        Catch ex As Exception

                        Finally
                            SchemaStream.Close()
                        End Try
                        m_DataSetCache.Add(DataSetName, NXMLDataSetContainer)



                    End If
                Else
                    Dim NXMLDataSetContainer As New XMLDataSetContainer(DataSetName)
                    SchemaStream = New IO.FileStream(PathSchema, IO.FileMode.Open)
                    Try
                        NXMLDataSetContainer.XMLDataSet.ReadXmlSchema(SchemaStream)
                    Catch ex As Exception
                    Finally
                        SchemaStream.Close()
                    End Try

                    Stream = New IO.FileStream(PahtDataSet, IO.FileMode.Open)
                    Try
                        NXMLDataSetContainer.XMLDataSet.ReadXml(Stream)
                    Catch ex As Exception
                    Finally
                        Stream.Close()
                    End Try
                    m_DataSetCache.Add(DataSetName, NXMLDataSetContainer)

                End If



            Catch ex As Exception
            Finally
                ' NXMLDataSetContainer.Lock.ReleaseWriterLock()
            End Try


        End Sub
        Shared Function IncAutonumericValue(ByVal DataSetName As String, ByVal DataTableName As String, ByVal ColumnName As String) As Integer
            Dim NXMLDataSetContainer As XMLDataSetContainer = GetXMLDataSet(DataSetName)
            Dim incvalue As Integer
            NXMLDataSetContainer.Lock.AcquireReaderLock(10000)
            Try

                incvalue = NXMLDataSetContainer.IncAutonumericValue(DataTableName, ColumnName)
            Catch ex As Exception
            Finally
                NXMLDataSetContainer.Lock.ReleaseReaderLock()
            End Try


            Return incvalue
        End Function
    End Class
End Namespace