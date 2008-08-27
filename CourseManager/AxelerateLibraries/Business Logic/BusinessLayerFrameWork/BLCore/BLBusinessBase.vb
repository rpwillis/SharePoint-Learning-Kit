Imports System.Reflection
Imports Axelerate.BusinessLayerFrameWork.BLCore.Attributes
Imports Axelerate.BusinessLayerFrameWork.BLCore.Security
Imports Microsoft.Interop.Security.AzRoles
Imports System.Xml.Serialization
Namespace BLCore

    ''' <summary>
    ''' BusinessBase. Bussiness object able to fetch, update, insert and delete 
    ''' objects from any data store. Relational mapping is done through attribute oriented programming.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public MustInherit Class BLBusinessBase
        Implements ICloneable
        Implements IComparable


#Region "Constructors"
        ''' <summary>
        ''' Creates an instance of the object.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub New()
            Initialize()
        End Sub
#End Region

#Region " Initialize "

        ''' <summary>
        ''' Override this method to set up event handlers so user
        ''' code in a partial class can respond to events raised by generated code.
        ''' </summary>
        Protected Overridable Sub Initialize()
        End Sub

#End Region

#Region "DataBinding Helpers"
        'Returns a reference of himself
        Public Overridable ReadOnly Property THIS() As BLBusinessBase
            Get
                Return Me
            End Get
        End Property

        'Unique String is a property that will have all the business objects
        'that returns a unique string for identification
        Public Overridable ReadOnly Property UniqueString() As String
            Get
                Return ToString()
            End Get
        End Property

#End Region

#Region "Reflection Helpers"

        'Access the value of the property PropertyName by Reflection
        Default Public Property PropertyValue(ByVal PropertyName As String) As Object
            Get
                Try
                    Dim PropertyInfo As System.Reflection.PropertyInfo = _
                        Me.GetType.GetProperty(PropertyName, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)
                    If PropertyInfo Is Nothing Then
                        Dim FieldMap As BLFieldMap = CType(DataLayer.FieldMapList.Item(PropertyName), BLFieldMap)
                        Return FieldValue(FieldMap)
                    Else
                        Return PropertyInfo.GetValue(Me, Nothing)
                    End If
                Catch ex As Reflection.AmbiguousMatchException
                    Dim PropertyInfo As System.Reflection.PropertyInfo
                    PropertyInfo = AmbiguousPropertyValue(PropertyName)
                    Return PropertyInfo.GetValue(Me, Nothing)
                    'Return Me.GetType.InvokeMember(PropertyName, BindingFlags.Default Or BindingFlags.GetProperty, Nothing, Me, Nothing)
                End Try
            End Get

            Set(ByVal value As Object)
                Try
                    Dim PropertyInfo As System.Reflection.PropertyInfo = _
                                        Me.GetType.GetProperty(PropertyName, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)
                    If PropertyInfo Is Nothing Then
                        Dim FieldMap As BLFieldMap = CType(DataLayer.FieldMapList.Item(PropertyName), BLFieldMap)
                        FieldValue(FieldMap) = value
                    Else
                        PropertyInfo.SetValue(Me, value, Nothing)
                    End If
                Catch ex As Reflection.AmbiguousMatchException
                    Dim PropertyInfo As System.Reflection.PropertyInfo
                    PropertyInfo = AmbiguousPropertyValue(PropertyName)
                    PropertyInfo.SetValue(Me, value, Nothing)
                    'Dim Values(1) As Object
                    'Values(0) = value
                    'Me.GetType.InvokeMember(PropertyName, BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.SetProperty, Nothing, Me, Values)
                End Try

            End Set
        End Property
        'Obtains the most superficial property with the provided name
        Private ReadOnly Property AmbiguousPropertyValue(ByVal PropertyName As String) As PropertyInfo
            Get
                Dim ActualType As Type = Me.GetType
                Dim FoundProperty As Boolean = False
                Dim MyProperty As PropertyInfo = Nothing
                While FoundProperty = False
                    MyProperty = _
                    ActualType.GetProperty(PropertyName, BindingFlags.DeclaredOnly Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)
                    If (MyProperty Is Nothing) Then
                        ActualType = ActualType.BaseType
                    Else
                        FoundProperty = True
                    End If
                End While
                Return MyProperty

                'Return Me.GetType.GetField(Field.Name, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public).GetValue(Me)
            End Get
        End Property
        'Access the provided FieldInfo, that contains the key for a property with cache of an ForeignObject
        Friend Property CachedPropertyValue(ByVal Field As FieldInfo) As Object
            Get
                If CachedForeignObjectAttribute.isDefined(Field) Then
                    Return Me.GetType.GetField(Field.Name, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public).GetValue(Me)
                Else
                    Throw New Exception(BLResources.Exception_NotValidCachedProperty)
                End If
            End Get
            Set(ByVal value As Object)
                If CachedForeignObjectAttribute.isDefined(Field) Then
                    Me.GetType.GetField(Field.Name, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public).SetValue(Me, value)
                Else
                    Throw New Exception(BLResources.Exception_NotValidCachedProperty)
                End If
            End Set
        End Property

        'Represents a data field of the object indexed by a FieldInfo
        Public ReadOnly Property FieldValue(ByVal Field As FieldInfo) As Object
            Get
                Return Me.GetType.GetField(Field.Name, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public).GetValue(Me)
            End Get
        End Property

        'Represents a data field of the object indexed by a BLFieldMap
        Friend Property FieldValue(ByVal FieldMap As BLFieldMap) As Object
            Get
                If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Then
                    Return GetFieldInfoValue(FieldMap.Field)
                Else
                    Return DynamicField(FieldMap.NeutralFieldName)
                End If
            End Get

            Set(ByVal value As Object)
                If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Then
                    Dim CurrentType As Type = Me.GetType()
                    CurrentType.GetField(FieldMap.Field.Name, BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public).SetValue(Me, IIf(GetType(DBNull).IsInstanceOfType(value), Nothing, value))
                Else
                    DynamicField(FieldMap.NeutralFieldName) = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' Returns the specified value according to the custom attributes defined for the FielInfo
        ''' </summary>
        ''' <param name="field"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetFieldInfoValue(ByVal field As FieldInfo) As Object
            Dim CustomAttributes As Object() = field.GetCustomAttributes(GetType(NumericNullableAttribute), True)
            If CustomAttributes.Count <> 0 Then
                If CustomAttributes.GetValue(0).GetType().Equals(GetType(IntNullableAttribute)) Then
                    Dim intNullable As IntNullableAttribute = DirectCast(CustomAttributes.GetValue(0), IntNullableAttribute)
                    If intNullable.IsNull(field.GetValue(Me)) Then
                        Return Nothing
                    End If
                ElseIf CustomAttributes.GetValue(0).GetType().Equals(GetType(DoubleNullableAttribute)) Then
                    Dim doubleNullable As DoubleNullableAttribute = DirectCast(CustomAttributes.GetValue(0), DoubleNullableAttribute)
                    If doubleNullable.IsNull(field.GetValue(Me)) Then
                        Return Nothing
                    End If
                End If
            End If
            Return field.GetValue(Me)
        End Function
        ''' <summary>
        ''' Returns the associated FieldMap for a Property Name
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend ReadOnly Property PropertyFieldMap(ByVal PropertyName As String) As BLFieldMap
            Get
                For Each FieldMap As BLFieldMap In DataLayer.FieldMapList.OrderedFields
                    If FieldMap.PropertyName.ToLower = PropertyName Then
                        Return FieldMap
                    End If
                Next
                Return Nothing
            End Get
        End Property


#End Region

#Region "Formating Helpers"
        Private Function ReplaceTag(ByRef ToReplace As String, ByVal TagName As String, ByVal TagValue As String) As String
            Return ToReplace.Replace(TagName, TagValue)
        End Function

        Public Function ReplaceBusinessFieldTags(ByVal ToReplace As String) As String
            Dim ToReturn As String = ToReplace

            Dim Properties() As PropertyInfo = Me.GetType().GetProperties(BindingFlags.FlattenHierarchy Or BindingFlags.Public Or BindingFlags.Instance)
            For Each PropInfo As PropertyInfo In Properties
                Dim TagName As String = "[" + PropInfo.Name + "]"
                If (ToReturn.IndexOf(TagName) <> -1) Then
                    Dim Value As Object = PropertyValue(PropInfo.Name)
                    If (Not Value Is Nothing) Then
                        ToReturn = ReplaceTag(ToReturn, TagName, clsSecurityAssuranceHelper.HTMLEncode(Value.ToString()))
                    End If
                End If
            Next

            For Each FieldMap As BLFieldMap In DataLayer.FieldMapList.OrderedFields
                Dim TagName As String = "[" + FieldMap.NeutralFieldName + "]"
                If (ToReturn.IndexOf(TagName) <> -1) Then
                    Dim Value As Object = FieldValue(FieldMap.Field)
                    If (Not Value Is Nothing) Then
                        ToReturn = ReplaceTag(ToReturn, TagName, Value.ToString())
                    End If
                End If
            Next


            Return ToReturn

        End Function

#End Region

#Region "Data Layer Definition"
        ''' <summary>
        ''' Creation of the DataLayer
        ''' </summary>
        ''' <remarks></remarks>
        <NonSerialized()> _
        Private m_DataLayerContextInfo As DataLayerContextInfo

        ''' <summary>
        ''' Specific instance of the DataLayer to be used
        ''' </summary>
        ''' <remarks></remarks>
        <NonSerialized()> _
        Private m_DataLayer As DataLayerAbstraction = Nothing

        <XmlIgnore()> _
        Public Property DataLayerContextInfo() As DataLayerContextInfo
            Get
                If m_DataLayerContextInfo Is Nothing Then
                    m_DataLayerContextInfo = New DataLayerContextInfo(Me.GetType)
                End If
                Return m_DataLayerContextInfo
            End Get
            Set(ByVal value As DataLayerContextInfo)
                m_DataLayerContextInfo = value
            End Set
        End Property

        ''' <summary>
        ''' Returns or assigns the DataLayer associated to the object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlIgnore()> _
        Public Overridable Property DataLayer() As DLCore.DataLayerAbstraction
            Get
                If m_DataLayer Is Nothing Then
                    If DataLayerInfoAttribute.isDefined(Me.GetType) Then
                        Dim Attr As DataLayerInfoAttribute = DataLayerInfoAttribute.GetAttribute(Me.GetType)
                        Attr.RegisterDataLayerInfo()
                    End If
                    m_DataLayer = DataLayerCache.DataLayer(DataLayerContextInfo)
                End If
                Return m_DataLayer
            End Get
            Set(ByVal value As DLCore.DataLayerAbstraction)
                m_DataLayer = value
                DataLayerContextInfo.DataLayerType = value.GetType
            End Set
        End Property



#End Region

#Region "Must Overrides"


        'Must be overwritten for returning the type of the Criteria(inherits from BLCriteria)
        'defined by the child class.  This Criteria will will work as a unique identifier
        Public MustOverride ReadOnly Property KeyCriteria() As BLCriteria

        'Must be overwritten for returning the DataKey of the instance
        Public MustOverride ReadOnly Property DataKey() As BLDataKey
#End Region

#Region "DataPortal Overrides"
        'Uses the information of the DataLayer to read the object
        Protected Sub DataPortal_Fetch(ByVal Criteria As Object)
            Dim SecurityOperation As clsSecurityOperation = ReadSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            Dim NCriteria As BLCriteria = CType(Criteria, BLCriteria)
            CType(Criteria, BLCriteria).Update()
            BLFetch(NCriteria)

        End Sub

        'Uses the information of the DataLayer to delete the object
        Protected Sub DataPortal_Delete(ByVal Criteria As Object)
            Dim SecurityOperation As clsSecurityOperation = DeleteSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            Dim NCriteria As BLCriteria = CType(Criteria, BLCriteria)
            NCriteria.Update()
            BLFetch(NCriteria)

            BLDelete()
        End Sub

        'Uses the information of the DataLayer to update the object
        Protected Sub DataPortal_Update()
            Dim SecurityOperation As clsSecurityOperation = UpdateSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            If Not Me.IsDirty Then Exit Sub


            m_DataLayerContextInfo = PopSerializationContext()
            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                Try
                    BLUpdate()
                    scope.Complete()
                Catch e As System.Exception
                    Throw New System.Exception(BLResources.Exception_UpdateOperationFailed, e)
                End Try
            End Using
        End Sub

        'Uses the information of the DataLayer to insert the object
        Protected Sub DataPortal_Insert()
            Dim SecurityOperation As clsSecurityOperation = CreateSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If

            If Not Me.IsDirty Then Exit Sub


            m_DataLayerContextInfo = PopSerializationContext()
            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                'BeginEdit()
                Try
                    BLInsert()
                    scope.Complete()
                Catch e As System.Exception
                    'UndoChanges()
                    Throw New System.Exception(BLResources.Exception_InsertOperationFailed, e)
                End Try
            End Using
        End Sub

        'Uses the information of the DataLayer to delete the object
        Protected Sub DataPortal_DeleteSelf()
            Dim SecurityOperation As clsSecurityOperation = DeleteSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            If Not Me.IsDirty Then Exit Sub


            m_DataLayerContextInfo = PopSerializationContext()
            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                'BeginEdit()
                Try
                    BLDelete()
                    scope.Complete()
                Catch e As System.Exception
                    'UndoChanges()
                    Throw New System.Exception(BLResources.Exception_DeleteOperationFailed, e)
                End Try
            End Using
        End Sub


#End Region

#Region "Data Layer Access"
        'Uses the DataLayer to create the object with values
        Public Overridable Sub BLFetch(ByVal Criteria As BLCriteria)


            DataLayerContextInfo = Criteria.DataLayerContextInfo

            Dim CachedObjects As List(Of BLFieldMap) = DataLayer.FieldMapList.OnCreationCachedFields(Criteria)
            Dim ReadCommand As DLCore.DataLayerCommandBase = DataLayer.ReadCommand(Me, Criteria, CachedObjects)
            Dim Fetched As Boolean = False
            Try
                ReadCommand.Execute()

                Dim RecordExists As Boolean = ReadCommand.NextRecord()
                If Not RecordExists And Not Criteria.TryGet Then
                    Throw New Exception(BLResources.Exception_FetchOperationFailed)
                End If

                If RecordExists Then
                    BLFetchFromCommand(ReadCommand, Criteria)
                    'TODO: Aquí tiene que haber algun checkeo para que no se pueda hacer con stored procedures
                    BLFetchCachedObjectsFromCommand(ReadCommand, CachedObjects)
                    Fetched = True

                End If


            Catch ex As Exception

                Throw New Exception(BLResources.Exception_FetchOperationFailed, ex)
            Finally
                ReadCommand.Finish()
            End Try

            Try
                If Fetched Then
                    BLFetchClassFields(DataLayer.FieldMapList.ClassFetchFields)
                End If
            Catch ex As Exception

                Throw New Exception(BLResources.Exception_FetchOperationFailed, ex)
            End Try

            If Fetched Then
                MarkOld()
            End If
        End Sub

        'Reads the information of the mapeable fields from a DataReader
        'that has been posicioned over the information of the object
        Public Overridable Sub BLFetchFromCommand(ByVal Command As DLCore.DataLayerCommandBase, ByVal Criteria As BLCriteria)
            Dim FieldMap As BLFieldMap
            Dim EmptyObject As Boolean = True

            Try
                Dim ActualValueIndex As Integer = 0
                Dim FetchFields As List(Of BLFieldMap)
                If Not Criteria.DataLayerContextInfo.HasDynamicFields Then
                    FetchFields = DataLayer.FieldMapList.DataFetchFields
                Else
                    FetchFields = DataLayer.FieldMapList.Fields(Criteria.DataLayerContextInfo.DynamicFields)
                End If

                For Each FieldMap In FetchFields
                    Dim Value As Object
                    Value = Command.ReadData(ActualValueIndex)
                    If Not Value Is Nothing Then
                        EmptyObject = False
                        FieldValue(FieldMap) = Value
                    End If

                    ActualValueIndex = ActualValueIndex + 1
                Next
            Catch ex As Exception
                Throw New Exception(BLResources.Exception_DataStoreInvalidFieldValue, ex)
            End Try
            If Not EmptyObject Then
                MarkOld()
            End If
        End Sub

        'Reads the objects marked with the attribute CachedForeignObjectAttribute
        Friend Sub BLFetchCachedObjectsFromCommand(ByVal Command As DLCore.DataLayerCommandBase, ByVal CachedObjects As List(Of BLFieldMap))
            If CachedObjects.Count > 0 Then
                Dim TmpCriteria As New BLCriteria
                For Each FieldMap As BLFieldMap In CachedObjects
                    Dim Attr As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(FieldMap.Field)
                    Command.NextTable()
                    Command.NextRecord()

                    Dim BO As BLBusinessBase = CType(Attr.NewBusinessObjectInstance(DataLayerContextInfo), BLBusinessBase)
                    BO.DataLayerContextInfo.ForceLocal = DataLayerContextInfo.ForceLocal
                    BO.BLFetchFromCommand(Command, TmpCriteria)
                    Attr.SetPropertyValue(Me, BO)
                Next
            End If
            MarkOld()
        End Sub

        'Reads the objects marked with the attribute ClassFieldMapAttribute or ListFieldMapAttrbiute
        Friend Sub BLFetchClassFields(ByVal ClassFields As List(Of BLFieldMap))
            Try
                For Each FieldMap As BLFieldMap In ClassFields
                    Dim GetMethodName As String = "GetObject"
                    If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.BusinessCollectionField Then
                        GetMethodName = "GetCollection"
                    End If

                    Dim ClassType As Type
                    Dim Value As Object
                    Dim Args(1) As Object
                    Args(0) = Me
                    Args(1) = DataLayerContextInfo
                    ClassType = FieldMap.Field.FieldType
                    Value = ClassType.InvokeMember(GetMethodName, _
                        BindingFlags.DeclaredOnly Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or Reflection.BindingFlags.Default, _
                        Nothing, Nothing, Args)
                    FieldMap.Field.SetValue(Me, Value)
                Next
            Catch ex As Exception
                Throw New Exception(BLResources.Exception_ClassFieldFetchError, ex)
            End Try
            MarkOld()

        End Sub

        'Updates the information of the ocject and his childs towards the DataLayer
        Public Overridable Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing Then
                DataLayerContextInfo = ParentObject.DataLayerContextInfo
            End If

            Dim Message As String = ""
            Dim Validate As Boolean = ValidateUpdate(Message)

            If Validate Then

                Try
                    If Me.IsDirty Then
                        Dim UpdateCommand As DataLayerCommandBase = DataLayer.UpdateCommand(Me)
                        Try
                            UpdateCommand.Execute()
                        Catch ex As Exception
                            Throw ex
                        Finally
                            UpdateCommand.Finish()
                        End Try
                        MarkOld()
                    End If
                    BLSmartUpdateChilds()
                Catch ex As SqlException
                    Select Case ex.Number
                        Case 547
                            Throw New System.Exception(BLResources.Exception_RelatedObject_UpdateFailed, ex)
                        Case Else
                            Throw New System.Exception(BLResources.Exception_UpdateOperationFailed, ex)
                    End Select
                Catch ex As Exception
                    Throw New System.Exception(BLResources.Exception_UpdateOperationFailed, ex)
                End Try
            Else
                Throw New System.Exception(Message)
            End If

        End Sub


        'Inserts the information of the object and his childs towards the DataLayer
        Public Overridable Sub BLInsert(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing Then
                DataLayerContextInfo = ParentObject.DataLayerContextInfo
            End If

            Dim Message As String = ""
            Dim Validate As Boolean = ValidateInsert(Message)

            If Validate Then

                Try
                    If Me.IsDirty Then
                        Dim InsertCommand As DataLayerCommandBase = DataLayer.InsertCommand(Me)
                        Try
                            InsertCommand.Execute()
                            UpdateAutoNumeric(InsertCommand)
                        Catch ex As Exception
                            Throw ex
                        Finally
                            InsertCommand.Finish()
                        End Try

                    End If
                    BLSmartUpdateChilds()
                    MarkOld()
                Catch ex As SqlException
                    Select Case ex.Number
                        Case 547
                            Throw New System.Exception(BLResources.Exception_RelatedObject_InsertFailed, ex)
                        Case Else
                            Throw New System.Exception(BLResources.Exception_InsertOperationFailed, ex)
                    End Select
                Catch ex As Exception
                    Throw New System.Exception(BLResources.Exception_InsertOperationFailed, ex)
                End Try
            Else
                Throw New System.Exception(Message)
            End If

        End Sub

        'Deletes the information of the object and his childs towards the DataLayer
        Public Overridable Sub BLDelete(Optional ByVal ParentObject As BLBusinessBase = Nothing)
            If Not ParentObject Is Nothing Then
                DataLayerContextInfo = ParentObject.DataLayerContextInfo
            End If

            If Not Me.IsDirty Then Exit Sub

            Dim Message As String = ""
            Dim Validate As Boolean = ValidateDelete(Message)

            If Validate Then

                Try
                    BLDeleteChilds()
                    BLSmartUpdateChilds()
                    If Me.IsDirty Then
                        Dim DeleteCommand As DataLayerCommandBase = DataLayer.DeleteCommand(Me)
                        Try
                            DeleteCommand.Execute()
                        Finally
                            DeleteCommand.Finish()
                        End Try
                        MarkOld()
                    End If
                Catch ex As SqlException
                    Select Case ex.Number
                        Case 547
                            Throw New System.Exception(BLResources.Exception_RelatedObject_DeleteFailed, ex)
                        Case Else
                            Throw New System.Exception(BLResources.Exception_DeleteOperationFailed, ex)
                    End Select
                Catch ex As Exception
                    Throw New System.Exception(BLResources.Exception_DeleteOperationFailed, ex)
                End Try
            Else
                Throw New System.Exception(Message)
            End If

        End Sub

        'Se encarga de borrar de la base de datos los objetos hijos de este objeto
        'during an Update(). Could be overwritten by child classes but they must call MyBase.DeleteChilds()
        Protected Overridable Sub BLDeleteChilds()
            Dim ClassFields As New Collections.ArrayList
            Dim CurrentType As Type = Me.GetType()
            Dim FieldMap As BLFieldMap
            Dim ClassUpdateFields As List(Of BLFieldMap) = DataLayer.FieldMapList.ClassUpdateFields

            Try

                For Each FieldMap In ClassUpdateFields
                    Dim ClassType As Type
                    Dim Value As Object

                    ClassType = FieldMap.Field.FieldType
                    Value = FieldMap.Field.GetValue(Me)
                    If Not Value Is Nothing Then
                        If ClassFieldMapAttribute.isDefined(FieldMap.Field) Then
                            Dim BusinessObject As BLBusinessBase = CType(Value, BLBusinessBase)
                            BusinessObject.DataLayerContextInfo = DataLayerContextInfo
                            BusinessObject.Delete()
                        Else
                            Dim BusinessCollection As IBindingList = CType(Value, IBindingList)
                            Dim BusinessCollectionBase As IBLListBase = CType(Value, IBLListBase)
                            BusinessCollectionBase.DataLayerContextInfo = DataLayerContextInfo
                            BusinessCollection.Clear()
                        End If
                    End If

                Next
            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_ClassFieldDeleteError, ex)
            End Try
        End Sub




        'Se encarga de borrar de la base de datos los objetos hijos de este objeto
        'during an Update(). Could be overwritten by child classes but they must call MyBase.UpdateChilds()
        Friend Overridable Sub BLSmartUpdateChilds()
            Dim ClassFields As New Collections.ArrayList
            Dim CurrentType As Type = Me.GetType()
            Dim FieldMap As BLFieldMap
            Dim ClassUpdateFields As List(Of BLFieldMap) = DataLayer.FieldMapList.ClassUpdateFields
            Try

                For Each FieldMap In ClassUpdateFields
                    Dim ClassType As Type
                    Dim Value As Object

                    ClassType = FieldMap.Field.FieldType
                    Value = FieldMap.Field.GetValue(Me)
                    If Not Value Is Nothing Then
                        If ClassFieldMapAttribute.isDefined(FieldMap.Field) Then
                            Dim Attr As ClassFieldMapAttribute = ClassFieldMapAttribute.GetAttribute(FieldMap.Field)
                            Dim BusinessObject As BLBusinessBase = CType(Value, BLBusinessBase)
                            If Not Attr.UseParentSecurity Then
                                BusinessObject.SecurityCheck("ReadWrite")
                            End If
                            BusinessObject.BLSmartUpdate(Me)
                        Else
                            Dim BusinessCollection As IBLListBase = CType(Value, IBLListBase)
                            Dim Attr As ListFieldMapAttribute = ListFieldMapAttribute.GetAttribute(FieldMap.Field)
                            If Not Attr.UseParentSecurity Then
                                BusinessCollection.SecurityCheck("ReadWrite")
                            End If
                            BusinessCollection.BLUpdate(Me)
                        End If
                    End If
                Next
            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_ClassFieldUpdateError, ex)
            End Try
        End Sub

        'Examines the object and calls Update, Insert or Delete depending of the object
        Friend Overridable Sub BLSmartUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing)

            If Me.IsDeleted Then
                BLDelete(ParentObject)
            Else
                If Me.IsNew Then
                    BLInsert(ParentObject)
                Else
                    BLUpdate(ParentObject)
                End If
                BLSmartUpdateChilds()
            End If
        End Sub

        Protected Sub UpdateAutoNumeric(ByVal InsertCommand As DataLayerCommandBase)
            Dim Value As Object
            Dim FieldMap As BLFieldMap = DataLayer.FieldMapList.GetFirstAutoNumeric
            If Not FieldMap Is Nothing Then
                InsertCommand.NextRecord()
                Value = InsertCommand.ReadData(0)
                If FieldMap.Field.FieldType Is GetType(Integer) Then
                    FieldMap.Field.SetValue(Me, CInt(Value))
                ElseIf FieldMap.Field.FieldType Is GetType(String) Then
                    FieldMap.Field.SetValue(Me, CStr(Value))
                End If
            End If
        End Sub

        ''' <summary>
        ''' Saves the object to the database.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Calling this method starts the save operation, causing the object
        ''' to be inserted, updated or deleted within the database based on the
        ''' object's current state.
        ''' </para><para>
        ''' If <see cref="Core.BusinessBase.IsDeleted" /> is <see langword="true"/>
        ''' the object will be deleted. Otherwise, if <see cref="Core.BusinessBase.IsNew" /> 
        ''' is <see langword="true"/> the object will be inserted. 
        ''' Otherwise the object's data will be updated in the database.
        ''' </para><para>
        ''' All this is contingent on <see cref="Core.BusinessBase.IsDirty" />. If
        ''' this value is <see langword="false"/>, no data operation occurs. 
        ''' It is also contingent on <see cref="Core.BusinessBase.IsValid" />. 
        ''' If this value is <see langword="false"/> an
        ''' exception will be thrown to indicate that the UI attempted to save an
        ''' invalid object.
        ''' </para><para>
        ''' It is important to note that this method returns a new version of the
        ''' business object that contains any data updated during the save operation.
        ''' You MUST update all object references to use this new version of the
        ''' business object in order to have access to the correct object data.
        ''' </para><para>
        ''' You can override this method to add your own custom behaviors to the save
        ''' operation. For instance, you may add some security checks to make sure
        ''' the user can save the object. If all security checks pass, you would then
        ''' invoke the base Save method via <c>MyBase.Save()</c>.
        ''' </para>
        ''' </remarks>
        ''' <returns>A new object containing the saved values.</returns>
        Public Overridable Function Save() As BLBusinessBase
            If Not IsValid Then
                Throw New Exception(BLResources.Exception_InvalidStateSave)
                'Throw New ValidationException(BLResources.Exception_InvalidStateSave)
            End If

            If IsDirty Then
                If Not DataLayerContextInfo.IsDefaultDataLayer Then
                    PushSerializationContext(DataLayerContextInfo)
                End If

                Dim TmpObject As BLBusinessBase = CType(GetClone(), BLBusinessBase)
                If Me.IsDeleted Then
                    TmpObject.DataPortal_DeleteSelf()
                Else
                    If Me.IsNew Then
                        TmpObject.DataPortal_Insert()
                    Else
                        TmpObject.DataPortal_Update()
                    End If
                End If
                PopSerializationContext()
                Return TmpObject
            Else
                Return DirectCast(Me, BLBusinessBase)
            End If

        End Function

        ''' <summary>
        ''' Saves the object to the database, forcing
        ''' IsNew to <see langword="false"/> and IsDirty to True.
        ''' </summary>
        ''' <param name="forceUpdate">
        ''' If <see langword="true"/>, triggers overriding IsNew and IsDirty. 
        ''' If <see langword="false"/> then it is the same as calling Save().
        ''' </param>
        ''' <returns>A new object containing the saved values.</returns>
        ''' <remarks>
        ''' This overload is designed for use in web applications
        ''' when implementing the Update method in your 
        ''' data wrapper object.
        ''' </remarks>
        Public Overridable Function Save(ByVal forceUpdate As Boolean) As BLBusinessBase

            If forceUpdate AndAlso IsNew Then
                MarkDirtyRecursive()
                ' mark the object as old - which makes it
                ' not dirty
                MarkOld()
                ' now mark the object as dirty so it can save
                MarkDirty()
            End If
            Return Me.Save()

        End Function

        Public Sub Delete()
            MarkDeleted()
        End Sub

