Imports System.Runtime.Serialization

Namespace BLCore

    ''' <summary>
    ''' Concrete implementation of the criteria object and 
    ''' its main purpose is to filter the results of fetch operations.  
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class BLCriteria

#Region "Private Data"
        ''' <summary>
        ''' Base type from which Criteria classes
        ''' </summary>
        ''' <remarks></remarks>
        Private mObjectType As Type

        ''' <summary>
        ''' Expression that will be used to determine if a register meets the Criteria
        ''' </summary>
        ''' <remarks></remarks>
        Private m_Expression As BLCriteriaExpression = New BLCriteriaExpression.ListExpression(Guid.NewGuid.ToString)

        ''' <summary>
        ''' Lists the different elements that composes this filter. Type of BLCriteriaParameter
        ''' </summary>
        ''' <remarks></remarks>
        Private m_UseAllCachedFields As Boolean = False

        'Passes duirng creation of the objects a more powerfull filter
        Private m_Filter As DataLayerFilterBase = Nothing

        'Number of maximum resgisters
        Private m_MaxRegisters As Integer = -1

        ''' <summary>
        ''' Creation parameters that mark which Datalayer is going to be used
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DataLayerContextInfo As DataLayerContextInfo

        ''' <summary>
        ''' Cache fields that will be loaded during object reading
        ''' </summary>
        ''' <remarks></remarks>
        Private m_CachedFieldListToLoad As New Hashtable

        ''' <summary>
        ''' Should reapeated results be ignored in the query
        ''' </summary>
        ''' <remarks></remarks>
        Private m_Distinct As Boolean = False

        Private m_OrderByFields As Collections.Generic.List(Of BLCore.DataTypes.Pair(Of String, Boolean)) = New List(Of BLCore.DataTypes.Pair(Of String, Boolean))()

        Private m_UsePaging As Boolean = False

        Private m_PageSize As Integer = 0

        Private m_PageNumber As Integer = 0

        Private m_IsCount As Boolean = False

        ''' <summary>
        ''' When retrieving a business object, if true, disables the default behavior in which
        ''' the retrieval generates an exception if there is no data to read.
        ''' </summary>
        ''' <remarks></remarks>
        Private m_TryGet As Boolean = False



#End Region

#Region "Prefilter Private Data"

        ''' <summary>
        ''' List of PreFilter arguments.
        ''' </summary>
        ''' <remarks></remarks>
        Private m_PreFilters As List(Of Pair(Of String, Object)) = New List(Of Pair(Of String, Object))

        Private m_FactoryMethodName As String = ""

#End Region
#Region "Constructors"

        'Initialize an empty Criteria
        Public Sub New(Optional ByVal NObjectType As Type = Nothing)
            'MyBase.New(NObjectType)
            m_Expression.NextOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
        End Sub

        'Initializes a new Criteria based on another Criteria
        Public Sub New(ByVal NCriteria As BLCriteria, Optional ByVal NObjectType As Type = Nothing)
            'MyBase.New(NObjectType)

            If Not NCriteria Is Nothing Then
                m_Distinct = NCriteria.Distinct
                m_DataLayerContextInfo = New DataLayerContextInfo(NCriteria.DataLayerContextInfo)
                m_Expression = CType(NCriteria.m_Expression.clone, BLCriteriaExpression)
                m_UseAllCachedFields = NCriteria.m_UseAllCachedFields
                m_Filter = NCriteria.Filter
                m_MaxRegisters = NCriteria.MaxRegisters
                m_TryGet = NCriteria.TryGet
                Dim Enumerator As IDictionaryEnumerator = NCriteria.m_CachedFieldListToLoad.GetEnumerator
                OrderByFields = NCriteria.OrderByFields

                For i As Integer = 0 To NCriteria.m_CachedFieldListToLoad.Count - 1
                    Enumerator.MoveNext()
                    Dim Value As String
                    Value = CType(Enumerator.Value, String)
                    LoadCachedObject(Value)
                Next

                m_UsePaging = NCriteria.m_UsePaging
                m_PageSize = NCriteria.m_PageSize
                m_PageNumber = NCriteria.m_PageNumber
                m_IsCount = NCriteria.m_IsCount
            End If

            Dim PreFilter As Pair(Of String, Object) = Nothing
            For Each PreFilter In NCriteria.m_PreFilters
                Dim ToAdd As Pair(Of String, Object) = New Pair(Of String, Object)(PreFilter.First, PreFilter.Second)
                m_PreFilters.Add(ToAdd)
            Next

            m_FactoryMethodName = NCriteria.m_FactoryMethodName

        End Sub

