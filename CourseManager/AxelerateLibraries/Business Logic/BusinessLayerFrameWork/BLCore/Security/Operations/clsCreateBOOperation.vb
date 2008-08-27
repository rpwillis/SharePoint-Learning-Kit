Namespace BLCore.Security

    ''' <summary>
    ''' Security Operation to check access to create the Business Objects.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsCreateBOOperation
        Inherits clsBOOperation

        Public Sub New(ByVal BBase As BLBusinessBase)
            MyBase.New(BBase)
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Create" + BusinessBaseName
        End Sub
    End Class
End Namespace
