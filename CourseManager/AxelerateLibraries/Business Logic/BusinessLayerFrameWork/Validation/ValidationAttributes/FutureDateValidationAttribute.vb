Namespace BLCore.Validation.ValidationAttributes

    <AttributeUsage(AttributeTargets.Property)> _
    Public Class FutureDateValidationAttribute
        Inherits SinglePropertyValidationAttribute

        Public Sub New()
            MyBase.New()
        End Sub

#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)
            If (Not Value Is Nothing) Then
                Dim DateValue As Date = CDate(Value)
                If (DateValue < System.DateTime.Today) Then
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
                Return My.Resources.BLResources.Validation_FutureDate.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region

    End Class
End Namespace
