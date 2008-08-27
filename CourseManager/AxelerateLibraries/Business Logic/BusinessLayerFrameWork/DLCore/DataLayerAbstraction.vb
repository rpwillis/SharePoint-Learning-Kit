Imports System.Reflection
Imports System.Configuration.ConfigurationManager
Imports System.Configuration
Imports Microsoft.Win32

Namespace DLCore


    ''' <summary>
    ''' The DataLayerAbstraction class  provides the business logic with an abstract interface to access commands 
    ''' that will allow them to perform operations on the data store.
    ''' This class abstracts the concept of a data store (usually a data table).  The inherited classes must implement the the common insertion, 
    ''' deletion, udapting and retrieval functions for this table.  Inherited classes can also implemnt the Execution Command
    ''' which is not table dependant.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class DataLayerAbstraction
#Region "Automatic Test"
        Public TestExceptionList As List(Of TestException) = Nothing

#End Region
#Region "Security"
        Private m_SecurityAttribute As SecurableClassAttribute = Nothing
        Private m_PropertyHashTable As Hashtable = Nothing
        Private m_MethodHashTable As Hashtable = Nothing
        Private m_SecurityOperations As Hashtable = Nothing

        ''' <summary>
        ''' A Hash Table with the properties of the business logic type.
        ''' If the type has properties with the same name, the table only contains the last declared property
        ''' </summary>
        ''' <value></value>
        ''' <returns>A hashtable with the properties</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PropertyHashTable() As Hashtable
            Get
                Return m_PropertyHashTable
            End Get
        End Property
        ''' <summary>
        ''' A Hash Table with the methods of the business logic type.
        ''' If the type has methods with the same name, the table contains the last declared method
        ''' </summary>
        ''' <value></value>
        ''' <returns>A hashtable with the methods</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MethodHashTable() As Hashtable
            Get
                Return m_MethodHashTable
            End Get
        End Property
        ''' <summary>
        ''' The Security Attribute of the business logic type
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SecurityAttribute() As SecurableClassAttribute
            Get
                Return m_SecurityAttribute
            End Get
        End Property
        ''' <summary>
        ''' Gets a property of the business logic type.
        ''' If the type has properties with the same name, returns the last declared property.
        ''' </summary>
        ''' <param name="PropertyName">The name of the property</param>
        ''' <returns>The selected property info</returns>
        ''' <remarks></remarks>
        Public Function GetProperty(ByVal PropertyName As String) As PropertyInfo
            Return CType(PropertyHashTable(PropertyName), PropertyInfo)
        End Function
        ''' <summary>
        ''' Gets a method of the business logic type.
        ''' If the type has methods with the same name, returns the last declared method.
        ''' </summary>
        ''' <param name="MethodName">The name of the method</param>
        ''' <returns>The selected method info</returns>
        ''' <remarks></remarks>
        Public Function GetMethod(ByVal MethodName As String) As MethodInfo
            Return CType(MethodHashTable(MethodName), MethodInfo)
        End Function
        ''' <summary>
        ''' Creates the hashtable with the properties of the business logic type.
        ''' If the type has properties with the same name, the hashtable contains the last declared.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub CreatePropertyHashTable()
            m_PropertyHashTable = New Hashtable
            For Each NewProperty As PropertyInfo In m_BusinessLogicType.GetProperties()
                If PropertyHashTable.Contains(NewProperty.Name) = False Then
                    PropertyHashTable.Add(NewProperty.Name, NewProperty)
                Else
                    Dim PropInfo As PropertyInfo = CType(PropertyHashTable(NewProperty.Name), PropertyInfo)
                    Dim ActualType As Type = PropInfo.DeclaringType
                    Dim NewType As Type = NewProperty.DeclaringType

                    While ((ActualType IsNot Nothing) AndAlso (ActualType.Name <> NewType.Name))
                        ActualType = ActualType.BaseType
                    End While
                    If (ActualType Is Nothing) Then
                        PropertyHashTable(NewProperty.Name) = NewProperty
                    End If
                End If
            Next
        End Sub
        ''' <summary>
        ''' Creates the hashtable with the methods of the business logic type.
        ''' If the type has methods with the same name, the hashtable contains the last declared.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub CreateMethodHashTable()
            m_MethodHashTable = New Hashtable
            For Each NewMethod As MethodInfo In m_BusinessLogicType.GetMethods()
                If MethodHashTable.Contains(NewMethod.Name) = False Then
                    MethodHashTable.Add(NewMethod.Name, NewMethod)
                Else
                    Dim MethInfo As MethodInfo = CType(MethodHashTable(NewMethod.Name), MethodInfo)
                    Dim ActualType As Type = MethInfo.DeclaringType
                    Dim NewType As Type = NewMethod.DeclaringType

                    While ((ActualType IsNot Nothing) AndAlso (ActualType.Name <> NewType.Name))
                        ActualType = ActualType.BaseType
                    End While
                    If (ActualType Is Nothing) Then
                        MethodHashTable(NewMethod.Name) = NewMethod
                    End If
                End If
            Next
        End Sub
        Private Sub CreateSecurityAttribute()
            If SecurableClassAttribute.isDefined(m_BusinessLogicType) Then
                m_SecurityAttribute = SecurableClassAttribute.GetAttribute(m_BusinessLogicType)
            End If

        End Sub

        Private Sub CreateSecurityOperationsHashTable()
            m_SecurityOperations = New Hashtable()
            If (m_SecurityAttribute IsNot Nothing) Then
                Dim BLSecurityConfigFile As String = ConfigurationManager.AppSettings("BLSecurityConfigFile")
                If BLSecurityConfigFile IsNot Nothing And BLSecurityConfigFile <> "" Then
                    Dim doc As Xml.XmlDocument
                    doc = New Xml.XmlDocument()
                    doc.Load(BLSecurityConfigFile)
                    Dim FoundNodes As System.Xml.XmlNodeList = doc.GetElementsByTagName("DEFAULTSECURITYOPERATIONS")
                    If FoundNodes.Count > 0 Then
                        For Each dfNode As Xml.XmlNode In FoundNodes.Item(0).ChildNodes
                            If dfNode.Name.ToUpper() = "OPERATION" Then
                                m_SecurityOperations(dfNode.Attributes("Name").Value.ToUpper()) = Type.GetType(dfNode.Attributes("Type").Value.ToUpper(), True, True)
                            End If
                        Next
                    End If
                    Dim ClassSecurity As Xml.XmlElement = doc.GetElementById(m_BusinessLogicType.FullName)
                    If ClassSecurity IsNot Nothing Then
                        For Each dfNode As Xml.XmlNode In ClassSecurity.ChildNodes
                            If dfNode.Name.ToUpper() = "OPERATION" Then
                                m_SecurityOperations(dfNode.Attributes("Name").Value.ToUpper()) = Type.GetType(dfNode.Attributes("Type").Value.ToUpper(), True, True)
                            End If
                        Next
                    End If
                End If
            End If
        End Sub

        Public Function GetCRUDSecurityOperation(ByVal OperationName As String, ByVal BO As BLBusinessBase) As clsBLSecurityOperation
            If Not BO.GetType().Equals(m_BusinessLogicType) Then
                Return Nothing
            End If
            Dim Operation As clsBLSecurityOperation = Nothing
            If m_SecurityOperations.ContainsKey(OperationName.ToUpper()) Then
                Operation = DirectCast(System.Activator.CreateInstance(DirectCast(m_SecurityOperations(OperationName.ToUpper()), Type), BO), clsBLSecurityOperation)
            Else
                Select Case OperationName.ToUpper()
                    Case "CREATE"
                        Return New clsCreateBOOperation(BO)
                    Case "READ"
                        Return New clsReadBOOperation(BO)
                    Case "UPDATE"
                        Return New clsUpdateBOOperation(BO)
                    Case "DELETE"
                        Return New clsDeleteBOOperation(BO)
                    Case "METHOD"

                End Select
            End If
            Return Operation
        End Function

        Public Function GetPropertySecurityOperation(ByVal OperationName As String, ByVal BO As BLBusinessBase, ByVal PropertyName As String) As clsBLSecurityOperation
            If Not BO.GetType().Equals(m_BusinessLogicType) Then
                Return Nothing
            End If
            Dim Operation As clsBLSecurityOperation = Nothing
            If m_SecurityOperations.ContainsKey(OperationName.ToUpper()) Then
                Operation = DirectCast(System.Activator.CreateInstance(DirectCast(m_SecurityOperations(OperationName.ToUpper()), Type), BO, PropertyName), clsBLSecurityOperation)
            Else
                Select Case OperationName.ToUpper()
                    Case "READPROPERY"
                        Return New clsReadBOPropertyOperation(BO, PropertyName)
                    Case "UPDATEPROPERY"
                        Return New clsUpdateBOPropertyOperation(BO, PropertyName)

                End Select
            End If
            Return Operation
        End Function

        Public Function GetExecuteBOMethodSecurityOperation(ByVal OperationName As String, ByVal BO As BLBusinessBase, ByVal MethodName As String) As clsBLSecurityOperation
            If Not BO.GetType().Equals(m_BusinessLogicType) Then
                Return Nothing
            End If
            Dim Operation As clsBLSecurityOperation = Nothing
            If m_SecurityOperations.ContainsKey(OperationName.ToUpper()) Then
                Operation = DirectCast(System.Activator.CreateInstance(DirectCast(m_SecurityOperations(OperationName.ToUpper()), Type), BO, MethodName), clsBLSecurityOperation)
            Else
                Select Case OperationName.ToUpper()
                    Case "METHOD"
                        Return New clsExecuteBOMethodOperation(BO, MethodName)


                End Select
            End If
            Return Operation
        End Function

