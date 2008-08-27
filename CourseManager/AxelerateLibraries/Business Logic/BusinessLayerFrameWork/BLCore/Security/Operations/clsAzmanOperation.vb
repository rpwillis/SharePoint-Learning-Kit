Imports Microsoft.Interop.Security.AzRoles

Namespace BLCore.Security

    ''' <summary>
    ''' Base class for Azman operation oriented authorization.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class clsAzmanOperation
        Inherits clsSecurityOperation


        ''' <summary>
        ''' Grants access to User or Group
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GrantAccess() As Boolean
            Return True
        End Function
        ''' <summary>
        ''' Revokes access to User or Group
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function RevokeAccess() As Boolean

            Return True
        End Function
        ''' <summary>
        ''' Check access for User
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function CheckAccess() As Boolean
            Return clsAzmanUtility.CheckAccess(Name)
        End Function


        ''' <summary>
        ''' Gets the number of the operation
        ''' </summary>
        ''' <returns></returns>
        ''' Returns a integer with the number of the operation
        ''' <remarks></remarks>
        Public Overridable Function GetOperationID() As Integer
            Return clsAzmanUtility.GetOperationID(Name)

        End Function


    End Class
End Namespace