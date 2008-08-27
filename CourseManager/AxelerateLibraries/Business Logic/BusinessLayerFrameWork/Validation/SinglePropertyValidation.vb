Namespace BLCore.Validation
    'Se encarga de proveer una base para todos aquellos atributos de validación que se hacen cargo
    'de una única validación
    Public Class SinglePropertyValidationAttribute
        Inherits PropertyValidationRule

        Public Sub New()
            MyBase.New("", "")
        End Sub

        Public Property ReflectedProperty(ByVal Target As BLBusinessBase) As Object
            Get
                Return Target.PropertyValue(PropertyName)
            End Get
            Set(ByVal value As Object)
                Target.PropertyValue(PropertyName) = value
            End Set
        End Property

        Protected Overridable ReadOnly Property ValidationFailedMessage() As String
            Get
                Return My.Resources.BLResources.Validation_InvalidFieldValue.Replace("[PropertyName]", PropertyName)
            End Get
        End Property

        Public Overrides Function isValid(ByVal RuleTarget As BLCore.BLBusinessBase, ByRef ErrorMessage As String) As Boolean
            Return Validate(RuleTarget, ErrorMessage)
        End Function

        'RuleHandler encargado de marcar el objeto como inválido de no cumplirse la validacion
        Public Overridable Function Validate(ByVal target As Object, ByRef ErrorMessage As String) As Boolean
        End Function
    End Class
End Namespace