#End Region
#Region "DataLayer Construction"
        Private m_DataDefined As Boolean = False

        ''' <summary>
        ''' Creates a Data Layer that is not associated to any particular table in the data store or any particular 
        ''' object in the business logic.  This kind of DataLayer will only support Execution Commands.
        ''' </summary>
        ''' <param name="DefaultDataSourceName">Identitifcation of the data store</param>
        ''' <remarks></remarks>
        Protected Sub New(ByVal DefaultDataSourceName As String)
            If DefaultDataSourceName = "" Then
                m_DefaultDataSourceName = ConnectionStrings(0).Name
            Else
                m_DefaultDataSourceName = DefaultDataSourceName
            End If
            m_TableName = ""
            m_DataLayerFieldSuffix = ""
            m_DataDefined = False
            m_BusinessLogicType = Nothing
        End Sub

        ''' <summary>
        ''' Creates a Data Layer associated to a specific table in a Data Store and to a specific business object.
        ''' </summary>
        ''' <param name="NTableName">Name for the table in the data store</param>
        ''' <param name="NDataLayerFieldSuffix">Suffix that is appended to the name of the fields in the data store</param>
        ''' <param name="BusinessObjectType">Type of the logical business object that this data layer will relate to</param>
        ''' <param name="DefaultDataSourceName">Identitifcation of the data store</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal NTableName As String, ByVal NDataLayerFieldSuffix As String, _
            ByVal BusinessObjectType As System.Type, ByVal DefaultDataSourceName As String)
            m_TableName = NTableName
            m_DataLayerFieldSuffix = NDataLayerFieldSuffix
            m_DataDefined = False
            If DefaultDataSourceName = "" Then
                m_DefaultDataSourceName = ConnectionStrings(0).Name
            Else
                m_DefaultDataSourceName = DefaultDataSourceName
            End If
            m_BusinessLogicType = BusinessObjectType


            If DataLayerInfoAttribute.isDefined(BusinessObjectType) Then
                Dim DataLayerInfoAttrib As DataLayerInfoAttribute = DataLayerInfoAttribute.GetAttribute(m_BusinessLogicType)
                UseDynamicFields = DataLayerInfoAttrib.HasDynamicFields
            End If
        End Sub

        Protected Sub New(ByVal ParameterList() As DataTypes.Pair(Of String, String))
            For Each Pair As DataTypes.Pair(Of String, String) In ParameterList
                Select Case Pair.First
                    Case "TableName"
                        m_TableName = Pair.Second
                    Case "DataLayerFieldSuffix"
                        m_DataLayerFieldSuffix = Pair.Second
                    Case "DataSourceName"
                        m_DefaultDataSourceName = Pair.Second
                    Case "ClassName"
                        m_BusinessLogicType = ReflectionHelper.CreateBusinessType(Pair.Second)
                End Select

            Next

            If DataLayerInfoAttribute.isDefined(m_BusinessLogicType) Then
                Dim DataLayerInfoAttrib As DataLayerInfoAttribute = DataLayerInfoAttribute.GetAttribute(m_BusinessLogicType)
                UseDynamicFields = DataLayerInfoAttrib.HasDynamicFields
            End If

        End Sub



        Protected Sub BeginDataDefinition()
            m_DataDefined = False
        End Sub

        Protected Sub EndDataDefinition(ByVal CheckRepository As Boolean)
            Try
                m_DataDefined = True
                If CheckRepository Then
                    DataDefinition_VerifyExistence(True)
                End If
                If UseDynamicFields Then
                    AddDynamicFieldsFromRepository()
                    FieldMapList.ClearCaches()
                End If
                CreatePropertyHashTable()
                CreateMethodHashTable()
                CreateSecurityAttribute()
                CreateSecurityOperationsHashTable()
            Catch ex As System.Exception
                Trace.WriteLine(My.Resources.BLResources.errDataTableDefinition + TableName + ": " + ex.ToString)
            End Try
        End Sub

        Public Shared Function ParseInitializationString(ByVal InitializationString As String) As DataTypes.Pair(Of String, String)()
            Dim Separators() As String = {";", "="}
            Dim Tokens() As String = InitializationString.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
            Dim ActualPair As Integer = 0
            Dim ParseError As Boolean = False
            If Tokens.Length Mod 2 = 0 Then
                Dim PairListLenght As Integer = CInt(Tokens.Length / 2)
                Dim PairList(PairListLenght - 1) As DataTypes.Pair(Of String, String)
                Dim i As Integer = 0

                While i < Tokens.Length And Not ParseError
                    Try
                        Dim Pair As New DataTypes.Pair(Of String, String)(Tokens(i), Tokens(i + 1))
                        PairList(ActualPair) = Pair
                        ActualPair = ActualPair + 1
                        i = i + 2
                    Catch ex As System.Exception
                        ParseError = True
                    End Try
                End While
                If Not ParseError Then
                    Return PairList
                Else
                    Return Nothing
                End If
            End If
            Return Nothing

        End Function


#End Region

#Region "Factory Methods"
        ''' <summary>
        ''' Creates a new DataLayer of the DataLayerType type.  For this factory to work properly, inherited class must
        ''' support construction with only one parameter, the DataSource name.  
        ''' The new DataLayer that this factory returns is not bound to any specific table or business object.
        ''' </summary>
        ''' <typeparam name="DataLayerType">Type of the DataLayer to be created</typeparam>
        ''' <param name="DataSourceName">Identification of the data store</param>
        ''' <returns>A new DataLayer of the desired type</returns>
        ''' <remarks></remarks>
        Public Shared Function NewDataLayer(Of DataLayerType As DataLayerAbstraction)(ByVal DataSourceName As String) As DataLayerAbstraction
            Dim Args(0) As Object
            Args(0) = DataSourceName
            Return CType(System.Activator.CreateInstance(GetType(DataLayerType), Args), DataLayerAbstraction)
        End Function
