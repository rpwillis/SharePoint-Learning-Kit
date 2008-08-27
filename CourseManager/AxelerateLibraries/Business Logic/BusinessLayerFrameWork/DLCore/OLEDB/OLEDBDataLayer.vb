Imports System.Reflection
Imports Data_Access_Application_Block
Imports System.Configuration.ConfigurationManager
Imports System.Data.OleDb

Namespace DLCore

    'Class that all the business object musy inherit and contains the information
    'of the metadata generated from the atrributes of the business class
    Public Class OLEDBDataLayer
        Inherits DataLayerAbstraction

#Region "SQL Data"
        Public Enum SelectSQLPartType
            SelectClausePart = 0
            FetchFieldsPart = 1
            FromClausePart = 2
            StoreProcedurePart = 3
        End Enum

        'Variable that stores the SQL instruction or Stored Procedure of the selection

        ''' <summary>
        ''' Stores the different parts of the SQL de selection
        ''' </summary>
        ''' <remarks></remarks>
        Private m_SelectSQL() As String = {"SELECT ", "", "", ""}


        'Stores the SQL instruccion or Stored Procedure of the delete
        Private m_DeleteSQL As String = ""

        'Stores the SQL instruccion or Stored Procedure of the insert
        Private m_InsertSQL As String = ""

        'Stores the SQL instruccion or Stored Procedure of the update
        Private m_UpdateSQL As String = ""

        'General purpose Cache that provides a standard way for storing multiple SQL's 
        Private m_SQLCache As Hashtable = New Hashtable


        'Gets or Sets m_SelectSQL
        Public Property SelectSQL(ByVal SQLPart As SelectSQLPartType) As String
            Get
                Return m_SelectSQL(SQLPart)
            End Get
            Set(ByVal Value As String)
                m_SelectSQL(SQLPart) = Value
            End Set
        End Property

        'Gets or Sets m_SelectSQL
        Public ReadOnly Property AssembledSelectSQL(ByVal Criteria As BLCriteria) As String
            Get
                Dim CommandText As String = SelectSQL(SelectSQLPartType.SelectClausePart)
                If Criteria.Distinct Then
                    CommandText = CommandText + "DISTINCT "
                End If

                If Criteria.DataLayerContextInfo.HasDynamicFields Then
                    For Each FieldName As String In Criteria.DataLayerContextInfo.DynamicFields
                        CommandText = CommandText + FieldName + ", "
                    Next
                    CommandText = Mid(CommandText, 1, CommandText.Length - 2)
                Else
                    CommandText = CommandText + SelectSQL(SelectSQLPartType.FetchFieldsPart)
                End If

                CommandText = CommandText + " " + SelectSQL(SelectSQLPartType.FromClausePart)

                Return CommandText
            End Get
        End Property


        'Gets or Sets m_UpdateSQL
        Public Property UpdateSQL() As String
            Get
                Return m_UpdateSQL
            End Get
            Set(ByVal Value As String)
                m_UpdateSQL = Value
            End Set
        End Property

        'Gets or Sets m_InsertSQL
        Public Property InsertSQL() As String
            Get
                Return m_InsertSQL
            End Get
            Set(ByVal Value As String)
                m_InsertSQL = Value
            End Set
        End Property

        'Gets or Sets m_Delete
        Public Property DeleteSQL() As String
            Get
                Return m_DeleteSQL
            End Get
            Set(ByVal Value As String)
                m_DeleteSQL = Value
            End Set
        End Property

        'Returns the function that selects all 
        Public ReadOnly Property SelectAllFunction() As String
            Get
                Return "SEL_" + m_TableName.ToUpper + "_ALL()"
            End Get
        End Property

        'Gets through the list of object fields and returns in Parameters
        'an array of SqlParameter that colud be used to complete operations of insert or update
        Protected Sub GetUpdateParameters(ByVal BusinessObject As BLBusinessBase, ByRef Parameters() As OleDbParameter)
            Dim Offset As Integer
            Dim ActualUpdateParameter As Integer = 0
            Dim NumInitialParemeters As Integer
            If Not Parameters Is Nothing Then
                Offset = Parameters.Length
                NumInitialParemeters = Offset
                ReDim Preserve Parameters(FieldMapList.Count + NumInitialParemeters - 1)
            Else
                Offset = 0
                NumInitialParemeters = 0
                ReDim Parameters(FieldMapList.Count - 1)
            End If
            Dim Field As BLFieldMap
            Dim UpdateFieldMapList As List(Of BLFieldMap) = FieldMapList.DataUpdateFields
            For Each Field In UpdateFieldMapList
                Dim Value As Object
                Dim Param As OleDbParameter
                Value = BusinessObject.FieldValue(Field)
                Param = New OleDbParameter("@" + Field.DLFieldName, Value)

                Parameters(Offset + ActualUpdateParameter) = Param
                ActualUpdateParameter = ActualUpdateParameter + 1
            Next

            ReDim Preserve Parameters(ActualUpdateParameter + NumInitialParemeters - 1)
        End Sub

        Public Property SQLCache(ByVal SQLName As String) As String
            Get
                If m_SQLCache.ContainsKey(SQLName) Then
                    Return CType(SQLCache(SQLName), String)
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If m_SQLCache.ContainsKey(SQLName) Then
                    m_SQLCache.Remove(SQLName)
                End If
                m_SQLCache.Add(SQLName, value)
            End Set
        End Property

        'Generates the four instructions XXXSQL, using the name of the table and the list
        'of fields of the object
        Public Sub GenerateDynamicSQL()

            Dim FieldMap As BLFieldMap

            Dim FetchFieldNames As String = ""
            Dim InsertFieldNames As String = ""
            Dim UpdateFieldParameters As String = ""
            Dim InsertFieldParameters As String = ""

            For Each FieldMap In FieldMapList.OrderedFields
                If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField Or FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DynamicDataField Then
                    If FieldMap.isFetchField Then
                        FetchFieldNames = FetchFieldNames + " " + FieldMap.DLFieldName + ", "
                    End If

                    If FieldMap.isUpdateField And FieldMap.AutoNumericType = BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric Then
                        InsertFieldNames = InsertFieldNames + " " + FieldMap.DLFieldName + ", "
                        InsertFieldParameters = InsertFieldParameters + " @" + FieldMap.DLFieldName + ", "
                        UpdateFieldParameters = UpdateFieldParameters + FieldMap.DLFieldName + " = " + " @" + FieldMap.DLFieldName + ", "
                    End If
                End If
            Next

            Dim AutoNumericField As BLFieldMap = FieldMapList.GetFirstAutoNumeric()
            Dim GeneratedColumnSPName As String = ""
            Dim AutoNumericType As BLFieldMap.AutoNumericTypeEnum = BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric
            Dim AutoNumericDBFieldType As String = ""
            Dim AutoNumericReferenceField As String = ""
            If Not AutoNumericField Is Nothing Then
                Throw New Exception(BLResources.Exception_OLEDB_AutoNumericNotSupported)
            End If

            If FetchFieldNames.Length > 0 Then
                FetchFieldNames = Mid(FetchFieldNames, 1, FetchFieldNames.Length - 2)
            End If

            If InsertFieldNames.Length > 0 Then
                InsertFieldNames = Mid(InsertFieldNames, 1, InsertFieldNames.Length - 2)
                UpdateFieldParameters = Mid(UpdateFieldParameters, 1, UpdateFieldParameters.Length - 2)
                InsertFieldParameters = Mid(InsertFieldParameters, 1, InsertFieldParameters.Length - 2)
            End If

            SelectSQL(SelectSQLPartType.FetchFieldsPart) = FetchFieldNames
            SelectSQL(SelectSQLPartType.FromClausePart) = " FROM " + TableName

            DeleteSQL = "Delete FROM " + TableName

            UpdateSQL = "UPDATE " + TableName + " SET " + UpdateFieldParameters

            InsertSQL = ""
            If AutoNumericType = BLFieldMap.AutoNumericTypeEnum.GeneratedColumn Then
                Throw New Exception(BLResources.Exception_OLEDB_AutoNumericNotSupported)
            End If
            InsertSQL = InsertSQL + "INSERT INTO " + TableName + " (" + InsertFieldNames + ") VALUES (" + InsertFieldParameters + ")"



        End Sub

        Public Function GetSQLType(ByVal FieldMap As BLFieldMap) As String
            If Not FieldMap.Field Is Nothing Then
                Select Case FieldMap.Field.Name.ToUpper
                    Case "INTEGER"
                        Return "int"
                    Case "DOUBLE"
                        Return "float"
                    Case "STRING"
                        Return "varchar"
                    Case "DATETIME"
                        Return "datetime"
                    Case Else
                        Return "Unknown"
                End Select
            Else
                Return "Unknown"
            End If
        End Function

        Public Shared Function ToOLEDBParameterArray(ByRef ParameterList As List(Of DataLayerParameter)) As OleDbParameter()
            Dim ToReturn(ParameterList.Count - 1) As OleDbParameter
            Dim i As Integer = 0
            For Each Parameter As DataLayerParameter In ParameterList
                ToReturn(i) = Parameter.GetOLEDBParameter
                i = i + 1
            Next
            Return ToReturn
        End Function

        Public Sub ModifyCommandText(ByVal Criteria As BLCriteria, ByRef CommandText As String)

            Dim isSelectDistinct As Boolean = False

            Try
                Dim Tokens() As String
                Dim Separators As String = " "

                Tokens = CommandText.Split(Separators.ToCharArray)
                Dim SelectTokenIndex As Integer = 0
                While Tokens(SelectTokenIndex) <> "SELECT" And (SelectTokenIndex < Tokens.Length)
                    SelectTokenIndex = SelectTokenIndex + 1
                End While
                If Tokens(SelectTokenIndex + 1) = "DISTINCT" Then
                    isSelectDistinct = True
                End If

            Catch ex As System.Exception
            End Try

            If Not isSelectDistinct And Criteria.Distinct Then
                CommandText = Replace(CommandText, "SELECT", "SELECT DISTINCT ", 1, 1, CompareMethod.Text)
                isSelectDistinct = True
            End If

            If Criteria.MaxRegisters >= 0 Then
                CommandText = CommandText.ToUpper

                If isSelectDistinct Then
                    CommandText = Replace(CommandText, "DISTINCT", "DISTINCT TOP " + Criteria.MaxRegisters.ToString + " ", 1, 1, CompareMethod.Text)
                Else
                    CommandText = Replace(CommandText, "SELECT", "SELECT TOP " + Criteria.MaxRegisters.ToString + " ", 1, 1, CompareMethod.Text)
                End If
            End If
        End Sub



