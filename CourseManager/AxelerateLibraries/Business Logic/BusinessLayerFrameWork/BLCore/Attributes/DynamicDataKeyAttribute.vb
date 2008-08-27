Imports System.Reflection

Namespace BLCore.Attributes
    'Atributo que define si un campo es parte de la llave de datos
    <AttributeUsage(AttributeTargets.All)> _
    Public Class DynamicDataKeyAttribute
        Inherits Attribute

        Public Sub New()
        End Sub


#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(DynamicDataKeyAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Field As FieldInfo) As Boolean
            Return Attribute.IsDefined(Field, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Field As FieldInfo) As DynamicDataKeyAttribute
            Return CType(Attribute.GetCustomAttribute(Field, AttributeType), DynamicDataKeyAttribute)
        End Function


#End Region



    End Class
End Namespace