#End Region

#Region "Validation"
        Public Overridable Function ValidateUpdate(ByRef Message As String) As Boolean
            Message = ""
            Return True
        End Function

        Public Overridable Function ValidateInsert(ByRef Message As String) As Boolean
            Message = ""
            Return True
        End Function

        Public Overridable Function ValidateDelete(ByRef Message As String) As Boolean
            Message = ""
            Return True
        End Function
#End Region

#Region "System.Object Overrides"

        'Redefines the method ToString in rder to return the unique identifier(Criteria) of the object
        Public Overrides Function ToString() As String
            Return KeyCriteria.ToString
        End Function

        'Redefines the method Equals in a way that they are consideed equal when
        'their unique identifiers(Criteria) are the same
        Public Overloads Function Equals(ByVal obj As BLBusinessBase) As Boolean
            Return DataKey.Equals(obj.DataKey)
        End Function

        'Redefines the hash code for using the unique identifier(Criteria).
        Public Overloads Function GetHashCode() As Integer
            Return KeyCriteria.ToString.GetHashCode()
        End Function

#End Region

#Region "Security Properties and Methods"

        ''' <summary>
        ''' Gets the Security Operation to read the selected property of this Business Object
        ''' </summary>
        ''' <param name="PropertyName">The name of the property</param>
        ''' <value></value>
        ''' <returns>The read property security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property ReadPropertySecurityOperation(ByVal PropertyName As String) As clsSecurityOperation
            Get
                Dim CurrentProperty As PropertyInfo = DataLayer.GetProperty(PropertyName)
                If SecurablePropertyAttribute.isDefined(CurrentProperty) Then
                    Dim Attrib As SecurablePropertyAttribute = SecurablePropertyAttribute.GetAttribute(CurrentProperty)
                    If ((Attrib.SecurableType = SecurablePropertyAttribute.SecurableTypes.Read) Or _
                        (Attrib.SecurableType = SecurablePropertyAttribute.SecurableTypes.Update)) Then
                        'Dim Operation As New clsReadBOPropertyOperation(Me, CurrentProperty.Name)
                        'Return Operation
                        Return DataLayer.GetPropertySecurityOperation("READPROPERY", Me, PropertyName)
                    Else
                        Return New clsDeniedSecurityOperation()
                    End If
                Else
                    Return New clsGrantedSecurityOperation()
                End If

            End Get
        End Property

        ''' <summary>
        ''' Gets the Security Operation to update the selected property of this Business Object
        ''' </summary>
        ''' <param name="PropertyName">The name of the property</param>
        ''' <value></value>
        ''' <returns>The update property security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property UpdatePropertySecurityOperation(ByVal PropertyName As String) As clsSecurityOperation
            Get
                Dim CurrentProperty As PropertyInfo = DataLayer.GetProperty(PropertyName)
                If SecurablePropertyAttribute.isDefined(CurrentProperty) Then
                    Dim Attrib As SecurablePropertyAttribute = SecurablePropertyAttribute.GetAttribute(CurrentProperty)
                    If (Attrib.SecurableType = SecurablePropertyAttribute.SecurableTypes.Update) Then
                        'Dim Operation As New clsUpdateBOPropertyOperation(Me, CurrentProperty.Name)
                        'Return Operation
                        Return DataLayer.GetPropertySecurityOperation("UPDATEPROPERY", Me, PropertyName)
                    Else
                        Return New clsDeniedSecurityOperation()
                    End If
                Else
                    Return New clsGrantedSecurityOperation()
                End If

            End Get
        End Property

        ''' <summary>
        ''' Gets the Security Operation to create instances of this type
        ''' </summary>
        ''' <value></value>
        ''' <returns>The create security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property CreateSecurityOperation() As clsSecurityOperation
            Get
                If DataLayer.SecurityAttribute Is Nothing Then
                    Return New clsGrantedSecurityOperation()
                End If

                If (DataLayer.SecurityAttribute.MustCheckCreateAccess()) Then
                    'Return New clsCreateBOOperation(Me)
                    Return DataLayer.GetCRUDSecurityOperation("CREATE", Me)
                Else
                    Return New clsDeniedSecurityOperation()
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets the Security Operation to read instances of this type
        ''' </summary>
        ''' <value></value>
        ''' <returns>The read security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property ReadSecurityOperation() As clsSecurityOperation
            Get
                If DataLayer.SecurityAttribute Is Nothing Then
                    Return New clsGrantedSecurityOperation()
                End If
                If (DataLayer.SecurityAttribute.MustCheckReadAccess()) Then
                    'Return New clsReadBOOperation(Me)
                    Return DataLayer.GetCRUDSecurityOperation("READ", Me)
                Else
                    Return New clsDeniedSecurityOperation()
                End If

            End Get
        End Property
        ''' <summary>
        ''' Gets the Security Operation to update instances of this type
        ''' </summary>
        ''' <value></value>
        ''' <returns>The update security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property UpdateSecurityOperation() As clsSecurityOperation
            Get
                If DataLayer.SecurityAttribute Is Nothing Then
                    Return New clsGrantedSecurityOperation()
                End If
                If (DataLayer.SecurityAttribute.MustCheckUpdateAccess()) Then
                    'Return New clsUpdateBOOperation(Me)
                    Return DataLayer.GetCRUDSecurityOperation("UPDATE", Me)
                Else
                    Return New clsDeniedSecurityOperation()
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets the Security Operation to delete instances of this type
        ''' </summary>
        ''' <value></value>
        ''' <returns>The delete security operation</returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property DeleteSecurityOperation() As clsSecurityOperation
            Get
                If DataLayer.SecurityAttribute Is Nothing Then
                    Return New clsGrantedSecurityOperation()
                End If
                If (DataLayer.SecurityAttribute.MustCheckDeleteAccess()) Then
                    'Return New clsDeleteBOOperation(Me)
                    Return DataLayer.GetCRUDSecurityOperation("DELETE", Me)
                Else
                    Return New clsDeniedSecurityOperation()
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets the security scope of this object
        ''' </summary>
        ''' <returns>The security scope</returns>
        ''' <remarks></remarks>
        Public Overridable Function GetSecurityScope() As Security.SecurityScope
            Return New Security.SecurityScope("")
        End Function

        ''' <summary>
        ''' Gets a list of all the security operations of this Business Object
        ''' </summary>
        ''' <returns>A list with the security operations</returns>
        ''' <remarks></remarks>
        Public Overridable Function GetSecurityOperations() As List(Of clsSecurityOperation)
            Dim OperationList As List(Of clsSecurityOperation) = New List(Of clsSecurityOperation)

            GetClassSecurityOperations(OperationList)
            Dim CurrentType As Type = Me.GetType()

            For Each CurrentProperty As PropertyInfo In DataLayer.PropertyHashTable.Values
                GetPropertySecurityOperations(CurrentProperty, OperationList)
            Next

            For Each CurrentMethod As MethodInfo In DataLayer.MethodHashTable.Values
                GetMethodSecurityOperations(CurrentMethod, OperationList)
            Next
            Return OperationList
        End Function
        ''' <summary>
        ''' Gets a list of all the security operations that evaluate the current property of this class
        ''' </summary>
        ''' <param name="CurrentProperty">The current property info</param>
        ''' <param name="OperationList">The operation list</param>
        ''' <remarks></remarks>
        Private Sub GetPropertySecurityOperations(ByVal CurrentProperty As PropertyInfo, ByVal OperationList As List(Of clsSecurityOperation))
            'Property check
            Dim UpdateOperation As clsSecurityOperation = UpdatePropertySecurityOperation(CurrentProperty.Name)
            If UpdateOperation IsNot Nothing Then
                OperationList.Add(UpdateOperation)
            End If

            Dim ReadOperation As clsSecurityOperation = ReadPropertySecurityOperation(CurrentProperty.Name)
            If ReadOperation IsNot Nothing Then
                OperationList.Add(ReadOperation)
            End If

        End Sub

        ''' <summary>
        ''' Gets a list of all the security operations that evaluate the type
        ''' </summary>
        ''' <param name="OperationList">The list of operations</param>
        ''' <remarks></remarks>
        Private Sub GetClassSecurityOperations(ByVal OperationList As List(Of clsSecurityOperation))
            'Class check 
            Dim CreateOperation As clsSecurityOperation = CreateSecurityOperation()
            Dim ReadOperation As clsSecurityOperation = ReadSecurityOperation()
            Dim UpdateOperation As clsSecurityOperation = UpdateSecurityOperation()
            Dim DeleteOperation As clsSecurityOperation = DeleteSecurityOperation()
            If CreateOperation IsNot Nothing Then
                OperationList.Add(CreateOperation)
            End If
            If ReadOperation IsNot Nothing Then
                OperationList.Add(ReadOperation)
            End If
            If UpdateOperation IsNot Nothing Then
                OperationList.Add(UpdateOperation)
            End If
            If DeleteOperation IsNot Nothing Then
                OperationList.Add(DeleteOperation)
            End If
        End Sub
        ''' <summary>
        ''' Gets a list of all the security operations that evaluate the current method of this class
        ''' </summary>
        ''' <param name="CurrentMethod">The current method info</param>
        ''' <param name="OperationList">The list of operations</param>
        ''' <remarks></remarks>
        Private Sub GetMethodSecurityOperations(ByVal CurrentMethod As MethodInfo, ByVal OperationList As List(Of clsSecurityOperation))
            'Method check

            If SecurableMethodAttribute.isDefined(CurrentMethod) Then
                Dim Attrib As SecurableMethodAttribute = SecurableMethodAttribute.GetAttribute(CurrentMethod)
                'Dim Operation As New clsExecuteBOMethodOperation(Me, CurrentMethod.Name)
                Dim Operation As clsBLSecurityOperation = DataLayer.GetExecuteBOMethodSecurityOperation("EXECUTE", Me, CurrentMethod.Name)
                OperationList.Add(Operation)
            End If
        End Sub
        ''' <summary>
        ''' Obsolete
        ''' </summary>
        ''' <param name="NivelAcceso"></param>
        ''' <remarks></remarks>
        Friend Overridable Sub SecurityCheck(ByVal NivelAcceso As String)
            'Dim MyPrincipal As MyGenericPrincipal = CType(System.Threading.Thread.CurrentPrincipal, MyGenericPrincipal)
            'Dim BusinessObjectName As String = Me.GetType.Name
            'Try

            '    Dim DemandString As String = BusinessObjectName + "/" + _
            '        GetSecurityContext().IDSede + "/" + NivelAcceso
            '    Dim MyPermission As Permissions.PrincipalPermission
            '    MyPermission = New Permissions.PrincipalPermission(Nothing, DemandString)
            '    MyPermission.Demand()
            'Catch e As System.Exception
            '    Throw New System.Exception("El usuario no tiene el nivel de acceso adecuado (" + NivelAcceso + ") para realizar la operación sobre " + BusinessObjectName)
            'Finally
            'End Try
        End Sub