#End Region

#Region "FieldList Mapping"

        'Generates a string with the fields separated by a coma with the existing fields in FieldMapList 
        Public ReadOnly Property FieldListString(ByVal FieldMapList As BLFieldMapList, Optional ByVal TableName As String = "") As String
            Get
                If TableName = "" Then
                    TableName = m_TableName + "."
                Else
                    TableName = TableName + "."
                End If

                Dim FieldMap As BLFieldMap
                Dim ReturnString As String = ""
                Dim i As Integer
                For Each FieldMap In FieldMapList.OrderedFields
                    If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField And FieldMap.isFetchField Then
                        ReturnString = ReturnString + TableName + FieldMap.DLFieldName + ", "
                    End If
                Next

                If ReturnString.Length > 0 Then
                    ReturnString = Mid(ReturnString, 1, ReturnString.Length - 2)
                End If

                Return ReturnString
            End Get
        End Property

        'Generates a string with the fields separated by a coma with the existing fields in FieldMapCollection
        Public ReadOnly Property FieldCollectionString(ByVal FieldCollection As List(Of BLFieldMap)) As String
            Get
                Dim TableName As String = m_TableName + "."

                Dim ReturnString As String = ""
                For Each FieldMap As BLFieldMap In FieldCollection
                    If FieldMap.FieldMapType = BLFieldMap.FieldMapTypeEnum.DataField And FieldMap.isFetchField Then
                        ReturnString = ReturnString + TableName + FieldMap.DLFieldName + ", "
                    End If

                Next
                If ReturnString.Length > 0 Then
                    ReturnString = Mid(ReturnString, 1, ReturnString.Length - 2)
                End If

                Return ReturnString
            End Get
        End Property

        'Generates a string with the fields separated by a coma with the existing fields in FieldMapCollection
        'as SQL parameters
        Public ReadOnly Property ParameterString(ByVal FieldMapList As BLFieldMapList) As String
            Get
                Dim ToReturn As String = ""

                Dim FieldMap As BLFieldMap
                For Each FieldMap In FieldMapList.OrderedFields
                    ToReturn = ToReturn + "@" + FieldMap.DLFieldName + ", "
                Next
                If ToReturn <> "" Then
                    ToReturn = Mid(ToReturn, 1, ToReturn.Length - 2)
                End If
                Return ToReturn
            End Get
        End Property

        'Generates a string with the fields separated by a coma with the existing fields in FieldMapCollection
        'as SQL parameters
        Public ReadOnly Property ParameterString(ByVal FieldMapCollection As List(Of BLFieldMap)) As String
            Get
                Dim ToReturn As String = ""
                Dim FieldMap As BLFieldMap
                For Each FieldMap In FieldMapCollection
                    ToReturn = ToReturn + "@" + FieldMap.DLFieldName + ", "
                Next
                If ToReturn <> "" Then
                    ToReturn = Mid(ToReturn, 1, ToReturn.Length - 2)
                End If
                Return ToReturn
            End Get
        End Property


        'Generates the SQL croos joined query in the actual DataLayer of the object
        'describing the attribute added as a parameter.  Stores it in the cache with
        'name:  "PropertySelectSQL_PropertyName.  PropertyName  the name of the property
        'associated with the attribute
        Public Sub GenerateChachedForeignObjectSQL( _
            ByVal CachedForeignObjectAttribute As BLCore.Attributes.CachedForeignObjectAttribute)


            Dim BusinessObjectDataLayer As SQLDataLayer = CType(CachedForeignObjectAttribute.BusinessObject.DataLayer, SQLDataLayer)
            Dim TmpCriteria As New BLCriteria
            Dim ExternalSQL As String = BusinessObjectDataLayer.AssembledSelectSQL(TmpCriteria)

            Dim GeneratedSQL As String

            GeneratedSQL = "SELECT [FieldList] FROM ([InternalSQL]) InternalTable LEFT OUTER JOIN (" + _
                    BusinessObjectDataLayer.AssembledSelectSQL(TmpCriteria) + ")  ExternalTable ON " + CachedForeignObjectAttribute.InternalKeyName1 + " = " + CachedForeignObjectAttribute.ExternalKeyName1

            If CachedForeignObjectAttribute.InternalKeyName2 <> "" Then
                GeneratedSQL = GeneratedSQL + " AND " + CachedForeignObjectAttribute.InternalKeyName2 + " = " + CachedForeignObjectAttribute.ExternalKeyName2
            End If

            If CachedForeignObjectAttribute.InternalKeyName3 <> "" Then
                GeneratedSQL = GeneratedSQL + " AND " + CachedForeignObjectAttribute.InternalKeyName3 + " = " + CachedForeignObjectAttribute.ExternalKeyName3
            End If

            SQLCache("PropertySelectSQL_" + CachedForeignObjectAttribute.PropertyName) = GeneratedSQL
        End Sub


        'Obtains the SQL for the attribute CachedForeignObjectAttribute from the cache
        'and replaces the values of this SQL so it gets valid
        Public Function GetChachedForeignObjectSQL(ByVal CachedForeignObjectAttribute As BLCore.Attributes.CachedForeignObjectAttribute, _
            ByVal InternalSQL As String, ByVal FieldMapList As BLFieldMapList) As String

            Dim ToReturn As String = SQLCache("PropertySelectSQL_" + CachedForeignObjectAttribute.PropertyName)
            ToReturn = ToReturn.Replace("[InternalSQL]", InternalSQL)
            ToReturn = ToReturn.Replace("[FieldList]", FieldListString(FieldMapList))
            Return ToReturn

        End Function


        Public Function CachedObjectsSQL(ByVal MainSQL As String, ByVal CachedObjects As List(Of BLFieldMap)) As String
            Dim ToReturn As String = ""
            If CachedObjects.Count > 0 Then
                Dim i As Integer = 0
                For Each FieldMap As BLFieldMap In CachedObjects
                    Dim Attr As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(FieldMap.Field)
                    ToReturn = ToReturn + GetChachedForeignObjectSQL(Attr, MainSQL, FieldMapList) + vbCrLf
                Next
            End If
            Return ToReturn
        End Function


