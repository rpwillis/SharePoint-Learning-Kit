Namespace BLCore.Validation.ValidationAttributes

    <AttributeUsage(AttributeTargets.Property)> _
    Public Class BCAllowedValuesAttribute
        Inherits AllowedValuesAttribute

        Private m_CollectionType As Type = Nothing
        Private m_TargetPropertyName As String = ""
        Private m_FactoryMethodName As String = ""
        Private m_FactoryMethodParams As String = ""
        Private m_ExistsCriteriaParemeter As Boolean = False
        Private m_TargetFieldMap As BLFieldMap = Nothing
        Private m_ExistsTargetFieldMap As Integer = -1


        Private m_FactoryMethodParamArray() As Object = Nothing


        Public Sub New(ByVal pCollectionType As Type, ByVal pTargetPropertyName As String)
            m_CollectionType = pCollectionType
            m_FactoryMethodName = "GetCollection"
            m_FactoryMethodParams = ""
            m_TargetPropertyName = pTargetPropertyName
            InitializeFactoryMethod()

        End Sub

        Public Sub New(ByVal pCollectionType As Type, ByVal pFactoryMethodName As String, ByVal pFactoryMethodParams As String, ByVal pTargetPropertyName As String)
            m_CollectionType = pCollectionType
            m_FactoryMethodName = pFactoryMethodName
            m_FactoryMethodParams = pFactoryMethodParams
            m_TargetPropertyName = pTargetPropertyName
            InitializeFactoryMethod()
        End Sub


        Private Sub InitializeFactoryMethod()
            m_FactoryMethodParamArray = m_FactoryMethodParams.Split(",".ToCharArray, StringSplitOptions.RemoveEmptyEntries)

            For Each FactoryMethodParam As String In m_FactoryMethodParamArray
                If FactoryMethodParam.ToLower = "[criteria]" Then
                    m_ExistsCriteriaParemeter = True
                    Exit For
                End If
            Next

            If (m_FactoryMethodParamArray.Length = 0) Then
                m_FactoryMethodParamArray = Nothing
            End If
        End Sub

        Public ReadOnly Property AllowedObjects(ByVal pParameterArray() As Object) As IBLListBase
            Get
                Dim AllowedValues As IBLListBase = Nothing

                If (Not pParameterArray Is Nothing) AndAlso (pParameterArray.Length > 0) Then
                    AllowedValues = CType(ReflectionHelper.GetSharedBusinessClassProperty(m_CollectionType.AssemblyQualifiedName, m_FactoryMethodName, pParameterArray), IBLListBase)
                Else
                    AllowedValues = CType(ReflectionHelper.GetSharedBusinessClassProperty(m_CollectionType.AssemblyQualifiedName, m_FactoryMethodName), IBLListBase)
                End If
                Return AllowedValues
            End Get
        End Property

        Public ReadOnly Property TargetPropertyName() As String
            Get
                Return m_TargetPropertyName
            End Get
        End Property

        Public ReadOnly Property TargetFieldMap() As BLFieldMap
            Get
                If m_ExistsTargetFieldMap = -1 Then
                    Dim Collection As IBLListBase = CType(System.Activator.CreateInstance(m_CollectionType), IBLListBase)
                    Dim BO As BLBusinessBase = Collection.NewBusinessObject
                    m_TargetFieldMap = BO.PropertyFieldMap(m_TargetPropertyName)
                    If m_TargetFieldMap Is Nothing Then
                        m_ExistsTargetFieldMap = 0
                    Else
                        m_ExistsTargetFieldMap = 1
                    End If
                End If

                Return m_TargetFieldMap

            End Get
        End Property

        Public Function GetFactoryMethodParameterArray(ByVal target As Object, ByVal Criteria As BLCriteria) As Object()
            If (Not m_ExistsCriteriaParemeter) Then
                Return m_FactoryMethodParamArray
            Else

                Dim ToReturnParameters() As Object = CType(m_FactoryMethodParamArray.Clone(), Object())
                Dim i As Integer = 0
                For i = 0 To ToReturnParameters.Length - 1
                    If ToReturnParameters(i).ToString() = "[criteria]" Then
                        ToReturnParameters(i) = Criteria

                    Else
                        If ToReturnParameters(i).ToString().StartsWith("{") Then
                            ToReturnParameters(i) = System.Web.UI.DataBinder.Eval(target, ToReturnParameters(i).ToString().Substring(1, ToReturnParameters(i).ToString().Length - 2))
                        End If
                    End If
                Next
                Return ToReturnParameters
            End If
        End Function


#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)

            Dim Criteria As BLCriteria = New BLCriteria
            Dim ParameterArray() As Object = Nothing

            If (m_ExistsCriteriaParemeter) And (Not TargetFieldMap Is Nothing) Then
                Criteria.AddBinaryExpression(TargetFieldMap.DLFieldName, TargetFieldMap.NeutralFieldName, "=", Value, BLCriteriaExpression.BLCriteriaOperator.OperatorNone)
                ParameterArray = GetFactoryMethodParameterArray(target, Criteria)
            Else
                ParameterArray = GetFactoryMethodParameterArray(target, Criteria)
            End If

            Dim AllowedValues As IBLListBase = AllowedObjects(ParameterArray)

            For Each BO As BLBusinessBase In AllowedValues
                If (BO.PropertyValue(m_TargetPropertyName).ToString() = Value.ToString()) Then
                    Return True
                End If
            Next

            e = ValidationFailedMessage
            Return False
        End Function
#End Region
#Region "SinglePropertyValidationAttribute Overrides"
        Protected Overrides ReadOnly Property ValidationFailedMessage() As String
            Get
                Return My.Resources.BLResources.Validation_AllowedValues.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region

    End Class


End Namespace