Namespace BLCore.Security

    ''' <summary>
    ''' Security Operation to check access to update the Business Objects.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsUpdateBOOperation
        Inherits clsBOOperation
        Public Sub New(ByVal BBase As BLBusinessBase)
            MyBase.New(BBase)
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Update" + BusinessBaseName
        End Sub
    End Class
End Namespace