Imports BusinessLayerFrameWork.BLCore.Attributes
Imports System.Collections

Namespace BLCore

    Public Class BLFieldMapList
        'Inherits Hashtable

#Region "Collections"
        Private m_OrderedFields As New ArrayList
        Private m_HashedFields As New Hashtable
#End Region



#Region "Cache Data"
        Private m_DataFetchFieldsCache As List(Of BLFieldMap)
        Private m_DynamicDataFieldsCache As List(Of BLFieldMap)
        Private m_ClassFetchFieldsCache As List(Of BLFieldMap)
        Private m_DataUpdateFieldsCache As List(Of BLFieldMap)
        Private m_ClassUpdateFieldsCache As List(Of BLFieldMap)
        Private m_AutoNumericRelevantFieldsCache As List(Of BLFieldMap)
        Private m_AutoNumericFieldsCache As List(Of BLFieldMap)

        Private m_KeyFieldsCache As List(Of BLFieldMap)
        Private m_OnCreationCachedFieldsCache As List(Of BLFieldMap)

        Public Sub ClearCaches()
            SyncLock Me
                If Not m_DataFetchFieldsCache Is Nothing Then
                    m_DataFetchFieldsCache = Nothing
                End If

                If Not m_DynamicDataFieldsCache Is Nothing Then
                    m_DynamicDataFieldsCache = Nothing
                End If

                If Not m_ClassFetchFieldsCache Is Nothing Then
                    m_ClassFetchFieldsCache = Nothing
                End If

                If Not m_DataUpdateFieldsCache Is Nothing Then
                    m_DataUpdateFieldsCache = Nothing
                End If

                If Not m_ClassUpdateFieldsCache Is Nothing Then
                    m_ClassUpdateFieldsCache = Nothing
                End If

                If Not m_AutoNumericRelevantFieldsCache Is Nothing Then
                    m_AutoNumericRelevantFieldsCache = Nothing
                End If

                If Not m_AutoNumericFieldsCache Is Nothing Then
                    m_AutoNumericFieldsCache = Nothing
                End If

                If Not m_AutoNumericFieldsCache Is Nothing Then
                    m_AutoNumericFieldsCache = Nothing
                End If

                If Not m_OnCreationCachedFieldsCache Is Nothing Then
                    m_OnCreationCachedFieldsCache = Nothing
                End If
            End SyncLock
        End Sub


        'Regresa una colección con los FieldMap cuyo FieldMapType es Data y son Fetch
        Public ReadOnly Property DataFetchFields() As List(Of BLFieldMap)
            Get
                If m_DataFetchFieldsCache Is Nothing Then
                    SyncLock Me
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Or BLFieldMap.FieldMapTypeEnum.DynamicDataField
                        Criteria.isFetch = True
                        m_DataFetchFieldsCache = GetFields(Criteria)
                    End SyncLock
                End If
                Return m_DataFetchFieldsCache
            End Get
        End Property

        'Regresa una colección con los FieldMap cuyo FieldMapType es DynamicData y son Fetch
        Public ReadOnly Property DynamicDataFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_DynamicDataFieldsCache Is Nothing Then

                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.FieldMapType = BLFieldMap.FieldMapTypeEnum.DynamicDataField
                        m_DynamicDataFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_DynamicDataFieldsCache
            End Get
        End Property


        'Regresa una colección con los FieldMap cuyo BusinessClassField es BusinessClass o BusinessCollection y son Fetch
        Public ReadOnly Property ClassFetchFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_ClassFetchFieldsCache Is Nothing Then

                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.FieldMapType = BLFieldMap.FieldMapTypeEnum.BusinessClassField Or BLFieldMap.FieldMapTypeEnum.BusinessCollectionField
                        Criteria.isFetch = True
                        m_ClassFetchFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_ClassFetchFieldsCache
            End Get
        End Property

        'Regresa una colección con los FieldMap cuyo FieldMapType es Data y son Update
        Public ReadOnly Property DataUpdateFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_DataUpdateFieldsCache Is Nothing Then
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Or BLFieldMap.FieldMapTypeEnum.DynamicDataField
                        Criteria.isUpdate = True
                        m_DataUpdateFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_DataUpdateFieldsCache

            End Get
        End Property

        'Regresa una colección con los FieldMap cuyo BusinessClassField es BusinessClass o BusinessCollection y son Update
        Public ReadOnly Property ClassUpdateFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_ClassUpdateFieldsCache Is Nothing Then
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.FieldMapType = BLFieldMap.FieldMapTypeEnum.BusinessClassField Or BLFieldMap.FieldMapTypeEnum.BusinessCollectionField
                        Criteria.isUpdate = True
                        m_ClassUpdateFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_ClassUpdateFieldsCache
            End Get
        End Property

        'Regresa una colección con los FieldMap que son AutoNumericRelevant
        Public ReadOnly Property AutoNumericRelevantFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_AutoNumericRelevantFieldsCache Is Nothing Then
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.isAutonumericRelevant = True
                        m_AutoNumericRelevantFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_AutoNumericRelevantFieldsCache
            End Get
        End Property

        'Regresa una colección con los FieldMap que son AutoNumeric
        Public ReadOnly Property AutoNumericFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_AutoNumericRelevantFieldsCache Is Nothing Then
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.AutonumericType = BLFieldMap.AutoNumericTypeEnum.IdentityColumn Or BLFieldMap.AutoNumericTypeEnum.GeneratedColumn
                        m_AutoNumericRelevantFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_AutoNumericRelevantFieldsCache
            End Get
        End Property



        Public ReadOnly Property KeyFields() As List(Of BLFieldMap)
            Get
                SyncLock Me
                    If m_KeyFieldsCache Is Nothing Then
                        Dim Criteria As New BLFieldMap.FieldMapCriteria
                        Criteria.isKey = True
                        m_KeyFieldsCache = GetFields(Criteria)
                    End If
                End SyncLock
                Return m_KeyFieldsCache
            End Get
        End Property

        Public ReadOnly Property OnCreationCachedFields(ByVal Criteria As BLCriteria) As List(Of BLFieldMap)
            Get

                If Criteria.NumCachedObjects = 0 And Not Criteria.UseAllCacheFields Then
                    SyncLock Me
                        If m_OnCreationCachedFieldsCache Is Nothing Then
                            Dim ToReturn As New List(Of BLFieldMap)

                            For Each FieldMap As BLFieldMap In m_OrderedFields
                                If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.CachedObjectField Then
                                    Dim Attr As Attributes.CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(FieldMap.Field)
                                    If Attr.LoadType = CachedForeignObjectAttribute.CachedObjectLoadType.OnCreation Then
                                        ToReturn.Add(FieldMap)
                                    End If
                                End If
                            Next

                            m_OnCreationCachedFieldsCache = ToReturn
                        End If
                    End SyncLock
                    Return m_OnCreationCachedFieldsCache
                Else
                    Dim ToReturn As New List(Of BLFieldMap)
                    SyncLock Me


                        For Each FieldMap As BLFieldMap In m_OrderedFields
                            If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.CachedObjectField Then
                                Dim Attr As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(FieldMap.Field)
                                If Attr.LoadType = CachedForeignObjectAttribute.CachedObjectLoadType.OnCreation Or Criteria.UseAllCacheFields Or Criteria.isCachedObjectLoaded(Attr.PropertyName) Then
                                    ToReturn.Add(FieldMap)
                                End If
                            End If
                        Next
                        m_OnCreationCachedFieldsCache = ToReturn
                    End SyncLock

                    Return ToReturn

                End If


            End Get
        End Property

