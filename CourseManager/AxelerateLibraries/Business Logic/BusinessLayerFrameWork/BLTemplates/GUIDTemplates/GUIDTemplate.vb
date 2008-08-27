Imports System.Xml.Serialization

Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class GUIDTemplate(Of _
        tBusinessObject As {GUIDTemplate(Of tBusinessObject), New})
        Inherits BLBusinessBase(Of tBusinessObject, BOGUIDDataKey)

#Region "DataLayer Overrides"

        Public Overrides ReadOnly Property DataKey() As BLDataKey
            Get
                Dim NDataKey As New BOGUIDDataKey(DataLayerContextInfo)
                NDataKey.m_GUID = m_GUID
                Return NDataKey
            End Get
        End Property

#End Region

#Region "Business Object Data"
        'Es necesario siempre inicializar los objetos con algun valor
        <FieldMap(True)> Protected m_GUID As String = System.Guid.NewGuid.ToString
#End Region

#Region "Business Properties and Methods"

        <StringLengthValidation(50), StringRequiredValidation()> _
        <XmlIgnore()> _
        Public Property GUID() As String
            Get
                Return m_GUID
            End Get
            Set(ByVal value As String)
                m_GUID = value
                PropertyHasChanged()
            End Set
        End Property

#End Region

#Region "Factory Methods"
        'Devuelve un nuevo objeto de negocios, que lee de la base de datos
        'utilizando su llave
        Public Shared Function GetObjectByGUID(ByVal GUID As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
            Dim DataKey As New BOGUIDDataKey(NDataLayerContextInfo)
            DataKey.m_GUID = GUID
            Dim Criteria As New BusinessObjectCriteria(DataKey)
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Return GetObject(Criteria)
        End Function

        'Devuelve un objeto de negocios, que lee de la base de datos
        'utilizando su llave o un nothing (null en c#) si no existe un objeto con ese guid
        Public Shared Function TryGetObjectByGUID(ByVal GUID As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
            'Dim DataKey As New BOGUIDDataKey(NDataLayerContextInfo)
            'DataKey.m_GUID = GUID
            'Dim Criteria As New BusinessObjectCriteria(DataKey)
            'Criteria.DataLayerContextInfo = NDataLayerContextInfo
            'Criteria.TryGet = True
            'Dim result As tBusinessObject = GetObject(Criteria)
            'If result.DataKey.isEmpty Then
            '    Return Nothing 'por que no se pudo leer desde la base de datos.
            'End If
            'Return result

            Try
                Return GetObjectByGUID(GUID, NDataLayerContextInfo)
            Catch ex As Exception
                Return Nothing
            End Try

        End Function

#End Region


#Region "DataKey"
        <Serializable()> _
        Public Class BOGUIDDataKey
            Inherits BLDataKey

            <DataKey("GUID")> Public m_GUID As String = ""

            Public Sub New(Optional ByVal DataLayerContext As DataLayerContextInfo = Nothing)
                MyBase.New(GetType(tBusinessObject), DataLayerContext)
            End Sub

            Public Overrides ReadOnly Property NewDataKey() As BLDataKey
                Get
                    Return New BOGUIDDataKey(m_DataLayerContext)
                End Get
            End Property


        End Class



#End Region


    End Class
End Namespace