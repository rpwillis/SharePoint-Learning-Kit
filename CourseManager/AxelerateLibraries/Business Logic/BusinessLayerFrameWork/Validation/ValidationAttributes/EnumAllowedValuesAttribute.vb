Namespace BLCore.Validation.ValidationAttributes
    <AttributeUsage(AttributeTargets.Property)> _
    Public Class EnumAllowedValuesAttribute
        Inherits AllowedValuesAttribute

        Private mResourcesFile As String = ""

        Public Property ResourcesFile() As String
            Get
                Return mResourcesFile
            End Get
            Set(ByVal value As String)
                mResourcesFile = value
            End Set
        End Property


#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Return True
        End Function
#End Region
#Region "SinglePropertyValidationAttribute Overrides"
        Protected Overrides ReadOnly Property ValidationFailedMessage() As String
            Get
                'Return My.Resources.BLResources.Validation_FieldLength.Replace("[PropertyName]", PropertyName)
                Return My.Resources.BLResources.valueNotAllowed
            End Get
        End Property
#End Region

    End Class
End Namespace