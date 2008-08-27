Imports System.Reflection

Namespace BLCore.Security

    ''' <summary>    
    ''' Defines the relation that a method has with the Security Framework.
    ''' Defines the security properties of a method.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Method)> _
    Public Class SecurableMethodAttribute
        Inherits SecurableAttribute

        ''' <summary>
        ''' Initializes the securable attribute
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

        End Sub

#Region "Attribute Checking"

        ''' <summary>
        ''' Type of Automap attribute
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared AttributeType As Type = GetType(SecurableMethodAttribute)

        ''' <summary>
        ''' Indicates if the type has defined the AutoMapField Attribute
        ''' </summary>
        ''' <param name="Method"></param>
        ''' The method to be checked
        ''' <returns></returns>
        ''' Returns true if the method has the attribute defined
        ''' <remarks></remarks>
        Public Overloads Shared Function isDefined(ByVal Method As MethodInfo) As Boolean
            Return Attribute.IsDefined(Method, AttributeType)
        End Function

        ''' <summary>
        ''' Searchs in the selected method, the securable attribute
        ''' </summary>
        ''' <param name="Method"></param>
        ''' The Method to be checked
        ''' <returns></returns>
        ''' Returns the securable attribute of the method
        ''' <remarks></remarks>
        Public Overloads Shared Function GetAttribute(ByVal Method As MethodInfo) As SecurableMethodAttribute
            Return CType(Attribute.GetCustomAttribute(Method, AttributeType), SecurableMethodAttribute)
        End Function


#End Region

    End Class
End Namespace