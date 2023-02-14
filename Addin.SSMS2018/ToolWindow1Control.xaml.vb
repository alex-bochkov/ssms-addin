

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports EnvDTE
Imports Microsoft.SqlServer.TransactSql.ScriptDom
Imports Microsoft.VisualStudio.Shell
'''<summary>
''' Interaction logic for ToolWindow1Control.xaml
'''</summary>
Partial Public Class ToolWindow1Control
    Inherits System.Windows.Controls.UserControl

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        AddFiles(False)

        MenuSettings.Icon = GetImage("settings")
        'MenuFormat.Icon = GetImage("magic_wand")


    End Sub


    Function AddFiles(Optional Interactive As Boolean = True)

        FileMenuTemplates.Items.Clear()

        Dim Folder = SettingManager.GetTemplatesFolder()

        If My.Computer.FileSystem.DirectoryExists(Folder) Then

            Dim i As Integer = 1

            CreateCommands(FileMenuTemplates, Folder, i)

        Else
            If Interactive Then
                System.Windows.MessageBox.Show("Folder '" + Folder + "' doesn't exist!")
            End If
        End If

        Dim mi = New MenuItem
        mi.Header = "Refresh This List"
        mi.Icon = GetImage("refresh")

        AddHandler mi.Click, AddressOf buttonRefresh_Click

        FileMenuTemplates.Items.Add(mi)

    End Function

    Function CreateCommands(MenuItem As MenuItem, Folder As String, ByRef i As Integer)


        Dim Dirs = My.Computer.FileSystem.GetDirectories(Folder)
        For Each DirStr In Dirs

            Dim DI = My.Computer.FileSystem.GetFileInfo(DirStr)

            Dim mi = New MenuItem
            mi.Header = DI.Name
            mi.Icon = GetImage("folder")

            MenuItem.Items.Add(mi)

            CreateCommands(mi, Path.Combine(Folder, DirStr), i)

            i = i + 1

        Next

        Dim Files = My.Computer.FileSystem.GetFiles(Folder)

        For Each File In Files

            Dim FI = My.Computer.FileSystem.GetFileInfo(File)


            Dim mi = New MenuItem
            mi.Header = FI.Name
            mi.ToolTip = File
            mi.Icon = GetImage("sql_script")

            AddHandler mi.Click, AddressOf insert_template

            MenuItem.Items.Add(mi)

            i = i + 1

        Next

    End Function


    Function GetImage(Name As String) As System.Windows.Controls.Image

        '' This is the only way I was able to add image into the menu
        Try

            Dim memory As MemoryStream = New MemoryStream()
            My.Resources.ResourceManager.GetObject(Name).Save(memory, ImageFormat.Png)
            memory.Position = 0
            Dim BitmapImage = New BitmapImage()
            BitmapImage.BeginInit()
            BitmapImage.StreamSource = memory
            BitmapImage.CacheOption = BitmapCacheOption.OnLoad
            BitmapImage.EndInit()

            Dim ObjImage = New System.Windows.Controls.Image
            ObjImage.Source = BitmapImage

            Return ObjImage

        Catch ex As Exception

        End Try


    End Function

    Private Sub buttonRefresh_Click(ByVal sender As Object, ByVal e As System.EventArgs)

        AddFiles()

        System.Windows.MessageBox.Show("The template list has been refreshed!")

    End Sub

    Private Sub insert_template(ByVal sender As Object, ByVal e As System.EventArgs)

        Try

            Dim FileName As String = sender.ToolTip

            If My.Computer.FileSystem.FileExists(FileName) Then

                Dim FileContent = My.Computer.FileSystem.ReadAllText(FileName)

                Dim dte As DTE = TryCast(Package.GetGlobalService(GetType(DTE)), DTE)

                If Not dte.ActiveDocument Is Nothing Then

                    Dim selection As TextSelection = DirectCast(dte.ActiveDocument.Selection, TextSelection)

                    selection.Delete()

                    selection.Insert(FileContent.Trim)

                End If
            Else
                System.Windows.MessageBox.Show("File " + FileName + " doesn't exist!")
            End If

        Catch ex As Exception
            System.Windows.MessageBox.Show(ex.Message)
        End Try


    End Sub


    ''' <summary>
    ''' Handles click on the button by displaying a message box.
    ''' </summary>
    ''' <param name="sender">The event sender.</param>
    ''' <param name="e">The event args.</param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")>
    Private Sub button1_Click(ByVal sender As Object, ByVal e As System.EventArgs)


        FormatSelection()


    End Sub

    Function ParseCodeViaTSQLParser(OldCode As String) As String

        Dim ResultCode As String = ""

        Dim SqlParser As TSqlParser = Nothing

        Dim TargetVersion As String = SettingManager.GetSQLParserVersion()
        If TargetVersion = "SQL Server 2008" Then
            SqlParser = New TSql100Parser(False)
        ElseIf TargetVersion = "SQL Server 2012" Then
            SqlParser = New TSql110Parser(False)
        ElseIf TargetVersion = "SQL Server 2014" Then
            SqlParser = New TSql120Parser(False)
        ElseIf TargetVersion = "SQL Server 2016" Then
            SqlParser = New TSql130Parser(False)
        ElseIf TargetVersion = "SQL Server 2017" Then
            SqlParser = New TSql140Parser(False)
        ElseIf TargetVersion = "SQL Server 2019" Then
            SqlParser = New TSql150Parser(False)
        ElseIf TargetVersion = "SQL Server 2022" Then
            SqlParser = New TSql160Parser(False)
        Else
            SqlParser = New TSql150Parser(False)
        End If

        Dim parseErrors As IList(Of ParseError) = New List(Of ParseError)
        Dim result As TSqlFragment = SqlParser.Parse(New StringReader(OldCode), parseErrors)

        If parseErrors.Count > 0 Then

            Dim ErrorStr = ""
            For Each StrError In parseErrors
                ErrorStr = ErrorStr + Environment.NewLine + StrError.Message
            Next

            Throw New System.Exception("TSqlParser unable format selected T-SQL due to a syntax error." + Environment.NewLine + ErrorStr)

        End If

        '---------------------------------------------------------------
        ' This is how you can strip off all comments, but keep the format on
        'Dim resultScript = ""
        'For Each Item In result.ScriptTokenStream
        '    If Item.TokenType = TSqlTokenType.MultilineComment _
        '        Or Item.TokenType = TSqlTokenType.SingleLineComment Then
        '        Continue For
        '    End If
        '    resultScript = resultScript + Item.Text
        'Next
        '---------------------------------------------------------------
        ' Still need to find how to format the script and keep comments in the text 
        '---------------------------------------------------------------

        Dim Gen As SqlScriptGenerator = Nothing

        If TargetVersion = "SQL Server 2008" Then
            Gen = New Sql100ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql100
        ElseIf TargetVersion = "SQL Server 2012" Then
            Gen = New Sql110ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql110
        ElseIf TargetVersion = "SQL Server 2014" Then
            Gen = New Sql120ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql120
        ElseIf TargetVersion = "SQL Server 2016" Then
            Gen = New Sql130ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql130
        ElseIf TargetVersion = "SQL Server 2017" Then
            Gen = New Sql140ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql140
        ElseIf TargetVersion = "SQL Server 2019" Then
            Gen = New Sql150ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql150
        ElseIf TargetVersion = "SQL Server 2022" Then
            Gen = New Sql160ScriptGenerator
            Gen.Options.SqlVersion = SqlVersion.Sql160
        Else
            Gen = New Sql150ScriptGenerator
        End If

        Gen.Options.AlignClauseBodies = False
        Gen.GenerateScript(result, ResultCode)

        Return ResultCode

    End Function

    Private Sub FormatSelection()


        Dim dte As DTE = TryCast(Package.GetGlobalService(GetType(DTE)), DTE)

        If Not dte.ActiveDocument Is Nothing Then

            Dim title = "Format TSQL"

            Try

                Dim selection As TextSelection = DirectCast(dte.ActiveDocument.Selection, TextSelection)

                Dim OldStr = selection.Text

                'nothing is selected
                If String.IsNullOrEmpty(OldStr) Then
                    Return
                End If

                Dim Result As String = ""

                If SettingManager.GetSQLParserType() = False Then
                    Result = ParseCodeViaTSQLParser(OldStr)
                Else
                    Dim FO = New PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions With {
                                       .IndentString = "\t",
                                       .SpacesPerTab = 4,
                                       .MaxLineWidth = 999,
                                       .ExpandCommaLists = True,
                                       .TrailingCommas = True,
                                       .SpaceAfterExpandedComma = False,
                                       .ExpandBooleanExpressions = True,
                                       .ExpandCaseStatements = True,
                                       .ExpandBetweenConditions = False,
                                       .BreakJoinOnSections = False,
                                       .UppercaseKeywords = True,
                                       .HTMLColoring = False,
                                       .KeywordStandardization = True}

                    Dim formatter = New PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(FO)
                    Dim formatMgr = New PoorMansTSqlFormatterLib.SqlFormattingManager(formatter)

                    Result = formatMgr.Format(OldStr)
                End If

                selection.Delete()

                selection.Insert(Result)

            Catch ex As Exception
                System.Windows.MessageBox.Show(ex.Message)
            End Try

        End If

    End Sub

    Private Sub buttonHelp_Click(sender As Object, e As System.Windows.RoutedEventArgs)

        Dim AboutBoxForm = New AboutBox

        AboutBoxForm.ShowDialog()

        AboutBoxForm.Dispose()

    End Sub

    Private Sub buttonSetting_Click(sender As Object, e As System.Windows.RoutedEventArgs)

        Dim SettingFormForm = New SettingForm

        SettingFormForm.ShowDialog()

        SettingFormForm.Dispose()

    End Sub

End Class