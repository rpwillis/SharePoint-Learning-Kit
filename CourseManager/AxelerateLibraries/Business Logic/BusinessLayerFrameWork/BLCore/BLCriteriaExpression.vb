Imports Axelerate.BusinessLayerFrameWork.BLCore.Security

Namespace BLCore
    ''' <summary>
    ''' Class that manages representing a conditional expression for the BLCriteria class
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class BLCriteriaExpression
        Implements ICloneable

#Region "BLCriteriaOperator"
        ''' <summary>
        ''' Operator type for the binary operations
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum BLCriteriaOperator
            OperatorOr
            OperatorAnd
            OperatorEquals
            OperatorGreaterThan
            OperatorGreaterThanOrEquals
            OperatorLessThan
            OperatorLessThanOrEquals
            OperatorLike
            OperatorNone
        End Enum
#End Region

#Region "BLCriteriaExpression Private Data"

        ''' <summary>
        ''' Name of the expression for external refrences
        ''' </summary>
        ''' <remarks></remarks>
        Private m_Name As String


        ''' <summary>
        ''' Final value of the expression is denied
        ''' </summary>
        ''' <remarks></remarks>
        Private m_Negates As Boolean

        ''' <summary> 
        ''' Indicates the logical binary operator that sholud be applied to the next argument on the list
        ''' </summary>
        ''' <remarks></remarks>
        Private m_NextOperator As BLCriteriaOperator = BLCriteriaOperator.OperatorNone


#End Region

#Region "BLCriteriaExpression Constructors"
        Public Sub New(ByVal NName As String)
            Name = NName
        End Sub

        Public Sub New(ByVal NCriteriaExpression As BLCriteriaExpression)
            m_Name = NCriteriaExpression.m_Name
            m_Negates = NCriteriaExpression.m_Negates
            m_NextOperator = NCriteriaExpression.m_NextOperator
        End Sub

#End Region

#Region "BLCriteriaExpression Publics Properties and Methods"

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property
        ''' <summary>
        ''' Final value of the expression is denied
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Negates() As Boolean
            Get
                Return m_Negates
            End Get
            Set(ByVal value As Boolean)
                m_Negates = value
            End Set
        End Property

        Public Property NextOperator() As BLCriteriaOperator
            Get
                Return m_NextOperator
            End Get
            Set(ByVal value As BLCriteriaOperator)
                m_NextOperator = value
            End Set
        End Property

        Public Function BooleanBinaryOperatorString() As String
            Select Case m_NextOperator
                Case BLCriteriaOperator.OperatorAnd
                    Return "AND"
                Case BLCriteriaOperator.OperatorNone
                    Return ""
                Case BLCriteriaOperator.OperatorOr
                    Return "OR"
            End Select
            Return ""
        End Function

#End Region