#End Region

#Region "Constructors"
        Public Sub New(ByVal BusinessObjectType As System.Type, ByVal TableName As String, _
            ByVal DataLayerFieldSuffix As String, Optional ByVal DataSourceName As String = "")
            MyBase.New(TableName, DataLayerFieldSuffix, BusinessObjectType, DataSourceName)
            Init(BusinessObjectType, DataSourceName)
        End Sub

        'Public Sub New(ByVal BusinessObjectTypeName As String, _
        '    ByVal TableName As String, _
        '    ByVal DataLayerFieldSuffix As String, _
        '    ByVal NUseStoredProcedures As String, _
        '    Optional ByVal SelectSpecialView As String = "False", _
        '    Optional ByVal DataSourceName As String = "")
        '    MyBase.New(TableName, DataLayerFieldSuffix, ReflectionHelper.GetTypeByName(BusinessObjectTypeName), DataSourceName)
        '    Init(ReflectionHelper.GetTypeByName(BusinessObjectTypeName), CBool(NUseStoredProcedures), CBool(SelectSpecialView), DataSourceName)
        'End Sub

        Public Sub New(ByVal ParameterList() As DataTypes.Pair(Of String, String))
            MyBase.New(ParameterList)


            Init(BusinessLogicType, DefaultDataSourceName)
        End Sub


        Private Sub Init(ByVal BusinessObjectType As System.Type, Optional ByVal DataSourceName As String = "")
            BeginDataDefinition()
            AddFieldsFromType(BusinessObjectType)
            EndDataDefinition(False)

            GenerateDynamicSQL()

        End Sub
