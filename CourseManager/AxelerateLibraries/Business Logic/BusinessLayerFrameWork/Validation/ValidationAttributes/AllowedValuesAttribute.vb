Namespace BLCore.Validation.ValidationAttributes

    ''' <summary>
    ''' Validation attribute that checks if a property has a value inside a valid list of values.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property)> _
    Public MustInherit Class AllowedValuesAttribute
        Inherits SinglePropertyValidationAttribute

    End Class
End Namespace