#End Region

#Region "Properties"

        ''' <summary>
        ''' Type of the business object to be instantiated by
        ''' the server-side DataPortal. 
        ''' </summary>
        Public ReadOnly Property ObjectType() As Type
            Get
                Return mObjectType
            End Get
        End Property


        Public Property TryGet() As Boolean
            Get
                Return m_TryGet
            End Get
            Set(ByVal value As Boolean)
                m_TryGet = value

            End Set
        End Property
        Public Property DataLayerContextInfo() As DataLayerContextInfo
            Get
                If m_DataLayerContextInfo Is Nothing Then
                    m_DataLayerContextInfo = New DataLayerContextInfo(ObjectType)
                End If
                Return m_DataLayerContextInfo
            End Get
            Set(ByVal value As DataLayerContextInfo)
                m_DataLayerContextInfo = value
            End Set
        End Property

        Public Property Expression() As BLCriteriaExpression
            Get
                Return m_Expression
            End Get
            Set(ByVal value As BLCriteriaExpression)
                m_Expression = value
            End Set
        End Property


#End Region

#Region "Filter Properties"
        Public Property OrderByFields() As List(Of BLCore.DataTypes.Pair(Of String, Boolean))
            Get
                Return m_OrderByFields
            End Get
            Set(ByVal value As List(Of BLCore.DataTypes.Pair(Of String, Boolean)))
                m_OrderByFields = value
            End Set
        End Property

        Public Sub AddOrderedField(ByVal FieldName As String, Optional ByVal Ascending As Boolean = True)
            If m_OrderByFields Is Nothing Then
                m_OrderByFields = New List(Of BLCore.DataTypes.Pair(Of String, Boolean))()
            End If

            'Checks if the ascending or descending modifiers comes embedded in the fieled name
            FieldName = FieldName.Trim()
            If FieldName.EndsWith("ASC", StringComparison.OrdinalIgnoreCase) Or FieldName.EndsWith("ASCENDING", StringComparison.OrdinalIgnoreCase) Then
                Ascending = True
            End If

            If FieldName.EndsWith("DESC", StringComparison.OrdinalIgnoreCase) Or FieldName.EndsWith("DESCENDING", StringComparison.OrdinalIgnoreCase) Then
                Ascending = False
            End If

            'Extracts the first token as the Field Name
            FieldName = FieldName.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)(0).Trim()

            Dim AlreadyAdded As Boolean = False
            For Each Pair As BLCore.DataTypes.Pair(Of String, Boolean) In m_OrderByFields
                If String.Compare(Pair.First, FieldName, True) = 0 Then
                    Pair.First = FieldName
                    Pair.Second = Ascending
                    AlreadyAdded = True
                    Exit For
                End If
            Next
            If Not AlreadyAdded Then
                m_OrderByFields.Add(New Pair(Of String, Boolean)(FieldName, Ascending))
            End If

        End Sub

        Public Property MaxRegisters() As Integer
            Get
                Return m_MaxRegisters
            End Get
            Set(ByVal Value As Integer)
                m_MaxRegisters = Value
            End Set
        End Property

        Public Property UseAllCacheFields() As Boolean
            Get
                Return m_UseAllCachedFields

            End Get
            Set(ByVal Value As Boolean)
                m_UseAllCachedFields = Value

            End Set
        End Property

        'Passes during creation of the objects a more powerfull filter
        Public Property Filter() As DataLayerFilterBase

            Get
                Return m_Filter
            End Get
            Set(ByVal Value As DataLayerFilterBase)
                m_Filter = Value
            End Set
        End Property


        'Returns a text filter that includes the SQL clauses of the type WHERE.
        'Also returns an array of parameters of ParameterList
        'in case the returned text needs it
        Public Overridable Function GetFilter(ByRef ParameterList As List(Of DataLayerParameter)) As String
            Return m_Expression.GetFilter(ParameterList)
        End Function


        'Returns a representative text chain of the elements contained in the object
        Public Overridable Function GetFilterText() As String
            Return m_Expression.GetFilterText()
        End Function


        ''' <summary>
        ''' Adds a new parameter for identification. This criteria must include the name in the database, the name
        ''' of the field mappeable field in the object, the operator of the criteria ( "=", ">", etc), and the value
        ''' DBFieldName could have a  ":xxx" after the name of the field.  This substring willbe used for
        ''' identifing the parameter, but will be ignored by the SQL instructions
        ''' </summary>
        ''' <param name="DBFieldName"></param>
        ''' <param name="ObjectFieldName"></param>
        ''' <param name="OperatorStr"></param>
        ''' <param name="Value"></param>
        ''' <param name="ConcatenationOperator"></param>
        ''' <remarks></remarks>
        Public Sub AddBinaryExpression(ByVal DBFieldName As String, ByVal ObjectFieldName As String, ByVal OperatorStr As String, ByVal Value As Object, _
              Optional ByVal ConcatenationOperator As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorAnd)
            Dim ParameterFieldName As String = DBFieldName
            If DBFieldName.Contains(":") Then
                Dim Pos As Integer = DBFieldName.IndexOf(":")
                DBFieldName = Mid(DBFieldName, 1, Pos)
            End If
            ParameterFieldName = ParameterFieldName.Replace(":", "_")
            Dim BinaryExpression As New BLCriteriaExpression.BinaryOperatorExpression(ParameterFieldName, DBFieldName, ObjectFieldName, OperatorStr, Value)
            AddExpression(BinaryExpression, ConcatenationOperator)
        End Sub

        ''' <summary>
        ''' Adds a new expression to the list of expressions of this Criteria object 
        ''' </summary>
        ''' <param name="Expression"></param>
        ''' <param name="ConcatenationOperator"></param>
        ''' <remarks>If the expression of this object is not a list of expressions, creates a new list and inserts both expressions</remarks>
        Public Sub AddExpression(ByVal Expression As BLCriteriaExpression, _
        Optional ByVal ConcatenationOperator As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorAnd)
            If Expression.GetType Is GetType(BLCriteriaExpression.ListExpression) Then
                Dim ListExpression As BLCriteriaExpression.ListExpression = CType(Expression, BLCriteriaExpression.ListExpression)
                If ListExpression.Expressions.Count = 0 Then
                    Return
                End If
            End If

            If m_Expression.GetType Is GetType(BLCriteriaExpression.ListExpression) Then
                Dim ListExpression As BLCriteriaExpression.ListExpression = CType(m_Expression, BLCriteriaExpression.ListExpression)
                ListExpression.AddExpression(Expression, ConcatenationOperator)
            Else
                Dim ListExpression As New BLCriteriaExpression.ListExpression(Guid.NewGuid.ToString)
                ListExpression.AddExpression(m_Expression, BLCriteriaExpression.BLCriteriaOperator.OperatorNone)
                ListExpression.AddExpression(Expression, ConcatenationOperator)
                m_Expression = ListExpression
            End If
        End Sub

        'Deletes the parameters of the database
        Public Sub ClearParameters()
            m_Expression = New BLCriteriaExpression.ListExpression(Guid.NewGuid.ToString)
        End Sub


        'Returns the value of the parameter which mapeable name field
        'in the database is ParameterName
        Default Public ReadOnly Property Item(ByVal ParameterName As String) As BLCriteriaExpression
            Get
                Return m_Expression.FindExpression(ParameterName)
            End Get
        End Property

        Public Property Distinct() As Boolean
            Get
                Return m_Distinct
            End Get
            Set(ByVal value As Boolean)
                m_Distinct = value
            End Set
        End Property

