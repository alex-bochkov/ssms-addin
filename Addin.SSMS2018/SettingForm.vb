Public Class SettingForm
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        TextBoxScriptFolder.Text = SettingManager.GetTemplatesFolder()

    End Sub

    Private Sub Button2_Click(sender As Object, e As System.EventArgs) Handles Button2.Click

        Close()

    End Sub

    Private Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click

        SettingManager.SaveTemplatesFolder(TextBoxScriptFolder.Text)

        Close()

    End Sub
End Class