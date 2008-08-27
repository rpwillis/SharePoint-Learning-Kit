Imports Data_Access_Application_Block

Namespace DLCore
    Public MustInherit Class DataLayerXMLCommand
        Inherits DataLayerCommandBase

        Private m_DataLayer As XMLDataLayer = Nothing
        Private m_Criteria As BLCriteria = Nothing
        Private m_DataSetXML As DataSet = Nothing
        Private m_BusinessObject As BLBusinessBase = Nothing
        Private m_RecordIndex As Integer = -1
        Private m_TableIndex As Integer = 0
 

        Public Shadows Property DataLayer() As XMLDataLayer
            Get
                Return m_DataLayer
            End Get
            Set(ByVal Value As XMLDataLayer)
                m_DataLayer = Value
            End Set
        End Property

        Public Property Criteria() As BLCriteria
            Get
                Return m_Criteria
            End Get
            Set(ByVal Value As BLCriteria)
                m_Criteria = Value
            End Set
        End Property

        Public Property DataSetXML() As DataSet
            Get
                Return m_DataSetXML
            End Get
            Set(ByVal Value As DataSet)
                m_DataSetXML = Value
            End Set
        End Property

        Public Property BusinessObject() As BLBusinessBase
            Get
                Return m_BusinessObject
            End Get
            Set(ByVal Value As BLBusinessBase)
                m_BusinessObject = Value
            End Set
        End Property

        Public Sub New(ByVal Command As DataLayerXMLCommand, ByVal NewCriteria As BLCriteria, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, Command.DataLayer)
            DataLayer = Command.DataLayer
            'm_DataSet = Command.m_DataSet
            Criteria = NewCriteria
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub

        Public Sub New(ByVal NewDataLayer As XMLDataLayer, ByVal NewCriteria As BLCriteria, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, NewDataLayer)
            DataLayer = NewDataLayer
            Criteria = NewCriteria
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub



#Region "DataLayerCommandBase Overrides"
        Public Overrides Function NextTable() As Boolean

            If CommandState = CommandStateType.DataRetrievalState Then
                If DataSetXML Is Nothing Then
                Else
                    If DataSetXML.Tables.Count > m_TableIndex + 1 Then
                        m_TableIndex = m_TableIndex + 1
                        m_RecordIndex = -1
                        Return True
                    End If
                End If

            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
            Return False
        End Function

        Public Overrides Function NextRecord() As Boolean
            If CommandState = CommandStateType.DataRetrievalState Then
                If DataSetXML Is Nothing Then
                Else
                    If DataSetXML.Tables.Count > m_TableIndex Then
                        Dim Table As DataTable = DataSetXML.Tables(m_TableIndex)
                        If Table.Rows.Count > m_RecordIndex + 1 Then
                            m_RecordIndex = m_RecordIndex + 1
                            Return True
                        End If
                    End If
                End If
                Return False
            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
                Return False
        End Function

        'Protected Overrides Sub OnExecute()
        '    m_CommandState = CommandStateType.DataRetrievalState
        'End Sub

        Public Overrides Function ReadData(ByVal index As Integer) As Object
            If DataSetXML Is Nothing Then
            Else
                If DataSetXML.Tables.Count > m_TableIndex Then
                    Dim Table As DataTable = DataSetXML.Tables(m_TableIndex)
                    If Table.Rows.Count > m_RecordIndex Then
                        Dim Row As DataRow = Table.Rows(m_RecordIndex)
                        Dim DataType As System.Type = Table.Columns(index).DataType
                        Return System.Convert.ChangeType(Row(index), DataType)
                    End If
                End If
            End If
            Return Nothing

        End Function

        Public Overrides ReadOnly Property FieldCount() As Integer
            Get
                If (m_TableIndex >= 0) Then
                    Return DataSetXML.Tables(m_TableIndex).Columns.Count
                End If
                Return 0
            End Get
        End Property

        Public Overrides ReadOnly Property FieldName(ByVal pIndex As Integer) As String
            Get
                If (m_TableIndex >= 0) Then
                    Return DataSetXML.Tables(m_TableIndex).Columns(pIndex).ColumnName
                End If
                Return pIndex.ToString
            End Get
        End Property


        'Public Overrides Sub Finish()

        'm_CommandState = CommandStateType.Finished
        'End Sub
#End Region
    End Class
End Namespace