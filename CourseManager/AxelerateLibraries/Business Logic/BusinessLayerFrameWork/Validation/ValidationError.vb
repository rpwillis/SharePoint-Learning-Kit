Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace BLCore.Validation
    ''' <summary>
    ''' Represents the existence of a validation problem within a BLBusinessBase
    ''' </summary>
    <Serializable()> _
    Public Class ValidationError
        ''' <summary>
        ''' Indicates the Error text
        ''' </summary>
        Private m_ErrorText As String

        ''' <summary>
        ''' Indicates the Error Location
        ''' </summary>
        Private m_ErrorLocation As String

        ''' <summary>
        ''' Initializes a new instance of the class with a specified error message and location
        ''' </summary>
        ''' <param name="pErrorText">Error Text</param>
        ''' <param name="pErrorLocation">Error Location</param>
        Public Sub New(ByVal pErrorText As String, ByVal pErrorLocation As String)
            m_ErrorText = pErrorText
            m_ErrorLocation = pErrorLocation
        End Sub

        ''' <summary>
        ''' Gets the text of the Error
        ''' </summary>
        Public ReadOnly Property ErrorText() As String
            Get
                Return m_ErrorText
            End Get
        End Property

        ''' <summary>
        ''' Gets the Error location
        ''' </summary>
        Public ReadOnly Property ErrorLocation() As String
            Get
                Return m_ErrorLocation
            End Get
        End Property


    End Class
End Namespace