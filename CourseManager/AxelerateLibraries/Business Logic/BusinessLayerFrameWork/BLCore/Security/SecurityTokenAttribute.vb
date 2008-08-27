Imports System.Reflection

Namespace BLCore.Security

    'Attribute that defines the security properties of a field

    <AttributeUsage(AttributeTargets.All)> _
    Public Class SecurityTokenAttribute
        Inherits Attribute

        Private m_Directory As String
        Private m_Name As String
        Private m_Description As String

        Public Sub New(ByVal nName As String, ByVal nDescription As String, ByVal nDirectory As String)
            m_Name = nName
            m_Description = nDescription
            m_Directory = nDirectory
        End Sub

        Public ReadOnly Property Name() As String
            Get
                Return m_Name
            End Get
        End Property

        Public ReadOnly Property Description() As String
            Get
                Return m_Description
            End Get
        End Property

        Public ReadOnly Property Directory() As String
            Get
                Return m_Directory
            End Get
        End Property

#Region "Attribute Checking"

        'Type of the attribute automapeable
        Private Shared AttributeType As Type = GetType(SecurityTokenAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal NType As Type) As Boolean
            Return Attribute.IsDefined(NType, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal NType As Type) As SecurityTokenAttribute
            Return CType(Attribute.GetCustomAttribute(NType, AttributeType), SecurityTokenAttribute)
        End Function


#End Region


    End Class
End Namespace