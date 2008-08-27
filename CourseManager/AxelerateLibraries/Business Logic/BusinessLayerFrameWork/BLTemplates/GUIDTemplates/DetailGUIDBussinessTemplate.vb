Imports Microsoft.VisualBasic

Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class DetailGUIDBussinessTemplate(Of tMasterObject As {GUIDTemplate(Of tMasterObject), New}, tDetailObject As {DetailGUIDBussinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits GUIDTemplate(Of tDetailObject)

        <FieldMap(False, , , , "MasterObject")> Protected m_MasterGUID As String = ""

        <NonSerialized()> _
        Private m_MasterObjectCache As tMasterObject = Nothing

#Region "Business Properties and Methods"
        Public Property MasterGUID() As String
            Get
                Return m_MasterGUID
            End Get
            Set(ByVal value As String)
                m_MasterGUID = value
                MarkDirty()

            End Set
        End Property

        <ForeignObjectProperty("MasterGUID", "GUID")> _
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
                    m_MasterObjectCache = CType(value.Clone, tMasterObject)
                End If

                Dim Datakey As GUIDTemplate(Of tMasterObject).BOGUIDDataKey
                Datakey = CType(m_MasterObjectCache.DataKey, GUIDTemplate(Of tMasterObject).BOGUIDDataKey)
                m_MasterGUID = Datakey.m_GUID
                MarkDirty()
            End Set
        End Property
#End Region

#Region "BLBusinessBase Overrides"
        Public Overrides Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing AndAlso ParentObject Is GetType(tMasterObject) Then
                Dim Master As tMasterObject = CType(ParentObject, tMasterObject)
                m_MasterGUID = Master.GUID
            End If
            MyBase.BLUpdate(ParentObject)
        End Sub

        Public Overrides Sub BLInsert(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing AndAlso GetType(tMasterObject).IsInstanceOfType(ParentObject) Then
                Dim Master As tMasterObject = CType(ParentObject, tMasterObject)
                m_MasterGUID = Master.GUID
            End If
            MyBase.BLInsert(ParentObject)
        End Sub

#End Region

#Region "Factory Methods"

        Public Shared Function GetObjectByMasterGUID(ByVal MasterGUID As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tDetailObject
            Dim Criteria As New BLCriteria(GetType(tDetailObject))
            Dim MasterInstance As New tDetailObject()
            Criteria.AddBinaryExpression("MasterGUID" + MasterInstance.DataLayer.DataLayerFieldSuffix, "MasterGUID", "=", MasterGUID, BLCriteriaExpression.BLCriteriaOperator.OperatorNone)
            Return GetObject(Criteria)
        End Function

        Public Shared Function GetObjectByMaster(ByVal MasterObject As tMasterObject, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tDetailObject
            Return GetObjectByMasterGUID(MasterObject.GUID, NDataLayerContextInfo)
        End Function

#End Region

    End Class

End Namespace

