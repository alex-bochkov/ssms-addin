<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SettingForm
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TemplateScriptFolderTextBox = New System.Windows.Forms.TextBox()
        Me.ButtonSave = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.ExcelExportFolderTextBox = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'TemplateScriptFolderTextBox
        '
        Me.TemplateScriptFolderTextBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TemplateScriptFolderTextBox.Location = New System.Drawing.Point(123, 21)
        Me.TemplateScriptFolderTextBox.Name = "TemplateScriptFolderTextBox"
        Me.TemplateScriptFolderTextBox.Size = New System.Drawing.Size(374, 20)
        Me.TemplateScriptFolderTextBox.TabIndex = 0
        '
        'ButtonSave
        '
        Me.ButtonSave.Location = New System.Drawing.Point(17, 101)
        Me.ButtonSave.Name = "ButtonSave"
        Me.ButtonSave.Size = New System.Drawing.Size(75, 23)
        Me.ButtonSave.TabIndex = 1
        Me.ButtonSave.Text = "Save"
        Me.ButtonSave.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(4, 24)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(113, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Template scripts folder"
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Location = New System.Drawing.Point(98, 101)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 1
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'ExcelExportFolderTextBox
        '
        Me.ExcelExportFolderTextBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ExcelExportFolderTextBox.Location = New System.Drawing.Point(123, 47)
        Me.ExcelExportFolderTextBox.Name = "ExcelExportFolderTextBox"
        Me.ExcelExportFolderTextBox.Size = New System.Drawing.Size(374, 20)
        Me.ExcelExportFolderTextBox.TabIndex = 0
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(4, 50)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(94, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Excel export folder"
        '
        'SettingForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonSave)
        Me.Controls.Add(Me.ExcelExportFolderTextBox)
        Me.Controls.Add(Me.TemplateScriptFolderTextBox)
        Me.Name = "SettingForm"
        Me.Size = New System.Drawing.Size(500, 132)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TemplateScriptFolderTextBox As Windows.Forms.TextBox
    Friend WithEvents ButtonSave As Windows.Forms.Button
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents ButtonCancel As Windows.Forms.Button
    Friend WithEvents ExcelExportFolderTextBox As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
End Class
