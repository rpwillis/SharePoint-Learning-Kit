Imports System.Reflection

Namespace BLCore.Attributes
    ''' <summary>
    ''' Attribute that defines a Integer field as null
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class IntNullableAttribute
        Inherits NumericNullableAttribute
        Private m_nullableValue As Int32 = -1
        ''' <summary>
        ''' Defines the numeric Integer that determines the field as null
        ''' </summary>
        ''' <param name="nullValue">numeric Integer accepted as null</param>
        ''' <remarks></remarks>
        Sub New(ByVal nullValue As Int32)
            m_nullableValue = nullValue
        End Sub
        ''' <summary>
        ''' Defines the default numeric Integer that determines the field as null
        ''' </summary>
        ''' <remarks></remarks>
        Sub New()
            MyBase.New()
        End Sub
        ''' <summary>
        ''' returns the the Integer defined as null
        ''' </summary>
        ''' <Integer></Integer>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NullableValue() As Int32
            Get
                Return m_nullableValue
            End Get
        End Property
        ''' <summary>
        ''' Determines if the Integer is null
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function IsNull(ByVal value As Object) As Boolean
            Dim valid As Boolean = False
            If NullableValue.Equals(DirectCast(value, Int32)) Then
                valid = True
            End If
            Return valid
        End Function
    End Class
End Namespace