#End Region

#Region "Uncategorized"

        Friend Function MarkDirtyRecursive() As Boolean
            Dim NeedsMarkingAsDirty As Boolean = False
            If DataLayer.FieldMapList.ClassUpdateFields.Count > 0 Then
                For Each FieldMap As BLFieldMap In DataLayer.FieldMapList.ClassUpdateFields
                    Dim ClassType As Type
                    Dim Value As Object

                    ClassType = FieldMap.Field.FieldType
                    Value = FieldMap.Field.GetValue(Me)
                    If Not Value Is Nothing Then
                        If ClassFieldMapAttribute.isDefined(FieldMap.Field) Then
                            Dim BusinessObject As BLBusinessBase = CType(Value, BLBusinessBase)
                            If BusinessObject.MarkDirtyRecursive Then
                                NeedsMarkingAsDirty = True
                            End If
                        Else
                            Dim BusinessCollection As IBindingList = CType(Value, IBindingList)
                            Dim BusinessCollectionBase As IBLListBase = CType(Value, IBLListBase)
                            If BusinessCollectionBase.MarkDirtyRecursive Then
                                NeedsMarkingAsDirty = True
                            End If
                        End If
                    End If
                Next
            End If

            If (NeedsMarkingAsDirty) And Not IsDirty Then
                MarkDirty()
            End If

            Return IsDirty


        End Function




