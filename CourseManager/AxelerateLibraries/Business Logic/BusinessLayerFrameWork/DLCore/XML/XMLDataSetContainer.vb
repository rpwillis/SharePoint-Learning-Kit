Imports System.Threading
Public Class XMLDataSetContainer
    'Protected m_AccessCount As Integer = 0
    Protected m_Name As String = ""
    Protected m_XMLDataSet As DataSet = Nothing
    Protected m_DataTableInformations As Hashtable = Nothing
    Protected m_Lock As New ReaderWriterLock
    'Public Property AccessCount() As Integer
    '   Get
    '      Return m_AccessCount
    ' End Get
    'Set(ByVal value As Integer)
    '   m_AccessCount = value
    '  End Set
    'End Property
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            m_Name = value
        End Set
    End Property

    Public Property XMLDataSet() As DataSet
        Get
            Return m_XMLDataSet
        End Get
        Set(ByVal value As DataSet)
            m_XMLDataSet = value
        End Set
    End Property

    Public Property DataTableInformations() As Hashtable
        Get
            Return m_DataTableInformations
        End Get
        Set(ByVal value As Hashtable)
            m_DataTableInformations = value
        End Set
    End Property
    Public Property Lock() As ReaderWriterLock
        Get
            Return m_Lock
        End Get
        Set(ByVal value As ReaderWriterLock)
            m_Lock = value
        End Set
    End Property
    Public Sub New(ByVal DataSetName As String)
        m_Name = DataSetName
        m_XMLDataSet = New DataSet(DataSetName)
        m_DataTableInformations = New Hashtable

    End Sub

    Public Sub AddTableInformation(ByVal nDataTable As DataTable, ByVal FieldMapList As BLFieldMapList)
        If DataTableInformations.Contains(nDataTable.TableName) = False Then
            Dim NDataTableInformation As New DataTableInformation(nDataTable.TableName)
            Dim FieldMap As BLFieldMap
            For Each FieldMap In FieldMapList.OrderedFields
                If ((FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField) And _
                (FieldMap.AutoNumericType = BLFieldMap.AutoNumericTypeEnum.IdentityColumn)) Then
                    Dim AutonumericValue As Integer = 0
                    For Each Row As DataRow In nDataTable.Rows
                        Dim ActualValue As Integer = CInt(Row(FieldMap.DLFieldName))
                        If ActualValue > AutonumericValue Then
                            AutonumericValue = ActualValue
                        End If
                    Next
                    If NDataTableInformation.AutonumericValues Is Nothing Then
                        NDataTableInformation.AutonumericValues = New Hashtable
                    End If
                    NDataTableInformation.AutonumericValues.Add(FieldMap.DLFieldName, AutonumericValue)
                End If
            Next
            DataTableInformations.Add(nDataTable.TableName, NDataTableInformation)
        End If
    End Sub
    Public Function IncAutonumericValue(ByVal DataTableName As String, ByVal ColumnName As String) As Integer
        Dim nDataTableInformation As DataTableInformation = CType(DataTableInformations(DataTableName), DataTableInformation)
        Dim AutonumericValue As Integer = CInt(nDataTableInformation.AutonumericValues(ColumnName))
        AutonumericValue = AutonumericValue + 1
        nDataTableInformation.AutonumericValues(ColumnName) = AutonumericValue
        Return AutonumericValue
    End Function
End Class
