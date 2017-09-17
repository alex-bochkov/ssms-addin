Public Class SettingFormUI
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        TemplateScriptFolderTextBox.Text = SettingManager.GetTemplatesFolder()
        ExcelExportFolderTextBox.Text = SettingManager.GetExcelExportFolder()
        TSQLFormatVersion.SelectedItem = SettingManager.GetTSQLFormatVersion

    End Sub

    Private Sub SaveButon_Click(sender As Object, e As System.EventArgs) Handles SaveButon.Click

        SettingManager.SaveTemplatesFolder(TemplateScriptFolderTextBox.Text)
        SettingManager.SaveExcelExportFolder(ExcelExportFolderTextBox.Text)
        SettingManager.SaveTSQLFormatVersion(TSQLFormatVersion.SelectedItem)

    End Sub
End Class