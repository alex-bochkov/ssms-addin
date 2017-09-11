<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SettingFormUI
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ExcelExportFolderTextBox = New System.Windows.Forms.TextBox()
        Me.SaveButon = New System.Windows.Forms.Button()
        Me.TSQLFormatVersion = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'TemplateScriptFolderTextBox
        '
        Me.TemplateScriptFolderTextBox.Location = New System.Drawing.Point(173, 9)
        Me.TemplateScriptFolderTextBox.Name = "TemplateScriptFolderTextBox"
        Me.TemplateScriptFolderTextBox.Size = New System.Drawing.Size(347, 20)
        Me.TemplateScriptFolderTextBox.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 12)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(99, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Templates directory"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 35)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(108, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Excel export directory"
        '
        'ExcelExportFolderTextBox
        '
        Me.ExcelExportFolderTextBox.Location = New System.Drawing.Point(173, 32)
        Me.ExcelExportFolderTextBox.Name = "ExcelExportFolderTextBox"
        Me.ExcelExportFolderTextBox.Size = New System.Drawing.Size(347, 20)
        Me.ExcelExportFolderTextBox.TabIndex = 0
        '
        'SaveButon
        '
        Me.SaveButon.Location = New System.Drawing.Point(15, 84)
        Me.SaveButon.Name = "SaveButon"
        Me.SaveButon.Size = New System.Drawing.Size(75, 23)
        Me.SaveButon.TabIndex = 2
        Me.SaveButon.Text = "Save"
        Me.SaveButon.UseVisualStyleBackColor = True
        '
        'TSQLFormatVersion
        '
        Me.TSQLFormatVersion.FormattingEnabled = True
        Me.TSQLFormatVersion.Items.AddRange(New Object() {"2008", "2012", "2014", "2016", "2017"})
        Me.TSQLFormatVersion.Location = New System.Drawing.Point(173, 59)
        Me.TSQLFormatVersion.Name = "TSQLFormatVersion"
        Me.TSQLFormatVersion.Size = New System.Drawing.Size(127, 21)
        Me.TSQLFormatVersion.TabIndex = 3
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 59)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(107, 13)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Format TSQL version"
        '
        'SettingFormUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(642, 282)
        Me.Controls.Add(Me.TSQLFormatVersion)
        Me.Controls.Add(Me.SaveButon)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ExcelExportFolderTextBox)
        Me.Controls.Add(Me.TemplateScriptFolderTextBox)
        Me.Name = "SettingFormUI"
        Me.Text = "SettingFormUI"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TemplateScriptFolderTextBox As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ExcelExportFolderTextBox As System.Windows.Forms.TextBox
    Friend WithEvents SaveButon As System.Windows.Forms.Button
    Friend WithEvents TSQLFormatVersion As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
End Class
