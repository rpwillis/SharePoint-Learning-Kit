
Namespace BLCore.Security

    ''' <summary>
    ''' This security operation always denies the access to the user.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class clsDeniedSecurityOperation
        Inherits clsSecurityOperation


        ''' <summary>
        ''' Always denies the access to the User
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CheckAccess() As Boolean
            Return False
        End Function

    End Class
End Namespace