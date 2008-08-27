Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class AutoNumericIDTemplate(Of _
        tBusinessObject As {AutoNumericIDTemplate(Of tBusinessObject), New})
        Inherits BLBusinessBase(Of tBusinessObject, BOAutoNumericIDDataKey)

#Region "DataLayer Overrides"

        Public Overrides ReadOnly Property DataKey() As BLDataKey
            Get
                Dim NDataKey As New BOAutoNumericIDDataKey(DataLayerContextInfo)
                NDataKey.m_ID = m_ID
                Return NDataKey
            End Get
        End Property

#End Region

#Region "Business Object Data"
        'Es necesario siempre inicializar los objetos con algun valor
        <FieldMap(True, , , BLFieldMap.AutoNumericTypeEnum.IdentityColumn)> Protected m_ID As Integer = 0
#End Region

#Region "Business Properties and Methods"

        Public Property ID() As Integer
            Get
                Return m_ID
            End Get
            Set(ByVal value As Integer)
                m_ID = value
                MarkDirty()
            End Set
        End Property

#End Region

#Region "Factory Methods"
        'Devuelve un nuevo objeto de negocios vacio, que lee de la base de datos
        'utilizando su llave
        Public Shared Function GetObjectByID(ByVal ID As Integer, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
            Dim DataKey As New BOAutoNumericIDDataKey(NDataLayerContextInfo)
            DataKey.m_ID = ID
            Dim Criteria As New BusinessObjectCriteria(DataKey)
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Return GetObject(Criteria)
        End Function

        'TODO: Re-Define the funtion logic 

        'Devuelve un nuevo objeto de negocios, que lee de la base de datos
        'utilizando su llave o nothing (null en c#) si el objeto no existe en la base de datos.
        Public Shared Function TryGetObjectByID(ByVal ID As Integer, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
            'Dim DataKey As New BOAutoNumericIDDataKey(NDataLayerContextInfo)
            'DataKey.m_ID = ID
            'Dim Criteria As New BusinessObjectCriteria(DataKey)
            'Criteria.DataLayerContextInfo = NDataLayerContextInfo
            'Criteria.TryGet = True
            'Dim result As tBusinessObject
            'result = GetObject(Criteria)
            'If result.IsNew Then
            '    Return Nothing
            'End If
            'Return result

            Try
                Return GetObjectByID(ID, NDataLayerContextInfo)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

#End Region


#Region "DataKey"
        <Serializable()> _
        Public Class BOAutoNumericIDDataKey
            Inherits BLDataKey

            <DataKey("ID")> Public m_ID As Integer = 0

            Public Sub New(Optional ByVal DataLayerContext As DataLayerContextInfo = Nothing)
                MyBase.New(GetType(tBusinessObject), DataLayerContext)
            End Sub

            Public Overrides ReadOnly Property NewDataKey() As BLDataKey
                Get
                    Return New BOAutoNumericIDDataKey(m_DataLayerContext)
                End Get
            End Property


        End Class



#End Region


    End Class
End Namespace