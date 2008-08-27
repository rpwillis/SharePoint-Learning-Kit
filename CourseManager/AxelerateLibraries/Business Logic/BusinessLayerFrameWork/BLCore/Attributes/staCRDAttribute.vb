Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a class has with the Test Framework.
    ''' The class execute the create,read and delete test.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.All)> _
    Public Class staCRDAttribute
        Inherits staBaseAttribute



        Public Sub New()

        End Sub




#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(staCRDAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Member As MemberInfo) As Boolean
            Return Attribute.IsDefined(Member, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Member As MemberInfo) As staCRDAttribute
            Return CType(Attribute.GetCustomAttribute(Member, AttributeType), staCRDAttribute)
        End Function


#End Region


    End Class


End Namespace