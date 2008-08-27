Namespace DLCore

    ''' <summary>
    ''' Class that stores certain parameters of the business object building
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class DataLayerContextInfo

#Region "Private Data"
        ''' <summary>
        ''' Name of the table associated to the DataLayer
        ''' </summary>
        ''' <remarks></remarks>
        Private m_TableName As String = ""

        ''' <summary>
        ''' Name of the data source where operations for the objects DataLayer will take place
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DataSourceName As String = ""

        ''' <summary>
        ''' Type of DataLayer going to be used during the cosntruction of the object
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DataLayerType As System.Type = Nothing

        ''' <summary>
        ''' Should force an operation using a DataLayer locally created
        ''' </summary>
        ''' <remarks></remarks>
        Private m_ForceLocal As Boolean = False

        ''' <summary>
        ''' Type of business object used fir this DataLayerContextInfo
        ''' </summary>
        ''' <remarks></remarks>
        Private m_BusinessObjectType As Type = Nothing

        ''' <summary>
        ''' DataLayer that is going to be created could be obtained from the DataLayers cache
        ''' </summary>
        ''' <remarks></remarks>
        Private m_isCacheable As Boolean = True

        ''' <summary>
        ''' Fields to be written in case the object type is dynamic
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DynamicFields() As String
#End Region

#Region "Constructors"

        Public Sub New()

        End Sub

        Public Sub New(ByVal BusinessObjectType As Type)
            m_BusinessObjectType = ReflectionHelper.ResolveBusinessType(BusinessObjectType)
            If Not m_BusinessObjectType Is Nothing Then
                If DataLayerInfoAttribute.isDefined(m_BusinessObjectType) Then
                    Dim Attribute As DataLayerInfoAttribute = DataLayerInfoAttribute.GetAttribute(m_BusinessObjectType)
                    m_isCacheable = Not Attribute.HasDynamicFields
                End If
            End If
        End Sub

        ''' <summary>
        ''' Creates new object copy of NCreationInfo
        ''' </summary>
        ''' <param name="NCreationInfo"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal NCreationInfo As DataLayerContextInfo, Optional ByVal SourceBusinessObjectType As Type = Nothing)
            Copy(NCreationInfo, SourceBusinessObjectType)
        End Sub

        ''' <summary>
        ''' Creates new object copy of NCreationInfo
        ''' </summary>
        ''' <param name="NCreationInfo"></param>
        ''' <remarks></remarks>
        Public Sub Copy(ByVal NCreationInfo As DataLayerContextInfo, Optional ByVal SourceBusinessObjectType As Type = Nothing)
            m_BusinessObjectType = NCreationInfo.m_BusinessObjectType
            m_DataSourceName = NCreationInfo.m_DataSourceName
            m_DataLayerType = NCreationInfo.m_DataLayerType
            m_TableName = NCreationInfo.TableName
            m_isCacheable = NCreationInfo.m_isCacheable
            m_DynamicFields = NCreationInfo.m_DynamicFields
            m_ForceLocal = NCreationInfo.m_ForceLocal


            If Not SourceBusinessObjectType Is Nothing Then
                m_BusinessObjectType = SourceBusinessObjectType
            End If
        End Sub

#End Region

#Region "Public Properties"
        Public ReadOnly Property IsCacheable() As Boolean
            Get
                Return True
            End Get
        End Property

        Public ReadOnly Property IsDefaultDataLayer() As Boolean
            Get
                Return m_TableName = "" And m_DataSourceName = "" _
                    And m_DataLayerType Is Nothing And Not m_ForceLocal And m_BusinessObjectType Is Nothing
            End Get
        End Property

        Public ReadOnly Property BusinessType() As Type
            Get
                Return m_BusinessObjectType
            End Get
        End Property

        ''' <summary>
        ''' Gets or Sets the name of the data source where operations for the objects DataLayer will take place
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>If text begins with [DataSourceInstance] is taken as a Connection String</remarks>
        Public Property DataSourceName() As String
            Get
                Return m_DataSourceName
            End Get
            Set(ByVal value As String)
                m_DataSourceName = value
            End Set
        End Property

        Public Property TableName() As String
            Get
                Return m_TableName
            End Get
            Set(ByVal value As String)
                m_TableName = value
            End Set
        End Property


        Public Property DataLayerType() As System.Type
            Get
                Return m_DataLayerType
            End Get
            Set(ByVal value As System.Type)
                m_DataLayerType = value
            End Set
        End Property

        Public Property ForceLocal() As Boolean
            Get
                Return m_ForceLocal
            End Get
            Set(ByVal value As Boolean)
                m_ForceLocal = value

            End Set
        End Property

        ''' <summary>
        ''' Gets or Sets the fields to be written in case the object type is dynamic
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DynamicFields() As String()
            Get
                Return m_DynamicFields
            End Get

            Set(ByVal value As String())
                m_DynamicFields = value
            End Set
        End Property

        Public ReadOnly Property HasDynamicFields() As Boolean
            Get
                Return (Not m_DynamicFields Is Nothing) AndAlso (m_DynamicFields.Length > 0)
            End Get
        End Property

#End Region

#Region "Factory Methods"


        Public Shared Function GetLocalContextInfo(ByVal BOType As Type) As DataLayerContextInfo
            Dim NDataLayerContextInfo As New DataLayerContextInfo(BOType)
            NDataLayerContextInfo.ForceLocal = True
            Return NDataLayerContextInfo
        End Function


        Public Shared Function GetServerDynamicContextInfo(ByVal DataLayerType As Type, ByVal DataSourceName As String, ByVal TableName As String) As DataLayerContextInfo
            Dim NDataLayerContextInfo As New DataLayerContextInfo(GetType(BLDynamicBusinessObject))
            NDataLayerContextInfo.DataSourceName = DataSourceName
            NDataLayerContextInfo.TableName = TableName
            NDataLayerContextInfo.DataLayerType = DataLayerType
            Return NDataLayerContextInfo
        End Function



#End Region

#Region "System.Object Overrides"

        'Redefines the ToString method so it returns the unique identifier(Criteria) of the object
        Public Overrides Function ToString() As String

            Dim DataLayerTypeName As String = ""
            Dim BusinessTypeName As String = ""

            If Not m_DataLayerType Is Nothing Then
                DataLayerTypeName = DataLayerType.Name
            End If
            If Not m_BusinessObjectType Is Nothing Then
                BusinessTypeName = m_BusinessObjectType.Name
            End If

            Return "DataLayerType=" + DataLayerTypeName + ";BusinessType=" + _
                BusinessTypeName + ";DataSource=" + m_DataSourceName + ";TableName=" + m_TableName
        End Function

        'Redefines the method Equals in a way that they are consideed equal when
        'their unique identifiers(Criteria) are the same
        Public Overloads Function Equals(ByVal obj As DataLayerContextInfo) As Boolean
            Return obj.TableName = TableName And _
                obj.DataLayerType Is DataLayerType And _
                obj.DataSourceName = DataSourceName And _
                obj.BusinessType Is BusinessType
        End Function

        'Redefines the hash code for using the unique identifier(Criteria)
        Public Overloads Function GetHashCode() As Integer
            Return ToString.GetHashCode()
        End Function

#End Region


    End Class
End Namespace