Namespace BLCore.Security

    ''' <summary>
    ''' Base class for operation oriented authorization.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class clsSecurityOperation

        Private m_Name As String

        ''' <summary>
        ''' The name of the security operation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        ''' <summary>
        ''' Grants access to User or Group
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GrantAccess() As Boolean
            Return True
        End Function
        ''' <summary>
        ''' Revokes access to User or Group
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function RevokeAccess() As Boolean
            Return True
        End Function
        ''' <summary>
        ''' Check access for User
        ''' </summary>
        ''' <returns></returns>
        ''' Returns true if the user has enough rights to perform the operation.
        ''' <remarks></remarks>
        Public Overridable Function CheckAccess() As Boolean
            Return True
        End Function

        ''' <summary>
        ''' Demand access for User. Throws a Security Exception if the user doesn't have
        ''' enough rights to perform the operation
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub DemandAccess()
            If Not CheckAccess() Then
                Throw New SecurityException(My.Resources.BLResources.errPerformingPermission1 _
                            + Name + My.Resources.BLResources.errPerformingPermission2)
            End If
        End Sub
    End Class
End Namespace