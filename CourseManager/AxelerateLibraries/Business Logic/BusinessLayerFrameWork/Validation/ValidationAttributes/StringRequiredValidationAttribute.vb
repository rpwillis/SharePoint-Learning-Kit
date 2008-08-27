Namespace BLCore.Validation.ValidationAttributes

    <AttributeUsage(AttributeTargets.Property)> _
    Public Class StringRequiredValidationAttribute
        Inherits SinglePropertyValidationAttribute

        Public Sub New()
            MyBase.New()
        End Sub

#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)
            If (Not Value Is Nothing) AndAlso (CStr(Value).Trim().Length > 0) Then
                Return True
            Else
                e = ValidationFailedMessage
                Return False
            End If
        End Function
#End Region

#Region "SinglePropertyValidationAttribute Overrides"
        Protected Overrides ReadOnly Property ValidationFailedMessage() As String
            Get
                Return My.Resources.BLResources.Validation_FieldRequired.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region



    End Class
End Namespace
