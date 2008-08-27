Imports System.Configuration.ConfigurationManager
Imports System.Configuration
Namespace DLCore



    ''' <summary>
    ''' The DataLayerCommandBase class provides an abstraction for a command 
    ''' (simple or complex data operation that can return data in form of tables) to be performed on the data store.  If the command has a result, the data can be obtained in the same way it is obtained from a typical DataReader.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class DataLayerCommandBase

#Region "Private Object Data"
        Private m_DataLayer As DataLayerAbstraction
        'Connection String is used for connecting to the database
        Private m_ConnectionString As String = ""
        Protected m_DataSourceName As String = ""

#End Region

#Region "Public Properties and Methods"
        Public ReadOnly Property DataLayer() As DataLayerAbstraction
            Get
                Return m_DataLayer
            End Get
        End Property

#End Region

#Region "Command State"

        Protected m_CommandState As CommandStateType = CommandStateType.InvalidState

        Public ReadOnly Property CommandState() As CommandStateType
            Get
                Return m_CommandState
            End Get
        End Property

#End Region

#Region "Execution"
        'Executes the command
        Public Sub Execute()
            If m_CommandState = CommandStateType.ReadyToExecuteState Then
                OnExecute()
            Else
                Throw New Exception(BLResources.Exception_CommandInvalidState)
            End If

        End Sub

        ''' <summary>
        ''' Returns the conecction string used for connecting to the database
        ''' </summary>
        ''' <param name="DataSourceName"> Contains a string that makes reference to the name a connection String in the local machine registry</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property ConnectionString(ByVal DataSourceName As String) As String
            Get
                If m_ConnectionString = "" Then
                    Dim InstanceIdentifier As String = "[DataSourceInstance]"
                    If DataSourceName.StartsWith(InstanceIdentifier) Then
                        m_ConnectionString = DataSourceName.Substring(InstanceIdentifier.Length)
                    Else
                        If DataSourceName <> "" Then
                            Dim ConnectionStringSetting As ConnectionStringSettings
                            ConnectionStringSetting = ConnectionStrings(DataSourceName)
                            If ConnectionStringSetting Is Nothing Then
                                Return DataLayerAbstraction.GetConnectionString(DataSourceName).ConnectionString
                            End If
                            m_ConnectionString = ConnectionStrings(DataSourceName).ConnectionString
                        Else
                            m_ConnectionString = ConnectionStrings(0).ConnectionString
                        End If

                    End If
                End If
                Return m_ConnectionString
            End Get
        End Property



        'Must overridable method for implementing the command behavior
        Protected MustOverride Sub OnExecute()

        Public MustOverride Sub Finish()




#End Region

#Region "Data Retrieval"

        'Posicions the returned data by the command in the next table. When it is executed
        'automatically is posicioned in the first table.
        'Returns true as long as more tables exist
        Public MustOverride Function NextTable() As Boolean

        'Posicions the returned data by the command in the next register. Should be call 
        'once before reading the first register of the table.
        'Returns true as long more data exist
        Public MustOverride Function NextRecord() As Boolean

        'Reads a field of the registry where it is currently positioned
        Public MustOverride Function ReadData(ByVal index As Integer) As Object

        Public MustOverride ReadOnly Property FieldCount() As Integer

        ''' <summary>
        ''' Returns the Name of the field located at an specific Index
        ''' </summary>
        ''' <param name="pIndex">Index of the Field to be returned</param>
        ''' <value></value>
        ''' <returns>Field Name</returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property FieldName(ByVal pIndex As Integer) As String


#End Region

#Region "CommandStateType"

        Public Enum CommandStateType As Integer
            'The command hasn't been properly initialized
            InvalidState = 0

            'The command has been inicialized and is ready to be executed
            ReadyToExecuteState = 1

            'The command has been correctly executed and has generated data, it can be extracted
            DataRetrievalState = 2

            'All the data has been read and the command succesfully ended the execution
            Finished = 3

        End Enum
#End Region

#Region "Constructor"
        Public Sub New(ByVal DataSourceName As String, ByVal NDataLayer As DataLayerAbstraction)
            m_DataLayer = NDataLayer
            If DataSourceName = "" Then
                If m_DataLayer.DefaultDataSourceName = "" Then
                    m_DataSourceName = ConnectionStrings(0).Name
                Else
                    m_DataSourceName = m_DataLayer.DefaultDataSourceName
                End If
            Else
                m_DataSourceName = DataSourceName
            End If
        End Sub

        ''' <summary>
        ''' Creates a new Data Layer Command that is not directly asociated to a specific DataLayer
        ''' </summary>
        ''' <param name="DataSourceName">Name of the DataSource where the command will be issued </param>
        ''' <remarks></remarks>
        Public Sub New(ByVal DataSourceName As String)
            m_DataLayer = Nothing
            m_DataSourceName = DataSourceName
        End Sub
#End Region
    End Class
End Namespace