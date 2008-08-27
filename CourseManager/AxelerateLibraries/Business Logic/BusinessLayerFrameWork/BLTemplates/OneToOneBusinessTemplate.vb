Imports Microsoft.VisualBasic
Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class OneToOneBusinessTemplate( _
        Of tMasterObject As {AutoNumericIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {OneToOneBusinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits AutoNumericIDTemplate(Of tDetailObject)

#Region "DataLayer Overrides"

        Public Overrides ReadOnly Property DataKey() As BLDataKey
            Get
                Dim NDataKey As New AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey(DataLayerContextInfo)
                NDataKey.m_ID = m_ID
                Return NDataKey
            End Get
        End Property

#End Region


#Region "Business Object Data"
        <FieldMap(False, , , , "Master")> Protected m_MasterID As Integer = 0

        Private m_MasterObjectCache As tMasterObject = Nothing
#End Region

#Region "Business Properties and Methods"
        <ForeignObjectProperty("MasterID", "ID"), _
        ValidationAttributes.DuplicateKeyValidation()> _
        Public Property Master() As tMasterObject
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


#End Region



    End Class
End Namespace