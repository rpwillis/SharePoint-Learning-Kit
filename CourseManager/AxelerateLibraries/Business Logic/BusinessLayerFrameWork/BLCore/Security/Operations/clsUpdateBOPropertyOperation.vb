Namespace BLCore.Security

    ''' <summary>
    ''' Security Operation to check access to update the property of Business Objects.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsUpdateBOPropertyOperation
        Inherits clsBOOperation
        Private m_PropertyName As String
        Public Sub New(ByVal BBase As BLBusinessBase, ByVal PropertyName As String)
            MyBase.New(BBase)
            m_PropertyName = PropertyName
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Update" + BusinessBaseName + PropertyName
        End Sub
    End Class
End Namespace