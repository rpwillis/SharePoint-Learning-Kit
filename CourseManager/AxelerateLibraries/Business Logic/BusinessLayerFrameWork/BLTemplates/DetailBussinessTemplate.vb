Imports Microsoft.VisualBasic

Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class DetailBussinessTemplate(Of tMasterObject As {AutoNumericIDTemplate(Of tMasterObject), New}, tDetailObject As {DetailBussinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits AutoNumericIDTemplate(Of tDetailObject)

        <FieldMap(False, , , , "MasterObject")> Protected m_MasterID As Integer = 0

        <NonSerialized()> _
        Private m_MasterObjectCache As tMasterObject = Nothing

#Region "Business Properties and Methods"
        Public Property MasterID() As Integer
            Get
                Return m_MasterID
            End Get
            Set(ByVal value As Integer)
                m_MasterID = value
                MarkDirty()

            End Set
        End Property

        <ForeignObjectProperty("MasterID", "ID")> _
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
                    m_MasterObjectCache = CType(value.Clone, tMasterObject)
                End If

                Dim Datakey As AutoNumericIDTemplate(Of tMasterObject).BOAutoNumericIDDataKey
                Datakey = CType(m_MasterObjectCache.DataKey, AutoNumericIDTemplate(Of tMasterObject).BOAutoNumericIDDataKey)
                m_MasterID = Datakey.m_ID
                MarkDirty()
            End Set
        End Property
#End Region

#Region "BLBusinessBase Overrides"
        Public Overrides Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing Then
                Dim Master As tMasterObject = CType(ParentObject, tMasterObject)
                m_MasterID = Master.ID
            End If
            MyBase.BLUpdate(ParentObject)
        End Sub

        Public Overrides Sub BLInsert(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing Then
                Dim Master As tMasterObject = CType(ParentObject, tMasterObject)
                m_MasterID = Master.ID
            End If
            MyBase.BLInsert(ParentObject)
        End Sub

#End Region

    End Class

End Namespace