#End Region

#Region "Factory Functions"

        Friend Sub Local_GetObject(ByVal Criteria As BLCriteria)
            If Me.DataLayerContextInfo.ForceLocal Then
                Throw New System.Exception(BLResources.Exception_InvalidLocalGet)
            End If
            DataPortal_Fetch(Criteria)
        End Sub
#End Region

#Region "DataLayer Serialization Context"
        Private m_DataLayerSerializationContext As DLCore.DataLayerContextInfo = Nothing

        Public Sub PushSerializationContext(ByVal SerializationContext As DLCore.DataLayerContextInfo)
            m_DataLayerSerializationContext = SerializationContext
        End Sub

        Public Function PopSerializationContext() As DLCore.DataLayerContextInfo
            Dim ToReturn As DLCore.DataLayerContextInfo = m_DataLayerSerializationContext
            m_DataLayerSerializationContext = Nothing

            'Ensures that the DataLayer is properly loaded
            Dim DataLayerInstance As DataLayerAbstraction = DataLayer

            Return ToReturn
        End Function

#End Region

#Region "Dynamic Data"
        'Values of the dynamic properties of the object
        Private m_DynamicValues As New Hashtable

        Public Property DynamicField(ByVal FieldName As String) As Object
            Get
                Return m_DynamicValues(FieldName)
            End Get
            Set(ByVal value As Object)
                m_DynamicValues(FieldName) = value
            End Set
        End Property


