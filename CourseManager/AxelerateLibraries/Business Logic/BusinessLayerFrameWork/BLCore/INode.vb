Namespace BLCore
    Public Interface INode

        ''' <summary>
        ''' Parent Node of this INode, can be a null if no has parent
        ''' </summary>
        ''' <param name="TreeSelector"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Parent(ByVal TreeSelector As String) As INode
        ReadOnly Property Icon() As System.Drawing.Bitmap
        ReadOnly Property Name() As String
        Function HasChilds(ByVal TreeSelector As String) As Boolean
        Function Childs(ByVal TreeSelector As String) As IList

    End Interface
End Namespace
