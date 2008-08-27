Imports System.Reflection

Namespace BLCore.Attributes
    'Atributo que define si un campo es parte de la llave de datos
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class DataKeyAttribute
        Inherits Attribute

        'Indica el nombre de la propiedad relacionada al campo en el objeto
        Private m_PropertyName As String

        Public Sub New(ByVal NPropertyName As String)
            m_PropertyName = NPropertyName
        End Sub

        Public ReadOnly Property PropertyName() As String
            Get
                Return m_PropertyName
            End Get
        End Property

#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(DataKeyAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Field As FieldInfo) As Boolean
            Return Attribute.IsDefined(Field, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Field As FieldInfo) As DataKeyAttribute
            Return CType(Attribute.GetCustomAttribute(Field, AttributeType), DataKeyAttribute)
        End Function


#End Region



    End Class
End Namespace