#End Region

#Region "Count Property"
        Friend Property IsCount() As Boolean
            Get
                Return m_IsCount
            End Get
            Set(ByVal value As Boolean)
                m_IsCount = value
                If (value) Then
                    PageSize = 0
                    m_OrderByFields = New List(Of BLCore.DataTypes.Pair(Of String, Boolean))()
                End If
            End Set
        End Property
#End Region

#Region "Paging Properties"
        Public Property PageSize() As Integer
            Get
                If m_UsePaging Then
                    Return m_PageSize
                Else
                    Return 0
                End If
            End Get

            Set(ByVal value As Integer)
                If value = 0 Then
                    m_UsePaging = False
                    m_PageSize = 0
                    m_PageNumber = 0
                Else
                    m_UsePaging = True
                    m_PageSize = value
                End If
            End Set
        End Property

        Public Property PageNumber() As Integer
            Get
                If (m_UsePaging) Then
                    Return m_PageNumber
                Else
                    Return 0
                End If
            End Get
            Set(ByVal value As Integer)
                If (m_UsePaging) Then
                    m_PageNumber = value
                Else
                    m_PageNumber = 0
                    Throw New System.Exception(My.Resources.BLResources.errPageSize)
                End If
            End Set
        End Property

        Public ReadOnly Property UsePaging() As Boolean
            Get
                Return m_UsePaging
            End Get
        End Property

        Public ReadOnly Property PageStartIndex() As Integer
            Get
                If m_UsePaging Then
                    Return (PageSize * PageNumber) + 1
                Else
                    Return 0
                End If

            End Get
        End Property

        Public ReadOnly Property PageEndIndex() As Integer
            Get
                If m_UsePaging Then
                    Return (PageSize * PageNumber) + PageSize
                Else
                    Return 0
                End If


            End Get
        End Property
