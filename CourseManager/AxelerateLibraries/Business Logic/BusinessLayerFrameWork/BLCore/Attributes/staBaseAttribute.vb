Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a field,property or method has with the Test Framework.
    ''' Defines how the field,property or method is tested.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.All)> _
    Public Class staBaseAttribute
        Inherits Attribute

        Public Sub New()

        End Sub




#Region "Attribute Checking"

        'Type of the attribute
        Private Shared AttributeType As Type = GetType(staBaseAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Member As MemberInfo) As Boolean
            Return Attribute.IsDefined(Member, AttributeType)
        End Function

        'Obtains the AutoFieldMapAttribute
        Public Overloads Shared Function GetAttribute(ByVal Member As MemberInfo) As staBaseAttribute
            Return CType(Attribute.GetCustomAttribute(Member, AttributeType), staBaseAttribute)
        End Function


#End Region




    End Class


End Namespace
