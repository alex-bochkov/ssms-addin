''------------------------------------------------------------------------------
'' <copyright file="SSMSAddin.vb" company="Company">
''     Copyright (c) Company.  All rights reserved.
'' </copyright>
''------------------------------------------------------------------------------

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
'Imports EnvDTE
Imports Microsoft.SqlServer.Management.UI.VSIntegration
Imports Microsoft.SqlServer.TransactSql.ScriptDom
Imports Microsoft.VisualStudio.Text.Editor

''' <summary>
''' Margin's canvas and visual definition including both size and content
''' </summary>
Friend NotInheritable Class SSMSAddin
    Inherits Canvas
    Implements IWpfTextViewMargin

    ''' <summary>
    ''' The name of the margin
    ''' </summary>
    Public Const MarginName As String = "SSMSAddin"

    ''' <summary>
    ''' A value indicating whether the object is disposed
    ''' </summary>
    Private isDisposed As Boolean

    Dim textViewRef As IWpfTextView
    Dim MainMenu As New Menu
    Dim TemplatesMenuId = 0
    Private TemplatesList As Hashtable = New Hashtable
    ''' <summary>
    ''' Initializes a new instance of the <see cref="SSMSAddin"/> class for a given <paramref name="textView"/>.
    ''' </summary>
    ''' <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
    Public Sub New(ByVal textView As IWpfTextView)

        textViewRef = textView

        ' Margin height sufficient to have the label.
        Me.Height = 16
        'Me.ClipToBounds = True
        'Me.Background = New SolidColorBrush(Colors.LightGreen)

        '' Add a green colored label that says "Hello SSMSAddin"
        'Dim label As New Label With {
        '    .Background = New SolidColorBrush(Colors.LightGreen),
        '    .Content = "Hello SSMSAddin"
        '}
        'Me.Children.Add(label)

        'Dim MainToolBar = New ToolBar


        'Dim buttonF As New Button With {.Content = "Format"}
        'AddHandler buttonF.Click, AddressOf ButtonFormat_Click
        'MainToolBar.Items.Add(buttonF)

        'Dim buttonE As New Button With {.Content = "ExportToExcel"}
        'AddHandler buttonE.Click, AddressOf ButtonExportToExcel_Click
        'MainToolBar.Items.Add(buttonE)

        'Dim SubMenu As New Menu

        'Dim ObjImage As Image = New Image()
        'Dim Path = "C:\Users\Alex\Documents\Visual Studio 2015\Projects\VSIXProject1\VSIXProject1\Resources\settings-icon.png" 'My.Application.Info.DirectoryPath() + "\Resources\setting-icon.png"
        'ObjImage.Source = New BitmapImage(New Uri(Path, UriKind.RelativeOrAbsolute))

        Dim SubMenuItem1 As New MenuItem With {.Header = "Settings"}
        AddHandler SubMenuItem1.Click, AddressOf ButtonSettings_Click
        'SubMenuItem1.Icon = ObjImage

        Dim SubMenuItem2 As New MenuItem With {.Header = "Format SQL"}
        AddHandler SubMenuItem2.Click, AddressOf ButtonFormat_Click

        Dim SubMenuItem3 As New MenuItem With {.Header = "Export To Excel"}
        AddHandler SubMenuItem3.Click, AddressOf ButtonExportToExcel_Click

        Dim SubMenuItem4 As New MenuItem With {.Header = "Templates"}
        '''SubMenuItem4.Icon = New BitmapImage(New Uri("resources/sql-icon.png", UriKind.Relative))

        MainMenu = New Menu
        MainMenu.IsMainMenu = True
        MainMenu.Items.Add(SubMenuItem2)
        MainMenu.Items.Add(SubMenuItem3)
        TemplatesMenuId = MainMenu.Items.Add(SubMenuItem4)
        MainMenu.Items.Add(SubMenuItem1)

        Me.Children.Add(MainMenu)

        RefreshTemplatesList()

    End Sub

    Private Sub ButtonRefreshTemplates_Click(sender As Object, e As System.EventArgs)

        RefreshTemplatesList()

    End Sub

    Sub RefreshTemplatesList()

        Dim MenuTemplates As MenuItem = MainMenu.Items(TemplatesMenuId)
        MenuTemplates.Items.Clear()

        Dim TemplatesList = New Hashtable
        Dim Folder = SettingManager.GetTemplatesFolder()

        If Not String.IsNullOrEmpty(Folder) Then
            Dim i = 1
            FillTemplates(MenuTemplates, Folder, i)
            MenuTemplates.Header = "Templates (" + i.ToString + ")"
        End If

        MenuTemplates.Items.Add(New Separator)
        Dim SubMenuItem41 As New MenuItem With {.Header = "Refresh templates list"}
        AddHandler SubMenuItem41.Click, AddressOf ButtonRefreshTemplates_Click
        MenuTemplates.Items.Add(SubMenuItem41)

    End Sub


    Sub FillTemplates(ByRef SubMenuItem As MenuItem, Folder As String, ByRef i As Integer)

        Dim Dirs = My.Computer.FileSystem.GetDirectories(Folder)

        For Each DirStr In Dirs

            Dim DI = My.Computer.FileSystem.GetFileInfo(DirStr)

            Dim NewSubMenuItem As New MenuItem
            NewSubMenuItem.Header = DI.Name
            SubMenuItem.Items.Add(NewSubMenuItem)

            FillTemplates(NewSubMenuItem, DirStr, i)

            i = i + 1
        Next

        Dim Files = My.Computer.FileSystem.GetFiles(Folder)

        For Each File In Files

            Dim FI = My.Computer.FileSystem.GetFileInfo(File)

            Dim NewSubMenuItem As New MenuItem
            NewSubMenuItem.Header = FI.Name
            NewSubMenuItem.Tag = FI.FullName

            AddHandler NewSubMenuItem.Click, AddressOf ButtonTemplates_Click

            Dim id = SubMenuItem.Items.Add(NewSubMenuItem)

            'TemplatesList.Add(id, FI.FullName)

            i = i + 1

        Next

    End Sub

    Private Sub ButtonSettings_Click(sender As Object, e As System.EventArgs)

        Dim S = New SettingFormUI
        S.ShowDialog()


    End Sub

    Private Sub ButtonFormat_Click(sender As Object, e As System.EventArgs)

        Dim title = "Format TSQL"

        Try

            Dim Spans = textViewRef.Selection.SelectedSpans
            If Spans.Count > 1 Then
                System.Windows.MessageBox.Show("Multu-span selection is not supported")
                Return
            ElseIf Spans.Count = 0 Then
                Return
            End If

            Dim Span = Spans.Item(0)
            Dim StartPosition = Span.Start.Position

            Dim OldStr = textViewRef.TextSnapshot.GetText(Span)

            Dim SqlParser As TSqlParser = Nothing

            Dim TargetVersion As String = SettingManager.GetTSQLFormatVersion()
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
            Else
                SqlParser = New TSql130Parser(False)
            End If

            Dim parseErrors As IList(Of ParseError) = New List(Of ParseError)
            Dim result As TSqlFragment = SqlParser.Parse(New StringReader(OldStr), parseErrors)

            If parseErrors.Count > 0 Then

                Dim ErrorStr = ""
                For Each StrError In parseErrors
                    ErrorStr = ErrorStr + Environment.NewLine + StrError.Message
                Next

                Throw New System.Exception("TSql120Parser unable format selected T-SQL due to a syntax error." + Environment.NewLine + ErrorStr)

            End If

            textViewRef.TextBuffer.Delete(Span)

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
            Else
                Gen = New Sql130ScriptGenerator
            End If

            Gen.Options.IncludeSemicolons = False
            Gen.Options.AlignClauseBodies = False
            Gen.GenerateScript(result, StrAdd2)

            textViewRef.TextBuffer.Insert(StartPosition, StrAdd2.Trim)

        Catch ex As Exception
            System.Windows.MessageBox.Show(ex.Message)
        End Try



    End Sub

    Private Sub ButtonExportToExcel_Click(sender As Object, e As System.EventArgs)

        System.Windows.MessageBox.Show("Click ButtonExportToExcel_Click")

    End Sub

    Private Sub ButtonTemplates_Click(sender As Object, e As System.EventArgs)

        Dim Tag As String = sender.Tag

        If Not String.IsNullOrEmpty(Tag) Then

            Dim Text = My.Computer.FileSystem.ReadAllText(Tag)

            If Not String.IsNullOrEmpty(Text) Then

                'Dim i = textViewRef.Selection.Start.Position.
                'Dim j = textViewRef.Selection.End.Position.Position
                'textViewRef.TextBuffer.Delete()
                'If Not txt.Selection.IsEmpty Then txt.Selection.Delete()

                Dim Spans = textViewRef.Selection.SelectedSpans
                For Each Span In Spans
                    textViewRef.TextBuffer.Delete(Span)
                Next

                Dim i = textViewRef.Selection.ActivePoint.Position.Position

                textViewRef.TextBuffer.Insert(i, Text)

            End If

        End If


    End Sub

    Private Sub SubMenuItem_Click(sender As Object, e As System.EventArgs)

        System.Windows.MessageBox.Show("Click SubMenuItem_Click")

    End Sub

