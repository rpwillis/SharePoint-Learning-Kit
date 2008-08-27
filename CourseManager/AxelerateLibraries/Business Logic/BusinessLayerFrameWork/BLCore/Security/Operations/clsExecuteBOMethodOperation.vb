Namespace BLCore.Security

    ''' <summary>
    ''' Security operations that checks if the user has rights to execute the method of a business object.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
Public Class clsExecuteBOMethodOperation
        Inherits clsBOOperation
        ''' <summary>
        ''' The name of the method associated to the operation
        ''' </summary>
        ''' <remarks></remarks>
        Private m_MethodName As String
        ''' <summary>
        ''' Initializes the security operation
        ''' </summary>
        ''' <param name="BBase"></param>
        ''' The businessbase associated to the operation
        ''' <param name="MethodName"></param>
        ''' The name of the method associated to the operation
        ''' <remarks></remarks>
        Public Sub New(ByVal BBase As BLBusinessBase, ByVal MethodName As String)
            MyBase.New(BBase)
            m_MethodName = MethodName
            Dim BusinessBaseName As String = BBase.GetType().Name
            BusinessBaseName = BusinessBaseName.Replace("cls", "")
            Name = "Execute" + BusinessBaseName + MethodName
        End Sub
    End Class
End Namespace