#End Region

#Region "DataLayerAbstraction Overrides"

        Public Overrides Function InsertCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String = ""
            Dim CommandText As String
            Dim Parameters() As OleDbParameter = Nothing
            Dim InsertCmd As DataLayerOLEDBCommand = Nothing
            If InsertSQL <> "" Then
                CommandText = InsertSQL
                GetUpdateParameters(BusinessObject, Parameters)
                InsertCmd = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)

            End If

            Return InsertCmd
        End Function

        Public Overrides Function UpdateCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String = ""
            Dim CommandText As String
            Dim Parameters() As OleDbParameter = Nothing
            Dim UpdateCmd As DataLayerOLEDBCommand = Nothing
            If UpdateSQL <> "" Then
                Dim ParameterList As New List(Of DataLayerParameter)
                Filter = BusinessObject.KeyCriteria.GetFilter(ParameterList)
                Parameters = ToOLEDBParameterArray(ParameterList)
                GetUpdateParameters(BusinessObject, Parameters)
                CommandText = UpdateSQL + " WHERE " + Filter
                UpdateCmd = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
            End If
            Return UpdateCmd
        End Function

        Public Overrides Function DeleteCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String
            Dim CommandText As String
            Dim Parameters() As OleDbParameter
            Dim DeleteCmd As DataLayerOLEDBCommand = Nothing
            If DeleteSQL <> "" Then
                Dim ParameterList As New List(Of DataLayerParameter)
                Filter = BusinessObject.KeyCriteria.GetFilter(ParameterList)
                Parameters = ToOLEDBParameterArray(ParameterList)
                CommandText = DeleteSQL + " WHERE " + Filter
                DeleteCmd = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
            End If
            Return DeleteCmd
        End Function

        Public Overrides Function ReadCommand(ByVal Caller As Object, ByVal Criteria As BLCriteria, _
            Optional ByVal CachedObjects As List(Of BLFieldMap) = Nothing) As DataLayerCommandBase

            Dim Command As DataLayerOLEDBCommand
            Dim FilterText As String
            Dim Parameters As OleDbParameter() = Nothing
            Dim CommandText As String
            Dim Filter As DataLayerFilterBase = Criteria.Filter

            If TypeOf Caller Is BLBusinessBase Then
                Dim BOCaller As BLBusinessBase = CType(Caller, BLBusinessBase)
                Dim ParameterList As New List(Of DataLayerParameter)
                FilterText = Criteria.GetFilter(ParameterList)
                Parameters = ToOLEDBParameterArray(ParameterList)
                If CachedObjects Is Nothing Then
                    CachedObjects = FieldMapList.OnCreationCachedFields(Criteria)
                End If
                CommandText = AssembledSelectSQL(Criteria) + " Where " + FilterText
                CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                Command = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Parameters, BOCaller.DataLayerContextInfo.DataSourceName)

            Else
                Dim BO As BLBusinessBase
                Dim List As BLCore.IBLListBase = CType(Caller, BLCore.IBLListBase)
                BO = List.NewBusinessObject

                Dim ParameterList As New List(Of DataLayerParameter)
                FilterText = Criteria.GetFilter(ParameterList)
                If Filter Is Nothing Then
                    CommandText = AssembledSelectSQL(Criteria)
                    If FilterText <> "" Then
                        CommandText = CommandText + " Where " + FilterText + vbCrLf
                    End If
                Else
                    CommandText = Filter.SelectCommandText(Me, FieldMapList, FilterText, ParameterList)
                End If

                Parameters = ToOLEDBParameterArray(ParameterList)

                If FilterText <> "" Or Parameters.Length > 0 Then
                    CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                    ModifyCommandText(Criteria, CommandText)
                    Command = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Parameters, List.DataLayerContextInfo.DataSourceName)
                Else
                    CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                    ModifyCommandText(Criteria, CommandText)
                    Command = New DataLayerOLEDBCommand(Me, CommandText, CommandType.Text, Nothing, List.DataLayerContextInfo.DataSourceName)
                End If

            End If

            Return Command
        End Function