#End Region

#Region " ICloneable "

        Private Function Clone() As Object Implements ICloneable.Clone

            Return GetClone()

        End Function

        ''' <summary>
        ''' Creates a clone of the object.
        ''' </summary>
        ''' <returns>
        ''' A new object containing the exact data of the original object.
        ''' </returns>
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Function GetClone() As Object

            Return ObjectCloner.Clone(Me)

        End Function

#End Region

#Region "IComparable"

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
            If obj.GetType().Name <> Me.GetType().Name Then
                Return -1
            End If

            Dim OtherObject As BLBusinessBase = DirectCast(obj, BLBusinessBase)
            Dim AllField As ArrayList = DataLayer.FieldMapList.OrderedFields

            For Each FieldMap As BLFieldMap In AllField
                Try
                    If Not FieldMap.Field.GetType.GetInterface("IList") Is Nothing Then

                        Dim HashOriginalState As Hashtable = New Hashtable
                        Dim HashActualState As Hashtable = New Hashtable
                        Dim OriginalValues As IList = DirectCast(Me.FieldValue(FieldMap), IList)
                        Dim ActualValues As IList = DirectCast(OtherObject.FieldValue(FieldMap), IList)

                        ' Fills the lookup hash with the elements in the original object state
                        For Each ValueElement As Object In OriginalValues
                            HashOriginalState.Add(ValueElement.ToString, ValueElement)
                        Next

                        ' Fills the lookup hash with the elements in the actual object state 
                        ' Looks for new elements
                        For Each ValueElement As Object In ActualValues
                            HashActualState.Add(ValueElement.ToString(), ValueElement)
                            If Not HashOriginalState.ContainsKey(ValueElement.ToString) Then
                                Return -1
                            End If
                        Next

                        ' Looks for deleted elements
                        For Each ValueElement As Object In OriginalValues
                            If Not HashActualState.ContainsKey(ValueElement.ToString) Then
                                Return -1
                            End If
                        Next
                    Else
                        Dim ThisFieldValue As String = FieldValue(FieldMap).ToString
                        Dim OtherFieldValue As String = (DirectCast(obj, BLBusinessBase)).FieldValue(FieldMap).ToString

                        If ThisFieldValue <> OtherFieldValue Then
                            Return -1
                        End If
                    End If
                Catch ex As Exception
                End Try
            Next
            Return 0
        End Function

