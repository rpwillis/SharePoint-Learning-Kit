Imports System.Reflection

Namespace BLCore.Attributes
    ''' <summary>
    ''' Attribute that defines a value as null for a numeric field
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class NumericNullableAttribute
        Inherits Attribute
        ''' <summary>
        ''' Defines the default numeric value that determines the field as null
        ''' </summary>
        ''' <remarks></remarks>
        Sub New()
        End Sub
        ''' <summary>
        ''' Determines if the value is null
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function IsNull(ByVal value As Object) As Boolean
    End Class
End Namespace
