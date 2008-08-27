Namespace BLCore.Security
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class SecurityForm
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.openFileDialog1 = New System.Windows.Forms.OpenFileDialog
            Me.buttonGenerateSecurity = New System.Windows.Forms.Button
            Me.buttonBrowse = New System.Windows.Forms.Button
            Me.textBoxDLL = New System.Windows.Forms.TextBox
            Me.label2 = New System.Windows.Forms.Label
            Me.SuspendLayout()
            '
            'buttonGenerateSecurity
            '
            Me.buttonGenerateSecurity.Location = New System.Drawing.Point(12, 83)
            Me.buttonGenerateSecurity.Name = "buttonGenerateSecurity"
            Me.buttonGenerateSecurity.Size = New System.Drawing.Size(122, 23)
            Me.buttonGenerateSecurity.TabIndex = 10
            Me.buttonGenerateSecurity.Text = "Generate Security"
            Me.buttonGenerateSecurity.UseVisualStyleBackColor = True
            '
            'buttonBrowse
            '
            Me.buttonBrowse.Location = New System.Drawing.Point(321, 40)
            Me.buttonBrowse.Name = "buttonBrowse"
            Me.buttonBrowse.Size = New System.Drawing.Size(75, 23)
            Me.buttonBrowse.TabIndex = 9
            Me.buttonBrowse.Text = "Browse"
            Me.buttonBrowse.UseVisualStyleBackColor = True
            '
            'textBoxDLL
            '
            Me.textBoxDLL.Location = New System.Drawing.Point(12, 43)
            Me.textBoxDLL.Name = "textBoxDLL"
            Me.textBoxDLL.Size = New System.Drawing.Size(303, 20)
            Me.textBoxDLL.TabIndex = 8
            '
            'label2
            '
            Me.label2.AutoSize = True
            Me.label2.Location = New System.Drawing.Point(12, 9)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(105, 13)
            Me.label2.TabIndex = 7
            Me.label2.Text = "Please select a DLL:"
            '
            'SecurityForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(416, 136)
            Me.Controls.Add(Me.buttonGenerateSecurity)
            Me.Controls.Add(Me.buttonBrowse)
            Me.Controls.Add(Me.textBoxDLL)
            Me.Controls.Add(Me.label2)
            Me.Name = "SecurityForm"
            Me.Text = "SecurityForm"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Private WithEvents openFileDialog1 As System.Windows.Forms.OpenFileDialog
        Private WithEvents buttonGenerateSecurity As System.Windows.Forms.Button
        Private WithEvents buttonBrowse As System.Windows.Forms.Button
        Private WithEvents textBoxDLL As System.Windows.Forms.TextBox
        Private WithEvents label2 As System.Windows.Forms.Label
    End Class
End Namespace