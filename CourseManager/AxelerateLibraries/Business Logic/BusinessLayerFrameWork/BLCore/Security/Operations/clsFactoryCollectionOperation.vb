Namespace BLCore.Security

    ''' <summary>
    ''' Security operations that checks if the user has rights to execute the selected factory of a collection.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsFactoryCollectionOperation
        Inherits clsCollectionOperation

        Private m_MethodName As String
        Public Sub New(ByVal Collection As Object, ByVal MethodName As String)
            MyBase.New(Collection)
            m_MethodName = MethodName
            Dim CollectionName As String = Collection.GetType().Name
            CollectionName = CollectionName.Replace("cls", "")
            Name = "Factory" + CollectionName + MethodName

        End Sub
    End Class
End Namespace