#End Region


#Region "Table Mapping"

        'Preffix of the internal field names
        Protected m_ObjectFieldPrefix As String = "m_"

        'Suffix of the data store fields
        Protected m_DataLayerFieldSuffix As String = ""

        ''' <summary>
        ''' Lists the fields of this abstract object capable of extracting of a data store
        ''' </summary>
        ''' <remarks></remarks>
        Protected m_FieldMapList As New BLFieldMapList

        'Name of the table where it is automap. In case of the stored procedures
        'the base of the four stored procedures and a user they are call
        'SEL_TABLENAME, DEL_TABLENAME, INS_TABLENAME, UPD_TABLENAME y SEL_TABLENAME_ALL
        Protected m_TableName As String = ""

        'BLBusinessBase type
        Protected Shared BusinessBaseType As Type = GetType(BLBusinessBase)

        ''' <summary>
        ''' Uses dynamics fields during the Fetch and Update operations
        ''' </summary>
        ''' <remarks></remarks>
        Protected m_UseDynamicFields As Boolean = False

        ''' <summary>
        ''' Name of the default database of this DataLayer 
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DefaultDataSourceName As String = ""

        ''' <summary>
        ''' Business object associated to the DataLayer
        ''' </summary>
        ''' <remarks></remarks>
        Private m_BusinessLogicType As Type = Nothing

        ''' <summary>
        ''' Uses dynamic fields during Fetch and Update operations
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UseDynamicFields() As Boolean
            Get
                Return m_UseDynamicFields
            End Get
            Set(ByVal value As Boolean)
                m_UseDynamicFields = value
            End Set
        End Property

        Public ReadOnly Property BusinessLogicType() As Type
            Get
                Return m_BusinessLogicType
            End Get
        End Property

        Public ReadOnly Property DefaultDataSourceName() As String
            Get
                Return m_DefaultDataSourceName
            End Get
        End Property

        Public ReadOnly Property DefaultConnectionString() As String
            Get
                Dim ConnectionString As String = ""
                Dim InstanceIdentifier As String = "[DataSourceInstance]"
                If DefaultDataSourceName.StartsWith(InstanceIdentifier) Then
                    ConnectionString = DefaultDataSourceName.Substring(InstanceIdentifier.Length)
                Else
                    If DefaultDataSourceName <> "" Then
                        Dim ConnectionStringSetting As ConnectionStringSettings
                        ConnectionStringSetting = ConnectionStrings(DefaultDataSourceName)
                        If ConnectionStringSetting Is Nothing Then
                            Return DataLayerAbstraction.GetConnectionString(DefaultDataSourceName).ConnectionString
                        End If

                        ConnectionString = ConnectionStrings(DefaultDataSourceName).ConnectionString
                    Else
                        ConnectionString = ConnectionStrings(0).ConnectionString
                    End If

                End If
                Return ConnectionString

            End Get
        End Property


        Public Property FieldMapList() As BLFieldMapList
            Get
                Return m_FieldMapList
            End Get
            Set(ByVal Value As BLFieldMapList)
                m_FieldMapList = Value
            End Set
        End Property

        Public ReadOnly Property DataLayerFieldSuffix() As String
            Get
                Return m_DataLayerFieldSuffix
            End Get
        End Property

        Public ReadOnly Property ObjectFieldPrefix() As String
            Get
                Return m_ObjectFieldPrefix
            End Get
        End Property

        'Generates the fields over the  FieldMap list (FieldList), for the BusinessType type.
        'Assumes that the preffix of the automappable fields are ObjectFieldPrefix and
        'data store fields mantain the same name (without the preffix) plus the suffix (DBFieldSuffix)
        Protected Sub AddFieldsFromType(ByVal BusinessType As Type)

            Dim Fields() As FieldInfo
            Dim Field As FieldInfo
            Dim CurrentType As Type = BusinessType


            Fields = BusinessType.GetFields(BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)

            Dim Order As Integer = 0
            Do
                For Each Field In Fields
                    If Field.DeclaringType Is CurrentType Then
                        If FieldMapAttribute.isDefined(Field) Then
                            Dim FieldMapAttr As FieldMapAttribute = FieldMapAttribute.GetAttribute(Field)
                            Dim DLFieldName As String
                            If (Mid(Field.Name, 1, ObjectFieldPrefix.Length) <> ObjectFieldPrefix) Then
                                Throw New Exception(BLResources.Exception_DataLayer_CommandExecutionNotSupported)
                            Else
                                DLFieldName = Mid(Field.Name, ObjectFieldPrefix.Length + 1) + m_DataLayerFieldSuffix

                                Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.DataField, Field, FieldMapAttr.IsKey, _
                                    FieldMapAttr.IsFetchField, FieldMapAttr.IsUpdateField, _
                                    Order, FieldMapAttr.AutoNumericType, FieldMapAttr.PropertyName, FieldMapAttr.IsAutoNumericRelevant)
                                FieldMapList.Add(FieldMap)
                                Order = Order + 1
                            End If
                        End If

                        If ClassFieldMapAttribute.isDefined(Field) Then
                            Dim ClassFieldMapAttr As ClassFieldMapAttribute = ClassFieldMapAttribute.GetAttribute(Field)
                            Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.BusinessClassField, Field, False, _
                                ClassFieldMapAttr.isFetchField, ClassFieldMapAttr.isUpdateField, Order, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, _
                                "", False)
                            FieldMapList.Add(FieldMap)
                            Order = Order + 1
                        End If

                        If ListFieldMapAttribute.isDefined(Field) Then
                            Dim ListFieldMapAttr As ListFieldMapAttribute = ListFieldMapAttribute.GetAttribute(Field)
                            Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.BusinessCollectionField, Field, False, _
                                ListFieldMapAttr.isFetchField, ListFieldMapAttr.isUpdateField, Order, BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, _
                                "", False)
                            FieldMapList.Add(FieldMap)
                            Order = Order + 1
                        End If

                        If CachedForeignObjectAttribute.isDefined(Field) Then
                            Dim CachedForeignObjectAtt As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(Field)
                            Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.CachedObjectField, Field, False, _
                                CachedForeignObjectAtt.LoadType = CachedForeignObjectAttribute.CachedObjectLoadType.OnCreation, False, Order, _
                                 BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, "", False)

                            FieldMapList.Add(FieldMap)
                            Order = Order + 1
                        End If

                    End If
                Next
                CurrentType = CurrentType.BaseType
            Loop Until CurrentType Is BusinessBaseType
        End Sub

        Private Sub AddDynamicFieldsFromRepository()
            'Generates the list of dynamic fields
            Dim RepositoryFields As List(Of BLFieldMap) = DataDefinition_Fields.DynamicDataFields
            For Each RepositoryField As BLFieldMap In RepositoryFields
                Dim DynamicField As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.DynamicDataField, _
                        RepositoryField.NeutralFieldName, RepositoryField.isKey, RepositoryField.isFetchField, RepositoryField.isUpdateField, FieldMapList.Count, RepositoryField.AutoNumericType, RepositoryField.PropertyName, False)
                FieldMapList.Add(DynamicField)
            Next
        End Sub


        Public ReadOnly Property TableName() As String
            Get
                Return m_TableName
            End Get
        End Property