#End Region

#Region "Data Definition Overrides"

        Public Overrides ReadOnly Property DataDefinition_Fields() As BLFieldMapList
            Get
                Dim FieldsToReturn As New BLFieldMapList

                Dim cnn As OleDb.OleDbConnection
                Dim dtt As New DataTable
                cnn = New OleDb.OleDbConnection(DefaultConnectionString)
                Using cnn
                    cnn.Open()
                    Dim dta As New OleDb.OleDbDataAdapter("Select * from " & TableName, cnn)
                    dta.FillSchema(dtt, SchemaType.Source)
                    dta.Dispose()
                End Using

                Dim i As Integer = 0

                For Each Column As DataColumn In dtt.Columns
                    Dim isKey As Boolean = False
                    Dim AutonumericType As BLFieldMap.AutoNumericTypeEnum = BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric
                    Dim isUpdate As Boolean = True

                    Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.DynamicDataField, _
                        Mid(Column.ColumnName, 1, Column.ColumnName.Length - Me.DataLayerFieldSuffix.Length), _
                         isKey, True, isUpdate, i, AutonumericType, "", False)
                    FieldsToReturn.Add(FieldMap)
                    i = i + 1

                Next



                Return FieldsToReturn
            End Get
        End Property

        ''' <summary>
        ''' Table exists in the database
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function DataDefinition_Exists() As Boolean
            Dim Exists As Boolean = True
            Try
                Dim cnn As OleDb.OleDbConnection
                Dim dtt As New DataTable
                cnn = New OleDb.OleDbConnection(DefaultConnectionString)
                Using cnn
                    cnn.Open()
                    Dim dta As New OleDb.OleDbDataAdapter("Select * from " & TableName, cnn)
                    dta.Dispose()
                End Using
            Catch ex As Exception
                Exists = False
            End Try

            Return Exists

        End Function

        ''' <summary>
        ''' Creates the table in the database
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub DataDefinition_Create()
            'Dim CreateTableString As String = "CREATE TABLE [" + m_TableName + "] ("
            'For Each FieldMap As BLFieldMap In FieldMapList
            '    CreateTableString = CreateTableString + FieldMap.DLFieldName + " "
            '    CreateTableString = CreateTableString + FieldMap.DLFieldName + " "
            'Next

            'For Each FieldMap As BLFieldMap In FieldMapList
            '    If FieldMap.isKey Then
            '        CreateTableString = CreateTableString + FieldMap.DLFieldName + " "
            '        CreateTableString = CreateTableString + FieldMap.DLFieldName + " "
            '    End If
            'Next



            'CreateTableString = CreateTableString + ")"
            Throw New Exception(BLResources.Exception_DataLayer_TableCreationNotSupported)
        End Sub

#End Region
    End Class

End Namespace