#End Region

#Region "HashTable Overrides"
        Public Sub New()
            MyBase.New()
        End Sub

        'Agrega un nuevo campo a la lista de campos
        Public Overloads Sub Add(ByVal FieldMap As BLFieldMap)
            SyncLock Me
                If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.CachedObjectField Then
                    m_HashedFields.Add("Cached_" + FieldMap.NeutralFieldName, FieldMap)
                Else
                    m_HashedFields.Add(FieldMap.NeutralFieldName, FieldMap)
                End If
                m_OrderedFields.Add(FieldMap)
                m_DataFetchFieldsCache = Nothing
                m_ClassFetchFieldsCache = Nothing
                m_DataUpdateFieldsCache = Nothing
                m_ClassUpdateFieldsCache = Nothing
                m_KeyFieldsCache = Nothing
                m_AutoNumericFieldsCache = Nothing
                m_OnCreationCachedFieldsCache = Nothing
                m_AutoNumericRelevantFieldsCache = Nothing
            End SyncLock
        End Sub

        Default Public Property Item(ByVal Index As String) As Object
            Get
                Return m_HashedFields(Index)
            End Get
            Set(ByVal value As Object)
                m_HashedFields(Index) = value
            End Set
        End Property

        Public Function ContainsKey(ByVal Key As String) As Boolean
            Return m_HashedFields.ContainsKey(Key)
        End Function

        Public Function Count() As Integer
            Return m_HashedFields.Count
        End Function
#End Region

#Region "Misc"
        Private Function GetFields(ByVal Criteria As BLFieldMap.FieldMapCriteria) As List(Of BLFieldMap)

            Dim ToReturn As New List(Of BLFieldMap)
            For Each FieldMap As BLFieldMap In m_OrderedFields
                If Criteria.Match(FieldMap) Then
                    ToReturn.Add(FieldMap)
                End If
            Next

            Return ToReturn
        End Function

        Public ReadOnly Property Fields(ByVal FieldName As String) As BLFieldMap
            Get
                If ContainsKey(FieldName) Then
                    Return CType(Item(FieldName), BLFieldMap)
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Fields(ByVal FieldNames() As String) As List(Of BLFieldMap)
            Get
                Dim ToReturn As New List(Of BLFieldMap)
                For Each fieldName As String In FieldNames
                    Dim FoundField As BLFieldMap = Fields(fieldName)
                    If Not FoundField Is Nothing Then
                        ToReturn.Add(FoundField)
                    End If
                Next
                Return ToReturn
            End Get
        End Property

        Public ReadOnly Property OrderedFields() As ArrayList
            Get
                Return m_OrderedFields
            End Get
        End Property


        'Obtiene el primer mapeo autonumérico
        Public Function GetFirstAutoNumeric() As BLFieldMap
            If AutoNumericFields.Count > 0 Then
                Return CType(AutoNumericFields(0), BLFieldMap)
            Else
                Return Nothing
            End If
        End Function


#End Region

    End Class
End Namespace
