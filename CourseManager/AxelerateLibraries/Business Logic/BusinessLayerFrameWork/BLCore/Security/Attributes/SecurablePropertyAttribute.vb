Imports System.Reflection

Namespace BLCore.Security

    ''' <summary>    
    ''' Defines the relation that a property has with the Security Framework.
    ''' Defines the security properties of a property.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property)> _
    Public Class SecurablePropertyAttribute
        Inherits SecurableAttribute
        ''' <summary>
        ''' Enum representing the the types of security in the properties.
        ''' </summary>
        Public Enum SecurableTypes
            Read
            Update
        End Enum
        Private m_SecurableType As SecurableTypes
        Public ReadOnly Property SecurableType() As SecurableTypes
            Get
                Return m_SecurableType
            End Get
        End Property

        ''' <summary>
        ''' Initializes the securable attribute
        ''' </summary>
        ''' <param name="Type"></param>
        ''' The type that describes the security of the property
        ''' <remarks></remarks>
        Public Sub New(Optional ByVal Type As SecurableTypes = SecurableTypes.Read)
            m_SecurableType = Type
        End Sub

#Region "Attribute Checking"

        ''' <summary>
        ''' Type of Automap attribute
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared AttributeType As Type = GetType(SecurablePropertyAttribute)

        ''' <summary>
        ''' Indicates if the property has defined the AutoMapField Attribute
        ''' </summary>
        ''' <param name="NProperty"></param>
        ''' The property to be checked
        ''' <returns></returns>
        ''' Returns true if the property has the attribute defined
        ''' <remarks></remarks>
        Public Overloads Shared Function isDefined(ByVal NProperty As PropertyInfo) As Boolean
            Return Attribute.IsDefined(NProperty, AttributeType)
        End Function

        ''' <summary>
        ''' Searchs in the selected property, the securable attribute
        ''' </summary>
        ''' <param name="NProperty"></param>
        ''' The Property to be checked
        ''' <returns></returns>
        ''' Returns the securable attribute of the property
        ''' <remarks></remarks>
        Public Overloads Shared Function GetAttribute(ByVal NProperty As PropertyInfo) As SecurablePropertyAttribute
            Return CType(Attribute.GetCustomAttribute(NProperty, AttributeType), SecurablePropertyAttribute)
        End Function


#End Region

    End Class
End Namespace