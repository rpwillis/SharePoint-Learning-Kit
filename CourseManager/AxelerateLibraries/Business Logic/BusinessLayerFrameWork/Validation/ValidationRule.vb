Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace BLCore.Validation
    ''' <summary>
    ''' Represents a validation rule for a BLBusinessBase
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public MustInherit Class ValidationRule
        Inherits Attribute
        Private m_ValidateWhenPropertyChanges As Boolean = True

        ''' <summary>
        ''' Initializes a new instance
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Checks the object state to see if it is valid
        ''' </summary>
        ''' <param name="RuleTarget">Provisioning object to be checked</param>
        ''' <param name="ErrorMessage">Message Error generated if the object state is invalid</param>
        ''' <returns></returns>
        Public MustOverride Function isValid(ByVal RuleTarget As BLBusinessBase, ByRef ErrorMessage As String) As Boolean

        Public Property ValidateWhenPropertyChanges() As Boolean
            Get
                Return m_ValidateWhenPropertyChanges
            End Get
            Set(ByVal value As Boolean)
                m_ValidateWhenPropertyChanges = value
            End Set
        End Property
    End Class
End Namespace