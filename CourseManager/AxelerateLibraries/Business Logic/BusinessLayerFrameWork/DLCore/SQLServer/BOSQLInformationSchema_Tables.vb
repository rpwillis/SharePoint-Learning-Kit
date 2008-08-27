Public Class BOSQLInformationSchema_Tables
    Inherits BLBusinessBase(Of BOSQLInformationSchema_Tables, SQLSchemaDataKey)

    Private Shared m_DataLayers As New Hashtable

    Public Overrides Property DataLayer() As DataLayerAbstraction
        Get
            'Private Shared m_DataLayer As 
            Dim vDataLayer As SQLDataLayer = CType(m_DataLayers(DataLayerContextInfo.DataSourceName), SQLDataLayer)
            If (vDataLayer Is Nothing) Then
                vDataLayer = New SQLDataLayer(GetType(BOSQLInformationSchema_Tables), "InformationSchema_Tables", "_ist", False, True, DataLayerContextInfo.DataSourceName)
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
            NDataKey.m_TableName = m_TableName
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
    <FieldMap(True)> Protected m_TableName As String = ""
    <FieldMap(False)> Protected m_TableType As String = ""
#End Region

#Region "Business Properties and Methods"

    Public ReadOnly Property TableName() As String
        Get
            Return m_TableName
        End Get
    End Property
#End Region



#Region "DataKey"
    Public Class SQLSchemaDataKey
        Inherits BLDataKey

        <DataKey("TableName")> Public m_TableName As String = ""

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
