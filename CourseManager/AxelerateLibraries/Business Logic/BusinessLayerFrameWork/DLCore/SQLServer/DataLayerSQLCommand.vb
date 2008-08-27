Imports Data_Access_Application_Block
Imports System.Security.Principal

Namespace DLCore

    ''' <summary>
    ''' This class allows the business objects to interact with the SQL database through the DataLayerCommandBase interface
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class DataLayerSQLCommand
        Inherits DataLayerCommandBase

        <NonSerialized()> Private m_SQLDataReader As SqlDataReader = Nothing
        Private m_SQLCommand As String = ""
        Private m_SQLCommandType As CommandType
        <NonSerialized()> Private m_SQLDataLayer As SQLDataLayer
        Private m_SQLParameters() As SqlParameter

        Public Sub New(ByVal Command As DataLayerSQLCommand, ByVal NParameters() As SqlParameter, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, Command.DataLayer)
            m_SQLDataLayer = Command.m_SQLDataLayer
            m_SQLCommand = Command.m_SQLCommand
            m_SQLCommandType = Command.m_SQLCommandType
            m_SQLParameters = NParameters
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub

        Public Sub New(ByVal DataLayer As SQLDataLayer, ByVal CommandText As String, ByVal CommandType As CommandType, _
            ByVal NParameters() As SqlParameter, ByVal DataSourceName As String)
            MyBase.New(DataSourceName, DataLayer)
            m_SQLDataLayer = DataLayer
            m_SQLCommand = CommandText
            m_SQLCommandType = CommandType
            m_SQLParameters = NParameters
            m_CommandState = CommandStateType.ReadyToExecuteState
        End Sub

        Public Property Parameters() As SqlParameter()
            Get
                Return m_SQLParameters
            End Get
            Set(ByVal value As SqlParameter())
                m_SQLParameters = value
            End Set
        End Property

#Region "DataLayerCommandBase Overrides"
        Public Overrides Function NextTable() As Boolean
            If CommandState = CommandStateType.DataRetrievalState Then
                Dim Finished As Boolean = m_SQLDataReader.Read
                Return Finished
            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
            Return False
        End Function

        Public Overrides Function NextRecord() As Boolean
            If CommandState = CommandStateType.DataRetrievalState Then
                Dim Finished As Boolean = m_SQLDataReader.Read
                Return Finished
            Else
                Throw New System.Exception(BLResources.Exception_CommandInvalidState)
            End If
            Return False
        End Function

        Protected Overrides Sub OnExecute()
            Dim m_context As WindowsImpersonationContext = Nothing
            Try
                m_context = WindowsIdentity.Impersonate(IntPtr.Zero)
                m_SQLDataReader = SqlHelper.ExecuteReader(ConnectionString(m_DataSourceName), m_SQLCommandType, _
            m_SQLCommand, m_SQLParameters)
                m_CommandState = CommandStateType.DataRetrievalState
            Finally
                If Not IsNothing(m_context) Then
                    m_context.Dispose()
                End If
            End Try
        End Sub

        Public Overrides Function ReadData(ByVal index As Integer) As Object
            Return m_SQLDataReader.GetValue(index)
        End Function

        Public Overrides Sub Finish()
            If (Not m_SQLDataReader Is Nothing) Then
                m_SQLDataReader.Close()
                m_SQLDataReader = Nothing
            End If
            m_CommandState = CommandStateType.Finished
        End Sub

        Public Overrides ReadOnly Property FieldCount() As Integer
            Get
                If Not m_SQLDataReader Is Nothing Then
                    Return m_SQLDataReader.FieldCount
                End If
                Return 0
            End Get
        End Property

        Public Overrides ReadOnly Property FieldName(ByVal pIndex As Integer) As String
            Get
                If Not m_SQLDataReader Is Nothing Then
                    Return m_SQLDataReader.GetName(pIndex)
                End If
                Return pIndex.ToString
            End Get
        End Property

#End Region
    End Class
End Namespace