#End Region

#Region " ValidationRules, IsValid "

        Private mValidationErrors As List(Of ValidationError)

        ''' <summary>
        ''' Provides access to the validation rules collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable ReadOnly Property ValidationRules() As ValidationRules
            Get
                Return Nothing
            End Get
        End Property
        ''' <summary>
        ''' Provides access to the broken rules functionality.
        ''' </summary>
        ''' <remarks>
        ''' This property is used within your business logic so you can
        ''' easily call the AddRule() method to associate validation
        ''' rules with your object's properties.
        ''' </remarks>
        Public ReadOnly Property ValidationErrors() _
          As List(Of ValidationError)
            Get
                If mValidationErrors Is Nothing Then
                    mValidationErrors = New List(Of ValidationError)()
                End If
                Return mValidationErrors
            End Get
        End Property


        ''' <summary>
        ''' Returns <see langword="true" /> if the object is currently valid, <see langword="false" /> if the
        ''' object has broken rules or is otherwise invalid.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' By default this property relies on the underling ValidationRules
        ''' object to track whether any business rules are currently broken for this object.
        ''' </para><para>
        ''' You can override this property to provide more sophisticated
        ''' implementations of the behavior. For instance, you should always override
        ''' this method if your object has child objects, since the validity of this object
        ''' is affected by the validity of all child objects.
        ''' </para>
        ''' </remarks>
        ''' <returns>A value indicating if the object is currently valid.</returns>
        <Browsable(False)> _
        Public Overridable ReadOnly Property IsValid() As Boolean
            Get
                Return ValidationRules.IsValid(Me, Me.IsDirty())
            End Get
        End Property

        ''' <summary>
        ''' Provides access to the readonly collection of broken business rules
        ''' for this object.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Public Overridable ReadOnly Property BrokenRulesCollection() _
          As List(Of ValidationError)
            Get
                Return mValidationErrors
            End Get
        End Property

#End Region

#Region " IsNew, IsDeleted, IsDirty "

        Private mIsNew As Boolean = True
        Private mIsDeleted As Boolean
        Private mIsDirty As Boolean = True

        ''' <summary>
        ''' Returns <see langword="true" /> if this is a new object, 
        ''' <see langword="false" /> if it is a pre-existing object.
        ''' </summary>
        <Browsable(False)> _
        Public ReadOnly Property IsNew() As Boolean
            Get
                Return mIsNew
            End Get
        End Property

        ''' <summary>
        ''' Returns <see langword="true" /> if this object is marked for deletion.
        ''' </summary>
        <Browsable(False)> _
        Public ReadOnly Property IsDeleted() As Boolean
            Get
                Return mIsDeleted
            End Get
        End Property

        ''' <summary>
        ''' Returns <see langword="true" /> if this object's data has been changed.
        ''' </summary>
        <Browsable(False)> _
        Public Overridable ReadOnly Property IsDirty() As Boolean
            Get
                Return mIsDirty
            End Get
        End Property

        ''' <summary>
        ''' Marks the object as being a new object. This also marks the object
        ''' as being dirty and ensures that it is not marked for deletion.
        ''' </summary>
        Protected Overridable Sub MarkNew()
            mIsNew = True
            mIsDeleted = False
            MarkDirty()
        End Sub

        ''' <summary>
        ''' Marks the object as being an old (not new) object. This also
        ''' marks the object as being unchanged (not dirty).
        ''' </summary>
        Protected Overridable Sub MarkOld()
            mIsNew = False
            mIsDirty = False

        End Sub

        ''' <summary>
        ''' Marks an object for deletion. This also marks the object
        ''' as being dirty.
        ''' </summary>
        Protected Sub MarkDeleted()
            mIsDeleted = True
            MarkDirty()
        End Sub

        ''' <summary>
        ''' Marks an object as being dirty, or changed.
        ''' </summary>
        Protected Sub MarkDirty()
            mIsDirty = True
        End Sub

        ''' <summary>
        ''' Performs processing required when the current
        ''' property has changed.
        ''' </summary>
        '''<System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)> _
        Protected Sub PropertyHasChanged()
            Dim propertyName As String = New System.Diagnostics.StackTrace().GetFrame(1).GetMethod.Name.Substring(4)
            PropertyHasChanged(propertyName)
        End Sub

        ''' <summary>
        ''' Performs processing required when a property
        ''' has changed.
        ''' </summary>
        Protected Overridable Sub PropertyHasChanged(ByVal propertyName As String)

            ValidationRules.IsValid(Me, True)
            MarkDirty()
            'OnPropertyChanged(propertyName)

        End Sub

        ''' <summary>
        ''' Returns <see langword="true" /> if this object is both dirty and valid.
        ''' </summary>
        Public Overridable ReadOnly Property IsSavable() As Boolean
            Get
                Return IsDirty AndAlso IsValid
            End Get
        End Property

