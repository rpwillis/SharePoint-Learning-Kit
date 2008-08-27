Imports System.Reflection
Namespace BLCore.Attributes


    'Este atributo describe na propiedad que retorna un objeto foráneo (es decir que se encuentra en otra tabla 
    'y que está relacionado mediante algún identificador)
    <AttributeUsage(AttributeTargets.Property)> _
    Public Class ForeignObjectPropertyAttribute
        Inherits Attribute

        'Indica si es parte de la llave
        Private m_SourcePropertyName As String
        Private m_ForeignPropertyName As String

        'SourceField Name
        Public Sub New(ByVal NSourcePropertyName As String, ByVal NForeignPropertyName As String)
            m_SourcePropertyName = NSourcePropertyName
            m_ForeignPropertyName = NForeignPropertyName
        End Sub


        Public ReadOnly Property SourcePropertyName() As String
            Get
                Return m_SourcePropertyName
            End Get
        End Property

        Public ReadOnly Property ForeignPropertyName() As String
            Get
                Return m_ForeignPropertyName
            End Get
        End Property


#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(ForeignObjectPropertyAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal PropertyInf As PropertyInfo) As Boolean
            Return Attribute.IsDefined(PropertyInf, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal PropertyInf As PropertyInfo) As ForeignObjectPropertyAttribute
            Return CType(Attribute.GetCustomAttribute(PropertyInf, AttributeType), ForeignObjectPropertyAttribute)
        End Function


#End Region


    End Class

End Namespace