#Region "BLCriteriaExpression Overridables"
        Public Overridable Function clone() As Object Implements ICloneable.Clone
            Return New BLCriteriaExpression(Me)
        End Function

        ''' <summary>
        ''' Returns a text filter that works for the inclussion of the WHERE clause in the SQL sentence.
        ''' Also returns an array of parameter in ParameterList, in case the text filter needs it
        ''' </summary>
        ''' <param name="ParameterList"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GetFilter(ByRef ParameterList As List(Of DataLayerParameter)) As String
            Return ""
        End Function

        ''' <summary>
        ''' Returns a text filter that represents the expression
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GetFilterText() As String
            Return ""
        End Function

        ''' <summary>
        ''' Indicates the number of parameters that are not pure strings
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property NonPureStringParameterCount() As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overridable Function FindExpression(ByVal ExpressionName As String) As BLCriteriaExpression
            If Name = ExpressionName Then
                Return Me
            End If
            Return Nothing
        End Function


#End Region

#Region "BinaryOperatorExpression"
        <Serializable()> _
        Public Class BinaryOperatorExpression
            Inherits BLCriteriaExpression

#Region "BinaryOperatorExpression Private Data"

            ''' <summary>
            ''' Name in the database of a mappable field
            ''' </summary>
            ''' <remarks></remarks>
            Private m_DataLayerFieldName As String

            ''' <summary>
            ''' Name in the object of a mappable field
            ''' </summary>
            ''' <remarks></remarks>
            Private m_ObjectFieldName As String

            ''' <summary>
            ''' Operator that the comparison will be made with
            ''' </summary>
            ''' <remarks></remarks>
            Private m_Operator As String

            ''' <summary>
            ''' Value that the comparison will be compared against
            ''' </summary>
            ''' <remarks></remarks>
            Private m_Value As Object

            ''' <summary>
            ''' Indicates if the value of Object could be passed to the DataLayer as a string
            ''' </summary>
            ''' <remarks></remarks>
            Private m_PureStringValue As Boolean

#End Region

#Region "BinaryOperatorExpression Constructors"
            'Builds a new object initialized with all his fields with the new
            'values DataLayerFieldName, ObjectFieldName, Operator and Value
            Public Sub New(ByVal ExpressionName As String, ByVal DataLayerFieldName As String, _
                ByVal ObjectFieldName As String, ByVal OperatorStr As String, ByVal Value As Object)
                MyBase.New(ExpressionName)
                m_DataLayerFieldName = DataLayerFieldName
                m_Operator = OperatorStr
                m_Value = Value
                m_ObjectFieldName = ObjectFieldName
                If m_Operator.ToUpper = "LIKE" Then
                    m_PureStringValue = True
                Else
                    m_PureStringValue = False
                End If
            End Sub

            'Builds new object copiyng the onformation of NBLCriteriaParameter 
            Public Sub New(ByVal NBinaryOperatorExpression As BinaryOperatorExpression)
                MyBase.New(NBinaryOperatorExpression)
                m_DataLayerFieldName = NBinaryOperatorExpression.m_DataLayerFieldName
                m_Operator = NBinaryOperatorExpression.m_Operator
                m_ObjectFieldName = NBinaryOperatorExpression.m_ObjectFieldName
                m_Value = NBinaryOperatorExpression.m_Value
                m_PureStringValue = NBinaryOperatorExpression.m_PureStringValue
            End Sub

#End Region

#Region "BinaryOperatorExpression Publics Properties and Methods"

            ''' <summary>
            ''' In case that this parameter involves SQL parameter, retuns the
            ''' regarding SQLParamater and the name of the paramater could be specified.
            ''' If not, uses the name of the field in the database
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property GetParameter() As DataLayerParameter
                Get
                    If Not m_PureStringValue Then
                        Return New DataLayerParameter("@Filter_" + m_Name, m_Value)
                    Else
                        Return Nothing
                    End If

                End Get
            End Property

            'In case that this parameter involves a SQL Parameter returns true otherwise is false
            Public ReadOnly Property isPureStringValue() As Boolean
                Get
                    Return m_PureStringValue
                End Get
            End Property

            'Returns the object name of a mappable field in the DataLayer
            Public ReadOnly Property DataLayerFieldName() As String
                Get
                    Return m_DataLayerFieldName
                End Get
            End Property

            'Returns the object name of a mappable field
            Public ReadOnly Property ObjectFieldName() As String
                Get
                    Return m_ObjectFieldName
                End Get
            End Property


            'Returns the operator that the comparison will be made with
            Public Property Value() As Object
                Get
                    Return m_Value

                End Get
                Set(ByVal Value As Object)
                    m_Value = Value
                End Set
            End Property

            'Returns a text that represents the object
            Public ReadOnly Property FilterText() As String
                Get
                    If m_Value.GetType Is GetType(String) Then
                        Return m_DataLayerFieldName + " " + m_Operator + " '" + m_Value.ToString + "'"
                    Else
                        Return m_DataLayerFieldName + " " + m_Operator + " " + m_Value.ToString
                    End If
                End Get
            End Property

            'Returns the operator of the comparison
            Public ReadOnly Property ExpressionOperator() As String
                Get
                    Return m_Operator
                End Get
            End Property

#End Region

#Region "BLCriteriaExpression Overrides"
            Public Overrides Function clone() As Object
                Return New BinaryOperatorExpression(Me)
            End Function


            ''' <summary>
            ''' Returns a text filter that works for the inclussion of the WHERE clause in the SQL sentence.
            ''' Also returns an array of parameter in ParameterList, in case the text filter needs it
            ''' </summary>
            ''' <param name="ParameterList"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetFilter(ByRef ParameterList As List(Of DataLayerParameter)) As String
                Dim Filter As String
                If Not isPureStringValue Then
                    Filter = "(" + m_DataLayerFieldName + " " + m_Operator + " @Filter_" + m_Name + ")"
                    ParameterList.Add(New DataLayerParameter("@Filter_" + m_Name, m_Value))
                Else
                    Filter = "(" + m_DataLayerFieldName + " " + m_Operator + " '" + clsSecurityAssuranceHelper.AvoidSQLInjectionInLiteral(m_Value.ToString) + "'" + ")"
                End If
                If m_Negates Then
                    Filter = "NOT " + Filter
                End If
                Filter = Filter + " " + BooleanBinaryOperatorString()
                Return Filter
            End Function

            ''' <summary>
            ''' Returns a text filter that represents the expression
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetFilterText() As String
                Dim Filter As String
                Filter = "(" + m_DataLayerFieldName + " " + m_Operator + " '" + clsSecurityAssuranceHelper.AvoidSQLInjectionInLiteral(m_Value.ToString) + "'" + ")"
                If m_Negates Then
                    Filter = "NOT " + Filter
                End If
                Filter = Filter + " " + BooleanBinaryOperatorString()
                Return Filter
            End Function

            ''' <summary>
            ''' Indicates the number of parameters that are not pure strings
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides ReadOnly Property NonPureStringParameterCount() As Integer
                Get
                    If Not isPureStringValue Then
                        Return 1
                    End If
                    Return 0
                End Get
            End Property