#End Region
#Region "ConnectionStrings"
        ''' <summary>
        ''' Returns the connection string defined for the defined Business object instance
        ''' </summary>
        ''' <param name="pName">Name defined for the connection site</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Shared Function GetConnectionString(ByVal pName As String) As ConnectionStringSettings
            Dim masterKey As RegistryKey = Registry.LocalMachine.OpenSubKey(BLResources.Connection_RegistrySettingsLocation)
            Dim connectionStringValue As String
            Try
                connectionStringValue = masterKey.GetValue(pName + ConfigurationManager.AppSettings.Get("WebSiteGUID")).ToString()
            Catch ex As Exception
                Throw New Exception(BLResources.Connection_ExceptionValueNotFound)
            End Try
            Return New ConnectionStringSettings(pName, connectionStringValue, BLResources.Connection_DataProvider)
        End Function
        ''' <summary>
        ''' Set a new value in the local machine registry for connecting to the database
        ''' </summary>
        ''' <param name="pName">Name defined for the connection site</param>
        ''' <param name="pConnectionString"></param>
        ''' <param name="pProviderName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Shared Function SetConnectionString(ByVal pName As String, ByVal pConnectionString As String, ByVal pProviderName As String) As ConnectionStringSettings
            Dim masterKey As RegistryKey = Registry.LocalMachine.CreateSubKey(BLResources.Connection_RegistrySettingsLocation)
            If masterKey Is Nothing Then
                Throw New Exception(BLResources.Connection_ExceptionSubKeyCreation)
            Else
                Try
                    masterKey.SetValue(pName, pConnectionString)
                Catch ex As Exception
                    Throw New Exception(ex.Message)
                Finally
                    masterKey.Close()
                End Try
            End If
            Return New ConnectionStringSettings(pName, pConnectionString, BLResources.Connection_DataProvider)
        End Function
