Namespace BLCore.Security

    ''' <summary>
    ''' The security context object is intended to be defined in the authentication process and 
    ''' since then it will be passed into each framework security call so the business objects can validate the security based on this context.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class SecurityScope
        Private m_SecurityScope As String

        'Indicates that the process is a process of the system and has total permissions
        <NonSerialized()> _
        Private m_SystemProcess As Boolean = False

        Public Sub New()
            m_SecurityScope = ""
            m_SystemProcess = False
        End Sub
        Public Sub New(ByVal NIDSecurityContext As String)
            m_SecurityScope = NIDSecurityContext
            m_SystemProcess = False
        End Sub

        Public ReadOnly Property IDSecurityContext() As String
            Get
                Return m_SecurityScope.Trim
            End Get
        End Property
        Public Property SystemProcess() As Boolean
            Get
                Return m_SystemProcess
            End Get
            Set(ByVal Value As Boolean)
                m_SystemProcess = Value
            End Set
        End Property

        'Redefines the method ToString so it returns the unique identifier(Criteria) of the object
        Public Overrides Function ToString() As String
            Return m_SecurityScope
        End Function

        Public Shared ReadOnly Property UnauthorizedString() As String
            Get
                Return My.Resources.BLResources.noAuthorized
            End Get
        End Property



    End Class
End Namespace