#End Region

        End Class
#End Region

#Region "ListExpression"
        <Serializable()> _
        Public Class ListExpression
            Inherits BLCriteriaExpression


#Region "ListExpression Private Data"
            ''' <summary>
            ''' List of expressions that conform this criteria
            ''' </summary>
            ''' <remarks></remarks>
            Private m_Expressions As New List(Of BLCriteriaExpression)
#End Region

#Region "ListExpression Constructors"
            Public Sub New(ByVal NName As String)
                MyBase.New(NName)
            End Sub

            'Builds a new object copying the information of NBLCriteriaParameter
            Public Sub New(ByVal NListExpression As ListExpression)
                MyBase.New(NListExpression)
                For Each Expression As BLCriteriaExpression In NListExpression.m_Expressions
                    m_Expressions.Add(CType(Expression.clone, BLCriteriaExpression))
                Next
            End Sub
#End Region

#Region "ListExpression Publics Properties and Methods"
            Public Sub AddExpression(ByVal Expression As BLCriteriaExpression, ByVal ConcatenationOperator As BLCriteriaOperator)
                If m_Expressions.Count > 0 Then
                    Dim LastExpression As BLCriteriaExpression = m_Expressions(m_Expressions.Count - 1)
                    LastExpression.NextOperator = ConcatenationOperator
                End If
                Expression.NextOperator = BLCriteriaOperator.OperatorNone
                m_Expressions.Add(Expression)
            End Sub

            Public ReadOnly Property Expressions() As List(Of BLCriteriaExpression)
                Get
                    Return m_Expressions
                End Get
            End Property
#End Region

#Region "BLCriteriaExpression Overrides"
            Public Overrides Function clone() As Object
                Return New ListExpression(Me)
            End Function

            ''' <summary>
            ''' Returns a text filter that works for the inclussion of the WHERE clause in the SQL sentence.
            ''' Also returns an array of parameter in ParameterList, in case the text filter needs it
            ''' </summary>
            ''' <param name="ParameterList"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetFilter(ByRef ParameterList As List(Of DataLayerParameter)) As String
                If m_Expressions.Count > 0 Then
                    Dim Filter As String = "( "
                    For Each expression As BLCriteriaExpression In m_Expressions
                        Filter = Filter + expression.GetFilter(ParameterList)
                    Next
                    Filter = Filter + ")"
                    If m_Negates Then
                        Filter = "NOT " + Filter
                    End If
                    Filter = Filter + " " + BooleanBinaryOperatorString()
                    Return Filter
                End If
                Return ""
            End Function

            ''' <summary>
            ''' Returns a text filter that represents the expression
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function GetFilterText() As String
                If m_Expressions.Count > 0 Then
                    Dim Filter As String = "( "
                    For Each expression As BLCriteriaExpression In m_Expressions
                        Filter = Filter + expression.GetFilterText()
                    Next
                    Filter = Filter + ")"
                    If m_Negates Then
                        Filter = "NOT " + Filter
                    End If
                    Filter = Filter + " " + BooleanBinaryOperatorString()
                    Return Filter
                End If
                Return ""
            End Function

            ''' <summary>
            ''' Indicates the number of parameters that are not pure strings
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides ReadOnly Property NonPureStringParameterCount() As Integer
                Get
                    Dim ToReturn As Integer = 0
                    For Each expression As BLCriteriaExpression In m_Expressions
                        ToReturn = ToReturn + expression.NonPureStringParameterCount
                    Next
                    Return ToReturn
                End Get
            End Property


            ''' <summary>
            ''' Looks in the expression and subexpressions with this name
            ''' </summary>
            ''' <param name="ExpressionName"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function FindExpression(ByVal ExpressionName As String) As BLCriteriaExpression
                If Name = ExpressionName Then
                    Return Me
                End If

                For Each Expression As BLCriteriaExpression In m_Expressions
                    Dim Result As BLCriteriaExpression = Expression.FindExpression(ExpressionName)
                    If Not Result Is Nothing Then
                        Return Result
                    End If
                Next
                Return Nothing
            End Function
#End Region


        End Class
#End Region

    End Class

End Namespace
