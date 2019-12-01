<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SettingForm
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
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBoxScriptFolder = New System.Windows.Forms.TextBox()
        Me.ComboBoxParserVersion = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(179, 63)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(141, 34)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Save"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(326, 62)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(126, 35)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "Cancel And Close"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(119, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "TSQL Templates Folder"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(10, 33)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(106, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "TSQL Parser Version"
        '
        'TextBoxScriptFolder
        '
        Me.TextBoxScriptFolder.Location = New System.Drawing.Point(136, 7)
        Me.TextBoxScriptFolder.Name = "TextBoxScriptFolder"
        Me.TextBoxScriptFolder.Size = New System.Drawing.Size(316, 20)
        Me.TextBoxScriptFolder.TabIndex = 4
        '
        'ComboBoxParserVersion
        '
        Me.ComboBoxParserVersion.FormattingEnabled = True
        Me.ComboBoxParserVersion.Items.AddRange(New Object() {"SQL Server 2008", "SQL Server 2012", "SQL Server 2014", "SQL Server 2016", "SQL Server 2017", "SQL Server 2019"})
        Me.ComboBoxParserVersion.Location = New System.Drawing.Point(136, 30)
        Me.ComboBoxParserVersion.Name = "ComboBoxParserVersion"
        Me.ComboBoxParserVersion.Size = New System.Drawing.Size(316, 21)
        Me.ComboBoxParserVersion.TabIndex = 5
        '
        'SettingForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(460, 105)
        Me.Controls.Add(Me.ComboBoxParserVersion)
        Me.Controls.Add(Me.TextBoxScriptFolder)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Name = "SettingForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "DBA Helper Setting"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TextBoxScriptFolder As System.Windows.Forms.TextBox
    Friend WithEvents ComboBoxParserVersion As System.Windows.Forms.ComboBox
End Class
