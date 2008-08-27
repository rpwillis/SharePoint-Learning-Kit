Public Class BOSQLInformationSchema_Columns
    Inherits BLBusinessBase(Of BOSQLInformationSchema_Columns, SQLSchemaDataKey)

    Private Shared m_DataLayers As New Hashtable

    Public Overrides Property DataLayer() As DataLayerAbstraction
        Get
            'Private Shared m_DataLayer As 
            Dim vDataLayer As SQLDataLayer = CType(m_DataLayers(DataLayerContextInfo.DataSourceName), SQLDataLayer)
            If (vDataLayer Is Nothing) Then
                vDataLayer = New SQLDataLayer(GetType(BOSQLInformationSchema_Columns), "InformationSchema_Columns", "_isc", False, True, DataLayerContextInfo.DataSourceName)
                m_DataLayers.Add(DataLayerContextInfo.DataSourceName, vDataLayer)
            End If
            Return vDataLayer
        End Get
        Set(ByVal value As DataLayerAbstraction)
            MyBase.DataLayer = value
        End Set
    End Property

#Region "DataLayer Overrides"
    Public Overrides ReadOnly Property DataKey() As BLDataKey
        Get
            Dim NDataKey As New SQLSchemaDataKey(DataLayerContextInfo)
            NDataKey.m_ID = m_ID
            Return NDataKey
        End Get
    End Property

#End Region

#Region "Constructor"
    Public Sub New()
    End Sub
#End Region

#Region "Business Object Data"
    'Es necesario siempre inicializar los objetos con algun valor
    <FieldMap(True)> Protected m_ID As String = ""
    <FieldMap(False)> Protected m_TableName As String = ""
    <FieldMap(False)> Protected m_ColumnName As String = ""
    <FieldMap(False)> Protected m_Position As Integer = 0
    <FieldMap(False)> Protected m_DataType As String = ""
    <FieldMap(False)> Protected m_MaxLength As Integer = 0
#End Region

#Region "Business Properties and Methods"
    Public ReadOnly Property ID() As String
        Get
            Return m_ID
        End Get
    End Property

    Public ReadOnly Property TableName() As String
        Get
            Return m_TableName
        End Get
    End Property

    Public ReadOnly Property ColumnName() As String
        Get
            Return m_ColumnName
        End Get
    End Property

    Public ReadOnly Property Position() As Integer
        Get
            Return m_Position
        End Get
    End Property

    Public ReadOnly Property DataType() As String
        Get
            Return m_DataType
        End Get
    End Property

    Public ReadOnly Property MaxLenght() As Integer
        Get
            Return m_MaxLength
        End Get
    End Property

#End Region

#Region "DataKey"
    Public Class SQLSchemaDataKey
        Inherits BLDataKey

        <DataKey("ID")> Public m_ID As String = ""

        Public Sub New(Optional ByVal DataLayerContext As DataLayerContextInfo = Nothing)
            MyBase.New(GetType(BOSQLInformationSchema_Columns), DataLayerContext)
        End Sub

        Public Overrides ReadOnly Property NewDataKey() As BLDataKey
            Get
                Return New SQLSchemaDataKey(m_DataLayerContext)
            End Get
        End Property


    End Class



#End Region




End Class
