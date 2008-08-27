Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a method has with the Test Framework.
    ''' The method with this attribute is a factory.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Method)> _
    Public Class staFactoryAttribute
        Inherits staBaseAttribute

        'Use this field to save specific values and types for the parameters of the method.
        Public ParameterName As String = ""
        Public ParameterValue As Object = Nothing

        'New constructor that receives specific values and types for the parameters of the method in a string
        Public Sub New(ByVal pParameterName As String, ByVal pParameterValue As Object)

            ParameterName = pParameterName
            ParameterValue = pParameterValue
        End Sub

        Public Sub New()

        End Sub



#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(staFactoryAttribute)

        'Indicates if Method has the staFactoryAttribute defined
        Public Overloads Shared Function isDefined(ByVal Method As MethodInfo) As Boolean
            Return Attribute.IsDefined(Method, AttributeType)
        End Function

        'Indicates if Method has the staFactoryAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Method As MethodInfo) As staFactoryAttribute
            Return CType(Attribute.GetCustomAttribute(Method, AttributeType), staFactoryAttribute)
        End Function

        'Indicates if Method has the staFactoryAttribute defined
        Public Overloads Shared Function GetAttributes(ByVal Method As MethodInfo) As staFactoryAttribute()
            Return CType(Attribute.GetCustomAttributes(Method, AttributeType), staFactoryAttribute())
        End Function
#End Region




    End Class
    Public Class staParameter
        Inherits Pair(Of String, Object)
        Public Sub New(ByVal pParameterName As String, ByVal pParameterValue As Object)
            MyBase.New(pParameterName, pParameterValue)
        End Sub
    End Class
End Namespace