#End Region

#Region "PreFilter Properties"
        Public ReadOnly Property PreFilters() As List(Of Pair(Of String, Object))
            Get
                Return m_PreFilters
            End Get
        End Property

        ''' <summary>
        ''' Adds a PreFilter Argument to the criteria
        ''' </summary>
        ''' <param name="PreFilterName"></param>
        ''' <param name="PreFilterValue"></param>
        ''' <remarks></remarks>
        Public Sub AddPreFilter(ByVal PreFilterName As String, ByVal PreFilterValue As Object)
            For Each PreFilter As Pair(Of String, Object) In m_PreFilters
                If (PreFilter.First = PreFilterName) Then
                    PreFilter.Second = PreFilterValue
                    Return
                End If

              
            Next
            Dim ToAdd As Pair(Of String, Object) = New Pair(Of String, Object)(PreFilterName, PreFilterValue)
            m_PreFilters.Add(ToAdd)
        End Sub

        ''' <summary>
        ''' Return the preferred factory method for this criteria 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FactoryMethodName() As String
            Get
                Return m_FactoryMethodName
            End Get
            Set(ByVal value As String)
                m_FactoryMethodName = value
            End Set
        End Property
#End Region

#Region "XML Serialization"
        Public Function ToXML() As String
            Dim doc As Xml.Linq.XDocument
            doc = New Xml.Linq.XDocument()
            Dim CritNode As New Xml.Linq.XElement("CRITERIA")
            doc.Add(CritNode)
            If Expression Is GetType(BLCriteriaExpression.ListExpression) Then
                CritNode.Add(ListExpressionToXMLElement(CType(Expression, BLCriteriaExpression.ListExpression)))
            ElseIf Expression Is GetType(BLCriteriaExpression.BinaryOperatorExpression) Then
                CritNode.Add(BinaryExpressionToXMLElement(CType(Expression, BLCriteriaExpression.BinaryOperatorExpression)))
            End If

            For Each ordField As BLCore.DataTypes.Pair(Of String, Boolean) In OrderByFields
                CritNode.Add(OrderedFieldToXMLElement(ordField))
            Next

            For Each prFilter As Pair(Of String, Object) In PreFilters
                CritNode.Add(PreFilterToXMLElement(prFilter))
            Next


            Return doc.ToString()
        End Function

        Private Function ListExpressionToXMLElement(ByVal listExpression As BLCriteriaExpression.ListExpression) As Xml.Linq.XElement
            Dim SerializedListExpression As New Xml.Linq.XElement("EXPRESSION")
            SerializedListExpression.Add(New Xml.Linq.XAttribute("Name", listExpression.Name.ToString()))
            SerializedListExpression.Add(New Xml.Linq.XAttribute("BLCriteriaOperator", listExpression.NextOperator.ToString("g")))
            SerializedListExpression.Add(New Xml.Linq.XAttribute("Negates", listExpression.Negates.ToString()))

            For Each expr As BLCriteriaExpression In listExpression.Expressions
                If expr Is GetType(BLCriteriaExpression.ListExpression) Then
                    SerializedListExpression.Add(ListExpressionToXMLElement(CType(expr, BLCriteriaExpression.ListExpression)))
                ElseIf expr Is GetType(BLCriteriaExpression.BinaryOperatorExpression) Then
                    SerializedListExpression.Add(BinaryExpressionToXMLElement(CType(expr, BLCriteriaExpression.BinaryOperatorExpression)))
                End If
            Next


            Return SerializedListExpression
        End Function

        Private Function BinaryExpressionToXMLElement(ByVal binaryExpression As BLCriteriaExpression.BinaryOperatorExpression) As Xml.Linq.XElement
            Dim SerializedBinaryExpression As New Xml.Linq.XElement("BINARYEXPRESSION")
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("DBFieldName", binaryExpression.DataLayerFieldName))
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("ObjectFieldName", binaryExpression.ObjectFieldName))
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("Operator", UnpParseOperatorValue(binaryExpression.ExpressionOperator)))
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("Value", binaryExpression.Value.ToString()))
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("ValueType", binaryExpression.Value.GetType().AssemblyQualifiedName))
            SerializedBinaryExpression.Add(New Xml.Linq.XAttribute("BLCriteriaOperator", binaryExpression.NextOperator.ToString("g")))

            Return SerializedBinaryExpression
        End Function

        Private Function OrderedFieldToXMLElement(ByVal ordField As BLCore.DataTypes.Pair(Of String, Boolean)) As Xml.Linq.XElement
            Dim SerializedOrderedField As New Xml.Linq.XElement("ORDEREDFIELD")
            SerializedOrderedField.Add(New Xml.Linq.XAttribute("FieldName", ordField.First))
            SerializedOrderedField.Add(New Xml.Linq.XAttribute("Ascending", ordField.Second.ToString()))
            Return SerializedOrderedField
        End Function

        '
        Private Function PreFilterToXMLElement(ByVal PreFilter As Pair(Of String, Object)) As Xml.Linq.XElement
            Dim SerializedOrderedField As New Xml.Linq.XElement("PREFILTER")
            SerializedOrderedField.Add(New Xml.Linq.XAttribute("PrefilerName", PreFilter.First))
            SerializedOrderedField.Add(New Xml.Linq.XAttribute("PrefilterValueParam", PreFilter.Second.ToString()))
            SerializedOrderedField.Add(New Xml.Linq.XAttribute("ValueType", PreFilter.Second.GetType().AssemblyQualifiedName))
            Return SerializedOrderedField
        End Function

        Public Sub LoadFromXML(ByVal Source As Xml.XmlNode, Optional ByVal GlobalParameters As Hashtable = Nothing)
            If Source.Name.ToUpper() = "CRITERIA" Then
                For Each Node As Xml.XmlNode In Source.ChildNodes
                    Select Case Node.Name.ToLower()
                        Case "binaryexpression"
                            If Node.Attributes("DBFieldName") IsNot Nothing And _
                               Node.Attributes("ObjectFieldName") IsNot Nothing And _
                               Node.Attributes("Operator") IsNot Nothing And _
                               Node.Attributes("Value") IsNot Nothing And _
                               Node.Attributes("BLCriteriaOperator") IsNot Nothing _
                            Then
                                Dim TextValue As String = Node.Attributes("Value").Value
                                Dim ValueType As String = ""
                                If Node.Attributes("ValueType") IsNot Nothing Then
                                    ValueType = Node.Attributes("ValueType").Value
                                End If
                                Dim ConcatOp As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
                                ConcatOp = DirectCast(System.Enum.Parse(GetType(BLCriteriaExpression.BLCriteriaOperator), Node.Attributes("BLCriteriaOperator").Value, True), BLCriteriaExpression.BLCriteriaOperator)

                                Me.AddBinaryExpression(Node.Attributes("DBFieldName").Value, _
                                                    Node.Attributes("ObjectFieldName").Value, _
                                                    ParseOperatorValue(Node.Attributes("Operator").Value), _
                                                    ParseValue(TextValue, ValueType, GlobalParameters), ConcatOp)
                            End If

                        Case "expression"
                            Dim ConcatOp As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
                            If Node.Attributes("BLCriteriaOperator") IsNot Nothing Then
                                ConcatOp = DirectCast(System.Enum.Parse(GetType(BLCriteriaExpression.BLCriteriaOperator), Node.Attributes("BLCriteriaOperator").Value, True), BLCriteriaExpression.BLCriteriaOperator)
                            End If
                            'ConcatOp = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
                            Me.AddExpression(ParseExpression(Node, GlobalParameters), ConcatOp)

                        Case "orderedfield"
                            If Node.Attributes("FieldName") IsNot Nothing Then
                                Dim isAscending As Boolean = False
                                If Node.Attributes("Ascending") IsNot Nothing Then
                                    isAscending = System.Boolean.Parse(Node.Attributes("Ascending").Value)
                                End If
                                Me.AddOrderedField(Node.Attributes("FieldName").Value, isAscending)
                            End If

                        Case "prefilter"
                            If Node.Attributes("PrefilerName") IsNot Nothing And _
                               Node.Attributes("PrefilterValueParam") IsNot Nothing _
                            Then
                                Dim ValueType As String = ""
                                If Node.Attributes("ValueType") IsNot Nothing Then
                                    ValueType = Node.Attributes("ValueType").Value
                                End If

                                Me.AddPreFilter("PrefilerName", ParseValue(Node.Attributes("PrefilterValueParam").Value, ValueType, GlobalParameters))
                            End If

                    End Select
                Next
            End If
        End Sub

        Private Function ParseExpression(ByVal ExpressionNode As System.Xml.XmlNode, ByVal GlobalParameters As Hashtable) As BLCriteriaExpression
            If ExpressionNode.Name.ToLower() = "expression" Then
                Dim expr As New Axelerate.BusinessLayerFrameWork.BLCore.BLCriteriaExpression.ListExpression("")

                For Each Node As Xml.XmlNode In ExpressionNode.ChildNodes
                    Select Case Node.Name.ToLower()
                        Case "expression"
                            Dim ConcatOp As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
                            If Node.Attributes("BLCriteriaOperator") IsNot Nothing Then
                                ConcatOp = DirectCast(System.Enum.Parse(GetType(BLCriteriaExpression.BLCriteriaOperator), Node.Attributes("BLCriteriaOperator").Value, True), BLCriteriaExpression.BLCriteriaOperator)
                            End If

                            expr.AddExpression(ParseExpression(Node, GlobalParameters), ConcatOp)
                        Case "binaryexpression"
                            If Node.Attributes("DBFieldName") IsNot Nothing And _
                               Node.Attributes("ObjectFieldName") IsNot Nothing And _
                               Node.Attributes("Operator") IsNot Nothing And _
                               Node.Attributes("Value") IsNot Nothing And _
                               Node.Attributes("BLCriteriaOperator") IsNot Nothing _
                            Then
                                Dim TextValue As String = Node.Attributes("Value").Value
                                Dim ValueType As String = ""
                                If Node.Attributes("ValueType") IsNot Nothing Then
                                    ValueType = Node.Attributes("ValueType").Value
                                End If
                                Dim ConcatOp As BLCriteriaExpression.BLCriteriaOperator = BLCriteriaExpression.BLCriteriaOperator.OperatorNone
                                ConcatOp = DirectCast(System.Enum.Parse(GetType(BLCriteriaExpression.BLCriteriaOperator), Node.Attributes("BLCriteriaOperator").Value, True), BLCriteriaExpression.BLCriteriaOperator)
                                Dim DBFieldName As String = Node.Attributes("DBFieldName").Value
                                Dim ParameterFieldName As String = DBFieldName
                                If DBFieldName.Contains(":") Then
                                    Dim Pos As Integer = DBFieldName.IndexOf(":")
                                    DBFieldName = Mid(DBFieldName, 1, Pos)
                                End If
                                ParameterFieldName = ParameterFieldName.Replace(":", "_")
                                Dim bexpr As New BLCore.BLCriteriaExpression.BinaryOperatorExpression(ParameterFieldName, DBFieldName, Node.Attributes("ObjectFieldName").Value, ParseOperatorValue(Node.Attributes("Operator").Value), ParseValue(TextValue, ValueType, GlobalParameters))

                                expr.AddExpression(bexpr, ConcatOp)

                            End If
                    End Select
                Next
                Return expr
            End If
            Return Nothing
        End Function


        Private Function ParseValue(ByVal TextValue As String, ByVal ValueType As String, ByVal GlobalParameters As Hashtable) As Object
            If TextValue.ToUpper() = "NULL" Or TextValue.ToUpper() = "NOTHING" Then
                Return Nothing
            Else
                If TextValue.StartsWith("[") Then
                    Dim GlobalParameterName As String = TextValue.Substring(1, TextValue.Length - 2)
                    If GlobalParameters IsNot Nothing Then
                        Return GlobalParameters(GlobalParameterName)
                    End If
                Else
                    If ValueType = "" Then
                        Return TextValue
                    Else
                        Dim tpy As System.Type
                        tpy = Type.GetType(ValueType, False, True)
                        If tpy IsNot Nothing Then
                            Return Convert.ChangeType(TextValue, tpy)
                        End If
                    End If
                End If
            End If
            Return Nothing
        End Function


        Public Function ParseOperatorValue(ByVal operatorValue As String) As String
            Select Case operatorValue
                Case "OperatorEqual"
                    Return "="
                Case "OperatorLess"
                    Return "<"
                Case "OperatorGreater"
                    Return ">"
                Case "OperatorGreaterEqual"
                    Return ">="
                Case "OperatorLessEqual"
                    Return "<="
                Case "OperatorNotEqual"
                    Return "<>"
                Case "OperatorLike"
                    Return "Like"
                Case Else
                    Return Nothing
            End Select
            Return Nothing
        End Function

        Public Function UnpParseOperatorValue(ByVal operatorValue As String) As String
            Select Case operatorValue
                Case "="
                    Return "OperatorEqual"
                Case "<"
                    Return "OperatorLess"
                Case ">"
                    Return "OperatorGreater"
                Case ">="
                    Return "OperatorGreaterEqual"
                Case "<="
                    Return "OperatorLessEqual"
                Case "<>"
                    Return "OperatorNotEqual"
                Case "!="
                    Return "OperatorNotEqual"
                Case "Like"
                    Return "OperatorLike"
                Case Else
                    Return Nothing
            End Select
            Return Nothing
        End Function
