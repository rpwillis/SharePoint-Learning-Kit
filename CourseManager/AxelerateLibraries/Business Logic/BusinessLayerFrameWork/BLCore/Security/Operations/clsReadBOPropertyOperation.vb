Namespace BLCore.Security

    ''' <summary>
    ''' Security Operation to check access to read the property of Business Objects.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsReadBOPropertyOperation
        Inherits clsBOOperation
        Protected m_PropertyName As String
        Public Sub New(ByVal BBase As BLBusinessBase, ByVal PropertyName As String)
            MyBase.New(BBase)
            m_PropertyName = PropertyName
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Read" + BusinessBaseName + PropertyName
        End Sub
    End Class
End Namespace