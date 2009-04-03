Imports System.Reflection

Namespace BLCore.Security
    Public Class SecurityForm
        ''' <summary>
        ''' Browses the dll to add the Azman Security
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub buttonBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonBrowse.Click

            If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                textBoxDLL.Text = openFileDialog1.FileName
            End If

        End Sub
        ''' <summary>
        ''' Generates the Azman Security of the selected dll
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub buttonGenerateSecurity_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGenerateSecurity.Click
            Dim MyAssembly As Assembly = Assembly.LoadFile(textBoxDLL.Text)
            Try
                Throw New NotSupportedException()
            Catch ex As Exception
                Throw ex
            End Try

        End Sub
    End Class
End Namespace
