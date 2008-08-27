Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a class has with the Test Framework.
    ''' The class execute the read test only.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.All)> _
    Public Class staReadOnlyAttribute
        Inherits staBaseAttribute



        Public Sub New()

        End Sub




#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(staReadOnlyAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Member As MemberInfo) As Boolean
            Return Attribute.IsDefined(Member, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Member As MemberInfo) As staReadOnlyAttribute
            Return CType(Attribute.GetCustomAttribute(Member, AttributeType), staReadOnlyAttribute)
        End Function


#End Region


    End Class


End Namespace