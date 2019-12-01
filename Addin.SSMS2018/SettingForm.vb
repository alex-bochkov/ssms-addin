Public Class SettingForm
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        TextBoxScriptFolder.Text = SettingManager.GetTemplatesFolder()
        ComboBoxParserVersion.SelectedItem = SettingManager.GetSQLParserVersion()
        CheckBoxUsePoorManParser.Checked = SettingManager.GetSQLParserType()

    End Sub

    Private Sub Button2_Click(sender As Object, e As System.EventArgs) Handles Button2.Click

        Close()

    End Sub

    Private Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click

        If Not String.IsNullOrEmpty(TextBoxScriptFolder.Text) Then
            SettingManager.SaveTemplatesFolder(TextBoxScriptFolder.Text)
        End If

        If Not String.IsNullOrEmpty(ComboBoxParserVersion.SelectedItem) Then
            SettingManager.SaveSQLParserVersion(ComboBoxParserVersion.SelectedItem)
        End If

        SettingManager.SaveSQLParserType(CheckBoxUsePoorManParser.Checked)

        Close()

    End Sub

End Class