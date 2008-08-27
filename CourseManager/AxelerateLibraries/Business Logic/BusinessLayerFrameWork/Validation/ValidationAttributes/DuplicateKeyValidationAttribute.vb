Namespace BLCore.Validation.ValidationAttributes

    'Verifica que al cambiar la propiedad la llave del objeto no esté duplicada
    <AttributeUsage(AttributeTargets.Property)> _
    Public Class DuplicateKeyValidationAttribute
        Inherits SinglePropertyValidationAttribute

        Public Sub New()
            MyBase.New()
        End Sub

#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Validator As Validation.VldDuplicate = Validation.VldDuplicate.GetVldDuplicate(BLTarget)
            If (Validator.isValid) Then
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
                Return My.Resources.BLResources.Validation_DuplicateKey.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region

    End Class
End Namespace
