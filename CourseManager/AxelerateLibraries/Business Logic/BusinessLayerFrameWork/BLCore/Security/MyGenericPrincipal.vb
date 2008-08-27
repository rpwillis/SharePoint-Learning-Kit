Imports System.Security.Principal
Namespace BLCore.Security

    ''' <summary>
    ''' MyGenericPrincipal is a custom GenericPrincipal implementation that defines a security context 
    ''' (by exposing a SecurityContext object) and supports and extended role architecture.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public NotInheritable Class MyGenericPrincipal
        Inherits GenericPrincipal
        'Implements IPrincipal
        ' Protected m_Identity As GenericIdentity = New GenericIdentity("", "")
        Friend m_Roles() As String

        Protected m_SecurityScope As SecurityScope


#Region "Constructors"
        Public Sub New( _
            ByVal identity As IIdentity, _
            ByVal roles() As String)

            MyBase.New(identity, Nothing)
            m_SecurityScope = Nothing
            m_Roles = roles
        End Sub
#End Region


#Region "Business Properties and Methods"
        Public Property SecurityScope() As SecurityScope
            Get
                If m_SecurityScope Is Nothing Then
                    m_SecurityScope = New SecurityScope("")
                End If
                Return m_SecurityScope

            End Get
            Set(ByVal Value As SecurityScope)
                m_SecurityScope = Value

            End Set
        End Property


        Public Overrides Function IsInRole(ByVal role As String) As Boolean
            Return True

        End Function
#End Region
    End Class
End Namespace