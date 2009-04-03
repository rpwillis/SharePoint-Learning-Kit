Imports Microsoft.Interop.Security.AzRoles

Namespace BLCore.Security

    ''' <summary>
    ''' This class checks if the user has access to the role.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class clsRoleOperation
        Inherits clsAzmanOperation

        ''' <summary>
        ''' Initializes the role operations
        ''' </summary>
        ''' <param name="RoleName"></param>
        ''' The name of the role to be checked
        ''' <remarks></remarks>
        Public Sub New(ByVal RoleName As String)
            Name = RoleName
        End Sub



    End Class
End Namespace