#End Region

#Region "System.Object Overrides"

        Public Overrides Function ToString() As String
            Return GetFilterText()
        End Function

        Public Overloads Function Equals(ByVal obj As BLCriteria) As Boolean
            Return Me.ToString = obj.ToString
        End Function

#End Region

        ''' <summary>
        ''' This method can be override to provide a custom generation of the criteria using internal variables.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub Update()

        End Sub

#Region "Cached Objects Properties and Methods"
        ''' <summary>
        ''' Marks the associated object to the property PropertyName as ready to be read
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <remarks></remarks>
        Public Sub LoadCachedObject(ByVal PropertyName As String)
            If Not m_CachedFieldListToLoad.ContainsKey(PropertyName) Then
                m_CachedFieldListToLoad.Add(PropertyName, PropertyName)
            End If
        End Sub

        ''' <summary>
        ''' Is the associated property PropertyName ready to be read
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property isCachedObjectLoaded(ByVal PropertyName As String) As Boolean
            Get
                Return m_CachedFieldListToLoad.ContainsKey(PropertyName)
            End Get
        End Property

        ''' <summary>
        ''' Total number ob objects "cached" to be read
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NumCachedObjects() As Integer
            Get
                Return m_CachedFieldListToLoad.Count
            End Get
        End Property
#End Region

    End Class


End Namespace