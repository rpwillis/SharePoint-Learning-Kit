Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a field,property or method has with the Test Framework.
    ''' The field,property or method skip the test.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.All)> _
    Public Class staSkipAttribute
        Inherits staBaseAttribute

        ''' <summary>
        ''' Enum representing the locations code can execute.
        ''' </summary>
        Public Enum SkipReasons
            Obsolete
            Unfinished
            NoReason
        End Enum
        Private m_SkipReason As SkipReasons
        Public Sub New(Optional ByVal SkipReason As SkipReasons = SkipReasons.NoReason)
            m_SkipReason = SkipReason
        End Sub




#Region "Attribute Checking"

        'Type of the attribute 
        Private Shared AttributeType As Type = GetType(staSkipAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Member As MemberInfo) As Boolean
            Return Attribute.IsDefined(Member, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal Member As MemberInfo) As staSkipAttribute
            Return CType(Attribute.GetCustomAttribute(Member, AttributeType), staSkipAttribute)
        End Function


#End Region




    End Class


End Namespace
