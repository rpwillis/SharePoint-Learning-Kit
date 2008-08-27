Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' This attribute is used to mark a field that belongs to a class as a complex object that needs to be fetched, 
    ''' updated, inserted or deleted when one of these operations is performed on its parent object (the class that 
    ''' contains this field).  The expected type for a field marked with this attribute is any type 
    ''' that inherits from BLBusinessBase.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class ClassFieldMapAttribute
        Inherits Attribute

        'Indicates if it's usable during the Fetch operation
        Private m_isFetchField As Boolean

        'Indicates if it's usable during the Update operation
        Private m_isUpdateField As Boolean

        'True if it must do the security check
        'false if it uses the seurity check for the parent
        Private m_UseParentSecurity As Boolean


        'Returns true if it uses the Fetch operation
        Public ReadOnly Property isFetchField() As Boolean
            Get
                Return m_isFetchField
            End Get
        End Property

        'Returns true if it uses the Update operation
        Public ReadOnly Property isUpdateField() As Boolean
            Get
                Return m_isUpdateField
            End Get
        End Property


        Public ReadOnly Property UseParentSecurity() As Boolean
            Get
                Return m_UseParentSecurity
            End Get
        End Property



        Public Sub New(Optional ByVal isFetchField As Boolean = True, Optional ByVal isUpdateField As Boolean = True, _
            Optional ByVal UseParentSecurity As Boolean = True)
            m_isFetchField = isFetchField
            m_isUpdateField = isUpdateField
            m_UseParentSecurity = UseParentSecurity
        End Sub

#Region "Attribute Checking"

        'Type of the attribute
        Private Shared AttributeType As Type = GetType(ClassFieldMapAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Field As FieldInfo) As Boolean
            Return Attribute.IsDefined(Field, AttributeType)
        End Function

        'Obtains the AutoFieldMapAttribute
        Public Overloads Shared Function GetAttribute(ByVal Field As FieldInfo) As ClassFieldMapAttribute
            Return CType(Attribute.GetCustomAttribute(Field, AttributeType), ClassFieldMapAttribute)
        End Function

#End Region



    End Class
End Namespace