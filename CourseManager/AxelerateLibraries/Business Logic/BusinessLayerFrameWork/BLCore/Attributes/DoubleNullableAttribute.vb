Imports System.Reflection

Namespace BLCore.Attributes
    ''' <summary>
    ''' Attribute that defines a Double field as null
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class DoubleNullableAttribute
        Inherits NumericNullableAttribute
        Private m_nullableValue As Double = -1
        ''' <summary>
        ''' Defines the numeric Double that determines the field as null
        ''' </summary>
        ''' <param name="nullValue">numeric Double accepted as null</param>
        ''' <remarks></remarks>
        Sub New(ByVal nullValue As Double)
            m_nullableValue = nullValue
        End Sub
        ''' <summary>
        ''' Defines the default numeric Double that determines the field as null
        ''' </summary>
        ''' <remarks></remarks>
        Sub New()
            MyBase.New()
        End Sub
        ''' <summary>
        ''' returns the the Double defined as null
        ''' </summary>
        ''' <Double></Double>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property NullableValue() As Double
            Get
                Return m_nullableValue
            End Get
        End Property
        ''' <summary>
        ''' Determines if the Double is null
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function IsNull(ByVal value As Object) As Boolean
            Dim valid As Boolean = False
            If NullableValue.Equals(DirectCast(value, Double)) Then
                valid = True
            End If
            Return valid
        End Function
    End Class
End Namespace
