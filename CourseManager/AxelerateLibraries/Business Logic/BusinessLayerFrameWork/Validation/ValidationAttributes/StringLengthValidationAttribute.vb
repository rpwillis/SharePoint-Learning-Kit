Namespace BLCore.Validation.ValidationAttributes

    <AttributeUsage(AttributeTargets.Property)> _
    Public Class StringLengthValidationAttribute
        Inherits SinglePropertyValidationAttribute

        Private m_MaxLength As Integer = 0
        Public ReadOnly Property MaxLength() As Integer
            Get
                Return m_MaxLength
            End Get
        End Property
        Public Sub New(ByVal MaxLength As Integer)
            MyBase.New()
            m_MaxLength = MaxLength
        End Sub

#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)
            If (Not Value Is Nothing) Then
                If (CStr(Value).Length > m_MaxLength) Then
                    e = ValidationFailedMessage
                    Return False
                End If
            End If
            Return True
        End Function
#End Region
#Region "SinglePropertyValidationAttribute Overrides"
        Protected Overrides ReadOnly Property ValidationFailedMessage() As String
            Get
                Return My.Resources.BLResources.Validation_FieldLength.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region

    End Class
End Namespace