#Region "IWpfTextViewMargin"

    ''' <summary>
    ''' The <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation
    ''' of the margin.
    ''' </summary>
    Public ReadOnly Property VisualElement() As FrameworkElement Implements IWpfTextViewMargin.VisualElement
        ' Since this margin implements Canvas, this is the object which renders
        ' the margin.
        Get

            ThrowIfDisposed()
            Return Me

        End Get
    End Property

#End Region

#Region "ITextViewMargin"

    ''' <summary>
    ''' Gets the size of the margin.
    ''' </summary>
    ''' <remarks>
    ''' For a horizontal margin this Is the height of the margin,
    ''' since the width will be determined by the <see cref="ITextView"/>.
    ''' For a vertical margin this Is the width of the margin,
    ''' since the height will be determined by the <see cref="ITextView"/>.
    ''' </remarks>
    ''' <exception cref="ObjectDisposedException">The margin Is disposed.</exception>
    Public ReadOnly Property MarginSize() As Double Implements IWpfTextViewMargin.MarginSize
        ' Since this is a horizontal margin, its width will be bound to the width of the text view.
        ' Therefore, its size is its height.
        Get

            ThrowIfDisposed()
            Return Me.ActualHeight

        End Get
    End Property

    ''' <summary>
    ''' Gets a value indicating whether the margin Is enabled.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">The margin Is disposed.</exception>
    Public ReadOnly Property Enabled() As Boolean Implements IWpfTextViewMargin.Enabled
        ' The margin should always be enabled
        Get

            ThrowIfDisposed()
            Return True

        End Get
    End Property

    ''' <summary>
    ''' Returns an instance of the margin if this is the margin that has been requested or null if no match is found
    ''' </summary>
    ''' <param name="marginName">The name of the margin requested</param>
    ''' <returns>An instance of SSMSAddin or null</returns>
    Public Function GetTextViewMargin(ByVal marginName As String) As ITextViewMargin Implements IWpfTextViewMargin.GetTextViewMargin

        Return If(String.Equals(marginName, SSMSAddin.MarginName, StringComparison.OrdinalIgnoreCase), Me, Nothing)

    End Function

    ''' <summary>
    ''' Disposes an instance of <see cref="SSMSAddin"/> class.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose

        If Not Me.isDisposed Then
            GC.SuppressFinalize(Me)
            Me.isDisposed = True
        End If

    End Sub

#End Region

    ''' <summary>
    ''' Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
    ''' </summary>
    Private Sub ThrowIfDisposed()

        If Me.isDisposed Then
            Throw New ObjectDisposedException(MarginName)
        End If

    End Sub
End Class
