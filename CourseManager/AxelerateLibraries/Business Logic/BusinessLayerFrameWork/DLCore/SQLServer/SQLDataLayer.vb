Imports System.Reflection
Imports Data_Access_Application_Block
Imports System.Configuration.ConfigurationManager

Namespace DLCore

    'Class that all the business object musy inherit and contains the information
    'of the metadata generated from the atrributes of the business class
    Public Class SQLDataLayer
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

        'Object uses Stored Procedures or SQL Syntax
        Private m_UseStoredProcedures As Boolean = False

        'Private m_DataSourceName As String = ""

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

        'Gets or Sets m_SelectSQL based on a specific list of fields
        Public ReadOnly Property AssembledSelectSQL(ByVal Criteria As BLCriteria, ByVal FieldNames() As String) As String
            Get
                Dim CommandText As String = SelectSQL(SelectSQLPartType.SelectClausePart)
                If Criteria.Distinct Then
                    CommandText = CommandText + "DISTINCT "
                End If

                If (FieldNames.Length > 0) Then
                    For Each FieldName As String In FieldNames
                        CommandText = CommandText + FieldName + ", "
                    Next
                    CommandText = Mid(CommandText, 1, CommandText.Length - 2)
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

        'Current object uses Stored Procedures or SQL Syntax
        Public ReadOnly Property UseStoredProcedures() As Boolean
            Get
                Return m_UseStoredProcedures
            End Get
        End Property

        'Gets through the list of object fields and returns in Parameters
        'an array of SqlParameter that colud be used to complete operations of insert or update
        Protected Sub GetUpdateParameters(ByVal BusinessObject As BLBusinessBase, ByRef Parameters() As SqlParameter)
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
                Dim Param As SqlParameter
                Value = BusinessObject.FieldValue(Field)
                If Value Is Nothing Then
                    Value = System.DBNull.Value
                End If
                Param = New SqlParameter("@" + Field.DLFieldName, Value)

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
        Public Sub GenerateDynamicSQL(Optional ByVal SpecialSelectView As Boolean = False)

            Dim FieldMap As BLFieldMap
            Dim Enumerator As IDictionaryEnumerator
            Dim i As Integer

            If (Not m_UseStoredProcedures) Then
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
                    AutoNumericType = AutoNumericField.AutoNumericType
                    Select Case AutoNumericType

                        Case BLFieldMap.AutoNumericTypeEnum.GeneratedColumn
                            InsertFieldNames = InsertFieldNames + " " + AutoNumericField.DLFieldName + ", "
                            InsertFieldParameters = InsertFieldParameters + " @@GeneratedColumn, "
                            GeneratedColumnSPName = " GNC_SP_" + TableName + "_" + AutoNumericField.DLFieldName

                            If AutoNumericField.Field.FieldType Is GetType(String) Then
                                AutoNumericDBFieldType = "VARCHAR(255)"
                            ElseIf AutoNumericField.Field.FieldType Is GetType(Integer) Then
                                AutoNumericDBFieldType = "INT"
                            End If
                            Dim RelevantFields As List(Of BLFieldMap) = FieldMapList.AutoNumericRelevantFields
                            AutoNumericReferenceField = ParameterString(RelevantFields)

                    End Select
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
                If SpecialSelectView Then
                    SelectSQL(SelectSQLPartType.FromClausePart) = " FROM VIW_" + TableName
                Else
                    SelectSQL(SelectSQLPartType.FromClausePart) = " FROM " + TableName
                End If
                DeleteSQL = "Delete FROM " + TableName

                UpdateSQL = "UPDATE " + TableName + " SET " + UpdateFieldParameters

                InsertSQL = ""
                If AutoNumericType = BLFieldMap.AutoNumericTypeEnum.GeneratedColumn Then
                    InsertSQL = "DECLARE @@GeneratedColumn " + AutoNumericDBFieldType + vbCrLf + _
                                " EXECUTE " + GeneratedColumnSPName + " @@GeneratedColumn OUTPUT "
                    If AutoNumericReferenceField <> "" Then
                        InsertSQL = InsertSQL + ", " + AutoNumericReferenceField
                    End If
                    InsertSQL = InsertSQL + vbCrLf + _
                                " SELECT @@GeneratedColumn as 'Identity' " + vbCrLf
                End If
                InsertSQL = InsertSQL + "INSERT INTO " + TableName + " (" + InsertFieldNames + ") VALUES (" + InsertFieldParameters + ")"
                If AutoNumericType = BLFieldMap.AutoNumericTypeEnum.IdentityColumn Then
                    InsertSQL = InsertSQL + vbCrLf + "SELECT @@IDENTITY AS 'Identity'"
                End If


            Else
                SelectSQL(SelectSQLPartType.StoreProcedurePart) = "SEL_" + TableName.ToUpper
                DeleteSQL = "DEL_" + TableName.ToUpper
                InsertSQL = "INS_" + TableName.ToUpper
                UpdateSQL = "UPD_" + TableName.ToUpper
            End If


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

        Public Shared Function ToSQLParameterArray(ByRef ParameterList As List(Of DataLayerParameter)) As SqlParameter()
            Dim ToReturn(ParameterList.Count - 1) As SqlParameter
            Dim i As Integer = 0
            For Each Parameter As DataLayerParameter In ParameterList
                ToReturn(i) = Parameter.GetSQLParameter
                i = i + 1
            Next
            Return ToReturn
        End Function

        Public Shared Function ToSQLParameterArray(ByRef ParameterArray() As DataLayerParameter) As SqlParameter()
            Dim ToReturn(ParameterArray.Length - 1) As SqlParameter
            Dim i As Integer = 0
            For Each Parameter As DataLayerParameter In ParameterArray
                ToReturn(i) = Parameter.GetSQLParameter
                i = i + 1
            Next
            Return ToReturn
        End Function


        Public Sub ModifyCommandText(ByVal Criteria As BLCriteria, ByRef CommandText As String)

            Dim isSelectDistinct As Boolean = False

            'Adds the distinct keyword if necessary
            Try
                Dim Tokens() As String
                Dim Separators As String = " "

                Tokens = CommandText.Split(Separators.ToCharArray)
                Dim SelectTokenIndex As Integer = 0
                While (SelectTokenIndex < Tokens.Length) AndAlso (Tokens(SelectTokenIndex).ToUpper() <> "SELECT")
                    SelectTokenIndex = SelectTokenIndex + 1
                End While
                If Tokens(SelectTokenIndex + 1).ToUpper() = "DISTINCT" Then
                    isSelectDistinct = True
                End If

            Catch ex As System.Exception
            End Try

            If Not isSelectDistinct And Criteria.Distinct Then
                CommandText = Replace(CommandText, "SELECT", "SELECT DISTINCT ", 1, 1, CompareMethod.Text)
                isSelectDistinct = True
            End If

            'Adds the top keyword if necessary
            If Criteria.MaxRegisters >= 0 Then
                CommandText = CommandText.ToUpper

                If isSelectDistinct Then
                    CommandText = Replace(CommandText, "DISTINCT", "DISTINCT TOP " + Criteria.MaxRegisters.ToString + " ", 1, 1, CompareMethod.Text)
                Else
                    CommandText = Replace(CommandText, "SELECT", "SELECT TOP " + Criteria.MaxRegisters.ToString + " ", 1, 1, CompareMethod.Text)
                End If
            End If


            'Creates the orderby string
            Dim OrderByString As String = ""
            If Not Criteria.OrderByFields Is Nothing AndAlso Criteria.OrderByFields.Count > 0 Then
                OrderByString = " Order By "
                For Each OrderByElement As BLCore.DataTypes.Pair(Of String, Boolean) In Criteria.OrderByFields
                    OrderByString = OrderByString + OrderByElement.First
                    If Not OrderByElement.Second Then
                        OrderByString = OrderByString + " DESC "
                    End If
                    OrderByString = OrderByString + ", "
                Next
                OrderByString = OrderByString.Substring(0, OrderByString.Length - 2)
            End If

            'Adds Paging Support if necessary
            If (Criteria.UsePaging) And (OrderByString <> "") Then
                Dim RegexFinder As New System.Text.RegularExpressions.Regex("[\)\x20\r\n\t]FROM[\(\x20\r\n\t]", Text.RegularExpressions.RegexOptions.IgnoreCase)
                Dim mMatch As System.Text.RegularExpressions.Match
                mMatch = RegexFinder.Match(CommandText, 0)
                If Not mMatch.Success Then
                    Throw New System.Exception("Error trying to add paging supoort.  FROM Caluse not found in SQL Command.\nSelect Command: " + CommandText)
                End If
                Dim FirstFromClauseIndex As Integer = mMatch.Captures(0).Index + 1 'CommandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase)
                If FirstFromClauseIndex < 0 Then
                    Throw New System.Exception("Error trying to add paging supoort.  FROM Caluse not found in SQL Command.\nSelect Command: " + CommandText)
                End If
                CommandText = CommandText.Insert(FirstFromClauseIndex, ", ROW_NUMBER() OVER (" + OrderByString + ") AS RowNumber ")
                CommandText = "With PagedTable AS (" + CommandText + ") "
                CommandText = CommandText + " SELECT * FROM PagedTable"
                CommandText = CommandText + " WHERE RowNumber BETWEEN " + Criteria.PageStartIndex.ToString() + " AND " + Criteria.PageEndIndex.ToString()
            End If


            'Adds the order by keyword if necessary
            If (OrderByString <> "") Then
                CommandText = CommandText + OrderByString
            End If

            'Morphs the command into a select count(*) command if necessary
            If (Criteria.IsCount) Then
                CommandText = "Select Count(*) AS ResultCount FROM (" + CommandText + ") AS CountTable"
            End If

        End Sub

        ''' <summary>
        ''' Builds the Text needed to call the stored procedure with the supplied parameters
        ''' </summary>
        ''' <param name="StoreProcedureName"></param>
        ''' <param name="SQLParameters"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function BuildStoredProcedureText(ByVal StoreProcedureName As String, ByVal SQLParameters() As SqlParameter) As String
            Dim CommandText As String = StoreProcedureName
            'If Not SQLParameters Is Nothing AndAlso SQLParameters.Length > 0 Then
            '    CommandText = CommandText + " "
            '    For Each Parameter As SqlParameter In SQLParameters
            '        CommandText = CommandText + "@" + Parameter.ParameterName + ", "
            '    Next
            '    CommandText = Mid(CommandText, 1, CommandText.Length - 2)
            'End If
            Return CommandText
        End Function



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

        'Generates a string with the fields separated by a coma with the existing fields in FieldMapList 
        Public ReadOnly Property FieldListString(ByVal FieldNames() As String, Optional ByVal TableName As String = "") As String
            Get
                If TableName = "" Then
                    TableName = m_TableName + "."
                Else
                    TableName = TableName + "."
                End If

                Dim ReturnString As String = ""
                For Each FieldName As String In FieldNames
                    ReturnString = ReturnString + TableName + FieldName + ", "
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
            If Not CachedObjects Is Nothing AndAlso CachedObjects.Count > 0 Then
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

        Public Sub New(ByVal DataSourceName As String)
            MyBase.New(DataSourceName)


        End Sub
        Public Sub New(ByVal BusinessObjectType As System.Type, ByVal TableName As String, _
            ByVal DataLayerFieldSuffix As String, ByVal NUseStoredProcedures As Boolean, _
            Optional ByVal SelectSpecialView As Boolean = False, Optional ByVal DataSourceName As String = "")
            MyBase.New(TableName, DataLayerFieldSuffix, BusinessObjectType, DataSourceName)
            Init(BusinessObjectType, NUseStoredProcedures, SelectSpecialView, DataSourceName)
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

            Dim SelectSpecialViewParameter As Boolean = False
            Dim UseStoredProceduresParameter As Boolean = False

            For Each Pair As DataTypes.Pair(Of String, String) In ParameterList

                Select Case Pair.First
                    Case "SelectSpecialView"
                        SelectSpecialViewParameter = CBool(Pair.Second)
                    Case "UseStoredProcedures"
                        UseStoredProceduresParameter = CBool(Pair.Second)
                End Select
            Next

            Init(BusinessLogicType, UseStoredProceduresParameter, SelectSpecialViewParameter, DefaultDataSourceName)
        End Sub


        Private Sub Init(ByVal BusinessObjectType As System.Type, ByVal NUseStoredProcedures As Boolean, _
            Optional ByVal SelectSpecialView As Boolean = False, Optional ByVal DataSourceName As String = "")
            BeginDataDefinition()
            AddFieldsFromType(BusinessObjectType)

            Dim CheckRepository As Boolean = ConfigurationHelper.BoolAppSetting("CheckRepositoryDefinition")
            If SelectSpecialView Then
                CheckRepository = False
            End If
            EndDataDefinition(CheckRepository)

            m_UseStoredProcedures = NUseStoredProcedures
            GenerateDynamicSQL(SelectSpecialView)

        End Sub
#End Region

#Region "DataLayerAbstraction Overrides"

        Public Overrides Function InsertCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String = ""
            Dim CommandText As String
            Dim Parameters() As SqlParameter = Nothing
            Dim InsertCmd As DataLayerSQLCommand = Nothing
            If InsertSQL <> "" Then
                CommandText = InsertSQL
                GetUpdateParameters(BusinessObject, Parameters)


                If UseStoredProcedures Then
                    InsertCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.StoredProcedure, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                Else
                    InsertCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                End If
            End If

            Return InsertCmd
        End Function

        Public Overrides Function UpdateCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String = ""
            Dim CommandText As String
            Dim Parameters() As SqlParameter = Nothing
            Dim UpdateCmd As DataLayerSQLCommand = Nothing
            If UpdateSQL <> "" Then
                If UseStoredProcedures Then
                    GetUpdateParameters(BusinessObject, Parameters)
                    CommandText = UpdateSQL
                    UpdateCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.StoredProcedure, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                Else
                    Dim ParameterList As New List(Of DataLayerParameter)
                    Filter = BusinessObject.KeyCriteria.GetFilter(ParameterList)
                    Parameters = ToSQLParameterArray(ParameterList)
                    GetUpdateParameters(BusinessObject, Parameters)
                    CommandText = UpdateSQL + " WHERE " + Filter
                    UpdateCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                End If
            End If
            Return UpdateCmd
        End Function

        Public Overrides Function DeleteCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim Filter As String
            Dim CommandText As String
            Dim Parameters() As SqlParameter
            Dim DeleteCmd As DataLayerSQLCommand = Nothing
            If DeleteSQL <> "" Then
                Dim ParameterList As New List(Of DataLayerParameter)
                Filter = BusinessObject.KeyCriteria.GetFilter(ParameterList)
                Parameters = ToSQLParameterArray(ParameterList)
                If UseStoredProcedures Then
                    CommandText = DeleteSQL
                    DeleteCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.StoredProcedure, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                Else
                    CommandText = DeleteSQL + " WHERE " + Filter
                    DeleteCmd = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, BusinessObject.DataLayerContextInfo.DataSourceName)
                End If
            End If
            Return DeleteCmd
        End Function

        Public Overrides Function ReadCommand(ByVal Caller As Object, ByVal Criteria As BLCriteria, _
            Optional ByVal CachedObjects As List(Of BLFieldMap) = Nothing) As DataLayerCommandBase

            Dim Command As DataLayerSQLCommand
            Dim FilterText As String
            Dim Parameters As SqlParameter() = Nothing
            Dim CommandText As String
            Dim Filter As DataLayerFilterBase = Criteria.Filter

            If TypeOf Caller Is BLBusinessBase Then
                Dim BOCaller As BLBusinessBase = CType(Caller, BLBusinessBase)
                Dim ParameterList As New List(Of DataLayerParameter)
                FilterText = Criteria.GetFilter(ParameterList)
                Parameters = ToSQLParameterArray(ParameterList)
                If (UseStoredProcedures) Then
                    Command = New DataLayerSQLCommand(Me, SelectSQL(SelectSQLPartType.StoreProcedurePart), CommandType.StoredProcedure, Parameters, BOCaller.DataLayerContextInfo.DataSourceName)
                Else
                    If CachedObjects Is Nothing Then
                        CachedObjects = FieldMapList.OnCreationCachedFields(Criteria)
                    End If
                    CommandText = AssembledSelectSQL(Criteria)
                    If (FilterText.Trim() <> "") Then
                        CommandText = CommandText + " Where " + FilterText
                    End If
                    CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                    Command = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, BOCaller.DataLayerContextInfo.DataSourceName)
                End If
            Else
                Dim BO As BLBusinessBase
                Dim List As BLCore.IBLListBase = CType(Caller, BLCore.IBLListBase)
                BO = List.NewBusinessObject

                Dim ParameterList As New List(Of DataLayerParameter)
                FilterText = Criteria.GetFilter(ParameterList)
                If UseStoredProcedures Then
                    If Filter Is Nothing Then
                        CommandText = SelectAllFunction
                    Else
                        CommandText = "" ' Filter.FilterProcedure

                    End If
                    CommandText = "Select * From " + CommandText + " SelectAllTable "

                Else
                    If Filter Is Nothing Then
                        CommandText = AssembledSelectSQL(Criteria)
                        If FilterText <> "" Then
                            CommandText = CommandText
                            If (FilterText.Trim() <> "") Then
                                CommandText = CommandText + " Where " + FilterText
                            End If
                            CommandText = CommandText + vbCrLf
                        End If
                    Else
                        CommandText = Filter.SelectCommandText(Me, FieldMapList, FilterText, ParameterList)
                    End If
                End If

                Parameters = ToSQLParameterArray(ParameterList)

                If FilterText <> "" Or Parameters.Length > 0 Then
                    CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                    ModifyCommandText(Criteria, CommandText)
                    Command = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, List.DataLayerContextInfo.DataSourceName)
                Else
                    CommandText = CommandText + vbCrLf + CachedObjectsSQL(CommandText, CachedObjects)
                    ModifyCommandText(Criteria, CommandText)
                    Command = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Nothing, List.DataLayerContextInfo.DataSourceName)
                End If

            End If

            Return Command
        End Function

        ''' <summary>
        ''' Creates a command that will perform a operation on the data store based on the information
        ''' contained in the caller
        ''' </summary>
        ''' <param name="Caller"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ExecutionCommand(ByVal Caller As IBLCommandBase) As DataLayerCommandBase
            Dim SQLParemeters() As SqlParameter = SQLDataLayer.ToSQLParameterArray(Caller.Parameters)
            Dim CommandText As String = BuildStoredProcedureText(Caller.CommandName, SQLParemeters)
            Dim ToReturn As DataLayerSQLCommand = Nothing
            If (Caller.IsStoredProcedure) Then
                ToReturn = New DataLayerSQLCommand(CType(Caller.DataLayer, SQLDataLayer), CommandText, _
                System.Data.CommandType.StoredProcedure, SQLParemeters, Caller.DataSourceName)
            Else
                ToReturn = New DataLayerSQLCommand(CType(Caller.DataLayer, SQLDataLayer), CommandText, _
                    System.Data.CommandType.Text, SQLParemeters, Caller.DataSourceName)
            End If
            Return ToReturn
        End Function

        ''' <summary>
        ''' Returns a command that will select all the disctinc values for the specified Fields
        ''' </summary>
        ''' <param name="Caller"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ValueListCommand(ByVal Caller As IBLListBase, ByVal FieldNames() As String, ByVal Criteria As BLCriteria) As DataLayerCommandBase
            Dim Command As DataLayerSQLCommand
            Dim FilterText As String
            Dim Parameters As SqlParameter() = Nothing
            Dim CommandText As String
            Dim Filter As DataLayerFilterBase = Criteria.Filter
            Criteria.Distinct = True

            If (FieldNames Is Nothing) Or (FieldNames.Length = 0) Then
                Throw New System.Exception(String.Format(BLResources.Exception_InvalidArgument, "FieldNames"))
            End If

            If UseStoredProcedures Then
                Throw New System.Exception(BLResources.Exception_DataLayer_NotSupportedForStoredProcedures)
            End If
            Dim BO As BLBusinessBase
            Dim List As BLCore.IBLListBase = Caller
            BO = List.NewBusinessObject

            Dim ParameterList As New List(Of DataLayerParameter)
            FilterText = Criteria.GetFilter(ParameterList)
            If Filter Is Nothing Then
                CommandText = AssembledSelectSQL(Criteria, FieldNames)
                If FilterText <> "" Then
                    CommandText = CommandText
                    If (FilterText.Trim() <> "") Then
                        CommandText = CommandText + " Where " + FilterText
                    End If
                    CommandText = CommandText + vbCrLf
                End If
            Else
                CommandText = "SELECT DISTINCT " + FieldListString(FieldNames) + " FROM (" + Filter.SelectCommandText(Me, FieldMapList, FilterText, ParameterList) + ") AS " + m_TableName
            End If

            Parameters = ToSQLParameterArray(ParameterList)

            If FilterText <> "" Or Parameters.Length > 0 Then
                ModifyCommandText(Criteria, CommandText)
                Command = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Parameters, List.DataLayerContextInfo.DataSourceName)
            Else
                ModifyCommandText(Criteria, CommandText)
                Command = New DataLayerSQLCommand(Me, CommandText, CommandType.Text, Nothing, List.DataLayerContextInfo.DataSourceName)
            End If

            Return Command

        End Function

