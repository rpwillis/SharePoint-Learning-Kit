Namespace BLCore.Security

    ''' <summary>
    ''' Base class for Business Object Security Operation using Azman.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsBOOperation
        Inherits clsBLSecurityOperation
        Private m_BusinessBase As BLBusinessBase
        ''' <summary>
        ''' The Business Object associated to the operation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property BusinessBase() As BLBusinessBase
            Get
                Return m_BusinessBase
            End Get
        End Property
        ''' <summary>
        ''' Initializes the business object security operations
        ''' </summary>
        ''' <param name="BBase"></param>
        ''' The Business Object associated to the operation
        ''' <remarks></remarks>
        Public Sub New(ByVal BBase As BLBusinessBase)
            m_BusinessBase = BBase

        End Sub

    End Class
End Namespace
