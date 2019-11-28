

Imports System
Imports System.Collections.Generic
Imports System.IO
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

        AddFiles()
        ' Add any initialization after the InitializeComponent() call.

    End Sub


    Function AddFiles()



        'ToolBox


    End Function

    ''' <summary>
    ''' Handles click on the button by displaying a message box.
    ''' </summary>
    ''' <param name="sender">The event sender.</param>
    ''' <param name="e">The event args.</param>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")>
    Private Sub button1_Click(ByVal sender As Object, ByVal e As System.EventArgs)


        FormatSelection()


    End Sub

    Private Sub ComboBox_SelectionChanged(sender As Object, e As System.Windows.Controls.SelectionChangedEventArgs)

    End Sub



    Private Sub FormatSelection()


        Dim dte As DTE = TryCast(Package.GetGlobalService(GetType(DTE)), DTE)

        If Not dte.ActiveDocument Is Nothing Then

            Dim title = "Format TSQL"

            Try

                Dim selection As TextSelection = DirectCast(dte.ActiveDocument.Selection, TextSelection)

                Dim OldStr = selection.Text

                Dim SqlParser As TSqlParser = Nothing

                Dim TargetVersion As String = "2019" 'SettingManager.GetTSQLFormatVersion()
                If TargetVersion = "2008" Then
                    SqlParser = New TSql100Parser(False)
                ElseIf TargetVersion = "2012" Then
                    SqlParser = New TSql110Parser(False)
                ElseIf TargetVersion = "2014" Then
                    SqlParser = New TSql120Parser(False)
                ElseIf TargetVersion = "2016" Then
                    SqlParser = New TSql130Parser(False)
                ElseIf TargetVersion = "2017" Then
                    SqlParser = New TSql140Parser(False)
                ElseIf TargetVersion = "2019" Then
                    SqlParser = New TSql150Parser(False)
                Else
                    SqlParser = New TSql140Parser(False)
                End If

                Dim parseErrors As IList(Of ParseError) = New List(Of ParseError)
                Dim result As TSqlFragment = SqlParser.Parse(New StringReader(OldStr), parseErrors)

                If parseErrors.Count > 0 Then

                    Dim ErrorStr = ""
                    For Each StrError In parseErrors
                        ErrorStr = ErrorStr + Environment.NewLine + StrError.Message
                    Next

                    Throw New System.Exception("TSqlParser unable format selected T-SQL due to a syntax error." + Environment.NewLine + ErrorStr)

                End If

                selection.Delete()

                Dim StrAdd2 = ""
                Dim Gen As SqlScriptGenerator = Nothing

                If TargetVersion = "2008" Then
                    Gen = New Sql100ScriptGenerator
                ElseIf TargetVersion = "2012" Then
                    Gen = New Sql110ScriptGenerator
                ElseIf TargetVersion = "2014" Then
                    Gen = New Sql120ScriptGenerator
                ElseIf TargetVersion = "2016" Then
                    Gen = New Sql130ScriptGenerator
                ElseIf TargetVersion = "2017" Then
                    Gen = New Sql140ScriptGenerator
                ElseIf TargetVersion = "2019" Then
                    Gen = New Sql150ScriptGenerator
                Else
                    Gen = New Sql140ScriptGenerator
                End If

                Gen.Options.IncludeSemicolons = False
                Gen.Options.AlignClauseBodies = False
                Gen.GenerateScript(result, StrAdd2)

                selection.Insert(StrAdd2.Trim)

            Catch ex As Exception
                System.Windows.MessageBox.Show(ex.Message)
            End Try


        End If

    End Sub


End Class