#End Region
#Region "MustOverride"

        'Returns a insert command for the data store
        Public MustOverride Function InsertCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase

        'Returns a update command for the data store
        Public MustOverride Function UpdateCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase

        'Returns a delete command for the data store
        Public MustOverride Function DeleteCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase

        'Returns a read comamnd for the data store based on a Criteria object.
        'If CachedObjects is Nothing, simply creates this collection inside
        'the method. Caller is the object asking for the command
        Public MustOverride Function ReadCommand(ByVal Caller As Object, ByVal Criteria As BLCriteria, _
            Optional ByVal CachedObjects As List(Of BLFieldMap) = Nothing) As DataLayerCommandBase

        ''' <summary>
        ''' Returns a command that will execute a custom action in the data store
        ''' </summary>
        ''' <param name="Caller"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function ExecutionCommand(ByVal Caller As IBLCommandBase) As DataLayerCommandBase
            Throw New System.Exception(BLResources.Exception_DataLayer_CommandExecutionNotSupported)
        End Function

        ''' <summary>
        ''' Returns a command that will select all the disctinc values for the specified Fields
        ''' </summary>
        ''' <param name="Caller"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function ValueListCommand(ByVal Caller As IBLListBase, ByVal FieldNames() As String, ByVal Criteria As BLCriteria) As DataLayerCommandBase
            Throw New System.Exception(BLResources.Exception_DataLayer_CommandExecutionNotSupported)
        End Function