#End Region

#Region "Data Definition Overrides"

        Public Overrides ReadOnly Property DataDefinition_Fields() As BLFieldMapList
            Get
                Dim FieldsToReturn As New BLFieldMapList
                Dim Columns As BCSQLInformationSchema_Columns
                Dim KeyColumns As BCSQLInformationSchema_Keys

                'Obtains the columns for this table
                Dim ColumnsCriteria As New BLCriteria
                ColumnsCriteria.AddBinaryExpression("TableName_isc", "TableName", "=", TableName)
                ColumnsCriteria.DataLayerContextInfo.DataSourceName = DefaultDataSourceName
                Columns = BCSQLInformationSchema_Columns.GetCollection(ColumnsCriteria)

                'Obtains the key columns for this table
                Dim KeyColumnsCriteria As New BLCriteria
                KeyColumnsCriteria.AddBinaryExpression("TableName_isk", "TableName", "=", TableName)
                KeyColumnsCriteria.DataLayerContextInfo.DataSourceName = DefaultDataSourceName
                KeyColumns = BCSQLInformationSchema_Keys.GetCollection(KeyColumnsCriteria)


                For Each Column As BOSQLInformationSchema_Columns In Columns
                    Dim isKey As Boolean = False
                    Dim AutonumericType As BLFieldMap.AutoNumericTypeEnum = BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric
                    Dim isUpdate As Boolean = True

                    Dim DataKey As New BOSQLInformationSchema_Keys.SQLSchemaDataKey(Nothing)

                    DataKey.m_ID = Column.ID
                    If KeyColumns.Contains(DataKey) Then
                        isKey = True
                        isUpdate = False
                    End If

                    Dim FieldMap As New BLFieldMap(Me, BLFieldMap.FieldMapTypeEnum.DynamicDataField, _
                        Mid(Column.ColumnName, 1, Column.ColumnName.Length - Me.DataLayerFieldSuffix.Length), _
                         isKey, True, isUpdate, Column.Position, AutonumericType, "", False)
                    FieldsToReturn.Add(FieldMap)
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
            Dim Tables As BCSQLInformationSchema_Tables
            'Obtains the columns for this table
            Dim TablesCriteria As New BLCriteria
            TablesCriteria.AddBinaryExpression("TableName_ist", "TableName", "=", TableName)
            TablesCriteria.DataLayerContextInfo.DataSourceName = DefaultDataSourceName
            Tables = BCSQLInformationSchema_Tables.GetCollection(TablesCriteria)

            Return Tables.Count > 0

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
