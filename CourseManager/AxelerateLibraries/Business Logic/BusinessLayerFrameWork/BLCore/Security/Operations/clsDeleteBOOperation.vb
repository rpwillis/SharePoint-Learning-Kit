Namespace BLCore.Security

    ''' <summary>
    ''' Security Operation to check access to delete the Business Objects.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsDeleteBOOperation
        Inherits clsBOOperation
        Public Sub New(ByVal BBase As BLBusinessBase)
            MyBase.New(BBase)
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Delete" + BusinessBaseName
        End Sub
    End Class
End Namespace
