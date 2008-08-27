Imports Microsoft.VisualBasic
Namespace BLCore.Templates

    <Serializable()> _
    Public MustInherit Class MNRelationBusinessTemplate( _
        Of tBusinessObject As {MNRelationBusinessTemplate(Of tBusinessObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {AutoNumericIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {AutoNumericIDTemplate(Of tDetailObject), New})
        Inherits AutoNumericIDTemplate(Of tBusinessObject)
        Implements IBLMNRelationBusinessBase

#Region "DataLayer Overrides"
#End Region

#Region "Business Object Data"
        <FieldMap(False, , , , "MasterObject")> Protected m_MasterID As Integer = 0
        <FieldMap(False, , , , "DetailObject")> Protected m_DetailID As Integer = 0

        Private m_MasterObjectCache As tMasterObject = Nothing
        Private m_DetailObjectCache As tDetailObject = Nothing

#End Region

#Region "Business Properties and Methods"
        <ForeignObjectProperty("MasterID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Property MasterObject() As tMasterObject
            Get
                Dim Refresh As Boolean = False
                Dim Datakey As New AutoNumericIDTemplate(Of tMasterObject).BOAutoNumericIDDataKey(DataLayerContextInfo)
                Datakey.m_ID = m_MasterID
                If m_MasterObjectCache Is Nothing Then
                    Refresh = True
                ElseIf Not m_MasterObjectCache.DataKey.Equals(Datakey) Then
                    Refresh = True
                End If

                If Refresh Then
                    m_MasterObjectCache = AutoNumericIDTemplate(Of tMasterObject).GetObject(Datakey, DataLayerContextInfo)
                End If

                Return m_MasterObjectCache
            End Get

            Set(ByVal value As tMasterObject)
                If value Is Nothing Then
                    m_MasterObjectCache = New tMasterObject
                Else
                    m_MasterObjectCache = value.Clone
                End If

                Dim Datakey As AutoNumericIDTemplate(Of tMasterObject).BOAutoNumericIDDataKey
                Datakey = CType(m_MasterObjectCache.DataKey, AutoNumericIDTemplate(Of tMasterObject).BOAutoNumericIDDataKey)
                m_MasterID = Datakey.m_ID
                PropertyHasChanged()
            End Set
        End Property

        <ForeignObjectProperty("DetailID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Property DetailObject() As tDetailObject
            Get
                Dim Refresh As Boolean = False
                Dim Datakey As New AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey(DataLayerContextInfo)
                Datakey.m_ID = m_DetailID
                If m_DetailObjectCache Is Nothing Then
                    Refresh = True
                ElseIf Not m_DetailObjectCache.DataKey.Equals(Datakey) Then
                    Refresh = True
                End If

                If Refresh Then
                    m_DetailObjectCache = AutoNumericIDTemplate(Of tDetailObject).GetObject(Datakey, DataLayerContextInfo)
                End If

                Return m_DetailObjectCache
            End Get

            Set(ByVal value As tDetailObject)
                If value Is Nothing Then
                    m_DetailObjectCache = New tDetailObject
                Else
                    m_DetailObjectCache = CType(value.Clone, tDetailObject)
                End If

                Dim Datakey As AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey
                Datakey = CType(m_DetailObjectCache.DataKey, AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey)
                m_DetailID = Datakey.m_ID

                PropertyHasChanged()
            End Set
        End Property

        Public Property MasterID() As Integer
            Get
                Return m_MasterID
            End Get
            Set(ByVal value As Integer)
                m_MasterID = value
                MarkDirty()

            End Set
        End Property

        Public Property DetailID() As Integer
            Get
                Return m_DetailID
            End Get
            Set(ByVal value As Integer)
                m_DetailID = value
                MarkDirty()

            End Set
        End Property

#End Region
#Region "IBLMNRelationBusinessBase"
        Public ReadOnly Property MasterDataKey() As Object Implements IBLMNRelationBusinessBase.MasterDataKey
            Get
                Return MasterID
            End Get
        End Property

        Public ReadOnly Property DetailDataKey() As Object Implements IBLMNRelationBusinessBase.DetailDataKey
            Get
                Return DetailID
            End Get
        End Property
#End Region
    End Class
End Namespace