Namespace BLCore.Security

    ''' <summary>
    ''' Base class for Business List Security Operation using Azman.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsCollectionOperation
        Inherits clsBLSecurityOperation
        Private m_ListBase As IBLListBase
        ''' <summary>
        ''' The ListBase associated to the operation
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListBase() As IBLListBase
            Get
                Return m_ListBase
            End Get
        End Property
        ''' <summary>
        ''' Initializes the business List security operations
        ''' </summary>
        ''' <param name="Collection"></param>
        ''' The Business List associated to the operation
        ''' <remarks></remarks>
        Public Sub New(ByVal Collection As Object)
            m_ListBase = CType(Collection, IBLListBase)
        End Sub

    End Class
End Namespace