#End Region

#Region "Reflection Properties"

        ''' <summary>
        ''' Gets the underlying propertyInfo for a property with property name.  Not implemented at this level.
        ''' </summary>
        ''' <param name="PropertyName">Name of the property used the extract the propertyinfo</param>
        ''' <returns>propertyInfo for a property with property name</returns>
        Public Overridable Function GetProperty(ByVal PropertyName As String) As PropertyInfo
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the underlying MethodInfo for a method  with method name.  Not implemented at this level.
        ''' </summary>
        ''' <param name="PropertyName">Name of the method used the extract the MethodInfo</param>
        ''' <returns>MethodInfo for a method with Method Name</returns>
        Public Overridable Function GetMethod(ByVal MethodName As String) As MethodInfo
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the value of a property of the provisioning object (using reflection).
        ''' </summary>
        ''' <param name="PropertyName">Name of the property that is used to retrieve the value</param>
        ''' <returns>Value of the property</returns>
        Public Function GetPropertyValue(ByVal PropertyName As String) As Object
            Try
                Dim PropertyInfo As System.Reflection.PropertyInfo = Me.[GetType]().GetProperty(PropertyName, BindingFlags.FlattenHierarchy Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.[Public])

                If PropertyInfo Is Nothing Then
                    Dim Mapping As BLFieldMap = DirectCast(DataLayer.FieldMapList(PropertyName), BLFieldMap)
                    Return GetFieldValue(Mapping)
                Else
                    Return PropertyInfo.GetValue(Me, Nothing)
                End If
            Catch ex As AmbiguousMatchException
                Dim PropertyInfo As PropertyInfo
                PropertyInfo = GetAmbiguousPropertyValue(PropertyName)
                Return PropertyInfo.GetValue(Me, Nothing)
            End Try
        End Function

        ''' <summary>
        ''' Sets the value of a property of the provisioning object (using reflection).
        ''' </summary>
        ''' <param name="PropertyName">Name of the property that is used to set the value</param>
        ''' <param name="Value">Value to set to the property</param>
        Public Sub SetPropertyValue(ByVal PropertyName As String, ByVal Value As Object)
            Try
                Dim PropertyInfo As PropertyInfo = Me.[GetType]().GetProperty(PropertyName, (BindingFlags.FlattenHierarchy Or (BindingFlags.NonPublic Or (BindingFlags.Instance Or BindingFlags.[Public]))))
                If PropertyInfo Is Nothing Then
                    Dim Mapping As BLFieldMap = DirectCast((DataLayer.FieldMapList(PropertyName)), BLFieldMap)
                    SetFieldValue(Mapping, Value)
                    PropertyHasChanged()
                Else
                    PropertyInfo.SetValue(Me, Value, Nothing)
                End If
            Catch ex As AmbiguousMatchException
                Dim PropertyInfo As System.Reflection.PropertyInfo
                PropertyInfo = GetAmbiguousPropertyValue(PropertyName)
                PropertyInfo.SetValue(Me, Value, Nothing)
            End Try
        End Sub

        ''' <summary>
        ''' Gets the value of a property of the provisioning object (using reflection).  This is used when
        ''' the property name cannot be directly resolved because there are various versions of the property.
        ''' </summary>
        ''' <param name="PropertyName">Name of the property that is used to retrieve the value</param>
        ''' <returns>Value of the property</returns>
        Public Function GetAmbiguousPropertyValue(ByVal PropertyName As String) As PropertyInfo
            Dim ActualType As Type = Me.[GetType]()
            Dim FoundProperty As Boolean = False
            Dim MyProperty As PropertyInfo = Nothing
            While (FoundProperty = False)
                MyProperty = ActualType.GetProperty(PropertyName, BindingFlags.DeclaredOnly Or (BindingFlags.NonPublic Or (BindingFlags.Instance Or BindingFlags.[Public])))
                If (MyProperty Is Nothing) Then
                    ActualType = ActualType.BaseType
                Else
                    FoundProperty = True
                End If
            End While
            Return MyProperty
        End Function

        ''' <summary>
        ''' Gets the value of a field of the provisioning object (using reflection).
        ''' </summary>
        ''' <param name="Mapping">Mapping for the object field</param>
        ''' <returns>Value of the field</returns>
        Public Function GetFieldValue(ByVal Mapping As BLFieldMap) As Object
            If (Mapping.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField) Then
                Dim CurrentType As Type = Me.[GetType]()
                Return Mapping.Field.GetValue(Me)
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Sets the value of a field of the provisioning object (using reflection).
        ''' </summary>
        ''' <param name="Mapping">Mapping for the object field</param>
        ''' <param name="Value">Value for the object</param>

        Public Sub SetFieldValue(ByVal Mapping As BLFieldMap, ByVal Value As Object)
            If (Mapping.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField) Then
                Dim CurrentType As Type = Me.[GetType]()
                CurrentType.GetField(Mapping.Field.Name, BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.[Public]).SetValue(Me, Value)
            End If
        End Sub

#End Region

    End Class
End Namespace
