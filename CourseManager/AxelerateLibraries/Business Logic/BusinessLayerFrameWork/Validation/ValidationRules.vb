Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace BLCore.Validation
    ''' <summary>
    ''' Collection of validation rules for a specific BLBusinessBase object.
    ''' </summary>
    <Serializable()> _
    Public Class ValidationRules
        Inherits Dictionary(Of String, ValidationRule)
        ''' <summary>
        ''' Initializes a new instance
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Returns a value indicating whether there are any broken rules at this time. For every error found
        ''' a ValidationError is created and inserted in the ValidationErrors collection of the BLBusinessBase object.
        ''' </summary>
        ''' <param name="RuleTarget">BLBusinessBase object instance to be checked for errors</param>
        ''' <returns>True if no valiadtion errors. False if errors found</returns>
        Public Function IsValid(ByVal RuleTarget As BLBusinessBase, ByVal PropertyHasChanged As Boolean) As Boolean
            RuleTarget.ValidationErrors.Clear()
            For Each Rule As ValidationRule In Me.Values
                Dim Validate As Boolean = True
                If PropertyHasChanged AndAlso Not Rule.ValidateWhenPropertyChanges Then
                    Validate = False
                End If

                If Validate Then
                    Dim ErrorMessage As String = ""
                    Dim Valid As Boolean = Rule.isValid(RuleTarget, ErrorMessage)
                    If Not Valid Then
                        Dim ErrorLocation As String = RuleTarget.[GetType]().Name
                        If Rule.[GetType]().IsSubclassOf(GetType(PropertyValidationRule)) Then
                            ErrorLocation = DirectCast(Rule, PropertyValidationRule).PropertyName
                        End If

                        Dim [Error] As New ValidationError(ErrorMessage, ErrorLocation)
                        RuleTarget.ValidationErrors.Add([Error])
                    End If
                End If
            Next
            Return RuleTarget.ValidationErrors.Count = 0
        End Function
    End Class
End Namespace