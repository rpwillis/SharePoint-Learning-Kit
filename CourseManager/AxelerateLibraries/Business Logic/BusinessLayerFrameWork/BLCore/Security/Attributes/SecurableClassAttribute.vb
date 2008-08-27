Imports System.Reflection

Namespace BLCore.Security

    ''' <summary>    
    ''' Defines the relation that a class has with the Security Framework.
    ''' Defines the security properties of a class.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class)> _
    Public Class SecurableClassAttribute
        Inherits SecurableAttribute
        ''' <summary>
        ''' Enum representing the the types of security in the classes.
        ''' </summary>
        Public Enum SecurableTypes
            CRUD
            Read
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
        ''' The type that describes the security of the class
        ''' <remarks></remarks>
        Public Sub New(Optional ByVal Type As SecurableTypes = SecurableTypes.CRUD)
            m_SecurableType = Type
        End Sub
        ''' <summary>
        ''' Defines if the read security operation for the class, needs to perform the security.
        ''' </summary>
        ''' <returns></returns>
        ''' returns true if the security must check a read access
        ''' <remarks></remarks>
        Public Function MustCheckReadAccess() As Boolean
            If (SecurableType = SecurableTypes.CRUD) Or (SecurableType = SecurableTypes.Read) Then
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' Defines if the create security operation for the class, needs to perform the security.
        ''' </summary>
        ''' <returns></returns>
        ''' returns true if the security must check a read access
        ''' <remarks></remarks>
        Public Function MustCheckCreateAccess() As Boolean
            If SecurableType = SecurableTypes.CRUD Then
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' Defines if the update security operation for the class, needs to perform the security.
        ''' </summary>
        ''' <returns></returns>
        ''' returns true if the security must check a read access
        ''' <remarks></remarks>
        Public Function MustCheckUpdateAccess() As Boolean
            If SecurableType = SecurableTypes.CRUD Then
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' Defines if the delete security operation for the class, needs to perform the security.
        ''' </summary>
        ''' <returns></returns>
        ''' returns true if the security must check a read access
        ''' <remarks></remarks>
        Public Function MustCheckDeleteAccess() As Boolean
            If SecurableType = SecurableTypes.CRUD Then
                Return True
            End If
            Return False
        End Function

#Region "Attribute Checking"

        ''' <summary>
        ''' Type of Automap attribute
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared AttributeType As Type = GetType(SecurableClassAttribute)

        ''' <summary>
        ''' Indicates if the type has defined the AutoMapField Attribute
        ''' </summary>
        ''' <param name="NType"></param>
        ''' The type to be checked
        ''' <returns></returns>
        ''' Returns true if the type has the attribute defined
        ''' <remarks></remarks>
        Public Overloads Shared Function isDefined(ByVal NType As Type) As Boolean
            Return Attribute.IsDefined(NType, AttributeType)
        End Function

        ''' <summary>
        ''' Searchs in the selected type, the securable attribute
        ''' </summary>
        ''' <param name="NType"></param>
        ''' The type to be checked
        ''' <returns></returns>
        ''' Returns the securable attribute of the type
        ''' <remarks></remarks>
        Public Overloads Shared Function GetAttribute(ByVal NType As Type) As SecurableClassAttribute
            Return CType(Attribute.GetCustomAttribute(NType, AttributeType), SecurableClassAttribute)
        End Function


#End Region

    End Class
End Namespace