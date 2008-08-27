Imports Data_Access_Application_Block
Imports System.Data.OleDb

Namespace DLCore
    Public Class DataLayerOLEDBCommand
        Inherits DataLayerCommandBase

        Private m_OLEDBDataReader As OleDbDataReader = Nothing
        Private m_OLEDBCommand As String = ""
        Private m_OLEDBCommandType As CommandType
        Private m_OLEDBDataLayer As OLEDBDataLayer
        Private m_OLEDBParamteres() As OleDbParameter

        Public Sub New(ByVal Command As DataLayerOLEDBCommand, ByVal NParameters() As OleDbParameter, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, Command.DataLayer)
            m_OLEDBDataLayer = Command.m_OLEDBDataLayer
            m_OLEDBCommand = Command.m_OLEDBCommand
            m_OLEDBCommandType = Command.m_OLEDBCommandType
            m_OLEDBParamteres = NParameters
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub

        Public Sub New(ByVal DataLayer As OLEDBDataLayer, ByVal CommandText As String, ByVal CommandType As CommandType, _
            ByVal NParameters() As OleDbParameter, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, DataLayer)
            m_OLEDBDataLayer = DataLayer
            m_OLEDBCommand = CommandText
            m_OLEDBCommandType = CommandType
            m_OLEDBParamteres = NParameters
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub

        Public Property Parameters() As OleDbParameter()
            Get
                Return m_OLEDBParamteres
            End Get
            Set(ByVal value As OleDbParameter())
                m_OLEDBParamteres = value
            End Set
        End Property

#Region "DataLayerCommandBase Overrides"
        Public Overrides Function NextTable() As Boolean
            If CommandState = CommandStateType.DataRetrievalState Then
                Dim Finished As Boolean = m_OLEDBDataReader.Read
                Return Finished
            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
            Return False
        End Function

        Public Overrides Function NextRecord() As Boolean
            If CommandState = CommandStateType.DataRetrievalState Then
                Dim Finished As Boolean = m_OLEDBDataReader.Read
                Return Finished
            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
            Return False
        End Function

        Protected Overrides Sub OnExecute()
            Dim cnn As OleDb.OleDbConnection
            cnn = New OleDb.OleDbConnection(ConnectionString(m_DataSourceName))


            If cnn.State <> ConnectionState.Open Then
                cnn.Open()
            End If

            Dim Command As New OleDbCommand(m_OLEDBCommand, cnn)

            If Not m_OLEDBParamteres Is Nothing Then
                For Each Parameter As OleDbParameter In m_OLEDBParamteres
                    Command.Parameters.Add(Parameter)
                Next
            End If

            Dim OLEDBDataReader As OleDb.OleDbDataReader = Command.ExecuteReader()
            m_OLEDBDataReader = OLEDBDataReader
            m_CommandState = CommandStateType.DataRetrievalState
        End Sub

        Public Overrides Function ReadData(ByVal index As Integer) As Object
            Return m_OLEDBDataReader.GetValue(index)
        End Function

        Public Overrides ReadOnly Property FieldCount() As Integer
            Get
                If Not m_OLEDBDataReader Is Nothing Then
                    Return m_OLEDBDataReader.FieldCount
                End If
                Return 0
            End Get
        End Property

        Public Overrides ReadOnly Property FieldName(ByVal pIndex As Integer) As String
            Get
                If Not m_OLEDBDataReader Is Nothing Then
                    Return m_OLEDBDataReader.GetName(pIndex)
                End If
                Return pIndex.ToString
            End Get
        End Property

        Public Overrides Sub Finish()
            m_OLEDBDataReader.Close()
            m_OLEDBDataReader = Nothing
            m_CommandState = CommandStateType.Finished
        End Sub
#End Region
    End Class
End Namespace