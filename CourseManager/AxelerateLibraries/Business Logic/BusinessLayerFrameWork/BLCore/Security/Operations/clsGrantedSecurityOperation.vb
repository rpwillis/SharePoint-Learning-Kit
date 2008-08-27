
Namespace BLCore.Security

    ''' <summary>
    ''' This security operation always grants the access to the user.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class clsGrantedSecurityOperation
        Inherits clsSecurityOperation



        ''' <summary>
        ''' Always grants the access to the User
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CheckAccess() As Boolean
            Return True
        End Function

    End Class
End Namespace