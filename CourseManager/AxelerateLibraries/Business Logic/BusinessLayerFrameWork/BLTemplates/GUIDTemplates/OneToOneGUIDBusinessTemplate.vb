Imports Microsoft.VisualBasic
Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class OneToOneGUIDBusinessTemplate( _
        Of tMasterObject As {GUIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {OneToOneGUIDBusinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits GUIDTemplate(Of tDetailObject)
        Implements IOneToOneGUIDBusinessTemplate(Of tMasterObject)

#Region "DataLayer Overrides"

        Public Overrides ReadOnly Property DataKey() As BLDataKey
            Get
                Dim NDataKey As New GUIDTemplate(Of tDetailObject).BOGUIDDataKey(DataLayerContextInfo)
                NDataKey.m_GUID = m_GUID
                Return NDataKey
            End Get
        End Property

#End Region


#Region "Business Object Data"
        <FieldMap(False, , , , "Master")> Protected m_MasterGUID As String = ""

        Private m_MasterObjectCache As tMasterObject = Nothing
#End Region

#Region "Business Properties and Methods"
        <ForeignObjectProperty("MasterID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Overridable Property Master() As tMasterObject Implements IOneToOneGUIDBusinessTemplate(Of tMasterObject).Master
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
                    m_MasterObjectCache = CType(value.Clone, tMasterObject)
                End If

                Dim Datakey As GUIDTemplate(Of tMasterObject).BOGUIDDataKey
                Datakey = CType(m_MasterObjectCache.DataKey, GUIDTemplate(Of tMasterObject).BOGUIDDataKey)
                m_MasterGUID = Datakey.m_GUID
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


#End Region

#Region "Factory Methods"

        Public Shared Function GetObjectByMasterGUID(ByVal MasterGUID As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tDetailObject
            Dim Criteria As New BLCriteria(GetType(tDetailObject))
            Dim MasterInstance As New tDetailObject()
            Criteria.AddBinaryExpression("MasterGUID_" + MasterInstance.DataLayer.DataLayerFieldSuffix, "MasterGUID", "=", MasterGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone)
            Return GetObject(Criteria)
        End Function

        Public Shared Function GetObjectByMaster(ByVal MasterObject As tMasterObject, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tDetailObject
            Return GetObjectByMasterGUID(MasterObject.GUID, NDataLayerContextInfo)
        End Function

#End Region

    End Class

    Public Interface IOneToOneGUIDBusinessTemplate(Of tMasterObject)

        Property Master() As tMasterObject

    End Interface
End Namespace