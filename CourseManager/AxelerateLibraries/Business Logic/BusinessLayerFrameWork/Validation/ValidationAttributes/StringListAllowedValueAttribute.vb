Namespace BLCore.Validation.ValidationAttributes

    <AttributeUsage(AttributeTargets.Property)> _
    Public Class StringListAllowedValueAttribute
        Inherits AllowedValuesAttribute

        Private m_ResourceFileName As String = ""

        Private m_Values() As String = Nothing

        Public Sub New(ByVal pValues As String, ByVal pSeparators As String, ByVal pResourceFileName As String)
            m_Values = pValues.Split(pSeparators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            m_ResourceFileName = pResourceFileName
        End Sub

        Public Sub New(ByVal pValues As String, ByVal pSeparators As String)
            m_Values = pValues.Split(pSeparators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
        End Sub


        Public ReadOnly Property Values() As String()
            Get
                Return m_Values
            End Get
        End Property

        Public ReadOnly Property ResourceFileName() As String
            Get
                Return m_ResourceFileName
            End Get
        End Property


#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)

            For Each AllowedValue As String In m_Values
                If (AllowedValue = Value.ToString()) Then
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
