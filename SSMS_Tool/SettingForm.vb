Public Class SettingForm
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        TemplateScriptFolderTextBox.Text = SettingManager.GetTemplatesFolder()
        ExcelExportFolderTextBox.Text = SettingManager.GetExcelExportFolder()

    End Sub

    Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
        'Visible = False
        'Me.Dispose()


    End Sub

    Private Sub ButtonSave_Click(sender As Object, e As EventArgs) Handles ButtonSave.Click

        SettingManager.SaveTemplatesFolder(TemplateScriptFolderTextBox.Text)
        SettingManager.SaveExcelExportFolder(ExcelExportFolderTextBox.Text)

    End Sub
End Class
