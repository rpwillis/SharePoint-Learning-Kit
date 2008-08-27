
Namespace BLCore.Security
    ''' <summary>
    ''' Security exception used when a demand operation fails
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SecurityException
        Inherits Exception
        ''' <summary>
        ''' Initializes the Security Exception
        ''' </summary>
        ''' <param name="message"></param>
        ''' The Description of the Security Exception
        ''' <remarks></remarks>
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub
    End Class
End Namespace
