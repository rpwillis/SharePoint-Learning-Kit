Imports Microsoft.VisualBasic
Namespace BLCore.Templates

    <Serializable()> _
    Public MustInherit Class MNGUIDRelationBusinessTemplate( _
        Of tBusinessObject As {MNGUIDRelationBusinessTemplate(Of tBusinessObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {GUIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {GUIDTemplate(Of tDetailObject), New})
        Inherits GUIDTemplate(Of tBusinessObject)
        Implements IBLMNRelationBusinessBase

#Region "DataLayer Overrides"
#End Region

#Region "Business Object Data"
        <FieldMap(False, , , , "MasterObject")> Protected m_MasterGUID As String = ""
        <FieldMap(False, , , , "DetailObject")> Protected m_DetailGUID As String = ""

        Private m_MasterObjectCache As tMasterObject = Nothing
        Private m_DetailObjectCache As tDetailObject = Nothing

#End Region

#Region "Business Properties and Methods"
        <ForeignObjectProperty("MasterID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Overridable Property MasterObject() As tMasterObject
            Get
                Dim Refresh As Boolean = False
                Dim Datakey As New GUIDTemplate(Of tMasterObject).BOGUIDDataKey(DataLayerContextInfo)
                Datakey.m_GUID = m_MasterGUID
                If m_MasterObjectCache Is Nothing Then
                    Refresh = True
                ElseIf Not m_MasterObjectCache.DataKey.Equals(Datakey) Then
                    Refresh = True
                End If

                If Refresh Then
                    m_MasterObjectCache = GUIDTemplate(Of tMasterObject).GetObject(Datakey, DataLayerContextInfo)
                End If

                Return m_MasterObjectCache
            End Get

            Set(ByVal value As tMasterObject)
                If value Is Nothing Then
                    m_MasterObjectCache = New tMasterObject
                Else
                    m_MasterObjectCache = value.Clone
                End If

                Dim Datakey As GUIDTemplate(Of tMasterObject).BOGUIDDataKey
                Datakey = CType(m_MasterObjectCache.DataKey, GUIDTemplate(Of tMasterObject).BOGUIDDataKey)
                m_MasterGUID = Datakey.m_GUID
                PropertyHasChanged()
            End Set
        End Property

        <ForeignObjectProperty("DetailID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Overridable Property DetailObject() As tDetailObject
            Get
                Dim Refresh As Boolean = False
                Dim Datakey As New GUIDTemplate(Of tDetailObject).BOGUIDDataKey(DataLayerContextInfo)
                Datakey.m_GUID = m_DetailGUID
                If m_DetailObjectCache Is Nothing Then
                    Refresh = True
                ElseIf Not m_DetailObjectCache.DataKey.Equals(Datakey) Then
                    Refresh = True
                End If

                If Refresh Then
                    m_DetailObjectCache = GUIDTemplate(Of tDetailObject).GetObject(Datakey, DataLayerContextInfo)
                End If

                Return m_DetailObjectCache
            End Get

            Set(ByVal value As tDetailObject)
                If value Is Nothing Then
                    m_DetailObjectCache = New tDetailObject
                Else
                    m_DetailObjectCache = CType(value.Clone, tDetailObject)
                End If

                Dim Datakey As GUIDTemplate(Of tDetailObject).BOGUIDDataKey
                Datakey = CType(m_DetailObjectCache.DataKey, GUIDTemplate(Of tDetailObject).BOGUIDDataKey)
                m_DetailGUID = Datakey.m_GUID

                PropertyHasChanged()
            End Set
        End Property

        Public Property MasterGUID() As String
            Get
                Return m_MasterGUID
            End Get
            Set(ByVal value As String)
                m_MasterGUID = value
                MarkDirty()

            End Set
        End Property

        Public Property DetailGUID() As String
            Get
                Return m_DetailGUID
            End Get
            Set(ByVal value As String)
                m_DetailGUID = value
                MarkDirty()

            End Set
        End Property

#End Region

#Region "IBLMNRelationBusinessBase"
        Public ReadOnly Property MasterDataKey() As Object Implements IBLMNRelationBusinessBase.MasterDataKey
            Get
                Return MasterGUID
            End Get
        End Property

        Public ReadOnly Property DetailDataKey() As Object Implements IBLMNRelationBusinessBase.DetailDataKey
            Get
                Return DetailGUID
            End Get
        End Property
#End Region

    End Class
End Namespace