#End Region

#Region "Data Definition Methods and Properties"
        ''' <summary>
        ''' Creates the table in the data store
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub DataDefinition_Create()
            Dim TestException As TableCreationTestException = New TableCreationTestException()
            TestException.TableName = TableName
            If (TestExceptionList Is Nothing) Then
                TestExceptionList = New List(Of TestException)
            End If
            TestExceptionList.Add(TestException)
            Throw New Exception(BLResources.Exception_DataLayer_TableCreationNotSupported)

        End Sub

        ''' <summary>
        ''' Table exists in the data store
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function DataDefinition_Exists() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Destroys the table in the data store
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub DataDefinition_Drop()
            Throw New Exception(BLResources.Exception_DataLayer_TableDroppingNotSupported)
        End Sub

        Public Overridable Sub DataDefinition_Field_Create(ByVal FieldList As BLFieldMapList)
            Dim TestException As FieldCreationTestException = New FieldCreationTestException()
            TestException.FieldList = FieldList
            If (TestExceptionList Is Nothing) Then
                TestExceptionList = New List(Of TestException)
            End If
            TestExceptionList.Add(TestException)
            Throw New Exception(BLResources.Exception_DataLayer_FieldCreationNotSupported)
        End Sub

        Public Overridable Function DataDefinition_Field_Exisits(ByVal Field As BLFieldMap) As Boolean
            Return True

        End Function


        Public Overridable Sub DataDefinition_Field_Drop(ByVal FieldList As BLFieldMapList)
            Throw New Exception(BLResources.Exception_DataLayer_FieldDroppingNotSupported)

        End Sub

        Public Overridable ReadOnly Property DataDefinition_Fields() As BLFieldMapList
            Get
                Return New BLFieldMapList

            End Get
        End Property


        ''' <summary>
        ''' Verifies if the table and each field exist in the data store, if not creates the table
        ''' </summary>
        ''' <param name="ForceStructureUpdate"></param>
        ''' <remarks></remarks>
        Private Sub DataDefinition_VerifyExistence(ByVal ForceStructureUpdate As Boolean)

            'If table doesn't exists, creates it
            If Not DataDefinition_Exists() Then
                DataDefinition_Create()
            Else
                Try
                    'Si exists verifies each field are equal
                    Dim RepositoryFields As BLFieldMapList = DataDefinition_Fields
                    Dim FieldsToCreate As New BLFieldMapList
                    Dim FieldsToDrop As New BLFieldMapList
                    For Each Field As BLFieldMap In FieldMapList.DataFetchFields
                        Dim RepositoryField As BLFieldMap = CType(RepositoryFields.Fields(Field.NeutralFieldName), BLFieldMap)
                        If RepositoryField Is Nothing Then
                            FieldsToCreate.Add(Field)
                        Else
                            If Not RepositoryField.Equals(Field) Then
                                FieldsToDrop.Add(RepositoryField)
                                FieldsToCreate.Add(Field)
                            End If
                        End If

                    Next
                    If ForceStructureUpdate Then
                        'If FieldsToDrop.Count > 0 Then
                        'DataDefinition_Field_Drop(FieldsToDrop)
                        'End If
                        If FieldsToCreate.Count > 0 Then
                            DataDefinition_Field_Create(FieldsToCreate)
                        End If
                    Else
                        Throw New System.Exception(BLResources.Exception_DataLayer_UnrequestedSchemaGeneration)
                    End If

                Catch e As System.Exception
                    Throw New System.Exception(BLResources.Exception_DataLayer_SchemaGenerationFailed, e)
                End Try
            End If

        End Sub





#End Region


    End Class

End Namespace

