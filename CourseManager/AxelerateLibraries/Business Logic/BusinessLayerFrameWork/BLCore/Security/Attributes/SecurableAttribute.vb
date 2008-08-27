Imports System.Reflection

Namespace BLCore.Security


    ''' <summary>    
    ''' Defines the relation that a class, method or property has with the Security Framework.
    ''' Defines the security properties of a class, method or property.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.All)> _
    Public Class SecurableAttribute
        Inherits Attribute
        Public Sub New()

        End Sub


#Region "Attribute Checking"


        ''' <summary>
        ''' Type of Automap attribute
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared AttributeType As Type = GetType(SecurableAttribute)


        ''' <summary>
        ''' Indicates if the member has defined the AutoMapField Attribute
        ''' </summary>
        ''' <param name="Member"></param>
        ''' The member to be checked
        ''' <returns></returns>
        ''' Returns true if the member has the attribute defined
        ''' <remarks></remarks>
        Public Overloads Shared Function isDefined(ByVal Member As MemberInfo) As Boolean
            Return Attribute.IsDefined(Member, AttributeType)
        End Function


        ''' <summary>
        ''' Searchs in the selected member, the securable attribute
        ''' </summary>
        ''' <param name="Member"></param>
        ''' The member to be checked
        ''' <returns></returns>
        ''' Returns the securable attribute of the member
        ''' <remarks></remarks>
        Public Overloads Shared Function GetAttribute(ByVal Member As MemberInfo) As SecurableAttribute
            Return CType(Attribute.GetCustomAttribute(Member, AttributeType), SecurableAttribute)
        End Function


#End Region


    End Class
End Namespace